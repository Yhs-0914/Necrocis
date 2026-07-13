using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    internal sealed class CombatImpactVfx : MonoBehaviour
    {
        private const int FragmentSortingOrder = 5200;
        private const int MistSortingOrder = 5180;
        private const int CoreSortingOrder = 5190;
        private const int RingSortingOrder = 5170;

        private ParticleSystem fragments;
        private ParticleSystem mist;
        private SpriteRenderer core;
        private SpriteRenderer ring;
        private RuntimePoolAutoReturn autoReturn;

        private Color coreColor;
        private Color ringColor;
        private float duration;
        private float elapsed;
        private float effectScale;
        private bool showRing;

        public static GameObject CreateObject()
        {
            GameObject root = new GameObject("CombatImpactFx");
            root.SetActive(false);

            CombatImpactVfx effect = root.AddComponent<CombatImpactVfx>();
            effect.fragments = CreateParticleSystem(root.transform, "Fragments", FragmentSortingOrder, false);
            effect.mist = CreateParticleSystem(root.transform, "Mist", MistSortingOrder, true);
            effect.core = CreateSpriteRenderer(root.transform, "Core", CombatVfxResources.GetSoftCircleSprite(), CoreSortingOrder);
            effect.ring = CreateSpriteRenderer(root.transform, "Ring", CombatVfxResources.GetRingSprite(), RingSortingOrder);
            effect.autoReturn = RuntimePool.EnsureAutoReturn(root);
            return root;
        }

        public void Show(
            Vector3 position,
            Vector3 direction,
            float scale,
            Color primary,
            Color secondary,
            int fragmentCount,
            int mistCount,
            float lifeTime,
            bool withRing,
            float directionalBias)
        {
            EnsureComponents();
            transform.SetParent(null, false);
            transform.position = position;
            transform.rotation = GetCameraRotation();

            duration = Mathf.Max(0.08f, lifeTime);
            elapsed = 0f;
            effectScale = Mathf.Max(0.08f, scale);
            showRing = withRing;
            coreColor = primary;
            ringColor = secondary;

            core.enabled = true;
            core.color = WithAlpha(primary, Mathf.Max(0.6f, primary.a));
            core.transform.localScale = Vector3.one * effectScale * 0.22f;

            ring.enabled = withRing;
            ring.color = WithAlpha(secondary, Mathf.Max(0.5f, secondary.a));
            ring.transform.localScale = Vector3.one * effectScale * 0.3f;

            EmitFragments(position, direction, primary, secondary, Mathf.Max(0, fragmentCount), directionalBias);
            EmitMist(position, primary, secondary, Mathf.Max(0, mistCount));

            autoReturn.Schedule(duration);
            enabled = true;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float fastT = 1f - Mathf.Pow(1f - t, 3f);
            transform.rotation = GetCameraRotation();

            if (core != null)
            {
                float pulse = Mathf.Sin(Mathf.Clamp01(t * 1.7f) * Mathf.PI);
                core.transform.localScale = Vector3.one * effectScale * Mathf.Lerp(0.18f, 0.72f, fastT);
                core.color = WithAlpha(coreColor, pulse * coreColor.a * 0.8f);
            }

            if (ring != null && showRing)
            {
                ring.transform.localScale = Vector3.one * effectScale * Mathf.Lerp(0.28f, 1.25f, fastT);
                ring.color = WithAlpha(ringColor, (1f - t) * ringColor.a);
            }

            if (t >= 1f)
            {
                RuntimePool.Release(gameObject);
            }
        }

        private void EmitFragments(
            Vector3 worldPosition,
            Vector3 direction,
            Color primary,
            Color secondary,
            int count,
            float directionalBias)
        {
            if (fragments == null || count <= 0)
            {
                return;
            }

            fragments.Clear(true);
            fragments.Play(true);

            Vector3 forward = direction;
            forward.y = 0f;
            bool hasDirection = forward.sqrMagnitude > 0.0001f;
            if (hasDirection)
            {
                forward.Normalize();
            }

            float bias = Mathf.Clamp01(directionalBias);
            for (int i = 0; i < count; i++)
            {
                float angle = Random.Range(-72f, 72f);
                Vector3 radial = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
                Vector3 biased = hasDirection
                    ? Quaternion.AngleAxis(angle, Vector3.up) * forward
                    : radial;
                Vector3 velocity = Vector3.Slerp(radial, biased, bias).normalized;
                velocity *= Random.Range(1.1f, 3.8f) * effectScale;
                velocity.y = Random.Range(0.2f, 1.25f) * Mathf.Sqrt(effectScale);

                ParticleSystem.EmitParams parameters = new ParticleSystem.EmitParams
                {
                    position = worldPosition + Random.insideUnitSphere * effectScale * 0.06f,
                    velocity = velocity,
                    startColor = Color.Lerp(primary, secondary, Random.value),
                    startLifetime = Random.Range(duration * 0.7f, duration * 1.25f),
                    startSize = Random.Range(0.06f, 0.18f) * effectScale,
                    rotation = Random.Range(0f, 360f)
                };
                fragments.Emit(parameters, 1);
            }
        }

        private void EmitMist(Vector3 worldPosition, Color primary, Color secondary, int count)
        {
            if (mist == null || count <= 0)
            {
                return;
            }

            mist.Clear(true);
            mist.Play(true);

            for (int i = 0; i < count; i++)
            {
                Vector3 velocity = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
                velocity *= Random.Range(0.15f, 0.8f) * effectScale;
                velocity.y = Random.Range(0.05f, 0.4f);

                Color color = Color.Lerp(primary, secondary, Random.Range(0.15f, 0.65f));
                color.a = Mathf.Min(color.a, Random.Range(0.12f, 0.32f));
                ParticleSystem.EmitParams parameters = new ParticleSystem.EmitParams
                {
                    position = worldPosition + Random.insideUnitSphere * effectScale * 0.1f,
                    velocity = velocity,
                    startColor = color,
                    startLifetime = Random.Range(duration * 0.9f, duration * 1.45f),
                    startSize = Random.Range(0.24f, 0.55f) * effectScale,
                    rotation = Random.Range(0f, 360f)
                };
                mist.Emit(parameters, 1);
            }
        }

        private void EnsureComponents()
        {
            if (fragments == null)
            {
                fragments = transform.Find("Fragments")?.GetComponent<ParticleSystem>();
            }
            if (mist == null)
            {
                mist = transform.Find("Mist")?.GetComponent<ParticleSystem>();
            }
            if (core == null)
            {
                core = transform.Find("Core")?.GetComponent<SpriteRenderer>();
            }
            if (ring == null)
            {
                ring = transform.Find("Ring")?.GetComponent<SpriteRenderer>();
            }
            if (autoReturn == null)
            {
                autoReturn = RuntimePool.EnsureAutoReturn(gameObject);
            }
        }

        private static ParticleSystem CreateParticleSystem(Transform parent, string objectName, int sortingOrder, bool soft)
        {
            GameObject particleObject = new GameObject(objectName);
            particleObject.transform.SetParent(parent, false);
            ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = soft ? 24 : 64;
            main.startSpeed = 0f;
            main.startLifetime = 0.3f;
            main.startSize = 0.1f;
            main.gravityModifier = soft ? -0.015f : 0.12f;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = false;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
                1f,
                soft
                    ? new AnimationCurve(new Keyframe(0f, 0.45f), new Keyframe(0.35f, 1f), new Keyframe(1f, 1.25f))
                    : new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.72f, 0.82f), new Keyframe(1f, 0f)));

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, soft ? 0.18f : 0.03f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            ParticleSystemRenderer renderer = particleObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortingOrder = sortingOrder;
            renderer.sharedMaterial = soft
                ? CombatVfxResources.GetMistMaterial()
                : CombatVfxResources.GetParticleMaterial();

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return particleSystem;
        }

        private static SpriteRenderer CreateSpriteRenderer(Transform parent, string name, Sprite sprite, int sortingOrder)
        {
            GameObject spriteObject = new GameObject(name);
            spriteObject.transform.SetParent(parent, false);
            SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.enabled = false;
            return renderer;
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
