using UnityEngine;

namespace Necrocis
{
    public static class DifficultyBalanceService
    {
        public const string CatalogResourcePath = "Balance/Difficulty/DifficultyBalanceCatalog";

        private static DifficultyBalanceCatalog catalog;
        private static DifficultyBalanceProfile runtimeNormalFallback;
        private static DifficultyBalanceProfile runtimeHardFallback;

        public static GameDifficulty ActiveDifficulty =>
            SaveService.HasActiveSession ? SaveService.ActiveDifficulty : GameDifficulty.Normal;

        public static DifficultyBalanceProfile ActiveProfile => GetProfile(ActiveDifficulty);

        public static DifficultyBalanceProfile GetProfile(GameDifficulty difficulty)
        {
            EnsureLoaded();
            DifficultyBalanceProfile configured = catalog != null ? catalog.Get(difficulty) : null;
            if (configured != null)
            {
                return configured;
            }

            return difficulty == GameDifficulty.Hard
                ? runtimeHardFallback
                : runtimeNormalFallback;
        }

        public static LevelProgressionConfig GetProgressionConfig()
        {
            return ActiveProfile != null ? ActiveProfile.progression : null;
        }

        public static float GetPlayerBaseStatMultiplier(CharacterStatType statType)
        {
            PlayerDifficultyBalance player = ActiveProfile?.player;
            return player != null ? player.GetBaseStatMultiplier(statType) : 1f;
        }

        public static EnemyDifficultyBalance GetEnemyBalance(bool boss)
        {
            DifficultyBalanceProfile profile = ActiveProfile;
            EnemyDifficultyBalance result = boss ? profile?.bosses : profile?.enemies;
            return result ?? new EnemyDifficultyBalance();
        }

        public static float GetIncomingDamageMultiplier(EnemyController sourceEnemy)
        {
            bool boss = sourceEnemy != null
                ? sourceEnemy.IsBossEncounter
                : GameManager.Instance != null
                  && (GameManager.Instance.CurrentState == GameState.InBossRoom
                      || GameManager.Instance.CurrentState == GameState.InFinalBoss);
            return Mathf.Max(0f, GetEnemyBalance(boss).outgoingDamage);
        }

        public static BiomeConfig ResolveBiomeConfig(BiomeType biome, BiomeConfig fallback)
        {
            DifficultyBalanceProfile profile = ActiveProfile;
            return profile != null ? profile.ResolveBiomeConfig(biome, fallback) : fallback;
        }

        public static WorldDifficultyBalance GetWorldBalance(BiomeType biome)
        {
            DifficultyBalanceProfile profile = ActiveProfile;
            return profile != null
                ? profile.ResolveWorldBalance(biome)
                : new WorldDifficultyBalance();
        }

#if UNITY_EDITOR
        public static void ResetForTests()
        {
            catalog = null;
            DestroyFallback(ref runtimeNormalFallback);
            DestroyFallback(ref runtimeHardFallback);
        }
#endif

        private static void EnsureLoaded()
        {
            if (catalog == null)
            {
                catalog = Resources.Load<DifficultyBalanceCatalog>(CatalogResourcePath);
            }

            runtimeNormalFallback ??= CreateFallback(GameDifficulty.Normal);
            runtimeHardFallback ??= CreateFallback(GameDifficulty.Hard);
        }

        private static DifficultyBalanceProfile CreateFallback(GameDifficulty difficulty)
        {
            DifficultyBalanceProfile profile = ScriptableObject.CreateInstance<DifficultyBalanceProfile>();
            profile.hideFlags = HideFlags.HideAndDontSave;
            profile.difficulty = difficulty;
            profile.displayName = difficulty.ToString();
            profile.progression = Resources.Load<LevelProgressionConfig>(
                LevelProgressionConfig.DefaultResourcePath);
            return profile;
        }

#if UNITY_EDITOR
        private static void DestroyFallback(ref DifficultyBalanceProfile profile)
        {
            if (profile != null)
            {
                Object.DestroyImmediate(profile);
                profile = null;
            }
        }
#endif
    }
}
