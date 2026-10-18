using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public static partial class SaveService
    {
        public const int CurrentSchemaVersion = SaveSchema.CurrentVersion;

        private const string ProfileFileName = "profile.json";
        private const string NormalFileName = "normal.json";
        private const string HardFileName = "hard-run.json";
        private const int RandomSeedMin = 1;
        private const int RandomSeedMax = 100000;
        private const int BiomeSeedBucketSize = 100000;

        private static ISessionRuntimeSavePort runtimePort;
        public static void BindRuntimePort(ISessionRuntimeSavePort port)=>runtimePort=port??throw new ArgumentNullException(nameof(port));
        private static SaveFileStore store;
        private static SaveRepository repository;
        private static SaveOperation pendingOperation;
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

        public static bool HasActiveSession => activeRun != null && activeRun.isActive && !HasPendingTerminalOperation;
        public static GameDifficulty ActiveDifficulty => HasActiveSession ? activeRun.difficulty : GameDifficulty.Normal;
        public static bool IsRestorePending => restorePending;
        public static bool IsHardUnlocked => Profile.hardUnlocked;

        public static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            InitializeStorage(Application.persistentDataPath);
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
            if (!TryFinishPendingSave(out error)) return false;
            if (pendingOperation != null)
                return RetryMatchingOperation("new-game", difficulty, out error);
            if (!CanUseSlot(difficulty, out error)) return false;
            if (difficulty == GameDifficulty.Hard && !profile.hardUnlocked)
            {
                error = "Hard 난이도는 Normal 최종 보스 클리어 후 해금됩니다.";
                return false;
            }

            RunSaveData run = CreateNewRun(difficulty);
            ProfileSaveData target = Copy(profile);
            target.lastSelectedDifficulty = target.lastPlayedDifficulty = difficulty;
            return Commit("new-game", run, target, false, out error);
        }

        public static bool TryContinue(GameDifficulty difficulty, out string error)
        {
            EnsureInitialized();
            if (!TryFinishPendingSave(out error)) return false;
            if (pendingOperation != null)
                return RetryMatchingOperation("continue", difficulty, out error);
            if (!CanUseSlot(difficulty, out error)) return false;
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

            ProfileSaveData target = Copy(profile);
            target.lastSelectedDifficulty = target.lastPlayedDifficulty = difficulty;
            return Commit("continue", Copy(run), target, false, out error);
        }

        public static bool TrySaveActiveRun(out string error)
        {
            EnsureInitialized();
            if (pendingOperation != null)
            {
                bool terminal = HasPendingTerminalOperation;
                if (!TryRetryPendingOperation(out error)) return false;
                if (terminal && !HasActiveSession) return true;
            }
            if (!HasActiveSession)
            {
                error = "활성 게임 세션이 없습니다.";
                return false;
            }
            if (!CanUseSlot(activeRun.difficulty, out error)) return false;
            // Keep the live DTO current before freezing the operation. If this save fails,
            // later gameplay can advance it while the journal retains its original snapshot.
            GameDifficulty sessionDifficulty = activeRun.difficulty;
            if(!TryCaptureRuntimeState(activeRun,out error))return false;
            activeRun.difficulty = sessionDifficulty;
            profile.lastPlayedDifficulty = activeRun.difficulty;
            return Commit("save", activeRun, profile, false, out error);
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

            if(runtimePort==null){error="게임 저장 연결이 준비되지 않았습니다.";return false;}
            if (!runtimePort.TryRestore(activeRun, out error))
                return false;

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

        // Compatibility entry point; production callers use the result-bearing API.
        public static void MarkFinalBossDefeated()
        {
            if (!TryMarkFinalBossDefeated(out string error))
                Debug.LogError("[SaveService] 최종 클리어 저장 실패: " + error);
        }

        public static bool TryMarkFinalBossDefeated(out string error)
        {
            EnsureInitialized();
            if (!TryFinishPendingSave(out error)) return false;
            if (pendingOperation != null)
                return RetryMatchingOperation("final-clear", pendingOperation.difficulty, out error);
            if (activeRun?.campaignCompleted == true && activeRun.bosses?.finalBossDefeated == true)
            {
                error = string.Empty;
                return true;
            }
            if (!HasActiveSession)
            {
                // Direct arena previews have no persistent session, matching the legacy no-op.
                error = string.Empty;
                return true;
            }
            if (!CanUseSlot(activeRun.difficulty, out error)) return false;
            RunSaveData targetRun = Copy(activeRun);
            if(!TryCaptureRuntimeState(targetRun,out error))return false;
            targetRun.difficulty = activeRun.difficulty;
            ProfileSaveData targetProfile = Copy(profile);
            targetRun.bosses ??= new BossProgressSaveData();
            targetRun.bosses.finalBossDefeated = true;
            targetRun.campaignCompleted = true;
            if (targetRun.difficulty == GameDifficulty.Normal)
            {
                targetProfile.normalCampaignCompleted = true;
                targetProfile.hardUnlocked = true;
                targetProfile.normalClearCount++;
            }
            else
            {
                targetProfile.hardClearCount++;
                targetRun.isActive = false;
            }
            return Commit("final-clear", targetRun, targetProfile,
                targetRun.difficulty == GameDifficulty.Hard, out error);
        }

        public static bool TryHandleHardDeath(out string error)
        {
            EnsureInitialized();
            if (!TryFinishPendingSave(out error)) return false;
            if (pendingOperation != null)
                return RetryMatchingOperation("hard-death", GameDifficulty.Hard, out error);
            if (!HasActiveSession || activeRun.difficulty != GameDifficulty.Hard)
            {
                error = "활성 Hard run이 없습니다.";
                return false;
            }
            if (!CanUseSlot(GameDifficulty.Hard, out error)) return false;
            // Preserve the run identity and seeds. Retrying this terminal snapshot must not create a new run.
            RunSaveData cleared = Copy(activeRun);
            cleared.isActive = false;
            cleared.campaignStarted = false;
            cleared.player.items.Clear();
            return Commit("hard-death", cleared, Copy(profile), true, out error);
        }

        public static bool TryPrepareNormalDeathRespawn(out string error)
        {
            EnsureInitialized();
            if (!TryFinishPendingSave(out error)) return false;
            if (pendingOperation != null)
                return RetryMatchingOperation("normal-death", GameDifficulty.Normal, out error);
            if (!HasActiveSession || activeRun.difficulty != GameDifficulty.Normal)
            {
                error = "활성 Normal run이 없습니다.";
                return false;
            }
            if (!CanUseSlot(GameDifficulty.Normal, out error)) return false;
            RunSaveData targetRun = Copy(activeRun);
            if(!TryCaptureRuntimeState(targetRun,out error))return false;
            targetRun.player.currentHealth = -1f;
            targetRun.checkpoint.sceneName = SceneNames.Hub;
            targetRun.checkpoint.biome = BiomeType.None;
            targetRun.checkpoint.wasInBossRoom = false;
            ProfileSaveData targetProfile = Copy(profile);
            targetProfile.lastPlayedDifficulty = GameDifficulty.Normal;
            return Commit("normal-death", targetRun, targetProfile, false, out error);
        }

        public static void ClearActiveSessionReference()
        {
            activeRun = null;
            ResetLoadedVisit();
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
            InitializeStorage(rootPath);
        }

        public static void ResetStaticStateForTests()
        {
            store = null;
            repository = null;
            pendingOperation = null;
            profile = null;
            normal = null;
            hard = null;
            activeRun = null;
            ResetLoadedVisit();
            initialized = false;
            restorePending = false;
        }
#endif

        private static void ActivateRun(RunSaveData run)
        {
            activeRun = run;
            ResetLoadedVisit(true);
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

        private static bool IsValidContinueRun(RunSaveData run, GameDifficulty difficulty)
        {
            return pendingOperation == null && repository != null && !repository.HasBlockingError
                   && string.IsNullOrEmpty(repository.GetRunLoadError(difficulty))
                   && SaveRepository.ValidateRun(run, difficulty)
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
                run.checkpoint?.sceneName ?? SceneNames.Hub,
                run.lastSavedUtcTicks);
        }

        private static bool TryCaptureRuntimeState(RunSaveData run,out string error)
        {
            if(runtimePort==null){error="게임 저장 연결이 준비되지 않았습니다.";return false;}
            runtimePort.Capture(run);error=string.Empty;return true;
        }

        private static bool TryWriteRun(RunSaveData run, out string error)
        {
            if (!TryFinishPendingSave(out error)) return false;
            if (pendingOperation != null)
            {
                error = "완료되지 않은 저장 작업을 먼저 재시도해야 합니다.";
                return false;
            }
            return repository.TryWriteRun(run, out error);
        }

        private static bool SaveProfile(out string error)
        {
            if (!TryFinishPendingSave(out error)) return false;
            if (pendingOperation != null)
            {
                error = "완료되지 않은 저장 작업을 먼저 재시도해야 합니다.";
                return false;
            }
            profile.lastSavedUtcTicks = DateTime.UtcNow.Ticks;
            return repository.TryWriteProfile(profile, out error);
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
                    || PlayerPrefs.GetInt(BossLegacyKeys.Get(biome), 0) != 1)
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
