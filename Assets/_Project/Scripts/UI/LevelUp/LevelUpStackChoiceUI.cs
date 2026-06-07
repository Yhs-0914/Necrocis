using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Necrocis;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LevelUpStackChoiceUI : MonoBehaviour
{
    private const int VisibleChoiceCount = 5;

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

    private Canvas levelUpCanvasRoot;
    private bool pausedByThisUI;
    private float previousTimeScale = 1f;
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
        if (stackChoicePanel == null || !stackChoicePanel.activeInHierarchy || selectionInProgress)
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
        List<StackStatType> allStats = new List<StackStatType>
        {
            StackStatType.AttackPower,
            StackStatType.AttackRange,
            StackStatType.AttackSpeed,
            StackStatType.Magic,
            StackStatType.MoveSpeed,
            StackStatType.MaxHealth
        };

        Shuffle(allStats);
        currentChoices.AddRange(allStats.Take(VisibleChoiceCount));
        selectionInProgress = false;

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
        }

        ShowUI();
    }

    private void SelectChoice(int index)
    {
        Debug.Log($"[LevelUpStackChoiceUI] SelectChoice called. index={index}, currentChoices={currentChoices.Count}, panelActive={(stackChoicePanel != null && stackChoicePanel.activeInHierarchy)}");

        if (selectionInProgress)
        {
            Debug.Log("[LevelUpStackChoiceUI] SelectChoice ignored: selectionInProgress=true");
            return;
        }

        if (index < 0 || index >= currentChoices.Count)
        {
            Debug.LogWarning($"[LevelUpStackChoiceUI] SelectChoice ignored: invalid index {index}");
            return;
        }

        selectionInProgress = true;
        SetButtonsInteractable(false);

        ApplyStat(currentChoices[index]);
        LevelUpManager.RecordSelection(MapToLegacyChoice(currentChoices[index]));

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

    private IEnumerator ProcessPendingLevelUpNextFrame()
    {
        yield return null;
        LevelUpManager.ProcessNextPendingLevelUp();
        selectionInProgress = false;
    }

    private void ApplyStat(StackStatType statType)
    {
        PlayerStats playerStats = PlayerStats.Instance ?? FindFirstObjectByType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogWarning("[LevelUpStackChoiceUI] PlayerStats instance not found.");
            return;
        }

        string source = $"LevelUpStackChoiceUI_{statType}_{selectionSerial++}";
        PlayerItemManager itemManager = PlayerItemManager.Instance ?? playerStats.GetComponent<PlayerItemManager>();
        LevelProgressionConfig config = LevelUpManager.Config;
        bool hasBioGamble = config != null
            && config.bioGambleEnabled
            && itemManager != null
            && itemManager.ContainsItem(PlayerItemCombatEffects.BioGambleId);
        if (hasBioGamble)
        {
            int minDelta = config.bioGambleMinDelta;
            int maxDelta = Mathf.Max(minDelta, config.bioGambleMaxDelta);
            int randomDelta = Random.Range(minDelta, maxDelta + 1);
            ApplyBioGambleStat(playerStats, statType, randomDelta, source);
            return;
        }

        CharacterStatType characterStatType = ToCharacterStatType(statType);
        if (config == null || !config.TryGetLevelUpStatValue(characterStatType, out LevelUpStatValueConfig valueConfig))
        {
            Debug.LogWarning($"[LevelUpStackChoiceUI] Missing level-up stat config for {characterStatType}.");
            return;
        }

        float modifierValue = config.GetRuntimeModifierValue(valueConfig);
        playerStats.ApplyModifier(new CharacterStatModifier(characterStatType, modifierValue, valueConfig.mode, source));
        if (valueConfig.healWhenPositive
            && characterStatType == CharacterStatType.MaxHealth
            && valueConfig.mode == CharacterStatModifierMode.Flat
            && valueConfig.value > 0f)
        {
            playerStats.Heal(valueConfig.value);
        }
    }

    private static void ApplyBioGambleStat(PlayerStats playerStats, StackStatType statType, int randomDelta, string source)
    {
        switch (statType)
        {
            case StackStatType.AttackPower:
                playerStats.ApplyModifier(new CharacterStatModifier(CharacterStatType.AttackPower, randomDelta, CharacterStatModifierMode.Flat, source));
                break;
            case StackStatType.AttackRange:
                playerStats.ApplyModifier(new CharacterStatModifier(CharacterStatType.AttackRange, randomDelta / 100f, CharacterStatModifierMode.PercentAdd, source));
                break;
            case StackStatType.AttackSpeed:
                playerStats.ApplyModifier(new CharacterStatModifier(CharacterStatType.AttackSpeed, randomDelta / 100f, CharacterStatModifierMode.PercentAdd, source));
                break;
            case StackStatType.Magic:
                playerStats.ApplyModifier(new CharacterStatModifier(CharacterStatType.Magic, randomDelta, CharacterStatModifierMode.Flat, source));
                break;
            case StackStatType.MoveSpeed:
                playerStats.ApplyModifier(new CharacterStatModifier(CharacterStatType.MoveSpeed, randomDelta / 100f, CharacterStatModifierMode.PercentAdd, source));
                break;
            case StackStatType.MaxHealth:
                playerStats.ApplyModifier(new CharacterStatModifier(CharacterStatType.MaxHealth, randomDelta, CharacterStatModifierMode.Flat, source));
                if (randomDelta > 0)
                {
                    playerStats.Heal(randomDelta);
                }
                break;
        }
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
        if (attachScriptObjectToPlayer && transform.parent != playerTransform)
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

        if (!pauseGameWhileOpen || pausedByThisUI)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        pausedByThisUI = true;
    }

    private void HideUI(bool forceRestoreTimeScale = true)
    {
        if (stackChoicePanel != null && stackChoicePanel.activeSelf)
        {
            stackChoicePanel.SetActive(false);
        }

        if (!pausedByThisUI)
        {
            return;
        }

        if (forceRestoreTimeScale)
        {
            Time.timeScale = previousTimeScale;
        }

        pausedByThisUI = false;
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

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static LevelUpStatChoice MapToLegacyChoice(StackStatType statType)
    {
        return statType switch
        {
            StackStatType.AttackPower => LevelUpStatChoice.AttackPowerUp,
            StackStatType.AttackRange => LevelUpStatChoice.AttackSpeedRangeUp,
            StackStatType.AttackSpeed => LevelUpStatChoice.AttackSpeedRangeUp,
            StackStatType.Magic => LevelUpStatChoice.MagicUp,
            StackStatType.MoveSpeed => LevelUpStatChoice.SpeedUp,
            StackStatType.MaxHealth => LevelUpStatChoice.HealthUp,
            _ => LevelUpStatChoice.HealthUp
        };
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
