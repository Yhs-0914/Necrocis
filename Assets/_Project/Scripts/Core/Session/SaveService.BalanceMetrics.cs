using UnityEngine;

namespace Necrocis
{
    public static partial class SaveService
    {
        public static CampaignBalanceMetrics BalanceMetrics => HasActiveSession
            ? (activeRun.balanceMetrics ??= new CampaignBalanceMetrics()) : null;



        public static void RecordPlayerHealing(float amount)
        {
            if (Application.isPlaying && amount > 0f && !IsRestorePending && BalanceMetrics != null) BalanceMetrics.healing += amount;
        }

        public static void RecordPlayerHit(float damage, bool dead)
        {
            var metrics = BalanceMetrics;
            if (metrics == null) return;
            metrics.playerDamageTaken += damage;
            if (dead) { metrics.deaths++; metrics.Record("death", activeRun.checkpoint?.biome.ToString()); }
        }

        public static void RecordItemChoice(string id)
        {
            var metrics = BalanceMetrics;
            if (metrics == null || IsRestorePending) return;
            if (metrics.firstItemSeconds < 0f) metrics.firstItemSeconds = metrics.activeSeconds;
            metrics.Record("item", id);
        }
    }
}
