using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Necrocis
{
    public class InGameAudioSettings : MonoBehaviour
    {
        private Rect windowRect = new Rect(0f, 0f, 420f, 300f);
        private bool isOpen;
        private float previousTimeScale = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "MainMenu" || FindFirstObjectByType<InGameAudioSettings>() != null)
            {
                return;
            }

            new GameObject("InGameAudioSettings").AddComponent<InGameAudioSettings>();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            GameSettings.ApplySaved();
            CenterWindow();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetOpen(!isOpen);
            }
        }

        private void OnGUI()
        {
            if (!isOpen)
            {
                return;
            }

            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), string.Empty);
            windowRect = GUI.Window(GetInstanceID(), windowRect, DrawWindow, "설정");
        }

        private void DrawWindow(int id)
        {
            GUILayout.Space(12f);
            DrawVolumeSlider("전체 음량", GameSettings.MasterVolume, GameSettings.SetMasterVolume);
            DrawVolumeSlider("배경음", GameSettings.BgmVolume, GameSettings.SetBgmVolume);
            DrawVolumeSlider("효과음", GameSettings.SfxVolume, GameSettings.SetSfxVolume);
            GUILayout.Space(18f);

            if (GUILayout.Button("게임으로 돌아가기", GUILayout.Height(42f)))
            {
                SetOpen(false);
            }

            GUI.DragWindow(new Rect(0f, 0f, windowRect.width, 35f));
        }

        private static void DrawVolumeSlider(string label, float value, System.Action<float> setter)
        {
            GUILayout.Label($"{label}: {Mathf.RoundToInt(value * 100f)}%");
            float changed = GUILayout.HorizontalSlider(value, 0f, 1f, GUILayout.Height(24f));
            if (!Mathf.Approximately(changed, value))
            {
                setter(changed);
            }
            GUILayout.Space(8f);
        }

        private void SetOpen(bool open)
        {
            if (isOpen == open)
            {
                return;
            }

            isOpen = open;
            AudioManager.Instance?.PlaySFX(open ? "SettingsOpen" : "UIClose");
            if (open)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                CenterWindow();
            }
            else
            {
                Time.timeScale = previousTimeScale;
                GameSettings.Save();
            }
        }

        private void CenterWindow()
        {
            windowRect.x = (Screen.width - windowRect.width) * 0.5f;
            windowRect.y = (Screen.height - windowRect.height) * 0.5f;
        }

        private void OnDestroy()
        {
            if (isOpen)
            {
                Time.timeScale = previousTimeScale;
            }
        }
    }
}
