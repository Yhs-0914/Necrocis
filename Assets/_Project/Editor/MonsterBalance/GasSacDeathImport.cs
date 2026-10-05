using System;
using System.IO;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace NecrocisEditor
{
    // Standalone live-Editor operation. Sprite metadata always goes through the supported provider API.
    public static class GasSacDeathImport
    {
        public const string SheetPath = "Assets/_Project/Art/Generated/BiomeElites/GasSac/Death/GasSac_Death_Sheet.png";

        public static object Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 가져오세요.");
            if (GasSacSetup.DetectPipeline() != "URP") throw new InvalidOperationException("URP 표현 구성을 확인하세요.");
            var art = AssetDatabase.LoadAssetAtPath<GasSacPresentation>(GasSacSetup.PresentationPath);
            if (art == null || art.idle == null) throw new InvalidOperationException("기존 V2 표현 원본이 필요합니다.");
            byte[] png = File.ReadAllBytes(SheetPath);
            int ReadInt(int offset) => (png[offset] << 24) | (png[offset + 1] << 16) | (png[offset + 2] << 8) | png[offset + 3];
            int width = ReadInt(16), height = ReadInt(20);
            AssetDatabase.ImportAsset(SheetPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(SheetPath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("TextureImporter가 없습니다.");
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true; importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = Mathf.Max(2048, Mathf.NextPowerOfTwo(Mathf.Max(width, height)));
            importer.wrapMode = TextureWrapMode.Clamp; importer.isReadable = true;
            var textureSettings = new TextureImporterSettings(); importer.ReadTextureSettings(textureSettings);
            textureSettings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(textureSettings);
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SheetPath);
            if (texture.width != width || texture.height != height) throw new InvalidOperationException("가져온 크기가 원본과 다릅니다.");
            Color32[] pixels = texture.GetPixels32();
            int transparent = pixels.Count(p => p.a == 0);
            if (transparent < pixels.Length * .4f) throw new InvalidOperationException("투명 배경을 확인하세요.");
            var rectangles = new Rect[6]; var pivots = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                int column = i % 3, row = i / 3;
                int left = Mathf.RoundToInt(column * width / 3f), right = Mathf.RoundToInt((column + 1) * width / 3f);
                int bottom = Mathf.RoundToInt((1 - row) * height / 2f), top = Mathf.RoundToInt((2 - row) * height / 2f);
                int minX = right, maxX = left, minY = top, maxY = bottom, count = 0;
                for (int y = bottom; y < top; y++) for (int x = left; x < right; x++)
                {
                    if (pixels[y * width + x].a < 160) continue;
                    minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); count++;
                }
                if (count < (right - left) * (top - bottom) * .02f) throw new InvalidOperationException("비어 있는 사망 프레임: " + i);
                // Anchor to the lowest solid feet/corpse band, never to a changing gas wisp or body center.
                int footLeft = right, footRight = left;
                for (int y = minY; y < Math.Min(top, minY + (top - bottom) / 18); y++)
                    for (int x = minX; x <= maxX; x++)
                        if (pixels[y * width + x].a >= 160) { footLeft = Math.Min(footLeft, x); footRight = Math.Max(footRight, x); }
                int pivotX = Mathf.RoundToInt((footLeft + footRight + 1) * .5f);
                const int padding = 8;
                rectangles[i] = Rect.MinMaxRect(Math.Max(left, minX - padding), Math.Max(bottom, minY - padding),
                    Math.Min(right, maxX + 1 + padding), Math.Min(top, maxY + 1 + padding));
                Rect rect = rectangles[i]; pivots[i] = new Vector2((pivotX - rect.x) / rect.width, (minY - rect.y) / rect.height);
            }
            // Different source sheet resolution, same world-size envelope as the approved idle. One PPU across all death frames.
            importer.spritePixelsPerUnit = Mathf.Ceil(Mathf.Max(rectangles[0].width / art.idle.bounds.size.x,
                rectangles[0].height / art.idle.bounds.size.y));
            importer.isReadable = false; importer.SaveAndReimport();

            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null) throw new InvalidOperationException("Sprite data provider가 없습니다.");
            provider.InitSpriteEditorDataProvider();
            var editor = provider.GetDataProvider<ISpriteFrameEditCapability>();
            if (editor == null) throw new InvalidOperationException("Sprite 편집 capability가 없습니다.");
            var capability = editor.GetEditCapability();
            foreach (EEditCapability required in new[] { EEditCapability.CreateAndDeleteSprite, EEditCapability.EditSpriteRect,
                EEditCapability.EditPivot, EEditCapability.EditSpriteName })
                if (!capability.HasCapability(required)) throw new InvalidOperationException("지원하지 않는 작업: " + required);
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (names == null) throw new InvalidOperationException("name/file-ID provider가 없습니다.");
            var previous = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
            var frames = new SpriteRect[6];
            for (int i = 0; i < 6; i++)
            {
                string name = "GasSac_Death_" + i;
                frames[i] = new SpriteRect { name = name, rect = rectangles[i], pivot = pivots[i], alignment = SpriteAlignment.Custom,
                    border = Vector4.zero, spriteID = previous.TryGetValue(name, out GUID id) ? id : GUID.Generate() };
            }
            provider.SetSpriteRects(frames);
            names.SetNameFileIdPairs(frames.Select(f => new SpriteNameFileIdPair(f.name, f.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(SheetPath).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            if (sprites.Length != 6) throw new InvalidOperationException("사망 프레임 수가 6이 아닙니다.");
            Undo.RecordObject(art, "Connect Gas Sac death animation");
            art.deathFrames = sprites;
            string error = art.GetValidationError();
            if (error != null) throw new InvalidOperationException(error);
            EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
            Debug.Log("[GasSac-Death] Six dedicated death poses connected, integer foot pivots, original alpha preserved.");
            return new { count = sprites.Length, width, height, ppu = importer.spritePixelsPerUnit,
                seconds = art.deathFrames.Length * art.deathFrameSeconds, transparentFraction = (double)transparent / pixels.Length };
        }
    }
}
