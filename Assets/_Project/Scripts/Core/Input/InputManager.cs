using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Necrocis
{
    /// <summary>Owns the project gameplay actions and their persisted binding overrides.</summary>
    [DefaultExecutionOrder(-11000)]
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance
        {
            get
            {
                if (instance == null || instance.retiring)
                {
                    instance = FindObjectsByType<InputManager>(FindObjectsSortMode.None).FirstOrDefault(manager => !manager.retiring);
                    if (instance == null) instance = new GameObject("InputManager").AddComponent<InputManager>();
                }
                instance.EnsureInitialized();
                return instance;
            }
        }

        private static InputManager instance;
        private InputActionAsset ownedActions;
        public InputContextStack Contexts { get; private set; }
        public static InputManager ExistingInstance => instance != null && !instance.retiring ? instance : null;
        public GameplayInputAdapter Gameplay { get; private set; }
        public GameplayFocusOwner Focus { get; private set; }
        private bool retiring, released;
        public bool IsRetiring => retiring;
        private IBindingOverrideStore rebinds = new PlayerPrefsBindingOverrideStore();
        public string LastRebindLoadWarning { get; private set; }

        public InputAction MoveAction { get; private set; }
        public InputAction MeleeAttackAction { get; private set; }
        public InputAction RangedAttackAction { get; private set; }
        public InputAction Skill1Action { get; private set; }
        public InputAction Skill2Action { get; private set; }
        public InputAction Digit1Action { get; private set; }
        public InputAction Digit2Action { get; private set; }
        public InputAction Digit3Action { get; private set; }
        public InputAction Digit4Action { get; private set; }
        public InputAction Digit5Action { get; private set; }
        public InputAction DashAction { get; private set; }
        public InputAction StatWindowAction { get; private set; }
        public InputAction InventoryAction { get; private set; }
        public InputAction DebugLevelUpAction { get; private set; }
        public InputAction TraversalHoldAction { get; private set; }
        public InputAction MenuBackAction { get; private set; }

        private void Awake()
        {
            if (instance != null && instance != this && !instance.retiring) { retiring = true; Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureInitialized();
            Debug.Log("[InputManager] Initialized");
        }

        private void OnEnable()
        {
            if (retiring) return;
            EnsureInitialized(); SetActionsEnabled(true); Focus.SetNativeFocused(Application.isFocused);
        }
        private void OnDisable() => SetActionsEnabled(false);
        private void OnApplicationFocus(bool focused) { if (!retiring) Focus?.SetNativeFocused(focused); }
        private void OnDestroy() => ReleaseOwnedInput(null);

        public void PrepareForSessionEnd() => ReleaseOwnedInput(1f);
        public static void ResetSessionClock()
        {
            if (ExistingInstance != null) ExistingInstance.Contexts.ResetSession(1f);
            else Time.timeScale = 1f;
        }

        private void ReleaseOwnedInput(float? baseline)
        {
            if (released) return;
            retiring = true; released = true;
            try
            {
                if (baseline.HasValue) Contexts?.ResetSession(baseline.Value);
            }
            finally
            {
                try { Focus?.Dispose(); }
                finally
                {
                    try { Contexts?.Dispose(); }
                    finally
                    {
                        if (instance == this) instance = null;
                        if (ownedActions != null) { ownedActions.Disable(); Destroy(ownedActions); }
                    }
                }
            }
        }

        private void EnsureInitialized()
        {
            if (ownedActions != null) return;
            Contexts = new InputContextStack(new PauseCoordinator(() => Time.timeScale, value => {
                if (ReferenceEquals(instance, this)) Time.timeScale = value;
            }));
            CreateActions();
            Gameplay = new GameplayInputAdapter(this);
            Focus = new GameplayFocusOwner(Contexts, Gameplay.ClearHeldIntent);
            LoadSavedRebinds();
            SetActionsEnabled(isActiveAndEnabled);
            Focus.SetNativeFocused(Application.isFocused);
        }

        private void CreateActions()
        {
            ownedActions = GameplayInputDefinition.CreateAsset();
            MoveAction = ownedActions.FindAction("Move", true);
            MeleeAttackAction = ownedActions.FindAction("MeleeAttack", true);
            RangedAttackAction = ownedActions.FindAction("RangedAttack", true);
            Skill1Action = ownedActions.FindAction("Skill1", true);
            Skill2Action = ownedActions.FindAction("Skill2", true);
            Digit1Action = ownedActions.FindAction("Digit1", true);
            Digit2Action = ownedActions.FindAction("Digit2", true);
            Digit3Action = ownedActions.FindAction("Digit3", true);
            Digit4Action = ownedActions.FindAction("Digit4", true);
            Digit5Action = ownedActions.FindAction("Digit5", true);
            DashAction = ownedActions.FindAction("Dash", true);
            StatWindowAction = ownedActions.FindAction("StatWindow", true);
            InventoryAction = ownedActions.FindAction("Inventory", true);
            DebugLevelUpAction = ownedActions.FindAction("DebugLevelUp", true);
            TraversalHoldAction = ownedActions.FindAction("TraversalHold", true);
            MenuBackAction = ownedActions.FindAction("MenuBack", true);
        }

        public void SaveRebinds()
        {
            EnsureInitialized();
            PreserveUnreadableSource();
            rebinds.Write(BuildRebindJson());
            rebinds.Save();
            LastRebindLoadWarning = null;
        }

        public void LoadRebinds()
        {
            if (ownedActions == null) { EnsureInitialized(); return; }
            LoadSavedRebinds();
        }

        private void LoadSavedRebinds() => ApplyRebindJson(rebinds.Read());

        public void ResetToDefaults()
        {
            EnsureInitialized();
            PreserveUnreadableSource();
            RemoveAllOverrides();
            rebinds.Delete();
            rebinds.Save();
            LastRebindLoadWarning = null;
        }

        private void PreserveUnreadableSource()
        {
            if (LastRebindLoadWarning != null && rebinds.HasValue) rebinds.Preserve(rebinds.Read());
        }

        private string BuildRebindJson() => BindingOverrideCodec.Encode(ownedActions);
        private void ApplyRebindJson(string json)
        {
            BindingOverrideCodec.TryApply(ownedActions, json, out string warning);
            LastRebindLoadWarning = warning;
            if (warning != null) Debug.LogWarning("[InputManager] " + warning);
        }
        private void RemoveAllOverrides() => ownedActions?.RemoveAllBindingOverrides();
        private void EnableAll() => SetActionsEnabled(true);
        // Kept for existing Editor runners that temporarily suppress physical input.
        private void SetActionsEnabled(bool enabled)
        {
            if (ownedActions == null) return;
            if (enabled && isActiveAndEnabled && !retiring) ownedActions.Enable(); else ownedActions.Disable();
        }
        private InputAction[] GetAllActions() => ownedActions == null ? Array.Empty<InputAction>() : ownedActions.ToArray();
    }
}
