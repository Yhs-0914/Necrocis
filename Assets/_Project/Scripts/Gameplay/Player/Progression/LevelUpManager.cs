using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Necrocis
{
    // 직업 종류 (레벨 10에서 선택)
    public enum JobType
    {
        None,    // 미선택
        Warrior, // 전사: 공격력/방어력 특화
        Mage,    // 마법사: 마력/쿨타임 특화
        Archer   // 궁수: 공격속도/사거리 특화
    }

    /// <summary>
    /// 경험치, 레벨업, 직업 선택을 관리하는 정적 클래스.
    /// 적 처치 → AddExp() → 레벨업 시 OnLevelUp 이벤트 → LevelUpUI에서 스탯 선택.
    /// </summary>
    public static class LevelUpManager
    {
        private static int currentLevel = 1;   // 현재 레벨
        private static int currentExp = 0;     // 현재 누적 경험치
        private static int expRequired = 100;  // 다음 레벨까지 필요 경험치

        private static LevelProgressionConfig progressionConfig;
        private static LevelProgressionConfig expRequirementConfig;
        private static bool expRequirementInitialized;

        public static Action OnLevelUp;       // 레벨업 이벤트 (LevelUpUI가 구독)
        public static Action OnJobSelect;     // 직업 선택 이벤트 (config의 jobSelectionLevel에서 발생)
        public static Action<JobType> OnJobChanged; // 직업 확정 이벤트 (전직 완료 시 발생)
        public static Action<int> OnExpGained; // 경험치 획득 이벤트 (ExpBarUI가 구독)

        // 경험치 추가 (레벨별 배율 적용 후 누적)
        public static void AddExp(int baseAmount)
        {
            EnsureExpRequirementInitialized();
            if (currentLevel >= Config.MaxLevel) return;

            float multiplier = GetExpMultiplier();                     // 레벨 구간별 경험치 배율
            int actualExp = Mathf.RoundToInt(baseAmount * multiplier); // 실제 획득 경험치

            currentExp += actualExp;
            OnExpGained?.Invoke(actualExp);

            CheckLevelUp();
        }

        // 이전 호출부 호환용: 전역 기본 처치 경험치를 지급합니다.
        public static void AddEnemyKillExp()
        {
            AddEnemyKillExp(Config.EnemyKillExp);
        }

        // 적 설정에 지정된 개별 경험치를 지급합니다.
        public static void AddEnemyKillExp(int amount)
        {
            AddEnemyKillExp(amount, 1f);
        }

        // 적별 보상값과 난이도별 경험치 배율을 함께 적용합니다.
        public static void AddEnemyKillExp(int amount, float rewardMultiplier)
        {
            EnsureExpRequirementInitialized();
            if (currentLevel >= Config.MaxLevel) return;
            if (IsWaitingForJobSelection()) return;

            int enemyKillExp = Mathf.Max(
                0,
                Mathf.RoundToInt(Mathf.Max(0, amount) * Mathf.Max(0f, rewardMultiplier)));
            if (enemyKillExp == 0) return;

            currentExp += enemyKillExp;
            OnExpGained?.Invoke(enemyKillExp);

            CheckLevelUp();
        }

        private static float GetExpMultiplier()
        {
            return Config.GetExpGainMultiplier(currentLevel, currentJob);
        }

        private static int pendingLevelUps; // 대기 중인 레벨업 수 (한번에 여러 레벨 오를 때)

        // 레벨업 가능 여부 확인 (한번에 여러 레벨 오를 수 있으므로 while 사용)
        private static void CheckLevelUp()
        {
            EnsureExpRequirementInitialized();
            while (currentExp >= expRequired && currentLevel < Config.MaxLevel)
            {
                currentExp -= expRequired;
                currentLevel++;
                CalculateExpRequired();
                pendingLevelUps++;
            }

            if (pendingLevelUps > 0)
            {
                pendingLevelUps--;
                if (IsWaitingForJobSelection())
                    OnJobSelect?.Invoke();
                else
                {
                    AudioManager.Instance?.PlaySFX("LevelUp"); // [Sound] 레벨업
                    OnLevelUp?.Invoke();
                }
            }
        }

        public static bool HasPendingLevelUp()
        {
            return pendingLevelUps > 0;
        }

        public static void ProcessNextPendingLevelUp()
        {
            if (pendingLevelUps > 0)
            {
                pendingLevelUps--;
                if (IsWaitingForJobSelection())
                    OnJobSelect?.Invoke();
                else
                {
                    AudioManager.Instance?.PlaySFX("LevelUp");
                    OnLevelUp?.Invoke();
                }
            }
        }

        private static void CalculateExpRequired()
        {
            expRequired = Config.GetRequiredExpForCurrentLevel(currentLevel);
            expRequirementConfig = Config;
            expRequirementInitialized = true;
        }

        public static void DebugLevelUp()
        {
            EnsureExpRequirementInitialized();
            if (currentLevel >= Config.MaxLevel) return;
            currentLevel++;
            CalculateExpRequired();
            if (IsWaitingForJobSelection())
                OnJobSelect?.Invoke();
            else
            {
                AudioManager.Instance?.PlaySFX("LevelUp");
                OnLevelUp?.Invoke();
            }
        }

        public static int GetCurrentLevel() => currentLevel;
        public static int GetCurrentExp() => currentExp;
        public static int GetExpRequired()
        {
            EnsureExpRequirementInitialized();
            return expRequired;
        }
        public static float GetExpProgress()
        {
            EnsureExpRequirementInitialized();
            return (float)currentExp / expRequired;
        }
        public static int GetJobSelectionLevel() => Config.JobSelectionLevel;
        public static int GetSkillUnlockLevel(int skillSlotIndex) => Config.GetSkillUnlockLevel(skillSlotIndex);
        public static bool IsSkillUnlocked(int skillSlotIndex)
        {
            return currentJob != JobType.None
                && currentLevel >= GetSkillUnlockLevel(skillSlotIndex);
        }

        // ─────────────────────────────────
        // 직업 시스템
        // ─────────────────────────────────

        private static JobType currentJob = JobType.None;                        // 현재 선택한 직업
        private static List<LevelUpStatChoice> selectionHistory = new List<LevelUpStatChoice>(); // 지금까지 선택한 스탯 기록
        private static readonly List<SavedStatModifierData> resolvedModifiers = new List<SavedStatModifierData>();

        public static readonly object RestoredModifierSource = new object();

        // 직업별 고유 스탯 매핑 (레벨 11+ 선택지에서 1번째로 고정 등장)
        private static Dictionary<JobType, LevelUpStatChoice> jobStatMap = new Dictionary<JobType, LevelUpStatChoice>
        {
            [JobType.Warrior] = LevelUpStatChoice.AttackPowerUp,     // 전사 → 공격력
            [JobType.Mage] = LevelUpStatChoice.MagicUp,              // 마법사 → 마력
            [JobType.Archer] = LevelUpStatChoice.AttackSpeedRangeUp  // 궁수 → 공격속도/사거리
        };

        public static List<LevelUpStatChoice> GetRandomChoices()
        {
            if (currentLevel >= Config.JobBasedChoiceStartLevel && currentJob != JobType.None)
                return GetJobBasedChoices();

            return GetRandomFourChoices();
        }

        private static List<LevelUpStatChoice> GetRandomFourChoices()
        {
            List<LevelUpStatChoice> allChoices = Enum.GetValues(typeof(LevelUpStatChoice))
                                               .Cast<LevelUpStatChoice>()
                                               .ToList();
            Shuffle(allChoices);
            return allChoices.Take(4).ToList();
        }

        private static List<LevelUpStatChoice> GetJobBasedChoices()
        {
            List<LevelUpStatChoice> result = new List<LevelUpStatChoice>();

            LevelUpStatChoice jobStat = jobStatMap[currentJob];
            result.Add(jobStat);

            LevelUpStatChoice mostSelected = GetMostSelectedStat(exclude: jobStat);
            result.Add(mostSelected);

            List<LevelUpStatChoice> remaining = GetRemainingChoices(result);
            Shuffle(remaining);
            result.Add(remaining[0]);

            remaining = GetRemainingChoices(result);
            Shuffle(remaining);
            result.Add(remaining[0]);

            return result;
        }

        private static LevelUpStatChoice GetMostSelectedStat(LevelUpStatChoice exclude)
        {
            Dictionary<LevelUpStatChoice, int> counts = new Dictionary<LevelUpStatChoice, int>();

            foreach (LevelUpStatChoice choice in selectionHistory)
            {
                if (choice == exclude) continue;
                if (!counts.ContainsKey(choice))
                    counts[choice] = 0;
                counts[choice]++;
            }

            int maxCount = 0;
            List<LevelUpStatChoice> mostSelectedList = new List<LevelUpStatChoice>();

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
                return mostSelectedList[UnityEngine.Random.Range(0, mostSelectedList.Count)];

            List<LevelUpStatChoice> allChoices = Enum.GetValues(typeof(LevelUpStatChoice))
                                               .Cast<LevelUpStatChoice>()
                                               .Where(c => c != exclude)
                                               .ToList();
            return allChoices[UnityEngine.Random.Range(0, allChoices.Count)];
        }

        private static List<LevelUpStatChoice> GetRemainingChoices(List<LevelUpStatChoice> alreadySelected)
        {
            return Enum.GetValues(typeof(LevelUpStatChoice))
                        .Cast<LevelUpStatChoice>()
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

        public static void RecordSelection(LevelUpStatChoice choice) => selectionHistory.Add(choice);
        public static void RecordResolvedModifier(
            CharacterStatType statType,
            float value,
            CharacterStatModifierMode mode)
        {
            resolvedModifiers.Add(new SavedStatModifierData
            {
                statType = statType,
                value = value,
                mode = mode
            });
        }

        public static void CaptureProgress(PlayerProgressSaveData target)
        {
            if (target == null)
            {
                return;
            }

            target.level = Mathf.Clamp(currentLevel, 1, Config.MaxLevel);
            target.experience = Mathf.Max(0, currentExp);
            target.job = currentJob;
            target.selectionHistory = new List<LevelUpStatChoice>(selectionHistory);
            target.levelUpModifiers = new List<SavedStatModifierData>(resolvedModifiers.Count);
            for (int i = 0; i < resolvedModifiers.Count; i++)
            {
                SavedStatModifierData source = resolvedModifiers[i];
                target.levelUpModifiers.Add(new SavedStatModifierData
                {
                    statType = source.statType,
                    value = source.value,
                    mode = source.mode
                });
            }
        }

        public static void RestoreProgress(PlayerProgressSaveData source)
        {
            source ??= new PlayerProgressSaveData();
            currentLevel = Mathf.Clamp(source.level, 1, Config.MaxLevel);
            currentExp = Mathf.Max(0, source.experience);
            pendingLevelUps = 0;
            CalculateExpRequired();
            if (currentLevel < Config.MaxLevel)
            {
                currentExp = Mathf.Min(currentExp, Mathf.Max(0, expRequired - 1));
            }
            else
            {
                currentExp = 0;
            }

            selectionHistory.Clear();
            if (source.selectionHistory != null)
            {
                selectionHistory.AddRange(source.selectionHistory);
            }

            resolvedModifiers.Clear();
            if (source.levelUpModifiers != null)
            {
                for (int i = 0; i < source.levelUpModifiers.Count; i++)
                {
                    SavedStatModifierData modifier = source.levelUpModifiers[i];
                    if (modifier == null)
                    {
                        continue;
                    }

                    resolvedModifiers.Add(new SavedStatModifierData
                    {
                        statType = modifier.statType,
                        value = modifier.value,
                        mode = modifier.mode
                    });
                }
            }

            bool jobChanged = currentJob != source.job;
            currentJob = source.job;
            if (jobChanged)
            {
                OnJobChanged?.Invoke(currentJob);
            }

            OnExpGained?.Invoke(0);
        }

        public static void ResetProgress()
        {
            RestoreProgress(new PlayerProgressSaveData());
        }

        public static void SetJob(JobType job)
        {
            if (job == JobType.None || currentJob == job)
            {
                return;
            }

            currentJob = job;
            AudioManager.Instance?.PlaySFX("JobSelect");
            OnJobChanged?.Invoke(currentJob);
        }
        public static void ResetSelectionHistory() => selectionHistory.Clear();
        public static JobType GetCurrentJob() => currentJob;

        internal static LevelProgressionConfig Config
        {
            get
            {
                LevelProgressionConfig difficultyConfig = DifficultyBalanceService.GetProgressionConfig();
                if (difficultyConfig != null)
                {
                    return difficultyConfig;
                }

                if (progressionConfig == null)
                {
                    progressionConfig = Resources.Load<LevelProgressionConfig>(LevelProgressionConfig.DefaultResourcePath);
                    if (progressionConfig == null)
                    {
                        progressionConfig = ScriptableObject.CreateInstance<LevelProgressionConfig>();
                        progressionConfig.hideFlags = HideFlags.HideAndDontSave;
                    }
                }

                return progressionConfig;
            }
        }

        private static void EnsureExpRequirementInitialized()
        {
            LevelProgressionConfig activeConfig = Config;
            if (expRequirementInitialized && expRequirementConfig == activeConfig)
            {
                return;
            }

            CalculateExpRequired();
        }

        private static bool IsWaitingForJobSelection()
        {
            return currentLevel == Config.JobSelectionLevel && currentJob == JobType.None;
        }
    }
}
