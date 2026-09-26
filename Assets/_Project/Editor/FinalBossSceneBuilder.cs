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
            new RectInt(10, 25, 5, 4), new RectInt(33, 25, 5, 4),
            new RectInt(10, 12, 5, 4), new RectInt(33, 12, 5, 4)
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
                    new Vector2(38, 7), new Vector2(44, 13), new Vector2(44, 29),
                    new Vector2(38, 35), new Vector2(10, 35), new Vector2(4, 29),
                    new Vector2(4, 13), new Vector2(10, 7), new Vector2(21, 7)
                };
                layout.blockedAreas = PillarBounds.Concat(new[] { new RectInt(18, 31, 12, 4) }).ToArray();
                EditorUtility.SetDirty(layout);
                BiomeConfig config = LoadOrCreate<BiomeConfig>(ArtPath + "/FinalBossBiomeConfig.asset");
                config.biomeType = BiomeType.None;
                config.GetMidBossArenaConfig().enabled = false;
                config.returnPortal.enabled = false; // The authored entrance supplies the return portal.
                EditorUtility.SetDirty(config);

                Texture2D artwork = AssetDatabase.LoadAssetAtPath<Texture2D>(ArtPath + "/CerebrumArena_Dormant.png");
                if (artwork == null) throw new InvalidOperationException("Dormant cerebrum artwork missing.");
                // Reuse source texture UVs as independent sprites. No flattened room mesh or bitmap rewrite.
                Sprite floor = SaveSprite("CerebrumFloorTile", artwork,
                    new Rect(.48f, .43f, .02f, .02f * artwork.width / artwork.height), 1f, null, new Vector2(.5f, .5f));
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
                Vector2[][] contours = {
                    new[] { new Vector2(.48f,0), new Vector2(.03f,.17f), new Vector2(.22f,.32f),
                        new Vector2(.22f,.59f), new Vector2(.48f,.77f), new Vector2(.58f,1),
                        new Vector2(.75f,1), new Vector2(.79f,.77f), new Vector2(.94f,.60f),
                        new Vector2(.88f,.28f), new Vector2(1,.12f) },
                    new[] { new Vector2(.5f,0), new Vector2(0,.17f), new Vector2(.20f,.36f),
                        new Vector2(.19f,.72f), new Vector2(.35f,1), new Vector2(.62f,.99f),
                        new Vector2(.82f,.79f), new Vector2(.87f,.36f), new Vector2(1,.17f) },
                    new[] { new Vector2(.5f,0), new Vector2(.04f,.13f), new Vector2(.19f,.36f),
                        new Vector2(.13f,.71f), new Vector2(.38f,.94f), new Vector2(.86f,1),
                        new Vector2(.95f,.87f), new Vector2(.86f,.47f), new Vector2(1,.16f) },
                    new[] { new Vector2(.5f,0), new Vector2(.05f,.13f), new Vector2(.18f,.46f),
                        new Vector2(.34f,.71f), new Vector2(.47f,.77f), new Vector2(.48f,1),
                        new Vector2(.60f,1), new Vector2(.64f,.78f), new Vector2(.83f,.61f),
                        new Vector2(.90f,.33f), new Vector2(1,.15f) }
                };
                var pillarTransforms = new Transform[4];
                for (int i = 0; i < 4; i++)
                {
                    Sprite sprite = SaveSprite("Pillar_" + names[i], artwork, regions[i], 6f, contours[i], new Vector2(.5f, 0));
                    RectInt bounds = PillarBounds[i];
                    pillarTransforms[i] = MakeProp(props, "Pillar_" + names[i], sprite,
                        new Vector3(bounds.center.x, -2f, bounds.center.y));
                }
                Sprite boss = SaveSprite("DormantCerebrum", artwork, new Rect(.351f, .749f, .281f, .25f), 13f,
                    new[] { new Vector2(.5f,0), new Vector2(.07f,.08f), new Vector2(0,.34f),
                        new Vector2(.17f,.75f), new Vector2(.32f,1), new Vector2(.70f,1),
                        new Vector2(.90f,.77f), new Vector2(1,.34f), new Vector2(.93f,.09f) }, new Vector2(.5f,0));
                MakeProp(props, "DormantCerebrum", boss, new Vector3(24f, -2f, 33f));

                GameObject exit = new GameObject("ReturnToHub_Entrance");
                exit.transform.SetParent(mapObject.transform, false);
                exit.transform.position = new Vector3(24.5f, -2f, 4.5f);
                BoxCollider trigger = exit.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.center = Vector3.up;
                trigger.size = new Vector3(4f, 6f, 1.5f);
                exit.AddComponent<ReturnPortal>().SetActive(true);
                var arena = new SerializedObject(mapObject.AddComponent<FinalBossArena>());
                arena.FindProperty("returnEntrance").objectReferenceValue = exit.transform;
                var pillars = arena.FindProperty("pillars");
                pillars.arraySize = 4;
                for (int i = 0; i < 4; i++) pillars.GetArrayElementAtIndex(i).objectReferenceValue = pillarTransforms[i];
                arena.ApplyModifiedPropertiesWithoutUndo();

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

        private static Transform MakeProp(Transform parent, string name, Sprite sprite, Vector3 position)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(45, 0, 0);
            go.AddComponent<SpriteRenderer>().sprite = sprite;
            var billboard = new SerializedObject(go.AddComponent<Billboard>());
            billboard.FindProperty("yOffset").floatValue = 0;
            billboard.ApplyModifiedPropertiesWithoutUndo();
            var sorting = new SerializedObject(go.AddComponent<SpriteYSort>());
            sorting.FindProperty("baseSortingOrder").intValue = SpriteYSort.WorldDynamicBaseSortingOrder;
            sorting.ApplyModifiedPropertiesWithoutUndo();
            return go.transform;
        }

        private static Sprite SaveSprite(string name, Texture2D texture, Rect uv, float width, Vector2[] outline, Vector2 pivot)
        {
            Rect rect = new Rect(Mathf.Round(uv.x * texture.width), Mathf.Round(uv.y * texture.height),
                Mathf.Round(uv.width * texture.width), Mathf.Round(uv.height * texture.height));
            float ppu = rect.width / width;
            Sprite sprite = Sprite.Create(texture, rect, pivot, ppu, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            if (outline != null)
            {
                var vertices = new Vector2[outline.Length + 1];
                vertices[0] = new Vector2((.5f - pivot.x) * rect.width / ppu, (.5f - pivot.y) * rect.height / ppu);
                var triangles = new ushort[outline.Length * 3];
                for (int i = 0; i < outline.Length; i++)
                {
                    vertices[i + 1] = new Vector2((outline[i].x - pivot.x) * rect.width / ppu,
                        (outline[i].y - pivot.y) * rect.height / ppu);
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = (ushort)(i + 1);
                    triangles[i * 3 + 2] = (ushort)((i + 1) % outline.Length + 1);
                }
                sprite.OverrideGeometry(vertices, triangles);
            }
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
