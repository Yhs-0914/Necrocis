using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Necrocis;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class ClassChoiceUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject classChoiceUI;
    [SerializeField] private Button warriorButton;
    [SerializeField] private Button archerButton;
    [SerializeField] private Button mageButton;

    [Header("Player Canvas")]
    [SerializeField] private bool attachScriptObjectToPlayer = true;
    [SerializeField] private bool createPlayerClassCanvasIfMissing = true;
    [SerializeField] private string playerClassCanvasName = "ClassCanvas";
    [SerializeField] private int canvasSortingOrder = 120;
    [SerializeField] private Vector2 canvasReferenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private bool compensatePlayerScaleForCanvas = true;

    [Header("Settings")]
    [SerializeField] private bool pauseGameWhileOpen = true;
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.85f);
    [SerializeField] private float uiScaleMultiplier = 1f;

    private bool isBound;
    private bool pausedByThisUI;
    private float previousTimeScale = 1f;
    private Canvas classCanvasRoot;
    private GameObject overlayBlocker;
    private bool cachedAuthoredUIScale;
    private Vector3 authoredUIScale = Vector3.one;

    private void Awake()
    {
        RefreshUIContext();
        HideUI(forceRestoreTimeScale: false);
    }

    private void OnEnable()
    {
        LevelUpManager.OnJobSelect += HandleJobSelectionRequested;
        LevelUpManager.OnLevelUp += HandleLevelChanged;
        LevelUpManager.OnJobChanged += HandleJobChanged;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        LevelUpManager.OnJobSelect -= HandleJobSelectionRequested;
        LevelUpManager.OnLevelUp -= HandleLevelChanged;
        LevelUpManager.OnJobChanged -= HandleJobChanged;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        HideUI();
    }

    private void Start()
    {
        RefreshUIContext();
        EvaluateVisibility();
    }

    public void ChooseWarrior() => SelectJob(JobType.Warrior);
    public void ChooseArcher() => SelectJob(JobType.Archer);
    public void ChooseMage() => SelectJob(JobType.Mage);

    private void HandleSceneLoaded(Scene _, LoadSceneMode __)
    {
        RefreshUIContext();
    }

    private void HandleLevelChanged()
    {
        EvaluateVisibility();
    }

    private void HandleJobSelectionRequested()
    {
        EvaluateVisibility(forceShow: true);
    }

    private void HandleJobChanged(JobType _)
    {
        EvaluateVisibility();
    }

    private void EvaluateVisibility(bool forceShow = false)
    {
        if (LevelUpManager.GetCurrentJob() != JobType.None)
        {
            HideUI();
            return;
        }

        if (forceShow || LevelUpManager.GetCurrentLevel() >= LevelUpManager.GetJobSelectionLevel())
        {
            ShowUI();
        }
        else
        {
            HideUI();
        }
    }

    private void SelectJob(JobType job)
    {
        if (job == JobType.None || LevelUpManager.GetCurrentJob() != JobType.None)
        {
            AudioManager.Instance?.PlaySFX("UIInvalid");
            return;
        }

        AudioManager.Instance?.PlaySFX("UISelect");
        LevelUpManager.SetJob(job);
        SyncLegacyPlayerClass(job);
        HideUI();

        if (LevelUpManager.HasPendingLevelUp())
        {
            LevelUpManager.ProcessNextPendingLevelUp();
        }
    }

    private void ShowUI()
    {
        RefreshUIContext();

        if (classChoiceUI == null)
        {
            Debug.LogWarning("[ClassChoice] classChoiceUI is null. Assign Class_Choice_UI in inspector.");
            return;
        }

        EnsureOverlayBlocker();
        if (overlayBlocker != null && !overlayBlocker.activeSelf)
        {
            overlayBlocker.SetActive(true);
        }

        if (!classChoiceUI.activeSelf)
        {
            classChoiceUI.SetActive(true);
        }

        classChoiceUI.transform.SetAsLastSibling();

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
        if (overlayBlocker != null && overlayBlocker.activeSelf)
        {
            overlayBlocker.SetActive(false);
        }

        if (classChoiceUI != null && classChoiceUI.activeSelf)
        {
            classChoiceUI.SetActive(false);
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

    private void ResolveReferences()
    {
        if (classChoiceUI == null && TryGetComponent<RectTransform>(out _))
        {
            classChoiceUI = gameObject;
        }

        if (classChoiceUI == null && classCanvasRoot != null)
        {
            Transform existingPanel = classCanvasRoot.transform.Find("Class_Choice_UI");
            if (existingPanel != null)
                classChoiceUI = existingPanel.gameObject;
        }

        if (warriorButton == null)
        {
            warriorButton = FindButtonByKeyword("warrior");
        }

        if (archerButton == null)
        {
            archerButton = FindButtonByKeyword("archer");
        }

        if (mageButton == null)
        {
            mageButton = FindButtonByKeyword("mage");
        }
    }

    private void BindButtons()
    {
        ResolveReferences();
        TryBindButton(warriorButton, ChooseWarrior);
        TryBindButton(archerButton, ChooseArcher);
        TryBindButton(mageButton, ChooseMage);
        isBound = warriorButton != null || archerButton != null || mageButton != null;

        if (!isBound)
        {
            Debug.LogWarning("[ClassChoice] No class buttons are bound. Check Warrior/Archer/Mage button references.");
        }
    }

    private static void TryBindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void RefreshUIContext()
    {
        EnsureEventSystem();
        EnsurePlayerOwnedCanvasHierarchy();
        ResolveReferences();
        BindButtons();
    }

    private Button FindButtonByKeyword(string keyword)
    {
        if (classChoiceUI != null)
        {
            Button[] buttons = classChoiceUI.GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons)
            {
                if (button != null && button.name.ToLowerInvariant().Contains(keyword))
                {
                    return button;
                }
            }
        }

        if (classCanvasRoot != null)
        {
            Button[] buttons = classCanvasRoot.GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons)
            {
                if (button != null && button.name.ToLowerInvariant().Contains(keyword))
                {
                    return button;
                }
            }
        }

        return null;
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

        classCanvasRoot = FindOrCreatePlayerClassCanvas(playerTransform);

        if (classChoiceUI == null)
        {
            return;
        }

        if (classCanvasRoot != null && classChoiceUI.transform.parent != classCanvasRoot.transform)
        {
            classChoiceUI.transform.SetParent(classCanvasRoot.transform, false);
        }

        ApplyCanvasParentScaleCompensation(playerTransform);
        NormalizeClassChoiceRectForOverlay();
    }

    private Canvas FindOrCreatePlayerClassCanvas(Transform playerTransform)
    {
        if (playerTransform == null)
        {
            return null;
        }

        Transform existing = playerTransform.Find(playerClassCanvasName);
        if (existing != null)
        {
            Canvas existingCanvas = existing.GetComponent<Canvas>();
            if (existingCanvas != null)
            {
                ConfigureCanvas(existingCanvas);
                return existingCanvas;
            }
        }

        if (!createPlayerClassCanvasIfMissing)
        {
            return null;
        }

        GameObject canvasObject = new GameObject(
            playerClassCanvasName,
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

    private void NormalizeClassChoiceRectForOverlay()
    {
        if (classChoiceUI == null)
        {
            return;
        }

        RectTransform uiRect = classChoiceUI.GetComponent<RectTransform>();
        if (uiRect == null)
        {
            return;
        }

        if (!cachedAuthoredUIScale)
        {
            authoredUIScale = uiRect.localScale;
            cachedAuthoredUIScale = true;
        }

        // Keep authored panel scale and allow a runtime multiplier.
        float safeMultiplier = Mathf.Max(0.1f, uiScaleMultiplier);
        uiRect.localScale = authoredUIScale * safeMultiplier;
        uiRect.localRotation = Quaternion.identity;
        uiRect.localPosition = new Vector3(uiRect.localPosition.x, uiRect.localPosition.y, 0f);
    }

    private void ApplyCanvasParentScaleCompensation(Transform playerTransform)
    {
        if (!compensatePlayerScaleForCanvas || classCanvasRoot == null || playerTransform == null)
        {
            return;
        }

        Vector3 parentScale = playerTransform.lossyScale;
        float x = Mathf.Max(0.0001f, Mathf.Abs(parentScale.x));
        float y = Mathf.Max(0.0001f, Mathf.Abs(parentScale.y));

        RectTransform canvasRect = classCanvasRoot.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.localScale = new Vector3(1f / x, 1f / y, 1f);
        }
    }

    private void EnsureOverlayBlocker()
    {
        if (classCanvasRoot == null)
        {
            return;
        }

        if (overlayBlocker == null)
        {
            Transform existing = classCanvasRoot.transform.Find("ClassChoiceOverlay");
            if (existing != null)
            {
                overlayBlocker = existing.gameObject;
            }
        }

        if (overlayBlocker == null)
        {
            overlayBlocker = new GameObject("ClassChoiceOverlay", typeof(RectTransform), typeof(Image));
            overlayBlocker.transform.SetParent(classCanvasRoot.transform, false);
        }

        RectTransform overlayRect = overlayBlocker.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.localScale = Vector3.one;
        overlayRect.localRotation = Quaternion.identity;

        Image overlayImage = overlayBlocker.GetComponent<Image>();
        overlayImage.color = overlayColor;
        overlayImage.raycastTarget = true;

        if (classChoiceUI != null)
        {
            overlayBlocker.transform.SetSiblingIndex(Mathf.Max(0, classChoiceUI.transform.GetSiblingIndex() - 1));
            classChoiceUI.transform.SetAsLastSibling();
        }

        if (!overlayBlocker.activeSelf)
        {
            overlayBlocker.SetActive(true);
        }
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

    private static void SyncLegacyPlayerClass(JobType job)
    {
        PlayerController playerController = PlayerController.Instance;
        if (playerController == null)
        {
            playerController = Object.FindFirstObjectByType<PlayerController>();
        }

        if (playerController == null)
        {
            return;
        }

        PlayerClass playerClass = playerController.GetComponent<PlayerClass>();
        if (playerClass == null)
        {
            return;
        }

        playerClass.UnlockAdvance();
        playerClass.AdvanceTo(job);
    }
}
