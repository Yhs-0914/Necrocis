using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("적 설정")]
    public float maxHealth = 50f;
    public float moveSpeed = 2f;
    public int expReward = 1;  // 기본 경험치 단위 (레벨별 배율 적용됨)
    
    private float currentHealth;
    private Transform player;
    private CharacterController controller;
    
    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        controller = GetComponent<CharacterController>();
    }
    
    void Update()
    {
        if (player != null)
        {
            // XZ 평면에서만 추적
            Vector3 direction = player.position - transform.position;
            direction.y = 0;
            direction.Normalize();
            
            controller.Move(direction * moveSpeed * Time.deltaTime);
            
            // 플레이어 방향 바라보기
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        Debug.Log($"Enemy took {damage} damage! Remaining HP: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log($"Enemy died! Giving {expReward} EXP");
        LevelUpManager.AddExp(expReward);
        Destroy(gameObject);
    }
}