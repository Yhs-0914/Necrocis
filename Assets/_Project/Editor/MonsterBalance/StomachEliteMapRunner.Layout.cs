using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Necrocis;
using ProceduralMap;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class StomachEliteMapRunner
    {
        [Serializable] private sealed class LayoutPoint { public string id; public int x, y; }
        [Serializable] private sealed class LayoutRecord
        {
            public int seed, width, height, roadTiles, reachableCells, entranceX, entranceY, bossX, bossY;
            public float tileSize, spacing, bossWidth, bossHeight;
            public string terrainBase64, difficulty;
            public LayoutPoint[] points;
        }
        private static void ExportMapOverview()
        {
            var start = biome.WorldToGrid(biome.GetPlayerSpawnPosition()); var map = biome.GetComponent<MapGenerator>();
            var queue = new Queue<Vector2Int>(); var reached = new HashSet<Vector2Int> { start }; queue.Enqueue(start);
            var steps = new[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                foreach (var step in steps)
                {
                    var next = p + step;
                    if (!biome.IsValidPosition(next.x, next.y) || reached.Contains(next) || map.IsCellReservedForBossArena(next.x, next.y)
                        || !biome.IsWalkable(next.x, next.y) || !biome.CanMove(biome.GridToWorld(p.x, p.y), biome.GridToWorld(next.x, next.y))) continue;
                    reached.Add(next); queue.Enqueue(next);
                }
            }
            foreach (var point in field.Plan.placements) Require(reached.Contains(new Vector2Int(point.x, point.y)), "spawn reachable without crossing reserved boss area");
            byte[] tiles = new byte[biome.MapWidth * biome.MapHeight]; int roads = 0;
            for (int y = 0; y < biome.MapHeight; y++) for (int x = 0; x < biome.MapWidth; x++)
            {
                bool road = map.Data.GetCell(x, y).HasRoad; if (road) roads++;
                tiles[y * biome.MapWidth + x] = (byte)(!biome.IsWalkable(x, y) ? 0 : road ? 3 : biome.GetHeightLevel(x, y) > 0 ? 2 : 1);
            }
            var arena = biome.GetBiomeConfig().GetMidBossArenaConfig(); var boss = arena.useCustomCenter ? arena.centerGrid : new Vector2Int(biome.MapWidth / 2, biome.MapHeight / 2);
            var record = new LayoutRecord {
                seed = biome.Seed, width = biome.MapWidth, height = biome.MapHeight, roadTiles = roads, reachableCells = reached.Count,
                entranceX = start.x, entranceY = start.y, bossX = boss.x, bossY = boss.y, tileSize = biome.TileSize,
                spacing = source.minimumSpacing, bossWidth = arena.arenaSize.x, bossHeight = arena.arenaSize.y,
                terrainBase64 = Convert.ToBase64String(tiles), difficulty = difficulty.ToString(),
                points = field.Plan.placements.Select(p => new LayoutPoint { id = p.monsterId, x = p.x, y = p.y }).ToArray() };
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/Stomach-S-MAP-" + difficulty + (preview ? "-preview" : "") + "-layout.json", JsonUtility.ToJson(record, true));
            if (!preview) Pass($"{difficulty} ACCESS seed{record.seed}: both real placements reachable from entrance outside boss reserve; {reached.Count} reachable cells, road tiles={roads}");
        }
        private static IEnumerator AccessibilityChecks()
        {
            ExportMapOverview();
            foreach (var point in field.Plan.placements.ToArray())
            {
                Move(Home(point) + Vector3.right * 4.2f); var actor = Actor(point);
                foreach (var pattern in actor.GetComponents<MonsterPatternController>()) pattern.EndSpawn(); actor.SetAiSuppressed(true);
                health.ResetHealth(); float hp = health.CurrentHealth;
                yield return WalkTo(Home(point) + new Vector3(4.2f, 0, 3.5f), 1.5f);
                yield return WalkTo(Home(point) + Vector3.forward * 4.2f, 1.5f);
                Equal(hp, health.CurrentHealth, "native local clearance walk safe"); actor.ReleaseToPool(); field.Refresh(player.transform.position);
                Pass(difficulty + " WALK " + point.monsterId + ": actual native walk around spawn confirms the authored 5-cell clearance");
            }
        }
    }
}
