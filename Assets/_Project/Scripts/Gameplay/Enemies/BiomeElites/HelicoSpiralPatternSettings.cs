using UnityEngine;
namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Helico Spiral Pattern")]
    public sealed class HelicoSpiralPatternSettings : MonsterPatternSettings
    {
        public const string DamageId = "helico-dash";
        public const string TailDamageId = "helico-tail";
        [Min(.1f)] public float triggerDistance = 4.5f;
        [Min(.1f)] public float approachStopDistance = 2.5f;
        [Min(0)] public float spawnGraceSeconds = .75f;
        [Min(.1f)] public float windupSeconds = .7f;
        [Min(.1f)] public float dashDistance = 3.5f;
        [Min(.05f)] public float dashSeconds = .25f;
        [Tooltip("빨간 예고·돌진 피해·몸 접촉의 공통 몸통 폭입니다. 편모는 포함하지 않습니다.")]
        [Min(.1f)] public float bodyWidth = .8f;
        [Tooltip("몸 중앙에서 앞뒤로 이어지는 캡슐 중심선의 반길이. 반원 반경은 몸통 폭/2입니다.")]
        [Min(0)] public float bodyHalfLength = .35f;
        [Min(.1f)] public float recoverySeconds = .9f;
        [Min(0)] public float rearmSeconds = 2.5f;
        // Serialized compatibility only: S-01-B is retired and cannot run.
        [HideInInspector] public bool tailSweepEnabled;
        [HideInInspector, Min(.1f)] public float tailWindupSeconds = .9f;
        [HideInInspector, Min(.05f)] public float tailSweepSeconds = .18f;
        [HideInInspector, Min(.1f)] public float tailReach = 1.1f;
        [HideInInspector, Range(10, 160)] public float tailArcDegrees = 120;
        [HideInInspector, Min(.01f)] public float tailWidth = .09f;
        [HideInInspector, Min(.1f)] public float tailRecoverySeconds = 1.25f;
        [HideInInspector, Min(0)] public float tailRearmSeconds = 2.5f;
        public HelicoSpiralPresentation presentation;
        public override string GetValidationError(MonsterDefinition definition)
        {
            foreach (float n in new[] { triggerDistance, approachStopDistance, windupSeconds, dashDistance, dashSeconds, bodyWidth, recoverySeconds })
                if (!MonsterBalanceNumbers.Positive(n)) return "나선충 거리·폭·시간은 양수여야 합니다.";
            foreach (float n in new[] { spawnGraceSeconds, bodyHalfLength, rearmSeconds })
                if (!MonsterBalanceNumbers.NonNegative(n)) return "나선충 대기·몸통 반길이는 0 이상이어야 합니다.";
            if (approachStopDistance > triggerDistance) return "접근 정지 거리는 공격 발동 거리 이하여야 합니다.";
            if (definition.tier != MonsterTier.Elite || definition.patternDamage == null || !definition.patternDamage.Exists(p => p != null && p.id == DamageId)) return "Elite 등급과 helico-dash 피해 계수가 필요합니다.";
            return presentation != null ? presentation.GetValidationError() : "나선충 표현 원본이 없습니다.";
        }
        public override void Attach(EnemyController enemy)
        {
            var p = enemy.GetComponent<HelicoSpiralElitePattern>() ?? enemy.gameObject.AddComponent<HelicoSpiralElitePattern>();
            p.Initialize(enemy, this);
        }
    }
}
