using System;
using System.IO;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace NecrocisEditor
{
    // Standalone Sprite Editor API operation; no import callbacks or menu-driven slicing.
    public static class InflammationEmberSetup
    {
        public const string DefinitionPath = "Assets/_Project/Data/MonsterBalance/Definitions/H-02.asset";
        public const string PatternPath = "Assets/_Project/Data/MonsterBalance/Patterns/InflammationEmber_A.asset";
        public const string PresentationPath = "Assets/_Project/Data/MonsterBalance/Presentations/InflammationEmber_Presentation.asset";
        public const string ArtRoot = "Assets/_Project/Art/Generated/BiomeElites/InflammationEmber";

        public static object Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode에서 실행하세요.");
            if (GasSacSetup.DetectPipeline() != "URP") throw new InvalidOperationException("URP 표현 구성을 확인하세요.");
            Sprite[] main = Slice(ArtRoot + "/InflammationEmber_Main.png", 5, 2, "Ember_Main_", new Vector2(1.75f, 1.55f));
            Sprite[] death = Slice(ArtRoot + "/InflammationEmber_Death.png", 3, 2, "Ember_Death_", main[0].bounds.size);
            Sprite[] thorn = Slice(ArtRoot + "/InflammationEmber_Thorn.png", 2, 1, "Ember_Thorn_", new Vector2(.7f, .32f), true);
            var art = AssetDatabase.LoadAssetAtPath<InflammationEmberPresentation>(PresentationPath);
            bool newArt = art == null;
            if (newArt) art = ScriptableObject.CreateInstance<InflammationEmberPresentation>();
            art.idleFrames = main.Take(2).ToArray(); art.moveFrames = main.Skip(2).Take(2).ToArray();
            art.preparationFrames = main.Skip(4).Take(3).ToArray(); art.release = main[7]; art.recovery = main[8]; art.hit = main[9];
            art.deathFrames = death; art.thornFrames = thorn;
            art.groundDisc = AssetDatabase.LoadAssetAtPath<GasSacPresentation>(GasSacSetup.PresentationPath).filledCircle;
            if (newArt) AssetDatabase.CreateAsset(art, PresentationPath); else EditorUtility.SetDirty(art);
            var pattern = AssetDatabase.LoadAssetAtPath<InflammationEmberPatternSettings>(PatternPath);
            if (pattern == null)
            { pattern = ScriptableObject.CreateInstance<InflammationEmberPatternSettings>(); pattern.presentation = art; AssetDatabase.CreateAsset(pattern, PatternPath); }
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(DefinitionPath);
            if (definition == null) throw new InvalidOperationException("H-02 원본 정의가 없습니다.");
            if (definition.pattern == null)
            {
                definition.pattern = pattern; definition.contact.enabled = true;
                definition.designNote = "H-02-A: 유효 피격 시 방향 고정 → 핵 점등 → 가시탄 1발 → 회복. 연타는 예약/타이머를 늘리지 않음. 별도 사망 6프레임. H-MAP 본맵 등록은 별도 승인.";
            }
            if (!definition.patternDamage.Exists(p => p.id == InflammationEmberPatternSettings.DamageId))
                definition.patternDamage.Add(new MonsterPatternDamage { id = InflammationEmberPatternSettings.DamageId, coefficient = 1.5f });
            string error = definition.GetValidationError();
            if (error != null) throw new InvalidOperationException(error);
            EditorUtility.SetDirty(definition); AssetDatabase.SaveAssets(); Selection.activeObject = definition;
            return new { mainFrames = main.Length, deathFrames = death.Length, thornFrames = thorn.Length,
                mainPpu = main[0].pixelsPerUnit, deathPpu = death[0].pixelsPerUnit, thornPpu = thorn[0].pixelsPerUnit };
        }

        public static EnemySpawnRuleConfig PreviewRule(MonsterDefinition definition) => new EnemySpawnRuleConfig
        {
            name = "H-02 Inflammation Ember", monsterDefinition = definition, isElite = true,
            chaseRadius = 7, wanderRadius = 0, leashRadius = 16, stoppingDistance = .2f,
            addCollider = true, isTrigger = true, colliderSize = new Vector3(1.2f, 1.2f, .95f),
            colliderCenter = new Vector3(0, .55f, 0), scale = Vector3.one, useBillboard = true, useYSort = true
        };

        private static Sprite[] Slice(string path, int columns, int rows, string prefix, Vector2 firstFrameSize, bool centerPivot = false)
        {
            byte[] png = File.ReadAllBytes(path);
            int ReadInt(int offset) => (png[offset] << 24) | (png[offset + 1] << 16) | (png[offset + 2] << 8) | png[offset + 3];
            int width = ReadInt(16), height = ReadInt(20), count = columns * rows;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("TextureImporter가 없습니다: " + path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None; importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = Mathf.Max(2048, Mathf.NextPowerOfTwo(Mathf.Max(width, height))); importer.isReadable = true;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture.width != width || texture.height != height) throw new InvalidOperationException("가져온 크기가 원본과 다릅니다.");
            Color32[] pixels = texture.GetPixels32();
            if (pixels.Count(p => p.a == 0) < pixels.Length * .35f) throw new InvalidOperationException("실제 투명 배경이 필요합니다.");
            Rect[] rects = new Rect[count]; Vector2[] pivots = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                int column = i % columns, row = i / columns;
                int left = Mathf.RoundToInt(column * width / (float)columns), right = Mathf.RoundToInt((column + 1) * width / (float)columns);
                int bottom = Mathf.RoundToInt((rows - row - 1) * height / (float)rows), top = Mathf.RoundToInt((rows - row) * height / (float)rows);
                int minX = right, maxX = left, minY = top, maxY = bottom, opaque = 0;
                for (int y = bottom; y < top; y++) for (int x = left; x < right; x++)
                {
                    if (pixels[y * width + x].a < 160) continue;
                    minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); opaque++;
                }
                if (opaque < (right - left) * (top - bottom) * .02f) throw new InvalidOperationException("빈 프레임: " + prefix + i);
                const int pad = 6;
                rects[i] = Rect.MinMaxRect(Math.Max(left, minX - pad), Math.Max(bottom, minY - pad), Math.Min(right, maxX + 1 + pad), Math.Min(top, maxY + 1 + pad));
                int pivotX = Mathf.RoundToInt((left + right) * .5f); // Fixed cell anchor: a lifted foot must not shift the whole walking body.
                pivots[i] = new Vector2(((centerPivot ? Mathf.Round((minX + maxX) * .5f) : pivotX) - rects[i].x) / rects[i].width, ((centerPivot ? Mathf.Round((minY + maxY) * .5f) : minY) - rects[i].y) / rects[i].height);
            }
            importer.spritePixelsPerUnit = Mathf.Ceil(Mathf.Max(rects[0].width / firstFrameSize.x, rects[0].height / firstFrameSize.y));
            importer.isReadable = false; importer.SaveAndReimport();
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null) throw new InvalidOperationException("Sprite data provider가 없습니다.");
            provider.InitSpriteEditorDataProvider();
            var edit = provider.GetDataProvider<ISpriteFrameEditCapability>();
            if (edit == null) throw new InvalidOperationException("Sprite edit capability가 없습니다.");
            var capability = edit.GetEditCapability();
            foreach (EEditCapability required in new[] { EEditCapability.CreateAndDeleteSprite, EEditCapability.EditSpriteRect,
                EEditCapability.EditPivot, EEditCapability.EditSpriteName })
                if (!capability.HasCapability(required)) throw new InvalidOperationException("지원하지 않는 Sprite 작업: " + required);
            var nameIds = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (nameIds == null) throw new InvalidOperationException("name/file-ID provider가 없습니다.");
            var old = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
            var frames = new SpriteRect[count];
            for (int i = 0; i < count; i++)
            {
                string name = prefix + i.ToString("D2");
                frames[i] = new SpriteRect { name = name, rect = rects[i], pivot = pivots[i], alignment = SpriteAlignment.Custom,
                    border = Vector4.zero, spriteID = old.TryGetValue(name, out GUID id) ? id : GUID.Generate() };
            }
            provider.SetSpriteRects(frames); nameIds.SetNameFileIdPairs(frames.Select(f => new SpriteNameFileIdPair(f.name, f.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            if (sprites.Length != count) throw new InvalidOperationException("가져온 프레임 수를 확인하세요.");
            return sprites;
        }
    }
}
