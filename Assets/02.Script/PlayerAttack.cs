using UnityEngine;
using UnityEngine.InputSystem; // [필수] 네임스페이스 추가

public class PlayerAttack : MonoBehaviour
{
    [Header("공격 설정")]
    [SerializeField] private Transform firePoint;

    [Header("근거리 공격")]
    [SerializeField] private Vector3 meleeAttackBoxSize = new Vector3(1, 1, 2);
    [SerializeField] private float meleeAttackOffset = 1f;

    private float nextAttackTime = 0f;
    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Update에서는 쿨타임 계산만 하고, 입력 감지는 제거함
    /* void Update() 
    {
       // 기존의 Input.GetKeyDown(KeyCode.Q) 삭제!
       // Input System이 알아서 OnFire를 호출해줍니다.
    } 
    */

    // ==========================================
    // [NEW] 주신 코드에서 뽑아온 입력 로직 이식
    // ==========================================
    void OnFire(InputValue value)
    {
        // 1. 키를 눌렀는지(isPressed) & 쿨타임이 지났는지 확인
        if (value.isPressed && Time.time >= nextAttackTime)
        {
            // 2. 공격 실행 (원거리/근거리 중 선택)
            // 일단 기본 공격을 '원거리(W)'로 가정하고 연결합니다. 
            // 근거리를 원하시면 MeleeAttack();으로 바꾸세요.
            RangedAttack();

            // 3. 쿨타임 갱신
            float attackSpeed = PlayerStats.Instance.GetAttackSpeed();
            nextAttackTime = Time.time + (1f / attackSpeed);
        }
    }

    private void MeleeAttack()
    {
        // (기존 코드와 동일)
        Debug.Log("근거리 공격!");
        float damage = PlayerStats.Instance.GetAttack();
        Vector3 boxCenter = transform.position + transform.forward * meleeAttackOffset;
        Collider[] hitColliders = Physics.OverlapBox(boxCenter, meleeAttackBoxSize / 2, transform.rotation);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
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
        // (기존 코드와 동일)
        Debug.Log("원거리 공격!");
        GameObject projectile = ObjectPooler.Instance.GetPooledObject();

        if (projectile != null)
        {
            if (firePoint == null) firePoint = transform;

            projectile.transform.position = firePoint.position;

            // [핵심] 조준 로직은 이미 PlayerMovement에 있는 걸 그대로 씀
            // 주신 코드의 lastFacingDirection 역할 = lastMoveDirection
            Vector3 shootDir = playerMovement.lastMoveDirection;
            if (shootDir == Vector3.zero) shootDir = transform.forward;

            projectile.transform.rotation = Quaternion.LookRotation(shootDir);
            projectile.SetActive(true);

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