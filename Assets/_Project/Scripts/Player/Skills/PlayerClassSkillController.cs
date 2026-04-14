using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Necrocis
{
    public enum PlayerClassType
    {
        None,
        Mage,
        Archer,
        Warrior
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController))]
    public class PlayerClassSkillController : MonoBehaviour
    {
        private const string SkillProjectileFallbackPoolName = "__SkillProjectileFallbackSphere";
        private const string SkillEffectFallbackPoolName = "__SkillEffectFallbackSphere";
        private const string SkillAttachedEffectFallbackPoolName = "__SkillAttachedEffectFallbackSphere";
        private const string ArcherSkill2FallbackProjectilePoolName = "__ArcherSkill2FallbackCylinder";

        [System.Serializable]
        private class MageSkill1Config
        {
            public float cooldown = 1f;
            public float radius = 3f;
            public float forwardOffset = 0f;
            public float baseDamage = 0f;
            public float attackPowerScale = 0f;
            public float additionalDamage = 0f;
            public float additionalDamageMin = 5f;
            public float additionalDamageMax = 7f;
            public float stunDuration = 1f;
            public GameObject areaEffectPrefab;
            public float areaEffectLifetime = 1f;
            public float fallbackEffectScale = 2.5f;
        }

        [System.Serializable]
        private class MageSkill2Config
        {
            public float cooldown = 3f;
            public float radius = 3.5f;
            public float forwardOffset = 0f;
            public float baseDamage = 15f;
            public float attackPowerScale = 0f;
            public float detonationDelay = 3f;
            public float damageTakenIncreaseRatio = 0.1f;
            public float damageTakenIncreaseDuration = 3f;
            public GameObject markEffectPrefab;
            public float markEffectLifetime = 3f;
            public float markFallbackEffectScale = 0.8f;
            public float markHeadOffset = 0.2f;
            public bool scaleEffectByTargetSize = true;
            public float effectReferenceTargetHeight = 1.1f;
            public float effectSizeMultiplier = 1f;
            public float effectMinScaleMultiplier = 0.7f;
            public float effectMaxScaleMultiplier = 2.5f;
            public GameObject explosionEffectPrefab;
            public float explosionEffectLifetime = 1f;
            public float fallbackEffectScale = 3f;
        }

        [System.Serializable]
        private class ArcherSkill1Config
        {
            public float cooldown = 1f;
            public int projectileCount = 5;
            public float fanAngle = 55f;
            public float projectileDamage = 3f;
            public float projectileSpeed = 16f;
            public float projectileRange = 4f;
            public float projectileLifeTime = 2f;
            public GameObject projectilePrefab;
            public float projectileScale = 0.25f;
            public float poisonDuration = 5f;
            public float poisonTickInterval = 1f;
            public float poisonTickDamage = 1f;
            public GameObject shootEffectPrefab;
            public float shootEffectLifetime = 0.4f;
        }

        [System.Serializable]
        private class ArcherSkill2Config
        {
            public float cooldown = 3f;
            public float aimDuration = 0.5f;
            public float range = 10f;
            [FormerlySerializedAs("lineHitRadius")]
            public float projectileHitRadius = 0.8f;
            public float projectileVisualLength = 4f;
            public float projectileVisualThickness = 0.8f;
            public float targetForwardOffset = 6f;
            public float projectileDamage = 10f;
            public float projectileTravelSpeed = 16f;
            public float projectileLifeTime = 0f;
            public GameObject projectilePrefab;
            public float projectileScale = 1f;
            [FormerlySerializedAs("poisonExplosionDamage")]
            public float virusExplosionDamage = 10f;
            [FormerlySerializedAs("poisonExplosionRadius")]
            public float virusExplosionRadius = 1.8f;
            [FormerlySerializedAs("poisonExplosionEffectPrefab")]
            public GameObject virusExplosionEffectPrefab;
            [FormerlySerializedAs("poisonExplosionEffectLifetime")]
            public float virusExplosionEffectLifetime = 1f;
        }

        [System.Serializable]
        private class WarriorSkill1Config
        {
            public float cooldown = 4f;
            public float range = 2.5f;
            public float damage = 6f;
            public float bleedDuration = 3f;
            public float bleedTickInterval = 1f;
            public float bleedTickDamage = 1.5f;
            public GameObject hitEffectPrefab;
            public float hitEffectLifetime = 0.5f;
            public float fallbackEffectScale = 0.8f;
        }

        [Header("Class")]
        [SerializeField] private PlayerClassType currentClass = PlayerClassType.Mage;

        [Header("Shared")]
        [SerializeField] private LayerMask enemyMask = ~0;
        [SerializeField] private int overlapBufferSize = 32;
        [SerializeField] private Transform skillSpawnPoint;
        [SerializeField] private float projectileSpawnOffset = 0.65f;
        [SerializeField] private float projectileSpawnHeight = 1f;
        [SerializeField] private float skillVerticalOffset = 2f;
        [SerializeField] private float skillHitHeightOffset = 0.75f;
        [SerializeField] private float skillHitVerticalHalfHeight = 4f;
        [SerializeField] private bool enableDebugLogs = true;

        [Header("Mage")]
        [SerializeField] private MageSkill1Config mageSkill1 = new MageSkill1Config();
        [SerializeField] private MageSkill2Config mageSkill2 = new MageSkill2Config();

        [Header("Archer")]
        [SerializeField] private ArcherSkill1Config archerSkill1 = new ArcherSkill1Config();
        [SerializeField] private ArcherSkill2Config archerSkill2 = new ArcherSkill2Config();

        [Header("Warrior")]
        [SerializeField] private WarriorSkill1Config warriorSkill1 = new WarriorSkill1Config();
        [SerializeField] private bool autoTargetForwardEnemyForArcherSkill2 = true;
        [SerializeField] private float archerSkill2AutoTargetAngle = 90f;

        [Header("Test Cooldown Override")]
        [SerializeField] private bool useTestCooldownOverride = true;
        [SerializeField] private float testSkill1Cooldown = 1f;
        [SerializeField] private float testSkill2Cooldown = 3f;

        private readonly HashSet<EnemyController> uniqueEnemies = new HashSet<EnemyController>();

        private PlayerController playerController;
        private Collider[] overlapBuffer;

        private float nextSkill1ReadyTime;
        private float nextSkill2ReadyTime;

        private bool archerSkill2Running;

        public bool ConsumesSkillInput => enabled && currentClass != PlayerClassType.None;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            if (enemyMask.value == 0)
            {
                enemyMask = ~0;
                Debug.LogWarning("[PlayerClassSkillController] enemyMask was Nothing. Fallback to Everything.");
            }

            ApplyTestCooldownOverrideIfNeeded();
            EnsureOverlapBuffer();
            ResolveSkillSpawnPoint();
        }

        private void Update()
        {
            if (!ShouldAcceptInput())
            {
                return;
            }

            InputManager input = InputManager.Instance;
            if (input == null)
            {
                return;
            }

            if (input.Skill1Action.WasPressedThisFrame())
            {
                TryUseSkill1();
            }

            if (input.Skill2Action.WasPressedThisFrame())
            {
                TryUseSkill2();
            }
        }

        private void OnDisable()
        {
            archerSkill2Running = false;
            StopAllCoroutines();
        }

        private bool ShouldAcceptInput()
        {
            if (!Application.isFocused || Time.timeSinceLevelLoad < 0.5f)
            {
                return false;
            }

            if (playerController == null || playerController.IsDead)
            {
                return false;
            }

            return currentClass != PlayerClassType.None;
        }

        private void ApplyTestCooldownOverrideIfNeeded()
        {
            if (!useTestCooldownOverride)
            {
                return;
            }

            float skill1Cooldown = Mathf.Max(0f, testSkill1Cooldown);
            float skill2Cooldown = Mathf.Max(0f, testSkill2Cooldown);

            mageSkill1.cooldown = skill1Cooldown;
            archerSkill1.cooldown = skill1Cooldown;
            warriorSkill1.cooldown = skill1Cooldown;
            mageSkill2.cooldown = skill2Cooldown;
            archerSkill2.cooldown = skill2Cooldown;
        }

        private void TryUseSkill1()
        {
            switch (currentClass)
            {
                case PlayerClassType.Mage:
                    if (!TryStartCooldown(ref nextSkill1ReadyTime, mageSkill1.cooldown, "Mage Skill E"))
                    {
                        return;
                    }

                    ExecuteMageSkill1();
                    break;

                case PlayerClassType.Archer:
                    if (!TryStartCooldown(ref nextSkill1ReadyTime, archerSkill1.cooldown, "Archer Skill E"))
                    {
                        return;
                    }

                    ExecuteArcherSkill1FanShot();
                    break;

                case PlayerClassType.Warrior:
                    if (!TryStartCooldown(ref nextSkill1ReadyTime, warriorSkill1.cooldown, "Warrior Skill E"))
                    {
                        return;
                    }

                    ExecuteWarriorSkill1Bite();
                    break;
            }
        }

        private void TryUseSkill2()
        {
            switch (currentClass)
            {
                case PlayerClassType.Mage:
                    Vector3 mageSkill2Center = GetSkillCenter(mageSkill2.forwardOffset);
                    if (!TryFindNearestEnemyInRadius(mageSkill2Center, mageSkill2.radius, out EnemyController mageSkill2Target))
                    {
                        if (enableDebugLogs)
                        {
                            Debug.Log("Mage Skill R failed: no enemy in range.");
                        }

                        return;
                    }

                    if (!TryStartCooldown(ref nextSkill2ReadyTime, mageSkill2.cooldown, "Mage Skill R"))
                    {
                        return;
                    }

                    StartCoroutine(ExecuteMageSkill2(mageSkill2Target));
                    break;

                case PlayerClassType.Archer:
                    if (archerSkill2Running)
                    {
                        return;
                    }

                    if (!TryStartCooldown(ref nextSkill2ReadyTime, archerSkill2.cooldown, "Archer Skill R"))
                    {
                        return;
                    }

                    StartCoroutine(ExecuteArcherSkill2());
                    break;
            }
        }

        private bool TryStartCooldown(ref float nextReadyTime, float cooldown, string label)
        {
            float now = Time.time;
            if (now < nextReadyTime)
            {
                if (enableDebugLogs)
                {
                    float remain = Mathf.Max(0f, nextReadyTime - now);
                    Debug.Log($"[{label}] Cooldown: {remain:0.00}s");
                }

                return false;
            }

            nextReadyTime = now + Mathf.Max(0f, cooldown);
            return true;
        }

        private void ExecuteMageSkill1()
        {
            Vector3 center = GetSkillCenter(mageSkill1.forwardOffset);
            float baseDamage = mageSkill1.baseDamage + playerController.AttackPower * mageSkill1.attackPowerScale;
            float bonusMin = Mathf.Min(mageSkill1.additionalDamageMin, mageSkill1.additionalDamageMax);
            float bonusMax = Mathf.Max(mageSkill1.additionalDamageMin, mageSkill1.additionalDamageMax);
            if (bonusMax <= 0f && mageSkill1.additionalDamage > 0f)
            {
                bonusMin = mageSkill1.additionalDamage;
                bonusMax = mageSkill1.additionalDamage;
            }

            int hitCount = ApplyAreaSkill(
                center,
                mageSkill1.radius,
                enemy =>
                {
                    float bonusDamage = Random.Range(bonusMin, bonusMax + 0.001f);
                    float totalDamage = Mathf.Max(0f, baseDamage + bonusDamage);
                    enemy.TakeDamage(totalDamage);
                    EnemyStatusEffectController status = EnsureStatusController(enemy);
                    status?.ApplyStun(mageSkill1.stunDuration);
                });

            SpawnSkillEffect(
                mageSkill1.areaEffectPrefab,
                center,
                mageSkill1.areaEffectLifetime,
                mageSkill1.fallbackEffectScale,
                new Color(0.35f, 0.8f, 1f, 0.45f));

            if (enableDebugLogs)
            {
                Debug.Log($"Mage Skill E hit {hitCount} enemies. BonusDamage={bonusMin:0.#}~{bonusMax:0.#}");
            }
        }

        private void ExecuteWarriorSkill1Bite()
        {
            Vector3 center = GetSkillCenter(0f);
            if (!TryFindNearestEnemyInRadius(center, warriorSkill1.range, out EnemyController target))
            {
                if (enableDebugLogs)
                {
                    Debug.Log("Warrior Skill E failed: no enemy in range.");
                }
                return;
            }

            target.TakeDamage(warriorSkill1.damage);

            EnemyStatusEffectController status = EnsureStatusController(target);
            status?.ApplyBleed(warriorSkill1.bleedDuration, warriorSkill1.bleedTickInterval, warriorSkill1.bleedTickDamage);

            Vector3 effectPos = GetTargetEffectPosition(target);
            SpawnSkillEffect(
                warriorSkill1.hitEffectPrefab,
                effectPos,
                warriorSkill1.hitEffectLifetime,
                warriorSkill1.fallbackEffectScale,
                new Color(0.85f, 0.1f, 0.1f, 0.6f));

            if (enableDebugLogs)
            {
                Debug.Log($"Warrior Skill E hit {target.name}. Damage={warriorSkill1.damage}, Bleed={warriorSkill1.bleedTickDamage}/s for {warriorSkill1.bleedDuration}s");
            }
        }

        private IEnumerator ExecuteMageSkill2(EnemyController target)
        {
            if (target == null || target.IsDead)
            {
                yield break;
            }

            float markLifeTime = Mathf.Max(0.1f, Mathf.Min(mageSkill2.markEffectLifetime, mageSkill2.detonationDelay + 0.05f));
            SpawnAttachedSkillEffect(
                mageSkill2.markEffectPrefab,
                target,
                markLifeTime,
                mageSkill2.markFallbackEffectScale,
                new Color(0.8f, 0.95f, 1f, 0.5f),
                mageSkill2.markHeadOffset);

            float delay = Mathf.Max(0f, mageSkill2.detonationDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (target == null || target.IsDead)
            {
                if (enableDebugLogs)
                {
                    Debug.Log("Mage Skill R canceled: marked target died before detonation.");
                }
                yield break;
            }

            Vector3 targetEffectPosition = GetTargetEffectPosition(target);
            float targetEffectScaleMultiplier = GetTargetEffectScaleMultiplier(target);
            float damage = mageSkill2.baseDamage + playerController.AttackPower * mageSkill2.attackPowerScale;
            target.TakeDamage(damage);
            EnemyStatusEffectController status = EnsureStatusController(target);
            status?.ApplyDamageTakenIncrease(mageSkill2.damageTakenIncreaseRatio, mageSkill2.damageTakenIncreaseDuration);

            SpawnSkillEffect(
                mageSkill2.explosionEffectPrefab,
                targetEffectPosition,
                mageSkill2.explosionEffectLifetime,
                mageSkill2.fallbackEffectScale,
                new Color(1f, 0.5f, 0.2f, 0.45f),
                targetEffectScaleMultiplier);

            if (enableDebugLogs)
            {
                Debug.Log($"Mage Skill R detonated on {target.name}. Damage={damage:0.##}, Debuff={mageSkill2.damageTakenIncreaseRatio * 100f:0.#}% for {mageSkill2.damageTakenIncreaseDuration:0.##}s");
            }
        }

        private void ExecuteArcherSkill1FanShot()
        {
            SkillProjectileDebuff debuff = new SkillProjectileDebuff
            {
                applyPoison = true,
                poisonDuration = archerSkill1.poisonDuration,
                poisonTickInterval = archerSkill1.poisonTickInterval,
                poisonTickDamage = archerSkill1.poisonTickDamage
            };

            int projectileCount = Mathf.Max(1, archerSkill1.projectileCount);
            float fanAngle = Mathf.Max(0f, archerSkill1.fanAngle);
            projectileCount = Mathf.Max(5, projectileCount);

            float projectileSpeed = Mathf.Max(0.01f, archerSkill1.projectileSpeed);
            float targetRange = archerSkill1.projectileRange > 0f ? archerSkill1.projectileRange : 4f;
            float projectileLifeTime = Mathf.Max(0.05f, targetRange / projectileSpeed);

            Vector3 centerDirection = GetFacingDirection();
            float startAngle = -fanAngle * 0.5f;
            float stepAngle = projectileCount > 1 ? fanAngle / (projectileCount - 1) : 0f;

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = startAngle + stepAngle * i;
                Vector3 shotDirection = Quaternion.AngleAxis(angle, Vector3.up) * centerDirection;

                SpawnSkillProjectile(
                    archerSkill1.projectilePrefab,
                    archerSkill1.projectileScale,
                    archerSkill1.projectileDamage,
                    projectileSpeed,
                    projectileLifeTime,
                    shotDirection,
                    debuff,
                    false);
            }

            SpawnSkillEffect(
                archerSkill1.shootEffectPrefab,
                GetProjectileSpawnPosition(centerDirection),
                archerSkill1.shootEffectLifetime,
                0.25f,
                new Color(0.7f, 1f, 0.5f, 0.55f));

            if (enableDebugLogs)
            {
                Debug.Log($"Archer Skill E fan-shot fired {projectileCount} piercing projectiles. FanAngle={fanAngle:0.#}, Range={targetRange:0.#}");
            }
        }

        private IEnumerator ExecuteArcherSkill2()
        {
            archerSkill2Running = true;
            Vector3 direction = GetArcherSkill2Direction();

            float aimDuration = Mathf.Max(0f, archerSkill2.aimDuration);
            if (aimDuration > 0f)
            {
                yield return new WaitForSeconds(aimDuration);
            }

            FireArcherSkill2Projectile(direction);
            if (enableDebugLogs)
            {
                Debug.Log("Archer Skill R fired a piercing cell spear.");
            }

            archerSkill2Running = false;
        }

        private Vector3 GetArcherSkill2Direction()
        {
            Vector3 fallbackDirection = GetFacingDirection();
            if (!autoTargetForwardEnemyForArcherSkill2)
            {
                return fallbackDirection;
            }

            float maxDistance = Mathf.Max(archerSkill2.range, archerSkill2.targetForwardOffset, 1f);
            if (!TryFindForwardEnemyPoint(maxDistance, archerSkill2AutoTargetAngle, out Vector3 targetPoint))
            {
                return fallbackDirection;
            }

            Vector3 toTarget = targetPoint - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                return fallbackDirection;
            }

            return toTarget.normalized;
        }

        private void FireArcherSkill2Projectile(Vector3 direction)
        {
            Vector3 moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : GetFacingDirection();
            float range = Mathf.Max(0.5f, archerSkill2.range);
            float speed = Mathf.Max(0.1f, archerSkill2.projectileTravelSpeed);
            float computedLifeTime = range / speed;
            float lifeTime = archerSkill2.projectileLifeTime > 0f
                ? archerSkill2.projectileLifeTime
                : computedLifeTime;

            Vector3 spawnPosition = GetProjectileSpawnPosition(moveDirection);
            GameObject projectileObject = CreateArcherSkill2ProjectileObject(spawnPosition, moveDirection);
            if (projectileObject == null)
            {
                return;
            }

            SkillProjectile projectile = projectileObject.GetComponent<SkillProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<SkillProjectile>();
            }

            projectile.ConfigureHitDetection(
                archerSkill2.projectileHitRadius,
                skillHitHeightOffset,
                skillHitVerticalHalfHeight);

            projectile.Launch(
                moveDirection,
                archerSkill2.projectileDamage,
                speed,
                Mathf.Max(0.05f, lifeTime),
                enemyMask,
                false,
                default(SkillProjectileDebuff),
                HandleArcherSkill2EnemyHit);
        }

        private GameObject CreateArcherSkill2ProjectileObject(Vector3 spawnPosition, Vector3 direction)
        {
            GameObject projectileObject = archerSkill2.projectilePrefab != null
                ? RuntimePool.Acquire(archerSkill2.projectilePrefab)
                : AcquireFallbackVisual(
                    ArcherSkill2FallbackProjectilePoolName,
                    PrimitiveType.Cylinder,
                    archerSkill2.projectileScale,
                    new Color(0.95f, 0.72f, 0.25f, 0.9f));
            if (projectileObject == null)
            {
                return null;
            }

            EnsureProjectilePhysics(projectileObject);
            projectileObject.transform.position = spawnPosition;
            projectileObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);

            if (archerSkill2.projectilePrefab != null)
            {
                projectileObject.transform.localScale *= Mathf.Max(0.01f, archerSkill2.projectileScale);
            }
            else
            {
                float thickness = Mathf.Max(0.1f, archerSkill2.projectileVisualThickness);
                float length = Mathf.Max(0.5f, archerSkill2.projectileVisualLength);
                projectileObject.transform.localScale = new Vector3(thickness, length * 0.5f, thickness);
            }

            return projectileObject;
        }

        private void HandleArcherSkill2EnemyHit(EnemyController enemy, Vector3 hitPosition)
        {
            if (enemy == null)
            {
                return;
            }

            EnemyStatusEffectController status = EnsureStatusController(enemy);
            if (status == null || !status.IsPoisoned)
            {
                return;
            }

            TriggerArcherSkill2VirusExplosion(hitPosition);
        }

        private void TriggerArcherSkill2VirusExplosion(Vector3 worldCenter)
        {
            Vector3 center = worldCenter;
            center.y = transform.position.y + skillVerticalOffset;

            int explosionHit = ApplyAreaSkill(
                center,
                archerSkill2.virusExplosionRadius,
                enemy => enemy.TakeDamage(archerSkill2.virusExplosionDamage));

            SpawnSkillEffect(
                archerSkill2.virusExplosionEffectPrefab,
                center,
                archerSkill2.virusExplosionEffectLifetime,
                Mathf.Max(0.25f, archerSkill2.virusExplosionRadius * 0.9f),
                new Color(1f, 0.45f, 0.2f, 0.45f));

            if (enableDebugLogs)
            {
                Debug.Log($"[Archer Skill R] Virus explosion triggered. Hit={explosionHit}, Damage={archerSkill2.virusExplosionDamage:0.##}");
            }
        }

        private bool TryFindForwardEnemyPoint(float maxDistance, float maxAngle, out Vector3 point)
        {
            point = Vector3.zero;
            EnsureOverlapBuffer();

            Vector3 forward = GetFacingDirection();
            Vector3 origin = transform.position;
            origin.y += skillHitHeightOffset;

            int count = Physics.OverlapSphereNonAlloc(
                origin,
                Mathf.Max(0.1f, maxDistance),
                overlapBuffer,
                enemyMask,
                QueryTriggerInteraction.Collide);

            EnemyController bestEnemy = null;
            float bestScore = float.NegativeInfinity;
            float halfAngle = Mathf.Max(1f, maxAngle) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                EnemyController enemy = collider.GetComponentInParent<EnemyController>();
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - transform.position;
                toEnemy.y = 0f;
                float distance = toEnemy.magnitude;
                if (distance <= 0.01f || distance > maxDistance)
                {
                    continue;
                }

                Vector3 toEnemyDir = toEnemy / distance;
                float angle = Vector3.Angle(forward, toEnemyDir);
                if (angle > halfAngle)
                {
                    continue;
                }

                float forwardScore = Vector3.Dot(forward, toEnemyDir) * 2f;
                float distanceScore = -Mathf.Abs(distance - archerSkill2.targetForwardOffset) * 0.15f;
                float score = forwardScore + distanceScore;
                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestEnemy = enemy;
            }

            if (bestEnemy == null)
            {
                return false;
            }

            point = bestEnemy.transform.position;
            point.y = transform.position.y + skillVerticalOffset;
            return true;
        }

        private int ApplyAreaSkill(Vector3 center, float radius, System.Action<EnemyController> apply)
        {
            EnsureOverlapBuffer();
            uniqueEnemies.Clear();
            float safeRadius = Mathf.Max(0f, radius);

            Vector3 hitCenter = center;
            hitCenter.y += skillHitHeightOffset;
            float halfHeight = Mathf.Max(0.05f, skillHitVerticalHalfHeight);
            Vector3 capsuleTop = hitCenter + Vector3.up * halfHeight;
            Vector3 capsuleBottom = hitCenter - Vector3.up * halfHeight;

            int count = Physics.OverlapCapsuleNonAlloc(
                capsuleTop,
                capsuleBottom,
                safeRadius,
                overlapBuffer,
                enemyMask,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                if (!TryGetEnemyFromCollider(collider, out EnemyController enemy))
                {
                    continue;
                }

                if (!uniqueEnemies.Add(enemy))
                {
                    continue;
                }

                apply?.Invoke(enemy);
            }

            // Fallback: if colliders are missing or overlap filtering misses enemies,
            // run a lightweight distance check against active EnemyController instances.
            if (uniqueEnemies.Count == 0)
            {
                int fallbackHits = ApplyAreaSkillDistanceFallback(center, safeRadius, apply);
                if (enableDebugLogs && fallbackHits > 0)
                {
                    Debug.Log($"[SkillHit] Overlap fallback applied {fallbackHits} enemies.");
                }
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[SkillHit] Center={center}, HitCenter={hitCenter}, Radius={safeRadius:0.##}, OverlapCount={count}, UniqueHit={uniqueEnemies.Count}");
            }

            return uniqueEnemies.Count;
        }

        private bool TryFindNearestEnemyInRadius(Vector3 center, float radius, out EnemyController nearestEnemy)
        {
            nearestEnemy = null;
            EnsureOverlapBuffer();

            float safeRadius = Mathf.Max(0f, radius);
            Vector3 hitCenter = center;
            hitCenter.y += skillHitHeightOffset;
            float halfHeight = Mathf.Max(0.05f, skillHitVerticalHalfHeight);
            Vector3 capsuleTop = hitCenter + Vector3.up * halfHeight;
            Vector3 capsuleBottom = hitCenter - Vector3.up * halfHeight;

            int count = Physics.OverlapCapsuleNonAlloc(
                capsuleTop,
                capsuleBottom,
                safeRadius,
                overlapBuffer,
                enemyMask,
                QueryTriggerInteraction.Collide);

            float bestDistanceSqr = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null || !TryGetEnemyFromCollider(collider, out EnemyController enemy))
                {
                    continue;
                }

                Vector3 enemyPos = enemy.transform.position;
                enemyPos.y = center.y;
                float distanceSqr = (enemyPos - center).sqrMagnitude;
                if (distanceSqr > bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }

            if (nearestEnemy != null)
            {
                return true;
            }

            IReadOnlyList<EnemyController> enemies = EnemyController.ActiveEnemyControllers;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 enemyPos = enemy.transform.position;
                enemyPos.y = center.y;
                float distanceSqr = (enemyPos - center).sqrMagnitude;
                if (distanceSqr > safeRadius * safeRadius || distanceSqr > bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }

            return nearestEnemy != null;
        }

        private Vector3 GetTargetEffectPosition(EnemyController target)
        {
            if (target == null)
            {
                return GetSkillCenter(0f);
            }

            if (TargetAttachedEffect.TryGetTargetBounds(target.transform, out Bounds targetBounds))
            {
                return targetBounds.center;
            }

            return target.transform.position;
        }

        private float GetTargetEffectScaleMultiplier(EnemyController target)
        {
            if (!mageSkill2.scaleEffectByTargetSize || target == null)
            {
                return 1f;
            }

            float minMultiplier = Mathf.Max(0.05f, mageSkill2.effectMinScaleMultiplier);
            float maxMultiplier = Mathf.Max(minMultiplier, mageSkill2.effectMaxScaleMultiplier);
            float referenceHeight = Mathf.Max(0.01f, mageSkill2.effectReferenceTargetHeight);
            float sizeMultiplier = Mathf.Max(0.01f, mageSkill2.effectSizeMultiplier);

            if (!TargetAttachedEffect.TryGetTargetBounds(target.transform, out Bounds targetBounds))
            {
                return Mathf.Clamp(sizeMultiplier, minMultiplier, maxMultiplier);
            }

            float targetHeight = Mathf.Max(0.01f, targetBounds.size.y);
            float scaleMultiplier = (targetHeight / referenceHeight) * sizeMultiplier;
            return Mathf.Clamp(scaleMultiplier, minMultiplier, maxMultiplier);
        }

        private int ApplyAreaSkillDistanceFallback(Vector3 center, float radius, System.Action<EnemyController> apply)
        {
            IReadOnlyList<EnemyController> enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return 0;
            }

            int hitCount = 0;
            float radiusSqr = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 enemyPos = enemy.transform.position;
                enemyPos.y = center.y;
                if ((enemyPos - center).sqrMagnitude > radiusSqr)
                {
                    continue;
                }

                if (!uniqueEnemies.Add(enemy))
                {
                    continue;
                }

                apply?.Invoke(enemy);
                hitCount++;
            }

            return hitCount;
        }

        private static bool TryGetEnemyFromCollider(Collider collider, out EnemyController enemy)
        {
            enemy = null;
            if (collider == null)
            {
                return false;
            }

            enemy = collider.GetComponent<EnemyController>();
            if (enemy == null)
            {
                enemy = collider.GetComponentInParent<EnemyController>();
            }

            return enemy != null && !enemy.IsDead;
        }

        private static EnemyStatusEffectController EnsureStatusController(EnemyController enemy)
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

        private void SpawnSkillProjectile(
            GameObject prefab,
            float fallbackScale,
            float damage,
            float speed,
            float lifeTime,
            Vector3 direction,
            SkillProjectileDebuff debuff,
            bool shouldDisableOnHit = true)
        {
            SpawnSkillProjectile(prefab, fallbackScale, damage, speed, lifeTime, direction, debuff, null, shouldDisableOnHit);
        }

        private void SpawnSkillProjectile(
            GameObject prefab,
            float fallbackScale,
            float damage,
            float speed,
            float lifeTime,
            Vector3 direction,
            SkillProjectileDebuff debuff,
            Vector3? worldSpawnPosition,
            bool shouldDisableOnHit = true)
        {
            Vector3 spawnPosition = worldSpawnPosition ?? GetProjectileSpawnPosition(direction);
            GameObject projectileObject = CreateProjectileObject(prefab, spawnPosition, fallbackScale);
            if (projectileObject == null)
            {
                return;
            }

            SkillProjectile projectile = projectileObject.GetComponent<SkillProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<SkillProjectile>();
            }

            projectile.Launch(direction, damage, speed, lifeTime, enemyMask, shouldDisableOnHit, debuff);
        }

        private GameObject CreateProjectileObject(GameObject prefab, Vector3 position, float fallbackScale)
        {
            GameObject projectileObject = prefab != null
                ? RuntimePool.Acquire(prefab)
                : AcquireFallbackVisual(
                    SkillProjectileFallbackPoolName,
                    PrimitiveType.Sphere,
                    fallbackScale,
                    new Color(0.75f, 1f, 0.65f, 0.9f));

            if (projectileObject == null)
            {
                return null;
            }

            EnsureProjectilePhysics(projectileObject);
            projectileObject.transform.position = position;
            projectileObject.transform.rotation = Quaternion.identity;
            return projectileObject;
        }

        private void EnsureProjectilePhysics(GameObject projectileObject)
        {
            Collider collider = projectileObject.GetComponent<Collider>();
            if (collider == null)
            {
                collider = projectileObject.AddComponent<SphereCollider>();
            }

            collider.enabled = true;
            collider.isTrigger = true;

            Rigidbody body = projectileObject.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = projectileObject.AddComponent<Rigidbody>();
            }

            body.useGravity = false;
            body.isKinematic = true;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        private Vector3 GetProjectileSpawnPosition(Vector3 direction)
        {
            Vector3 basePos = skillSpawnPoint != null ? skillSpawnPoint.position : transform.position;
            basePos += direction.normalized * projectileSpawnOffset;
            basePos.y += projectileSpawnHeight + skillVerticalOffset;
            return basePos;
        }

        private Vector3 GetSkillCenter(float forwardOffset)
        {
            Vector3 center = transform.position + GetFacingDirection() * forwardOffset;
            center.y = transform.position.y + skillVerticalOffset;
            return center;
        }

        private Vector3 GetFacingDirection()
        {
            if (playerController == null)
            {
                return Vector3.forward;
            }

            Vector3 direction = playerController.GetLogicalFacingDirection();
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        private void ResolveSkillSpawnPoint()
        {
            if (IsValidRootSpawnPoint(skillSpawnPoint))
            {
                return;
            }

            Transform found = transform.Find("SkillSpawnPoint");
            if (!IsValidRootSpawnPoint(found))
            {
                found = transform.Find("FirePoint");
            }

            if (IsValidRootSpawnPoint(found))
            {
                skillSpawnPoint = found;
                return;
            }

            GameObject pointObject = new GameObject("SkillSpawnPoint");
            skillSpawnPoint = pointObject.transform;
            skillSpawnPoint.SetParent(transform, false);
            skillSpawnPoint.localPosition = Vector3.zero;
            skillSpawnPoint.localRotation = Quaternion.identity;
        }

        private bool IsValidRootSpawnPoint(Transform point)
        {
            if (point == null || !point.IsChildOf(transform))
            {
                return false;
            }

            Transform cursor = point;
            while (cursor != null && cursor != transform)
            {
                if (cursor.GetComponent<SpriteRenderer>() != null)
                {
                    return false;
                }

                cursor = cursor.parent;
            }

            return true;
        }

        private void SpawnAttachedSkillEffect(
            GameObject prefab,
            EnemyController target,
            float lifeTime,
            float fallbackScale,
            Color fallbackColor,
            float headOffset)
        {
            if (target == null)
            {
                SpawnSkillEffect(prefab, GetSkillCenter(0f), lifeTime, fallbackScale, fallbackColor);
                return;
            }

            GameObject effect = prefab != null
                ? RuntimePool.Acquire(prefab)
                : AcquireFallbackVisual(
                    SkillAttachedEffectFallbackPoolName,
                    PrimitiveType.Sphere,
                    fallbackScale,
                    fallbackColor);
            if (effect == null)
            {
                return;
            }

            effect.transform.position = target.transform.position;
            effect.transform.rotation = Quaternion.identity;

            TargetAttachedEffect attachedEffect = effect.GetComponent<TargetAttachedEffect>();
            if (attachedEffect == null)
            {
                attachedEffect = effect.AddComponent<TargetAttachedEffect>();
            }

            attachedEffect.Bind(
                target.transform,
                headOffset,
                mageSkill2.scaleEffectByTargetSize,
                mageSkill2.effectReferenceTargetHeight,
                mageSkill2.effectSizeMultiplier,
                mageSkill2.effectMinScaleMultiplier,
                mageSkill2.effectMaxScaleMultiplier,
                true);

            SchedulePoolReturn(effect, lifeTime);
        }

        private void SpawnSkillEffect(
            GameObject prefab,
            Vector3 position,
            float lifeTime,
            float fallbackScale,
            Color fallbackColor,
            float scaleMultiplier = 1f)
        {
            float safeScaleMultiplier = Mathf.Max(0.01f, scaleMultiplier);
            if (prefab != null)
            {
                GameObject effect = RuntimePool.Acquire(prefab);
                if (effect == null)
                {
                    return;
                }

                effect.transform.position = position;
                effect.transform.rotation = Quaternion.identity;
                effect.transform.localScale *= safeScaleMultiplier;
                SchedulePoolReturn(effect, lifeTime);
                return;
            }

            GameObject fallback = AcquireFallbackVisual(
                SkillEffectFallbackPoolName,
                PrimitiveType.Sphere,
                fallbackScale * safeScaleMultiplier,
                fallbackColor);
            if (fallback == null)
            {
                return;
            }

            fallback.transform.position = position;
            fallback.transform.rotation = Quaternion.identity;
            SchedulePoolReturn(fallback, lifeTime);
        }

        private static GameObject AcquireFallbackVisual(string poolName, PrimitiveType primitiveType, float scale, Color color)
        {
            GameObject visual = RuntimePool.Acquire(poolName, () => CreatePrimitiveVisualObject(primitiveType, color));
            if (visual == null)
            {
                return null;
            }

            ConfigurePrimitiveVisualObject(visual, scale, color);
            return visual;
        }

        private static GameObject CreatePrimitiveVisualObject(PrimitiveType primitiveType, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            Collider collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                UnityEngine.Object.Destroy(collider);
            }

            Renderer renderer = primitive.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = GetFallbackVisualShader();
                if (shader != null)
                {
                    Material material = new Material(shader)
                    {
                        color = color
                    };
                    renderer.material = material;
                }
            }

            return primitive;
        }

        private static void ConfigurePrimitiveVisualObject(GameObject visual, float scale, Color color)
        {
            if (visual == null)
            {
                return;
            }

            visual.transform.localScale = Vector3.one * Mathf.Max(0.05f, scale);

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = renderer.material;
                if (material != null)
                {
                    material.color = color;
                }
            }
        }

        private static Shader GetFallbackVisualShader()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            return shader;
        }

        private static void SchedulePoolReturn(GameObject obj, float lifeTime)
        {
            RuntimePoolAutoReturn autoReturn = RuntimePool.EnsureAutoReturn(obj);
            autoReturn?.Schedule(Mathf.Max(0.1f, lifeTime));
        }

        private void EnsureOverlapBuffer()
        {
            int desiredSize = Mathf.Max(1, overlapBufferSize);
            if (overlapBuffer != null && overlapBuffer.Length == desiredSize)
            {
                return;
            }

            overlapBuffer = new Collider[desiredSize];
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.identity;

            switch (currentClass)
            {
                case PlayerClassType.Mage:
                    DrawSkillRadius(mageSkill1.forwardOffset, mageSkill1.radius, new Color(0.3f, 0.9f, 1f, 0.4f));
                    DrawSkillRadius(0f, mageSkill2.radius, new Color(1f, 0.5f, 0.2f, 0.4f));
                    break;

                case PlayerClassType.Archer:
                    DrawSkillRadius(archerSkill2.targetForwardOffset, archerSkill2.projectileHitRadius, new Color(0.4f, 1f, 0.4f, 0.6f));
                    DrawArcherSkill2LineGizmo(new Color(1f, 0.45f, 0.25f, 0.65f));
                    break;

                case PlayerClassType.Warrior:
                    DrawSkillRadius(0f, warriorSkill1.range, new Color(0.9f, 0.1f, 0.1f, 0.4f));
                    break;
            }
        }

        private void DrawSkillRadius(float forwardOffset, float radius, Color color)
        {
            Vector3 direction = Application.isPlaying ? GetFacingDirection() : Vector3.forward;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.forward;
            }

            Vector3 center = transform.position + direction.normalized * forwardOffset;
            center.y = transform.position.y + skillVerticalOffset;

            Gizmos.color = color;
            Gizmos.DrawWireSphere(center, Mathf.Max(0f, radius));
        }

        private void DrawArcherSkill2LineGizmo(Color color)
        {
            Vector3 direction = Application.isPlaying ? GetArcherSkill2Direction() : Vector3.forward;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.forward;
            }

            Vector3 start = transform.position + direction.normalized * projectileSpawnOffset;
            start.y = transform.position.y + projectileSpawnHeight + skillVerticalOffset;
            Vector3 end = start + direction.normalized * Mathf.Max(0.5f, archerSkill2.range);

            Gizmos.color = color;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, Mathf.Max(0.05f, archerSkill2.projectileHitRadius));
        }
    }
}
