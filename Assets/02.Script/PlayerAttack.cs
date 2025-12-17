using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("근거리 공격 (Q)")]
    [SerializeField] private float meleeAttackDamage = 20f;
    [SerializeField] private Vector3 meleeAttackBoxSize = new Vector3(1, 1, 2);
    [SerializeField] private float meleeAttackOffset = 1f;

    [Header("원거리 공격 (W)")]
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("공통 설정")]
    [SerializeField] private float attackCooldown = 1f;

    private float lastAttackTime;
    private Rigidbody rigid;

    private Vector2 moveInput;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        HandleAttackInput();   // 공격은 기존 Q/W 방식 유지
    }

    void FixedUpdate()
    {
        HandleMovementAndRotation(); // 이제 이동만 처리
    }

    // =====================
    // Input System (이동만)
    // =====================
    public void OnMove(InputAction.CallbackContext value)
    {
        moveInput = value.ReadValue<Vector2>();
    }

    // =====================
    // 이동 (회전 제거됨)
    // =====================
    private void HandleMovementAndRotation()
    {
        Vector3 moveDirection =
            transform.forward * moveInput.y +
            transform.right * moveInput.x;

        Vector3 targetVelocity = moveDirection.normalized * moveSpeed;

        rigid.linearVelocity = new Vector3(
            targetVelocity.x,
            rigid.linearVelocity.y,
            targetVelocity.z
        );
    }

    // =====================
    // 공격 (Q / W 그대로)
    // =====================
    private void HandleAttackInput()
    {
        bool canAttack = Time.time >= lastAttackTime + attackCooldown;
        if (!canAttack) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            lastAttackTime = Time.time;
            MeleeAttack();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            lastAttackTime = Time.time;
            RangedAttack();
        }
    }

    private void MeleeAttack()
    {
        Debug.Log("근거리 공격 실행! (Q)");
        Vector3 boxCenter = transform.position + transform.forward * meleeAttackOffset;
        Collider[] hitColliders =
            Physics.OverlapBox(boxCenter, meleeAttackBoxSize / 2, transform.rotation);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                Health enemyHealth = hitCollider.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(meleeAttackDamage);
                }
            }
        }
    }

    private void RangedAttack()
    {
        Debug.Log("원거리 공격 실행! (W)");
        if (projectileSpawnPoint == null)
        {
            Debug.LogError("Spawn Point가 설정되지 않았습니다!");
            return;
        }

        GameObject projectile = ObjectPooler.Instance.GetPooledObject();
        if (projectile != null)
        {
            projectile.transform.position = projectileSpawnPoint.position;
            projectile.transform.rotation = projectileSpawnPoint.rotation;
            projectile.SetActive(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.forward * meleeAttackOffset, meleeAttackBoxSize);
    }
}
