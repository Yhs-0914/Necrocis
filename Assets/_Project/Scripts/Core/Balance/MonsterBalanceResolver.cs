using System;
using UnityEngine;

namespace Necrocis
{
    public readonly struct MonsterBalanceSnapshot
    {
        public string MonsterId { get; }
        public MonsterTier Tier { get; }
        public string StatSetId { get; }
        public GameDifficulty Difficulty { get; }
        public int Stage { get; }
        public float MaxHealth { get; }
        public float HealthUnits => Mathf.Max(1f, CharacterStats.ToHealthUnits(MaxHealth));
        public float AttackPower { get; }
        public float MoveSpeed { get; }
        public int Experience { get; }
        public float ContactDamage { get; }
        public float CooldownMultiplier { get; }

        internal MonsterBalanceSnapshot(MonsterDefinition definition, MonsterStatSet set,
            DifficultyBalanceProfile difficulty, int stage, float health, float attack,
            float speed, int experience, float contact, float cooldown)
        {
            MonsterId = definition.monsterId;
            Tier = definition.tier;
            StatSetId = set.id;
            Difficulty = difficulty.difficulty;
            Stage = stage;
            MaxHealth = health;
            AttackPower = attack;
            MoveSpeed = speed;
            Experience = experience;
            ContactDamage = contact;
            CooldownMultiplier = cooldown;
        }

        public float GetPatternDamage(float coefficient)
        {
            if (!MonsterBalanceNumbers.NonNegative(coefficient)
                || !MonsterBalanceNumbers.NonNegative(AttackPower * coefficient))
                throw new ArgumentOutOfRangeException(nameof(coefficient));
            return AttackPower * coefficient;
        }

        public float GetRearmCooldown(float baseSeconds)
        {
            if (!MonsterBalanceNumbers.NonNegative(baseSeconds)
                || !MonsterBalanceNumbers.NonNegative(baseSeconds * CooldownMultiplier))
                throw new ArgumentOutOfRangeException(nameof(baseSeconds));
            return baseSeconds * CooldownMultiplier;
        }
    }

    public static class MonsterBalanceResolver
    {
        public static EnemyDifficultyBalance GetTierBalance(DifficultyBalanceProfile profile, MonsterTier tier)
        {
            if (profile == null) return null;
            switch (tier)
            {
                case MonsterTier.Normal: return profile.enemies;
                case MonsterTier.Elite: return profile.elites;
                case MonsterTier.Boss: return profile.bosses;
                default: return null;
            }
        }

        public static string GetDifficultyValidationError(EnemyDifficultyBalance balance)
        {
            if (balance == null) return "선택 등급의 난이도 배율이 없습니다.";
            if (!MonsterBalanceNumbers.Positive(balance.maxHealth)
                || !MonsterBalanceNumbers.Positive(balance.moveSpeed)
                || !MonsterBalanceNumbers.Positive(balance.attackCooldown)
                || !MonsterBalanceNumbers.NonNegative(balance.outgoingDamage)
                || !MonsterBalanceNumbers.NonNegative(balance.experienceReward))
                return "난이도 배율에 음수, 0 이하 체력/속도/대기 배율 또는 NaN/Infinity가 있습니다.";
            return null;
        }

        public static bool TryResolve(MonsterDefinition definition, string statSetId,
            DifficultyBalanceProfile difficulty, MonsterProgressionProfile progression, int stage,
            out MonsterBalanceSnapshot result, out string error)
        {
            result = default;
            error = null;
            if (definition == null) error = "몬스터 정의가 없습니다.";
            else if (difficulty == null) error = "난이도 프로필이 없습니다.";
            else if (!Enum.IsDefined(typeof(GameDifficulty), difficulty.difficulty)) error = "난이도 종류가 올바르지 않습니다.";
            else if (progression == null) error = "몬스터 진행도 프로필이 없습니다.";
            else error = definition.GetValidationError() ?? progression.GetValidationError();
            if (error != null) return false;

            if (!definition.TryGetStatSet(statSetId, out MonsterStatSet set))
            {
                error = "StatSet을 찾을 수 없습니다: " + statSetId;
                return false;
            }
            if (!progression.TryGetStage(stage, out MonsterProgressionStage progress))
            {
                error = "진행도 범위는 0~4입니다.";
                return false;
            }
            EnemyDifficultyBalance balance = GetTierBalance(difficulty, definition.tier);
            error = GetDifficultyValidationError(balance);
            if (error != null) return false;

            float health = set.maxHealth * balance.maxHealth * progress.maxHealth;
            float attack = set.attackPower * balance.outgoingDamage * progress.attackPower;
            float speed = set.moveSpeed * balance.moveSpeed;
            float contact = definition.contact.enabled ? attack * definition.contact.damageCoefficient : 0f;
            double rawExperience = (double)definition.reward.baseExperience * balance.experienceReward * progress.experience;
            if (!MonsterBalanceNumbers.Positive(health) || !MonsterBalanceNumbers.NonNegative(attack)
                || !MonsterBalanceNumbers.NonNegative(speed) || !MonsterBalanceNumbers.NonNegative(contact)
                || double.IsNaN(rawExperience) || double.IsInfinity(rawExperience) || rawExperience > int.MaxValue)
            {
                error = "계산 결과가 허용 범위를 초과했습니다.";
                return false;
            }

            int experience = checked((int)Math.Round(rawExperience, MidpointRounding.ToEven));
            result = new MonsterBalanceSnapshot(definition, set, difficulty, stage, health, attack,
                speed, experience, contact, balance.attackCooldown);
            return true;
        }
    }
}
