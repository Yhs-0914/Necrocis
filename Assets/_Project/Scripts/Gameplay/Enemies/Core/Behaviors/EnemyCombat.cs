using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public partial class EnemyController
    {
        [SerializeField] private bool logDamageToConsole;

        private float DifficultyAttackCooldown =>
            config != null
                ? config.attackCooldown
                  * Mathf.Max(
                      0.01f,
                      DifficultyBalanceService.GetEnemyBalance(IsBossEncounter).attackCooldown)
                : 0f;

        public bool TryPerformAttack(float deltaTime)
        {
            if (statusEffectController != null && statusEffectController.IsStunned)
            {
                return false;
            }

            // 공격 애니메이션 재생 중이면 대기
            if (attackAnimPlaying) return false;

            attackTimer -= deltaTime;
            if (attackTimer > 0f) return false;

            // 공격 애니메이션이 있으면 재생 → 완료 시 데미지
            Sprite[] attackFrames = GetAttackFrames();
            if (attackFrames != null && attackFrames.Length > 0)
            {
                attackAnimPlaying = true;
                usingMoveAnimation = false;

                // 대식세포 등: 공격 시 콜라이더 확장
                if (config.expandColliderOnAttack)
                {
                    ExpandAttackCollider();
                }

                UpdateAttackFacing();

                animatedSprite.enabled = true;
                currentLoopFrames = null;
                animatedSprite.PlayOneShot(attackFrames, config.attackAnimationSpeed, OnAttackAnimationComplete);
                return true;
            }

            // 공격 애니메이션이 없으면 즉시 데미지 (기존 동작)
            ApplyDamageToPlayer();
            attackTimer = DifficultyAttackCooldown;
            return false;
        }

        public bool PlayAttackAnimationOnly()
        {
            if (IsDead || config == null || animatedSprite == null || attackAnimPlaying)
            {
                return false;
            }

            Sprite[] attackFrames = GetAttackFrames();
            if (attackFrames == null || attackFrames.Length == 0)
            {
                return false;
            }

            attackAnimPlaying = true;
            usingMoveAnimation = false;
            UpdateAttackFacing();
            animatedSprite.enabled = true;
            currentLoopFrames = null;
            animatedSprite.PlayOneShot(attackFrames, config.attackAnimationSpeed, OnPatternAttackAnimationComplete);
            return true;
        }

        public bool IsPatternAnimationPlaying => attackAnimPlaying;

        public bool PlayPatternAnimation(Sprite[] frames, float frameRate, System.Action onComplete = null, bool returnToIdleOnComplete = true)
        {
            if (IsDead || animatedSprite == null || frames == null || frames.Length == 0 || attackAnimPlaying)
            {
                return false;
            }

            attackAnimPlaying = true;
            usingMoveAnimation = false;
            currentLoopFrames = null;
            animatedSprite.enabled = true;
            animatedSprite.PlayOneShot(frames, Mathf.Max(0.01f, frameRate), () =>
            {
                attackAnimPlaying = false;
                if (colliderExpanded)
                {
                    RestoreCollider();
                }

                if (returnToIdleOnComplete)
                {
                    SetIdleAnimation();
                }

                onComplete?.Invoke();
            });
            return true;
        }


        private void OnAttackAnimationComplete()
        {
            attackAnimPlaying = false;
            attackTimer = DifficultyAttackCooldown;

            // 공격 범위 내 플레이어에게 데미지
            ApplyDamageToPlayer();

            // 콜라이더 복원
            if (colliderExpanded)
            {
                RestoreCollider();
            }

            // 대기 애니메이션으로 복귀
            SetIdleAnimation();
        }

        private void OnPatternAttackAnimationComplete()
        {
            attackAnimPlaying = false;
            if (colliderExpanded)
            {
                RestoreCollider();
            }

            SetIdleAnimation();
        }


        private void ApplyDamageToPlayer()
        {
            PlayerController player = PlayerController.Instance;
            if (player == null) return;

            float damage = EnemyCombatCalculator.GetAttackDamage(stats, config);

            if (config.isRanged)
            {
                LaunchProjectile(damage);
                return;
            }

            if (!CanMeleeDamagePlayer(player))
            {
                return;
            }

            Health playerHealth = player.HealthComponent;
            if (playerHealth == null) return;

            if (!ignoreMidBossArenaRestriction)
            {
                AudioManager.Instance?.PlaySFX("EnemyAttack");
            }
            playerHealth.TakeDamage(damage, this);
        }


        private bool CanMeleeDamagePlayer(PlayerController player)
        {
            if (player == null || config == null)
            {
                return false;
            }

            if (!IsPlayerInAttackRange())
            {
                return false;
            }

            if (TryGetPlayerDamageBounds(player, out Bounds playerBounds)
                && TryGetMeleeAttackBounds(out Bounds attackBounds))
            {
                if (BoundsOverlapPlanar(attackBounds, playerBounds))
                {
                    return true;
                }
            }

            float fallbackRange = GetEffectiveMeleeAttackRange(player);
            Vector3 toPlayer = player.transform.position - GetCurrentPosition();
            toPlayer.y = 0f;
            return toPlayer.sqrMagnitude <= fallbackRange * fallbackRange;
        }


        private bool TryGetPlayerDamageBounds(PlayerController player, out Bounds bounds)
        {
            bounds = default;
            if (player == null)
            {
                return false;
            }

            Collider playerCollider = player.HitCollider;

            if (playerCollider == null || !playerCollider.enabled)
            {
                return false;
            }

            bounds = playerCollider.bounds;
            return true;
        }


        private bool TryGetMeleeAttackBounds(out Bounds bounds)
        {
            bounds = default;
            if (config == null)
            {
                return false;
            }

            Vector3 localCenter = config.expandColliderOnAttack ? config.attackColliderCenter : config.colliderCenter;
            Vector3 localSize = config.expandColliderOnAttack ? config.attackColliderSize : config.colliderSize;
            if (localSize.x <= 0f || localSize.y <= 0f || localSize.z <= 0f)
            {
                return false;
            }

            Vector3 scaledSize = Vector3.Scale(localSize, Abs(transform.lossyScale));
            bounds = new Bounds(transform.TransformPoint(localCenter), scaledSize);
            bounds.Expand(new Vector3(0.2f, 0f, 0.2f));
            return true;
        }


        private static bool BoundsOverlapPlanar(Bounds a, Bounds b)
        {
            return a.min.x <= b.max.x
                && a.max.x >= b.min.x
                && a.min.z <= b.max.z
                && a.max.z >= b.min.z;
        }


        private static Vector3 Abs(Vector3 value)
        {
            return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }


        private void LaunchProjectile(float damage)
        {
            Sprite projSprite = config.projectileSprite; // null이면 EnemyProjectile에서 기본 구체 사용

            Vector3 spawnPos = GetCurrentPosition();
            Vector3 toPlayer = PlayerController.Instance.transform.position - spawnPos;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.0001f) return;

            Vector3 dir = toPlayer.normalized;
            spawnPos += dir * config.projectileSpawnOffset;
            spawnPos.y += 2f;

            // 스프라이트 방향 업데이트
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = dir.x < 0f;
            }

            EnemyProjectile proj = EnemyProjectile.Acquire(spawnPos, projSprite, config.projectileScale);
            if (!ignoreMidBossArenaRestriction)
            {
                AudioManager.Instance?.PlaySFX("EnemyAttack");
            }
            proj.Launch(dir, damage, config.projectileSpeed, config.projectileLifeTime, this);
        }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;
            if (stats == null)
            {
                return;
            }

            float finalDamage = EnemyCombatCalculator.GetIncomingDamage(damage, statusEffectController);
            if (finalDamage <= 0f)
            {
                return;
            }

            float appliedDamage = stats.ApplyDamage(finalDamage);
            if (appliedDamage > 0f)
            {
                DamageTaken?.Invoke(this, appliedDamage);
                CombatVfx.PlayEnemyHit(this, appliedDamage, stats.IsDead);
            }

            if (logDamageToConsole && config != null && !config.isElite)
            {
                string enemyName = string.IsNullOrWhiteSpace(config.name) ? gameObject.name : config.name;
                Debug.Log($"[DamageLog] Player -> {enemyName} : {appliedDamage:0.##} (HP {stats.CurrentHealth:0.##}/{stats.MaxHealth:0.##})");
            }

            if (stats.IsDead)
            {
                if (!ignoreMidBossArenaRestriction)
                {
                    AudioManager.Instance?.PlaySFX("EnemyDeath");
                }

                if (PlayerController.Instance != null)
                {
                    PlayerItemCombatEffects itemEffects = PlayerController.Instance.GetComponent<PlayerItemCombatEffects>();
                    itemEffects?.NotifyEnemyDefeatedByPlayer(this);
                }

                RaiseDefeated();
                ChangeState(EnemyDeadState.Instance);
            }
            else if (appliedDamage > 0f && !ignoreMidBossArenaRestriction)
            {
                AudioManager.Instance?.PlaySFX("EnemyHit");
            }
        }

        public void GrantExp()
        {
            if (config == null) return;
            float multiplier = DifficultyBalanceService
                .GetEnemyBalance(IsBossEncounter)
                .experienceReward;
            LevelUpManager.AddEnemyKillExp(config.expReward, multiplier);

            // 엘리트 스포너에 킬 알림
            if (EliteSpawner.Instance != null && !config.isElite)
            {
                EliteSpawner.Instance.NotifyEnemyKilled(config.name);
            }
        }

        public void ExpandAttackCollider()
        {
            if (boxCollider == null || config == null || colliderExpanded) return;
            boxCollider.size = config.attackColliderSize;
            boxCollider.center = config.attackColliderCenter;
            colliderExpanded = true;
        }

        public void RestoreCollider()
        {
            if (boxCollider == null || config == null || !colliderExpanded) return;
            boxCollider.size = config.colliderSize;
            boxCollider.center = config.colliderCenter;
            colliderExpanded = false;
        }

        private Sprite[] GetAttackFrames()
        {
            if (config == null) return null;

            // 방향별 공격 스프라이트가 없으면 기본 attackSprites 사용
            bool hasDirectional = config.attackSpritesUp != null && config.attackSpritesUp.Length > 0;
            if (!hasDirectional)
            {
                return config.attackSprites;
            }

            // 방향별 공격 (NK세포)
            int dir = GetAttackDirection();
            switch (dir)
            {
                case 0: return config.attackSpritesUp;                        // 상
                case 1: return config.attackSprites;                          // 우
                case 2: return config.attackSprites;                          // 좌 (flipX로 처리)
                case 3: return config.attackSpritesDown;                      // 하
                default: return config.attackSprites;
            }
        }

        private int GetAttackDirection()
        {
            if (playerTransform == null) return facingDirection;

            Vector3 toPlayer = playerTransform.position - GetCurrentPosition();
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude <= 0.000001f)
            {
                return facingDirection;
            }

            return GetPlanarDirection(toPlayer);
        }

        private void UpdateAttackFacing()
        {
            if (spriteRenderer == null || config == null)
            {
                return;
            }

            facingDirection = GetAttackDirection();
            bool hasDirectional = config.attackSpritesUp != null && config.attackSpritesUp.Length > 0;
            if (hasDirectional)
            {
                ApplyFacingFlip();
                return;
            }

            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                return;
            }

            ApplyFacingFlip();
            Vector3 toPlayer = player.transform.position - GetCurrentPosition();
            if (Mathf.Abs(toPlayer.x) > 0.01f)
            {
                spriteRenderer.flipX = toPlayer.x < 0f;
            }
        }

        public void CancelAttackAnimation()
        {
            if (!attackAnimPlaying) return;
            attackAnimPlaying = false;
            if (colliderExpanded) RestoreCollider();
            SetIdleAnimation();
        }

    }
}
