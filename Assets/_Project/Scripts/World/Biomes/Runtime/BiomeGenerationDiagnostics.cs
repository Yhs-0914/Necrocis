using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public enum BiomeGenerationDiagnosticSeverity { Information, Warning }

    public readonly struct BiomeGenerationDiagnostic
    {
        public string Code { get; }
        public string FieldPath { get; }
        public string Message { get; }
        public BiomeGenerationDiagnosticSeverity Severity { get; }

        public BiomeGenerationDiagnostic(string code, string fieldPath, string message,
            BiomeGenerationDiagnosticSeverity severity = BiomeGenerationDiagnosticSeverity.Warning)
        { Code = code; FieldPath = fieldPath; Message = message; Severity = severity; }
    }

    // Describes the existing procedural backend. It never changes a placement rule or consumes random state.
    public static class BiomeGenerationDiagnostics
    {
        public static IReadOnlyList<BiomeGenerationDiagnostic> Collect(BiomeConfig config, bool areaBalanced,
            WorldDifficultyBalance world, DifficultyBalanceProfile difficulty = null)
        {
            var result = new List<BiomeGenerationDiagnostic>();
            if (config == null)
            {
                result.Add(new BiomeGenerationDiagnostic("missing-config", "config", "BiomeConfig가 연결되지 않았습니다."));
                return result;
            }
            result.Add(new BiomeGenerationDiagnostic("placement-mode", "useAreaBalancedEnemySpawning",
                areaBalanced
                    ? "면적 균형 배치: density × world.enemySpawnerDensity × cellArea / minDistance². minDistance는 후보 inset에도 사용하며 전역 Poisson 최소거리 보장은 아닙니다."
                    : "기존 청크 확률 배치: 청크·일반 종마다 Clamp01(density × 4). minDistance/region/margin/world density를 배치에 적용하지 않습니다.",
                BiomeGenerationDiagnosticSeverity.Information));
            result.Add(new BiomeGenerationDiagnostic("spawner-runtime-policy", "enemySpawnRules[].maxAlive/respawnCooldown/activationRadius",
                areaBalanced
                    ? "적 수·리스폰 대기는 world 배율을 사용합니다. 활성화 반경은 Bridge의 enemySpawnerActivationRadius override를 사용합니다."
                    : "적 수·리스폰 대기는 world 배율을 사용합니다. 활성화 반경은 각 rule.activationRadius를 사용합니다.",
                BiomeGenerationDiagnosticSeverity.Information));

            if (!areaBalanced && world != null && !Mathf.Approximately(world.enemySpawnerDensity, 1f))
                result.Add(new BiomeGenerationDiagnostic("ignored-world-density", "world.enemySpawnerDensity",
                    "현재 청크 확률 모드는 이 배치 밀도 배율을 사용하지 않습니다. 모드/수치는 자동 변경하지 않습니다."));
            if (!areaBalanced && (config.marginLeft != 0 || config.marginRight != 0 || config.marginBottom != 0 || config.marginTop != 0))
                result.Add(new BiomeGenerationDiagnostic("ignored-spawn-margin", "marginLeft/right/bottom/top",
                    "현재 청크 확률 모드의 일반 스포너 배치는 BiomeConfig margin을 사용하지 않습니다."));

            IReadOnlyList<EnemySpawnRuleConfig> rules = config.GetEnemySpawnRules();
            for (int i = 0; i < rules.Count; i++)
            {
                EnemySpawnRuleConfig rule = rules[i];
                if (rule == null || rule.isElite) continue;
                if (rule.allowedRegions != null && rule.allowedRegions.Count > 0)
                    result.Add(new BiomeGenerationDiagnostic("ignored-spawn-regions", "enemySpawnRules[" + i + "].allowedRegions",
                        rule.name + ": ProceduralBiomeBridge의 두 배치 모드 모두 이 region 제한을 사용하지 않습니다."));
            }
            if (difficulty != null && difficulty.biomeOverrides != null)
            {
                for (int i = 0; i < difficulty.biomeOverrides.Count; i++)
                {
                    DifficultyBiomeOverride entry = difficulty.biomeOverrides[i];
                    if (entry != null && entry.biome == config.biomeType && entry.config != null)
                        result.Add(new BiomeGenerationDiagnostic("ignored-full-biome-override", "biomeOverrides[" + i + "]",
                            "ProceduralBiomeBridge는 full BiomeConfig override를 해석하지 않습니다. 현재 scene의 config를 계속 사용합니다."));
                }
            }
            if (config.objectRules != null && config.objectRules.Count > 0)
                result.Add(new BiomeGenerationDiagnostic("legacy-object-rules", "objectRules",
                    "Bridge의 장식/지형 생성은 MapGenerator가 담당합니다. 이 legacy objectRules는 Bridge의 장식 배치 입력이 아닙니다."));
            if (config.exteriorBackdrop != null && config.exteriorBackdrop.IsUsable)
                result.Add(new BiomeGenerationDiagnostic("legacy-exterior-backdrop", "exteriorBackdrop",
                    "이 배경은 ConfigurableBiomeManager용입니다. ProceduralBiomeBridge는 생성하지 않습니다."));
            if ((config.regions != null && config.regions.Count > 0) || (config.tileMappings != null && config.tileMappings.Count > 0))
                result.Add(new BiomeGenerationDiagnostic("terrain-owner", "regions/tileMappings/heightNoise",
                    "절차 지형의 권위는 MapGenerator입니다. 이 BiomeConfig의 legacy 지형 필드를 편집해도 Bridge의 지형 생성 방식은 바뀌지 않습니다.",
                    BiomeGenerationDiagnosticSeverity.Information));
            return result;
        }
    }
}
