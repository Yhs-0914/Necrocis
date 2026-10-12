using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrocis;
using ProceduralMap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    // Exercises the interaction between main's environmental damage and definition-based enemies.
    public static class MergeHazardRegressionRunner
    {
        private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        private const string Report = "Logs/MergeHazardRegression-results.txt";
        private static readonly List<string> results = new List<string>();
        private static readonly List<EnemyController> actors = new List<EnemyController>();
        private static double deadline;
        private static bool started, passed;
        private static bool previousOptionsEnabled;
        private static EnterPlayModeOptions previousOptions;

        public static void Run()
        {
            if (!Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Run in an isolated batch project in Edit Mode.");
            results.Clear(); actors.Clear(); started = passed = false;
            MergeIntegrationChecks.Run();
            EliteIntegrationAudit.Run();
            FinalBossSmokeRunner.VerifyExistingOrganMaps();
            Pass("Existing monster definitions, elite configuration, final boss balance and four organ maps validated");
            SaveService.UseStorageRootForTests(Path.GetFullPath("Library/MergeHazardSave-" + Guid.NewGuid().ToString("N")));
            Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error), error);
            previousOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            previousOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            EditorApplication.playModeStateChanged += OnPlayMode;
            EditorApplication.update += Tick;
            deadline = EditorApplication.timeSinceStartup + 180;
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Hub.unity");
            EditorApplication.isPlaying = true;
        }

        private static void Tick()
        {
            if (EditorApplication.timeSinceStartup > deadline)
            {
                deadline = double.PositiveInfinity;
                Fail(new TimeoutException("Merge hazard regression timeout"));
                return;
            }
            if (started || !EditorApplication.isPlaying || PlayerController.Instance?.HealthComponent == null) return;
            started = true;
            Application.runInBackground = true;
            Time.timeScale = 1f;
            PlayerController.Instance.StartCoroutine(Guard(Checks()));
        }

        private static IEnumerator Guard(IEnumerator checks)
        {
            while (true)
            {
                object next;
                try { if (!checks.MoveNext()) break; next = checks.Current; }
                catch (Exception error) { Fail(error); yield break; }
                yield return next;
            }
            passed = true;
            EditorApplication.isPlaying = false;
        }

        private static IEnumerator Checks()
        {
            yield return null;
            PlayerController player = PlayerController.Instance;
            player.GetComponent<CharacterStats>().SetBaseStat(CharacterStatType.MaxHealth, 1000, true);
            EnemySpawnConfig spawns = AssetDatabase.LoadAssetAtPath<EnemySpawnConfig>(
                "Assets/_Project/Data/BiomeConfigs/StomachEnemySpawnConfig.asset");
            EnemySpawnRuleConfig normalRule = spawns.GetEnemySpawnRules().First(r => !r.isElite && !r.isRanged);
            Require(normalRule.monsterDefinition != null, "Normal rule has a monster definition");
            Vector3 position = player.transform.position + Vector3.right * 20f;
            int awarded = 0;
            Action<int> observe = amount => awarded += amount;
            LevelUpManager.OnExpGained += observe;
            try
            {
                EnemyController untouched = Spawn(normalRule, position);
                int defeatEvents = 0, playerHits = 0;
                untouched.Defeated += _ => defeatEvents++;
                untouched.DamageTaken += (_, amount) => playerHits++;
                untouched.TakeFloodDamage(1000);
                untouched.GrantExp(); untouched.GrantExp();
                Require(untouched.IsDead && defeatEvents == 1 && playerHits == 0 && awarded == 0,
                    "Environmental death reports defeat once without player hit or XP credit");
                Pass("Untouched flood kill: one defeat event, zero player damage events, zero XP");

                EnemyController assisted = Spawn(normalRule, position);
                int expected = assisted.Balance.Current.Experience;
                Require(expected > 0, "Fixture XP must be positive");
                assisted.TakeDamage(1);
                assisted.TakeFloodDamage(1000);
                assisted.GrantExp(); assisted.GrantExp();
                Require(awarded == expected, "Player-assisted flood death awards resolved XP exactly once");
                Pass("Player-assisted flood kill awards definition-based XP exactly once");

                EnemyController suppressed = Spawn(normalRule, position);
                suppressed.SuppressExperienceReward = true;
                suppressed.TakeDamage(1000);
                suppressed.GrantExp();
                Require(awarded == expected, "Encounter XP suppression survives the merge");
                Pass("Explicit encounter XP suppression retained");

                EnemyController fractional = Spawn(normalRule, position);
                fractional.TakeFloodDamage(.75f);
                Require(fractional.Stats.CurrentHealth == 100, "Sub-unit flood damage accumulates before application");
                fractional.ReleaseToPool();
                EnemyController reused = EnemyController.Acquire(null, "MergeHazardReused", EnemyController.GetPoolArchetypeId(normalRule));
                Require(reused == fractional, "Fixture reuses the same pooled object");
                actors.Add(reused);
                // Final boss summons call this overload; it must also reset flood state.
                reused.Configure(null, normalRule, position, position, MonsterBalanceRuntime.Capture(normalRule.monsterDefinition));
                reused.SetAiSuppressed(true);
                reused.Stats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
                reused.TakeFloodDamage(.5f);
                Require(reused.Stats.CurrentHealth == 100, "Summon overload resets pending fractional flood damage");
                reused.TakeFloodDamage(.5f);
                Require(reused.Stats.CurrentHealth == 99, "Fractional flood damage reaches exactly one health unit");
                reused.SetIgnoreMidBossArenaRestriction(true);
                float bossHealth = reused.Stats.CurrentHealth;
                reused.TakeFloodDamage(1000);
                Require(reused.Stats.CurrentHealth == bossHealth, "Boss encounter is immune to flood damage");
                Pass("Pooled summon resets fractional flood damage; accumulation and boss immunity retained");

                untouched.ReleaseToPool();
                untouched.Configure(null, normalRule, position, position);
                untouched.SetAiSuppressed(true);
                untouched.Stats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
                untouched.TakeDamage(1000); untouched.GrantExp(); untouched.GrantExp();
                Require(awarded == expected + untouched.Balance.Current.Experience,
                    "Pool reuse resets kill-credit suppression and XP guard");
                Pass("Environmental kill followed by pool reuse restores normal player XP credit");
            }
            finally { LevelUpManager.OnExpGained -= observe; }

            foreach (EnemyController enemy in actors.Distinct().ToArray())
                if (enemy != null && enemy.Config != null) enemy.ReleaseToPool();
            actors.Clear();
            SceneLoader.Instance.LoadBiome(BiomeType.Stomach);
            while (SceneManager.GetActiveScene().name != "Stomach" || Object.FindFirstObjectByType<MapGenerator>()?.IsReady != true)
                yield return null;
            yield return null;
            MapGenerator map = Object.FindFirstObjectByType<MapGenerator>();
            StomachAcidCycle cycle = Object.FindFirstObjectByType<StomachAcidCycle>();
            Require(cycle != null && Object.FindObjectsByType<StomachAcidCycle>(FindObjectsSortMode.None).Length == 1,
                "Actual stomach scene has exactly one acid cycle");
            cycle.enabled = false;
            Set(cycle, "floodMap", map);
            Vector3 low = FindFloor(map, false), high = FindFloor(map, true);
            Require(map.IsPlayerInAcid(low, true) && !map.IsPlayerInAcid(high, true), "Flood affects ground floor only");
            Require(map.TryGetClearTraversalLanding(low, Vector3.right, new Vector2(.3f, .3f), out Vector3 landing),
                "Clear traversal landing works after authored atlas changes");
            Require(map.GetHeightLevelAtWorld(landing) == 0, "Landing remains on the destination floor");

            BiomeEliteSpawnConfig eliteConfig = AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>(
                "Assets/_Project/Data/BiomeElites/StomachBiomeEliteSpawnConfig.asset");
            EnemyController normal = Spawn(normalRule, low);
            EnemyController elite = Spawn(eliteConfig.monsters[0], low);
            EnemyController sheltered = Spawn(normalRule, high);
            EnemyController boss = Spawn(normalRule, low);
            boss.SetIgnoreMidBossArenaRestriction(true);
            boss.Stats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
            Invoke(cycle, "ApplyEnemyFloodDamage", 10f);
            Require(normal.Stats.CurrentHealth == 70 && elite.Stats.CurrentHealth == 90
                && sheltered.Stats.CurrentHealth == 100 && boss.Stats.CurrentHealth == 100,
                "Actual flood uses normal/elite fractions and excludes elevated enemies and bosses");
            Pass("Actual stomach flood: normal -30%, definition-based elite -10%, elevated enemy/boss unharmed");

            player.SpawnAt(low);
            player.HealthComponent.ResetHealth();
            Set(cycle, "acidLevel", 1f); Set(cycle, "nextDamageTime", 0f);
            float before = player.HealthComponent.CurrentHealth;
            Invoke(cycle, "ApplyAcidDamage");
            Require(player.HealthComponent.CurrentHealth < before, "Actual acid damages player on low floor");
            player.SpawnAt(high);
            player.HealthComponent.ResetHealth();
            Set(cycle, "nextDamageTime", 0f);
            before = player.HealthComponent.CurrentHealth;
            Invoke(cycle, "ApplyAcidDamage");
            Require(player.HealthComponent.CurrentHealth == before, "High floor protects player");
            Pass("Actual acid damage respects player elevation; traversal landing retains clearance");

            Set(cycle, "warningDuration", .1f); Set(cycle, "riseDuration", .1f);
            Set(cycle, "floodDuration", .2f); Set(cycle, "drainDuration", .1f);
            cycle.enabled = true;
            cycle.AddGauge(100f);
            Require(cycle.CurrentPhase == StomachAcidCycle.Phase.Warning, "Full gauge starts warning");
            bool sawRising = false, sawFlooded = false, sawDraining = false;
            float until = Time.time + 3f;
            while (Time.time < until)
            {
                sawRising |= cycle.CurrentPhase == StomachAcidCycle.Phase.Rising;
                sawFlooded |= cycle.CurrentPhase == StomachAcidCycle.Phase.Flooded;
                sawDraining |= cycle.CurrentPhase == StomachAcidCycle.Phase.Draining;
                if (sawDraining && cycle.CurrentPhase == StomachAcidCycle.Phase.Charging) break;
                yield return null;
            }
            Require(sawRising && sawFlooded && sawDraining && cycle.CurrentPhase == StomachAcidCycle.Phase.Charging
                && cycle.AcidLevel == 0f, "Acid cycle completes and drains the visual layer");
            Pass("Real Update cycle: warning, rising, flooded, draining, recharge with zero acid level");
            SceneLoader.Instance.ReturnToHub();
            while (SceneManager.GetActiveScene().name != "Hub") yield return null;
            Require(Object.FindFirstObjectByType<StomachAcidCycle>() == null, "Acid controller does not leak into Hub");
            Pass("Stomach exit removes acid controller and HUD");
        }

        private static EnemyController Spawn(EnemySpawnRuleConfig rule, Vector3 position)
        {
            EnemyController enemy = EnemyController.Acquire(null, "MergeHazardFixture", EnemyController.GetPoolArchetypeId(rule));
            enemy.Configure(null, rule, position, position);
            enemy.SetAiSuppressed(true);
            enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
            actors.Add(enemy);
            return enemy;
        }

        private static Vector3 FindFloor(MapGenerator map, bool elevated)
        {
            for (int y = 10; y < map.MapHeight - 10; y++)
            for (int x = 10; x < map.MapWidth - 10; x++)
            {
                var cell = map.Data.GetCell(x, y);
                if (cell.IsVoid || cell.HasCliff || cell.HasLava || cell.HasStomachRock
                    || (elevated ? cell.HeightLevel < 1 : cell.HeightLevel != 0)) continue;
                Vector3 point = map.GetCellCenterWorld(x, y);
                if (map.TryGetClearTraversalLanding(point, Vector3.right, new Vector2(.3f, .3f), out Vector3 clear)) return clear;
            }
            throw new InvalidOperationException("Missing floor fixture: elevated=" + elevated);
        }

        private static void Set(object target, string name, object value) => target.GetType().GetField(name, Hidden).SetValue(target, value);
        private static void Invoke(object target, string name, params object[] args) => target.GetType().GetMethod(name, Hidden).Invoke(target, args);
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void Pass(string message) { results.Add("PASS " + message); Debug.Log("[MergeHazard] PASS " + message); }
        private static void Fail(Exception error)
        {
            passed = false; results.Add("FAIL " + error); Debug.LogException(error);
            EditorApplication.isPlaying = false;
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayMode;
            EditorSettings.enterPlayModeOptionsEnabled = previousOptionsEnabled;
            EditorSettings.enterPlayModeOptions = previousOptions;
            SaveService.ResetStaticStateForTests();
            DifficultyBalanceService.ResetForTests();
            Directory.CreateDirectory("Logs"); File.WriteAllLines(Report, results);
            EditorApplication.Exit(passed ? 0 : 1);
        }
    }
}
