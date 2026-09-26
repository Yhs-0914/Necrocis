using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Temporary local verification driver. Removed after the migration is verified.
[InitializeOnLoad]
public static class FinalBossTaskBridge
{
    static FinalBossTaskBridge() { EditorApplication.update += Tick; }
    private static void Tick()
    {
        const string request = "Library/FinalBossTask.request";
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(request)) return;
        string command = File.ReadAllText(request).Trim();
        File.Delete(request);
        try
        {
            if (command == "status")
            {
                File.WriteAllText("Library/FinalBossTask.result", "play=" + EditorApplication.isPlaying + "; scene=" + SceneManager.GetActiveScene().path + "; dirty=" + SceneManager.GetActiveScene().isDirty);
                return;
            }
            if (command == "refresh")
            {
                AssetDatabase.Refresh();
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("User Play Mode is active.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new Exception("Unsaved scene: " + SceneManager.GetSceneAt(i).path);
            if (command == "build")
            {
                SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
                try
                {
                    if (SceneManager.GetSceneByPath(NecrocisEditor.FinalBossSceneBuilder.ScenePath).isLoaded)
                        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    NecrocisEditor.FinalBossSceneBuilder.Build();
                }
                finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            }
            else if (command == "regression")
            {
                foreach (string name in new[] { "Stomach", "Intestine", "Liver", "Lung" })
                {
                    string path = "Assets/_Project/Scenes/" + name + ".unity";
                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    try
                    {
                        ProceduralMap.MapGenerator map = null;
                        foreach (GameObject root in scene.GetRootGameObjects())
                            if (root.GetComponentInChildren<ProceduralMap.MapGenerator>() is ProceduralMap.MapGenerator candidate) map = candidate;
                        if (map == null || map.AuthoredLayout != null) throw new Exception(name + " generator changed unexpectedly");
                        map.GenerateMap();
                        Vector2Int spawn = map.WorldToCell(map.GetPlayerSpawnWorldPosition());
                        if (!map.IsReady || map.MapWidth != 300 || map.MapHeight != 300 || !map.IsCellWalkable(spawn.x, spawn.y))
                            throw new Exception(name + " generation/spawn regression");
                        map.ClearMap();
                    }
                    finally { EditorSceneManager.CloseScene(scene, true); }
                }
            }
            else if (command == "smoke") NecrocisEditor.FinalBossSmokeRunner.Run();
            else if (command == "direct") NecrocisEditor.FinalBossSmokeRunner.RunDirect();
            else throw new Exception("Unknown command: " + command);
            File.WriteAllText("Library/FinalBossTask.result", "OK " + command);
        }
        catch (Exception e) { File.WriteAllText("Library/FinalBossTask.result", e.ToString()); }
    }
}
