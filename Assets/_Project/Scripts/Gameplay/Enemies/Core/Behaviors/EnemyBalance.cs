using UnityEngine;

namespace Necrocis
{
    public partial class EnemyController
    {
        public MonsterBalanceRuntime Balance { get; private set; }
        public uint SpawnGeneration { get; private set; }
        private bool experienceGranted;
        public bool PatternOwnsContact { get; set; }
        public bool SuppressExperienceReward { get; set; }
        private readonly object playerLevelHealthSource = new object();
        private int healthScalingPlayerLevel = -1;
        private float playerLevelHealthMultiplier = 1f;

        private void InitializePlayerLevelHealthScaling()
        {
            healthScalingPlayerLevel = -1;
            playerLevelHealthMultiplier = 1f;
            RefreshPlayerLevelHealth();
        }

        // Applies to enemies already alive as well as new/pool-reused spawns.
        // Preserve the injured fraction so levelling up never fully heals an enemy.
        private void RefreshPlayerLevelHealth()
        {
            if (stats == null || IsDead) return;
            int level = LevelUpManager.GetCurrentLevel();
            if (healthScalingPlayerLevel == level) return;
            healthScalingPlayerLevel = level;
            MonsterTier tier = IsBossEncounter ? MonsterTier.Boss
                : Balance != null ? Balance.Current.Tier : IsElite ? MonsterTier.Elite : MonsterTier.Normal;
            float multiplier = LevelUpManager.Config.GetEnemyHealthMultiplier(level, tier);
            if (Mathf.Approximately(multiplier, playerLevelHealthMultiplier)) return;
            float fraction = stats.MaxHealth > 0f ? stats.CurrentHealth / stats.MaxHealth : 1f;
            stats.RemoveModifiersFromSource(playerLevelHealthSource);
            if (multiplier > 1f)
                stats.AddModifier(CharacterStatType.MaxHealth, multiplier - 1f,
                    CharacterStatModifierMode.PercentMultiply, playerLevelHealthSource);
            playerLevelHealthMultiplier = multiplier;
            stats.SetCurrentHealth(stats.MaxHealth * fraction);
        }

        public void ApplyBalancePhase(string id, bool restoreHealth = false)
        {
            if (Balance == null) return;
            string previousPhase = Balance.Current.StatSetId;
            Balance.SelectPhase(id);
            if (IsBossEncounter && previousPhase != id) SaveService.BalanceMetrics?.Record("boss_phase", Balance.Current.MonsterId + ":" + id);
            MonsterBalanceSnapshot values = Balance.Current;
            stats.SetBaseStat(CharacterStatType.MaxHealth, Mathf.Max(1f, values.MaxHealth), restoreHealth);
            stats.SetBaseStat(CharacterStatType.AttackPower, values.AttackPower);
            stats.SetBaseStat(CharacterStatType.MoveSpeed, values.MoveSpeed);
            ConfigureContactBalance();
        }

        private void ConfigureContactBalance()
        {
            contactDamage.Configure(this,
                Balance != null ? Balance.ContactEnabled : config.enableContactDamage,
                Balance != null ? Balance.Current.ContactDamage : config.contactDamage,
                Balance != null ? Balance.ContactKnockback : config.contactKnockbackDistance);
        }

        public float GetRearmCooldown(float seconds) => Balance != null
            ? Balance.Current.GetRearmCooldown(seconds)
            : seconds * Mathf.Max(0.01f, DifficultyBalanceService.GetEnemyBalance(IsBossEncounter).attackCooldown);

        public EnemyDamageRequest CreateAttackDamage(float amount) => new EnemyDamageRequest(
            Balance != null ? amount : ResolveLegacyDamage(amount), this);

        private float ResolveLegacyDamage(float amount)
        {
            float fixedDamage = IsElite && !IsBossEncounter
                ? DifficultyBalanceService.ActiveProfile?.elites?.fixedHitDamage ?? 0f : 0f;
            return fixedDamage > 0f && amount > 0f ? fixedDamage
                : amount * DifficultyBalanceService.GetIncomingDamageMultiplier(this);
        }

        public EnemyDamageRequest CreatePatternDamage(string id, float legacyDamage)
        {
            float amount = Balance != null
                ? stats.AttackPower * Balance.GetCoefficient(id)
                : ResolveLegacyDamage(legacyDamage);
            return new EnemyDamageRequest(amount, this);
        }

        public EnemyDamageRequest CreateContactDamage(float legacyDamage) => Balance != null
            ? new EnemyDamageRequest(Balance.ContactEnabled ? stats.AttackPower * Balance.ContactCoefficient : 0f, this)
            : CreateAttackDamage(legacyDamage);
    }
}
