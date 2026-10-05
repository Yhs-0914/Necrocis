using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Necrocis;
using ProceduralMap;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class DustClumpConnectionRunner
    {
        private static Vector3 FindPatch(int level, int clearance = 7)
        {
            for (int z = clearance + 1; z < biome.MapHeight - clearance - 1; z++) for (int x = clearance + 1; x < biome.MapWidth - clearance - 1; x++)
            {
                if (biome.GetHeightLevel(x, z) != level) continue; bool clear = true;
                for (int dx = -clearance; dx <= clearance && clear; dx++) for (int dz = -clearance; dz <= clearance && clear; dz++)
                    clear = biome.IsWalkable(x + dx, z + dz) && biome.GetHeightLevel(x + dx, z + dz) == level;
                if (clear) return biome.GridToWorldWithHeight(x, z);
            }
            throw new InvalidOperationException("No flat patch at terrain level " + level);
        }
        private static DustCloud LaunchProbe(Vector3 start, Vector3 aim, float speed = 2, float seconds = 2)
        {
            origin = start - aim * settings.launchOffset; origin.y = biome.GetGroundHeight(origin);
            Spawn(); pattern.EndSpawn(); enemy.SetAiSuppressed(true);
            Move(start + Vector3.Cross(aim, Vector3.up) * 5);
            Require(DustCloud.CanTravel(start, start, settings.cloudRadius), "probe starts clear");
            var life = enemy.GetComponent<EnemyPatternLifetime>(); life.Bind(enemy);
            var marker = new GameObject("P01_TerrainProbeMarker"); marker.transform.position = start + Vector3.up * .055f;
            life.Own(marker, go => { go.SetActive(false); UnityEngine.Object.Destroy(go); });
            EnemyGroundTelegraph.Circle(marker.transform, settings.cloudRadius, settings.presentation.dangerFillColor);
            var art = settings.presentation;
            return DustCloud.Launch(life, enemy.SpawnGeneration, start, aim, enemy.CreatePatternDamage(DustClumpPatternSettings.DamageId, 0),
                marker, settings.cloudRadius, speed, seconds, art.cloudFrames, art.dissolveFrames, art.cloudFrameSeconds, art.dissolveFrameSeconds, art.cloudOpacity);
        }
        private static IEnumerator TerrainChecks()
        {
            Vector3 saved = origin;
            foreach (float direction in BattleAngles)
            {
                Spawn(); Vector3 aim = Aim(direction); Move(origin + aim * 3.6f);
                yield return Wait(() => pattern.ActiveCloud != null, 3, "late obstacle cloud"); var cloud = pattern.ActiveCloud;
                Vector2Int blocked = biome.WorldToGrid(cloud.LaunchPosition + aim * 3); biome.AddRuntimeBlockedCells(new[] { blocked });
                try
                {
                    Move(cloud.LaunchPosition + aim * 4); health.ResetHealth(); float hp = health.CurrentHealth;
                    yield return Wait(() => cloud.IsDissolving, 3, "wall stops cloud");
                    Require(Vector3.Distance(cloud.LaunchPosition, cloud.transform.position) < 3 && !pattern.TelegraphObject.activeSelf, "wall ends damage before crossing");
                    var bounds = new Bounds(biome.GridToWorld(blocked.x, blocked.y), new Vector3(biome.TileSize, 1, biome.TileSize));
                    Require(CombatHitGeometry.PointRectDistanceSquared(CombatHitGeometry.Flat(cloud.transform.position), bounds) >= settings.cloudRadius * settings.cloudRadius - .002f, "whole circle stops outside wall");
                    yield return Wait(() => pattern.Phase == DustClumpPhase.Ready, 2, "wall cleanup unlock");
                    Equal(hp, health.CurrentHealth, "cannot hit target behind wall"); Require(!enemy.IsPatternPositionLocked, "wall stop cannot leave locked owner");
                }
                finally { biome.RemoveRuntimeBlockedCells(new[] { blocked }); }
                Pass($"WALL {direction:0}deg: late obstacle stops whole moving circle before contact, no through-wall damage, clean recovery");
            }
            Spawn(); Move(origin + Vector3.right * 3.6f); yield return Wait(() => pattern.Phase == DustClumpPhase.Windup, 2, "late reservation block");
            var birthBlock = biome.WorldToGrid(pattern.LaunchPosition); biome.AddRuntimeBlockedCells(new[] { birthBlock });
            try { yield return new WaitForSeconds(1.1f); Require(pattern.ReleaseCount == 0 && pattern.TelegraphObject == null && !enemy.IsPatternPositionLocked, "new preparation obstacle cancels launch"); }
            finally { biome.RemoveRuntimeBlockedCells(new[] { birthBlock }); }
            Pass("WINDUP BLOCK: new obstacle before release cancels reservation, visuals and lock");

            var cell = biome.WorldToGrid(saved); Vector3 tileCenter = biome.GridToWorldWithHeight(cell.x, cell.y);
            foreach (float direction in new[] { 45f, 135f, 225f, 315f }) foreach (float sign in new[] { -1f, 1f })
            {
                Vector3 aim = Aim(direction), normal = Vector3.Cross(aim, Vector3.up);
                Vector3 corner = tileCenter + new Vector3(Mathf.Sign(normal.x), 0, Mathf.Sign(normal.z)) * (biome.TileSize * .5f);
                Vector3 tangent = corner + normal * (settings.cloudRadius + sign * .025f), start = tangent - aim * 2.2f;
                var cloud = LaunchProbe(start, aim, 2, 2); biome.AddRuntimeBlockedCells(new[] { cell });
                bool crossed = false;
                try
                {
                    float until = Time.time + 3;
                    while (cloud != null && !crossed) { Require(Time.time < until, "corner finishes"); crossed = Vector3.Dot(cloud.transform.position - tangent, aim) > .6f; yield return null; }
                    Require(sign < 0 ? !crossed : crossed, "rounded corner clearance");
                }
                finally { biome.RemoveRuntimeBlockedCells(new[] { cell }); enemy.ReleaseToPool(); }
                Pass($"CORNER {direction:0}deg/{sign * .025f:+.000;-.000}m: {(crossed ? "passed" : "stopped")}, real moving-cloud rounded geometry");
            }
            foreach (int level in new[] { 0, 1 })
            {
                origin = FindPatch(level, 3); Spawn(); Move(origin + Vector3.right * 2.2f);
                yield return Wait(() => pattern.ActiveCloud != null, 3, "height cloud"); var cloud = pattern.ActiveCloud;
                Move(origin + Vector3.left * 2); float until = Time.time + .3f;
                while (Time.time < until)
                {
                    yield return new WaitForEndOfFrame(); Require(cloud != null && !cloud.IsDissolving, "clear local height lane");
                    Equal(level * biome.HeightStep, cloud.Body.transform.position.y, "cloud bottom on real terrain");
                    Equal(level * biome.HeightStep + .055f, pattern.TelegraphObject.transform.position.y, "red circle ground height");
                    Equal(level * biome.HeightStep, enemy.GetComponent<Rigidbody>().position.y, "core ground anchor stable");
                }
                Pass($"HEIGHT level{level}: real core/cloud ground={level * biome.HeightStep:F2}m and marker+.055m during movement");
            }
            bool found = false; Vector2Int edge = default;
            for (int z = 4; z < biome.MapHeight - 4 && !found; z++) for (int x = 6; x < biome.MapWidth - 7 && !found; x++)
            {
                if (biome.GetHeightLevel(x, z) != 0 || biome.GetHeightLevel(x + 1, z) != 1) continue; bool clear = true;
                for (int dx = -5; dx <= 6 && clear; dx++) for (int dz = -2; dz <= 2 && clear; dz++)
                    clear = biome.IsWalkable(x + dx, z + dz) && biome.GetHeightLevel(x + dx, z + dz) == (dx <= 0 ? 0 : 1);
                if (clear) { edge = new Vector2Int(x, z); found = true; }
            }
            Require(found, "real walkable low/high boundary");
            foreach (bool up in new[] { true, false })
            {
                Vector3 aim = up ? Vector3.right : Vector3.left;
                var cloud = LaunchProbe(biome.GridToWorldWithHeight(edge.x + (up ? -2 : 3), edge.y), aim);
                Move(biome.GridToWorldWithHeight(edge.x + (up ? 4 : -3), edge.y)); health.ResetHealth(); float hp = health.CurrentHealth;
                yield return Wait(() => cloud.IsDissolving, 3, "ledge stop");
                var stopped = biome.WorldToGrid(cloud.transform.position); Require(biome.GetHeightLevel(stopped.x, stopped.y) == (up ? 0 : 1), "cloud stays on original height");
                Equal((up ? 0 : 1) * biome.HeightStep, cloud.Body.transform.position.y, "no hovering across ledge"); Equal(hp, health.CurrentHealth, "opposite-height target unharmed");
                enemy.ReleaseToPool();
                Pass(up ? "LEDGE uphill: cloud stops before low-to-high boundary and cannot hit across it" : "LEDGE downhill: cloud stops before high-to-low boundary without hovering or hitting across it");
            }
            origin = saved;
        }
        private static IEnumerator RoadChecks()
        {
            if (enemy != null) enemy.ReleaseToPool(); var map = biome.GetComponent<MapGenerator>();
            int roads = 0, ordinaryWalkable = 0;
            for (int z = 0; z < biome.MapHeight; z++) for (int x = 0; x < biome.MapWidth; x++)
            {
                if (map.Data.GetCell(x, z).HasRoad) roads++;
                else if (biome.IsWalkable(x, z)) ordinaryWalkable++;
            }
            int count = (int)typeof(MapGenerator).GetField("roadPathCount", Private).GetValue(map);
            int tries = (int)typeof(MapGenerator).GetField("placementAttemptsPerArea", Private).GetValue(map) * count;
            int margin = (int)typeof(MapGenerator).GetField("roadEdgeMargin", Private).GetValue(map);
            float turn = (float)typeof(MapGenerator).GetField("roadTurnChance", Private).GetValue(map);
            var random = new System.Random(unchecked(map.RandomSeed ^ 0x2A6F91C3)); int bossRejected = 0;
            for (int n = 0; n < tries; n++)
            {
                var path = LungRoadGenerator.Create(random, biome.MapWidth, biome.MapHeight, margin, turn, true, biome.MapHeight / 2);
                if (LungRoadGenerator.BuildRibbon(path, biome.MapWidth, biome.MapHeight).Any(p => map.IsCellReservedForBossArena(p.x, p.y))) bossRejected++;
            }
            var start = biome.WorldToGrid(biome.GetPlayerSpawnPosition()); var target = biome.WorldToGrid(fieldOrigin);
            var queue = new Queue<Vector2Int>(); var seen = new HashSet<Vector2Int> { start }; queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var at = queue.Dequeue();
                foreach (var step in new[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down })
                {
                    var next = at + step;
                    if (!biome.IsValidPosition(next.x, next.y) || seen.Contains(next) || map.IsCellReservedForBossArena(next.x, next.y) || !biome.IsWalkable(next.x, next.y)
                        || !biome.CanMove(biome.GridToWorld(at.x, at.y), biome.GridToWorld(next.x, next.y))) continue;
                    seen.Add(next); queue.Enqueue(next);
                }
            }
            Require(seen.Contains(target) && seen.Contains(biome.WorldToGrid(origin)) && ordinaryWalkable > 0, "road status does not disconnect test and field patches");
            Move(origin + Vector3.left * 3); yield return WalkTo(origin + Vector3.right * 3, 2);
            Pass($"ROAD AUDIT seed={map.RandomSeed}: roadTiles={roads}, ordinaryWalkable={ordinaryWalkable}, center-road candidates touching boss reserve={bossRejected}/{tries}; field/test patches reachable and actual 6m floor walk succeeds");
        }
    }
}
