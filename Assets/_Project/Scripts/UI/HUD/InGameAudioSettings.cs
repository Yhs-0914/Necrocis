using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Necrocis.RuntimeUiFactory;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public sealed class InGameAudioSettings : MonoBehaviour
    {
        private const int SortingOrder = 450;
        private static InGameAudioSettings instance;

        private readonly Color panelColor = new Color(0.055f, 0.012f, 0.024f, 0.98f);
        private readonly Color borderColor = new Color(0.67f, 0.12f, 0.08f, 0.95f);
        private readonly Color goldColor = new Color(1f, 0.87f, 0.62f, 1f);

        private Font menuFont;
        private GameObject canvasObject;
        private GameObject pauseOverlay;
        private GameObject settingsOverlay;
        private CanvasGroup pauseGroup;
        private Button continueButton;
        private Button settingsButton;
        private Button saveMainMenuButton;
        private Button saveQuitButton;
        private Button settingsBackButton;
        private Slider masterVolumeSlider;
        private Slider bgmVolumeSlider;
        private Slider sfxVolumeSlider;
        private Text masterVolumeValue;
        private Text bgmVolumeValue;
        private Text sfxVolumeValue;
        private Text statusText;
        private bool isOpen;
        private bool isBusy;
        private float previousTimeScale = 1f;
        private PlayerController pausedPlayer;
        private PlayerAttack pausedAttack;
        private PlayerClassSkillController pausedSkills;
        private bool playerWasEnabled;
        private bool attackWasEnabled;
        private bool skillsWereEnabled;

        public bool IsOpen => isOpen;
        public GameObject PauseOverlay => pauseOverlay;
        public GameObject SettingsOverlay => settingsOverlay;
        public Button ContinueButton => continueButton;
        public Button SettingsButton => settingsButton;
        public Button SaveMainMenuButton => saveMainMenuButton;
        public Button SaveQuitButton => saveQuitButton;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            EnsureCreated();
        }

        public static void EnsureCreated()
        {
            if (string.Equals(
                    SceneManager.GetActiveScene().name,
                    SceneLoader.SCENE_MAIN_MENU,
                    System.StringComparison.Ordinal)
                || instance != null
                || Object.FindFirstObjectByType<InGameAudioSettings>() != null)
            {
                return;
            }

            new GameObject("InGamePauseMenu").AddComponent<InGameAudioSettings>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            GameSettings.ApplySaved();
            menuFont = GameUiTheme.LoadFont();
            EnsureEventSystem();
            BuildInterface();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            if (instance == this)
            {
                instance = null;
            }

            if (isOpen)
            {
                Time.timeScale = previousTimeScale;
                RestoreGameplayInput();
            }
        }

        private void Update()
        {
            if (isBusy || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            if (settingsOverlay != null && settingsOverlay.activeSelf)
            {
                CloseSettings();
                return;
            }

            if (isOpen)
            {
                CloseMenu();
                return;
            }

            if (Time.timeScale > 0f)
            {
                OpenMenu();
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (string.Equals(scene.name, SceneLoader.SCENE_MAIN_MENU, System.StringComparison.Ordinal))
            {
                Destroy(gameObject);
            }
        }

        public void OpenMenu()
        {
            if (isOpen || isBusy || pauseOverlay == null)
            {
                return;
            }

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            CacheAndDisableGameplayInput();
            isOpen = true;
            statusText.text = string.Empty;
            settingsOverlay.SetActive(false);
            pauseOverlay.SetActive(true);
            pauseGroup.interactable = true;
            pauseGroup.blocksRaycasts = true;
            AudioManager.Instance?.PlaySFX("SettingsOpen");
            continueButton?.Select();
        }

        public void CloseMenu()
        {
            if (!isOpen || isBusy)
            {
                return;
            }

            GameSettings.Save();
            pauseOverlay.SetActive(false);
            settingsOverlay.SetActive(false);
            isOpen = false;
            Time.timeScale = previousTimeScale;
            RestoreGameplayInput();
            AudioManager.Instance?.PlaySFX("UIClose");
        }

        private void OpenSettings()
        {
            if (!isOpen || isBusy)
            {
                return;
            }

            pauseOverlay.SetActive(false);
            settingsOverlay.SetActive(true);
            AudioManager.Instance?.PlaySFX("UIClick");
            masterVolumeSlider?.Select();
        }

        private void CloseSettings()
        {
            if (!isOpen || isBusy || settingsOverlay == null || !settingsOverlay.activeSelf)
            {
                return;
            }

            GameSettings.Save();
            settingsOverlay.SetActive(false);
            pauseOverlay.SetActive(true);
            AudioManager.Instance?.PlaySFX("UIClose");
            settingsButton?.Select();
        }

        private void SaveAndReturnToMainMenu()
        {
            if (!isBusy)
            {
                StartCoroutine(SaveAndReturnRoutine());
            }
        }

        private IEnumerator SaveAndReturnRoutine()
        {
            SetBusy(true, "저장 중...");
            yield return null;

            if (!SaveService.TrySaveActiveRun(out string error))
            {
                SetBusy(false, $"저장 실패: {error}");
                AudioManager.Instance?.PlaySFX("UIInvalid");
                yield break;
            }

            GameSettings.Save();
            statusText.text = "저장 완료";
            AudioManager.Instance?.PlaySFX("UISelect");
            yield return new WaitForSecondsRealtime(0.15f);
            Time.timeScale = 1f;
            GameplaySessionLifecycle.LoadMainMenu();
        }

        private void SaveAndQuit()
        {
            if (isBusy)
            {
                return;
            }

            SetBusy(true, "저장 중...");
            if (!SaveService.TrySaveActiveRun(out string error))
            {
                SetBusy(false, $"저장 실패: {error}");
                AudioManager.Instance?.PlaySFX("UIInvalid");
                return;
            }

            GameSettings.Save();
            statusText.text = "저장 완료";
            Time.timeScale = 1f;
            Debug.Log("[PauseMenu] 저장 후 게임 종료 요청");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void SetBusy(bool busy, string message)
        {
            isBusy = busy;
            if (pauseGroup != null)
            {
                pauseGroup.interactable = !busy;
            }

            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private void BuildInterface()
        {
            canvasObject = CreateUiObject("InGamePauseCanvas", transform);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            pauseOverlay = CreateOverlay(canvasObject.transform, "PauseOverlay");
            BuildPausePanel(pauseOverlay.transform);
            settingsOverlay = CreateOverlay(canvasObject.transform, "PauseSettingsOverlay");
            BuildSettingsPanel(settingsOverlay.transform);

            pauseOverlay.SetActive(false);
            settingsOverlay.SetActive(false);
        }

        private GameObject CreateOverlay(Transform parent, string objectName)
        {
            GameObject overlay = CreateUiObject(objectName, parent);
            Stretch(overlay.GetComponent<RectTransform>());
            Image dim = overlay.AddComponent<Image>();
            dim.color = new Color(0.005f, 0.002f, 0.006f, 0.84f);
            return overlay;
        }

        private void BuildPausePanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "PausePanel", new Vector2(760f, 650f));
            pauseGroup = panel.AddComponent<CanvasGroup>();

            Text title = CreateText(
                "PauseTitle",
                panel.transform,
                "일시 정지",
                54,
                new Color(1f, 0.84f, 0.53f, 1f));
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(620f, 80f), new Vector2(0f, 245f));
            title.alignment = TextAnchor.MiddleCenter;

            continueButton = CreateMenuButton(panel.transform, "ContinueButton", "계속", new Vector2(0f, 125f));
            settingsButton = CreateMenuButton(panel.transform, "PauseSettingsButton", "설정", new Vector2(0f, 35f));
            saveMainMenuButton = CreateMenuButton(
                panel.transform,
                "SaveMainMenuButton",
                "저장 후 메인 화면",
                new Vector2(0f, -55f));
            saveQuitButton = CreateMenuButton(
                panel.transform,
                "SaveQuitButton",
                "저장 후 종료",
                new Vector2(0f, -145f));

            continueButton.onClick.AddListener(CloseMenu);
            settingsButton.onClick.AddListener(OpenSettings);
            saveMainMenuButton.onClick.AddListener(SaveAndReturnToMainMenu);
            saveQuitButton.onClick.AddListener(SaveAndQuit);

            SetVerticalNavigation(continueButton, saveQuitButton, settingsButton);
            SetVerticalNavigation(settingsButton, continueButton, saveMainMenuButton);
            SetVerticalNavigation(saveMainMenuButton, settingsButton, saveQuitButton);
            SetVerticalNavigation(saveQuitButton, saveMainMenuButton, continueButton);

            statusText = CreateText("SaveStatus", panel.transform, string.Empty, 25, new Color(1f, 0.65f, 0.3f, 1f));
            SetRect(statusText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(650f, 55f), new Vector2(0f, -255f));
            statusText.alignment = TextAnchor.MiddleCenter;
        }

        private void BuildSettingsPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "PauseSettingsPanel", new Vector2(760f, 650f));

            Text title = CreateText(
                "PauseSettingsTitle",
                panel.transform,
                "설정",
                54,
                new Color(1f, 0.84f, 0.53f, 1f));
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(620f, 80f), new Vector2(0f, 245f));
            title.alignment = TextAnchor.MiddleCenter;

            CreateVolumeRow(
                panel.transform,
                "전체 음량",
                120f,
                GameSettings.MasterVolume,
                value => GameSettings.SetMasterVolume(value),
                out masterVolumeSlider,
                out masterVolumeValue);
            CreateVolumeRow(
                panel.transform,
                "배경음",
                25f,
                GameSettings.BgmVolume,
                value => GameSettings.SetBgmVolume(value),
                out bgmVolumeSlider,
                out bgmVolumeValue);
            CreateVolumeRow(
                panel.transform,
                "효과음",
                -70f,
                GameSettings.SfxVolume,
                value => GameSettings.SetSfxVolume(value),
                out sfxVolumeSlider,
                out sfxVolumeValue);

            BindVolumeText(masterVolumeSlider, masterVolumeValue);
            BindVolumeText(bgmVolumeSlider, bgmVolumeValue);
            BindVolumeText(sfxVolumeSlider, sfxVolumeValue);

            settingsBackButton = CreateMenuButton(
                panel.transform,
                "PauseSettingsBackButton",
                "돌아가기",
                new Vector2(0f, -205f));
            settingsBackButton.onClick.AddListener(CloseSettings);
            settingsBackButton.GetComponent<RectTransform>().sizeDelta = new Vector2(360f, 58f);
        }

        private void CreateVolumeRow(
            Transform parent,
            string label,
            float y,
            float initialValue,
            UnityEngine.Events.UnityAction<float> setter,
            out Slider slider,
            out Text valueText)
        {
            Text rowLabel = CreateText(
                $"{label}Label",
                parent,
                label,
                30,
                new Color(0.92f, 0.82f, 0.72f, 1f));
            SetRect(rowLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(220f, 52f), new Vector2(-220f, y));
            rowLabel.alignment = TextAnchor.MiddleLeft;

            slider = CreateSlider(parent, new Vector2(55f, y));
            slider.SetValueWithoutNotify(initialValue);
            slider.onValueChanged.AddListener(setter);

            valueText = CreateText(
                $"{label}Value",
                parent,
                $"{Mathf.RoundToInt(initialValue * 100f)}%",
                28,
                new Color(1f, 0.72f, 0.37f, 1f));
            SetRect(valueText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(90f, 52f), new Vector2(290f, y));
            valueText.alignment = TextAnchor.MiddleRight;
        }

        private static void BindVolumeText(Slider slider, Text valueText)
        {
            slider.onValueChanged.AddListener(value =>
            {
                if (valueText != null)
                {
                    valueText.text = $"{Mathf.RoundToInt(value * 100f)}%";
                }
            });
        }

        private GameObject CreatePanel(Transform parent, string objectName, Vector2 size)
        {
            GameObject panel = CreateUiObject(objectName, parent);
            SetRect(panel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), size, Vector2.zero);
            Image image = panel.AddComponent<Image>();
            image.color = panelColor;
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(3f, -3f);
            return panel;
        }

        private Button CreateMenuButton(Transform parent, string objectName, string label, Vector2 position)
        {
            GameObject buttonObject = CreateUiObject(objectName, parent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(500f, 68f), position);

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
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            Outline border = buttonObject.AddComponent<Outline>();
            border.effectColor = new Color(0.8f, 0.48f, 0.16f, 0.5f);
            border.effectDistance = new Vector2(1.5f, -1.5f);

            Text text = CreateText("Label", buttonObject.transform, label, 34, goldColor);
            Stretch(text.rectTransform, new Vector2(24f, 4f), new Vector2(-24f, -4f));
            text.alignment = TextAnchor.MiddleCenter;
            return button;
        }

        private Slider CreateSlider(Transform parent, Vector2 position)
        {
            return CreateVolumeSlider("VolumeSlider", parent, position);
        }

        private Text CreateText(string objectName, Transform parent, string value, int fontSize, Color color)
        {
            return RuntimeUiFactory.CreateText(objectName, parent, value, menuFont, fontSize, color);
        }

        private void CacheAndDisableGameplayInput()
        {
            pausedPlayer = PlayerController.Instance;
            if (pausedPlayer == null)
            {
                return;
            }

            pausedAttack = pausedPlayer.GetComponent<PlayerAttack>();
            pausedSkills = pausedPlayer.GetComponent<PlayerClassSkillController>();
            playerWasEnabled = pausedPlayer.enabled;
            attackWasEnabled = pausedAttack != null && pausedAttack.enabled;
            skillsWereEnabled = pausedSkills != null && pausedSkills.enabled;
            pausedPlayer.enabled = false;
            if (pausedAttack != null)
            {
                pausedAttack.enabled = false;
            }

            if (pausedSkills != null)
            {
                pausedSkills.enabled = false;
            }
        }

        private void RestoreGameplayInput()
        {
            if (pausedPlayer != null)
            {
                pausedPlayer.enabled = playerWasEnabled;
            }

            if (pausedAttack != null)
            {
                pausedAttack.enabled = attackWasEnabled;
            }

            if (pausedSkills != null)
            {
                pausedSkills.enabled = skillsWereEnabled;
            }

            pausedPlayer = null;
            pausedAttack = null;
            pausedSkills = null;
        }

    }
}
