using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static class BiomeElitePlayModeRunner
    {
        private static string storage;
        private static bool oldOptionsEnabled, started, passed, preview, oldBackground;
        private static EnterPlayModeOptions oldOptions;
        private static float oldTimeScale;
        private static int enteredFrame;
        private static double deadline;
        private static PlayerController player;
        private static BiomeEliteSpawnConfig config;
        private static BiomeEliteSpawnConfig productionField;
        private static bool productionWasEnabled;
        private static readonly List<Object> temporary = new List<Object>();
        private static readonly List<string> report = new List<string>();

        [MenuItem("Necrocis/Balance/Run C-03 Field Play Mode Checks")]
        public static void Run() => Start(false);

        [MenuItem("Necrocis/Balance/Preview C-03 Biome Elite Placement")]
        public static void Preview() => Start(true);

        private static void Start(bool previewOnly)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            preview = previewOnly; started = passed = false; report.Clear(); temporary.Clear();
            productionField = AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>(IntestineEliteMapSetup.ConfigPath);
            productionWasEnabled = productionField.enabled; productionField.enabled = false;
            storage = Path.Combine(Path.GetTempPath(), "necrocis-c03-play-" + Guid.NewGuid().ToString("N"));
            SaveService.UseStorageRootForTests(storage);
            Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error), error);
            oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            oldOptions = EditorSettings.enterPlayModeOptions;
            oldTimeScale = Time.timeScale;
            oldBackground = Application.runInBackground;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = oldOptions | EnterPlayModeOptions.DisableDomainReload;
            EditorApplication.playModeStateChanged += OnPlayMode;
            EditorApplication.update += Tick;
            deadline = EditorApplication.timeSinceStartup + 180;
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Hub.unity");
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode) enteredFrame = Time.frameCount;
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayMode;
            EditorSettings.enterPlayModeOptionsEnabled = oldOptionsEnabled;
            productionField.enabled = productionWasEnabled;
            EditorSettings.enterPlayModeOptions = oldOptions;
            Time.timeScale = oldTimeScale;
            Application.runInBackground = oldBackground;
            SaveService.ResetStaticStateForTests();
            foreach (Object item in temporary) if (item != null) Object.DestroyImmediate(item);
            temporary.Clear(); config = null;
            if (Directory.Exists(storage)) Directory.Delete(storage, true);
            if (!preview)
            {
                Directory.CreateDirectory("Logs"); File.WriteAllLines("Logs/BiomeElite-C03-play-results.txt", report);
                Debug.Log("[BiomeElite-C03-Play] " + (passed ? "ALL PASS" : "FAIL"));
            }
            if (Application.isBatchMode) EditorApplication.Exit(passed ? 0 : 1);
        }

        private static void Tick()
        {
            if (EditorApplication.timeSinceStartup > deadline)
            { Fail(new TimeoutException("C-03 test timed out")); deadline = double.PositiveInfinity; return; }
            if (started || !EditorApplication.isPlaying || Time.frameCount - enteredFrame < 15) return;
            player = PlayerController.Instance;
            if (player == null || player.HealthComponent == null) return;
            started = true; Time.timeScale = 1;
            if (!preview) Application.runInBackground = true;
            player.StartCoroutine(Guard(Checks()));
        }

        private static IEnumerator Guard(IEnumerator checks)
        {
            while (true)
            {
                object next;
                try { if (!checks.MoveNext()) break; next = checks.Current; }
                catch (Exception error) { Fail(error); yield break; }
                yield return next;
            }
            passed = true;
            if (preview)
            {
                deadline = double.PositiveInfinity;
                player.HealthComponent.GrantTemporaryInvincibility(3600);
                EditorApplication.isPaused = true;
                Debug.Log("[BiomeElite-C03] Preview ready: cyan/magenta placement probes, not the eight species. Stop Play to restore the isolated test session.");
            }
            else EditorApplication.isPlaying = false;
        }

        private static void Fail(Exception error)
        {
            passed = false; report.Add("FAIL " + error); Debug.LogException(error);
            EditorApplication.isPlaying = false;
        }

        private static IEnumerator Checks()
        {
            config = BiomeEliteSmokeRunner.CreateProbeConfig(); temporary.Add(config);
            foreach (EnemySpawnRuleConfig rule in config.monsters)
            {
                temporary.Add(rule.monsterDefinition);
                var texture = new Texture2D(8, 8) { filterMode = FilterMode.Point };
                Color fill = rule == config.monsters[0] ? Color.cyan : Color.magenta;
                for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) texture.SetPixel(x, y, x == 0 || x == 7 || y == 0 || y == 7 ? Color.black : fill);
                texture.Apply();
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(.5f, 0), 8);
                temporary.Add(texture); temporary.Add(sprite);
                rule.idleSprites = new[] { sprite };
                rule.scale = Vector3.one * 2;
            }
            GameManager.Instance.EnterBiome(BiomeType.Intestine);
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_INTESTINE);
            yield return null;
            var biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>();
            Require(biome != null, "real biome bridge");
            BiomeEliteField field = biome.GetComponent<BiomeEliteField>();
            Require(field != null && field.Plan == null, "probe-only test isolates the production field");
            field.Configure(biome, config, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Require(field.Plan != null && field.Plan.placements.Count == 6, "six actual map placement probes");
            string planJson = JsonUtility.ToJson(field.Plan);
            BiomeElitePlacement point = field.Plan.placements[0];
            MoveTo(biome, point.x, point.y);
            field.Refresh(player.transform.position);
            BiomeEliteFieldSpawner spawner = field.Spawners.First(s => s.Placement.spawnId == point.spawnId);
            EnemyController first = spawner.ActiveEnemy;
            Require(first != null && first.Balance.Current.Stage == 0 && first.IsElite, "map activation without a kill trigger");
            Require(first.Stats.MaxHealth == 12, "probe original health applied");
            Selection.activeGameObject = field.gameObject;
            if (preview) yield break;
            Pass("actual Intestine map: 6 saved placements, 2 probe types, first activation with zero kills");

            var lifetime = first.GetComponent<EnemyPatternLifetime>();
            uint oldGeneration = first.SpawnGeneration;
            var owned = new GameObject("C03OwnedPatternProbe");
            lifetime.Own(owned);
            int callbacks = 0;
            Require(lifetime.TryStartCycle(Delayed(() => callbacks++)), "first attack cycle owns execution");
            Require(!lifetime.TryStartCycle(Delayed(() => callbacks++)), "overlapping attack cycle rejected");
            MoveTo(biome, point.x < 150 ? 280 : 15, point.y < 150 ? 280 : 15);
            field.Refresh(player.transform.position);
            yield return null;
            // Another loaded point may acquire this same pooled GameObject in the same Refresh call.
            // The released spawn generation and its owned objects must be gone; the reusable object may be active again.
            Require(!lifetime.IsCurrent(oldGeneration) && owned == null,
                $"chunk unload ends spawn and clears owned objects (active={first.gameObject.activeSelf}, old={oldGeneration}, current={first.SpawnGeneration}, owned={owned != null})");
            Require(!SaveService.IsBiomeEliteDefeated(point.spawnId), "unload is not death");
            MoveTo(biome, point.x, point.y);
            field.Refresh(player.transform.position);
            spawner = field.Spawners.First(s => s.Placement.spawnId == point.spawnId);
            Require(spawner.ActiveEnemy != null && spawner.ActiveEnemy.Stats.MaxHealth == 12, "unbeaten point respawns at visit health");
            Require(!lifetime.TryExecute(oldGeneration, () => callbacks++), "stale callback cannot affect reused actor");
            yield return new WaitForSeconds(.7f);
            Require(callbacks == 0, "cancelled delayed work never executes");
            var chunks = (Chunk[,])typeof(BiomeManager).GetField("chunks", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(biome);
            Vector2Int chunkCell = biome.GridToChunk(point.x, point.y);
            Require(chunks[chunkCell.x, chunkCell.y].isLoaded && chunks[chunkCell.x, chunkCell.y].isObjectsLoaded, "deferred unload cleanup cannot destroy a reloaded chunk");
            Require(JsonUtility.ToJson(field.Plan) == planJson, "chunk round trip preserves plan");
            Pass("actual chunk unload/reload: alive points return without kill/XP; single cycle, owned objects and stale callbacks cleaned");

            int xp = 0;
            Action<int> observe = gained => xp += gained;
            LevelUpManager.OnExpGained += observe;
            EnemyController defeated = spawner.ActiveEnemy;
            try { defeated.TakeDamage(10000); defeated.GrantExp(); }
            finally { LevelUpManager.OnExpGained -= observe; }
            Require(xp == 3 && SaveService.IsBiomeEliteDefeated(point.spawnId), "kill saved and XP granted exactly once");
            field.Refresh(player.transform.position);
            MoveTo(biome, point.x < 150 ? 280 : 15, point.y < 150 ? 280 : 15); field.Refresh(player.transform.position);
            MoveTo(biome, point.x, point.y); field.Refresh(player.transform.position);
            Require(!field.Spawners.Any(s => s.Placement.spawnId == point.spawnId), "dead point cannot respawn after chunk reload");
            Pass("actual defeat: XP 3 once; defeated spawn ID stays absent after chunk round trip");

            EliteSpawner legacy = biome.GetComponent<EliteSpawner>();
            Require(legacy != null && legacy.enabled, "legacy kill spawner retained");
            int killsBefore = (int)Get(legacy, "totalNormalKills");
            for (int i = 0; i < 10; i++) legacy.NotifyEnemyKilled("Macrophage");
            Require((int)Get(legacy, "totalNormalKills") == killsBefore + 10, "legacy kill counter still works");
            yield return new WaitForSeconds(3.4f);
            Require(Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Any(e => e.Balance != null && e.Balance.Current.MonsterId.StartsWith("legacy-elite.")), "legacy threshold still spawns its own elite");
            Require(JsonUtility.ToJson(field.Plan) == planJson, "kill threshold cannot alter biome elite placements");
            Pass("legacy 10-kill elite still spawns; biome elite plan remains identical and separate");

            MonsterVisitContext visit0 = biome.MonsterVisit;
            GameManager.Instance.CollectRelic(BiomeType.Liver);
            Require(SaveService.RunMonsterStage == 1 && biome.MonsterVisit.Stage == 0, "boss progress does not change current visit");
            var fresh = EnemyController.Acquire(null, "C03VisitProbe", 763203);
            fresh.Configure(null, config.monsters[0], player.transform.position, player.transform.position);
            Require(fresh.Balance.Current.Stage == 0, "new spawn during same visit uses entry stage");
            fresh.ReleaseToPool();
            GameManager.Instance.ReturnToHub();
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_HUB);
            GameManager.Instance.EnterBiome(BiomeType.Intestine);
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_INTESTINE);
            yield return null;
            biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>();
            field = biome.GetComponent<BiomeEliteField>();
            field.Configure(biome, config, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Require(biome.MonsterVisit.Stage == 1 && biome.MonsterVisit.VisitId != visit0.VisitId, "real reentry updates stage once");
            Require(JsonUtility.ToJson(field.Plan) == planJson && SaveService.IsBiomeEliteDefeated(point.spawnId), "real reentry retains plan and defeat");
            BiomeElitePlacement survivorPoint = field.Plan.placements[1];
            MoveTo(biome, survivorPoint.x, survivorPoint.y); field.Refresh(player.transform.position);
            EnemyController survivor = field.Spawners.First(s => s.Placement.spawnId == survivorPoint.spawnId).ActiveEnemy;
            Require(survivor.Balance.Current.Stage == 1 && survivor.Stats.MaxHealth == 14, "stage 1 actual HP 12 × 1.15 rounds to 14");
            Pass("real Hub return/reentry: stage 0 stays frozen, next visit stage 1 gives HP 14 while preserving placement/death");

            string visitId = biome.MonsterVisit.VisitId;
            Require(SaveService.TrySaveActiveRun(out string error), error);
            SaveService.UseStorageRootForTests(storage);
            Require(SaveService.TryContinue(GameDifficulty.Normal, out error), error);
            Require(SaveService.TryRestorePendingSession(out BiomeType resume, out error) && resume == BiomeType.Intestine, error);
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_INTESTINE);
            yield return null;
            biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>();
            field = biome.GetComponent<BiomeEliteField>(); field.Configure(biome, config, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Require(biome.MonsterVisit.VisitId == visitId && biome.MonsterVisit.Stage == 1, "continue restores exact visit");
            Require(JsonUtility.ToJson(field.Plan) == planJson && SaveService.IsBiomeEliteDefeated(point.spawnId), "continue retains plan/kill across disk reload");
            Pass("actual SaveService continue + scene reload restores visit ID, stage, plan and defeated ID");

            GameManager.Instance.EnterBiome(BiomeType.Liver);
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LIVER);
            yield return null;
            biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>();
            Require(biome.GetBiomeConfig().GetEnemySpawnRules().Count == 0, "Liver normal spawn list remains empty");
            field = biome.GetComponent<BiomeEliteField>(); field.Configure(biome, config, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Require(field.Plan.placements.Count == 6, "biome elite placement independent of normal list");
            var liverPoint = field.Plan.placements[0]; MoveTo(biome, liverPoint.x, liverPoint.y); field.Refresh(player.transform.position);
            Require(field.Spawners.Any(s => s.ActiveEnemy != null), "actual Liver field activation with no normal monsters");
            Pass("actual Liver map with empty normal rules still places and activates biome elites");
        }

        private static IEnumerator Delayed(Action action) { yield return new WaitForSeconds(.5f); action(); }
        private static void MoveTo(BiomeManager biome, int x, int y)
        {
            player = PlayerController.Instance;
            player.HealthComponent.GrantTemporaryInvincibility(30);
            player.SpawnAt(biome.GridToWorldWithHeight(x, y) + Vector3.right * 3);
            typeof(BiomeManager).GetMethod("UpdateChunks", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(biome, null);
            DontStarveCamera.Instance?.SnapToTarget();
        }
        private static object Get(object value, string name) => value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(value);
        private static void Require(bool result, string message) { if (!result) throw new InvalidOperationException(message); }
        private static void Pass(string message) { report.Add("PASS " + message); Debug.Log("[BiomeElite-C03-Play] PASS " + message); }
    }
}
