using System.Collections.Generic;
using UnityEngine;
using static Necrocis.ItemCompanionResources;

namespace Necrocis
{
    public sealed class PlayerBioProjectile : MonoBehaviour
    {
        private EnemyController target;
        private uint targetGeneration;
        private SpriteRenderer spriteRenderer;
        private float damage;
        private float expireTime;
        private float speed;
        private HealthEffectSource damageSource;
        private uint launchGeneration;

        public static void Spawn(Vector3 position, EnemyController target, float damage, Color color, HealthEffectSource source)
        {
            if (!ItemEnemyQueries.IsTargetable(target))
            {
                return;
            }

            GameObject projectileObject = RuntimePool.Acquire(BloodDroneProjectilePoolName, ItemCompanionRuntime.CreateBloodDroneProjectileFunc);
            if (projectileObject == null)
            {
                return;
            }

            projectileObject.name = "BloodDroneProjectile";
            projectileObject.transform.position = position;
            PlayerBioProjectile projectile = projectileObject.GetComponent<PlayerBioProjectile>();
            projectile.Initialize(target, damage, color, source);
        }

        private void Initialize(EnemyController target, float damage, Color color, HealthEffectSource source)
        {
            unchecked { launchGeneration++; }
            damageSource = source;
            this.target = target;
            targetGeneration=target.SpawnGeneration;
            this.damage = Mathf.Max(0f, damage);
            speed = 11f;
            expireTime = Time.time + 1.6f;
            if (spriteRenderer == null)
            {
                spriteRenderer = ItemEffectVisuals.GetOrAddSpriteRenderer(gameObject);
            }

            spriteRenderer.sprite = TextureSpriteCache.GetCircleSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = 5320;
            spriteRenderer.enabled = true;
            transform.localScale = Vector3.one * 0.22f;
            Camera activeCamera = DontStarveCamera.GetActiveCamera();
            if (activeCamera != null)
            {
                transform.rotation = activeCamera.transform.rotation;
            }
        }

        private void Update()
        {
            if (Time.time >= expireTime || !ItemEnemyQueries.IsTargetable(target) || target.SpawnGeneration!=targetGeneration)
            {
                ReleaseSelf();
                return;
            }

            Vector3 toTarget = target.transform.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= 0.45f * 0.45f)
            {
                uint generation = launchGeneration;
                target.TakeDamage(damage, damageSource);
                if (generation == launchGeneration && isActiveAndEnabled) ReleaseSelf();
                return;
            }

            transform.position += toTarget.normalized * speed * Time.deltaTime;
        }

        private void ReleaseSelf()
        {
            target = null;
            RuntimePool.Release(gameObject);
        }

        private void OnDisable()
        {
            unchecked { launchGeneration++; }
            target = null;
        }
    }
}
