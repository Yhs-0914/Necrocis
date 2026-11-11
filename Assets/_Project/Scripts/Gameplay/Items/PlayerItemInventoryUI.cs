using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Necrocis
{
    /// <summary>
    /// I 키로 열고 닫는 플레이어 아이템 인벤토리.
    /// 현재 보유 아이템의 정보 확인과 버리기를 담당한다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerItemManager))]
    public class PlayerItemInventoryUI : MonoBehaviour
    {
        private const string PanelSpritePath = "UI/Inventory/inventory_panel";
        private const string CardSpritePath = "UI/Inventory/inventory_card";

        private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.85f);
        private static readonly Color PanelFallbackColor = new Color(0.19f, 0.075f, 0.11f, 0.98f);
        private static readonly Color CardColor = Color.white;
        private static readonly Color EmptyCardColor = new Color(0.52f, 0.42f, 0.46f, 0.82f);
        private static readonly Color AccentColor = new Color(0.47f, 0.11f, 0.15f, 1f);
        private static readonly Color MainTextColor = new Color(0.93f, 0.81f, 0.76f, 1f);
        private static readonly Color MutedTextColor = new Color(0.72f, 0.56f, 0.57f, 1f);

        [Header("Inventory UI")]
        [SerializeField] private int sortingOrder = 210;
        [SerializeField] private bool pauseGameWhileOpen = true;

        private PlayerItemManager itemManager;
        private GameObject canvasObject;
        private Transform cardContainer;
        private Sprite panelSprite;
        private Sprite cardSprite;
        private bool isOpen;
        private ModalInputScope inputScope;
        private string replacementItemId;
        private Action replacementCollected;
        private Text replacementHint;
        private List<string> rewardOptions;
        private Action<string> rewardSelected;
        private ScrollRect itemScroll;
        private Button closeButton;
        private Text inventoryHint;
        private bool requiresRewardSelection;

        public bool RequestReward(IReadOnlyList<string> options, Action<string> onSelected, bool requireSelection = false)
        {
            if (isOpen || itemManager == null || options == null || Time.timeScale <= 0f
                || InputManager.Instance?.Contexts.CanOpenAuxiliary != true) return false;
            var valid = new List<string>();
            foreach (string id in options)
                if (itemManager.CanAcquireItem(id) && !valid.Contains(id)) valid.Add(id);
            if (valid.Count == 0) return false;
            rewardOptions = valid;
            rewardSelected = onSelected;
            requiresRewardSelection = requireSelection;
            SetOpen(true);
            return true;
        }

        public bool IsOpen => isOpen;

        public void RequestReplacement(string itemId, Action onCollected)
        {
            if (isOpen || itemManager == null || itemManager.ContainsItem(itemId)) return;
            replacementItemId = itemId;
            replacementCollected = onCollected;
            SetOpen(true);
        }

        private void Awake()
        {
            itemManager = GetComponent<PlayerItemManager>();
            BuildUi();
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += HandleSceneChanged;
            if (itemManager == null)
            {
                itemManager = GetComponent<PlayerItemManager>();
            }

            if (itemManager != null)
            {
                itemManager.ItemAcquired += HandleInventoryChanged;
                itemManager.ItemRemoved += HandleInventoryChanged;
            }
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= HandleSceneChanged;
            if (itemManager != null)
            {
                itemManager.ItemAcquired -= HandleInventoryChanged;
                itemManager.ItemRemoved -= HandleInventoryChanged;
            }

            if (isOpen)
            {
                requiresRewardSelection = false;
                SetOpen(false);
            }
        }

        private void OnDestroy()
        {
            RestoreTimeScale();
        }

        private void HandleSceneChanged(Scene previous, Scene next)
        {
            if (isOpen)
            {
                requiresRewardSelection = false;
                SetOpen(false);
            }
        }

        private void Update()
        {
            TryOpenStartingReward();
            InputManager input = InputManager.Instance;
            if (!requiresRewardSelection && input != null && input.InventoryAction.WasPressedThisFrame()
                && (isOpen ? inputScope?.CanInteract == true : input.Contexts.CanOpenAuxiliary))
            {
                SetOpen(!isOpen);
            }
        }

        private void TryOpenStartingReward()
        {
            if (isOpen || itemManager == null || SaveService.IsRestorePending || Time.timeScale <= 0f
                || SceneManager.GetActiveScene().name != SceneLoader.SCENE_HUB
                || (SceneLoader.Instance != null && SceneLoader.Instance.IsLoading)) return;

            var reward = SaveService.FindWorldReward(CampaignRewardRules.StarterRewardId);
            // Older saves may still have an unopened starter box in a biome.
            if (reward == null || reward.collected || reward.biome != BiomeType.None) return;

            var catalog = itemManager.ItemEntries.Where(item => item != null).Select(item => item.ItemId);
            var owned = itemManager.AcquiredItems.Select(item => item.ItemId);
            var offers = reward.itemChoices == null || reward.itemChoices.Count == 0
                ? CampaignRewardRules.ChooseStarterOffers(catalog, owned, UnityEngine.Random.Range(1, int.MaxValue))
                : CampaignRewardRules.RefreshOffers(reward.itemChoices, catalog, owned, null, 0);
            if (offers.Count == 0) return;
            if (reward.itemChoices == null || !reward.itemChoices.SequenceEqual(offers))
            {
                reward.itemChoices = offers;
                SaveService.TrySaveActiveRun(out _);
            }

            RequestReward(offers, selected =>
            {
                var player = GetComponent<PlayerController>();
                if (player != null)
                    WorldItemRewardChoice.PlayPickup(player, selected, transform.position + Vector3.up * 0.45f);
                SaveService.CompleteWorldReward(reward);
            }, requireSelection: true);
        }

        public void SetOpen(bool open)
        {
            if (!open && requiresRewardSelection) return;
            if (isOpen == open || open && InputManager.Instance?.Contexts.CanOpenAuxiliary != true)
            {
                return;
            }

            isOpen = open;
            if (open)
            {
                BuildUi();
                Refresh();
                canvasObject.SetActive(true);
                PauseGame();
            }
            else
            {
                replacementItemId = null;
                replacementCollected = null;
                rewardOptions = null;
                rewardSelected = null;
                if (canvasObject != null)
                {
                    canvasObject.SetActive(false);
                }

                RestoreTimeScale();
            }
        }

        private void HandleInventoryChanged(PlayerItemManager _, PlayerItemManager.AcquiredPlayerItem __)
        {
            if (isOpen)
            {
                Refresh();
            }
        }

        private void DiscardItem(string itemId)
        {
            if (inputScope?.CanInteract != true || itemManager == null || string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            if (!string.IsNullOrEmpty(replacementItemId))
            {
                if (!itemManager.TryReplaceItem(itemId, replacementItemId)) return;
                Action collected = replacementCollected;
                SetOpen(false);
                collected?.Invoke();
                AudioManager.Instance?.PlaySFX("ItemPickup");
                return;
            }

            if (itemManager.RemoveItem(itemId))
            {
                Debug.Log($"[PlayerItemInventoryUI] 아이템 버림: {itemId}");
            }
        }

        private void SelectReward(string itemId)
        {
            if (inputScope?.CanInteract != true || rewardOptions == null || !rewardOptions.Contains(itemId)) return;
            Action<string> selected = rewardSelected;
            if (!itemManager.TryAcquireItem(itemId, out _)) return;
            requiresRewardSelection = false;
            SetOpen(false);
            selected?.Invoke(itemId);
            AudioManager.Instance?.PlaySFX("ItemPickup");
        }

        private void PauseGame()
        {
            inputScope ??= new ModalInputScope(this, canvasObject, pauseGameWhileOpen, () => SetOpen(false));
        }

        private void RestoreTimeScale()
        {
            inputScope?.Dispose(); inputScope = null;
        }

        private void BuildUi()
        {
            if (canvasObject != null)
            {
                return;
            }

            EnsureEventSystem();

            panelSprite = Resources.Load<Sprite>(PanelSpritePath);
            cardSprite = Resources.Load<Sprite>(CardSpritePath);

            canvasObject = CreateUiObject("PlayerItemInventoryCanvas", transform);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject overlay = CreateUiObject("Overlay", canvasObject.transform);
            StretchToParent(overlay.GetComponent<RectTransform>());
            overlay.AddComponent<Image>().color = OverlayColor;

            GameObject panel = CreateUiObject("InventoryPanel", overlay.transform);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(1600f, 760f);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.sprite = panelSprite;
            panelImage.color = panelSprite != null ? Color.white : PanelFallbackColor;
            panelImage.preserveAspect = panelSprite != null;

            if (panelSprite == null)
            {
                Outline panelOutline = panel.AddComponent<Outline>();
                panelOutline.effectColor = new Color(0.53f, 0.31f, 0.32f, 0.95f);
                panelOutline.effectDistance = new Vector2(3f, -3f);

                Text fallbackTitle = CreateText("Title", panel.transform, 44, FontStyle.Bold, TextAnchor.MiddleCenter);
                SetRect(fallbackTitle.rectTransform, new Vector2(0f, 320f), new Vector2(900f, 64f));
                fallbackTitle.text = "아이템";
                fallbackTitle.color = MainTextColor;
            }

            closeButton = CreateButton(
                "CloseButton",
                panel.transform,
                new Vector2(650f, 270f),
                new Vector2(52f, 46f),
                "×",
                new Color(0.32f, 0.085f, 0.12f, 0.96f));
            closeButton.onClick.AddListener(() => { if (inputScope?.CanInteract == true) SetOpen(false); });

            GameObject scrollObject = CreateUiObject("ItemScroll", panel.transform);
            SetRect(scrollObject.GetComponent<RectTransform>(), new Vector2(0f, -22f), new Vector2(1190f, 520f));
            itemScroll = scrollObject.AddComponent<ScrollRect>();
            itemScroll.horizontal = true;
            itemScroll.vertical = false;
            itemScroll.movementType = ScrollRect.MovementType.Clamped;
            itemScroll.scrollSensitivity = 55f;
            GameObject viewport = CreateUiObject("Viewport", scrollObject.transform);
            StretchToParent(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            itemScroll.viewport = viewport.GetComponent<RectTransform>();

            GameObject cards = CreateUiObject("ItemCards", viewport.transform);
            RectTransform cardsRect = cards.GetComponent<RectTransform>();
            cardsRect.anchorMin = new Vector2(0f, 0f);
            cardsRect.anchorMax = new Vector2(0f, 1f);
            cardsRect.pivot = new Vector2(0f, 0.5f);
            cardsRect.sizeDelta = Vector2.zero;
            itemScroll.content = cardsRect;
            HorizontalLayoutGroup layout = cards.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 34f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            cards.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            cardContainer = cards.transform;

            Text hint = CreateText("Hint", panel.transform, 22, FontStyle.Normal, TextAnchor.MiddleCenter);
            inventoryHint = hint;
            SetRect(hint.rectTransform, new Vector2(0f, -326f), new Vector2(1100f, 40f));
            hint.text = "I 키로 닫기  ·  휠 / 드래그로 전체 보기  ·  보유 개수 제한 없음";
            hint.color = MutedTextColor;

            replacementHint = CreateText("ReplacementHint", panel.transform, 22, FontStyle.Normal, TextAnchor.MiddleCenter);
            SetRect(replacementHint.rectTransform, new Vector2(0f, 312f), new Vector2(1250f, 88f));
            replacementHint.color = MainTextColor;

            canvasObject.SetActive(false);
        }

        private void Refresh()
        {
            if (itemManager == null || cardContainer == null)
            {
                return;
            }

            replacementHint.text = !string.IsNullOrEmpty(replacementItemId)
                && itemManager.TryGetItemEntry(replacementItemId, out var incoming)
                ? $"{incoming.DisplayName}\n{incoming.Description}\n교체할 아이템을 선택하세요. I 키로 취소할 수 있습니다."
                : string.Empty;
            if (rewardOptions != null) replacementHint.text = requiresRewardSelection
                ? $"시작 아이템 선택\n아이템 {rewardOptions.Count}개 중 1개를 선택하고 모험을 시작하세요."
                : $"아이템 {rewardOptions.Count}개 중 1개를 선택하세요.\nI 키로 닫으면 상자에 다시 접근해 선택할 수 있습니다.";
            else if (string.IsNullOrEmpty(replacementItemId)) replacementHint.text = $"보유 아이템 {itemManager.ItemCount}개 · 개수 제한 없음";
            closeButton.gameObject.SetActive(!requiresRewardSelection);
            inventoryHint.text = requiresRewardSelection
                ? "선택한 아이템을 즉시 획득합니다. · 이번 모험에서 1회만 선택할 수 있습니다."
                : "I 키로 닫기  ·  휠 / 드래그로 전체 보기  ·  보유 개수 제한 없음";

            for (int i = cardContainer.childCount - 1; i >= 0; i--)
            {
                GameObject child = cardContainer.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
            itemScroll.StopMovement();
            itemScroll.horizontalNormalizedPosition = 0f;

            if (rewardOptions != null)
            {
                for (int i = 0; i < rewardOptions.Count; i++)
                    if (itemManager.TryGetItemEntry(rewardOptions[i], out var entry))
                        CreateItemCard(new PlayerItemManager.AcquiredPlayerItem(entry), i, 360f, true);
                return;
            }

            IReadOnlyList<PlayerItemManager.AcquiredPlayerItem> items = itemManager.AcquiredItems;
            int slotCount = Mathf.Max(1, items.Count);
            const float cardWidth = 360f;

            for (int i = 0; i < slotCount; i++)
            {
                if (i < items.Count && items[i] != null)
                {
                    CreateItemCard(items[i], i, cardWidth);
                }
                else
                {
                    CreateEmptyCard(i, cardWidth);
                }
            }
        }

        private void CreateItemCard(PlayerItemManager.AcquiredPlayerItem item, int slotIndex, float width, bool offer = false)
        {
            GameObject card = CreateCardRoot($"Item_{slotIndex + 1}", width, CardColor);

            Text name = CreateText("Name", card.transform, 29, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetRect(name.rectTransform, new Vector2(0f, 174f), new Vector2(width - 52f, 54f));
            name.text = GetItemName(item);
            name.color = MainTextColor;

            GameObject iconFrame = CreateUiObject("IconFrame", card.transform);
            RectTransform iconFrameRect = iconFrame.GetComponent<RectTransform>();
            SetRect(iconFrameRect, new Vector2(0f, 66f), new Vector2(142f, 142f));
            Image iconFrameImage = iconFrame.AddComponent<Image>();
            iconFrameImage.color = new Color(0.12f, 0.035f, 0.055f, 0.9f);
            Outline iconOutline = iconFrame.AddComponent<Outline>();
            iconOutline.effectColor = new Color(0.48f, 0.27f, 0.27f, 0.95f);
            iconOutline.effectDistance = new Vector2(2f, -2f);

            if (item.Icon != null)
            {
                GameObject iconObject = CreateUiObject("Icon", iconFrame.transform);
                RectTransform iconRect = iconObject.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = new Vector2(124f, 124f);
                Image icon = iconObject.AddComponent<Image>();
                icon.sprite = item.Icon;
                icon.preserveAspect = true;
            }
            else
            {
                Text noIcon = CreateText("NoIcon", iconFrame.transform, 18, FontStyle.Normal, TextAnchor.MiddleCenter);
                StretchToParent(noIcon.rectTransform);
                noIcon.text = "이미지 없음";
                noIcon.color = MutedTextColor;
            }

            Text description = CreateText("Description", card.transform, 20, FontStyle.Normal, TextAnchor.UpperCenter);
            SetRect(description.rectTransform, new Vector2(0f, -94f), new Vector2(width - 52f, 140f));
            description.text = string.IsNullOrWhiteSpace(item.Description) ? "아이템 설명이 없습니다." : item.Description;
            description.color = MainTextColor;

            string itemId = item.ItemId;
            Button discardButton = CreateButton(
                "DiscardButton",
                card.transform,
                new Vector2(0f, -194f),
                new Vector2(width - 92f, 48f),
                offer ? "선택" : string.IsNullOrEmpty(replacementItemId) ? "버리기" : "이 아이템과 교체",
                AccentColor);
            discardButton.onClick.AddListener(() => { if (offer) SelectReward(itemId); else DiscardItem(itemId); });
        }

        private void CreateEmptyCard(int slotIndex, float width)
        {
            GameObject card = CreateCardRoot($"Empty_{slotIndex + 1}", width, EmptyCardColor);

            Text empty = CreateText("Empty", card.transform, 27, FontStyle.Normal, TextAnchor.MiddleCenter);
            SetRect(empty.rectTransform, Vector2.zero, new Vector2(width - 40f, 120f));
            empty.text = "보유 아이템 없음";
            empty.color = MutedTextColor;
        }

        private GameObject CreateCardRoot(string name, float width, Color color)
        {
            GameObject card = CreateUiObject(name, cardContainer);
            RectTransform rect = card.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, 510f);
            LayoutElement layoutElement = card.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = width;
            layoutElement.preferredHeight = 510f;
            Image image = card.AddComponent<Image>();
            image.sprite = cardSprite;
            image.color = cardSprite != null ? color : new Color(0.22f, 0.08f, 0.11f, color.a);
            image.preserveAspect = cardSprite != null;

            if (cardSprite == null)
            {
                Outline outline = card.AddComponent<Outline>();
                outline.effectColor = new Color(0.56f, 0.33f, 0.31f, 0.9f);
                outline.effectDistance = new Vector2(2f, -2f);
            }
            return card;
        }

        private static string GetItemName(PlayerItemManager.AcquiredPlayerItem item)
        {
            return string.IsNullOrWhiteSpace(item.DisplayName) ? item.ItemId : item.DisplayName;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            Vector2 position,
            Vector2 size,
            string label,
            Color color)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            SetRect(rect, position, size);

            Image image = buttonObject.AddComponent<Image>();
            image.color = color;
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.72f, 0.72f, 1f);
            colors.pressedColor = new Color(0.72f, 0.52f, 0.52f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.colorMultiplier = 1f;
            button.colors = colors;

            Outline outline = buttonObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.66f, 0.36f, 0.34f, 0.95f);
            outline.effectDistance = new Vector2(2f, -2f);

            Text text = CreateText("Label", buttonObject.transform, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
            StretchToParent(text.rectTransform);
            text.text = label;
            text.color = MainTextColor;
            return button;
        }

        private static Text CreateText(string name, Transform parent, int fontSize, FontStyle style, TextAnchor alignment)
        {
            GameObject textObject = CreateUiObject(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.font = GameUiTheme.LoadFont();
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            Shadow shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.08f, 0.005f, 0.012f, 0.95f);
            shadow.effectDistance = new Vector2(2f, -2f);
            return text;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem");
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

            InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
            {
                inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            eventSystem.enabled = true;
            inputModule.enabled = true;
        }
    }
}
