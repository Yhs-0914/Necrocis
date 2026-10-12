using System.Collections;
using Necrocis;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class GasSacPlayModeRunner
    {
        private static GasSacOrb LaunchTerrain(Vector3 from, Vector3 target, Vector3 aim, float speed = 3)
        {
            origin = from; SpawnOrbTest(); MovePlayerTo(target); health.ResetHealth();
            pattern.enabled = false; enemy.SetAiSuppressed(true); enemy.SetPatternFacing(aim);
            enemy.SetPatternFrame(settings.presentation.deflated); Physics.SyncTransforms();
            return GasSacOrb.Launch(enemy, enemy.GetComponent<EnemyPatternLifetime>(), enemy.SpawnGeneration, aim,
                enemy.CreatePatternDamage(GasSacPatternSettings.OrbDamageId, 0), speed, 3, settings.orbHitRadius,
                settings.presentation.gasPuff, settings.presentation.filledCircle, settings.presentation.gasColor);
        }

        private static IEnumerator GasTerrainChecks()
        {
            field.enabled = false; enemy = null; origin = FindDirectionPatch();
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 60, true);
            Vector3 patch = origin; float half = biome.TileSize * .5f;
            Vector2Int wall = biome.WorldToGrid(patch); Vector3 center = biome.GridToWorldWithHeight(wall.x, wall.y);
            foreach (bool corner in new[] { false, true }) foreach (float offset in new[] { settings.orbHitRadius - .02f, settings.orbHitRadius + .02f })
            {
                Vector3 aim, from, target;
                if (corner)
                {
                    aim = new Vector3(1, 0, -1).normalized;
                    Vector3 nearest = center - new Vector3(half, 0, half) - new Vector3(1, 0, 1).normalized * offset;
                    from = nearest - aim * 2.5f; target = nearest + aim * 2.5f;
                }
                else
                {
                    aim = Vector3.right;
                    from = new Vector3(center.x - 2.5f, center.y, center.z - half - offset);
                    target = from + aim * 5;
                }
                biome.AddRuntimeBlockedCells(new[] { wall });
                try
                {
                    var shot = LaunchTerrain(from, target, aim); Require(shot != null, "grazing probe starts outside wall");
                    float hp = health.CurrentHealth;
                    yield return Wait(() => shot == null, 3, "grazing wall/corner contact");
                    Equal(offset < settings.orbHitRadius ? 0 : 3, hp - health.CurrentHealth,
                        (corner ? "rounded corner" : "wall side") + " uses full orb radius at clearance " + offset);
                    Pass($"TERRAIN {(corner ? "corner" : "side")}: clearance={offset:F3}m, radius=.3m, damage={hp - health.CurrentHealth:0}; near stops / far clears");
                }
                finally { biome.RemoveRuntimeBlockedCells(new[] { wall }); }
            }
            foreach (float speed in new[] { 30f, 120f })
            {
                Vector3 from = center - Vector3.right * 2.5f, target = center + Vector3.right * 2.5f;
                biome.AddRuntimeBlockedCells(new[] { wall });
                try
                {
                    var shot = LaunchTerrain(from, target, Vector3.right, speed); Require(shot != null, "fast wall shot launches");
                    float hp = health.CurrentHealth; yield return Wait(() => shot == null, 1, "fast shot hits wall");
                    Equal(hp, health.CurrentHealth, "high-speed shot cannot cross wall"); Pass("TERRAIN wall blocks speed=" + speed + " without tunneling");
                }
                finally { biome.RemoveRuntimeBlockedCells(new[] { wall }); }
            }
            origin = patch; SpawnOrbTest(); MovePlayer(4); pattern.enabled = false; enemy.SetAiSuppressed(true); enemy.SetPatternFacing(Vector3.right);
            Vector2Int adjacent = biome.WorldToGrid(enemy.transform.position + Vector3.right * .8f);
            biome.AddRuntimeBlockedCells(new[] { adjacent });
            try
            {
                var prevented = GasSacOrb.Launch(enemy, enemy.GetComponent<EnemyPatternLifetime>(), enemy.SpawnGeneration, Vector3.right,
                    enemy.CreatePatternDamage(GasSacPatternSettings.OrbDamageId, 0), 3, 3, .3f,
                    settings.presentation.gasPuff, settings.presentation.filledCircle, settings.presentation.gasColor);
                Require(prevented == null && enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, "muzzle cannot skip adjacent obstacle");
                Pass("TERRAIN adjacent wall blocks emission without orphan projectile ownership");
            }
            finally { biome.RemoveRuntimeBlockedCells(new[] { adjacent }); }

            for (int level = biome.MinHeightLevel; level <= biome.MaxHeightLevel; level++)
            {
                Vector3 flat = default; bool found = false, present = false;
                for (int y = 2; y < biome.MapHeight - 2 && !found; y++) for (int x = 2; x < biome.MapWidth - 2 && !found; x++)
                {
                    if (biome.GetHeightLevel(x, y) != level || !biome.IsWalkable(x, y)) continue;
                    present = true; bool clear = true;
                    for (int dx = -1; dx <= 1 && clear; dx++) for (int dz = -1; dz <= 1 && clear; dz++)
                        clear = biome.IsWalkable(x + dx, y + dz) && biome.GetHeightLevel(x + dx, y + dz) == level;
                    if (clear) { flat = biome.GridToWorldWithHeight(x, y); found = true; }
                }
                if (!present) { results.Add("INFO no walkable cells at terrain level " + level); continue; }
                Require(found, "actual 3x3 flat patch for level " + level);
                var shot = LaunchTerrain(flat, flat + Vector3.forward * 1.2f, Vector3.right, .5f); Require(shot != null, "flat height shot launches");
                float end = Time.time + .3f;
                while (Time.time < end)
                {
                    yield return new WaitForEndOfFrame(); Require(shot != null, "flat height probe remains alive");
                    Transform body = shot.transform.Find("CompressedGas");
                    Equal(level * biome.HeightStep + settings.orbHitRadius, body.position.y, "orb follows actual terrain height");
                }
                Pass($"HEIGHT level={level}, ground={flat.y:F3}m: moving center={shot.transform.Find("CompressedGas").position.y:F3}m, +.3m above surface");
                enemy.GetComponent<EnemyPatternLifetime>().ReleaseOwned(shot.gameObject); yield return null;
            }
            Vector2Int edge = default; bool boundary = false; int low = 0, high = 0;
            for (int y = 2; y < biome.MapHeight - 2 && !boundary; y++) for (int x = 4; x < biome.MapWidth - 5 && !boundary; x++)
            {
                int a = biome.GetHeightLevel(x, y), b = biome.GetHeightLevel(x + 1, y); if (a >= b) continue;
                bool clear = true;
                for (int dx = -3; dx <= 4 && clear; dx++) for (int dz = -1; dz <= 1 && clear; dz++)
                    clear = biome.IsWalkable(x + dx, y + dz) && biome.GetHeightLevel(x + dx, y + dz) == (dx <= 0 ? a : b);
                if (clear) { edge = new Vector2Int(x, y); low = a; high = b; boundary = true; }
            }
            Require(boundary, "actual walkable height discontinuity");
            foreach (bool uphill in new[] { true, false })
            {
                Vector3 from = biome.GridToWorldWithHeight(edge.x + (uphill ? -2 : 3), edge.y);
                Vector3 target = biome.GridToWorldWithHeight(edge.x + (uphill ? 3 : -2), edge.y);
                var shot = LaunchTerrain(from, target, uphill ? Vector3.right : Vector3.left); Require(shot != null, "shot begins before ledge");
                float hp = health.CurrentHealth, until = Time.time + 2;
                while (shot != null)
                {
                    Require(Time.time < until, "ledge stops flight"); Vector2Int cell = biome.WorldToGrid(shot.transform.position);
                    Require(biome.GetHeightLevel(cell.x, cell.y) == (uphill ? low : high), "orb cannot cross height discontinuity"); yield return null;
                }
                Equal(hp, health.CurrentHealth, "no hit across height boundary");
                Pass($"HEIGHT {(uphill ? "uphill" : "downhill")}: {low * biome.HeightStep:F3}/{high * biome.HeightStep:F3}m, stopped before crossing or damage");
            }
        }
    }
}
