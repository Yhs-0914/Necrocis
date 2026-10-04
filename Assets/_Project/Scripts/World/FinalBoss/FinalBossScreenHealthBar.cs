using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Necrocis
{
    /// <summary>Minimal four-stage final-boss gauge. It reveals progress without exact HP.</summary>
    [DisallowMultipleComponent]
    public sealed class FinalBossScreenHealthBar : MonoBehaviour
    {
        private const int SegmentCount = 4;
        // The EXP artwork occupies roughly the lower 160 reference pixels despite its transparent canvas.
        private const float GaugeVerticalCenter = 222f;
        private static readonly Color NormalAccent = new Color(.96f, .07f, .3f, 1f);
        private static readonly Color ReflectionAccent = new Color(.68f, .3f, 1f, 1f);
        private static readonly Color TrailColor = new Color(1f, .55f, .18f, .9f);

        private readonly RectTransform[] segmentFills = new RectTransform[SegmentCount];
        private readonly RectTransform[] segmentTrails = new RectTransform[SegmentCount];
        private readonly Image[] fillImages = new Image[SegmentCount];
        private readonly Image[] nodeImages = new Image[SegmentCount + 1];
        private readonly Text[] sealLabels = new Text[SegmentCount];
        private readonly float[] sealValues = { 1f, 1f, 1f, 1f };
        private IReadOnlyList<FinalBossPillar> seals;

        private CanvasGroup canvasGroup;
        private Image neuralLine;
        private Image hitFlash;
        private Text stateText;
        private Text hintText;
        private RectTransform panelRect;
        private float stateDuration;
        private float stateElapsed;
        private float targetValue = 1f;
        private float displayedValue = 1f;
        private float trailingValue = 1f;
        private float revealTimer;
        private float flashStrength;
        private bool reflecting;
        private bool objectiveVisible;

        public static FinalBossScreenHealthBar Create(
            Transform parent,
            float currentHealth,
            float maxHealth)
        {
            GameObject root = new GameObject("FinalBoss_PhaseTwo_StageGauge", typeof(RectTransform));
            root.transform.SetParent(parent, false);

            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 7600;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;

            FinalBossScreenHealthBar gauge = root.AddComponent<FinalBossScreenHealthBar>();
            gauge.canvasGroup = root.AddComponent<CanvasGroup>();
            gauge.canvasGroup.alpha = 0f;
            gauge.canvasGroup.blocksRaycasts = false;
            gauge.canvasGroup.interactable = false;
            gauge.Build();
            gauge.SetValue(maxHealth > 0f ? currentHealth / maxHealth : 0f, currentHealth, maxHealth);
            gauge.revealTimer = 2.2f;
            return gauge;
        }

        private void Build()
        {
            Font font = GameUiTheme.LoadFont();
            Image panel = RuntimeUiFactory.CreateImage(
                "StageGaugePanel",
                transform,
                new Color(.025f, .005f, .022f, .72f));
            panelRect = panel.rectTransform;
            RuntimeUiFactory.SetRect(
                panel.rectTransform,
                new Vector2(.5f, 0f),
                new Vector2(650f, 70f),
                new Vector2(0f, 205f));

            Shadow shadow = panel.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, .8f);
            shadow.effectDistance = new Vector2(0f, -5f);

            stateText = RuntimeUiFactory.CreateText(
                "StageLabel",
                panel.transform,
                "CEREBRUM  //  PHASE II",
                font,
                16,
                new Color(1f, .78f, .88f, .95f));
            stateText.fontStyle = FontStyle.Bold;
            stateText.alignment = TextAnchor.MiddleCenter;
            RuntimeUiFactory.SetRect(
                stateText.rectTransform,
                new Vector2(.5f, .5f),
                new Vector2(420f, 24f),
                new Vector2(0f, 20f));

            neuralLine = RuntimeUiFactory.CreateImage(
                "NeuralConnection",
                panel.transform,
                new Color(NormalAccent.r, NormalAccent.g, NormalAccent.b, .48f));
            RuntimeUiFactory.SetRect(
                neuralLine.rectTransform,
                new Vector2(.5f, .5f),
                new Vector2(570f, 3f),
                new Vector2(0f, -13f));

            const float segmentWidth = 130f;
            const float gap = 12f;
            float startX = -((segmentWidth + gap) * (SegmentCount - 1)) * .5f;
            for (int i = 0; i < SegmentCount; i++)
            {
                float x = startX + i * (segmentWidth + gap);
                BuildSegment(panel.transform, i, new Vector2(x, -13f), segmentWidth);
                sealLabels[i] = RuntimeUiFactory.CreateText("SealName", panel.transform, "", font,
                    11, new Color(.85f, .86f, .92f));
                sealLabels[i].alignment = TextAnchor.MiddleCenter;
                RuntimeUiFactory.SetRect(sealLabels[i].rectTransform, new Vector2(.5f, .5f),
                    new Vector2(segmentWidth, 16f), new Vector2(x, 3f));
            }

            hintText = RuntimeUiFactory.CreateText("PatternHint", panel.transform, "", font,
                13, new Color(.78f, .8f, .88f));
            hintText.alignment = TextAnchor.MiddleCenter;
            RuntimeUiFactory.SetRect(hintText.rectTransform, new Vector2(.5f, .5f),
                new Vector2(590f, 20f), new Vector2(0f, -34f));
            hintText.gameObject.SetActive(false);

            for (int i = 0; i <= SegmentCount; i++)
            {
                float x = startX - segmentWidth * .5f - gap * .5f + i * (segmentWidth + gap);
                nodeImages[i] = CreateNode(panel.transform, new Vector2(x, -13f));
            }

            hitFlash = RuntimeUiFactory.CreateImage(
                "GaugeHitFlash",
                panel.transform,
                new Color(1f, .75f, .86f, 0f));
            RuntimeUiFactory.Stretch(hitFlash.rectTransform);

            UpdateSegmentVisuals(1f, 1f);
            ApplyAccent(NormalAccent);
        }

        private void BuildSegment(Transform parent, int index, Vector2 position, float width)
        {
            Image frame = RuntimeUiFactory.CreateImage(
                $"Stage_{index + 1}_Frame",
                parent,
                new Color(.2f, .035f, .13f, .96f));
            RuntimeUiFactory.SetRect(frame.rectTransform, new Vector2(.5f, .5f),
                new Vector2(width, 21f), position);

            Image background = RuntimeUiFactory.CreateImage(
                "Background",
                frame.transform,
                new Color(.075f, .012f, .06f, 1f));
            RuntimeUiFactory.Stretch(background.rectTransform, new Vector2(3f, 3f), new Vector2(-3f, -3f));

            Image trail = RuntimeUiFactory.CreateImage("DamageTrail", background.transform, TrailColor);
            RuntimeUiFactory.Stretch(trail.rectTransform);
            segmentTrails[index] = trail.rectTransform;

            Image fill = RuntimeUiFactory.CreateImage("Vitality", background.transform, NormalAccent);
            RuntimeUiFactory.Stretch(fill.rectTransform);
            segmentFills[index] = fill.rectTransform;
            fillImages[index] = fill;

            Image gloss = RuntimeUiFactory.CreateImage(
                "NeuralGlow",
                fill.transform,
                new Color(1f, .72f, .86f, .22f));
            gloss.rectTransform.anchorMin = new Vector2(0f, .58f);
            gloss.rectTransform.anchorMax = Vector2.one;
            gloss.rectTransform.offsetMin = Vector2.zero;
            gloss.rectTransform.offsetMax = Vector2.zero;
        }

        private static Image CreateNode(Transform parent, Vector2 position)
        {
            Image node = RuntimeUiFactory.CreateImage("SynapseNode", parent, NormalAccent);
            RuntimeUiFactory.SetRect(node.rectTransform, new Vector2(.5f, .5f),
                new Vector2(13f, 13f), position);
            node.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            return node;
        }

        public void SetValue(float normalized, float currentHealth, float maxHealth)
        {
            float next = Mathf.Clamp01(normalized);
            if (next < targetValue - .0001f)
            {
                flashStrength = 1f;
                revealTimer = 1.8f;
            }
            targetValue = next;
        }

        public void SetPattern(string label, Color accent, string hint = "", float duration = 0f)
        {
            if (hintText != null && !objectiveVisible)
            {
                RuntimeUiFactory.SetRect(panelRect, new Vector2(.5f, 0f),
                    new Vector2(650f, 104f), new Vector2(0f, GaugeVerticalCenter));
                stateText.rectTransform.anchoredPosition = new Vector2(0f, 31f);
                hintText.gameObject.SetActive(!string.IsNullOrEmpty(hint));
                hintText.text = hint;
            }
            stateDuration = Mathf.Max(0f, duration);
            stateElapsed = 0f;
            reflecting = label == "REFLECTION ACTIVE";
            ApplyAccent(reflecting ? ReflectionAccent : accent);
            if (stateText != null)
            {
                stateText.text = reflecting
                    ? "CEREBRUM  //  REFLECTION ACTIVE"
                    : "CEREBRUM  //  " + label;
                stateText.color = reflecting
                    ? new Color(.86f, .68f, 1f, 1f)
                    : new Color(1f, .78f, .88f, .95f);
            }
            revealTimer = 2f;
        }

        public void BindSeals(IReadOnlyList<FinalBossPillar> pillars)
        {
            seals = pillars;
            objectiveVisible = true;
            RuntimeUiFactory.SetRect(panelRect, new Vector2(.5f, 0f),
                new Vector2(650f, 104f), new Vector2(0f, GaugeVerticalCenter));
            stateText.rectTransform.anchoredPosition = new Vector2(0f, 31f);
            if (hintText != null)
            {
                hintText.gameObject.SetActive(true);
                hintText.text = "DESTROY THE SEALS TO AWAKEN THE CEREBRUM";
            }
            for (int i = 0; i < SegmentCount; i++)
            {
                FinalBossPillar pillar = seals != null && i < seals.Count ? seals[i] : null;
                sealLabels[i].text = pillar != null ? pillar.Biome.ToString().ToUpperInvariant() : "";
            }
        }

        public void SetSealProgress(int destroyed, int total)
        {
            objectiveVisible = true;
            SetValue(total > 0 ? (total - destroyed) / (float)total : 0f, 0f, 0f);
            ApplyAccent(new Color(.28f, .86f, 1f));
            if (stateText != null)
                stateText.text = $"PHASE I  //  BREAK SEALS  {destroyed}/{total}";
        }

        public void Hide()
        {
            if (gameObject != null) gameObject.SetActive(false);
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            stateElapsed += delta;
            revealTimer = Mathf.Max(0f, revealTimer - delta);
            float desiredAlpha = objectiveVisible || reflecting || stateElapsed < stateDuration || revealTimer > 0f ? 1f : .62f;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, desiredAlpha, delta * 3.8f);

            displayedValue = Mathf.MoveTowards(displayedValue, targetValue, delta * 2.4f);
            float trailSpeed = trailingValue > displayedValue ? .22f : 2.4f;
            trailingValue = Mathf.MoveTowards(trailingValue, targetValue, delta * trailSpeed);
            trailingValue = Mathf.Max(trailingValue, displayedValue);
            if (seals != null) UpdateSealVisuals(delta);
            else UpdateSegmentVisuals(displayedValue, trailingValue);

            flashStrength = Mathf.MoveTowards(flashStrength, 0f, delta * 3.5f);
            if (hitFlash != null)
                hitFlash.color = new Color(1f, .72f, .86f, flashStrength * .36f);
        }

        private void UpdateSealVisuals(float delta)
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                FinalBossPillar pillar = i < seals.Count ? seals[i] : null;
                bool intact = pillar != null && !pillar.IsDestroyed;
                float value = intact && pillar.MaxHealth > 0f ? pillar.Health / pillar.MaxHealth : 0f;
                sealValues[i] = Mathf.MoveTowards(sealValues[i], value, delta * 1.8f);
                ApplyFill(segmentFills[i], sealValues[i]);
                ApplyFill(segmentTrails[i], 0f);
                Color accent = pillar != null ? pillar.AccentColor : NormalAccent;
                fillImages[i].color = accent;
                nodeImages[i].color = intact ? accent : new Color(.18f, .13f, .2f);
                sealLabels[i].color = intact ? Color.Lerp(accent, Color.white, .55f) : new Color(.4f, .35f, .43f);
            }
        }

        private void UpdateSegmentVisuals(float fillValue, float trailValue)
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                ApplyFill(segmentFills[i], Mathf.Clamp01(fillValue * SegmentCount - i));
                ApplyFill(segmentTrails[i], Mathf.Clamp01(trailValue * SegmentCount - i));
            }
        }

        private void ApplyAccent(Color accent)
        {
            for (int i = 0; i < fillImages.Length; i++)
                if (fillImages[i] != null) fillImages[i].color = accent;
            for (int i = 0; i < nodeImages.Length; i++)
                if (nodeImages[i] != null) nodeImages[i].color = accent;
            if (neuralLine != null)
                neuralLine.color = new Color(accent.r, accent.g, accent.b, .48f);
        }

        private static void ApplyFill(RectTransform rect, float normalized)
        {
            if (rect == null) return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
