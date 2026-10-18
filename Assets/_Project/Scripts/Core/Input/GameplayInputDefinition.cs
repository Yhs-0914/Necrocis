using UnityEngine.InputSystem;

namespace Necrocis
{
    // Project-owned IDs preserve binding identity across instances and process restarts.
    // TraversalHold remains independent of the rebindable Dash action, as the old raw Space input was.
    public static class GameplayInputDefinition
    {
        public static InputActionAsset CreateAsset() => InputActionAsset.FromJson(Json);

        private const string Json = @"{
  ""name"": ""NecrocisGameplayInput"",
  ""maps"": [
    {
      ""name"": ""Gameplay"",
      ""id"": ""3037e947-e517-5b71-945d-e5d125500239"",
      ""actions"": [
        {
          ""name"": ""Move"",
          ""type"": ""Value"",
          ""id"": ""4c611732-5276-5687-b393-b1e56ff9b0a7"",
          ""expectedControlType"": ""Vector2"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": true
        },
        {
          ""name"": ""MeleeAttack"",
          ""type"": ""Button"",
          ""id"": ""50e38c00-7e7e-5fb7-a0f3-5096d438ce4f"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        },
        {
          ""name"": ""RangedAttack"",
          ""type"": ""Button"",
          ""id"": ""90fad02e-7884-5448-a91f-f195a2740f62"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        },
        {
          ""name"": ""Skill1"",
          ""type"": ""Button"",
          ""id"": ""16ab5b60-5fda-5a99-b837-f829fd37dcee"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        },
        {
          ""name"": ""Skill2"",
          ""type"": ""Button"",
          ""id"": ""5ccf0cb2-e1a0-559e-bb73-e9d2b89544b7"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        },
        {
          ""name"": ""Digit1"",
          ""type"": ""Button"",
          ""id"": ""529b61a8-9842-5d74-9d6f-e214d91a8cdc"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        },
        {
          ""name"": ""Digit2"",
          ""type"": ""Button"",
          ""id"": ""db5001cc-1842-5695-9f74-af78674a8d15"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        },
        {
          ""name"": ""Digit3"",
          ""type"": ""Button"",
          ""id"": ""a7444711-e026-5041-b86b-a25d2954d556"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        },
        {
          ""name"": ""Digit4"",
          ""type"": ""Button"",
          ""id"": ""5780c4c1-2791-5ac2-8338-a4eab9b57743"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        },
        {
          ""name"": ""Digit5"",
          ""type"": ""Button"",
          ""id"": ""ac3ae62d-6708-5396-b44e-9edc26417432"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        },
        {
          ""name"": ""Dash"",
          ""type"": ""Button"",
          ""id"": ""ed26f9e9-d48c-5f46-a1b8-da481286497a"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        },
        {
          ""name"": ""StatWindow"",
          ""type"": ""Button"",
          ""id"": ""e4bbfe85-3b60-554e-a916-9a5e8d475a64"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        },
        {
          ""name"": ""Inventory"",
          ""type"": ""Button"",
          ""id"": ""8f34b14a-6473-52ed-bf8d-33804cd6056e"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        },
        {
          ""name"": ""DebugLevelUp"",
          ""type"": ""Button"",
          ""id"": ""ed3196b6-a03c-519f-a814-3739d58092cd"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        },
        {
          ""name"": ""TraversalHold"",
          ""type"": ""Button"",
          ""id"": ""ea427848-c3fd-5a23-8788-9fcf350c9e1a"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        },
        {
          ""name"": ""MenuBack"",
          ""type"": ""Button"",
          ""id"": ""3d48b099-16e8-5790-8890-5eade5829b6e"",
          ""expectedControlType"": ""Button"",
          ""processors"": """",
          ""interactions"": """",
          ""initialStateCheck"": false
        }
      ],
      ""bindings"": [
        {
          ""name"": ""2DVector"",
          ""id"": ""c31aca61-fc53-50ad-9e11-7202d5e9baa0"",
          ""path"": ""2DVector"",
          ""action"": ""Move"",
          ""isComposite"": true
        },
        {
          ""name"": ""Up"",
          ""id"": ""bb02e368-7cd9-5611-8844-ed22cf6987b3"",
          ""path"": ""<Keyboard>/upArrow"",
          ""action"": ""Move"",
          ""isPartOfComposite"": true
        },
        {
          ""name"": ""Down"",
          ""id"": ""80d22e8b-6977-5be9-bd1c-6590033ce40d"",
          ""path"": ""<Keyboard>/downArrow"",
          ""action"": ""Move"",
          ""isPartOfComposite"": true
        },
        {
          ""name"": ""Left"",
          ""id"": ""7360de90-8e06-5136-bb2a-60d1a81595e3"",
          ""path"": ""<Keyboard>/leftArrow"",
          ""action"": ""Move"",
          ""isPartOfComposite"": true
        },
        {
          ""name"": ""Right"",
          ""id"": ""0a8f4ad1-307e-541d-8477-68916624da76"",
          ""path"": ""<Keyboard>/rightArrow"",
          ""action"": ""Move"",
          ""isPartOfComposite"": true
        },
        {
          ""name"": """",
          ""id"": ""b9b7dd1b-dfde-5656-b592-4a29d193a091"",
          ""path"": ""<Keyboard>/q"",
          ""action"": ""MeleeAttack""
        },
        {
          ""name"": """",
          ""id"": ""b01ce707-1bcd-52b7-89ac-809da89119c2"",
          ""path"": ""<Keyboard>/w"",
          ""action"": ""RangedAttack""
        },
        {
          ""name"": """",
          ""id"": ""9b361c61-fc29-5fba-aa73-5e8bdb5527ff"",
          ""path"": ""<Keyboard>/e"",
          ""action"": ""Skill1""
        },
        {
          ""name"": """",
          ""id"": ""6e740e87-9bab-5434-b97f-91cf8ed0c51e"",
          ""path"": ""<Keyboard>/r"",
          ""action"": ""Skill2""
        },
        {
          ""name"": """",
          ""id"": ""2894b81a-4ab3-53a0-aa69-98061fc47a9c"",
          ""path"": ""<Keyboard>/1"",
          ""action"": ""Digit1""
        },
        {
          ""name"": """",
          ""id"": ""233927e6-0694-51f6-80e7-7ac19b5cf5cd"",
          ""path"": ""<Keyboard>/2"",
          ""action"": ""Digit2""
        },
        {
          ""name"": """",
          ""id"": ""65a11b7c-bffd-5847-849b-d11020ada79c"",
          ""path"": ""<Keyboard>/3"",
          ""action"": ""Digit3""
        },
        {
          ""name"": """",
          ""id"": ""407dd87c-3018-56ed-b1a0-d26cdde038a5"",
          ""path"": ""<Keyboard>/4"",
          ""action"": ""Digit4""
        },
        {
          ""name"": """",
          ""id"": ""4df2b555-6919-5ae1-b5f6-c7e467a98b31"",
          ""path"": ""<Keyboard>/5"",
          ""action"": ""Digit5""
        },
        {
          ""name"": """",
          ""id"": ""59e88cb1-e0cf-5161-bda1-ed2afac99de9"",
          ""path"": ""<Keyboard>/space"",
          ""action"": ""Dash""
        },
        {
          ""name"": """",
          ""id"": ""4cd5ecaa-c3aa-5f8e-ab5e-4d731e88d4dc"",
          ""path"": ""<Keyboard>/o"",
          ""action"": ""StatWindow""
        },
        {
          ""name"": """",
          ""id"": ""1878af90-c3b2-5ec1-8316-2d81f3fa9ecd"",
          ""path"": ""<Keyboard>/i"",
          ""action"": ""Inventory""
        },
        {
          ""name"": """",
          ""id"": ""ead1abd3-e1a0-5839-9c3c-90b64f758f4f"",
          ""path"": ""<Keyboard>/p"",
          ""action"": ""DebugLevelUp""
        },
        {
          ""name"": """",
          ""id"": ""cd47ba0c-873f-56c2-b70c-b8c78607325f"",
          ""path"": ""<Keyboard>/space"",
          ""action"": ""TraversalHold""
        },
        {
          ""name"": """",
          ""id"": ""26d505da-0047-550b-8860-7df7744977ca"",
          ""path"": ""<Keyboard>/escape"",
          ""action"": ""MenuBack""
        }
      ]
    }
  ],
  ""controlSchemes"": []
}";
    }
}
