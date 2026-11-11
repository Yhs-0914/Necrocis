using System;

namespace Necrocis
{
    public interface IEnemyDamageGate
    {
        DamageBlockReason EvaluateDamage(EnemyController target, float finalDamage, HealthDeltaPreview preview);
    }

    public partial class EnemyController
    {
        private IEnemyDamageGate damageGate;
        private long damageGateRegistration;

        private sealed class DamageGateLease : IDisposable
        {
            private EnemyController target;
            private readonly uint generation;
            private readonly long registration;
            public DamageGateLease(EnemyController target, uint generation, long registration)
            {
                this.target = target; this.generation = generation; this.registration = registration;
            }
            public void Dispose()
            {
                if (target != null && target.SpawnGeneration == generation && target.damageGateRegistration == registration)
                    target.ResetDamageGate();
                target = null;
            }
        }

        public IDisposable BindDamageGate(IEnemyDamageGate gate)
        {
            damageGate = gate ?? throw new ArgumentNullException(nameof(gate));
            damageGateRegistration++;
            return new DamageGateLease(this, SpawnGeneration, damageGateRegistration);
        }

        private void ResetDamageGate() { damageGate = null; damageGateRegistration++; }
        public HealthEffectSource GetHealthEffectSource(string effectId)
            => logicalLife!=null?logicalLife.Source(effectId):HealthEffectSource.ForOwner(this, effectId, SpawnGeneration);
    }
}
