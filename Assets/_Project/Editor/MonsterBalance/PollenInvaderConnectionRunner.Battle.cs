using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Necrocis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class PollenInvaderConnectionRunner
    {
        private static readonly float[] BattleAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };
        private static IEnumerator BattleChecks(string section)
        {
            if (section == "all" || section == "states") yield return StateChecks();
            if (section == "all" || section == "terrain") yield return TerrainChecks();
            if (section == "all" || section == "death") yield return DirectionDeathChecks();
            if (section == "all" || section == "field") yield return FieldUnloadChecks();
            if (section == "visits") { yield return CleanupAndContact(); yield return VisitChecks(); }
            if (section == "visuals") { yield return SlowMuzzleCheck(); yield return FrameRateChecks(); }
        }
        private static IEnumerator WalkTo(Vector3 target, float seconds)
        {
            float until = Time.time + seconds;
            while (true)
            {
                var delta = target - player.transform.position; delta.y = 0;
                if (delta.magnitude < .06f) yield break;
                Require(Time.time < until, "native walk reaches destination");
                player.TryMoveByWorld(delta.normalized * Mathf.Min(delta.magnitude, player.MoveSpeed * Time.fixedDeltaTime));
                yield return new WaitForFixedUpdate();
            }
        }
        private static void Face(Vector3 direction)
        {
            typeof(PlayerController).GetField("movement", Private).SetValue(player, Vector3.zero);
            typeof(PlayerController).GetField("lastMoveDirection", Private).SetValue(player, direction);
        }
        private static IEnumerator StateChecks()
        {
            foreach (float direction in BattleAngles)
            {
                Spawn(); enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 1); Move(origin + Aim(direction) * 7);
                var start = enemy.GetComponent<Rigidbody>().position;
                yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 5, "native approach");
                Require(Vector3.Distance(start, enemy.GetComponent<Rigidbody>().position) > .8f && enemy.IsPatternPositionLocked, "native approach enters range then locks");
                Require(Mathf.Abs(Mathf.DeltaAngle(direction, (int)enemy.PatternFacing * 45)) <= 53.1f, "facing appropriate to target sector");
                var pursuitDirection = pattern.LockedDirection; var facing = enemy.PatternFacing;
                Move(origin - Aim(direction) * 3); yield return Wait(() => pattern.VolleyCount == 1, 2, "diagonal release");
                Require(enemy.PatternFacing == facing && Vector3.Angle(pursuitDirection, pattern.LockedDirection) < .01f, "diagonal target cannot retarget");
                foreach (var pellet in pattern.ActivePellets) CheckPellet(pellet, .16f, 3.5f);
                Pass($"PURSUIT {direction:0}deg: real movement, proper body sector, committed diagonal fan and low flight");
            }
            foreach (float direction in BattleAngles) foreach (int sideSign in new[] { -1, 1 })
            {
                Spawn(); var aim = Aim(direction);
                Move(origin + aim * 2); health.ResetHealth(); float hp = health.CurrentHealth;
                yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 2, "near tell");
                var side = Vector3.Cross(pattern.LockedDirection, Vector3.up) * sideSign;
                yield return WalkTo(origin + aim * 2 + side * 3, 1.5f);
                yield return Wait(() => pattern.VolleyCount == 1, 2, "near release");
                yield return Wait(() => enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, 3, "near fan passed");
                Equal(hp, health.CurrentHealth, "native near sidestep damage zero");
                Pass($"NEAR {direction:0}deg/side{sideSign}: actual 2m start and native walk perpendicular to committed fan avoids volley, damage0");
            }
            Set(settings, "spawnGraceSeconds", 10); Spawn();
            foreach (var sample in new[] { (40f, EnemyFacing.Right), (48f, EnemyFacing.Right), (54f, EnemyFacing.Back),
                (46f, EnemyFacing.Back), (36f, EnemyFacing.Right), (320f, EnemyFacing.Right), (312f, EnemyFacing.Right),
                (306f, EnemyFacing.Front), (314f, EnemyFacing.Front), (324f, EnemyFacing.Right) })
            {
                Move(origin + Aim(sample.Item1) * 4); yield return null; yield return null;
                Require(enemy.PatternFacing == sample.Item2, "hysteresis at " + sample.Item1);
                Require(Body().sprite.name.Contains(sample.Item2 == EnemyFacing.Back ? "Back" : sample.Item2 == EnemyFacing.Front ? "Front" : "Side"), "actual directional sprite");
            }
            Restore(); enemy.ReleaseToPool(); Pass("FACING: actual near-boundary motion retains view inside 8deg hysteresis and changes at both front/back boundaries");
            foreach (float direction in new[] { 45f, 135f, 225f, 315f }) foreach (float edge in new[] { -.025f, .025f })
            {
                Spawn(); Move(origin + Aim(direction) * 4); yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 2, "diagonal edge tell");
                Move(origin - Aim(direction) * 3); yield return Wait(() => pattern.VolleyCount == 1, 2, "diagonal edge release");
                var pellet = pattern.ActivePellets[0]; var side = Quaternion.AngleAxis(-90, Vector3.up) * pellet.Direction;
                PlaceAtEdge(pellet.LaunchPosition + pellet.Direction * 3, side, pellet.Radius, edge); float hp = health.CurrentHealth;
                yield return Wait(() => enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, 3, "diagonal edge ends");
                Equal(edge < 0 ? 3 : 0, hp - health.CurrentHealth, "diagonal rounded edge");
                Pass($"EDGE {direction:0}deg/{edge:+.000;-.000}m: actual capsule swept damage={(edge < 0 ? 3 : 0)}");
            }
            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.VolleyCount == 1, 3, "dash release");
            yield return new WaitForSeconds(.45f); var central = pattern.ActivePellets[1]; var volley = pattern.LastVolley;
            Move(central.transform.position); health.ResetHealth(); float full = health.CurrentHealth;
            Face(Vector3.Cross(central.Direction, Vector3.up));
            player.StartCoroutine((IEnumerator)typeof(PlayerController).GetMethod("DashCoroutine", Private).Invoke(player, null));
            Require(player.IsDashInvincible, "native dash active");
            yield return Wait(() => volley.HitAttempts == 1, .4f, "dash collision consumes attempt"); Equal(full, health.CurrentHealth, "dash actual collision harmless");
            yield return new WaitForSeconds(.35f); Require(!player.IsDashInvincible, "dash ended");
            var other = pattern.ActivePellets.First(p => p != null && p.gameObject.activeSelf); Move(other.transform.position + other.Direction * .6f);
            yield return Wait(() => other == null || !other.gameObject.activeSelf, 1, "post-dash pellet collision");
            Equal(full, health.CurrentHealth, "no delayed same volley hit"); Pass("DASH: native dash absorbs actual collision; later pellet cannot damage after dash ends");

            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 2, "rapid tell");
            float began = Time.time; var locked = pattern.LockedDirection;
            while (pattern.VolleyCount == 0)
            {
                enemy.TakeDamage(1); enemy.ApplyKnockback(Vector3.left, .3f);
                Require(Time.time - began < 1.05f && pattern.LockedDirection == locked, "rapid hits do not extend or retarget tell");
                Equal(0, Vector3.Distance(origin, enemy.GetComponent<Rigidbody>().position), "no windup displacement");
                yield return new WaitForSeconds(.04f);
            }
            var moving = pattern.ActivePellets[1]; var pos = moving.transform.position;
            enemy.StatusEffects.ApplyPoison(.8f, .1f, 1); enemy.StatusEffects.ApplyStun(3);
            Move(moving.transform.position + moving.Direction * 2.6f); health.ResetHealth(); full = health.CurrentHealth;
            yield return new WaitForSeconds(.2f); Require(Vector3.Distance(pos, moving.transform.position) > .5f, "stunned owner's projectile moves");
            yield return Wait(() => pattern.LastVolley.HitAttempts == 1, 2, "stun projectile hit"); Equal(3, full - health.CurrentHealth, "one hit during owner stun");
            yield return Wait(() => pattern.Phase == PollenInvaderPhase.Ready, 3, "stun recovery");
            Require(pattern.VolleyCount == 1 && !enemy.IsPatternPositionLocked && !enemy.IsPatternFacingLocked, "no stuck lock or extra volley");
            Pass("RAPID/POISON/STUN: tell immutable, owner vulnerable, released pellets continue once and recovery unlocks");

            Set(settings, "rearmSeconds", 0); Set(settings, "recoverySeconds", .1f); Set(settings, "pelletLifetimeSeconds", .6f);
            Spawn(); Move(origin + Vector3.right * 4); float until = Time.time + 6;
            while (pattern.VolleyCount < 2)
            {
                Require(Time.time < until, "second zero-rearm cycle"); Require(Object.FindObjectsByType<PollenPellet>(FindObjectsSortMode.None).Length <= 3, "three live pellets maximum");
                Require(enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount <= 3, "owned projectile upper bound"); yield return null;
            }
            Restore(); enemy.ReleaseToPool(); Pass("ZERO REARM: two complete cycles, <=3 live pellets, no ownership accumulation");
            yield return SlowMuzzleCheck();
            yield return FrameRateChecks();
        }
        private static void PlaceAtEdge(Vector3 point, Vector3 side, float radius, float edge)
        {
            Move(point); Require(CombatHitGeometry.TryCapsule(player.HitCollider, out var a, out var b, out float r), "actual capsule");
            var axis = CombatHitGeometry.Flat(side); var center = CombatHitGeometry.Flat(player.transform.position);
            float reach = Mathf.Max(Vector2.Dot(a - center, -axis), Vector2.Dot(b - center, -axis)) + r;
            Move(point + side * (reach + radius + edge)); health.ResetHealth();
        }
        private static IEnumerator SlowMuzzleCheck()
        {
            Set(settings, "pelletSpeed", .8f); Spawn(); Move(origin + Vector3.back * 4);
            yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 2, "slow tell"); Move(origin + Vector3.right * 4);
            yield return Wait(() => pattern.VolleyCount == 1, 2, "slow release"); yield return new WaitForSeconds(.55f); yield return new WaitForEndOfFrame();
            var pellet = pattern.ActivePellets[1]; Require(pellet != null && pellet.UsesOpeningSort && pellet.Body.sortingOrder > Body().sortingOrder, "slow front pellet remains visible while still on near shell");
            CheckPellet(pellet, .16f, .8f);
            yield return new WaitForSeconds(1.2f); Require(pellet != null && !pellet.UsesOpeningSort, "slow pellet returns to world sort after leaving shell");
            Restore(); enemy.ReleaseToPool(); Pass("SLOW MUZZLE: speed .8 stays visible over own front shell until exit, then restores world sorting without altering height/radius");
        }
        private static IEnumerator FrameRateChecks()
        {
            int rate = Application.targetFrameRate, sync = QualitySettings.vSyncCount; var saved = origin;
            try
            {
                QualitySettings.vSyncCount = 0;
                foreach (int fps in new[] { 60, 15 }) foreach (float edge in new[] { -.025f, .025f })
                {
                    Application.targetFrameRate = fps;
                    var pellet = LaunchProbe(saved, Vector3.right, 12, .4f); var trace = pellet.gameObject.AddComponent<PollenFlightTrace>(); trace.Initialize(pellet);
                    PlaceAtEdge(saved + Vector3.right * 2.4f, Vector3.forward, pellet.Radius, edge); float hp = health.CurrentHealth;
                    yield return Wait(() => trace.Finished, 2, "fast flight finishes"); Equal(edge < 0 ? 3 : 0, hp - health.CurrentHealth, "fast rounded edge");
                    if (edge > 0) Equal(4.8f, Vector3.Distance(saved, trace.LastPosition), "lifetime-clamped full travel");
                    if (fps == 15) Require(trace.MaxDelta > .045f, "actual low FPS observed");
                    Require(trace.MaxHeightError < .005f, "fast flight remains low");
                    Pass($"FRAME {fps}fps/{edge:+.000;-.000}m: speed12, max dt={trace.MaxDelta:F3}s, actual damage={(edge < 0 ? 3 : 0)}, low grounded flight and exact lifetime");
                }
            }
            finally { Application.targetFrameRate = rate; QualitySettings.vSyncCount = sync; origin = saved; }
        }
    }

    public sealed class PollenFlightTrace : MonoBehaviour
    {
        private PollenPellet pellet;
        public Vector3 LastPosition;
        public float MaxDelta, MaxHeightError;
        public bool Finished;
        public void Initialize(PollenPellet source) { pellet = source; LastPosition = transform.position; }
        private void LateUpdate()
        {
            LastPosition = transform.position; MaxDelta = Mathf.Max(MaxDelta, Time.deltaTime);
            if (pellet != null) MaxHeightError = Mathf.Max(MaxHeightError, Mathf.Abs(pellet.Body.transform.position.y - BiomeManager.Active.GetGroundHeight(transform.position) - pellet.Radius));
        }
        private void OnDisable() { LastPosition = transform.position; Finished = true; }
    }
}
