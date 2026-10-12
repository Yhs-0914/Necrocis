using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Inflammation Ember Pattern")]
    public sealed class InflammationEmberPatternSettings : MonsterPatternSettings
    {
        public const string DamageId = "ember-counter";
        [Min(.1f)] public float counterMaxDistance = 8f;
        [Min(.1f)] public float approachStopDistance = 1.5f;
        [Min(.1f)] public float windupSeconds = .7f;
        [Min(.1f)] public float recoverySeconds = 1.2f;
        [Tooltip("회복 뒤 난이도 대기 배율을 한 번 적용합니다. 대기 중의 피격은 다음 반격으로 예약하지 않습니다.")]
        [Min(0)] public float rearmSeconds = 2.5f;
        [Min(.1f)] public float thornSpeed = 4f;
        [Min(.1f)] public float thornLifetimeSeconds = 2f;
        [Tooltip("가시 끝부터 뒤까지의 전체 길이. 그림과 수평 캡슐 판정이 공유합니다.")]
        [Min(.1f)] public float thornLength = .7f;
        [Tooltip("가시의 반두께. 전체 길이의 절반 이하여야 합니다. 비행 높이도 이 값을 사용합니다.")]
        [Min(.02f)] public float thornRadius = .16f;
        public InflammationEmberPresentation presentation;

        public override string GetValidationError(MonsterDefinition definition)
        {
            if (!MonsterBalanceNumbers.Positive(counterMaxDistance) || !MonsterBalanceNumbers.Positive(approachStopDistance)
                || approachStopDistance > counterMaxDistance || !MonsterBalanceNumbers.Positive(windupSeconds)
                || !MonsterBalanceNumbers.Positive(recoverySeconds) || !MonsterBalanceNumbers.NonNegative(rearmSeconds)
                || !MonsterBalanceNumbers.Positive(thornSpeed) || !MonsterBalanceNumbers.Positive(thornLifetimeSeconds)
                || !MonsterBalanceNumbers.Positive(thornLength) || !MonsterBalanceNumbers.Positive(thornRadius)
                || thornRadius * 2 > thornLength) return "염증 불씨의 거리·시간·탄 크기를 확인하세요. 가시 길이는 반경의 두 배 이상이어야 합니다.";
            if (definition.tier != MonsterTier.Elite || definition.patternDamage == null
                || !definition.patternDamage.Exists(p => p != null && p.id == DamageId)) return "염증 불씨에는 Elite 등급과 ember-counter 피해 계수가 필요합니다.";
            return presentation != null ? presentation.GetValidationError() : "염증 불씨 표현 원본이 없습니다.";
        }

        public override void Attach(EnemyController enemy)
        {
            var pattern = enemy.GetComponent<InflammationEmberElitePattern>() ?? enemy.gameObject.AddComponent<InflammationEmberElitePattern>();
            pattern.Initialize(enemy, this);
        }
    }
}
