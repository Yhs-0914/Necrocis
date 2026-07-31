using System;
using System.IO;
using UnityEngine;

namespace Necrocis
{
    internal sealed class SaveFileStore
    {
        private readonly string saveDirectory;

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
            string primaryPath = GetPath(fileName);
            string backupPath = GetBackupPath(primaryPath);

            if (TryReadPath(primaryPath, out value, out error))
            {
                return true;
            }

            string primaryError = error;
            if (TryReadPath(backupPath, out value, out error))
            {
                return true;
            }

            error = string.IsNullOrEmpty(primaryError)
                ? error
                : $"{primaryError} | backup: {error}";
            return false;
        }

        public bool TryWrite<T>(string fileName, T value, out string error) where T : class
        {
            error = string.Empty;
            string targetPath = GetPath(fileName);
            string backupPath = GetBackupPath(targetPath);
            string temporaryPath = targetPath + ".tmp";

            try
            {
                Directory.CreateDirectory(saveDirectory);
                string json = JsonUtility.ToJson(value, true);
                using (FileStream stream = new FileStream(
                           temporaryPath,
                           FileMode.Create,
                           FileAccess.Write,
                           FileShare.None))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(json);
                    writer.Flush();
                    stream.Flush(true);
                }

                if (File.Exists(targetPath))
                {
                    try
                    {
                        File.Replace(temporaryPath, targetPath, backupPath);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        File.Copy(targetPath, backupPath, true);
                        File.Delete(targetPath);
                        File.Move(temporaryPath, targetPath);
                    }
                    catch (IOException)
                    {
                        File.Copy(targetPath, backupPath, true);
                        File.Delete(targetPath);
                        File.Move(temporaryPath, targetPath);
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
        {
            error = string.Empty;
            try
            {
                string path = GetPath(fileName);
                string backup = GetBackupPath(path);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                if (File.Exists(backup))
                {
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

        private static bool TryReadPath<T>(string path, out T value, out string error) where T : class
        {
            value = null;
            error = string.Empty;
            if (!File.Exists(path))
            {
                error = $"파일 없음: {path}";
                return false;
            }

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                {
                    error = $"빈 저장 파일: {path}";
                    return false;
                }

                value = JsonUtility.FromJson<T>(json);
                if (value == null)
                {
                    error = $"JSON 역직렬화 실패: {path}";
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                error = $"{path}: {exception.Message}";
                return false;
            }
        }

        private static string GetBackupPath(string primaryPath)
        {
            string directory = Path.GetDirectoryName(primaryPath);
            string fileName = Path.GetFileNameWithoutExtension(primaryPath);
            string extension = Path.GetExtension(primaryPath);
            return Path.Combine(directory ?? string.Empty, $"{fileName}.backup{extension}");
        }
    }
}
