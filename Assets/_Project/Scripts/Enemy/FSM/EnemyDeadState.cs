namespace Necrocis
{
    /// <summary>
    /// 사망 상태: 콜라이더 비활성화 → 경험치 부여 → 사망 애니메이션 재생 → 풀로 반환
    /// </summary>
    public class EnemyDeadState : IEnemyState
    {
        public static readonly EnemyDeadState Instance = new EnemyDeadState();

        private static readonly System.Collections.Generic.HashSet<EnemyController> waitingForAnim
            = new System.Collections.Generic.HashSet<EnemyController>();

        public void Enter(EnemyController enemy)
        {
            enemy.DisableCollider();
            enemy.GrantExp();

            // 엘리트 사망 특수 효과 (분열, 잔해 등)
            UnityEngine.Debug.Log($"[EnemyDead] Enter - IsElite={enemy.IsElite}, name={enemy.gameObject.name}");
            if (enemy.IsElite)
            {
                enemy.HandleEliteDeath();
            }

            // 사망 애니메이션 재생 시작
            waitingForAnim.Add(enemy);
            enemy.PlayDeathAnimation(() =>
            {
                waitingForAnim.Remove(enemy);
            });
        }

        public void Update(EnemyController enemy, float deltaTime)
        {
            // 사망 애니메이션이 끝날 때까지 대기
            if (waitingForAnim.Contains(enemy)) return;

            enemy.ReleaseToPool();
        }

        public void Exit(EnemyController enemy)
        {
            waitingForAnim.Remove(enemy);
        }
    }
}
