using System.Collections;
using Necrocis;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class InflammationEmberConnectionRunner
    {
        private static IEnumerator TerrainHeightChecks()
        {
            field.enabled = false; PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
            Vector3 original = origin;
            try
            {
                foreach (int level in new[] { 0, 1 })
                {
                    bool found = false;
                    for (int y = 6; y < biome.MapHeight - 6 && !found; y += 2)
                    for (int x = 6; x < biome.MapWidth - 6 && !found; x += 2)
                    {
                        if (biome.GetHeightLevel(x, y) != level || !biome.IsWalkable(x, y)) continue;
                        bool clear = true;
                        for (int dx = -4; dx <= 4 && clear; dx++) for (int dz = -4; dz <= 4 && clear; dz++)
                            clear = biome.GetHeightLevel(x + dx, y + dz) == level && biome.IsWalkable(x + dx, y + dz);
                        if (clear) { origin = biome.GridToWorldWithHeight(x, y); found = true; }
                    }
                    Require(found, "real flat terrain patch at level " + level);
                    var shot = LaunchAudit(Vector3.right, 1.3f, 4, 2); Require(shot != null, "height probe launches");
                    float until = Time.time + .3f;
                    while (Time.time < until)
                    {
                        yield return new WaitForEndOfFrame(); Require(shot != null, "height probe remains on clear terrain");
                        var body = shot.GetComponentInChildren<SpriteRenderer>();
                        Equal(level * biome.HeightStep + settings.thornRadius, body.transform.position.y, "flight uses actual local ground height");
                    }
                    enemy.GetComponent<EnemyPatternLifetime>().ReleaseOwned(shot.gameObject); yield return null;
                    Pass($"TERRAIN level={level}, ground={level * biome.HeightStep:F3}m: moving thorn center remains {settings.thornRadius:F3}m above that surface");
                }

                Vector2Int edge = default; bool boundaryFound = false;
                for (int y = 2; y < biome.MapHeight - 2 && !boundaryFound; y++)
                for (int x = 4; x < biome.MapWidth - 5 && !boundaryFound; x++)
                {
                    if (biome.GetHeightLevel(x, y) != 0 || biome.GetHeightLevel(x + 1, y) != 1) continue;
                    bool clear = true;
                    for (int dx = -3; dx <= 4 && clear; dx++) for (int dz = -1; dz <= 1 && clear; dz++)
                        clear = biome.IsWalkable(x + dx, y + dz) && biome.GetHeightLevel(x + dx, y + dz) == (dx <= 0 ? 0 : 1);
                    if (clear) { edge = new Vector2Int(x, y); boundaryFound = true; }
                }
                Require(boundaryFound, "actual walkable low/high terrain boundary");
                foreach (bool uphill in new[] { true, false })
                {
                    origin = biome.GridToWorldWithHeight(edge.x + (uphill ? -2 : 3), edge.y);
                    SpawnBattle(); MoveTo(biome.GridToWorldWithHeight(edge.x + (uphill ? 3 : -2), edge.y));
                    health.ResetHealth(); float hp = health.CurrentHealth; pattern.enabled = false; enemy.SetAiSuppressed(true); Physics.SyncTransforms();
                    var shot = EmberThorn.Launch(enemy, enemy.GetComponent<EnemyPatternLifetime>(), enemy.SpawnGeneration,
                        uphill ? Vector3.right : Vector3.left, enemy.CreatePatternDamage(InflammationEmberPatternSettings.DamageId, 0),
                        4, 2, settings.thornLength, settings.thornRadius, settings.presentation.thornFrames, settings.presentation.thornFrameSeconds);
                    Require(shot != null, "launch on original side before ledge");
                    float deadline = Time.time + 1;
                    while (shot != null)
                    {
                        Require(Time.time < deadline, "height boundary stops shot");
                        var cell = biome.WorldToGrid(shot.transform.position);
                        Require(biome.GetHeightLevel(cell.x, cell.y) == (uphill ? 0 : 1), "active thorn never crosses the height discontinuity");
                        yield return null;
                    }
                    Equal(hp, health.CurrentHealth, "different-height target cannot be hit through ledge");
                    Pass(uphill ? "TERRAIN uphill: thorn stops at real 0m -> .5m boundary without hitting target" : "TERRAIN downhill: thorn stops at real .5m -> 0m boundary without hovering across gap or hitting target");
                }
            }
            finally { if (enemy != null) enemy.ReleaseToPool(); origin = original; }
        }
    }
}
