using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public class MidBossContactDamage : MonoBehaviour
    {
        private EnemyController sourceBoss;
        private float damage = 1f;
        private float cooldown = 1f;
        private float pushSpeed = 3f;
        private float nextDamageTime;
        private bool damageActive;

        public EnemyController SourceBoss => sourceBoss;

        public void Initialize(EnemyController boss, float contactDamage, float contactCooldown, float contactPushSpeed)
        {
            sourceBoss = boss;
            damage = Mathf.Max(0f, contactDamage);
            cooldown = Mathf.Max(0.1f, contactCooldown);
            pushSpeed = Mathf.Max(0f, contactPushSpeed);
            nextDamageTime = 0f;
        }

        public void SetDamageActive(bool active)
        {
            damageActive = active;
            if (active)
            {
                nextDamageTime = 0f;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision != null)
            {
                HandleContact(collision.collider);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision != null)
            {
                HandleContact(collision.collider);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleContact(other);
        }

        private void OnTriggerStay(Collider other)
        {
            HandleContact(other);
        }

        private void HandleContact(Collider other)
        {
            if (!damageActive)
            {
                return;
            }

            if (sourceBoss != null && sourceBoss.IsDead)
            {
                return;
            }

            PlayerController player = other != null
                ? other.GetComponent<PlayerController>() ?? other.GetComponentInParent<PlayerController>()
                : null;
            if (player == null || player.IsDead)
            {
                return;
            }

            ApplySoftPush(player);
            TryDamage(player);
        }

        private void ApplySoftPush(PlayerController player)
        {
            if (player == null || pushSpeed <= 0f)
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

            float step = pushSpeed * Time.fixedDeltaTime;
            player.TryMoveByWorld(direction.normalized * step);
        }

        private void TryDamage(PlayerController player)
        {
            if (player == null || damage <= 0f || Time.time < nextDamageTime)
            {
                return;
            }

            Health health = player.GetComponent<Health>();
            if (health == null)
            {
                player.TakeDamage(damage);
            }
            else
            {
                health.TakeDamage(damage, sourceBoss);
            }

            nextDamageTime = Time.time + cooldown;
        }
    }
}
