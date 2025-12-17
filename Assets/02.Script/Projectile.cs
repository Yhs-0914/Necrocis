using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifeTime = 3f;

    // Start 대신 OnEnable을 사용합니다. 오브젝트가 활성화될 때마다 호출됩니다.
    private void OnEnable()
    {
        // lifeTime이 지나면 비활성화하는 코루틴을 시작합니다.
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
            Health enemyHealth = other.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }

            // 적과 부딪히면 즉시 비활성화 (풀로 반환)
            gameObject.SetActive(false);
        }
    }

    // 일정 시간 후 비활성화하는 코루틴
    private IEnumerator DeactivateAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false);
    }
}