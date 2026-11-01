using UnityEngine;

namespace Necrocis
{
    public static partial class SaveService
    {
        public static CampaignBalanceMetrics BalanceMetrics => HasActiveSession
            ? (activeRun.balanceMetrics ??= new CampaignBalanceMetrics()) : null;

        public static void TickBalanceMetrics(float seconds, float sessionSeconds)
        {
            if (IsRestorePending || !HasActiveSession || activeRun.campaignCompleted) return;
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == SceneLoader.SCENE_MAIN_MENU) return;
            var manager = GameManager.Instance;
            if (manager == null || PlayerStats.Instance == null) return;
            if (Application.isFocused) BalanceMetrics.TickSession(sessionSeconds);
            if (PlayerStats.Instance.IsDead) return;
            BalanceMetrics.Tick(seconds, manager.CurrentBiome, manager.CurrentState);
            BalanceMetrics.job = LevelUpManager.GetCurrentJob().ToString();
            int level = LevelUpManager.GetCurrentLevel();
            if (level >= 10 && BalanceMetrics.level10Seconds < 0) BalanceMetrics.level10Seconds = BalanceMetrics.activeSeconds;
            if (level >= 20 && BalanceMetrics.level20Seconds < 0) BalanceMetrics.level20Seconds = BalanceMetrics.activeSeconds;
        }

        public static void RecordPlayerHealing(float amount)
        {
            if (Application.isPlaying && amount > 0f && !IsRestorePending && BalanceMetrics != null) BalanceMetrics.healing += amount;
        }

        public static void RecordPlayerHit(float damage, bool dead)
        {
            var metrics = BalanceMetrics;
            if (metrics == null) return;
            metrics.playerDamageTaken += damage;
            if (dead) { metrics.deaths++; metrics.Record("death", GameManager.Instance?.CurrentBiome.ToString()); }
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
