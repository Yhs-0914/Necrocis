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

        private const int MAX_LEVEL = 30;          // 최대 레벨
        private const int BASE_EXP = 100;          // 기본 필요 경험치
        private const float EXP_MULTIPLIER = 1.25f; // 레벨당 필요 경험치 증가 배율

        public static Action OnLevelUp;       // 레벨업 이벤트 (LevelUpUI가 구독)
        public static Action OnJobSelect;     // 직업 선택 이벤트 (레벨 10에서 발생)
        public static Action<JobType> OnJobChanged; // 직업 확정 이벤트 (전직 완료 시 발생)
        public static Action<int> OnExpGained; // 경험치 획득 이벤트 (ExpBarUI가 구독)

        // 경험치 추가 (레벨별 배율 적용 후 누적)
        public static void AddExp(int baseAmount)
        {
            if (currentLevel >= MAX_LEVEL) return;

            float multiplier = GetExpMultiplier();                     // 레벨 구간별 경험치 배율
            int actualExp = Mathf.RoundToInt(baseAmount * multiplier); // 실제 획득 경험치

            currentExp += actualExp;
            OnExpGained?.Invoke(actualExp);

            CheckLevelUp();
        }

        // 레벨 구간별 경험치 배율
        // 1~9: 2배 (초반 빠른 성장), 10: 0배 (직업 선택 전 경험치 차단)
        // 11~20: 1배 (기본), 21+: 0.8배 (후반 성장 둔화)
        private static float GetExpMultiplier()
        {
            if (currentLevel <= 9)
                return 2.0f;
            else if (currentLevel == 10)
                return currentJob == JobType.None ? 0f : 1f;
            else if (currentLevel <= 20)
                return 1.0f;
            else
                return 0.8f;
        }

        private static int pendingLevelUps; // 대기 중인 레벨업 수 (한번에 여러 레벨 오를 때)

        // 레벨업 가능 여부 확인 (한번에 여러 레벨 오를 수 있으므로 while 사용)
        private static void CheckLevelUp()
        {
            while (currentExp >= expRequired && currentLevel < MAX_LEVEL)
            {
                currentExp -= expRequired;
                currentLevel++;
                CalculateExpRequired();
                pendingLevelUps++;
            }

            if (pendingLevelUps > 0)
            {
                pendingLevelUps--;
                if (currentLevel == 10 && currentJob == JobType.None)
                    OnJobSelect?.Invoke();
                else
                    OnLevelUp?.Invoke();
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
                if (currentLevel == 10 && currentJob == JobType.None)
                    OnJobSelect?.Invoke();
                else
                    OnLevelUp?.Invoke();
            }
        }

        private static void CalculateExpRequired()
        {
            expRequired = Mathf.RoundToInt(BASE_EXP * Mathf.Pow(EXP_MULTIPLIER, currentLevel - 2));
        }

        public static void DebugLevelUp()
        {
            if (currentLevel >= MAX_LEVEL) return;
            currentLevel++;
            CalculateExpRequired();
            if (currentLevel == 10 && currentJob == JobType.None)
                OnJobSelect?.Invoke();
            else
                OnLevelUp?.Invoke();
        }

        public static int GetCurrentLevel() => currentLevel;
        public static int GetCurrentExp() => currentExp;
        public static int GetExpRequired() => expRequired;
        public static float GetExpProgress() => (float)currentExp / expRequired;

        // ─────────────────────────────────
        // 직업 시스템
        // ─────────────────────────────────

        private static JobType currentJob = JobType.None;                        // 현재 선택한 직업
        private static List<StatChoice> selectionHistory = new List<StatChoice>(); // 지금까지 선택한 스탯 기록

        // 직업별 고유 스탯 매핑 (레벨 11+ 선택지에서 1번째로 고정 등장)
        private static Dictionary<JobType, StatChoice> jobStatMap = new Dictionary<JobType, StatChoice>
        {
            [JobType.Warrior] = StatChoice.AttackPowerUp,     // 전사 → 공격력
            [JobType.Mage] = StatChoice.MagicUp,              // 마법사 → 마력
            [JobType.Archer] = StatChoice.AttackSpeedRangeUp  // 궁수 → 공격속도/사거리
        };

        public static List<StatChoice> GetRandomChoices()
        {
            if (currentLevel >= 11 && currentJob != JobType.None)
                return GetJobBasedChoices();

            return GetRandomFourChoices();
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
                return mostSelectedList[UnityEngine.Random.Range(0, mostSelectedList.Count)];

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

        public static void RecordSelection(StatChoice choice) => selectionHistory.Add(choice);
        public static void SetJob(JobType job)
        {
            if (job == JobType.None || currentJob == job)
            {
                return;
            }

            currentJob = job;
            OnJobChanged?.Invoke(currentJob);
        }
        public static void ResetSelectionHistory() => selectionHistory.Clear();
        public static JobType GetCurrentJob() => currentJob;
    }
}
