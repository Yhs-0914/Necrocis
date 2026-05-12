using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 공격 상태: 공격 애니메이션 재생 → 완료 시 데미지 적용 → 쿨타임 대기 → 반복
    /// 대식세포: 공격 중 콜라이더 확장 (가시 펼침)
    /// NK세포: 플레이어 방향에 따라 상/하/좌/우 공격 애니메이션
    /// </summary>
    public class EnemyAttackState : IEnemyState
    {
        public static readonly EnemyAttackState Instance = new EnemyAttackState();

        public void Enter(EnemyController enemy)
        {
            enemy.SetIdleAnimation();
            // 즉시 공격 시도 (쿨타임이 0이면 바로 공격 애니메이션 시작)
            enemy.TryPerformAttack(0f);
        }

        public void Update(EnemyController enemy, float deltaTime)
        {
            // 체력 0 → Dead
            if (enemy.IsDead)
            {
                enemy.CancelAttackAnimation();
                enemy.ChangeState(EnemyDeadState.Instance);
                return;
            }

            // 공격 애니메이션 재생 중이면 대기
            if (enemy.IsAttackAnimPlaying)
            {
                // 원거리 적: 공격 중에도 플레이어 방향으로 스프라이트 방향 갱신
                enemy.UpdateFacingDirection();
                return;
            }

            // 범위 이탈 → Chase
            if (!enemy.IsPlayerInAttackRange())
            {
                enemy.ChangeState(EnemyChaseState.Instance);
                return;
            }

            // 공격 시도 (쿨타임 기반)
            enemy.TryPerformAttack(deltaTime);
        }

        public void Exit(EnemyController enemy)
        {
            enemy.CancelAttackAnimation();
        }
    }
}
