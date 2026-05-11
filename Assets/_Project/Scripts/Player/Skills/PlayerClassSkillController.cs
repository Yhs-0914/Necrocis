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
    public class PlayerClassSkillController : MonoBehaviour
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
        private const int Skill1UnlockLevel = 10;
        private const int Skill2UnlockLevel = 20;

        private enum ProjectileDirectionReferenceAxis
        {
            Up,
            Right,
            Forward
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
            public float bleedTickDamage = 1.5f;
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

        private readonly HashSet<EnemyController> uniqueEnemies = new HashSet<EnemyController>();

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

            switch (currentClass)
            {
                case PlayerClassType.Mage:
                    if (!TryStartCooldown(ref nextSkill1ReadyTime, mageSkill1.cooldown, "Mage Skill E", SkillSlot.Skill1))
                    {
                        return;
                    }

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
            int requiredLevel = slot == SkillSlot.Skill1 ? Skill1UnlockLevel : Skill2UnlockLevel;
            int currentLevel = LevelUpManager.GetCurrentLevel();
            if (currentLevel >= requiredLevel)
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

        private void ExecuteMageSkill1()
        {
            Vector3 center = GetSkillCenter(mageSkill1.forwardOffset);
            float baseDamage = mageSkill1.baseDamage;
            float bonusMin = Mathf.Min(mageSkill1.additionalDamageMin, mageSkill1.additionalDamageMax);
            float bonusMax = Mathf.Max(mageSkill1.additionalDamageMin, mageSkill1.additionalDamageMax);
            if (bonusMax <= 0f && mageSkill1.additionalDamage > 0f)
            {
                bonusMin = mageSkill1.additionalDamage;
                bonusMax = mageSkill1.additionalDamage;
            }

            int hitCount = ApplyAreaSkill(
                center,
                mageSkill1.radius,
                enemy =>
                {
                    float bonusDamage = UnityEngine.Random.Range(bonusMin, bonusMax + 0.001f);
                    float totalDamage = PlayerCombatCalculator.GetSkillDamage(baseDamage + bonusDamage, CurrentPlayerStats);
                    enemy.TakeDamage(totalDamage);
                    EnemyStatusEffectController status = EnsureStatusController(enemy);
                    status?.ApplyStun(mageSkill1.stunDuration);
                });

            SpawnSkillEffect(
                mageSkill1.areaEffectPrefab,
                center,
                mageSkill1.areaEffectLifetime,
                mageSkill1.fallbackEffectScale,
                new Color(0.35f, 0.8f, 1f, 0.45f));

            if (enableDebugLogs)
            {
                Debug.Log($"Mage Skill E hit {hitCount} enemies. BonusDamage={bonusMin:0.#}~{bonusMax:0.#}");
            }
        }

        private void ExecuteWarriorSkill1Bite()
        {
            Vector3 center = GetSkillCenter(0f);
            if (!TryFindNearestEnemyInForwardArc(center, warriorSkill1.range, warriorSkill1.forwardAngle, out EnemyController target))
            {
                if (enableDebugLogs)
                {
                    Debug.Log("Warrior Skill E failed: no enemy in range.");
                }
                return;
            }

            float damage = PlayerCombatCalculator.GetSkillDamage(warriorSkill1.damage, CurrentPlayerStats);
            target.TakeDamage(damage);

            EnemyStatusEffectController status = EnsureStatusController(target);
            float bleedTickDamage = PlayerCombatCalculator.GetSkillDamage(warriorSkill1.bleedTickDamage, CurrentPlayerStats);
            status?.ApplyBleed(warriorSkill1.bleedDuration, warriorSkill1.bleedTickInterval, bleedTickDamage);

            Vector3 effectPos = GetTargetEffectPosition(target);
            SpawnSkillEffect(
                warriorSkill1.hitEffectPrefab,
                effectPos,
                warriorSkill1.hitEffectLifetime,
                warriorSkill1.fallbackEffectScale,
                new Color(0.85f, 0.1f, 0.1f, 0.6f));

            if (enableDebugLogs)
            {
                Debug.Log($"Warrior Skill E hit {target.name}. Damage={damage}, Bleed={bleedTickDamage}/s for {warriorSkill1.bleedDuration}s");
            }
        }

        private IEnumerator ExecuteWarriorSkill2Dash()
        {
            // 전방 적 탐색
            if (!TryFindForwardEnemyPoint(warriorSkill2.searchRange, warriorSkill2.searchAngle, out Vector3 targetPoint))
            {
                // 전방에 적이 없으면 그냥 전방으로 짧게 돌진
                targetPoint = transform.position + GetFacingDirection() * warriorSkill2.searchRange;
            }

            Vector3 dashTarget = targetPoint;
            dashTarget.y = transform.position.y;

            Vector3 dashDir = (dashTarget - transform.position);
            dashDir.y = 0f;
            float dashDistance = dashDir.magnitude;

            if (dashDistance > 0.01f)
            {
                dashDir = dashDir.normalized;
                float dashDuration = dashDistance / Mathf.Max(0.1f, warriorSkill2.dashSpeed);
                float elapsed = 0f;

                CharacterController cc = playerController.GetComponent<CharacterController>();
                Rigidbody rb = playerController.GetComponent<Rigidbody>();

                while (elapsed < dashDuration)
                {
                    float step = warriorSkill2.dashSpeed * Time.deltaTime;
                    if (cc != null)
                        cc.Move(dashDir * step);
                    else if (rb != null)
                        rb.MovePosition(rb.position + dashDir * step);
                    else
                        playerController.transform.position += dashDir * step;

                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            // 도착 후 범위 내 적에게 데미지 + 구속
            Vector3 hitCenter = GetSkillCenter(0f);
            if (TryFindNearestEnemyInRadius(hitCenter, warriorSkill1.range + 1f, out EnemyController hitTarget))
            {
                float damage = PlayerCombatCalculator.GetSkillDamage(warriorSkill2.damage, CurrentPlayerStats);
                hitTarget.TakeDamage(damage);
                EnemyStatusEffectController status = EnsureStatusController(hitTarget);
                status?.ApplyStun(warriorSkill2.rootDuration);

                Vector3 effectPos = GetTargetEffectPosition(hitTarget);
                SpawnSkillEffect(
                    warriorSkill2.hitEffectPrefab,
                    effectPos,
                    warriorSkill2.hitEffectLifetime,
                    warriorSkill2.fallbackEffectScale,
                    new Color(0.9f, 0.2f, 0.05f, 0.7f));

                if (enableDebugLogs)
                    Debug.Log($"Warrior Skill R hit {hitTarget.name}. Damage={damage}, Root={warriorSkill2.rootDuration}s");
            }
            else if (enableDebugLogs)
            {
                Debug.Log("Warrior Skill R dash: no enemy at destination.");
            }
        }

        private IEnumerator ExecuteMageSkill2(EnemyController target)
        {
            if (target == null || target.IsDead)
            {
                yield break;
            }

            float markLifeTime = Mathf.Max(0.1f, Mathf.Min(mageSkill2.markEffectLifetime, mageSkill2.detonationDelay + 0.05f));
            SpawnAttachedSkillEffect(
                mageSkill2.markEffectPrefab,
                target,
                markLifeTime,
                mageSkill2.markFallbackEffectScale,
                new Color(0.8f, 0.95f, 1f, 0.5f),
                mageSkill2.markHeadOffset);

            float delay = Mathf.Max(0f, mageSkill2.detonationDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (target == null || target.IsDead)
            {
                if (enableDebugLogs)
                {
                    Debug.Log("Mage Skill R canceled: marked target died before detonation.");
                }
                yield break;
            }

            Vector3 targetEffectPosition = GetTargetEffectPosition(target);
            float targetEffectScaleMultiplier = GetTargetEffectScaleMultiplier(target);
            float damage = PlayerCombatCalculator.GetSkillDamage(mageSkill2.baseDamage, CurrentPlayerStats);
            target.TakeDamage(damage);
            EnemyStatusEffectController status = EnsureStatusController(target);
            status?.ApplyDamageTakenIncrease(mageSkill2.damageTakenIncreaseRatio, mageSkill2.damageTakenIncreaseDuration);

            SpawnSkillEffect(
                mageSkill2.explosionEffectPrefab,
                targetEffectPosition,
                mageSkill2.explosionEffectLifetime,
                mageSkill2.fallbackEffectScale,
                new Color(1f, 0.5f, 0.2f, 0.45f),
                targetEffectScaleMultiplier);

            if (enableDebugLogs)
            {
                Debug.Log($"Mage Skill R detonated on {target.name}. Damage={damage:0.##}, Debuff={mageSkill2.damageTakenIncreaseRatio * 100f:0.#}% for {mageSkill2.damageTakenIncreaseDuration:0.##}s");
            }
        }

        private void ExecuteArcherSkill1FanShot()
        {
            SkillProjectileDebuff debuff = new SkillProjectileDebuff
            {
                applyPoison = true,
                poisonDuration = archerSkill1.poisonDuration,
                poisonTickInterval = archerSkill1.poisonTickInterval,
                poisonTickDamage = PlayerCombatCalculator.GetSkillDamage(archerSkill1.poisonTickDamage, CurrentPlayerStats)
            };

            int projectileCount = Mathf.Max(1, archerSkill1.projectileCount);
            float fanAngle = Mathf.Max(0f, archerSkill1.fanAngle);
            projectileCount = Mathf.Max(5, projectileCount);

            float projectileSpeed = Mathf.Max(0.01f, archerSkill1.projectileSpeed);
            float targetRange = archerSkill1.projectileRange > 0f ? archerSkill1.projectileRange : 4f;
            float projectileLifeTime = Mathf.Max(0.05f, targetRange / projectileSpeed);

            Vector3 centerDirection = GetFacingDirection();
            float startAngle = -fanAngle * 0.5f;
            float stepAngle = projectileCount > 1 ? fanAngle / (projectileCount - 1) : 0f;

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = startAngle + stepAngle * i;
                Vector3 shotDirection = Quaternion.AngleAxis(angle, Vector3.up) * centerDirection;

                SpawnSkillProjectile(
                    archerSkill1.projectilePrefab,
                    archerSkill1.projectileScale,
                    PlayerCombatCalculator.GetSkillDamage(archerSkill1.projectileDamage, CurrentPlayerStats),
                    projectileSpeed,
                    projectileLifeTime,
                    shotDirection,
                    debuff,
                    false);
            }

            SpawnSkillEffect(
                archerSkill1.shootEffectPrefab,
                GetProjectileSpawnPosition(centerDirection),
                archerSkill1.shootEffectLifetime,
                0.25f,
                new Color(0.7f, 1f, 0.5f, 0.55f));

            if (enableDebugLogs)
            {
                Debug.Log($"Archer Skill E fan-shot fired {projectileCount} piercing projectiles. FanAngle={fanAngle:0.#}, Range={targetRange:0.#}");
            }
        }

        private IEnumerator ExecuteArcherSkill2()
        {
            archerSkill2Running = true;
            Vector3 direction = GetArcherSkill2Direction();

            float aimDuration = Mathf.Max(0f, archerSkill2.aimDuration);
            if (aimDuration > 0f)
            {
                yield return new WaitForSeconds(aimDuration);
            }

            FireArcherSkill2Projectile(direction);
            if (enableDebugLogs)
            {
                Debug.Log("Archer Skill R fired a piercing cell spear.");
            }

            archerSkill2Running = false;
        }

        private Vector3 GetArcherSkill2Direction()
        {
            Vector3 fallbackDirection = GetFacingDirection();
            if (!autoTargetForwardEnemyForArcherSkill2)
            {
                return fallbackDirection;
            }

            float maxDistance = Mathf.Max(archerSkill2.range, archerSkill2.targetForwardOffset, 1f);
            if (!TryFindForwardEnemyPoint(maxDistance, archerSkill2AutoTargetAngle, out Vector3 targetPoint))
            {
                return fallbackDirection;
            }

            Vector3 toTarget = targetPoint - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                return fallbackDirection;
            }

            return toTarget.normalized;
        }

        private void FireArcherSkill2Projectile(Vector3 direction)
        {
            Vector3 moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : GetFacingDirection();
            float range = Mathf.Max(0.5f, archerSkill2.range);
            float speed = Mathf.Max(0.1f, archerSkill2.projectileTravelSpeed);
            float computedLifeTime = range / speed;
            float lifeTime = archerSkill2.projectileLifeTime > 0f
                ? archerSkill2.projectileLifeTime
                : computedLifeTime;

            Vector3 spawnPosition = GetProjectileSpawnPosition(moveDirection);
            GameObject projectileObject = CreateArcherSkill2ProjectileObject(spawnPosition, moveDirection);
            if (projectileObject == null)
            {
                return;
            }

            SkillProjectile projectile = projectileObject.GetComponent<SkillProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<SkillProjectile>();
            }

            projectile.ConfigureHitDetection(
                archerSkill2.projectileHitRadius,
                skillHitHeightOffset,
                skillHitVerticalHalfHeight);

            projectile.Launch(
                moveDirection,
                PlayerCombatCalculator.GetSkillDamage(archerSkill2.projectileDamage, CurrentPlayerStats),
                speed,
                Mathf.Max(0.05f, lifeTime),
                enemyMask,
                false,
                default(SkillProjectileDebuff),
                HandleArcherSkill2EnemyHit);
        }

        private GameObject CreateArcherSkill2ProjectileObject(Vector3 spawnPosition, Vector3 direction)
        {
            GameObject projectileObject = archerSkill2.projectilePrefab != null
                ? RuntimePool.Acquire(archerSkill2.projectilePrefab)
                : AcquireFallbackVisual(
                    ArcherSkill2FallbackProjectilePoolName,
                    PrimitiveType.Cylinder,
                    archerSkill2.projectileScale,
                    new Color(0.95f, 0.72f, 0.25f, 0.9f));
            if (projectileObject == null)
            {
                return null;
            }

            EnsureProjectilePhysics(projectileObject);
            projectileObject.transform.position = spawnPosition;
            projectileObject.transform.rotation = GetArcherSkill2ProjectileRotation(direction, archerSkill2.projectilePrefab != null);

            if (archerSkill2.projectilePrefab != null)
            {
                projectileObject.transform.localScale *= Mathf.Max(0.01f, archerSkill2.projectileScale);
            }
            else
            {
                float thickness = Mathf.Max(0.1f, archerSkill2.projectileVisualThickness);
                float length = Mathf.Max(0.5f, archerSkill2.projectileVisualLength);
                projectileObject.transform.localScale = new Vector3(thickness, length * 0.5f, thickness);
            }

            ConfigureArcherSkill2AfterImage(projectileObject);
            return projectileObject;
        }

        private void ConfigureArcherSkill2AfterImage(GameObject projectileObject)
        {
            if (projectileObject == null)
            {
                return;
            }

            ProjectileAfterImageTrail afterImageTrail = projectileObject.GetComponent<ProjectileAfterImageTrail>();
            if (afterImageTrail == null)
            {
                afterImageTrail = projectileObject.AddComponent<ProjectileAfterImageTrail>();
            }

            SpriteRenderer sourceRenderer = projectileObject.GetComponentInChildren<SpriteRenderer>(true);
            afterImageTrail.Configure(
                sourceRenderer,
                archerSkill2.enableAfterImage,
                archerSkill2.afterImageInterval,
                archerSkill2.afterImageFadeDuration,
                archerSkill2.afterImageStartAlpha,
                archerSkill2.afterImageMaxVisibleCount);
        }

        private Quaternion GetArcherSkill2ProjectileRotation(Vector3 direction, bool usesPrefabVisual)
        {
            Vector3 safeDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            Vector3 sourceAxis = usesPrefabVisual
                ? GetDirectionReferenceAxis(archerSkill2.prefabDirectionAxis)
                : Vector3.up;

            Quaternion rotation = Quaternion.FromToRotation(sourceAxis, safeDirection);
            if (!usesPrefabVisual)
            {
                return rotation;
            }

            Vector3 offsetEuler = archerSkill2.prefabRotationOffsetEuler;
            if (offsetEuler.sqrMagnitude > 0.0001f)
            {
                rotation *= Quaternion.Euler(offsetEuler);
            }

            if (archerSkill2.autoRollToCamera)
            {
                rotation = AlignProjectileRollToCamera(rotation, safeDirection, archerSkill2.prefabSurfaceNormalAxis);
            }

            return rotation;
        }

        private static Vector3 GetDirectionReferenceAxis(ProjectileDirectionReferenceAxis axis)
        {
            switch (axis)
            {
                case ProjectileDirectionReferenceAxis.Up:
                    return Vector3.up;
                case ProjectileDirectionReferenceAxis.Forward:
                    return Vector3.forward;
                default:
                    return Vector3.right;
            }
        }

        private Quaternion AlignProjectileRollToCamera(
            Quaternion baseRotation,
            Vector3 moveDirection,
            ProjectileDirectionReferenceAxis surfaceNormalAxis)
        {
            Camera activeCamera = DontStarveCamera.GetActiveCamera();
            if (activeCamera == null)
            {
                return baseRotation;
            }

            Vector3 targetNormal = Vector3.ProjectOnPlane(-activeCamera.transform.forward, moveDirection);
            if (targetNormal.sqrMagnitude <= 0.0001f)
            {
                targetNormal = Vector3.ProjectOnPlane(activeCamera.transform.up, moveDirection);
                if (targetNormal.sqrMagnitude <= 0.0001f)
                {
                    return baseRotation;
                }
            }

            targetNormal.Normalize();

            Vector3 currentNormal = baseRotation * GetDirectionReferenceAxis(surfaceNormalAxis);
            currentNormal = Vector3.ProjectOnPlane(currentNormal, moveDirection);
            if (currentNormal.sqrMagnitude <= 0.0001f)
            {
                return baseRotation;
            }

            currentNormal.Normalize();
            float rollAngle = Vector3.SignedAngle(currentNormal, targetNormal, moveDirection);
            return Quaternion.AngleAxis(rollAngle, moveDirection) * baseRotation;
        }

        private void HandleArcherSkill2EnemyHit(EnemyController enemy, Vector3 hitPosition)
        {
            if (enemy == null)
            {
                return;
            }

            EnemyStatusEffectController status = EnsureStatusController(enemy);
            if (status == null || !status.IsPoisoned)
            {
                return;
            }

            TriggerArcherSkill2VirusExplosion(enemy, hitPosition);
        }

        private void TriggerArcherSkill2VirusExplosion(EnemyController target, Vector3 fallbackCenter)
        {
            Vector3 center = target != null ? GetTargetEffectPosition(target) : fallbackCenter;
            float effectScaleMultiplier = target != null ? GetTargetEffectScaleMultiplier(target) : 1f;
            float damage = PlayerCombatCalculator.GetSkillDamage(archerSkill2.virusExplosionDamage, CurrentPlayerStats);

            int explosionHit = ApplyAreaSkill(
                center,
                archerSkill2.virusExplosionRadius,
                enemy => enemy.TakeDamage(damage));

            SpawnSkillEffect(
                archerSkill2.virusExplosionEffectPrefab,
                center,
                archerSkill2.virusExplosionEffectLifetime,
                Mathf.Max(0.25f, archerSkill2.virusExplosionRadius * 0.9f),
                new Color(1f, 0.45f, 0.2f, 0.45f),
                effectScaleMultiplier);

            if (enableDebugLogs)
            {
                Debug.Log($"[Archer Skill R] Virus explosion triggered. Hit={explosionHit}, Damage={damage:0.##}");
            }
        }

        private bool TryFindForwardEnemyPoint(float maxDistance, float maxAngle, out Vector3 point)
        {
            point = Vector3.zero;
            EnsureOverlapBuffer();

            Vector3 forward = GetFacingDirection();
            Vector3 origin = transform.position;
            origin.y += skillHitHeightOffset;

            int count = Physics.OverlapSphereNonAlloc(
                origin,
                Mathf.Max(0.1f, maxDistance),
                overlapBuffer,
                enemyMask,
                QueryTriggerInteraction.Collide);

            EnemyController bestEnemy = null;
            float bestScore = float.NegativeInfinity;
            float halfAngle = Mathf.Max(1f, maxAngle) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                EnemyController enemy = collider.GetComponentInParent<EnemyController>();
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - transform.position;
                toEnemy.y = 0f;
                float distance = toEnemy.magnitude;
                if (distance <= 0.01f || distance > maxDistance)
                {
                    continue;
                }

                Vector3 toEnemyDir = toEnemy / distance;
                float angle = Vector3.Angle(forward, toEnemyDir);
                if (angle > halfAngle)
                {
                    continue;
                }

                float forwardScore = Vector3.Dot(forward, toEnemyDir) * 2f;
                float distanceScore = -Mathf.Abs(distance - archerSkill2.targetForwardOffset) * 0.15f;
                float score = forwardScore + distanceScore;
                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestEnemy = enemy;
            }

            if (bestEnemy == null)
            {
                return false;
            }

            point = bestEnemy.transform.position;
            point.y = transform.position.y + skillVerticalOffset;
            return true;
        }

        private int ApplyAreaSkill(Vector3 center, float radius, System.Action<EnemyController> apply)
        {
            EnsureOverlapBuffer();
            uniqueEnemies.Clear();
            float safeRadius = Mathf.Max(0f, radius);

            Vector3 hitCenter = center;
            hitCenter.y += skillHitHeightOffset;
            float halfHeight = Mathf.Max(0.05f, skillHitVerticalHalfHeight);
            Vector3 capsuleTop = hitCenter + Vector3.up * halfHeight;
            Vector3 capsuleBottom = hitCenter - Vector3.up * halfHeight;

            int count = Physics.OverlapCapsuleNonAlloc(
                capsuleTop,
                capsuleBottom,
                safeRadius,
                overlapBuffer,
                enemyMask,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                if (!TryGetEnemyFromCollider(collider, out EnemyController enemy))
                {
                    continue;
                }

                if (!uniqueEnemies.Add(enemy))
                {
                    continue;
                }

                apply?.Invoke(enemy);
            }

            // Fallback: if colliders are missing or overlap filtering misses enemies,
            // run a lightweight distance check against active EnemyController instances.
            if (uniqueEnemies.Count == 0)
            {
                int fallbackHits = ApplyAreaSkillDistanceFallback(center, safeRadius, apply);
                if (enableDebugLogs && fallbackHits > 0)
                {
                    Debug.Log($"[SkillHit] Overlap fallback applied {fallbackHits} enemies.");
                }
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[SkillHit] Center={center}, HitCenter={hitCenter}, Radius={safeRadius:0.##}, OverlapCount={count}, UniqueHit={uniqueEnemies.Count}");
            }

            return uniqueEnemies.Count;
        }

        // 전방 각도(forwardAngle) 안에 있는 가장 가까운 적을 찾음
        private bool TryFindNearestEnemyInForwardArc(Vector3 center, float radius, float forwardAngle, out EnemyController nearestEnemy)
        {
            nearestEnemy = null;
            Vector3 forward = GetFacingDirection();
            float halfAngle = Mathf.Max(1f, forwardAngle) * 0.5f;
            float safeRadius = Mathf.Max(0f, radius);
            float bestDistanceSqr = float.PositiveInfinity;

            EnsureOverlapBuffer();
            Vector3 hitCenter = center;
            hitCenter.y += skillHitHeightOffset;
            float halfHeight = Mathf.Max(0.05f, skillHitVerticalHalfHeight);
            Vector3 capsuleTop = hitCenter + Vector3.up * halfHeight;
            Vector3 capsuleBottom = hitCenter - Vector3.up * halfHeight;

            int count = Physics.OverlapCapsuleNonAlloc(
                capsuleTop, capsuleBottom, safeRadius,
                overlapBuffer, enemyMask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null || !TryGetEnemyFromCollider(collider, out EnemyController enemy)) continue;

                Vector3 toEnemy = enemy.transform.position - transform.position;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude > 0.0001f && Vector3.Angle(forward, toEnemy.normalized) > halfAngle) continue;

                float distanceSqr = toEnemy.sqrMagnitude;
                if (distanceSqr > bestDistanceSqr) continue;

                bestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }

            if (nearestEnemy != null) return true;

            // 콜라이더 탐지 실패 시 ActiveEnemyControllers 직접 순회
            IReadOnlyList<EnemyController> allEnemies = EnemyController.ActiveEnemyControllers;
            for (int i = 0; i < allEnemies.Count; i++)
            {
                EnemyController enemy = allEnemies[i];
                if (enemy == null || enemy.IsDead) continue;

                Vector3 toEnemy = enemy.transform.position - transform.position;
                toEnemy.y = 0f;
                float distanceSqr = toEnemy.sqrMagnitude;
                if (distanceSqr > safeRadius * safeRadius || distanceSqr > bestDistanceSqr) continue;
                if (toEnemy.sqrMagnitude > 0.0001f && Vector3.Angle(forward, toEnemy.normalized) > halfAngle) continue;

                bestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }

            return nearestEnemy != null;
        }

        private bool TryFindNearestEnemyInRadius(Vector3 center, float radius, out EnemyController nearestEnemy)
        {
            nearestEnemy = null;
            EnsureOverlapBuffer();

            float safeRadius = Mathf.Max(0f, radius);
            Vector3 hitCenter = center;
            hitCenter.y += skillHitHeightOffset;
            float halfHeight = Mathf.Max(0.05f, skillHitVerticalHalfHeight);
            Vector3 capsuleTop = hitCenter + Vector3.up * halfHeight;
            Vector3 capsuleBottom = hitCenter - Vector3.up * halfHeight;

            int count = Physics.OverlapCapsuleNonAlloc(
                capsuleTop,
                capsuleBottom,
                safeRadius,
                overlapBuffer,
                enemyMask,
                QueryTriggerInteraction.Collide);

            float bestDistanceSqr = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null || !TryGetEnemyFromCollider(collider, out EnemyController enemy))
                {
                    continue;
                }

                Vector3 enemyPos = enemy.transform.position;
                enemyPos.y = center.y;
                float distanceSqr = (enemyPos - center).sqrMagnitude;
                if (distanceSqr > bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }

            if (nearestEnemy != null)
            {
                return true;
            }

            IReadOnlyList<EnemyController> enemies = EnemyController.ActiveEnemyControllers;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 enemyPos = enemy.transform.position;
                enemyPos.y = center.y;
                float distanceSqr = (enemyPos - center).sqrMagnitude;
                if (distanceSqr > safeRadius * safeRadius || distanceSqr > bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }

            return nearestEnemy != null;
        }

        private Vector3 GetTargetEffectPosition(EnemyController target)
        {
            if (target == null)
            {
                return GetSkillCenter(0f);
            }

            if (TargetAttachedEffect.TryGetTargetBounds(target.transform, out Bounds targetBounds))
            {
                return targetBounds.center;
            }

            return target.transform.position;
        }

        private float GetTargetEffectScaleMultiplier(EnemyController target)
        {
            if (!mageSkill2.scaleEffectByTargetSize || target == null)
            {
                return 1f;
            }

            float minMultiplier = Mathf.Max(0.05f, mageSkill2.effectMinScaleMultiplier);
            float maxMultiplier = Mathf.Max(minMultiplier, mageSkill2.effectMaxScaleMultiplier);
            float referenceHeight = Mathf.Max(0.01f, mageSkill2.effectReferenceTargetHeight);
            float sizeMultiplier = Mathf.Max(0.01f, mageSkill2.effectSizeMultiplier);

            if (!TargetAttachedEffect.TryGetTargetBounds(target.transform, out Bounds targetBounds))
            {
                return Mathf.Clamp(sizeMultiplier, minMultiplier, maxMultiplier);
            }

            float targetHeight = Mathf.Max(0.01f, targetBounds.size.y);
            float scaleMultiplier = (targetHeight / referenceHeight) * sizeMultiplier;
            return Mathf.Clamp(scaleMultiplier, minMultiplier, maxMultiplier);
        }

        private int ApplyAreaSkillDistanceFallback(Vector3 center, float radius, System.Action<EnemyController> apply)
        {
            IReadOnlyList<EnemyController> enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return 0;
            }

            int hitCount = 0;
            float radiusSqr = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 enemyPos = enemy.transform.position;
                enemyPos.y = center.y;
                if ((enemyPos - center).sqrMagnitude > radiusSqr)
                {
                    continue;
                }

                if (!uniqueEnemies.Add(enemy))
                {
                    continue;
                }

                apply?.Invoke(enemy);
                hitCount++;
            }

            return hitCount;
        }

        private static bool TryGetEnemyFromCollider(Collider collider, out EnemyController enemy)
        {
            enemy = null;
            if (collider == null)
            {
                return false;
            }

            enemy = collider.GetComponent<EnemyController>();
            if (enemy == null)
            {
                enemy = collider.GetComponentInParent<EnemyController>();
            }

            if (enemy == null)
            {
                enemy = collider.GetComponentInChildren<EnemyController>();
            }

            return enemy != null && !enemy.IsDead;
        }

        private static EnemyStatusEffectController EnsureStatusController(EnemyController enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            EnemyStatusEffectController status = enemy.StatusEffects;
            if (status == null)
            {
                status = enemy.GetComponent<EnemyStatusEffectController>();
            }

            if (status == null)
            {
                status = enemy.gameObject.AddComponent<EnemyStatusEffectController>();
            }

            status.Initialize(enemy);
            return status;
        }

        private void SpawnSkillProjectile(
            GameObject prefab,
            float fallbackScale,
            float damage,
            float speed,
            float lifeTime,
            Vector3 direction,
            SkillProjectileDebuff debuff,
            bool shouldDisableOnHit = true)
        {
            SpawnSkillProjectile(prefab, fallbackScale, damage, speed, lifeTime, direction, debuff, null, shouldDisableOnHit);
        }

        private void SpawnSkillProjectile(
            GameObject prefab,
            float fallbackScale,
            float damage,
            float speed,
            float lifeTime,
            Vector3 direction,
            SkillProjectileDebuff debuff,
            Vector3? worldSpawnPosition,
            bool shouldDisableOnHit = true)
        {
            Vector3 spawnPosition = worldSpawnPosition ?? GetProjectileSpawnPosition(direction);
            GameObject projectileObject = CreateProjectileObject(prefab, spawnPosition, fallbackScale);
            if (projectileObject == null)
            {
                return;
            }

            SkillProjectile projectile = projectileObject.GetComponent<SkillProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<SkillProjectile>();
            }

            projectile.Launch(direction, damage, speed, lifeTime, enemyMask, shouldDisableOnHit, debuff);
        }

        private GameObject CreateProjectileObject(GameObject prefab, Vector3 position, float fallbackScale)
        {
            GameObject projectileObject = prefab != null
                ? RuntimePool.Acquire(prefab)
                : AcquireFallbackVisual(
                    SkillProjectileFallbackPoolName,
                    PrimitiveType.Sphere,
                    fallbackScale,
                    new Color(0.75f, 1f, 0.65f, 0.9f));

            if (projectileObject == null)
            {
                return null;
            }

            EnsureProjectilePhysics(projectileObject);
            projectileObject.transform.position = position;
            projectileObject.transform.rotation = Quaternion.identity;
            return projectileObject;
        }

        private void EnsureProjectilePhysics(GameObject projectileObject)
        {
            Collider collider = projectileObject.GetComponent<Collider>();
            if (collider == null)
            {
                collider = projectileObject.AddComponent<SphereCollider>();
            }

            collider.enabled = true;
            collider.isTrigger = true;

            Rigidbody body = projectileObject.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = projectileObject.AddComponent<Rigidbody>();
            }

            body.useGravity = false;
            body.isKinematic = true;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        private Vector3 GetProjectileSpawnPosition(Vector3 direction)
        {
            Vector3 basePos = skillSpawnPoint != null ? skillSpawnPoint.position : transform.position;
            basePos += direction.normalized * projectileSpawnOffset;
            basePos.y += projectileSpawnHeight + skillVerticalOffset;
            return basePos;
        }

        private Vector3 GetSkillCenter(float forwardOffset)
        {
            Vector3 center = transform.position + GetFacingDirection() * forwardOffset;
            center.y = transform.position.y + skillVerticalOffset;
            return center;
        }

        private Vector3 GetFacingDirection()
        {
            if (playerController == null)
            {
                return Vector3.forward;
            }

            Vector3 direction = playerController.GetLogicalFacingDirection();
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        private void ResolveSkillSpawnPoint()
        {
            if (IsValidRootSpawnPoint(skillSpawnPoint))
            {
                return;
            }

            Transform found = transform.Find("SkillSpawnPoint");
            if (!IsValidRootSpawnPoint(found))
            {
                found = transform.Find("FirePoint");
            }

            if (IsValidRootSpawnPoint(found))
            {
                skillSpawnPoint = found;
                return;
            }

            GameObject pointObject = new GameObject("SkillSpawnPoint");
            skillSpawnPoint = pointObject.transform;
            skillSpawnPoint.SetParent(transform, false);
            skillSpawnPoint.localPosition = Vector3.zero;
            skillSpawnPoint.localRotation = Quaternion.identity;
        }

        private bool IsValidRootSpawnPoint(Transform point)
        {
            if (point == null || !point.IsChildOf(transform))
            {
                return false;
            }

            Transform cursor = point;
            while (cursor != null && cursor != transform)
            {
                if (cursor.GetComponent<SpriteRenderer>() != null)
                {
                    return false;
                }

                cursor = cursor.parent;
            }

            return true;
        }

        private void SpawnAttachedSkillEffect(
            GameObject prefab,
            EnemyController target,
            float lifeTime,
            float fallbackScale,
            Color fallbackColor,
            float headOffset)
        {
            if (target == null)
            {
                SpawnSkillEffect(prefab, GetSkillCenter(0f), lifeTime, fallbackScale, fallbackColor);
                return;
            }

            GameObject effect = prefab != null
                ? RuntimePool.Acquire(prefab)
                : AcquireFallbackVisual(
                    SkillAttachedEffectFallbackPoolName,
                    PrimitiveType.Sphere,
                    fallbackScale,
                    fallbackColor);
            if (effect == null)
            {
                return;
            }

            effect.transform.position = target.transform.position;
            effect.transform.rotation = Quaternion.identity;

            TargetAttachedEffect attachedEffect = effect.GetComponent<TargetAttachedEffect>();
            if (attachedEffect == null)
            {
                attachedEffect = effect.AddComponent<TargetAttachedEffect>();
            }

            attachedEffect.Bind(
                target.transform,
                headOffset,
                mageSkill2.scaleEffectByTargetSize,
                mageSkill2.effectReferenceTargetHeight,
                mageSkill2.effectSizeMultiplier,
                mageSkill2.effectMinScaleMultiplier,
                mageSkill2.effectMaxScaleMultiplier,
                true);

            SchedulePoolReturn(effect, lifeTime);
        }

        private void SpawnSkillEffect(
            GameObject prefab,
            Vector3 position,
            float lifeTime,
            float fallbackScale,
            Color fallbackColor,
            float scaleMultiplier = 1f)
        {
            float safeScaleMultiplier = Mathf.Max(0.01f, scaleMultiplier);
            if (prefab != null)
            {
                GameObject effect = RuntimePool.Acquire(prefab);
                if (effect == null)
                {
                    return;
                }

                effect.transform.position = position;
                effect.transform.rotation = Quaternion.identity;
                effect.transform.localScale *= safeScaleMultiplier;
                SchedulePoolReturn(effect, lifeTime);
                return;
            }

            GameObject fallback = AcquireFallbackVisual(
                SkillEffectFallbackPoolName,
                PrimitiveType.Sphere,
                fallbackScale * safeScaleMultiplier,
                fallbackColor);
            if (fallback == null)
            {
                return;
            }

            fallback.transform.position = position;
            fallback.transform.rotation = Quaternion.identity;
            SchedulePoolReturn(fallback, lifeTime);
        }

        private static GameObject AcquireFallbackVisual(string poolName, PrimitiveType primitiveType, float scale, Color color)
        {
            GameObject visual = RuntimePool.Acquire(poolName, () => CreatePrimitiveVisualObject(primitiveType, color));
            if (visual == null)
            {
                return null;
            }

            ConfigurePrimitiveVisualObject(visual, scale, color);
            return visual;
        }

        private static GameObject CreatePrimitiveVisualObject(PrimitiveType primitiveType, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            Collider collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                UnityEngine.Object.Destroy(collider);
            }

            Renderer renderer = primitive.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = GetFallbackVisualShader();
                if (shader != null)
                {
                    Material material = new Material(shader)
                    {
                        color = color
                    };
                    renderer.material = material;
                }
            }

            return primitive;
        }

        private static void ConfigurePrimitiveVisualObject(GameObject visual, float scale, Color color)
        {
            if (visual == null)
            {
                return;
            }

            visual.transform.localScale = Vector3.one * Mathf.Max(0.05f, scale);

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = renderer.material;
                if (material != null)
                {
                    material.color = color;
                }
            }
        }

        private static Shader GetFallbackVisualShader()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            return shader;
        }

        private static void SchedulePoolReturn(GameObject obj, float lifeTime)
        {
            RuntimePoolAutoReturn autoReturn = RuntimePool.EnsureAutoReturn(obj);
            autoReturn?.Schedule(Mathf.Max(0.1f, lifeTime));
        }

        private void EnsureOverlapBuffer()
        {
            int desiredSize = Mathf.Max(1, overlapBufferSize);
            if (overlapBuffer != null && overlapBuffer.Length == desiredSize)
            {
                return;
            }

            overlapBuffer = new Collider[desiredSize];
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.identity;

            switch (currentClass)
            {
                case PlayerClassType.Mage:
                    DrawSkillRadius(mageSkill1.forwardOffset, mageSkill1.radius, new Color(0.3f, 0.9f, 1f, 0.4f));
                    DrawSkillRadius(0f, mageSkill2.radius, new Color(1f, 0.5f, 0.2f, 0.4f));
                    break;

                case PlayerClassType.Archer:
                    DrawSkillRadius(archerSkill2.targetForwardOffset, archerSkill2.projectileHitRadius, new Color(0.4f, 1f, 0.4f, 0.6f));
                    DrawArcherSkill2LineGizmo(new Color(1f, 0.45f, 0.25f, 0.65f));
                    break;

                case PlayerClassType.Warrior:
                    DrawSkillRadius(0f, warriorSkill1.range, new Color(0.9f, 0.1f, 0.1f, 0.4f));
                    DrawSkillRadius(0f, warriorSkill2.searchRange, new Color(1f, 0.4f, 0.0f, 0.3f));
                    break;
            }
        }

        private void DrawSkillRadius(float forwardOffset, float radius, Color color)
        {
            Vector3 direction = Application.isPlaying ? GetFacingDirection() : Vector3.forward;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.forward;
            }

            Vector3 center = transform.position + direction.normalized * forwardOffset;
            center.y = transform.position.y + skillVerticalOffset;

            Gizmos.color = color;
            Gizmos.DrawWireSphere(center, Mathf.Max(0f, radius));
        }

        private void DrawArcherSkill2LineGizmo(Color color)
        {
            Vector3 direction = Application.isPlaying ? GetArcherSkill2Direction() : Vector3.forward;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.forward;
            }

            Vector3 start = transform.position + direction.normalized * projectileSpawnOffset;
            start.y = transform.position.y + projectileSpawnHeight + skillVerticalOffset;
            Vector3 end = start + direction.normalized * Mathf.Max(0.5f, archerSkill2.range);

            Gizmos.color = color;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, Mathf.Max(0.05f, archerSkill2.projectileHitRadius));
        }
    }
}
