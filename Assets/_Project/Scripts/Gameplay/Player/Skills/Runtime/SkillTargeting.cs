using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public partial class PlayerClassSkillController
    {

        private int ApplyAreaSkill(Vector3 center, float radius, System.Action<EnemyController> apply)
        {
            areaSkillCandidates.Clear();
            float safeRadius = Mathf.Max(0f, radius);
            float radiusSqr = safeRadius * safeRadius;
            IReadOnlyList<EnemyController> enemies = EnemyController.ActiveEnemyControllers;

            if (enemies == null || enemies.Count == 0)
            {
                return 0;
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 enemyPos = enemy.transform.position;
                enemyPos.y = center.y;
                float distanceSqr = (enemyPos - center).sqrMagnitude;
                if (distanceSqr > radiusSqr)
                {
                    continue;
                }

                areaSkillCandidates.Add(new AreaSkillCandidate(enemy, distanceSqr));
            }

            areaSkillCandidates.Sort(CompareAreaSkillCandidates);

            int hitLimit = Mathf.Max(1, maxAreaSkillHitTargets);
            int hitCount = Mathf.Min(areaSkillCandidates.Count, hitLimit);
            for (int i = 0; i < hitCount; i++)
            {
                apply?.Invoke(areaSkillCandidates[i].Enemy);
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[SkillHit] Center={center}, Radius={safeRadius:0.##}, Candidates={areaSkillCandidates.Count}, Hit={hitCount}, MaxHit={hitLimit}");
            }

            return hitCount;
        }

        private int ApplyForwardArcSkill(
            Vector3 center,
            float radius,
            float forwardAngle,
            int maxTargets,
            System.Action<EnemyController> apply)
        {
            areaSkillCandidates.Clear();
            Vector3 forward = GetFacingDirection();
            float safeRadius = Mathf.Max(0f, radius);
            float radiusSqr = safeRadius * safeRadius;
            float halfAngle = Mathf.Clamp(forwardAngle * 0.5f, 0.5f, 180f);
            IReadOnlyList<EnemyController> enemies = EnemyController.ActiveEnemyControllers;

            if (enemies == null || enemies.Count == 0)
            {
                return 0;
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - center;
                toEnemy.y = 0f;
                float distanceSqr = toEnemy.sqrMagnitude;
                if (distanceSqr > radiusSqr)
                {
                    continue;
                }

                if (distanceSqr > 0.0001f && Vector3.Angle(forward, toEnemy) > halfAngle)
                {
                    continue;
                }

                areaSkillCandidates.Add(new AreaSkillCandidate(enemy, distanceSqr));
            }

            areaSkillCandidates.Sort(CompareAreaSkillCandidates);

            int hitLimit = Mathf.Max(1, Mathf.Min(maxTargets, maxAreaSkillHitTargets));
            int hitCount = Mathf.Min(areaSkillCandidates.Count, hitLimit);
            for (int i = 0; i < hitCount; i++)
            {
                apply?.Invoke(areaSkillCandidates[i].Enemy);
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[SkillHit] ForwardArc Radius={safeRadius:0.##}, Angle={forwardAngle:0.#}, Candidates={areaSkillCandidates.Count}, Hit={hitCount}, MaxHit={hitLimit}");
            }

            return hitCount;
        }

        private bool TryFindNearestEnemyInForwardArc(Vector3 center, float radius, float forwardAngle, out EnemyController nearestEnemy)
        {
            nearestEnemy = null;
            Vector3 forward = GetFacingDirection();
            float halfAngle = Mathf.Max(1f, forwardAngle) * 0.5f;
            float safeRadius = Mathf.Max(0f, radius);
            float bestDistanceSqr = float.PositiveInfinity;

            EnsureOverlapBuffer();
            Vector3 hitCenter = center;
            hitCenter.y += skillHitHeightOffset;
            float halfHeight = Mathf.Max(0.05f, skillHitVerticalHalfHeight);
            Vector3 capsuleTop = hitCenter + Vector3.up * halfHeight;
            Vector3 capsuleBottom = hitCenter - Vector3.up * halfHeight;

            int count = Physics.OverlapCapsuleNonAlloc(
                capsuleTop, capsuleBottom, safeRadius,
                overlapBuffer, enemyMask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null || !TryGetEnemyFromCollider(collider, out EnemyController enemy)) continue;

                Vector3 toEnemy = enemy.transform.position - transform.position;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude > 0.0001f && Vector3.Angle(forward, toEnemy.normalized) > halfAngle) continue;

                float distanceSqr = toEnemy.sqrMagnitude;
                if (distanceSqr > bestDistanceSqr) continue;

                bestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }

            if (nearestEnemy != null) return true;

            // 콜라이더 탐지 실패 시 ActiveEnemyControllers 직접 순회
            IReadOnlyList<EnemyController> allEnemies = EnemyController.ActiveEnemyControllers;
            for (int i = 0; i < allEnemies.Count; i++)
            {
                EnemyController enemy = allEnemies[i];
                if (enemy == null || enemy.IsDead) continue;

                Vector3 toEnemy = enemy.transform.position - transform.position;
                toEnemy.y = 0f;
                float distanceSqr = toEnemy.sqrMagnitude;
                if (distanceSqr > safeRadius * safeRadius || distanceSqr > bestDistanceSqr) continue;
                if (toEnemy.sqrMagnitude > 0.0001f && Vector3.Angle(forward, toEnemy.normalized) > halfAngle) continue;

                bestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }

            return nearestEnemy != null;
        }


        private bool TryFindNearestEnemyInRadius(Vector3 center, float radius, out EnemyController nearestEnemy)
        {
            nearestEnemy = null;
            EnsureOverlapBuffer();

            float safeRadius = Mathf.Max(0f, radius);
            Vector3 hitCenter = center;
            hitCenter.y += skillHitHeightOffset;
            float halfHeight = Mathf.Max(0.05f, skillHitVerticalHalfHeight);
            Vector3 capsuleTop = hitCenter + Vector3.up * halfHeight;
            Vector3 capsuleBottom = hitCenter - Vector3.up * halfHeight;

            int count = Physics.OverlapCapsuleNonAlloc(
                capsuleTop,
                capsuleBottom,
                safeRadius,
                overlapBuffer,
                enemyMask,
                QueryTriggerInteraction.Collide);

            float bestDistanceSqr = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null || !TryGetEnemyFromCollider(collider, out EnemyController enemy))
                {
                    continue;
                }

                Vector3 enemyPos = enemy.transform.position;
                enemyPos.y = center.y;
                float distanceSqr = (enemyPos - center).sqrMagnitude;
                if (distanceSqr > bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }

            if (nearestEnemy != null)
            {
                return true;
            }

            IReadOnlyList<EnemyController> enemies = EnemyController.ActiveEnemyControllers;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 enemyPos = enemy.transform.position;
                enemyPos.y = center.y;
                float distanceSqr = (enemyPos - center).sqrMagnitude;
                if (distanceSqr > safeRadius * safeRadius || distanceSqr > bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }

            return nearestEnemy != null;
        }


        private Vector3 GetTargetEffectPosition(EnemyController target)
        {
            if (target == null)
            {
                return GetSkillCenter(0f);
            }

            if (TargetAttachedEffect.TryGetTargetBounds(target.transform, out Bounds targetBounds))
            {
                return targetBounds.center;
            }

            return target.transform.position;
        }


        private float GetTargetEffectScaleMultiplier(EnemyController target)
        {
            if (!mageSkill2.scaleEffectByTargetSize || target == null)
            {
                return 1f;
            }

            float minMultiplier = Mathf.Max(0.05f, mageSkill2.effectMinScaleMultiplier);
            float maxMultiplier = Mathf.Max(minMultiplier, mageSkill2.effectMaxScaleMultiplier);
            float referenceHeight = Mathf.Max(0.01f, mageSkill2.effectReferenceTargetHeight);
            float sizeMultiplier = Mathf.Max(0.01f, mageSkill2.effectSizeMultiplier);

            if (!TargetAttachedEffect.TryGetTargetBounds(target.transform, out Bounds targetBounds))
            {
                return Mathf.Clamp(sizeMultiplier, minMultiplier, maxMultiplier);
            }

            float targetHeight = Mathf.Max(0.01f, targetBounds.size.y);
            float scaleMultiplier = (targetHeight / referenceHeight) * sizeMultiplier;
            return Mathf.Clamp(scaleMultiplier, minMultiplier, maxMultiplier);
        }


        private static int CompareAreaSkillCandidates(AreaSkillCandidate a, AreaSkillCandidate b)
        {
            return a.DistanceSqr.CompareTo(b.DistanceSqr);
        }


        private static bool TryGetEnemyFromCollider(Collider collider, out EnemyController enemy)
        {
            enemy = null;
            if (collider == null)
            {
                return false;
            }

            enemy = collider.GetComponent<EnemyController>();
            if (enemy == null)
            {
                enemy = collider.GetComponentInParent<EnemyController>();
            }

            if (enemy == null)
            {
                enemy = collider.GetComponentInChildren<EnemyController>();
            }

            return enemy != null && !enemy.IsDead;
        }


        private static EnemyStatusEffectController EnsureStatusController(EnemyController enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            EnemyStatusEffectController status = enemy.StatusEffects;
            if (status == null)
            {
                status = enemy.GetComponent<EnemyStatusEffectController>();
            }

            if (status == null)
            {
                status = enemy.gameObject.AddComponent<EnemyStatusEffectController>();
            }

            status.Initialize(enemy);
            return status;
        }

    }
}
