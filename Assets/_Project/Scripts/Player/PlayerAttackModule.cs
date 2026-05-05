using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Necrocis
{
    /// <summary>
    /// 플레이어 기본 공격 모듈.
    /// 현재는 전방 부채꼴 판정 근접 공격을 제공한다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAttackModule : MonoBehaviour
    {
        [Header("입력")]
        [SerializeField] private bool useDirectionalArrowAttack = true; // 방향키로 공격 방향 결정

        [Header("공격")]
        [SerializeField] private float attackCooldown = 0.35f;          // 공격 쿨타임 (초)
        [SerializeField] private float attackOriginOffset = 0.35f;      // 판정 원점 오프셋 (플레이어 앞)
        [SerializeField] private float attackRadius = 1.8f;             // 공격 판정 반경
        [SerializeField, Range(20f, 180f)] private float attackAngle = 100f; // 부채꼴 각도
        [SerializeField] private float attackHeightOffset = 0.75f;      // Y축 판정 오프셋
        [SerializeField] private float enemyKnockbackDistance = 0.4f;   // 적 넉백 거리
        [SerializeField] private LayerMask targetMask = ~0;             // 공격 대상 레이어
        [SerializeField] private int overlapBufferSize = 16;            // OverlapSphere 버퍼 크기

        private readonly HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>(); // 중복 히트 방지

        private PlayerController playerController; // 방향/스탯 참조
        private Collider[] overlapResults;         // OverlapSphere 결과 버퍼 (GC 방지용 재사용)
        private float nextAttackTime;              // 다음 공격 가능 시간

        public event Action<PlayerAttackModule> AttackPerformed;                     // 공격 수행 이벤트
        public event Action<PlayerAttackModule, EnemyController, float> EnemyHit;   // 적 적중 이벤트

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            EnsureOverlapBuffer();
        }

        private void Update()
        {
            if (!ShouldAcceptInput())
            {
                return;
            }

            if (!TryGetAttackDirectionInput(out PlayerController.Direction attackDirection))
            {
                return;
            }

            TryAttack(attackDirection);
        }

        // 부채꼴 범위 공격 실행: 쿨타임 확인 → OverlapSphere → 각도 필터 → 데미지+넉백
        public bool TryAttack(PlayerController.Direction attackDirection)
        {
            if (playerController == null || Time.time < nextAttackTime)
            {
                return false;
            }

            PlayerStats stats = playerController.Stats;
            float attackDamage = PlayerCombatCalculator.GetBasicAttackDamage(stats);
            if (attackDamage <= 0f)
            {
                return false;
            }

            nextAttackTime = Time.time + PlayerCombatCalculator.GetBasicAttackCooldown(attackCooldown, stats);
            EnsureOverlapBuffer();
            hitEnemies.Clear();
            playerController.FaceDirection(attackDirection);

            Vector3 attackDirectionVector = DirectionToVector(attackDirection);
            Vector3 attackOrigin = transform.position;
            attackOrigin.y += attackHeightOffset;
            float effectiveAttackOriginOffset = PlayerCombatCalculator.GetBasicAttackRange(attackOriginOffset, stats);
            float effectiveAttackRadius = PlayerCombatCalculator.GetBasicAttackRange(attackRadius, stats);
            Vector3 overlapCenter = attackOrigin + attackDirectionVector * effectiveAttackOriginOffset;
            float halfAngle = attackAngle * 0.5f;

            int hitCount = Physics.OverlapSphereNonAlloc(
                overlapCenter,
                effectiveAttackRadius,
                overlapResults,
                targetMask,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < hitCount; i++)
            {
                Collider collider = overlapResults[i];
                if (collider == null)
                {
                    continue;
                }

                EnemyController enemy = collider.GetComponent<EnemyController>();
                if (enemy == null)
                {
                    enemy = collider.GetComponentInParent<EnemyController>();
                }

                if (enemy == null || enemy.IsDead || !hitEnemies.Add(enemy))
                {
                    continue;
                }

                Vector3 enemyPosition = enemy.transform.position;
                enemyPosition.y = attackOrigin.y;
                Vector3 toEnemy = enemyPosition - attackOrigin;
                if (toEnemy.sqrMagnitude <= 0.0001f)
                {
                    continue;
                }

                float angleToEnemy = Vector3.Angle(attackDirectionVector, toEnemy.normalized);
                if (angleToEnemy > halfAngle)
                {
                    continue;
                }

                enemy.TakeDamage(attackDamage);
                enemy.ApplyKnockback(attackDirectionVector, enemyKnockbackDistance);
                Debug.Log($"[PlayerAttack] {attackDirection} 공격 적중 | 대상: {enemy.gameObject.name} | 데미지: {attackDamage}");
                EnemyHit?.Invoke(this, enemy, attackDamage);
            }

            if (hitEnemies.Count == 0)
            {
                Debug.Log($"[PlayerAttack] {attackDirection} 공격 | 데미지: {attackDamage} | 적중 대상 없음");
            }

            AttackPerformed?.Invoke(this);
            return true;
        }

        // 입력 수신 가능 여부 (포커스/로딩/사망 체크)
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

            return true;
        }

        // 방향키 입력에서 공격 방향 결정 (직접 Keyboard 사용 — InputManager 미연동)
        private bool TryGetAttackDirectionInput(out PlayerController.Direction attackDirection)
        {
            Keyboard keyboard = Keyboard.current;
            attackDirection = PlayerController.Direction.Down;
            if (!useDirectionalArrowAttack || keyboard == null)
            {
                return false;
            }

            if (keyboard.upArrowKey.isPressed)
            {
                attackDirection = PlayerController.Direction.Up;
                return true;
            }

            if (keyboard.downArrowKey.isPressed)
            {
                attackDirection = PlayerController.Direction.Down;
                return true;
            }

            if (keyboard.leftArrowKey.isPressed)
            {
                attackDirection = PlayerController.Direction.Left;
                return true;
            }

            if (keyboard.rightArrowKey.isPressed)
            {
                attackDirection = PlayerController.Direction.Right;
                return true;
            }

            return false;
        }

        // Direction 열거형을 3D 방향 벡터로 변환
        private static Vector3 DirectionToVector(PlayerController.Direction direction)
        {
            return direction switch
            {
                PlayerController.Direction.Up => Vector3.forward,
                PlayerController.Direction.Left => Vector3.left,
                PlayerController.Direction.Right => Vector3.right,
                _ => Vector3.back
            };
        }

        // OverlapSphere 결과 버퍼 크기 보장 (크기 변경 시에만 재할당)
        private void EnsureOverlapBuffer()
        {
            int desiredSize = Mathf.Max(1, overlapBufferSize);
            if (overlapResults != null && overlapResults.Length == desiredSize)
            {
                return;
            }

            overlapResults = new Collider[desiredSize];
        }

        // Scene 뷰에서 공격 범위를 와이어 스피어 + 부채꼴 선으로 시각화
        private void OnDrawGizmosSelected()
        {
            PlayerController controller = playerController != null ? playerController : GetComponent<PlayerController>();
            Vector3 attackDirection = controller != null
                ? DirectionToVector(controller.GetCurrentDirection())
                : Vector3.forward;

            Vector3 attackOrigin = transform.position;
            attackOrigin.y += attackHeightOffset;
            Vector3 overlapCenter = attackOrigin + attackDirection * attackOriginOffset;
            Vector3 leftBoundary = Quaternion.Euler(0f, -attackAngle * 0.5f, 0f) * attackDirection;
            Vector3 rightBoundary = Quaternion.Euler(0f, attackAngle * 0.5f, 0f) * attackDirection;

            Gizmos.color = new Color(1f, 0.35f, 0.2f, 0.45f);
            Gizmos.DrawWireSphere(overlapCenter, attackRadius);
            Gizmos.DrawLine(attackOrigin, attackOrigin + leftBoundary * attackRadius);
            Gizmos.DrawLine(attackOrigin, attackOrigin + rightBoundary * attackRadius);
        }
    }
}
