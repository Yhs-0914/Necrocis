using System.Collections.Generic;
using UnityEngine;
using static Necrocis.ItemCompanionResources;

namespace Necrocis
{
    public sealed class PlayerBioSummon : MonoBehaviour
    {
        public enum SummonKind
        {
            InfectedHost,
            Spore
        }

        private IItemCompanionOwner owner;
        private SpriteRenderer spriteRenderer;
        private EnemyController target;
        private uint targetGeneration;
        private float damage;
        private float expireTime;
        private float searchRadius;
        private float attackRadius;
        private float attackInterval;
        private float nextAttackTime;
        private float moveSpeed;
        private bool destroyOnAttack;
        private Sprite idleSprite;
        private Sprite[] attackSprites;
        private SpriteRenderer infectionMarkerRenderer;
        private float attackVisualEndTime;
        private float attackVisualFrameTime;
        private uint healthGeneration;
        private HealthEffectSource damageSource;
        private readonly HashSet<EnemyLogicalLife> healthRecipients = new HashSet<EnemyLogicalLife>();

        public SummonKind Kind { get; private set; }

        public void Initialize(
            IItemCompanionOwner owner,
            SummonKind kind,
            Sprite sprite,
            Sprite[] attackSprites,
            Vector3 visualScale,
            Color color,
            float damage,
            float lifetime,
            float searchRadius,
            float attackRadius,
            float attackInterval,
            bool destroyOnAttack)
        {
            RetireHealthSource();
            unchecked { healthGeneration++; }
            damageSource = HealthEffectSource.ForOwner(this, "bio-summon/attack", healthGeneration);
            this.owner = owner;
            Kind = kind;
            this.damage = Mathf.Max(0f, damage);
            this.searchRadius = Mathf.Max(0.1f, searchRadius);
            this.attackRadius = Mathf.Max(0.1f, attackRadius);
            this.attackInterval = Mathf.Max(0.05f, attackInterval);
            this.destroyOnAttack = destroyOnAttack;
            this.attackSprites = attackSprites;
            target = null;
            nextAttackTime = 0f;
            attackVisualEndTime = 0f;
            attackVisualFrameTime = 0f;
            moveSpeed = kind == SummonKind.Spore ? 5.2f : 3.2f;
            expireTime = Time.time + Mathf.Max(0.1f, lifetime);

            if (spriteRenderer == null)
            {
                spriteRenderer = ItemEffectVisuals.GetOrAddSpriteRenderer(gameObject);
            }
            idleSprite = sprite != null ? sprite : TextureSpriteCache.GetCircleSprite();
            spriteRenderer.sprite = idleSprite;
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = kind == SummonKind.Spore ? 5100 : 5050;
            spriteRenderer.enabled = true;
            transform.localScale = visualScale;
            SyncBillboard();

            if (kind == SummonKind.InfectedHost)
            {
                EnsureInfectionMarker();
            }
            else if (infectionMarkerRenderer != null)
            {
                infectionMarkerRenderer.enabled = false;
            }
        }

