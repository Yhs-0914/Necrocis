using UnityEngine;
using System.Collections.Generic;

namespace Necrocis
{
    /// <summary>
    /// Voronoi/Perlin 기반 지역 + Poisson 배치 공통 로직.
    /// </summary>
    public abstract class RegionPoissonBiomeManager : BiomeManager
    {
        [Header("=== Regions ===")]
        [SerializeField] protected int regionCount = 3;
        [SerializeField] protected float regionCellSize = 20f;
        [SerializeField] protected float regionBlendWidth = 3f;

        [Header("=== Height ===")]
        [SerializeField] protected float heightNoiseScale = 0.02f;
        [SerializeField] protected float heightNoiseAmplitude = 0.45f;
        [SerializeField] protected float heightThreshold = 0f;
        [SerializeField] protected int heightCaIterations = 0;

        protected BiomePerlinNoise heightNoise;
        protected readonly List<ObjectRule> objectRules = new List<ObjectRule>();

        private readonly Dictionary<Vector2Int, ChunkCache> chunkCaches = new Dictionary<Vector2Int, ChunkCache>();

        private const int RegionCellJitterSalt = 501;
        private const int RegionTypeSalt = 777;
        private const int RegionBlendSalt = 888;

        protected struct RegionSample
        {
            public int primary;
            public int secondary;
            public float blend;

            public RegionSample(int primary, int secondary, float blend)
            {
                this.primary = primary;
                this.secondary = secondary;
                this.blend = blend;
            }
        }

        protected struct ObjectRule
        {
            public SpawnCategory category;
            public BiomeObjectKind kind;
            public float density;
            public float minDistance;
            public bool blocksMovement;
            public int regionMask;
            public int salt;
            public int configIndex;

            // Scale-aware spacing: 후보의 scale에 따라 실제 거리 요구치를 키움
            public float scaleMin;
            public float scaleMax;
            public int scaleSalt;
            public float scaleBias;
            public float spacingPadding;
            public float avoidPlayerSpawnRadius;
        }

        protected enum SpawnCategory
        {
            SceneObject = 0,
            EnemySpawner = 1
        }

        private sealed class ChunkCache
        {
            public int[] regionTypes;
            public bool[] regionValid;
            public int[] heightLevels;
            public bool[] heightValid;

            // CA 평활화 단계용 boost (0/1 이진).
            public int[] rawBoosts;
            public bool[] rawBoostValid;
            public int[] caBoosts1;
            public bool[] caBoost1Valid;

            public ChunkCache(int tileCount)
            {
                regionTypes = new int[tileCount];
                regionValid = new bool[tileCount];
                heightLevels = new int[tileCount];
                heightValid = new bool[tileCount];
                rawBoosts = new int[tileCount];
                rawBoostValid = new bool[tileCount];
                caBoosts1 = new int[tileCount];
                caBoost1Valid = new bool[tileCount];
            }
        }

        protected override void Awake()
        {
            base.Awake();
            InitializeNoise();
            BuildObjectRules();
        }

        protected virtual void InitializeNoise()
        {
            heightNoise = new BiomePerlinNoise(seed + 97);
            heightNoise.SetFrequency(heightNoiseScale);
        }

        protected abstract void BuildObjectRules();

        protected override int GetBaseHeightLevel(int worldX, int worldY)
        {
            return GetHeightLevelCached(worldX, worldY);
        }

        protected override void GenerateObjectsForChunk(Chunk chunk)
        {
            if (!chunk.isSpawnManifestBuilt)
            {
                var manifestEnumerator = BuildChunkSpawnManifestInternal(chunk);
                while (manifestEnumerator.MoveNext())
                {
                }
            }

            var enumerator = SpawnObjectsFromManifestInternal(chunk);
            while (enumerator.MoveNext())
            {
            }
        }

        protected override System.Collections.IEnumerator GenerateObjectsForChunkAsync(Chunk chunk)
        {
            if (!chunk.isSpawnManifestBuilt)
            {
                System.Collections.IEnumerator manifestEnumerator = BuildChunkSpawnManifestInternal(chunk);
                while (manifestEnumerator.MoveNext())
                {
                    yield return manifestEnumerator.Current;
                }
            }

            System.Collections.IEnumerator spawnEnumerator = SpawnObjectsFromManifestInternal(chunk);
            while (spawnEnumerator.MoveNext())
            {
                yield return spawnEnumerator.Current;
            }
        }

