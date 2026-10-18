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
        private enum HitResult
        {
            Ignored,
            Continued,
            Terminated
        }

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
        private HealthEffectSource damageSource;
        private SkillProjectileDebuff debuff;
        private float despawnTime;
        private bool initialized;
        private Collider hitCollider;
        private bool hasImpacted = true;
        private uint launchGeneration;
        private int visualSortingOrder = 5200;
        private Collider[] hitBuffer = new Collider[HitBufferSize];
        private readonly HashSet<long> hitEnemyIds = new HashSet<long>();
        private Action<EnemyController, Vector3> onEnemyHit;
        private int propAttackToken;

        public void Launch(
            Vector3 direction,
            float damage,
            float projectileSpeed,
            float projectileLifeTime,
            LayerMask mask,
            bool shouldDisableOnHit,
            SkillProjectileDebuff debuff,
            Action<EnemyController, Vector3> onEnemyHit = null,
            HealthEffectSource? source = null)
        {
            unchecked { launchGeneration++; }
            moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            keepFlightHeight = Mathf.Abs(moveDirection.y) <= 0.0001f;
            flightHeight = transform.position.y;
            this.damage = damage;
            damageSource = source ?? HealthEffectSource.Legacy;
            speed = Mathf.Max(0.01f, projectileSpeed);
            lifeTime = Mathf.Max(0.05f, projectileLifeTime);
            targetMask = mask;
            disableOnHit = shouldDisableOnHit;
            this.debuff = debuff;
            this.onEnemyHit = onEnemyHit;
            hitCollider = GetComponent<Collider>();
            propAttackToken = ResidueRubble.NextAttackToken();
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

        public void ConfigureVisualSorting(int sortingOrder)
        {
            visualSortingOrder = sortingOrder;
            ApplyVisualSorting(gameObject, visualSortingOrder);
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
            uint generation = launchGeneration;
            if (hasImpacted || !IsCurrentFlight(generation)) return;

            Vector3 nextPosition = transform.position + moveDirection * speed * Time.deltaTime;
            if (keepFlightHeight)
            {
                nextPosition.y = flightHeight;
            }

            ResidueRubble.HitAlongSegment(transform.position, nextPosition, hitCheckRadius, damage, targetMask, propAttackToken);
            if (!IsCurrentFlight(generation)) return;
            transform.position = nextPosition;
            TryDetectHitByOverlap();
            if (hasImpacted || !IsCurrentFlight(generation)) return;

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
            uint generation = launchGeneration;
            if (hasImpacted || !IsCurrentFlight(generation))
            {
                return;
            }

            float radius = Mathf.Max(.05f, Mathf.Max(hitCheckRadius, PlayerAttackGeometry.ColliderRadius(hitCollider))) + PlayerAttackGeometry.Padding;
            Vector3 hitCenter = transform.position;
            hitCenter.y += hitCheckHeightOffset;
            float halfHeight = Mathf.Max(0.05f, hitCheckVerticalHalfHeight);
            Vector3 capsuleTop = hitCenter + Vector3.up * halfHeight;
            Vector3 capsuleBottom = hitCenter - Vector3.up * halfHeight;

            var hits=EnemyHitSnapshot.Capsule(capsuleTop,capsuleBottom,
                radius,targetMask,QueryTriggerInteraction.Collide,ref hitBuffer);
            var hitData=hits;
            int hitCount=hits.Length;

            for (int i = 0; i < hitCount; i++)
            {
                if(!hitData[i].IsCurrent)continue;
                Collider collider = hitData[i].Collider;
                if (collider == null)
                {
                    continue;
                }

                HitResult result = TryApplyHit(collider);
                if (result == HitResult.Terminated || hasImpacted || !IsCurrentFlight(generation))
                {
                    break;
                }
            }
        }

        private HitResult TryApplyHit(Collider other)
        {
            uint generation = launchGeneration;
            if (other == null || hasImpacted || !IsCurrentFlight(generation))
            {
                return HitResult.Ignored;
            }

            if ((targetMask.value & (1 << other.gameObject.layer)) == 0)
            {
                return HitResult.Ignored;
            }

            if (other.TryGetComponent(out ResidueRubble rubble))
            {
                bool applied = rubble.ApplyPlayerHit(damage, propAttackToken);
                if (!IsCurrentFlight(generation)) return HitResult.Terminated;
                return applied ? HitResult.Continued : HitResult.Ignored;
            } // Remains in flight after breaking the prop.
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy == null)
            {
                enemy = other.GetComponentInParent<EnemyController>();
            }

            if (enemy == null || enemy.IsDead)
            {
                return HitResult.Ignored;
            }

            Vector3 receivingCenter=transform.position+Vector3.up*hitCheckHeightOffset;
            if(!EnemyReceivingShape.Allows(enemy,other) || !EnemyReceivingShape.IntersectsCapsule(enemy,receivingCenter+Vector3.up*hitCheckVerticalHalfHeight,receivingCenter-Vector3.up*hitCheckVerticalHalfHeight,Mathf.Max(Mathf.Max(.05f,hitCheckRadius),PlayerAttackGeometry.ColliderRadius(hitCollider))+PlayerAttackGeometry.Padding))return HitResult.Ignored;
            long enemyId = enemy.LogicalLife?.Id ?? (long.MinValue+unchecked((uint)enemy.GetInstanceID()));
            if (!hitEnemyIds.Add(enemyId))
            {
                return HitResult.Ignored;
            }

            bool terminates = disableOnHit || IsBossTarget(enemy);
            if (terminates) hasImpacted = true; // Reserve this hit before invoking damage and skill callbacks.
            var hitLife = new EnemyHitLife(enemy);
            try
            {
                enemy.TakeDamage(damage, damageSource);
                if (!IsCurrentFlight(generation)) return HitResult.Terminated;
                EnemyStatusEffectController status = hitLife.IsCurrent ? EnsureStatusController(enemy) : null;
                if (!IsCurrentFlight(generation)) return HitResult.Terminated;
                if (hitLife.IsCurrent && debuff.applyStun)
                {
                    status?.ApplyStun(debuff.stunDuration);
                    if (!IsCurrentFlight(generation)) return HitResult.Terminated;
                }

                if (hitLife.IsCurrent && debuff.applyPoison)
                {
                    status?.ApplyPoison(debuff.poisonDuration, debuff.poisonTickInterval, debuff.poisonTickDamage, damageSource.WithEffect("poison"));
                    if (!IsCurrentFlight(generation)) return HitResult.Terminated;
                }

                if (hitLife.IsCurrent && debuff.applyDamageTakenIncrease)
                {
                    status?.ApplyDamageTakenIncrease(debuff.damageTakenIncreaseRatio, debuff.damageTakenIncreaseDuration);
                    if (!IsCurrentFlight(generation)) return HitResult.Terminated;
                }

                if (hitLife.HasNotRespawned) onEnemyHit?.Invoke(enemy, transform.position);
                if (!IsCurrentFlight(generation)) return HitResult.Terminated;
                SpawnHitEffect(transform.position);
                if (!IsCurrentFlight(generation)) return HitResult.Terminated;
                return terminates ? HitResult.Terminated : HitResult.Continued;
            }
            finally
            {
                if (terminates && IsCurrentFlight(generation)) ReleaseSelf();
            }
        }

        private bool IsCurrentFlight(uint generation) =>
            this != null && launchGeneration == generation && gameObject.activeInHierarchy;

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
            ApplyVisualSorting(effect, visualSortingOrder);
            RuntimePool.EnsureAutoReturn(effect)?.Schedule(Mathf.Max(0.1f, hitEffectLifetime));
        }

        private static void ApplyVisualSorting(GameObject target, int baseSortingOrder)
        {
            if (target == null)
            {
                return;
            }

            SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(true);
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
                renderer.sortingOrder = baseSortingOrder + relativeOrder;
            }
        }

        private void ReleaseSelf()
        {
            hasImpacted = true;
            if (TryGetComponent(out PooledRuntimeObject _))
            {
                RuntimePool.Release(gameObject);
                return;
            }

            Destroy(gameObject);
        }

        private void OnDisable()
        {
            unchecked { launchGeneration++; }
            hasImpacted = true;
            initialized = false;
            onEnemyHit = null;
            hitEnemyIds.Clear();
        }
    }
}
