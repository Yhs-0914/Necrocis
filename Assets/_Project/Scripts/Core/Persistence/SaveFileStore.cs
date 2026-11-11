using System;
using System.IO;
using UnityEngine;

namespace Necrocis
{
    internal enum SaveFileReadStatus { Missing, Valid, Invalid, IoError }

    public sealed class SaveFileStore
    {
        private readonly string saveDirectory;

#if UNITY_EDITOR
        public static Action<string, string> FaultInjector;
#endif

        public SaveFileStore(string rootPath)
        {
            saveDirectory = Path.Combine(rootPath, "Saves");
        }

        public string GetPath(string fileName)
        {
            return Path.Combine(saveDirectory, fileName);
        }

        public bool TryRead<T>(string fileName, out T value, out string error) where T : class
        {
            if (TryReadSingle(fileName, out value, out _, out error))
            {
                return true;
            }

            string primaryError = error;
            if (TryReadSingle(GetBackupFileName(fileName), out value, out _, out error))
            {
                return true;
            }

            error = string.IsNullOrEmpty(primaryError)
                ? error
                : $"{primaryError} | backup: {error}";
            return false;
        }

        public bool TryWrite<T>(string fileName, T value, out string error) where T : class
            => TryWriteFile(fileName, value, true, false, out error);

        public bool TryWritePreservingBackup<T>(string fileName, T value, out string error) where T : class
            => TryWriteFile(fileName, value, false, false, out error);

        // A pending operation has no rotating backup: an old journal must never be replayed.
        public bool TryCreatePending<T>(string fileName, T value, out string error) where T : class
            => TryWriteFile(fileName, value, false, true, out error);

        public bool TryWriteTerminalBackup<T>(string fileName, T value, out string error) where T : class
        {
            try { Inject(fileName, "terminal-backup"); }
            catch (Exception exception)
            {
                error = exception.Message;
                Debug.LogError($"[SaveFileStore] '{fileName}' 종료 백업 저장 실패: {exception}");
                return false;
            }
            return TryWriteFile(GetBackupFileName(fileName), value, false, false, out error);
        }

        private bool TryWriteFile<T>(string fileName, T value, bool retainBackup, bool createOnly, out string error) where T : class
        {
            error = string.Empty;
            string targetPath = GetPath(fileName);
            string backupPath = retainBackup ? GetPath(GetBackupFileName(fileName)) : null;
            string temporaryPath = targetPath + ".tmp";

            try
            {
                Inject(fileName, "mkdir");
                Directory.CreateDirectory(saveDirectory);
                string json = JsonUtility.ToJson(value, true);
                Inject(fileName, "write");
                using (FileStream stream = new FileStream(
                           temporaryPath,
                           FileMode.Create,
                           FileAccess.Write,
                           FileShare.None))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(json);
                    Inject(fileName, "flush");
                    writer.Flush();
                    stream.Flush(true);
                }

                // Inject outside File.Replace's IOException fallback so a requested failure stays a failure.
                if (retainBackup && File.Exists(targetPath)) Inject(fileName, "backup");
                Inject(fileName, "replace");
                if (createOnly)
                {
                    File.Move(temporaryPath, targetPath);
                }
                else if (File.Exists(targetPath))
                {
                    try
                    {
                        File.Replace(temporaryPath, targetPath, backupPath);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        ReplaceFallback(fileName, temporaryPath, targetPath, backupPath);
                    }
                    catch (IOException)
                    {
                        ReplaceFallback(fileName, temporaryPath, targetPath, backupPath);
                    }
                }
                else
                {
                    File.Move(temporaryPath, targetPath);
                }

                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                Debug.LogError($"[SaveFileStore] '{fileName}' 저장 실패: {exception}");
                return false;
            }
            finally
            {
                try
                {
                    if (File.Exists(temporaryPath))
                    {
                        File.Delete(temporaryPath);
                    }
                }
                catch (Exception)
                {
                    // A stale temp file is harmless and will be replaced on the next save.
                }
            }
        }

        public bool Delete(string fileName, out string error)
            => DeleteFile(fileName, true, out error);

        public bool DeleteSingle(string fileName, out string error)
            => DeleteFile(fileName, false, out error);

        private bool DeleteFile(string fileName, bool includeBackup, out string error)
        {
            error = string.Empty;
            try
            {
                string path = GetPath(fileName);
                string backupFile = GetBackupFileName(fileName);
                string backup = GetPath(backupFile);
                if (File.Exists(path))
                {
                    Inject(fileName, "delete");
                    File.Delete(path);
                }

                if (includeBackup && File.Exists(backup))
                {
                    Inject(backupFile, "delete");
                    File.Delete(backup);
                }

                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                Debug.LogError($"[SaveFileStore] '{fileName}' 삭제 실패: {exception}");
                return false;
            }
        }

        public bool TryReadSingle<T>(string fileName, out T value, out bool exists, out string error) where T : class
        {
            SaveFileReadStatus status = ReadCandidate(fileName, out value, out error);
            exists = status != SaveFileReadStatus.Missing;
            return status == SaveFileReadStatus.Valid;
        }

        internal SaveFileReadStatus ReadCandidate<T>(string fileName, out T value, out string error) where T : class
            => ReadCandidate(fileName, out value, out error, out _);

        internal SaveFileReadStatus ReadCandidate<T>(string fileName, out T value, out string error, out string json) where T : class
        {
            value = null;
            json = null;
            error = string.Empty;
            string path = GetPath(fileName);
            try
            {
                Inject(fileName, "read");
                // File.Exists suppresses access errors; preserve missing versus unreadable here.
                File.GetAttributes(path);
                json = File.ReadAllText(path);
            }
            catch (FileNotFoundException) { error = $"파일 없음: {path}"; return SaveFileReadStatus.Missing; }
            catch (DirectoryNotFoundException) { error = $"파일 없음: {path}"; return SaveFileReadStatus.Missing; }
            catch (Exception exception)
            {
                error = $"{path}: {exception.Message}";
                Debug.LogError($"[SaveFileStore] '{fileName}' 읽기 실패: {exception}");
                return SaveFileReadStatus.IoError;
            }
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    error = $"빈 저장 파일: {path}";
                    return SaveFileReadStatus.Invalid;
                }

                value = JsonUtility.FromJson<T>(json);
                if (value == null)
                {
                    error = $"JSON 역직렬화 실패: {path}";
                    return SaveFileReadStatus.Invalid;
                }

                return SaveFileReadStatus.Valid;
            }
            catch (Exception exception)
            {
                error = $"{path}: {exception.Message}";
                return SaveFileReadStatus.Invalid;
            }
        }

        public static string GetBackupFileName(string fileName)
        {
            string leaf = Path.GetFileNameWithoutExtension(fileName) + ".backup" + Path.GetExtension(fileName);
            return Path.Combine(Path.GetDirectoryName(fileName) ?? string.Empty, leaf);
        }

        private static void ReplaceFallback(string fileName, string temporary, string target, string backup)
        {
            if (backup != null)
            {
                Inject(fileName, "fallback-copy");
                File.Copy(target, backup, true);
            }
            Inject(fileName, "fallback-delete");
            File.Delete(target);
            Inject(fileName, "fallback-move");
            File.Move(temporary, target);
        }

        private static void Inject(string fileName, string point)
        {
#if UNITY_EDITOR
            FaultInjector?.Invoke(fileName, point);
#endif
        }
    }
}
