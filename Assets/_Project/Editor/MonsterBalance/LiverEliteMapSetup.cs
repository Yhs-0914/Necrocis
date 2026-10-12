using System;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static class LiverEliteMapSetup
    {
        public const string ConfigPath = "Assets/_Project/Data/BiomeElites/LiverBiomeEliteSpawnConfig.asset";

        // Explicit H-MAP operation. Re-running preserves subsequent Inspector tuning.
        public static void RegisterApprovedSpecies()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            var config = AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>(ConfigPath);
            var ember = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(InflammationEmberSetup.DefinitionPath);
            var hangover = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(HangoverRemnantSetup.DefinitionPath);
            if (config == null || ember == null || hangover == null) throw new InvalidOperationException("간 엘리트 원본이 없습니다.");
            foreach (var definition in new[] { ember, hangover })
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
                if (!proposed.monsters.Any(r => r.monsterDefinition == ember)) proposed.monsters.Add(InflammationEmberSetup.PreviewRule(ember));
                if (!proposed.monsters.Any(r => r.monsterDefinition == hangover)) proposed.monsters.Add(HangoverRemnantSetup.PreviewRule(hangover));
                if (proposed.GetValidationError() is string error) throw new InvalidOperationException(error);
                Undo.RecordObject(config, "Register approved Liver biome elites");
                EditorUtility.CopySerialized(proposed, config);
                config.name = "LiverBiomeEliteSpawnConfig";
                EditorUtility.SetDirty(config); AssetDatabase.SaveAssetIfDirty(config); Selection.activeObject = config;
            }
            finally { UnityEngine.Object.DestroyImmediate(proposed); }
        }
    }
}
