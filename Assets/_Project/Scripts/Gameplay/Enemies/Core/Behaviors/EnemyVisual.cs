using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public partial class EnemyController
    {

        public void UpdateFacingDirection()
        {
            if (spriteRenderer == null || playerTransform == null) return;
            float dx = playerTransform.position.x - GetCurrentPosition().x;
            if (Mathf.Abs(dx) > 0.01f)
            {
                spriteRenderer.flipX = dx < 0f;
            }
        }


        public void DisableCollider()
        {
            if (boxCollider != null) boxCollider.enabled = false;
        }

        public void PlayDeathAnimation(System.Action onComplete)
        {
            Sprite[] deathFrames = config != null ? config.deathSprites : null;
            if (deathFrames == null || deathFrames.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            deathAnimPlaying = true;
            usingMoveAnimation = false;
            animatedSprite.enabled = true;
            animatedSprite.PlayOneShot(deathFrames, config.deathAnimationSpeed, () =>
            {
                deathAnimPlaying = false;
                onComplete?.Invoke();
            });
        }


        public void SetIdleAnimation()
        {
            usingMoveAnimation = false;
            ApplyAnimation(GetIdleFrames());
        }


        public void SetMoveAnimation()
        {
            usingMoveAnimation = true;
            ApplyAnimation(config.moveSprites);
        }


        private void ApplyAnimation(Sprite[] frames)
        {
            if (spriteRenderer == null) return;

            if (frames == null || frames.Length == 0)
            {
                animatedSprite.Stop();
                animatedSprite.enabled = false;
                return;
            }

            if (frames.Length == 1)
            {
                animatedSprite.Stop();
                animatedSprite.enabled = false;
                spriteRenderer.sprite = frames[0];
                return;
            }

            animatedSprite.enabled = true;
            animatedSprite.SetFrames(frames, config.animationSpeed);
            animatedSprite.Play();
        }


        private Sprite[] GetIdleFrames()
        {
            if (config.idleSprites != null && config.idleSprites.Length > 0)
                return config.idleSprites;
            return config.moveSprites;
        }


        private void SyncHeight()
        {
            if (config == null) return;
            BiomeManager biome = BiomeManager.Active;
            if (biome == null) return;

            Vector3 position = GetCurrentPosition();
            Vector2Int grid = biome.WorldToGrid(position);
            if (!hasCachedGroundHeight || grid != cachedGroundGrid)
            {
                cachedGroundGrid = grid;
                cachedGroundHeight = biome.GetGroundHeight(grid.x, grid.y) + config.heightOffset;
                hasCachedGroundHeight = true;
            }

            if (Mathf.Abs(position.y - cachedGroundHeight) > 0.0001f)
            {
                position.y = cachedGroundHeight;
                SetPosition(position);
            }
        }


        private void EnsureComponents()
        {
            if (visualRoot == null)
            {
                Transform child = transform.Find("Visual");
                if (child == null)
                {
                    GameObject visualObject = new GameObject("Visual");
                    child = visualObject.transform;
                    child.SetParent(transform, false);
                }
                visualRoot = child;
            }

            spriteRenderer = GetOrAddComponent<SpriteRenderer>(visualRoot.gameObject);
            animatedSprite = GetOrAddComponent<SpriteFrameAnimator>(visualRoot.gameObject);
            billboard = GetOrAddComponent<Billboard>(visualRoot.gameObject);
            ySort = GetOrAddComponent<SpriteYSort>(visualRoot.gameObject);
            body = GetOrAddComponent<Rigidbody>(gameObject);
            boxCollider = GetOrAddComponent<BoxCollider>(gameObject);
            stats = GetOrAddComponent<CharacterStats>(gameObject);
            statusEffectController = GetOrAddComponent<EnemyStatusEffectController>(gameObject);
            enemySkillBridge = GetOrAddComponent<EnemySkillBridge>(gameObject);
        }

        private void ConfigureStats()
        {
            if (stats == null || config == null)
            {
                return;
            }

            List<CharacterStatValue> additionalStats = config.additionalBaseStats ?? new List<CharacterStatValue>();
            List<CharacterStatValue> baseStats = new List<CharacterStatValue>(3 + additionalStats.Count)
            {
                new CharacterStatValue(CharacterStatType.MoveSpeed, config.moveSpeed),
                new CharacterStatValue(CharacterStatType.MaxHealth, config.maxHealth),
                new CharacterStatValue(CharacterStatType.AttackPower, config.attackDamage)
            };

            for (int i = 0; i < additionalStats.Count; i++)
            {
                baseStats.Add(additionalStats[i]);
            }

            stats.ClearModifiers();
            stats.ConfigureBaseStats(baseStats, true);
        }


        private void ApplyVisualSetup()
        {
            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = config.scale;
            spriteRenderer.sortingOrder = config.sortingOrder;
            spriteRenderer.flipX = false;

            // 엘리트 틴트 색상 적용
            spriteRenderer.color = (config.isElite && config.tintColor != Color.white)
                ? config.tintColor
                : Color.white;

            billboard.enabled = config.useBillboard;
            if (config.useBillboard)
            {
                billboard.ResetBaseLocalPosition(Vector3.zero);
                billboard.SetUpdateMode(Billboard.UpdateMode.Continuous);
            }

            ySort.enabled = config.useYSort;
            if (config.useYSort)
            {
                ySort.Configure(SpriteYSort.WorldDynamicBaseSortingOrder, true, SpriteYSort.WorldDynamicMinSortingOrder);
                ySort.SetUpdateMode(SpriteYSort.UpdateMode.Continuous);
            }
        }


        private void ApplyPhysicsSetup()
        {
            body.useGravity = false;
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.constraints = RigidbodyConstraints.FreezeRotation;

            boxCollider.enabled = config.addCollider;
            boxCollider.isTrigger = config.isTrigger;
            boxCollider.size = config.colliderSize;
            boxCollider.center = config.colliderCenter;
        }

    }
}
