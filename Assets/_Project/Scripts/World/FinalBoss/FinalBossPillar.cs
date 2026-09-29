using System.Collections;
using ProceduralMap;
using UnityEngine;

namespace Necrocis
{
    /// <summary>Damageable phase-one seal backed by the normal enemy combat pipeline.</summary>
    [DisallowMultipleComponent]
    public sealed class FinalBossPillar : MonoBehaviour
    {
        [SerializeField] private BiomeType biome;
        [SerializeField] private RectInt blockedArea;
        [SerializeField, Min(1f)] private float maxHealth = 65f;
        [SerializeField] private SpriteRenderer pillarRenderer;
        [SerializeField] private Sprite intactSprite;
        [SerializeField] private Sprite brokenSprite;

        private FinalBossPhaseOneController owner;
        private MapGenerator map;
        private EnemyController damageTarget;
        private FinalBossWorldHealthBar healthBar;
        private CombatHitFlash hitFlash;
        private Vector3 intactScale;
        private Vector3 intactPosition;
        private Coroutine hitReaction;
        private bool destroyed;

        public BiomeType Biome => biome;
        public bool IsDestroyed => destroyed;
        public float Health => damageTarget != null && damageTarget.Stats != null ? damageTarget.Stats.CurrentHealth : 0f;
        public float MaxHealth => damageTarget != null && damageTarget.Stats != null ? damageTarget.Stats.MaxHealth : maxHealth;
        public EnemyController DamageTarget => damageTarget;
        public Sprite IntactSprite => intactSprite;
        public Sprite BrokenSprite => brokenSprite;
        public Sprite CurrentSprite => pillarRenderer != null ? pillarRenderer.sprite : null;

        public void Configure(BiomeType value, RectInt area, SpriteRenderer renderer)
        {
            biome = value;
            blockedArea = area;
            pillarRenderer = renderer;
        }

        public void Initialize(FinalBossPhaseOneController phaseOwner, MapGenerator arenaMap)
        {
            if (damageTarget != null) return;
            owner = phaseOwner;
            map = arenaMap;
            if (pillarRenderer == null) pillarRenderer = GetComponent<SpriteRenderer>();
            LoadIsolatedSprites();
            intactScale = transform.localScale;
            intactPosition = transform.localPosition;

            var rule = new EnemySpawnRuleConfig
            {
                name = $"FinalBoss_{biome}_Pillar",
                poissonSalt = 9100 + (int)biome,
                maxHealth = maxHealth,
                attackDamage = 0f,
                expReward = 0,
                moveSpeed = 0f,
                chaseRadius = 0f,
                leashRadius = 0f,
                enableContactDamage = false,
                addCollider = true,
                isTrigger = true,
                colliderSize = new Vector3(Mathf.Max(2.5f, blockedArea.width - .7f), 6f, Mathf.Max(1.8f, blockedArea.height - .5f)),
                colliderCenter = new Vector3(0f, 2.5f, 0f),
                useBillboard = false,
                useYSort = false,
                idleSprites = System.Array.Empty<Sprite>(),
                moveSprites = System.Array.Empty<Sprite>(),
                attackSprites = System.Array.Empty<Sprite>(),
                deathSprites = System.Array.Empty<Sprite>()
            };

            damageTarget = EnemyController.Acquire(owner.transform, rule.name, EnemyController.GetPoolArchetypeId(rule));
            damageTarget.Configure(null, rule, transform.position, transform.position);
            damageTarget.SetAiSuppressed(true);
            damageTarget.DamageTaken += HandleDamageTaken;
            damageTarget.Defeated += HandleDefeated;
            if (pillarRenderer != null)
                hitFlash = pillarRenderer.gameObject.GetComponent<CombatHitFlash>()
                    ?? pillarRenderer.gameObject.AddComponent<CombatHitFlash>();

            healthBar = FinalBossWorldHealthBar.Create(damageTarget.transform, GetHealthBarHeight(), GetBiomeColor());
            healthBar.SetValue(1f);
        }

        private void LoadIsolatedSprites()
        {
            string resourceBase = $"FinalBoss/Pillars/Pillar_{biome}_";
            if (intactSprite == null) intactSprite = Resources.Load<Sprite>(resourceBase + "Intact");
            if (brokenSprite == null) brokenSprite = Resources.Load<Sprite>(resourceBase + "Broken");
            if (pillarRenderer == null || intactSprite == null) return;

            // The old sprite was a rectangular crop of the arena artwork. The new sprite owns
            // real transparency, so its flash affects only the organ silhouette.
            SpriteOutline outline = pillarRenderer.GetComponent<SpriteOutline>();
            if (outline != null) outline.enabled = false;
            pillarRenderer.sprite = intactSprite;
            pillarRenderer.color = Color.white;
        }

        private float GetHealthBarHeight()
        {
            if (pillarRenderer == null || pillarRenderer.sprite == null) return 5f;
            return Mathf.Clamp(pillarRenderer.bounds.size.y + .55f, 3.8f, 7f);
        }

        private Color GetBiomeColor()
        {
            return biome switch
            {
                BiomeType.Intestine => new Color(.83f, .46f, .79f),
                BiomeType.Liver => new Color(.72f, .16f, .25f),
                BiomeType.Stomach => new Color(.92f, .55f, .18f),
                BiomeType.Lung => new Color(.35f, .82f, .92f),
                _ => new Color(.9f, .2f, .3f)
            };
        }

        private void HandleDamageTaken(EnemyController _, float damage)
        {
            if (destroyed) return;
            healthBar?.SetValue(damageTarget.Stats.HealthNormalized);
            if (hitFlash == null && pillarRenderer != null)
                hitFlash = pillarRenderer.gameObject.GetComponent<CombatHitFlash>() ?? pillarRenderer.gameObject.AddComponent<CombatHitFlash>();
            hitFlash?.Flash(new Color(1f, .9f, .55f), .11f);
            if (hitReaction != null) StopCoroutine(hitReaction);
            hitReaction = StartCoroutine(PlayHitReaction());
        }

        private IEnumerator PlayHitReaction()
        {
            Vector3 basePosition = intactPosition;
            Vector3 baseScale = intactScale;
            const float duration = .14f;
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float strength = 1f - elapsed / duration;
                transform.localPosition = basePosition + new Vector3(Mathf.Sin(elapsed * 95f) * .11f * strength, 0f, 0f);
                transform.localScale = baseScale * (1f + .045f * strength);
                yield return null;
            }
            transform.localPosition = basePosition;
            transform.localScale = baseScale;
            hitReaction = null;
        }

        private void HandleDefeated(EnemyController defeated)
        {
            if (destroyed) return;
            destroyed = true;
            defeated.DamageTaken -= HandleDamageTaken;
            defeated.Defeated -= HandleDefeated;
            if (hitReaction != null) StopCoroutine(hitReaction);
            healthBar?.Hide();
            map?.SetAuthoredAreaBlocked(blockedArea, false);
            ShowBrokenRemnant();
            owner?.NotifyPillarDestroyed(this);
        }

        private void ShowBrokenRemnant()
        {
            transform.localPosition = intactPosition;
            transform.localScale = intactScale;
            if (pillarRenderer == null) return;
            if (brokenSprite != null) pillarRenderer.sprite = brokenSprite;
            pillarRenderer.color = Color.white;
        }

#if UNITY_EDITOR
        public void DestroyForTest()
        {
            if (!destroyed && damageTarget != null) damageTarget.TakeDamage(MaxHealth * 2f);
        }
#endif
    }
}
