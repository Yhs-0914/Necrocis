using System.Collections.Generic;

// ========================================
// 1단계: Enum 정의
// ========================================

// 실제 능력치 종류
public enum StatType 
{
    Health,        // 체력
    Speed,         // 이동속도
    Attack,        // 공격력
    Defense,       // 방어력
    AttackSpeed,   // 공격속도
    Range,         // 공격 범위
    Magic,         // 마력
    Cooldown       // 쿨타임
}

// 플레이어가 선택하는 옵션
public enum StatChoice 
{
    HealthUp,           // 체력 +10
    SpeedUp,            // 이속 +3%
    AttackDefenseUp,    // 공격력 +3, 방어력 +1
    AttackSpeedRangeUp, // 공속 +5%, 범위 +5%
    MagicCooldownUp     // 마력 +3, 쿨타임 -3%
}

// ========================================
// 2단계: StatEffect 클래스
// ========================================

public class StatEffect 
{
    public Dictionary<StatType, float> flatStats = new Dictionary<StatType, float>();     // 고정 수치
    public Dictionary<StatType, float> percentStats = new Dictionary<StatType, float>();  // 비율 수치
}

// ========================================
// 3단계: StatManager (데이터 관리)
// ========================================

public static class StatManager 
{
    // 스탯 효과 데이터
    private static Dictionary<StatChoice, StatEffect> statEffects;
    
    // 정적 생성자 (게임 시작 시 자동 실행)
    static StatManager() 
    {
        InitializeStatData();
    }
    
    // 데이터 초기화
    private static void InitializeStatData() 
    {
        statEffects = new Dictionary<StatChoice, StatEffect>
        {
            // 1. 체력 +10
            [StatChoice.HealthUp] = new StatEffect 
            {
                flatStats = new Dictionary<StatType, float> 
                {
                    [StatType.Health] = 10
                }
            },
            
            // 2. 이속 +3%
            [StatChoice.SpeedUp] = new StatEffect 
            {
                percentStats = new Dictionary<StatType, float> 
                {
                    [StatType.Speed] = 3
                }
            },
            
            // 3. 공격력 +3, 방어력 +1
            [StatChoice.AttackDefenseUp] = new StatEffect 
            {
                flatStats = new Dictionary<StatType, float> 
                {
                    [StatType.Attack] = 3,
                    [StatType.Defense] = 1
                }
            },
            
            // 4. 공속 +5%, 범위 +5%
            [StatChoice.AttackSpeedRangeUp] = new StatEffect 
            {
                percentStats = new Dictionary<StatType, float> 
                {
                    [StatType.AttackSpeed] = 5,
                    [StatType.Range] = 5
                }
            },
            
            // 5. 마력 +3, 쿨타임 -3%
            [StatChoice.MagicCooldownUp] = new StatEffect 
            {
                flatStats = new Dictionary<StatType, float> 
                {
                    [StatType.Magic] = 3
                },
                percentStats = new Dictionary<StatType, float> 
                {
                    [StatType.Cooldown] = -3
                }
            }
        };
    }
    
    // 외부에서 스탯 효과 가져오기
    public static StatEffect GetStatEffect(StatChoice choice) 
    {
        return statEffects[choice];
    }
}