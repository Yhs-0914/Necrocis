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
    public static partial class HardenedResiduePlayModeRunner
    {
        private static bool preview, deathPreview, started, passed, oldOptionsEnabled, oldBackground;
        private static bool directionChecks;
        private static int stage3Mode;
        private static SceneSetup[] oldScenes;
        private static EnterPlayModeOptions oldOptions;
        private static float oldTimeScale, playerGroundOffset;
        private static double deadline;
        private static int enteredFrame;
        private static string storage;
        private static MonsterDefinition definition;
        private static HardenedResiduePatternSettings settings;
        private static PlayerController player;
        private static Health health;
        private static EnemyController enemy;
        private static HardenedResidueElitePattern pattern;
        private static BiomeEliteSpawnConfig fieldConfig;
        private static BiomeEliteSpawnConfig productionField;
        private static bool productionWasEnabled;
        private static BiomeEliteField field;
        private static ProceduralBiomeBridge biome;
        private static BiomeElitePlacement point;
        private static Vector3 origin;
        private static readonly Dictionary<Object, string> backups = new Dictionary<Object, string>();
        private static readonly List<string> results = new List<string>();
        private static readonly List<GameObject> projectiles = new List<GameObject>();

        [MenuItem("Necrocis/Balance/Run I-02-A Hardened Residue Checks")]
        public static void Run() => Start(false, false);
        [MenuItem("Necrocis/Balance/Preview I-02-A Hardened Residue")]
        public static void Preview() => Start(true, false);
        [MenuItem("Necrocis/Balance/Preview I-02-A Hardened Residue Death")]
        public static void PreviewDeath() => Start(true, true);

        private static void Start(bool previewOnly, bool showDeath, bool directions = false, int stage3 = 0)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("변경 중인 씬을 먼저 저장하세요.");
            oldScenes = EditorSceneManager.GetSceneManagerSetup();
            definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(HardenedResidueSetup.DefinitionPath);
            Require(definition != null && definition.GetValidationError() == null, "valid I-02 definition and sprites");
            settings = (HardenedResiduePatternSettings)definition.pattern;
            preview = previewOnly; deathPreview = showDeath; started = passed = false; results.Clear(); backups.Clear(); projectiles.Clear();
            directionChecks = directions;
            stage3Mode = stage3;
            productionField = AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>(IntestineEliteMapSetup.ConfigPath);
            productionWasEnabled = productionField.enabled; productionField.enabled = false;
            storage = Path.Combine(Path.GetTempPath(), "necrocis-i02-" + Guid.NewGuid().ToString("N"));
            SaveService.UseStorageRootForTests(storage);
            Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error), error);
            oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled; oldOptions = EditorSettings.enterPlayModeOptions;
            oldTimeScale = Time.timeScale; oldBackground = Application.runInBackground;
            Application.runInBackground = true;
            EditorSettings.enterPlayModeOptionsEnabled = true; EditorSettings.enterPlayModeOptions = oldOptions | EnterPlayModeOptions.DisableDomainReload;
            EditorApplication.playModeStateChanged += OnPlayMode; EditorApplication.update += Tick;
            deadline = EditorApplication.timeSinceStartup + 240;
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Hub.unity"); EditorApplication.isPlaying = true;
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode) enteredFrame = Time.frameCount;
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlayMode;
            RestoreAssets();
            productionField.enabled = productionWasEnabled;
            EditorSettings.enterPlayModeOptionsEnabled = oldOptionsEnabled; EditorSettings.enterPlayModeOptions = oldOptions;
            AssetDatabase.SaveAssets();
            Time.timeScale = oldTimeScale; Application.runInBackground = oldBackground;
            SaveService.ResetStaticStateForTests(); DifficultyBalanceService.ResetForTests();
            if (fieldConfig != null) Object.DestroyImmediate(fieldConfig);
            fieldConfig = null; enemy = null; pattern = null; projectiles.Clear();
            if (Directory.Exists(storage)) Directory.Delete(storage, true);
            if (!preview)
            {
                Directory.CreateDirectory("Logs");
                string report = stage3Mode == 1 ? "Logs/Residue-D3-battle-results.txt"
                    : stage3Mode == 2 ? "Logs/Residue-D3-terrain-results.txt"
                    : directionChecks ? DirectionReport : "Logs/Residue-I02A-results.txt";
                File.WriteAllLines(report, results);
                Debug.Log("[Residue-I02A] " + (passed ? "ALL PASS" : "FAIL"));
            }
            if (oldScenes != null && oldScenes.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(oldScenes);
            if (Application.isBatchMode) EditorApplication.Exit(passed ? 0 : 1);
        }

        private static void Tick()
        {
            if (EditorApplication.isPlaying && !started) Application.runInBackground = true;
            if (EditorApplication.timeSinceStartup > deadline)
            { Fail(new TimeoutException("I-02 test timed out")); deadline = double.PositiveInfinity; return; }
            if (started || !EditorApplication.isPlaying || Time.frameCount - enteredFrame < 15) return;
            player = PlayerController.Instance; if (player == null || player.HealthComponent == null) return;
            health = player.HealthComponent; started = true; Time.timeScale = 1;
            if (!preview) Application.runInBackground = true;
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
                yield return new WaitForEndOfFrame(); deadline = double.PositiveInfinity;
                EditorApplication.isPaused = true; Selection.activeGameObject = enemy.gameObject;
                Debug.Log(deathPreview ? "[Residue-I02A] Dedicated collapse death preview. Unpause to finish. Temporary saves only."
                    : "[Residue-I02A] Preview: fixed crack → one slab → recovery. Unpause; Q/W can break the slab, or walk around it. Temporary saves only.");
            }
            else EditorApplication.isPlaying = false;
        }

        private static void Fail(Exception error)
        {
            passed = false; results.Add("FAIL " + error); Debug.LogException(error); RestoreAssets(); EditorApplication.isPlaying = false;
        }

        private static IEnumerator SetupField()
        {
            CheckImports();
            GameManager.Instance.EnterBiome(BiomeType.Intestine);
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_INTESTINE); yield return null;
            biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>();
            playerGroundOffset = player.transform.position.y - biome.GetGroundHeight(player.transform.position);
            Equal(-2, playerGroundOffset, "preserve actual gameplay player ground offset");
            field = biome.GetComponent<BiomeEliteField>();
            Require(field.Plan == null, "single-species test isolates the production field");
            fieldConfig = ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>();
            fieldConfig.minimumCount = fieldConfig.maximumCount = 1; fieldConfig.clearanceCells = 6;
            fieldConfig.monsters.Add(HardenedResidueSetup.PreviewRule(definition));
            field.Configure(biome, fieldConfig, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Require(field.Plan != null && field.Plan.placements.Count == 1, "one real map I-02 placement");
            point = field.Plan.placements[0]; origin = biome.GridToWorldWithHeight(point.x, point.y);
            MovePlayer(8); UpdateChunks(); yield return new WaitForSeconds(.25f);
            foreach (EnemySpawner spawner in Object.FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None))
            { spawner.ReleaseSpawnedEnemies(); spawner.enabled = false; }
            foreach (EnemyController other in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
                if (other.Balance == null || other.Balance.Current.MonsterId != definition.monsterId) other.ReleaseToPool();
            if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
            field.Refresh(player.transform.position); enemy = field.Spawners.Single().ActiveEnemy;
            pattern = enemy.GetComponent<HardenedResidueElitePattern>();
            Require(pattern != null && enemy.IsAiSuppressed && enemy.IsElite, "field automatically attaches I-02 without kills");
            Require(ResiduePlacementSafety.CanPlace(origin + Vector3.right * settings.placementDistance, Vector3.forward, settings.footprint), "field preview has a safe landing and detour perimeter");
        }

        private static void Spawn()
        {
            foreach (GameObject shot in projectiles) if (shot != null) Object.Destroy(shot);
            projectiles.Clear();
            if (enemy != null && enemy.gameObject.activeSelf) enemy.ReleaseToPool();
            MovePlayer(8);
            var rule = HardenedResidueSetup.PreviewRule(definition);
            enemy = EnemyController.Acquire(null, "I02_Test", EnemyController.GetPoolArchetypeId(rule));
            enemy.Configure(null, rule, origin, origin); enemy.SuppressExperienceReward = true;
            enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0);
            pattern = enemy.GetComponent<HardenedResidueElitePattern>();
        }

        private static IEnumerator Land()
        {
            Spawn(); MovePlayer(2.6f); health.ResetHealth();
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Recovery && pattern.ActiveRubble != null, 4, "landing");
        }

        private static void MovePlayer(float distance) => MovePlayerTo(origin + Vector3.right * distance);
        private static void MovePlayerTo(Vector3 position)
        {
            position.y = biome.GetGroundHeight(position) + playerGroundOffset;
            player.SpawnAt(position); DontStarveCamera.Instance?.SnapToTarget();
        }
        private static void UpdateChunks() => typeof(BiomeManager).GetMethod("UpdateChunks", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(biome, null);
        private static void Face(Vector3 direction)
        {
            typeof(PlayerController).GetField("movement", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(player, Vector3.zero);
            typeof(PlayerController).GetField("lastMoveDirection", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(player, direction);
        }
        private static object Invoke(object target, string method, params object[] arguments)
            => target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, arguments);
        private static IEnumerator Wait(Func<bool> condition, float seconds, string label)
        {
            float until = Time.time + seconds;
            while (!condition())
            {
                Require(Time.time < until, "timeout: " + label + $"; phase={pattern?.Phase}, rejected={pattern?.RejectedPlacements}, "
                    + $"enemy={enemy?.transform.position}, active={enemy?.gameObject.activeInHierarchy}, player={player?.transform.position}, "
                    + $"chase={enemy?.IsPlayerInChaseRange()}, leash={enemy?.IsOutOfLeash()}, timeScale={Time.timeScale}");
                yield return null;
            }
        }
        private static void Set(Object source, string property, float value)
        {
            Backup(source); using var data = new SerializedObject(source); data.FindProperty(property).floatValue = value; data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void SetHits(int value)
        {
            Backup(settings); using var data = new SerializedObject(settings); data.FindProperty("hitsToBreak").intValue = value; data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Backup(Object source) { if (!backups.ContainsKey(source)) backups.Add(source, EditorJsonUtility.ToJson(source)); }
        private static void RestoreAssets()
        {
            foreach (var entry in backups) if (entry.Key != null) { EditorJsonUtility.FromJsonOverwrite(entry.Value, entry.Key); EditorUtility.ClearDirty(entry.Key); }
            backups.Clear();
        }
        private static void Require(bool value, string label) { if (!value) throw new InvalidOperationException(label); }
        private static void Equal(float expected, float actual, string label) => Require(Mathf.Abs(expected - actual) < .001f, label + $": {expected} expected, {actual} actual");
        private static void Pass(string message) { results.Add("PASS " + message); Debug.Log("[Residue-I02A] PASS " + message); }

        private static void CheckImports()
        {
            var art = settings.presentation;
            Require(art.deathFrames.Length == 6 && art.deathFrames.Distinct().Count() == 6, "six dedicated death frames");
            foreach (Sprite sprite in art.idleFrames.Concat(art.moveFrames).Concat(art.liftFrames).Concat(art.deathFrames).Concat(new[] { art.release, art.recovery, art.hit, art.rubble }))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite));
                Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "pixel sprite import");
                Equal(Mathf.Round(sprite.pivot.x), sprite.pivot.x, "integer x anchor"); Equal(Mathf.Round(sprite.pivot.y), sprite.pivot.y, "integer ground anchor");
            }
            Pass("12 main/prop sprites and 6 dedicated death sprites: Point, no mip, uncompressed, integer ground anchors");
        }
    }
}
