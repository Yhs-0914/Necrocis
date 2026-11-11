namespace Necrocis
{
    public partial class PlayerItemCombatEffects
    {
        public BasicAttackSummary ReadBasicAttackSummary() => new BasicAttackSummary(
            Attack.GetOutgoingBasicDamageFlatBonus(), Attack.GetOutgoingBasicDamageMultiplier(),
            Attack.GetRangedBasicDamageMultiplier(), Attack.GetAttackCooldownMultiplier(),
            Ownership.HasUnstableCore, Attack.unstableCoreInitialized, Attack.unstableCoreCurrentMultiplier,
            Ownership.HasHeartSniper || Ownership.HasBioResonance);
    }
}
