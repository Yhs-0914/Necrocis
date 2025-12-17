using UnityEngine;

public class Projectile : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                float damage = PlayerStats.Instance.GetAttack();
                enemy.TakeDamage(damage);
            }
            
            Destroy(gameObject);
        }
    }
}