using System;
using System.Collections.Generic;

namespace Necrocis
{
    public static partial class SaveService
    {
        private static long previewCurrency;
        private static readonly HashSet<BiomeType> previewCurrencyBosses = new HashSet<BiomeType>();
        private static readonly List<MerchantShopSaveData> previewMerchantShops = new List<MerchantShopSaveData>();
        public static event Action<long> CurrencyChanged;
        public static long CurrencyBalance => Math.Max(0, HasActiveSession ? activeRun.currency : previewCurrency);

        public static void CreditCurrency(long value)
        {
            if (value <= 0 || HasPendingTerminalOperation) return;
            long current = CurrencyBalance;
            if (current == long.MaxValue) return;
            SetCurrency(current + Math.Min(value, long.MaxValue - current));
        }

        public static bool TrySpendCurrency(long price)
        {
            if (price < 0 || HasPendingTerminalOperation) return false;
            long current = CurrencyBalance;
            if (current < price) return false;
            if (price > 0) SetCurrency(current - price);
            return true;
        }

        private static void SetCurrency(long value)
        {
            if (HasActiveSession) activeRun.currency = value; else previewCurrency = value;
            CurrencyChanged?.Invoke(value);
        }

        private static void NotifyCurrencyChanged() => CurrencyChanged?.Invoke(CurrencyBalance);
        private static void ResetPreviewCurrency()
        {
            previewCurrency = 0; previewCurrencyBosses.Clear(); previewMerchantShops.Clear();
            NotifyCurrencyChanged();
        }

        public static bool PrepareMerchantPurchase(out string error)
        {
            error = null;
            if (HasPendingTerminalOperation) { error = "런 종료 처리가 진행 중입니다."; return false; }
            if (!HasActiveSession) return true;
            return TryFinishPendingSave(out error) && CanUseSlot(activeRun.difficulty, out error);
        }

        public static MerchantShopSaveData FindMerchantShop(BiomeType biome, int mapSeed)
        {
            var shops = HasActiveSession ? activeRun.world.merchantShops : previewMerchantShops;
            return shops?.Find(value => value != null && value.biome == biome && value.mapSeed == mapSeed);
        }

        public static bool IsFirstMerchantShop(MerchantShopSaveData shop)
        {
            var shops = HasActiveSession ? activeRun.world.merchantShops : previewMerchantShops;
            return shop != null && shops != null && shops.Count > 0 && ReferenceEquals(shops[0], shop);
        }

        public static MerchantShopSaveData RememberMerchantShop(MerchantShopSaveData shop)
        {
            var existing = FindMerchantShop(shop.biome, shop.mapSeed);
            if (existing != null) return existing;
            var shops = HasActiveSession ? (activeRun.world.merchantShops ??= new List<MerchantShopSaveData>()) : previewMerchantShops;
            // The save list records shop visit order even if a biome gets a different map seed.
            int index = shops.FindIndex(value => value != null && value.biome == shop.biome);
            shops.RemoveAll(value => value != null && value.biome == shop.biome);
            shops.Insert(index >= 0 ? index : shops.Count, shop);
            return shop;
        }
    }
}
