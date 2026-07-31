using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    internal sealed class JobChangeVfx : MonoBehaviour
    {
        private const int AuraSortingOrder = 5330;
        private const int RingSortingOrder = 5340;
        private const int ShardSortingOrder = 5350;
        private const int CoreSortingOrder = 5370;
        private const int ParticleSortingOrder = 5380;
        private const int RingCount = 2;
        private const int ShardCount = 8;

        private readonly SpriteRenderer[] rings = new SpriteRenderer[RingCount];
        private readonly SpriteRenderer[] shards = new SpriteRenderer[ShardCount];

        private SpriteRenderer groundAura;
        private SpriteRenderer core;
        private ParticleSystem motes;
        private RuntimePoolAutoReturn autoReturn;

        private Transform followTarget;
        private Vector3 followOffset;
        private Vector3 centerOffset;
        private Color primaryColor;
        private Color accentColor;
        private float spinSpeed;
        private float shardLength;
        private float duration;
        private float elapsed;
        private float effectScale;

        public static GameObject CreateObject()
        {
            GameObject root = new GameObject("JobChangeFx");
            root.SetActive(false);

            JobChangeVfx effect = root.AddComponent<JobChangeVfx>();
            effect.groundAura = CreateSpriteRenderer(
                root.transform,
                "GroundAura",
                CombatVfxResources.GetSoftCircleSprite(),
                AuraSortingOrder);

            for (int i = 0; i < RingCount; i++)
            {
                effect.rings[i] = CreateSpriteRenderer(
                    root.transform,
                    $"JobRing{i + 1}",
                    CombatVfxResources.GetRingSprite(),
                    RingSortingOrder + i);
            }

            for (int i = 0; i < ShardCount; i++)
            {
                effect.shards[i] = CreateSpriteRenderer(
                    root.transform,
                    $"RisingShard{i + 1}",
                    CombatVfxResources.GetSoftCircleSprite(),
                    ShardSortingOrder + i);
            }

            effect.core = CreateSpriteRenderer(
                root.transform,
                "TransformationFlash",
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
            JobType job,
            Color primary,
            Color accent)
        {
            EnsureComponents();

            followTarget = target;
            followOffset = target != null ? groundPosition - target.position : Vector3.zero;
            centerOffset = center - groundPosition;
            primaryColor = primary;
            accentColor = accent;
            ConfigureJobMotion(job);

            transform.SetParent(null, false);
            transform.position = groundPosition;
            transform.rotation = Quaternion.identity;

            duration = 1.32f;
            elapsed = 0f;
            effectScale = Mathf.Clamp(scale, 0.65f, 1.55f);

            groundAura.enabled = true;
            groundAura.transform.localPosition = Vector3.up * 0.018f;
            groundAura.transform.localScale = Vector3.one * effectScale * 0.18f;
            groundAura.color = WithAlpha(primaryColor, 0f);

            for (int i = 0; i < rings.Length; i++)
            {
                SpriteRenderer ring = rings[i];
                if (ring == null)
                {
                    continue;
                }

                ring.enabled = true;
                ring.transform.localPosition = Vector3.up * (0.026f + i * 0.014f);
                ring.transform.localScale = Vector3.one * effectScale * 0.1f;
                ring.color = WithAlpha(
                    Color.Lerp(primaryColor, accentColor, i * 0.65f),
                    0f);
            }

            for (int i = 0; i < shards.Length; i++)
            {
                SpriteRenderer shard = shards[i];
                if (shard == null)
                {
                    continue;
                }

                shard.enabled = true;
                shard.transform.localPosition = centerOffset;
                shard.transform.localScale = new Vector3(
                    effectScale * 0.08f,
                    effectScale * shardLength,
                    1f);
                shard.color = WithAlpha(
                    Color.Lerp(primaryColor, accentColor, i / (ShardCount - 1f)),
                    0f);
            }

            core.enabled = true;
            core.transform.localPosition = centerOffset;
            core.transform.localScale = Vector3.one * effectScale * 0.16f;
            core.color = WithAlpha(accentColor, 0f);

            ApplyElementOrientations();
            EmitMotes(groundPosition, job);
            autoReturn.Schedule(duration);
            enabled = true;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float appear = Mathf.Clamp01(t / 0.12f);
            float fade = Mathf.Clamp01((1f - t) / 0.28f);
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
            UpdateShards(t, envelope, expand);
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

            float size = Mathf.Lerp(0.18f, 0.68f, expand);
            groundAura.transform.localScale = Vector3.one * effectScale * size;
            groundAura.color = WithAlpha(primaryColor, envelope * (1f - t) * 0.22f);
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

                float delay = i * 0.16f;
                float ringT = Mathf.Clamp01((t - delay) / Mathf.Max(0.01f, 0.82f - delay));
                float ringExpand = 1f - Mathf.Pow(1f - ringT, 3f);
                float ringFade = Mathf.Sin(ringT * Mathf.PI);
                float size = Mathf.Lerp(0.1f, 0.74f - i * 0.08f, ringExpand);

                ring.transform.localScale = Vector3.one * effectScale * size;
                Color ringColor = Color.Lerp(primaryColor, accentColor, i * 0.65f);
                ring.color = WithAlpha(ringColor, ringFade * (0.88f - i * 0.12f));
            }
        }

        private void UpdateShards(float t, float envelope, float expand)
        {
            float angleOffset = Time.unscaledTime * spinSpeed;
            float radius = effectScale * Mathf.Lerp(0.46f, 0.2f, expand);

            for (int i = 0; i < shards.Length; i++)
            {
                SpriteRenderer shard = shards[i];
                if (shard == null)
                {
                    continue;
                }

                float delay = i * 0.025f;
                float shardT = Mathf.Clamp01((t - delay) / Mathf.Max(0.01f, 0.88f - delay));
                float angle = angleOffset + i * (360f / ShardCount);
                float radians = angle * Mathf.Deg2Rad;
                float rise = centerOffset.y
                    + Mathf.Lerp(-0.5f, 1.0f, shardT) * effectScale;

                shard.transform.localPosition = new Vector3(
                    Mathf.Cos(radians) * radius,
                    rise,
                    Mathf.Sin(radians) * radius);

                float shardPulse = Mathf.Sin(shardT * Mathf.PI);
                shard.transform.localScale = new Vector3(
                    effectScale * Mathf.Lerp(0.045f, 0.095f, shardPulse),
                    effectScale * shardLength * Mathf.Lerp(0.55f, 1f, shardPulse),
                    1f);
                Color shardColor = Color.Lerp(primaryColor, accentColor, (i % 3) * 0.35f);
                shard.color = WithAlpha(shardColor, envelope * shardPulse * 0.9f);
            }
        }

        private void UpdateCore(float t, float envelope)
        {
            if (core == null)
            {
                return;
            }

            float pulse = Mathf.Sin(Mathf.Clamp01(t * 1.55f) * Mathf.PI);
            core.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.16f, 0.82f, pulse);
            core.color = WithAlpha(accentColor, pulse * envelope * 0.92f);
        }

        private void ConfigureJobMotion(JobType job)
        {
            switch (job)
            {
                case JobType.Warrior:
                    spinSpeed = 72f;
                    shardLength = 0.38f;
                    break;
                case JobType.Mage:
                    spinSpeed = -185f;
                    shardLength = 0.5f;
                    break;
                case JobType.Archer:
                    spinSpeed = 132f;
                    shardLength = 0.44f;
                    break;
                default:
                    spinSpeed = 100f;
                    shardLength = 0.42f;
                    break;
            }
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

            for (int i = 0; i < shards.Length; i++)
            {
                if (shards[i] != null)
                {
                    shards[i].transform.rotation = billboardRotation;
                }
            }
        }

        private void EmitMotes(Vector3 groundPosition, JobType job)
        {
            if (motes == null)
            {
                return;
            }

            motes.Clear(true);
            motes.Play(true);

            int count = job == JobType.Mage ? 38 : 32;
            for (int i = 0; i < count; i++)
            {
                Vector3 radial = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up)
                    * Vector3.forward;
                float outwardSpeed = job == JobType.Warrior
                    ? Random.Range(0.55f, 1.35f)
                    : Random.Range(0.25f, 0.85f);
                Vector3 velocity = radial * outwardSpeed * effectScale;
                velocity.y = Random.Range(1.5f, 3.8f) * effectScale;

                Color moteColor = Color.Lerp(primaryColor, accentColor, Random.Range(0.15f, 0.9f));
                ParticleSystem.EmitParams parameters = new ParticleSystem.EmitParams
                {
                    position = groundPosition
                        + radial * Random.Range(0.08f, 0.42f) * effectScale
                        + Vector3.up * Random.Range(0.04f, 0.3f),
                    velocity = velocity,
                    startColor = moteColor,
                    startLifetime = Random.Range(0.65f, 1.15f),
                    startSize = Random.Range(0.065f, 0.17f) * effectScale,
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
                core = transform.Find("TransformationFlash")?.GetComponent<SpriteRenderer>();
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
                    rings[i] = transform.Find($"JobRing{i + 1}")?.GetComponent<SpriteRenderer>();
                }
            }

            for (int i = 0; i < ShardCount; i++)
            {
                if (shards[i] == null)
                {
                    shards[i] = transform.Find($"RisingShard{i + 1}")?.GetComponent<SpriteRenderer>();
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
            main.maxParticles = 56;
            main.startSpeed = 0f;
            main.startLifetime = 0.9f;
            main.startSize = 0.12f;
            main.gravityModifier = -0.1f;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = false;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
                1f,
                new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(0.12f, 1f),
                    new Keyframe(0.74f, 0.72f),
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
}
