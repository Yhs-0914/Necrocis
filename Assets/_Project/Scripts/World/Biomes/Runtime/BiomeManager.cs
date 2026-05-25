using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace Necrocis
{
    /// <summary>
    /// 바이옴 맵 관리 기본 클래스 (청크 기반 + Tilemap)
    /// </summary>
    public abstract partial class BiomeManager : MonoBehaviour
    {
        public static BiomeManager Active { get; private set; }

        [Header("맵 설정")]
        [SerializeField] protected int mapWidth;
        [SerializeField] protected int mapHeight;
        [SerializeField] protected int chunkSize;
        [SerializeField] protected float tileSize = 1f;

        [Header("생성 설정")]
        [SerializeField] protected int seed = 0;
        [SerializeField] protected bool useRandomSeed = true;

        private const int RandomSeedRange = 100000;
        private const int BiomeSeedBucketSize = 100000;
        private static readonly Dictionary<BiomeType, int> biomeSeedCache = new Dictionary<BiomeType, int>();

        [Header("청크 로딩 설정")]
        [SerializeField] protected int loadDistance;    // 플레이어 주변 로드할 청크 수
        [SerializeField] protected int unloadDistance;  // 언로드 거리
        [SerializeField] protected float chunkUpdateInterval;  // 청크 갱신 간격
        [SerializeField] protected bool destroyChunkRootOnUnload = true;

        [Header("오브젝트 로딩 설정")]
        [SerializeField] protected int objectGenerationBudget; // 프레임당 처리 예산

        [Header("Tilemap")]
        [SerializeField] protected Grid grid;
        [SerializeField] protected Transform tilesParent;
        [SerializeField] protected Transform objectsParent;
        [SerializeField] protected Transform pooledObjectsParent;

        [Header("청크 풀링")]
        [SerializeField] protected bool useChunkRootPooling = true;
        [SerializeField] protected int maxChunkRootPoolSize;
        [SerializeField] protected bool useCliffOverlayTilemaps = false;

        [Header("오브젝트 풀 제한")]
        [SerializeField] private int defaultMaxPoolSizePerType = 64;
        [SerializeField] private List<PoolLimit> poolLimits = new List<PoolLimit>();
        [SerializeField] private int maxTotalPoolSize = 0;

        [Header("높이 설정")]
        [SerializeField] protected bool enableHeight = true;
        [SerializeField] protected int minHeightLevel = -1;
        [SerializeField] protected int maxHeightLevel = 1;
        [SerializeField] protected int maxStepHeight = 1;
        [SerializeField] protected float heightStep = 0.5f;
        [SerializeField] protected float cliffOverlayOffset = 0.01f;
        [SerializeField] protected Color cliffTint = new Color(0.6f, 0.6f, 0.6f, 1f);
        [SerializeField] protected float playerHeightOffset = -2f;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;

        // 청크 정보
        protected int chunksX;
        protected int chunksY;
        protected Chunk[,] chunks;

        // 로드된 청크 추적
        protected HashSet<Vector2Int> loadedChunks = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> chunksToLoadCache = new HashSet<Vector2Int>();
        private readonly List<Vector2Int> chunksToUnloadCache = new List<Vector2Int>();
        protected Transform playerTransform;
        protected Vector2Int lastPlayerChunk = new Vector2Int(-999, -999);
        protected float chunkUpdateTimer = 0f;

        // 바이옴 타입 (하위 클래스에서 설정)
        protected BiomeType biomeType = BiomeType.None;

        // 로드된 청크 내 이동 불가 타일
        protected HashSet<Vector2Int> blockedCells = new HashSet<Vector2Int>();

        private readonly Dictionary<ObjectPoolKey, Stack<GameObject>> objectPool = new Dictionary<ObjectPoolKey, Stack<GameObject>>();
        private readonly Stack<GameObject> chunkRootPool = new Stack<GameObject>();
        private Transform pooledChunkRootsParent;
        private Dictionary<BiomeObjectKind, int> poolLimitLookup;
        private int pooledObjectCount;
        private readonly HashSet<Vector2Int> reportedResidualChunks = new HashSet<Vector2Int>();

        public int MapWidth => mapWidth;
        public int MapHeight => mapHeight;
        public int ChunkSize => chunkSize;
        public float TileSize => tileSize;
        public int Seed => seed;
        public BiomeType BiomeType => biomeType;
        public float HeightStep => heightStep;
        public int MinHeightLevel => minHeightLevel;
        public int MaxHeightLevel => maxHeightLevel;

        protected virtual void Awake()
        {
            Active = this;

            // 시드 설정
            if (useRandomSeed)
            {
                seed = GetOrCreateBiomeSeed(biomeType);
            }

            if (chunkSize <= 0)
            {
                Debug.LogError("[BiomeManager] chunkSize가 0 이하입니다. 인스펙터에서 설정하세요.");
                chunkSize = 1;
            }

            // 청크 수 계산
            chunksX = Mathf.CeilToInt((float)mapWidth / chunkSize);
            chunksY = Mathf.CeilToInt((float)mapHeight / chunkSize);

            Log($"[BiomeManager] 맵: {mapWidth}x{mapHeight}, 청크: {chunksX}x{chunksY} (각 {chunkSize}x{chunkSize})");
        }

        private static int GetOrCreateBiomeSeed(BiomeType biome)
        {
            if (biome == BiomeType.None)
            {
                return Random.Range(0, RandomSeedRange);
            }

            if (!biomeSeedCache.TryGetValue(biome, out int cachedSeed))
            {
                int baseSeed = Random.Range(0, RandomSeedRange);
                int offset = ((int)biome) * BiomeSeedBucketSize;
                cachedSeed = baseSeed + offset;
                biomeSeedCache[biome] = cachedSeed;
            }

            return cachedSeed;
        }

        protected virtual void Start()
        {
            Initialize();
            SetupPlayer();
        }

        protected virtual void Update()
        {
            if (playerTransform == null) return;

            chunkUpdateTimer += Time.deltaTime;
            if (chunkUpdateTimer >= chunkUpdateInterval)
            {
                chunkUpdateTimer = 0f;
                UpdateChunks();
            }
        }

        /// <summary>
        /// 초기화
        /// </summary>
        protected virtual void Initialize()
        {
            EnsureGrid();
            BuildPoolLimitLookup();

            // 부모 오브젝트 생성
            if (tilesParent == null)
            {
                GameObject tilesObj = new GameObject("Tiles");
                tilesObj.transform.SetParent(grid.transform, false);
                tilesParent = tilesObj.transform;
            }

            if (objectsParent == null)
            {
                GameObject objsObj = new GameObject("Objects");
                objsObj.transform.SetParent(transform, false);
                objectsParent = objsObj.transform;
            }

            if (pooledObjectsParent == null)
            {
                GameObject poolObj = new GameObject("PooledObjects");
                poolObj.transform.SetParent(objectsParent, false);
                pooledObjectsParent = poolObj.transform;
            }

            if (pooledChunkRootsParent == null)
            {
                GameObject poolObj = new GameObject("PooledChunkRoots");
                poolObj.transform.SetParent(tilesParent, false);
                pooledChunkRootsParent = poolObj.transform;
            }

            // 청크 초기화
            chunks = new Chunk[chunksX, chunksY];
            for (int cx = 0; cx < chunksX; cx++)
            {
                for (int cy = 0; cy < chunksY; cy++)
                {
                    chunks[cx, cy] = new Chunk(cx, cy, chunkSize);
                }
            }
        }

        private void EnsureGrid()
        {
            if (grid != null) return;

            GameObject gridObj = new GameObject("BiomeGrid");
            gridObj.transform.SetParent(transform, false);
            grid = gridObj.AddComponent<Grid>();
            grid.cellSize = new Vector3(tileSize, tileSize, tileSize);
            grid.cellGap = Vector3.zero;
        }

        /// <summary>
        /// 플레이어 설정
        /// </summary>
        protected virtual void SetupPlayer()
        {
            // 기존 플레이어 찾기
            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                player = FindFirstObjectByType<PlayerController>();
            }

            // 플레이어가 없으면 Hub에서 시작하라고 안내
            if (player == null)
            {
                Debug.LogError("[BiomeManager] 플레이어가 없습니다! Hub 씬에서 시작하세요.");
                // Hub로 이동
                if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.ReturnToHub();
                }
                return;
            }

            playerTransform = player.transform;
            Vector3 spawnPos = GetPlayerSpawnPosition();
            Vector2Int spawnGrid = WorldToGrid(spawnPos);
            float groundHeight = GetGroundHeight(spawnGrid.x, spawnGrid.y);
            spawnPos.y = groundHeight + playerHeightOffset;
            player.SpawnAt(spawnPos);
            player.LockY(playerHeightOffset);
            Log($"[BiomeManager] 플레이어 스폰: {spawnPos}");

            // 카메라 설정
            SetupCamera(playerTransform);

            // 초기 청크 로드
            UpdateChunks();

            WorldItemSpawner itemSpawner = GetComponent<WorldItemSpawner>();
            if (itemSpawner == null)
            {
                itemSpawner = gameObject.AddComponent<WorldItemSpawner>();
            }

            itemSpawner.SpawnItemsNow();
        }

        /// <summary>
        /// 카메라 설정
        /// </summary>
        protected virtual void SetupCamera(Transform target)
        {
            DontStarveCamera cam = DontStarveCamera.Instance;
            if (cam == null)
            {
                cam = FindFirstObjectByType<DontStarveCamera>();
            }

            if (cam != null)
            {
                cam.SetTarget(target);
                cam.SnapToTarget();
                Log("[BiomeManager] 카메라 타겟 설정 완료");
            }
            else
            {
                Debug.LogError("[BiomeManager] 카메라가 없습니다! Hub 씬의 Camera에 DontStarveCamera 스크립트가 있는지 확인하세요.");
            }
        }

        /// <summary>
        /// 청크 업데이트 (로드/언로드)
        /// </summary>
        protected virtual void UpdateChunks()
        {
            if (playerTransform == null) return;

            Vector2Int playerGrid = WorldToGrid(playerTransform.position);
            Vector2Int playerChunk = GridToChunk(playerGrid.x, playerGrid.y);

            if (playerChunk == lastPlayerChunk)
            {
                SweepOrphanedChunkObjects();
                return;
            }
            lastPlayerChunk = playerChunk;

            chunksToLoadCache.Clear();
            for (int dx = -loadDistance; dx <= loadDistance; dx++)
            {
                for (int dy = -loadDistance; dy <= loadDistance; dy++)
                {
                    int cx = playerChunk.x + dx;
                    int cy = playerChunk.y + dy;
                    if (IsValidChunk(cx, cy))
                    {
                        chunksToLoadCache.Add(new Vector2Int(cx, cy));
                    }
                }
            }

            chunksToUnloadCache.Clear();
            foreach (var chunkPos in loadedChunks)
            {
                int dist = Mathf.Max(Mathf.Abs(chunkPos.x - playerChunk.x), Mathf.Abs(chunkPos.y - playerChunk.y));
                if (dist > unloadDistance)
                {
                    chunksToUnloadCache.Add(chunkPos);
                }
            }

            // 언로드
            foreach (var chunkPos in chunksToUnloadCache)
            {
                UnloadChunk(chunkPos.x, chunkPos.y);
            }

            // 로드
            foreach (var chunkPos in chunksToLoadCache)
            {
                if (!loadedChunks.Contains(chunkPos))
                {
                    LoadChunk(chunkPos.x, chunkPos.y);
                }
            }

            SweepOrphanedChunkObjects();
        }

        /// <summary>
        /// 청크 로드
        /// </summary>
        protected virtual void LoadChunk(int chunkX, int chunkY)
        {
            if (!IsValidChunk(chunkX, chunkY)) return;

            Chunk chunk = chunks[chunkX, chunkY];

            // 이미 로드됨
            if (chunk.isLoaded) return;

            EnsureChunkRoot(chunk);

            // 타일/오브젝트 생성
            GenerateTiles(chunk);

            OnChunkLoaded(chunk);
            chunk.isLoaded = true;
            loadedChunks.Add(new Vector2Int(chunkX, chunkY));
            LoadChunkObjects(chunk);
            Log($"[BiomeManager] 청크 로드: ({chunkX}, {chunkY})");
        }

        /// <summary>
        /// 청크 언로드
        /// </summary>
        protected virtual void UnloadChunk(int chunkX, int chunkY)
        {
            if (!IsValidChunk(chunkX, chunkY)) return;

            Chunk chunk = chunks[chunkX, chunkY];
            if (!chunk.isLoaded) return;

            // 오브젝트 제거
            UnloadChunkObjects(chunk);

            // 타일 제거
            ClearChunkTilemaps(chunk);

            OnChunkUnloaded(chunk);

            if (useChunkRootPooling || destroyChunkRootOnUnload)
            {
                ReleaseChunkRoot(chunk);
            }

            chunk.isLoaded = false;
            loadedChunks.Remove(new Vector2Int(chunkX, chunkY));
            Log($"[BiomeManager] 청크 언로드: ({chunkX}, {chunkY})");
        }

        /// <summary>
        /// 타일 생성
        /// </summary>
        protected virtual void GenerateTiles(Chunk chunk)
        {
            if (chunk.tilemaps == null || chunk.tilemaps.Length == 0) return;

            int levelCount = GetHeightLevelCount();
            int tileCount = chunkSize * chunkSize;
            EnsureChunkBuffers(chunk, tileCount);

            int startX = chunk.chunkX * chunkSize;
            int startY = chunk.chunkY * chunkSize;

            for (int i = 0; i < tileCount; i++)
            {
                chunk.baseTiles[i] = null;
                chunk.heightLevels[i] = int.MinValue;
            }
            for (int i = 0; i < tileCount; i++)
            {
                chunk.cliffLevels[i] = int.MinValue;
            }

            for (int ly = 0; ly < chunkSize; ly++)
            {
                for (int lx = 0; lx < chunkSize; lx++)
                {
                    int gx = startX + lx;
                    int gy = startY + ly;
                    int index = ly * chunkSize + lx;

                    if (!IsValidPosition(gx, gy))
                    {
                        chunk.baseTiles[index] = null;
                        chunk.heightLevels[index] = int.MinValue;
                        continue;
                    }

                    TileSample sample = SampleTile(gx, gy, chunk);
                    chunk.baseTiles[index] = sample.tile;

                    int heightLevel = GetHeightLevel(gx, gy);
                    chunk.heightLevels[index] = heightLevel;
                }
            }

            for (int ly = 1; ly < chunkSize; ly++)
            {
                for (int lx = 0; lx < chunkSize; lx++)
                {
                    int upperIndex = ly * chunkSize + lx;
                    int lowerIndex = (ly - 1) * chunkSize + lx;

                    if (chunk.baseTiles[upperIndex] == null || chunk.baseTiles[lowerIndex] == null)
                    {
                        continue;
                    }

                    int upperLevel = chunk.heightLevels[upperIndex];
                    int lowerLevel = chunk.heightLevels[lowerIndex];
                    if (upperLevel <= lowerLevel)
                    {
                        continue;
                    }

                    chunk.cliffLevels[lowerIndex] = lowerLevel;
                }
            }

            BoundsInt bounds = new BoundsInt(0, 0, 0, chunkSize, chunkSize, 1);
            for (int i = 0; i < levelCount; i++)
            {
                int heightLevel = minHeightLevel + i;
                System.Array.Clear(chunk.tileBuffer, 0, tileCount);
                if (useCliffOverlayTilemaps && chunk.cliffBuffer != null)
                {
                    System.Array.Clear(chunk.cliffBuffer, 0, tileCount);
                }
                else
                {
                    for (int c = 0; c < tileCount; c++)
                    {
                        chunk.colorBuffer[c] = Color.white;
                    }
                }

                for (int index = 0; index < tileCount; index++)
                {
                    if (chunk.baseTiles[index] == null) continue;

                    if (chunk.heightLevels[index] == heightLevel)
                    {
                        chunk.tileBuffer[index] = chunk.baseTiles[index];
                        if (!useCliffOverlayTilemaps && chunk.cliffLevels[index] == heightLevel)
                        {
                            chunk.colorBuffer[index] = cliffTint;
                        }
                    }

                    if (useCliffOverlayTilemaps && chunk.cliffBuffer != null && chunk.cliffLevels[index] == heightLevel)
                    {
                        chunk.cliffBuffer[index] = chunk.baseTiles[index];
                    }
                }

                chunk.tilemaps[i].SetTilesBlock(bounds, chunk.tileBuffer);
                if (useCliffOverlayTilemaps && chunk.cliffTilemaps != null)
                {
                    chunk.cliffTilemaps[i].SetTilesBlock(bounds, chunk.cliffBuffer);
                }
                else
                {
                    ApplyTileColors(chunk.tilemaps[i], chunk.tileBuffer, chunk.colorBuffer);
                }
            }
        }

        /// <summary>
        /// 오브젝트 생성 (하위 클래스에서 구현)
        /// </summary>
        protected abstract void GenerateObjectsForChunk(Chunk chunk);

        protected virtual System.Collections.IEnumerator GenerateObjectsForChunkAsync(Chunk chunk)
        {
            GenerateObjectsForChunk(chunk);
            yield break;
        }

        /// <summary>
        /// 타일 샘플링
        /// </summary>
        protected virtual TileSample SampleTile(int worldX, int worldY, Chunk chunk)
        {
            return SampleBaseTile(worldX, worldY);
        }

        protected abstract TileSample SampleBaseTile(int worldX, int worldY);
        protected abstract TileBase GetTileAsset(BiomeTileType tileType);

        protected virtual bool IsTileWalkable(BiomeTileType tileType)
        {
            return tileType != BiomeTileType.Wall && tileType != BiomeTileType.Obstacle;
        }

        /// <summary>
        /// 청크 오브젝트 파괴
        /// </summary>

        /// <summary>
        /// 유효한 청크인지 확인
        /// </summary>

        /// <summary>
        /// 그리드 좌표 → 월드 좌표
        /// </summary>

        /// <summary>
        /// 월드 좌표 → 그리드 좌표
        /// </summary>

        /// <summary>
        /// 그리드 좌표 → 청크 좌표
        /// </summary>

        /// <summary>
        /// 유효한 좌표인지 확인
        /// </summary>

        protected virtual int GetBaseHeightLevel(int worldX, int worldY)
        {
            return 0;
        }
    }
}
