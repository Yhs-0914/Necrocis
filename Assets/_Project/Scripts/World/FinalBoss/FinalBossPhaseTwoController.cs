using System.Collections;
using System.Collections.Generic;
using ProceduralMap;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Necrocis
{
    /// <summary>
    /// Final boss phase two. Every attack owns a readable windup and is kept in a
    /// separate routine so individual patterns can be tuned without changing the scheduler.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(FinalBossArena), typeof(MapGenerator))]
    public sealed class FinalBossPhaseTwoController : MonoBehaviour
    {
        public enum PhaseTwoPattern
        {
            None = 0,
            ElectricLines = 1,
            HomingVolley = 2,
            CellBombs = 3,
            HalfArenaSweep = 4,
            ReflectionShield = 5,
            CellBarrage = 6
        }

        [Header("Boss")]
        [SerializeField, Min(1f)] private float bossMaxHealth = 520f;
        [SerializeField, Min(.1f)] private float phaseStartDelay = 1.4f;
        [SerializeField, Min(.1f)] private float patternCooldown = 1.8f;

        [Header("Pattern 1 - Electric Lines")]
        [SerializeField, Min(.1f)] private float electricWarningDuration = 1.35f;
        [SerializeField, Range(1, 6)] private int electricLineCount = 3;
        [SerializeField, Min(.2f)] private float electricLineWidth = 1.45f;
        [SerializeField, Min(0f)] private float electricDamage = 2f;
        [SerializeField, Min(.1f)] private float reversedControlDuration = 1f;

        [Header("Pattern 2 - Homing Volley")]
        [SerializeField, Min(.1f)] private float missileWarningDuration = 1.15f;
        [SerializeField, Range(1, 24)] private int missileCount = 12;
        [SerializeField, Min(.01f)] private float missileInterval = .11f;
        [SerializeField, Min(.1f)] private float missileSpeed = 7.8f;
        [SerializeField, Min(0f)] private float missileDamage = 1f;
        [SerializeField, Min(0f)] private float missileTurnSpeed = 115f;

        [Header("Pattern 3 - Cell Bombs")]
        [SerializeField, Min(.1f)] private float bombWarningDuration = 1.45f;
        [SerializeField, Range(1, 12)] private int bombCount = 6;
        [SerializeField, Min(.2f)] private float bombRadius = 2.35f;
        [SerializeField, Min(0f)] private float bombDamage = 2f;

        [Header("Pattern 4 - Half Arena")]
        [SerializeField, Min(.1f)] private float sweepWarningDuration = 1.55f;
        [SerializeField, Min(0f)] private float sweepDamage = 2f;

        [Header("Pattern 5 - Reflection")]
        [SerializeField, Min(.1f)] private float reflectionWarningDuration = 1.1f;
        [SerializeField, Min(.1f)] private float reflectionActiveDuration = 3.2f;
        [SerializeField, Range(0f, 2f)] private float reflectedDamageRatio = .55f;

        [Header("Pattern 6 - Cell Barrage")]
        [SerializeField, Min(.1f)] private float barrageWarningDuration = 1.2f;
        [SerializeField, Range(12, 36)] private int barrageCellsPerWave = 24;
        [SerializeField, Range(1, 5)] private int barrageWaveCount = 3;
        [SerializeField, Min(.2f)] private float barrageWaveInterval = .65f;
        [SerializeField, Min(.1f)] private float barrageCellSpeed = 6.4f;
        [SerializeField, Min(0f)] private float barrageCellDamage = 1f;

        private static readonly Color WarningColor = new Color(1f, .48f, .03f, .42f);
        private static readonly Color ElectricColor = new Color(.2f, .78f, 1f, .72f);
        private static readonly Color BombColor = new Color(1f, .1f, .32f, .72f);
        private static readonly Color ReflectionColor = new Color(.62f, .22f, 1f, .48f);
        private const string PhaseTwoResourceRoot = "FinalBoss/PhaseTwo/";

        private readonly List<PhaseTwoPattern> patternBag = new List<PhaseTwoPattern>(6);
        private FinalBossArena arena;
        private MapGenerator map;
        private Transform bossVisual;
        private SpriteRenderer bossRenderer;
        private Color bossBaseColor = Color.white;
        private Vector3 bossBaseScale = Vector3.one;
        private Transform runtimeRoot;
        private EnemyController bossDamageTarget;
        private FinalBossScreenHealthBar healthBar;
        private CombatHitFlash bossHitFlash;
        private Sprite tentacleSprite;
        private Sprite electricLineSprite;
        private Sprite cellBombSprite;
        private Sprite reflectionShieldSprite;
        private Coroutine attackLoop;
        private PhaseTwoPattern previousPattern;
        private bool active;
        private bool reflecting;
        private float nextReflectionHitTime;
        private bool awakening;
        private bool enraged;
        private float windupEnd;
        private float windupDuration;
        private float recoil;
        private float nextHitSoundTime;
        private FinalBossPhaseThreeController phaseThree;
        private bool phaseThreeStarted;

        public bool IsActive => active;
        public bool IsReflecting => reflecting;
        public bool IsEnraged => enraged;
        public int CurrentPhase => phaseThreeStarted ? 3 : 2;
        public FinalBossPhaseThreeController PhaseThree => phaseThree;
        public bool IsAttacking => phaseThreeStarted
            ? phaseThree != null && phaseThree.CurrentMobilePattern != FinalBossPhaseThreeController.MobilePattern.None
            : CurrentPattern != PhaseTwoPattern.None;
        public PhaseTwoPattern CurrentPattern { get; private set; }
        public EnemyController BossDamageTarget => bossDamageTarget;
        public float Health => bossDamageTarget != null && bossDamageTarget.Stats != null
            ? bossDamageTarget.Stats.CurrentHealth
            : 0f;
        public float MaxHealth => bossDamageTarget != null && bossDamageTarget.Stats != null
            ? bossDamageTarget.Stats.MaxHealth
            : bossMaxHealth;

        public void Begin(Transform revealedBoss)
        {
            if (active) return;
            arena = GetComponent<FinalBossArena>();
            map = GetComponent<MapGenerator>();
            bossVisual = revealedBoss;
            if (bossVisual != null) bossBaseScale = bossVisual.localScale;
            bossRenderer = bossVisual != null ? bossVisual.GetComponent<SpriteRenderer>() : null;
            if (bossRenderer != null)
            {
                bossBaseColor = bossRenderer.color;
                bossHitFlash = bossRenderer.GetComponent<CombatHitFlash>();
                if (bossHitFlash == null) bossHitFlash = bossRenderer.gameObject.AddComponent<CombatHitFlash>();
            }

            GameObject runtimeObject = new GameObject("FinalBoss Phase Two Runtime");
            runtimeObject.transform.SetParent(transform, false);
            runtimeRoot = runtimeObject.transform;
            LoadPatternSprites();
            CreateBossDamageTarget();

            active = true;
            attackLoop = StartCoroutine(AttackLoop());
        }

        private void CreateBossDamageTarget()
        {
            if (bossVisual == null || bossDamageTarget != null) return;
            var rule = new EnemySpawnRuleConfig
            {
                name = "Final Cerebrum",
                poissonSalt = 9801,
                maxHealth = bossMaxHealth,
                attackDamage = 0f,
                expReward = 0,
                moveSpeed = 0f,
                chaseRadius = 0f,
                leashRadius = 0f,
                enableContactDamage = false,
                addCollider = true,
                isTrigger = true,
                colliderSize = new Vector3(12.5f, 6f, 4.2f),
                colliderCenter = new Vector3(0f, 2.25f, 0f),
                useBillboard = false,
                useYSort = false,
                idleSprites = System.Array.Empty<Sprite>(),
                moveSprites = System.Array.Empty<Sprite>(),
                attackSprites = System.Array.Empty<Sprite>(),
                deathSprites = System.Array.Empty<Sprite>()
            };

            Vector3 targetPosition = bossVisual.position + Vector3.forward * 1.7f;
            targetPosition.y = GetGroundHeight(targetPosition);
            bossDamageTarget = EnemyController.Acquire(
                runtimeRoot,
                "FinalBoss_Cerebrum_DamageTarget",
                EnemyController.GetPoolArchetypeId(rule));
            bossDamageTarget.Configure(null, rule, targetPosition, targetPosition);
            bossDamageTarget.SetAiSuppressed(true);
            bossDamageTarget.SetIgnoreMidBossArenaRestriction(true);
            bossDamageTarget.DamageTaken += HandleBossDamageTaken;
            bossDamageTarget.Defeated += HandleBossDefeated;

            healthBar = FinalBossScreenHealthBar.Create(
                runtimeRoot,
                bossDamageTarget.Stats.CurrentHealth,
                bossDamageTarget.Stats.MaxHealth);
        }

        private IEnumerator AttackLoop()
        {
            awakening = true;
            healthBar?.SetPattern("AWAKENING", ElectricColor, "THE FOUR SEALS ARE BROKEN", phaseStartDelay);
            AudioManager.Instance?.PlaySFX("BossPhaseChange", .8f);
            Vector3 originalScale = bossVisual != null ? bossVisual.localScale : Vector3.one;
            float elapsed = 0f;
            while (elapsed < phaseStartDelay && active)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / phaseStartDelay);
                if (bossVisual != null)
                    bossVisual.localScale = originalScale * (1f + Mathf.Sin(t * Mathf.PI) * .045f);
                if (bossRenderer != null)
                    bossRenderer.color = Color.Lerp(new Color(.44f, .2f, .55f, bossBaseColor.a), bossBaseColor, t);
                yield return null;
            }
            if (bossVisual != null) bossVisual.localScale = originalScale;
            if (bossRenderer != null) bossRenderer.color = bossBaseColor;
            awakening = false;
            AudioManager.Instance?.PlaySFX("BossRoar", .7f);
            DontStarveCamera.Instance?.AddCombatImpulse(.2f, .3f);
            while (active && bossDamageTarget != null && !bossDamageTarget.IsDead)
            {
                if (PlayerController.Instance == null || PlayerController.Instance.IsDead)
                {
                    yield return null;
                    continue;
                }

                if (!enraged && Health <= MaxHealth * .5f)
                {
                    enraged = true;
                    healthBar?.SetPattern("NEURAL OVERDRIVE", BombColor, "THE CEREBRUM'S PULSE QUICKENS", 1.1f);
                    AudioManager.Instance?.PlaySFX("BossRoar", .65f);
                    recoil = .05f;
                    DontStarveCamera.Instance?.AddCombatImpulse(.15f, .22f);
                    yield return new WaitForSeconds(1.1f);
                }
                PhaseTwoPattern pattern = DrawNextPattern();
                CurrentPattern = pattern;
                windupDuration = GetWarningDuration(pattern);
                windupEnd = Time.time + windupDuration;
                healthBar?.SetPattern(GetPatternLabel(pattern), GetPatternColor(pattern),
                    GetPatternHint(pattern), windupDuration);
                yield return ExecutePattern(pattern);
                // Let this volley dissipate before exposing the next pattern's warning.
                if (pattern == PhaseTwoPattern.HomingVolley || pattern == PhaseTwoPattern.CellBarrage)
                {
                    healthBar?.SetPattern("CELLS IN FLIGHT", GetPatternColor(pattern), "WEAVE BETWEEN THE CELLS");
                    while (active && FinalBossHomingProjectile.ActiveCount > 0) yield return null;
                }
                CurrentPattern = PhaseTwoPattern.None;
                if (active)
                {
                    // Escalate the cadence gradually while preserving every attack's warning time.
                    float healthRatio = MaxHealth > 0f ? Health / MaxHealth : 1f;
                    float recovery = patternCooldown * Mathf.Lerp(.7f, 1f, healthRatio);
                    if (pattern == PhaseTwoPattern.ReflectionShield || pattern == PhaseTwoPattern.HalfArenaSweep)
                        recovery = Mathf.Max(2.1f, recovery);
                    healthBar?.SetPattern("CORE EXPOSED", new Color(.48f, .92f, .72f),
                        "ATTACK THE CEREBRUM", recovery);
                    yield return new WaitForSeconds(recovery);
                }
            }
            attackLoop = null;
        }

        private void Update()
        {
            if (!active || PlayerController.Instance == null || !PlayerController.Instance.IsDead) return;
            StopAllCoroutines();
            active = false;
            phaseThree?.StopMovement();
            reflecting = false;
            awakening = false;
            CurrentPattern = PhaseTwoPattern.None;
            attackLoop = null;
            if (bossHitFlash != null) bossHitFlash.enabled = false;
            if (bossVisual != null) bossVisual.localScale = bossBaseScale;
            if (bossRenderer != null) bossRenderer.color = bossBaseColor;
            if (runtimeRoot != null) runtimeRoot.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (!active || awakening || phaseThreeStarted || bossVisual == null) return;
            recoil = Mathf.MoveTowards(recoil, 0f, Time.deltaTime * .18f);
            float breath = Mathf.Sin(Time.time * (enraged ? 3.6f : 2.2f)) * .012f;
            float charge = Time.time < windupEnd && windupDuration > 0f
                ? Mathf.Clamp01(1f - (windupEnd - Time.time) / windupDuration) * .025f : 0f;
            bossVisual.localScale = Vector3.Scale(bossBaseScale,
                new Vector3(1f + breath + charge + recoil, 1f - breath * .5f + charge - recoil * .6f, 1f));
        }

        private void PlayAttackAccent(string sound, float volume = .5f)
        {
            recoil = .04f;
            AudioManager.Instance?.PlaySFX(sound, volume);
        }

        private PhaseTwoPattern DrawNextPattern()
        {
            if (patternBag.Count == 0)
            {
                patternBag.Add(PhaseTwoPattern.ElectricLines);
                patternBag.Add(PhaseTwoPattern.HomingVolley);
                patternBag.Add(PhaseTwoPattern.CellBombs);
                patternBag.Add(PhaseTwoPattern.HalfArenaSweep);
                patternBag.Add(PhaseTwoPattern.ReflectionShield);
                patternBag.Add(PhaseTwoPattern.CellBarrage);
                for (int i = patternBag.Count - 1; i > 0; i--)
                {
                    int swap = Random.Range(0, i + 1);
                    (patternBag[i], patternBag[swap]) = (patternBag[swap], patternBag[i]);
                }
                if (patternBag.Count > 1 && patternBag[patternBag.Count - 1] == previousPattern)
                {
                    int swap = Random.Range(0, patternBag.Count - 1);
                    (patternBag[swap], patternBag[patternBag.Count - 1]) =
                        (patternBag[patternBag.Count - 1], patternBag[swap]);
                }
            }

            int last = patternBag.Count - 1;
            PhaseTwoPattern result = patternBag[last];
            patternBag.RemoveAt(last);
            previousPattern = result;
            return result;
        }

        private IEnumerator ExecutePattern(PhaseTwoPattern pattern)
        {
            switch (pattern)
            {
                case PhaseTwoPattern.ElectricLines:
                    yield return ElectricLineRoutine();
                    break;
                case PhaseTwoPattern.HomingVolley:
                    yield return HomingVolleyRoutine();
                    break;
                case PhaseTwoPattern.CellBombs:
                    yield return CellBombRoutine();
                    break;
                case PhaseTwoPattern.HalfArenaSweep:
                    yield return HalfArenaSweepRoutine();
                    break;
                case PhaseTwoPattern.ReflectionShield:
                    yield return ReflectionRoutine();
                    break;
                case PhaseTwoPattern.CellBarrage:
                    yield return CellBarrageRoutine();
                    break;
            }
        }

        private IEnumerator ElectricLineRoutine()
        {
            float width = CombatWidth;
            float height = CombatHeight;
            bool horizontal = Random.value < .5f;
            int laneTotal = 7;
            int count = Mathf.Min(electricLineCount, laneTotal);
            var laneIndices = new List<int>(laneTotal);
            for (int i = 0; i < laneTotal; i++) laneIndices.Add(i);
            for (int i = laneIndices.Count - 1; i > 0; i--)
            {
                int swap = Random.Range(0, i + 1);
                (laneIndices[i], laneIndices[swap]) = (laneIndices[swap], laneIndices[i]);
            }

            var areas = new List<AttackArea>(count);
            var warnings = new List<FinalBossTelegraph>(count);
            FinalBossTelegraph bossTell = CreateBossTell(electricWarningDuration, ElectricColor);
            for (int i = 0; i < count; i++)
            {
                float lane = (laneIndices[i] + .5f) / laneTotal;
                Vector3 center = ArenaCenter;
                Vector2 size;
                if (horizontal)
                {
                    center.z = ArenaMinZ + height * lane;
                    size = new Vector2(width, electricLineWidth);
                }
                else
                {
                    center.x = ArenaMinX + width * lane;
                    size = new Vector2(electricLineWidth, height);
                }
                center = GroundPosition(center);
                areas.Add(new AttackArea(center, size));
                warnings.Add(FinalBossTelegraph.CreateRectangle(runtimeRoot, center, size,
                    electricWarningDuration, WarningColor));
            }

            yield return new WaitForSeconds(electricWarningDuration);
            PlayAttackAccent("LiverBossImpact", .5f);
            bossTell?.Dismiss();
            foreach (FinalBossTelegraph warning in warnings) warning?.Dismiss();
            PlayerController player = PlayerController.Instance;
            bool hit = player != null && areas.Exists(area => area.Contains(player.transform.position));
            foreach (AttackArea area in areas)
            {
                GameObject electricLine = CreateGroundSprite(
                    "FinalBoss_ElectricNerveLine",
                    electricLineSprite,
                    area.Center + Vector3.up * .04f,
                    area.Size,
                    Color.white,
                    4100);
                if (electricLine != null) Destroy(electricLine, .3f);
                FinalBossTelegraph.CreateRectangle(runtimeRoot, area.Center, area.Size,
                    .3f, ElectricColor, true);
            }
            bool canReceiveHit = hit
                && !player.IsDashInvincible
                && (player.HealthComponent == null || !player.HealthComponent.IsInvincible);
            if (canReceiveHit)
            {
                player.TakeDamage(electricDamage);
                player.ApplyMovementControlReversal(reversedControlDuration);
            }
            yield return new WaitForSeconds(.3f);
        }

        private IEnumerator HomingVolleyRoutine()
        {
            PlayerController player = PlayerController.Instance;
            if (player == null) yield break;
            FinalBossTelegraph bossTell = CreateBossTell(
                missileWarningDuration, new Color(1f, .15f, .5f, .55f));
            yield return new WaitForSeconds(missileWarningDuration);
            bossTell?.Dismiss();
            healthBar?.SetPattern("HOMING CELLS", GetPatternColor(PhaseTwoPattern.HomingVolley), "KEEP MOVING TO BREAK THEIR TRACKING");

            for (int i = 0; i < missileCount && active; i++)
            {
                player = PlayerController.Instance;
                if (player == null || player.IsDead) yield break;
                Vector3 spawn = bossVisual != null ? bossVisual.position : ArenaCenter;
                spawn.y = player.transform.position.y + 1.15f;
                Vector3 direction = player.transform.position - spawn;
                direction.y = 0f;
                float spread = Mathf.Lerp(-42f, 42f, missileCount <= 1 ? .5f : i / (float)(missileCount - 1));
                direction = Quaternion.Euler(0f, spread, 0f) * direction.normalized;
                FinalBossHomingProjectile.Launch(runtimeRoot, spawn, direction,
                    missileDamage, missileSpeed, missileTurnSpeed, 6.5f);
                if (i % 4 == 0) PlayAttackAccent("LiverBloodThrow", .32f);
                yield return new WaitForSeconds(missileInterval);
            }
        }

        private IEnumerator CellBarrageRoutine()
        {
            FinalBossTelegraph bossTell = CreateBossTell(barrageWarningDuration,
                new Color(1f, .25f, .65f, .65f));
            yield return new WaitForSeconds(barrageWarningDuration);
            bossTell?.Dismiss();

            int count = Mathf.Clamp(barrageCellsPerWave, 12, 36);
            int waves = Mathf.Clamp(barrageWaveCount, 1, 5);
            const float fanAngle = 150f;
            float angleStep = fanAngle / (count - 1);
            for (int wave = 0; wave < waves && active; wave++)
            {
                PlayerController player = PlayerController.Instance;
                if (player == null || player.IsDead) yield break;
                Vector3 origin = bossVisual != null ? bossVisual.position : ArenaCenter;
                origin.y = player.transform.position.y + 1.15f;
                Vector3 forward = ArenaCenter - origin;
                forward.y = 0f;
                forward = forward.sqrMagnitude > .0001f ? forward.normalized : Vector3.back;

                // Stagger the gaps between waves, with straight trajectories the player can read.
                float stagger = wave % 2 == 0 ? -angleStep * .25f : angleStep * .25f;
                healthBar?.SetPattern("CELL BARRAGE", GetPatternColor(PhaseTwoPattern.CellBarrage),
                    $"WAVE {wave + 1}/{waves}  //  WEAVE THROUGH THE GAPS");
                PlayAttackAccent("LiverBloodBurst", .4f);
                for (int i = 0; i < count; i++)
                {
                    float angle = -fanAngle * .5f + angleStep * i + stagger;
                    Vector3 direction = Quaternion.Euler(0f, angle, 0f) * forward;
                    FinalBossHomingProjectile cell = FinalBossHomingProjectile.Launch(
                        runtimeRoot, origin + direction * .8f, direction,
                        barrageCellDamage, barrageCellSpeed, 0f, 5f);
                    cell.name = "FinalBoss_BarrageCell";
                }
                FinalBossTelegraph.CreateCircle(runtimeRoot, GroundPosition(origin),
                    2.4f, .25f, new Color(1f, .25f, .65f, .6f), true);
                DontStarveCamera.Instance?.AddCombatImpulse(.09f, .15f);
                if (wave + 1 < waves) yield return new WaitForSeconds(barrageWaveInterval);
            }
            yield return new WaitForSeconds(2f);
        }

        private IEnumerator CellBombRoutine()
        {
            var positions = new List<Vector3>(bombCount);
            var fallingBombs = new List<GameObject>(bombCount);
            var warnings = new List<FinalBossTelegraph>(bombCount);
            PlayerController player = PlayerController.Instance;
            if (player != null) positions.Add(GroundPosition(player.transform.position));
            int attempts = 0;
            while (positions.Count < bombCount && attempts++ < 120)
            {
                Vector3 candidate = arena.UVToWorld(new Vector2(Random.Range(.17f, .83f), Random.Range(.24f, .77f)));
                candidate = GroundPosition(candidate);
                if (!arena.IsWalkable(candidate, new Vector2(.3f, .3f))) continue;
                bool overlaps = positions.Exists(existing =>
                    PlanarDistanceSqr(existing, candidate) < Mathf.Pow(bombRadius * 2f + .6f, 2f));
                // Fewer bombs are preferable to stacking warnings or placing them inside blocked terrain.
                if (!overlaps) positions.Add(candidate);
            }

            FinalBossTelegraph bossTell = CreateBossTell(bombWarningDuration, BombColor);
            foreach (Vector3 position in positions)
            {
                warnings.Add(FinalBossTelegraph.CreateCircle(runtimeRoot, position, bombRadius,
                    bombWarningDuration, WarningColor));
                fallingBombs.Add(CreateFallingBomb(position));
            }

            float elapsed = 0f;
            while (elapsed < bombWarningDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / bombWarningDuration);
                Camera camera = DontStarveCamera.GetActiveCamera();
                for (int i = 0; i < fallingBombs.Count; i++)
                {
                    GameObject bomb = fallingBombs[i];
                    if (bomb == null) continue;
                    Vector3 position = positions[i];
                    position.y += Mathf.Lerp(9f, .65f, t * t);
                    bomb.transform.position = position;
                    if (camera != null) bomb.transform.rotation = camera.transform.rotation;
                }
                yield return null;
            }

            bossTell?.Dismiss();
            foreach (FinalBossTelegraph warning in warnings) warning?.Dismiss();
            player = PlayerController.Instance;
            bool hit = player != null && positions.Exists(position =>
                PlanarDistanceSqr(position, player.transform.position) <= bombRadius * bombRadius);
            if (hit) player.TakeDamage(bombDamage);
            PlayAttackAccent("StomachImpact", .55f);
            DontStarveCamera.Instance?.AddCombatImpulse(.13f, .2f);
            foreach (Vector3 position in positions)
                FinalBossTelegraph.CreateCircle(runtimeRoot, position, bombRadius, .3f, BombColor, true);

            var impactScales = new List<Vector3>(fallingBombs.Count);
            foreach (GameObject bomb in fallingBombs)
                impactScales.Add(bomb != null ? bomb.transform.localScale : Vector3.one);
            const float burstDuration = .3f;
            elapsed = 0f;
            while (elapsed < burstDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / burstDuration);
                float burst = 1f - Mathf.Pow(1f - t, 3f);
                Camera camera = DontStarveCamera.GetActiveCamera();
                for (int i = 0; i < fallingBombs.Count; i++)
                {
                    GameObject bomb = fallingBombs[i];
                    if (bomb == null) continue;
                    bomb.transform.position = positions[i] + Vector3.up * Mathf.Lerp(.65f, 1.05f, t);
                    bomb.transform.localScale = impactScales[i] * Mathf.Lerp(1f, 2.15f, burst);
                    if (camera != null) bomb.transform.rotation = camera.transform.rotation;
                    SpriteRenderer renderer = bomb.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        Color color = renderer.color;
                        color.a = 1f - t;
                        renderer.color = color;
                    }
                }
                yield return null;
            }
            foreach (GameObject bomb in fallingBombs)
                if (bomb != null) Destroy(bomb);
        }

        private IEnumerator HalfArenaSweepRoutine()
        {
            int side = Random.Range(0, 2);
            float direction = side == 0 ? -1f : 1f;
            Vector3 center = ArenaCenter;
            Vector2 size = new Vector2(CombatWidth * .5f, CombatHeight);
            center.x += direction * CombatWidth * .25f;
            center = GroundPosition(center);
            AttackArea area = new AttackArea(center, size);
            FinalBossTelegraph bossTell = CreateBossTell(
                sweepWarningDuration, new Color(1f, .05f, .08f, .55f));
            FinalBossTelegraph warning = FinalBossTelegraph.CreateRectangle(runtimeRoot, center, size,
                sweepWarningDuration, WarningColor);

            SpriteRenderer tentacleRenderer;
            GameObject tentacle = CreateTentacle(direction, out tentacleRenderer);
            float windupAngle = direction * 54f;
            float elapsed = 0f;
            while (elapsed < sweepWarningDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / sweepWarningDuration);
                float anticipation = Mathf.SmoothStep(0f, 1f, t);
                float angle = Mathf.Lerp(direction * -22f, windupAngle, anticipation);
                float warningLength = Mathf.Lerp(8.5f, 11.5f, anticipation)
                    + Mathf.Sin(elapsed * 13f) * .22f;
                SetTentaclePose(tentacle, direction, angle, warningLength, 5.6f);
                if (tentacleRenderer != null)
                {
                    float pulse = .56f + Mathf.Sin(elapsed * 16f) * .12f + t * .25f;
                    tentacleRenderer.color = new Color(1f, .26f, .46f, Mathf.Clamp01(pulse));
                }
                yield return null;
            }

            const float swingDuration = .66f;
            bossTell?.Dismiss();
            warning?.Dismiss();

            SpriteRenderer afterimageRenderer;
            GameObject afterimage = CreateTentacle(direction, out afterimageRenderer);
            if (afterimage != null)
            {
                afterimage.name += "_Afterimage";
                afterimage.SetActive(false);
            }
            if (afterimageRenderer != null && tentacleRenderer != null)
                afterimageRenderer.sortingOrder = tentacleRenderer.sortingOrder - 1;

            bool dealtDamage = false;
            elapsed = 0f;
            while (elapsed < swingDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / swingDuration);
                GetTentacleAttackPose(direction, t, out float angle, out float length, out float thickness);
                SetTentaclePose(tentacle, direction, angle, length, thickness);
                if (tentacleRenderer != null)
                    tentacleRenderer.color = Color.Lerp(
                        new Color(1f, .92f, .98f, 1f),
                        new Color(.62f, .17f, .72f, .72f),
                        Mathf.Clamp01((t - .55f) / .45f));

                float trailT = t - .075f;
                if (trailT > 0f && afterimage != null)
                {
                    if (!afterimage.activeSelf) afterimage.SetActive(true);
                    GetTentacleAttackPose(direction, trailT, out float trailAngle,
                        out float trailLength, out float trailThickness);
                    SetTentaclePose(afterimage, direction, trailAngle, trailLength, trailThickness * 1.05f);
                    if (afterimageRenderer != null)
                    {
                        float trailAlpha = Mathf.Sin(Mathf.Clamp01(trailT) * Mathf.PI) * .34f;
                        afterimageRenderer.color = new Color(.28f, .9f, 1f, trailAlpha);
                    }
                }

                if (!dealtDamage && t >= .34f)
                {
                    dealtDamage = true;
                    PlayAttackAccent("IntestineBossImpact", .55f);
                    DontStarveCamera.Instance?.AddCombatImpulse(.22f, .24f);
                    PlayerController player = PlayerController.Instance;
                    if (player != null && area.Contains(player.transform.position)) player.TakeDamage(sweepDamage);
                }
                yield return null;
            }
            if (tentacle != null) Destroy(tentacle);
            if (afterimage != null) Destroy(afterimage);
        }

        private IEnumerator ReflectionRoutine()
        {
            healthBar?.SetPattern("REFLECTION INCOMING", ReflectionColor,
                "STOP ATTACKING WHEN THE SHIELD FORMS", reflectionWarningDuration);
            Vector3 center = GroundPosition(bossVisual != null ? bossVisual.position + Vector3.forward * 1.6f : ArenaCenter);
            FinalBossTelegraph warning = FinalBossTelegraph.CreateCircle(runtimeRoot, center, 6.2f,
                reflectionWarningDuration, WarningColor);
            FinalBossTelegraph bossTell = CreateBossTell(reflectionWarningDuration, ReflectionColor);

            SpriteRenderer shieldRenderer;
            GameObject shield = CreateBillboardSprite(
                "FinalBoss_ReflectionShield",
                reflectionShieldSprite,
                GetBossSpriteCenter(),
                15.5f,
                bossRenderer != null ? bossRenderer.sortingOrder + 6 : 5200,
                out shieldRenderer);
            Vector3 shieldScale = shield != null ? shield.transform.localScale : Vector3.one;
            float elapsed = 0f;
            while (elapsed < reflectionWarningDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / reflectionWarningDuration);
                UpdateBillboardRotation(shield);
                if (shield != null) shield.transform.localScale = shieldScale * Mathf.Lerp(.68f, 1f, t);
                if (shieldRenderer != null)
                    shieldRenderer.color = new Color(.78f, .42f, 1f, Mathf.Lerp(.15f, .78f, t));
                yield return null;
            }

            warning?.Dismiss();
            bossTell?.Dismiss();
            reflecting = true;
            if (bossHitFlash != null) bossHitFlash.enabled = false;
            PlayAttackAccent("BossPhaseChange", .35f);
            nextReflectionHitTime = 0f;
            healthBar?.SetPattern("REFLECTION ACTIVE", ReflectionColor,
                "DAMAGE IS REFLECTED BACK TO YOU", reflectionActiveDuration);
            if (bossRenderer != null) bossRenderer.color = new Color(.72f, .38f, 1f, bossBaseColor.a);
            elapsed = 0f;
            while (elapsed < reflectionActiveDuration)
            {
                elapsed += Time.deltaTime;
                UpdateBillboardRotation(shield);
                if (shield != null)
                    shield.transform.localScale = shieldScale * (1f + Mathf.Sin(elapsed * 5.5f) * .025f);
                if (shieldRenderer != null)
                    shieldRenderer.color = new Color(.86f, .64f, 1f, .76f + Mathf.Sin(elapsed * 8f) * .14f);
                yield return null;
            }
            reflecting = false;
            if (bossRenderer != null) bossRenderer.color = bossBaseColor;
            if (shield != null) Destroy(shield);
        }

        private void HandleBossDamageTaken(EnemyController target, float damage)
        {
            if (target == null || target.Stats == null) return;
            if (phaseThreeStarted && phaseThree != null && phaseThree.IsDetaching)
            {
                target.Stats.RestoreHealth(damage);
                return;
            }
            if (reflecting)
            {
                target.Stats.RestoreHealth(damage);
                PlayerController player = PlayerController.Instance;
                if (player != null && Time.time >= nextReflectionHitTime)
                {
                    nextReflectionHitTime = Time.time + .45f;
                    player.TakeDamage(Mathf.Clamp(damage * reflectedDamageRatio, 1f, 2f));
                }
            }
            else
            {
                // Preserve one shared health pool. Even an oversized hit must enter phase III.
                if (!phaseThreeStarted && target.Stats.HealthNormalized <= .4f)
                {
                    if (target.Stats.IsDead) target.Stats.RestoreHealth(target.Stats.MaxHealth * .4f);
                    BeginPhaseThree();
                    return;
                }
                bossHitFlash?.Flash(new Color(1f, .76f, .84f), .1f);
                recoil = Mathf.Max(recoil, .018f);
                if (Time.time >= nextHitSoundTime)
                {
                    AudioManager.Instance?.PlaySFX("EnemyHit", .4f);
                    nextHitSoundTime = Time.time + .15f;
                }
            }
            healthBar?.SetValue(
                target.Stats.HealthNormalized,
                target.Stats.CurrentHealth,
                target.Stats.MaxHealth);
        }

        private void BeginPhaseThree()
        {
            phaseThree = GetComponent<FinalBossPhaseThreeController>();
            if (phaseThree == null) phaseThree = gameObject.AddComponent<FinalBossPhaseThreeController>();

            // Snapshot only the old hazards: Begin creates new mobile-body/effect roots.
            var oldHazards = new List<GameObject>();
            foreach (Transform child in runtimeRoot)
            {
                if (child == bossDamageTarget.transform || (healthBar != null && child == healthBar.transform)) continue;
                oldHazards.Add(child.gameObject);
            }
            if (bossHitFlash != null) bossHitFlash.enabled = false;
            if (bossRenderer != null) bossRenderer.color = bossBaseColor;
            if (bossVisual != null) bossVisual.localScale = bossBaseScale;
            map.SetAuthoredAreaBlocked(new RectInt(17, 30, 14, 5), false);
            phaseThree.Begin(bossVisual, bossDamageTarget, runtimeRoot);
            if (!phaseThree.IsActive)
            {
                map.SetAuthoredAreaBlocked(new RectInt(17, 30, 14, 5), true);
                if (bossHitFlash != null) bossHitFlash.enabled = true;
                Debug.LogError("[FinalBoss] Phase three failed to start; phase two remains active.");
                return;
            }

            // Commit the encounter state only after the mobile controller actually starts.
            phaseThreeStarted = true;
            StopAllCoroutines();
            attackLoop = null;
            CurrentPattern = PhaseTwoPattern.None;
            awakening = false;
            reflecting = false;

            // Keep the health gauge and combat target, but remove every phase-two hazard.
            foreach (GameObject hazard in oldHazards)
            {
                if (hazard == null) continue;
                hazard.SetActive(false);
                Destroy(hazard);
            }
            healthBar?.SetValue(bossDamageTarget.Stats.HealthNormalized, Health, MaxHealth);
            healthBar?.SetPattern("PHASE III  //  UNBOUND", new Color(1f, .35f, .55f));
            GetComponent<FinalBossPhaseOneController>()?.NotifyPhaseThreeStarted();
            Debug.Log("[FinalBoss] Phase three: the cerebrum detached and is pursuing the player.");
        }

        internal void SetMobileReflection(bool value)
        {
            reflecting = value;
            if (value) nextReflectionHitTime = Time.time;
        }

        internal void SetMobilePattern(string label, string hint, float duration = 2f)
        {
            healthBar?.SetPattern(label, new Color(1f, .38f, .65f), hint, duration);
        }

        private void HandleBossDefeated(EnemyController defeated)
        {
            if (!active) return;
            active = false;
            phaseThree?.StopMovement();
            reflecting = false;
            CurrentPattern = PhaseTwoPattern.None;
            if (bossHitFlash != null) bossHitFlash.enabled = false;
            if (phaseThreeStarted && bossVisual != null)
            {
                // Preserve the detached artwork when the runtime combat root is cleaned up.
                bossVisual.SetParent(transform, true);
                bossBaseScale = bossVisual.localScale;
            }
            if (bossVisual != null) bossVisual.localScale = bossBaseScale;
            if (healthBar != null)
            {
                healthBar.transform.SetParent(transform, false);
                healthBar.SetValue(0f, 0f, MaxHealth);
                healthBar.SetPattern("CEREBRUM SILENCED", new Color(.55f, .86f, 1f), "FINAL BOSS DEFEATED", 3f);
            }
            SaveService.MarkFinalBossDefeated();
            StopAllCoroutines();
            attackLoop = null;
            if (runtimeRoot != null)
            {
                // Remove live hazards immediately, then let the victory pulse finish independently.
                runtimeRoot.gameObject.SetActive(false);
                Destroy(runtimeRoot.gameObject);
            }
            DontStarveCamera.Instance?.AddCombatImpulse(.32f, .4f);
            AudioManager.Instance?.PlaySFX("BossDeath", .85f);
            StartCoroutine(DefeatPresentation());
            Debug.Log("[FinalBoss] Final cerebrum defeated.");
        }

        private IEnumerator DefeatPresentation()
        {
            const float duration = 1.6f;
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                if (bossRenderer != null)
                    bossRenderer.color = Color.Lerp(bossBaseColor,
                        new Color(.24f, .18f, .28f, bossBaseColor.a), t);
                if (bossVisual != null)
                    bossVisual.localScale = Vector3.Scale(bossBaseScale, new Vector3(1f - t * .04f, 1f - t * .12f, 1f));
                yield return null;
            }
            if (bossRenderer != null) bossRenderer.color = new Color(.24f, .18f, .28f, bossBaseColor.a);
            yield return new WaitForSeconds(1.4f);
            if (healthBar != null) Destroy(healthBar.gameObject);
        }

        private float GetWarningDuration(PhaseTwoPattern pattern)
        {
            switch (pattern)
            {
                case PhaseTwoPattern.ElectricLines: return electricWarningDuration;
                case PhaseTwoPattern.HomingVolley: return missileWarningDuration;
                case PhaseTwoPattern.CellBombs: return bombWarningDuration;
                case PhaseTwoPattern.HalfArenaSweep: return sweepWarningDuration;
                case PhaseTwoPattern.ReflectionShield: return reflectionWarningDuration;
                case PhaseTwoPattern.CellBarrage: return barrageWarningDuration;
                default: return 0f;
            }
        }

        private static string GetPatternHint(PhaseTwoPattern pattern)
        {
            switch (pattern)
            {
                case PhaseTwoPattern.ElectricLines: return "STEP BETWEEN THE CHARGED LANES";
                case PhaseTwoPattern.HomingVolley: return "KEEP MOVING TO BREAK THEIR TRACKING";
                case PhaseTwoPattern.CellBombs: return "LEAVE THE MARKED LANDING ZONES";
                case PhaseTwoPattern.HalfArenaSweep: return "MOVE TO THE UNMARKED HALF";
                case PhaseTwoPattern.ReflectionShield: return "PREPARE TO HOLD FIRE";
                case PhaseTwoPattern.CellBarrage: return "KEEP YOUR DISTANCE AND WEAVE THROUGH THE GAPS";
                default: return "";
            }
        }

        private FinalBossTelegraph CreateBossTell(float duration, Color color)
        {
            Vector3 center = bossVisual != null ? bossVisual.position + Vector3.forward * 1.5f : ArenaCenter;
            return FinalBossTelegraph.CreateCircle(
                runtimeRoot, GroundPosition(center), 3.6f, duration, color);
        }

        private GameObject CreateFallingBomb(Vector3 target)
        {
            GameObject bomb = new GameObject("FinalBoss_FallingCellBomb");
            bomb.transform.SetParent(runtimeRoot, true);
            bomb.transform.position = target + Vector3.up * 9f;
            SpriteRenderer renderer = bomb.AddComponent<SpriteRenderer>();
            Sprite sprite = cellBombSprite != null ? cellBombSprite : FinalBossHomingProjectile.GetSharedSprite();
            renderer.sprite = sprite;
            renderer.color = cellBombSprite != null ? Color.white : new Color(.85f, .06f, .28f, 1f);
            renderer.sortingOrder = 5200;
            float spriteWidth = sprite != null ? Mathf.Max(.01f, sprite.bounds.size.x) : 1f;
            bomb.transform.localScale = Vector3.one * (2.1f / spriteWidth);
            return bomb;
        }

        private void LoadPatternSprites()
        {
            tentacleSprite = Resources.Load<Sprite>(PhaseTwoResourceRoot + "BossTentacle");
            electricLineSprite = Resources.Load<Sprite>(PhaseTwoResourceRoot + "ElectricNerveLine");
            cellBombSprite = Resources.Load<Sprite>(PhaseTwoResourceRoot + "CellBomb");
            reflectionShieldSprite = Resources.Load<Sprite>(PhaseTwoResourceRoot + "ReflectionShield");
        }

        private GameObject CreateGroundSprite(
            string objectName,
            Sprite sprite,
            Vector3 center,
            Vector2 size,
            Color color,
            int sortingOrder)
        {
            if (sprite == null || runtimeRoot == null) return null;
            GameObject visual = new GameObject(objectName);
            visual.transform.SetParent(runtimeRoot, true);
            visual.transform.position = center;
            Vector2 bounds = sprite.bounds.size;
            bool vertical = size.y > size.x;
            visual.transform.rotation = Quaternion.Euler(90f, 0f, 0f)
                * Quaternion.Euler(0f, 0f, vertical ? 90f : 0f);
            visual.transform.localScale = vertical
                ? new Vector3(
                    size.y / Mathf.Max(.01f, bounds.x),
                    size.x / Mathf.Max(.01f, bounds.y),
                    1f)
                : new Vector3(
                    size.x / Mathf.Max(.01f, bounds.x),
                    size.y / Mathf.Max(.01f, bounds.y),
                    1f);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return visual;
        }

        private GameObject CreateTentacle(float direction, out SpriteRenderer renderer)
        {
            renderer = null;
            if (tentacleSprite == null || runtimeRoot == null) return null;
            GameObject tentacle = new GameObject(direction < 0f
                ? "FinalBoss_LeftTentacleSweep"
                : "FinalBoss_RightTentacleSweep");
            tentacle.transform.SetParent(runtimeRoot, true);
            renderer = tentacle.AddComponent<SpriteRenderer>();
            renderer.sprite = tentacleSprite;
            renderer.color = new Color(1f, .28f, .48f, .5f);
            renderer.sortingOrder = 6300;
            SetTentaclePose(tentacle, direction, direction * -22f, 8.5f, 5.6f);
            return tentacle;
        }

        private void SetTentaclePose(
            GameObject tentacle,
            float direction,
            float angle,
            float desiredLength,
            float desiredThickness)
        {
            if (tentacle == null) return;
            tentacle.transform.position = GetTentacleAnchor(direction);
            Camera camera = DontStarveCamera.GetActiveCamera();
            Quaternion facing = camera != null
                ? camera.transform.rotation
                : bossVisual != null ? bossVisual.rotation : Quaternion.identity;
            tentacle.transform.rotation = facing * Quaternion.Euler(0f, 0f, angle);
            Vector2 spriteSize = tentacleSprite != null ? tentacleSprite.bounds.size : Vector2.one;
            float lengthScale = Mathf.Max(.1f, desiredLength) / Mathf.Max(.01f, spriteSize.x);
            float thicknessScale = Mathf.Max(.1f, desiredThickness) / Mathf.Max(.01f, spriteSize.y);
            tentacle.transform.localScale = new Vector3(
                direction * lengthScale,
                thicknessScale,
                Mathf.Max(lengthScale, thicknessScale));
        }

        private static void GetTentacleAttackPose(
            float direction,
            float normalized,
            out float angle,
            out float length,
            out float thickness)
        {
            float t = Mathf.Clamp01(normalized);
            const float extensionEnd = .34f;
            if (t <= extensionEnd)
            {
                float extension = Mathf.SmoothStep(0f, 1f, t / extensionEnd);
                angle = Mathf.Lerp(direction * 54f, direction * -62f, extension);
                length = Mathf.Lerp(11.5f, 34f, extension);
                thickness = Mathf.Lerp(5.6f, 7.4f, Mathf.Sin(extension * Mathf.PI * .5f));
                return;
            }

            float sweep = Mathf.SmoothStep(0f, 1f, (t - extensionEnd) / (1f - extensionEnd));
            angle = Mathf.Lerp(direction * -62f, direction * -14f, sweep);
            length = Mathf.Lerp(34f, 31f, sweep);
            thickness = Mathf.Lerp(7.4f, 6.4f, sweep);
        }

        private Vector3 GetTentacleAnchor(float direction)
        {
            if (bossVisual == null) return ArenaCenter + Vector3.up * 3f;
            return bossVisual.position
                + bossVisual.right * (direction * 4.75f)
                + bossVisual.up * 2.7f;
        }

        private GameObject CreateBillboardSprite(
            string objectName,
            Sprite sprite,
            Vector3 position,
            float desiredWidth,
            int sortingOrder,
            out SpriteRenderer renderer)
        {
            renderer = null;
            if (sprite == null || runtimeRoot == null) return null;
            GameObject visual = new GameObject(objectName);
            visual.transform.SetParent(runtimeRoot, true);
            visual.transform.position = position;
            renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            float scale = desiredWidth / Mathf.Max(.01f, sprite.bounds.size.x);
            visual.transform.localScale = Vector3.one * scale;
            UpdateBillboardRotation(visual);
            return visual;
        }

        private Vector3 GetBossSpriteCenter()
        {
            if (bossVisual == null) return ArenaCenter + Vector3.up * 3f;
            return bossVisual.position + bossVisual.up * 3.25f;
        }

        private static void UpdateBillboardRotation(GameObject visual)
        {
            if (visual == null) return;
            Camera camera = DontStarveCamera.GetActiveCamera();
            if (camera != null) visual.transform.rotation = camera.transform.rotation;
        }

        private Vector3 GroundPosition(Vector3 position)
        {
            position.y = GetGroundHeight(position) + .08f;
            return position;
        }

        private float GetGroundHeight(Vector3 position)
        {
            BiomeManager biome = BiomeManager.Active;
            return biome != null ? biome.GetGroundHeight(position) : arena != null ? arena.SpawnPosition.y : position.y;
        }

        private Vector3 ArenaCenter => new Vector3(
            ArenaMinX + CombatWidth * .5f,
            0f,
            ArenaMinZ + CombatHeight * .5f);
        private float ArenaMinX => 5f;
        private float ArenaMinZ => 7f;
        private float CombatWidth => Mathf.Max(8f, arena.WorldSize.x - 10f);
        private float CombatHeight => Mathf.Max(8f, arena.WorldSize.y - 14f);

        private static float PlanarDistanceSqr(Vector3 a, Vector3 b)
        {
            float x = a.x - b.x;
            float z = a.z - b.z;
            return x * x + z * z;
        }

        private static string GetPatternLabel(PhaseTwoPattern pattern)
        {
            switch (pattern)
            {
                case PhaseTwoPattern.ElectricLines: return "ELECTRIC SURGE";
                case PhaseTwoPattern.HomingVolley: return "HOMING CELLS";
                case PhaseTwoPattern.CellBombs: return "CELLULAR RAIN";
                case PhaseTwoPattern.HalfArenaSweep: return "TENTACLE SWEEP";
                case PhaseTwoPattern.ReflectionShield: return "REFLECTION INCOMING";
                case PhaseTwoPattern.CellBarrage: return "CELL BARRAGE";
                default: return "SYNAPSES CHARGING";
            }
        }

        private static Color GetPatternColor(PhaseTwoPattern pattern)
        {
            switch (pattern)
            {
                case PhaseTwoPattern.ElectricLines: return new Color(.28f, .86f, 1f);
                case PhaseTwoPattern.HomingVolley: return new Color(1f, .3f, .66f);
                case PhaseTwoPattern.CellBombs: return new Color(1f, .2f, .3f);
                case PhaseTwoPattern.HalfArenaSweep: return new Color(1f, .12f, .24f);
                case PhaseTwoPattern.ReflectionShield: return new Color(.72f, .38f, 1f);
                case PhaseTwoPattern.CellBarrage: return new Color(1f, .25f, .65f);
                default: return new Color(.72f, .52f, .72f);
            }
        }

        private readonly struct AttackArea
        {
            public AttackArea(Vector3 center, Vector2 size)
            {
                Center = center;
                Size = size;
            }

            public Vector3 Center { get; }
            public Vector2 Size { get; }

            public bool Contains(Vector3 point)
            {
                return Mathf.Abs(point.x - Center.x) <= Size.x * .5f
                    && Mathf.Abs(point.z - Center.z) <= Size.y * .5f;
            }
        }

        private void OnDestroy()
        {
            if (bossDamageTarget != null)
            {
                bossDamageTarget.DamageTaken -= HandleBossDamageTaken;
                bossDamageTarget.Defeated -= HandleBossDefeated;
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            attackLoop = null;
            if (healthBar != null) healthBar.Hide();
            if (runtimeRoot != null) runtimeRoot.gameObject.SetActive(false);
            if (!active) return;
            active = false;
            if (phaseThree != null) phaseThree.StopMovement();
            reflecting = false;
            CurrentPattern = PhaseTwoPattern.None;
            if (bossHitFlash != null) bossHitFlash.enabled = false;
            if (bossVisual != null) bossVisual.localScale = bossBaseScale;
            if (bossRenderer != null) bossRenderer.color = bossBaseColor;
        }
    }
}
