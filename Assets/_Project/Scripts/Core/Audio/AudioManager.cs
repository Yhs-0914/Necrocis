using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        // SFX 매핑: (PlaySFX 이름, Resources/Audio/ 파일명, 볼륨, 피치변화)
        private static readonly (string key, string file, float vol, float pitchVar)[] SfxEntries =
        {
            ("PlayerFootstep", "Audio/Player/캐릭터 이동",    1f, 0.08f),
            ("PlayerDash",     "Audio/Player/player_dash",    0.4f, 0.05f),
            ("PlayerHit",      "Audio/Player/피격",           1f, 0.05f),
            ("PlayerDeath",    "Audio/Player/플레이어 사망",  1f, 0f),
            ("MeleeAttack",    "Audio/Player/근거리 박치기",  1f, 0.05f),
            ("RangedAttack",   "Audio/Player/중거리 침뱉기",  1f, 0.05f),
            ("EnemyCharge",    "Audio/Player/전사 돌격",      1f, 0f),
            ("LevelUp",        "Audio/Player/레벨업",         1f, 0f),
            ("SkillUse",       "Audio/Player/스킬 사용",      1f, 0f),
            ("JobSelect",      "Audio/Player/직업 선택",      1f, 0f),
            ("WarriorSkill1",  "Audio/Player/전사 E스킬",      1f, 0f),
            ("WarriorSkill2",  "Audio/Player/전사 R스킬",      1f, 0f),
            ("MageSkill1",     "Audio/Player/마법사 E스킬",    2f, 0f),
            ("MageSkill2",     "Audio/Player/마법사 R스킬",    1f, 0f),
            ("ArcherSkill1",   "Audio/Player/아처 E스킬",      1f, 0f),
            ("ArcherSkill2",   "Audio/Player/아처 R스킬",      1f, 0f),
            ("EnemyAttack",    "Audio/Monster/몬스터 타격",    1f, 0.05f),
            ("EnemyHit",       "Audio/Monster/몬스터 피격",    1f, 0.05f),
            ("EnemyDeath",     "Audio/Monster/몬스터 사망",    1f, 0.05f),
            ("BossRoar",       "Audio/Boss/보스 으르렁",              1f, 0f),
            ("BossPhaseChange","Audio/Boss/보스 페이즈 전환",         1f, 0f),
            ("IntestineSkill1","Audio/Boss/장1",                     1f, 0f),
            ("IntestineSkill2","Audio/Boss/장1-2",                   1f, 0f),
            ("IntestineLand",  "Audio/Boss/장2 착지",                 1f, 0f),
            ("LiverBloodThrow","Audio/Boss/간1 피던지기",             1f, 0f),
            ("LiverBloodBurst","Audio/Boss/간1 혈액 폭발",            1f, 0f),
            ("StomachImpact",  "Audio/Boss/위 보스 기본 임팩트",       1f, 0f),
            ("StomachHeadbutt","Audio/Boss/위 페이즈1 박치기",         1f, 0f),
            ("StomachCharge",  "Audio/Boss/위 페이즈1 스킬 전 차징",   1f, 0f),
            ("StomachAcidReady","Audio/Boss/위 페이즈2 산성 발사준비", 1f, 0f),
            ("StomachAcidFire","Audio/Boss/위 페이즈2 산성발사 찐",    1f, 0f),
            ("LungPhase2",     "Audio/Boss/폐 2",                     1f, 0f),
            ("LungAccelerate", "Audio/Boss/폐 가속",                  1f, 0f),
            ("LungHighSpeed",  "Audio/Boss/폐1고속이동",              1f, 0f),
            ("LungPhase2Skill","Audio/Boss/폐2-1",                   1f, 0f),
            ("BossDeath",      "Audio/보스몬스터 사망",       0.8f, 0f),
            ("BossDung",       "Audio/보스 오물",             0.4f, 0f),
        };

        // BGM 매핑
        private static readonly (string key, string file, float vol)[] BgmEntries =
        {
            ("InGame", "Audio/BGM1", 0.3f),
            ("IntestineMap", "Audio/Boss/맵 장1", 0.3f),
        };

        [Header("볼륨")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.6f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

        private AudioSource bgmSource;
        private AudioSource sfxSource;

        private class ClipData
        {
            public AudioClip clip;
            public float volume;
            public float pitchVariance;
        }

        private readonly Dictionary<string, ClipData> sfxMap = new Dictionary<string, ClipData>();
        private readonly Dictionary<string, ClipData> bgmMap = new Dictionary<string, ClipData>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CreateAudioSources();
            LoadClips();
        }

        private void Start()
        {
            PlayBGM("InGame");
        }

        private void CreateAudioSources()
        {
            bgmSource = CreateSource("BGMSource", loop: true);
            sfxSource = CreateSource("SFXSource", loop: false);
        }

        private AudioSource CreateSource(string objName, bool loop)
        {
            GameObject obj = new GameObject(objName);
            obj.transform.SetParent(transform);
            AudioSource src = obj.AddComponent<AudioSource>();
            src.loop = loop;
            src.playOnAwake = false;
            return src;
        }

        private void LoadClips()
        {
            foreach (var (key, file, vol, pitchVar) in SfxEntries)
            {
                AudioClip clip = Resources.Load<AudioClip>(file);
                if (clip != null)
                    sfxMap[key] = new() { clip = clip, volume = vol, pitchVariance = pitchVar };
                else
                    Debug.LogWarning($"[AudioManager] SFX 파일 없음: Resources/Audio/{file}");
            }

            foreach (var (key, file, vol) in BgmEntries)
            {
                AudioClip clip = Resources.Load<AudioClip>(file);
                if (clip != null)
                    bgmMap[key] = new() { clip = clip, volume = vol, pitchVariance = 0f };
                else
                    Debug.LogWarning($"[AudioManager] BGM 파일 없음: Resources/Audio/{file}");
            }
        }

        public void PlaySFX(string clipName)
        {
            if (!sfxMap.TryGetValue(clipName, out ClipData data))
            {
                Debug.LogWarning($"[AudioManager] SFX 없음: '{clipName}'");
                return;
            }

            float pitch = 1f + Random.Range(-data.pitchVariance, data.pitchVariance);
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(data.clip, data.volume * sfxVolume * masterVolume);
        }

        public void PlayBGM(string clipName)
        {
            if (!bgmMap.TryGetValue(clipName, out ClipData data)) return;
            if (bgmSource.clip == data.clip && bgmSource.isPlaying) return;

            bgmSource.clip = data.clip;
            bgmSource.volume = data.volume * bgmVolume * masterVolume;
            bgmSource.Play();
        }

        public void StopBGM() => bgmSource.Stop();

        public void SetMasterVolume(float v) { masterVolume = Mathf.Clamp01(v); RefreshBGMVolume(); }
        public void SetBGMVolume(float v) { bgmVolume = Mathf.Clamp01(v); RefreshBGMVolume(); }
        public void SetSFXVolume(float v) { sfxVolume = Mathf.Clamp01(v); }

        private void RefreshBGMVolume()
        {
            if (bgmSource != null) bgmSource.volume = bgmVolume * masterVolume;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (Instance != null) return;
            new GameObject("AudioManager").AddComponent<AudioManager>();
        }
    }
}
