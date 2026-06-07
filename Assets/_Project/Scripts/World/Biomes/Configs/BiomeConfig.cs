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

        [Header("Object Spawn Area Padding")]
        public int marginLeft;
        public int marginRight;
        public int marginBottom;
        public int marginTop;

        [Header("Objects")]
        public List<BiomeObjectRuleConfig> objectRules = new List<BiomeObjectRuleConfig>();

        [Header("Enemy Config")]
        public EnemySpawnConfig enemySpawnConfig;

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
    public class EnemySpawnRuleConfig
    {
        public string name = "Enemy";

        [Header("Poisson")]
        public float density = 0.0025f;
        public float minDistance = 8f;
        public int poissonSalt = 400;

        [Tooltip("비워두면 모든 지역에서 허용")]
        public List<int> allowedRegions = new List<int>();

        [Header("Spawner")]
        public int maxAlive = 1;
        public float activationRadius = 20f;
        public float respawnCooldown = 8f;
        public float spawnRadius = 1.5f;

        [Header("Movement")]
        public float moveSpeed = 1.5f;
        public float stoppingDistance = 0.1f;
        public float wanderRadius = 4f;
        public float chaseRadius = 6f;
        public float leashRadius = 8f;
        public Vector2 idleDelayRange = new Vector2(0.5f, 1.5f);

        [Header("Combat")]
        public float maxHealth = 30f;
        public float attackDamage = 1f;
        public float attackRange = 1.5f;
        public float attackCooldown = 1f;
        public int expReward = 10;

        [Header("Additional Stats")]
        public List<CharacterStatValue> additionalBaseStats = new List<CharacterStatValue>();

        [Header("Separation")]
        public float separationDistance = 1.1f;
        public float separationStrength = 1f;

        [Header("Visual")]
        public float heightOffset = 0f;
        public Vector3 scale = Vector3.one;
        public int sortingOrder = 1000;
        public bool useBillboard = true;
        public bool useYSort = true;
        public float animationSpeed = 0.15f;

        [Header("Physics")]
        public bool addCollider = true;
        public bool isTrigger = false;
        public Vector3 colliderSize = new Vector3(0.7f, 1.1f, 0.7f);
        public Vector3 colliderCenter = new Vector3(0f, 0.55f, 0f);

        [Header("Sprites - Idle / Move")]
        public Sprite[] idleSprites;
        public Sprite[] moveSprites;

        [Header("Sprites - Attack")]
        public Sprite[] attackSprites;         // 기본 공격 (좌우는 flipX로 처리)
        public Sprite[] attackSpritesUp;       // 상방 공격 (NK세포 등 방향별 공격용)
        public Sprite[] attackSpritesDown;     // 하방 공격
        public float attackAnimationSpeed = 0.12f;

        [Header("Ranged Attack (원거리 공격)")]
        public bool isRanged = false;
        public float projectileSpeed = 8f;
        public float projectileLifeTime = 3f;
        public Sprite projectileSprite;
        public Vector3 projectileScale = new Vector3(0.4f, 0.4f, 0.4f);
        public float projectileSpawnOffset = 0.5f;

        [Header("Attack Collider (대식세포 등 공격 시 콜라이더 확장)")]
        public bool expandColliderOnAttack = false;
        public Vector3 attackColliderSize = new Vector3(2f, 2f, 2f);
        public Vector3 attackColliderCenter = new Vector3(0f, 0.55f, 0f);

        [Header("Sprites - Death")]
        public Sprite[] deathSprites;
        public float deathAnimationSpeed = 0.15f;

        [Header("Elite")]
        public bool isElite = false;
        public Color tintColor = Color.white;

        [Tooltip("이 엘리트를 소환하기 위해 잡아야 하는 일반 적 이름")]
        public string killTriggerEnemyName = "";
        [Tooltip("소환에 필요한 킬 수")]
        public int killTriggerCount = 10;

        [Header("Elite - Split on Death (육아종)")]
        public bool splitsOnDeath = false;
        public int splitCount = 2;
        public string splitEnemyName = "";

        [Header("Elite - Split VFX (분열 이펙트)")]
        public Sprite[] splitVfxSprites;
        public float splitVfxScale = 3f;
        public float splitVfxSpeed = 0.08f;
        public float splitVfxDuration = 0.6f;

        [Header("Elite - Charge (항체)")]
        public bool chargesAtPlayer = false;
        public float chargeSpeed = 6f;
        public float chargeAccelTime = 0.3f;

        [Header("Elite - Aggro Debris (항체 잔해)")]
        public bool leavesDebrisOnDeath = false;
        public float debrisDuration = 5f;
        public float debrisAggroRadius = 8f;

        [Header("Elite - Debris VFX (충격파 이펙트)")]
        public Sprite[] debrisVfxSprites;
        public float debrisVfxScale = 15f;
        public float debrisVfxSpeed = 0.12f;
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
        public Vector2Int arenaSize = new Vector2Int(26, 26);
        public int wallThicknessInCells = 1;
        [Tooltip("안개 벽보다 안쪽으로 봉쇄 경계를 들여놓을 칸 수")]
        public int lockBoundaryInsetInCells = 1;
        [Tooltip("안개 벽 안쪽 모서리에서 추가로 진입 트리거를 들여놓을 칸 수")]
        public int triggerInsetInCells = 2;

        [Header("Visual")]
        public float wallHeight = 4f;
        public float wallHeightOffset = 1.5f;
        public float groundFogOffset = 0.15f;
        public float triggerHeight = 4f;
        public int sortingOrder = 3500;
        public Sprite fogSprite;
        public Color unlockedFogColor = new Color(1f, 1f, 1f, 0.85f);
        public Color lockedFogColor = new Color(1f, 1f, 1f, 0.35f);

        [Header("Fog Reveal")]
        public bool useInteriorFogCover = true;
        public Color interiorFogColor = Color.white;
        [Range(0f, 1f)] public float interiorFogHiddenAlpha = 1f;
        [Range(0f, 1f)] public float interiorFogRevealedAlpha = 0f;
        public float fogRevealDuration = 1.4f;
        public int interiorFogSortingOrderOffset = 3000;

        [Header("Return Portal")]
        public Sprite returnPortalSprite;
        public Vector3 returnPortalScale = Vector3.one;

        [Header("Boss Contact")]
        public float bossContactDamage = 1f;
        public float bossContactDamageCooldown = 1f;
        public float bossContactPushSpeed = 5f;

        [Header("Boss")]
        public MidBossDefinition boss = new MidBossDefinition();
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