        protected virtual bool IsObjectAreaAllowed(int x, int y)
        {
            return true;
        }

        protected abstract void SpawnChunkRecord(ChunkSpawnRecord record, Chunk chunk);

        protected virtual void AddExtraChunkSpawnRecords(Chunk chunk)
        {
        }

        protected override void OnChunkUnloaded(Chunk chunk)
        {
            Vector2Int chunkPos = new Vector2Int(chunk.chunkX, chunk.chunkY);
            chunkCaches.Remove(chunkPos);
        }

        protected static int Mask(params int[] regionTypes)
        {
            int mask = 0;
            for (int i = 0; i < regionTypes.Length; i++)
            {
                mask |= 1 << regionTypes[i];
            }
            return mask;
        }

        protected void AddChunkSpawnRecord(Chunk chunk, ChunkSpawnRecord record)
        {
            chunk.spawnManifest.Add(record);
        }

        private System.Collections.IEnumerator BuildChunkSpawnManifestInternal(Chunk chunk)
        {
            chunk.spawnManifest.Clear();

            int startX = GetChunkStartX(chunk.chunkX);
            int startY = GetChunkStartY(chunk.chunkY);

            HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
            int processed = 0;
            int budget = Mathf.Max(16, objectGenerationBudget);

            for (int lx = 0; lx < chunkSize; lx++)
            {
                for (int ly = 0; ly < chunkSize; ly++)
                {
                    int gx = startX + lx;
                    int gy = startY + ly;

                    if (!IsValidPosition(gx, gy)) continue;
                    if (!IsObjectAreaAllowed(gx, gy)) continue;

                    int regionType = GetRegionTypeCached(gx, gy);
                    Vector2Int pos = new Vector2Int(gx, gy);

                    foreach (var rule in objectRules)
                    {
                        if (occupied.Contains(pos)) break;
                        if (!IsRegionAllowed(rule.regionMask, regionType)) continue;
                        if (!IsPoissonSelected(gx, gy, regionType, rule)) continue;

                        chunk.spawnManifest.Add(new ChunkSpawnRecord(
                            ToChunkSpawnCategory(rule.category),
                            rule.kind,
                            rule.configIndex,
                            gx,
                            gy,
                            rule.blocksMovement));
                        occupied.Add(pos);
                        break;
                    }

                    processed++;
                    if (processed >= budget)
                    {
                        processed = 0;
                        yield return null;
                    }
                }
            }

            AddExtraChunkSpawnRecords(chunk);
            chunk.isSpawnManifestBuilt = true;
        }

        private System.Collections.IEnumerator SpawnObjectsFromManifestInternal(Chunk chunk)
        {
            int processed = 0;
            int budget = Mathf.Max(16, objectGenerationBudget);

            for (int i = 0; i < chunk.spawnManifest.Count; i++)
            {
                SpawnChunkRecord(chunk.spawnManifest[i], chunk);

                processed++;
                if (processed >= budget)
                {
                    processed = 0;
                    yield return null;
                }
            }
        }

        private static ChunkSpawnCategory ToChunkSpawnCategory(SpawnCategory category)
        {
            return category == SpawnCategory.EnemySpawner
                ? ChunkSpawnCategory.EnemySpawner
                : ChunkSpawnCategory.SceneObject;
        }

        private ChunkCache GetOrCreateChunkCache(Vector2Int chunkPos)
        {
            if (!chunkCaches.TryGetValue(chunkPos, out ChunkCache cache))
            {
                cache = new ChunkCache(chunkSize * chunkSize);
                chunkCaches.Add(chunkPos, cache);
            }
            return cache;
        }

        private int GetChunkIndex(int worldX, int worldY, Vector2Int chunkPos)
        {
            int localX = worldX - GetChunkStartX(chunkPos.x);
            int localY = worldY - GetChunkStartY(chunkPos.y);
            return localY * chunkSize + localX;
        }

