using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    internal sealed class BossEncounterVfx : MonoBehaviour
    {
        private const int GroundSortingOrder = 5300;
        private const int DetailSortingOrder = 5330;
        private const int ParticleSortingOrder = 5360;
        private const int DetailCount = 10;
        private const int RayCount = 8;

        private readonly SpriteRenderer[] details = new SpriteRenderer[DetailCount];
        private readonly SpriteRenderer[] groundRays = new SpriteRenderer[RayCount];

        private SpriteRenderer groundMark;
        private SpriteRenderer groundAccent;
        private SpriteRenderer shockwave;
        private SpriteRenderer secondaryShockwave;
        private SpriteRenderer bossEcho;
        private SpriteRenderer bossFlash;
        private ParticleSystem particles;
        private RuntimePoolAutoReturn autoReturn;
        private SpriteRenderer sourceRenderer;
        private Vector3 sourceBaseScale;
        private Vector3 centerOffset;
        private Color primaryColor;
        private Color accentColor;
        private BiomeType currentBiome;
        private float duration;
        private float elapsed;
        private float effectScale;
        private float angleSeed;
        private bool shouldShakeCamera;
        private bool cameraShakePlayed;

        public static GameObject CreateObject()
        {
            GameObject root = new GameObject("BossEncounterFx");
            root.SetActive(false);

            BossEncounterVfx effect = root.AddComponent<BossEncounterVfx>();
            effect.groundMark = CreateSpriteRenderer(
                root.transform,
                "BossGroundMark",
                CombatVfxResources.GetSplatSprite(),
                GroundSortingOrder);
            effect.groundAccent = CreateSpriteRenderer(
                root.transform,
                "BossGroundAccent",
                CombatVfxResources.GetCrackSprite(),
                GroundSortingOrder + 1);
            effect.shockwave = CreateSpriteRenderer(
                root.transform,
                "BossShockwave",
                CombatVfxResources.GetRingSprite(),
                GroundSortingOrder + 2);
            effect.secondaryShockwave = CreateSpriteRenderer(
                root.transform,
                "BossSecondaryShockwave",
                CombatVfxResources.GetRingSprite(),
                GroundSortingOrder + 3);

            for (int i = 0; i < RayCount; i++)
            {
                effect.groundRays[i] = CreateSpriteRenderer(
                    root.transform,
                    $"BossGroundRay{i + 1}",
                    CombatVfxResources.GetSoftCircleSprite(),
                    GroundSortingOrder + 4 + i);
            }

            for (int i = 0; i < DetailCount; i++)
            {
                effect.details[i] = CreateSpriteRenderer(
                    root.transform,
                    $"BossDetail{i + 1}",
                    CombatVfxResources.GetStarSprite(),
                    DetailSortingOrder + i);
            }

            effect.bossEcho = CreateSpriteRenderer(
                root.transform,
                "BossBodyEcho",
                CombatVfxResources.GetSoftCircleSprite(),
                DetailSortingOrder + DetailCount + 2);
            effect.bossFlash = CreateSpriteRenderer(
                root.transform,
                "BossBodyFlash",
                CombatVfxResources.GetSoftCircleSprite(),
                DetailSortingOrder + DetailCount + 3);

            effect.particles = CreateParticleSystem(root.transform);
            effect.autoReturn = RuntimePool.EnsureAutoReturn(root);
            return root;
        }

        public void Show(
            Transform bossTarget,
            Vector3 groundPosition,
            Vector3 center,
            float scale,
            BiomeType biome,
            Color primary,
            Color accent,
            bool addCameraShake)
        {
            EnsureComponents();

            transform.SetParent(null, false);
            transform.position = groundPosition;
            transform.rotation = Quaternion.identity;
            centerOffset = center - groundPosition;
            currentBiome = biome;
            primaryColor = primary;
            accentColor = accent;
            effectScale = Mathf.Clamp(scale * 1.22f, 1.15f, 4.2f);
            duration = biome == BiomeType.Liver ? 2.25f : biome == BiomeType.Lung ? 2.1f : 2.05f;
            elapsed = 0f;
            angleSeed = Random.Range(0f, 360f);
            shouldShakeCamera = addCameraShake;
            cameraShakePlayed = false;
            sourceRenderer = bossTarget != null
                ? bossTarget.GetComponentInChildren<SpriteRenderer>(false)
                : null;
            sourceBaseScale = sourceRenderer != null
                ? sourceRenderer.transform.lossyScale
                : Vector3.one * effectScale;

            ResetRenderers();
            ConfigureBiomeVisuals(biome);
            ConfigureBossEcho();
            EmitBiomeParticles(groundPosition, biome);
            autoReturn.Schedule(duration);
            enabled = true;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.rotation = Quaternion.identity;
            if (shouldShakeCamera && !cameraShakePlayed && t >= 0.22f)
            {
                cameraShakePlayed = true;
                DontStarveCamera.Instance?.AddCombatImpulse(0.34f, 0.42f);
            }
            UpdateBossPresence(t);
            UpdateGroundBurstLayers(t);

            switch (currentBiome)
            {
                case BiomeType.Intestine:
                    UpdateIntestine(t);
                    break;
                case BiomeType.Liver:
                    UpdateLiver(t);
                    break;
                case BiomeType.Stomach:
                    UpdateStomach(t);
                    break;
                case BiomeType.Lung:
                    UpdateLung(t);
                    break;
                default:
                    UpdateFallback(t);
                    break;
            }

            if (t >= 1f)
            {
                RuntimePool.Release(gameObject);
            }
        }

        private void ResetRenderers()
        {
            groundMark.enabled = true;
            groundMark.transform.localPosition = Vector3.up * 0.022f;
            groundMark.transform.localScale = Vector3.one * 0.04f;
            groundMark.color = WithAlpha(primaryColor, 0f);

            groundAccent.enabled = true;
            groundAccent.transform.localPosition = Vector3.up * 0.034f;
            groundAccent.transform.localScale = Vector3.one * 0.04f;
            groundAccent.color = WithAlpha(accentColor, 0f);

            shockwave.enabled = true;
            shockwave.transform.localPosition = Vector3.up * 0.046f;
            shockwave.transform.localScale = Vector3.one * 0.04f;
            shockwave.color = WithAlpha(accentColor, 0f);

            secondaryShockwave.enabled = true;
            secondaryShockwave.transform.localPosition = Vector3.up * 0.052f;
            secondaryShockwave.transform.localScale = Vector3.one * 0.04f;
            secondaryShockwave.transform.rotation = GroundRotation(0f);
            secondaryShockwave.color = WithAlpha(accentColor, 0f);

            for (int i = 0; i < groundRays.Length; i++)
            {
                SpriteRenderer ray = groundRays[i];
                if (ray == null)
                {
                    continue;
                }

                ray.enabled = true;
                ray.transform.localPosition = Vector3.up * (0.056f + i * 0.001f);
                ray.transform.localScale = Vector3.one * 0.02f;
                ray.transform.rotation = GroundRotation(i * (360f / RayCount));
                ray.color = WithAlpha(i % 2 == 0 ? primaryColor : accentColor, 0f);
            }

            bossEcho.enabled = false;
            bossEcho.color = WithAlpha(primaryColor, 0f);
            bossFlash.enabled = false;
            bossFlash.color = WithAlpha(Color.white, 0f);

            for (int i = 0; i < details.Length; i++)
            {
                SpriteRenderer detail = details[i];
                if (detail == null)
                {
                    continue;
                }

                detail.enabled = false;
                detail.transform.localPosition = centerOffset;
                detail.transform.localScale = Vector3.one * 0.02f;
                detail.transform.rotation = GetCameraRotation();
                detail.color = WithAlpha(primaryColor, 0f);
            }
        }

        private void ConfigureBossEcho()
        {
            if (sourceRenderer == null || sourceRenderer.sprite == null)
            {
                return;
            }

            bossEcho.sprite = sourceRenderer.sprite;
            bossEcho.flipX = sourceRenderer.flipX;
            bossEcho.sortingLayerID = sourceRenderer.sortingLayerID;
            bossEcho.sortingOrder = sourceRenderer.sortingOrder + 18;
            bossEcho.enabled = true;

            bossFlash.sprite = sourceRenderer.sprite;
            bossFlash.flipX = sourceRenderer.flipX;
            bossFlash.sortingLayerID = sourceRenderer.sortingLayerID;
            bossFlash.sortingOrder = sourceRenderer.sortingOrder + 19;
            bossFlash.enabled = true;
            SyncBossOverlay(bossEcho, 1f);
            SyncBossOverlay(bossFlash, 1f);
        }

        private void ConfigureBiomeVisuals(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.Intestine:
                    groundMark.sprite = CombatVfxResources.GetSplatSprite();
                    groundAccent.sprite = CombatVfxResources.GetCrackSprite();
                    for (int i = 0; i < DetailCount; i++)
                    {
                        EnableDetail(i, i < 8
                            ? CombatVfxResources.GetSoftCircleSprite()
                            : CombatVfxResources.GetSplatSprite());
                    }
                    break;

                case BiomeType.Liver:
                    groundMark.sprite = CombatVfxResources.GetSplatSprite();
                    groundAccent.sprite = CombatVfxResources.GetRuneSprite();
                    for (int i = 0; i < 8; i++)
                    {
                        EnableDetail(i, CombatVfxResources.GetBloodDropSprite());
                    }
                    EnableDetail(8, CombatVfxResources.GetRuneSprite());
                    EnableDetail(9, CombatVfxResources.GetStarSprite());
                    break;

                case BiomeType.Stomach:
                    groundMark.sprite = CombatVfxResources.GetSplatSprite();
                    groundAccent.sprite = CombatVfxResources.GetSplatSprite();
                    for (int i = 0; i < 4; i++)
                    {
                        EnableDetail(i, CombatVfxResources.GetFangSprite());
                    }
                    for (int i = 4; i < DetailCount; i++)
                    {
                        EnableDetail(i, CombatVfxResources.GetSoftCircleSprite());
                    }
                    break;

                case BiomeType.Lung:
                    groundMark.sprite = CombatVfxResources.GetWindArcSprite();
                    groundAccent.sprite = CombatVfxResources.GetWindArcSprite();
                    for (int i = 0; i < 7; i++)
                    {
                        EnableDetail(i, CombatVfxResources.GetWindArcSprite());
                    }
                    for (int i = 7; i < DetailCount; i++)
                    {
                        EnableDetail(i, CombatVfxResources.GetSoftCircleSprite());
                    }
                    break;

                default:
                    groundMark.sprite = CombatVfxResources.GetRuneSprite();
                    groundAccent.sprite = CombatVfxResources.GetCrackSprite();
                    EnableDetail(0, CombatVfxResources.GetStarSprite());
                    break;
            }
        }

        private void UpdateBossPresence(float t)
        {
            if (sourceRenderer == null || sourceRenderer.sprite == null)
            {
                if (bossEcho != null)
                {
                    bossEcho.enabled = false;
                }
                if (bossFlash != null)
                {
                    bossFlash.enabled = false;
                }
                return;
            }

            bossEcho.sprite = sourceRenderer.sprite;
            bossEcho.flipX = sourceRenderer.flipX;
            bossFlash.sprite = sourceRenderer.sprite;
            bossFlash.flipX = sourceRenderer.flipX;

            float anticipation = Mathf.Clamp01(t / 0.24f);
            float echoBurstT = Mathf.Clamp01((t - 0.18f) / 0.5f);
            float echoBurst = Mathf.Sin(echoBurstT * Mathf.PI);
            float presenceFade = Mathf.Clamp01((0.84f - t) / 0.3f);
            float echoScale = Mathf.Lerp(0.92f, 1.08f, anticipation)
                + echoBurst * 0.22f;
            SyncBossOverlay(bossEcho, echoScale);
            Color presenceColor = Color.Lerp(primaryColor, accentColor, 0.62f);
            bossEcho.color = WithAlpha(
                presenceColor,
                presenceFade * (anticipation * 0.24f + echoBurst * 0.48f));

            float flashT = Mathf.Clamp01((t - 0.22f) / 0.24f);
            float flashPulse = Mathf.Sin(flashT * Mathf.PI);
            SyncBossOverlay(bossFlash, Mathf.Lerp(0.96f, 1.1f, flashPulse));
            bossFlash.color = WithAlpha(
                Color.Lerp(Color.white, accentColor, 0.2f),
                flashPulse * 0.72f);
        }

        private void UpdateGroundBurstLayers(float t)
        {
            float secondWaveT = Mathf.Clamp01((t - 0.34f) / 0.4f);
            secondaryShockwave.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.12f, 2.38f, EaseOut(secondWaveT));
            secondaryShockwave.transform.rotation = GroundRotation(t * -35f);
            secondaryShockwave.color = WithAlpha(
                Color.Lerp(primaryColor, accentColor, 0.58f),
                Mathf.Sin(secondWaveT * Mathf.PI) * 0.64f);

            for (int i = 0; i < groundRays.Length; i++)
            {
                SpriteRenderer ray = groundRays[i];
                if (ray == null)
                {
                    continue;
                }

                float delay = 0.18f + (i % 2) * 0.035f;
                float rayT = Mathf.Clamp01((t - delay) / 0.48f);
                float burst = EaseOut(rayT);
                float alpha = Mathf.Sin(rayT * Mathf.PI);
                float angle = angleSeed + i * (360f / RayCount) + (i % 2) * 11f;
                float radians = angle * Mathf.Deg2Rad;
                Vector3 radial = new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
                float length = effectScale * Mathf.Lerp(0.08f, 1.35f + (i % 3) * 0.18f, burst);
                ray.transform.localPosition = radial * length * 0.48f
                    + Vector3.up * (0.056f + i * 0.001f);
                ray.transform.localScale = new Vector3(
                    effectScale * Mathf.Lerp(0.075f, 0.025f, burst),
                    length,
                    1f);
                ray.transform.rotation = GroundRotation(angle);
                ray.color = WithAlpha(
                    i % 2 == 0 ? primaryColor : accentColor,
                    alpha * (0.68f - (i % 3) * 0.08f));
            }
        }

        private void SyncBossOverlay(SpriteRenderer overlay, float scaleMultiplier)
        {
            if (overlay == null || sourceRenderer == null)
            {
                return;
            }

            Transform sourceTransform = sourceRenderer.transform;
            overlay.transform.position = sourceTransform.position;
            overlay.transform.rotation = sourceTransform.rotation;
            Vector3 liveScale = sourceTransform.lossyScale;
            if (liveScale.sqrMagnitude <= 0.0001f)
            {
                liveScale = sourceBaseScale;
            }
            overlay.transform.localScale = liveScale * scaleMultiplier;
        }

        private void UpdateIntestine(float t)
        {
            float envelope = FadeEnvelope(t, 0.1f, 0.2f);
            float spread = 1f - Mathf.Pow(1f - t, 3f);

            groundMark.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.08f, 1.38f, spread);
            groundMark.transform.rotation = GroundRotation(12f);
            groundMark.color = WithAlpha(primaryColor, envelope * (1f - t * 0.55f) * 0.72f);

            float crackT = Mathf.Clamp01((t - 0.14f) / 0.62f);
            groundAccent.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.04f, 1.08f, EaseOut(crackT));
            groundAccent.transform.rotation = GroundRotation(74f);
            groundAccent.color = WithAlpha(accentColor, Mathf.Sin(crackT * Mathf.PI) * 0.62f);
            UpdateExpandingShockwave(t, 0.28f, 0.46f, 1.72f, primaryColor, 0.66f);

            for (int i = 0; i < 8; i++)
            {
                SpriteRenderer spore = details[i];
                float delay = i * 0.025f;
                float sporeT = Mathf.Clamp01((t - delay) / 0.82f);
                float angle = (angleSeed + i * 137.5f) * Mathf.Deg2Rad;
                Vector3 radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                float radius = effectScale * Mathf.Lerp(0.16f, 0.82f, EaseOut(sporeT));
                spore.transform.localPosition = centerOffset
                    + radial * radius
                    + Vector3.up * effectScale * Mathf.Lerp(-0.34f, 1.18f + (i % 3) * 0.18f, sporeT);
                float pulse = Mathf.Sin(sporeT * Mathf.PI);
                spore.transform.localScale = Vector3.one
                    * effectScale
                    * Mathf.Lerp(0.07f, 0.2f + (i % 2) * 0.06f, pulse);
                spore.transform.rotation = GetCameraRotation();
                Color sporeColor = i % 3 == 0
                    ? new Color(1f, 0.94f, 0.58f, 1f)
                    : i % 3 == 1 ? accentColor : Color.Lerp(primaryColor, accentColor, 0.4f);
                spore.color = WithAlpha(sporeColor, pulse * 0.9f);
            }

            for (int i = 8; i < DetailCount; i++)
            {
                SpriteRenderer glob = details[i];
                float globT = Mathf.Clamp01((t - 0.12f - (i - 8) * 0.08f) / 0.58f);
                float side = i == 8 ? -1f : 1f;
                glob.transform.localPosition = centerOffset
                    + GetCameraRight() * side * effectScale * Mathf.Lerp(0.05f, 0.72f, EaseOut(globT))
                    + Vector3.up * effectScale * Mathf.Sin(globT * Mathf.PI) * 0.48f;
                glob.transform.localScale = Vector3.one * effectScale * Mathf.Lerp(0.08f, 0.3f, Mathf.Sin(globT * Mathf.PI));
                glob.transform.rotation = GetCameraRotation() * Quaternion.Euler(0f, 0f, side * 30f);
                glob.color = WithAlpha(primaryColor, Mathf.Sin(globT * Mathf.PI) * 0.78f);
            }
        }

        private void UpdateLiver(float t)
        {
            float envelope = FadeEnvelope(t, 0.12f, 0.22f);
            float poolSpread = EaseOut(Mathf.Clamp01(t / 0.72f));
            groundMark.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.08f, 1.48f, poolSpread);
            groundMark.transform.rotation = GroundRotation(-18f);
            groundMark.color = WithAlpha(primaryColor, envelope * 0.68f);

            float runePulse = Mathf.Sin(Mathf.Clamp01(t / 0.78f) * Mathf.PI);
            groundAccent.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.22f, 1.08f, runePulse);
            groundAccent.transform.rotation = GroundRotation(t * 110f);
            groundAccent.color = WithAlpha(accentColor, runePulse * envelope * 0.78f);
            UpdateExpandingShockwave(t, 0.42f, 0.42f, 1.65f, accentColor, 0.72f);

            for (int i = 0; i < 8; i++)
            {
                SpriteRenderer drop = details[i];
                float delay = i * 0.018f;
                float dropT = Mathf.Clamp01((t - delay) / 0.76f);
                float angle = (angleSeed + i * 45f) * Mathf.Deg2Rad;
                float radius = effectScale * Mathf.Lerp(1.0f, 0.13f, EaseOut(dropT));
                Vector3 radial = GetCameraRight() * Mathf.Cos(angle)
                    + Vector3.up * Mathf.Sin(angle);
                drop.transform.localPosition = centerOffset
                    + radial * radius
                    + Vector3.up * effectScale * 0.16f;
                float pulse = Mathf.Sin(dropT * Mathf.PI);
                drop.transform.localScale = new Vector3(
                    effectScale * Mathf.Lerp(0.09f, 0.18f, pulse),
                    effectScale * Mathf.Lerp(0.15f, 0.3f, pulse),
                    1f);
                drop.transform.rotation = GetCameraRotation()
                    * Quaternion.Euler(0f, 0f, -angle * Mathf.Rad2Deg - 90f);
                drop.color = WithAlpha(Color.Lerp(primaryColor, new Color(1f, 0.08f, 0.12f, 1f), i / 7f), pulse * 0.95f);
            }

            SpriteRenderer bodyRune = details[8];
            float bodyRuneT = Mathf.Clamp01((t - 0.28f) / 0.62f);
            float bodyRunePulse = Mathf.Sin(bodyRuneT * Mathf.PI);
            bodyRune.transform.localPosition = centerOffset + Vector3.up * effectScale * 0.08f;
            bodyRune.transform.localScale = Vector3.one * effectScale * Mathf.Lerp(0.12f, 0.82f, bodyRunePulse);
            bodyRune.transform.rotation = GetCameraRotation() * Quaternion.Euler(0f, 0f, t * -210f);
            bodyRune.color = WithAlpha(accentColor, bodyRunePulse * 0.72f);

            SpriteRenderer bloodCore = details[9];
            float coreT = Mathf.Clamp01((t - 0.44f) / 0.38f);
            float corePulse = Mathf.Sin(coreT * Mathf.PI);
            bloodCore.transform.localPosition = centerOffset + Vector3.up * effectScale * 0.08f;
            bloodCore.transform.localScale = Vector3.one * effectScale * Mathf.Lerp(0.03f, 0.72f, corePulse);
            bloodCore.transform.rotation = GetCameraRotation() * Quaternion.Euler(0f, 0f, coreT * 140f);
            bloodCore.color = WithAlpha(new Color(1f, 0.82f, 0.9f, 1f), corePulse);
        }

        private void UpdateStomach(float t)
        {
            float envelope = FadeEnvelope(t, 0.1f, 0.2f);
            float acidSpread = EaseOut(Mathf.Clamp01(t / 0.72f));
            groundMark.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.08f, 1.34f, acidSpread);
            groundMark.transform.rotation = GroundRotation(24f);
            groundMark.color = WithAlpha(primaryColor, envelope * 0.7f);

            groundAccent.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.04f, 0.9f, acidSpread);
            groundAccent.transform.rotation = GroundRotation(-58f);
            groundAccent.color = WithAlpha(Color.Lerp(primaryColor, accentColor, 0.35f), envelope * 0.5f);

            float suctionT = Mathf.Clamp01(t / 0.56f);
            shockwave.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(1.72f, 0.08f, EaseOut(suctionT));
            shockwave.transform.rotation = GroundRotation(t * -100f);
            shockwave.color = WithAlpha(primaryColor, Mathf.Sin(suctionT * Mathf.PI) * 0.76f);

            for (int i = 0; i < 4; i++)
            {
                SpriteRenderer fang = details[i];
                float delay = (i % 2) * 0.045f;
                float biteT = Mathf.Clamp01((t - delay) / 0.58f);
                float side = i % 2 == 0 ? -1f : 1f;
                bool upper = i < 2;
                float vertical = upper ? 1f : -1f;
                fang.transform.localPosition = centerOffset
                    + GetCameraRight() * side * effectScale * Mathf.Lerp(0.82f, 0.24f, EaseOut(biteT))
                    + Vector3.up * vertical * effectScale * Mathf.Lerp(0.72f, 0.26f, EaseOut(biteT));
                float bitePulse = Mathf.Sin(biteT * Mathf.PI);
                fang.transform.localScale = new Vector3(
                    effectScale * 0.28f,
                    effectScale * Mathf.Lerp(0.42f, 0.72f, bitePulse),
                    1f);
                float rotation = upper ? side * 22f : 180f - side * 22f;
                fang.transform.rotation = GetCameraRotation() * Quaternion.Euler(0f, 0f, rotation);
                fang.color = WithAlpha(i % 2 == 0 ? accentColor : new Color(1f, 0.94f, 0.72f, 1f), bitePulse * 0.95f);
            }

            for (int i = 4; i < DetailCount; i++)
            {
                SpriteRenderer bubble = details[i];
                float delay = (i - 4) * 0.035f;
                float bubbleT = Mathf.Clamp01((t - delay) / 0.8f);
                float angle = (angleSeed + (i - 4) * 60f) * Mathf.Deg2Rad;
                float radius = effectScale * Mathf.Lerp(1.05f, 0.08f, EaseOut(bubbleT));
                bubble.transform.localPosition = centerOffset
                    + GetCameraRight() * Mathf.Cos(angle) * radius
                    + Vector3.up * (Mathf.Sin(angle) * radius * 0.55f + effectScale * 0.12f);
                float pulse = Mathf.Sin(bubbleT * Mathf.PI);
                bubble.transform.localScale = Vector3.one
                    * effectScale
                    * Mathf.Lerp(0.08f, 0.22f + (i % 2) * 0.08f, pulse);
                bubble.transform.rotation = GetCameraRotation();
                bubble.color = WithAlpha(Color.Lerp(primaryColor, new Color(0.9f, 1f, 0.4f, 1f), (i - 4) / 5f), pulse * 0.82f);
            }
        }

        private void UpdateLung(float t)
        {
            float envelope = FadeEnvelope(t, 0.08f, 0.2f);
            float expand = EaseOut(t);
            groundMark.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.18f, 1.22f, expand);
            groundMark.transform.rotation = GroundRotation(t * 220f);
            groundMark.color = WithAlpha(primaryColor, envelope * 0.62f);

            groundAccent.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.12f, 0.9f, expand);
            groundAccent.transform.rotation = GroundRotation(-t * 310f + 180f);
            groundAccent.color = WithAlpha(accentColor, envelope * 0.5f);
            UpdateExpandingShockwave(t, 0.24f, 0.54f, 1.58f, primaryColor, 0.5f);

            Quaternion cameraRotation = GetCameraRotation();
            for (int i = 0; i < 7; i++)
            {
                SpriteRenderer arc = details[i];
                float delay = i * 0.028f;
                float arcT = Mathf.Clamp01((t - delay) / 0.76f);
                float direction = i % 2 == 0 ? 1f : -1f;
                float angle = angleSeed + i * 51.4f + direction * arcT * 210f;
                float radius = effectScale * Mathf.Lerp(0.12f, 0.78f, EaseOut(arcT));
                float radians = angle * Mathf.Deg2Rad;
                arc.transform.localPosition = centerOffset
                    + GetCameraRight() * Mathf.Cos(radians) * radius
                    + Vector3.up * (Mathf.Sin(radians) * radius * 0.52f + effectScale * 0.12f);
                float pulse = Mathf.Sin(arcT * Mathf.PI);
                arc.transform.localScale = Vector3.one
                    * effectScale
                    * Mathf.Lerp(0.15f, 0.46f + (i % 3) * 0.08f, pulse);
                arc.transform.rotation = cameraRotation * Quaternion.Euler(0f, 0f, angle + direction * 70f);
                arc.color = WithAlpha(i % 2 == 0 ? primaryColor : accentColor, pulse * 0.78f);
            }

            for (int i = 7; i < DetailCount; i++)
            {
                SpriteRenderer puff = details[i];
                float puffT = Mathf.Clamp01((t - (i - 7) * 0.08f) / 0.82f);
                float side = i % 2 == 0 ? -1f : 1f;
                puff.transform.localPosition = centerOffset
                    + GetCameraRight() * side * effectScale * Mathf.Lerp(0.12f, 1.18f, EaseOut(puffT))
                    + Vector3.up * effectScale * Mathf.Lerp(-0.08f, 0.58f + (i - 7) * 0.14f, puffT);
                float pulse = Mathf.Sin(puffT * Mathf.PI);
                puff.transform.localScale = new Vector3(
                    effectScale * Mathf.Lerp(0.1f, 0.42f, pulse),
                    effectScale * Mathf.Lerp(0.08f, 0.24f, pulse),
                    1f);
                puff.transform.rotation = cameraRotation;
                puff.color = WithAlpha(Color.Lerp(primaryColor, Color.white, 0.55f), pulse * 0.56f);
            }
        }

        private void UpdateFallback(float t)
        {
            float envelope = FadeEnvelope(t, 0.1f, 0.2f);
            groundMark.transform.localScale = Vector3.one * effectScale * Mathf.Lerp(0.08f, 1.2f, EaseOut(t));
            groundMark.transform.rotation = GroundRotation(t * 90f);
            groundMark.color = WithAlpha(primaryColor, envelope * 0.68f);
            groundAccent.transform.localScale = Vector3.one * effectScale * Mathf.Lerp(0.04f, 1f, EaseOut(t));
            groundAccent.transform.rotation = GroundRotation(45f);
            groundAccent.color = WithAlpha(accentColor, envelope * 0.6f);
            UpdateExpandingShockwave(t, 0.2f, 0.5f, 1.6f, accentColor, 0.72f);
        }

        private void UpdateExpandingShockwave(
            float t,
            float delay,
            float span,
            float maxSize,
            Color color,
            float maxAlpha)
        {
            float waveT = Mathf.Clamp01((t - delay) / span);
            shockwave.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.05f, maxSize, EaseOut(waveT));
            shockwave.transform.rotation = GroundRotation(0f);
            shockwave.color = WithAlpha(color, Mathf.Sin(waveT * Mathf.PI) * maxAlpha);
        }

        private void EmitBiomeParticles(Vector3 groundPosition, BiomeType biome)
        {
            if (particles == null)
            {
                return;
            }

            particles.Clear(true);
            particles.Play(true);
            int count = biome == BiomeType.Lung ? 88 : biome == BiomeType.Liver ? 76 : 68;
            for (int i = 0; i < count; i++)
            {
                Vector3 radial = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
                Vector3 velocity;
                Vector3 position;
                float size;

                switch (biome)
                {
                    case BiomeType.Intestine:
                        velocity = radial * Random.Range(0.25f, 0.9f) * effectScale
                            + Vector3.up * Random.Range(0.8f, 2.1f) * effectScale;
                        position = groundPosition + radial * Random.Range(0.08f, 0.62f) * effectScale;
                        size = Random.Range(0.07f, 0.18f);
                        break;
                    case BiomeType.Liver:
                        velocity = radial * Random.Range(0.45f, 1.35f) * effectScale
                            + Vector3.up * Random.Range(0.3f, 1.15f) * effectScale;
                        position = groundPosition + centerOffset + radial * Random.Range(0.05f, 0.3f) * effectScale;
                        size = Random.Range(0.055f, 0.15f);
                        break;
                    case BiomeType.Stomach:
                        velocity = radial * Random.Range(0.22f, 0.78f) * effectScale
                            + Vector3.up * Random.Range(0.55f, 1.7f) * effectScale;
                        position = groundPosition + radial * Random.Range(0.1f, 0.72f) * effectScale;
                        size = Random.Range(0.08f, 0.2f);
                        break;
                    case BiomeType.Lung:
                        velocity = radial * Random.Range(1.4f, 3.2f) * effectScale
                            + Vector3.up * Random.Range(0.08f, 0.5f) * effectScale;
                        position = groundPosition + centerOffset * 0.45f + radial * Random.Range(0.05f, 0.35f) * effectScale;
                        size = Random.Range(0.045f, 0.12f);
                        break;
                    default:
                        velocity = radial * Random.Range(0.6f, 1.6f) * effectScale + Vector3.up;
                        position = groundPosition;
                        size = 0.1f;
                        break;
                }

                ParticleSystem.EmitParams parameters = new ParticleSystem.EmitParams
                {
                    position = position + Vector3.up * Random.Range(0.04f, 0.18f),
                    velocity = velocity,
                    startColor = Color.Lerp(primaryColor, accentColor, Random.Range(0.08f, 0.92f)),
                    startLifetime = Random.Range(0.68f, 1.28f),
                    startSize = size * effectScale,
                    rotation = Random.Range(0f, 360f)
                };
                particles.Emit(parameters, 1);
            }
        }

        private void EnableDetail(int index, Sprite sprite)
        {
            if (index < 0 || index >= details.Length || details[index] == null)
            {
                return;
            }

            details[index].sprite = sprite;
            details[index].enabled = true;
        }

        private void EnsureComponents()
        {
            if (groundMark == null)
            {
                groundMark = transform.Find("BossGroundMark")?.GetComponent<SpriteRenderer>();
            }
            if (groundAccent == null)
            {
                groundAccent = transform.Find("BossGroundAccent")?.GetComponent<SpriteRenderer>();
            }
            if (shockwave == null)
            {
                shockwave = transform.Find("BossShockwave")?.GetComponent<SpriteRenderer>();
            }
            if (secondaryShockwave == null)
            {
                secondaryShockwave = transform.Find("BossSecondaryShockwave")?.GetComponent<SpriteRenderer>();
            }
            if (bossEcho == null)
            {
                bossEcho = transform.Find("BossBodyEcho")?.GetComponent<SpriteRenderer>();
            }
            if (bossFlash == null)
            {
                bossFlash = transform.Find("BossBodyFlash")?.GetComponent<SpriteRenderer>();
            }
            if (particles == null)
            {
                particles = transform.Find("BossParticles")?.GetComponent<ParticleSystem>();
            }
            if (autoReturn == null)
            {
                autoReturn = RuntimePool.EnsureAutoReturn(gameObject);
            }
            for (int i = 0; i < DetailCount; i++)
            {
                if (details[i] == null)
                {
                    details[i] = transform.Find($"BossDetail{i + 1}")?.GetComponent<SpriteRenderer>();
                }
            }
            for (int i = 0; i < RayCount; i++)
            {
                if (groundRays[i] == null)
                {
                    groundRays[i] = transform.Find($"BossGroundRay{i + 1}")?.GetComponent<SpriteRenderer>();
                }
            }
        }

        private static SpriteRenderer CreateSpriteRenderer(
            Transform parent,
            string objectName,
            Sprite sprite,
            int sortingOrder)
        {
            GameObject spriteObject = new GameObject(objectName);
            spriteObject.transform.SetParent(parent, false);
            SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.enabled = false;
            return renderer;
        }

        private static ParticleSystem CreateParticleSystem(Transform parent)
        {
            GameObject particleObject = new GameObject("BossParticles");
            particleObject.transform.SetParent(parent, false);
            ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 128;
            main.startSpeed = 0f;
            main.startLifetime = 1f;
            main.startSize = 0.15f;
            main.gravityModifier = 0f;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = false;
            ParticleSystem.SizeOverLifetimeModule size = particleSystem.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(
                1f,
                new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(0.12f, 1f),
                    new Keyframe(0.72f, 0.76f),
                    new Keyframe(1f, 0f)));

            ParticleSystemRenderer renderer = particleObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortingOrder = ParticleSortingOrder;
            renderer.sharedMaterial = CombatVfxResources.GetParticleMaterial();
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return particleSystem;
        }

        private static Quaternion GroundRotation(float yaw)
        {
            return Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
        }

        private static Quaternion GetCameraRotation()
        {
            Camera camera = DontStarveCamera.GetActiveCamera();
            return camera != null ? camera.transform.rotation : Quaternion.Euler(45f, 0f, 0f);
        }

        private static Vector3 GetCameraRight()
        {
            Camera camera = DontStarveCamera.GetActiveCamera();
            return camera != null ? camera.transform.right : Vector3.right;
        }

        private static float FadeEnvelope(float t, float fadeIn, float fadeOut)
        {
            return Mathf.Clamp01(t / Mathf.Max(0.01f, fadeIn))
                * Mathf.Clamp01((1f - t) / Mathf.Max(0.01f, fadeOut));
        }

        private static float EaseOut(float t)
        {
            return 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }
    }

    [DisallowMultipleComponent]
    internal sealed class BossEncounterVfxPending : MonoBehaviour
    {
        private EnemyController target;
        private BiomeType biome;
        private bool addCameraShake;

        public void Arm(EnemyController boss, BiomeType encounterBiome, bool shake)
        {
            target = boss;
            biome = encounterBiome;
            addCameraShake = shake;
            enabled = true;
        }

        private void Update()
        {
            if (target == null || target.IsDead || !target.gameObject.activeInHierarchy)
            {
                enabled = false;
                return;
            }

            if (CombatVfx.TryPlayBossEncounterNow(target, biome, addCameraShake))
            {
                target = null;
                enabled = false;
            }
        }

        private void OnDisable()
        {
            target = null;
        }
    }
}
