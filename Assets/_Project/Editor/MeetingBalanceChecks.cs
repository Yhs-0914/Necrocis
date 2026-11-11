using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    // Run in an empty batch editor; uses an isolated save directory and temporary objects.
    public static class MeetingBalanceChecks
    {
        private static int assertions;
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        public static void Run()
        {
            int exit = 1;
            var temporary = new List<GameObject>();
            try
            {
                if (!Application.isBatchMode || Application.isPlaying)
                    throw new InvalidOperationException("Run with Unity -batchmode -executeMethod NecrocisEditor.MeetingBalanceChecks.Run.");
                SaveService.UseStorageRootForTests(Path.GetFullPath("tmp/meeting-balance-saves"));
                var savedProgress = new PlayerProgressSaveData { level = 1, job = JobType.Mage };
                LevelUpManager.RestoreProgress(savedProgress);
                var go = new GameObject("MeetingInventoryCheck");
                temporary.Add(go);
                var player = go.AddComponent<PlayerStats>();
                player.EnsureInitialized();
                CheckLevelUpGrowth(player, go);
                var inventory = go.AddComponent<PlayerItemManager>();
                Invoke(inventory, "Awake");
                inventory.PopulateBasicProjectileTemplateItems();
                Equal(68, inventory.ItemEntries.Count, "catalog contains 68 items");
                float baseAttack = player.AttackPower, baseHealth = player.MaxHealth;
                for (int i = 0; i < 12; i++) Require(inventory.TryAcquireItem("dense_muscle", out _), "repeatable pickup " + i);
                Require(!inventory.IsFull && inventory.ItemCount == 12, "inventory exceeds old three-slot cap");
                Equal(baseAttack + 12, player.AttackPower, "repeatable attack bonuses stack");
                Require(inventory.TryAcquireItem("hardened_muscle", out _), "attack +2 pickup");
                Equal(baseAttack + 14, player.AttackPower, "attack +2 applies");
                Require(inventory.TryAcquireItem("vital_cell", out _), "health +1 pickup");
                Require(inventory.TryAcquireItem("vital_organ", out _), "health +2 pickup");
                Equal(baseHealth + 3, player.MaxHealth, "health bonuses stack");
                float baseSpeed = player.AttackSpeed, baseMove = player.MoveSpeed;
                Require(inventory.TryAcquireItem("swift_cell", out _), "attack speed pickup");
                Require(inventory.TryAcquireItem("elastic_fiber", out _), "move speed pickup");
                Equal(baseSpeed * 1.15f, player.AttackSpeed, "attack speed +15 percent");
                Equal(baseMove + .5f, player.MoveSpeed, "move speed +0.5");
                Require(inventory.TryAcquireItem("double_core", out _), "unique pickup");
                Require(!inventory.TryAcquireItem("double_core", out var reason) && reason == PlayerItemAcquireFailureReason.DuplicateNotAllowed,
                    "special items remain unique");
                var savedItems = inventory.CaptureSavedItems();
                inventory.ClearAllItems();
                Equal(baseAttack, player.AttackPower, "clear removes stat bonuses");
                Equal(baseHealth, player.MaxHealth, "clear removes health bonuses");
                inventory.RestoreSavedItems(savedItems);
                Equal(savedItems.Count, inventory.ItemCount, "all saved items restore beyond three slots");
                Equal(baseAttack + 14, player.AttackPower, "save restoration applies bonuses once");
                Require(inventory.RemoveItem("dense_muscle"), "remove one repeated item");
                Equal(baseAttack + 13, player.AttackPower, "removing one stack leaves other stacks intact");

                var enemyGo = new GameObject("MeetingEnemyHealthCheck");
                temporary.Add(enemyGo);
                var enemy = enemyGo.AddComponent<EnemyController>();
                var stats = enemyGo.AddComponent<CharacterStats>();
                typeof(EnemyController).GetField("stats", Private).SetValue(enemy, stats);
                typeof(EnemyController).GetField("config", Private).SetValue(enemy, new EnemySpawnRuleConfig { name = "Check", maxHealth = 10 });
                stats.ConfigureBaseStats(new CharacterStatValue(CharacterStatType.MaxHealth, 10));
                Invoke(enemy, "InitializePlayerLevelHealthScaling");
                stats.ApplyDamage(5);
                LevelUpManager.RestoreProgress(new PlayerProgressSaveData { level = 6, job = JobType.Mage });
                Invoke(enemy, "RefreshPlayerLevelHealth");
                Equal(11.5f, stats.MaxHealth, "already-alive enemy gains health at level six");
                Equal(5.75f, stats.CurrentHealth, "injured fraction preserved on level change");
                Invoke(enemy, "RefreshPlayerLevelHealth");
                Equal(11.5f, stats.MaxHealth, "same band cannot compound");
                LevelUpManager.RestoreProgress(new PlayerProgressSaveData { level = 26, job = JobType.Mage });
                Invoke(enemy, "RefreshPlayerLevelHealth");
                Equal(17.5f, stats.MaxHealth, "multi-band level jump");
                Equal(8.75f, stats.CurrentHealth, "multi-band injured fraction");
                stats.ClearModifiers();
                stats.ConfigureBaseStats(new CharacterStatValue(CharacterStatType.MaxHealth, 10));
                Invoke(enemy, "InitializePlayerLevelHealthScaling");
                Equal(17.5f, stats.CurrentHealth, "reused spawn initializes at current level with full health");
                typeof(EnemyController).GetField("config", Private).SetValue(enemy,
                    new EnemySpawnRuleConfig { name = "EliteCheck", maxHealth = 10, isElite = true });
                stats.ClearModifiers();
                stats.ConfigureBaseStats(new CharacterStatValue(CharacterStatType.MaxHealth, 10));
                Invoke(enemy, "InitializePlayerLevelHealthScaling");
                Equal(16f, stats.MaxHealth, "elite uses its own health growth rate");
                typeof(EnemyController).GetField("ignoreMidBossArenaRestriction", Private).SetValue(enemy, true);
                stats.ClearModifiers();
                stats.ConfigureBaseStats(new CharacterStatValue(CharacterStatType.MaxHealth, 10));
                Invoke(enemy, "InitializePlayerLevelHealthScaling");
                Equal(10f, stats.MaxHealth, "boss health excludes player level growth");

                var catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath);
                var definitions = AssetDatabase.FindAssets("t:MonsterDefinition", new[] { "Assets/_Project/Data/MonsterBalance/Definitions" })
                    .Select(guid => AssetDatabase.LoadAssetAtPath<MonsterDefinition>(AssetDatabase.GUIDToAssetPath(guid))).ToList();
                foreach (var difficulty in new[] { GameDifficulty.Normal, GameDifficulty.Hard })
                foreach (var definition in definitions.Where(value => value.tier == MonsterTier.Elite))
                {
                    var runtime = new MonsterBalanceRuntime(definition, catalog.difficultyCatalog.Get(difficulty), catalog.progression, 4);
                    float damage = difficulty == GameDifficulty.Hard ? 4 : 3;
                    foreach (var pattern in definition.patternDamage)
                        Equal(pattern.coefficient > 0f ? damage : 0f, runtime.Current.AttackPower * runtime.GetCoefficient(pattern.id), "runtime elite pattern damage");
                    if (runtime.ContactEnabled && definition.contact.damageCoefficient > 0)
                        Equal(damage, runtime.Current.AttackPower * runtime.ContactCoefficient, "runtime elite contact damage");
                }
                var export = inventory.ItemEntries.Select(entry => new ExportItem
                { id = entry.ItemId, name = entry.DisplayName, description = entry.Description,
                    role = CampaignRewardRules.Role(entry.ItemId).ToString(), repeatable = PlayerStatItemCatalog.IsRepeatable(entry.ItemId) }).ToArray();
                Directory.CreateDirectory("output/meeting-balance");
                File.WriteAllText("output/meeting-balance/items.json", JsonUtility.ToJson(new ExportCatalog { items = export }, true));
                File.WriteAllText("output/meeting-balance/unity-checks.txt", "PASS: " + assertions + " Unity runtime assertions. Physics, scene combat and visual UI not exercised.\n");
                Debug.Log("[MeetingBalance] PASS: " + assertions + " assertions.");
                exit = 0;
            }
            catch (Exception error) { Debug.LogException(error); }
            finally
            {
                foreach (var go in temporary) if (go != null) UnityEngine.Object.DestroyImmediate(go);
                SaveService.ResetStaticStateForTests();
                EditorApplication.Exit(exit);
            }
        }

        private static void Invoke(object target, string method) => target.GetType().GetMethod(method, Private).Invoke(target, null);
        private static void CheckLevelUpGrowth(PlayerStats player, GameObject go)
        {
            var ui = go.AddComponent<LevelUpStackChoiceUI>();
            var uiType = typeof(LevelUpStackChoiceUI);
            var choiceType = uiType.GetNestedType("StackStatType", BindingFlags.NonPublic);
            var apply = uiType.GetMethod("ApplyStat", Private);
            foreach (var (type, amount, mode) in new[]
            {
                (CharacterStatType.AttackPower, 1f, CharacterStatModifierMode.Flat),
                (CharacterStatType.AttackSpeed, .2f, CharacterStatModifierMode.PercentAdd),
                (CharacterStatType.AttackRange, .1f, CharacterStatModifierMode.PercentAdd),
                (CharacterStatType.Magic, 25f, CharacterStatModifierMode.Flat),
                (CharacterStatType.MoveSpeed, .05f, CharacterStatModifierMode.PercentAdd),
                (CharacterStatType.MaxHealth, 2f, CharacterStatModifierMode.Flat)
            })
            {
                player.ResetStats();
                player.ResetBaseStats(true);
                LevelUpManager.ResetProgress();
                float initial = player.RuntimeStats.GetValue(type);
                if (type == CharacterStatType.MaxHealth) player.TakeDamage(4f);
                float initialHealth = player.CurrentHealth;
                var choice = Enum.Parse(choiceType, type.ToString());
                apply.Invoke(ui, new[] { choice });
                Equal(mode == CharacterStatModifierMode.Flat ? initial + amount : initial * (1f + amount),
                    player.RuntimeStats.GetValue(type), "actual growth screen applies " + type);
                if (type == CharacterStatType.MaxHealth)
                    Equal(initialHealth + 2f, player.CurrentHealth, "health choice heals exactly one heart");
                apply.Invoke(ui, new[] { choice });
                Equal(mode == CharacterStatModifierMode.Flat ? initial + amount * 2f : initial * (1f + amount * 2f),
                    player.RuntimeStats.GetValue(type), "repeated growth adds without compounding " + type);
                var saved = new PlayerProgressSaveData();
                LevelUpManager.CaptureProgress(saved);
                Require(saved.levelUpModifiers.Count == 2, "growth choice stores both grants " + type);
                Equal(amount, saved.levelUpModifiers[0].value, "saved growth matches runtime amount " + type);
            }
            player.ResetStats();
            player.ResetBaseStats(true);
            LevelUpManager.ResetProgress();
            float attack = player.AttackPower;
            player.ApplyLevelUpStatChoice(LevelUpStatChoice.AttackPowerUp);
            Equal(attack + 1f, player.AttackPower, "legacy level-up path grants attack +1");
            player.ResetStats();
            player.ResetBaseStats(true);
            LevelUpManager.ResetProgress();
        }
        private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); assertions++; }
        private static void Equal(float expected, float actual, string message) => Require(Mathf.Abs(expected - actual) < .001f,
            message + ": " + actual + " expected " + expected);
        [Serializable] private sealed class ExportCatalog { public ExportItem[] items; }
        [Serializable] private sealed class ExportItem { public string id, name, description, role; public bool repeatable; }
    }
}
