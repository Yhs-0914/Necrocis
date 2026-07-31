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
    public static class HardDeathSmokeRunner
    {
        private const string HubScenePath = "Assets/_Project/Scenes/Hub.unity";

        private static string testStorageRoot;
        private static int enteredPlayFrame;
        private static bool deathClicked;
        private static bool previousEnterPlayModeOptionsEnabled;
        private static EnterPlayModeOptions previousEnterPlayModeOptions;

        public static void Run()
        {
            testStorageRoot = Path.Combine(
                Path.GetTempPath(),
                $"necrocis-hard-death-smoke-{Guid.NewGuid():N}");
            SaveService.UseStorageRootForTests(testStorageRoot);

            if (!SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error))
            {
                throw new InvalidOperationException($"Normal test save 생성 실패: {error}");
            }

            SaveService.MarkFinalBossDefeated();
            if (!SaveService.TryBeginNewGame(GameDifficulty.Hard, out error))
            {
                throw new InvalidOperationException($"Hard test run 생성 실패: {error}");
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
                enteredPlayFrame = Time.frameCount;
                deathClicked = false;
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
            Debug.Log("[HardDeathSmoke] PASS - Hard death wipe and MainMenu return verified");
            EditorApplication.Exit(0);
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying || Time.frameCount - enteredPlayFrame < 12)
            {
                return;
            }

            if (deathClicked)
            {
                if (SceneManager.GetActiveScene().name != SceneLoader.SCENE_MAIN_MENU)
                {
                    return;
                }

                MainMenuController mainMenu =
                    UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
                if (mainMenu == null)
                {
                    Fail("Hard 사망 후 MainMenuController가 생성되지 않았습니다.");
                    return;
                }

                if (SaveService.HasContinueSave(GameDifficulty.Hard))
                {
                    Fail("Hard 사망 후 Hard 계속하기 저장이 남았습니다.");
                    return;
                }

                if (!SaveService.HasContinueSave(GameDifficulty.Normal))
                {
                    Fail("Hard 사망이 독립된 Normal 슬롯을 제거했습니다.");
                    return;
                }

                EditorApplication.isPlaying = false;
                return;
            }

            PlayerDeathScreen deathScreen =
                UnityEngine.Object.FindFirstObjectByType<PlayerDeathScreen>();
            if (deathScreen == null)
            {
                Fail("Hub에서 PlayerDeathScreen을 찾지 못했습니다.");
                return;
            }

            deathScreen.ShowDeath();
            Button returnButton = deathScreen
                .GetComponentsInChildren<Button>(true)
                .FirstOrDefault(button => button.name == "ReturnToHubHotspot");
            if (!deathScreen.IsShowing || returnButton == null)
            {
                Fail("사망 UI 또는 귀환 버튼을 열지 못했습니다.");
                return;
            }

            returnButton.onClick.Invoke();
            deathClicked = true;
        }

        private static void Fail(string message)
        {
            RestoreTestState();
            Debug.LogError($"[HardDeathSmoke] FAIL - {message}");
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
