using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Hardened Residue A Pattern")]
    public sealed class HardenedResiduePatternSettings : MonsterPatternSettings
    {
        public const string DamageId = "residue-landing";
        [Header("Selection and Landing")]
        [Min(.1f)] public float triggerDistance = 2.8f;
        [Min(.1f)] public float placementDistance = 2.3f;
        [Tooltip("잔해의 기준 길이(X)·폭(Y)입니다. 좌우는 가로, 상하는 세로로 예고·피해·이동 차단·파괴 판정에 함께 적용합니다. 플레이어 중심이 표시 안에 있으면 착지 피해를 받습니다.")]
        public Vector2 footprint = new Vector2(2.2f, .7f);
        [Min(.1f)] public float windupSeconds = .8f;
        [Min(.02f)] public float dropSeconds = .12f;
        [Min(.1f)] public float recoverySeconds = 1f;
        [Tooltip("회복 뒤 한 번 적용하며 난이도 재사용 대기 배율만 곱합니다.")]
        [Min(0)] public float rearmSeconds = 2.5f;
        [Min(0)] public float spawnGraceSeconds = .75f;
        [Header("Breakable Rubble")]
        [Min(.1f)] public float rubbleLifetime = 2f;
        [Tooltip("서로 다른 유효 플레이어 타격 수. 기본 1회. 동일 투사체의 중복 충돌은 한 번만 셉니다.")]
        [Min(1)] public int hitsToBreak = 1;
        [Header("Presentation")]
        public HardenedResiduePresentation presentation;

        public override string GetValidationError(MonsterDefinition definition)
        {
            if (!MonsterBalanceNumbers.Positive(triggerDistance) || !MonsterBalanceNumbers.Positive(placementDistance)
                || placementDistance > triggerDistance || !MonsterBalanceNumbers.Positive(footprint.x)
                || !MonsterBalanceNumbers.Positive(footprint.y) || !MonsterBalanceNumbers.Positive(windupSeconds)
                || !MonsterBalanceNumbers.Positive(dropSeconds) || !MonsterBalanceNumbers.Positive(recoverySeconds)
                || !MonsterBalanceNumbers.Positive(rubbleLifetime) || !MonsterBalanceNumbers.NonNegative(rearmSeconds)
                || !MonsterBalanceNumbers.NonNegative(spawnGraceSeconds) || hitsToBreak < 1)
                return "굳은 잔여체의 거리/폭/깊이/시간/파괴 횟수를 확인하세요. 배치 거리는 공격 시작 거리 이하여야 합니다.";
            if (definition.tier != MonsterTier.Elite || definition.patternDamage == null
                || !definition.patternDamage.Exists(p => p != null && p.id == DamageId))
                return "Elite 등급과 residue-landing 피해 계수가 필요합니다.";
            return presentation != null ? presentation.GetValidationError() : "굳은 잔여체 표현 원본이 없습니다.";
        }

        public override void Attach(EnemyController enemy)
        {
            var pattern = enemy.GetComponent<HardenedResidueElitePattern>() ?? enemy.gameObject.AddComponent<HardenedResidueElitePattern>();
            pattern.Initialize(enemy, this);
        }
    }
}
