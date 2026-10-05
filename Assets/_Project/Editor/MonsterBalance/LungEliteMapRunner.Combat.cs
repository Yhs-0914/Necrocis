using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class LungEliteMapRunner
    {
        private static EnemyController pollen, dust;
        private static PollenInvaderElitePattern pollenPattern;
        private static DustClumpElitePattern dustPattern;
        private static Vector3 encounterOrigin;
        private static readonly float[] Angles = { 0, 90, 180, 270 };
        private static Vector3 Aim(float angle) => new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
        private static SpriteRenderer Body(EnemyController actor) => actor.transform.Find("Visual").GetComponent<SpriteRenderer>();
        private static Vector2Int FindPairPatch()
        {
            for (int z = 10; z < biome.MapHeight - 10; z++) for (int x = 10; x < biome.MapWidth - 10; x++)
            {
                int level = biome.GetHeightLevel(x, z); bool clear = biome.IsWalkable(x, z);
                for (int dx = -9; dx <= 9 && clear; dx++) for (int dz = -9; dz <= 9 && clear; dz++)
                    clear = biome.IsWalkable(x + dx, z + dz) && biome.GetHeightLevel(x + dx, z + dz) == level;
                if (clear) return new Vector2Int(x, z);
            }
            throw new InvalidOperationException("No two-species combat patch in actual Lung terrain");
        }
        private static void StagePair(Vector2Int cell, Vector3 aim)
        {
            var d = field.Plan.placements.Single(p => p.monsterId == dustDefinition.monsterId);
            var pollenPoint = field.Plan.placements.Single(p => p.monsterId == pollenDefinition.monsterId);
            Vector3 normal = Vector3.Cross(aim, Vector3.up), offset = aim * 2 + normal * 3;
            d.x = cell.x; d.y = cell.y; pollenPoint.x = cell.x + Mathf.RoundToInt(offset.x); pollenPoint.y = cell.y + Mathf.RoundToInt(offset.z);
            encounterOrigin = biome.GridToWorldWithHeight(cell.x, cell.y);
            field.Configure(biome, temporaryConfig, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            ResetPair();
        }
        private static void ResetPair(bool mobile = false)
        {
            foreach (var s in field.Spawners.ToArray()) s.ReleaseEnemy(); Move(encounterOrigin + Vector3.right * 6);
            pollen = field.Spawners.Single(s => s.Placement.monsterId == pollenDefinition.monsterId).ActiveEnemy;
            dust = field.Spawners.Single(s => s.Placement.monsterId == dustDefinition.monsterId).ActiveEnemy;
            Require(pollen != null && dust != null && pollen.Balance.ContactEnabled && dust.Balance.ContactEnabled, "dedicated live pair and body contact");
            pollenPattern = pollen.GetComponent<PollenInvaderElitePattern>(); dustPattern = dust.GetComponent<DustClumpElitePattern>();
            Require(pollen.HasPatternDirections && !dust.HasPatternDirections && !Body(dust).flipX, "per-species facing contract");
            if (!mobile) { pollen.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0); dust.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0); }
            pollen.Stats.SetBaseStat(CharacterStatType.MaxHealth, 200, true); dust.Stats.SetBaseStat(CharacterStatType.MaxHealth, 200, true); health.ResetHealth();
        }
        private static void SetFollowup(bool enabled)
        {
            var settings = (PollenInvaderPatternSettings)pollenDefinition.pattern;
            if (!backups.ContainsKey(settings)) backups.Add(settings, EditorJsonUtility.ToJson(settings));
            using var data = new SerializedObject(settings); data.FindProperty("followup.enabled").boolValue = enabled; data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static IEnumerator CombinedCombat()
        {
            Require(field.Plan == null, "combo isolated from production plan"); temporaryConfig = ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>();
            temporaryConfig.minimumCount = temporaryConfig.maximumCount = 2; temporaryConfig.clearanceCells = 5;
            temporaryConfig.monsters.Add(DustClumpSetup.PreviewRule(dustDefinition)); temporaryConfig.monsters.Add(PollenInvaderSetup.PreviewRule(pollenDefinition));
            field.Configure(biome, temporaryConfig, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Require(field.Plan.placements.Count == 2, "zero-kill dedicated pair");
            if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
            var cell = FindPairPatch();
            Pass(difficulty + " COMBO: real Lung terrain and temporary close pair; production placements/spacing remain untouched");
            if (gapOnly)
            {
                SetFollowup(true); StagePair(cell, Vector3.right); yield return CloudAndChangingGap(false); yield return CloudAndChangingGap(true); RestoreAssets(); yield break;
            }
            if (preview && (previewPhase == "gap-stay" || previewPhase == "gap-move"))
            {
                SetFollowup(true); StagePair(cell, Vector3.right); yield return CloudAndChangingGap(previewPhase == "gap-move"); yield break;
            }
            foreach (bool useB in preview ? new[] { previewPhase != "a" } : new[] { false, true })
            {
                SetFollowup(useB);
                foreach (float angle in preview ? new[] { 90f } : Angles)
                {
                    var aim = Aim(angle); var normal = Vector3.Cross(aim, Vector3.up); StagePair(cell, aim);
                    foreach (float side in preview ? new[] { -1f } : new[] { -1f, 1f })
                    {
                        ResetPair(); Move(encounterOrigin + aim * 3.6f); health.ResetHealth(); float hp = health.CurrentHealth;
                        yield return Wait(() => dustPattern.Phase == DustClumpPhase.Windup && pollenPattern.Phase == PollenInvaderPhase.Windup, 3, "combined windups");
                        var locked = pollenPattern.LockedDirection; var facing = pollen.PatternFacing;
                        ElitePresentationChecks.RedArea(dustPattern.TelegraphObject, Vector2.one * 2.2f);
                        if (preview && previewPhase == "tell") { yield return new WaitForSeconds(.25f); yield break; }
                        float width = side > 0 ? 5.4f : 2.6f;
                        yield return WalkTo(encounterOrigin + aim * 3.6f + normal * (side * width), 1.5f);
                        yield return Wait(() => dustPattern.ActiveCloud != null && pollenPattern.CycleVolleyCount >= 1, 2, "cloud and pellets active");
                        var cloud = dustPattern.ActiveCloud;
                        Equal(biome.GetGroundHeight(cloud.transform.position), cloud.Body.transform.position.y, "cloud bottom grounded");
                        foreach (var pellet in pollenPattern.ActivePellets.Where(p => p != null))
                        {
                            Equal(biome.GetGroundHeight(pellet.transform.position) + .16f, pellet.Body.transform.position.y, "pellet low height");
                            Require(pellet.GetComponentsInChildren<SpriteRenderer>().Length == 1 && pellet.GetComponentsInChildren<MeshRenderer>().Length == 0, "no simple projectile floor UI");
                        }
                        Require(pollen.PatternFacing == facing && pollen.IsPatternFacingLocked && !Body(dust).flipX && dustPattern.CoreExposed, "committed pollen and stationary directionless core");
                        if (preview)
                        {
                            if (useB) yield return Wait(() => pollenPattern.CycleVolleyCount == 2, 2, "preview B second");
                            yield return new WaitForSeconds(.15f); yield break;
                        }
                        yield return WalkTo(encounterOrigin - aim * 1.2f + normal * (side * width), 1.5f);
                        yield return WalkTo(encounterOrigin - aim * 1.2f, 1.5f);
                        Require(dustPattern.CoreExposed, "detour arrives while core remains exposed");
                        Face(aim); Physics.SyncTransforms(); float oldHp = dust.Stats.CurrentHealth;
                        typeof(PlayerAttack).GetMethod("MeleeAttack", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(player.GetComponent<PlayerAttack>(), null);
                        Require(dust.Stats.CurrentHealth < oldHp, "native Q hits exposed core during mixed encounter");
                        yield return WalkTo(encounterOrigin - aim * 1.8f, 1);
                        yield return Wait(() => dustPattern.ActiveCloud == null && pollen.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, 4, "both threats expire");
                        Equal(hp, health.CurrentHealth, "combined native detour takes no damage");
                        Require(dustPattern.ReleaseCount == 1 && pollenPattern.VolleyCount == (useB ? 2 : 1), "one cloud and selected A/B waves only");
                        Equal(0, (locked - pollenPattern.LockedDirection).magnitude, "no retarget after dodge");
                        Pass($"{difficulty} COMBO {(useB ? "B" : "A")}/{angle:0}deg/side{side:+0;-0}: native detour avoids cloud and fan, Q hits core, damage0, correct {pollenPattern.VolleyCount} waves");
                    }
                }
            }
            SetFollowup(true); StagePair(cell, Vector3.right);
            yield return CloudAndChangingGap(false); yield return CloudAndChangingGap(true);
            RestoreAssets(); StagePair(cell, Vector3.right); ResetPair(true); Move(encounterOrigin + Vector3.right * 5.5f);
            var startD = dust.GetComponent<Rigidbody>().position; var startP = pollen.GetComponent<Rigidbody>().position; yield return new WaitForSeconds(.4f);
            Require(Distance(startD, dust.GetComponent<Rigidbody>().position) > .2f && Distance(startP, pollen.GetComponent<Rigidbody>().position) > .2f, "both production-speed actors move");
            Require(dustPattern.ReleaseCount == 0 && pollenPattern.VolleyCount == 0, "spawn grace holds"); Pass(difficulty + " MOBILE: both base-speed actors approach, with spawn grace and original facing contracts");
            foreach (bool isPollen in new[] { false, true })
            {
                ResetPair(); var actor = isPollen ? pollen : dust;
                foreach (var pattern in actor.GetComponents<MonsterPatternController>()) pattern.EndSpawn(); actor.SetAiSuppressed(true);
                var home = actor.GetComponent<Rigidbody>().position; var away = isPollen ? Vector3.forward : Vector3.left;
                Move(home + away * 2.2f); health.ResetHealth(); float hp = health.CurrentHealth;
                for (int n = 0; n < 60 && health.CurrentHealth == hp; n++) { player.TryMoveByWorld(-away * .05f); yield return new WaitForFixedUpdate(); }
                Equal(CharacterStats.ToHealthUnits(actor.CreateContactDamage(0).Amount), hp - health.CurrentHealth, "actual body contact");
                Pass(difficulty + " CONTACT " + actor.Balance.Current.MonsterId + ": native walking collision damage once (attacks isolated)");
            }
            yield return CombinedVisibility();
            yield return PriorityKills(cell);
        }
        private static IEnumerator CombinedVisibility()
        {
            ResetPair(); Move(encounterOrigin + Vector3.right * 3.6f); yield return Wait(() => dustPattern.ActiveCloud != null && pollenPattern.CycleVolleyCount == 1, 3, "visibility attacks");
            var cloud = dustPattern.ActiveCloud; Move(cloud.transform.position + Vector3.forward * .35f); health.GrantTemporaryInvincibility(3);
            yield return new WaitForSeconds(.18f);
            Require(cloud.IsPlayerOccluded && cloud.Body.color.a <= .36f, "dust fades over player in combined scene");
            Require(pollenPattern.Phase == PollenInvaderPhase.Rotation && Body(pollen).sprite.name.Contains("Rotation") && GasSacOrb.IsVisible(Body(pollen).bounds.center), "actual B cue visible beside dust");
            ElitePresentationChecks.RedArea(dustPattern.TelegraphObject, Vector2.one * 2.2f);
            Move(encounterOrigin - Vector3.right * 3); yield return Wait(() => pollenPattern.CycleVolleyCount == 2, 2, "visible second volley");
            Require(pollenPattern.SecondWavePellets.Count(p => p != null) == 3, "second warning/shot still available with dust");
            Require(pollenPattern.SecondWavePellets.All(p => GasSacOrb.IsVisible(p.Body.transform.position)), "second pellets in viewport");
            health.ResetHealth(); Pass(difficulty + " VISIBILITY: overlapping dust fades over real player, full red area retained, B warning/second volley still visible");
        }
        private static IEnumerator PriorityKills(Vector2Int cell)
        {
            foreach (bool killDust in new[] { difficulty == GameDifficulty.Normal })
            {
                StagePair(cell, Vector3.right); ResetPair(); Move(encounterOrigin + Vector3.right * 3.6f);
                yield return Wait(() => dustPattern.ActiveCloud != null && pollenPattern.CycleVolleyCount == 1, 3, "priority active effects");
                var first = killDust ? dust : pollen; var survivor = killDust ? pollen : dust;
                first.SuppressExperienceReward = true; first.TakeDamage(10000);
                Require(first.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0 && first.IsDeathAnimPlaying, "priority kill clears its threats");
                Require(survivor.GetComponent<EnemyPatternLifetime>().OwnedObjectCount > 0 && !survivor.IsDead, "other enemy retains independent attack");
                Move(encounterOrigin + new Vector3(-2, 0, -3)); health.ResetHealth(); float hp = health.CurrentHealth;
                yield return Wait(() => survivor.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, 4, "survivor threat expires"); Equal(hp, health.CurrentHealth, "priority retreat safe");
                survivor.SuppressExperienceReward = true; survivor.TakeDamage(10000);
                yield return Wait(() => !first.gameObject.activeSelf && !survivor.gameObject.activeSelf, 2, "pair deaths"); field.Refresh(player.transform.position);
                Require(field.Spawners.Count == 0 && field.Plan.placements.All(p => SaveService.IsBiomeEliteDefeated(p.spawnId)), "both deaths remain recorded");
                Pass(difficulty + (killDust ? " PRIORITY dust-first" : " PRIORITY pollen-first") + ": owner-only cleanup, survivor attack independent, safe retreat and both saved deaths");
            }
        }
        private static IEnumerator CloudAndChangingGap(bool move)
        {
            ResetPair(); Move(encounterOrigin + Vector3.right * 3.6f);
            yield return Wait(() => pollenPattern.CycleVolleyCount == 1 && dustPattern.ActiveCloud != null, 3, "cloud/fan gap setup");
            var first = pollenPattern.LockedDirection; var launch = pollenPattern.LaunchPosition;
            Vector3 firstSafe = launch + Quaternion.AngleAxis(-pollenPattern.SignedRotationOffset, Vector3.up) * first * 3.4f;
            Vector3 blockedNewGap = launch + first * 3.4f;
            Vector3 alternate = launch + Quaternion.AngleAxis(30, Vector3.up) * first * 3.4f;
            Move(firstSafe); health.ResetHealth(); float hp = health.CurrentHealth;
            // Crossing at .95s can clip the first wave; waiting until 1.2s instead
            // runs into the second. Cross after the first has passed the route.
            yield return Wait(() => Time.time - pollenPattern.ReleasedAt >= 1.08f, 2, "first fan clears crossing route");
            Require(pollenPattern.FirstVolley.WaveDamageApplications == 0 && dustPattern.ActiveCloud.HitAttempts == 0, "initial gap safe from both");
            Require(Distance(blockedNewGap, dustPattern.ActiveCloud.transform.position) < dustPattern.CloudRadius, "cloud occupies nearest second-wave gap");
            if (move)
            {
                var outside = alternate; outside.z = Mathf.Max(firstSafe.z, encounterOrigin.z + 1.5f);
                yield return GapWalkTo(outside, hp); yield return GapWalkTo(alternate, hp);
                if (preview) yield return Wait(() => Time.time - pollenPattern.SecondReleasedAt >= 1.1f, 2, "alternate gap preview");
                else yield return Wait(() => dustPattern.ActiveCloud == null && pollen.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, 4, "combined gap finishes");
                Equal(hp, health.CurrentHealth, "outside-cloud path to alternate gap safe");
            }
            else
            {
                yield return Wait(() => pollenPattern.SecondVolley != null && pollenPattern.SecondVolley.WaveDamageApplications == 1, 2, "old gap second hit");
                Equal(CharacterStats.ToHealthUnits(pollen.CreatePatternDamage(PollenInvaderPatternSettings.FollowupDamageId, 0).Amount), hp - health.CurrentHealth, "old gap only pollen damage");
                Require(dustPattern.ActiveCloud.HitAttempts == 0, "dust did not hit old gap");
            }
            Pass(difficulty + (move ? " MIXED GAP move: cloud blocks nearest new gap, native outside-circle route reaches another gap with damage0" : " MIXED GAP stay: first gap avoids cloud/first fan, second pollen wave alone hits it"));
        }
        private static IEnumerator GapWalkTo(Vector3 destination, float hp)
        {
            var steps = WalkTo(destination, 1);
            while (steps.MoveNext())
            {
                yield return steps.Current;
                Require(health.CurrentHealth == hp, $"gap route hit: player={player.transform.position}, goal={destination}, moveSpeed={player.MoveSpeed}, firstElapsed={Time.time-pollenPattern.ReleasedAt:F3}, cloudHits={dustPattern.ActiveCloud?.HitAttempts}, firstDamage={pollenPattern.FirstVolley?.WaveDamageApplications}, secondDamage={pollenPattern.SecondVolley?.WaveDamageApplications}");
            }
        }
    }
}
