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
    public static partial class HelicoSpiralConnectionRunner
    {
        private static bool battle, preview, started, passed, oldBackground, oldOptionsEnabled, sourceEnabled;
        private static float angle, oldTimeScale, groundOffset, oldCaptureDelta;
        private static int enteredFrame, oldFrameRate, oldVsync;
        private static string phase, storage, battleSection;
        private static string StageLabel => battleSection == "tail" ? "HelicoSpiral-S01-B2" + (phase == "alignment" ? "-alignment" : "") : battle ? "HelicoSpiral-S01-A3" + (battleSection == "all" ? "" : "-" + battleSection) : "HelicoSpiral-S01-A2";
        private static double deadline;
        private static SceneSetup[] scenes;
        private static EnterPlayModeOptions oldOptions;
        private static MonsterDefinition definition;
        private static HelicoSpiralPatternSettings settings;
        private static EnemyController enemy;
        private static HelicoSpiralElitePattern pattern;
        private static PlayerController player;
        private static Health health;
        private static ProceduralBiomeBridge biome;
        private static BiomeEliteField field;
        private static BiomeEliteSpawnConfig temporary, source;
        private static Vector3 origin, fieldOrigin;
        private static readonly BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly List<string> results = new List<string>();
        private static readonly Dictionary<Object, string> backups = new Dictionary<Object, string>();
        public static void RunTailAlignmentChecks() => Start(false, 0, "alignment", false, "tail");
        public static void RunTailChecks() => Start(false, 0, "tail", false, "tail");
        public static void PreviewTail(float targetAngle = 0, string state = "sweep") => Start(true, targetAngle, state, false, "tail");
        public static void Run() => Start(false, 0, "dash");
        public static void RunBattleChecks() => Start(false, 0, "dash", true);
        public static void RunBattleSection(string section) => Start(false, 0, "dash", true, section);
        public static void PreviewBattle(float targetAngle = 0, string state = "walk") => Start(true, targetAngle, state, true);
        public static void Preview(float targetAngle = 0, string state = "dash") => Start(true, targetAngle, state);
        private static void Start(bool show, float targetAngle, string state, bool fullBattle = false, string section = "all")
        {
            if (section == "tail") throw new InvalidOperationException("S-01-B tail sweep is retired; use Run() for the dash-only pattern.");
            if (!new[] { "all", "states", "terrain", "death", "field", "visits", "repeat", "sorting", "outgoing", "tail" }.Contains(section)) throw new ArgumentException("Unknown S-01 battle section");
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) throw new InvalidOperationException("Saved Edit Mode and completed compilation required");
            for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("변경 중인 씬을 먼저 저장하세요.");
            definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(HelicoSpiralSetup.DefinitionPath);
            Require(definition != null && definition.GetValidationError() == null, "valid S-01 source"); settings = (HelicoSpiralPatternSettings)definition.pattern;
            scenes = EditorSceneManager.GetSceneManagerSetup(); battle = fullBattle; battleSection = section; preview = show; angle = targetAngle; phase = state; started = passed = false; results.Clear(); backups.Clear();
            storage = Path.Combine(Path.GetTempPath(), (battle ? "necrocis-s01-a3-" : "necrocis-s01-a2-") + Guid.NewGuid().ToString("N"));
            SaveService.UseStorageRootForTests(storage); Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error), error);
            source = AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>("Assets/_Project/Data/BiomeElites/StomachBiomeEliteSpawnConfig.asset");
            Require(source.monsters.Count == 0, "production Stomach stays unregistered"); sourceEnabled = source.enabled; source.enabled = false;
            oldFrameRate = Application.targetFrameRate; oldVsync = QualitySettings.vSyncCount;
            oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled; oldOptions = EditorSettings.enterPlayModeOptions; oldBackground = Application.runInBackground; oldTimeScale = Time.timeScale; oldCaptureDelta = Time.captureDeltaTime;
            EditorSettings.enterPlayModeOptionsEnabled = true; EditorSettings.enterPlayModeOptions = oldOptions | EnterPlayModeOptions.DisableDomainReload;
            Application.runInBackground = true; EditorApplication.playModeStateChanged += OnPlay; EditorApplication.update += Tick;
            deadline = EditorApplication.timeSinceStartup + (battle ? 1200 : 600);
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Hub.unity"); EditorApplication.isPlaying = true;
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode) enteredFrame = Time.frameCount;
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlay; Restore(); source.enabled = sourceEnabled;
            EditorSettings.enterPlayModeOptionsEnabled = oldOptionsEnabled; EditorSettings.enterPlayModeOptions = oldOptions;
            Time.timeScale = oldTimeScale; Time.captureDeltaTime = oldCaptureDelta; Application.runInBackground = oldBackground; Application.targetFrameRate = oldFrameRate; QualitySettings.vSyncCount = oldVsync;
            SaveService.ResetStaticStateForTests(); DifficultyBalanceService.ResetForTests(); if (temporary != null) Object.DestroyImmediate(temporary);
            temporary = null; enemy = null; pattern = null; if (Directory.Exists(storage)) Directory.Delete(storage, true);
            if (!preview)
            {
                if (!passed && !results.Any(r => r.StartsWith("FAIL"))) results.Add("FAIL interrupted before completing checks");
                Directory.CreateDirectory("Logs"); File.WriteAllLines("Logs/" + StageLabel + "-results.txt", results); Debug.Log("[" + StageLabel + "] " + (passed ? "ALL PASS" : "FAIL"));
            }
            if (scenes != null && scenes.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(scenes);
        }
        private static void Tick()
        {
            if (EditorApplication.isPlaying && !started) Application.runInBackground = true;
            if (EditorApplication.timeSinceStartup > deadline) { deadline = double.PositiveInfinity; Fail(new TimeoutException(StageLabel + " timeout")); return; }
            if (started || !EditorApplication.isPlaying || Time.frameCount - enteredFrame < 15) return;
            player = PlayerController.Instance; if (player == null || player.HealthComponent == null) return;
            health = player.HealthComponent; started = true; Time.timeScale = 1; player.StartCoroutine(Guard(Checks()));
        }
        private static IEnumerator Guard(IEnumerator root)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(root);
            while (stack.Count > 0)
            {
                object next;
                try { var current = stack.Peek(); if (!current.MoveNext()) { (current as IDisposable)?.Dispose(); stack.Pop(); continue; } next = current.Current; if (next is IEnumerator nested) { stack.Push(nested); continue; } }
                catch (Exception error) { Fail(error); yield break; }
                yield return next;
            }
            passed = true;
            if (preview) { yield return new WaitForEndOfFrame(); deadline = double.PositiveInfinity; EditorApplication.isPaused = true; Selection.activeGameObject = enemy.gameObject; }
            else EditorApplication.isPlaying = false;
        }
        private static void Spawn()
        {
            if (enemy != null && enemy.gameObject.activeSelf) enemy.ReleaseToPool(); Move(origin + Vector3.right * 6);
            var rule = HelicoSpiralSetup.PreviewRule(definition);
            enemy = EnemyController.Acquire(null, "S01_ConnectionProbe", EnemyController.GetPoolArchetypeId(rule));
            enemy.Configure(null, rule, origin, origin); enemy.SuppressExperienceReward = true;
            enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0);
            pattern = enemy.GetComponent<HelicoSpiralElitePattern>(); health.ResetHealth();
        }
        private static void Move(Vector3 position)
        {
            position.y = biome.GetGroundHeight(position) + groundOffset; player.SpawnAt(position); DontStarveCamera.Instance?.SnapToTarget();
            typeof(BiomeManager).GetMethod("UpdateChunks", Private).Invoke(biome, null);
            foreach (var s in Object.FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None)) { s.ReleaseSpawnedEnemies(); s.enabled = false; }
        }
        private static Vector3 Aim(float n) => new Vector3(Mathf.Cos(n * Mathf.Deg2Rad), 0, Mathf.Sin(n * Mathf.Deg2Rad));
        private static SpriteRenderer Body() => enemy.transform.Find("Visual").GetComponent<SpriteRenderer>();
        private static void Set(Object value, string key, float number)
        {
            if (!backups.ContainsKey(value)) backups.Add(value, EditorJsonUtility.ToJson(value));
            using var data = new SerializedObject(value); data.FindProperty(key).floatValue = number; data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Restore()
        {
            foreach (var b in backups) { EditorJsonUtility.FromJsonOverwrite(b.Value, b.Key); EditorUtility.ClearDirty(b.Key); }
            backups.Clear();
        }
        private static IEnumerator Wait(Func<bool> condition, float seconds, string label)
        {
            float until = Time.time + seconds; while (!condition()) { Require(Time.time < until, "timeout: " + label); yield return null; }
        }
        private static void Equal(float expected, float actual, string label) => Require(Mathf.Abs(expected - actual) < .006f, label + $": expected {expected}, got {actual}");
        private static void Require(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); }
        private static void Pass(string label) { results.Add("PASS " + label); Debug.Log("[" + StageLabel + "] PASS " + label); }
        private static void Fail(Exception error) { passed = false; results.Add("FAIL " + error); Debug.LogException(error); EditorApplication.isPaused = false; EditorApplication.isPlaying = false; }
    }
}
