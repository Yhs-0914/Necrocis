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

        public void ApplyBalancePhase(string id, bool restoreHealth = false)
        {
            if (Balance == null) return;
            Balance.SelectPhase(id);
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
            Balance != null ? amount : amount * DifficultyBalanceService.GetIncomingDamageMultiplier(this), this);

        public EnemyDamageRequest CreatePatternDamage(string id, float legacyDamage)
        {
            float amount = Balance != null
                ? stats.AttackPower * Balance.GetCoefficient(id)
                : legacyDamage * DifficultyBalanceService.GetIncomingDamageMultiplier(this);
            return new EnemyDamageRequest(amount, this);
        }

        public EnemyDamageRequest CreateContactDamage(float legacyDamage) => Balance != null
            ? new EnemyDamageRequest(Balance.ContactEnabled ? stats.AttackPower * Balance.ContactCoefficient : 0f, this)
            : CreateAttackDamage(legacyDamage);
    }
}
