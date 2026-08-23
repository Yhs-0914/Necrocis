using System;
using System.Collections;
using UnityEngine;

namespace Necrocis
{
    public partial class PlayerClassSkillController
    {

        private void SpawnSkillProjectile(
            GameObject prefab,
            float fallbackScale,
            float damage,
            float speed,
            float lifeTime,
            Vector3 direction,
            SkillProjectileDebuff debuff,
            bool shouldDisableOnHit = true)
        {
            SpawnSkillProjectile(prefab, fallbackScale, damage, speed, lifeTime, direction, debuff, null, shouldDisableOnHit);
        }


        private void SpawnSkillProjectile(
            GameObject prefab,
            float fallbackScale,
            float damage,
            float speed,
            float lifeTime,
            Vector3 direction,
            SkillProjectileDebuff debuff,
            Vector3? worldSpawnPosition,
            bool shouldDisableOnHit = true)
        {
            Vector3 spawnPosition = worldSpawnPosition ?? GetProjectileSpawnPosition(direction);
            GameObject projectileObject = CreateProjectileObject(prefab, spawnPosition, fallbackScale);
            if (projectileObject == null)
            {
                return;
            }

            SkillProjectile projectile = projectileObject.GetComponent<SkillProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<SkillProjectile>();
            }

            projectile.ConfigureVisualSorting(skillEffectSortingOrder);
            projectile.Launch(direction, damage, speed, lifeTime, enemyMask, shouldDisableOnHit, debuff);
        }


        private GameObject CreateProjectileObject(GameObject prefab, Vector3 position, float fallbackScale)
        {
            GameObject projectileObject = prefab != null
                ? RuntimePool.Acquire(prefab)
                : AcquireFallbackVisual(
                    SkillProjectileFallbackPoolName,
                    PrimitiveType.Sphere,
                    fallbackScale,
                    new Color(0.75f, 1f, 0.65f, 0.9f));

            if (projectileObject == null)
            {
                return null;
            }

            EnsureProjectilePhysics(projectileObject);
            projectileObject.transform.position = position;
            projectileObject.transform.rotation = Quaternion.identity;
            return projectileObject;
        }


        private void EnsureProjectilePhysics(GameObject projectileObject)
        {
            Collider collider = projectileObject.GetComponent<Collider>();
            if (collider == null)
            {
                collider = projectileObject.AddComponent<SphereCollider>();
            }

            collider.enabled = true;
            collider.isTrigger = true;

            Rigidbody body = projectileObject.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = projectileObject.AddComponent<Rigidbody>();
            }

