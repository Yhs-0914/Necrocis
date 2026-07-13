using System;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NecrocisEditor
{
    public static class MainMenuSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string RevealedBackgroundPath = "Assets/_Project/Art/UI/MainMenu/MainMenuBackgroundRevealed.png";
        private const string LockedBackgroundPath = "Assets/_Project/Art/UI/MainMenu/MainMenuBackground.png";
        private const string PlayerPath = "Assets/_Project/Art/Images/Player/Basics/대기/기본 IDEL.png";
        private const string FontPath = "Assets/_Project/Art/Fonts/PFStardust.ttf";
        private const string IntestineSilhouettePath = "Assets/_Project/Art/UI/MainMenu/Silhouettes/IntestineSilhouette.png";
        private const string LiverSilhouettePath = "Assets/_Project/Art/UI/MainMenu/Silhouettes/LiverSilhouette.png";
        private const string StomachSilhouettePath = "Assets/_Project/Art/UI/MainMenu/Silhouettes/StomachSilhouette.png";
        private const string LungSilhouettePath = "Assets/_Project/Art/UI/MainMenu/Silhouettes/LungSilhouette.png";

        [MenuItem("Necrocis/Build Main Menu Scene")]
        public static void Build()
        {
            ConfigureArtworkImporters();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraObject = new GameObject("MainMenuCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.cullingMask = 0;
            camera.orthographic = true;
            cameraObject.AddComponent<AudioListener>();

            GameObject root = new GameObject("MainMenu");
            MainMenuController controller = root.AddComponent<MainMenuController>();

            SerializedObject serializedController = new SerializedObject(controller);
            SetObject(serializedController, "backgroundArtwork", AssetDatabase.LoadAssetAtPath<Sprite>(RevealedBackgroundPath));
            SetObject(serializedController, "lockedBackgroundArtwork", AssetDatabase.LoadAssetAtPath<Sprite>(LockedBackgroundPath));
            SetObject(serializedController, "playerSprite", LoadFirstSprite(PlayerPath));
            SetObject(serializedController, "intestineSilhouette", AssetDatabase.LoadAssetAtPath<Sprite>(IntestineSilhouettePath));
            SetObject(serializedController, "liverSilhouette", AssetDatabase.LoadAssetAtPath<Sprite>(LiverSilhouettePath));
            SetObject(serializedController, "stomachSilhouette", AssetDatabase.LoadAssetAtPath<Sprite>(StomachSilhouettePath));
            SetObject(serializedController, "lungSilhouette", AssetDatabase.LoadAssetAtPath<Sprite>(LungSilhouettePath));
            SetObject(serializedController, "menuFont", AssetDatabase.LoadAssetAtPath<Font>(FontPath));
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterFirstBuildScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ValidateOrThrow();
            Debug.Log("[MainMenuSceneBuilder] MainMenu 씬 생성 및 Build Settings 등록 완료");
        }

        public static void ValidateOrThrow()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            MainMenuController controller = UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
            if (controller == null)
            {
                throw new InvalidOperationException("MainMenuController is missing from the MainMenu scene.");
            }

            SerializedObject serializedController = new SerializedObject(controller);
            string[] requiredReferences =
            {
                "backgroundArtwork",
                "lockedBackgroundArtwork",
                "playerSprite",
                "intestineSilhouette",
                "liverSilhouette",
                "stomachSilhouette",
                "lungSilhouette",
                "menuFont"
            };

            foreach (string propertyName in requiredReferences)
            {
                SerializedProperty property = serializedController.FindProperty(propertyName);
                if (property == null || property.objectReferenceValue == null)
                {
                    throw new InvalidOperationException($"MainMenu reference '{propertyName}' is missing.");
                }
            }

            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            if (buildScenes.Length == 0 || !buildScenes[0].enabled || buildScenes[0].path != ScenePath)
            {
                throw new InvalidOperationException("MainMenu is not the first enabled Build Settings scene.");
            }

            if (!scene.IsValid())
            {
                throw new InvalidOperationException("MainMenu scene could not be opened for validation.");
            }
        }

        private static void ConfigureArtworkImporters()
        {
            string[] artworkPaths =
            {
                RevealedBackgroundPath,
                LockedBackgroundPath,
                IntestineSilhouettePath,
                LiverSilhouettePath,
                StomachSilhouettePath,
                LungSilhouettePath
            };

            foreach (string path in artworkPaths)
            {
                ConfigureArtworkImporter(path);
            }
        }

        private static void ConfigureArtworkImporter(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Could not import main-menu artwork at {path}");
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        private static Sprite LoadFirstSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .OrderBy(asset => asset.name, StringComparer.Ordinal)
                .FirstOrDefault();

            if (sprite == null)
            {
                throw new InvalidOperationException($"No Sprite sub-asset was found at {path}");
            }

            return sprite;
        }

        private static void SetObject(SerializedObject target, string propertyName, UnityEngine.Object value)
        {
            if (value == null)
            {
                throw new InvalidOperationException($"Could not load asset for '{propertyName}'.");
            }

            SerializedProperty property = target.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"Serialized property '{propertyName}' was not found.");
            }

            property.objectReferenceValue = value;
        }

        private static void RegisterFirstBuildScene()
        {
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            EditorBuildSettingsScene[] updated = new[] { new EditorBuildSettingsScene(ScenePath, true) }
                .Concat(existing.Where(scene => scene.path != ScenePath))
                .ToArray();
            EditorBuildSettings.scenes = updated;
        }
    }
}
