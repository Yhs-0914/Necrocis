using System;
using System.Collections.Generic;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static class BossArenaBoundarySmokeRunner
    {
        private static readonly string[] BiomeNames = { "Intestine", "Liver", "Stomach", "Lung" };
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [MenuItem("Tools/Necrocis/Verify Boss Arena Boundaries")]
        public static void Run()
        {
            for (int i = 0; i < BiomeNames.Length; i++)
            {
                VerifyBiome(BiomeNames[i]);
            }

            Debug.Log("[BossArenaBoundarySmoke] PASS - grid wall, trigger, door and locked movement verified");
        }

        public static void RunBatch()
        {
            try
            {
                Run();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void VerifyBiome(string biomeName)
        {
            BossArenaConfig configAsset = AssetDatabase.LoadAssetAtPath<BossArenaConfig>(
                $"Assets/_Project/Data/BiomeConfigs/{biomeName}BossArenaConfig.asset");
            Require(configAsset != null, $"{biomeName}: config asset missing");

            MidBossArenaConfig config = configAsset.midBossArena;
            GameObject biomeObject = new GameObject($"{biomeName}_BoundarySmokeBiome");
            BossArenaWallPreviewBiome biome = biomeObject.AddComponent<BossArenaWallPreviewBiome>();
            biome.ConfigureForPreview(config.boss.patternType);

            GameObject arenaObject = new GameObject($"{biomeName}_BoundarySmokeArena");
            MidBossArenaController arena = arenaObject.AddComponent<MidBossArenaController>();
            Vector2Int arenaSize = new Vector2Int(
                Mathf.Max(8, config.arenaSize.x),
                Mathf.Max(8, config.arenaSize.y));
            Vector2Int centerGrid = config.useCustomCenter
                ? config.centerGrid
                : new Vector2Int(150, 150);

            SetField(arena, "biome", biome);
            SetField(arena, "arenaConfig", config);
            SetField(arena, "centerGrid", centerGrid);
            SetField(arena, "arenaSize", arenaSize);
            SetField(arena, "bossDefeated", false);
            arenaObject.transform.position = ResolveExpectedCenter(biome, centerGrid, arenaSize);

            Invoke(arena, "BuildBoundaryCellCache");
            List<Vector2Int> blockedCells =
                (List<Vector2Int>)GetField(arena, "blockedBoundaryCells");
            int boundaryWidth = arenaSize.x - config.lockBoundaryInsetInCells * 2;
            int boundaryHeight = arenaSize.y - config.lockBoundaryInsetInCells * 2;
            int expectedBoundaryCells = boundaryWidth * 2 + boundaryHeight * 2 - 4;
            Require(
                blockedCells.Count == expectedBoundaryCells,
                $"{biomeName}: blocked boundary {blockedCells.Count}/{expectedBoundaryCells}");

            Invoke(arena, "BuildTrigger");
            BoxCollider trigger = arena.GetComponent<BoxCollider>();
            Require(trigger != null && trigger.isTrigger, $"{biomeName}: trigger missing");
            float expectedSouthInnerEdge = -arenaSize.y * 0.5f
                + config.lockBoundaryInsetInCells
                + config.wallThicknessInCells;
            float triggerSouthEdge = trigger.center.z - trigger.size.z * 0.5f;
            Require(
                Mathf.Abs(triggerSouthEdge - expectedSouthInnerEdge) <= 0.001f,
                $"{biomeName}: trigger edge {triggerSouthEdge:F3} != wall inner edge {expectedSouthInnerEdge:F3}");

            Vector3 center = arenaObject.transform.position;
            Vector2 footprint = new Vector2(0.68f, 0.48f);
            Vector3 outsideSouth = center + new Vector3(0f, 0f, -16.25f);
            Vector3 insideSouth = center + new Vector3(0f, 0f, -14.45f);
            Vector3 sideEntry = outsideSouth + Vector3.right * 5f;
            Vector3 sideEntryTarget = insideSouth + Vector3.right * 5f;
            Vector3 outsideWallStart = center + new Vector3(6f, 0f, -16.75f);
            Vector3 insideWallImage = center + new Vector3(6f, 0f, -15.75f);
            Require(
                !ContainsPlayableCenter(arena, outsideSouth, footprint),
                $"{biomeName}: outside player considered ready to lock");
            Require(
                ContainsPlayableCenter(arena, insideSouth, footprint),
                $"{biomeName}: fully entered player not considered ready to lock");

            SetField(arena, "arenaLocked", false);
            Require(
                CanTraverse(
                    arena,
                    outsideSouth,
                    insideSouth,
                    footprint),
                $"{biomeName}: south door entry rejected");
            Require(
                !CanTraverse(
                    arena,
                    sideEntry,
                    sideEntryTarget,
                    footprint),
                $"{biomeName}: non-door entry accepted");
            Require(
                !CanTraverse(
                    arena,
                    outsideWallStart,
                    insideWallImage,
                    footprint),
                $"{biomeName}: outside movement entered visible wall range");
            Require(
                CanTraverse(
                    arena,
                    outsideWallStart,
                    outsideWallStart + Vector3.right * 0.2f,
                    footprint),
                $"{biomeName}: movement parallel to outside wall rejected");

            SetField(arena, "arenaLocked", true);
            Vector3 insideCenter = center;
            biome.AddRuntimeBlockedCells(blockedCells);
            Require(
                biome.CanMove(insideCenter, insideCenter + Vector3.right * 0.25f),
                $"{biomeName}: biome grid rejected movement inside locked room");
            Require(
                CanTraverse(
                    arena,
                    insideCenter,
                    insideCenter + Vector3.right * 0.25f,
                    footprint),
                $"{biomeName}: locked X movement rejected");
            Require(
                CanTraverse(
                    arena,
                    insideCenter,
                    insideCenter + Vector3.forward * 0.25f,
                    footprint),
                $"{biomeName}: locked Z movement rejected");
            Require(
                !CanTraverse(
                    arena,
                    insideSouth,
                    outsideSouth,
                    footprint),
                $"{biomeName}: locked wall crossing accepted");
            biome.RemoveRuntimeBlockedCells(blockedCells);

            UnityEngine.Object.DestroyImmediate(arenaObject);
            UnityEngine.Object.DestroyImmediate(biomeObject);
        }

        private static Vector3 ResolveExpectedCenter(
            BossArenaWallPreviewBiome biome,
            Vector2Int centerGrid,
            Vector2Int arenaSize)
        {
            Vector3 center = biome.GridToWorld(centerGrid.x, centerGrid.y);
            if (arenaSize.x % 2 == 0) center.x -= biome.TileSize * 0.5f;
            if (arenaSize.y % 2 == 0) center.z -= biome.TileSize * 0.5f;
            return center;
        }

        private static object GetField(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            Require(field != null, $"Missing field: {fieldName}");
            return field.GetValue(target);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            Require(field != null, $"Missing field: {fieldName}");
            field.SetValue(target, value);
        }

        private static void Invoke(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, PrivateInstance);
            Require(method != null, $"Missing method: {methodName}");
            method.Invoke(target, null);
        }

        private static bool CanTraverse(
            MidBossArenaController arena,
            Vector3 current,
            Vector3 desired,
            Vector2 halfExtents)
        {
            MethodInfo method = arena.GetType().GetMethod("CanTraverseBoundary", PrivateInstance);
            Require(method != null, "Missing method: CanTraverseBoundary");
            return (bool)method.Invoke(arena, new object[] { current, desired, halfExtents });
        }

        private static bool ContainsPlayableCenter(
            MidBossArenaController arena,
            Vector3 position,
            Vector2 halfExtents)
        {
            MethodInfo getBounds = arena.GetType().GetMethod("GetArenaWorldBounds", PrivateInstance);
            Require(getBounds != null, "Missing method: GetArenaWorldBounds");
            object bounds = getBounds.Invoke(arena, new object[] { halfExtents });
            MethodInfo contains = bounds.GetType().GetMethod("ContainsPlayableCenter");
            Require(contains != null, "Missing method: ContainsPlayableCenter");
            return (bool)contains.Invoke(bounds, new object[] { position });
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
