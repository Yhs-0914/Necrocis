using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProceduralMap;
using UnityEngine;

namespace Necrocis
{
    /// <summary>Final boss phase one: four biome seals and their reinforcements.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(FinalBossArena), typeof(MapGenerator))]
    public sealed class FinalBossPhaseOneController : MonoBehaviour
    {
        [SerializeField] private Transform bossVisual;
        [SerializeField] private FinalBossPillar[] pillars;
        [SerializeField] private BiomeConfig[] sourceBiomes;
        [SerializeField, Min(.25f)] private float spawnInterval = 3.5f;
        [SerializeField, Range(1, 4)] private int maximumLivingMinionsPerBiome = 1;
        [SerializeField, Range(.1f, 1f)] private float weakenedMidBossStatRatio = .33f;
        [SerializeField, Range(.1f, 1f)] private float weakenedMidBossScaleRatio = .5f;

        private readonly Dictionary<BiomeType, List<EnemyController>> minions = new Dictionary<BiomeType, List<EnemyController>>();
        private readonly Dictionary<BiomeType, EnemySpawnRuleConfig> normalRules = new Dictionary<BiomeType, EnemySpawnRuleConfig>();
        private readonly HashSet<BiomeType> disabledBiomes = new HashSet<BiomeType>();
        private readonly List<EnemySpawnRuleConfig> runtimeBossRules = new List<EnemySpawnRuleConfig>();
        private MapGenerator map;
        private FinalBossArena arena;
        private float nextSpawnTime;
        private int spawnCursor;
        private bool spawningPausedForTests;
        private Color bossAwakeColor = Color.white;
        private bool bossColorCaptured;

        public int CurrentPhase { get; private set; } = 1;
        public int DestroyedPillarCount => pillars == null ? 0 : pillars.Count(pillar => pillar != null && pillar.IsDestroyed);
        public int ConfiguredBiomeCount => normalRules.Count;
        public IReadOnlyList<FinalBossPillar> Pillars => pillars;
        public bool BossVisible => bossVisual != null && bossVisual.gameObject.activeSelf;
        public bool IsBiomeSpawning(BiomeType biome) => CurrentPhase == 1 && !disabledBiomes.Contains(biome);

        public void Configure(Transform boss, FinalBossPillar[] phasePillars, BiomeConfig[] biomes)
        {
            bossVisual = boss;
            pillars = phasePillars;
            sourceBiomes = biomes;
        }

        private IEnumerator Start()
        {
            map = GetComponent<MapGenerator>();
            arena = GetComponent<FinalBossArena>();
            SetBossPresentation(true);
            while (!map.IsReady || PlayerController.Instance == null || SaveService.IsRestorePending) yield return null;

            BuildRules();
            if (pillars != null)
                foreach (FinalBossPillar pillar in pillars)
                    pillar?.Initialize(this, map);
            SpawnInitialMinions();
            nextSpawnTime = Time.time + .8f;
        }

        private void Update()
        {
            if (CurrentPhase != 1 || spawningPausedForTests || Time.time < nextSpawnTime) return;
            nextSpawnTime = Time.time + spawnInterval;
            SpawnNextBiomeMinion();
        }

        private void BuildRules()
        {
            normalRules.Clear();
            minions.Clear();
            if (sourceBiomes == null) return;
            foreach (BiomeConfig biome in sourceBiomes)
            {
                if (biome == null || biome.biomeType == BiomeType.None) continue;
                EnemySpawnRuleConfig normal = biome.GetEnemySpawnRules().FirstOrDefault(rule => rule != null && !rule.isElite);
                if (normal != null) normalRules[biome.biomeType] = normal;
                minions[biome.biomeType] = new List<EnemyController>();
            }
        }

        private void SpawnNextBiomeMinion()
        {
            if (pillars == null || pillars.Length == 0) return;
            for (int attempt = 0; attempt < pillars.Length; attempt++)
            {
                FinalBossPillar pillar = pillars[spawnCursor++ % pillars.Length];
                if (pillar == null || pillar.IsDestroyed || disabledBiomes.Contains(pillar.Biome)) continue;
                if (!normalRules.TryGetValue(pillar.Biome, out EnemySpawnRuleConfig rule)) continue;
                List<EnemyController> active = minions[pillar.Biome];
                active.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy || enemy.IsDead);
                if (active.Count >= maximumLivingMinionsPerBiome) continue;
                EnemyController enemy = SpawnEnemy(rule, pillar.Biome, $"FinalBoss_{pillar.Biome}_Minion");
                if (enemy != null) active.Add(enemy);
                return;
            }
        }

