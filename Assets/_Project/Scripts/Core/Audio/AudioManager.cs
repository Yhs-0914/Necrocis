using System;
using System.Collections;
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
            // ─────────────────────────────────────────────
            // 플레이어 - 이동 / 전투 / 성장
            // ─────────────────────────────────────────────
            ("PlayerFootstep", "Audio/Player/캐릭터 이동", 1f, 0.08f),
            ("PlayerDash", "Audio/Player/player_dash", 0.4f, 0.05f),
            ("PlayerHit", "Audio/Player/피격", 1f, 0.05f),
            ("PlayerDeath", "Audio/Player/플레이어 사망", 1f, 0f),
            ("MeleeAttack", "Audio/Player/근거리 박치기", 1f, 0.05f),
            ("RangedAttack", "Audio/Player/중거리 침뱉기", 1f, 0.05f),
            ("LevelUp", "Audio/Player/레벨업 소리", 1f, 0f),
            ("SkillUse", "Audio/Player/스킬 사용", 1f, 0f),
            ("JobSelect", "Audio/Player/직업 선택", 1f, 0f),

            // UI - 버튼 / 창 / 선택 피드백
            ("UISelect", "Audio/UI/선택", 1f, 0f),
            ("UIInvalid", "Audio/UI/선택 불가", 1f, 0f),
            ("StatWindow", "Audio/UI/스탯창", 1f, 0f),
            ("SettingsClose", "Audio/UI/설정 닫기", 1f, 0f),
            ("SettingsOpen", "Audio/UI/설정버튼", 1f, 0f),
            ("ButtonClick", "Audio/UI/버튼 클릭", 1f, 0f),
            ("UIClick", "Audio/UI/UI 클릭", 1f, 0f),
            ("UIClose", "Audio/UI/ui-close-sfx", 1f, 0f),
            ("SkillCooldown", "Audio/Player/스킬 쿨타임", 1f, 0f),
            ("PortalEnter", "Audio/Player/포털 진입", 1f, 0f),
            ("StatAttackRangeSelect", "Audio/UI/스탯 공격사거리 선택", 1f, 0f),
            ("StatHealthSelect", "Audio/UI/스탯 체력 선택", 1f, 0f),
            ("StatMoveSpeedSelect", "Audio/UI/스탯 이동속도 선택", 1f, 0f),
            ("StatMagicSelect", "Audio/UI/스탯 마력 선택", 1f, 0f),
            ("StatAttackPowerSelect", "Audio/UI/스탯 공격력 선택", 1f, 0f),

            // 플레이어 - 직업 스킬
            ("WarriorSkill1", "Audio/Player/전사 E스킬", 1f, 0f),
            ("WarriorSkill2", "Audio/Player/전사 R스킬", 1f, 0f),
            ("MageSkill1", "Audio/Player/마법사 E스킬", 3f, 0f),
            ("MageSkill2", "Audio/Player/마법사 R스킬", 1f, 0f),
            ("ArcherSkill1", "Audio/Player/아처 E스킬", 1f, 0f),
            ("ArcherSkill2", "Audio/Player/아처 R스킬", 1f, 0f),

            // ─────────────────────────────────────────────
            // 아이템 - 공통 획득 / 카테고리
            // ─────────────────────────────────────────────
            ("ItemPickup", "Audio/Player/보물상자 아이템 먹는소리", 1f, 0f),
            ("ItemBasicProjectile", "Audio/Item/아이템_기본 투사체 변화", 1f, 0f),
            ("ItemAttackStyle", "Audio/Item/아이템_공격 방식 변화", 1f, 0f),
            ("ItemHighRiskHighReturn", "Audio/Item/아이템_위험 보상", 1f, 0f),
            ("ItemSurvivalDefense", "Audio/Item/아이템_생존 방어", 1f, 0f),
            ("ItemSummonPet", "Audio/Item/아이템_소환수", 1f, 0f),
            ("ItemChainClear", "Audio/Item/아이템_연쇄처치 광역 정리", 1f, 0f),
            ("ItemBossSpecialized", "Audio/Item/아이템_보스 특화", 1f, 0f),
            ("ItemFunRandom", "Audio/Item/아이템_무작위", 1f, 0f),

            // ─────────────────────────────────────────────
            // 일반 몬스터 / 엘리트 몬스터
            // ─────────────────────────────────────────────
            ("EnemyCharge", "Audio/Player/전사 돌격", 1f, 0f),
            ("EnemyAttack", "Audio/Monster/몬스터 타격", 1f, 0.05f),
            ("EnemyHit", "Audio/Monster/몬스터 피격", 1f, 0.05f),
            ("EnemyDeath", "Audio/Monster/몬스터 사망", 1f, 0.05f),

            // ─────────────────────────────────────────────
            // 보스 - 공통
            // ─────────────────────────────────────────────
            ("BossRoar", "Audio/Boss/보스 으르렁", 1f, 0f),
            ("BossPhaseChange", "Audio/Boss/보스 페이즈 전환", 1f, 0f),
            ("BossSpawn", "Audio/Boss/맵 보스", 1f, 0f),
            ("BossDeath", "Audio/보스몬스터 사망", 0.8f, 0f),
            ("IntestineBossDeath", "Audio/Boss/장 보스 사망", 1f, 0f),
            ("LiverBossDeath", "Audio/Boss/간 보스 사망", 1f, 0f),
            ("StomachBossDeath", "Audio/Boss/위 보스 사망", 1f, 0f),
            ("LungBossDeath", "Audio/Boss/폐 보스 사망", 1f, 0f),
            ("IntestineBossImpact", "Audio/Boss/장 보스 임팩트", 1f, 0f),
            ("LiverBossImpact", "Audio/Boss/간 보스 임팩트", 1f, 0f),
            ("StomachBossImpact", "Audio/Boss/위 보스 임팩트", 1f, 0f),
            ("BossDung", "Audio/보스 오물", 0.4f, 0f),

            // 보스 - 장
            ("IntestineSkill1", "Audio/Boss/장1", 1f, 0f),
            ("IntestineSkill2", "Audio/Boss/장1-2", 1f, 0f),
            ("IntestineLand", "Audio/Boss/장2 착지", 1f, 0f),

            // 보스 - 간
            ("LiverBloodThrow", "Audio/Boss/간1 피던지기", 1f, 0f),
            ("LiverBloodBurst", "Audio/Boss/간1 혈액 폭발", 1f, 0f),

            // 보스 - 위
            ("StomachImpact", "Audio/Boss/위 보스 기본 임팩트", 1f, 0f),
            ("StomachHeadbutt", "Audio/Boss/위 페이즈1 박치기", 1f, 0f),
            ("StomachCharge", "Audio/Boss/위 페이즈1 스킬 전 차징", 1f, 0f),
            ("StomachAcidReady", "Audio/Boss/위 페이즈2 산성 발사준비", 1f, 0f),
            ("StomachAcidFire", "Audio/Boss/위 페이즈2 산성발사 찐", 1f, 0f),

            // 보스 - 폐
            ("LungPhase2", "Audio/Boss/폐 2", 1f, 0f),
            ("LungAccelerate", "Audio/Boss/폐 가속", 1f, 0f),
            ("LungHighSpeed", "Audio/Boss/폐1고속이동", 1f, 0f),
            ("LungPhase2Skill", "Audio/Boss/폐2-1", 1f, 0f),
            ("LungAmbientImpact", "Audio/BGM/폐 배경음 임팩트", 1f, 0f),
        };

        private static readonly (string key, string file, float volume)[] ResourceBgmEntries =
        {
            // 기본 / 허브 / 클리어
            ("InGame", "Audio/BGM1", 0.3f),
            ("Hub", "Audio/BGM/메인광장", 0.3f),
            ("GameClear", "Audio/BGM/게임 클리어", 0.7f),

            // 바이옴별 배경음
            ("IntestineMap", "Audio/BGM/장 배경음", 0.3f),
            ("LiverMap", "Audio/BGM/간 배경음", 0.3f),
            ("StomachMap", "Audio/BGM/위 배경음", 0.3f),
            ("LungMap", "Audio/BGM/폐 배경음", 0.3f),
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
        [SerializeField, Min(0f)] private float bgmFadeDuration = 1f;

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
        private float activeBgmVolumeScale = 1f;
        private Coroutine bgmFadeRoutine;

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
            SetBGMVolume(GameSettings.BgmVolume);
            SetSFXVolume(GameSettings.SfxVolume);
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

        public bool PlayTimedSFX(string clipName, float duration, float volumeScale = 1f)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(clipName)
                || !resourceSfxLookup.TryGetValue(clipName, out ClipData data)
                || data.clip == null)
            {
                if (warnIfMissingMapping)
                {
                    Debug.LogWarning($"[AudioManager] SFX 없음: '{clipName}'");
                }

                return false;
            }

            GameObject sourceObject = new GameObject($"TimedSFX_{clipName}");
            sourceObject.transform.SetParent(transform);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.pitch = 1f + UnityEngine.Random.Range(-data.pitchVariance, data.pitchVariance);
            source.volume = Mathf.Clamp01(data.volume * volumeScale) * sfxMasterVolume;
            source.clip = data.clip;
            source.Play();

            float playDuration = Mathf.Min(Mathf.Max(0.01f, duration), data.clip.length);
            Destroy(sourceObject, playDuration);
            return true;
        }

        public bool PlayItemCategorySFX(PlayerItemCategory category)
        {
            string key = category switch
            {
                PlayerItemCategory.BasicProjectile => "ItemBasicProjectile",
                PlayerItemCategory.AttackStyle => "ItemAttackStyle",
                PlayerItemCategory.HighRiskHighReturn => "ItemHighRiskHighReturn",
                PlayerItemCategory.SurvivalDefense => "ItemSurvivalDefense",
                PlayerItemCategory.SummonPet => "ItemSummonPet",
                PlayerItemCategory.ChainClear => "ItemChainClear",
                PlayerItemCategory.BossSpecialized => "ItemBossSpecialized",
                PlayerItemCategory.FunRandom => "ItemFunRandom",
                _ => null,
            };

            return key != null && PlaySFX(key);
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

            float targetScale = Mathf.Clamp01(volumeScale);
            if (bgm2DSource.clip == clip)
            {
                activeBgmVolumeScale = targetScale;
                bgm2DSource.loop = loop;
                ApplyBgmVolume();
                if (!bgm2DSource.isPlaying)
                {
                    bgm2DSource.Play();
                }

                return true;
            }

            if (bgmFadeRoutine != null)
            {
                StopCoroutine(bgmFadeRoutine);
            }

            bgmFadeRoutine = StartCoroutine(FadeToBgm(clip, loop, targetScale));
            return true;
        }

        private IEnumerator FadeToBgm(AudioClip clip, bool loop, float targetScale)
        {
            float duration = Mathf.Max(0f, bgmFadeDuration);
            if (bgm2DSource.isPlaying && duration > 0f)
            {
                float startVolume = bgm2DSource.volume;
                for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
                {
                    bgm2DSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                    yield return null;
                }
            }

            bgm2DSource.Stop();
            bgm2DSource.clip = clip;
            bgm2DSource.loop = loop;
            activeBgmVolumeScale = targetScale;
            bgm2DSource.volume = 0f;
            bgm2DSource.Play();

            float targetVolume = GetBgmTargetVolume();
            if (duration > 0f)
            {
                for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
                {
                    bgm2DSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                    yield return null;
                }
            }

            bgm2DSource.volume = targetVolume;
            bgmFadeRoutine = null;
        }

        public void StopBGM()
        {
            StopBgm();
        }

        public void StopBgm()
        {
            EnsureInitialized();
            if (bgmFadeRoutine != null)
            {
                StopCoroutine(bgmFadeRoutine);
                bgmFadeRoutine = null;
            }
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

            bgm2DSource.volume = GetBgmTargetVolume();
        }

        private float GetBgmTargetVolume()
        {
            return Mathf.Clamp01(bgmMasterVolume * gameplayBgmVolume * activeBgmVolumeScale);
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
