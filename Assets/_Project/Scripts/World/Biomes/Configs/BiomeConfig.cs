using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome/Biome Config", fileName = "BiomeConfig")]
    public class BiomeConfig : ScriptableObject
    {
        [Header("기본")]
        public BiomeType biomeType = BiomeType.None;

        [Header("Regions")]
        public float regionCellSize = 20f;
        public float regionBlendWidth = 3f;
        public float detailNoiseScale = 0.05f;
        public List<BiomeRegionDefinition> regions = new List<BiomeRegionDefinition>();

        [Header("Height Noise")]
        public float heightNoiseScale = 0.02f;
        public float heightNoiseAmplitude = 0.45f;

        [Header("Tile Defaults")]
        public List<TileTypeMapping> tileMappings = new List<TileTypeMapping>();

        [Header("Exterior Backdrop")]
        public BiomeExteriorBackdropConfig exteriorBackdrop = new BiomeExteriorBackdropConfig();

        [Header("Object Spawn Area Padding")]
        public int marginLeft;
        public int marginRight;
        public int marginBottom;
        public int marginTop;

        [Header("Objects")]
        public bool spawnWorldItems = true;
        public List<BiomeObjectRuleConfig> objectRules = new List<BiomeObjectRuleConfig>();

        [Header("Enemy Config")]
        public EnemySpawnConfig enemySpawnConfig;

        [Header("Biome Elite Map Placement")]
        public BiomeEliteSpawnConfig biomeEliteSpawnConfig;
        [Header("Final Boss Phase Sources")]
        [Tooltip("Only used by the final-boss room to reuse each biome's enemies and mid boss.")]
        public List<BiomeConfig> finalBossSourceBiomes = new List<BiomeConfig>();

        [SerializeField, HideInInspector]
        private List<EnemySpawnRuleConfig> enemySpawnRules = new List<EnemySpawnRuleConfig>();

        [Header("Boss Arena Config")]
        public BossArenaConfig bossArenaConfig;

        [SerializeField, HideInInspector]
        private MidBossArenaConfig midBossArena = new MidBossArenaConfig();

        public TileBase GetTileForType(BiomeTileType type)
        {
            foreach (var mapping in tileMappings)
            {
                if (mapping.tileType == type)
                {
                    return mapping.tile;
                }
            }
            return null;
        }

        public IReadOnlyList<EnemySpawnRuleConfig> GetEnemySpawnRules()
        {
            if (enemySpawnConfig != null)
            {
                IReadOnlyList<EnemySpawnRuleConfig> configuredRules = enemySpawnConfig.GetEnemySpawnRules();
                if (configuredRules != null)
                {
                    return configuredRules;
                }
            }

            return enemySpawnRules != null
                ? enemySpawnRules
                : System.Array.Empty<EnemySpawnRuleConfig>();
        }

        public MidBossArenaConfig GetMidBossArenaConfig()
        {
            if (bossArenaConfig != null)
            {
                MidBossArenaConfig configuredArena = bossArenaConfig.GetMidBossArenaConfig();
                if (configuredArena != null)
                {
                    return configuredArena;
                }
            }

            return midBossArena;
        }

        [Header("Return Portal")]
        public BiomeReturnPortalConfig returnPortal = new BiomeReturnPortalConfig();

        public BiomeReturnPortalConfig GetReturnPortalConfig()
        {
            return returnPortal;
        }
    }

    [System.Serializable]
    public class BiomeRegionDefinition
    {
        public string name = "Region";
        public int baseHeight;

        public TileBase primaryTile;
        public BiomeTileType primaryType = BiomeTileType.Floor;

        public TileBase variantTile;
        public BiomeTileType variantType = BiomeTileType.FloorVariant;

        [Range(0f, 1f)]
        public float variantThreshold = 0.5f;

        [Tooltip("같은 리전 내 시각적 변형 타일들 (해시 기반 선택)")]
        public TileBase[] tileVariants;
    }

    [System.Serializable]
    public class TileTypeMapping
    {
        public BiomeTileType tileType = BiomeTileType.Floor;
        public TileBase tile;
    }

    [System.Serializable]
    public class BiomeExteriorBackdropConfig
    {
        public bool enabled;
        public Sprite sprite;
        public Color tint = Color.white;
        [Range(0f, 1f)] public float opacity = 0.82f;
        [Tooltip("배경 이미지 뒤에 남는 카메라 바탕색")]
        public Color cameraClearColor = new Color(0.035f, 0.015f, 0.018f, 1f);
        [Tooltip("플레이 공간보다 충분히 뒤에 배치할 카메라 기준 거리")]
        [Min(1f)] public float cameraDistance = 50f;
        [Tooltip("줌이나 화면비 변화 때 이미지 가장자리가 드러나지 않도록 추가로 키우는 배율")]
        [Range(1f, 2f)] public float overscan = 1.2f;
        [Tooltip("정적인 벽지처럼 보이지 않게 하는 아주 느린 화면 드리프트 크기")]
        [Range(0f, 0.1f)] public float driftAmount = 0.015f;
        [Min(0f)] public float driftSpeed = 0.08f;
        public int sortingOrder = -32000;

        public bool IsUsable => enabled && sprite != null;
    }

    [System.Serializable]
    public class BiomeObjectRuleConfig
    {
        public string name = "Object";
        public BiomeObjectKind poolKind = BiomeObjectKind.SmallDecoration;

        [Header("Poisson")]
        public float density = 0.01f;
        public float minDistance = 2f;
        public int poissonSalt = 100;

        [Tooltip("비워두면 모든 지역에서 허용")]
        public List<int> allowedRegions = new List<int>();

        [Header("Placement")]
        public bool blocksMovement;
        public float heightOffset = 0f;
        public int sortingOrder = 100;

        [Header("Sprite")]
        public Sprite[] sprites;
        public bool useDeterministicSprite = true;
        public int spriteSalt = 0;
        public bool animate = false;
        public float animationSpeed = 0.15f;

        [Header("Components")]
        public bool useBillboard = true;
        public bool useYSort = false;

        [Header("Collider")]
        public bool addCollider = false;
        public bool isTrigger = false;
        public Vector3 colliderSize = new Vector3(1f, 1f, 1f);
        public Vector3 colliderCenter = Vector3.zero;
    }

    [System.Serializable]
    public class BiomeReturnPortalConfig
    {
        public bool enabled = true;
        public string name = "ReturnPortal";
        public BiomeObjectKind poolKind = BiomeObjectKind.Portal;
        public Sprite sprite;
        public int sortingOrder = 1000;
        public bool useCustomPosition = true;
        public Vector2Int gridPosition = new Vector2Int(0, 4);
        public float heightOffset = 0f;
        public bool useBillboard = true;
        public Vector3 scale = new Vector3(0.5f, 0.5f, 0.5f);
        public bool addCollider = true;
        public bool isTrigger = true;
        public Vector3 colliderSize = new Vector3(2f, 2f, 2f);
        public Vector3 colliderCenter = Vector3.zero;
    }

    [System.Serializable]
    public class MidBossArenaConfig
    {
        public bool enabled = true;
        public bool onlyEnableOnLargeMaps = true;
        public int minimumMapWidth = 300;
        public int minimumMapHeight = 300;

        [Header("Layout")]
        public bool useCustomCenter = false;
        public Vector2Int centerGrid = new Vector2Int(150, 150);
        public Vector2Int arenaSize = new Vector2Int(32, 32);
        public int wallThicknessInCells = 1;
        [Tooltip("아레나 외곽보다 안쪽으로 봉쇄 경계를 들여놓을 칸 수")]
        public int lockBoundaryInsetInCells = 1;
        [Tooltip("아레나 모서리에서 추가로 진입 트리거를 들여놓을 칸 수")]
        public int triggerInsetInCells = 2;

        [Header("Runtime Bounds")]
        public float wallHeightOffset = 1.5f;
        public float triggerHeight = 4f;
        public int sortingOrder = 3500;

        [Header("Boss Concealment")]
        [Tooltip("아레나에 진입하기 전까지 보스 Renderer를 꺼서 미리 노출되지 않게 한다.")]
        public bool hideBossUntilEncounter = true;

        [Header("Arena Presentation")]
        public BossArenaPresentationConfig presentation = new BossArenaPresentationConfig();

        [Header("Return Portal")]
        public Sprite returnPortalSprite;
        public Vector3 returnPortalScale = Vector3.one;

        [Header("Boss")]
        public MidBossDefinition boss = new MidBossDefinition();

        public BossArenaPresentationConfig GetPresentationConfig()
        {
            if (presentation == null)
            {
                presentation = new BossArenaPresentationConfig();
            }

            return presentation;
        }
    }

    public enum BossArenaEntranceSide
    {
        South,
        North,
        West,
        East
    }

    [System.Serializable]
    public class BossArenaPresentationConfig
    {
        public bool enabled = true;

        [Header("Entrance")]
        public BossArenaEntranceSide entranceSide = BossArenaEntranceSide.South;
        [Min(2)] public int entranceWidthInCells = 4;
        [Min(3)] public int approachLengthInCells = 9;

        [Header("Animation")]
        [Min(0.1f)] public float pulseSpeed = 1.4f;
        [Range(0f, 1f)] public float approachOpacity = 0.72f;

        [Header("Rendering")]
        public Sprite entranceSprite;
        [Min(0.25f)] public float entranceVisualScale = 1.35f;

        [Header("Biome Wall Tiles")]
        public Sprite[] wallStraightSprites;
        public Sprite wallCornerSprite;
        [Range(1, 4)] public int wallCornerSpanInCells = 2;
        [Tooltip("NW 코너에 적용할 원본 코너 스프라이트의 Y 회전값")]
        public float wallCornerBaseYaw = 270f;
        [Tooltip("코너의 가로·세로 접속 셀을 동일한 직선 벽 조각으로 덮어 연결한다.")]
        public bool wallUseStraightCornerConnectors;

        public int floorSortingOrder = 40;
        public int gateSortingOrder = 3490;

        [Header("Optional Palette Override")]
        [Tooltip("꺼면 장·간·위·폐 기본 팔레트를 사용한다.")]
        public bool useBiomePalette = true;
        public Color primaryColor = new Color(0.43f, 0.72f, 0.22f, 1f);
        public Color accentColor = new Color(0.82f, 0.48f, 0.16f, 1f);
        public Color lockedColor = new Color(0.92f, 0.12f, 0.08f, 1f);
        public Color clearedColor = new Color(0.34f, 0.32f, 0.3f, 1f);
    }

    [System.Serializable]
    public class MidBossDefinition
    {
        public string displayName = "MidBoss";
        public bool useCustomBossRule = false;
        public EnemySpawnRuleConfig bossRule;
        public bool useEnemyRuleFallback = true;
        public int fallbackEnemyRuleIndex = 0;
        public MidBossPatternType patternType = MidBossPatternType.Auto;

        [Header("Optional Overrides")]
        public bool overrideStats = false;
        public float maxHealthMultiplier = 1f;
        public float attackDamageMultiplier = 1f;
        public float moveSpeedMultiplier = 1f;
        public Vector3 scaleMultiplier = Vector3.one;

        [Header("Boss Health")]
        [Tooltip("0이면 bossRule의 maxHealth를 그대로 사용합니다. 0보다 크면 최소 체력으로 보정합니다.")]
        public float minimumMaxHealth = 0f;

        [Header("Pattern Settings")]
        public IntestineBossPatternSettings intestinePattern = new IntestineBossPatternSettings();
        public LiverBossPatternSettings liverPattern = new LiverBossPatternSettings();
        public StomachBossPatternSettings stomachPattern = new StomachBossPatternSettings();
        public LungBossPatternSettings lungPattern = new LungBossPatternSettings();
    }

    public enum MidBossPatternType
    {
        Auto,
        None,
        Intestine,
        Liver,
        Stomach,
        Lung
    }
}
