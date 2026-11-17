using System.Globalization;
using System.Linq;
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
        private ModalInputScope inputScope;
        private float nextScan;
        public bool IsOpen => merchant != null && overlay != null && overlay.activeSelf;
        public WorldMerchantNpc NearbyMerchant => nearby;
        public bool IsInteractionPromptVisible => prompt != null && prompt.gameObject.activeSelf;

        private void Awake() { player = GetComponent<PlayerController>(); inventory = GetComponent<PlayerItemManager>(); }
        private void OnEnable() { SaveService.CurrencyChanged += OnBalanceChanged; SceneManager.activeSceneChanged += OnSceneChanged; }
        private void OnDisable() { Close(); SaveService.CurrencyChanged -= OnBalanceChanged; SceneManager.activeSceneChanged -= OnSceneChanged; if (prompt != null) prompt.gameObject.SetActive(false); }
        private void OnDestroy() { Close(); if (canvasObject != null) Destroy(canvasObject); }
        private void OnSceneChanged(Scene previous, Scene next) { Close(); nearby = null; nextScan = 0; }
        private void OnBalanceChanged(long value) { if (IsOpen) Refresh(); }

        private void Update()
        {
            var input = InputManager.Instance;
            if (IsOpen)
            {
                if (!WorldMerchantShop.IsInReach(merchant, player)) { Close(); return; }
                if (inputScope?.CanInteract == true && input.InteractAction.WasPressedThisFrame()) Close();
                return;
            }
            // A destroyed or unloaded NPC also releases the modal pause.
            if (inputScope != null) Close();
            if (Time.unscaledTime >= nextScan || input.InteractAction.WasPressedThisFrame())
            {
                nextScan = Time.unscaledTime + .2f;
                nearby = null;
                float best = float.MaxValue;
                foreach (var candidate in FindObjectsByType<WorldMerchantNpc>(FindObjectsSortMode.None))
                {
                    if (!WorldMerchantShop.IsInReach(candidate, player)) continue;
                    float distance = (candidate.transform.position - player.transform.position).sqrMagnitude;
                    if (distance < best) { nearby = candidate; best = distance; }
                }
            }
            bool available = WorldMerchantShop.IsInReach(nearby, player) && input.Gameplay.CanReadGameplay && input.Contexts.CanOpenAuxiliary;
            if (available) BuildUi();
            if (prompt != null) prompt.gameObject.SetActive(available && TryPositionPrompt());
            if (available && input.InteractAction.WasPressedThisFrame()) TryOpen(nearby);
        }

        private void LateUpdate()
        {
            // Follow the billboard after camera movement and other menus' Update callbacks.
            if (!IsInteractionPromptVisible) return;
            var input = InputManager.Instance;
            if (!WorldMerchantShop.IsInReach(nearby, player) || !input.Gameplay.CanReadGameplay
                || !input.Contexts.CanOpenAuxiliary || !TryPositionPrompt()) prompt.gameObject.SetActive(false);
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
            Refresh();
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
            status.text = message; Refresh();
        }
        private void BuyRecovery()
        {
            if (!IsOpen || inputScope?.CanInteract != true) return;
            WorldMerchantShop.TryBuyRecovery(merchant, player, out string message);
            status.text = message; Refresh();
        }

        private void Refresh()
        {
            if (stock == null || balance == null) return;
            balance.text = SaveService.CurrencyBalance.ToString("N0", CultureInfo.InvariantCulture);
            var ownedIds = inventory.AcquiredItems.Select(item => item.ItemId).ToArray();
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
                buyButtons[i].interactable = !sold && !owned && entry != null && SaveService.CurrencyBalance >= quote.Price;
            }
            recovery.interactable = !player.IsDead && player.CurrentHealth < player.MaxHealth - .0001f
                && SaveService.CurrencyBalance >= CurrencyEconomy.RecoveryPrice;
        }

        private void BuildUi()
        {
            if (canvasObject != null) return;
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
            var panel = RuntimeUiFactory.CreateImage("MerchantPanel", overlay.transform, new Color(.16f, .055f, .08f));
            Rect(panel.rectTransform, new Vector2(1200, 730), Vector2.zero);
            var line = RuntimeUiFactory.CreateImage("GoldRule", panel.transform, new Color(.7f, .43f, .18f));
            Rect(line.rectTransform, new Vector2(1100, 2), new Vector2(0, 226));
            Text("Title", panel.transform, "세포 상인", 46, Gold, new Vector2(-365, 302), new Vector2(380, 65));
            Text("Subtitle", panel.transform, "살아 돌아갈 준비는 되었나?", 22, Muted, new Vector2(-335, 254), new Vector2(440, 35));
            var coin = RuntimeUiFactory.CreateImage("WalletCoin", panel.transform, Resources.Load<Sprite>(PlayerCurrencyUI.CoinResourcePath), Color.white);
            coin.preserveAspect = true; Rect(coin.rectTransform, new Vector2(44, 44), new Vector2(280, 285));
            balance = Text("Balance", panel.transform, "0", 32, Gold, new Vector2(398, 285), new Vector2(180, 48));
            balance.alignment = TextAnchor.MiddleLeft;
            Button("Close", panel.transform, "닫기  [ESC / F]", new Vector2(425, -302), new Vector2(230, 50), Close);
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                var card = RuntimeUiFactory.CreateImage("ItemCard" + i, panel.transform, new Color(.24f, .095f, .12f));
                Rect(card.rectTransform, new Vector2(346, 360), new Vector2(-370 + 370 * i, 19));
                icons[i] = RuntimeUiFactory.CreateImage("Icon", card.transform, Color.white); icons[i].preserveAspect = true;
                Rect(icons[i].rectTransform, new Vector2(88, 88), new Vector2(0, 115));
                names[i] = Text("Name", card.transform, "", 29, Ink, new Vector2(0, 44), new Vector2(308, 46));
                names[i].resizeTextForBestFit = true; names[i].resizeTextMinSize = 20; names[i].resizeTextMaxSize = 29;
                descriptions[i] = Text("Description", card.transform, "", 21, Muted, new Vector2(0, -34), new Vector2(302, 106));
                descriptions[i].horizontalOverflow = HorizontalWrapMode.Wrap; descriptions[i].verticalOverflow = VerticalWrapMode.Truncate;
                descriptions[i].resizeTextForBestFit = true; descriptions[i].resizeTextMinSize = 16; descriptions[i].resizeTextMaxSize = 21;
                buyButtons[i] = Button("BuyItem" + i, card.transform, "구매", new Vector2(0, -135), new Vector2(298, 56), () => BuyItem(index));
                buyLabels[i] = buyButtons[i].GetComponentInChildren<Text>();
            }
            var recoveryRow = RuntimeUiFactory.CreateImage("RecoveryRow", panel.transform, new Color(.23f, .085f, .10f));
            Rect(recoveryRow.rectTransform, new Vector2(1086, 88), new Vector2(0, -225));
            Text("RecoveryTitle", recoveryRow.transform, "체력 회복 물약", 28, Ink, new Vector2(-337, 0), new Vector2(330, 44));
            Text("RecoveryDetail", recoveryRow.transform, "즉시 2HP 회복 · 반복 구매 가능", 21, Muted, new Vector2(20, 0), new Vector2(410, 44));
            recovery = Button("BuyRecovery", recoveryRow.transform, "회복  " + CurrencyEconomy.RecoveryPrice, new Vector2(405, 0), new Vector2(220, 56), BuyRecovery);
            status = Text("Status", panel.transform, "", 21, Muted, new Vector2(-135, -302), new Vector2(820, 60));
            status.alignment = TextAnchor.MiddleLeft; status.horizontalOverflow = HorizontalWrapMode.Wrap;
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
