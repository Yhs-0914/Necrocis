using System.Collections;
using System.Collections.Generic;
using ProceduralMap;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Necrocis
{
    /// <summary>
    /// 절차적 Tilemap을 기존 Necrocis의 적, 아이템, 보스 및 저장 시스템과 연결합니다.
    /// 지형 렌더링은 MapGenerator가 담당하고 이 컴포넌트는 게임플레이 정보만 제공합니다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MapGenerator))]
    public sealed class ProceduralBiomeBridge : BiomeManager
    {
        [SerializeField] private BiomeConfig config;
        [SerializeField, Min(8)] private int proceduralChunkSize = 32;
        [SerializeField, Min(1)] private int proceduralLoadDistance = 2;
        [SerializeField, Min(1)] private int proceduralUnloadDistance = 3;

        [Header("Enemy Spawner Coverage")]
        [Tooltip("새 절차 맵에서 이전 맵의 면적당 스포너 밀도를 복원합니다.")]
        [SerializeField] private bool useAreaBalancedEnemySpawning;
        [Tooltip("이전 맵의 스포너 배치 단위입니다. 16이면 기존 16x16 청크 밀도를 유지합니다.")]
        [SerializeField, Min(4)] private int enemySpawnerCellSize = 16;
        [Tooltip("촘촘히 배치된 스포너 중 플레이어 주변에서 실제 전투를 활성화할 반경입니다.")]
        [SerializeField, Min(1f)] private float enemySpawnerActivationRadius = 28f;

        private MapGenerator mapGenerator;
        private readonly List<EnemySpawnRuleConfig> normalEnemyRules = new List<EnemySpawnRuleConfig>();
        private MidBossArenaController bossArena;

        public void Configure(BiomeConfig biomeConfig)
        {
            config = biomeConfig;
        }

        public BiomeConfig GetBiomeConfig() => config;

        protected override void Awake()
        {
            mapGenerator = GetComponent<MapGenerator>();
            if (config == null || mapGenerator == null)
            {
                Debug.LogError("[ProceduralBiomeBridge] MapGenerator 또는 BiomeConfig가 없습니다.");
                enabled = false;
                return;
            }

            biomeType = config.biomeType;
            mapWidth = mapGenerator.MapWidth;
            mapHeight = mapGenerator.MapHeight;
            chunkSize = proceduralChunkSize;
            loadDistance = proceduralLoadDistance;
            unloadDistance = Mathf.Max(proceduralLoadDistance, proceduralUnloadDistance);
            chunkUpdateInterval = 0.2f;
            objectGenerationBudget = 8;
            tileSize = 1f;
            useRandomSeed = true;
            enableHeight = true;
            minHeightLevel = 0;
            maxHeightLevel = 2;
            maxStepHeight = 0;
            heightStep = 0.5f;
            destroyChunkRootOnUnload = true;
            useChunkRootPooling = true;
            ConfigureBossArenaReservation();
            base.Awake();
            mapGenerator.ConfigureRandomSeed(seed);
        }

        private void ConfigureBossArenaReservation()
        {
            MidBossArenaConfig arenaConfig = config.GetMidBossArenaConfig();
            if (!IsBossArenaEnabled(arenaConfig))
            {
                return;
            }

            Vector2Int center = arenaConfig.useCustomCenter
                ? arenaConfig.centerGrid
                : new Vector2Int(mapGenerator.MapWidth / 2, mapGenerator.MapHeight / 2);
            int padding = Mathf.Max(
                2,
                arenaConfig.wallThicknessInCells + arenaConfig.lockBoundaryInsetInCells + 2);
            BossArenaPresentationConfig presentation = arenaConfig.GetPresentationConfig();
            if (presentation.enabled)
            {
                padding = Mathf.Max(padding, presentation.approachLengthInCells + 2);
            }

            mapGenerator.ConfigureBossArenaReservation(center, arenaConfig.arenaSize, padding);
        }

        protected override void Start()
        {
            StartCoroutine(InitializeWhenMapReady());
        }

        private IEnumerator InitializeWhenMapReady()
        {
            while (mapGenerator != null && !mapGenerator.IsReady)
            {
                yield return null;
            }

            if (mapGenerator == null) yield break;

            Initialize();
            BuildEnemyRules();
            playerTransform = PlayerController.Instance != null
                ? PlayerController.Instance.transform
                : null;

            if (playerTransform != null)
            {
                SetupCamera(playerTransform);
                UpdateChunks();
            }

            WorldItemSpawner itemSpawner = GetComponent<WorldItemSpawner>();
            if (itemSpawner == null) itemSpawner = gameObject.AddComponent<WorldItemSpawner>();
            itemSpawner.SpawnItemsNow();

            CreateBossArena();
            PlayBiomeBgm();
        }

        private void BuildEnemyRules()
        {
            normalEnemyRules.Clear();
            EliteSpawner eliteSpawner = GetComponent<EliteSpawner>();
            if (eliteSpawner == null) eliteSpawner = gameObject.AddComponent<EliteSpawner>();
            eliteSpawner.ClearConfigs();
            eliteSpawner.ConfigureKillInterval(
                config.enemySpawnConfig != null
                    ? config.enemySpawnConfig.NormalKillsPerElite
                    : 10);

            IReadOnlyList<EnemySpawnRuleConfig> rules = config.GetEnemySpawnRules();
            for (int i = 0; i < rules.Count; i++)
            {
                EnemySpawnRuleConfig rule = rules[i];
                if (rule == null) continue;
                if (rule.isElite) eliteSpawner.RegisterEliteConfig(rule);
                else
                {
                    normalEnemyRules.Add(rule);
                    eliteSpawner.RegisterNormalEnemyConfig(rule);
                }
            }
        }

        private void CreateBossArena()
        {
            MidBossArenaConfig arenaConfig = config.GetMidBossArenaConfig();
            if (!IsBossArenaEnabled(arenaConfig)) return;
            GameObject arenaObject = new GameObject("MidBossArena");
            arenaObject.transform.SetParent(transform, false);
            bossArena = arenaObject.AddComponent<MidBossArenaController>();
            bossArena.Configure(this, arenaConfig, normalEnemyRules, config.GetReturnPortalConfig());
        }

        private bool IsBossArenaEnabled(MidBossArenaConfig arenaConfig)
        {
            if (arenaConfig == null || !arenaConfig.enabled)
            {
                return false;
            }

            return !arenaConfig.onlyEnableOnLargeMaps
                || (mapGenerator.MapWidth >= arenaConfig.minimumMapWidth
                    && mapGenerator.MapHeight >= arenaConfig.minimumMapHeight);
        }

        private void PlayBiomeBgm()
        {
            string key = biomeType switch
            {
                BiomeType.Intestine => "IntestineMap",
                BiomeType.Liver => "LiverMap",
                BiomeType.Stomach => "StomachMap",
                BiomeType.Lung => "LungMap",
                _ => "InGame"
            };
            AudioManager.Instance?.PlayBGM(key);
        }

        protected override TileSample SampleBaseTile(int worldX, int worldY)
        {
            bool walkable = mapGenerator != null && mapGenerator.IsCellWalkable(worldX, worldY);
            return new TileSample(walkable ? BiomeTileType.Floor : BiomeTileType.Obstacle, null, walkable);
        }

        protected override TileBase GetTileAsset(BiomeTileType tileType) => null;

        protected override int GetBaseHeightLevel(int worldX, int worldY)
        {
            return mapGenerator != null ? mapGenerator.GetCellHeightLevel(worldX, worldY) : 0;
        }

        public override Vector3 GetPlayerSpawnPosition()
        {
            return mapGenerator != null
                ? mapGenerator.GetPlayerSpawnWorldPosition()
                : base.GetPlayerSpawnPosition();
        }

        protected override void GenerateObjectsForChunk(Chunk chunk)
        {
            if (useAreaBalancedEnemySpawning)
            {
                GenerateAreaBalancedEnemySpawners(chunk);
                return;
            }

            for (int ruleIndex = 0; ruleIndex < normalEnemyRules.Count; ruleIndex++)
            {
                EnemySpawnRuleConfig rule = normalEnemyRules[ruleIndex];
                float chance = Mathf.Clamp01(rule.density * 4f);
                int chanceHash = BiomeDeterministic.HashRange(
                    seed, chunk.chunkX, chunk.chunkY, rule.poissonSalt + 1701, 10000);
                if (chanceHash >= Mathf.RoundToInt(chance * 10000f)) continue;
                if (!TryFindWalkableCell(chunk, rule.poissonSalt, out int x, out int y)) continue;

                GameObject spawnerObject = new GameObject($"{rule.name}_Spawner_{x}_{y}");
                spawnerObject.transform.position = GridToWorldWithHeight(x, y, rule.heightOffset);
                EnemySpawner spawner = spawnerObject.AddComponent<EnemySpawner>();
                spawner.Configure(rule, spawnerObject.transform.position);

                ObjectId id = new ObjectId(x, y, BiomeObjectKind.EnemySpawner);
                ObjectPoolKey poolKey = new ObjectPoolKey(BiomeObjectKind.EnemySpawner, ruleIndex);
                RegisterObject(chunk, spawnerObject, id, poolKey, false);
            }
        }

        private void GenerateAreaBalancedEnemySpawners(Chunk chunk)
        {
            int placementCellSize = Mathf.Max(4, enemySpawnerCellSize);
            int chunkStartX = chunk.chunkX * chunkSize;
            int chunkStartY = chunk.chunkY * chunkSize;
            int chunkEndX = Mathf.Min(chunkStartX + chunkSize, mapWidth);
            int chunkEndY = Mathf.Min(chunkStartY + chunkSize, mapHeight);
            WorldDifficultyBalance worldBalance = DifficultyBalanceService.GetWorldBalance(BiomeType);
            float densityMultiplier = worldBalance != null
                ? Mathf.Max(0f, worldBalance.enemySpawnerDensity)
                : 1f;
            HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();

            for (int cellStartY = chunkStartY; cellStartY < chunkEndY; cellStartY += placementCellSize)
            {
                for (int cellStartX = chunkStartX; cellStartX < chunkEndX; cellStartX += placementCellSize)
                {
                    int cellEndX = Mathf.Min(cellStartX + placementCellSize, chunkEndX);
                    int cellEndY = Mathf.Min(cellStartY + placementCellSize, chunkEndY);
                    int cellKeyX = Mathf.FloorToInt((float)cellStartX / placementCellSize);
                    int cellKeyY = Mathf.FloorToInt((float)cellStartY / placementCellSize);
                    int cellArea = Mathf.Max(1, (cellEndX - cellStartX) * (cellEndY - cellStartY));

                    for (int ruleIndex = 0; ruleIndex < normalEnemyRules.Count; ruleIndex++)
                    {
                        EnemySpawnRuleConfig rule = normalEnemyRules[ruleIndex];
                        // 기존 배치기는 minDistance 크기의 셀마다 density 확률로 후보를 뽑았습니다.
                        // 같은 면적 기준을 사용해 새 청크 크기와 무관하게 이전 밀도를 유지합니다.
                        float spacingArea = Mathf.Max(1f, rule.minDistance * rule.minDistance);
                        float chance = Mathf.Clamp01(rule.density * densityMultiplier * cellArea / spacingArea);
                        int chanceHash = BiomeDeterministic.HashRange(
                            seed, cellKeyX, cellKeyY, rule.poissonSalt + 1701, 10000);
                        if (chanceHash >= Mathf.RoundToInt(chance * 10000f)) continue;

                        if (!TryFindWalkableCellInArea(
                                cellStartX,
                                cellStartY,
                                cellEndX,
                                cellEndY,
                                cellKeyX,
                                cellKeyY,
                                rule,
                                occupiedPositions,
                                out int x,
                                out int y))
                        {
                            continue;
                        }

                        SpawnEnemySpawner(chunk, rule, ruleIndex, x, y, enemySpawnerActivationRadius);
                        occupiedPositions.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        private bool TryFindWalkableCellInArea(
            int startX,
            int startY,
            int endX,
            int endY,
            int cellKeyX,
            int cellKeyY,
            EnemySpawnRuleConfig rule,
            HashSet<Vector2Int> occupiedPositions,
            out int resultX,
            out int resultY)
        {
            int inset = Mathf.Max(1, Mathf.CeilToInt(rule.minDistance * 0.5f));
            int minX = Mathf.Min(endX - 1, startX + inset);
            int minY = Mathf.Min(endY - 1, startY + inset);
            int maxX = Mathf.Max(minX, endX - inset - 1);
            int maxY = Mathf.Max(minY, endY - inset - 1);
            int width = Mathf.Max(1, maxX - minX + 1);
            int height = Mathf.Max(1, maxY - minY + 1);

            for (int attempt = 0; attempt < 24; attempt++)
            {
                int salt = rule.poissonSalt + attempt * 2;
                int x = minX + BiomeDeterministic.HashRange(seed, cellKeyX, cellKeyY, salt, width);
                int y = minY + BiomeDeterministic.HashRange(seed, cellKeyX, cellKeyY, salt + 1, height);
                Vector2Int position = new Vector2Int(x, y);
                if (occupiedPositions.Contains(position)) continue;
                if (!IsEnemySpawnAreaAllowed(x, y)) continue;
                if (!IsValidPosition(x, y) || !mapGenerator.IsCellWalkable(x, y)) continue;
                if (IsInsideMidBossArenaBounds(x, y)) continue;

                resultX = x;
                resultY = y;
                return true;
            }

            resultX = resultY = 0;
            return false;
        }

        private bool IsEnemySpawnAreaAllowed(int x, int y)
        {
            if (config == null) return true;

            int left = Mathf.Max(0, config.marginLeft);
            int right = Mathf.Max(0, config.marginRight);
            int bottom = Mathf.Max(0, config.marginBottom);
            int top = Mathf.Max(0, config.marginTop);
            return x >= left && x < mapWidth - right && y >= bottom && y < mapHeight - top;
        }

        private bool IsInsideMidBossArenaBounds(int gridX, int gridY)
        {
            if (config == null) return false;

            MidBossArenaConfig arenaConfig = config.GetMidBossArenaConfig();
            if (arenaConfig == null || !arenaConfig.enabled) return false;
            if (arenaConfig.onlyEnableOnLargeMaps
                && (mapWidth < arenaConfig.minimumMapWidth || mapHeight < arenaConfig.minimumMapHeight))
            {
                return false;
            }

            Vector2Int center = arenaConfig.useCustomCenter
                ? arenaConfig.centerGrid
                : new Vector2Int(mapWidth / 2, mapHeight / 2);
            int halfWidth = Mathf.Max(4, arenaConfig.arenaSize.x / 2);
            int halfHeight = Mathf.Max(4, arenaConfig.arenaSize.y / 2);
            return gridX >= center.x - halfWidth
                && gridX <= center.x + halfWidth
                && gridY >= center.y - halfHeight
                && gridY <= center.y + halfHeight;
        }

        private void SpawnEnemySpawner(
            Chunk chunk,
            EnemySpawnRuleConfig rule,
            int ruleIndex,
            int x,
            int y,
            float activationRadiusOverride = -1f)
        {
            GameObject spawnerObject = new GameObject($"{rule.name}_Spawner_{x}_{y}");
            spawnerObject.transform.position = GridToWorldWithHeight(x, y, rule.heightOffset);
            EnemySpawner spawner = spawnerObject.AddComponent<EnemySpawner>();
            spawner.Configure(rule, spawnerObject.transform.position, activationRadiusOverride);

            ObjectId id = new ObjectId(x, y, BiomeObjectKind.EnemySpawner);
            ObjectPoolKey poolKey = new ObjectPoolKey(BiomeObjectKind.EnemySpawner, ruleIndex);
            RegisterObject(chunk, spawnerObject, id, poolKey, false);
        }

        private bool TryFindWalkableCell(Chunk chunk, int salt, out int resultX, out int resultY)
        {
            int startX = chunk.chunkX * chunkSize;
            int startY = chunk.chunkY * chunkSize;
            for (int attempt = 0; attempt < 24; attempt++)
            {
                int x = startX + BiomeDeterministic.HashRange(seed, chunk.chunkX, chunk.chunkY, salt + attempt * 2, chunkSize);
                int y = startY + BiomeDeterministic.HashRange(seed, chunk.chunkX, chunk.chunkY, salt + attempt * 2 + 1, chunkSize);
                if (IsValidPosition(x, y)
                    && !mapGenerator.IsCellReservedForBossArena(x, y)
                    && mapGenerator.IsCellWalkable(x, y))
                {
                    resultX = x;
                    resultY = y;
                    return true;
                }
            }

            resultX = resultY = 0;
            return false;
        }
    }
}
