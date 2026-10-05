using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class PollenInvaderSetup
    {
        public static object EnableFollowup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || GasSacSetup.DetectPipeline() != "URP") throw new InvalidOperationException("URP Edit Mode required");
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(DefinitionPath);
            var settings = definition != null ? definition.pattern as PollenInvaderPatternSettings : null;
            if (settings == null || settings.presentation == null) throw new InvalidOperationException("Existing P-03 A connection required");
            var art = settings.presentation; var directions = art.directionalPresentation;
            var sheet = JsonUtility.FromJson<Document>(File.ReadAllText(SourceFolder + "rotation-frames.json")).art.rotation;
            var sprites = Import(sheet, 9, SourceFolder);
            if (art.rotationFrames != null && art.rotationFrames.Length > 0 && !art.rotationFrames.SequenceEqual(sprites.Take(3)))
                throw new InvalidOperationException("Do not replace edited rotation art");
            Undo.RecordObjects(new UnityEngine.Object[] { definition, settings, art, directions }, "Connect approved P-03 B");
            art.rotationFrames = sprites.Take(3).ToArray(); var rows = directions.frames.ToList();
            for (int i = 0; i < 3; i++)
                if (!rows.Any(r => r.source == sprites[i])) rows.Add(new DirectionalSpriteFrame {
                    source = sprites[i], front = sprites[i + 3], back = sprites[i + 6], motion = EnemyPoseGroup.Preparation, label = "B 회전 예고 " + (i + 1) });
            directions.frames = rows.ToArray();
            if (!definition.patternDamage.Exists(p => p.id == PollenInvaderPatternSettings.FollowupDamageId))
                definition.patternDamage.Add(new MonsterPatternDamage { id = PollenInvaderPatternSettings.FollowupDamageId, coefficient = 1.5f });
            settings.followup.enabled = true;
            if (definition.designNote.StartsWith("P-03-A:"))
                definition.designNote = "P-03 A/B: original three-pellet fan or shell-turn follow-up. Shared one-hit budget per cycle; selected recovery/rearm once. P-MAP registration requires separate approval.";
            string error = definition.GetValidationError(); if (error != null) throw new InvalidOperationException(error);
            foreach (var asset in new UnityEngine.Object[] { definition, settings, art, directions }) { EditorUtility.SetDirty(asset); AssetDatabase.SaveAssetIfDirty(asset); }
            Selection.activeObject = settings;
            return new { rotationSprites = 9, totalSprites = 59, followupEnabled = settings.followup.enabled,
                rotationSeconds = settings.followup.rotationSeconds, angleOffset = settings.followup.angleOffset, sharedHitAttempts = 1, productionMapRegistration = false };
        }
    }
}
