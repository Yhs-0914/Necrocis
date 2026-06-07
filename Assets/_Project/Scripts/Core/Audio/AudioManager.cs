using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// Centralized audio manager supporting both serialized PlayerSoundId mappings and Resources string keys.
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

        private class ClipData
        {
            public AudioClip clip;
            public float volume;
            public float pitchVariance;
        }

        private static readonly (string key, string file, float volume, float pitchVariance)[] ResourceSfxEntries =
        {
            ("PlayerFootstep", "Audio/Player/캐릭터 이동", 1f, 0.08f),
            ("PlayerDash", "Audio/Player/dash", 0.4f, 0.05f),
            ("PlayerHit", "Audio/Player/피격", 1f, 0.05f),
            ("PlayerDeath", "Audio/Player/플레이어 사망", 1f, 0f),
            ("MeleeAttack", "Audio/Player/근거리 박치기", 1f, 0.05f),
            ("RangedAttack", "Audio/Player/중거리 침뱉기", 1f, 0.05f),
            ("EnemyCharge", "Audio/Player/전사 돌격", 1f, 0f),
            ("LevelUp", "Audio/Player/레벨업", 1f, 0f),
            ("SkillUse", "Audio/Player/스킬 사용", 1f, 0f),
            ("JobSelect", "Audio/Player/직업 선택", 1f, 0f),
            ("WarriorSkill1", "Audio/Player/전사 1차", 1f, 0f),
            ("WarriorSkill2", "Audio/Player/전사 2차", 1f, 0f),
            ("MageSkill1", "Audio/Player/법사 1차", 1f, 0f),
            ("MageSkill2", "Audio/Player/법사 2차", 1f, 0f),
            ("BossDeath", "Audio/보스몬스터 사망", 0.8f, 0f),
            ("BossDung", "Audio/보스 오물", 0.4f, 0f),
        };

        private static readonly (string key, string file, float volume)[] ResourceBgmEntries =
        {
            ("InGame", "Audio/BGM1", 0.3f),
        };

        private static readonly Dictionary<PlayerSoundId, string> PlayerSoundFallbackKeys =
            new Dictionary<PlayerSoundId, string>
            {
                { PlayerSoundId.MeleeAttack, "MeleeAttack" },
                { PlayerSoundId.RangedAttack, "RangedAttack" },
                { PlayerSoundId.Death, "PlayerDeath" },
            };

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

                instance.EnsureInitialized();
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

        [Header("Resources Fallback")]
        [SerializeField] private bool loadResourceAudio = true;

        private readonly Dictionary<PlayerSoundId, PlayerSfxEntry> playerSfxLookup =
            new Dictionary<PlayerSoundId, PlayerSfxEntry>();
        private readonly Dictionary<string, ClipData> resourceSfxLookup = new Dictionary<string, ClipData>();
        private readonly Dictionary<string, ClipData> resourceBgmLookup = new Dictionary<string, ClipData>();

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

        public bool PlayPlayerSfx(PlayerSoundId soundId, float volumeScale = 1f)
        {
            EnsureInitialized();

            if (playerSfxLookup.TryGetValue(soundId, out PlayerSfxEntry entry) && entry.clip != null)
            {
                PlayOneShot(entry.clip, entry.volume, 0f, volumeScale);
                return true;
            }

            if (PlayerSoundFallbackKeys.TryGetValue(soundId, out string fallbackKey))
            {
                return PlaySFX(fallbackKey, volumeScale);
            }

            if (warnIfMissingMapping && soundId != PlayerSoundId.None)
            {
                Debug.LogWarning($"[AudioManager] Missing SFX mapping for {soundId}.");
            }

            return false;
        }

        public bool PlaySFX(string clipName, float volumeScale = 1f)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(clipName))
            {
                return false;
            }

            if (!resourceSfxLookup.TryGetValue(clipName, out ClipData data) || data.clip == null)
            {
                if (warnIfMissingMapping)
                {
                    Debug.LogWarning($"[AudioManager] SFX 없음: '{clipName}'");
                }

                return false;
            }

            PlayOneShot(data.clip, data.volume, data.pitchVariance, volumeScale);
            return true;
        }

        public bool PlayGameplayBgm()
        {
            if (gameplayBgmClip != null)
            {
                return PlayBgm(gameplayBgmClip, gameplayBgmLoop, gameplayBgmVolume);
            }

            return PlayBGM("InGame");
        }

        public bool PlayBGM(string clipName)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(clipName))
            {
                return false;
            }

            if (resourceBgmLookup.TryGetValue(clipName, out ClipData data) && data.clip != null)
            {
                return PlayBgm(data.clip, true, data.volume);
            }

            if (clipName == "InGame" && gameplayBgmClip != null)
            {
                return PlayGameplayBgm();
            }

            if (warnIfMissingMapping)
            {
                Debug.LogWarning($"[AudioManager] BGM 없음: '{clipName}'");
            }

            return false;
        }

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

        public void StopBGM()
        {
            StopBgm();
        }

        public void StopBgm()
        {
            EnsureInitialized();
            bgm2DSource.Stop();
        }

        public void SetMasterVolume(float value)
        {
            sfxMasterVolume = Mathf.Clamp01(value);
            bgmMasterVolume = Mathf.Clamp01(value);
            ApplyBgmVolume();
        }

        public void SetBGMVolume(float value)
        {
            gameplayBgmVolume = Mathf.Clamp01(value);
            ApplyBgmVolume();
        }

        public void SetSFXVolume(float value)
        {
            sfxMasterVolume = Mathf.Clamp01(value);
        }

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            EnsureAudioSources();
            LoadResourceClips();
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

        private void LoadResourceClips()
        {
            resourceSfxLookup.Clear();
            resourceBgmLookup.Clear();

            if (!loadResourceAudio)
            {
                return;
            }

            foreach ((string key, string file, float volume, float pitchVariance) in ResourceSfxEntries)
            {
                AudioClip clip = Resources.Load<AudioClip>(file);
                if (clip != null)
                {
                    resourceSfxLookup[key] = new ClipData
                    {
                        clip = clip,
                        volume = volume,
                        pitchVariance = pitchVariance,
                    };
                }
                else if (warnIfMissingMapping)
                {
                    Debug.LogWarning($"[AudioManager] SFX 파일 없음: Resources/{file}");
                }
            }

            foreach ((string key, string file, float volume) in ResourceBgmEntries)
            {
                AudioClip clip = Resources.Load<AudioClip>(file);
                if (clip != null)
                {
                    resourceBgmLookup[key] = new ClipData
                    {
                        clip = clip,
                        volume = volume,
                        pitchVariance = 0f,
                    };
                }
                else if (warnIfMissingMapping)
                {
                    Debug.LogWarning($"[AudioManager] BGM 파일 없음: Resources/{file}");
                }
            }
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

        private void PlayOneShot(AudioClip clip, float baseVolume, float pitchVariance, float volumeScale)
        {
            if (clip == null)
            {
                return;
            }

            EnsureAudioSources();
            float originalPitch = sfx2DSource.pitch;
            sfx2DSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariance, pitchVariance);
            sfx2DSource.PlayOneShot(clip, Mathf.Clamp01(baseVolume * volumeScale) * sfxMasterVolume);
            sfx2DSource.pitch = originalPitch;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            _ = Instance;
        }
    }
}
