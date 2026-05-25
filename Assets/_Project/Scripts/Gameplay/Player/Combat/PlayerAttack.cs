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
        [SerializeField] private LayerMask rangedTargetMask = ~0;

        [Header("Beam")]
        [SerializeField, Min(1)] private int beamOverlapBufferSize = 48;
        [SerializeField] private float beamVerticalHalfHeight = 2.5f;
        [SerializeField] private float beamHeightOffset = 0.8f;

        [Header("Shared")]
        [SerializeField] private float attackCooldown = 0.3f;
        [SerializeField] private bool enableDebugLogs;

        private PlayerController playerController;
        private PlayerItemCombatEffects itemEffects;
        private float lastAttackTime = float.NegativeInfinity;
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

            PlayerStats stats = PlayerStats.Instance;
            float effectiveAttackCooldown = PlayerCombatCalculator.GetBasicAttackCooldown(attackCooldown, stats);
            bool canAttack = Time.time >= lastAttackTime + effectiveAttackCooldown;

            if (input.DebugLevelUpAction.WasPressedThisFrame())
            {
                LevelUpManager.DebugLevelUp();
                return;
            }

            if (input.MeleeAttackAction.WasPressedThisFrame())
            {
                if (!canAttack)
                {
                    return;
                }

                lastAttackTime = Time.time;
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
            float effectiveAttackOffset = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackOffset, stats);
            Vector3 boxCenter = transform.position + direction * effectiveAttackOffset;
            float effectiveWidth = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackBoxSize.x, stats);
            float effectiveDepth = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackBoxSize.z, stats);
            Vector3 tallBoxSize = new Vector3(effectiveWidth, 20f, effectiveDepth);
            Quaternion rotation = Quaternion.LookRotation(direction);

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

                float damage = PlayerCombatCalculator.GetBasicAttackDamage(stats, meleeAttackDamage);
                enemy.TakeDamage(damage);
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
            float damage = PlayerCombatCalculator.GetBasicAttackDamage(stats, 10f);
            float effectiveProjectileRange = PlayerCombatCalculator.GetBasicAttackRange(projectileRange, stats);

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

            if (projectileCount <= 1)
            {
                SpawnProjectile(direction, damage, range, Projectile.SpawnKind.Normal);
            }
            else
            {
                float center = (projectileCount - 1) * 0.5f;
                for (int i = 0; i < projectileCount; i++)
                {
                    float offset = (i - center) * spread;
                    Vector3 shotDirection = Quaternion.Euler(0f, offset, 0f) * direction;
                    SpawnProjectile(shotDirection, damage, range, Projectile.SpawnKind.Normal);
                }
            }

            if (itemEffects != null && itemEffects.HasLaryngealNerve)
            {
                SpawnProjectile(
                    -direction,
                    damage * itemEffects.GetBackShotDamageMultiplier(),
                    range,
                    Projectile.SpawnKind.Normal);
            }

            if (allowExtraVolley && itemEffects != null && itemEffects.RollCellProliferation())
            {
                FireProjectileVolley(
                    direction,
                    damage * itemEffects.CellProliferationDamageMultiplier,
                    range,
                    false);
            }
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

                enemy.TakeDamage(damage);
                itemEffects?.ApplyCommonOnHitEffects(enemy, damage, enemy.transform.position);
            }
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
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(
                transform.position + direction * meleeAttackOffset,
                Quaternion.LookRotation(direction),
                Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(meleeAttackBoxSize.x, 20f, meleeAttackBoxSize.z));
        }
    }
}
