using System;
using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Balance/Final Boss Balance", fileName = "FinalBossBalanceConfig")]
    public sealed class FinalBossBalanceConfig : ScriptableObject
    {
        public const string ResourcePath = "Balance/FinalBoss/FinalBossBalanceConfig";
        [Tooltip("최종 보스 전용 설정입니다. 일반 바이옴의 난이도/진행도 배율을 중복 적용하지 않습니다.")]
        public FinalBossPhaseOneBalance phaseOne = new FinalBossPhaseOneBalance();
        public FinalBossPhaseTwoBalance phaseTwo = new FinalBossPhaseTwoBalance();
        public FinalBossPhaseThreeBalance phaseThree = new FinalBossPhaseThreeBalance();
        public FinalBossDifficultyBalance normal = new FinalBossDifficultyBalance();
        public FinalBossDifficultyBalance hard = new FinalBossDifficultyBalance();

        public FinalBossDifficultyBalance GetDifficulty(GameDifficulty difficulty) =>
            difficulty == GameDifficulty.Hard ? hard : normal;

        public string GetValidationError()
        {
            if (phaseOne == null || phaseTwo == null || phaseThree == null || normal == null || hard == null)
                return "최종 보스 페이즈/난이도 설정이 없습니다.";
            foreach (object group in new object[] { phaseOne, phaseTwo, phaseThree, normal, hard })
                foreach (var field in group.GetType().GetFields())
                {
                    object value = field.GetValue(group);
                    if (value is float number)
                    {
                        if (!MonsterBalanceNumbers.NonNegative(number)) return field.Name + ": 유한한 0 이상의 값이 필요합니다.";
                        var min = (MinAttribute)Attribute.GetCustomAttribute(field, typeof(MinAttribute));
                        var range = (RangeAttribute)Attribute.GetCustomAttribute(field, typeof(RangeAttribute));
                        if (min != null && number < min.min) return field.Name + ": 최솟값은 " + min.min;
                        if (range != null && (number < range.min || number > range.max)) return field.Name + ": 허용 범위를 벗어났습니다.";
                    }
                    else if (value is int count)
                    {
                        var range = (RangeAttribute)Attribute.GetCustomAttribute(field, typeof(RangeAttribute));
                        if (count < 1 || (range != null && (count < range.min || count > range.max)))
                            return field.Name + ": 개수 범위를 확인하세요.";
                    }
                }
            return null;
        }
    }

    [Serializable]
    public sealed class FinalBossDifficultyBalance
    {
        [Min(.01f)] public float health = 1f;
        [Min(0f)] public float damage = 1f;
        [Min(.01f)] public float summonMoveSpeed = 1f;
        [Min(.01f)] public float summonCooldown = 1f;
        [Min(0f)] public float summonExperience = 1f;
    }

    [Serializable]
    public sealed class FinalBossPhaseOneBalance
    {
        [Min(.25f)] public float spawnInterval = 3.5f;
        [Range(1, 12)] public int commonMinionsPerWave = 4;
        [Range(1, 3)] public int exclusiveTypesPerBiome = 2;
        [Range(1, 4)] public int exclusiveCopiesPerType = 2;
        [Range(.1f, 1f)] public float weakenedMidBossStatRatio = .33f;
        [Range(.1f, 1f)] public float weakenedMidBossScaleRatio = 1f;
        [Min(1f)] public float pillarMaxHealth = 65f;
    }

    [Serializable]
    public sealed class FinalBossPhaseTwoBalance
    {
        [Min(1f)] public float bossMaxHealth = 520f;
        [Min(.1f)] public float phaseStartDelay = 1.4f;
        [Min(.1f)] public float patternCooldown = 1.8f;
        [Min(.1f)] public float electricWarningDuration = 1.35f;
        [Range(1, 6)] public int electricLineCount = 3;
        [Min(.2f)] public float electricLineWidth = 1.45f;
        [Min(0f)] public float electricDamage = 2f;
        [Min(.1f)] public float reversedControlDuration = 1f;
        [Min(.1f)] public float missileWarningDuration = 1.15f;
        [Range(1, 24)] public int missileCount = 12;
        [Min(.01f)] public float missileInterval = .11f;
        [Min(.1f)] public float missileSpeed = 7.8f;
        [Min(0f)] public float missileDamage = 1f;
        [Min(0f)] public float missileTurnSpeed = 115f;
        [Min(.1f)] public float bombWarningDuration = 1.45f;
        [Range(1, 12)] public int bombCount = 6;
        [Min(.2f)] public float bombRadius = 2.35f;
        [Min(0f)] public float bombDamage = 2f;
        [Min(.1f)] public float sweepWarningDuration = 1.55f;
        [Min(0f)] public float sweepDamage = 2f;
        [Min(.1f)] public float reflectionWarningDuration = 1.1f;
        [Min(.1f)] public float reflectionActiveDuration = 3.2f;
        [Range(0f, 2f)] public float reflectedDamageRatio = .55f;
        [Min(.1f)] public float barrageWarningDuration = 1.2f;
        [Range(12, 36)] public int barrageCellsPerWave = 24;
        [Range(1, 5)] public int barrageWaveCount = 3;
        [Min(.2f)] public float barrageWaveInterval = .65f;
        [Min(.1f)] public float barrageCellSpeed = 6.4f;
        [Min(0f)] public float barrageCellDamage = 1f;
        [Range(.01f, .99f)] public float phaseThreeHealthRatio = .4f;
    }

    [Serializable]
    public sealed class FinalBossPhaseThreeBalance
    {
        [Min(.1f)] public float detachDuration = 1.8f;
        [Min(.1f)] public float pursuitSpeed = 3.1f;
        [Min(.1f)] public float stoppingDistance = 3.2f;
        [Min(10f)] public float collapseDuration = 45f;
        [Min(.5f)] public float patternRecovery = 1.8f;
        [Min(0f)] public float dashDamage = 2f;
        [Min(0f)] public float grabDamage = 2f;
        [Min(0f)] public float tentacleDamage = 2f;
        [Min(0f)] public float fallingCellDamage = 1f;
        [Min(0f)] public float fallingBombDamage = 2f;
        [Min(0f)] public float boundaryDamage = 1f;
        [Min(0f)] public float overtimeDamage = 2f;
        [Range(.01f, .99f)] public float collapseHealthRatio = .1f;
    }
}
