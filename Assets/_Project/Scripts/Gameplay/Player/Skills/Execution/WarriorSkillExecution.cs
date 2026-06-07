using System;
using System.Collections;
using UnityEngine;

namespace Necrocis
{
    public partial class PlayerClassSkillController
    {

        private void ExecuteWarriorSkill1Bite()
        {
            Vector3 center = GetSkillCenter(0f);
            if (!TryFindNearestEnemyInForwardArc(center, warriorSkill1.range, warriorSkill1.forwardAngle, out EnemyController target))
            {
                if (enableDebugLogs)
                {
                    Debug.Log("Warrior Skill E failed: no enemy in range.");
                }
                return;
            }

            float damage = PlayerCombatCalculator.GetSkillDamage(warriorSkill1.damage, CurrentPlayerStats);
            target.TakeDamage(damage);

            EnemyStatusEffectController status = EnsureStatusController(target);
            float bleedTickDamage = PlayerCombatCalculator.GetSkillDamage(warriorSkill1.bleedTickDamage, CurrentPlayerStats);
            status?.ApplyBleed(warriorSkill1.bleedDuration, warriorSkill1.bleedTickInterval, bleedTickDamage);

            Vector3 effectPos = GetTargetEffectPosition(target);
            SpawnSkillEffect(
                warriorSkill1.hitEffectPrefab,
                effectPos,
                warriorSkill1.hitEffectLifetime,
                warriorSkill1.fallbackEffectScale,
                new Color(0.85f, 0.1f, 0.1f, 0.6f));

            if (enableDebugLogs)
            {
                Debug.Log($"Warrior Skill E hit {target.name}. Damage={damage}, Bleed={bleedTickDamage}/s for {warriorSkill1.bleedDuration}s");
            }
        }


        private IEnumerator ExecuteWarriorSkill2Dash()
        {
            AudioManager.Instance?.PlaySFX("WarriorSkill2");

            // 전방 적 탐색
            if (!TryFindForwardEnemyPoint(warriorSkill2.searchRange, warriorSkill2.searchAngle, out Vector3 targetPoint))
            {
                // 전방에 적이 없으면 그냥 전방으로 짧게 돌진
                targetPoint = transform.position + GetFacingDirection() * warriorSkill2.searchRange;
            }

            Vector3 dashTarget = targetPoint;
            dashTarget.y = transform.position.y;

            Vector3 dashDir = (dashTarget - transform.position);
            dashDir.y = 0f;
            float dashDistance = dashDir.magnitude;

            if (dashDistance > 0.01f)
            {
                dashDir = dashDir.normalized;
                float dashDuration = dashDistance / Mathf.Max(0.1f, warriorSkill2.dashSpeed);
                float elapsed = 0f;

                while (elapsed < dashDuration)
                {
                    float step = warriorSkill2.dashSpeed * Time.deltaTime;
                    playerController.TryMoveByWorld(dashDir * step);

                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            // 도착 후 범위 내 적에게 데미지 + 구속
            Vector3 hitCenter = GetSkillCenter(0f);
            if (TryFindNearestEnemyInRadius(hitCenter, warriorSkill1.range + 1f, out EnemyController hitTarget))
            {
                float damage = PlayerCombatCalculator.GetSkillDamage(warriorSkill2.damage, CurrentPlayerStats);
                hitTarget.TakeDamage(damage);
                EnemyStatusEffectController status = EnsureStatusController(hitTarget);
                status?.ApplyStun(warriorSkill2.rootDuration);

                Vector3 effectPos = GetTargetEffectPosition(hitTarget);
                SpawnSkillEffect(
                    warriorSkill2.hitEffectPrefab,
                    effectPos,
                    warriorSkill2.hitEffectLifetime,
                    warriorSkill2.fallbackEffectScale,
                    new Color(0.9f, 0.2f, 0.05f, 0.7f));

                if (enableDebugLogs)
                    Debug.Log($"Warrior Skill R hit {hitTarget.name}. Damage={damage}, Root={warriorSkill2.rootDuration}s");
            }
            else if (enableDebugLogs)
            {
                Debug.Log("Warrior Skill R dash: no enemy at destination.");
            }
        }

    }
}
