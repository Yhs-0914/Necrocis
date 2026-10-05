using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Necrocis.RuntimeUiFactory;

namespace Necrocis
{
    /// <summary>Scene-owned victory confirmation; declining leaves the completed arena playable.</summary>
    [DisallowMultipleComponent]
    public sealed class FinalBossVictoryPrompt : MonoBehaviour
    {
        private bool scheduled, answered, leaving;
        private GameObject canvasObject;
        private Text status;
        private float previousTimeScale;
        private bool previousCursorVisible;
        private CursorLockMode previousCursorLock;
        private GameObject previousSelection;
        private PlayerController player;
        private PlayerAttack attack;
        private PlayerClassSkillController skills;
        private bool playerEnabled, attackEnabled, skillsEnabled;

        public bool IsOpen { get; private set; }
        public Button YesButton { get; private set; }
        public Button NoButton { get; private set; }
        public Canvas PromptCanvas => canvasObject != null ? canvasObject.GetComponent<Canvas>() : null;

        public void Schedule()
        {
            if (scheduled || answered) return;
            scheduled = true;
            StartCoroutine(ShowAfterVictory());
        }

        private IEnumerator ShowAfterVictory()
        {
            yield return new WaitForSecondsRealtime(5f);
            // Let any pause/level-up screen finish before taking ownership of input.
            while (Time.timeScale <= 0f) yield return null;
            if (PlayerController.Instance != null && PlayerController.Instance.IsDead) yield break;
            Show();
        }

        private void Show()
        {
            if (IsOpen || answered) return;
            EnsureEventSystem();
            BuildInterface();
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            previousTimeScale = Time.timeScale;
            previousCursorVisible = Cursor.visible;
            previousCursorLock = Cursor.lockState;
            player = PlayerController.Instance;
            if (player != null)
            {
                attack = player.GetComponent<PlayerAttack>();
                skills = player.GetComponent<PlayerClassSkillController>();
                playerEnabled = player.enabled;
                attackEnabled = attack != null && attack.enabled;
                skillsEnabled = skills != null && skills.enabled;
                player.enabled = false;
                if (attack != null) attack.enabled = false;
                if (skills != null) skills.enabled = false;
            }
            IsOpen = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            canvasObject.SetActive(true);
            NoButton.Select();
            AudioManager.Instance?.PlaySFX("SettingsOpen");
        }

        public void Decline()
        {
            if (!IsOpen || leaving) return;
            answered = true;
            canvasObject.SetActive(false);
            RestoreInput();
            AudioManager.Instance?.PlaySFX("UIClose");
        }

        public void ReturnToMainMenu()
        {
            if (!IsOpen || leaving) return;
            if (!Application.CanStreamedLevelBeLoaded(SceneLoader.SCENE_MAIN_MENU))
            {
                status.text = "메인메뉴를 불러올 수 없습니다.";
                AudioManager.Instance?.PlaySFX("UIInvalid");
                return;
            }
            leaving = true;
            answered = true;
            YesButton.interactable = NoButton.interactable = false;
            GameSettings.Save();
            AudioManager.Instance?.PlaySFX("UISelect");
            RestoreInput();
            // MarkFinalBossDefeated already persisted the clear before this prompt was scheduled.
            GameplaySessionLifecycle.LoadMainMenu();
        }

        private void RestoreInput()
        {
            if (!IsOpen) return;
            IsOpen = false;
            Time.timeScale = previousTimeScale;
            Cursor.visible = previousCursorVisible;
            Cursor.lockState = previousCursorLock;
            if (player != null) player.enabled = playerEnabled;
            if (attack != null) attack.enabled = attackEnabled;
            if (skills != null) skills.enabled = skillsEnabled;
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(previousSelection);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (canvasObject != null) canvasObject.SetActive(false);
            RestoreInput();
        }

        private void BuildInterface()
        {
            if (canvasObject != null) return;
            canvasObject = CreateUiObject("FinalBossVictoryCanvas", transform);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 600;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = .5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            Image dim = CreateImage("Backdrop", canvasObject.transform, new Color(.005f, .002f, .006f, .76f));
            Stretch(dim.rectTransform);
            Image panel = CreateImage("VictoryPanel", dim.transform, new Color(.055f, .012f, .024f, .98f));
            SetRect(panel.rectTransform, Vector2.one * .5f, new Vector2(820f, 430f), Vector2.zero);
            Outline border = panel.gameObject.AddComponent<Outline>();
            border.effectColor = new Color(.67f, .12f, .08f, .95f);
            border.effectDistance = new Vector2(3f, -3f);
            Font font = GameUiTheme.LoadFont();
            Text title = CreateText("Title", panel.transform, "최종보스 처치", font, 42, new Color(1f, .84f, .53f));
            SetRect(title.rectTransform, Vector2.one * .5f, new Vector2(700f, 66f), new Vector2(0f, 136f));
            title.alignment = TextAnchor.MiddleCenter;
            Text question = CreateText("Question", panel.transform, "게임 메인메뉴로 돌아가겠습니까?", font, 32, new Color(1f, .91f, .8f));
            SetRect(question.rectTransform, Vector2.one * .5f, new Vector2(730f, 100f), new Vector2(0f, 32f));
            question.alignment = TextAnchor.MiddleCenter;
            question.horizontalOverflow = HorizontalWrapMode.Wrap;
            YesButton = CreateChoice(panel.transform, "YesButton", "Yes", new Vector2(-178f, -100f), font);
            NoButton = CreateChoice(panel.transform, "NoButton", "No", new Vector2(178f, -100f), font);
            YesButton.onClick.AddListener(ReturnToMainMenu);
            NoButton.onClick.AddListener(Decline);
            Navigation navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnLeft = NoButton, selectOnRight = NoButton };
            YesButton.navigation = navigation;
            navigation.selectOnLeft = navigation.selectOnRight = YesButton;
            NoButton.navigation = navigation;
            status = CreateText("Status", panel.transform, string.Empty, font, 22, new Color(1f, .55f, .45f));
            SetRect(status.rectTransform, Vector2.one * .5f, new Vector2(700f, 36f), new Vector2(0f, -165f));
            status.alignment = TextAnchor.MiddleCenter;
        }

        private static Button CreateChoice(Transform parent, string name, string label, Vector2 position, Font font)
        {
            Image image = CreateImage(name, parent, new Color(.14f, .035f, .035f, .9f));
            SetRect(image.rectTransform, Vector2.one * .5f, new Vector2(300f, 70f), position);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1f, .65f, .3f);
            colors.selectedColor = new Color(1f, .65f, .3f);
            colors.pressedColor = new Color(.7f, .3f, .1f);
            colors.fadeDuration = .1f;
            button.colors = colors;
            Outline outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(.8f, .48f, .16f, .65f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            Text text = CreateText("Label", image.transform, label, font, 34, new Color(1f, .87f, .62f));
            Stretch(text.rectTransform);
            text.alignment = TextAnchor.MiddleCenter;
            return button;
        }
    }
}
