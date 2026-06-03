using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Necrocis
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerItemManager))]
    public class PlayerItemTestPanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private bool enablePanel = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F8;
        [SerializeField] private int sortingOrder = 220;

        private PlayerItemManager itemManager;
        private GameObject canvasObject;
        private GameObject panelObject;
        private Transform catalogContent;
        private Transform acquiredContent;
        private Text titleText;
        private Text summaryText;
        private Text detailText;
        private Text statusText;

        private string selectedCatalogItemId;
        private string selectedAcquiredItemId;

        private void Awake()
        {
            itemManager = GetComponent<PlayerItemManager>();
            EnsureUi();
        }

        private void OnEnable()
        {
            if (itemManager == null)
            {
                return;
            }

            itemManager.ItemAcquired += HandleItemChanged;
            itemManager.ItemRemoved += HandleItemChanged;
        }

        private void OnDisable()
        {
            if (itemManager == null)
            {
                return;
            }

            itemManager.ItemAcquired -= HandleItemChanged;
            itemManager.ItemRemoved -= HandleItemChanged;
        }

        private void Start()
        {
            if (panelObject == null)
            {
                return;
            }

            panelObject.SetActive(false);
            RefreshLists();
            UpdateStatus($"{toggleKey}: 테스트 패널 열기/닫기");
        }

        private void Update()
        {
            if (!enablePanel || panelObject == null)
            {
                return;
            }

            if (IsTogglePressedThisFrame())
            {
                bool visible = !panelObject.activeSelf;
                panelObject.SetActive(visible);
                if (visible)
                {
                    RefreshLists();
                }
            }
        }

        private void HandleItemChanged(PlayerItemManager _, PlayerItemManager.AcquiredPlayerItem __)
        {
            RefreshLists();
        }

        private void EnsureUi()
        {
            if (canvasObject != null)
            {
                return;
            }

            EnsureEventSystem();

            canvasObject = CreateUiObject("ItemTestCanvas", transform);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            panelObject = CreateUiObject("Panel", canvasObject.transform);
            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0.06f, 0.1f, 0.14f, 0.92f);
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.offsetMin = new Vector2(24f, 24f);
            panelRect.offsetMax = new Vector2(-24f, -24f);

            titleText = CreateLabel("Title", panelObject.transform, new Vector2(0f, 470f), new Vector2(1740f, 56f), 36, TextAnchor.MiddleCenter);
            titleText.text = "아이템 테스트 패널";
            titleText.color = new Color(0.95f, 0.96f, 0.98f, 1f);

            summaryText = CreateLabel("Summary", panelObject.transform, new Vector2(0f, 420f), new Vector2(1740f, 42f), 26, TextAnchor.MiddleCenter);
            summaryText.color = new Color(0.77f, 0.89f, 0.98f, 1f);

            detailText = CreateLabel("Detail", panelObject.transform, new Vector2(0f, 365f), new Vector2(1740f, 60f), 24, TextAnchor.MiddleCenter);
            detailText.color = new Color(0.93f, 0.93f, 0.93f, 1f);

            CreateLabel("CatalogLabel", panelObject.transform, new Vector2(-450f, 315f), new Vector2(840f, 40f), 26, TextAnchor.MiddleLeft).text = "전체 아이템 목록";
            CreateLabel("AcquiredLabel", panelObject.transform, new Vector2(450f, 315f), new Vector2(840f, 40f), 26, TextAnchor.MiddleLeft).text = "현재 보유 아이템";

            catalogContent = CreateScrollList(panelObject.transform, new Vector2(-450f, -30f), new Vector2(840f, 660f));
            acquiredContent = CreateScrollList(panelObject.transform, new Vector2(450f, -30f), new Vector2(840f, 660f));

            CreateActionButton("AddButton", panelObject.transform, new Vector2(-310f, -425f), new Vector2(300f, 68f), "선택 아이템 추가", AddSelectedCatalogItem, new Color(0.18f, 0.44f, 0.22f, 1f));
            CreateActionButton("RemoveButton", panelObject.transform, new Vector2(10f, -425f), new Vector2(300f, 68f), "선택 아이템 삭제", RemoveSelectedAcquiredItem, new Color(0.5f, 0.18f, 0.18f, 1f));
            CreateActionButton("ClearButton", panelObject.transform, new Vector2(300f, -425f), new Vector2(220f, 68f), "전체 삭제", ClearAllItems, new Color(0.41f, 0.14f, 0.14f, 1f));
            CreateActionButton("RefreshButton", panelObject.transform, new Vector2(-600f, -425f), new Vector2(220f, 68f), "새로고침", RefreshLists, new Color(0.2f, 0.3f, 0.42f, 1f));

            statusText = CreateLabel("Status", panelObject.transform, new Vector2(0f, -500f), new Vector2(1740f, 46f), 24, TextAnchor.MiddleCenter);
            statusText.color = new Color(0.97f, 0.83f, 0.35f, 1f);
            panelObject.SetActive(false);
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        private bool IsTogglePressedThisFrame()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            return TryGetKeyboardControl(toggleKey, keyboard, out ButtonControl keyControl)
                && keyControl.wasPressedThisFrame;
        }

        private static bool TryGetKeyboardControl(KeyCode keyCode, Keyboard keyboard, out ButtonControl keyControl)
        {
            switch (keyCode)
            {
                case KeyCode.F1: keyControl = keyboard.f1Key; return true;
                case KeyCode.F2: keyControl = keyboard.f2Key; return true;
                case KeyCode.F3: keyControl = keyboard.f3Key; return true;
                case KeyCode.F4: keyControl = keyboard.f4Key; return true;
                case KeyCode.F5: keyControl = keyboard.f5Key; return true;
                case KeyCode.F6: keyControl = keyboard.f6Key; return true;
                case KeyCode.F7: keyControl = keyboard.f7Key; return true;
                case KeyCode.F8: keyControl = keyboard.f8Key; return true;
                case KeyCode.F9: keyControl = keyboard.f9Key; return true;
                case KeyCode.F10: keyControl = keyboard.f10Key; return true;
                case KeyCode.F11: keyControl = keyboard.f11Key; return true;
                case KeyCode.F12: keyControl = keyboard.f12Key; return true;
                case KeyCode.BackQuote: keyControl = keyboard.backquoteKey; return true;
                case KeyCode.Tab: keyControl = keyboard.tabKey; return true;
                case KeyCode.Escape: keyControl = keyboard.escapeKey; return true;
                default:
                    keyControl = null;
                    return false;
            }
        }

        private Transform CreateScrollList(Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject root = CreateUiObject("ScrollRoot", parent);
            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.26f);

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = size;

            GameObject viewport = CreateUiObject("Viewport", root.transform);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(8f, 8f);
            viewportRect.offsetMax = new Vector2(-8f, -8f);
            viewport.AddComponent<RectMask2D>();
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.16f);

            GameObject content = CreateUiObject("Content", viewport.transform);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 0f);

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            ScrollRect scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 28f;

            return content.transform;
        }

        private void RefreshLists()
        {
            if (itemManager == null || catalogContent == null || acquiredContent == null)
            {
                return;
            }

            for (int i = catalogContent.childCount - 1; i >= 0; i--)
            {
                Destroy(catalogContent.GetChild(i).gameObject);
            }

            for (int i = acquiredContent.childCount - 1; i >= 0; i--)
            {
                Destroy(acquiredContent.GetChild(i).gameObject);
            }

            IReadOnlyList<PlayerItemManager.PlayerItemEntry> entries = itemManager.ItemEntries;
            for (int i = 0; i < entries.Count; i++)
            {
                PlayerItemManager.PlayerItemEntry entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    continue;
                }

                string label = $"{entry.DisplayName} ({entry.ItemId})";
                if (!string.IsNullOrWhiteSpace(entry.Description))
                {
                    label = $"{label} - {entry.Description}";
                }

                bool selected = string.Equals(selectedCatalogItemId, entry.ItemId, StringComparison.OrdinalIgnoreCase);
                CreateItemRow(catalogContent, label, selected, () =>
                {
                    selectedCatalogItemId = entry.ItemId;
                    detailText.text = $"선택: {entry.DisplayName} ({entry.ItemId})";
                    RefreshLists();
                });
            }

            IReadOnlyList<PlayerItemManager.AcquiredPlayerItem> acquired = itemManager.AcquiredItems;
            for (int i = 0; i < acquired.Count; i++)
            {
                PlayerItemManager.AcquiredPlayerItem item = acquired[i];
                if (item == null || string.IsNullOrWhiteSpace(item.ItemId))
                {
                    continue;
                }

                string label = $"{i + 1}. {item.DisplayName} ({item.ItemId})";
                bool selected = string.Equals(selectedAcquiredItemId, item.ItemId, StringComparison.OrdinalIgnoreCase);
                CreateItemRow(acquiredContent, label, selected, () =>
                {
                    selectedAcquiredItemId = item.ItemId;
                    detailText.text = $"삭제 대상: {item.DisplayName} ({item.ItemId})";
                    RefreshLists();
                });
            }

            summaryText.text = $"슬롯: {itemManager.ItemCount}/{itemManager.MaxItemSlots} | 카탈로그: {entries.Count}";
        }

        private void AddSelectedCatalogItem()
        {
            if (itemManager == null || string.IsNullOrWhiteSpace(selectedCatalogItemId))
            {
                UpdateStatus("추가할 아이템을 먼저 선택하세요.");
                return;
            }

            if (itemManager.TryAcquireItem(selectedCatalogItemId, out PlayerItemAcquireFailureReason failure))
            {
                selectedAcquiredItemId = selectedCatalogItemId;
                UpdateStatus($"추가 성공: {selectedCatalogItemId}");
                return;
            }

            UpdateStatus($"추가 실패: {selectedCatalogItemId} ({failure})");
        }

        private void RemoveSelectedAcquiredItem()
        {
            if (itemManager == null)
            {
                return;
            }

            string itemId = selectedAcquiredItemId;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                UpdateStatus("삭제할 보유 아이템을 먼저 선택하세요.");
                return;
            }

            if (itemManager.RemoveItem(itemId))
            {
                UpdateStatus($"삭제 성공: {itemId}");
                if (!itemManager.ContainsItem(itemId))
                {
                    selectedAcquiredItemId = null;
                }
                RefreshLists();
                return;
            }

            UpdateStatus($"삭제 실패: {itemId}");
        }

        private void ClearAllItems()
        {
            if (itemManager == null)
            {
                return;
            }

            itemManager.ClearAllItems();
            selectedAcquiredItemId = null;
            UpdateStatus("모든 보유 아이템을 삭제했습니다.");
            RefreshLists();
        }

        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private void CreateItemRow(Transform parent, string label, bool selected, Action onClick)
        {
            GameObject row = CreateUiObject("Row", parent);
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 62f);

            Image rowImage = row.AddComponent<Image>();
            rowImage.color = selected
                ? new Color(0.24f, 0.49f, 0.67f, 0.95f)
                : new Color(0.14f, 0.17f, 0.21f, 0.9f);

            Button button = row.AddComponent<Button>();
            button.onClick.AddListener(() => onClick?.Invoke());

            Text rowText = CreateLabel("Text", row.transform, Vector2.zero, new Vector2(0f, 0f), 22, TextAnchor.MiddleLeft);
            rowText.text = label;
            rowText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            RectTransform textRect = rowText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 0f);
            textRect.offsetMax = new Vector2(-10f, 0f);
        }

        private void CreateActionButton(
            string name,
            Transform parent,
            Vector2 anchoredPosition,
            Vector2 size,
            string label,
            Action onClick,
            Color backgroundColor)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchoredPosition = anchoredPosition;
            buttonRect.sizeDelta = size;

            Image image = buttonObject.AddComponent<Image>();
            image.color = backgroundColor;

            Button button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(() => onClick?.Invoke());

            Text text = CreateLabel("Label", buttonObject.transform, Vector2.zero, Vector2.zero, 24, TextAnchor.MiddleCenter);
            text.text = label;
            text.color = Color.white;
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        private Text CreateLabel(
            string name,
            Transform parent,
            Vector2 anchoredPosition,
            Vector2 size,
            int fontSize,
            TextAnchor alignment)
        {
            GameObject textObject = CreateUiObject(name, parent);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.color = Color.white;
            return text;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }
    }
}
