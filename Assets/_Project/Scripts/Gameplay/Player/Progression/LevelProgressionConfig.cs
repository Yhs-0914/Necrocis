using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Balance/Level Progression Config", fileName = "LevelProgressionConfig")]
    public class LevelProgressionConfig : ScriptableObject
    {
        public const string DefaultResourcePath = "Balance/LevelProgressionConfig";

        [Header("Experience")]
        [Min(1)] public int maxLevel = 30;
        [Min(1)] public int baseExp = 30;
        [Min(0)] public int expIncreasePerLevel = 20;
        [Min(0)] public int enemyKillExp = 10;

        [Header("Experience Multipliers")]
        [Min(1)] public int earlyLevelUpperBound = 9;
        [Min(0f)] public float earlyExpGainMultiplier = 2f;
        [Min(0f)] public float jobSelectionBlockedExpMultiplier = 0f;
        [Min(0f)] public float jobSelectionSelectedExpMultiplier = 1f;
        [Min(1)] public int midLevelUpperBound = 20;
        [Min(0f)] public float midExpGainMultiplier = 1f;
        [Min(0f)] public float lateExpGainMultiplier = 0.8f;

        [Header("Unlock Levels")]
        [Min(1)] public int jobSelectionLevel = 5;
        [Min(1)] public int jobBasedChoiceStartLevel = 6;
        [Min(1)] public int skill1UnlockLevel = 5;
        [Min(1)] public int skill2UnlockLevel = 10;

        [Header("Normal Enemy Spawner Population")]
        [Tooltip("잡몹 스포너의 최소/최대 마릿수를 1씩 늘리는 레벨 간격입니다.")]
        [Min(1)] public int normalEnemySpawnLevelInterval = 5;
        [Tooltip("첫 레벨 구간에서 스포너 하나가 유지할 최소 잡몹 수입니다.")]
        [Min(1)] public int normalEnemySpawnMinCount = 1;
        [Tooltip("첫 레벨 구간에서 스포너 하나가 유지할 최대 잡몹 수입니다. 난이도별 maxAlive 배율 대신 이 범위를 사용합니다.")]
        [Min(1)] public int normalEnemySpawnMaxCount = 2;

        [Header("Enemy Health by Player Level")]
        [Min(1)] public int enemyHealthLevelInterval = 5;
        [Tooltip("구간마다 기본 체력에 더하는 비율입니다. 복리로 증가하지 않습니다.")]
        [Min(0f)] public float normalEnemyHealthIncreasePerBand = 0.15f;
        [Min(0f)] public float eliteEnemyHealthIncreasePerBand = 0.12f;

        [Header("Player Base Stats")]
        public List<CharacterStatValue> playerBaseStats = CreateDefaultPlayerBaseStats();

        [Header("Level Up Stat Values")]
        public List<LevelUpStatValueConfig> levelUpStatValues = CreateDefaultLevelUpStatValues();

        [Header("Bio Gamble")]
        public bool bioGambleEnabled = true;
        public int bioGambleMinDelta = 0;
        public int bioGambleMaxDelta = 2;

        public int RollGrowthMultiplier()
        {
            float roll = UnityEngine.Random.value;
            return roll < 0.2f ? 0 : roll < 0.8f ? 1 : 2;
        }

        public int MaxLevel => Mathf.Max(1, maxLevel);
        public int BaseExp => Mathf.Max(1, baseExp);
        public int ExpIncreasePerLevel => Mathf.Max(0, expIncreasePerLevel);
        public int EnemyKillExp => Mathf.Max(0, enemyKillExp);
        public int JobSelectionLevel => Mathf.Max(1, jobSelectionLevel);
        public int JobBasedChoiceStartLevel => Mathf.Max(1, jobBasedChoiceStartLevel);
        public int Skill1UnlockLevel => Mathf.Max(1, skill1UnlockLevel);
        public int Skill2UnlockLevel => Mathf.Max(1, skill2UnlockLevel);

        public int GetRequiredExpForCurrentLevel(int currentLevel)
        {
            int completedLevelUps = Mathf.Max(0, currentLevel - 1);
            return Mathf.Max(1, BaseExp + completedLevelUps * ExpIncreasePerLevel);
        }

        public void GetNormalEnemySpawnCountRange(int currentLevel, out int minCount, out int maxCount)
        {
            int levelBand = (Mathf.Clamp(currentLevel, 1, MaxLevel) - 1) / Mathf.Max(1, normalEnemySpawnLevelInterval);
            int baseMin = Mathf.Max(1, normalEnemySpawnMinCount);
            minCount = baseMin + levelBand;
            maxCount = Mathf.Max(baseMin, normalEnemySpawnMaxCount) + levelBand;
        }

        public float GetEnemyHealthMultiplier(int currentLevel, MonsterTier tier)
        {
            if (tier == MonsterTier.Boss) return 1f;
            int band = (Mathf.Clamp(currentLevel, 1, MaxLevel) - 1) / Mathf.Max(1, enemyHealthLevelInterval);
            float increase = tier == MonsterTier.Elite ? eliteEnemyHealthIncreasePerBand : normalEnemyHealthIncreasePerBand;
            return 1f + band * Mathf.Max(0f, increase);
        }

        public float GetExpGainMultiplier(int currentLevel, JobType currentJob)
        {
            if (currentLevel <= Mathf.Max(1, earlyLevelUpperBound))
            {
                return Mathf.Max(0f, earlyExpGainMultiplier);
            }

            if (currentLevel == JobSelectionLevel)
            {
                return currentJob == JobType.None
                    ? Mathf.Max(0f, jobSelectionBlockedExpMultiplier)
                    : Mathf.Max(0f, jobSelectionSelectedExpMultiplier);
            }

            if (currentLevel <= Mathf.Max(JobSelectionLevel, midLevelUpperBound))
            {
                return Mathf.Max(0f, midExpGainMultiplier);
            }

            return Mathf.Max(0f, lateExpGainMultiplier);
        }

        public int GetSkillUnlockLevel(int skillSlotIndex)
        {
            return skillSlotIndex <= 1 ? Skill1UnlockLevel : Skill2UnlockLevel;
        }

        public CharacterStatValue[] BuildPlayerBaseStats()
        {
            IReadOnlyList<CharacterStatValue> source = playerBaseStats;
            if (source == null || source.Count == 0)
            {
                source = CreateDefaultPlayerBaseStats();
            }

            CharacterStatValue[] stats = new CharacterStatValue[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                stats[i] = source[i];
            }

            return stats;
        }

        public float GetPlayerBaseStat(CharacterStatType statType)
        {
            IReadOnlyList<CharacterStatValue> source = playerBaseStats;
            if (source == null || source.Count == 0)
            {
                source = CreateDefaultPlayerBaseStats();
            }

            for (int i = 0; i < source.Count; i++)
            {
                CharacterStatValue value = source[i];
                if (value.statType == statType)
                {
                    return value.value;
                }
            }

            IReadOnlyList<CharacterStatValue> defaults = CreateDefaultPlayerBaseStats();
            for (int i = 0; i < defaults.Count; i++)
            {
                CharacterStatValue value = defaults[i];
                if (value.statType == statType)
                {
                    return value.value;
                }
            }

            return 0f;
        }

        public Dictionary<LevelUpStatChoice, LevelUpStatEffect> BuildStatEffectMap()
        {
            Dictionary<LevelUpStatChoice, LevelUpStatEffect> effects = new Dictionary<LevelUpStatChoice, LevelUpStatEffect>();
            effects[LevelUpStatChoice.HealthUp] = BuildEffect(CharacterStatType.MaxHealth);
            effects[LevelUpStatChoice.SpeedUp] = BuildEffect(CharacterStatType.MoveSpeed);
            effects[LevelUpStatChoice.AttackPowerUp] = BuildEffect(CharacterStatType.AttackPower);
            effects[LevelUpStatChoice.AttackSpeedRangeUp] = BuildEffect(CharacterStatType.AttackSpeed, CharacterStatType.AttackRange);
            effects[LevelUpStatChoice.MagicUp] = BuildEffect(CharacterStatType.Magic);
            return effects;
        }

        public bool TryGetLevelUpStatValue(CharacterStatType statType, out LevelUpStatValueConfig valueConfig)
        {
            IReadOnlyList<LevelUpStatValueConfig> source = levelUpStatValues;
            if (source == null || source.Count == 0)
            {
                source = CreateDefaultLevelUpStatValues();
            }

            for (int i = 0; i < source.Count; i++)
            {
                LevelUpStatValueConfig entry = source[i];
                if (entry != null && entry.statType == statType)
                {
                    valueConfig = entry;
                    return true;
                }
            }

            valueConfig = null;
            return false;
        }

        public float GetRuntimeModifierValue(LevelUpStatValueConfig valueConfig)
        {
            if (valueConfig == null)
            {
                return 0f;
            }

            return valueConfig.mode == CharacterStatModifierMode.PercentAdd
                ? valueConfig.value / 100f
                : valueConfig.value;
        }

        private LevelUpStatEffect BuildEffect(params CharacterStatType[] statTypes)
        {
            LevelUpStatEffect effect = new LevelUpStatEffect();
            if (statTypes == null)
            {
                return effect;
            }

            for (int i = 0; i < statTypes.Length; i++)
            {
                if (!TryGetLevelUpStatValue(statTypes[i], out LevelUpStatValueConfig valueConfig))
                {
                    continue;
                }

                if (valueConfig.mode == CharacterStatModifierMode.PercentAdd)
                {
                    effect.percentStats[valueConfig.statType] = valueConfig.value;
                }
                else
                {
                    effect.flatStats[valueConfig.statType] = valueConfig.value;
                }
            }

            return effect;
        }

        private static List<CharacterStatValue> CreateDefaultPlayerBaseStats()
        {
            return new List<CharacterStatValue>
            {
                new CharacterStatValue(CharacterStatType.MaxHealth, 10f),
                new CharacterStatValue(CharacterStatType.MoveSpeed, 10f),
                new CharacterStatValue(CharacterStatType.AttackPower, 1f),
                new CharacterStatValue(CharacterStatType.AttackSpeed, 1f),
                new CharacterStatValue(CharacterStatType.AttackRange, 1f),
                new CharacterStatValue(CharacterStatType.Magic, 1f),
                new CharacterStatValue(CharacterStatType.SkillCooldownReduction, 0f)
            };
        }

        private static List<LevelUpStatValueConfig> CreateDefaultLevelUpStatValues()
        {
            return new List<LevelUpStatValueConfig>
            {
                new LevelUpStatValueConfig(CharacterStatType.AttackPower, CharacterStatModifierMode.Flat, 1f),
                new LevelUpStatValueConfig(CharacterStatType.AttackRange, CharacterStatModifierMode.PercentAdd, 10f),
                new LevelUpStatValueConfig(CharacterStatType.AttackSpeed, CharacterStatModifierMode.PercentAdd, 20f),
                new LevelUpStatValueConfig(CharacterStatType.Magic, CharacterStatModifierMode.Flat, 25f),
                new LevelUpStatValueConfig(CharacterStatType.MoveSpeed, CharacterStatModifierMode.PercentAdd, 5f),
                new LevelUpStatValueConfig(CharacterStatType.MaxHealth, CharacterStatModifierMode.Flat, 2f, true)
            };
        }
    }

    [Serializable]
    public class LevelUpStatValueConfig
    {
        public CharacterStatType statType;
        public CharacterStatModifierMode mode;
        public float value;
        public bool healWhenPositive;

        public LevelUpStatValueConfig()
        {
        }

        public LevelUpStatValueConfig(
            CharacterStatType statType,
            CharacterStatModifierMode mode,
            float value,
            bool healWhenPositive = false)
        {
            this.statType = statType;
            this.mode = mode;
            this.value = value;
            this.healWhenPositive = healWhenPositive;
        }
    }
}
