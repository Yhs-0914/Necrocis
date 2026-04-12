namespace Necrocis
{
    /// <summary>
    /// 적 FSM 상태 인터페이스.
    /// Enter: 상태 진입 시 1회, Update: 매 프레임, Exit: 상태 종료 시 1회 호출.
    /// 싱글톤 패턴으로 구현 (각 상태 클래스에 static Instance).
    /// </summary>
    public interface IEnemyState
    {
        void Enter(EnemyController enemy);
        void Update(EnemyController enemy, float deltaTime);
        void Exit(EnemyController enemy);
    }
}
