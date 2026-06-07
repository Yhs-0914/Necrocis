using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Necrocis
{
    public class GameOverUI : MonoBehaviour
    {
        private static GameOverUI instance;

        [SerializeField] private float fadeInDuration = 1.2f;
        [SerializeField] private float delayBeforeShow = 0.8f;

        private Canvas canvas;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            BuildUI();
            canvasGroup.alpha = 0f;
            canvas.enabled = false;
        }

        private void OnEnable()
        {
            PlayerController.OnPlayerDied += HandlePlayerDied;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            PlayerController.OnPlayerDied -= HandlePlayerDied;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StopAllCoroutines();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvas.enabled = false;
        }

        private void HandlePlayerDied()
        {
            PlayerController player = PlayerController.Instance;
            PlayerDeathScreen deathScreen = player != null ? player.GetComponent<PlayerDeathScreen>() : null;
            if (deathScreen != null && deathScreen.IsShowing)
            {
                return;
            }

            StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            yield return new WaitForSeconds(delayBeforeShow);

            canvas.enabled = true;
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        private void Restart()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.ReturnToHub();
            }
        }

        // UI를 코드로 생성 (프리팹 없이 동작)
        private void BuildUI()
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            gameObject.AddComponent<GraphicRaycaster>();

            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // 어두운 배경
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(transform, false);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.75f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // GAME OVER 텍스트
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(transform, false);
            Text title = titleObj.AddComponent<Text>();
            title.text = "GAME OVER";
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.fontSize = 72;
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = new Color(0.9f, 0.15f, 0.15f, 1f);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.55f);
            titleRect.anchorMax = new Vector2(1f, 0.75f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // 재시작 버튼
            GameObject btnObj = new GameObject("RestartButton");
            btnObj.transform.SetParent(transform, false);
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImage;
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            colors.pressedColor = new Color(0.05f, 0.05f, 0.05f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(Restart);
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.35f, 0.35f);
            btnRect.anchorMax = new Vector2(0.65f, 0.47f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            Text btnText = btnTextObj.AddComponent<Text>();
            btnText.text = "처음으로";
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 32;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
            RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;
        }

        // 씬에 자동 생성 (GameOverUI 오브젝트가 없으면 만들어줌)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (instance != null) return;
            GameObject obj = new GameObject("GameOverUI");
            obj.AddComponent<GameOverUI>();
        }
    }
}
