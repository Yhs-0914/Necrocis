using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class IntestineEliteMapRunner
    {
        public static void RunDirectionalCombination() => Start(true, false, GameDifficulty.Normal, true);
        public static void RunDirectionalNormal() => Start(false, false, GameDifficulty.Normal, true);
        public static void RunDirectionalHard() => Start(false, false, GameDifficulty.Hard, true);
        public static void PreviewDirectionalCombination() => Start(true, true, GameDifficulty.Normal, true);
        private static readonly float[] MapAngles = { 0, 90, 180, 270 };
        private static Vector3 MapAim(float angle) => new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));

        private static Vector2Int FindCombinationCell()
        {
            var settings = (HardenedResiduePatternSettings)residueDefinition.pattern;
            for (int z = 10; z < biome.MapHeight - 10; z++) for (int x = 10; x < biome.MapWidth - 10; x++)
            {
                int level = biome.GetHeightLevel(x, z); bool clear = biome.IsWalkable(x, z);
                for (int dx = -9; dx <= 9 && clear; dx++) for (int dz = -9; dz <= 9 && clear; dz++)
                    clear = biome.IsWalkable(x + dx, z + dz) && biome.GetHeightLevel(x + dx, z + dz) == level;
                if (!clear) continue;
                Vector3 origin = biome.GridToWorldWithHeight(x, z);
                foreach (float angle in MapAngles)
                {
                    Vector3 aim = MapAim(angle), normal = Vector3.Cross(aim, Vector3.up);
                    if (!ResiduePlacementSafety.CanPlace(origin + aim * settings.placementDistance, normal, settings.footprint)) { clear = false; break; }
                }
                if (clear) return new Vector2Int(x, z);
            }
            throw new InvalidOperationException("No flat four-direction combination patch in the actual map");
        }

        private static void StageDirectionPair(Vector2Int cell, Vector3 aim)
        {
            var residuePoint = field.Plan.placements.Single(p => p.monsterId == residueDefinition.monsterId);
            var gasPoint = field.Plan.placements.Single(p => p.monsterId == gasDefinition.monsterId);
            Vector3 normal = Vector3.Cross(aim, Vector3.up), gasOffset = aim * 2 + normal * 3;
            residuePoint.x = cell.x; residuePoint.y = cell.y;
            gasPoint.x = cell.x + Mathf.RoundToInt(gasOffset.x); gasPoint.y = cell.y + Mathf.RoundToInt(gasOffset.z);
            encounterOrigin = biome.GridToWorldWithHeight(cell.x, cell.y);
            field.Configure(biome, temporaryConfig, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            ResetDirectionPair();
        }

        private static void ResetDirectionPair()
        {
            ResetPair();
            gas.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0); residue.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0);
            Require(gas.HasPatternDirections && residue.HasPatternDirections, "both real field actors bind directional art");
        }

        private static IEnumerator DirectionalCombinedCombat()
        {
            Require(field.Plan == null, "temporary combination starts before production placement");
            temporaryConfig = ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>();
            temporaryConfig.minimumCount = temporaryConfig.maximumCount = 2; temporaryConfig.clearanceCells = 6;
            temporaryConfig.monsters.Add(GasSacSetup.PreviewRule(gasDefinition));
            temporaryConfig.monsters.Add(HardenedResidueSetup.PreviewRule(residueDefinition));
            field.Configure(biome, temporaryConfig, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Require(field.Plan.placements.Count == 2, "two temporary map placements");
            Vector2Int cell = FindCombinationCell();
            source.enabled = sourceEnabled; field.enabled = false;
            if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
            Pass("two dedicated field species at zero kills; approved directional art, production 4-6 count and 18m spacing untouched");
            foreach (float angle in preview ? new[] { 90f } : MapAngles)
            {
                Vector3 aim = MapAim(angle), normal = Vector3.Cross(aim, Vector3.up);
                StageDirectionPair(cell, aim);
                foreach (int route in preview ? new[] { 0 } : new[] { 0, -1, 1 })
                {
                    ResetDirectionPair(); Move(encounterOrigin + aim * 2.3f); health.ResetHealth();
                    yield return Wait(() => residuePattern.Phase == HardenedResiduePhase.Windup, 4, "directional residue tell " + angle);
                    Vector3 center = residuePattern.LandingCenter; EnemyFacing committed = residue.PatternFacing;
                    bool vertical = residuePattern.UsesVerticalRubble;
                    Require(vertical == (angle == 90 || angle == 270), "body direction selects corresponding ground axis");
                    ElitePresentationChecks.RedArea(residuePattern.TelegraphObject,
                        vertical ? new Vector2(.7f, 2.2f) : new Vector2(2.2f, .7f));
                    Move(center + normal * 1.1f);
                    if (preview)
                    {
                        PrivateSet(gasPattern, "nextReady", Time.time);
                        yield return Wait(() => gasPattern.Phase == GasSacPhase.Windup, 1, "preview overlapping gas tell");
                        yield break;
                    }
                    yield return Wait(() => residuePattern.ActiveRubble != null && residuePattern.ActiveRubble.IsBlocking, 2, "directional slab landing");
                    Require(residue.PatternFacing == committed && residue.IsPatternFacingLocked, "residue keeps committed pose with player beside it");
                    var slab = residuePattern.ActiveRubble;
                    var art = ((HardenedResiduePatternSettings)residueDefinition.pattern).presentation;
                    Require(slab.transform.Find("CrustSlab").GetComponent<SpriteRenderer>().sprite == (vertical ? art.verticalRubble : art.rubble), "current approved prop view in mixed encounter");
                    float hp = health.CurrentHealth;
                    PrivateSet(gasPattern, "nextReady", Time.time);
                    yield return Wait(() => gasPattern.Phase == GasSacPhase.Windup, 1, "gas tell while directional slab remains");
                    EnemyFacing gasFacing = gas.PatternFacing;
                    ElitePresentationChecks.RedArea(gasPattern.TelegraphObject, Vector2.one * gasPattern.BurstRadius * 2);
                    var feet = player.GetComponent<ProceduralTerrainMotor>().TerrainHalfExtents;
                    Require(!ResidueRubble.CanTraverse(center + normal * 1.1f, center - normal, feet), "intact slab blocks the straight escape path");
                    if (route == 0)
                    {
                        Face(-normal); Physics.SyncTransforms();
                        typeof(PlayerAttack).GetMethod("MeleeAttack", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(player.GetComponent<PlayerAttack>(), null);
                        Require(!slab.IsBlocking, "native Q breaks the slab immediately");
                        yield return WalkTo(center - normal, 1);
                    }
                    else
                    {
                        float halfAlongFeet = Mathf.Abs(aim.x) * feet.x + Mathf.Abs(aim.z) * feet.y;
                        float side = residuePattern.Footprint.x * .5f + halfAlongFeet + .2f;
                        float approach = 1.1f;
                        if (route < 0)
                        {
                            // The old negative-end shortcut crossed the residue body once contact was enabled.
                            // Go around both the slab and the body, retaining native speed and all damage.
                            var body = residue.GetComponent<Collider>().bounds.extents;
                            var hurt = player.HitCollider.bounds.extents;
                            float along = Mathf.Abs(aim.x) * (body.x + hurt.x) + Mathf.Abs(aim.z) * (body.z + hurt.z);
                            float across = Mathf.Abs(normal.x) * (body.x + hurt.x) + Mathf.Abs(normal.z) * (body.z + hurt.z);
                            side = Mathf.Max(side, Vector3.Dot(center - residue.transform.position, aim) + along + .2f);
                            approach = Mathf.Max(approach, across + .2f);
                            yield return WalkTo(center + normal * approach, 1);
                        }
                        yield return WalkTo(center + normal * approach + aim * (route * side), 1);
                        yield return WalkTo(center + aim * (route * side) - normal * .15f, 1);
                        Require(slab != null && slab.IsBlocking, "detour completed before default prop expiry");
                    }
                    Require(gasPattern.BurstCount == 0 && Distance(player.transform.position, gas.transform.position) > gasPattern.BurstRadius,
                        "native default-speed movement leaves gas area before impact");
                    Require(gas.PatternFacing == gasFacing && gas.IsPatternFacingLocked, "gas keeps its committed directional pose during escape");
                    yield return Wait(() => gasPattern.BurstCount == 1, 2, "combined burst impact");
                    Equal(hp, health.CurrentHealth, "combined escape takes no damage");
                    Pass($"COMBO {angle:0}deg/{committed}: {(route == 0 ? "Q break then walk" : route < 0 ? "negative-side detour" : "positive-side detour")}, default tell/lifetime/speed, damage 0");
                }
            }

            foreach (float angle in new[] { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f })
            {
                ResetDirectionPair(); Vector3 aim = MapAim(angle); Move(gas.transform.position + aim * 4.8f);
                PrivateSet(gasPattern, "nextReady", Time.time);
                yield return Wait(() => gasPattern.Phase == GasSacPhase.OrbWindup, 3, "mixed directional orb tell");
                EnemyFacing facing = gas.PatternFacing; Vector3 locked = gasPattern.LockedOrbDirection;
                Require(gasPattern.TelegraphObject == null, "simple orb has no floor UI");
                yield return Wait(() => gasPattern.ActiveOrb != null, 2, "mixed directional orb launch");
                var orb = gasPattern.ActiveOrb;
                Equal(0, (orb.Direction - locked).magnitude, "orb keeps exact diagonal XZ aim");
                Equal(.3f, orb.FlightHeight, "orb remains close to ground");
                Require(gas.PatternFacing == facing && gas.IsPatternFacingLocked, "mixed orb keeps facing during recovery");
                gas.ReleaseToPool(); Require(!orb.gameObject.activeSelf, "release disables orb immediately");
                yield return null; Require(gasPattern.ActiveOrb == null, "orb destroyed after frame boundary");
                Pass($"MIXED ORB {angle:0}deg/{facing}: exact aim, .3m ground flight, no floor UI, immediate owner cleanup");
            }
            StageDirectionPair(cell, Vector3.forward); Move(encounterOrigin + Vector3.forward * 2.3f);
            yield return Wait(() => residuePattern.Phase == HardenedResiduePhase.Windup, 4, "mixed death slab tell");
            Move(residuePattern.LandingCenter + Vector3.left * 1.1f);
            yield return Wait(() => residuePattern.ActiveRubble != null && residuePattern.ActiveRubble.IsBlocking, 2, "mixed death vertical slab");
            PrivateSet(gasPattern, "nextReady", Time.time);
            yield return Wait(() => gasPattern.Phase == GasSacPhase.Windup, 1, "mixed death gas tell");
            var tell = gasPattern.TelegraphObject; var prop = residuePattern.ActiveRubble;
            gas.SuppressExperienceReward = residue.SuppressExperienceReward = true;
            gas.TakeDamage(10000); residue.TakeDamage(10000);
            Require((tell == null || !tell.activeSelf) && !prop.IsBlocking && !prop.gameObject.activeSelf && ResidueRubble.ActiveBlockCount == 0,
                "two owners immediately clear warnings and vertical obstacle");
            Require(gas.IsDeathAnimPlaying && residue.IsDeathAnimPlaying, "both directional deaths continue");
            yield return new WaitForSeconds(1.5f); field.Refresh(player.transform.position);
            Require(field.Spawners.Count == 0, "both defeated temporary points stay cleared");
            Pass("MIXED DEATH: vertical prop and gas warning deactivate immediately; both death animations finish without respawn");
        }

        private static IEnumerator DirectionalFieldLifetimes(string planJson)
        {
            foreach (string mode in new[] { "gas-tell", "gas-orb", "residue-tell", "residue-drop", "residue-land" })
            {
                bool isGas = mode.StartsWith("gas", StringComparison.Ordinal);
                var point = field.Plan.placements.First(p => p.monsterId == (isGas ? gasDefinition.monsterId : residueDefinition.monsterId));
                Vector3 home = biome.GridToWorldWithHeight(point.x, point.y);
                Move(home + Vector3.right * 4);
                Actor(point).ReleaseToPool(); field.Refresh(player.transform.position);
                var actor = Actor(point); actor.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0);
                Vector3 aim = Vector3.zero;
                foreach (float angle in new[] { 90f, 270f, 0f, 180f, 45f, 135f, 225f, 315f })
                {
                    Vector3 candidate = MapAim(angle);
                    if (isGas ? GasSacOrb.HasClearPath(home, home + candidate * (mode == "gas-orb" ? 4.8f : 2.3f))
                        : ResiduePlacementSafety.CanPlace(home + candidate * ((HardenedResiduePatternSettings)residueDefinition.pattern).placementDistance,
                            Mathf.Abs(candidate.z) > Mathf.Abs(candidate.x) ? Vector3.left : Vector3.forward,
                            ((HardenedResiduePatternSettings)residueDefinition.pattern).footprint))
                    { aim = candidate; break; }
                }
                Require(aim != Vector3.zero, "valid actual field attack direction for " + mode);
                Move(home + aim * (mode == "gas-orb" ? 4.8f : isGas ? 2.3f : 2.6f));
                var gasAttack = actor.GetComponent<GasSacElitePattern>(); var residueAttack = actor.GetComponent<HardenedResidueElitePattern>();
                if (mode == "gas-tell") yield return Wait(() => gasAttack.Phase == GasSacPhase.Windup, 4, mode);
                else if (mode == "gas-orb") yield return Wait(() => gasAttack.ActiveOrb != null, 5, mode);
                else yield return Wait(() => residueAttack.Phase == (mode == "residue-tell" ? HardenedResiduePhase.Windup
                    : mode == "residue-drop" ? HardenedResiduePhase.Dropping : HardenedResiduePhase.Recovery), 5, mode);
                var owned = new List<GameObject>();
                if (isGas)
                { if (gasAttack.TelegraphObject != null) owned.Add(gasAttack.TelegraphObject); if (gasAttack.ActiveOrb != null) owned.Add(gasAttack.ActiveOrb.gameObject); }
                else
                { if (residueAttack.TelegraphObject != null) owned.Add(residueAttack.TelegraphObject); if (residueAttack.ActiveRubble != null) owned.Add(residueAttack.ActiveRubble.gameObject); }
                Require(owned.Count > 0 && actor.HasPatternDirections, "real field has active directional attack objects");
                var life = actor.GetComponent<EnemyPatternLifetime>(); uint generation = actor.SpawnGeneration;
                Move(biome.GridToWorldWithHeight(point.x < 150 ? 280 : 15, point.y < 150 ? 280 : 15));
                Require(!life.IsCurrent(generation) && life.OwnedObjectCount == 0 && owned.All(o => o == null || !o.activeSelf), "chunk unload immediately cleans attack ownership");
                Require(!SaveService.IsBiomeEliteDefeated(point.spawnId), "unload is not a defeat");
                yield return null;
                Move(home + Vector3.right * 4);
                var returned = Actor(point);
                Require(returned != null && returned.HasPatternDirections && (!ReferenceEquals(returned, actor) || returned.SpawnGeneration != generation)
                    && !returned.IsPatternFacingLocked && returned.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, "new field generation has clean direction and attack state");
                Require(JsonUtility.ToJson(field.Plan) == planJson, "active-attack chunk trip preserves exact saved plan");
                returned.ReleaseToPool();
                Pass(difficulty + " FIELD " + mode + ": actual chunk unload clears attack immediately; same point returns with fresh directional generation, no defeat/plan change");
            }
        }
    }
}
