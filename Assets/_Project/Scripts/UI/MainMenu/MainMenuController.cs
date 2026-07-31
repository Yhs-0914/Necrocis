using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Necrocis.RuntimeUiFactory;

namespace Necrocis
{
    /// <summary>
    /// Runtime-built title UI. The scene owns only artwork references so the menu can
    /// be regenerated without hand-editing a large scene hierarchy.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        private const string MainSceneName = SceneLoader.SCENE_HUB;
        private const float ArtworkWidth = 1672f;
        private const float ArtworkHeight = 941f;
        private const float FourButtonTopY = 117f;
        private const float FourButtonStepY = 78f;
        private const float ThreeButtonTopY = 78f;

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
        private GameObject difficultyOverlay;
        private GameObject continueOverlay;
        private GameObject confirmationOverlay;
        private Slider volumeSlider;
        private Text volumeValueText;
        private Toggle fullscreenToggle;
        private Button continueButton;
        private Button newGameButton;
        private Button settingsButton;
        private Button quitButton;
        private Button settingsBackButton;
        private Button normalDifficultyButton;
        private Button hardDifficultyButton;
        private Button difficultyBackButton;
        private Button normalContinueButton;
        private Button hardContinueButton;
        private Button continueBackButton;
        private Button confirmButton;
        private Button cancelButton;
        private Text confirmationBody;
        private Text menuStatusText;
        private Image fadeImage;
        private RectTransform playerRect;
        private Vector3 playerBaseScale = Vector3.one;
        private Action pendingConfirmation;
        private bool isStarting;

        // StartButton is retained for the existing smoke runner and now maps to New Game.
        public Button StartButton => newGameButton;
        public Button ContinueButton => continueButton;
        public Button NewGameButton => newGameButton;
        public Button SettingsButton => settingsButton;
        public Button QuitButton => quitButton;
        public Button NormalDifficultyButton => normalDifficultyButton;
        public Button HardDifficultyButton => hardDifficultyButton;
        public GameObject SettingsOverlay => settingsOverlay;
        public GameObject DifficultyOverlay => difficultyOverlay;
        public GameObject ContinueOverlay => continueOverlay;

