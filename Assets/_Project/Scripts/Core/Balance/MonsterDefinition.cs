using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public enum MonsterTier { Normal = 0, Elite = 1, Boss = 2 }

    [Serializable]
    public sealed class MonsterStatSet
    {
        public string id = "Default";
        [Min(0.01f)] public float maxHealth = 30f;
        [Min(0f)] public float attackPower = 2f;
        [Min(0f)] public float moveSpeed = 1f;
    }

    [Serializable]
    public sealed class MonsterRewardSettings
    {
        [Min(0)] public int baseExperience = 50;
    }

    [Serializable]
    public sealed class MonsterPatternDamage
    {
        public string id;
        [Tooltip("최종 공격력에 곱할 비율입니다. 난이도/진행도 배율을 다시 넣지 마세요.")]
        [Min(0f)] public float coefficient = 1f;
    }

    [Serializable]
    public sealed class MonsterContactSettings
    {
        public bool enabled = true;
        [Min(0f)] public float damageCoefficient = 0.5f;
        [Min(0f)] public float knockbackDistance = 0.45f;
    }

    [CreateAssetMenu(menuName = "Necrocis/Balance/Monster Definition", fileName = "MonsterDefinition")]
    public sealed class MonsterDefinition : ScriptableObject
    {
        [Tooltip("표시 이름/파일 이름과 독립적인 저장 식별자입니다. 등록 후 변경하지 마세요.")]
        public string monsterId;
        public string displayName;
        public MonsterTier tier = MonsterTier.Elite;
        [TextArea] public string designNote;
        public List<MonsterStatSet> statSets = new List<MonsterStatSet> { new MonsterStatSet() };
        public MonsterRewardSettings reward = new MonsterRewardSettings();
        public MonsterContactSettings contact = new MonsterContactSettings();
        public MonsterPatternSettings pattern;
        public List<MonsterPatternDamage> patternDamage = new List<MonsterPatternDamage>();

        public string GetValidationError()
        {
            if (string.IsNullOrWhiteSpace(monsterId)) return "Monster ID가 비어 있습니다.";
            if (!Enum.IsDefined(typeof(MonsterTier), tier)) return "몬스터 등급이 올바르지 않습니다.";
            if (statSets == null || statSets.Count == 0) return "StatSet이 필요합니다.";
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (MonsterStatSet set in statSets)
            {
                if (set == null || string.IsNullOrWhiteSpace(set.id)) return "StatSet ID가 비어 있습니다.";
                if (!ids.Add(set.id)) return "StatSet ID 중복: " + set.id;
                if (!MonsterBalanceNumbers.Positive(set.maxHealth)) return set.id + ": 체력은 유한한 양수여야 합니다.";
                if (!MonsterBalanceNumbers.NonNegative(set.attackPower)) return set.id + ": 공격력이 잘못되었습니다.";
                if (!MonsterBalanceNumbers.NonNegative(set.moveSpeed)) return set.id + ": 이동속도가 잘못되었습니다.";
            }
            if (!ids.Contains("Default")) return "생성 시 사용할 Default StatSet이 필요합니다.";
            if (reward == null || reward.baseExperience < 0) return "기본 경험치가 잘못되었습니다.";
            if (contact == null || !MonsterBalanceNumbers.NonNegative(contact.damageCoefficient)
                || !MonsterBalanceNumbers.NonNegative(contact.knockbackDistance))
                return "접촉 피해 계수/넉백 거리가 잘못되었습니다.";
            ids.Clear();
            if (patternDamage == null) return "패턴 피해 목록이 없습니다.";
            foreach (MonsterPatternDamage pattern in patternDamage)
            {
                if (pattern == null || string.IsNullOrWhiteSpace(pattern.id) || !ids.Add(pattern.id)
                    || !MonsterBalanceNumbers.NonNegative(pattern.coefficient))
                    return "패턴 피해 ID/계수가 잘못되었거나 중복되었습니다.";
            }
            return pattern != null ? pattern.GetValidationError(this) : null;
        }

        public bool TryGetStatSet(string id, out MonsterStatSet result)
        {
            result = null;
            if (statSets == null) return false;
            foreach (MonsterStatSet set in statSets)
            {
                if (set != null && string.Equals(set.id, id, StringComparison.Ordinal))
                {
                    result = set;
                    return true;
                }
            }
            return false;
        }
    }

    internal static class MonsterBalanceNumbers
    {
        internal static bool NonNegative(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
        internal static bool Positive(float value) => NonNegative(value) && value > 0f;
    }
}
