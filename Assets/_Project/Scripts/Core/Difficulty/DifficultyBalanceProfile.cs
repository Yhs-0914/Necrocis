using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(
        menuName = "Necrocis/Balance/Difficulty Balance Profile",
        fileName = "DifficultyBalanceProfile")]
    public sealed class DifficultyBalanceProfile : ScriptableObject
    {
        public GameDifficulty difficulty = GameDifficulty.Normal;
        public string displayName = "Normal";
        [TextArea(2, 4)] public string description;

        [Header("Progression")]
        public LevelProgressionConfig progression;

        [Header("Player")]
        public PlayerDifficultyBalance player = new PlayerDifficultyBalance();

        [Header("Normal Enemies")]
        public EnemyDifficultyBalance enemies = new EnemyDifficultyBalance();

        [Header("Bosses")]
        public EnemyDifficultyBalance bosses = new EnemyDifficultyBalance();

        [Header("Items")]
        public ItemDifficultyBalance items = new ItemDifficultyBalance();

        [Header("World")]
        public WorldDifficultyBalance world = new WorldDifficultyBalance();

        [Header("Optional Biome World Overrides")]
        public List<DifficultyBiomeWorldOverride> biomeWorldOverrides = new List<DifficultyBiomeWorldOverride>();

        [Header("Optional Full Biome Overrides")]
        [Tooltip("지정하면 해당 난이도는 씬에 연결된 BiomeConfig 대신 이 에셋을 사용합니다.")]
        public List<DifficultyBiomeOverride> biomeOverrides = new List<DifficultyBiomeOverride>();

        public BiomeConfig ResolveBiomeConfig(BiomeType biome, BiomeConfig fallback)
        {
            if (biomeOverrides != null)
            {
                for (int index = 0; index < biomeOverrides.Count; index++)
                {
                    DifficultyBiomeOverride entry = biomeOverrides[index];
                    if (entry != null && entry.biome == biome && entry.config != null)
                    {
                        return entry.config;
                    }
                }
            }

            return fallback;
        }

        public WorldDifficultyBalance ResolveWorldBalance(BiomeType biome)
        {
            if (biomeWorldOverrides != null)
            {
                for (int index = 0; index < biomeWorldOverrides.Count; index++)
                {
                    DifficultyBiomeWorldOverride entry = biomeWorldOverrides[index];
                    if (entry != null && entry.biome == biome && entry.world != null)
                    {
                        return entry.world;
                    }
                }
            }

            return world ?? new WorldDifficultyBalance();
        }
    }

    [Serializable]
    public sealed class PlayerDifficultyBalance
    {
        [Header("Base Stat Multipliers")]
        [Min(0.01f)] public float maxHealth = 1f;
        [Min(0.01f)] public float moveSpeed = 1f;
        [Min(0.01f)] public float attackPower = 1f;
        [Min(0.01f)] public float attackSpeed = 1f;
        [Min(0.01f)] public float attackRange = 1f;
        [Min(0f)] public float magic = 1f;
        [Min(0f)] public float skillCooldownReduction = 1f;

        [Header("Combat Multipliers")]
        [Min(0f)] public float basicAttackDamage = 1f;
        [Min(0.01f)] public float basicAttackCooldown = 1f;
        [Min(0f)] public float basicAttackRange = 1f;
        [Min(0f)] public float skillDamage = 1f;
        [Min(0.01f)] public float skillCooldown = 1f;

        public float GetBaseStatMultiplier(CharacterStatType statType)
        {
            return statType switch
            {
                CharacterStatType.MaxHealth => Mathf.Max(0.01f, maxHealth),
                CharacterStatType.MoveSpeed => Mathf.Max(0.01f, moveSpeed),
                CharacterStatType.AttackPower => Mathf.Max(0.01f, attackPower),
                CharacterStatType.AttackSpeed => Mathf.Max(0.01f, attackSpeed),
                CharacterStatType.AttackRange => Mathf.Max(0.01f, attackRange),
                CharacterStatType.Magic => Mathf.Max(0f, magic),
                CharacterStatType.SkillCooldownReduction => Mathf.Max(0f, skillCooldownReduction),
                _ => 1f
            };
        }
    }

    [Serializable]
    public sealed class EnemyDifficultyBalance
    {
        [Min(0.01f)] public float maxHealth = 1f;
        [Min(0.01f)] public float moveSpeed = 1f;
        [Min(0f)] public float outgoingDamage = 1f;
        [Min(0.01f)] public float attackCooldown = 1f;
        [Min(0f)] public float experienceReward = 1f;
    }

    [Serializable]
    public sealed class ItemDifficultyBalance
    {
        [Tooltip("0이면 PlayerItemManager에 설정된 슬롯 수를 사용합니다.")]
        [Min(0)] public int maxSlotsOverride;
        [Min(0f)] public float worldSpawnCount = 1f;
    }

    [Serializable]
    public sealed class WorldDifficultyBalance
    {
        [Min(0f)] public float enemySpawnerDensity = 1f;
        [Min(0f)] public float sceneObjectDensity = 1f;
        [Min(0f)] public float enemyMaxAlive = 1f;
        [Min(0.01f)] public float enemyRespawnCooldown = 1f;
    }

    [Serializable]
    public sealed class DifficultyBiomeOverride
    {
        public BiomeType biome = BiomeType.None;
        public BiomeConfig config;
    }

    [Serializable]
    public sealed class DifficultyBiomeWorldOverride
    {
        public BiomeType biome = BiomeType.None;
        public WorldDifficultyBalance world = new WorldDifficultyBalance();
    }
}
