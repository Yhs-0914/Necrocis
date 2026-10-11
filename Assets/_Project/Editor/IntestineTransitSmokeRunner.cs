using System;
using System.Reflection;
using Necrocis;
using ProceduralMap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NecrocisEditor
{
    public static class IntestineTransitSmokeRunner
    {
        // Batch-only: opening a scene must not disturb an interactive editor's unsaved work.
        public static void Run()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Run this check with Unity -batchmode.");
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Intestine.unity");
            MapGenerator map = UnityEngine.Object.FindFirstObjectByType<MapGenerator>();
            Type hazardType = typeof(PlayerController).Assembly.GetType("Necrocis.IntestineTransitHazard", true);
            Component hazard = map.GetComponent(hazardType);
            Require(hazard != null, "Hazard is attached to the intestine scene");
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Sprite sprite = (Sprite)hazardType.GetField("bolusSprite", flags).GetValue(hazard);
            Require(sprite != null && sprite.texture != null, "Generated sprite imports and resolves");
            Require(hazardType.GetField("ground", flags).GetValue(hazard) != null, "Ground tilemap resolves");
            hazardType.GetField("map", flags).SetValue(hazard, map);
            Require((float)hazardType.GetField("laneWidth", flags).GetValue(hazard) >= 2.6f,
                "Scene uses the wider warning and collision strip");
            VerifyTransitTiming(hazardType, hazard);
            map.GenerateMap();
            MethodInfo choose = hazardType.GetMethod("TryChooseLane", flags);
            MethodInfo safe = hazardType.GetMethod("SafeCrossSection", flags);
            Require(!(bool)safe.Invoke(hazard, new object[] { -1, -1, 0, false, 3 }), "Out-of-map lanes are rejected");
            Require(!(bool)choose.Invoke(hazard, new object[] { map.GetCellCenterWorld(-1, -1) }),
                "Out-of-map player positions are rejected");
            int candidates = 0;
            MethodInfo directional = hazardType.GetMethod("TryChooseDirectionalLane", flags);
            MethodInfo overlap = hazardType.GetMethod("OverlapsLane", flags);
            for (int heading = 0; heading < 4; heading++)
            {
            int headingCandidates = 0;
            for (int z = 12; z < map.MapHeight - 12; z += 12)
                for (int x = 12; x < map.MapWidth - 12; x += 12)
                {
                    if (!(bool)directional.Invoke(hazard, new object[] { map.GetCellCenterWorld(x, z), heading })) continue;
                    candidates++;
                    headingCandidates++;
                    Vector3 start = (Vector3)hazardType.GetField("start", flags).GetValue(hazard);
                    Vector3 end = (Vector3)hazardType.GetField("end", flags).GetValue(hazard);
                    bool vertical = heading >= 2;
                    Vector3 step = map.GetCellCenterWorld(vertical ? 0 : 1, vertical ? 1 : 0) - map.GetCellCenterWorld(0, 0);
                    float expectedLength = step.magnitude * (vertical ? map.MapHeight : map.MapWidth);
                    Vector3 dir = step.normalized * ((heading & 1) == 0 ? 1 : -1);
                    Require(Vector3.Distance(end - start, dir * expectedLength) < 0.01f,
                        "Lane spans the map in requested direction " + heading);
                    Vector3 perpendicular = new Vector3(-dir.z, 0, dir.x);
                    Require((bool)overlap.Invoke(hazard, new object[] { new Bounds(start + dir * 5, Vector3.one * 0.2f), 4f, 6f }),
                        "Collision detects a segment in direction " + heading);
                    Require(!(bool)overlap.Invoke(hazard, new object[] { new Bounds(start + dir * 5 + perpendicular * 4, Vector3.one * 0.2f), 4f, 6f }),
                        "Collision excludes positions outside warning in direction " + heading);
                }
            Require(headingCandidates > 0, "Usable lanes for direction " + heading);
            }
            Require(candidates > 0, "Generated map contains usable lanes");
            Debug.Log($"[IntestineTransitSmoke] PASS: sprite, scene wiring, bounds, direction; {candidates} usable sample positions.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("[IntestineTransitSmoke] FAIL: " + message);
        }

        private static void VerifyTransitTiming(Type type, Component hazard)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo warning = type.GetField("warningDuration", flags);
            float originalWarning = (float)warning.GetValue(hazard);
            try
            {
                foreach (float seconds in new[] { 3f, 4f, 5f })
                foreach (float distance in new[] { 30f, 300f, 1000f })
                {
                    warning.SetValue(hazard, seconds);
                    type.GetField("length", flags).SetValue(hazard, distance);
                    type.GetMethod("ConfigureTransit", flags).Invoke(hazard, null);
                    float velocity = (float)type.GetField("transitSpeed", flags).GetValue(hazard);
                    int count = (int)type.GetField("transitCarriageCount", flags).GetValue(hazard);
                    float spacing = (float)type.GetProperty("CarSpacing", flags).GetValue(hazard);
                    float halfLength = (float)type.GetProperty("HalfLength", flags).GetValue(hazard);
                    float deadline = (float)type.GetField("latestArrival", flags).GetValue(hazard);
                    float duration = (float)type.GetField("passageDuration", flags).GetValue(hazard);
                    Require(seconds + distance / velocity <= deadline + 0.001f,
                        "Front arrives by deadline including warning");
                    Require(((count - 1) * spacing + halfLength * 2) / velocity >= duration - 0.001f,
                        "Train passes continuously for the configured duration");
                    Require(spacing < halfLength * 2, "Streamed segments have no gaps");
                }
            }
            finally { warning.SetValue(hazard, originalWarning); }
        }
    }
}
