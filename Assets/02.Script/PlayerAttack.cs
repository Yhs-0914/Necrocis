using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("공격 설정")]
    [SerializeField] private Transform firePoint; // 투사체 발사 위치

    [Header("근거리 공격 (Q)")]
    [SerializeField] private Vector3 meleeAttackBoxSize = new Vector3(1, 1, 2);
    [SerializeField] private float meleeAttackOffset = 1f;

    private float nextAttackTime = 0f;

    // 외부 스크립트 참조
    private PlayerMovement playerMovement;

    void Start()
    {
        // 2단계에서 만든 이동 스크립트 가져오기
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // 쿨타임 체크
        if (Time.time < nextAttackTime) return;

        // 공격 입력 (기존 Q/W 방식 유지)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            MeleeAttack();
            UpdateCooldown();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            RangedAttack();
            UpdateCooldown();
        }
    }

    // 쿨타임 계산 (공격 속도 스탯 적용)
    void UpdateCooldown()
    {
        // 기본 1초 / 공격속도 스탯 (예: 공속 2면 0.5초 쿨타임)
        float attackSpeed = PlayerStats.Instance.GetAttackSpeed();
        nextAttackTime = Time.time + (1f / attackSpeed);
    }

    private void MeleeAttack()
    {
        Debug.Log("근거리 공격 실행! (Q)");

        // 데미지 가져오기
        float damage = PlayerStats.Instance.GetAttack();

        // 히트 박스 생성
        Vector3 boxCenter = transform.position + transform.forward * meleeAttackOffset;
        Collider[] hitColliders = Physics.OverlapBox(boxCenter, meleeAttackBoxSize / 2, transform.rotation);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                // Health(3단계) 또는 Enemy(1단계) 스크립트 찾기
                Health h = hitCollider.GetComponent<Health>();
                if (h != null) h.TakeDamage(damage);
                else
                {
                    Enemy e = hitCollider.GetComponent<Enemy>();
                    if (e != null) e.TakeDamage(damage);
                }
            }
        }
    }

    private void RangedAttack()
    {
        Debug.Log("원거리 공격 실행! (W)");

        // 풀링 시스템에서 투사체 가져오기
        GameObject projectile = ObjectPooler.Instance.GetPooledObject();

        if (projectile != null)
        {
            if (firePoint == null) firePoint = transform; // 방어 코드

            projectile.transform.position = firePoint.position;

            // [중요] 2단계 PlayerMovement가 기억하는 '마지막 이동 방향'으로 발사
            Vector3 shootDir = playerMovement.lastMoveDirection;
            if (shootDir == Vector3.zero) shootDir = transform.forward; // 정지 상태면 앞쪽

            projectile.transform.rotation = Quaternion.LookRotation(shootDir);
            projectile.SetActive(true);

            // 데미지 주입
            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.SetDamage(PlayerStats.Instance.GetAttack());
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.forward * meleeAttackOffset, meleeAttackBoxSize);
    }
}