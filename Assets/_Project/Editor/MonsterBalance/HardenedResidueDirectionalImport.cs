using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace NecrocisEditor
{
    // Import approved D-1 PNGs without changing their pixels or the existing side/prop sprite IDs.
    public static class HardenedResidueDirectionalImport
    {
        public const string AssetPath = "Assets/_Project/Data/MonsterBalance/Presentations/HardenedResidue_Directions.asset";
        public const string ArtFolder = HardenedResidueSetup.ArtRoot + "/Directions";
        public const string SelectedRubbleFile = "HardenedResidue_Rubble_Vertical_v7_aligned.png";
        private const string PreviewFolder = "output/art/HardenedResidueDirections/";
        [Serializable] private sealed class Frame { public int[] rect, anchor, size; }
        [Serializable] private sealed class Sheet { public string file; public int width, height, referenceHeight; public Frame[] frames; }
        [Serializable] private sealed class ApprovedArt { public Sheet frontMain, backMain, frontDeath, backDeath; }

        public static object Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            if (GasSacSetup.DetectPipeline() != "URP") throw new InvalidOperationException("기존 URP 구성을 확인하세요.");
            var art = AssetDatabase.LoadAssetAtPath<HardenedResiduePresentation>(HardenedResidueSetup.PresentationPath);
            if (art == null || art.GetValidationError() != null) throw new InvalidOperationException("기존 I-02 표현이 필요합니다.");
            string html = File.ReadAllText(PreviewFolder + "preview.html");
            int start = html.IndexOf("const art=", StringComparison.Ordinal) + "const art=".Length;
            if (start < "const art=".Length) throw new InvalidOperationException("승인된 미리보기 메타데이터를 찾지 못했습니다.");
            int end = html.IndexOf(';', start);
            var approved = JsonUtility.FromJson<ApprovedArt>(html.Substring(start, end - start));
            if (!AssetDatabase.IsValidFolder(ArtFolder)) AssetDatabase.CreateFolder(HardenedResidueSetup.ArtRoot, "Directions");
            float bodyHeight = SolidHeight(art.idleFrames[0]) / art.idleFrames[0].pixelsPerUnit;
            Sprite[] front = Import(approved.frontMain, 11, bodyHeight, true), back = Import(approved.backMain, 11, bodyHeight, false);
            Sprite[] frontDeath = Import(approved.frontDeath, 6, bodyHeight, false), backDeath = Import(approved.backDeath, 6, bodyHeight, false);
            var directions = AssetDatabase.LoadAssetAtPath<EnemyDirectionalPresentation>(AssetPath);
            if (directions == null)
            {
                directions = ScriptableObject.CreateInstance<EnemyDirectionalPresentation>();
                Sprite[] sources = art.idleFrames.Concat(art.moveFrames).Concat(art.liftFrames).Concat(new[] { art.release, art.recovery, art.hit }).ToArray();
                var frames = new List<DirectionalSpriteFrame>();
                for (int i = 0; i < 11; i++) frames.Add(new DirectionalSpriteFrame {
                    motion = i < 2 ? EnemyPoseGroup.Idle : i < 4 ? EnemyPoseGroup.Move : i < 8 ? EnemyPoseGroup.Preparation
                        : i == 8 ? EnemyPoseGroup.Release : i == 9 ? EnemyPoseGroup.Recovery : EnemyPoseGroup.Hit,
                    label = i < 2 ? "대기 " + (i + 1) : i < 4 ? "이동 " + (i - 1) : i < 8 ? "조각 들기 " + (i - 3)
                        : i == 8 ? "내려놓기" : i == 9 ? "회복" : "피격",
                    source = sources[i], front = front[i], back = back[i] });
                for (int i = 0; i < 6; i++) frames.Add(new DirectionalSpriteFrame {
                    motion = EnemyPoseGroup.Death, label = "사망 " + (i + 1), source = art.deathFrames[i], front = frontDeath[i], back = backDeath[i] });
                directions.frames = frames.ToArray();
                // Bottom centers of the raised chip, measured in the approved final lift pose (top-left PNG coordinates).
                directions.sideOrigin = Origin(art.liftFrames[3], new Vector2(1305, 530));
                directions.frontOrigin = Origin(front[7], new Vector2(1259, 507));
                directions.backOrigin = Origin(back[7], new Vector2(1282, 515));
                AssetDatabase.CreateAsset(directions, AssetPath);
            }
            Undo.RecordObject(art, "Connect I-02 directional presentation");
            art.directionalPresentation = directions;
            art.airborneChip = AssetDatabase.LoadAllAssetsAtPath(ArtFolder + "/" + approved.frontMain.file).OfType<Sprite>()
                .Single(s => s.name == "Residue_AirborneChip");
            string error = art.GetValidationError();
            if (error != null) throw new InvalidOperationException(error);
            EditorUtility.SetDirty(art); AssetDatabase.SaveAssets(); Selection.activeObject = directions;
            return new { asset = AssetPath, keys = directions.frames.Length, newBodyDeathSprites = 34, chipCrop = art.airborneChip.name,
                directions.sideOrigin, directions.frontOrigin, directions.backOrigin };
        }

        private static Vector2 Origin(Sprite liftedBody, Vector2 pngPoint)
        {
            Vector2 anchor = liftedBody.rect.position + liftedBody.pivot;
            return (new Vector2(pngPoint.x, liftedBody.texture.height - pngPoint.y) - anchor) / liftedBody.pixelsPerUnit;
        }

        public static object ImportSelectedRubble()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            if (GasSacSetup.DetectPipeline() != "URP") throw new InvalidOperationException("기존 URP 구성을 확인하세요.");
            var art = AssetDatabase.LoadAssetAtPath<HardenedResiduePresentation>(HardenedResidueSetup.PresentationPath);
            if (art == null || art.rubble == null) throw new InvalidOperationException("기존 잔해 원본이 필요합니다.");
            var sheet = new Sheet { file = SelectedRubbleFile, width = 560, height = 754,
                referenceHeight = 742, frames = new[] {
                    new Frame { rect = new[] { 0, 0, 560, 754 }, anchor = new[] { 291, 748 } } } };
            Sprite[] sprites = Import(sheet, 1, 742f / art.rubble.pixelsPerUnit, false, "output/art/HardenedResidueRubbleDirections/");
            Sprite sprite = sprites[0];
            Undo.RecordObject(art, "Connect approved aligned residue rubble");
            // Replacing the art must preserve the presentation ratio edited in the Inspector.
            art.verticalRubble = sprite;
            string error = art.GetValidationError();
            if (error != null) throw new InvalidOperationException(error);
            EditorUtility.SetDirty(art); AssetDatabase.SaveAssetIfDirty(art);
            return new { sprite = sprite.name, path = AssetDatabase.GetAssetPath(sprite), pixelsUnchanged = true,
                importedFrames = sprites.Length, groundDepthFraction = art.verticalRubbleGroundDepthFraction,
                orientation = "aligned vertical view; runtime follows the committed four-direction body bank" };
        }

        private static float SolidHeight(Sprite sprite)
        {
            var texture = new Texture2D(2, 2);
            try
            {
                if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite.texture)))) throw new InvalidOperationException("PNG 읽기 실패");
                Color32[] pixels = texture.GetPixels32(); Rect r = sprite.rect; int min = (int)r.yMax, max = (int)r.y;
                for (int y = (int)r.y; y < r.yMax; y++) for (int x = (int)r.x; x < r.xMax; x++)
                    if (pixels[y * texture.width + x].a >= 128) { min = Math.Min(min, y); max = Math.Max(max, y); }
                return max - min + 1;
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        private static Sprite[] Import(Sheet sheet, int count, float bodyHeight, bool chip, string sourceFolder = PreviewFolder)
        {
            if (sheet == null || sheet.frames == null || sheet.frames.Length != count) throw new InvalidOperationException("승인 프레임 수가 다릅니다.");
            string path = ArtFolder + "/" + sheet.file;
            byte[] png = File.ReadAllBytes(sourceFolder + sheet.file);
            if (File.Exists(path))
            {
                if (!png.SequenceEqual(File.ReadAllBytes(path))) throw new InvalidOperationException("다른 기존 PNG를 덮어쓰지 않습니다: " + path);
            }
            else File.Copy(sourceFolder + sheet.file, path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048; importer.wrapMode = TextureWrapMode.Clamp;
            importer.spritePixelsPerUnit = Mathf.Ceil(sheet.referenceHeight / bodyHeight);
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture.width != sheet.width || texture.height != sheet.height) throw new InvalidOperationException("가져오기 중 PNG 크기가 바뀌었습니다.");
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null) throw new InvalidOperationException("Sprite data provider가 없습니다.");
            provider.InitSpriteEditorDataProvider();
            var edit = provider.GetDataProvider<ISpriteFrameEditCapability>();
            if (edit == null) throw new InvalidOperationException("Sprite 편집 capability가 없습니다.");
            var capability = edit.GetEditCapability();
            foreach (EEditCapability required in new[] { EEditCapability.CreateAndDeleteSprite, EEditCapability.EditSpriteRect, EEditCapability.EditPivot, EEditCapability.EditSpriteName, EEditCapability.EditBorder })
                if (!capability.HasCapability(required)) throw new InvalidOperationException("지원하지 않는 Sprite 작업: " + required);
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (names == null) throw new InvalidOperationException("name/file-ID provider가 없습니다.");
            var previous = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
            var rects = new List<SpriteRect>();
            void Add(string name, int[] bounds, int[] anchor)
            {
                Rect rect = Rect.MinMaxRect(bounds[0], sheet.height - bounds[3], bounds[2], sheet.height - bounds[1]);
                if (rect.xMin < 0 || rect.yMin < 0 || rect.xMax > sheet.width || rect.yMax > sheet.height) throw new InvalidOperationException("잘못된 Sprite 영역");
                rects.Add(new SpriteRect { name = name, rect = rect,
                    pivot = new Vector2((anchor[0] - bounds[0]) / rect.width, (bounds[3] - anchor[1]) / rect.height),
                    alignment = SpriteAlignment.Custom, border = Vector4.zero,
                    spriteID = previous.TryGetValue(name, out GUID id) ? id : GUID.Generate() });
            }
            string prefix = Path.GetFileNameWithoutExtension(sheet.file);
            for (int i = 0; i < count; i++) Add(prefix + "_" + i.ToString("D2"), sheet.frames[i].rect, sheet.frames[i].anchor);
            // Only the isolated front chip is sampled. No PNG editing, extra body pixels, or new artwork.
            if (chip) Add("Residue_AirborneChip", new[] { 1187, 393, 1331, 509 }, new[] { 1259, 507 });
            provider.SetSpriteRects(rects.ToArray()); names.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Where(s => s.name.StartsWith(prefix + "_", StringComparison.Ordinal)).OrderBy(s => s.name).ToArray();
            if (sprites.Length != count) throw new InvalidOperationException("가져온 Sprite 수가 다릅니다.");
            return sprites;
        }
    }
}
