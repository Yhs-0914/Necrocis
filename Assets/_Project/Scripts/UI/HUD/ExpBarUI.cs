using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Necrocis
{
    /// <summary>
    /// EXP 바 UI 표시 및 레벨/경험치 텍스트 동기화.
    /// 수동으로 바인딩한 UI가 있으면 우선 사용하고, 없으면 런타임 기본 UI를 생성한다.
    /// </summary>
    public class ExpBarUI : MonoBehaviour
    {
        private enum FillVisualMode
        {
            FillAmount = 0,
            SpriteByStep = 1
        }

        [Header("Auto Setup")]
        [SerializeField] private bool autoCreateRuntimeUI = true;
        [SerializeField] private bool keepCanvasAcrossScenes = true;

        [Header("References")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Text levelText;
        [SerializeField] private Text expText;

        [Header("Fill Visual")]
        [SerializeField] private FillVisualMode fillVisualMode = FillVisualMode.FillAmount;
        [SerializeField] private Sprite[] progressSprites;
        [SerializeField] private bool applyFillAmountWhenUsingSprites = false;

        [Header("Runtime Fallback Colors")]
        [SerializeField] private Color barColor = new Color(0.3f, 0.7f, 1f, 1f);
        [SerializeField] private Color bgColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);

        private static readonly Vector2 ResponsiveReferenceResolution = new Vector2(1920f, 1080f);
        private const float ResponsiveMatchWidthOrHeight = 0.5f;
        private const float ExpBarBottomMargin = -150f;
        private const float ExpBarWidth = 1160f;
        private const float ExpBarHeight = 540f;

        private static GameObject persistentCanvas;
        private bool runtimeUIBuilt;
        private int lastAppliedScreenWidth = -1;
        private int lastAppliedScreenHeight = -1;

        private void Awake()
        {
            EnsureUIReady();
        }

        private void Start()
        {
            EnsureUIReady();
            ApplyResponsiveLayout();
            UpdateDisplay();
        }

        private void OnEnable()
        {
            EnsureUIReady();
            ApplyResponsiveLayout();
            LevelUpManager.OnExpGained += OnExpGained;
            LevelUpManager.OnLevelUp += OnLevelUp;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            UpdateDisplay();
        }

        private void OnDisable()
        {
            LevelUpManager.OnExpGained -= OnExpGained;
            LevelUpManager.OnLevelUp -= OnLevelUp;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void LateUpdate()
        {
            if (lastAppliedScreenWidth == Screen.width && lastAppliedScreenHeight == Screen.height)
            {
                return;
            }

            ApplyResponsiveLayout();
        }

        private void OnExpGained(int amount)
        {
            UpdateDisplay();
        }

        private void OnLevelUp()
        {
            UpdateDisplay();
        }

        private void HandleSceneLoaded(Scene _, LoadSceneMode __)
        {
            EnsureUIReady();
            ApplyResponsiveLayout();
            UpdateDisplay();
        }

        private void EnsureUIReady()
        {
            if (fillImage != null && levelText != null && expText != null)
            {
                EnsureCanvasPersistence();
                return;
            }

            TryAutoBindExistingUI();
            if (fillImage != null && levelText != null && expText != null)
            {
                EnsureCanvasPersistence();
                return;
            }

            if (!autoCreateRuntimeUI || runtimeUIBuilt)
            {
                return;
            }

            BuildRuntimeUI();
            runtimeUIBuilt = true;
            EnsureCanvasPersistence();
        }

        private void TryAutoBindExistingUI()
        {
            Transform expBarRoot = null;
            GameObject expBarCanvas = GameObject.Find("ExpBarCanvas");
            if (expBarCanvas != null)
            {
                expBarRoot = expBarCanvas.transform;
            }

            if (fillImage == null)
            {
                if (expBarRoot != null)
                {
                    Transform fill = expBarRoot.Find("ExpBar/Fill");
                    if (fill != null)
                    {
                        fillImage = fill.GetComponent<Image>();
                    }
                }
            }

            if (levelText == null)
            {
                if (expBarRoot != null)
                {
                    Transform level = expBarRoot.Find("Level");
                    if (level != null)
                    {
                        levelText = level.GetComponent<Text>();
                    }
                }
            }

            if (expText == null)
            {
                if (expBarRoot != null)
                {
                    Transform exp = expBarRoot.Find("ExpBar/ExpText");
                    if (exp != null)
                    {
                        expText = exp.GetComponent<Text>();
                    }
                }
            }

            if (fillImage != null && levelText != null && expText != null)
            {
                return;
            }

            Text[] texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Image[] images = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (fillImage == null)
            {
                for (int i = 0; i < images.Length; i++)
                {
                    Image candidate = images[i];
                    if (candidate == null) continue;

                    string lowerName = candidate.name.ToLowerInvariant();
                    if (lowerName.Contains("expfill") || lowerName == "fill" || lowerName.Contains("exp_fill"))
                    {
                        fillImage = candidate;
                        break;
                    }
                }
            }

            if (levelText == null)
            {
                for (int i = 0; i < texts.Length; i++)
                {
                    Text candidate = texts[i];
                    if (candidate == null) continue;

                    string lowerName = candidate.name.ToLowerInvariant();
                    if (lowerName.Contains("level") || lowerName.Contains("lv"))
                    {
                        levelText = candidate;
                        break;
                    }
                }
            }

            if (expText == null)
            {
                for (int i = 0; i < texts.Length; i++)
                {
                    Text candidate = texts[i];
                    if (candidate == null || candidate == levelText) continue;

                    string lowerName = candidate.name.ToLowerInvariant();
                    if (lowerName.Contains("exp"))
                    {
                        expText = candidate;
                        break;
                    }
                }
            }
        }

        private void UpdateDisplay()
        {
            int level = LevelUpManager.GetCurrentLevel();
            int currentExp = LevelUpManager.GetCurrentExp();
            int expRequired = Mathf.Max(1, LevelUpManager.GetExpRequired());
            float progress = Mathf.Clamp01((float)currentExp / expRequired);

            UpdateFill(progress);

            if (levelText != null)
            {
                levelText.text = $"Lv.{level}";
            }

            if (expText != null)
            {
                expText.text = $"{currentExp}/{expRequired}";
            }
        }

        private void UpdateFill(float progress)
        {
            if (fillImage == null)
            {
                return;
            }

            if (fillVisualMode == FillVisualMode.SpriteByStep && progressSprites != null && progressSprites.Length > 0)
            {
                int index = Mathf.Clamp(Mathf.FloorToInt(progress * progressSprites.Length), 0, progressSprites.Length - 1);
                fillImage.sprite = progressSprites[index];

                if (applyFillAmountWhenUsingSprites)
                {
                    fillImage.fillAmount = progress;
                }

                return;
            }

            fillImage.fillAmount = progress;
        }

        private void BuildRuntimeUI()
        {
            if (persistentCanvas == null)
            {
                persistentCanvas = GameObject.Find("ExpBarCanvas");
            }

            if (persistentCanvas != null)
            {
                runtimeUIBuilt = true;
                return;
            }

            GameObject canvasObj = new GameObject("ExpBarCanvas");
            canvasObj.transform.SetParent(null);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject barRoot = CreateUIElement("ExpBar", canvasObj.transform);
            RectTransform barRect = barRoot.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.2f, 0f);
            barRect.anchorMax = new Vector2(0.8f, 0f);
            barRect.pivot = new Vector2(0.5f, 0f);
            barRect.anchoredPosition = new Vector2(0f, 20f);
            barRect.sizeDelta = new Vector2(0f, 25f);

            Image bgImage = barRoot.AddComponent<Image>();
            bgImage.color = bgColor;

            GameObject fillObject = CreateUIElement("Fill", barRoot.transform);
            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            fillImage = fillObject.AddComponent<Image>();
            fillImage.color = barColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 0f;

            GameObject levelObject = CreateUIElement("Level", canvasObj.transform);
            RectTransform levelRect = levelObject.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.1f, 0f);
            levelRect.anchorMax = new Vector2(0.2f, 0f);
            levelRect.pivot = new Vector2(0.5f, 0f);
            levelRect.anchoredPosition = new Vector2(0f, 20f);
            levelRect.sizeDelta = new Vector2(0f, 25f);

            levelText = levelObject.AddComponent<Text>();
            levelText.text = "Lv.1";
            levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            levelText.fontSize = 18;
            levelText.color = Color.white;
            levelText.alignment = TextAnchor.MiddleCenter;

            GameObject expObject = CreateUIElement("ExpText", barRoot.transform);
            RectTransform expRect = expObject.GetComponent<RectTransform>();
            expRect.anchorMin = Vector2.zero;
            expRect.anchorMax = Vector2.one;
            expRect.offsetMin = Vector2.zero;
            expRect.offsetMax = Vector2.zero;

            expText = expObject.AddComponent<Text>();
            expText.text = "0/100";
            expText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            expText.fontSize = 14;
            expText.color = Color.white;
            expText.alignment = TextAnchor.MiddleCenter;

            persistentCanvas = canvasObj;
            EnsureCanvasPersistence();
        }

        private void EnsureCanvasPersistence()
        {
            if (!keepCanvasAcrossScenes)
            {
                return;
            }

            Canvas canvas = null;
            if (fillImage != null)
            {
                canvas = fillImage.GetComponentInParent<Canvas>(true);
            }

            if (canvas == null && levelText != null)
            {
                canvas = levelText.GetComponentInParent<Canvas>(true);
            }

            if (canvas == null && expText != null)
            {
                canvas = expText.GetComponentInParent<Canvas>(true);
            }

            if (canvas == null)
            {
                return;
            }

            GameObject canvasObject = canvas.gameObject;
            persistentCanvas = canvasObject;
            DontDestroyOnLoad(canvasObject);
        }

        private void ApplyResponsiveLayout()
        {
            Canvas canvas = null;
            if (fillImage != null)
            {
                canvas = fillImage.GetComponentInParent<Canvas>(true);
            }

            if (canvas == null && levelText != null)
            {
                canvas = levelText.GetComponentInParent<Canvas>(true);
            }

            if (canvas == null && expText != null)
            {
                canvas = expText.GetComponentInParent<Canvas>(true);
            }

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

            RectTransform barRoot = fillImage != null
                ? fillImage.transform.parent as RectTransform
                : canvas.transform.Find("Image") as RectTransform;

            if (barRoot == null)
            {
                return;
            }

            barRoot.localScale = Vector3.one;
            barRoot.anchorMin = new Vector2(0.5f, 0f);
            barRoot.anchorMax = new Vector2(0.5f, 0f);
            barRoot.pivot = new Vector2(0.5f, 0f);
            barRoot.anchoredPosition = new Vector2(0f, ExpBarBottomMargin);
            barRoot.sizeDelta = new Vector2(ExpBarWidth, ExpBarHeight);

            lastAppliedScreenWidth = Screen.width;
            lastAppliedScreenHeight = Screen.height;
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }
    }
}
