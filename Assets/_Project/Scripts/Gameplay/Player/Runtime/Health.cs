using UnityEngine;
using System;
using System.Collections;

namespace Necrocis
{
    /// <summary>
    /// 플레이어 체력 관리.
    /// CharacterStats 백엔드를 사용하며, 무적 시간 처리를 담당.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Tooltip("피격 후 무적 시간(초)")]
        [SerializeField] private float invincibilityDuration = 0.2f;
        [SerializeField] private bool disableIncomingDamageForTest = true;

        private bool isInvincible; // 현재 무적 상태인지

        // PlayerStats의 CharacterStats를 참조 (PlayerStats가 아직 없으면 null 반환)
        private CharacterStats Stats => PlayerStats.Instance?.RuntimeStats;

        public float CurrentHealth => Stats?.CurrentHealth ?? 0f;
        public float MaxHealth => Stats?.MaxHealth ?? 0f;
        public bool IsDead => Stats?.IsDead ?? false;

        public event Action<float, float> OnHealthChanged; // HP 변경 시 (현재HP, 최대HP)
        public event Action OnDeath;                         // 사망 시

        private bool subscribed; // CharacterStats 이벤트 구독 완료 여부

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Update()
        {
            if (!subscribed)
                TrySubscribe();
        }

        // PlayerStats가 준비되면 HP 변경 이벤트 구독
        // OnEnable 시점에 PlayerStats가 없을 수 있어서 Update에서도 재시도
        private void TrySubscribe()
        {
            if (subscribed || Stats == null) return;
            Stats.HealthChanged += HandleHealthChanged;
            subscribed = true;
        }

        private void OnDisable()
        {
            if (Stats != null)
                Stats.HealthChanged -= HandleHealthChanged;
            subscribed = false;
        }

        private void HandleHealthChanged(CharacterStats sender, CharacterHealthChangedEventArgs args)
        {
            if (args.CurrentValue <= 0f && args.PreviousValue > 0f)
            {
                if (TryReviveFromSplitRegeneration(args.MaxValue))
                {
                    return;
                }

                OnHealthChanged?.Invoke(args.CurrentValue, args.MaxValue);
                OnDeath?.Invoke();
                return;
            }

            OnHealthChanged?.Invoke(args.CurrentValue, args.MaxValue);
        }

        private bool TryReviveFromSplitRegeneration(float maxHealth)
        {
            if (Stats != null && Stats.CurrentHealth > 0f)
            {
                return true;
            }

            PlayerItemCombatEffects itemEffects = GetComponent<PlayerItemCombatEffects>();
            if (itemEffects == null || Stats == null)
            {
                return false;
            }

            if (!itemEffects.TryConsumeSplitRegeneration(0f, maxHealth, out float reviveHealth))
            {
                return false;
            }

            Stats.RestoreHealth(reviveHealth);
            StartCoroutine(InvincibilityCoroutine());
            return true;
        }

        // 데미지 처리: 무적/사망 체크 → 실제 데미지 적용 → 무적 시작
        public void TakeDamage(float damageAmount, EnemyController sourceEnemy = null)
        {
            if (disableIncomingDamageForTest || isInvincible || IsDead || damageAmount <= 0f) return;

            float actualDamage = Mathf.Max(0f, damageAmount);
            PlayerItemCombatEffects itemEffects = GetComponent<PlayerItemCombatEffects>();
            if (itemEffects != null)
            {
                actualDamage = itemEffects.ProcessIncomingDamage(actualDamage, sourceEnemy);
            }

            if (actualDamage <= 0f)
            {
                return;
            }

            if (itemEffects != null && Stats != null)
            {
                float currentHealth = Stats.CurrentHealth;
                float maxHealth = Stats.MaxHealth;
                if (actualDamage >= currentHealth && itemEffects.TryConsumeSplitRegeneration(currentHealth, maxHealth, out float reviveHealth))
                {
                    float targetHealth = Mathf.Clamp(reviveHealth, 0f, maxHealth);
                    if (currentHealth > targetHealth)
                    {
                        float damageToApply = currentHealth - targetHealth;
                        Stats.ApplyDamage(damageToApply);
                    }
                    else if (targetHealth > currentHealth)
                    {
                        Stats.RestoreHealth(targetHealth - currentHealth);
                    }

                    StartCoroutine(InvincibilityCoroutine());
                    return;
                }
            }
            Stats?.ApplyDamage(actualDamage);

            StartCoroutine(InvincibilityCoroutine());
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Stats?.RestoreHealth(amount);
        }

        public void ResetHealth()
        {
            isInvincible = false;
            Stats?.ResetHealthToMax();
        }

        // 외부에서 호출 가능한 임시 무적 부여 (레벨업 후 등)
        public void GrantTemporaryInvincibility(float duration)
        {
            StartCoroutine(InvincibilityCoroutine(duration));
        }

        private IEnumerator InvincibilityCoroutine()
        {
            isInvincible = true;
            yield return new WaitForSeconds(invincibilityDuration);
            isInvincible = false;
        }

        private IEnumerator InvincibilityCoroutine(float duration)
        {
            isInvincible = true;
            yield return new WaitForSeconds(duration);
            isInvincible = false;
        }
    }
}
