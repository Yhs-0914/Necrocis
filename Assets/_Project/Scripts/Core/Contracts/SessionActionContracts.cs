using System.Collections;
using UnityEngine;
namespace Necrocis
{
    public readonly struct DeathReturnPlan
    {
        public readonly bool MainMenu,RestoreNormalProgress;
        public DeathReturnPlan(bool menu,bool restore){MainMenu=menu;RestoreNormalProgress=restore;}
    }
    public interface IDeathReturnActions
    {
        bool TryPrepare(out DeathReturnPlan plan,out string error);
        IEnumerator Execute(DeathReturnPlan plan);
    }
    public interface ISessionMenuActions
    {
        bool IsPlayerDead {get;}
        bool TryPrepareVictoryReturn(ref string clearSaveError,out string status);
        bool TrySaveBeforeExit(out string error);
        void SaveSettings();
        AsyncOperation LoadMainMenu();
        void Quit();
    }
    public static class SessionActionBindings
    {
        public static IDeathReturnActions Death {get;set;}
        public static ISessionMenuActions Menu {get;set;}
    }
}
