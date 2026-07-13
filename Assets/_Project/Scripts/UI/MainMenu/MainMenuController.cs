using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Necrocis
{
    /// <summary>
    /// Runtime-built title UI. The scene only owns the art references so the layout can
    /// be regenerated without hand-editing a large Unity scene file.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        private const string MainSceneName = SceneLoader.SCENE_HUB;
        private const float ArtworkWidth = 1672f;
        private const float ArtworkHeight = 941f;

        [Header("Artwork")]
        [SerializeField] private Sprite backgroundArtwork;
        [SerializeField] private Sprite lockedBackgroundArtwork;
        [SerializeField] private Sprite playerSprite;
        [SerializeField] private Sprite intestineSilhouette;
        [SerializeField] private Sprite liverSilhouette;
        [SerializeField] private Sprite stomachSilhouette;
        [SerializeField] private Sprite lungSilhouette;
        [SerializeField] private Font menuFont;

        private CanvasGroup mainMenuGroup;
        private GameObject settingsOverlay;
        private Slider volumeSlider;
        private Text volumeValueText;
        private Toggle fullscreenToggle;
        private Button startButton;
        private Button settingsButton;
        private Button quitButton;
        private Button settingsBackButton;
        private Image fadeImage;
        private RectTransform playerRect;
        private Vector3 playerBaseScale = Vector3.one;
        private bool isStarting;

        public Button StartButton => startButton;
        public Button SettingsButton => settingsButton;
        public Button QuitButton => quitButton;
        public GameObject SettingsOverlay => settingsOverlay;

        private void Awake()
        {
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            GameSettings.ApplySaved();

            EnsureEventSystem();
            BuildInterface();
            startButton?.Select();
        }

        private void Update()
        {
            float time = Time.unscaledTime;

            if (playerRect != null)
            {
                playerRect.localScale = playerBaseScale * (1f + Mathf.Sin(time * 2.1f) * 0.035f);
                Vector2 position = playerRect.anchoredPosition;
                position.y = Mathf.Sin(time * 1.75f) * 5f;
                playerRect.anchoredPosition = position;
            }

            if (settingsOverlay != null && settingsOverlay.activeSelf &&
                Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseSettings();
            }
        }

        private void OnApplicationQuit()
        {
            GameSettings.Save();
        }

        private void BuildInterface()
        {
            Canvas canvas = CreateCanvas();
            bool hasAnyDefeatedBoss = HasAnyDefeatedBoss();
            Sprite selectedBackground = !hasAnyDefeatedBoss && lockedBackgroundArtwork != null
                ? lockedBackgroundArtwork
                : backgroundArtwork;
            Image background = CreateImage("MainMenuArtwork", canvas.transform, selectedBackground, Color.white);
            Stretch(background.rectTransform);
            background.raycastTarget = false;

            if (hasAnyDefeatedBoss)
            {
                CreateBossSilhouettes(background.transform);
            }
            CreatePlayerVisual(background.transform);
            CreateMainMenu(canvas.transform);
            CreateSettingsOverlay(canvas.transform);

            fadeImage = CreateImage("SceneFade", canvas.transform, null, Color.black);
            Stretch(fadeImage.rectTransform);
            fadeImage.raycastTarget = true;
            fadeImage.canvasRenderer.SetAlpha(0f);
            fadeImage.gameObject.SetActive(false);
        }

        private Canvas CreateCanvas()
        {
            GameObject canvasObject = CreateUIObject("MainMenuCanvas", transform);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private void CreateBossSilhouettes(Transform artwork)
        {
            CreateBossSilhouette(artwork, BiomeType.Lung, lungSilhouette, new Rect(620f, 0f, 430f, 350f));
            CreateBossSilhouette(artwork, BiomeType.Liver, liverSilhouette, new Rect(940f, 0f, 600f, 420f));
            CreateBossSilhouette(artwork, BiomeType.Intestine, intestineSilhouette, new Rect(860f, 300f, 620f, 530f));
            CreateBossSilhouette(artwork, BiomeType.Stomach, stomachSilhouette, new Rect(1190f, 380f, 482f, 561f));
        }

        private static void CreateBossSilhouette(Transform parent, BiomeType biome, Sprite sprite, Rect sourceRect)
        {
            if (sprite == null || BossProgress.IsDefeated(biome))
            {
                return;
            }

            Image image = CreateImage($"LockedBoss_{biome}", parent, sprite, Color.white);
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(sourceRect.xMin / ArtworkWidth, 1f - sourceRect.yMax / ArtworkHeight);
            rect.anchorMax = new Vector2(sourceRect.xMax / ArtworkWidth, 1f - sourceRect.yMin / ArtworkHeight);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            image.preserveAspect = false;
            image.raycastTarget = false;
        }

        private static bool HasAnyDefeatedBoss()
        {
            return BossProgress.IsDefeated(BiomeType.Intestine)
                   || BossProgress.IsDefeated(BiomeType.Liver)
                   || BossProgress.IsDefeated(BiomeType.Stomach)
                   || BossProgress.IsDefeated(BiomeType.Lung);
        }

        private void CreatePlayerVisual(Transform artwork)
        {
            if (playerSprite == null)
            {
                return;
            }

            Image player = CreateImage("PlayerVirus", artwork, playerSprite, Color.white);
            playerRect = player.rectTransform;
            playerRect.anchorMin = new Vector2(0.503f, 0.167f);
            playerRect.anchorMax = playerRect.anchorMin;
            playerRect.pivot = new Vector2(0.5f, 0.5f);
            playerRect.anchoredPosition = Vector2.zero;
            playerRect.sizeDelta = new Vector2(120f, 120f);
            player.preserveAspect = true;
            player.raycastTarget = false;

            Outline outline = player.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.12f, 0.01f, 0.025f, 0.95f);
            outline.effectDistance = new Vector2(4f, -4f);
            playerBaseScale = playerRect.localScale;
        }

        private void CreateMainMenu(Transform canvas)
        {
            GameObject menuRoot = CreateUIObject("MainMenuActions", canvas);
            RectTransform menuRect = menuRoot.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(0.075f, 0.13f);
            menuRect.anchorMax = new Vector2(0.385f, 0.47f);
            menuRect.offsetMin = Vector2.zero;
            menuRect.offsetMax = Vector2.zero;

            Image backdrop = menuRoot.AddComponent<Image>();
            backdrop.color = new Color(0.018f, 0.008f, 0.014f, 0.9f);
            backdrop.raycastTarget = false;
            Outline backdropOutline = menuRoot.AddComponent<Outline>();
            backdropOutline.effectColor = new Color(0.52f, 0.08f, 0.055f, 0.65f);
            backdropOutline.effectDistance = new Vector2(2f, -2f);

            mainMenuGroup = menuRoot.AddComponent<CanvasGroup>();

            startButton = CreateMenuButton(menuRoot.transform, "StartButton", "게임 시작", new Vector2(0f, 78f));
            settingsButton = CreateMenuButton(menuRoot.transform, "SettingsButton", "설정", Vector2.zero);
            quitButton = CreateMenuButton(menuRoot.transform, "QuitButton", "종료", new Vector2(0f, -78f));

            startButton.onClick.AddListener(StartGame);
            settingsButton.onClick.AddListener(OpenSettings);
            quitButton.onClick.AddListener(QuitGame);

            SetVerticalNavigation(startButton, quitButton, settingsButton);
            SetVerticalNavigation(settingsButton, startButton, quitButton);
            SetVerticalNavigation(quitButton, settingsButton, startButton);
        }

        private Button CreateMenuButton(Transform parent, string objectName, string label, Vector2 position)
        {
            GameObject buttonObject = CreateUIObject(objectName, parent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(470f, 66f);
            rect.anchoredPosition = position;

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.14f, 0.035f, 0.035f, 0.42f);

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.38f, 0.2f, 0.19f, 0.48f);
            colors.highlightedColor = new Color(0.82f, 0.37f, 0.13f, 0.8f);
            colors.pressedColor = new Color(0.95f, 0.61f, 0.23f, 0.92f);
            colors.selectedColor = new Color(0.76f, 0.29f, 0.1f, 0.85f);
            colors.disabledColor = new Color(0.14f, 0.1f, 0.1f, 0.35f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            Outline border = buttonObject.AddComponent<Outline>();
            border.effectColor = new Color(0.8f, 0.48f, 0.16f, 0.5f);
            border.effectDistance = new Vector2(1.5f, -1.5f);

            Text text = CreateText("Label", buttonObject.transform, label, 38, new Color(1f, 0.87f, 0.62f, 1f));
            Stretch(text.rectTransform, new Vector2(22f, 4f), new Vector2(-22f, -4f));
            text.alignment = TextAnchor.MiddleLeft;
            return button;
        }

        private void CreateSettingsOverlay(Transform canvas)
        {
            settingsOverlay = CreateUIObject("SettingsOverlay", canvas);
            RectTransform overlayRect = settingsOverlay.GetComponent<RectTransform>();
            Stretch(overlayRect);
            Image overlayDim = settingsOverlay.AddComponent<Image>();
            overlayDim.color = new Color(0.005f, 0.002f, 0.006f, 0.84f);

            GameObject panel = CreateUIObject("SettingsPanel", settingsOverlay.transform);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = panelRect.anchorMin;
            panelRect.sizeDelta = new Vector2(720f, 480f);
            panelRect.anchoredPosition = Vector2.zero;
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.055f, 0.012f, 0.024f, 0.98f);
            Outline panelOutline = panel.AddComponent<Outline>();
            panelOutline.effectColor = new Color(0.67f, 0.12f, 0.08f, 0.95f);
            panelOutline.effectDistance = new Vector2(3f, -3f);

            Text title = CreateText("SettingsTitle", panel.transform, "설정", 48, new Color(1f, 0.84f, 0.53f, 1f));
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(600f, 70f), new Vector2(0f, 172f));
            title.alignment = TextAnchor.MiddleCenter;

            Text volumeLabel = CreateText("VolumeLabel", panel.transform, "전체 음량", 30, new Color(0.92f, 0.82f, 0.72f, 1f));
            SetRect(volumeLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(250f, 52f), new Vector2(-190f, 80f));
            volumeLabel.alignment = TextAnchor.MiddleLeft;

            volumeSlider = CreateSlider(panel.transform, new Vector2(62f, 80f));
            volumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
            volumeSlider.onValueChanged.AddListener(HandleVolumeChanged);

            volumeValueText = CreateText("VolumeValue", panel.transform, $"{Mathf.RoundToInt(GameSettings.MasterVolume * 100f)}%", 28, new Color(1f, 0.72f, 0.37f, 1f));
            SetRect(volumeValueText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(90f, 52f), new Vector2(278f, 80f));
            volumeValueText.alignment = TextAnchor.MiddleRight;

            Text fullscreenLabel = CreateText("FullscreenLabel", panel.transform, "전체 화면", 30, new Color(0.92f, 0.82f, 0.72f, 1f));
            SetRect(fullscreenLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(260f, 52f), new Vector2(-185f, -20f));
            fullscreenLabel.alignment = TextAnchor.MiddleLeft;

            fullscreenToggle = CreateToggle(panel.transform, new Vector2(238f, -20f));
            fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);
            fullscreenToggle.onValueChanged.AddListener(HandleFullscreenChanged);

            settingsBackButton = CreateMenuButton(panel.transform, "SettingsBackButton", "돌아가기", new Vector2(0f, -155f));
            RectTransform backRect = settingsBackButton.GetComponent<RectTransform>();
            backRect.sizeDelta = new Vector2(360f, 62f);
            Text backLabel = settingsBackButton.GetComponentInChildren<Text>();
            backLabel.alignment = TextAnchor.MiddleCenter;
            settingsBackButton.onClick.AddListener(CloseSettings);

            settingsOverlay.SetActive(false);
        }

        private Slider CreateSlider(Transform parent, Vector2 position)
        {
            GameObject sliderObject = CreateUIObject("MasterVolumeSlider", parent);
            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            SetRect(sliderRect, new Vector2(0.5f, 0.5f), new Vector2(360f, 42f), position);

            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.direction = Slider.Direction.LeftToRight;

            Image background = CreateImage("Background", sliderObject.transform, null, new Color(0.14f, 0.04f, 0.055f, 1f));
            Stretch(background.rectTransform, new Vector2(0f, 13f), new Vector2(0f, -13f));

            GameObject fillArea = CreateUIObject("Fill Area", sliderObject.transform);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            Stretch(fillAreaRect, new Vector2(8f, 13f), new Vector2(-8f, -13f));
            Image fill = CreateImage("Fill", fillArea.transform, null, new Color(0.88f, 0.29f, 0.1f, 1f));
            Stretch(fill.rectTransform);

            GameObject handleArea = CreateUIObject("Handle Slide Area", sliderObject.transform);
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            Stretch(handleAreaRect, new Vector2(12f, 0f), new Vector2(-12f, 0f));
            Image handle = CreateImage("Handle", handleArea.transform, null, new Color(1f, 0.76f, 0.33f, 1f));
            RectTransform handleRect = handle.rectTransform;
            handleRect.anchorMin = new Vector2(0.5f, 0.5f);
            handleRect.anchorMax = handleRect.anchorMin;
            handleRect.sizeDelta = new Vector2(24f, 38f);

            slider.fillRect = fill.rectTransform;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            return slider;
        }

        private Toggle CreateToggle(Transform parent, Vector2 position)
        {
            GameObject toggleObject = CreateUIObject("FullscreenToggle", parent);
            RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();
            SetRect(toggleRect, new Vector2(0.5f, 0.5f), new Vector2(86f, 52f), position);

            Toggle toggle = toggleObject.AddComponent<Toggle>();
            Image background = CreateImage("Background", toggleObject.transform, null, new Color(0.14f, 0.04f, 0.055f, 1f));
            SetRect(background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(44f, 44f), Vector2.zero);
            Outline outline = background.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.76f, 0.26f, 0.09f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            Image checkmark = CreateImage("Checkmark", background.transform, null, new Color(1f, 0.7f, 0.25f, 1f));
            Stretch(checkmark.rectTransform, new Vector2(9f, 9f), new Vector2(-9f, -9f));

            toggle.targetGraphic = background;
            toggle.graphic = checkmark;
            return toggle;
        }

        private void OpenSettings()
        {
            if (isStarting || settingsOverlay == null)
            {
                return;
            }

            mainMenuGroup.interactable = false;
            mainMenuGroup.blocksRaycasts = false;
            settingsOverlay.SetActive(true);
            volumeSlider?.Select();
        }

        private void CloseSettings()
        {
            if (settingsOverlay == null || !settingsOverlay.activeSelf)
            {
                return;
            }

            GameSettings.Save();
            settingsOverlay.SetActive(false);
            mainMenuGroup.interactable = true;
            mainMenuGroup.blocksRaycasts = true;
            settingsButton?.Select();
        }

        private void HandleVolumeChanged(float value)
        {
            GameSettings.SetMasterVolume(value);
            if (volumeValueText != null)
            {
                volumeValueText.text = $"{Mathf.RoundToInt(value * 100f)}%";
            }
        }

        private void HandleFullscreenChanged(bool enabled)
        {
            GameSettings.SetFullscreen(enabled);
        }

        public void StartGame()
        {
            if (!isStarting)
            {
                StartCoroutine(StartGameRoutine());
            }
        }

        private IEnumerator StartGameRoutine()
        {
            isStarting = true;
            GameSettings.Save();
            mainMenuGroup.interactable = false;
            mainMenuGroup.blocksRaycasts = false;

            fadeImage.gameObject.SetActive(true);
            fadeImage.canvasRenderer.SetAlpha(0f);
            fadeImage.CrossFadeAlpha(1f, 0.45f, true);
            yield return new WaitForSecondsRealtime(0.46f);

            if (!Application.CanStreamedLevelBeLoaded(MainSceneName))
            {
                Debug.LogError($"[MainMenu] '{MainSceneName}' 씬을 Build Settings에서 찾을 수 없습니다.");
                fadeImage.CrossFadeAlpha(0f, 0.2f, true);
                yield return new WaitForSecondsRealtime(0.2f);
                fadeImage.gameObject.SetActive(false);
                mainMenuGroup.interactable = true;
                mainMenuGroup.blocksRaycasts = true;
                isStarting = false;
                yield break;
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(MainSceneName, LoadSceneMode.Single);
            while (loadOperation != null && !loadOperation.isDone)
            {
                yield return null;
            }
        }

        public void QuitGame()
        {
            if (isStarting)
            {
                return;
            }

            GameSettings.Save();
            Debug.Log("[MainMenu] 게임 종료 요청");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private Text CreateText(string objectName, Transform parent, string value, int fontSize, Color color)
        {
            GameObject textObject = CreateUIObject(objectName, parent);
            Text text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = menuFont != null ? menuFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Normal;
            text.color = color;
            text.supportRichText = false;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            Shadow shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.08f, 0.005f, 0.012f, 0.95f);
            shadow.effectDistance = new Vector2(3f, -3f);
            return text;
        }

        private static Image CreateImage(string objectName, Transform parent, Sprite sprite, Color color)
        {
            GameObject imageObject = CreateUIObject(objectName, parent);
            Image image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            return image;
        }

        private static GameObject CreateUIObject(string objectName, Transform parent)
        {
            GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }

        private static void Stretch(RectTransform rect)
        {
            Stretch(rect, Vector2.zero, Vector2.zero);
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 position)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void SetVerticalNavigation(Selectable selectable, Selectable up, Selectable down)
        {
            Navigation navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = up;
            navigation.selectOnDown = down;
            selectable.navigation = navigation;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }

    }
}
