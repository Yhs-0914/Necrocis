using UnityEngine;
using UnityEngine.InputSystem;

namespace Necrocis
{
    /// <summary>
    /// Centralized input action manager with runtime rebinding support.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("InputManager");
                    obj.AddComponent<InputManager>();
                }
                return instance;
            }
        }

        private static InputManager instance;

        public InputAction MoveAction { get; private set; }

        public InputAction MeleeAttackAction { get; private set; }
        public InputAction RangedAttackAction { get; private set; }
        public InputAction Skill1Action { get; private set; }
        public InputAction Skill2Action { get; private set; }

        public InputAction Digit1Action { get; private set; }
        public InputAction Digit2Action { get; private set; }
        public InputAction Digit3Action { get; private set; }
        public InputAction Digit4Action { get; private set; }

        public InputAction StatWindowAction { get; private set; }
        public InputAction DebugLevelUpAction { get; private set; }

        private const string RebindKey = "InputRebinds";

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            CreateActions();
            LoadRebinds();
            EnableAll();
            Debug.Log("[InputManager] Initialized");
        }

        private void CreateActions()
        {
            MoveAction = new InputAction("Move", InputActionType.Value);
            MoveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            MeleeAttackAction = new InputAction("MeleeAttack", InputActionType.Button, "<Keyboard>/q");
            RangedAttackAction = new InputAction("RangedAttack", InputActionType.Button, "<Keyboard>/w");
            Skill1Action = new InputAction("Skill1", InputActionType.Button, "<Keyboard>/e");
            Skill2Action = new InputAction("Skill2", InputActionType.Button, "<Keyboard>/r");

            Digit1Action = new InputAction("Digit1", InputActionType.Button, "<Keyboard>/1");
            Digit2Action = new InputAction("Digit2", InputActionType.Button, "<Keyboard>/2");
            Digit3Action = new InputAction("Digit3", InputActionType.Button, "<Keyboard>/3");
            Digit4Action = new InputAction("Digit4", InputActionType.Button, "<Keyboard>/4");

            StatWindowAction = new InputAction("StatWindow", InputActionType.Button, "<Keyboard>/o");
            DebugLevelUpAction = new InputAction("DebugLevelUp", InputActionType.Button, "<Keyboard>/p");
        }

        private void EnableAll()
        {
            MoveAction.Enable();
            MeleeAttackAction.Enable();
            RangedAttackAction.Enable();
            Skill1Action.Enable();
            Skill2Action.Enable();
            Digit1Action.Enable();
            Digit2Action.Enable();
            Digit3Action.Enable();
            Digit4Action.Enable();
            StatWindowAction.Enable();
            DebugLevelUpAction.Enable();
        }

        private void OnDisable()
        {
            MoveAction?.Disable();
            MeleeAttackAction?.Disable();
            RangedAttackAction?.Disable();
            Skill1Action?.Disable();
            Skill2Action?.Disable();
            Digit1Action?.Disable();
            Digit2Action?.Disable();
            Digit3Action?.Disable();
            Digit4Action?.Disable();
            StatWindowAction?.Disable();
            DebugLevelUpAction?.Disable();
        }

        public void SaveRebinds()
        {
            string json = BuildRebindJson();
            PlayerPrefs.SetString(RebindKey, json);
            PlayerPrefs.Save();
        }

        public void LoadRebinds()
        {
            string json = PlayerPrefs.GetString(RebindKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            ApplyRebindJson(json);
        }

        public void ResetToDefaults()
        {
            RemoveAllOverrides();
            PlayerPrefs.DeleteKey(RebindKey);
            PlayerPrefs.Save();
        }

        private string BuildRebindJson()
        {
            InputAction[] actions = GetAllActions();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("{");
            for (int i = 0; i < actions.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }

                sb.Append($"\"{actions[i].name}\":");
                sb.Append(actions[i].SaveBindingOverridesAsJson());
            }

            sb.Append("}");
            return sb.ToString();
        }

        private void ApplyRebindJson(string json)
        {
            InputAction[] actions = GetAllActions();
            foreach (InputAction action in actions)
            {
                string key = $"\"{action.name}\":";
                int start = json.IndexOf(key);
                if (start < 0)
                {
                    continue;
                }

                start += key.Length;

                int depth = 0;
                int end = start;
                bool inString = false;
                for (int i = start; i < json.Length; i++)
                {
                    char c = json[i];
                    if (c == '"' && (i == 0 || json[i - 1] != '\\'))
                    {
                        inString = !inString;
                    }

                    if (!inString)
                    {
                        if (c == '{' || c == '[')
                        {
                            depth++;
                        }
                        else if (c == '}' || c == ']')
                        {
                            depth--;
                        }

                        if (depth == 0 && (c == ',' || c == '}'))
                        {
                            end = i;
                            break;
                        }
                    }
                }

                string actionJson = json.Substring(start, end - start);
                action.LoadBindingOverridesFromJson(actionJson);
            }
        }

        private void RemoveAllOverrides()
        {
            foreach (InputAction action in GetAllActions())
            {
                action.RemoveAllBindingOverrides();
            }
        }

        private InputAction[] GetAllActions()
        {
            return new[]
            {
                MoveAction,
                MeleeAttackAction,
                RangedAttackAction,
                Skill1Action,
                Skill2Action,
                Digit1Action,
                Digit2Action,
                Digit3Action,
                Digit4Action,
                StatWindowAction,
                DebugLevelUpAction
            };
        }
    }
}
