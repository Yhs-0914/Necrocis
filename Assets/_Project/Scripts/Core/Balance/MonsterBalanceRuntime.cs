using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    // One capture per spawn. Phase changes never read mutable assets or reapply multipliers.
    public sealed class MonsterBalanceRuntime
    {
        public const string CatalogResourcePath = "Balance/MonsterBalanceCatalog";
        private readonly Dictionary<string, MonsterBalanceSnapshot> phases = new Dictionary<string, MonsterBalanceSnapshot>();
        private readonly Dictionary<string, float> coefficients = new Dictionary<string, float>();
        public MonsterBalanceSnapshot Current { get; private set; }
        public bool ContactEnabled { get; }
        public float ContactCoefficient { get; }
        public float ContactKnockback { get; }

        public MonsterBalanceRuntime(MonsterDefinition definition, DifficultyBalanceProfile difficulty,
            MonsterProgressionProfile progression, int stage)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            string validation = definition.GetValidationError();
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

        public static MonsterBalanceRuntime Capture(MonsterDefinition definition)
        {
            var catalog = Resources.Load<MonsterBalanceCatalog>(CatalogResourcePath);
            if (catalog == null) throw new InvalidOperationException("런타임 MonsterBalanceCatalog가 없습니다.");
            MonsterVisitContext visit = BiomeManager.Active != null ? BiomeManager.Active.MonsterVisit : default;
            bool inVisit = visit.Biome != BiomeType.None && visit.RunId == SaveService.ActiveRunId;
            return new MonsterBalanceRuntime(definition,
                catalog.difficultyCatalog.Get(inVisit ? visit.Difficulty : DifficultyBalanceService.ActiveDifficulty),
                catalog.progression, inVisit ? visit.Stage : 0);
        }

        public void SelectPhase(string id)
        {
            if (!phases.TryGetValue(id, out MonsterBalanceSnapshot snapshot))
                throw new InvalidOperationException("등록되지 않은 페이즈: " + id);
            Current = snapshot;
        }

        public float GetCoefficient(string id)
        {
            if (!coefficients.TryGetValue(id, out float value))
                throw new InvalidOperationException(Current.MonsterId + ": 등록되지 않은 패턴 피해 " + id);
            return value;
        }
    }

    // Final outgoing damage, including difficulty/progression. Safe to keep in a projectile/DoT.
    public readonly struct EnemyDamageRequest
    {
        public float Amount { get; }
        private readonly EnemyController source;
        private readonly uint generation;
        public EnemyController Source => source != null && source.SpawnGeneration == generation
            && source.gameObject.activeInHierarchy ? source : null;

        public EnemyDamageRequest(float amount, EnemyController source)
        {
            if (!MonsterBalanceNumbers.NonNegative(amount)) throw new ArgumentOutOfRangeException(nameof(amount));
            Amount = amount;
            this.source = source;
            generation = source != null ? source.SpawnGeneration : 0;
        }
    }
}
