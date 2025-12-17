using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 추가

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [Tooltip("피격 후 무적 시간(초)")]
    [SerializeField] private float invincibilityDuration = 0.2f;

    private float currentHealth;
    private bool isInvincible = false; // 무적 상태 플래그

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        // 무적 상태이거나 데미지가 0 이하면 함수를 즉시 종료
        if (isInvincible || damageAmount <= 0) return;

        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name}이(가) {damageAmount}의 데미지를 입었습니다. 현재 체력: {currentHealth}");

        // 경직 상태를 활성화하기 위해 다른 스크립트에 신호를 보냅니다.
        // DontRequireReceiver 옵션은 신호를 받을 컴포넌트가 없어도 에러를 발생시키지 않습니다.
        SendMessage("OnHit", SendMessageOptions.DontRequireReceiver);

        // 무적 코루틴을 시작합니다.
        StartCoroutine(InvincibilityCoroutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name}이(가) 파괴되었습니다.");
        Destroy(gameObject);
    }

    // 무적 시간을 처리하는 코루틴
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true; // 무적 상태로 전환
        yield return new WaitForSeconds(invincibilityDuration); // 정해진 시간만큼 대기
        isInvincible = false; // 무적 상태 해제
    }
}