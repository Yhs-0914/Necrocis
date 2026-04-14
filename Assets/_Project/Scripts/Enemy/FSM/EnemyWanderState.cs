namespace Necrocis
{
    /// <summary>
    /// 배회 상태: 앵커 주변 랜덤 위치로 이동, 도착 시 Idle로 복귀
    /// </summary>
    public class EnemyWanderState : IEnemyState
    {
        public static readonly EnemyWanderState Instance = new EnemyWanderState();

        public void Enter(EnemyController enemy)
        {
            enemy.PickWanderDestination();
            enemy.SetMoveAnimation();
        }

        public void Update(EnemyController enemy, float deltaTime)
        {
            // 감지범위 진입 → Chase
            if (enemy.IsPlayerInChaseRange())
            {
                enemy.ChangeState(EnemyChaseState.Instance);
                return;
            }

            // leash 이탈 → Return
            if (enemy.IsOutOfLeash())
            {
                enemy.ChangeState(EnemyReturnState.Instance);
                return;
            }

            // 목적지 도착 → Idle
            if (!enemy.MoveTowardDestination(deltaTime))
            {
                enemy.ChangeState(EnemyIdleState.Instance);
            }
        }

        public void Exit(EnemyController enemy) { }
    }
}
