using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [System.Serializable]
    public class IntestineBossPatternSettings
    {
        [Header("Phase")]
        [Range(0.1f, 0.9f)] public float phase2HealthRatio = 0.5f;

        [Header("Movement")]
        public float phase1FleeDistance = 7f;
        public float phase1RoamRadius = 8f;
        public float phase1MoveMultiplier = 1f;
        public float phase2PreferredDistance = 2.5f;
        public float phase2MoveMultiplier = 1.15f;

        [Header("Dung")]
        public float dungPreDelay = 0.5f;
        public float phase1DungCooldown = 8f;
        public float phase2ThrowCooldown = 5f;
        public float dungCollisionDamage = 1f;
        [Range(0f, 1f)] public float dungSlowRatio = 0.2f;
        public float dungSlowDuration = 3f;
        public float dungHazardRadius = 0.75f;
        public float dungLifeTime = 10f;
        public float throwTravelTime = 0.7f;
        public float throwArcHeight = 2.5f;

        [Header("Parasites")]
        public int parasiteMinCount = 2;
        public int parasiteMaxCount = 3;
        public float parasiteSpawnDelay = 0.45f;
        public float parasiteSpawnRadius = 1.1f;
        public float parasiteMaxHealth = 6f;
        public float parasiteMoveSpeed = 2.3f;
        public float parasiteAttackDamage = 1f;
        public float parasiteAttackRange = 1.1f;
        public float parasiteAttackCooldown = 1.3f;
        public Vector3 parasiteScale = new Vector3(0.55f, 0.55f, 0.55f);

        [Header("Stomp")]
        public float stompMinDelay = 7f;
        public float stompPreDelay = 0.55f;
        public float stompRadius = 6f;
        public float stompDamage = 2f;
        [Range(0f, 1f)] public float stompSlowRatio = 0.3f;
        public float stompSlowDuration = 3f;

        [Header("Temporary Visuals")]
        public Color phase1Color = new Color(0.72f, 0.45f, 0.24f, 1f);
        public Color phase2Color = new Color(0.95f, 0.32f, 0.18f, 1f);
        public Color dungColor = new Color(0.34f, 0.18f, 0.08f, 0.95f);
        public Color parasiteColor = new Color(0.62f, 0.88f, 0.36f, 1f);
        public Color shockwaveColor = new Color(0.65f, 0.38f, 0.18f, 0.45f);

        [Header("Phase Verification")]
        public bool startInPhase2ForDebug = false;
        public bool useFastPatternCooldownsForDebug = false;
        public float fastPatternCooldown = 1f;
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public class IntestineBossPattern : MonoBehaviour, IBossPatternTempSpriteOwner
    {
        private enum BossPhase
        {
            Phase1,
            Transition,
            Phase2
        }

        [Header("Phase")]
        [SerializeField, Range(0.1f, 0.9f)] private float phase2HealthRatio = 0.5f;

        [Header("Movement")]
        [SerializeField] private float phase1FleeDistance = 7f;
        [SerializeField] private float phase1RoamRadius = 8f;
        [SerializeField] private float phase1MoveMultiplier = 1f;
        [SerializeField] private float phase2PreferredDistance = 2.5f;
        [SerializeField] private float phase2MoveMultiplier = 1.15f;

        [Header("Dung")]
        [SerializeField] private float dungPreDelay = 0.5f;
        [SerializeField] private float phase1DungCooldown = 8f;
        [SerializeField] private float phase2ThrowCooldown = 5f;
        [SerializeField] private float dungCollisionDamage = 1f;
        [SerializeField, Range(0f, 1f)] private float dungSlowRatio = 0.2f;
        [SerializeField] private float dungSlowDuration = 3f;
        [SerializeField] private float dungHazardRadius = 0.75f;
        [SerializeField] private float dungLifeTime = 10f;
        [SerializeField] private float throwTravelTime = 0.7f;
        [SerializeField] private float throwArcHeight = 2.5f;

        [Header("Parasites")]
        [SerializeField] private int parasiteMinCount = 2;
        [SerializeField] private int parasiteMaxCount = 3;
        [SerializeField] private float parasiteSpawnDelay = 0.45f;
        [SerializeField] private float parasiteSpawnRadius = 1.1f;

        [Header("Stomp")]
        [SerializeField] private float stompMinDelay = 7f;
        [SerializeField] private float stompPreDelay = 0.55f;
        [SerializeField] private float stompRadius = 6f;
        [SerializeField] private float stompDamage = 2f;
        [SerializeField, Range(0f, 1f)] private float stompSlowRatio = 0.3f;
        [SerializeField] private float stompSlowDuration = 3f;

        [Header("Temporary Visuals")]
        [SerializeField] private Color phase1Color = new Color(0.72f, 0.45f, 0.24f, 1f);
        [SerializeField] private Color phase2Color = new Color(0.95f, 0.32f, 0.18f, 1f);
        [SerializeField] private Color dungColor = new Color(0.34f, 0.18f, 0.08f, 0.95f);
        [SerializeField] private Color parasiteColor = new Color(0.62f, 0.88f, 0.36f, 1f);
        [SerializeField] private Color shockwaveColor = new Color(0.65f, 0.38f, 0.18f, 0.45f);

        [Header("Parasite Stats")]
        [SerializeField] private float parasiteMaxHealth = 6f;
        [SerializeField] private float parasiteMoveSpeed = 2.3f;
        [SerializeField] private float parasiteAttackDamage = 1f;
        [SerializeField] private float parasiteAttackRange = 1.1f;
        [SerializeField] private float parasiteAttackCooldown = 1.3f;
        [SerializeField] private Vector3 parasiteScale = new Vector3(0.55f, 0.55f, 0.55f);

        [Header("Phase Verification")]
        [SerializeField] private bool startInPhase2ForDebug = false;
        [SerializeField] private bool useFastPatternCooldownsForDebug = false;
        [SerializeField] private float fastPatternCooldown = 1f;

        private static Sprite dungSprite;
        private static Sprite parasiteSprite;
        private static Sprite shockwaveSprite;

        private EnemyController boss;
        private CharacterStats stats;
        private Transform summonParent;
        private EnemySpawnRuleConfig runtimeParasiteRule;
        private SpriteRenderer visualRenderer;
        private Vector3 anchorPosition;
        private Vector3 baseScale = Vector3.one;
        private BossPhase phase;
        private float nextDungTime;
        private float nextStompTime;
        private bool actionRunning;
        private bool encounterActive;
        private readonly List<GameObject> activeTempObjects = new List<GameObject>();
        private readonly List<EnemyController> activeSummons = new List<EnemyController>();

        public void Initialize(EnemyController controller, Vector3 anchor, Transform parent, IntestineBossPatternSettings settings = null)
        {
            ApplySettings(settings);

            boss = controller != null ? controller : GetComponent<EnemyController>();
            stats = boss != null ? boss.Stats : GetComponent<CharacterStats>();
            summonParent = parent;
            anchorPosition = anchor;
            phase = startInPhase2ForDebug ? BossPhase.Phase2 : BossPhase.Phase1;
            nextDungTime = Time.time + 1f;
            nextStompTime = phase == BossPhase.Phase2
                ? Time.time + GetStompCooldown()
                : float.PositiveInfinity;
            actionRunning = false;
            encounterActive = false;
            baseScale = transform.localScale;
            visualRenderer = GetComponentInChildren<SpriteRenderer>();
            runtimeParasiteRule = null;

            if (boss != null)
            {
                boss.SetAiSuppressed(true);
                boss.Defeated -= HandleBossDefeated;
                boss.Defeated += HandleBossDefeated;
            }

            ApplyPhaseVisual();
            enabled = true;
        }

        private void ApplySettings(IntestineBossPatternSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            phase2HealthRatio = settings.phase2HealthRatio;
            phase1FleeDistance = settings.phase1FleeDistance;
            phase1RoamRadius = settings.phase1RoamRadius;
            phase1MoveMultiplier = settings.phase1MoveMultiplier;
            phase2PreferredDistance = settings.phase2PreferredDistance;
            phase2MoveMultiplier = settings.phase2MoveMultiplier;
            dungPreDelay = settings.dungPreDelay;
            phase1DungCooldown = settings.phase1DungCooldown;
            phase2ThrowCooldown = settings.phase2ThrowCooldown;
            dungCollisionDamage = settings.dungCollisionDamage;
            dungSlowRatio = settings.dungSlowRatio;
            dungSlowDuration = settings.dungSlowDuration;
            dungHazardRadius = settings.dungHazardRadius;
            dungLifeTime = settings.dungLifeTime;
            throwTravelTime = settings.throwTravelTime;
            throwArcHeight = settings.throwArcHeight;
            parasiteMinCount = settings.parasiteMinCount;
            parasiteMaxCount = settings.parasiteMaxCount;
            parasiteSpawnDelay = settings.parasiteSpawnDelay;
            parasiteSpawnRadius = settings.parasiteSpawnRadius;
            parasiteMaxHealth = settings.parasiteMaxHealth;
            parasiteMoveSpeed = settings.parasiteMoveSpeed;
            parasiteAttackDamage = settings.parasiteAttackDamage;
            parasiteAttackRange = settings.parasiteAttackRange;
            parasiteAttackCooldown = settings.parasiteAttackCooldown;
            parasiteScale = settings.parasiteScale;
            stompMinDelay = settings.stompMinDelay;
            stompPreDelay = settings.stompPreDelay;
            stompRadius = settings.stompRadius;
            stompDamage = settings.stompDamage;
            stompSlowRatio = settings.stompSlowRatio;
            stompSlowDuration = settings.stompSlowDuration;
            phase1Color = settings.phase1Color;
            phase2Color = settings.phase2Color;
            dungColor = settings.dungColor;
            parasiteColor = settings.parasiteColor;
            shockwaveColor = settings.shockwaveColor;
            startInPhase2ForDebug = settings.startInPhase2ForDebug;
            useFastPatternCooldownsForDebug = settings.useFastPatternCooldownsForDebug;
            fastPatternCooldown = settings.fastPatternCooldown;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            CleanupPatternObjects();
            encounterActive = false;

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

        public string CurrentPhaseName => phase.ToString();

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
                nextDungTime = Time.time + 1f;
                nextStompTime = phase == BossPhase.Phase2
                    ? Time.time + GetStompCooldown()
                    : float.PositiveInfinity;
            }
        }

        [ContextMenu("Debug/Force Phase 1")]
        public void ForcePhase1ForDebug()
        {
            StopAllCoroutines();
            actionRunning = false;
            phase = BossPhase.Phase1;
            nextDungTime = Time.time;
            nextStompTime = float.PositiveInfinity;
            transform.localScale = baseScale;
            ApplyPhaseVisual();
        }

        [ContextMenu("Debug/Force Phase 2")]
        public void ForcePhase2ForDebug()
        {
            StopAllCoroutines();
            actionRunning = false;
            phase = BossPhase.Phase2;
            nextDungTime = Time.time;
            nextStompTime = Time.time;
            transform.localScale = baseScale;
            ApplyPhaseVisual();
        }

        [ContextMenu("Debug/Run Phase 1 Dung")]
        public void RunPhase1DungForDebug()
        {
            ForcePhase1ForDebug();
            StartCoroutine(Phase1DungRoutine());
        }

        [ContextMenu("Debug/Run Phase 2 Throw")]
        public void RunPhase2ThrowForDebug()
        {
            ForcePhase2ForDebug();
            StartCoroutine(ThrowDungRoutine());
        }

        [ContextMenu("Debug/Run Phase 2 Stomp")]
        public void RunPhase2StompForDebug()
        {
            ForcePhase2ForDebug();
            StartCoroutine(StompRoutine());
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

            if (PlayerController.Instance == null)
            {
                return;
            }

            if (phase == BossPhase.Phase1 && stats != null && stats.HealthNormalized <= phase2HealthRatio)
            {
                StartCoroutine(EnterPhase2Routine());
                return;
            }

            if (phase == BossPhase.Phase1 && Time.time >= nextDungTime && !actionRunning)
            {
                StartCoroutine(Phase1DungRoutine());
            }
            else if (phase == BossPhase.Phase2 && !actionRunning)
            {
                if (Time.time >= nextStompTime)
                {
                    StartCoroutine(StompRoutine());
                }
                else if (Time.time >= nextDungTime)
                {
                    StartCoroutine(ThrowDungRoutine());
                }
            }
        }

        private void LateUpdate()
        {
            if (!encounterActive || boss == null || boss.IsDead || phase == BossPhase.Transition || PlayerController.Instance == null)
            {
                return;
            }

            Vector3 moveDirection = phase == BossPhase.Phase1
                ? GetPhase1MoveDirection()
                : GetPhase2MoveDirection();

            if (moveDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float multiplier = phase == BossPhase.Phase1 ? phase1MoveMultiplier : phase2MoveMultiplier;
            float speed = stats != null ? stats.MoveSpeed : 1.5f;
            Vector3 step = moveDirection.normalized * Mathf.Max(0f, speed * multiplier) * Time.deltaTime;
            boss.MoveByExternalPattern(step);
        }

        private float GetPhase1DungCooldown()
        {
            return useFastPatternCooldownsForDebug
                ? Mathf.Max(0.05f, fastPatternCooldown)
                : Mathf.Max(0f, phase1DungCooldown);
        }

        private float GetPhase2ThrowCooldown()
        {
            return useFastPatternCooldownsForDebug
                ? Mathf.Max(0.05f, fastPatternCooldown)
                : Mathf.Max(0f, phase2ThrowCooldown);
        }

        private float GetStompCooldown()
        {
            return useFastPatternCooldownsForDebug
                ? Mathf.Max(0.05f, fastPatternCooldown)
                : Mathf.Max(0f, stompMinDelay);
        }

        private Vector3 GetPhase1MoveDirection()
        {
            Vector3 position = transform.position;
            Vector3 playerPosition = PlayerController.Instance.transform.position;
            Vector3 fromPlayer = position - playerPosition;
            fromPlayer.y = 0f;

            Vector3 fromAnchor = position - anchorPosition;
            fromAnchor.y = 0f;
            if (fromAnchor.magnitude > phase1RoamRadius)
            {
                Vector3 toAnchor = anchorPosition - position;
                toAnchor.y = 0f;
                return toAnchor;
            }

            if (fromPlayer.magnitude <= phase1FleeDistance)
            {
                return fromPlayer.sqrMagnitude > 0.0001f ? fromPlayer : RandomPlanarDirection();
            }

            Vector3 orbit = Vector3.Cross(Vector3.up, fromPlayer);
            return orbit.sqrMagnitude > 0.0001f ? orbit : RandomPlanarDirection();
        }

        private Vector3 GetPhase2MoveDirection()
        {
            Vector3 toPlayer = PlayerController.Instance.transform.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.magnitude <= phase2PreferredDistance)
            {
                return Vector3.zero;
            }

            return toPlayer;
        }

        private IEnumerator EnterPhase2Routine()
        {
            phase = BossPhase.Transition;
            actionRunning = true;

            float elapsed = 0f;
            float duration = 1.1f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float pulse = 1f + Mathf.Sin(elapsed * 22f) * 0.12f;
                transform.localScale = baseScale * pulse;
                if (visualRenderer != null)
                {
                    visualRenderer.color = Color.Lerp(phase1Color, phase2Color, Mathf.PingPong(elapsed * 6f, 1f));
                }

                yield return null;
            }

            transform.localScale = baseScale;
            phase = BossPhase.Phase2;
            nextDungTime = Time.time + 1.5f;
            nextStompTime = Time.time + GetStompCooldown();
            actionRunning = false;
            ApplyPhaseVisual();
        }

        private IEnumerator Phase1DungRoutine()
        {
            actionRunning = true;
            nextDungTime = Time.time + GetPhase1DungCooldown();
            yield return new WaitForSeconds(Mathf.Max(0f, dungPreDelay));

            Vector3 dropPosition = transform.position - GetDirectionToPlayer() * 0.7f;
            SpawnDungHazard(dropPosition, true);

            actionRunning = false;
        }

        private IEnumerator ThrowDungRoutine()
        {
            actionRunning = true;
            nextDungTime = Time.time + GetPhase2ThrowCooldown();
            yield return new WaitForSeconds(Mathf.Max(0f, dungPreDelay));

            Vector3 start = transform.position + Vector3.up * 1.2f;
            Vector3 target = PlayerController.Instance != null
                ? PlayerController.Instance.transform.position
                : transform.position + GetDirectionToPlayer() * 4f;
            target.y = GetGroundHeight(target) + 0.05f;

            GameObject projectile = CreateTempSpriteObject("IntestineBoss_DungProjectile", GetDungSprite(), dungColor, start, 0.7f, 5000);
            float elapsed = 0f;
            float duration = Mathf.Max(0.1f, throwTravelTime);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Vector3 pos = Vector3.Lerp(start, target, t);
                pos.y += Mathf.Sin(t * Mathf.PI) * throwArcHeight;
                if (projectile != null)
                {
                    projectile.transform.position = pos;
                }

                yield return null;
            }

            if (projectile != null)
            {
                ReleaseTempSprite(projectile);
            }

            SpawnDungHazard(target, true);
            actionRunning = false;
        }

        private IEnumerator StompRoutine()
        {
            actionRunning = true;
            nextStompTime = Time.time + GetStompCooldown();

            Vector3 originalScale = baseScale;
            float elapsed = 0f;
            float delay = Mathf.Max(0f, stompPreDelay);
            while (elapsed < delay)
            {
                elapsed += Time.deltaTime;
                float lift = Mathf.Sin(Mathf.Clamp01(elapsed / Mathf.Max(0.01f, delay)) * Mathf.PI);
                transform.localScale = new Vector3(originalScale.x * (1f + lift * 0.08f), originalScale.y * (1f + lift * 0.22f), originalScale.z * (1f + lift * 0.08f));
                yield return null;
            }

            transform.localScale = originalScale;
            SpawnShockwave(transform.position, stompRadius);

            PlayerController player = PlayerController.Instance;
            if (player != null)
            {
                Vector3 toPlayer = player.transform.position - transform.position;
                toPlayer.y = 0f;
                if (toPlayer.magnitude <= stompRadius)
                {
                    player.TakeDamage(stompDamage);
                    ApplyPlayerMoveSlow(player, stompSlowRatio, stompSlowDuration);
                }
            }

            actionRunning = false;
        }

        private void SpawnDungHazard(Vector3 position, bool spawnParasites)
        {
            position.y = GetGroundHeight(position) + 0.05f;
            GameObject obj = CreateTempSpriteObject("IntestineBoss_Dung", GetDungSprite(), dungColor, position, dungHazardRadius * 2f, 2500);
            BossDungHazard hazard = obj.AddComponent<BossDungHazard>();
            hazard.Initialize(this, dungCollisionDamage, dungSlowRatio, dungSlowDuration, dungHazardRadius, dungLifeTime, spawnParasites, parasiteSpawnDelay);
        }

        public void SpawnParasites(Vector3 center)
        {
            int min = Mathf.Max(0, parasiteMinCount);
            int max = Mathf.Max(min, parasiteMaxCount);
            int count = Random.Range(min, max + 1);
            EnemySpawnRuleConfig rule = GetParasiteRule();
            Transform parent = summonParent != null ? summonParent : transform.parent;

            for (int i = 0; i < count; i++)
            {
                Vector2 offset2D = Random.insideUnitCircle.normalized * Random.Range(0.35f, parasiteSpawnRadius);
                Vector3 spawnPosition = center + new Vector3(offset2D.x, 0f, offset2D.y);
                spawnPosition.y = GetGroundHeight(spawnPosition) + rule.heightOffset;

                int poolId = EnemyController.GetPoolArchetypeId(rule);
                EnemyController parasite = EnemyController.Acquire(parent, "IntestineParasite_Summon", poolId);
                parasite.Configure(null, rule, center, spawnPosition);
                parasite.SetIgnoreMidBossArenaRestriction(true);
                ApplyParasiteVisual(parasite);
                activeSummons.Add(parasite);
            }
        }

        private void SpawnShockwave(Vector3 center, float radius)
        {
            center.y = GetGroundHeight(center) + 0.08f;
            GameObject obj = CreateTempSpriteObject("IntestineBoss_StompShockwave", GetShockwaveSprite(), shockwaveColor, center, radius * 2f, 2400);
            StartCoroutine(FadeAndDestroy(obj, 0.45f));
        }

        private IEnumerator FadeAndDestroy(GameObject obj, float duration)
        {
            if (obj == null)
            {
                yield break;
            }

            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            Vector3 startScale = obj.transform.localScale * 0.35f;
            Vector3 endScale = obj.transform.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
                obj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                if (renderer != null)
                {
                    Color color = renderer.color;
                    color.a = Mathf.Lerp(shockwaveColor.a, 0f, t);
                    renderer.color = color;
                }

                yield return null;
            }

            ReleaseTempSprite(obj);
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

        private void ApplyParasiteVisual(EnemyController parasite)
        {
            if (parasite == null)
            {
                return;
            }

            SpriteRenderer renderer = parasite.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sprite = GetParasiteSprite();
                renderer.color = parasiteColor;
                renderer.sortingOrder = 1800;
            }
        }

        private Vector3 GetDirectionToPlayer()
        {
            if (PlayerController.Instance == null)
            {
                return transform.forward.sqrMagnitude > 0.0001f ? transform.forward : Vector3.forward;
            }

            Vector3 direction = PlayerController.Instance.transform.position - transform.position;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        private float GetGroundHeight(Vector3 position)
        {
            BiomeManager biome = BiomeManager.Active;
            return biome != null ? biome.GetGroundHeight(position) : position.y;
        }

        private static Vector3 RandomPlanarDirection()
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            return dir.sqrMagnitude > 0.0001f ? new Vector3(dir.x, 0f, dir.y) : Vector3.forward;
        }

        private static void ApplyPlayerMoveSlow(PlayerController player, float slowRatio, float duration)
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

            status.ApplyMoveSpeedSlow(slowRatio, duration);
        }

        private GameObject CreateTempSpriteObject(string name, Sprite sprite, Color color, Vector3 position, float scale, int sortingOrder)
        {
            return BossPatternVisualPool.Acquire(name, sprite, color, position, scale, sortingOrder, this, activeTempObjects);
        }

        public void ReleaseTempSprite(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            activeTempObjects.Remove(obj);
            BossPatternVisualPool.Release(obj);
        }

        private void CleanupPatternObjects()
        {
            for (int i = 0; i < activeTempObjects.Count; i++)
            {
                if (activeTempObjects[i] != null)
                {
                    BossPatternVisualPool.Release(activeTempObjects[i]);
                }
            }

            activeTempObjects.Clear();

            for (int i = 0; i < activeSummons.Count; i++)
            {
                EnemyController summon = activeSummons[i];
                if (summon != null && summon.gameObject.activeInHierarchy && !summon.IsDead)
                {
                    summon.ReleaseToPool();
                }
            }

            activeSummons.Clear();
        }

        private EnemySpawnRuleConfig GetParasiteRule()
        {
            if (runtimeParasiteRule != null)
            {
                return runtimeParasiteRule;
            }

            runtimeParasiteRule = new EnemySpawnRuleConfig
            {
                name = "IntestineParasite",
                poissonSalt = 9917,
                maxAlive = 1,
                activationRadius = 0f,
                respawnCooldown = 0f,
                spawnRadius = 0f,
                moveSpeed = parasiteMoveSpeed,
                stoppingDistance = 0.15f,
                wanderRadius = 2f,
                chaseRadius = 18f,
                leashRadius = 24f,
                idleDelayRange = new Vector2(0.1f, 0.35f),
                maxHealth = parasiteMaxHealth,
                attackDamage = parasiteAttackDamage,
                attackRange = parasiteAttackRange,
                attackCooldown = parasiteAttackCooldown,
                expReward = 0,
                separationDistance = 0.7f,
                separationStrength = 0.8f,
                heightOffset = 0f,
                scale = parasiteScale,
                sortingOrder = 1800,
                useBillboard = true,
                useYSort = true,
                addCollider = true,
                isTrigger = false,
                colliderSize = new Vector3(0.5f, 0.7f, 0.5f),
                colliderCenter = new Vector3(0f, 0.35f, 0f)
            };

            return runtimeParasiteRule;
        }

        private static Sprite GetDungSprite()
        {
            if (dungSprite == null)
            {
                dungSprite = CreateCircleSprite("TempDungSprite", 48, 0.82f, true);
            }

            return dungSprite;
        }

        private static Sprite GetParasiteSprite()
        {
            if (parasiteSprite == null)
            {
                parasiteSprite = CreateCapsuleSprite("TempParasiteSprite", 48, 28);
            }

            return parasiteSprite;
        }

        private static Sprite GetShockwaveSprite()
        {
            if (shockwaveSprite == null)
            {
                shockwaveSprite = CreateCircleSprite("TempShockwaveSprite", 64, 0.95f, false);
            }

            return shockwaveSprite;
        }

        private static Sprite CreateCircleSprite(string name, int size, float radiusRatio, bool filled)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            float center = (size - 1) * 0.5f;
            float radius = center * radiusRatio;
            float innerRadius = radius * 0.72f;

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
                        alpha = Mathf.Clamp01(1f - Mathf.Abs(distance - (innerRadius + radius) * 0.5f) / Mathf.Max(0.01f, radius - innerRadius));
                    }

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = name;
            return sprite;
        }

        private static Sprite CreateCapsuleSprite(string name, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
            float rx = width * 0.42f;
            float ry = height * 0.36f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x - center.x) / rx;
                    float ny = (y - center.y) / ry;
                    float value = nx * nx + ny * ny;
                    float alpha = value <= 1f ? 1f : 0f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), width);
            sprite.name = name;
            return sprite;
        }

        private class BossDungHazard : MonoBehaviour
        {
            private IntestineBossPattern owner;
            private float damage;
            private float slowRatio;
            private float slowDuration;
            private float radius;
            private float destroyTime;
            private bool hasHitPlayer;
            private bool spawnParasites;
            private float parasiteDelay;

            public void Initialize(IntestineBossPattern owner, float damage, float slowRatio, float slowDuration, float radius, float lifeTime, bool spawnParasites, float parasiteDelay)
            {
                StopAllCoroutines();
                this.owner = owner;
                this.damage = damage;
                this.slowRatio = slowRatio;
                this.slowDuration = slowDuration;
                this.radius = Mathf.Max(0.05f, radius);
                hasHitPlayer = false;
                this.spawnParasites = spawnParasites;
                this.parasiteDelay = Mathf.Max(0f, parasiteDelay);
                destroyTime = Time.time + Mathf.Max(0.2f, lifeTime);

                SphereCollider collider = GetComponent<SphereCollider>();
                if (collider == null)
                {
                    collider = gameObject.AddComponent<SphereCollider>();
                }

                collider.enabled = true;
                collider.isTrigger = true;
                collider.radius = this.radius;

                Rigidbody body = GetComponent<Rigidbody>();
                if (body == null)
                {
                    body = gameObject.AddComponent<Rigidbody>();
                }

                body.useGravity = false;
                body.isKinematic = true;

                if (spawnParasites)
                {
                    StartCoroutine(SpawnParasitesRoutine());
                }
            }

            private void Update()
            {
                if (Time.time >= destroyTime)
                {
                    ReleaseSelf();
                    return;
                }

                if (!hasHitPlayer && PlayerController.Instance != null)
                {
                    Vector3 toPlayer = PlayerController.Instance.transform.position - transform.position;
                    toPlayer.y = 0f;
                    if (toPlayer.magnitude <= radius)
                    {
                        HitPlayer(PlayerController.Instance);
                    }
                }
            }

            private void OnTriggerEnter(Collider other)
            {
                if (hasHitPlayer || other == null)
                {
                    return;
                }

                PlayerController player = other.GetComponent<PlayerController>();
                if (player == null)
                {
                    player = other.GetComponentInParent<PlayerController>();
                }

                if (player != null)
                {
                    HitPlayer(player);
                }
            }

            private void HitPlayer(PlayerController player)
            {
                hasHitPlayer = true;
                player.TakeDamage(damage);
                ApplyPlayerMoveSlow(player, slowRatio, slowDuration);
            }

            private IEnumerator SpawnParasitesRoutine()
            {
                yield return new WaitForSeconds(parasiteDelay);
                if (owner != null && spawnParasites)
                {
                    owner.SpawnParasites(transform.position);
                }
            }

            private void ReleaseSelf()
            {
                BossPatternTempSprite tempSprite = GetComponent<BossPatternTempSprite>();
                if (tempSprite != null)
                {
                    tempSprite.Release();
                    return;
                }

                if (owner != null)
                {
                    owner.ReleaseTempSprite(gameObject);
                }
                else
                {
                    BossPatternVisualPool.Release(gameObject);
                }
            }

            private void OnDisable()
            {
                StopAllCoroutines();
                owner = null;
                hasHitPlayer = false;
            }
        }
    }
}
