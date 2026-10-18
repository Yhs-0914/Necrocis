using System;
using System.Collections.Generic;
using UnityEngine;
using static Necrocis.PlayerItemIds;
namespace Necrocis
{
    internal sealed class ItemCombatQueries
    {
        private readonly IItemCombatHost host;
        internal ItemCombatQueries(IItemCombatHost host){this.host=host;}

        public bool CanInfectEnemy(EnemyController enemy)
        {
            if (enemy == null || enemy.IsElite)
            {
                return false;
            }

            return !IsBossLikeEnemy(enemy);
        }

        public float GetCompanionBaseDamage()
        {
            return host.HasStatsOwner ? Mathf.Max(0.1f, host.AttackPower) : 1f;
        }

        public EnemyController FindClosestEnemy(Vector3 center, float radius, List<EnemyController> excluded = null)
        {
            var enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return null;
            }

            float radiusSqr = Mathf.Max(0.1f, radius);
            radiusSqr *= radiusSqr;
            float bestDistanceSqr = float.PositiveInfinity;
            EnemyController bestEnemy = null;

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (!IsEnemyTargetable(enemy) || (excluded != null && excluded.Contains(enemy)))
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - center;
                toEnemy.y = 0f;
                float distanceSqr = toEnemy.sqrMagnitude;
                if (distanceSqr > radiusSqr || distanceSqr >= bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                bestEnemy = enemy;
            }

            return bestEnemy;
        }

        public int CountEnemiesWithinRadius(Vector3 center, float radius)
        {
            var enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return 0;
            }

            float radiusSqr = Mathf.Max(0.1f, radius);
            radiusSqr *= radiusSqr;
            int count = 0;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (!IsEnemyTargetable(enemy))
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - center;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude <= radiusSqr)
                {
                    count++;
                }
            }

            return count;
        }

        public bool IsBossOrEliteWithinRadius(float radius)
        {
            var enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return false;
            }

            float radiusSqr = Mathf.Max(0.1f, radius);
            radiusSqr *= radiusSqr;
            Vector3 center = host.Transform.position;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (!IsEnemyTargetable(enemy) || (!enemy.IsElite && !IsBossLikeEnemy(enemy)))
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - center;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude <= radiusSqr)
                {
                    return true;
                }
            }

            return false;
        }

        public EnemyController FindNearestBossLikeEnemy(float radius)
        {
            var enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return null;
            }

            Vector3 center = host.GlobalPlayerPosition;
            float radiusSqr = Mathf.Max(0.1f, radius);
            radiusSqr *= radiusSqr;
            float bestDistanceSqr = float.PositiveInfinity;
            EnemyController bestEnemy = null;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (!IsEnemyTargetable(enemy) || !IsBossLikeEnemy(enemy))
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - center;
                toEnemy.y = 0f;
                float distanceSqr = toEnemy.sqrMagnitude;
                if (distanceSqr > radiusSqr || distanceSqr >= bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                bestEnemy = enemy;
            }

            return bestEnemy;
        }

        public bool IsEnemyAboveHealthRatio(EnemyController enemy, float ratio)
        {
            CharacterStats stats = enemy != null ? enemy.Stats : null;
            if (stats == null || stats.MaxHealth <= 0f)
            {
                return false;
            }

            return stats.CurrentHealth / stats.MaxHealth >= ratio;
        }

        public bool IsEnemyBelowHealthRatio(EnemyController enemy, float ratio)
        {
            CharacterStats stats = enemy != null ? enemy.Stats : null;
            if (stats == null || stats.MaxHealth <= 0f)
            {
                return false;
            }

            return stats.CurrentHealth / stats.MaxHealth <= ratio;
        }

        public float GetEnemyCurrentHealth(EnemyController enemy)
        {
            CharacterStats stats = enemy != null ? enemy.Stats : null;
            return stats != null ? Mathf.Max(0f, stats.CurrentHealth) : 0f;
        }

        public bool IsEnemyTargetable(EnemyController enemy)
        {
            return enemy != null && enemy.gameObject.activeInHierarchy && !enemy.IsDead;
        }

        public bool IsBossLikeEnemy(EnemyController enemy)
        {
            if (enemy == null)
            {
                return false;
            }

            if (enemy.GetComponent<IntestineBossPattern>() != null)
            {
                return true;
            }

            return enemy.gameObject.name.EndsWith("_MidBoss", StringComparison.OrdinalIgnoreCase);
        }

        public Sprite FindEnemySprite(EnemyController enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            SpriteRenderer[] renderers = enemy.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null || renderer.sprite == null || renderer.gameObject.name.Contains("Overlay"))
                {
                    continue;
                }

                return renderer.sprite;
            }

            return TextureSpriteCache.GetCircleSprite();
        }

        public Sprite[] FindEnemyAttackSprites(EnemyController enemy)
        {
            EnemySpawnRuleConfig config = enemy != null ? enemy.Config : null;
            if (config == null)
            {
                return null;
            }

            if (config.attackSprites != null && config.attackSprites.Length > 0)
            {
                return config.attackSprites;
            }

            if (config.attackSpritesDown != null && config.attackSpritesDown.Length > 0)
            {
                return config.attackSpritesDown;
            }

            if (config.attackSpritesUp != null && config.attackSpritesUp.Length > 0)
            {
                return config.attackSpritesUp;
            }

            return null;
        }

        public Vector3 FindEnemyVisualScale(EnemyController enemy, Vector3 fallback)
        {
            if (enemy == null)
            {
                return fallback;
            }

            SpriteRenderer[] renderers = enemy.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null || renderer.gameObject.name.Contains("Overlay"))
                {
                    continue;
                }

                return renderer.transform.lossyScale;
            }

            return fallback;
        }

        public Vector3 ResolveGroundSpawnPosition(Vector3 desiredPosition, float heightOffset)
            => ItemGroundPlacementBindings.Resolve(desiredPosition,host.Transform.position,heightOffset);

        public PlayerProjectilePool ResolvePool()
        {
            PlayerProjectilePool pool = PlayerProjectilePool.Instance;
            if (pool != null)
            {
                return pool;
            }

            pool = UnityEngine.Object.FindFirstObjectByType<PlayerProjectilePool>();
            if (pool != null)
            {
                PlayerProjectilePool.Instance = pool;
            }

            return pool;
        }

        public EnemyStatusEffectController EnsureStatusController(EnemyController enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            EnemyStatusEffectController status = enemy.StatusEffects;
            if (status == null)
            {
                status = enemy.GetComponent<EnemyStatusEffectController>();
            }

            if (status == null)
            {
                status = enemy.gameObject.AddComponent<EnemyStatusEffectController>();
            }

            status.Initialize(enemy);
            return status;
        }
    }
}
