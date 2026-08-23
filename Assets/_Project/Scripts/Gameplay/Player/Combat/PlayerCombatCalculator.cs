using UnityEngine;

namespace Necrocis
{
    public static class PlayerCombatCalculator
    {
        private const float PercentToRatio = 0.01f;
        private const float MaximumSkillCooldownReductionPercent = 40f;

        public static float GetBasicAttackDamage(PlayerStats stats, float fallbackDamage = 0f)
        {
            float damage = stats != null ? stats.AttackPower : fallbackDamage;
            float multiplier = DifficultyBalanceService.ActiveProfile?.player?.basicAttackDamage ?? 1f;
            return Mathf.Max(0f, damage * Mathf.Max(0f, multiplier));
        }

        public static float GetBasicAttackCooldown(float baseCooldown, PlayerStats stats)
        {
            float attackSpeed = stats != null ? stats.AttackSpeed : 1f;
            attackSpeed = Mathf.Max(0.01f, attackSpeed);
            float multiplier = DifficultyBalanceService.ActiveProfile?.player?.basicAttackCooldown ?? 1f;
            return Mathf.Max(0f, baseCooldown / attackSpeed * Mathf.Max(0.01f, multiplier));
        }

        public static float GetBasicAttackRange(float baseRange, PlayerStats stats)
        {
            float attackRange = stats != null ? stats.AttackRange : 1f;
            float multiplier = DifficultyBalanceService.ActiveProfile?.player?.basicAttackRange ?? 1f;
            return Mathf.Max(0f, baseRange * Mathf.Max(0f, attackRange) * Mathf.Max(0f, multiplier));
        }

        public static float GetSkillCooldown(float baseCooldown, PlayerStats stats)
        {
            float reductionPercent = stats != null ? stats.SkillCooldownReduction : 0f;
            reductionPercent = Mathf.Clamp(reductionPercent, 0f, MaximumSkillCooldownReductionPercent);
            float multiplier = 1f - reductionPercent * PercentToRatio;
            float difficultyMultiplier = DifficultyBalanceService.ActiveProfile?.player?.skillCooldown ?? 1f;
            return Mathf.Max(0f, baseCooldown * multiplier * Mathf.Max(0.01f, difficultyMultiplier));
        }

        public static float GetSkillDamage(float baseDamage, PlayerStats stats)
        {
            float multiplier = DifficultyBalanceService.ActiveProfile?.player?.skillDamage ?? 1f;
            return Mathf.Max(0f, baseDamage * GetSkillDamageMultiplier(stats) * Mathf.Max(0f, multiplier));
        }

        public static float GetSkillDamageMultiplier(PlayerStats stats)
        {
            float magicPercent = stats != null ? stats.Magic : 0f;
            return Mathf.Max(0f, 1f + magicPercent * PercentToRatio);
        }
    }
}
