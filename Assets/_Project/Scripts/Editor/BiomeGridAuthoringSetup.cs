#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Necrocis.EditorTools
{
    [InitializeOnLoad]
    public static class BiomeGridAuthoringSetup
    {
        private const int MapSize = 300;
        private const string SetupKey = "Necrocis.BiomeGridAuthoringSetup.v1";
        private static readonly string[] ScenePaths =
        {
            "Assets/_Project/Scenes/Lung.unity",
            "Assets/_Project/Scenes/Stomach.unity",
            "Assets/_Project/Scenes/Intestine.unity"
        };

        static BiomeGridAuthoringSetup()
        {
            EditorApplication.delayCall += SetupOtherScenesOnce;
        }

        [MenuItem("Necrocis/Setup/Add 300x300 Authoring Layers To Other Scenes")]
        public static void AddLayersToOtherScenes()
        {
            foreach (string scenePath in ScenePaths)
            {
                AddLayersToScene(scenePath);
            }
            EditorPrefs.SetBool(SetupKey, true);
        }

        [MenuItem("Necrocis/Setup/Add 300x300 Authoring Layers To Open Scene")]
        public static void AddLayersToOpenScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            Grid grid = Object.FindFirstObjectByType<Grid>();
            if (grid == null)
            {
                Debug.LogError("[BiomeGridAuthoringSetup] Open scene has no Grid.");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(grid.gameObject, "Add Biome Authoring Layers");

            GetOrCreateTilemap(grid.transform, "Floor", 0);
            GetOrCreateTilemap(grid.transform, "Ground_Overlay", 10);
            GetOrCreateTilemap(grid.transform, "Height", 20);
            GetOrCreateTilemap(grid.transform, "Cliff", 30);
            GetOrCreateTilemap(grid.transform, "Blocker", 40);
            GetOrCreateTilemap(grid.transform, "object", 50);

            EditorUtility.SetDirty(grid.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = grid.gameObject;
            SceneView.RepaintAll();
            Debug.Log($"[BiomeGridAuthoringSetup] {scene.name}: kept existing tiles and added 300x300 authoring layers.");
        }

        private static void SetupOtherScenesOnce()
        {
            if (EditorPrefs.GetBool(SetupKey, false) || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            AddLayersToOtherScenes();
        }

        private static void AddLayersToScene(string scenePath)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool wasLoaded = scene.isLoaded;
            if (!wasLoaded)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            Grid grid = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                grid = root.GetComponentInChildren<Grid>(true);
                if (grid != null) break;
            }

            if (grid != null)
            {
                GetOrCreateTilemap(grid.transform, "Floor", 0);
                GetOrCreateTilemap(grid.transform, "Ground_Overlay", 10);
                GetOrCreateTilemap(grid.transform, "Height", 20);
                GetOrCreateTilemap(grid.transform, "Cliff", 30);
                GetOrCreateTilemap(grid.transform, "Blocker", 40);
                GetOrCreateTilemap(grid.transform, "object", 50);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!wasLoaded)
                {
                    EditorSceneManager.SaveScene(scene);
                }
            }

            if (!wasLoaded)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Tilemap GetOrCreateTilemap(Transform grid, string name, int sortingOrder)
        {
            Transform child = grid.Find(name);
            if (child == null)
            {
                GameObject obj = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(obj, "Create Tilemap Layer");
                child = obj.transform;
                child.SetParent(grid, false);
            }

            Tilemap tilemap = child.GetComponent<Tilemap>();
            if (tilemap == null) tilemap = Undo.AddComponent<Tilemap>(child.gameObject);

            TilemapRenderer renderer = child.GetComponent<TilemapRenderer>();
            if (renderer == null) renderer = Undo.AddComponent<TilemapRenderer>(child.gameObject);
            renderer.sortingOrder = sortingOrder;
            return tilemap;
        }

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        private static void DrawGridBounds(Grid grid, GizmoType gizmoType)
        {
            Vector3 size = new Vector3(MapSize * grid.cellSize.x, MapSize * grid.cellSize.y, 0f);
            Vector3 center = grid.transform.position;
            Gizmos.color = new Color(0f, 1f, 1f, 0.9f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
#endif
