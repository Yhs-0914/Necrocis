using UnityEngine;

namespace Necrocis
{
    public partial class PlayerController
    {
        [Header("피격·공격 판정")]
        [Tooltip("가장 좁은 기본 대기 몸통 폭 안에 들어가는 피격 비율입니다. 직업/동작에 따라 흔들리지 않고, 아이템의 몸 크기 변화는 함께 따릅니다.")]
        [SerializeField, Range(.5f, 1f)] private float hurtboxWidthRatio = .9f;
        [Tooltip("플레이어 공격 가장자리의 추가 여유(m). 적에게 받는 피해 범위에는 더하지 않습니다.")]
        [SerializeField, Range(0f, .2f)] private float attackEdgePadding = .08f;
        private CapsuleCollider combatCapsule;
        private float bodyRadiusReference, bodyHeightReference, appliedHurtRatio = -1;
        public float AttackEdgePadding => Mathf.Clamp(attackEdgePadding, 0, .2f);
        public float HurtboxWidthRatio => Mathf.Clamp(hurtboxWidthRatio, .5f, 1f);

        private void InitializeCombatHitbox()
        {
            combatCapsule = HitCollider as CapsuleCollider;
            if (combatCapsule == null) return;
            float originalRadius = combatCapsule.radius;
            Sprite reference = null;
            if (idleSprites != null)
                foreach (Sprite frame in idleSprites)
                    if (frame != null && (reference == null || frame.bounds.size.x < reference.bounds.size.x)) reference = frame;
            if (reference == null) reference = CurrentVisualSprite;
            bodyRadiusReference = originalRadius;
            if (reference != null && spriteRenderer != null)
            {
                float worldWidth = spriteRenderer.transform.TransformVector(Vector3.right * reference.bounds.size.x).magnitude;
                Vector3 scale = combatCapsule.transform.lossyScale;
                float horizontalScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
                if (horizontalScale > .0001f) bodyRadiusReference = Mathf.Min(originalRadius, worldWidth * .5f / horizontalScale);
            }
            bodyHeightReference = combatCapsule.height * bodyRadiusReference / Mathf.Max(.0001f, originalRadius);
            RefreshCombatHitbox();
        }
        private void RefreshCombatHitbox()
        {
            if (combatCapsule == null || Mathf.Approximately(appliedHurtRatio, HurtboxWidthRatio)) return;
            appliedHurtRatio = HurtboxWidthRatio;
            combatCapsule.radius = Mathf.Max(.0001f, bodyRadiusReference * appliedHurtRatio);
            combatCapsule.height = Mathf.Max(combatCapsule.radius * 2, bodyHeightReference * appliedHurtRatio);
        }
    }
}
