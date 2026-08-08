using UnityEngine;
using UnityEngine.InputSystem;

namespace Necrocis
{
    /// <summary>
    /// Centralized input action manager with runtime rebinding support.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        // 싱글톤: null이면 자동 생성 (씬에 없어도 동작 보장)
        public static InputManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<InputManager>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject("InputManager");
                        instance = obj.AddComponent<InputManager>();
                    }
                }

                instance?.EnsureInitialized();
                return instance;
            }
        }

        private static InputManager instance;

        // --- InputAction 정의 ---
        public InputAction MoveAction { get; private set; }         // 이동 (방향키 2DVector 컴포지트)

        public InputAction MeleeAttackAction { get; private set; }  // 근거리 공격 (Q)
        public InputAction RangedAttackAction { get; private set; } // 원거리 공격 (W)
        public InputAction Skill1Action { get; private set; }
        public InputAction Skill2Action { get; private set; }

        public InputAction Digit1Action { get; private set; }       // 숫자키 1 (레벨업/직업 선택)
        public InputAction Digit2Action { get; private set; }       // 숫자키 2
        public InputAction Digit3Action { get; private set; }       // 숫자키 3
        public InputAction Digit4Action { get; private set; }       // 숫자키 4
        public InputAction Digit5Action { get; private set; }       // 숫자키 5

        public InputAction DashAction { get; private set; }          // 대시 (Shift)
        public InputAction StatWindowAction { get; private set; }   // 스탯창 토글 (O)
        public InputAction InventoryAction { get; private set; }    // 인벤토리 토글 (I)
        public InputAction DebugLevelUpAction { get; private set; } // 디버그 레벨업 (P)

        private const string RebindKey = "InputRebinds"; // PlayerPrefs 키(리바인딩 저장용)

        // 싱글톤 초기화: 액션 생성 -> 리바인딩 로드 -> 전체 활성화
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지

            EnsureInitialized();
            Debug.Log("[InputManager] Initialized");
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void EnsureInitialized()
        {
            if (MoveAction != null)
            {
                return;
            }

            CreateActions();
            LoadRebinds();
            EnableAll();
        }

        // 모든 InputAction을 코드로 정의 (.inputactions 에셋 없이 동작)
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
            Digit5Action = new InputAction("Digit5", InputActionType.Button, "<Keyboard>/5");

            DashAction = new InputAction("Dash", InputActionType.Button, "<Keyboard>/space");

            StatWindowAction = new InputAction("StatWindow", InputActionType.Button, "<Keyboard>/o");
            InventoryAction = new InputAction("Inventory", InputActionType.Button, "<Keyboard>/i");
            DebugLevelUpAction = new InputAction("DebugLevelUp", InputActionType.Button, "<Keyboard>/p");
        }

        // 모든 액션 활성화 (Enable 되어야 입력을 받을 수 있음)
        private void EnableAll()
        {
            SetActionsEnabled(true);
        }

        // 비활성화 시 모든 액션 Disable (메모리 누수 방지)
        private void OnDisable()
        {
            SetActionsEnabled(false);
        }

        // ---------------------------------
        // 리바인딩
        // ---------------------------------

        // 현재 리바인딩 상태를 PlayerPrefs에 JSON으로 저장
        public void SaveRebinds()
        {
            string json = BuildRebindJson();
            PlayerPrefs.SetString(RebindKey, json);
            PlayerPrefs.Save();
        }

        // PlayerPrefs에서 리바인딩 JSON을 읽어 각 액션에 적용
        public void LoadRebinds()
        {
            string json = PlayerPrefs.GetString(RebindKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            ApplyRebindJson(json);
        }

        // 모든 리바인딩을 기본값으로 초기화
        public void ResetToDefaults()
        {
            RemoveAllOverrides();
            PlayerPrefs.DeleteKey(RebindKey);
            PlayerPrefs.Save();
        }

        // 모든 액션의 바인딩 오버라이드를 JSON으로 직렬화
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

        // JSON에서 각 액션의 바인딩 오버라이드를 파싱하여 적용
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

        // 모든 액션에서 바인딩 오버라이드 제거
        private void RemoveAllOverrides()
        {
            foreach (InputAction action in GetAllActions())
            {
                action?.RemoveAllBindingOverrides();
            }
        }

        private void SetActionsEnabled(bool enabled)
        {
            foreach (InputAction action in GetAllActions())
            {
                if (action == null)
                {
                    continue;
                }

                if (enabled)
                {
                    action.Enable();
                }
                else
                {
                    action.Disable();
                }
            }
        }

        // 관리 중인 모든 InputAction 배열 반환 (리바인딩 일괄 처리용)
        private InputAction[] GetAllActions()
        {
            return new[]
            {
                MoveAction,
                MeleeAttackAction,
                RangedAttackAction,
                Skill1Action,
                Skill2Action,
                DashAction,
                Digit1Action,
                Digit2Action,
                Digit3Action,
                Digit4Action,
                Digit5Action,
                StatWindowAction,
                InventoryAction,
                DebugLevelUpAction
            };
        }
    }
}