        private void Awake()
        {
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            SaveService.EnsureInitialized();
            GameSettings.ApplySaved();

            EnsureEventSystem();
            BuildInterface();
            RefreshContinueState();
            (continueButton != null && continueButton.interactable ? continueButton : newGameButton)?.Select();
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

            if (isStarting
                || Keyboard.current == null
                || !Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            if (confirmationOverlay != null && confirmationOverlay.activeSelf)
            {
                CloseConfirmation();
            }
            else if (continueOverlay != null && continueOverlay.activeSelf)
            {
                CloseContinueSelection();
            }
            else if (difficultyOverlay != null && difficultyOverlay.activeSelf)
            {
                CloseDifficultySelection();
            }
            else if (settingsOverlay != null && settingsOverlay.activeSelf)
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
            bool hasAnyDefeatedBoss = HasAnyDiscoveredBoss();
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
            CreateDifficultyOverlay(canvas.transform);
            CreateContinueOverlay(canvas.transform);
            CreateConfirmationOverlay(canvas.transform);

            fadeImage = CreateImage("SceneFade", canvas.transform, null, Color.black);
            Stretch(fadeImage.rectTransform);
            fadeImage.raycastTarget = true;
            fadeImage.canvasRenderer.SetAlpha(0f);
            fadeImage.gameObject.SetActive(false);
        }

        private Canvas CreateCanvas()
        {
            GameObject canvasObject = CreateUiObject("MainMenuCanvas", transform);
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
            if (sprite == null || SaveService.IsBossDiscovered(biome))
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

        private static bool HasAnyDiscoveredBoss()
        {
            return SaveService.IsBossDiscovered(BiomeType.Intestine)
                   || SaveService.IsBossDiscovered(BiomeType.Liver)
                   || SaveService.IsBossDiscovered(BiomeType.Stomach)
                   || SaveService.IsBossDiscovered(BiomeType.Lung);
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
            GameObject menuRoot = CreateUiObject("MainMenuActions", canvas);
            RectTransform menuRect = menuRoot.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(0.075f, 0.09f);
            menuRect.anchorMax = new Vector2(0.385f, 0.51f);
            menuRect.offsetMin = Vector2.zero;
            menuRect.offsetMax = Vector2.zero;

            Image backdrop = menuRoot.AddComponent<Image>();
            backdrop.color = new Color(0.018f, 0.008f, 0.014f, 0.9f);
            backdrop.raycastTarget = false;
            Outline backdropOutline = menuRoot.AddComponent<Outline>();
            backdropOutline.effectColor = new Color(0.52f, 0.08f, 0.055f, 0.65f);
            backdropOutline.effectDistance = new Vector2(2f, -2f);
            mainMenuGroup = menuRoot.AddComponent<CanvasGroup>();

            continueButton = CreateMenuButton(menuRoot.transform, "ContinueButton", "계속하기", new Vector2(0f, 117f));
            newGameButton = CreateMenuButton(menuRoot.transform, "NewGameButton", "새 게임", new Vector2(0f, 39f));
            settingsButton = CreateMenuButton(menuRoot.transform, "SettingsButton", "설정", new Vector2(0f, -39f));
            quitButton = CreateMenuButton(menuRoot.transform, "QuitButton", "종료", new Vector2(0f, -117f));

            continueButton.onClick.AddListener(OpenContinueSelection);
            newGameButton.onClick.AddListener(StartGame);
            settingsButton.onClick.AddListener(OpenSettings);
            quitButton.onClick.AddListener(QuitGame);
            continueButton.onClick.AddListener(PlayButtonClick);
            newGameButton.onClick.AddListener(PlayButtonClick);
            quitButton.onClick.AddListener(PlayButtonClick);

            menuStatusText = CreateText(
                "MenuStatus",
                menuRoot.transform,
                string.Empty,
                22,
                new Color(1f, 0.63f, 0.29f, 1f));
            SetRect(menuStatusText.rectTransform, new Vector2(0.5f, 0f), new Vector2(500f, 38f), new Vector2(0f, 18f));
            menuStatusText.alignment = TextAnchor.MiddleCenter;
        }

        private void CreateSettingsOverlay(Transform canvas)
        {
            settingsOverlay = CreateOverlay(canvas, "SettingsOverlay");
            GameObject panel = CreatePanel(settingsOverlay.transform, "SettingsPanel", new Vector2(720f, 480f));

            Text title = CreateText("SettingsTitle", panel.transform, "설정", 48, new Color(1f, 0.84f, 0.53f, 1f));
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(600f, 70f), new Vector2(0f, 172f));
            title.alignment = TextAnchor.MiddleCenter;

            Text volumeLabel = CreateText("VolumeLabel", panel.transform, "전체 음량", 30, new Color(0.92f, 0.82f, 0.72f, 1f));
            SetRect(volumeLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(250f, 52f), new Vector2(-190f, 65f));
            volumeLabel.alignment = TextAnchor.MiddleLeft;

            volumeSlider = CreateSlider(panel.transform, new Vector2(62f, 65f));
            volumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
            volumeSlider.onValueChanged.AddListener(HandleVolumeChanged);

            volumeValueText = CreateText(
                "VolumeValue",
                panel.transform,
                $"{Mathf.RoundToInt(GameSettings.MasterVolume * 100f)}%",
                28,
                new Color(1f, 0.72f, 0.37f, 1f));
            SetRect(volumeValueText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(90f, 52f), new Vector2(278f, 65f));
            volumeValueText.alignment = TextAnchor.MiddleRight;

            Text fullscreenLabel = CreateText("FullscreenLabel", panel.transform, "전체 화면", 30, new Color(0.92f, 0.82f, 0.72f, 1f));
            SetRect(fullscreenLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(260f, 52f), new Vector2(-185f, -35f));
            fullscreenLabel.alignment = TextAnchor.MiddleLeft;

            fullscreenToggle = CreateToggle(panel.transform, new Vector2(238f, -35f));
            fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);
            fullscreenToggle.onValueChanged.AddListener(HandleFullscreenChanged);

            settingsBackButton = CreateMenuButton(panel.transform, "SettingsBackButton", "돌아가기", new Vector2(0f, -150f));
            settingsBackButton.GetComponent<RectTransform>().sizeDelta = new Vector2(360f, 52f);
            settingsBackButton.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleCenter;
            settingsBackButton.onClick.AddListener(CloseSettings);
            settingsOverlay.SetActive(false);
        }

