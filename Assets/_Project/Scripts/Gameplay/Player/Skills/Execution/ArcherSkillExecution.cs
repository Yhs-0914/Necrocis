using System;
using System.Collections;
using UnityEngine;

namespace Necrocis
{
    public partial class PlayerClassSkillController
    {

        private void ExecuteArcherSkill1FanShot()
        {
            SkillProjectileDebuff debuff = new SkillProjectileDebuff
            {
                applyPoison = true,
                poisonDuration = archerSkill1.poisonDuration,
                poisonTickInterval = archerSkill1.poisonTickInterval,
                poisonTickDamage = PlayerCombatCalculator.GetSkillDamage(archerSkill1.poisonTickDamage, CurrentPlayerStats)
            };

            int projectileCount = Mathf.Max(1, archerSkill1.projectileCount);
            float fanAngle = Mathf.Max(0f, archerSkill1.fanAngle);
            projectileCount = Mathf.Max(5, projectileCount);

            float projectileSpeed = Mathf.Max(0.01f, archerSkill1.projectileSpeed);
            float targetRange = archerSkill1.projectileRange > 0f ? archerSkill1.projectileRange : 4f;
            float projectileLifeTime = Mathf.Max(0.05f, targetRange / projectileSpeed);

            Vector3 centerDirection = GetFacingDirection();
            float startAngle = -fanAngle * 0.5f;
            float stepAngle = projectileCount > 1 ? fanAngle / (projectileCount - 1) : 0f;

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = startAngle + stepAngle * i;
                Vector3 shotDirection = Quaternion.AngleAxis(angle, Vector3.up) * centerDirection;

                SpawnSkillProjectile(
                    archerSkill1.projectilePrefab,
                    archerSkill1.projectileScale,
                    PlayerCombatCalculator.GetSkillDamage(archerSkill1.projectileDamage, CurrentPlayerStats),
                    projectileSpeed,
                    projectileLifeTime,
                    shotDirection,
                    debuff,
                    false);
            }

            SpawnSkillEffect(
                archerSkill1.shootEffectPrefab,
                GetProjectileSpawnPosition(centerDirection),
                archerSkill1.shootEffectLifetime,
                0.25f,
                new Color(0.7f, 1f, 0.5f, 0.55f));

            if (enableDebugLogs)
            {
                Debug.Log($"Archer Skill E fan-shot fired {projectileCount} piercing projectiles. FanAngle={fanAngle:0.#}, Range={targetRange:0.#}");
            }
        }


        private IEnumerator ExecuteArcherSkill2()
        {
            archerSkill2Running = true;
            Vector3 direction = GetArcherSkill2Direction();

            float aimDuration = Mathf.Max(0f, archerSkill2.aimDuration);
            if (aimDuration > 0f)
            {
                yield return new WaitForSeconds(aimDuration);
            }

            FireArcherSkill2Projectile(direction);
            if (enableDebugLogs)
            {
                Debug.Log("Archer Skill R fired a piercing cell spear.");
            }

            archerSkill2Running = false;
        }


        private Vector3 GetArcherSkill2Direction()
        {
            Vector3 fallbackDirection = GetFacingDirection();
            if (!autoTargetForwardEnemyForArcherSkill2)
            {
                return fallbackDirection;
            }

            float maxDistance = Mathf.Max(archerSkill2.range, archerSkill2.targetForwardOffset, 1f);
            if (!TryFindForwardEnemyPoint(maxDistance, archerSkill2AutoTargetAngle, out Vector3 targetPoint))
            {
                return fallbackDirection;
            }

            Vector3 toTarget = targetPoint - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                return fallbackDirection;
            }

