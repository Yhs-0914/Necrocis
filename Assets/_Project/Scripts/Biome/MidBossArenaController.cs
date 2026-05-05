using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 맵 중앙 중간보스 구역을 관리한다.
    /// 안개 벽 시각효과, 진입 후 봉쇄, 보스 처치 후 해제를 담당한다.
    /// </summary>
    [DisallowMultipleComponent]
    public class MidBossArenaController : MonoBehaviour
    {
        private const float DefaultMidBossMinimumMaxHealth = 250f;
        private const float IntestineMidBossMinimumMaxHealth = 500f;

        private static Sprite fogSprite;
        private static readonly List<MidBossArenaController> ActiveArenas = new List<MidBossArenaController>();

        private readonly List<SpriteRenderer> fogRenderers = new List<SpriteRenderer>();
        private readonly List<Vector2Int> blockedBoundaryCells = new List<Vector2Int>();

        private BiomeManager biome;
        private MidBossArenaConfig arenaConfig;
        private EnemySpawnRuleConfig bossRule;
        private EnemyController activeBoss;
        private Vector2Int centerGrid;
        private Vector2Int arenaSize;
        private bool arenaLocked;
        private bool bossDefeated;

        public bool IsLocked => arenaLocked;

        public void Configure(BiomeManager biome, MidBossArenaConfig arenaConfig, IList<EnemySpawnRuleConfig> availableEnemyRules)
        {
            this.biome = biome;
            this.arenaConfig = arenaConfig;
            bossRule = ResolveBossRule(availableEnemyRules);

            centerGrid = ResolveCenterGrid();
            arenaSize = new Vector2Int(
                Mathf.Max(8, arenaConfig.arenaSize.x),
                Mathf.Max(8, arenaConfig.arenaSize.y));

            transform.position = biome.GridToWorldWithHeight(centerGrid.x, centerGrid.y);
            transform.name = "MidBossArena";

            BuildBoundaryCellCache();
            BuildTrigger();
            BuildFogWalls();
            SpawnBoss();
            ApplyFogVisualState();
        }

        private void OnEnable()
        {
            if (!ActiveArenas.Contains(this))
            {
                ActiveArenas.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveArenas.Remove(this);
        }

        private void Update()
        {
            AnimateFog();

            if (!arenaLocked || bossDefeated)
            {
                return;
            }

            if (activeBoss == null || activeBoss.IsDead || !activeBoss.gameObject.activeInHierarchy)
            {
                UnlockArena();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (arenaLocked || bossDefeated)
            {
                return;
            }

            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null)
            {
                player = other.GetComponentInParent<PlayerController>();
            }

            if (player == null)
            {
                return;
            }

            TryActivateArena();
        }

        private void OnDestroy()
        {
            ActiveArenas.Remove(this);

            if (activeBoss != null)
            {
                activeBoss.Defeated -= HandleBossDefeated;
            }

            if (arenaLocked && biome != null)
            {
                biome.RemoveRuntimeBlockedCells(blockedBoundaryCells);
            }
        }

        private void TryActivateArena()
        {
            if (biome == null || bossRule == null)
            {
                Debug.LogWarning("[MidBossArena] 보스 룰이 없어 아레나를 활성화할 수 없습니다.");
                return;
            }

            if (activeBoss == null || activeBoss.IsDead || !activeBoss.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[MidBossArena] 중간보스가 없거나 이미 처치되어 봉쇄를 시작하지 않습니다.");
                return;
            }

            arenaLocked = true;
            biome.AddRuntimeBlockedCells(blockedBoundaryCells);
            ApplyFogVisualState();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameState(GameState.InBossRoom);
            }

            Debug.Log($"[MidBossArena] 중간보스 구역 진입 - 탈출 차단 활성화 ({biome.BiomeType})");
        }

        private void SpawnBoss()
        {
            if (activeBoss != null && activeBoss.gameObject.activeInHierarchy)
            {
                return;
            }

            if (bossRule == null)
            {
                return;
            }

            Vector3 bossSpawnPosition = biome.GridToWorldWithHeight(centerGrid.x, centerGrid.y, bossRule.heightOffset);
            int poolArchetypeId = EnemyController.GetPoolArchetypeId(bossRule);
            activeBoss = EnemyController.Acquire(transform, $"{bossRule.name}_MidBoss", poolArchetypeId);
            activeBoss.Configure(null, bossRule, bossSpawnPosition, bossSpawnPosition);
            activeBoss.transform.SetParent(transform, true);
            activeBoss.SetIgnoreMidBossArenaRestriction(true);
            activeBoss.Defeated -= HandleBossDefeated;
            activeBoss.Defeated += HandleBossDefeated;
            ConfigureBiomeSpecificBossPattern(activeBoss, bossSpawnPosition);

            Debug.Log($"[MidBossArena] 중간보스 스폰: {bossRule.name} @ {bossSpawnPosition}");
        }

        private void ConfigureBiomeSpecificBossPattern(EnemyController boss, Vector3 bossSpawnPosition)
        {
            if (boss == null || biome == null)
            {
                return;
            }

            IntestineBossPattern intestinePattern = boss.GetComponent<IntestineBossPattern>();
            MidBossPatternType patternType = ResolveBossPatternType();
            if (patternType == MidBossPatternType.Intestine)
            {
                if (intestinePattern == null)
                {
                    intestinePattern = boss.gameObject.AddComponent<IntestineBossPattern>();
                }

                intestinePattern.Initialize(boss, bossSpawnPosition, transform, arenaConfig?.boss?.intestinePattern);
                return;
            }

            if (intestinePattern != null)
            {
                intestinePattern.enabled = false;
            }

            boss.SetAiSuppressed(false);
        }

        private MidBossPatternType ResolveBossPatternType()
        {
            MidBossPatternType configuredType = arenaConfig != null && arenaConfig.boss != null
                ? arenaConfig.boss.patternType
                : MidBossPatternType.Auto;

            if (configuredType != MidBossPatternType.Auto)
            {
                return configuredType;
            }

            return biome != null && biome.BiomeType == BiomeType.Intestine
                ? MidBossPatternType.Intestine
                : MidBossPatternType.None;
        }

        private void UnlockArena()
        {
            if (!arenaLocked)
            {
                return;
            }

            arenaLocked = false;
            bossDefeated = true;

            if (biome != null)
            {
                biome.RemoveRuntimeBlockedCells(blockedBoundaryCells);
            }

            if (activeBoss != null)
            {
                activeBoss.Defeated -= HandleBossDefeated;
                activeBoss = null;
            }

            ApplyFogVisualState();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameState(GameState.InBiome);
            }

            Debug.Log("[MidBossArena] 중간보스 처치 - 봉쇄 해제");
        }

        private void BuildTrigger()
        {
            BoxCollider trigger = GetComponent<BoxCollider>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<BoxCollider>();
            }

            int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
            float innerWidth = Mathf.Max(biome.TileSize, (arenaSize.x - thickness * 2) * biome.TileSize);
            float innerDepth = Mathf.Max(biome.TileSize, (arenaSize.y - thickness * 2) * biome.TileSize);

            trigger.isTrigger = true;
            trigger.size = new Vector3(innerWidth, arenaConfig.triggerHeight, innerDepth);
            trigger.center = new Vector3(0f, arenaConfig.wallHeightOffset, 0f);
        }

        private void BuildFogWalls()
        {
            fogRenderers.Clear();

            Vector3 worldCenter = biome.GridToWorld(centerGrid.x, centerGrid.y);
            float widthWorld = arenaSize.x * biome.TileSize;
            float depthWorld = arenaSize.y * biome.TileSize;
            float thicknessWorld = Mathf.Max(1, arenaConfig.wallThicknessInCells) * biome.TileSize;
            float halfWidth = widthWorld * 0.5f;
            float halfDepth = depthWorld * 0.5f;
            float wallOffsetX = halfWidth - thicknessWorld * 0.5f;
            float wallOffsetZ = halfDepth - thicknessWorld * 0.5f;

            CreateFogWall(
                "NorthFogWall",
                worldCenter + new Vector3(0f, 0f, wallOffsetZ),
                new Vector2(widthWorld, thicknessWorld));

            CreateFogWall(
                "SouthFogWall",
                worldCenter + new Vector3(0f, 0f, -wallOffsetZ),
                new Vector2(widthWorld, thicknessWorld));

            CreateFogWall(
                "EastFogWall",
                worldCenter + new Vector3(wallOffsetX, 0f, 0f),
                new Vector2(thicknessWorld, depthWorld));

            CreateFogWall(
                "WestFogWall",
                worldCenter + new Vector3(-wallOffsetX, 0f, 0f),
                new Vector2(thicknessWorld, depthWorld));
        }

        private void CreateFogWall(string name, Vector3 position, Vector2 size)
        {
            GameObject wall = new GameObject(name);
            wall.transform.SetParent(transform, false);
            position.y = biome.GetGroundHeight(position) + arenaConfig.groundFogOffset;
            wall.transform.position = position;
            wall.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            wall.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
            renderer.sprite = GetFogSprite();
            renderer.sortingOrder = arenaConfig.sortingOrder;
            renderer.color = arenaLocked ? arenaConfig.lockedFogColor : arenaConfig.unlockedFogColor;
            fogRenderers.Add(renderer);
        }

        private void HandleBossDefeated(EnemyController boss)
        {
            if (boss == null || boss != activeBoss || bossDefeated)
            {
                return;
            }

            UnlockArena();
        }

        public bool ContainsWorldPosition(Vector3 worldPosition)
        {
            Vector3 centerWorld = biome.GridToWorld(centerGrid.x, centerGrid.y);
            float halfWidth = arenaSize.x * biome.TileSize * 0.5f;
            float halfDepth = arenaSize.y * biome.TileSize * 0.5f;
            return worldPosition.x >= centerWorld.x - halfWidth
                && worldPosition.x <= centerWorld.x + halfWidth
                && worldPosition.z >= centerWorld.z - halfDepth
                && worldPosition.z <= centerWorld.z + halfDepth;
        }

        public static bool IsPlayerInsideLockedArena(Vector3 playerPosition)
        {
            for (int i = 0; i < ActiveArenas.Count; i++)
            {
                MidBossArenaController arena = ActiveArenas[i];
                if (arena == null || !arena.IsLocked)
                {
                    continue;
                }

                if (arena.ContainsWorldPosition(playerPosition))
                {
                    return true;
                }
            }

            return false;
        }

        private void BuildBoundaryCellCache()
        {
            blockedBoundaryCells.Clear();

            int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
            int minX = centerGrid.x - arenaSize.x / 2;
            int minY = centerGrid.y - arenaSize.y / 2;
            int maxX = minX + arenaSize.x - 1;
            int maxY = minY + arenaSize.y - 1;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    bool isBoundary = x < minX + thickness
                        || x > maxX - thickness
                        || y < minY + thickness
                        || y > maxY - thickness;

                    if (!isBoundary || !biome.IsValidPosition(x, y))
                    {
                        continue;
                    }

                    blockedBoundaryCells.Add(new Vector2Int(x, y));
                }
            }
        }

        private Vector2Int ResolveCenterGrid()
        {
            if (arenaConfig.useCustomCenter)
            {
                return arenaConfig.centerGrid;
            }

            return new Vector2Int(biome.MapWidth / 2, biome.MapHeight / 2);
        }

        private EnemySpawnRuleConfig ResolveBossRule(IList<EnemySpawnRuleConfig> availableEnemyRules)
        {
            if (arenaConfig == null)
            {
                return null;
            }

            MidBossDefinition bossDefinition = arenaConfig.boss;
            if (bossDefinition != null && bossDefinition.useCustomBossRule && bossDefinition.bossRule != null)
            {
                EnemySpawnRuleConfig customBossRule = BuildBossRule(bossDefinition.bossRule, bossDefinition);
                if (HasRenderableSprite(customBossRule))
                {
                    return customBossRule;
                }

                Debug.LogWarning("[MidBossArena] 커스텀 보스 룰에 스프라이트가 없어 적 fallback 룰을 사용합니다.");
            }

            if (bossDefinition != null
                && bossDefinition.useEnemyRuleFallback
                && availableEnemyRules != null
                && availableEnemyRules.Count > 0)
            {
                int index = Mathf.Clamp(bossDefinition.fallbackEnemyRuleIndex, 0, availableEnemyRules.Count - 1);
                EnemySpawnRuleConfig fallbackRule = availableEnemyRules[index];
                if (fallbackRule != null)
                {
                    return BuildBossRule(fallbackRule, bossDefinition);
                }
            }

            return null;
        }

        private EnemySpawnRuleConfig BuildBossRule(EnemySpawnRuleConfig source, MidBossDefinition bossDefinition)
        {
            if (source == null)
            {
                return null;
            }

            EnemySpawnRuleConfig boss = new EnemySpawnRuleConfig
            {
                name = string.IsNullOrWhiteSpace(bossDefinition?.displayName) ? source.name : bossDefinition.displayName,
                density = source.density,
                minDistance = source.minDistance,
                poissonSalt = source.poissonSalt,
                allowedRegions = source.allowedRegions != null ? new List<int>(source.allowedRegions) : new List<int>(),
                maxAlive = 1,
                activationRadius = 0f,
                respawnCooldown = 0f,
                spawnRadius = 0f,
                moveSpeed = source.moveSpeed,
                stoppingDistance = source.stoppingDistance,
                wanderRadius = source.wanderRadius,
                chaseRadius = source.chaseRadius,
                leashRadius = source.leashRadius,
                idleDelayRange = source.idleDelayRange,
                maxHealth = source.maxHealth,
                attackDamage = source.attackDamage,
                attackRange = source.attackRange,
                attackCooldown = source.attackCooldown,
                expReward = source.expReward,
                additionalBaseStats = source.additionalBaseStats != null ? new List<CharacterStatValue>(source.additionalBaseStats) : new List<CharacterStatValue>(),
                separationDistance = 0f,
                separationStrength = 0f,
                heightOffset = source.heightOffset,
                scale = source.scale,
                sortingOrder = source.sortingOrder,
                useBillboard = source.useBillboard,
                useYSort = source.useYSort,
                animationSpeed = source.animationSpeed,
                addCollider = source.addCollider,
                isTrigger = source.isTrigger,
                colliderSize = source.colliderSize,
                colliderCenter = source.colliderCenter,
                idleSprites = source.idleSprites,
                moveSprites = source.moveSprites,
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
                isElite = source.isElite,
                tintColor = source.tintColor,
                killTriggerEnemyName = source.killTriggerEnemyName,
                killTriggerCount = source.killTriggerCount,
                splitsOnDeath = source.splitsOnDeath,
                splitCount = source.splitCount,
                splitEnemyName = source.splitEnemyName,
                splitVfxSprites = source.splitVfxSprites,
                splitVfxScale = source.splitVfxScale,
                splitVfxSpeed = source.splitVfxSpeed,
                splitVfxDuration = source.splitVfxDuration,
                chargesAtPlayer = source.chargesAtPlayer,
                chargeSpeed = source.chargeSpeed,
                chargeAccelTime = source.chargeAccelTime,
                leavesDebrisOnDeath = source.leavesDebrisOnDeath,
                debrisDuration = source.debrisDuration,
                debrisAggroRadius = source.debrisAggroRadius,
                debrisVfxSprites = source.debrisVfxSprites,
                debrisVfxScale = source.debrisVfxScale,
                debrisVfxSpeed = source.debrisVfxSpeed
            };

            if (bossDefinition != null && bossDefinition.overrideStats)
            {
                boss.maxHealth *= Mathf.Max(0.01f, bossDefinition.maxHealthMultiplier);
                boss.attackDamage *= Mathf.Max(0.01f, bossDefinition.attackDamageMultiplier);
                boss.moveSpeed *= Mathf.Max(0.01f, bossDefinition.moveSpeedMultiplier);
            }

            if (bossDefinition != null)
            {
                boss.scale = Vector3.Scale(boss.scale, GetSafeScaleMultiplier(bossDefinition.scaleMultiplier));
            }

            boss.maxHealth = Mathf.Max(boss.maxHealth, GetMinimumBossMaxHealth(bossDefinition));

            return boss;
        }

        private static Vector3 GetSafeScaleMultiplier(Vector3 scaleMultiplier)
        {
            return new Vector3(
                Mathf.Approximately(scaleMultiplier.x, 0f) ? 1f : Mathf.Max(0.01f, scaleMultiplier.x),
                Mathf.Approximately(scaleMultiplier.y, 0f) ? 1f : Mathf.Max(0.01f, scaleMultiplier.y),
                Mathf.Approximately(scaleMultiplier.z, 0f) ? 1f : Mathf.Max(0.01f, scaleMultiplier.z));
        }

        private static bool HasRenderableSprite(EnemySpawnRuleConfig rule)
        {
            return rule != null
                && ((rule.idleSprites != null && rule.idleSprites.Length > 0)
                    || (rule.moveSprites != null && rule.moveSprites.Length > 0)
                    || (rule.attackSprites != null && rule.attackSprites.Length > 0)
                    || (rule.deathSprites != null && rule.deathSprites.Length > 0));
        }

        private float GetMinimumBossMaxHealth(MidBossDefinition bossDefinition)
        {
            if (bossDefinition != null && bossDefinition.minimumMaxHealth > 0f)
            {
                return bossDefinition.minimumMaxHealth;
            }

            return biome != null && biome.BiomeType == BiomeType.Intestine
                ? IntestineMidBossMinimumMaxHealth
                : DefaultMidBossMinimumMaxHealth;
        }

        private void ApplyFogVisualState()
        {
            Color baseColor = arenaLocked ? arenaConfig.lockedFogColor : arenaConfig.unlockedFogColor;
            for (int i = 0; i < fogRenderers.Count; i++)
            {
                if (fogRenderers[i] != null)
                {
                    fogRenderers[i].color = baseColor;
                }
            }
        }

        private void AnimateFog()
        {
            Color baseColor = arenaLocked ? arenaConfig.lockedFogColor : arenaConfig.unlockedFogColor;
            float baseAlpha = baseColor.a;
            for (int i = 0; i < fogRenderers.Count; i++)
            {
                SpriteRenderer renderer = fogRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                float pulse = 0.82f + Mathf.Sin(Time.time * 1.8f + i * 0.65f) * 0.12f;
                Color animatedColor = baseColor;
                animatedColor.a = Mathf.Clamp01(baseAlpha * pulse);
                renderer.color = animatedColor;
            }
        }

        private static Sprite GetFogSprite()
        {
            if (fogSprite != null)
            {
                return fogSprite;
            }

            fogSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            fogSprite.name = "MidBossFogSprite";
            return fogSprite;
        }
    }
}
