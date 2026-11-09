using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Necrocis;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LevelUpStackChoiceUI : MonoBehaviour
{
    private const int VisibleChoiceCount = 5;
    private const float BioGambleResultRevealSeconds = 0.8f;

    private static readonly Color32 NormalBadgeColor = new Color32(58, 31, 37, 248);
    private static readonly Color32 BioGambleBadgeColor = new Color32(49, 28, 54, 250);
    private static readonly Color32 NormalValueColor = new Color32(236, 207, 199, 255);
    private static readonly Color32 UnknownValueColor = new Color32(246, 207, 99, 255);
    private static readonly Color32 PositiveValueColor = new Color32(143, 227, 136, 255);
    private static readonly Color32 ZeroValueColor = new Color32(193, 183, 183, 255);
    private static readonly Color32 NegativeValueColor = new Color32(255, 119, 119, 255);

    private enum StackStatType
    {
        AttackPower,
        AttackRange,
        AttackSpeed,
        Magic,
        MoveSpeed,
        MaxHealth
    }

    [Header("References")]
    [SerializeField] private GameObject levelUpCanvas;
    [SerializeField] private GameObject stackChoicePanel;
    [SerializeField] private Button[] statButtons = new Button[VisibleChoiceCount];

    [Header("Player Canvas")]
    [SerializeField] private bool attachScriptObjectToPlayer = true;
    [SerializeField] private bool createPlayerLevelUpCanvasIfMissing = true;
    [SerializeField] private string playerLevelUpCanvasName = "LevelUpCanvas";
    [SerializeField] private int canvasSortingOrder = 130;
    [SerializeField] private Vector2 canvasReferenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private bool compensatePlayerScaleForCanvas = true;

    [Header("Settings")]
    [SerializeField] private bool autoFindReferences = true;
    [SerializeField] private bool pauseGameWhileOpen = true;
    [SerializeField] private bool disableBuiltInLevelUpUI = true;
    [SerializeField] private float uiScaleMultiplier = 2.5f;

    [Header("Stat Sprites")]
    [SerializeField] private Sprite attackPowerSprite;
    [SerializeField] private Sprite attackRangeSprite;
    [SerializeField] private Sprite attackSpeedSprite;
    [SerializeField] private Sprite magicSprite;
    [SerializeField] private Sprite moveSpeedSprite;
    [SerializeField] private Sprite maxHealthSprite;

    private readonly List<StackStatType> currentChoices = new List<StackStatType>(VisibleChoiceCount);
    private readonly Dictionary<Button, UnityEngine.Events.UnityAction> boundButtonActions = new Dictionary<Button, UnityEngine.Events.UnityAction>();
    private readonly TMP_Text[] statValueTexts = new TMP_Text[VisibleChoiceCount];
    private readonly TMP_Text[] statRangeTexts = new TMP_Text[VisibleChoiceCount];
    private readonly Image[] statValueBadges = new Image[VisibleChoiceCount];

    private Canvas levelUpCanvasRoot;
    private ModalInputScope inputScope;
    private bool cachedAuthoredUIScale;
    private Vector3 authoredUIScale = Vector3.one;
    private bool selectionInProgress;
    private int selectionSerial;

    private void Awake()
    {
        RefreshUIContext();

        if (disableBuiltInLevelUpUI)
        {
            LevelUpUI[] allLevelUpUIs = FindObjectsByType<LevelUpUI>(FindObjectsSortMode.None);
            for (int i = 0; i < allLevelUpUIs.Length; i++)
            {
                if (allLevelUpUIs[i] != null)
                {
                    allLevelUpUIs[i].enabled = false;
                }
            }
        }

        HideUI(forceRestoreTimeScale: false);
    }

    private void OnEnable()
    {
        LevelUpManager.OnLevelUp += ShowLevelUpChoices;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        LevelUpManager.OnLevelUp -= ShowLevelUpChoices;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        UnbindButtons();
        HideUI();
    }

    private void HandleSceneLoaded(Scene _, LoadSceneMode __)
    {
        RefreshUIContext();
    }

    private void Update()
    {
        if (stackChoicePanel == null || !stackChoicePanel.activeInHierarchy || selectionInProgress || inputScope?.CanInteract != true)
        {
            return;
        }

        InputManager input = InputManager.Instance;
        if (input == null)
        {
            return;
        }

        int selectedIndex = GetKeyboardSelectedIndex(input);
        if (selectedIndex >= 0)
        {
            SelectChoice(selectedIndex);
        }
    }

    private void ShowLevelUpChoices()
    {
        RefreshUIContext();

        if (stackChoicePanel == null)
        {
            Debug.LogWarning("[LevelUpStackChoiceUI] Stack_Choice_UI reference is missing.");
            return;
        }

        currentChoices.Clear();
        currentChoices.AddRange(LevelUpChoiceService.BuildChoices().Select(type=>(StackStatType)System.Enum.Parse(typeof(StackStatType),type.ToString())));
        selectionInProgress = false;
        EnsureStatValueUI();

        LevelProgressionConfig config = LevelUpManager.Config;
        bool hasBioGamble = HasBioGamble(config);

        for (int i = 0; i < statButtons.Length; i++)
        {
            Button button = statButtons[i];
            if (button == null)
            {
                continue;
            }

            bool canUse = i < currentChoices.Count;
            button.gameObject.SetActive(canUse);
            button.interactable = canUse;
            if (!canUse)
            {
                continue;
            }

            Image image = button.targetGraphic as Image;
            if (image == null)
            {
                image = button.GetComponent<Image>();
            }

            if (image == null)
            {
                continue;
            }

            Sprite sprite = GetSprite(currentChoices[i]);
            if (sprite != null)
            {
                image.sprite = sprite;
            }
            image.preserveAspect = true;
            var content = button.transform.Find("LocalizedStatContent") as RectTransform;
            if (content != null) SetCardContentBounds(content, image);

            UpdateStatValueDisplay(i, currentChoices[i], config, hasBioGamble);
        }

        foreach (TMP_Text label in stackChoicePanel.GetComponentsInChildren<TMP_Text>(true))
        {
            if (label.text != "[선택]" && label.text != "Choice"
                && label.text != "숫자 1~5 또는 클릭으로 선택") continue;
            label.text = "숫자 1~5 또는 클릭으로 선택";
            label.enableAutoSizing = true; label.fontSizeMin = 7f; label.fontSizeMax = 10f;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Truncate;
            if (label.transform.parent == stackChoicePanel.transform)
            {
                label.rectTransform.anchorMin = new Vector2(.15f, .01f);
                label.rectTransform.anchorMax = new Vector2(.85f, .12f);
                label.rectTransform.anchoredPosition = label.rectTransform.sizeDelta = Vector2.zero;
            }
        }
        ShowUI();
    }

    private void SelectChoice(int index)
    {
        if (inputScope?.CanInteract != true) return;
        Debug.Log($"[LevelUpStackChoiceUI] SelectChoice called. index={index}, currentChoices={currentChoices.Count}, panelActive={(stackChoicePanel != null && stackChoicePanel.activeInHierarchy)}");

        if (selectionInProgress)
        {
            Debug.Log("[LevelUpStackChoiceUI] SelectChoice ignored: selectionInProgress=true");
            AudioManager.Instance?.PlaySFX("UIInvalid");
            return;
        }

        if (index < 0 || index >= currentChoices.Count)
        {
            Debug.LogWarning($"[LevelUpStackChoiceUI] SelectChoice ignored: invalid index {index}");
            AudioManager.Instance?.PlaySFX("UIInvalid");
            return;
        }

        PlayStatSelectionSfx(currentChoices[index]);
        selectionInProgress = true;
        SetButtonsInteractable(false);

        int? bioGambleResult = ApplyStat(currentChoices[index]);
        LevelUpChoiceService.RecordSelection(ToCharacterStatType(currentChoices[index]));

        if (bioGambleResult.HasValue)
        {
            ShowBioGambleResult(index, bioGambleResult.Value);
            StartCoroutine(CompleteBioGambleSelectionAfterReveal());
            return;
        }

        CompleteSelection();
    }

    private void CompleteSelection()
    {
        if (LevelUpManager.HasPendingLevelUp())
        {
            HideUI();
            StartCoroutine(ProcessPendingLevelUpNextFrame());
            return;
        }

        HideUI();
        GrantPostLevelUpInvincibility();
        selectionInProgress = false;
    }

    private IEnumerator CompleteBioGambleSelectionAfterReveal()
    {
        yield return new WaitForSecondsRealtime(BioGambleResultRevealSeconds);
        CompleteSelection();
    }

    private IEnumerator ProcessPendingLevelUpNextFrame()
    {
        yield return null;
        LevelUpManager.ProcessNextPendingLevelUp();
        if (!LevelUpManager.HasPendingLevelUp()) GrantPostLevelUpInvincibility();
        selectionInProgress = false;
    }

    private int? ApplyStat(StackStatType statType)
    {
        IPlayerGrowthRecipient recipient = GrowthChoiceBindings.ResolveRecipient?.Invoke();
        if (recipient == null || !recipient.IsAvailable)
        {
            Debug.LogWarning("[LevelUpStackChoiceUI] PlayerStats instance not found.");
            return null;
        }
        string source = $"LevelUpStackChoiceUI_{statType}_{selectionSerial++}";
        bool ownsBioGamble = GrowthChoiceBindings.OwnsBioGamble?.Invoke(recipient) ?? false;
        return LevelUpChoiceService.ApplyChoiceTo(recipient,ownsBioGamble,LevelUpManager.Config,ToCharacterStatType(statType),source,LevelUpManager.RecordResolvedModifier);
    }

    private static bool HasBioGamble(LevelProgressionConfig config)
        => config != null && config.bioGambleEnabled && (GrowthChoiceBindings.AnyPlayerOwnsBioGamble?.Invoke() ?? false);

    private static void PlayStatSelectionSfx(StackStatType statType)
    {
        string soundKey = statType switch
        {
            StackStatType.AttackPower => "StatAttackPowerSelect",
            StackStatType.AttackRange => "StatAttackRangeSelect",
            StackStatType.AttackSpeed => "StatAttackRangeSelect",
            StackStatType.Magic => "StatMagicSelect",
            StackStatType.MoveSpeed => "StatMoveSpeedSelect",
            StackStatType.MaxHealth => "StatHealthSelect",
            _ => "UISelect"
        };

        AudioManager.Instance?.PlaySFX(soundKey);
    }

    private void EnsureStatValueUI()
    {
        if (statButtons == null)
        {
            return;
        }

        for (int i = 0; i < statButtons.Length && i < VisibleChoiceCount; i++)
        {
            Button button = statButtons[i];
            if (button == null)
            {
                continue;
            }

            RectTransform content = EnsureStatContent(button);
            Transform existingBadge = content.Find("DynamicStatValueBadge")
                ?? button.transform.Find("DynamicStatValueBadge");
            GameObject badgeObject;
            if (existingBadge != null)
            {
                badgeObject = existingBadge.gameObject;
            }
            else
            {
                badgeObject = new GameObject(
                    "DynamicStatValueBadge",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                badgeObject.layer = button.gameObject.layer;
                badgeObject.transform.SetParent(content, false);
            }

            badgeObject.transform.SetParent(content, false);
            RectTransform badgeRect = badgeObject.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0.18f, 0.38f);
            badgeRect.anchorMax = new Vector2(0.82f, 0.62f);
            badgeRect.anchoredPosition = Vector2.zero;
            badgeRect.sizeDelta = Vector2.zero;
            badgeRect.localScale = Vector3.one;

            Image badgeImage = badgeObject.GetComponent<Image>();
            badgeImage.color = NormalBadgeColor;
            badgeImage.raycastTarget = false;

            Outline badgeOutline = badgeObject.GetComponent<Outline>();
            if (badgeOutline == null)
            {
                badgeOutline = badgeObject.AddComponent<Outline>();
            }
            badgeOutline.effectColor = new Color32(31, 15, 21, 230);
            badgeOutline.effectDistance = new Vector2(1f, -1f);
            badgeOutline.useGraphicAlpha = true;

            TMP_Text valueText = FindOrCreateBadgeText(
                badgeRect,
                "Value",
                Vector2.zero,
                Vector2.one,
                15f,
                9f);
            TMP_Text rangeText = FindOrCreateBadgeText(
                badgeRect,
                "Range",
                Vector2.zero,
                new Vector2(1f, 0.40f),
                8f,
                5f);

            badgeObject.transform.SetAsLastSibling();
            statValueBadges[i] = badgeImage;
            statValueTexts[i] = valueText;
            statRangeTexts[i] = rangeText;
        }
    }

    private static RectTransform EnsureStatContent(Button button)
    {
        Transform existing = button.transform.Find("LocalizedStatContent");
        GameObject owner = existing != null ? existing.gameObject : new GameObject(
            "LocalizedStatContent", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        owner.layer = button.gameObject.layer;
        owner.transform.SetParent(button.transform, false);
        RectTransform rect = owner.GetComponent<RectTransform>();
        // The authored card contains labels and a fixed +1. Cover its text area as one inset panel.
        SetCardContentBounds(rect, button.GetComponent<UnityEngine.UI.Image>());
        rect.anchoredPosition = rect.sizeDelta = Vector2.zero;
        rect.localScale = Vector3.one;
        var background = owner.GetComponent<UnityEngine.UI.Image>();
        background.color = new Color32(60, 32, 39, 255);
        background.raycastTarget = false;
        owner.transform.SetAsLastSibling();
        EnsureCardLabel(rect, "StatName", new Vector2(.02f, .70f), new Vector2(.98f, .98f), 13, 9);
        EnsureCardLabel(rect, "StatDescription", new Vector2(.05f, .06f), new Vector2(.95f, .32f), 9, 7);
        return rect;
    }

    private static void SetCardContentBounds(RectTransform content, UnityEngine.UI.Image image)
    {
        Vector2 min = new Vector2(.115f, .13f);
        Vector2 max = new Vector2(.89f, .66f);
        if (image != null && image.preserveAspect && image.sprite != null)
        {
            Rect bounds = image.rectTransform.rect;
            if (bounds.width > 0f && bounds.height > 0f && image.sprite.rect.height > 0f)
            {
                float spriteAspect = image.sprite.rect.width / image.sprite.rect.height;
                Vector2 fit = Vector2.one;
                if (spriteAspect > bounds.width / bounds.height)
                    fit.y = bounds.width / spriteAspect / bounds.height;
                else fit.x = bounds.height * spriteAspect / bounds.width;
                Vector2 inset = Vector2.Scale(Vector2.one - fit, image.rectTransform.pivot);
                min = inset + Vector2.Scale(min, fit);
                max = inset + Vector2.Scale(max, fit);
            }
        }
        content.anchorMin = min; content.anchorMax = max;
    }

    private static UnityEngine.UI.Text EnsureCardLabel(RectTransform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, int maxSize, int minSize)
    {
        Transform existing = parent.Find(name);
        GameObject owner = existing != null ? existing.gameObject
            : new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Text));
        owner.layer = parent.gameObject.layer;
        owner.transform.SetParent(parent, false);
        var rect = owner.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
        rect.anchoredPosition = rect.sizeDelta = Vector2.zero; rect.localScale = Vector3.one;
        var label = owner.GetComponent<UnityEngine.UI.Text>();
        label.font = GameUiTheme.LoadFont(); label.fontSize = maxSize;
        label.resizeTextForBestFit = true; label.resizeTextMinSize = minSize; label.resizeTextMaxSize = maxSize;
        label.alignment = TextAnchor.MiddleCenter; label.color = new Color32(242, 216, 203, 255);
        label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Truncate;
        label.raycastTarget = false;
        return label;
    }

    private static string GetStatLabel(StackStatType type) => type switch
    {
        StackStatType.AttackPower => "공격력",
        StackStatType.AttackRange => "공격 사거리",
        StackStatType.AttackSpeed => "공격 속도",
        StackStatType.Magic => "마력",
        StackStatType.MoveSpeed => "이동 속도",
        _ => "체력"
    };

    private static string GetStatDescription(StackStatType type) => type switch
    {
        StackStatType.AttackPower => "기본 공격 피해 증가",
        StackStatType.AttackRange => "기본 공격 사거리 증가",
        StackStatType.AttackSpeed => "기본 공격 속도 증가",
        StackStatType.Magic => "스킬 피해 증가",
        StackStatType.MoveSpeed => "이동 속도 증가",
        _ => "최대 체력 증가"
    };

    private static bool UsesPercentUnits(StackStatType type, LevelProgressionConfig config)
    {
        if (config != null && config.TryGetLevelUpStatValue(ToCharacterStatType(type), out var value))
            return value.mode == CharacterStatModifierMode.PercentAdd;
        return type == StackStatType.AttackRange || type == StackStatType.AttackSpeed || type == StackStatType.MoveSpeed;
    }

    private static TMP_Text FindOrCreateBadgeText(
        RectTransform parent,
        string childName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        float maxFontSize,
        float minFontSize)
    {
        Transform existing = parent.Find(childName);
        GameObject textObject;
        if (existing != null)
        {
            textObject = existing.gameObject;
        }
        else
        {
            textObject = new GameObject(childName, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.layer = parent.gameObject.layer;
            textObject.transform.SetParent(parent, false);
        }

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.localScale = Vector3.one;

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.enableAutoSizing = true;
        text.fontSizeMin = minFontSize;
        text.fontSizeMax = maxFontSize;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Truncate;
        text.raycastTarget = false;
        text.outlineColor = new Color32(30, 13, 18, 255);
        text.outlineWidth = 0.18f;
        return text;
    }

    private void UpdateStatValueDisplay(
        int index,
        StackStatType statType,
        LevelProgressionConfig config,
        bool hasBioGamble)
    {
        if (!TryGetStatValueUI(index, out Image badge, out TMP_Text valueText, out TMP_Text rangeText))
        {
            return;
        }

        RectTransform content = badge.transform.parent as RectTransform;
        if (content != null)
        {
            content.Find("StatName").GetComponent<UnityEngine.UI.Text>().text = GetStatLabel(statType);
            content.Find("StatDescription").GetComponent<UnityEngine.UI.Text>().text = GetStatDescription(statType);
        }
        string unit = UsesPercentUnits(statType, config) ? "%" : string.Empty;
        valueText.rectTransform.anchorMin = hasBioGamble ? new Vector2(0f, .32f) : Vector2.zero;
        valueText.rectTransform.anchorMax = Vector2.one;
        rangeText.gameObject.SetActive(hasBioGamble);

        if (hasBioGamble)
        {
            int minDelta = config.bioGambleMinDelta;
            int maxDelta = Mathf.Max(minDelta, config.bioGambleMaxDelta);
            badge.color = BioGambleBadgeColor;
            valueText.text = "?" + unit;
            valueText.color = UnknownValueColor;
            rangeText.text = $"{FormatSignedValue(minDelta)} ~ {FormatSignedValue(maxDelta)}{unit}";
            rangeText.color = NormalValueColor;
            return;
        }

        float displayedValue = 1f;
        CharacterStatType characterStatType = ToCharacterStatType(statType);
        if (config != null && config.TryGetLevelUpStatValue(characterStatType, out LevelUpStatValueConfig valueConfig))
        {
            displayedValue = valueConfig.value;
        }

        badge.color = NormalBadgeColor;
        valueText.text = FormatSignedValue(displayedValue) + unit;
        valueText.color = NormalValueColor;
        rangeText.text = string.Empty;
    }

    private void ShowBioGambleResult(int selectedIndex, int result)
    {
        if (!TryGetStatValueUI(selectedIndex, out Image badge, out TMP_Text valueText, out TMP_Text rangeText))
        {
            return;
        }

        badge.color = BioGambleBadgeColor;
        valueText.text = FormatSignedValue(result)
            + (UsesPercentUnits(currentChoices[selectedIndex], LevelUpManager.Config) ? "%" : string.Empty);
        valueText.rectTransform.anchorMin = Vector2.zero;
        rangeText.gameObject.SetActive(false);
        valueText.color = result > 0
            ? PositiveValueColor
            : result < 0
                ? NegativeValueColor
                : ZeroValueColor;
        rangeText.text = string.Empty;
    }

    private bool TryGetStatValueUI(int index, out Image badge, out TMP_Text valueText, out TMP_Text rangeText)
    {
        badge = null;
        valueText = null;
        rangeText = null;

        if (index < 0 || index >= VisibleChoiceCount)
        {
            return false;
        }

        badge = statValueBadges[index];
        valueText = statValueTexts[index];
        rangeText = statRangeTexts[index];
        return badge != null && valueText != null && rangeText != null;
    }

    private static string FormatSignedValue(float value)
    {
        string magnitude = Mathf.Abs(value).ToString("0.##", CultureInfo.InvariantCulture);
        if (value > 0f)
        {
            return $"+{magnitude}";
        }

        if (value < 0f)
        {
            return $"-{magnitude}";
        }

        return "0";
    }

    private void ResolveReferences()
    {
        if (levelUpCanvas == null && levelUpCanvasRoot != null)
        {
            levelUpCanvas = levelUpCanvasRoot.gameObject;
        }

        if (!autoFindReferences)
        {
            return;
        }

        if (stackChoicePanel == null && levelUpCanvasRoot != null)
        {
            Transform panel = levelUpCanvasRoot.transform.Find("Stack_Choice_UI");
            if (panel != null)
            {
                stackChoicePanel = panel.gameObject;
            }
        }

        if (stackChoicePanel == null && levelUpCanvas != null)
        {
            Transform panel = levelUpCanvas.transform.Find("Stack_Choice_UI");
            if (panel != null)
            {
                stackChoicePanel = panel.gameObject;
            }
        }

        if (stackChoicePanel == null)
        {
            GameObject found = FindSceneObjectByName("Stack_Choice_UI");
            if (found != null)
            {
                stackChoicePanel = found;
            }
        }

        if (statButtons == null || statButtons.Length != 5 || statButtons.Any(button => button == null))
        {
            statButtons = new Button[VisibleChoiceCount];
            if (stackChoicePanel == null)
            {
                return;
            }

            Button[] buttons = stackChoicePanel.GetComponentsInChildren<Button>(true);
            for (int i = 1; i <= VisibleChoiceCount; i++)
            {
                statButtons[i - 1] = buttons.FirstOrDefault(button => button != null && button.name == $"Stack{i}");
            }

            if (statButtons.Any(button => button == null))
            {
                Button[] ordered = buttons
                    .Where(button => button != null)
                    .OrderBy(button => button.transform.GetSiblingIndex())
                    .ToArray();

                int cursor = 0;
                for (int i = 0; i < statButtons.Length && cursor < ordered.Length; i++)
                {
                    if (statButtons[i] != null)
                    {
                        continue;
                    }

                    while (cursor < ordered.Length && statButtons.Contains(ordered[cursor]))
                    {
                        cursor++;
                    }

                    if (cursor < ordered.Length)
                    {
                        statButtons[i] = ordered[cursor];
                        cursor++;
                    }
                }
            }
        }
    }

    private void BindButtons()
    {
        ResolveReferences();

        if (boundButtonActions.Count > 0)
        {
            HashSet<Button> activeButtons = new HashSet<Button>(statButtons.Where(button => button != null));
            List<Button> staleButtons = new List<Button>();

            foreach (KeyValuePair<Button, UnityEngine.Events.UnityAction> pair in boundButtonActions)
            {
                Button boundButton = pair.Key;
                if (boundButton == null || !activeButtons.Contains(boundButton))
                {
                    if (boundButton != null)
                    {
                        boundButton.onClick.RemoveListener(pair.Value);
                    }

                    staleButtons.Add(boundButton);
                }
            }

            for (int i = 0; i < staleButtons.Count; i++)
            {
                boundButtonActions.Remove(staleButtons[i]);
            }
        }

        for (int i = 0; i < statButtons.Length; i++)
        {
            Button button = statButtons[i];
            if (button == null)
            {
                continue;
            }

            if (boundButtonActions.TryGetValue(button, out UnityEngine.Events.UnityAction existingAction))
            {
                button.onClick.RemoveListener(existingAction);
            }

            int captured = i;
            UnityEngine.Events.UnityAction action = () => SelectChoice(captured);
            button.onClick.AddListener(action);
            boundButtonActions[button] = action;
        }
    }

    private void UnbindButtons()
    {
        if (boundButtonActions.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<Button, UnityEngine.Events.UnityAction> pair in boundButtonActions)
        {
            if (pair.Key != null)
            {
                pair.Key.onClick.RemoveListener(pair.Value);
            }
        }

        boundButtonActions.Clear();
    }

    private void RefreshUIContext()
    {
        EnsureEventSystem();
        EnsurePlayerOwnedCanvasHierarchy();
        ResolveReferences();
        BindButtons();
    }

    private int GetKeyboardSelectedIndex(InputManager input)
    {
        if (input.Digit1Action.WasPressedThisFrame() && currentChoices.Count >= 1) return 0;
        if (input.Digit2Action.WasPressedThisFrame() && currentChoices.Count >= 2) return 1;
        if (input.Digit3Action.WasPressedThisFrame() && currentChoices.Count >= 3) return 2;
        if (input.Digit4Action.WasPressedThisFrame() && currentChoices.Count >= 4) return 3;
        if (input.Digit5Action.WasPressedThisFrame() && currentChoices.Count >= 5) return 4;
        return -1;
    }

    private void EnsurePlayerOwnedCanvasHierarchy()
    {
        PlayerController player = ResolvePlayerController();
        if (player == null)
        {
            return;
        }

        Transform playerTransform = player.transform;
        if (attachScriptObjectToPlayer && transform != playerTransform && transform.parent != playerTransform)
        {
            transform.SetParent(playerTransform, false);
        }

        levelUpCanvasRoot = FindOrCreatePlayerLevelUpCanvas(playerTransform);
        if (levelUpCanvasRoot != null)
        {
            levelUpCanvas = levelUpCanvasRoot.gameObject;
        }

        if (stackChoicePanel == null)
        {
            Transform panelInCanvas = levelUpCanvasRoot != null ? levelUpCanvasRoot.transform.Find("Stack_Choice_UI") : null;
            if (panelInCanvas != null)
            {
                stackChoicePanel = panelInCanvas.gameObject;
            }
        }

        if (stackChoicePanel == null)
        {
            GameObject found = FindSceneObjectByName("Stack_Choice_UI");
            if (found != null)
            {
                stackChoicePanel = found;
            }
        }

        if (stackChoicePanel != null && levelUpCanvasRoot != null && stackChoicePanel.transform.parent != levelUpCanvasRoot.transform)
        {
            stackChoicePanel.transform.SetParent(levelUpCanvasRoot.transform, false);
        }

        ApplyCanvasParentScaleCompensation(playerTransform);
        NormalizeStackChoiceRect();
    }

    private Canvas FindOrCreatePlayerLevelUpCanvas(Transform playerTransform)
    {
        if (playerTransform == null)
        {
            return null;
        }

        Transform existing = playerTransform.Find(playerLevelUpCanvasName);
        if (existing != null)
        {
            Canvas existingCanvas = existing.GetComponent<Canvas>();
            if (existingCanvas != null)
            {
                ConfigureCanvas(existingCanvas);
                return existingCanvas;
            }
        }

        if (!createPlayerLevelUpCanvasIfMissing)
        {
            return null;
        }

        GameObject canvasObject = new GameObject(
            playerLevelUpCanvasName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        canvasObject.transform.SetParent(playerTransform, false);
        Canvas createdCanvas = canvasObject.GetComponent<Canvas>();
        ConfigureCanvas(createdCanvas);

        RectTransform rect = canvasObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        return createdCanvas;
    }

    private void ConfigureCanvas(Canvas canvas)
    {
        if (canvas == null)
        {
            return;
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = canvasSortingOrder;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = canvasReferenceResolution;

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void NormalizeStackChoiceRect()
    {
        if (stackChoicePanel == null)
        {
            return;
        }

        RectTransform rect = stackChoicePanel.GetComponent<RectTransform>();
        if (rect == null)
        {
            return;
        }

        if (!cachedAuthoredUIScale)
        {
            authoredUIScale = rect.localScale;
            cachedAuthoredUIScale = true;
        }

        rect.localScale = authoredUIScale * Mathf.Max(0.1f, uiScaleMultiplier);
        rect.localRotation = Quaternion.identity;
        rect.localPosition = new Vector3(rect.localPosition.x, rect.localPosition.y, 0f);
    }

    private void ApplyCanvasParentScaleCompensation(Transform playerTransform)
    {
        if (!compensatePlayerScaleForCanvas || levelUpCanvasRoot == null || playerTransform == null)
        {
            return;
        }

        Vector3 parentScale = playerTransform.lossyScale;
        float x = Mathf.Max(0.0001f, Mathf.Abs(parentScale.x));
        float y = Mathf.Max(0.0001f, Mathf.Abs(parentScale.y));

        RectTransform canvasRect = levelUpCanvasRoot.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.localScale = new Vector3(1f / x, 1f / y, 1f);
        }
    }

    private void ShowUI()
    {
        EnsureEventSystem();

        if (stackChoicePanel == null)
        {
            return;
        }

        if (!stackChoicePanel.activeSelf)
        {
            stackChoicePanel.SetActive(true);
        }

        stackChoicePanel.transform.SetAsLastSibling();

        inputScope ??= new ModalInputScope(this,
            levelUpCanvasRoot != null ? levelUpCanvasRoot.gameObject : stackChoicePanel, pauseGameWhileOpen);
    }

    private void HideUI(bool forceRestoreTimeScale = true)
    {
        if (stackChoicePanel != null && stackChoicePanel.activeSelf) stackChoicePanel.SetActive(false);
        inputScope?.Dispose(); inputScope = null;
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (statButtons == null)
        {
            return;
        }

        for (int i = 0; i < statButtons.Length; i++)
        {
            if (statButtons[i] != null)
            {
                statButtons[i].interactable = interactable;
            }
        }
    }

    private void GrantPostLevelUpInvincibility()
    {
        Health health = FindFirstObjectByType<Health>();
        if (health != null)
        {
            health.GrantTemporaryInvincibility(1f);
        }
    }

    private Sprite GetSprite(StackStatType statType)
    {
        return statType switch
        {
            StackStatType.AttackPower => attackPowerSprite,
            StackStatType.AttackRange => attackRangeSprite,
            StackStatType.AttackSpeed => attackSpeedSprite,
            StackStatType.Magic => magicSprite,
            StackStatType.MoveSpeed => moveSpeedSprite,
            StackStatType.MaxHealth => maxHealthSprite,
            _ => null
        };
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            eventSystem = esObj.AddComponent<EventSystem>();
        }

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        if (!eventSystem.enabled)
        {
            eventSystem.enabled = true;
        }

        if (!inputModule.enabled)
        {
            inputModule.enabled = true;
        }
    }

    private static PlayerController ResolvePlayerController()
    {
        PlayerController player = PlayerController.Instance;
        if (player == null)
        {
            player = Object.FindFirstObjectByType<PlayerController>();
        }

        return player;
    }

    private static GameObject FindSceneObjectByName(string objectName)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < allTransforms.Length; i++)
        {
            Transform tr = allTransforms[i];
            if (tr == null || !tr.gameObject.scene.IsValid())
            {
                continue;
            }

            if (tr.name == objectName)
            {
                return tr.gameObject;
            }
        }

        return null;
    }

    private static CharacterStatType ToCharacterStatType(StackStatType statType)
    {
        return statType switch
        {
            StackStatType.AttackPower => CharacterStatType.AttackPower,
            StackStatType.AttackRange => CharacterStatType.AttackRange,
            StackStatType.AttackSpeed => CharacterStatType.AttackSpeed,
            StackStatType.Magic => CharacterStatType.Magic,
            StackStatType.MoveSpeed => CharacterStatType.MoveSpeed,
            StackStatType.MaxHealth => CharacterStatType.MaxHealth,
            _ => CharacterStatType.MaxHealth
        };
    }
}
