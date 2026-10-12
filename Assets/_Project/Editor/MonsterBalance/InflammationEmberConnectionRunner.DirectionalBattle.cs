using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class InflammationEmberConnectionRunner
    {
        public static void RunDirectionalBattleChecks() => Start(false, true, false, 180, false, false, true);

        private static IEnumerator DirectionBattleChecks()
        {
            Require(enemy.HasPatternDirections, "real field actor uses direction snapshot");
            field.enabled = false; enemy = null;
            ((IList)typeof(ProceduralBiomeBridge).GetField("normalEnemyRules", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(biome)).Clear();
            typeof(InputManager).GetMethod("SetActionsEnabled", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(InputManager.Instance, new object[] { false });
            Vector3 fieldOrigin = origin; origin = FindDirectionalPatch();
            yield return BattleChecks();
            yield return NearDirectionChecks();
            yield return DirectionDeathChecks();
            yield return CornerChecks();
            yield return TerrainHeightChecks();
            origin = fieldOrigin; yield return FieldDeathChecks();
            Require(biome.GetBiomeConfig().biomeEliteSpawnConfig.monsters.Count == productionLiverCount, "production Liver list unchanged");
            Pass("D-3 complete: production Liver list unchanged, isolated save only; art and balance sources restored");
        }

        private static IEnumerator NearDirectionChecks()
        {
            var snapshot = settings.presentation.directionalPresentation.Capture();
            foreach (float angle in FlightAngles) foreach (float distance in new[] { .8f, 3f })
            {
                Vector3 aim = Direction(angle); SpawnBattle(); MoveTo(origin + aim * distance); health.ResetHealth();
                yield return null; float hp = health.CurrentHealth, startedAt = Time.time; enemy.TakeDamage(1);
                var facing = enemy.PatternFacing;
                Require(pattern.Phase == InflammationEmberPhase.Windup && enemy.IsPatternFacingLocked, "near first hit commits facing");
                while (pattern.Phase == InflammationEmberPhase.Windup)
                {
                    enemy.TakeDamage(1);
                    Require(enemy.PatternFacing == facing && Body().flipX == snapshot.Flip(facing), "rapid hits keep selected direction");
                    Require(settings.presentation.preparationFrames.Any(f => snapshot.Resolve(f, facing) == Body().sprite), "actual directional preparation pose");
                    Require(Time.time - startedAt < 1, "rapid hits cannot prolong tell");
                    yield return null;
                }
                yield return Wait(() => health.CurrentHealth < hp, 2, "near directional hit");
                Equal(3, hp - health.CurrentHealth, "near counter deals original damage exactly once");
                Equal(0, (pattern.LockedDirection - aim).magnitude, "committed flight vector at every angle");
                Require(pattern.ShotCount == 1 && enemy.PatternFacing == facing, "one shot and held recovery facing");
                MoveTo(origin - aim * 3);
                yield return Wait(() => pattern.Phase == InflammationEmberPhase.Ready, 2, "near recovery unlock"); yield return null;
                Require(!enemy.IsPatternFacingLocked && enemy.PatternFacing != facing, "ready turns toward opposite player");
                Equal(3, hp - health.CurrentHealth, "no residual damage after hit");
                Pass($"COUNTER {angle:0}deg/range={distance:F1}m: rapid-hit tell, {facing} poses, one 3-damage hit, recovery unlock");
            }
            SpawnBattle(); MoveTo(origin); yield return null; enemy.TakeDamage(1);
            yield return new WaitForSeconds(.8f);
            Require(pattern.ShotCount == 0 && pattern.Phase == InflammationEmberPhase.Ready, "zero direction cannot produce invalid shot");
            Pass("coincident player position produces no invalid direction or stuck counter");
        }

        private static IEnumerator DirectionDeathChecks()
        {
            var snapshot = settings.presentation.directionalPresentation.Capture();
            foreach (float angle in FlightAngles) foreach (bool duringFlight in new[] { false, true })
            {
                SpawnBattle(); MoveTo(origin + Direction(angle) * 4); yield return null; enemy.TakeDamage(1);
                var facing = enemy.PatternFacing;
                if (duringFlight) yield return Wait(() => pattern.ActiveThorn != null, 1.2f, "directional live shot before death");
                var shot = pattern.ActiveThorn; uint generation = enemy.SpawnGeneration; var body = Body();
                enemy.TakeDamage(10000);
                Require(shot == null || !shot.gameObject.activeSelf, "death removes live shot immediately");
                MoveTo(origin - Direction(angle) * 3);
                var seen = new HashSet<Sprite>(); float until = Time.time + 2;
                while (enemy.gameObject.activeSelf)
                {
                    Require(Time.time < until && enemy.PatternFacing == facing && body.flipX == snapshot.Flip(facing), "death retains committed directional sprite");
                    seen.Add(body.sprite); yield return null;
                }
                Require(settings.presentation.deathFrames.All(f => seen.Contains(snapshot.Resolve(f, facing))), "six selected death frames finish before release");
                SpawnBattle(); MoveTo(origin - Direction(angle) * 3); yield return null;
                Require(enemy.SpawnGeneration != generation && pattern.ActiveThorn == null && pattern.ShotCount == 0
                    && enemy.HasPatternDirections && !enemy.IsPatternFacingLocked, "reuse has clean direction and owned effects");
                enemy.TakeDamage(1); yield return Wait(() => pattern.ActiveThorn != null, 1.2f, "reused directional shot");
                Require(Vector3.Dot(pattern.ActiveThorn.Direction, -Direction(angle)) > .999f && pattern.ShotCount == 1, "reuse has one callback and new opposite aim");
                enemy.ReleaseToPool(); yield return null;
                Require(UnityEngine.Object.FindObjectsByType<EmberThorn>(FindObjectsSortMode.None).Length == 0, "pool clears every live thorn");
                Pass($"DEATH {angle:0}deg/{(duringFlight ? "flight" : "tell")}: six {facing} frames, immediate cleanup, fresh opposite-direction reuse");
            }
        }

        private static IEnumerator CornerChecks()
        {
            Vector3 savedOrigin = origin;
            Vector2Int cell = biome.WorldToGrid(origin); Vector3 center = biome.GridToWorldWithHeight(cell.x, cell.y);
            foreach (float angle in new[] { 45f, 135f, 225f, 315f }) foreach (float sign in new[] { -1f, 1f })
            {
                Vector3 aim = Direction(angle), normal = new Vector3(-aim.z, 0, aim.x);
                Vector3 corner = center + new Vector3(Mathf.Sign(normal.x), 0, Mathf.Sign(normal.z)) * (biome.TileSize * .5f);
                Vector3 tangent = corner + normal * (settings.thornRadius + sign * .025f);
                origin = tangent - aim * 2.8f; origin.y = biome.GetGroundHeight(origin);
                SpawnBattle(); pattern.enabled = false; enemy.SetPatternFacing(aim, false);
                MoveTo(tangent + normal * 2); health.ResetHealth(); Physics.SyncTransforms();
                float hp = health.CurrentHealth;
                biome.AddRuntimeBlockedCells(new[] { cell });
                try
                {
                    var shot = EmberThorn.Launch(enemy, enemy.GetComponent<EnemyPatternLifetime>(), enemy.SpawnGeneration, aim,
                        enemy.CreatePatternDamage(InflammationEmberPatternSettings.DamageId, 0), 4, 2,
                        settings.thornLength, settings.thornRadius, settings.presentation.thornFrames, settings.presentation.thornFrameSeconds);
                    Require(shot != null, "corner probe launches before the obstruction");
                    float until = Time.time + 1.2f; bool crossed = false;
                    while (shot != null && !crossed)
                    {
                        Require(Time.time < until, "corner traversal deadline");
                        crossed = Vector3.Dot(shot.transform.position - tangent, aim) > .7f;
                        yield return null;
                    }
                    Require(sign < 0 ? !crossed : crossed, "corner capsule +/- .025m clearance at " + angle);
                    Equal(hp, health.CurrentHealth, "corner fixture target is outside shot");
                    if (shot != null) enemy.GetComponent<EnemyPatternLifetime>().ReleaseOwned(shot.gameObject);
                    Pass($"CORNER {angle:0}deg/clearance={settings.thornRadius + sign * .025f:F3}m: {(sign < 0 ? "stopped" : "passed")}, no stray damage");
                }
                finally { biome.RemoveRuntimeBlockedCells(new[] { cell }); }
                enemy.ReleaseToPool(); yield return null;
            }
            origin = savedOrigin;
        }
    }
}
