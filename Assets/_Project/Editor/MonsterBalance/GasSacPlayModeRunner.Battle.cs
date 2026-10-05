using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class GasSacPlayModeRunner
    {
        private static readonly float[] GasAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };
        public static void RunDirectionBattle() => Start(false, true, false, false, false, 1);
        public static void RunTerrainChecks() => Start(false, true, false, false, false, 2);

        private static GasSacOrb LaunchGasAudit(Vector3 aim, float offset, float speed = 3, float seconds = 2)
        {
            SpawnOrbTest(); MovePlayer(0); health.ResetHealth();
            pattern.enabled = false; enemy.SetAiSuppressed(true);
            Vector3 target = player.HitCollider.bounds.center; target.y = origin.y;
            enemy.transform.position = target - aim * 3 + new Vector3(-aim.z, 0, aim.x) * offset;
            enemy.SetPatternFacing(aim); enemy.SetPatternFrame(settings.presentation.deflated); Physics.SyncTransforms();
            return GasSacOrb.Launch(enemy, enemy.GetComponent<EnemyPatternLifetime>(), enemy.SpawnGeneration, aim,
                enemy.CreatePatternDamage(GasSacPatternSettings.OrbDamageId, 0), speed, seconds, settings.orbHitRadius,
                settings.presentation.gasPuff, settings.presentation.filledCircle, settings.presentation.gasColor);
        }

        private static IEnumerator DirectionBattleChecks()
        {
            field.enabled = false; enemy = null; origin = FindDirectionPatch();
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 60, true);
            var art = settings.presentation; var directions = art.directionalPresentation;
            foreach (float angle in GasAngles)
            {
                Vector3 aim = Aim(angle); var shot = LaunchGasAudit(aim, 1.4f);
                Require(shot != null, "visual shot launches at " + angle);
                float forward = Vector3.Dot(shot.LaunchPosition - enemy.transform.position, aim);
                Vector3 extents = enemy.GetComponent<Collider>().bounds.extents;
                float extent = Mathf.Abs(aim.x) * extents.x + Mathf.Abs(aim.z) * extents.z;
                Require(forward - settings.orbHitRadius < extent && forward + settings.orbHitRadius > extent,
                    "orb overlaps body front at release without a detached gap");
                float minimum = float.PositiveInfinity, maximum = float.NegativeInfinity, end = Time.time + .25f;
                while (Time.time < end)
                {
                    yield return new WaitForEndOfFrame(); Require(shot != null, "visual shot stays alive");
                    float ground = biome.GetGroundHeight(shot.transform.position);
                    Transform body = shot.transform.Find("CompressedGas");
                    Equal(settings.orbHitRadius, body.position.y - ground, "orb center height above actual ground");
                    foreach (var layer in body.GetComponentsInChildren<SpriteRenderer>())
                    {
                        Bounds b = layer.sprite.bounds;
                        foreach (float x in new[] { b.min.x, b.max.x }) foreach (float y in new[] { b.min.y, b.max.y })
                        {
                            float h = layer.transform.TransformPoint(new Vector3(x, y, 0)).y - ground;
                            minimum = Mathf.Min(minimum, h); maximum = Mathf.Max(maximum, h);
                            Require(h >= -.003f && h <= settings.orbHitRadius * 2 + .03f, "entire visible quad stays grounded without floor penetration");
                        }
                    }
                    Require(shot.GetComponentsInChildren<LineRenderer>().Length == 0, "no simple projectile floor UI");
                }
                Pass($"FLIGHT {angle:0}deg: muzzle={forward:F3}m, center=.3m, quad={minimum:F3}..{maximum:F3}m, no detached launch/floor UI");
                enemy.GetComponent<EnemyPatternLifetime>().ReleaseOwned(shot.gameObject); yield return null;
            }
            foreach (float angle in GasAngles)
            {
                Vector3 aim = Aim(angle); var shot = LaunchGasAudit(aim, 0); Require(shot != null, "foot probe launches");
                yield return new WaitForEndOfFrame();
                var outline = shot.transform.Find("CompressedGas/Outline").GetComponent<SpriteRenderer>();
                var playerBody = player.GetComponentsInChildren<SpriteRenderer>().First(r => r.sprite != null);
                var camera = DontStarveCamera.GetActiveCamera();
                Vector3 contactCenter = GasSacOrb.GetFlightPosition(player.HitCollider.bounds.center, shot.FlightHeight);
                float rx = outline.sprite.bounds.extents.x * outline.transform.lossyScale.x;
                float ry = outline.sprite.bounds.extents.y * outline.transform.lossyScale.y;
                bool overlaps = false;
                for (int i = 0; i <= 6 && !overlaps; i++)
                {
                    Bounds b = playerBody.sprite.bounds;
                    Vector3 foot = playerBody.transform.TransformPoint(new Vector3(b.center.x, b.min.y + b.size.y * (.02f * i), 0));
                    Vector3 delta = foot - contactCenter;
                    float x = Vector3.Dot(delta, camera.transform.right) / rx, y = Vector3.Dot(delta, camera.transform.up) / ry;
                    overlaps = x * x + y * y <= 1.05f * 1.05f;
                }
                Require(overlaps, "visible orb ellipse overlaps foot/ankle band at " + angle);
                float hp = health.CurrentHealth; yield return Wait(() => shot == null, 3, "central orb contact");
                Equal(3, hp - health.CurrentHealth, "actual central hit once");
                Pass($"FOOT {angle:0}deg: visible ellipse touches player foot/ankle band; actual damage 3 once");
            }
            foreach (float angle in new[] { 45f, 135f, 225f, 315f })
            {
                Vector3 aim = Aim(angle); SpawnOrbTest(); MovePlayerTo(origin + aim * 4.8f); health.ResetHealth();
                yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 4, "diagonal actual tell");
                EnemyFacing facing = enemy.PatternFacing; Vector3 locked = pattern.LockedOrbDirection;
                MovePlayerTo(origin - aim * 3.8f);
                yield return Wait(() => pattern.ActiveOrb != null, 3, "diagonal actual shot");
                Require(enemy.PatternFacing == facing && enemy.IsPatternFacingLocked, "diagonal body stays committed");
                Equal(0, (pattern.ActiveOrb.Direction - locked).magnitude, "diagonal aim is not snapped to sprite sector");
                enemy.SuppressExperienceReward = true; var liveShot = pattern.ActiveOrb; enemy.TakeDamage(10000);
                Require(!liveShot.gameObject.activeSelf, "death disables launched orb synchronously");
                var body = DirectionBody; var seen = new HashSet<Sprite>(); float until = Time.time + 2;
                while (enemy.gameObject.activeSelf && Time.time < until) { seen.Add(body.sprite); yield return null; }
                Require(!enemy.gameObject.activeSelf && art.deathFrames.All(f => seen.Contains(directions.Capture().Resolve(f, facing))),
                    "all selected-direction death frames finish before pooling");
                Pass($"DIAGONAL {angle:0}deg: exact XZ aim, stable {facing} pose, immediate flight cancellation and 6-frame death");
            }
            SpawnOrbTest(); MovePlayerTo(origin + Aim(135) * 2.3f);
            yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 4, "diagonal A death cancellation");
            var area = pattern.TelegraphObject; enemy.SuppressExperienceReward = true; enemy.TakeDamage(10000);
            Require(area != null && !area.activeSelf && pattern.BurstCount == 0, "A death disables full warning area synchronously");
            yield return null;
            Pass("Death during diagonal A preparation disables the red area immediately and cancels the burst");
            foreach (bool dash in new[] { false, true })
            {
                Vector3 aim = Aim(dash ? 135 : 45), normal = new Vector3(-aim.z, 0, aim.x);
                Spawn(); MovePlayerTo(origin + aim * 4.8f); health.ResetHealth();
                yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 4, "diagonal dodge tell");
                float hp = health.CurrentHealth; Vector3 start = player.transform.position;
                if (dash)
                {
                    typeof(PlayerController).GetField("lastMoveDirection", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(player, normal);
                    player.StartCoroutine((IEnumerator)typeof(PlayerController).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(player, null));
                    yield return new WaitForSeconds(.4f);
                }
                else
                {
                    float until = Time.time + 1;
                    while (Vector3.Dot(player.transform.position - start, normal) < 1.4f)
                    { Require(Time.time < until, "actual walking moves sideways during tell"); player.TryMoveByWorld(normal * player.MoveSpeed * Time.deltaTime); yield return null; }
                }
                Require(Vector3.Dot(player.transform.position - start, normal) > .8f, "native dodge actually moved");
                yield return Wait(() => pattern.OrbCount == 1, 2, "dodged diagonal shot");
                yield return Wait(() => pattern.ActiveOrb == null, 4, "dodged diagonal expiry"); Equal(hp, health.CurrentHealth, "diagonal dodge avoids hit");
                Pass(dash ? "Actual diagonal player dash avoids B with default enemy movement and zero damage" : "Actual diagonal default-speed walking avoids B with zero damage");
            }
            Spawn(); Vector3 before = enemy.transform.position; MovePlayerTo(origin + Aim(45) * 6.9f);
            yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 5, "actual approach reaches B range");
            Require(PlanarDistance(before, enemy.transform.position) > .1f && enemy.Stats.MoveSpeed > 0, "default enemy movement closes range");
            Vector3 stopped = enemy.transform.position; yield return new WaitForSeconds(.35f);
            Equal(0, PlanarDistance(stopped, enemy.transform.position), "enemy stops locomotion during committed tell");
            Pass("Default-speed enemy approaches diagonal target, enters B range and stops during preparation");
        }
    }
}
