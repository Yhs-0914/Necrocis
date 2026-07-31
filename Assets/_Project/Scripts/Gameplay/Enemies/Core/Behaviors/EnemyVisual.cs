using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public partial class EnemyController
    {

        public void UpdateFacingDirection()
        {
            if (spriteRenderer == null || playerTransform == null) return;
            UpdateFacingFromVector(playerTransform.position - GetCurrentPosition());
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
            currentLoopFrames = null;
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
            ApplyAnimation(GetMoveFrames());
        }


        private void ApplyAnimation(Sprite[] frames)
        {
            if (spriteRenderer == null) return;

            if (frames == null || frames.Length == 0)
            {
                animatedSprite.Stop();
                animatedSprite.enabled = false;
                currentLoopFrames = null;
                return;
            }

            if (currentLoopFrames == frames && animatedSprite.enabled && animatedSprite.IsPlaying)
            {
                return;
            }

            currentLoopFrames = frames;

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
            Sprite[] frames = GetDirectionalFrames(config.idleSprites, config.idleSpritesUp, config.idleSpritesDown);
            if (HasFrames(frames))
            {
                return frames;
            }

            return GetDirectionalFrames(config.moveSprites, config.moveSpritesUp, config.moveSpritesDown);
        }


        private Sprite[] GetMoveFrames()
        {
            Sprite[] frames = GetDirectionalFrames(config.moveSprites, config.moveSpritesUp, config.moveSpritesDown);
            if (HasFrames(frames))
            {
                return frames;
            }

            return GetDirectionalFrames(config.idleSprites, config.idleSpritesUp, config.idleSpritesDown);
        }


        private Sprite[] GetDirectionalFrames(Sprite[] sideFrames, Sprite[] upFrames, Sprite[] downFrames)
        {
            switch (facingDirection)
            {
                case 0:
                    return HasFrames(upFrames) ? upFrames : sideFrames;
                case 3:
                    return HasFrames(downFrames) ? downFrames : sideFrames;
                default:
                    return sideFrames;
            }
        }


        private static bool HasFrames(Sprite[] frames)
        {
            return frames != null && frames.Length > 0;
        }


        private void UpdateFacingFromVector(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            int nextDirection = GetPlanarDirection(direction);
            bool changed = nextDirection != facingDirection;
            facingDirection = nextDirection;
            ApplyFacingFlip();

            if (changed && !attackAnimPlaying && !deathAnimPlaying)
            {
                if (usingMoveAnimation)
                {
                    ApplyAnimation(GetMoveFrames());
                }
                else
                {
                    ApplyAnimation(GetIdleFrames());
                }
            }
        }


        private int GetPlanarDirection(Vector3 direction)
        {
            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.z))
            {
                return direction.x >= 0f ? 1 : 2;
            }

            return direction.z >= 0f ? 0 : 3;
        }


        private void ApplyFacingFlip()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (facingDirection == 1)
            {
                spriteRenderer.flipX = false;
                return;
            }

            if (facingDirection == 2)
            {
                spriteRenderer.flipX = true;
                return;
            }

            if (HasDirectionalFramesForFacing())
            {
                spriteRenderer.flipX = false;
            }
        }


        private bool HasDirectionalFramesForFacing()
        {
            if (config == null)
            {
                return false;
            }

            if (facingDirection == 0)
            {
                return HasFrames(config.idleSpritesUp) || HasFrames(config.moveSpritesUp);
            }

            if (facingDirection == 3)
            {
                return HasFrames(config.idleSpritesDown) || HasFrames(config.moveSpritesDown);
            }

            return false;
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
            contactDamage = GetOrAddComponent<EnemyContactDamage>(gameObject);
        }

        private void ConfigureStats()
        {
            if (stats == null || config == null)
            {
                return;
            }

            EnemyDifficultyBalance balance = DifficultyBalanceService.GetEnemyBalance(IsBossEncounter);
            statConfigurationBuffer.Clear();
            statConfigurationBuffer.Add(new CharacterStatValue(
                CharacterStatType.MoveSpeed,
                config.moveSpeed * Mathf.Max(0.01f, balance.moveSpeed)));
            statConfigurationBuffer.Add(new CharacterStatValue(
                CharacterStatType.MaxHealth,
                config.maxHealth * Mathf.Max(0.01f, balance.maxHealth)));
            statConfigurationBuffer.Add(new CharacterStatValue(CharacterStatType.AttackPower, config.attackDamage));

            List<CharacterStatValue> additionalStats = config.additionalBaseStats;
            if (additionalStats != null)
            {
                for (int i = 0; i < additionalStats.Count; i++)
                {
                    statConfigurationBuffer.Add(additionalStats[i]);
                }
            }

            stats.ClearModifiers();
            stats.ConfigureBaseStats(statConfigurationBuffer, true);
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
                billboard.SetUpdateMode(Billboard.UpdateMode.Once);
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
