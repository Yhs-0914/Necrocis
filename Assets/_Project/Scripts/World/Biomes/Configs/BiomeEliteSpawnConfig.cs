using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome/Biome Elite Spawn Config", fileName = "BiomeEliteSpawnConfig")]
    public sealed class BiomeEliteSpawnConfig : ScriptableObject
    {
        [Tooltip("맵 생성 시 소량 배치합니다. 일반몹 처치 수와 무관합니다. 목록은 구현·검토를 마친 바이옴 엘리트만 등록합니다.")]
        public bool enabled = true;
        public List<EnemySpawnRuleConfig> monsters = new List<EnemySpawnRuleConfig>();
        [Tooltip("청크당 수가 아닌 맵 전체 수입니다. 저장된 배치는 유지되며 변경은 새 런의 최초 배치부터 적용합니다.")]
        [Min(0)] public int minimumCount = 2;
        [Tooltip("청크당 수가 아닌 맵 전체 수입니다. 저장된 배치는 유지되며 변경은 새 런의 최초 배치부터 적용합니다.")]
        [Min(0)] public int maximumCount = 2;
        [Min(0)] public int minimumPerType = 1;
        [Tooltip("엘리트 배치 지점 사이의 최소 거리(칸)입니다. 유효 위치가 부족해도 이 간격은 줄이지 않습니다.")]
        [Min(1f)] public float minimumSpacing = 64f;
        [Min(0f)] public float entranceExclusionRadius = 18f;
        [Min(0f)] public float portalExclusionRadius = 10f;
        [Min(0f)] public float bossExclusionPadding = 8f;
        [Min(0)] public int clearanceCells = 1;
        [Min(1f)] public float activationDistance = 48f;
        [Min(1f)] public float releaseDistance = 72f;
        [Min(1f)] public float leashDistance = 16f;

        public string GetValidationError()
        {
            if (minimumCount < 0 || maximumCount < minimumCount || minimumPerType < 0 || clearanceCells < 0)
                return "배치 수/간격 설정을 확인하세요.";
            if (!MonsterBalanceNumbers.Positive(minimumSpacing) || !MonsterBalanceNumbers.Positive(activationDistance)
                || !MonsterBalanceNumbers.Positive(releaseDistance) || releaseDistance <= activationDistance
                || !MonsterBalanceNumbers.NonNegative(entranceExclusionRadius) || !MonsterBalanceNumbers.NonNegative(portalExclusionRadius) || !MonsterBalanceNumbers.NonNegative(bossExclusionPadding)
                || !MonsterBalanceNumbers.Positive(leashDistance)) return "배치 거리와 활성화/해제 거리를 확인하세요.";
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (monsters == null) return "바이옴 엘리트 목록이 없습니다.";
            foreach (EnemySpawnRuleConfig rule in monsters)
            {
                if (rule?.monsterDefinition == null || rule.monsterDefinition.tier != MonsterTier.Elite)
                    return "바이옴 엘리트에는 Elite 정의가 필요합니다.";
                string error = rule.monsterDefinition.GetValidationError();
                if (error != null) return error;
                if (!ids.Add(rule.monsterDefinition.monsterId)) return "중복된 바이옴 엘리트 정의입니다.";
            }
            if ((long)minimumPerType * monsters.Count > maximumCount) return "종류별 최소 배정이 전체 최대 수보다 큽니다.";
            return null;
        }
    }
}
