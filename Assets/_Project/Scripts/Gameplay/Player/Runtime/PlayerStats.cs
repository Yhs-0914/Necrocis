using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 플레이어 스탯 싱글톤.
    /// CharacterStats를 백엔드로 사용하며, 레벨업 시스템과 연동.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterStats))]
    public class PlayerStats : MonoBehaviour
    {
        private static PlayerStats instance;

        public static PlayerStats Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<PlayerStats>();
                }

                if (instance != null && !instance.initialized)
                {
                    instance.EnsureInitialized();
                }

                return instance;
            }
            private set => instance = value;
        }

        private CharacterStats runtimeStats; // 실제 스탯 데이터를 관리하는 CharacterStats 컴포넌트
        private bool initialized;             // 기본 스탯 초기화 완료 여부

        // CharacterStats의 이벤트를 외부에 전달 (중계 패턴)
        public event Action<CharacterStats, CharacterStatChangedEventArgs> StatChanged
        {
            add => RuntimeStats.StatChanged += value;     // 구독 시 CharacterStats에 연결
            remove => RuntimeStats.StatChanged -= value;  // 해제 시 CharacterStats에서 분리
        }

        public event Action<CharacterStats, CharacterHealthChangedEventArgs> HealthChanged
        {
            add => RuntimeStats.HealthChanged += value;
            remove => RuntimeStats.HealthChanged -= value;
        }

        // CharacterStats 컴포넌트에 대한 지연 초기화 접근자
        // 없으면 자동으로 추가하여 항상 유효한 참조 보장
        public CharacterStats RuntimeStats
        {
            get
            {
                if (runtimeStats == null)
                {
                    runtimeStats = GetComponent<CharacterStats>();
                    if (runtimeStats == null)
                        runtimeStats = gameObject.AddComponent<CharacterStats>();
                }
                return runtimeStats;
            }
        }
        private void Awake()
        {
            if (instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            EnsureInitialized();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public void EnsureInitialized()
        {
            if (initialized) return;

            ApplyDefaultBaseStats(true);
            initialized = true;
        }

        public void ResetBaseStats(bool resetCurrentHealth = true)
        {
            ApplyDefaultBaseStats(resetCurrentHealth);
            initialized = true;
        }

        private void ApplyDefaultBaseStats(bool resetCurrentHealth)
        {
            CharacterStatValue[] baseStats = LevelUpManager.Config.BuildPlayerBaseStats();
            for (int index = 0; index < baseStats.Length; index++)
            {
                CharacterStatValue value = baseStats[index];
                value.value *= DifficultyBalanceService.GetPlayerBaseStatMultiplier(value.statType);
                baseStats[index] = value;
            }

            RuntimeStats.ConfigureBaseStats(baseStats, resetCurrentHealth);
        }

        public void ConfigureBaseStats(float moveSpeed, float maxHealth, float attackPower, bool resetCurrentHealth = false)
        {
            ConfigureBaseStats(
                moveSpeed,
                maxHealth,
                attackPower,
                LevelUpManager.Config.GetPlayerBaseStat(CharacterStatType.AttackSpeed),
                LevelUpManager.Config.GetPlayerBaseStat(CharacterStatType.AttackRange),
                LevelUpManager.Config.GetPlayerBaseStat(CharacterStatType.Magic),
                LevelUpManager.Config.GetPlayerBaseStat(CharacterStatType.SkillCooldownReduction),
                resetCurrentHealth);
        }

        public void ConfigureBaseStats(
            float moveSpeed,
            float maxHealth,
            float attackPower,
            float attackSpeed,
            float attackRange,
            float magic,
            float skillCooldownReduction,
            bool resetCurrentHealth = false)
        {
            RuntimeStats.ConfigureBaseStats(new CharacterStatValue[]
            {
                new CharacterStatValue(CharacterStatType.MaxHealth, maxHealth),
                new CharacterStatValue(CharacterStatType.MoveSpeed, moveSpeed),
                new CharacterStatValue(CharacterStatType.AttackPower, attackPower),
                new CharacterStatValue(CharacterStatType.AttackSpeed, attackSpeed),
                new CharacterStatValue(CharacterStatType.AttackRange, attackRange),
                new CharacterStatValue(CharacterStatType.Magic, magic),
                new CharacterStatValue(CharacterStatType.SkillCooldownReduction, skillCooldownReduction),
            }, !initialized || resetCurrentHealth);
            initialized = true;
        }

        // ─────────────────────────────────
        // 레벨업 연동
        // ─────────────────────────────────

        // 레벨업 선택지를 실제 모디파이어로 변환하여 적용
        // LevelUpStatCatalog에서 선택지의 효과(고정값/퍼센트)를 가져와서 CharacterStats에 추가
        public void ApplyLevelUpStatChoice(LevelUpStatChoice choice)
        {
            EnsureInitialized();
            LevelUpStatEffect effect = LevelUpStatCatalog.GetStatEffect(choice);
            float previousMaxHealth = RuntimeStats.MaxHealth;

            // 고정값 모디파이어 적용 (예: 공격력 +3)
            foreach (var stat in effect.flatStats)
            {
                RuntimeStats.AddModifier(
                    stat.Key,
                    stat.Value,
                    CharacterStatModifierMode.Flat,
                    choice); // source를 choice로 설정하여 추적 가능
                LevelUpManager.RecordResolvedModifier(
                    stat.Key,
                    stat.Value,
                    CharacterStatModifierMode.Flat);
            }

            // 퍼센트 모디파이어 적용 (예: 이동속도 +3% → 0.03으로 변환)
            foreach (var stat in effect.percentStats)
            {
                RuntimeStats.AddModifier(
                    stat.Key,
                    stat.Value / 100f, // UI에선 3%로 표시, 내부적으론 0.03
                    CharacterStatModifierMode.PercentAdd,
                    choice);
                LevelUpManager.RecordResolvedModifier(
                    stat.Key,
                    stat.Value / 100f,
                    CharacterStatModifierMode.PercentAdd);
            }

            float maxHealthIncrease = RuntimeStats.MaxHealth - previousMaxHealth;
            if (maxHealthIncrease > 0f)
            {
                RuntimeStats.RestoreHealth(maxHealthIncrease);
            }
        }

        public void ResetStats()
        {
            RuntimeStats.ClearModifiers();
        }

        // ─────────────────────────────────
        // 프로퍼티 (PlayerController 호환)
        // ─────────────────────────────────

        public float MoveSpeed => RuntimeStats.MoveSpeed;
        public float MaxHealth => RuntimeStats.MaxHealth;
        public float CurrentHealth => RuntimeStats.CurrentHealth;
        public float AttackPower => RuntimeStats.AttackPower;
        public float AttackSpeed => RuntimeStats.AttackSpeed;
        public float AttackRange => RuntimeStats.AttackRange;
        public float Magic => RuntimeStats.Magic;
        public float SkillCooldownReduction => RuntimeStats.SkillCooldownReduction;
        public bool IsDead => RuntimeStats.IsDead;

        // ─────────────────────────────────
        // 편의 게터 (기존 코드 호환)
        // ─────────────────────────────────

        public float GetHealth() => RuntimeStats.MaxHealth;
        public float GetAttack() => RuntimeStats.AttackPower;
        [Obsolete("Player defense is no longer part of the base stat set.")]
        public float GetDefense() => 0f;
        public float GetSpeed() => RuntimeStats.MoveSpeed;
        public float GetAttackSpeed() => RuntimeStats.AttackSpeed;
        public float GetRange() => RuntimeStats.AttackRange;
        public float GetMagic() => RuntimeStats.Magic;
        public float GetCooldown() => RuntimeStats.SkillCooldownReduction;

        // ─────────────────────────────────
        // HP 위임
        // ─────────────────────────────────

        public void TakeDamage(float damage) => RuntimeStats.ApplyDamage(damage);
        public void Heal(float amount) => RuntimeStats.RestoreHealth(amount);

        // ─────────────────────────────────
        // 모디파이어 API (아이템/버프용)
        // ─────────────────────────────────

        public void ApplyModifier(CharacterStatModifier modifier)
        {
            EnsureInitialized();
            RuntimeStats.AddModifier(modifier);
        }

        public void ApplyModifiers(IEnumerable<CharacterStatModifierData> modifiers, object source)
        {
            EnsureInitialized();
            if (modifiers == null) return;
            foreach (CharacterStatModifierData modifier in modifiers)
                RuntimeStats.AddModifier(modifier.ToModifier(source));
        }

        public void ApplyPlayerItemStatModifiers(IEnumerable<PlayerItemStatModifierData> modifiers, object source)
        {
            EnsureInitialized();
            if (modifiers == null) return;
            foreach (PlayerItemStatModifierData modifier in modifiers)
                RuntimeStats.AddModifier(modifier.ToModifier(source));
        }

        // 특정 출처의 모디파이어를 새 것으로 교체 (기존 제거 → 새로 적용)
        // 장비 교체 시 유용: 이전 장비 효과 제거 후 새 장비 효과 적용
        public void ApplyOrReplaceSourceModifiers(IEnumerable<CharacterStatModifierData> modifiers, object source)
        {
            EnsureInitialized();
            RuntimeStats.RemoveModifiersFromSource(source);
            ApplyModifiers(modifiers, source);
        }

        public int RemoveModifiersFromSource(object source)
        {
            EnsureInitialized();
            return RuntimeStats.RemoveModifiersFromSource(source);
        }
    }
}
