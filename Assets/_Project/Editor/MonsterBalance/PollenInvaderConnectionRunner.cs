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
    public static partial class PollenInvaderConnectionRunner
    {
        private static BiomeEliteSpawnConfig productionLung;
        private static bool productionLungEnabled;
        private static int productionLungCount;
        private static bool regression, extension, originalFollowupEnabled, battle, preview, started, passed, oldBackground, oldOptionsEnabled;
        private static float angle, oldTimeScale, groundOffset;
        private static string previewPhase, storage, battleSection;
        private static SceneSetup[] scenes;
        private static EnterPlayModeOptions oldOptions;
        private static double deadline;
        private static int enteredFrame, oldFrameRate, oldVsync;
        private static MonsterDefinition definition;
        private static PollenInvaderPatternSettings settings;
        private static EnemyController enemy;
        private static PollenInvaderElitePattern pattern;
        private static PlayerController player;
        private static Health health;
        private static ProceduralBiomeBridge biome;
        private static BiomeEliteField field;
        private static BiomeEliteSpawnConfig temporary;
        private static Vector3 origin, fieldOrigin;
        private static readonly List<string> results = new List<string>();
        private static readonly Dictionary<Object, string> backups = new Dictionary<Object, string>();
        private static readonly BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        public static void Run() => Start(false, 90, "flight");
        public static void RunFollowupChecks() => Start(false, 90, "flight", false, "all", true);
        public static void RunFollowupTail() => Start(false, 90, "flight", false, "tail", true);
        public static void RunFollowupRegression() => Start(false, 90, "flight", false, "all", false, true);
        public static void PreviewFollowup(float targetAngle = 90, string phase = "second") => Start(true, targetAngle, phase, false, "all", true);
        public static void RunBattleChecks() => Start(false, 90, "flight", true);
        public static void RunBattleSection(string section) => Start(false, 90, "flight", true, section);
        private static string StageLabel => regression ? "PollenInvader-P03-B-A-regression" : extension ? "PollenInvader-P03-B" + (battleSection == "all" ? "" : "-" + battleSection) : battle ? "PollenInvader-P03-A3" + (battleSection == "all" ? "" : "-" + battleSection) : "PollenInvader-P03-A2";
        public static void Preview(float targetAngle = 90, string phase = "flight") => Start(true, targetAngle, phase);
        private static void Start(bool show, float targetAngle, string phase, bool fullBattle = false, string section = "all", bool followupTest = false, bool basicRegression = false)
        {
            if (fullBattle && !new[] { "all", "states", "terrain", "death", "field", "visits", "visuals" }.Contains(section)) throw new ArgumentException("Unknown battle section");
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) throw new InvalidOperationException("Wait for script import before verification.");
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("변경 중인 씬을 먼저 저장하세요.");
            definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(PollenInvaderSetup.DefinitionPath);
            Require(definition != null && definition.GetValidationError() == null, "valid P-03 source"); settings = (PollenInvaderPatternSettings)definition.pattern;
            battle = fullBattle; battleSection = section; scenes = EditorSceneManager.GetSceneManagerSetup(); preview = show; angle = targetAngle; previewPhase = phase; started = passed = false; results.Clear(); backups.Clear();
            regression = basicRegression; extension = followupTest; originalFollowupEnabled = settings.followup.enabled; settings.followup.enabled = extension;
            storage = Path.Combine(Path.GetTempPath(), "necrocis-p03-connection-" + Guid.NewGuid().ToString("N"));
            SaveService.UseStorageRootForTests(storage); Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error), error);
            oldFrameRate = Application.targetFrameRate; oldVsync = QualitySettings.vSyncCount;
            oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled; oldOptions = EditorSettings.enterPlayModeOptions; oldBackground = Application.runInBackground; oldTimeScale = Time.timeScale;
            EditorSettings.enterPlayModeOptionsEnabled = true; EditorSettings.enterPlayModeOptions = oldOptions | EnterPlayModeOptions.DisableDomainReload;
            Application.runInBackground = true; EditorApplication.playModeStateChanged += OnPlay; EditorApplication.update += Tick;
            deadline = EditorApplication.timeSinceStartup + (battle || extension ? 900 : 300); productionLung = AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>("Assets/_Project/Data/BiomeElites/LungBiomeEliteSpawnConfig.asset");
            productionLungEnabled = productionLung.enabled; productionLungCount = productionLung.monsters.Count;
            productionLung.enabled = false; // Temporary isolation; restore after Play without saving.
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Hub.unity"); EditorApplication.isPlaying = true;
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode) enteredFrame = Time.frameCount;
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlay; Restore();
            settings.followup.enabled = originalFollowupEnabled;
            productionLung.enabled = productionLungEnabled;
            EditorSettings.enterPlayModeOptionsEnabled = oldOptionsEnabled; EditorSettings.enterPlayModeOptions = oldOptions;
            Time.timeScale = oldTimeScale; Application.runInBackground = oldBackground;
            Application.targetFrameRate = oldFrameRate; QualitySettings.vSyncCount = oldVsync;
            SaveService.ResetStaticStateForTests(); DifficultyBalanceService.ResetForTests(); if (temporary != null) Object.DestroyImmediate(temporary);
            temporary = null; enemy = null; pattern = null; if (Directory.Exists(storage)) Directory.Delete(storage, true);
            if (!preview) { Directory.CreateDirectory("Logs"); File.WriteAllLines("Logs/" + StageLabel + "-results.txt", results); Debug.Log("[" + StageLabel + "] " + (passed ? "ALL PASS" : "FAIL")); }
            if (scenes != null && scenes.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(scenes);
        }
        private static void Tick()
        {
            if (EditorApplication.isPlaying && !started) Application.runInBackground = true;
            if (EditorApplication.timeSinceStartup > deadline) { deadline = double.PositiveInfinity; Fail(new TimeoutException("P-03 checks timed out")); return; }
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
            if (enemy != null && enemy.gameObject.activeSelf) enemy.ReleaseToPool(); Move(origin + Vector3.right * 5);
            var rule = PollenInvaderSetup.PreviewRule(definition);
            enemy = EnemyController.Acquire(null, "P03_ConnectionProbe", EnemyController.GetPoolArchetypeId(rule));
            enemy.Configure(null, rule, origin, origin); enemy.SuppressExperienceReward = true;
            enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 0); enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth, 200, true);
            pattern = enemy.GetComponent<PollenInvaderElitePattern>(); health.ResetHealth();
        }
        private static void Move(Vector3 position)
        {
            position.y = biome.GetGroundHeight(position) + groundOffset; player.SpawnAt(position); DontStarveCamera.Instance?.SnapToTarget();
            typeof(BiomeManager).GetMethod("UpdateChunks", Private).Invoke(biome, null);
            foreach (var spawner in Object.FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None)) { spawner.ReleaseSpawnedEnemies(); spawner.enabled = false; }
        }
        private static Vector3 Aim(float value) => new Vector3(Mathf.Cos(value * Mathf.Deg2Rad), 0, Mathf.Sin(value * Mathf.Deg2Rad));
        private static SpriteRenderer Body() => enemy.transform.Find("Visual").GetComponent<SpriteRenderer>();
        private static void Set(Object source, string property, float value)
        {
            if (!backups.ContainsKey(source)) backups.Add(source, EditorJsonUtility.ToJson(source));
            using var data = new SerializedObject(source); data.FindProperty(property).floatValue = value; data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Restore() { foreach (var item in backups) if (item.Key != null) { EditorJsonUtility.FromJsonOverwrite(item.Value, item.Key); EditorUtility.ClearDirty(item.Key); } backups.Clear(); }
        private static IEnumerator Wait(Func<bool> condition, float seconds, string label) { float until = Time.time + seconds; while (!condition()) { Require(Time.time < until, "timeout: " + label); yield return null; } }
        private static void Equal(float wanted, float actual, string label) => Require(Mathf.Abs(wanted - actual) < .005f, label + $": expected={wanted}, actual={actual}");
        private static void Require(bool condition, string label) { if (!condition) throw new InvalidOperationException(label); }
        private static void Pass(string value) { results.Add("PASS " + value); Debug.Log("[" + StageLabel + "] PASS " + value); }
        private static void Fail(Exception error) { results.Add("FAIL " + error); Debug.LogException(error); EditorApplication.isPaused = false; EditorApplication.isPlaying = false; }
    }
}
