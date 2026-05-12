namespace Necrocis
{
    /// <summary>
    /// 복귀 상태: 앵커 위치로 돌아감, 도착 시 Idle로 전환
    /// </summary>
    public class EnemyReturnState : IEnemyState
    {
        public static readonly EnemyReturnState Instance = new EnemyReturnState();

        public void Enter(EnemyController enemy)
        {
            enemy.SetReturnDestination();
            enemy.SetMoveAnimation();
        }

        public void Update(EnemyController enemy, float deltaTime)
        {
            // 앵커 도착 → Idle
            if (!enemy.MoveTowardDestination(deltaTime))
            {
                enemy.ChangeState(EnemyIdleState.Instance);
            }
        }

        public void Exit(EnemyController enemy) { }
    }
}
