using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    internal sealed class ItemPickupVfx : MonoBehaviour
    {
        private const int BackRingSortingOrder = 5230;
        private const int BeamSortingOrder = 5240;
        private const int FrontRingSortingOrder = 5250;
        private const int GlintSortingOrder = 5260;
        private const int ParticleSortingOrder = 5270;

        private SpriteRenderer backRing;
        private SpriteRenderer frontRing;
        private SpriteRenderer beam;
        private SpriteRenderer glint;
        private ParticleSystem motes;
        private RuntimePoolAutoReturn autoReturn;

        private Color primaryColor;
        private Color accentColor;
        private Vector3 visualOffset;
        private float duration;
        private float elapsed;
        private float effectScale;

        public static GameObject CreateObject()
        {
            GameObject root = new GameObject("ItemPickupFx");
            root.SetActive(false);

            ItemPickupVfx effect = root.AddComponent<ItemPickupVfx>();
            effect.backRing = CreateSpriteRenderer(
                root.transform,
                "BackRing",
                CombatVfxResources.GetRingSprite(),
                BackRingSortingOrder);
            effect.beam = CreateSpriteRenderer(
                root.transform,
                "Beam",
                CombatVfxResources.GetSoftCircleSprite(),
                BeamSortingOrder);
            effect.frontRing = CreateSpriteRenderer(
                root.transform,
                "FrontRing",
                CombatVfxResources.GetRingSprite(),
                FrontRingSortingOrder);
            effect.glint = CreateSpriteRenderer(
                root.transform,
                "Glint",
                CombatVfxResources.GetSoftCircleSprite(),
                GlintSortingOrder);
            effect.motes = CreateMoteSystem(root.transform);
            effect.autoReturn = RuntimePool.EnsureAutoReturn(root);
            return root;
        }

        public void Show(
            Vector3 groundPosition,
            Vector3 visualPosition,
            Color primary,
            Color accent,
            float scale)
        {
            EnsureComponents();

            transform.SetParent(null, false);
            transform.position = groundPosition;
            transform.rotation = Quaternion.identity;

            primaryColor = primary;
            accentColor = accent;
            visualOffset = visualPosition - groundPosition;
            duration = 0.68f;
            elapsed = 0f;
            effectScale = Mathf.Clamp(scale, 0.65f, 1.8f);

            backRing.enabled = true;
            frontRing.enabled = true;
            beam.enabled = true;
            glint.enabled = true;

            backRing.transform.localScale = Vector3.one * effectScale * 0.28f;
            backRing.transform.localPosition = Vector3.up * 0.025f;
            frontRing.transform.localScale = Vector3.one * effectScale * 0.16f;
            frontRing.transform.localPosition = Vector3.up * 0.045f;
            beam.transform.localPosition = visualOffset + Vector3.up * effectScale * 0.18f;
            beam.transform.localScale = new Vector3(
                effectScale * 0.3f,
                effectScale * 2.25f,
                1f);
            glint.transform.localPosition = visualOffset;
            glint.transform.localScale = Vector3.one * effectScale * 0.16f;

            backRing.color = WithAlpha(primaryColor, 0.72f);
            frontRing.color = WithAlpha(accentColor, 0.95f);
            beam.color = WithAlpha(primaryColor, 0.38f);
            glint.color = WithAlpha(accentColor, 1f);

            ApplyElementOrientations();
            EmitMotes(visualPosition);
            autoReturn.Schedule(duration);
            enabled = true;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float expand = 1f - Mathf.Pow(1f - t, 3f);
            float fade = 1f - t;
            float glintPulse = Mathf.Sin(Mathf.Clamp01(t * 1.45f) * Mathf.PI);

            transform.rotation = Quaternion.identity;
            ApplyElementOrientations();

            if (backRing != null)
            {
                backRing.transform.localScale = Vector3.one
                    * effectScale
                    * Mathf.Lerp(0.28f, 1.55f, expand);
                backRing.color = WithAlpha(primaryColor, fade * fade * 0.72f);
            }

            if (frontRing != null)
            {
                frontRing.transform.localScale = Vector3.one
                    * effectScale
                    * Mathf.Lerp(0.16f, 1.05f, expand);
                frontRing.color = WithAlpha(accentColor, fade * 0.95f);
            }

            if (beam != null)
            {
                float beamWidth = Mathf.Lerp(0.3f, 0.08f, expand);
                float beamHeight = Mathf.Lerp(2.25f, 2.75f, expand);
                beam.transform.localScale = new Vector3(
                    effectScale * beamWidth,
                    effectScale * beamHeight,
                    1f);
                beam.transform.localPosition = visualOffset
                    + Vector3.up * effectScale * Mathf.Lerp(0.1f, 0.38f, expand);
                beam.color = WithAlpha(primaryColor, fade * fade * 0.38f);
            }

            if (glint != null)
            {
                glint.transform.localScale = Vector3.one
                    * effectScale
                    * Mathf.Lerp(0.16f, 0.72f, glintPulse);
                glint.transform.localPosition = visualOffset;
                glint.color = WithAlpha(accentColor, glintPulse);
            }

            if (t >= 1f)
            {
                RuntimePool.Release(gameObject);
            }
        }

        private void ApplyElementOrientations()
        {
            Quaternion billboardRotation = GetCameraRotation();

            if (backRing != null)
            {
                backRing.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            if (frontRing != null)
            {
                frontRing.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            if (beam != null)
            {
                beam.transform.rotation = billboardRotation;
            }
            if (glint != null)
            {
                glint.transform.rotation = billboardRotation;
            }
        }

        private void EmitMotes(Vector3 worldPosition)
        {
            if (motes == null)
            {
                return;
            }

            motes.Clear(true);
            motes.Play(true);

            for (int i = 0; i < 16; i++)
            {
                Vector3 radial = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up)
                    * Vector3.forward;
                Vector3 velocity = radial * Random.Range(0.45f, 1.55f) * effectScale;
                velocity.y = Random.Range(1.2f, 2.8f) * effectScale;

                Color moteColor = Color.Lerp(primaryColor, accentColor, Random.Range(0.25f, 0.85f));
                ParticleSystem.EmitParams parameters = new ParticleSystem.EmitParams
                {
                    position = worldPosition
                        + radial * Random.Range(0.04f, 0.22f) * effectScale,
                    velocity = velocity,
                    startColor = moteColor,
                    startLifetime = Random.Range(0.38f, 0.72f),
                    startSize = Random.Range(0.055f, 0.15f) * effectScale,
                    rotation = Random.Range(0f, 360f)
                };
                motes.Emit(parameters, 1);
            }
        }

        private void EnsureComponents()
        {
            if (backRing == null)
            {
                backRing = transform.Find("BackRing")?.GetComponent<SpriteRenderer>();
            }
            if (frontRing == null)
            {
                frontRing = transform.Find("FrontRing")?.GetComponent<SpriteRenderer>();
            }
            if (beam == null)
            {
                beam = transform.Find("Beam")?.GetComponent<SpriteRenderer>();
            }
            if (glint == null)
            {
                glint = transform.Find("Glint")?.GetComponent<SpriteRenderer>();
            }
            if (motes == null)
            {
                motes = transform.Find("Motes")?.GetComponent<ParticleSystem>();
            }
            if (autoReturn == null)
            {
                autoReturn = RuntimePool.EnsureAutoReturn(gameObject);
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
            main.maxParticles = 32;
            main.startSpeed = 0f;
            main.startLifetime = 0.6f;
            main.startSize = 0.1f;
            main.gravityModifier = -0.08f;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = false;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
                1f,
                new AnimationCurve(
                    new Keyframe(0f, 0.25f),
                    new Keyframe(0.18f, 1f),
                    new Keyframe(0.78f, 0.7f),
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