        private void SpawnInitialMinions()
        {
            if (spawningPausedForTests || pillars == null) return;
            foreach (FinalBossPillar pillar in pillars)
            {
                if (pillar == null || !normalRules.TryGetValue(pillar.Biome, out EnemySpawnRuleConfig rule)) continue;
                EnemyController enemy = SpawnEnemy(rule, pillar.Biome, $"FinalBoss_{pillar.Biome}_Minion");
                if (enemy != null) minions[pillar.Biome].Add(enemy);
            }
        }

        private EnemyController SpawnEnemy(EnemySpawnRuleConfig rule, BiomeType biome, string objectName)
        {
            if (rule == null || !TryFindSpawnPosition(out Vector3 position)) return null;
            EnemyController enemy = EnemyController.Acquire(transform, objectName, EnemyController.GetPoolArchetypeId(rule));
            enemy.Configure(null, rule, position, position);
            return enemy;
        }

        private bool TryFindSpawnPosition(out Vector3 position)
        {
            for (int i = 0; i < 24; i++)
            {
                Vector2 uv = new Vector2(Random.Range(.22f, .78f), Random.Range(.24f, .73f));
                Vector3 candidate = arena.UVToWorld(uv);
                if (!arena.IsWalkable(candidate, new Vector2(.55f, .42f))) continue;
                if (PlayerController.Instance != null)
                {
                    Vector3 delta = candidate - PlayerController.Instance.transform.position;
                    delta.y = 0f;
                    if (delta.sqrMagnitude < 20f) continue;
                }
                BiomeManager biome = BiomeManager.Active;
                candidate.y = biome != null ? biome.GetGroundHeight(candidate) : arena.SpawnPosition.y;
                position = candidate;
                return true;
            }
            position = arena.UVToWorld(new Vector2(.5f, .5f));
            position.y = arena.SpawnPosition.y;
            return arena.IsWalkable(position, new Vector2(.55f, .42f));
        }

        public void NotifyPillarDestroyed(FinalBossPillar pillar)
        {
            if (pillar == null || CurrentPhase != 1) return;
            disabledBiomes.Add(pillar.Biome);
            SpawnWeakenedMidBoss(pillar.Biome);
            if (DestroyedPillarCount >= 4) BeginPhaseTwo();
        }

        private void SpawnWeakenedMidBoss(BiomeType biomeType)
        {
            BiomeConfig biome = sourceBiomes?.FirstOrDefault(candidate => candidate != null && candidate.biomeType == biomeType);
            MidBossDefinition definition = biome?.GetMidBossArenaConfig()?.boss;
            EnemySpawnRuleConfig source = definition != null && definition.useCustomBossRule
                ? definition.bossRule
                : biome?.GetEnemySpawnRules().FirstOrDefault(rule => rule != null && !rule.isElite);
            if (source == null) return;
            EnemySpawnRuleConfig weakened = CloneForWeakenedBoss(source, biomeType, definition);
            runtimeBossRules.Add(weakened);
            EnemyController boss = SpawnEnemy(weakened, biomeType, $"FinalBoss_{biomeType}_WeakenedMidBoss");
            if (boss != null) boss.SetIgnoreMidBossArenaRestriction(true);
        }

