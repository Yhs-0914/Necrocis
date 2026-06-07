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
        [SerializeField] private float invincibilityDuration = 0.5f;
        [SerializeField] private float hitFlashInterval = 0.08f;
        [SerializeField] private Color hitFlashColor = new Color(1f, 0.18f, 0.12f, 1f);

        private bool isInvincible; // 현재 무적 상태인지
        private Coroutine invincibilityRoutine;
        private SpriteRenderer[] cachedSpriteRenderers;
        private Color[] cachedSpriteColors;

        // PlayerStats의 CharacterStats를 참조 (PlayerStats가 아직 없으면 null 반환)
        private CharacterStats Stats => PlayerStats.Instance?.RuntimeStats;

        public float CurrentHealth => Stats?.CurrentHealth ?? 0f;
        public float MaxHealth => Stats?.MaxHealth ?? 0f;
        public bool IsDead => Stats?.IsDead ?? false;
        public bool IsInvincible => isInvincible;

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
            StopInvincibilityRoutine();
            RestoreSpriteColors();
        }

        private void HandleHealthChanged(CharacterStats sender, CharacterHealthChangedEventArgs args)
        {
            OnHealthChanged?.Invoke(args.CurrentValue, args.MaxValue);

            if (args.CurrentValue <= 0f && args.PreviousValue > 0f)
            {
                if (TryReviveFromSplitRegeneration(args.MaxValue))
                {
                    return;
                }

                OnDeath?.Invoke();

                PlayerController player = GetComponent<PlayerController>();
                if (player == null)
                    player = GetComponentInParent<PlayerController>();

                player?.HandleDeath();
            }
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
            StartInvincibility(invincibilityDuration, true);
            return true;
        }

        // 데미지 처리: 무적/사망 체크 → 실제 데미지 적용 → 무적 시작
        public void TakeDamage(float damageAmount, EnemyController sourceEnemy = null)
        {
            if (isInvincible || IsDead || damageAmount <= 0f) return;
            if (Stats == null) return;

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

            if (itemEffects != null)
            {
                float currentHealth = Stats.CurrentHealth;
                float maxHealth = Stats.MaxHealth;
                if (actualDamage >= currentHealth
                    && itemEffects.TryConsumeSplitRegeneration(currentHealth, maxHealth, out float reviveHealth))
                {
                    float targetHealth = Mathf.Clamp(reviveHealth, 0f, maxHealth);
                    if (currentHealth > targetHealth)
                    {
                        Stats.ApplyDamage(currentHealth - targetHealth);
                    }
                    else if (targetHealth > currentHealth)
                    {
                        Stats.RestoreHealth(targetHealth - currentHealth);
                    }

                    StartInvincibility(invincibilityDuration, true);
                    return;
                }
            }

            AudioManager.Instance?.PlaySFX("PlayerHit"); // [Sound] 피격
            Stats.ApplyDamage(actualDamage);

            if (!IsDead)
            {
                StartInvincibility(invincibilityDuration, true);
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Stats?.RestoreHealth(amount);
        }

        public void ResetHealth()
        {
            StopInvincibilityRoutine();
            RestoreSpriteColors();
            Stats?.ResetHealthToMax();
        }

        // 외부에서 호출 가능한 임시 무적 부여 (레벨업 후 등)
        public void GrantTemporaryInvincibility(float duration)
        {
            StartInvincibility(duration, false);
        }

        private void StartInvincibility(float duration, bool flash)
        {
            StopInvincibilityRoutine();

            if (duration <= 0f)
            {
                isInvincible = false;
                RestoreSpriteColors();
                return;
            }

            invincibilityRoutine = StartCoroutine(InvincibilityCoroutine(duration, flash));
        }

        private IEnumerator InvincibilityCoroutine(float duration, bool flash)
        {
            isInvincible = true;

            if (!flash)
            {
                yield return new WaitForSeconds(duration);
                isInvincible = false;
                invincibilityRoutine = null;
                yield break;
            }

            CacheSpriteRenderers();
            float elapsed = 0f;
            bool flashOn = true;
            float interval = Mathf.Max(0.03f, hitFlashInterval);

            while (elapsed < duration && !IsDead)
            {
                if (flashOn)
                    ApplySpriteColor(hitFlashColor);
                else
                    ApplyOriginalSpriteColors();

                flashOn = !flashOn;
                float wait = Mathf.Min(interval, duration - elapsed);
                yield return new WaitForSeconds(wait);
                elapsed += wait;
            }

            RestoreSpriteColors();
            isInvincible = false;
            invincibilityRoutine = null;
        }

        private void StopInvincibilityRoutine()
        {
            if (invincibilityRoutine != null)
            {
                StopCoroutine(invincibilityRoutine);
                invincibilityRoutine = null;
            }

            isInvincible = false;
        }

        private void CacheSpriteRenderers()
        {
            cachedSpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            if (cachedSpriteRenderers == null || cachedSpriteRenderers.Length == 0)
            {
                cachedSpriteColors = Array.Empty<Color>();
                return;
            }

            cachedSpriteColors = new Color[cachedSpriteRenderers.Length];
            for (int i = 0; i < cachedSpriteRenderers.Length; i++)
            {
                cachedSpriteColors[i] = cachedSpriteRenderers[i] != null
                    ? cachedSpriteRenderers[i].color
                    : Color.white;
            }
        }

        private void ApplySpriteColor(Color color)
        {
            if (cachedSpriteRenderers == null)
            {
                return;
            }

            for (int i = 0; i < cachedSpriteRenderers.Length; i++)
            {
                if (cachedSpriteRenderers[i] != null)
                {
                    cachedSpriteRenderers[i].color = color;
                }
            }
        }

        private void RestoreSpriteColors()
        {
            if (cachedSpriteRenderers == null || cachedSpriteColors == null)
            {
                return;
            }

            ApplyOriginalSpriteColors();
            cachedSpriteRenderers = null;
            cachedSpriteColors = null;
        }

        private void ApplyOriginalSpriteColors()
        {
            if (cachedSpriteRenderers == null || cachedSpriteColors == null)
            {
                return;
            }

            int count = Mathf.Min(cachedSpriteRenderers.Length, cachedSpriteColors.Length);
            for (int i = 0; i < count; i++)
            {
                if (cachedSpriteRenderers[i] != null)
                {
                    cachedSpriteRenderers[i].color = cachedSpriteColors[i];
                }
            }
        }
    }
}
