using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public class Projectile : MonoBehaviour
    {
        public enum SpawnKind
        {
            Normal = 0,
            SplitChild = 1
        }

        private const int HitBufferSize = 8;
        private const int ExplosionBufferSize = 24;

        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField] private float hitCheckRadius = 0.35f;
        [SerializeField] private float hitCheckHeightOffset = 0.75f;
        [SerializeField] private float hitCheckVerticalHalfHeight = 2.5f;

        private Vector3 moveDirection;
        private float flightHeight;
        private float damage;
        private float deactivateTime;
        private bool hasImpacted;
        private float traveledDistance;
        private bool returning;
        private int maxHitCount = 1;
        private int currentHitCount;
        private int remainingBounces;
        private bool splitTriggered;
        private SpawnKind spawnKind = SpawnKind.Normal;
        private PlayerItemCombatEffects itemEffects;
        private Transform ownerTransform;
        private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
        private readonly Collider[] explosionBuffer = new Collider[ExplosionBufferSize];
        private readonly HashSet<int> hitEnemyIds = new HashSet<int>();

        private Vector3 defaultLocalScale = Vector3.one;
        private bool defaultScaleCached;

        public void Launch(Vector3 direction, float damage)
        {
            Launch(direction, damage, targetMask, lifeTime * Mathf.Max(0.1f, speed), null, SpawnKind.Normal);
        }

        public void Launch(Vector3 direction, float damage, LayerMask mask)
        {
            Launch(direction, damage, mask, lifeTime * Mathf.Max(0.1f, speed), null, SpawnKind.Normal);
        }

        public void Launch(Vector3 direction, float damage, LayerMask mask, float range)
        {
            Launch(direction, damage, mask, range, null, SpawnKind.Normal);
        }

        public void Launch(Vector3 direction, float damage, LayerMask mask, float range, PlayerItemCombatEffects effects, SpawnKind kind = SpawnKind.Normal)
        {
            direction.y = 0f;
            moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            this.damage = damage;
            targetMask = mask;
            itemEffects = effects;
            spawnKind = kind;
            flightHeight = transform.position.y;
            ownerTransform = PlayerController.Instance != null ? PlayerController.Instance.transform : null;

            traveledDistance = 0f;
            currentHitCount = 0;
            hitEnemyIds.Clear();
            hasImpacted = false;
            returning = false;
            splitTriggered = false;

            maxHitCount = 1;
            if (spawnKind == SpawnKind.Normal && itemEffects != null)
            {
                maxHitCount = Mathf.Max(1, itemEffects.GetPiercingHitCount());
                remainingBounces = itemEffects.GetReflectionBounceCount();
            }
            else
            {
                remainingBounces = 0;
            }

            CacheDefaultScale();
            float scaleMultiplier = 1f;
            if (spawnKind == SpawnKind.Normal && itemEffects != null)
            {
                scaleMultiplier = itemEffects.GetScaleMultiplier();
            }
            transform.localScale = defaultLocalScale * scaleMultiplier;

            float effectiveRange = Mathf.Max(0.05f, range);
            if (spawnKind == SpawnKind.Normal && itemEffects != null)
            {
                effectiveRange *= itemEffects.GetRangeMultiplier();
            }

            deactivateTime = Time.time + effectiveRange / Mathf.Max(0.01f, speed);
        }

        private void OnEnable()
        {
            CacheDefaultScale();
            if (deactivateTime <= Time.time)
            {
                deactivateTime = Time.time + lifeTime;
            }
            hasImpacted = false;
        }

        private void Update()
        {
            if (spawnKind == SpawnKind.Normal && itemEffects != null)
            {
                if (itemEffects.HasHomingCell)
                {
                    ApplyHoming(Time.deltaTime);
                }

                if (itemEffects.HasRefluxOrgan)
                {
                    UpdateBoomerangState();
                }
            }

            Vector3 step = moveDirection * speed * Time.deltaTime;
            if (TryReflectFromBiome(ref step))
            {
                // reflected
            }

            Vector3 nextPosition = transform.position + step;
            nextPosition.y = flightHeight;
            transform.position = nextPosition;
            traveledDistance += step.magnitude;

            if (spawnKind == SpawnKind.Normal && itemEffects != null && itemEffects.HasPulseBullet)
            {
                ApplyPulseScale();
            }

            if (returning && ownerTransform != null)
            {
                Vector3 toOwner = ownerTransform.position - transform.position;
                toOwner.y = 0f;
                if (toOwner.sqrMagnitude <= 0.49f)
                {
                    gameObject.SetActive(false);
                    return;
                }
            }

            TryDetectHitByOverlap();

            if (Time.time >= deactivateTime)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleHit(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
            {
                return;
            }

            HandleHit(collision.collider);
        }

        private void HandleHit(Collider other)
        {
            if (other == null || hasImpacted)
            {
                return;
            }

            if ((targetMask.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }

            EnemyController enemy = other.GetComponent<EnemyController>()
                ?? other.GetComponentInParent<EnemyController>();

            if (enemy == null || enemy.IsDead)
            {
                return;
            }

            int enemyId = enemy.GetInstanceID();
            if (!hitEnemyIds.Add(enemyId))
            {
                return;
            }

            currentHitCount++;
            enemy.TakeDamage(damage);

            if (itemEffects != null)
            {
                itemEffects.ApplyCommonOnHitEffects(enemy, damage, transform.position);

                if (spawnKind == SpawnKind.Normal && itemEffects.HasSplitTissue && !splitTriggered)
                {
                    splitTriggered = true;
                    itemEffects.SpawnSplitProjectiles(transform.position, moveDirection, damage, targetMask, GetRemainingRange());
                }

                if (spawnKind == SpawnKind.Normal && itemEffects.HasExplosiveBloodCell)
                {
                    ApplyExplosionDamage(enemy);
                }
            }

            if (currentHitCount >= maxHitCount)
            {
                hasImpacted = true;
                gameObject.SetActive(false);
            }
        }

        private void TryDetectHitByOverlap()
        {
            if (hasImpacted)
            {
                return;
            }

            float radius = Mathf.Max(0.05f, hitCheckRadius);
            Vector3 hitCenter = transform.position;
            hitCenter.y += hitCheckHeightOffset;
            float halfHeight = Mathf.Max(0.05f, hitCheckVerticalHalfHeight);
            Vector3 capsuleTop = hitCenter + Vector3.up * halfHeight;
            Vector3 capsuleBottom = hitCenter - Vector3.up * halfHeight;

            int count = Physics.OverlapCapsuleNonAlloc(
                capsuleTop,
                capsuleBottom,
                radius,
                hitBuffer,
                targetMask,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider collider = hitBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                HandleHit(collider);
                if (hasImpacted)
                {
                    break;
                }
            }
        }

        private void ApplyHoming(float deltaTime)
        {
            EnemyController nearestEnemy = FindNearestEnemy(itemEffects.GetHomingSearchRadius());
            if (nearestEnemy == null)
            {
                return;
            }

            Vector3 toTarget = nearestEnemy.transform.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector3 desiredDirection = toTarget.normalized;
            float turnRate = itemEffects.GetHomingTurnRate();
            moveDirection = Vector3.Slerp(moveDirection, desiredDirection, turnRate * deltaTime).normalized;
        }

        private EnemyController FindNearestEnemy(float radius)
        {
            var enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return null;
            }

            float radiusSqr = radius * radius;
            float nearestDistSqr = float.MaxValue;
            EnemyController nearest = null;

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - transform.position;
                toEnemy.y = 0f;
                float distSqr = toEnemy.sqrMagnitude;
                if (distSqr > radiusSqr || distSqr >= nearestDistSqr)
                {
                    continue;
                }

                nearestDistSqr = distSqr;
                nearest = enemy;
            }

            return nearest;
        }

        private void UpdateBoomerangState()
        {
            if (itemEffects == null || ownerTransform == null)
            {
                return;
            }

            if (!returning && traveledDistance >= itemEffects.GetBoomerangReturnDistance())
            {
                returning = true;
            }

            if (!returning)
            {
                return;
            }

            Vector3 toOwner = ownerTransform.position - transform.position;
            toOwner.y = 0f;
            if (toOwner.sqrMagnitude > 0.0001f)
            {
                moveDirection = toOwner.normalized;
            }
        }

        private bool TryReflectFromBiome(ref Vector3 step)
        {
            if (remainingBounces <= 0)
            {
                return false;
            }

            BiomeManager biome = BiomeManager.Active;
            if (biome == null)
            {
                return false;
            }

            Vector3 current = transform.position;
            Vector3 next = current + step;
            if (IsWalkablePosition(biome, next))
            {
                return false;
            }

            bool xBlocked = !IsWalkablePosition(biome, current + new Vector3(step.x, 0f, 0f));
            bool zBlocked = !IsWalkablePosition(biome, current + new Vector3(0f, 0f, step.z));

            Vector3 reflectedDirection = moveDirection;
            if (xBlocked)
            {
                reflectedDirection.x = -reflectedDirection.x;
            }

            if (zBlocked)
            {
                reflectedDirection.z = -reflectedDirection.z;
            }

            if (!xBlocked && !zBlocked)
            {
                reflectedDirection = -reflectedDirection;
            }

            moveDirection = reflectedDirection.sqrMagnitude > 0.0001f ? reflectedDirection.normalized : -moveDirection;
            remainingBounces--;
            step = moveDirection * speed * Time.deltaTime;
            return true;
        }

        private static bool IsWalkablePosition(BiomeManager biome, Vector3 worldPosition)
        {
            Vector2Int grid = biome.WorldToGrid(worldPosition);
            if (!biome.IsValidPosition(grid.x, grid.y))
            {
                return false;
            }

            return biome.IsWalkable(grid.x, grid.y);
        }

        private void ApplyPulseScale()
        {
            float amplitude = itemEffects.GetPulseAmplitude();
            float frequency = itemEffects.GetPulseFrequency();
            float pulse = 1f + Mathf.Sin((traveledDistance / frequency) * Mathf.PI * 2f) * amplitude;
            transform.localScale = defaultLocalScale * pulse;
        }

        private void ApplyExplosionDamage(EnemyController primaryEnemy)
        {
            float radius = itemEffects.GetExplosionRadius();
            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                radius,
                explosionBuffer,
                targetMask,
                QueryTriggerInteraction.Collide);

            float explosionDamage = Mathf.Max(0f, damage * itemEffects.GetExplosionDamageMultiplier());
            for (int i = 0; i < hitCount; i++)
            {
                Collider collider = explosionBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                EnemyController enemy = collider.GetComponent<EnemyController>()
                    ?? collider.GetComponentInParent<EnemyController>();

                if (enemy == null || enemy.IsDead || enemy == primaryEnemy)
                {
                    continue;
                }

                enemy.TakeDamage(explosionDamage);
                itemEffects.ApplyCommonOnHitEffects(enemy, explosionDamage, transform.position);
            }
        }

        private float GetRemainingRange()
        {
            float remainingTime = Mathf.Max(0f, deactivateTime - Time.time);
            return remainingTime * Mathf.Max(0.01f, speed);
        }

        private void CacheDefaultScale()
        {
            if (defaultScaleCached)
            {
                return;
            }

            defaultLocalScale = transform.localScale;
            defaultScaleCached = true;
        }

        private void OnDisable()
        {
            if (defaultScaleCached)
            {
                transform.localScale = defaultLocalScale;
            }

            hitEnemyIds.Clear();
            hasImpacted = false;
            itemEffects = null;
            returning = false;
            splitTriggered = false;
            currentHitCount = 0;
            remainingBounces = 0;
        }
    }
}