        private EnemySpawnRuleConfig CloneForWeakenedBoss(
            EnemySpawnRuleConfig source, BiomeType biome, MidBossDefinition definition)
        {
            float originalHealth = source.maxHealth;
            float originalAttackDamage = source.attackDamage;
            float originalMoveSpeed = source.moveSpeed;
            if (definition != null && definition.overrideStats)
            {
                originalHealth *= Mathf.Max(.01f, definition.maxHealthMultiplier);
                originalAttackDamage *= Mathf.Max(.01f, definition.attackDamageMultiplier);
                originalMoveSpeed *= Mathf.Max(.01f, definition.moveSpeedMultiplier);
            }
            if (definition != null)
                originalHealth = Mathf.Max(originalHealth, definition.minimumMaxHealth);

            Vector3 originalScaleMultiplier = GetSafeScaleMultiplier(definition?.scaleMultiplier ?? Vector3.one);
            Vector3 weakenedScaleMultiplier = originalScaleMultiplier * weakenedMidBossScaleRatio;
            Vector3 originalScale = Vector3.Scale(source.scale, originalScaleMultiplier);

            return new EnemySpawnRuleConfig
            {
                name = $"Weak {(!string.IsNullOrWhiteSpace(definition?.displayName) ? definition.displayName : source.name)}",
                poissonSalt = 9300 + (int)biome,
                maxAlive = 1,
                moveSpeed = originalMoveSpeed,
                stoppingDistance = source.stoppingDistance,
                wanderRadius = Mathf.Min(source.wanderRadius, 6f),
                chaseRadius = Mathf.Max(source.chaseRadius, 14f),
                leashRadius = 24f,
                idleDelayRange = source.idleDelayRange,
                maxHealth = Mathf.Max(1f, originalHealth * weakenedMidBossStatRatio),
                attackDamage = originalAttackDamage * weakenedMidBossStatRatio,
                attackRange = source.attackRange,
                attackCooldown = source.attackCooldown,
                expReward = 0,
                enableContactDamage = source.enableContactDamage,
                contactDamage = source.contactDamage * weakenedMidBossStatRatio,
                contactKnockbackDistance = source.contactKnockbackDistance,
                additionalBaseStats = source.additionalBaseStats != null ? new List<CharacterStatValue>(source.additionalBaseStats) : new List<CharacterStatValue>(),
                separationDistance = source.separationDistance,
                separationStrength = source.separationStrength,
                heightOffset = source.heightOffset,
                scale = originalScale * weakenedMidBossScaleRatio,
                sortingOrder = source.sortingOrder,
                useBillboard = source.useBillboard,
                useYSort = source.useYSort,
                animationSpeed = source.animationSpeed,
                addCollider = source.addCollider,
                isTrigger = source.isTrigger,
                colliderSize = Vector3.Scale(source.colliderSize, weakenedScaleMultiplier),
                colliderCenter = Vector3.Scale(source.colliderCenter, weakenedScaleMultiplier),
                idleSprites = source.idleSprites,
                idleSpritesUp = source.idleSpritesUp,
                idleSpritesDown = source.idleSpritesDown,
                moveSprites = source.moveSprites,
                moveSpritesUp = source.moveSpritesUp,
                moveSpritesDown = source.moveSpritesDown,
                attackSprites = source.attackSprites,
                attackSpritesUp = source.attackSpritesUp,
                attackSpritesDown = source.attackSpritesDown,
                attackAnimationSpeed = source.attackAnimationSpeed,
                isRanged = source.isRanged,
                projectileSpeed = source.projectileSpeed,
                projectileLifeTime = source.projectileLifeTime,
                projectileSprite = source.projectileSprite,
                projectileScale = source.projectileScale,
                projectileSpawnOffset = source.projectileSpawnOffset,
                expandColliderOnAttack = source.expandColliderOnAttack,
                attackColliderSize = source.attackColliderSize,
                attackColliderCenter = source.attackColliderCenter,
                deathSprites = source.deathSprites,
                deathAnimationSpeed = source.deathAnimationSpeed,
                isElite = false,
                tintColor = Color.white
            };
        }

        private static Vector3 GetSafeScaleMultiplier(Vector3 value)
        {
            return new Vector3(
                Mathf.Approximately(value.x, 0f) ? 1f : Mathf.Max(.01f, value.x),
                Mathf.Approximately(value.y, 0f) ? 1f : Mathf.Max(.01f, value.y),
                Mathf.Approximately(value.z, 0f) ? 1f : Mathf.Max(.01f, value.z));
        }

        private void BeginPhaseTwo()
        {
            CurrentPhase = 2;
            SetBossPresentation(false);
            Debug.Log("[FinalBoss] Four biome pillars destroyed. Phase two unlocked.");
        }

        private void SetBossPresentation(bool dormant)
        {
            if (bossVisual == null) return;
            bossVisual.gameObject.SetActive(true);
            SpriteRenderer renderer = bossVisual.GetComponent<SpriteRenderer>();
            if (renderer == null) return;
            if (!bossColorCaptured)
            {
                bossAwakeColor = renderer.color;
                bossColorCaptured = true;
            }
            renderer.color = dormant
                ? new Color(bossAwakeColor.r * .64f, bossAwakeColor.g * .58f,
                    bossAwakeColor.b * .68f, bossAwakeColor.a)
                : bossAwakeColor;
        }

#if UNITY_EDITOR
        public void PauseSpawningForTest()
        {
            spawningPausedForTests = true;
            foreach (List<EnemyController> active in minions.Values)
            {
                foreach (EnemyController enemy in active)
                    if (enemy != null && enemy.gameObject.activeInHierarchy) enemy.ReleaseToPool();
                active.Clear();
            }
        }
        public void DestroyAllPillarsForTest()
        {
            PauseSpawningForTest();
            if (pillars == null) return;
            foreach (FinalBossPillar pillar in pillars) pillar?.DestroyForTest();
        }
#endif
    }
}
