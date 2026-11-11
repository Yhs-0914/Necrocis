namespace Necrocis
{
    public interface IBasicAttackReadModel
    {
        BasicAttackSummary ReadBasicAttackSummary();
    }

    // Display state only: reading it cannot roll damage or consume item stacks.
    public readonly struct BasicAttackSummary
    {
        public readonly float FlatDamageBonus, DamageMultiplier, RangedDamageMultiplier, CooldownMultiplier;
        public readonly bool HasRandomMultiplier, RandomMultiplierInitialized, HasTargetConditionalBonus;
        public readonly float CurrentRandomMultiplier;
        public BasicAttackSummary(float flat, float damage, float ranged, float cooldown,
            bool hasRandom, bool initialized, float random, bool targetConditional)
        {
            FlatDamageBonus=flat;DamageMultiplier=damage;RangedDamageMultiplier=ranged;CooldownMultiplier=cooldown;
            HasRandomMultiplier=hasRandom;RandomMultiplierInitialized=initialized;CurrentRandomMultiplier=random;
            HasTargetConditionalBonus=targetConditional;
        }
        public static BasicAttackSummary Neutral => new BasicAttackSummary(0f,1f,1f,1f,false,false,1f,false);
    }
}