        protected int GetRegionTypeCached(int worldX, int worldY)
        {
            if (!IsValidPosition(worldX, worldY))
            {
                RegionSample sample = SampleRegion(worldX, worldY);
                return ResolveRegionType(sample, worldX, worldY);
            }

            Vector2Int chunkPos = GridToChunk(worldX, worldY);
            ChunkCache cache = GetOrCreateChunkCache(chunkPos);
            int index = GetChunkIndex(worldX, worldY, chunkPos);

            if (!cache.regionValid[index])
            {
                RegionSample sample = SampleRegion(worldX, worldY);
                cache.regionTypes[index] = ResolveRegionType(sample, worldX, worldY);
                cache.regionValid[index] = true;
            }

            return cache.regionTypes[index];
        }

        protected int GetHeightLevelCached(int worldX, int worldY)
        {
            if (!IsValidPosition(worldX, worldY))
            {
                RegionSample sample = SampleRegion(worldX, worldY);
                return ResolveHeight(sample, worldX, worldY);
            }

            Vector2Int chunkPos = GridToChunk(worldX, worldY);
            ChunkCache cache = GetOrCreateChunkCache(chunkPos);
            int index = GetChunkIndex(worldX, worldY, chunkPos);

            if (!cache.heightValid[index])
            {
                RegionSample sample = SampleRegion(worldX, worldY);
                cache.heightLevels[index] = ResolveHeight(sample, worldX, worldY);
                cache.heightValid[index] = true;
            }

            return cache.heightLevels[index];
        }

        private RegionSample SampleRegion(int worldX, int worldY)
        {
            float cellSize = Mathf.Max(1f, regionCellSize);
            int cellX = Mathf.FloorToInt(worldX / cellSize);
            int cellY = Mathf.FloorToInt(worldY / cellSize);

            float bestDist = float.MaxValue;
            float secondDist = float.MaxValue;
            int bestType = 0;
            int secondType = 0;

            int safeRegionCount = Mathf.Max(1, regionCount);
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = cellX + dx;
                    int ny = cellY + dy;

                    Vector2 offset = BiomeDeterministic.HashInCell(seed, nx, ny, RegionCellJitterSalt);
                    float fx = (nx + offset.x) * cellSize;
                    float fy = (ny + offset.y) * cellSize;

                    float dist = (fx - worldX) * (fx - worldX) + (fy - worldY) * (fy - worldY);
                    int regionType = BiomeDeterministic.HashRange(seed, nx, ny, RegionTypeSalt, safeRegionCount);

                    if (dist < bestDist)
                    {
                        secondDist = bestDist;
                        secondType = bestType;
                        bestDist = dist;
                        bestType = regionType;
                    }
                    else if (dist < secondDist)
                    {
                        secondDist = dist;
                        secondType = regionType;
                    }
                }
            }

            float blend = 0f;
            if (regionBlendWidth > 0f)
            {
                float edge = Mathf.Sqrt(secondDist) - Mathf.Sqrt(bestDist);
                blend = Mathf.Clamp01((regionBlendWidth - edge) / regionBlendWidth);
            }

