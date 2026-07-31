using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// Keeps the four biome-boss clear flags between game sessions.
    /// </summary>
    public static class BossProgress
    {
        private const string KeyPrefix = "necrocis.boss-defeated.";

        public static bool IsDefeated(BiomeType biome)
        {
            string key = GetLegacyKey(biome);
            if (SaveService.HasActiveSession)
            {
                return SaveService.IsBossDefeated(biome);
            }

            return SaveService.IsBossDiscovered(biome)
                   || (!string.IsNullOrEmpty(key) && PlayerPrefs.GetInt(key, 0) == 1);
        }

        public static void MarkDefeated(BiomeType biome)
        {
            string key = GetLegacyKey(biome);
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            SaveService.MarkBossDefeated(biome);
        }

        public static void ResetAll()
        {
            PlayerPrefs.DeleteKey(GetLegacyKey(BiomeType.Intestine));
            PlayerPrefs.DeleteKey(GetLegacyKey(BiomeType.Liver));
            PlayerPrefs.DeleteKey(GetLegacyKey(BiomeType.Stomach));
            PlayerPrefs.DeleteKey(GetLegacyKey(BiomeType.Lung));
            PlayerPrefs.Save();
            SaveService.ResetBossDiscoveriesForDevelopment();
        }

        internal static string GetLegacyKey(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Intestine => KeyPrefix + "intestine",
                BiomeType.Liver => KeyPrefix + "liver",
                BiomeType.Stomach => KeyPrefix + "stomach",
                BiomeType.Lung => KeyPrefix + "lung",
                _ => string.Empty
            };
        }
    }
}
