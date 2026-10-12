using UnityEngine;

namespace Necrocis
{
    public enum GasSacAttackKind { Burst, Orb }

    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Gas Sac Pattern")]
    public sealed class GasSacPatternSettings : MonsterPatternSettings
    {
        public const string DamageId = "gas-burst";
        public const string OrbDamageId = "gas-orb";
        [Header("A · Self-Centered Burst")]
        [Min(.1f)] public float triggerDistance = 2.5f;
        [Min(.1f)] public float burstRadius = 3.25f;
        [Min(.1f)] public float windupSeconds = 1.3f;
        [Min(.05f)] public float burstVisualSeconds = .18f;
        [Min(.1f)] public float recoverySeconds = 1.5f;
        [Tooltip("회복 뒤 한 번 적용하는 재사용 대기. 난이도 대기 배율만 적용합니다.")]
        [Min(0)] public float rearmSeconds = 2.5f;
        [Min(0)] public float spawnGraceSeconds = .75f;
        [Header("B · Distance Selection")]
        public bool orbEnabled;
        [Tooltip("이 거리 이하에서 근거리 우선으로 돌아옵니다. A 시작 거리 이상이어야 합니다.")]
        [Min(.1f)] public float orbExitDistance = 3f;
        [Tooltip("이 거리 이상에서 원거리 우선으로 바뀝니다. 두 경계 사이에서는 이전 우선순위를 유지합니다.")]
        [Min(.1f)] public float orbEnterDistance = 3.8f;
        [Tooltip("생성 당시 경계 구간에 있을 때의 최초 우선순위입니다. 공격 시작 후에는 회복까지 고정합니다.")]
        public GasSacAttackKind boundaryPriority = GasSacAttackKind.Burst;
        [Min(.1f)] public float orbMaxDistance = 6.5f;
        [Header("B · Single Compressed Gas Orb")]
        [Min(.1f)] public float orbWindupSeconds = 1.1f;
        [Min(.1f)] public float orbSpeed = 3f;
        [Min(.1f)] public float orbLifetimeSeconds = 3f;
        [Tooltip("탄 자체의 판정 반경. 플레이어 충돌체 크기는 별도로 포함합니다.")]
        [Min(.05f)] public float orbHitRadius = .3f;
        [Min(.1f)] public float orbRecoverySeconds = 1.2f;
        [Tooltip("B 회복 뒤 한 번만 적용합니다. A의 회복/대기를 합산하지 않습니다.")]
        [Min(0)] public float orbRearmSeconds = 2.5f;
        [Header("Presentation Reference")]
        public GasSacPresentation presentation;

        public override string GetValidationError(MonsterDefinition definition)
        {
            if (!MonsterBalanceNumbers.Positive(triggerDistance) || !MonsterBalanceNumbers.Positive(burstRadius)
                || triggerDistance > burstRadius || !MonsterBalanceNumbers.Positive(windupSeconds)
                || !MonsterBalanceNumbers.Positive(burstVisualSeconds) || !MonsterBalanceNumbers.Positive(recoverySeconds)
                || !MonsterBalanceNumbers.NonNegative(rearmSeconds) || !MonsterBalanceNumbers.NonNegative(spawnGraceSeconds))
                return "가스낭의 거리/시간을 확인하세요. 시작 거리는 폭발 반경 이하여야 합니다.";
            if (definition.tier != MonsterTier.Elite || definition.patternDamage == null
                || !definition.patternDamage.Exists(p => p != null && p.id == DamageId))
                return "가스낭에는 Elite 등급과 gas-burst 피해 계수가 필요합니다.";
            if (orbEnabled)
            {
                if (!MonsterBalanceNumbers.Positive(orbExitDistance) || orbExitDistance < triggerDistance
                    || !MonsterBalanceNumbers.Positive(orbEnterDistance) || orbEnterDistance <= orbExitDistance
                    || !MonsterBalanceNumbers.Positive(orbMaxDistance) || orbMaxDistance < orbEnterDistance
                    || !MonsterBalanceNumbers.Positive(orbWindupSeconds) || !MonsterBalanceNumbers.Positive(orbSpeed)
                    || !MonsterBalanceNumbers.Positive(orbLifetimeSeconds) || !MonsterBalanceNumbers.Positive(orbHitRadius)
                    || !MonsterBalanceNumbers.Positive(orbRecoverySeconds) || !MonsterBalanceNumbers.NonNegative(orbRearmSeconds)
                    || (boundaryPriority != GasSacAttackKind.Burst && boundaryPriority != GasSacAttackKind.Orb))
                    return "가스탄: A 시작 거리 ≤ 근거리 복귀 < 원거리 진입 ≤ 최대 거리, 양수 시간/속도/반경을 확인하세요.";
                if (!definition.patternDamage.Exists(p => p != null && p.id == OrbDamageId))
                    return "B를 사용하려면 원본 정의에 gas-orb 피해 계수가 필요합니다.";
            }
            if (presentation == null) return "가스낭 표현 에셋이 없습니다.";
            if (presentation.directionalPresentation == null) return "가스낭의 방향별 표현 원본이 필요합니다.";
            return presentation.GetValidationError();
        }

        public override void Attach(EnemyController enemy)
        {
            var controller = enemy.GetComponent<GasSacElitePattern>() ?? enemy.gameObject.AddComponent<GasSacElitePattern>();
            controller.Initialize(enemy, this);
        }
    }
}
