using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    
    private float attackCooldown = 0f;
    private PlayerMovement playerMovement;
    
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }
    
    void Update()
    {
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
        
        if (Input.GetKeyDown(KeyCode.Q) && attackCooldown <= 0)
        {
            Attack();
        }
    }
    
    void Attack()
    {
        // 마지막 이동 방향으로 발사
        Vector3 attackDirection = playerMovement.lastMoveDirection;
        
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.linearVelocity = attackDirection * projectileSpeed;
        
        float attackSpeed = PlayerStats.Instance.GetAttackSpeed();
        attackCooldown = 1f / attackSpeed;
        
        Destroy(projectile, 2f);
    }
}