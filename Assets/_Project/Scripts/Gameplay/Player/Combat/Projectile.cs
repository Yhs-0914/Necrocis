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
        private float currentSpeed;
        private float deactivateTime;
        private bool hasImpacted;
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
        private PlayerItemCombatEffects itemEffects;
        private Transform ownerTransform;
        private float launchRange;
        private Vector3 activeBaseScale = Vector3.one;
        private bool boomerangRehitResetDone;
        private int boomerangPassHitCount;
        private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
        private readonly Collider[] explosionBuffer = new Collider[ExplosionBufferSize];
        private readonly RaycastHit[] obstacleHitBuffer = new RaycastHit[ObstacleHitBufferSize];
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
            if (spawnKind == SpawnKind.Normal && itemEffects != null)
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
            if (spawnKind == SpawnKind.Normal && itemEffects != null)
            {
                scaleMultiplier = itemEffects.GetScaleMultiplier();
            }
            transform.localScale = defaultLocalScale * scaleMultiplier;
            activeBaseScale = transform.localScale;

            float effectiveRange = launchRange;
            if (spawnKind == SpawnKind.Normal && itemEffects != null)
            {
                effectiveRange *= itemEffects.GetRangeMultiplier();
                if (itemEffects.HasRefluxOrgan)
                {
                    // Outbound + return travel budget.
                    effectiveRange *= 2f;
                }
            }
            pulseRangeBudget = effectiveRange;
            if (spawnKind == SpawnKind.Normal && itemEffects != null && itemEffects.HasPulseBullet)
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
            hasImpacted = false;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (spawnKind == SpawnKind.Normal && itemEffects != null)
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
            transform.position = nextPosition;
            traveledDistance += reflected ? step.magnitude : stepDistance;

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

            if (!TryGetEnemyController(other, out EnemyController enemy) || enemy.IsDead)
            {
                return;
            }

            bool isBoomerang = spawnKind == SpawnKind.Normal && itemEffects != null && itemEffects.HasRefluxOrgan;
            if (isBoomerang && boomerangPassHitCount >= itemEffects.GetBoomerangMaxHitsPerPass())
            {
                return;
            }

            int enemyId = enemy.GetInstanceID();
            if (!hitEnemyIds.Add(enemyId))
            {
                return;
            }

            float appliedDamage = damage;
            if (itemEffects != null && itemEffects.HasPiercingMucus && !isBoomerang)
            {
                appliedDamage *= itemEffects.GetPiercingHitDamageMultiplier(currentHitCount);
            }

            if (isBoomerang && returning)
            {
                appliedDamage *= itemEffects.GetBoomerangRepeatHitDamageMultiplier();
            }

            if (itemEffects != null)
            {
                appliedDamage = itemEffects.ApplyPerTargetDamageModifiers(enemy, appliedDamage);
            }

            currentHitCount++;
            if (isBoomerang)
            {
                boomerangPassHitCount++;
            }
            enemy.TakeDamage(appliedDamage);
            CombatVfx.PlayProjectileImpact(transform.position, moveDirection);

            if (itemEffects != null)
            {
                itemEffects.TryApplyPostDamageExecutionInstinct(enemy, appliedDamage);
                itemEffects.ApplyCommonOnHitEffects(enemy, appliedDamage, transform.position);

                if (spawnKind == SpawnKind.Normal && itemEffects.HasSplitTissue && !splitTriggered)
                {
                    splitTriggered = true;
                    itemEffects.SpawnSplitProjectiles(transform.position, moveDirection, appliedDamage, targetMask, GetRemainingRange());
                }

                if (spawnKind == SpawnKind.Normal && itemEffects.HasExplosiveBloodCell)
                {
                    ApplyExplosionDamage(enemy, appliedDamage);
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

            float radius = GetScaledHitCheckRadius();
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
            int hitCount = Physics.SphereCastNonAlloc(
                transform.position,
                probeRadius,
                direction,
                obstacleHitBuffer,
                stepDistance + 0.02f,
                ~0,
                QueryTriggerInteraction.Ignore);

            if (hitCount <= 0)
            {
                return false;
            }

            bool found = false;
            float nearestDistance = float.MaxValue;
            RaycastHit nearestHit = default;
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = obstacleHitBuffer[i];
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
            return Mathf.Max(0.05f, hitCheckRadius * scaleRatio);
        }

        private void ApplyExplosionDamage(EnemyController primaryEnemy, float sourceDamage)
        {
            float radius = itemEffects.GetExplosionRadius();
            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                radius,
                explosionBuffer,
                targetMask,
                QueryTriggerInteraction.Collide);

            float explosionDamage = Mathf.Max(0f, sourceDamage * itemEffects.GetExplosionDamageMultiplier());
            SpawnExplosionVisual(transform.position, radius);
            for (int i = 0; i < hitCount; i++)
            {
                Collider collider = explosionBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                if (!TryGetEnemyController(collider, out EnemyController enemy)
                    || enemy.IsDead
                    || enemy == primaryEnemy)
                {
                    continue;
                }

                float appliedExplosionDamage = itemEffects.ApplyPerTargetDamageModifiers(enemy, explosionDamage);
                enemy.TakeDamage(appliedExplosionDamage);
                itemEffects.TryApplyPostDamageExecutionInstinct(enemy, appliedExplosionDamage);
                itemEffects.ApplyCommonOnHitEffects(enemy, appliedExplosionDamage, transform.position);
            }
        }

        private static void SpawnExplosionVisual(Vector3 center, float radius)
        {
            Sprite effectSprite = TextureSpriteCache.LoadResourceSprite("ItemEffects/explosive_blood_cell_effect");
            PlayerItemCombatEffects.SpawnPooledCircleVisual(
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

    internal static class TextureSpriteCache
    {
        private static Sprite circleSprite;
        private static Material spriteMaterial;
        private static readonly Dictionary<string, Sprite> ResourceSprites = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Material> ResourceSpriteMaterials = new Dictionary<string, Material>();

        public static Sprite LoadResourceSprite(string resourcePath, float pixelsPerUnit = 100f)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return null;
            }

            if (ResourceSprites.TryGetValue(resourcePath, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null)
            {
                Texture2D texture = Resources.Load<Texture2D>(resourcePath);
                if (texture != null)
                {
                    sprite = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        Mathf.Max(1f, pixelsPerUnit));
                    sprite.name = texture.name;
                }
            }

            ResourceSprites[resourcePath] = sprite;
            return sprite;
        }

        public static float GetUniformScaleForWorldSize(Sprite sprite, float targetWorldSize)
        {
            float spriteSize = sprite != null
                ? Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y)
                : 1f;
            return Mathf.Max(0.01f, targetWorldSize) / Mathf.Max(0.0001f, spriteSize);
        }

        public static Material GetResourceSpriteMaterial(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return GetSpriteMaterial();
            }

            if (ResourceSpriteMaterials.TryGetValue(resourcePath, out Material cachedMaterial))
            {
                return cachedMaterial;
            }

            Sprite sprite = LoadResourceSprite(resourcePath);
            Material baseMaterial = GetSpriteMaterial();
            if (sprite == null || baseMaterial == null)
            {
                return baseMaterial;
            }

            Material material = new Material(baseMaterial)
            {
                name = $"Runtime_{sprite.name}_Material",
                mainTexture = sprite.texture
            };
            ResourceSpriteMaterials[resourcePath] = material;
            return material;
        }

        public static Sprite GetCircleSprite()
        {
            if (circleSprite != null)
            {
                return circleSprite;
            }

            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            float center = (size - 1) * 0.5f;
            float radius = center - 1f;
            float innerRadius = radius * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    if (distance > radius)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    if (distance <= innerRadius)
                    {
                        texture.SetPixel(x, y, Color.white);
                        continue;
                    }

                    float alpha = Mathf.InverseLerp(radius, innerRadius, distance);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            circleSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size);
            circleSprite.name = "RuntimeCircleSprite";
            return circleSprite;
        }

        public static Material GetSpriteMaterial()
        {
            if (spriteMaterial != null)
            {
                return spriteMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }

            if (shader == null)
            {
                return null;
            }

            spriteMaterial = new Material(shader)
            {
                name = "RuntimeSpriteMaterial"
            };
            return spriteMaterial;
        }
    }
}
