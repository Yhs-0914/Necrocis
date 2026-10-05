using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class IntestineEliteMapRunner
    {
        private static EnemyController gas, residue;
        private static GasSacElitePattern gasPattern;
        private static HardenedResidueElitePattern residuePattern;
        private static Vector3 encounterOrigin;

        private static IEnumerator CombinedCombat()
        {
            Require(field.Plan == null, "isolated encounter starts before production placement");
            temporaryConfig = ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>();
            temporaryConfig.minimumCount = temporaryConfig.maximumCount = 2; temporaryConfig.clearanceCells = 6;
            temporaryConfig.monsters.Add(GasSacSetup.PreviewRule(gasDefinition));
            temporaryConfig.monsters.Add(HardenedResidueSetup.PreviewRule(residueDefinition));
            field.Configure(biome, temporaryConfig, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Require(field.Plan.placements.Count == 2, "one placement of each approved species");
            var residuePoint = field.Plan.placements.Single(p => p.monsterId == residueDefinition.monsterId);
            var gasPoint = field.Plan.placements.Single(p => p.monsterId == gasDefinition.monsterId);
            // Stage a worst-case overlap in the temporary run only. Production still uses independent 18m-spaced homes.
            gasPoint.x = residuePoint.x + 2; gasPoint.y = residuePoint.y + 3;
            encounterOrigin = biome.GridToWorldWithHeight(residuePoint.x, residuePoint.y);
            field.Configure(biome, temporaryConfig, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            source.enabled = sourceEnabled; // The field now owns its temporary config; keep the Inspector's production switch intact.
            field.enabled = preview;
            if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
            ResetPair();
            Require(gasPattern.OrbEnabled, "approved gas A+B enabled");
            Pass("temporary real-map spawners attach gas A+B and residue A with zero normal kills; production spacing unchanged");

            if (preview)
            {
                Move(encounterOrigin + Vector3.right * 2.3f);
                yield return Wait(() => residuePattern.Phase == HardenedResiduePhase.Windup, 3, "preview residue tell");
                Move(residuePattern.LandingCenter + Vector3.forward * 1.1f);
                PrivateSet(gasPattern, "nextReady", Time.time);
                yield return Wait(() => gasPattern.Phase == GasSacPhase.Windup, 1, "preview gas tell");
                yield break;
            }

            foreach (bool breakSlab in new[] { true, false })
            {
                ResetPair(); Move(encounterOrigin + Vector3.right * 2.3f); health.ResetHealth();
                yield return Wait(() => residuePattern.Phase == HardenedResiduePhase.Windup, 3, "residue tell");
                Vector3 center = residuePattern.LandingCenter;
                ElitePresentationChecks.RedArea(residuePattern.TelegraphObject, residuePattern.Footprint);
                Move(center + Vector3.forward * 1.1f);
                yield return Wait(() => residuePattern.ActiveRubble != null && residuePattern.ActiveRubble.IsBlocking, 2, "slab landing");
                float hp = health.CurrentHealth;
                var slab = residuePattern.ActiveRubble;
                PrivateSet(gasPattern, "nextReady", Time.time);
                yield return Wait(() => gasPattern.Phase == GasSacPhase.Windup, 1, "gas burst tell with slab active");
                ElitePresentationChecks.RedArea(gasPattern.TelegraphObject, Vector2.one * gasPattern.BurstRadius * 2);
                var motor = player.GetComponent<ProceduralTerrainMotor>();
                Require(!ResidueRubble.CanTraverse(center + Vector3.forward * 1.1f, center - Vector3.forward, motor.TerrainHalfExtents), "slab blocks direct escape before breaking");
                if (breakSlab)
                {
                    Face(Vector3.back); Physics.SyncTransforms();
                    typeof(PlayerAttack).GetMethod("MeleeAttack", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(player.GetComponent<PlayerAttack>(), null);
                    Require(!slab.IsBlocking, "actual Q attack opens escape route");
                    yield return WalkTo(center - Vector3.forward, 1);
                }
                else
                {
                    float side = residuePattern.Footprint.x * .5f + motor.TerrainHalfExtents.x + .2f;
                    yield return WalkTo(center + Vector3.forward * 1.1f + Vector3.right * side, 1);
                    yield return WalkTo(center + Vector3.right * side - Vector3.forward * .15f, 1);
                    Require(slab != null && slab.IsBlocking, "side route clears blast while slab is still present");
                }
                Require(gasPattern.BurstCount == 0 && Distance(player.transform.position, gas.transform.position) > gasPattern.BurstRadius,
                    "default walking reaches outside the actual burst before impact");
                yield return Wait(() => gasPattern.BurstCount == 1, 2, "burst");
                Equal(hp, health.CurrentHealth, "combined encounter dodge loses no HP");
                Pass(breakSlab ? "real Q breaks slab, then default-speed walking escapes gas before impact with zero damage"
                    : "default-speed side detour escapes gas before impact while slab still blocks the direct route; zero damage");
            }

            ResetPair(); Move(gas.transform.position + Vector3.right * 5);
            PrivateSet(gasPattern, "nextReady", Time.time);
            yield return Wait(() => gasPattern.Phase == GasSacPhase.OrbWindup, 3, "combined encounter orb selection");
            Require(gasPattern.TelegraphObject == null, "simple orb has no ground range marker");
            Move(gas.transform.position + Vector3.left * 5); yield return null;
            Require(gas.transform.Find("Visual").GetComponent<SpriteRenderer>().flipX, "orb keeps committed right-facing pose");
            yield return Wait(() => gasPattern.ActiveOrb != null, 2, "orb launch");
            Require(gasPattern.ActiveOrb.FlightHeight <= .31f, "gas projectile stays close to ground");
            gas.ReleaseToPool();
            yield return null;
            Require(gasPattern.ActiveOrb == null, "gas owner release cleans live projectile in mixed encounter");
            Pass("mixed encounter keeps fixed-facing gas B, no ground UI, ground-height projectile and owner cleanup");

            ResetPair(); Move(encounterOrigin + Vector3.right * 2.3f);
            yield return Wait(() => residuePattern.Phase == HardenedResiduePhase.Windup, 3, "final residue tell");
            Move(residuePattern.LandingCenter + Vector3.forward * 1.1f);
            yield return Wait(() => residuePattern.ActiveRubble != null && residuePattern.ActiveRubble.IsBlocking, 2, "final slab");
            PrivateSet(gasPattern, "nextReady", Time.time);
            yield return Wait(() => gasPattern.Phase == GasSacPhase.Windup, 1, "final gas tell");
            gas.SuppressExperienceReward = residue.SuppressExperienceReward = true;
            gas.TakeDamage(10000); residue.TakeDamage(10000);
            Require(ResidueRubble.ActiveBlockCount == 0 && gasPattern.Phase == GasSacPhase.Inactive && residuePattern.Phase == HardenedResiduePhase.Inactive,
                "both deaths immediately cancel attack and obstacle ownership");
            Require(gas.IsDeathAnimPlaying && residue.IsDeathAnimPlaying, "both actors keep their dedicated death animations");
            yield return new WaitForSeconds(1.5f);
            field.Refresh(player.transform.position);
            Require(field.Spawners.Count == 0 && ResidueRubble.ActiveBlockCount == 0, "both defeated points stay cleared");
            Pass("two simultaneous deaths retain death animations but immediately clear gas warning and slab blocking; no respawn");
        }

        private static void ResetPair()
        {
            foreach (var spawner in field.Spawners.ToArray()) spawner.ReleaseEnemy();
            Move(encounterOrigin + Vector3.right * 8);
            gas = field.Spawners.Single(s => s.Placement.monsterId == gasDefinition.monsterId).ActiveEnemy;
            residue = field.Spawners.Single(s => s.Placement.monsterId == residueDefinition.monsterId).ActiveEnemy;
            Require(gas != null && residue != null, "both field actors active");
            gasPattern = gas.GetComponent<GasSacElitePattern>(); residuePattern = residue.GetComponent<HardenedResidueElitePattern>();
            if (!preview)
            { gas.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0); residue.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0); }
            PrivateSet(gasPattern, "nextReady", float.PositiveInfinity);
        }
    }
}
