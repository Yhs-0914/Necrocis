using System;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NecrocisEditor
{
    // Run with -batchmode -nographics -executeMethod NecrocisEditor.BalanceValidationRunner.Run.
    public static class BalanceValidationRunner
    {
        private static int checks;
        private static int playFrames;
        private const string PendingPlayValidation = "Necrocis.BalanceValidation.PendingPlay";

        public static void RunPlayMode()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(PendingPlayValidation, true);
            RegisterPendingPlayValidation();
            EditorApplication.isPlaying = true;
        }

        [InitializeOnLoadMethod]
        private static void RegisterPendingPlayValidation()
        {
            if (!SessionState.GetBool(PendingPlayValidation, false)) return;
            EditorApplication.update -= WaitForPlayValidation;
            EditorApplication.update += WaitForPlayValidation;
        }

        private static void WaitForPlayValidation()
        {
            if (!Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (++playFrames < 3) return;
            EditorApplication.update -= WaitForPlayValidation;
            SessionState.SetBool(PendingPlayValidation, false);
            Run();
        }

        public static void Run()
        {
            checks = 0;
            try
            {
                ValidateProgression();
                ValidateItems();
                ValidateEnemyTiming();
                ValidateDifficulty();
                Debug.Log($"BALANCE VALIDATION PASSED: {checks} checks; playing={Application.isPlaying}");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void Check(bool condition, string message)
        {
            checks++;
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void SetProgressionField(string name, object value)
        {
            typeof(LevelUpManager).GetField(name, BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, value);
        }

        private static void ValidateProgression()
        {
            var config = ScriptableObject.CreateInstance<LevelProgressionConfig>();
            try
            {
                Check(config.MaxLevel == 30, "Maximum level must remain 30.");
                Check(config.JobSelectionLevel == 5, "Default job selection must be level 5.");
                Check(config.Skill1UnlockLevel == 5 && config.Skill2UnlockLevel == 10, "Skills must unlock at 5 and 10.");
                Check(config.GetExpGainMultiplier(5, JobType.None) == 0f, "Selection blocks the early XP multiplier.");
                Check(config.GetExpGainMultiplier(5, JobType.Warrior) == config.jobSelectionSelectedExpMultiplier,
                    "Selected-job XP multiplier takes precedence at level 5.");
                Check(config.GetRequiredExpForCurrentLevel(2) == 100 && config.GetRequiredExpForCurrentLevel(3) == 125,
                    "Required XP formula must be preserved.");
                var asset = Resources.Load<LevelProgressionConfig>(LevelProgressionConfig.DefaultResourcePath);
                Check(asset != null && asset.JobSelectionLevel == 5 && asset.Skill1UnlockLevel == 5 && asset.Skill2UnlockLevel == 10,
                    "Resource config must match the progression contract.");
                SetProgressionField("progressionConfig", config);
                SetProgressionField("currentLevel", 1);
                SetProgressionField("currentExp", 0);
                SetProgressionField("pendingLevelUps", 0);
                SetProgressionField("currentJob", JobType.None);
                SetProgressionField("expRequirementInitialized", false);
                LevelUpManager.SetJob(JobType.Warrior);
                Check(LevelUpManager.GetCurrentJob() == JobType.None, "Jobs cannot be selected before level 5.");
                LevelUpManager.AddExp(10000);
                Check(LevelUpManager.GetCurrentLevel() == 5, "Large XP awards must stop at job selection.");
                int storedExp = LevelUpManager.GetCurrentExp();
                LevelUpManager.AddExp(100);
                LevelUpManager.AddEnemyKillExp();
                Check(LevelUpManager.GetCurrentExp() == storedExp, "XP gain must be blocked while awaiting a job.");
                Check(!LevelUpManager.IsSkillUnlocked(1), "A job is required for skills.");
                LevelUpManager.SetJob(JobType.Warrior);
                Check(LevelUpManager.IsSkillUnlocked(1) && !LevelUpManager.IsSkillUnlocked(2), "Level 5 unlocks only skill 1.");
                LevelUpManager.SetJob(JobType.Mage);
                Check(LevelUpManager.GetCurrentJob() == JobType.Warrior, "Job selection is final.");
                int safety = 100;
                while (LevelUpManager.HasPendingLevelUp() && safety-- > 0) LevelUpManager.ProcessNextPendingLevelUp();
                Check(safety > 0 && LevelUpManager.GetCurrentLevel() >= 10, "Stored XP must resume after selection.");
                foreach (JobType job in new[] { JobType.Warrior, JobType.Archer, JobType.Mage })
                {
                    SetProgressionField("currentJob", job);
                    foreach (int level in new[] { 4, 5, 9, 10, 30 })
                    {
                        SetProgressionField("currentLevel", level);
                        Check(LevelUpManager.IsSkillUnlocked(1) == (level >= 5), $"{job} skill 1 at {level}");
                        Check(LevelUpManager.IsSkillUnlocked(2) == (level >= 10), $"{job} skill 2 at {level}");
                        Check(!LevelUpManager.IsSkillUnlocked(0) && !LevelUpManager.IsSkillUnlocked(3), "Invalid skill slots stay locked.");
                        Check(LevelUpManager.GetGrowthStage() == (level >= 10 ? PlayerGrowthStage.Second : PlayerGrowthStage.First),
                            "Second growth preserves the current job and follows skill 2 unlock.");
                    }
                }
                LevelUpManager.AddExp(10000);
                LevelUpManager.DebugLevelUp();
                Check(LevelUpManager.GetCurrentLevel() == 30, "XP/debug must respect cap.");
            }
            finally
            {
                SetProgressionField("progressionConfig", null);
                SetProgressionField("currentLevel", 1);
                SetProgressionField("currentExp", 0);
                SetProgressionField("pendingLevelUps", 0);
                SetProgressionField("currentJob", JobType.None);
                SetProgressionField("expRequirementInitialized", false);
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        private static void ValidateItems()
        {
            var tier = typeof(PlayerItemManager.PlayerItemEntry).GetProperty("Tier");
            Check(tier != null, "Item catalog must expose tier metadata.");
            foreach (int input in new[] { -1, 0, 2, 4, 9 })
            {
                var entry = JsonUtility.FromJson<PlayerItemManager.PlayerItemEntry>("{\"itemId\":\"test\",\"tier\":" + input + "}");
                int expected = Mathf.Clamp(input, 0, 4);
                Check((int)tier.GetValue(entry) == expected, "Catalog tiers must be bounded to 0–4.");
                var acquired = new PlayerItemManager.AcquiredPlayerItem(entry);
                Check((int)typeof(PlayerItemManager.AcquiredPlayerItem).GetProperty("Tier").GetValue(acquired) == expected,
                    "Acquisition must retain tier.");
            }
            var owner = new GameObject("ItemSpawnValidation");
            try
            {
                var spawner = owner.AddComponent<WorldItemSpawner>();
                var count = typeof(WorldItemSpawner).GetMethod("GetSpawnCount");
                Check(count != null, "Spawner must resolve biome quantity overrides.");
                Check((int)count.Invoke(spawner, new object[] { BiomeType.Liver }) == 3, "Empty overrides preserve floor count.");
                var serialized = new SerializedObject(spawner);
                var overrides = serialized.FindProperty("biomeSpawnCounts");
                overrides.arraySize = 2;
                overrides.GetArrayElementAtIndex(0).FindPropertyRelative("biome").intValue = (int)BiomeType.Liver;
                overrides.GetArrayElementAtIndex(0).FindPropertyRelative("count").intValue = 0;
                overrides.GetArrayElementAtIndex(1).FindPropertyRelative("biome").intValue = (int)BiomeType.Lung;
                overrides.GetArrayElementAtIndex(1).FindPropertyRelative("count").intValue = 7;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Check((int)count.Invoke(spawner, new object[] { BiomeType.Liver }) == 0, "Zero disables floor spawning.");
                Check((int)count.Invoke(spawner, new object[] { BiomeType.Lung }) == 7, "Biome overrides resolve independently.");
                Check((int)count.Invoke(spawner, new object[] { BiomeType.Intestine }) == 3, "Unspecified biome keeps default count.");
            }
            finally { UnityEngine.Object.DestroyImmediate(owner); }
        }

        private static void ValidateEnemyTiming()
        {
            var rule = new EnemySpawnRuleConfig { attackCooldown = 0.8f, attackAnimationSpeed = 0.1f, isElite = true };
            var cooldown = typeof(EnemySpawnRuleConfig).GetProperty("EffectiveAttackCooldown");
            var interval = typeof(EnemySpawnRuleConfig).GetProperty("EffectiveAttackFrameInterval");
            Check(cooldown != null && interval != null, "Enemy rules must expose resolved elite timing.");
            Check((float)cooldown.GetValue(rule) == 0.8f && (float)interval.GetValue(rule) == 0.1f, "Legacy elite timing must be preserved.");
            JsonUtility.FromJsonOverwrite("{\"eliteAttackCooldown\":2,\"eliteAttackFrameInterval\":0.25}", rule);
            Check((float)cooldown.GetValue(rule) == 2f && (float)interval.GetValue(rule) == 0.25f, "Elite timing must be independently configurable.");
            rule.isElite = false;
            Check((float)cooldown.GetValue(rule) == 0.8f && (float)interval.GetValue(rule) == 0.1f, "Normal enemies must ignore elite overrides.");
            Check(rule.chargeCooldown == 3f, "Default charge cooldown must remain 3 seconds.");
        }

        private static void ValidateDifficulty()
        {
            Check(Enum.IsDefined(typeof(GameDifficulty), "Hard"), "Hard difficulty must be supported.");
            var type = typeof(GameManager).Assembly.GetType("Necrocis.EnemyBalanceMultipliers");
            Check(type != null, "Difficulty profiles must expose extensible enemy multipliers.");
            var profile = Activator.CreateInstance(type);
            var apply = type.GetMethod("CreateRuntimeRule");
            var source = new EnemySpawnRuleConfig { maxHealth = 30f, attackDamage = 2f, maxAlive = 3 };
            var copy = (EnemySpawnRuleConfig)apply.Invoke(profile, new object[] { source });
            Check(!ReferenceEquals(source, copy), "Difficulty must use a runtime copy.");
            Check(copy.maxHealth == 30f && copy.attackDamage == 2f && copy.maxAlive == 3, "Neutral profiles preserve balance.");
            JsonUtility.FromJsonOverwrite("{\"health\":2,\"attack\":3,\"spawnCount\":2}", profile);
            copy = (EnemySpawnRuleConfig)apply.Invoke(profile, new object[] { source });
            Check(copy.maxHealth == 60f && copy.attackDamage == 6f && copy.maxAlive == 6, "Configured profiles must scale runtime values.");
            Check(source.maxHealth == 30f && source.attackDamage == 2f && source.maxAlive == 3, "Source rules must remain unchanged.");
            source.additionalBaseStats.Add(new CharacterStatValue(CharacterStatType.MaxHealth, 40f));
            copy = (EnemySpawnRuleConfig)apply.Invoke(profile, new object[] { source });
            Check(copy.additionalBaseStats[0].value == 80f && source.additionalBaseStats[0].value == 40f,
                "Additional health overrides scale without mutating source lists.");
            copy.chaseRadius = 999f;
            Check(source.chaseRadius != 999f, "Runtime aggro changes must not affect source assets.");
            var owner = new GameObject("DifficultyValidation");
            try
            {
                var manager = owner.AddComponent<GameManager>();
                Check(manager.TrySetDifficulty(GameDifficulty.Hard), "Hub difficulty can change to Hard.");
                var scaled = manager.CreateEnemyRuntimeRule(source);
                Check(scaled.maxHealth == source.maxHealth && scaled.attackDamage == source.attackDamage,
                    "Hard must use neutral values until configured.");
                var serialized = new SerializedObject(manager);
                serialized.FindProperty("hardBalance").FindPropertyRelative("eliteEnemies").FindPropertyRelative("health").floatValue = 3f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Check(manager.CreateEnemyRuntimeRule(source).maxHealth == source.maxHealth, "Elite profile must not affect normal enemies.");
                source.isElite = true;
                Check(manager.CreateEnemyRuntimeRule(source).maxHealth == source.maxHealth * 3f, "Elite profile applies independently.");
                manager.SetGameState(GameState.InBiome);
                Check(!manager.TrySetDifficulty(GameDifficulty.Normal), "Difficulty changes during an encounter must be rejected.");
            }
            finally { UnityEngine.Object.DestroyImmediate(owner); }
        }
    }
}
