using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    // World/session interpretation belongs here; the Core constructor only calculates numbers.
    public static class EnemyBalanceFactory
    {
        public static MonsterBalanceRuntime Capture(MonsterDefinition definition, MonsterBalanceCatalog catalog,
            MonsterVisitContext visit, string activeRunId, GameDifficulty fallbackDifficulty)
        {
            if (catalog == null) throw new InvalidOperationException("런타임 MonsterBalanceCatalog가 없습니다.");
            string validation = definition != null ? definition.GetValidationError() : "몬스터 정의가 없습니다.";
            if (validation != null) throw new InvalidOperationException(validation);
            bool inVisit = visit.Biome != BiomeType.None && visit.RunId == activeRunId;
            return new MonsterBalanceRuntime(definition,
                catalog.difficultyCatalog.Get(inVisit ? visit.Difficulty : fallbackDifficulty),
                catalog.progression, inVisit ? visit.Stage : 0);
        }
    }

    public sealed class BiomeEnemyRuleCatalog : IEnemyRuleCatalog
    {
        private readonly IReadOnlyList<EnemySpawnRuleConfig> rules;
        public BiomeEnemyRuleCatalog(IReadOnlyList<EnemySpawnRuleConfig> rules)
            { this.rules = rules ?? Array.Empty<EnemySpawnRuleConfig>(); }
        public bool TryFindByName(string name, out EnemySpawnRuleConfig rule)
        {
            for (int i = 0; i < rules.Count; i++)
                if (rules[i] != null && string.Equals(rules[i].name, name, StringComparison.Ordinal))
                { rule = rules[i]; return true; }
            rule = null;
            return false;
        }
    }

    public static class EnemySpawnContextFactory
    {
        public static EnemySpawnerIdentity CaptureSpawnerIdentity(EnemySpawnRuleConfig rule, Vector3 anchor)
        {
            var biome=BiomeManager.Active;
            var cell=biome!=null?biome.WorldToGrid(anchor):new Vector2Int(Mathf.RoundToInt(anchor.x*1024f),Mathf.RoundToInt(anchor.z*1024f));
            return new EnemySpawnerIdentity(biome!=null?biome.Seed:0,biome!=null?biome.BiomeType:BiomeType.None,
                cell,rule?.monsterDefinition?.monsterId??rule?.name,rule?.poissonSalt??0);
        }

        public static EnemySpawnContext CaptureCurrent(EnemySpawnRuleConfig rule, Vector3 position,
            MonsterBalanceRuntime explicitBalance = null, EnemyEncounterRole role = EnemyEncounterRole.Field,
            bool ignoreEncounterMovementRestrictions = false)
            => WithBalanceForBiome(BiomeManager.Active, rule, position, explicitBalance, role, ignoreEncounterMovementRestrictions);

        public static EnemySpawnContext CaptureForBiome(BiomeManager biome, EnemySpawnRuleConfig rule, Vector3 position,
            EnemyEncounterRole role = EnemyEncounterRole.Field, bool ignoreEncounterMovementRestrictions = false)
            => WithBalanceForBiome(biome, rule, position, null, role, ignoreEncounterMovementRestrictions);

        public static EnemySpawnContext WithBalanceForBiome(BiomeManager biome, EnemySpawnRuleConfig rule, Vector3 position,
            MonsterBalanceRuntime explicitBalance, EnemyEncounterRole role = EnemyEncounterRole.Field,
            bool ignoreEncounterMovementRestrictions = false)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            MonsterVisitContext visit = biome != null ? biome.MonsterVisit : default;
            MonsterBalanceRuntime balance = explicitBalance;
            if (balance == null && rule.monsterDefinition != null)
                balance = EnemyBalanceFactory.Capture(rule.monsterDefinition,
                    Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath), visit,
                    WorldProgressBindings.Session.ActiveRunId, DifficultyBalanceService.ActiveDifficulty);
            IEnemyRuleCatalog rules = ResolveRules(biome);
            EnemySpawnScope scope = default;
            if (biome != null)
            {
                Vector2Int cell = biome.WorldToGrid(position);
                scope = new EnemySpawnScope(visit.RunId, visit.VisitId, biome.gameObject.scene.handle,
                    biome.GetInstanceID(), biome.GridToChunk(cell.x, cell.y));
            }
            return new EnemySpawnContext(balance, BiomeWorldQueries.ForBiome(biome), role,
                ignoreEncounterMovementRestrictions, rules,
                biome != null ? DeathSpawnCoordinator.ForBiome(biome) : null, scope);
        }

        private static IEnemyRuleCatalog ResolveRules(BiomeManager biome)
        {
            if (biome is IEnemyRuleCatalogProvider provider) return provider.GetEnemyRuleCatalog();
            // Legacy adapter remains World-owned; enemies never cast to a concrete biome manager.
            if (biome is ConfigurableBiomeManager legacy)
                return new BiomeEnemyRuleCatalog(legacy.GetBiomeConfig()?.GetEnemySpawnRules());
            return new BiomeEnemyRuleCatalog(null);
        }
    }
}
