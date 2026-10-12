using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NecrocisEditor
{
    public static partial class IntestineEliteMapRunner
    {
        private static IEnumerator ProductionChecks()
        {
            Require(field.Plan != null, "production field configures automatically on real scene entry");
            string planJson = JsonUtility.ToJson(field.Plan);
            CheckPlan();
            foreach (var point in field.Plan.placements)
            {
                Move(biome.GridToWorldWithHeight(point.x, point.y) + Vector3.right * 4);
                var actor = Actor(point);
                Require(actor != null && actor.IsElite, "automatic map actor without kill trigger");
                CheckBalance(actor, 0);
            }
            Pass(difficulty + ": real map automatically places " + field.Plan.placements.Count + " elites of both types with zero kills; all stage-0 snapshots match source assets");

            var legacy = biome.GetComponent<EliteSpawner>();
            Require(legacy != null && legacy.enabled, "legacy spawner remains active");
            int before = (int)typeof(EliteSpawner).GetField("totalNormalKills", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(legacy);
            for (int i = 0; i < 10; i++) legacy.NotifyEnemyKilled("Macrophage");
            Equal(before + 10, (int)typeof(EliteSpawner).GetField("totalNormalKills", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(legacy), "legacy kill counter");
            Require(JsonUtility.ToJson(field.Plan) == planJson, "ten normal kills do not add or relocate biome elites");
            yield return new WaitForSeconds(3.5f);
            Require(UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Any(e => e.Balance != null && e.Balance.Current.MonsterId.StartsWith("legacy-elite.")), "legacy threshold still produces only its original elite");
            legacy.enabled = false;
            foreach (var actor in UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
                if (actor.Balance != null && actor.Balance.Current.MonsterId.StartsWith("legacy-elite.")) actor.ReleaseToPool();
            Pass("legacy ten-kill elite still spawns independently; production biome placement stays identical");

            if (directionalMap) yield return DirectionalFieldLifetimes(planJson);

            var alivePoint = field.Plan.placements[0];
            Move(biome.GridToWorldWithHeight(alivePoint.x, alivePoint.y) + Vector3.right * 4);
            var alive = Actor(alivePoint); var lifetime = alive.GetComponent<EnemyPatternLifetime>(); uint generation = alive.SpawnGeneration;
            Move(biome.GridToWorldWithHeight(alivePoint.x < 150 ? 280 : 15, alivePoint.y < 150 ? 280 : 15)); yield return null;
            Require(!lifetime.IsCurrent(generation) && !SaveService.IsBiomeEliteDefeated(alivePoint.spawnId), "unload releases live generation without recording a death");
            Move(biome.GridToWorldWithHeight(alivePoint.x, alivePoint.y) + Vector3.right * 4); yield return new WaitForSeconds(.3f);
            Require(Actor(alivePoint) != null && JsonUtility.ToJson(field.Plan) == planJson, "unbeaten point returns with stable plan");
            Pass("actual chunk round trip releases old generation and restores live field actor without changing saved placements");

            var gasPoint = field.Plan.placements.First(p => p.monsterId == gasDefinition.monsterId);
            Move(biome.GridToWorldWithHeight(gasPoint.x, gasPoint.y) + Vector3.right * 4);
            var frozen = Actor(gasPoint); float frozenHealth = frozen.Stats.MaxHealth;
            var profile = DifficultyBalanceService.GetProfile(difficulty);
            Set(gasDefinition, "statSets.Array.data[0].maxHealth", 42);
            Set(gasDefinition, "statSets.Array.data[0].attackPower", 4);
            Set(profile, "elites.maxHealth", 1.5f); Set(profile, "elites.outgoingDamage", 1.5f);
            int damageIndex = gasDefinition.patternDamage.FindIndex(p => p.id == GasSacPatternSettings.DamageId);
            Set(gasDefinition, $"patternDamage.Array.data[{damageIndex}].coefficient", 2);
            Equal(frozenHealth, frozen.Stats.MaxHealth, "existing map actor retains captured HP");
            frozen.ReleaseToPool(); field.Refresh(player.transform.position);
            var edited = Actor(gasPoint); Equal(63, edited.Stats.MaxHealth, "Inspector HP 42 x 1.5 on new map actor");
            health.ResetHealth(); float hp = health.CurrentHealth;
            player.TakeDamage(edited.CreatePatternDamage(GasSacPatternSettings.DamageId, 0));
            Equal(12, hp - health.CurrentHealth, "actual edited player HP damage: 4 x 1.5 x 2 exactly once");
            RestoreAssets(); edited.ReleaseToPool(); field.Refresh(player.transform.position); CheckBalance(Actor(gasPoint), 0);
            Pass("Serialized Inspector source edits reach newly spawned field actors: HP 63 and actual damage 12, old capture unchanged; original assets restored");

            var killed = new[] { gasPoint, field.Plan.placements.First(p => p.monsterId == residueDefinition.monsterId) };
            foreach (var point in killed)
            {
                Move(biome.GridToWorldWithHeight(point.x, point.y) + Vector3.right * 4);
                var actor = Actor(point); int expectedXp = actor.Balance.Current.Experience, gained = 0;
                EnemyFacing deathFacing = actor.PatternFacing;
                var directionArt = point.monsterId == gasDefinition.monsterId
                    ? ((GasSacPatternSettings)gasDefinition.pattern).presentation.directionalPresentation
                    : ((HardenedResiduePatternSettings)residueDefinition.pattern).presentation.directionalPresentation;
                Sprite[] deathSources = point.monsterId == gasDefinition.monsterId
                    ? ((GasSacPatternSettings)gasDefinition.pattern).presentation.deathFrames
                    : ((HardenedResiduePatternSettings)residueDefinition.pattern).presentation.deathFrames;
                var body = actor.transform.Find("Visual").GetComponent<SpriteRenderer>();
                var seen = new System.Collections.Generic.HashSet<Sprite>();
                Action<int> observe = amount => gained += amount; var levelCallback = LevelUpManager.OnLevelUp;
                LevelUpManager.OnLevelUp = null; LevelUpManager.OnExpGained += observe;
                try { actor.TakeDamage(10000); actor.TakeDamage(10000); actor.GrantExp(); }
                finally { LevelUpManager.OnExpGained -= observe; LevelUpManager.OnLevelUp = levelCallback; }
                Equal(expectedXp, gained, "one reward per real field death");
                Require(SaveService.IsBiomeEliteDefeated(point.spawnId) && actor.IsDeathAnimPlaying, "death saved while dedicated animation plays");
                float deathDeadline = Time.time + 2;
                while (actor.gameObject.activeSelf && Time.time < deathDeadline) { seen.Add(body.sprite); yield return null; }
                Require(!actor.gameObject.activeSelf, "dedicated death animation finishes before pool return");
                if (directionalMap) Require(deathSources.All(f => seen.Contains(directionArt.Capture().Resolve(f, deathFacing))), "all six selected-direction death frames shown on real field actor");
                field.Refresh(player.transform.position);
                Require(!field.Spawners.Any(s => s.Placement.spawnId == point.spawnId), "dead point removed after animation");
            }
            Pass("gas and residue each save one defeat and grant their source XP once; dedicated death animations finish before removal");

            string runId = SaveService.ActiveRunId;
            var survivorPoint = field.Plan.placements.First(p => !killed.Contains(p));
            foreach (var relic in new[] { BiomeType.Liver, BiomeType.Lung, BiomeType.Stomach, BiomeType.Intestine })
            {
                int oldStage = biome.MonsterVisit.Stage;
                Move(biome.GridToWorldWithHeight(survivorPoint.x, survivorPoint.y) + Vector3.right * 4);
                GameManager.Instance.CollectRelic(relic);
                Require(biome.MonsterVisit.Stage == oldStage, "progression stays frozen during current visit");
                Actor(survivorPoint).ReleaseToPool(); field.Refresh(player.transform.position); CheckBalance(Actor(survivorPoint), oldStage);
                GameManager.Instance.ReturnToHub(); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_HUB);
                GameManager.Instance.EnterBiome(BiomeType.Intestine); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_INTESTINE); yield return null;
                BindScene();
                Require(JsonUtility.ToJson(field.Plan) == planJson && killed.All(p => SaveService.IsBiomeEliteDefeated(p.spawnId)), "reentry preserves both deaths and original plan");
                Move(biome.GridToWorldWithHeight(survivorPoint.x, survivorPoint.y) + Vector3.right * 4);
                CheckBalance(Actor(survivorPoint), oldStage + 1);
                // A species may have had only one map placement and now be dead. Check both definitions
                // against the real scene's new visit, without resurrecting any recorded field point.
                foreach (var rule in source.monsters)
                {
                    var visitProbe = EnemyController.Acquire(null, "IMap_VisitBalance", EnemyController.GetPoolArchetypeId(rule));
                    try
                    {
                        visitProbe.Configure(null, rule, player.transform.position, player.transform.position);
                        visitProbe.SuppressExperienceReward = true; CheckBalance(visitProbe, oldStage + 1);
                    }
                    finally { visitProbe.ReleaseToPool(); }
                }
                Pass(difficulty + " real Hub return/reentry: stage " + (oldStage + 1) + " applies once to both definitions, attack tells/ranges unchanged, saved placements and two defeats retained");
            }

            string visitId = biome.MonsterVisit.VisitId;
            Require(SaveService.TrySaveActiveRun(out string error), error);
            SaveService.UseStorageRootForTests(storage);
            Require(SaveService.TryContinue(difficulty, out error), error);
            Require(SaveService.TryRestorePendingSession(out BiomeType resume, out error) && resume == BiomeType.Intestine, error);
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_INTESTINE); yield return null; BindScene();
            Require(SaveService.ActiveRunId == runId && biome.MonsterVisit.VisitId == visitId && biome.MonsterVisit.Stage == 4, "disk continue restores exact run and stage-4 visit");
            Require(JsonUtility.ToJson(field.Plan) == planJson && killed.All(p => SaveService.IsBiomeEliteDefeated(p.spawnId)), "disk reload retains plan and deaths");
            foreach (var point in killed)
            {
                Move(biome.GridToWorldWithHeight(point.x, point.y));
                Require(!field.Spawners.Any(s => s.Placement.spawnId == point.spawnId), "continued dead point cannot spawn");
            }
            Pass("actual save/continue + scene reload restores exact visit/stage/plan and keeps both defeated species absent");

            GameManager.Instance.ReturnToHub(); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_HUB);
            Require(SaveService.TryBeginNewGame(difficulty, out error), error);
            GameManager.Instance.EnterBiome(BiomeType.Intestine); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_INTESTINE); yield return null; BindScene();
            Require(SaveService.ActiveRunId != runId && biome.MonsterVisit.Stage == 0 && killed.All(p => !SaveService.IsBiomeEliteDefeated(p.spawnId)), "new run has no previous defeats/progression");
            CheckPlan();
            Pass(difficulty + " new run resets stage/deaths and automatically creates its own sparse production plan");
        }

        private static EnemyController Actor(BiomeElitePlacement point) => field.Spawners.Single(s => s.Placement.spawnId == point.spawnId).ActiveEnemy;

        private static void CheckPlan()
        {
            var plan = field.Plan;
            Require(plan.placements.Count >= source.minimumCount && plan.placements.Count <= source.maximumCount, "whole-map count");
            foreach (var rule in source.monsters) Require(plan.placements.Count(p => p.monsterId == rule.monsterDefinition.monsterId) >= source.minimumPerType, "minimum per species");
            Vector2Int entrance = biome.WorldToGrid(biome.GetPlayerSpawnPosition());
            var arena = biome.GetBiomeConfig().GetMidBossArenaConfig(); var portal = biome.GetBiomeConfig().GetReturnPortalConfig();
            Vector2Int arenaCenter = arena.useCustomCenter ? arena.centerGrid : new Vector2Int(biome.MapWidth / 2, biome.MapHeight / 2);
            float padding = source.bossExclusionPadding + arena.wallThicknessInCells + (arena.presentation != null && arena.presentation.enabled ? arena.presentation.approachLengthInCells : 0);
            foreach (var p in plan.placements)
            {
                var cell = new Vector2Int(p.x, p.y);
                Require(biome.IsWalkable(p.x, p.y) && Vector2Int.Distance(cell, entrance) >= source.entranceExclusionRadius, "walkable and outside entrance exclusion");
                if (portal != null && portal.enabled) Require(Vector2Int.Distance(cell, portal.useCustomPosition ? portal.gridPosition : entrance) > source.portalExclusionRadius, "portal exclusion");
                if (arena.enabled) Require(Mathf.Abs(p.x - arenaCenter.x) > arena.arenaSize.x / 2f + padding || Mathf.Abs(p.y - arenaCenter.y) > arena.arenaSize.y / 2f + padding, "boss arena and approach exclusion");
                foreach (var q in plan.placements) if (p != q) Require(Vector2Int.Distance(cell, new Vector2Int(q.x, q.y)) >= source.minimumSpacing, "cross-type spacing");
            }
            foreach (string other in new[] { "Stomach" })
                Require(AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>("Assets/_Project/Data/BiomeElites/" + other + "BiomeEliteSpawnConfig.asset").monsters.Count == 2, "completed Stomach registration retained");
        }

        private static void CheckBalance(EnemyController actor, int stage)
        {
            var definition = actor.Balance.Current.MonsterId == gasDefinition.monsterId ? gasDefinition : residueDefinition;
            var catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath);
            var multipliers = catalog.difficultyCatalog.Get(difficulty).elites;
            Require(catalog.progression.TryGetStage(stage, out var progression), "valid progression stage");
            var stats = definition.statSets.Single(s => s.id == "Default"); var snapshot = actor.Balance.Current;
            if (directionalMap) Require(actor.HasPatternDirections, "real field/visit actor binds approved directional presentation");
            Require(snapshot.Difficulty == difficulty && snapshot.Stage == stage, "correct visit difficulty and stage");
            Equal(Mathf.Max(1, CharacterStats.ToHealthUnits(stats.maxHealth * multipliers.maxHealth * progression.maxHealth)), actor.Stats.MaxHealth, "actual HP single multiplication");
            Equal(stats.moveSpeed * multipliers.moveSpeed, actor.Stats.MoveSpeed, "movement has no progression multiplier");
            Equal((int)Math.Round((double)definition.reward.baseExperience * multipliers.experienceReward * progression.experience, MidpointRounding.ToEven), snapshot.Experience, "reward multiplier once");
            foreach (var attack in definition.patternDamage)
            {
                float expected = stats.attackPower * multipliers.outgoingDamage * progression.attackPower * attack.coefficient;
                Equal(expected, actor.CreatePatternDamage(attack.id, 0).Amount, "pattern damage once");
                health.ResetHealth(); float hp = health.CurrentHealth;
                player.TakeDamage(actor.CreatePatternDamage(attack.id, 0));
                Equal(CharacterStats.ToHealthUnits(expected), hp - health.CurrentHealth, "real health applies final pattern damage once");
            }
            if (definition == gasDefinition)
            {
                var pattern = actor.GetComponent<GasSacElitePattern>(); var settings = (GasSacPatternSettings)definition.pattern;
                Equal(settings.windupSeconds, pattern.WindupDuration, "gas tell unchanged"); Equal(settings.burstRadius, pattern.BurstRadius, "gas radius unchanged");
                Equal(settings.orbWindupSeconds, pattern.OrbWindupDuration, "orb tell unchanged"); Require(pattern.OrbEnabled == settings.orbEnabled, "B selection setting unchanged");
                Equal(settings.rearmSeconds * multipliers.attackCooldown, actor.GetRearmCooldown(settings.rearmSeconds), "gas cooldown multiplier once");
            }
            else
            {
                var pattern = actor.GetComponent<HardenedResidueElitePattern>(); var settings = (HardenedResiduePatternSettings)definition.pattern;
                Equal(settings.windupSeconds, pattern.WindupDuration, "residue tell unchanged"); Require(settings.footprint == pattern.Footprint, "residue footprint unchanged");
                Equal(settings.rearmSeconds * multipliers.attackCooldown, actor.GetRearmCooldown(settings.rearmSeconds), "residue cooldown multiplier once");
            }
        }
    }
}
