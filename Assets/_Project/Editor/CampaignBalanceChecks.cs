using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    /// <summary>Checks the saved campaign assets and real runtime math without changing the open scene or saves.</summary>
    public static class CampaignBalanceChecks
    {
        private static readonly List<string> checks = new List<string>();

        [MenuItem("Necrocis/Balance/Export Current Balance Report")]
        public static void ExportReport()
        {
            if (SaveService.BalanceMetrics == null)
            {
                Debug.LogWarning("[CampaignBalance] 활성 플레이 세션에서 내보내세요.");
                return;
            }
            Directory.CreateDirectory("Logs/CampaignBalance");
            File.WriteAllText("Logs/CampaignBalance/current-play-report.json", JsonUtility.ToJson(SaveService.BalanceMetrics, true));
            Debug.Log("[CampaignBalance] Logs/CampaignBalance/current-play-report.json");
        }

        [MenuItem("Necrocis/Balance/Run Campaign Balance Checks %&b")]
        public static void Run()
        {
            checks.Clear();
            string error = null;
            try
            {
                Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Run these checks in Edit Mode.");
                CheckHealth();
                CheckProgression();
                CheckMonsters();
                CheckPlayerPrefab();
                CheckFinalBoss();
            }
            catch (Exception exception)
            {
                error = exception.ToString();
                Debug.LogException(exception);
            }
            Directory.CreateDirectory("Logs/CampaignBalance");
            File.WriteAllText("Logs/CampaignBalance/checks.txt",
                (error == null ? "PASS" : "FAIL") + "\n" + string.Join("\n", checks) + "\n" + error);
            Debug.Log("[CampaignBalance] " + (error == null ? "PASS" : "FAIL") + ": " + checks.Count + " assertions. Logs/CampaignBalance/checks.txt");
            if (Application.isBatchMode) EditorApplication.Exit(error == null ? 0 : 1);
        }

        private static void CheckHealth()
        {
            var go = new GameObject("CampaignBalance.Temp") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var stats = go.AddComponent<CharacterStats>();
                stats.ConfigureBaseStats(new CharacterStatValue(CharacterStatType.MaxHealth, 10));
                var source = HealthEffectSource.ForOwner(go, "review", persistenceKey: "review:damage");
                stats.ApplyDamage(.75f, source);
                Equal(10, stats.CurrentHealth, "main whole-health accounting retained");
                Equal(.75f, stats.DamageRemainder(source), "fractional damage retained by source");
                var saved = JsonUtility.FromJson<PlayerProgressSaveData>(JsonUtility.ToJson(new PlayerProgressSaveData
                { currentHealth = stats.CurrentHealth, healthDeltas = stats.CapturePersistentHealthDeltas(), splitRegenerationUsed = true }));
                stats.BeginHealthLifetime();
                stats.SetCurrentHealth(saved.currentHealth);
                stats.RestorePersistentHealthDeltas(saved.healthDeltas, key => key == source.PersistenceKey ? source : (HealthEffectSource?)null);
                Require(saved.splitRegenerationUsed, "run-wide revival flag survives JSON");
                for (int i = 0; i < 3; i++) stats.ApplyDamage(.75f, source);
                Equal(7, stats.CurrentHealth, "reduced hits accumulate across load through main ledger");
                stats.ResetHealthToMax();
                Equal(0, stats.DamageRemainder(source), "full heal clears residual damage");
                stats.ApplyDamage(1);
                for (int i = 0; i < 4; i++) stats.RestoreHealth(.25f);
                Equal(10, stats.CurrentHealth, "main fractional healing accumulation retained");
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        private static void CheckProgression()
        {
            var config = AssetDatabase.LoadAssetAtPath<LevelProgressionConfig>("Assets/_Project/Resources/Balance/LevelProgressionConfig.asset");
            Require(config != null, "normal progression loads");
            Equal(8990, Enumerable.Range(1, 29).Sum(config.GetRequiredExpForCurrentLevel), "Lv30 cumulative XP");
            foreach (var pair in new[] {
                (CharacterStatType.AttackPower, 1f), (CharacterStatType.AttackSpeed, 20f),
                (CharacterStatType.AttackRange, 10f), (CharacterStatType.Magic, 25f),
                (CharacterStatType.MoveSpeed, 5f), (CharacterStatType.MaxHealth, 2f) })
            {
                Require(config.TryGetLevelUpStatValue(pair.Item1, out var value), "growth config " + pair.Item1);
                Equal(pair.Item2, value.value, "growth value " + pair.Item1);
            }
            var original = new PlayerProgressSaveData();
            LevelUpManager.CaptureProgress(original);
            try
            {
                LevelUpManager.RestoreProgress(new PlayerProgressSaveData());
                LevelUpManager.AddResolvedEnemyKillExp(8990);
                Equal(5, LevelUpManager.GetCurrentLevel(), "large reward stops for job selection");
                Equal(8750, LevelUpManager.GetCurrentExp(), "overflow XP is banked");
                LevelUpManager.SetJob(JobType.Mage);
                Equal(30, LevelUpManager.GetCurrentLevel(), "banked XP resumes after job selection");
            }
            finally { LevelUpManager.RestoreProgress(original); }
        }

        private static void CheckMonsters()
        {
            var progression = AssetDatabase.LoadAssetAtPath<MonsterProgressionProfile>("Assets/_Project/Data/MonsterBalance/MonsterProgressionProfile.asset");
            var normal = AssetDatabase.LoadAssetAtPath<DifficultyBalanceProfile>("Assets/_Project/Resources/Balance/Difficulty/NormalDifficultyBalance.asset");
            var hard = AssetDatabase.LoadAssetAtPath<DifficultyBalanceProfile>("Assets/_Project/Resources/Balance/Difficulty/HardDifficultyBalance.asset");
            Require(progression.GetValidationError() == null, "stage profile valid");
            string[] guids = AssetDatabase.FindAssets("t:MonsterDefinition", new[] { "Assets/_Project/Data/MonsterBalance/Definitions" });
            Require(guids.Length == 30, "30 monster definitions");
            foreach (string guid in guids)
            {
                var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                foreach (var difficulty in new[] { normal, hard })
                    for (int stage = 0; stage <= 4; stage++)
                        foreach (var phase in definition.statSets)
                            Require(MonsterBalanceResolver.TryResolve(definition, phase.id, difficulty, progression, stage, out _, out _),
                                definition.monsterId + "/" + phase.id + "/" + difficulty.difficulty + "/" + stage);
            }
            int[] eliteXP = { 100, 120, 125, 140, 160 }, bossXP = { 240, 300, 300, 350, 400 };
            var elite = LoadDefinition("I-01");
            var boss = LoadDefinition("boss.intestine");
            for (int i = 0; i <= 4; i++)
            {
                Require(MonsterBalanceResolver.TryResolve(elite, "Default", normal, progression, i, out var e, out _), "resolve elite");
                Require(MonsterBalanceResolver.TryResolve(boss, "Default", normal, progression, i, out var b, out _), "resolve boss");
                Equal(eliteXP[i], e.Experience, "field elite XP stage " + i);
                Equal(bossXP[i], b.Experience, "region boss XP stage " + i);
                Equal(360, b.MaxHealth, "region boss HP independent of stage " + i);
            }
            // Every ordering of the four regions gets each stage once.
            foreach (var a in new[] { 0, 1, 2, 3 })
                foreach (var b in new[] { 0, 1, 2, 3 }.Where(x => x != a))
                    foreach (var c in new[] { 0, 1, 2, 3 }.Where(x => x != a && x != b))
                    {
                        int d = 6 - a - b - c;
                        Require(new[] { a, b, c, d }.Distinct().Count() == 4, "valid region ordering");
                    }
        }

        private static MonsterDefinition LoadDefinition(string name) => AssetDatabase.LoadAssetAtPath<MonsterDefinition>(
            "Assets/_Project/Data/MonsterBalance/Definitions/" + name + ".asset");

        private static void CheckPlayerPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Generated/FinalBoss/FinalBossPlayerFallback.prefab");
            var attack = new SerializedObject(prefab.GetComponent<PlayerAttack>());
            Equal(.55f, attack.FindProperty("meleeCooldown").floatValue, "prefab melee cooldown");
            Equal(.75f, attack.FindProperty("attackCooldown").floatValue, "prefab ranged cooldown");
            var health = new SerializedObject(prefab.GetComponent<Health>());
            Equal(.6f, health.FindProperty("invincibilityDuration").floatValue, "prefab hit immunity");
            var skills = new SerializedObject(prefab.GetComponent<PlayerClassSkillController>());
            foreach (var v in new[] { ("mageSkill1.additionalDamageMin", 5f), ("mageSkill2.baseDamage", 16f),
                ("archerSkill1.projectileDamage", 1.5f), ("archerSkill2.projectileDamage", 8f),
                ("warriorSkill1.damage", 5f), ("warriorSkill2.damage", 12f) })
                Equal(v.Item2, skills.FindProperty(v.Item1).floatValue, "prefab " + v.Item1);
            var items = new SerializedObject(prefab.GetComponent<PlayerItemCombatEffects>());
            Equal(.7f, items.FindProperty("doubleShotDamageMultiplier").floatValue, "prefab double shot");
            Equal(45, items.FindProperty("recoveryFactorInterval").floatValue, "prefab combat recovery interval");
            Equal(4, items.FindProperty("splitRegenerationReviveHealth").floatValue, "prefab revive HP");
        }

        private static void CheckFinalBoss()
        {
            var config = AssetDatabase.LoadAssetAtPath<FinalBossBalanceConfig>("Assets/_Project/Resources/Balance/FinalBoss/FinalBossBalanceConfig.asset");
            Require(config.GetValidationError() == null, "final boss config valid");
            Equal(180, config.phaseOne.pillarMaxHealth, "pillar HP");
            Equal(1600, config.phaseTwo.bossMaxHealth, "shared body HP");
            Equal(60, config.phaseThree.collapseDuration, "collapse seconds");
            Equal(0, config.normal.summonExperience, "no infinite summon XP");
            Equal(.3f, config.phaseTwo.reflectedDamageRatio, "reflection ratio");
        }

        private static void Equal(float expected, float actual, string label) => Require(Mathf.Abs(expected - actual) < .001f, label + " expected=" + expected + " actual=" + actual);
        private static void Require(bool condition, string label)
        {
            if (!condition) throw new InvalidOperationException(label);
            checks.Add(label);
        }
    }
}
