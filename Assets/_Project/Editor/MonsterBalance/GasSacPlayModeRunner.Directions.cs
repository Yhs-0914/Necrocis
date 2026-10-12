using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class GasSacPlayModeRunner
    {
        private static float directionPreviewAngle = 90;
        private static bool directionPreviewBurst;
        public static void RunDirections() => Start(false, true, false, false, true);
        public static void PreviewDirections(float angle = 90, bool burst = false)
        { directionPreviewAngle = angle; directionPreviewBurst = burst; Start(true, true, false, false, true); }

        private static Vector3 Aim(float angle) => new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
        private static SpriteRenderer DirectionBody => enemy.transform.Find("Visual").GetComponent<SpriteRenderer>();

        private static IEnumerator DirectionChecks()
        {
            field.enabled = false; enemy = null;
            origin = FindDirectionPatch();
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 60, true);
            var art = settings.presentation;
            var directions = art.directionalPresentation;
            Require(directions != null && directions.GetValidationError() == null, "complete directional source");
            if (preview)
            {
                SpawnOrbTest(); MovePlayerTo(origin + Aim(directionPreviewAngle) * (directionPreviewBurst ? 2.3f : 4.8f));
                health.ResetHealth();
                if (directionPreviewBurst)
                {
                    yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 4, "directional A preview");
                    yield return new WaitForSeconds(settings.windupSeconds * .65f);
                }
                else
                {
                    yield return Wait(() => pattern.ActiveOrb != null, 4, "directional B preview");
                    yield return new WaitForSeconds(.12f);
                    Require(pattern.ActiveOrb != null, "projectile visible near release");
                }
                yield break;
            }
            Pass("I-01 map-spawn path binds 14 directional keys and dedicated death poses without a kill trigger");
            foreach (float angle in new[] { 0f, 90f, 180f, 270f })
            {
                Vector3 aim = Aim(angle);
                SpawnOrbTest(); MovePlayerTo(origin + aim * 2.3f); health.ResetHealth();
                yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 4, "A directional tell");
                EnemyFacing facing = enemy.PatternFacing;
                Require(enemy.HasPatternDirections && enemy.IsPatternFacingLocked, "A binds and commits facing");
                Require(facing == directions.Capture().Select(aim, DontStarveCamera.GetActiveCamera(), EnemyFacing.Front, false), "A faces target");
                MovePlayerTo(origin - aim * 4.8f);
                yield return Wait(() => pattern.Phase == GasSacPhase.Recovery, 3, "A directional recovery");
                Require(enemy.PatternFacing == facing && enemy.IsPatternFacingLocked
                    && DirectionBody.sprite == directions.Capture().Resolve(art.deflated, facing), "A recovery preserves facing and directional deflated frame");
                Require(pattern.BurstCount == 1 && pattern.OrbCount == 0 && !pattern.TryBeginAttack(), "A cannot stack B during recovery");
                yield return Wait(() => pattern.Phase == GasSacPhase.Ready, 3, "A directional unlock"); yield return null;
                Require(!enemy.IsPatternFacingLocked && enemy.PatternFacing != facing, "A unlocks and faces opposite target after recovery");
                Pass("A " + facing + ": tell/recovery direction fixed; directional recovery; Ready unlock; unchanged single burst");

                SpawnOrbTest(); MovePlayerTo(origin + aim * 4.8f); health.ResetHealth();
                yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 4, "B directional tell");
                facing = enemy.PatternFacing; Vector3 lockedAim = pattern.LockedOrbDirection;
                Require(enemy.IsPatternFacingLocked && pattern.TelegraphObject == null, "B commits with no ground UI");
                MovePlayerTo(origin - aim * 3.8f);
                yield return Wait(() => pattern.ActiveOrb != null, 3, "B directional launch");
                var shot = pattern.ActiveOrb; yield return null;
                Require(shot != null && shot.gameObject.activeSelf, "B orb survives launch into its committed lane");
                Equal(0, (shot.Direction - lockedAim).magnitude, "continuous aim unchanged by sprite sector");
                Require(Vector3.Dot(shot.LaunchPosition - enemy.transform.position, lockedAim) > .5f, "orb originates forward of the grounded body");
                Transform gas = shot.transform.Find("CompressedGas");
                Equal(shot.HitRadius, gas.position.y - biome.GetGroundHeight(gas.position), "flight center remains ground plus one radius");
                Require(shot.GetComponentsInChildren<LineRenderer>().Length == 0 && shot.GetComponentsInChildren<SpriteYSort>().Length == 3,
                    "no path UI; all three visual layers use world depth");
                if (Mathf.Abs(aim.z) > .9f)
                {
                    int[] orders = gas.GetComponentsInChildren<SpriteRenderer>().Select(r => r.sortingOrder).ToArray();
                    Require(aim.z > 0 ? orders.Max() < DirectionBody.sortingOrder : orders.Min() > DirectionBody.sortingOrder,
                        "orb sorts behind/up and in front/down relative to owner: angle=" + angle + ", body="
                        + DirectionBody.sortingOrder + ", orb=" + string.Join(",", orders));
                }
                Require(enemy.PatternFacing == facing && enemy.IsPatternFacingLocked
                    && DirectionBody.sprite == directions.Capture().Resolve(art.deflated, facing), "B recovery direction and sprite retained");
                yield return Wait(() => pattern.Phase == GasSacPhase.Ready, 3, "B directional unlock"); yield return null;
                Require(!enemy.IsPatternFacingLocked && enemy.PatternFacing != facing && pattern.OrbCount == 1, "B unlocks without a second projectile");
                Pass("B " + facing + ": committed aim; forward grounded origin; correct depth; no floor UI; recovery then unlock");
            }
            foreach (bool orb in new[] { false, true })
            {
                SpawnOrbTest(); MovePlayerTo(origin + Vector3.forward * (orb ? 4.8f : 2.3f));
                yield return Wait(() => pattern.Phase == (orb ? GasSacPhase.OrbWindup : GasSacPhase.Windup), 4, "stun cancellation tell");
                enemy.StatusEffects.ApplyStun(.4f);
                yield return Wait(() => pattern.Phase == GasSacPhase.Ready, 2, "stun cancellation unlock");
                Require(!enemy.IsPatternFacingLocked && pattern.BurstCount == 0 && pattern.OrbCount == 0, "cancelled attack releases facing and does not fire");
                Pass((orb ? "B" : "A") + " stun cancellation releases direction lock without a queued hit");
            }

            SpawnOrbTest(); MovePlayerTo(origin + Vector3.back * 4.8f); yield return null;
            enemy.SetPatternFacing(Vector3.back);
            Vector3 frozen = enemy.GetPatternVisualOrigin();
            if (!backups.ContainsKey(directions)) backups.Add(directions, EditorJsonUtility.ToJson(directions));
            using (var serialized = new SerializedObject(directions))
            { serialized.FindProperty("frontOrigin").vector2Value = new Vector2(.25f, .2f); serialized.ApplyModifiedPropertiesWithoutUndo(); }
            Equal(0, (enemy.GetPatternVisualOrigin() - frozen).magnitude, "existing actor keeps captured emission origin");
            SpawnOrbTest(); MovePlayerTo(origin + Vector3.back * 4.8f); health.ResetHealth();
            yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 4, "edited directional origin");
            Vector3 visibleOrigin = enemy.GetPatternVisualOrigin();
            Require(visibleOrigin.x - enemy.transform.position.x > .2f, "Inspector origin reaches next actual pattern spawn");
            Require(GasSacOrb.TryGetLaunchPosition(enemy, pattern.LockedOrbDirection, settings.orbHitRadius, out Vector3 expected), "edited origin is a safe launch");
            yield return Wait(() => pattern.ActiveOrb != null, 3, "edited origin shot");
            Equal(0, (pattern.ActiveOrb.LaunchPosition - expected).magnitude, "actual shot starts at the edited origin plus body clearance");
            Equal(settings.orbHitRadius, pattern.ActiveOrb.FlightHeight, "visual anchor height does not raise flight height");
            Pass("Inspector directional origin is captured on spawn and changes actual B launch; ground height still derives from radius");
            RestoreAssets();

            SpawnOrbTest(); MovePlayerTo(origin + Vector3.forward * 4.8f); health.ResetHealth();
            yield return Wait(() => pattern.Phase == GasSacPhase.OrbWindup, 4, "rear death tell");
            EnemyFacing deathFacing = enemy.PatternFacing; var renderer = DirectionBody;
            enemy.SuppressExperienceReward = true; enemy.TakeDamage(10000);
            var seen = new HashSet<Sprite>(); float until = Time.time + 2;
            while (enemy.gameObject.activeSelf && Time.time < until) { seen.Add(renderer.sprite); yield return null; }
            Require(!enemy.gameObject.activeSelf && art.deathFrames.All(f => seen.Contains(directions.Capture().Resolve(f, deathFacing))),
                "pattern cleanup keeps rear death direction and all six frames before pooling");
            SpawnOrbTest();
            Require(enemy.HasPatternDirections && !enemy.IsPatternFacingLocked && !enemy.IsDead, "reused gas sac binds new directions with unlocked live state");
            Pass("Death during real B tell retains all 6 rear death frames; pool reuse rebinds cleanly");

            pattern.enabled = false; enemy.SetPatternFacing(Vector3.right); MovePlayerTo(origin + Vector3.right * .35f); health.ResetHealth();
            float before = health.CurrentHealth;
            var near = GasSacOrb.Launch(enemy, enemy.GetComponent<EnemyPatternLifetime>(), enemy.SpawnGeneration, Vector3.right,
                enemy.CreatePatternDamage(GasSacPatternSettings.OrbDamageId, 0), settings.orbSpeed, settings.orbLifetimeSeconds,
                settings.orbHitRadius, art.gasPuff, art.filledCircle, art.gasColor);
            Equal(3, before - health.CurrentHealth, "initial body-to-muzzle sweep does not skip nearby player");
            yield return null;
            Require(near == null && enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, "near hit consumed once at launch");
            Pass("Body-to-emission sweep catches close target once instead of teleporting the projectile past it");
        }

        private static Vector3 FindDirectionPatch()
        {
            Vector2Int initial = biome.WorldToGrid(origin);
            for (int radius = 0; radius <= 45; radius++) for (int dx = -radius; dx <= radius; dx++) for (int dz = -radius; dz <= radius; dz++)
            {
                if (Mathf.Abs(dx) != radius && Mathf.Abs(dz) != radius) continue;
                if (!biome.IsValidPosition(initial.x + dx, initial.y + dz) || !biome.IsWalkable(initial.x + dx, initial.y + dz)) continue;
                Vector3 candidate = biome.GridToWorldWithHeight(initial.x + dx, initial.y + dz);
                bool clear = true;
                for (int angle = 0; angle < 360 && clear; angle += 15) clear = GasSacOrb.HasClearPath(candidate, candidate + Aim(angle) * 6);
                if (clear) return candidate;
            }
            throw new System.InvalidOperationException("No six-meter clear directional test patch");
        }
    }
}