            body.useGravity = false;
            body.isKinematic = true;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }


        private Vector3 GetProjectileSpawnPosition(Vector3 direction)
        {
            Vector3 basePos = skillSpawnPoint != null ? skillSpawnPoint.position : transform.position;
            basePos += direction.normalized * projectileSpawnOffset;
            basePos.y += projectileSpawnHeight + skillVerticalOffset;
            return basePos;
        }


        private Vector3 GetSkillCenter(float forwardOffset)
        {
            Vector3 center = transform.position + GetFacingDirection() * forwardOffset;
            center.y = transform.position.y + skillVerticalOffset;
            return center;
        }


        private Vector3 GetFacingDirection()
        {
            if (playerController == null)
            {
                return Vector3.forward;
            }

            Vector3 direction = playerController.GetLogicalFacingDirection();
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }


        private void ResolveSkillSpawnPoint()
        {
            if (IsValidRootSpawnPoint(skillSpawnPoint))
            {
                return;
            }

            Transform found = transform.Find("SkillSpawnPoint");
            if (!IsValidRootSpawnPoint(found))
            {
                found = transform.Find("FirePoint");
            }

            if (IsValidRootSpawnPoint(found))
            {
                skillSpawnPoint = found;
                return;
            }

            GameObject pointObject = new GameObject("SkillSpawnPoint");
            skillSpawnPoint = pointObject.transform;
            skillSpawnPoint.SetParent(transform, false);
            skillSpawnPoint.localPosition = Vector3.zero;
            skillSpawnPoint.localRotation = Quaternion.identity;
        }


        private bool IsValidRootSpawnPoint(Transform point)
        {
            if (point == null || !point.IsChildOf(transform))
            {
                return false;
            }

            Transform cursor = point;
            while (cursor != null && cursor != transform)
            {
                if (cursor.GetComponent<SpriteRenderer>() != null)
                {
                    return false;
                }

                cursor = cursor.parent;
            }

            return true;
        }


        private void SpawnAttachedSkillEffect(
            GameObject prefab,
            EnemyController target,
            float lifeTime,
            float fallbackScale,
            Color fallbackColor,
            float headOffset)
        {
            if (target == null)
            {
                SpawnSkillEffect(prefab, GetSkillCenter(0f), lifeTime, fallbackScale, fallbackColor);
                return;
            }

            GameObject effect = prefab != null
                ? RuntimePool.Acquire(prefab)
                : AcquireFallbackVisual(
                    SkillAttachedEffectFallbackPoolName,
                    PrimitiveType.Sphere,
                    fallbackScale,
                    fallbackColor);
            if (effect == null)
            {
                return;
            }

            effect.transform.position = target.transform.position;
            effect.transform.rotation = Quaternion.identity;
            ConfigureSkillEffectRenderers(effect);

            TargetAttachedEffect attachedEffect = effect.GetComponent<TargetAttachedEffect>();
            if (attachedEffect == null)
            {
                attachedEffect = effect.AddComponent<TargetAttachedEffect>();
            }

            attachedEffect.Bind(
                target.transform,
                headOffset,
                mageSkill2.scaleEffectByTargetSize,
                mageSkill2.effectReferenceTargetHeight,
                mageSkill2.effectSizeMultiplier,
                mageSkill2.effectMinScaleMultiplier,
                mageSkill2.effectMaxScaleMultiplier,
                true);

            SchedulePoolReturn(effect, lifeTime);
        }


        private GameObject SpawnSkillEffect(
            GameObject prefab,
            Vector3 position,
            float lifeTime,
            float fallbackScale,
            Color fallbackColor,
            float scaleMultiplier = 1f)
        {
            float safeScaleMultiplier = Mathf.Max(0.01f, scaleMultiplier);
            if (prefab != null)
            {
                GameObject effect = RuntimePool.Acquire(prefab);
                if (effect == null)
                {
                    return null;
                }

                effect.transform.position = position;
                effect.transform.rotation = Quaternion.identity;
                effect.transform.localScale *= safeScaleMultiplier;
                ConfigureSkillEffectRenderers(effect);
                SchedulePoolReturn(effect, lifeTime);
                return effect;
            }

            GameObject fallback = AcquireFallbackVisual(
                SkillEffectFallbackPoolName,
                PrimitiveType.Sphere,
                fallbackScale * safeScaleMultiplier,
                fallbackColor);
            if (fallback == null)
            {
                return null;
            }

            fallback.transform.position = position;
            fallback.transform.rotation = Quaternion.identity;
            SchedulePoolReturn(fallback, lifeTime);
            return fallback;
        }


        private void ConfigureSkillEffectRenderers(GameObject effect)
        {
            if (effect == null)
            {
                return;
            }

            SpriteRenderer[] renderers = effect.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            int minimumOrder = int.MaxValue;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    minimumOrder = Mathf.Min(minimumOrder, renderers[i].sortingOrder);
                }
            }

            if (minimumOrder == int.MaxValue)
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                int relativeOrder = Mathf.Max(0, renderer.sortingOrder - minimumOrder);
                renderer.sortingOrder = skillEffectSortingOrder + relativeOrder;
            }
        }


        private static GameObject AcquireFallbackVisual(string poolName, PrimitiveType primitiveType, float scale, Color color)
        {
            GameObject visual = RuntimePool.Acquire(poolName, () => CreatePrimitiveVisualObject(primitiveType, color));
            if (visual == null)
            {
                return null;
            }

            ConfigurePrimitiveVisualObject(visual, scale, color);
            return visual;
        }


        private static GameObject CreatePrimitiveVisualObject(PrimitiveType primitiveType, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            Collider collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                UnityEngine.Object.Destroy(collider);
            }

            Renderer renderer = primitive.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = GetFallbackVisualShader();
                if (shader != null)
                {
                    Material material = new Material(shader)
                    {
                        color = color
                    };
                    renderer.material = material;
                }
            }

            return primitive;
        }


        private static void ConfigurePrimitiveVisualObject(GameObject visual, float scale, Color color)
        {
            if (visual == null)
            {
                return;
            }

            visual.transform.localScale = Vector3.one * Mathf.Max(0.05f, scale);

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = renderer.material;
                if (material != null)
                {
                    material.color = color;
                }
            }
        }


        private static Shader GetFallbackVisualShader()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            return shader;
        }


        private static void SchedulePoolReturn(GameObject obj, float lifeTime)
        {
            RuntimePoolAutoReturn autoReturn = RuntimePool.EnsureAutoReturn(obj);
            autoReturn?.Schedule(Mathf.Max(0.1f, lifeTime));
        }


        private void EnsureOverlapBuffer()
        {
            int desiredSize = Mathf.Max(1, overlapBufferSize);
            if (overlapBuffer != null && overlapBuffer.Length == desiredSize)
            {
                return;
            }

            overlapBuffer = new Collider[desiredSize];
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.identity;

            switch (currentClass)
            {
                case PlayerClassType.Mage:
                    DrawSkillRadius(mageSkill1.forwardOffset, mageSkill1.radius, new Color(0.3f, 0.9f, 1f, 0.4f));
                    DrawSkillRadius(0f, mageSkill2.radius, new Color(1f, 0.5f, 0.2f, 0.4f));
                    break;

                case PlayerClassType.Archer:
                    DrawSkillRadius(archerSkill2.targetForwardOffset, archerSkill2.projectileHitRadius, new Color(0.4f, 1f, 0.4f, 0.6f));
                    DrawArcherSkill2LineGizmo(new Color(1f, 0.45f, 0.25f, 0.65f));
                    break;

                case PlayerClassType.Warrior:
                    DrawSkillArc(warriorSkill1.range, warriorSkill1.forwardAngle, new Color(0.9f, 0.1f, 0.1f, 0.65f));
                    DrawSkillRadius(0f, warriorSkill2.searchRange, new Color(1f, 0.4f, 0.0f, 0.3f));
                    break;
            }
        }


        private void DrawSkillRadius(float forwardOffset, float radius, Color color)
        {
            Vector3 direction = Application.isPlaying ? GetFacingDirection() : Vector3.forward;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.forward;
            }

            Vector3 center = transform.position + direction.normalized * forwardOffset;
            center.y = transform.position.y + skillVerticalOffset;

            Gizmos.color = color;
            Gizmos.DrawWireSphere(center, Mathf.Max(0f, radius));
        }

        private void DrawSkillArc(float radius, float angle, Color color)
        {
            Vector3 forward = Application.isPlaying ? GetFacingDirection() : Vector3.forward;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            Vector3 center = transform.position;
            center.y = transform.position.y + skillVerticalOffset;
            float safeRadius = Mathf.Max(0f, radius);
            float halfAngle = Mathf.Clamp(angle * 0.5f, 0.5f, 180f);
            const int segments = 24;

            Gizmos.color = color;
            Vector3 leftDirection = Quaternion.AngleAxis(-halfAngle, Vector3.up) * forward.normalized;
            Vector3 previousPoint = center + leftDirection * safeRadius;
            Gizmos.DrawLine(center, previousPoint);

            for (int i = 1; i <= segments; i++)
            {
                float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);
                Vector3 currentDirection = Quaternion.AngleAxis(currentAngle, Vector3.up) * forward.normalized;
                Vector3 currentPoint = center + currentDirection * safeRadius;
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }

            Gizmos.DrawLine(center, previousPoint);
        }


        private void DrawArcherSkill2LineGizmo(Color color)
        {
            Vector3 direction = Application.isPlaying ? GetArcherSkill2Direction() : Vector3.forward;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.forward;
            }

            Vector3 start = transform.position + direction.normalized * projectileSpawnOffset;
            start.y = transform.position.y + projectileSpawnHeight + skillVerticalOffset;
            Vector3 end = start + direction.normalized * Mathf.Max(0.5f, archerSkill2.range);

            Gizmos.color = color;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, Mathf.Max(0.05f, archerSkill2.projectileHitRadius));
        }

    }
}
