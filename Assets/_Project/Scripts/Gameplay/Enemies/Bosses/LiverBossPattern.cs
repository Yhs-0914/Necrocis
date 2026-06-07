using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [System.Serializable]
    public class LiverBossPatternSettings
    {
        [Header("Phase")]
        [Range(0.1f, 0.9f)] public float phase2HealthRatio = 0.5f;

        [Header("Stats")]
        public float phase1AttackDamage = 2f;
        public float phase1MoveSpeed = 1f;
        public float phase2AttackDamage = 2f;
        public float phase2MoveSpeed = 1.5f;

        [Header("Blood Bomb")]
        public float bloodBombRange = 7f;
        public float bloodBombExplosionRadius = 2.5f;
        public float bloodBombFuseTime = 1f;
        public float bloodBombCooldown = 4f;
        [Range(0f, 1f)] public float attackPowerDebuffRatio = 0.15f;
        public float attackPowerDebuffDuration = 5f;
        public float bloodBombArcHeight = 1.4f;
        public float bloodBombProjectileScale = 1.15f;
        public Sprite bloodBombSprite;

        [Header("Healing Pose")]
        public float healingPosePreDelay = 1f;
        public float healingPoseCooldown = 15f;
        public float healingPoseDuration = 5f;
        [Range(0f, 1f)] public float damageHealRatio = 0.7f;

        [Header("Movement")]
        public float phase1PreferredDistanceRatio = 0.72f;
        public float phase2PreferredDistance = 2.6f;
        public float roamRadius = 9f;

        [Header("Temporary Visuals")]
        public Color phase1Color = new Color(0.78f, 0.08f, 0.12f, 1f);
        public Color phase2Color = new Color(0.52f, 0.05f, 0.1f, 1f);
        public Color healingPoseColor = new Color(1f, 0.38f, 0.52f, 1f);
        public Color bloodBombColor = new Color(0.7f, 0f, 0.04f, 0.96f);
        public Color explosionColor = new Color(0.9f, 0.05f, 0.08f, 0.45f);

        [Header("Debug")]
        public bool startInPhase2ForDebug = false;
        public bool useFastPatternCooldownsForDebug = false;
        public float fastPatternCooldown = 1f;
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public class LiverBossPattern : MonoBehaviour, IBossPatternTempSpriteOwner
    {
        private enum BossPhase
        {
            Phase1,
            Transition,
            Phase2
        }

        [Header("Phase")]
        [SerializeField, Range(0.1f, 0.9f)] private float phase2HealthRatio = 0.5f;

        [Header("Stats")]
        [SerializeField] private float phase1AttackDamage = 2f;
        [SerializeField] private float phase1MoveSpeed = 1f;
        [SerializeField] private float phase2AttackDamage = 2f;
        [SerializeField] private float phase2MoveSpeed = 1.5f;

        [Header("Blood Bomb")]
        [SerializeField] private float bloodBombRange = 7f;
        [SerializeField] private float bloodBombExplosionRadius = 2.5f;
        [SerializeField] private float bloodBombFuseTime = 1f;
        [SerializeField] private float bloodBombCooldown = 4f;
        [SerializeField, Range(0f, 1f)] private float attackPowerDebuffRatio = 0.15f;
        [SerializeField] private float attackPowerDebuffDuration = 5f;
        [SerializeField] private float bloodBombArcHeight = 1.4f;
        [SerializeField] private float bloodBombProjectileScale = 1.15f;
        [SerializeField] private Sprite bloodBombSpriteAsset;

        [Header("Healing Pose")]
        [SerializeField] private float healingPosePreDelay = 1f;
        [SerializeField] private float healingPoseCooldown = 15f;
        [SerializeField] private float healingPoseDuration = 5f;
        [SerializeField, Range(0f, 1f)] private float damageHealRatio = 0.7f;

        [Header("Movement")]
        [SerializeField] private float phase1PreferredDistanceRatio = 0.72f;
        [SerializeField] private float phase2PreferredDistance = 2.6f;
        [SerializeField] private float roamRadius = 9f;

        [Header("Temporary Visuals")]
        [SerializeField] private Color phase1Color = new Color(0.78f, 0.08f, 0.12f, 1f);
        [SerializeField] private Color phase2Color = new Color(0.52f, 0.05f, 0.1f, 1f);
        [SerializeField] private Color healingPoseColor = new Color(1f, 0.38f, 0.52f, 1f);
        [SerializeField] private Color bloodBombColor = new Color(0.7f, 0f, 0.04f, 0.96f);
        [SerializeField] private Color explosionColor = new Color(0.9f, 0.05f, 0.08f, 0.45f);

        [Header("Debug")]
        [SerializeField] private bool startInPhase2ForDebug = false;
        [SerializeField] private bool useFastPatternCooldownsForDebug = false;
        [SerializeField] private float fastPatternCooldown = 1f;

        private static Sprite bloodBombSprite;
        private static Sprite explosionSprite;

        private EnemyController boss;
        private CharacterStats stats;
        private SpriteRenderer visualRenderer;
        private Vector3 anchorPosition;
        private Vector3 baseScale = Vector3.one;
        private BossPhase phase;
        private float nextBloodBombTime;
        private float nextHealingPoseTime;
        private bool actionRunning;
        private bool healingPoseActive;
        private bool encounterActive;
        private readonly List<GameObject> activeTempObjects = new List<GameObject>();

        public void Initialize(EnemyController controller, Vector3 anchor, Transform parent, LiverBossPatternSettings settings = null)
        {
            ApplySettings(settings);

            boss = controller != null ? controller : GetComponent<EnemyController>();
            stats = boss != null ? boss.Stats : GetComponent<CharacterStats>();
            anchorPosition = anchor;
            phase = startInPhase2ForDebug ? BossPhase.Phase2 : BossPhase.Phase1;
            nextBloodBombTime = Time.time + 1f;
            nextHealingPoseTime = phase == BossPhase.Phase2 ? Time.time + GetHealingPoseCooldown() : float.PositiveInfinity;
            actionRunning = false;
            healingPoseActive = false;
            encounterActive = false;
            baseScale = transform.localScale;
            visualRenderer = GetComponentInChildren<SpriteRenderer>();

            if (boss != null)
            {
                boss.SetAiSuppressed(true);
                boss.DamageTaken -= HandleBossDamageTaken;
                boss.DamageTaken += HandleBossDamageTaken;
                boss.Defeated -= HandleBossDefeated;
                boss.Defeated += HandleBossDefeated;
            }

            ApplyPhaseStats();
            ApplyPhaseVisual();
            enabled = true;
        }

        private void ApplySettings(LiverBossPatternSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            phase2HealthRatio = settings.phase2HealthRatio;
            phase1AttackDamage = settings.phase1AttackDamage;
            phase1MoveSpeed = settings.phase1MoveSpeed;
            phase2AttackDamage = settings.phase2AttackDamage;
            phase2MoveSpeed = settings.phase2MoveSpeed;
            bloodBombRange = settings.bloodBombRange;
            bloodBombExplosionRadius = settings.bloodBombExplosionRadius;
            bloodBombFuseTime = settings.bloodBombFuseTime;
            bloodBombCooldown = settings.bloodBombCooldown;
            attackPowerDebuffRatio = settings.attackPowerDebuffRatio;
            attackPowerDebuffDuration = settings.attackPowerDebuffDuration;
            bloodBombArcHeight = settings.bloodBombArcHeight;
            bloodBombProjectileScale = settings.bloodBombProjectileScale;
            bloodBombSpriteAsset = settings.bloodBombSprite;
            healingPosePreDelay = settings.healingPosePreDelay;
            healingPoseCooldown = settings.healingPoseCooldown;
            healingPoseDuration = settings.healingPoseDuration;
            damageHealRatio = settings.damageHealRatio;
            phase1PreferredDistanceRatio = settings.phase1PreferredDistanceRatio;
            phase2PreferredDistance = settings.phase2PreferredDistance;
            roamRadius = settings.roamRadius;
            phase1Color = settings.phase1Color;
            phase2Color = settings.phase2Color;
            healingPoseColor = settings.healingPoseColor;
            bloodBombColor = settings.bloodBombColor;
            explosionColor = settings.explosionColor;
            startInPhase2ForDebug = settings.startInPhase2ForDebug;
            useFastPatternCooldownsForDebug = settings.useFastPatternCooldownsForDebug;
            fastPatternCooldown = settings.fastPatternCooldown;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            healingPoseActive = false;
            encounterActive = false;
            CleanupPatternObjects();

            if (boss != null)
            {
                boss.DamageTaken -= HandleBossDamageTaken;
                boss.Defeated -= HandleBossDefeated;
                boss.SetAiSuppressed(false);
            }
        }

        private void HandleBossDefeated(EnemyController defeatedBoss)
        {
            if (defeatedBoss != boss)
            {
                return;
            }

            StopAllCoroutines();
            actionRunning = false;
            healingPoseActive = false;
            transform.localScale = baseScale;
            CleanupPatternObjects();
        }

        public string CurrentPhaseName => phase.ToString();

        public void SetEncounterActive(bool active)
        {
            if (encounterActive == active)
            {
                return;
            }

            encounterActive = active;
            StopAllCoroutines();
            actionRunning = false;
            healingPoseActive = false;

            if (active)
            {
                nextBloodBombTime = Time.time + 1f;
                nextHealingPoseTime = phase == BossPhase.Phase2
                    ? Time.time + GetHealingPoseCooldown()
                    : float.PositiveInfinity;
            }
        }

        [ContextMenu("Debug/Force Phase 1")]
        public void ForcePhase1ForDebug()
        {
            StopAllCoroutines();
            actionRunning = false;
            healingPoseActive = false;
            phase = BossPhase.Phase1;
            nextBloodBombTime = Time.time;
            nextHealingPoseTime = float.PositiveInfinity;
            transform.localScale = baseScale;
            ApplyPhaseStats();
            ApplyPhaseVisual();
        }

        [ContextMenu("Debug/Force Phase 2")]
        public void ForcePhase2ForDebug()
        {
            StopAllCoroutines();
            actionRunning = false;
            healingPoseActive = false;
            phase = BossPhase.Phase2;
            nextBloodBombTime = float.PositiveInfinity;
            nextHealingPoseTime = Time.time;
            transform.localScale = baseScale;
            ApplyPhaseStats();
            ApplyPhaseVisual();
        }

        [ContextMenu("Debug/Run Blood Bomb")]
        public void RunBloodBombForDebug()
        {
            ForcePhase1ForDebug();
            StartCoroutine(BloodBombRoutine());
        }

        [ContextMenu("Debug/Run Healing Pose")]
        public void RunHealingPoseForDebug()
        {
            ForcePhase2ForDebug();
            StartCoroutine(HealingPoseRoutine());
        }

        private void Update()
        {
            if (!encounterActive)
            {
                return;
            }

            if (boss == null || boss.IsDead)
            {
                enabled = false;
                return;
            }

            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                return;
            }

            if (phase == BossPhase.Phase1 && stats != null && stats.HealthNormalized <= phase2HealthRatio)
            {
                StartCoroutine(EnterPhase2Routine());
                return;
            }

            if (actionRunning)
            {
                return;
            }

            if (phase == BossPhase.Phase1 && Time.time >= nextBloodBombTime && IsPlayerWithinBloodBombRange(player))
            {
                StartCoroutine(BloodBombRoutine());
            }
            else if (phase == BossPhase.Phase2 && Time.time >= nextHealingPoseTime)
            {
                StartCoroutine(HealingPoseRoutine());
            }
        }

        private void LateUpdate()
        {
            if (!encounterActive || boss == null || boss.IsDead || phase == BossPhase.Transition || PlayerController.Instance == null)
            {
                return;
            }

            Vector3 moveDirection = GetMoveDirection();
            if (moveDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float speed = stats != null ? stats.MoveSpeed : (phase == BossPhase.Phase2 ? phase2MoveSpeed : phase1MoveSpeed);
            Vector3 step = moveDirection.normalized * Mathf.Max(0f, speed) * Time.deltaTime;
            boss.MoveByExternalPattern(step);
        }

        private IEnumerator EnterPhase2Routine()
        {
            phase = BossPhase.Transition;
            actionRunning = true;
            healingPoseActive = false;

            float elapsed = 0f;
            const float duration = 1f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float pulse = 1f + Mathf.Sin(t * Mathf.PI * 6f) * 0.1f;
                transform.localScale = baseScale * pulse;

                if (visualRenderer != null)
                {
                    visualRenderer.color = Color.Lerp(phase1Color, phase2Color, t);
                }

                yield return null;
            }

            phase = BossPhase.Phase2;
            transform.localScale = baseScale;
            nextBloodBombTime = float.PositiveInfinity;
            nextHealingPoseTime = Time.time + 1.5f;
            actionRunning = false;
            ApplyPhaseStats();
            ApplyPhaseVisual();
        }

        private IEnumerator BloodBombRoutine()
        {
            actionRunning = true;
            nextBloodBombTime = Time.time + GetBloodBombCooldown();

            Vector3 start = transform.position + Vector3.up * 1.2f;
            Vector3 target = PlayerController.Instance != null
                ? PlayerController.Instance.transform.position
                : transform.position + GetDirectionToPlayer() * bloodBombRange;
            target.y = GetGroundHeight(target) + 0.05f;

            GameObject projectile = CreateTempSpriteObject(
                "LiverBoss_BloodBomb",
                GetBloodBombSprite(),
                bloodBombColor,
                start,
                bloodBombProjectileScale,
                5000);
            float elapsed = 0f;
            float duration = Mathf.Max(0.05f, bloodBombFuseTime);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Vector3 position = Vector3.Lerp(start, target, t);
                position.y += Mathf.Sin(t * Mathf.PI) * bloodBombArcHeight;
                if (projectile != null)
                {
                    projectile.transform.position = position;
                }

                yield return null;
            }

            if (projectile != null)
            {
                ReleaseTempSprite(projectile);
            }

            ExplodeBloodBomb(target);
            actionRunning = false;
        }

        private IEnumerator HealingPoseRoutine()
        {
            actionRunning = true;
            nextHealingPoseTime = Time.time + GetHealingPoseCooldown();

            float preDelay = Mathf.Max(0f, healingPosePreDelay);
            float elapsed = 0f;
            while (elapsed < preDelay)
            {
                elapsed += Time.deltaTime;
                float pulse = 1f + Mathf.Sin(elapsed * 18f) * 0.06f;
                transform.localScale = baseScale * pulse;
                yield return null;
            }

            healingPoseActive = true;
            elapsed = 0f;
            float duration = Mathf.Max(0.05f, healingPoseDuration);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float pulse = 1f + Mathf.Sin(elapsed * 9f) * 0.05f;
                transform.localScale = new Vector3(baseScale.x * (1.12f + pulse * 0.02f), baseScale.y * 0.86f, baseScale.z * (1.12f + pulse * 0.02f));

                if (visualRenderer != null)
                {
                    visualRenderer.color = Color.Lerp(phase2Color, healingPoseColor, 0.82f + Mathf.Sin(elapsed * 8f) * 0.12f);
                }

                yield return null;
            }

            healingPoseActive = false;
            transform.localScale = baseScale;
            ApplyPhaseVisual();
            actionRunning = false;
        }

        private void ExplodeBloodBomb(Vector3 center)
        {
            center.y = GetGroundHeight(center) + 0.08f;
            GameObject explosion = CreateTempSpriteObject(
                "LiverBoss_BloodBombExplosion",
                GetExplosionSprite(),
                explosionColor,
                center,
                bloodBombExplosionRadius * 2f,
                4900);
            StartCoroutine(FadeAndDestroy(explosion, 0.35f, explosionColor));

            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                return;
            }

            Vector3 toPlayer = player.transform.position - center;
            toPlayer.y = 0f;
            if (toPlayer.magnitude > bloodBombExplosionRadius)
            {
                return;
            }

            player.TakeDamage(phase1AttackDamage);
            ApplyPlayerAttackPowerReduction(player, attackPowerDebuffRatio, attackPowerDebuffDuration);
        }

        private IEnumerator FadeAndDestroy(GameObject obj, float duration, Color startColor)
        {
            if (obj == null)
            {
                yield break;
            }

            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            Vector3 startScale = obj.transform.localScale * 0.35f;
            Vector3 endScale = obj.transform.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
                obj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                if (renderer != null)
                {
                    Color color = startColor;
                    color.a = Mathf.Lerp(startColor.a, 0f, t);
                    renderer.color = color;
                }

                yield return null;
            }

            ReleaseTempSprite(obj);
        }

        private void HandleBossDamageTaken(EnemyController damagedBoss, float appliedDamage)
        {
            if (damagedBoss != boss || !healingPoseActive || stats == null || appliedDamage <= 0f)
            {
                return;
            }

            stats.RestoreHealth(appliedDamage * damageHealRatio);
        }

        private void ApplyPhaseStats()
        {
            if (stats == null)
            {
                return;
            }

            if (phase == BossPhase.Phase2)
            {
                stats.SetBaseStat(CharacterStatType.MoveSpeed, phase2MoveSpeed);
                stats.SetBaseStat(CharacterStatType.AttackPower, phase2AttackDamage);
            }
            else
            {
                stats.SetBaseStat(CharacterStatType.MoveSpeed, phase1MoveSpeed);
                stats.SetBaseStat(CharacterStatType.AttackPower, phase1AttackDamage);
            }
        }

        private void ApplyPhaseVisual()
        {
            if (visualRenderer == null)
            {
                visualRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (visualRenderer != null)
            {
                visualRenderer.color = phase == BossPhase.Phase2 ? phase2Color : phase1Color;
            }
        }

        private Vector3 GetMoveDirection()
        {
            Vector3 position = transform.position;
            Vector3 fromAnchor = position - anchorPosition;
            fromAnchor.y = 0f;
            if (fromAnchor.magnitude > roamRadius)
            {
                Vector3 toAnchor = anchorPosition - position;
                toAnchor.y = 0f;
                return toAnchor;
            }

            Vector3 toPlayer = PlayerController.Instance.transform.position - position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;
            if (distance <= 0.0001f)
            {
                return Vector3.zero;
            }

            if (phase == BossPhase.Phase1)
            {
                float preferredDistance = Mathf.Max(0.5f, bloodBombRange * phase1PreferredDistanceRatio);
                if (distance > bloodBombRange * 0.96f)
                {
                    return toPlayer;
                }

                if (distance < preferredDistance * 0.72f)
                {
                    return -toPlayer;
                }

                return Vector3.Cross(Vector3.up, toPlayer);
            }

            if (distance > phase2PreferredDistance)
            {
                return toPlayer;
            }

            if (distance < phase2PreferredDistance * 0.55f)
            {
                return -toPlayer;
            }

            return Vector3.zero;
        }

        private bool IsPlayerWithinBloodBombRange(PlayerController player)
        {
            if (player == null)
            {
                return false;
            }

            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;
            float range = Mathf.Max(0.1f, bloodBombRange);
            return toPlayer.sqrMagnitude <= range * range;
        }

        private float GetBloodBombCooldown()
        {
            return useFastPatternCooldownsForDebug
                ? Mathf.Max(0.05f, fastPatternCooldown)
                : Mathf.Max(0f, bloodBombCooldown);
        }

        private float GetHealingPoseCooldown()
        {
            return useFastPatternCooldownsForDebug
                ? Mathf.Max(0.05f, fastPatternCooldown)
                : Mathf.Max(0f, healingPoseCooldown);
        }

        private Vector3 GetDirectionToPlayer()
        {
            if (PlayerController.Instance == null)
            {
                return Vector3.forward;
            }

            Vector3 direction = PlayerController.Instance.transform.position - transform.position;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        private float GetGroundHeight(Vector3 position)
        {
            BiomeManager biome = BiomeManager.Active;
            return biome != null ? biome.GetGroundHeight(position) : position.y;
        }

        private static void ApplyPlayerAttackPowerReduction(PlayerController player, float reductionRatio, float duration)
        {
            if (player == null)
            {
                return;
            }

            PlayerStatusEffectController status = player.GetComponent<PlayerStatusEffectController>();
            if (status == null)
            {
                status = player.gameObject.AddComponent<PlayerStatusEffectController>();
            }

            status.ApplyAttackPowerReduction(reductionRatio, duration);
        }

        private GameObject CreateTempSpriteObject(string name, Sprite sprite, Color color, Vector3 position, float scale, int sortingOrder)
        {
            return BossPatternVisualPool.Acquire(name, sprite, color, position, scale, sortingOrder, this, activeTempObjects);
        }

        public void ReleaseTempSprite(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            activeTempObjects.Remove(obj);
            BossPatternVisualPool.Release(obj);
        }

        private void CleanupPatternObjects()
        {
            for (int i = 0; i < activeTempObjects.Count; i++)
            {
                if (activeTempObjects[i] != null)
                {
                    BossPatternVisualPool.Release(activeTempObjects[i]);
                }
            }

            activeTempObjects.Clear();
        }

        private Sprite GetBloodBombSprite()
        {
            if (bloodBombSpriteAsset != null)
            {
                return bloodBombSpriteAsset;
            }

            if (bloodBombSprite == null)
            {
                bloodBombSprite = CreateCircleSprite("TempBloodBombSprite", 48, 0.82f, true);
            }

            return bloodBombSprite;
        }

        private static Sprite GetExplosionSprite()
        {
            if (explosionSprite == null)
            {
                explosionSprite = CreateCircleSprite("TempBloodBombExplosionSprite", 64, 0.94f, false);
            }

            return explosionSprite;
        }

        private static Sprite CreateCircleSprite(string name, int size, float radiusRatio, bool filled)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            float center = (size - 1) * 0.5f;
            float radius = center * radiusRatio;
            float innerRadius = radius * 0.72f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = 0f;

                    if (filled && distance <= radius)
                    {
                        alpha = Mathf.Clamp01(1f - distance / radius * 0.35f);
                    }
                    else if (!filled && distance <= radius && distance >= innerRadius)
                    {
                        alpha = Mathf.Clamp01(1f - Mathf.Abs(distance - (innerRadius + radius) * 0.5f) / Mathf.Max(0.01f, radius - innerRadius));
                    }

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = name;
            return sprite;
        }

    }
}
