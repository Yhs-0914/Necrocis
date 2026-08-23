using System;
using System.Collections;
using UnityEngine;

namespace Necrocis
{
    public partial class PlayerClassSkillController
    {

        private void ExecuteWarriorSkill1Cleave()
        {
            Vector3 center = GetSkillCenter(0f);
            Vector3 facingDirection = GetFacingDirection();
            float damage = PlayerCombatCalculator.GetSkillDamage(warriorSkill1.damage, CurrentPlayerStats);
            float bleedTickDamage = PlayerCombatCalculator.GetSkillDamage(warriorSkill1.bleedTickDamage, CurrentPlayerStats);
            int hitCount = ApplyForwardArcSkill(
                center,
                warriorSkill1.range,
                warriorSkill1.forwardAngle,
                warriorSkill1.maxTargets,
                enemy =>
                {
                    enemy.TakeDamage(damage);
                    EnemyStatusEffectController status = EnsureStatusController(enemy);
                    status?.ApplyBleed(warriorSkill1.bleedDuration, warriorSkill1.bleedTickInterval, bleedTickDamage);
                });

            GameObject effect = SpawnSkillEffect(
                warriorSkill1.hitEffectPrefab,
                center,
                warriorSkill1.hitEffectLifetime,
                warriorSkill1.fallbackEffectScale,
                new Color(0.85f, 0.1f, 0.1f, 0.6f));
            OrientSkillEffectToward(effect, center, facingDirection);
            StartCoroutine(AnimateWarriorSkillEffect(effect, warriorSkill1.hitEffectLifetime, 0.72f, 1.08f));

            if (enableDebugLogs)
            {
                Debug.Log($"Warrior Skill E cleave hit {hitCount} targets. Damage={damage}, Bleed={bleedTickDamage}/s for {warriorSkill1.bleedDuration}s");
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
                GameObject effect = SpawnSkillEffect(
                    warriorSkill2.hitEffectPrefab,
                    effectPos,
                    warriorSkill2.hitEffectLifetime,
                    warriorSkill2.fallbackEffectScale,
                    new Color(0.9f, 0.2f, 0.05f, 0.7f));
                StartCoroutine(AnimateWarriorSkillEffect(effect, warriorSkill2.hitEffectLifetime, 0.62f, 1.12f));

                if (enableDebugLogs)
                    Debug.Log($"Warrior Skill R hit {hitTarget.name}. Damage={damage}, Root={warriorSkill2.rootDuration}s");
            }
            else if (enableDebugLogs)
            {
                Debug.Log("Warrior Skill R dash: no enemy at destination.");
            }
        }

        private static void OrientSkillEffectToward(GameObject effect, Vector3 origin, Vector3 direction)
        {
            if (effect == null || direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Camera activeCamera = DontStarveCamera.GetActiveCamera();
            if (activeCamera == null)
            {
                effect.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg);
                return;
            }

            Vector3 screenOrigin = activeCamera.WorldToScreenPoint(origin);
            Vector3 screenForward = activeCamera.WorldToScreenPoint(origin + direction.normalized);
            Vector2 screenDirection = screenForward - screenOrigin;
            if (screenDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float screenAngle = Mathf.Atan2(screenDirection.y, screenDirection.x) * Mathf.Rad2Deg;
            effect.transform.rotation = Quaternion.Euler(0f, 0f, screenAngle);
        }

        private static IEnumerator AnimateWarriorSkillEffect(
            GameObject effect,
            float duration,
            float startScaleMultiplier,
            float endScaleMultiplier)
        {
            if (effect == null)
            {
                yield break;
            }

            float safeDuration = Mathf.Max(0.05f, duration);
            Vector3 baseScale = effect.transform.localScale;
            SpriteRenderer[] renderers = effect.GetComponentsInChildren<SpriteRenderer>(true);
            Color[] baseColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    baseColors[i] = Color.white;
                    continue;
                }

                Color restoredColor = renderers[i].color;
                restoredColor.a = 1f;
                renderers[i].color = restoredColor;
                baseColors[i] = restoredColor;
            }

            float elapsed = 0f;
            while (elapsed < safeDuration && effect != null && effect.activeInHierarchy)
            {
                float normalizedTime = Mathf.Clamp01(elapsed / safeDuration);
                float easedTime = 1f - Mathf.Pow(1f - normalizedTime, 3f);
                effect.transform.localScale = baseScale * Mathf.Lerp(startScaleMultiplier, endScaleMultiplier, easedTime);
                float alphaMultiplier = 1f - normalizedTime * normalizedTime;

                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] == null)
                    {
                        continue;
                    }

                    Color color = baseColors[i];
                    color.a *= alphaMultiplier;
                    renderers[i].color = color;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (effect != null)
            {
                effect.transform.localScale = baseScale;
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null)
                    {
                        renderers[i].color = baseColors[i];
                    }
                }
            }
        }

    }
}
