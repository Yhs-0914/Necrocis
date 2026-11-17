using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrocis;
using ProceduralMap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NecrocisEditor
{
    public static class MerchantChecks
    {
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static int assertions;
        private static bool passed, started, previousEnabled;
        private static EnterPlayModeOptions previousOptions;
        private static IEnumerator animation;
        private static double deadline;

        public static void Run()
        {
            if (!Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Run in an isolated batch project.");
            assertions = 0; passed = started = false;
            GameplayFocusOwner.TestFocusOverride = true;
            SaveService.UseStorageRootForTests(Path.GetFullPath("tmp/merchant-check-" + Guid.NewGuid().ToString("N")));
            SessionRuntimeBindings.Register();
            try
            {
                Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out var error), "isolated run: " + error);
                CheckSelection(); CheckScenes();
                previousEnabled = EditorSettings.enterPlayModeOptionsEnabled; previousOptions = EditorSettings.enterPlayModeOptions;
                EditorSettings.enterPlayModeOptionsEnabled = true; EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                deadline = EditorApplication.timeSinceStartup + 120;
                EditorApplication.update += Tick; EditorApplication.playModeStateChanged += State;
                EditorApplication.isPlaying = true;
            }
            catch (Exception exception) { Debug.LogException(exception); GameplayFocusOwner.TestFocusOverride = null; SaveService.ResetStaticStateForTests(); EditorApplication.Exit(1); }
        }

        private static void CheckSelection()
        {
            var boss = new RectInt(48, 48, 32, 32);
            Vector2Int start = new Vector2Int(64, 6);
            bool Walk(Vector2Int cell) => true;
            bool Move(Vector2Int a, Vector2Int b) => true;
            UnityEngine.Random.InitState(8261); var random = UnityEngine.Random.state;
            var positions = new HashSet<Vector2Int>();
            for (int seed = 0; seed < 100; seed++)
            {
                Require(MerchantPlacement.TryChoose(128, 128, seed, start, boss, Walk, Move, boss.Contains, out var p, out bool preferred), "selection seed " + seed);
                Require(preferred && Vector2Int.Distance(p, start) >= 30 && MerchantPlacement.DistanceToArea(p, boss) >= 20, "entrance and arena separation seed " + seed);
                Require(MerchantPlacement.TryChoose(128, 128, seed, start, boss, Walk, Move, boss.Contains, out var repeated, out _) && repeated == p, "seed deterministic " + seed);
                Require(MerchantPlacement.TryChoose(128, 128, seed, start, boss, Walk, Move, boss.Contains, out var nearby, out _, nearEntrance: true)
                    && Vector2Int.Distance(nearby, start) >= 3 && Vector2Int.Distance(nearby, start) <= MerchantPlacement.TestEntranceRadius,
                    "test placement stays near entrance seed " + seed);
                positions.Add(p);
            }
            Require(positions.Count > 50, "placement varies across map seeds");
            Require(UnityEngine.Random.state.Equals(random), "placement preserves combat's global random stream");
            Require(MerchantPlacement.TryChoose(30, 30, 7, new Vector2Int(15, 15), null, Walk, Move, null, out var small, out bool strict)
                && !strict && Vector2Int.Distance(small, new Vector2Int(15, 15)) > 16, "small maps use distant safe fallback");
            Require(!MerchantPlacement.TryChoose(20, 20, 7, start, null, _ => false, Move, null, out _, out _), "no placement on invalid ground");
            bool Split(Vector2Int p) => p.x != 25;
            Require(MerchantPlacement.TryChoose(60, 60, 10, new Vector2Int(5, 5), null, Split, Move, null, out var connected, out _)
                && connected.x < 25, "merchant cannot spawn on disconnected island");
        }

        private static void CheckScenes()
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(WorldMerchantNpc.IdleResourcePath);
            Require(sprites.Length == 4, "four imported idle frames");
            foreach (Sprite sprite in sprites)
                Require(sprite.texture.filterMode == FilterMode.Point && sprite.rect.width == 513 && sprite.rect.height == 569,
                    "consistent pixel-art frame import: " + sprite.name);
            var expected = new Dictionary<BiomeType, Vector2Int>();
            foreach (string name in new[] { "Intestine", "Liver", "Stomach", "Lung" })
            {
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/" + name + ".unity", OpenSceneMode.Single);
                var bridge = UnityEngine.Object.FindFirstObjectByType<ProceduralBiomeBridge>();
                var map = UnityEngine.Object.FindFirstObjectByType<MapGenerator>();
                Require(bridge != null && map != null, name + " actual scene terrain and bridge");
                Invoke(bridge, "Awake"); map.GenerateMap();
                Require(map.IsReady, name + " real terrain generated");
                var spawner = WorldMerchantSpawner.Ensure(bridge);
                typeof(WorldMerchantSpawner).GetField("spawnNearEntranceForTesting", Members).SetValue(spawner, false);
                spawner.SpawnNow();
                Require(spawner.Merchant != null && spawner.UsedPreferredDistances, name + " one merchant with preferred distances");
                Vector2Int cell = spawner.Merchant.Cell;
                Require(map.WorldToCell(spawner.Merchant.transform.position) == cell, name + " merchant world position matches terrain cell");
                expected[bridge.BiomeType] = cell;
                Require(Vector2Int.Distance(cell, bridge.WorldToGrid(bridge.GetPlayerSpawnPosition())) >= 30,
                    name + " merchant far from entrance");
                Require(!spawner.BossArea.HasValue || MerchantPlacement.DistanceToArea(cell, spawner.BossArea.Value) >= 20,
                    name + " merchant far from boss walls and approach");
                Require(!map.Data.GetCell(cell.x, cell.y).Occupied && map.IsCellWalkable(cell.x, cell.y), name + " merchant on unoccupied safe ground");
                Require(!spawner.Merchant.IsVisible, name + " hidden outside loaded chunks");
                var chunks = (HashSet<Vector2Int>)typeof(BiomeManager).GetField("loadedChunks", Members).GetValue(bridge);
                Vector2Int chunk = bridge.GridToChunk(cell.x, cell.y);
                chunks.Add(chunk); spawner.RefreshVisibility(); Require(spawner.Merchant.IsVisible, name + " visible in loaded chunk");
                chunks.Remove(chunk); spawner.RefreshVisibility(); Require(!spawner.Merchant.IsVisible, name + " hidden on chunk unload");
                chunks.Add(chunk); spawner.RefreshVisibility();
                WorldMerchantSpawner.Ensure(bridge).SpawnNow(); spawner.SpawnNow();
                Require(bridge.GetComponentsInChildren<WorldMerchantNpc>(true).Length == 1, name + " repeated spawning does not duplicate merchant");
                RenderPreview(name, map, spawner.Merchant);
                UnityEngine.Object.DestroyImmediate(spawner.Merchant.gameObject); UnityEngine.Object.DestroyImmediate(spawner);
                spawner = WorldMerchantSpawner.Ensure(bridge);
                typeof(WorldMerchantSpawner).GetField("spawnNearEntranceForTesting", Members).SetValue(spawner, false);
                spawner.SpawnNow();
                Require(spawner.Merchant != null && spawner.Merchant.Cell == cell, name + " stored placement reused on reentry");
                UnityEngine.Object.DestroyImmediate(spawner.Merchant.gameObject); UnityEngine.Object.DestroyImmediate(spawner);
                spawner = WorldMerchantSpawner.Ensure(bridge); spawner.SpawnNow();
                Vector2Int entrance = bridge.WorldToGrid(bridge.GetPlayerSpawnPosition());
                Require(spawner.SpawnNearEntranceForTesting && spawner.Merchant != null
                    && Vector2Int.Distance(spawner.Merchant.Cell, entrance) <= MerchantPlacement.TestEntranceRadius,
                    name + " test mode overrides old far-away placement");
                var preserved = SaveService.FindWorldMerchant(bridge.BiomeType, bridge.Seed);
                Require(preserved != null && new Vector2Int(preserved.x, preserved.y) == cell, name + " normal saved placement remains intact during test mode");
                Debug.Log("MERCHANT_TEST_NEAR " + name + " cell=" + spawner.Merchant.Cell + " entranceDistance=" + Vector2Int.Distance(spawner.Merchant.Cell, entrance));
                Debug.Log("MERCHANT_SCENE " + name + " cell=" + cell + " entranceDistance="
                    + Vector2Int.Distance(cell, bridge.WorldToGrid(bridge.GetPlayerSpawnPosition())));
            }
            SaveService.ClearActiveSessionReference();
            Require(SaveService.TryContinue(GameDifficulty.Normal, out var error), "continue merchant placements: " + error);
            foreach (var entry in expected)
            {
                var stored = SaveService.FindWorldMerchant(entry.Key, SaveService.GetOrCreateBiomeSeed(entry.Key));
                Require(stored != null && new Vector2Int(stored.x, stored.y) == entry.Value, entry.Key + " placement survives save reload");
            }
        }

        private static void RenderPreview(string name, MapGenerator map, WorldMerchantNpc merchant)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            var node = new GameObject("MerchantPreviewCamera"); var camera = node.AddComponent<Camera>(); node.tag = "MainCamera";
            camera.orthographic = true; camera.orthographicSize = 2.3f;
            camera.transform.position = merchant.transform.position + new Vector3(0, 7, -6);
            camera.transform.LookAt(merchant.transform.position + Vector3.up * .8f);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.12f, .07f, .1f);
            typeof(MapGenerator).GetField("player", Members).SetValue(map, merchant.transform); Invoke(map, "Update");
            foreach (var billboard in merchant.GetComponentsInChildren<Billboard>()) Invoke(billboard, "LateUpdate");
            var target = new RenderTexture(640, 640, 24); var previous = RenderTexture.active;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                var image = new Texture2D(640, 640, TextureFormat.RGBA32, false);
                image.ReadPixels(new Rect(0, 0, 640, 640), 0, 0); image.Apply();
                Directory.CreateDirectory("tmp/merchant-previews");
                File.WriteAllBytes("tmp/merchant-previews/" + name + ".png", image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
            }
            finally
            {
                RenderTexture.active = previous; camera.targetTexture = null; target.Release();
                UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(node);
            }
        }

        private static void Tick()
        {
            if (EditorApplication.timeSinceStartup > deadline) { Debug.LogError("Merchant idle checks timed out."); Finish(); return; }
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || Time.frameCount < 3) return;
            try
            {
                if (!started) { started = true; animation = CheckAnimation(); }
                if (!animation.MoveNext())
                { passed = true; Debug.Log("MERCHANT_PASS assertions=" + assertions); EditorApplication.isPlaying = false; }
            }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.isPlaying = false; }
        }

        private static IEnumerator CheckAnimation()
        {
            InputManager.Instance.Focus.SetNativeFocused(true); InputManager.ResetSessionClock();
            Time.timeScale = 1;
            var parent = new GameObject("MerchantIdleCheck");
            parent.AddComponent<AudioListener>();
            var merchant = WorldMerchantNpc.Create(parent.transform, Vector2Int.zero, new Vector3(2, 0, 2));
            Require(merchant != null && merchant.FrameIndex == 0, "idle begins at first frame");
            Vector3 ground = merchant.transform.position;
            float end = Time.time + 1.02f; while (Time.time < end) yield return null;
            Require(merchant.FrameIndex != 0 && merchant.transform.position == ground, "idle animates without walking");
            int frame = merchant.FrameIndex; Time.timeScale = 0;
            double wait = EditorApplication.timeSinceStartup + .15; while (EditorApplication.timeSinceStartup < wait) yield return null;
            Require(merchant.FrameIndex == frame, "idle pauses with game time"); Time.timeScale = 1;
            end = Time.time + .65f; while (Time.time < end) yield return null;
            Require(merchant.FrameIndex == 0 && merchant.transform.position == ground, "idle returns seamlessly to rest");
            foreach (string name in new[] { "Intestine", "Liver", "Stomach", "Lung" })
            {
                EditorSceneManager.LoadSceneInPlayMode("Assets/_Project/Scenes/" + name + ".unity", new LoadSceneParameters(LoadSceneMode.Single));
                yield return null;
                double until = EditorApplication.timeSinceStartup + 20;
                WorldMerchantSpawner spawner = null;
                while (EditorApplication.timeSinceStartup < until)
                {
                    spawner = UnityEngine.Object.FindObjectsByType<WorldMerchantSpawner>(FindObjectsSortMode.None)
                        .FirstOrDefault(value => value.gameObject.scene.name == name);
                    if (spawner != null && spawner.Merchant != null) break;
                    yield return null;
                }
                Require(spawner != null && spawner.Merchant != null, name + " scene startup automatically creates merchant");
                Require(spawner.GetComponent<BiomeManager>().BiomeType.ToString() == name, name + " runtime biome matches requested scene");
                var owner = spawner.GetComponent<BiomeManager>();
                Require(spawner.SpawnNearEntranceForTesting && Vector2Int.Distance(spawner.Merchant.Cell,
                    owner.WorldToGrid(owner.GetPlayerSpawnPosition())) <= MerchantPlacement.TestEntranceRadius,
                    name + " live test merchant spawns near player entrance");
                Require(UnityEngine.Object.FindObjectsByType<WorldMerchantNpc>(FindObjectsSortMode.None).Length == 1,
                    name + " live scene contains exactly one merchant");
                Debug.Log("MERCHANT_RUNTIME " + name + " cell=" + spawner.Merchant.Cell);
            }
        }

        private static object Invoke(object target, string name) => target.GetType().GetMethod(name, Members).Invoke(target, null);
        private static void Require(bool condition, string label) { assertions++; if (!condition) throw new InvalidOperationException(label); }
        private static void State(PlayModeStateChange state) { if (state == PlayModeStateChange.EnteredEditMode) Finish(); }
        private static void Finish()
        {
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= State;
            EditorSettings.enterPlayModeOptionsEnabled = previousEnabled; EditorSettings.enterPlayModeOptions = previousOptions;
            Time.timeScale = 1; GameplayFocusOwner.TestFocusOverride = null; SaveService.ResetStaticStateForTests(); EditorApplication.Exit(passed ? 0 : 1);
        }
    }
}
