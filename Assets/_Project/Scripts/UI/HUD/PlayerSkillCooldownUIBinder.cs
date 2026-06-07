using Necrocis;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerSkillCooldownUIBinder : MonoBehaviour
{
    private static PlayerSkillCooldownUIBinder persistentInstance;

    [Header("References")]
    [SerializeField] private PlayerClassSkillController skillController;
    [SerializeField] private SkillCooldownUI skillEUI;
    [SerializeField] private SkillCooldownUI skillRUI;

    [Header("Scene Transition")]
    [SerializeField] private bool keepUIAcrossScenes = true;
    [SerializeField] private Transform persistenceRoot;
    [SerializeField] private bool suppressDuplicateSkillUI = true;

    [Header("Labels")]
    [SerializeField] private bool autoApplyKeyLabels = true;
    [SerializeField] private string skillELabel = "E";
    [SerializeField] private string skillRLabel = "R";

    private static readonly Vector2 ResponsiveReferenceResolution = new Vector2(1920f, 1080f);
    private const float ResponsiveMatchWidthOrHeight = 0.5f;
    private const float ExpBarWidth = 1160f;
    private const float ExpBarBottomMargin = 20f;
    private const float SkillGapFromExpBar = 48f;
    private const float SlotSize = 176f;
    private const float SlotSpacing = 0f;

    private int lastAppliedScreenWidth = -1;
    private int lastAppliedScreenHeight = -1;

    private void Awake()
    {
        if (!RegisterPersistentInstanceIfNeeded())
        {
            return;
        }

        TryResolveController();
        ApplyKeyLabels();
        EnsurePersistence();
    }

    private void OnEnable()
    {
        TryResolveController();
        ApplyResponsiveLayout();
        Subscribe();
        SubscribeLevelEvents();
        SceneManager.sceneLoaded += HandleSceneLoaded;
        SyncUnlockState();
        SyncCurrentCooldownState();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        Unsubscribe();
        UnsubscribeLevelEvents();

        if (persistentInstance == this)
        {
            persistentInstance = null;
        }
    }

    private void LateUpdate()
    {
        if (lastAppliedScreenWidth == Screen.width && lastAppliedScreenHeight == Screen.height)
        {
            return;
        }

        ApplyResponsiveLayout();
    }

    private void HandleSceneLoaded(Scene _, LoadSceneMode __)
    {
        TryResolveController();
        ApplyResponsiveLayout();
        Subscribe();
        EnsurePersistence();
        SyncUnlockState();
        SyncCurrentCooldownState();
    }

    private void TryResolveController()
    {
        if (skillController != null)
        {
            return;
        }

        skillController = FindFirstObjectByType<PlayerClassSkillController>();
    }

    private void Subscribe()
    {
        if (skillController == null)
        {
            Debug.LogWarning($"[{nameof(PlayerSkillCooldownUIBinder)}] PlayerClassSkillController not found.", this);
            return;
        }

        skillController.CooldownStarted -= HandleCooldownStarted;
        skillController.CooldownStarted += HandleCooldownStarted;
        skillController.CooldownReset -= HandleCooldownReset;
        skillController.CooldownReset += HandleCooldownReset;
    }

    private void Unsubscribe()
    {
        if (skillController == null)
        {
            return;
        }

        skillController.CooldownStarted -= HandleCooldownStarted;
        skillController.CooldownReset -= HandleCooldownReset;
    }

    private void HandleCooldownStarted(PlayerClassSkillController.SkillSlot slot, float duration)
    {
        SkillCooldownUI targetUI = GetTargetUI(slot);
        if (targetUI == null || !IsSlotUnlocked(slot))
        {
            return;
        }

        targetUI.StartCooldown(duration);
    }

    private void HandleCooldownReset(PlayerClassSkillController.SkillSlot slot)
    {
        SkillCooldownUI targetUI = GetTargetUI(slot);
        targetUI?.ForceReady();
    }

    private void SyncCurrentCooldownState()
    {
        if (skillController == null)
        {
            return;
        }

        SyncSlot(PlayerClassSkillController.SkillSlot.Skill1);
        SyncSlot(PlayerClassSkillController.SkillSlot.Skill2);
    }

    private void SyncSlot(PlayerClassSkillController.SkillSlot slot)
    {
        SkillCooldownUI targetUI = GetTargetUI(slot);
        if (targetUI == null)
        {
            return;
        }

        if (!IsSlotUnlocked(slot))
        {
            targetUI.ForceReady();
            return;
        }

        float remain = skillController.GetRemainingCooldown(slot);
        if (remain > 0f)
        {
            float configured = skillController.GetConfiguredCooldown(slot);
            targetUI.StartCooldown(remain, configured);
            return;
        }

        targetUI.ForceReady();
    }

    private void SubscribeLevelEvents()
    {
        LevelUpManager.OnLevelUp -= HandleLevelProgressOrJobChanged;
        LevelUpManager.OnLevelUp += HandleLevelProgressOrJobChanged;
        LevelUpManager.OnJobSelect -= HandleLevelProgressOrJobChanged;
        LevelUpManager.OnJobSelect += HandleLevelProgressOrJobChanged;
        LevelUpManager.OnJobChanged -= HandleJobChanged;
        LevelUpManager.OnJobChanged += HandleJobChanged;
    }

    private void UnsubscribeLevelEvents()
    {
        LevelUpManager.OnLevelUp -= HandleLevelProgressOrJobChanged;
        LevelUpManager.OnJobSelect -= HandleLevelProgressOrJobChanged;
        LevelUpManager.OnJobChanged -= HandleJobChanged;
    }

    private void HandleLevelProgressOrJobChanged()
    {
        SyncUnlockState();
        SyncCurrentCooldownState();
    }

    private void HandleJobChanged(JobType _)
    {
        SyncUnlockState();
        SyncCurrentCooldownState();
    }

    private void SyncUnlockState()
    {
        skillEUI?.SetUnlocked(IsSlotUnlocked(PlayerClassSkillController.SkillSlot.Skill1));
        skillRUI?.SetUnlocked(IsSlotUnlocked(PlayerClassSkillController.SkillSlot.Skill2));
    }

    private bool IsSlotUnlocked(PlayerClassSkillController.SkillSlot slot)
    {
        int skillSlotIndex = slot == PlayerClassSkillController.SkillSlot.Skill1 ? 1 : 2;
        return LevelUpManager.IsSkillUnlocked(skillSlotIndex);
    }

    private SkillCooldownUI GetTargetUI(PlayerClassSkillController.SkillSlot slot)
    {
        return slot == PlayerClassSkillController.SkillSlot.Skill1 ? skillEUI : skillRUI;
    }

    private void ApplyKeyLabels()
    {
        if (!autoApplyKeyLabels)
        {
            return;
        }

        skillEUI?.SetKeyLabel(skillELabel);
        skillRUI?.SetKeyLabel(skillRLabel);
    }

    private void ApplyResponsiveLayout()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ResponsiveReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = Mathf.Clamp01(ResponsiveMatchWidthOrHeight);
        }

        RectTransform root = transform as RectTransform;
        if (root == null)
        {
            return;
        }

        float safeSlotSize = SlotSize;
        float safeSpacing = SlotSpacing;
        float containerWidth = (safeSlotSize * 2f) + safeSpacing + 24f;
        float containerHeight = safeSlotSize + 24f;

        root.localScale = Vector3.one;
        root.anchorMin = new Vector2(0.5f, 0f);
        root.anchorMax = new Vector2(0.5f, 0f);
        root.pivot = new Vector2(0f, 0f);

        float expBarRightEdgeOffset = (ExpBarWidth * 0.5f) + SkillGapFromExpBar;
        float rootX = expBarRightEdgeOffset;
        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect != null)
        {
            float halfWidth = canvasRect.rect.width * 0.5f;
            float maxRootX = halfWidth - 12f - containerWidth;
            rootX = Mathf.Min(rootX, maxRootX);
        }

        root.anchoredPosition = new Vector2(rootX, ExpBarBottomMargin);
        root.sizeDelta = new Vector2(containerWidth, containerHeight);

        float slotY = 12f + (safeSlotSize * 0.5f);
        float leftSlotX = 12f + (safeSlotSize * 0.5f);
        float rightSlotX = leftSlotX + safeSlotSize + safeSpacing;

        ApplySkillSlotRect(skillEUI, new Vector2(leftSlotX, slotY), safeSlotSize);
        ApplySkillSlotRect(skillRUI, new Vector2(rightSlotX, slotY), safeSlotSize);

        lastAppliedScreenWidth = Screen.width;
        lastAppliedScreenHeight = Screen.height;
    }

    private static void ApplySkillSlotRect(SkillCooldownUI slot, Vector2 anchoredPosition, float size)
    {
        if (slot == null)
        {
            return;
        }

        RectTransform slotRect = slot.transform as RectTransform;
        if (slotRect == null)
        {
            return;
        }

        slotRect.localScale = Vector3.one;
        slotRect.anchorMin = new Vector2(0f, 0f);
        slotRect.anchorMax = new Vector2(0f, 0f);
        slotRect.pivot = new Vector2(0.5f, 0.5f);
        slotRect.anchoredPosition = anchoredPosition;
        slotRect.sizeDelta = new Vector2(size, size);

        ResizeChildSquare(slotRect, "Icon", size - 8f);
        ResizeChildSquare(slotRect, "CooldownFill", size - 8f);
    }

    private static void ResizeChildSquare(RectTransform parent, string childName, float size)
    {
        RectTransform child = parent.Find(childName) as RectTransform;
        if (child == null)
        {
            return;
        }

        float safeSize = Mathf.Max(1f, size);
        child.localScale = Vector3.one;
        child.anchorMin = new Vector2(0.5f, 0.5f);
        child.anchorMax = new Vector2(0.5f, 0.5f);
        child.pivot = new Vector2(0.5f, 0.5f);
        child.anchoredPosition = Vector2.zero;
        child.sizeDelta = new Vector2(safeSize, safeSize);
    }

    private void EnsurePersistence()
    {
        if (!keepUIAcrossScenes)
        {
            return;
        }

        Transform root = persistenceRoot != null ? persistenceRoot : transform.root;
        if (root == null)
        {
            return;
        }

        DontDestroyOnLoad(root.gameObject);
    }

    private bool RegisterPersistentInstanceIfNeeded()
    {
        if (!keepUIAcrossScenes)
        {
            return true;
        }

        if (persistentInstance == null || persistentInstance == this)
        {
            persistentInstance = this;
            return true;
        }

        if (suppressDuplicateSkillUI)
        {
            skillEUI?.gameObject.SetActive(false);
            skillRUI?.gameObject.SetActive(false);
        }

        enabled = false;
        return false;
    }
}
