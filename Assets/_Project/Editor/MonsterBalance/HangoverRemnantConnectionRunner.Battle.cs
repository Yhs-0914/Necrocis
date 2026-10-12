using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Necrocis;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class HangoverRemnantConnectionRunner
    {
        private static readonly float[] BattleAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };

        private static IEnumerator BattleChecks()
        {
            foreach (float direction in BattleAngles)
            {
                Spawn(); enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 1); Move(origin + Aim(direction) * 4);
                Vector3 start = enemy.GetComponent<Rigidbody>().position;
                yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Windup, 3, "actual pursuit");
                Require(Vector3.Dot(enemy.GetComponent<Rigidbody>().position - start, Aim(direction)) > 1.4f, "native pursuit reaches trigger distance");
                Require(!Body().flipX && settings.presentation.preparationFrames.Contains(Body().sprite), "pursuit keeps front presentation");
                Pass($"PURSUIT {direction:0}deg: real movement enters trigger range and begins front-facing preparation");
            }
            foreach (float direction in new[] { 0f, 90f, 180f, 270f })
            {
                Spawn(); Move(origin + Aim(direction) * 2.2f); yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Windup, 2, "contact tell");
                float hp = health.CurrentHealth;
                for (int n = 0; n < 35 && health.CurrentHealth == hp; n++)
                { player.TryMoveByWorld(-Aim(direction) * .07f); yield return new WaitForFixedUpdate(); }
                Equal(1, hp - health.CurrentHealth, "actual native body contact during tell");
                Require(health.IsInvincible, "contact activates invulnerability");
                Move(origin + Aim(direction + 90) * 3); yield return Wait(() => pattern.BurstCount == 1, 3, "contact then avoid burst");
                Equal(1, hp - health.CurrentHealth, "escaping burst causes no second damage");
                Pass($"CONTACT {direction:0}deg: walking into preparing body deals 1 once; lateral exit avoids burst");
            }
            foreach (bool crossCenter in new[] { false, true })
            {
                Spawn(); Move(origin + Vector3.right * 2.2f); yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Recovery, 3, "pursuit choice after body clears area");
                Move(origin + Vector3.forward * 1.3f); health.ResetHealth(); float hp = health.CurrentHealth;
                if (!crossCenter) yield return WalkTo(origin + Vector3.forward * 2.2f, 1);
                else yield return WalkTo(origin + Vector3.forward * .8f, 1);
                yield return Wait(() => pattern.BurstCount == 1, 2, "pursuit consequence");
                Equal(crossCenter ? 3 : 0, hp - health.CurrentHealth, "walk into remnant versus walk around");
                Pass(crossCenter ? "CHOICE continuing through the abandoned danger area takes one 3-damage burst" : "CHOICE ordinary walking around the full red area avoids all burst damage");
            }
            foreach (bool around in new[] { false, true })
            {
                Spawn(); Move(origin + Vector3.right * 2.2f); float hp = health.CurrentHealth;
                yield return Wait(() => pattern.ActiveRemnant != null, 2, "native pursuit release");
                if (around)
                {
                    yield return WalkTo(origin + new Vector3(2.2f, 0, 2.2f), 1);
                    yield return WalkTo(origin + new Vector3(-3.6f, 0, 2.2f), 1);
                    yield return WalkTo(origin + Vector3.left * 3.6f, 1);
                }
                else
                {
                    yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Recovery, 1, "straight approach after retreat");
                    yield return WalkTo(origin + Vector3.left * 1.2f, 1);
                }
                Require(pattern.Phase == HangoverRemnantPhase.Recovery, "native pursuit reaches recovery window");
                typeof(PlayerController).GetField("movement", Private).SetValue(player, Vector3.zero);
                typeof(PlayerController).GetField("lastMoveDirection", Private).SetValue(player, around ? Vector3.right : Vector3.left);
                Physics.SyncTransforms(); float enemyHp = enemy.Stats.CurrentHealth;
                typeof(PlayerAttack).GetMethod("MeleeAttack", Private).Invoke(player.GetComponent<PlayerAttack>(), null);
                Require(enemy.Stats.CurrentHealth < enemyHp && pattern.ReleaseCount == 1, "native Q punishes recovery without duplicate cycle");
                yield return Wait(() => pattern.BurstCount == 1, 2, "pursuit burst");
                Equal(around ? 0 : 3, hp - health.CurrentHealth, "actual straight versus around pursuit");
                Pass(around ? "PLAYER ROUTE: actual side detour reaches native Q recovery punish with zero incoming damage" : "PLAYER ROUTE: actual straight pursuit reaches native Q but standing in abandoned red area takes burst 3");
            }
            foreach (float direction in BattleAngles)
            foreach (float offset in new[] { -.025f, .025f })
            {
                Spawn(); Move(origin + Vector3.right * 2.2f); yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Recovery, 3, "edge recovery");
                // Keep the receiving target separate from the retreated body in this burst-only test.
                enemy.GetComponent<EnemyContactDamage>().SetDamageActive(false);
                Move(origin + Aim(direction) * (settings.burstRadius + offset)); health.ResetHealth(); float hp = health.CurrentHealth;
                CheckMarker(settings.burstRadius);
                yield return Wait(() => pattern.BurstCount == 1, 2, "eight-angle area edge");
                Equal(offset < 0 ? 3 : 0, hp - health.CurrentHealth, "actual red circle edge");
                yield return new WaitForSeconds(.3f); Equal(offset < 0 ? 3 : 0, hp - health.CurrentHealth, "no extra burst ticks");
                Pass($"AREA {direction:0}deg/{offset:+.000;-.000}m: actual ground-center damage={(offset < 0 ? 3 : 0)}, no delayed ticks (body isolated)");
            }
            Spawn(); Move(origin + Vector3.right * 2.2f); yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Windup, 2, "rapid hit tell");
            float began = Time.time;
            while (pattern.ReleaseCount == 0) { enemy.TakeDamage(1); Require(Time.time - began < .8f, "damage cannot extend preparation"); yield return new WaitForSeconds(.04f); }
            enemy.StatusEffects.ApplyPoison(.7f, .1f, 1);
            yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Ready, 3, "rapid hit finish");
            Require(pattern.ReleaseCount == 1 && pattern.BurstCount == 1 && pattern.ActiveRemnant == null, "rapid hits and DoT cannot multiply remnants");
            Pass("RAPID/DoT: repeated real damage cannot postpone preparation or create extra remnants/bursts");

            Spawn(); Move(origin + Vector3.right * 2.2f); yield return Wait(() => pattern.ActiveRemnant != null, 2, "stun release");
            enemy.StatusEffects.ApplyStun(1.5f); yield return null; Vector3 stopped = enemy.GetComponent<Rigidbody>().position;
            yield return new WaitForSeconds(.2f); Equal(0, Vector3.Distance(stopped, enemy.GetComponent<Rigidbody>().position), "stun stops retreat");
            Move(origin + Vector3.forward * 1.2f); health.ResetHealth(); float before = health.CurrentHealth;
            yield return Wait(() => pattern.BurstCount == 1, 2, "stunned timer"); Equal(3, before - health.CurrentHealth, "released remnant still bursts");
            Require(pattern.BurstAt - pattern.ReleasedAt >= 1.09f && pattern.BurstAt - pattern.ReleasedAt < 1.18f, "stun preserves original clock");
            Pass("STUN after release stops body movement; remnant keeps its original 1.1s timer and single damage");

            Set(settings, "rearmSeconds", 0); Set(settings, "recoverySeconds", .1f);
            Spawn(); Move(origin + Vector3.right * 2.2f); yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Recovery, 3, "zero cooldown recovery");
            while (pattern.ActiveRemnant != null)
            {
                Require(!pattern.TryBeginAttack() && pattern.ReleaseCount == 1, "live remnant forbids next cycle");
                Require(enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount <= 2, "single remnant and one warning max"); yield return null;
            }
            yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Ready, 1, "zero cooldown ready");
            Move(enemy.GetComponent<Rigidbody>().position + Vector3.forward * 2.2f);
            yield return Wait(() => pattern.ReleaseCount == 2, 2, "next independent cycle"); Restore();
            Pass("ZERO REARM: only one owned remnant even with .1s recovery and zero rearm; next cycle starts after cleanup");

            yield return TerrainChecks();
            yield return DeathChecks();
            yield return FieldUnloadChecks();
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

        private static IEnumerator TerrainChecks()
        {
            Vector3 saved = origin;
            foreach (float direction in BattleAngles)
            {
                Spawn(); Move(origin + Aim(direction) * 2.2f);
                var block = biome.WorldToGrid(origin - Aim(direction) * 2.4f);
                biome.AddRuntimeBlockedCells(new[] { block });
                try
                {
                    yield return new WaitForSeconds(1.05f);
                    Require(pattern.RejectedPaths > 0 && pattern.ReleaseCount == 0 && pattern.TelegraphObject == null, "wall/corner rejects full body retreat");
                    Equal(0, Vector3.Distance(origin, enemy.GetComponent<Rigidbody>().position), "cannot move into blocked cell");
                }
                finally { biome.RemoveRuntimeBlockedCells(new[] { block }); }
                Pass($"TERRAIN wall/corner {direction:0}deg: unsafe retreat rejected before warning or remnant");
            }
            foreach (int level in new[] { 0, 1 })
            {
                origin = FindPatch(level); Spawn(); Move(origin + Vector3.right * 2.2f);
                yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Recovery, 3, "elevated retreat");
                Equal(level * biome.HeightStep, pattern.ActiveRemnant.transform.position.y, "remnant on actual ground");
                Equal(level * biome.HeightStep, enemy.GetComponent<Rigidbody>().position.y, "body grounded at endpoint");
                Equal(level * biome.HeightStep + .055f, pattern.TelegraphObject.transform.position.y, "warning ground offset");
                Pass($"HEIGHT level={level}: body/remnant grounded at {level * biome.HeightStep:F2}m, red fill +.055m");
            }
            bool found = false; Vector2Int edge = default;
            for (int z = 5; z < biome.MapHeight - 5 && !found; z++) for (int x = 5; x < biome.MapWidth - 5 && !found; x++)
            {
                if (biome.GetHeightLevel(x, z) != 0 || biome.GetHeightLevel(x + 1, z) != 1) continue;
                bool clear = true;
                for (int dx = -4; dx <= 5 && clear; dx++) for (int dz = -2; dz <= 2 && clear; dz++)
                    clear = biome.IsWalkable(x + dx, z + dz) && biome.GetHeightLevel(x + dx, z + dz) == (dx <= 0 ? 0 : 1);
                if (clear) { found = true; edge = new Vector2Int(x, z); }
            }
            Require(found, "real height discontinuity fixture");
            foreach (bool uphill in new[] { true, false })
            {
                origin = biome.GridToWorldWithHeight(edge.x + (uphill ? -1 : 2), edge.y);
                Spawn(); Move(origin + (uphill ? Vector3.left : Vector3.right) * 2.2f);
                yield return new WaitForSeconds(1.05f);
                Require(pattern.RejectedPaths > 0 && pattern.TelegraphObject == null && pattern.ReleaseCount == 0, "height edge rejects retreat");
                Equal(0, Vector3.Distance(origin, enemy.GetComponent<Rigidbody>().position), "stays on original height");
                Pass(uphill ? "LEDGE real low-to-high boundary rejects retreat without crossing or warning" : "LEDGE real high-to-low boundary rejects retreat without hovering or warning");
            }
            origin = saved;
        }

        private static Vector3 FindPatch(int level)
        {
            for (int z = 6; z < biome.MapHeight - 6; z++) for (int x = 6; x < biome.MapWidth - 6; x++)
            {
                if (biome.GetHeightLevel(x, z) != level) continue; bool clear = true;
                for (int dx = -5; dx <= 5 && clear; dx++) for (int dz = -5; dz <= 5 && clear; dz++)
                    clear = biome.IsWalkable(x + dx, z + dz) && biome.GetHeightLevel(x + dx, z + dz) == level;
                if (clear) return biome.GridToWorldWithHeight(x, z);
            }
            throw new InvalidOperationException("missing clear patch at level " + level);
        }

        private static IEnumerator DeathChecks()
        {
            foreach (float direction in BattleAngles)
            foreach (bool afterRelease in new[] { false, true })
            {
                Spawn(); Move(origin + Aim(direction) * 2.2f);
                yield return Wait(() => afterRelease ? pattern.ActiveRemnant != null : pattern.Phase == HangoverRemnantPhase.Windup, 3, "directional death fixture");
                var warning = pattern.TelegraphObject; var prop = pattern.ActiveRemnant; var body = Body(); var life = enemy.GetComponent<EnemyPatternLifetime>(); uint gen = enemy.SpawnGeneration;
                enemy.TakeDamage(10000); Require((warning == null || !warning.activeSelf) && (prop == null || !prop.activeSelf), "death immediately hides all effects");
                Move(origin + Aim(direction + 180) * 1.3f); health.ResetHealth(); float hp = health.CurrentHealth;
                var frames = new HashSet<Sprite>(); float until = Time.time + 2;
                while (enemy.gameObject.activeSelf) { Require(Time.time < until && !body.flipX, "fixed front death finishes"); frames.Add(body.sprite); yield return null; }
                Require(settings.presentation.deathFrames.All(frames.Contains) && life.OwnedObjectCount == 0 && !life.IsCurrent(gen), "all death frames and no remaining ownership");
                Equal(hp, health.CurrentHealth, "dead body/remnant cannot hit");
                Spawn(); Require(enemy.SpawnGeneration != gen && pattern.ReleaseCount == 0 && pattern.BurstCount == 0 && !Body().flipX, "fresh pooled generation");
                Pass($"DEATH {direction:0}deg/{(afterRelease ? "released" : "tell")}: all 6 front frames, no old damage/effects, clean pool generation");
            }
        }

        private static IEnumerator FieldUnloadChecks()
        {
            enemy.ReleaseToPool(); origin = fieldOrigin; field.enabled = true; Move(origin + Vector3.right * 2.2f); field.Refresh(player.transform.position);
            enemy = field.Spawners.Single().ActiveEnemy; pattern = enemy.GetComponent<HangoverRemnantElitePattern>();
            yield return Wait(() => pattern.ActiveRemnant != null, 3, "real field release");
            var prop = pattern.ActiveRemnant; var life = enemy.GetComponent<EnemyPatternLifetime>(); uint gen = enemy.SpawnGeneration;
            var point = field.Plan.placements.Single();
            Move(biome.GridToWorldWithHeight(point.x < 150 ? 280 : 15, point.y < 150 ? 280 : 15)); field.Refresh(player.transform.position); yield return null;
            Require(prop == null && !life.IsCurrent(gen) && !SaveService.IsBiomeEliteDefeated(point.spawnId), "chunk unload removes effect without defeat");
            Move(origin + Vector3.right * 5); field.Refresh(player.transform.position);
            enemy = field.Spawners.Single().ActiveEnemy; pattern = enemy.GetComponent<HangoverRemnantElitePattern>();
            Require(pattern.ReleaseCount == 0 && pattern.ActiveRemnant == null && !Body().flipX, "field returns clean");
            field.enabled = false; enemy = null; origin = FindPatch(0);
            Pass("CHUNK: real dedicated field unload removes live remnant without a kill; returning spawns clean front-facing state");
        }

        private static IEnumerator VisitChecks()
        {
            var point = field.Plan.placements.Single(); string spawnId = point.spawnId;
            Require(SaveService.TrySaveActiveRun(out string error), error); string run = SaveService.ActiveRunId, visit = biome.MonsterVisit.VisitId;
            SaveService.UseStorageRootForTests(storage); Require(SaveService.TryContinue(GameDifficulty.Normal, out error), error);
            Require(SaveService.TryRestorePendingSession(out BiomeType resume, out error) && resume == BiomeType.Liver, error);
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LIVER); yield return null; BindBattleScene();
            Require(SaveService.ActiveRunId == run && biome.MonsterVisit.VisitId == visit && SaveService.IsBiomeEliteDefeated(spawnId), "disk continue preserves visit and kill");
            field.Configure(biome, temporary, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Move(biome.GridToWorldWithHeight(point.x, point.y)); field.Refresh(player.transform.position);
            Require(field.Spawners.Count == 0, "disk-loaded defeated field does not respawn"); field.enabled = false;
            Pass("SAVE: actual disk save/continue and scene reload preserve visit and defeated H-03; no respawn");
            var catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath);
            foreach (var difficulty in new[] { GameDifficulty.Normal, GameDifficulty.Hard })
            {
                if (enemy != null && enemy.gameObject.activeSelf) enemy.ReleaseToPool();
                SaveService.MarkFinalBossDefeated(); Require(SaveService.TryBeginNewGame(difficulty, out error), error);
                Require(SaveService.TryRestorePendingSession(out _, out error), error);
                GameManager.Instance.EnterBiome(BiomeType.Liver); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LIVER); yield return null; BindBattleScene();
                foreach (int stage in new[] { 0, 1, 2, 3, 4 })
                {
                    Require(biome.MonsterVisit.Stage == stage && biome.MonsterVisit.Difficulty == difficulty, $"actual visit captures stage/difficulty: expected={difficulty}/{stage}, actual={biome.MonsterVisit.Difficulty}/{biome.MonsterVisit.Stage}");
                    origin = FindPatch(0); Spawn(); enemy.ApplyBalancePhase("Default", true);
                    var source = definition.statSets.Single(s => s.id == "Default"); var multi = catalog.difficultyCatalog.Get(difficulty).elites;
                    Require(catalog.progression.TryGetStage(stage, out var progression), "stage source exists");
                    Equal(CharacterStats.ToHealthUnits(source.maxHealth * multi.maxHealth * progression.maxHealth), enemy.Stats.MaxHealth, "HP multipliers once");
                    Equal(source.moveSpeed * multi.moveSpeed, enemy.Stats.MoveSpeed, "move speed excludes progression");
                    float raw = source.attackPower * multi.outgoingDamage * progression.attackPower * definition.patternDamage.Single(p => p.id == HangoverRemnantPatternSettings.DamageId).coefficient;
                    Equal(raw, enemy.CreatePatternDamage(HangoverRemnantPatternSettings.DamageId, 0).Amount, "damage multipliers once");
                    enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0); Move(origin + Vector3.right * 2.2f);
                    yield return Wait(() => pattern.ActiveRemnant != null, 3, "stage live burst"); Move(origin + Vector3.forward * 1.3f); health.ResetHealth(); float hp = health.CurrentHealth;
                    yield return Wait(() => pattern.BurstCount == 1, 2, "stage actual damage"); Equal(CharacterStats.ToHealthUnits(raw), hp - health.CurrentHealth, "live burst actual HP");
                    Equal(1.6f, pattern.BurstRadius, "stage does not expand danger area"); Equal(.65f, pattern.WindupDuration, "stage does not compress warning");
                    yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Ready, 2, "stage rearm");
                    Require(Mathf.Abs(pattern.NextReadyTime - Time.time - settings.rearmSeconds * multi.attackCooldown) < .06f, "one difficulty rearm multiplier");
                    int expectedXp = (int)Math.Round((double)definition.reward.baseExperience * multi.experienceReward * progression.experience, MidpointRounding.ToEven), xp = 0;
                    enemy.SuppressExperienceReward = false; Action<int> observe = amount => xp += amount; var levelUp = LevelUpManager.OnLevelUp;
                    LevelUpManager.OnLevelUp = null; LevelUpManager.OnExpGained += observe;
                    try { enemy.TakeDamage(10000); enemy.TakeDamage(10000); enemy.GrantExp(); }
                    finally { LevelUpManager.OnLevelUp = levelUp; LevelUpManager.OnExpGained -= observe; }
                    Equal(expectedXp, xp, "actual reward once");
                    yield return Wait(() => !enemy.gameObject.activeSelf, 2, "stage death");
                    Pass($"BALANCE {difficulty}/stage{stage}: actual HP, burst={CharacterStats.ToHealthUnits(raw)}, XP={xp} once, movement/radius/tell unchanged, single rearm multiplier");
                    if (stage == 4) continue;
                    GameManager.Instance.CollectRelic(new[] { BiomeType.Intestine, BiomeType.Lung, BiomeType.Stomach, BiomeType.Liver }[stage]);
                    Require(biome.MonsterVisit.Stage == stage, "current visit stays frozen");
                    Spawn(); Require(enemy.Balance.Current.Stage == stage, "same visit respawn stays frozen"); enemy.ReleaseToPool();
                    GameManager.Instance.ReturnToHub(); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_HUB);
                    GameManager.Instance.EnterBiome(BiomeType.Liver); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LIVER); yield return null; BindBattleScene();
                }
            }
        }

        private static void BindBattleScene()
        {
            biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>(); field = biome.GetComponent<BiomeEliteField>();
            field.enabled = false; enemy = null; pattern = null;
            Require(biome.GetBiomeConfig().biomeEliteSpawnConfig.monsters.Count == productionLiverCount, "production Liver unchanged");
            ((IList)typeof(ProceduralBiomeBridge).GetField("normalEnemyRules", Private).GetValue(biome)).Clear();
            if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
            typeof(InputManager).GetMethod("SetActionsEnabled", Private).Invoke(InputManager.Instance, new object[] { false });
            groundOffset = player.transform.position.y - biome.GetGroundHeight(player.transform.position);
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
        }
    }
}
