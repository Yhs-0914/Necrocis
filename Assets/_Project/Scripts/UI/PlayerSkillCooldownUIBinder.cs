using Necrocis;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class PlayerSkillCooldownUIBinder : MonoBehaviour
{
    private static PlayerSkillCooldownUIBinder persistentInstance;
    private const int Skill1UnlockLevel = 10;
    private const int Skill2UnlockLevel = 20;

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

    private void HandleSceneLoaded(Scene _, LoadSceneMode __)
    {
        TryResolveController();
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
        int level = LevelUpManager.GetCurrentLevel();
        bool hasSelectedJob = LevelUpManager.GetCurrentJob() != JobType.None;
        if (!hasSelectedJob)
        {
            return false;
        }

        return slot == PlayerClassSkillController.SkillSlot.Skill1
            ? level >= Skill1UnlockLevel
            : level >= Skill2UnlockLevel;
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
