using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NecrocisEditor
{
    // Batch-only integration checks: actual trigger contact, reward UI and isolated persistence.
    public static class WorldItemBoxPickupChecks
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static bool passed, started, oldOptionsEnabled;
        private static EnterPlayModeOptions oldOptions;
        private static double deadline;
        private static int assertions;
        private static string storageRoot;

        public static void Run()
        {
            if (!Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Run in an isolated Unity batch project.");
            passed = started = false;
            assertions = 0;
            oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            oldOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            storageRoot = Path.GetFullPath("tmp/box-pickup-check-" + Guid.NewGuid().ToString("N"));
            SaveService.UseStorageRootForTests(storageRoot);
            GameplayFocusOwner.TestFocusOverride = true;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            deadline = EditorApplication.timeSinceStartup + 120;
            EditorApplication.playModeStateChanged += OnState;
            EditorApplication.update += Tick;
            EditorApplication.isPlaying = true;
        }

        private static void Tick()
        {
            if (!started && EditorApplication.isPlaying && !EditorApplication.isCompiling && Time.frameCount > 2)
            {
                started = true;
                Execute();
                return;
            }
            if (EditorApplication.timeSinceStartup < deadline) return;
            Debug.LogError("WorldItemBoxPickupChecks timed out.");
            Finish();
        }

        private static void Execute()
        {
            var simulation = Physics.simulationMode;
            try
            {
                SessionRuntimeBindings.Register();
                Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out var error), "new isolated run: " + error);
                InputManager.Instance.Focus.SetNativeFocused(true);
                InputManager.ResetSessionClock();
                var playerObject = new GameObject("BoxCheckPlayer");
                PlayerCompositionRoot.EnsureGameplay(playerObject);
                var player = playerObject.GetComponent<PlayerController>();
                player.enabled = false; // Keep the fixture still while driving physics explicitly.
                var inventory = playerObject.GetComponent<PlayerItemManager>();
                inventory.PopulateBasicProjectileTemplateItems();
                var ui = playerObject.AddComponent<PlayerItemInventoryUI>();
                var playerCollider = playerObject.GetComponent<Collider>();
                Require(ui.CanRequestReward, "reward UI is available before contact");

                var camera = new GameObject("BoxCheckCamera").AddComponent<Camera>();
                camera.tag = "MainCamera";
                camera.transform.rotation = Quaternion.Euler(60, 0, 0);
                var spawner = new GameObject("BoxCheckSpawner").AddComponent<WorldItemSpawner>();
                spawner.SetAutoSpawnOnStart(false);
                Sprite closed = MakeSprite(Color.yellow), opened = MakeSprite(Color.green);
                Set(spawner, "itemBoxClosedSprite", closed);
                Set(spawner, "itemBoxOpenSprite", opened);

                var firstReward = Reward("box.first");
                var first = Box(spawner, inventory, firstReward, Vector3.zero);
                var art = first.GetComponentInChildren<SpriteRenderer>();
                var billboard = art.GetComponent<Billboard>();
                Invoke(billboard, "Start");
                Invoke(billboard, "LateUpdate");
                Require(first.GetComponent<Billboard>() == null, "billboard affects artwork only");
                Require(first.transform.position == Vector3.zero && first.transform.rotation == Quaternion.identity,
                    "camera tilt and artwork height cannot move or rotate trigger");
                Require(Quaternion.Angle(art.transform.rotation, camera.transform.rotation) < .01f,
                    "artwork still faces the camera");

                Physics.simulationMode = SimulationMode.Script;
                Physics.SyncTransforms();
                Physics.Simulate(.02f);
                Require(ui.IsOpen, "actual first trigger contact opens selection immediately");
                Require(art.sprite == opened, "chest opens with the selection UI");
                Require(first.transform.Find("RevealedItem") == null, "no world item reveal object");
                Require(!firstReward.collected && inventory.ItemCount == 0, "contact alone does not grant an item");
                Require(Time.timeScale == 0f, "selection pauses gameplay");
                Invoke(first, "OnTriggerStay", playerCollider);
                Invoke(first, "OnTriggerEnter", playerCollider);
                Require(inventory.ItemCount == 0, "repeated contact cannot auto-grant a reward");
                Invoke(ui, "SelectReward", "dense_muscle");
                Require(firstReward.collected && inventory.ItemCount == 1 && inventory.ContainsItem("dense_muscle"),
                    "selecting one offer grants exactly that item and consumes chest");
                Invoke(ui, "SelectReward", "dense_muscle");
                Invoke(first, "OnTriggerStay", playerCollider);
                Require(inventory.ItemCount == 1 && !ui.IsOpen, "consumed chest cannot grant twice");
                Require(Time.timeScale == 1f, "selection restores gameplay clock");

                var secondReward = Reward("box.cancel");
                var second = Box(spawner, inventory, secondReward, Vector3.right * 10);
                var stranger = new GameObject("NotPlayer").AddComponent<BoxCollider>();
                Invoke(second, "OnTriggerEnter", stranger);
                Require(!ui.IsOpen, "non-player collider cannot open chest");
                Invoke(second, "OnTriggerEnter", playerCollider);
                Require(ui.IsOpen, "next chest opens on first contact");
                var choices = string.Join(",", secondReward.itemChoices);
                ui.SetOpen(false);
                Invoke(second, "OnTriggerStay", playerCollider);
                Require(!ui.IsOpen && !secondReward.collected, "closing preserves reward without reopening every physics tick");
                Invoke(second, "OnTriggerExit", playerCollider);
                Invoke(second, "OnTriggerEnter", playerCollider);
                Require(ui.IsOpen && string.Join(",", secondReward.itemChoices) == choices,
                    "reapproaching cancelled chest reopens the same choices");
                Invoke(ui, "SelectReward", "vital_cell");
                Require(secondReward.collected && inventory.ItemCount == 2, "reopened chest can be collected once");

                var thirdReward = Reward("box.blocked");
                var third = Box(spawner, inventory, thirdReward, Vector3.right * 20);
                // Saved choice rewards must work without a legacy single-item identifier.
                third.Initialize(string.Empty, closed, opened, 250);
                using (InputManager.Instance.Contexts.Register(new object(), 999, true, allowAuxiliary: false))
                {
                    Invoke(third, "OnTriggerEnter", playerCollider);
                    Require(!ui.IsOpen && third.GetComponentInChildren<SpriteRenderer>().sprite == closed,
                        "blocked request keeps chest available and closed");
                }
                Invoke(third, "OnTriggerStay", playerCollider);
                Require(ui.IsOpen, "stay retries blocked contact without leaving and reentering");
                Invoke(ui, "SelectReward", "active_fiber");
                Require(thirdReward.collected && inventory.ItemCount == 3, "bound reward works without a single-item id");

                var saved = JsonUtility.FromJson<RunSaveData>(File.ReadAllText(Path.Combine(storageRoot, "Saves", "normal.json")));
                Require(saved.player.items.Count == 3, "selected items are saved exactly once");
                foreach (var reward in new[] { firstReward, secondReward, thirdReward })
                    Require(saved.world.rewards.Exists(value => value.id == reward.id && value.collected),
                        "consumed state persisted: " + reward.id);
                var consumed = Box(spawner, inventory, firstReward, Vector3.right * 30);
                Invoke(consumed, "OnTriggerEnter", playerCollider);
                Require(!ui.IsOpen && inventory.ItemCount == 3, "restored consumed chest cannot reopen");
                Debug.Log("WORLD_ITEM_BOX_PICKUP_PASS assertions=" + assertions);
                passed = true;
            }
            catch (Exception error) { Debug.LogException(error); }
            finally
            {
                Physics.simulationMode = simulation;
                EditorApplication.isPlaying = false;
            }
        }

        private static WorldRewardSaveData Reward(string id) => SaveService.RememberWorldReward(new WorldRewardSaveData
        {
            id = id,
            itemChoices = new List<string> { "dense_muscle", "vital_cell", "active_fiber" }
        });
        private static WorldItemBoxPickup Box(WorldItemSpawner spawner, PlayerItemManager inventory,
            WorldRewardSaveData reward, Vector3 position)
        {
            inventory.TryGetItemEntry("dense_muscle", out var entry);
            var box = (WorldItemBoxPickup)Invoke(spawner, "SpawnItemObject", entry, position);
            box.BindReward(reward);
            return box;
        }
        private static Sprite MakeSprite(Color color)
        {
            var texture = new Texture2D(2, 2);
            texture.SetPixels(new[] { color, color, color, color }); texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.one * .5f, 2);
        }
        private static object Invoke(object target, string name, params object[] values)
            => target.GetType().GetMethod(name, Private).Invoke(target, values);
        private static void Set(object target, string name, object value)
            => target.GetType().GetField(name, Private).SetValue(target, value);
        private static void Require(bool value, string label)
        {
            assertions++;
            if (!value) throw new InvalidOperationException(label);
        }
        private static void OnState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode) Finish();
        }
        private static void Finish()
        {
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnState;
            EditorSettings.enterPlayModeOptionsEnabled = oldOptionsEnabled;
            EditorSettings.enterPlayModeOptions = oldOptions;
            GameplayFocusOwner.TestFocusOverride = null;
            SaveService.ResetStaticStateForTests();
            EditorApplication.Exit(passed ? 0 : 1);
        }
    }
}
