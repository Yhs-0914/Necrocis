using System;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static class LungEliteMapSetup
    {
        public const string ConfigPath = "Assets/_Project/Data/BiomeElites/LungBiomeEliteSpawnConfig.asset";

        // Explicit P-MAP operation. Re-running preserves subsequent Inspector tuning.
        public static void RegisterApprovedSpecies()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            var config = AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>(ConfigPath);
            var pollen = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(PollenInvaderSetup.DefinitionPath);
            var dust = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(DustClumpSetup.DefinitionPath);
            if (config == null || pollen == null || dust == null) throw new InvalidOperationException("폐 엘리트 원본이 없습니다.");
            if (!((PollenInvaderPatternSettings)pollen.pattern).followup.enabled) throw new InvalidOperationException("Approved P-03 B must be enabled");
            foreach (var definition in new[] { pollen, dust })
                if (definition.GetValidationError() is string error) throw new InvalidOperationException(error);
            var proposed = UnityEngine.Object.Instantiate(config);
            try
            {
                if (proposed.monsters.Count == 0)
                {
                    proposed.minimumCount = proposed.maximumCount = 2;
                    proposed.minimumPerType = 1;
                    proposed.clearanceCells = 5;
                }
                if (!proposed.monsters.Any(r => r.monsterDefinition == pollen)) proposed.monsters.Add(PollenInvaderSetup.PreviewRule(pollen));
                if (!proposed.monsters.Any(r => r.monsterDefinition == dust)) proposed.monsters.Add(DustClumpSetup.PreviewRule(dust));
                if (proposed.GetValidationError() is string error) throw new InvalidOperationException(error);
                Undo.RecordObject(config, "Register approved Lung biome elites");
                EditorUtility.CopySerialized(proposed, config);
                config.name = "LungBiomeEliteSpawnConfig";
                EditorUtility.SetDirty(config); AssetDatabase.SaveAssetIfDirty(config); Selection.activeObject = config;
            }
            finally { UnityEngine.Object.DestroyImmediate(proposed); }
        }
    }
}
