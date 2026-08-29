using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using ProceduralMap.Pooling;

namespace ProceduralMap
{
    public sealed class MapGenerator : MonoBehaviour
    {
        [Header("Tilemaps")]
        [SerializeField] private Tilemap baseTilemap;
        [SerializeField] private Tilemap grassTilemap;
        [SerializeField] private Tilemap secondFloorTilemap;
        [SerializeField] private Tilemap cliffTilemap;
        [SerializeField] private Tilemap thirdFloorTilemap;
        [SerializeField] private Tilemap thirdFloorCliffTilemap;
        [SerializeField] private Tilemap roadTilemap;
        [SerializeField] private Tilemap lavaTilemap;
        [SerializeField] private Tilemap stomachRockTilemap;

        [Header("Tiles")]
        [Tooltip("PNG를 Sprite로 임포트한 뒤 이곳에 직접 연결하세요.")]
        [SerializeField] private Sprite baseTile;
        [SerializeField] private TerrainSpriteSet9 grassTiles = new TerrainSpriteSet9();
        [Tooltip("폐 맵의 왼쪽/중앙/오른쪽/아래 코너 규칙 6타일을 사용할 때 켭니다.")]
        [SerializeField] private bool useLungGrassTiles;
        [SerializeField] private LungGrassSpriteSet6 lungGrassTiles = new LungGrassSpriteSet6();
        [Tooltip("Colon처럼 여러 잔디 종류를 사용할 때 켭니다. 각 Shape는 한 종류만 사용합니다.")]
        [SerializeField] private bool useMultipleGrassTypes;
        [SerializeField] private List<TerrainSpriteSet9> grassTypeTileSets = new List<TerrainSpriteSet9>();
        [Tooltip("2층 영역 전체에 반복해서 배치할 PNG Sprite")]
        [SerializeField] private Sprite secondFloorTile;
        [Tooltip("2층의 아래쪽 노출면에 반복해서 배치할 절벽 타일")]
        [SerializeField] private Sprite cliffTile;
        [Tooltip("3층 영역 전체에 반복 배치할 바닥 PNG Sprite")]
        [SerializeField] private Sprite thirdFloorTile;
        [Tooltip("2층과 3층 사이의 아래쪽 노출면에 배치할 절벽 PNG Sprite")]
        [SerializeField] private Sprite thirdFloorCliffTile;
        [SerializeField] private RoadSpriteSet8 roadTiles = new RoadSpriteSet8();
        [SerializeField] private TerrainSpriteSet9 lavaTiles = new TerrainSpriteSet9();
        [SerializeField] private TerrainSpriteSet9 stomachRockTiles = new TerrainSpriteSet9();

        [Header("Map")]
        [Tooltip("Necrocis 통합 씬처럼 Grid를 XZ 바닥으로 회전해 사용할 때 켭니다.")]
        [SerializeField] private bool useXZWorld = true;
        [SerializeField, Min(1)] private int mapWidth = 300;
        [SerializeField, Min(1)] private int mapHeight = 300;
        [SerializeField, Min(0)] private int bottomEmptyRows = 2;

        [Header("Grass Shapes")]
        [SerializeField] private bool generateGrass = true;
        [SerializeField, Min(0)] private int grassAreaCount = 30;
        [SerializeField] private Vector2Int grassMinSize = new Vector2Int(3, 3);
        [SerializeField] private Vector2Int grassMaxSize = new Vector2Int(20, 14);
        [SerializeField, Range(0, 6)] private int grassExtraShapeParts = 3;
        [SerializeField, Min(0)] private int terrainSpacing = 2;
        [SerializeField, Min(0)] private int edgeMargin = 2;
        [SerializeField, Min(1)] private int placementAttemptsPerArea = 50;

        [Header("Second Floor Shapes")]
        [SerializeField, Min(0)] private int secondFloorAreaCount = 6;
        [SerializeField, Min(4)] private int secondFloorMinCells = 800;
        [SerializeField, Min(4)] private int secondFloorMaxCells = 3500;
        [SerializeField, Range(0, 5)] private int secondFloorSmoothingIterations = 2;
        [SerializeField, Range(0, 3)] private int cliffGrassClearance = 1;

        [Header("Stomach Lava")]
        [SerializeField] private bool generateLava;
        [SerializeField, Min(0)] private int lavaAreaCount = 20;
        [SerializeField] private Vector2Int lavaMinSize = new Vector2Int(5, 5);
        [SerializeField] private Vector2Int lavaMaxSize = new Vector2Int(24, 18);
        [SerializeField, Range(0, 6)] private int lavaExtraShapeParts = 3;

        [Header("Third Floor Shapes")]
        [SerializeField] private bool generateThirdFloor;
        [SerializeField, Min(0)] private int thirdFloorAreaCount = 4;
        [SerializeField, Min(4)] private int thirdFloorMinCells = 120;
        [SerializeField, Min(4)] private int thirdFloorMaxCells = 600;
        [SerializeField, Range(0, 5)] private int thirdFloorSmoothingIterations = 1;
        [SerializeField, Min(0)] private int thirdFloorSpacing = 2;

        [Header("Lung Roads")]
        [SerializeField] private bool generateRoads;
        [SerializeField, Min(1)] private int roadPathCount = 1;
        [SerializeField, Range(0f, 0.8f)] private float roadTurnChance = 0.18f;
        [SerializeField, Min(1)] private int roadEdgeMargin = 8;
        [SerializeField, Min(0)] private int roadTerrainClearance = 2;
        [Header("Stomach Rock Shapes")]
        [SerializeField, Min(0)] private int stomachRockAreaCount = 12;
        [SerializeField] private Vector2Int stomachRockMinSize = new Vector2Int(3, 3);
        [SerializeField] private Vector2Int stomachRockMaxSize = new Vector2Int(12, 10);
        [SerializeField, Range(0, 6)] private int stomachRockExtraShapeParts = 2;
        [SerializeField, Min(0)] private int stomachRockSpacing = 1;

        [Header("Obstacles")]
        [SerializeField] private Transform obstacleRoot;
        [SerializeField] private List<ObstacleDefinition> obstacleDefinitions = new List<ObstacleDefinition>();

        [Header("Lava Pop Hazards")]
        [SerializeField] private List<LavaPopDefinition> lavaPopDefinitions = new List<LavaPopDefinition>();

        [Header("Chunk Streaming")]
        [SerializeField] private Transform player;
        [SerializeField, Min(1)] private int chunkSize = 30;
        [SerializeField, Min(0)] private int loadRadius = 1;
        [SerializeField, Min(0)] private int unloadRadius = 2;

        [Header("Pooling")]
        [SerializeField] private ObjectPoolManager objectPool;

        [Header("Random")]
        [SerializeField] private int randomSeed = 12345;
        [SerializeField] private bool generateOnStart = true;

        private GridData gridData;
        private OccupancyGrid occupancyGrid;
        private OccupancyGrid thirdFloorOccupancyGrid;
        private readonly List<UnityEngine.Tilemaps.Tile> runtimeTiles = new List<UnityEngine.Tilemaps.Tile>();
        private UnityEngine.Tilemaps.Tile baseRuntimeTile;
        private readonly UnityEngine.Tilemaps.Tile[] grassRuntimeTiles = new UnityEngine.Tilemaps.Tile[9];
        private readonly UnityEngine.Tilemaps.Tile[] lungGrassRuntimeTiles = new UnityEngine.Tilemaps.Tile[6];
        private readonly List<UnityEngine.Tilemaps.Tile[]> grassTypeRuntimeTiles =
            new List<UnityEngine.Tilemaps.Tile[]>();
        private UnityEngine.Tilemaps.Tile secondFloorRuntimeTile;
        private UnityEngine.Tilemaps.Tile cliffRuntimeTile;
        private UnityEngine.Tilemaps.Tile thirdFloorRuntimeTile;
        private UnityEngine.Tilemaps.Tile thirdFloorCliffRuntimeTile;
        private readonly UnityEngine.Tilemaps.Tile[] roadRuntimeTiles = new UnityEngine.Tilemaps.Tile[8];
        private readonly UnityEngine.Tilemaps.Tile[] lavaRuntimeTiles = new UnityEngine.Tilemaps.Tile[9];
        private readonly UnityEngine.Tilemaps.Tile[] stomachRockRuntimeTiles = new UnityEngine.Tilemaps.Tile[9];
        private readonly List<GameObject> generatedObstacles = new List<GameObject>();
        private readonly HashSet<Vector2Int> activeChunks = new HashSet<Vector2Int>();
        private readonly Dictionary<Vector2Int, List<ObstacleSpawnData>> obstaclePlanByChunk =
            new Dictionary<Vector2Int, List<ObstacleSpawnData>>();
        private readonly Dictionary<Vector2Int, List<GameObject>> chunkObstacleInstances =
            new Dictionary<Vector2Int, List<GameObject>>();
        private Vector2Int currentPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);
        private bool hasBossArenaReservation;
        private Vector2Int bossArenaCenter;
        private Vector2Int bossArenaSize;
        private int bossArenaPadding;

