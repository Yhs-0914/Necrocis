using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// Small shared settings store used by the title screen.
    /// </summary>
    public static class GameSettings
    {
        private const string MasterVolumeKey = "necrocis.settings.master-volume";
        private const string FullscreenKey = "necrocis.settings.fullscreen";

        public static float MasterVolume => Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));

        public static bool Fullscreen => PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;

        public static void ApplySaved()
        {
            AudioListener.volume = MasterVolume;
            Screen.fullScreen = Fullscreen;
        }

        public static void SetMasterVolume(float value)
        {
            value = Mathf.Clamp01(value);
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(MasterVolumeKey, value);
        }

        public static void SetFullscreen(bool enabled)
        {
            Screen.fullScreen = enabled;
            PlayerPrefs.SetInt(FullscreenKey, enabled ? 1 : 0);
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }
    }
}
