using System;

namespace Necrocis
{
    // Final outgoing damage, including difficulty/progression. Safe to keep in a projectile/DoT.
    public readonly struct EnemyDamageRequest
    {
        public float Amount { get; }
        public HealthEffectSource EffectSource { get; }
        private readonly EnemyController source;
        private readonly uint generation;
        public EnemyController Source => source != null && source.SpawnGeneration == generation
            && source.gameObject.activeInHierarchy ? source : null;

        public EnemyDamageRequest(float amount, EnemyController source)
            : this(amount, source, source != null ? source.SpawnGeneration : 0,
                source != null ? source.GetHealthEffectSource("attack") : HealthEffectSource.Legacy)
        {
        }

        public EnemyDamageRequest(float amount, EnemyController source, HealthEffectSource effectSource)
            : this(amount, source, source != null ? source.SpawnGeneration : 0, effectSource)
        {
        }

        private EnemyDamageRequest(float amount, EnemyController source, uint generation, HealthEffectSource effectSource)
        {
            if (!MonsterBalanceNumbers.NonNegative(amount)) throw new ArgumentOutOfRangeException(nameof(amount));
            Amount = amount;
            this.source = source;
            this.generation = generation;
            EffectSource = effectSource;
        }

        public EnemyDamageRequest WithAmount(float amount) => new EnemyDamageRequest(amount, source, generation, EffectSource);
        public EnemyDamageRequest WithEffect(string effectId) => new EnemyDamageRequest(Amount, source, generation, EffectSource.WithEffect(effectId));
    }
}
