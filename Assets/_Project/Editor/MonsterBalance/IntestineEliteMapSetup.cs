using System;
using System.Linq;
using Necrocis;
using UnityEditor;

namespace NecrocisEditor
{
    public static class IntestineEliteMapSetup
    {
        public const string ConfigPath = "Assets/_Project/Data/BiomeElites/IntestineBiomeEliteSpawnConfig.asset";

        // Explicitly invoked after I-MAP approval; never runs on import or resets tuning on reload.
        public static void RegisterApprovedSpecies()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            var config = AssetDatabase.LoadAssetAtPath<BiomeEliteSpawnConfig>(ConfigPath);
            var gas = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(GasSacSetup.DefinitionPath);
            var residue = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(HardenedResidueSetup.DefinitionPath);
            if (config == null || gas == null || residue == null) throw new InvalidOperationException("장 엘리트 원본 연결이 없습니다.");
            foreach (var definition in new[] { gas, residue })
                if (definition.GetValidationError() is string error) throw new InvalidOperationException(error);
            Undo.RecordObject(config, "Register approved Intestine biome elites");
            if (!config.monsters.Any(r => r.monsterDefinition == gas)) config.monsters.Add(GasSacSetup.PreviewRule(gas));
            if (!config.monsters.Any(r => r.monsterDefinition == residue)) config.monsters.Add(HardenedResidueSetup.PreviewRule(residue));
            if (config.GetValidationError() is string configError) throw new InvalidOperationException(configError);
            EditorUtility.SetDirty(config); AssetDatabase.SaveAssets(); Selection.activeObject = config;
        }
    }
}
