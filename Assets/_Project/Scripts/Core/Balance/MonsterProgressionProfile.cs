using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [Serializable]
    public sealed class MonsterProgressionStage
    {
        [Range(0, 4)] public int stage;
        [Min(0.01f)] public float maxHealth = 1f;
        [Min(0f)] public float attackPower = 1f;
        [Min(0f)] public float experience = 1f;
    }

    [CreateAssetMenu(menuName = "Necrocis/Balance/Monster Progression Profile", fileName = "MonsterProgressionProfile")]
    public sealed class MonsterProgressionProfile : ScriptableObject
    {
        public const int MaximumStage = 4;
        public List<MonsterProgressionStage> stages = CreateRecommendedStages();

        public static List<MonsterProgressionStage> CreateRecommendedStages()
        {
            return new List<MonsterProgressionStage>
            {
                new MonsterProgressionStage { stage = 0 },
                new MonsterProgressionStage { stage = 1, maxHealth = 1.15f, attackPower = 1.08f, experience = 1.10f },
                new MonsterProgressionStage { stage = 2, maxHealth = 1.35f, attackPower = 1.20f, experience = 1.25f },
                new MonsterProgressionStage { stage = 3, maxHealth = 1.60f, attackPower = 1.35f, experience = 1.40f },
                new MonsterProgressionStage { stage = 4, maxHealth = 1.85f, attackPower = 1.50f, experience = 1.60f }
            };
        }

        public string GetValidationError()
        {
            if (stages == null || stages.Count != MaximumStage + 1) return "진행도 0~4를 각각 한 번 정의해야 합니다.";
            var seen = new HashSet<int>();
            foreach (MonsterProgressionStage value in stages)
            {
                if (value == null || value.stage < 0 || value.stage > MaximumStage || !seen.Add(value.stage))
                    return "진행도 단계가 누락/중복되었거나 범위를 벗어났습니다.";
                if (!MonsterBalanceNumbers.Positive(value.maxHealth)
                    || !MonsterBalanceNumbers.NonNegative(value.attackPower)
                    || !MonsterBalanceNumbers.NonNegative(value.experience))
                    return "진행도 배율은 유한한 값이어야 하며 체력은 양수, 공격력/경험치는 0 이상이어야 합니다.";
            }
            return null;
        }

        public bool TryGetStage(int stage, out MonsterProgressionStage result)
        {
            result = null;
            if (stages == null) return false;
            foreach (MonsterProgressionStage value in stages)
            {
                if (value != null && value.stage == stage)
                {
                    result = value;
                    return true;
                }
            }
            return false;
        }
    }
}
