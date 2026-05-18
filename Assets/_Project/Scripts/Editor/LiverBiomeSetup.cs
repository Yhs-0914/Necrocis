#if UNITY_EDITOR
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

        private const string Sand1TileAsset = TileFolderPath + "/sand1.asset";
        private const string Sand2TileAsset = TileFolderPath + "/sand2.asset";
        private const string GrassDecoTileAsset = TileFolderPath + "/grass.asset";
        private const string GrassRegionTileAsset = TileFolderPath + "/grass_region.asset";
        private const string GrassWall1TileAsset = TileFolderPath + "/grass_wall_1.asset";
        private const string GrassWall2TileAsset = TileFolderPath + "/grass_wall_2.asset";
        private const string GrassWall3TileAsset = TileFolderPath + "/grass_wall_3.asset";
        private const string SandWallTileAsset = TileFolderPath + "/sand_wall.asset";

        [MenuItem("Necrocis/Setup/Build Liver Sand Biome")]
        public static void BuildLiverSandBiome()
        {
            EnsureSpriteImport(Sand1Png);
            EnsureSpriteImport(Sand2Png);
            EnsureSpriteImport(GrassDecoPng);
            EnsureSpriteImport(GrassRegionPng);
            EnsureSpriteImport(GrassWall1Png);
            EnsureSpriteImport(GrassWall2Png);
            EnsureSpriteImport(GrassWall3Png);
            EnsureSpriteImport(SandWallPng);

            Sprite sand1Sprite = LoadSprite(Sand1Png);
            Sprite sand2Sprite = LoadSprite(Sand2Png);
            Sprite grassDecoSprite = LoadSprite(GrassDecoPng);
            Sprite grassRegionSprite = LoadSprite(GrassRegionPng);
            Sprite grassWall1Sprite = LoadSprite(GrassWall1Png);
            Sprite grassWall2Sprite = LoadSprite(GrassWall2Png);
            Sprite grassWall3Sprite = LoadSprite(GrassWall3Png);
            Sprite sandWallSprite = LoadSprite(SandWallPng);

            if (sand1Sprite == null || sand2Sprite == null || grassDecoSprite == null
                || grassRegionSprite == null
                || grassWall1Sprite == null || grassWall2Sprite == null || grassWall3Sprite == null
                || sandWallSprite == null)
            {
                Debug.LogError(
                    $"[LiverBiomeSetup] Sprite를 찾을 수 없습니다. 경로 확인: " +
                    $"sand1={(sand1Sprite != null)}, sand2={(sand2Sprite != null)}, " +
                    $"grassDeco={(grassDecoSprite != null)}, grassRegion={(grassRegionSprite != null)} ({GrassRegionPng}), " +
                    $"grassWall1={(grassWall1Sprite != null)}, grassWall2={(grassWall2Sprite != null)}, grassWall3={(grassWall3Sprite != null)}, " +
                    $"sandWall={(sandWallSprite != null)}");
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
            config.heightNoiseAmplitude = 0.45f;

            TileBase[] grassWalls = new TileBase[] { grassWall1Tile, grassWall2Tile, grassWall3Tile };

            config.regions.Clear();
            // Sand region 단 1개. 높이 노이즈로 인한 상승 부위에 grass_wall 3-stack 렌더.
            // 맵 남쪽 외곽에 sand_wall 3타일 세로 렌더 (mapEdge 효과).
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
                wallTiles = grassWalls,
                mapEdgeWallTile = sandWallTile,
                mapEdgeWallDepth = 3,
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
