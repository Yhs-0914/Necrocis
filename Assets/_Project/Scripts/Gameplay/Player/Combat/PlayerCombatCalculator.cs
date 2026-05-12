using UnityEngine;

namespace Necrocis
{
    public static class PlayerCombatCalculator
    {
        private const float PercentToRatio = 0.01f;

        public static float GetBasicAttackDamage(PlayerStats stats, float fallbackDamage = 0f)
        {
            float damage = stats != null ? stats.AttackPower : fallbackDamage;
            return Mathf.Max(0f, damage);
        }

        public static float GetBasicAttackCooldown(float baseCooldown, PlayerStats stats)
        {
            float attackSpeed = stats != null ? stats.AttackSpeed : 1f;
            attackSpeed = Mathf.Max(0.01f, attackSpeed);
            return Mathf.Max(0f, baseCooldown / attackSpeed);
        }

        public static float GetBasicAttackRange(float baseRange, PlayerStats stats)
        {
            float attackRange = stats != null ? stats.AttackRange : 1f;
            return Mathf.Max(0f, baseRange * Mathf.Max(0f, attackRange));
        }

        public static float GetSkillCooldown(float baseCooldown, PlayerStats stats)
        {
            float reductionPercent = stats != null ? stats.SkillCooldownReduction : 0f;
            float multiplier = 1f - reductionPercent * PercentToRatio;
            return Mathf.Max(0f, baseCooldown * multiplier);
        }

        public static float GetSkillDamage(float baseDamage, PlayerStats stats)
        {
            return Mathf.Max(0f, baseDamage * GetSkillDamageMultiplier(stats));
        }

        public static float GetSkillDamageMultiplier(PlayerStats stats)
        {
            float magicPercent = stats != null ? stats.Magic : 0f;
            return Mathf.Max(0f, 1f + magicPercent * PercentToRatio);
        }
    }
}
