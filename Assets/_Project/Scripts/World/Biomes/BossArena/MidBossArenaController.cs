using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 맵 중앙 중간보스 구역을 관리한다.
    /// 안개 벽 시각효과, 진입 후 봉쇄, 보스 처치 후 해제를 담당한다.
    /// </summary>
    [DisallowMultipleComponent]
    public class MidBossArenaController : MonoBehaviour
    {
        private static readonly int FogTilingId = Shader.PropertyToID("_FogTiling");
        private static readonly int PrimarySpeedId = Shader.PropertyToID("_PrimarySpeed");
        private static readonly int SecondarySpeedId = Shader.PropertyToID("_SecondarySpeed");
        private static readonly int SecondaryColorId = Shader.PropertyToID("_SecondaryColor");
        private static readonly int DistortionStrengthId = Shader.PropertyToID("_DistortionStrength");
        private static readonly int EdgeSoftnessId = Shader.PropertyToID("_EdgeSoftness");
        private static readonly int DensityId = Shader.PropertyToID("_Density");
        private static readonly int BaseOpacityId = Shader.PropertyToID("_BaseOpacity");
        private static readonly int InteriorModeId = Shader.PropertyToID("_InteriorMode");
        private static readonly int CoreDarknessId = Shader.PropertyToID("_CoreDarkness");
        private static readonly int WispBrightnessId = Shader.PropertyToID("_WispBrightness");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");
        private static readonly int SealAmountId = Shader.PropertyToID("_SealAmount");
        private static readonly int RevealAmountId = Shader.PropertyToID("_RevealAmount");
        private static readonly int FlowBoostId = Shader.PropertyToID("_FlowBoost");
        private static readonly int PixelDensityId = Shader.PropertyToID("_PixelDensity");
        private static readonly int AnimationFpsId = Shader.PropertyToID("_AnimationFps");
        private static readonly int AspectRatioId = Shader.PropertyToID("_AspectRatio");
        private static readonly int ApproachAmountId = Shader.PropertyToID("_ApproachAmount");
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int TintId = Shader.PropertyToID("_Tint");
        private static readonly int SideSealId = Shader.PropertyToID("_SideSeal");
        private static readonly int SideApproachId = Shader.PropertyToID("_SideApproach");
        private static readonly int UseSideStateId = Shader.PropertyToID("_UseSideState");
        private static readonly int OrganProfileId = Shader.PropertyToID("_OrganProfile");
        private static readonly int MotionIntensityId = Shader.PropertyToID("_MotionIntensity");
        private static readonly int GroundContactId = Shader.PropertyToID("_GroundContact");

        private static Sprite fogSprite;
        private static Sprite runtimeBossSprite;
        private static Material runtimeFogMaterial;
        private static readonly List<MidBossArenaController> ActiveArenas = new List<MidBossArenaController>();

        private readonly List<Renderer> fogRenderers = new List<Renderer>();
        private readonly List<float> fogRendererAlphaScales = new List<float>();
        private readonly List<int> fogRendererSideIndices = new List<int>();
        private readonly List<Vector2Int> blockedBoundaryCells = new List<Vector2Int>();
        private readonly HashSet<Renderer> concealedBossRenderers = new HashSet<Renderer>();
        private SpriteRenderer interiorFogRenderer;
        private MaterialPropertyBlock fogPropertyBlock;

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
        private bool bossIntroPending;
        private float bossIntroPreludeElapsed;
        private float fogRevealAmount;
        private float borderFogAmount;
        private int entryFogWallIndex = -1;

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

            transform.position = biome.GridToWorldWithHeight(centerGrid.x, centerGrid.y);
            transform.name = "MidBossArena";

            BuildBoundaryCellCache();
            BuildTrigger();
            BuildFogWalls();

            SpawnBoss();

            ApplyFogVisualState();
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
            bossIntroPending = false;
        }

        private void Update()
        {
            float presentationDeltaTime = Time.unscaledDeltaTime;
            UpdateFogReveal(presentationDeltaTime);
            UpdateBossIntroPrelude(presentationDeltaTime);

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

            TryActivateArena(player);
        }

        private void OnDestroy()
        {
            ActiveArenas.Remove(this);
            BossIntroPresentation.Cancel(this);
            bossIntroPending = false;

            if (activeBoss != null)
                activeBoss.Defeated -= HandleBossDefeated;

            if (arenaLocked && biome != null)
            {
                biome.RemoveRuntimeBlockedCells(blockedBoundaryCells);
            }

            for (int i = 0; i < fogRenderers.Count; i++)
            {
                MeshFilter meshFilter = fogRenderers[i] != null
                    ? fogRenderers[i].GetComponent<MeshFilter>()
                    : null;
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    Destroy(meshFilter.sharedMesh);
                }
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
            entryFogWallIndex = ResolveEntryFogWallIndex(player != null ? player.transform.position : transform.position);
            bossIntroPreludeElapsed = 0f;
            biome.AddRuntimeBlockedCells(blockedBoundaryCells);
            ApplyFogVisualState();
            RecenterBossEncounter();
            SetBossEncounterActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameState(GameState.InBossRoom);
            }

            float preludeDuration = Mathf.Max(0f, arenaConfig.bossIntroFogPreludeDuration);
            bossIntroPending = preludeDuration > 0.01f;
            if (bossIntroPending)
            {
                DontStarveCamera.Instance?.AddCombatImpulse(
                    Mathf.Max(0f, arenaConfig.fogSealCameraImpulse),
                    Mathf.Min(0.35f, preludeDuration));
            }
            else
            {
                StartBossIntroOrEncounter();
            }

            Debug.Log($"[MidBossArena] 중간보스 구역 진입 - 탈출 차단 활성화 ({biome.BiomeType})");
        }

        private void UpdateBossIntroPrelude(float deltaTime)
        {
            if (!bossIntroPending)
            {
                return;
            }

            if (!arenaLocked || bossDefeated || !isActiveAndEnabled)
            {
                bossIntroPending = false;
                return;
            }

            bossIntroPreludeElapsed += Mathf.Max(0f, deltaTime);
            if (bossIntroPreludeElapsed < Mathf.Max(0f, arenaConfig.bossIntroFogPreludeDuration))
            {
                return;
            }

            bossIntroPending = false;
            StartBossIntroOrEncounter();
        }

        private void StartBossIntroOrEncounter()
        {
            bossIntroPlaying = TryPlayBossIntro();
            if (!bossIntroPlaying)
            {
                BeginBossEncounter();
            }
        }

        private int ResolveEntryFogWallIndex(Vector3 playerPosition)
        {
            if (biome == null)
            {
                return -1;
            }

            Vector3 arenaCenter = biome.GridToWorld(centerGrid.x, centerGrid.y);
            Vector3 offset = playerPosition - arenaCenter;
            float halfWidth = Mathf.Max(0.01f, arenaSize.x * biome.TileSize * 0.5f);
            float halfDepth = Mathf.Max(0.01f, arenaSize.y * biome.TileSize * 0.5f);
            float normalizedX = Mathf.Abs(offset.x) / halfWidth;
            float normalizedZ = Mathf.Abs(offset.z) / halfDepth;
            if (normalizedZ >= normalizedX)
            {
                return offset.z >= 0f ? 0 : 1;
            }

            return offset.x >= 0f ? 2 : 3;
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

            Vector3 bossSpawnPosition = biome.GridToWorldWithHeight(centerGrid.x, centerGrid.y, bossRule.heightOffset);
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

            Vector3 center = biome.GridToWorldWithHeight(centerGrid.x, centerGrid.y, bossRule.heightOffset);
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
            bossIntroPending = false;
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

            ApplyFogVisualState();

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
            return biome.GridToWorldWithHeight(centerGrid.x, centerGrid.y, heightOffset);
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
                : hasArenaSprite ? arenaConfig.returnPortalSprite : GetFogSprite(true);
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

            int triggerInset = GetTriggerInsetCells();
            float innerWidth = Mathf.Max(biome.TileSize, (arenaSize.x - triggerInset * 2) * biome.TileSize);
            float innerDepth = Mathf.Max(biome.TileSize, (arenaSize.y - triggerInset * 2) * biome.TileSize);

            trigger.isTrigger = true;
            trigger.size = new Vector3(innerWidth, arenaConfig.triggerHeight, innerDepth);
            trigger.center = new Vector3(0f, arenaConfig.wallHeightOffset, 0f);
        }

        private void BuildFogWalls()
        {
            fogRenderers.Clear();
            fogRendererAlphaScales.Clear();
            fogRendererSideIndices.Clear();
            interiorFogRenderer = null;

            Vector3 worldCenter = biome.GridToWorld(centerGrid.x, centerGrid.y);
            float widthWorld = arenaSize.x * biome.TileSize;
            float depthWorld = arenaSize.y * biome.TileSize;
            float thicknessWorld = Mathf.Max(1, arenaConfig.wallThicknessInCells) * biome.TileSize;
            float halfWidth = widthWorld * 0.5f;
            float halfDepth = depthWorld * 0.5f;
            float wallOffsetX = halfWidth - thicknessWorld * 0.5f;
            float wallOffsetZ = halfDepth - thicknessWorld * 0.5f;

            if (arenaConfig.useInteriorFogCover)
            {
                CreateInteriorFogCover(worldCenter, new Vector2(widthWorld, depthWorld));
            }

            float baseHeight = biome.GetGroundHeight(worldCenter) + arenaConfig.groundFogOffset;
            CreatePerimeterFogCards(worldCenter, wallOffsetX, wallOffsetZ, baseHeight);
        }

        private void CreatePerimeterFogCards(
            Vector3 worldCenter,
            float halfWidth,
            float halfDepth,
            float baseHeight)
        {
            Vector3[] sideStarts =
            {
                worldCenter + new Vector3(-halfWidth, 0f, halfDepth),
                worldCenter + new Vector3(halfWidth, 0f, -halfDepth),
                worldCenter + new Vector3(halfWidth, 0f, halfDepth),
                worldCenter + new Vector3(-halfWidth, 0f, -halfDepth)
            };
            Vector3[] sideEnds =
            {
                worldCenter + new Vector3(halfWidth, 0f, halfDepth),
                worldCenter + new Vector3(-halfWidth, 0f, -halfDepth),
                worldCenter + new Vector3(halfWidth, 0f, -halfDepth),
                worldCenter + new Vector3(-halfWidth, 0f, halfDepth)
            };
            Vector3[] outwardNormals =
            {
                Vector3.forward,
                Vector3.back,
                Vector3.right,
                Vector3.left
            };

            for (int side = 0; side < 4; side++)
            {
                CreateFogCardsForSide(
                    sideStarts[side],
                    sideEnds[side],
                    outwardNormals[side],
                    side,
                    baseHeight + 1.06f,
                    4.8f,
                    new Vector2(3.2f, 2.7f),
                    0f,
                    1f,
                    0.82f,
                    arenaConfig.sortingOrder + 2,
                    10.7f + side * 17.3f);
                CreateFogCardsForSide(
                    sideStarts[side],
                    sideEnds[side],
                    outwardNormals[side],
                    side,
                    baseHeight + 0.82f,
                    6.2f,
                    new Vector2(2.35f, 1.9f),
                    -0.48f,
                    arenaConfig.fogRearLayerOpacity,
                    0.52f,
                    arenaConfig.sortingOrder + 1,
                    31.9f + side * 13.1f);
                CreateFogCardsForSide(
                    sideStarts[side],
                    sideEnds[side],
                    outwardNormals[side],
                    side,
                    baseHeight + 0.34f,
                    5.5f,
                    new Vector2(2.85f * arenaConfig.fogGroundSpread, 1.1f),
                    0.3f,
                    arenaConfig.fogGroundContactOpacity,
                    0.34f,
                    arenaConfig.sortingOrder,
                    53.3f + side * 11.7f);
            }

            Vector3[] corners =
            {
                worldCenter + new Vector3(halfWidth, baseHeight + 1.02f, halfDepth),
                worldCenter + new Vector3(-halfWidth, baseHeight + 1.02f, halfDepth),
                worldCenter + new Vector3(halfWidth, baseHeight + 1.02f, -halfDepth),
                worldCenter + new Vector3(-halfWidth, baseHeight + 1.02f, -halfDepth)
            };
            int[] cornerSides = { 0, 0, 1, 1 };
            for (int i = 0; i < corners.Length; i++)
            {
                CreateFogCard(
                    $"CornerFogCard_{i}",
                    corners[i],
                    new Vector2(3.65f, 3f),
                    cornerSides[i],
                    1f,
                    0.76f,
                    arenaConfig.sortingOrder + 3,
                    79.1f + i * 7.9f);
            }
        }

        private void CreateFogCardsForSide(
            Vector3 start,
            Vector3 end,
            Vector3 outwardNormal,
            int sideIndex,
            float height,
            float spacing,
            Vector2 baseSize,
            float radialOffset,
            float alphaScale,
            float speedMultiplier,
            int sortingOrder,
            float seedBase)
        {
            float length = Vector3.Distance(start, end);
            int count = Mathf.Max(1, Mathf.CeilToInt(length / Mathf.Max(0.5f, spacing)));
            for (int i = 0; i < count; i++)
            {
                float t = (i + 0.5f) / count;
                float wave = Mathf.Sin((i + 1f) * 2.17f + seedBase) * 0.16f;
                Vector3 position = Vector3.Lerp(start, end, t) + outwardNormal * (radialOffset + wave);
                position.y = height + Mathf.Sin((i + 1f) * 1.31f + seedBase) * 0.15f;
                float scaleVariation = 0.86f + Mathf.Abs(Mathf.Sin((i + 1f) * 1.73f + seedBase)) * 0.28f;
                CreateFogCard(
                    $"FogCard_{sideIndex}_{sortingOrder}_{i}",
                    position,
                    baseSize * scaleVariation,
                    sideIndex,
                    alphaScale,
                    speedMultiplier,
                    sortingOrder,
                    seedBase + i * 1.91f);
            }
        }

        private void CreateFogCard(
            string objectName,
            Vector3 position,
            Vector2 size,
            int sideIndex,
            float alphaScale,
            float speedMultiplier,
            int sortingOrder,
            float seed)
        {
            GameObject card = new GameObject(objectName);
            card.transform.SetParent(transform, false);
            card.transform.position = position;
            Billboard billboard = card.AddComponent<Billboard>();
            billboard.Configure(Billboard.BillboardMode.FaceCamera, 0f, Billboard.UpdateMode.Once);

            SpriteRenderer renderer = card.AddComponent<SpriteRenderer>();
            renderer.sprite = GetFogSprite(false);
            renderer.sortingOrder = sortingOrder;
            ApplyFogWorldSize(card.transform, renderer.sprite, size);
            ConfigureFogMaterial(
                renderer,
                size,
                seed,
                arenaConfig.fogDensity,
                arenaConfig.fogEdgeSoftness,
                0f,
                speedMultiplier,
                false,
                sortingOrder == arenaConfig.sortingOrder,
                1f);
            fogRenderers.Add(renderer);
            fogRendererAlphaScales.Add(Mathf.Clamp01(alphaScale));
            fogRendererSideIndices.Add(sideIndex);
        }

        private void CreateContinuousFogRibbon(
            string name,
            Vector3 worldCenter,
            float halfWidth,
            float halfDepth,
            float thickness,
            float height,
            float radialOffset,
            float verticalOffset,
            float alphaScale,
            float speedMultiplier,
            float tilingMultiplier,
            float perimeterLength,
            bool groundContact)
        {
            GameObject ribbon = new GameObject(name);
            ribbon.transform.SetParent(transform, false);
            ribbon.transform.position = new Vector3(worldCenter.x, height + verticalOffset, worldCenter.z);

            MeshFilter filter = ribbon.AddComponent<MeshFilter>();
            filter.sharedMesh = BuildContinuousFogRibbonMesh(
                name,
                halfWidth,
                halfDepth,
                thickness,
                radialOffset,
                groundContact);
            MeshRenderer renderer = ribbon.AddComponent<MeshRenderer>();
            renderer.sortingOrder = arenaConfig.sortingOrder + (groundContact ? 0 : radialOffset < 0f ? 1 : 2);
            ConfigureFogMaterial(
                renderer,
                new Vector2(perimeterLength, thickness),
                9.41f + radialOffset * 13.7f,
                Mathf.Min(2f, arenaConfig.fogDensity * (groundContact ? 0.74f : radialOffset < 0f ? 0.88f : 1.14f)),
                arenaConfig.fogEdgeSoftness,
                0f,
                speedMultiplier,
                false,
                groundContact,
                tilingMultiplier);
            fogRenderers.Add(renderer);
            fogRendererAlphaScales.Add(Mathf.Clamp01(alphaScale));
        }

        private static Mesh BuildContinuousFogRibbonMesh(
            string name,
            float halfWidth,
            float halfDepth,
            float thickness,
            float radialOffset,
            bool groundContact)
        {
            const int cornerSegments = 10;
            const int rowCount = 3;
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();
            List<Vector2> path = new List<Vector2>();
            List<Vector4> sideWeights = new List<Vector4>();

            float radius = Mathf.Clamp(Mathf.Min(halfWidth, halfDepth) * 0.045f, 0.45f, 0.85f);
            if (groundContact)
            {
                radius = Mathf.Max(radius, thickness * 0.55f);
            }
            float centerX = Mathf.Max(radius, halfWidth - radius + radialOffset);
            float centerZ = Mathf.Max(radius, halfDepth - radius + radialOffset);
            AddRoundedCorner(path, sideWeights, new Vector2(centerX, centerZ), radius, 0f, 90f, 2, 0, cornerSegments);
            AddRoundedCorner(path, sideWeights, new Vector2(-centerX, centerZ), radius, 90f, 180f, 0, 3, cornerSegments);
            AddRoundedCorner(path, sideWeights, new Vector2(-centerX, -centerZ), radius, 180f, 270f, 3, 1, cornerSegments);
            AddRoundedCorner(path, sideWeights, new Vector2(centerX, -centerZ), radius, 270f, 360f, 1, 2, cornerSegments);

            float[] distance = new float[path.Count + 1];
            for (int i = 1; i <= path.Count; i++)
            {
                distance[i] = distance[i - 1] + Vector2.Distance(path[i - 1], path[i % path.Count]);
            }
            float totalDistance = Mathf.Max(0.01f, distance[path.Count]);

            for (int i = 0; i <= path.Count; i++)
            {
                int pathIndex = i % path.Count;
                Vector2 point = path[pathIndex];
                Vector2 previous = path[(pathIndex - 1 + path.Count) % path.Count];
                Vector2 next = path[(pathIndex + 1) % path.Count];
                Vector2 tangent = (next - previous).normalized;
                Vector2 outward = new Vector2(tangent.y, -tangent.x);
                float u = distance[i] / totalDistance;

                for (int row = 0; row < rowCount; row++)
                {
                    float across = row / (float)(rowCount - 1);
                    float acrossOffset = (across - 0.5f) * thickness;
                    Vector2 horizontal = groundContact
                        ? point + outward * acrossOffset
                        : point;
                    float y = groundContact
                        ? Mathf.Sin(across * Mathf.PI) * thickness * 0.08f
                        : (across - 0.5f) * thickness;
                    vertices.Add(new Vector3(horizontal.x, y, horizontal.y));
                    uvs.Add(new Vector2(u, across));
                }
            }

            for (int segment = 0; segment < path.Count; segment++)
            {
                for (int row = 0; row < rowCount - 1; row++)
                {
                    int current = segment * rowCount + row;
                    int next = current + rowCount;
                    triangles.Add(current);
                    triangles.Add(next);
                    triangles.Add(current + 1);
                    triangles.Add(current + 1);
                    triangles.Add(next);
                    triangles.Add(next + 1);
                }
            }

            Mesh mesh = new Mesh { name = name, hideFlags = HideFlags.HideAndDontSave };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            List<Vector4> vertexSideWeights = new List<Vector4>(vertices.Count);
            List<Color> colors = new List<Color>(vertices.Count);
            for (int i = 0; i <= path.Count; i++)
            {
                Vector4 weights = sideWeights[i % path.Count];
                for (int row = 0; row < rowCount; row++)
                {
                    vertexSideWeights.Add(weights);
                    colors.Add(Color.white);
                }
            }
            mesh.SetUVs(1, vertexSideWeights);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddRoundedCorner(
            List<Vector2> path,
            List<Vector4> sideWeights,
            Vector2 center,
            float radius,
            float startAngle,
            float endAngle,
            int startSide,
            int endSide,
            int segmentCount)
        {
            for (int segment = 0; segment < segmentCount; segment++)
            {
                float t = segment / (float)segmentCount;
                float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
                path.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
                float blend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.22f, 0.78f, t));
                sideWeights.Add(Vector4.Lerp(GetFogSideWeight(startSide), GetFogSideWeight(endSide), blend));
            }
        }

        private static Vector4 GetFogSideWeight(int sideIndex)
        {
            return sideIndex switch
            {
                0 => new Vector4(1f, 0f, 0f, 0f),
                1 => new Vector4(0f, 1f, 0f, 0f),
                2 => new Vector4(0f, 0f, 1f, 0f),
                3 => new Vector4(0f, 0f, 0f, 1f),
                _ => Vector4.zero
            };
        }

        private void CreateInteriorFogCover(Vector3 position, Vector2 size)
        {
            GameObject cover = new GameObject("InteriorFogCover");
            cover.transform.SetParent(transform, false);
            position.y = biome.GetGroundHeight(position) + arenaConfig.groundFogOffset + 0.02f;
            cover.transform.position = position;
            cover.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            interiorFogRenderer = cover.AddComponent<SpriteRenderer>();
            interiorFogRenderer.sprite = GetFogSprite(true);
            ApplyFogWorldSize(cover.transform, interiorFogRenderer.sprite, size);
            interiorFogRenderer.sortingOrder = arenaConfig.sortingOrder + arenaConfig.interiorFogSortingOrderOffset;
            ConfigureFogMaterial(
                interiorFogRenderer,
                size,
                19.73f,
                arenaConfig.interiorFogDensity,
                Mathf.Max(0.025f, arenaConfig.fogEdgeSoftness * 0.35f),
                arenaConfig.interiorFogBaseOpacity,
                0.58f,
                true);
        }

        private void ConfigureFogMaterial(
            Renderer renderer,
            Vector2 worldSize,
            float seed,
            float density,
            float edgeSoftness,
            float baseOpacity,
            float speedMultiplier,
            bool interiorMode = false,
            bool groundContact = false,
            float tilingMultiplier = 1f)
        {
            if (renderer == null)
            {
                return;
            }

            Material material = GetFogMaterial();
            if (material == null)
            {
                return;
            }

            renderer.sharedMaterial = material;
            fogPropertyBlock ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(fogPropertyBlock);
            SpriteRenderer spriteRenderer = renderer as SpriteRenderer;
            Sprite densitySprite = GetFogSprite(interiorMode);
            Texture mainTexture = spriteRenderer != null && spriteRenderer.sprite != null
                ? spriteRenderer.sprite.texture
                : densitySprite != null ? densitySprite.texture : null;
            if (mainTexture != null)
            {
                fogPropertyBlock.SetTexture(MainTexId, mainTexture);
            }
            fogPropertyBlock.SetColor(TintId, Color.white);

            float worldTileSize = Mathf.Max(0.5f, arenaConfig.fogWorldTileSize);
            float sourceAspect = densitySprite != null && densitySprite.texture != null
                ? densitySprite.texture.width / (float)Mathf.Max(1, densitySprite.texture.height)
                : 1f;
            float shapePreservingTileWidth = Mathf.Max(
                0.5f,
                worldSize.y * Mathf.Max(0.25f, sourceAspect) * 2f);
            bool useSingleSpriteShape = spriteRenderer != null && !interiorMode;
            Vector2 tiling = interiorMode
                ? new Vector2(
                    Mathf.Max(0.5f, worldSize.x / worldTileSize),
                    Mathf.Max(0.5f, worldSize.y / worldTileSize))
                : useSingleSpriteShape
                    ? Vector2.one
                : new Vector2(
                    Mathf.Max(
                        1f,
                        worldSize.x / Mathf.Min(worldTileSize, shapePreservingTileWidth)) * tilingMultiplier,
                    1f);
            fogPropertyBlock.SetVector(
                FogTilingId,
                new Vector4(tiling.x, tiling.y, 0f, 0f));
            fogPropertyBlock.SetVector(
                PrimarySpeedId,
                new Vector4(
                    arenaConfig.fogPrimaryScrollSpeed.x * speedMultiplier,
                    arenaConfig.fogPrimaryScrollSpeed.y * speedMultiplier,
                    0f,
                    0f));
            fogPropertyBlock.SetVector(
                SecondarySpeedId,
                new Vector4(
                    arenaConfig.fogSecondaryScrollSpeed.x * speedMultiplier,
                    arenaConfig.fogSecondaryScrollSpeed.y * speedMultiplier,
                    0f,
                    0f));
            fogPropertyBlock.SetColor(SecondaryColorId, arenaConfig.fogSecondaryColor);
            fogPropertyBlock.SetFloat(DistortionStrengthId, arenaConfig.fogDistortionStrength);
            fogPropertyBlock.SetFloat(EdgeSoftnessId, Mathf.Clamp(edgeSoftness, 0.001f, 0.45f));
            fogPropertyBlock.SetFloat(DensityId, Mathf.Clamp(density, 0f, 2f));
            fogPropertyBlock.SetFloat(BaseOpacityId, Mathf.Clamp01(baseOpacity));
            fogPropertyBlock.SetFloat(InteriorModeId, interiorMode ? 1f : 0f);
            fogPropertyBlock.SetFloat(CoreDarknessId, Mathf.Clamp01(arenaConfig.fogCoreDarkness));
            fogPropertyBlock.SetFloat(WispBrightnessId, Mathf.Max(0f, arenaConfig.fogWispBrightness));
            fogPropertyBlock.SetFloat(SeedId, seed);
            fogPropertyBlock.SetFloat(SealAmountId, 0f);
            fogPropertyBlock.SetFloat(RevealAmountId, 0f);
            fogPropertyBlock.SetFloat(FlowBoostId, 0f);
            fogPropertyBlock.SetFloat(PixelDensityId, Mathf.Clamp(arenaConfig.fogPixelDensity, 16f, 256f));
            fogPropertyBlock.SetFloat(AnimationFpsId, Mathf.Clamp(arenaConfig.fogAnimationFps, 4f, 30f));
            fogPropertyBlock.SetFloat(ApproachAmountId, interiorMode ? 0f : 0.18f);
            fogPropertyBlock.SetFloat(UseSideStateId, interiorMode || useSingleSpriteShape ? 0f : 1f);
            fogPropertyBlock.SetFloat(OrganProfileId, (float)arenaConfig.fogMotionProfile);
            fogPropertyBlock.SetFloat(MotionIntensityId, Mathf.Clamp(arenaConfig.fogMotionIntensity, 0f, 2f));
            fogPropertyBlock.SetFloat(GroundContactId, groundContact ? 1f : 0f);
            fogPropertyBlock.SetFloat(
                AspectRatioId,
                interiorMode ? Mathf.Max(0.25f, worldSize.x / Mathf.Max(0.01f, worldSize.y)) : 1f);
            renderer.SetPropertyBlock(fogPropertyBlock);
        }

        private static void ApplyFogWorldSize(Transform target, Sprite sprite, Vector2 worldSize)
        {
            if (target == null)
            {
                return;
            }

            if (sprite == null)
            {
                target.localScale = new Vector3(worldSize.x, worldSize.y, 1f);
                return;
            }

            Vector3 spriteSize = sprite.bounds.size;
            float scaleX = spriteSize.x > 0.0001f ? worldSize.x / spriteSize.x : worldSize.x;
            float scaleY = spriteSize.y > 0.0001f ? worldSize.y / spriteSize.y : worldSize.y;
            target.localScale = new Vector3(scaleX, scaleY, 1f);
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
            Vector3 centerWorld = biome.GridToWorld(centerGrid.x, centerGrid.y);
            float halfWidth = arenaSize.x * biome.TileSize * 0.5f;
            float halfDepth = arenaSize.y * biome.TileSize * 0.5f;
            return worldPosition.x >= centerWorld.x - halfWidth
                && worldPosition.x <= centerWorld.x + halfWidth
                && worldPosition.z >= centerWorld.z - halfDepth
                && worldPosition.z <= centerWorld.z + halfDepth;
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
            if (biome == null || arenaConfig == null)
            {
                return worldPosition;
            }

            Vector3 centerWorld = biome.GridToWorld(centerGrid.x, centerGrid.y);
            float tileSize = Mathf.Max(0.01f, biome.TileSize);
            float wallPadding = (Mathf.Max(1, arenaConfig.wallThicknessInCells) + GetLockBoundaryInsetCells()) * tileSize;
            float extraMargin = Mathf.Max(0f, margin);
            float halfWidth = Mathf.Max(tileSize * 0.5f, arenaSize.x * tileSize * 0.5f - wallPadding - extraMargin);
            float halfDepth = Mathf.Max(tileSize * 0.5f, arenaSize.y * tileSize * 0.5f - wallPadding - extraMargin);

            worldPosition.x = Mathf.Clamp(worldPosition.x, centerWorld.x - halfWidth, centerWorld.x + halfWidth);
            worldPosition.z = Mathf.Clamp(worldPosition.z, centerWorld.z - halfDepth, centerWorld.z + halfDepth);
            return worldPosition;
        }

        private void EnforcePlayerInsidePlayableBounds()
        {
            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                return;
            }

            float margin = GetPlayerClampMargin(player);
            Vector3 currentPosition = player.transform.position;
            Vector3 clampedPosition = ClampToPlayableBounds(currentPosition, margin);
            Vector3 planarDelta = clampedPosition - currentPosition;
            planarDelta.y = 0f;
            if (planarDelta.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            player.SpawnAt(clampedPosition);
        }

        private static float GetPlayerClampMargin(PlayerController player)
        {
            Collider hitCollider = player != null ? player.HitCollider : null;
            if (hitCollider == null)
            {
                return 0.55f;
            }

            return Mathf.Max(0.35f, Mathf.Max(hitCollider.bounds.extents.x, hitCollider.bounds.extents.z) + 0.2f);
        }

        private void BuildBoundaryCellCache()
        {
            blockedBoundaryCells.Clear();

            int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
            int lockInset = GetLockBoundaryInsetCells();
            int minX = centerGrid.x - arenaSize.x / 2 + lockInset;
            int minY = centerGrid.y - arenaSize.y / 2 + lockInset;
            int maxX = centerGrid.x - arenaSize.x / 2 + arenaSize.x - 1 - lockInset;
            int maxY = centerGrid.y - arenaSize.y / 2 + arenaSize.y - 1 - lockInset;

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
                }
            }
        }

        private int GetLockBoundaryInsetCells()
        {
            int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
            int configuredInset = Mathf.Max(0, arenaConfig.lockBoundaryInsetInCells);
            int maxInset = Mathf.Max(0, (Mathf.Min(arenaSize.x, arenaSize.y) - thickness * 2 - 2) / 2);
            return Mathf.Min(configuredInset, maxInset);
        }

        private int GetTriggerInsetCells()
        {
            int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
            int configuredInset = Mathf.Max(0, arenaConfig.triggerInsetInCells);
            int maxInset = Mathf.Max(0, (Mathf.Min(arenaSize.x, arenaSize.y) - 1) / 2);
            return Mathf.Min(thickness + configuredInset, maxInset);
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

        private void ApplyFogVisualState()
        {
            UpdateFogVisuals();
        }

        private void UpdateFogReveal(float deltaTime)
        {
            float revealTarget = ShouldRevealInteriorFog() ? 1f : 0f;
            float revealDuration = Mathf.Max(0.01f, arenaConfig.fogRevealDuration);
            fogRevealAmount = Mathf.MoveTowards(fogRevealAmount, revealTarget, deltaTime / revealDuration);

            float borderTarget = arenaLocked && !bossDefeated ? 1f : 0f;
            float borderDuration = borderTarget > borderFogAmount
                ? Mathf.Max(0.05f, arenaConfig.fogSealDuration)
                : Mathf.Max(0.01f, arenaConfig.fogDissolveDuration);
            borderFogAmount = Mathf.MoveTowards(borderFogAmount, borderTarget, deltaTime / borderDuration);
            UpdateFogVisuals();
        }

        private bool ShouldRevealInteriorFog()
        {
            return arenaLocked || bossDefeated;
        }

        private void UpdateFogVisuals()
        {
            float easedBorderAmount = Mathf.SmoothStep(0f, 1f, borderFogAmount);
            Color borderColor = Color.Lerp(
                arenaConfig.unlockedFogColor,
                arenaConfig.lockedFogColor,
                easedBorderAmount);
            for (int i = 0; i < fogRenderers.Count; i++)
            {
                if (fogRenderers[i] != null)
                {
                    float pulse = 1f + Mathf.Sin(
                        Time.unscaledTime * Mathf.Max(0f, arenaConfig.fogPulseSpeed) + i * 0.65f)
                        * arenaConfig.fogPulseAmount;
                    Color animatedColor = borderColor;
                    float alphaScale = i < fogRendererAlphaScales.Count
                        ? fogRendererAlphaScales[i]
                        : 1f;
                    int sideIndex = i < fogRendererSideIndices.Count
                        ? fogRendererSideIndices[i]
                        : -1;
                    animatedColor.a = Mathf.Clamp01(borderColor.a * pulse * alphaScale);
                    if (bossDefeated)
                    {
                        animatedColor.a *= easedBorderAmount;
                    }

                    SetRendererTint(fogRenderers[i], animatedColor);
                    SetFogTransitionProperties(
                        fogRenderers[i],
                        sideIndex >= 0 ? GetWallSealAmount(sideIndex) : easedBorderAmount,
                        0f,
                        Mathf.Sin(easedBorderAmount * Mathf.PI) * Mathf.Max(0f, arenaConfig.fogTransitionFlowBoost),
                        sideIndex >= 0 ? GetWallApproachAmount(sideIndex) : 0.18f,
                        sideIndex < 0);
                }
            }

            if (interiorFogRenderer != null)
            {
                Color interiorColor = arenaConfig.interiorFogColor;
                float lateAlphaFade = Mathf.SmoothStep(0.82f, 1f, fogRevealAmount);
                interiorColor.a = Mathf.Lerp(
                    Mathf.Clamp01(arenaConfig.interiorFogHiddenAlpha),
                    Mathf.Clamp01(arenaConfig.interiorFogRevealedAlpha),
                    lateAlphaFade);
                interiorFogRenderer.color = interiorColor;
                SetFogTransitionProperties(
                    interiorFogRenderer,
                    borderFogAmount,
                    fogRevealAmount,
                    Mathf.Sin(fogRevealAmount * Mathf.PI) * Mathf.Max(0f, arenaConfig.fogTransitionFlowBoost),
                    0f,
                    false);
            }
        }

        private float GetWallApproachAmount(int sideIndex)
        {
            const float idleReadability = 0.18f;
            if (arenaLocked || bossDefeated || biome == null || sideIndex < 0 || sideIndex >= 4)
            {
                return idleReadability;
            }

            PlayerController player = PlayerController.Instance;
            if (player == null || ResolveEntryFogWallIndex(player.transform.position) != sideIndex)
            {
                return idleReadability;
            }

            Vector3 center = biome.GridToWorld(centerGrid.x, centerGrid.y);
            Vector3 offset = player.transform.position - center;
            float halfWidth = arenaSize.x * biome.TileSize * 0.5f;
            float halfDepth = arenaSize.y * biome.TileSize * 0.5f;
            float distanceToWall = sideIndex switch
            {
                0 => Mathf.Abs(offset.z - halfDepth),
                1 => Mathf.Abs(offset.z + halfDepth),
                2 => Mathf.Abs(offset.x - halfWidth),
                3 => Mathf.Abs(offset.x + halfWidth),
                _ => float.MaxValue
            };
            float previewDistance = Mathf.Max(0.5f, arenaConfig.fogApproachPreviewDistance);
            float proximity = 1f - Mathf.Clamp01(distanceToWall / previewDistance);
            return Mathf.Max(idleReadability, Mathf.SmoothStep(0f, 1f, proximity));
        }

        private float GetWallSealAmount(int wallIndex)
        {
            float amount = Mathf.Clamp01(borderFogAmount);
            if (!arenaLocked || entryFogWallIndex < 0 || wallIndex < 0 || wallIndex >= 4)
            {
                return Mathf.SmoothStep(0f, 1f, amount);
            }

            int distanceFromEntry;
            if (wallIndex == entryFogWallIndex)
            {
                distanceFromEntry = 0;
            }
            else if (AreOppositeFogWalls(wallIndex, entryFogWallIndex))
            {
                distanceFromEntry = 2;
            }
            else
            {
                distanceFromEntry = 1;
            }

            float delay = Mathf.Clamp01(distanceFromEntry * Mathf.Max(0f, arenaConfig.fogSealStagger));
            float staggeredAmount = Mathf.InverseLerp(delay, 1f, amount);
            return Mathf.SmoothStep(0f, 1f, staggeredAmount);
        }

        private static bool AreOppositeFogWalls(int first, int second)
        {
            return (first == 0 && second == 1)
                || (first == 1 && second == 0)
                || (first == 2 && second == 3)
                || (first == 3 && second == 2);
        }

        private void SetFogTransitionProperties(
            Renderer renderer,
            float sealAmount,
            float revealAmount,
            float flowBoost,
            float approachAmount,
            bool useSideState)
        {
            if (renderer == null || renderer.sharedMaterial == null)
            {
                return;
            }

            fogPropertyBlock ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(fogPropertyBlock);
            fogPropertyBlock.SetFloat(SealAmountId, Mathf.Clamp01(sealAmount));
            fogPropertyBlock.SetFloat(RevealAmountId, Mathf.Clamp01(revealAmount));
            fogPropertyBlock.SetFloat(FlowBoostId, Mathf.Max(0f, flowBoost));
            fogPropertyBlock.SetFloat(ApproachAmountId, Mathf.Clamp01(approachAmount));
            fogPropertyBlock.SetFloat(UseSideStateId, useSideState ? 1f : 0f);
            fogPropertyBlock.SetVector(
                SideSealId,
                new Vector4(
                    GetWallSealAmount(0),
                    GetWallSealAmount(1),
                    GetWallSealAmount(2),
                    GetWallSealAmount(3)));
            fogPropertyBlock.SetVector(
                SideApproachId,
                new Vector4(
                    GetWallApproachAmount(0),
                    GetWallApproachAmount(1),
                    GetWallApproachAmount(2),
                    GetWallApproachAmount(3)));
            renderer.SetPropertyBlock(fogPropertyBlock);
        }

        private void SetRendererTint(Renderer renderer, Color color)
        {
            if (renderer is SpriteRenderer spriteRenderer)
            {
                spriteRenderer.color = color;
                return;
            }

            fogPropertyBlock ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(fogPropertyBlock);
            fogPropertyBlock.SetColor(TintId, color);
            renderer.SetPropertyBlock(fogPropertyBlock);
        }

        private Material GetFogMaterial()
        {
            if (arenaConfig != null && arenaConfig.fogMaterial != null)
            {
                return arenaConfig.fogMaterial;
            }

            if (runtimeFogMaterial != null)
            {
                return runtimeFogMaterial;
            }

            Shader shader = Shader.Find("Necrocis/BossArenaFog");
            if (shader == null)
            {
                return null;
            }

            runtimeFogMaterial = new Material(shader)
            {
                name = "RuntimeBossArenaFog",
                hideFlags = HideFlags.HideAndDontSave
            };
            return runtimeFogMaterial;
        }

        private Sprite GetFogSprite(bool interior)
        {
            if (interior && arenaConfig != null && arenaConfig.interiorFogSprite != null)
            {
                return arenaConfig.interiorFogSprite;
            }

            if (arenaConfig != null && arenaConfig.fogSprite != null)
            {
                return arenaConfig.fogSprite;
            }

            if (fogSprite != null)
            {
                return fogSprite;
            }

            fogSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            fogSprite.name = "MidBossFogSprite";
            return fogSprite;
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
    }
}