            return new RegionSample(bestType, secondType, blend);
        }

        private int ResolveRegionType(RegionSample sample, int worldX, int worldY)
        {
            if (sample.blend <= 0f || sample.primary == sample.secondary)
            {
                return sample.primary;
            }

            float mix = BiomeDeterministic.Hash01(seed, worldX, worldY, RegionBlendSalt);
            return mix < sample.blend ? sample.secondary : sample.primary;
        }

        private int ResolveHeight(RegionSample sample, int worldX, int worldY)
        {
            int primaryHeight = GetRegionHeight(sample.primary);
            int secondaryHeight = GetRegionHeight(sample.secondary);

            float baseHeight = primaryHeight;
            if (sample.blend > 0f && primaryHeight != secondaryHeight)
            {
                baseHeight = Mathf.Lerp(primaryHeight, secondaryHeight, sample.blend);
            }

            int level;
            if (heightThreshold > 0f && heightNoise != null)
            {
                // Threshold 모드: 이진 boost (0/1) → CA 평활화 → plateauRise 배수.
                int finalBoost = GetFinalBoost(worldX, worldY);
                int rise = GetRegionPlateauRise(sample.primary);
                level = Mathf.RoundToInt(baseHeight) + finalBoost * rise;
            }
            else
            {
                // Legacy 연속 노이즈 모드.
                float noise = 0f;
                if (heightNoise != null)
                {
                    noise = heightNoise.GetNoise(worldX, worldY) * heightNoiseAmplitude;
                }
                float heightValue = baseHeight + noise;
                level = Mathf.RoundToInt(heightValue);
            }

            return Mathf.Clamp(level, minHeightLevel, maxHeightLevel);
        }

        /// <summary>
        /// Raw threshold boost (CA 전): noise > threshold면 1, 아니면 0.
        /// 청크 캐시에 결정론적으로 저장.
        /// </summary>
        private int GetRawBoost(int worldX, int worldY)
        {
            if (heightThreshold <= 0f || heightNoise == null) return 0;

            if (!IsValidPosition(worldX, worldY))
            {
                float noise = heightNoise.GetNoise(worldX, worldY);
                return noise > heightThreshold ? 1 : 0;
            }

            Vector2Int chunkPos = GridToChunk(worldX, worldY);
            ChunkCache cache = GetOrCreateChunkCache(chunkPos);
            int index = GetChunkIndex(worldX, worldY, chunkPos);

            if (!cache.rawBoostValid[index])
            {
                float noise = heightNoise.GetNoise(worldX, worldY);
                cache.rawBoosts[index] = noise > heightThreshold ? 1 : 0;
                cache.rawBoostValid[index] = true;
            }
            return cache.rawBoosts[index];
        }

        /// <summary>
        /// 1차 CA 결과 (raw boost ±1 8방향 vote). 청크 캐시에 저장.
        /// </summary>
        private int GetCaBoost1(int worldX, int worldY)
        {
            if (!IsValidPosition(worldX, worldY))
            {
                return VoteBoost(worldX, worldY, false);
            }

            Vector2Int chunkPos = GridToChunk(worldX, worldY);
            ChunkCache cache = GetOrCreateChunkCache(chunkPos);
            int index = GetChunkIndex(worldX, worldY, chunkPos);

            if (!cache.caBoost1Valid[index])
            {
                cache.caBoosts1[index] = VoteBoost(worldX, worldY, false);
                cache.caBoost1Valid[index] = true;
            }
            return cache.caBoosts1[index];
        }

        /// <summary>
        /// 최종 boost. caIterations 횟수만큼 CA 적용:
        /// - 0: raw
        /// - 1: 1차 CA
        /// - 2+: 2차 CA (1차 CA 결과를 한 번 더 vote)
        /// </summary>
        private int GetFinalBoost(int worldX, int worldY)
        {
            if (heightCaIterations <= 0) return GetRawBoost(worldX, worldY);
            if (heightCaIterations == 1) return GetCaBoost1(worldX, worldY);
            return VoteBoost(worldX, worldY, true);
        }

        /// <summary>
        /// 5/8 majority rule. useCaBoost1=false면 raw vote(1차 결과 도출), true면 caBoost1 vote(2차 결과 도출).
        /// 8방향 이웃 중 plateau(=1)가 5칸 이상이면 1, 3칸 이하면 0, 정확히 4칸이면 자신 유지.
        /// </summary>
        private int VoteBoost(int worldX, int worldY, bool useCaBoost1)
        {
            int self = useCaBoost1 ? GetCaBoost1(worldX, worldY) : GetRawBoost(worldX, worldY);
            int plateauCount = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int neighbor = useCaBoost1
                        ? GetCaBoost1(worldX + dx, worldY + dy)
                        : GetRawBoost(worldX + dx, worldY + dy);
                    if (neighbor > 0) plateauCount++;
                }
            }
            if (plateauCount >= 5) return 1;
            if (plateauCount <= 3) return 0;
            return self;
        }

        protected abstract int GetRegionHeight(int regionType);

        /// <summary>
        /// Threshold 모드에서 해당 region이 고원으로 솟을 때 추가할 height 단계.
        /// 기본 1 (legacy). region의 wallTiles.Length와 일치시켜야 시각과 로직이 정합.
        /// </summary>
        protected virtual int GetRegionPlateauRise(int regionType)
        {
            return 1;
        }

        protected bool IsRegionAllowed(int mask, int regionType)
        {
            int region = 1 << regionType;
            return (mask & region) != 0;
        }

        protected virtual float GetDensityForRule(ObjectRule rule, int worldX, int worldY, int regionType)
        {
            return IsRegionAllowed(rule.regionMask, regionType) ? rule.density : 0f;
        }

        private bool IsPoissonSelected(int worldX, int worldY, int regionType, ObjectRule rule)
        {
            float density = GetDensityForRule(rule, worldX, worldY, regionType);
            if (density <= 0f) return false;

            if (rule.avoidPlayerSpawnRadius > 0f)
            {
                Vector2Int playerSpawnGrid = WorldToGrid(GetPlayerSpawnPosition());
                float dxFromSpawn = worldX - playerSpawnGrid.x;
                float dyFromSpawn = worldY - playerSpawnGrid.y;
                if (dxFromSpawn * dxFromSpawn + dyFromSpawn * dyFromSpawn
                    < rule.avoidPlayerSpawnRadius * rule.avoidPlayerSpawnRadius)
                {
                    return false;
                }
            }

            int cellSize = Mathf.Max(1, Mathf.RoundToInt(rule.minDistance));
            int cellX = Mathf.FloorToInt((float)worldX / cellSize);
            int cellY = Mathf.FloorToInt((float)worldY / cellSize);
            Vector2Int candidate = GetCandidateInCell(cellX, cellY, cellSize, rule.salt);
            if (candidate.x != worldX || candidate.y != worldY) return false;

            float selfValue = BiomeDeterministic.Hash01(seed, worldX, worldY, rule.salt);
            if (selfValue >= density) return false;

            float padding = rule.spacingPadding > 0f ? rule.spacingPadding : 1f;
            float baseRadius = Mathf.Max(0.5f, rule.minDistance);
            float selfScale = GetRuleScale(rule, worldX, worldY);
            // 가장 큰 가능한 scale 기준으로 검색 범위 확보
            float maxPossibleScale = Mathf.Max(1f, rule.scaleMax);
            float searchRadius = baseRadius * padding * Mathf.Max(selfScale, maxPossibleScale);
            int cellRange = Mathf.CeilToInt(searchRadius / cellSize);

            for (int dx = -cellRange; dx <= cellRange; dx++)
            {
                for (int dy = -cellRange; dy <= cellRange; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    Vector2Int otherCandidate = GetCandidateInCell(cellX + dx, cellY + dy, cellSize, rule.salt);
                    if (!IsValidPosition(otherCandidate.x, otherCandidate.y)) continue;

                    float offsetX = otherCandidate.x - worldX;
                    float offsetY = otherCandidate.y - worldY;
                    float distSq = offsetX * offsetX + offsetY * offsetY;

                    int otherRegionType = GetRegionTypeCached(otherCandidate.x, otherCandidate.y);
                    float otherDensity = GetDensityForRule(rule, otherCandidate.x, otherCandidate.y, otherRegionType);
                    if (otherDensity <= 0f) continue;

                    float otherScale = GetRuleScale(rule, otherCandidate.x, otherCandidate.y);
                    float requiredRadius = baseRadius * padding * Mathf.Max(selfScale, otherScale);
                    if (distSq > requiredRadius * requiredRadius) continue;

                    float otherValue = BiomeDeterministic.Hash01(seed, otherCandidate.x, otherCandidate.y, rule.salt);
                    if (otherValue < otherDensity && otherValue < selfValue)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private float GetRuleScale(ObjectRule rule, int x, int y)
        {
            if (rule.scaleMax <= 0f) return 1f;
            int salt = rule.scaleSalt != 0 ? rule.scaleSalt : rule.salt + 9173;
            return BiomeDeterministic.ComputeScale(seed, x, y, salt, rule.scaleMin, rule.scaleMax, rule.scaleBias);
        }

        private Vector2Int GetCandidateInCell(int cellX, int cellY, int cellSize, int salt)
        {
            Vector2 offset = BiomeDeterministic.HashInCell(seed, cellX, cellY, salt);
            int originX = cellX * cellSize;
            int originY = cellY * cellSize;
            int candidateX = originX + Mathf.FloorToInt(offset.x * cellSize);
            int candidateY = originY + Mathf.FloorToInt(offset.y * cellSize);
            return new Vector2Int(candidateX, candidateY);
        }
    }
}
