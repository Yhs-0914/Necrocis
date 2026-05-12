using System.Collections;
using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public class EnemyStatusEffectController : MonoBehaviour
    {
        [SerializeField] private bool enableDebugLogs = true;

        private EnemyController enemy;

        private float stunEndTime;
        private float vulnerabilityEndTime;
        private float damageTakenIncreaseRatio;

        private Coroutine poisonCoroutine;
        private float poisonEndTime;
        private float poisonTickInterval;
        private float poisonTickDamage;

        public bool IsStunned => Time.time < stunEndTime;
        public bool IsPoisoned => Time.time < poisonEndTime;

        public void Initialize(EnemyController owner)
        {
            enemy = owner;
        }

        public void ResetEffects()
        {
            stunEndTime = 0f;
            vulnerabilityEndTime = 0f;
            damageTakenIncreaseRatio = 0f;

            poisonEndTime = 0f;
            poisonTickInterval = 0f;
            poisonTickDamage = 0f;

            if (poisonCoroutine != null)
            {
                StopCoroutine(poisonCoroutine);
                poisonCoroutine = null;
            }
        }

        public float GetIncomingDamageMultiplier()
        {
            if (damageTakenIncreaseRatio > 0f && Time.time >= vulnerabilityEndTime)
            {
                damageTakenIncreaseRatio = 0f;
                vulnerabilityEndTime = 0f;
            }

            return 1f + Mathf.Max(0f, damageTakenIncreaseRatio);
        }

        public void ApplyStun(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            float newEndTime = Time.time + duration;
            if (newEndTime > stunEndTime)
            {
                stunEndTime = newEndTime;
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[EnemyStatus] Stun applied to {EnemyName} for {duration:0.##}s");
            }
        }

        public void ApplyDamageTakenIncrease(float increaseRatio, float duration)
        {
            if (increaseRatio <= 0f || duration <= 0f)
            {
                return;
            }

            damageTakenIncreaseRatio = Mathf.Max(damageTakenIncreaseRatio, increaseRatio);

            float newEndTime = Time.time + duration;
            if (newEndTime > vulnerabilityEndTime)
            {
                vulnerabilityEndTime = newEndTime;
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[EnemyStatus] Damage taken increased on {EnemyName} by {increaseRatio * 100f:0.#}% for {duration:0.##}s");
            }
        }

        public void ApplyBleed(float duration, float tickInterval, float tickDamage)
        {
            // 출혈은 독과 동일한 틱 데미지 구조
            ApplyPoison(duration, tickInterval, tickDamage);

            if (enableDebugLogs)
                Debug.Log($"[EnemyStatus] Bleed applied to {EnemyName} | {duration:0.#}s / {tickDamage:0.#}dmg per {tickInterval:0.#}s");
        }

        public void ApplyPoison(float duration, float tickInterval, float tickDamage)
        {
            if (duration <= 0f || tickInterval <= 0f || tickDamage <= 0f)
            {
                return;
            }

            poisonTickInterval = tickInterval;
            poisonTickDamage = tickDamage;
            poisonEndTime = Mathf.Max(poisonEndTime, Time.time + duration);

            if (poisonCoroutine == null)
            {
                poisonCoroutine = StartCoroutine(PoisonRoutine());
            }

            if (enableDebugLogs)
            {
                Debug.Log($"Poison applied to {EnemyName} for {duration:0.##}s");
            }
        }

        private IEnumerator PoisonRoutine()
        {
            while (Time.time < poisonEndTime)
            {
                float interval = Mathf.Max(0.05f, poisonTickInterval);
                yield return new WaitForSeconds(interval);

                if (enemy == null || enemy.IsDead)
                {
                    break;
                }

                enemy.TakeDamage(poisonTickDamage);
            }

            poisonCoroutine = null;
            poisonEndTime = 0f;
        }

        private void OnDisable()
        {
            ResetEffects();
        }

        private string EnemyName => enemy != null ? enemy.gameObject.name : gameObject.name;
    }
}
