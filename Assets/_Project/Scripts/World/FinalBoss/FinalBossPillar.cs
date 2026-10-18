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
        private Sprite[] destructionFrames;
        private bool destroyed;
        private static Material linkMaterial;
        private LineRenderer neuralLink;
        private float linkVisibility = 1f;

        public BiomeType Biome => biome;
        public Color AccentColor => GetBiomeColor();
        public bool IsDestroyed => destroyed;
        public bool IsBreaking { get; private set; }
        public int DestructionFrameIndex { get; private set; } = -1;
        public int DestructionFrameCount => destructionFrames != null ? destructionFrames.Length : 0;
        public float Health => damageTarget != null && damageTarget.Stats != null ? damageTarget.Stats.CurrentHealth : 0f;
        public float MaxHealth => damageTarget != null && damageTarget.Stats != null ? damageTarget.Stats.MaxHealth : maxHealth;
        public EnemyController DamageTarget => damageTarget;
        public Sprite IntactSprite => intactSprite;
        public Sprite BrokenSprite => brokenSprite;
        public Sprite CurrentSprite => pillarRenderer != null ? pillarRenderer.sprite : null;
        public bool RemnantVisible => destroyed && pillarRenderer != null && pillarRenderer.enabled
            && pillarRenderer.gameObject.activeInHierarchy;

        public void HideBrokenRemnant()
        {
            if (!destroyed) return;
            if (pillarRenderer != null) pillarRenderer.enabled = false;
            linkVisibility = 0f;
            if (neuralLink != null) neuralLink.enabled = false;
            healthBar?.Hide();
        }

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
            damageTarget.ConfigureWithContext(null, rule, transform.position, transform.position,
                EnemySpawnContextFactory.CaptureForBiome(BiomeManager.Active, rule, transform.position));
            maxHealth = owner.Balance.phaseOne.pillarMaxHealth * owner.Balance.GetDifficulty(WorldProgressBindings.Session.ActiveDifficulty).health;
            damageTarget.Stats.SetBaseStat(CharacterStatType.MaxHealth, maxHealth, true);
            damageTarget.SetAiSuppressed(true);
            damageTarget.DamageTaken += HandleDamageTaken;
            damageTarget.Defeated += HandleDefeated;
            if (pillarRenderer != null)
                hitFlash = pillarRenderer.gameObject.GetComponent<CombatHitFlash>()
                    ?? pillarRenderer.gameObject.AddComponent<CombatHitFlash>();

            healthBar = FinalBossWorldHealthBar.Create(damageTarget.transform, GetHealthBarHeight(), GetBiomeColor());
            healthBar.FollowArtwork(pillarRenderer);
            healthBar.SetValue(1f);
            CreateNeuralLink();
        }

        private void CreateNeuralLink()
        {
            if (linkMaterial == null)
                linkMaterial = new Material(Shader.Find("Sprites/Default"))
                {
                    name = "FinalBossSealConnection",
                    hideFlags = HideFlags.HideAndDontSave
                };
            GameObject link = new GameObject("SealNeuralConnection");
            link.transform.SetParent(transform, false);
            neuralLink = link.AddComponent<LineRenderer>();
            neuralLink.sharedMaterial = linkMaterial;
            neuralLink.positionCount = 20;
            neuralLink.useWorldSpace = true;
            neuralLink.widthMultiplier = .065f;
            neuralLink.sortingOrder = 4050;
            neuralLink.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            neuralLink.receiveShadows = false;
        }

        private void LateUpdate()
        {
            if (neuralLink == null || owner == null) return;
            linkVisibility = Mathf.MoveTowards(linkVisibility,
                destroyed || owner.CurrentPhase != 1 ? 0f : 1f, Time.deltaTime * 2.5f);
            neuralLink.enabled = linkVisibility > .001f;
            if (!neuralLink.enabled) return;
            Vector3 from = transform.position;
            Vector3 to = owner.BossGroundPosition;
            from.y = to.y;
            Vector3 bend = Vector3.Cross((to - from).normalized, Vector3.up) * 1.2f;
            for (int i = 0; i < neuralLink.positionCount; i++)
            {
                float t = i / (float)(neuralLink.positionCount - 1);
                neuralLink.SetPosition(i, Vector3.Lerp(from, to, t) + bend * Mathf.Sin(t * Mathf.PI));
            }
            Color color = AccentColor;
            color.a = (.19f + .09f * Mathf.Sin(Time.time * 2.4f + (int)biome)) * linkVisibility;
            neuralLink.startColor = color;
            color.a *= .4f;
            neuralLink.endColor = color;
        }

        private void LoadIsolatedSprites()
        {
            string resourceBase = $"FinalBoss/Pillars/Pillar_{biome}_";
            if (intactSprite == null) intactSprite = Resources.Load<Sprite>(resourceBase + "Intact");
            if (brokenSprite == null) brokenSprite = Resources.Load<Sprite>(resourceBase + "Broken");
            destructionFrames = Resources.LoadAll<Sprite>("FinalBoss/Pillars/Animations/" + biome);
            System.Array.Sort(destructionFrames, (a, b) => string.CompareOrdinal(a.name, b.name));
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
                BiomeType.Intestine => new Color(.35f, .82f, .58f),
                BiomeType.Liver => new Color(.76f, .37f, .75f),
                BiomeType.Stomach => new Color(.92f, .55f, .18f),
                BiomeType.Lung => new Color(.94f, .78f, .51f),
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
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
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
            if (hitFlash != null) hitFlash.enabled = false;
            healthBar?.Hide();
            map?.SetAuthoredAreaBlocked(blockedArea, false);
            if (destructionFrames != null && destructionFrames.Length == 5)
                StartCoroutine(PlayDestruction());
            else ShowBrokenRemnant();
            owner?.NotifyPillarDestroyed(this);
        }

        private IEnumerator PlayDestruction()
        {
            IsBreaking = true;
            transform.localPosition = intactPosition;
            transform.localScale = intactScale;
            pillarRenderer.color = Color.white;
            for (int i = 0; i < destructionFrames.Length; i++)
            {
                DestructionFrameIndex = i;
                pillarRenderer.sprite = destructionFrames[i];
                if (i < destructionFrames.Length - 1)
                    yield return new WaitForSeconds(GetDestructionFrameDuration(i));
            }
            ShowBrokenRemnant();
            IsBreaking = false;
        }

        private float GetDestructionFrameDuration(int index)
        {
            // The approved preview's timing: wet slump, canopy topple, timber split, deflation.
            return biome switch
            {
                BiomeType.Stomach => index switch { 0 => .12f, 1 => .14f, 2 => .20f, _ => .26f },
                BiomeType.Intestine => index switch { 0 => .16f, 1 => .22f, 2 => .30f, _ => .36f },
                BiomeType.Liver => index switch { 0 => .14f, 1 => .22f, 2 => .24f, _ => .32f },
                _ => index switch { 0 => .24f, 1 => .30f, 2 => .24f, _ => .32f }
            };
        }

        private void ShowBrokenRemnant()
        {
            transform.localPosition = intactPosition;
            transform.localScale = intactScale;
            if (pillarRenderer == null) return;
            if (brokenSprite != null) pillarRenderer.sprite = brokenSprite;
            pillarRenderer.color = Color.white;
        }

        private void OnDestroy()
        {
            if (damageTarget == null) return;
            damageTarget.DamageTaken -= HandleDamageTaken;
            damageTarget.Defeated -= HandleDefeated;
        }

#if UNITY_EDITOR
        public void DestroyForTest()
        {
            if (!destroyed && damageTarget != null) damageTarget.TakeDamage(MaxHealth * 2f);
        }
#endif
    }
}
