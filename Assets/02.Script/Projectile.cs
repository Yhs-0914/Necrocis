using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifeTime = 3f;

    // 데미지는 외부(PlayerAttack)에서 주입받습니다.
    private float damage = 10f;

    public void SetDamage(float newDamage)
    {
        this.damage = newDamage;
    }

    private void OnEnable()
    {
        StartCoroutine(DeactivateAfterTime());
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 1. 3단계 방식(Health 컴포넌트) 체크
            Health enemyHealth = other.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            // 2. 1단계 방식(Enemy 컴포넌트) 체크 (호환성 유지)
            else
            {
                Enemy enemyScript = other.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.TakeDamage(damage);
                }
            }

            gameObject.SetActive(false);
        }
    }

    private IEnumerator DeactivateAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false);
    }
}