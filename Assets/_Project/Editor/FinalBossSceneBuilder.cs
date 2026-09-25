using System;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NecrocisEditor
{
    public static class FinalBossSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/FinalBoss.unity";
        private const string ArtPath = "Assets/_Project/Art/Generated/FinalBoss";

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
                AssetDatabase.Refresh();
                var importer = (TextureImporter)AssetImporter.GetAtPath(ArtPath + "/CerebrumArena_Dormant.png");
                importer.textureType = TextureImporterType.Default;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(importer.assetPath);
                string materialPath = ArtPath + "/CerebrumArena.mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    material = new Material(Shader.Find("Unlit/Texture"));
                    AssetDatabase.CreateAsset(material, materialPath);
                }
                material.mainTexture = texture;
                EditorUtility.SetDirty(material);

                // Reuse the actual Hub player/camera settings, including every class's animation.
                PlayerController sourcePlayer = FindInScene<PlayerController>(hub);
                DontStarveCamera sourceCamera = FindInScene<DontStarveCamera>(hub);
                if (sourcePlayer == null || sourceCamera == null)
                    throw new InvalidOperationException("Hub player or camera not found.");
                GameObject playerPrefab = SaveTemplate(sourcePlayer.gameObject, "FinalBossPlayerFallback");
                GameObject cameraPrefab = SaveTemplate(sourceCamera.gameObject, "FinalBossCameraFallback");
                arenaScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                SceneManager.SetActiveScene(arenaScene);
                var arena = new GameObject("FinalBossArena_48x38").AddComponent<FinalBossArena>();
                arena.BuildLayout(material);
                GameInitializer initializer = new GameObject("DirectSceneBootstrap").AddComponent<GameInitializer>();
                var serialized = new SerializedObject(initializer);
                serialized.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
                serialized.FindProperty("cameraPrefab").objectReferenceValue = cameraPrefab;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                new GameObject("SceneLoader").AddComponent<SceneLoader>();
                EditorSceneManager.SaveScene(arenaScene, ScenePath);
                var scenes = EditorBuildSettings.scenes.ToList();
                int index = scenes.FindIndex(s => s.path == ScenePath);
                if (index < 0) scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
                else scenes[index] = new EditorBuildSettingsScene(ScenePath, true);
                EditorBuildSettings.scenes = scenes.ToArray();
                AssetDatabase.SaveAssets();
                Debug.Log("[FinalBossBuilder] Built 48x38 exploration scene, four inert pillars, dormant boss, return entrance.");
            }
            finally
            {
                if (arenaScene.IsValid() && arenaScene.isLoaded) EditorSceneManager.CloseScene(arenaScene, true);
                if (openedHub && hub.isLoaded) EditorSceneManager.CloseScene(hub, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).FirstOrDefault();
        }

        private static GameObject SaveTemplate(GameObject source, string name)
        {
            GameObject copy = UnityEngine.Object.Instantiate(source);
            try
            {
                copy.name = name;
                copy.transform.position = Vector3.zero;
                // A fallback camera resolves the active player at runtime, never a Hub scene reference.
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
