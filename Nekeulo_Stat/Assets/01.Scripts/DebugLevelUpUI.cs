using UnityEngine;
using System.Collections.Generic;

public class DebugLevelUpUI : MonoBehaviour
{
    private List<StatChoice> currentChoices;
    private bool isSelectingStats = false;
    
    void Awake()  // Start → Awake로 변경 (더 빨리 실행)
    {
        // 레벨업 이벤트 구독
        LevelUpManager.OnLevelUp += HandleLevelUp;
        
        // 디버그: 현재 레벨 확인
        Debug.Log($"게임 시작 - 현재 레벨: {LevelUpManager.GetCurrentLevel()}");
        Debug.Log($"필요 경험치: {LevelUpManager.GetExpRequired()}");
    }
    
    void HandleLevelUp()
    {
        int level = LevelUpManager.GetCurrentLevel();
        Debug.Log($"========== LEVEL UP! Now Level {level} ==========");
        
        // 10레벨: 전직
        if (level == 10)
        {
            Debug.Log("=== 전직 선택 ===");
            Debug.Log("1번 키: 전사 (공격력+방어력)");
            Debug.Log("2번 키: 마법사 (마력+쿨타임)");
            Debug.Log("3번 키: 궁수 (공속+범위)");
            Time.timeScale = 0;
            return;
        }
        
        // 1~9레벨: 스탯 선택
        if (level < 10 || level >= 11)
        {
            ShowStatChoices();
        }
    }
    
    void ShowStatChoices()
    {
        currentChoices = LevelUpManager.GetRandomChoices();
        
        Debug.Log("=== 스탯 선택 ===");
        for (int i = 0; i < currentChoices.Count; i++)
        {
            Debug.Log($"{i + 1}번 키: {GetStatName(currentChoices[i])}");
        }
        
        isSelectingStats = true;
        Time.timeScale = 0;
    }
    
    void Update()
    {
        // 디버그: 현재 경험치 표시
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"현재 경험치: {LevelUpManager.GetCurrentExp()} / {LevelUpManager.GetExpRequired()}");
            Debug.Log($"현재 레벨: {LevelUpManager.GetCurrentLevel()}");
        }
        
        // 전직 선택 (10레벨)
        if (LevelUpManager.GetCurrentLevel() == 10 && Time.timeScale == 0)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectJob(JobType.Warrior);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectJob(JobType.Mage);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectJob(JobType.Archer);
            }
        }
        
        // 스탯 선택
        if (isSelectingStats)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && currentChoices.Count > 0)
            {
                SelectStat(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && currentChoices.Count > 1)
            {
                SelectStat(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) && currentChoices.Count > 2)
            {
                SelectStat(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) && currentChoices.Count > 3)
            {
                SelectStat(3);
            }
        }
    }
    
    void SelectJob(JobType job)
    {
        Debug.Log($"전직 선택: {job}");
        LevelUpManager.SetJob(job);
        Time.timeScale = 1;
    }
    
    void SelectStat(int index)
    {
        StatChoice choice = currentChoices[index];
        Debug.Log($"선택: {GetStatName(choice)}");
        
        PlayerStats.Instance.ApplyStatChoice(choice);
        LevelUpManager.RecordSelection(choice);
        
        Debug.Log($"현재 체력: {PlayerStats.Instance.GetHealth()}");
        Debug.Log($"현재 공격력: {PlayerStats.Instance.GetAttack()}");
        Debug.Log($"현재 이동속도: {PlayerStats.Instance.GetSpeed()}");
        
        isSelectingStats = false;
        Time.timeScale = 1;
    }
    
    string GetStatName(StatChoice choice)
    {
        switch (choice)
        {
            case StatChoice.HealthUp:
                return "체력 +10";
            case StatChoice.SpeedUp:
                return "이동속도 +3%";
            case StatChoice.AttackDefenseUp:
                return "공격력 +3, 방어력 +1";
            case StatChoice.AttackSpeedRangeUp:
                return "공격속도 +5%, 범위 +5%";
            case StatChoice.MagicCooldownUp:
                return "마력 +3, 쿨타임 -3%";
            default:
                return "???";
        }
    }
    
    void OnDestroy()
    {
        LevelUpManager.OnLevelUp -= HandleLevelUp;
    }
}