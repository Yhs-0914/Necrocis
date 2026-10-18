using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public sealed class GameplaySaveCoordinator : MonoBehaviour
    {
        public static void EnsureOn(GameObject owner)
        {
            if (owner != null && owner.GetComponent<GameplaySaveCoordinator>() == null)
            {
                owner.AddComponent<GameplaySaveCoordinator>();
            }
        }

        // Resolve per operation, including direct facade calls without a coordinator component.
        // A partial capture is valid before a player exists; restore checks all required bindings.
        internal static SessionSaveAdapter CreateSessionSaveAdapter()
        {
            return new SessionSaveAdapter(ResolveSourceBindings());
        }

        internal static SessionSaveAdapter.SourceBindings ResolveSourceBindings()
        {
            PlayerController player = PlayerController.Instance ?? Object.FindFirstObjectByType<PlayerController>();
            PlayerStats stats = PlayerStats.Instance ?? Object.FindFirstObjectByType<PlayerStats>();
            PlayerItemManager items = PlayerItemManager.Instance ?? Object.FindFirstObjectByType<PlayerItemManager>();
            return new SessionSaveAdapter.SourceBindings(
                player,
                stats,
                items,
                GameManager.Instance,
                SceneManager.GetActiveScene().name,
                player != null ? player.GetComponent<PlayerItemCombatEffects>() : items != null ? items.GetComponent<PlayerItemCombatEffects>() : null,
                player != null ? player.GetComponent<PlayerDamageCoordinator>() : null);
        }

        private IEnumerator Start()
        {
            InGameAudioSettings.EnsureCreated();
            yield return null;

            if (!SaveService.IsRestorePending)
            {
                yield break;
            }

            if (!SaveService.TryRestorePendingSession(out BiomeType resumeBiome, out string error))
            {
                Debug.LogError($"[GameplaySaveCoordinator] 저장 복원 실패: {error}");
                yield break;
            }

            if (resumeBiome == BiomeType.None)
            {
                yield break;
            }

            yield return null;
            SceneLoader.Instance?.LoadBiome(resumeBiome);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (string.Equals(scene.name, SceneLoader.SCENE_MAIN_MENU, System.StringComparison.Ordinal)
                || SaveService.IsRestorePending
                || !SaveService.HasActiveSession)
            {
                return;
            }

            SaveService.TrySaveActiveRun(out _);
        }

        private void OnApplicationQuit()
        {
            if (SaveService.HasActiveSession)
            {
                SaveService.TrySaveActiveRun(out _);
            }
        }
    }
}
