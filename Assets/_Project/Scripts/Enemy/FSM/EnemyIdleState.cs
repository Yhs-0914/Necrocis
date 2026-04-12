namespace Necrocis
{
    /// <summary>
    /// 대기 상태: 일정 시간 후 Wander로 전환, 플레이어 감지 시 Chase로 전환
    /// 고정형(원거리): 이동하지 않고 공격 범위 진입 시 바로 Attack
    /// </summary>
    public class EnemyIdleState : IEnemyState
    {
        public static readonly EnemyIdleState Instance = new EnemyIdleState();

        public void Enter(EnemyController enemy)
        {
            enemy.SetIdleAnimation();
            enemy.ResetIdleTimer();
        }

        public void Update(EnemyController enemy, float deltaTime)
        {
            if (enemy.IsStationary)
            {
                // 고정형: 공격 범위 안이면 바로 Attack, 아니면 제자리 대기
                if (enemy.IsPlayerInAttackRange())
                {
                    enemy.ChangeState(EnemyAttackState.Instance);
                }
                return;
            }

            // 감지범위 진입 → Chase
            if (enemy.IsPlayerInChaseRange())
            {
                enemy.ChangeState(EnemyChaseState.Instance);
                return;
            }

            // 대기 타임아웃 → Wander
            if (enemy.IsIdleTimerExpired(deltaTime))
            {
                enemy.ChangeState(EnemyWanderState.Instance);
            }
        }

        public void Exit(EnemyController enemy) { }
    }
}
