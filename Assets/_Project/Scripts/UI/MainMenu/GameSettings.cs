using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// Small shared settings store used by the title screen.
    /// </summary>
    public static class GameSettings
    {
        private const string MasterVolumeKey = "necrocis.settings.master-volume";
        private const string BgmVolumeKey = "necrocis.settings.bgm-volume";
        private const string SfxVolumeKey = "necrocis.settings.sfx-volume";
        private const string FullscreenKey = "necrocis.settings.fullscreen";

        public static float MasterVolume => Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        public static float BgmVolume => Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumeKey, 1f));
        public static float SfxVolume => Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, 1f));

        public static bool Fullscreen => PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;

        public static void ApplySaved()
        {
            AudioListener.volume = MasterVolume;
            AudioManager.Instance?.SetBGMVolume(BgmVolume);
            AudioManager.Instance?.SetSFXVolume(SfxVolume);
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

        public static void SetBgmVolume(float value)
        {
            value = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(BgmVolumeKey, value);
            AudioManager.Instance?.SetBGMVolume(value);
        }

        public static void SetSfxVolume(float value)
        {
            value = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxVolumeKey, value);
            AudioManager.Instance?.SetSFXVolume(value);
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }
    }
}
