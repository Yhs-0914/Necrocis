#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Necrocis.EditorTools
{
    public static class LiverBiomeSetup
    {
        private const string ConfigPath = "Assets/_Project/Data/BiomeConfigs/LiverBiomeConfig.asset";
        private const string TileFolderPath = "Assets/_Project/Data/Tiles/Liver";
        private const string LiverImagesPath = "Assets/_Project/Art/Images/Liver";
        private const string LiverTilesPath = LiverImagesPath + "/Tiles";
        private const string LiverWallsPath = LiverImagesPath + "/Walls";

        private const string Sand1Png = LiverImagesPath + "/sand1.png";
        private const string Sand2Png = LiverImagesPath + "/sand2.png";
        private const string GrassDecoPng = LiverImagesPath + "/grass_.png";
        private const string GrassRegionPng = LiverTilesPath + "/grass1_5.png";

        private const string GrassWall1Png = LiverWallsPath + "/grass_wall_1.png";
        private const string GrassWall2Png = LiverWallsPath + "/grass_wall_2.png";
        private const string GrassWall3Png = LiverWallsPath + "/grass_wall_3.png";
        private const string SandWallPng = LiverWallsPath + "/sand_wall.png";
        private const string SideWallBlackPng = LiverWallsPath + "/wall_side_black.png";

        private const string Sand1TileAsset = TileFolderPath + "/sand1.asset";
        private const string Sand2TileAsset = TileFolderPath + "/sand2.asset";
        private const string GrassDecoTileAsset = TileFolderPath + "/grass.asset";
        private const string GrassRegionTileAsset = TileFolderPath + "/grass_region.asset";
        private const string GrassWall1TileAsset = TileFolderPath + "/grass_wall_1.asset";
        private const string GrassWall2TileAsset = TileFolderPath + "/grass_wall_2.asset";
        private const string GrassWall3TileAsset = TileFolderPath + "/grass_wall_3.asset";
        private const string SandWallTileAsset = TileFolderPath + "/sand_wall.asset";
        private const string SideWallBlackTileAsset = TileFolderPath + "/wall_side_black.asset";

        [MenuItem("Necrocis/Setup/Build Liver Sand Biome")]
        public static void BuildLiverSandBiome()
        {
            EnsureSolidColorSprite(SideWallBlackPng, Color.black, 16);

            EnsureSpriteImport(Sand1Png);
            EnsureSpriteImport(Sand2Png);
            EnsureSpriteImport(GrassDecoPng);
            EnsureSpriteImport(GrassRegionPng);
            EnsureSpriteImport(GrassWall1Png);
            EnsureSpriteImport(GrassWall2Png);
            EnsureSpriteImport(GrassWall3Png);
            EnsureSpriteImport(SandWallPng);
            EnsureSpriteImport(SideWallBlackPng);

            Sprite sand1Sprite = LoadSprite(Sand1Png);
            Sprite sand2Sprite = LoadSprite(Sand2Png);
            Sprite grassDecoSprite = LoadSprite(GrassDecoPng);
            Sprite grassRegionSprite = LoadSprite(GrassRegionPng);
            Sprite grassWall1Sprite = LoadSprite(GrassWall1Png);
            Sprite grassWall2Sprite = LoadSprite(GrassWall2Png);
            Sprite grassWall3Sprite = LoadSprite(GrassWall3Png);
            Sprite sandWallSprite = LoadSprite(SandWallPng);
            Sprite sideWallBlackSprite = LoadSprite(SideWallBlackPng);

            if (sand1Sprite == null || sand2Sprite == null || grassDecoSprite == null
                || grassRegionSprite == null
                || grassWall1Sprite == null || grassWall2Sprite == null || grassWall3Sprite == null
                || sandWallSprite == null || sideWallBlackSprite == null)
            {
                Debug.LogError(
                    $"[LiverBiomeSetup] Sprite를 찾을 수 없습니다. 경로 확인: " +
                    $"sand1={(sand1Sprite != null)}, sand2={(sand2Sprite != null)}, " +
                    $"grassDeco={(grassDecoSprite != null)}, grassRegion={(grassRegionSprite != null)} ({GrassRegionPng}), " +
                    $"grassWall1={(grassWall1Sprite != null)}, grassWall2={(grassWall2Sprite != null)}, grassWall3={(grassWall3Sprite != null)}, " +
                    $"sandWall={(sandWallSprite != null)}, sideWallBlack={(sideWallBlackSprite != null)}");
                return;
            }

            EnsureFolder(TileFolderPath);

            Tile sand1Tile = GetOrCreateTile(Sand1TileAsset, sand1Sprite);
            Tile sand2Tile = GetOrCreateTile(Sand2TileAsset, sand2Sprite);
            Tile grassDecoTile = GetOrCreateTile(GrassDecoTileAsset, grassDecoSprite);
            Tile grassRegionTile = GetOrCreateTile(GrassRegionTileAsset, grassRegionSprite);
            Tile grassWall1Tile = GetOrCreateTile(GrassWall1TileAsset, grassWall1Sprite);
            Tile grassWall2Tile = GetOrCreateTile(GrassWall2TileAsset, grassWall2Sprite);
            Tile grassWall3Tile = GetOrCreateTile(GrassWall3TileAsset, grassWall3Sprite);
            Tile sandWallTile = GetOrCreateTile(SandWallTileAsset, sandWallSprite);
            Tile sideWallBlackTile = GetOrCreateTile(SideWallBlackTileAsset, sideWallBlackSprite);

            BiomeConfig config = AssetDatabase.LoadAssetAtPath<BiomeConfig>(ConfigPath);
            if (config == null)
            {
                Debug.LogError($"[LiverBiomeSetup] LiverBiomeConfig를 찾을 수 없습니다: {ConfigPath}");
                return;
            }

            Undo.RecordObject(config, "Setup Liver Sand Biome");

            config.regionCellSize = 35f;
            config.regionBlendWidth = 4f;
            config.detailNoiseScale = 0.05f;
            config.heightNoiseScale = 0.01f;      // 큰 고원 (period ~100 타일)
            config.heightNoiseAmplitude = 0f;     // threshold 모드에서는 사용 안 함
            config.heightThreshold = 0.35f;       // Perlin > 0.35 → 고원 (약 30% 면적)
            config.heightCaIterations = 2;        // CA 2회 평활화 (1x1 파편 + 톱니 제거)

            // Sand 지형 자체의 오르막은 sand 벽면.
            // grass_wall은 나중에 grass region 추가될 때를 위해 보존 (현재는 사용 안 함).
            TileBase[] sandWalls = new TileBase[] { sandWallTile, sandWallTile, sandWallTile };

            config.regions.Clear();
            // Sand region. 높이 노이즈로 인한 상승 부위에 sand_wall 3-stack 렌더.
            // 맵 남쪽 외곽에도 sand_wall 3타일 세로 렌더.
            // 동/서/모서리는 검정 실루엣 (sideWallTile).
            // plateauRise=3 → 시각 3-stack과 height 단계가 정합 (maxStepHeight=1로 못 오름).
            config.regions.Add(new BiomeRegionDefinition
            {
                name = "Sand",
                baseHeight = 0,
                primaryTile = sand1Tile,
                primaryType = BiomeTileType.Floor,
                variantTile = sand2Tile,
                variantType = BiomeTileType.FloorVariant,
                variantThreshold = 0.65f,
                tileVariants = null,
                wallTiles = sandWalls,
                mapEdgeWallTile = sandWallTile,
                mapEdgeWallDepth = 3,
                sideWallTile = sideWallBlackTile,
                plateauRise = 3,
            });

            config.tileMappings.Clear();
            config.tileMappings.Add(new TileTypeMapping
            {
                tileType = BiomeTileType.Floor,
                tile = sand1Tile,
            });
            config.tileMappings.Add(new TileTypeMapping
            {
                tileType = BiomeTileType.FloorVariant,
                tile = sand2Tile,
            });

            config.objectRules.Clear();
            // Grass — 잔디 (scatter decoration). density 높여서 자주, scaleBias=1로 작은~큰 골고루.
            config.objectRules.Add(new BiomeObjectRuleConfig
            {
                name = "Grass",
                poolKind = BiomeObjectKind.FloorDecoration,
                density = 0.15f,
                minDistance = 3f,
                poissonSalt = 201,
                allowedRegions = new System.Collections.Generic.List<int> { 0 },
                blocksMovement = false,
                heightOffset = 0.01f,
                sortingOrder = 50,
                sprites = new[] { grassDecoSprite },
                useDeterministicSprite = true,
                spriteSalt = 0,
                animate = false,
                animationSpeed = 0.15f,
                useBillboard = false,
                useYSort = false,
                addCollider = false,
                isTrigger = false,
                colliderSize = new Vector3(1f, 1f, 1f),
                colliderCenter = Vector3.zero,
                scaleRange = new Vector2(0.5f, 2.8f),
                scaleSalt = 4201,
                scaleBias = 1f,
                spacingPadding = 1.6f,
            });
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[LiverBiomeSetup] 완료: Sand region + grass_wall(3-stack) 높이 cliff + sand_wall 맵 외곽 남쪽 벽. Grass decoration 유지. Pond 제거.");
        }

        private static void EnsureSolidColorSprite(string path, Color color, int size)
        {
            if (File.Exists(path)) return;

            string absolutePath = Path.GetFullPath(path);
            string absoluteDir = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(absoluteDir) && !Directory.Exists(absoluteDir))
            {
                Directory.CreateDirectory(absoluteDir);
            }

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            tex.SetPixels(pixels);
            tex.Apply();

            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            File.WriteAllBytes(absolutePath, png);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        private static void EnsureSpriteImport(string path, float pixelsPerUnit = 100f)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[LiverBiomeSetup] 임포터 없음 (파일이 없거나 텍스처가 아님): {path}");
                return;
            }

            bool dirty = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                dirty = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                dirty = true;
            }
            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                dirty = true;
            }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                dirty = true;
            }
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                dirty = true;
            }
            if (Mathf.Abs(importer.spritePixelsPerUnit - pixelsPerUnit) > 0.01f)
            {
                importer.spritePixelsPerUnit = pixelsPerUnit;
                dirty = true;
            }

            if (dirty)
            {
                importer.SaveAndReimport();
            }
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;

            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in subAssets)
            {
                if (asset is Sprite s) return s;
            }
            return null;
        }

        private static Tile GetOrCreateTile(string assetPath, Sprite sprite)
        {
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(assetPath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, assetPath);
            }

            Undo.RecordObject(tile, "Assign Tile Sprite");
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
#endif
