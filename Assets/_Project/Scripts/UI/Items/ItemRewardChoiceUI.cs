using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public sealed class ItemRewardChoiceUI : MonoBehaviour
    {
        private PlayerItemManager items;
        private PlayerController player;
        private GameObject canvasObject, cardRoot;
        private Text title, hint;
        private Button backButton;
        private readonly List<Button> buttons = new List<Button>();
        private ModalInputScope scope;
        private ItemRewardState shownState;
        private string replacingWith;
        private bool selecting;
        private float nextCheck;

        public static void EnsureOn(GameObject owner)
        {
            if (owner != null && owner.GetComponent<ItemRewardChoiceUI>() == null)
                owner.AddComponent<ItemRewardChoiceUI>();
        }

        private void Awake()
        {
            items = GetComponent<PlayerItemManager>();
            player = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (items == null || player == null || player.IsDead || SaveService.IsRestorePending) { Hide(); return; }
            if (scope != null && !ReferenceEquals(shownState, LevelUpManager.ItemRewards)) Hide();
            if (scope == null)
            {
                if (Time.unscaledTime < nextCheck) return;
                nextCheck = Time.unscaledTime + .2f;
                TryShowReward();
                return;
            }
            if (!scope.CanInteract || selecting) return;
            InputManager input = InputManager.Instance;
            if (input == null) return;
            if (input.Digit1Action.WasPressedThisFrame()) Select(0);
            else if (input.Digit2Action.WasPressedThisFrame()) Select(1);
            else if (input.Digit3Action.WasPressedThisFrame()) Select(2);
        }

        private void TryShowReward()
        {
            ItemRewardState state = LevelUpManager.ItemRewards;
            LevelProgressionConfig config = LevelUpManager.Config;
            if (state.pendingReward == 0)
            {
                int reward = TempoBalanceRules.PendingItemReward(LevelUpManager.GetCurrentLevel(),
                    state.claimedMask, config.starterItemChoiceEnabled, config.midgameItemChoiceLevel);
                if (reward == 0) return;
                var candidates = new List<PlayerItemManager.PlayerItemEntry>();
                var seen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
                foreach (var entry in items.ItemEntries)
                    if (entry != null && seen.Add(entry.ItemId)
                        && (items.AllowsDuplicateItems || !items.ContainsItem(entry.ItemId))) candidates.Add(entry);
                for (int i = candidates.Count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    var swap = candidates[i]; candidates[i] = candidates[j]; candidates[j] = swap;
                }
                var ids = new List<string>();
                for (int i = 0; i < Mathf.Min(3, candidates.Count); i++) ids.Add(candidates[i].ItemId);
                if (!state.BeginOffer(reward, ids)) return;
                SaveRewardState();
            }
            BuildInterface();
            shownState = state;
            replacingWith = null;
            ShowCards();
            canvasObject.SetActive(true);
            scope = new ModalInputScope(this, canvasObject, true, allowAuxiliary: false);
            if (buttons.Count > 0) EventSystem.current?.SetSelectedGameObject(buttons[0].gameObject);
        }

        private void Select(int index)
        {
            if (scope?.CanInteract != true || selecting || index < 0 || index >= buttons.Count) return;
            if (replacingWith == null)
            {
                if (index >= shownState.pendingItemIds.Count) return;
                string id = shownState.pendingItemIds[index];
                if (items.IsFull) { replacingWith = id; ShowCards(); return; }
                Acquire(id);
                return;
            }
            if (index >= items.AcquiredItems.Count) return;
            string removed = items.AcquiredItems[index].ItemId;
            var previous = items.CaptureSavedItems();
            if (!items.RemoveItem(removed)) return;
            if (!Acquire(replacingWith)) items.RestoreSavedItems(previous);
        }

        private bool Acquire(string id)
        {
            if (!shownState.pendingItemIds.Contains(id)) return false;
            selecting = true;
            if (!items.TryAcquireItem(id, out PlayerItemAcquireFailureReason failure))
            {
                selecting = false;
                hint.text = failure == PlayerItemAcquireFailureReason.DuplicateNotAllowed
                    ? "이미 보유한 아이템입니다. 다른 아이템을 선택하세요." : "아이템을 획득할 수 없습니다. 다시 선택하세요.";
                return false;
            }
            shownState.Claim(id);
            AudioManager.Instance?.PlaySFX("ItemPickup");
            SaveRewardState();
            Hide();
            return true;
        }

        private static void SaveRewardState()
        {
            if (SaveService.HasActiveSession && !SaveService.TrySaveActiveRun(out string error))
                Debug.LogError($"[ItemRewardChoice] 보상 저장 실패: {error}");
        }

        private void ShowCards()
        {
            foreach (Button button in buttons)
            {
                button.gameObject.SetActive(false);
                Destroy(button.gameObject);
            }
            buttons.Clear();
            bool replacing = replacingWith != null;
            title.text = replacing ? "교체할 보유 아이템 선택" : shownState.pendingReward == 1
                ? "첫 변이 — 시작 아이템 선택" : "추가 변이 — 아이템 선택";
            hint.text = replacing ? "슬롯이 가득 찼습니다. 새 아이템으로 교체할 대상을 선택하세요."
                : "셋 중 하나를 선택하세요 · 클릭 또는 숫자 1 / 2 / 3";
            backButton.gameObject.SetActive(replacing);
            int count = replacing ? items.AcquiredItems.Count : shownState.pendingItemIds.Count;
            float width = Mathf.Min(350f, 1120f / Mathf.Max(1, count));
            for (int i = 0; i < count; i++)
            {
                string id = replacing ? items.AcquiredItems[i].ItemId : shownState.pendingItemIds[i];
                if (!items.TryGetItemEntry(id, out var entry)) continue;
                var card = NewImage("ItemCard", cardRoot.transform, new Color(.13f, .055f, .085f, 1f));
                RectTransform rect = card.rectTransform;
                rect.sizeDelta = new Vector2(width - 20f, 390f);
                rect.anchoredPosition = new Vector2((i - (count - 1) * .5f) * width, 0f);
                Button button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card;
                int selected = i;
                button.onClick.AddListener(() => Select(selected));
                buttons.Add(button);
                var label = NewText("Name", card.transform, $"{i + 1}. {entry.DisplayName}", 29);
                Place(label.rectTransform, new Vector2(0f, 148f), new Vector2(width - 44f, 66f));
                var description = NewText("Description", card.transform, entry.Description, 23);
                Place(description.rectTransform, new Vector2(0f, -85f), new Vector2(width - 60f, 160f));
                description.alignment = TextAnchor.UpperCenter;
                description.resizeTextForBestFit = true; description.resizeTextMinSize = 16;
                if (entry.Icon != null)
                {
                    var icon = NewImage("Icon", card.transform, Color.white);
                    icon.sprite = entry.Icon; icon.preserveAspect = true;
                    Place(icon.rectTransform, new Vector2(0f, 65f), new Vector2(90f, 90f));
                }
            }
            if (buttons.Count > 0) EventSystem.current?.SetSelectedGameObject(buttons[0].gameObject);
        }

        private void BuildInterface()
        {
            if (canvasObject != null) return;
            canvasObject = new GameObject("ItemRewardCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 160;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f); scaler.matchWidthOrHeight = .5f;
            var backdrop = NewImage("Backdrop", canvasObject.transform, new Color(.015f, .005f, .012f, .95f));
            backdrop.rectTransform.anchorMin = Vector2.zero; backdrop.rectTransform.anchorMax = Vector2.one;
            backdrop.rectTransform.offsetMin = backdrop.rectTransform.offsetMax = Vector2.zero;
            title = NewText("Title", backdrop.transform, string.Empty, 44);
            Place(title.rectTransform, new Vector2(0f, 320f), new Vector2(1400f, 85f));
            hint = NewText("Hint", backdrop.transform, string.Empty, 25);
            Place(hint.rectTransform, new Vector2(0f, -300f), new Vector2(1500f, 65f));
            cardRoot = new GameObject("Choices", typeof(RectTransform));
            cardRoot.transform.SetParent(backdrop.transform, false);
            var back = NewImage("Back", backdrop.transform, new Color(.24f, .10f, .15f));
            Place(back.rectTransform, new Vector2(0f, -395f), new Vector2(270f, 65f));
            backButton = back.gameObject.AddComponent<Button>(); backButton.targetGraphic = back;
            backButton.onClick.AddListener(() => { if (scope?.CanInteract == true) { replacingWith = null; ShowCards(); } });
            var backLabel = NewText("Label", back.transform, "선택지로 돌아가기", 23);
            Place(backLabel.rectTransform, Vector2.zero, new Vector2(260f, 60f));
            canvasObject.SetActive(false);
        }

        private static Image NewImage(string name, Transform parent, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false);
            var image = obj.GetComponent<Image>(); image.color = color; return image;
        }
        private static Text NewText(string name, Transform parent, string text, int size)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false);
            var label = obj.GetComponent<Text>(); label.text = text; label.font = GameUiTheme.LoadFont();
            label.fontSize = size; label.color = new Color(1f, .88f, .74f); label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false; return label;
        }
        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position; rect.sizeDelta = size;
        }
        private void Hide()
        {
            scope?.Dispose(); scope = null; shownState = null; replacingWith = null; selecting = false;
            if (canvasObject != null) canvasObject.SetActive(false);
        }
        private void OnDisable() => Hide();
        private void OnDestroy() { Hide(); if (canvasObject != null) Destroy(canvasObject); }
    }
}
