using System;
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
    /// <summary>Exercises the real Hub entry, persistent player, arena movement and locked final-boss room.</summary>
    public static class FinalBossSmokeRunner
    {
        private static bool spriteAnimationSmoke;
        private static bool victorySmoke;
        private static float victoryRealtime, dismissedRealtime;
        private static string Output => victorySmoke
            ? "Exports/FinalBossConcepts/2026-10-05-victory"
            : spriteAnimationSmoke
            ? "Exports/FinalBossConcepts/2026-10-05-animations/PlayMode"
            : "Exports/FinalBossConcepts/2026-09-28/TilemapImplementation";
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

        [MenuItem("Tools/Necrocis/Final Boss/Verify Existing Organ Maps")]
        public static void VerifyExistingOrganMaps()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before checking organ maps.");
            string[] names = { "Stomach", "Intestine", "Liver", "Lung" };
            // Open separate scene copies only; do not clear or regenerate an already open scene.
            foreach (string name in names)
                if (SceneManager.GetSceneByPath("Assets/_Project/Scenes/" + name + ".unity").isLoaded)
                    throw new InvalidOperationException("Close " + name + " before running this check.");
            Scene previous = SceneManager.GetActiveScene();
            try
            {
                foreach (string name in names)
                {
                    Scene scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/" + name + ".unity", OpenSceneMode.Additive);
                    try
                    {
                        MapGenerator map = null;
                        foreach (GameObject root in scene.GetRootGameObjects())
                        {
                            MapGenerator candidate = root.GetComponentInChildren<MapGenerator>();
                            if (candidate != null) map = candidate;
                        }
                        Require(map != null && map.AuthoredLayout == null, name + " generator changed unexpectedly");
                        ProceduralBiomeBridge bridge = map.GetComponent<ProceduralBiomeBridge>();
                        Require(bridge != null && bridge.GetBiomeConfig().spawnWorldItems,
                            name + " lost its default item spawning setting");
                        map.GenerateMap();
                        Vector2Int spawn = map.WorldToCell(map.GetPlayerSpawnWorldPosition());
                        Require(map.IsReady && map.MapWidth == 300 && map.MapHeight == 300
                            && map.IsCellWalkable(spawn.x, spawn.y), name + " generation/spawn regression");
                        map.ClearMap();
                    }
                    finally { EditorSceneManager.CloseScene(scene, true); }
                }
                Directory.CreateDirectory(Output);
                File.WriteAllText(Output + "/organ-regression-result.txt",
                    "PASS: Stomach, Intestine, Liver and Lung retain procedural 300x300 generation, safe spawn and enabled world-item spawning.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

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

        [MenuItem("Tools/Necrocis/Final Boss/Run Sprite Animation Smoke Test")]
        public static void RunAnimations()
        {
            FinalBossPhaseThreeChecks.Validate();
            Begin(true, true);
        }

        [MenuItem("Tools/Necrocis/Final Boss/Run Collapse and Victory Smoke Test")]
        public static void RunVictory() => Begin(true, true, true);

        private static void Begin(bool direct, bool animations = false, bool victory = false)
        {
            if (running || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before running the test.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save open scene edits before running the test.");

            spriteAnimationSmoke = animations;
            victorySmoke = victory;
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
            // A freshly imported Unity 6 project can emit this editor-only search-index exception
            // during startup. It does not originate from or affect the running game scene.
            if (stack != null && stack.Contains("UnityEditor.Search.SearchDatabase")) return;
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
            if (victorySmoke && stage >= 16)
            {
                try { TickVictoryPrompt(); }
                catch (Exception e) { Finish(e.ToString()); }
                return;
            }
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
                    if (spriteAnimationSmoke)
                    {
                        stage = 4;
                        return;
                    }
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
                    Require(UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length == 4,
                        "Phase one should start with four pillar damage targets only");
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
                    Capture(Camera.main, "gameplay-wall-collision.png");
                    player.SpawnAt(arena.UVToWorld(new Vector2(.5f, .53f)));
                    Physics.SyncTransforms();
                    stage = 7;
                    return;
                }
                if (stage == 7)
                {
                    FinalBossArena arena = FinalBossArena.Instance;
                    Capture(Camera.main, "gameplay-center.png");
                    CaptureOverview(arena);
                    player.SpawnAt(arena.UVToWorld(new Vector2(.5f, .70f)));
                    Physics.SyncTransforms();
                    stage = 4;
                    return;
                }
                if (stage == 4)
                {
                    FinalBossPhaseOneController phase = FinalBossArena.Instance.GetComponent<FinalBossPhaseOneController>();
                    GameObject dormantBoss = GameObject.Find("DormantCerebrum");
                    Require(phase.CurrentPhase == 1 && phase.BossVisible && dormantBoss != null
                        && dormantBoss.GetComponent<EnemyController>() == null,
                        "Dormant boss must remain visible but not be a combat target during phase one");
                    Capture(Camera.main, "gameplay-phase-one-visible-boss.png");

                    FinalBossPillar firstPillar = phase.Pillars[0];
                    float healthBeforeHit = firstPillar.Health;
                    firstPillar.DamageTarget.TakeDamage(5f);
                    Require(firstPillar.Health < healthBeforeHit && !firstPillar.IsDestroyed,
                        "Pillar did not receive normal combat damage before destruction");
                    firstPillar.DestroyForTest();
                    Require(phase.CurrentPhase == 1 && phase.BossVisible && phase.DestroyedPillarCount == 1,
                        "Destroying one pillar changed the dormant boss phase unexpectedly");
                    Require(firstPillar.BrokenSprite != null && firstPillar.CurrentSprite == firstPillar.BrokenSprite,
                        "Destroyed pillar did not switch to its authored broken sprite");
                    Require(firstPillar.RemnantVisible, "Broken pillar disappeared before phase two");
                    Require(!phase.IsBiomeSpawning(firstPillar.Biome),
                        "Destroyed pillar biome kept its continuous spawn channel enabled");
                    EnemyController weakenedBoss = UnityEngine.Object
                        .FindObjectsByType<EnemyController>(FindObjectsSortMode.None)
                        .FirstOrDefault(enemy => enemy.name.Contains("WeakenedMidBoss"));
                    Require(weakenedBoss != null,
                        "Destroying a pillar did not summon its weakened biome mid-boss");
                    Require(Mathf.Abs(weakenedBoss.Config.scale.x - 4.4f) < .01f,
                        "Stomach weakened mid-boss must be twice its previous final-boss-room size");
                    Require(Mathf.Abs(weakenedBoss.Config.maxHealth - 3.3f) < .01f
                        && Mathf.Abs(weakenedBoss.Config.attackDamage - .99f) < .01f,
                        "Weakened mid-boss health/attack stats must be 33% of the original boss rule");
                    VerifyOnlyRemainingBiomesRespawn(phase, firstPillar.Biome);
                    phase.PauseSpawningForTest();
                    CaptureOverview(FinalBossArena.Instance, "arena-overview-pillar-broken.png");

                    phase.DestroyAllPillarsForTest();
                    Require(phase.DestroyedPillarCount == 4 && phase.CurrentPhase == 2 && phase.BossVisible,
                        "Destroying four pillars did not unlock phase two");
                    Require(phase.Pillars.All(pillar => pillar != null && !pillar.RemnantVisible),
                        "Broken pillar remnants remained visible in phase two");
                    FinalBossPhaseTwoController phaseTwo = phase.GetComponent<FinalBossPhaseTwoController>();
                    Require(phaseTwo != null && phaseTwo.IsActive && phaseTwo.BossDamageTarget != null
                        && phaseTwo.Health > 0f,
                        "Phase two did not create a directly damageable final boss");
                    Require(UnityEngine.Object.FindObjectsByType<FinalBossScreenHealthBar>(
                            FindObjectsSortMode.None).Length == 1,
                        "Phase two did not create its bottom-center screen health bar");
                    if (spriteAnimationSmoke)
                        CaptureOverview(FinalBossArena.Instance, "arena-overview-phase-two.png");
                    stage = 9;
                    return;
                }
                if (stage == 9)
                {
                    FinalBossPhaseTwoController phaseTwo = FinalBossArena.Instance
                        .GetComponent<FinalBossPhaseTwoController>();
                    if (phaseTwo == null || !phaseTwo.IsAttacking) return;
                    if (UnityEngine.Object.FindObjectsByType<FinalBossTelegraph>(FindObjectsSortMode.None).Length == 0)
                        return;
                    SpriteRenderer revealedBoss = GameObject.Find("DormantCerebrum")?.GetComponent<SpriteRenderer>();
                    Require(revealedBoss != null && revealedBoss.sprite != null && revealedBoss.sprite.vertices.Length > 4,
                        "Revealed phase-two boss lost its authored outline");
                    Capture(Camera.main, "gameplay-phase-two-unlocked.png");
                    if (phaseTwo.IsReflecting) return;
                    phaseTwo.BossDamageTarget.TakeDamage(phaseTwo.MaxHealth * .65f);
                    Require(phaseTwo.CurrentPhase == 3 && phaseTwo.Health > 0f
                        && phaseTwo.Health <= phaseTwo.MaxHealth * .4f
                        && phaseTwo.PhaseThree != null && phaseTwo.PhaseThree.IsDetaching,
                        "40% threshold did not start phase three with preserved health");
                    Require(FinalBossArena.Instance.GetComponent<FinalBossPhaseOneController>().CurrentPhase == 3,
                        "Encounter phase did not advance to three");
                    Require(phaseTwo.BossDamageTarget.GetComponentsInChildren<BoxCollider>().Length == 8,
                        "Phase-three ground-anchor/brain/stem/tentacle hitboxes were not created");
                    nextAction = Time.time + 2.2f;
                    stage = 12;
                    return;
                }
                if (stage == 12)
                {
                    FinalBossPhaseThreeController phaseThree = FinalBossArena.Instance
                        .GetComponent<FinalBossPhaseThreeController>();
                    Require(phaseThree != null && phaseThree.IsActive && !phaseThree.IsDetaching
                        && phaseThree.HasMobileAppearance,
                        "Phase-three separation did not finish");
                    Require(phaseThree.transform.GetComponent<FinalBossPhaseTwoController>()
                            .BossDamageTarget.GetComponentsInChildren<BoxCollider>().Length == 8,
                        "Phase-two cleanup removed phase-three hitboxes");
                    SpriteRenderer mobileBoss = GameObject.Find("DormantCerebrum")?.GetComponent<SpriteRenderer>();
                    FinalBossSpriteAnimator animation = mobileBoss != null
                        ? mobileBoss.GetComponent<FinalBossSpriteAnimator>() : null;
                    Require(animation != null && animation.HasFrames && mobileBoss.sprite != null
                        && mobileBoss.sprite.name.StartsWith(animation.CurrentPose.ToString(), StringComparison.Ordinal),
                        "Phase three did not animate the dedicated cerebrum-boss artwork");
                    startPosition = phaseThree.Position;
                    player.SpawnAt(FinalBossArena.Instance.UVToWorld(new Vector2(.25f, .38f)));
                    Physics.SyncTransforms();
                    nextAction = Time.time + .8f;
                    stage = 13;
                    return;
                }
                if (stage == 13)
                {
                    FinalBossPhaseTwoController encounter = FinalBossArena.Instance
                        .GetComponent<FinalBossPhaseTwoController>();
                    Vector3 position = encounter.PhaseThree.Position;
                    Require((position - startPosition).sqrMagnitude > .1f,
                        "Detached boss did not pursue the player");
                    Require((position - player.transform.position).sqrMagnitude
                        < (startPosition - player.transform.position).sqrMagnitude,
                        "Detached boss moved away from the player");
                    Vector3 bodyDelta = encounter.BossDamageTarget.transform.position - position;
                    bodyDelta.y = 0f;
                    Require(bodyDelta.sqrMagnitude < .01f,
                        "Phase-three damage target did not follow the visible body");
                    Capture(Camera.main, "gameplay-phase-three-pursuit.png");
                    if (spriteAnimationSmoke)
                    {
                        CaptureOverview(FinalBossArena.Instance, "arena-overview-phase-three-pursuit.png");
                        if (encounter.IsReflecting) return;
                        if (victorySmoke)
                        {
                            encounter.BossDamageTarget.TakeDamage(encounter.Health - encounter.MaxHealth * .05f);
                            stage = 15;
                            nextAction = Time.time + .3f;
                            return;
                        }
                        encounter.BossDamageTarget.TakeDamage(encounter.MaxHealth * 2f);
                        nextAction = Time.time + 1.8f;
                        stage = 14;
                        return;
                    }
                    Finish(null);
                    return;
                }
                if (stage == 14)
                {
                    SpriteRenderer corpse = GameObject.Find("DormantCerebrum")?.GetComponent<SpriteRenderer>();
                    Require(corpse != null && corpse.gameObject.activeInHierarchy
                        && corpse.sprite != null && corpse.sprite.name == "Death_03",
                        "Terminal death artwork did not survive combat-root cleanup");
                    Require(corpse.GetComponent<FinalBossSpriteAnimator>().CurrentPose == FinalBossSpriteAnimator.Pose.Death,
                        "Death animation was replaced by a living pose");
                    Require(FinalBossArena.Instance.GetComponent<FinalBossPhaseOneController>()
                        .Pillars.All(pillar => !pillar.RemnantVisible), "Pillar remnants reappeared in phase three");
                    Capture(Camera.main, "gameplay-phase-three-death.png");
                    CaptureOverview(FinalBossArena.Instance, "arena-overview-phase-three-death.png");
                    if (victorySmoke)
                    {
                        Require(!FinalBossArena.Instance.GetComponent<FinalBossVictoryPrompt>().IsOpen,
                            "Victory prompt appeared before five seconds");
                        stage = 16;
                        return;
                    }
                    Finish(null);
                    return;
                }
                if (stage == 15)
                {
                    FinalBossPhaseTwoController encounter = FinalBossArena.Instance.GetComponent<FinalBossPhaseTwoController>();
                    FinalBossPhaseThreeController phase = encounter.PhaseThree;
                    Require(phase.IsTimeAttack, "Low health did not start terminal collapse");
                    const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
                    typeof(FinalBossPhaseThreeController).GetField("collapseElapsed", flags).SetValue(phase, 25f);
                    typeof(FinalBossPhaseThreeController).GetMethod("UpdateCollapse", flags).Invoke(phase, null);
                    var neuralEdge = (FinalBossNeuralVfx)typeof(FinalBossPhaseThreeController).GetField("neuralBoundary", flags).GetValue(phase);
                    neuralEdge.RenderAt(25f);
                    var overlays = (FinalBossTelegraph[])typeof(FinalBossPhaseThreeController).GetField("collapsedFloor", flags).GetValue(phase);
                    Require(overlays.Length == 4 && overlays.All(overlay => overlay != null
                        && overlay.GetComponentsInChildren<LineRenderer>(true).Length == 0),
                        "Collapsed floor still creates dark warning outlines");
                    CaptureOverview(FinalBossArena.Instance, "arena-shrinking-without-seams.png");
                    if (encounter.IsReflecting) return;
                    encounter.BossDamageTarget.TakeDamage(encounter.MaxHealth * 2f);
                    victoryRealtime = Time.realtimeSinceStartup;
                    stage = 14;
                    nextAction = Time.time + 1.8f;
                    return;
                }
            }
            catch (Exception e) { Finish(e.ToString()); }
        }

        private static void TickVictoryPrompt()
        {
            if (stage == 16)
            {
                FinalBossVictoryPrompt prompt = FinalBossArena.Instance.GetComponent<FinalBossVictoryPrompt>();
                float sinceVictory = Time.realtimeSinceStartup - victoryRealtime;
                if (sinceVictory < 4.9f)
                {
                    Require(!prompt.IsOpen, "Prompt opened before its five-second delay");
                    return;
                }
                if (!prompt.IsOpen) return;
                Require(sinceVictory >= 5f && sinceVictory < 6.5f, "Incorrect victory-prompt delay: " + sinceVictory);
                Require(Time.timeScale == 0f && !PlayerController.Instance.enabled, "Victory prompt did not block gameplay input");
                Require(prompt.YesButton.interactable && prompt.NoButton.interactable, "Victory choices are not usable");
                CapturePrompt(prompt);
                prompt.NoButton.onClick.Invoke();
                Require(!prompt.IsOpen && Time.timeScale == 1f && PlayerController.Instance.enabled
                    && SceneManager.GetActiveScene().name == SceneLoader.SCENE_FINAL_BOSS,
                    "No did not restore gameplay in the completed arena");
                prompt.Schedule();
                dismissedRealtime = Time.realtimeSinceStartup;
                stage = 17;
                return;
            }
            if (stage == 17)
            {
                if (Time.realtimeSinceStartup - dismissedRealtime < 5.3f) return;
                Require(!FinalBossArena.Instance.GetComponent<FinalBossVictoryPrompt>().IsOpen,
                    "Declined victory prompt appeared again");
                // Exercise the other button on a separate fixture after verifying No.
                FinalBossVictoryPrompt yesFixture = new GameObject("VictoryYesBranchFixture").AddComponent<FinalBossVictoryPrompt>();
                typeof(FinalBossVictoryPrompt).GetMethod("Show", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(yesFixture, null);
                yesFixture.YesButton.onClick.Invoke();
                stage = 18;
                return;
            }
            if (stage == 18)
            {
                if (SceneManager.GetActiveScene().name != SceneLoader.SCENE_MAIN_MENU) return;
                Require(PlayerController.Instance == null && GameManager.Instance == null && Time.timeScale == 1f,
                    "Yes did not clean up the gameplay session");
                Require(SaveService.Profile.normalCampaignCompleted && SaveService.Profile.hardUnlocked
                    && SaveService.Profile.normalClearCount == 1, "Victory completion/progress changed on return to menu");
                Finish(null);
            }
        }

        private static void CapturePrompt(FinalBossVictoryPrompt prompt)
        {
            Canvas canvas = prompt.PromptCanvas;
            RenderMode previousMode = canvas.renderMode;
            Camera previousCamera = canvas.worldCamera;
            float previousDistance = canvas.planeDistance;
            int previousOrder = canvas.sortingOrder;
            try
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = Camera.main;
                canvas.planeDistance = 1f;
                // Screen-space overlay draws after world sprites. Match that order when
                // temporarily rendering the UI through the game camera for the capture.
                canvas.sortingOrder = short.MaxValue;
                Canvas.ForceUpdateCanvases();
                Capture(Camera.main, "victory-confirmation.png");
            }
            finally
            {
                canvas.renderMode = previousMode;
                canvas.worldCamera = previousCamera;
                canvas.planeDistance = previousDistance;
                canvas.sortingOrder = previousOrder;
            }
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
            Require(map.AuthoredLayout.environmentAtlas != null, "Reference environment atlas missing");
            UnityEngine.Tilemaps.Tilemap floor = arena.gameObject.scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>())
                .First(tilemap => tilemap.name == "Base Tilemap");
            Require(floor.GetTile(new Vector3Int(24, 19, 0)) != floor.GetTile(new Vector3Int(25, 19, 0)),
                "Authored environment must use position-specific tiles");
            Require(BiomeManager.Active == arena.GetComponent<ProceduralBiomeBridge>(), "Shared biome bridge not active");
            Require(player.GetComponent<ProceduralTerrainMotor>().HasActiveMap, "Common terrain motor not bound");
            FinalBossPhaseOneController phase = arena.GetComponent<FinalBossPhaseOneController>();
            Require(phase != null && phase.Pillars != null && phase.Pillars.Count == 4,
                "Final boss phase-one controller or pillars missing");
            Require(phase.ConfiguredBiomeCount == 4,
                "All four source biome enemy rules are required for phase-one reinforcements");
            Require(phase.CommonRuleCount == 3,
                "The three shared phase-one enemy rules are required");
            foreach (FinalBossPillar pillar in phase.Pillars)
                Require(phase.GetExclusiveRuleCount(pillar.Biome) == 3,
                    $"{pillar.Biome} must provide three biome-exclusive enemy rules");
            GameObject dormantBoss = GameObject.Find("DormantCerebrum");
            Require(phase.CurrentPhase == 1 && phase.BossVisible && dormantBoss != null
                && dormantBoss.GetComponent<EnemyController>() == null,
                "Phase one must show the dormant boss without making it a combat target");
            foreach (FinalBossPillar pillar in phase.Pillars)
            {
                Require(pillar != null && pillar.DamageTarget != null && pillar.MaxHealth > 0f,
                    "Pillar combat target/health missing");
                Require(pillar.IntactSprite != null && pillar.BrokenSprite != null
                    && pillar.CurrentSprite == pillar.IntactSprite,
                    "Pillar isolated intact/broken sprite set is missing");
                Require(pillar.CurrentSprite.texture.name.Contains("Pillar_"),
                    "Pillar still uses a rectangular crop of the arena background");
            }
            Require(UnityEngine.Object.FindObjectsByType<FinalBossWorldHealthBar>(FindObjectsSortMode.None).Length == 4,
                "One health bar is required above each pillar");
            VerifyPhaseOneMinionComposition(phase);
            phase.PauseSpawningForTest();
            Require(GameObject.Find("DormantCerebrum_MapArtwork") == null, "Old flattened map still present");
            Require(arena.GetComponent<WorldItemSpawner>() == null, "Unexpected automatic item spawner");
            Require(arena.GetComponentInChildren<ReturnPortal>(true) == null
                && GameObject.Find("ReturnToHub_Entrance") == null,
                "Final-boss room must not have a walk-out return portal");
            SpriteOutline[] outlines = arena.GetComponentsInChildren<SpriteOutline>(true);
            Require(outlines.Length == 5, "Four pillar outlines and cerebrum outline required");
            foreach (SpriteOutline outline in outlines)
                if (outline.gameObject.activeInHierarchy)
                {
                    if (outline.name.StartsWith("Pillar_", StringComparison.Ordinal))
                    {
                        Require(!outline.enabled, "Transparent pillar sprites must not use the old cropped outline mesh");
                        continue;
                    }
                    Sprite sprite = outline.GetComponent<SpriteRenderer>().sprite;
                    Require(sprite != null && sprite.vertices.Length > 4,
                        "Sprite outline was lost after reload: " + outline.name);
                }
            Require(UnityEngine.Object.FindObjectsByType<UnityEngine.Tilemaps.Tilemap>(FindObjectsSortMode.None)
                .Length >= 4, "Shared tilemap layers missing");
        }

        private static void VerifyPhaseOneMinionComposition(FinalBossPhaseOneController phase)
        {
            EnemyController[] enemies = UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            EnemyController[] common = enemies
                .Where(enemy => enemy.name.StartsWith("FinalBoss_Common_", StringComparison.Ordinal))
                .ToArray();
            Require(common.Length == 4, "Phase one must keep a four-enemy shared wave active");
            Require(common.Select(enemy => enemy.Config.name).Distinct().Count() == 3,
                "The initial shared wave must draw from all three shared enemy types");

            foreach (FinalBossPillar pillar in phase.Pillars)
            {
                string prefix = $"FinalBoss_{pillar.Biome}_BiomeMinion_";
                EnemyController[] biomeEnemies = enemies
                    .Where(enemy => enemy.name.StartsWith(prefix, StringComparison.Ordinal))
                    .ToArray();
                Require(biomeEnemies.Length == 4,
                    $"{pillar.Biome} must spawn two copies of two exclusive enemy types");
                int[] copiesPerType = biomeEnemies.GroupBy(enemy => enemy.Config.name)
                    .Select(group => group.Count()).OrderBy(count => count).ToArray();
                Require(copiesPerType.SequenceEqual(new[] { 2, 2 }),
                    $"{pillar.Biome} exclusive group must contain exactly two types with two copies each");
                biomeEnemies[0].ReleaseToPool();
            }

            foreach (EnemyController enemy in common) enemy.ReleaseToPool();
            phase.ReplenishMinionsForTest();
            EnemyController[] replenished = UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None)
                .Where(enemy => enemy.name.StartsWith("FinalBoss_Common_", StringComparison.Ordinal))
                .ToArray();
            Require(replenished.Length == 4 && replenished.Select(enemy => enemy.Config.name).Distinct().Count() == 3,
                "Shared phase-one minions were not continuously replenished as a four-enemy wave");
            foreach (FinalBossPillar pillar in phase.Pillars)
            {
                string prefix = $"FinalBoss_{pillar.Biome}_BiomeMinion_";
                EnemyController[] biomeEnemies = UnityEngine.Object
                    .FindObjectsByType<EnemyController>(FindObjectsSortMode.None)
                    .Where(enemy => enemy.name.StartsWith(prefix, StringComparison.Ordinal))
                    .ToArray();
                int[] copiesPerType = biomeEnemies.GroupBy(enemy => enemy.Config.name)
                    .Select(group => group.Count()).OrderBy(count => count).ToArray();
                Require(biomeEnemies.Length == 4 && copiesPerType.SequenceEqual(new[] { 2, 2 }),
                    $"{pillar.Biome} exclusive minions were not replenished after one was removed");
            }
        }

        private static void VerifyOnlyRemainingBiomesRespawn(
            FinalBossPhaseOneController phase, BiomeType destroyedBiome)
        {
            phase.ReplenishMinionsForTest();
            EnemyController[] enemies = UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            Require(enemies.Count(enemy => enemy.name.StartsWith("FinalBoss_Common_", StringComparison.Ordinal)) == 4,
                "Shared minions stopped respawning after a biome pillar was destroyed");

            foreach (FinalBossPillar pillar in phase.Pillars)
            {
                string prefix = $"FinalBoss_{pillar.Biome}_BiomeMinion_";
                int count = enemies.Count(enemy => enemy.name.StartsWith(prefix, StringComparison.Ordinal));
                int expected = pillar.Biome == destroyedBiome ? 0 : 4;
                Require(count == expected,
                    $"{pillar.Biome} respawn count was {count}; expected {expected} after destroying {destroyedBiome}");
            }
        }

        private static void VerifyGeometry(FinalBossArena arena)
        {
            Require(arena.IsWalkable(arena.SpawnPosition, Footprint), "Spawn blocked");
            Require(arena.CanTraverse(arena.SpawnPosition, arena.UVToWorld(new Vector2(.5f, .70f)), Footprint), "Central route blocked");
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

        private static void CaptureOverview(FinalBossArena arena, string filename = "arena-overview.png")
        {
            var go = new GameObject("OverviewCapture");
            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 14.5f;
            camera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            camera.transform.position = arena.UVToWorld(new Vector2(.5f, .5f)) + new Vector3(0f, 40f, -40f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.08f, .055f, .1f);
            Capture(camera, filename);
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
            try
            {
                if (sceneSetup != null && sceneSetup.Any(item => item.isLoaded))
                    EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
                else
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
            catch (Exception restoreError)
            {
                failure ??= "Scene restore failed after test: " + restoreError;
            }
            string result = failure ?? "PASS: all 15 incomplete clear combinations blocked; four-clear Hub trigger entered FinalBoss; walk-out return portal absent; visible but untargetable dormant boss, four shared minions, two-by-two biome-exclusive groups, four isolated transparent pillar sprites, hit/health behavior, authored broken-state swaps, weakened mid-boss at doubled room size and 33% combat stats, and phase-two damage target plus floor-warning attack activation verified; spawn, connected floor, pillar/wall collision and actual player movement verified.";
            if (directPreview && failure == null)
                result = "PASS: direct FinalBoss scene Play creates the actual player and camera; correct spawn, visible animation, actual movement, no walk-out return portal, and no boss-progress unlock verified.";
            if (spriteAnimationSmoke && failure == null)
                result = "PASS: broken pillars visible in phase one and hidden in phases two/three; preserved-health 40% detachment; animated mobile artwork, pursuit and eight hitboxes; final Death_03 corpse survives combat-root cleanup. Isolated test saves used.";
            if (victorySmoke && failure == null)
                result = "PASS: four shrinking-floor overlays have no dark outline renderers; victory dialog appears after five real seconds; No restores input and stays in the arena without reopening; Yes loads MainMenu and cleans up gameplay; saved completion and Hard unlock preserved. Isolated test saves used.";
            File.WriteAllText(Output + (directPreview ? "/direct-smoke-result.txt" : "/smoke-result.txt"), result);
            File.WriteAllText("Library/FinalBossSmoke.result", result);
            Debug.Log("[FinalBossSmoke] " + result);
            if (spriteAnimationSmoke && Application.isBatchMode)
                EditorApplication.delayCall += () => EditorApplication.Exit(failure == null ? 0 : 1);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
