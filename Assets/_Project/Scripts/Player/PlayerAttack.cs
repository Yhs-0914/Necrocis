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
        [SerializeField] private float meleeAttackDamage = 20f;                        // 기본 근거리 데미지 (PlayerStats가 없을 때 사용)
        [SerializeField] private Vector3 meleeAttackBoxSize = new Vector3(3f, 3f, 3f); // 공격 판정 박스 크기
        [SerializeField] private float meleeAttackOffset = 2f;                         // 플레이어로부터 판정 박스까지 거리

        [Header("Ranged Attack")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private float projectileSpawnOffset = 0.65f;
        [SerializeField] private float projectileSpawnHeight = 1f;
        [SerializeField] private float projectileSpawnExtraHeight = 2f;
        [SerializeField] private float projectileRange = 8f;
        [SerializeField] private LayerMask rangedTargetMask = ~0;

        [Header("Shared")]
        [SerializeField] private float attackCooldown = 0.3f;

        private PlayerController playerController;
        private float lastAttackTime = float.NegativeInfinity; // 마지막 공격 시간 (초기값을 -∞로 설정하여 첫 공격 즉시 가능)

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            if (playerController == null)
                playerController = GetComponentInParent<PlayerController>();
            if (playerController == null)
                Debug.LogError("[PlayerAttack] PlayerController를 찾지 못했습니다. 공격 애니메이션이 작동하지 않습니다.");
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

        // 입력 처리: P=디버그 레벨업, Q=근거리(쿨타임없음), E=원거리(쿨타임있음)
        private void HandleAttackInput()
        {
            InputManager input = InputManager.Instance;
            if (input == null)
            {
                return;
            }

            PlayerStats stats = PlayerStats.Instance;
            float effectiveAttackCooldown = PlayerCombatCalculator.GetBasicAttackCooldown(attackCooldown, stats);
            bool canAttack = Time.time >= lastAttackTime + effectiveAttackCooldown; // 원거리 공격 쿨다운 체크

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
                Debug.Log("W pressed: fire bullet");
                RangedAttack();
            }
        }

        // PlayerController의 현재 방향을 3D 벡터로 변환
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

        // 근거리 공격: 방향 앞에 OverlapBox를 생성하여 범위 내 적에게 데미지
        private void MeleeAttack()
        {
            PlayerStats stats = PlayerStats.Instance;
            playerController?.PlayAttackAnimation(true);
            Vector3 direction = GetAttackDirection();
            float effectiveAttackOffset = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackOffset, stats);
            Vector3 boxCenter = transform.position + direction * effectiveAttackOffset; // 판정 중심점
            // Y를 높여서 높이 차이와 관계없이 적을 감지
            float effectiveWidth = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackBoxSize.x, stats);
            float effectiveDepth = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackBoxSize.z, stats);
            Vector3 tallBoxSize = new Vector3(effectiveWidth, 20f, effectiveDepth);
            Quaternion rotation = Quaternion.LookRotation(direction);

            Collider[] hitColliders = Physics.OverlapBox(
                boxCenter,
                tallBoxSize / 2f,
                rotation,
                ~0,
                QueryTriggerInteraction.Collide);

            Debug.Log($"[PlayerAttack] Melee hit scan count: {hitColliders.Length}");

            // 히트된 콜라이더에서 EnemyController를 찾아 데미지 적용
            foreach (Collider hitCollider in hitColliders)
            {
                EnemyController enemy = hitCollider.GetComponentInParent<EnemyController>();
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                float damage = PlayerCombatCalculator.GetBasicAttackDamage(stats, meleeAttackDamage);
                enemy.TakeDamage(damage);
                Debug.Log($"[PlayerAttack] Melee hit {hitCollider.gameObject.name} for {damage}");
            }
        }

        // 원거리 공격: 오브젝트 풀에서 투사체를 가져와 발사
        private void RangedAttack()
        {
            playerController?.PlayAttackAnimation(false);
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

            PlayerStats stats = PlayerStats.Instance;
            float damage = PlayerCombatCalculator.GetBasicAttackDamage(stats, 10f);
            float effectiveProjectileRange = PlayerCombatCalculator.GetBasicAttackRange(projectileRange, stats);

            proj.Launch(direction, damage, rangedTargetMask, effectiveProjectileRange);
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

        // Scene 뷰에서 근거리 공격 판정 범위를 빨간 와이어프레임으로 표시
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
