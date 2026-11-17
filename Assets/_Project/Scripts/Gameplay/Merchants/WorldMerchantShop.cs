using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Necrocis
{
    public static class WorldMerchantShop
    {
        public const float InteractionDistance = 3f;
        private static bool purchasing;

        public static bool IsInReach(WorldMerchantNpc merchant, PlayerController player)
        {
            if (merchant == null || !merchant.isActiveAndEnabled || !merchant.IsVisible || player == null
                || !player.gameObject.activeInHierarchy || player.IsDead || player.IsBossDisplaced
                || merchant.gameObject.scene != SceneManager.GetActiveScene()) return false;
            Vector3 delta = merchant.transform.position - player.transform.position;
            delta.y = 0;
            return delta.sqrMagnitude <= InteractionDistance * InteractionDistance;
        }

        public static MerchantShopSaveData EnsureStock(WorldMerchantNpc merchant, PlayerItemManager inventory)
        {
            var biome = merchant != null ? merchant.GetComponentInParent<BiomeManager>() : null;
            if (biome == null || inventory == null) return null;
            var stock = SaveService.FindMerchantShop(biome.BiomeType, biome.Seed);
            bool created = stock == null;
            int seed = unchecked(biome.Seed * 397 ^ (int)biome.BiomeType * 7919 ^ 0x53484F50);
            if (created) stock = SaveService.RememberMerchantShop(new MerchantShopSaveData
            {
                biome = biome.BiomeType, mapSeed = biome.Seed
            });
            stock.itemIds ??= new List<string>(); stock.purchasedItemIds ??= new List<string>();
            var catalog = new HashSet<string>(inventory.ItemEntries.Where(item => item != null).Select(item => item.ItemId), StringComparer.OrdinalIgnoreCase);
            var owned = new HashSet<string>(inventory.AcquiredItems.Select(item => item.ItemId), StringComparer.OrdinalIgnoreCase);
            var purchased = new HashSet<string>(stock.purchasedItemIds, StringComparer.OrdinalIgnoreCase);
            var eligible = new HashSet<string>(catalog.Where(id => MerchantItemPricing.TryQuote(id, owned, out _)), StringComparer.OrdinalIgnoreCase);
            // Keep valid offers and sold-out slots. Only replace unsold items acquired elsewhere.
            var offers = stock.itemIds.Where(id => !string.IsNullOrWhiteSpace(id) && catalog.Contains(id)
                    && (purchased.Contains(id) || eligible.Contains(id)))
                .Distinct(StringComparer.OrdinalIgnoreCase).Take(3).ToList();
            offers.AddRange(CampaignRewardRules.ChooseMerchantOffers(eligible, owned.Concat(offers).Concat(purchased), seed).Take(3 - offers.Count));
            bool changed = created || !stock.itemIds.SequenceEqual(offers, StringComparer.OrdinalIgnoreCase);
            if (!changed) return stock;
            stock.itemIds = offers;
            if (SaveService.HasActiveSession && !SaveService.TrySaveActiveRun(out string error))
                Debug.LogWarning("[Merchant] 상품 목록 저장 재시도 필요: " + error);
            return stock;
        }

        public static bool TryBuyItem(WorldMerchantNpc merchant, PlayerController player, string itemId, out string message)
        {
            message = "지금은 구매할 수 없습니다.";
            if (purchasing || !IsInReach(merchant, player)) return false;
            purchasing = true;
            try
            {
                var inventory = player.GetComponent<PlayerItemManager>();
                var stock = EnsureStock(merchant, inventory);
                if (stock == null || string.IsNullOrEmpty(itemId) || !stock.itemIds.Contains(itemId)) return false;
                stock.purchasedItemIds ??= new List<string>();
                if (stock.purchasedItemIds.Contains(itemId)) { message = "이미 판매된 상품입니다."; return false; }
                var owned = inventory.AcquiredItems.Select(item => item.ItemId).ToArray();
                if (!MerchantItemPricing.TryQuote(itemId, owned, out var quote) || !inventory.CanAcquireItem(itemId))
                { message = "이미 같은 아이템이나 상위 단계를 보유하고 있습니다."; return false; }
                if (!SaveService.PrepareMerchantPurchase(out message)) return false;
                if (!SaveService.TrySpendCurrency(quote.Price)) { message = "재화가 부족합니다."; return false; }
                bool granted = quote.IsUpgrade ? inventory.TryReplaceItem(quote.ReplacementItemId, itemId) : inventory.TryAcquireItem(itemId, out _);
                if (!granted)
                { SaveService.CreditCurrency(quote.Price); message = "아이템을 획득할 수 없습니다."; return false; }
                if (quote.IsUpgrade)
                    foreach (string previous in owned.Where(id => MerchantItemPricing.IsLowerTier(id, itemId)).Distinct(StringComparer.OrdinalIgnoreCase))
                        while (inventory.ContainsItem(previous) && inventory.RemoveItem(previous)) { }
                stock.purchasedItemIds.Add(itemId);
                inventory.TryGetItemEntry(itemId, out var entry);
                SavePurchase((entry?.DisplayName ?? itemId) + (quote.IsUpgrade ? " 교체 완료" : " 구매 완료") + " · " + quote.Price + "골드", out message);
                return true;
            }
            finally { purchasing = false; }
        }

        public static bool TryBuyRecovery(WorldMerchantNpc merchant, PlayerController player, out string message)
        {
            message = "지금은 구매할 수 없습니다.";
            if (purchasing || !IsInReach(merchant, player)) return false;
            purchasing = true;
            try
            {
                if (player.CurrentHealth >= player.MaxHealth - .0001f) { message = "체력이 가득 차 있습니다."; return false; }
                if (!SaveService.PrepareMerchantPurchase(out message)) return false;
                if (!SaveService.TrySpendCurrency(CurrencyEconomy.RecoveryPrice)) { message = "재화가 부족합니다."; return false; }
                player.Stats.Heal(CurrencyEconomy.RecoveryHealth, player.GetHealthEffectSource("merchant-recovery"));
                SavePurchase("체력 회복 완료", out message);
                return true;
            }
            finally { purchasing = false; }
        }

        private static void SavePurchase(string success, out string message)
        {
            // Keep a completed grant charged even when disk I/O fails. The pending save contains
            // wallet, stock and player together, and is retried before the next purchase.
            message = success;
            if (SaveService.HasActiveSession && !SaveService.TrySaveActiveRun(out string error))
            { message += " · 저장 재시도 필요"; Debug.LogWarning("[Merchant] " + error); }
        }
    }
}
