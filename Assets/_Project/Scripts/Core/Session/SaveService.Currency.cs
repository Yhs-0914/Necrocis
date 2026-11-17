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
            SetCurrency(current + Math.Min(value, long.MaxValue - current));
        }

        public static bool TrySpendCurrency(long price)
        {
            if (price < 0 || HasPendingTerminalOperation || CurrencyBalance < price) return false;
            SetCurrency(CurrencyBalance - price); return true;
        }

        private static void SetCurrency(long value)
        {
            if (HasActiveSession) activeRun.currency = value; else previewCurrency = value;
            NotifyCurrencyChanged();
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

        public static MerchantShopSaveData RememberMerchantShop(MerchantShopSaveData shop)
        {
            var existing = FindMerchantShop(shop.biome, shop.mapSeed);
            if (existing != null) return existing;
            var shops = HasActiveSession ? (activeRun.world.merchantShops ??= new List<MerchantShopSaveData>()) : previewMerchantShops;
            shops.RemoveAll(value => value != null && value.biome == shop.biome);
            shops.Add(shop); return shop;
        }
    }
}
