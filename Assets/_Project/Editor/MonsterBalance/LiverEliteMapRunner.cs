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
    public static partial class LiverEliteMapRunner
    {
        private static bool combo, preview, started, passed, oldOptionsEnabled, oldBackground, sourceEnabled;
        private static string previewPhase;
        private static SceneSetup[] oldScenes;
        private static EnterPlayModeOptions oldOptions;
        private static float oldTimeScale, groundOffset;
        private static int enteredFrame;
        private static double deadline;
        private static string storage;
        private static GameDifficulty difficulty;
        private static BiomeEliteSpawnConfig source, temporaryConfig;
        private static MonsterDefinition emberDefinition, hangoverDefinition;
        private static PlayerController player;
        private static Health health;
        private static ProceduralBiomeBridge biome;
        private static BiomeEliteField field;
        private static readonly List<string> results = new List<string>();
        private static readonly Dictionary<Object, string> backups = new Dictionary<Object, string>();

        public static void RunCombination() => Start(true, false, GameDifficulty.Normal);
        public static void PreviewCombination(string phase = "flight") { previewPhase = phase; Start(true, true, GameDifficulty.Normal); }
        public static void PreviewField(string monsterId = "elite.h-03") { previewPhase = monsterId; Start(false, true, GameDifficulty.Normal); }
        public static void RunHardCombination() => Start(true, false, GameDifficulty.Hard);
        public static void RunNormal() => Start(false, false, GameDifficulty.Normal);
        public static void RunHard() => Start(false, false, GameDifficulty.Hard);

        private static void Start(bool combination, bool showPreview, GameDifficulty mode)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("변경 중인 씬을 먼저 저장하세요.");
            oldScenes = EditorSceneManager.GetSceneManagerSetup();
            combo = combination; preview = showPreview; difficulty = mode; started = passed = false; results.Clear(); backups.Clear();
            source = AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>(LiverEliteMapSetup.ConfigPath);
            emberDefinition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(InflammationEmberSetup.DefinitionPath);
            hangoverDefinition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(HangoverRemnantSetup.DefinitionPath);
            Require(source != null && emberDefinition != null && hangoverDefinition != null, "required source assets");
            Require(emberDefinition.GetValidationError() == null && hangoverDefinition.GetValidationError() == null, "valid approved patterns");
            sourceEnabled = source.enabled;
            if (combo) source.enabled = false; // In-memory isolation only; never save this test override.
            else Require(source.enabled && source.monsters.Count == 2, "production registration contains the two approved species");
            storage = Path.Combine(Path.GetTempPath(), "necrocis-h-map-" + Guid.NewGuid().ToString("N"));
            SaveService.UseStorageRootForTests(storage);
            Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error), error);
            if (mode == GameDifficulty.Hard)
            {
                SaveService.MarkFinalBossDefeated(); // Unlock only this temporary profile, following the existing save API.
                Require(SaveService.TryBeginNewGame(mode, out error), error);
            }
            oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled; oldOptions = EditorSettings.enterPlayModeOptions;
            oldTimeScale = Time.timeScale; oldBackground = Application.runInBackground;
            Application.runInBackground = true;
            EditorSettings.enterPlayModeOptionsEnabled = true; EditorSettings.enterPlayModeOptions = oldOptions | EnterPlayModeOptions.DisableDomainReload;
            EditorApplication.playModeStateChanged += OnPlayMode; EditorApplication.update += Tick;
            deadline = EditorApplication.timeSinceStartup + 600;
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Hub.unity"); EditorApplication.isPlaying = true;
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode) enteredFrame = Time.frameCount;
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlayMode;
            RestoreAssets(); source.enabled = sourceEnabled;
            EditorSettings.enterPlayModeOptionsEnabled = oldOptionsEnabled; EditorSettings.enterPlayModeOptions = oldOptions;
            Time.timeScale = oldTimeScale; Application.runInBackground = oldBackground;
            SaveService.ResetStaticStateForTests(); DifficultyBalanceService.ResetForTests();
            if (temporaryConfig != null) Object.DestroyImmediate(temporaryConfig);
            temporaryConfig = null;
            if (Directory.Exists(storage)) Directory.Delete(storage, true);
            if (!preview)
            {
                Directory.CreateDirectory("Logs");
                File.WriteAllLines("Logs/Liver-H-MAP-" + (combo ? "combination-" : "") + difficulty + "-results.txt", results);
                Debug.Log("[H-MAP] " + (passed ? "ALL PASS" : "FAIL"));
            }
            if (oldScenes != null && oldScenes.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(oldScenes);
        }

        private static void Tick()
        {
            if (EditorApplication.isPlaying && !started) Application.runInBackground = true;
            if (EditorApplication.timeSinceStartup > deadline)
            { Fail(new TimeoutException("H-MAP test timeout")); deadline = double.PositiveInfinity; return; }
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
                yield return new WaitForEndOfFrame(); deadline = double.PositiveInfinity;
                EditorApplication.isPaused = true;
                Selection.activeGameObject = field.gameObject;
                Debug.Log("[H-MAP] Temporary two-species combat preview. Production uses one of each, at least 18m apart.");
            }
            else EditorApplication.isPlaying = false;
        }

        private static IEnumerator Checks()
        {
            GameManager.Instance.EnterBiome(BiomeType.Liver);
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LIVER); yield return null;
            BindScene();
            if (combo) yield return CombinedCombat();
            else yield return ProductionChecks();
        }

        private static void BindScene()
        {
            player = PlayerController.Instance; health = player.HealthComponent;
            biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>(); field = biome.GetComponent<BiomeEliteField>();
            groundOffset = player.transform.position.y - biome.GetGroundHeight(player.transform.position);
            Equal(-2, groundOffset, "actual gameplay player ground offset");
            if (!preview) PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 1000, true);
            {
                // Isolate automated controls and normal spawns, retaining the real legacy kill-trigger spawner.
                ((IList)typeof(ProceduralBiomeBridge).GetField("normalEnemyRules", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(biome)).Clear();
                typeof(InputManager).GetMethod("SetActionsEnabled", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(InputManager.Instance, new object[] { false });
            }
        }

        private static void Move(Vector3 position)
        {
            position.y = biome.GetGroundHeight(position) + groundOffset;
            player.SpawnAt(position); DontStarveCamera.Instance?.SnapToTarget();
            typeof(BiomeManager).GetMethod("UpdateChunks", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(biome, null);
            foreach (EnemySpawner spawner in Object.FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None))
            { spawner.ReleaseSpawnedEnemies(); spawner.enabled = false; }
            field.Refresh(position);
        }

        private static IEnumerator WalkTo(Vector3 destination, float seconds)
        {
            float until = Time.time + seconds;
            while (true)
            {
                Vector3 delta = destination - player.transform.position; delta.y = 0;
                if (delta.magnitude < .08f) yield break;
                Require(Time.time < until, "walk timeout: " + player.transform.position + " -> " + destination);
                player.TryMoveByWorld(delta.normalized * Mathf.Min(delta.magnitude, player.MoveSpeed * Time.fixedDeltaTime));
                yield return new WaitForFixedUpdate();
            }
        }
        private static IEnumerator Wait(Func<bool> condition, float seconds, string label)
        {
            float until = Time.time + seconds;
            while (!condition()) { Require(Time.time < until, "timeout: " + label); yield return null; }
        }
        private static void PrivateSet(object target, string name, object value) => target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
        private static void Face(Vector3 direction)
        { PrivateSet(player, "movement", Vector3.zero); PrivateSet(player, "lastMoveDirection", direction); }
        private static void Set(Object asset, string path, float value)
        {
            if (!backups.ContainsKey(asset)) backups.Add(asset, EditorJsonUtility.ToJson(asset));
            using var data = new SerializedObject(asset); var property = data.FindProperty(path);
            if (property.propertyType == SerializedPropertyType.Integer) property.intValue = Mathf.RoundToInt(value); else property.floatValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void RestoreAssets()
        {
            foreach (var entry in backups) if (entry.Key != null) { EditorJsonUtility.FromJsonOverwrite(entry.Value, entry.Key); EditorUtility.ClearDirty(entry.Key); }
            backups.Clear();
        }
        private static float Distance(Vector3 a, Vector3 b) { a.y = b.y = 0; return Vector3.Distance(a, b); }
        private static void Equal(float a, float b, string label) => Require(Mathf.Abs(a - b) < .002f, label + $": expected {a}, actual {b}");
        private static void Require(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); }
        private static void Pass(string label) { results.Add("PASS " + label); Debug.Log("[H-MAP] PASS " + label); }
        private static void Fail(Exception error)
        { passed = false; results.Add("FAIL " + error); Debug.LogException(error); EditorApplication.isPaused = false; EditorApplication.isPlaying = false; }
    }
}
