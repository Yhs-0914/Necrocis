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
        private static bool startClicked;
        private static bool initialCaptureComplete;
        private static bool settingsCaptureComplete;
        private static bool previousEnterPlayModeOptionsEnabled;
        private static EnterPlayModeOptions previousEnterPlayModeOptions;

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

        private static void Begin(SmokePhase requestedPhase, bool revealBosses)
        {
            MainMenuSceneBuilder.ValidateOrThrow();
            BackupAndSetBossProgress(revealBosses ? 1 : 0);
            if (requestedPhase == SmokePhase.PartialFlow)
            {
                PlayerPrefs.SetInt("necrocis.boss-defeated.lung", 1);
                PlayerPrefs.Save();
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
                startClicked = false;
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
            Debug.Log(phase == SmokePhase.StartFlow
                ? "[MainMenuSmoke] PASS - settings and Hub start flow verified"
                : phase == SmokePhase.QuitFlow
                    ? "[MainMenuSmoke] PASS - quit flow verified"
                    : phase == SmokePhase.RevealFlow
                        ? "[MainMenuSmoke] PASS - revealed boss collection state verified"
                        : "[MainMenuSmoke] PASS - partial boss collection state verified");
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

            if (controller.StartButton == null || controller.SettingsButton == null || controller.QuitButton == null)
            {
                Fail("One or more main-menu buttons are missing.");
                return;
            }

            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                Fail("EventSystem is missing from the title screen.");
                return;
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

            if (!startClicked && settingsClosed && Time.frameCount - enteredPlayFrame >= 16)
            {
                controller.StartButton.onClick.Invoke();
                startClicked = true;
                return;
            }

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
            PartialFlow
        }
    }

}
