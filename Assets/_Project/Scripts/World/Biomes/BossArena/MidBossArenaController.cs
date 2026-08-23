using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 맵 중앙 중간보스 구역을 관리한다.
    /// 진입 후 봉쇄, 보스 전투, 보스 처치 후 해제를 담당한다.
    /// </summary>
    [DisallowMultipleComponent]
    public class MidBossArenaController : MonoBehaviour
    {
        private static Sprite runtimeBossSprite;
        private static Sprite runtimePortalSprite;
        private static readonly List<MidBossArenaController> ActiveArenas = new List<MidBossArenaController>();

        private readonly List<Vector2Int> blockedBoundaryCells = new List<Vector2Int>();
        private readonly List<Vector2Int> approachBoundaryCells = new List<Vector2Int>();
        private readonly HashSet<Renderer> concealedBossRenderers = new HashSet<Renderer>();

        private BiomeManager biome;
        private MidBossArenaConfig arenaConfig;
        private BiomeReturnPortalConfig returnPortalConfig;
        private EnemySpawnRuleConfig bossRule;

        private EnemyController activeBoss;
        private IntestineBossPattern activeIntestinePattern;
        private LiverBossPattern activeLiverPattern;
        private StomachBossPattern activeStomachPattern;
        private LungBossPattern activeLungPattern;
        private readonly List<EnemyContactDamage> activeContactDamage = new List<EnemyContactDamage>();
        private Vector2Int centerGrid;
        private Vector2Int arenaSize;
        private bool arenaLocked;
        private bool bossDefeated;
        private bool bossIntroPlaying;
        private bool approachBoundaryActive;
        private BossArenaPresentation arenaPresentation;

        public bool IsLocked => arenaLocked;

        public void Configure(
            BiomeManager biome,
            MidBossArenaConfig arenaConfig,
            IList<EnemySpawnRuleConfig> availableEnemyRules,
            BiomeReturnPortalConfig returnPortalConfig = null)
        {
            this.biome = biome;
            this.arenaConfig = arenaConfig;
            this.returnPortalConfig = returnPortalConfig;
            bossRule = ResolveBossRule(availableEnemyRules);

            centerGrid = ResolveCenterGrid();
            arenaSize = new Vector2Int(
                Mathf.Max(8, arenaConfig.arenaSize.x),
                Mathf.Max(8, arenaConfig.arenaSize.y));

            transform.position = ResolveArenaCenterWorld();
            transform.name = "MidBossArena";

            BuildBoundaryCellCache();
            ActivateApproachBoundary();
            BuildTrigger();
            arenaPresentation = BossArenaPresentation.Create(transform, biome, arenaSize, arenaConfig);

            SpawnBoss();
        }

        private void OnEnable()
        {
            if (!ActiveArenas.Contains(this))
            {
                ActiveArenas.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveArenas.Remove(this);
            BossIntroPresentation.Cancel(this);
            bossIntroPlaying = false;
        }

        private void Update()
        {
            if (!arenaLocked || bossDefeated)
                return;

            EnforcePlayerInsidePlayableBounds();

            if (activeLungPattern != null)
            {
                if (activeLungPattern.IsEncounterDefeated)
                {
                    UnlockArena();
                }

                return;
            }

            if (activeBoss == null || activeBoss.IsDead || !activeBoss.gameObject.activeInHierarchy)
            {
                UnlockArena();
            }
        }

        private void LateUpdate()
        {
            if (!arenaLocked || bossDefeated)
            {
                return;
            }

            EnforcePlayerInsidePlayableBounds();
        }

        private void OnTriggerEnter(Collider other)
        {
            TryActivateFromCollider(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryActivateFromCollider(other);
        }

        private void TryActivateFromCollider(Collider other)
        {
            if (arenaLocked || bossDefeated)
            {
                return;
            }

            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null)
            {
                player = other.GetComponentInParent<PlayerController>();
            }

            if (player == null)
            {
                return;
            }

            Vector2 playerHalfExtents = GetPlayerClampExtents(player);
            ArenaWorldBounds bounds = GetArenaWorldBounds(playerHalfExtents);
            if (!bounds.ContainsPlayableCenter(player.transform.position))
            {
                return;
            }

            TryActivateArena(player);
        }

        private void OnDestroy()
        {
            ActiveArenas.Remove(this);
            BossIntroPresentation.Cancel(this);

            if (activeBoss != null)
                activeBoss.Defeated -= HandleBossDefeated;

            if (biome != null)
            {
                if (arenaLocked)
                {
                    biome.RemoveRuntimeBlockedCells(blockedBoundaryCells);
                }

                DeactivateApproachBoundary();
            }
        }

        private void TryActivateArena(PlayerController player)
        {
            if (biome == null)
            {
                Debug.LogWarning("[MidBossArena] 보스 룰이 없어 아레나를 활성화할 수 없습니다.");
                return;
            }

            if (activeBoss == null || activeBoss.IsDead || !activeBoss.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[MidBossArena] 중간보스가 없거나 이미 처치되어 봉쇄를 시작하지 않습니다.");
                return;
            }

            arenaLocked = true;
            DeactivateApproachBoundary();
            biome.AddRuntimeBlockedCells(blockedBoundaryCells);
            arenaPresentation?.SetState(BossArenaPresentationState.Locked);
            RecenterBossEncounter();
            SetBossEncounterActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameState(GameState.InBossRoom);
            }

            StartBossIntroOrEncounter();

            Debug.Log($"[MidBossArena] 중간보스 구역 진입 - 탈출 차단 활성화 ({biome.BiomeType})");
        }

        private void StartBossIntroOrEncounter()
        {
            bossIntroPlaying = TryPlayBossIntro();
            if (!bossIntroPlaying)
            {
                BeginBossEncounter();
            }
        }

        private bool TryPlayBossIntro()
        {
            List<SpriteRenderer> renderers = new List<SpriteRenderer>(2);
            if (activeLungPattern != null)
            {
                activeLungPattern.ForEachEncounterBoss(boss =>
                {
                    SpriteRenderer renderer = FindBossPortraitRenderer(boss);
                    if (renderer != null && !renderers.Contains(renderer))
                    {
                        renderers.Add(renderer);
                    }
                });
            }
            else
            {
                SpriteRenderer renderer = FindBossPortraitRenderer(activeBoss);
                if (renderer != null)
                {
                    renderers.Add(renderer);
                }
            }

            BiomeType encounterBiome = biome != null ? biome.BiomeType : BiomeType.None;
            return BossIntroPresentation.Show(this, encounterBiome, renderers, HandleBossIntroCompleted);
        }

        private void HandleBossIntroCompleted()
        {
            bossIntroPlaying = false;
            if (!arenaLocked || bossDefeated || !isActiveAndEnabled)
            {
                return;
            }

            BeginBossEncounter();
        }

        private void BeginBossEncounter()
        {
            if (!arenaLocked || bossDefeated)
            {
                return;
            }

            SetBossEncounterActive(true);
            PlayBossEncounterVfx();
            AudioManager.Instance?.PlayTimedSFX("BossSpawn", 5f);
            PlayBossEncounterImpactSfx();
        }

        private static SpriteRenderer FindBossPortraitRenderer(EnemyController boss)
        {
            if (boss == null)
            {
                return null;
            }

            SpriteRenderer[] renderers = boss.GetComponentsInChildren<SpriteRenderer>(true);
            SpriteRenderer best = null;
            float bestArea = -1f;
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer candidate = renderers[i];
                if (candidate == null || candidate.sprite == null)
                {
                    continue;
                }

                Rect rect = candidate.sprite.rect;
                float area = rect.width * rect.height;
                if (area <= bestArea)
                {
                    continue;
                }

                best = candidate;
                bestArea = area;
            }

            return best;
        }

        private void SpawnBoss()
        {
            if (activeBoss != null && activeBoss.gameObject.activeInHierarchy)
            {
                return;
            }

            if (bossRule == null)
            {
                return;
            }

            Vector3 bossSpawnPosition = ResolveArenaCenterWorld(bossRule.heightOffset);
            int poolArchetypeId = EnemyController.GetPoolArchetypeId(bossRule);
            activeBoss = EnemyController.Acquire(transform, $"{bossRule.name}_MidBoss", poolArchetypeId);
            activeBoss.Configure(null, bossRule, bossSpawnPosition, bossSpawnPosition);
            activeBoss.transform.SetParent(transform, true);
            activeBoss.SetIgnoreMidBossArenaRestriction(true);
            activeBoss.Defeated -= HandleBossDefeated;
            activeBoss.Defeated += HandleBossDefeated;
            ConfigureBiomeSpecificBossPattern(activeBoss, bossSpawnPosition);
            RegisterBossContactDamage(activeBoss);
            SetBossEncounterActive(false);
            RecenterBossEncounter();

            Debug.Log($"[MidBossArena] 중간보스 스폰: {bossRule.name} @ {bossSpawnPosition}");
        }

        private void ConfigureBiomeSpecificBossPattern(EnemyController boss, Vector3 bossSpawnPosition)
        {
            if (boss == null || biome == null)
            {
                return;
            }

            MidBossPatternType patternType = ResolveBossPatternType();
            IntestineBossPattern intestinePattern = boss.GetComponent<IntestineBossPattern>();
            LiverBossPattern liverPattern = boss.GetComponent<LiverBossPattern>();
            StomachBossPattern stomachPattern = boss.GetComponent<StomachBossPattern>();
            LungBossPattern lungPattern = GetComponent<LungBossPattern>();
            activeIntestinePattern = null;
            activeLiverPattern = null;
            activeStomachPattern = null;
            activeLungPattern = null;

            if (patternType == MidBossPatternType.Intestine)
            {
                if (liverPattern != null)
                {
                    liverPattern.enabled = false;
                }

                if (stomachPattern != null)
                {
                    stomachPattern.enabled = false;
                }

                if (lungPattern != null)
                {
                    lungPattern.enabled = false;
                }

                if (intestinePattern == null)
                {
                    intestinePattern = boss.gameObject.AddComponent<IntestineBossPattern>();
                }

                intestinePattern.Initialize(boss, bossSpawnPosition, transform, arenaConfig?.boss?.intestinePattern);
                intestinePattern.SetEncounterActive(false);
                activeIntestinePattern = intestinePattern;
                return;
            }

            if (patternType == MidBossPatternType.Liver)
            {
                if (intestinePattern != null)
                {
                    intestinePattern.enabled = false;
                }

                if (stomachPattern != null)
                {
                    stomachPattern.enabled = false;
                }

                if (lungPattern != null)
                {
                    lungPattern.enabled = false;
                }

                if (liverPattern == null)
                {
                    liverPattern = boss.gameObject.AddComponent<LiverBossPattern>();
                }

                liverPattern.Initialize(boss, bossSpawnPosition, transform, arenaConfig?.boss?.liverPattern);
                liverPattern.SetEncounterActive(false);
                activeLiverPattern = liverPattern;
                return;
            }

            if (patternType == MidBossPatternType.Stomach)
            {
                if (intestinePattern != null)
                {
                    intestinePattern.enabled = false;
                }

                if (liverPattern != null)
                {
                    liverPattern.enabled = false;
                }

                if (lungPattern != null)
                {
                    lungPattern.enabled = false;
                }

                if (stomachPattern == null)
                {
                    stomachPattern = boss.gameObject.AddComponent<StomachBossPattern>();
                }

                stomachPattern.Initialize(boss, bossSpawnPosition, transform, arenaConfig?.boss?.stomachPattern);
                stomachPattern.SetEncounterActive(false);
                activeStomachPattern = stomachPattern;
                return;
            }

            if (patternType == MidBossPatternType.Lung)
            {
                if (intestinePattern != null)
                {
                    intestinePattern.enabled = false;
                }

                if (liverPattern != null)
                {
                    liverPattern.enabled = false;
                }

                if (stomachPattern != null)
                {
                    stomachPattern.enabled = false;
                }

                if (lungPattern == null)
                {
                    lungPattern = gameObject.AddComponent<LungBossPattern>();
                }

                activeLungPattern = lungPattern;
                lungPattern.Initialize(boss, bossSpawnPosition, transform, arenaConfig?.boss?.lungPattern);
                lungPattern.SetEncounterActive(false);
                lungPattern.ForEachEncounterBoss(RegisterBossContactDamage);
                return;
            }

            if (intestinePattern != null)
            {
                intestinePattern.enabled = false;
            }

            if (liverPattern != null)
            {
                liverPattern.enabled = false;
            }

            if (stomachPattern != null)
            {
                stomachPattern.enabled = false;
            }

            if (lungPattern != null)
            {
                lungPattern.enabled = false;
            }

            boss.SetAiSuppressed(true);
        }

        private void SetBossEncounterActive(bool active)
        {
            bool hasPattern = activeIntestinePattern != null
                || activeLiverPattern != null
                || activeStomachPattern != null
                || activeLungPattern != null;

            if (activeBoss != null && !activeBoss.IsDead)
            {
                activeBoss.SetAiSuppressed(!active || hasPattern);
            }

            if (active)
            {
                RevealBossVisuals();
            }
            else if (!bossDefeated)
            {
                ConcealBossVisuals();
            }

            activeIntestinePattern?.SetEncounterActive(active);
            activeLiverPattern?.SetEncounterActive(active);
            activeStomachPattern?.SetEncounterActive(active);
            activeLungPattern?.SetEncounterActive(active);

            for (int i = 0; i < activeContactDamage.Count; i++)
            {
                if (activeContactDamage[i] != null)
                {
                    activeContactDamage[i].SetDamageActive(active && !bossDefeated);
                }
            }
        }

        private void ConcealBossVisuals()
        {
            if (arenaConfig == null || !arenaConfig.hideBossUntilEncounter)
            {
                return;
            }

            concealedBossRenderers.RemoveWhere(renderer => renderer == null);
            ConcealBossRenderers(activeBoss);
            activeLungPattern?.ForEachEncounterBoss(ConcealBossRenderers);
        }

        private void ConcealBossRenderers(EnemyController boss)
        {
            if (boss == null)
            {
                return;
            }

            Renderer[] renderers = boss.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null && renderer.enabled && concealedBossRenderers.Add(renderer))
                {
                    renderer.enabled = false;
                }
            }
        }

        private void RevealBossVisuals()
        {
            foreach (Renderer renderer in concealedBossRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }

            concealedBossRenderers.Clear();
        }

        private void RecenterBossEncounter()
        {
            if (activeBoss == null || biome == null || bossRule == null)
            {
                return;
            }

            Vector3 center = ResolveArenaCenterWorld(bossRule.heightOffset);
            if (activeLungPattern != null)
            {
                activeLungPattern.RecenterEncounter();
                return;
            }

            activeBoss.transform.position = center;
        }

        private void RegisterBossContactDamage(EnemyController boss)
        {
            if (boss == null)
            {
                return;
            }

            EnemyContactDamage contactDamage = boss.GetComponent<EnemyContactDamage>();
            if (contactDamage == null)
            {
                contactDamage = boss.gameObject.AddComponent<EnemyContactDamage>();
            }

            contactDamage.SetDamageActive(arenaLocked && !bossDefeated);

            if (!activeContactDamage.Contains(contactDamage))
            {
                activeContactDamage.Add(contactDamage);
            }

            Collider bossCollider = boss.GetComponent<Collider>();
            if (bossCollider != null)
            {
                bossCollider.isTrigger = true;
            }
        }

        private MidBossPatternType ResolveBossPatternType()
        {
            MidBossPatternType configuredType = arenaConfig != null && arenaConfig.boss != null
                ? arenaConfig.boss.patternType
                : MidBossPatternType.Auto;

            if (configuredType != MidBossPatternType.Auto)
            {
                return configuredType;
            }

            if (biome == null)
            {
                return MidBossPatternType.None;
            }

            return biome.BiomeType switch
            {
                BiomeType.Intestine => MidBossPatternType.Intestine,
                BiomeType.Liver => MidBossPatternType.Liver,
                BiomeType.Stomach => MidBossPatternType.Stomach,
                BiomeType.Lung => MidBossPatternType.Lung,
                _ => MidBossPatternType.None
            };
        }

        private void UnlockArena()
        {
            if (!arenaLocked)
            {
                return;
            }

            arenaLocked = false;
            bossDefeated = true;
            arenaPresentation?.SetState(BossArenaPresentationState.Cleared);
            BossIntroPresentation.Cancel(this);
            bossIntroPlaying = false;
            SetBossEncounterActive(false);
            PlayBossDeathSfx();

            if (biome != null)
            {
                biome.RemoveRuntimeBlockedCells(blockedBoundaryCells);
            }

            Vector3 returnPortalPosition = ResolveReturnPortalPosition();
            Vector3 bossDeathPos = activeBoss != null
                ? activeBoss.transform.position
                : returnPortalPosition;
            EnemyController defeatedBoss = activeBoss;

            if (defeatedBoss != null)
            {
                EnemyProjectile.ReturnProjectilesOwnedBy(defeatedBoss);
                defeatedBoss.Defeated -= HandleBossDefeated;
                activeBoss = null;
            }

            if (activeLungPattern != null)
            {
                activeLungPattern.DisposeEncounter();
                activeLungPattern = null;
            }

            if (GameManager.Instance != null)
            {
                if (biome != null)
                {
                    GameManager.Instance.CollectRelic(biome.BiomeType);
                }

                GameManager.Instance.SetGameState(GameState.InBiome);
            }

            SpawnReturnPortal(returnPortalPosition);
            SpawnBonusItemDrop(bossDeathPos);

            Debug.Log("[MidBossArena] 중간보스 처치 - 봉쇄 해제, 귀환 포탈 생성");
        }

        private void PlayBossDeathSfx()
        {
            string soundKey = biome != null
                ? biome.BiomeType switch
                {
                    BiomeType.Intestine => "IntestineBossDeath",
                    BiomeType.Liver => "LiverBossDeath",
                    BiomeType.Stomach => "StomachBossDeath",
                    BiomeType.Lung => "LungBossDeath",
                    _ => "BossDeath"
                }
                : "BossDeath";

            AudioManager.Instance?.PlaySFX(soundKey);
        }

        private void PlayBossEncounterImpactSfx()
        {
            if (biome == null)
            {
                return;
            }

            switch (biome.BiomeType)
            {
                case BiomeType.Intestine:
                    AudioManager.Instance?.PlaySFX("IntestineBossImpact");
                    break;
                case BiomeType.Liver:
                    AudioManager.Instance?.PlayTimedSFX("LiverBossImpact", 0.8f);
                    break;
                case BiomeType.Stomach:
                    AudioManager.Instance?.PlaySFX("StomachBossImpact");
                    break;
            }
        }

        private void PlayBossEncounterVfx()
        {
            BiomeType encounterBiome = biome != null ? biome.BiomeType : BiomeType.None;
            if (activeLungPattern != null)
            {
                bool addCameraShake = true;
                activeLungPattern.ForEachEncounterBoss(boss =>
                {
                    CombatVfx.PlayBossEncounter(boss, encounterBiome, addCameraShake);
                    addCameraShake = false;
                });
                return;
            }

            CombatVfx.PlayBossEncounter(activeBoss, encounterBiome);
        }

        private Vector3 ResolveReturnPortalPosition()
        {
            if (biome == null)
            {
                return transform.position;
            }

            float heightOffset = returnPortalConfig != null ? returnPortalConfig.heightOffset : 0f;
            return ResolveArenaCenterWorld(heightOffset);
        }

        private void SpawnReturnPortal(Vector3 portalPos)
        {
            if (returnPortalConfig != null && !returnPortalConfig.enabled)
            {
                return;
            }

            string portalName = returnPortalConfig != null && !string.IsNullOrWhiteSpace(returnPortalConfig.name)
                ? returnPortalConfig.name
                : "BossReturnPortal";
            GameObject portalObj = new GameObject(portalName);
            portalObj.transform.SetParent(transform, true);
            portalObj.transform.position = portalPos;
            portalObj.transform.localScale = returnPortalConfig != null
                ? GetSafeScaleMultiplier(returnPortalConfig.scale)
                : Vector3.one;

            SpriteRenderer sr = portalObj.AddComponent<SpriteRenderer>();
            bool hasConfiguredSprite = returnPortalConfig != null && returnPortalConfig.sprite != null;
            bool hasArenaSprite = !hasConfiguredSprite && arenaConfig != null && arenaConfig.returnPortalSprite != null;
            sr.sprite = hasConfiguredSprite
                ? returnPortalConfig.sprite
                : hasArenaSprite ? arenaConfig.returnPortalSprite : GetRuntimePortalSprite();
            sr.color = hasConfiguredSprite || hasArenaSprite
                ? Color.white
                : new Color(0.6f, 0.2f, 1f, 0.85f);
            sr.sortingOrder = returnPortalConfig != null
                ? returnPortalConfig.sortingOrder
                : arenaConfig != null ? arenaConfig.sortingOrder : 3500;

            if (returnPortalConfig == null && hasArenaSprite)
            {
                portalObj.transform.localScale = GetSafeScaleMultiplier(arenaConfig.returnPortalScale);
            }

            if (returnPortalConfig == null || returnPortalConfig.useBillboard)
            {
                Billboard billboard = portalObj.AddComponent<Billboard>();
                billboard.SetUpdateMode(Billboard.UpdateMode.Once);
            }

            SpriteYSort ySort = portalObj.AddComponent<SpriteYSort>();
            ySort.Configure(SpriteYSort.WorldDynamicBaseSortingOrder, true, SpriteYSort.WorldDynamicMinSortingOrder);
            ySort.SetUpdateMode(SpriteYSort.UpdateMode.Once);

            if (returnPortalConfig == null || returnPortalConfig.addCollider)
            {
                BoxCollider col = portalObj.AddComponent<BoxCollider>();
                col.isTrigger = returnPortalConfig == null || returnPortalConfig.isTrigger;
                col.size = GetSafeColliderSize(returnPortalConfig != null ? returnPortalConfig.colliderSize : new Vector3(2f, 2f, 2f));
                col.center = returnPortalConfig != null ? returnPortalConfig.colliderCenter : Vector3.zero;
            }

            ReturnPortal portal = portalObj.AddComponent<ReturnPortal>();
            portal.SetActive(true);
        }

        private void SpawnBonusItemDrop(Vector3 bossDeathPos)
        {
            if (biome == null)
            {
                return;
            }

            WorldItemSpawner spawner = biome.GetComponent<WorldItemSpawner>();
            if (spawner == null)
            {
                spawner = biome.gameObject.AddComponent<WorldItemSpawner>();
            }

            bool spawnedItem = spawner.TrySpawnSingleRandomItemAt(bossDeathPos);
            if (!spawnedItem)
            {
                Debug.Log("[MidBossArena] 보스 보너스 아이템 드랍 위치를 찾지 못했습니다.");
            }
        }

        private void BuildTrigger()
        {
            BoxCollider trigger = GetComponent<BoxCollider>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<BoxCollider>();
            }

            float tileSize = Mathf.Max(0.01f, biome.TileSize);
            int entranceWidth = Mathf.Clamp(
                arenaConfig.GetPresentationConfig().entranceWidthInCells,
                2,
                Mathf.Max(2, arenaSize.x - 2));
            float triggerWidth = entranceWidth * tileSize;
            float triggerDepth = Mathf.Max(tileSize, arenaConfig.triggerInsetInCells * tileSize);
            ArenaWorldBounds arenaBounds = GetArenaWorldBounds(Vector2.zero);
            float southInnerEdgeLocal = arenaBounds.playableMinZ - transform.position.z;

            trigger.isTrigger = true;
            trigger.size = new Vector3(triggerWidth, arenaConfig.triggerHeight, triggerDepth);
            trigger.center = new Vector3(
                0f,
                arenaConfig.wallHeightOffset,
                southInnerEdgeLocal + triggerDepth * 0.5f);
        }

        private void HandleBossDefeated(EnemyController boss)
        {
            if (boss == null || boss != activeBoss || bossDefeated)
            {
                return;
            }

            if (activeLungPattern != null && !activeLungPattern.IsEncounterDefeated)
            {
                return;
            }

            UnlockArena();
        }

        public bool ContainsWorldPosition(Vector3 worldPosition)
        {
            return GetArenaWorldBounds(Vector2.zero).ContainsOuter(worldPosition);
        }

        public static bool CanPlayerTraverseArenaBoundary(
            Vector3 currentPosition,
            Vector3 desiredPosition,
            Vector2 playerHalfExtents)
        {
            for (int i = 0; i < ActiveArenas.Count; i++)
            {
                MidBossArenaController arena = ActiveArenas[i];
                if (arena != null
                    && !arena.CanTraverseBoundary(currentPosition, desiredPosition, playerHalfExtents))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TryClampPlayerMovementInsideLockedArena(
            Vector3 currentPosition,
            Vector3 desiredPosition,
            float margin,
            out Vector3 clampedPosition)
        {
            clampedPosition = desiredPosition;
            MidBossArenaController arena = FindLockedArenaForMovement(currentPosition, desiredPosition);
            if (arena == null)
            {
                return false;
            }

            clampedPosition = arena.ClampToPlayableBounds(desiredPosition, margin);
            return true;
        }

        public static bool TryClampPositionInsideLockedArena(Vector3 position, float margin, out Vector3 clampedPosition)
        {
            clampedPosition = position;
            MidBossArenaController arena = FindLockedArenaForPosition(position) ?? FindSingleLockedArena();
            if (arena == null)
            {
                return false;
            }

            clampedPosition = arena.ClampToPlayableBounds(position, margin);
            return true;
        }

        public static bool IsPlayerInsideLockedArena(Vector3 playerPosition)
        {
            for (int i = 0; i < ActiveArenas.Count; i++)
            {
                MidBossArenaController arena = ActiveArenas[i];
                if (arena == null || !arena.IsLocked)
                {
                    continue;
                }

                if (arena.ContainsWorldPosition(playerPosition))
                {
                    return true;
                }
            }

            return false;
        }

        private static MidBossArenaController FindLockedArenaForMovement(Vector3 currentPosition, Vector3 desiredPosition)
        {
            MidBossArenaController fallback = null;

            for (int i = 0; i < ActiveArenas.Count; i++)
            {
                MidBossArenaController arena = ActiveArenas[i];
                if (arena == null || !arena.IsLocked)
                {
                    continue;
                }

                if (fallback == null)
                {
                    fallback = arena;
                }

                if (arena.ContainsWorldPosition(currentPosition) || arena.ContainsWorldPosition(desiredPosition))
                {
                    return arena;
                }
            }

            return CountLockedArenas() == 1 ? fallback : null;
        }

        private static MidBossArenaController FindLockedArenaForPosition(Vector3 position)
        {
            for (int i = 0; i < ActiveArenas.Count; i++)
            {
                MidBossArenaController arena = ActiveArenas[i];
                if (arena == null || !arena.IsLocked)
                {
                    continue;
                }

                if (arena.ContainsWorldPosition(position))
                {
                    return arena;
                }
            }

            return null;
        }

        private static MidBossArenaController FindSingleLockedArena()
        {
            MidBossArenaController single = null;
            int count = 0;

            for (int i = 0; i < ActiveArenas.Count; i++)
            {
                MidBossArenaController arena = ActiveArenas[i];
                if (arena == null || !arena.IsLocked)
                {
                    continue;
                }

                single = arena;
                count++;
            }

            return count == 1 ? single : null;
        }

        private static int CountLockedArenas()
        {
            int count = 0;
            for (int i = 0; i < ActiveArenas.Count; i++)
            {
                MidBossArenaController arena = ActiveArenas[i];
                if (arena != null && arena.IsLocked)
                {
                    count++;
                }
            }

            return count;
        }

        private Vector3 ClampToPlayableBounds(Vector3 worldPosition, float margin)
        {
            float safeMargin = Mathf.Max(0f, margin);
            return ClampToPlayableBounds(worldPosition, new Vector2(safeMargin, safeMargin));
        }

        private Vector3 ClampToPlayableBounds(Vector3 worldPosition, Vector2 margins)
        {
            if (biome == null || arenaConfig == null)
            {
                return worldPosition;
            }

            ArenaWorldBounds bounds = GetArenaWorldBounds(margins);
            worldPosition.x = Mathf.Clamp(worldPosition.x, bounds.playableMinX, bounds.playableMaxX);
            worldPosition.z = Mathf.Clamp(worldPosition.z, bounds.playableMinZ, bounds.playableMaxZ);
            return worldPosition;
        }

        private bool CanTraverseBoundary(
            Vector3 currentPosition,
            Vector3 desiredPosition,
            Vector2 playerHalfExtents)
        {
            if (bossDefeated || biome == null || arenaConfig == null)
            {
                return true;
            }

            IReadOnlyList<Vector2Int> solidCells = arenaLocked
                ? blockedBoundaryCells
                : approachBoundaryCells;
            if (solidCells == null || solidCells.Count == 0)
            {
                return true;
            }

            Vector2 safeHalfExtents = new Vector2(
                Mathf.Max(0f, playerHalfExtents.x),
                Mathf.Max(0f, playerHalfExtents.y));
            float currentOverlap = GetBoundaryOverlapScore(
                currentPosition,
                safeHalfExtents,
                solidCells);
            float desiredOverlap = GetBoundaryOverlapScore(
                desiredPosition,
                safeHalfExtents,
                solidCells);

            if (currentOverlap > 0f)
            {
                ArenaWorldBounds rawBounds = GetArenaWorldBounds(Vector2.zero);
                bool currentOutsideWall = !rawBounds.ContainsOuter(currentPosition);
                bool currentInsideWall = rawBounds.ContainsPlayableCenter(currentPosition);
                if ((currentOutsideWall && rawBounds.ContainsOuter(desiredPosition))
                    || (currentInsideWall && !rawBounds.ContainsPlayableCenter(desiredPosition)))
                {
                    return false;
                }

                return desiredOverlap + 0.000001f < currentOverlap;
            }

            if (desiredOverlap > 0f)
            {
                return false;
            }

            return !SegmentCrossesSolidBoundary(
                currentPosition,
                desiredPosition,
                safeHalfExtents,
                solidCells);
        }

        private float GetBoundaryOverlapScore(
            Vector3 position,
            Vector2 playerHalfExtents,
            IReadOnlyList<Vector2Int> solidCells)
        {
            float tileSize = Mathf.Max(0.01f, biome.TileSize);
            float score = 0f;
            for (int i = 0; i < solidCells.Count; i++)
            {
                Vector2Int cell = solidCells[i];
                Vector3 center = biome.GridToWorld(cell.x, cell.y);
                float minX = center.x - tileSize * 0.5f - playerHalfExtents.x;
                float maxX = center.x + tileSize * 0.5f + playerHalfExtents.x;
                float minZ = center.z - tileSize * 0.5f - playerHalfExtents.y;
                float maxZ = center.z + tileSize * 0.5f + playerHalfExtents.y;
                if (position.x <= minX
                    || position.x >= maxX
                    || position.z <= minZ
                    || position.z >= maxZ)
                {
                    continue;
                }

                float penetrationX = Mathf.Min(position.x - minX, maxX - position.x);
                float penetrationZ = Mathf.Min(position.z - minZ, maxZ - position.z);
                score += Mathf.Min(penetrationX, penetrationZ);
            }

            return score;
        }

        private bool SegmentCrossesSolidBoundary(
            Vector3 start,
            Vector3 end,
            Vector2 playerHalfExtents,
            IReadOnlyList<Vector2Int> solidCells)
        {
            float tileSize = Mathf.Max(0.01f, biome.TileSize);
            Vector3 delta = end - start;
            for (int i = 0; i < solidCells.Count; i++)
            {
                Vector2Int cell = solidCells[i];
                Vector3 center = biome.GridToWorld(cell.x, cell.y);
                float enter = 0f;
                float exit = 1f;
                if (!ClipSegmentAxis(
                        start.x,
                        delta.x,
                        center.x - tileSize * 0.5f - playerHalfExtents.x,
                        center.x + tileSize * 0.5f + playerHalfExtents.x,
                        ref enter,
                        ref exit)
                    || !ClipSegmentAxis(
                        start.z,
                        delta.z,
                        center.z - tileSize * 0.5f - playerHalfExtents.y,
                        center.z + tileSize * 0.5f + playerHalfExtents.y,
                        ref enter,
                        ref exit))
                {
                    continue;
                }

                if (exit - enter > 0.0001f)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ClipSegmentAxis(
            float start,
            float delta,
            float minimum,
            float maximum,
            ref float enter,
            ref float exit)
        {
            if (Mathf.Abs(delta) <= 0.000001f)
            {
                return start >= minimum && start <= maximum;
            }

            float first = (minimum - start) / delta;
            float second = (maximum - start) / delta;
            if (first > second)
            {
                (first, second) = (second, first);
            }

            enter = Mathf.Max(enter, first);
            exit = Mathf.Min(exit, second);
            return enter <= exit;
        }

        private void EnforcePlayerInsidePlayableBounds()
        {
            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                return;
            }

            Vector2 margins = GetPlayerClampExtents(player);
            Vector3 currentPosition = player.transform.position;
            Vector3 clampedPosition = ClampToPlayableBounds(currentPosition, margins);
            Vector3 planarDelta = clampedPosition - currentPosition;
            planarDelta.y = 0f;
            if (planarDelta.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            player.SpawnAt(clampedPosition);
        }

        private static Vector2 GetPlayerClampExtents(PlayerController player)
        {
            ProceduralTerrainMotor terrainMotor = player != null
                ? player.GetComponent<ProceduralTerrainMotor>()
                : null;
            if (terrainMotor != null)
            {
                return terrainMotor.TerrainHalfExtents;
            }

            Collider hitCollider = player != null ? player.HitCollider : null;
            if (hitCollider == null)
            {
                return new Vector2(0.5f, 0.5f);
            }

            return new Vector2(
                Mathf.Max(0.1f, hitCollider.bounds.extents.x),
                Mathf.Max(0.1f, hitCollider.bounds.extents.z));
        }

        private void BuildBoundaryCellCache()
        {
            blockedBoundaryCells.Clear();
            approachBoundaryCells.Clear();

            BossArenaPresentationConfig presentation = arenaConfig.GetPresentationConfig();
            bool usesPresentedEntrance = presentation.enabled;

            int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
            GetBoundaryGridBounds(out int minX, out int minY, out int maxX, out int maxY);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    bool isBoundary = x < minX + thickness
                        || x > maxX - thickness
                        || y < minY + thickness
                        || y > maxY - thickness;

                    if (!isBoundary || !biome.IsValidPosition(x, y))
                    {
                        continue;
                    }

                    blockedBoundaryCells.Add(new Vector2Int(x, y));
                    if (usesPresentedEntrance
                        && !IsEntranceOpeningCell(x, y, minX, minY, maxX, maxY, thickness))
                    {
                        approachBoundaryCells.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        private void ActivateApproachBoundary()
        {
            if (approachBoundaryActive || biome == null || approachBoundaryCells.Count == 0)
            {
                return;
            }

            biome.AddRuntimeBlockedCells(approachBoundaryCells);
            approachBoundaryActive = true;
        }

        private void DeactivateApproachBoundary()
        {
            if (!approachBoundaryActive || biome == null)
            {
                return;
            }

            biome.RemoveRuntimeBlockedCells(approachBoundaryCells);
            approachBoundaryActive = false;
        }

        private bool IsEntranceOpeningCell(
            int x,
            int y,
            int minX,
            int minY,
            int maxX,
            int maxY,
            int thickness)
        {
            BossArenaPresentationConfig presentation = arenaConfig.GetPresentationConfig();

            bool isOnEntranceSide = presentation.entranceSide switch
            {
                BossArenaEntranceSide.North => y > maxY - thickness,
                BossArenaEntranceSide.West => x < minX + thickness,
                BossArenaEntranceSide.East => x > maxX - thickness,
                _ => y < minY + thickness
            };
            if (!isOnEntranceSide)
            {
                return false;
            }

            GetEntranceOpeningRange(
                minX,
                minY,
                maxX,
                maxY,
                thickness,
                out int openingStart,
                out int openingEnd,
                out bool horizontalEntrance);
            int coordinate = horizontalEntrance ? x : y;
            return coordinate >= openingStart && coordinate <= openingEnd;
        }

        private void GetEntranceOpeningRange(
            int minX,
            int minY,
            int maxX,
            int maxY,
            int thickness,
            out int openingStart,
            out int openingEnd,
            out bool horizontalEntrance)
        {
            BossArenaPresentationConfig presentation = arenaConfig.GetPresentationConfig();
            horizontalEntrance = presentation.entranceSide == BossArenaEntranceSide.South
                || presentation.entranceSide == BossArenaEntranceSide.North;
            int sideMin = horizontalEntrance ? minX : minY;
            int sideMax = horizontalEntrance ? maxX : maxY;
            int availableWidth = Mathf.Max(2, sideMax - sideMin + 1 - thickness * 2);
            int openingWidth = Mathf.Clamp(presentation.entranceWidthInCells, 2, availableWidth);
            openingStart = sideMin + (sideMax - sideMin + 1 - openingWidth) / 2;
            openingEnd = openingStart + openingWidth - 1;
        }

        private void GetBoundaryGridBounds(
            out int minX,
            out int minY,
            out int maxX,
            out int maxY)
        {
            int lockInset = GetLockBoundaryInsetCells();
            minX = centerGrid.x - arenaSize.x / 2 + lockInset;
            minY = centerGrid.y - arenaSize.y / 2 + lockInset;
            maxX = centerGrid.x - arenaSize.x / 2 + arenaSize.x - 1 - lockInset;
            maxY = centerGrid.y - arenaSize.y / 2 + arenaSize.y - 1 - lockInset;
        }

        private ArenaWorldBounds GetArenaWorldBounds(Vector2 playerHalfExtents)
        {
            GetBoundaryGridBounds(out int minX, out int minY, out int maxX, out int maxY);
            float tileSize = Mathf.Max(0.01f, biome.TileSize);
            int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
            Vector3 minimumCellCenter = biome.GridToWorld(minX, minY);
            Vector3 maximumCellCenter = biome.GridToWorld(maxX, maxY);

            float outerMinX = minimumCellCenter.x - tileSize * 0.5f;
            float outerMaxX = maximumCellCenter.x + tileSize * 0.5f;
            float outerMinZ = minimumCellCenter.z - tileSize * 0.5f;
            float outerMaxZ = maximumCellCenter.z + tileSize * 0.5f;
            float playableMinX = outerMinX + thickness * tileSize + Mathf.Max(0f, playerHalfExtents.x);
            float playableMaxX = outerMaxX - thickness * tileSize - Mathf.Max(0f, playerHalfExtents.x);
            float playableMinZ = outerMinZ + thickness * tileSize + Mathf.Max(0f, playerHalfExtents.y);
            float playableMaxZ = outerMaxZ - thickness * tileSize - Mathf.Max(0f, playerHalfExtents.y);

            if (playableMinX > playableMaxX)
            {
                float midpoint = (outerMinX + outerMaxX) * 0.5f;
                playableMinX = midpoint;
                playableMaxX = midpoint;
            }

            if (playableMinZ > playableMaxZ)
            {
                float midpoint = (outerMinZ + outerMaxZ) * 0.5f;
                playableMinZ = midpoint;
                playableMaxZ = midpoint;
            }

            return new ArenaWorldBounds(
                outerMinX,
                outerMaxX,
                outerMinZ,
                outerMaxZ,
                playableMinX,
                playableMaxX,
                playableMinZ,
                playableMaxZ);
        }

        private int GetLockBoundaryInsetCells()
        {
            int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
            int configuredInset = Mathf.Max(0, arenaConfig.lockBoundaryInsetInCells);
            int maxInset = Mathf.Max(0, (Mathf.Min(arenaSize.x, arenaSize.y) - thickness * 2 - 2) / 2);
            return Mathf.Min(configuredInset, maxInset);
        }

        private Vector3 ResolveArenaCenterWorld(float heightOffset = 0f)
        {
            Vector3 center = biome.GridToWorldWithHeight(centerGrid.x, centerGrid.y, heightOffset);
            float halfCell = biome.TileSize * 0.5f;
            if (arenaSize.x % 2 == 0)
            {
                center.x -= halfCell;
            }

            if (arenaSize.y % 2 == 0)
            {
                center.z -= halfCell;
            }

            return center;
        }

        private Vector2Int ResolveCenterGrid()
        {
            if (arenaConfig.useCustomCenter)
            {
                return arenaConfig.centerGrid;
            }

            return new Vector2Int(biome.MapWidth / 2, biome.MapHeight / 2);
        }

        private EnemySpawnRuleConfig ResolveBossRule(IList<EnemySpawnRuleConfig> availableEnemyRules)
        {
            if (arenaConfig == null)
            {
                return null;
            }

            MidBossDefinition bossDefinition = arenaConfig.boss;
            if (bossDefinition != null && bossDefinition.useCustomBossRule && bossDefinition.bossRule != null)
            {
                EnemySpawnRuleConfig customBossRule = BuildBossRule(bossDefinition.bossRule, bossDefinition);
                if (HasRenderableSprite(customBossRule))
                {
                    return customBossRule;
                }

                Debug.LogWarning("[MidBossArena] 커스텀 보스 룰에 스프라이트가 없어 적 fallback 룰을 사용합니다.");
            }

            if (bossDefinition != null
                && bossDefinition.useEnemyRuleFallback
                && availableEnemyRules != null
                && availableEnemyRules.Count > 0)
            {
                int index = Mathf.Clamp(bossDefinition.fallbackEnemyRuleIndex, 0, availableEnemyRules.Count - 1);
                EnemySpawnRuleConfig fallbackRule = availableEnemyRules[index];
                if (fallbackRule != null)
                {
                    return BuildBossRule(fallbackRule, bossDefinition);
                }
            }

            MidBossPatternType patternType = ResolveBossPatternType();
            if (patternType != MidBossPatternType.None)
            {
                return BuildRuntimeBossRule(bossDefinition, patternType);
            }

            return null;
        }

        private EnemySpawnRuleConfig BuildRuntimeBossRule(MidBossDefinition bossDefinition, MidBossPatternType patternType)
        {
            EnemySpawnRuleConfig source = bossDefinition?.bossRule ?? new EnemySpawnRuleConfig
            {
                name = GetDefaultRuntimeBossName(patternType)
            };

            EnemySpawnRuleConfig boss = BuildBossRule(source, bossDefinition);
            if (boss == null)
            {
                return null;
            }

            EnsureRuntimeBossSprites(boss, GetRuntimeBossSprite());
            return boss;
        }

        private static string GetDefaultRuntimeBossName(MidBossPatternType patternType)
        {
            return patternType switch
            {
                MidBossPatternType.Liver => "LiverBoss",
                MidBossPatternType.Stomach => "StomachBoss",
                MidBossPatternType.Lung => "LungBoss",
                MidBossPatternType.Intestine => "IntestineBoss",
                _ => "MidBoss"
            };
        }

        private static void EnsureRuntimeBossSprites(EnemySpawnRuleConfig boss, Sprite sprite)
        {
            if (boss == null || sprite == null)
            {
                return;
            }

            if (boss.idleSprites == null || boss.idleSprites.Length == 0)
            {
                boss.idleSprites = new[] { sprite };
            }

            if (boss.moveSprites == null || boss.moveSprites.Length == 0)
            {
                boss.moveSprites = boss.idleSprites;
            }

            if (boss.attackSprites == null || boss.attackSprites.Length == 0)
            {
                boss.attackSprites = boss.idleSprites;
            }

            if (boss.deathSprites == null || boss.deathSprites.Length == 0)
            {
                boss.deathSprites = boss.idleSprites;
            }
        }

        private EnemySpawnRuleConfig BuildBossRule(EnemySpawnRuleConfig source, MidBossDefinition bossDefinition)
        {
            if (source == null)
            {
                return null;
            }

            EnemySpawnRuleConfig boss = new EnemySpawnRuleConfig
            {
                name = string.IsNullOrWhiteSpace(bossDefinition?.displayName) ? source.name : bossDefinition.displayName,
                density = source.density,
                minDistance = source.minDistance,
                poissonSalt = source.poissonSalt,
                allowedRegions = source.allowedRegions != null ? new List<int>(source.allowedRegions) : new List<int>(),
                maxAlive = 1,
                activationRadius = 0f,
                respawnCooldown = 0f,
                spawnRadius = 0f,
                moveSpeed = source.moveSpeed,
                stoppingDistance = source.stoppingDistance,
                wanderRadius = source.wanderRadius,
                chaseRadius = source.chaseRadius,
                leashRadius = source.leashRadius,
                idleDelayRange = source.idleDelayRange,
                maxHealth = source.maxHealth,
                attackDamage = source.attackDamage,
                attackRange = source.attackRange,
                attackCooldown = source.attackCooldown,
                expReward = source.expReward,
                additionalBaseStats = source.additionalBaseStats != null ? new List<CharacterStatValue>(source.additionalBaseStats) : new List<CharacterStatValue>(),
                separationDistance = 0f,
                separationStrength = 0f,
                heightOffset = source.heightOffset,
                scale = source.scale,
                sortingOrder = source.sortingOrder,
                useBillboard = source.useBillboard,
                useYSort = source.useYSort,
                animationSpeed = source.animationSpeed,
                addCollider = source.addCollider,
                isTrigger = true,
                colliderSize = source.colliderSize,
                colliderCenter = source.colliderCenter,
                idleSprites = source.idleSprites,
                idleSpritesUp = source.idleSpritesUp,
                idleSpritesDown = source.idleSpritesDown,
                moveSprites = source.moveSprites,
                moveSpritesUp = source.moveSpritesUp,
                moveSpritesDown = source.moveSpritesDown,
                attackSprites = source.attackSprites,
                attackSpritesUp = source.attackSpritesUp,
                attackSpritesDown = source.attackSpritesDown,
                attackAnimationSpeed = source.attackAnimationSpeed,
                isRanged = source.isRanged,
                projectileSpeed = source.projectileSpeed,
                projectileLifeTime = source.projectileLifeTime,
                projectileSprite = source.projectileSprite,
                projectileScale = source.projectileScale,
                projectileSpawnOffset = source.projectileSpawnOffset,
                expandColliderOnAttack = source.expandColliderOnAttack,
                attackColliderSize = source.attackColliderSize,
                attackColliderCenter = source.attackColliderCenter,
                deathSprites = source.deathSprites,
                deathAnimationSpeed = source.deathAnimationSpeed,
                isElite = source.isElite,
                tintColor = source.tintColor,
                killTriggerEnemyName = source.killTriggerEnemyName,
                killTriggerCount = source.killTriggerCount,
                splitsOnDeath = source.splitsOnDeath,
                splitCount = source.splitCount,
                splitEnemyName = source.splitEnemyName,
                splitVfxSprites = source.splitVfxSprites,
                splitVfxScale = source.splitVfxScale,
                splitVfxSpeed = source.splitVfxSpeed,
                splitVfxDuration = source.splitVfxDuration,
                chargesAtPlayer = source.chargesAtPlayer,
                chargeSpeed = source.chargeSpeed,
                chargeAccelTime = source.chargeAccelTime,
                leavesDebrisOnDeath = source.leavesDebrisOnDeath,
                debrisDuration = source.debrisDuration,
                debrisAggroRadius = source.debrisAggroRadius,
                debrisVfxSprites = source.debrisVfxSprites,
                debrisVfxScale = source.debrisVfxScale,
                debrisVfxSpeed = source.debrisVfxSpeed
            };

            if (bossDefinition != null && bossDefinition.overrideStats)
            {
                boss.maxHealth *= Mathf.Max(0.01f, bossDefinition.maxHealthMultiplier);
                boss.attackDamage *= Mathf.Max(0.01f, bossDefinition.attackDamageMultiplier);
                boss.moveSpeed *= Mathf.Max(0.01f, bossDefinition.moveSpeedMultiplier);
            }

            if (bossDefinition != null)
            {
                Vector3 scaleMultiplier = GetSafeScaleMultiplier(bossDefinition.scaleMultiplier);
                boss.scale = Vector3.Scale(boss.scale, scaleMultiplier);
                ApplyBossScaleToCollision(boss, scaleMultiplier);
            }

            boss.maxHealth = Mathf.Max(boss.maxHealth, GetConfiguredMinimumBossMaxHealth(bossDefinition));

            return boss;
        }

        private static void ApplyBossScaleToCollision(EnemySpawnRuleConfig boss, Vector3 scaleMultiplier)
        {
            if (boss == null)
            {
                return;
            }

            boss.colliderSize = ScaleVector(boss.colliderSize, scaleMultiplier);
            boss.colliderCenter = ScaleVector(boss.colliderCenter, scaleMultiplier);
        }

        private static Vector3 ScaleVector(Vector3 value, Vector3 scale)
        {
            return new Vector3(value.x * scale.x, value.y * scale.y, value.z * scale.z);
        }

        private static Vector3 GetSafeScaleMultiplier(Vector3 scaleMultiplier)
        {
            return new Vector3(
                Mathf.Approximately(scaleMultiplier.x, 0f) ? 1f : Mathf.Max(0.01f, scaleMultiplier.x),
                Mathf.Approximately(scaleMultiplier.y, 0f) ? 1f : Mathf.Max(0.01f, scaleMultiplier.y),
                Mathf.Approximately(scaleMultiplier.z, 0f) ? 1f : Mathf.Max(0.01f, scaleMultiplier.z));
        }

        private static Vector3 GetSafeColliderSize(Vector3 size)
        {
            return new Vector3(
                size.x > 0.0001f ? size.x : 2f,
                size.y > 0.0001f ? size.y : 2f,
                size.z > 0.0001f ? size.z : 2f);
        }

        private static bool HasRenderableSprite(EnemySpawnRuleConfig rule)
        {
            return rule != null
                && ((rule.idleSprites != null && rule.idleSprites.Length > 0)
                    || (rule.idleSpritesUp != null && rule.idleSpritesUp.Length > 0)
                    || (rule.idleSpritesDown != null && rule.idleSpritesDown.Length > 0)
                    || (rule.moveSprites != null && rule.moveSprites.Length > 0)
                    || (rule.moveSpritesUp != null && rule.moveSpritesUp.Length > 0)
                    || (rule.moveSpritesDown != null && rule.moveSpritesDown.Length > 0)
                    || (rule.attackSprites != null && rule.attackSprites.Length > 0)
                    || (rule.deathSprites != null && rule.deathSprites.Length > 0));
        }

        private static float GetConfiguredMinimumBossMaxHealth(MidBossDefinition bossDefinition)
        {
            if (bossDefinition != null && bossDefinition.minimumMaxHealth > 0f)
            {
                return bossDefinition.minimumMaxHealth;
            }

            return 0f;
        }

        private static Sprite GetRuntimePortalSprite()
        {
            if (runtimePortalSprite != null)
            {
                return runtimePortalSprite;
            }

            const int size = 48;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outerRadius = size * 0.44f;
            float innerRadius = size * 0.27f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    bool isRing = distance <= outerRadius && distance >= innerRadius;
                    texture.SetPixel(x, y, isRing ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            runtimePortalSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size);
            runtimePortalSprite.name = "RuntimeBossReturnPortalSprite";
            return runtimePortalSprite;
        }

        private static Sprite GetRuntimeBossSprite()
        {
            if (runtimeBossSprite != null)
            {
                return runtimeBossSprite;
            }

            const int width = 48;
            const int height = 40;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
            float rx = width * 0.4f;
            float ry = height * 0.36f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x - center.x) / rx;
                    float ny = (y - center.y) / ry;
                    float value = nx * nx + ny * ny;
                    texture.SetPixel(x, y, value <= 1f ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            runtimeBossSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), width);
            runtimeBossSprite.name = "RuntimeMidBossSprite";
            return runtimeBossSprite;
        }

        private readonly struct ArenaWorldBounds
        {
            public readonly float outerMinX;
            public readonly float outerMaxX;
            public readonly float outerMinZ;
            public readonly float outerMaxZ;
            public readonly float playableMinX;
            public readonly float playableMaxX;
            public readonly float playableMinZ;
            public readonly float playableMaxZ;

            public ArenaWorldBounds(
                float outerMinX,
                float outerMaxX,
                float outerMinZ,
                float outerMaxZ,
                float playableMinX,
                float playableMaxX,
                float playableMinZ,
                float playableMaxZ)
            {
                this.outerMinX = outerMinX;
                this.outerMaxX = outerMaxX;
                this.outerMinZ = outerMinZ;
                this.outerMaxZ = outerMaxZ;
                this.playableMinX = playableMinX;
                this.playableMaxX = playableMaxX;
                this.playableMinZ = playableMinZ;
                this.playableMaxZ = playableMaxZ;
            }

            public bool ContainsOuter(Vector3 position)
            {
                return position.x >= outerMinX
                    && position.x <= outerMaxX
                    && position.z >= outerMinZ
                    && position.z <= outerMaxZ;
            }

            public bool ContainsPlayableCenter(Vector3 position)
            {
                return position.x >= playableMinX
                    && position.x <= playableMaxX
                    && position.z >= playableMinZ
                    && position.z <= playableMaxZ;
            }
        }
    }
}
