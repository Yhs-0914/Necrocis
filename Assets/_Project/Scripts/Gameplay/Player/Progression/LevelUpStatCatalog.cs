using System.Collections.Generic;

namespace Necrocis
{
    // 레벨업 시 선택 가능한 스탯 종류
    public enum LevelUpStatChoice
    {
        HealthUp,            // 체력 증가
        SpeedUp,             // 이동속도 증가
        AttackPowerUp,       // 공격력 증가
        AttackSpeedRangeUp,  // 공격속도/사거리 증가
        MagicUp              // 마력 증가
    }

    /// <summary>
    /// 스탯 선택지의 효과를 정의하는 데이터 클래스.
    /// flatStats: 고정값 증가 (예: 공격력 +3)
    /// percentStats: 퍼센트 증가 (예: 이동속도 +3%, 내부적으로 /100 변환은 PlayerStats에서 처리)
    /// </summary>
    public class LevelUpStatEffect
    {
        public Dictionary<CharacterStatType, float> flatStats = new Dictionary<CharacterStatType, float>();
        public Dictionary<CharacterStatType, float> percentStats = new Dictionary<CharacterStatType, float>();
    }

    /// <summary>
    /// 각 LevelUpStatChoice에 대한 구체적 효과를 정의하는 정적 클래스.
    /// PlayerStats.ApplyLevelUpStatChoice()에서 이 데이터를 참조하여 모디파이어를 적용한다.
    /// </summary>
    public static class LevelUpStatCatalog
    {
        private static Dictionary<LevelUpStatChoice, LevelUpStatEffect> statEffects; // 선택지별 효과 매핑

        static LevelUpStatCatalog()
        {
            InitializeStatData();
        }

        private static void InitializeStatData()
        {
            statEffects = LevelUpManager.Config.BuildStatEffectMap();
        }

        // 선택지에 해당하는 효과 데이터 반환
        public static LevelUpStatEffect GetStatEffect(LevelUpStatChoice choice)
        {
            return statEffects[choice];
        }
    }
}
