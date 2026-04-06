namespace Necrocis
{
    /// <summary>
    /// 추격 상태: 플레이어를 향해 이동, 공격 범위 진입 시 Attack, leash 이탈 시 Return
    /// 고정형(원거리): 이동하지 않고 범위 체크만
    /// </summary>
    public class EnemyChaseState : IEnemyState
    {
        public static readonly EnemyChaseState Instance = new EnemyChaseState();

        public void Enter(EnemyController enemy)
        {
            if (!enemy.IsStationary)
            {
                enemy.SetMoveAnimation();
            }
        }

        public void Update(EnemyController enemy, float deltaTime)
        {
            // 체력 0 → Dead
            if (enemy.IsDead)
            {
                enemy.ChangeState(EnemyDeadState.Instance);
                return;
            }

            // 돌진형 (항체): 쿨타임 완료 + 플레이어 감지 → 돌진
            if (enemy.CanCharge && enemy.IsPlayerInChaseRange())
            {
                enemy.ChangeState(EnemyChargeState.Instance);
                return;
            }

            // 공격 범위 진입 → Attack
            if (enemy.IsPlayerInAttackRange())
            {
                enemy.ChangeState(EnemyAttackState.Instance);
                return;
            }

            // 고정형: 범위 밖이면 Idle로 복귀 (이동 안 함)
            if (enemy.IsStationary)
            {
                enemy.ChangeState(EnemyIdleState.Instance);
                return;
            }

            // leash 이탈 → Return
            if (enemy.IsOutOfLeash())
            {
                enemy.ChangeState(EnemyReturnState.Instance);
                return;
            }

            // 감지범위 이탈 → Wander
            if (!enemy.IsPlayerInChaseRange())
            {
                enemy.ChangeState(EnemyWanderState.Instance);
                return;
            }

            // 플레이어 추격
            enemy.SetChaseDestination();
            enemy.MoveTowardDestination(deltaTime);
        }

        public void Exit(EnemyController enemy) { }
    }
}
