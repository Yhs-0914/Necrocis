using System;
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
    public static class PauseMenuSmokeRunner
    {
        private const string HubScenePath = "Assets/_Project/Scenes/Hub.unity";
        private const string ScreenshotPath = "/tmp/necrocis-pause-menu.png";

        private static string testStorageRoot;
        private static int enteredPlayFrame;
        private static bool menuOpened;
        private static bool screenshotCaptured;
        private static bool settingsOpened;
        private static bool settingsClosed;
        private static bool saveMainClicked;
        private static bool previousEnterPlayModeOptionsEnabled;
        private static EnterPlayModeOptions previousEnterPlayModeOptions;

        private struct CanvasRenderState
        {
            public Canvas Canvas;
            public RenderMode RenderMode;
            public Camera WorldCamera;
            public float PlaneDistance;
            public bool Enabled;
        }

        public static void Run()
        {
            testStorageRoot = Path.Combine(Path.GetTempPath(), $"necrocis-pause-smoke-{Guid.NewGuid():N}");
            SaveService.UseStorageRootForTests(testStorageRoot);
            if (!SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error))
            {
                throw new InvalidOperationException($"Could not create Normal smoke save: {error}");
            }

            previousEnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            previousEnterPlayModeOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions =
                previousEnterPlayModeOptions | EnterPlayModeOptions.DisableDomainReload;

            EditorApplication.playModeStateChanged += HandlePlayModeChanged;
            EditorSceneManager.OpenScene(HubScenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        private static void HandlePlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                Screen.SetResolution(1920, 1080, false);
                enteredPlayFrame = Time.frameCount;
                menuOpened = false;
                screenshotCaptured = false;
                settingsOpened = false;
                settingsClosed = false;
                saveMainClicked = false;
                EditorApplication.update += Tick;
                return;
            }

            if (change != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            RestoreTestState();
            Debug.Log("[PauseMenuSmoke] PASS - pause, settings, save-and-main-menu flow verified");
            EditorApplication.Exit(0);
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying || Time.frameCount - enteredPlayFrame < 12)
            {
                return;
            }

            if (saveMainClicked)
            {
                if (SceneManager.GetActiveScene().name != SceneLoader.SCENE_MAIN_MENU)
                {
                    return;
                }

                MainMenuController mainMenu = UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
                if (mainMenu == null || mainMenu.ContinueButton == null || !mainMenu.ContinueButton.interactable)
                {
                    Fail("Saved Normal run is not available from MainMenu Continue.");
                    return;
                }

                EditorApplication.isPlaying = false;
                return;
            }

            InGameAudioSettings pauseMenu = UnityEngine.Object.FindFirstObjectByType<InGameAudioSettings>();
            if (pauseMenu == null)
            {
                Fail("In-game pause menu was not created.");
                return;
            }

            if (!menuOpened)
            {
                pauseMenu.OpenMenu();
                if (!pauseMenu.IsOpen
                    || pauseMenu.PauseOverlay == null
                    || !pauseMenu.PauseOverlay.activeSelf
                    || !Mathf.Approximately(Time.timeScale, 0f))
                {
                    Fail("Pause menu did not open and pause gameplay.");
                    return;
                }

                if (pauseMenu.ContinueButton == null
                    || pauseMenu.SettingsButton == null
                    || pauseMenu.SaveMainMenuButton == null
                    || pauseMenu.SaveQuitButton == null)
                {
                    Fail("One or more required pause actions are missing.");
                    return;
                }

                menuOpened = true;
                return;
            }

            if (!screenshotCaptured)
            {
                if (!CaptureCanvasScreenshot(pauseMenu, ScreenshotPath))
                {
                    Fail($"Could not capture pause menu screenshot at {ScreenshotPath}.");
                    return;
                }

                screenshotCaptured = true;
                return;
            }

            if (!settingsOpened)
            {
                pauseMenu.SettingsButton.onClick.Invoke();
                if (pauseMenu.SettingsOverlay == null || !pauseMenu.SettingsOverlay.activeSelf)
                {
                    Fail("Pause Settings button did not open its panel.");
                    return;
                }

                settingsOpened = true;
                return;
            }

            if (!settingsClosed)
            {
                Button backButton = pauseMenu.SettingsOverlay
                    .GetComponentsInChildren<Button>(true)
                    .FirstOrDefault(button => button.name == "PauseSettingsBackButton");
                if (backButton == null)
                {
                    Fail("Pause settings Back button is missing.");
                    return;
                }

                backButton.onClick.Invoke();
                if (pauseMenu.SettingsOverlay.activeSelf || !pauseMenu.PauseOverlay.activeSelf)
                {
                    Fail("Pause settings Back button did not restore the pause actions.");
                    return;
                }

                settingsClosed = true;
                return;
            }

            pauseMenu.SaveMainMenuButton.onClick.Invoke();
            saveMainClicked = true;
        }

        private static bool CaptureCanvasScreenshot(InGameAudioSettings pauseMenu, string path)
        {
            Canvas pauseCanvas = pauseMenu.GetComponentInChildren<Canvas>();
            if (pauseCanvas == null)
            {
                return false;
            }

            Canvas[] rootCanvases = UnityEngine.Object
                .FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(canvas => canvas != null && canvas.enabled && canvas.rootCanvas == canvas)
                .ToArray();
            CanvasRenderState[] canvasStates = rootCanvases
                .Select(canvas => new CanvasRenderState
                {
                    Canvas = canvas,
                    RenderMode = canvas.renderMode,
                    WorldCamera = canvas.worldCamera,
                    PlaneDistance = canvas.planeDistance,
                    Enabled = canvas.enabled
                })
                .ToArray();
            RenderTexture previousActive = RenderTexture.active;

            GameObject cameraObject = new GameObject("PauseMenuSmokeRenderCamera");
            Camera renderCamera = cameraObject.AddComponent<Camera>();
            renderCamera.transform.position = new Vector3(100000f, 100000f, -10f);
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = Color.black;
            renderCamera.cullingMask = ~0;
            renderCamera.enabled = false;

            RenderTexture target = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            Texture2D screenshot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            try
            {
                renderCamera.targetTexture = target;
                foreach (Canvas rootCanvas in rootCanvases)
                {
                    if (rootCanvas != pauseCanvas.rootCanvas)
                    {
                        rootCanvas.enabled = false;
                        continue;
                    }

                    rootCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                    rootCanvas.worldCamera = renderCamera;
                    rootCanvas.planeDistance = 1f;
                }

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
                foreach (CanvasRenderState state in canvasStates)
                {
                    if (state.Canvas == null)
                    {
                        continue;
                    }

                    state.Canvas.renderMode = state.RenderMode;
                    state.Canvas.worldCamera = state.WorldCamera;
                    state.Canvas.planeDistance = state.PlaneDistance;
                    state.Canvas.enabled = state.Enabled;
                }

                RenderTexture.active = previousActive;
                renderCamera.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(screenshot);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        private static void Fail(string message)
        {
            RestoreTestState();
            Debug.LogError($"[PauseMenuSmoke] FAIL - {message}");
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            EditorApplication.Exit(1);
        }

        private static void RestoreTestState()
        {
            Time.timeScale = 1f;
            EditorSettings.enterPlayModeOptionsEnabled = previousEnterPlayModeOptionsEnabled;
            EditorSettings.enterPlayModeOptions = previousEnterPlayModeOptions;
            SaveService.ResetStaticStateForTests();
            if (!string.IsNullOrEmpty(testStorageRoot) && Directory.Exists(testStorageRoot))
            {
                Directory.Delete(testStorageRoot, true);
            }
            testStorageRoot = null;
        }
    }
}
