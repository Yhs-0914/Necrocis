using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    // File/schema policy only. Runtime capture, scene loading and session transitions belong to the facade's adapter.
    public sealed class SaveRepository
    {
        public const string ProfileFileName = "profile.json";
        public const string NormalFileName = "normal.json";
        public const string HardFileName = "hard-run.json";
        public const string PendingFileName = "save-pending.json";
        private readonly SaveFileStore store;
        private readonly Dictionary<GameDifficulty, string> runErrors = new Dictionary<GameDifficulty, string>();
        private string profileError, journalError, lastError;
        private SaveOperation pending;

        public bool HasBlockingError => !string.IsNullOrEmpty(profileError) || !string.IsNullOrEmpty(journalError);
        public string LastError
        {
            get
            {
                if (HasBlockingError) return BlockingError;
                if (!string.IsNullOrEmpty(lastError)) return lastError;
                string normalError = GetRunLoadError(GameDifficulty.Normal);
                return !string.IsNullOrEmpty(normalError) ? normalError : GetRunLoadError(GameDifficulty.Hard);
            }
            private set => lastError = value;
        }
        public SaveOperation Pending => Copy(pending);

        public SaveRepository(SaveFileStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string GetRunLoadError(GameDifficulty difficulty)
            => runErrors.TryGetValue(difficulty, out string error) ? error : string.Empty;

        public ProfileSaveData LoadProfile(out string error)
        {
            ProfileSaveData result = ReadValidated<ProfileSaveData>(ProfileFileName, ValidateProfile, p => p.schemaVersion, out bool missing, out error);
            if (missing) result = new ProfileSaveData { lastSavedUtcTicks = DateTime.UtcNow.Ticks };
            profileError = result == null ? error : string.Empty;
            if (result != null) result.bossDiscoveries ??= new BossProgressSaveData();
            if (!string.IsNullOrEmpty(error)) LastError = error;
            return result;
        }

        public RunSaveData LoadRun(string fileName, GameDifficulty difficulty, out string error)
        {
            if (fileName != RunFile(difficulty))
            {
                error = "Run filename does not match difficulty: " + fileName;
                runErrors[difficulty] = LastError = error;
                return null;
            }
            RunSaveData result = ReadValidated<RunSaveData>(fileName, r => ValidateRun(r, difficulty), r => r.schemaVersion, out _, out error);
            if (result != null) NormalizeRun(result);
            runErrors[difficulty] = error;
            if (!string.IsNullOrEmpty(error)) LastError = error;
            return result;
        }

        public static bool ValidateProfile(ProfileSaveData value)
            => value != null && value.schemaVersion > 0 && value.schemaVersion <= SaveSchema.CurrentVersion;

        public static bool ValidateRun(RunSaveData value, GameDifficulty difficulty)
            => value != null && ValidDifficulty(difficulty) && value.schemaVersion > 0 && value.schemaVersion <= SaveSchema.CurrentVersion
                && value.difficulty == difficulty && !string.IsNullOrWhiteSpace(value.saveId)
                && ValidateHealthDeltas(value.player?.healthDeltas);

        private static bool ValidateHealthDeltas(List<HealthDeltaRemainderData> entries)
        {
            if (entries == null) return true; // Existing saves omit this optional field.
            var sources = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in entries)
                if (entry == null || string.IsNullOrWhiteSpace(entry.sourceId) || !sources.Add(entry.sourceId)
                    || !HealthDeltaAccumulator.ValidRemainder(entry.damage) || !HealthDeltaAccumulator.ValidRemainder(entry.heal)) return false;
            return true;
        }

        public static void NormalizeRun(RunSaveData run)
        {
            if (run == null) return;
            run.player ??= new PlayerProgressSaveData();
            run.player.selectionHistory ??= new List<LevelUpStatChoice>();
            run.player.levelUpModifiers ??= new List<SavedStatModifierData>();
            run.player.items ??= new List<SavedItemStateData>();
            run.player.healthDeltas ??= new List<HealthDeltaRemainderData>();
            run.bosses ??= new BossProgressSaveData();
            run.checkpoint ??= new ResumeCheckpointSaveData();
            run.world ??= new WorldRunSaveData();
            run.world.biomeElitePlans ??= new List<BiomeElitePlan>();
            run.world.defeatedBiomeEliteIds ??= new List<string>();
            run.world.biomeElitePlans.RemoveAll(p => p == null);
            foreach (BiomeElitePlan plan in run.world.biomeElitePlans)
            {
                plan.placements ??= new List<BiomeElitePlacement>();
                plan.placements.RemoveAll(e => e == null || string.IsNullOrEmpty(e.spawnId) || string.IsNullOrEmpty(e.monsterId));
            }
        }

        public bool TryWriteRun(RunSaveData run, out string error)
        {
            if (run == null || !ValidateRun(run, run.difficulty)) return Fail("Invalid run target", out error);
            if (!CanStartWrite(out error)) return false;
            LoadRun(RunFile(run.difficulty), run.difficulty, out error);
            if (!string.IsNullOrEmpty(error)) return false;
            RunSaveData target = Copy(run);
            NormalizeRun(target);
            target.schemaVersion = SaveSchema.CurrentVersion;
            if (!WriteTarget(RunFile(target.difficulty), target, r => ValidateRun(r, target.difficulty), out error))
                return Fail(error, out error);
            return Success(out error);
        }

        public bool TryWriteProfile(ProfileSaveData profile, out string error)
        {
            if (!ValidateProfile(profile)) return Fail("Invalid profile target", out error);
            if (!CanStartWrite(out error)) return false;
            if (LoadProfile(out error) == null) return false;
            ProfileSaveData target = Copy(profile);
            target.schemaVersion = SaveSchema.CurrentVersion;
            if (!WriteTarget(ProfileFileName, target, ValidateProfile, out error)) return Fail(error, out error);
            return Success(out error);
        }

        public bool TryCommit(SaveOperation operation, out string error)
        {
            if (operation == null) return Fail("Save operation is missing", out error);
            SaveOperation disk = ReadPending(out error);
            if (!string.IsNullOrEmpty(error)) return false;
            SaveOperation fixedTarget = disk ?? pending;
            if (fixedTarget != null)
            {
                if (operation.id != fixedTarget.id || operation.kind != fixedTarget.kind
                    || operation.sourceRunId != fixedTarget.sourceRunId || operation.runFile != fixedTarget.runFile)
                    return Fail("A different save operation is still pending: " + fixedTarget.id, out error);
                // An existing ID always resumes its frozen payload, never caller-mutated retry data.
            }
            else
            {
                fixedTarget = Copy(operation);
                if (!ValidateOperation(fixedTarget, out error)) return Fail(error, out error);
                NormalizeRun(fixedTarget.run);
                fixedTarget.run.schemaVersion = SaveSchema.CurrentVersion;
                fixedTarget.profile.schemaVersion = SaveSchema.CurrentVersion;
                if (!string.IsNullOrEmpty(journalError)) return Fail(journalError, out error);
                if (LoadProfile(out error) == null) return false;
                LoadRun(fixedTarget.runFile, fixedTarget.difficulty, out error);
                if (!string.IsNullOrEmpty(error)) return false;
                pending = fixedTarget;
            }

            if (disk == null && !store.TryCreatePending(PendingFileName, fixedTarget, out error))
                return Fail(error, out error);
            if (!ApplyFixedTarget(fixedTarget, out error)) return Fail(error, out error);
            return Success(out error);
        }

        public SaveOperation ReadPending(out string error)
        {
            SaveFileReadStatus status = store.ReadCandidate(PendingFileName, out SaveOperation value, out error, out string json);
            if (status == SaveFileReadStatus.Missing)
            {
                error = journalError = string.Empty;
                return null;
            }
            bool validHeader = false;
            if (status == SaveFileReadStatus.Valid)
            {
                var header = JsonUtility.FromJson<OperationHeader>(json);
                validHeader = header != null && header.formatVersion == 1 && header.profile != null && header.profile.schemaVersion > 0
                    && header.run != null && header.run.schemaVersion > 0;
                if (!validHeader) error = "Pending operation is missing its explicit format/schema headers";
            }
            if (status != SaveFileReadStatus.Valid || !validHeader || !ValidateOperation(value, out error))
            {
                if (string.IsNullOrEmpty(error)) error = "Pending save operation is invalid";
                journalError = LastError = error;
                return null;
            }
            journalError = string.Empty;
            pending = Copy(value);
            return Copy(value);
        }

        // Called before ordinary profile/slot loads. A valid intent can repair half-written targets.
        public bool TryRecover(out SaveOperation recovered, out string error)
        {
            recovered = null;
            SaveOperation operation = ReadPending(out error);
            if (!string.IsNullOrEmpty(error)) return false;
            if (operation == null) return Success(out error);
            if (!ApplyFixedTarget(operation, out error))
            {
                journalError = LastError = error;
                return false;
            }
            recovered = Copy(operation);
            return Success(out error);
        }

        private bool ApplyFixedTarget(SaveOperation operation, out string error)
        {
            // Do not overwrite files from a newer application even if an older journal remains.
            if (!CheckFutureFiles<RunSaveData>(operation.runFile, r => r.schemaVersion, out error)
                || !CheckFutureFiles<ProfileSaveData>(ProfileFileName, p => p.schemaVersion, out error)) return false;
            if (!WriteTarget(operation.runFile, operation.run, r => ValidateRun(r, operation.difficulty), out error)) return false;
            if (operation.terminal)
            {
                if (!store.TryWriteTerminalBackup(operation.runFile, operation.run, out error)) return false;
                if (!Verify(SaveFileStore.GetBackupFileName(operation.runFile), operation.run,
                        r => ValidateRun(r, operation.difficulty) && !r.isActive, out error)) return false;
            }
            if (!WriteTarget(ProfileFileName, operation.profile, ValidateProfile, out error)) return false;
            if (!Verify(operation.runFile, operation.run, r => ValidateRun(r, operation.difficulty), out error)
                || !Verify(ProfileFileName, operation.profile, ValidateProfile, out error)) return false;
            if (!store.DeleteSingle(PendingFileName, out error)) return false;
            pending = null;
            journalError = profileError = string.Empty;
            runErrors[operation.difficulty] = string.Empty;
            return true;
        }

        private bool CanStartWrite(out string error)
        {
            SaveOperation disk = ReadPending(out error);
            if (!string.IsNullOrEmpty(error)) return false;
            if (disk != null || pending != null) return Fail("A save operation must finish before another write", out error);
            if (!string.IsNullOrEmpty(journalError)) return Fail(journalError, out error);
            if (!string.IsNullOrEmpty(profileError) && LoadProfile(out error) == null) return false;
            return true;
        }

        private string BlockingError => !string.IsNullOrEmpty(journalError) ? journalError : profileError;

        private T ReadValidated<T>(string fileName, Func<T, bool> validate, Func<T, int> schema,
            out bool missing, out string error) where T : class
        {
            missing = false;
            SaveFileReadStatus primary = ReadDataCandidate(fileName, out T value, out string primaryError);
            if (primary == SaveFileReadStatus.IoError) { error = primaryError; return null; }
            if (primary == SaveFileReadStatus.Valid)
            {
                if (schema(value) > SaveSchema.CurrentVersion)
                { error = "Unsupported future schema in " + fileName + ": " + schema(value); return null; }
                if (validate(value)) { error = string.Empty; return value; }
                primaryError = "Semantic validation failed: " + fileName;
            }
            string backupFile = SaveFileStore.GetBackupFileName(fileName);
            SaveFileReadStatus backup = ReadDataCandidate(backupFile, out value, out string backupError);
            if (backup == SaveFileReadStatus.Valid)
            {
                if (schema(value) > SaveSchema.CurrentVersion)
                { error = "Unsupported future schema in " + backupFile + ": " + schema(value); return null; }
                if (validate(value)) { error = string.Empty; return value; }
                backupError = "Semantic validation failed: " + backupFile;
            }
            if (primary == SaveFileReadStatus.Missing && backup == SaveFileReadStatus.Missing)
            { missing = true; error = string.Empty; return null; }
            error = primaryError + " | backup: " + backupError;
            return null;
        }

        private bool WriteTarget<T>(string fileName, T target, Func<T, bool> validate, out string error) where T : class
        {
            SaveFileReadStatus current = ReadDataCandidate(fileName, out T value, out error);
            if (current == SaveFileReadStatus.IoError) return false;
            // An invalid primary must not replace a previously valid recovery backup.
            bool written = current == SaveFileReadStatus.Valid && validate(value)
                ? store.TryWrite(fileName, target, out error)
                : store.TryWritePreservingBackup(fileName, target, out error);
            return written && Verify(fileName, target, validate, out error);
        }

        private bool Verify<T>(string fileName, T expected, Func<T, bool> validate, out string error) where T : class
        {
            if (ReadDataCandidate(fileName, out T actual, out error) != SaveFileReadStatus.Valid) return false;
            if (!validate(actual) || JsonUtility.ToJson(actual) != JsonUtility.ToJson(expected))
            { error = "Written snapshot verification failed: " + fileName; return false; }
            return true;
        }

        private bool CheckFutureFiles<T>(string fileName, Func<T, int> schema, out string error) where T : class
        {
            foreach (string candidate in new[] { fileName, SaveFileStore.GetBackupFileName(fileName) })
            {
                SaveFileReadStatus status = ReadDataCandidate(candidate, out T value, out error);
                if (status == SaveFileReadStatus.IoError) return false;
                if (status == SaveFileReadStatus.Valid && schema(value) > SaveSchema.CurrentVersion)
                { error = "Recovery cannot overwrite future schema: " + candidate; return false; }
            }
            error = string.Empty;
            return true;
        }

        private static bool ValidateOperation(SaveOperation operation, out string error)
        {
            error = string.Empty;
            if (operation == null || operation.formatVersion != 1 || string.IsNullOrWhiteSpace(operation.id)
                || operation.run == null || operation.profile == null || !ValidDifficulty(operation.difficulty)
                || operation.runFile != RunFile(operation.difficulty)
                || !ValidateRun(operation.run, operation.difficulty) || !ValidateProfile(operation.profile))
            { error = "Invalid or unsupported pending operation snapshot"; return false; }
            switch (operation.kind)
            {
                case "save": case "new-game": case "continue": case "normal-death": case "hard-death": case "final-clear": break;
                default: error = "Unsupported save operation kind: " + operation.kind; return false;
            }
            if (operation.terminal && (operation.difficulty != GameDifficulty.Hard || operation.run.isActive
                || string.IsNullOrWhiteSpace(operation.sourceRunId)))
            { error = "Terminal save requires an inactive Hard run and its original run ID"; return false; }
            if (operation.difficulty == GameDifficulty.Hard && !operation.run.isActive && !operation.terminal)
            { error = "Inactive Hard operation must update its recovery backup"; return false; }
            return true;
        }

        [Serializable] private sealed class SchemaHeader { public int schemaVersion = 0; }
        [Serializable] private sealed class OperationHeader
        {
            public int formatVersion = 0;
            public SchemaHeader profile = null, run = null;
        }

        private SaveFileReadStatus ReadDataCandidate<T>(string fileName, out T value, out string error) where T : class
        {
            SaveFileReadStatus status = store.ReadCandidate(fileName, out value, out error, out string json);
            if (status != SaveFileReadStatus.Valid) return status;
            // The DTO's schema initializer must not make a header-less '{}' look like schema 2.
            SchemaHeader header = JsonUtility.FromJson<SchemaHeader>(json);
            if (header == null || header.schemaVersion <= 0)
            { error = "Missing or invalid explicit schemaVersion: " + fileName; return SaveFileReadStatus.Invalid; }
            return status;
        }

        private static bool ValidDifficulty(GameDifficulty difficulty)
            => difficulty == GameDifficulty.Normal || difficulty == GameDifficulty.Hard;

        private static string RunFile(GameDifficulty difficulty)
            => difficulty == GameDifficulty.Normal ? NormalFileName : HardFileName;

        private static T Copy<T>(T value) where T : class
            => value == null ? null : JsonUtility.FromJson<T>(JsonUtility.ToJson(value));

        private bool Fail(string message, out string error)
        { error = LastError = message; return false; }

        private bool Success(out string error)
        { error = string.Empty; LastError = HasBlockingError ? BlockingError : string.Empty; return true; }

#if UNITY_EDITOR
        [Serializable]
        public sealed class DiagnosticSnapshot
        {
            public string profileError, journalError, normalError, hardError, lastError;
            public SaveOperation pending;
        }

        public DiagnosticSnapshot CaptureDiagnosticsForTests() => new DiagnosticSnapshot {
            profileError = profileError, journalError = journalError, lastError = this.lastError,
            normalError = GetRunLoadError(GameDifficulty.Normal), hardError = GetRunLoadError(GameDifficulty.Hard), pending = Copy(pending)
        };

        public void RestoreDiagnosticsForTests(DiagnosticSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            profileError = snapshot.profileError; journalError = snapshot.journalError; LastError = snapshot.lastError;
            runErrors[GameDifficulty.Normal] = snapshot.normalError; runErrors[GameDifficulty.Hard] = snapshot.hardError;
            pending = Copy(snapshot.pending);
        }
#endif
    }
}
