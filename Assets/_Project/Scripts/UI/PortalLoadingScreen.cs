using UnityEngine;
using UnityEngine.UI;

namespace Necrocis
{
    /// <summary>
    /// Persistent, runtime-built loading presentation used for portal scene transitions.
    /// </summary>
    public sealed class PortalLoadingScreen : MonoBehaviour
    {
        private const int SortingOrder = 32760;
        private const float RunFrameRate = 10f;

        private CanvasGroup canvasGroup;
        private Text loadingText;
        private Text progressText;
        private Image runnerImage;
        private Image progressFill;
        private Image progressLead;
        private RectTransform runnerRect;
        private Sprite[] runFrames = System.Array.Empty<Sprite>();
        private Sprite fallbackSprite;
        private int currentFrame;
        private float nextFrameTime;
        private Vector2 runnerBasePosition;
        private float visibleSince;
        private float displayedProgress;

        public float VisibleDuration => isActiveAndEnabled
            ? Time.unscaledTime - visibleSince
            : 0f;

        public static PortalLoadingScreen Create(Transform owner)
        {
            GameObject loadingObject = new GameObject(nameof(PortalLoadingScreen));
            loadingObject.transform.SetParent(owner, false);
            return loadingObject.AddComponent<PortalLoadingScreen>();
        }

        private void Awake()
        {
            BuildInterface();
            gameObject.SetActive(false);
        }

        private void Update()
        {
            AnimateRunner();
            AnimateLoadingLabel();
            AnimateProgressBar();
        }

        public void Show(Sprite[] frames, Sprite fallback)
        {
            runFrames = frames ?? System.Array.Empty<Sprite>();
            fallbackSprite = fallback;
            currentFrame = 0;
            nextFrameTime = Time.unscaledTime;
            displayedProgress = 0f;
            visibleSince = Time.unscaledTime;

            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            runnerImage.sprite = FindFirstUsableFrame() ?? fallbackSprite;
            runnerImage.enabled = runnerImage.sprite != null;
            SetProgress(0f);
        }

        public void SetProgress(float progress)
        {
            displayedProgress = Mathf.Clamp01(progress);

            RectTransform fillRect = progressFill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(displayedProgress, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            RectTransform leadRect = progressLead.rectTransform;
            leadRect.anchorMin = new Vector2(displayedProgress, 0f);
            leadRect.anchorMax = new Vector2(displayedProgress, 1f);
            leadRect.anchoredPosition = Vector2.zero;
            progressLead.enabled = displayedProgress > 0.005f;
            progressText.text = $"{Mathf.RoundToInt(displayedProgress * 100f):00}%";
        }

        public void Hide()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            gameObject.SetActive(false);
        }

        private void BuildInterface()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

            Image background = RuntimeUiFactory.CreateImage(
                "Background",
                transform,
                new Color(0.018f, 0.006f, 0.011f, 1f));
            RuntimeUiFactory.Stretch(background.rectTransform);

            Image centerBand = RuntimeUiFactory.CreateImage(
                "CenterBand",
                background.transform,
                new Color(0.12f, 0.018f, 0.028f, 0.34f));
            RuntimeUiFactory.SetRect(
                centerBand.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(2200f, 430f),
                new Vector2(0f, 8f));

            CreateAccentLine(background.transform, "TopAccent", new Vector2(0f, 224f));
            CreateAccentLine(background.transform, "BottomAccent", new Vector2(0f, -208f));
            CreatePortalDiamond(background.transform);

            Font font = GameUiTheme.LoadFont();
            loadingText = RuntimeUiFactory.CreateText(
                "LoadingLabel",
                background.transform,
                "LOADING...",
                font,
                58,
                new Color(1f, 0.82f, 0.52f, 1f));
            RuntimeUiFactory.SetRect(
                loadingText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(760f, 90f),
                new Vector2(0f, 330f));
            loadingText.alignment = TextAnchor.MiddleCenter;

            runnerImage = RuntimeUiFactory.CreateImage(
                "RunningPlayer",
                background.transform,
                null,
                Color.white);
            runnerRect = runnerImage.rectTransform;
            runnerBasePosition = new Vector2(0f, 30f);
            RuntimeUiFactory.SetRect(
                runnerRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(245f, 245f),
                runnerBasePosition);
            runnerImage.preserveAspect = true;
            runnerImage.raycastTarget = false;

