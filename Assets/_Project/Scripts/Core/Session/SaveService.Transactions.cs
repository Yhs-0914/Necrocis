using System;
using UnityEngine;

namespace Necrocis
{
    public static partial class SaveService
    {
        public static bool HasPendingFinalClear => pendingOperation?.kind == "final-clear";
        public static GameDifficulty PendingOperationDifficulty => pendingOperation?.difficulty ?? GameDifficulty.Normal;
        public static bool HasBlockingStorageIssue
        {
            get { EnsureInitialized(); return pendingOperation != null || repository.HasBlockingError; }
        }
        public static bool HasStorageRecoveryIssue
        {
            get { EnsureInitialized(); return pendingOperation != null || repository.HasBlockingError
                || !string.IsNullOrEmpty(repository.GetRunLoadError(GameDifficulty.Normal))
                || !string.IsNullOrEmpty(repository.GetRunLoadError(GameDifficulty.Hard)); }
        }
        public static bool HasPendingHardDeath => pendingOperation?.kind == "hard-death";
        public static bool HasPendingTerminalOperation => pendingOperation != null
            && (pendingOperation.kind == "hard-death" || pendingOperation.kind == "final-clear");
        public static string LastStorageError { get { EnsureInitialized(); return HasStorageRecoveryIssue ? repository.LastError : string.Empty; } }

        private static void InitializeStorage(string rootPath)
        {
            store = new SaveFileStore(rootPath);
            repository = new SaveRepository(store);
            repository.TryRecover(out _, out _);
            pendingOperation = repository.Pending;
            // Invalid files remain protected by repository diagnostics. The default profile
            // is a read model for locked menus only, never permission to overwrite those files.
            profile = repository.LoadProfile(out _) ?? new ProfileSaveData();
            normal = repository.LoadRun(NormalFileName, GameDifficulty.Normal, out _);
            hard = repository.LoadRun(HardFileName, GameDifficulty.Hard, out _);
            activeRun = null;
            ResetLoadedVisit();
            restorePending = false;
            initialized = true;
            if (!repository.HasBlockingError && pendingOperation == null) MigrateLegacyBossDiscoveries();
        }

        private static T Copy<T>(T value) where T : class
            => value == null ? null : JsonUtility.FromJson<T>(JsonUtility.ToJson(value));

        private static bool TryFinishPendingSave(out string error)
        {
            error = string.Empty;
            return pendingOperation?.kind != "save" || TryRetryPendingOperation(out error);
        }

        // Main-menu recovery re-reads repaired/transiently unavailable files without
        // activating a run. It never replaces an invalid file with a new-game default.
        public static bool TryRecoverStorage(out string error)
        {
            EnsureInitialized();
            if (HasActiveSession)
            {
                error = "게임 세션을 종료한 뒤 저장 복구를 재시도해야 합니다.";
                return false;
            }
            if (pendingOperation != null)
            {
                // Recovery is file-only: completing a transition here must never
                // activate its run, including when a later read fails.
                if (!repository.TryCommit(pendingOperation, out error))
                {
                    pendingOperation = repository.Pending ?? pendingOperation;
                    return false;
                }
                pendingOperation = null;
                activeRun = null;
                restorePending = false;
                ResetLoadedVisit();
            }
            if (!repository.TryRecover(out _, out error))
            {
                pendingOperation = repository.Pending;
                return false;
            }
            pendingOperation = repository.Pending;
            profile = repository.LoadProfile(out _) ?? new ProfileSaveData();
            normal = repository.LoadRun(NormalFileName, GameDifficulty.Normal, out _);
            hard = repository.LoadRun(HardFileName, GameDifficulty.Hard, out _);
            activeRun = null;
            restorePending = false;
            ResetLoadedVisit();
            error = HasStorageRecoveryIssue ? repository.LastError : string.Empty;
            return !HasStorageRecoveryIssue;
        }

        private static bool CanUseSlot(GameDifficulty difficulty, out string error)
        {
            error = repository.HasBlockingError ? repository.LastError : repository.GetRunLoadError(difficulty);
            if (string.IsNullOrEmpty(error) && pendingOperation != null)
                error = "완료되지 않은 저장 작업을 먼저 재시도해야 합니다.";
            return string.IsNullOrEmpty(error);
        }

