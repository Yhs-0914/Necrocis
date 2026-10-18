using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Necrocis
{
    public interface IBindingOverrideStore
    {
        bool HasValue { get; }
        string Read();
        void Write(string value);
        void Delete();
        void Preserve(string value);
        void Save();
    }

    public sealed class PlayerPrefsBindingOverrideStore : IBindingOverrideStore
    {
        private readonly string key;
        public PlayerPrefsBindingOverrideStore(string key = "InputRebinds")
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A preference key is required.", nameof(key));
            this.key = key;
        }
        public bool HasValue => PlayerPrefs.HasKey(key);
        public string Read() => PlayerPrefs.GetString(key, string.Empty);
        public void Write(string value) => PlayerPrefs.SetString(key, value);
        public void Delete() => PlayerPrefs.DeleteKey(key);
        public void Save() => PlayerPrefs.Save();
        public void Preserve(string value)
        {
            string digest;
            using (var hash = SHA256.Create()) digest = BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "");
            string backup = key + ".Preserved." + digest.Substring(0, 16);
            if (!PlayerPrefs.HasKey(backup)) PlayerPrefs.SetString(backup, value);
        }
    }
}
