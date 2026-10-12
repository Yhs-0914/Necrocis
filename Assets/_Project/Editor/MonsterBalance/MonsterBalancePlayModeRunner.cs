using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    // Runs real combat components in Hub Play Mode. Asset changes use SerializedObject like the Inspector.
    public static class MonsterBalancePlayModeRunner
    {
        private static readonly Dictionary<Object, string> backups = new Dictionary<Object, string>();
        private static readonly List<string> results = new List<string>();
        private static readonly List<EnemyController> actors = new List<EnemyController>();
        private static MonsterBalanceCatalog catalog;
        private static PlayerController player;
        private static Health health;
        private static string storage;
        private static bool previousOptionsEnabled, started, passed;
        private static EnterPlayModeOptions previousOptions;
        private static double deadline;
        private static float previousTimeScale;
        private static int enteredFrame;
        private const string Report = "Logs/MonsterBalance-C02-results.txt";

        [MenuItem("Necrocis/Balance/Run C-02 Play Mode Checks")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            catalog = AssetDatabase.LoadAssetAtPath<MonsterBalanceCatalog>(MonsterBalanceSetup.CatalogPath);
            Require(catalog != null && catalog.GetValidationErrors().Count == 0, "Catalog validation");
            results.Clear(); backups.Clear(); actors.Clear(); started = false; passed = false;
            storage = Path.Combine(Path.GetTempPath(), "necrocis-balance-c02-" + Guid.NewGuid().ToString("N"));
            SaveService.UseStorageRootForTests(storage);
            Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error), error);
            previousTimeScale = Time.timeScale;
            previousOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            previousOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = previousOptions | EnterPlayModeOptions.DisableDomainReload;
            deadline = EditorApplication.timeSinceStartup + 180;
            EditorApplication.playModeStateChanged += OnPlayMode;
            EditorApplication.update += Tick;
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Hub.unity", OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode) enteredFrame = Time.frameCount;
            if (state != PlayModeStateChange.EnteredEditMode) return;
            RestoreAssets();
            Time.timeScale = previousTimeScale;
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayMode;
            EditorSettings.enterPlayModeOptionsEnabled = previousOptionsEnabled;
            EditorSettings.enterPlayModeOptions = previousOptions;
            SaveService.ResetStaticStateForTests();
            DifficultyBalanceService.ResetForTests();
            if (Directory.Exists(storage)) Directory.Delete(storage, true);
            Directory.CreateDirectory("Logs");
            File.WriteAllLines(Report, results);
            Debug.Log("[MonsterBalance-C02] " + (passed ? "ALL PASS" : "FAIL") + ": " + Report);
            if (Application.isBatchMode) EditorApplication.Exit(passed ? 0 : 1);
        }

        private static void Tick()
        {
            if (EditorApplication.timeSinceStartup > deadline)
            {
                Fail(new TimeoutException("Play Mode check timeout"));
                deadline = double.PositiveInfinity;
                return;
            }
            if (started || !EditorApplication.isPlaying || Time.frameCount - enteredFrame < 12) return;
            player = PlayerController.Instance;
            if (player == null || player.HealthComponent == null) return;
            health = player.HealthComponent;
            started = true;
            Time.timeScale = 1f;
            player.StartCoroutine(Guard(Checks()));
        }

        private static IEnumerator Guard(IEnumerator checks)
        {
            while (true)
            {
                object step;
                try
                {
                    if (!checks.MoveNext()) break;
                    step = checks.Current;
                }
                catch (Exception error) { Fail(error); yield break; }
                yield return step;
            }
            passed = true;
            RestoreAssets();
            EditorApplication.isPlaying = false;
        }

        private static void Fail(Exception error)
        {
            passed = false;
            results.Add("FAIL " + error);
            Debug.LogError("[MonsterBalance-C02] " + error);
            RestoreAssets();
            EditorApplication.isPlaying = false;
        }

        private static IEnumerator Checks()
        {
            player.GetComponent<CharacterStats>().SetBaseStat(CharacterStatType.MaxHealth, 1000, true);
            EnemySpawnConfig spawns = AssetDatabase.LoadAssetAtPath<EnemySpawnConfig>("Assets/_Project/Data/BiomeConfigs/IntestineEnemySpawnConfig.asset");
            EnemySpawnRuleConfig normalRule = spawns.enemySpawnRules.First(r => !r.isElite && !r.isRanged);
            MonsterDefinition normal = normalRule.monsterDefinition;
            var duplicateRule = new EnemySpawnRuleConfig
            {
                name = "DuplicateBalanceGuard", monsterDefinition = normal,
                additionalBaseStats = new List<CharacterStatValue> { new CharacterStatValue(CharacterStatType.MaxHealth, 999) }
            };
            var invalid = new GameObject("DuplicateBalanceGuard").AddComponent<EnemyController>();
            bool duplicateRejected = false;
            try { invalid.Configure(null, duplicateRule, Vector3.zero, Vector3.zero); }
            catch (InvalidOperationException) { duplicateRejected = true; }
            finally { Object.Destroy(invalid.gameObject); }
            Require(duplicateRejected, "duplicate base stats must fail before activating the actor");
            Set(normal, "statSets.Array.data[0].maxHealth", 40);
            Set(normal, "statSets.Array.data[0].attackPower", 4);
            Set(normal, "statSets.Array.data[0].moveSpeed", 2);
            Set(normal, "reward.baseExperience", 7);
            Set(normal, "contact.damageCoefficient", .5f);
            Set(catalog.difficultyCatalog.normal, "enemies.maxHealth", 1.5f);
            Set(catalog.difficultyCatalog.normal, "enemies.outgoingDamage", 2);
            Set(catalog.difficultyCatalog.normal, "enemies.moveSpeed", 1.25f);
            Set(catalog.difficultyCatalog.normal, "enemies.experienceReward", 2);
            Set(catalog.difficultyCatalog.normal, "enemies.attackCooldown", .5f);
            EnemyController enemy = Spawn(normalRule);
            Require(!ReferenceEquals(enemy.Config, normalRule), "spawn scalar settings copied from source");
            Equal(60, enemy.Stats.MaxHealth, "Normal HP 40 × 1.5");
            Equal(8, enemy.Stats.AttackPower, "Normal attack 4 × 2");
            Equal(2.5f, enemy.Stats.MoveSpeed, "Normal speed 2 × 1.25");
            Equal(normalRule.attackCooldown * .5f, enemy.GetRearmCooldown(normalRule.attackCooldown), "Normal cooldown");
            Hit(8, () => Invoke(enemy, "ApplyDamageToPlayer"), "actual normal melee, no double damage factor");
            Hit(4, () => Require(enemy.GetComponent<EnemyContactDamage>().TryApplyTo(player), "contact applied"), "actual contact");
            int awarded = 0;
            Action<int> observe = xp => awarded += xp;
            LevelUpManager.OnExpGained += observe;
            try { enemy.TakeDamage(10000); enemy.GrantExp(); enemy.GrantExp(); }
            finally { LevelUpManager.OnExpGained -= observe; }
            Equal(14, awarded, "real death XP once (7 × 2), repeated callbacks ignored");
            Pass("Inspector → normal spawn HP 60 / attack 8 / speed 2.5 / melee 8 / contact 4 / XP 14 once");

            // Editing assets affects a new spawn; pooled generations capture again.
            EnemyController old = Spawn(normalRule);
            Set(normal, "statSets.Array.data[0].maxHealth", 50);
            Set(catalog.difficultyCatalog.normal, "enemies.outgoingDamage", 3);
            Equal(60, old.Stats.MaxHealth, "existing HP frozen");
            Equal(8, old.CreateAttackDamage(old.Stats.AttackPower).Amount, "existing attack frozen");
            EnemyDamageRequest oldShot = old.CreateAttackDamage(old.Stats.AttackPower);
            old.ReleaseToPool();
            EnemyController reused = Spawn(normalRule);
            Equal(75, reused.Stats.MaxHealth, "new HP 50 × 1.5");
            Equal(12, reused.Stats.AttackPower, "new damage 4 × 3");
            Require(oldShot.Source == null, "pooled owner generation must not alias a new enemy");
            Hit(8, () => player.TakeDamage(oldShot), "in-flight snapshot survives owner reuse");
            Pass("Asset edit → new/pooled spawn HP 75 / damage 12; existing snapshot remains 60 / 8");

            EnemySpawnRuleConfig eliteRule = spawns.enemySpawnRules.First(r => r.isElite);
            Set(eliteRule.monsterDefinition, "statSets.Array.data[0].maxHealth", 30);
            Set(eliteRule.monsterDefinition, "statSets.Array.data[0].attackPower", 4);
            Set(catalog.difficultyCatalog.normal, "elites.maxHealth", 2);
            Set(catalog.difficultyCatalog.normal, "elites.outgoingDamage", 4);
            EnemyController elite = Spawn(eliteRule);
            Equal(60, elite.Stats.MaxHealth, "elite tier HP");
            Hit(16, () => Invoke(elite, "ApplyDamageToPlayer"), "elite row only, no normal factor");
            Pass("Legacy elite uses Elite row only: HP 60 / damage 16");

            SaveService.MarkFinalBossDefeated(); // Unlock only inside the isolated temporary save.
            Require(SaveService.TryBeginNewGame(GameDifficulty.Hard, out string error), error);
            Set(catalog.difficultyCatalog.hard, "enemies.maxHealth", 2);
            Set(catalog.difficultyCatalog.hard, "enemies.outgoingDamage", 5);
            EnemyController hard = Spawn(normalRule);
            Equal(100, hard.Stats.MaxHealth, "Hard HP");
            Hit(20, () => Invoke(hard, "ApplyDamageToPlayer"), "Hard melee once");
            Equal(12, reused.Stats.AttackPower, "Normal spawn unaffected by Hard session");
            Pass("Hard new spawn HP 100 / melee 20; Normal actor retains captured values");

            // The real projectile Update performs collision and applies the captured request.
            health.ResetHealth();
            float before = health.CurrentHealth;
            EnemyProjectile projectile = EnemyProjectile.Acquire(player.transform.position, null, Vector3.one * .5f);
            projectile.Launch(Vector3.forward, hard.Stats.AttackPower, 0, 5, hard);
            Set(catalog.difficultyCatalog.hard, "enemies.outgoingDamage", 9);
            yield return null;
            yield return null;
            Equal(20, before - health.CurrentHealth, "actual projectile update hit uses launch snapshot");
            Pass("Actual projectile collision: damage 20 remains after profile is changed to ×9");

            Set(catalog.difficultyCatalog.hard, "bosses.maxHealth", 2);
            Set(catalog.difficultyCatalog.hard, "bosses.outgoingDamage", 3);
            Set(catalog.difficultyCatalog.hard, "bosses.moveSpeed", 1.5f);
            Set(catalog.difficultyCatalog.hard, "bosses.attackCooldown", .5f);
            foreach (string biome in new[] { "Intestine", "Liver", "Stomach", "Lung" })
            {
                BossArenaConfig arena = AssetDatabase.LoadAssetAtPath<BossArenaConfig>("Assets/_Project/Data/BiomeConfigs/" + biome + "BossArenaConfig.asset");
                MidBossDefinition bossDefinition = arena.midBossArena.boss;
                MonsterDefinition definition = bossDefinition.bossRule.monsterDefinition;
                Set(definition, "statSets.Array.data[0].maxHealth", 40);
                Set(definition, "statSets.Array.data[0].attackPower", 4);
                Set(definition, "statSets.Array.data[0].moveSpeed", 2);
                Set(definition, "statSets.Array.data[1].maxHealth", 80);
                Set(definition, "statSets.Array.data[1].attackPower", 8);
                Set(definition, "statSets.Array.data[1].moveSpeed", 3);
                if (biome == "Lung")
                {
                    Set(definition, "reward.baseExperience", 7);
                    Set(catalog.difficultyCatalog.hard, "bosses.experienceReward", 2);
                }
                EnemyController boss = Spawn(bossDefinition.bossRule);
                boss.SetIgnoreMidBossArenaRestriction(true);
                MonoBehaviour pattern;
                switch (biome)
                {
                    case "Intestine":
                        var intestine = boss.gameObject.AddComponent<IntestineBossPattern>();
                        intestine.Initialize(boss, boss.transform.position, null, bossDefinition.intestinePattern); pattern = intestine; break;
                    case "Liver":
                        var liver = boss.gameObject.AddComponent<LiverBossPattern>();
                        liver.Initialize(boss, boss.transform.position, null, bossDefinition.liverPattern); pattern = liver; break;
                    case "Stomach":
                        var stomach = boss.gameObject.AddComponent<StomachBossPattern>();
                        stomach.Initialize(boss, boss.transform.position, null, bossDefinition.stomachPattern); pattern = stomach; break;
                    default:
                        var lung = new GameObject("LungBalanceTestEncounter").AddComponent<LungBossPattern>();
                        lung.Initialize(boss, boss.transform.position, null, bossDefinition.lungPattern); pattern = lung; break;
                }
                Equal(80, boss.Stats.MaxHealth, biome + " init HP");
                Equal(12, boss.Stats.AttackPower, biome + " init damage");
                Equal(3, boss.Stats.MoveSpeed, biome + " init speed");
                // Even phase 2 is frozen at spawn, before editing this source again.
                Set(definition, "statSets.Array.data[1].attackPower", 99);
                if (pattern is IntestineBossPattern i) i.ForcePhase2ForDebug();
                else if (pattern is LiverBossPattern l) l.ForcePhase2ForDebug();
                else if (pattern is StomachBossPattern s) s.ForcePhase2ForDebug();
                else
                {
                    var lung = (LungBossPattern)pattern;
                    EnemyController survivor = null;
                    lung.ForEachEncounterBoss(e => { if (e != boss) survivor = e; });
                    int firstReward = 0;
                    Action<int> record = xp => firstReward += xp;
                    LevelUpManager.OnExpGained += record;
                    try { boss.TakeDamage(10000); }
                    finally { LevelUpManager.OnExpGained -= record; }
                    Equal(0, firstReward, "first lung brother must not grant encounter XP");
                    Require(lung.CurrentPhaseName == "Phase2", "actual brother death changes lung phase");
                    boss = survivor;
                    actors.Add(boss);
                }
                Equal(160, boss.Stats.MaxHealth, biome + " phase2 HP");
                Equal(24, boss.Stats.AttackPower, biome + " phase2 damage remains captured 8 × 3");
                Equal(4.5f, boss.Stats.MoveSpeed, biome + " phase2 speed");
                Equal(2, boss.GetRearmCooldown(4), biome + " cooldown once");
                foreach (MonsterPatternDamage action in definition.patternDamage)
                    Equal(24 * action.coefficient, boss.CreatePatternDamage(action.id, 999).Amount, biome + " / " + action.id);

                if (pattern is LiverBossPattern)
                {
                    EnemyDamageRequest bomb = boss.CreatePatternDamage("blood-bomb", 999);
                    Hit(bomb.Amount, () => Invoke(pattern, "ExplodeBloodBomb", player.transform.position, bomb), "actual liver bomb hit");
                }
                if (pattern is StomachBossPattern)
                {
                    EnemyDamageRequest acid = boss.CreatePatternDamage("acid", 999);
                    EnemyDamageRequest dot = boss.CreatePatternDamage("acid-dot", 999);
                    Hit(acid.Amount, () => Invoke(pattern, "SplashAcid", player.transform.position, acid, dot), "actual stomach acid splash");
                    health.ResetHealth(); before = health.CurrentHealth;
                    yield return new WaitForSeconds(bossDefinition.stomachPattern.acidTickInterval + .15f);
                    Equal(RoundDamage(dot.Amount), before - health.CurrentHealth, "actual acid DoT tick once");
                    player.GetComponent<PlayerStatusEffectController>().CleanseTemporaryDebuffs();
                }
                if (pattern is LungBossPattern lungPattern)
                {
                    Require(boss.PatternOwnsContact && !boss.GetComponent<EnemyContactDamage>().TryApplyTo(player), "lung common/radial contact ownership");
                    int finalReward = 0;
                    Action<int> record = xp => finalReward += xp;
                    LevelUpManager.OnExpGained += record;
                    try { boss.TakeDamage(10000); boss.GrantExp(); }
                    finally { LevelUpManager.OnExpGained -= record; }
                    Equal(14, finalReward, "lung encounter final XP exactly once");
                    Require(lungPattern.IsEncounterDefeated, "actual final lung death completes encounter");
                    Pass("Lung real deaths: first brother XP 0; final survivor XP 14 exactly once");
                    Object.Destroy(pattern.gameObject);
                }
                pattern.StopAllCoroutines();
                boss.ReleaseToPool();
                Pass(biome + " actual pattern initialize/phase switch: HP 80→160 / attack 12→24 / speed 3→4.5, all action coefficients");
            }
            // Use the actual scene's spawners and boss rule clone, beyond controlled actors above.
            RestoreAssets();
            Set(normal, "statSets.Array.data[0].maxHealth", 40.5f);
            Set(normal, "statSets.Array.data[0].attackPower", 2.4f);
            Set(catalog.difficultyCatalog.hard, "enemies.maxHealth", 1);
            Set(catalog.difficultyCatalog.hard, "enemies.outgoingDamage", 1);
            EnemyController fraction = Spawn(normalRule);
            Equal(41, fraction.Stats.MaxHealth, "integer health conversion 40.5 → 41");
            Hit(2, () => Invoke(fraction, "ApplyDamageToPlayer"), "integer damage conversion 2.4 → 2");
            foreach (EnemyController actor in actors) if (actor != null && actor.gameObject.activeSelf) actor.ReleaseToPool();
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Intestine");
            EnemyController mapNormal = null, mapBoss = null;
            for (int frame = 0; frame < 100 && (mapNormal == null || mapBoss == null); frame++)
            {
                player = PlayerController.Instance;
                if (player != null)
                {
                    player.HealthComponent?.GrantTemporaryInvincibility(20);
                    if (mapNormal == null)
                    {
                        EnemySpawner targetSpawner = Object.FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None)
                            .FirstOrDefault(sp => ((EnemySpawnRuleConfig)typeof(EnemySpawner).GetField("config", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sp))?.monsterDefinition == normal);
                        if (targetSpawner != null) player.transform.position = targetSpawner.transform.position;
                        else if (frame % 10 == 0) player.transform.position = new Vector3(35 + (frame / 10 % 4) * 55, 0, 45 + (frame / 40) * 60);
                    }
                }
                var live = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
                mapNormal = live.FirstOrDefault(e => e.Balance != null && e.Config.monsterDefinition == normal && !e.IsBossEncounter);
                mapBoss = live.FirstOrDefault(e => e.Balance != null && e.IsBossEncounter);
                yield return new WaitForSeconds(.1f);
            }
            Require(mapNormal != null && mapBoss != null, "Intestine scene connected spawns: normal=" + (mapNormal != null) + ", boss=" + (mapBoss != null));
            Equal(41, mapNormal.Stats.MaxHealth, "actual map normal HP from edited asset");
            Equal(2.4f, mapNormal.Stats.AttackPower, "actual map normal attack from edited asset");
            Require(mapBoss.Balance.Current.MonsterId == "boss.intestine", "boss rule clone retains MonsterDefinition");
            Equal(mapBoss.Balance.Current.HealthUnits, mapBoss.Stats.MaxHealth, "actual map boss HP");
            Require(mapNormal.Balance.Current.Stage == 0 && mapBoss.Balance.Current.Stage == 0, "current gameplay stage 0");
            Pass("Actual Intestine scene: automatic normal spawner HP 41 / attack 2.4; boss factory retains definition and resolved HP");
            Pass("Fractional game units verified: HP 40.5 → 41, attack 2.4 → 2 HP; Inspector displays both");
            Pass("Fresh-run stage 0 verified; visit progression and persistence are covered by C-03 checks");
        }

        private static EnemyController Spawn(EnemySpawnRuleConfig rule)
        {
            foreach (EnemyController other in actors)
                if (other != null && other.gameObject.activeSelf) other.transform.position = player.transform.position + Vector3.right * 100;
            Vector3 position = player.transform.position + Vector3.left * .1f;
            EnemyController enemy = EnemyController.Acquire(null, "BalanceTest_" + rule.name, EnemyController.GetPoolArchetypeId(rule));
            enemy.Configure(null, rule, position, position);
            enemy.SetAiSuppressed(true);
            enemy.GetComponent<Collider>().enabled = false; // Invoke attacks explicitly; no unrelated physics hits between checks.
            Invoke(enemy, "EnsurePlayerTransform");
            actors.Add(enemy);
            return enemy;
        }

        private static void Set(Object asset, string path, float value)
        {
            if (!backups.ContainsKey(asset)) backups.Add(asset, EditorJsonUtility.ToJson(asset));
            using var serialized = new SerializedObject(asset);
            SerializedProperty property = serialized.FindProperty(path);
            Require(property != null, "missing Inspector property " + path);
            if (property.propertyType == SerializedPropertyType.Integer) property.intValue = (int)value;
            else property.floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RestoreAssets()
        {
            foreach (var entry in backups)
                if (entry.Key != null) { EditorJsonUtility.FromJsonOverwrite(entry.Value, entry.Key); EditorUtility.ClearDirty(entry.Key); }
            backups.Clear();
        }

        private static void Hit(float expected, Action action, string label)
        {
            health.ResetHealth();
            float before = health.CurrentHealth;
            action();
            Equal(RoundDamage(expected), before - health.CurrentHealth, label);
        }
        private static float RoundDamage(float damage) => damage <= 0 ? 0 : Mathf.Max(1, Mathf.Floor(damage + .5001f));
        private static void Pass(string text) { results.Add("PASS " + text); Debug.Log("[MonsterBalance-C02] PASS " + text); }
        private static void Require(bool condition, string label) { if (!condition) throw new InvalidOperationException(label); }
        private static void Equal(float expected, float actual, string label) => Require(Mathf.Abs(expected - actual) < .001f, label + ": expected " + expected + ", actual " + actual);
        private static object Invoke(object target, string method, params object[] args)
        {
            MethodInfo info = target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Require(info != null, "method " + method);
            return info.Invoke(target, args);
        }
    }
}
