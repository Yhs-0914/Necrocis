using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Necrocis
{
    // Identity of a repeating effect, independent of an individual projectile or hit attempt.
    public readonly struct HealthEffectSource : IEquatable<HealthEffectSource>
    {
        private sealed class OwnerIdentity { public readonly long Value = Interlocked.Increment(ref nextOwnerId); }
        private static long nextOwnerId;
        private static readonly ConditionalWeakTable<object, OwnerIdentity> owners = new ConditionalWeakTable<object, OwnerIdentity>();
        private readonly long ownerId;
        private readonly uint generation;
        private readonly string effectId;
        public string EffectId => effectId ?? "legacy";
        public string PersistenceKey { get; }
        public static HealthEffectSource Legacy => default;

        private HealthEffectSource(long ownerId, uint generation, string effectId, string persistenceKey)
        {
            this.ownerId = ownerId;
            this.generation = generation;
            this.effectId = effectId;
            PersistenceKey = persistenceKey;
        }

        public static HealthEffectSource ForOwner(object owner, string effectId, uint generation = 0, string persistenceKey = null)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (string.IsNullOrWhiteSpace(effectId)) throw new ArgumentException("An effect ID is required.", nameof(effectId));
            return new HealthEffectSource(owners.GetValue(owner, _ => new OwnerIdentity()).Value,
                generation, effectId, string.IsNullOrWhiteSpace(persistenceKey) ? null : persistenceKey);
        }

        public HealthEffectSource WithEffect(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("An effect ID is required.", nameof(id));
            return new HealthEffectSource(ownerId, generation, EffectId + "/" + id,
                PersistenceKey == null ? null : PersistenceKey + "/" + id);
        }

        public bool Equals(HealthEffectSource other) => ownerId == other.ownerId && generation == other.generation
            && string.Equals(EffectId, other.EffectId, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is HealthEffectSource other && Equals(other);
        public override int GetHashCode()
        {
            unchecked { return ((ownerId.GetHashCode() * 397) ^ (int)generation) * 397 ^ StringComparer.Ordinal.GetHashCode(EffectId); }
        }
        public static bool operator ==(HealthEffectSource left, HealthEffectSource right) => left.Equals(right);
        public static bool operator !=(HealthEffectSource left, HealthEffectSource right) => !left.Equals(right);
    }

    public enum DamageBlockReason { None, Invalid, Invincible, Absorbed, Immune, Reflected }

    public readonly struct DamageResult
    {
        public bool HitAccepted { get; }
        public float HpApplied { get; }
        public DamageBlockReason HpBlock { get; }
        public bool Revived { get; }
        public bool Died { get; }

        public DamageResult(bool accepted, float hpApplied = 0f, DamageBlockReason block = DamageBlockReason.None,
            bool revived = false, bool died = false)
        {
            HitAccepted = accepted; HpApplied = hpApplied; HpBlock = block; Revived = revived; Died = died;
        }
        public static DamageResult Invalid => new DamageResult(false, 0f, DamageBlockReason.Invalid);
    }
}
