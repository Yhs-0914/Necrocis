using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace Necrocis
{
    public enum BiomeMapSource
    {
        Procedural = 0,
        AuthoredTilemap = 1
    }

    /// <summary>
    /// BiomeConfig로 동작하는 범용 바이옴 매니저
    /// </summary>
    public class ConfigurableBiomeManager : RegionPoissonBiomeManager
    {
        [Header("Biome Config")]
        [SerializeField] private BiomeConfig config;

        [Header("Map Source")]
        [SerializeField] private BiomeMapSource mapSource = BiomeMapSource.Procedural;
        [SerializeField] private Tilemap authoredFloorTilemap;
        [SerializeField] private Tilemap authoredDropTransitionTilemap;
        [SerializeField] private Tilemap authoredWaterTilemap;
        [SerializeField] private Tilemap authoredBlockerTilemap;
        [SerializeField] private bool useAuthoredTilemapVisuals = true;
        [SerializeField] private bool spawnAuthoredMapObjects = true;
        [SerializeField] private bool spawnAuthoredEnemies = true;
        [SerializeField] private BiomeTileType authoredFloorType = BiomeTileType.Floor;
        [SerializeField] private BiomeTileType authoredBlockerType = BiomeTileType.Wall;
        [SerializeField] private int authoredHeightLevel = 0;
        [SerializeField] private int authoredHighTileHeightLevel = 1;
        [SerializeField] private string authoredHighTileNameContains = "sand2";
        [SerializeField] private string authoredTransitionTileNameContains = "벽";
        [SerializeField] private bool useCustomAuthoredSpawnCell;
        [SerializeField] private Vector2Int customAuthoredSpawnCell;

        private BiomePerlinNoise detailNoise;
        private readonly List<BiomeObjectRuleConfig> runtimeRules = new List<BiomeObjectRuleConfig>();
        private readonly List<EnemySpawnRuleConfig> runtimeEnemyRules = new List<EnemySpawnRuleConfig>();
        private readonly Dictionary<string, GameObject> resourcePrefabCache = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Material> resourceMaterialCache = new Dictionary<string, Material>();
        private MidBossArenaController midBossArenaController;

        /// <summary>
        /// 현재 바이옴 설정 반환 (엘리트 몹 분열 시 적 설정 검색용)
        /// </summary>
        public BiomeConfig GetBiomeConfig() => config;

        protected override void Awake()
        {
            if (config == null)
            {
                Debug.LogError("[ConfigurableBiomeManager] BiomeConfig가 없습니다.");
                enabled = false;
                return;
            }

            if (config.regions == null || config.regions.Count == 0)
            {
                Debug.LogError("[ConfigurableBiomeManager] Region 설정이 비어 있습니다.");
                enabled = false;
                return;
            }

            biomeType = config.biomeType;
            regionCellSize = config.regionCellSize;
            regionBlendWidth = config.regionBlendWidth;
            regionCount = config.regions.Count;
            heightNoiseScale = config.heightNoiseScale;
            heightNoiseAmplitude = config.heightNoiseAmplitude;
            heightThreshold = config.heightThreshold;
            heightCaIterations = config.heightCaIterations;

            base.Awake();
        }

        protected override void Start()
        {
            HideAuthoredTilemapRenderersIfChunked();
            base.Start();
            TryCreateMidBossArena();
            EnsureWorldMapUI();
        }

        private void EnsureWorldMapUI()
        {
            if (mapSource != BiomeMapSource.AuthoredTilemap || GetComponent<WorldMapUI>() != null)
            {
                return;
            }

            gameObject.AddComponent<WorldMapUI>();
        }

        public Sprite GetAuthoredMapSprite(int gridX, int gridY)
        {
            Vector3Int cell = new Vector3Int(gridX, gridY, 0);
            Tilemap[] layers =
            {
                authoredBlockerTilemap,
                authoredWaterTilemap,
                authoredDropTransitionTilemap,
                authoredFloorTilemap
            };

            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i] == null)
                {
                    continue;
                }

                Sprite sprite = layers[i].GetSprite(cell);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            return null;
        }

        private void HideAuthoredTilemapRenderersIfChunked()
        {
            if (mapSource != BiomeMapSource.AuthoredTilemap || useAuthoredTilemapVisuals)
            {
                return;
            }

            SetTilemapRendererEnabled(authoredFloorTilemap, false);
            SetTilemapRendererEnabled(authoredDropTransitionTilemap, false);
            SetTilemapRendererEnabled(authoredWaterTilemap, false);
            SetTilemapRendererEnabled(authoredBlockerTilemap, false);
        }

        private static void SetTilemapRendererEnabled(Tilemap tilemap, bool enabled)
        {
            if (tilemap == null)
            {
                return;
            }

            TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
            if (renderer != null)
            {
                renderer.enabled = enabled;
            }
        }

        protected override void InitializeNoise()
        {
            base.InitializeNoise();
            detailNoise = new BiomePerlinNoise(seed);
            detailNoise.SetFrequency(config.detailNoiseScale);
        }

        protected override TileSample SampleBaseTile(int worldX, int worldY)
        {
            if (mapSource == BiomeMapSource.AuthoredTilemap)
            {
                return SampleAuthoredTile(worldX, worldY);
            }

            BiomeRegionDefinition region = GetRegionDefinition(worldX, worldY);
            if (region == null)
            {
                return new TileSample(BiomeTileType.None, null, true);
            }

            float detailValue = 0f;
            if (detailNoise != null)
            {
                detailValue = (detailNoise.GetNoise(worldX, worldY) + 1f) * 0.5f;
            }

            // variantTile 체크 (높은 노이즈 값일 때만 사용)
            bool useVariant = region.variantTile != null && detailValue >= region.variantThreshold;
            TileBase tile = useVariant ? region.variantTile : region.primaryTile;
            BiomeTileType type = useVariant ? region.variantType : region.primaryType;

            // tileVariants가 있으면 해시 기반으로 변형 선택 (같은 리전 내 시각적 다양성)
            if (!useVariant && region.tileVariants != null && region.tileVariants.Length > 0)
            {
                int totalTiles = 1 + region.tileVariants.Length; // primaryTile + variants
                int hash = BiomeDeterministic.HashRange(seed, worldX, worldY, 777, totalTiles);
                if (hash > 0)
                {
                    tile = region.tileVariants[hash - 1];
                }
            }

            return new TileSample(type, tile, IsTileWalkable(type));
        }

        protected override int GetBaseHeightLevel(int worldX, int worldY)
        {
            if (mapSource == BiomeMapSource.AuthoredTilemap)
            {
                return GetAuthoredHeightLevel(worldX, worldY);
            }

            return base.GetBaseHeightLevel(worldX, worldY);
        }

        private int GetAuthoredHeightLevel(int worldX, int worldY)
        {
            if (authoredFloorTilemap == null)
            {
                return authoredHeightLevel;
            }

            TileBase floor = authoredFloorTilemap.GetTile(new Vector3Int(worldX, worldY, 0));
            if (floor != null && !string.IsNullOrEmpty(authoredHighTileNameContains)
                && floor.name.Contains(authoredHighTileNameContains))
            {
                return authoredHighTileHeightLevel;
            }

            return authoredHeightLevel;
        }

        protected override void GenerateTiles(Chunk chunk)
        {
            if (mapSource == BiomeMapSource.AuthoredTilemap && useAuthoredTilemapVisuals)
            {
                return;
            }

            base.GenerateTiles(chunk);
            ApplyAuthoredTileTransforms(chunk);
            ApplyAuthoredTransitionOverlays(chunk);
        }

        private void ApplyAuthoredTileTransforms(Chunk chunk)
        {
            if (mapSource != BiomeMapSource.AuthoredTilemap || chunk.tilemaps == null)
            {
                return;
            }

            int startX = GetChunkStartX(chunk.chunkX);
            int startY = GetChunkStartY(chunk.chunkY);
            for (int ly = 0; ly < chunkSize; ly++)
            {
                for (int lx = 0; lx < chunkSize; lx++)
                {
                    int gx = startX + lx;
                    int gy = startY + ly;
                    Vector3Int sourceCell = new Vector3Int(gx, gy, 0);
                    Tilemap sourceTilemap = GetAuthoredBaseSourceTilemap(sourceCell);
                    if (sourceTilemap == null)
                    {
                        continue;
                    }

                    int levelIndex = Mathf.Clamp(GetAuthoredHeightLevel(gx, gy) - MinHeightLevel, 0, chunk.tilemaps.Length - 1);
                    Tilemap targetTilemap = chunk.tilemaps[levelIndex];
                    Vector3Int targetCell = new Vector3Int(lx, ly, 0);
                    if (targetTilemap == null || targetTilemap.GetTile(targetCell) == null)
                    {
                        continue;
                    }

                    targetTilemap.SetTileFlags(targetCell, TileFlags.None);
                    targetTilemap.SetTransformMatrix(targetCell, sourceTilemap.GetTransformMatrix(sourceCell));
                }
            }
        }

        private Tilemap GetAuthoredBaseSourceTilemap(Vector3Int cell)
        {
            if (authoredWaterTilemap != null && authoredWaterTilemap.GetTile(cell) != null)
            {
                return authoredWaterTilemap;
            }

            if (authoredBlockerTilemap != null && authoredBlockerTilemap.GetTile(cell) != null)
            {
                return authoredBlockerTilemap;
            }

            if (authoredFloorTilemap != null && authoredFloorTilemap.GetTile(cell) != null)
            {
                return authoredFloorTilemap;
            }

            if (authoredDropTransitionTilemap != null && authoredDropTransitionTilemap.GetTile(cell) != null)
            {
                return authoredDropTransitionTilemap;
            }

            return null;
        }

        private void ApplyAuthoredTransitionOverlays(Chunk chunk)
        {
            if (mapSource != BiomeMapSource.AuthoredTilemap
                || authoredDropTransitionTilemap == null
                || chunk.cliffTilemaps == null
                || chunk.cliffTilemaps.Length == 0)
            {
                return;
            }

            int overlayLevelIndex = Mathf.Clamp(authoredHeightLevel - MinHeightLevel, 0, chunk.cliffTilemaps.Length - 1);
            Tilemap overlayTilemap = chunk.cliffTilemaps[overlayLevelIndex];
            if (overlayTilemap == null)
            {
                return;
            }

            overlayTilemap.color = Color.white;
            Transform decorationsRoot = CreateAuthoredDecorationsRoot(chunk);
            int startX = GetChunkStartX(chunk.chunkX);
            int startY = GetChunkStartY(chunk.chunkY);
            for (int ly = 0; ly < chunkSize; ly++)
            {
                for (int lx = 0; lx < chunkSize; lx++)
                {
                    int gx = startX + lx;
                    int gy = startY + ly;
                    TileBase transitionTile = authoredDropTransitionTilemap.GetTile(new Vector3Int(gx, gy, 0));
                    if (transitionTile == null)
                    {
                        continue;
                    }

                    if (IsAuthoredTransitionTile(transitionTile))
                    {
                        Vector3Int targetCell = new Vector3Int(lx, ly, 0);
                        Vector3Int sourceCell = new Vector3Int(gx, gy, 0);
                        overlayTilemap.SetTile(targetCell, transitionTile);
                        overlayTilemap.SetTileFlags(targetCell, TileFlags.None);
                        overlayTilemap.SetTransformMatrix(
                            targetCell,
                            authoredDropTransitionTilemap.GetTransformMatrix(sourceCell));
                        CreateAuthoredTransitionWall(decorationsRoot, gx, gy);
                    }
                    else
                    {
                        CreateAuthoredDecoration(decorationsRoot, gx, gy);
                    }
                }
            }
        }

        private static Transform CreateAuthoredDecorationsRoot(Chunk chunk)
        {
            Transform existing = chunk.root.transform.Find("AuthoredDecorations");
            if (existing != null)
            {
                for (int i = existing.childCount - 1; i >= 0; i--)
                {
                    Destroy(existing.GetChild(i).gameObject);
                }

                return existing;
            }

            GameObject root = new GameObject("AuthoredDecorations");
            root.transform.SetParent(chunk.root.transform, false);
            return root.transform;
        }

        private void CreateAuthoredDecoration(Transform parent, int gridX, int gridY)
        {
            Vector3Int cell = new Vector3Int(gridX, gridY, 0);
            Sprite sprite = authoredDropTransitionTilemap.GetSprite(cell);
            if (sprite == null)
            {
                return;
            }

            GameObject decoration = new GameObject($"AuthoredDecoration_{gridX}_{gridY}");
            decoration.transform.SetParent(parent, false);
            decoration.transform.position = GridToWorldWithHeight(gridX, gridY);

            SpriteRenderer renderer = decoration.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 100;

            Billboard billboard = decoration.AddComponent<Billboard>();
            billboard.SetUpdateMode(Billboard.UpdateMode.Continuous);
        }

        private void CreateAuthoredTransitionWall(Transform parent, int gridX, int gridY)
        {
            Vector3Int cell = new Vector3Int(gridX, gridY, 0);
            Sprite sprite = authoredDropTransitionTilemap.GetSprite(cell);
            if (sprite == null)
            {
                return;
            }

            GameObject wall = new GameObject($"AuthoredTransitionWall_{gridX}_{gridY}");
            wall.transform.SetParent(parent, false);
            wall.transform.position = GridToWorldWithHeight(gridX, gridY);

            SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 90;

            Billboard billboard = wall.AddComponent<Billboard>();
            billboard.SetUpdateMode(Billboard.UpdateMode.Continuous);
        }

        public override Vector3 GetPlayerSpawnPosition()
        {
            if (mapSource == BiomeMapSource.AuthoredTilemap)
            {
                Vector2Int spawn = FindAuthoredSpawnCell();
                return GridToWorld(spawn.x, spawn.y);
            }

            return base.GetPlayerSpawnPosition();
        }

        private Vector2Int FindAuthoredSpawnCell()
        {
            if (useCustomAuthoredSpawnCell
                && TryUseAuthoredSpawnCell(customAuthoredSpawnCell.x, customAuthoredSpawnCell.y, out Vector2Int customSpawn))
            {
                return customSpawn;
            }

            if (IsValidPosition(0, 0) && SampleAuthoredTile(0, 0).walkable)
            {
                return Vector2Int.zero;
            }

            int maxRadius = Mathf.Max(mapWidth, mapHeight);
            for (int radius = 1; radius <= maxRadius; radius++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (TryUseAuthoredSpawnCell(x, -radius, out Vector2Int bottom)) return bottom;
                    if (TryUseAuthoredSpawnCell(x, radius, out Vector2Int top)) return top;
                }

                for (int y = -radius + 1; y <= radius - 1; y++)
                {
                    if (TryUseAuthoredSpawnCell(-radius, y, out Vector2Int left)) return left;
                    if (TryUseAuthoredSpawnCell(radius, y, out Vector2Int right)) return right;
                }
            }

            return Vector2Int.zero;
        }

        private bool TryUseAuthoredSpawnCell(int x, int y, out Vector2Int cell)
        {
            cell = new Vector2Int(x, y);
            return IsValidPosition(x, y) && SampleAuthoredTile(x, y).walkable;
        }

        protected override bool IsDropTransitionCell(int x, int y, int referenceLevel)
        {
            if (mapSource != BiomeMapSource.AuthoredTilemap || authoredDropTransitionTilemap == null)
            {
                return false;
            }

            if (referenceLevel <= authoredHeightLevel)
            {
                return false;
            }

            Vector3Int cell = new Vector3Int(x, y, 0);
            return IsAuthoredTransitionTile(authoredDropTransitionTilemap.GetTile(cell));
        }

        protected override bool IsValidDropDestination(int x, int y, int currentLevel)
        {
            if (mapSource != BiomeMapSource.AuthoredTilemap)
            {
                return base.IsValidDropDestination(x, y, currentLevel);
            }

            Vector3Int cell = new Vector3Int(x, y, 0);
            if (authoredWaterTilemap != null && authoredWaterTilemap.GetTile(cell) != null)
            {
                return false;
            }

            if (authoredBlockerTilemap != null && authoredBlockerTilemap.GetTile(cell) != null)
            {
                return false;
            }

            if (authoredDropTransitionTilemap != null
                && IsAuthoredTransitionTile(authoredDropTransitionTilemap.GetTile(cell)))
            {
                return false;
            }

            if (authoredFloorTilemap == null)
            {
                return false;
            }

            TileBase floor = authoredFloorTilemap.GetTile(cell);
            return floor != null && floor.name.Contains("sand1");
        }

        protected override bool IsClimbTransitionCell(int x, int y)
        {
            if (mapSource != BiomeMapSource.AuthoredTilemap || authoredDropTransitionTilemap == null)
            {
                return false;
            }

            Vector3Int cell = new Vector3Int(x, y, 0);
            return IsAuthoredTransitionTile(authoredDropTransitionTilemap.GetTile(cell));
        }

        private TileSample SampleAuthoredTile(int worldX, int worldY)
        {
            Vector3Int cell = new Vector3Int(worldX, worldY, 0);

            if (authoredWaterTilemap != null && authoredWaterTilemap.GetTile(cell) != null)
            {
                return new TileSample(BiomeTileType.Puddle, authoredWaterTilemap.GetTile(cell), false);
            }

            if (authoredBlockerTilemap != null)
            {
                TileBase blocker = authoredBlockerTilemap.GetTile(cell);
                if (blocker != null)
                {
                    return new TileSample(authoredBlockerType, blocker, false);
                }
            }

            if (authoredFloorTilemap != null)
            {
                TileBase floor = authoredFloorTilemap.GetTile(cell);
                if (floor != null)
                {
                    return new TileSample(authoredFloorType, floor, IsTileWalkable(authoredFloorType));
                }
            }

            if (authoredDropTransitionTilemap != null)
            {
                TileBase transition = authoredDropTransitionTilemap.GetTile(cell);
                if (transition != null)
                {
                    return new TileSample(authoredFloorType, transition, true);
                }
            }

            return new TileSample(BiomeTileType.None, null, false);
        }

        private bool IsAuthoredTransitionTile(TileBase tile)
        {
            if (tile == null)
            {
                return false;
            }

            string tileName = tile.name;
            if (string.IsNullOrEmpty(tileName))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(authoredTransitionTileNameContains)
                && tileName.Contains(authoredTransitionTileNameContains))
            {
                return true;
            }

            return tileName.Contains("wall")
                || tileName.Contains("Wall")
                || tileName.Contains("cliff")
                || tileName.Contains("Cliff");
        }

        protected override TileBase GetTileAsset(BiomeTileType tileType)
        {
            return config != null ? config.GetTileForType(tileType) : null;
        }

        protected override int GetRegionHeight(int regionType)
        {
            if (config == null || config.regions == null || config.regions.Count == 0)
            {
                return 0;
            }

            int index = Mathf.Clamp(regionType, 0, config.regions.Count - 1);
            return config.regions[index].baseHeight;
        }

        protected override int GetRegionPlateauRise(int regionType)
        {
            if (config == null || config.regions == null || config.regions.Count == 0)
            {
                return 1;
            }

            int index = Mathf.Clamp(regionType, 0, config.regions.Count - 1);
            int rise = config.regions[index].plateauRise;
            return rise > 0 ? rise : 1;
        }

        protected override int GetWallDepth(int worldX, int worldY)
        {
            if (mapSource == BiomeMapSource.AuthoredTilemap)
            {
                return 0;
            }

            BiomeRegionDefinition region = GetRegionDefinition(worldX, worldY);
            if (region == null || region.wallTiles == null) return 0;
            return region.wallTiles.Length;
        }

        protected override TileBase GetWallTile(int worldX, int worldY, int wallRow)
        {
            if (mapSource == BiomeMapSource.AuthoredTilemap)
            {
                return null;
            }

            BiomeRegionDefinition region = GetRegionDefinition(worldX, worldY);
            if (region == null || region.wallTiles == null) return null;
            if (wallRow < 0 || wallRow >= region.wallTiles.Length) return null;
            return region.wallTiles[wallRow];
        }

        protected override TileBase GetMapEdgeWallTile(int worldX, int worldY)
        {
            if (mapSource == BiomeMapSource.AuthoredTilemap)
            {
                return null;
            }

            BiomeRegionDefinition region = GetRegionDefinition(worldX, worldY);
            return region != null ? region.mapEdgeWallTile : null;
        }

        protected override int GetMapEdgeWallDepth(int worldX, int worldY)
        {
            if (mapSource == BiomeMapSource.AuthoredTilemap)
            {
                return 0;
            }

            BiomeRegionDefinition region = GetRegionDefinition(worldX, worldY);
            return region != null ? region.mapEdgeWallDepth : 0;
        }

        protected override TileBase GetSideWallTile(int worldX, int worldY)
        {
            // Authored maps already contain their intended cliff edges. Generating the
            // procedural side-wall fallback here adds visible black tiles beside them.
            if (mapSource == BiomeMapSource.AuthoredTilemap)
            {
                return null;
            }

            BiomeRegionDefinition region = GetRegionDefinition(worldX, worldY);
            return region != null ? region.sideWallTile : null;
        }

        protected override bool IsObjectAreaAllowed(int x, int y)
        {
            if (config == null) return true;

            int left = Mathf.Max(0, config.marginLeft);
            int right = Mathf.Max(0, config.marginRight);
            int bottom = Mathf.Max(0, config.marginBottom);
            int top = Mathf.Max(0, config.marginTop);

            return x >= MinGridX + left
                && x < MaxGridXExclusive - right
                && y >= MinGridY + bottom
                && y < MaxGridYExclusive - top;
        }

        protected override float GetDensityForRule(ObjectRule rule, int worldX, int worldY, int regionType)
        {
            if (mapSource == BiomeMapSource.AuthoredTilemap)
            {
                bool isEnemySpawner = rule.category == SpawnCategory.EnemySpawner;
                if ((isEnemySpawner && !spawnAuthoredEnemies)
                    || (!isEnemySpawner && !spawnAuthoredMapObjects))
                {
                    return 0f;
                }
            }

            float density = base.GetDensityForRule(rule, worldX, worldY, regionType);
            if (density <= 0f)
            {
                return 0f;
            }

            if (mapSource == BiomeMapSource.AuthoredTilemap && !SampleAuthoredTile(worldX, worldY).walkable)
            {
                return 0f;
            }

            if (mapSource == BiomeMapSource.AuthoredTilemap
                && rule.category == SpawnCategory.EnemySpawner
                && !IsAuthoredEnemySpawnAllowed(worldX, worldY))
            {
                return 0f;
            }

            if (mapSource == BiomeMapSource.AuthoredTilemap
                && authoredDropTransitionTilemap != null
                && IsAuthoredTransitionTile(authoredDropTransitionTilemap.GetTile(new Vector3Int(worldX, worldY, 0))))
            {
                return 0f;
            }

            if (rule.category == SpawnCategory.EnemySpawner && IsInsideMidBossArenaBounds(worldX, worldY))
            {
                return 0f;
            }

            return density;
        }

        private bool IsAuthoredEnemySpawnAllowed(int worldX, int worldY)
        {
            Vector3Int cell = new Vector3Int(worldX, worldY, 0);
            if (authoredWaterTilemap != null && authoredWaterTilemap.GetTile(cell) != null)
            {
                return false;
            }

            if (authoredBlockerTilemap != null && authoredBlockerTilemap.GetTile(cell) != null)
            {
                return false;
            }

            if (authoredDropTransitionTilemap != null
                && IsAuthoredTransitionTile(authoredDropTransitionTilemap.GetTile(cell)))
            {
                return false;
            }

            return SampleAuthoredTile(worldX, worldY).walkable;
        }

        public override bool CanSpawnEnemyAt(int x, int y)
        {
            if (!base.CanSpawnEnemyAt(x, y))
            {
                return false;
            }

            if (mapSource != BiomeMapSource.AuthoredTilemap)
            {
                return !IsInsideMidBossArenaBounds(x, y);
            }

            return IsAuthoredEnemySpawnAllowed(x, y)
                && !IsInsideMidBossArenaBounds(x, y);
        }

        protected override void BuildObjectRules()
        {
            objectRules.Clear();
            runtimeRules.Clear();
            runtimeEnemyRules.Clear();

            if (config == null)
            {
                return;
            }

            // 엘리트 스포너 설정
            EliteSpawner eliteSpawner = GetComponent<EliteSpawner>();
            if (eliteSpawner == null)
                eliteSpawner = gameObject.AddComponent<EliteSpawner>();
            eliteSpawner.ClearConfigs();

            int allMask = 0;
            for (int i = 0; i < config.regions.Count; i++)
            {
                allMask |= 1 << i;
            }

            if (config.objectRules != null)
            {
                for (int i = 0; i < config.objectRules.Count; i++)
                {
                    BiomeObjectRuleConfig ruleConfig = config.objectRules[i];
                    if (ruleConfig == null) continue;

                    int mask = BuildRegionMask(ruleConfig.allowedRegions, allMask);
                    int salt = ruleConfig.poissonSalt != 0 ? ruleConfig.poissonSalt : 200 + i;

                    objectRules.Add(new ObjectRule
                    {
                        category = SpawnCategory.SceneObject,
                        kind = ruleConfig.poolKind,
                        density = ruleConfig.density,
                        minDistance = ruleConfig.minDistance,
                        blocksMovement = ruleConfig.blocksMovement,
                        regionMask = mask,
                        salt = salt,
                        configIndex = runtimeRules.Count,
                        scaleMin = ruleConfig.scaleRange.x,
                        scaleMax = ruleConfig.scaleRange.y,
                        scaleSalt = ruleConfig.scaleSalt,
                        scaleBias = ruleConfig.scaleBias,
                        spacingPadding = ruleConfig.spacingPadding,
                        avoidPlayerSpawnRadius = ruleConfig.avoidPlayerSpawnRadius
                    });

                    runtimeRules.Add(ruleConfig);
                }
            }

            IReadOnlyList<EnemySpawnRuleConfig> enemySpawnRules = config.GetEnemySpawnRules();
            if (enemySpawnRules != null)
            {
                for (int i = 0; i < enemySpawnRules.Count; i++)
                {
                    EnemySpawnRuleConfig ruleConfig = enemySpawnRules[i];
                    if (ruleConfig == null) continue;

                    // 엘리트 몹은 EliteSpawner에 등록 (포아송 분포가 아닌 타이머 기반 스폰)
                    if (ruleConfig.isElite)
                    {
                        eliteSpawner.RegisterEliteConfig(ruleConfig);
                        continue;
                    }

                    int mask = BuildRegionMask(ruleConfig.allowedRegions, allMask);
                    int salt = ruleConfig.poissonSalt != 0 ? ruleConfig.poissonSalt : 600 + i;

                    objectRules.Add(new ObjectRule
                    {
                        category = SpawnCategory.EnemySpawner,
                        kind = BiomeObjectKind.EnemySpawner,
                        density = ruleConfig.density,
                        minDistance = ruleConfig.minDistance,
                        blocksMovement = false,
                        regionMask = mask,
                        salt = salt,
                        configIndex = runtimeEnemyRules.Count
                    });

                    runtimeEnemyRules.Add(ruleConfig);
                }
            }
        }

        protected override void SpawnChunkRecord(ChunkSpawnRecord record, Chunk chunk)
        {
            if (record.category == ChunkSpawnCategory.Portal)
            {
                return;
            }

            if (record.category == ChunkSpawnCategory.EnemySpawner)
            {
                if (record.configIndex < 0 || record.configIndex >= runtimeEnemyRules.Count) return;

                EnemySpawnRuleConfig enemyRule = runtimeEnemyRules[record.configIndex];
                SpawnEnemySpawner(enemyRule, record, chunk);
                return;
            }

            if (record.configIndex < 0 || record.configIndex >= runtimeRules.Count) return;

            BiomeObjectRuleConfig ruleConfig = runtimeRules[record.configIndex];
            SpawnConfiguredObject(ruleConfig, record, chunk);
        }

        protected override void AddExtraChunkSpawnRecords(Chunk chunk)
        {
        }

        private int BuildRegionMask(List<int> regions, int fallbackMask)
        {
            if (regions == null || regions.Count == 0) return fallbackMask;

            int mask = 0;
            foreach (int regionIndex in regions)
            {
                if (regionIndex < 0 || regionIndex >= config.regions.Count) continue;
                mask |= 1 << regionIndex;
            }

            return mask == 0 ? fallbackMask : mask;
        }

        private BiomeRegionDefinition GetRegionDefinition(int worldX, int worldY)
        {
            if (config == null || config.regions == null || config.regions.Count == 0)
            {
                return null;
            }

            int index = Mathf.Clamp(GetRegionTypeCached(worldX, worldY), 0, config.regions.Count - 1);
            return config.regions[index];
        }

        private void TryCreateMidBossArena()
        {
            if (config == null)
            {
                return;
            }

            MidBossArenaConfig midBossArenaConfig = config.GetMidBossArenaConfig();
            if (midBossArenaConfig == null || !midBossArenaConfig.enabled)
            {
                return;
            }

            if (midBossArenaConfig.onlyEnableOnLargeMaps
                && (mapWidth < midBossArenaConfig.minimumMapWidth || mapHeight < midBossArenaConfig.minimumMapHeight))
            {
                return;
            }

            if (midBossArenaController != null)
            {
                return;
            }

            GameObject arenaObject = new GameObject("MidBossArena");
            arenaObject.transform.SetParent(objectsParent != null ? objectsParent : transform, false);
            midBossArenaController = arenaObject.AddComponent<MidBossArenaController>();
            midBossArenaController.Configure(this, midBossArenaConfig, runtimeEnemyRules);
        }

        private bool IsInsideMidBossArenaBounds(int gridX, int gridY)
        {
            if (config == null)
            {
                return false;
            }

            MidBossArenaConfig midBossArenaConfig = config.GetMidBossArenaConfig();
            if (midBossArenaConfig == null || !midBossArenaConfig.enabled)
            {
                return false;
            }

            if (midBossArenaConfig.onlyEnableOnLargeMaps
                && (mapWidth < midBossArenaConfig.minimumMapWidth || mapHeight < midBossArenaConfig.minimumMapHeight))
            {
                return false;
            }

            Vector2Int center = midBossArenaConfig.useCustomCenter
                ? midBossArenaConfig.centerGrid
                : new Vector2Int(MinGridX + mapWidth / 2, MinGridY + mapHeight / 2);

            int halfWidth = Mathf.Max(4, midBossArenaConfig.arenaSize.x / 2);
            int halfHeight = Mathf.Max(4, midBossArenaConfig.arenaSize.y / 2);

            return gridX >= center.x - halfWidth
                && gridX <= center.x + halfWidth
                && gridY >= center.y - halfHeight
                && gridY <= center.y + halfHeight;
        }

        private void SpawnConfiguredObject(BiomeObjectRuleConfig rule, ChunkSpawnRecord record, Chunk chunk)
        {
            bool hasPrefab = !string.IsNullOrWhiteSpace(rule.resourcePrefabPath);
            bool hasSprites = rule.sprites != null && rule.sprites.Length > 0;
            if (!hasPrefab && !hasSprites) return;

            int x = record.x;
            int y = record.y;
            ObjectId id = new ObjectId(x, y, record.objectKind);
            string baseName = string.IsNullOrEmpty(rule.name) ? rule.poolKind.ToString() : rule.name;
            ObjectPoolKey poolKey = GetPoolKey(record);
            GameObject obj = AcquireObject(poolKey, $"{baseName}_{x}_{y}");
            obj.transform.position = GridToWorldWithHeight(x, y, rule.heightOffset);

            if (hasPrefab)
            {
                if (!ConfigurePrefabObject(obj, rule))
                {
                    ReleasePooledObject(poolKey, obj);
                    return;
                }
            }
            else
            {
                ConfigureSpriteObject(obj, rule, x, y);
            }

            ConfigureBillboard(obj, rule.useBillboard);
            ConfigureYSort(obj, rule.useYSort, rule.sortingOrder);
            ConfigureCollider(obj, rule);
            ApplyScale(obj, rule, x, y);

            RegisterObject(chunk, obj, id, poolKey, rule.blocksMovement);
            ActivateSpawnedObject(obj);
        }

        private void ConfigureSpriteObject(GameObject obj, BiomeObjectRuleConfig rule, int x, int y)
        {
            SpriteRenderer sr = GetOrAddComponent<SpriteRenderer>(obj);
            sr.enabled = true;
            sr.sortingOrder = rule.sortingOrder;

            RuntimePrefabInstance marker = obj.GetComponent<RuntimePrefabInstance>();
            if (marker != null && marker.instance != null)
            {
                marker.instance.SetActive(false);
            }

            if (rule.animate)
            {
                SpriteFrameAnimator anim = GetOrAddComponent<SpriteFrameAnimator>(obj);
                anim.enabled = true;
                anim.SetFrames(rule.sprites, rule.animationSpeed);
                anim.Play();
                sr.sprite = rule.sprites[0];
            }
            else
            {
                SpriteFrameAnimator anim = obj.GetComponent<SpriteFrameAnimator>();
                if (anim != null)
                {
                    anim.Stop();
                    anim.enabled = false;
                }

                Sprite sprite = SelectSprite(rule, x, y);
                sr.sprite = sprite;
            }
        }

        private bool ConfigurePrefabObject(GameObject obj, BiomeObjectRuleConfig rule)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;
                sr.sprite = null;
            }

            SpriteFrameAnimator anim = obj.GetComponent<SpriteFrameAnimator>();
            if (anim != null)
            {
                anim.Stop();
                anim.enabled = false;
            }

            GameObject prefab = LoadResourcePrefab(rule.resourcePrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[ConfigurableBiomeManager] Resources prefab를 찾을 수 없습니다: {rule.resourcePrefabPath}");
                return false;
            }

            RuntimePrefabInstance marker = GetOrAddComponent<RuntimePrefabInstance>(obj);
            if (marker.instance == null || marker.resourcePath != rule.resourcePrefabPath)
            {
                if (marker.instance != null)
                {
                    Destroy(marker.instance);
                }

                marker.instance = Instantiate(prefab, obj.transform);
                marker.instance.name = prefab.name;
                marker.resourcePath = rule.resourcePrefabPath;
            }

            marker.instance.SetActive(true);
            marker.instance.transform.localPosition = rule.prefabLocalPosition;
            marker.instance.transform.localRotation = Quaternion.Euler(rule.prefabRotationEuler);
            ApplyPrefabMaterial(marker.instance, rule.resourceTexturePath);
            return true;
        }

        private GameObject LoadResourcePrefab(string path)
        {
            if (resourcePrefabCache.TryGetValue(path, out GameObject cached))
            {
                return cached;
            }

            GameObject prefab = Resources.Load<GameObject>(path);
            resourcePrefabCache[path] = prefab;
            return prefab;
        }

        private void ApplyPrefabMaterial(GameObject instance, string texturePath)
        {
            if (instance == null || string.IsNullOrWhiteSpace(texturePath))
            {
                return;
            }

            Material material = LoadResourceMaterial(texturePath);
            if (material == null)
            {
                return;
            }

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sharedMaterial = material;
            }
        }

        private Material LoadResourceMaterial(string texturePath)
        {
            if (resourceMaterialCache.TryGetValue(texturePath, out Material cached))
            {
                return cached;
            }

            Texture2D texture = Resources.Load<Texture2D>(texturePath);
            if (texture == null)
            {
                Debug.LogWarning($"[ConfigurableBiomeManager] Resources texture를 찾을 수 없습니다: {texturePath}");
                return null;
            }

            texture.filterMode = FilterMode.Point;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Texture");
            }

            Material material = new Material(shader);
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }
            else
            {
                material.mainTexture = texture;
            }

            resourceMaterialCache[texturePath] = material;
            return material;
        }

        private void ApplyScale(GameObject obj, BiomeObjectRuleConfig rule, int x, int y)
        {
            int salt = rule.scaleSalt != 0 ? rule.scaleSalt : rule.poissonSalt + 9173;
            float scale = BiomeDeterministic.ComputeScale(seed, x, y, salt, rule.scaleRange.x, rule.scaleRange.y, rule.scaleBias);
            obj.transform.localScale = new Vector3(scale, scale, scale);
        }

        private void SpawnEnemySpawner(EnemySpawnRuleConfig rule, ChunkSpawnRecord record, Chunk chunk)
        {
            ObjectId id = new ObjectId(record.x, record.y, record.objectKind);
            string baseName = string.IsNullOrEmpty(rule.name) ? "EnemySpawner" : rule.name;
            ObjectPoolKey poolKey = GetPoolKey(record);
            GameObject obj = AcquireObject(poolKey, $"{baseName}_Spawner_{record.x}_{record.y}");
            obj.transform.position = GridToWorldWithHeight(record.x, record.y, rule.heightOffset);

            RegisterObject(chunk, obj, id, poolKey, false);

            EnemySpawner spawner = GetOrAddComponent<EnemySpawner>(obj);
            spawner.Configure(rule, obj.transform.position);
            ActivateSpawnedObject(obj);
        }

        private Sprite SelectSprite(BiomeObjectRuleConfig rule, int x, int y)
        {
            if (rule.sprites == null || rule.sprites.Length == 0) return null;
            if (!rule.useDeterministicSprite || rule.sprites.Length == 1)
            {
                return rule.sprites[0];
            }

            int salt = rule.spriteSalt != 0 ? rule.spriteSalt : rule.poissonSalt;
            int index = BiomeDeterministic.HashRange(seed, x, y, salt, rule.sprites.Length);
            return rule.sprites[index];
        }

        private GameObject AcquireObject(ObjectPoolKey poolKey, string name)
        {
            GameObject obj = GetPooledObject(poolKey, () => new GameObject(name));
            obj.name = name;
            obj.transform.SetParent(objectsParent, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            obj.SetActive(false);
            return obj;
        }

        private static void ActivateSpawnedObject(GameObject obj)
        {
            if (obj != null && !obj.activeSelf)
            {
                obj.SetActive(true);
            }
        }

        private static ObjectPoolKey GetPoolKey(ChunkSpawnRecord record)
        {
            int archetypeId = record.category == ChunkSpawnCategory.Portal ? 0 : record.configIndex + 1;
            return new ObjectPoolKey(record.objectKind, archetypeId);
        }

        private static T GetOrAddComponent<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
            {
                component = obj.AddComponent<T>();
            }
            return component;
        }

        private void ConfigureBillboard(GameObject obj, bool enabled)
        {
            Billboard billboard = obj.GetComponent<Billboard>();
            if (enabled)
            {
                if (billboard == null)
                {
                    billboard = obj.AddComponent<Billboard>();
                }
                billboard.enabled = true;
                billboard.ResetBaseLocalPosition(obj.transform.localPosition);
                billboard.SetUpdateMode(Billboard.UpdateMode.Continuous);
            }
            else if (billboard != null)
            {
                billboard.enabled = false;
            }
        }

        private void ConfigureYSort(GameObject obj, bool enabled, int sortingOrder)
        {
            SpriteYSort sorter = obj.GetComponent<SpriteYSort>();
            if (enabled)
            {
                if (sorter == null)
                {
                    sorter = obj.AddComponent<SpriteYSort>();
                }
                sorter.Configure(SpriteYSort.WorldDynamicBaseSortingOrder, true, SpriteYSort.WorldDynamicMinSortingOrder);
                sorter.SetUpdateMode(SpriteYSort.UpdateMode.Once);
            }
            else if (sorter != null)
            {
                sorter.enabled = false;
            }
        }

        private void ConfigureCollider(GameObject obj, BiomeObjectRuleConfig rule)
        {
            BoxCollider col = obj.GetComponent<BoxCollider>();
            if (!rule.addCollider)
            {
                if (col != null) col.enabled = false;
                return;
            }

            if (col == null)
            {
                col = obj.AddComponent<BoxCollider>();
            }

            col.enabled = true;
            col.isTrigger = rule.isTrigger;
            col.size = rule.colliderSize;
            col.center = rule.colliderCenter;
        }

    }
}
