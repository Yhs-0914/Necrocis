using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public partial class PlayerClassSkillController
    {

        private int ApplyAreaSkill(Vector3 center, float radius, Action<EnemyController> apply, float propDamage = 0)
        {
            ResidueRubble.HitArea(center, Mathf.Max(0, radius), Vector3.forward, 180, propDamage, enemyMask, ResidueRubble.NextAttackToken());
            CollectBodyTargets(center, radius, Vector3.forward, 180);
            return ApplyBodyTargets(apply, maxAreaSkillHitTargets);
        }

        private int ApplyForwardArcSkill(Vector3 center, float radius, float forwardAngle, int maxTargets,
            Action<EnemyController> apply, float propDamage = 0)
        {
            Vector3 forward = GetFacingDirection();
            float halfAngle = Mathf.Clamp(forwardAngle * .5f, .5f, 180);
            ResidueRubble.HitArea(center, Mathf.Max(0, radius), forward, halfAngle, propDamage, enemyMask, ResidueRubble.NextAttackToken());
            CollectBodyTargets(center, radius, forward, halfAngle);
            return ApplyBodyTargets(apply, Mathf.Min(maxTargets, maxAreaSkillHitTargets));
        }

        private void CollectBodyTargets(Vector3 center, float radius, Vector3 forward, float halfAngle)
        {
            areaSkillCandidates.Clear();
            var enemies = EnemyController.ActiveEnemyControllers;
            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (!PlayerAttackGeometry.Eligible(enemy, enemyMask)
                    || !PlayerAttackGeometry.InArc(center, forward, radius, halfAngle, enemy)) continue;
                areaSkillCandidates.Add(new AreaSkillCandidate(enemy, PlayerAttackGeometry.DistanceSquared(center, enemy)));
            }
            areaSkillCandidates.Sort(CompareAreaSkillCandidates);
        }

        private int ApplyBodyTargets(Action<EnemyController> apply, int limit)
        {
            int count = Mathf.Min(Mathf.Max(1, limit), areaSkillCandidates.Count);
            for (int i = 0; i < count; i++) apply?.Invoke(areaSkillCandidates[i].Enemy);
            return count;
        }

        private bool TryFindNearestEnemyInForwardArc(Vector3 center, float radius, float forwardAngle, out EnemyController nearestEnemy)
        {
            CollectBodyTargets(center, radius, GetFacingDirection(), Mathf.Clamp(forwardAngle * .5f, .5f, 180));
            nearestEnemy = areaSkillCandidates.Count > 0 ? areaSkillCandidates[0].Enemy : null;
            return nearestEnemy != null;
        }

        private bool TryFindNearestEnemyInRadius(Vector3 center, float radius, out EnemyController nearestEnemy)
        {
            CollectBodyTargets(center, radius, Vector3.forward, 180);
            nearestEnemy = areaSkillCandidates.Count > 0 ? areaSkillCandidates[0].Enemy : null;
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
