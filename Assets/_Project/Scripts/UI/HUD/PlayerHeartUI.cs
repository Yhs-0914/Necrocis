using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Necrocis
{
    /// <summary>
    /// Player health heart HUD.
    /// Creates a top-left heart row and syncs it with Health.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerHeartUI : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool autoCreateRuntimeUI = true;
        [SerializeField] private bool keepCanvasAcrossScenes = true;
        [SerializeField] private bool autoFindHealth = true;

        [Header("References")]
        [SerializeField] private MonoBehaviour health;
        private IHealthDisplay healthDisplay;
        [SerializeField] private RectTransform heartContainer;

        [Header("Sprites")]
        [SerializeField] private Sprite fullHeartSprite;
        [SerializeField] private Sprite halfHeartSprite;
        [SerializeField] private Sprite emptyHeartSprite;

        [Header("Layout")]
        [SerializeField] private string canvasName = "PlayerHudCanvas";
        [SerializeField] private string heartContainerName = "HeartContainer";
        [SerializeField] private int canvasSortingOrder = 60;
        [SerializeField] private Vector2 canvasReferenceResolution = new Vector2(1920f, 1080f);
        [SerializeField] private Vector2 anchoredPosition = new Vector2(24f, -24f);
        [SerializeField] private Vector2 heartSize = new Vector2(32f, 32f);
        [SerializeField] private float spacing = 4f;

        [Header("Heart Mapping")]
        [SerializeField] private int healthPerHeart = 2;
        [Tooltip("Hearts per row. Total heart count always follows maximum HP; zero uses one row.")]
        [SerializeField] private int maxVisibleHearts = 12;
        [SerializeField] private Color emptyHeartTint = new Color(1f, 1f, 1f, 0.35f);

        private static GameObject persistentCanvas;
        private readonly List<Image> heartImages = new List<Image>();

        private RectTransform boundLayoutContainer;
        private bool subscribed;
        private int cachedHeartCount = -1;
        private Vector2 cachedAppliedHeartSize = new Vector2(-1f, -1f);
        private Vector2 cachedAppliedAnchorPos = new Vector2(float.MinValue, float.MinValue);

        private void Awake()
        {
            EnsureUIReady();
            ResolveHealth();
        }

        private void OnEnable()
        {
            EnsureUIReady();
            ResolveHealth();
            TrySubscribeHealth();
            SceneManager.sceneLoaded += HandleSceneLoaded;
            RefreshNow();
        }

        private void OnValidate()
        {
            if (heartContainer == null)
            {
                return;
            }

            ApplyContainerLayoutSettings();
            ApplyHeartSizeToExisting(force: true);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            TryUnsubscribeHealth();
        }

        private void HandleSceneLoaded(Scene _, LoadSceneMode __)
        {
            EnsureUIReady();
            ResolveHealth();
            TrySubscribeHealth();
            RefreshNow();
        }

        private void ResolveHealth()
        {
            if(health!=null && healthDisplay?.Owner==health)return;
            if(health==null && !autoFindHealth)return;
            TryUnsubscribeHealth();
            healthDisplay=PlayerDisplayBindings.Health(health!=null?health.gameObject:gameObject);
            health=healthDisplay?.Owner as MonoBehaviour;
        }

        private void TrySubscribeHealth()
        {
            if (subscribed || health == null)
            {
                return;
            }

            healthDisplay.OnHealthChanged += HandleHealthChanged;
            subscribed = true;
        }

        private void TryUnsubscribeHealth()
        {
            if (!subscribed || healthDisplay == null)
            {
                subscribed = false;
                return;
            }

            healthDisplay.OnHealthChanged -= HandleHealthChanged;
            subscribed = false;
        }

        private void HandleHealthChanged(float current, float max)
        {
            RefreshHearts(current, max);
        }

        private void RefreshNow()
        {
            if (health == null)
            {
                return;
            }

            RefreshHearts(healthDisplay.CurrentHealth, healthDisplay.MaxHealth);
        }

        private void EnsureUIReady()
        {
            if (heartContainer != null)
            {
                ApplyContainerLayoutSettings();
                EnsureCanvasPersistence();
                return;
            }

            TryBindExistingUI();
            if (heartContainer != null)
            {
                ApplyContainerLayoutSettings();
                EnsureCanvasPersistence();
                return;
            }

            if (!autoCreateRuntimeUI)
            {
                return;
            }

            BuildRuntimeUI();
            ApplyContainerLayoutSettings();
            EnsureCanvasPersistence();
        }

        private void TryBindExistingUI()
        {
            GameObject canvasObject = GameObject.Find(canvasName);
            if (canvasObject == null)
            {
                return;
            }

            Transform existingContainer = canvasObject.transform.Find(heartContainerName);
            if (existingContainer != null)
            {
                heartContainer = existingContainer as RectTransform;
            }
        }

        private void BuildRuntimeUI()
        {
            if (persistentCanvas == null)
            {
                persistentCanvas = GameObject.Find(canvasName);
            }

            if (persistentCanvas == null)
            {
                persistentCanvas = new GameObject(
                    canvasName,
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));

                Canvas canvas = persistentCanvas.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = canvasSortingOrder;

                CanvasScaler scaler = persistentCanvas.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = canvasReferenceResolution;
            }

            Transform existing = persistentCanvas.transform.Find(heartContainerName);
            if (existing != null)
            {
                heartContainer = existing as RectTransform;
                ApplyContainerLayoutSettings();
                return;
            }

            GameObject containerObject = new GameObject(heartContainerName, typeof(RectTransform));
            containerObject.transform.SetParent(persistentCanvas.transform, false);
            heartContainer = containerObject.GetComponent<RectTransform>();

            heartContainer.anchorMin = new Vector2(0f, 1f);
            heartContainer.anchorMax = new Vector2(0f, 1f);
            heartContainer.pivot = new Vector2(0f, 1f);
            heartContainer.anchoredPosition = anchoredPosition;
            heartContainer.localScale = Vector3.one;

            HorizontalLayoutGroup layout = containerObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = containerObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ApplyContainerLayoutSettings();
        }

        private void EnsureCanvasPersistence()
        {
            if (!keepCanvasAcrossScenes || heartContainer == null)
            {
                return;
            }

            Canvas canvas = heartContainer.GetComponentInParent<Canvas>(true);
            if (canvas == null)
            {
                return;
            }

            persistentCanvas = canvas.gameObject;
            DontDestroyOnLoad(persistentCanvas);
        }

        private void RefreshHearts(float currentHealth, float maxHealth)
        {
            if (heartContainer == null) return;
            SynchronizeContainerIdentity();
            if (maxHealth <= 0f) { EnsureHeartImageCount(0); ApplyContainerLayoutSettings(); return; }

            int valuePerHeart = Mathf.Max(1, healthPerHeart);
            int maxHealthUnits = Mathf.Max(0, Mathf.RoundToInt(maxHealth));
            int currentHealthUnits = Mathf.Clamp(Mathf.RoundToInt(currentHealth), 0, maxHealthUnits);
            int totalHearts = Mathf.Max(1, (maxHealthUnits + valuePerHeart - 1) / valuePerHeart);
            EnsureHeartImageCount(totalHearts);
            ApplyContainerLayoutSettings();
            ApplyHeartSizeToExisting(force: false);

            int filledUnits = Mathf.Clamp(currentHealthUnits, 0, totalHearts * valuePerHeart);

            for (int i = 0; i < totalHearts; i++)
            {
                int units = Mathf.Clamp(filledUnits - (i * valuePerHeart), 0, valuePerHeart);
                ApplyHeartState(heartImages[i], ToHeartSpriteUnits(units, valuePerHeart));
            }
        }

        private void EnsureHeartImageCount(int count)
        {
            if (count == cachedHeartCount && heartImages.Count == count)
            {
                return;
            }

            for (int i = heartContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(heartContainer.GetChild(i).gameObject);
            }

            heartImages.Clear();
            for (int i = 0; i < count; i++)
            {
                GameObject heartObject = new GameObject($"Heart_{i + 1}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                heartObject.transform.SetParent(heartContainer, false);

                RectTransform rect = heartObject.GetComponent<RectTransform>();
                rect.sizeDelta = heartSize;
                rect.localScale = Vector3.one;

                LayoutElement layout = heartObject.GetComponent<LayoutElement>();
                layout.minWidth = heartSize.x;
                layout.minHeight = heartSize.y;
                layout.preferredWidth = heartSize.x;
                layout.preferredHeight = heartSize.y;
                layout.flexibleWidth = 0f;
                layout.flexibleHeight = 0f;

                Image image = heartObject.GetComponent<Image>();
                image.preserveAspect = true;
                heartImages.Add(image);
            }

            cachedHeartCount = count;
            cachedAppliedHeartSize = new Vector2(-1f, -1f);
            LayoutRebuilder.ForceRebuildLayoutImmediate(heartContainer);
        }

        private void SynchronizeContainerIdentity()
        {
            if(boundLayoutContainer==heartContainer)return;
            boundLayoutContainer=heartContainer;heartImages.Clear();cachedHeartCount=-1;
            cachedAppliedHeartSize=new Vector2(-1f,-1f);
            cachedAppliedAnchorPos=new Vector2(float.MinValue,float.MinValue);
        }

        private void ApplyContainerLayoutSettings()
        {
            if (heartContainer == null)
            {
                return;
            }

            if(Application.isPlaying)SynchronizeContainerIdentity();
            if (cachedAppliedAnchorPos != anchoredPosition)
            {
                heartContainer.anchoredPosition = anchoredPosition;
                cachedAppliedAnchorPos = anchoredPosition;
            }

            if (!Application.isPlaying)
            {
                var authoredLayout=heartContainer.GetComponent<HorizontalLayoutGroup>();
                if(authoredLayout!=null)authoredLayout.spacing=spacing;
                return;
            }

            // Layout is explicit so existing authored single-row containers migrate without
            // two LayoutGroups competing during Unity's deferred component destruction.
            var oldLayout=heartContainer.GetComponent<HorizontalLayoutGroup>();
            if(oldLayout!=null){oldLayout.enabled=false;Destroy(oldLayout);}
            var fitter=heartContainer.GetComponent<ContentSizeFitter>();if(fitter!=null)fitter.enabled=false;
            int columns=maxVisibleHearts>0?maxVisibleHearts:Mathf.Max(1,heartImages.Count);
            int usedColumns=Mathf.Min(columns,heartImages.Count);
            int rows=(heartImages.Count+columns-1)/columns;
            var size=new Vector2(Mathf.Max(1f,heartSize.x),Mathf.Max(1f,heartSize.y));
            heartContainer.sizeDelta=new Vector2(usedColumns*size.x+Mathf.Max(0,usedColumns-1)*spacing,rows*size.y+Mathf.Max(0,rows-1)*spacing);
            for(int i=0;i<heartImages.Count;i++)
            {
                var rect=heartImages[i].rectTransform;rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0f,1f);
                rect.anchoredPosition=new Vector2(i%columns*(size.x+spacing),-(i/columns)*(size.y+spacing));
            }
        }

        private void ApplyHeartSizeToExisting(bool force)
        {
            if (heartContainer == null || heartImages.Count == 0)
            {
                return;
            }

            Vector2 clampedSize = new Vector2(
                Mathf.Max(1f, heartSize.x),
                Mathf.Max(1f, heartSize.y));

            if (!force && cachedAppliedHeartSize == clampedSize)
            {
                return;
            }

            for (int i = 0; i < heartImages.Count; i++)
            {
                Image image = heartImages[i];
                if (image == null)
                {
                    continue;
                }

                RectTransform rect = image.rectTransform;
                rect.sizeDelta = clampedSize;
                rect.localScale = Vector3.one;

                LayoutElement layout = image.GetComponent<LayoutElement>();
                if (layout == null)
                {
                    layout = image.gameObject.AddComponent<LayoutElement>();
                }

                layout.minWidth = clampedSize.x;
                layout.minHeight = clampedSize.y;
                layout.preferredWidth = clampedSize.x;
                layout.preferredHeight = clampedSize.y;
                layout.flexibleWidth = 0f;
                layout.flexibleHeight = 0f;
            }

            cachedAppliedHeartSize = clampedSize;
            LayoutRebuilder.ForceRebuildLayoutImmediate(heartContainer);
        }

        private void ApplyHeartState(Image image, int units)
        {
            if (image == null)
            {
                return;
            }

            if (units >= 2)
            {
                image.sprite = fullHeartSprite != null ? fullHeartSprite : halfHeartSprite;
                image.color = Color.white;
                return;
            }

            if (units == 1)
            {
                image.sprite = halfHeartSprite != null ? halfHeartSprite : fullHeartSprite;
                image.color = Color.white;
                return;
            }

            image.sprite = emptyHeartSprite != null ? emptyHeartSprite : fullHeartSprite;
            image.color = emptyHeartSprite != null ? Color.white : emptyHeartTint;
        }

        private static int ToHeartSpriteUnits(int healthUnitsInHeart, int valuePerHeart)
        {
            if (healthUnitsInHeart <= 0)
            {
                return 0;
            }

            return healthUnitsInHeart >= valuePerHeart ? 2 : 1;
        }
    }
}
