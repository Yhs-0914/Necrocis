using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NecrocisEditor
{
    // Run only in the disposable batch project. Uses real scene/NPC, combat, UI and save ports.
    public static class CurrencyEconomyChecks
    {
        private static IEnumerator scenario;
        private static bool started, passed, previousEnabled;
        private static EnterPlayModeOptions previousOptions;
        private static double deadline;
        private static int assertions;
        private static string storage;

        public static void Run()
        {
            if (!Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Use an isolated Unity batch project.");
            started = passed = false; assertions = 0;
            previousEnabled = EditorSettings.enterPlayModeOptionsEnabled; previousOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true; EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            GameplayFocusOwner.TestFocusOverride = true;
            storage = Path.GetFullPath("tmp/economy-" + Guid.NewGuid().ToString("N"));
            SaveService.UseStorageRootForTests(storage); SessionRuntimeBindings.Register();
            Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out var error), "new run: " + error);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            deadline = EditorApplication.timeSinceStartup + 180;
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += OnState; EditorApplication.isPlaying = true;
        }

        private static void Tick()
        {
            if (EditorApplication.timeSinceStartup > deadline) { Debug.LogError("Economy checks timed out."); Finish(); return; }
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || Time.frameCount < 3) return;
            try
            {
                if (!started) { started = true; scenario = Checks(); }
                if (!scenario.MoveNext()) { passed = true; Debug.Log("CURRENCY_ECONOMY_PASS assertions=" + assertions); EditorApplication.isPlaying = false; }
            }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.isPlaying = false; }
        }

        private static IEnumerator Checks()
        {
            InputManager.Instance.Focus.SetNativeFocused(true); InputManager.ResetSessionClock(); Time.timeScale = 1;
            var starter = SaveService.FindWorldReward(CampaignRewardRules.StarterRewardId);
            if (starter != null) starter.collected = true;
            EditorSceneManager.LoadSceneInPlayMode("Assets/_Project/Scenes/Hub.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            double ready = EditorApplication.timeSinceStartup + 20;
            while ((PlayerStats.Instance == null || PlayerStats.Instance.GetComponent<PlayerCurrencyUI>() == null)
                && EditorApplication.timeSinceStartup < ready) yield return null;
            Require(PlayerStats.Instance != null && Camera.main != null, "actual hub initializes persistent player and camera");
            EditorSceneManager.LoadSceneInPlayMode("Assets/_Project/Scenes/Intestine.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            WorldMerchantSpawner spawner = null;
            double until = EditorApplication.timeSinceStartup + 30;
            while (EditorApplication.timeSinceStartup < until)
            {
                spawner = UnityEngine.Object.FindFirstObjectByType<WorldMerchantSpawner>();
                if (spawner?.Merchant != null && PlayerStats.Instance != null) break;
                yield return null;
            }
            Require(spawner?.Merchant != null && PlayerStats.Instance != null, "real map spawns merchant beside persistent player");
            var merchant = spawner.Merchant;
            var player = PlayerStats.Instance.GetComponent<PlayerController>();
            var inventory = player.GetComponent<PlayerItemManager>();
            var ui = player.GetComponent<PlayerMerchantShopUI>();
            var hud = player.GetComponent<PlayerCurrencyUI>();
            Require(ui != null && hud != null && SaveService.CurrencyBalance == 0, "new run wallet and automatic shop UI");
            player.enabled = false; player.GetComponent<PlayerAttack>().enabled = false; player.GetComponent<PlayerClassSkillController>().enabled = false;
            player.GetComponent<Rigidbody>().isKinematic = true;
            foreach (var enemy in UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None)) enemy.SetAiSuppressed(true);
            foreach (var source in UnityEngine.Object.FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None)) source.enabled = false;
            var organReward = SaveService.WorldRewards.First(reward => reward.recoveryOrgan && reward.biome == BiomeType.Intestine);
            var organ = GameObject.Find("Recovery_" + organReward.id).GetComponent<WorldRecoveryPickup>();
            var touch = typeof(WorldRecoveryPickup).GetMethod("OnTriggerStay", BindingFlags.Instance | BindingFlags.NonPublic);
            var playerCollider = player.GetComponentInChildren<Collider>();
            player.RuntimeStats.SetCurrentHealth(player.MaxHealth);
            touch.Invoke(organ, new object[] { playerCollider });
            Require(!organReward.collected, "full health preserves the recovery organ");
            player.RuntimeStats.SetCurrentHealth(player.MaxHealth - 3f);
            touch.Invoke(organ, new object[] { playerCollider });
            Require(organReward.collected && Mathf.Approximately(player.CurrentHealth, player.MaxHealth - 1f),
                "touching a recovery organ heals 2 HP and consumes it");
            touch.Invoke(organ, new object[] { playerCollider });
            Require(Mathf.Approximately(player.CurrentHealth, player.MaxHealth - 1f), "a consumed recovery organ cannot heal twice");
            inventory.ClearAllItems();
            const string repeatable = "dense_muscle";
            Require(inventory.TryAcquireItem(repeatable, out _) && inventory.TryAcquireItem(repeatable.ToUpperInvariant(), out _),
                "repeatable items can be acquired using either casing");
            Require(inventory.RemoveItem(repeatable) && inventory.ContainsItem(repeatable.ToUpperInvariant()),
                "removing one copy preserves ownership until the last copy is removed");
            Require(inventory.RemoveItem(repeatable) && !inventory.ContainsItem(repeatable),
                "removing the last copy clears ownership");
            CheckTierPrices();
            CheckTierPurchases(merchant, player, inventory, ui);
            Kill(player, false, EnemyEncounterRole.Field, false, 1);
            Kill(player, true, EnemyEncounterRole.Field, false, 10);
            Kill(player, false, EnemyEncounterRole.Boss, false, 0);
            Kill(player, true, EnemyEncounterRole.Field, true, 0);
            Require(hud.Amount == 11, "normal and elite awards immediately update HUD");
            foreach (BiomeType biome in new[] { BiomeType.Intestine, BiomeType.Liver, BiomeType.Stomach, BiomeType.Lung })
            {
                long before = SaveService.CurrencyBalance;
                Require(SaveService.TryRewardRegionBoss(biome, 0) && SaveService.CurrencyBalance == before + 50, biome + " encounter clear awards 50");
                Require(!SaveService.TryRewardRegionBoss(biome, 0) && SaveService.CurrencyBalance == before + 50, biome + " duplicate clear pays nothing");
            }
            Require(!SaveService.TryRewardRegionBoss(BiomeType.None, 0), "invalid region cannot pay");
            player.transform.position = merchant.transform.position + Vector3.right;
            merchant.SetVisible(true);
            CheckMerchantSelection(merchant, inventory, player);
            var stock = WorldMerchantShop.EnsureStock(merchant, inventory);
            Require(stock != null && stock.itemIds.Count == 3 && stock.itemIds.Distinct().Count() == 3, "three distinct stocked items");
            string choices = string.Join(",", stock.itemIds);
            Require(ReferenceEquals(stock, WorldMerchantShop.EnsureStock(merchant, inventory)), "opening again keeps stock");
            Require(!WorldMerchantShop.TryBuyRecovery(merchant, player, out _) && SaveService.CurrencyBalance == 211, "full health does not consume money");
            player.RuntimeStats.SetCurrentHealth(player.MaxHealth - 3);
            Require(WorldMerchantShop.TryBuyRecovery(merchant, player, out _) && SaveService.CurrencyBalance == 186
                && Mathf.Approximately(player.CurrentHealth, player.MaxHealth - 1), "25 buys immediate 2HP heal");
            Require(WorldMerchantShop.TryBuyRecovery(merchant, player, out _) && SaveService.CurrencyBalance == 161
                && Mathf.Approximately(player.CurrentHealth, player.MaxHealth), "recovery clamps to full health");

            CheckInteractionPrompt(ui, merchant, player);

            InputManager.Instance.Focus.SetNativeFocused(true); InputManager.ResetSessionClock();
            Require(InputManager.Instance.InteractAction.bindings.Any(binding => binding.effectivePath == "<Keyboard>/f"), "F interaction binding installed");
            Require(ui.TryOpen(merchant) && ui.IsOpen && Time.timeScale == 0 && InputManager.Instance.Contexts.BlocksGameplay,
                "opening actual shop pauses and blocks gameplay");
            Require(!ui.IsInteractionPromptVisible, "open shop hides merchant interaction prompt");
            int budgetSlot = stock.itemIds.FindIndex(id => MerchantItemPricing.GetListPrice(id) == 50);
            string first = stock.itemIds[budgetSlot];
            string second = stock.itemIds.First(id => MerchantItemPricing.GetListPrice(id) == 100);
            string third = stock.itemIds.Single(id => id != first && id != second);
            var button = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(value => value.name == "BuyItem" + budgetSlot);
            button.onClick.Invoke();
            Require(inventory.ContainsItem(first) && stock.purchasedItemIds.Contains(first) && SaveService.CurrencyBalance == 111, "UI purchase grants budget item and charges 50");
            Require(!button.interactable && button.GetComponentInChildren<Text>().text == "품절", "bought item becomes sold out");
            int count = inventory.ItemCount;
            Require(!WorldMerchantShop.TryBuyItem(merchant, player, first, out _) && inventory.ItemCount == count
                && SaveService.CurrencyBalance == 111, "duplicate purchase cannot grant or charge");
            int standardSlot = stock.itemIds.IndexOf(second);
            var standardButton = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .First(value => value.name == "BuyItem" + standardSlot);
            Require(standardButton.interactable, "standard item is affordable at 111");
            Require(SaveService.TrySpendCurrency(50), "fixture sets balance below standard item price");
            Require(!standardButton.interactable && standardButton.GetComponentInChildren<Text>().text == "구매  100",
                "balance change updates affordability without changing the displayed price");
            Require(!WorldMerchantShop.TryBuyItem(merchant, player, second, out _) && SaveService.CurrencyBalance == 61, "insufficient funds cannot grant or charge");
            Require(!WorldMerchantShop.TryBuyItem(merchant, player, "removed_item", out _), "unstocked item cannot be bought");
            SaveService.CreditCurrency(200);
            Require(standardButton.interactable, "crediting currency re-enables the standard offer");
            RenderPreview("MerchantShop-1920x1080", 1920, 1080);
            RenderPreview("MerchantShop-1280x720", 1280, 720);
            Require(InputManager.Instance.Contexts.DispatchEscape(Time.frameCount), "Escape routed to shop modal");
            Require(!ui.IsOpen && Time.timeScale == 1 && !InputManager.Instance.Contexts.BlocksGameplay, "Escape closes and resumes gameplay");
            Require(ui.TryOpen(merchant) && string.Join(",", stock.itemIds) == choices, "reopen preserves offers and sold state");
            ui.Close();

            // A failed post-grant save must not refund an item; journal retry preserves all three states.
            bool injected = false;
            using (SaveService.InjectStorageFaultsForTests((file, point) =>
            {
                if (!injected && file == "normal.json" && point == "write") { injected = true; throw new IOException("economy check simulated save failure"); }
            }))
            {
                Require(WorldMerchantShop.TryBuyItem(merchant, player, second, out string message) && injected
                    && message.Contains("저장 재시도"), "completed purchase reports a retryable save failure");
            }
            Require(SaveService.CurrencyBalance == 161 && inventory.ContainsItem(second) && stock.purchasedItemIds.Count == 2,
                "failed save retains charged wallet, granted item and sold stock together");
            Require(SaveService.TrySaveActiveRun(out var saveError), "retry purchase save: " + saveError);
            var disk = JsonUtility.FromJson<RunSaveData>(File.ReadAllText(Path.Combine(storage, "Saves", "normal.json")));
            Require(disk.currency == 161 && disk.player.items.Count == 2 && disk.world.merchantShops.Single().purchasedItemIds.Count == 2,
                "disk commits wallet, inventory and stock together");
            merchant.SetVisible(false);
            Require(!ui.TryOpen(merchant) && !WorldMerchantShop.TryBuyItem(merchant, player, third, out _), "invisible merchant cannot interact");
            merchant.SetVisible(true); player.transform.position += Vector3.right * 10;
            Require(!ui.TryOpen(merchant) && !WorldMerchantShop.TryBuyRecovery(merchant, player, out _), "distant merchant cannot interact");
            player.transform.position = merchant.transform.position + Vector3.right;
            Require(ui.TryOpen(merchant), "shop opens again nearby");
            using (InputManager.Instance.Contexts.Register(new object(), 999, true, allowAuxiliary: false))
            {
                ui.Close(); Require(!ui.TryOpen(merchant), "higher modal prevents shop opening");
            }
            long balance = SaveService.CurrencyBalance;
            SaveService.ClearActiveSessionReference(); SaveService.UseStorageRootForTests(storage);
            Require(SaveService.TryContinue(GameDifficulty.Normal, out var continueError), "continue economy run: " + continueError);
            var restoredOrgan = SaveService.FindWorldReward(organReward.id);
            Require(restoredOrgan != null && restoredOrgan.collected && restoredOrgan.x == organReward.x && restoredOrgan.z == organReward.z,
                "continue preserves a spent recovery organ and its fixed position");
            Require(SaveService.CurrencyBalance == balance && hud.Amount == balance, "continue restores wallet and HUD");
            var restored = SaveService.FindMerchantShop(stock.biome, stock.mapSeed);
            Require(restored != null && restored.purchasedItemIds.Count == 2 && string.Join(",", restored.itemIds) == choices,
                "continue restores exact stock and sold entries");
            Require(SaveService.IsFirstMerchantShop(restored), "continue preserves the first visited shop");
            Require(!SaveService.TryRewardRegionBoss(BiomeType.Lung, 0) && SaveService.CurrencyBalance == balance, "regional clear cannot pay again after continue");
            Require(SaveService.TryBeginNewGame(GameDifficulty.Normal, out var newError), "fresh run: " + newError);
            Require(SaveService.CurrencyBalance == 0 && SaveService.FindMerchantShop(stock.biome, stock.mapSeed) == null, "new run resets money and merchandise");

            Require(ui.TryOpen(merchant), "shop can open before scene transition");
            EditorSceneManager.LoadSceneInPlayMode("Assets/_Project/Scenes/FinalBoss.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            Require(!ui.IsOpen && Time.timeScale == 1, "scene transition releases shop pause");
            Require(FinalBossArena.Instance != null, "real final arena loaded");
            Kill(player, false, EnemyEncounterRole.Field, false, 0);
            Require(SaveService.CurrencyBalance == 0, "final boss summoned field mobs award no currency");
            SaveService.ClearActiveSessionReference(); SaveService.CreditCurrency(long.MaxValue); SaveService.CreditCurrency(1);
            var firstShop = SaveService.RememberMerchantShop(new MerchantShopSaveData { biome = BiomeType.Lung, mapSeed = 1 });
            var laterShop = SaveService.RememberMerchantShop(new MerchantShopSaveData { biome = BiomeType.Intestine, mapSeed = 2 });
            Require(SaveService.IsFirstMerchantShop(firstShop) && !SaveService.IsFirstMerchantShop(laterShop),
                "budget offer follows the first opened shop, regardless of region");
            var reseededShop = SaveService.RememberMerchantShop(new MerchantShopSaveData { biome = BiomeType.Lung, mapSeed = 3 });
            Require(SaveService.IsFirstMerchantShop(reseededShop), "a replacement map seed preserves shop visit order");
            Require(SaveService.CurrencyBalance == long.MaxValue, "wallet overflow saturates safely");
            Require(!SaveService.TrySpendCurrency(-1) && SaveService.TrySpendCurrency(long.MaxValue), "negative spend rejected and full balance spend allowed");
            Require(SaveService.CurrencyBalance == 0, "wallet cannot go below zero");
            var old = JsonUtility.FromJson<RunSaveData>("{\"schemaVersion\":1,\"saveId\":\"legacy\",\"isActive\":true}");
            SaveRepository.NormalizeRun(old);
            Require(old.currency == 0 && old.world.merchantShops != null, "legacy saves default to zero currency and empty shops");
        }

        private static void Kill(PlayerController player, bool elite, EnemyEncounterRole role, bool suppress, long reward)
        {
            long before = SaveService.CurrencyBalance;
            var rule = new EnemySpawnRuleConfig { name = "Economy-" + assertions, maxHealth = 10, moveSpeed = 0,
                colliderSize = Vector3.one, enableContactDamage = false, expReward = 0, isElite = elite };
            var enemy = EnemyController.Acquire(null, rule.name, EnemyController.GetPoolArchetypeId(rule));
            enemy.ConfigureWithContext(null, rule, Vector3.one * 500, Vector3.one * 500, new EnemySpawnContext(null, new FlatWorld(), role));
            enemy.SetAiSuppressed(true); enemy.SuppressExperienceReward = suppress;
            enemy.TakeDamage(10000, player.GetHealthEffectSource("economy-check"));
            Require(enemy.IsDead && SaveService.CurrencyBalance == before + reward, "actual death pays " + reward + " for " + role + "/elite=" + elite);
            enemy.GrantExp(); Require(SaveService.CurrencyBalance == before + reward, "repeated reward call does not pay twice");
        }

        private static void Require(bool value, string label) { assertions++; if (!value) throw new InvalidOperationException(label); }

        private static void CheckTierPrices()
        {
            Require(MerchantItemPricing.TieredItems.Count == 36 && MerchantItemPricing.TieredItems.Select(tier => tier.ItemId).Distinct().Count() == 36,
                "price metadata covers exactly 12 three-stage families");
            foreach (var family in MerchantItemPricing.TieredItems.GroupBy(tier => tier.FamilyId))
            {
                var stages = family.OrderBy(tier => tier.Level).ToArray();
                for (int i = 0; i < 3; i++)
                {
                    string incoming = stages[i].ItemId;
                    Require(MerchantItemPricing.GetListPrice(incoming) == (i + 1) * 50
                        && MerchantItemPricing.TryQuote(incoming, null, out var direct) && direct.Price == (i + 1) * 50 && !direct.IsUpgrade,
                        family.Key + " direct stage " + (i + 1) + " price");
                    Require(!MerchantItemPricing.TryQuote(incoming, new[] { incoming.ToUpperInvariant() }, out _), "same item never sells twice: " + incoming);
                    for (int j = 0; j < 3; j++)
                    {
                        bool allowed = MerchantItemPricing.TryQuote(incoming, new[] { stages[j].ItemId }, out var quote);
                        Require(allowed == (j < i), family.Key + " lower/equal/higher purchase guard " + i + "/" + j);
                        if (allowed) Require(quote.Price == (i - j) * 50 && quote.ReplacementItemId == stages[j].ItemId,
                            family.Key + " upgrade pays only stage difference");
                    }
                }
                Require(MerchantItemPricing.TryQuote(stages[2].ItemId, new[] { stages[0].ItemId, stages[1].ItemId }, out var highest)
                    && highest.Price == 50 && highest.ReplacementItemId == stages[1].ItemId, family.Key + " legacy multiple stages credit only the highest");
            }
            Require(MerchantItemPricing.TryQuote("swift_cell", new[] { "piercing_mucus" }, out var independent)
                && independent.Price == 100 && !independent.IsUpgrade, "independent items keep price 100 without unrelated discounts");
            foreach (var (basic, stronger) in new[] { ("dense_muscle", "hardened_muscle"), ("vital_cell", "vital_organ"), ("elastic_fiber", "active_fiber") })
            {
                Require(MerchantItemPricing.TryQuote(basic.ToUpperInvariant(), new[] { stronger }, out var budget)
                    && budget.Price == 50 && !budget.IsUpgrade, basic + " costs 50 even when the stronger stat item is owned");
                Require(MerchantItemPricing.TryQuote(stronger, new[] { basic }, out var standard)
                    && standard.Price == 100 && !standard.IsUpgrade, stronger + " costs 100 and can coexist with basic stat item");
            }
            Require(!MerchantItemPricing.TryQuote(null, null, out _) && !MerchantItemPricing.TryQuote(" ", null, out _), "invalid price requests rejected");
        }

        private static void CheckTierPurchases(WorldMerchantNpc merchant, PlayerController player, PlayerItemManager inventory, PlayerMerchantShopUI ui)
        {
            // Simulate the item owner's future catalog registration only inside this isolated fixture.
            // Production retired-ID filters and effects remain untouched.
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var map = (System.Collections.Generic.Dictionary<string, PlayerItemManager.PlayerItemEntry>)typeof(PlayerItemManager).GetField("entryMap", flags).GetValue(inventory);
            var entries = (System.Collections.Generic.List<PlayerItemManager.PlayerItemEntry>)typeof(PlayerItemManager).GetField("itemEntries", flags).GetValue(inventory);
            const string low = "dilute_mucus", middle = "piercing_mucus", high = "binding_mucus";
            inventory.TryGetItemEntry(middle, out var middleEntry);
            var lowEntry = new PlayerItemManager.PlayerItemEntry(low, "상점 검사 1단계", "가격 검사", PlayerItemCategory.BasicProjectile, middleEntry.Icon);
            var highEntry = new PlayerItemManager.PlayerItemEntry(high, "상점 검사 3단계", "가격 검사", PlayerItemCategory.BasicProjectile, middleEntry.Icon);
            Require(!map.ContainsKey(low) && !map.ContainsKey(high), "retired stage effects remain unregistered in production catalog");
            entries.Add(lowEntry); entries.Add(highEntry); map.Add(low, lowEntry); map.Add(high, highEntry);
            player.transform.position = merchant.transform.position + Vector3.right; merchant.SetVisible(true);
            var stock = WorldMerchantShop.EnsureStock(merchant, inventory);
            var previousOffers = stock.itemIds.ToList(); var previousSold = stock.purchasedItemIds.ToList();
            long startingMoney = SaveService.CurrencyBalance;
            void ResetStock()
            {
                ui.Close(); inventory.ClearAllItems();
                stock.itemIds = new System.Collections.Generic.List<string> { low, middle, high };
                stock.purchasedItemIds.Clear();
            }
            try
            {
                ResetStock(); SaveService.CreditCurrency(1000);
                Require(ui.TryOpen(merchant), "tier shop opens with registered fixture items");
                Button Buy(int i) => UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(button => button.name == "BuyItem" + i);
                Require(Buy(0).GetComponentInChildren<Text>().text == "구매  50" && Buy(1).GetComponentInChildren<Text>().text == "구매  100"
                    && Buy(2).GetComponentInChildren<Text>().text == "구매  150", "UI shows the three real purchase prices");
                Buy(0).onClick.Invoke();
                Require(SaveService.CurrencyBalance == 950 && inventory.ContainsItem(low), "stage one costs 50");
                Require(Buy(1).GetComponentInChildren<Text>().text == "교체  50" && Buy(2).GetComponentInChildren<Text>().text == "교체  100",
                    "UI immediately shows owned-stage discounts");
                Buy(1).onClick.Invoke();
                Require(SaveService.CurrencyBalance == 900 && inventory.ContainsItem(middle) && !inventory.ContainsItem(low), "one to two replaces lower stage for 50");
                Require(Buy(2).GetComponentInChildren<Text>().text == "교체  50", "two to three UI price is 50");
                Buy(2).onClick.Invoke();
                Require(SaveService.CurrencyBalance == 850 && inventory.ContainsItem(high) && inventory.ItemCount == 1,
                    "one through three costs 150 total and retains only the highest");
                Require(stock.purchasedItemIds.Count == 3, "tier purchase sold-out slots are preserved");
                var disk = JsonUtility.FromJson<RunSaveData>(File.ReadAllText(Path.Combine(storage, "Saves", "normal.json")));
                Require(disk.currency == 850 && disk.player.items.Count == 1 && disk.player.items[0].itemId == high
                    && disk.world.merchantShops.Single().purchasedItemIds.Count == 3, "upgrade save atomically captures money, replaced inventory and sold stock");

                ResetStock(); Require(WorldMerchantShop.TryBuyItem(merchant, player, high, out _) && SaveService.CurrencyBalance == 700,
                    "direct stage three costs 150 without lower ownership");
                long before = SaveService.CurrencyBalance;
                Require(!WorldMerchantShop.TryBuyItem(merchant, player, low, out _) && SaveService.CurrencyBalance == before,
                    "higher stage prevents lower-stage purchase and charge");

                ResetStock(); Require(inventory.TryAcquireItem(middle, out _), "fixture acquires stage two from another source");
                Require(SaveService.TrySpendCurrency(SaveService.CurrencyBalance - 49), "fixture retains 49 gold");
                Require(!WorldMerchantShop.TryBuyItem(merchant, player, high, out _) && SaveService.CurrencyBalance == 49
                    && inventory.ContainsItem(middle) && !inventory.ContainsItem(high), "insufficient upgrade money preserves existing item");
                SaveService.CreditCurrency(1);
                bool removed = false;
                Action<long> changeCatalog = value => { if (!removed && value == 0) { removed = true; map.Remove(high); } };
                SaveService.CurrencyChanged += changeCatalog;
                try
                {
                    Require(!WorldMerchantShop.TryBuyItem(merchant, player, high, out _) && removed && SaveService.CurrencyBalance == 50
                        && inventory.ContainsItem(middle) && !stock.purchasedItemIds.Contains(high), "failed item replacement refunds only the 50 paid and keeps the lower item");
                }
                finally { SaveService.CurrencyChanged -= changeCatalog; map[high] = highEntry; }
                Require(WorldMerchantShop.TryBuyItem(merchant, player, high, out _) && SaveService.CurrencyBalance == 0
                    && !inventory.ContainsItem(middle) && inventory.ContainsItem(high), "free-found lower stage still grants its market-value credit");

                ResetStock(); inventory.TryAcquireItem(low, out _); inventory.TryAcquireItem(middle, out _); SaveService.CreditCurrency(50);
                Require(WorldMerchantShop.TryBuyItem(merchant, player, high, out _) && SaveService.CurrencyBalance == 0
                    && inventory.ItemCount == 1 && inventory.ContainsItem(high), "legacy multiple lower stages do not stack discounts or survive upgrade");
            }
            finally
            {
                ui.Close(); inventory.ClearAllItems();
                entries.Remove(lowEntry); entries.Remove(highEntry); map.Remove(low); map.Remove(high);
                stock.itemIds = previousOffers; stock.purchasedItemIds = previousSold;
                player.RuntimeStats.SetCurrentHealth(player.MaxHealth);
                if (SaveService.CurrencyBalance > startingMoney) SaveService.TrySpendCurrency(SaveService.CurrencyBalance - startingMoney);
                Require(SaveService.TrySaveActiveRun(out var error), "clear tier price fixture: " + error);
            }
        }

        private static void CheckMerchantSelection(WorldMerchantNpc merchant, PlayerItemManager inventory, PlayerController player)
        {
            var catalog = inventory.ItemEntries.Select(entry => entry.ItemId).ToArray();
            var owned = new[] { "swift_cell", "homing_cell" };
            for (int seed = 0; seed < 20; seed++)
            {
                var selected = CampaignRewardRules.ChooseMerchantOffers(catalog, owned, seed);
                Require(selected.Count == 3 && selected.Distinct().Count() == 3 && !selected.Intersect(owned).Any(),
                    "merchant draw excludes all owned items, including repeatable basics, seed=" + seed);
            }
            var attack = catalog.Where(id => CampaignRewardRules.Role(id) == CampaignRewardRole.Attack).Take(3).ToArray();
            var mixed = attack.Concat(new[] { "exoskeleton", "homing_cell" }).ToArray();
            Require(Enumerable.Range(0, 100).Any(seed => CampaignRewardRules.ChooseMerchantOffers(mixed, null, seed).All(attack.Contains)),
                "merchant draw can select three attack items without role quotas");
            Require(Enumerable.Range(0, 20).Select(seed => string.Join(",", CampaignRewardRules.ChooseMerchantOffers(catalog, owned, seed))).Distinct().Count() > 1,
                "different map seeds yield different merchant draws");
            Require(CampaignRewardRules.ChooseMerchantOffers(new[] { "swift_cell", "SWIFT_CELL", "homing_cell", "exoskeleton", "", null },
                new[] { "SWIFT_CELL" }, 1).Count == 2, "merchant draw handles case, duplicates and fewer than three eligible items");
            Require(CampaignRewardRules.ChooseMerchantOffers(catalog, catalog, 1).Count == 0, "all owned catalog yields no merchandise");
            Require(inventory.TryAcquireItem(owned[0], out _) && inventory.TryAcquireItem(owned[1], out _), "fixture owns basic and special items");
            var stock = WorldMerchantShop.EnsureStock(merchant, inventory);
            Require(stock.itemIds.Count == 3 && !stock.itemIds.Intersect(owned).Any(), "actual first merchant stock excludes inventory");
            bool IsBudgetItem(string id) => MerchantItemPricing.GetListPrice(id) == 50;
            Require(stock.itemIds.Any(IsBudgetItem), "first merchant reserves one eligible 50 gold offer");
            string retained = stock.itemIds[2];
            stock.itemIds = new System.Collections.Generic.List<string> { owned[0], owned[1], retained };
            WorldMerchantShop.EnsureStock(merchant, inventory);
            Require(stock.itemIds.Count == 3 && !stock.itemIds.Intersect(owned).Any() && stock.itemIds.Contains(retained),
                "legacy owned offers are replaced while valid unsold offers stay");
            string previous = string.Join(",", stock.itemIds);
            WorldMerchantShop.EnsureStock(merchant, inventory);
            Require(string.Join(",", stock.itemIds) == previous, "reopening without inventory changes never rerolls stock");
            string acquiredElsewhere = stock.itemIds[0];
            string[] unchanged = stock.itemIds.Skip(1).ToArray();
            Require(inventory.TryAcquireItem(acquiredElsewhere, out _), "acquire a stocked item outside merchant");
            WorldMerchantShop.EnsureStock(merchant, inventory);
            Require(stock.itemIds.Count == 3 && !stock.itemIds.Contains(acquiredElsewhere) && unchanged.All(stock.itemIds.Contains),
                "revisit replaces newly owned unsold item and keeps remaining offers");
            string sold = stock.itemIds[0];
            Require(inventory.TryAcquireItem(sold, out _), "fixture owns purchased item");
            stock.purchasedItemIds.Add(sold);
            previous = string.Join(",", stock.itemIds);
            WorldMerchantShop.EnsureStock(merchant, inventory);
            Require(stock.itemIds.Contains(sold) && stock.purchasedItemIds.Contains(sold) && string.Join(",", stock.itemIds) == previous,
                "owned sold-out slot stays sold out without free restock");
            long funds = SaveService.CurrencyBalance;
            Require(!WorldMerchantShop.TryBuyItem(merchant, player, owned[0], out _) && SaveService.CurrencyBalance == funds,
                "already owned repeatable basic cannot be purchased again");
            stock.purchasedItemIds.Clear(); inventory.ClearAllItems(); player.RuntimeStats.SetCurrentHealth(player.MaxHealth);
            var legacyOffers = catalog.Where(id => MerchantItemPricing.GetListPrice(id) == 100).Take(3).ToList();
            stock.itemIds = legacyOffers.ToList();
            stock.purchasedItemIds.Add(legacyOffers[2]);
            WorldMerchantShop.EnsureStock(merchant, inventory);
            Require(stock.itemIds[0] == legacyOffers[0] && IsBudgetItem(stock.itemIds[1]) && stock.itemIds[2] == legacyOffers[2]
                && stock.purchasedItemIds.SequenceEqual(new[] { legacyOffers[2] }), "existing full shop replaces only one unsold slot with a budget item");

            var budgetItems = catalog.Where(IsBudgetItem).ToArray();
            foreach (string id in budgetItems) Require(inventory.TryAcquireItem(id, out _), "fixture owns budget item " + id);
            WorldMerchantShop.EnsureStock(merchant, inventory);
            Require(stock.itemIds.Count == 3 && !stock.itemIds.Any(IsBudgetItem), "merchant fills all slots when no budget item is eligible");

            inventory.ClearAllItems();
            stock.purchasedItemIds.Clear();
            stock.itemIds = legacyOffers;
            WorldMerchantShop.EnsureStock(merchant, inventory);
            Require(SaveService.TrySaveActiveRun(out var error), "reset selection fixture: " + error);
        }

        private static void CheckInteractionPrompt(PlayerMerchantShopUI ui, WorldMerchantNpc merchant, PlayerController player)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            void Tick()
            {
                typeof(PlayerMerchantShopUI).GetField("nextScan", flags).SetValue(ui, 0f);
                typeof(PlayerMerchantShopUI).GetMethod("Update", flags).Invoke(ui, null);
                typeof(PlayerMerchantShopUI).GetMethod("LateUpdate", flags).Invoke(ui, null);
                Canvas.ForceUpdateCanvases();
            }
            var camera = Camera.main;
            Vector3 position = camera.transform.position; Quaternion rotation = camera.transform.rotation;
            try
            {
                camera.transform.position = merchant.transform.position + new Vector3(0, 7, -7);
                camera.transform.LookAt(merchant.transform.position + Vector3.up);
                Tick(); Tick();
                Require(ui.IsInteractionPromptVisible && !ui.IsOpen, "approaching merchant shows prompt without auto-opening");
                var prompt = GameObject.Find("MerchantPrompt").GetComponent<RectTransform>();
                Require(prompt.GetComponentsInChildren<Graphic>().All(graphic => !graphic.raycastTarget), "interaction hint does not block world clicks");
                RectTransformUtility.ScreenPointToLocalPointInRectangle(prompt.parent as RectTransform,
                    camera.WorldToScreenPoint(merchant.PromptWorldPosition), null, out Vector2 top);
                Require(prompt.anchoredPosition.y > top.y, "interaction prompt follows merchant overhead");
                player.transform.position = merchant.transform.position; Tick();
                Require(ui.IsInteractionPromptVisible && !ui.IsOpen, "overlapping merchant only shows hint and never opens automatically");
                player.transform.position += Vector3.right * 10;
                // Keep the cached merchant to test that leaving hides the hint before the next scan.
                typeof(PlayerMerchantShopUI).GetField("nextScan", flags).SetValue(ui, float.MaxValue);
                typeof(PlayerMerchantShopUI).GetMethod("Update", flags).Invoke(ui, null);
                Require(!ui.IsInteractionPromptVisible && !ui.TryOpen(merchant), "leaving range immediately hides hint and denies opening");
                player.transform.position = merchant.transform.position + Vector3.right; Tick();
                using (InputManager.Instance.Contexts.Register(new object(), 999, true, allowAuxiliary: false))
                {
                    typeof(PlayerMerchantShopUI).GetMethod("LateUpdate", flags).Invoke(ui, null);
                    Require(!ui.IsInteractionPromptVisible && !ui.TryOpen(merchant), "other modal hides hint and blocks merchant use");
                }
                Tick(); Require(ui.IsInteractionPromptVisible, "closing other modal restores nearby hint");
                merchant.SetVisible(false); Tick();
                Require(!ui.IsInteractionPromptVisible && !ui.TryOpen(merchant), "hidden merchant cannot show hint or open");
                merchant.SetVisible(true); Tick();
            }
            finally { camera.transform.SetPositionAndRotation(position, rotation); }
        }
        private static void OnState(PlayModeStateChange state) { if (state == PlayModeStateChange.EnteredEditMode) Finish(); }
        private static void Finish()
        {
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnState;
            EditorSettings.enterPlayModeOptionsEnabled = previousEnabled; EditorSettings.enterPlayModeOptions = previousOptions;
            GameplayFocusOwner.TestFocusOverride = null; Time.timeScale = 1; SaveService.ResetStaticStateForTests(); EditorApplication.Exit(passed ? 0 : 1);
        }

        private static void RenderPreview(string name, int width, int height)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            var camera = Camera.main;
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .Where(canvas => canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
            var previous = RenderTexture.active; var previousTarget = camera.targetTexture;
            var target = new RenderTexture(width, height, 24);
            try
            {
                camera.targetTexture = target;
                foreach (var canvas in canvases)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = .5f;
                    var scaler = canvas.GetComponent<CanvasScaler>(); if (scaler != null) { scaler.enabled = false; scaler.enabled = true; }
                }
                Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
                var panel = GameObject.Find("MerchantPanel").GetComponent<RectTransform>();
                var corners = new Vector3[4]; panel.GetWorldCorners(corners);
                Vector2 lower = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
                Vector2 upper = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
                Require(lower.x >= 0 && lower.y >= 0 && upper.x <= width && upper.y <= height, name + " entire shop panel fits within screen");
                var image = new Texture2D(width, height, TextureFormat.RGBA32, false);
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
                Directory.CreateDirectory("tmp/economy-previews"); File.WriteAllBytes("tmp/economy-previews/" + name + ".png", image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
            }
            finally
            {
                foreach (var canvas in canvases) if (canvas != null) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
                camera.targetTexture = previousTarget; RenderTexture.active = previous; target.Release(); UnityEngine.Object.DestroyImmediate(target);
                Canvas.ForceUpdateCanvases();
            }
        }

        private sealed class FlatWorld : IEnemyWorldQueries
        {
            public bool HasTerrain => false;
            public bool IsValidCell(Vector2Int cell) => true;
            public bool IsWalkableCell(Vector2Int cell) => true;
            public Vector2Int WorldToCell(Vector3 p) => new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.z));
            public Vector3 CellToWorld(Vector2Int c) => new Vector3(c.x, 0, c.y);
            public float GetGroundHeight(Vector3 p) => 0;
            public float GetGroundHeight(Vector2Int c) => 0;
            public bool CanEnemyMove(Vector3 from, Vector3 to, Vector2 half) => true;
            public bool IsPlayerInsideLockedEncounter(Vector3 p) => false;
        }
    }
}
