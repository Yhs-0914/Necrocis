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
    public static partial class LungEliteMapRunner
    {
        private static EnemyController Actor(BiomeElitePlacement point) => field.Spawners.Single(s => s.Placement.spawnId == point.spawnId).ActiveEnemy;
        private static Vector3 Home(BiomeElitePlacement point) => biome.GridToWorldWithHeight(point.x, point.y);

        private static IEnumerator ProductionChecks()
        {
            Require(field.Plan != null, "production field is automatic on real Lung entry"); CheckPlan();
            if (preview)
            {
                ExportMapOverview();
                var point = field.Plan.placements.Single(p => p.monsterId == previewPhase);
                Move(Home(point) + Vector3.right * 3.6f); var actor = Actor(point);
                if (point.monsterId == pollenDefinition.monsterId)
                {
                    var pattern = actor.GetComponent<PollenInvaderElitePattern>();
                    yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 3, "real map pollen tell");
                    Move(Home(point) + new Vector3(-3, 0, 2));
                    yield return Wait(() => pattern.CycleVolleyCount == 2, 3, "real map B second");
                    yield return new WaitForSeconds(.3f);
                }
                else
                {
                    var pattern = actor.GetComponent<DustClumpElitePattern>();
                    yield return Wait(() => pattern.ActiveCloud != null, 3, "real map cloud");
                    Move(Home(point) + new Vector3(-2, 0, 3)); yield return new WaitForSeconds(.4f);
                }
                Selection.activeGameObject = actor.gameObject; yield break;
            }
            yield return AccessibilityChecks();
            string planJson = JsonUtility.ToJson(field.Plan), runId = SaveService.ActiveRunId;
            var points = field.Plan.placements.ToArray();
            foreach (var p in points) { Move(Home(p) + Vector3.right * 4); CheckBalance(Actor(p), 0); }
            Pass(difficulty + " MAP: automatic one P-01 + one B-enabled P-03, zero kills, >=18m spacing, 5-cell flat clearance, entrance/portal/boss exclusions");

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
            Pass(difficulty + (hasLegacy ? " SPAWN: legacy ten-kill spawn remains independent; biome plan stays fixed" : " SPAWN: existing Lung normal/legacy list is empty; ten kill notifications cannot spawn or change biome elites"));

            foreach (var p in points)
            {
                Move(Home(p) + Vector3.right * 3); Actor(p).ReleaseToPool(); field.Refresh(player.transform.position);
                var e = Actor(p); e.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0);
                if (e.Balance.Current.MonsterId == pollenDefinition.monsterId)
                {
                    yield return Wait(() => e.GetComponent<PollenInvaderElitePattern>().CycleVolleyCount == 2, 4, "field B before unload");
                }
                else
                {
                    yield return Wait(() => e.GetComponent<DustClumpElitePattern>().ActiveCloud != null, 3, "field cloud before unload");
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

            var tunedPoint = points.First(p => p.monsterId == dustDefinition.monsterId);
            Move(Home(tunedPoint) + Vector3.right * 4); var oldActor = Actor(tunedPoint); float oldHp = oldActor.Stats.MaxHealth;
            Set(dustDefinition, "statSets.Array.data[0].maxHealth", 42); Set(dustDefinition, "statSets.Array.data[0].attackPower", 4);
            Set(DifficultyBalanceService.GetProfile(difficulty), "elites.maxHealth", 1.5f); Set(DifficultyBalanceService.GetProfile(difficulty), "elites.outgoingDamage", 1.5f);
            int coefficient = dustDefinition.patternDamage.FindIndex(p => p.id == DustClumpPatternSettings.DamageId);
            Set(dustDefinition, $"patternDamage.Array.data[{coefficient}].coefficient", 2);
            Equal(oldHp, oldActor.Stats.MaxHealth, "old field actor keeps snapshot"); oldActor.ReleaseToPool(); field.Refresh(player.transform.position);
            Equal(63, Actor(tunedPoint).Stats.MaxHealth, "new field actor captures Inspector HP");
            yield return AttackAtPoint(tunedPoint, 12); RestoreAssets(); Actor(tunedPoint).ReleaseToPool(); field.Refresh(player.transform.position); CheckBalance(Actor(tunedPoint), 0);
            Pass(difficulty + " INSPECTOR: new production actor HP63 and real cloud12; existing snapshot stable, all sources restored");

            for (int stage = 0; stage <= 4; stage++)
            {
                Require(biome.MonsterVisit.Difficulty == difficulty && biome.MonsterVisit.Stage == stage, "real visit difficulty/stage");
                foreach (var p in points)
                {
                    Move(Home(p) + Vector3.right * 4); Actor(p).ReleaseToPool(); field.Refresh(player.transform.position);
                    var e = Actor(p); CheckBalance(e, stage);
                    string id = p.monsterId == pollenDefinition.monsterId ? PollenInvaderPatternSettings.FollowupDamageId : DustClumpPatternSettings.DamageId;
                    float expected = CharacterStats.ToHealthUnits(e.CreatePatternDamage(id, 0).Amount);
                    yield return AttackAtPoint(p, expected);
                    Pass($"{difficulty} VISIT stage{stage}/{p.monsterId}: actual field HP/speed/XP snapshot, live attack damage={expected}, original tell/radius/facing");
                }
                if (stage == 4) break;
                GameManager.Instance.CollectRelic(new[] { BiomeType.Intestine, BiomeType.Liver, BiomeType.Stomach, BiomeType.Lung }[stage]);
                Require(biome.MonsterVisit.Stage == stage, "stage freezes during visit");
                Move(Home(points[0]) + Vector3.right * 4); Actor(points[0]).ReleaseToPool(); field.Refresh(player.transform.position); CheckBalance(Actor(points[0]), stage);
                GameManager.Instance.ReturnToHub(); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_HUB);
                GameManager.Instance.EnterBiome(BiomeType.Lung); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LUNG); yield return null; BindScene();
                if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
                Require(JsonUtility.ToJson(field.Plan) == planJson, "reentry preserves exact map plan");
            }
            foreach (var p in points) yield return KillPoint(p);
            string visitId = biome.MonsterVisit.VisitId;
            Require(SaveService.TrySaveActiveRun(out string error), error); SaveService.UseStorageRootForTests(storage);
            Require(SaveService.TryContinue(difficulty, out error), error);
            Require(SaveService.TryRestorePendingSession(out BiomeType resume, out error) && resume == BiomeType.Lung, error);
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LUNG); yield return null; BindScene();
            Require(SaveService.ActiveRunId == runId && biome.MonsterVisit.VisitId == visitId && biome.MonsterVisit.Stage == 4, "disk reload exact visit");
            Require(JsonUtility.ToJson(field.Plan) == planJson && points.All(p => SaveService.IsBiomeEliteDefeated(p.spawnId)), "disk retains both deaths and placements");
            foreach (var p in points) { Move(Home(p)); Require(!field.Spawners.Any(s => s.Placement.spawnId == p.spawnId), "dead point remains absent"); }
            Pass(difficulty + " SAVE: actual disk continue restores same run/visit/stage4/plan; both species stay dead after reload");

            Set(source, "minimumCount", 4); Set(source, "maximumCount", 4); Set(source, "minimumSpacing", 24);
            Require(JsonUtility.ToJson(field.Plan) == planJson, "Inspector density cannot rewrite current saved plan");
            Require(SaveService.TryBeginNewGame(difficulty, out error), error); Require(SaveService.TryRestorePendingSession(out _, out error), error);
            GameManager.Instance.EnterBiome(BiomeType.Lung); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LUNG); yield return null; BindScene(); CheckPlan();
            Require(field.Plan.placements.Count == 4 && biome.MonsterVisit.Stage == 0 && SaveService.ActiveRunId != runId, "new run uses edited count and resets progress");
            Pass(difficulty + " DENSITY: Inspector count2->4 and spacing18->24 apply only to new run; old saved plan untouched");
            RestoreAssets();
            Require(SaveService.TryBeginNewGame(difficulty, out error), error); Require(SaveService.TryRestorePendingSession(out _, out error), error);
            GameManager.Instance.EnterBiome(BiomeType.Lung); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LUNG); yield return null; BindScene(); CheckPlan();
            Require(field.Plan.placements.Count == 2 && points.All(p => !SaveService.IsBiomeEliteDefeated(p.spawnId)), "baseline new run resets defeated IDs");
            foreach (var p in field.Plan.placements.ToArray()) yield return KillPoint(p);
            Pass(difficulty + " NEW RUN: restored one-of-each source, stage0, fresh defeat state and XP50 once per actual field kill");
        }

        private static IEnumerator AttackAtPoint(BiomeElitePlacement point, float expected)
        {
            var e = Actor(point); e.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0);
            var home = e.GetComponent<Rigidbody>().position; Move(home + Vector3.right * 3.6f);
            if (point.monsterId == pollenDefinition.monsterId)
            {
                var p = e.GetComponent<PollenInvaderElitePattern>(); Require(p.FollowupEnabled, "production uses approved B");
                yield return Wait(() => p.CycleVolleyCount == 1, 4, "production first volley"); Move(home + Vector3.left * 3);
                yield return Wait(() => p.CycleVolleyCount == 2, 2, "production second volley"); yield return new WaitForSeconds(.5f);
                var pellet = p.SecondWavePellets[1]; Require(pellet != null, "field second remains in flight");
                Equal(.85f, p.WindupDuration, "pollen first tell"); Equal(.8f, p.RotationDuration, "pollen second tell");
                Equal(.16f, pellet.Radius, "pollen radius"); Equal(biome.GetGroundHeight(pellet.transform.position) + .16f, pellet.Body.transform.position.y, "pollen low flight");
                Require(pellet.GetComponentsInChildren<SpriteRenderer>().Length == 1 && e.HasPatternDirections, "no pellet floor UI, directional body");
                Move(pellet.transform.position + pellet.Direction * 1.6f); health.ResetHealth(); float hp = health.CurrentHealth;
                yield return Wait(() => p.SecondVolley.WaveDamageApplications == 1, 1.5f, "actual production second hit");
                Equal(expected, hp - health.CurrentHealth, "production B HP delta"); Require(p.FirstVolley.WaveDamageApplications == 0 && p.CycleHitBudget.DamageApplications == 1, "second-only shared budget");
            }
            else
            {
                var p = e.GetComponent<DustClumpElitePattern>(); yield return Wait(() => p.ActiveCloud != null, 4, "production cloud release");
                var cloud = p.ActiveCloud; Equal(.9f, p.WindupDuration, "dust tell"); Equal(1.1f, p.CloudRadius, "dust full red radius");
                ElitePresentationChecks.RedArea(p.TelegraphObject, Vector2.one * 2.2f);
                Require(!e.HasPatternDirections && !Body(e).flipX && p.CoreExposed, "directionless vulnerable core");
                Move(cloud.LaunchPosition + cloud.Direction * 1.8f); health.ResetHealth(); float hp = health.CurrentHealth;
                yield return Wait(() => cloud.HitAttempts == 1, 2, "actual production cloud hit"); Equal(expected, hp - health.CurrentHealth, "production cloud HP delta");
                Equal(biome.GetGroundHeight(cloud.transform.position), cloud.Body.transform.position.y, "grounded cloud");
            }
            Move(home + Vector3.left * 3);
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
            var art = ((PollenInvaderPatternSettings)pollenDefinition.pattern).presentation;
            var wanted = point.monsterId == pollenDefinition.monsterId
                ? e.Config.deathSprites.Select(s => art.directionalPresentation.Capture().Resolve(s, facing)).ToArray()
                : (Sprite[])e.Config.deathSprites.Clone();
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
            Require(((PollenInvaderPatternSettings)pollenDefinition.pattern).followup.enabled, "approved B stays enabled");
            foreach (string completed in new[] { "Intestine", "Liver" }) Require(AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>("Assets/_Project/Data/BiomeElites/" + completed + "BiomeEliteSpawnConfig.asset").monsters.Count == 2, "completed biome registration retained");
            foreach (string other in new[] { "Stomach" }) Require(AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>("Assets/_Project/Data/BiomeElites/" + other + "BiomeEliteSpawnConfig.asset").monsters.Count == 2, "completed Stomach registration retained");
        }

        private static void CheckBalance(EnemyController e, int stage)
        {
            var definition = e.Balance.Current.MonsterId == pollenDefinition.monsterId ? pollenDefinition : dustDefinition;
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
