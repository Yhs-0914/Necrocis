using System;
using System.Collections;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class PollenInvaderConnectionRunner
    {
        private static void SetBool(Object source, string property, bool value)
        {
            if (!backups.ContainsKey(source)) backups.Add(source, EditorJsonUtility.ToJson(source));
            using var data = new SerializedObject(source); data.FindProperty(property).boolValue = value; data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static IEnumerator FollowupPreview()
        {
            Spawn(); Move(origin + Aim(angle) * 4);
            yield return Wait(() => pattern.CycleVolleyCount == 1, 3, "B preview first");
            if (previewPhase == "gap-stay" || previewPhase == "gap-shift")
            {
                Vector3 launch = pattern.LaunchPosition, firstDirection = pattern.LockedDirection;
                Move(launch + pattern.SecondLockedDirection * 3.4f); health.ResetHealth();
                yield return Wait(() => Time.time - pattern.ReleasedAt >= 1.1f, 2, "preview first safe gap");
                Require(pattern.CycleHitBudget.HitAttempts == 0, "preview first gap safe");
                if (previewPhase == "gap-stay") yield return Wait(() => pattern.SecondVolley.WaveDamageApplications == 1, 2, "preview second hits old gap");
                else
                {
                    yield return WalkTo(launch + firstDirection * 3.4f, 1);
                    yield return Wait(() => Time.time - pattern.SecondReleasedAt >= 1.1f, 2, "preview changed gap safe");
                    Require(pattern.CycleHitBudget.HitAttempts == 0, "preview native gap shift safe");
                }
                yield break;
            }
            Move(origin - Aim(angle) * 3 + Aim(angle + 90) * 2);
            if (previewPhase == "first") { yield return new WaitForSeconds(.08f); yield break; }
            if (previewPhase == "turn") { yield return new WaitForSeconds(.55f); yield break; }
            yield return Wait(() => pattern.CycleVolleyCount == 2, 2, "B preview second");
            yield return new WaitForSeconds(previewPhase == "second-launch" ? .01f : .35f);
        }
        private static IEnumerator FollowupChecks()
        {
            if (battleSection == "tail")
            {
                yield return FollowupDamageChecks(); yield return FollowupSourceChecks(); yield return FollowupLifecycleChecks(); yield return FollowupBalanceChecks(); yield break;
            }
            var catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath); var editor = Editor.CreateEditor(catalog);
            try
            {
                typeof(MonsterBalanceCatalogEditor).GetField("monsterIndex", Private).SetValue(editor, catalog.monsters.IndexOf(definition));
                var paths = editor.CreateInspectorGUI().Query<PropertyField>().ToList().Select(p => p.bindingPath).ToArray();
                Require(new[] { "followup.enabled", "followup.angleOffset", "followup.rotationSeconds", "followup.recoverySeconds", "followup.rearmSeconds", "rotationFrames" }.All(paths.Contains), "B Inspector separate sources");
            }
            finally { Object.DestroyImmediate(editor); }
            Pass("INSPECTOR B: explicit content switch, turn angle/timing, final recovery/rearm and directional rotation art; coefficients remain in definition");
            var clone = Object.Instantiate(settings); var cloneArt = Object.Instantiate(settings.presentation); clone.presentation = cloneArt;
            try
            {
                clone.followup.angleOffset = 0; Require(clone.GetValidationError(definition) != null, "zero angle rejected");
                clone.followup.angleOffset = clone.spreadHalfAngle; Require(clone.GetValidationError(definition) != null, "duplicate old ray angle rejected");
                clone.followup.angleOffset = 15; clone.followup.rotationSeconds = .1f; Require(clone.GetValidationError(definition) != null, "unreadably short turn rejected");
                clone.followup.rotationSeconds = .8f; cloneArt.rotationFrames = null; Require(clone.GetValidationError(definition) != null, "B requires art");
                clone.followup.enabled = false; Require(clone.GetValidationError(definition) == null, "A remains usable with disabled B art/tuning absent");
            }
            finally { Object.DestroyImmediate(clone); Object.DestroyImmediate(cloneArt); }
            Pass("VALIDATION: invalid angle/short cue/missing rotation rejected for B, disabled B does not invalidate A");
            foreach (float targetAngle in BattleAngles)
            {
                Spawn(); Move(origin + Aim(targetAngle) * 4); yield return Wait(() => pattern.CycleVolleyCount == 1, 3, "B first wave");
                Require(pattern.Phase == PollenInvaderPhase.Rotation && pattern.FirstWavePellets.Length == 3 && pattern.SecondWavePellets.Length == 0, "first three then rotation");
                var first = pattern.LockedDirection; var second = pattern.SecondLockedDirection; var facing = enemy.PatternFacing;
                float wantedOffset = facing == EnemyFacing.Left ? -15 : 15;
                Equal(wantedOffset, pattern.SignedRotationOffset, "mirrored offset"); Equal(0, Vector3.Distance(Quaternion.AngleAxis(wantedOffset, Vector3.up) * first, second), "second aim fixed at rotation start");
                Move(origin - Aim(targetAngle) * 3); enemy.TakeDamage(1); enemy.ApplyKnockback(Vector3.right, .4f);
                Equal(0, Vector3.Distance(origin, enemy.GetComponent<Rigidbody>().position), "rotation position locked and vulnerable");
                var bank = settings.presentation.directionalPresentation.Capture();
                var expected = settings.presentation.rotationFrames.Select(f => bank.Resolve(f, facing)).ToArray(); var seen = new System.Collections.Generic.HashSet<Sprite>();
                while (pattern.CycleVolleyCount < 2) { seen.Add(Body().sprite); Require(Time.time - pattern.RotationStartedAt < 1, "second wave timely"); yield return null; }
                Require(expected.All(seen.Contains), "three directional rotation frames shown");
                Require(pattern.FirstWavePellets.Length == 3 && pattern.SecondWavePellets.Length == 3 && pattern.ActivePellets.Length == 6, "exactly 3+3");
                Require(pattern.SecondReleasedAt - pattern.ReleasedAt >= .79f && pattern.SecondReleasedAt - pattern.ReleasedAt <= .85f, "single .8s inter-wave cue");
                Equal(0, Vector3.Distance(second, pattern.SecondLockedDirection), "moving target never retargets wave2");
                Require(enemy.PatternFacing == facing && enemy.IsPatternFacingLocked, "same body view committed across both waves");
                for (int i = 0; i < 3; i++)
                {
                    var pellet = pattern.SecondWavePellets[i]; CheckPellet(pellet, .16f, 3.5f);
                    Equal(0, Vector3.Distance(pellet.Direction, Quaternion.AngleAxis((i - 1) * 30, Vector3.up) * second), "wave2 fan shares original spread");
                }
                yield return Wait(() => pattern.Phase == PollenInvaderPhase.Ready, 2, "B final recovery");
                Require(Mathf.Abs(Time.time - pattern.SecondReleasedAt - 1.2f) < .06f && Mathf.Abs(pattern.NextReadyTime - Time.time - 3.2f) < .06f, "B final timings once, no A recovery added");
                Require(!enemy.IsPatternPositionLocked && !enemy.IsPatternFacingLocked && pattern.VolleyCount == 2, "B unlock and two waves only");
                Pass($"FLOW B {targetAngle:0}deg/{facing}: rotation3 frames, 3+.8s+3 with mirrored15deg locked aim, shared dimensions/low height, only B recovery1.2/rearm3.2");
            }
            foreach (float targetAngle in BattleAngles) foreach (bool shift in new[] { false, true }) yield return FollowupGapCase(targetAngle, shift);
            yield return FollowupDamageChecks();
            yield return FollowupSourceChecks();
            yield return FollowupLifecycleChecks();
            yield return FollowupBalanceChecks();
            Pass("P-03-B complete; no P-MAP registration, original base/common sources restored");
        }
        private static IEnumerator FollowupGapCase(float targetAngle, bool shift)
        {
            Spawn(); Move(origin + Aim(targetAngle) * 4); yield return Wait(() => pattern.CycleVolleyCount == 1, 3, "gap first");
            Vector3 launch = pattern.LaunchPosition, oldDirection = pattern.LockedDirection;
            Move(launch + pattern.SecondLockedDirection * 3.4f); health.ResetHealth(); float hp = health.CurrentHealth;
            yield return Wait(() => Time.time - pattern.ReleasedAt >= 1.1f, 2, "first passes old gap");
            Require(pattern.FirstVolley.WaveDamageApplications == 0 && pattern.CycleHitBudget.HitAttempts == 0, "first gap safe before second arrives"); Equal(hp, health.CurrentHealth, "first gap HP unchanged");
            if (shift)
            {
                yield return WalkTo(launch + oldDirection * 3.4f, 1);
                yield return Wait(() => enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, 3, "shifted gap finish");
                Equal(hp, health.CurrentHealth, "native shift avoids second");
            }
            else
            {
                yield return Wait(() => pattern.SecondVolley.WaveDamageApplications == 1, 2, "old gap gets second pellet");
                Equal(3, hp - health.CurrentHealth, "old gap damage3");
            }
            Pass($"GAP B {targetAngle:0}deg/{(shift ? "move" : "stay")}: first gap safe; {(shift ? "native walk to changed gap avoids both, damage0" : "staying is hit by second wave only, damage3")}");
        }
        private static IEnumerator FollowupDamageChecks()
        {
            foreach (bool invulnerable in new[] { false, true })
            {
                Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.CycleVolleyCount == 1, 3, "shared budget first");
                var first = pattern.FirstWavePellets[1]; Move(first.transform.position + first.Direction * 1.2f); health.ResetHealth(); float hp = health.CurrentHealth;
                if (invulnerable) health.GrantTemporaryInvincibility(.5f);
                yield return Wait(() => pattern.CycleHitBudget.HitAttempts == 1, 1, "first consumes budget");
                Equal(invulnerable ? 0 : 3, hp - health.CurrentHealth, "first budget damage");
                Move(origin - Vector3.right * 3);
                yield return Wait(() => pattern.CycleVolleyCount == 2, 2, "shared budget second"); yield return new WaitForSeconds(.55f);
                var second = pattern.SecondWavePellets[1]; Require(second != null, "second still flying"); Move(second.transform.position + second.Direction * 1.2f);
                yield return Wait(() => pattern.SecondVolley.WaveHitAttempts > 0, 1, "actual second collision");
                Equal(invulnerable ? 0 : 3, hp - health.CurrentHealth, "cycle cannot hit twice");
                Require(pattern.CycleHitBudget.HitAttempts == 1 && pattern.SecondVolley.WaveDamageApplications == 0, "one shared attempt across six");
                Pass(invulnerable ? "B INVULNERABILITY: protected first collision consumes both-wave budget; second cannot retry after protection" : "B DAMAGE CAP: first damage3, later second collision after hurt protection adds0, total3");
            }
        }
        private static IEnumerator FollowupSourceChecks()
        {
            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 2, "B snapshot");
            Set(settings, "followup.angleOffset", 20); Set(settings, "followup.rotationSeconds", .45f); Set(settings, "followup.recoverySeconds", .7f); Set(settings, "followup.rearmSeconds", 1.1f);
            Set(settings, "pelletSpeed", 5); Set(settings, "pelletRadius", .2f); Set(settings, "pelletLifetimeSeconds", 1.5f);
            Set(definition, "statSets.Array.data[0].attackPower", 4);
            int secondIndex = definition.patternDamage.FindIndex(p => p.id == PollenInvaderPatternSettings.FollowupDamageId);
            Set(definition, "patternDamage.Array.data[" + secondIndex + "].coefficient", 2);
            Set(DifficultyBalanceService.GetProfile(GameDifficulty.Normal), "elites.outgoingDamage", 1.5f);
            Equal(.8f, pattern.RotationDuration, "existing B snapshot"); Equal(.16f, pattern.PelletRadius, "existing radius snapshot");
            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.CycleVolleyCount == 1, 2, "edited first"); Move(origin - Vector3.right * 3);
            yield return Wait(() => pattern.CycleVolleyCount == 2, 2, "edited second");
            Equal(.45f, pattern.RotationDuration, "edited turn time"); Equal(20, Mathf.Abs(pattern.SignedRotationOffset), "edited turn angle");
            Require(Mathf.Abs(pattern.SecondReleasedAt - pattern.ReleasedAt - .45f) < .06f, "actual .45s second shot");
            yield return new WaitForSeconds(.6f); var second = pattern.SecondWavePellets[1]; CheckPellet(second, .2f, 5); Equal(1.5f, second.Duration, "shared lifetime");
            Move(second.transform.position + second.Direction * 1.2f); health.ResetHealth(); float hp = health.CurrentHealth;
            yield return Wait(() => pattern.SecondVolley.WaveDamageApplications == 1, 1, "edited second damage"); Equal(12, hp - health.CurrentHealth, "second coefficient actual damage12");
            Require(pattern.FirstVolley.WaveDamageApplications == 0, "only second coefficient used");
            yield return Wait(() => pattern.Phase == PollenInvaderPhase.Ready, 1, "edited B recovery");
            Require(Mathf.Abs(pattern.NextReadyTime - pattern.SecondReleasedAt - 1.8f) < .08f, "edited B recovery/rearm once"); Restore();
            Pass("INSPECTOR B: next spawn captures turn20deg/.45s, shared speed5/radius.2/lifetime1.5, second actual damage12, B .7+1.1 recovery/rearm; old snapshot stable, sources restored");
            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.CycleVolleyCount == 1, 3, "live toggle snapshot");
            SetBool(settings, "followup.enabled", false); Require(pattern.FollowupEnabled, "live B unaffected by source toggle");
            yield return Wait(() => pattern.CycleVolleyCount == 2, 2, "live B keeps second");
            Spawn(); Move(origin + Vector3.right * 4); Require(!pattern.FollowupEnabled && pattern.RotationDuration == 0, "new A has no B timer");
            yield return Wait(() => pattern.CycleVolleyCount == 1, 3, "new A first"); Move(origin - Vector3.right * 3);
            yield return Wait(() => pattern.Phase == PollenInvaderPhase.Ready, 2, "new A recovery");
            Require(pattern.CycleVolleyCount == 1 && pattern.SecondVolley == null && pattern.SecondLockedDirection == Vector3.zero, "A no second scheduling");
            Require(Mathf.Abs(pattern.NextReadyTime - Time.time - 2.5f) < .06f && Mathf.Abs(Time.time - pattern.ReleasedAt - 1) < .06f, "original A timing"); Restore();
            Pass("TOGGLE: live B remains B; next spawn with switch off is exactly one wave, A recovery1/rearm2.5 and no second timer");
        }
    }
}