            Shadow runnerShadow = runnerImage.gameObject.AddComponent<Shadow>();
            runnerShadow.effectColor = new Color(0.25f, 0f, 0.025f, 0.9f);
            runnerShadow.effectDistance = new Vector2(7f, -8f);

            CreateProgressBar(background.transform, font);
        }

        private static void CreateAccentLine(Transform parent, string name, Vector2 position)
        {
            Image line = RuntimeUiFactory.CreateImage(
                name,
                parent,
                new Color(0.78f, 0.12f, 0.075f, 0.52f));
            RuntimeUiFactory.SetRect(
                line.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(980f, 3f),
                position);
        }

        private static void CreatePortalDiamond(Transform parent)
        {
            Image outer = RuntimeUiFactory.CreateImage(
                "PortalGlow",
                parent,
                new Color(0.58f, 0.035f, 0.055f, 0.22f));
            RuntimeUiFactory.SetRect(
                outer.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(300f, 300f),
                new Vector2(0f, 28f));
            outer.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            Image inner = RuntimeUiFactory.CreateImage(
                "PortalCore",
                outer.transform,
                new Color(0.035f, 0.008f, 0.014f, 0.96f));
            RuntimeUiFactory.Stretch(inner.rectTransform, new Vector2(8f, 8f), new Vector2(-8f, -8f));
        }

        private void CreateProgressBar(Transform parent, Font font)
        {
            Image border = RuntimeUiFactory.CreateImage(
                "ProgressBorder",
                parent,
                new Color(0.72f, 0.12f, 0.075f, 1f));
            RuntimeUiFactory.SetRect(
                border.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(760f, 42f),
                new Vector2(0f, -250f));

            Image track = RuntimeUiFactory.CreateImage(
                "ProgressTrack",
                border.transform,
                new Color(0.055f, 0.012f, 0.018f, 1f));
            RuntimeUiFactory.Stretch(track.rectTransform, new Vector2(4f, 4f), new Vector2(-4f, -4f));

            progressFill = RuntimeUiFactory.CreateImage(
                "ProgressFill",
                track.transform,
                new Color(0.92f, 0.23f, 0.08f, 1f));
            progressFill.rectTransform.pivot = new Vector2(0f, 0.5f);

            progressLead = RuntimeUiFactory.CreateImage(
                "ProgressLead",
                track.transform,
                new Color(1f, 0.8f, 0.38f, 1f));
            progressLead.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            progressLead.rectTransform.sizeDelta = new Vector2(7f, 0f);

            progressText = RuntimeUiFactory.CreateText(
                "ProgressPercent",
                parent,
                "00%",
                font,
                24,
                new Color(0.88f, 0.66f, 0.48f, 1f));
            RuntimeUiFactory.SetRect(
                progressText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(240f, 42f),
                new Vector2(0f, -301f));
            progressText.alignment = TextAnchor.MiddleCenter;
        }

        private void AnimateRunner()
        {
            if (runnerImage == null)
            {
                return;
            }

            float now = Time.unscaledTime;
            if (runFrames.Length > 0 && now >= nextFrameTime)
            {
                nextFrameTime = now + 1f / RunFrameRate;
                for (int i = 0; i < runFrames.Length; i++)
                {
                    Sprite frame = runFrames[currentFrame % runFrames.Length];
                    currentFrame = (currentFrame + 1) % runFrames.Length;
                    if (frame != null)
                    {
                        runnerImage.sprite = frame;
                        runnerImage.enabled = true;
                        break;
                    }
                }
            }

            float phase = (now - visibleSince) * 12f;
            runnerRect.anchoredPosition = runnerBasePosition + Vector2.up * Mathf.Abs(Mathf.Sin(phase)) * 5f;
            runnerRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(phase * 0.5f) * 1.2f);
        }

        private void AnimateLoadingLabel()
        {
            int dots = 1 + Mathf.FloorToInt((Time.unscaledTime - visibleSince) * 2.5f) % 3;
            loadingText.text = "LOADING" + new string('.', dots);
        }

        private void AnimateProgressBar()
        {
            float pulse = 0.82f + Mathf.Sin(Time.unscaledTime * 8f) * 0.18f;
            Color color = progressLead.color;
            color.a = pulse;
            progressLead.color = color;
        }

        private Sprite FindFirstUsableFrame()
        {
            for (int i = 0; i < runFrames.Length; i++)
            {
                if (runFrames[i] != null)
                {
                    return runFrames[i];
                }
            }

            return null;
        }
    }
}
