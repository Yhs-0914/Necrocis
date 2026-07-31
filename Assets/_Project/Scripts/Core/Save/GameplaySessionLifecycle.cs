using UnityEngine;
using UnityEngine.SceneManagement;

namespace Necrocis
{
    public static class GameplaySessionLifecycle
    {
        public static void EndGameplaySession()
        {
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            DestroyComponentOwner(PlayerController.Instance);
            DestroyComponentOwner(DontStarveCamera.Instance);
            DestroyComponentOwner(GameManager.Instance);
            DestroyComponentOwner(SceneLoader.Instance);
            DestroyComponentOwner(PlayerProjectilePool.Instance);
            DestroyAllOfType<InGameAudioSettings>();
            DestroyAllOfType<GameOverUI>();

            Canvas[] canvases = Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas != null)
                {
                    Object.Destroy(canvas.gameObject);
                }
            }

            RuntimePool.ClearAll();
            SaveService.ClearActiveSessionReference();
            LevelUpManager.ResetProgress();
        }

        public static AsyncOperation LoadMainMenu()
        {
            EndGameplaySession();
            return SceneManager.LoadSceneAsync(SceneLoader.SCENE_MAIN_MENU, LoadSceneMode.Single);
        }

        private static void DestroyAllOfType<T>() where T : Component
        {
            T[] components = Object.FindObjectsByType<T>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < components.Length; i++)
            {
                DestroyComponentOwner(components[i]);
            }
        }

        private static void DestroyComponentOwner(Component component)
        {
            if (component != null)
            {
                Object.Destroy(component.gameObject);
            }
        }
    }
}
