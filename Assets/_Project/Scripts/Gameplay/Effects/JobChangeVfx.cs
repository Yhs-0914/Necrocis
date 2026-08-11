using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    internal sealed class JobChangeVfx : MonoBehaviour
    {
        private const int SymbolSortingOrder = 5350;
        private const int ParticleSortingOrder = 5380;
        private const int SymbolCount = 8;

        private readonly SpriteRenderer[] symbols = new SpriteRenderer[SymbolCount];

        private ParticleSystem particles;
        private RuntimePoolAutoReturn autoReturn;
        private Transform followTarget;
        private Vector3 followOffset;
        private Vector3 centerOffset;
        private Color primaryColor;
        private Color accentColor;
        private JobType currentJob;
        private float duration;
        private float elapsed;
        private float effectScale;

        public static GameObject CreateObject()
        {
            GameObject root = new GameObject("JobChangeFx");
            root.SetActive(false);

            JobChangeVfx effect = root.AddComponent<JobChangeVfx>();
            for (int i = 0; i < SymbolCount; i++)
            {
                effect.symbols[i] = CreateSpriteRenderer(
                    root.transform,
                    $"JobSymbol{i + 1}",
                    CombatVfxResources.GetStarSprite(),
                    SymbolSortingOrder + i);
            }

            effect.particles = CreateParticleSystem(root.transform);
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
            currentJob = job;
            primaryColor = primary;
            accentColor = accent;
            effectScale = Mathf.Clamp(scale, 0.65f, 1.55f);
            duration = job == JobType.Mage ? 1.35f : job == JobType.Warrior ? 0.88f : 1.02f;
            elapsed = 0f;

            transform.SetParent(null, false);
            transform.position = groundPosition;
            transform.rotation = Quaternion.identity;

            ConfigureSymbols(job);
            EmitJobParticles(groundPosition, job);
            autoReturn.Schedule(duration);
            enabled = true;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (followTarget != null)
            {
                transform.position = followTarget.position + followOffset;
            }
            transform.rotation = Quaternion.identity;

            switch (currentJob)
            {
                case JobType.Warrior:
                    UpdateWarrior(t);
                    break;
                case JobType.Mage:
                    UpdateMage(t);
                    break;
                case JobType.Archer:
                    UpdateArcher(t);
                    break;
            }

            if (t >= 1f)
            {
                RuntimePool.Release(gameObject);
            }
        }

        private void ConfigureSymbols(JobType job)
        {
            for (int i = 0; i < symbols.Length; i++)
            {
                SpriteRenderer symbol = symbols[i];
                if (symbol == null)
                {
                    continue;
                }

                symbol.enabled = false;
                symbol.transform.localPosition = centerOffset;
                symbol.transform.localScale = Vector3.one * 0.02f;
                symbol.transform.rotation = GetCameraRotation();
                symbol.color = WithAlpha(primaryColor, 0f);
            }

            if (job == JobType.Warrior)
            {
                EnableSymbol(0, CombatVfxResources.GetSlashSprite());
                EnableSymbol(1, CombatVfxResources.GetSlashSprite());
                EnableSymbol(2, CombatVfxResources.GetStarSprite());
            }
            else if (job == JobType.Mage)
            {
                EnableSymbol(0, CombatVfxResources.GetRuneSprite());
                EnableSymbol(1, CombatVfxResources.GetRuneSprite());
                for (int i = 2; i < 6; i++)
                {
                    EnableSymbol(i, CombatVfxResources.GetStarSprite());
                }
            }
            else if (job == JobType.Archer)
            {
                for (int i = 0; i < 7; i++)
                {
                    EnableSymbol(i, CombatVfxResources.GetArrowSprite());
                }
            }
        }

        private void UpdateWarrior(float t)
        {
            Quaternion cameraRotation = GetCameraRotation();
            for (int i = 0; i < 2; i++)
            {
                SpriteRenderer slash = symbols[i];
                if (slash == null)
                {
                    continue;
                }

                float delay = i * 0.13f;
                float slashT = Mathf.Clamp01((t - delay) / 0.55f);
                float sweep = 1f - Mathf.Pow(1f - slashT, 3f);
                float alpha = Mathf.Sin(slashT * Mathf.PI);
                float side = i == 0 ? -1f : 1f;
                slash.transform.localPosition = centerOffset
                    + GetCameraRight() * side * effectScale * Mathf.Lerp(0.5f, -0.1f, sweep)
                    + Vector3.up * effectScale * Mathf.Lerp(-0.28f, 0.32f, sweep);
                slash.transform.localScale = new Vector3(
                    effectScale * Mathf.Lerp(0.4f, 1.15f, sweep),
                    effectScale * Mathf.Lerp(0.3f, 0.82f, sweep),
                    1f);
                slash.transform.rotation = cameraRotation
                    * Quaternion.Euler(0f, 0f, i == 0 ? -24f : 114f);
                slash.color = WithAlpha(i == 0 ? primaryColor : accentColor, alpha);
            }

            SpriteRenderer impact = symbols[2];
            if (impact != null)
            {
                float impactT = Mathf.Clamp01((t - 0.24f) / 0.5f);
                float pulse = Mathf.Sin(impactT * Mathf.PI);
                impact.transform.localPosition = centerOffset;
                impact.transform.localScale = Vector3.one
                    * effectScale
                    * Mathf.Lerp(0.05f, 0.72f, pulse);
                impact.transform.rotation = cameraRotation
                    * Quaternion.Euler(0f, 0f, impactT * 110f);
                impact.color = WithAlpha(accentColor, pulse * 0.9f);
            }
        }

        private void UpdateMage(float t)
        {
            Quaternion cameraRotation = GetCameraRotation();
            float appear = Mathf.Clamp01(t / 0.16f);
            float fade = Mathf.Clamp01((1f - t) / 0.24f);
            float envelope = appear * fade;

            for (int i = 0; i < 2; i++)
            {
                SpriteRenderer rune = symbols[i];
                if (rune == null)
                {
                    continue;
                }

                float direction = i == 0 ? 1f : -1f;
                float size = i == 0
                    ? Mathf.Lerp(0.18f, 1.02f, Mathf.SmoothStep(0f, 1f, t))
                    : Mathf.Lerp(0.12f, 0.66f, Mathf.SmoothStep(0f, 1f, t));
                rune.transform.localPosition = centerOffset
                    + Vector3.up * effectScale * (i == 0 ? 0.05f : 0.18f);
                rune.transform.localScale = Vector3.one * effectScale * size;
                rune.transform.rotation = cameraRotation
                    * Quaternion.Euler(0f, 0f, direction * t * (i == 0 ? 210f : 320f));
                rune.color = WithAlpha(i == 0 ? primaryColor : accentColor, envelope * (i == 0 ? 0.82f : 0.62f));
            }

            for (int i = 2; i < 6; i++)
            {
                SpriteRenderer star = symbols[i];
                if (star == null)
                {
                    continue;
                }

                float angle = t * -260f + (i - 2) * 90f;
                float radians = angle * Mathf.Deg2Rad;
                float radius = effectScale * Mathf.Lerp(0.22f, 0.68f, Mathf.Sin(t * Mathf.PI));
                star.transform.localPosition = centerOffset
                    + GetCameraRight() * Mathf.Cos(radians) * radius
                    + Vector3.up * (Mathf.Sin(radians) * radius + effectScale * 0.15f);
                star.transform.localScale = Vector3.one
                    * effectScale
                    * (0.1f + Mathf.Sin(t * Mathf.PI) * 0.11f);
                star.transform.rotation = cameraRotation
                    * Quaternion.Euler(0f, 0f, -angle * 0.6f);
                star.color = WithAlpha(Color.Lerp(primaryColor, accentColor, (i - 2) / 3f), envelope);
            }
        }

        private void UpdateArcher(float t)
        {
            Quaternion cameraRotation = GetCameraRotation();
            Vector3 cameraRight = GetCameraRight();
            for (int i = 0; i < 7; i++)
            {
                SpriteRenderer arrow = symbols[i];
                if (arrow == null)
                {
                    continue;
                }

                float delay = i * 0.025f;
                float arrowT = Mathf.Clamp01((t - delay) / 0.76f);
                float launch = 1f - Mathf.Pow(1f - arrowT, 3f);
                float alpha = Mathf.Sin(arrowT * Mathf.PI);
                float angle = Mathf.Lerp(-58f, 58f, i / 6f);
                float radians = angle * Mathf.Deg2Rad;
                Vector3 direction = Vector3.up * Mathf.Cos(radians)
                    + cameraRight * Mathf.Sin(radians);
                arrow.transform.localPosition = centerOffset
                    - Vector3.up * effectScale * 0.22f
                    + direction * effectScale * Mathf.Lerp(0.05f, 1.3f, launch);
                arrow.transform.localScale = new Vector3(
                    effectScale * 0.22f,
                    effectScale * Mathf.Lerp(0.35f, 0.62f, alpha),
                    1f);
                arrow.transform.rotation = cameraRotation
                    * Quaternion.Euler(0f, 0f, -angle);
                arrow.color = WithAlpha(Color.Lerp(primaryColor, accentColor, i / 6f), alpha * 0.94f);
            }
        }

        private void EmitJobParticles(Vector3 groundPosition, JobType job)
        {
            if (particles == null)
            {
                return;
            }

            particles.Clear(true);
            particles.Play(true);
            int count = job == JobType.Mage ? 18 : 24;
            for (int i = 0; i < count; i++)
            {
                Vector3 velocity;
                Vector3 spawnPosition;
                if (job == JobType.Warrior)
                {
                    Vector3 radial = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
                    velocity = radial * Random.Range(1.6f, 3.2f) * effectScale
                        + Vector3.up * Random.Range(0.35f, 1.2f) * effectScale;
                    spawnPosition = groundPosition + centerOffset + radial * Random.Range(0.02f, 0.2f);
                }
                else if (job == JobType.Mage)
                {
                    Vector3 radial = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
                    velocity = radial * Random.Range(0.15f, 0.55f) * effectScale
                        + Vector3.up * Random.Range(0.6f, 1.5f) * effectScale;
                    spawnPosition = groundPosition + centerOffset + radial * Random.Range(0.25f, 0.62f) * effectScale;
                }
                else
                {
                    float angle = Random.Range(-55f, 55f) * Mathf.Deg2Rad;
                    velocity = (Vector3.up * Mathf.Cos(angle) + GetCameraRight() * Mathf.Sin(angle))
                        * Random.Range(1.5f, 3.1f) * effectScale;
                    spawnPosition = groundPosition + centerOffset - Vector3.up * effectScale * 0.2f;
                }

                ParticleSystem.EmitParams parameters = new ParticleSystem.EmitParams
                {
                    position = spawnPosition,
                    velocity = velocity,
                    startColor = Color.Lerp(primaryColor, accentColor, Random.Range(0.12f, 0.9f)),
                    startLifetime = Random.Range(0.42f, 0.92f),
                    startSize = Random.Range(0.045f, 0.13f) * effectScale,
                    rotation = Random.Range(0f, 360f)
                };
                particles.Emit(parameters, 1);
            }
        }

        private void EnableSymbol(int index, Sprite sprite)
        {
            if (index < 0 || index >= symbols.Length || symbols[index] == null)
            {
                return;
            }

            symbols[index].sprite = sprite;
            symbols[index].enabled = true;
        }

        private void EnsureComponents()
        {
            if (particles == null)
            {
                particles = transform.Find("JobParticles")?.GetComponent<ParticleSystem>();
            }
            if (autoReturn == null)
            {
                autoReturn = RuntimePool.EnsureAutoReturn(gameObject);
            }
            for (int i = 0; i < SymbolCount; i++)
            {
                if (symbols[i] == null)
                {
                    symbols[i] = transform.Find($"JobSymbol{i + 1}")?.GetComponent<SpriteRenderer>();
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
            GameObject particleObject = new GameObject("JobParticles");
            particleObject.transform.SetParent(parent, false);
            ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 40;
            main.startSpeed = 0f;
            main.startLifetime = 0.7f;
            main.startSize = 0.1f;
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
                    new Keyframe(0.72f, 0.72f),
                    new Keyframe(1f, 0f)));

            ParticleSystemRenderer renderer = particleObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.alignment = ParticleSystemRenderSpace.Velocity;
            renderer.lengthScale = 1.6f;
            renderer.velocityScale = 0.18f;
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