        public GridData Data => gridData;
        public bool IsReady => gridData != null;
        public int MapWidth => mapWidth;
        public int MapHeight => mapHeight;
        public int RandomSeed => randomSeed;

        public void ConfigureRandomSeed(int seed)
        {
            randomSeed = seed;
        }

        public void ConfigureBossArenaReservation(
            Vector2Int center, Vector2Int size, int padding)
        {
            hasBossArenaReservation = true;
            bossArenaCenter = new Vector2Int(
                Mathf.Clamp(center.x, 0, Mathf.Max(0, mapWidth - 1)),
                Mathf.Clamp(center.y, 0, Mathf.Max(0, mapHeight - 1)));
            bossArenaSize = new Vector2Int(Mathf.Max(8, size.x), Mathf.Max(8, size.y));
            bossArenaPadding = Mathf.Max(0, padding);
        }

        public bool IsCellReservedForBossArena(int x, int y)
        {
            if (!hasBossArenaReservation)
            {
                return false;
            }

            GetBossArenaReservationBounds(out int minX, out int minY, out int maxX, out int maxY);
            return x >= minX && x <= maxX && y >= minY && y <= maxY;
        }

        public bool IsCellWalkable(int x, int y)
        {
            if (gridData == null || !gridData.IsInside(x, y)) return false;
            MapCell cell = gridData.GetCell(x, y);
            return !cell.IsVoid && !cell.HasCliff && (!cell.HasLava || cell.HasStomachRock);
        }

        public int GetCellHeightLevel(int x, int y)
        {
            return gridData != null && gridData.IsInside(x, y)
                ? gridData.GetCell(x, y).HeightLevel
                : 0;
        }

        public Vector3 GetCellCenterWorld(int x, int y)
        {
            return GetCellCenterWorld(new Vector2Int(x, y));
        }

        public bool CanPlayerMove(Vector2 currentWorldPosition, Vector2 targetWorldPosition)
        {
            return CanPlayerMove(currentWorldPosition, targetWorldPosition, Vector2.zero);
        }

        public bool CanPlayerMove(
            Vector2 currentWorldPosition, Vector2 targetWorldPosition, Vector2 halfExtents)
        {
            if (gridData == null) return true;
            Vector2Int current = WorldToCell(currentWorldPosition);
            if (!gridData.IsInside(current.x, current.y)) return true;
            int currentHeight = gridData.GetCell(current.x, current.y).HeightLevel;

            halfExtents.x = Mathf.Max(0f, halfExtents.x);
            halfExtents.y = Mathf.Max(0f, halfExtents.y);
            int targetInvalidCount = CountInvalidPlayerSamples(targetWorldPosition, halfExtents, currentHeight);
            if (targetInvalidCount == 0) return true;

            // 점프 착지나 큰 스프라이트 때문에 이미 경계에 걸린 경우에는
            // 겹침이 감소하는 방향으로만 이동을 허용하여 영구적으로 끼지 않게 한다.
            int currentInvalidCount = CountInvalidPlayerSamples(currentWorldPosition, halfExtents, currentHeight);
            if (currentInvalidCount <= 0) return false;
            if (targetInvalidCount < currentInvalidCount) return true;
            if (targetInvalidCount > currentInvalidCount) return false;

            Vector3 safeCenter3 = baseTilemap.GetCellCenterWorld(new Vector3Int(current.x, current.y, 0));
            Vector2 safeCenter = new Vector2(safeCenter3.x, safeCenter3.y);
            return (targetWorldPosition - safeCenter).sqrMagnitude <
                   (currentWorldPosition - safeCenter).sqrMagnitude;
        }

        public bool CanPlayerMoveWorld(
            Vector3 currentWorldPosition, Vector3 targetWorldPosition, Vector2 halfExtents)
        {
            if (gridData == null) return true;
            Vector2Int current = WorldToCell(currentWorldPosition);
            if (!gridData.IsInside(current.x, current.y)) return false;
            int currentHeight = gridData.GetCell(current.x, current.y).HeightLevel;
            halfExtents.x = Mathf.Max(0f, halfExtents.x);
            halfExtents.y = Mathf.Max(0f, halfExtents.y);

            Vector3 axisY = useXZWorld ? Vector3.forward : Vector3.up;
            int targetInvalid = CountInvalidWorldSamples(
                targetWorldPosition, halfExtents, currentHeight, axisY);
            if (targetInvalid == 0) return true;

            int currentInvalid = CountInvalidWorldSamples(
                currentWorldPosition, halfExtents, currentHeight, axisY);
            if (currentInvalid <= 0 || targetInvalid > currentInvalid) return false;
            if (targetInvalid < currentInvalid) return true;

            Vector3 safeCenter = GetCellCenterWorld(current);
            return (targetWorldPosition - safeCenter).sqrMagnitude <
                   (currentWorldPosition - safeCenter).sqrMagnitude;
        }

        private int CountInvalidWorldSamples(
            Vector3 center, Vector2 halfExtents, int requiredHeight, Vector3 secondAxis)
        {
            Vector3 axisX = Vector3.right * halfExtents.x;
            Vector3 axisY = secondAxis * halfExtents.y;
            int invalid = IsValidPlayerWorldSample(center, requiredHeight) ? 0 : 1;
            if (!IsValidPlayerWorldSample(center - axisX - axisY, requiredHeight)) invalid++;
            if (!IsValidPlayerWorldSample(center + axisX - axisY, requiredHeight)) invalid++;
            if (!IsValidPlayerWorldSample(center - axisX + axisY, requiredHeight)) invalid++;
            if (!IsValidPlayerWorldSample(center + axisX + axisY, requiredHeight)) invalid++;
            return invalid;
        }

        private bool IsValidPlayerWorldSample(Vector3 worldPosition, int requiredHeight)
        {
            Vector2Int target = WorldToCell(worldPosition);
            if (!gridData.IsInside(target.x, target.y)) return false;
            MapCell cell = gridData.GetCell(target.x, target.y);
            bool unsafeLava = cell.HasLava && !cell.HasStomachRock;
            return !cell.IsVoid && !cell.HasCliff && !unsafeLava &&
                   cell.HeightLevel == requiredHeight;
        }

        public int GetHeightLevelAtWorld(Vector3 worldPosition)
        {
            if (gridData == null) return 0;
            Vector2Int cell = WorldToCell(worldPosition);
            return gridData.IsInside(cell.x, cell.y)
                ? gridData.GetCell(cell.x, cell.y).HeightLevel
                : 0;
        }

        public Vector3 GetPlayerSpawnWorldPosition()
        {
            if (gridData == null)
                return GetCellCenterWorld(new Vector2Int(mapWidth / 2, Mathf.Min(8, mapHeight - 1)));

            // 보스 아레나는 맵 중앙에 생성되므로 입장 지점은 아래쪽에 둡니다.
            // 장애물, 용암, 절벽 및 다른 층을 피하면서 가장 가까운 안전 셀을 찾습니다.
            Vector2Int entrance = new Vector2Int(
                mapWidth / 2,
                Mathf.Clamp(edgeMargin + 6, 0, mapHeight - 1));
            int maxRadius = Mathf.Max(mapWidth, mapHeight);
            for (int radius = 0; radius < maxRadius; radius++)
            {
                for (int y = entrance.y - radius; y <= entrance.y + radius; y++)
                for (int x = entrance.x - radius; x <= entrance.x + radius; x++)
                {
                    if (!gridData.IsInside(x, y)) continue;
                    MapCell cell = gridData.GetCell(x, y);
                    if (!cell.IsVoid && !cell.HasCliff && !cell.HasLava &&
                        !cell.Occupied && cell.HeightLevel == 0)
                        return GetCellCenterWorld(new Vector2Int(x, y));
                }
            }
            return GetCellCenterWorld(entrance);
        }

        private int CountInvalidPlayerSamples(Vector2 center, Vector2 halfExtents, int requiredHeight)
        {
            int invalid = IsValidPlayerSample(center, requiredHeight) ? 0 : 1;
            if (!IsValidPlayerSample(center + new Vector2(-halfExtents.x, -halfExtents.y), requiredHeight)) invalid++;
            if (!IsValidPlayerSample(center + new Vector2( halfExtents.x, -halfExtents.y), requiredHeight)) invalid++;
            if (!IsValidPlayerSample(center + new Vector2(-halfExtents.x,  halfExtents.y), requiredHeight)) invalid++;
            if (!IsValidPlayerSample(center + new Vector2( halfExtents.x,  halfExtents.y), requiredHeight)) invalid++;
            return invalid;
        }

