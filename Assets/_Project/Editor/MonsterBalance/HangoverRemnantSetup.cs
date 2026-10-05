using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Necrocis;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace NecrocisEditor
{
    public static class HangoverRemnantSetup
    {
        public const string DefinitionPath = "Assets/_Project/Data/MonsterBalance/Definitions/H-03.asset";
        public const string PatternPath = "Assets/_Project/Data/MonsterBalance/Patterns/HangoverRemnant_A.asset";
        public const string PresentationPath = "Assets/_Project/Data/MonsterBalance/Presentations/HangoverRemnant_Presentation.asset";
        public const string ArtFolder = "Assets/_Project/Art/Generated/BiomeElites/HangoverRemnant";
        public const string SourceFolder = "output/art/HangoverRemnantDirections/";
        private const string SharedSource = "output/art/HangoverRemnant/";
        [Serializable] private sealed class Frame { public int[] rect, anchor; public string label; }
        [Serializable] private sealed class Sheet { public string file, sha256; public int width, height, ppu; public Frame[] frames; }
        [Serializable] private sealed class Art { public Sheet frontMain, frontDeath, remnant, burst; }
        [Serializable] private sealed class Document { public Art art; }

        public static object Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            if (GasSacSetup.DetectPipeline() != "URP") throw new InvalidOperationException("기존 URP 구성을 확인하세요.");
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(DefinitionPath);
            if (definition == null) throw new InvalidOperationException("기존 H-03 정의가 없습니다.");
            var approved = JsonUtility.FromJson<Document>(File.ReadAllText(SourceFolder + "frames.json")).art;
            if (!AssetDatabase.IsValidFolder(ArtFolder)) AssetDatabase.CreateFolder("Assets/_Project/Art/Generated/BiomeElites", "HangoverRemnant");
            var main = Import(approved.frontMain, 12, SourceFolder);
            var death = Import(approved.frontDeath, 6, SourceFolder);
            var remnant = Import(approved.remnant, 2, SharedSource);
            var burst = Import(approved.burst, 4, SharedSource);
            var art = AssetDatabase.LoadAssetAtPath<HangoverRemnantPresentation>(PresentationPath);
            if (art == null)
            {
                art = ScriptableObject.CreateInstance<HangoverRemnantPresentation>();
                art.idleFrames = main.Take(2).ToArray(); art.moveFrames = main.Skip(2).Take(2).ToArray();
                art.preparationFrames = main.Skip(4).Take(3).ToArray(); art.release = main[7];
                art.retreatFrames = main.Skip(8).Take(2).ToArray(); art.recovery = main[10]; art.hit = main[11];
                art.deathFrames = death; art.remnantFrames = remnant; art.burstFrames = burst;
                art.groundDisc = AssetDatabase.LoadAssetAtPath<GasSacPresentation>(GasSacSetup.PresentationPath).filledCircle;
                AssetDatabase.CreateAsset(art, PresentationPath);
            }
            var settings = AssetDatabase.LoadAssetAtPath<HangoverRemnantPatternSettings>(PatternPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<HangoverRemnantPatternSettings>(); settings.presentation = art;
                AssetDatabase.CreateAsset(settings, PatternPath);
            }
            if (definition.pattern != null && definition.pattern != settings) throw new InvalidOperationException("다른 H-03 패턴을 덮어쓰지 않습니다.");
            Undo.RecordObject(definition, "Connect approved H-03 fixed-front pattern");
            if (definition.pattern == null)
            {
                definition.pattern = settings; definition.contact.enabled = true;
                definition.designNote = "H-03-A: 정면 고정, 자기 지면에 0.65초 예고 → 잔여물 1개 → 대상 반대 후퇴 → 생성 1.1초 후 단발 폭발. 전용 정면 사망 6장. H-MAP 등록 별도 승인.";
            }
            if (!definition.patternDamage.Exists(x => x.id == HangoverRemnantPatternSettings.DamageId))
                definition.patternDamage.Add(new MonsterPatternDamage { id = HangoverRemnantPatternSettings.DamageId, coefficient = 1.5f });
            string error = definition.GetValidationError(); if (error != null) throw new InvalidOperationException(error);
            EditorUtility.SetDirty(definition); AssetDatabase.SaveAssetIfDirty(definition); Selection.activeObject = definition;
            return new { main = main.Length, death = death.Length, remnant = remnant.Length, burst = burst.Length,
                fixedFront = true, sourcePixelsUnchanged = true, productionMapRegistration = false };
        }

        public static EnemySpawnRuleConfig PreviewRule(MonsterDefinition definition) => new EnemySpawnRuleConfig
        {
            name = "H-03 Hangover Remnant", monsterDefinition = definition, isElite = true,
            chaseRadius = 7, wanderRadius = 0, leashRadius = 16, stoppingDistance = .2f,
            addCollider = true, isTrigger = true, colliderSize = new Vector3(1.25f, 1.1f, .95f),
            colliderCenter = new Vector3(0, .5f, 0), scale = Vector3.one, useBillboard = true, useYSort = true
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
                var frame = sheet.frames[i]; int[] b = frame.rect, a = frame.anchor;
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
