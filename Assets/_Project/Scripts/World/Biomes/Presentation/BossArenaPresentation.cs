using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public enum BossArenaPresentationState
    {
        Approach,
        Locked,
        Cleared
    }

    /// <summary>
    /// 보스 아레나의 진입로, 생체 문, 바이옴별 표식을 관리한다.
    /// 전투 상태는 MidBossArenaController가 소유하고 이 컴포넌트는 표현만 담당한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BossArenaPresentation : MonoBehaviour
    {
        private const int DetailCount = 8;

        private readonly SpriteRenderer[] biomeDetails = new SpriteRenderer[DetailCount];
        private readonly List<WallTileVisual> wallTileVisuals = new List<WallTileVisual>(64);

        private BossArenaPresentationConfig config;
        private BossArenaPresentationState state;
        private Transform gateRoot;
        private SpriteRenderer gateRing;
        private SpriteRenderer gateCore;
        private Vector3 entranceLocalPosition;
        private float phase;
        private Palette palette;
        private bool usesCustomGateSprite;

        public BossArenaPresentationState State => state;
        public Vector3 EntranceWorldPosition => transform.TransformPoint(entranceLocalPosition);

        public static BossArenaPresentation Create(
            Transform owner,
            BiomeManager biome,
            Vector2Int arenaSize,
            MidBossArenaConfig arenaConfig)
        {
            BossArenaPresentationConfig presentationConfig = arenaConfig?.GetPresentationConfig();
            if (owner == null || biome == null || presentationConfig == null || !presentationConfig.enabled)
            {
                return null;
            }

            GameObject presentationObject = new GameObject("BossArenaPresentation");
            presentationObject.transform.SetParent(owner, false);
            BossArenaPresentation presentation = presentationObject.AddComponent<BossArenaPresentation>();
            presentation.Initialize(biome, arenaSize, arenaConfig, presentationConfig);
            return presentation;
        }

        public void SetState(BossArenaPresentationState nextState)
        {
            state = nextState;
            ApplyStateImmediate();
        }

        private void Initialize(
            BiomeManager ownerBiome,
            Vector2Int arenaSize,
            MidBossArenaConfig arenaConfig,
            BossArenaPresentationConfig presentationConfig)
        {
            config = presentationConfig;
            palette = ResolvePalette(ownerBiome.BiomeType, presentationConfig);
            phase = Mathf.Abs(((int)ownerBiome.BiomeType * 1.731f) % Mathf.PI);

            float tileSize = Mathf.Max(0.01f, ownerBiome.TileSize);
            int lockInset = Mathf.Max(0, arenaConfig.lockBoundaryInsetInCells);
            int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
            float playableWidth = Mathf.Max(tileSize * 4f, (arenaSize.x - lockInset * 2) * tileSize);
            float playableDepth = Mathf.Max(tileSize * 4f, (arenaSize.y - lockInset * 2) * tileSize);

            CreateBiomeWallTiles(playableWidth, playableDepth, tileSize);

            Vector2 outward = GetOutwardDirection(config.entranceSide);
            float halfExtent = Mathf.Abs(outward.x) > 0f ? playableWidth * 0.5f : playableDepth * 0.5f;
            float boundaryOffset = Mathf.Max(tileSize, halfExtent - thickness * tileSize * 0.5f);
            entranceLocalPosition = new Vector3(outward.x * boundaryOffset, 0f, outward.y * boundaryOffset);

            CreateGate(tileSize);
            if (wallTileVisuals.Count == 0)
            {
                CreateBiomeDetails(playableWidth, playableDepth, ownerBiome.BiomeType);
            }
            SetState(BossArenaPresentationState.Approach);
        }

        private void LateUpdate()
        {
            if (config == null || gateRoot == null)
            {
                return;
            }

            Camera activeCamera = DontStarveCamera.GetActiveCamera();
            if (activeCamera != null)
            {
                gateRoot.rotation = activeCamera.transform.rotation;
            }

            float time = Time.unscaledTime * Mathf.Max(0.1f, config.pulseSpeed) + phase;
            float pulse = (Mathf.Sin(time * Mathf.PI * 2f) + 1f) * 0.5f;
            UpdateLivingWallTiles(time);
            UpdateDetails(pulse);
        }

        private void CreateGate(float tileSize)
        {
            GameObject gateObject = new GameObject("LivingGate");
            gateObject.transform.SetParent(transform, false);
            gateObject.transform.localPosition = entranceLocalPosition + Vector3.up * (tileSize * 1.65f);
            gateRoot = gateObject.transform;

            gateCore = CreateSpriteRenderer(
                gateRoot,
                "GateMembrane",
                CombatVfxResources.GetSoftCircleSprite(),
                config.gateSortingOrder);
            gateCore.color = WithAlpha(Color.black, 0.58f);

            usesCustomGateSprite = config.entranceSprite != null;
            gateRing = CreateSpriteRenderer(
                gateRoot,
                "GateSphincter",
                usesCustomGateSprite ? config.entranceSprite : CombatVfxResources.GetRingSprite(),
                config.gateSortingOrder + 1);
            gateRing.color = usesCustomGateSprite ? Color.white : palette.primary;
            gateCore.enabled = !usesCustomGateSprite;

            float desiredSize = Mathf.Max(tileSize * 3.6f, config.entranceWidthInCells * tileSize * 0.92f)
                * Mathf.Max(0.25f, config.entranceVisualScale);
            float gateBaseScale = desiredSize / Mathf.Max(0.01f, gateRing.sprite.bounds.size.x);
            gateRing.transform.localScale = Vector3.one * gateBaseScale;
            gateCore.transform.localScale = Vector3.one * (gateBaseScale * 0.82f);
            gateRoot.localPosition = entranceLocalPosition + Vector3.up * (desiredSize * 0.48f);
        }

        private void CreateBiomeDetails(float width, float depth, BiomeType biomeType)
        {
            Sprite detailSprite = GetBiomeDetailSprite(biomeType);
            Vector3[] positions =
            {
                new Vector3(-width * 0.25f, 0.065f, depth * 0.47f),
                new Vector3(width * 0.25f, 0.065f, depth * 0.47f),
                new Vector3(width * 0.47f, 0.065f, depth * 0.25f),
                new Vector3(width * 0.47f, 0.065f, -depth * 0.25f),
                new Vector3(width * 0.25f, 0.065f, -depth * 0.47f),
                new Vector3(-width * 0.25f, 0.065f, -depth * 0.47f),
                new Vector3(-width * 0.47f, 0.065f, -depth * 0.25f),
                new Vector3(-width * 0.47f, 0.065f, depth * 0.25f)
            };

            for (int i = 0; i < biomeDetails.Length; i++)
            {
                SpriteRenderer detail = CreateGroundRenderer(
                    transform,
                    $"BiomeOrganMark{i + 1}",
                    detailSprite,
                    positions[i],
                    i * -45f,
                    config.floorSortingOrder + 1);
                SetRendererSize(detail, 1.35f, 1.35f);
                biomeDetails[i] = detail;
            }
        }

        private void ApplyStateImmediate()
        {
            if (gateRing == null || gateCore == null)
            {
                return;
            }

            bool cleared = state == BossArenaPresentationState.Cleared;
            gateRoot.gameObject.SetActive(state == BossArenaPresentationState.Approach);
            gateRing.color = GetGateColor(cleared ? 0.22f : 0.92f);
            gateCore.color = WithAlpha(cleared ? palette.cleared : Color.black, cleared ? 0.14f : 0.62f);
            for (int i = 0; i < wallTileVisuals.Count; i++)
            {
                WallTileVisual wallTile = wallTileVisuals[i];
                if (wallTile?.renderer != null && wallTile.isDoorClosure)
                {
                    wallTile.renderer.gameObject.SetActive(state == BossArenaPresentationState.Locked);
                }
            }

            UpdateLivingWallTiles(Time.unscaledTime);
        }

        private void CreateBiomeWallTiles(float width, float depth, float tileSize)
        {
            if (config.wallStraightSprites == null
                || config.wallStraightSprites.Length == 0
                || config.wallCornerSprite == null)
            {
                return;
            }

            int widthInCells = Mathf.Max(8, Mathf.RoundToInt(width / tileSize));
            int depthInCells = Mathf.Max(8, Mathf.RoundToInt(depth / tileSize));
            int cornerSpan = Mathf.Clamp(
                config.wallCornerSpanInCells,
                1,
                Mathf.Max(1, Mathf.Min(widthInCells, depthInCells) / 2 - 1));
            int entranceWidth = Mathf.Clamp(config.entranceWidthInCells, 2, widthInCells - 2);
            int openingStart = (widthInCells - entranceWidth) / 2;
            int openingEnd = openingStart + entranceWidth - 1;
            float lowCornerCell = (cornerSpan - 1) * 0.5f;
            float highCornerXCell = widthInCells - 1 - lowCornerCell;
            float highCornerZCell = depthInCells - 1 - lowCornerCell;

            CreateBoundaryCell(
                "Boundary_Corner_NW",
                config.wallCornerSprite,
                lowCornerCell,
                highCornerZCell,
                widthInCells,
                depthInCells,
                tileSize,
                tileSize * cornerSpan,
                config.wallCornerBaseYaw,
                false);
            CreateBoundaryCell(
                "Boundary_Corner_NE",
                config.wallCornerSprite,
                highCornerXCell,
                highCornerZCell,
                widthInCells,
                depthInCells,
                tileSize,
                tileSize * cornerSpan,
                config.wallCornerBaseYaw + 90f,
                false);
            CreateBoundaryCell(
                "Boundary_Corner_SE",
                config.wallCornerSprite,
                highCornerXCell,
                lowCornerCell,
                widthInCells,
                depthInCells,
                tileSize,
                tileSize * cornerSpan,
                config.wallCornerBaseYaw + 180f,
                false);
            CreateBoundaryCell(
                "Boundary_Corner_SW",
                config.wallCornerSprite,
                lowCornerCell,
                lowCornerCell,
                widthInCells,
                depthInCells,
                tileSize,
                tileSize * cornerSpan,
                config.wallCornerBaseYaw + 270f,
                false);

            if (config.wallUseStraightCornerConnectors && cornerSpan >= 2)
            {
                CreateStraightCornerConnectors(
                    widthInCells,
                    depthInCells,
                    cornerSpan,
                    tileSize);
            }

            for (int x = cornerSpan; x < widthInCells - cornerSpan; x++)
            {
                CreateBoundaryCell(
                    $"Boundary_North_{x:D2}",
                    GetStraightSprite(x - cornerSpan),
                    x,
                    depthInCells - 1,
                    widthInCells,
                    depthInCells,
                    tileSize,
                    tileSize,
                    0f,
                    false);
            }

            for (int z = cornerSpan; z < depthInCells - cornerSpan; z++)
            {
                CreateBoundaryCell(
                    $"Boundary_West_{z:D2}",
                    GetStraightSprite(z - cornerSpan),
                    0,
                    z,
                    widthInCells,
                    depthInCells,
                    tileSize,
                    tileSize,
                    270f,
                    false);
                CreateBoundaryCell(
                    $"Boundary_East_{z:D2}",
                    GetStraightSprite(cornerSpan - 1 - z),
                    widthInCells - 1,
                    z,
                    widthInCells,
                    depthInCells,
                    tileSize,
                    tileSize,
                    90f,
                    false);
            }

            for (int x = cornerSpan; x < widthInCells - cornerSpan; x++)
            {
                bool isDoorClosure = x >= openingStart && x <= openingEnd;
                CreateBoundaryCell(
                    isDoorClosure ? $"Boundary_DoorClosure_{x:D2}" : $"Boundary_South_{x:D2}",
                    GetStraightSprite(cornerSpan - 1 - x),
                    x,
                    0,
                    widthInCells,
                    depthInCells,
                    tileSize,
                    tileSize,
                    180f,
                    isDoorClosure);
            }
        }

        private Sprite GetStraightSprite(int index)
        {
            int count = config.wallStraightSprites.Length;
            int normalizedIndex = ((index % count) + count) % count;
            return config.wallStraightSprites[normalizedIndex];
        }

        private void CreateStraightCornerConnectors(
            int widthInCells,
            int depthInCells,
            int cornerSpan,
            float tileSize)
        {
            int westConnectorX = cornerSpan - 1;
            int eastConnectorX = widthInCells - cornerSpan;
            int southConnectorZ = cornerSpan - 1;
            int northConnectorZ = depthInCells - cornerSpan;

            CreateBoundaryCell(
                "Boundary_North_CornerConnector_NW",
                GetStraightSprite(westConnectorX - cornerSpan),
                westConnectorX,
                depthInCells - 1,
                widthInCells,
                depthInCells,
                tileSize,
                tileSize,
                0f,
                false,
                4);
            CreateBoundaryCell(
                "Boundary_North_CornerConnector_NE",
                GetStraightSprite(eastConnectorX - cornerSpan),
                eastConnectorX,
                depthInCells - 1,
                widthInCells,
                depthInCells,
                tileSize,
                tileSize,
                0f,
                false,
                4);
            CreateBoundaryCell(
                "Boundary_South_CornerConnector_SW",
                GetStraightSprite(cornerSpan - 1 - westConnectorX),
                westConnectorX,
                0,
                widthInCells,
                depthInCells,
                tileSize,
                tileSize,
                180f,
                false,
                4);
            CreateBoundaryCell(
                "Boundary_South_CornerConnector_SE",
                GetStraightSprite(cornerSpan - 1 - eastConnectorX),
                eastConnectorX,
                0,
                widthInCells,
                depthInCells,
                tileSize,
                tileSize,
                180f,
                false,
                4);
            CreateBoundaryCell(
                "Boundary_West_CornerConnector_SW",
                GetStraightSprite(southConnectorZ - cornerSpan),
                0,
                southConnectorZ,
                widthInCells,
                depthInCells,
                tileSize,
                tileSize,
                270f,
                false,
                4);
            CreateBoundaryCell(
                "Boundary_West_CornerConnector_NW",
                GetStraightSprite(northConnectorZ - cornerSpan),
                0,
                northConnectorZ,
                widthInCells,
                depthInCells,
                tileSize,
                tileSize,
                270f,
                false,
                4);
            CreateBoundaryCell(
                "Boundary_East_CornerConnector_SE",
                GetStraightSprite(cornerSpan - 1 - southConnectorZ),
                widthInCells - 1,
                southConnectorZ,
                widthInCells,
                depthInCells,
                tileSize,
                tileSize,
                90f,
                false,
                4);
            CreateBoundaryCell(
                "Boundary_East_CornerConnector_NE",
                GetStraightSprite(cornerSpan - 1 - northConnectorZ),
                widthInCells - 1,
                northConnectorZ,
                widthInCells,
                depthInCells,
                tileSize,
                tileSize,
                90f,
                false,
                4);
        }

        private void CreateBoundaryCell(
            string objectName,
            Sprite sprite,
            float xCell,
            float zCell,
            int widthInCells,
            int depthInCells,
            float tileSize,
            float targetWidth,
            float yaw,
            bool isDoorClosure,
            int sortingOrderOffset = 3)
        {
            Vector3 position = new Vector3(
                (xCell - (widthInCells - 1) * 0.5f) * tileSize,
                0.12f,
                (zCell - (depthInCells - 1) * 0.5f) * tileSize);
            SpriteRenderer renderer = CreateGroundRenderer(
                transform,
                objectName,
                sprite,
                position,
                yaw,
                config.floorSortingOrder + sortingOrderOffset);
            SetRendererUniformCellSize(renderer, targetWidth);

            int index = wallTileVisuals.Count;
            WallTileVisual visual = new WallTileVisual
            {
                renderer = renderer,
                basePosition = renderer.transform.localPosition,
                phase = index * 1.731f + Hash01(index * 17 + 3) * 2.4f,
                speed = Mathf.Lerp(0.68f, 1.18f, Hash01(index * 23 + 11)),
                isDoorClosure = isDoorClosure
            };
            renderer.gameObject.SetActive(!isDoorClosure);
            wallTileVisuals.Add(visual);
        }

        private static void SetRendererUniformCellSize(SpriteRenderer renderer, float cellSize)
        {
            if (renderer == null || renderer.sprite == null)
            {
                return;
            }

            float sourceWidth = Mathf.Max(0.01f, renderer.sprite.bounds.size.x);
            float uniformScale = cellSize / sourceWidth;
            renderer.transform.localScale = new Vector3(uniformScale, uniformScale, uniformScale);
        }

        private void UpdateLivingWallTiles(float time)
        {
            Color baseColor = state switch
            {
                BossArenaPresentationState.Locked => Color.Lerp(Color.white, palette.locked, 0.18f),
                BossArenaPresentationState.Cleared => Color.Lerp(Color.white, palette.cleared, 0.78f),
                _ => Color.white
            };
            baseColor.a = state == BossArenaPresentationState.Cleared ? 0.16f : 1f;

            for (int i = 0; i < wallTileVisuals.Count; i++)
            {
                WallTileVisual visual = wallTileVisuals[i];
                if (visual?.renderer == null || !visual.renderer.gameObject.activeSelf)
                {
                    continue;
                }

                float breath = state == BossArenaPresentationState.Cleared
                    ? 0f
                    : Mathf.Sin(time * visual.speed * Mathf.PI * 2f + visual.phase);
                visual.renderer.transform.localPosition = visual.basePosition;
                float tissueShift = state == BossArenaPresentationState.Cleared
                    ? 0f
                    : (breath + 1f) * 0.012f;
                Color animatedColor = Color.Lerp(baseColor, palette.accent, tissueShift);
                animatedColor.a = baseColor.a;
                visual.renderer.color = animatedColor;
            }
        }

        private static float Hash01(int value)
        {
            float raw = Mathf.Sin(value * 12.9898f + 78.233f) * 43758.5453f;
            return raw - Mathf.Floor(raw);
        }

        private void UpdateDetails(float pulse)
        {
            Color color = state switch
            {
                BossArenaPresentationState.Locked => Color.Lerp(palette.locked, palette.accent, pulse * 0.35f),
                BossArenaPresentationState.Cleared => palette.cleared,
                _ => Color.Lerp(palette.primary, palette.accent, pulse * 0.45f)
            };
            float alpha = state == BossArenaPresentationState.Cleared ? 0.16f : Mathf.Lerp(0.32f, 0.58f, pulse);

            for (int i = 0; i < biomeDetails.Length; i++)
            {
                if (biomeDetails[i] != null)
                {
                    biomeDetails[i].color = WithAlpha(color, alpha);
                }
            }
        }

        private Color GetStateColor()
        {
            return state switch
            {
                BossArenaPresentationState.Locked => palette.locked,
                BossArenaPresentationState.Cleared => palette.cleared,
                _ => palette.primary
            };
        }

        private Color GetGateColor(float alpha)
        {
            if (!usesCustomGateSprite)
            {
                return WithAlpha(GetStateColor(), alpha);
            }

            Color color = state switch
            {
                BossArenaPresentationState.Locked => Color.Lerp(Color.white, palette.locked, 0.28f),
                BossArenaPresentationState.Cleared => Color.Lerp(Color.white, palette.cleared, 0.68f),
                _ => Color.white
            };
            return WithAlpha(color, alpha);
        }

        private static SpriteRenderer CreateSpriteRenderer(
            Transform parent,
            string objectName,
            Sprite sprite,
            int sortingOrder)
        {
            GameObject spriteObject = new GameObject(objectName);
            spriteObject.transform.SetParent(parent, false);
            SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return renderer;
        }

        private static SpriteRenderer CreateGroundRenderer(
            Transform parent,
            string objectName,
            Sprite sprite,
            Vector3 localPosition,
            float yaw,
            int sortingOrder)
        {
            SpriteRenderer renderer = CreateSpriteRenderer(parent, objectName, sprite, sortingOrder);
            renderer.transform.localPosition = localPosition;
            renderer.transform.localRotation = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
            return renderer;
        }

        private static void SetRendererSize(SpriteRenderer renderer, float width, float height)
        {
            if (renderer == null || renderer.sprite == null)
            {
                return;
            }

            Vector2 size = renderer.sprite.bounds.size;
            renderer.transform.localScale = new Vector3(
                width / Mathf.Max(0.01f, size.x),
                height / Mathf.Max(0.01f, size.y),
                1f);
        }

        private static Vector2 GetOutwardDirection(BossArenaEntranceSide side)
        {
            return side switch
            {
                BossArenaEntranceSide.North => Vector2.up,
                BossArenaEntranceSide.West => Vector2.left,
                BossArenaEntranceSide.East => Vector2.right,
                _ => Vector2.down
            };
        }

        private static Sprite GetBiomeDetailSprite(BiomeType biomeType)
        {
            return biomeType switch
            {
                BiomeType.Liver => CombatVfxResources.GetBloodDropSprite(),
                BiomeType.Stomach => CombatVfxResources.GetFangSprite(),
                BiomeType.Lung => CombatVfxResources.GetWindArcSprite(),
                _ => CombatVfxResources.GetSplatSprite()
            };
        }

        private static Palette ResolvePalette(BiomeType biomeType, BossArenaPresentationConfig presentationConfig)
        {
            if (!presentationConfig.useBiomePalette)
            {
                return new Palette(
                    presentationConfig.primaryColor,
                    presentationConfig.accentColor,
                    presentationConfig.lockedColor,
                    presentationConfig.clearedColor);
            }

            return biomeType switch
            {
                BiomeType.Liver => new Palette(
                    new Color(0.58f, 0.04f, 0.08f, 1f),
                    new Color(0.74f, 0.55f, 0.08f, 1f),
                    new Color(0.94f, 0.04f, 0.06f, 1f),
                    new Color(0.26f, 0.12f, 0.14f, 1f)),
                BiomeType.Stomach => new Palette(
                    new Color(0.96f, 0.38f, 0.04f, 1f),
                    new Color(1f, 0.74f, 0.18f, 1f),
                    new Color(0.9f, 0.08f, 0.04f, 1f),
                    new Color(0.34f, 0.2f, 0.12f, 1f)),
                BiomeType.Lung => new Palette(
                    new Color(0.42f, 0.84f, 0.9f, 1f),
                    new Color(0.78f, 0.68f, 0.94f, 1f),
                    new Color(0.56f, 0.2f, 0.5f, 1f),
                    new Color(0.38f, 0.42f, 0.46f, 1f)),
                _ => new Palette(
                    new Color(0.4f, 0.68f, 0.2f, 1f),
                    new Color(0.75f, 0.42f, 0.12f, 1f),
                    new Color(0.78f, 0.08f, 0.04f, 1f),
                    new Color(0.25f, 0.23f, 0.2f, 1f))
            };
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a *= Mathf.Clamp01(alpha);
            return color;
        }

        private sealed class WallTileVisual
        {
            public SpriteRenderer renderer;
            public Vector3 basePosition;
            public float phase;
            public float speed;
            public bool isDoorClosure;
        }

        private readonly struct Palette
        {
            public readonly Color primary;
            public readonly Color accent;
            public readonly Color locked;
            public readonly Color cleared;

            public Palette(Color primary, Color accent, Color locked, Color cleared)
            {
                this.primary = primary;
                this.accent = accent;
                this.locked = locked;
                this.cleared = cleared;
            }
        }
    }
}
