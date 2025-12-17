using UnityEngine;
using TMPro;

public class StatInfoUI : MonoBehaviour
{
    [Header("UI 참조")]
    public GameObject statPanel;
    public TextMeshProUGUI statText;
    
    void Start()
    {
        // 시작 시 숨김
        statPanel.SetActive(false);
    }
    
    void Update()
    {
        // I키로 토글
        if (Input.GetKeyDown(KeyCode.I))
        {
            statPanel.SetActive(!statPanel.activeSelf);
            
            if (statPanel.activeSelf)
            {
                UpdateStatDisplay();
            }
        }
        
        // ESC로 닫기
        if (Input.GetKeyDown(KeyCode.Escape) && statPanel.activeSelf)
        {
            statPanel.SetActive(false);
        }
    }
    
    void UpdateStatDisplay()
    {
        PlayerStats stats = PlayerStats.Instance;
        
        string display = "";
        
        display += "=== 기본 스탯 ===\n\n";
        
        // 체력
        display += $"체력: {stats.GetHealth():F0}\n";
        display += $"  (기본 100 + {stats.GetFlatBonus(StatType.Health):F0})\n\n";
        
        // 공격력
        display += $"공격력: {stats.GetAttack():F0}\n";
        display += $"  (기본 30 + {stats.GetFlatBonus(StatType.Attack):F0})\n\n";
        
        // 방어력
        display += $"방어력: {stats.GetDefense():F0}\n";
        display += $"  (기본 5 + {stats.GetFlatBonus(StatType.Defense):F0})\n\n";
        
        // 마력
        display += $"마력: {stats.GetMagic():F0}\n";
        display += $"  (기본 20 + {stats.GetFlatBonus(StatType.Magic):F0})\n\n";
        
        display += "=== 이동/공격 ===\n\n";
        
        // 이동속도
        float speedBase = 5f;
        float speedPercent = stats.GetPercentBonus(StatType.Speed);
        display += $"이동속도: {stats.GetSpeed():F2}\n";
        display += $"  (기본 {speedBase} +{speedPercent:F0}%)\n\n";
        
        // 공격속도
        float atkSpdBase = 1f;
        float atkSpdPercent = stats.GetPercentBonus(StatType.AttackSpeed);
        display += $"공격속도: {stats.GetAttackSpeed():F2}\n";
        display += $"  (기본 {atkSpdBase} +{atkSpdPercent:F0}%)\n\n";
        
        // 공격범위
        float rangeBase = 1f;
        float rangePercent = stats.GetPercentBonus(StatType.Range);
        display += $"공격범위: {stats.GetRange():F2}\n";
        display += $"  (기본 {rangeBase} +{rangePercent:F0}%)\n\n";
        
        // 쿨타임 감소
        float cdPercent = stats.GetPercentBonus(StatType.Cooldown);
        display += $"쿨타임 감소: {cdPercent:F0}%\n\n";
        
        display += "=== 기타 ===\n\n";
        display += $"현재 레벨: {LevelUpManager.GetCurrentLevel()}\n";
        display += $"직업: {LevelUpManager.GetCurrentJob()}\n";
        
        statText.text = display;
    }
}