using UnityEngine;

namespace Necrocis
{
    [System.Serializable]
    public sealed class PollenFollowupSettings
    {
        public bool enabled;
        [Tooltip("첫 탄 사이 틈으로 옮기는 회전 크기. 측면 대칭 시 회전 부호도 대칭합니다.")]
        [Range(1, 59)] public float angleOffset = 15f;
        [Tooltip("첫 발사 자세를 포함한 두 발사 사이 전체 예고 시간입니다.")]
        [Min(.2f)] public float rotationSeconds = .8f;
        [Min(.1f)] public float recoverySeconds = 1.2f;
        [Min(0)] public float rearmSeconds = 3.2f;
    }

    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Pollen Invader Pattern")]
    public sealed class PollenInvaderPatternSettings : MonsterPatternSettings
    {
        public const string DamageId = "pollen-volley";
        public const string FollowupDamageId = "pollen-followup";
        [Min(.1f)] public float triggerDistance = 6f;
        [Min(.1f)] public float approachStopDistance = 3.2f;
        [Min(0)] public float spawnGraceSeconds = .75f;
        [Min(.1f)] public float windupSeconds = .85f;
        [Range(10, 60)] public float spreadHalfAngle = 30f;
        [Min(.1f)] public float pelletSpeed = 3.5f;
        [Min(.1f)] public float pelletLifetimeSeconds = 2.4f;
        [Tooltip("탄 크기·원형 판정·낮은 비행 높이가 공유하는 반경입니다.")]
        [Min(.02f)] public float pelletRadius = .16f;
        [Min(.1f)] public float recoverySeconds = 1f;
        [Min(0)] public float rearmSeconds = 2.5f;
        public PollenFollowupSettings followup = new PollenFollowupSettings();
        public PollenInvaderPresentation presentation;

        public override string GetValidationError(MonsterDefinition definition)
        {
            foreach (float n in new[] { triggerDistance, approachStopDistance, windupSeconds, pelletSpeed, pelletLifetimeSeconds, pelletRadius, recoverySeconds })
                if (!MonsterBalanceNumbers.Positive(n)) return "꽃가루 거리·시간·탄 크기는 양수여야 합니다.";
            if (approachStopDistance > triggerDistance || !MonsterBalanceNumbers.NonNegative(spawnGraceSeconds)
                || !MonsterBalanceNumbers.NonNegative(rearmSeconds) || !MonsterBalanceNumbers.Positive(spreadHalfAngle)
                || spreadHalfAngle < 10 || spreadHalfAngle > 60) return "꽃가루 접근 거리·대기·발사 각도(10~60도)를 확인하세요.";
            if (definition.tier != MonsterTier.Elite || definition.patternDamage == null
                || !definition.patternDamage.Exists(p => p != null && p.id == DamageId)) return "Elite 등급과 pollen-volley 피해 계수가 필요합니다.";
            if (followup == null) return "꽃가루 추가 선택 설정이 없습니다.";
            if (followup.enabled)
            {
                if (!MonsterBalanceNumbers.Positive(followup.angleOffset) || followup.angleOffset >= spreadHalfAngle
                    || !MonsterBalanceNumbers.Positive(followup.rotationSeconds) || followup.rotationSeconds < .2f
                    || !MonsterBalanceNumbers.Positive(followup.recoverySeconds) || !MonsterBalanceNumbers.NonNegative(followup.rearmSeconds))
                    return "B 회전각은 0보다 크고 기본 탄 간격보다 작아야 하며, 예고는 0.2초 이상이어야 합니다.";
                if (!definition.patternDamage.Exists(p => p != null && p.id == FollowupDamageId)) return "B의 pollen-followup 피해 계수가 필요합니다.";
            }
            return presentation != null ? presentation.GetValidationError(followup.enabled) : "꽃가루 표현 원본이 없습니다.";
        }
        public override void Attach(EnemyController enemy)
        {
            var pattern = enemy.GetComponent<PollenInvaderElitePattern>() ?? enemy.gameObject.AddComponent<PollenInvaderElitePattern>();
            pattern.Initialize(enemy, this);
        }
    }
}
