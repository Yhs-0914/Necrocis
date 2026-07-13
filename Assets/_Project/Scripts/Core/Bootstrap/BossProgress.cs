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
            string key = GetKey(biome);
            return !string.IsNullOrEmpty(key) && PlayerPrefs.GetInt(key, 0) == 1;
        }

        public static void MarkDefeated(BiomeType biome)
        {
            string key = GetKey(biome);
            if (string.IsNullOrEmpty(key) || PlayerPrefs.GetInt(key, 0) == 1)
            {
                return;
            }

            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }

        private static string GetKey(BiomeType biome)
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
