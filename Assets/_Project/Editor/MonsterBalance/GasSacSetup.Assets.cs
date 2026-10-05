using System;
using System.IO;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    public static partial class GasSacSetup
    {
        public const string PatternPath = "Assets/_Project/Data/MonsterBalance/Patterns/GasSac_A.asset";
        public const string PresentationPath = "Assets/_Project/Data/MonsterBalance/Presentations/GasSac_Presentation.asset";
        public const string DefinitionPath = "Assets/_Project/Data/MonsterBalance/Definitions/I-01.asset";
        private const string ArtRoot = "Assets/_Project/Art/Generated/BiomeElites/GasSac";

        [MenuItem("Necrocis/Balance/Set Up I-01-A Gas Sac")]
        public static void Run()
        {
            Diagnose();
            if (DetectPipeline() != "URP") throw new InvalidOperationException("현재 가스낭 표현은 프로젝트의 URP 구성을 기준으로 검증합니다.");
            Folder(ArtRoot); Folder(Path.GetDirectoryName(PatternPath)); Folder(Path.GetDirectoryName(PresentationPath));
            var presentation = AssetDatabase.LoadAssetAtPath<GasSacPresentation>(PresentationPath);
            if (presentation == null)
            {
                presentation = ScriptableObject.CreateInstance<GasSacPresentation>();
                presentation.idle = CreateBodySprite("Idle", 0, false);
                presentation.inflationFrames = new Sprite[7];
                for (int i = 0; i < 7; i++) presentation.inflationFrames[i] = CreateBodySprite("Inflate_" + i, i / 6f, false);
                presentation.deflated = CreateBodySprite("Deflated", 0, true);
                presentation.filledCircle = CreateDisc("Circle", 64, 32, false);
                presentation.gasPuff = CreateDisc("Puff", 16, 16, true);
                AssetDatabase.CreateAsset(presentation, PresentationPath);
            }
            var pattern = AssetDatabase.LoadAssetAtPath<GasSacPatternSettings>(PatternPath);
            if (pattern == null)
            {
                pattern = ScriptableObject.CreateInstance<GasSacPatternSettings>();
                pattern.presentation = presentation;
                AssetDatabase.CreateAsset(pattern, PatternPath);
            }
            MonsterDefinition definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(DefinitionPath);
            if (definition == null) throw new InvalidOperationException("I-01 원본 정의가 없습니다.");
            if (definition.pattern == null)
            {
                definition.pattern = pattern;
                definition.contact.enabled = true;
                definition.designNote = "I-01-A 기본 팽창 폭발. 예고 → 단발 폭발 → 쭈그러진 반격 창. 임시 도트 표현이며 I-01-B와 I-MAP 등록은 별도 단계.";
            }
            if (!definition.patternDamage.Exists(p => p.id == GasSacPatternSettings.DamageId))
                definition.patternDamage.Add(new MonsterPatternDamage { id = GasSacPatternSettings.DamageId, coefficient = 1.5f });
            string error = definition.GetValidationError();
            if (error != null) throw new InvalidOperationException(error);
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            Selection.activeObject = definition;
            Debug.Log("[GasSac-I01A] Setup PASS: A pattern and point-filtered sprites connected. Existing B settings preserved; no production map registration.");
        }

        public static EnemySpawnRuleConfig PreviewRule(MonsterDefinition definition) => new EnemySpawnRuleConfig
        {
            name = "I-01 GasSac", monsterDefinition = definition, isElite = true,
            chaseRadius = 7, wanderRadius = 0, leashRadius = 16, stoppingDistance = .2f,
            addCollider = true, isTrigger = true, colliderSize = new Vector3(1.1f, 1.25f, 1.1f),
            colliderCenter = new Vector3(0, .6f, 0), scale = Vector3.one, useBillboard = true, useYSort = true
        };

        public static void RebuildTemporaryPoses()
        {
            var presentation = AssetDatabase.LoadAssetAtPath<GasSacPresentation>(PresentationPath);
            if (presentation == null) { Run(); return; }
            if (AssetDatabase.GetAssetPath(presentation.idle).Contains("/V2/"))
                throw new InvalidOperationException("V2 생성 아트를 사용 중입니다. 기존 임시 도형 생성기로 덮어쓰지 않습니다.");
            presentation.idle = CreateBodySprite("Idle", 0, false);
            for (int i = 0; i < presentation.inflationFrames.Length; i++)
                presentation.inflationFrames[i] = CreateBodySprite("Inflate_" + i, i / 6f, false);
            presentation.deflated = CreateBodySprite("Deflated", 0, true);
            EditorUtility.SetDirty(presentation); AssetDatabase.SaveAssets();
            Debug.Log("[GasSac-I01A] Grounded temporary poses rebuilt.");
        }

        private static Color C(string hex) { ColorUtility.TryParseHtmlString(hex, out Color value); return value; }

        private static Sprite CreateBodySprite(string name, float inflation, bool collapsed)
        {
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color outline = C("#3B2B38"), shadow = C("#56664E"), mid = C("#82936C"), light = C("#ADB990"), shine = C("#D9E1BC");
            float rx = collapsed ? 26 : Mathf.Lerp(20, 27, inflation);
            float ry = collapsed ? 8 : Mathf.Lerp(19, 27, inflation);
            float cy = 8 + ry, cx = 32;
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float u = (x - cx - (y - cy) * (collapsed ? -.1f : .22f * (1 - inflation))) / rx;
                float v = (y - cy) / ry;
                float q = u * u + v * v;
                Color color = Color.clear;
                if (q <= 1)
                {
                    color = q > .86f ? outline : (u > .35f || v < -.55f ? shadow : mid);
                    if (q < .79f && -u + v > .25f) color = light;
                    if (q < .70f && (x - 25) * (x - 25) + (y - (cy + 8)) * (y - (cy + 8)) < 19) color = shine;
                    if (q < .7f && ((x * 13 + y * 7) % 89) < 3) color = shadow;
                    if (collapsed && q < .8f && (x + y / 2) % 9 < 2) color = shadow;
                }
                texture.SetPixel(x, y, color);
            }
            void Rect(int left, int bottom, int width, int height, Color color)
            { for (int y = bottom; y < bottom + height; y++) for (int x = left; x < left + width; x++) texture.SetPixel(x, y, color); }
            Color foot = C("#D7A18A");
            foreach (int footX in new[] { 24, 41 })
            {
                int top = 8;
                while (top < 20 && texture.GetPixel(footX, top).a == 0) top++;
                Rect(footX - 2, 4, 5, 3, foot);
                Rect(footX, 7, 2, Mathf.Max(1, top - 6), foot);
            }
            int face = collapsed ? 14 : 20;
            int faceY = collapsed ? 12 : 16;
            Rect(face, faceY, 2, 4, outline); Rect(face + 7, faceY - 1, 2, 4, outline);
            Rect(face + 2, faceY - 3, 2, 1, foot);
            texture.Apply();
            return SaveSprite(name, texture, 32, new Vector2(.5f, 4f / 64));
        }

        private static Sprite CreateDisc(string name, int size, int ppu, bool puff)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * .5f;
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center, dy = (y - center) / center;
                float q = dx * dx + dy * dy;
                Color color = q <= 1 ? Color.white : Color.clear;
                if (puff && q <= 1) color = q > .65f ? C("#889877") : C("#D7E0BE");
                texture.SetPixel(x, y, color);
            }
            texture.Apply();
            return SaveSprite(name, texture, ppu, new Vector2(.5f, .5f));
        }

        private static Sprite SaveSprite(string name, Texture2D texture, int ppu, Vector2 pivot)
        {
            string path = ArtRoot + "/" + name + ".png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None; importer.wrapMode = TextureWrapMode.Clamp;
            importer.spritePixelsPerUnit = ppu; importer.spritePivot = pivot;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteAlignment = (int)SpriteAlignment.Custom; settings.spritePivot = pivot;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void Folder(string path)
        {
            path = path.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path)) return;
            Folder(Path.GetDirectoryName(path)); AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path));
        }
    }
}