        private bool IsValidPlayerSample(Vector2 worldPosition, int requiredHeight)
        {
            Vector2Int target = WorldToCell(worldPosition);
            if (!gridData.IsInside(target.x, target.y)) return false;
            MapCell cell = gridData.GetCell(target.x, target.y);
            bool unsafeLava = cell.HasLava && !cell.HasStomachRock;
            return !cell.IsVoid && !cell.HasCliff && !unsafeLava && cell.HeightLevel == requiredHeight;
        }

        public bool TryGetLavaJumpDestination(
            Vector2 worldPosition, Vector2 facingDirection, out Vector2 destination)
        {
            destination = default;
            if (gridData == null) return false;
            Vector2Int facing = Mathf.Abs(facingDirection.y) >= Mathf.Abs(facingDirection.x)
                ? new Vector2Int(0, facingDirection.y >= 0f ? 1 : -1)
                : new Vector2Int(facingDirection.x >= 0f ? 1 : -1, 0);
            Vector2Int current = WorldToCell(worldPosition);
            Vector2Int lava = current + facing;
            Vector2Int landing = current + facing * 2;
            if (!gridData.IsInside(current.x, current.y) ||
                !gridData.IsInside(lava.x, lava.y) ||
                !gridData.IsInside(landing.x, landing.y)) return false;

            MapCell currentCell = gridData.GetCell(current.x, current.y);
            MapCell lavaCell = gridData.GetCell(lava.x, lava.y);
            MapCell landingCell = gridData.GetCell(landing.x, landing.y);
            bool currentSafe = !currentCell.IsVoid && !currentCell.HasCliff &&
                               (!currentCell.HasLava || currentCell.HasStomachRock);
            bool gapIsLava = lavaCell.HasLava && !lavaCell.HasStomachRock;
            bool landingSafe = !landingCell.IsVoid && !landingCell.HasCliff &&
                               (!landingCell.HasLava || landingCell.HasStomachRock);
            if (!currentSafe || !gapIsLava || !landingSafe ||
                currentCell.HeightLevel != landingCell.HeightLevel) return false;

            // 첫 안전 칸의 정중앙에 착지하면 넓은 플레이어 판정 일부가
            // 뒤쪽 용암에 남아 돌아오는 점프 후 끼일 수 있다.
            // ROCK과 일반 땅 모두 진행 방향 안쪽으로 조금 더 착지시킨다.
            Vector2 landingCenter = new Vector2(landing.x + 0.5f, landing.y + 0.5f);
            landingCenter += (Vector2)facing * 0.22f;
            destination = landingCenter;
            return true;
        }

        public bool TryGetLavaJumpDestinationWorld(
            Vector3 worldPosition, Vector3 facingDirection, out Vector3 destination)
        {
            destination = default;
            Vector2Int facing = WorldFacingToCellDirection(facingDirection);
            Vector2Int current = WorldToCell(worldPosition);
            Vector2Int gapPosition = current + facing;
            Vector2Int landingPosition = current + facing * 2;
            if (!TryValidateLavaJump(current, gapPosition, landingPosition)) return false;

            destination = GetCellCenterWorld(landingPosition);
            destination += CellDirectionToWorld(facing) * 0.22f;
            return true;
        }

        public bool TryGetClimbDestination(
            Vector2 worldPosition, Vector2 facingDirection, out Vector2 destination, out bool ascending)
        {
            destination = default;
            ascending = false;
            if (gridData == null) return false;

            Vector2Int facing = Mathf.Abs(facingDirection.y) >= Mathf.Abs(facingDirection.x)
                ? new Vector2Int(0, facingDirection.y >= 0f ? 1 : -1)
                : new Vector2Int(facingDirection.x >= 0f ? 1 : -1, 0);

            Vector2Int current = WorldToCell(worldPosition);
            Vector2Int adjacent = current + facing;
            if (!gridData.IsInside(current.x, current.y) || !gridData.IsInside(adjacent.x, adjacent.y))
                return false;
            MapCell currentCell = gridData.GetCell(current.x, current.y);

            Vector2Int landing;
            if (gridData.GetCell(adjacent.x, adjacent.y).HasCliff)
            {
                // 아래쪽 절벽 면은 한 셀을 차지하므로 그 너머까지 두 칸 이동한다.
                landing = current + facing * 2;
            }
            else
            {
                // 절벽 Sprite가 없는 직접 높이 경계는 방향과 관계없이 바로 인접한 셀로 이동한다.
                landing = adjacent;
            }

            if (!gridData.IsInside(landing.x, landing.y)) return false;
            MapCell landingCell = gridData.GetCell(landing.x, landing.y);
            if (landingCell.IsVoid || landingCell.HasCliff) return false;

            int heightDifference = landingCell.HeightLevel - currentCell.HeightLevel;
            ascending = heightDifference == 1;
            bool descending = heightDifference == -1;
            // 한 번에 한 층만 이동한다. 따라서 3층(2)에서 1층(0)으로 직접 내려갈 수 없다.
            if (!ascending && !descending) return false;

            destination = new Vector2(landing.x + 0.5f, landing.y + 0.5f);
            return true;
        }

        public bool TryGetClimbDestinationWorld(
            Vector3 worldPosition, Vector3 facingDirection,
            out Vector3 destination, out bool ascending)
        {
            destination = default;
            ascending = false;
            if (gridData == null) return false;
            Vector2Int facing = WorldFacingToCellDirection(facingDirection);
            Vector2Int current = WorldToCell(worldPosition);
            Vector2Int adjacent = current + facing;
            if (!gridData.IsInside(current.x, current.y) ||
                !gridData.IsInside(adjacent.x, adjacent.y)) return false;

            Vector2Int landing = gridData.GetCell(adjacent.x, adjacent.y).HasCliff
                ? current + facing * 2
                : adjacent;
            if (!gridData.IsInside(landing.x, landing.y)) return false;
            MapCell currentCell = gridData.GetCell(current.x, current.y);
            MapCell landingCell = gridData.GetCell(landing.x, landing.y);
            if (landingCell.IsVoid || landingCell.HasCliff) return false;

            int difference = landingCell.HeightLevel - currentCell.HeightLevel;
            ascending = difference == 1;
            if (!ascending && difference != -1) return false;
            destination = GetCellCenterWorld(landing);
            return true;
        }

        private bool TryValidateLavaJump(
            Vector2Int current, Vector2Int gapPosition, Vector2Int landingPosition)
        {
            if (gridData == null || !gridData.IsInside(current.x, current.y) ||
                !gridData.IsInside(gapPosition.x, gapPosition.y) ||
                !gridData.IsInside(landingPosition.x, landingPosition.y)) return false;
            MapCell currentCell = gridData.GetCell(current.x, current.y);
            MapCell gap = gridData.GetCell(gapPosition.x, gapPosition.y);
            MapCell landing = gridData.GetCell(landingPosition.x, landingPosition.y);
            bool currentSafe = !currentCell.IsVoid && !currentCell.HasCliff &&
                               (!currentCell.HasLava || currentCell.HasStomachRock);
            bool gapUnsafe = gap.HasLava && !gap.HasStomachRock;
            bool landingSafe = !landing.IsVoid && !landing.HasCliff &&
                               (!landing.HasLava || landing.HasStomachRock);
            return currentSafe && gapUnsafe && landingSafe &&
                   currentCell.HeightLevel == landing.HeightLevel;
        }

        private Vector2Int WorldToCell(Vector2 worldPosition)
        {
            Vector3Int cell = baseTilemap.WorldToCell(worldPosition);
            return new Vector2Int(cell.x, cell.y);
        }

        private Vector2Int WorldToCell(Vector3 worldPosition)
        {
            Vector3Int cell = baseTilemap.WorldToCell(worldPosition);
            return new Vector2Int(cell.x, cell.y);
        }

        private Vector3 GetCellCenterWorld(Vector2Int cell)
        {
            return baseTilemap.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
        }

        private Vector2Int WorldFacingToCellDirection(Vector3 facing)
        {
            Vector3 local = baseTilemap.transform.InverseTransformDirection(facing);
            return Mathf.Abs(local.y) >= Mathf.Abs(local.x)
                ? new Vector2Int(0, local.y >= 0f ? 1 : -1)
                : new Vector2Int(local.x >= 0f ? 1 : -1, 0);
        }

        private Vector3 CellDirectionToWorld(Vector2Int direction)
        {
            return baseTilemap.transform.TransformDirection(
                new Vector3(direction.x, direction.y, 0f)).normalized;
        }

        private void Start()
        {
            if (generateOnStart) GenerateMap();
        }

