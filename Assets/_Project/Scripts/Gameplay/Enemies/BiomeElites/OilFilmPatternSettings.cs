using UnityEngine;
namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Oil Film Pattern")]
    public sealed class OilFilmPatternSettings : MonsterPatternSettings
    {
        public const string DamageId = "oil-film-spread";
        [Min(.1f)] public float triggerDistance = 3.2f;
        [Min(.1f)] public float approachStopDistance = 2f;
        [Min(0)] public float spawnGraceSeconds = .75f;
        [Min(.1f)] public float windupSeconds = .85f;
        [Min(.1f)] public float attackRange = 2.5f;
        [Range(30, 160)] public float arcDegrees = 110f;
        [Min(.08f)] public float releaseSeconds = .28f;
        [Range(.15f, .85f)] public float impactNormalizedTime = .5f;
        [Min(.1f)] public float recoverySeconds = 1.1f;
        [Min(0)] public float rearmSeconds = 2.5f;
        [Tooltip("핵 몸통의 접촉 범위입니다. 펼쳐진 막이나 받는 공격용 콜라이더를 포함하지 않습니다.")]
        [Min(.05f)] public float coreRadius = .28f;
        public OilFilmPresentation presentation;
        public override string GetValidationError(MonsterDefinition definition)
        {
            foreach (float n in new[] { triggerDistance, approachStopDistance, windupSeconds, attackRange, releaseSeconds, recoverySeconds, coreRadius })
                if (!MonsterBalanceNumbers.Positive(n)) return "기름막 거리·시간·핵 범위는 유한한 양수여야 합니다.";
            if (!MonsterBalanceNumbers.NonNegative(spawnGraceSeconds) || !MonsterBalanceNumbers.NonNegative(rearmSeconds)
                || approachStopDistance > triggerDistance || coreRadius >= attackRange) return "기름막 접근·대기·핵 범위를 확인하세요.";
            if (!MonsterBalanceNumbers.Positive(arcDegrees) || arcDegrees < 30 || arcDegrees > 160
                || !MonsterBalanceNumbers.Positive(impactNormalizedTime) || impactNormalizedTime < .15f || impactNormalizedTime > .85f)
                return "기름막 각도는30~160도, 타격 시점은0.15~0.85입니다.";
            if (definition.tier != MonsterTier.Elite || definition.patternDamage == null || !definition.patternDamage.Exists(p => p != null && p.id == DamageId))
                return "Elite 등급과 oil-film-spread 피해 계수가 필요합니다.";
            return presentation != null ? presentation.GetValidationError() : "기름막 표현 원본이 없습니다.";
        }
        public override void Attach(EnemyController enemy)
        {
            var pattern = enemy.GetComponent<OilFilmElitePattern>() ?? enemy.gameObject.AddComponent<OilFilmElitePattern>();
            pattern.Initialize(enemy, this);
        }
    }
}
