using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    // ========================================
    // 싱글톤
    // ========================================
    
    public static PlayerStats Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        InitializeStats();
    }
    
    // ========================================
    // 기본 스탯 (불변)
    // ========================================
    
    private const float BASE_HEALTH = 100f;
    private const float BASE_ATTACK = 30f;
    private const float BASE_DEFENSE = 5f;
    private const float BASE_SPEED = 5f;
    private const float BASE_ATTACK_SPEED = 1f;
    private const float BASE_RANGE = 1f;
    private const float BASE_MAGIC = 20f;
    
    // ========================================
    // 증가량 저장 (고정)
    // ========================================
    
    private Dictionary<StatType, float> flatBonus = new Dictionary<StatType, float>();
    
    // ========================================
    // 증가량 저장 (비율)
    // ========================================
    
    private Dictionary<StatType, float> percentBonus = new Dictionary<StatType, float>();
    
    // ========================================
    // 초기화
    // ========================================
    
    private void InitializeStats()
    {
        // 모든 StatType에 대해 초기화
        foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
        {
            flatBonus[statType] = 0f;
            percentBonus[statType] = 0f;
        }
    }
    
    // ========================================
    // 스탯 적용 (외부에서 호출)
    // ========================================
    
    public void ApplyStatChoice(StatChoice choice)
    {
        // StatManager에서 효과 가져오기
        StatEffect effect = StatManager.GetStatEffect(choice);
        
        // 고정 수치 적용
        foreach (var stat in effect.flatStats)
        {
            flatBonus[stat.Key] += stat.Value;
        }
        
        // 비율 수치 적용 (가산 방식)
        foreach (var stat in effect.percentStats)
        {
            percentBonus[stat.Key] += stat.Value;
        }
    }
    
    // ========================================
    // 최종 스탯 계산 (외부에서 사용)
    // ========================================
    
    public float GetHealth()
    {
        return CalculateFinalStat(BASE_HEALTH, StatType.Health);
    }
    
    public float GetAttack()
    {
        return CalculateFinalStat(BASE_ATTACK, StatType.Attack);
    }
    
    public float GetDefense()
    {
        return CalculateFinalStat(BASE_DEFENSE, StatType.Defense);
    }
    
    public float GetSpeed()
    {
        return CalculateFinalStat(BASE_SPEED, StatType.Speed);
    }
    
    public float GetAttackSpeed()
    {
        return CalculateFinalStat(BASE_ATTACK_SPEED, StatType.AttackSpeed);
    }
    
    public float GetRange()
    {
        return CalculateFinalStat(BASE_RANGE, StatType.Range);
    }
    
    public float GetMagic()
    {
        return CalculateFinalStat(BASE_MAGIC, StatType.Magic);
    }
    
    // ========================================
    // 스탯 계산 (고정 먼저 → 비율 나중)
    // ========================================
    
    private float CalculateFinalStat(float baseStat, StatType statType)
    {
        // 1단계: 기본값 + 고정 증가량
        float withFlat = baseStat + flatBonus[statType];
        
        // 2단계: 비율 적용 (가산)
        float percentMultiplier = 1f + (percentBonus[statType] / 100f);
        
        // 최종값
        return withFlat * percentMultiplier;
    }
    
    // ========================================
    // 스탯 초기화 (전직 시 사용)
    // ========================================
    
    public void ResetStats()
    {
        foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
        {
            flatBonus[statType] = 0f;
            percentBonus[statType] = 0f;
        }
    }
    
    // ========================================
    // 디버깅용 (증가량 확인)
    // ========================================
    
    public float GetFlatBonus(StatType statType)
    {
        return flatBonus[statType];
    }
    
    public float GetPercentBonus(StatType statType)
    {
        return percentBonus[statType];
    }
}