            return toTarget.normalized;
        }


        private void FireArcherSkill2Projectile(Vector3 direction)
        {
            Vector3 moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : GetFacingDirection();
            float range = Mathf.Max(0.5f, archerSkill2.range);
            float speed = Mathf.Max(0.1f, archerSkill2.projectileTravelSpeed);
            float computedLifeTime = range / speed;
            float lifeTime = archerSkill2.projectileLifeTime > 0f
                ? archerSkill2.projectileLifeTime
                : computedLifeTime;

            Vector3 spawnPosition = GetProjectileSpawnPosition(moveDirection);
            GameObject projectileObject = CreateArcherSkill2ProjectileObject(spawnPosition, moveDirection);
            if (projectileObject == null)
            {
                return;
            }

            SkillProjectile projectile = projectileObject.GetComponent<SkillProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<SkillProjectile>();
            }

            projectile.ConfigureHitDetection(
                archerSkill2.projectileHitRadius,
                skillHitHeightOffset,
                skillHitVerticalHalfHeight);

            projectile.Launch(
                moveDirection,
                PlayerCombatCalculator.GetSkillDamage(archerSkill2.projectileDamage, CurrentPlayerStats),
                speed,
                Mathf.Max(0.05f, lifeTime),
                enemyMask,
                false,
                default(SkillProjectileDebuff),
                HandleArcherSkill2EnemyHit);
        }


        private GameObject CreateArcherSkill2ProjectileObject(Vector3 spawnPosition, Vector3 direction)
        {
            GameObject projectileObject = archerSkill2.projectilePrefab != null
                ? RuntimePool.Acquire(archerSkill2.projectilePrefab)
                : AcquireFallbackVisual(
                    ArcherSkill2FallbackProjectilePoolName,
                    PrimitiveType.Cylinder,
                    archerSkill2.projectileScale,
                    new Color(0.95f, 0.72f, 0.25f, 0.9f));
            if (projectileObject == null)
            {
                return null;
            }

            EnsureProjectilePhysics(projectileObject);
            projectileObject.transform.position = spawnPosition;
            projectileObject.transform.rotation = GetArcherSkill2ProjectileRotation(direction, archerSkill2.projectilePrefab != null);

            if (archerSkill2.projectilePrefab != null)
            {
                projectileObject.transform.localScale *= Mathf.Max(0.01f, archerSkill2.projectileScale);
            }
            else
            {
                float thickness = Mathf.Max(0.1f, archerSkill2.projectileVisualThickness);
                float length = Mathf.Max(0.5f, archerSkill2.projectileVisualLength);
                projectileObject.transform.localScale = new Vector3(thickness, length * 0.5f, thickness);
            }

            ConfigureArcherSkill2AfterImage(projectileObject);
            return projectileObject;
        }


        private void ConfigureArcherSkill2AfterImage(GameObject projectileObject)
        {
            if (projectileObject == null)
            {
                return;
            }

            ProjectileAfterImageTrail afterImageTrail = projectileObject.GetComponent<ProjectileAfterImageTrail>();
            if (afterImageTrail == null)
            {
                afterImageTrail = projectileObject.AddComponent<ProjectileAfterImageTrail>();
            }

            SpriteRenderer sourceRenderer = projectileObject.GetComponentInChildren<SpriteRenderer>(true);
            afterImageTrail.Configure(
                sourceRenderer,
                archerSkill2.enableAfterImage,
                archerSkill2.afterImageInterval,
                archerSkill2.afterImageFadeDuration,
                archerSkill2.afterImageStartAlpha,
                archerSkill2.afterImageMaxVisibleCount);
        }


        private Quaternion GetArcherSkill2ProjectileRotation(Vector3 direction, bool usesPrefabVisual)
        {
            Vector3 safeDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            Vector3 sourceAxis = usesPrefabVisual
                ? GetDirectionReferenceAxis(archerSkill2.prefabDirectionAxis)
                : Vector3.up;

            Quaternion rotation = Quaternion.FromToRotation(sourceAxis, safeDirection);
            if (!usesPrefabVisual)
            {
                return rotation;
            }

            Vector3 offsetEuler = archerSkill2.prefabRotationOffsetEuler;
            if (offsetEuler.sqrMagnitude > 0.0001f)
            {
                rotation *= Quaternion.Euler(offsetEuler);
            }

            if (archerSkill2.autoRollToCamera)
            {
                rotation = AlignProjectileRollToCamera(rotation, safeDirection, archerSkill2.prefabSurfaceNormalAxis);
            }

            return rotation;
        }


        private static Vector3 GetDirectionReferenceAxis(ProjectileDirectionReferenceAxis axis)
        {
            switch (axis)
            {
                case ProjectileDirectionReferenceAxis.Up:
                    return Vector3.up;
                case ProjectileDirectionReferenceAxis.Forward:
                    return Vector3.forward;
                default:
                    return Vector3.right;
            }
        }


        private Quaternion AlignProjectileRollToCamera(
            Quaternion baseRotation,
            Vector3 moveDirection,
            ProjectileDirectionReferenceAxis surfaceNormalAxis)
        {
            Camera activeCamera = DontStarveCamera.GetActiveCamera();
            if (activeCamera == null)
            {
                return baseRotation;
            }

            Vector3 targetNormal = Vector3.ProjectOnPlane(-activeCamera.transform.forward, moveDirection);
            if (targetNormal.sqrMagnitude <= 0.0001f)
            {
                targetNormal = Vector3.ProjectOnPlane(activeCamera.transform.up, moveDirection);
                if (targetNormal.sqrMagnitude <= 0.0001f)
                {
                    return baseRotation;
                }
            }

            targetNormal.Normalize();

            Vector3 currentNormal = baseRotation * GetDirectionReferenceAxis(surfaceNormalAxis);
            currentNormal = Vector3.ProjectOnPlane(currentNormal, moveDirection);
            if (currentNormal.sqrMagnitude <= 0.0001f)
            {
                return baseRotation;
            }

            currentNormal.Normalize();
            float rollAngle = Vector3.SignedAngle(currentNormal, targetNormal, moveDirection);
            return Quaternion.AngleAxis(rollAngle, moveDirection) * baseRotation;
        }


        private void HandleArcherSkill2EnemyHit(EnemyController enemy, Vector3 hitPosition)
        {
            if (enemy == null)
            {
                return;
            }

            EnemyStatusEffectController status = EnsureStatusController(enemy);
            if (status == null || !status.IsPoisoned)
            {
                return;
            }

            TriggerArcherSkill2VirusExplosion(enemy, hitPosition);
        }


        private void TriggerArcherSkill2VirusExplosion(EnemyController target, Vector3 fallbackCenter)
        {
            Vector3 center = target != null ? GetTargetEffectPosition(target) : fallbackCenter;
            float effectScaleMultiplier = target != null ? GetTargetEffectScaleMultiplier(target) : 1f;
            float damage = PlayerCombatCalculator.GetSkillDamage(archerSkill2.virusExplosionDamage, CurrentPlayerStats);

            int explosionHit = ApplyAreaSkill(
                center,
                archerSkill2.virusExplosionRadius,
                enemy => enemy.TakeDamage(damage));

            SpawnSkillEffect(
                archerSkill2.virusExplosionEffectPrefab,
                center,
                archerSkill2.virusExplosionEffectLifetime,
                Mathf.Max(0.25f, archerSkill2.virusExplosionRadius * 0.9f),
                new Color(1f, 0.45f, 0.2f, 0.45f),
                effectScaleMultiplier);

            if (enableDebugLogs)
            {
                Debug.Log($"[Archer Skill R] Virus explosion triggered. Hit={explosionHit}, Damage={damage:0.##}");
            }
        }


        private bool TryFindForwardEnemyPoint(float maxDistance, float maxAngle, out Vector3 point)
        {
            point = Vector3.zero;
            EnsureOverlapBuffer();

            Vector3 forward = GetFacingDirection();
            Vector3 origin = transform.position;
            origin.y += skillHitHeightOffset;

            int count = Physics.OverlapSphereNonAlloc(
                origin,
                Mathf.Max(0.1f, maxDistance),
                overlapBuffer,
                enemyMask,
                QueryTriggerInteraction.Collide);

            EnemyController bestEnemy = null;
            float bestScore = float.NegativeInfinity;
            float halfAngle = Mathf.Max(1f, maxAngle) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                EnemyController enemy = collider.GetComponentInParent<EnemyController>();
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - transform.position;
                toEnemy.y = 0f;
                float distance = toEnemy.magnitude;
                if (distance <= 0.01f || distance > maxDistance)
                {
                    continue;
                }

                Vector3 toEnemyDir = toEnemy / distance;
                float angle = Vector3.Angle(forward, toEnemyDir);
                if (angle > halfAngle)
                {
                    continue;
                }

                float forwardScore = Vector3.Dot(forward, toEnemyDir) * 2f;
                float distanceScore = -Mathf.Abs(distance - archerSkill2.targetForwardOffset) * 0.15f;
                float score = forwardScore + distanceScore;
                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestEnemy = enemy;
            }

            if (bestEnemy == null)
            {
                return false;
            }

            point = bestEnemy.transform.position;
            point.y = transform.position.y + skillVerticalOffset;
            return true;
        }

    }
}
