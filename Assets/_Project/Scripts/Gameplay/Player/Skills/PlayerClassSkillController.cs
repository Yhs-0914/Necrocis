using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Necrocis
{
    public enum PlayerClassType
    {
        None,
        Mage,
        Archer,
        Warrior
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController))]
    public partial class PlayerClassSkillController : MonoBehaviour
    {
        public enum SkillSlot
        {
            Skill1,
            Skill2
        }

        private const string SkillProjectileFallbackPoolName = "__SkillProjectileFallbackSphere";
        private const string SkillEffectFallbackPoolName = "__SkillEffectFallbackSphere";
        private const string SkillAttachedEffectFallbackPoolName = "__SkillAttachedEffectFallbackSphere";
        private const string ArcherSkill2FallbackProjectilePoolName = "__ArcherSkill2FallbackCylinder";

        private enum ProjectileDirectionReferenceAxis
        {
            Up,
            Right,
            Forward
        }

        private readonly struct AreaSkillCandidate
        {
            public AreaSkillCandidate(EnemyController enemy, float distanceSqr)
            {
                Enemy = enemy;
                DistanceSqr = distanceSqr;
            }

            public EnemyController Enemy { get; }
            public float DistanceSqr { get; }
        }

        [System.Serializable]
        private class MageSkill1Config
        {
            public float cooldown = 1f;
            public float radius = 3f;
            public float forwardOffset = 0f;
            public float baseDamage = 0f;
            public float additionalDamage = 0f;
            public float additionalDamageMin = 5f;
            public float additionalDamageMax = 7f;
            public float stunDuration = 1f;
            public GameObject areaEffectPrefab;
            public float areaEffectLifetime = 1f;
            public float fallbackEffectScale = 2.5f;
        }

        [System.Serializable]
        private class MageSkill2Config
        {
            public float cooldown = 3f;
            public float radius = 3.5f;
            public float forwardOffset = 0f;
            public float baseDamage = 15f;
            public float detonationDelay = 3f;
            public float damageTakenIncreaseRatio = 0.1f;
            public float damageTakenIncreaseDuration = 3f;
            public GameObject markEffectPrefab;
            public float markEffectLifetime = 3f;
            public float markFallbackEffectScale = 0.8f;
            public float markHeadOffset = 0.2f;
            public bool scaleEffectByTargetSize = true;
            public float effectReferenceTargetHeight = 1.1f;
            public float effectSizeMultiplier = 1f;
            public float effectMinScaleMultiplier = 0.7f;
            public float effectMaxScaleMultiplier = 2.5f;
            public GameObject explosionEffectPrefab;
            public float explosionEffectLifetime = 1f;
            public float fallbackEffectScale = 3f;
        }

        [System.Serializable]
        private class ArcherSkill1Config
        {
            public float cooldown = 1f;
            public int projectileCount = 5;
            public float fanAngle = 55f;
            public float projectileDamage = 3f;
            public float projectileSpeed = 16f;
            public float projectileRange = 4f;
            public float projectileLifeTime = 2f;
            public GameObject projectilePrefab;
            public float projectileScale = 0.25f;
            public float poisonDuration = 5f;
            public float poisonTickInterval = 1f;
            public float poisonTickDamage = 1f;
            public GameObject shootEffectPrefab;
            public float shootEffectLifetime = 0.4f;
        }

        [System.Serializable]
        private class ArcherSkill2Config
        {
            public float cooldown = 3f;
            public float aimDuration = 0.5f;
            public float range = 10f;
            [FormerlySerializedAs("lineHitRadius")]
            public float projectileHitRadius = 0.8f;
            public float projectileVisualLength = 4f;
            public float projectileVisualThickness = 0.8f;
            public float targetForwardOffset = 6f;
            public float projectileDamage = 10f;
            public float projectileTravelSpeed = 16f;
            public float projectileLifeTime = 0f;
            public GameObject projectilePrefab;
            public float projectileScale = 1f;
            public ProjectileDirectionReferenceAxis prefabDirectionAxis = ProjectileDirectionReferenceAxis.Right;
            public Vector3 prefabRotationOffsetEuler = Vector3.zero;
            public bool autoRollToCamera = true;
            public ProjectileDirectionReferenceAxis prefabSurfaceNormalAxis = ProjectileDirectionReferenceAxis.Forward;
            [FormerlySerializedAs("poisonExplosionDamage")]
            public float virusExplosionDamage = 10f;
            [FormerlySerializedAs("poisonExplosionRadius")]
            public float virusExplosionRadius = 1.8f;
            [FormerlySerializedAs("poisonExplosionEffectPrefab")]
            public GameObject virusExplosionEffectPrefab;
            [FormerlySerializedAs("poisonExplosionEffectLifetime")]
            public float virusExplosionEffectLifetime = 1f;
            public bool enableAfterImage = true;
            public float afterImageInterval = 0.03f;
            public float afterImageFadeDuration = 0.15f;
            public float afterImageStartAlpha = 0.4f;
            public int afterImageMaxVisibleCount = 3;
        }

        [System.Serializable]
        private class WarriorSkill1Config
        {
            public float cooldown = 4f;
            public float range = 2.5f;
            public float forwardAngle = 120f;  // 전방 탐색 각도 (좌우 각 60도)
            public float damage = 6f;
            public float bleedDuration = 3f;
            public float bleedTickInterval = 1f;
            public float bleedTickDamage = 2f;
            public GameObject hitEffectPrefab;
            public float hitEffectLifetime = 0.5f;
            public float fallbackEffectScale = 0.8f;
        }

        [System.Serializable]
        private class WarriorSkill2Config
        {
            public float cooldown = 8f;
            public float searchRange = 6f;   // 돌진 대상 탐색 범위
            public float dashSpeed = 18f;    // 돌진 속도
            public float damage = 11f;
            public float rootDuration = 2f;  // 구속(이동불가) 시간
            public float searchAngle = 90f;  // 전방 탐색 각도
            public GameObject hitEffectPrefab;
            public float hitEffectLifetime = 0.5f;
            public float fallbackEffectScale = 1.0f;
        }

        [Header("Class")]
        [SerializeField] private PlayerClassType currentClass = PlayerClassType.None;

        [Header("Shared")]
        [SerializeField] private LayerMask enemyMask = ~0;
        [SerializeField] private int overlapBufferSize = 32;
        [SerializeField] private Transform skillSpawnPoint;
        [SerializeField] private float projectileSpawnOffset = 0.65f;
        [SerializeField] private float projectileSpawnHeight = 1f;
        [SerializeField] private float skillVerticalOffset = 2f;
        [SerializeField] private float skillHitHeightOffset = 0.75f;
        [SerializeField] private float skillHitVerticalHalfHeight = 4f;
        [SerializeField, Min(1)] private int maxAreaSkillHitTargets = 32;
        [SerializeField] private bool enableDebugLogs = true;

        [Header("Mage")]
        [SerializeField] private MageSkill1Config mageSkill1 = new MageSkill1Config();
        [SerializeField] private MageSkill2Config mageSkill2 = new MageSkill2Config();

        [Header("Archer")]
        [SerializeField] private ArcherSkill1Config archerSkill1 = new ArcherSkill1Config();
        [SerializeField] private ArcherSkill2Config archerSkill2 = new ArcherSkill2Config();

        [Header("Warrior")]
        [SerializeField] private WarriorSkill1Config warriorSkill1 = new WarriorSkill1Config();
        [SerializeField] private WarriorSkill2Config warriorSkill2 = new WarriorSkill2Config();
        [SerializeField] private bool autoTargetForwardEnemyForArcherSkill2 = true;
        [SerializeField] private float archerSkill2AutoTargetAngle = 90f;

        [Header("Test Cooldown Override")]
        [SerializeField] private bool useTestCooldownOverride = true;
        [SerializeField] private float testSkill1Cooldown = 1f;
        [SerializeField] private float testSkill2Cooldown = 3f;

        private readonly List<AreaSkillCandidate> areaSkillCandidates = new List<AreaSkillCandidate>(64);

        private PlayerController playerController;
        private Collider[] overlapBuffer;

        private float nextSkill1ReadyTime;
        private float nextSkill2ReadyTime;

        private bool archerSkill2Running;

        private PlayerStats CurrentPlayerStats => playerController != null ? playerController.Stats : PlayerStats.Instance;

        public bool ConsumesSkillInput => enabled && currentClass != PlayerClassType.None;
        public PlayerClassType CurrentClass => currentClass;
        public event Action<SkillSlot, float> CooldownStarted;
        public event Action<SkillSlot> CooldownReset;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            if (enemyMask.value == 0)
            {
                enemyMask = ~0;
                Debug.LogWarning("[PlayerClassSkillController] enemyMask was Nothing. Fallback to Everything.");
            }

            ApplyTestCooldownOverrideIfNeeded();
            EnsureOverlapBuffer();
            ResolveSkillSpawnPoint();
        }

        private void OnEnable()
        {
            LevelUpManager.OnJobChanged += HandleJobChanged;
            ApplyJob(LevelUpManager.GetCurrentJob());
        }

        private void Update()
        {
            if (!ShouldAcceptInput())
            {
                return;
            }

            InputManager input = InputManager.Instance;
            if (input == null)
            {
                return;
            }

            if (input.Skill1Action.WasPressedThisFrame())
            {
                TryUseSkill1();
            }

            if (input.Skill2Action.WasPressedThisFrame())
            {
                TryUseSkill2();
            }
        }

        private void OnDisable()
        {
            LevelUpManager.OnJobChanged -= HandleJobChanged;
            ResetCooldownState();
        }

        public void ApplyJob(JobType job)
        {
            SetClass(MapJobToClass(job));
        }

        public void SetClass(PlayerClassType newClass, bool resetCooldown = true)
        {
            currentClass = newClass;

            if (!resetCooldown)
            {
                return;
            }

            ResetCooldownState();
        }

        public bool IsSkillCoolingDown(SkillSlot slot)
        {
            return GetRemainingCooldown(slot) > 0f;
        }

        public float GetRemainingCooldown(SkillSlot slot)
        {
            float now = Time.time;
            float nextReady = slot == SkillSlot.Skill1 ? nextSkill1ReadyTime : nextSkill2ReadyTime;
            return Mathf.Max(0f, nextReady - now);
        }

        public float GetConfiguredCooldown(SkillSlot slot)
        {
            return currentClass switch
            {
                PlayerClassType.Mage => slot == SkillSlot.Skill1 ? Mathf.Max(0f, mageSkill1.cooldown) : Mathf.Max(0f, mageSkill2.cooldown),
                PlayerClassType.Archer => slot == SkillSlot.Skill1 ? Mathf.Max(0f, archerSkill1.cooldown) : Mathf.Max(0f, archerSkill2.cooldown),
                PlayerClassType.Warrior => slot == SkillSlot.Skill1 ? Mathf.Max(0f, warriorSkill1.cooldown) : Mathf.Max(0f, warriorSkill2.cooldown),
                _ => 0f
            };
        }

        private void HandleJobChanged(JobType job)
        {
            ApplyJob(job);
        }

        private static PlayerClassType MapJobToClass(JobType job)
        {
            return job switch
            {
                JobType.Mage => PlayerClassType.Mage,
                JobType.Archer => PlayerClassType.Archer,
                JobType.Warrior => PlayerClassType.Warrior,
                _ => PlayerClassType.None
            };
        }

        private bool ShouldAcceptInput()
        {
            if (!Application.isFocused || Time.timeSinceLevelLoad < 0.5f)
            {
                return false;
            }

            if (playerController == null || playerController.IsDead)
            {
                return false;
            }

            return currentClass != PlayerClassType.None;
        }

        private void ApplyTestCooldownOverrideIfNeeded()
        {
            if (!useTestCooldownOverride)
            {
                return;
            }

            float skill1Cooldown = Mathf.Max(0f, testSkill1Cooldown);
            float skill2Cooldown = Mathf.Max(0f, testSkill2Cooldown);

            mageSkill1.cooldown = skill1Cooldown;
            archerSkill1.cooldown = skill1Cooldown;
            warriorSkill1.cooldown = skill1Cooldown;
            mageSkill2.cooldown = skill2Cooldown;
            archerSkill2.cooldown = skill2Cooldown;
            warriorSkill2.cooldown = skill2Cooldown;
        }

        private void TryUseSkill1()
        {
            if (!CanUseSkillSlot(SkillSlot.Skill1))
            {
                return;
            }

            AudioManager.Instance?.PlaySFX("SkillUse"); // [Sound] 스킬1 사용

            switch (currentClass)
            {
                case PlayerClassType.Mage:
                    if (!TryStartCooldown(ref nextSkill1ReadyTime, mageSkill1.cooldown, "Mage Skill E", SkillSlot.Skill1))
                    {
                        return;
                    }

                    AudioManager.Instance?.PlaySFX("MageSkill1");
                    ExecuteMageSkill1();
                    break;

                case PlayerClassType.Archer:
                    if (!TryStartCooldown(ref nextSkill1ReadyTime, archerSkill1.cooldown, "Archer Skill E", SkillSlot.Skill1))
                    {
                        return;
                    }

                    ExecuteArcherSkill1FanShot();
                    break;

                case PlayerClassType.Warrior:
                    if (!TryStartCooldown(ref nextSkill1ReadyTime, warriorSkill1.cooldown, "Warrior Skill E", SkillSlot.Skill1))
                    {
                        return;
                    }

                    AudioManager.Instance?.PlaySFX("WarriorSkill1");
                    ExecuteWarriorSkill1Bite();
                    break;
            }
        }

        private void TryUseSkill2()
        {
            if (!CanUseSkillSlot(SkillSlot.Skill2))
            {
                return;
            }

            AudioManager.Instance?.PlaySFX("SkillUse"); // [Sound] 스킬2 사용

            switch (currentClass)
            {
                case PlayerClassType.Mage:
                    Vector3 mageSkill2Center = GetSkillCenter(mageSkill2.forwardOffset);
                    if (!TryFindNearestEnemyInRadius(mageSkill2Center, mageSkill2.radius, out EnemyController mageSkill2Target))
                    {
                        if (enableDebugLogs)
                        {
                            Debug.Log("Mage Skill R failed: no enemy in range.");
                        }

                        return;
                    }

                    if (!TryStartCooldown(ref nextSkill2ReadyTime, mageSkill2.cooldown, "Mage Skill R", SkillSlot.Skill2))
                    {
                        return;
                    }

                    AudioManager.Instance?.PlaySFX("MageSkill2");
                    StartCoroutine(ExecuteMageSkill2(mageSkill2Target));
                    break;

                case PlayerClassType.Archer:
                    if (archerSkill2Running)
                    {
                        return;
                    }

                    if (!TryStartCooldown(ref nextSkill2ReadyTime, archerSkill2.cooldown, "Archer Skill R", SkillSlot.Skill2))
                    {
                        return;
                    }

                    StartCoroutine(ExecuteArcherSkill2());
                    break;

                case PlayerClassType.Warrior:
                    if (!TryStartCooldown(ref nextSkill2ReadyTime, warriorSkill2.cooldown, "Warrior Skill R", SkillSlot.Skill2))
                    {
                        return;
                    }

                    StartCoroutine(ExecuteWarriorSkill2Dash());
                    break;
            }
        }

        private bool TryStartCooldown(ref float nextReadyTime, float cooldown, string label, SkillSlot slot)
        {
            float now = Time.time;
            if (now < nextReadyTime)
            {
                if (enableDebugLogs)
                {
                    float remain = Mathf.Max(0f, nextReadyTime - now);
                    Debug.Log($"[{label}] Cooldown: {remain:0.00}s");
                }

                return false;
            }

            float effectiveCooldown = PlayerCombatCalculator.GetSkillCooldown(cooldown, CurrentPlayerStats);
            nextReadyTime = now + effectiveCooldown;
            CooldownStarted?.Invoke(slot, effectiveCooldown);
            return true;
        }

        private bool CanUseSkillSlot(SkillSlot slot)
        {
            int skillSlotIndex = slot == SkillSlot.Skill1 ? 1 : 2;
            int requiredLevel = LevelUpManager.GetSkillUnlockLevel(skillSlotIndex);
            int currentLevel = LevelUpManager.GetCurrentLevel();
            if (LevelUpManager.IsSkillUnlocked(skillSlotIndex))
            {
                return true;
            }

            if (enableDebugLogs)
            {
                string skillKey = slot == SkillSlot.Skill1 ? "E" : "R";
                Debug.Log($"[Skill {skillKey}] Locked: requires level {requiredLevel}. Current level {currentLevel}.");
            }

            return false;
        }

        private void ResetCooldownState()
        {
            nextSkill1ReadyTime = 0f;
            nextSkill2ReadyTime = 0f;
            archerSkill2Running = false;
            StopAllCoroutines();
            CooldownReset?.Invoke(SkillSlot.Skill1);
            CooldownReset?.Invoke(SkillSlot.Skill2);
        }

        // 전방 각도(forwardAngle) 안에 있는 가장 가까운 적을 찾음
    }
}
