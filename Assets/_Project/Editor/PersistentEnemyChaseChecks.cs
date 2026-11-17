using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace NecrocisEditor
{
    public sealed class ChaseCheckBiome : BiomeManager
    {
        protected override void Awake()
        {
            biomeType = BiomeType.Intestine;
            mapWidth = mapHeight = 512; chunkSize = 16; tileSize = 1;
            base.Awake();
        }
        protected override void Start() { }
        protected override void Update() { }
        protected override void GenerateObjectsForChunk(Chunk chunk) { }
        protected override TileSample SampleBaseTile(int x, int y) => default;
        protected override TileBase GetTileAsset(BiomeTileType type) => null;
    }

    // Uses actual FSM, pool ownership and visit storage in an isolated Play Mode scene.
    public static class PersistentEnemyChaseChecks
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static bool passed, started, oldOptionsEnabled;
        private static EnterPlayModeOptions oldOptions;
        private static double deadline;
        private static int assertions;

        public static void Run()
        {
            if (!Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Run in an isolated Unity batch project.");
            passed = started = false; assertions = 0;
            oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            oldOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            SaveService.UseStorageRootForTests(Path.GetFullPath("tmp/chase-check-" + Guid.NewGuid().ToString("N")));
            GameplayFocusOwner.TestFocusOverride = true;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("ChaseCheckAudio").AddComponent<AudioListener>();
            deadline = EditorApplication.timeSinceStartup + 120;
            EditorApplication.playModeStateChanged += OnState;
            EditorApplication.update += Tick;
            EditorApplication.isPlaying = true;
        }

        private static void Tick()
        {
            if (!started && EditorApplication.isPlaying && !EditorApplication.isCompiling && Time.frameCount > 2)
            {
                started = true; Execute(); return;
            }
            if (EditorApplication.timeSinceStartup < deadline) return;
            Debug.LogError("PersistentEnemyChaseChecks timed out."); Finish();
        }

        private static void Execute()
        {
            try
            {
                SessionRuntimeBindings.Register();
                Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out var error), "isolated run: " + error);
                InputManager.Instance.Focus.SetNativeFocused(true);
                InputManager.ResetSessionClock();
                var playerObject = new GameObject("ChaseCheckPlayer");
                PlayerCompositionRoot.EnsureGameplay(playerObject);
                var player = playerObject.GetComponent<PlayerController>();
                player.enabled = false;
                var biome = new GameObject("ChaseCheckBiome").AddComponent<ChaseCheckBiome>();
                var store = EnemyVisitStateStore.ForBiome(biome);
                Require(store.IsAvailable, "world visit is available");
                var chunkRoot = new GameObject("OriginChunk").transform;
                chunkRoot.SetParent(biome.transform, false);
                var rule = new EnemySpawnRuleConfig { name = "ChaseCheckNormal", moveSpeed = 2, chaseRadius = 6,
                    leashRadius = 8, maxHealth = 10, attackRange = 1, enableContactDamage = false };
                var world = new FlatWorld();
                var context = new EnemySpawnContext(null, world);
                var spawner = new GameObject("OriginSpawner").AddComponent<EnemySpawner>();
                spawner.transform.SetParent(chunkRoot, false);
                spawner.ConfigureForWorld(rule, Vector3.zero);
                spawner.BindStateStore(store);
                var identity = spawner.Identity;
                var enemy = EnemyController.Acquire(chunkRoot, rule.name, EnemyController.GetPoolArchetypeId(rule));
                enemy.ConfigureWithContext(spawner, rule, Vector3.zero, Vector3.zero, context);
                List<EnemyController> members = Get<List<EnemyController>>(spawner, "activeEnemies");
                members.Add(enemy); Set(spawner, "initializedWave", true);

                MovePlayer(player, Vector3.right * 100);
                Invoke(enemy, "Update");
                Require(!enemy.IsPlayerInChaseRange() && !enemy.HasPersistentPlayerAggro,
                    "undetected normal enemy does not acquire a faraway player");
                MovePlayer(player, Vector3.right * 3);
                Invoke(enemy, "Update");
                Require(enemy.HasPersistentPlayerAggro && Get<IEnemyState>(enemy, "currentState") == EnemyChaseState.Instance,
                    "first detection latches chase");
                MovePlayer(player, Vector3.right * 100);
                MoveEnemy(enemy, Vector3.right * 30);
                Require(enemy.IsPlayerInChaseRange() && !enemy.IsOutOfLeash(), "latched normal ignores detection and anchor distances");
                EnemyChaseState.Instance.Update(enemy, .5f);
                Require(enemy.GetComponent<Rigidbody>().position.x > 30
                    && Get<IEnemyState>(enemy, "currentState") == EnemyChaseState.Instance, "normal keeps moving toward distant player");
                world.Locked = true;
                Require(!enemy.IsPlayerInChaseRange(), "locked boss encounter still blocks field detection");
                world.Locked = false;
                enemy.ChangeState(EnemyAttackState.Instance);
                EnemyAttackState.Instance.Update(enemy, 0);
                Require(Get<IEnemyState>(enemy, "currentState") == EnemyChaseState.Instance && enemy.HasPersistentPlayerAggro,
                    "leaving attack range resumes persistent chase");

                var eliteRule = rule.CreateRuntimeCopy(); eliteRule.isElite = true; eliteRule.name = "ChaseCheckElite";
                var elite = Spawn(chunkRoot, eliteRule, context);
                elite.ChangeState(EnemyChaseState.Instance); Invoke(elite, "Update");
                Require(!elite.HasPersistentPlayerAggro && !elite.IsPlayerInChaseRange(), "elite retains finite detection");
                var boss = Spawn(chunkRoot, rule, new EnemySpawnContext(null, world, EnemyEncounterRole.Boss));
                boss.ChangeState(EnemyChaseState.Instance); Invoke(boss, "Update");
                Require(!boss.HasPersistentPlayerAggro && !boss.IsPlayerInChaseRange(), "boss retains encounter behavior");
                var stationaryRule = rule.CreateRuntimeCopy(); stationaryRule.isRanged = true;
                var stationary = Spawn(chunkRoot, stationaryRule, context);
                stationary.ChangeState(EnemyChaseState.Instance); Invoke(stationary, "Update");
                Require(!stationary.HasPersistentPlayerAggro, "stationary ranged enemy remains stationary");

                var reused = Spawn(chunkRoot, rule, context);
                MovePlayer(player, Vector3.right * 3); Invoke(reused, "Update");
                var snapshot = reused.CaptureSuspension();
                reused.ReleaseForSuspension(snapshot);
                MovePlayer(player, Vector3.right * 100);
                reused = EnemyController.Acquire(chunkRoot, "ResumedChaser", EnemyController.GetPoolArchetypeId(rule));
                reused.PrepareSuspendedRestore(null, snapshot, context); reused.PublishSuspendedRestore(); Invoke(reused, "Update");
                Require(reused.HasPersistentPlayerAggro && reused.IsPlayerInChaseRange(), "suspended logical enemy remembers pursuit");
                reused.ReleaseToPool();
                reused = EnemyController.Acquire(chunkRoot, "FreshNormal", EnemyController.GetPoolArchetypeId(rule));
                reused.ConfigureWithContext(null, rule, Vector3.zero, Vector3.zero, context); Invoke(reused, "Update");
                Require(!reused.HasPersistentPlayerAggro && !reused.IsPlayerInChaseRange(), "fresh pooled spawn clears previous aggro");

                var idle = EnemyController.Acquire(chunkRoot, "UndetectedNeighbor", EnemyController.GetPoolArchetypeId(rule));
                idle.ConfigureWithContext(spawner, rule, Vector3.zero, Vector3.forward * 5, context);
                members.Add(idle);
                var pursuitLife = enemy.LogicalLife;
                Require(spawner.SuspendSpawnedEnemies(), "origin placement suspends normally");
                Require(store.TryGet(identity, out var group) && group.Living.Length == 1 && group.Pursuers.Length == 1,
                    "unengaged neighbor suspends while pursuit keeps its slot");
                Require(enemy.gameObject.activeInHierarchy && !pursuitLife.IsSuspended && !pursuitLife.IsRetired,
                    "pursuing actor remains alive rather than being suspended");
                Require(enemy.transform.parent.name == "PursuingEnemies" && !enemy.transform.IsChildOf(chunkRoot),
                    "pursuer detaches from origin chunk");
                Require(enemy.TrySuspendByOwner() && enemy.gameObject.activeInHierarchy,
                    "buffered chunk cleanup cannot suspend a retained pursuer");
                Require(spawner.ParkForReuse(), "native origin spawner can return to its pool");
                UnityEngine.Object.DestroyImmediate(chunkRoot.gameObject);
                Require(enemy != null && enemy.gameObject.activeInHierarchy && enemy.HasPersistentPlayerAggro,
                    "destroying origin chunk does not end chase");
                float before = enemy.GetComponent<Rigidbody>().position.x;
                EnemyChaseState.Instance.Update(enemy, .5f);
                Require(enemy.GetComponent<Rigidbody>().position.x > before, "pursuer keeps moving after chunk unload");

                var reloadedRoot = new GameObject("ReloadedChunk").transform;
                reloadedRoot.SetParent(biome.transform, false);
                var reloaded = new GameObject("ReloadedSpawner").AddComponent<EnemySpawner>();
                reloaded.transform.SetParent(reloadedRoot, false);
                reloaded.ConfigureWithIdentity(rule, Vector3.zero, identity);
                reloaded.BindStateStore(store);
                Require((bool)Invoke(reloaded, "TryResumeStoredGroup"), "origin group restores on return");
                Require(Get<List<EnemyController>>(reloaded, "activeEnemies").Count == 1
                    && Get<int>(reloaded, "OccupiedSpawnSlots", true) == 2, "live pursuer reserves one population slot after reload");
                Invoke(reloaded, "TrySpawnWave", 2);
                Require(Get<List<EnemyController>>(reloaded, "activeEnemies").Count == 1, "origin cannot duplicate a living pursuer");
                enemy.ReleaseToPool();
                Invoke(reloaded, "CleanupReleasedEnemies");
                Require(pursuitLife.IsRetired && Get<int>(reloaded, "OccupiedSpawnSlots", true) == 1,
                    "retiring pursuer releases its original population slot");
                Require(Get<float>(reloaded, "nextSpawnTime") >= Time.time + rule.respawnCooldown - .01f,
                    "released pursuit slot uses the normal respawn cooldown");

                var last = Spawn(reloadedRoot, rule, context);
                MovePlayer(player, Vector3.right * 3); Invoke(last, "Update");
                Require(store.TryRetainPursuer(last, null), "visit can own a loose pursuing enemy");
                player.Stats.RuntimeStats.ApplyDamage(10000);
                Require(player.IsDead, "player fixture receives lethal damage");
                Invoke(last, "Update");
                Require(!last.HasPersistentPlayerAggro && Get<IEnemyState>(last, "currentState") == EnemyReturnState.Instance,
                    "player death ends pursuit");
                store.enabled = false;
                Require(!last.gameObject.activeInHierarchy && last.LogicalLife == null, "ending visit releases retained actors");
                Debug.Log("PERSISTENT_ENEMY_CHASE_PASS assertions=" + assertions);
                passed = true;
            }
            catch (Exception error) { Debug.LogException(error); }
            EditorApplication.isPlaying = false;
        }

        private static EnemyController Spawn(Transform parent, EnemySpawnRuleConfig rule, EnemySpawnContext context)
        {
            var enemy = EnemyController.Acquire(parent, rule.name, EnemyController.GetPoolArchetypeId(rule));
            enemy.ConfigureWithContext(null, rule, Vector3.zero, Vector3.zero, context);
            return enemy;
        }
        private static void MovePlayer(PlayerController player, Vector3 position)
        {
            player.transform.position = position; player.GetComponent<Rigidbody>().position = position; Physics.SyncTransforms();
        }
        private static void MoveEnemy(EnemyController enemy, Vector3 position)
        {
            enemy.transform.position = position; enemy.GetComponent<Rigidbody>().position = position; Physics.SyncTransforms();
        }
        private static object Invoke(object target, string name, params object[] args)
            => target.GetType().GetMethod(name, Private).Invoke(target, args);
        private static T Get<T>(object target, string name, bool property = false) => (T)(property
            ? target.GetType().GetProperty(name, Private).GetValue(target)
            : target.GetType().GetField(name, Private).GetValue(target));
        private static void Set(object target, string name, object value) => target.GetType().GetField(name, Private).SetValue(target, value);
        private static void Require(bool value, string label)
        {
            assertions++; if (!value) throw new InvalidOperationException(label);
        }
        private static void OnState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode) Finish();
        }
        private static void Finish()
        {
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnState;
            EditorSettings.enterPlayModeOptionsEnabled = oldOptionsEnabled; EditorSettings.enterPlayModeOptions = oldOptions;
            GameplayFocusOwner.TestFocusOverride = null; SaveService.ResetStaticStateForTests();
            EditorApplication.Exit(passed ? 0 : 1);
        }
        private sealed class FlatWorld : IEnemyWorldQueries
        {
            public bool Locked;
            public bool HasTerrain => false;
            public bool IsValidCell(Vector2Int cell) => true;
            public bool IsWalkableCell(Vector2Int cell) => true;
            public Vector2Int WorldToCell(Vector3 position) => new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.z));
            public Vector3 CellToWorld(Vector2Int cell) => new Vector3(cell.x, 0, cell.y);
            public float GetGroundHeight(Vector3 position) => 0;
            public float GetGroundHeight(Vector2Int cell) => 0;
            public bool CanEnemyMove(Vector3 from, Vector3 to, Vector2 halfExtents) => true;
            public bool IsPlayerInsideLockedEncounter(Vector3 position) => Locked;
        }
    }
}
