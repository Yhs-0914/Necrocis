using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Necrocis;
using ProceduralMap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NecrocisEditor
{
    /// <summary>Exercises the real Hub trigger, persistent player, arena movement and return trigger.</summary>
    public static class FinalBossSmokeRunner
    {
        private const string Output = "Exports/FinalBossConcepts/2026-09-26/TilemapImplementation";
        private static readonly Vector2 Footprint = new Vector2(.68f, .48f);
        private static SceneSetup[] sceneSetup;
        private static bool previousOptionsEnabled;
        private static EnterPlayModeOptions previousOptions;
        private static int stage;
        private static double deadline;
        private static float nextAction;
        private static Vector3 startPosition;
        private static string failure;
        private static int movementSteps;
        private static bool running;
        private static bool directPreview;
        private static bool previousRunInBackground;
        private static bool playModeEntered;

        [MenuItem("Tools/Necrocis/Final Boss/Run Exploration Smoke Test")]
        public static void Run()
        {
            Begin(false);
        }

        [MenuItem("Tools/Necrocis/Final Boss/Run Direct Scene Smoke Test")]
        public static void RunDirect()
        {
            Begin(true);
        }

        private static void Begin(bool direct)
        {
            if (running || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before running the test.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save open scene edits before running the test.");

            sceneSetup = EditorSceneManager.GetSceneManagerSetup();
            Directory.CreateDirectory(Output);
            SaveService.UseStorageRootForTests(Path.GetFullPath("Library/FinalBossSmokeSave-" + Guid.NewGuid().ToString("N")));
            if (!SaveService.TryBeginNewGame(GameDifficulty.Normal, out string error))
                throw new InvalidOperationException(error);
            previousOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            previousOptions = EditorSettings.enterPlayModeOptions;
            previousRunInBackground = Application.runInBackground;
            Application.runInBackground = true;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            failure = null;
            directPreview = direct;
            stage = direct ? 10 : 0;
            nextAction = 0f;
            movementSteps = 0;
            running = true;
            playModeEntered = false;
            deadline = EditorApplication.timeSinceStartup + 120;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += Tick;
            Application.logMessageReceived += OnLog;
            EditorSceneManager.OpenScene(direct ? FinalBossSceneBuilder.ScenePath : "Assets/_Project/Scenes/Hub.unity");
            EditorApplication.isPlaying = true;
        }

        private static void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error)
                failure ??= message + "\n" + stack;
        }

        private static void Tick()
        {
            if (!running) return;
            Application.runInBackground = true;
            if (EditorApplication.timeSinceStartup > deadline)
            {
                Finish("Timed out at stage " + stage);
                return;
            }
            if (!playModeEntered || !EditorApplication.isPlaying || Time.timeSinceLevelLoad < 1.3f) return;
            if (failure != null) { Finish(failure); return; }
            if (Time.time < nextAction) return;
            nextAction = Time.time + .08f;
            try
            {
                GameManager game = GameManager.Instance;
                PlayerController player = PlayerController.Instance;
                Require(game != null && player != null, "Player/game missing");
                if (stage == 10)
                {
                    FinalBossArena arena = FinalBossArena.Instance;
                    Require(arena != null && DontStarveCamera.Instance != null, "Direct preview bootstrap missing");
                    Require(!game.HasAllRelics, "Direct preview modified boss progress");
                    Require(game.CurrentState == GameState.InFinalBoss, "Direct preview state incorrect");
                    Require(SpawnDistance(player, arena) < .1f, "Direct preview spawn incorrect");
                    VerifySharedPipeline(arena, player);
                    Require(player.CurrentVisualSprite != null, "Direct preview player invisible");
                    startPosition = player.transform.position;
                    Require(player.TryMoveByWorld(Vector3.forward), "Direct preview movement blocked");
                    stage = 11;
                    return;
                }
                if (stage == 11)
                {
                    Require(player.transform.position.z > startPosition.z + .8f, "Direct preview did not move");
                    Capture(Camera.main, "gameplay-direct-preview.png");
                    player.SpawnAt(FinalBossArena.Instance.ReturnPosition);
                    Physics.SyncTransforms();
                    stage = 12;
                    return;
                }
                if (stage == 12)
                {
                    if (SceneManager.GetActiveScene().name != SceneLoader.SCENE_HUB || SceneLoader.Instance.IsLoading) return;
                    Require(!game.HasAllRelics && game.CurrentState == GameState.InHub, "Preview return modified progress");
                    Finish(null);
                    return;
                }
                if (stage == 0)
                {
                    Altar altar = UnityEngine.Object.FindFirstObjectByType<Altar>();
                    Require(altar != null, "Hub final portal missing");
                    // All fifteen incomplete combinations must reject entry, including 3/4.
                    for (int mask = 0; mask < 15; mask++)
                    {
                        game.RestoreFromSave(new RunSaveData { bosses = new BossProgressSaveData {
                            intestineDefeated = (mask & 1) != 0, liverDefeated = (mask & 2) != 0,
                            stomachDefeated = (mask & 4) != 0, lungDefeated = (mask & 8) != 0 } });
                        altar.TryInteract(player.gameObject);
                        Require(!SceneLoader.Instance.LoadFinalBoss(), "Incomplete clear mask admitted: " + mask);
                        Require(game.CurrentState != GameState.InFinalBoss, "Incomplete mask changed state");
                    }
                    PlayerItemTestPanel panel = player.GetComponent<PlayerItemTestPanel>();
                    Require(panel != null, "F8 panel missing");
                    typeof(PlayerItemTestPanel).GetMethod("EnsureUi", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(panel, null);
                    Transform buttonTransform = player.transform.Find("ItemTestCanvas/Panel/CompleteBiomeBossesButton");
                    Require(buttonTransform != null, "F8 complete-bosses button missing");
                    var button = buttonTransform.GetComponent<UnityEngine.UI.Button>();
                    button.onClick.Invoke();
                    button.onClick.Invoke(); // Repeated clicks must leave a consistent 4/4 state.
                    foreach (BiomeType biome in new[] { BiomeType.Intestine, BiomeType.Liver, BiomeType.Stomach, BiomeType.Lung })
                        Require(game.HasRelic(biome) && BossProgress.IsDefeated(biome), "F8 progress mismatch: " + biome);
                    Require(game.HasAllRelics, "Four clears not recorded");
                    Vector3 entry = altar.transform.position;
                    entry.y = player.transform.position.y;
                    player.SpawnAt(entry);
                    Physics.SyncTransforms(); // actual trigger, not a direct LoadScene call
                    stage = 1;
                    return;
                }
                if (stage == 1)
                {
                    if (SceneManager.GetActiveScene().name != SceneLoader.SCENE_FINAL_BOSS || SceneLoader.Instance.IsLoading) return;
                    FinalBossArena arena = FinalBossArena.Instance;
                    Require(arena != null && arena.PillarCount == 4, "Arena/pillars missing");
                    Require(SpawnDistance(player, arena) < .1f, "Incorrect spawn");
                    VerifySharedPipeline(arena, player);
                    Require(game.CurrentState == GameState.InFinalBoss, "Incorrect game state");
                    VerifyGeometry(arena);
                    Require(UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length == 0, "Unexpected enemies");
                    Capture(Camera.main, "gameplay-entry.png");
                    startPosition = player.transform.position;
                    stage = 2;
                    return;
                }
                if (stage == 2)
                {
                    if (movementSteps++ < 18)
                    {
                        Require(player.TryMoveByWorld(Vector3.forward * .2f), "Actual northward movement rejected");
                        return;
                    }
                    Require(player.transform.position.z > startPosition.z + 3f, "Player did not physically move");
                    FinalBossArena arena = FinalBossArena.Instance;
                    player.SpawnAt(arena.UVToWorld(new Vector2(.5f, .53f)));
                    Physics.SyncTransforms();
                    startPosition = player.transform.position;
                    Require(!player.GetComponent<ProceduralTerrainMotor>().CanMove(startPosition, startPosition + Vector3.forward * 30f), "North wall/boss sweep accepted");
                    player.TryMoveByWorld(Vector3.forward * 30f);
                    stage = 3;
                    return;
                }
                if (stage == 3)
                {
                    // The common biome motor can move up to a wall; its bool reports any movement,
                    // not whether the entire requested displacement was completed.
                    FinalBossArena arena = FinalBossArena.Instance;
                    Require(arena.IsWalkable(player.transform.position, Footprint)
                        && arena.CanTraverse(startPosition, player.transform.position, Footprint), $"North movement crossed blocked cells: {startPosition} -> {player.transform.position}; walkable={arena.IsWalkable(player.transform.position, Footprint)}; motor={player.GetComponent<ProceduralTerrainMotor>().TerrainHalfExtents}");
                    startPosition = player.transform.position;
                    Require(!player.GetComponent<ProceduralTerrainMotor>().CanMove(startPosition, startPosition + Vector3.right * 60f), "East wall sweep accepted");
                    player.TryMoveByWorld(Vector3.right * 60f);
                    stage = 6;
                    return;
                }
                if (stage == 6)
                {
                    FinalBossArena arena = FinalBossArena.Instance;
                    Require(arena.IsWalkable(player.transform.position, Footprint)
                        && arena.CanTraverse(startPosition, player.transform.position, Footprint), $"East movement crossed blocked cells: {startPosition} -> {player.transform.position}");
                    Capture(Camera.main, "gameplay-center.png");
                    CaptureOverview(arena);
                    player.SpawnAt(arena.UVToWorld(new Vector2(.5f, .70f)));
                    Physics.SyncTransforms();
                    stage = 4;
                    return;
                }
                if (stage == 4)
                {
                    Capture(Camera.main, "gameplay-dormant-boss.png");
                    player.SpawnAt(FinalBossArena.Instance.ReturnPosition);
                    Physics.SyncTransforms();
                    stage = 5;
                    return;
                }
                if (stage == 5)
                {
                    if (SceneManager.GetActiveScene().name != SceneLoader.SCENE_HUB || SceneLoader.Instance.IsLoading) return;
                    Require(game.HasAllRelics && game.CurrentState == GameState.InHub, "Hub return lost progress/state");
                    Require(Vector3.Distance(player.transform.position, new Vector3(16f, -2f, 7f)) < .2f, "Hub return spawn incorrect");
                    Require(FinalBossArena.Instance == null, "Stale arena after return");
                    Finish(null);
                }
            }
            catch (Exception e) { Finish(e.ToString()); }
        }

        private static float SpawnDistance(PlayerController player, FinalBossArena arena)
        {
            Vector3 delta = player.transform.position - arena.SpawnPosition;
            delta.y = 0;
            return delta.magnitude;
        }

        private static void VerifySharedPipeline(FinalBossArena arena, PlayerController player)
        {
            MapGenerator map = arena.GetComponent<MapGenerator>();
            Require(map != null && map.IsReady && map.AuthoredLayout != null, "Shared map generator not ready");
            Require(BiomeManager.Active == arena.GetComponent<ProceduralBiomeBridge>(), "Shared biome bridge not active");
            Require(player.GetComponent<ProceduralTerrainMotor>().HasActiveMap, "Common terrain motor not bound");
            Require(GameObject.Find("DormantCerebrum_MapArtwork") == null, "Old flattened map still present");
            Require(UnityEngine.Object.FindObjectsByType<UnityEngine.Tilemaps.Tilemap>(FindObjectsSortMode.None)
                .Length >= 4, "Shared tilemap layers missing");
        }

        private static void VerifyGeometry(FinalBossArena arena)
        {
            Require(arena.IsWalkable(arena.SpawnPosition, Footprint), "Spawn blocked");
            Require(arena.CanTraverse(arena.SpawnPosition, arena.UVToWorld(new Vector2(.5f, .70f)), Footprint), "Central route blocked");
            Require(arena.CanTraverse(arena.SpawnPosition, arena.ReturnPosition, Footprint), "Return corridor blocked");
            for (int i = 0; i < 4; i++)
            {
                Vector3 pillar = arena.GetPillarPosition(i);
                Require(!arena.IsWalkable(pillar, Footprint), "Pillar center walkable");
                Require(!arena.CanTraverse(pillar - Vector3.right * 7f, pillar + Vector3.right * 7f, Footprint), "Dash crosses pillar " + i);
            }
            // Flood-fill the walkable floor; every region must be reachable from the entrance.
            const float step = .25f;
            int width = Mathf.RoundToInt(arena.WorldSize.x / step), height = Mathf.RoundToInt(arena.WorldSize.y / step);
            var walkable = new HashSet<Vector2Int>();
            for (int x = 0; x <= width; x++)
                for (int z = 0; z <= height; z++)
                    if (arena.IsWalkable(new Vector3(x * step, 0f, z * step), Footprint))
                        walkable.Add(new Vector2Int(x, z));
            var pending = new Queue<Vector2Int>();
            Vector2Int seed = new Vector2Int(Mathf.RoundToInt(arena.SpawnPosition.x / step), Mathf.RoundToInt(arena.SpawnPosition.z / step));
            Require(walkable.Remove(seed), "Flood-fill seed invalid");
            pending.Enqueue(seed);
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };
            while (pending.Count > 0)
            {
                Vector2Int p = pending.Dequeue();
                foreach (Vector2Int d in directions)
                    if (walkable.Contains(p + d)
                        && arena.CanTraverse(new Vector3(p.x * step, 0f, p.y * step),
                            new Vector3((p.x + d.x) * step, 0f, (p.y + d.y) * step), Footprint))
                    {
                        walkable.Remove(p + d);
                        pending.Enqueue(p + d);
                    }
            }
            Require(walkable.Count == 0, "Unreachable floor cells: " + walkable.Count + " " + string.Join(", ", walkable));
        }

        private static void CaptureOverview(FinalBossArena arena)
        {
            var go = new GameObject("OverviewCapture");
            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 14.5f;
            camera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            camera.transform.position = arena.UVToWorld(new Vector2(.5f, .5f)) + new Vector3(0f, 40f, -40f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.08f, .055f, .1f);
            Capture(camera, "arena-overview.png");
            UnityEngine.Object.Destroy(go);
        }

        private static void Capture(Camera camera, string name)
        {
            Require(camera != null, "Camera missing");
            RenderTexture previous = camera.targetTexture, active = RenderTexture.active;
            var rt = new RenderTexture(1600, 900, 24);
            var image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = rt;
                camera.Render();
                RenderTexture.active = rt;
                image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
                image.Apply();
                File.WriteAllBytes(Path.Combine(Output, name), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previous;
                RenderTexture.active = active;
                UnityEngine.Object.Destroy(image);
                rt.Release();
                UnityEngine.Object.Destroy(rt);
            }
        }

        private static void Finish(string error)
        {
            failure = error;
            running = false;
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;
            EditorApplication.isPlaying = false;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                playModeEntered = true;
                nextAction = Time.time + 1.5f;
                Application.runInBackground = true;
                return;
            }
            if (state != PlayModeStateChange.EnteredEditMode) return;
            if (running) failure ??= "Test interrupted before completing stage " + stage;
            running = false;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;
            EditorSettings.enterPlayModeOptionsEnabled = previousOptionsEnabled;
            EditorSettings.enterPlayModeOptions = previousOptions;
            Application.runInBackground = previousRunInBackground;
            SaveService.ResetStaticStateForTests();
            EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
            string result = failure ?? "PASS: all 15 incomplete clear combinations blocked; four-clear Hub trigger entered FinalBoss; spawn, connected floor, four pillar sweeps, actual player movement, wall sweeps, return trigger and retained progress verified.";
            if (directPreview && failure == null)
                result = "PASS: direct FinalBoss scene Play creates the actual player and camera; correct spawn, visible animation, actual movement, Hub return, and no boss-progress unlock verified.";
            File.WriteAllText(Output + (directPreview ? "/direct-smoke-result.txt" : "/smoke-result.txt"), result);
            File.WriteAllText("Library/FinalBossSmoke.result", result);
            Debug.Log("[FinalBossSmoke] " + result);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
