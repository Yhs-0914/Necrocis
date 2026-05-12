using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Necrocis
{
    /// 타일 샘플 결과
    /// </summary>
    public struct TileSample
    {
        public BiomeTileType tileType;
        public TileBase tile;
        public bool walkable;

        public TileSample(BiomeTileType tileType, TileBase tile, bool walkable)
        {
            this.tileType = tileType;
            this.tile = tile;
            this.walkable = walkable;
        }
    }

    /// <summary>
    /// 오브젝트 식별자 (결정론)
    /// </summary>
    public struct ObjectId
    {
        public int x;
        public int y;
        public BiomeObjectKind type;

        public ObjectId(int x, int y, BiomeObjectKind type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + x;
                hash = hash * 31 + y;
                hash = hash * 31 + (int)type;
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is ObjectId other)
            {
                return x == other.x && y == other.y && type == other.type;
            }
            return false;
        }
    }

    public struct ObjectPoolKey
    {
        public BiomeObjectKind kind;
        public int archetypeId;

        public ObjectPoolKey(BiomeObjectKind kind, int archetypeId = 0)
        {
            this.kind = kind;
            this.archetypeId = archetypeId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (int)kind;
                hash = hash * 31 + archetypeId;
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is ObjectPoolKey other)
            {
                return kind == other.kind && archetypeId == other.archetypeId;
            }
            return false;
        }
    }

    public enum ChunkSpawnCategory
    {
        SceneObject = 0,
        EnemySpawner = 1,
        Portal = 2
    }

    [System.Serializable]
    public struct ChunkSpawnRecord
    {
        public ChunkSpawnCategory category;
        public BiomeObjectKind objectKind;
        public int configIndex;
        public int x;
        public int y;
        public bool blocksMovement;

        public ChunkSpawnRecord(ChunkSpawnCategory category, BiomeObjectKind objectKind, int configIndex, int x, int y, bool blocksMovement)
        {
            this.category = category;
            this.objectKind = objectKind;
            this.configIndex = configIndex;
            this.x = x;
            this.y = y;
            this.blocksMovement = blocksMovement;
        }
    }

    /// <summary>
    /// 청크 데이터
    /// </summary>
    [System.Serializable]
    public class Chunk
    {
        public int chunkX;
        public int chunkY;
        public int size;
        public bool isLoaded;
        public bool isObjectsLoaded;

        public GameObject root;
        public Transform objectsRoot;
        public Tilemap[] tilemaps;
        public TilemapRenderer[] tilemapRenderers;
        public Tilemap[] cliffTilemaps;
        public TilemapRenderer[] cliffTilemapRenderers;

        public TileBase[] baseTiles;
        public int[] heightLevels;
        public int[] cliffLevels;
        public TileBase[] tileBuffer;
        public TileBase[] cliffBuffer;
        public Color[] colorBuffer;

        public Coroutine objectGenerationRoutine;

        public bool isSpawnManifestBuilt;
        public List<ChunkSpawnRecord> spawnManifest = new List<ChunkSpawnRecord>();
        public List<GameObject> liveObjects = new List<GameObject>();

        public Chunk(int x, int y, int size)
        {
            chunkX = x;
            chunkY = y;
            this.size = size;
            isLoaded = false;
            isObjectsLoaded = false;
        }
    }

    public class ChunkRoot : MonoBehaviour
    {
        public int levelCount;
        public bool hasCliffTilemaps;
        public Tilemap[] tilemaps;
        public TilemapRenderer[] tilemapRenderers;
        public Tilemap[] cliffTilemaps;
        public TilemapRenderer[] cliffTilemapRenderers;

        public bool Matches(int expectedLevelCount, bool expectedCliffTilemaps)
        {
            if (tilemaps == null || tilemapRenderers == null) return false;
            if (tilemaps.Length != expectedLevelCount) return false;
            if (tilemapRenderers.Length != expectedLevelCount) return false;
            if (expectedCliffTilemaps)
            {
                if (cliffTilemaps == null || cliffTilemapRenderers == null) return false;
                if (cliffTilemaps.Length != expectedLevelCount) return false;
                if (cliffTilemapRenderers.Length != expectedLevelCount) return false;
            }
            else if (cliffTilemaps != null || cliffTilemapRenderers != null)
            {
                return false;
            }
            return levelCount == expectedLevelCount && hasCliffTilemaps == expectedCliffTilemaps;
        }

        public void Configure(int levelCount, bool hasCliffTilemaps, Tilemap[] tilemaps, TilemapRenderer[] tilemapRenderers, Tilemap[] cliffTilemaps, TilemapRenderer[] cliffTilemapRenderers)
        {
            this.levelCount = levelCount;
            this.hasCliffTilemaps = hasCliffTilemaps;
            this.tilemaps = tilemaps;
            this.tilemapRenderers = tilemapRenderers;
            this.cliffTilemaps = cliffTilemaps;
            this.cliffTilemapRenderers = cliffTilemapRenderers;
        }
    }

    /// <summary>
    /// 바이옴 타일 종류 (기본)
    /// </summary>
    public enum BiomeTileType
    {
        None,
        Floor,          // 기본 바닥
        FloorVariant,   // 바닥 변형
        Decoration,     // 장식 바닥 (풀 등)
        Puddle,         // 웅덩이
        Wall,           // 벽
        Obstacle        // 장애물
    }

    /// <summary>
    /// 풀링/배치에 사용하는 바이옴 오브젝트 카테고리
    /// </summary>
    public enum BiomeObjectKind
    {
        None = 0,
        FloorDecoration = 1,
        SmallDecoration = 2,
        LargeObstacle = 3,
        AnimatedDecoration = 4,
        Item = 5,
        EnemySpawner = 6,
        Portal = 100
    }

    /// <summary>
    /// 바이옴 오브젝트 종류 (기본)
    /// </summary>
    public enum BiomeObjectType
    {
        None,
        DecorationSmall,        // 작은 장식물
        DecorationLarge,        // 큰 장식물
        InteractableDecoration, // 상호작용 가능 장식물
        DestructibleObject,     // 파괴 가능 오브젝트
        Item,                   // 아이템
        MonsterSpawnPoint,      // 몬스터 스폰 포인트
        ReturnPortal            // 귀환 포털
    }

    [System.Serializable]
    public struct PoolLimit
    {
        public BiomeObjectKind type;
        public int maxSize;
    }
}
