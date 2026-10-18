using System;
using System.Collections.Generic;

namespace Necrocis
{
    // One capture per spawn. Phase changes never read mutable assets or reapply multipliers.
    public sealed class MonsterBalanceRuntime
    {
        public const string CatalogResourcePath = "Balance/MonsterBalanceCatalog";
        private readonly Dictionary<string, MonsterBalanceSnapshot> phases = new Dictionary<string, MonsterBalanceSnapshot>();
        private readonly Dictionary<string, float> coefficients = new Dictionary<string, float>();
        private bool encounterMultipliersApplied;
        public MonsterBalanceSnapshot Current { get; private set; }
        public bool ContactEnabled { get; }
        public float ContactCoefficient { get; }
        public float ContactKnockback { get; }

        public MonsterBalanceRuntime(MonsterDefinition definition, DifficultyBalanceProfile difficulty,
            MonsterProgressionProfile progression, int stage)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            string validation = definition.GetBalanceValidationError();
            if (validation != null) throw new InvalidOperationException(validation);
            foreach (MonsterStatSet set in definition.statSets)
            {
                if (!MonsterBalanceResolver.TryResolve(definition, set?.id, difficulty, progression, stage,
                    out MonsterBalanceSnapshot snapshot, out string error))
                    throw new InvalidOperationException(definition.name + ": " + error);
                phases.Add(set.id, snapshot);
            }
            foreach (MonsterPatternDamage pattern in definition.patternDamage)
                coefficients.Add(pattern.id, pattern.coefficient);
            ContactEnabled = definition.contact.enabled;
            ContactCoefficient = definition.contact.damageCoefficient;
            ContactKnockback = definition.contact.knockbackDistance;
            SelectPhase("Default");
        }

        public void SelectPhase(string id)
        {
            if (!phases.TryGetValue(id, out MonsterBalanceSnapshot snapshot))
                throw new InvalidOperationException("등록되지 않은 페이즈: " + id);
            Current = snapshot;
        }

        internal void ApplyEncounterMultipliers(float health, float damage, float speed, float experience, float cooldown)
        {
            if (encounterMultipliersApplied) throw new InvalidOperationException("전투별 배율은 생성 시 한 번만 적용합니다.");
            foreach (string id in new List<string>(phases.Keys))
                phases[id] = phases[id].ForEncounter(health, damage, speed, experience, cooldown);
            SelectPhase(Current.StatSetId);
            encounterMultipliersApplied = true;
        }

        public float GetCoefficient(string id)
        {
            if (!coefficients.TryGetValue(id, out float value))
                throw new InvalidOperationException(Current.MonsterId + ": 등록되지 않은 패턴 피해 " + id);
            return value;
        }
    }

}
