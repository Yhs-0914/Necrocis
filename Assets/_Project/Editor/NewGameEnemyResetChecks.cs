using System;
using System.IO;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEngine;

public static class NewGameEnemyResetChecks
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int assertions;

    public static void Run()
    {
        int exit = 1;
        GameObject enemyObject = null, spawnerObject = null;
        try
        {
            if (!Application.isBatchMode || Application.isPlaying)
                throw new InvalidOperationException("Requires an isolated batch editor.");
            SessionRuntimeBindings.Register();
            SaveService.UseStorageRootForTests(Path.GetFullPath("tmp/reset-check-" + Guid.NewGuid().ToString("N")));
            Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out var error), "initial run: " + error);
            string oldRun = SaveService.ActiveRunId;
            LevelUpManager.RestoreProgress(new PlayerProgressSaveData { level = 30, job = JobType.Mage });
            foreach (var biome in new[] { BiomeType.Intestine, BiomeType.Liver, BiomeType.Stomach, BiomeType.Lung })
                SaveService.MarkBossDefeated(biome);
            var oldVisit = SaveService.EnterLoadedBiome(BiomeType.Intestine, 1234);
            Require(oldVisit.Stage == 4, "old run has four-boss progression");

            spawnerObject = new GameObject("ResetSpawnerCheck");
            var spawner = spawnerObject.AddComponent<EnemySpawner>();
            var rule = new EnemySpawnRuleConfig { name = "ResetCheck", maxHealth = 10, maxAlive = 1 };
            typeof(EnemySpawner).GetField("config", Private).SetValue(spawner, rule);
            int grownCount = Population(spawner);
            Require(grownCount >= 6 && grownCount <= 7, "level 30 population is 6-7");

            enemyObject = new GameObject("ResetHealthCheck");
            var enemy = enemyObject.AddComponent<EnemyController>();
            var stats = enemyObject.AddComponent<CharacterStats>();
            typeof(EnemyController).GetField("config", Private).SetValue(enemy, rule);
            typeof(EnemyController).GetField("stats", Private).SetValue(enemy, stats);
            stats.ConfigureBaseStats(new CharacterStatValue(CharacterStatType.MaxHealth, 10));
            Invoke(enemy, "InitializePlayerLevelHealthScaling");
            Require(stats.MaxHealth == 20, "level 30 enemy health includes +10");
            Require(SaveService.TrySaveActiveRun(out error), "save old campaign: " + error);

            Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out error), "replace campaign: " + error);
            Require(SaveService.ActiveRunId != oldRun, "new run identity");
            Require(LevelUpManager.GetCurrentLevel() == 1, "new-game activation resets level before player restoration");
            Require(LevelUpManager.GetCurrentJob() == JobType.None, "new-game activation resets job");
            Require(SaveService.IsRestorePending, "full player restoration remains pending");
            Require(SaveService.RunMonsterStage == 0, "new game resets monster stage");
            int freshCount = Population(spawner);
            Require(freshCount >= 1 && freshCount <= 2, "same spawner recalculates level 1 population");
            Invoke(enemy, "RefreshPlayerLevelHealth");
            Require(stats.MaxHealth == 10, "existing enemy loses old level health bonus");
            var freshVisit = SaveService.EnterLoadedBiome(BiomeType.Intestine, 1234);
            Require(freshVisit.Stage == 0 && freshVisit.RunId != oldVisit.RunId && freshVisit.VisitId != oldVisit.VisitId,
                "new game cannot reuse the old visit even with the same scene handle");
            Require(SaveService.IsBossDiscovered(BiomeType.Intestine), "profile artwork discovery remains unlocked");
            Require(!SaveService.IsBossDefeated(BiomeType.Intestine), "profile discoveries do not raise run monster stage");

            LevelUpManager.RestoreProgress(new PlayerProgressSaveData { level = 26, job = JobType.Mage });
            Require(SaveService.TrySaveActiveRun(out error), "save continue fixture: " + error);
            SaveService.ClearActiveSessionReference();
            LevelUpManager.ResetProgress();
            Require(SaveService.TryContinue(GameDifficulty.Normal, out error), "continue: " + error);
            Require(LevelUpManager.GetCurrentLevel() == 26, "continue publishes saved level before player restoration");
            Require(LevelUpManager.GetCurrentJob() == JobType.Mage, "continue preserves saved job");
            Debug.Log("NEW_GAME_ENEMY_RESET_PASS assertions=" + assertions + " population=" + grownCount + "->" + freshCount + " health=20->10");
            exit = 0;
        }
        catch (Exception error) { Debug.LogException(error); }
        finally
        {
            if (spawnerObject != null) UnityEngine.Object.DestroyImmediate(spawnerObject);
            if (enemyObject != null) UnityEngine.Object.DestroyImmediate(enemyObject);
            SaveService.ResetStaticStateForTests();
            EditorApplication.Exit(exit);
        }
    }

    private static int Population(EnemySpawner spawner) =>
        (int)typeof(EnemySpawner).GetMethod("GetMaxAlive", Private).Invoke(spawner, new object[] { new WorldDifficultyBalance() });
    private static void Invoke(object target, string name) => target.GetType().GetMethod(name, Private).Invoke(target, null);
    private static void Require(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new InvalidOperationException(message);
    }
}
