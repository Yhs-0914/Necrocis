namespace Necrocis
{
    public partial class PlayerAttack
    {
        public BasicAttackDisplaySnapshot ReadBasicAttackDisplay()
        {
            var stats=playerController!=null?playerController.Stats:GetComponent<PlayerStats>();
            var effects=HasItemEffects?itemEffects:GetComponent<IBasicAttackItemEffects>();
            var items=effects!=null && effects.IsAvailable?effects.ReadBasicAttackSummary():BasicAttackSummary.Neutral;
            float random=items.HasRandomMultiplier && items.RandomMultiplierInitialized?items.CurrentRandomMultiplier:1f;
            return new BasicAttackDisplaySnapshot(
                (PlayerCombatCalculator.GetBasicAttackDamage(stats,meleeAttackDamage)+items.FlatDamageBonus)*items.DamageMultiplier*random,
                (PlayerCombatCalculator.GetBasicAttackDamage(stats,rangedAttackDamage)+items.FlatDamageBonus)*items.DamageMultiplier*items.RangedDamageMultiplier*random,
                PlayerCombatCalculator.GetBasicAttackCooldown(meleeCooldown,stats)*items.CooldownMultiplier,
                PlayerCombatCalculator.GetBasicAttackCooldown(attackCooldown,stats)*items.CooldownMultiplier,items);
        }
    }
}
