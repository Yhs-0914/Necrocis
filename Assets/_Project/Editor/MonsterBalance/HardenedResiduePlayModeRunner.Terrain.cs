using System.Collections;
using System.Collections.Generic;
using Necrocis;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class HardenedResiduePlayModeRunner
    {
        private static IEnumerator ResidueTerrainChecks()
        {
            BeginResidueD3();
            Require(TryFindResiduePatch(0, out origin), "wide real-map terrain patch");
            Vector3 patch = origin;
            Set(settings, "spawnGraceSeconds", 0);
            foreach (float angle in ResidueAngles) foreach (bool corner in new[] { false, true })
            {
                origin = patch; Spawn(); MovePlayerTo(origin + Aim(angle) * 2.6f);
                // Manual entry before the next update keeps the blocked cell in place for the whole attempt.
                enemy.SetPatternFacing(Aim(angle));
                bool vertical = enemy.PatternFacing == EnemyFacing.Front || enemy.PatternFacing == EnemyFacing.Back;
                Vector3 forward = vertical ? Vector3.left : Vector3.forward;
                Vector3 along = Vector3.Cross(Vector3.up, forward);
                Vector3 center = origin + Aim(angle) * settings.placementDistance;
                Vector3 obstruction = center + (corner ? along * 1.2f + forward * .8f : Vector3.zero);
                Vector2Int wall = biome.WorldToGrid(obstruction);
                Require(biome.IsWalkable(wall.x, wall.y), "temporary obstruction starts in walkable terrain");
                biome.AddRuntimeBlockedCells(new[] { wall });
                try
                {
                    Require(!ResiduePlacementSafety.CanPlace(center, forward, settings.footprint), "wall/corner placement rejected");
                    yield return new WaitForSeconds(.3f);
                    Require(pattern.Phase == HardenedResiduePhase.Ready && pattern.RejectedPlacements > 0
                        && pattern.TelegraphObject == null && pattern.ActiveRubble == null && pattern.LandingCount == 0,
                        "unsafe placement produces no tell, hit or blocker");
                    Pass($"WALL {angle:0}deg/{(corner ? "corner-clearance" : "center")}: actual attack refused, no warning/impact/blocker");
                }
                finally { biome.RemoveRuntimeBlockedCells(new[] { wall }); }
                enemy.ReleaseToPool();
            }

            foreach (float angle in new[] { 0f, 90f })
            {
                origin = patch; Spawn(); MovePlayerTo(origin + Aim(angle) * 2.6f);
                enemy.SetPatternFacing(Aim(angle));
                Vector3 forward = angle == 0 ? Vector3.forward : Vector3.left;
                Vector3 along = Vector3.Cross(Vector3.up, forward);
                Vector3 center = origin + Aim(angle) * settings.placementDistance;
                var walls = new HashSet<Vector2Int>();
                foreach (float sign in new[] { -1f, 1f }) for (int n = -3; n <= 3; n++)
                    walls.Add(biome.WorldToGrid(center + along * (sign * 2 * biome.TileSize) + forward * (n * biome.TileSize)));
                foreach (var cell in walls) Require(biome.IsWalkable(cell.x, cell.y), "corridor fixtures are initially clear");
                biome.AddRuntimeBlockedCells(walls);
                try
                {
                    Vector2Int middle = biome.WorldToGrid(center);
                    Require(biome.IsWalkable(middle.x, middle.y), "corridor floor itself stays open");
                    Require(!ResiduePlacementSafety.CanPlace(center, forward, settings.footprint), "sole corridor cannot lose its detour");
                    yield return new WaitForSeconds(.3f);
                    Require(pattern.RejectedPlacements > 0 && pattern.LandingCount == 0 && pattern.ActiveRubble == null,
                        "actual attack never closes narrow corridor");
                    Pass($"CORRIDOR axis={angle:0}: floor open, side walls prevent a safe detour, actual attack rejected");
                }
                finally { biome.RemoveRuntimeBlockedCells(walls); }
                enemy.ReleaseToPool();
            }

            foreach (HardenedResiduePhase phase in new[] { HardenedResiduePhase.Windup, HardenedResiduePhase.Dropping })
            {
                origin = patch; Spawn(); MovePlayerTo(origin + Aim(135) * 2.6f); health.ResetHealth();
                yield return Wait(() => pattern.Phase == phase, 3, "late obstruction " + phase);
                Vector2Int wall = biome.WorldToGrid(pattern.LandingCenter); var tell = pattern.TelegraphObject; var slab = pattern.ActiveRubble;
                float hp = health.CurrentHealth;
                biome.AddRuntimeBlockedCells(new[] { wall });
                try
                {
                    yield return Wait(() => pattern.Phase == HardenedResiduePhase.Ready, 2, "late unsafe placement cancels");
                    Require(pattern.LandingCount == 0 && (pattern.ActiveRubble == null || !pattern.ActiveRubble.IsBlocking) && pattern.TelegraphObject == null
                        && ResidueRubble.ActiveBlockCount == 0 && !enemy.IsPatternFacingLocked,
                        "safety recheck cancels impact, props, tell and facing lock");
                    Require((tell == null || !tell.activeSelf) && (slab == null || !slab.gameObject.activeSelf), "cancelled graphics inactive");
                    Equal(hp, health.CurrentHealth, "late obstruction has no damage");
                    yield return null; // Unity destroys an already-disabled object at the frame boundary.
                    Require(pattern.ActiveRubble == null && enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0,
                        "cancelled object is destroyed and removed from ownership after the frame boundary");
                    Pass("RECHECK " + phase + ": newly blocked landing cancels damage, visuals and movement blocker");
                }
                finally { biome.RemoveRuntimeBlockedCells(new[] { wall }); }
            }
            if (enemy != null) enemy.ReleaseToPool(); RestoreAssets();

            for (int level = biome.MinHeightLevel; level <= biome.MaxHeightLevel; level++)
            {
                bool present = false;
                for (int z = 0; z < biome.MapHeight && !present; z++) for (int x = 0; x < biome.MapWidth && !present; x++)
                    present = biome.IsWalkable(x, z) && biome.GetHeightLevel(x, z) == level;
                if (!present) { results.Add("INFO no walkable cells at height level " + level); continue; }
                Require(TryFindResiduePatch(level, out origin), "full landing/detour patch at walkable level " + level);
                foreach (float angle in new[] { 0f, 90f, 180f, 270f })
                {
                    Spawn(); MovePlayerTo(origin + Aim(angle) * 2.6f); health.ResetHealth();
                    yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 4, "height tell");
                    float ground = biome.GetGroundHeight(pattern.LandingCenter);
                    Equal(ground + .06f, pattern.TelegraphObject.transform.position.y, "red area grounded above actual surface");
                    MovePlayerTo(pattern.LandingCenter); float hp = health.CurrentHealth;
                    yield return Wait(() => pattern.Phase == HardenedResiduePhase.Recovery, 3, "height landing");
                    Equal(ground, pattern.ActiveRubble.LandingVisualPosition.y, "rubble base matches terrain surface");
                    Equal(ground, pattern.ActiveRubble.transform.position.y, "collider uses same terrain surface");
                    Equal(3, hp - health.CurrentHealth, "height landing damage remains 3 once");
                    Pass($"HEIGHT level={level}/axis={angle:0}: ground={ground:F3}m, matching warning/pivot/collider, damage 3 once");
                    enemy.ReleaseToPool();
                }
            }

            Vector2Int edge = default; bool found = false;
            for (int z = 7; z < biome.MapHeight - 7 && !found; z++) for (int x = 7; x < biome.MapWidth - 7 && !found; x++)
            {
                if (!biome.IsWalkable(x, z) || !biome.IsWalkable(x + 1, z)
                    || biome.GetHeightLevel(x, z) == biome.GetHeightLevel(x + 1, z)) continue;
                bool clear = true;
                for (int dx = -3; dx <= 4 && clear; dx++) for (int dz = -3; dz <= 3 && clear; dz++)
                    clear = biome.IsWalkable(x + dx, z + dz);
                if (clear) { edge = new Vector2Int(x, z); found = true; }
            }
            Require(found, "actual walkable terrain height boundary");
            Vector3 boundary = biome.GridToWorldWithHeight(edge.x, edge.y) + Vector3.right * (biome.TileSize * .5f);
            foreach (float angle in new[] { 0f, 180f })
            {
                Vector3 aim = Aim(angle); origin = boundary - aim * settings.placementDistance;
                origin.y = biome.GetGroundHeight(origin); Spawn(); MovePlayerTo(origin + aim * 2.6f);
                yield return Wait(() => pattern.RejectedPlacements > 0, 3, "height boundary refusal");
                Require(pattern.Phase == HardenedResiduePhase.Ready && pattern.LandingCount == 0 && pattern.TelegraphObject == null,
                    "no slab straddles terrain levels");
                Pass($"LEDGE {angle:0}deg: actual attack refuses footprint/perimeter spanning levels {biome.GetHeightLevel(edge.x, edge.y)}/{biome.GetHeightLevel(edge.x + 1, edge.y)}");
                enemy.ReleaseToPool();
            }
            origin = patch; Spawn(); MovePlayer(8);
            Require(!ResiduePlacementSafety.CanPlace(biome.GetPlayerSpawnPosition(), Vector3.forward, settings.footprint), "entrance stays excluded");
            Vector3 outside = new Vector3(-biome.TileSize, 0, -biome.TileSize);
            Require(!ResiduePlacementSafety.CanPlace(outside, Vector3.forward, settings.footprint), "map boundary stays excluded");
            Pass("EXCLUSIONS: entrance and out-of-map placement rejected; all temporary wall cells removed");
        }
    }
}
