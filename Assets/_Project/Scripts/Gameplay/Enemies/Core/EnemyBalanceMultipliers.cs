using System;
using UnityEngine;

namespace Necrocis
{
    [Serializable]
    public class EnemyBalanceMultipliers
    {
        [Min(0f)] public float health = 1f;
        [Min(0f)] public float attack = 1f;
        [Tooltip("Ordinary spawner wave capacity; elite kill triggers and boss encounters are unchanged.")]
        [Min(0f)] public float spawnCount = 1f;

        public EnemySpawnRuleConfig CreateRuntimeRule(EnemySpawnRuleConfig source)
        {
            if (source == null) return null;

            EnemySpawnRuleConfig result = source.CreateRuntimeCopy();
            result.maxHealth *= Mathf.Max(0f, health);
            result.attackDamage *= Mathf.Max(0f, attack);
            result.maxAlive = Mathf.Max(0, Mathf.RoundToInt(source.maxAlive * Mathf.Max(0f, spawnCount)));
            // Additional base stats can override the primary fields in CharacterStats.
            if (result.additionalBaseStats != null)
            {
                for (int i = 0; i < result.additionalBaseStats.Count; i++)
                {
                    CharacterStatValue value = result.additionalBaseStats[i];
                    if (value.statType == CharacterStatType.MaxHealth) value.value *= Mathf.Max(0f, health);
                    if (value.statType == CharacterStatType.AttackPower) value.value *= Mathf.Max(0f, attack);
                    result.additionalBaseStats[i] = value;
                }
            }

            return result;
        }
    }

    [Serializable]
    public class DifficultyBalanceSettings
    {
        public EnemyBalanceMultipliers normalEnemies = new EnemyBalanceMultipliers();
        public EnemyBalanceMultipliers eliteEnemies = new EnemyBalanceMultipliers();
    }
}
