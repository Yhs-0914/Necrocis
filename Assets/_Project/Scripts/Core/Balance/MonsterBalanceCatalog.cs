using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Balance/Monster Balance Catalog", fileName = "MonsterBalanceCatalog")]
    public sealed class MonsterBalanceCatalog : ScriptableObject
    {
        public DifficultyBalanceCatalog difficultyCatalog;
        public MonsterProgressionProfile progression;
        public List<BiomeEliteSpawnConfig> biomeEliteSpawns = new List<BiomeEliteSpawnConfig>();
        public List<MonsterDefinition> monsters = new List<MonsterDefinition>();

        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();
            if (difficultyCatalog == null) errors.Add("기존 DifficultyBalanceCatalog 연결이 필요합니다.");
            else
            {
                ValidateProfile(difficultyCatalog.normal, GameDifficulty.Normal, errors);
                ValidateProfile(difficultyCatalog.hard, GameDifficulty.Hard, errors);
            }

            string progressionError = progression != null ? progression.GetValidationError() : "몬스터 진행도 프로필이 없습니다.";
            if (progressionError != null) errors.Add(progressionError);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (monsters == null || monsters.Count == 0) errors.Add("몬스터 정의가 없습니다.");
            else foreach (MonsterDefinition definition in monsters)
            {
                if (definition == null) { errors.Add("비어 있는 몬스터 참조가 있습니다."); continue; }
                string error = definition.GetValidationError();
                if (error != null) errors.Add(definition.name + ": " + error);
                if (!ids.Add(definition.monsterId ?? string.Empty)) errors.Add("Monster ID 중복: " + definition.monsterId);
            }
            if (biomeEliteSpawns != null)
                foreach (BiomeEliteSpawnConfig spawn in biomeEliteSpawns)
                {
                    string error = spawn != null ? spawn.GetValidationError() : "비어 있는 바이옴 엘리트 배치 참조입니다.";
                    if (error != null) errors.Add(error);
                }
            return errors;
        }

        private static void ValidateProfile(DifficultyBalanceProfile profile, GameDifficulty expected, List<string> errors)
        {
            if (profile == null) { errors.Add(expected + " 프로필이 없습니다."); return; }
            if (profile.difficulty != expected) errors.Add(expected + " 슬롯에 다른 난이도 프로필이 연결되었습니다.");
            foreach (MonsterTier tier in Enum.GetValues(typeof(MonsterTier)))
            {
                string error = MonsterBalanceResolver.GetDifficultyValidationError(MonsterBalanceResolver.GetTierBalance(profile, tier));
                if (error != null) errors.Add(expected + " / " + tier + ": " + error);
            }
        }
    }
}
