using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static class MergeIntegrationChecks
    {
        [MenuItem("Necrocis/Balance/Verify Main Integration")]
        public static void Run()
        {
            var report = new List<string>();
            var catalog = AssetDatabase.LoadAssetAtPath<MonsterBalanceCatalog>(MonsterBalanceSetup.CatalogPath);
            Require(catalog != null && catalog.GetValidationErrors().Count == 0, "monster catalog validation");
            var normalConfigs = new Dictionary<string, EnemySpawnConfig>();
            var definitions = catalog.monsters.ToDictionary(d => d.monsterId, d => EditorJsonUtility.ToJson(d));
            foreach (string biome in new[] { "Intestine", "Liver", "Stomach", "Lung" })
            {
                var config = AssetDatabase.LoadAssetAtPath<EnemySpawnConfig>("Assets/_Project/Data/BiomeConfigs/" + biome + "EnemySpawnConfig.asset");
                normalConfigs.Add(biome, config);
                var exclusive = config.GetEnemySpawnRules().Where(r => r.name.StartsWith(biome, StringComparison.Ordinal)).ToArray();
                Require(exclusive.Length == 3 && exclusive.All(r => r.monsterDefinition != null
                    && r.monsterDefinition.tier == MonsterTier.Normal && catalog.monsters.Contains(r.monsterDefinition)), biome + " normal definitions");
                var elites = AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>(BiomeEliteSetup.Root + "/" + biome + "BiomeEliteSpawnConfig.asset");
                Require(elites.monsters.Count == 2 && elites.minimumCount == 2 && elites.maximumCount == 2
                    && elites.minimumSpacing == 64 && elites.monsters.All(r => r.monsterDefinition.tier == MonsterTier.Elite), biome + " elite population preserved");
                report.Add("PASS " + biome + ": three normal species and two dedicated elite species retained");
            }
            string[] legacyNames = { "Macrophage", "NKCell", "BCell", "Granuloma", "Antibody" };
            Require(legacyNames.All(n => normalConfigs["Intestine"].GetEnemySpawnRules().Any(r => r.name == n)), "legacy intestine species retained");
            var expansion = normalConfigs["Intestine"].GetEnemySpawnRules().Where(r => r.expandColliderOnAttack).Select(r => r.name).OrderBy(n => n).ToArray();
            Require(expansion.SequenceEqual(new[] { "Granuloma", "Macrophage" }), "existing collider expansion flags retained");
            var beforeRules = normalConfigs.ToDictionary(p => p.Key, p => p.Value.GetEnemySpawnRules().Select(r => r.name).OrderBy(n => n).ToArray());
            BiomeEnemySetup.Run();
            foreach (var pair in normalConfigs)
                Require(beforeRules[pair.Key].SequenceEqual(pair.Value.GetEnemySpawnRules().Select(r => r.name).OrderBy(n => n)), "generator preserves " + pair.Key + " roster");
            foreach (var definition in catalog.monsters)
                Require(definitions[definition.monsterId] == EditorJsonUtility.ToJson(definition), "generator preserves definition tuning: " + definition.monsterId);
            report.Add("PASS generator rerun preserves existing rosters, MonsterDefinition references and tuning");

            var fixture = new GameObject("FinalBossBalanceIntegrationFixture");
            fixture.SetActive(false);
            FinalBossBalanceConfig balance = null;
            try
            {
                var arena = fixture.AddComponent<FinalBossArena>();
                balance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<FinalBossBalanceConfig>(MergeIntegrationSetup.FinalBossPath));
                Require(balance != null && balance.GetValidationError() == null, "final boss standalone settings");
                typeof(FinalBossArena).GetField("runtimeBalance", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(arena, balance);
                var boss = catalog.monsters.First(d => d.monsterId == "boss.stomach");
                boss.TryGetStatSet("Default", out MonsterStatSet source);
                float originalRatio = balance.phaseOne.weakenedMidBossStatRatio;
                FinalBossDifficultyBalance difficulty = balance.GetDifficulty(SaveService.ActiveDifficulty);
                MonsterBalanceRuntime weak = arena.CaptureSummonBalance(boss, true);
                Require(Mathf.Abs(weak.Current.MaxHealth - source.maxHealth * originalRatio * difficulty.health) < .001f
                    && Mathf.Abs(weak.Current.AttackPower - source.attackPower * originalRatio * difficulty.damage) < .001f
                    && weak.Current.Experience == 0 && weak.Current.Stage == 0, "weak summon uses definition, own multipliers, no reward or biome progression");
                float changedRatio = originalRatio < .6f ? .75f : .5f;
                balance.phaseOne.weakenedMidBossStatRatio = changedRatio;
                MonsterBalanceRuntime changed = arena.CaptureSummonBalance(boss, true);
                Require(Mathf.Abs(changed.Current.MaxHealth - source.maxHealth * changedRatio * difficulty.health) < .001f
                    && Mathf.Abs(weak.Current.MaxHealth - source.maxHealth * originalRatio * difficulty.health) < .001f, "encounter values freeze per spawn");
                balance.phaseOne.commonMinionsPerWave = 0;
                Require(balance.GetValidationError() != null, "invalid final boss counts rejected");
                report.Add("PASS final boss independent settings, definition-based weak summons, spawn snapshots and validation");
            }
            finally
            {
                Object.DestroyImmediate(fixture);
                if (balance != null) Object.DestroyImmediate(balance);
            }
            Directory.CreateDirectory("Logs");
            File.WriteAllLines("Logs/MergeIntegrationChecks.txt", report);
            Debug.Log("[MergeIntegrationChecks] PASS: " + report.Count + " groups");
        }

        public static void VerifyGrabInput(PlayerController player)
        {
            player.HealthComponent.ResetHealth();
            Require(player.BeginBossDisplacement(), "grab input fixture starts displacement");
            var attack = player.GetComponent<PlayerAttack>();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var meleeTime = typeof(PlayerAttack).GetField("lastMeleeTime", flags);
            var rangedTime = typeof(PlayerAttack).GetField("lastAttackTime", flags);
            float beforeMelee = (float)meleeTime.GetValue(attack), beforeRanged = (float)rangedTime.GetValue(attack);
            InputAction melee = InputManager.Instance.MeleeAttackAction, ranged = InputManager.Instance.RangedAttackAction;
            bool meleeEnabled = melee.enabled, rangedEnabled = ranged.enabled;
            string meleeBindings = melee.SaveBindingOverridesAsJson(), rangedBindings = ranged.SaveBindingOverridesAsJson();
            InputSettings originalSettings = InputSystem.settings;
            InputSettings testSettings = Object.Instantiate(originalSettings);
            testSettings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
            testSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings = testSettings;
            MethodInfo update = typeof(InputSystem).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Static,
                null, new[] { typeof(InputUpdateType) }, null);
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            try
            {
                // Respect the user's saved rebinds: override only these in-memory test actions and restore below.
                melee.ApplyBindingOverride(0, "<Keyboard>/q");
                ranged.ApplyBindingOverride(0, "<Keyboard>/w");
                melee.Enable(); ranged.Enable();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Q, Key.W));
                update.Invoke(null, new object[] { InputUpdateType.Dynamic });
                Require(InputManager.Instance.MeleeAttackAction.WasPressedThisFrame()
                    && InputManager.Instance.RangedAttackAction.WasPressedThisFrame(),
                    "Q/W input events delivered: keyboard=" + keyboard.qKey.isPressed + "/" + keyboard.wKey.isPressed
                    + ", actions=" + melee.IsPressed() + "/" + ranged.IsPressed());
                typeof(PlayerAttack).GetMethod("HandleAttackInput", flags).Invoke(attack, null);
                Require((float)meleeTime.GetValue(attack) == beforeMelee && (float)rangedTime.GetValue(attack) == beforeRanged,
                    "grab blocks both basic attack inputs");
                Require(!player.TryMoveByWorld(Vector3.forward), "grab blocks displacement input");
                var skills = player.GetComponent<PlayerClassSkillController>();
                Require(!(bool)typeof(PlayerClassSkillController).GetMethod("ShouldAcceptInput", flags).Invoke(skills, null), "grab blocks class skills");
            }
            finally
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                update.Invoke(null, new object[] { InputUpdateType.Dynamic });
                InputSystem.RemoveDevice(keyboard);
                melee.LoadBindingOverridesFromJson(meleeBindings);
                ranged.LoadBindingOverridesFromJson(rangedBindings);
                if (!meleeEnabled) melee.Disable();
                if (!rangedEnabled) ranged.Disable();
                InputSystem.settings = originalSettings;
                Object.Destroy(testSettings);
                player.EndBossDisplacement();
            }
            Debug.Log("[MergeIntegrationChecks] PASS: grabbed player rejects Q/W, movement and class skill input");
        }

        private static void Require(bool ok, string message)
        {
            if (!ok) throw new InvalidOperationException(message);
        }
    }
}