        private void CreateDifficultyOverlay(Transform canvas)
        {
            difficultyOverlay = CreateOverlay(canvas, "DifficultyOverlay");
            GameObject panel = CreatePanel(difficultyOverlay.transform, "DifficultyPanel", new Vector2(820f, 590f));
            CreateModalTitle(panel.transform, "난이도 선택", 215f);

            Text description = CreateText(
                "DifficultyDescription",
                panel.transform,
                "Normal은 진행을 유지합니다.\nHard는 사망하면 Run 전체가 초기화됩니다.",
                27,
                new Color(0.92f, 0.82f, 0.72f, 1f));
            SetRect(description.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(680f, 90f), new Vector2(0f, 115f));
            description.alignment = TextAnchor.MiddleCenter;

            normalDifficultyButton = CreateMenuButton(
                panel.transform,
                "NormalDifficultyButton",
                "Normal",
                new Vector2(0f, 15f));
            hardDifficultyButton = CreateMenuButton(
                panel.transform,
                "HardDifficultyButton",
                "Hard",
                new Vector2(0f, -70f));
            difficultyBackButton = CreateMenuButton(
                panel.transform,
                "DifficultyBackButton",
                "돌아가기",
                new Vector2(0f, -180f));
            difficultyBackButton.GetComponent<RectTransform>().sizeDelta = new Vector2(330f, 54f);

            normalDifficultyButton.onClick.AddListener(() => RequestNewGame(GameDifficulty.Normal));
            hardDifficultyButton.onClick.AddListener(() => RequestNewGame(GameDifficulty.Hard));
            difficultyBackButton.onClick.AddListener(CloseDifficultySelection);
            SetVerticalNavigation(normalDifficultyButton, difficultyBackButton, hardDifficultyButton);
            SetVerticalNavigation(hardDifficultyButton, normalDifficultyButton, difficultyBackButton);
            SetVerticalNavigation(difficultyBackButton, hardDifficultyButton, normalDifficultyButton);
            difficultyOverlay.SetActive(false);
        }

        private void CreateContinueOverlay(Transform canvas)
        {
            continueOverlay = CreateOverlay(canvas, "ContinueOverlay");
            GameObject panel = CreatePanel(continueOverlay.transform, "ContinuePanel", new Vector2(820f, 590f));
            CreateModalTitle(panel.transform, "계속하기", 215f);

            normalContinueButton = CreateMenuButton(
                panel.transform,
                "NormalContinueButton",
                "Normal",
                new Vector2(0f, 75f));
            hardContinueButton = CreateMenuButton(
                panel.transform,
                "HardContinueButton",
                "Hard",
                new Vector2(0f, -30f));
            normalContinueButton.GetComponent<RectTransform>().sizeDelta = new Vector2(620f, 88f);
            hardContinueButton.GetComponent<RectTransform>().sizeDelta = new Vector2(620f, 88f);
            normalContinueButton.GetComponentInChildren<Text>().fontSize = 27;
            hardContinueButton.GetComponentInChildren<Text>().fontSize = 27;

            continueBackButton = CreateMenuButton(
                panel.transform,
                "ContinueBackButton",
                "돌아가기",
                new Vector2(0f, -180f));
            continueBackButton.GetComponent<RectTransform>().sizeDelta = new Vector2(330f, 54f);

            normalContinueButton.onClick.AddListener(() => BeginContinue(GameDifficulty.Normal));
            hardContinueButton.onClick.AddListener(() => BeginContinue(GameDifficulty.Hard));
            continueBackButton.onClick.AddListener(CloseContinueSelection);
            SetVerticalNavigation(normalContinueButton, continueBackButton, hardContinueButton);
            SetVerticalNavigation(hardContinueButton, normalContinueButton, continueBackButton);
            SetVerticalNavigation(continueBackButton, hardContinueButton, normalContinueButton);
            continueOverlay.SetActive(false);
        }

