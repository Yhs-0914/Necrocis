using System;
using ProceduralMap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Necrocis.EditorTools
{
    /// <summary>
    /// Map_Final에서 가져온 장기 씬을 Necrocis의 XZ 월드 구조에 맞게 정리합니다.
    /// Hub에서 유지되는 실제 플레이어와 카메라를 사용하므로 임시 오브젝트는 제거합니다.
    /// </summary>
    public static class ProceduralMapIntegrationTool
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/_Project/Scenes/Liver.unity",
            "Assets/_Project/Scenes/Stomach.unity",
            "Assets/_Project/Scenes/Lung.unity",
            "Assets/_Project/Scenes/Intestine.unity"
        };

        [MenuItem("Tools/Necrocis/Integrate Procedural Organ Scenes")]
        public static void IntegrateOrganScenes()
        {
            foreach (string scenePath in ScenePaths)
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                RemoveSceneOnlyObject(scene, "Basic Player");
                RemoveSceneOnlyObject(scene, "Main Camera");

                GameObject grid = FindRoot(scene, "Grid");
                if (grid == null)
                {
                    throw new InvalidOperationException($"{scenePath}: Grid 오브젝트를 찾지 못했습니다.");
                }

                grid.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(90f, 0f, 0f));
                grid.transform.localScale = Vector3.one;

                MapGenerator generator = UnityEngine.Object.FindFirstObjectByType<MapGenerator>();
                if (generator == null || generator.gameObject.scene != scene)
                {
                    throw new InvalidOperationException($"{scenePath}: MapGenerator를 찾지 못했습니다.");
                }

                SerializedObject serializedGenerator = new SerializedObject(generator);
                SetBoolean(serializedGenerator, "useXZWorld", true);
                SetBoolean(serializedGenerator, "generateOnStart", true);
                serializedGenerator.ApplyModifiedPropertiesWithoutUndo();

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[ProceduralMapIntegration] 변환 완료: {scenePath}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[ProceduralMapIntegration] 모든 장기 씬 변환이 완료되었습니다.");
        }

        public static void IntegrateOrganScenesBatch()
        {
            try
            {
                IntegrateOrganScenes();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void SetBoolean(SerializedObject target, string propertyName, bool value)
        {
            SerializedProperty property = target.FindProperty(propertyName);
            if (property != null) property.boolValue = value;
        }

        private static void RemoveSceneOnlyObject(Scene scene, string objectName)
        {
            GameObject target = FindInScene(scene, objectName);
            if (target != null) UnityEngine.Object.DestroyImmediate(target);
        }

        private static GameObject FindInScene(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
                foreach (Transform candidate in transforms)
                {
                    if (string.Equals(candidate.name, objectName, StringComparison.Ordinal)
                        || candidate.name.StartsWith(objectName + " (", StringComparison.Ordinal))
                    {
                        return candidate.gameObject;
                    }
                }
            }

            return null;
        }

        private static GameObject FindRoot(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (string.Equals(root.name, objectName, StringComparison.Ordinal)) return root;
            }

            return null;
        }
    }
}
