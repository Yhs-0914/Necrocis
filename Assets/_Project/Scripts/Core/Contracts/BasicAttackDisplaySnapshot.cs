namespace Necrocis
{
    public interface IBasicAttackDisplayReadModel { BasicAttackDisplaySnapshot ReadBasicAttackDisplay(); }
    public readonly struct BasicAttackDisplaySnapshot
    {
        public readonly float MeleeDamage,RangedDamage,MeleeInterval,RangedInterval;
        public readonly BasicAttackSummary Items;
        public BasicAttackDisplaySnapshot(float melee,float ranged,float meleeInterval,float rangedInterval,BasicAttackSummary items)
        {MeleeDamage=melee;RangedDamage=ranged;MeleeInterval=meleeInterval;RangedInterval=rangedInterval;Items=items;}
    }
}
