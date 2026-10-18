using System;
using System.Collections.Generic;

namespace Necrocis
{
    [Serializable]
    public sealed class HealthDeltaRemainderData
    {
        public string sourceId;
        public float damage;
        public float heal;
    }

    public readonly struct HealthDeltaPreview
    {
        internal readonly HealthDeltaAccumulator Owner;
        internal readonly long Epoch, Revision;
        internal readonly double Remainder;
        internal readonly bool Healing;
        public HealthEffectSource Source { get; }
        public float WholeUnits { get; }

        internal HealthDeltaPreview(HealthDeltaAccumulator owner, long epoch, long revision,
            HealthEffectSource source, bool healing, float wholeUnits, double remainder)
        {
            Owner = owner; Epoch = epoch; Revision = revision; Source = source;
            Healing = healing; WholeUnits = wholeUnits; Remainder = remainder;
        }
    }

    // The recipient owns this ledger. Only a successful commit advances its state.
    public sealed class HealthDeltaAccumulator
    {
        private const double BoundaryEpsilon = 0.0000001;
        private readonly Dictionary<HealthEffectSource, double> damage = new Dictionary<HealthEffectSource, double>();
        private readonly Dictionary<HealthEffectSource, double> heal = new Dictionary<HealthEffectSource, double>();
        private long epoch, revision;

        public HealthDeltaPreview PreviewDamage(float amount, HealthEffectSource source) => Preview(amount, source, false);
        public HealthDeltaPreview PreviewHealing(float amount, HealthEffectSource source) => Preview(amount, source, true);

        private HealthDeltaPreview Preview(float amount, HealthEffectSource source, bool healing)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), "Health deltas must be finite and nonnegative.");
            var ledger = healing ? heal : damage;
            ledger.TryGetValue(source, out double remainder);
            double total = remainder + amount;
            double whole = Math.Floor(total + BoundaryEpsilon);
            double next = Math.Max(0d, total - whole);
            return new HealthDeltaPreview(this, epoch, revision, source, healing, (float)whole, next);
        }

        public bool TryCommit(HealthDeltaPreview preview)
        {
            if (!ReferenceEquals(preview.Owner, this) || preview.Epoch != epoch || preview.Revision != revision) return false;
            var ledger = preview.Healing ? heal : damage;
            if (preview.Remainder > 0d) ledger[preview.Source] = preview.Remainder;
            else ledger.Remove(preview.Source);
            revision++;
            return true;
        }

        public float DamageRemainder(HealthEffectSource source) => damage.TryGetValue(source, out double value) ? (float)value : 0f;
        public float HealingRemainder(HealthEffectSource source) => heal.TryGetValue(source, out double value) ? (float)value : 0f;

        public void RemoveSource(HealthEffectSource source)
        {
            damage.Remove(source);
            heal.Remove(source);
            revision++;
        }

        public void ClearHealing()
        {
            heal.Clear(); revision++;
        }

        public void BeginLifetime()
        {
            damage.Clear(); heal.Clear(); epoch++; revision++;
        }

        internal void InvalidatePreviews() => revision++;

        // In-memory suspension preserves exact doubles and every source, unlike save DTOs.
        public HealthDeltaAccumulator CloneRuntime()
        {
            var copy = new HealthDeltaAccumulator();
            foreach (var pair in damage) copy.damage.Add(pair.Key, pair.Value);
            foreach (var pair in heal) copy.heal.Add(pair.Key, pair.Value);
            return copy;
        }

        public void RestoreRuntime(HealthDeltaAccumulator snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (ReferenceEquals(this, snapshot)) throw new ArgumentException("Restore requires independent ownership.", nameof(snapshot));
            BeginLifetime(); // Old previews must never commit into a restored recipient.
            foreach (var pair in snapshot.damage) damage.Add(pair.Key, pair.Value);
            foreach (var pair in snapshot.heal) heal.Add(pair.Key, pair.Value);
        }

        public List<HealthDeltaRemainderData> CapturePersistent()
        {
            var entries = new Dictionary<string, HealthDeltaRemainderData>(StringComparer.Ordinal);
            var sources = new Dictionary<string, HealthEffectSource>(StringComparer.Ordinal);
            Capture(damage, false, entries, sources);
            Capture(heal, true, entries, sources);
            var result = new List<HealthDeltaRemainderData>(entries.Values);
            result.Sort((a, b) => StringComparer.Ordinal.Compare(a.sourceId, b.sourceId));
            return result;
        }

        private static void Capture(Dictionary<HealthEffectSource, double> ledger, bool healing,
            Dictionary<string, HealthDeltaRemainderData> entries, Dictionary<string, HealthEffectSource> sources)
        {
            foreach (var pair in ledger)
            {
                string id = pair.Key.PersistenceKey;
                if (id == null) continue;
                if (sources.TryGetValue(id, out HealthEffectSource existing) && existing != pair.Key)
                    throw new InvalidOperationException("Two live health effects share persistence key: " + id);
                sources[id] = pair.Key;
                if (!entries.TryGetValue(id, out var entry)) entries.Add(id, entry = new HealthDeltaRemainderData { sourceId = id });
                // The closest float below one is a valid representation of a double residual near one.
                float value = Math.Min(0.99999994f, (float)pair.Value);
                if (healing) entry.heal = value; else entry.damage = value;
            }
        }

        public void RestorePersistent(IEnumerable<HealthDeltaRemainderData> entries, Func<string, HealthEffectSource?> resolve)
        {
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));
            if (entries == null) return;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var resolvedSources = new HashSet<HealthEffectSource>();
            var resolvedEntries = new List<KeyValuePair<HealthEffectSource, HealthDeltaRemainderData>>();
            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.sourceId) || !seen.Add(entry.sourceId)
                    || !ValidRemainder(entry.damage) || !ValidRemainder(entry.heal))
                    throw new ArgumentException("Invalid or duplicate saved health remainder.", nameof(entries));
                HealthEffectSource? resolved = resolve(entry.sourceId);
                if (!resolved.HasValue) continue;
                HealthEffectSource source = resolved.Value;
                if (!string.Equals(source.PersistenceKey, entry.sourceId, StringComparison.Ordinal))
                    throw new InvalidOperationException("Health effect resolver returned a different persistence key.");
                if (!resolvedSources.Add(source)) throw new InvalidOperationException("Saved health effects resolve to the same runtime source.");
                resolvedEntries.Add(new KeyValuePair<HealthEffectSource, HealthDeltaRemainderData>(source, entry));
            }
            foreach (var pair in resolvedEntries)
            {
                HealthEffectSource source = pair.Key;
                HealthDeltaRemainderData entry = pair.Value;
                if (entry.damage > 0f) damage[source] = entry.damage; else damage.Remove(source);
                if (entry.heal > 0f) heal[source] = entry.heal; else heal.Remove(source);
                revision++;
            }
        }

        public static bool ValidRemainder(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f && value < 1f;
    }
}
