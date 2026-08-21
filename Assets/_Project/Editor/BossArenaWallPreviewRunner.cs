using System;
using System.IO;
using Necrocis;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace NecrocisEditor
{
    public static class BossArenaWallPreviewRunner
    {
        private static readonly string[] Names = { "Intestine", "Liver", "Stomach", "Lung" };

        [MenuItem("Tools/Necrocis/Render Boss Arena Wall Previews")]
        public static void Run()
        {
            for (int i = 0; i < Names.Length; i++) Render(Names[i]);
            Debug.Log("[BossArenaWallPreview] PASS - previews saved under /tmp/necrocis-final-wall-*");
        }

        public static void RunBatch()
        {
            try
            {
                Run();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void Render(string name)
        {
            BossArenaConfig config = AssetDatabase.LoadAssetAtPath<BossArenaConfig>(
                $"Assets/_Project/Data/BiomeConfigs/{name}BossArenaConfig.asset");
            GameObject biomeObject = new GameObject("PreviewBiome");
            BossArenaWallPreviewBiome biome = biomeObject.AddComponent<BossArenaWallPreviewBiome>();
            biome.ConfigureForPreview(config.midBossArena.boss.patternType);
            GameObject arenaObject = new GameObject("PreviewArena");
            BossArenaPresentation presentation = BossArenaPresentation.Create(
                arenaObject.transform,
                biome,
                new Vector2Int(32, 32),
                config.midBossArena);
            ValidateLayout(name, presentation, config.midBossArena);
            GameObject cameraObject = new GameObject("PreviewCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 17.25f;
            camera.aspect = 1f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.02f, 0.025f, 1f);
            camera.transform.position = new Vector3(0f, 24f, -24f);
            camera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            Transform gate = presentation.transform.Find("LivingGate");
            if (gate != null)
            {
                gate.rotation = camera.transform.rotation;
            }
            Capture(camera, $"/tmp/necrocis-final-wall-{name.ToLowerInvariant()}.png");
            CaptureCorner(camera, name, "nw", new Vector3(-15f, 0f, 15f));
            CaptureCorner(camera, name, "ne", new Vector3(15f, 0f, 15f));
            CaptureCorner(camera, name, "se", new Vector3(15f, 0f, -15f));
            CaptureCorner(camera, name, "sw", new Vector3(-15f, 0f, -15f));
            camera.orthographicSize = 17.25f;
            camera.transform.position = new Vector3(0f, 24f, -24f);
            presentation.SetState(BossArenaPresentationState.Locked);
            Capture(camera, $"/tmp/necrocis-final-wall-{name.ToLowerInvariant()}-locked.png");
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(arenaObject);
            UnityEngine.Object.DestroyImmediate(biomeObject);
        }

        private static void CaptureCorner(Camera camera, string biomeName, string suffix, Vector3 target)
        {
            camera.orthographicSize = 3.25f;
            camera.transform.position = target + new Vector3(0f, 24f, -24f);
            Capture(
                camera,
                $"/tmp/necrocis-final-wall-{biomeName.ToLowerInvariant()}-corner-{suffix}.png");
        }

        private static void ValidateLayout(
            string biomeName,
            BossArenaPresentation presentation,
            MidBossArenaConfig arenaConfig)
        {
            const float epsilon = 0.001f;
            int arenaWidth = Mathf.Max(8, arenaConfig.arenaSize.x);
            int arenaDepth = Mathf.Max(8, arenaConfig.arenaSize.y);
            int inset = Mathf.Max(0, arenaConfig.lockBoundaryInsetInCells);
            int visualWidth = arenaWidth - inset * 2;
            int visualDepth = arenaDepth - inset * 2;
            int cornerSpan = Mathf.Clamp(
                arenaConfig.GetPresentationConfig().wallCornerSpanInCells,
                1,
                Mathf.Max(1, Mathf.Min(visualWidth, visualDepth) / 2 - 1));
            int expectedBoundaryCount = 4
                + (visualWidth - cornerSpan * 2) * 2
                + (visualDepth - cornerSpan * 2) * 2;
            if (arenaConfig.GetPresentationConfig().wallUseStraightCornerConnectors
                && cornerSpan >= 2)
            {
                expectedBoundaryCount += 8;
            }
            int expectedDoorCount = Mathf.Clamp(
                arenaConfig.GetPresentationConfig().entranceWidthInCells,
                2,
                visualWidth - 2);
            float expectedHalfWidth = (visualWidth - 1) * 0.5f;
            float expectedHalfDepth = (visualDepth - 1) * 0.5f;
            int boundaryCount = 0;
            int doorCount = 0;

            SpriteRenderer[] renderers = presentation.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (!renderer.name.StartsWith("Boundary_", StringComparison.Ordinal))
                {
                    continue;
                }

                boundaryCount++;
                if (renderer.name.StartsWith("Boundary_DoorClosure_", StringComparison.Ordinal))
                {
                    doorCount++;
                }

                Vector3 position = renderer.transform.localPosition;
                bool isCorner = renderer.name.StartsWith("Boundary_Corner_", StringComparison.Ordinal);
                float expectedCornerX = expectedHalfWidth - (cornerSpan - 1) * 0.5f;
                float expectedCornerZ = expectedHalfDepth - (cornerSpan - 1) * 0.5f;
                bool onHorizontalBoundary = Mathf.Abs(
                    Mathf.Abs(position.z) - (isCorner ? expectedCornerZ : expectedHalfDepth)) <= epsilon;
                bool onVerticalBoundary = Mathf.Abs(
                    Mathf.Abs(position.x) - (isCorner ? expectedCornerX : expectedHalfWidth)) <= epsilon;
                if (!onHorizontalBoundary && !onVerticalBoundary)
                {
                    throw new InvalidOperationException(
                        $"[{biomeName}] {renderer.name} is off the physical boundary: {position}");
                }

                float snappedX = position.x + expectedHalfWidth - (isCorner ? (cornerSpan - 1) * 0.5f : 0f);
                float snappedZ = position.z + expectedHalfDepth - (isCorner ? (cornerSpan - 1) * 0.5f : 0f);
                if (!isCorner
                    && (Mathf.Abs(snappedX - Mathf.Round(snappedX)) > epsilon
                        || Mathf.Abs(snappedZ - Mathf.Round(snappedZ)) > epsilon))
                {
                    throw new InvalidOperationException(
                        $"[{biomeName}] {renderer.name} is not centered on a collision cell: {position}");
                }

                Vector3 scale = renderer.transform.localScale;
                if (Mathf.Abs(scale.x - scale.y) > epsilon)
                {
                    throw new InvalidOperationException(
                        $"[{biomeName}] {renderer.name} has non-uniform sprite scale: {scale}");
                }

                float expectedRenderedWidth = isCorner ? cornerSpan : 1f;
                float renderedCellLength = renderer.sprite.bounds.size.x * scale.x;
                if (Mathf.Abs(renderedCellLength - expectedRenderedWidth) > epsilon)
                {
                    throw new InvalidOperationException(
                        $"[{biomeName}] {renderer.name} width {renderedCellLength} "
                        + $"does not match {expectedRenderedWidth} collision cells");
                }

                if (isCorner)
                {
                    float cornerOffset = renderer.name.EndsWith("NW", StringComparison.Ordinal) ? 0f
                        : renderer.name.EndsWith("NE", StringComparison.Ordinal) ? 90f
                        : renderer.name.EndsWith("SE", StringComparison.Ordinal) ? 180f
                        : 270f;
                    float expectedYaw = arenaConfig.GetPresentationConfig().wallCornerBaseYaw + cornerOffset;
                    Quaternion expectedRotation = Quaternion.AngleAxis(expectedYaw, Vector3.up)
                        * Quaternion.Euler(90f, 0f, 0f);
                    if (Quaternion.Angle(renderer.transform.localRotation, expectedRotation) > 0.01f)
                    {
                        throw new InvalidOperationException(
                            $"[{biomeName}] {renderer.name} has the wrong corner orientation");
                    }
                }
                else
                {
                    float expectedYaw = renderer.name.StartsWith("Boundary_West_", StringComparison.Ordinal) ? 270f
                        : renderer.name.StartsWith("Boundary_East_", StringComparison.Ordinal) ? 90f
                        : renderer.name.StartsWith("Boundary_South_", StringComparison.Ordinal)
                            || renderer.name.StartsWith("Boundary_DoorClosure_", StringComparison.Ordinal) ? 180f
                        : 0f;
                    Quaternion expectedRotation = Quaternion.AngleAxis(expectedYaw, Vector3.up)
                        * Quaternion.Euler(90f, 0f, 0f);
                    if (Quaternion.Angle(renderer.transform.localRotation, expectedRotation) > 0.01f
                        || renderer.flipX
                        || renderer.flipY)
                    {
                        throw new InvalidOperationException(
                            $"[{biomeName}] {renderer.name} has a flipped or incorrect wall orientation");
                    }
                }
            }

            if (boundaryCount != expectedBoundaryCount || doorCount != expectedDoorCount)
            {
                throw new InvalidOperationException(
                    $"[{biomeName}] boundary count {boundaryCount}/{expectedBoundaryCount}, "
                    + $"door count {doorCount}/{expectedDoorCount}");
            }

            float wallPadding = Mathf.Max(1, arenaConfig.wallThicknessInCells) + inset;
            Vector2 footprint = new Vector2(0.68f, 0.48f);
            Vector2 expectedPlayerCenterLimit = new Vector2(
                arenaWidth * 0.5f - wallPadding - footprint.x,
                arenaDepth * 0.5f - wallPadding - footprint.y);
            Debug.Log(
                $"[BossArenaWallPreview] {biomeName} aligned: {boundaryCount} renderers cover "
                + $"{visualWidth * 2 + visualDepth * 2 - 4} physical boundary cells, "
                + $"door {doorCount}, player center limit ±({expectedPlayerCenterLimit.x:F2}, "
                + $"{expectedPlayerCenterLimit.y:F2}) for footprint {footprint.x:F2}x{footprint.y:F2}");
        }

        private static void Capture(Camera camera, string path)
        {
            RenderTexture target = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = target;
            camera.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            Texture2D output = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
            output.ReadPixels(new Rect(0f, 0f, 1024f, 1024f), 0, 0);
            output.Apply();
            File.WriteAllBytes(path, output.EncodeToPNG());
            RenderTexture.active = previous;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(output);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
        }
    }

    public sealed class BossArenaWallPreviewBiome : BiomeManager
    {
        public void ConfigureForPreview(MidBossPatternType pattern)
        {
            biomeType = pattern switch
            {
                MidBossPatternType.Liver => BiomeType.Liver,
                MidBossPatternType.Stomach => BiomeType.Stomach,
                MidBossPatternType.Lung => BiomeType.Lung,
                _ => BiomeType.Intestine
            };
            mapWidth = 300;
            mapHeight = 300;
            tileSize = 1f;
            chunkSize = 16;
        }

        protected override void GenerateObjectsForChunk(Chunk chunk) { }
        protected override TileSample SampleBaseTile(int worldX, int worldY) =>
            new TileSample(BiomeTileType.None, null, true);
        protected override TileBase GetTileAsset(BiomeTileType tileType) => null;
    }
}
