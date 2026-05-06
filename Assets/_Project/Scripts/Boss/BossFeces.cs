using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 장 중간보스가 발사하는 배설물 투사체.
    /// 포물선으로 날아가 착지 시 기생충 소환 + 플레이어 충돌 시 1데미지 + 20% 이동속도 감소 3초.
    /// </summary>
    public class BossFeces : MonoBehaviour
    {
        [SerializeField] private float flightTime = 1.2f;
        [SerializeField] private float arcHeight = 4f;
        [SerializeField] private float collisionDamage = 1f;
        [SerializeField] private float slowDuration = 3f;
        [SerializeField] private float slowRatio = 0.20f;

        private Vector3 startPos;
        private Vector3 targetPos;
        private float elapsed;
        private bool landed;
        private bool playerHit;

        private BossParasite parasitePrefab;
        private int minParasites;
        private int maxParasites;

        public void Initialize(Vector3 target, BossParasite prefab, int min, int max)
        {
            startPos = transform.position;
            targetPos = target;
            parasitePrefab = prefab;
            minParasites = min;
            maxParasites = max;

            BoxCollider col = GetComponent<BoxCollider>();
            if (col == null) col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(0.8f, 0.8f, 0.8f);

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = gameObject.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.5f, 0.35f, 0.1f);
                sr.sortingOrder = 5;
            }

            if (GetComponent<Billboard>() == null)
                gameObject.AddComponent<Billboard>();
        }

        private void Update()
        {
            if (landed) return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flightTime);

            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            transform.position = pos;

            if (t >= 1f)
                Land();
        }

        private void Land()
        {
            landed = true;

            int count = Random.Range(minParasites, maxParasites + 1);
            for (int i = 0; i < count; i++)
            {
                if (parasitePrefab == null) break;
                Vector2 r = Random.insideUnitCircle * 1.2f;
                Vector3 spawnPos = transform.position + new Vector3(r.x, 0f, r.y);
                Instantiate(parasitePrefab, spawnPos, Quaternion.identity);
            }

            Destroy(gameObject, 0.15f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (landed || playerHit) return;

            PlayerController player = other.GetComponent<PlayerController>()
                ?? other.GetComponentInParent<PlayerController>();
            if (player == null) return;

            playerHit = true;
            player.TakeDamage(collisionDamage);
            PlayerSlowEffect.Apply(player, slowRatio, slowDuration);
        }
    }
}
