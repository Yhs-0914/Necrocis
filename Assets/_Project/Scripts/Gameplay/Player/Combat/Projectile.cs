using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// Pooled projectile movement and hit handling.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private const int HitBufferSize = 8;

        [SerializeField] private float speed = 15f;    // 투사체 이동 속도
        [SerializeField] private float lifeTime = 3f;  // 수명 (초) — 이후 자동 비활성화
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField] private float hitCheckRadius = 0.35f;
        [SerializeField] private float hitCheckHeightOffset = 0.75f;
        [SerializeField] private float hitCheckVerticalHalfHeight = 2.5f;

        private Vector3 moveDirection; // 이동 방향 (정규화)
        private float flightHeight;
        private float damage;          // 적에게 가할 데미지
        private float deactivateTime;
        private bool hasImpacted;
        private readonly Collider[] hitBuffer = new Collider[HitBufferSize];

        // 외부에서 호출: 방향과 데미지를 설정하여 발사
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

        public void Launch(Vector3 direction, float damage, LayerMask mask, float range)
        {
            Launch(direction, damage, mask);
            float effectiveRange = Mathf.Max(0.05f, range);
            deactivateTime = Time.time + effectiveRange / Mathf.Max(0.01f, speed);
        }

        // 풀에서 활성화될 때 수명 타이머 시작
        private void OnEnable()
        {
            // Object pooling rule: return to pool by disabling, do not destroy.
            deactivateTime = Time.time + lifeTime;
            hasImpacted = false;
        }

        // 매 프레임 방향으로 이동 + 히트 감지
        private void Update()
        {
            Vector3 nextPosition = transform.position + moveDirection * speed * Time.deltaTime;
            nextPosition.y = flightHeight;
            transform.position = nextPosition;
            TryDetectHitByOverlap();

            // 수명 만료 시 자동 비활성화 (풀로 반환)
            if (Time.time >= deactivateTime)
            {
                gameObject.SetActive(false);
            }
        }

        // 트리거 충돌: 적에게 데미지 후 풀로 반환 (비활성화)
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

            EnemyController enemy = other.GetComponent<EnemyController>()
                ?? other.GetComponentInParent<EnemyController>();

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
