using System.Collections.Generic;
using UnityEngine;
using static Necrocis.ItemCompanionResources;

namespace Necrocis
{
    public sealed class PlayerBloodDrone : MonoBehaviour
    {
        private IItemCompanionOwner owner;
        private SpriteRenderer spriteRenderer;
        private float angle;
        private float nextFireTime;
        private uint healthGeneration;
        private HealthEffectSource damageSource;

        public void Initialize(IItemCompanionOwner owner)
        {
            this.owner = owner;
            unchecked { healthGeneration++; }
            damageSource = HealthEffectSource.ForOwner(this, "blood-drone/projectile", healthGeneration);
            angle = 0f;
            nextFireTime = 0f;

            if (spriteRenderer == null)
            {
                spriteRenderer = ItemEffectVisuals.GetOrAddSpriteRenderer(gameObject);
            }

            Sprite droneSprite = TextureSpriteCache.LoadResourceSprite(BloodDroneEffectPath);
            spriteRenderer.sprite = droneSprite != null ? droneSprite : TextureSpriteCache.GetCircleSprite();
            spriteRenderer.color = droneSprite != null ? Color.white : new Color(0.95f, 0.08f, 0.18f, 0.92f);
            spriteRenderer.sortingOrder = 5300;
            spriteRenderer.enabled = true;
            transform.localScale = Vector3.one * TextureSpriteCache.GetUniformScaleForWorldSize(spriteRenderer.sprite, 0.68f);
            SyncBillboard();
        }

        public void ClearOwner()
        {
            owner = null;
        }

        private void Update()
        {
            if ((owner == null || !owner.IsAvailable))
            {
                RuntimePool.Release(gameObject);
                return;
            }

            angle += Time.deltaTime * 170f;
            float radians = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)) * Mathf.Max(0.2f, owner.BloodDroneOrbitRadius);
            transform.position = owner.OwnerTransform.position + offset + Vector3.up * 0.45f;

            if (Time.time < nextFireTime)
            {
                return;
            }

            EnemyController target = owner.FindClosestEnemy(owner.OwnerTransform.position, Mathf.Max(0.1f, owner.BloodDroneTargetRadius));
            if (target == null)
            {
                return;
            }

            nextFireTime = Time.time + Mathf.Max(0.1f, owner.BloodDroneFireInterval);
            float damage = owner.GetCompanionBaseDamage() * Mathf.Max(0.05f, owner.BloodDroneDamageMultiplier);
            PlayerBioProjectile.Spawn(transform.position, target, damage, new Color(1f, 0.05f, 0.16f, 0.95f), damageSource);
        }

        private void SyncBillboard()
        {
            Camera activeCamera = DontStarveCamera.GetActiveCamera();
            if (activeCamera != null)
            {
                transform.rotation = activeCamera.transform.rotation;
            }
        }

        private void OnDisable()
        {
            owner = null;
        }
    }
}
