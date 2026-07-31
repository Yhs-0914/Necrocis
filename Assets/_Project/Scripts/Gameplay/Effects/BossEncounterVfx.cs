using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    internal sealed class BossEncounterVfx : MonoBehaviour
    {
        private const int AuraSortingOrder = 5300;
        private const int RingSortingOrder = 5310;
        private const int WispSortingOrder = 5320;
        private const int CoreSortingOrder = 5350;
        private const int ParticleSortingOrder = 5360;
        private const int RingCount = 3;
        private const int WispCount = 6;

        private readonly SpriteRenderer[] rings = new SpriteRenderer[RingCount];
        private readonly SpriteRenderer[] wisps = new SpriteRenderer[WispCount];

        private SpriteRenderer groundAura;
        private SpriteRenderer core;
        private ParticleSystem motes;
        private RuntimePoolAutoReturn autoReturn;
        private Transform followTarget;
        private Vector3 followOffset;
        private Vector3 centerOffset;
        private Color primaryColor;
        private Color accentColor;
        private float duration;
        private float elapsed;
        private float effectScale;

        public static GameObject CreateObject()
        {
            GameObject root = new GameObject("BossEncounterFx");
            root.SetActive(false);

            BossEncounterVfx effect = root.AddComponent<BossEncounterVfx>();
            effect.groundAura = CreateSpriteRenderer(
                root.transform,
                "GroundAura",
                CombatVfxResources.GetSoftCircleSprite(),
                AuraSortingOrder);

            for (int i = 0; i < RingCount; i++)
            {
                effect.rings[i] = CreateSpriteRenderer(
                    root.transform,
                    $"SealRing{i + 1}",
                    CombatVfxResources.GetRingSprite(),
                    RingSortingOrder + i);
            }

            for (int i = 0; i < WispCount; i++)
            {
                effect.wisps[i] = CreateSpriteRenderer(
                    root.transform,
                    $"RisingWisp{i + 1}",
                    CombatVfxResources.GetSoftCircleSprite(),
                    WispSortingOrder + i);
            }

            effect.core = CreateSpriteRenderer(
                root.transform,
                "BossRevealFlash",
                CombatVfxResources.GetSoftCircleSprite(),
                CoreSortingOrder);
            effect.motes = CreateMoteSystem(root.transform);
            effect.autoReturn = RuntimePool.EnsureAutoReturn(root);
            return root;
        }

        public void Show(
            Transform target,
            Vector3 groundPosition,
            Vector3 center,
            float scale,
            Color primary,
            Color accent)
        {
            EnsureComponents();

            followTarget = target;
            followOffset = target != null ? groundPosition - target.position : Vector3.zero;
            centerOffset = center - groundPosition;
            primaryColor = primary;
            accentColor = accent;
            duration = 1.55f;
            elapsed = 0f;
            effectScale = Mathf.Clamp(scale, 0.9f, 3.4f);

            transform.SetParent(null, false);
            transform.position = groundPosition;
            transform.rotation = Quaternion.identity;

            groundAura.enabled = true;
            groundAura.transform.localPosition = Vector3.up * 0.018f;
            groundAura.transform.localScale = Vector3.one * effectScale * 0.16f;
            groundAura.color = WithAlpha(primaryColor, 0f);

            for (int i = 0; i < rings.Length; i++)
            {
                SpriteRenderer ring = rings[i];
                if (ring == null)
                {
                    continue;
                }

                ring.enabled = true;
                ring.transform.localPosition = Vector3.up * (0.026f + i * 0.012f);
                ring.transform.localScale = Vector3.one * effectScale * 0.1f;
                ring.color = WithAlpha(Color.Lerp(primaryColor, accentColor, i * 0.32f), 0f);
            }

            for (int i = 0; i < wisps.Length; i++)
            {
                SpriteRenderer wisp = wisps[i];
                if (wisp == null)
                {
                    continue;
                }

                wisp.enabled = true;
                wisp.transform.localPosition = centerOffset;
                wisp.transform.localScale = new Vector3(
                    effectScale * 0.12f,
                    effectScale * 0.62f,
                    1f);
                wisp.color = WithAlpha(Color.Lerp(primaryColor, accentColor, i / (WispCount - 1f)), 0f);
            }

            core.enabled = true;
            core.transform.localPosition = centerOffset;
            core.transform.localScale = Vector3.one * effectScale * 0.18f;
            core.color = WithAlpha(accentColor, 0f);

            ApplyElementOrientations();
            EmitMotes(groundPosition);
            autoReturn.Schedule(duration);
            enabled = true;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float appear = Mathf.Clamp01(t / 0.1f);
            float fade = Mathf.Clamp01((1f - t) / 0.3f);
            float envelope = appear * fade;
            float expand = 1f - Mathf.Pow(1f - t, 3f);

            if (followTarget != null)
            {
                transform.position = followTarget.position + followOffset;
            }
            transform.rotation = Quaternion.identity;
            ApplyElementOrientations();

            UpdateGroundAura(t, envelope, expand);
            UpdateRings(t);
            UpdateWisps(t, envelope, expand);
            UpdateCore(t, envelope);

            if (t >= 1f)
            {
                RuntimePool.Release(gameObject);
            }
        }

        private void UpdateGroundAura(float t, float envelope, float expand)
        {
            if (groundAura == null)
            {
                return;
            }

            groundAura.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.16f, 0.92f, expand);
            groundAura.color = WithAlpha(primaryColor, envelope * (1f - t) * 0.3f);
        }

        private void UpdateRings(float t)
        {
            for (int i = 0; i < rings.Length; i++)
            {
                SpriteRenderer ring = rings[i];
                if (ring == null)
                {
                    continue;
                }

                float delay = i * 0.12f;
                float ringT = Mathf.Clamp01((t - delay) / Mathf.Max(0.01f, 0.88f - delay));
                float ringExpand = 1f - Mathf.Pow(1f - ringT, 3f);
                float ringFade = Mathf.Sin(ringT * Mathf.PI);
                float maxSize = 1.02f - i * 0.1f;
                ring.transform.localScale = Vector3.one
                    * effectScale
                    * Mathf.Lerp(0.1f, maxSize, ringExpand);
                ring.color = WithAlpha(
                    Color.Lerp(primaryColor, accentColor, i * 0.32f),
                    ringFade * (0.9f - i * 0.12f));
            }
        }

        private void UpdateWisps(float t, float envelope, float expand)
        {
            float angleOffset = Time.unscaledTime * -72f;
            float radius = effectScale * Mathf.Lerp(0.72f, 0.42f, expand);

            for (int i = 0; i < wisps.Length; i++)
            {
                SpriteRenderer wisp = wisps[i];
                if (wisp == null)
                {
                    continue;
                }

                float delay = i * 0.035f;
                float wispT = Mathf.Clamp01((t - delay) / Mathf.Max(0.01f, 0.82f - delay));
                float angle = angleOffset + i * (360f / WispCount);
                float radians = angle * Mathf.Deg2Rad;
                float rise = centerOffset.y
                    + Mathf.Lerp(-0.72f, 1.0f, wispT) * effectScale;
                wisp.transform.localPosition = new Vector3(
                    Mathf.Cos(radians) * radius,
                    rise,
                    Mathf.Sin(radians) * radius);

                float pulse = Mathf.Sin(wispT * Mathf.PI);
                wisp.transform.localScale = new Vector3(
                    effectScale * Mathf.Lerp(0.055f, 0.13f, pulse),
                    effectScale * Mathf.Lerp(0.3f, 0.72f, pulse),
                    1f);
                wisp.color = WithAlpha(
                    Color.Lerp(primaryColor, accentColor, (i % 3) * 0.34f),
                    envelope * pulse * 0.82f);
            }
        }

        private void UpdateCore(float t, float envelope)
        {
            if (core == null)
            {
                return;
            }

            float pulse = Mathf.Sin(Mathf.Clamp01(t * 1.75f) * Mathf.PI);
            core.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.18f, 0.96f, pulse);
            core.color = WithAlpha(accentColor, pulse * envelope * 0.78f);
        }

        private void ApplyElementOrientations()
        {
            Quaternion billboardRotation = GetCameraRotation();
            Quaternion groundRotation = Quaternion.Euler(90f, 0f, 0f);

            if (groundAura != null)
            {
                groundAura.transform.rotation = groundRotation;
            }
            if (core != null)
            {
                core.transform.rotation = billboardRotation;
            }

            for (int i = 0; i < rings.Length; i++)
            {
                if (rings[i] != null)
                {
                    rings[i].transform.rotation = groundRotation;
                }
            }

            for (int i = 0; i < wisps.Length; i++)
            {
                if (wisps[i] != null)
                {
                    wisps[i].transform.rotation = billboardRotation;
                }
            }
        }

        private void EmitMotes(Vector3 groundPosition)
        {
            if (motes == null)
            {
                return;
            }

            motes.Clear(true);
            motes.Play(true);

            for (int i = 0; i < 42; i++)
            {
                Vector3 radial = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up)
                    * Vector3.forward;
                Vector3 velocity = radial * Random.Range(0.4f, 1.2f) * effectScale;
                velocity.y = Random.Range(1.2f, 3.2f) * effectScale;

                ParticleSystem.EmitParams parameters = new ParticleSystem.EmitParams
                {
                    position = groundPosition
                        + radial * Random.Range(0.08f, 0.68f) * effectScale
                        + Vector3.up * Random.Range(0.03f, 0.24f),
                    velocity = velocity,
                    startColor = Color.Lerp(primaryColor, accentColor, Random.Range(0.1f, 0.9f)),
                    startLifetime = Random.Range(0.7f, 1.3f),
                    startSize = Random.Range(0.07f, 0.2f) * effectScale,
                    rotation = Random.Range(0f, 360f)
                };
                motes.Emit(parameters, 1);
            }
        }

        private void EnsureComponents()
        {
            if (groundAura == null)
            {
                groundAura = transform.Find("GroundAura")?.GetComponent<SpriteRenderer>();
            }
            if (core == null)
            {
                core = transform.Find("BossRevealFlash")?.GetComponent<SpriteRenderer>();
            }
            if (motes == null)
            {
                motes = transform.Find("Motes")?.GetComponent<ParticleSystem>();
            }
            if (autoReturn == null)
            {
                autoReturn = RuntimePool.EnsureAutoReturn(gameObject);
            }

            for (int i = 0; i < RingCount; i++)
            {
                if (rings[i] == null)
                {
                    rings[i] = transform.Find($"SealRing{i + 1}")?.GetComponent<SpriteRenderer>();
                }
            }

            for (int i = 0; i < WispCount; i++)
            {
                if (wisps[i] == null)
                {
                    wisps[i] = transform.Find($"RisingWisp{i + 1}")?.GetComponent<SpriteRenderer>();
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

        private static ParticleSystem CreateMoteSystem(Transform parent)
        {
            GameObject particleObject = new GameObject("Motes");
            particleObject.transform.SetParent(parent, false);

            ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 72;
            main.startSpeed = 0f;
            main.startLifetime = 1f;
            main.startSize = 0.14f;
            main.gravityModifier = -0.08f;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = false;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
                1f,
                new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(0.12f, 1f),
                    new Keyframe(0.72f, 0.74f),
                    new Keyframe(1f, 0f)));

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.1f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            ParticleSystemRenderer renderer = particleObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortingOrder = ParticleSortingOrder;
            renderer.sharedMaterial = CombatVfxResources.GetParticleMaterial();

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return particleSystem;
        }

        private static Quaternion GetCameraRotation()
        {
            Camera camera = DontStarveCamera.GetActiveCamera();
            return camera != null ? camera.transform.rotation : Quaternion.Euler(45f, 0f, 0f);
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
