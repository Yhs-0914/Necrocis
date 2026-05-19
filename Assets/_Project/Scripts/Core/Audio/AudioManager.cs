using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// Centralized SFX playback manager.
    /// Player or systems call this by sound id, not by direct AudioSource references.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Serializable]
        private struct PlayerSfxEntry
        {
            public PlayerSoundId soundId;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume;
        }

        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<AudioManager>();
                    if (instance == null)
                    {
                        GameObject managerObject = new GameObject("AudioManager");
                        instance = managerObject.AddComponent<AudioManager>();
                    }
                }

                instance?.EnsureInitialized();
                return instance;
            }
        }

        private static AudioManager instance;

        [Header("SFX")]
        [SerializeField] private List<PlayerSfxEntry> playerSfxEntries = new List<PlayerSfxEntry>();
        [SerializeField, Range(0f, 1f)] private float sfxMasterVolume = 1f;
        [SerializeField] private bool warnIfMissingMapping = true;

        [Header("BGM")]
        [SerializeField] private AudioClip gameplayBgmClip;
        [SerializeField] private bool autoPlayGameplayBgm = true;
        [SerializeField] private bool gameplayBgmLoop = true;
        [SerializeField, Range(0f, 1f)] private float bgmMasterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float gameplayBgmVolume = 0.6f;

        private readonly Dictionary<PlayerSoundId, PlayerSfxEntry> playerSfxLookup =
            new Dictionary<PlayerSoundId, PlayerSfxEntry>();

        private AudioSource sfx2DSource;
        private AudioSource bgm2DSource;
        private bool initialized;
        private bool autoBgmStarted;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureInitialized();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void OnValidate()
        {
            if (!initialized)
            {
                return;
            }

            EnsureAudioSources();
            RebuildLookup();
            ApplyBgmVolume();
        }

        /// <summary>
        /// Plays a registered player SFX clip as 2D one-shot.
        /// </summary>
        public bool PlayPlayerSfx(PlayerSoundId soundId, float volumeScale = 1f)
        {
            EnsureInitialized();

            if (!playerSfxLookup.TryGetValue(soundId, out PlayerSfxEntry entry) || entry.clip == null)
            {
                if (warnIfMissingMapping && soundId != PlayerSoundId.None)
                {
                    Debug.LogWarning($"[AudioManager] Missing SFX mapping for {soundId}.");
                }
                return false;
            }

            float finalVolume = Mathf.Clamp01(entry.volume * volumeScale) * sfxMasterVolume;
            if (finalVolume <= 0f)
            {
                return false;
            }

            sfx2DSource.PlayOneShot(entry.clip, finalVolume);
            return true;
        }

        /// <summary>
        /// Plays configured gameplay BGM clip using BGM channel.
        /// </summary>
        public bool PlayGameplayBgm()
        {
            return PlayBgm(gameplayBgmClip, gameplayBgmLoop, gameplayBgmVolume);
        }

        /// <summary>
        /// Plays a clip in BGM channel (2D looping by default).
        /// </summary>
        public bool PlayBgm(AudioClip clip, bool loop = true, float volumeScale = 1f)
        {
            EnsureInitialized();
            if (clip == null)
            {
                return false;
            }

            bool clipChanged = bgm2DSource.clip != clip;
            bgm2DSource.clip = clip;
            bgm2DSource.loop = loop;
            bgm2DSource.volume = Mathf.Clamp01(bgmMasterVolume * Mathf.Clamp01(volumeScale));

            if (clipChanged || !bgm2DSource.isPlaying)
            {
                bgm2DSource.Play();
            }

            return true;
        }

        public void StopBgm()
        {
            EnsureInitialized();
            bgm2DSource.Stop();
        }

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            EnsureAudioSources();
            RebuildLookup();
            initialized = true;
            TryAutoStartGameplayBgm();
        }

        private void EnsureAudioSources()
        {
            EnsureSfxAudioSource();
            EnsureBgmAudioSource();
        }

        private void EnsureSfxAudioSource()
        {
            if (sfx2DSource != null)
            {
                return;
            }

            sfx2DSource = GetComponent<AudioSource>();
            if (sfx2DSource == null)
            {
                sfx2DSource = gameObject.AddComponent<AudioSource>();
            }

            sfx2DSource.playOnAwake = false;
            sfx2DSource.loop = false;
            sfx2DSource.spatialBlend = 0f;
        }

        private void EnsureBgmAudioSource()
        {
            if (bgm2DSource != null)
            {
                return;
            }

            AudioSource[] sources = GetComponents<AudioSource>();
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource source = sources[i];
                if (source != null && source != sfx2DSource)
                {
                    bgm2DSource = source;
                    break;
                }
            }

            if (bgm2DSource == null)
            {
                bgm2DSource = gameObject.AddComponent<AudioSource>();
            }

            bgm2DSource.playOnAwake = false;
            bgm2DSource.loop = gameplayBgmLoop;
            bgm2DSource.spatialBlend = 0f;
            ApplyBgmVolume();
        }

        private void TryAutoStartGameplayBgm()
        {
            if (autoBgmStarted || !autoPlayGameplayBgm)
            {
                return;
            }

            autoBgmStarted = true;
            PlayGameplayBgm();
        }

        private void ApplyBgmVolume()
        {
            if (bgm2DSource == null)
            {
                return;
            }

            bgm2DSource.volume = Mathf.Clamp01(bgmMasterVolume * gameplayBgmVolume);
        }

        private void RebuildLookup()
        {
            playerSfxLookup.Clear();
            for (int i = 0; i < playerSfxEntries.Count; i++)
            {
                PlayerSfxEntry entry = playerSfxEntries[i];
                if (entry.soundId == PlayerSoundId.None)
                {
                    continue;
                }

                if (entry.volume <= 0f)
                {
                    entry.volume = 1f;
                }

                playerSfxLookup[entry.soundId] = entry;
            }
        }
    }
}
