using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class LiverEliteMapRunner
    {
        private static EnemyController Actor(BiomeElitePlacement point) => field.Spawners.Single(s => s.Placement.spawnId == point.spawnId).ActiveEnemy;
        private static Vector3 Home(BiomeElitePlacement point) => biome.GridToWorldWithHeight(point.x, point.y);

        private static IEnumerator ProductionChecks()
        {
            Require(field.Plan != null, "production field is automatic on real Liver entry"); CheckPlan();
            if (preview)
            {
                var point = field.Plan.placements.Single(p => p.monsterId == previewPhase);
                bool isEmber = point.monsterId == emberDefinition.monsterId;
                Move(Home(point) + Vector3.right * (isEmber ? 3.5f : 2.2f)); var actor = Actor(point);
                if (isEmber)
                {
                    yield return new WaitForSeconds(.2f); actor.TakeDamage(1);
                    var pattern = actor.GetComponent<InflammationEmberElitePattern>();
                    yield return Wait(() => pattern.ActiveThorn != null, 2, "real map flight preview");
                    yield return new WaitForSeconds(.15f);
                }
                else
                {
                    var pattern = actor.GetComponent<HangoverRemnantElitePattern>();
                    yield return Wait(() => pattern.ActiveRemnant != null, 3, "real map remnant preview");
                    Move(pattern.RemnantCenter + Vector3.forward * 3.5f);
                    yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Recovery, 2, "real map retreat preview");
                    yield return new WaitForSeconds(.05f);
                }
                Selection.activeGameObject = actor.gameObject; yield break;
            }
            string planJson = JsonUtility.ToJson(field.Plan), runId = SaveService.ActiveRunId;
            var points = field.Plan.placements.ToArray();
            foreach (var p in points) { Move(Home(p) + Vector3.right * 4); CheckBalance(Actor(p), 0); }
            Pass(difficulty + " MAP: automatic one H-02 + one H-03, zero kills, >=18m spacing, 5-cell flat clearance, entrance/portal/boss exclusions");

            var legacy = biome.GetComponent<EliteSpawner>(); Require(legacy != null && legacy.enabled, "legacy subsystem intact");
            var names = (HashSet<string>)typeof(EliteSpawner).GetField("normalEnemyNames", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(legacy);
            var configs = (List<EnemySpawnRuleConfig>)typeof(EliteSpawner).GetField("activeEliteConfigs", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(legacy);
            Require(configs.All(c => c.monsterDefinition != null && c.monsterDefinition.monsterId.StartsWith("legacy-elite.")), "legacy pool excludes biome elites");
            int oldKills = (int)typeof(EliteSpawner).GetField("totalNormalKills", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(legacy);
            bool hasLegacy = names.Count > 0 && configs.Count > 0;
            for (int n = 0; n < 10; n++) legacy.NotifyEnemyKilled(names.Count > 0 ? names.First() : "Macrophage");
            Equal(oldKills + (hasLegacy ? 10 : 0), (int)typeof(EliteSpawner).GetField("totalNormalKills", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(legacy), "legacy counter follows only its own registered rules");
            if (hasLegacy)
            {
                yield return new WaitForSeconds(3.5f);
                Require(Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Any(e => e.Balance != null && e.Balance.Current.MonsterId.StartsWith("legacy-elite.")), "legacy threshold spawns only legacy actor");
            }
            Require(JsonUtility.ToJson(field.Plan) == planJson, "ten kills cannot add or move biome elites"); legacy.enabled = false;
            foreach (var e in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None)) if (e.Balance != null && e.Balance.Current.MonsterId.StartsWith("legacy-elite.")) e.ReleaseToPool();
            Pass(difficulty + (hasLegacy ? " SPAWN: legacy ten-kill spawn remains independent; biome plan stays fixed" : " SPAWN: existing Liver normal/legacy list is empty; ten kill notifications cannot spawn or change biome elites"));

            foreach (var p in points)
            {
                Move(Home(p) + Vector3.right * 3); Actor(p).ReleaseToPool(); field.Refresh(player.transform.position);
                var e = Actor(p); e.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0);
                if (e.Balance.Current.MonsterId == emberDefinition.monsterId)
                {
                    e.TakeDamage(1); yield return Wait(() => e.GetComponent<InflammationEmberElitePattern>().ActiveThorn != null, 2, "field thorn before unload");
                }
                else
                {
                    Move(Home(p) + Vector3.right * 2.2f); yield return Wait(() => e.GetComponent<HangoverRemnantElitePattern>().ActiveRemnant != null, 3, "field remnant before unload");
                }
                var life = e.GetComponent<EnemyPatternLifetime>(); uint generation = e.SpawnGeneration;
                Require(life.OwnedObjectCount > 0, "actual field owns an active attack");
                Move(biome.GridToWorldWithHeight(p.x < 150 ? 280 : 15, p.y < 150 ? 280 : 15)); yield return null;
                Require(!life.IsCurrent(generation) && life.OwnedObjectCount == 0 && !SaveService.IsBiomeEliteDefeated(p.spawnId), "unload cleans active effects without kill");
                Move(Home(p) + Vector3.right * 4); var returned = Actor(p);
                Require(returned != null && (!ReferenceEquals(returned, e) || returned.SpawnGeneration != generation)
                    && returned.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0 && JsonUtility.ToJson(field.Plan) == planJson, "clean same-point return");
                Pass(difficulty + " CHUNK " + p.monsterId + ": live attack removed, no defeat, fresh same-point actor and exact saved plan");
            }

            var tunedPoint = points.First(p => p.monsterId == hangoverDefinition.monsterId);
            Move(Home(tunedPoint) + Vector3.right * 4); var oldActor = Actor(tunedPoint); float oldHp = oldActor.Stats.MaxHealth;
            Set(hangoverDefinition, "statSets.Array.data[0].maxHealth", 42); Set(hangoverDefinition, "statSets.Array.data[0].attackPower", 4);
            Set(DifficultyBalanceService.GetProfile(difficulty), "elites.maxHealth", 1.5f); Set(DifficultyBalanceService.GetProfile(difficulty), "elites.outgoingDamage", 1.5f);
            int coefficient = hangoverDefinition.patternDamage.FindIndex(p => p.id == HangoverRemnantPatternSettings.DamageId);
            Set(hangoverDefinition, $"patternDamage.Array.data[{coefficient}].coefficient", 2);
            Equal(oldHp, oldActor.Stats.MaxHealth, "old field actor keeps snapshot"); oldActor.ReleaseToPool(); field.Refresh(player.transform.position);
            Equal(63, Actor(tunedPoint).Stats.MaxHealth, "new field actor captures Inspector HP");
            yield return AttackAtPoint(tunedPoint, 12); RestoreAssets(); Actor(tunedPoint).ReleaseToPool(); field.Refresh(player.transform.position); CheckBalance(Actor(tunedPoint), 0);
            Pass(difficulty + " INSPECTOR: new production actor HP63 and real burst12; existing snapshot stable, all sources restored");

            for (int stage = 0; stage <= 4; stage++)
            {
                Require(biome.MonsterVisit.Difficulty == difficulty && biome.MonsterVisit.Stage == stage, "real visit difficulty/stage");
                foreach (var p in points)
                {
                    Move(Home(p) + Vector3.right * 4); Actor(p).ReleaseToPool(); field.Refresh(player.transform.position);
                    var e = Actor(p); CheckBalance(e, stage);
                    string id = p.monsterId == emberDefinition.monsterId ? InflammationEmberPatternSettings.DamageId : HangoverRemnantPatternSettings.DamageId;
                    float expected = CharacterStats.ToHealthUnits(e.CreatePatternDamage(id, 0).Amount);
                    yield return AttackAtPoint(p, expected);
                    Pass($"{difficulty} VISIT stage{stage}/{p.monsterId}: actual field HP/speed/XP snapshot, live attack damage={expected}, original tell/radius/facing");
                }
                if (stage == 4) break;
                GameManager.Instance.CollectRelic(new[] { BiomeType.Intestine, BiomeType.Lung, BiomeType.Stomach, BiomeType.Liver }[stage]);
                Require(biome.MonsterVisit.Stage == stage, "stage freezes during visit");
                Move(Home(points[0]) + Vector3.right * 4); Actor(points[0]).ReleaseToPool(); field.Refresh(player.transform.position); CheckBalance(Actor(points[0]), stage);
                GameManager.Instance.ReturnToHub(); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_HUB);
                GameManager.Instance.EnterBiome(BiomeType.Liver); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LIVER); yield return null; BindScene();
                if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
                Require(JsonUtility.ToJson(field.Plan) == planJson, "reentry preserves exact map plan");
            }
            foreach (var p in points) yield return KillPoint(p);
            string visitId = biome.MonsterVisit.VisitId;
            Require(SaveService.TrySaveActiveRun(out string error), error); SaveService.UseStorageRootForTests(storage);
            Require(SaveService.TryContinue(difficulty, out error), error);
            Require(SaveService.TryRestorePendingSession(out BiomeType resume, out error) && resume == BiomeType.Liver, error);
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LIVER); yield return null; BindScene();
            Require(SaveService.ActiveRunId == runId && biome.MonsterVisit.VisitId == visitId && biome.MonsterVisit.Stage == 4, "disk reload exact visit");
            Require(JsonUtility.ToJson(field.Plan) == planJson && points.All(p => SaveService.IsBiomeEliteDefeated(p.spawnId)), "disk retains both deaths and placements");
            foreach (var p in points) { Move(Home(p)); Require(!field.Spawners.Any(s => s.Placement.spawnId == p.spawnId), "dead point remains absent"); }
            Pass(difficulty + " SAVE: actual disk continue restores same run/visit/stage4/plan; both species stay dead after reload");

            Set(source, "minimumCount", 4); Set(source, "maximumCount", 4); Set(source, "minimumSpacing", 24);
            Require(JsonUtility.ToJson(field.Plan) == planJson, "Inspector density cannot rewrite current saved plan");
            Require(SaveService.TryBeginNewGame(difficulty, out error), error); Require(SaveService.TryRestorePendingSession(out _, out error), error);
            GameManager.Instance.EnterBiome(BiomeType.Liver); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LIVER); yield return null; BindScene(); CheckPlan();
            Require(field.Plan.placements.Count == 4 && biome.MonsterVisit.Stage == 0 && SaveService.ActiveRunId != runId, "new run uses edited count and resets progress");
            Pass(difficulty + " DENSITY: Inspector count2->4 and spacing18->24 apply only to new run; old saved plan untouched");
            RestoreAssets();
            Require(SaveService.TryBeginNewGame(difficulty, out error), error); Require(SaveService.TryRestorePendingSession(out _, out error), error);
            GameManager.Instance.EnterBiome(BiomeType.Liver); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LIVER); yield return null; BindScene(); CheckPlan();
            Require(field.Plan.placements.Count == 2 && points.All(p => !SaveService.IsBiomeEliteDefeated(p.spawnId)), "baseline new run resets defeated IDs");
            foreach (var p in field.Plan.placements.ToArray()) yield return KillPoint(p);
            Pass(difficulty + " NEW RUN: restored one-of-each source, stage0, fresh defeat state and XP50 once per actual field kill");
        }

        private static IEnumerator AttackAtPoint(BiomeElitePlacement point, float expected)
        {
            var e = Actor(point); e.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0);
            bool isEmber = point.monsterId == emberDefinition.monsterId; Vector3 home = e.GetComponent<Rigidbody>().position;
            Move(home + Vector3.right * (isEmber ? 3 : 2.2f)); health.ResetHealth(); float hp = health.CurrentHealth;
            if (isEmber)
            {
                var p = e.GetComponent<InflammationEmberElitePattern>();
                yield return Wait(() => p.Phase == InflammationEmberPhase.Ready && Time.time >= p.NextReadyTime, 6, "counter ready");
                e.TakeDamage(1); yield return Wait(() => p.ActiveThorn != null, 2, "production counter launch");
                Equal(.16f, p.ActiveThorn.FlightHeight, "real map low flight");
                Equal(biome.GetGroundHeight(p.ActiveThorn.transform.position) + .16f, p.ActiveThorn.GetComponentInChildren<SpriteRenderer>().transform.position.y, "actual thorn sprite height"); Equal(.7f, p.WindupDuration, "counter tell fixed");
                Require(p.ActiveThorn.transform.childCount == 1 && e.HasPatternDirections, "no projectile floor UI, correct directions");
                yield return Wait(() => health.CurrentHealth < hp, 3, "actual field thorn hit");
            }
            else
            {
                var p = e.GetComponent<HangoverRemnantElitePattern>();
                yield return Wait(() => p.ActiveRemnant != null, 4, "production remnant release");
                Equal(.65f, p.WindupDuration, "remnant tell fixed"); Equal(1.6f, p.BurstRadius, "radius fixed");
                Move(p.RemnantCenter + Vector3.forward * 1.3f); health.ResetHealth(); hp = health.CurrentHealth;
                Require(!Body(e).flipX && !e.HasPatternDirections, "front-fixed body in actual field");
                yield return Wait(() => p.BurstCount == 1, 2, "actual field burst hit");
            }
            Equal(expected, hp - health.CurrentHealth, "real production attack HP delta");
            Move(home + Vector3.right * 5);
        }

        private static IEnumerator KillPoint(BiomeElitePlacement point)
        {
            Move(Home(point) + Vector3.right * 4); var e = Actor(point); int xp = 0, expected = e.Balance.Current.Experience;
            Action<int> observe = value => xp += value; var callback = LevelUpManager.OnLevelUp;
            LevelUpManager.OnExpGained += observe; LevelUpManager.OnLevelUp = null;
            try { e.TakeDamage(10000); e.TakeDamage(10000); e.GrantExp(); }
            finally { LevelUpManager.OnExpGained -= observe; LevelUpManager.OnLevelUp = callback; }
            Equal(expected, xp, "actual reward once"); Require(SaveService.IsBiomeEliteDefeated(point.spawnId), "field death saved");
            var body = Body(e); var frames = new HashSet<Sprite>(); bool flip = body.flipX; var facing = e.PatternFacing;
            var art = (InflammationEmberPatternSettings)emberDefinition.pattern;
            var wanted = point.monsterId == emberDefinition.monsterId
                ? art.presentation.deathFrames.Select(s => art.presentation.directionalPresentation.Capture().Resolve(s, facing)).ToArray()
                : ((HangoverRemnantPatternSettings)hangoverDefinition.pattern).presentation.deathFrames;
            float until = Time.time + 2;
            while (e.gameObject.activeSelf) { Require(Time.time < until && body.flipX == flip, "death animation retains facing"); frames.Add(body.sprite); yield return null; }
            Require(wanted.All(frames.Contains), "all six real field death frames"); field.Refresh(player.transform.position);
            Require(!field.Spawners.Any(s => s.Placement.spawnId == point.spawnId), "dead point removed");
            Pass($"{difficulty} DEATH {point.monsterId}: all 6 frames, XP{expected} exactly once, saved defeat and no respawn");
        }

        private static void CheckPlan()
        {
            Require(source.GetValidationError() == null && field.Plan.placements.Count == source.maximumCount, "full sparse placement count");
            foreach (var rule in source.monsters) Require(field.Plan.placements.Count(p => p.monsterId == rule.monsterDefinition.monsterId) >= source.minimumPerType, "minimum each type");
            var entrance = biome.WorldToGrid(biome.GetPlayerSpawnPosition()); var arena = biome.GetBiomeConfig().GetMidBossArenaConfig(); var portal = biome.GetBiomeConfig().GetReturnPortalConfig();
            var boss = arena.useCustomCenter ? arena.centerGrid : new Vector2Int(biome.MapWidth / 2, biome.MapHeight / 2);
            float pad = source.bossExclusionPadding + arena.wallThicknessInCells + (arena.presentation != null && arena.presentation.enabled ? arena.presentation.approachLengthInCells : 0);
            foreach (var p in field.Plan.placements)
            {
                var cell = new Vector2Int(p.x, p.y); int level = biome.GetHeightLevel(p.x, p.y);
                Require(Vector2Int.Distance(cell, entrance) >= source.entranceExclusionRadius, "entrance clear");
                if (portal != null && portal.enabled) Require(Vector2Int.Distance(cell, portal.useCustomPosition ? portal.gridPosition : entrance) > source.portalExclusionRadius, "portal clear");
                if (arena.enabled) Require(Mathf.Abs(p.x - boss.x) > arena.arenaSize.x / 2f + pad || Mathf.Abs(p.y - boss.y) > arena.arenaSize.y / 2f + pad, "boss approach clear");
                for (int dx = -source.clearanceCells; dx <= source.clearanceCells; dx++) for (int dz = -source.clearanceCells; dz <= source.clearanceCells; dz++)
                    Require(biome.IsWalkable(p.x + dx, p.y + dz) && biome.GetHeightLevel(p.x + dx, p.y + dz) == level, "flat body/retreat/detour clearance");
                foreach (var q in field.Plan.placements) if (p != q) Require(Vector2Int.Distance(cell, new Vector2Int(q.x, q.y)) >= source.minimumSpacing, "cross-species spacing");
            }
            foreach (string other in new[] { "Stomach" }) Require(AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>("Assets/_Project/Data/BiomeElites/" + other + "BiomeEliteSpawnConfig.asset").monsters.Count == 2, "completed Stomach registration retained");
        }

        private static void CheckBalance(EnemyController e, int stage)
        {
            var definition = e.Balance.Current.MonsterId == emberDefinition.monsterId ? emberDefinition : hangoverDefinition;
            var catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath); var m = catalog.difficultyCatalog.Get(difficulty).elites;
            Require(catalog.progression.TryGetStage(stage, out var p), "progression source"); var stats = definition.statSets.Single(s => s.id == "Default");
            Require(e.Balance.Current.Stage == stage && e.Balance.Current.Difficulty == difficulty && e.Balance.ContactEnabled, "field captures current visit and body contact");
            Equal(CharacterStats.ToHealthUnits(stats.maxHealth * m.maxHealth * p.maxHealth), e.Stats.MaxHealth, "HP once");
            Equal(stats.moveSpeed * m.moveSpeed, e.Stats.MoveSpeed, "speed excludes progression");
            Equal((int)Math.Round((double)definition.reward.baseExperience * m.experienceReward * p.experience, MidpointRounding.ToEven), e.Balance.Current.Experience, "XP once");
            foreach (var d in definition.patternDamage) Equal(stats.attackPower * m.outgoingDamage * p.attackPower * d.coefficient, e.CreatePatternDamage(d.id, 0).Amount, "attack once");
        }
    }
}
