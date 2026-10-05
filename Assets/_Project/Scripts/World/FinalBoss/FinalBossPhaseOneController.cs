using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProceduralMap;
using UnityEngine;
using Random = UnityEngine.Random;

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
        [SerializeField, Range(1, 12)] private int commonMinionsPerWave = 4;
        [SerializeField, Range(1, 3)] private int exclusiveTypesPerBiome = 2;
        [SerializeField, Range(1, 4)] private int exclusiveCopiesPerType = 2;
        [SerializeField, Range(.1f, 1f)] private float weakenedMidBossStatRatio = .33f;
        [SerializeField, Range(.1f, 1f)] private float weakenedMidBossScaleRatio = 1f;

        private readonly List<EnemySpawnRuleConfig> commonRules = new List<EnemySpawnRuleConfig>();
        private readonly List<EnemyController> commonMinions = new List<EnemyController>();
        private readonly Dictionary<BiomeType, List<EnemySpawnRuleConfig>> exclusiveRules = new Dictionary<BiomeType, List<EnemySpawnRuleConfig>>();
        private readonly Dictionary<BiomeType, List<EnemySpawnRuleConfig>> selectedExclusiveRules = new Dictionary<BiomeType, List<EnemySpawnRuleConfig>>();
        private readonly Dictionary<BiomeType, List<EnemyController>> exclusiveMinions = new Dictionary<BiomeType, List<EnemyController>>();
        private readonly HashSet<BiomeType> disabledBiomes = new HashSet<BiomeType>();
        private readonly List<EnemySpawnRuleConfig> runtimeBossRules = new List<EnemySpawnRuleConfig>();
        private MapGenerator map;
        private FinalBossArena arena;
        private float nextSpawnTime;
        private int commonSpawnCursor;
        private int exclusiveSpawnSequence;
        private bool spawningPausedForTests;
        private Color bossAwakeColor = Color.white;
        private bool bossColorCaptured;
        private FinalBossScreenHealthBar sealGauge;
        private Vector3 bossRestScale;
        private SpriteRenderer dormantRenderer;
        private bool initialized;

        public Vector3 BossGroundPosition => new Vector3(
            bossVisual != null ? bossVisual.position.x : 24f,
            arena != null ? arena.SpawnPosition.y + .12f : .12f,
            bossVisual != null ? bossVisual.position.z : 30f);

        public int CurrentPhase { get; private set; } = 1;
        public int DestroyedPillarCount => pillars == null ? 0 : pillars.Count(pillar => pillar != null && pillar.IsDestroyed);
        public int ConfiguredBiomeCount => exclusiveRules.Count;
        public int CommonRuleCount => commonRules.Count;
        public IReadOnlyList<FinalBossPillar> Pillars => pillars;
        public bool BossVisible => bossVisual != null && bossVisual.gameObject.activeSelf;
        public bool IsBiomeSpawning(BiomeType biome) => CurrentPhase == 1 && !disabledBiomes.Contains(biome);
        public int GetExclusiveRuleCount(BiomeType biome) =>
            exclusiveRules.TryGetValue(biome, out List<EnemySpawnRuleConfig> rules) ? rules.Count : 0;

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
            if (bossVisual != null)
            {
                bossRestScale = bossVisual.localScale;
                dormantRenderer = bossVisual.GetComponent<SpriteRenderer>();
            }
            while (!map.IsReady || PlayerController.Instance == null || SaveService.IsRestorePending) yield return null;

            BuildRules();
            sealGauge = FinalBossScreenHealthBar.Create(transform, 4f, 4f);
            sealGauge.SetSealProgress(DestroyedPillarCount, 4);
            if (pillars != null)
                foreach (FinalBossPillar pillar in pillars)
                    pillar?.Initialize(this, map);
            sealGauge.BindSeals(pillars);
            initialized = true;
            SpawnInitialMinions();
            nextSpawnTime = Time.time + spawnInterval;
        }

        private void Update()
        {
            if (!initialized || CurrentPhase != 1 || spawningPausedForTests
                || PlayerController.Instance == null || PlayerController.Instance.IsDead
                || Time.time < nextSpawnTime) return;
            nextSpawnTime = Time.time + spawnInterval;
            ReplenishMinions();
        }

        private void LateUpdate()
        {
            if (!initialized || CurrentPhase != 1 || bossVisual == null) return;
            float progress = DestroyedPillarCount / 4f;
            float breath = Mathf.Sin(Time.time * Mathf.Lerp(1.35f, 2.8f, progress));
            bossVisual.localScale = Vector3.Scale(bossRestScale,
                new Vector3(1f + breath * .009f, 1f + breath * .016f, 1f));
            if (dormantRenderer != null)
                dormantRenderer.color = Color.Lerp(
                    new Color(bossAwakeColor.r * .64f, bossAwakeColor.g * .58f,
                        bossAwakeColor.b * .68f, bossAwakeColor.a), bossAwakeColor, progress * .6f);
        }

        private void BuildRules()
        {
            commonRules.Clear();
            commonMinions.Clear();
            exclusiveRules.Clear();
            selectedExclusiveRules.Clear();
            exclusiveMinions.Clear();
            disabledBiomes.Clear();
            commonSpawnCursor = 0;
            exclusiveSpawnSequence = 0;
            if (sourceBiomes == null) return;
            foreach (BiomeConfig biome in sourceBiomes)
            {
                if (biome == null || biome.biomeType == BiomeType.None) continue;
                var biomeSpecific = new List<EnemySpawnRuleConfig>();
                foreach (EnemySpawnRuleConfig rule in biome.GetEnemySpawnRules())
                {
                    if (rule == null || rule.isElite) continue;
                    if (IsBiomeSpecificRule(rule, biome.biomeType))
                    {
                        biomeSpecific.Add(rule);
                        continue;
                    }

                    if (!commonRules.Any(candidate => string.Equals(candidate.name, rule.name, StringComparison.OrdinalIgnoreCase)))
                        commonRules.Add(rule);
                }

                if (biomeSpecific.Count > 0)
                {
                    exclusiveRules[biome.biomeType] = biomeSpecific;
                    exclusiveMinions[biome.biomeType] = new List<EnemyController>();
                }
            }
        }

        private static bool IsBiomeSpecificRule(EnemySpawnRuleConfig rule, BiomeType biome)
        {
            return rule != null
                && !string.IsNullOrWhiteSpace(rule.name)
                && rule.name.StartsWith(biome.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private void SpawnInitialMinions()
        {
            if (spawningPausedForTests || pillars == null) return;
            foreach (FinalBossPillar pillar in pillars)
            {
                if (pillar == null || !exclusiveRules.TryGetValue(pillar.Biome, out List<EnemySpawnRuleConfig> rules))
                    continue;
                SpawnBiomeExclusiveGroup(pillar.Biome, rules);
            }

            ReplenishCommonMinions();
        }

        private void SpawnBiomeExclusiveGroup(BiomeType biome, List<EnemySpawnRuleConfig> rules)
        {
            int typeCount = Mathf.Min(exclusiveTypesPerBiome, rules.Count);
            var shuffled = new List<EnemySpawnRuleConfig>(rules);
            var selected = new List<EnemySpawnRuleConfig>(typeCount);
            for (int i = 0; i < typeCount; i++)
            {
                int selectedIndex = Random.Range(i, shuffled.Count);
                (shuffled[i], shuffled[selectedIndex]) = (shuffled[selectedIndex], shuffled[i]);
                selected.Add(shuffled[i]);
            }
            selectedExclusiveRules[biome] = selected;
            ReplenishBiomeExclusiveMinions(biome);
        }

        private void ReplenishMinions()
        {
            ReplenishCommonMinions();
            foreach (BiomeType biome in selectedExclusiveRules.Keys)
                ReplenishBiomeExclusiveMinions(biome);
        }

        private void ReplenishBiomeExclusiveMinions(BiomeType biome)
        {
            if (disabledBiomes.Contains(biome)
                || !selectedExclusiveRules.TryGetValue(biome, out List<EnemySpawnRuleConfig> selected)
                || !exclusiveMinions.TryGetValue(biome, out List<EnemyController> active))
                return;

            active.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy || enemy.IsDead);
            foreach (EnemySpawnRuleConfig rule in selected)
            {
                int livingCount = active.Count(enemy => enemy.Config == rule);
                while (livingCount < exclusiveCopiesPerType)
                {
                    EnemyController enemy = SpawnEnemy(rule, biome,
                        $"FinalBoss_{biome}_BiomeMinion_{rule.name}_{++exclusiveSpawnSequence}");
                    if (enemy == null) break;
                    active.Add(enemy);
                    livingCount++;
                }
            }
        }

        private void ReplenishCommonMinions()
        {
            commonMinions.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy || enemy.IsDead);
            while (commonMinions.Count < commonMinionsPerWave && commonRules.Count > 0)
            {
                EnemySpawnRuleConfig rule = commonRules[commonSpawnCursor++ % commonRules.Count];
                EnemyController enemy = SpawnEnemy(rule, BiomeType.None,
                    $"FinalBoss_Common_{rule.name}_{commonSpawnCursor}");
                if (enemy == null) break;
                commonMinions.Add(enemy);
            }
        }

        private EnemyController SpawnEnemy(EnemySpawnRuleConfig rule, BiomeType biome, string objectName)
        {
            if (rule == null || !TryFindSpawnPosition(biome, out Vector3 position)) return null;
            EnemyController enemy = EnemyController.Acquire(transform, objectName, EnemyController.GetPoolArchetypeId(rule));
            enemy.Configure(null, rule, position, position);
            return enemy;
        }

        private bool TryFindSpawnPosition(BiomeType sourceBiome, out Vector3 position)
        {
            FinalBossPillar sourcePillar = pillars?.FirstOrDefault(pillar => pillar != null && pillar.Biome == sourceBiome);
            for (int i = 0; i < 24; i++)
            {
                Vector2 uv = new Vector2(Random.Range(.22f, .78f), Random.Range(.24f, .73f));
                Vector3 candidate = arena.UVToWorld(uv);
                if (sourcePillar != null && i < 16)
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    float distance = Random.Range(3.5f, 6.5f);
                    candidate = sourcePillar.transform.position
                        + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
                }
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
            if (PlayerController.Instance != null)
            {
                Vector3 delta = position - PlayerController.Instance.transform.position;
                delta.y = 0f;
                if (delta.sqrMagnitude < 20f) return false;
            }
            return arena.IsWalkable(position, new Vector2(.55f, .42f));
        }

        public void NotifyPillarDestroyed(FinalBossPillar pillar)
        {
            if (pillar == null || CurrentPhase != 1) return;
            if (!disabledBiomes.Add(pillar.Biome)) return;
            sealGauge?.SetSealProgress(DestroyedPillarCount, 4);
            DontStarveCamera.Instance?.AddCombatImpulse(.16f, .22f);
            nextSpawnTime = Mathf.Max(nextSpawnTime, Time.time + 2f);
            if (DestroyedPillarCount >= 4)
            {
                BeginPhaseTwo();
                return;
            }
            AudioManager.Instance?.PlaySFX("BossPhaseChange", .45f);
            SpawnWeakenedMidBoss(pillar.Biome);
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
            if (pillars != null)
                foreach (FinalBossPillar pillar in pillars)
                    pillar?.HideBrokenRemnant();
            if (sealGauge != null)
            {
                sealGauge.Hide();
                Destroy(sealGauge.gameObject);
            }
            ReleasePhaseOneCombatants();
            if (bossVisual != null) bossVisual.localScale = bossRestScale;
            SetBossPresentation(false);
            FinalBossPhaseTwoController phaseTwo = GetComponent<FinalBossPhaseTwoController>();
            if (phaseTwo == null) phaseTwo = gameObject.AddComponent<FinalBossPhaseTwoController>();
            phaseTwo.Begin(bossVisual);
            Debug.Log("[FinalBoss] Four biome pillars destroyed. Phase two unlocked.");
        }

        public void NotifyPhaseThreeStarted()
        {
            if (CurrentPhase == 2) CurrentPhase = 3;
        }

        private void ReleasePhaseOneCombatants()
        {
            ReleaseTrackedMinions(commonMinions);
            foreach (List<EnemyController> active in exclusiveMinions.Values)
                ReleaseTrackedMinions(active);

            EnemyController[] remaining = EnemyController.ActiveEnemyControllers.ToArray();
            foreach (EnemyController enemy in remaining)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
                string enemyName = enemy.name;
                if (enemyName.StartsWith("FinalBoss_Common_", StringComparison.Ordinal)
                    || enemyName.Contains("_BiomeMinion_")
                    || enemyName.Contains("_WeakenedMidBoss"))
                {
                    EnemyProjectile.ReturnProjectilesOwnedBy(enemy);
                    enemy.ReleaseToPool();
                }
            }
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

        private static void ReleaseTrackedMinions(List<EnemyController> active)
        {
            foreach (EnemyController enemy in active)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    EnemyProjectile.ReturnProjectilesOwnedBy(enemy);
                    enemy.ReleaseToPool();
                }
            }
            active.Clear();
        }

#if UNITY_EDITOR
        public void ReplenishMinionsForTest()
        {
            ReplenishMinions();
        }

        public void PauseSpawningForTest()
        {
            spawningPausedForTests = true;
            ReleaseTrackedMinions(commonMinions);
            foreach (List<EnemyController> active in exclusiveMinions.Values)
                ReleaseTrackedMinions(active);
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
