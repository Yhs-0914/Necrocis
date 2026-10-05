using System;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static class GasSacOrbSetup
    {
        [MenuItem("Necrocis/Balance/Set Up I-01-B Gas Sac Orb")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(GasSacSetup.DefinitionPath);
            if (definition == null || !(definition.pattern is GasSacPatternSettings settings))
                throw new InvalidOperationException("I-01-A 원본과 표현을 먼저 준비하세요.");
            if (!definition.patternDamage.Exists(p => p.id == GasSacPatternSettings.OrbDamageId))
            {
                definition.patternDamage.Add(new MonsterPatternDamage { id = GasSacPatternSettings.OrbDamageId, coefficient = 1.25f });
                settings.orbEnabled = true; // First B migration only; reruns preserve the user's toggle and tuning.
                definition.designNote = "I-01 A/B: 근거리 단발 폭발, 원거리 고정 조준 가스탄 1발. 회복/대기는 선택한 패턴만 적용. I-MAP 등록 전 개별 검토 단계.";
            }
            string error = definition.GetValidationError();
            if (error != null) throw new InvalidOperationException(error);
            EditorUtility.SetDirty(definition); EditorUtility.SetDirty(settings); AssetDatabase.SaveAssets();
            Selection.activeObject = definition;
            Debug.Log("[GasSac-I01B] Setup PASS: gas-orb coefficient connected; existing A values/art and production spawn lists preserved.");
        }
    }
}
