using UnityEngine;
namespace Necrocis
{
    // Stateless upper adapter resolves the current actor life for each operation.
    public static class SessionRuntimeBindings
    {
        private sealed class RuntimePort : ISessionRuntimeSavePort
        {
            public void Capture(RunSaveData run)=>GameplaySaveCoordinator.CreateSessionSaveAdapter().Capture(run);
            public bool TryRestore(RunSaveData run,out string error)=>GameplaySaveCoordinator.CreateSessionSaveAdapter().TryRestore(run,out error);
        }
        private static readonly ISessionRuntimeSavePort port=new RuntimePort();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Register()
        {
            SaveService.BindRuntimePort(port);
            SessionActionBindings.Death=GameplaySessionActions.Instance;
            SessionActionBindings.Menu=GameplaySessionActions.Instance;
            PlayerDisplayAdapters.Register();
            PlayerWorldAdapters.Register();
            EnemyWorldAdapters.Register();
            ItemGameplayBindings.Register();
            WorldProgressAdapters.Register();
            PlayerGameplayInstaller.CompositionRequested-=InstallPlayerComposition;
            PlayerGameplayInstaller.CompositionRequested+=InstallPlayerComposition;
        }
        private static void InstallPlayerComposition(GameObject owner)=>PlayerCompositionRoot.EnsureGameplay(owner);
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterEditor()=>Register();
#endif
    }
}
