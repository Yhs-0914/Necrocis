using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static class DirectionalPresentationRunner
    {
        private static IEnumerator routine;
        private static SceneSetup[] oldScenes;
        private static bool oldOptionsEnabled, oldBackground, preview, started;
        private static EnterPlayModeOptions oldOptions;
        private static float oldTimeScale;
        private static double deadline;
        private static int lastFrame;
        private static string assetBackup;
        private static EnemyDirectionalPresentation data;
        private static GasSacPresentation art;
        private static Camera camera;
        private static readonly List<string> results = new List<string>();
        public const string Report = "Logs/MonsterPresentation-C04-results.txt";

        public static void Run() => Start(false);
        public static void Preview() => Start(true);

        private static void Start(bool previewOnly)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("먼저 Play Mode를 종료하세요.");
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("변경 중인 씬을 먼저 저장하세요.");
            data = AssetDatabase.LoadAssetAtPath<EnemyDirectionalPresentation>(GasSacDirectionalImport.AssetPath);
            art = AssetDatabase.LoadAssetAtPath<GasSacPresentation>(GasSacSetup.PresentationPath);
            if (data == null || data.GetValidationError() != null) throw new InvalidOperationException("C-04 방향 미술 가져오기가 필요합니다.");
            assetBackup = EditorJsonUtility.ToJson(data);
            oldScenes = EditorSceneManager.GetSceneManagerSetup();
            oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled; oldOptions = EditorSettings.enterPlayModeOptions;
            oldBackground = Application.runInBackground; oldTimeScale = Time.timeScale;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = oldOptions | EnterPlayModeOptions.DisableDomainReload;
            preview = previewOnly; started = false; routine = null; lastFrame = -1; results.Clear();
            deadline = EditorApplication.timeSinceStartup + 90;
            EditorApplication.playModeStateChanged += OnPlayMode;
            EditorApplication.update += Tick;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlayMode;
            (routine as IDisposable)?.Dispose(); routine = null;
            if (!preview && data != null && !string.IsNullOrEmpty(assetBackup))
            { EditorJsonUtility.FromJsonOverwrite(assetBackup, data); EditorUtility.SetDirty(data); AssetDatabase.SaveAssets(); }
            EditorSettings.enterPlayModeOptionsEnabled = oldOptionsEnabled; EditorSettings.enterPlayModeOptions = oldOptions;
            AssetDatabase.SaveAssets();
            Time.timeScale = oldTimeScale; Application.runInBackground = oldBackground;
            if (oldScenes != null && oldScenes.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(oldScenes);
            if (!preview) { Directory.CreateDirectory("Logs"); File.WriteAllLines(Report, results); Debug.Log("[C-04] " + string.Join("\n", results)); }
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isPaused) return;
            if (Time.frameCount == lastFrame) return;
            lastFrame = Time.frameCount;
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("C-04 검증 제한 시간 초과");
                if (!started)
                {
                    started = true; Time.timeScale = 1; Application.runInBackground = true;
                    camera = new GameObject("C04_Camera").AddComponent<Camera>(); camera.tag = "MainCamera";
                    camera.gameObject.AddComponent<AudioListener>();
                    camera.orthographic = true; camera.orthographicSize = 3.6f;
                    camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.23f, .16f, .19f);
                    camera.transform.position = new Vector3(0, 8, -10); camera.transform.LookAt(new Vector3(0, .7f, 0));
                    routine = preview ? PreviewFlow() : Checks();
                }
                if (!routine.MoveNext()) { results.Add("PASS complete"); EditorApplication.isPlaying = false; }
            }
            catch (Exception error)
            { results.Add("FAIL " + error); Debug.LogException(error); EditorApplication.isPlaying = false; }
        }

        private static EnemySpawnRuleConfig Rule(string name = "C04_Gas") => new EnemySpawnRuleConfig {
            name = name, poissonSalt = 94004, maxHealth = 30, attackDamage = 2, moveSpeed = 0, expReward = 0,
            enableContactDamage = false, idleSprites = new[] { art.idle }, deathSprites = art.deathFrames,
            deathAnimationSpeed = art.deathFrameSeconds, scale = Vector3.one, useBillboard = true, useYSort = true };

        private static EnemyController Spawn(Vector3 position, bool bind = true, EnemySpawnRuleConfig rule = null)
        {
            rule = rule ?? Rule();
            EnemyController enemy = EnemyController.Acquire(null, rule.name, EnemyController.GetPoolArchetypeId(rule));
            enemy.Configure(null, rule, position, position); enemy.SuppressExperienceReward = true;
            enemy.SetAiSuppressed(true); enemy.AlignPatternFeetToGround();
            if (bind) enemy.BindPatternDirections(data);
            enemy.SetPatternFrame(art.idle);
            return enemy;
        }

        private static SpriteRenderer Renderer(EnemyController enemy) => enemy.transform.Find("Visual").GetComponent<SpriteRenderer>();
        private static Vector3 Direction(float angle) => new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
        private static void Check(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); results.Add("PASS " + message); }

        private static IEnumerator Checks()
        {
            Check(data.frames.Length == 14 && data.GetValidationError() == null, "14 unique source keys; front/back and death complete");
            var sprites = data.frames.SelectMany(f => new[] { f.front, f.back }).ToArray();
            Check(sprites.Length == 28 && sprites.Distinct().Count() == 28, "28 distinct new sprites");
            foreach (var group in sprites.GroupBy(s => AssetDatabase.GetAssetPath(s.texture)))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(group.Key);
                Check(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled
                    && importer.textureCompression == TextureImporterCompression.Uncompressed, "Point/no mip/uncompressed " + Path.GetFileName(group.Key));
            }
            Check(sprites.All(s => Mathf.Abs(s.pivot.x - Mathf.Round(s.pivot.x)) < .001f
                && Mathf.Abs(s.pivot.y - Mathf.Round(s.pivot.y)) < .001f), "All 28 pivots are integer pixels");
            EnemyController enemy = Spawn(Vector3.zero); yield return null;
            var renderer = Renderer(enemy);
            foreach (var facing in new[] { EnemyFacing.Right, EnemyFacing.Back, EnemyFacing.Left, EnemyFacing.Front })
            {
                enemy.SetPatternFacing(Direction((int)facing * 45)); enemy.SetPatternFrame(art.inflationFrames[3]);
                Check(enemy.PatternFacing == facing && renderer.sprite == data.Capture().Resolve(art.inflationFrames[3], facing)
                    && renderer.flipX == data.Capture().Flip(facing), "Live renderer " + facing + ": frame/flip");
            }
            enemy.SetPatternFacing(Vector3.right); enemy.SetPatternFacing(Direction(46));
            Check(enemy.PatternFacing == EnemyFacing.Right, "Boundary jitter retains right at 46 degrees");
            enemy.SetPatternFacing(Direction(54));
            Check(enemy.PatternFacing == EnemyFacing.Back, "Boundary crosses at 54 degrees");
            enemy.SetPatternFacing(Direction(46)); enemy.SetPatternFacing(Vector3.zero);
            Check(enemy.PatternFacing == EnemyFacing.Back && renderer.sprite == data.frames[3].back, "Reverse hysteresis/zero vector preserve current pose");
            enemy.CommitPatternFacing(Vector3.back); enemy.SetPatternFacing(Vector3.forward); enemy.SetPatternFrame(art.inflationFrames[5]);
            Check(enemy.PatternFacing == EnemyFacing.Front && renderer.sprite == data.frames[5].front, "Committed facing locked while pose advances");
            enemy.ReleasePatternFacing(); enemy.SetPatternFacing(Vector3.forward);
            Check(enemy.PatternFacing == EnemyFacing.Back && renderer.sprite == data.frames[5].back, "Unlock changes direction without restarting pose");
            enemy.SetPatternFacing(Vector3.right); enemy.BindPatternDirections(data); enemy.SetPatternFrame(art.idle);
            Check(!renderer.flipX && renderer.sprite == data.frames[0].front, "Rebind clears previous side flip for front view");
            Quaternion rotation = camera.transform.rotation;
            camera.transform.rotation = Quaternion.Euler(40, 90, 0);
            enemy.SetPatternFacing(Vector3.ProjectOnPlane(camera.transform.right, Vector3.up));
            Check(enemy.PatternFacing == EnemyFacing.Right, "Rotated camera screen-right mapping");
            enemy.SetPatternFacing(Vector3.ProjectOnPlane(camera.transform.up, Vector3.up));
            Check(enemy.PatternFacing == EnemyFacing.Back, "Rotated camera screen-up mapping");
            camera.transform.rotation = rotation;
            var so = new SerializedObject(data);
            Sprite original = data.frames[0].front;
            so.FindProperty("frames").GetArrayElementAtIndex(0).FindPropertyRelative("front").objectReferenceValue = data.frames[0].back;
            so.FindProperty("frontOrigin").vector2Value = new Vector2(.25f, .1f); so.ApplyModifiedPropertiesWithoutUndo();
            enemy.SetPatternFacing(Vector3.back); enemy.SetPatternFrame(art.idle);
            Check(renderer.sprite == original, "Running snapshot is not silently changed by authoring edits");
            enemy.BindPatternDirections(data); enemy.SetPatternFacing(Vector3.back); enemy.SetPatternFrame(art.idle);
            Vector3 expectedOrigin = renderer.transform.TransformPoint(new Vector3(.25f, .1f, 0));
            Check(renderer.sprite == data.frames[0].back && Vector3.Distance(enemy.GetPatternVisualOrigin(), expectedOrigin) < .001f,
                "Serialized Inspector frame/origin edit reaches actual renderer after explicit rebind");
            EditorJsonUtility.FromJsonOverwrite(assetBackup, data); enemy.BindPatternDirections(data);
            var invalid = Object.Instantiate(data); invalid.frames[0].front = null;
            Check(invalid.GetValidationError() != null, "Missing front frame reported");
            bool rejected = false; try { invalid.Capture(); } catch (InvalidOperationException) { rejected = true; }
            Check(rejected, "Invalid presentation cannot bind silently"); Object.Destroy(invalid);
            var missingDeath = Object.Instantiate(data); missingDeath.frames = missingDeath.frames.Take(13).ToArray();
            rejected = false; try { enemy.BindPatternDirections(missingDeath); } catch (InvalidOperationException) { rejected = true; }
            Check(rejected && enemy.HasPatternDirections, "Missing death key rejected before replacing live binding"); Object.Destroy(missingDeath);
            var eight = Object.Instantiate(data); eight.mode = EnemyDirectionMode.Eight;
            Check(eight.GetValidationError() != null, "8-way requires actual diagonal banks");
            foreach (var f in eight.frames) { f.frontDiagonal = f.front; f.backDiagonal = f.back; }
            var snapshot = eight.Capture();
            Check(Enumerable.Range(0, 8).All(i => snapshot.Select(Direction(i * 45), camera, EnemyFacing.Front, false) == (EnemyFacing)i),
                "8-way selector supports all sectors with test-only references"); Object.Destroy(eight);
            foreach (EnemyFacing facing in new[] { EnemyFacing.Front, EnemyFacing.Back, EnemyFacing.Left, EnemyFacing.Right })
            {
                enemy.BindPatternDirections(data); enemy.SetPatternFacing(Direction((int)facing * 45));
                var seen = new HashSet<Sprite>(); int callbacks = 0;
                enemy.PlayDeathAnimation(() => callbacks++);
                float until = Time.unscaledTime + 2;
                while (enemy.IsDeathAnimPlaying && Time.unscaledTime < until)
                {
                    seen.Add(renderer.sprite); enemy.SetPatternFacing(Direction(((int)facing + 4) * 45));
                    enemy.SetPatternFrame(art.idle); yield return null;
                }
                Sprite[] expected = art.deathFrames.Select(f => data.Capture().Resolve(f, facing)).ToArray();
                Check(callbacks == 1 && expected.All(seen.Contains) && renderer.sprite == expected[5]
                    && enemy.PatternFacing == facing, "Death all 6 frames / callback once / facing retained: " + facing);
            }
            enemy.CommitPatternFacing(Vector3.back); enemy.TakeDamage(10000);
            float deathEnd = Time.unscaledTime + 2;
            while (enemy.gameObject.activeSelf && Time.unscaledTime < deathEnd) yield return null;
            Check(!enemy.gameObject.activeSelf && !enemy.HasPatternDirections && !enemy.IsPatternFacingLocked, "Real death returns to pool and clears binding/lock");
            EnemyController reused = Spawn(Vector3.right, false);
            Check(reused == enemy && !reused.HasPatternDirections && reused.PatternFacing == EnemyFacing.Front && !reused.IsDead,
                "Pooled actor resets facing/binding and health on next spawn");
            reused.SetPatternFacing(Vector3.left); Check(!Renderer(reused).flipX, "Legacy left-authored side flip retained without binding");
            reused.SetPatternFacing(Vector3.right); Check(Renderer(reused).flipX, "Legacy opposite-side flip retained without binding");
            reused.ReleaseToPool();
            foreach (string path in new[] { GasSacSetup.PatternPath, HardenedResidueSetup.PatternPath, InflammationEmberSetup.PatternPath })
            {
                var setting = AssetDatabase.LoadAssetAtPath<MonsterPatternSettings>(path);
                EnemyController legacy = Spawn(Vector3.zero, false, Rule(Path.GetFileNameWithoutExtension(path)));
                setting.Attach(legacy); yield return null;
                Check(legacy.HasPatternDirections == (setting is GasSacPatternSettings || setting is HardenedResiduePatternSettings)
                    && Renderer(legacy).sprite != null && legacy.Config.deathSprites.Length == 6,
                    "Existing pattern initialization/death compatibility: " + setting.name);
                legacy.ReleaseToPool();
            }
        }

        private static IEnumerator PreviewFlow()
        {
            deadline = double.PositiveInfinity;
            var enemies = new List<EnemyController>();
            EnemyFacing[] directions = { EnemyFacing.Left, EnemyFacing.Front, EnemyFacing.Back, EnemyFacing.Right };
            for (int i = 0; i < 4; i++)
            {
                var enemy = Spawn(new Vector3(-4.2f + i * 2.8f, 0, 0), true, Rule("C04_" + directions[i]));
                enemy.SetPatternFacing(Direction((int)directions[i] * 45)); enemies.Add(enemy);
            }
            Selection.activeObject = data;
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            while (true)
            {
                for (int i = 0; i < 8; i++)
                {
                    foreach (var enemy in enemies) enemy.SetPatternFrame(i == 7 ? art.deflated : art.inflationFrames[i]);
                    float end = Time.unscaledTime + (i == 0 || i == 7 ? .9f : .25f);
                    while (Time.unscaledTime < end) yield return null;
                }
                foreach (var enemy in enemies) enemy.PlayDeathAnimation(null);
                float deathEnd = Time.unscaledTime + 1.5f;
                while (Time.unscaledTime < deathEnd) yield return null;
            }
        }
    }
}
