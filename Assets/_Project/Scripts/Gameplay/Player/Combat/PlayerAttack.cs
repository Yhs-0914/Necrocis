using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// Basic player attack controller.
    /// Q = melee, W = ranged.
    /// </summary>
    public class PlayerAttack : MonoBehaviour
    {
        [Header("Melee Attack (Q)")]
        [SerializeField] private float meleeAttackDamage = 20f;
        [SerializeField] private Vector3 meleeAttackBoxSize = new Vector3(3f, 3f, 3f);
        [SerializeField] private float meleeAttackOffset = 2f;
        [SerializeField] private LayerMask meleeTargetMask = ~0;
        [SerializeField, Min(1)] private int meleeOverlapBufferSize = 24;

        [Header("Ranged Attack")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private float projectileSpawnOffset = 0.65f;
        [SerializeField] private float projectileSpawnHeight = 1f;
        [SerializeField] private float projectileSpawnExtraHeight = 2f;
        [SerializeField] private float projectileRange = 8f;
        [SerializeField] private float rangedAttackDamage = 10f;
        [SerializeField] private LayerMask rangedTargetMask = ~0;

        [Header("Beam")]
        [SerializeField, Min(1)] private int beamOverlapBufferSize = 48;
        [SerializeField] private float beamVerticalHalfHeight = 2.5f;
        [SerializeField] private float beamHeightOffset = 0.8f;
        [SerializeField] private float beamVisualDuration = 0.1f;

        [Header("Shared")]
        [SerializeField] private float meleeCooldown  = 0.2f;
        [SerializeField] private float attackCooldown = 0.3f;
        [SerializeField, Min(0f)] private float cellProliferationDelay = 0.5f;
        [SerializeField] private bool enableDebugLogs;

        private PlayerController playerController;
        private PlayerItemCombatEffects itemEffects;
        private float lastAttackTime = float.NegativeInfinity;
        private float lastMeleeTime = float.NegativeInfinity;
        private readonly HashSet<EnemyController> meleeHitEnemies = new HashSet<EnemyController>();
        private readonly HashSet<EnemyController> beamHitEnemies = new HashSet<EnemyController>();
        private Collider[] meleeOverlapResults;
        private Collider[] beamOverlapResults;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = GetComponentInParent<PlayerController>();
            }

            if (playerController == null)
            {
                Debug.LogError("[PlayerAttack] PlayerController not found.");
            }

            itemEffects = GetComponent<PlayerItemCombatEffects>();
            if (itemEffects == null)
            {
                itemEffects = gameObject.AddComponent<PlayerItemCombatEffects>();
            }

            if (meleeTargetMask.value == 0)
            {
                meleeTargetMask = ~0;
                Debug.LogWarning("[PlayerAttack] meleeTargetMask was Nothing. Fallback to Everything.");
            }

            if (rangedTargetMask.value == 0)
            {
                rangedTargetMask = ~0;
                Debug.LogWarning("[PlayerAttack] rangedTargetMask was Nothing. Fallback to Everything.");
            }

            EnsureMeleeOverlapBuffer();
            EnsureBeamOverlapBuffer();
            ResolveRootFirePoint();
        }

        private void Update()
        {
            HandleAttackInput();
        }

        private void HandleAttackInput()
        {
            InputManager input = InputManager.Instance;
            if (input == null)
            {
                return;
            }

            if (playerController == null || playerController.IsDead)
            {
                return;
            }

            PlayerStats stats = PlayerStats.Instance;
            if (stats == null || stats.IsDead)
            {
                return;
            }

            float effectiveAttackCooldown = PlayerCombatCalculator.GetBasicAttackCooldown(attackCooldown, stats);
            if (itemEffects != null)
            {
                effectiveAttackCooldown *= itemEffects.GetAttackCooldownMultiplier();
            }
            bool canAttack = Time.time >= lastAttackTime + effectiveAttackCooldown;

            if (input.DebugLevelUpAction.WasPressedThisFrame())
            {
                LevelUpManager.DebugLevelUp();
                return;
            }

            if (input.MeleeAttackAction.WasPressedThisFrame())
            {
                float effectiveMeleeCooldown = PlayerCombatCalculator.GetBasicAttackCooldown(meleeCooldown, stats);
                if (itemEffects != null)
                {
                    effectiveMeleeCooldown *= itemEffects.GetAttackCooldownMultiplier();
                }

                if (Time.time < lastMeleeTime + effectiveMeleeCooldown) return;

                lastMeleeTime = Time.time;
                MeleeAttack();
                return;
            }

            if (input.RangedAttackAction.WasPressedThisFrame())
            {
                if (!canAttack)
                {
                    return;
                }

                lastAttackTime = Time.time;
                RangedAttack();
            }
        }

        private Vector3 GetAttackDirection()
        {
            PlayerController controller = playerController != null ? playerController : PlayerController.Instance;
            if (controller == null)
            {
                return Vector3.forward;
            }

            Vector3 direction = controller.GetLogicalFacingDirection();
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        private void MeleeAttack()
        {
            PlayerStats stats = PlayerStats.Instance;
            playerController?.PlayAttackAnimation(true);
            AudioManager.Instance?.PlayPlayerSfx(PlayerSoundId.MeleeAttack);
            Vector3 direction = GetAttackDirection();
            float rangeMultiplier = itemEffects != null ? itemEffects.GetMeleeRangeMultiplier() : 1f;
            float effectiveAttackOffset = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackOffset * rangeMultiplier, stats);
            Vector3 boxCenter = transform.position + direction * effectiveAttackOffset;
            float effectiveWidth = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackBoxSize.x * rangeMultiplier, stats);
            float effectiveDepth = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackBoxSize.z * rangeMultiplier, stats);
            Vector3 tallBoxSize = new Vector3(effectiveWidth, 20f, effectiveDepth);
            Quaternion rotation = Quaternion.LookRotation(direction);
            float damageMultiplier = itemEffects != null ? itemEffects.GetOutgoingBasicDamageMultiplier() : 1f;
            float flatDamageBonus = itemEffects != null ? itemEffects.GetOutgoingBasicDamageFlatBonus() : 0f;
            float unstableMultiplier = itemEffects != null ? itemEffects.RollUnstableCoreDamageMultiplier() : 1f;
            float baseDamage = PlayerCombatCalculator.GetBasicAttackDamage(stats, meleeAttackDamage) + flatDamageBonus;
            float finalDamage = baseDamage * damageMultiplier * unstableMultiplier;
            itemEffects?.NotifyBasicAttackPerformed(finalDamage, meleeTargetMask, effectiveAttackOffset + effectiveDepth, direction);

            EnsureMeleeOverlapBuffer();
            meleeHitEnemies.Clear();
            int hitCount = Physics.OverlapBoxNonAlloc(
                boxCenter,
                tallBoxSize / 2f,
                meleeOverlapResults,
                rotation,
                meleeTargetMask,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = meleeOverlapResults[i];
                if (hitCollider == null)
                {
                    continue;
                }

                EnemyController enemy = hitCollider.GetComponentInParent<EnemyController>();
                if (enemy == null || enemy.IsDead || !meleeHitEnemies.Add(enemy))
                {
                    continue;
                }

                float appliedDamage = itemEffects != null
                    ? itemEffects.ApplyPerTargetDamageModifiers(enemy, finalDamage)
                    : finalDamage;
                enemy.TakeDamage(appliedDamage);
                itemEffects?.TryApplyPostDamageExecutionInstinct(enemy, appliedDamage);
                itemEffects?.ApplyCommonOnHitEffects(enemy, appliedDamage, enemy.transform.position);
            }
        }

        private void EnsureMeleeOverlapBuffer()
        {
            int size = Mathf.Max(1, meleeOverlapBufferSize);
            if (meleeOverlapResults == null || meleeOverlapResults.Length != size)
            {
                meleeOverlapResults = new Collider[size];
            }
        }

        private void EnsureBeamOverlapBuffer()
        {
            int size = Mathf.Max(1, beamOverlapBufferSize);
            if (itemEffects != null)
            {
                size = Mathf.Max(size, itemEffects.BeamHitBufferSize);
            }

            if (beamOverlapResults == null || beamOverlapResults.Length != size)
            {
                beamOverlapResults = new Collider[size];
            }
        }

        private void RangedAttack()
        {
            playerController?.PlayAttackAnimation(false);
            AudioManager.Instance?.PlayPlayerSfx(PlayerSoundId.RangedAttack);

            Vector3 direction = GetAttackDirection();
            PlayerStats stats = PlayerStats.Instance;
            float flatDamageBonus = itemEffects != null ? itemEffects.GetOutgoingBasicDamageFlatBonus() : 0f;
            float damage = PlayerCombatCalculator.GetBasicAttackDamage(stats, rangedAttackDamage) + flatDamageBonus;
            float unstableMultiplier = 1f;
            if (itemEffects != null)
            {
                damage *= itemEffects.GetOutgoingBasicDamageMultiplier();
                unstableMultiplier = itemEffects.RollUnstableCoreDamageMultiplier();
                damage *= unstableMultiplier;
            }
            float effectiveProjectileRange = PlayerCombatCalculator.GetBasicAttackRange(projectileRange, stats);
            itemEffects?.NotifyBasicAttackPerformed(damage, rangedTargetMask, effectiveProjectileRange, direction);

            if (itemEffects != null && itemEffects.HasBeamOrgan)
            {
                FireBeam(direction, damage * itemEffects.BeamDamageMultiplier, effectiveProjectileRange);
                return;
            }

            FireProjectileVolley(direction, damage, effectiveProjectileRange, true);
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayerAttack] Fired ranged attack toward {direction}");
            }
        }

        private void FireProjectileVolley(Vector3 direction, float damage, float range, bool allowExtraVolley)
        {
            int projectileCount = itemEffects != null ? itemEffects.GetForwardProjectileCount() : 1;
            float spread = itemEffects != null ? itemEffects.GetSpreadAngleForCount(projectileCount) : 0f;
            float accuracyPenalty = itemEffects != null ? itemEffects.GetAccuracyPenaltyAngle() : 0f;
            Vector3 firingDirection = ApplyAccuracyPenalty(direction, accuracyPenalty);

            if (projectileCount <= 1)
            {
                SpawnProjectile(firingDirection, damage, range, Projectile.SpawnKind.Normal);
            }
            else
            {
                float center = (projectileCount - 1) * 0.5f;
                for (int i = 0; i < projectileCount; i++)
                {
                    float offset = (i - center) * spread;
                    Vector3 shotDirection = Quaternion.Euler(0f, offset, 0f) * firingDirection;
                    SpawnProjectile(shotDirection, damage, range, Projectile.SpawnKind.Normal);
                }
            }

            if (itemEffects != null && itemEffects.HasLaryngealNerve)
            {
                SpawnProjectile(
                    -firingDirection,
                    damage * itemEffects.GetBackShotDamageMultiplier(),
                    range,
                    Projectile.SpawnKind.Normal);
            }

            if (allowExtraVolley && itemEffects != null && itemEffects.RollCellProliferation())
            {
                StartCoroutine(FireCellProliferationVolleyDelayed(direction, damage, range));
            }
        }

        private static Vector3 ApplyAccuracyPenalty(Vector3 direction, float penaltyAngle)
        {
            Vector3 safeDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            if (penaltyAngle <= 0f)
            {
                return safeDirection;
            }

            float randomOffset = Random.Range(-penaltyAngle, penaltyAngle);
            return (Quaternion.Euler(0f, randomOffset, 0f) * safeDirection).normalized;
        }

        private IEnumerator FireCellProliferationVolleyDelayed(Vector3 direction, float damage, float range)
        {
            if (cellProliferationDelay > 0f)
            {
                yield return new WaitForSeconds(cellProliferationDelay);
            }

            if (itemEffects == null || !isActiveAndEnabled)
            {
                yield break;
            }

            FireProjectileVolley(
                direction,
                damage * itemEffects.CellProliferationDamageMultiplier,
                range,
                false);
        }

        private void SpawnProjectile(Vector3 direction, float damage, float range, Projectile.SpawnKind spawnKind)
        {
            PlayerProjectilePool pooler = ResolveObjectPooler();
            if (pooler == null)
            {
                Debug.LogWarning("[PlayerAttack] PlayerProjectilePool.Instance is null");
                return;
            }

            GameObject projectile = pooler.GetPooledObject();
            if (projectile == null)
            {
                LogPoolUnavailableReason(pooler);
                return;
            }

            Vector3 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            Vector3 spawnOrigin = firePoint != null ? firePoint.position : transform.position;
            Vector3 spawnPos = spawnOrigin + forward * projectileSpawnOffset;
            spawnPos.y += projectileSpawnHeight + projectileSpawnExtraHeight;

            projectile.transform.position = spawnPos;
            projectile.SetActive(true);

            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj == null)
            {
                Debug.LogWarning("[PlayerAttack] Pooled projectile has no Projectile component.");
                return;
            }

            proj.Launch(forward, damage, rangedTargetMask, range, itemEffects, spawnKind);
            projectile.GetComponent<ProjectileDirectionalSprite>()?.SetDirection(forward);
        }

        private void FireBeam(Vector3 direction, float damage, float range)
        {
            EnsureBeamOverlapBuffer();
            beamHitEnemies.Clear();

            Vector3 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            Vector3 origin = firePoint != null ? firePoint.position : transform.position;
            origin += forward * projectileSpawnOffset;
            origin.y += projectileSpawnHeight + projectileSpawnExtraHeight + beamHeightOffset;

            Vector3 end = origin + forward * Mathf.Max(0.5f, range);
            float radius = itemEffects != null ? itemEffects.BeamRadius : 0.8f;
            float halfHeight = Mathf.Max(0.05f, beamVerticalHalfHeight);
            SpawnBeamVisual(origin, end, radius);

            int hitCount = Physics.OverlapCapsuleNonAlloc(
                origin + Vector3.up * halfHeight,
                end - Vector3.up * halfHeight,
                radius,
                beamOverlapResults,
                rangedTargetMask,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < hitCount; i++)
            {
                Collider collider = beamOverlapResults[i];
                if (collider == null)
                {
                    continue;
                }

                EnemyController enemy = collider.GetComponent<EnemyController>()
                    ?? collider.GetComponentInParent<EnemyController>();

                if (enemy == null || enemy.IsDead || !beamHitEnemies.Add(enemy))
                {
                    continue;
                }

                float appliedDamage = itemEffects != null
                    ? itemEffects.ApplyPerTargetDamageModifiers(enemy, damage)
                    : damage;
                enemy.TakeDamage(appliedDamage);
                itemEffects?.TryApplyPostDamageExecutionInstinct(enemy, appliedDamage);
                itemEffects?.ApplyCommonOnHitEffects(enemy, appliedDamage, enemy.transform.position);
            }
        }

        private void SpawnBeamVisual(Vector3 start, Vector3 end, float radius)
        {
            GameObject fx = new GameObject("BeamOrganFx");
            LineRenderer line = fx.AddComponent<LineRenderer>();

            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.useWorldSpace = true;
            line.numCapVertices = 6;
            line.startWidth = Mathf.Max(0.05f, radius * 1.45f);
            line.endWidth = Mathf.Max(0.03f, radius * 1.1f);
            line.material = TextureSpriteCache.GetSpriteMaterial();

            Color baseColor = new Color(1f, 0.24f, 0.15f, 0.85f);
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(baseColor, 0f),
                    new GradientColorKey(new Color(1f, 0.7f, 0.2f, 1f), 0.55f),
                    new GradientColorKey(baseColor, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.95f, 0f),
                    new GradientAlphaKey(0.65f, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                });
            line.colorGradient = gradient;
            line.sortingOrder = 5005;

            Destroy(fx, Mathf.Max(0.05f, beamVisualDuration));
        }

        private static PlayerProjectilePool ResolveObjectPooler()
        {
            PlayerProjectilePool pooler = PlayerProjectilePool.Instance;
            if (pooler != null)
            {
                return pooler;
            }

            pooler = FindFirstObjectByType<PlayerProjectilePool>();
            if (pooler != null)
            {
                PlayerProjectilePool.Instance = pooler;
            }

            return pooler;
        }

        private static void LogPoolUnavailableReason(PlayerProjectilePool pooler)
        {
            if (pooler == null)
            {
                Debug.LogWarning("[PlayerAttack] PlayerProjectilePool.Instance is null");
                return;
            }

            string status = pooler.GetDebugStatus();
            Debug.LogWarning($"[PlayerAttack] {status}");
        }

        private void ResolveRootFirePoint()
        {
            if (IsValidRootSpawnPoint(firePoint))
            {
                return;
            }

            if (IsValidRootSpawnPoint(projectileSpawnPoint))
            {
                firePoint = projectileSpawnPoint;
                return;
            }

            Transform found = transform.Find("FirePoint");
            if (IsValidRootSpawnPoint(found))
            {
                firePoint = found;
                return;
            }

            GameObject pointObject = new GameObject("FirePoint");
            firePoint = pointObject.transform;
            firePoint.SetParent(transform, false);
            firePoint.localPosition = Vector3.zero;
            firePoint.localRotation = Quaternion.identity;
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

        private void OnDrawGizmosSelected()
        {
            Vector3 direction = GetAttackDirection();
            PlayerStats stats = PlayerStats.Instance;
            float rangeMultiplier = itemEffects != null ? itemEffects.GetMeleeRangeMultiplier() : 1f;
            float gizmoOffset = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackOffset * rangeMultiplier, stats);
            float gizmoWidth = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackBoxSize.x * rangeMultiplier, stats);
            float gizmoDepth = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackBoxSize.z * rangeMultiplier, stats);

            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(
                transform.position + direction * gizmoOffset,
                Quaternion.LookRotation(direction),
                Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(gizmoWidth, 20f, gizmoDepth));
        }
    }
}
