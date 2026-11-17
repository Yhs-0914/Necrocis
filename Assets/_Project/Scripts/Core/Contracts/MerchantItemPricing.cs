using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Necrocis
{
    public readonly struct MerchantItemTier
    {
        public string ItemId { get; }
        public string FamilyId { get; }
        public int Level { get; }
        public MerchantItemTier(string itemId, string familyId, int level)
        { ItemId = itemId; FamilyId = familyId; Level = level; }
    }

    public readonly struct MerchantPurchaseQuote
    {
        public int ListPrice { get; }
        public int Price { get; }
        public string ReplacementItemId { get; }
        public bool IsUpgrade => !string.IsNullOrEmpty(ReplacementItemId);
        public MerchantPurchaseQuote(int listPrice, int price, string replacementItemId)
        { ListPrice = listPrice; Price = price; ReplacementItemId = replacementItemId; }
    }

    // Price metadata only: declaring a tier never registers or re-enables an item effect.
    public static class MerchantItemPricing
    {
        private static readonly ReadOnlyCollection<MerchantItemTier> tiers = BuildTiers();
        private static readonly Dictionary<string, MerchantItemTier> index = BuildIndex();
        public static IReadOnlyList<MerchantItemTier> TieredItems => tiers;

        private static ReadOnlyCollection<MerchantItemTier> BuildTiers()
        {
            var families = new[]
            {
                new[] { "dilute_mucus", "piercing_mucus", "binding_mucus" },
                new[] { "luminous_tissue", "beam_organ", "focusing_organ" },
                new[] { "split_fragment", "split_tissue", "radial_tissue" },
                new[] { "acid_leak", "acidic_rupture", "acid_cyst" },
                new[] { "elastic_vessel", "vascular_reflection", "layered_vessel" },
                new[] { "dilute_mucosa", "toxic_mucosa", "concentrated_mucosa" },
                new[] { "cooling_nerve", "freezing_nerve", "frost_ganglion" },
                new[] { "ruptured_capillary", "hemorrhage_organ", "spurting_vessel" },
                new[] { "clot_membrane", "platelet_membrane", "aggregated_platelets" },
                new[] { "regeneration_fragment", "recovery_factor", "regenerative_marrow" },
                new[] { "blood_larva", "blood_drone", "pulsing_blood_drone" },
                new[] { "shield_vesicle", "guardian_organ", "layered_guardian" }
            };
            var result = new List<MerchantItemTier>(36);
            foreach (var family in families)
                for (int i = 0; i < family.Length; i++) result.Add(new MerchantItemTier(family[i], family[1], i + 1));
            return result.AsReadOnly();
        }

        private static Dictionary<string, MerchantItemTier> BuildIndex()
        {
            var result = new Dictionary<string, MerchantItemTier>(StringComparer.OrdinalIgnoreCase);
            foreach (var tier in tiers) result.Add(tier.ItemId, tier);
            return result;
        }

        public static bool TryGetTier(string itemId, out MerchantItemTier tier)
            => index.TryGetValue(itemId ?? string.Empty, out tier);

        private static int PriceForLevel(int level) => level == 1 ? CurrencyEconomy.TierOneItemPrice
            : level == 3 ? CurrencyEconomy.TierThreeItemPrice : CurrencyEconomy.TierTwoItemPrice;

        public static int GetListPrice(string itemId)
            => TryGetTier(itemId, out var tier) ? PriceForLevel(tier.Level) : CurrencyEconomy.ItemPrice;

        public static bool IsLowerTier(string ownedId, string incomingId)
            => TryGetTier(ownedId, out var owned) && TryGetTier(incomingId, out var incoming)
                && owned.FamilyId == incoming.FamilyId && owned.Level < incoming.Level;

        public static bool TryQuote(string itemId, IEnumerable<string> ownedIds, out MerchantPurchaseQuote quote)
        {
            quote = default;
            if (string.IsNullOrWhiteSpace(itemId)) return false;
            bool tiered = TryGetTier(itemId, out var incoming);
            int ownedLevel = 0;
            string replacement = null;
            foreach (string ownedId in ownedIds ?? Array.Empty<string>())
            {
                if (string.Equals(itemId, ownedId, StringComparison.OrdinalIgnoreCase)) return false;
                if (!tiered || !TryGetTier(ownedId, out var owned) || owned.FamilyId != incoming.FamilyId) continue;
                if (owned.Level >= incoming.Level) return false;
                if (owned.Level > ownedLevel) { ownedLevel = owned.Level; replacement = ownedId; }
            }
            int listPrice = GetListPrice(itemId);
            quote = new MerchantPurchaseQuote(listPrice, listPrice - (ownedLevel > 0 ? PriceForLevel(ownedLevel) : 0), replacement);
            return true;
        }
    }
}
