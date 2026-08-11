using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

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
        private static readonly Color OverlayColor = new Color(0.01f, 0.015f, 0.025f, 0.78f);
        private static readonly Color PanelColor = new Color(0.055f, 0.07f, 0.095f, 0.98f);
        private static readonly Color CardColor = new Color(0.09f, 0.115f, 0.15f, 1f);
        private static readonly Color EmptyCardColor = new Color(0.065f, 0.08f, 0.105f, 0.85f);
        private static readonly Color AccentColor = new Color(0.72f, 0.18f, 0.2f, 1f);

        [Header("Inventory UI")]
        [SerializeField] private int sortingOrder = 210;
        [SerializeField] private bool pauseGameWhileOpen = true;

        private PlayerItemManager itemManager;
        private GameObject canvasObject;
        private Transform cardContainer;
        private Text slotCountText;
        private bool isOpen;
        private bool ownsPause;
        private float previousTimeScale = 1f;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            itemManager = GetComponent<PlayerItemManager>();
            BuildUi();
        }

        private void OnEnable()
        {
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
            if (itemManager != null)
            {
                itemManager.ItemAcquired -= HandleInventoryChanged;
                itemManager.ItemRemoved -= HandleInventoryChanged;
            }

            if (isOpen)
            {
                SetOpen(false);
            }
        }

        private void OnDestroy()
        {
            RestoreTimeScale();
        }

        private void Update()
        {
            InputManager input = InputManager.Instance;
            if (input != null && input.InventoryAction.WasPressedThisFrame())
            {
                SetOpen(!isOpen);
            }
        }

        public void SetOpen(bool open)
        {
            if (isOpen == open)
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
            if (itemManager == null || string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            if (itemManager.RemoveItem(itemId))
            {
                Debug.Log($"[PlayerItemInventoryUI] 아이템 버림: {itemId}");
            }
        }

        private void PauseGame()
        {
            ownsPause = false;
            if (!pauseGameWhileOpen || Time.timeScale <= Mathf.Epsilon)
            {
                return;
            }

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            ownsPause = true;
        }

        private void RestoreTimeScale()
        {
            if (!ownsPause)
            {
                return;
            }

            if (Time.timeScale <= Mathf.Epsilon)
            {
                Time.timeScale = previousTimeScale;
            }

            ownsPause = false;
        }

        private void BuildUi()
        {
            if (canvasObject != null)
            {
                return;
            }

            EnsureEventSystem();

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
            panelRect.sizeDelta = new Vector2(1540f, 790f);
            panel.AddComponent<Image>().color = PanelColor;
            Outline panelOutline = panel.AddComponent<Outline>();
            panelOutline.effectColor = new Color(0.25f, 0.3f, 0.38f, 0.9f);
            panelOutline.effectDistance = new Vector2(2f, -2f);

            Text title = CreateText("Title", panel.transform, 42, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 335f), new Vector2(900f, 64f));
            title.text = "보유 아이템";

            slotCountText = CreateText("SlotCount", panel.transform, 25, FontStyle.Normal, TextAnchor.MiddleCenter);
            SetRect(slotCountText.rectTransform, new Vector2(0f, 286f), new Vector2(700f, 42f));
            slotCountText.color = new Color(0.72f, 0.8f, 0.9f, 1f);

            Button closeButton = CreateButton(
                "CloseButton",
                panel.transform,
                new Vector2(704f, 339f),
                new Vector2(70f, 54f),
                "X",
                new Color(0.25f, 0.28f, 0.33f, 1f));
            closeButton.onClick.AddListener(() => SetOpen(false));

            GameObject cards = CreateUiObject("ItemCards", panel.transform);
            RectTransform cardsRect = cards.GetComponent<RectTransform>();
            SetRect(cardsRect, new Vector2(0f, -25f), new Vector2(1400f, 560f));
            HorizontalLayoutGroup layout = cards.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 28f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            cardContainer = cards.transform;

            Text hint = CreateText("Hint", panel.transform, 22, FontStyle.Normal, TextAnchor.MiddleCenter);
            SetRect(hint.rectTransform, new Vector2(0f, -354f), new Vector2(1100f, 40f));
            hint.text = "I 키로 닫기  ·  아이템을 버리면 빈 슬롯에 새로운 아이템을 획득할 수 있습니다.";
            hint.color = new Color(0.65f, 0.69f, 0.75f, 1f);

            canvasObject.SetActive(false);
        }

        private void Refresh()
        {
            if (itemManager == null || cardContainer == null)
            {
                return;
            }

            for (int i = cardContainer.childCount - 1; i >= 0; i--)
            {
                GameObject child = cardContainer.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }

            IReadOnlyList<PlayerItemManager.AcquiredPlayerItem> items = itemManager.AcquiredItems;
            int slotCount = Mathf.Max(1, itemManager.MaxItemSlots);
            float cardWidth = Mathf.Clamp((1400f - 28f * (slotCount - 1)) / slotCount, 280f, 430f);

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

            slotCountText.text = $"슬롯  {itemManager.ItemCount} / {itemManager.MaxItemSlots}";
        }

        private void CreateItemCard(PlayerItemManager.AcquiredPlayerItem item, int slotIndex, float width)
        {
            GameObject card = CreateCardRoot($"Item_{slotIndex + 1}", width, CardColor);

            Text slotLabel = CreateText("Slot", card.transform, 19, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(slotLabel.rectTransform, new Vector2(-width * 0.5f + 58f, 252f), new Vector2(90f, 30f));
            slotLabel.text = $"SLOT {slotIndex + 1}";
            slotLabel.color = new Color(0.52f, 0.6f, 0.7f, 1f);

            Text name = CreateText("Name", card.transform, 30, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetRect(name.rectTransform, new Vector2(0f, 210f), new Vector2(width - 34f, 55f));
            name.text = GetItemName(item);

            GameObject iconFrame = CreateUiObject("IconFrame", card.transform);
            RectTransform iconFrameRect = iconFrame.GetComponent<RectTransform>();
            SetRect(iconFrameRect, new Vector2(0f, 86f), new Vector2(170f, 170f));
            iconFrame.AddComponent<Image>().color = new Color(0.025f, 0.035f, 0.05f, 0.95f);

            if (item.Icon != null)
            {
                GameObject iconObject = CreateUiObject("Icon", iconFrame.transform);
                RectTransform iconRect = iconObject.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = new Vector2(146f, 146f);
                Image icon = iconObject.AddComponent<Image>();
                icon.sprite = item.Icon;
                icon.preserveAspect = true;
            }
            else
            {
                Text noIcon = CreateText("NoIcon", iconFrame.transform, 18, FontStyle.Normal, TextAnchor.MiddleCenter);
                StretchToParent(noIcon.rectTransform);
                noIcon.text = "이미지 없음";
                noIcon.color = new Color(0.48f, 0.52f, 0.58f, 1f);
            }

            Text description = CreateText("Description", card.transform, 22, FontStyle.Normal, TextAnchor.UpperCenter);
            SetRect(description.rectTransform, new Vector2(0f, -75f), new Vector2(width - 50f, 130f));
            description.text = string.IsNullOrWhiteSpace(item.Description) ? "아이템 설명이 없습니다." : item.Description;
            description.color = new Color(0.82f, 0.84f, 0.88f, 1f);

            string itemId = item.ItemId;
            Button discardButton = CreateButton(
                "DiscardButton",
                card.transform,
                new Vector2(0f, -224f),
                new Vector2(width - 66f, 58f),
                "버리기",
                AccentColor);
            discardButton.onClick.AddListener(() => DiscardItem(itemId));
        }

        private void CreateEmptyCard(int slotIndex, float width)
        {
            GameObject card = CreateCardRoot($"Empty_{slotIndex + 1}", width, EmptyCardColor);

            Text slotLabel = CreateText("Slot", card.transform, 19, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(slotLabel.rectTransform, new Vector2(-width * 0.5f + 58f, 252f), new Vector2(90f, 30f));
            slotLabel.text = $"SLOT {slotIndex + 1}";
            slotLabel.color = new Color(0.4f, 0.45f, 0.52f, 1f);

            Text empty = CreateText("Empty", card.transform, 27, FontStyle.Normal, TextAnchor.MiddleCenter);
            SetRect(empty.rectTransform, Vector2.zero, new Vector2(width - 40f, 120f));
            empty.text = "+\n빈 슬롯";
            empty.color = new Color(0.38f, 0.43f, 0.5f, 1f);
        }

        private GameObject CreateCardRoot(string name, float width, Color color)
        {
            GameObject card = CreateUiObject(name, cardContainer);
            RectTransform rect = card.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, 550f);
            LayoutElement layoutElement = card.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = width;
            layoutElement.preferredHeight = 550f;
            card.AddComponent<Image>().color = color;
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(0.22f, 0.27f, 0.34f, 0.8f);
            outline.effectDistance = new Vector2(1f, -1f);
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
            colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            Text text = CreateText("Label", buttonObject.transform, 23, FontStyle.Bold, TextAnchor.MiddleCenter);
            StretchToParent(text.rectTransform);
            text.text = label;
            return button;
        }

        private static Text CreateText(string name, Transform parent, int fontSize, FontStyle style, TextAnchor alignment)
        {
            GameObject textObject = CreateUiObject(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
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
