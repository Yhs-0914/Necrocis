using System;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static class StomachEliteMapSetup
    {
        public const string ConfigPath = "Assets/_Project/Data/BiomeElites/StomachBiomeEliteSpawnConfig.asset";

        // Explicit S-MAP operation. Re-running preserves subsequent Inspector tuning.
        public static void RegisterApprovedSpecies()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            var config = AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>(ConfigPath);
            var helico = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(HelicoSpiralSetup.DefinitionPath);
            var oil = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(OilFilmSetup.DefinitionPath);
            if (config == null || helico == null || oil == null) throw new InvalidOperationException("위 엘리트 원본이 없습니다.");
            if (((HelicoSpiralPatternSettings)helico.pattern).tailSweepEnabled) throw new InvalidOperationException("Retired S-01 tail must remain disabled");
            foreach (var definition in new[] { helico, oil })
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
                if (!proposed.monsters.Any(r => r.monsterDefinition == helico)) proposed.monsters.Add(HelicoSpiralSetup.PreviewRule(helico));
                if (!proposed.monsters.Any(r => r.monsterDefinition == oil)) proposed.monsters.Add(OilFilmSetup.PreviewRule(oil));
                if (proposed.GetValidationError() is string error) throw new InvalidOperationException(error);
                Undo.RecordObject(config, "Register approved Stomach biome elites");
                EditorUtility.CopySerialized(proposed, config);
                config.name = "StomachBiomeEliteSpawnConfig";
                EditorUtility.SetDirty(config); AssetDatabase.SaveAssetIfDirty(config); Selection.activeObject = config;
            }
            finally { UnityEngine.Object.DestroyImmediate(proposed); }
        }
    }
}
