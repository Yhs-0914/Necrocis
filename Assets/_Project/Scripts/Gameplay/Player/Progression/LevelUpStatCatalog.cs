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

        // 정적 생성자: 클래스 최초 접근 시 자동 초기화
        static LevelUpStatCatalog()
        {
            InitializeStatData();
        }
        // InitializeStatData: 관련 설정과 상태를 구성합니다.

        // 모든 스탯 선택지의 효과를 정의
        private static void InitializeStatData()
        {
            statEffects = new Dictionary<LevelUpStatChoice, LevelUpStatEffect>
            {
                // 체력 증가: 최대HP +10 (고정값)
                [LevelUpStatChoice.HealthUp] = new LevelUpStatEffect
                {
                    flatStats = new Dictionary<CharacterStatType, float>
                    {
                        [CharacterStatType.MaxHealth] = 10
                    }
                },
                // 이동속도 증가: 이동속도 +3% (퍼센트)
                [LevelUpStatChoice.SpeedUp] = new LevelUpStatEffect
                {
                    percentStats = new Dictionary<CharacterStatType, float>
                    {
                        [CharacterStatType.MoveSpeed] = 3
                    }
                },
                // 공격 증가: 공격력 +3 (고정값)
                [LevelUpStatChoice.AttackPowerUp] = new LevelUpStatEffect
                {
                    flatStats = new Dictionary<CharacterStatType, float>
                    {
                        [CharacterStatType.AttackPower] = 3
                    }
                },
                // 공격속도/사거리 증가: 공격속도 +5%, 사거리 +5% (퍼센트)
                [LevelUpStatChoice.AttackSpeedRangeUp] = new LevelUpStatEffect
                {
                    percentStats = new Dictionary<CharacterStatType, float>
                    {
                        [CharacterStatType.AttackSpeed] = 5,
                        [CharacterStatType.AttackRange] = 5
                    }
                },
                // 마력 증가: 스킬 데미지 +3% (고정값)
                [LevelUpStatChoice.MagicUp] = new LevelUpStatEffect
                {
                    flatStats = new Dictionary<CharacterStatType, float>
                    {
                        [CharacterStatType.Magic] = 3
                    }
                }
            };
        }
        // GetStatEffect: 필요한 값을 반환합니다.

        // 선택지에 해당하는 효과 데이터 반환
        public static LevelUpStatEffect GetStatEffect(LevelUpStatChoice choice)
        {
            return statEffects[choice];
        }
    }
}
