using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Necrocis
{
    /// <summary>
    /// 씬 전환 관리자
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader instance;

        public static SceneLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<SceneLoader>();
                }

                return instance;
            }
            private set => instance = value;
        }

        [Header("씬 이름")]
        public const string SCENE_MAIN_MENU = "MainMenu";
        public const string SCENE_HUB = "Hub";
        public const string SCENE_INTESTINE = "Intestine";
        public const string SCENE_LIVER = "Liver";
        public const string SCENE_STOMACH = "Stomach";
        public const string SCENE_LUNG = "Lung";

        private bool isLoading = false;
        private bool forceHubRespawn = false;
        private PortalLoadingScreen loadingScreen;

        private const float MinimumLoadingScreenDuration = 0.55f;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// 바이옴 타입으로 씬 이름 가져오기
        /// </summary>
        public static string GetSceneName(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Intestine => SCENE_INTESTINE,
                BiomeType.Liver => SCENE_LIVER,
                BiomeType.Stomach => SCENE_STOMACH,
                BiomeType.Lung => SCENE_LUNG,
                BiomeType.None => SCENE_HUB,  // None이면 허브로
                _ => SCENE_HUB
            };
        }

        /// <summary>
        /// 바이옴으로 이동
        /// </summary>
        public void LoadBiome(BiomeType biome)
        {
            if (isLoading) return;

            string sceneName = GetSceneName(biome);
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        /// <summary>
        /// 허브로 돌아가기
        /// </summary>
        public void ReturnToHub()
        {
            if (isLoading) return;
            forceHubRespawn = true;
            StartCoroutine(LoadSceneAsync(SCENE_HUB));
        }

        /// <summary>
        /// 비동기 씬 로드
        /// </summary>
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            isLoading = true;

            PlayerController loadingPlayer = PlayerController.Instance;
            if (loadingPlayer == null)
            {
                loadingPlayer = FindFirstObjectByType<PlayerController>();
            }

            if (loadingScreen == null)
            {
                loadingScreen = PortalLoadingScreen.Create(transform);
            }

            loadingScreen.Show(
                loadingPlayer != null ? loadingPlayer.GetLoadingRunAnimationFrames() : null,
                loadingPlayer != null ? loadingPlayer.CurrentVisualSprite : null);

            // Give the overlay one rendered frame before scene loading starts.
            yield return null;

            Debug.Log($"[SceneLoader] {sceneName} 씬 로딩 시작...");

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            if (asyncLoad == null)
            {
                Debug.LogError($"[SceneLoader] {sceneName} 씬 로딩을 시작하지 못했습니다.");
                loadingScreen.Hide();
                isLoading = false;
                yield break;
            }

            float displayedProgress = 0f;

            while (!asyncLoad.isDone)
            {
                float actualProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                displayedProgress = Mathf.MoveTowards(
                    displayedProgress,
                    actualProgress,
                    Time.unscaledDeltaTime * 1.8f);
                loadingScreen.SetProgress(displayedProgress);
                yield return null;
            }

            Debug.Log($"[SceneLoader] {sceneName} 씬 로딩 완료!");

            if (sceneName == SCENE_HUB && forceHubRespawn)
            {
                forceHubRespawn = false;
                yield return null;

                PlayerController player = PlayerController.Instance;
                if (player == null)
                {
                    player = FindFirstObjectByType<PlayerController>();
                }

                if (player != null)
                {
                    player.UnlockY();
                    player.SpawnAt(new Vector3(16f, -2f, 7f));
                    SnapCameraToPlayer(player.transform);
                }
            }

            // Finish the bar cleanly and prevent very fast loads from producing a brief flash.
            while (displayedProgress < 1f
                   || loadingScreen.VisibleDuration < MinimumLoadingScreenDuration)
            {
                displayedProgress = Mathf.MoveTowards(
                    displayedProgress,
                    1f,
                    Time.unscaledDeltaTime * 3.5f);
                loadingScreen.SetProgress(displayedProgress);
                yield return null;
            }

            loadingScreen.SetProgress(1f);
            yield return null;
            loadingScreen.Hide();
            isLoading = false;
        }

        private void SnapCameraToPlayer(Transform playerTransform)
        {
            if (playerTransform == null)
            {
                return;
            }

            DontStarveCamera followCamera = DontStarveCamera.Instance;
            if (followCamera == null)
            {
                followCamera = FindFirstObjectByType<DontStarveCamera>();
            }

            if (followCamera == null)
            {
                return;
            }

            followCamera.SetTarget(playerTransform);
            followCamera.SnapToTarget();
        }

        /// <summary>
        /// 현재 씬 이름
        /// </summary>
        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// 현재 허브인지 확인
        /// </summary>
        public bool IsInHub()
        {
            return GetCurrentSceneName() == SCENE_HUB;
        }
    }
}
