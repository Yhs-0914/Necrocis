using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
        [Tooltip("분열 재생 부활 후 보호 시간(초)")]
        [SerializeField] private float revivalInvincibilityDuration = 0.5f;
        [SerializeField] private float hitFlashInterval = 0.08f;
        [SerializeField] private Color hitFlashColor = new Color(1f, 0.18f, 0.12f, 1f);

        private bool isInvincible; // 현재 무적 상태인지
        private Coroutine invincibilityRoutine;
        private readonly List<SpriteRenderer> cachedSpriteRenderers = new List<SpriteRenderer>();
        private readonly List<Color> cachedSpriteColors = new List<Color>();
        private PlayerController playerController;
        private PlayerStats playerStats;
        private CharacterStats subscribedStats;
        private PlayerDamageCoordinator damageCoordinator;

        // PlayerStats의 CharacterStats를 참조 (PlayerStats가 아직 없으면 null 반환)
        private CharacterStats Stats => playerStats != null ? playerStats.RuntimeStats : null;

        public float CurrentHealth => Stats?.CurrentHealth ?? 0f;
        public float MaxHealth => Stats?.MaxHealth ?? 0f;
        public bool IsDead => Stats?.IsDead ?? false;
        public bool IsInvincible => isInvincible;

        public event Action<float, float> OnHealthChanged; // HP 변경 시 (현재HP, 최대HP)
        public event Action OnDeath;                         // 사망 시

        private bool subscribed; // CharacterStats 이벤트 구독 완료 여부
        private bool runtimeBound;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = GetComponentInParent<PlayerController>();
            }

            playerStats = GetComponent<PlayerStats>() ?? GetComponentInParent<PlayerStats>();
            damageCoordinator = GetComponent<PlayerDamageCoordinator>();
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Update()
        {
            if (!subscribed)
                TrySubscribe();
        }

        public void BindRuntime(PlayerController owner, PlayerStats ownerStats, PlayerDamageCoordinator coordinator)
        {
            Unsubscribe();
            playerController = owner;
            playerStats = ownerStats;
            damageCoordinator = coordinator;
            runtimeBound = true;
            TrySubscribe();
        }

        private void TrySubscribe()
        {
            if (subscribed || !runtimeBound || !isActiveAndEnabled) return;
            if (playerStats == null) playerStats = GetComponent<PlayerStats>() ?? GetComponentInParent<PlayerStats>();
            if (damageCoordinator == null) damageCoordinator = GetComponent<PlayerDamageCoordinator>();
            if (Stats == null || damageCoordinator == null) return;
            subscribedStats = Stats;
            subscribedStats.HealthChanged += HandleHealthChanged;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (subscribedStats != null) subscribedStats.HealthChanged -= HandleHealthChanged;
            subscribedStats = null;
            subscribed = false;
        }

        private void OnDisable()
        {
            Unsubscribe();
            StopInvincibilityRoutine();
            RestoreSpriteColors();
        }

        private void HandleHealthChanged(CharacterStats sender, CharacterHealthChangedEventArgs args)
        {
            OnHealthChanged?.Invoke(sender.CurrentHealth, sender.MaxHealth);
        }

        internal void NotifyResolvedDeath() => OnDeath?.Invoke();
        internal void BeginHitProtection() => StartInvincibility(invincibilityDuration, true);
        internal void BeginRevivalProtection() => StartInvincibility(revivalInvincibilityDuration, true);

        public void TakeDamage(float damageAmount, EnemyController sourceEnemy = null)
        {
            TryTakeDamage(new EnemyDamageRequest(Mathf.Max(0f,
                damageAmount * DifficultyBalanceService.GetIncomingDamageMultiplier(sourceEnemy)), sourceEnemy));
        }

        public void TakeDamage(EnemyDamageRequest request) => TryTakeDamage(request);

        public DamageResult TryTakeDamage(EnemyDamageRequest request)
        {
            return damageCoordinator != null ? damageCoordinator.TryApplyIncoming(request) : DamageResult.Invalid;
        }

        public void Heal(float amount) => Heal(amount, playerStats != null
            ? playerStats.GetHealthEffectSource("legacy-heal", "player:legacy-heal") : HealthEffectSource.Legacy);

        public void Heal(float amount, HealthEffectSource source)
        {
            if (IsDead || amount <= 0f) return;
            Stats?.RestoreHealth(amount, source);
        }

        public void ResetHealth()
        {
            StopInvincibilityRoutine();
            RestoreSpriteColors();
            if (damageCoordinator != null) damageCoordinator.BeginLifetime();
            else Stats?.BeginHealthLifetime();
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
                RestoreSpriteColors();
                isInvincible = false;
                invincibilityRoutine = null;
                yield break;
            }

            CacheSpriteRenderers();
            float elapsed = 0f;
            float nextFlashTime = 0f;
            bool flashOn = true;
            float interval = Mathf.Max(0.03f, hitFlashInterval);

            while (elapsed < duration && !IsDead)
            {
                if (elapsed >= nextFlashTime)
                {
                    if (flashOn)
                        ApplySpriteColor(hitFlashColor);
                    else
                        ApplyOriginalSpriteColors();

                    flashOn = !flashOn;
                    nextFlashTime += interval;
                }

                yield return null;
                elapsed += Time.deltaTime;
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

            RestoreSpriteColors();
            isInvincible = false;
        }

        private void CacheSpriteRenderers()
        {
            cachedSpriteRenderers.Clear();
            GetComponentsInChildren(true, cachedSpriteRenderers);
            cachedSpriteColors.Clear();
            if (cachedSpriteRenderers.Count == 0)
            {
                return;
            }

            for (int i = 0; i < cachedSpriteRenderers.Count; i++)
            {
                cachedSpriteColors.Add(cachedSpriteRenderers[i] != null
                    ? cachedSpriteRenderers[i].color
                    : Color.white);
            }
        }

        private void ApplySpriteColor(Color color)
        {
            for (int i = 0; i < cachedSpriteRenderers.Count; i++)
            {
                if (cachedSpriteRenderers[i] != null)
                {
                    cachedSpriteRenderers[i].color = color;
                }
            }
        }

        private void RestoreSpriteColors()
        {
            if (cachedSpriteRenderers.Count == 0 || cachedSpriteColors.Count == 0)
            {
                return;
            }

            ApplyOriginalSpriteColors();
        }

        private void ApplyOriginalSpriteColors()
        {
            if (cachedSpriteRenderers.Count == 0 || cachedSpriteColors.Count == 0)
            {
                return;
            }

            int count = Mathf.Min(cachedSpriteRenderers.Count, cachedSpriteColors.Count);
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
