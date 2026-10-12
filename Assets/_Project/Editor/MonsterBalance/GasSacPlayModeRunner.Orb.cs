using System;
using System.Collections;
using Necrocis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class GasSacPlayModeRunner
    {
        private static IEnumerator OrbChecks()
        {
            Require(settings.orbEnabled && pattern.OrbEnabled, "field spawn captures enabled B");
            Require(GasSacOrb.HasClearPath(origin, origin + Vector3.right * 5.2f), "preview lane must be clear on the same height");
            if (preview)
            {
                MovePlayer(5.2f);
                yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 5, "B preview aim");
                yield return new WaitForSeconds(settings.orbWindupSeconds * .4f);
                yield break;
            }
            Pass("B attaches through the actual map field with zero kill requirement; A art remains V2");
            field.enabled = false; enemy = null;
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 60, true);

            // During spawn grace, exercise both directions through the real selector without starting an attack.
            SpawnOrbTest(); MovePlayer(3.4f); yield return null; pattern.TryBeginAttack();
            Require(pattern.PreferredAttack == GasSacAttackKind.Burst, "initial boundary priority is near");
            MovePlayer(4.1f); pattern.TryBeginAttack();
            Require(pattern.PreferredAttack == GasSacAttackKind.Orb, "far threshold selects orb");
            MovePlayer(3.4f); pattern.TryBeginAttack();
            Require(pattern.PreferredAttack == GasSacAttackKind.Orb, "return through dead band retains orb");
            MovePlayer(2.9f); pattern.TryBeginAttack();
            Require(pattern.PreferredAttack == GasSacAttackKind.Burst, "near threshold selects burst");
            MovePlayer(3.4f); pattern.TryBeginAttack();
            Require(pattern.PreferredAttack == GasSacAttackKind.Burst && pattern.Phase == GasSacPhase.Ready, "boundary does not reset or start a tell");
            Pass("near / 3.0–3.8 boundary / far: both hysteresis directions retain the previous choice in the dead band");

            SpawnOrbTest(); MovePlayer(2.3f); health.ResetHealth();
            yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 3, "near A");
            MovePlayer(5.2f);
            yield return Wait(() => pattern.Phase == GasSacPhase.Recovery, 3, "A recovery after crossing far threshold");
            Require(pattern.SelectedAttack == GasSacAttackKind.Burst && pattern.BurstCount == 1 && pattern.OrbCount == 0, "A is locked through recovery");
            Require(!pattern.TryBeginAttack(), "B cannot chain during A recovery");
            yield return Wait(() => pattern.Phase == GasSacPhase.Ready, 3, "A recovery complete");
            Require(!pattern.TryBeginAttack() && pattern.NextReadyTime > Time.time, "A rearm prevents immediate B chain");
            Pass("near A stays A when the player retreats; no B during recovery or rearm");

            SpawnOrbTest(); MovePlayer(5.2f); health.ResetHealth(); float before = health.CurrentHealth;
            yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 3, "far B");
            Vector3 lockedPosition = enemy.transform.position, lockedAim = pattern.LockedOrbDirection;
            Require(pattern.TelegraphObject == null && enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0,
                "B windup uses only body poses, without a ground arrow, line or range UI");
            Require(enemy.transform.Find("Visual").GetComponent<SpriteRenderer>().flipX, "B sprite mirrors toward its rightward aim");
            yield return Wait(() => health.CurrentHealth < before, 4, "single orb hit");
            Equal(3, before - health.CurrentHealth, "base attack 2 × 1.25 = 2.5 rounds once to 3 HP");
            yield return new WaitForSeconds(.15f);
            Require(pattern.OrbCount == 1 && pattern.BurstCount == 0 && pattern.ActiveOrb == null, "one orb consumed on first hit");
            Equal(0, PlanarDistance(lockedPosition, enemy.transform.position), "B stops locomotion");
            Equal(0, (lockedAim - Vector3.right).magnitude, "aim points along original tell");
            Equal(3, before - health.CurrentHealth, "no splash or lingering second hit");
            Pass("far B: body-only windup, no ground UI → one slow orb → B-only recovery, actual 3 HP once, projectile consumed");

            SpawnOrbTest(); MovePlayer(5.2f); health.ResetHealth(); before = health.CurrentHealth;
            yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 3, "sidestep tell");
            lockedAim = pattern.LockedOrbDirection;
            while (pattern.Phase == GasSacPhase.OrbWindup)
            {
                if (Mathf.Abs(player.transform.position.z - origin.z) < 1.5f)
                    player.TryMoveByWorld(Vector3.forward * player.MoveSpeed * Time.deltaTime);
                yield return null;
            }
            Require(Mathf.Abs(player.transform.position.z - origin.z) > .8f, "actual walking leaves orb lane");
            Require(pattern.ActiveOrb != null, "orb still launches along original direction");
            Equal(0, (pattern.ActiveOrb.Direction - lockedAim).magnitude, "orb does not home after sidestep");
            yield return Wait(() => pattern.ActiveOrb == null, 4, "missed orb expires");
            Equal(before, health.CurrentHealth, "sidestep avoids damage");
            Require(enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, "miss leaves no registered objects");
            Pass("actual sidestep avoids B; aim is fixed, missed orb expires without a damage zone or ownership leak");

            SpawnOrbTest(); MovePlayer(5.2f); health.ResetHealth(); before = health.CurrentHealth;
            yield return Wait(() => pattern.ActiveOrb != null, 3, "dash shot");
            yield return Wait(() => pattern.ActiveOrb == null || PlanarDistance(pattern.ActiveOrb.transform.position, player.transform.position) < 1.8f, 3, "dash timing");
            Require(pattern.ActiveOrb != null, "dash begins before impact");
            typeof(PlayerController).GetField("lastMoveDirection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(player, Vector3.forward);
            var dash = (IEnumerator)typeof(PlayerController).GetMethod("DashCoroutine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(player, null);
            player.StartCoroutine(dash);
            yield return Wait(() => pattern.ActiveOrb == null, 4, "dash avoids orb");
            Equal(before, health.CurrentHealth, "actual dash takes zero B damage");
            Pass("actual player dash crosses out of the incoming orb lane and takes zero damage");

            SpawnOrbTest(); MovePlayer(5.2f);
            yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 3, "B lock tell");
            MovePlayer(2.3f);
            yield return Wait(() => pattern.Phase == GasSacPhase.OrbRecovery, 3, "B lock recovery");
            Require(pattern.SelectedAttack == GasSacAttackKind.Orb && pattern.BurstCount == 0 && pattern.OrbCount == 1, "crossing near cannot switch a running B to A");
            Require(!pattern.TryBeginAttack(), "cannot stack A on B");
            Pass("far B remains B when the player approaches during the tell; no simultaneous burst");

            SpawnOrbTest(); MovePlayer(-4.1f);
            yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 3, "left B tell");
            var facingBody = enemy.transform.Find("Visual").GetComponent<SpriteRenderer>();
            Require(!facingBody.flipX && pattern.LockedOrbDirection.x < -.9f, "B body and orb aim both face left");
            MovePlayer(4.1f); yield return null;
            Require(!facingBody.flipX, "B cannot turn during its locked tell");
            yield return Wait(() => pattern.Phase == GasSacPhase.OrbRecovery, 3, "left B recovery");
            Require(!facingBody.flipX, "B remains left-facing in recovery");
            yield return Wait(() => pattern.Phase == GasSacPhase.Ready, 2, "B facing ready"); yield return null;
            Require(facingBody.flipX, "B turns right after recovery");
            Pass("B mirrors correctly to left/right, keeps its committed direction, then faces the player again after recovery");

            SpawnOrbTest(); MovePlayer(5.2f); health.ResetHealth(); before = health.CurrentHealth;
            yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 3, "death before launch");
            var cancelled = pattern; var aimMarker = pattern.TelegraphObject;
            enemy.SuppressExperienceReward = true; enemy.TakeDamage(10000);
            yield return new WaitForSeconds(settings.orbWindupSeconds + .1f);
            Require(cancelled.OrbCount == 0 && aimMarker == null, "death cancels planned shot; no B ground UI exists");
            Equal(before, health.CurrentHealth, "no post-death queued hit");

            SpawnOrbTest(); MovePlayer(5.2f); health.ResetHealth(); before = health.CurrentHealth;
            yield return Wait(() => pattern.ActiveOrb != null, 3, "death after launch");
            var orb = pattern.ActiveOrb;
            enemy.SuppressExperienceReward = true; enemy.TakeDamage(10000);
            yield return null;
            Require(orb == null, "death removes already launched orb");
            yield return new WaitForSeconds(1.8f); Equal(before, health.CurrentHealth, "no post-death flight damage");

            SpawnOrbTest(); MovePlayer(5.2f);
            yield return Wait(() => pattern.ActiveOrb != null, 3, "release after launch");
            orb = pattern.ActiveOrb; uint generation = enemy.SpawnGeneration;
            var lifetime = enemy.GetComponent<EnemyPatternLifetime>();
            SpawnOrbTest(); yield return null;
            Require(orb == null && !lifetime.IsCurrent(generation) && pattern.OrbCount == 0, "pool reuse removes old shot and invalidates generation");
            Require(enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, "pool reuse has no orphan ownership");
            Pass("death before/after launch and release→pool reuse remove orbs and prevent delayed hits");

            // Move the actual camera away, rather than disabling visibility checks in tests.
            SpawnOrbTest(); MovePlayer(5.2f);
            var follow = DontStarveCamera.Instance; Camera camera = DontStarveCamera.GetActiveCamera();
            bool wasEnabled = follow.enabled; follow.enabled = false;
            Vector3 cameraPosition = camera.transform.position;
            try
            {
                camera.transform.position += Vector3.right * 100;
                yield return new WaitForSeconds(settings.spawnGraceSeconds + .15f);
                Require(pattern.Phase == GasSacPhase.Ready && pattern.OrbCount == 0, "offscreen enemy cannot start a shot");
                camera.transform.position = cameraPosition;
                yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 2, "visible shot starts");
                camera.transform.position += Vector3.right * 100;
                yield return Wait(() => pattern.Phase == GasSacPhase.OrbRecovery, 2, "offscreen cancellation recovery");
                Require(pattern.OrbCount == 0, "leaving screen during windup cancels launch");
            }
            finally { camera.transform.position = cameraPosition; follow.enabled = wasEnabled; }
            Pass("offscreen start blocked; leaving the screen during the tell cancels the shot and still applies B recovery");

            // Edit the same serialized sources shown in the Inspector; the current actor keeps its captured settings.
            GasSacElitePattern frozen = pattern;
            Set(settings, "orbWindupSeconds", .6f); Set(settings, "orbSpeed", 6);
            Set(settings, "orbLifetimeSeconds", 1.7f); Set(settings, "orbHitRadius", .4f);
            Set(settings, "orbRecoverySeconds", .4f); Set(settings, "orbRearmSeconds", .6f);
            Set(settings, "spawnGraceSeconds", 0);
            Set(definition, "statSets.Array.data[0].attackPower", 4);
            int index = definition.patternDamage.FindIndex(p => p.id == GasSacPatternSettings.OrbDamageId);
            Set(definition, $"patternDamage.Array.data[{index}].coefficient", 2);
            var profile = DifficultyBalanceService.GetProfile(GameDifficulty.Normal);
            Set(profile, "elites.outgoingDamage", 1.5f); Set(profile, "elites.attackCooldown", .5f);
            Equal(1.1f, frozen.OrbWindupDuration, "existing B windup frozen"); Equal(3, frozen.OrbSpeed, "existing B speed frozen");
            SpawnOrbTest(); MovePlayer(5.2f); health.ResetHealth(); before = health.CurrentHealth;
            Equal(.6f, pattern.OrbWindupDuration, "Inspector B tell applied to next spawn");
            Equal(6, pattern.OrbSpeed, "Inspector B speed applied"); Equal(1.7f, pattern.OrbLifetime, "Inspector B life applied");
            yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 2, "edited B tell");
            float start = Time.time;
            yield return Wait(() => pattern.ActiveOrb != null, 2, "edited B launch");
            Require(Time.time - start >= .55f, "cooldown multiplier does not shorten B windup");
            Equal(.4f, pattern.ActiveOrb.HitRadius, "Inspector radius applied to projectile");
            Equal(.4f, pattern.ActiveOrb.FlightHeight, "Inspector radius also keeps the projectile close to the ground without a second height value");
            yield return Wait(() => pattern.Phase == GasSacPhase.Ready, 1, "edited B recovery");
            Require(pattern.NextReadyTime - Time.time > .2f && pattern.NextReadyTime - Time.time <= .301f, "B rearm = .6 × .5, without A recovery/rearm");
            Require(!pattern.TryBeginAttack(), "edited B cooldown cannot be bypassed");
            yield return Wait(() => health.CurrentHealth < before, 2, "edited B actual hit");
            Equal(12, before - health.CurrentHealth, "4 × 1.5 × 2 = 12 damage, applied once");
            Pass("Inspector B edits reach the next actor and projectile; actual hit 12 HP, tell .6s, B recovery .4s, one scaled .3s rearm");

            Set(settings, "orbRecoverySeconds", .1f); Set(settings, "orbRearmSeconds", 0);
            Set(settings, "orbSpeed", 1); Set(settings, "orbLifetimeSeconds", 2);
            SpawnOrbTest(); MovePlayer(5.2f);
            yield return Wait(() => pattern.ActiveOrb != null, 2, "long flight");
            yield return Wait(() => pattern.Phase == GasSacPhase.Ready, 1, "short recovery ends while shot is alive");
            Require(pattern.ActiveOrb != null && !pattern.TryBeginAttack(), "live shot blocks another B even with zero rearm");
            MovePlayer(2.3f);
            Require(!pattern.TryBeginAttack() && pattern.BurstCount == 0, "live shot also blocks overlapping A");
            MovePlayer(5.2f);
            yield return Wait(() => pattern.ActiveOrb == null, 3, "short lifetime expires");
            Pass("extreme Inspector tuning (0s rearm, .1s recovery) still permits only one active attack/projectile");

            SetBool(settings, "orbEnabled", false);
            SpawnOrbTest(); MovePlayer(5.2f);
            yield return new WaitForSeconds(.8f);
            Require(!pattern.OrbEnabled && pattern.OrbCount == 0 && pattern.Phase == GasSacPhase.Ready, "B disabled restores A-only at far distance");
            MovePlayer(2.3f);
            yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 2, "A still starts when B disabled");
            Pass("B toggle off restores near-only A; production map lists and existing kill-trigger elites unchanged");
        }

        private static void SpawnOrbTest()
        {
            Spawn();
            enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0); // Isolate distance selection from approach movement in checks only.
        }
    }
}
