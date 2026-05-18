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

        private readonly Dictionary<PlayerSoundId, PlayerSfxEntry> playerSfxLookup =
            new Dictionary<PlayerSoundId, PlayerSfxEntry>();

        private AudioSource sfx2DSource;
        private bool initialized;

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

            EnsureAudioSource();
            RebuildLookup();
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

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            EnsureAudioSource();
            RebuildLookup();
            initialized = true;
        }

        private void EnsureAudioSource()
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
