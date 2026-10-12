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
    // Standalone import operation, invoked through the live Editor's CLI eval command.
    // No AssetPostprocessor/MenuItem; capability checks precede all sprite metadata edits.
    public static class GasSacV2Import
    {
        public const string SheetPath = "Assets/_Project/Art/Generated/BiomeElites/GasSac/V2/GasSac_V2_Sheet.png";

        public static object Run()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Mode에서 가져오세요.");
            if (GasSacSetup.DetectPipeline() != "URP") throw new InvalidOperationException("예상한 URP 구성이 아닙니다.");
            byte[] png = File.ReadAllBytes(SheetPath);
            int ReadInt(int offset) => (png[offset] << 24) | (png[offset + 1] << 16) | (png[offset + 2] << 8) | png[offset + 3];
            int originalWidth = ReadInt(16), originalHeight = ReadInt(20);
            AssetDatabase.ImportAsset(SheetPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(SheetPath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("PNG TextureImporter가 필요합니다.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = Mathf.Max(2048, Mathf.NextPowerOfTwo(Mathf.Max(originalWidth, originalHeight)));
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.isReadable = true;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SheetPath);
            if (texture.width != originalWidth || texture.height != originalHeight)
                throw new InvalidOperationException("원본과 가져온 이미지 크기가 달라 슬라이스를 중단합니다.");
            Color32[] pixels = texture.GetPixels32();
            int transparent = pixels.Count(p => p.a < 16);
            if (transparent < pixels.Length * .3f) throw new InvalidOperationException("실제 투명 배경이 없습니다. 이미지 재생성이 필요합니다.");

            var rects = new Rect[8];
            var pivots = new Vector2[8];
            var names = new[] { "GasSac_V2_Idle", "GasSac_V2_Inflate_1", "GasSac_V2_Inflate_2", "GasSac_V2_Inflate_3",
                "GasSac_V2_Inflate_4", "GasSac_V2_Inflate_5", "GasSac_V2_Inflate_6", "GasSac_V2_Deflated" };
            float pixelsPerUnit = 1;
            for (int i = 0; i < 8; i++)
            {
                int column = i % 4, row = i / 4;
                int left = Mathf.RoundToInt(column * originalWidth / 4f), right = Mathf.RoundToInt((column + 1) * originalWidth / 4f);
                int bottom = Mathf.RoundToInt((1 - row) * originalHeight / 2f), top = Mathf.RoundToInt((2 - row) * originalHeight / 2f);
                int minX = right, minY = top, maxX = left, maxY = bottom, opaque = 0;
                for (int y = bottom; y < top; y++) for (int x = left; x < right; x++)
                {
                    if (pixels[y * originalWidth + x].a < 160) continue;
                    minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); opaque++;
                }
                if (opaque < (right - left) * (top - bottom) * .03f) throw new InvalidOperationException("빈 프레임: " + names[i]);
                // Ground pivot uses the two feet, not the changing center of the inflating body.
                int footLeft = right, footRight = left;
                int footBandTop = Math.Min(top, minY + Mathf.Max(6, (top - bottom) / 18));
                for (int y = minY; y < footBandTop; y++) for (int x = minX; x <= maxX; x++)
                    if (pixels[y * originalWidth + x].a >= 160) { footLeft = Math.Min(footLeft, x); footRight = Math.Max(footRight, x); }
                int pivotX = Mathf.RoundToInt((footLeft + footRight + 1) * .5f);
                const int padding = 6;
                Rect rect = Rect.MinMaxRect(Math.Max(left, minX - padding), Math.Max(bottom, minY - padding),
                    Math.Min(right, maxX + 1 + padding), Math.Min(top, maxY + 1 + padding));
                rects[i] = rect;
                pivots[i] = new Vector2((pivotX - rect.x) / rect.width, (minY - rect.y) / rect.height);
                pixelsPerUnit = Mathf.Max(pixelsPerUnit, (maxX - minX + 1) / 2.05f, (maxY - minY + 1) / 2.15f);
            }
            importer.spritePixelsPerUnit = Mathf.Ceil(pixelsPerUnit);
            importer.isReadable = false;
            importer.SaveAndReimport();

            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null) throw new InvalidOperationException("Sprite Editor data provider를 지원하지 않습니다.");
            provider.InitSpriteEditorDataProvider();
            var edit = provider.GetDataProvider<ISpriteFrameEditCapability>();
            if (edit == null) throw new InvalidOperationException("Sprite edit capability가 없습니다. 작업 중단.");
            var capability = edit.GetEditCapability();
            foreach (EEditCapability required in new[] { EEditCapability.CreateAndDeleteSprite, EEditCapability.EditSpriteRect,
                EEditCapability.EditPivot, EEditCapability.EditSpriteName })
                if (!capability.HasCapability(required)) throw new InvalidOperationException("지원하지 않는 Sprite 작업: " + required);
            var nameIds = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (nameIds == null) throw new InvalidOperationException("Sprite name/file-ID provider가 없습니다. 작업 중단.");

            var previous = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
            var frames = new SpriteRect[8];
            for (int i = 0; i < 8; i++)
                frames[i] = new SpriteRect { name = names[i], rect = rects[i], pivot = pivots[i], alignment = SpriteAlignment.Custom,
                    border = Vector4.zero, spriteID = previous.TryGetValue(names[i], out GUID id) ? id : GUID.Generate() };
            provider.SetSpriteRects(frames);
            nameIds.SetNameFileIdPairs(frames.Select(f => new SpriteNameFileIdPair(f.name, f.spriteID)));
            provider.Apply(); importer.SaveAndReimport();

            var sprites = AssetDatabase.LoadAllAssetsAtPath(SheetPath).OfType<Sprite>().ToDictionary(s => s.name);
            if (sprites.Count != 8) throw new InvalidOperationException("가져온 프레임 수가 8이 아닙니다.");
            var presentation = AssetDatabase.LoadAssetAtPath<GasSacPresentation>(GasSacSetup.PresentationPath);
            Undo.RecordObject(presentation, "Apply Gas Sac V2 sprites");
            presentation.idle = sprites[names[0]];
            presentation.inflationFrames = names.Take(7).Select(n => sprites[n]).ToArray();
            presentation.deflated = sprites[names[7]];
            EditorUtility.SetDirty(presentation); AssetDatabase.SaveAssets();
            Debug.Log($"[GasSac-V2] Imported 8 transparent poses, {originalWidth}x{originalHeight}, PPU {importer.spritePixelsPerUnit}. Integer foot anchors; original PNG alpha preserved.");
            return new { frames = sprites.Count, width = originalWidth, height = originalHeight,
                ppu = importer.spritePixelsPerUnit, transparentFraction = (double)transparent / pixels.Length, path = SheetPath };
        }
    }
}
