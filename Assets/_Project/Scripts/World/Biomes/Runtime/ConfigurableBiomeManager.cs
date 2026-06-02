using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace Necrocis
{
    /// <summary>
    /// BiomeConfig로 동작하는 범용 바이옴 매니저
    /// </summary>
    public class ConfigurableBiomeManager : RegionPoissonBiomeManager
    {
        [Header("Biome Config")]
        [SerializeField] private BiomeConfig config;

        private BiomePerlinNoise detailNoise;
        private readonly List<BiomeObjectRuleConfig> runtimeRules = new List<BiomeObjectRuleConfig>();
        private readonly List<EnemySpawnRuleConfig> runtimeEnemyRules = new List<EnemySpawnRuleConfig>();
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

            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            TryCreateMidBossArena();
        }

        protected override void InitializeNoise()
        {
            base.InitializeNoise();
            detailNoise = new BiomePerlinNoise(seed);
            detailNoise.SetFrequency(config.detailNoiseScale);
        }

        protected override TileSample SampleBaseTile(int worldX, int worldY)
        {
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

        protected override bool IsObjectAreaAllowed(int x, int y)
        {
            if (config == null) return true;

            int left = Mathf.Max(0, config.marginLeft);
            int right = Mathf.Max(0, config.marginRight);
            int bottom = Mathf.Max(0, config.marginBottom);
            int top = Mathf.Max(0, config.marginTop);

            return x >= left && x < mapWidth - right && y >= bottom && y < mapHeight - top;
        }

        protected override float GetDensityForRule(ObjectRule rule, int worldX, int worldY, int regionType)
        {
            float density = base.GetDensityForRule(rule, worldX, worldY, regionType);
            if (density <= 0f)
            {
                return 0f;
            }

            if (rule.category == SpawnCategory.EnemySpawner && IsInsideMidBossArenaBounds(worldX, worldY))
            {
                return 0f;
            }

            return density;
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
                        configIndex = runtimeRules.Count
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
            midBossArenaController.Configure(this, midBossArenaConfig, runtimeEnemyRules, config.GetReturnPortalConfig());
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
                : new Vector2Int(mapWidth / 2, mapHeight / 2);

            int halfWidth = Mathf.Max(4, midBossArenaConfig.arenaSize.x / 2);
            int halfHeight = Mathf.Max(4, midBossArenaConfig.arenaSize.y / 2);

            return gridX >= center.x - halfWidth
                && gridX <= center.x + halfWidth
                && gridY >= center.y - halfHeight
                && gridY <= center.y + halfHeight;
        }

        private void SpawnConfiguredObject(BiomeObjectRuleConfig rule, ChunkSpawnRecord record, Chunk chunk)
        {
            if (rule.sprites == null || rule.sprites.Length == 0) return;

            int x = record.x;
            int y = record.y;
            ObjectId id = new ObjectId(x, y, record.objectKind);
            string baseName = string.IsNullOrEmpty(rule.name) ? rule.poolKind.ToString() : rule.name;
            ObjectPoolKey poolKey = GetPoolKey(record);
            GameObject obj = AcquireObject(poolKey, $"{baseName}_{x}_{y}");
            obj.transform.position = GridToWorldWithHeight(x, y, rule.heightOffset);

            SpriteRenderer sr = GetOrAddComponent<SpriteRenderer>(obj);
            sr.sortingOrder = rule.sortingOrder;

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

            ConfigureBillboard(obj, rule.useBillboard);
            ConfigureYSort(obj, rule.useYSort, rule.sortingOrder);
            ConfigureCollider(obj, rule);

            RegisterObject(chunk, obj, id, poolKey, rule.blocksMovement);
            ActivateSpawnedObject(obj);
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
