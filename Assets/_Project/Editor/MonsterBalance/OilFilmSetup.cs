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
    public static class OilFilmSetup
    {
        public const string DefinitionPath = "Assets/_Project/Data/MonsterBalance/Definitions/S-03.asset";
        public const string PatternPath = "Assets/_Project/Data/MonsterBalance/Patterns/OilFilm_A.asset";
        public const string PresentationPath = "Assets/_Project/Data/MonsterBalance/Presentations/OilFilm_Presentation.asset";
        public const string DirectionPath = "Assets/_Project/Data/MonsterBalance/Presentations/OilFilm_Directions.asset";
        public const string ArtFolder = "Assets/_Project/Art/Generated/BiomeElites/OilFilmGlider";
        public const string SourceFolder = "output/art/OilFilmGlider/";
        [Serializable] private sealed class Frame
        {
            public int[] rect, anchor; public int index, view; public string label;
            public float coreCut, angleMin, angleMax, reachFraction; public float[] rimRadii; public bool unfold;
        }
        [Serializable] private sealed class Sheet { public string key, file, sha256; public int width, height, ppu; public Frame[] frames; }
        [Serializable] private sealed class Document { public Sheet[] sheets; }
        public static object Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || GasSacSetup.DetectPipeline() != "URP") throw new InvalidOperationException("URP Edit Mode required");
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(DefinitionPath);
            if (definition == null) throw new InvalidOperationException("Existing S-03 definition required");
            var document = JsonUtility.FromJson<Document>(File.ReadAllText(SourceFolder + "unity-frames.json"));
            if (!AssetDatabase.IsValidFolder(ArtFolder)) AssetDatabase.CreateFolder("Assets/_Project/Art/Generated/BiomeElites", "OilFilmGlider");
            var sprites = document.sheets.SelectMany(s => Import(s, 24, SourceFolder)).ToDictionary(s => s.name);
            Sprite[] Bank(string key) { var sheet = document.sheets.Single(s => s.key == key); string prefix = Path.GetFileNameWithoutExtension(sheet.file); return Enumerable.Range(0, 24).Select(i => sprites[prefix + "_" + i.ToString("D2")]).ToArray(); }
            var side = Bank("side"); var front = Bank("front"); var back = Bank("back");
            var directions = AssetDatabase.LoadAssetAtPath<EnemyDirectionalPresentation>(DirectionPath);
            if (directions == null)
            {
                directions = ScriptableObject.CreateInstance<EnemyDirectionalPresentation>(); directions.mode = EnemyDirectionMode.Four; directions.authoredFacingLeft = false;
                directions.frames = Enumerable.Range(0,24).Select(i => new DirectionalSpriteFrame { source = side[i], front = front[i], back = back[i], label = "S03 " + i,
                    motion = i < 2 ? EnemyPoseGroup.Idle : i < 4 ? EnemyPoseGroup.Move : i < 8 ? EnemyPoseGroup.Preparation : i < 12 ? EnemyPoseGroup.Release : i < 16 ? EnemyPoseGroup.Recovery : i < 18 ? EnemyPoseGroup.Hit : EnemyPoseGroup.Death }).ToArray();
                AssetDatabase.CreateAsset(directions, DirectionPath);
            }
            var art = AssetDatabase.LoadAssetAtPath<OilFilmPresentation>(PresentationPath);
            if (art == null)
            {
                art = ScriptableObject.CreateInstance<OilFilmPresentation>(); art.directionalPresentation = directions;
                art.idleFrames = side.Take(2).ToArray(); art.moveFrames = side.Skip(2).Take(2).ToArray(); art.preparationFrames = side.Skip(4).Take(4).ToArray();
                art.releaseFrames = side.Skip(8).Take(4).ToArray(); art.recoveryFrames = side.Skip(12).Take(4).ToArray(); art.hitFrames = side.Skip(16).Take(2).ToArray(); art.deathFrames = side.Skip(18).ToArray();
                art.profiles = document.sheets.SelectMany(s => s.frames.Select(f => new OilFilmFrameProfile { sprite = sprites[Path.GetFileNameWithoutExtension(s.file) + "_" + f.index.ToString("D2")],
                    view = f.view, index = f.index, coreCut = f.coreCut, angleMin = f.angleMin, angleMax = f.angleMax, reachFraction = f.reachFraction, rimRadii = f.rimRadii, unfold = f.unfold })).ToArray();
                AssetDatabase.CreateAsset(art, PresentationPath);
            }
            if (art.membraneShader == null)
            {
                Undo.RecordObject(art, "Connect oil membrane shader dependency");
                art.membraneShader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Shaders/OilFilmTerrainClip.shader");
                EditorUtility.SetDirty(art); AssetDatabase.SaveAssetIfDirty(art);
            }
            var settings = AssetDatabase.LoadAssetAtPath<OilFilmPatternSettings>(PatternPath);
            if (settings == null) { settings = ScriptableObject.CreateInstance<OilFilmPatternSettings>(); settings.presentation = art; AssetDatabase.CreateAsset(settings, PatternPath); }
            if (definition.pattern != null && definition.pattern != settings) throw new InvalidOperationException("Do not overwrite another S-03 pattern");
            Undo.RecordObject(definition, "Connect approved S-03 filled area attack");
            definition.pattern = settings; definition.designNote = "S-03-A: fixed core, committed full frontal sector, one impact, no inner safe zone. Production S-MAP placement requires separate approval.";
            if (!definition.patternDamage.Exists(p => p.id == OilFilmPatternSettings.DamageId)) definition.patternDamage.Add(new MonsterPatternDamage { id = OilFilmPatternSettings.DamageId, coefficient = 1.5f });
            string error = definition.GetValidationError(); if (error != null) throw new InvalidOperationException(error);
            EditorUtility.SetDirty(definition); AssetDatabase.SaveAssetIfDirty(definition); Selection.activeObject = definition;
            return new { sprites = sprites.Count, directionKeys = directions.frames.Length, baseStatsPreserved = true, productionRegistration = false };
        }
        public static EnemySpawnRuleConfig PreviewRule(MonsterDefinition definition) => new EnemySpawnRuleConfig {
            name = "S-03 Oil Film Glider", monsterDefinition = definition, isElite = true, chaseRadius = 8, wanderRadius = 0, leashRadius = 16, stoppingDistance = .2f,
            addCollider = true, isTrigger = true, colliderSize = new Vector3(1f, .8f, 1f), colliderCenter = new Vector3(0, .4f, 0),
            scale = Vector3.one, useBillboard = true, useYSort = true };
        private static Sprite[] Import(Sheet sheet, int count, string sourceFolder)
        {
            if (sheet == null || sheet.frames == null || sheet.frames.Length != count || sheet.ppu <= 0)
                throw new InvalidOperationException("승인 메타데이터를 확인하세요.");
            byte[] png = File.ReadAllBytes(sourceFolder + sheet.file);
            using (var sha = SHA256.Create())
                if (!string.Equals(BitConverter.ToString(sha.ComputeHash(png)).Replace("-", "").ToLowerInvariant(), sheet.sha256, StringComparison.Ordinal))
                    throw new InvalidOperationException("승인된 PNG 해시와 다릅니다: " + sheet.file);
            string path = ArtFolder + "/" + Path.GetFileName(sheet.file);
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
