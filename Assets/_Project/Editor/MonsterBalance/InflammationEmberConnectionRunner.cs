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
    public static partial class InflammationEmberConnectionRunner
    {
        private static BiomeEliteSpawnConfig productionLiver;
        private static bool productionLiverEnabled;
        private static int productionLiverCount;
        private static bool preview, started, passed, oldOptionsEnabled, oldBackground;
        private static bool battle, flightPreview, heightAudit;
        private static bool directionChecks, directionalBattle;
        private static SceneSetup[] oldScenes;
        private static Vector3 previewDirection;
        private static EnterPlayModeOptions oldOptions;
        private static float oldTimeScale, groundOffset;
        private static double deadline;
        private static int enteredFrame;
        private static string storage;
        private static MonsterDefinition definition;
        private static InflammationEmberPatternSettings settings;
        private static PlayerController player;
        private static Health health;
        private static EnemyController enemy;
        private static InflammationEmberElitePattern pattern;
        private static BiomeEliteSpawnConfig temporaryConfig;
        private static BiomeEliteField field;
        private static ProceduralBiomeBridge biome;
        private static Vector3 origin;
        private static readonly Dictionary<Object, string> backups = new Dictionary<Object, string>();
        private static readonly List<string> results = new List<string>();

        [MenuItem("Necrocis/Balance/Run H-02 Stage 2 Connection Checks")]
        public static void Run() => Start(false);
        [MenuItem("Necrocis/Balance/Preview H-02 Inflammation Ember")]
        public static void Preview() => Start(true);
        [MenuItem("Necrocis/Balance/Run H-02 Stage 3 Battle Checks")]
        public static void RunBattleChecks() => Start(false, true);
        [MenuItem("Necrocis/Balance/Run H-02 Terrain Height Checks")]
        public static void RunHeightChecks() => Start(false, true, false, 180, true);
        [MenuItem("Necrocis/Balance/Preview H-02 Ground Thorn Flight")]
        public static void PreviewFlight() => PreviewFlightAngle(45);
        public static void PreviewFlightAngle(float angle) => Start(true, false, true, angle);

        private static void Start(bool previewOnly, bool fullBattle = false, bool showFlight = false, float angle = 180, bool heightsOnly = false, bool directions = false, bool fullDirections = false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("변경 중인 씬을 먼저 저장하세요.");
            oldScenes = EditorSceneManager.GetSceneManagerSetup(); directionChecks = directions; directionalBattle = fullDirections;
            definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(InflammationEmberSetup.DefinitionPath);
            Require(definition != null && definition.GetValidationError() == null, "valid H-02 sources");
            settings = (InflammationEmberPatternSettings)definition.pattern;
            preview = previewOnly; started = passed = false; results.Clear(); backups.Clear();
            battle = fullBattle; flightPreview = showFlight; heightAudit = heightsOnly;
            previewDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
            storage = Path.Combine(Path.GetTempPath(), "necrocis-h02-connection-" + Guid.NewGuid().ToString("N"));
            SaveService.UseStorageRootForTests(storage); Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error), error);
            oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled; oldOptions = EditorSettings.enterPlayModeOptions;
            oldTimeScale = Time.timeScale; oldBackground = Application.runInBackground;
            Application.runInBackground = true;
            EditorSettings.enterPlayModeOptionsEnabled = true; EditorSettings.enterPlayModeOptions = oldOptions | EnterPlayModeOptions.DisableDomainReload;
            EditorApplication.playModeStateChanged += OnPlayMode; EditorApplication.update += Tick;
            deadline = EditorApplication.timeSinceStartup + (directionalBattle ? 600 : battle ? 300 : 180);
            productionLiver = AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>(LiverEliteMapSetup.ConfigPath);
            productionLiverEnabled = productionLiver.enabled; productionLiverCount = productionLiver.monsters.Count;
            productionLiver.enabled = false; // Temporary isolation; restore after Play without saving.
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Hub.unity"); EditorApplication.isPlaying = true;
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode) enteredFrame = Time.frameCount;
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlayMode;
            RestoreAssets(); productionLiver.enabled = productionLiverEnabled; EditorSettings.enterPlayModeOptionsEnabled = oldOptionsEnabled; EditorSettings.enterPlayModeOptions = oldOptions;
            Time.timeScale = oldTimeScale; Application.runInBackground = oldBackground;
            SaveService.ResetStaticStateForTests(); DifficultyBalanceService.ResetForTests();
            if (temporaryConfig != null) Object.DestroyImmediate(temporaryConfig);
            temporaryConfig = null; enemy = null; pattern = null;
            if (Directory.Exists(storage)) Directory.Delete(storage, true);
            if (!preview)
            { Directory.CreateDirectory("Logs"); File.WriteAllLines(directionalBattle ? "Logs/Ember-H02-D3-results.txt" : directionChecks ? "Logs/Ember-H02-D2-results.txt" : heightAudit ? "Logs/Ember-H02-height-results.txt" : battle ? "Logs/Ember-H02-battle-results.txt" : "Logs/Ember-H02-connection-results.txt", results); Debug.Log("[Ember-H02] " + (passed ? "ALL PASS" : "FAIL")); }
            if (oldScenes != null && oldScenes.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(oldScenes);
            if (Application.isBatchMode) EditorApplication.Exit(passed ? 0 : 1);
        }

        private static void Tick()
        {
            if (EditorApplication.isPlaying && !started) Application.runInBackground = true;
            if (EditorApplication.timeSinceStartup > deadline)
            { Fail(new TimeoutException("H-02 connection check timed out")); deadline = double.PositiveInfinity; return; }
            if (started || !EditorApplication.isPlaying || Time.frameCount - enteredFrame < 15) return;
            player = PlayerController.Instance; if (player == null || player.HealthComponent == null) return;
            health = player.HealthComponent; started = true; Time.timeScale = 1; Application.runInBackground = true;
            player.StartCoroutine(Guard(Checks()));
        }

        private static IEnumerator Guard(IEnumerator root)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(root);
            while (stack.Count > 0)
            {
                object next;
                try
                {
                    IEnumerator current = stack.Peek();
                    if (!current.MoveNext()) { (current as IDisposable)?.Dispose(); stack.Pop(); continue; }
                    next = current.Current;
                    if (next is IEnumerator nested) { stack.Push(nested); continue; }
                }
                catch (Exception error) { Fail(error); yield break; }
                yield return next;
            }
            passed = true;
            if (preview)
            {
                yield return new WaitForEndOfFrame(); deadline = double.PositiveInfinity; Application.runInBackground = oldBackground;
                EditorApplication.isPaused = true; Selection.activeGameObject = enemy.gameObject;
                Debug.Log(flightPreview
                    ? "[Ember-H02] Ground-flight preview: thorn is in flight. Unpause to continue. Temporary saves; no production Liver registration."
                    : "[Ember-H02] Preview: unpause to fire the committed counter. Q can trigger the next one after recovery/rearm. Temporary saves; no production Liver registration.");
            }
            else EditorApplication.isPlaying = false;
        }

        private static IEnumerator Checks()
        {
            var art = settings.presentation;
            var frames = art.idleFrames.Concat(art.moveFrames).Concat(art.preparationFrames).Concat(new[] { art.release, art.recovery, art.hit }).Concat(art.deathFrames).Concat(art.thornFrames).ToArray();
            Require(frames.Length == 18 && frames.Distinct().Count() == 18, "18 distinct approved sprites");
            foreach (var sprite in frames)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite));
                Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "pixel import settings");
                Equal(Mathf.Round(sprite.pivot.x), sprite.pivot.x, "integer X pivot"); Equal(Mathf.Round(sprite.pivot.y), sprite.pivot.y, "integer Y pivot");
            }
            Require(definition.contact.enabled, "shared body contact source enabled");
            Pass("18 unique sprites, dedicated six-frame death, Point/no mip/uncompressed and integer anchors connected to H-02 sources");

            GameManager.Instance.EnterBiome(BiomeType.Liver); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LIVER); yield return null;
            biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>(); field = biome.GetComponent<BiomeEliteField>();
            groundOffset = player.transform.position.y - biome.GetGroundHeight(player.transform.position); Equal(-2, groundOffset, "actual player ground offset");
            Require(field.Plan == null && biome.GetBiomeConfig().biomeEliteSpawnConfig.monsters.Count == productionLiverCount, "production Liver list unchanged");
            temporaryConfig = ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>();
            temporaryConfig.minimumCount = temporaryConfig.maximumCount = 1; temporaryConfig.clearanceCells = 5;
            temporaryConfig.monsters.Add(InflammationEmberSetup.PreviewRule(definition));
            field.Configure(biome, temporaryConfig, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Require(field.Plan.placements.Count == 1, "one temporary Liver map point");
            var point = field.Plan.placements[0]; origin = biome.GridToWorldWithHeight(point.x, point.y);
            if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
            Spawn();
            Require(enemy.IsAiSuppressed && enemy.IsElite && enemy.Config.deathSprites.Length == 6, "exclusive pattern and death hookup");
            Equal(0, enemy.transform.Find("Visual").localPosition.y, "foot pivot aligned to ground");
            Pass("temporary real Liver field automatically attaches counter pattern and death at zero kills; production map list unchanged");
            if (directionalBattle) { yield return DirectionBattleChecks(); yield break; }
            if (directionChecks) { yield return DirectionChecks(); yield break; }
            if (preview)
            {
                if (flightPreview)
                {
                    MoveTo(origin + previewDirection * 4.5f); yield return new WaitForSeconds(.25f); enemy.TakeDamage(1);
                    yield return Wait(() => pattern.ActiveThorn != null, 2, "flight preview release");
                    yield return Wait(() => pattern.ActiveThorn == null || Vector3.Distance(pattern.ActiveThorn.transform.position, pattern.ActiveThorn.LaunchPosition) > 1.75f, 1, "flight preview separation");
                    Require(pattern.ActiveThorn != null, "preview thorn remains visible before contact"); yield break;
                }
                Move(-2.5f); yield return new WaitForSeconds(.3f); enemy.TakeDamage(1);
                yield return Wait(() => pattern.Phase == InflammationEmberPhase.Windup, 1, "preview tell");
                yield return new WaitForSeconds(settings.windupSeconds * .65f); yield break;
            }
            if (heightAudit) { yield return TerrainHeightChecks(); yield break; }
            if (battle) { yield return BattleChecks(); yield break; }
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 60, true);
            Move(3); yield return new WaitForSeconds(.9f);
            Require(pattern.Phase == InflammationEmberPhase.Ready && pattern.ShotCount == 0, "no attack without a hit");
            var body = enemy.transform.Find("Visual").GetComponent<SpriteRenderer>(); Require(!body.flipX, "right-authored idle faces right");
            Move(-3); yield return null; Require(body.flipX, "mirrored idle faces left");
            health.ResetHealth(); float hp = health.CurrentHealth, start = Time.time;
            enemy.TakeDamage(1);
            Require(pattern.Phase == InflammationEmberPhase.Windup && pattern.LockedDirection.x < -.99f, "first hit commits left-facing preparation");
            yield return Wait(() => pattern.ShotCount == 1, 2, "first counter release");
            Require(Time.time - start >= .69f && pattern.ActiveThorn != null, "counter launches after visible preparation");
            Equal(.7f, pattern.ActiveThorn.Length, "authored thorn length"); Equal(.16f, pattern.ActiveThorn.FlightHeight, "ground-height thorn");
            Require(pattern.ActiveThorn.transform.childCount == 1 && pattern.ActiveThorn.transform.GetChild(0).name == "ThornSprite", "simple thorn has sprite only, no floor UI");
            yield return Wait(() => health.CurrentHealth < hp, 2, "thorn hit");
            Equal(3, hp - health.CurrentHealth, "actual counter damage 2 x 1.5 = 3 once");
            yield return Wait(() => pattern.Phase == InflammationEmberPhase.Ready, 2, "recovery ends");
            Require(pattern.NextReadyTime - Time.time > 2.4f && pattern.NextReadyTime - Time.time <= 2.51f, "single 2.5-second rearm");
            Pass("first-hit event → .7s locked left-facing tell → one grounded thorn → actual 3 HP damage → 1.2s recovery and 2.5s rearm; no autonomous attack or floor UI");

            Set(settings, "windupSeconds", .4f); Set(settings, "thornLength", .9f); Set(settings, "thornRadius", .2f);
            Set(definition, "statSets.Array.data[0].attackPower", 4);
            int index = definition.patternDamage.FindIndex(p => p.id == InflammationEmberPatternSettings.DamageId);
            Set(definition, $"patternDamage.Array.data[{index}].coefficient", 2);
            Set(DifficultyBalanceService.GetProfile(GameDifficulty.Normal), "elites.outgoingDamage", 1.5f);
            Equal(.7f, pattern.WindupDuration, "existing actor retains its captured tell"); Spawn(); Move(3); health.ResetHealth(); hp = health.CurrentHealth;
            enemy.TakeDamage(1); Equal(.4f, pattern.WindupDuration, "Inspector tell on new actor");
            yield return Wait(() => pattern.ActiveThorn != null, 2, "edited thorn");
            Equal(.9f, pattern.ActiveThorn.Length, "edited visible/collision length"); Equal(.2f, pattern.ActiveThorn.HitRadius, "edited thickness");
            yield return Wait(() => health.CurrentHealth < hp, 2, "edited damage"); Equal(12, hp - health.CurrentHealth, "actual edited damage 4 x 1.5 x 2 = 12 once");
            RestoreAssets(); enemy.ReleaseToPool(); yield return null;
            Pass("Inspector edits reach the next field actor: .4s tell, .9m length, .2m radius and actual 12 HP damage; previous capture unchanged, sources restored");
        }

        private static void Spawn()
        {
            if (enemy != null && enemy.gameObject.activeSelf) enemy.ReleaseToPool();
            Move(4); field.Refresh(player.transform.position); enemy = field.Spawners.Single().ActiveEnemy;
            pattern = enemy.GetComponent<InflammationEmberElitePattern>();
            if (!preview) enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0);
        }
        private static void Move(float distance) => MoveTo(origin + Vector3.right * distance);
        private static void MoveTo(Vector3 position)
        {
            position.y = biome.GetGroundHeight(position) + groundOffset;
            player.SpawnAt(position); DontStarveCamera.Instance?.SnapToTarget();
            typeof(BiomeManager).GetMethod("UpdateChunks", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(biome, null);
            foreach (EnemySpawner spawner in Object.FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None)) { spawner.ReleaseSpawnedEnemies(); spawner.enabled = false; }
        }
        private static IEnumerator Wait(Func<bool> condition, float seconds, string label)
        { float until = Time.time + seconds; while (!condition()) { Require(Time.time < until, "timeout: " + label); yield return null; } }
        private static void Set(Object source, string path, float value)
        {
            if (!backups.ContainsKey(source)) backups.Add(source, EditorJsonUtility.ToJson(source));
            using var data = new SerializedObject(source); data.FindProperty(path).floatValue = value; data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void RestoreAssets()
        { foreach (var item in backups) if (item.Key != null) { EditorJsonUtility.FromJsonOverwrite(item.Value, item.Key); EditorUtility.ClearDirty(item.Key); } backups.Clear(); }
        private static void Require(bool value, string label) { if (!value) throw new InvalidOperationException(label); }
        private static void Equal(float expected, float actual, string label) => Require(Mathf.Abs(expected - actual) < .002f, label + $": expected {expected}, actual {actual}");
        private static void Pass(string label) { results.Add("PASS " + label); Debug.Log((battle ? "[Ember-H02-Battle] PASS " : "[Ember-H02-Connection] PASS ") + label); }
        private static void Fail(Exception error)
        { passed = false; results.Add("FAIL " + error); Debug.LogException(error); EditorApplication.isPaused = false; EditorApplication.isPlaying = false; }
    }
}
