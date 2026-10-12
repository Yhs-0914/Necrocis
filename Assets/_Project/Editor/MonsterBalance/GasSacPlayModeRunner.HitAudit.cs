using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Necrocis;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class GasSacPlayModeRunner
    {
        // Regression contract from the user-approved audit: preserve the burst; round capsule/orb hits at every angle.
        private static IEnumerator HitRangeAudit()
        {
            field.enabled = false; enemy = null;
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 60, true);
            SetBool(settings, "orbEnabled", false);
            // Give the test a reproducible, same-height open patch; this does not change the generated map.
            origin = FindAuditPatch();
            MovePlayer(8);
            Require(Mathf.Abs(playerGroundOffset + 2f) < .01f, "audit preserves the real gameplay spawn's -2m player root offset");
            Pass("audit origin=" + origin + "; actual gameplay player ground offset=" + playerGroundOffset + "; production hit settings are unchanged");
            foreach (float angle in GasAngles)
            foreach (float distance in new[] { 3.24f, 3.25f, 3.26f })
            {
                SpawnOrbTest(); MovePlayer(2.3f); health.ResetHealth();
                yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 3, "burst edge sample");
                Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
                MovePlayerTo(enemy.transform.position + direction * distance);
                float actualDistance = PlanarDistance(player.transform.position, enemy.transform.position);
                float before = health.CurrentHealth;
                var ring = pattern.TelegraphObject.GetComponent<LineRenderer>();
                float ringRadius = ring.GetPosition(0).magnitude;
                yield return Wait(() => pattern.Phase == GasSacPhase.Recovery, 3, "burst edge impact");
                Equal(3.25f, ringRadius, "approved burst radius remains unchanged");
                if (distance < 3.25f) Equal(3, before - health.CurrentHealth, "burst inside edge hits once");
                if (distance > 3.25f) Equal(0, before - health.CurrentHealth, "burst outside edge misses");
                Pass($"BURST angle={angle:0} distance={actualDistance:F4} ringRadius={ringRadius:F4} damage={before - health.CurrentHealth:0}");
            }

            // Long straight paths offset from the actual collider center, at several incoming angles.
            foreach (float angle in new[] { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f })
            foreach (float offset in new[] { .94f, .99f, 1.20f, 1.40f })
            {
                SpawnOrbTest(); MovePlayer(0); health.ResetHealth();
                enemy.enabled = false; pattern.enabled = false; // Manual launch uses the production projectile and its real Update/hit path.
                yield return null;
                Bounds playerBounds = player.HitCollider.bounds;
                Vector3 center = playerBounds.center; center.y = origin.y;
                Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector3 normal = new Vector3(-direction.z, 0, direction.x);
                enemy.transform.position = center - direction * 2f + normal * offset;
                float before = health.CurrentHealth;
                var lifetime = enemy.GetComponent<EnemyPatternLifetime>();
                GasSacPresentation art = settings.presentation;
                var shot = GasSacOrb.Launch(enemy, lifetime, enemy.SpawnGeneration, direction,
                    enemy.CreatePatternDamage(GasSacPatternSettings.OrbDamageId, 0), settings.orbSpeed,
                    4f / settings.orbSpeed, settings.orbHitRadius, art.gasPuff, art.filledCircle, art.gasColor);
                yield return Wait(() => shot == null, 3, "offset shot completion");
                Equal(offset < .97f ? 3 : 0, before - health.CurrentHealth, "same capsule boundary for every incoming direction");
                Pass($"ORB angle={angle:0} lateralOffset={offset:F2} damage={before - health.CurrentHealth:0} playerXZHalf=({playerBounds.extents.x:F3},{playerBounds.extents.z:F3}) radius={settings.orbHitRadius:F3}");
            }

            foreach (float speed in new[] { 30f, 120f })
            foreach (float offset in new[] { .94f, .99f, 1.2f })
            {
                SpawnOrbTest(); MovePlayer(0); health.ResetHealth(); enemy.enabled = false; pattern.enabled = false;
                yield return null;
                Vector3 center = player.HitCollider.bounds.center; center.y = origin.y;
                Vector3 direction = new Vector3(1, 0, 1).normalized, normal = new Vector3(-1, 0, 1).normalized;
                enemy.transform.position = center - direction * 2 + normal * offset;
                float before = health.CurrentHealth;
                var fastShot = GasSacOrb.Launch(enemy, enemy.GetComponent<EnemyPatternLifetime>(), enemy.SpawnGeneration,
                    direction, enemy.CreatePatternDamage(GasSacPatternSettings.OrbDamageId, 0), speed,
                    4 / speed, settings.orbHitRadius, settings.presentation.gasPuff, settings.presentation.filledCircle, settings.presentation.gasColor);
                yield return Wait(() => fastShot == null, 1, "fast swept hit");
                Equal(offset < .97f ? 3 : 0, before - health.CurrentHealth, "fast sweep preserves rounded hit boundary");
                Pass($"FAST ORB speed={speed:0} diagonalOffset={offset:F2} damage={before - health.CurrentHealth:0}; no tunneling or enlarged corners");
            }

            // Exercise the non-degenerate capsule center-line geometry as well as the current near-spherical player capsule.
            var segmentDistance = typeof(GasSacOrb).GetMethod("SegmentDistanceSquared", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            float Distance(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => (float)segmentDistance.Invoke(null, new object[] { a, b, c, d });
            Equal(0, Distance(new Vector2(-2, 0), new Vector2(2, 0), new Vector2(0, -2), new Vector2(0, 2)), "crossing capsule center lines");
            Equal(1, Distance(new Vector2(-2, 0), new Vector2(2, 0), new Vector2(-1, 1), new Vector2(1, 1)), "parallel center lines");
            Equal(2, Distance(Vector2.zero, Vector2.right, new Vector2(2, 1), new Vector2(3, 1)), "rounded end-cap distance");
            Equal(1, Distance(Vector2.zero, Vector2.zero, Vector2.right, Vector2.right), "zero-length sweep and capsule");
            Pass("capsule geometry: crossing, parallel, rounded end caps and degenerate segments verified");

            SpawnOrbTest(); MovePlayer(0); health.ResetHealth(); enemy.enabled = false; pattern.enabled = false;
            yield return null;
            Collider collider = player.HitCollider; Bounds bounds = collider.bounds;
            var capsule = collider as CapsuleCollider;
            var playerRenderer = player.GetComponentsInChildren<SpriteRenderer>().First(r => r.sprite != null);
            Camera camera = DontStarveCamera.GetActiveCamera();
            Vector3 target = bounds.center; target.y = origin.y;
            enemy.transform.position = target - Vector3.right * 2;
            var bodyShot = GasSacOrb.Launch(enemy, enemy.GetComponent<EnemyPatternLifetime>(), enemy.SpawnGeneration,
                Vector3.right, enemy.CreatePatternDamage(GasSacPatternSettings.OrbDamageId, 0), settings.orbSpeed,
                2, settings.orbHitRadius, settings.presentation.gasPuff, settings.presentation.filledCircle, settings.presentation.gasColor);
            var body = bodyShot.transform.Find("CompressedGas");
            var outer = body.Find("Outline").GetComponent<SpriteRenderer>();
            Vector3 projectedPlayer = camera.WorldToScreenPoint(playerRenderer.bounds.center);
            Vector3 projectedOrbAtPlayerXZ = camera.WorldToScreenPoint(GasSacOrb.GetFlightPosition(target, bodyShot.FlightHeight));
            Equal(settings.orbHitRadius, bodyShot.FlightHeight, "ground flight height derives from the existing orb radius");
            Equal(.3f, bodyShot.FlightHeight, "default orb center is .3m above ground");
            Vector3 foot = playerRenderer.transform.TransformPoint(new Vector3(playerRenderer.sprite.bounds.center.x, playerRenderer.sprite.bounds.min.y, 0));
            Vector3 atPlayer = GasSacOrb.GetFlightPosition(target, bodyShot.FlightHeight);
            Vector3 footDelta = foot - atPlayer;
            float projectedFootDistance = new Vector2(Vector3.Dot(footDelta, camera.transform.right), Vector3.Dot(footDelta, camera.transform.up)).magnitude;
            float visibleOrbRadius = outer.sprite.bounds.extents.x * outer.transform.lossyScale.x;
            Require(projectedFootDistance < visibleOrbRadius, "at contact XZ, the visible low orb overlaps the actual player's feet");
            Require(Mathf.Abs(foot.y - origin.y) < .4f, "player feet stay near the map surface in the corrected preview");
            Pass($"GROUND ALIGNMENT rootOffset={playerGroundOffset:F3}, orbHeight={bodyShot.FlightHeight:F3}, footY={foot.y:F3}, projected foot-to-orb distance={projectedFootDistance:F3} < visible radius={visibleOrbRadius:F3}");
            Pass($"VISUAL playerCollider={collider.GetType().Name} centerY={bounds.center.y:F3} spriteCenterY={playerRenderer.bounds.center.y:F3} orbCenterY={body.position.y:F3} orbOutlineDiameter={outer.sprite.bounds.size.x * outer.transform.lossyScale.x:F3}; center screen vertical offset={Mathf.Abs(projectedPlayer.y - projectedOrbAtPlayerXZ.y):F1}px at {camera.pixelWidth}x{camera.pixelHeight}");
            if (capsule != null) Pass($"CAPSULE radiusLocal={capsule.radius:F4} heightLocal={capsule.height:F4} direction={capsule.direction} lossyScale={capsule.transform.lossyScale}");
            float healthBefore = health.CurrentHealth;
            Vector3 lastPosition = bodyShot.transform.position;
            var lastScreenBounds = new Bounds();
            while (bodyShot != null && health.CurrentHealth == healthBefore)
            {
                lastPosition = bodyShot.transform.position; lastScreenBounds = outer.bounds;
                yield return null;
            }
            Require(health.CurrentHealth < healthBefore, "central orb must produce an actual hit");
            float closestCenterGap = PlanarDistance(lastPosition, bounds.center);
            Pass($"ORB head-on actual hit damage={healthBefore - health.CurrentHealth:0}; last sampled center separation={closestCenterGap:F3}; last visual bounds center={lastScreenBounds.center}");
        }

        private static Vector3 FindAuditPatch()
        {
            Vector2Int initial = biome.WorldToGrid(origin);
            for (int radius = 0; radius <= 40; radius++)
            for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius) continue;
                int x = initial.x + dx, y = initial.y + dy;
                if (!biome.IsValidPosition(x, y) || !biome.IsWalkable(x, y)) continue;
                Vector3 candidate = biome.GridToWorldWithHeight(x, y);
                bool clear = true;
                for (int angle = 0; angle < 360 && clear; angle += 15)
                {
                    Vector3 end = candidate + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * 4;
                    clear = GasSacOrb.HasClearPath(candidate, end);
                }
                if (clear) return candidate;
            }
            throw new System.InvalidOperationException("No open same-height patch for hit audit");
        }
    }
}
