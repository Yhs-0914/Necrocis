using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Necrocis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class DustClumpConnectionRunner
    {
        private static readonly float[] BattleAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };
        private static IEnumerator BattleChecks(string section)
        {
            if (section == "all" || section == "states") yield return StateChecks();
            if (section == "all" || section == "terrain") { yield return TerrainChecks(); yield return RoadChecks(); }
            if (section == "all" || section == "death") yield return DirectionDeathChecks();
            if (section == "all" || section == "field") yield return FieldUnloadChecks();
            if (section == "visits") { yield return CleanupChecks(); yield return VisitChecks(); }
        }
        private static IEnumerator WalkTo(Vector3 target, float seconds)
        {
            float until = Time.time + seconds;
            while (true)
            {
                Vector3 delta = target - player.transform.position; delta.y = 0;
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
                Spawn(); enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 1); Move(origin + Aim(direction) * 6);
                Vector3 start = enemy.GetComponent<Rigidbody>().position;
                yield return Wait(() => pattern.Phase == DustClumpPhase.Windup, 4, "native approach");
                Require(Vector3.Dot(enemy.GetComponent<Rigidbody>().position - start, Aim(direction)) > 1.3f && enemy.IsPatternPositionLocked && !Body().flipX,
                    "actual approach enters range then freezes without turning sprite");
                Pass($"PURSUIT {direction:0}deg: actual base-speed approach enters trigger distance and fixes the core");
            }
            foreach (float direction in BattleAngles)
            {
                Vector3 aim = Aim(direction), side = Vector3.Cross(aim, Vector3.up);
                Spawn(); Move(origin + aim * 3.6f); yield return Wait(() => pattern.ActiveCloud != null, 3, "detour cloud"); float hp = health.CurrentHealth;
                yield return WalkTo(origin + aim * 3.6f + side * 1.7f, 1);
                yield return WalkTo(origin - aim * 1.1f + side * 1.7f, 1);
                yield return WalkTo(origin - aim * 1.1f, 1);
                Require(pattern.Phase == DustClumpPhase.Exposed, "detour reaches exposed opportunity");
                Face(aim); Physics.SyncTransforms(); float old = enemy.Stats.CurrentHealth;
                typeof(PlayerAttack).GetMethod("MeleeAttack", Private).Invoke(player.GetComponent<PlayerAttack>(), null);
                Require(enemy.Stats.CurrentHealth < old && pattern.ReleaseCount == 1, "native Q punishes core without creating another cloud");
                Equal(hp, health.CurrentHealth, "detour and core attack remain safe");
                yield return WalkTo(origin - aim * 1.8f, 1); // Leave space before the full body reforms.
                yield return Wait(() => pattern.Phase == DustClumpPhase.Ready, 4, "detour finish"); Equal(hp, health.CurrentHealth, "detour damage zero");
                Pass($"DETOUR {direction:0}deg: actual walking avoids cloud, native Q damages exposed core, damage 0");
            }
            foreach (float direction in BattleAngles) foreach (float edge in new[] { -.025f, .025f })
            {
                Spawn(); Vector3 aim = Aim(direction), side = Vector3.Cross(aim, Vector3.up); Move(origin + aim * 3.6f);
                yield return Wait(() => pattern.ActiveCloud != null, 3, "directional edge cloud"); var cloud = pattern.ActiveCloud;
                Move(cloud.LaunchPosition + aim * 1.3f + side * (settings.cloudRadius + edge)); health.ResetHealth(); float hp = health.CurrentHealth;
                yield return Wait(() => cloud.IsDissolving, 3, "directional edge traversal");
                Equal(edge < 0 ? 3 : 0, hp - health.CurrentHealth, "directional round edge");
                Pass($"EDGE {direction:0}deg/{edge:+.000;-.000}m: actual moving-circle damage={(edge < 0 ? 3 : 0)}");
            }
            foreach (bool dash in new[] { true, false })
            {
                Spawn(); Move(origin + Vector3.right * 3.6f); yield return Wait(() => pattern.ActiveCloud != null, 3, "invulnerability cloud"); var cloud = pattern.ActiveCloud;
                Move(cloud.transform.position); health.ResetHealth(); float hp = health.CurrentHealth;
                if (dash)
                {
                    Face(Vector3.right); player.StartCoroutine((IEnumerator)typeof(PlayerController).GetMethod("DashCoroutine", Private).Invoke(player, null));
                    Require(player.IsDashInvincible, "native dash activates invulnerability");
                }
                else health.GrantTemporaryInvincibility(.15f);
                yield return Wait(() => cloud.HitAttempts == 1, .5f, "invulnerable contact consumes attempt");
                Equal(hp, health.CurrentHealth, "invulnerable contact does not damage");
                yield return new WaitForSeconds(.22f); Move(cloud.transform.position); yield return new WaitForSeconds(.12f);
                Require(!player.IsDashInvincible && !health.IsInvincible && cloud.HitAttempts == 1 && cloud.DamageApplications == 0, "no delayed re-hit after invulnerability");
                Equal(hp, health.CurrentHealth, "reentry into same cloud remains safe");
                Pass(dash ? "DASH: real dash ignores first cloud contact; same cloud cannot hit after dash ends" : "INVULNERABILITY: temporary hurt protection absorbs first contact; no delayed retry/tick");
            }
            Spawn(); Move(origin + Vector3.right * 3.6f); yield return Wait(() => pattern.Phase == DustClumpPhase.Windup, 2, "rapid fixture");
            float began = Time.time;
            while (pattern.ReleaseCount == 0)
            {
                enemy.TakeDamage(1); enemy.ApplyKnockback(Vector3.left, .3f);
                Require(Time.time - began < 1.1f, "rapid hits do not delay windup"); Equal(0, Vector3.Distance(origin, enemy.GetComponent<Rigidbody>().position), "rapid knockback cannot move core");
                yield return new WaitForSeconds(.04f);
            }
            enemy.StatusEffects.ApplyPoison(.8f, .1f, 1); enemy.StatusEffects.ApplyStun(3);
            var moving = pattern.ActiveCloud; Vector3 cloudStart = moving.transform.position;
            Move(moving.LaunchPosition + Vector3.right * 2); health.ResetHealth(); float before = health.CurrentHealth;
            yield return new WaitForSeconds(.3f);
            Require(Vector3.Distance(cloudStart, moving.transform.position) > .5f && pattern.ReleaseCount == 1, "released cloud keeps moving through owner stun");
            yield return Wait(() => moving.HitAttempts == 1, 2, "stunned-owner cloud hit"); Equal(3, before - health.CurrentHealth, "stun does not duplicate/cancel released damage");
            yield return Wait(() => pattern.Phase == DustClumpPhase.Ready, 4, "stun recovery");
            Require(!enemy.IsPatternPositionLocked && pattern.ReleaseCount == 1, "one cycle and unlock after stun");
            Pass("RAPID/POISON/STUN: windup unchanged, fixed vulnerable core, released cloud keeps clock/hit, no extra wave or stuck lock");

            Set(settings, "rearmSeconds", 0); Set(settings, "recoverySeconds", .1f); Set(settings, "cloudLifetimeSeconds", .6f);
            Spawn(); Move(origin + Vector3.right * 3.6f); float deadline = Time.time + 6;
            while (pattern.ReleaseCount < 2)
            {
                Require(Time.time < deadline, "second zero-rearm cycle occurs");
                Require(Object.FindObjectsByType<DustCloud>(FindObjectsSortMode.None).Length <= 1, "one live cloud upper bound");
                Require(enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount <= 2, "no accumulated owned objects"); yield return null;
            }
            Restore(); enemy.ReleaseToPool();
            Pass("ZERO REARM: two full cycles with one live cloud maximum and no owned-object accumulation");
            Spawn(); Move(origin + Vector3.right * 3.6f); yield return Wait(() => pattern.ActiveCloud != null, 3, "fade fixture");
            var fading = pattern.ActiveCloud; Move(origin + Vector3.left * 3); yield return Wait(() => fading.IsDissolving, 3, "fade starts");
            Require(!pattern.TelegraphObject.activeSelf, "fade hides damage area"); Move(fading.transform.position); health.ResetHealth(); float full = health.CurrentHealth;
            yield return Wait(() => fading == null, 1, "fade completes"); Equal(full, health.CurrentHealth, "fade cannot deal damage");
            Pass("DISSOLVE: both final visual frames are harmless and have no red damage marker");
            yield return VisibilityChecks();
            yield return FrameRateChecks();
        }
        private static Rect ScreenBounds(SpriteRenderer renderer)
        {
            var camera = DontStarveCamera.GetActiveCamera(); var b = renderer.sprite.bounds;
            Vector2 min = Vector2.one * float.PositiveInfinity, max = Vector2.one * float.NegativeInfinity;
            foreach (float x in new[] { b.min.x, b.max.x }) foreach (float y in new[] { b.min.y, b.max.y })
            { Vector2 p = camera.WorldToViewportPoint(renderer.transform.TransformPoint(new Vector3(x, y, 0))); min = Vector2.Min(min, p); max = Vector2.Max(max, p); }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
        private static IEnumerator VisibilityChecks()
        {
            Spawn(); Move(origin + Vector3.forward * 3.6f); yield return Wait(() => pattern.ActiveCloud != null, 3, "visibility cloud"); var cloud = pattern.ActiveCloud;
            Move(cloud.transform.position + Vector3.forward * .35f); health.GrantTemporaryInvincibility(5);
            yield return new WaitForSeconds(.15f);
            Require(cloud.IsPlayerOccluded, "actual player sprite overlaps dust in screen space"); Equal(settings.presentation.playerOverlapOpacity, cloud.Body.color.a, "occlusion alpha cap");
            CheckMarker(settings.cloudRadius); Equal(2, cloud.Speed, "fade does not affect speed");
            Move(origin + Vector3.left * 3); yield return new WaitForSeconds(.15f);
            Require(!cloud.IsPlayerOccluded, "player leaves projected cloud"); Equal(settings.presentation.cloudOpacity, cloud.Body.color.a, "original alpha restores");
            health.ResetHealth();
            Pass("VISIBILITY: actual player overlap fades dust to .35; away from player restores .85; radius/speed unaffected");
        }
        private static IEnumerator FrameRateChecks()
        {
            int oldRate = Application.targetFrameRate, oldSync = QualitySettings.vSyncCount;
            try
            {
                QualitySettings.vSyncCount = 0;
                foreach (int fps in new[] { 60, 15 }) foreach (float edge in new[] { -.025f, .025f })
                {
                    Application.targetFrameRate = fps; Set(settings, "cloudSpeed", 12); Set(settings, "cloudLifetimeSeconds", .4f);
                    Spawn(); Move(origin + Vector3.right * 3.6f); yield return Wait(() => pattern.ActiveCloud != null, 3, "fast cloud"); var cloud = pattern.ActiveCloud;
                    Move(cloud.LaunchPosition + Vector3.right * 2.4f + Vector3.forward * (1.1f + edge)); health.ResetHealth(); float hp = health.CurrentHealth;
                    float maxDelta = 0;
                    while (!cloud.IsDissolving) { maxDelta = Mathf.Max(maxDelta, Time.deltaTime); yield return null; }
                    Equal(edge < 0 ? 3 : 0, hp - health.CurrentHealth, "fast swept edge");
                    Equal(4.8f, Vector3.Distance(cloud.LaunchPosition, cloud.transform.position), "travel distance uses lifetime exactly");
                    if (fps == 15) Require(maxDelta > .045f, "actual slow frame observed");
                    Pass($"FRAME {fps}fps/{edge:+.000;-.000}m: real max dt={maxDelta:F3}s, 12m/s swept edge damage={(edge < 0 ? 3 : 0)}, total travel4.8m");
                }
            }
            finally { Application.targetFrameRate = oldRate; QualitySettings.vSyncCount = oldSync; Restore(); }
        }

        private static IEnumerator DirectionDeathChecks()
        {
            foreach (float direction in BattleAngles) foreach (bool exposed in new[] { false, true })
                yield return DeathCase(direction, exposed ? "cloud" : "early");
            foreach (string phase in new[] { "gather", "fade", "recovery" }) yield return DeathCase(90, phase);
        }
        private static IEnumerator DeathCase(float direction, string phase)
        {
            Spawn(); Move(origin + Aim(direction) * 3.6f); yield return Wait(() => pattern.Phase == DustClumpPhase.Windup, 2, "death windup");
            if (phase == "gather") yield return Wait(() => pattern.CoreExposed, 1, "gather core exposed");
            if (phase == "cloud" || phase == "fade" || phase == "recovery") yield return Wait(() => pattern.ActiveCloud != null, 2, "death live cloud");
            if (phase == "fade") { var c = pattern.ActiveCloud; Move(origin - Aim(direction) * 3); yield return Wait(() => c.IsDissolving, 3, "death fade"); }
            if (phase == "recovery") { Move(origin - Aim(direction) * 3); yield return Wait(() => pattern.Phase == DustClumpPhase.Recovery, 3, "death recovery"); }
            bool isCore = phase != "early" && phase != "recovery";
            var frames = enemy.Config.deathSprites; Require(frames.All(f => f.name.Contains(isCore ? "CoreDeath" : "Clump_Death")), "death uses current visual state");
            var body = Body(); var life = enemy.GetComponent<EnemyPatternLifetime>(); uint gen = enemy.SpawnGeneration;
            enemy.TakeDamage(10000); Require(life.OwnedObjectCount == 0 && !enemy.IsPatternPositionLocked, "death clears ownership and lock immediately");
            Move(origin); health.ResetHealth(); float hp = health.CurrentHealth; var seen = new HashSet<Sprite>(); float until = Time.time + 2;
            while (enemy.gameObject.activeSelf) { Require(Time.time < until && !body.flipX, "death animation finishes unmirrored"); seen.Add(body.sprite); yield return null; }
            Require(frames.All(seen.Contains), "six correct death frames"); Equal(hp, health.CurrentHealth, "no late area or corpse damage");
            Spawn(); Require(enemy.SpawnGeneration != gen && !pattern.CoreExposed && !enemy.IsPatternPositionLocked && pattern.ReleaseCount == 0, "fresh pooled generation");
            Pass($"DEATH {direction:0}deg/{phase}: six {(isCore ? "core" : "cluster")} frames, instant cleanup/no late hit, clean reuse");
        }
        private static IEnumerator FieldUnloadChecks()
        {
            Vector3 saved = origin;
            foreach (bool duringCloud in new[] { false, true })
            {
                if (enemy != null) enemy.ReleaseToPool(); origin = fieldOrigin; field.enabled = true;
                Move(origin + Vector3.right * 3.6f); field.Refresh(player.transform.position);
                enemy = field.Spawners.Single().ActiveEnemy; pattern = enemy.GetComponent<DustClumpElitePattern>();
                yield return Wait(() => duringCloud ? pattern.ActiveCloud != null : pattern.Phase == DustClumpPhase.Windup, 3, "real field attack");
                var life = enemy.GetComponent<EnemyPatternLifetime>(); uint gen = enemy.SpawnGeneration; var point = field.Plan.placements.Single();
                Require(life.OwnedObjectCount > 0, "field owns active attack");
                Move(biome.GridToWorldWithHeight(point.x < 150 ? 280 : 15, point.y < 150 ? 280 : 15)); field.Refresh(player.transform.position); yield return null;
                Require(!life.IsCurrent(gen) && life.OwnedObjectCount == 0 && !SaveService.IsBiomeEliteDefeated(point.spawnId), "unload is clean, not a kill");
                Move(origin + Vector3.right * 5); field.Refresh(player.transform.position);
                enemy = field.Spawners.Single().ActiveEnemy; pattern = enemy.GetComponent<DustClumpElitePattern>();
                Require(!enemy.IsPatternPositionLocked && pattern.ReleaseCount == 0 && !pattern.CoreExposed, "fresh returned field actor");
                field.enabled = false; enemy = null;
                Pass(duringCloud ? "FIELD cloud: real chunk unload clears cloud/marker/lock without defeat; clean return" : "FIELD tell: real chunk unload cancels reservation without defeat; clean return");
            }
            origin = saved;
        }
    }
}
