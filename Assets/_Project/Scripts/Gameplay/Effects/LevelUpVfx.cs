using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    internal sealed class LevelUpVfx : MonoBehaviour
    {
        private const int ShaftSortingOrder = 5280;
        private const int ChevronSortingOrder = 5290;
        private const int CrownSortingOrder = 5310;
        private const int ParticleSortingOrder = 5320;
        private const int ShaftCount = 2;
        private const int ChevronCount = 4;

        private readonly SpriteRenderer[] shafts = new SpriteRenderer[ShaftCount];
        private readonly SpriteRenderer[] chevrons = new SpriteRenderer[ChevronCount];

        private SpriteRenderer crown;
        private ParticleSystem motes;
        private RuntimePoolAutoReturn autoReturn;
        private Transform followTarget;
        private Vector3 followOffset;
        private Vector3 centerOffset;
        private float duration;
        private float elapsed;
        private float effectScale;

        private readonly Color energyColor = new Color(0.96f, 0.08f, 0.3f, 0.95f);
        private readonly Color ascentColor = new Color(1f, 0.48f, 0.1f, 0.98f);
        private readonly Color highlightColor = new Color(1f, 0.96f, 0.68f, 1f);

        public static GameObject CreateObject()
        {
            GameObject root = new GameObject("LevelUpFx");
            root.SetActive(false);

            LevelUpVfx effect = root.AddComponent<LevelUpVfx>();
            for (int i = 0; i < ShaftCount; i++)
            {
                effect.shafts[i] = CreateSpriteRenderer(
                    root.transform,
                    $"LightShaft{i + 1}",
                    CombatVfxResources.GetSoftCircleSprite(),
                    ShaftSortingOrder + i);
            }

            for (int i = 0; i < ChevronCount; i++)
            {
                effect.chevrons[i] = CreateSpriteRenderer(
                    root.transform,
                    $"AscendMark{i + 1}",
                    CombatVfxResources.GetChevronSprite(),
                    ChevronSortingOrder + i);
            }

            effect.crown = CreateSpriteRenderer(
                root.transform,
                "LevelCrown",
                CombatVfxResources.GetStarSprite(),
                CrownSortingOrder);
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
            duration = 1.28f;
            elapsed = 0f;
            effectScale = Mathf.Clamp(scale, 0.65f, 1.55f);

            transform.SetParent(null, false);
            transform.position = groundPosition;
            transform.rotation = Quaternion.identity;

            for (int i = 0; i < shafts.Length; i++)
            {
                SpriteRenderer shaft = shafts[i];
                if (shaft == null)
                {
                    continue;
                }

                shaft.enabled = true;
                shaft.transform.localPosition = centerOffset;
                shaft.transform.localScale = new Vector3(
                    effectScale * (i == 0 ? 0.34f : 0.18f),
                    effectScale * (i == 0 ? 2.2f : 2.8f),
                    1f);
                shaft.color = WithAlpha(i == 0 ? energyColor : highlightColor, 0f);
            }

            for (int i = 0; i < chevrons.Length; i++)
            {
                SpriteRenderer chevron = chevrons[i];
                if (chevron == null)
                {
                    continue;
                }

                chevron.enabled = true;
                chevron.transform.localPosition = centerOffset - Vector3.up * effectScale * 0.65f;
                chevron.transform.localScale = Vector3.one * effectScale * 0.34f;
                chevron.color = WithAlpha(Color.Lerp(ascentColor, highlightColor, i * 0.22f), 0f);
            }

            crown.enabled = true;
            crown.transform.localPosition = centerOffset + Vector3.up * effectScale * 0.82f;
            crown.transform.localScale = Vector3.one * 0.02f;
            crown.color = WithAlpha(highlightColor, 0f);

            ApplyElementOrientations();
            EmitMotes(groundPosition);
            autoReturn.Schedule(duration);
            enabled = true;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float appear = Mathf.Clamp01(t / 0.12f);
            float fade = Mathf.Clamp01((1f - t) / 0.25f);
            float envelope = appear * fade;

            if (followTarget != null)
            {
                transform.position = followTarget.position + followOffset;
            }
            transform.rotation = Quaternion.identity;
            ApplyElementOrientations();

            UpdateShafts(t, envelope);
            UpdateChevrons(t);
            UpdateCrown(t);

            if (t >= 1f)
            {
                RuntimePool.Release(gameObject);
            }
        }

        private void UpdateShafts(float t, float envelope)
        {
            Vector3 cameraRight = GetCameraRight();
            for (int i = 0; i < shafts.Length; i++)
            {
                SpriteRenderer shaft = shafts[i];
                if (shaft == null)
                {
                    continue;
                }

                float side = i == 0 ? -1f : 1f;
                float rise = Mathf.SmoothStep(0f, 1f, t);
                shaft.transform.position = transform.position
                    + centerOffset
                    + cameraRight * side * effectScale * 0.22f
                    + Vector3.up * effectScale * Mathf.Lerp(0.1f, 0.62f, rise);
                shaft.transform.localScale = new Vector3(
                    effectScale * Mathf.Lerp(i == 0 ? 0.34f : 0.19f, 0.075f, rise),
                    effectScale * Mathf.Lerp(i == 0 ? 2.15f : 2.65f, 3.2f, rise),
                    1f);
                Color color = i == 0 ? energyColor : highlightColor;
                shaft.color = WithAlpha(color, envelope * (1f - t) * (i == 0 ? 0.38f : 0.24f));
            }
        }

        private void UpdateChevrons(float t)
        {
            for (int i = 0; i < chevrons.Length; i++)
            {
                SpriteRenderer chevron = chevrons[i];
                if (chevron == null)
                {
                    continue;
                }

                float delay = i * 0.09f;
                float markT = Mathf.Clamp01((t - delay) / Mathf.Max(0.01f, 0.7f - delay));
                float rise = 1f - Mathf.Pow(1f - markT, 2f);
                float alpha = Mathf.Sin(markT * Mathf.PI);
                chevron.transform.localPosition = centerOffset
                    + Vector3.up * effectScale * Mathf.Lerp(-0.68f, 1.72f, rise);
                chevron.transform.localScale = new Vector3(
                    effectScale * Mathf.Lerp(0.36f, 0.58f, alpha),
                    effectScale * Mathf.Lerp(0.2f, 0.34f, alpha),
                    1f);
                chevron.color = WithAlpha(
                    Color.Lerp(ascentColor, highlightColor, i * 0.22f),
                    alpha * (0.9f - i * 0.1f));
            }
        }

        private void UpdateCrown(float t)
        {
            if (crown == null)
            {
                return;
            }

            float crownT = Mathf.Clamp01((t - 0.28f) / 0.55f);
            float pop = Mathf.Sin(crownT * Mathf.PI);
            float settle = 1f - Mathf.Pow(1f - crownT, 3f);
            crown.transform.localPosition = centerOffset
                + Vector3.up * effectScale * Mathf.Lerp(0.62f, 1.05f, settle);
            crown.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.04f, 0.56f, pop);
            crown.transform.rotation = GetCameraRotation()
                * Quaternion.Euler(0f, 0f, t * 160f);
            crown.color = WithAlpha(highlightColor, pop);
        }

        private void ApplyElementOrientations()
        {
            Quaternion rotation = GetCameraRotation();
            for (int i = 0; i < shafts.Length; i++)
            {
                if (shafts[i] != null)
                {
                    shafts[i].transform.rotation = rotation;
                }
            }
            for (int i = 0; i < chevrons.Length; i++)
            {
                if (chevrons[i] != null)
                {
                    chevrons[i].transform.rotation = rotation;
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
            for (int i = 0; i < 24; i++)
            {
                Vector3 offset = GetCameraRight() * Random.Range(-0.36f, 0.36f) * effectScale;
                ParticleSystem.EmitParams parameters = new ParticleSystem.EmitParams
                {
                    position = groundPosition + offset + Vector3.up * Random.Range(0.05f, 0.55f) * effectScale,
                    velocity = Vector3.up * Random.Range(1.7f, 3.9f) * effectScale,
                    startColor = Color.Lerp(energyColor, highlightColor, Random.Range(0.2f, 0.9f)),
                    startLifetime = Random.Range(0.55f, 1.05f),
                    startSize = Random.Range(0.05f, 0.13f) * effectScale,
                    rotation = Random.Range(0f, 360f)
                };
                motes.Emit(parameters, 1);
            }
        }

        private void EnsureComponents()
        {
            if (crown == null)
            {
                crown = transform.Find("LevelCrown")?.GetComponent<SpriteRenderer>();
            }
            if (motes == null)
            {
                motes = transform.Find("Motes")?.GetComponent<ParticleSystem>();
            }
            if (autoReturn == null)
            {
                autoReturn = RuntimePool.EnsureAutoReturn(gameObject);
            }
            for (int i = 0; i < ShaftCount; i++)
            {
                if (shafts[i] == null)
                {
                    shafts[i] = transform.Find($"LightShaft{i + 1}")?.GetComponent<SpriteRenderer>();
                }
            }
            for (int i = 0; i < ChevronCount; i++)
            {
                if (chevrons[i] == null)
                {
                    chevrons[i] = transform.Find($"AscendMark{i + 1}")?.GetComponent<SpriteRenderer>();
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
            main.maxParticles = 40;
            main.startSpeed = 0f;
            main.startLifetime = 0.8f;
            main.startSize = 0.1f;
            main.gravityModifier = -0.06f;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = false;
            ParticleSystem.SizeOverLifetimeModule size = particleSystem.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(
                1f,
                new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(0.16f, 1f),
                    new Keyframe(0.78f, 0.62f),
                    new Keyframe(1f, 0f)));

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

        private static Vector3 GetCameraRight()
        {
            Camera camera = DontStarveCamera.GetActiveCamera();
            return camera != null ? camera.transform.right : Vector3.right;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }
    }
}
