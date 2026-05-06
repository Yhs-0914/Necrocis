using System;
using System.Collections;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 장(腸) 바이옴 중간보스 - 두꺼비형 생물.
    /// 1페이즈: 도망 + 배설물(기생충 소환) / 2페이즈: 추격 + 배설물 투척 + 점프 강타.
    /// 총 HP 175 (1페이즈 40% = 70, 2페이즈 60% = 105).
    /// </summary>
    public class IntestineMidBossController : MonoBehaviour
    {
        private const float TotalHP = 175f;
        private const float Phase2Threshold = 105f;

        [Header("1페이즈 스탯")]
        [SerializeField] private float phase1Speed = 0.8f;
        [SerializeField] private float phase1Attack = 3.5f;
        [SerializeField] private float phase1DefenseRatio = 0.10f;
        [SerializeField] private float fecesCooldown = 8f;
        [SerializeField] private float fecesPreDelay = 0.5f;
        [SerializeField] private float fleeRadius = 8f;

        [Header("2페이즈 스탯")]
        [SerializeField] private float phase2Speed = 1.2f;
        [SerializeField] private float phase2Attack = 5.0f;
        [SerializeField] private float phase2DefenseRatio = 0.15f;
        [SerializeField] private float jumpSlamCooldown = 7f;
        [SerializeField] private float jumpSlamRadius = 6f;
        [SerializeField] private float jumpSlamDamage = 2f;
        [SerializeField] private float jumpSlamSlowDuration = 3f;
        [SerializeField] private float jumpSlamSlowRatio = 0.30f;
        [SerializeField] private float jumpSlamPreDelay = 0.8f;

        [Header("배설물 / 기생충")]
        [SerializeField] private BossFeces fecesPrefab;
        [SerializeField] private BossParasite parasitePrefab;
        [SerializeField] private int minParasites = 2;
        [SerializeField] private int maxParasites = 3;

        [Header("스프라이트")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] phase1Sprites;
        [SerializeField] private Sprite[] phase2Sprites;
        [SerializeField] private Sprite[] rageTransitionSprites;
        [SerializeField] private Sprite[] jumpSprites;
        [SerializeField] private float animFPS = 8f;

        private float currentHP = TotalHP;
        private int currentPhase = 1;
        private bool isDead;
        private bool isTransitioning;

        private float nextFecesTime;
        private float nextJumpSlamTime;
        private float currentSpeed;
        private float currentDefenseRatio;
        private float currentAttack;

        private Sprite[] activeAnim;
        private int animFrame;
        private float animTimer;

        public bool IsDead => isDead;
        public event Action<IntestineMidBossController> OnDefeated;

        private void Awake()
        {
            currentSpeed = phase1Speed;
            currentDefenseRatio = phase1DefenseRatio;
            currentAttack = phase1Attack;

            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                GameObject go = new GameObject("BossSprite");
                go.transform.SetParent(transform, false);
                spriteRenderer = go.AddComponent<SpriteRenderer>();
                spriteRenderer.color = new Color(0.3f, 0.6f, 0.2f);
                spriteRenderer.sortingOrder = 10;
            }

            if (spriteRenderer.GetComponent<Billboard>() == null)
                spriteRenderer.gameObject.AddComponent<Billboard>();

            BoxCollider col = GetComponent<BoxCollider>();
            if (col == null) col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(2f, 2f, 2f);
            col.center = new Vector3(0f, 1f, 0f);
            col.isTrigger = false;
        }

        private void Start()
        {
            nextFecesTime = Time.time + fecesCooldown;
            nextJumpSlamTime = Time.time + jumpSlamCooldown;
            PlayAnim(phase1Sprites);
        }

        private void Update()
        {
            if (isDead || isTransitioning) return;

            TickAnim();

            PlayerController player = PlayerController.Instance;
            if (player == null) return;

            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;
            float dist = toPlayer.magnitude;

            if (currentPhase == 1)
                Phase1Tick(player, toPlayer, dist);
            else
                Phase2Tick(player, toPlayer, dist);
        }

        // ──── 페이즈 1 ────────────────────────────────────────────────
        private void Phase1Tick(PlayerController player, Vector3 toPlayer, float dist)
        {
            if (dist < fleeRadius)
                transform.position -= toPlayer.normalized * currentSpeed * Time.deltaTime;

            if (Time.time >= nextFecesTime)
            {
                nextFecesTime = Time.time + fecesCooldown;
                StartCoroutine(FireFeces(player.transform.position));
            }
        }

        // ──── 페이즈 2 ────────────────────────────────────────────────
        private void Phase2Tick(PlayerController player, Vector3 toPlayer, float dist)
        {
            transform.position += toPlayer.normalized * currentSpeed * Time.deltaTime;

            if (Time.time >= nextFecesTime)
            {
                nextFecesTime = Time.time + fecesCooldown;
                StartCoroutine(FireFeces(player.transform.position));
            }

            if (Time.time >= nextJumpSlamTime)
            {
                nextJumpSlamTime = Time.time + jumpSlamCooldown;
                StartCoroutine(DoJumpSlam(player));
            }
        }

        // ──── 스킬: 배설물 발사 ───────────────────────────────────────
        private IEnumerator FireFeces(Vector3 targetPos)
        {
            yield return new WaitForSeconds(fecesPreDelay);
            if (isDead || fecesPrefab == null) yield break;

            BossFeces feces = Instantiate(fecesPrefab,
                transform.position + Vector3.up * 0.8f, Quaternion.identity);
            feces.Initialize(targetPos, parasitePrefab, minParasites, maxParasites);
        }

        // ──── 스킬: 점프 강타 ─────────────────────────────────────────
        private IEnumerator DoJumpSlam(PlayerController player)
        {
            PlayAnim(jumpSprites);
            yield return new WaitForSeconds(jumpSlamPreDelay);

            if (!isDead && player != null)
            {
                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist <= jumpSlamRadius)
                {
                    player.TakeDamage(jumpSlamDamage);
                    PlayerSlowEffect.Apply(player, jumpSlamSlowRatio, jumpSlamSlowDuration);
                    Debug.Log($"[IntestineMidBoss] 점프 강타 명중! 거리:{dist:F1}/{jumpSlamRadius}");
                }
            }

            PlayAnim(phase2Sprites);
        }

        // ──── 피격 ────────────────────────────────────────────────────
        public void TakeDamage(float rawDamage)
        {
            if (isDead) return;

            float finalDamage = rawDamage * (1f - currentDefenseRatio);
            currentHP -= finalDamage;
            Debug.Log($"[IntestineMidBoss] HP {currentHP:F0}/{TotalHP}  (피해:{finalDamage:F1})");

            if (currentPhase == 1 && currentHP <= Phase2Threshold && !isTransitioning)
            {
                StartCoroutine(TransitionToPhase2());
                return;
            }

            if (currentHP <= 0f)
                Die();
        }

        // ──── 페이즈 전환 ─────────────────────────────────────────────
        private IEnumerator TransitionToPhase2()
        {
            isTransitioning = true;
            PlayAnim(rageTransitionSprites);
            yield return new WaitForSeconds(1.5f);

            currentPhase = 2;
            currentSpeed = phase2Speed;
            currentDefenseRatio = phase2DefenseRatio;
            currentAttack = phase2Attack;
            PlayAnim(phase2Sprites);
            isTransitioning = false;

            Debug.Log("[IntestineMidBoss] ★ 2페이즈 돌입!");
        }

        // ──── 사망 ────────────────────────────────────────────────────
        private void Die()
        {
            if (isDead) return;
            isDead = true;
            OnDefeated?.Invoke(this);
            Debug.Log("[IntestineMidBoss] 처치 완료");
            Destroy(gameObject, 0.8f);
        }

        private void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.CompareTag("Player"))
                col.gameObject.GetComponent<PlayerController>()?.TakeDamage(currentAttack);
        }

        // ──── 애니메이션 ──────────────────────────────────────────────
        private void PlayAnim(Sprite[] sprites)
        {
            if (sprites == null || sprites.Length == 0) return;
            activeAnim = sprites;
            animFrame = 0;
            animTimer = 0f;
            spriteRenderer.sprite = activeAnim[0];
        }

        private void TickAnim()
        {
            if (activeAnim == null || activeAnim.Length <= 1) return;
            animTimer += Time.deltaTime;
            if (animTimer >= 1f / animFPS)
            {
                animTimer -= 1f / animFPS;
                animFrame = (animFrame + 1) % activeAnim.Length;
                spriteRenderer.sprite = activeAnim[animFrame];
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, fleeRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, jumpSlamRadius);
        }
#endif
    }
}
