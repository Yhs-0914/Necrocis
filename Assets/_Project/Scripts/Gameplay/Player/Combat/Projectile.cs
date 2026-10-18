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

        private enum HitResult
        {
            Ignored,
            Continued,
            Terminated
        }

        private const int HitBufferSize = 8;
        private const int ExplosionBufferSize = 24;
        private const int ObstacleHitBufferSize = 12;
        private const string ExplosionVisualPoolName = "Projectile.ExplosiveBloodCellVisual";

        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField] private float hitCheckRadius = 0.35f;
        [SerializeField] private float hitCheckHeightOffset = 0.75f;
        [SerializeField] private float hitCheckVerticalHalfHeight = 2.5f;

        private Vector3 moveDirection;
        private float flightHeight;
        private float damage;
        private HealthEffectSource damageSource;
        private float currentSpeed;
        private float deactivateTime;
        private bool hasImpacted = true;
        private uint launchGeneration;
        private Collider hitCollider;
        private float traveledDistance;
        private bool returning;
        private int maxHitCount = 1;
        private int currentHitCount;
        private int remainingBounces;
        private bool splitTriggered;
        private bool pulseScaleGrowMode;
        private bool pulseScaleModeInitialized;
        private float pulseRangeBudget;
        private SpawnKind spawnKind = SpawnKind.Normal;
        private IProjectileItemEffects itemEffects;
        private bool HasItemEffects => itemEffects != null && itemEffects.IsAvailable;
        private Transform ownerTransform;
        private float launchRange;
        private Vector3 activeBaseScale = Vector3.one;
        private bool boomerangRehitResetDone;
        private int boomerangPassHitCount;
        private Collider[] hitBuffer = new Collider[HitBufferSize];
        private Collider[] explosionBuffer = new Collider[ExplosionBufferSize];
        private RaycastHit[] obstacleHitBuffer = new RaycastHit[ObstacleHitBufferSize];
        private readonly HashSet<long> hitEnemyIds = new HashSet<long>();
        private int propAttackToken;

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

        public void Launch(Vector3 direction, float damage, LayerMask mask, float range, PlayerItemCombatEffects effects, SpawnKind kind = SpawnKind.Normal, HealthEffectSource? source = null)
            => LaunchWithItemContext(direction, damage, mask, range, effects, kind, source);

        public void LaunchWithItemContext(Vector3 direction, float damage, LayerMask mask, float range, IProjectileItemEffects effects, SpawnKind kind = SpawnKind.Normal, HealthEffectSource? source = null)
        {
            unchecked { launchGeneration++; }
            propAttackToken = ResidueRubble.NextAttackToken();
            direction.y = 0f;
            moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            this.damage = damage;
            damageSource = source ?? HealthEffectSource.Legacy;
            currentSpeed = Mathf.Max(0.01f, speed);
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
            boomerangRehitResetDone = false;
            boomerangPassHitCount = 0;
            launchRange = Mathf.Max(0.05f, range);
            pulseScaleModeInitialized = false;
            pulseRangeBudget = launchRange;

            maxHitCount = 1;
            if (spawnKind == SpawnKind.Normal && HasItemEffects)
            {
                float damageMultiplier;
                currentSpeed = speed * itemEffects.RollUnstableCellProjectileSpeedMultiplier(out damageMultiplier);
                this.damage *= damageMultiplier;
                maxHitCount = Mathf.Max(1, itemEffects.GetPiercingHitCount());
                remainingBounces = itemEffects.GetReflectionBounceCount();
                if (itemEffects.HasRefluxOrgan)
                {
                    // Boomerang projectiles should not disappear on enemy hit count.
                    maxHitCount = int.MaxValue;
                }
            }
            else
            {
                remainingBounces = 0;
            }

            CacheDefaultScale();
            float scaleMultiplier = 1f;
            if (spawnKind == SpawnKind.Normal && HasItemEffects)
            {
                scaleMultiplier = itemEffects.GetScaleMultiplier();
            }
            transform.localScale = defaultLocalScale * scaleMultiplier;
            activeBaseScale = transform.localScale;

            float effectiveRange = launchRange;
            if (spawnKind == SpawnKind.Normal && HasItemEffects)
            {
                effectiveRange *= itemEffects.GetRangeMultiplier();
                if (itemEffects.HasRefluxOrgan)
                {
                    // Outbound + return travel budget.
                    effectiveRange *= 2f;
                }
            }
            pulseRangeBudget = effectiveRange;
            if (spawnKind == SpawnKind.Normal && HasItemEffects && itemEffects.HasPulseBullet)
            {
                pulseScaleGrowMode = Random.value < 0.5f;
                pulseScaleModeInitialized = true;
            }

            deactivateTime = Time.time + effectiveRange / Mathf.Max(0.01f, currentSpeed);
        }

        private void OnEnable()
        {
            CacheDefaultScale();
            if (currentSpeed <= 0f)
            {
                currentSpeed = Mathf.Max(0.01f, speed);
            }

            if (deactivateTime <= Time.time)
            {
                deactivateTime = Time.time + lifeTime;
            }
        }

        private void Update()
        {
            uint generation = launchGeneration;
            if (hasImpacted || !IsCurrentFlight(generation)) return;

            float deltaTime = Time.deltaTime;
            if (spawnKind == SpawnKind.Normal && HasItemEffects)
            {
                if (itemEffects.HasHomingCell)
                {
                    ApplyHoming(deltaTime);
                }

                if (itemEffects.HasRefluxOrgan)
                {
                    UpdateBoomerangState();
                }
            }

            float stepDistance = currentSpeed * deltaTime;
            Vector3 step = moveDirection * stepDistance;
            bool reflected = TryReflectFromObstacleCollider(ref step);
            if (!reflected)
            {
                reflected = TryReflectFromBiome(ref step);
            }

            Vector3 nextPosition = transform.position + step;
            nextPosition.y = flightHeight;
            ResidueRubble.HitAlongSegment(transform.position, nextPosition, GetScaledHitCheckRadius(), damage, targetMask, propAttackToken);
            if (!IsCurrentFlight(generation)) return;
            transform.position = nextPosition;
            traveledDistance += reflected ? step.magnitude : stepDistance;

            if (spawnKind == SpawnKind.Normal && HasItemEffects && itemEffects.HasPulseBullet)
            {
                ApplyPulseScale();
            }

            if (returning && ownerTransform != null)
            {
                Vector3 toOwner = ownerTransform.position - transform.position;
                toOwner.y = 0f;
                if (toOwner.sqrMagnitude <= 0.49f)
                {
                    EndFlight(generation);
                    return;
                }
            }

            TryDetectHitByOverlap();
            if (hasImpacted || !IsCurrentFlight(generation)) return;

            if (Time.time >= deactivateTime)
            {
                EndFlight(generation);
            }
        }

        private bool IsCurrentFlight(uint generation) =>
            this != null && launchGeneration == generation && gameObject.activeInHierarchy;

        private void EndFlight(uint generation)
        {
            if (!IsCurrentFlight(generation)) return;
            hasImpacted = true;
            gameObject.SetActive(false);
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

        private HitResult HandleHit(Collider other)
        {
            uint generation = launchGeneration;
            if (other == null || hasImpacted || !IsCurrentFlight(generation))
            {
                return HitResult.Ignored;
            }

            if ((targetMask.value & (1 << other.gameObject.layer)) == 0)
            {
                return HitResult.Ignored;
            }

            if (other.TryGetComponent(out ResidueRubble rubble))
            {
                bool applied = rubble.ApplyPlayerHit(damage, propAttackToken);
                if (!IsCurrentFlight(generation)) return HitResult.Terminated;
                return applied ? HitResult.Continued : HitResult.Ignored;
            } // Prop hits do not consume or redirect the projectile.
            if (!TryGetEnemyController(other, out EnemyController enemy) || enemy.IsDead)
            {
                return HitResult.Ignored;
            }

            Vector3 receivingCenter=transform.position+Vector3.up*hitCheckHeightOffset;
            if(!EnemyReceivingShape.Allows(enemy,other) || !EnemyReceivingShape.IntersectsCapsule(enemy,receivingCenter+Vector3.up*hitCheckVerticalHalfHeight,receivingCenter-Vector3.up*hitCheckVerticalHalfHeight,GetScaledHitCheckRadius()))return HitResult.Ignored;
            IProjectileItemEffects effects = HasItemEffects ? itemEffects : null;
            bool isBoomerang = spawnKind == SpawnKind.Normal && effects != null && effects.IsAvailable && effects.HasRefluxOrgan;
            if (isBoomerang && boomerangPassHitCount >= effects.GetBoomerangMaxHitsPerPass())
            {
                return HitResult.Ignored;
            }

            long enemyId = enemy.LogicalLife?.Id ?? (long.MinValue+unchecked((uint)enemy.GetInstanceID()));
            if (!hitEnemyIds.Add(enemyId))
            {
                return HitResult.Ignored;
            }

            float appliedDamage = damage;
            if (effects != null && effects.IsAvailable && effects.HasPiercingMucus && !isBoomerang)
            {
                appliedDamage *= effects.GetPiercingHitDamageMultiplier(currentHitCount);
            }

            if (isBoomerang && returning)
            {
                appliedDamage *= effects.GetBoomerangRepeatHitDamageMultiplier();
            }

            if (effects != null && effects.IsAvailable)
            {
                appliedDamage = effects.ApplyPerTargetDamageModifiers(enemy, appliedDamage);
            }

            currentHitCount++;
            if (isBoomerang)
            {
                boomerangPassHitCount++;
            }
            bool terminates = currentHitCount >= maxHitCount;
            if (terminates) hasImpacted = true; // Claim the final hit before damage callbacks can reenter.
            var hitLife = new EnemyHitLife(enemy);
            try
            {
                enemy.TakeDamage(appliedDamage, damageSource);
                if (!IsCurrentFlight(generation)) return HitResult.Terminated;
                CombatVfx.PlayProjectileImpact(transform.position, moveDirection);
                if (!IsCurrentFlight(generation)) return HitResult.Terminated;

                if (effects != null && effects.IsAvailable)
                {
                    effects.TryApplyPostDamageExecutionInstinct(enemy, appliedDamage, hitLife.Generation);
                    if (!IsCurrentFlight(generation)) return HitResult.Terminated;
                    effects.ApplyCommonOnHitEffects(enemy, appliedDamage, transform.position, hitLife.Generation);
                    if (!IsCurrentFlight(generation)) return HitResult.Terminated;

                    if (spawnKind == SpawnKind.Normal && effects.HasSplitTissue && !splitTriggered)
                    {
                        splitTriggered = true;
                        effects.SpawnSplitProjectiles(transform.position, moveDirection, appliedDamage, targetMask, GetRemainingRange());
                        if (!IsCurrentFlight(generation)) return HitResult.Terminated;
                    }

                    if (spawnKind == SpawnKind.Normal && effects.HasExplosiveBloodCell)
                    {
                        ApplyExplosionDamage(enemy, appliedDamage);
                        if (!IsCurrentFlight(generation)) return HitResult.Terminated;
                    }
                }

                return terminates ? HitResult.Terminated : HitResult.Continued;
            }
            finally
            {
                if (terminates) EndFlight(generation);
            }
        }

        private void TryDetectHitByOverlap()
        {
            uint generation = launchGeneration;
            if (hasImpacted || !IsCurrentFlight(generation))
            {
                return;
            }

            float radius = GetScaledHitCheckRadius();
            Vector3 hitCenter = transform.position;
            hitCenter.y += hitCheckHeightOffset;
            float halfHeight = Mathf.Max(0.05f, hitCheckVerticalHalfHeight);
            Vector3 capsuleTop = hitCenter + Vector3.up * halfHeight;
            Vector3 capsuleBottom = hitCenter - Vector3.up * halfHeight;

            var hits=EnemyHitSnapshot.Capsule(capsuleTop,capsuleBottom,
                radius,targetMask,QueryTriggerInteraction.Collide,ref hitBuffer);
            var hitData=hits;
            int count=hits.Length;

            for (int i = 0; i < count; i++)
            {
                if(!hitData[i].IsCurrent)continue;
                Collider collider = hitData[i].Collider;
                if (collider == null)
                {
                    continue;
                }

                HitResult result = HandleHit(collider);
                if (result == HitResult.Terminated || hasImpacted || !IsCurrentFlight(generation))
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
            if (!HasItemEffects || ownerTransform == null)
            {
                return;
            }

            float returnDistance = itemEffects.GetBoomerangReturnDistance(Mathf.Max(launchRange, 0.5f));
            if (!returning && traveledDistance >= returnDistance)
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

            if (!boomerangRehitResetDone)
            {
                // Allow one more hit pass while returning to the player.
                hitEnemyIds.Clear();
                boomerangPassHitCount = 0;
                boomerangRehitResetDone = true;
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
            int sampleCount = Mathf.Max(2, Mathf.CeilToInt(step.magnitude / 0.2f));
            Vector3 segment = step / sampleCount;
            Vector3 hitProbe = current;
            Vector3 preHitProbe = current;
            bool blocked = false;
            for (int i = 1; i <= sampleCount; i++)
            {
                Vector3 probe = current + segment * i;
                if (!IsWalkablePosition(biome, probe))
                {
                    hitProbe = probe;
                    preHitProbe = current + segment * (i - 1);
                    blocked = true;
                    break;
                }
            }

            if (!blocked)
            {
                return false;
            }

            Vector3 localStep = hitProbe - preHitProbe;
            bool xBlocked = !IsWalkablePosition(biome, preHitProbe + new Vector3(localStep.x, 0f, 0f));
            bool zBlocked = !IsWalkablePosition(biome, preHitProbe + new Vector3(0f, 0f, localStep.z));

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

        private bool TryReflectFromObstacleCollider(ref Vector3 step)
        {
            if (remainingBounces <= 0)
            {
                return false;
            }

            float stepDistance = step.magnitude;
            if (stepDistance <= 0.0001f)
            {
                return false;
            }

            Vector3 direction = step / stepDistance;
            float probeRadius = Mathf.Max(0.05f, GetScaledHitCheckRadius() * 0.65f);
            int hitCount=CompletePhysicsQueries.FillSphereCast(transform.position,probeRadius,direction,
                stepDistance+.02f,~0,QueryTriggerInteraction.Ignore,ref obstacleHitBuffer);
            var obstacles=obstacleHitBuffer;

            if (hitCount <= 0)
            {
                return false;
            }

            bool found = false;
            float nearestDistance = float.MaxValue;
            RaycastHit nearestHit = default;
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = obstacles[i];
                Collider obstacle = hit.collider;
                if (!IsValidObstacleCollider(obstacle))
                {
                    continue;
                }

                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    nearestHit = hit;
                    found = true;
                }
            }

            if (!found)
            {
                return false;
            }

            Vector3 normal = nearestHit.normal.sqrMagnitude > 0.0001f ? nearestHit.normal.normalized : -moveDirection;
            Vector3 reflectedDirection = Vector3.Reflect(moveDirection, normal);
            moveDirection = reflectedDirection.sqrMagnitude > 0.0001f ? reflectedDirection.normalized : -moveDirection;
            remainingBounces--;

            transform.position = nearestHit.point + normal * (probeRadius + 0.02f);
            step = moveDirection * speed * Time.deltaTime;
            return true;
        }

        private bool IsValidObstacleCollider(Collider obstacle)
        {
            if (obstacle == null || !obstacle.enabled || obstacle.isTrigger)
            {
                return false;
            }

            Transform obstacleTransform = obstacle.transform;
            if (obstacleTransform == transform || obstacleTransform.IsChildOf(transform))
            {
                return false;
            }

            if (ownerTransform != null && (obstacleTransform == ownerTransform || obstacleTransform.IsChildOf(ownerTransform)))
            {
                return false;
            }

            if (TryGetEnemyController(obstacle, out _))
            {
                return false;
            }

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
            if (!pulseScaleModeInitialized)
            {
                pulseScaleGrowMode = Random.value < 0.5f;
                pulseScaleModeInitialized = true;
            }

            float progress = Mathf.Clamp01(traveledDistance / Mathf.Max(0.05f, pulseRangeBudget));
            // Faster early growth/shrink so the effect is more visible right after firing.
            float acceleratedProgress = 1f - Mathf.Pow(1f - progress, 2.2f);
            float startScale = 1f;
            float endScale = pulseScaleGrowMode
                ? itemEffects.GetPulseAmplitude()
                : itemEffects.GetPulseFrequency();

            float scaleRatio = Mathf.Lerp(startScale, endScale, acceleratedProgress);
            transform.localScale = activeBaseScale * scaleRatio;
        }

        private float GetScaledHitCheckRadius()
        {
            float defaultSize = Mathf.Max(
                0.0001f,
                Mathf.Max(
                    Mathf.Abs(defaultLocalScale.x),
                    Mathf.Max(Mathf.Abs(defaultLocalScale.y), Mathf.Abs(defaultLocalScale.z))));
            float currentSize = Mathf.Max(
                Mathf.Abs(transform.localScale.x),
                Mathf.Max(Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z)));
            float scaleRatio = Mathf.Max(0.05f, currentSize / defaultSize);
            return Mathf.Max(.05f, Mathf.Max(hitCheckRadius * scaleRatio, PlayerAttackGeometry.ColliderRadius(hitCollider))) + PlayerAttackGeometry.Padding;
        }

        private void ApplyExplosionDamage(EnemyController primaryEnemy, float sourceDamage)
        {
            uint generation = launchGeneration;
            if (!IsCurrentFlight(generation)) return;
            IProjectileItemEffects effects = HasItemEffects ? itemEffects : null;
            if (effects == null) return;
            float radius = effects.GetExplosionRadius();
            var hits=EnemyHitSnapshot.Sphere(transform.position,radius,
                targetMask,QueryTriggerInteraction.Collide,ref explosionBuffer);
            var hitData=hits;
            int hitCount=hits.Length;
            var explosionEnemies=new HashSet<EnemyController>();

            HealthEffectSource explosionSource = effects.GetItemHealthEffectSource(PlayerItemIds.ExplosiveBloodCellId, "explosion");
            float explosionDamage = Mathf.Max(0f, sourceDamage * effects.GetExplosionDamageMultiplier());
            ResidueRubble.HitArea(transform.position, radius, Vector3.forward, 180, explosionDamage, targetMask, propAttackToken);
            if (!IsCurrentFlight(generation)) return;
            SpawnExplosionVisual(transform.position, radius);
            if (!IsCurrentFlight(generation)) return;
            for (int i = 0; i < hitCount; i++)
            {
                if(!hitData[i].IsCurrent)continue;
                Collider collider = hitData[i].Collider;
                if (collider == null)
                {
                    continue;
                }

                EnemyController enemy=hitData[i].Enemy;
                if (enemy==null || enemy.IsDead || enemy==primaryEnemy || !EnemyReceivingShape.IntersectsSphere(enemy,transform.position,radius) || !explosionEnemies.Add(enemy))
                {
                    continue;
                }

                float appliedExplosionDamage = effects.ApplyPerTargetDamageModifiers(enemy, explosionDamage);
                var hitLife = new EnemyHitLife(enemy);
                enemy.TakeDamage(appliedExplosionDamage, explosionSource);
                if (!IsCurrentFlight(generation)) return;
                effects.TryApplyPostDamageExecutionInstinct(enemy, appliedExplosionDamage, hitLife.Generation);
                if (!IsCurrentFlight(generation)) return;
                effects.ApplyCommonOnHitEffects(enemy, appliedExplosionDamage, transform.position, hitLife.Generation);
                if (!IsCurrentFlight(generation)) return;
            }
        }

        private static void SpawnExplosionVisual(Vector3 center, float radius)
        {
            Sprite effectSprite = TextureSpriteCache.LoadResourceSprite("ItemEffects/explosive_blood_cell_effect");
            ItemEffectVisuals.SpawnPooledCircleVisual(
                ExplosionVisualPoolName,
                "ExplosiveBloodCellFx",
                new Vector3(center.x, center.y + 0.08f, center.z),
                Mathf.Max(0.2f, radius * 2f * 0.85f),
                effectSprite != null ? Color.white : new Color(1f, 0.26f, 0.12f, 0.55f),
                5100,
                0.2f,
                effectSprite);
        }

        private float GetRemainingRange()
        {
            float remainingTime = Mathf.Max(0f, deactivateTime - Time.time);
            return remainingTime * Mathf.Max(0.01f, currentSpeed);
        }

        private static bool TryGetEnemyController(Component source, out EnemyController enemy)
        {
            if (source != null)
            {
                Transform current = source.transform;
                while (current != null)
                {
                    if (current.TryGetComponent(out enemy))
                    {
                        return true;
                    }

                    current = current.parent;
                }
            }

            enemy = null;
            return false;
        }

        private void CacheDefaultScale()
        {
            if (hitCollider == null) hitCollider = GetComponent<Collider>();
            if (defaultScaleCached)
            {
                return;
            }

            defaultLocalScale = transform.localScale;
            defaultScaleCached = true;
        }

        private void OnDisable()
        {
            unchecked { launchGeneration++; }
            hasImpacted = true;
            if (defaultScaleCached)
            {
                transform.localScale = defaultLocalScale;
            }

            hitEnemyIds.Clear();
            itemEffects = null;
            returning = false;
            splitTriggered = false;
            boomerangRehitResetDone = false;
            pulseScaleModeInitialized = false;
            pulseRangeBudget = 0f;
            currentHitCount = 0;
            remainingBounces = 0;
            launchRange = 0f;
            activeBaseScale = defaultLocalScale;
            currentSpeed = Mathf.Max(0.01f, speed);
        }
    }

}
