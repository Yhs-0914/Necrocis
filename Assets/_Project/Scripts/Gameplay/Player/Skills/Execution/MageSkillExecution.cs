using System;
using System.Collections;
using UnityEngine;

namespace Necrocis
{
    public partial class PlayerClassSkillController
    {

        private void ExecuteMageSkill1()
        {
            Vector3 center = GetSkillCenter(mageSkill1.forwardOffset);
            float baseDamage = mageSkill1.baseDamage;
            float bonusMin = Mathf.Min(mageSkill1.additionalDamageMin, mageSkill1.additionalDamageMax);
            float bonusMax = Mathf.Max(mageSkill1.additionalDamageMin, mageSkill1.additionalDamageMax);
            if (bonusMax <= 0f && mageSkill1.additionalDamage > 0f)
            {
                bonusMin = mageSkill1.additionalDamage;
                bonusMax = mageSkill1.additionalDamage;
            }

            int hitCount = ApplyAreaSkill(
                center,
                mageSkill1.radius,
                enemy =>
                {
                    float bonusDamage = UnityEngine.Random.Range(bonusMin, bonusMax + 0.001f);
                    float totalDamage = PlayerCombatCalculator.GetSkillDamage(baseDamage + bonusDamage, CurrentPlayerStats);
                    enemy.TakeDamage(totalDamage);
                    EnemyStatusEffectController status = EnsureStatusController(enemy);
                    status?.ApplyStun(mageSkill1.stunDuration);
                });

            SpawnSkillEffect(
                mageSkill1.areaEffectPrefab,
                center,
                mageSkill1.areaEffectLifetime,
                mageSkill1.fallbackEffectScale,
                new Color(0.35f, 0.8f, 1f, 0.45f),
                GetMageSkill1EffectScaleMultiplier());

            if (enableDebugLogs)
            {
                Debug.Log($"Mage Skill E hit {hitCount} enemies. BonusDamage={bonusMin:0.#}~{bonusMax:0.#}");
            }
        }


        private float GetMageSkill1EffectScaleMultiplier()
        {
            float referenceRadius = Mathf.Max(0.01f, mageSkill1.effectReferenceRadius);
            return Mathf.Max(0.01f, mageSkill1.radius) / referenceRadius;
        }


        private IEnumerator ExecuteMageSkill2(EnemyController target)
        {
            if (target == null || target.IsDead)
            {
                yield break;
            }

            float markLifeTime = Mathf.Max(0.1f, Mathf.Min(mageSkill2.markEffectLifetime, mageSkill2.detonationDelay + 0.05f));
            SpawnAttachedSkillEffect(
                mageSkill2.markEffectPrefab,
                target,
                markLifeTime,
                mageSkill2.markFallbackEffectScale,
                new Color(0.8f, 0.95f, 1f, 0.5f),
                mageSkill2.markHeadOffset);

            float delay = Mathf.Max(0f, mageSkill2.detonationDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (target == null || target.IsDead)
            {
                if (enableDebugLogs)
                {
                    Debug.Log("Mage Skill R canceled: marked target died before detonation.");
                }
                yield break;
            }

            Vector3 targetEffectPosition = GetTargetEffectPosition(target);
            float targetEffectScaleMultiplier = GetTargetEffectScaleMultiplier(target);
            float damage = PlayerCombatCalculator.GetSkillDamage(mageSkill2.baseDamage, CurrentPlayerStats);
            target.TakeDamage(damage);
            EnemyStatusEffectController status = EnsureStatusController(target);
            status?.ApplyDamageTakenIncrease(mageSkill2.damageTakenIncreaseRatio, mageSkill2.damageTakenIncreaseDuration);

            SpawnSkillEffect(
                mageSkill2.explosionEffectPrefab,
                targetEffectPosition,
                mageSkill2.explosionEffectLifetime,
                mageSkill2.fallbackEffectScale,
                new Color(1f, 0.5f, 0.2f, 0.45f),
                targetEffectScaleMultiplier);

            if (enableDebugLogs)
            {
                Debug.Log($"Mage Skill R detonated on {target.name}. Damage={damage:0.##}, Debuff={mageSkill2.damageTakenIncreaseRatio * 100f:0.#}% for {mageSkill2.damageTakenIncreaseDuration:0.##}s");
            }
        }

    }
}
