using System;
using System.Collections.Generic;

namespace Necrocis
{
    public partial class PlayerItemCombatEffects
    {
        private readonly Dictionary<string, uint> itemHealthGenerations = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, HealthEffectSource>> itemHealthSources
            = new Dictionary<string, Dictionary<string, HealthEffectSource>>(StringComparer.OrdinalIgnoreCase);

        public HealthEffectSource GetItemHealthEffectSource(string itemId, string effectId)
        {
            HealthEffectSource source = BuildItemHealthEffectSource(itemId, effectId);
            if (!itemHealthSources.TryGetValue(itemId, out var effects))
                itemHealthSources.Add(itemId, effects = new Dictionary<string, HealthEffectSource>(StringComparer.Ordinal));
            if (effects.TryGetValue(effectId, out HealthEffectSource previous) && previous != source)
                playerStats?.RuntimeStats.RemoveHealthEffectSource(previous);
            effects[effectId] = source;
            return source;
        }

        private HealthEffectSource BuildItemHealthEffectSource(string itemId, string effectId)
        {
            if (playerStats == null) throw new InvalidOperationException("Item health effects require the bound local PlayerStats.");
            if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(effectId))
                throw new ArgumentException("Item and effect IDs are required.");
            itemHealthGenerations.TryGetValue(itemId, out uint generation);
            string id = itemId.ToLowerInvariant();
            string persistenceKey = IsPersistentItemHealthEffect(id, effectId) ? "item:" + id + ":" + effectId : null;
            return playerStats.GetHealthEffectSource("item/" + id + "/acquisition:" + generation + "/" + effectId, persistenceKey);
        }

        private static bool IsPersistentItemHealthEffect(string itemId, string effectId)
            => effectId == "heal" && (itemId == RecoveryFactorId || itemId == BloodContractId || itemId == ImperfectRegenerationId)
                || effectId == "self" && itemId == OverheatedOrganId;

        // Pure identity lookup: restoring the ledger must not retire sources or advance lifetimes.
        public HealthEffectSource? ResolvePersistentItemHealthEffectSource(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            string[] parts = key.Split(':');
            if (parts.Length != 3 || parts[0] != "item" || !HasItem(parts[1])
                || !IsPersistentItemHealthEffect(parts[1], parts[2])) return null;
            return BuildItemHealthEffectSource(parts[1], parts[2]);
        }

        internal void RetireItemHealthSources(string itemId)
        {
            string id = itemId.ToLowerInvariant();
            // Restored sources may exist in the HP ledger before their first runtime use.
            if (IsPersistentItemHealthEffect(id, "heal"))
                playerStats?.RuntimeStats.RemoveHealthEffectSource(BuildItemHealthEffectSource(id, "heal"));
            if (IsPersistentItemHealthEffect(id, "self"))
                playerStats?.RuntimeStats.RemoveHealthEffectSource(BuildItemHealthEffectSource(id, "self"));
            if (itemHealthSources.TryGetValue(itemId, out var sources))
            {
                foreach (HealthEffectSource source in sources.Values) playerStats?.RuntimeStats.RemoveHealthEffectSource(source);
                itemHealthSources.Remove(itemId);
            }
            itemHealthGenerations.TryGetValue(itemId, out uint generation);
            itemHealthGenerations[itemId] = generation + 1;
        }
    }
}
