using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 장 중간보스 배설물에서 소환되는 기생충 소환수.
    /// 체력 3, 공격력 0.3, 이동속도 1.1, 공격 딜레이 2.5초.
    /// </summary>
    public class BossParasite : MonoBehaviour
    {
        [SerializeField] public float hp = 3f;
        [SerializeField] public float attackDamage = 0.3f;
        [SerializeField] public float moveSpeed = 1.1f;
        [SerializeField] public float attackDelay = 2.5f;

        private float nextAttackTime;
        private bool isDead;

        public bool IsDead => isDead;

        private void Awake()
        {
            gameObject.tag = "Enemy";

            if (GetComponent<SpriteRenderer>() == null)
            {
                SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.35f, 0.7f, 0.15f);
                sr.sortingOrder = 5;
            }

            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(0.5f, 0.5f, 0.5f);
            col.isTrigger = false;

            if (GetComponent<Billboard>() == null)
                gameObject.AddComponent<Billboard>();
        }

        private void Update()
        {
            if (isDead) return;

            PlayerController player = PlayerController.Instance;
            if (player == null) return;

            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;
            float dist = toPlayer.magnitude;

            if (dist > 0.01f)
                transform.position += toPlayer.normalized * moveSpeed * Time.deltaTime;

            if (dist < 1.2f && Time.time >= nextAttackTime)
            {
                player.TakeDamage(attackDamage);
                nextAttackTime = Time.time + attackDelay;
            }
        }

        public void TakeDamage(float damage)
        {
            if (isDead) return;
            hp -= damage;
            if (hp <= 0f) Die();
        }

        private void Die()
        {
            isDead = true;
            Destroy(gameObject);
        }
    }
}
