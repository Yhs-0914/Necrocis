using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    // 캐릭터 스탯 종류 열거형
    public enum CharacterStatType
    {
        MaxHealth = 0,               // 최대 체력
        MoveSpeed = 1,               // 이동 속도
        AttackPower = 2,             // 기본공격 공격력
        [Obsolete("Player defense is no longer part of the base stat set.")]
        Defense = 3,
        AttackSpeed = 4,             // 기본공격 공격 속도
        AttackRange = 5,             // 기본공격 사거리
        Magic = 6,                   // 스킬 데미지 증가율(%)
        SkillCooldownReduction = 7,  // 스킬 쿨타임 감소율(%)

        [Obsolete("Use AttackRange instead.")]
        Range = AttackRange,
        [Obsolete("Use SkillCooldownReduction instead.")]
        Cooldown = SkillCooldownReduction
    }

    // 모디파이어 적용 방식 (적용 순서: Flat → PercentAdd → PercentMultiply)
    public enum CharacterStatModifierMode
    {
        Flat,             // 고정값 덧셈 (예: +10)
        PercentAdd,       // 기본값 기준 퍼센트 합산 (예: +10%, +5% → 총 +15%)
        PercentMultiply   // 최종값에 곱셈 (예: ×1.1)
    }

    // 스탯 기본값을 저장하는 구조체 (초기화 시 사용)
    [Serializable]
    public struct CharacterStatValue
    {
        public CharacterStatType statType; // 스탯 종류
        public float value;                // 기본값

        public CharacterStatValue(CharacterStatType statType, float value)
        {
            this.statType = statType;
            this.value = value;
        }
    }

    // 모디파이어 직렬화용 데이터 (인스펙터/아이템 설정에서 사용)
    [Serializable]
    public struct CharacterStatModifierData
    {
        public CharacterStatType statType; // 대상 스탯
        public float value;                // 수치
        public CharacterStatModifierMode mode; // 적용 방식

        public CharacterStatModifierData(CharacterStatType statType, float value, CharacterStatModifierMode mode)
        {
            this.statType = statType;
            this.value = value;
            this.mode = mode;
        }

        // 런타임 모디파이어로 변환 (source = 누가 적용했는지 추적용)
        public CharacterStatModifier ToModifier(object source)
        {
            return new CharacterStatModifier(statType, value, mode, source);
        }
    }

    // 런타임 모디파이어 (불변 구조체, source로 출처 추적 → 제거 시 사용)
    public readonly struct CharacterStatModifier
    {
        public CharacterStatType StatType { get; }  // 대상 스탯
        public float Value { get; }                  // 수치
        public CharacterStatModifierMode Mode { get; } // 적용 방식
        public object Source { get; }                // 출처 (아이템, 레벨업 등)

        public CharacterStatModifier(
            CharacterStatType statType,
            float value,
            CharacterStatModifierMode mode = CharacterStatModifierMode.Flat,
            object source = null)
        {
            StatType = statType;
            Value = value;
            Mode = mode;
            Source = source;
        }
    }

    // 스탯 변경 이벤트 인자 (어떤 스탯이 얼마에서 얼마로 바뀌었는지)
    public readonly struct CharacterStatChangedEventArgs
    {
        public CharacterStatType StatType { get; }  // 변경된 스탯
        public float PreviousValue { get; }          // 이전 값
        public float CurrentValue { get; }           // 현재 값

        public CharacterStatChangedEventArgs(CharacterStatType statType, float previousValue, float currentValue)
        {
            StatType = statType;
            PreviousValue = previousValue;
            CurrentValue = currentValue;
        }
    }

    // HP 변경 이벤트 인자 (현재HP, 최대HP의 이전/현재 값)
    public readonly struct CharacterHealthChangedEventArgs
    {
        public float PreviousValue { get; }      // 이전 HP
        public float CurrentValue { get; }       // 현재 HP
        public float PreviousMaxValue { get; }   // 이전 최대HP
        public float MaxValue { get; }           // 현재 최대HP
        public float NormalizedCurrent => MaxValue <= 0f ? 0f : CurrentValue / MaxValue; // 0~1 정규화 HP

        public CharacterHealthChangedEventArgs(float previousValue, float currentValue, float previousMaxValue, float maxValue)
        {
            PreviousValue = previousValue;
            CurrentValue = currentValue;
            PreviousMaxValue = previousMaxValue;
            MaxValue = maxValue;
        }
    }

    /// <summary>
    /// 공통 캐릭터 스탯 런타임 컨테이너.
    /// UI, 아이템, 레벨업 시스템은 이 컴포넌트만 구독/조작하면 된다.
    /// </summary>
    public class CharacterStats : MonoBehaviour
    {
        private const float HealthUnitEpsilon = 0.0001f;

        private readonly Dictionary<CharacterStatType, float> baseStatValues = new Dictionary<CharacterStatType, float>();   // 기본 스탯값
        private readonly Dictionary<CharacterStatType, float> finalStatValues = new Dictionary<CharacterStatType, float>(); // 모디파이어 적용 후 최종값
        private readonly List<CharacterStatModifier> modifiers = new List<CharacterStatModifier>(); // 활성 모디파이어 목록

        private float currentHealth; // 현재 HP (최대HP와 별도 관리)

        public event Action<CharacterStats, CharacterStatChangedEventArgs> StatChanged;   // 스탯 변경 이벤트
        public event Action<CharacterStats, CharacterHealthChangedEventArgs> HealthChanged; // HP 변경 이벤트

        public float MaxHealth => ToHealthUnits(GetValue(CharacterStatType.MaxHealth));
        public float MoveSpeed => GetValue(CharacterStatType.MoveSpeed);
        public float AttackPower => GetValue(CharacterStatType.AttackPower);
        public float AttackSpeed => GetValue(CharacterStatType.AttackSpeed);
        public float AttackRange => GetValue(CharacterStatType.AttackRange);
        public float Magic => GetValue(CharacterStatType.Magic);
        public float SkillCooldownReduction => GetValue(CharacterStatType.SkillCooldownReduction);
        [Obsolete("Player defense is no longer part of the base stat set.")]
        public float Defense => 0f;
        [Obsolete("Use AttackRange instead.")]
        public float Range => AttackRange;
        [Obsolete("Use SkillCooldownReduction instead.")]
        public float Cooldown => SkillCooldownReduction;
        public float CurrentHealth => currentHealth;                                        // 현재 HP
        public float HealthNormalized => MaxHealth <= 0f ? 0f : currentHealth / MaxHealth; // HP 비율 (0~1)
        public bool IsDead => currentHealth <= 0f;                                         // 사망 여부

        // 기본 스탯을 설정하고 전체 재계산 (resetCurrentHealth=true면 HP를 최대로 초기화)
        public void ConfigureBaseStats(IEnumerable<CharacterStatValue> baseStats, bool resetCurrentHealth = true)
        {
            float previousCurrentHealth = currentHealth;
            float previousMaxHealth = MaxHealth;

            baseStatValues.Clear();
            if (baseStats != null)
            {
                foreach (CharacterStatValue stat in baseStats)
                {
                    baseStatValues[stat.statType] = stat.value;
                }
            }

            RecalculateAllStats();

            if (resetCurrentHealth)
            {
                currentHealth = MaxHealth;
            }
            else
            {
                currentHealth = ClampHealthUnits(currentHealth);
            }

            NotifyHealthChanged(previousCurrentHealth, previousMaxHealth, true);
        }

        public void ConfigureBaseStats(params CharacterStatValue[] baseStats)
        {
            ConfigureBaseStats((IEnumerable<CharacterStatValue>)baseStats, true);
        }

        // 특정 스탯의 기본값을 개별적으로 변경
        public void SetBaseStat(CharacterStatType statType, float value, bool restoreHealthToMax = false)
        {
            float previousCurrentHealth = currentHealth;
            float previousMaxHealth = MaxHealth;

            baseStatValues[statType] = value;
            RecalculateAllStats();

            if (statType == CharacterStatType.MaxHealth)
            {
                currentHealth = restoreHealthToMax ? MaxHealth : ClampHealthUnits(currentHealth);
                NotifyHealthChanged(previousCurrentHealth, previousMaxHealth, true);
            }
        }

        // 모디파이어 적용 전 기본값 반환
        public float GetBaseValue(CharacterStatType statType)
        {
            return baseStatValues.TryGetValue(statType, out float value) ? value : 0f;
        }

        // 모디파이어 적용 후 최종값 반환
        public float GetValue(CharacterStatType statType)
        {
            return finalStatValues.TryGetValue(statType, out float value) ? value : 0f;
        }

        public bool TryGetValue(CharacterStatType statType, out float value)
        {
            return finalStatValues.TryGetValue(statType, out value);
        }

        // 현재 최종 스탯값의 스냅샷 생성 (저장/비교용)
        public CharacterStatValue[] CreateSnapshot()
        {
            CharacterStatValue[] snapshot = new CharacterStatValue[finalStatValues.Count];
            int index = 0;
            foreach (KeyValuePair<CharacterStatType, float> pair in finalStatValues)
            {
                snapshot[index++] = new CharacterStatValue(pair.Key, pair.Value);
            }
            return snapshot;
        }

        // 모디파이어 추가 → 전체 재계산 → HP 클램핑
        public void AddModifier(CharacterStatModifier modifier)
        {
            float previousCurrentHealth = currentHealth;
            float previousMaxHealth = MaxHealth;

            modifiers.Add(modifier);
            RecalculateAllStats();
            ClampHealthAfterStatRefresh(previousCurrentHealth, previousMaxHealth);
        }

        public void AddModifier(
            CharacterStatType statType,
            float value,
            CharacterStatModifierMode mode = CharacterStatModifierMode.Flat,
            object source = null)
        {
            AddModifier(new CharacterStatModifier(statType, value, mode, source));
        }

        // 특정 출처(아이템/버프)의 모든 모디파이어 제거
        public int RemoveModifiersFromSource(object source)
        {
            if (source == null)
            {
                return 0;
            }

            float previousCurrentHealth = currentHealth;
            float previousMaxHealth = MaxHealth;

            int removedCount = modifiers.RemoveAll(modifier => Equals(modifier.Source, source));
            if (removedCount <= 0)
            {
                return 0;
            }

            RecalculateAllStats();
            ClampHealthAfterStatRefresh(previousCurrentHealth, previousMaxHealth);
            return removedCount;
        }

        // 모든 모디파이어 초기화 (레벨업 리셋 등)
        public void ClearModifiers()
        {
            if (modifiers.Count == 0)
            {
                return;
            }

            float previousCurrentHealth = currentHealth;
            float previousMaxHealth = MaxHealth;

            modifiers.Clear();
            RecalculateAllStats();
            ClampHealthAfterStatRefresh(previousCurrentHealth, previousMaxHealth);
        }

        // 데미지 적용 (HP 감소, 실제 받은 데미지 반환)
        public float ApplyDamage(float damage)
        {
            if (damage <= 0f || MaxHealth <= 0f)
            {
                return 0f;
            }

            float damageUnits = ToHealthDeltaUnits(damage);
            float previousCurrentHealth = ToHealthUnits(currentHealth);
            currentHealth = Mathf.Max(0f, previousCurrentHealth - damageUnits);
            NotifyHealthChanged(previousCurrentHealth, MaxHealth, false);
            return previousCurrentHealth - currentHealth;
        }

        // HP 회복 (최대HP 초과 불가, 실제 회복량 반환)
        public float RestoreHealth(float amount)
        {
            if (amount <= 0f || MaxHealth <= 0f)
            {
                return 0f;
            }

            float healUnits = ToHealthDeltaUnits(amount);
            float previousCurrentHealth = ToHealthUnits(currentHealth);
            currentHealth = Mathf.Min(MaxHealth, previousCurrentHealth + healUnits);
            NotifyHealthChanged(previousCurrentHealth, MaxHealth, false);
            return currentHealth - previousCurrentHealth;
        }

        // HP를 최대로 완전 회복
        public void ResetHealthToMax()
        {
            float previousCurrentHealth = currentHealth;
            currentHealth = MaxHealth;
            NotifyHealthChanged(previousCurrentHealth, MaxHealth, true);
        }

        // 스탯 재계산 후 HP가 최대HP를 초과하지 않도록 클램핑
        private void ClampHealthAfterStatRefresh(float previousCurrentHealth, float previousMaxHealth)
        {
            currentHealth = ClampHealthUnits(currentHealth);
            NotifyHealthChanged(previousCurrentHealth, previousMaxHealth, false);
        }

        private float ClampHealthUnits(float value)
        {
            return Mathf.Clamp(ToHealthUnits(value), 0f, MaxHealth);
        }

        private static float ToHealthUnits(float value)
        {
            if (value <= 0f)
            {
                return 0f;
            }

            return Mathf.Floor(value + 0.5f + HealthUnitEpsilon);
        }

        private static float ToHealthDeltaUnits(float value)
        {
            if (value <= 0f)
            {
                return 0f;
            }

            return Mathf.Max(1f, Mathf.Ceil(value - HealthUnitEpsilon));
        }

        // HP 변경 이벤트 발행 (force=true면 값 변화 없어도 강제 발행)
        private void NotifyHealthChanged(float previousCurrentHealth, float previousMaxHealth, bool force)
        {
            float currentMaxHealth = MaxHealth;
            if (!force
                && Mathf.Approximately(previousCurrentHealth, currentHealth)
                && Mathf.Approximately(previousMaxHealth, currentMaxHealth))
            {
                return;
            }

            HealthChanged?.Invoke(
                this,
                new CharacterHealthChangedEventArgs(previousCurrentHealth, currentHealth, previousMaxHealth, currentMaxHealth));
        }

        // 모든 스탯을 기본값 + 모디파이어로 재계산하고, 변경된 스탯에 대해 이벤트 발행
        private void RecalculateAllStats()
        {
            Dictionary<CharacterStatType, float> previousValues = new Dictionary<CharacterStatType, float>(finalStatValues);
            HashSet<CharacterStatType> statTypes = new HashSet<CharacterStatType>();

            foreach (KeyValuePair<CharacterStatType, float> pair in baseStatValues)
            {
                statTypes.Add(pair.Key);
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                statTypes.Add(modifiers[i].StatType);
            }

            finalStatValues.Clear();

            foreach (CharacterStatType statType in statTypes)
            {
                float currentValue = EvaluateStat(statType);
                finalStatValues[statType] = currentValue;

                float previousValue = previousValues.TryGetValue(statType, out float cachedValue) ? cachedValue : 0f;
                if (!Mathf.Approximately(previousValue, currentValue))
                {
                    StatChanged?.Invoke(this, new CharacterStatChangedEventArgs(statType, previousValue, currentValue));
                }
            }

            foreach (KeyValuePair<CharacterStatType, float> pair in previousValues)
            {
                if (finalStatValues.ContainsKey(pair.Key) || Mathf.Approximately(pair.Value, 0f))
                {
                    continue;
                }

                StatChanged?.Invoke(this, new CharacterStatChangedEventArgs(pair.Key, pair.Value, 0f));
            }
        }

        // 단일 스탯의 최종값 계산: 기본값 → Flat 합산 → PercentAdd 합산 → PercentMultiply 곱셈
        private float EvaluateStat(CharacterStatType statType)
        {
            float value = GetBaseValue(statType);    // 1단계: 기본값
            float additivePercent = 0f;              // PercentAdd 합계 (여러 개가 합산됨)
            float multiplicativePercent = 1f;        // PercentMultiply 누적곱

            for (int i = 0; i < modifiers.Count; i++)
            {
                CharacterStatModifier modifier = modifiers[i];
                if (modifier.StatType != statType)
                {
                    continue;
                }

                switch (modifier.Mode)
                {
                    case CharacterStatModifierMode.Flat:          // 2단계: 고정값 합산
                        value += modifier.Value;
                        break;
                    case CharacterStatModifierMode.PercentAdd:    // 3단계: 퍼센트 합산 (나중에 한번에 적용)
                        additivePercent += modifier.Value;
                        break;
                    case CharacterStatModifierMode.PercentMultiply: // 4단계: 곱셈 누적 (나중에 한번에 적용)
                        multiplicativePercent *= 1f + modifier.Value;
                        break;
                }
            }

            value *= 1f + additivePercent;    // PercentAdd 적용 (예: 기본100, +10% → 110)
            value *= multiplicativePercent;  // PercentMultiply 적용 (예: 110 × 1.2 → 132)
            return Mathf.Max(0f, value);     // 음수 방지
        }
    }
}
