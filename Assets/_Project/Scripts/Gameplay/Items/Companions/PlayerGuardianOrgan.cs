using System.Collections.Generic;
using UnityEngine;
using static Necrocis.ItemCompanionResources;

namespace Necrocis
{
    public sealed class PlayerGuardianOrgan : MonoBehaviour
    {
        private IItemCompanionOwner owner;
        private SpriteRenderer spriteRenderer;
        private float angle;
        private float nextBlockTime;
        private Vector3 cameraRight = Vector3.right;
        private Vector3 cameraUp = Vector3.up;

        public void Initialize(IItemCompanionOwner owner)
        {
            this.owner = owner;
            angle = 0f;
            nextBlockTime = 0f;

            if (spriteRenderer == null)
            {
                spriteRenderer = ItemEffectVisuals.GetOrAddSpriteRenderer(gameObject);
            }

            Sprite guardianSprite = TextureSpriteCache.LoadResourceSprite(GuardianOrganEffectPath);
            spriteRenderer.sprite = guardianSprite != null ? guardianSprite : TextureSpriteCache.GetCircleSprite();
            spriteRenderer.color = guardianSprite != null ? Color.white : new Color(0.68f, 0.82f, 1f, 0.95f);
            spriteRenderer.sortingOrder = 5350;
            spriteRenderer.enabled = true;
            transform.localScale = Vector3.one * TextureSpriteCache.GetUniformScaleForWorldSize(spriteRenderer.sprite, 0.82f);
            CacheCameraBasis();
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

            angle += Time.deltaTime * 260f;
            float radians = angle * Mathf.Deg2Rad;
            Vector3 center = owner.GetPlayerVisualCenter();
            Vector3 offset = (cameraRight * Mathf.Cos(radians) + cameraUp * Mathf.Sin(radians)) * Mathf.Max(0.2f, owner.GuardianOrganOrbitRadius);
            transform.position = center + offset;
            spriteRenderer.color = Time.time >= nextBlockTime
                ? Color.white
                : new Color(0.45f, 0.55f, 0.7f, 0.5f);
            if (Time.time < nextBlockTime)
            {
                return;
            }

            TryBlockProjectile();
        }

        private void TryBlockProjectile()
        {
            IReadOnlyList<EnemyProjectile> projectiles = EnemyProjectile.ActiveEnemyProjectiles;
            float blockRadius = Mathf.Max(0.1f, owner.GuardianOrganBlockRadius);
            float blockRadiusSqr = blockRadius * blockRadius;

            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                EnemyProjectile projectile = projectiles[i];
                if (projectile == null || !projectile.IsLaunched)
                {
                    continue;
                }

                Vector3 toProjectile = projectile.transform.position - transform.position;
                toProjectile.y = 0f;
                if (toProjectile.sqrMagnitude > blockRadiusSqr)
                {
                    continue;
                }

                projectile.Deflect();
                nextBlockTime = Time.time + Mathf.Max(0.1f, owner.GuardianOrganCooldown);
                SpawnBlockVisual();
                return;
            }
        }

        private void SpawnBlockVisual()
        {
            ItemEffectVisuals.SpawnPooledCircleVisual(
                GuardianBlockVisualPoolName,
                "GuardianBlockVisual",
                transform.position,
                1.2f,
                new Color(0.68f, 0.82f, 1f, 0.35f),
                5360,
                0.16f);
        }

        private void CacheCameraBasis()
        {
            Camera activeCamera = DontStarveCamera.GetActiveCamera();
            if (activeCamera != null)
            {
                cameraRight = activeCamera.transform.right;
                cameraUp = activeCamera.transform.up;
                transform.rotation = activeCamera.transform.rotation;
            }
            else
            {
                cameraRight = Vector3.right;
                cameraUp = Vector3.up;
                transform.rotation = Quaternion.identity;
            }
        }

        private void OnDisable()
        {
            owner = null;
        }
    }
}