        private void Update()
        {
            if (gridData == null) return;
            if (!player)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject) player = playerObject.transform;
            }

            Vector3 fallback = GetCellCenterWorld(new Vector2Int(mapWidth / 2, mapHeight / 2));
            Vector2Int nextChunk = WorldToChunk(player ? player.position : fallback);
            if (nextChunk != currentPlayerChunk) RefreshVisibleChunks(nextChunk);
        }

        private void OnDestroy()
        {
            ReleaseRuntimeTiles();
        }

        [ContextMenu("Generate Map")]
        public void GenerateMap()
        {
            ClampSettings();
            if (!ValidateRequiredReferences()) return;

            ClearMap();
            gridData = new GridData(mapWidth, mapHeight);
            occupancyGrid = new OccupancyGrid(mapWidth, mapHeight);
            thirdFloorOccupancyGrid = new OccupancyGrid(mapWidth, mapHeight);
            ReserveBottomBoundaryCliff();
            ReserveBossArenaFootprint();
            if (generateRoads) GenerateRoads(new System.Random(unchecked(randomSeed ^ 0x2A6F91C3)));
            ConfigureTilemapOrder();
            CreateRuntimeTiles();

            GenerateSecondFloorShapes(new System.Random(unchecked(randomSeed * 397 ^ 0x51ED270B)));
            if (generateThirdFloor)
                GenerateThirdFloorShapes(new System.Random(unchecked(randomSeed ^ 0x73A4C12D)));
            // 2층과 절벽이 차지한 공간을 피해서 1층에만 잔디를 생성한다.
            if (generateGrass) GenerateGrassShapes(new System.Random(randomSeed));
            if (generateLava) GenerateLavaShapes(new System.Random(unchecked(randomSeed ^ 0x2C9277B5)));
            if (generateLava) GenerateStomachRockShapes(new System.Random(unchecked(randomSeed ^ 0x41C64E6D)));
            BuildObstaclePlan();
            currentPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);
            Vector3 focus = player ? player.position : GetCellCenterWorld(new Vector2Int(mapWidth / 2, mapHeight / 2));
            RefreshVisibleChunks(WorldToChunk(focus));
            // 맵 최하단 경계는 멀리 있는 청크라도 항상 한 줄 전체를 표시한다.
            RenderBottomBoundaryCliff();
        }

        [ContextMenu("Clear Map")]
        public void ClearMap()
        {
            ClearTilemap(baseTilemap);
            ClearTilemap(grassTilemap);
            ClearTilemap(secondFloorTilemap);
            ClearTilemap(cliffTilemap);
            ClearTilemap(thirdFloorTilemap);
            ClearTilemap(thirdFloorCliffTilemap);
            ClearTilemap(roadTilemap);
            ClearTilemap(lavaTilemap);
            ClearTilemap(stomachRockTilemap);
            ClearGeneratedObstacles();
            activeChunks.Clear();
            obstaclePlanByChunk.Clear();
            chunkObstacleInstances.Clear();
            currentPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);
            gridData = null;
            occupancyGrid = null;
            thirdFloorOccupancyGrid = null;
            ReleaseRuntimeTiles();
        }

        private void FillBaseTilemap()
        {
            // SetTilesBlock으로 한 번에 배치해 셀별 GameObject 생성을 피한다.
            int baseStartY = bottomEmptyRows + 1;
            int baseHeight = mapHeight - baseStartY;
            if (baseHeight <= 0) return;

            var bounds = new BoundsInt(0, baseStartY, 0, mapWidth, baseHeight, 1);
            var tiles = new TileBase[mapWidth * baseHeight];
            Array.Fill(tiles, baseRuntimeTile);
            baseTilemap.SetTilesBlock(bounds, tiles);
        }

        private void GenerateRoads(System.Random random)
        {
            int placed = 0;
            int maxAttempts = roadPathCount * placementAttemptsPerArea;
            for (int attempt = 0; attempt < maxAttempts && placed < roadPathCount; attempt++)
            {
                // 현재 8종 세트에는 교차로 타일이 없으므로 좌우를 잇는 독립 경로만 만든다.
                List<Vector2Int> path = LungRoadGenerator.Create(
                    random, mapWidth, mapHeight, roadEdgeMargin, roadTurnChance, true,
                    placed == 0 ? mapHeight / 2 : -1);
                if (path == null || path.Count < 2) continue;
                HashSet<Vector2Int> roadArea = LungRoadGenerator.BuildRibbon(path, mapWidth, mapHeight);
                var roadCells = new List<Vector2Int>(roadArea);
                if (!occupancyGrid.CanPlace(roadCells, roadTerrainClearance)) continue;

                occupancyGrid.Occupy(roadCells);
                foreach (Vector2Int p in roadArea)
                {
                    MapCell cell = gridData.GetCell(p.x, p.y);
                    cell.HasRoad = true;
                    cell.RoadKind = LungRoadGenerator.Resolve(roadArea, p);
                }
                placed++;
            }

            if (placed < roadPathCount)
                Debug.LogWarning($"폐 길 {roadPathCount}개 중 {placed}개만 겹치지 않게 배치했습니다.", this);
        }

        private void ReserveBottomBoundaryCliff()
        {
            int cliffY = bottomEmptyRows;
            var reservedRows = new List<Vector2Int>(mapWidth * (cliffY + 1));
            for (int y = 0; y <= cliffY; y++)
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2Int position = new Vector2Int(x, y);
                reservedRows.Add(position);
                MapCell cell = gridData.GetCell(x, y);
                cell.IsVoid = y < cliffY;
                cell.HasCliff = y == cliffY;
                cell.CliffLevel = y == cliffY ? 1 : 0;
            }
            occupancyGrid.Occupy(reservedRows);
        }

        private void ReserveBossArenaFootprint()
        {
            if (!hasBossArenaReservation || gridData == null || occupancyGrid == null)
            {
                return;
            }

            GetBossArenaReservationBounds(out int minX, out int minY, out int maxX, out int maxY);
            var reservedCells = new List<Vector2Int>((maxX - minX + 1) * (maxY - minY + 1));
            for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                if (!gridData.IsInside(x, y))
                {
                    continue;
                }

                MapCell cell = gridData.GetCell(x, y);
                cell.Reset();
                cell.Occupied = true;
                reservedCells.Add(new Vector2Int(x, y));
            }

            occupancyGrid.Occupy(reservedCells);
            thirdFloorOccupancyGrid?.Occupy(reservedCells);
        }

        private void GetBossArenaReservationBounds(
            out int minX, out int minY, out int maxX, out int maxY)
        {
            minX = Mathf.Max(0, bossArenaCenter.x - bossArenaSize.x / 2 - bossArenaPadding);
            minY = Mathf.Max(0, bossArenaCenter.y - bossArenaSize.y / 2 - bossArenaPadding);
            maxX = Mathf.Min(
                mapWidth - 1,
                bossArenaCenter.x - bossArenaSize.x / 2 + bossArenaSize.x - 1 + bossArenaPadding);
            maxY = Mathf.Min(
                mapHeight - 1,
                bossArenaCenter.y - bossArenaSize.y / 2 + bossArenaSize.y - 1 + bossArenaPadding);
        }

        private void GenerateGrassShapes(System.Random random)
        {
            int placed = 0;
            int maxAttempts = grassAreaCount * placementAttemptsPerArea;
            int grassTypeCount = useMultipleGrassTypes ? grassTypeTileSets.Count : 1;

            for (int attempt = 0; attempt < maxAttempts && placed < grassAreaCount; attempt++)
            {
                List<Vector2Int> shape = RectangularShapeGenerator.CreateRandom(
                    random, mapWidth, mapHeight, grassMinSize, grassMaxSize, edgeMargin, grassExtraShapeParts);

                // Occupancy에는 2층 본체와 절벽이 들어 있다. 잔디끼리는 점유하지 않아 서로 합쳐질 수 있다.
                if (shape == null || shape.Count <= 1 || !occupancyGrid.CanPlace(shape, terrainSpacing))
                    continue;

                int variant = grassTypeCount > 1 ? random.Next(grassTypeCount) : 0;
                for (int i = 0; i < shape.Count; i++)
                {
                    Vector2Int position = shape[i];
                    MapCell cell = gridData.GetCell(position.x, position.y);
                    cell.TerrainType = TerrainType.Grass;
                    cell.HasGrass = true;
                    cell.GrassVariant = variant;
                    cell.Occupied = true;
                }
                // 이후 생성되는 모든 잔디 Shape가 이 영역 및 설정한 간격을 침범하지 못한다.
                occupancyGrid.Occupy(shape);
                placed++;
            }

            // 잔디끼리는 겹칠 수 있지만 이후 생성되는 2층과는 겹치지 않도록 최종 영역을 점유한다.
            if (placed < grassAreaCount)
                Debug.LogWarning($"잔디 영역 {grassAreaCount}개 중 {placed}개만 배치했습니다. 맵 크기, 간격 또는 시도 횟수를 확인하세요.", this);
        }

        private void GenerateLavaShapes(System.Random random)
        {
            int placed = 0;
            int maxAttempts = lavaAreaCount * placementAttemptsPerArea;
            for (int attempt = 0; attempt < maxAttempts && placed < lavaAreaCount; attempt++)
            {
                List<Vector2Int> shape = RectangularShapeGenerator.CreateRandom(
                    random, mapWidth, mapHeight, lavaMinSize, lavaMaxSize, edgeMargin, lavaExtraShapeParts);
                if (shape == null || shape.Count <= 1 || !occupancyGrid.CanPlace(shape, terrainSpacing))
                    continue;

                for (int i = 0; i < shape.Count; i++)
                {
                    Vector2Int position = shape[i];
                    MapCell cell = gridData.GetCell(position.x, position.y);
                    if (cell.HeightLevel != 0 || cell.HasCliff || cell.IsVoid) continue;
                    cell.HasLava = true;
                    cell.HasGrass = false;
                    if (cell.TerrainType == TerrainType.Grass) cell.TerrainType = TerrainType.Base;
                }
                placed++;
            }

            if (placed < lavaAreaCount)
                Debug.LogWarning($"용암 영역 {lavaAreaCount}개 중 {placed}개만 배치했습니다.", this);
        }

        private void GenerateStomachRockShapes(System.Random random)
        {
            var lavaCells = new List<Vector2Int>();
            for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                if (gridData.GetCell(x, y).HasLava)
                    lavaCells.Add(new Vector2Int(x, y));

            if (lavaCells.Count == 0) return;
            int placed = 0;
            int maxAttempts = stomachRockAreaCount * placementAttemptsPerArea * 4;
            for (int attempt = 0; attempt < maxAttempts && placed < stomachRockAreaCount; attempt++)
            {
                List<Vector2Int> shape = CreateStomachRockCandidate(random, lavaCells);
                if (shape == null || shape.Count <= 1 || !CanPlaceStomachRock(shape)) continue;

                for (int i = 0; i < shape.Count; i++)
                    gridData.GetCell(shape[i].x, shape[i].y).HasStomachRock = true;
                placed++;
            }

            if (placed < stomachRockAreaCount)
                Debug.LogWarning($"Stomach Rock 영역 {stomachRockAreaCount}개 중 {placed}개만 용암 위에 배치했습니다.", this);
        }

        private List<Vector2Int> CreateStomachRockCandidate(
            System.Random random, List<Vector2Int> lavaCells)
        {
            Vector2Int anchor = lavaCells[random.Next(lavaCells.Count)];
            int width = random.Next(stomachRockMinSize.x, stomachRockMaxSize.x + 1);
            int height = random.Next(stomachRockMinSize.y, stomachRockMaxSize.y + 1);
            int originX = anchor.x - random.Next(width);
            int originY = anchor.y - random.Next(height);
            var cells = new HashSet<Vector2Int>();
            AddRockRectangle(cells, originX, originY, width, height);

            // 본체에 작은 직사각형을 이어 붙여 T/L/계단형 ROCK도 섞는다.
            for (int part = 0; part < stomachRockExtraShapeParts; part++)
            {
                int partWidth = random.Next(2, Mathf.Max(3, width / 2 + 1));
                int partHeight = random.Next(2, Mathf.Max(3, height / 2 + 1));
                int direction = random.Next(4);
                int x = originX;
                int y = originY;
                if (direction == 0)
                {
                    x += random.Next(Mathf.Max(1, width - partWidth + 1));
                    y += height;
                }
                else if (direction == 1)
                {
                    x += random.Next(Mathf.Max(1, width - partWidth + 1));
                    y -= partHeight;
                }
                else if (direction == 2)
                {
                    x -= partWidth;
                    y += random.Next(Mathf.Max(1, height - partHeight + 1));
                }
                else
                {
                    x += width;
                    y += random.Next(Mathf.Max(1, height - partHeight + 1));
                }
                AddRockRectangle(cells, x, y, partWidth, partHeight);
            }
            return new List<Vector2Int>(cells);
        }

        private static void AddRockRectangle(
            HashSet<Vector2Int> cells, int originX, int originY, int width, int height)
        {
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                cells.Add(new Vector2Int(originX + x, originY + y));
        }

        private bool CanPlaceStomachRock(List<Vector2Int> shape)
        {
            var shapeSet = new HashSet<Vector2Int>(shape);
            for (int i = 0; i < shape.Count; i++)
            {
                Vector2Int p = shape[i];
                if (!gridData.IsInside(p.x, p.y)) return false;
                MapCell cell = gridData.GetCell(p.x, p.y);
                if (!cell.HasLava || cell.HasStomachRock || cell.HasCliff || cell.IsVoid) return false;

                // ROCK 바깥에는 최소 한 칸의 순수 용암을 남긴다.
                // 일반 땅에서 ROCK으로 바로 걸어 올라가는 상황과 대각선 접촉도 방지한다.
                for (int borderY = p.y - 1; borderY <= p.y + 1; borderY++)
                for (int borderX = p.x - 1; borderX <= p.x + 1; borderX++)
                {
                    var border = new Vector2Int(borderX, borderY);
                    if (shapeSet.Contains(border)) continue;
                    if (!gridData.IsInside(borderX, borderY)) return false;
                    MapCell borderCell = gridData.GetCell(borderX, borderY);
                    if (!borderCell.HasLava || borderCell.HasStomachRock ||
                        borderCell.HasCliff || borderCell.IsVoid)
                        return false;
                }

                for (int y = p.y - stomachRockSpacing; y <= p.y + stomachRockSpacing; y++)
                for (int x = p.x - stomachRockSpacing; x <= p.x + stomachRockSpacing; x++)
                    if (!shapeSet.Contains(new Vector2Int(x, y)) && gridData.IsInside(x, y) &&
                        gridData.GetCell(x, y).HasStomachRock)
                        return false;
            }
            return HasStomachRockJumpApproach(shapeSet);
        }

        private bool HasStomachRockJumpApproach(HashSet<Vector2Int> shape)
        {
            Vector2Int[] directions =
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };

            foreach (Vector2Int rockCell in shape)
            {
                for (int i = 0; i < directions.Length; i++)
                {
                    Vector2Int gapPosition = rockCell + directions[i];
                    Vector2Int takeoffPosition = rockCell + directions[i] * 2;
                    if (shape.Contains(gapPosition) ||
                        !gridData.IsInside(gapPosition.x, gapPosition.y) ||
                        !gridData.IsInside(takeoffPosition.x, takeoffPosition.y))
                        continue;

                    MapCell gap = gridData.GetCell(gapPosition.x, gapPosition.y);
                    MapCell takeoff = gridData.GetCell(takeoffPosition.x, takeoffPosition.y);
                    bool oneLavaGap = gap.HasLava && !gap.HasStomachRock;
                    bool safeTakeoff = !takeoff.HasLava && !takeoff.HasCliff &&
                                       !takeoff.IsVoid && takeoff.HeightLevel == 0;
                    if (oneLavaGap && safeTakeoff) return true;
                }
            }
            return false;
        }

        private void GenerateSecondFloorShapes(System.Random random)
        {
            if (!secondFloorTile || !cliffTile) return;

            int placed = 0;
            int maxAttempts = secondFloorAreaCount * placementAttemptsPerArea;
            for (int attempt = 0; attempt < maxAttempts && placed < secondFloorAreaCount; attempt++)
            {
                int targetCells = random.Next(secondFloorMinCells, secondFloorMaxCells + 1);
                List<Vector2Int> shape = SecondFloorShapeGenerator.Create(
                    random, mapWidth, mapHeight, targetCells, edgeMargin, secondFloorSmoothingIterations);

                if (shape == null || shape.Count < 4) continue;

                // 절벽은 2층의 한 칸 아래에 생기므로 본체와 예상 절벽 위치를 함께 검사한다.
                // 이 footprint를 점유하면 잔디-절벽 및 다른 2층-절벽의 겹침도 방지된다.
                List<Vector2Int> footprint = BuildSecondFloorFootprint(shape);
                if (!occupancyGrid.CanPlace(footprint, terrainSpacing)) continue;

                List<Vector2Int> cliffCells = BuildCliffCells(shape);
                for (int i = 0; i < cliffCells.Count; i++)
                {
                    Vector2Int cliffPosition = cliffCells[i];
                    ClearGrassDataAround(cliffPosition, cliffGrassClearance);
                    if (gridData.IsInside(cliffPosition.x, cliffPosition.y))
                    {
                        MapCell cliffCell = gridData.GetCell(cliffPosition.x, cliffPosition.y);
                        cliffCell.HasCliff = true;
                        cliffCell.CliffLevel = 1;
                    }
                }

                occupancyGrid.Occupy(footprint);
                for (int i = 0; i < shape.Count; i++)
                {
                    Vector2Int position = shape[i];
                    MapCell cell = gridData.GetCell(position.x, position.y);
                    cell.TerrainType = TerrainType.SecondFloor;
                    cell.Occupied = true;
                    cell.HeightLevel = 1;
                }
                placed++;
            }

            if (placed < secondFloorAreaCount)
                Debug.LogWarning($"2층 영역 {secondFloorAreaCount}개 중 {placed}개만 배치했습니다.", this);
        }

        private static List<Vector2Int> BuildSecondFloorFootprint(List<Vector2Int> shape)
        {
            var shapeSet = new HashSet<Vector2Int>(shape);
            var footprint = new HashSet<Vector2Int>(shapeSet);
            List<Vector2Int> cliffs = BuildCliffCells(shape);
            for (int i = 0; i < cliffs.Count; i++) footprint.Add(cliffs[i]);
            return new List<Vector2Int>(footprint);
        }

        private void GenerateThirdFloorShapes(System.Random random)
        {
            int placed = 0;
            int maxAttempts = thirdFloorAreaCount * placementAttemptsPerArea;
            for (int attempt = 0; attempt < maxAttempts && placed < thirdFloorAreaCount; attempt++)
            {
                int targetCells = random.Next(thirdFloorMinCells, thirdFloorMaxCells + 1);
                List<Vector2Int> shape = ThirdFloorShapeGenerator.Create(
                    random, gridData, targetCells, thirdFloorSmoothingIterations);
                if (shape == null || shape.Count < 4) continue;

                List<Vector2Int> cliffs = BuildCliffCells(shape);
                var footprintSet = new HashSet<Vector2Int>(shape);
                bool valid = true;
                for (int i = 0; i < cliffs.Count; i++)
                {
                    Vector2Int p = cliffs[i];
                    if (!gridData.IsInside(p.x, p.y)) { valid = false; break; }
                    MapCell cell = gridData.GetCell(p.x, p.y);
                    if (cell.HeightLevel != 1 || cell.HasCliff || cell.IsVoid)
                    { valid = false; break; }
                    footprintSet.Add(p);
                }
                if (!valid) continue;

                var footprint = new List<Vector2Int>(footprintSet);
                if (!thirdFloorOccupancyGrid.CanPlace(footprint, thirdFloorSpacing)) continue;
                thirdFloorOccupancyGrid.Occupy(footprint);

                for (int i = 0; i < shape.Count; i++)
                {
                    Vector2Int p = shape[i];
                    MapCell cell = gridData.GetCell(p.x, p.y);
                    cell.TerrainType = TerrainType.ThirdFloor;
                    cell.HeightLevel = 2;
                }
                for (int i = 0; i < cliffs.Count; i++)
                {
                    Vector2Int p = cliffs[i];
                    MapCell cell = gridData.GetCell(p.x, p.y);
                    cell.HasCliff = true;
                    cell.CliffLevel = 2;
                }
                placed++;
            }

            if (placed < thirdFloorAreaCount)
                Debug.LogWarning($"3층 영역 {thirdFloorAreaCount}개 중 {placed}개만 2층 안에 배치했습니다.", this);
        }

        private static List<Vector2Int> BuildCliffCells(List<Vector2Int> shape)
        {
            var shapeSet = new HashSet<Vector2Int>(shape);
            var cliffs = new HashSet<Vector2Int>();
            for (int i = 0; i < shape.Count; i++)
            {
                Vector2Int below = shape[i] + Vector2Int.down;
                if (!shapeSet.Contains(below)) cliffs.Add(below);
            }
            return new List<Vector2Int>(cliffs);
        }

        private void ClearGrassData(Vector2Int position)
        {
            if (!gridData.IsInside(position.x, position.y)) return;
            MapCell cell = gridData.GetCell(position.x, position.y);
            cell.HasGrass = false;
            cell.GrassVariant = -1;
            if (cell.TerrainType == TerrainType.Grass)
                cell.TerrainType = TerrainType.Base;
        }

        private void ClearGrassDataAround(Vector2Int center, int radius)
        {
            for (int y = center.y - radius; y <= center.y + radius; y++)
            for (int x = center.x - radius; x <= center.x + radius; x++)
                ClearGrassData(new Vector2Int(x, y));
        }

        private void RenderSecondFloorAndCliffs()
        {
            if (!secondFloorRuntimeTile || !cliffRuntimeTile) return;

            for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
            {
                if (!gridData.IsTerrain(x, y, TerrainType.SecondFloor)) continue;
                secondFloorTilemap.SetTile(new Vector3Int(x, y, 0), secondFloorRuntimeTile);

                // 현재 셀은 2층이고 바로 아래가 2층이 아닐 때만 절벽을 표시한다.
                if (!gridData.IsTerrain(x, y - 1, TerrainType.SecondFloor))
                {
                    cliffTilemap.SetTile(new Vector3Int(x, y - 1, 0), cliffRuntimeTile);
                }
            }
        }

        private void RenderBottomBoundaryCliff()
        {
            if (!cliffRuntimeTile) return;
            int cliffY = bottomEmptyRows;
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y <= cliffY; y++)
                {
                    baseTilemap.SetTile(new Vector3Int(x, y, 0), null);
                    grassTilemap.SetTile(new Vector3Int(x, y, 0), null);
                    secondFloorTilemap.SetTile(new Vector3Int(x, y, 0), null);
                    cliffTilemap.SetTile(new Vector3Int(x, y, 0), y == cliffY ? cliffRuntimeTile : null);
                }
            }
        }

        private void RenderGrass()
        {
            for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
            {
                if (!gridData.HasGrassAt(x, y)) continue;
                int variant = gridData.GetCell(x, y).GrassVariant;
                grassTilemap.SetTile(new Vector3Int(x, y, 0), GetGrassRuntimeTile(x, y, variant));
            }
        }

        private Vector2Int WorldToChunk(Vector3 worldPosition)
        {
            Vector2Int cell = WorldToCell(worldPosition);
            return new Vector2Int(cell.x / chunkSize, cell.y / chunkSize);
        }

        private Vector2Int GridPositionToChunk(Vector3 gridPosition)
        {
            return new Vector2Int(
                Mathf.FloorToInt(gridPosition.x / chunkSize),
                Mathf.FloorToInt(gridPosition.y / chunkSize));
        }

        [ContextMenu("Refresh Visible Chunks")]
        private void RefreshVisibleChunksNow()
        {
            if (gridData == null) return;
            Vector3 focus = player ? player.position : GetCellCenterWorld(new Vector2Int(mapWidth / 2, mapHeight / 2));
            currentPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);
            RefreshVisibleChunks(WorldToChunk(focus));
        }

        private void RefreshVisibleChunks(Vector2Int center)
        {
            int chunkCountX = Mathf.CeilToInt(mapWidth / (float)chunkSize);
            int chunkCountY = Mathf.CeilToInt(mapHeight / (float)chunkSize);

            for (int y = center.y - loadRadius; y <= center.y + loadRadius; y++)
            for (int x = center.x - loadRadius; x <= center.x + loadRadius; x++)
            {
                if (x < 0 || x >= chunkCountX || y < 0 || y >= chunkCountY) continue;
                Vector2Int coordinate = new Vector2Int(x, y);
                if (!activeChunks.Contains(coordinate)) LoadChunk(coordinate);
            }

            var unloadTargets = new List<Vector2Int>();
            foreach (Vector2Int coordinate in activeChunks)
            {
                int distance = Mathf.Max(Mathf.Abs(coordinate.x - center.x), Mathf.Abs(coordinate.y - center.y));
                if (distance > unloadRadius) unloadTargets.Add(coordinate);
            }
            for (int i = 0; i < unloadTargets.Count; i++) UnloadChunk(unloadTargets[i]);
            currentPlayerChunk = center;
        }

        private void LoadChunk(Vector2Int coordinate)
        {
            int startX = coordinate.x * chunkSize;
            int startY = coordinate.y * chunkSize;
            int endX = Mathf.Min(startX + chunkSize, mapWidth);
            int endY = Mathf.Min(startY + chunkSize, mapHeight);

            for (int y = startY; y < endY; y++)
            for (int x = startX; x < endX; x++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                MapCell cell = gridData.GetCell(x, y);
                if (!cell.IsVoid && y > bottomEmptyRows)
                    baseTilemap.SetTile(tilePosition, baseRuntimeTile);
                if (cell.HasRoad && cell.RoadKind != RoadTileKind.None && roadTilemap)
                    roadTilemap.SetTile(tilePosition, roadRuntimeTiles[(int)cell.RoadKind - 1]);
                if (cell.HasGrass)
                {
                    int variant = cell.GrassVariant;
                    grassTilemap.SetTile(tilePosition, GetGrassRuntimeTile(x, y, variant));
                }
                if (cell.HasLava && lavaTilemap)
                    lavaTilemap.SetTile(tilePosition, lavaRuntimeTiles[TileResolver9.ResolveLava(gridData, x, y) - 1]);
                if (cell.HasStomachRock && stomachRockTilemap)
                    stomachRockTilemap.SetTile(tilePosition, stomachRockRuntimeTiles[TileResolver9.ResolveStomachRock(gridData, x, y) - 1]);
                if (cell.TerrainType == TerrainType.SecondFloor)
                    secondFloorTilemap.SetTile(tilePosition, secondFloorRuntimeTile);
                if (cell.TerrainType == TerrainType.ThirdFloor && thirdFloorTilemap)
                    thirdFloorTilemap.SetTile(tilePosition, thirdFloorRuntimeTile);
                if (cell.HasCliff)
                {
                    if (cell.CliffLevel == 2 && thirdFloorCliffTilemap)
                        thirdFloorCliffTilemap.SetTile(tilePosition, thirdFloorCliffRuntimeTile);
                    else
                        cliffTilemap.SetTile(tilePosition, cliffRuntimeTile);
                }
            }

            LoadChunkObstacles(coordinate);
            activeChunks.Add(coordinate);
        }

        private void UnloadChunk(Vector2Int coordinate)
        {
            int startX = coordinate.x * chunkSize;
            int startY = coordinate.y * chunkSize;
            int endX = Mathf.Min(startX + chunkSize, mapWidth);
            int endY = Mathf.Min(startY + chunkSize, mapHeight);
            for (int y = startY; y < endY; y++)
            for (int x = startX; x < endX; x++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                baseTilemap.SetTile(position, null);
                grassTilemap.SetTile(position, null);
                secondFloorTilemap.SetTile(position, null);
                // 최하단 300칸 경계 절벽은 청크가 언로드돼도 유지한다.
                if (y != bottomEmptyRows)
                cliffTilemap.SetTile(position, null);
                if (thirdFloorTilemap) thirdFloorTilemap.SetTile(position, null);
                if (thirdFloorCliffTilemap) thirdFloorCliffTilemap.SetTile(position, null);
                if (roadTilemap) roadTilemap.SetTile(position, null);
                if (lavaTilemap) lavaTilemap.SetTile(position, null);
                if (stomachRockTilemap) stomachRockTilemap.SetTile(position, null);
            }

            if (chunkObstacleInstances.TryGetValue(coordinate, out List<GameObject> instances))
            {
                for (int i = 0; i < instances.Count; i++) DestroyGeneratedObject(instances[i]);
                chunkObstacleInstances.Remove(coordinate);
            }
            activeChunks.Remove(coordinate);
        }

        private void BuildObstaclePlan()
        {
            obstaclePlanByChunk.Clear();
            var plan = new List<ObstacleSpawnData>();
            if (generateLava && lavaPopDefinitions != null)
                plan.AddRange(LavaPopGenerator.BuildPlan(
                    gridData, lavaPopDefinitions, unchecked(randomSeed ^ 0x19E3779B)));
            if (obstacleDefinitions != null)
                plan.AddRange(PerlinObstacleGenerator.BuildPlan(
                    gridData, obstacleDefinitions, unchecked(randomSeed ^ 0x6E624EB7)));
            for (int i = 0; i < plan.Count; i++)
            {
                ObstacleSpawnData spawn = plan[i];
                int spawnX = Mathf.FloorToInt(spawn.Position.x);
                int spawnY = Mathf.FloorToInt(spawn.Position.y);
                if (IsCellReservedForBossArena(spawnX, spawnY)) continue;
                Vector2Int coordinate = GridPositionToChunk(spawn.Position);
                if (!obstaclePlanByChunk.TryGetValue(coordinate, out List<ObstacleSpawnData> chunkPlan))
                {
                    chunkPlan = new List<ObstacleSpawnData>();
                    obstaclePlanByChunk.Add(coordinate, chunkPlan);
                }
                chunkPlan.Add(spawn);
            }
        }

        private void LoadChunkObstacles(Vector2Int coordinate)
        {
            if (!obstaclePlanByChunk.TryGetValue(coordinate, out List<ObstacleSpawnData> plan)) return;
            EnsureObstacleRoot();
            if (!objectPool) objectPool = ObjectPoolManager.GetOrCreate();
            var instances = new List<GameObject>(plan.Count);
            for (int i = 0; i < plan.Count; i++)
            {
                ObstacleSpawnData spawn = plan[i];
                Vector3 spawnPosition = useXZWorld
                    ? baseTilemap.transform.TransformPoint(spawn.Position)
                    : spawn.Position;
                GameObject instance = objectPool.Get(spawn.Prefab, spawnPosition, Quaternion.identity, obstacleRoot);
                instance.name = spawn.Name;
                if (spawn.FlipX)
                {
                    Vector3 scale = instance.transform.localScale;
                    scale.x *= -1f;
                    instance.transform.localScale = scale;
                }
                instances.Add(instance);
            }
            chunkObstacleInstances[coordinate] = instances;
        }

        private void EnsureObstacleRoot()
        {
            if (obstacleRoot) return;
            GameObject root = new GameObject("Generated Obstacles");
            root.transform.SetParent(transform, false);
            obstacleRoot = root.transform;
        }

        private void GenerateObstacles()
        {
            if (obstacleDefinitions == null || obstacleDefinitions.Count == 0) return;
            if (!obstacleRoot)
            {
                GameObject root = new GameObject("Generated Obstacles");
                root.transform.SetParent(transform, false);
                obstacleRoot = root.transform;
            }
            PerlinObstacleGenerator.Generate(
                gridData, obstacleDefinitions, unchecked(randomSeed ^ 0x6E624EB7),
                obstacleRoot, generatedObstacles);
        }

        private void DestroyGeneratedObject(GameObject target)
        {
            if (!target) return;
            PooledObjectIdentity identity = target.GetComponent<PooledObjectIdentity>();
            if (Application.isPlaying && identity)
            {
                if (!objectPool) objectPool = ObjectPoolManager.GetOrCreate();
                objectPool.Release(target);
                return;
            }
            if (Application.isPlaying) Destroy(target);
            else DestroyImmediate(target);
        }

        private void ClearGeneratedObstacles()
        {
            var targets = new HashSet<GameObject>();
            foreach (KeyValuePair<Vector2Int, List<GameObject>> pair in chunkObstacleInstances)
            {
                List<GameObject> instances = pair.Value;
                for (int i = 0; i < instances.Count; i++) targets.Add(instances[i]);
            }
            chunkObstacleInstances.Clear();

            for (int i = generatedObstacles.Count - 1; i >= 0; i--)
                targets.Add(generatedObstacles[i]);
            generatedObstacles.Clear();

            // 도메인 리로드 뒤 추적 목록이 사라진 경우에도 전용 Root의 이전 생성물을 정리한다.
            if (obstacleRoot)
            {
                for (int i = obstacleRoot.childCount - 1; i >= 0; i--)
                    targets.Add(obstacleRoot.GetChild(i).gameObject);
            }
            foreach (GameObject target in targets) DestroyGeneratedObject(target);
        }

        private void CreateRuntimeTiles()
        {
            baseRuntimeTile = CreateRuntimeTile(baseTile);
            if (generateGrass && useLungGrassTiles)
            {
                for (int i = 0; i < lungGrassRuntimeTiles.Length; i++)
                    lungGrassRuntimeTiles[i] = CreateRuntimeTile(lungGrassTiles.Get(i));
            }
            else if (generateGrass && useMultipleGrassTypes)
            {
                for (int type = 0; type < grassTypeTileSets.Count; type++)
                {
                    var tiles = new UnityEngine.Tilemaps.Tile[9];
                    for (int i = 0; i < tiles.Length; i++)
                        tiles[i] = CreateRuntimeTile(grassTypeTileSets[type].Get(i + 1));
                    grassTypeRuntimeTiles.Add(tiles);
                }
            }
            else if (generateGrass)
            {
                for (int i = 0; i < grassRuntimeTiles.Length; i++)
                    grassRuntimeTiles[i] = CreateRuntimeTile(grassTiles.Get(i + 1));
            }
            if (secondFloorTile) secondFloorRuntimeTile = CreateRuntimeTile(secondFloorTile);
            if (cliffTile) cliffRuntimeTile = CreateRuntimeTile(cliffTile);
            if (generateThirdFloor)
            {
                thirdFloorRuntimeTile = CreateRuntimeTile(thirdFloorTile);
                thirdFloorCliffRuntimeTile = CreateRuntimeTile(thirdFloorCliffTile);
            }
            if (generateRoads)
            {
                for (int i = 0; i < roadRuntimeTiles.Length; i++)
                    roadRuntimeTiles[i] = CreateRuntimeTile(roadTiles.Get((RoadTileKind)(i + 1)));
            }
            if (generateLava)
            {
                for (int i = 0; i < lavaRuntimeTiles.Length; i++)
                    lavaRuntimeTiles[i] = CreateRuntimeTile(lavaTiles.Get(i + 1));
                for (int i = 0; i < stomachRockRuntimeTiles.Length; i++)
                    stomachRockRuntimeTiles[i] = CreateRuntimeTile(stomachRockTiles.Get(i + 1));
            }
        }

        private TileBase GetGrassRuntimeTile(int x, int y, int variant)
        {
            if (useLungGrassTiles)
                return lungGrassRuntimeTiles[LungGrassResolver6.Resolve(gridData, x, y)];

            int tileNumber = TileResolver9.ResolveGrass(gridData, x, y, variant);
            int index = Mathf.Clamp(tileNumber - 1, 0, 8);
            if (useMultipleGrassTypes && variant >= 0 && variant < grassTypeRuntimeTiles.Count)
                return grassTypeRuntimeTiles[variant][index];
            return grassRuntimeTiles[index];
        }

        private UnityEngine.Tilemaps.Tile CreateRuntimeTile(Sprite sprite)
        {
            UnityEngine.Tilemaps.Tile tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
            tile.sprite = sprite;
            tile.hideFlags = HideFlags.HideAndDontSave;
            runtimeTiles.Add(tile);
            return tile;
        }

        private void ReleaseRuntimeTiles()
        {
            for (int i = 0; i < runtimeTiles.Count; i++)
            {
                if (!runtimeTiles[i]) continue;
                if (Application.isPlaying) Destroy(runtimeTiles[i]);
                else DestroyImmediate(runtimeTiles[i]);
            }
            runtimeTiles.Clear();
            baseRuntimeTile = null;
            Array.Clear(grassRuntimeTiles, 0, grassRuntimeTiles.Length);
            Array.Clear(lungGrassRuntimeTiles, 0, lungGrassRuntimeTiles.Length);
            grassTypeRuntimeTiles.Clear();
            secondFloorRuntimeTile = null;
            cliffRuntimeTile = null;
            thirdFloorRuntimeTile = null;
            thirdFloorCliffRuntimeTile = null;
            Array.Clear(roadRuntimeTiles, 0, roadRuntimeTiles.Length);
            Array.Clear(lavaRuntimeTiles, 0, lavaRuntimeTiles.Length);
            Array.Clear(stomachRockRuntimeTiles, 0, stomachRockRuntimeTiles.Length);
        }

        private bool ValidateRequiredReferences()
        {
            if (!baseTilemap || !grassTilemap || !secondFloorTilemap || !cliffTilemap || !baseTile)
            {
                Debug.LogError("Tilemap 4개와 기본 바닥 Sprite를 연결해야 합니다.", this);
                return false;
            }
            if (generateGrass && useLungGrassTiles)
            {
                if (!lungGrassTiles.IsComplete)
                {
                    Debug.LogError("폐 잔디 생성 시 Lung Grass Tiles 6종을 모두 연결해야 합니다.", this);
                    return false;
                }
            }
            else if (generateGrass && useMultipleGrassTypes)
            {
                if (grassTypeTileSets == null || grassTypeTileSets.Count == 0)
                {
                    Debug.LogError("Multiple Grass Types 사용 시 Grass Type Tile Sets를 연결해야 합니다.", this);
                    return false;
                }
                for (int i = 0; i < grassTypeTileSets.Count; i++)
                {
                    if (grassTypeTileSets[i] != null && grassTypeTileSets[i].IsComplete) continue;
                    Debug.LogError($"Grass Type Tile Sets의 Element {i}에 Tile 1~9를 모두 연결해야 합니다.", this);
                    return false;
                }
            }
            else if (generateGrass && !grassTiles.IsComplete)
            {
                Debug.LogError("Grass Tiles 1~9를 모두 연결해야 합니다.", this);
                return false;
            }
            if (generateLava && (!lavaTilemap || !lavaTiles.IsComplete ||
                                 !stomachRockTilemap || !stomachRockTiles.IsComplete))
            {
                Debug.LogError("용암 생성 시 Lava와 Stomach Rock Tilemap 및 각 Tiles 1~9를 모두 연결해야 합니다.", this);
                return false;
            }
            if (generateThirdFloor && (!thirdFloorTilemap || !thirdFloorCliffTilemap ||
                                       !thirdFloorTile || !thirdFloorCliffTile))
            {
                Debug.LogError("3층 생성 시 Third Floor/Cliff Tilemap과 각 Sprite를 연결해야 합니다.", this);
                return false;
            }
            if (generateRoads && (!roadTilemap || !roadTiles.IsComplete))
            {
                Debug.LogError("폐 길 생성 시 Road Tilemap과 Road Tiles 8종을 모두 연결해야 합니다.", this);
                return false;
            }
            return true;
        }

        private static void ClearTilemap(Tilemap tilemap)
        {
            if (tilemap) tilemap.ClearAllTiles();
        }

        private void ConfigureTilemapOrder()
        {
            SetSortingOrder(baseTilemap, 0);
            SetSortingOrder(secondFloorTilemap, 1);
            SetSortingOrder(grassTilemap, 2);
            SetSortingOrder(cliffTilemap, 3);
            if (lavaTilemap) SetSortingOrder(lavaTilemap, 2);
            if (stomachRockTilemap) SetSortingOrder(stomachRockTilemap, 3);
            if (thirdFloorTilemap) SetSortingOrder(thirdFloorTilemap, 4);
            if (thirdFloorCliffTilemap) SetSortingOrder(thirdFloorCliffTilemap, 5);
            if (roadTilemap) SetSortingOrder(roadTilemap, 2);
        }

        private static void SetSortingOrder(Tilemap tilemap, int order)
        {
            TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
            if (renderer) renderer.sortingOrder = order;
        }

        private void OnValidate() => ClampSettings();

        private void ClampSettings()
        {
            mapWidth = Mathf.Max(1, mapWidth);
            mapHeight = Mathf.Max(1, mapHeight);
            bottomEmptyRows = Mathf.Clamp(bottomEmptyRows, 0, Mathf.Max(0, mapHeight - 1));
            grassMinSize.x = Mathf.Max(2, grassMinSize.x);
            grassMinSize.y = Mathf.Max(2, grassMinSize.y);
            grassMaxSize.x = Mathf.Max(grassMinSize.x, grassMaxSize.x);
            grassMaxSize.y = Mathf.Max(grassMinSize.y, grassMaxSize.y);
            edgeMargin = Mathf.Max(0, edgeMargin);
            terrainSpacing = Mathf.Max(0, terrainSpacing);
            placementAttemptsPerArea = Mathf.Max(1, placementAttemptsPerArea);
            grassExtraShapeParts = Mathf.Max(0, grassExtraShapeParts);
            secondFloorMinCells = Mathf.Max(4, secondFloorMinCells);
            secondFloorMaxCells = Mathf.Max(secondFloorMinCells, secondFloorMaxCells);
            secondFloorSmoothingIterations = Mathf.Max(0, secondFloorSmoothingIterations);
            thirdFloorMinCells = Mathf.Max(4, thirdFloorMinCells);
            thirdFloorMaxCells = Mathf.Max(thirdFloorMinCells, thirdFloorMaxCells);
            thirdFloorSmoothingIterations = Mathf.Max(0, thirdFloorSmoothingIterations);
            thirdFloorSpacing = Mathf.Max(0, thirdFloorSpacing);
            roadPathCount = Mathf.Max(1, roadPathCount);
            roadEdgeMargin = Mathf.Max(1, roadEdgeMargin);
            roadTerrainClearance = Mathf.Max(0, roadTerrainClearance);
            cliffGrassClearance = Mathf.Max(0, cliffGrassClearance);
            lavaMinSize.x = Mathf.Max(2, lavaMinSize.x);
            lavaMinSize.y = Mathf.Max(2, lavaMinSize.y);
            lavaMaxSize.x = Mathf.Max(lavaMinSize.x, lavaMaxSize.x);
            lavaMaxSize.y = Mathf.Max(lavaMinSize.y, lavaMaxSize.y);
            stomachRockMinSize.x = Mathf.Max(2, stomachRockMinSize.x);
            stomachRockMinSize.y = Mathf.Max(2, stomachRockMinSize.y);
            stomachRockMaxSize.x = Mathf.Max(stomachRockMinSize.x, stomachRockMaxSize.x);
            stomachRockMaxSize.y = Mathf.Max(stomachRockMinSize.y, stomachRockMaxSize.y);
            stomachRockSpacing = Mathf.Max(0, stomachRockSpacing);
            chunkSize = Mathf.Max(1, chunkSize);
            loadRadius = Mathf.Max(0, loadRadius);
            unloadRadius = Mathf.Max(loadRadius, unloadRadius);
        }
    }
}
