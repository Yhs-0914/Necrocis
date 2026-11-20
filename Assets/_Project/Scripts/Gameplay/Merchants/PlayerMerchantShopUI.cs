using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Necrocis
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController), typeof(PlayerItemManager))]
    public sealed class PlayerMerchantShopUI : MonoBehaviour
    {
        private static readonly Color Gold = new Color(1, .84f, .48f);
        private static readonly Color Ink = new Color(.94f, .85f, .80f);
        private static readonly Color Muted = new Color(.72f, .60f, .61f);
        private static readonly Color PanelTrim = new Color(.15f, .065f, .10f);
        private static readonly Color CardTint = new Color(.14f, .06f, .09f, .84f);
        private PlayerController player;
        private PlayerItemManager inventory;
        private WorldMerchantNpc nearby, merchant;
        private MerchantShopSaveData stock;
        private GameObject canvasObject, overlay;
        private RectTransform prompt;
        private Text balance, status;
        private Button recovery;
        private readonly Button[] buyButtons = new Button[3];
        private readonly Text[] buyLabels = new Text[3];
        private readonly Text[] names = new Text[3];
        private readonly Text[] descriptions = new Text[3];
        private readonly Image[] icons = new Image[3];
        private readonly int[] offerPrices = new int[3];
        private ModalInputScope inputScope;
        private float nextScan;
        private long displayedBalance = -1;
        private bool offersDirty;
        public bool IsOpen => merchant != null && overlay != null && overlay.activeSelf;
        public WorldMerchantNpc NearbyMerchant => nearby;
        public bool IsInteractionPromptVisible => prompt != null && prompt.gameObject.activeSelf;

        private void Awake() { player = GetComponent<PlayerController>(); inventory = GetComponent<PlayerItemManager>(); }
        private void OnEnable()
        {
            SaveService.CurrencyChanged += OnBalanceChanged;
            SceneManager.activeSceneChanged += OnSceneChanged;
            if (inventory != null)
            {
                inventory.ItemAcquired += OnInventoryChanged;
                inventory.ItemRemoved += OnInventoryChanged;
            }
        }
        private void OnDisable()
        {
            Close();
            SaveService.CurrencyChanged -= OnBalanceChanged;
            SceneManager.activeSceneChanged -= OnSceneChanged;
            if (inventory != null)
            {
                inventory.ItemAcquired -= OnInventoryChanged;
                inventory.ItemRemoved -= OnInventoryChanged;
            }
            if (prompt != null) prompt.gameObject.SetActive(false);
        }
        private void OnDestroy() { Close(); if (canvasObject != null) Destroy(canvasObject); }
        private void OnSceneChanged(Scene previous, Scene next) { Close(); nearby = null; nextScan = 0; }
        private void OnBalanceChanged(long value) { if (IsOpen) RefreshBalance(value); }
        private void OnInventoryChanged(PlayerItemManager _, PlayerItemManager.AcquiredPlayerItem __) => offersDirty = true;

        private void Update()
        {
            var input = InputManager.Instance;
            bool interacted = input.InteractAction.WasPressedThisFrame();
            if (IsOpen)
            {
                if (!WorldMerchantShop.IsInReach(merchant, player)) { Close(); return; }
                if (interacted && inputScope?.CanInteract == true) Close();
                return;
            }
            // A destroyed or unloaded NPC also releases the modal pause.
            if (inputScope != null) Close();
            if (Time.unscaledTime >= nextScan || interacted)
            {
                nextScan = Time.unscaledTime + .2f;
                nearby = null;
                float best = float.MaxValue;
                var candidates = WorldMerchantNpc.ActiveMerchants;
                for (int i = 0; i < candidates.Count; i++)
                {
                    var candidate = candidates[i];
                    if (!WorldMerchantShop.IsInReach(candidate, player)) continue;
                    float distance = (candidate.transform.position - player.transform.position).sqrMagnitude;
                    if (distance < best) { nearby = candidate; best = distance; }
                }
            }
            if (IsInteractionPromptVisible && (!WorldMerchantShop.IsInReach(nearby, player)
                || !input.Gameplay.CanReadGameplay || !input.Contexts.CanOpenAuxiliary))
                prompt.gameObject.SetActive(false);
            if (interacted && nearby != null) TryOpen(nearby);
        }

        private void LateUpdate()
        {
            if (IsOpen)
            {
                if (offersDirty) RefreshOffers();
                RefreshRecoveryAvailability(SaveService.CurrencyBalance);
                return;
            }

            // Position once, after camera movement and other menus' Update callbacks.
            var input = InputManager.Instance;
            bool available = WorldMerchantShop.IsInReach(nearby, player)
                && input.Gameplay.CanReadGameplay && input.Contexts.CanOpenAuxiliary;
            if (available) BuildUi();
            if (prompt != null)
            {
                bool visible = available && TryPositionPrompt();
                if (prompt.gameObject.activeSelf != visible) prompt.gameObject.SetActive(visible);
            }
        }

        private bool TryPositionPrompt()
        {
            var camera = Camera.main;
            if (camera == null || nearby == null || prompt == null) return false;
            Vector3 screen = camera.WorldToScreenPoint(nearby.PromptWorldPosition);
            if (screen.z <= 0 || screen.x < 0 || screen.x > camera.pixelWidth || screen.y < 0 || screen.y > camera.pixelHeight) return false;
            var canvasRect = canvasObject.transform as RectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local)) return false;
            local.y += 30;
            Vector2 half = prompt.sizeDelta * .5f;
            local.x = Mathf.Clamp(local.x, canvasRect.rect.xMin + half.x + 12, canvasRect.rect.xMax - half.x - 12);
            local.y = Mathf.Clamp(local.y, canvasRect.rect.yMin + half.y + 12, canvasRect.rect.yMax - half.y - 12);
            prompt.anchoredPosition = local;
            return true;
        }

        public bool TryOpen(WorldMerchantNpc target)
        {
            if (IsOpen || !isActiveAndEnabled || !WorldMerchantShop.IsInReach(target, player)
                || !InputManager.Instance.Gameplay.CanReadGameplay || !InputManager.Instance.Contexts.CanOpenAuxiliary) return false;
            stock = WorldMerchantShop.EnsureStock(target, inventory);
            if (stock == null) return false;
            merchant = target;
            BuildUi(); prompt.gameObject.SetActive(false); overlay.SetActive(true);
            status.text = "필요한 물건을 골라보세요.";
            inputScope = new ModalInputScope(this, overlay, true, Close, allowAuxiliary: false);
            RefreshOffers();
            return true;
        }

        public void Close()
        {
            inputScope?.Dispose(); inputScope = null;
            if (overlay != null) overlay.SetActive(false);
            merchant = null; stock = null;
        }

        private void BuyItem(int index)
        {
            if (!IsOpen || inputScope?.CanInteract != true || index >= stock.itemIds.Count) return;
            WorldMerchantShop.TryBuyItem(merchant, player, stock.itemIds[index], out string message);
            status.text = message; RefreshOffers();
        }
        private void BuyRecovery()
        {
            if (!IsOpen || inputScope?.CanInteract != true) return;
            WorldMerchantShop.TryBuyRecovery(merchant, player, out string message);
            status.text = message;
            if (IsOpen) RefreshRecoveryAvailability(SaveService.CurrencyBalance);
        }

        private void RefreshOffers()
        {
            if (stock == null || balance == null) return;
            offersDirty = false;
            var ownedIds = inventory.OwnedItemIds;
            for (int i = 0; i < 3; i++)
            {
                string id = i < stock.itemIds.Count ? stock.itemIds[i] : null;
                var entry = id != null && inventory.TryGetItemEntry(id, out var found) ? found : null;
                bool sold = id == null || stock.purchasedItemIds?.Contains(id) == true;
                bool quotable = MerchantItemPricing.TryQuote(id, ownedIds, out var quote);
                bool owned = entry != null && (!quotable || !inventory.CanAcquireItem(id));
                names[i].text = entry?.DisplayName ?? "준비된 상품 없음";
                if (entry != null && MerchantItemPricing.TryGetTier(id, out var tier)) names[i].text += " · " + tier.Level + "단계";
                descriptions[i].text = entry?.Description ?? "";
                if (!sold && quote.IsUpgrade && inventory.TryGetItemEntry(quote.ReplacementItemId, out var previous))
                    descriptions[i].text += "\n" + previous.DisplayName + " 교체 · 정가 " + quote.ListPrice + " / 차액 " + quote.Price;
                icons[i].sprite = entry?.Icon; icons[i].enabled = entry?.Icon != null;
                buyLabels[i].text = sold ? "품절" : owned ? "보유 단계" : (quote.IsUpgrade ? "교체  " : "구매  ") + quote.Price;
                offerPrices[i] = !sold && !owned && entry != null ? quote.Price : -1;
            }
            RefreshBalance(SaveService.CurrencyBalance);
        }

        private void RefreshBalance(long amount)
        {
            if (displayedBalance != amount)
            {
                displayedBalance = amount;
                balance.text = amount.ToString("N0", CultureInfo.InvariantCulture);
            }
            for (int i = 0; i < buyButtons.Length; i++)
                buyButtons[i].interactable = offerPrices[i] >= 0 && amount >= offerPrices[i];
            RefreshRecoveryAvailability(amount);
        }

        private void RefreshRecoveryAvailability(long amount)
        {
            recovery.interactable = !player.IsDead && player.CurrentHealth < player.MaxHealth - .0001f
                && amount >= CurrencyEconomy.RecoveryPrice;
        }

        private void BuildUi()
        {
            if (canvasObject != null) return;
            displayedBalance = -1;
            RuntimeUiFactory.EnsureEventSystem();
            canvasObject = new GameObject("MerchantShopCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            // Player art uses a scaled root. Keep screen-space UI independent of that scale.
            DontDestroyOnLoad(canvasObject);
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 230;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            var promptBackground = RuntimeUiFactory.CreateImage("MerchantPrompt", canvasObject.transform, new Color(.10f, .035f, .055f, .94f));
            promptBackground.raycastTarget = false;
            prompt = promptBackground.rectTransform;
            Rect(prompt, new Vector2(250, 48), Vector2.zero);
            Text("Label", prompt, "[ F ]  거래하기", 26, Gold, Vector2.zero, new Vector2(238, 44));
            prompt.gameObject.SetActive(false);
            var shade = RuntimeUiFactory.CreateImage("MerchantShopOverlay", canvasObject.transform, new Color(.02f, .005f, .01f, .84f));
            overlay = shade.gameObject; RuntimeUiFactory.Stretch(shade.rectTransform);
            var panelSprite = GameUiTheme.LoadStatChoicePanel();
            var panel = RuntimeUiFactory.CreateImage("MerchantPanel", overlay.transform, panelSprite, panelSprite != null ? Color.white : PanelTrim);
            panel.preserveAspect = true;
            panel.raycastTarget = false;
            Rect(panel.rectTransform, new Vector2(1560, 680), Vector2.zero);

            // The title and footer cover the stat panel's baked-in labels.
            var title = RuntimeUiFactory.CreateImage("TitlePlaque", panel.transform, PanelTrim);
            Rect(title.rectTransform, new Vector2(276, 68), new Vector2(0, 247));
            Text("Title", title.transform, "세포 상인", 46, Gold, Vector2.zero, new Vector2(252, 60));
            Text("Subtitle", panel.transform, "살아 돌아갈 준비는 되었나?", 22, Ink, new Vector2(-400, 247), new Vector2(380, 44));
            var coin = RuntimeUiFactory.CreateImage("WalletCoin", panel.transform, Resources.Load<Sprite>(PlayerCurrencyUI.CoinResourcePath), Color.white);
            coin.preserveAspect = true; Rect(coin.rectTransform, new Vector2(44, 44), new Vector2(330, 247));
            balance = Text("Balance", panel.transform, "0", 32, Gold, new Vector2(475, 247), new Vector2(230, 48));
            balance.alignment = TextAnchor.MiddleLeft;
            balance.resizeTextForBestFit = true; balance.resizeTextMinSize = 16; balance.resizeTextMaxSize = 32;
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                var card = RuntimeUiFactory.CreateImage("ItemCard" + i, panel.transform, CardTint);
                Rect(card.rectTransform, new Vector2(380, 336), new Vector2(-400 + 400 * i, 35));
                icons[i] = RuntimeUiFactory.CreateImage("Icon", card.transform, Color.white); icons[i].preserveAspect = true;
                Rect(icons[i].rectTransform, new Vector2(88, 88), new Vector2(0, 110));
                names[i] = Text("Name", card.transform, "", 29, Ink, new Vector2(0, 42), new Vector2(348, 44));
                names[i].resizeTextForBestFit = true; names[i].resizeTextMinSize = 20; names[i].resizeTextMaxSize = 29;
                descriptions[i] = Text("Description", card.transform, "", 21, Muted, new Vector2(0, -35), new Vector2(342, 106));
                descriptions[i].horizontalOverflow = HorizontalWrapMode.Wrap; descriptions[i].verticalOverflow = VerticalWrapMode.Truncate;
                descriptions[i].resizeTextForBestFit = true; descriptions[i].resizeTextMinSize = 16; descriptions[i].resizeTextMaxSize = 21;
                buyButtons[i] = Button("BuyItem" + i, card.transform, "구매", new Vector2(0, -129), new Vector2(340, 52), () => BuyItem(index));
                buyLabels[i] = buyButtons[i].GetComponentInChildren<Text>();
            }
            var recoveryRow = RuntimeUiFactory.CreateImage("RecoveryRow", panel.transform, CardTint);
            Rect(recoveryRow.rectTransform, new Vector2(1180, 62), new Vector2(0, -173));
            Text("RecoveryTitle", recoveryRow.transform, "체력 회복 물약", 28, Ink, new Vector2(-390, 0), new Vector2(330, 44));
            Text("RecoveryDetail", recoveryRow.transform, "즉시 2HP 회복 · 반복 구매 가능", 21, Muted, new Vector2(20, 0), new Vector2(410, 44));
            recovery = Button("BuyRecovery", recoveryRow.transform, "회복  " + CurrencyEconomy.RecoveryPrice, new Vector2(455, 0), new Vector2(230, 46), BuyRecovery);
            var footer = RuntimeUiFactory.CreateImage("ShopFooter", panel.transform, PanelTrim);
            Rect(footer.rectTransform, new Vector2(1180, 54), new Vector2(0, -237));
            status = Text("Status", footer.transform, "", 21, Ink, new Vector2(-115, 0), new Vector2(840, 50));
            status.alignment = TextAnchor.MiddleLeft; status.horizontalOverflow = HorizontalWrapMode.Wrap;
            Button("Close", footer.transform, "닫기  [ESC / F]", new Vector2(455, 0), new Vector2(230, 44), Close);
            overlay.SetActive(false);
        }

        private static void Rect(RectTransform rect, Vector2 size, Vector2 position) => RuntimeUiFactory.SetRect(rect, new Vector2(.5f, .5f), size, position);
        private static Text Text(string name, Transform parent, string value, int size, Color color, Vector2 position, Vector2 dimensions)
        {
            var text = RuntimeUiFactory.CreateText(name, parent, value, GameUiTheme.LoadFont(), size, color);
            Rect(text.rectTransform, dimensions, position); text.alignment = TextAnchor.MiddleCenter;
            text.GetComponent<Shadow>().effectDistance = new Vector2(1, -1); return text;
        }
        private static Button Button(string name, Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            var image = RuntimeUiFactory.CreateImage(name, parent, new Color(.43f, .17f, .17f)); Rect(image.rectTransform, size, position);
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(action);
            var colors = button.colors; colors.highlightedColor = new Color(1, .86f, .64f); colors.pressedColor = new Color(.75f, .6f, .45f);
            colors.disabledColor = new Color(.4f, .35f, .35f, .6f); button.colors = colors;
            var text = Text("Label", image.transform, label, 25, Gold, Vector2.zero, size); text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 16; text.resizeTextMaxSize = 25; return button;
        }
    }
}
