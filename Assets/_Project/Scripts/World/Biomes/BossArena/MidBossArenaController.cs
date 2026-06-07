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
        private static Sprite fogSprite;
        private static Sprite runtimeBossSprite;
        private static readonly List<MidBossArenaController> ActiveArenas = new List<MidBossArenaController>();

        private readonly List<SpriteRenderer> fogRenderers = new List<SpriteRenderer>();
        private readonly List<Vector2Int> blockedBoundaryCells = new List<Vector2Int>();
        private SpriteRenderer interiorFogRenderer;

        private BiomeManager biome;
        private MidBossArenaConfig arenaConfig;
        private BiomeReturnPortalConfig returnPortalConfig;
        private EnemySpawnRuleConfig bossRule;

        private EnemyController activeBoss;
        private IntestineBossPattern activeIntestinePattern;
        private LiverBossPattern activeLiverPattern;
        private StomachBossPattern activeStomachPattern;
        private LungBossPattern activeLungPattern;
        private readonly List<MidBossContactDamage> activeContactDamage = new List<MidBossContactDamage>();
        private Vector2Int centerGrid;
        private Vector2Int arenaSize;
        private bool arenaLocked;
        private bool bossDefeated;
        private float fogRevealAmount;

        public bool IsLocked => arenaLocked;

        public void Configure(
            BiomeManager biome,
            MidBossArenaConfig arenaConfig,
            IList<EnemySpawnRuleConfig> availableEnemyRules,
            BiomeReturnPortalConfig returnPortalConfig = null)
        {
            this.biome = biome;
            this.arenaConfig = arenaConfig;
            this.returnPortalConfig = returnPortalConfig;
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
            UpdateFogReveal(Time.deltaTime);

            if (!arenaLocked || bossDefeated)
                return;

            EnforcePlayerInsidePlayableBounds();

            if (activeLungPattern != null)
            {
                if (activeLungPattern.IsEncounterDefeated)
                {
                    UnlockArena();
                }

                return;
            }

            if (activeBoss == null || activeBoss.IsDead || !activeBoss.gameObject.activeInHierarchy)
            {
                UnlockArena();
            }
        }

        private void LateUpdate()
        {
            if (!arenaLocked || bossDefeated)
            {
                return;
            }

            EnforcePlayerInsidePlayableBounds();
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
                activeBoss.Defeated -= HandleBossDefeated;

            if (arenaLocked && biome != null)
            {
                biome.RemoveRuntimeBlockedCells(blockedBoundaryCells);
            }
        }

        private void TryActivateArena()
        {
            if (biome == null)
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
            RecenterBossEncounter();
            SetBossEncounterActive(true);

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
            RegisterBossContactDamage(activeBoss);
            SetBossEncounterActive(false);
            RecenterBossEncounter();

            Debug.Log($"[MidBossArena] 중간보스 스폰: {bossRule.name} @ {bossSpawnPosition}");
        }

        private void ConfigureBiomeSpecificBossPattern(EnemyController boss, Vector3 bossSpawnPosition)
        {
            if (boss == null || biome == null)
            {
                return;
            }

            MidBossPatternType patternType = ResolveBossPatternType();
            IntestineBossPattern intestinePattern = boss.GetComponent<IntestineBossPattern>();
            LiverBossPattern liverPattern = boss.GetComponent<LiverBossPattern>();
            StomachBossPattern stomachPattern = boss.GetComponent<StomachBossPattern>();
            LungBossPattern lungPattern = GetComponent<LungBossPattern>();
            activeIntestinePattern = null;
            activeLiverPattern = null;
            activeStomachPattern = null;
            activeLungPattern = null;

            if (patternType == MidBossPatternType.Intestine)
            {
                if (liverPattern != null)
                {
                    liverPattern.enabled = false;
                }

                if (stomachPattern != null)
                {
                    stomachPattern.enabled = false;
                }

                if (lungPattern != null)
                {
                    lungPattern.enabled = false;
                }

                if (intestinePattern == null)
                {
                    intestinePattern = boss.gameObject.AddComponent<IntestineBossPattern>();
                }

                intestinePattern.Initialize(boss, bossSpawnPosition, transform, arenaConfig?.boss?.intestinePattern);
                intestinePattern.SetEncounterActive(false);
                activeIntestinePattern = intestinePattern;
                return;
            }

            if (patternType == MidBossPatternType.Liver)
            {
                if (intestinePattern != null)
                {
                    intestinePattern.enabled = false;
                }

                if (stomachPattern != null)
                {
                    stomachPattern.enabled = false;
                }

                if (lungPattern != null)
                {
                    lungPattern.enabled = false;
                }

                if (liverPattern == null)
                {
                    liverPattern = boss.gameObject.AddComponent<LiverBossPattern>();
                }

                liverPattern.Initialize(boss, bossSpawnPosition, transform, arenaConfig?.boss?.liverPattern);
                liverPattern.SetEncounterActive(false);
                activeLiverPattern = liverPattern;
                return;
            }

            if (patternType == MidBossPatternType.Stomach)
            {
                if (intestinePattern != null)
                {
                    intestinePattern.enabled = false;
                }

                if (liverPattern != null)
                {
                    liverPattern.enabled = false;
                }

                if (lungPattern != null)
                {
                    lungPattern.enabled = false;
                }

                if (stomachPattern == null)
                {
                    stomachPattern = boss.gameObject.AddComponent<StomachBossPattern>();
                }

                stomachPattern.Initialize(boss, bossSpawnPosition, transform, arenaConfig?.boss?.stomachPattern);
                stomachPattern.SetEncounterActive(false);
                activeStomachPattern = stomachPattern;
                return;
            }

            if (patternType == MidBossPatternType.Lung)
            {
                if (intestinePattern != null)
                {
                    intestinePattern.enabled = false;
                }

                if (liverPattern != null)
                {
                    liverPattern.enabled = false;
                }

                if (stomachPattern != null)
                {
                    stomachPattern.enabled = false;
                }

                if (lungPattern == null)
                {
                    lungPattern = gameObject.AddComponent<LungBossPattern>();
                }

                activeLungPattern = lungPattern;
                lungPattern.Initialize(boss, bossSpawnPosition, transform, arenaConfig?.boss?.lungPattern);
                lungPattern.SetEncounterActive(false);
                lungPattern.ForEachEncounterBoss(RegisterBossContactDamage);
                return;
            }

            if (intestinePattern != null)
            {
                intestinePattern.enabled = false;
            }

            if (liverPattern != null)
            {
                liverPattern.enabled = false;
            }

            if (stomachPattern != null)
            {
                stomachPattern.enabled = false;
            }

            if (lungPattern != null)
            {
                lungPattern.enabled = false;
            }

            boss.SetAiSuppressed(true);
        }

        private void SetBossEncounterActive(bool active)
        {
            bool hasPattern = activeIntestinePattern != null
                || activeLiverPattern != null
                || activeStomachPattern != null
                || activeLungPattern != null;

            if (activeBoss != null && !activeBoss.IsDead)
            {
                activeBoss.SetAiSuppressed(!active || hasPattern);
            }

            activeIntestinePattern?.SetEncounterActive(active);
            activeLiverPattern?.SetEncounterActive(active);
            activeStomachPattern?.SetEncounterActive(active);
            activeLungPattern?.SetEncounterActive(active);

            for (int i = 0; i < activeContactDamage.Count; i++)
            {
                if (activeContactDamage[i] != null)
                {
                    activeContactDamage[i].SetDamageActive(active && !bossDefeated);
                }
            }
        }

        private void RecenterBossEncounter()
        {
            if (activeBoss == null || biome == null || bossRule == null)
            {
                return;
            }

            Vector3 center = biome.GridToWorldWithHeight(centerGrid.x, centerGrid.y, bossRule.heightOffset);
            if (activeLungPattern != null)
            {
                activeLungPattern.RecenterEncounter();
                return;
            }

            activeBoss.transform.position = center;
        }

        private void RegisterBossContactDamage(EnemyController boss)
        {
            if (boss == null)
            {
                return;
            }

            MidBossContactDamage contactDamage = boss.GetComponent<MidBossContactDamage>();
            if (contactDamage == null)
            {
                contactDamage = boss.gameObject.AddComponent<MidBossContactDamage>();
            }

            if (arenaConfig == null)
            {
                return;
            }

            contactDamage.Initialize(
                boss,
                arenaConfig.bossContactDamage,
                arenaConfig.bossContactDamageCooldown,
                arenaConfig.bossContactPushSpeed);
            contactDamage.SetDamageActive(arenaLocked && !bossDefeated);

            if (!activeContactDamage.Contains(contactDamage))
            {
                activeContactDamage.Add(contactDamage);
            }

            Collider bossCollider = boss.GetComponent<Collider>();
            if (bossCollider != null)
            {
                bossCollider.isTrigger = true;
            }
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

            if (biome == null)
            {
                return MidBossPatternType.None;
            }

            return biome.BiomeType switch
            {
                BiomeType.Intestine => MidBossPatternType.Intestine,
                BiomeType.Liver => MidBossPatternType.Liver,
                BiomeType.Stomach => MidBossPatternType.Stomach,
                BiomeType.Lung => MidBossPatternType.Lung,
                _ => MidBossPatternType.None
            };
        }

        private void UnlockArena()
        {
            if (!arenaLocked)
            {
                return;
            }

            arenaLocked = false;
            bossDefeated = true;
            SetBossEncounterActive(false);

            if (biome != null)
            {
                biome.RemoveRuntimeBlockedCells(blockedBoundaryCells);
            }

            Vector3 returnPortalPosition = ResolveReturnPortalPosition();
            Vector3 bossDeathPos = activeBoss != null
                ? activeBoss.transform.position
                : returnPortalPosition;
            EnemyController defeatedBoss = activeBoss;

            if (defeatedBoss != null)
            {
                EnemyProjectile.ReturnProjectilesOwnedBy(defeatedBoss);
                defeatedBoss.Defeated -= HandleBossDefeated;
                activeBoss = null;
            }

            if (activeLungPattern != null)
            {
                activeLungPattern.DisposeEncounter();
                activeLungPattern = null;
            }

            ApplyFogVisualState();

            if (GameManager.Instance != null)
            {
                if (biome != null)
                {
                    GameManager.Instance.CollectRelic(biome.BiomeType);
                }

                GameManager.Instance.SetGameState(GameState.InBiome);
            }

            SpawnReturnPortal(returnPortalPosition);
            SpawnBonusItemDrop(bossDeathPos);

            Debug.Log("[MidBossArena] 중간보스 처치 - 봉쇄 해제, 귀환 포탈 생성");
        }

        private Vector3 ResolveReturnPortalPosition()
        {
            if (biome == null)
            {
                return transform.position;
            }

            float heightOffset = returnPortalConfig != null ? returnPortalConfig.heightOffset : 0f;
            return biome.GridToWorldWithHeight(centerGrid.x, centerGrid.y, heightOffset);
        }

        private void SpawnReturnPortal(Vector3 portalPos)
        {
            if (returnPortalConfig != null && !returnPortalConfig.enabled)
            {
                return;
            }

            string portalName = returnPortalConfig != null && !string.IsNullOrWhiteSpace(returnPortalConfig.name)
                ? returnPortalConfig.name
                : "BossReturnPortal";
            GameObject portalObj = new GameObject(portalName);
            portalObj.transform.SetParent(transform, true);
            portalObj.transform.position = portalPos;
            portalObj.transform.localScale = returnPortalConfig != null
                ? GetSafeScaleMultiplier(returnPortalConfig.scale)
                : Vector3.one;

            SpriteRenderer sr = portalObj.AddComponent<SpriteRenderer>();
            bool hasConfiguredSprite = returnPortalConfig != null && returnPortalConfig.sprite != null;
            bool hasArenaSprite = !hasConfiguredSprite && arenaConfig != null && arenaConfig.returnPortalSprite != null;
            sr.sprite = hasConfiguredSprite
                ? returnPortalConfig.sprite
                : hasArenaSprite ? arenaConfig.returnPortalSprite : GetFogSprite();
            sr.color = hasConfiguredSprite || hasArenaSprite
                ? Color.white
                : new Color(0.6f, 0.2f, 1f, 0.85f);
            sr.sortingOrder = returnPortalConfig != null
                ? returnPortalConfig.sortingOrder
                : arenaConfig != null ? arenaConfig.sortingOrder : 3500;

            if (returnPortalConfig == null && hasArenaSprite)
            {
                portalObj.transform.localScale = GetSafeScaleMultiplier(arenaConfig.returnPortalScale);
            }

            if (returnPortalConfig == null || returnPortalConfig.useBillboard)
            {
                Billboard billboard = portalObj.AddComponent<Billboard>();
                billboard.SetUpdateMode(Billboard.UpdateMode.Continuous);
            }

            SpriteYSort ySort = portalObj.AddComponent<SpriteYSort>();
            ySort.Configure(SpriteYSort.WorldDynamicBaseSortingOrder, true, SpriteYSort.WorldDynamicMinSortingOrder);
            ySort.SetUpdateMode(SpriteYSort.UpdateMode.Continuous);

            if (returnPortalConfig == null || returnPortalConfig.addCollider)
            {
                BoxCollider col = portalObj.AddComponent<BoxCollider>();
                col.isTrigger = returnPortalConfig == null || returnPortalConfig.isTrigger;
                col.size = GetSafeColliderSize(returnPortalConfig != null ? returnPortalConfig.colliderSize : new Vector3(2f, 2f, 2f));
                col.center = returnPortalConfig != null ? returnPortalConfig.colliderCenter : Vector3.zero;
            }

            ReturnPortal portal = portalObj.AddComponent<ReturnPortal>();
            portal.SetActive(true);
        }

        private void SpawnBonusItemDrop(Vector3 bossDeathPos)
        {
            if (biome == null)
            {
                return;
            }

            WorldItemSpawner spawner = biome.GetComponent<WorldItemSpawner>();
            if (spawner == null)
            {
                spawner = biome.gameObject.AddComponent<WorldItemSpawner>();
            }

            bool spawnedItem = spawner.TrySpawnSingleRandomItemAt(bossDeathPos);
            if (!spawnedItem)
            {
                Debug.Log("[MidBossArena] 보스 보너스 아이템 드랍 위치를 찾지 못했습니다.");
            }
        }

        private void BuildTrigger()
        {
            BoxCollider trigger = GetComponent<BoxCollider>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<BoxCollider>();
            }

            int triggerInset = GetTriggerInsetCells();
            float innerWidth = Mathf.Max(biome.TileSize, (arenaSize.x - triggerInset * 2) * biome.TileSize);
            float innerDepth = Mathf.Max(biome.TileSize, (arenaSize.y - triggerInset * 2) * biome.TileSize);

            trigger.isTrigger = true;
            trigger.size = new Vector3(innerWidth, arenaConfig.triggerHeight, innerDepth);
            trigger.center = new Vector3(0f, arenaConfig.wallHeightOffset, 0f);
        }

        private void BuildFogWalls()
        {
            fogRenderers.Clear();
            interiorFogRenderer = null;

            Vector3 worldCenter = biome.GridToWorld(centerGrid.x, centerGrid.y);
            float widthWorld = arenaSize.x * biome.TileSize;
            float depthWorld = arenaSize.y * biome.TileSize;
            float thicknessWorld = Mathf.Max(1, arenaConfig.wallThicknessInCells) * biome.TileSize;
            float halfWidth = widthWorld * 0.5f;
            float halfDepth = depthWorld * 0.5f;
            float wallOffsetX = halfWidth - thicknessWorld * 0.5f;
            float wallOffsetZ = halfDepth - thicknessWorld * 0.5f;

            if (arenaConfig.useInteriorFogCover)
            {
                CreateInteriorFogCover(worldCenter, new Vector2(widthWorld, depthWorld));
            }

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

            SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
            renderer.sprite = GetFogSprite();
            ApplyFogWorldSize(wall.transform, renderer.sprite, size);
            renderer.sortingOrder = arenaConfig.sortingOrder;
            renderer.color = arenaLocked ? arenaConfig.lockedFogColor : arenaConfig.unlockedFogColor;
            fogRenderers.Add(renderer);
        }

        private void CreateInteriorFogCover(Vector3 position, Vector2 size)
        {
            GameObject cover = new GameObject("InteriorFogCover");
            cover.transform.SetParent(transform, false);
            position.y = biome.GetGroundHeight(position) + arenaConfig.groundFogOffset + 0.02f;
            cover.transform.position = position;
            cover.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            interiorFogRenderer = cover.AddComponent<SpriteRenderer>();
            interiorFogRenderer.sprite = GetFogSprite();
            ApplyFogWorldSize(cover.transform, interiorFogRenderer.sprite, size);
            interiorFogRenderer.sortingOrder = arenaConfig.sortingOrder + arenaConfig.interiorFogSortingOrderOffset;
        }

        private static void ApplyFogWorldSize(Transform target, Sprite sprite, Vector2 worldSize)
        {
            if (target == null)
            {
                return;
            }

            if (sprite == null)
            {
                target.localScale = new Vector3(worldSize.x, worldSize.y, 1f);
                return;
            }

            Vector3 spriteSize = sprite.bounds.size;
            float scaleX = spriteSize.x > 0.0001f ? worldSize.x / spriteSize.x : worldSize.x;
            float scaleY = spriteSize.y > 0.0001f ? worldSize.y / spriteSize.y : worldSize.y;
            target.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        private void HandleBossDefeated(EnemyController boss)
        {
            if (boss == null || boss != activeBoss || bossDefeated)
            {
                return;
            }

            if (activeLungPattern != null && !activeLungPattern.IsEncounterDefeated)
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

        public static bool TryClampPlayerMovementInsideLockedArena(
            Vector3 currentPosition,
            Vector3 desiredPosition,
            float margin,
            out Vector3 clampedPosition)
        {
            clampedPosition = desiredPosition;
            MidBossArenaController arena = FindLockedArenaForMovement(currentPosition, desiredPosition);
            if (arena == null)
            {
                return false;
            }

            clampedPosition = arena.ClampToPlayableBounds(desiredPosition, margin);
            return true;
        }

        public static bool TryClampPositionInsideLockedArena(Vector3 position, float margin, out Vector3 clampedPosition)
        {
            clampedPosition = position;
            MidBossArenaController arena = FindLockedArenaForPosition(position) ?? FindSingleLockedArena();
            if (arena == null)
            {
                return false;
            }

            clampedPosition = arena.ClampToPlayableBounds(position, margin);
            return true;
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

        private static MidBossArenaController FindLockedArenaForMovement(Vector3 currentPosition, Vector3 desiredPosition)
        {
            MidBossArenaController fallback = null;

            for (int i = 0; i < ActiveArenas.Count; i++)
            {
                MidBossArenaController arena = ActiveArenas[i];
                if (arena == null || !arena.IsLocked)
                {
                    continue;
                }

                if (fallback == null)
                {
                    fallback = arena;
                }

                if (arena.ContainsWorldPosition(currentPosition) || arena.ContainsWorldPosition(desiredPosition))
                {
                    return arena;
                }
            }

            return CountLockedArenas() == 1 ? fallback : null;
        }

        private static MidBossArenaController FindLockedArenaForPosition(Vector3 position)
        {
            for (int i = 0; i < ActiveArenas.Count; i++)
            {
                MidBossArenaController arena = ActiveArenas[i];
                if (arena == null || !arena.IsLocked)
                {
                    continue;
                }

                if (arena.ContainsWorldPosition(position))
                {
                    return arena;
                }
            }

            return null;
        }

        private static MidBossArenaController FindSingleLockedArena()
        {
            MidBossArenaController single = null;
            int count = 0;

            for (int i = 0; i < ActiveArenas.Count; i++)
            {
                MidBossArenaController arena = ActiveArenas[i];
                if (arena == null || !arena.IsLocked)
                {
                    continue;
                }

                single = arena;
                count++;
            }

            return count == 1 ? single : null;
        }

        private static int CountLockedArenas()
        {
            int count = 0;
            for (int i = 0; i < ActiveArenas.Count; i++)
            {
                MidBossArenaController arena = ActiveArenas[i];
                if (arena != null && arena.IsLocked)
                {
                    count++;
                }
            }

            return count;
        }

        private Vector3 ClampToPlayableBounds(Vector3 worldPosition, float margin)
        {
            if (biome == null || arenaConfig == null)
            {
                return worldPosition;
            }

            Vector3 centerWorld = biome.GridToWorld(centerGrid.x, centerGrid.y);
            float tileSize = Mathf.Max(0.01f, biome.TileSize);
            float wallPadding = (Mathf.Max(1, arenaConfig.wallThicknessInCells) + GetLockBoundaryInsetCells()) * tileSize;
            float extraMargin = Mathf.Max(0f, margin);
            float halfWidth = Mathf.Max(tileSize * 0.5f, arenaSize.x * tileSize * 0.5f - wallPadding - extraMargin);
            float halfDepth = Mathf.Max(tileSize * 0.5f, arenaSize.y * tileSize * 0.5f - wallPadding - extraMargin);

            worldPosition.x = Mathf.Clamp(worldPosition.x, centerWorld.x - halfWidth, centerWorld.x + halfWidth);
            worldPosition.z = Mathf.Clamp(worldPosition.z, centerWorld.z - halfDepth, centerWorld.z + halfDepth);
            return worldPosition;
        }

        private void EnforcePlayerInsidePlayableBounds()
        {
            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                return;
            }

            float margin = GetPlayerClampMargin(player);
            Vector3 currentPosition = player.transform.position;
            Vector3 clampedPosition = ClampToPlayableBounds(currentPosition, margin);
            Vector3 planarDelta = clampedPosition - currentPosition;
            planarDelta.y = 0f;
            if (planarDelta.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            player.SpawnAt(clampedPosition);
        }

        private static float GetPlayerClampMargin(PlayerController player)
        {
            Collider hitCollider = player != null ? player.HitCollider : null;
            if (hitCollider == null)
            {
                return 0.55f;
            }

            return Mathf.Max(0.35f, Mathf.Max(hitCollider.bounds.extents.x, hitCollider.bounds.extents.z) + 0.2f);
        }

        private void BuildBoundaryCellCache()
        {
            blockedBoundaryCells.Clear();

            int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
            int lockInset = GetLockBoundaryInsetCells();
            int minX = centerGrid.x - arenaSize.x / 2 + lockInset;
            int minY = centerGrid.y - arenaSize.y / 2 + lockInset;
            int maxX = centerGrid.x - arenaSize.x / 2 + arenaSize.x - 1 - lockInset;
            int maxY = centerGrid.y - arenaSize.y / 2 + arenaSize.y - 1 - lockInset;

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

        private int GetLockBoundaryInsetCells()
        {
            int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
            int configuredInset = Mathf.Max(0, arenaConfig.lockBoundaryInsetInCells);
            int maxInset = Mathf.Max(0, (Mathf.Min(arenaSize.x, arenaSize.y) - thickness * 2 - 2) / 2);
            return Mathf.Min(configuredInset, maxInset);
        }

        private int GetTriggerInsetCells()
        {
            int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
            int configuredInset = Mathf.Max(0, arenaConfig.triggerInsetInCells);
            int maxInset = Mathf.Max(0, (Mathf.Min(arenaSize.x, arenaSize.y) - 1) / 2);
            return Mathf.Min(thickness + configuredInset, maxInset);
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

            MidBossPatternType patternType = ResolveBossPatternType();
            if (patternType != MidBossPatternType.None)
            {
                return BuildRuntimeBossRule(bossDefinition, patternType);
            }

            return null;
        }

        private EnemySpawnRuleConfig BuildRuntimeBossRule(MidBossDefinition bossDefinition, MidBossPatternType patternType)
        {
            EnemySpawnRuleConfig source = bossDefinition?.bossRule ?? new EnemySpawnRuleConfig
            {
                name = GetDefaultRuntimeBossName(patternType)
            };

            EnemySpawnRuleConfig boss = BuildBossRule(source, bossDefinition);
            if (boss == null)
            {
                return null;
            }

            EnsureRuntimeBossSprites(boss, GetRuntimeBossSprite());
            return boss;
        }

        private static string GetDefaultRuntimeBossName(MidBossPatternType patternType)
        {
            return patternType switch
            {
                MidBossPatternType.Liver => "LiverBoss",
                MidBossPatternType.Stomach => "StomachBoss",
                MidBossPatternType.Lung => "LungBoss",
                MidBossPatternType.Intestine => "IntestineBoss",
                _ => "MidBoss"
            };
        }

        private static void EnsureRuntimeBossSprites(EnemySpawnRuleConfig boss, Sprite sprite)
        {
            if (boss == null || sprite == null)
            {
                return;
            }

            if (boss.idleSprites == null || boss.idleSprites.Length == 0)
            {
                boss.idleSprites = new[] { sprite };
            }

            if (boss.moveSprites == null || boss.moveSprites.Length == 0)
            {
                boss.moveSprites = boss.idleSprites;
            }

            if (boss.attackSprites == null || boss.attackSprites.Length == 0)
            {
                boss.attackSprites = boss.idleSprites;
            }

            if (boss.deathSprites == null || boss.deathSprites.Length == 0)
            {
                boss.deathSprites = boss.idleSprites;
            }
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
                isTrigger = true,
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
                Vector3 scaleMultiplier = GetSafeScaleMultiplier(bossDefinition.scaleMultiplier);
                boss.scale = Vector3.Scale(boss.scale, scaleMultiplier);
                ApplyBossScaleToCollision(boss, scaleMultiplier);
            }

            boss.maxHealth = Mathf.Max(boss.maxHealth, GetConfiguredMinimumBossMaxHealth(bossDefinition));

            return boss;
        }

        private static void ApplyBossScaleToCollision(EnemySpawnRuleConfig boss, Vector3 scaleMultiplier)
        {
            if (boss == null)
            {
                return;
            }

            boss.colliderSize = ScaleVector(boss.colliderSize, scaleMultiplier);
            boss.colliderCenter = ScaleVector(boss.colliderCenter, scaleMultiplier);
        }

        private static Vector3 ScaleVector(Vector3 value, Vector3 scale)
        {
            return new Vector3(value.x * scale.x, value.y * scale.y, value.z * scale.z);
        }

        private static Vector3 GetSafeScaleMultiplier(Vector3 scaleMultiplier)
        {
            return new Vector3(
                Mathf.Approximately(scaleMultiplier.x, 0f) ? 1f : Mathf.Max(0.01f, scaleMultiplier.x),
                Mathf.Approximately(scaleMultiplier.y, 0f) ? 1f : Mathf.Max(0.01f, scaleMultiplier.y),
                Mathf.Approximately(scaleMultiplier.z, 0f) ? 1f : Mathf.Max(0.01f, scaleMultiplier.z));
        }

        private static Vector3 GetSafeColliderSize(Vector3 size)
        {
            return new Vector3(
                size.x > 0.0001f ? size.x : 2f,
                size.y > 0.0001f ? size.y : 2f,
                size.z > 0.0001f ? size.z : 2f);
        }

        private static bool HasRenderableSprite(EnemySpawnRuleConfig rule)
        {
            return rule != null
                && ((rule.idleSprites != null && rule.idleSprites.Length > 0)
                    || (rule.moveSprites != null && rule.moveSprites.Length > 0)
                    || (rule.attackSprites != null && rule.attackSprites.Length > 0)
                    || (rule.deathSprites != null && rule.deathSprites.Length > 0));
        }

        private static float GetConfiguredMinimumBossMaxHealth(MidBossDefinition bossDefinition)
        {
            if (bossDefinition != null && bossDefinition.minimumMaxHealth > 0f)
            {
                return bossDefinition.minimumMaxHealth;
            }

            return 0f;
        }

        private void ApplyFogVisualState()
        {
            UpdateFogVisuals();
        }

        private void UpdateFogReveal(float deltaTime)
        {
            float target = ShouldRevealInteriorFog() ? 1f : 0f;
            float duration = Mathf.Max(0.01f, arenaConfig.fogRevealDuration);
            fogRevealAmount = Mathf.MoveTowards(fogRevealAmount, target, deltaTime / duration);
            UpdateFogVisuals();
        }

        private bool ShouldRevealInteriorFog()
        {
            return arenaLocked || bossDefeated;
        }

        private void UpdateFogVisuals()
        {
            Color borderColor = arenaConfig.lockedFogColor;
            for (int i = 0; i < fogRenderers.Count; i++)
            {
                if (fogRenderers[i] != null)
                {
                    float pulse = 0.9f + Mathf.Sin(Time.time * 1.8f + i * 0.65f) * 0.08f;
                    Color animatedColor = borderColor;
                    animatedColor.a = Mathf.Clamp01(borderColor.a * fogRevealAmount * pulse);
                    fogRenderers[i].color = animatedColor;
                }
            }

            if (interiorFogRenderer != null)
            {
                Color interiorColor = arenaConfig.interiorFogColor;
                interiorColor.a = Mathf.Lerp(
                    Mathf.Clamp01(arenaConfig.interiorFogHiddenAlpha),
                    Mathf.Clamp01(arenaConfig.interiorFogRevealedAlpha),
                    fogRevealAmount);
                interiorFogRenderer.color = interiorColor;
            }
        }

        private Sprite GetFogSprite()
        {
            if (arenaConfig != null && arenaConfig.fogSprite != null)
            {
                return arenaConfig.fogSprite;
            }

            if (fogSprite != null)
            {
                return fogSprite;
            }

            fogSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            fogSprite.name = "MidBossFogSprite";
            return fogSprite;
        }

        private static Sprite GetRuntimeBossSprite()
        {
            if (runtimeBossSprite != null)
            {
                return runtimeBossSprite;
            }

            const int width = 48;
            const int height = 40;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
            float rx = width * 0.4f;
            float ry = height * 0.36f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x - center.x) / rx;
                    float ny = (y - center.y) / ry;
                    float value = nx * nx + ny * ny;
                    texture.SetPixel(x, y, value <= 1f ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            runtimeBossSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), width);
            runtimeBossSprite.name = "RuntimeMidBossSprite";
            return runtimeBossSprite;
        }
    }
}
