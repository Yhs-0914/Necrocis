using System;
using System.IO;
using System.Reflection;
using Necrocis;
using ProceduralMap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NecrocisEditor
{
    public static class LungHazardSmokeRunner
    {
        private static int phase;
        private static double deadline;
        private static float started;
        private static Vector3 origin, center;
        private static LungBiomeHazardController hazard;
        private static readonly BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;

        public static void Run()
        {
            if (!Application.isBatchMode) throw new Exception("Run in an isolated batch project.");
            SaveService.UseStorageRootForTests(Path.GetFullPath("Temp/LungHazardSave"));
            if (!SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error)) throw new Exception(error);
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Hub.unity");
            deadline = EditorApplication.timeSinceStartup + 150;
            EditorApplication.update += Tick;
            EditorApplication.isPlaying = true;
        }

        private static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Timeout phase " + phase);
                if (!EditorApplication.isPlaying || PlayerController.Instance == null) return;
                Time.timeScale = 1;
                var player = PlayerController.Instance;
                player.HealthComponent?.ResetHealth();
                if (phase == 0)
                {
                    if (SceneLoader.Instance == null) return;
                    SceneLoader.Instance.LoadBiome(BiomeType.Lung);
                    phase = 1;
                    return;
                }
                if (phase == 1)
                {
                    hazard = UnityEngine.Object.FindFirstObjectByType<LungBiomeHazardController>();
                    if (hazard == null) return;
                    var map = hazard.GetComponent<MapGenerator>();
                    Require(map != null && map.IsReady, "Hazard is not attached to the procedural map");
                    Require(UnityEngine.Object.FindObjectsByType<LungBiomeHazardController>(FindObjectsSortMode.None).Length == 1, "Duplicate hazards");
                    center = map.GetCellCenterWorld(map.MapWidth / 2, map.MapHeight / 2);
                    bool foundSand = false, foundCovered = false;
                    for (int y = 10; y < map.MapHeight - 10; y++)
                    for (int x = 10; x < map.MapWidth - 10; x++)
                    {
                        var cell = map.Data.GetCell(x, y);
                        Vector3 point = map.GetCellCenterWorld(x, y);
                        if (cell.HasGrass && !cell.IsVoid && cell.HeightLevel == 0)
                        {
                            Require(!map.IsExposedLungSand(point), "Grass triggers sandstorm");
                            foundCovered = true;
                        }
                        if (foundSand || !map.IsExposedLungSand(point) || Vector3.Distance(point, center) < 30) continue;
                        bool clear = true;
                        for (int dy = -5; dy <= 5; dy++)
                        for (int dx = -5; dx <= 5; dx++)
                        {
                            var neighbor = map.Data.GetCell(x + dx, y + dy);
                            if (neighbor.IsVoid || neighbor.HasCliff || neighbor.HeightLevel != 0 || neighbor.Occupied) clear = false;
                        }
                        if (!clear) continue;
                        origin = point;
                        foundSand = true;
                    }
                    Require(foundSand && foundCovered, "Missing sand/grass test locations");
                    player.SpawnAt(origin);
                    hazard.StopAllCoroutines();
                    SetWind("Exhale");
                    started = Time.time;
                    phase = 2;
                }
                else if (phase == 2 && Time.time - started > 1.2f)
                {
                    float outward = Vector3.Dot(player.transform.position - origin, (origin - center).normalized);
                    Require(outward > 0.5f, "Exhale did not push outward: " + outward);
                    var mask = (CanvasGroup)typeof(LungBiomeHazardController).GetField("visionCanvasGroup", Hidden).GetValue(hazard);
                    Require(mask.alpha > 0.5f, "Sand mask did not appear");
                    Debug.Log("LUNG TEST exhale displacement=" + outward + ", mask=" + mask.alpha);
                    origin = player.transform.position;
                    SetWind("Inhale");
                    started = Time.time;
                    phase = 3;
                }
                else if (phase == 3 && Time.time - started > 1.2f)
                {
                    float inward = Vector3.Dot(player.transform.position - origin, (center - origin).normalized);
                    Require(inward > 0.5f, "Inhale did not pull inward: " + inward);
                    Debug.Log("LUNG TEST inhale displacement=" + inward);
                    SceneLoader.Instance.ReturnToHub();
                    phase = 4;
                }
                else if (phase == 4 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Hub")
                {
                    Require(UnityEngine.Object.FindFirstObjectByType<LungBiomeHazardController>() == null, "Hazard leaked into hub");
                    Finish(true, "Automatic lung hookup, exhale, inhale, sand mask, grass exclusion and hub cleanup passed.");
                }
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private static void SetWind(string name)
        {
            var field = typeof(LungBiomeHazardController).GetField("windState", Hidden);
            field.SetValue(hazard, Enum.Parse(field.FieldType, name));
        }
        private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        private static void Finish(bool success, string message)
        {
            EditorApplication.update -= Tick;
            Directory.CreateDirectory("Temp");
            File.WriteAllText("Temp/lung-hazard-result.txt", (success ? "PASS " : "FAIL ") + message);
            Debug.Log(message);
            EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
