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
            seed = mapGenerator.RandomSeed;
            useRandomSeed = false;
            enableHeight = true;
            minHeightLevel = 0;
            maxHeightLevel = 2;
            maxStepHeight = 0;
            heightStep = 0.5f;
            destroyChunkRootOnUnload = true;
            useChunkRootPooling = true;
            ConfigureBossArenaReservation();
            base.Awake();
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

            IReadOnlyList<EnemySpawnRuleConfig> rules = config.GetEnemySpawnRules();
            for (int i = 0; i < rules.Count; i++)
            {
                EnemySpawnRuleConfig rule = rules[i];
                if (rule == null) continue;
                if (rule.isElite) eliteSpawner.RegisterEliteConfig(rule);
                else normalEnemyRules.Add(rule);
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
