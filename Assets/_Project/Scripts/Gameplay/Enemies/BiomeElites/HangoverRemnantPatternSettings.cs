using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Hangover Remnant Pattern")]
    public sealed class HangoverRemnantPatternSettings : MonsterPatternSettings
    {
        public const string DamageId = "hangover-burst";
        [Min(.1f)] public float triggerDistance = 2.5f;
        [Min(0)] public float spawnGraceSeconds = .75f;
        [Min(.1f)] public float windupSeconds = .65f;
        [Min(.1f)] public float retreatDistance = 2.4f;
        [Min(.1f)] public float retreatSpeed = 6f;
        [Min(.1f)] public float burstDelaySeconds = 1.1f;
        [Tooltip("빨간 범위와 피해 판정이 공유하는 반경입니다. 플레이어 지면 중심이 안에 있으면 맞습니다.")]
        [Min(.1f)] public float burstRadius = 1.6f;
        [Min(.1f)] public float recoverySeconds = 1f;
        [Tooltip("회복과 잔여물 정리가 모두 끝난 후 한 번 적용합니다. 난이도 재사용 대기 배율만 사용합니다.")]
        [Min(0)] public float rearmSeconds = 3.5f;
        public HangoverRemnantPresentation presentation;

        public override string GetValidationError(MonsterDefinition definition)
        {
            if (!MonsterBalanceNumbers.Positive(triggerDistance) || !MonsterBalanceNumbers.NonNegative(spawnGraceSeconds)
                || !MonsterBalanceNumbers.Positive(windupSeconds) || !MonsterBalanceNumbers.Positive(retreatDistance)
                || !MonsterBalanceNumbers.Positive(retreatSpeed) || !MonsterBalanceNumbers.Positive(burstDelaySeconds)
                || !MonsterBalanceNumbers.Positive(burstRadius) || !MonsterBalanceNumbers.Positive(recoverySeconds)
                || !MonsterBalanceNumbers.NonNegative(rearmSeconds)) return "숙취 잔재의 거리·시간은 유효한 양수여야 합니다.";
            if (definition.tier != MonsterTier.Elite || !definition.patternDamage.Exists(x => x != null && x.id == DamageId))
                return "Elite 등급과 hangover-burst 피해 계수가 필요합니다.";
            return presentation != null ? presentation.GetValidationError() : "숙취 잔재 표현 원본이 없습니다.";
        }

        public override void Attach(EnemyController enemy)
        {
            var pattern = enemy.GetComponent<HangoverRemnantElitePattern>() ?? enemy.gameObject.AddComponent<HangoverRemnantElitePattern>();
            pattern.Initialize(enemy, this);
        }
    }
}
