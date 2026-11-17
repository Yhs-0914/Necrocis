using System;
using System.Collections.Generic;
using System.Text;
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
        private ModalInputScope inputScope;
        private GameObject canvasObject;
        private GameObject panelObject;
        private Transform catalogContent;
        private Transform acquiredContent;
        private Text titleText;
        private Text summaryText;
        private Text detailText;
        private Text statusText;
        private InputField searchField;
        private string searchQuery = string.Empty;
        private Font panelFont;

        private string selectedCatalogItemId;
        private string selectedAcquiredItemId;

        public static bool IsAvailable
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return true;
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (!IsAvailable) { enabled = false; return; }
            itemManager = GetComponent<PlayerItemManager>();
        }

        private void OnEnable()
        {
            if (!IsAvailable) { enabled = false; return; }
            if (itemManager == null)
            {
                return;
            }

            itemManager.ItemAcquired += HandleItemChanged;
            itemManager.ItemRemoved += HandleItemChanged;
        }

        private void OnDisable()
        {
            HidePanel();
            if (itemManager == null)
            {
                return;
            }

            itemManager.ItemAcquired -= HandleItemChanged;
            itemManager.ItemRemoved -= HandleItemChanged;
        }

        private void Update()
        {
            if (!IsAvailable || !enablePanel || !IsTogglePressedThisFrame())
            {
                return;
            }

            if (canvasObject != null && canvasObject.activeSelf)
            {
                if (inputScope?.CanInteract == true) HidePanel();
                return;
            }
            ShowPanel();
        }

        private void ShowPanel()
        {
            if (!IsAvailable || !isActiveAndEnabled || !enablePanel || canvasObject != null && canvasObject.activeSelf
                || !InputManager.Instance.Contexts.CanOpenAuxiliary) return;
            EnsureUi();
            canvasObject.SetActive(true);
            inputScope = new ModalInputScope(this, canvasObject, false, HidePanel);
            RefreshLists();
            UpdateStatus($"{toggleKey}: 테스트 패널 열기/닫기");
            if (inputScope.CanInteract) searchField.ActivateInputField();
        }

        private void HidePanel()
        {
            if (searchField != null) searchField.DeactivateInputField();
            inputScope?.Dispose(); inputScope = null;
            if (canvasObject != null) canvasObject.SetActive(false);
        }
        private void OnDestroy() => inputScope?.Dispose();

        private void HandleItemChanged(PlayerItemManager _, PlayerItemManager.AcquiredPlayerItem __)
        {
            if (canvasObject != null && canvasObject.activeSelf)
            {
                RefreshLists();
            }
        }

        private void EnsureUi()
        {
            if (!IsAvailable) return;
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

            CreateSearchField();

            detailText = CreateLabel("Detail", panelObject.transform, new Vector2(0f, 310f), new Vector2(1740f, 50f), 24, TextAnchor.MiddleCenter);
            detailText.color = new Color(0.93f, 0.93f, 0.93f, 1f);

            CreateLabel("CatalogLabel", panelObject.transform, new Vector2(-450f, 260f), new Vector2(840f, 40f), 26, TextAnchor.MiddleLeft).text = "전체 아이템 목록";
            CreateLabel("AcquiredLabel", panelObject.transform, new Vector2(450f, 260f), new Vector2(840f, 40f), 26, TextAnchor.MiddleLeft).text = "현재 보유 아이템";

            catalogContent = CreateScrollList(panelObject.transform, new Vector2(-450f, -35f), new Vector2(840f, 550f));
            acquiredContent = CreateScrollList(panelObject.transform, new Vector2(450f, -35f), new Vector2(840f, 550f));

            CreateActionButton("RefreshButton", panelObject.transform, new Vector2(-690f, -365f), new Vector2(220f, 68f), "새로고침", RefreshLists, new Color(0.2f, 0.3f, 0.42f, 1f));
            CreateActionButton("AddButton", panelObject.transform, new Vector2(-430f, -365f), new Vector2(260f, 68f), "선택 아이템 추가", AddSelectedCatalogItem, new Color(0.18f, 0.44f, 0.22f, 1f));
            CreateActionButton("SpawnButton", panelObject.transform, new Vector2(-120f, -365f), new Vector2(300f, 68f), "플레이어 위치 생성", SpawnSelectedCatalogItemAtPlayer, new Color(0.44f, 0.32f, 0.12f, 1f));
            CreateActionButton("RemoveButton", panelObject.transform, new Vector2(210f, -365f), new Vector2(260f, 68f), "선택 아이템 삭제", RemoveSelectedAcquiredItem, new Color(0.5f, 0.18f, 0.18f, 1f));
            CreateActionButton("ClearButton", panelObject.transform, new Vector2(500f, -365f), new Vector2(220f, 68f), "전체 삭제", ClearAllItems, new Color(0.41f, 0.14f, 0.14f, 1f));
            CreateActionButton("AddGoldButton", panelObject.transform, new Vector2(-530f, -445f), new Vector2(340f, 58f), "골드 +100", AddTestGold, new Color(0.5f, 0.35f, 0.1f, 1f));
            CreateActionButton("HealButton", panelObject.transform, new Vector2(0f, -445f), new Vector2(340f, 58f), "플레이어 체력 전체 회복", RestorePlayerHealth, new Color(0.12f, 0.48f, 0.4f, 1f));

            CreateActionButton("CompleteBiomeBossesButton", panelObject.transform, new Vector2(520f, -445f), new Vector2(620f, 58f), "4개 맵 보스 처치 완료 (저장 반영)", CompleteBiomeBosses, new Color(0.38f, 0.24f, 0.53f, 1f));

            statusText = CreateLabel("Status", panelObject.transform, new Vector2(0f, -500f), new Vector2(1740f, 46f), 24, TextAnchor.MiddleCenter);
            statusText.color = new Color(0.97f, 0.83f, 0.35f, 1f);
            canvasObject.SetActive(false);
        }

        private void CreateSearchField()
        {
            CreateLabel("SearchLabel", panelObject.transform, new Vector2(-800f, 365f), new Vector2(140f, 50f), 26, TextAnchor.MiddleLeft).text = "아이템 검색";
            GameObject root = CreateUiObject("ItemSearch", panelObject.transform);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(-30f, 365f);
            rect.sizeDelta = new Vector2(1340f, 52f);
            Image background = root.AddComponent<Image>();
            background.color = new Color(0.13f, 0.19f, 0.25f, 1f);
            searchField = root.AddComponent<InputField>();
            searchField.targetGraphic = background;
            searchField.contentType = InputField.ContentType.Standard;
            searchField.lineType = InputField.LineType.SingleLine;
            searchField.characterLimit = 80;
            Text valueText = CreateLabel("SearchText", root.transform, Vector2.zero, Vector2.zero, 24, TextAnchor.MiddleLeft);
            Text placeholder = CreateLabel("Placeholder", root.transform, Vector2.zero, Vector2.zero, 24, TextAnchor.MiddleLeft);
            placeholder.text = "한글 이름 또는 아이템 ID 입력 (예: 점액, 혈액드론)";
            placeholder.color = new Color(0.65f, 0.71f, 0.78f, 1f);
            foreach (Text text in new[] { valueText, placeholder })
            {
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.supportRichText = false;
                text.raycastTarget = false;
                text.rectTransform.anchorMin = Vector2.zero;
                text.rectTransform.anchorMax = Vector2.one;
                text.rectTransform.offsetMin = new Vector2(14f, 6f);
                text.rectTransform.offsetMax = new Vector2(-14f, -6f);
            }
            searchField.textComponent = valueText;
            searchField.placeholder = placeholder;
            searchField.onValueChanged.AddListener(HandleSearchChanged);
            CreateActionButton("ClearSearchButton", panelObject.transform, new Vector2(770f, 365f), new Vector2(180f, 52f),
                "검색 초기화", () => { searchField.text = string.Empty; searchField.ActivateInputField(); }, new Color(.2f, .3f, .42f, 1f));
        }

        private void HandleSearchChanged(string value)
        {
            string query = NormalizeSearch(value);
            if (string.Equals(searchQuery, query, StringComparison.Ordinal)) return;
            searchQuery = query;
            RefreshLists();
            foreach (Transform content in new[] { catalogContent, acquiredContent })
            {
                if (content == null) continue;
                ScrollRect scroll = content.GetComponentInParent<ScrollRect>();
                scroll.StopMovement();
                scroll.verticalNormalizedPosition = 1f;
                // Reset the content too: the layout height updates at the end of the frame.
                ((RectTransform)content).anchoredPosition = Vector2.zero;
            }
        }

        private static string NormalizeSearch(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var result = new StringBuilder(value.Length);
            foreach (char character in value.Normalize(NormalizationForm.FormC))
                if (!char.IsWhiteSpace(character)) result.Append(character);
            return result.ToString();
        }

        private bool MatchesSearch(string displayName, string itemId) => searchQuery.Length == 0
            || NormalizeSearch(displayName).IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0
            || NormalizeSearch(itemId).IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;

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
                GameObject row = catalogContent.GetChild(i).gameObject;
                row.SetActive(false);
                Destroy(row);
            }

            for (int i = acquiredContent.childCount - 1; i >= 0; i--)
            {
                GameObject row = acquiredContent.GetChild(i).gameObject;
                row.SetActive(false);
                Destroy(row);
            }

            IReadOnlyList<PlayerItemManager.PlayerItemEntry> entries = itemManager.ItemEntries;
            int catalogMatches = 0, acquiredMatches = 0;
            bool catalogSelectionVisible = false, acquiredSelectionVisible = false;
            for (int i = 0; i < entries.Count; i++)
            {
                PlayerItemManager.PlayerItemEntry entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    continue;
                }

                if (!MatchesSearch(entry.DisplayName, entry.ItemId)) continue;
                catalogMatches++;

                string label = $"{entry.DisplayName} ({entry.ItemId})";
                if (!string.IsNullOrWhiteSpace(entry.Description))
                {
                    label = $"{label} - {entry.Description}";
                }

                bool selected = string.Equals(selectedCatalogItemId, entry.ItemId, StringComparison.OrdinalIgnoreCase);
                catalogSelectionVisible |= selected;
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

                if (!MatchesSearch(item.DisplayName, item.ItemId)) continue;
                acquiredMatches++;

                string label = $"{i + 1}. {item.DisplayName} ({item.ItemId})";
                bool selected = string.Equals(selectedAcquiredItemId, item.ItemId, StringComparison.OrdinalIgnoreCase);
                acquiredSelectionVisible |= selected;
                CreateItemRow(acquiredContent, label, selected, () =>
                {
                    selectedAcquiredItemId = item.ItemId;
                    detailText.text = $"삭제 대상: {item.DisplayName} ({item.ItemId})";
                    RefreshLists();
                });
            }

            bool selectionHidden = !catalogSelectionVisible && selectedCatalogItemId != null
                || !acquiredSelectionVisible && selectedAcquiredItemId != null;
            if (!catalogSelectionVisible) selectedCatalogItemId = null;
            if (!acquiredSelectionVisible) selectedAcquiredItemId = null;
            if (selectionHidden) detailText.text = "목록에서 아이템을 선택하세요.";
            if (catalogMatches == 0) CreateEmptySearchRow(catalogContent, "검색 결과가 없습니다.");
            if (acquiredMatches == 0) CreateEmptySearchRow(acquiredContent, searchQuery.Length > 0 ? "검색에 맞는 보유 아이템이 없습니다." : "보유 아이템이 없습니다.");
            summaryText.text = searchQuery.Length == 0
                ? $"보유: {itemManager.ItemCount}개 (제한 없음) | 카탈로그: {entries.Count}"
                : $"검색 결과 | 전체 목록: {catalogMatches}/{entries.Count}개 | 보유 목록: {acquiredMatches}/{itemManager.ItemCount}개";
        }

        private void CreateEmptySearchRow(Transform parent, string message)
        {
            Text text = CreateLabel("EmptySearch", parent, Vector2.zero, new Vector2(0f, 62f), 24, TextAnchor.MiddleCenter);
            text.text = message;
            text.color = new Color(.65f, .71f, .78f, 1f);
        }

        private void AddSelectedCatalogItem()
        {
            if (!IsAvailable || inputScope?.CanInteract != true) return;
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

        private void SpawnSelectedCatalogItemAtPlayer()
        {
            if (!IsAvailable || inputScope?.CanInteract != true) return;
            if (itemManager == null || string.IsNullOrWhiteSpace(selectedCatalogItemId))
            {
                UpdateStatus("생성할 아이템을 먼저 선택하세요.");
                return;
            }

            PlayerController player = PlayerController.Instance != null
                ? PlayerController.Instance
                : GetComponent<PlayerController>();
            if (player == null)
            {
                UpdateStatus("플레이어를 찾을 수 없어 아이템을 생성하지 못했습니다.");
                return;
            }

            WorldItemSpawner spawner = ResolveWorldItemSpawner();
            if (spawner == null)
            {
                UpdateStatus("아이템 스포너를 만들 수 없어 생성하지 못했습니다.");
                return;
            }

            if (spawner.TrySpawnItemAt(selectedCatalogItemId, player.transform.position, true, true))
            {
                UpdateStatus($"플레이어 위치에 생성: {selectedCatalogItemId}");
                return;
            }

            UpdateStatus($"생성 실패: {selectedCatalogItemId}");
        }

        private WorldItemSpawner ResolveWorldItemSpawner()
        {
            if (!IsAvailable) return null;
            BiomeManager activeBiome = BiomeManager.Active;
            WorldItemSpawner spawner = activeBiome != null
                ? activeBiome.GetComponent<WorldItemSpawner>()
                : null;

            if (spawner == null)
            {
                spawner = FindFirstObjectByType<WorldItemSpawner>();
            }

            if (spawner != null)
            {
                return spawner;
            }

            GameObject spawnerObject = new GameObject("ItemTestWorldItemSpawner");
            if (activeBiome != null)
            {
                spawnerObject.transform.SetParent(activeBiome.transform, true);
            }

            spawner = spawnerObject.AddComponent<WorldItemSpawner>();
            spawner.SetAutoSpawnOnStart(false);
            return spawner;
        }

        private void RemoveSelectedAcquiredItem()
        {
            if (!IsAvailable || inputScope?.CanInteract != true) return;
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
            if (!IsAvailable || inputScope?.CanInteract != true) return;
            if (itemManager == null)
            {
                return;
            }

            itemManager.ClearAllItems();
            selectedAcquiredItemId = null;
            UpdateStatus("모든 보유 아이템을 삭제했습니다.");
            RefreshLists();
        }

        private void CompleteBiomeBosses()
        {
            if (!IsAvailable || inputScope?.CanInteract != true) return;
            GameManager game = GameManager.Instance;
            if (game == null)
            {
                UpdateStatus("게임 상태를 찾을 수 없습니다. 게임 시작 후 다시 시도하세요.");
                return;
            }

            BiomeType[] biomes = { BiomeType.Intestine, BiomeType.Liver, BiomeType.Stomach, BiomeType.Lung };
            foreach (BiomeType biome in biomes)
            {
                if (!game.HasRelic(biome) || !BossProgress.IsDefeated(biome))
                    game.CollectRelic(biome);
            }
            UpdateStatus("보스 처치 및 부산물 4/4 완료. F8을 닫고 허브 중앙 포탈로 들어가세요. (현재 저장에 반영)");
        }

        private void AddTestGold()
        {
            if (!IsAvailable || inputScope?.CanInteract != true) return;
            long previous = SaveService.CurrencyBalance;
            SaveService.CreditCurrency(100);
            UpdateStatus($"골드 +{SaveService.CurrencyBalance - previous:N0} (보유 골드: {SaveService.CurrencyBalance:N0})");
        }

        private void RestorePlayerHealth()
        {
            if (!IsAvailable || inputScope?.CanInteract != true) return;
            PlayerStats playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                playerStats = PlayerStats.Instance;
            }

            if (playerStats == null || playerStats.RuntimeStats == null)
            {
                UpdateStatus("플레이어 스탯을 찾을 수 없어 체력을 회복하지 못했습니다.");
                return;
            }

            float previousHealth = playerStats.CurrentHealth;
            float maxHealth = playerStats.MaxHealth;
            playerStats.RuntimeStats.ResetHealthToMax();
            UpdateStatus($"플레이어 체력 전체 회복: {previousHealth:0.##} → {maxHealth:0.##}");
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
            button.onClick.AddListener(() => { if (inputScope?.CanInteract == true) onClick?.Invoke(); });

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
            button.onClick.AddListener(() => { if (inputScope?.CanInteract == true) onClick?.Invoke(); });

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
            if (panelFont == null) panelFont = Resources.Load<Font>("PFStardust") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.font = panelFont;
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
