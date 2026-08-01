using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Necrocis
{
    /// <summary>
    /// Freezes gameplay and presents the current biome boss as a high-contrast
    /// full-screen poster. The presentation is created at runtime so every biome
    /// automatically uses the sprite that its live boss is currently displaying.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BossIntroPresentation : MonoBehaviour
    {
        private const float TotalDuration = 2.75f;
        private const float EnterDuration = 0.48f;
        private const float ExitStartTime = 2.18f;
        private static readonly BossIntroVfxOption ActiveVfxOption = BossIntroVfxOption.Option1SpriteSpeedlines;
        private const string Option1UpperSpritePath = "VFX/BossIntro/Option1/BossIntroSpeedlinesUpperLeft";
        private const string Option1LowerSpritePath = "VFX/BossIntro/Option1/BossIntroSpeedlinesLower";

        private static BossIntroPresentation current;
        private static Sprite grayPanelSprite;

        private readonly List<RectTransform> portraitRects = new List<RectTransform>();
        private readonly List<SpeedLineState> speedLines = new List<SpeedLineState>();
        private readonly List<Option1SpriteState> option1Sprites = new List<Option1SpriteState>();

        private UnityEngine.Object owner;
        private Action completion;
        private CanvasGroup canvasGroup;
        private RectTransform cardRect;
        private RectTransform titleRect;
        private RectTransform nameRect;
        private Image flashImage;
        private float elapsed;
        private float previousTimeScale;
        private bool ownsPause;
        private bool finished;

        public static bool Show(
            UnityEngine.Object owner,
            BiomeType biomeType,
            IReadOnlyList<SpriteRenderer> bossRenderers,
            Action onComplete)
        {
            if (bossRenderers == null || bossRenderers.Count == 0)
            {
                return false;
            }

            List<PortraitData> portraits = new List<PortraitData>(bossRenderers.Count);
            for (int i = 0; i < bossRenderers.Count; i++)
            {
                SpriteRenderer renderer = bossRenderers[i];
                if (renderer == null || renderer.sprite == null)
                {
                    continue;
                }

                portraits.Add(new PortraitData(renderer.sprite, renderer.flipX));
            }

            if (portraits.Count == 0)
            {
                return false;
            }

            if (current != null)
            {
                current.CancelInternal();
            }

            GameObject presentationObject = new GameObject(
                "BossIntroPresentation",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup),
                typeof(BossIntroPresentation));

            BossIntroPresentation presentation = presentationObject.GetComponent<BossIntroPresentation>();
            current = presentation;
            presentation.Build(owner, biomeType, portraits, onComplete);
            return true;
        }

        public static void Cancel(UnityEngine.Object owner)
        {
            if (current != null && current.owner == owner)
            {
                current.CancelInternal();
            }
        }

        private void Build(
            UnityEngine.Object presentationOwner,
            BiomeType biomeType,
            IReadOnlyList<PortraitData> portraits,
            Action onComplete)
        {
            owner = presentationOwner;
            completion = onComplete;

            Canvas canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue - 4;

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
            raycaster.enabled = false;

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;

            Palette palette = GetPalette(biomeType);
            RectTransform root = (RectTransform)transform;

            // Keep the live game camera visible. Only a very light neutral wash is
            // placed above it so the white brush work remains readable.
            CreateStretchImage("GameplayDim", root, new Color(0.015f, 0.015f, 0.018f, 0.10f));

            if (ActiveVfxOption == BossIntroVfxOption.Option3RedPoster)
            {
                CreateOption3RedPoster(root, biomeType);
            }
            else
            {
                if (ActiveVfxOption == BossIntroVfxOption.Option1SpriteSpeedlines)
                {
                    CreateGrayPosterField(root, true);
                    CreateOption1SpeedlineSprites(root, palette);
                }
                else
                {
                    CreateGrayPosterField(root, false);
                    CreateOption2BrushStreaks(root, palette);
                }
            }

            cardRect = CreateRect("BossCard", root, Vector2.zero, new Vector2(1160f, 590f));
            cardRect.localRotation = Quaternion.Euler(0f, 0f, -2.2f);

            RectTransform portraitStage = CreateRect("PortraitStage", cardRect, new Vector2(0f, -3f), new Vector2(940f, 480f));
            CreatePortraits(portraitStage, portraits, palette);

            Font font = Resources.Load<Font>("PFStardust");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            titleRect = CreateRect("BossTitle", root, new Vector2(0f, 384f), new Vector2(960f, 170f));
            Text title = CreateText(titleRect, "BOSS", font, 126, new Color(0.58f, 0.005f, 0.055f, 1f));
            title.fontStyle = FontStyle.Bold;
            AddTextOutline(title.gameObject, new Color(0.075f, 0.008f, 0.014f, 0.98f), new Vector2(3f, -3f));
            AddTextShadow(title.gameObject, new Color(0.015f, 0.008f, 0.01f, 0.94f), new Vector2(8f, -8f));

            nameRect = CreateRect("BossName", root, new Vector2(0f, -373f), new Vector2(1300f, 110f));
            Text bossName = CreateText(nameRect, ResolveBossTitle(biomeType), font, 54, palette.light);
            bossName.fontStyle = FontStyle.Bold;
            AddTextOutline(bossName.gameObject, new Color(0.02f, 0.02f, 0.025f, 0.95f), new Vector2(5f, -5f));

            flashImage = CreateStretchImage("ImpactFlash", root, Color.clear);
            flashImage.transform.SetAsLastSibling();

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            ownsPause = true;

            ApplyAnimation(0f);
        }

        private void Update()
        {
            if (finished)
            {
                return;
            }

            elapsed += Time.unscaledDeltaTime;
            ApplyAnimation(elapsed);

            if (elapsed >= TotalDuration)
            {
                Complete();
            }
        }

        private void ApplyAnimation(float time)
        {
            float enter = Smooth01(time / EnterDuration);
            float exit = Smooth01((time - ExitStartTime) / (TotalDuration - ExitStartTime));

            canvasGroup.alpha = Mathf.Clamp01(enter * 1.45f) * (1f - exit);

            cardRect.anchoredPosition = new Vector2(
                Mathf.Lerp(1360f, 0f, enter) + Mathf.Lerp(0f, -1460f, exit),
                Mathf.Lerp(-45f, 0f, enter));
            cardRect.localScale = Vector3.one * Mathf.Lerp(0.82f, 1f, enter);
            cardRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-6.5f, -2.2f, enter));

            titleRect.anchoredPosition = new Vector2(
                Mathf.Lerp(-1280f, 0f, enter) + Mathf.Lerp(0f, 1420f, exit),
                384f);
            nameRect.anchoredPosition = new Vector2(
                Mathf.Lerp(1180f, 0f, enter) + Mathf.Lerp(0f, -1260f, exit),
                -373f);

            float portraitPulse = 1f + Mathf.Sin(Mathf.Max(0f, time - EnterDuration) * 5.2f) * 0.012f;
            for (int i = 0; i < portraitRects.Count; i++)
            {
                RectTransform portrait = portraitRects[i];
                if (portrait == null)
                {
                    continue;
                }

                float mirrored = Mathf.Sign(portrait.localScale.x);
                portrait.localScale = new Vector3(mirrored * portraitPulse, portraitPulse, 1f);
            }

            for (int i = 0; i < speedLines.Count; i++)
            {
                SpeedLineState line = speedLines[i];
                float lineEnter = Smooth01((time - 0.045f - line.delay) / line.revealDuration);
                float shimmer = Mathf.Sin(time * 9.5f + line.phase);
                float pulse = 0.94f + shimmer * 0.06f;
                for (int layerIndex = 0; layerIndex < line.layers.Length; layerIndex++)
                {
                    float layerOpacity = layerIndex == 0 ? pulse : 1f;
                    line.layers[layerIndex].SetReveal(lineEnter, layerOpacity);
                }
            }

            for (int i = 0; i < option1Sprites.Count; i++)
            {
                Option1SpriteState spriteState = option1Sprites[i];
                float spriteEnter = Smooth01((time - spriteState.delay) / 0.34f);
                Color color = spriteState.image.color;
                color.a = spriteState.baseAlpha * spriteEnter;
                spriteState.image.color = color;
                spriteState.rect.anchoredPosition = Vector2.Lerp(
                    spriteState.enterOffset,
                    Vector2.zero,
                    spriteEnter);
                spriteState.rect.localScale = Vector3.one * Mathf.Lerp(1.035f, 1f, spriteEnter);
            }

            float enterFlash = Mathf.Clamp01(1f - Mathf.Abs(time - 0.43f) / 0.11f) * 0.42f;
            float exitFlash = Mathf.Clamp01(1f - Mathf.Abs(time - ExitStartTime) / 0.10f) * 0.24f;
            Color flashColor = Color.white;
            flashColor.a = Mathf.Max(enterFlash, exitFlash);
            flashImage.color = flashColor;
        }

        private void Complete()
        {
            if (finished)
            {
                return;
            }

            finished = true;
            Action callback = completion;
            completion = null;
            RestoreTimeScale();

            if (current == this)
            {
                current = null;
            }

            Destroy(gameObject);
            callback?.Invoke();
        }

        private void CancelInternal()
        {
            if (finished)
            {
                return;
            }

            finished = true;
            completion = null;
            RestoreTimeScale();
            if (current == this)
            {
                current = null;
            }

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            RestoreTimeScale();

            if (current == this)
            {
                current = null;
            }
        }

        private void RestoreTimeScale()
        {
            if (!ownsPause)
            {
                return;
            }

            Time.timeScale = previousTimeScale;
            ownsPause = false;
        }

        private void CreatePortraits(
            RectTransform stage,
            IReadOnlyList<PortraitData> portraits,
            Palette palette)
        {
            int count = Mathf.Min(2, portraits.Count);
            for (int i = 0; i < count; i++)
            {
                PortraitData portrait = portraits[i];
                float x = count == 1 ? 0f : (i == 0 ? -255f : 255f);
                Vector2 size = count == 1 ? new Vector2(1040f, 575f) : new Vector2(620f, 535f);

                RectTransform portraitRect = CreateRect($"BossPortrait{i + 1}", stage, new Vector2(x, 8f), size);
                Image image = portraitRect.gameObject.AddComponent<Image>();
                image.sprite = portrait.sprite;
                image.preserveAspect = true;
                image.raycastTarget = false;
                image.color = Color.white;
                // No custom material or tint: display the exact live Unity sprite.
                image.material = null;
                portraitRect.localScale = new Vector3(portrait.flipX ? -1f : 1f, 1f, 1f);
                portraitRects.Add(portraitRect);
            }
        }

        private static void CreateGrayPosterField(RectTransform root, bool fillEntireScreen)
        {
            Image panel = CreateStretchImage("GrayPosterField", root, new Color(0.43f, 0.44f, 0.45f, 0.96f));
            panel.sprite = fillEntireScreen ? null : GetGrayPanelSprite();
            panel.type = Image.Type.Simple;
        }

        // Saved option 3: the original opaque red poster treatment. This uses
        // the exact biome palette values recovered from the first implementation.
        private static void CreateOption3RedPoster(RectTransform root, BiomeType biomeType)
        {
            Option3Palette palette = GetOption3Palette(biomeType);
            CreateStretchImage("Option3RedBackdrop", root, palette.background);
            CreateStretchImage(
                "Option3RedBackdropShade",
                root,
                new Color(palette.card.r, palette.card.g, palette.card.b, 0.42f),
                Vector2.zero,
                new Vector2(0.36f, 1f));
            CreateStretchImage(
                "Option3TopRule",
                root,
                palette.light,
                new Vector2(0f, 0.967f),
                Vector2.one);
            CreateStretchImage(
                "Option3BottomRule",
                root,
                palette.light,
                Vector2.zero,
                new Vector2(1f, 0.025f));

            for (int i = 0; i < 6; i++)
            {
                float width = 530f - i * 43f;
                RectTransform upper = CreateRect(
                    $"Option3UpperSlash{i + 1}",
                    root,
                    new Vector2(-710f + i * 25f, 405f - i * 18f),
                    new Vector2(width, i % 2 == 0 ? 11f : 5f));
                upper.localRotation = Quaternion.Euler(0f, 0f, -41f + i * 1.4f);
                Image upperImage = upper.gameObject.AddComponent<Image>();
                upperImage.color = i % 3 == 0 ? palette.gray : palette.light;
                upperImage.raycastTarget = false;

                RectTransform lower = CreateRect(
                    $"Option3LowerSlash{i + 1}",
                    root,
                    new Vector2(590f - i * 35f, -425f + i * 17f),
                    new Vector2(width + 180f, i % 2 == 0 ? 9f : 4f));
                lower.localRotation = Quaternion.Euler(0f, 0f, 9f - i * 0.8f);
                Image lowerImage = lower.gameObject.AddComponent<Image>();
                lowerImage.color = i % 3 == 0 ? palette.gray : palette.light;
                lowerImage.raycastTarget = false;
            }
        }

        private static Option3Palette GetOption3Palette(BiomeType biomeType)
        {
            return biomeType switch
            {
                BiomeType.Intestine => new Option3Palette(
                    new Color(0.30f, 0.055f, 0.025f, 1f),
                    new Color(0.085f, 0.045f, 0.025f, 0.96f),
                    new Color(0.94f, 0.91f, 0.76f, 1f),
                    new Color(0.48f, 0.45f, 0.36f, 1f)),
                BiomeType.Liver => new Option3Palette(
                    new Color(0.46f, 0.008f, 0.075f, 1f),
                    new Color(0.105f, 0.012f, 0.04f, 0.96f),
                    new Color(1f, 0.92f, 0.92f, 1f),
                    new Color(0.50f, 0.42f, 0.45f, 1f)),
                BiomeType.Stomach => new Option3Palette(
                    new Color(0.43f, 0.018f, 0.13f, 1f),
                    new Color(0.10f, 0.018f, 0.065f, 0.96f),
                    new Color(1f, 0.94f, 0.88f, 1f),
                    new Color(0.54f, 0.45f, 0.46f, 1f)),
                BiomeType.Lung => new Option3Palette(
                    new Color(0.30f, 0.025f, 0.105f, 1f),
                    new Color(0.035f, 0.045f, 0.07f, 0.96f),
                    new Color(0.93f, 0.97f, 1f, 1f),
                    new Color(0.40f, 0.49f, 0.55f, 1f)),
                _ => new Option3Palette(
                    new Color(0.40f, 0.015f, 0.07f, 1f),
                    new Color(0.07f, 0.02f, 0.035f, 0.96f),
                    new Color(0.96f, 0.96f, 0.94f, 1f),
                    new Color(0.48f, 0.48f, 0.48f, 1f))
            };
        }

        private void CreateOption1SpeedlineSprites(RectTransform root, Palette palette)
        {
            Sprite upperSprite = Resources.Load<Sprite>(Option1UpperSpritePath);
            Sprite lowerSprite = Resources.Load<Sprite>(Option1LowerSpritePath);
            if (upperSprite == null || lowerSprite == null)
            {
                Debug.LogWarning(
                    "[BossIntroPresentation] Option 1 speed-line sprites could not be loaded. "
                    + "Falling back to the saved option-2 brush layout.");
                CreateOption2BrushStreaks(root, palette);
                return;
            }

            AddOption1Sprite(
                "Option1UpperLeftSpeedlines",
                root,
                upperSprite,
                0.88f,
                new Vector2(-135f, 72f),
                0.025f);
            AddOption1Sprite(
                "Option1LowerSpeedlines",
                root,
                lowerSprite,
                0.82f,
                new Vector2(-105f, -48f),
                0.07f);
        }

        private void AddOption1Sprite(
            string name,
            RectTransform root,
            Sprite sprite,
            float baseAlpha,
            Vector2 enterOffset,
            float delay)
        {
            Image image = CreateStretchImage(name, root, new Color(1f, 1f, 1f, 0f));
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.material = null;

            option1Sprites.Add(new Option1SpriteState
            {
                image = image,
                rect = image.rectTransform,
                baseAlpha = baseAlpha,
                enterOffset = enterOffset,
                delay = delay
            });
        }

        // Saved option 2. It remains compiled and can be restored by changing
        // ActiveVfxOption without rebuilding the layout from scratch.
        private void CreateOption2BrushStreaks(RectTransform root, Palette palette)
        {
            RectTransform blurLayer = CreateStretchLayer("SpeedLineBlurLayer", root);
            RectTransform ghostLayer = CreateStretchLayer("SpeedLineGhostLayer", root);
            RectTransform coreLayer = CreateStretchLayer("SpeedLineCoreLayer", root);

            // Option 2: a dense, dry-brush border that follows the diagonal edge
            // of the gray poster instead of radiating across the whole screen.
            // The broad foundation scratches bind the smaller pointed strokes
            // into one intentional white brush band.
            const int upperFoundationCount = 7;
            for (int i = 0; i < upperFoundationCount; i++)
            {
                float offset = (i - (upperFoundationCount - 1) * 0.5f) * 11f;
                Vector2 start = new Vector2(-1050f - i * 9f, 36f + offset);
                Vector2 end = new Vector2(-292f + i * 13f, 558f + offset * 0.42f);
                float width = 6f + (i % 3) * 3.5f;
                Color coreColor = i % 3 == 1
                    ? WithAlpha(palette.gray, 0.62f)
                    : WithAlpha(palette.light, 0.90f);

                CreateRibbonSpeedLine(
                    $"UpperBrushFoundation{i + 1}",
                    blurLayer,
                    ghostLayer,
                    coreLayer,
                    start,
                    end,
                    width,
                    -16f + i * 5f,
                    coreColor,
                    i * 0.012f,
                    0.31f + (i % 3) * 0.025f,
                    i * 0.81f);
            }

            const int upperStrokeCount = 22;
            for (int i = 0; i < upperStrokeCount; i++)
            {
                float normalized = i / (float)(upperStrokeCount - 1);
                Vector2 edge = Vector2.Lerp(
                    new Vector2(-1018f, 58f),
                    new Vector2(-330f, 528f),
                    normalized);
                float jitter = Mathf.Sin(i * 2.17f) * 22f;
                float length = 118f + (i * 47 % 145);
                Vector2 direction = new Vector2(0.68f, 0.74f).normalized;
                Vector2 normal = new Vector2(-direction.y, direction.x);
                Vector2 start = edge - direction * (length * 0.46f) + normal * jitter;
                Vector2 end = edge + direction * (length * 0.54f) + normal * (jitter * 0.18f);
                float width = i % 5 == 0 ? 33f : 10f + (i * 9 % 17);
                Color coreColor = i % 4 == 2
                    ? WithAlpha(palette.gray, 0.70f)
                    : WithAlpha(palette.light, 0.98f - normalized * 0.08f);

                CreateRibbonSpeedLine(
                    $"UpperBrushStroke{i + 1}",
                    blurLayer,
                    ghostLayer,
                    coreLayer,
                    start,
                    end,
                    width,
                    -18f + (i % 7) * 6f,
                    coreColor,
                    0.025f + i * 0.008f,
                    0.22f + (i % 5) * 0.024f,
                    1.3f + i * 0.67f);
            }

            // Long lower sweep. These lines sit behind the boss name and health
            // bar, so the white brush mass frames the UI without erasing it.
            const int lowerFoundationCount = 8;
            for (int i = 0; i < lowerFoundationCount; i++)
            {
                float offset = (i - (lowerFoundationCount - 1) * 0.5f) * 13f;
                Vector2 start = new Vector2(-1085f - i * 13f, -505f + offset);
                Vector2 end = new Vector2(1040f + i * 22f, -182f + offset * 0.36f);
                Color coreColor = i % 3 == 1
                    ? WithAlpha(palette.gray, 0.64f)
                    : WithAlpha(palette.light, 0.92f);

                CreateRibbonSpeedLine(
                    $"LowerBrushFoundation{i + 1}",
                    blurLayer,
                    ghostLayer,
                    coreLayer,
                    start,
                    end,
                    7f + (i % 4) * 2.8f,
                    34f - i * 7f,
                    coreColor,
                    0.035f + i * 0.011f,
                    0.34f + (i % 3) * 0.025f,
                    3.2f + i * 0.72f);
            }

            const int lowerStrokeCount = 27;
            for (int i = 0; i < lowerStrokeCount; i++)
            {
                float normalized = i / (float)(lowerStrokeCount - 1);
                Vector2 edge = Vector2.Lerp(
                    new Vector2(-1035f, -506f),
                    new Vector2(895f, -205f),
                    normalized);
                float jitter = Mathf.Sin(i * 1.73f) * 27f;
                float length = 185f + (i * 83 % 270);
                Vector2 direction = new Vector2(0.985f, 0.174f).normalized;
                Vector2 normal = new Vector2(-direction.y, direction.x);
                Vector2 start = edge - direction * (length * 0.56f) + normal * jitter;
                Vector2 end = edge + direction * (length * 0.44f) + normal * (jitter * 0.12f);
                float width = i % 6 == 0 ? 38f : 9f + (i * 11 % 21);
                Color coreColor = i % 4 == 1
                    ? WithAlpha(palette.gray, 0.68f)
                    : WithAlpha(palette.light, 0.97f - normalized * 0.07f);

                CreateRibbonSpeedLine(
                    $"LowerBrushStroke{i + 1}",
                    blurLayer,
                    ghostLayer,
                    coreLayer,
                    start,
                    end,
                    width,
                    -23f + (i % 9) * 6f,
                    coreColor,
                    0.055f + i * 0.0065f,
                    0.23f + (i % 6) * 0.021f,
                    5.1f + i * 0.59f);
            }
        }

        private void CreateRibbonSpeedLine(
            string name,
            Transform blurLayer,
            Transform ghostLayer,
            Transform coreLayer,
            Vector2 start,
            Vector2 end,
            float startWidth,
            float curvature,
            Color coreColor,
            float delay,
            float revealDuration,
            float phase)
        {
            Vector2 direction = (end - start).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x);
            Vector2 control = Vector2.Lerp(start, end, 0.53f) + normal * curvature;

            BossIntroSpeedRibbonGraphic aura = CreateRibbonGraphic(
                $"{name}_Aura",
                blurLayer,
                start - direction * 34f,
                control + normal * 3f,
                end + direction * 58f,
                startWidth * 3.55f,
                WithAlpha(coreColor, coreColor.a * 0.038f),
                20);

            BossIntroSpeedRibbonGraphic blur = CreateRibbonGraphic(
                $"{name}_Blur",
                blurLayer,
                start - direction * 24f,
                control + normal * 2f,
                end + direction * 34f,
                startWidth * 2.45f,
                WithAlpha(coreColor, coreColor.a * 0.105f),
                20);

            BossIntroSpeedRibbonGraphic ghost = CreateRibbonGraphic(
                $"{name}_Ghost",
                ghostLayer,
                start - direction * 11f + normal * (startWidth * 0.72f + 2f),
                control + normal * (startWidth * 0.40f + 1f),
                end - direction * 26f + normal * 1.5f,
                Mathf.Max(2.4f, startWidth * 0.48f),
                WithAlpha(Color.gray, coreColor.a * 0.31f),
                16);

            BossIntroSpeedRibbonGraphic core = CreateRibbonGraphic(
                $"{name}_Core",
                coreLayer,
                start,
                control,
                end,
                startWidth,
                coreColor,
                18);

            speedLines.Add(new SpeedLineState
            {
                layers = new[] { aura, blur, ghost, core },
                delay = delay,
                revealDuration = revealDuration,
                phase = phase
            });
        }

        private static RectTransform CreateStretchLayer(string name, Transform parent)
        {
            GameObject layerObject = new GameObject(name, typeof(RectTransform));
            RectTransform layer = layerObject.GetComponent<RectTransform>();
            layer.SetParent(parent, false);
            layer.anchorMin = Vector2.zero;
            layer.anchorMax = Vector2.one;
            layer.offsetMin = Vector2.zero;
            layer.offsetMax = Vector2.zero;
            return layer;
        }

        private static BossIntroSpeedRibbonGraphic CreateRibbonGraphic(
            string name,
            Transform parent,
            Vector2 start,
            Vector2 control,
            Vector2 end,
            float startWidth,
            Color color,
            int segments)
        {
            GameObject ribbonObject = new GameObject(name, typeof(RectTransform), typeof(BossIntroSpeedRibbonGraphic));
            RectTransform rect = ribbonObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            BossIntroSpeedRibbonGraphic graphic = ribbonObject.GetComponent<BossIntroSpeedRibbonGraphic>();
            graphic.raycastTarget = false;
            graphic.Configure(start, control, end, startWidth, color, segments);
            return graphic;
        }

        private static Sprite GetGrayPanelSprite()
        {
            if (grayPanelSprite != null)
            {
                return grayPanelSprite;
            }

            const int width = 512;
            const int height = 288;
            Vector2[] outline =
            {
                new Vector2(198f, 287f),
                new Vector2(452f, 287f),
                new Vector2(480f, 282f),
                new Vector2(500f, 266f),
                new Vector2(510f, 241f),
                new Vector2(511f, 116f),
                new Vector2(506f, 94f),
                new Vector2(494f, 79f),
                new Vector2(472f, 69f),
                new Vector2(425f, 62f),
                new Vector2(45f, 9f),
                new Vector2(22f, 11f),
                new Vector2(7f, 24f),
                new Vector2(1f, 43f),
                new Vector2(1f, 137f),
                new Vector2(14f, 158f)
            };

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "BossIntroGrayPanelTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color32[] pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                    if (!IsPointInsidePolygon(point, outline))
                    {
                        pixels[y * width + x] = new Color32(255, 255, 255, 0);
                        continue;
                    }

                    float edgeDistance = DistanceToPolygon(point, outline);
                    uint hash = (uint)x * 374761393u + (uint)y * 668265263u;
                    hash = (hash ^ (hash >> 13)) * 1274126177u;
                    float noise = (hash & 1023u) / 1023f;
                    float edgeAlpha = Mathf.Clamp01((edgeDistance + noise * 1.4f) / 2.8f);
                    byte alpha = (byte)Mathf.RoundToInt(edgeAlpha * 255f);
                    pixels[y * width + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            grayPanelSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            grayPanelSprite.name = "BossIntroGrayPanelSprite";
            grayPanelSprite.hideFlags = HideFlags.HideAndDontSave;
            return grayPanelSprite;
        }

        private static bool IsPointInsidePolygon(Vector2 point, IReadOnlyList<Vector2> polygon)
        {
            bool inside = false;
            for (int i = 0, previous = polygon.Count - 1; i < polygon.Count; previous = i++)
            {
                Vector2 currentPoint = polygon[i];
                Vector2 previousPoint = polygon[previous];
                bool crosses = (currentPoint.y > point.y) != (previousPoint.y > point.y)
                    && point.x < (previousPoint.x - currentPoint.x)
                    * (point.y - currentPoint.y)
                    / (previousPoint.y - currentPoint.y)
                    + currentPoint.x;
                if (crosses)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private static float DistanceToPolygon(Vector2 point, IReadOnlyList<Vector2> polygon)
        {
            float closest = float.MaxValue;
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 start = polygon[i];
                Vector2 end = polygon[(i + 1) % polygon.Count];
                Vector2 segment = end - start;
                float lengthSquared = segment.sqrMagnitude;
                float t = lengthSquared > Mathf.Epsilon
                    ? Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared)
                    : 0f;
                closest = Mathf.Min(closest, Vector2.Distance(point, start + segment * t));
            }

            return closest;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            GameObject child = new GameObject(name, typeof(RectTransform));
            RectTransform rect = child.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static Image CreateStretchImage(
            string name,
            Transform parent,
            Color color,
            Vector2? anchorMin = null,
            Vector2? anchorMax = null)
        {
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = child.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin ?? Vector2.zero;
            rect.anchorMax = anchorMax ?? Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = child.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(RectTransform rect, string value, Font font, int size, Color color)
        {
            Text text = rect.gameObject.AddComponent<Text>();
            text.text = value;
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static void AddTextOutline(GameObject target, Color color, Vector2 distance)
        {
            Outline outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        private static void AddTextShadow(GameObject target, Color color, Vector2 distance)
        {
            Shadow shadow = target.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static string ResolveBossTitle(BiomeType biomeType)
        {
            return biomeType switch
            {
                BiomeType.Intestine => "INTESTINE OVERLORD",
                BiomeType.Liver => "LIVER WARDEN",
                BiomeType.Stomach => "THE DEVOURING STOMACH",
                BiomeType.Lung => "TWIN LUNGS",
                _ => "UNKNOWN ABOMINATION"
            };
        }

        private static Palette GetPalette(BiomeType biomeType)
        {
            return biomeType switch
            {
                BiomeType.Intestine => new Palette(
                    new Color(0.98f, 0.97f, 0.91f, 0.94f),
                    new Color(0.66f, 0.66f, 0.61f, 0.76f),
                    new Color(0.045f, 0.045f, 0.04f, 1f)),
                BiomeType.Liver => new Palette(
                    new Color(1f, 0.97f, 0.97f, 0.95f),
                    new Color(0.69f, 0.65f, 0.66f, 0.78f),
                    new Color(0.05f, 0.04f, 0.045f, 1f)),
                BiomeType.Stomach => new Palette(
                    new Color(1f, 0.97f, 0.94f, 0.95f),
                    new Color(0.69f, 0.66f, 0.64f, 0.78f),
                    new Color(0.05f, 0.043f, 0.04f, 1f)),
                BiomeType.Lung => new Palette(
                    new Color(0.95f, 0.98f, 1f, 0.95f),
                    new Color(0.62f, 0.68f, 0.72f, 0.78f),
                    new Color(0.035f, 0.045f, 0.055f, 1f)),
                _ => new Palette(
                    new Color(0.96f, 0.96f, 0.94f, 1f),
                    new Color(0.62f, 0.62f, 0.61f, 0.78f),
                    new Color(0.04f, 0.04f, 0.045f, 1f))
            };
        }

        private static float Smooth01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private readonly struct PortraitData
        {
            public readonly Sprite sprite;
            public readonly bool flipX;

            public PortraitData(Sprite sprite, bool flipX)
            {
                this.sprite = sprite;
                this.flipX = flipX;
            }
        }

        private sealed class SpeedLineState
        {
            public BossIntroSpeedRibbonGraphic[] layers;
            public float delay;
            public float revealDuration;
            public float phase;
        }

        private sealed class Option1SpriteState
        {
            public Image image;
            public RectTransform rect;
            public float baseAlpha;
            public Vector2 enterOffset;
            public float delay;
        }

        private enum BossIntroVfxOption
        {
            Option1SpriteSpeedlines,
            Option2ProceduralBrush,
            Option3RedPoster
        }

        private readonly struct Option3Palette
        {
            public readonly Color background;
            public readonly Color card;
            public readonly Color light;
            public readonly Color gray;

            public Option3Palette(Color background, Color card, Color light, Color gray)
            {
                this.background = background;
                this.card = card;
                this.light = light;
                this.gray = gray;
            }
        }

        private readonly struct Palette
        {
            public readonly Color light;
            public readonly Color gray;
            public readonly Color portraitShadow;

            public Palette(Color light, Color gray, Color portraitShadow)
            {
                this.light = light;
                this.gray = gray;
                this.portraitShadow = portraitShadow;
            }
        }
    }

    /// <summary>
    /// Draws one tapered dry-brush ribbon along a quadratic Bezier curve.
    /// Its asymmetric serrated edges and small bristle triangles keep the result
    /// visibly different from the previous smooth speed-line treatment.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class BossIntroSpeedRibbonGraphic : MaskableGraphic
    {
        private Vector2 start;
        private Vector2 control;
        private Vector2 end;
        private Color ribbonColor = Color.white;
        private float startWidth = 8f;
        private float reveal;
        private float opacity = 1f;
        private int segmentCount = 16;

        public void Configure(
            Vector2 startPoint,
            Vector2 controlPoint,
            Vector2 endPoint,
            float width,
            Color color,
            int segments)
        {
            start = startPoint;
            control = controlPoint;
            end = endPoint;
            startWidth = Mathf.Max(0.5f, width);
            ribbonColor = color;
            segmentCount = Mathf.Clamp(segments, 6, 32);
            reveal = 0f;
            SetVerticesDirty();
        }

        public void SetReveal(float value, float alphaMultiplier)
        {
            value = Mathf.Clamp01(value);
            alphaMultiplier = Mathf.Clamp01(alphaMultiplier);
            if (Mathf.Abs(reveal - value) < 0.0005f && Mathf.Abs(opacity - alphaMultiplier) < 0.0005f)
            {
                return;
            }

            reveal = value;
            opacity = alphaMultiplier;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            if (reveal <= 0.001f || ribbonColor.a <= 0.001f)
            {
                return;
            }

            int visibleSegments = Mathf.Max(2, Mathf.CeilToInt(segmentCount * reveal));
            float noiseSeed = Mathf.Abs(
                start.x * 0.0137f
                + start.y * 0.0191f
                + end.x * 0.0073f
                + end.y * 0.0119f);

            for (int i = 0; i <= visibleSegments; i++)
            {
                float localProgress = i / (float)visibleSegments;
                float curveProgress = reveal * localProgress;
                Vector2 position = EvaluateBezier(curveProgress);
                Vector2 tangent = EvaluateTangent(curveProgress);
                Vector2 normal = tangent.sqrMagnitude > 0.0001f
                    ? new Vector2(-tangent.y, tangent.x).normalized
                    : Vector2.up;

                float globalTaper = Mathf.Pow(1f - curveProgress, 0.92f);
                float sharpTip = Mathf.Pow(1f - localProgress, 0.48f);
                float halfWidth = Mathf.Max(0.08f, startWidth * 0.5f * globalTaper * sharpTip);
                if (i == visibleSegments)
                {
                    halfWidth = 0.06f;
                }

                float upperNoise = 0.70f + Mathf.Abs(Mathf.Sin(noiseSeed + i * 1.93f)) * 0.34f;
                float lowerNoise = 0.68f + Mathf.Abs(Mathf.Sin(noiseSeed * 1.41f + i * 2.47f)) * 0.38f;
                if (i > 0 && i < visibleSegments && (i + Mathf.FloorToInt(noiseSeed)) % 5 == 0)
                {
                    upperNoise *= 0.48f;
                }

                if (i > 0 && i < visibleSegments && (i + Mathf.FloorToInt(noiseSeed * 1.7f)) % 6 == 0)
                {
                    lowerNoise *= 0.42f;
                }

                float centerJitter = Mathf.Sin(noiseSeed * 0.77f + i * 2.11f)
                    * startWidth
                    * 0.055f
                    * globalTaper;
                position += normal * centerJitter;

                float startFade = Mathf.Lerp(0.76f, 1f, Mathf.Clamp01(localProgress / 0.08f));
                float longitudinalFade = Mathf.Lerp(1f, 0.58f, curveProgress);
                Color vertexColor = ribbonColor;
                vertexColor.a *= opacity * startFade * longitudinalFade;

                UIVertex upper = UIVertex.simpleVert;
                upper.position = position + normal * (halfWidth * upperNoise);
                upper.color = vertexColor;
                upper.uv0 = new Vector2(curveProgress, 1f);

                UIVertex lower = UIVertex.simpleVert;
                lower.position = position - normal * (halfWidth * lowerNoise);
                lower.color = vertexColor;
                lower.uv0 = new Vector2(curveProgress, 0f);

                vertexHelper.AddVert(upper);
                vertexHelper.AddVert(lower);

                if (i == 0)
                {
                    continue;
                }

                int vertexIndex = i * 2;
                vertexHelper.AddTriangle(vertexIndex - 2, vertexIndex, vertexIndex - 1);
                vertexHelper.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex - 1);
            }

            // Sparse pointed splinters create the torn, feathered brush silhouette
            // visible in the supplied option-2 reference without using a texture.
            int bristleStep = startWidth >= 18f ? 2 : 4;
            for (int i = bristleStep; i < visibleSegments - 1; i += bristleStep)
            {
                float localProgress = i / (float)visibleSegments;
                float curveProgress = reveal * localProgress;
                Vector2 position = EvaluateBezier(curveProgress);
                Vector2 tangent = EvaluateTangent(curveProgress).normalized;
                Vector2 normal = new Vector2(-tangent.y, tangent.x);
                float taper = Mathf.Pow(1f - curveProgress, 0.92f);
                float side = ((i / bristleStep) & 1) == 0 ? 1f : -1f;
                float edgeWidth = Mathf.Max(0.8f, startWidth * 0.42f * taper);
                float bristleWidth = Mathf.Max(0.45f, startWidth * (0.055f + (i % 3) * 0.018f));
                float bristleLength = (9f + (i * 13 % 29)) * Mathf.Lerp(1f, 0.42f, curveProgress);
                Vector2 baseCenter = position + normal * side * edgeWidth;
                Vector2 tip = baseCenter + tangent * bristleLength + normal * side * bristleWidth * 0.55f;

                Color bristleColor = ribbonColor;
                bristleColor.a *= opacity * 0.52f * Mathf.Lerp(1f, 0.45f, curveProgress);

                int baseIndex = vertexHelper.currentVertCount;
                UIVertex first = UIVertex.simpleVert;
                first.position = baseCenter - normal * bristleWidth;
                first.color = bristleColor;
                first.uv0 = new Vector2(curveProgress, 0f);

                UIVertex second = UIVertex.simpleVert;
                second.position = baseCenter + normal * bristleWidth;
                second.color = bristleColor;
                second.uv0 = new Vector2(curveProgress, 1f);

                UIVertex third = UIVertex.simpleVert;
                third.position = tip;
                third.color = bristleColor;
                third.uv0 = new Vector2(Mathf.Min(1f, curveProgress + 0.08f), 0.5f);

                vertexHelper.AddVert(first);
                vertexHelper.AddVert(second);
                vertexHelper.AddVert(third);
                vertexHelper.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
            }
        }

        private Vector2 EvaluateBezier(float t)
        {
            float inverse = 1f - t;
            return inverse * inverse * start + 2f * inverse * t * control + t * t * end;
        }

        private Vector2 EvaluateTangent(float t)
        {
            return 2f * (1f - t) * (control - start) + 2f * t * (end - control);
        }
    }

}
