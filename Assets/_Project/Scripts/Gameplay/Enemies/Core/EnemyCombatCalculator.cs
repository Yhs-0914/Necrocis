using UnityEngine;

namespace Necrocis
{
    public static class EnemyCombatCalculator
    {
        public static float GetAttackDamage(CharacterStats stats, EnemySpawnRuleConfig config)
        {
            float damage = stats != null
                ? stats.AttackPower
                : config != null ? config.attackDamage : 0f;

            return Mathf.Max(0f, damage);
        }

        public static float GetIncomingDamage(float rawDamage, EnemyStatusEffectController statusEffects)
        {
            float multiplier = statusEffects != null
                ? statusEffects.GetIncomingDamageMultiplier()
                : 1f;

            return Mathf.Max(0f, rawDamage * multiplier);
        }
    }
}
