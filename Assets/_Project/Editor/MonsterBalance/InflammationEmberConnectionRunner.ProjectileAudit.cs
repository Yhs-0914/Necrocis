using System;
using System.Collections;
using System.Linq;
using Necrocis;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class InflammationEmberConnectionRunner
    {
        private static readonly float[] FlightAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };

        private static IEnumerator ProjectileAudit()
        {
            foreach (float angle in FlightAngles)
            {
                Vector3 direction = Direction(angle);
                var shot = LaunchAudit(direction, 1.3f, 4, 2);
                Require(shot != null, "valid front muzzle");
                var renderer = shot.GetComponentInChildren<SpriteRenderer>();
                var collider = enemy.GetComponent<Collider>();
                float bodyExtent = Mathf.Abs(direction.x) * collider.bounds.extents.x + Mathf.Abs(direction.z) * collider.bounds.extents.z;
                float forward = Vector3.Dot(shot.LaunchPosition - enemy.transform.position, direction);
                Require(forward - settings.thornLength * .5f < bodyExtent && forward + settings.thornLength * .5f > bodyExtent,
                    "launch tail overlaps body front, tip is outside, no detached gap");
                float minimum = float.PositiveInfinity, maximum = float.NegativeInfinity;
                var seen = new System.Collections.Generic.HashSet<Sprite>();
                float end = Time.time + .3f;
                while (Time.time < end)
                {
                    yield return new WaitForEndOfFrame();
                    Require(shot != null, "visual probe remains alive"); seen.Add(renderer.sprite);
                    float ground = biome.GetGroundHeight(shot.transform.position);
                    Equal(settings.thornRadius, renderer.transform.position.y - ground, "flight center stays at authored radius above actual terrain");
                    Require(Vector3.Dot(renderer.transform.right, direction) > .999f, "sprite long axis follows actual XZ travel");
                    Equal(0, renderer.transform.right.y, "long axis never points into floor");
                    var bounds = renderer.sprite.bounds;
                    foreach (float x in new[] { bounds.min.x, bounds.max.x }) foreach (float y in new[] { bounds.min.y, bounds.max.y })
                    {
                        // The rendered sprite is a z=0 quad; SpriteRenderer.bounds also has artificial depth padding.
                        float height = renderer.transform.TransformPoint(new Vector3(x, y, 0)).y - ground;
                        minimum = Mathf.Min(minimum, height); maximum = Mathf.Max(maximum, height);
                        Require(height >= -.003f && height <= settings.thornRadius * 2 + .003f, "entire rendered quad remains above ground and close to surface");
                    }
                    Equal(settings.thornLength, Vector3.Distance(renderer.transform.TransformPoint(new Vector3(bounds.min.x, 0, 0)),
                        renderer.transform.TransformPoint(new Vector3(bounds.max.x, 0, 0))), "visible long-axis length matches collision length");
                    if (Mathf.Abs(direction.z) > .1f)
                        Require(direction.z > 0 ? renderer.sortingOrder < Body().sortingOrder : renderer.sortingOrder > Body().sortingOrder,
                            "thorn sorts behind/in front according to ground depth");
                    Require(shot.transform.childCount == 1 && shot.GetComponentsInChildren<LineRenderer>().Length == 0, "no floor marker or path UI");
                }
                Require(settings.presentation.thornFrames.All(seen.Contains), "both projectile frames retain ground alignment");
                Pass($"FLIGHT angle={angle:0}: muzzle={forward:F3}m, center={shot.FlightHeight:F3}m, quad bottom={minimum:F3}m/top={maximum:F3}m, both frames, XZ-aligned length and depth sorting; no floor UI");
                enemy.GetComponent<EnemyPatternLifetime>().ReleaseOwned(shot.gameObject); yield return null;
            }

            var capsule = player.HitCollider as CapsuleCollider; Require(capsule != null, "actual player capsule");
            foreach (float angle in FlightAngles)
            {
                Vector3 direction = Direction(angle), normal = new Vector3(-direction.z, 0, direction.x);
                float edge = CapsuleSupport(capsule, normal) + settings.thornRadius;
                foreach (float offset in new[] { edge - .025f, edge + .025f, edge + .3f })
                {
                    var shot = LaunchAudit(direction, offset, 4, 1.5f); Require(shot != null, "boundary shot launches");
                    float hp = health.CurrentHealth;
                    yield return Wait(() => shot == null, 3, "boundary flight finishes");
                    Equal(offset < edge ? 3 : 0, hp - health.CurrentHealth, "capsule side edge at every angle");
                    Pass($"HIT angle={angle:0}: lateral={offset:F3}m boundary={edge:F3}m actual damage={hp - health.CurrentHealth:0}");
                }
            }
            foreach (float speed in new[] { 30f, 120f })
            foreach (float sign in new[] { -1f, 1f })
            {
                Vector3 direction = Direction(45), normal = new Vector3(-direction.z, 0, direction.x);
                float edge = CapsuleSupport(capsule, normal) + settings.thornRadius;
                var shot = LaunchAudit(direction, edge + .025f * sign, speed, 6 / speed); Require(shot != null, "fast shot launches");
                float hp = health.CurrentHealth; yield return Wait(() => shot == null, 1, "fast flight finishes");
                Equal(sign < 0 ? 3 : 0, hp - health.CurrentHealth, "high speed does not tunnel or enlarge hit area");
                Pass($"FAST speed={speed:0}m/s: boundary sign={sign:0}, damage={hp - health.CurrentHealth:0}, no tunneling");
            }

            foreach (float angle in FlightAngles)
            {
                var shot = LaunchAudit(Direction(angle), 0, 4, 2); Require(shot != null, "central shot launches");
                yield return new WaitForEndOfFrame();
                var renderer = shot.GetComponentInChildren<SpriteRenderer>(); var camera = DontStarveCamera.GetActiveCamera();
                var playerBody = player.GetComponentsInChildren<SpriteRenderer>().First(r => r.sprite != null);
                Vector3 atPlayer = GasSacOrb.GetFlightPosition(player.HitCollider.bounds.center, shot.FlightHeight);
                Vector3 longAxis = renderer.transform.right * (settings.thornLength * .5f), shortAxis = renderer.transform.up * settings.thornRadius;
                Vector2 a = new Vector2(Vector3.Dot(longAxis, camera.transform.right), Vector3.Dot(longAxis, camera.transform.up));
                Vector2 b = new Vector2(Vector3.Dot(shortAxis, camera.transform.right), Vector3.Dot(shortAxis, camera.transform.up));
                float determinant = a.x * b.y - a.y * b.x;
                // A low thorn can touch an ankle without covering the exact bottom pixel of a tilted player billboard.
                // Test the visible foot/ankle band (bottom 12%), while the independent quad test forbids floating or floor penetration.
                bool overlaps = false; float along = 0, across = 0;
                for (int i = 0; i <= 6 && !overlaps; i++)
                {
                    var bounds = playerBody.sprite.bounds;
                    Vector3 lowerBody = playerBody.transform.TransformPoint(new Vector3(bounds.center.x, bounds.min.y + bounds.size.y * (.02f * i), 0));
                    Vector3 delta = lowerBody - atPlayer;
                    Vector2 projectedDelta = new Vector2(Vector3.Dot(delta, camera.transform.right), Vector3.Dot(delta, camera.transform.up));
                    along = (projectedDelta.x * b.y - projectedDelta.y * b.x) / determinant;
                    across = (a.x * projectedDelta.y - a.y * projectedDelta.x) / determinant;
                    overlaps = Mathf.Abs(along) < 1 && Mathf.Abs(across) < 1;
                }
                Require(overlaps, $"low thorn overlaps visible foot/ankle band at {angle} degrees (along={along}, across={across})");
                float hp = health.CurrentHealth;
                yield return Wait(() => shot == null, 3, "central contact"); Equal(3, hp - health.CurrentHealth, "central shot hits once");
                Pass($"FOOT angle={angle:0}: projected along={along:F3}, across={across:F3} within visible thorn, actual damage=3");
            }

            var wallShot = LaunchAudit(Vector3.right, 0, 4, 2); Require(wallShot != null, "wall probe launches before obstacle");
            Vector3 wallPosition = wallShot.LaunchPosition + Vector3.right * .8f;
            Vector2Int wall = biome.WorldToGrid(wallPosition); Require(biome.IsWalkable(wall.x, wall.y), "temporary wall starts on clear terrain");
            float beforeWall = health.CurrentHealth; biome.AddRuntimeBlockedCells(new[] { wall });
            try { yield return Wait(() => wallShot == null, 1, "thorn hits terrain wall"); Equal(beforeWall, health.CurrentHealth, "wall stops shot before player"); }
            finally { biome.RemoveRuntimeBlockedCells(new[] { wall }); }
            Pass("terrain obstacle stops the whole thorn before it can hit the player; no wall tunneling");

            SpawnBattle(); Move(3); pattern.enabled = false; enemy.SetAiSuppressed(true);
            Vector3 launchTip = enemy.transform.position + Vector3.right * 1.05f;
            wall = biome.WorldToGrid(launchTip); biome.AddRuntimeBlockedCells(new[] { wall });
            try
            {
                var prevented = EmberThorn.Launch(enemy, enemy.GetComponent<EnemyPatternLifetime>(), enemy.SpawnGeneration, Vector3.right,
                    enemy.CreatePatternDamage(InflammationEmberPatternSettings.DamageId, 0), 4, 2, .7f, .16f, settings.presentation.thornFrames, .12f);
                Require(prevented == null, "front muzzle must not jump through adjacent blocked cell");
            }
            finally { biome.RemoveRuntimeBlockedCells(new[] { wall }); }
            Pass("front muzzle cannot skip an adjacent obstacle when moving the emission point out of the body");

            var shortShot = LaunchAudit(Vector3.right, 1.3f, 4, .1f); Require(shortShot != null, "short-lived shot launches");
            float start = Time.time; yield return Wait(() => shortShot == null, .3f, "short lifetime");
            Require(Time.time - start < .2f, "authored lifetime is respected");
            var screenShot = LaunchAudit(Vector3.right, 1.3f, 4, 2); Require(screenShot != null, "view-exit shot launches");
            Move(20); yield return Wait(() => screenShot == null, .5f, "view exit removes thorn");
            Pass("lifetime expiry and screen exit remove projectile immediately without residual visuals");
        }

        private static Vector3 Direction(float angle) => new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
        private static EmberThorn LaunchAudit(Vector3 direction, float offset, float speed, float seconds)
        {
            SpawnBattle(); Move(0); health.ResetHealth(); pattern.enabled = false; enemy.SetAiSuppressed(true);
            Vector3 target = player.HitCollider.bounds.center; target.y = origin.y;
            Vector3 normal = new Vector3(-direction.z, 0, direction.x);
            enemy.transform.position = target - direction * 3 + normal * offset; enemy.SetPatternFacing(direction, false);
            enemy.SetPatternFrame(settings.presentation.release); Physics.SyncTransforms();
            return EmberThorn.Launch(enemy, enemy.GetComponent<EnemyPatternLifetime>(), enemy.SpawnGeneration, direction,
                enemy.CreatePatternDamage(InflammationEmberPatternSettings.DamageId, 0), speed, seconds,
                settings.thornLength, settings.thornRadius, settings.presentation.thornFrames, settings.presentation.thornFrameSeconds);
        }
        private static float CapsuleSupport(CapsuleCollider capsule, Vector3 normal)
        {
            Vector3 s = capsule.transform.lossyScale; s = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
            int axis = capsule.direction; Vector3 localAxis = axis == 0 ? Vector3.right : axis == 1 ? Vector3.up : Vector3.forward;
            float radiusScale = axis == 0 ? Mathf.Max(s.y, s.z) : axis == 1 ? Mathf.Max(s.x, s.z) : Mathf.Max(s.x, s.y);
            float radius = capsule.radius * radiusScale;
            float halfLine = Mathf.Max(0, capsule.height * s[axis] * .5f - radius);
            return radius + Mathf.Abs(Vector3.Dot(capsule.transform.TransformDirection(localAxis) * halfLine, normal));
        }
    }
}
