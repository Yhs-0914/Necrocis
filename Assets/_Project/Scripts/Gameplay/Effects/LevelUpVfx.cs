using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    internal sealed class LevelUpVfx : MonoBehaviour
    {
        private const int BeamSortingOrder = 5280;
        private const int RingSortingOrder = 5290;
        private const int OrbiterSortingOrder = 5300;
        private const int CoreSortingOrder = 5310;
        private const int ParticleSortingOrder = 5320;
        private const int RingCount = 3;
        private const int OrbiterCount = 6;

        private readonly SpriteRenderer[] rings = new SpriteRenderer[RingCount];
        private readonly SpriteRenderer[] orbiters = new SpriteRenderer[OrbiterCount];

        private SpriteRenderer beam;
        private SpriteRenderer core;
        private ParticleSystem motes;
        private RuntimePoolAutoReturn autoReturn;

        private Transform followTarget;
        private Vector3 followOffset;
        private Vector3 centerOffset;
        private float duration;
        private float elapsed;
        private float effectScale;

        private readonly Color energyColor = new Color(0.95f, 0.08f, 0.32f, 0.95f);
        private readonly Color lifeColor = new Color(1f, 0.48f, 0.12f, 0.95f);
        private readonly Color highlightColor = new Color(1f, 0.94f, 0.7f, 1f);

        public static GameObject CreateObject()
        {
            GameObject root = new GameObject("LevelUpFx");
            root.SetActive(false);

            LevelUpVfx effect = root.AddComponent<LevelUpVfx>();
            effect.beam = CreateSpriteRenderer(
                root.transform,
                "AscensionBeam",
                CombatVfxResources.GetSoftCircleSprite(),
                BeamSortingOrder);

            for (int i = 0; i < RingCount; i++)
            {
                effect.rings[i] = CreateSpriteRenderer(
                    root.transform,
                    $"PulseRing{i + 1}",
                    CombatVfxResources.GetRingSprite(),
                    RingSortingOrder + i);
            }

            for (int i = 0; i < OrbiterCount; i++)
            {
                effect.orbiters[i] = CreateSpriteRenderer(
                    root.transform,
                    $"LifeMote{i + 1}",
                    CombatVfxResources.GetSoftCircleSprite(),
                    OrbiterSortingOrder + i);
            }

            effect.core = CreateSpriteRenderer(
                root.transform,
                "CoreFlash",
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
            float scale)
        {
            EnsureComponents();

            followTarget = target;
            followOffset = target != null ? groundPosition - target.position : Vector3.zero;
            centerOffset = center - groundPosition;
            transform.SetParent(null, false);
            transform.position = groundPosition;
            transform.rotation = Quaternion.identity;

            duration = 1.15f;
            elapsed = 0f;
            effectScale = Mathf.Clamp(scale, 0.65f, 1.55f);

            beam.enabled = true;
            beam.transform.localPosition = centerOffset + Vector3.up * effectScale * 0.16f;
            beam.transform.localScale = new Vector3(
                effectScale * 0.26f,
                effectScale * 1.9f,
                1f);
            beam.color = WithAlpha(energyColor, 0f);

            core.enabled = true;
            core.transform.localPosition = centerOffset;
            core.transform.localScale = Vector3.one * effectScale * 0.2f;
            core.color = WithAlpha(highlightColor, 0f);

            for (int i = 0; i < RingCount; i++)
            {
                if (rings[i] == null)
                {
                    continue;
                }

                rings[i].enabled = true;
                rings[i].transform.localPosition = Vector3.up * (0.025f + i * 0.012f);
                rings[i].transform.localScale = Vector3.one * effectScale * 0.12f;
                rings[i].color = WithAlpha(
                    Color.Lerp(energyColor, highlightColor, i * 0.34f),
                    0f);
            }

            for (int i = 0; i < OrbiterCount; i++)
            {
                if (orbiters[i] == null)
                {
                    continue;
                }

                orbiters[i].enabled = true;
                orbiters[i].transform.localPosition = centerOffset;
                orbiters[i].transform.localScale = Vector3.one * effectScale * 0.13f;
                orbiters[i].color = WithAlpha(
                    Color.Lerp(lifeColor, highlightColor, i / (OrbiterCount - 1f)),
                    0f);
            }

            ApplyElementOrientations();
            EmitMotes(center);
            autoReturn.Schedule(duration);
            enabled = true;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float appear = Mathf.Clamp01(t / 0.16f);
            float fade = Mathf.Clamp01((1f - t) / 0.32f);
            float envelope = appear * fade;
            float expand = 1f - Mathf.Pow(1f - t, 3f);

            if (followTarget != null)
            {
                transform.position = followTarget.position + followOffset;
            }
            transform.rotation = Quaternion.identity;
            ApplyElementOrientations();

            UpdateBeam(t, envelope, expand);
            UpdateCore(t, envelope);
            UpdateRings(t);
            UpdateOrbiters(t, envelope, expand);

            if (t >= 1f)
            {
                RuntimePool.Release(gameObject);
            }
        }

        private void UpdateBeam(float t, float envelope, float expand)
        {
            if (beam == null)
            {
                return;
            }

            float width = Mathf.Lerp(0.3f, 0.1f, expand);
            float height = Mathf.Lerp(1.9f, 2.8f, expand);
            beam.transform.localScale = new Vector3(
                effectScale * width,
                effectScale * height,
                1f);
            beam.transform.localPosition = centerOffset
                + Vector3.up * effectScale * Mathf.Lerp(0.1f, 0.5f, expand);
            beam.color = WithAlpha(energyColor, envelope * (1f - t) * 0.42f);
        }

        private void UpdateCore(float t, float envelope)
        {
            if (core == null)
            {
                return;
            }

            float pulse = Mathf.Sin(Mathf.Clamp01(t * 1.65f) * Mathf.PI);
            core.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.2f, 1.05f, pulse);
            core.color = WithAlpha(highlightColor, pulse * envelope);
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

                float delay = i * 0.14f;
                float ringT = Mathf.Clamp01((t - delay) / Mathf.Max(0.01f, 1f - delay));
                float ringExpand = 1f - Mathf.Pow(1f - ringT, 3f);
                float ringFade = Mathf.Sin(ringT * Mathf.PI);
                float size = Mathf.Lerp(0.12f, 0.76f - i * 0.055f, ringExpand);

                ring.transform.localPosition = Vector3.up * (0.025f + i * 0.012f);
                ring.transform.localScale = Vector3.one * effectScale * size;

                Color ringColor = Color.Lerp(
                    energyColor,
                    highlightColor,
                    i / (RingCount - 1f));
                ring.color = WithAlpha(ringColor, ringFade * (0.82f - i * 0.1f));
            }
        }

        private void UpdateOrbiters(float t, float envelope, float expand)
        {
            float radius = effectScale * Mathf.Lerp(0.12f, 0.52f, expand);
            float orbitAngle = Time.unscaledTime * 150f;

            for (int i = 0; i < orbiters.Length; i++)
            {
                SpriteRenderer orbiter = orbiters[i];
                if (orbiter == null)
                {
                    continue;
                }

                float angle = orbitAngle + i * (360f / OrbiterCount);
                float radians = angle * Mathf.Deg2Rad;
                float verticalOffset = centerOffset.y + Mathf.Lerp(-0.35f, 1.05f, t) * effectScale;
                orbiter.transform.localPosition = new Vector3(
                    Mathf.Cos(radians) * radius,
                    verticalOffset + Mathf.Sin(radians * 2f) * effectScale * 0.12f,
                    Mathf.Sin(radians) * radius);

                float pulse = 0.72f + Mathf.Sin(radians * 2f + t * Mathf.PI) * 0.28f;
                orbiter.transform.localScale = Vector3.one
                    * effectScale
                    * Mathf.Lerp(0.08f, 0.18f, pulse);
                Color orbiterColor = Color.Lerp(lifeColor, highlightColor, pulse * 0.65f);
                orbiter.color = WithAlpha(orbiterColor, envelope * pulse);
            }
        }

        private void ApplyElementOrientations()
        {
            Quaternion billboardRotation = GetCameraRotation();

            if (beam != null)
            {
                beam.transform.rotation = billboardRotation;
            }
            if (core != null)
            {
                core.transform.rotation = billboardRotation;
            }

            for (int i = 0; i < rings.Length; i++)
            {
                if (rings[i] != null)
                {
                    rings[i].transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                }
            }

            for (int i = 0; i < orbiters.Length; i++)
            {
                if (orbiters[i] != null)
                {
                    orbiters[i].transform.rotation = billboardRotation;
                }
            }
        }

        private void EmitMotes(Vector3 center)
        {
            if (motes == null)
            {
                return;
            }

            motes.Clear(true);
            motes.Play(true);

            for (int i = 0; i < 28; i++)
            {
                Vector3 radial = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up)
                    * Vector3.forward;
                Vector3 velocity = radial * Random.Range(0.18f, 0.9f) * effectScale;
                velocity.y = Random.Range(1.4f, 3.6f) * effectScale;

                Color moteColor = Color.Lerp(
                    energyColor,
                    highlightColor,
                    Random.Range(0.15f, 0.9f));
                ParticleSystem.EmitParams parameters = new ParticleSystem.EmitParams
                {
                    position = center + radial * Random.Range(0.04f, 0.42f) * effectScale,
                    velocity = velocity,
                    startColor = moteColor,
                    startLifetime = Random.Range(0.55f, 1.05f),
                    startSize = Random.Range(0.06f, 0.17f) * effectScale,
                    rotation = Random.Range(0f, 360f)
                };
                motes.Emit(parameters, 1);
            }
        }

        private void EnsureComponents()
        {
            if (beam == null)
            {
                beam = transform.Find("AscensionBeam")?.GetComponent<SpriteRenderer>();
            }
            if (core == null)
            {
                core = transform.Find("CoreFlash")?.GetComponent<SpriteRenderer>();
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
                    rings[i] = transform.Find($"PulseRing{i + 1}")?.GetComponent<SpriteRenderer>();
                }
            }

            for (int i = 0; i < OrbiterCount; i++)
            {
                if (orbiters[i] == null)
                {
                    orbiters[i] = transform.Find($"LifeMote{i + 1}")?.GetComponent<SpriteRenderer>();
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
            main.maxParticles = 48;
            main.startSpeed = 0f;
            main.startLifetime = 0.85f;
            main.startSize = 0.12f;
            main.gravityModifier = -0.12f;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = false;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
                1f,
                new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(0.15f, 1f),
                    new Keyframe(0.72f, 0.78f),
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
                    new GradientAlphaKey(1f, 0.12f),
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
}
