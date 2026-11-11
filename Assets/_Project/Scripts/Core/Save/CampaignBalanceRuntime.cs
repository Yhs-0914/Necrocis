using UnityEngine;
namespace Necrocis
{
    internal static class CampaignBalanceRuntime
    {
        public static void Tick(float seconds, float sessionSeconds)
        {
            if (SaveService.IsRestorePending || !SaveService.HasActiveSession || SaveService.IsCampaignCompleted) return;
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == SceneLoader.SCENE_MAIN_MENU) return;
            var manager = GameManager.Instance;
            if (manager == null || PlayerStats.Instance == null) return;
            if (Application.isFocused) SaveService.BalanceMetrics.TickSession(sessionSeconds);
            if (PlayerStats.Instance.IsDead) return;
            SaveService.BalanceMetrics.Tick(seconds, manager.CurrentBiome, manager.CurrentState);
            SaveService.BalanceMetrics.job = LevelUpManager.GetCurrentJob().ToString();
            int level = LevelUpManager.GetCurrentLevel();
            if (level >= 10 && SaveService.BalanceMetrics.level10Seconds < 0) SaveService.BalanceMetrics.level10Seconds = SaveService.BalanceMetrics.activeSeconds;
            if (level >= 20 && SaveService.BalanceMetrics.level20Seconds < 0) SaveService.BalanceMetrics.level20Seconds = SaveService.BalanceMetrics.activeSeconds;
        }
    }
}
