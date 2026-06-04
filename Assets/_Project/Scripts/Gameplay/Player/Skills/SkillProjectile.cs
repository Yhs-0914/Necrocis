using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [System.Serializable]
    public struct SkillProjectileDebuff
    {
        public bool applyStun;
        public float stunDuration;

        public bool applyPoison;
        public float poisonDuration;
        public float poisonTickInterval;
        public float poisonTickDamage;

        public bool applyDamageTakenIncrease;
        public float damageTakenIncreaseRatio;
        public float damageTakenIncreaseDuration;
    }

    [DisallowMultipleComponent]
    public class SkillProjectile : MonoBehaviour
    {
        private const int HitBufferSize = 8;

        [SerializeField] private float speed = 14f;
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField] private bool disableOnHit = true;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private float hitEffectLifetime = 1f;
        [SerializeField] private float hitCheckRadius = 0.35f;
        [SerializeField] private float hitCheckHeightOffset = 0.75f;
        [SerializeField] private float hitCheckVerticalHalfHeight = 2.5f;

        private Vector3 moveDirection = Vector3.forward;
        private bool keepFlightHeight;
        private float flightHeight;
        private float damage;
        private SkillProjectileDebuff debuff;
        private float despawnTime;
        private bool initialized;
        private bool hasImpacted;
        private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
        private readonly HashSet<int> hitEnemyIds = new HashSet<int>();
        private Action<EnemyController, Vector3> onEnemyHit;

        public void Launch(
            Vector3 direction,
            float damage,
            float projectileSpeed,
            float projectileLifeTime,
            LayerMask mask,
            bool shouldDisableOnHit,
            SkillProjectileDebuff debuff,
            Action<EnemyController, Vector3> onEnemyHit = null)
        {
            moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            keepFlightHeight = Mathf.Abs(moveDirection.y) <= 0.0001f;
            flightHeight = transform.position.y;
            this.damage = damage;
            speed = Mathf.Max(0.01f, projectileSpeed);
            lifeTime = Mathf.Max(0.05f, projectileLifeTime);
            targetMask = mask;
            disableOnHit = shouldDisableOnHit;
            this.debuff = debuff;
            this.onEnemyHit = onEnemyHit;
            despawnTime = Time.time + lifeTime;
            initialized = true;
            hasImpacted = false;
            hitEnemyIds.Clear();
        }

        public void ConfigureHitDetection(float radius, float heightOffset, float verticalHalfHeight)
        {
            hitCheckRadius = Mathf.Max(0.05f, radius);
            hitCheckHeightOffset = heightOffset;
            hitCheckVerticalHalfHeight = Mathf.Max(0.05f, verticalHalfHeight);
        }

        private void OnEnable()
        {
            if (!initialized)
            {
                despawnTime = Time.time + lifeTime;
            }
        }

        private void Update()
        {
            Vector3 nextPosition = transform.position + moveDirection * speed * Time.deltaTime;
            if (keepFlightHeight)
            {
                nextPosition.y = flightHeight;
            }

            transform.position = nextPosition;
            TryDetectHitByOverlap();

            if (Time.time >= despawnTime)
            {
                ReleaseSelf();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasImpacted)
            {
                return;
            }

            TryApplyHit(other);
        }

        private void TryDetectHitByOverlap()
        {
            if (hasImpacted)
            {
                return;
            }

            float radius = Mathf.Max(0.05f, hitCheckRadius);
            Vector3 hitCenter = transform.position;
            hitCenter.y += hitCheckHeightOffset;
            float halfHeight = Mathf.Max(0.05f, hitCheckVerticalHalfHeight);
            Vector3 capsuleTop = hitCenter + Vector3.up * halfHeight;
            Vector3 capsuleBottom = hitCenter - Vector3.up * halfHeight;

            int hitCount = Physics.OverlapCapsuleNonAlloc(
                capsuleTop,
                capsuleBottom,
                radius,
                hitBuffer,
                targetMask,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < hitCount; i++)
            {
                Collider collider = hitBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                if (TryApplyHit(collider) && hasImpacted)
                {
                    break;
                }
            }
        }

        private bool TryApplyHit(Collider other)
        {
            if (other == null || hasImpacted)
            {
                return false;
            }

            if ((targetMask.value & (1 << other.gameObject.layer)) == 0)
            {
                return false;
            }

            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy == null)
            {
                enemy = other.GetComponentInParent<EnemyController>();
            }

            if (enemy == null || enemy.IsDead)
            {
                return false;
            }

            int enemyId = enemy.GetInstanceID();
            if (!hitEnemyIds.Add(enemyId))
            {
                return false;
            }

            enemy.TakeDamage(damage);
            EnemyStatusEffectController status = EnsureStatusController(enemy);
            if (debuff.applyStun)
            {
                status?.ApplyStun(debuff.stunDuration);
            }

            if (debuff.applyPoison)
            {
                status?.ApplyPoison(debuff.poisonDuration, debuff.poisonTickInterval, debuff.poisonTickDamage);
            }

            if (debuff.applyDamageTakenIncrease)
            {
                status?.ApplyDamageTakenIncrease(debuff.damageTakenIncreaseRatio, debuff.damageTakenIncreaseDuration);
            }

            onEnemyHit?.Invoke(enemy, transform.position);
            SpawnHitEffect(transform.position);

            if (disableOnHit || IsBossTarget(enemy))
            {
                hasImpacted = true;
                ReleaseSelf();
            }

            return true;
        }

        private static bool IsBossTarget(EnemyController enemy)
        {
            return enemy != null
                && (enemy.GetComponent<IntestineBossPattern>() != null
                    || enemy.GetComponent<LiverBossPattern>() != null
                    || enemy.GetComponent<StomachBossPattern>() != null
                    || enemy.GetComponent<LungBossPattern>() != null
                    || enemy.GetComponentInParent<MidBossArenaController>() != null);
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

        private void SpawnHitEffect(Vector3 position)
        {
            if (hitEffectPrefab == null)
            {
                return;
            }

            GameObject effect = RuntimePool.Acquire(hitEffectPrefab);
            if (effect == null)
            {
                return;
            }

            effect.transform.position = position;
            effect.transform.rotation = Quaternion.identity;
            RuntimePool.EnsureAutoReturn(effect)?.Schedule(Mathf.Max(0.1f, hitEffectLifetime));
        }

        private void ReleaseSelf()
        {
            if (TryGetComponent(out PooledRuntimeObject _))
            {
                RuntimePool.Release(gameObject);
                return;
            }

            Destroy(gameObject);
        }

        private void OnDisable()
        {
            hasImpacted = false;
            onEnemyHit = null;
            hitEnemyIds.Clear();
        }
    }
}
