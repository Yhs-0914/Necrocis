using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static class BiomeEliteSmokeRunner
    {
        private static readonly List<Object> temporary = new List<Object>();
        private static readonly List<string> report = new List<string>();

        [MenuItem("Necrocis/Balance/Run C-03 Placement and Save Checks")]
        public static void Run()
        {
            string storage = Path.Combine(Path.GetTempPath(), "necrocis-biome-elite-" + Guid.NewGuid().ToString("N"));
            report.Clear();
            bool passed = false;
            try
            {
                var config = CreateProbeConfig();
                CheckPlacement(config);
                SaveService.UseStorageRootForTests(storage);
                CheckSaveAndVisit(config, storage);
                CheckOldSchema(storage);
                passed = true;
            }
            catch (Exception error) { report.Add("FAIL " + error); Debug.LogException(error); }
            finally
            {
                SaveService.ResetStaticStateForTests();
                foreach (Object value in temporary) if (value != null) Object.DestroyImmediate(value);
                temporary.Clear();
                if (Directory.Exists(storage)) Directory.Delete(storage, true);
                Directory.CreateDirectory("Logs"); File.WriteAllLines("Logs/BiomeElite-C03-data-results.txt", report);
            }
            Debug.Log("[BiomeElite-C03-Data] " + (passed ? "PASS" : "FAIL"));
            if (Application.isBatchMode) EditorApplication.Exit(passed ? 0 : 1);
        }

        public static BiomeEliteSpawnConfig CreateProbeConfig()
        {
            var config = ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>();
            config.minimumCount = config.maximumCount = 6;
            config.minimumSpacing = 12; config.entranceExclusionRadius = 8;
            foreach (string id in new[] { "verification.biome-elite-a", "verification.biome-elite-b" })
            {
                var definition = ScriptableObject.CreateInstance<MonsterDefinition>();
                definition.monsterId = id; definition.displayName = "C-03 배치 검증용 " + id;
                definition.statSets[0].maxHealth = 12; definition.statSets[0].attackPower = 1; definition.statSets[0].moveSpeed = 0;
                definition.reward.baseExperience = 3; definition.contact.enabled = false;
                config.monsters.Add(new EnemySpawnRuleConfig
                {
                    name = id, monsterDefinition = definition, isElite = true,
                    chaseRadius = 0, wanderRadius = 0, addCollider = true, isTrigger = true,
                    scale = Vector3.one, colliderSize = Vector3.one, colliderCenter = Vector3.up * .5f
                });
            }
            return config;
        }

        private static BiomeElitePlan Build(BiomeEliteSpawnConfig config, int seed) => BiomeElitePlacementService.Build(
            config, BiomeType.Intestine, seed, 90, 90, new Vector2Int(5, 5), p => p.x != 60,
            (a, b) => true, p => p.x >= 30 && p.x <= 45 && p.y >= 30 && p.y <= 45);

        private static void CheckPlacement(BiomeEliteSpawnConfig config)
        {
            temporary.Add(config); temporary.AddRange(config.monsters.Select(r => (Object)r.monsterDefinition));
            var a = Build(config, 521);
            Require(a.placements.Count == 6, "six placements");
            Require(a.placements.Select(p => p.monsterId).Distinct().Count() == 2, "minimum allocation of both types");
            foreach (var p in a.placements)
            {
                Require(p.x < 60, "reachable side only");
                Require(!(p.x >= 29 && p.x <= 46 && p.y >= 29 && p.y <= 46), "boss exclusion and clearance");
                Require(Vector2.Distance(new Vector2(p.x, p.y), new Vector2(5, 5)) >= 8, "entrance exclusion");
                foreach (var q in a.placements)
                    if (p != q) Require(Vector2.Distance(new Vector2(p.x, p.y), new Vector2(q.x, q.y)) >= 12, "cross-type spacing");
            }
            config.monsters.Reverse();
            Require(JsonUtility.ToJson(a) == JsonUtility.ToJson(Build(config, 521)), "deterministic placement independent of source list order");
            config.minimumSpacing = 1000;
            Require(Build(config, 521).placements.Count == 1, "candidate shortage must not relax spacing");
            config.minimumSpacing = 12;
            config.minimumCount = config.maximumCount = config.minimumPerType = 0;
            Require(Build(config, 521).placements.Count == 0, "zero requested count");
            config.minimumCount = config.maximumCount = 6; config.minimumPerType = 1;
            Pass("deterministic whole-map count, both types, reachability, entrance/boss exclusion, shared spacing, shortage and zero count");
        }

        private static void CheckSaveAndVisit(BiomeEliteSpawnConfig config, string storage)
        {
            Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error), error);
            string run = SaveService.ActiveRunId;
            var first = SaveService.EnterLoadedBiome(BiomeType.Intestine, 101);
            Require(first.Stage == 0, "new run stage zero");
            int seed = SaveService.GetOrCreateBiomeSeed(BiomeType.Intestine);
            BiomeElitePlan plan = Build(config, seed);
            Require(SaveService.TryStoreBiomeElitePlan(plan, out error), error);
            string planJson = JsonUtility.ToJson(plan);
            Require(SaveService.TryRecordBiomeEliteDefeat(run, plan.placements[0].spawnId, out bool fresh, out error) && fresh, error);
            Require(SaveService.TryRecordBiomeEliteDefeat(run, plan.placements[0].spawnId, out fresh, out error) && !fresh, "duplicate defeat");
            SaveService.MarkBossDefeated(BiomeType.Liver);
            SaveService.MarkBossDefeated(BiomeType.Liver);
            Require(SaveService.RunMonsterStage == 1, "distinct run bosses only");
            Require(SaveService.EnterLoadedBiome(BiomeType.Intestine, 101).Stage == 0, "same visit/chunk frozen after boss defeat");
            // Write only the temporary test checkpoint, then genuinely reload the file through SaveService.
            string path = Path.Combine(storage, "Saves", "normal.json");
            RunSaveData saved = JsonUtility.FromJson<RunSaveData>(File.ReadAllText(path));
            saved.checkpoint.biome = BiomeType.Intestine; saved.checkpoint.sceneName = SceneLoader.SCENE_INTESTINE;
            File.WriteAllText(path, JsonUtility.ToJson(saved));
            SaveService.UseStorageRootForTests(storage);
            Require(SaveService.TryContinue(GameDifficulty.Normal, out error), error);
            var restored = SaveService.EnterLoadedBiome(BiomeType.Intestine, 102);
            Require(restored.VisitId == first.VisitId && restored.Stage == 0, "continue restores visit ID and entry stage");
            Require(JsonUtility.ToJson(SaveService.GetBiomeElitePlan(BiomeType.Intestine)) == planJson, "saved plan preserved");
            Require(SaveService.IsBiomeEliteDefeated(plan.placements[0].spawnId), "saved defeat restored");
            config.maximumCount = 20;
            Require(JsonUtility.ToJson(SaveService.GetBiomeElitePlan(BiomeType.Intestine)) == planJson, "settings changes cannot rewrite current run placement");
            var reentry = SaveService.EnterLoadedBiome(BiomeType.Intestine, 103);
            Require(reentry.VisitId != first.VisitId && reentry.Stage == 1, "actual reentry creates next-stage visit");
            Require(SaveService.TryPrepareNormalDeathRespawn(out error), error);
            Require(SaveService.IsBiomeEliteDefeated(plan.placements[0].spawnId), "Normal death retains kills");
            SaveService.MarkFinalBossDefeated();
            Require(SaveService.TryBeginNewGame(GameDifficulty.Hard, out error), error);
            Require(SaveService.RunMonsterStage == 0 && SaveService.GetBiomeElitePlan(BiomeType.Intestine) == null, "new Hard run isolation");
            Require(!SaveService.TryRecordBiomeEliteDefeat(run, plan.placements[0].spawnId, out fresh, out error), "old run callback rejected");
            Require(SaveService.TryHandleHardDeath(out error), error);
            Require(SaveService.TryBeginNewGame(GameDifficulty.Hard, out error), error);
            Require(!SaveService.IsBiomeEliteDefeated(plan.placements[0].spawnId), "Hard death and restart clear kills");
            Pass("placement/kill disk persistence, duplicate guard, visit freeze/continue/reentry, Normal retention, Hard isolation and old-run callback rejection");
        }

        private static void CheckOldSchema(string storage)
        {
            string path = Path.Combine(storage, "Saves", "normal.json");
            File.WriteAllText(path, "{\"schemaVersion\":1,\"saveId\":\"legacy-c03\",\"difficulty\":0,\"isActive\":true,\"campaignStarted\":true,\"player\":{\"level\":1},\"bosses\":{\"liverDefeated\":true}}");
            SaveService.UseStorageRootForTests(storage);
            Require(SaveService.TryContinue(GameDifficulty.Normal, out string error), error);
            Require(SaveService.GetBiomeElitePlan(BiomeType.Intestine) == null, "v1 missing collection normalization");
            Require(SaveService.EnterLoadedBiome(BiomeType.Intestine, 201).Stage == 1, "v1 first context derives current run only");
            Require(!SaveService.IsBiomeEliteDefeated("invented"), "v1 creates no invented defeats");
            Pass("schema v1 without new collections loads safely and does not invent kill IDs");
        }

        private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        private static void Pass(string message) { report.Add("PASS " + message); Debug.Log("[BiomeElite-C03-Data] PASS " + message); }
    }
}
