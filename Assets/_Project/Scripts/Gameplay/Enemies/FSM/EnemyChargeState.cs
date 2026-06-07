using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 돌진 상태 (항체 엘리트): 플레이어 방향으로 가속 → 접촉 공격.
    /// 돌진 시작 시 방향 고정, 가속 후 일직선 이동.
    /// </summary>
    public class EnemyChargeState : IEnemyState
    {
        public static readonly EnemyChargeState Instance = new EnemyChargeState();

        public void Enter(EnemyController enemy)
        {
            enemy.SetMoveAnimation();
            enemy.StartCharge();
            AudioManager.Instance?.PlaySFX("EnemyCharge");
        }

        public void Update(EnemyController enemy, float deltaTime)
        {
            if (enemy.IsDead)
            {
                enemy.ChangeState(EnemyDeadState.Instance);
                return;
            }

            // 돌진 이동 처리
            bool charging = enemy.UpdateCharge(deltaTime);

            // 돌진 중 공격 범위 진입 → 즉시 공격
            if (enemy.IsPlayerInAttackRange())
            {
                enemy.ChangeState(EnemyAttackState.Instance);
                return;
            }

            // 돌진 완료 (거리 초과 또는 시간 초과) → Chase로 복귀
            if (!charging)
            {
                enemy.ChangeState(EnemyChaseState.Instance);
                return;
            }
        }

        public void Exit(EnemyController enemy)
        {
            enemy.EndCharge();
        }
    }
}
