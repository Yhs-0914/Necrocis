using UnityEngine;
using System.Collections.Generic;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome/Enemy Spawn Config", fileName = "EnemySpawnConfig")]
    public class EnemySpawnConfig : ScriptableObject
    {
        [Header("Elite Spawn")]
        [Min(1)] public int normalKillsPerElite = 10;

        [Header("Enemy Spawn Rules")]
        public List<EnemySpawnRuleConfig> enemySpawnRules = new List<EnemySpawnRuleConfig>();

        public int NormalKillsPerElite => Mathf.Max(1, normalKillsPerElite);

        public IReadOnlyList<EnemySpawnRuleConfig> GetEnemySpawnRules()
        {
            return enemySpawnRules != null
                ? enemySpawnRules
                : System.Array.Empty<EnemySpawnRuleConfig>();
        }
    }
}
