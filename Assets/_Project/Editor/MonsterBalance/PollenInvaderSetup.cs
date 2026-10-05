using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Necrocis;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class PollenInvaderSetup
    {
        public const string DefinitionPath = "Assets/_Project/Data/MonsterBalance/Definitions/P-03.asset";
        public const string PatternPath = "Assets/_Project/Data/MonsterBalance/Patterns/PollenInvader_A.asset";
        public const string PresentationPath = "Assets/_Project/Data/MonsterBalance/Presentations/PollenInvader_Presentation.asset";
        public const string DirectionPath = "Assets/_Project/Data/MonsterBalance/Presentations/PollenInvader_Directions.asset";
        public const string ArtFolder = "Assets/_Project/Art/Generated/BiomeElites/PollenInvader";
        public const string SourceFolder = "output/art/PollenInvader/";
        [Serializable] private sealed class Frame { public int[] rect, anchor; public string label; }
        [Serializable] private sealed class Sheet { public string file, sha256; public int width, height, ppu; public Frame[] frames; }
        [Serializable] private sealed class Art { public Sheet sideMain, frontMain, backMain, sideDeath, frontDeath, backDeath, pellet, rotation; }
        [Serializable] private sealed class Document { public Art art; }
        public static object Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || GasSacSetup.DetectPipeline() != "URP") throw new InvalidOperationException("URP Edit Mode required");
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(DefinitionPath);
            if (definition == null) throw new InvalidOperationException("Existing P-03 definition required");
            var approved = JsonUtility.FromJson<Document>(File.ReadAllText(SourceFolder + "frames.json")).art;
            var projectile = JsonUtility.FromJson<Document>(File.ReadAllText(SourceFolder + "projectile-frames.json")).art;
            if (!AssetDatabase.IsValidFolder(ArtFolder)) AssetDatabase.CreateFolder("Assets/_Project/Art/Generated/BiomeElites", "PollenInvader");
            var main = Import(approved.sideMain, 10, SourceFolder); var front = Import(approved.frontMain, 10, SourceFolder); var back = Import(approved.backMain, 10, SourceFolder);
            var death = Import(approved.sideDeath, 6, SourceFolder); var frontDeath = Import(approved.frontDeath, 6, SourceFolder); var backDeath = Import(approved.backDeath, 6, SourceFolder);
            var pellet = Import(projectile.pellet, 2, SourceFolder);
            var directions = AssetDatabase.LoadAssetAtPath<EnemyDirectionalPresentation>(DirectionPath);
            if (directions == null)
            {
                directions = ScriptableObject.CreateInstance<EnemyDirectionalPresentation>(); directions.mode = EnemyDirectionMode.Four; directions.authoredFacingLeft = false;
                directions.sideOrigin = new Vector2(.52f, .88f); directions.frontOrigin = new Vector2(0, .85f); directions.backOrigin = new Vector2(0, 1.42f);
                var rows = new List<DirectionalSpriteFrame>();
                for (int i = 0; i < 10; i++) rows.Add(new DirectionalSpriteFrame { source = main[i], front = front[i], back = back[i], label = approved.sideMain.frames[i].label,
                    motion = i < 2 ? EnemyPoseGroup.Idle : i < 4 ? EnemyPoseGroup.Move : i < 7 ? EnemyPoseGroup.Preparation : i == 7 ? EnemyPoseGroup.Release : i == 8 ? EnemyPoseGroup.Recovery : EnemyPoseGroup.Hit });
                for (int i = 0; i < 6; i++) rows.Add(new DirectionalSpriteFrame { source = death[i], front = frontDeath[i], back = backDeath[i], label = "Death " + i, motion = EnemyPoseGroup.Death });
                directions.frames = rows.ToArray(); AssetDatabase.CreateAsset(directions, DirectionPath);
            }
            var art = AssetDatabase.LoadAssetAtPath<PollenInvaderPresentation>(PresentationPath);
            if (art == null)
            {
                art = ScriptableObject.CreateInstance<PollenInvaderPresentation>(); art.directionalPresentation = directions;
                art.idleFrames = main.Take(2).ToArray(); art.moveFrames = main.Skip(2).Take(2).ToArray(); art.preparationFrames = main.Skip(4).Take(3).ToArray();
                art.release = main[7]; art.recovery = main[8]; art.hit = main[9]; art.deathFrames = death; art.pelletFrames = pellet;
                art.groundDisc = AssetDatabase.LoadAssetAtPath<GasSacPresentation>(GasSacSetup.PresentationPath).filledCircle;
                AssetDatabase.CreateAsset(art, PresentationPath);
            }
            var settings = AssetDatabase.LoadAssetAtPath<PollenInvaderPatternSettings>(PatternPath);
            if (settings == null) { settings = ScriptableObject.CreateInstance<PollenInvaderPatternSettings>(); settings.presentation = art; AssetDatabase.CreateAsset(settings, PatternPath); }
            if (definition.pattern != null && definition.pattern != settings) throw new InvalidOperationException("Do not overwrite another P-03 pattern");
            Undo.RecordObject(definition, "Connect approved P-03 A pattern");
            if (definition.pattern == null) { definition.pattern = settings; definition.designNote = "P-03-A: committed three-pellet fan, one shared hit attempt, four body/death directions, ground-skimming shots. P-MAP requires separate approval."; }
            if (!definition.patternDamage.Exists(x => x.id == PollenInvaderPatternSettings.DamageId)) definition.patternDamage.Add(new MonsterPatternDamage { id = PollenInvaderPatternSettings.DamageId, coefficient = 1.5f });
            string error = definition.GetValidationError(); if (error != null) throw new InvalidOperationException(error);
            EditorUtility.SetDirty(definition); AssetDatabase.SaveAssetIfDirty(definition); Selection.activeObject = definition;
            return new { sprites = 50, directions = 4, productionMapRegistration = false, baseStatsPreserved = true };
        }
        public static EnemySpawnRuleConfig PreviewRule(MonsterDefinition definition) => new EnemySpawnRuleConfig
        {
            name = "P-03 Pollen Invader", monsterDefinition = definition, isElite = true,
            chaseRadius = 8, wanderRadius = 0, leashRadius = 16, stoppingDistance = .2f,
            addCollider = true, isTrigger = true, colliderSize = new Vector3(1.2f, 1.5f, 1.1f), colliderCenter = new Vector3(0, .75f, 0),
            scale = Vector3.one, useBillboard = true, useYSort = true
        };

        private static Sprite[] Import(Sheet sheet, int count, string sourceFolder)
        {
            if (sheet == null || sheet.frames == null || sheet.frames.Length != count || sheet.ppu <= 0)
                throw new InvalidOperationException("승인 메타데이터를 확인하세요.");
            byte[] png = File.ReadAllBytes(sourceFolder + sheet.file);
            using (var sha = SHA256.Create())
                if (!string.Equals(BitConverter.ToString(sha.ComputeHash(png)).Replace("-", "").ToLowerInvariant(), sheet.sha256, StringComparison.Ordinal))
                    throw new InvalidOperationException("승인된 PNG 해시와 다릅니다: " + sheet.file);
            string path = ArtFolder + "/" + sheet.file;
            if (File.Exists(path))
            {
                if (!png.SequenceEqual(File.ReadAllBytes(path))) throw new InvalidOperationException("다른 기존 미술을 덮어쓰지 않습니다: " + path);
            }
            else File.Copy(sourceFolder + sheet.file, path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true; importer.npotScale = TextureImporterNPOTScale.None; importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 4096; importer.spritePixelsPerUnit = sheet.ppu;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture.width != sheet.width || texture.height != sheet.height) throw new InvalidOperationException("PNG 크기가 변경됐습니다.");
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null) throw new InvalidOperationException("Sprite data provider가 없습니다.");
            provider.InitSpriteEditorDataProvider();
            var edit = provider.GetDataProvider<ISpriteFrameEditCapability>();
            if (edit == null) throw new InvalidOperationException("Sprite 편집 capability가 없습니다.");
            var capability = edit.GetEditCapability();
            foreach (EEditCapability required in new[] { EEditCapability.CreateAndDeleteSprite, EEditCapability.EditSpriteRect,
                EEditCapability.EditPivot, EEditCapability.EditSpriteName, EEditCapability.EditBorder })
                if (!capability.HasCapability(required)) throw new InvalidOperationException("지원하지 않는 Sprite 작업: " + required);
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (names == null) throw new InvalidOperationException("name/file-ID provider가 없습니다.");
            var previous = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
            string prefix = Path.GetFileNameWithoutExtension(sheet.file);
            var rects = new SpriteRect[count];
            for (int i = 0; i < count; i++)
            {
                var frame = sheet.frames[i]; int[] b = (int[])frame.rect.Clone(), a = frame.anchor;
                // Keep the integer ground/center pivot inside the transparent rectangle.
                b[0] = Mathf.Min(b[0], a[0]); b[1] = Mathf.Min(b[1], a[1]);
                b[2] = Mathf.Max(b[2], a[0] + 1); b[3] = Mathf.Max(b[3], a[1] + 1);
                Rect rect = Rect.MinMaxRect(b[0], sheet.height - b[3], b[2], sheet.height - b[1]);
                if (rect.xMin < 0 || rect.yMin < 0 || rect.xMax > sheet.width || rect.yMax > sheet.height || rect.width <= 0 || rect.height <= 0)
                    throw new InvalidOperationException("잘못된 Sprite 영역: " + i);
                string name = prefix + "_" + i.ToString("D2");
                rects[i] = new SpriteRect { name = name, rect = rect,
                    pivot = new Vector2((a[0] - b[0]) / rect.width, (b[3] - a[1]) / rect.height),
                    alignment = SpriteAlignment.Custom, border = Vector4.zero,
                    spriteID = previous.TryGetValue(name, out GUID id) ? id : GUID.Generate() };
            }
            provider.SetSpriteRects(rects); names.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            if (sprites.Length != count) throw new InvalidOperationException("분할된 Sprite 수가 다릅니다.");
            return sprites;
        }
    }
}
