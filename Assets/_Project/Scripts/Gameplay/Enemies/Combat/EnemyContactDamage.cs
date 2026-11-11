using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 모든 적의 접촉 피격 순서를 통일한다.
    /// 접촉 → Health 피해 및 무적/점멸 시작 → 넉백 순서로 처리된다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyContactDamage : MonoBehaviour
    {
        private EnemyController sourceEnemy;
        private float damage;
        private float knockbackDistance;
        private bool damageActive;

        public EnemyController SourceEnemy => sourceEnemy;

        public void Configure(
            EnemyController source,
            bool active,
            float contactDamage,
            float contactKnockbackDistance)
        {
            sourceEnemy = source;
            damage = Mathf.Max(0f, contactDamage);
            knockbackDistance = Mathf.Max(0f, contactKnockbackDistance);
            SetDamageActive(active);
        }

        public void SetDamageActive(bool active)
        {
            damageActive = active;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision != null)
            {
                TryApplyTo(collision.collider);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision != null)
            {
                TryApplyTo(collision.collider);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TryApplyTo(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryApplyTo(other);
        }

        public bool TryApplyTo(Collider other)
        {
            PlayerController player = other != null
                ? other.GetComponent<PlayerController>() ?? other.GetComponentInParent<PlayerController>()
                : null;
            return TryApplyTo(player);
        }

        public bool TryApplyTo(PlayerController player)
        {
            if (!damageActive
                || damage <= 0f
                || sourceEnemy == null
                || sourceEnemy.PatternOwnsContact
                || sourceEnemy.IsDead
                || player == null
                || player.IsDead)
            {
                return false;
            }

            Health health = player.HealthComponent;
            if (health == null || health.IsDead)
            {
                return false;
            }

            // An expanded attack collider must not turn the whole weapon volume into body contact.
            var collider = sourceEnemy.GetComponent<Collider>();
            float skin = (collider != null ? collider.contactOffset : 0) + (player.HitCollider != null ? player.HitCollider.contactOffset : 0);
            if (!CombatHitGeometry.BoundsTouchCollider(CombatHitGeometry.EnemyBodyBounds(sourceEnemy), player.HitCollider, skin)) return false;

            DamageResult result = player.TryTakeDamage(sourceEnemy.CreateContactDamage(damage));
            if (!result.HitAccepted || result.HpBlock == DamageBlockReason.Absorbed || player.IsDead)
            {
                return false;
            }

            ApplyKnockback(player);
            return true;
        }

        private void ApplyKnockback(PlayerController player)
        {
            if (knockbackDistance <= 0f)
            {
                return;
            }

            Vector3 direction = player.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = -player.GetLogicalFacingDirection();
                direction.y = 0f;
            }

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.forward;
            }

            player.TryMoveByWorld(direction.normalized * knockbackDistance);
        }
    }
}
