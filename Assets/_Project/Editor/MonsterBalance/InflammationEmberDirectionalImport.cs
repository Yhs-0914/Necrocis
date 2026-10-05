using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Necrocis;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace NecrocisEditor
{
    // Explicit operation on approved PNGs; no import callbacks or automatic regeneration.
    public static class InflammationEmberDirectionalImport
    {
        public const string AssetPath = "Assets/_Project/Data/MonsterBalance/Presentations/InflammationEmber_Directions.asset";
        public const string ArtFolder = InflammationEmberSetup.ArtRoot + "/Directions";
        public const string SourceFolder = "output/art/InflammationEmberDirections/";
        [Serializable] private sealed class Frame { public int[] rect, anchor; public string label; }
        [Serializable] private sealed class Sheet { public string file, sha256; public int width, height, ppu; public Frame[] frames; }
        [Serializable] private sealed class Art { public Sheet frontMain, backMain, frontDeath, backDeath; }
        [Serializable] private sealed class Document { public Art art; }

        public static object Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            if (GasSacSetup.DetectPipeline() != "URP") throw new InvalidOperationException("기존 URP 구성을 확인하세요.");
            var presentation = AssetDatabase.LoadAssetAtPath<InflammationEmberPresentation>(InflammationEmberSetup.PresentationPath);
            if (presentation == null || presentation.GetValidationError() != null) throw new InvalidOperationException("기존 H-02 미술 원본이 필요합니다.");
            var approved = JsonUtility.FromJson<Document>(File.ReadAllText(SourceFolder + "frames.json")).art;
            if (!AssetDatabase.IsValidFolder(ArtFolder)) AssetDatabase.CreateFolder(InflammationEmberSetup.ArtRoot, "Directions");
            Sprite[] front = Import(approved.frontMain, 10), back = Import(approved.backMain, 10);
            Sprite[] frontDeath = Import(approved.frontDeath, 6), backDeath = Import(approved.backDeath, 6);
            var directions = AssetDatabase.LoadAssetAtPath<EnemyDirectionalPresentation>(AssetPath);
            if (directions == null)
            {
                directions = ScriptableObject.CreateInstance<EnemyDirectionalPresentation>();
                directions.authoredFacingLeft = false; // Existing Ember side art faces right.
                directions.mode = EnemyDirectionMode.Four;
                // Projecting this local offset to the ground joins the near thorn's tail to the front spine.
                directions.frontOrigin = new Vector2(0, .18f);
                Sprite[] sources = presentation.idleFrames.Concat(presentation.moveFrames).Concat(presentation.preparationFrames)
                    .Concat(new[] { presentation.release, presentation.recovery, presentation.hit }).ToArray();
                if (sources.Length != 10 || presentation.deathFrames.Length != 6) throw new InvalidOperationException("승인된 원본 동작 수가 다릅니다.");
                var rows = new List<DirectionalSpriteFrame>();
                for (int i = 0; i < sources.Length; i++) rows.Add(new DirectionalSpriteFrame {
                    motion = i < 2 ? EnemyPoseGroup.Idle : i < 4 ? EnemyPoseGroup.Move : i < 7 ? EnemyPoseGroup.Preparation
                        : i == 7 ? EnemyPoseGroup.Release : i == 8 ? EnemyPoseGroup.Recovery : EnemyPoseGroup.Hit,
                    label = approved.frontMain.frames[i].label, source = sources[i], front = front[i], back = back[i] });
                for (int i = 0; i < 6; i++) rows.Add(new DirectionalSpriteFrame {
                    motion = EnemyPoseGroup.Death, label = "사망 " + (i + 1), source = presentation.deathFrames[i], front = frontDeath[i], back = backDeath[i] });
                directions.frames = rows.ToArray();
                AssetDatabase.CreateAsset(directions, AssetPath);
            }
            string error = directions.GetValidationError();
            if (error != null) throw new InvalidOperationException(error);
            Undo.RecordObject(presentation, "Connect approved H-02 directional art");
            presentation.directionalPresentation = directions;
            EditorUtility.SetDirty(presentation); AssetDatabase.SaveAssetIfDirty(presentation);
            Selection.activeObject = directions;
            return new { asset = AssetPath, newSprites = 32, sourceKeys = directions.frames.Length, authoredFacing = "right",
                directions.sideOrigin, directions.frontOrigin, directions.backOrigin, pixelsUnchanged = true };
        }

        private static Sprite[] Import(Sheet sheet, int count)
        {
            if (sheet == null || sheet.frames == null || sheet.frames.Length != count || sheet.ppu <= 0)
                throw new InvalidOperationException("승인 메타데이터를 확인하세요.");
            byte[] png = File.ReadAllBytes(SourceFolder + sheet.file);
            using (var sha = SHA256.Create())
                if (!string.Equals(BitConverter.ToString(sha.ComputeHash(png)).Replace("-", "").ToLowerInvariant(), sheet.sha256, StringComparison.Ordinal))
                    throw new InvalidOperationException("승인된 PNG 해시와 다릅니다: " + sheet.file);
            string path = ArtFolder + "/" + sheet.file;
            if (File.Exists(path))
            {
                if (!png.SequenceEqual(File.ReadAllBytes(path))) throw new InvalidOperationException("다른 기존 미술을 덮어쓰지 않습니다: " + path);
            }
            else File.Copy(SourceFolder + sheet.file, path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true; importer.npotScale = TextureImporterNPOTScale.None; importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 2048; importer.spritePixelsPerUnit = sheet.ppu;
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