        private void EnsureInfectionMarker()
        {
            Sprite markerSprite = TextureSpriteCache.LoadResourceSprite(InfectedHostEffectPath);
            if (markerSprite == null)
            {
                return;
            }

            GameObject markerObject;
            if (infectionMarkerRenderer == null)
            {
                markerObject = new GameObject("InfectedHostMarker");
                markerObject.transform.SetParent(transform, false);
                infectionMarkerRenderer = markerObject.AddComponent<SpriteRenderer>();
            }
            else
            {
                markerObject = infectionMarkerRenderer.gameObject;
            }

            markerObject.transform.localPosition = Vector3.zero;
            markerObject.transform.localRotation = Quaternion.identity;
            infectionMarkerRenderer.sprite = markerSprite;
            infectionMarkerRenderer.color = Color.white;
            infectionMarkerRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            infectionMarkerRenderer.enabled = true;

            float parentScale = Mathf.Max(
                0.0001f,
                Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y)));
            float markerScale = TextureSpriteCache.GetUniformScaleForWorldSize(markerSprite, 0.48f);
            markerObject.transform.localScale = Vector3.one * (markerScale / parentScale);
        }

        private void Update()
        {
            if ((owner == null || !owner.IsAvailable) || Time.time >= expireTime)
            {
                ReleaseToPool();
                return;
            }

            UpdateAttackVisual();
            if (!ItemEnemyQueries.IsTargetable(target)||target.SpawnGeneration!=targetGeneration)
            {
                target = owner.FindClosestEnemy(transform.position, searchRadius);
                targetGeneration=target!=null?target.SpawnGeneration:0;
            }

            if (!ItemEnemyQueries.IsTargetable(target))
            {
                DriftAroundOwner();
                return;
            }

            Vector3 toTarget = target.transform.position - transform.position;
            toTarget.y = 0f;
            float attackDistance = attackRadius;
            if (toTarget.sqrMagnitude > attackDistance * attackDistance)
            {
                Vector3 step = toTarget.normalized * moveSpeed * Time.deltaTime;
                transform.position += step;
                return;
            }

            if (Time.time < nextAttackTime)
            {
                return;
            }

            nextAttackTime = Time.time + attackInterval;
            StartAttackVisual();
            uint generation = healthGeneration;
            if(target.LogicalLife!=null)healthRecipients.Add(target.LogicalLife);
            target.TakeDamage(damage, damageSource);
            if (generation != healthGeneration || !isActiveAndEnabled) return;
            if (destroyOnAttack)
            {
                SpawnBurstVisual();
                ReleaseToPool();
            }
        }

        public void ReleaseToPool()
        {
            IItemCompanionOwner currentOwner = owner;
            owner = null;
            target = null;
            if (currentOwner != null && currentOwner.IsAvailable)
            {
                currentOwner.ReleaseBioSummon(this);
                return;
            }

            RuntimePool.Release(gameObject);
        }

        private void StartAttackVisual()
        {
            if (attackSprites == null || attackSprites.Length == 0)
            {
                return;
            }

            attackVisualEndTime = Time.time + 0.35f;
            attackVisualFrameTime = Time.time;
            spriteRenderer.sprite = attackSprites[0] != null ? attackSprites[0] : idleSprite;
        }

        private void UpdateAttackVisual()
        {
            if (attackSprites == null || attackSprites.Length == 0 || Time.time >= attackVisualEndTime)
            {
                if (spriteRenderer != null && spriteRenderer.sprite != idleSprite)
                {
                    spriteRenderer.sprite = idleSprite;
                }

                return;
            }

            float frameDuration = 0.35f / Mathf.Max(1, attackSprites.Length);
            if (Time.time < attackVisualFrameTime + frameDuration)
            {
                return;
            }

            int frameIndex = Mathf.Clamp(
                Mathf.FloorToInt((0.35f - Mathf.Max(0f, attackVisualEndTime - Time.time)) / frameDuration),
                0,
                attackSprites.Length - 1);
            attackVisualFrameTime = Time.time;
            if (attackSprites[frameIndex] != null)
            {
                spriteRenderer.sprite = attackSprites[frameIndex];
            }
        }

        private void DriftAroundOwner()
        {
            if ((owner == null || !owner.IsAvailable))
            {
                return;
            }

            Vector3 home = owner.OwnerTransform.position;
            Vector3 toHome = home - transform.position;
            toHome.y = 0f;
            if (toHome.sqrMagnitude <= 1.8f * 1.8f)
            {
                return;
            }

            transform.position += toHome.normalized * moveSpeed * 0.5f * Time.deltaTime;
        }

        private void SpawnBurstVisual()
        {
            ItemEffectVisuals.SpawnPooledCircleVisual(
                SporeBurstVisualPoolName,
                "SporeBurstVisual",
                transform.position,
                0.9f,
                new Color(0.52f, 1f, 0.45f, 0.45f),
                5150,
                0.18f,
                TextureSpriteCache.LoadResourceSprite(SporeColonyEffectPath));
        }

        private void SyncBillboard()
        {
            Camera activeCamera = DontStarveCamera.GetActiveCamera();
            if (activeCamera != null)
            {
                transform.rotation = activeCamera.transform.rotation;
            }
        }

        private void RetireHealthSource()
        {
            foreach (var recipient in healthRecipients)recipient.RemoveHealthSource(damageSource);
            healthRecipients.Clear();
        }

        private void OnDisable()
        {
            RetireHealthSource();
            unchecked { healthGeneration++; }
            owner = null;
            target = null;
            attackSprites = null;
        }
    }
}
