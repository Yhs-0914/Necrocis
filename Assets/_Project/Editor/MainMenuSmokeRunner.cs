using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NecrocisEditor
{
    /// <summary>
    /// Batch-invokable play-mode smoke test for the title screen's core flow.
    /// </summary>
    public static class MainMenuSmokeRunner
    {
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string ScreenshotPath = "/tmp/necrocis-main-menu.png";
        private const string SettingsScreenshotPath = "/tmp/necrocis-main-menu-settings.png";
        private const string RevealedScreenshotPath = "/tmp/necrocis-main-menu-revealed.png";
        private const string PartialScreenshotPath = "/tmp/necrocis-main-menu-partial.png";
        private const string ContinueLayoutScreenshotPath = "/tmp/necrocis-main-menu-with-save.png";
        private static readonly string[] BossKeys =
        {
            "necrocis.boss-defeated.intestine",
            "necrocis.boss-defeated.liver",
            "necrocis.boss-defeated.stomach",
            "necrocis.boss-defeated.lung"
        };

        private static readonly Dictionary<string, int?> SavedBossValues = new Dictionary<string, int?>();
        private static SmokePhase phase;
        private static int enteredPlayFrame;
        private static bool settingsOpened;
        private static bool settingsClosed;
        private static bool difficultyOpened;
        private static bool startClicked;
        private static bool layoutVerified;
        private static bool initialCaptureComplete;
        private static bool settingsCaptureComplete;
        private static bool previousEnterPlayModeOptionsEnabled;
        private static EnterPlayModeOptions previousEnterPlayModeOptions;
        private static string testStorageRoot;

        public static void Run()
        {
            Begin(SmokePhase.StartFlow, false);
        }

        public static void RunQuit()
        {
            Begin(SmokePhase.QuitFlow, false);
        }

        public static void RunRevealed()
        {
            Begin(SmokePhase.RevealFlow, true);
        }

        public static void RunPartial()
        {
            Begin(SmokePhase.PartialFlow, false);
        }

        public static void RunContinueLayout()
        {
            Begin(SmokePhase.ContinueLayout, false);
        }

        private static void Begin(SmokePhase requestedPhase, bool revealBosses)
        {
            MainMenuSceneBuilder.ValidateOrThrow();
            BackupAndSetBossProgress(revealBosses ? 1 : 0);
            if (requestedPhase == SmokePhase.PartialFlow)
            {
                PlayerPrefs.SetInt("necrocis.boss-defeated.lung", 1);
                PlayerPrefs.Save();
            }
            testStorageRoot = Path.Combine(Path.GetTempPath(), $"necrocis-main-menu-smoke-{Guid.NewGuid():N}");
            SaveService.UseStorageRootForTests(testStorageRoot);
            if (requestedPhase == SmokePhase.ContinueLayout
                && !SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error))
            {
                throw new InvalidOperationException($"Continue layout test save 생성 실패: {error}");
            }
            previousEnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            previousEnterPlayModeOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = previousEnterPlayModeOptions | EnterPlayModeOptions.DisableDomainReload;
            phase = requestedPhase;
            EditorApplication.playModeStateChanged += HandlePlayModeChanged;
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        private static void HandlePlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                Screen.SetResolution(1920, 1080, false);
                enteredPlayFrame = Time.frameCount;
                settingsOpened = false;
                settingsClosed = false;
                difficultyOpened = false;
                startClicked = false;
                layoutVerified = false;
                initialCaptureComplete = false;
                settingsCaptureComplete = false;
                EditorApplication.update += Tick;
                return;
            }

            if (change != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            EditorApplication.update -= Tick;
            RestoreBossProgress();
            RestoreEditorPlayModeSettings();
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            string result = phase switch
            {
                SmokePhase.StartFlow => "settings, no-save layout and Hub start flow verified",
                SmokePhase.QuitFlow => "quit flow verified",
                SmokePhase.RevealFlow => "revealed boss collection state verified",
                SmokePhase.PartialFlow => "partial boss collection state verified",
                SmokePhase.ContinueLayout => "continue-save four-button layout verified",
                _ => "main-menu flow verified"
            };
            Debug.Log($"[MainMenuSmoke] PASS - {result}");
            EditorApplication.Exit(0);
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying || Time.frameCount - enteredPlayFrame < 8)
            {
                return;
            }

            if (phase == SmokePhase.StartFlow && startClicked && SceneManager.GetActiveScene().name == SceneLoader.SCENE_HUB)
            {
                if (!File.Exists(ScreenshotPath) || !File.Exists(SettingsScreenshotPath))
                {
                    Fail("One or more title-screen screenshots were not written.");
                    return;
                }

                EditorApplication.isPlaying = false;
                return;
            }

            MainMenuController controller = UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
            if (controller == null)
            {
                Fail("MainMenuController was not created in play mode.");
                return;
            }

            if (controller.StartButton == null
                || controller.ContinueButton == null
                || controller.SettingsButton == null
                || controller.QuitButton == null)
            {
                Fail("One or more main-menu buttons are missing.");
                return;
            }

            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                Fail("EventSystem is missing from the title screen.");
                return;
            }

            if (phase == SmokePhase.ContinueLayout)
            {
                if (!ValidateMenuLayout(controller, true))
                {
                    return;
                }

                if (!CaptureCanvasScreenshot(controller, ContinueLayoutScreenshotPath))
                {
                    Fail($"Could not capture screenshot at {ContinueLayoutScreenshotPath}.");
                    return;
                }

                EditorApplication.isPlaying = false;
                return;
            }

            if (phase == SmokePhase.StartFlow && !layoutVerified)
            {
                if (!ValidateMenuLayout(controller, false))
                {
                    return;
                }

                layoutVerified = true;
            }

            if (phase == SmokePhase.QuitFlow)
            {
                controller.QuitButton.onClick.Invoke();
                return;
            }

            if (phase == SmokePhase.RevealFlow || phase == SmokePhase.PartialFlow)
            {
                string screenshotPath = phase == SmokePhase.RevealFlow
                    ? RevealedScreenshotPath
                    : PartialScreenshotPath;
                if (!CaptureCanvasScreenshot(controller, screenshotPath))
                {
                    Fail($"Could not capture screenshot at {screenshotPath}.");
                    return;
                }

                EditorApplication.isPlaying = false;
                return;
            }

            if (!settingsOpened)
            {
                if (!initialCaptureComplete)
                {
                    if (!CaptureCanvasScreenshot(controller, ScreenshotPath))
                    {
                        Fail($"Could not capture screenshot at {ScreenshotPath}.");
                        return;
                    }

                    initialCaptureComplete = true;
                }

                controller.SettingsButton.onClick.Invoke();
                if (controller.SettingsOverlay == null || !controller.SettingsOverlay.activeSelf)
                {
                    Fail("Settings button did not open the settings overlay.");
                    return;
                }

                settingsOpened = true;
                return;
            }

            if (!settingsClosed && Time.frameCount - enteredPlayFrame >= 12)
            {
                if (!settingsCaptureComplete)
                {
                    if (!CaptureCanvasScreenshot(controller, SettingsScreenshotPath))
                    {
                        Fail($"Could not capture screenshot at {SettingsScreenshotPath}.");
                        return;
                    }

                    settingsCaptureComplete = true;
                }

                Button backButton = controller.SettingsOverlay
                    .GetComponentsInChildren<Button>(true)
                    .FirstOrDefault(button => button.name == "SettingsBackButton");
                if (backButton == null)
                {
                    Fail("Settings back button is missing.");
                    return;
                }

                backButton.onClick.Invoke();
                if (controller.SettingsOverlay.activeSelf)
                {
                    Fail("Settings back button did not close the overlay.");
                    return;
                }

                settingsClosed = true;
                return;
            }

            if (!difficultyOpened && settingsClosed && Time.frameCount - enteredPlayFrame >= 16)
            {
                controller.StartButton.onClick.Invoke();
                if (controller.DifficultyOverlay == null || !controller.DifficultyOverlay.activeSelf)
                {
                    Fail("New Game button did not open the difficulty overlay.");
                    return;
                }

                difficultyOpened = true;
                return;
            }

            if (!startClicked && difficultyOpened && Time.frameCount - enteredPlayFrame >= 18)
            {
                if (controller.NormalDifficultyButton == null
                    || !controller.NormalDifficultyButton.interactable)
                {
                    Fail("Normal difficulty button is unavailable.");
                    return;
                }

                controller.NormalDifficultyButton.onClick.Invoke();
                startClicked = true;
                return;
            }

        }

        private static bool ValidateMenuLayout(MainMenuController controller, bool expectContinue)
        {
            Button continueButton = controller.ContinueButton;
            Button newGameButton = controller.NewGameButton;
            Button settingsButton = controller.SettingsButton;
            Button quitButton = controller.QuitButton;
            if (continueButton.gameObject.activeSelf != expectContinue)
            {
                Fail(expectContinue
                    ? "Continue save exists but Continue button is hidden."
                    : "No continue save exists but Continue button is visible.");
                return false;
            }

            Text status = controller
                .GetComponentsInChildren<Text>(true)
                .FirstOrDefault(text => text.name == "MenuStatus");
            if (status == null || status.gameObject.activeSelf != expectContinue)
            {
                Fail("Menu status visibility does not match Continue button visibility.");
                return false;
            }

            if (expectContinue)
            {
                if (!ApproximatelyY(continueButton, 117f)
                    || !ApproximatelyY(newGameButton, 39f)
                    || !ApproximatelyY(settingsButton, -39f)
                    || !ApproximatelyY(quitButton, -117f)
                    || string.IsNullOrWhiteSpace(status.text))
                {
                    Fail("Continue-save four-button layout is incorrect.");
                    return false;
                }

                if (continueButton.navigation.selectOnDown != newGameButton
                    || newGameButton.navigation.selectOnUp != continueButton
                    || quitButton.navigation.selectOnDown != continueButton)
                {
                    Fail("Continue-save four-button navigation is incorrect.");
                    return false;
                }

                return true;
            }

            if (!ApproximatelyY(newGameButton, 78f)
                || !ApproximatelyY(settingsButton, 0f)
                || !ApproximatelyY(quitButton, -78f))
            {
                Fail("No-save three-button layout is incorrect.");
                return false;
            }

            if (newGameButton.navigation.selectOnUp != quitButton
                || newGameButton.navigation.selectOnDown != settingsButton
                || quitButton.navigation.selectOnDown != newGameButton)
            {
                Fail("No-save three-button navigation is incorrect.");
                return false;
            }

            return true;
        }

        private static bool ApproximatelyY(Button button, float expected)
        {
            RectTransform rect = button.GetComponent<RectTransform>();
            return Mathf.Abs(rect.anchoredPosition.y - expected) < 0.01f;
        }

        private static void BackupAndSetBossProgress(int value)
        {
            SavedBossValues.Clear();
            foreach (string key in BossKeys)
            {
                SavedBossValues[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : null;
                PlayerPrefs.SetInt(key, value);
            }
            PlayerPrefs.Save();
            SaveService.ResetStaticStateForTests();
            if (!string.IsNullOrEmpty(testStorageRoot) && Directory.Exists(testStorageRoot))
            {
                Directory.Delete(testStorageRoot, true);
            }
            testStorageRoot = null;
        }

        private static bool CaptureCanvasScreenshot(MainMenuController controller, string path)
        {
            Canvas canvas = controller.GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                return false;
            }

            RenderMode previousRenderMode = canvas.renderMode;
            Camera previousCamera = canvas.worldCamera;
            float previousPlaneDistance = canvas.planeDistance;
            RenderTexture previousActive = RenderTexture.active;

            GameObject cameraObject = new GameObject("MainMenuSmokeRenderCamera");
            Camera renderCamera = cameraObject.AddComponent<Camera>();
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = Color.black;
            renderCamera.cullingMask = ~0;
            renderCamera.enabled = false;

            RenderTexture target = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            Texture2D screenshot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);

            try
            {
                renderCamera.targetTexture = target;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = renderCamera;
                canvas.planeDistance = 1f;
                Canvas.ForceUpdateCanvases();
                renderCamera.Render();

                RenderTexture.active = target;
                screenshot.ReadPixels(new Rect(0f, 0f, 1920f, 1080f), 0, 0);
                screenshot.Apply(false);
                File.WriteAllBytes(path, screenshot.EncodeToPNG());
                return File.Exists(path);
            }
            finally
            {
                canvas.renderMode = previousRenderMode;
                canvas.worldCamera = previousCamera;
                canvas.planeDistance = previousPlaneDistance;
                RenderTexture.active = previousActive;
                renderCamera.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(screenshot);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        private static void RestoreBossProgress()
        {
            foreach (KeyValuePair<string, int?> pair in SavedBossValues)
            {
                if (pair.Value.HasValue)
                {
                    PlayerPrefs.SetInt(pair.Key, pair.Value.Value);
                }
                else
                {
                    PlayerPrefs.DeleteKey(pair.Key);
                }
            }
            PlayerPrefs.Save();
        }

        private static void Fail(string message)
        {
            RestoreBossProgress();
            RestoreEditorPlayModeSettings();
            Debug.LogError($"[MainMenuSmoke] FAIL - {message}");
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            EditorApplication.Exit(1);
        }

        private static void RestoreEditorPlayModeSettings()
        {
            EditorSettings.enterPlayModeOptionsEnabled = previousEnterPlayModeOptionsEnabled;
            EditorSettings.enterPlayModeOptions = previousEnterPlayModeOptions;
        }

        private enum SmokePhase
        {
            StartFlow,
            QuitFlow,
            RevealFlow,
            PartialFlow,
            ContinueLayout
        }
    }

}