        private void CreateConfirmationOverlay(Transform canvas)
        {
            confirmationOverlay = CreateOverlay(canvas, "ConfirmationOverlay");
            GameObject panel = CreatePanel(confirmationOverlay.transform, "ConfirmationPanel", new Vector2(780f, 480f));
            CreateModalTitle(panel.transform, "진행 초기화 확인", 165f);

            confirmationBody = CreateText(
                "ConfirmationBody",
                panel.transform,
                string.Empty,
                29,
                new Color(0.95f, 0.85f, 0.72f, 1f));
            SetRect(confirmationBody.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(650f, 150f), new Vector2(0f, 35f));
            confirmationBody.alignment = TextAnchor.MiddleCenter;

            confirmButton = CreateMenuButton(panel.transform, "ConfirmButton", "처음부터 시작", new Vector2(-175f, -135f));
            cancelButton = CreateMenuButton(panel.transform, "CancelButton", "취소", new Vector2(175f, -135f));
            confirmButton.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 58f);
            cancelButton.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 58f);
            confirmButton.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleCenter;
            cancelButton.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleCenter;
            confirmButton.onClick.AddListener(ConfirmPendingAction);
            cancelButton.onClick.AddListener(CloseConfirmation);
            confirmationOverlay.SetActive(false);
        }

        private void CreateModalTitle(Transform parent, string value, float y)
        {
            Text title = CreateText("Title", parent, value, 48, new Color(1f, 0.84f, 0.53f, 1f));
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(650f, 70f), new Vector2(0f, y));
            title.alignment = TextAnchor.MiddleCenter;
        }

        private GameObject CreateOverlay(Transform canvas, string objectName)
        {
            GameObject overlay = CreateUiObject(objectName, canvas);
            Stretch(overlay.GetComponent<RectTransform>());
            Image dim = overlay.AddComponent<Image>();
            dim.color = new Color(0.005f, 0.002f, 0.006f, 0.84f);
            return overlay;
        }

        private GameObject CreatePanel(Transform parent, string objectName, Vector2 size)
        {
            GameObject panel = CreateUiObject(objectName, parent);
            SetRect(panel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), size, Vector2.zero);
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.055f, 0.012f, 0.024f, 0.98f);
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.67f, 0.12f, 0.08f, 0.95f);
            outline.effectDistance = new Vector2(3f, -3f);
            return panel;
        }

        private void RefreshContinueState()
        {
            IReadOnlyList<ContinueSaveSummary> summaries = SaveService.GetContinueSummaries();
            bool hasContinueSave = summaries.Count > 0;
            continueButton.gameObject.SetActive(hasContinueSave);
            continueButton.interactable = hasContinueSave;
            menuStatusText.gameObject.SetActive(hasContinueSave);
            menuStatusText.text = hasContinueSave ? BuildLatestSummary(summaries) : string.Empty;
            ApplyMainMenuLayout(hasContinueSave);
        }

        private void ApplyMainMenuLayout(bool showContinue)
        {
            if (showContinue)
            {
                SetButtonY(continueButton, FourButtonTopY);
                SetButtonY(newGameButton, FourButtonTopY - FourButtonStepY);
                SetButtonY(settingsButton, FourButtonTopY - FourButtonStepY * 2f);
                SetButtonY(quitButton, FourButtonTopY - FourButtonStepY * 3f);

                SetVerticalNavigation(continueButton, quitButton, newGameButton);
                SetVerticalNavigation(newGameButton, continueButton, settingsButton);
                SetVerticalNavigation(settingsButton, newGameButton, quitButton);
                SetVerticalNavigation(quitButton, settingsButton, continueButton);
                return;
            }

            SetButtonY(newGameButton, ThreeButtonTopY);
            SetButtonY(settingsButton, 0f);
            SetButtonY(quitButton, -ThreeButtonTopY);

            SetVerticalNavigation(newGameButton, quitButton, settingsButton);
            SetVerticalNavigation(settingsButton, newGameButton, quitButton);
            SetVerticalNavigation(quitButton, settingsButton, newGameButton);
        }

        private static void SetButtonY(Button button, float y)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            Vector2 position = rect.anchoredPosition;
            position.y = y;
            rect.anchoredPosition = position;
        }

        private static string BuildLatestSummary(IReadOnlyList<ContinueSaveSummary> summaries)
        {
            if (summaries == null || summaries.Count == 0)
            {
                return string.Empty;
            }

            ContinueSaveSummary latest = summaries
                .OrderByDescending(summary => summary.LastSavedUtcTicks)
                .First();
            return $"{latest.Difficulty} · Lv.{latest.Level} · 보스 {latest.DefeatedBossCount}/4";
        }

        public void StartGame()
        {
            OpenDifficultySelection();
        }

        private void OpenDifficultySelection()
        {
            if (isStarting || difficultyOverlay == null)
            {
                return;
            }

            SetMainMenuEnabled(false);
            normalDifficultyButton.GetComponentInChildren<Text>().text = SaveService.HasContinueSave(GameDifficulty.Normal)
                ? "Normal · 진행을 새로 시작"
                : "Normal · 새 게임";
            bool hardUnlocked = SaveService.IsHardUnlocked;
            hardDifficultyButton.interactable = hardUnlocked;
            hardDifficultyButton.GetComponentInChildren<Text>().text = hardUnlocked
                ? (SaveService.HasContinueSave(GameDifficulty.Hard)
                    ? "Hard · 현재 Run 포기 후 시작"
                    : "Hard · 새 Run")
                : "Hard · Normal 최종 보스 클리어 후 해금";
            difficultyOverlay.SetActive(true);
            normalDifficultyButton.Select();
        }

        private void CloseDifficultySelection()
        {
            if (difficultyOverlay == null || !difficultyOverlay.activeSelf)
            {
                return;
            }

            difficultyOverlay.SetActive(false);
            SetMainMenuEnabled(true);
            newGameButton.Select();
            AudioManager.Instance?.PlaySFX("UIClose");
        }

        private void OpenContinueSelection()
        {
            if (isStarting)
            {
                return;
            }

            IReadOnlyList<ContinueSaveSummary> summaries = SaveService.GetContinueSummaries();
            if (summaries.Count == 0)
            {
                menuStatusText.text = "계속할 수 있는 저장이 없습니다";
                AudioManager.Instance?.PlaySFX("UIInvalid");
                return;
            }

            if (summaries.Count == 1)
            {
                BeginContinue(summaries[0].Difficulty);
                return;
            }

            ContinueSaveSummary normalSummary = summaries.First(summary => summary.Difficulty == GameDifficulty.Normal);
            ContinueSaveSummary hardSummary = summaries.First(summary => summary.Difficulty == GameDifficulty.Hard);
            normalContinueButton.GetComponentInChildren<Text>().text = FormatContinueSummary(normalSummary);
            hardContinueButton.GetComponentInChildren<Text>().text = FormatContinueSummary(hardSummary);
            SetMainMenuEnabled(false);
            continueOverlay.SetActive(true);
            (SaveService.Profile.lastPlayedDifficulty == GameDifficulty.Hard
                ? hardContinueButton
                : normalContinueButton).Select();
        }

        private static string FormatContinueSummary(ContinueSaveSummary summary)
        {
            string job = summary.Job == JobType.None ? "미전직" : summary.Job.ToString();
            return $"{summary.Difficulty} · Lv.{summary.Level} {job} · 보스 {summary.DefeatedBossCount}/4";
        }

        private void CloseContinueSelection()
        {
            if (continueOverlay == null || !continueOverlay.activeSelf)
            {
                return;
            }

            continueOverlay.SetActive(false);
            SetMainMenuEnabled(true);
            continueButton.Select();
            AudioManager.Instance?.PlaySFX("UIClose");
        }

        private void RequestNewGame(GameDifficulty difficulty)
        {
            if (difficulty == GameDifficulty.Hard && !SaveService.IsHardUnlocked)
            {
                AudioManager.Instance?.PlaySFX("UIInvalid");
                return;
            }

            if (!SaveService.HasContinueSave(difficulty))
            {
                BeginNewGame(difficulty);
                return;
            }

            ContinueSaveSummary summary = SaveService.GetContinueSummaries()
                .First(value => value.Difficulty == difficulty);
            confirmationBody.text =
                $"{difficulty} 진행을 처음부터 다시 시작하시겠습니까?\n"
                + $"현재 Lv.{summary.Level}, 보스 {summary.DefeatedBossCount}/4 진행이 삭제됩니다.\n"
                + "Hard 해금·메인 화면 공개 그림·설정은 유지됩니다.";
            pendingConfirmation = () => BeginNewGame(difficulty);
            difficultyOverlay.SetActive(false);
            confirmationOverlay.SetActive(true);
            cancelButton.Select();
        }

        private void ConfirmPendingAction()
        {
            Action action = pendingConfirmation;
            pendingConfirmation = null;
            confirmationOverlay.SetActive(false);
            action?.Invoke();
        }

        private void CloseConfirmation()
        {
            if (confirmationOverlay == null || !confirmationOverlay.activeSelf)
            {
                return;
            }

            pendingConfirmation = null;
            confirmationOverlay.SetActive(false);
            difficultyOverlay.SetActive(true);
            normalDifficultyButton.Select();
            AudioManager.Instance?.PlaySFX("UIClose");
        }

        private void BeginNewGame(GameDifficulty difficulty)
        {
            if (!SaveService.TryBeginNewGame(difficulty, out string error))
            {
                ShowMenuError(error);
                return;
            }

            StartCoroutine(StartGameRoutine());
        }

        private void BeginContinue(GameDifficulty difficulty)
        {
            if (!SaveService.TryContinue(difficulty, out string error))
            {
                ShowMenuError(error);
                return;
            }

            StartCoroutine(StartGameRoutine());
        }

        private void ShowMenuError(string error)
        {
            difficultyOverlay.SetActive(false);
            continueOverlay.SetActive(false);
            confirmationOverlay.SetActive(false);
            SetMainMenuEnabled(true);
            menuStatusText.text = string.IsNullOrWhiteSpace(error) ? "작업을 완료하지 못했습니다" : error;
            AudioManager.Instance?.PlaySFX("UIInvalid");
        }

        private IEnumerator StartGameRoutine()
        {
            if (isStarting)
            {
                yield break;
            }

            isStarting = true;
            GameSettings.Save();
            SetMainMenuEnabled(false);
            settingsOverlay.SetActive(false);
            difficultyOverlay.SetActive(false);
            continueOverlay.SetActive(false);
            confirmationOverlay.SetActive(false);

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
                SetMainMenuEnabled(true);
                isStarting = false;
                yield break;
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(MainSceneName, LoadSceneMode.Single);
            while (loadOperation != null && !loadOperation.isDone)
            {
                yield return null;
            }
        }

        private void OpenSettings()
        {
            if (isStarting || settingsOverlay == null)
            {
                return;
            }

            SetMainMenuEnabled(false);
            settingsOverlay.SetActive(true);
            AudioManager.Instance?.PlaySFX("UIClick");
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
            AudioManager.Instance?.PlaySFX("UIClose");
            SetMainMenuEnabled(true);
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
            AudioManager.Instance?.PlaySFX("UIClick");
            GameSettings.SetFullscreen(enabled);
        }

        private void SetMainMenuEnabled(bool enabled)
        {
            if (mainMenuGroup == null)
            {
                return;
            }

            mainMenuGroup.interactable = enabled;
            mainMenuGroup.blocksRaycasts = enabled;
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

        private void PlayButtonClick()
        {
            AudioManager.Instance?.PlaySFX("ButtonClick");
        }

        private Button CreateMenuButton(Transform parent, string objectName, string label, Vector2 position)
        {
            GameObject buttonObject = CreateUiObject(objectName, parent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(470f, 66f), position);

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

            Text text = CreateText("Label", buttonObject.transform, label, 36, new Color(1f, 0.87f, 0.62f, 1f));
            Stretch(text.rectTransform, new Vector2(22f, 4f), new Vector2(-22f, -4f));
            text.alignment = TextAnchor.MiddleLeft;
            return button;
        }

        private Slider CreateSlider(Transform parent, Vector2 position)
        {
            return CreateVolumeSlider("MasterVolumeSlider", parent, position);
        }

        private Toggle CreateToggle(Transform parent, Vector2 position)
        {
            GameObject toggleObject = CreateUiObject("FullscreenToggle", parent);
            SetRect(toggleObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(86f, 52f), position);

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

        private Text CreateText(string objectName, Transform parent, string value, int fontSize, Color color)
        {
            Font font = menuFont != null ? menuFont : GameUiTheme.LoadFont();
            return RuntimeUiFactory.CreateText(objectName, parent, value, font, fontSize, color);
        }
    }
}
