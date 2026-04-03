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

        [Header("Ranged Attack")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private float projectileSpawnOffset = 0.65f;
        [SerializeField] private float projectileSpawnHeight = 1f;
        [SerializeField] private float projectileSpawnExtraHeight = 2f;
        [SerializeField] private LayerMask rangedTargetMask = ~0;

        [Header("Shared")]
        [SerializeField] private float attackCooldown = 0.3f;

        private PlayerController playerController;
        private float lastAttackTime = float.NegativeInfinity;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            if (rangedTargetMask.value == 0)
            {
                rangedTargetMask = ~0;
                Debug.LogWarning("[PlayerAttack] rangedTargetMask was Nothing. Fallback to Everything.");
            }

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

            bool canAttack = Time.time >= lastAttackTime + attackCooldown;

            if (input.DebugLevelUpAction.WasPressedThisFrame())
            {
                LevelUpManager.DebugLevelUp();
                return;
            }

            if (input.MeleeAttackAction.WasPressedThisFrame())
            {
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
                Debug.Log("W pressed: fire bullet");
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
            Vector3 direction = GetAttackDirection();
            Vector3 boxCenter = transform.position + direction * meleeAttackOffset;
            Vector3 tallBoxSize = new Vector3(meleeAttackBoxSize.x, 20f, meleeAttackBoxSize.z);
            Quaternion rotation = Quaternion.LookRotation(direction);

            Collider[] hitColliders = Physics.OverlapBox(
                boxCenter,
                tallBoxSize / 2f,
                rotation,
                ~0,
                QueryTriggerInteraction.Collide);

            Debug.Log($"[PlayerAttack] Melee hit scan count: {hitColliders.Length}");

            foreach (Collider hitCollider in hitColliders)
            {
                EnemyController enemy = hitCollider.GetComponentInParent<EnemyController>();
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                float damage = PlayerStats.Instance != null
                    ? PlayerStats.Instance.GetAttack()
                    : meleeAttackDamage;

                enemy.TakeDamage(damage);
                Debug.Log($"[PlayerAttack] Melee hit {hitCollider.gameObject.name} for {damage}");
            }
        }

        private void RangedAttack()
        {
            ObjectPooler pooler = ResolveObjectPooler();
            if (pooler == null)
            {
                Debug.LogWarning("[PlayerAttack] ObjectPooler.Instance is null");
                return;
            }

            GameObject projectile = pooler.GetPooledObject();
            if (projectile == null)
            {
                LogPoolUnavailableReason(pooler);
                return;
            }

            Vector3 direction = GetAttackDirection();
            Vector3 spawnOrigin = firePoint != null ? firePoint.position : transform.position;
            Vector3 spawnPos = spawnOrigin + direction * projectileSpawnOffset;
            spawnPos.y += projectileSpawnHeight + projectileSpawnExtraHeight;

            projectile.transform.position = spawnPos;
            projectile.SetActive(true);

            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj == null)
            {
                Debug.LogWarning("[PlayerAttack] Pooled projectile has no Projectile component.");
                return;
            }

            float damage = PlayerStats.Instance != null
                ? PlayerStats.Instance.GetAttack()
                : 10f;

            proj.Launch(direction, damage, rangedTargetMask);
            Debug.Log($"[PlayerAttack] Bullet fired toward {direction}");
        }

        private static ObjectPooler ResolveObjectPooler()
        {
            ObjectPooler pooler = ObjectPooler.Instance;
            if (pooler != null)
            {
                return pooler;
            }

            pooler = FindFirstObjectByType<ObjectPooler>();
            if (pooler != null)
            {
                ObjectPooler.Instance = pooler;
            }

            return pooler;
        }

        private static void LogPoolUnavailableReason(ObjectPooler pooler)
        {
            if (pooler == null)
            {
                Debug.LogWarning("[PlayerAttack] ObjectPooler.Instance is null");
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
