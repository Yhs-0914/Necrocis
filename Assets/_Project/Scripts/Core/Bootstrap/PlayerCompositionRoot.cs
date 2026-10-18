using UnityEngine;
using UnityEngine.SceneManagement;

namespace Necrocis
{
    // The upper composition boundary owns installation. Gameplay installation alone creates no UI.
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10000)]
    public sealed class PlayerCompositionRoot : MonoBehaviour
    {
        [SerializeField] private bool installHudOnStart;
        private bool installing, installed;
        private PlayerController player;
        private PlayerDamageCoordinator damage;
        private PlayerDeathScreen deathScreen;

        public static PlayerCompositionRoot EnsureGameplay(GameObject owner)
        {
            if (owner == null) throw new System.ArgumentNullException(nameof(owner));
            var root = owner.GetComponent<PlayerCompositionRoot>() ?? owner.AddComponent<PlayerCompositionRoot>();
            root.InstallGameplay();
            return root;
        }

        private void Awake()
        {
            PlayerController existing = PlayerController.ExistingInstance;
            if (existing != null && existing.gameObject != gameObject)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }
            InstallGameplay();
        }

        private void OnEnable()
        {
            DontStarveCamera.Created += HandleCameraCreated;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            if (installed) BindSceneCamera();
        }

        private void OnDisable()
        {
            DontStarveCamera.Created -= HandleCameraCreated;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            OnDisable();
            if (player != null) player.DeathPresentationRequested -= ShowDeathPresentation;
        }

        private void Start()
        {
            if (installHudOnStart)
            {
                InstallHud();
                InstallDevelopmentTools();
            }
            BindSceneCamera();
        }

        private void InstallGameplay()
        {
            if(installing||installed)return;installing=true;
            try{
                PlayerGameplayInstaller.Ensure(gameObject);
                player=GetComponent<PlayerController>();damage=GetComponent<PlayerDamageCoordinator>();installed=true;
            }finally{installing=false;}
        }

        public void InstallHud(bool forcePickupNotifier = false)
        {
            InstallGameplay();
            var items = GetComponent<PlayerItemManager>();
            if (forcePickupNotifier || items.PickupNotificationsEnabled) Ensure<PlayerItemPickupNotifier>();
            Ensure<PlayerItemInventoryUI>();
            Ensure<ExpBarUI>(); Ensure<PlayerHeartUI>();
            deathScreen = Ensure<PlayerDeathScreen>();
            Ensure<global::LevelUpStackChoiceUI>(); Ensure<LevelUpUI>(); Ensure<StatUI>();
            player.DeathPresentationRequested -= ShowDeathPresentation;
            player.DeathPresentationRequested += ShowDeathPresentation;
        }

        public void InstallDevelopmentTools()
        {
            if (!PlayerItemTestPanel.IsAvailable) return;
            InstallGameplay();
            Ensure<PlayerItemTestPanel>();
        }

        private void ShowDeathPresentation() => deathScreen?.ShowDeath();
        private T Ensure<T>() where T : Component => GetComponent<T>() ?? gameObject.AddComponent<T>();
        private void HandleCameraCreated(DontStarveCamera camera)
        {
            if (camera != null && camera.Target == null) BindCamera(camera, false);
        }
        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => BindSceneCamera();
        private void BindSceneCamera()
        {
            var camera = FindFirstObjectByType<DontStarveCamera>();
            if (camera != null && camera.Target == null) BindCamera(camera, false);
        }

        public void BindCamera(DontStarveCamera camera, bool snap)
        {
            if (player == null || camera == null) return;
            camera.SetTarget(player.transform);
            if (snap) camera.SnapToTarget();
        }
    }
}
