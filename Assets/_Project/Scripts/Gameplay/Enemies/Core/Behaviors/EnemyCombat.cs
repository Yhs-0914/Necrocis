using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public partial class EnemyController
    {
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

                // NK세포: 방향별 공격 스프라이트가 있으면 flipX 설정
                bool hasDirectional = config.attackSpritesUp != null && config.attackSpritesUp.Length > 0;
                if (hasDirectional && spriteRenderer != null)
                {
                    int dir = GetAttackDirection();
                    spriteRenderer.flipX = (dir == 2); // 2 = left (우 스프라이트를 좌우 반전)
                }

                animatedSprite.enabled = true;
                animatedSprite.PlayOneShot(attackFrames, config.attackAnimationSpeed, OnAttackAnimationComplete);
                return true;
            }

            // 공격 애니메이션이 없으면 즉시 데미지 (기존 동작)
            ApplyDamageToPlayer();
            attackTimer = config.attackCooldown;
            return false;
        }


        private void OnAttackAnimationComplete()
        {
            attackAnimPlaying = false;
            attackTimer = config.attackCooldown;

            // 콜라이더 복원
            if (colliderExpanded)
            {
                RestoreCollider();
            }

            // 공격 범위 내 플레이어에게 데미지
            ApplyDamageToPlayer();

            // 대기 애니메이션으로 복귀
            SetIdleAnimation();
        }


        private void ApplyDamageToPlayer()
        {
            if (PlayerController.Instance == null) return;

            float damage = EnemyCombatCalculator.GetAttackDamage(stats, config);

            if (config.isRanged)
            {
                LaunchProjectile(damage);
                return;
            }

            Health playerHealth = PlayerController.Instance.GetComponent<Health>();
            if (playerHealth == null) return;

            if (!ignoreMidBossArenaRestriction)
            {
                AudioManager.Instance?.PlaySFX("EnemyAttack");
            }
            playerHealth.TakeDamage(damage);
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
            proj.Launch(dir, damage, config.projectileSpeed, config.projectileLifeTime);
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

            stats.ApplyDamage(finalDamage);
            if (stats.IsDead)
            {
                if (!ignoreMidBossArenaRestriction)
                {
                    AudioManager.Instance?.PlaySFX("EnemyDeath");
                }
                RaiseDefeated();
                ChangeState(EnemyDeadState.Instance);
            }
            else
            {
                if (!ignoreMidBossArenaRestriction)
                {
                    AudioManager.Instance?.PlaySFX("EnemyHit");
                }
            }
        }

        public void GrantExp()
        {
            if (config == null) return;
            LevelUpManager.AddEnemyKillExp();

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
            if (playerTransform == null) return 1;

            Vector3 toPlayer = playerTransform.position - GetCurrentPosition();
            toPlayer.y = 0f;

            if (Mathf.Abs(toPlayer.x) >= Mathf.Abs(toPlayer.z))
            {
                return toPlayer.x >= 0f ? 1 : 2; // 우 / 좌
            }
            return toPlayer.z >= 0f ? 0 : 3; // 상 / 하
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
