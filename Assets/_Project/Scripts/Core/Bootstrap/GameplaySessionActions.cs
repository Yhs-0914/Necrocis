using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Necrocis
{
    // Upper game-flow implementation. Screens own coroutine execution/cancellation.
    public sealed class GameplaySessionActions : IDeathReturnActions,ISessionMenuActions
    {
        public static readonly GameplaySessionActions Instance=new GameplaySessionActions();
        private GameplaySessionActions(){}
        public bool IsPlayerDead=>PlayerController.Instance!=null && PlayerController.Instance.IsDead;
        public bool TryPrepare(out DeathReturnPlan plan,out string error)
        {
            plan=default;error=string.Empty;bool completedHardClear=false;
            if(SaveService.HasPendingFinalClear)
            {
                var difficulty=SaveService.PendingOperationDifficulty;
                if(!SaveService.TryRetryPendingOperation(out var message)){error=$"[PlayerDeathScreen] 최종 클리어 저장 복구 실패: {message}";return false;}
                completedHardClear=difficulty==GameDifficulty.Hard;
            }
            if(completedHardClear || SaveService.HasPendingHardDeath || (SaveService.HasActiveSession && SaveService.ActiveDifficulty==GameDifficulty.Hard))
            {
                if(!completedHardClear && !SaveService.TryHandleHardDeath(out var message)){error=$"[PlayerDeathScreen] Hard run 초기화 실패: {message}";return false;}
                plan=new DeathReturnPlan(true,false);return true;
            }
            bool restore=false;
            if(SaveService.HasActiveSession && SaveService.ActiveDifficulty==GameDifficulty.Normal)
            {
                if(!SaveService.TryPrepareNormalDeathRespawn(out var message)){error=$"[PlayerDeathScreen] Normal 사망 진행도 보존 실패: {message}";return false;}
                restore=true;
            }
            plan=new DeathReturnPlan(false,restore);return true;
        }
        public IEnumerator Execute(DeathReturnPlan plan)
        {
            if(plan.MainMenu)
            {
                var menu=GameplaySessionLifecycle.LoadMainMenu();while(menu!=null && !menu.isDone)yield return null;yield break;
            }
            var player=PlayerController.Instance;if(player==null)player=Object.FindFirstObjectByType<PlayerController>();
            if(player!=null)player.ReviveForRespawn();
            GameManager.Instance?.ReturnToHub();
            if(!plan.RestoreNormalProgress && SaveService.HasActiveSession && !SaveService.TrySaveActiveRun(out var saveError))
                Debug.LogError($"[PlayerDeathScreen] Normal 사망 저장 실패: {saveError}");
            if(SceneLoader.Instance!=null)
            {
                SceneLoader.Instance.ReturnToHub();
                while(!string.Equals(SceneManager.GetActiveScene().name,SceneNames.Hub,System.StringComparison.Ordinal))yield return null;
                yield return null;
            }
            else
            {
                var load=SceneManager.LoadSceneAsync(SceneNames.Hub);while(load!=null && !load.isDone)yield return null;
            }
            if(plan.RestoreNormalProgress && !SaveService.TryRestorePendingSession(out _,out var error))
            {Debug.LogError($"[PlayerDeathScreen] Normal 사망 진행도 복원 실패: {error}");AudioManager.Instance?.PlaySFX("UIInvalid");}
        }
        public bool TryPrepareVictoryReturn(ref string clearSaveError,out string status)
        {
            status=string.Empty;
            if(!Application.CanStreamedLevelBeLoaded(SceneLoader.SCENE_MAIN_MENU)){status="메인메뉴를 불러올 수 없습니다.";return false;}
            if(!string.IsNullOrEmpty(clearSaveError)||SaveService.HasPendingTerminalOperation)
            {
                if(!SaveService.TryMarkFinalBossDefeated(out clearSaveError)){status="클리어 저장 실패. Yes로 다시 시도할 수 있습니다.";return false;}
                clearSaveError=string.Empty;
            }
            return true;
        }
        public bool TrySaveBeforeExit(out string error)=>SaveService.TrySaveActiveRun(out error);
        public void SaveSettings()=>GameSettings.Save();
        public AsyncOperation LoadMainMenu()=>GameplaySessionLifecycle.LoadMainMenu();
        public void Quit()
        {
            InputManager.ExistingInstance?.Contexts.ResetSession();Debug.Log("[PauseMenu] 저장 후 게임 종료 요청");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
    }
}
