using System;
using System.Collections;
using System.Linq;
using Necrocis;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class PollenInvaderConnectionRunner
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
            throw new InvalidOperationException("No flat patch at level " + level);
        }
        private static PollenPellet LaunchProbe(Vector3 start, Vector3 aim, float speed = 3.5f, float seconds = 2.4f)
        {
            origin = start - aim * 2; origin.y = biome.GetGroundHeight(origin);
            Spawn(); pattern.EndSpawn(); enemy.SetAiSuppressed(true); Move(start + Vector3.Cross(aim, Vector3.up) * 4);
            Require(DustCloud.CanTravel(start, start, settings.pelletRadius), "probe starts clear");
            var life = enemy.GetComponent<EnemyPatternLifetime>(); life.Bind(enemy);
            return PollenPellet.Launch(enemy, life, enemy.SpawnGeneration, new PollenVolley(enemy.CreatePatternDamage(PollenInvaderPatternSettings.DamageId, 0)),
                start, aim, speed, seconds, settings.pelletRadius, settings.presentation.pelletFrames, settings.presentation.pelletFrameSeconds);
        }
        private static IEnumerator TerrainChecks()
        {
            var saved = origin;
            foreach (float direction in BattleAngles)
            {
                Spawn(); Move(origin + Aim(direction) * 4); yield return Wait(() => pattern.VolleyCount == 1, 3, "wall volley");
                var pellet = pattern.ActivePellets[1]; var trace = pellet.gameObject.AddComponent<PollenFlightTrace>(); trace.Initialize(pellet);
                var blocked = biome.WorldToGrid(pellet.LaunchPosition + pellet.Direction * 2.8f); biome.AddRuntimeBlockedCells(new[] { blocked });
                try
                {
                    Move(pellet.LaunchPosition + pellet.Direction * 4.2f); health.ResetHealth(); float hp = health.CurrentHealth;
                    yield return Wait(() => trace.Finished, 2, "central pellet stopped at wall");
                    var bounds = new Bounds(biome.GridToWorld(blocked.x, blocked.y), new Vector3(biome.TileSize, 1, biome.TileSize));
                    Require(CombatHitGeometry.PointRectDistanceSquared(CombatHitGeometry.Flat(trace.LastPosition), bounds) >= settings.pelletRadius * settings.pelletRadius - .0005f, "round pellet stops outside wall");
                    yield return Wait(() => enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, 3, "wall volley cleaned");
                    Equal(hp, health.CurrentHealth, "behind wall target unharmed"); Require(!enemy.IsPatternPositionLocked, "wall recovery unlocks");
                }
                finally { biome.RemoveRuntimeBlockedCells(new[] { blocked }); }
                Pass($"WALL {direction:0}deg: actual central pellet stops before full radius contacts wall; target behind unharmed and clean recovery");
            }
            var cell = biome.WorldToGrid(saved); var center = biome.GridToWorldWithHeight(cell.x, cell.y);
            foreach (float direction in new[] { 45f, 135f, 225f, 315f }) foreach (float sign in new[] { -1f, 1f })
            {
                var aim = Aim(direction); var normal = Vector3.Cross(aim, Vector3.up);
                var corner = center + new Vector3(Mathf.Sign(normal.x), 0, Mathf.Sign(normal.z)) * (biome.TileSize * .5f);
                var tangent = corner + normal * (settings.pelletRadius + sign * .025f); var start = tangent - aim * 2.2f;
                var pellet = LaunchProbe(start, aim, 3.5f, 1.4f); var trace = pellet.gameObject.AddComponent<PollenFlightTrace>(); trace.Initialize(pellet);
                biome.AddRuntimeBlockedCells(new[] { cell });
                try
                {
                    yield return Wait(() => trace.Finished, 2, "corner flight ends"); bool crossed = Vector3.Dot(trace.LastPosition - tangent, aim) > .6f;
                    Require(sign < 0 ? !crossed : crossed, "rounded corner pass/stop");
                    Pass($"CORNER {direction:0}deg/{sign * .025f:+.000;-.000}m: actual moving pellet {(crossed ? "passes" : "stops")} rounded corner");
                }
                finally { biome.RemoveRuntimeBlockedCells(new[] { cell }); enemy.ReleaseToPool(); }
            }
            foreach (int level in new[] { 0, 1 })
            {
                origin = FindPatch(level, 5); Spawn(); Move(origin + Vector3.right * 4);
                yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 2, "height tell"); Move(origin - Vector3.right * 3);
                yield return Wait(() => pattern.VolleyCount == 1, 2, "height release"); float until = Time.time + .3f;
                while (Time.time < until)
                {
                    yield return new WaitForEndOfFrame();
                    foreach (var pellet in pattern.ActivePellets) { Require(pellet != null, "height lane clear"); CheckPellet(pellet, .16f, 3.5f); }
                    Equal(level * biome.HeightStep, enemy.GetComponent<Rigidbody>().position.y, "body on real local ground");
                }
                Pass($"HEIGHT level{level}: actual body at {level * biome.HeightStep:F2}m, all pellets at local ground+.16m");
            }
            bool found = false; Vector2Int boundary = default;
            for (int z = 4; z < biome.MapHeight - 4 && !found; z++) for (int x = 6; x < biome.MapWidth - 7 && !found; x++)
            {
                if (biome.GetHeightLevel(x, z) != 0 || biome.GetHeightLevel(x + 1, z) != 1) continue; bool clear = true;
                for (int dx = -5; dx <= 6 && clear; dx++) for (int dz = -2; dz <= 2 && clear; dz++)
                    clear = biome.IsWalkable(x + dx, z + dz) && biome.GetHeightLevel(x + dx, z + dz) == (dx <= 0 ? 0 : 1);
                if (clear) { boundary = new Vector2Int(x, z); found = true; }
            }
            Require(found, "real low/high boundary");
            foreach (bool up in new[] { true, false })
            {
                var aim = up ? Vector3.right : Vector3.left;
                var pellet = LaunchProbe(biome.GridToWorldWithHeight(boundary.x + (up ? -2 : 3), boundary.y), aim);
                var trace = pellet.gameObject.AddComponent<PollenFlightTrace>(); trace.Initialize(pellet);
                Move(biome.GridToWorldWithHeight(boundary.x + (up ? 4 : -3), boundary.y)); health.ResetHealth(); float hp = health.CurrentHealth;
                yield return Wait(() => trace.Finished, 2, "ledge stops pellet");
                var grid = biome.WorldToGrid(trace.LastPosition); Require(biome.GetHeightLevel(grid.x, grid.y) == (up ? 0 : 1), "pellet remains original height");
                Require(trace.MaxHeightError < .005f, "no hovering at ledge"); Equal(hp, health.CurrentHealth, "no cross-height hit");
                enemy.ReleaseToPool(); Pass(up ? "LEDGE up: pellet stops before higher terrain with no cross-height damage" : "LEDGE down: pellet stops at edge instead of hovering across lower ground");
            }
            origin = saved;
        }
    }
}
