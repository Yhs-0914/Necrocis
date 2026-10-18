using UnityEngine;

namespace Necrocis
{
    // Values are read at the original call sites, including while a projectile is already flying.
    public interface IProjectileItemEffects
    {
        bool IsAvailable { get; }
        void ApplyCommonOnHitEffects( EnemyController enemy, float hitDamage, Vector3 hitPosition, uint? expectedGeneration = null);
        float ApplyPerTargetDamageModifiers(EnemyController enemy, float damage);
        int GetBoomerangMaxHitsPerPass();
        float GetBoomerangRepeatHitDamageMultiplier();
        float GetBoomerangReturnDistance();
        float GetBoomerangReturnDistance(float minimumReturnDistance);
        float GetExplosionDamageMultiplier();
        float GetExplosionRadius();
        float GetHomingSearchRadius();
        float GetHomingTurnRate();
        HealthEffectSource GetItemHealthEffectSource(string itemId, string effectId);
        int GetPiercingHitCount();
        float GetPiercingHitDamageMultiplier(int previousHitCount);
        float GetPulseAmplitude();
        float GetPulseFrequency();
        float GetRangeMultiplier();
        int GetReflectionBounceCount();
        float GetScaleMultiplier();
        bool HasExplosiveBloodCell { get; }
        bool HasHomingCell { get; }
        bool HasPiercingMucus { get; }
        bool HasPulseBullet { get; }
        bool HasRefluxOrgan { get; }
        bool HasSplitTissue { get; }
        float RollUnstableCellProjectileSpeedMultiplier(out float damageMultiplier);
        void SpawnSplitProjectiles(Vector3 origin, Vector3 forwardDirection, float damage, LayerMask mask, float range);
        void TryApplyPostDamageExecutionInstinct(EnemyController enemy, float appliedDamage, uint? expectedGeneration = null);
    }

    public interface IBasicAttackItemEffects : IProjectileItemEffects, IBasicAttackReadModel
    {
        float BeamDamageMultiplier { get; }
        int BeamHitBufferSize { get; }
        float BeamRadius { get; }
        float CellProliferationDamageMultiplier { get; }
        float GetAccuracyPenaltyAngle();
        float GetAttackCooldownMultiplier();
        float GetBackShotDamageMultiplier();
        int GetForwardProjectileCount();
        float GetForwardProjectileDamageMultiplier(int projectileCount);
        float GetMeleeRangeMultiplier();
        float GetOutgoingBasicDamageFlatBonus();
        float GetOutgoingBasicDamageMultiplier();
        float GetRangedBasicDamageMultiplier();
        float GetSpreadAngleForCount(int projectileCount);
        bool HasBeamOrgan { get; }
        bool HasLaryngealNerve { get; }
        void NotifyBasicAttackPerformed(float attackDamage, LayerMask mask, float range, Vector3 forwardDirection);
        bool RollCellProliferation();
        float RollUnstableCoreDamageMultiplier();
    }
}
