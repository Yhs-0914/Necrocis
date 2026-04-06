using System.Collections.Generic;

namespace Necrocis
{
    // 레벨업 시 선택 가능한 스탯 종류
    public enum StatChoice
    {
        HealthUp,            // 체력 증가
        SpeedUp,             // 이동속도 증가
        AttackDefenseUp,     // 공격력/방어력 증가
        AttackSpeedRangeUp,  // 공격속도/사거리 증가
        MagicCooldownUp      // 마력 증가/쿨타임 감소
    }

    /// <summary>
    /// 스탯 선택지의 효과를 정의하는 데이터 클래스.
    /// flatStats: 고정값 증가 (예: 공격력 +3)
    /// percentStats: 퍼센트 증가 (예: 이동속도 +3%, 내부적으로 /100 변환은 PlayerStats에서 처리)
    /// </summary>
    public class StatEffect
    {
        public Dictionary<CharacterStatType, float> flatStats = new Dictionary<CharacterStatType, float>();
        public Dictionary<CharacterStatType, float> percentStats = new Dictionary<CharacterStatType, float>();
    }

    /// <summary>
    /// 각 StatChoice에 대한 구체적 효과를 정의하는 정적 클래스.
    /// PlayerStats.ApplyStatChoice()에서 이 데이터를 참조하여 모디파이어를 적용한다.
    /// </summary>
    public static class StatManager
    {
        private static Dictionary<StatChoice, StatEffect> statEffects; // 선택지별 효과 매핑

        // 정적 생성자: 클래스 최초 접근 시 자동 초기화
        static StatManager()
        {
            InitializeStatData();
        }
        // InitializeStatData: 관련 설정과 상태를 구성합니다.

        // 모든 스탯 선택지의 효과를 정의
        private static void InitializeStatData()
        {
            statEffects = new Dictionary<StatChoice, StatEffect>
            {
                // 체력 증가: 최대HP +10 (고정값)
                [StatChoice.HealthUp] = new StatEffect
                {
                    flatStats = new Dictionary<CharacterStatType, float>
                    {
                        [CharacterStatType.MaxHealth] = 10
                    }
                },
                // 이동속도 증가: 이동속도 +3% (퍼센트)
                [StatChoice.SpeedUp] = new StatEffect
                {
                    percentStats = new Dictionary<CharacterStatType, float>
                    {
                        [CharacterStatType.MoveSpeed] = 3
                    }
                },
                // 공격/방어 증가: 공격력 +3, 방어력 +1 (고정값)
                [StatChoice.AttackDefenseUp] = new StatEffect
                {
                    flatStats = new Dictionary<CharacterStatType, float>
                    {
                        [CharacterStatType.AttackPower] = 3,
                        [CharacterStatType.Defense] = 1
                    }
                },
                // 공격속도/사거리 증가: 공격속도 +5%, 사거리 +5% (퍼센트)
                [StatChoice.AttackSpeedRangeUp] = new StatEffect
                {
                    percentStats = new Dictionary<CharacterStatType, float>
                    {
                        [CharacterStatType.AttackSpeed] = 5,
                        [CharacterStatType.Range] = 5
                    }
                },
                // 마력/쿨타임: 마력 +3 (고정값), 쿨타임 -3% (퍼센트 감소)
                [StatChoice.MagicCooldownUp] = new StatEffect
                {
                    flatStats = new Dictionary<CharacterStatType, float>
                    {
                        [CharacterStatType.Magic] = 3
                    },
                    percentStats = new Dictionary<CharacterStatType, float>
                    {
                        [CharacterStatType.Cooldown] = -3
                    }
                }
            };
        }
        // GetStatEffect: 필요한 값을 반환합니다.

        // 선택지에 해당하는 효과 데이터 반환
        public static StatEffect GetStatEffect(StatChoice choice)
        {
            return statEffects[choice];
        }
    }
}
