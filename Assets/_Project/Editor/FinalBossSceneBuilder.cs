using System;
using System.Linq;
using Necrocis;
using ProceduralMap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace NecrocisEditor
{
    public static class FinalBossSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/FinalBoss.unity";
        private const string ArtPath = "Assets/_Project/Art/Generated/FinalBoss";
        private const string LayoutPath = ArtPath + "/FinalBossLayout.asset";
        private static readonly RectInt[] PillarBounds =
        {
            new RectInt(9, 23, 6, 3), new RectInt(33, 23, 6, 3),
            new RectInt(9, 9, 6, 3), new RectInt(33, 9, 6, 3)
        };

        private static readonly Vector2[][] PillarContours = {
            new[] { new Vector2(.48f,0), new Vector2(.03f,.17f), new Vector2(.17f,.30f),
                new Vector2(.17f,.49f), new Vector2(.18f,.64f), new Vector2(.29f,.74f),
                new Vector2(.48f,.79f), new Vector2(.58f,1),
                new Vector2(.75f,1), new Vector2(.79f,.77f), new Vector2(.94f,.60f),
                new Vector2(.88f,.28f), new Vector2(1,.12f) },
            new[] { new Vector2(.5f,0), new Vector2(0,.17f), new Vector2(.20f,.36f),
                new Vector2(.19f,.72f), new Vector2(.35f,1), new Vector2(.62f,.99f),
                new Vector2(.82f,.79f), new Vector2(.87f,.36f), new Vector2(1,.17f) },
            new[] { new Vector2(.5f,0), new Vector2(.04f,.13f), new Vector2(.16f,.30f),
                new Vector2(.08f,.43f), new Vector2(.065f,.63f), new Vector2(.12f,.79f),
                new Vector2(.24f,.90f), new Vector2(.40f,.95f), new Vector2(.60f,.93f), new Vector2(.86f,1),
                new Vector2(.95f,.87f), new Vector2(.86f,.47f), new Vector2(1,.16f) },
            new[] { new Vector2(.5f,0), new Vector2(.05f,.13f), new Vector2(.18f,.46f),
                new Vector2(.34f,.71f), new Vector2(.47f,.77f), new Vector2(.48f,1),
                new Vector2(.60f,1), new Vector2(.64f,.78f), new Vector2(.83f,.61f),
                new Vector2(.90f,.33f), new Vector2(1,.15f) }
        };

        private static readonly Vector2[] CerebrumOutline = {
            new Vector2(.5f,0), new Vector2(.07f,.08f), new Vector2(0,.34f),
            new Vector2(.17f,.75f), new Vector2(.32f,1), new Vector2(.70f,1),
            new Vector2(.90f,.77f), new Vector2(1,.34f), new Vector2(.93f,.09f)
        };

        [MenuItem("Tools/Necrocis/Final Boss/Build Exploration Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before rebuilding the arena.");
            Scene previous = SceneManager.GetActiveScene();
            Scene hub = SceneManager.GetSceneByPath("Assets/_Project/Scenes/Hub.unity");
            bool openedHub = !hub.isLoaded;
            Scene arenaScene = default;
            if (SceneManager.GetSceneByPath(ScenePath).isLoaded)
                throw new InvalidOperationException("Close FinalBoss before rebuilding it; other scenes can stay open.");
            // Preserve the reference aspect ratio: nearest-power-of-two resizing squashes organ sprites.
            foreach (string filename in new[] { "CerebrumArena_Dormant.png", "CerebrumEnvironment_v2.png" })
            {
                var importer = AssetImporter.GetAtPath(ArtPath + "/" + filename) as TextureImporter;
                if (importer == null) throw new InvalidOperationException("Missing artwork: " + filename);
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Point;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }
            try
            {
                if (openedHub) hub = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Hub.unity", OpenSceneMode.Additive);
                PlayerController sourcePlayer = FindInScene<PlayerController>(hub);
                DontStarveCamera sourceCamera = FindInScene<DontStarveCamera>(hub);
                if (sourcePlayer == null || sourceCamera == null)
                    throw new InvalidOperationException("Hub player or camera not found.");
                GameObject playerPrefab = SaveTemplate(sourcePlayer.gameObject, "FinalBossPlayerFallback");
                GameObject cameraPrefab = SaveTemplate(sourceCamera.gameObject, "FinalBossCameraFallback");

                AuthoredMapLayout layout = LoadOrCreate<AuthoredMapLayout>(LayoutPath);
                layout.size = new Vector2Int(48, 38);
                layout.spawnCell = new Vector2Int(24, 9);
                layout.floorPolygon = new[] {
                    new Vector2(21, 2), new Vector2(27, 2), new Vector2(27, 7),
                    new Vector2(35, 7), new Vector2(40, 10), new Vector2(43, 15),
                    new Vector2(43, 24), new Vector2(41, 28), new Vector2(37, 31),
                    new Vector2(11, 31), new Vector2(7, 28), new Vector2(5, 24),
                    new Vector2(5, 15), new Vector2(8, 10), new Vector2(13, 7), new Vector2(21, 7)
                };
                layout.blockedAreas = PillarBounds.Concat(new[] { new RectInt(17, 30, 14, 5) }).ToArray();
                layout.environmentAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(ArtPath + "/CerebrumEnvironment_v2.png");
                if (layout.environmentAtlas == null) throw new InvalidOperationException("Environment atlas missing.");
                EditorUtility.SetDirty(layout);
                BiomeConfig config = LoadOrCreate<BiomeConfig>(ArtPath + "/FinalBossBiomeConfig.asset");
                config.biomeType = BiomeType.None;
                config.spawnWorldItems = false;
                config.GetMidBossArenaConfig().enabled = false;
                config.returnPortal.enabled = false; // The authored entrance supplies the return portal.
                EditorUtility.SetDirty(config);

                Texture2D artwork = AssetDatabase.LoadAssetAtPath<Texture2D>(ArtPath + "/CerebrumArena_Dormant.png");
                if (artwork == null) throw new InvalidOperationException("Dormant cerebrum artwork missing.");
                // Reuse source texture UVs as independent sprites. No flattened room mesh or bitmap rewrite.
                Sprite floor = SaveSprite("CerebrumFloorTile", artwork,
                    new Rect(.48f, .43f, .02f, .02f * artwork.width / artwork.height), 1f, new Vector2(.5f, .5f));
                Sprite wall = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/ProceduralMap/Tiles/폐벽면.png");
                if (wall == null) throw new InvalidOperationException("Shared organ wall tile missing.");

                arenaScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                SceneManager.SetActiveScene(arenaScene);
                GameObject gridObject = new GameObject("Grid", typeof(Grid));
                gridObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                Grid grid = gridObject.GetComponent<Grid>();
                Tilemap baseMap = MakeTilemap(gridObject.transform, "Base Tilemap");
                Tilemap grassMap = MakeTilemap(gridObject.transform, "Grass Tilemap");
                Tilemap upperMap = MakeTilemap(gridObject.transform, "Second Floor Tilemap");
                Tilemap cliffMap = MakeTilemap(gridObject.transform, "Cliff Tilemap");
                cliffMap.color = new Color(.72f, .56f, .76f, 1f);
                GameObject mapObject = new GameObject("Map Generator");
                MapGenerator map = mapObject.AddComponent<MapGenerator>();
                var generator = new SerializedObject(map);
                generator.FindProperty("baseTilemap").objectReferenceValue = baseMap;
                generator.FindProperty("grassTilemap").objectReferenceValue = grassMap;
                generator.FindProperty("secondFloorTilemap").objectReferenceValue = upperMap;
                generator.FindProperty("cliffTilemap").objectReferenceValue = cliffMap;
                generator.FindProperty("baseTile").objectReferenceValue = floor;
                generator.FindProperty("cliffTile").objectReferenceValue = wall;
                generator.FindProperty("authoredLayout").objectReferenceValue = layout;
                generator.FindProperty("mapWidth").intValue = layout.size.x;
                generator.FindProperty("mapHeight").intValue = layout.size.y;
                generator.FindProperty("bottomEmptyRows").intValue = 0;
                generator.FindProperty("generateGrass").boolValue = false;
                generator.FindProperty("secondFloorAreaCount").intValue = 0;
                generator.FindProperty("chunkSize").intValue = 16;
                generator.FindProperty("loadRadius").intValue = 2;
                generator.FindProperty("unloadRadius").intValue = 3;
                generator.ApplyModifiedPropertiesWithoutUndo();
                ProceduralBiomeBridge bridge = mapObject.AddComponent<ProceduralBiomeBridge>();
                var bridgeSettings = new SerializedObject(bridge);
                bridgeSettings.FindProperty("config").objectReferenceValue = config;
                bridgeSettings.FindProperty("grid").objectReferenceValue = grid;
                bridgeSettings.FindProperty("proceduralChunkSize").intValue = 16;
                bridgeSettings.ApplyModifiedPropertiesWithoutUndo();

                Transform props = new GameObject("Arena Props").transform;
                props.SetParent(mapObject.transform, false);
                string[] names = { "Stomach", "Intestine", "Liver", "Lung" };
                Rect[] regions = {
                    new Rect(.176f, .586f, .132f, .291f), new Rect(.686f, .579f, .14f, .285f),
                    new Rect(.182f, .217f, .153f, .266f), new Rect(.679f, .218f, .153f, .28f)
                };
                Vector2[][] contours = PillarContours;
                var pillarTransforms = new Transform[4];
                var phasePillars = new FinalBossPillar[4];
                BiomeType[] pillarBiomes = { BiomeType.Stomach, BiomeType.Intestine, BiomeType.Liver, BiomeType.Lung };
                for (int i = 0; i < 4; i++)
                {
                    Sprite sprite = SaveSprite("Pillar_" + names[i], artwork, regions[i], regions[i].width * 48f, new Vector2(.5f, 0));
                    RectInt bounds = PillarBounds[i];
                    pillarTransforms[i] = MakeProp(props, "Pillar_" + names[i], sprite,
                        new Vector3(regions[i].center.x * 48f, -2f, regions[i].y * 38f + 2f), contours[i]);
                    phasePillars[i] = pillarTransforms[i].gameObject.AddComponent<FinalBossPillar>();
                    phasePillars[i].Configure(pillarBiomes[i], PillarBounds[i],
                        pillarTransforms[i].GetComponent<SpriteRenderer>());
                }
                Sprite boss = SaveSprite("DormantCerebrum", artwork, new Rect(.351f, .749f, .281f, .25f), .281f * 48f, new Vector2(.5f,0));
                Transform bossTransform = MakeProp(props, "DormantCerebrum", boss,
                    new Vector3(.4915f * 48f, -2f, .749f * 38f + 2f), CerebrumOutline);

                FinalBossArena arenaComponent = mapObject.AddComponent<FinalBossArena>();
                var arena = new SerializedObject(arenaComponent);
                var pillars = arena.FindProperty("pillars");
                pillars.arraySize = 4;
                for (int i = 0; i < 4; i++) pillars.GetArrayElementAtIndex(i).objectReferenceValue = pillarTransforms[i];
                arena.ApplyModifiedPropertiesWithoutUndo();
                BiomeConfig[] sourceBiomes =
                {
                    AssetDatabase.LoadAssetAtPath<BiomeConfig>("Assets/_Project/Data/BiomeConfigs/StomachBiomeConfig.asset"),
                    AssetDatabase.LoadAssetAtPath<BiomeConfig>("Assets/_Project/Data/BiomeConfigs/IntestineBiomeConfig.asset"),
                    AssetDatabase.LoadAssetAtPath<BiomeConfig>("Assets/_Project/Data/BiomeConfigs/LiverBiomeConfig.asset"),
                    AssetDatabase.LoadAssetAtPath<BiomeConfig>("Assets/_Project/Data/BiomeConfigs/LungBiomeConfig.asset")
                };
                if (sourceBiomes.Any(value => value == null))
                    throw new InvalidOperationException("One or more source biome configs are missing.");
                mapObject.AddComponent<FinalBossPhaseOneController>()
                    .Configure(bossTransform, phasePillars, sourceBiomes);

                GameInitializer initializer = new GameObject("DirectSceneBootstrap").AddComponent<GameInitializer>();
                var bootstrap = new SerializedObject(initializer);
                bootstrap.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
                bootstrap.FindProperty("cameraPrefab").objectReferenceValue = cameraPrefab;
                bootstrap.ApplyModifiedPropertiesWithoutUndo();
                new GameObject("SceneLoader").AddComponent<SceneLoader>();
                EditorSceneManager.SaveScene(arenaScene, ScenePath);
                var scenes = EditorBuildSettings.scenes.ToList();
                int index = scenes.FindIndex(s => s.path == ScenePath);
                if (index < 0) scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
                else scenes[index] = new EditorBuildSettingsScene(ScenePath, true);
                EditorBuildSettings.scenes = scenes.ToArray();
                AssetDatabase.SaveAssets();
                Debug.Log("[FinalBossBuilder] Built shared Grid/Tilemap/MapGenerator/ProceduralBiomeBridge arena.");
            }
            finally
            {
                if (arenaScene.IsValid() && arenaScene.isLoaded) EditorSceneManager.CloseScene(arenaScene, true);
                if (openedHub && hub.isLoaded) EditorSceneManager.CloseScene(hub, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        [MenuItem("Tools/Necrocis/Final Boss/Update Existing Prop Outlines")]
        public static void UpdateExistingPropOutlines()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode first.");
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (!opened && scene.isDirty) throw new InvalidOperationException("Save the FinalBoss scene first.");
            try
            {
                if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                FinalBossArena arena = FindInScene<FinalBossArena>(scene);
                if (arena == null) throw new InvalidOperationException("FinalBossArena is missing.");
                string[] names = { "Pillar_Stomach", "Pillar_Intestine", "Pillar_Liver", "Pillar_Lung", "DormantCerebrum" };
                for (int i = 0; i < names.Length; i++)
                {
                    Transform prop = arena.transform.Find("Arena Props/" + names[i]);
                    if (prop == null) throw new InvalidOperationException("Missing prop: " + names[i]);
                    Sprite source = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "/" + names[i] + ".asset");
                    if (source == null) throw new InvalidOperationException("Missing sprite: " + names[i]);
                    SpriteOutline outline = prop.GetComponent<SpriteOutline>();
                    if (outline == null) outline = prop.gameObject.AddComponent<SpriteOutline>();
                    outline.Configure(source, i < 4 ? PillarContours[i] : CerebrumOutline);
                    EditorUtility.SetDirty(outline);
                }
                BiomeConfig config = arena.GetComponent<ProceduralBiomeBridge>().GetBiomeConfig();
                config.spawnWorldItems = false;
                EditorUtility.SetDirty(config);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                if (opened && scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) { asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); }
            return asset;
        }

        private static Tilemap MakeTilemap(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(parent, false);
            return go.GetComponent<Tilemap>();
        }

        private static Transform MakeProp(Transform parent, string name, Sprite sprite, Vector3 position, Vector2[] outline)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(45, 0, 0);
            go.AddComponent<SpriteRenderer>().sprite = sprite;
            go.AddComponent<SpriteOutline>().Configure(sprite, outline);
            var billboard = new SerializedObject(go.AddComponent<Billboard>());
            billboard.FindProperty("yOffset").floatValue = 0;
            billboard.ApplyModifiedPropertiesWithoutUndo();
            var sorting = new SerializedObject(go.AddComponent<SpriteYSort>());
            sorting.FindProperty("baseSortingOrder").intValue = SpriteYSort.WorldDynamicBaseSortingOrder;
            sorting.ApplyModifiedPropertiesWithoutUndo();
            return go.transform;
        }

        private static Sprite SaveSprite(string name, Texture2D texture, Rect uv, float width, Vector2 pivot)
        {
            Rect rect = new Rect(Mathf.Round(uv.x * texture.width), Mathf.Round(uv.y * texture.height),
                Mathf.Round(uv.width * texture.width), Mathf.Round(uv.height * texture.height));
            float ppu = rect.width / width;
            Sprite sprite = Sprite.Create(texture, rect, pivot, ppu, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            // Persist the texture region; SpriteOutline owns the mesh in scene data.
            string path = ArtPath + "/" + name + ".asset";
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing == null) { AssetDatabase.CreateAsset(sprite, path); return sprite; }
            EditorUtility.CopySerialized(sprite, existing);
            UnityEngine.Object.DestroyImmediate(sprite);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static T FindInScene<T>(Scene scene) where T : Component =>
            scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).FirstOrDefault();

        private static GameObject SaveTemplate(GameObject source, string name)
        {
            GameObject copy = UnityEngine.Object.Instantiate(source);
            try
            {
                copy.name = name;
                copy.transform.position = new Vector3(0, -2f, 0);
                DontStarveCamera camera = copy.GetComponent<DontStarveCamera>();
                if (camera != null) camera.SetTarget(null);
                return PrefabUtility.SaveAsPrefabAsset(copy, ArtPath + "/" + name + ".prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(copy); }
        }

        [MenuItem("Tools/Necrocis/Final Boss/Open Exploration Scene")]
        public static void Open()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
