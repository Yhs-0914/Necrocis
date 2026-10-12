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
    public static partial class GasSacPlayModeRunner
    {
        private static bool preview, orbChecks, deathChecks, hitAudit, directionChecks, started, passed, oldOptionsEnabled;
        private static EnterPlayModeOptions oldOptions;
        private static float oldTimeScale;
        private static bool oldRunInBackground;
        private static double deadline;
        private static int enteredFrame;
        private static int stage3Mode;
        private static string storage;
        private static MonsterDefinition definition;
        private static GasSacPatternSettings settings;
        private static PlayerController player;
        private static Health health;
        private static EnemyController enemy;
        private static GasSacElitePattern pattern;
        private static BiomeEliteSpawnConfig fieldConfig;
        private static BiomeEliteField field;
        private static ProceduralBiomeBridge biome;
        private static BiomeElitePlacement point;
        private static Vector3 origin;
        private static float playerGroundOffset;
        private static readonly Dictionary<Object, string> backups = new Dictionary<Object, string>();
        private static readonly List<string> results = new List<string>();

        [MenuItem("Necrocis/Balance/Run I-01-A Gas Sac Checks")]
        public static void Run() => Start(false);

        [MenuItem("Necrocis/Balance/Preview I-01-A Gas Sac")]
        public static void Preview() => Start(true);

        [MenuItem("Necrocis/Balance/Run I-01-B Gas Sac Checks")]
        public static void RunOrb() => Start(false, true);

        [MenuItem("Necrocis/Balance/Preview I-01-B Gas Sac")]
        public static void PreviewOrb() => Start(true, true);

        [MenuItem("Necrocis/Balance/Run I-01 Gas Sac Death Checks")]
        public static void RunDeath() => Start(false, true, true);

        [MenuItem("Necrocis/Balance/Preview I-01 Gas Sac Death")]
        public static void PreviewDeath() => Start(true, true, true);

        [MenuItem("Necrocis/Balance/Audit I-01 Gas Sac Hit Ranges")]
        public static void RunHitAudit() => Start(false, true, false, true);

        private static void Start(bool previewOnly, bool includeOrb = false, bool includeDeath = false, bool auditHits = false, bool checkDirections = false, int stage3 = 0)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(GasSacSetup.DefinitionPath);
            Require(definition != null && definition.GetValidationError() == null, "Gas sac definition/pattern assets must be valid");
            settings = (GasSacPatternSettings)definition.pattern;
            preview = previewOnly; started = passed = false; results.Clear(); backups.Clear();
            orbChecks = includeOrb;
            deathChecks = includeDeath;
            hitAudit = auditHits;
            directionChecks = checkDirections;
            stage3Mode = stage3;
            SetBool(settings, "orbEnabled", includeOrb); // A suite explicitly verifies the B-off fallback; restored on exit.
            SuspendProductionField();
            storage = Path.Combine(Path.GetTempPath(), "necrocis-gas-sac-" + Guid.NewGuid().ToString("N"));
            SaveService.UseStorageRootForTests(storage);
            Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error), error);
            oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled; oldOptions = EditorSettings.enterPlayModeOptions;
            oldTimeScale = Time.timeScale;
            oldRunInBackground = Application.runInBackground;
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
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlayMode;
            RestoreAssets();
            ResumeProductionField();
            EditorSettings.enterPlayModeOptionsEnabled = oldOptionsEnabled; EditorSettings.enterPlayModeOptions = oldOptions;
            AssetDatabase.SaveAssets();
            Time.timeScale = oldTimeScale;
            Application.runInBackground = oldRunInBackground;
            SaveService.ResetStaticStateForTests(); DifficultyBalanceService.ResetForTests();
            if (fieldConfig != null) Object.DestroyImmediate(fieldConfig);
            fieldConfig = null; enemy = null; pattern = null;
            if (Directory.Exists(storage)) Directory.Delete(storage, true);
            if (!preview)
            {
                Directory.CreateDirectory("Logs"); File.WriteAllLines(stage3Mode == 1 ? "Logs/GasSac-D3-battle-results.txt" : stage3Mode == 2 ? "Logs/GasSac-D3-terrain-results.txt" : directionChecks ? "Logs/GasSac-D2-results.txt" : hitAudit ? "Logs/GasSac-HitAudit-results.txt" : deathChecks ? "Logs/GasSac-Death-results.txt" : orbChecks ? "Logs/GasSac-I01B-results.txt" : "Logs/GasSac-I01A-results.txt", results);
                Debug.Log(LogPrefix + (passed ? "ALL PASS" : "FAIL"));
            }
            if (Application.isBatchMode) EditorApplication.Exit(passed ? 0 : 1);
        }

        private static void Tick()
        {
            if (EditorApplication.timeSinceStartup > deadline) { Fail(new TimeoutException("Gas sac test timed out")); deadline = double.PositiveInfinity; return; }
            if (started || !EditorApplication.isPlaying || Time.frameCount - enteredFrame < 15) return;
            player = PlayerController.Instance;
            if (player == null || player.HealthComponent == null) return;
            health = player.HealthComponent;
            started = true; Time.timeScale = 1;
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
                yield return new WaitForEndOfFrame(); // Present the complete body/player frame before pausing the preview.
                deadline = double.PositiveInfinity;
                EditorApplication.isPaused = true;
                Selection.activeGameObject = enemy.gameObject;
                Debug.Log(directionChecks ? "[GasSac-D2] Directional A/B preview paused in the actual Intestine biome. Temporary save only."
                    : deathChecks ? "[GasSac-Death] Preview paused mid-collapse. Unpause to finish death and pool release. Temporary save; scripted preview kill suppresses XP only to avoid opening level-up UI."
                    : orbChecks ? "[GasSac-I01B] Preview ready: body windup without ground UI, single orb, B recovery. Unpause and sidestep; approach for A. Temporary saves only."
                    : "[GasSac-I01A] Preview ready at windup. Unpause to leave the ring, then reapproach the deflated body. B disabled in this isolated preview only.");
            }
            else EditorApplication.isPlaying = false;
        }

        private static void Fail(Exception error)
        {
            passed = false; results.Add("FAIL " + error); Debug.LogException(error);
            RestoreAssets(); EditorApplication.isPlaying = false;
        }

        private static IEnumerator Checks()
        {
            CheckImports();
            GameManager.Instance.EnterBiome(BiomeType.Intestine);
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_INTESTINE);
            yield return null;
            biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>();
            // Preserve the actual gameplay spawn's root offset. The sprite/capsule already includes its local height.
            playerGroundOffset = player.transform.position.y - biome.GetGroundHeight(player.transform.position);
            field = biome.GetComponent<BiomeEliteField>();
            fieldConfig = ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>();
            int placementCount = deathChecks && !preview ? 2 : 1;
            fieldConfig.minimumCount = fieldConfig.maximumCount = placementCount; fieldConfig.clearanceCells = 4;
            fieldConfig.monsters.Add(GasSacSetup.PreviewRule(definition));
            field.Configure(biome, fieldConfig, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Require(field.Plan != null && field.Plan.placements.Count == placementCount, "requested gas sac test placements");
            point = field.Plan.placements[0];
            origin = biome.GridToWorldWithHeight(point.x, point.y);
            MovePlayer(8);
            typeof(BiomeManager).GetMethod("UpdateChunks", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(biome, null);
            yield return new WaitForSeconds(.25f);
            foreach (EnemySpawner spawner in Object.FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None))
            { spawner.ReleaseSpawnedEnemies(); spawner.enabled = false; }
            foreach (EnemyController other in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
                if (other.Balance == null || other.Balance.Current.MonsterId != definition.monsterId) other.ReleaseToPool();
            if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false; // Isolated preview only; no asset/production changes.
            field.Refresh(player.transform.position);
            enemy = field.Spawners.Single(s => s.Placement.spawnId == point.spawnId).ActiveEnemy; pattern = enemy.GetComponent<GasSacElitePattern>();
            Require(pattern != null && enemy.IsAiSuppressed, "definition automatically attaches the exclusive A controller");
            Equal(0, enemy.transform.Find("Visual").localPosition.y, "foot-pivot sprite is grounded at the telegraph center");
            Require(pattern.GroundShadowObject != null && pattern.GroundShadowObject.activeInHierarchy, "ground contact shadow exists");
            if (stage3Mode == 1) { yield return DirectionBattleChecks(); yield break; }
            if (stage3Mode == 2) { yield return GasTerrainChecks(); yield break; }
            if (directionChecks) { yield return DirectionChecks(); yield break; }
            if (hitAudit) { yield return HitRangeAudit(); yield break; }
            if (deathChecks) { yield return DeathChecks(); yield break; }
            if (orbChecks) { yield return OrbChecks(); yield break; }
            if (preview)
            {
                MovePlayer(2.3f);
                yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 4, "preview windup");
                yield return new WaitForSeconds(settings.windupSeconds * .4f);
                yield break;
            }
            Pass("real biome field spawner attaches I-01 A from the definition, without a kill trigger");
            field.enabled = false;
            enemy = null;
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 60, true);
            yield return FacingAndAreaChecks();

            Spawn(); MovePlayer(2.3f); health.ResetHealth();
            float healthBefore = health.CurrentHealth;
            yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 4, "windup starts");
            Vector3 locked = enemy.transform.position;
            LineRenderer ring = pattern.TelegraphObject.GetComponent<LineRenderer>();
            foreach (int index in new[] { 0, 24, 48, 72 })
                Equal(settings.burstRadius, ring.GetPosition(index).magnitude, "visible ring radius matches damage radius");
            yield return Wait(() => pattern.Phase == GasSacPhase.Recovery, 4, "recovery starts");
            Equal(3, healthBefore - health.CurrentHealth, "base attack 2 × coefficient 1.5, one burst only");
            Equal(0, PlanarDistance(locked, enemy.transform.position), "no locomotion during windup/burst");
            Require(pattern.BurstCount == 1 && pattern.TelegraphObject == null, "single hit and no lingering danger zone");
            float enemyHealth = enemy.Stats.CurrentHealth;
            MovePlayer(.6f); health.ResetHealth(); enemy.TakeDamage(5);
            Equal(5, enemyHealth - enemy.Stats.CurrentHealth, "recovery remains vulnerable to retaliation");
            Require(enemy.GetComponent<EnemyContactDamage>().TryApplyTo(player), "body contact remains enabled during recovery");
            yield return new WaitForSeconds(.2f);
            Equal(59, health.CurrentHealth, "one body contact hit; invincibility prevents per-frame duplicates");
            Pass("fixed-radius tell → one 3-HP burst → deflated recovery; movement stopped, hit once, retaliation allowed, body contact damage 1");

            Spawn(); MovePlayer(2.3f); health.ResetHealth(); healthBefore = health.CurrentHealth;
            yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 4, "walk escape windup");
            while (pattern.Phase == GasSacPhase.Windup)
            {
                if (PlanarDistance(player.transform.position, enemy.transform.position) < settings.burstRadius + .35f)
                    player.TryMoveByWorld(Vector3.right * player.MoveSpeed * Time.deltaTime);
                yield return null;
            }
            Require(PlanarDistance(player.transform.position, enemy.transform.position) > settings.burstRadius, "walking crosses the visible boundary before burst");
            Equal(healthBefore, health.CurrentHealth, "walk escape takes zero damage");
            Pass("actual player walking clears the radius during the 1.3-second tell and takes zero damage");

            Spawn(); MovePlayer(2.3f); health.ResetHealth(); healthBefore = health.CurrentHealth;
            yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 4, "dash escape windup");
            yield return new WaitForSeconds(settings.windupSeconds * .65f);
            typeof(PlayerController).GetField("lastMoveDirection", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(player, Vector3.right);
            var dash = (IEnumerator)typeof(PlayerController).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(player, null);
            player.StartCoroutine(dash);
            yield return Wait(() => pattern.Phase == GasSacPhase.Recovery, 3, "dash escape burst");
            Equal(healthBefore, health.CurrentHealth, "actual dash escape takes zero damage");
            Require(PlanarDistance(player.transform.position, enemy.transform.position) > settings.burstRadius, "dash exits the ring");
            Pass("actual player dash exits the telegraph and receives zero burst damage");

            Spawn(); MovePlayer(2.3f); health.ResetHealth(); healthBefore = health.CurrentHealth;
            yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 4, "kill cancellation windup");
            var cancelled = pattern; var marker = pattern.TelegraphObject;
            enemy.SuppressExperienceReward = true; enemy.TakeDamage(10000);
            yield return new WaitForSeconds(settings.windupSeconds + .2f);
            Require(cancelled.BurstCount == 0 && marker == null && cancelled.Phase == GasSacPhase.Inactive, "death cancels queued burst and marker");
            Require(!cancelled.GroundShadowObject.activeInHierarchy, "death removes ground shadow");
            Equal(healthBefore, health.CurrentHealth, "no delayed damage after death");
            Pass("death during tell cancels the burst and removes VFX; no delayed hit");

            Spawn(); MovePlayer(2.3f); health.ResetHealth(); healthBefore = health.CurrentHealth;
            yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 4, "release cancellation windup");
            marker = pattern.TelegraphObject; var lifetime = enemy.GetComponent<EnemyPatternLifetime>(); uint token = enemy.SpawnGeneration;
            Spawn(); // Returns the old actor to the pool, then captures a new spawn with the player far away.
            yield return new WaitForSeconds(settings.windupSeconds + .2f);
            Require(marker == null && pattern.BurstCount == 0 && !lifetime.IsCurrent(token), "pool reuse cancels the previous tell/generation");
            Require(enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, "no retained marker registrations");
            Equal(healthBefore, health.CurrentHealth, "no delayed damage after pooling");
            Pass("release during tell and immediate pool reuse leave no old burst, marker or generation callback");

            GasSacElitePattern frozen = pattern;
            Set(settings, "windupSeconds", .6f); Set(settings, "burstRadius", 2); Set(settings, "triggerDistance", 1.5f);
            Set(settings, "recoverySeconds", .4f); Set(settings, "rearmSeconds", .6f); Set(settings, "spawnGraceSeconds", 0);
            Set(definition, "statSets.Array.data[0].attackPower", 4);
            int damageIndex = definition.patternDamage.FindIndex(p => p.id == GasSacPatternSettings.DamageId);
            Set(definition, $"patternDamage.Array.data[{damageIndex}].coefficient", 2);
            var profile = DifficultyBalanceService.GetProfile(GameDifficulty.Normal);
            Set(profile, "elites.outgoingDamage", 1.5f); Set(profile, "elites.attackCooldown", .5f);
            Equal(1.3f, frozen.WindupDuration, "already spawned tell remains frozen");
            Equal(3.25f, frozen.BurstRadius, "already spawned radius remains frozen");
            Spawn(); MovePlayer(1.4f); health.ResetHealth(); healthBefore = health.CurrentHealth;
            Equal(.6f, pattern.WindupDuration, "new spawn uses Inspector windup");
            Equal(2, pattern.BurstRadius, "new spawn uses Inspector radius");
            float windupStart = Time.time;
            yield return Wait(() => pattern.Phase == GasSacPhase.Windup, 1, "edited tell");
            windupStart = Time.time;
            yield return Wait(() => pattern.Phase == GasSacPhase.Recovery, 2, "edited burst");
            Equal(12, healthBefore - health.CurrentHealth, "Inspector attack 4 × difficulty 1.5 × coefficient 2 = 12 once");
            Require(Time.time - windupStart >= .55f, "difficulty cooldown does not shorten windup");
            yield return Wait(() => pattern.Phase == GasSacPhase.Ready, 2, "edited recovery");
            Require(pattern.NextReadyTime - Time.time > .2f && pattern.NextReadyTime - Time.time <= .301f, "one scaled rearm after recovery (.6 × .5)");
            Require(!pattern.TryBeginBurst(), "cannot bypass rearm");
            yield return Wait(() => pattern.BurstCount == 2, 3, "second cycle");
            yield return Wait(() => pattern.Phase == GasSacPhase.Ready, 2, "second recovery ends");
            Require(enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, "repeated cycles do not accumulate VFX ownership");
            Pass("Serialized Inspector edit changes new tell/radius/recovery; actual damage 12 once; rearm 0.3s; existing actor unchanged; repeated cycles clean");

            if (enemy != null) enemy.ReleaseToPool(); enemy = null; RestoreAssets();
            MovePlayer(8); field.enabled = true; field.Refresh(player.transform.position);
            enemy = field.Spawners.Single().ActiveEnemy;
            int xp = 0; Action<int> record = amount => xp += amount;
            LevelUpManager.OnExpGained += record;
            try { enemy.TakeDamage(10000); enemy.GrantExp(); }
            finally { LevelUpManager.OnExpGained -= record; }
            Equal(50, xp, "actual gas sac reward once");
            Require(SaveService.IsBiomeEliteDefeated(point.spawnId), "gas sac field defeat is persisted");
            Pass("actual I-01 field death saves its spawn ID and grants base XP 50 once");
        }

        private static void CheckImports()
        {
            float bodyPpu = settings.presentation.idle.pixelsPerUnit;
            foreach (Sprite sprite in settings.presentation.inflationFrames.Concat(new[] { settings.presentation.idle, settings.presentation.deflated }))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite));
                Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "pixel import settings");
                Equal(bodyPpu, sprite.pixelsPerUnit, "shared body PPU");
                Equal(Mathf.Round(sprite.pivot.x), sprite.pivot.x, "integer x pivot");
                Equal(Mathf.Round(sprite.pivot.y), sprite.pivot.y, "integer ground pivot");
            }
        }

        private static void Spawn()
        {
            if (enemy != null && enemy.gameObject.activeSelf) enemy.ReleaseToPool();
            MovePlayer(8);
            EnemySpawnRuleConfig rule = GasSacSetup.PreviewRule(definition);
            enemy = EnemyController.Acquire(null, "GasSac_A_Test", EnemyController.GetPoolArchetypeId(rule));
            enemy.Configure(null, rule, origin, origin); pattern = enemy.GetComponent<GasSacElitePattern>();
        }

        private static void MovePlayer(float distance)
            => MovePlayerTo(origin + Vector3.right * distance);

        private static void MovePlayerTo(Vector3 position)
        {
            player = PlayerController.Instance; health = player.HealthComponent;
            position.y = biome.GetGroundHeight(position) + playerGroundOffset;
            player.SpawnAt(position);
            DontStarveCamera.Instance?.SnapToTarget();
        }
        private static IEnumerator Wait(Func<bool> condition, float timeout, string label)
        {
            // Gameplay deadlines follow simulation time; the Editor watchdog independently bounds wall-clock hangs.
            float until = Time.time + timeout;
            while (!condition()) { Require(Time.time < until, "timeout: " + label); yield return null; }
        }
        private static float PlanarDistance(Vector3 a, Vector3 b) { a.y = b.y = 0; return Vector3.Distance(a, b); }
        private static void Set(Object asset, string path, float value)
        {
            if (!backups.ContainsKey(asset)) backups.Add(asset, EditorJsonUtility.ToJson(asset));
            using var data = new SerializedObject(asset); data.FindProperty(path).floatValue = value; data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void SetBool(Object asset, string path, bool value)
        {
            if (!backups.ContainsKey(asset)) backups.Add(asset, EditorJsonUtility.ToJson(asset));
            using var data = new SerializedObject(asset); data.FindProperty(path).boolValue = value; data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void RestoreAssets()
        {
            foreach (var entry in backups) if (entry.Key != null) { EditorJsonUtility.FromJsonOverwrite(entry.Value, entry.Key); EditorUtility.ClearDirty(entry.Key); }
            backups.Clear();
        }
        private static void Require(bool result, string message) { if (!result) throw new InvalidOperationException(message); }
        private static void Equal(float expected, float actual, string label) => Require(Mathf.Abs(expected - actual) < .001f, label + ": expected " + expected + ", actual " + actual);
        private static string LogPrefix => stage3Mode > 0 ? "[GasSac-D3] " : directionChecks ? "[GasSac-D2] " : hitAudit ? "[GasSac-HitAudit] " : deathChecks ? "[GasSac-Death] " : orbChecks ? "[GasSac-I01B] " : "[GasSac-I01A] ";
        private static BiomeEliteSpawnConfig productionField;
        private static bool productionWasEnabled;
        private static void SuspendProductionField()
        {
            productionField = AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>(IntestineEliteMapSetup.ConfigPath);
            productionWasEnabled = productionField.enabled; productionField.enabled = false;
        }
        private static void ResumeProductionField() { if (productionField != null) productionField.enabled = productionWasEnabled; }
        private static void Pass(string message) { results.Add("PASS " + message); Debug.Log(LogPrefix + "PASS " + message); }
    }
}
