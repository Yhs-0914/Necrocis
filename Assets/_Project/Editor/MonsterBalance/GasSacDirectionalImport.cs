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
    public static class GasSacDirectionalImport
    {
        public const string AssetPath = "Assets/_Project/Data/MonsterBalance/Presentations/GasSac_Directions.asset";
        public const string ArtFolder = "Assets/_Project/Art/Generated/BiomeElites/GasSac/Directions";

        public static object Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            if (GasSacSetup.DetectPipeline() != "URP") throw new InvalidOperationException("URP 구성을 확인하세요.");
            var art = AssetDatabase.LoadAssetAtPath<GasSacPresentation>(GasSacSetup.PresentationPath);
            if (art == null || art.GetValidationError() != null) throw new InvalidOperationException("기존 가스낭 미술이 필요합니다.");
            if (!AssetDatabase.IsValidFolder(ArtFolder)) AssetDatabase.CreateFolder(Path.GetDirectoryName(ArtFolder), "Directions");
            float referenceHeight = SolidHeight(art.idle) / art.idle.pixelsPerUnit;
            Sprite[] front = Import("Front_Main", 8, referenceHeight), back = Import("Back_Main", 8, referenceHeight);
            Sprite[] frontDeath = Import("Front_Death", 6, referenceHeight), backDeath = Import("Back_Death", 6, referenceHeight);
            var directions = AssetDatabase.LoadAssetAtPath<EnemyDirectionalPresentation>(AssetPath);
            if (directions == null)
            {
                directions = ScriptableObject.CreateInstance<EnemyDirectionalPresentation>();
                var frames = new List<DirectionalSpriteFrame>();
                for (int i = 0; i < 8; i++) frames.Add(new DirectionalSpriteFrame {
                    motion = i == 0 ? EnemyPoseGroup.Idle : i == 7 ? EnemyPoseGroup.Recovery : EnemyPoseGroup.Preparation,
                    label = i == 0 ? "대기 · 팽창 시작" : i == 7 ? "수축 · 회복" : "팽창 " + (i + 1),
                    source = i == 7 ? art.deflated : art.inflationFrames[i], front = front[i], back = back[i] });
                for (int i = 0; i < 6; i++) frames.Add(new DirectionalSpriteFrame {
                    motion = EnemyPoseGroup.Death, label = "사망 " + (i + 1), source = art.deathFrames[i],
                    front = frontDeath[i], back = backDeath[i] });
                directions.frames = frames.ToArray();
                AssetDatabase.CreateAsset(directions, AssetPath);
            }
            string error = directions.GetValidationError();
            if (error != null) throw new InvalidOperationException(error);
            Undo.RecordObject(art, "Connect directional presentation authoring reference");
            art.directionalPresentation = directions;
            EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
            return new { asset = AssetPath, sourceKeys = directions.frames.Length, newSprites = 28,
                mode = directions.mode.ToString(), combatBinding = "GasSacElitePattern binds on spawn" };
        }

        private static float SolidHeight(Sprite sprite)
        {
            var texture = new Texture2D(2, 2);
            try
            {
                if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite.texture))))
                    throw new InvalidOperationException("원본 PNG 읽기 실패");
                Color32[] pixels = texture.GetPixels32(); Rect r = sprite.rect;
                int min = (int)r.yMax, max = (int)r.y;
                for (int y = (int)r.y; y < r.yMax; y++) for (int x = (int)r.x; x < r.xMax; x++)
                    if (pixels[y * texture.width + x].a >= 128) { min = Math.Min(min, y); max = Math.Max(max, y); }
                return max - min + 1;
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        private static Sprite[] Import(string suffix, int count, float referenceHeight)
        {
            string file = "GasSac_" + suffix + ".png", path = ArtFolder + "/" + file;
            string source = "output/art/GasSacDirections/" + file;
            byte[] png = File.ReadAllBytes(source);
            if (File.Exists(path))
            {
                if (!png.SequenceEqual(File.ReadAllBytes(path))) throw new InvalidOperationException("기존 다른 PNG를 덮어쓰지 않습니다: " + path);
            }
            else File.Copy(source, path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
            importer.wrapMode = TextureWrapMode.Clamp; importer.isReadable = true;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            int width = texture.width, height = texture.height, columns = count / 2;
            Color32[] pixels = texture.GetPixels32();
            int ReadInt(int n) => (png[n] << 24) | (png[n + 1] << 16) | (png[n + 2] << 8) | png[n + 3];
            if (width != ReadInt(16) || height != ReadInt(20)) throw new InvalidOperationException("PNG 해상도가 변경됐습니다.");
            if (pixels.Count(p => p.a == 0) < pixels.Length / 2) throw new InvalidOperationException("투명도를 확인하세요.");
            var occupied = new bool[height];
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
                if (pixels[y * width + x].a >= 128) { occupied[y] = true; break; }
            int split = height / 2;
            if (occupied[split])
            {
                int best = -1, distance = int.MaxValue;
                for (int y = 0; y < height;)
                {
                    if (occupied[y]) { y++; continue; }
                    int start = y;
                    while (y < height && !occupied[y]) y++;
                    int middle = (start + y) / 2;
                    if (y - start > 8 && start > height * .2f && y < height * .7f && Math.Abs(middle - split) < distance)
                    { best = middle; distance = Math.Abs(middle - split); }
                }
                if (best < 0) throw new InvalidOperationException("분리 가능한 투명 행 간격이 없습니다.");
                split = best;
            }
            var rectangles = new Rect[count]; var pivots = new Vector2[count];
            int firstHeight = 0;
            for (int i = 0; i < count; i++)
            {
                int left = Mathf.RoundToInt(i % columns * width / (float)columns), right = Mathf.RoundToInt((i % columns + 1) * width / (float)columns);
                int bottom = i < columns ? split : 0, top = i < columns ? height : split;
                int minX = right, maxX = left, minY = top, maxY = bottom, solid = 0;
                for (int y = bottom; y < top; y++) for (int x = left; x < right; x++)
                    if (pixels[y * width + x].a >= 128)
                    { minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); solid++; }
                if (solid < 100) throw new InvalidOperationException("빈 프레임 " + i);
                int footLeft = right, footRight = left;
                for (int y = minY; y < minY + Mathf.Max(1, (maxY - minY + 1) * .3f); y++) for (int x = minX; x <= maxX; x++)
                {
                    Color32 p = pixels[y * width + x];
                    if (p.a >= 128 && p.r > p.g * 1.12f && p.r > 90 && p.g > 40)
                    { footLeft = Math.Min(footLeft, x); footRight = Math.Max(footRight, x); }
                }
                int anchorX = Mathf.RoundToInt(((footLeft <= footRight ? footLeft + footRight : minX + maxX) + 1) * .5f);
                Rect rect = Rect.MinMaxRect(Math.Max(left, minX - 3), Math.Max(bottom, minY - 3), Math.Min(right, maxX + 4), Math.Min(top, maxY + 4));
                rectangles[i] = rect; pivots[i] = new Vector2((anchorX - rect.x) / rect.width, (minY - rect.y) / rect.height);
                if (i == 0) firstHeight = maxY - minY + 1;
            }
            importer.spritePixelsPerUnit = Mathf.Ceil(firstHeight / referenceHeight);
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null) throw new InvalidOperationException("Sprite provider가 없습니다.");
            provider.InitSpriteEditorDataProvider();
            var edit = provider.GetDataProvider<ISpriteFrameEditCapability>();
            if (edit == null) throw new InvalidOperationException("Sprite 편집 capability가 없습니다.");
            var capability = edit.GetEditCapability();
            foreach (EEditCapability required in new[] { EEditCapability.CreateAndDeleteSprite, EEditCapability.EditSpriteRect, EEditCapability.EditPivot, EEditCapability.EditSpriteName })
                if (!capability.HasCapability(required)) throw new InvalidOperationException("지원하지 않는 작업: " + required);
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (names == null) throw new InvalidOperationException("name/file-ID provider가 없습니다.");
            var previous = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
            var rects = new SpriteRect[count];
            for (int i = 0; i < count; i++)
            {
                string name = "GasSac_" + suffix + "_" + i.ToString("D2");
                rects[i] = new SpriteRect { name = name, rect = rectangles[i], pivot = pivots[i], alignment = SpriteAlignment.Custom,
                    border = Vector4.zero, spriteID = previous.TryGetValue(name, out GUID id) ? id : GUID.Generate() };
            }
            provider.SetSpriteRects(rects); names.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply(); importer.isReadable = false; importer.SaveAndReimport();
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            if (sprites.Length != count) throw new InvalidOperationException("가져온 프레임 수가 다릅니다.");
            return sprites;
        }
    }
}
