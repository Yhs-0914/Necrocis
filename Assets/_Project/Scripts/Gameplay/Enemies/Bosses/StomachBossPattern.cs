using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [System.Serializable]
    public class StomachBossPatternSettings
    {
        [Header("Phase")]
        [Range(0.1f, 0.9f)] public float phase2HealthRatio = 0.65f;

        [Header("Stats")]
        public float phase1AttackDamage = 3f;
        public float phase1MoveSpeed = 0.5f;
        public float phase2AttackDamage = 3f;
        public float phase2MoveSpeed = 1f;

        [Header("Phase 1 Basic")]
        public float phase1MeleeRange = 1.6f;
        public float phase1MeleeCooldown = 1.7f;

        [Header("Charge")]
        public float chargeSpeed = 4f;
        public float chargeCooldown = 5f;
        public float chargeDamage = 4f;
        public float chargeKnockbackDistance = 2f;
        public float chargeWindup = 0.35f;
        public float chargeDuration = 1.2f;
        public float chargeHitRadius = 1.25f;

        [Header("Phase 2 Pattern")]
        public float phase2PatternInterval = 3f;
        public float phase2PreferredDistance = 4f;
        public float roamRadius = 9f;

        [Header("Acid Spray")]
        public float acidRange = 9f;
        public float acidDamage = 2f;
        public float acidDebuffDuration = 5f;
        public float acidTickInterval = 1f;
        public float acidTickDamage = 1f;
        public float acidProjectileTravelTime = 0.55f;
        public float acidArcHeight = 0.8f;
        public float acidProjectileScale = 0.9f;
        public float acidHitRadius = 1.15f;

        [Header("Suck And Spit")]
        public float suctionRange = 4.2f;
        public float suctionDuration = 0.4f;
        public float suctionStopDistance = 1f;
        public float spitDelay = 0.35f;
        public float spitReturnDuration = 0.18f;
        public float shortRangeDamage = 5f;
        public float shortRangeRadius = 2.2f;

        [Header("Temporary Visuals")]
        public Color phase1Color = new Color(0.62f, 0.55f, 0.34f, 1f);
        public Color phase2Color = new Color(0.42f, 0.78f, 0.2f, 1f);
        public Color chargeColor = new Color(0.95f, 0.78f, 0.22f, 1f);
        public Color acidColor = new Color(0.55f, 0.95f, 0.15f, 0.95f);
        public Color acidSplashColor = new Color(0.5f, 0.9f, 0.12f, 0.42f);
        public Color suctionColor = new Color(0.75f, 0.95f, 0.45f, 0.42f);
        public Color leakColor = new Color(0.38f, 0.85f, 0.18f, 0.38f);

        [Header("Debug")]
        public bool startInPhase2ForDebug = false;
        public bool useFastPatternCooldownsForDebug = false;
        public float fastPatternCooldown = 1f;
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public class StomachBossPattern : MonoBehaviour
    {
        private enum BossPhase
        {
            Phase1,
            Transition,
            Phase2
        }

        private enum Phase2PatternRequest
        {
            Auto,
            AcidSpray,
            SuckAndSpit
        }

        private const float ArenaMovementClampExtraMargin = 0.2f;

        [Header("Phase")]
        [SerializeField, Range(0.1f, 0.9f)] private float phase2HealthRatio = 0.65f;

        [Header("Stats")]
        [SerializeField] private float phase1AttackDamage = 3f;
        [SerializeField] private float phase1MoveSpeed = 0.5f;
        [SerializeField] private float phase2AttackDamage = 3f;
        [SerializeField] private float phase2MoveSpeed = 1f;

        [Header("Phase 1 Basic")]
        [SerializeField] private float phase1MeleeRange = 1.6f;
        [SerializeField] private float phase1MeleeCooldown = 1.7f;

        [Header("Charge")]
        [SerializeField] private float chargeSpeed = 4f;
        [SerializeField] private float chargeCooldown = 5f;
        [SerializeField] private float chargeDamage = 4f;
        [SerializeField] private float chargeKnockbackDistance = 2f;
        [SerializeField] private float chargeWindup = 0.35f;
        [SerializeField] private float chargeDuration = 1.2f;
        [SerializeField] private float chargeHitRadius = 1.25f;

        [Header("Phase 2 Pattern")]
        [SerializeField] private float phase2PatternInterval = 3f;
        [SerializeField] private float phase2PreferredDistance = 4f;
        [SerializeField] private float roamRadius = 9f;

        [Header("Acid Spray")]
        [SerializeField] private float acidRange = 9f;
        [SerializeField] private float acidDamage = 2f;
        [SerializeField] private float acidDebuffDuration = 5f;
        [SerializeField] private float acidTickInterval = 1f;
        [SerializeField] private float acidTickDamage = 1f;
        [SerializeField] private float acidProjectileTravelTime = 0.55f;
        [SerializeField] private float acidArcHeight = 0.8f;
        [SerializeField] private float acidProjectileScale = 0.9f;
        [SerializeField] private float acidHitRadius = 1.15f;

        [Header("Suck And Spit")]
        [SerializeField] private float suctionRange = 4.2f;
        [SerializeField] private float suctionDuration = 0.4f;
        [SerializeField] private float suctionStopDistance = 1f;
        [SerializeField] private float spitDelay = 0.35f;
        [SerializeField] private float spitReturnDuration = 0.18f;
        [SerializeField] private float shortRangeDamage = 5f;
        [SerializeField] private float shortRangeRadius = 2.2f;

        [Header("Temporary Visuals")]
        [SerializeField] private Color phase1Color = new Color(0.62f, 0.55f, 0.34f, 1f);
        [SerializeField] private Color phase2Color = new Color(0.42f, 0.78f, 0.2f, 1f);
        [SerializeField] private Color chargeColor = new Color(0.95f, 0.78f, 0.22f, 1f);
        [SerializeField] private Color acidColor = new Color(0.55f, 0.95f, 0.15f, 0.95f);
        [SerializeField] private Color acidSplashColor = new Color(0.5f, 0.9f, 0.12f, 0.42f);
        [SerializeField] private Color suctionColor = new Color(0.75f, 0.95f, 0.45f, 0.42f);
        [SerializeField] private Color leakColor = new Color(0.38f, 0.85f, 0.18f, 0.38f);

        [Header("Debug")]
        [SerializeField] private bool startInPhase2ForDebug = false;
        [SerializeField] private bool useFastPatternCooldownsForDebug = false;
        [SerializeField] private float fastPatternCooldown = 1f;

        private static Sprite circleSprite;
        private static Sprite ringSprite;

        private EnemyController boss;
        private CharacterStats stats;
        private SpriteRenderer visualRenderer;
        private Vector3 anchorPosition;
        private Vector3 baseScale = Vector3.one;
        private BossPhase phase;
        private float nextChargeTime;
        private float nextMeleeTime;
        private float nextPhase2AttackTime;
        private bool actionRunning;
        private bool encounterActive;
        private readonly List<GameObject> activeTempObjects = new List<GameObject>();

        public string CurrentPhaseName => phase.ToString();

        public void Initialize(EnemyController controller, Vector3 anchor, Transform parent, StomachBossPatternSettings settings = null)
        {
            ApplySettings(settings);

            boss = controller != null ? controller : GetComponent<EnemyController>();
            stats = boss != null ? boss.Stats : GetComponent<CharacterStats>();
            anchorPosition = anchor;
            phase = startInPhase2ForDebug ? BossPhase.Phase2 : BossPhase.Phase1;
            nextChargeTime = Time.time + 1.2f;
            nextMeleeTime = Time.time + 1f;
            nextPhase2AttackTime = phase == BossPhase.Phase2 ? Time.time + 1f : float.PositiveInfinity;
            actionRunning = false;
            encounterActive = false;
            baseScale = transform.localScale;
            visualRenderer = GetComponentInChildren<SpriteRenderer>();

            if (boss != null)
            {
                boss.SetAiSuppressed(true);
                boss.Defeated -= HandleBossDefeated;
                boss.Defeated += HandleBossDefeated;
            }

            ApplyPhaseStats();
            ApplyPhaseVisual();
            enabled = true;
        }

        private void ApplySettings(StomachBossPatternSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            phase2HealthRatio = settings.phase2HealthRatio;
            phase1AttackDamage = settings.phase1AttackDamage;
            phase1MoveSpeed = settings.phase1MoveSpeed;
            phase2AttackDamage = settings.phase2AttackDamage;
            phase2MoveSpeed = settings.phase2MoveSpeed;
            phase1MeleeRange = settings.phase1MeleeRange;
            phase1MeleeCooldown = settings.phase1MeleeCooldown;
            chargeSpeed = settings.chargeSpeed;
            chargeCooldown = settings.chargeCooldown;
            chargeDamage = settings.chargeDamage;
            chargeKnockbackDistance = settings.chargeKnockbackDistance;
            chargeWindup = settings.chargeWindup;
            chargeDuration = settings.chargeDuration;
            chargeHitRadius = settings.chargeHitRadius;
            phase2PatternInterval = settings.phase2PatternInterval;
            phase2PreferredDistance = settings.phase2PreferredDistance;
            roamRadius = settings.roamRadius;
            acidRange = settings.acidRange;
            acidDamage = settings.acidDamage;
            acidDebuffDuration = settings.acidDebuffDuration;
            acidTickInterval = settings.acidTickInterval;
            acidTickDamage = settings.acidTickDamage;
            acidProjectileTravelTime = settings.acidProjectileTravelTime;
            acidArcHeight = settings.acidArcHeight;
            acidProjectileScale = settings.acidProjectileScale;
            acidHitRadius = settings.acidHitRadius;
            suctionRange = settings.suctionRange;
            suctionDuration = settings.suctionDuration;
            suctionStopDistance = settings.suctionStopDistance;
            spitDelay = settings.spitDelay;
            spitReturnDuration = settings.spitReturnDuration;
            shortRangeDamage = settings.shortRangeDamage;
            shortRangeRadius = settings.shortRangeRadius;
            phase1Color = settings.phase1Color;
            phase2Color = settings.phase2Color;
            chargeColor = settings.chargeColor;
            acidColor = settings.acidColor;
            acidSplashColor = settings.acidSplashColor;
            suctionColor = settings.suctionColor;
            leakColor = settings.leakColor;
            startInPhase2ForDebug = settings.startInPhase2ForDebug;
            useFastPatternCooldownsForDebug = settings.useFastPatternCooldownsForDebug;
            fastPatternCooldown = settings.fastPatternCooldown;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            actionRunning = false;
            encounterActive = false;
            transform.localScale = baseScale;
            CleanupPatternObjects();

            if (boss != null)
            {
                boss.Defeated -= HandleBossDefeated;
                boss.SetAiSuppressed(false);
            }
        }

        private void HandleBossDefeated(EnemyController defeatedBoss)
        {
            if (defeatedBoss != boss)
            {
                return;
            }

            StopAllCoroutines();
            actionRunning = false;
            transform.localScale = baseScale;
            CleanupPatternObjects();
        }

        public void SetEncounterActive(bool active)
        {
            if (encounterActive == active)
            {
                return;
            }

            encounterActive = active;
            StopAllCoroutines();
            actionRunning = false;

            if (active)
            {
                nextChargeTime = Time.time + 1.2f;
                nextMeleeTime = Time.time + 1f;
                nextPhase2AttackTime = phase == BossPhase.Phase2
                    ? Time.time + 1f
                    : float.PositiveInfinity;
            }
        }

        [ContextMenu("Debug/Force Phase 1")]
        public void ForcePhase1ForDebug()
        {
            StopAllCoroutines();
            actionRunning = false;
            phase = BossPhase.Phase1;
            nextChargeTime = Time.time;
            nextMeleeTime = Time.time;
            nextPhase2AttackTime = float.PositiveInfinity;
            transform.localScale = baseScale;
            ApplyPhaseStats();
            ApplyPhaseVisual();
        }

        [ContextMenu("Debug/Force Phase 2")]
        public void ForcePhase2ForDebug()
        {
            StopAllCoroutines();
            actionRunning = false;
            phase = BossPhase.Phase2;
            nextChargeTime = float.PositiveInfinity;
            nextMeleeTime = float.PositiveInfinity;
            nextPhase2AttackTime = Time.time;
            transform.localScale = baseScale;
            ApplyPhaseStats();
            ApplyPhaseVisual();
        }

        [ContextMenu("Debug/Run Charge")]
        public void RunChargeForDebug()
        {
            ForcePhase1ForDebug();
            StartCoroutine(ChargeRoutine());
        }

        [ContextMenu("Debug/Run Acid Spray")]
        public void RunAcidSprayForDebug()
        {
            ForcePhase2ForDebug();
            StartCoroutine(Phase2AttackRoutine(Phase2PatternRequest.AcidSpray));
        }

        [ContextMenu("Debug/Run Suck And Spit")]
        public void RunSuckAndSpitForDebug()
        {
            ForcePhase2ForDebug();
            StartCoroutine(Phase2AttackRoutine(Phase2PatternRequest.SuckAndSpit));
        }

        private void Update()
        {
            if (!encounterActive)
            {
                return;
            }

            if (boss == null || boss.IsDead)
            {
                enabled = false;
                return;
            }

            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                return;
            }

            if (phase == BossPhase.Phase1 && stats != null && stats.HealthNormalized <= phase2HealthRatio)
            {
                StartCoroutine(EnterPhase2Routine());
                return;
            }

            if (actionRunning)
            {
                return;
            }

            if (phase == BossPhase.Phase1)
            {
                if (Time.time >= nextChargeTime)
                {
                    StartCoroutine(ChargeRoutine());
                }
                else if (Time.time >= nextMeleeTime && IsPlayerWithinRange(player, phase1MeleeRange))
                {
                    StartCoroutine(BasicMeleeRoutine());
                }
            }
            else if (phase == BossPhase.Phase2 && Time.time >= nextPhase2AttackTime)
            {
                StartCoroutine(Phase2AttackRoutine());
            }
        }

        private void LateUpdate()
        {
            if (!encounterActive || boss == null || boss.IsDead || actionRunning || phase == BossPhase.Transition || PlayerController.Instance == null)
            {
                return;
            }

            Vector3 moveDirection = GetMoveDirection();
            if (moveDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float speed = stats != null ? stats.MoveSpeed : phase == BossPhase.Phase2 ? phase2MoveSpeed : phase1MoveSpeed;
            boss.MoveByExternalPattern(moveDirection.normalized * Mathf.Max(0f, speed) * Time.deltaTime);
        }

        private IEnumerator EnterPhase2Routine()
        {
            phase = BossPhase.Transition;
            actionRunning = true;

            Vector3 leakPosition = transform.position;
            leakPosition.y = GetGroundHeight(leakPosition) + 0.08f;
            GameObject leak = CreateTempSpriteObject("StomachBoss_AcidLeak", GetRingSprite(), leakColor, leakPosition, 1.5f, 4900);
            StartCoroutine(FadeAndDestroy(leak, 1f, leakColor, 3.8f));

            float elapsed = 0f;
            const float duration = 1f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = baseScale * (1f + Mathf.Sin(t * Mathf.PI * 6f) * 0.08f);

                if (visualRenderer != null)
                {
                    visualRenderer.color = Color.Lerp(phase1Color, phase2Color, t);
                }

                yield return null;
            }

            phase = BossPhase.Phase2;
            transform.localScale = baseScale;
            nextChargeTime = float.PositiveInfinity;
            nextMeleeTime = float.PositiveInfinity;
            nextPhase2AttackTime = Time.time + 1f;
            actionRunning = false;
            ApplyPhaseStats();
            ApplyPhaseVisual();
        }

        private IEnumerator BasicMeleeRoutine()
        {
            actionRunning = true;
            nextMeleeTime = Time.time + Mathf.Max(0.1f, phase1MeleeCooldown);

            float elapsed = 0f;
            const float duration = 0.28f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = new Vector3(baseScale.x * (1f + t * 0.08f), baseScale.y * (1f - t * 0.06f), baseScale.z);
                yield return null;
            }

            PlayerController player = PlayerController.Instance;
            if (IsPlayerWithinRange(player, phase1MeleeRange))
            {
                player.TakeDamage(phase1AttackDamage);
            }

            transform.localScale = baseScale;
            actionRunning = false;
        }

        private IEnumerator ChargeRoutine()
        {
            actionRunning = true;
            nextChargeTime = Time.time + GetChargeCooldown();

            Vector3 direction = GetDirectionToPlayer();
            float windup = Mathf.Max(0f, chargeWindup);
            float elapsed = 0f;
            while (elapsed < windup)
            {
                elapsed += Time.deltaTime;
                float pulse = 1f + Mathf.Sin(elapsed * 22f) * 0.08f;
                transform.localScale = new Vector3(baseScale.x * 1.12f, baseScale.y * 0.88f, baseScale.z) * pulse;

                if (visualRenderer != null)
                {
                    visualRenderer.color = Color.Lerp(phase1Color, chargeColor, 0.7f);
                }

                yield return null;
            }

            bool hitPlayer = false;
            elapsed = 0f;
            float duration = Mathf.Max(0.05f, chargeDuration);
            float speed = Mathf.Max(0f, chargeSpeed);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                boss.MoveByExternalPattern(direction * speed * Time.deltaTime);

                PlayerController player = PlayerController.Instance;
                if (!hitPlayer && IsPlayerWithinRange(player, chargeHitRadius))
                {
                    hitPlayer = true;
                    player.TakeDamage(chargeDamage);
                    ApplyPlayerKnockback(player, transform.position, chargeKnockbackDistance);
                }

                yield return null;
            }

            transform.localScale = baseScale;
            ApplyPhaseVisual();
            actionRunning = false;
        }

        private IEnumerator Phase2AttackRoutine(Phase2PatternRequest request = Phase2PatternRequest.Auto)
        {
            actionRunning = true;
            nextPhase2AttackTime = Time.time + GetPhase2PatternInterval();

            PlayerController player = PlayerController.Instance;
            bool canAcid = IsPlayerWithinRange(player, acidRange);
            bool canSuck = IsPlayerWithinRange(player, suctionRange);

            if (request == Phase2PatternRequest.AcidSpray)
            {
                yield return AcidSprayRoutine();
            }
            else if (request == Phase2PatternRequest.SuckAndSpit)
            {
                yield return SuckAndSpitRoutine();
            }
            else if (canSuck)
            {
                yield return SuckAndSpitRoutine();
            }
            else if (canAcid)
            {
                yield return AcidSprayRoutine();
            }

            transform.localScale = baseScale;
            ApplyPhaseVisual();
            actionRunning = false;
        }

        private IEnumerator AcidSprayRoutine()
        {
            float windup = 0.25f;
            float elapsed = 0f;
            while (elapsed < windup)
            {
                elapsed += Time.deltaTime;
                transform.localScale = new Vector3(baseScale.x * 1.08f, baseScale.y * 0.92f, baseScale.z);
                yield return null;
            }

            Vector3 start = transform.position + Vector3.up * 1.05f;
            Vector3 direction = GetDirectionToPlayer();
            Vector3 target = PlayerController.Instance != null
                ? PlayerController.Instance.transform.position
                : transform.position + direction * acidRange;
            Vector3 fromBoss = target - transform.position;
            fromBoss.y = 0f;
            float range = Mathf.Max(0.1f, acidRange);
            if (fromBoss.magnitude > range)
            {
                target = transform.position + fromBoss.normalized * range;
            }

            target.y = GetGroundHeight(target) + 0.05f;

            GameObject projectile = CreateTempSpriteObject(
                "StomachBoss_AcidProjectile",
                GetCircleSprite(),
                acidColor,
                start,
                acidProjectileScale,
                5000);

            elapsed = 0f;
            float travelTime = Mathf.Max(0.05f, acidProjectileTravelTime);
            while (elapsed < travelTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / travelTime);
                Vector3 position = Vector3.Lerp(start, target, t);
                position.y += Mathf.Sin(t * Mathf.PI) * acidArcHeight;
                if (projectile != null)
                {
                    projectile.transform.position = position;
                }

                yield return null;
            }

            if (projectile != null)
            {
                Destroy(projectile);
            }

            SplashAcid(target);
        }

        private IEnumerator SuckAndSpitRoutine()
        {
            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                yield break;
            }

            Vector3 startPosition = player.transform.position;
            Vector3 directionToPlayer = startPosition - transform.position;
            directionToPlayer.y = 0f;
            Vector3 pullDirection = directionToPlayer.sqrMagnitude > 0.0001f ? directionToPlayer.normalized : Vector3.forward;
            Vector3 pullTarget = transform.position + pullDirection * Mathf.Max(0.2f, suctionStopDistance);
            pullTarget.y = startPosition.y;

            GameObject suction = CreateTempSpriteObject(
                "StomachBoss_Suction",
                GetRingSprite(),
                suctionColor,
                transform.position,
                Mathf.Max(0.1f, suctionRange * 0.55f),
                4900);
            StartCoroutine(FadeAndDestroy(suction, Mathf.Max(0.1f, suctionDuration), suctionColor, Mathf.Max(0.1f, suctionRange * 0.9f)));

            float elapsed = 0f;
            float duration = Mathf.Max(0.05f, suctionDuration);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                Vector3 desired = Vector3.Lerp(startPosition, pullTarget, t);
                MovePlayerBy(player, desired - player.transform.position);
                transform.localScale = new Vector3(baseScale.x * (1f + t * 0.12f), baseScale.y * (1f - t * 0.08f), baseScale.z);
                yield return null;
            }

            elapsed = 0f;
            float delay = Mathf.Max(0f, spitDelay);
            while (elapsed < delay)
            {
                elapsed += Time.deltaTime;
                MovePlayerBy(player, pullTarget - player.transform.position);

                float pulse = 1f + Mathf.Sin(elapsed * 18f) * 0.04f;
                transform.localScale = new Vector3(baseScale.x * 1.12f, baseScale.y * 0.9f, baseScale.z) * pulse;
                yield return null;
            }

            if (IsPlayerWithinRange(player, shortRangeRadius))
            {
                player.TakeDamage(shortRangeDamage);
            }

            yield return SpitPlayerBackToStart(player, startPosition);
        }

        private IEnumerator SpitPlayerBackToStart(PlayerController player, Vector3 targetPosition)
        {
            if (player == null)
            {
                yield break;
            }

            targetPosition.y = player.transform.position.y;
            Vector3 returnStart = player.transform.position;
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, spitReturnDuration);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                Vector3 desired = Vector3.Lerp(returnStart, targetPosition, t);
                MovePlayerBy(player, desired - player.transform.position);
                yield return null;
            }

            MovePlayerBy(player, targetPosition - player.transform.position);
        }

        private void SplashAcid(Vector3 center)
        {
            center.y = GetGroundHeight(center) + 0.08f;
            GameObject splash = CreateTempSpriteObject(
                "StomachBoss_AcidSplash",
                GetRingSprite(),
                acidSplashColor,
                center,
                acidHitRadius * 2f,
                4900);
            StartCoroutine(FadeAndDestroy(splash, 0.35f, acidSplashColor, acidHitRadius * 2.7f));

            PlayerController player = PlayerController.Instance;
            if (!IsPlayerWithinRange(player, center, acidHitRadius))
            {
                return;
            }

            player.TakeDamage(acidDamage);
            ApplyPlayerAcidDamageOverTime(player, acidDebuffDuration, acidTickInterval, acidTickDamage);
        }

        private void ApplyPhaseStats()
        {
            if (stats == null)
            {
                return;
            }

            if (phase == BossPhase.Phase2)
            {
                stats.SetBaseStat(CharacterStatType.MoveSpeed, phase2MoveSpeed);
                stats.SetBaseStat(CharacterStatType.AttackPower, phase2AttackDamage);
            }
            else
            {
                stats.SetBaseStat(CharacterStatType.MoveSpeed, phase1MoveSpeed);
                stats.SetBaseStat(CharacterStatType.AttackPower, phase1AttackDamage);
            }
        }

        private void ApplyPhaseVisual()
        {
            if (visualRenderer == null)
            {
                visualRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (visualRenderer != null)
            {
                visualRenderer.color = phase == BossPhase.Phase2 ? phase2Color : phase1Color;
            }
        }

        private Vector3 GetMoveDirection()
        {
            Vector3 position = transform.position;
            Vector3 fromAnchor = position - anchorPosition;
            fromAnchor.y = 0f;
            if (fromAnchor.magnitude > roamRadius)
            {
                Vector3 toAnchor = anchorPosition - position;
                toAnchor.y = 0f;
                return toAnchor;
            }

            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                return Vector3.zero;
            }

            Vector3 toPlayer = player.transform.position - position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;
            if (distance <= 0.0001f)
            {
                return Vector3.zero;
            }

            if (phase == BossPhase.Phase1)
            {
                return distance > phase1MeleeRange * 1.05f ? toPlayer : Vector3.zero;
            }

            if (distance > acidRange * 0.88f)
            {
                return toPlayer;
            }

            if (distance < phase2PreferredDistance * 0.65f)
            {
                return -toPlayer;
            }

            if (distance > phase2PreferredDistance * 1.25f)
            {
                return toPlayer;
            }

            return Vector3.zero;
        }

        private bool IsPlayerWithinRange(PlayerController player, float range)
        {
            return IsPlayerWithinRange(player, transform.position, range);
        }

        private static bool IsPlayerWithinRange(PlayerController player, Vector3 center, float range)
        {
            if (player == null)
            {
                return false;
            }

            Vector3 toPlayer = player.transform.position - center;
            toPlayer.y = 0f;
            float safeRange = Mathf.Max(0.1f, range);
            return toPlayer.sqrMagnitude <= safeRange * safeRange;
        }

        private float GetChargeCooldown()
        {
            return useFastPatternCooldownsForDebug
                ? Mathf.Max(0.05f, fastPatternCooldown)
                : Mathf.Max(0f, chargeCooldown);
        }

        private float GetPhase2PatternInterval()
        {
            return useFastPatternCooldownsForDebug
                ? Mathf.Max(0.05f, fastPatternCooldown)
                : Mathf.Max(0f, phase2PatternInterval);
        }

        private Vector3 GetDirectionToPlayer()
        {
            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                return Vector3.forward;
            }

            Vector3 direction = player.transform.position - transform.position;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        private float GetGroundHeight(Vector3 position)
        {
            BiomeManager biome = BiomeManager.Active;
            return biome != null ? biome.GetGroundHeight(position) : position.y;
        }

        private static void ApplyPlayerAcidDamageOverTime(PlayerController player, float duration, float tickInterval, float tickDamage)
        {
            if (player == null)
            {
                return;
            }

            PlayerStatusEffectController status = player.GetComponent<PlayerStatusEffectController>();
            if (status == null)
            {
                status = player.gameObject.AddComponent<PlayerStatusEffectController>();
            }

            status.ApplyDamageOverTime(duration, tickInterval, tickDamage);
        }

        private static void ApplyPlayerKnockback(PlayerController player, Vector3 sourcePosition, float distance)
        {
            if (player == null || distance <= 0f)
            {
                return;
            }

            Vector3 direction = player.transform.position - sourcePosition;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.forward;
            }

            MovePlayerBy(player, direction.normalized * distance);
        }

        private static void MovePlayerBy(PlayerController player, Vector3 displacement)
        {
            if (player == null)
            {
                return;
            }

            displacement.y = 0f;
            if (displacement.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            Vector3 currentPosition = player.transform.position;
            Vector3 desiredPosition = currentPosition + displacement;
            float clampMargin = GetPlayerArenaClampMargin(player);
            if (MidBossArenaController.TryClampPlayerMovementInsideLockedArena(
                    currentPosition,
                    desiredPosition,
                    clampMargin,
                    out Vector3 clampedDesiredPosition))
            {
                displacement = clampedDesiredPosition - currentPosition;
                displacement.y = 0f;
                if (displacement.sqrMagnitude <= 0.000001f)
                {
                    CorrectPlayerBackIntoLockedArena(player, clampMargin);
                    return;
                }
            }
            else if (TryClampPositionInsideActiveMap(desiredPosition, clampMargin, out clampedDesiredPosition))
            {
                displacement = clampedDesiredPosition - currentPosition;
                displacement.y = 0f;
                if (displacement.sqrMagnitude <= 0.000001f)
                {
                    CorrectPlayerBackIntoActiveMap(player, clampMargin);
                    return;
                }
            }

            player.TryMoveByWorld(displacement);
            CorrectPlayerBackIntoLockedArena(player, clampMargin);
            CorrectPlayerBackIntoActiveMap(player, clampMargin);
        }

        private static float GetPlayerArenaClampMargin(PlayerController player)
        {
            Collider hitCollider = player != null ? player.HitCollider : null;
            if (hitCollider == null)
            {
                return 0.55f;
            }

            return Mathf.Max(0.35f, Mathf.Max(hitCollider.bounds.extents.x, hitCollider.bounds.extents.z) + ArenaMovementClampExtraMargin);
        }

        private static void CorrectPlayerBackIntoLockedArena(PlayerController player, float margin)
        {
            if (player == null)
            {
                return;
            }

            Vector3 currentPosition = player.transform.position;
            if (!MidBossArenaController.TryClampPositionInsideLockedArena(currentPosition, margin, out Vector3 correctedPosition))
            {
                return;
            }

            Vector3 planarDelta = correctedPosition - currentPosition;
            planarDelta.y = 0f;
            if (planarDelta.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            player.SpawnAt(correctedPosition);
        }

        private static void CorrectPlayerBackIntoActiveMap(PlayerController player, float margin)
        {
            if (player == null)
            {
                return;
            }

            Vector3 currentPosition = player.transform.position;
            if (!TryClampPositionInsideActiveMap(currentPosition, margin, out Vector3 correctedPosition))
            {
                return;
            }

            Vector3 planarDelta = correctedPosition - currentPosition;
            planarDelta.y = 0f;
            if (planarDelta.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            player.SpawnAt(correctedPosition);
        }

        private static bool TryClampPositionInsideActiveMap(Vector3 position, float margin, out Vector3 clampedPosition)
        {
            clampedPosition = position;
            BiomeManager biome = BiomeManager.Active;
            if (biome == null || biome.MapWidth <= 0 || biome.MapHeight <= 0)
            {
                return false;
            }

            float tileSize = Mathf.Max(0.01f, biome.TileSize);
            float safeMargin = Mathf.Max(0f, margin);
            float minX = safeMargin;
            float minZ = safeMargin;
            float maxX = Mathf.Max(minX, biome.MapWidth * tileSize - safeMargin);
            float maxZ = Mathf.Max(minZ, biome.MapHeight * tileSize - safeMargin);

            clampedPosition.x = Mathf.Clamp(position.x, minX, maxX);
            clampedPosition.z = Mathf.Clamp(position.z, minZ, maxZ);
            return true;
        }

        private IEnumerator FadeAndDestroy(GameObject obj, float duration, Color startColor, float endScale)
        {
            if (obj == null)
            {
                yield break;
            }

            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            Vector3 startScale = obj.transform.localScale;
            Vector3 finalScale = Vector3.one * Mathf.Max(0.01f, endScale);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
                obj.transform.localScale = Vector3.Lerp(startScale, finalScale, t);
                if (renderer != null)
                {
                    Color color = startColor;
                    color.a = Mathf.Lerp(startColor.a, 0f, t);
                    renderer.color = color;
                }

                yield return null;
            }

            Destroy(obj);
        }

        private GameObject CreateTempSpriteObject(string name, Sprite sprite, Color color, Vector3 position, float scale, int sortingOrder)
        {
            GameObject obj = new GameObject(name);
            obj.transform.position = position;
            obj.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);
            activeTempObjects.Add(obj);

            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            Billboard billboard = obj.AddComponent<Billboard>();
            billboard.SetUpdateMode(Billboard.UpdateMode.Continuous);
            return obj;
        }

        private void CleanupPatternObjects()
        {
            for (int i = 0; i < activeTempObjects.Count; i++)
            {
                if (activeTempObjects[i] != null)
                {
                    activeTempObjects[i].SetActive(false);
                    Destroy(activeTempObjects[i]);
                }
            }

            activeTempObjects.Clear();
        }

        private static Sprite GetCircleSprite()
        {
            if (circleSprite == null)
            {
                circleSprite = CreateCircleSprite("TempStomachAcidSprite", 48, 0.86f, true);
            }

            return circleSprite;
        }

        private static Sprite GetRingSprite()
        {
            if (ringSprite == null)
            {
                ringSprite = CreateCircleSprite("TempStomachRingSprite", 64, 0.94f, false);
            }

            return ringSprite;
        }

        private static Sprite CreateCircleSprite(string name, int size, float radiusRatio, bool filled)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            float center = (size - 1) * 0.5f;
            float radius = center * radiusRatio;
            float innerRadius = radius * 0.68f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = 0f;

                    if (filled && distance <= radius)
                    {
                        alpha = Mathf.Clamp01(1f - distance / radius * 0.35f);
                    }
                    else if (!filled && distance <= radius && distance >= innerRadius)
                    {
                        float midpoint = (innerRadius + radius) * 0.5f;
                        alpha = Mathf.Clamp01(1f - Mathf.Abs(distance - midpoint) / Mathf.Max(0.01f, radius - innerRadius));
                    }

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = name;
            return sprite;
        }
    }
}