        private static bool Commit(string kind, RunSaveData run, ProfileSaveData targetProfile,
            bool terminal, out string error)
        {
            long now = DateTime.UtcNow.Ticks;
            run.lastSavedUtcTicks = targetProfile.lastSavedUtcTicks = now;
            pendingOperation = new SaveOperation {
                id = Guid.NewGuid().ToString("N"), kind = kind,
                sourceRunId = activeRun?.saveId ?? run.saveId,
                difficulty = run.difficulty,
                runFile = run.difficulty == GameDifficulty.Normal ? NormalFileName : HardFileName,
                terminal = terminal, run = Copy(run), profile = Copy(targetProfile)
            };
            return TryRetryPendingOperation(out error);
        }

        private static bool RetryMatchingOperation(string kind, GameDifficulty difficulty, out string error)
        {
            if (pendingOperation.kind != kind || pendingOperation.difficulty != difficulty)
            {
                error = "완료되지 않은 저장 작업을 먼저 재시도해야 합니다: " + pendingOperation.kind;
                return false;
            }
            return TryRetryPendingOperation(out error);
        }

        public static bool TryRetryPendingOperation(out string error)
        {
            EnsureInitialized();
            SaveOperation operation = pendingOperation;
            if (operation == null)
            {
                if (!repository.TryRecover(out operation, out error)) return false;
                if (operation == null)
                {
                    error = repository.HasBlockingError ? repository.LastError : string.Empty;
                    return !repository.HasBlockingError;
                }
            }
            else if (!repository.TryCommit(operation, out error))
            {
                pendingOperation = repository.Pending ?? operation;
                return false;
            }
            ApplyCommittedOperation(operation);
            pendingOperation = null;
            error = string.Empty;
            return true;
        }

        private static void ApplyCommittedOperation(SaveOperation operation)
        {
            if (operation.kind == "save" && activeRun != null && activeRun.saveId == operation.sourceRunId)
            {
                // Completing an older autosave must not roll back later live world/boss
                // changes. A new save request captures gameplay again after this returns.
                if (activeRun.difficulty == GameDifficulty.Normal) normal = activeRun;
                else hard = activeRun;
                return;
            }
            profile = Copy(operation.profile);
            RunSaveData run = Copy(operation.run);
            if (operation.difficulty == GameDifficulty.Normal) normal = run;
            else hard = run;

            switch (operation.kind)
            {
                case "new-game":
                case "continue":
                    ActivateRun(run);
                    break;
                case "hard-death":
                    activeRun = null;
                    ResetLoadedVisit();
                    restorePending = false;
                    break;
                case "normal-death":
                    activeRun = run;
                    restorePending = true;
                    break;
                default:
                    if (activeRun == null || activeRun.saveId == operation.sourceRunId)
                        activeRun = run;
                    if (operation.terminal) { ResetLoadedVisit(); restorePending = false; }
                    break;
            }
        }

#if UNITY_EDITOR
        public static IDisposable InjectStorageFaultsForTests(Action<string, string> hook)
            => new StorageFaultScope(hook);

        private sealed class StorageFaultScope : IDisposable
        {
            private readonly Action<string, string> previous;
            private bool disposed;
            public StorageFaultScope(Action<string, string> hook)
            {
                previous = SaveFileStore.FaultInjector;
                SaveFileStore.FaultInjector = hook;
            }
            public void Dispose()
            {
                if (disposed) return;
                disposed = true;
                SaveFileStore.FaultInjector = previous;
            }
        }

        // Used only to preserve an existing editor test fixture across script reloads.
        // This restores in-memory diagnostics and never opens or writes user files.
        public static string CaptureRepositoryDiagnosticsForTests()
            => repository == null ? null : JsonUtility.ToJson(repository.CaptureDiagnosticsForTests());

        public static void RestoreRepositoryDiagnosticsForTests(string json)
        {
            repository = store == null ? null : new SaveRepository(store);
            if (repository != null && !string.IsNullOrEmpty(json))
                repository.RestoreDiagnosticsForTests(JsonUtility.FromJson<SaveRepository.DiagnosticSnapshot>(json));
        }
#endif
    }
}
