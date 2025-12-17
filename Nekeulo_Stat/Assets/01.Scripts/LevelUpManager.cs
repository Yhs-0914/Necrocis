using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ========================================
// 직업 Enum
// ========================================

public enum JobType 
{
    None,      // 전직 전
    Warrior,   // 전사
    Mage,      // 마법사
    Archer     // 궁수
}

// ========================================
// LevelUpManager
// ========================================

public static class LevelUpManager 
{
    // ========================================
    // 레벨/경험치 관리
    // ========================================
    
    private static int currentLevel = 1;
    private static int currentExp = 0;
    private static int expRequired = 100;
    
    private const int MAX_LEVEL = 30;
    private const int BASE_EXP = 100;
    private const float EXP_MULTIPLIER = 1.25f;
    
    // 이벤트
    public static Action OnLevelUp;
    public static Action<int> OnExpGained;
    
    // ========================================
    // 경험치 추가
    // ========================================

    public static void AddExp(int baseAmount)
    {
        if (currentLevel >= MAX_LEVEL)
        {
            return;
        }

        // 레벨별 경험치 배율 적용
        float multiplier = GetExpMultiplier();
        int actualExp = Mathf.RoundToInt(baseAmount * multiplier);

        currentExp += actualExp;
        OnExpGained?.Invoke(actualExp);

        CheckLevelUp();
    }

    // ========================================
    // 레벨별 경험치 배율
    // ========================================

    private static float GetExpMultiplier()
    {
        if (currentLevel <= 9)
            return 200f;  // 1~9레벨: 빠른 학습
        else if (currentLevel == 10)
            return 0f;    // 10레벨: 전직 중 경험치 획득 안됨
        else if (currentLevel <= 20)
            return 100f;  // 11~20레벨: 중간 속도
        else
            return 80f;   // 21~30레벨: 느린 성장
    }
    
    // ========================================
    // 레벨업 체크
    // ========================================
    
    private static void CheckLevelUp() 
    {
        while (currentExp >= expRequired && currentLevel < MAX_LEVEL) 
        {
            currentExp -= expRequired;
            currentLevel++;
            
            CalculateExpRequired();
            
            OnLevelUp?.Invoke();
        }
    }
    
    // ========================================
    // 필요 경험치 계산
    // ========================================
    
    private static void CalculateExpRequired() 
    {
        expRequired = Mathf.RoundToInt(BASE_EXP * Mathf.Pow(EXP_MULTIPLIER, currentLevel - 2));
    }
    
    // ========================================
    // 외부 접근용
    // ========================================
    
    public static int GetCurrentLevel() => currentLevel;
    public static int GetCurrentExp() => currentExp;
    public static int GetExpRequired() => expRequired;
    public static float GetExpProgress() => (float)currentExp / expRequired;
    
    // ========================================
    // 직업/히스토리
    // ========================================
    
    private static JobType currentJob = JobType.None;
    private static List<StatChoice> selectionHistory = new List<StatChoice>();
    
    private static Dictionary<JobType, StatChoice> jobStatMap = new Dictionary<JobType, StatChoice>
    {
        [JobType.Warrior] = StatChoice.AttackDefenseUp,
        [JobType.Mage] = StatChoice.MagicCooldownUp,
        [JobType.Archer] = StatChoice.AttackSpeedRangeUp
    };
    
    // ========================================
    // 랜덤 선택지 생성
    // ========================================
    
    public static List<StatChoice> GetRandomChoices() 
    {
        if (currentLevel < 10) 
        {
            return GetRandomFourChoices();
        }
        else if (currentLevel >= 11) 
        {
            return GetJobBasedChoices();
        }
        
        return new List<StatChoice>();
    }
    
    private static List<StatChoice> GetRandomFourChoices() 
    {
        List<StatChoice> allChoices = Enum.GetValues(typeof(StatChoice))
                                           .Cast<StatChoice>()
                                           .ToList();
        
        Shuffle(allChoices);
        return allChoices.Take(4).ToList();
    }
    
    private static List<StatChoice> GetJobBasedChoices() 
    {
        List<StatChoice> result = new List<StatChoice>();
        
        StatChoice jobStat = jobStatMap[currentJob];
        result.Add(jobStat);
        
        StatChoice mostSelected = GetMostSelectedStat(exclude: jobStat);
        result.Add(mostSelected);
        
        List<StatChoice> remaining = GetRemainingChoices(result);
        Shuffle(remaining);
        result.Add(remaining[0]);
        
        remaining = GetRemainingChoices(result);
        Shuffle(remaining);
        result.Add(remaining[0]);
        
        return result;
    }
    
    private static StatChoice GetMostSelectedStat(StatChoice exclude) 
    {
        Dictionary<StatChoice, int> counts = new Dictionary<StatChoice, int>();
        
        foreach (StatChoice choice in selectionHistory) 
        {
            if (choice == exclude) continue;
            
            if (!counts.ContainsKey(choice)) 
                counts[choice] = 0;
            
            counts[choice]++;
        }
        
        int maxCount = 0;
        List<StatChoice> mostSelectedList = new List<StatChoice>();
        
        foreach (var kvp in counts) 
        {
            if (kvp.Value > maxCount) 
            {
                maxCount = kvp.Value;
                mostSelectedList.Clear();
                mostSelectedList.Add(kvp.Key);
            }
            else if (kvp.Value == maxCount) 
            {
                mostSelectedList.Add(kvp.Key);
            }
        }
        
        if (mostSelectedList.Count > 0) 
        {
            return mostSelectedList[UnityEngine.Random.Range(0, mostSelectedList.Count)];
        }
        
        List<StatChoice> allChoices = Enum.GetValues(typeof(StatChoice))
                                           .Cast<StatChoice>()
                                           .Where(c => c != exclude)
                                           .ToList();
        return allChoices[UnityEngine.Random.Range(0, allChoices.Count)];
    }
    
    private static List<StatChoice> GetRemainingChoices(List<StatChoice> alreadySelected) 
    {
        return Enum.GetValues(typeof(StatChoice))
                    .Cast<StatChoice>()
                    .Where(c => !alreadySelected.Contains(c))
                    .ToList();
    }
    
    private static void Shuffle<T>(List<T> list) 
    {
        for (int i = list.Count - 1; i > 0; i--) 
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    // ========================================
    // 외부 인터페이스
    // ========================================
    
    public static void RecordSelection(StatChoice choice) 
    {
        selectionHistory.Add(choice);
    }
    
    public static void SetJob(JobType job) 
    {
        currentJob = job;
    }
    
    public static void ResetSelectionHistory() 
    {
        selectionHistory.Clear();
    }
    
    public static JobType GetCurrentJob() 
    {
        return currentJob;
    }
}