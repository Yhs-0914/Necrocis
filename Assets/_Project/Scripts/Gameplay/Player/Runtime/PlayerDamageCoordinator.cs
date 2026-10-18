using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-9000)]
    public sealed class PlayerDamageCoordinator : MonoBehaviour
    {
        private PlayerController player;
        private CharacterStats stats;
        private PlayerStats ownerStats;
        private Health health;
        private PlayerItemCombatEffects effects;
        private bool subscribed, resolvingRawDeath, deathNotified;

        public CharacterStats Stats => stats;

        private void Awake() => BindLocal();
        private void OnEnable() => BindLocal();
        private void OnDisable() => Unsubscribe();

        private void BindLocal()
        {
            var ownerStats = GetComponent<PlayerStats>();
            if (ownerStats == null) return;
            Bind(GetComponent<PlayerController>(), ownerStats, GetComponent<Health>(), GetComponent<PlayerItemCombatEffects>());
        }

        public void Bind(PlayerController owner, PlayerStats ownerStats, Health ownerHealth, PlayerItemCombatEffects ownerEffects)
        {
            CharacterStats next = ownerStats != null ? ownerStats.RuntimeStats : null;
            if (stats != next) Unsubscribe();
            player = owner; this.ownerStats = ownerStats; health = ownerHealth; effects = ownerEffects; stats = next;
            if (!subscribed && stats != null && isActiveAndEnabled)
            {
                stats.HealthChanged += HandleHealthChanged;
                subscribed = true;
            }
        }

        private void Unsubscribe()
        {
            if (subscribed && stats != null) stats.HealthChanged -= HandleHealthChanged;
            subscribed = false;
        }

        public void BeginLifetime(bool clearRemainders = true)
        {
            deathNotified = false;
            if (clearRemainders)
            {
                if (ownerStats != null) ownerStats.BeginHealthLifetime();
                else stats?.BeginHealthLifetime();
            }
        }

        public DamageResult TryApplyIncoming(EnemyDamageRequest request)
        {
            if (stats == null || stats.IsDead || deathNotified || request.Amount <= 0f) return DamageResult.Invalid;
            if ((health != null && health.IsInvincible) || (player != null && player.IsDashInvincible))
                return new DamageResult(true, 0f, DamageBlockReason.Invincible);
            EnemyController attacker = request.Source;
            Vector3 sourcePosition = attacker != null ? attacker.transform.position : transform.position - Vector3.forward;
            float amount = effects != null ? effects.ProcessIncomingDamage(request.Amount, attacker) : request.Amount;
            if (amount <= 0f) return new DamageResult(true, 0f, DamageBlockReason.Absorbed);
            // Reflection can synchronously change health or the attacker's generation.
            if (stats.IsDead || deathNotified) return new DamageResult(true, 0f, DamageBlockReason.Invalid, died: true);
            return ApplyFinalDamage(amount, request.EffectSource, sourcePosition, true);
        }

        public DamageResult ApplyBypassDamage(float amount, HealthEffectSource source)
        {
            if (stats == null || stats.IsDead || deathNotified || amount <= 0f) return DamageResult.Invalid;
            return ApplyFinalDamage(amount, source, transform.position - Vector3.forward, false);
        }

        private DamageResult ApplyFinalDamage(float amount, HealthEffectSource source, Vector3 sourcePosition, bool showHit)
        {
            HealthDeltaPreview preview = stats.PreviewDamage(amount, source);
            float? revivalHealth = null;
            if (preview.WholeUnits >= stats.CurrentHealth && effects != null
                && effects.TryConsumeSplitRegeneration(stats.CurrentHealth, stats.MaxHealth, out float reviveHealth))
                revivalHealth = reviveHealth;
            if (showHit && preview.WholeUnits >= 1f && !revivalHealth.HasValue)
                AudioManager.Instance?.PlaySFX("PlayerHit");
            if (!stats.TryApplyDamage(preview, out float applied, revivalHealth)) return DamageResult.Invalid;
            if (revivalHealth.HasValue)
            {
                ownerStats?.AdvanceHealthEffectGeneration();
                deathNotified = false;
                health?.BeginRevivalProtection();
                return new DamageResult(true, applied, revived: true);
            }
            if (showHit && applied >= 1f)
            {
                CombatVfx.PlayPlayerHit(transform, sourcePosition, applied, stats.IsDead);
                if (!stats.IsDead) health?.BeginHitProtection();
            }
            return new DamageResult(true, applied, died: stats.IsDead);
        }

        private void HandleHealthChanged(CharacterStats sender, CharacterHealthChangedEventArgs args)
        {
            if (args.CurrentValue <= 0f && sender.IsDead) ResolveRawDeath();
        }

        // Compatibility for direct low-level damage or a MaxHP change outside the incoming path.
        public bool ResolveRawDeath()
        {
            if (stats == null) return false;
            if (!stats.IsDead) return true;
            if (resolvingRawDeath || deathNotified) return false;
            resolvingRawDeath = true;
            try
            {
                if (effects != null && stats.MaxHealth > 0f
                    && effects.TryConsumeSplitRegeneration(0f, stats.MaxHealth, out float reviveHealth))
                {
                    if (ownerStats != null) ownerStats.BeginHealthLifetime();
                    else stats.BeginHealthLifetime();
                    stats.AdjustCurrentHealth(Mathf.Clamp(reviveHealth, Mathf.Min(1f, stats.MaxHealth), stats.MaxHealth));
                    health?.BeginRevivalProtection();
                    return true;
                }
                deathNotified = true;
                if (ownerStats != null) ownerStats.BeginHealthLifetime();
                else stats.BeginHealthLifetime();
                health?.NotifyResolvedDeath();
                player?.HandleDeath();
                return false;
            }
            finally { resolvingRawDeath = false; }
        }
    }
}
