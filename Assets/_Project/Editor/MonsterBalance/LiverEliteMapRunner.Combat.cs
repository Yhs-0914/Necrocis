using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class LiverEliteMapRunner
    {
        private static EnemyController ember, hangover;
        private static InflammationEmberElitePattern emberPattern;
        private static HangoverRemnantElitePattern hangoverPattern;
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
            throw new InvalidOperationException("No open two-species combat patch in actual Liver terrain");
        }

        private static void StagePair(Vector2Int cell, Vector3 aim)
        {
            var h = field.Plan.placements.Single(p => p.monsterId == hangoverDefinition.monsterId);
            var e = field.Plan.placements.Single(p => p.monsterId == emberDefinition.monsterId);
            Vector3 normal = Vector3.Cross(aim, Vector3.up), offset = aim * 2 + normal * 3;
            // Only the isolated, unsaved test run's placements are rearranged for combat stress.
            h.x = cell.x; h.y = cell.y; e.x = cell.x + Mathf.RoundToInt(offset.x); e.y = cell.y + Mathf.RoundToInt(offset.z);
            encounterOrigin = biome.GridToWorldWithHeight(cell.x, cell.y);
            field.Configure(biome, temporaryConfig, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            ResetPair();
        }

        private static void ResetPair(bool mobile = false)
        {
            foreach (var s in field.Spawners.ToArray()) s.ReleaseEnemy();
            Move(encounterOrigin + Vector3.right * 6);
            ember = field.Spawners.Single(s => s.Placement.monsterId == emberDefinition.monsterId).ActiveEnemy;
            hangover = field.Spawners.Single(s => s.Placement.monsterId == hangoverDefinition.monsterId).ActiveEnemy;
            Require(ember != null && hangover != null && ember.Balance.ContactEnabled && hangover.Balance.ContactEnabled, "dedicated live pair with body contact");
            emberPattern = ember.GetComponent<InflammationEmberElitePattern>(); hangoverPattern = hangover.GetComponent<HangoverRemnantElitePattern>();
            Require(ember.HasPatternDirections && !hangover.HasPatternDirections && !Body(hangover).flipX, "per-species facing contract");
            if (!mobile) { ember.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0); hangover.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0); }
            ember.Stats.SetBaseStat(CharacterStatType.MaxHealth, 200, true); hangover.Stats.SetBaseStat(CharacterStatType.MaxHealth, 200, true);
            health.ResetHealth();
        }

        private static IEnumerator CombinedCombat()
        {
            Require(field.Plan == null, "isolated combo starts without production plan");
            temporaryConfig = ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>();
            temporaryConfig.minimumCount = temporaryConfig.maximumCount = 2; temporaryConfig.clearanceCells = 5;
            temporaryConfig.monsters.Add(InflammationEmberSetup.PreviewRule(emberDefinition));
            temporaryConfig.monsters.Add(HangoverRemnantSetup.PreviewRule(hangoverDefinition));
            field.Configure(biome, temporaryConfig, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Require(field.Plan.placements.Count == 2, "one of each temporary dedicated point at zero kills");
            if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
            Vector2Int cell = FindPairPatch();
            Pass(difficulty + " COMBO: real Liver terrain, dedicated one-of-each spawners at zero kills; production source untouched");

            foreach (float angle in preview ? new[] { 90f } : Angles)
            {
                Vector3 aim = Aim(angle), normal = Vector3.Cross(aim, Vector3.up);
                StagePair(cell, aim);
                foreach (float side in preview ? new[] { -1f } : new[] { -1f, 1f })
                {
                    float detour = side > 0 ? 4.4f : 2.2f;
                    ResetPair(); Move(encounterOrigin + aim * 2.2f); health.ResetHealth(); float hp = health.CurrentHealth;
                    yield return Wait(() => hangoverPattern.Phase == HangoverRemnantPhase.Windup, 3, "combined remnant tell");
                    ember.TakeDamage(1); Require(emberPattern.Phase == InflammationEmberPhase.Windup, "damage starts one reactive counter");
                    var facing = ember.PatternFacing; Vector3 locked = emberPattern.LockedDirection, center = hangoverPattern.RemnantCenter;
                    ElitePresentationChecks.RedArea(hangoverPattern.TelegraphObject, Vector2.one * 3.2f);
                    if (preview && previewPhase == "tell") { yield return new WaitForSeconds(.25f); yield break; }
                    yield return WalkTo(encounterOrigin + aim * 4 + normal * (side * detour), 1);
                    yield return Wait(() => emberPattern.ActiveThorn != null && hangoverPattern.ActiveRemnant != null, 2, "both live effects");
                    var thorn = emberPattern.ActiveThorn;
                    Equal(.16f, thorn.FlightHeight, "grounded counter height");
                    Equal(biome.GetGroundHeight(thorn.transform.position) + .16f, thorn.GetComponentInChildren<SpriteRenderer>().transform.position.y, "actual moving thorn sprite height");
                    Equal(0, (thorn.Direction - locked).magnitude, "counter does not turn with dodge");
                    Require(thorn.transform.childCount == 1 && thorn.transform.GetChild(0).name == "ThornSprite", "simple thorn has no floor UI");
                    Require(ember.PatternFacing == facing && ember.IsPatternFacingLocked && !Body(hangover).flipX, "counter commits direction; remnant stays front");
                    if (preview) { yield return new WaitForSeconds(.4f); yield break; }
                    yield return WalkTo(encounterOrigin - aim * 3.6f + normal * (side * detour), 1.2f);
                    yield return WalkTo(encounterOrigin - aim * 3.6f, 1);
                    Require(hangoverPattern.Phase == HangoverRemnantPhase.Recovery, "native detour reaches recovery window");
                    Face(aim); Physics.SyncTransforms(); float enemyHp = hangover.Stats.CurrentHealth;
                    typeof(PlayerAttack).GetMethod("MeleeAttack", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(player.GetComponent<PlayerAttack>(), null);
                    Require(hangover.Stats.CurrentHealth < enemyHp, "native Q punishes recovering remnant in mixed encounter");
                    yield return Wait(() => hangoverPattern.BurstCount == 1 && emberPattern.ActiveThorn == null, 3, "both threats finish");
                    Equal(hp, health.CurrentHealth, "combined dodge takes no damage");
                    Equal(0, (center - hangoverPattern.RemnantCenter).magnitude, "remnant never follows player");
                    Require(emberPattern.ShotCount == 1 && hangoverPattern.ReleaseCount == 1, "no extra counters or remnants");
                    Pass($"{difficulty} COMBO {angle:0}deg/side{side:+0;-0}: native walk avoids .16m thorn and red circle, Q punishes recovery, damage 0");
                }
            }

            StagePair(cell, Vector3.right); ResetPair(true); Move(encounterOrigin + Vector3.right * 5);
            Vector3 startH = hangover.GetComponent<Rigidbody>().position, startE = ember.GetComponent<Rigidbody>().position;
            yield return new WaitForSeconds(.4f);
            Require(Distance(startH, hangover.GetComponent<Rigidbody>().position) > .2f && Distance(startE, ember.GetComponent<Rigidbody>().position) > .05f, "both production-speed patterns move");
            Require(emberPattern.ShotCount == 0 && hangoverPattern.ReleaseCount == 0, "unprovoked distant pair does not fire");
            Pass(difficulty + " MOBILE: both actual base-speed patterns pursue; distant untouched ember never auto-fires");

            foreach (bool isEmber in new[] { true, false })
            {
                ResetPair(); var actor = isEmber ? ember : hangover;
                foreach (var p in actor.GetComponents<MonsterPatternController>()) p.EndSpawn(); actor.SetAiSuppressed(true);
                Vector3 away = isEmber ? Vector3.forward : Vector3.left;
                Vector3 home = actor.GetComponent<Rigidbody>().position; Move(home + away * 2.2f); health.ResetHealth(); float hp = health.CurrentHealth;
                for (int n = 0; n < 60 && health.CurrentHealth == hp; n++) { player.TryMoveByWorld(-away * .05f); yield return new WaitForFixedUpdate(); }
                Equal(CharacterStats.ToHealthUnits(actor.CreateContactDamage(0).Amount), hp - health.CurrentHealth, "real pair body contact");
                Require(health.IsInvincible, "contact grants hurt invulnerability");
                Pass(difficulty + (isEmber ? " CONTACT H-02" : " CONTACT H-03") + ": actual walking collision applies one body hit (attack isolated)");
            }

            StagePair(cell, Vector3.right); Move(encounterOrigin + Vector3.right * 2.2f);
            yield return Wait(() => hangoverPattern.ActiveRemnant != null, 3, "kill-remnant-first release");
            var prop = hangoverPattern.ActiveRemnant; var warning = hangoverPattern.TelegraphObject;
            hangover.SuppressExperienceReward = true; hangover.TakeDamage(10000);
            Require(!prop.activeSelf && !warning.activeSelf && hangover.IsDeathAnimPlaying, "finishing remnant owner immediately clears danger");
            Move(ember.GetComponent<Rigidbody>().position + Vector3.right * 3); health.ResetHealth(); float before = health.CurrentHealth;
            ember.TakeDamage(1); yield return WalkTo(ember.GetComponent<Rigidbody>().position + new Vector3(3, 0, -2), 1);
            yield return Wait(() => emberPattern.ShotCount == 1, 2, "remaining ember counter");
            yield return Wait(() => emberPattern.ActiveThorn == null, 3, "remaining shot expiry");
            Equal(before, health.CurrentHealth, "kill-remnant-first leaves safe counter dodge");
            Require(hangoverPattern.BurstCount == 0 && field.Plan.placements.Any(p => SaveService.IsBiomeEliteDefeated(p.spawnId)), "killed remnant does not explode and field records death");
            ember.SuppressExperienceReward = true; ember.TakeDamage(10000);
            yield return Wait(() => !ember.gameObject.activeSelf && !hangover.gameObject.activeSelf, 2, "pair dedicated deaths"); field.Refresh(player.transform.position);
            Require(field.Spawners.Count == 0 && Object.FindObjectsByType<EmberThorn>(FindObjectsSortMode.None).Length == 0, "dead pair does not respawn or retain shots");
            Pass(difficulty + " PRIORITY: kill released remnant first clears danger immediately; dodge remaining counter, both dedicated deaths finish and points stay dead");
        }
    }
}
