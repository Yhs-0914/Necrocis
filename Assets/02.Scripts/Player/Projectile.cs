using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// Pooled projectile movement and hit handling.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private const int HitBufferSize = 8;

        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField] private float hitCheckRadius = 0.35f;
        [SerializeField] private float hitCheckHeightOffset = 0.75f;
        [SerializeField] private float hitCheckVerticalHalfHeight = 2.5f;

        private Vector3 moveDirection;
        private float flightHeight;
        private float damage;
        private float deactivateTime;
        private bool hasImpacted;
        private readonly Collider[] hitBuffer = new Collider[HitBufferSize];

        public void Launch(Vector3 direction, float damage)
        {
            Launch(direction, damage, targetMask);
        }

        public void Launch(Vector3 direction, float damage, LayerMask mask)
        {
            direction.y = 0f;
            moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            this.damage = damage;
            targetMask = mask;
            flightHeight = transform.position.y;
            hasImpacted = false;
        }

        private void OnEnable()
        {
            // Object pooling rule: return to pool by disabling, do not destroy.
            deactivateTime = Time.time + lifeTime;
            hasImpacted = false;
        }

        private void Update()
        {
            Vector3 nextPosition = transform.position + moveDirection * speed * Time.deltaTime;
            nextPosition.y = flightHeight;
            transform.position = nextPosition;
            TryDetectHitByOverlap();

            if (Time.time >= deactivateTime)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleHit(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
            {
                return;
            }

            HandleHit(collision.collider);
        }

        private void HandleHit(Collider other)
        {
            if (other == null || hasImpacted)
            {
                return;
            }

            if ((targetMask.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }

            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy == null)
            {
                enemy = other.GetComponentInParent<EnemyController>();
            }

            if (enemy == null || enemy.IsDead)
            {
                return;
            }

            hasImpacted = true;
            enemy.TakeDamage(damage);
            gameObject.SetActive(false);
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

            int count = Physics.OverlapCapsuleNonAlloc(
                capsuleTop,
                capsuleBottom,
                radius,
                hitBuffer,
                targetMask,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider collider = hitBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                HandleHit(collider);
                if (hasImpacted)
                {
                    break;
                }
            }
        }
    }
}
