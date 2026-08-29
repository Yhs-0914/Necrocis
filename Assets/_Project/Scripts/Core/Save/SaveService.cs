using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Necrocis
{
    public static class SaveService
    {
        public const int CurrentSchemaVersion = 1;

        private const string ProfileFileName = "profile.json";
        private const string NormalFileName = "normal.json";
        private const string HardFileName = "hard-run.json";
        private const int RandomSeedMin = 1;
        private const int RandomSeedMax = 100000;
        private const int BiomeSeedBucketSize = 100000;

        private static SaveFileStore store;
        private static ProfileSaveData profile;
        private static RunSaveData normal;
        private static RunSaveData hard;
        private static RunSaveData activeRun;
        private static bool initialized;
        private static bool restorePending;

        public static ProfileSaveData Profile
        {
            get
            {
                EnsureInitialized();
                return profile;
            }
        }

        public static bool HasActiveSession => activeRun != null && activeRun.isActive;
        public static GameDifficulty ActiveDifficulty => HasActiveSession ? activeRun.difficulty : GameDifficulty.Normal;
        public static bool IsRestorePending => restorePending;
        public static bool IsHardUnlocked => Profile.hardUnlocked;

        public static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            store = new SaveFileStore(Application.persistentDataPath);
            profile = LoadProfile();
            normal = LoadRun(NormalFileName, GameDifficulty.Normal);
            hard = LoadRun(HardFileName, GameDifficulty.Hard);
            MigrateLegacyBossDiscoveries();
            initialized = true;
        }

        public static IReadOnlyList<ContinueSaveSummary> GetContinueSummaries()
        {
            EnsureInitialized();
            List<ContinueSaveSummary> summaries = new List<ContinueSaveSummary>(2);
            if (IsValidContinueRun(normal, GameDifficulty.Normal))
            {
                summaries.Add(CreateSummary(normal));
            }

            if (profile.hardUnlocked && IsValidContinueRun(hard, GameDifficulty.Hard))
            {
                summaries.Add(CreateSummary(hard));
            }

            return summaries;
        }

        public static bool HasContinueSave(GameDifficulty difficulty)
        {
            EnsureInitialized();
            return difficulty == GameDifficulty.Normal
                ? IsValidContinueRun(normal, GameDifficulty.Normal)
                : profile.hardUnlocked && IsValidContinueRun(hard, GameDifficulty.Hard);
        }

        public static bool TryBeginNewGame(GameDifficulty difficulty, out string error)
        {
            EnsureInitialized();
            error = string.Empty;
            if (difficulty == GameDifficulty.Hard && !profile.hardUnlocked)
            {
                error = "Hard 난이도는 Normal 최종 보스 클리어 후 해금됩니다.";
                return false;
            }

            RunSaveData run = CreateNewRun(difficulty);
            if (!TryWriteRun(run, out error))
            {
                return false;
            }

            if (difficulty == GameDifficulty.Normal)
            {
                normal = run;
            }
            else
            {
                hard = run;
            }

            profile.lastSelectedDifficulty = difficulty;
            profile.lastPlayedDifficulty = difficulty;
            SaveProfile(out _);
            ActivateRun(run);
            return true;
        }

        public static bool TryContinue(GameDifficulty difficulty, out string error)
        {
            EnsureInitialized();
            error = string.Empty;
            RunSaveData run = difficulty == GameDifficulty.Normal ? normal : hard;
            if (difficulty == GameDifficulty.Hard && !profile.hardUnlocked)
            {
                error = "Hard 난이도가 잠겨 있습니다.";
                return false;
            }

            if (!IsValidContinueRun(run, difficulty))
            {
                error = $"{difficulty} 계속하기 저장이 없습니다.";
                return false;
            }

            profile.lastSelectedDifficulty = difficulty;
            profile.lastPlayedDifficulty = difficulty;
            SaveProfile(out _);
            ActivateRun(run);
            return true;
        }

        public static bool TrySaveActiveRun(out string error)
        {
            EnsureInitialized();
            error = string.Empty;
            if (!HasActiveSession)
            {
                error = "활성 게임 세션이 없습니다.";
                return false;
            }

            CaptureRuntimeState(activeRun);
            activeRun.lastSavedUtcTicks = DateTime.UtcNow.Ticks;
            profile.lastPlayedDifficulty = activeRun.difficulty;
            profile.lastSavedUtcTicks = activeRun.lastSavedUtcTicks;

            if (!TryWriteRun(activeRun, out error))
            {
                return false;
            }

            if (!SaveProfile(out error))
            {
                return false;
            }

            return true;
        }

        public static bool TryRestorePendingSession(out BiomeType resumeBiome, out string error)
        {
            EnsureInitialized();
            resumeBiome = BiomeType.None;
            error = string.Empty;
            if (!restorePending || !HasActiveSession)
            {
                return false;
            }

            PlayerController player = PlayerController.Instance ?? UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            PlayerStats playerStats = PlayerStats.Instance ?? UnityEngine.Object.FindFirstObjectByType<PlayerStats>();
            PlayerItemManager itemManager = PlayerItemManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<PlayerItemManager>();
            if (player == null || playerStats == null || itemManager == null)
            {
                error = "플레이어 저장 데이터를 복원할 런타임 컴포넌트가 준비되지 않았습니다.";
                return false;
            }

            PlayerProgressSaveData progress = activeRun.player ?? new PlayerProgressSaveData();
            LevelUpManager.RestoreProgress(progress);

            playerStats.ResetBaseStats(true);
            playerStats.ResetStats();
            IReadOnlyList<SavedStatModifierData> modifiers = progress.levelUpModifiers;
            if (modifiers != null)
            {
                for (int i = 0; i < modifiers.Count; i++)
                {
                    SavedStatModifierData modifier = modifiers[i];
                    if (modifier == null)
                    {
                        continue;
                    }

                    playerStats.ApplyModifier(new CharacterStatModifier(
                        modifier.statType,
                        modifier.value,
                        modifier.mode,
                        LevelUpManager.RestoredModifierSource));
                }
            }

            itemManager.RestoreSavedItems(progress.items);
            playerStats.RuntimeStats.SetCurrentHealth(
                progress.currentHealth < 0f ? playerStats.MaxHealth : progress.currentHealth);
            player.ReviveRuntimeStateAfterLoad();

            GameManager.Instance?.RestoreFromSave(activeRun);
            resumeBiome = activeRun.checkpoint != null ? activeRun.checkpoint.biome : BiomeType.None;
            restorePending = false;
            return true;
        }

        public static bool IsBossDefeated(BiomeType biome)
        {
            EnsureInitialized();
            if (HasActiveSession)
            {
                return activeRun.bosses != null && activeRun.bosses.IsDefeated(biome);
            }

            return profile.bossDiscoveries != null && profile.bossDiscoveries.IsDefeated(biome);
        }

        public static bool IsBossDiscovered(BiomeType biome)
        {
            EnsureInitialized();
            return profile.bossDiscoveries != null && profile.bossDiscoveries.IsDefeated(biome);
        }

        public static void MarkBossDefeated(BiomeType biome)
        {
            EnsureInitialized();
            if (HasActiveSession)
            {
                activeRun.bosses ??= new BossProgressSaveData();
                activeRun.bosses.SetDefeated(biome, true);
                if (activeRun.difficulty == GameDifficulty.Normal)
                {
                    profile.bossDiscoveries ??= new BossProgressSaveData();
                    profile.bossDiscoveries.SetDefeated(biome, true);
                }

                TrySaveActiveRun(out _);
                return;
            }

            profile.bossDiscoveries ??= new BossProgressSaveData();
            profile.bossDiscoveries.SetDefeated(biome, true);
            SaveProfile(out _);
        }

        public static void MarkFinalBossDefeated()
        {
            EnsureInitialized();
            if (!HasActiveSession)
            {
                return;
            }

            activeRun.bosses ??= new BossProgressSaveData();
            activeRun.bosses.finalBossDefeated = true;
            activeRun.campaignCompleted = true;
            if (activeRun.difficulty == GameDifficulty.Normal)
            {
                profile.normalCampaignCompleted = true;
                profile.hardUnlocked = true;
                profile.normalClearCount++;
            }
            else
            {
                profile.hardClearCount++;
                activeRun.isActive = false;
            }

            TryWriteRun(activeRun, out _);
            SaveProfile(out _);
        }

        public static bool TryHandleHardDeath(out string error)
        {
            EnsureInitialized();
            error = string.Empty;
            if (!HasActiveSession || activeRun.difficulty != GameDifficulty.Hard)
            {
                error = "활성 Hard run이 없습니다.";
                return false;
            }

            RunSaveData cleared = CreateNewRun(GameDifficulty.Hard);
            cleared.isActive = false;
            cleared.campaignStarted = false;
            cleared.player.items.Clear();
            if (!TryWriteRun(cleared, out error))
            {
                return false;
            }

            hard = cleared;
            activeRun = null;
            restorePending = false;
            return SaveProfile(out error);
        }

        public static bool TryPrepareNormalDeathRespawn(out string error)
        {
            EnsureInitialized();
            error = string.Empty;
            if (!HasActiveSession || activeRun.difficulty != GameDifficulty.Normal)
            {
                error = "활성 Normal run이 없습니다.";
                return false;
            }

            // Capture progression before any scene-load callbacks can replace or
            // reinitialize the persistent player. Death only restores health and
            // returns to the hub on Normal; level, stats and items stay intact.
            CaptureRuntimeState(activeRun);
            activeRun.player ??= new PlayerProgressSaveData();
            activeRun.player.currentHealth = -1f;
            activeRun.checkpoint ??= new ResumeCheckpointSaveData();
            activeRun.checkpoint.sceneName = SceneLoader.SCENE_HUB;
            activeRun.checkpoint.biome = BiomeType.None;
            activeRun.checkpoint.wasInBossRoom = false;
            activeRun.lastSavedUtcTicks = DateTime.UtcNow.Ticks;
            profile.lastPlayedDifficulty = activeRun.difficulty;
            profile.lastSavedUtcTicks = activeRun.lastSavedUtcTicks;

            if (!TryWriteRun(activeRun, out error) || !SaveProfile(out error))
            {
                return false;
            }

            // GameplaySaveCoordinator skips its scene-load autosave while this is
            // true, preventing an uninitialized Hub player from overwriting data.
            restorePending = true;
            return true;
        }

        public static void ClearActiveSessionReference()
        {
            activeRun = null;
            restorePending = false;
        }

        public static int GetOrCreateBiomeSeed(BiomeType biome)
        {
            EnsureInitialized();
            if (!HasActiveSession || biome == BiomeType.None)
            {
                return UnityEngine.Random.Range(RandomSeedMin, RandomSeedMax);
            }

            activeRun.world ??= new WorldRunSaveData();
            int seed = activeRun.world.GetSeed(biome);
            if (seed == 0)
            {
                seed = CreateBiomeSeed(biome, null);
                activeRun.world.SetSeed(biome, seed);
                TryWriteRun(activeRun, out _);
            }

            return seed;
        }

        public static void ResetBossDiscoveriesForDevelopment()
        {
            EnsureInitialized();
            profile.bossDiscoveries = new BossProgressSaveData();
            SaveProfile(out _);
        }

#if UNITY_EDITOR
        public static void UseStorageRootForTests(string rootPath)
        {
            store = new SaveFileStore(rootPath);
            profile = LoadProfile();
            normal = LoadRun(NormalFileName, GameDifficulty.Normal);
            hard = LoadRun(HardFileName, GameDifficulty.Hard);
            activeRun = null;
            restorePending = false;
            initialized = true;
            MigrateLegacyBossDiscoveries();
        }

        public static void ResetStaticStateForTests()
        {
            store = null;
            profile = null;
            normal = null;
            hard = null;
            activeRun = null;
            initialized = false;
            restorePending = false;
        }
#endif

        private static void ActivateRun(RunSaveData run)
        {
            activeRun = run;
            restorePending = true;
        }

        private static RunSaveData CreateNewRun(GameDifficulty difficulty)
        {
            DateTime now = DateTime.UtcNow;
            WorldRunSaveData previousWorld = difficulty == GameDifficulty.Normal
                ? normal?.world
                : hard?.world;
            return new RunSaveData
            {
                schemaVersion = CurrentSchemaVersion,
                saveId = Guid.NewGuid().ToString("N"),
                difficulty = difficulty,
                isActive = true,
                campaignStarted = true,
                campaignCompleted = false,
                player = new PlayerProgressSaveData(),
                bosses = new BossProgressSaveData(),
                checkpoint = new ResumeCheckpointSaveData(),
                world = CreateWorldRunSaveData(previousWorld),
                playTimeSeconds = 0L,
                lastSavedUtcTicks = now.Ticks
            };
        }

        private static WorldRunSaveData CreateWorldRunSaveData(WorldRunSaveData previousWorld)
        {
            var world = new WorldRunSaveData();
            world.SetSeed(BiomeType.Intestine, CreateBiomeSeed(BiomeType.Intestine, previousWorld));
            world.SetSeed(BiomeType.Liver, CreateBiomeSeed(BiomeType.Liver, previousWorld));
            world.SetSeed(BiomeType.Stomach, CreateBiomeSeed(BiomeType.Stomach, previousWorld));
            world.SetSeed(BiomeType.Lung, CreateBiomeSeed(BiomeType.Lung, previousWorld));
            return world;
        }

        private static int CreateBiomeSeed(BiomeType biome, WorldRunSaveData previousWorld)
        {
            int bucketOffset = (int)biome * BiomeSeedBucketSize;
            int seed = UnityEngine.Random.Range(RandomSeedMin, RandomSeedMax) + bucketOffset;
            int previousSeed = previousWorld?.GetSeed(biome) ?? 0;
            if (seed != previousSeed)
            {
                return seed;
            }

            int localSeed = seed - bucketOffset;
            localSeed = localSeed >= RandomSeedMax - 1 ? RandomSeedMin : localSeed + 1;
            return localSeed + bucketOffset;
        }

        private static ProfileSaveData LoadProfile()
        {
            if (store.TryRead(ProfileFileName, out ProfileSaveData loaded, out _)
                && ValidateProfile(loaded))
            {
                loaded.bossDiscoveries ??= new BossProgressSaveData();
                return loaded;
            }

            return new ProfileSaveData
            {
                schemaVersion = CurrentSchemaVersion,
                bossDiscoveries = new BossProgressSaveData(),
                lastSavedUtcTicks = DateTime.UtcNow.Ticks
            };
        }

        private static RunSaveData LoadRun(string fileName, GameDifficulty expectedDifficulty)
        {
            if (!store.TryRead(fileName, out RunSaveData loaded, out _)
                || !ValidateRun(loaded, expectedDifficulty))
            {
                return null;
            }

            NormalizeRun(loaded);
            return loaded;
        }

        private static void NormalizeRun(RunSaveData run)
        {
            run.player ??= new PlayerProgressSaveData();
            run.player.selectionHistory ??= new List<LevelUpStatChoice>();
            run.player.levelUpModifiers ??= new List<SavedStatModifierData>();
            run.player.items ??= new List<SavedItemStateData>();
            run.bosses ??= new BossProgressSaveData();
            run.checkpoint ??= new ResumeCheckpointSaveData();
            run.world ??= new WorldRunSaveData();
        }

        private static bool ValidateProfile(ProfileSaveData value)
        {
            return value != null
                   && value.schemaVersion > 0
                   && value.schemaVersion <= CurrentSchemaVersion;
        }

        private static bool ValidateRun(RunSaveData value, GameDifficulty difficulty)
        {
            return value != null
                   && value.schemaVersion > 0
                   && value.schemaVersion <= CurrentSchemaVersion
                   && value.difficulty == difficulty
                   && !string.IsNullOrWhiteSpace(value.saveId);
        }

        private static bool IsValidContinueRun(RunSaveData run, GameDifficulty difficulty)
        {
            return ValidateRun(run, difficulty)
                   && run.isActive
                   && run.campaignStarted
                   && run.player != null
                   && run.player.level >= 1;
        }

        private static ContinueSaveSummary CreateSummary(RunSaveData run)
        {
            return new ContinueSaveSummary(
                run.difficulty,
                Math.Max(1, run.player.level),
                run.player.job,
                run.bosses?.DefeatedCount ?? 0,
                run.checkpoint?.sceneName ?? SceneLoader.SCENE_HUB,
                run.lastSavedUtcTicks);
        }

        private static void CaptureRuntimeState(RunSaveData run)
        {
            run.player ??= new PlayerProgressSaveData();
            LevelUpManager.CaptureProgress(run.player);

            PlayerStats playerStats = PlayerStats.Instance ?? UnityEngine.Object.FindFirstObjectByType<PlayerStats>();
            if (playerStats != null)
            {
                run.player.currentHealth = playerStats.CurrentHealth;
            }

            PlayerItemManager itemManager = PlayerItemManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<PlayerItemManager>();
            if (itemManager != null)
            {
                run.player.items = itemManager.CaptureSavedItems();
            }

            GameManager.Instance?.CaptureToSave(run);

            string sceneName = SceneManager.GetActiveScene().name;
            if (!string.Equals(sceneName, SceneLoader.SCENE_MAIN_MENU, StringComparison.Ordinal))
            {
                run.checkpoint ??= new ResumeCheckpointSaveData();
                run.checkpoint.sceneName = sceneName;
                run.checkpoint.biome = GameManager.Instance?.CurrentBiome ?? BiomeType.None;
                run.checkpoint.wasInBossRoom = GameManager.Instance != null
                                                 && GameManager.Instance.CurrentState == GameState.InBossRoom;
                if (string.Equals(sceneName, SceneLoader.SCENE_HUB, StringComparison.Ordinal))
                {
                    run.checkpoint.biome = BiomeType.None;
                    run.checkpoint.wasInBossRoom = false;
                }
            }
        }

        private static bool TryWriteRun(RunSaveData run, out string error)
        {
            NormalizeRun(run);
            run.schemaVersion = CurrentSchemaVersion;
            string fileName = run.difficulty == GameDifficulty.Normal ? NormalFileName : HardFileName;
            return store.TryWrite(fileName, run, out error);
        }

        private static bool SaveProfile(out string error)
        {
            profile.schemaVersion = CurrentSchemaVersion;
            profile.lastSavedUtcTicks = DateTime.UtcNow.Ticks;
            return store.TryWrite(ProfileFileName, profile, out error);
        }

        private static void MigrateLegacyBossDiscoveries()
        {
            profile.bossDiscoveries ??= new BossProgressSaveData();
            bool changed = false;
            foreach (BiomeType biome in new[]
                     {
                         BiomeType.Intestine,
                         BiomeType.Liver,
                         BiomeType.Stomach,
                         BiomeType.Lung
                     })
            {
                if (profile.bossDiscoveries.IsDefeated(biome)
                    || PlayerPrefs.GetInt(BossProgress.GetLegacyKey(biome), 0) != 1)
                {
                    continue;
                }

                profile.bossDiscoveries.SetDefeated(biome, true);
                changed = true;
            }

            if (changed)
            {
                SaveProfile(out _);
            }
        }
    }
}
