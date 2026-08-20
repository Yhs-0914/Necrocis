using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 맵 팔레트와 무관하게 보스방 입구를 읽을 수 있도록 만드는 고대비 표식.
    /// 검은 문 실루엣, 해골/BOSS 표식, 바닥 방향 문양을 한 세트로 사용한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BossArenaEntranceMarker : MonoBehaviour
    {
        private const int GateTextureWidth = 96;
        private const int GateTextureHeight = 112;
        private const float PixelsPerUnit = 16f;

        private static readonly Color VoidColor = new Color32(8, 3, 8, 255);
        private static readonly Color OutlineColor = new Color32(28, 5, 12, 255);
        private static readonly Color FrameColor = new Color32(92, 13, 24, 255);
        private static readonly Color WarningColor = new Color32(255, 53, 20, 255);
        private static readonly Color BoneColor = new Color32(255, 226, 145, 255);

        private static Sprite gateSprite;
        private static Sprite warningDecalSprite;

        private Transform gateBillboard;
        private SpriteRenderer gateShadowRenderer;
        private SpriteRenderer gateRenderer;
        private SpriteRenderer decalShadowRenderer;
        private SpriteRenderer decalRenderer;
        private Vector3 gateBaseScale;
        private Vector3 decalBaseScale;
        private float phase;
        private float approachAmount;
        private float visibility = 1f;
        private bool arenaLocked;
        private bool bossDefeated;
        private bool isApproachSide;

        public int SideIndex { get; private set; }

        public void Configure(int sideIndex, Vector3 inwardDirection, int sortingOrder)
        {
            SideIndex = sideIndex;
            phase = sideIndex * 1.37f;

            CreateGateBillboard(sortingOrder);
            CreateGroundWarning(inwardDirection, sortingOrder);
            ApplyPresentationState();
        }

        public void SetState(float approach, bool locked, bool defeated, bool approachSide)
        {
            approachAmount = Mathf.Clamp01(approach);
            arenaLocked = locked;
            bossDefeated = defeated;
            isApproachSide = approachSide;
        }

        private void Update()
        {
            float targetVisibility = bossDefeated
                ? 0f
                : arenaLocked || isApproachSide ? 1f : 0.08f;
            visibility = Mathf.MoveTowards(visibility, targetVisibility, Time.unscaledDeltaTime * 2.8f);
            ApplyPresentationState();
        }

        private void CreateGateBillboard(int sortingOrder)
        {
            GameObject billboardObject = new GameObject("BossGateBillboard");
            billboardObject.transform.SetParent(transform, false);
            billboardObject.transform.localPosition = Vector3.up * 0.08f;
            gateBillboard = billboardObject.transform;
            gateBaseScale = new Vector3(0.64f, 0.64f, 1f);
            gateBillboard.localScale = gateBaseScale;

            Billboard billboard = billboardObject.AddComponent<Billboard>();
            billboard.Configure(Billboard.BillboardMode.FaceCamera, 0f, Billboard.UpdateMode.Once);

            gateShadowRenderer = CreateLayerRenderer(
                billboardObject.transform,
                "ContrastSilhouette",
                GetGateSprite(),
                sortingOrder,
                new Color(0f, 0f, 0f, 0.94f),
                new Vector3(1.1f, 1.1f, 1f));
            gateRenderer = CreateLayerRenderer(
                billboardObject.transform,
                "BossGate",
                GetGateSprite(),
                sortingOrder + 1,
                Color.white,
                Vector3.one);
        }

        private void CreateGroundWarning(Vector3 inwardDirection, int sortingOrder)
        {
            Vector3 flatInward = new Vector3(inwardDirection.x, 0f, inwardDirection.z).normalized;
            if (flatInward.sqrMagnitude < 0.001f)
            {
                flatInward = Vector3.forward;
            }

            GameObject decalObject = new GameObject("BossApproachWarning");
            decalObject.transform.SetParent(transform, false);
            decalObject.transform.position = transform.position - flatInward * 1.75f + Vector3.up * 0.08f;
            decalObject.transform.rotation = Quaternion.LookRotation(Vector3.up, flatInward);
            decalBaseScale = new Vector3(0.72f, 0.72f, 1f);
            decalObject.transform.localScale = decalBaseScale;

            decalShadowRenderer = CreateLayerRenderer(
                decalObject.transform,
                "WarningShadow",
                GetWarningDecalSprite(),
                sortingOrder - 2,
                new Color(0.02f, 0f, 0.01f, 0.92f),
                new Vector3(1.18f, 1.18f, 1f));
            decalRenderer = CreateLayerRenderer(
                decalObject.transform,
                "WarningSignal",
                GetWarningDecalSprite(),
                sortingOrder - 1,
                new Color(1f, 0.24f, 0.06f, 0.9f),
                Vector3.one);
        }

        private void ApplyPresentationState()
        {
            if (gateBillboard == null)
            {
                return;
            }

            float time = Time.unscaledTime;
            float urgency = Mathf.Max(approachAmount, arenaLocked ? 1f : 0f);
            float pulse = 0.5f + 0.5f * Mathf.Sin(time * Mathf.Lerp(2.4f, 6.4f, urgency) + phase);
            float scalePulse = 1f + pulse * Mathf.Lerp(0.018f, 0.055f, urgency);
            gateBillboard.localScale = gateBaseScale * scalePulse;

            float gateAlpha = visibility * Mathf.Lerp(0.92f, 1f, urgency);
            if (gateRenderer != null)
            {
                gateRenderer.color = new Color(1f, 1f, 1f, gateAlpha);
            }

            if (gateShadowRenderer != null)
            {
                gateShadowRenderer.color = new Color(0f, 0f, 0f, visibility * 0.96f);
            }

            float signalAlpha = visibility * Mathf.Lerp(0.72f, 1f, Mathf.Max(urgency, pulse * 0.55f));
            if (decalRenderer != null)
            {
                decalRenderer.color = new Color(1f, Mathf.Lerp(0.18f, 0.52f, pulse), 0.04f, signalAlpha);
            }

            if (decalShadowRenderer != null)
            {
                decalShadowRenderer.color = new Color(0.02f, 0f, 0.01f, visibility * 0.9f);
            }
        }

        private static SpriteRenderer CreateLayerRenderer(
            Transform parent,
            string objectName,
            Sprite sprite,
            int sortingOrder,
            Color color,
            Vector3 localScale)
        {
            GameObject layer = new GameObject(objectName);
            layer.transform.SetParent(parent, false);
            layer.transform.localScale = localScale;
            SpriteRenderer renderer = layer.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;
            return renderer;
        }

        private static Sprite GetGateSprite()
        {
            if (gateSprite != null)
            {
                return gateSprite;
            }

            Texture2D texture = CreateTexture(GateTextureWidth, GateTextureHeight, "RuntimeBossGateTexture");
            Color32[] pixels = CreateClearPixels(GateTextureWidth * GateTextureHeight);

            DrawRect(pixels, GateTextureWidth, GateTextureHeight, 8, 4, 25, 55, OutlineColor);
            DrawRect(pixels, GateTextureWidth, GateTextureHeight, 63, 4, 25, 55, OutlineColor);
            DrawRect(pixels, GateTextureWidth, GateTextureHeight, 13, 7, 16, 49, FrameColor);
            DrawRect(pixels, GateTextureWidth, GateTextureHeight, 67, 7, 16, 49, FrameColor);
            DrawRect(pixels, GateTextureWidth, GateTextureHeight, 29, 5, 38, 53, VoidColor);

            DrawArch(pixels, GateTextureWidth, GateTextureHeight);
            DrawGateTeeth(pixels, GateTextureWidth, GateTextureHeight);
            DrawSkull(pixels, GateTextureWidth, GateTextureHeight, 48, 77);
            DrawBossPlaque(pixels, GateTextureWidth, GateTextureHeight);

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            gateSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, GateTextureWidth, GateTextureHeight),
                new Vector2(0.5f, 0.035f),
                PixelsPerUnit);
            gateSprite.name = "RuntimeBossEntranceGate";
            return gateSprite;
        }

        private static Sprite GetWarningDecalSprite()
        {
            if (warningDecalSprite != null)
            {
                return warningDecalSprite;
            }

            const int width = 72;
            const int height = 96;
            Texture2D texture = CreateTexture(width, height, "RuntimeBossWarningDecalTexture");
            Color32[] pixels = CreateClearPixels(width * height);

            DrawChevron(pixels, width, height, 36, 8, 18, 4, Color.white);
            DrawChevron(pixels, width, height, 36, 29, 18, 4, Color.white);
            DrawRing(pixels, width, height, 36, 70, 21, 4, Color.white);
            DrawSkullMask(pixels, width, height, 36, 70, Color.white);

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            warningDecalSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit);
            warningDecalSprite.name = "RuntimeBossApproachWarning";
            return warningDecalSprite;
        }

        private static Texture2D CreateTexture(int width, int height, string textureName)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = textureName,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            return texture;
        }

        private static Color32[] CreateClearPixels(int count)
        {
            Color32[] pixels = new Color32[count];
            Color32 clear = new Color32(0, 0, 0, 0);
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = clear;
            }
            return pixels;
        }

        private static void DrawArch(Color32[] pixels, int width, int height)
        {
            for (int y = 44; y < 83; y++)
            {
                for (int x = 5; x < 91; x++)
                {
                    float outerX = (x - 48f) / 43f;
                    float outerY = (y - 44f) / 38f;
                    float innerX = (x - 48f) / 27f;
                    float innerY = (y - 44f) / 25f;
                    float outer = outerX * outerX + outerY * outerY;
                    float inner = innerX * innerX + innerY * innerY;
                    if (outer <= 1f && inner >= 1f)
                    {
                        SetPixel(pixels, width, height, x, y, outer > 0.79f ? OutlineColor : FrameColor);
                    }
                    else if (inner < 1f)
                    {
                        SetPixel(pixels, width, height, x, y, VoidColor);
                    }
                }
            }

            for (int angleIndex = 0; angleIndex <= 12; angleIndex++)
            {
                float angle = Mathf.Lerp(18f, 162f, angleIndex / 12f) * Mathf.Deg2Rad;
                int x = Mathf.RoundToInt(48f + Mathf.Cos(angle) * 34f);
                int y = Mathf.RoundToInt(44f + Mathf.Sin(angle) * 30f);
                DrawRect(pixels, width, height, x - 2, y - 2, 5, 5, WarningColor);
            }
        }

        private static void DrawGateTeeth(Color32[] pixels, int width, int height)
        {
            for (int y = 12; y <= 48; y += 12)
            {
                DrawTriangle(pixels, width, height, 28, y, 7, 6, true, BoneColor);
                DrawTriangle(pixels, width, height, 68, y, 7, 6, false, BoneColor);
            }

            DrawRect(pixels, width, height, 11, 9, 5, 42, WarningColor);
            DrawRect(pixels, width, height, 80, 9, 5, 42, WarningColor);
        }

        private static void DrawSkull(Color32[] pixels, int width, int height, int centerX, int centerY)
        {
            DrawRect(pixels, width, height, centerX - 11, centerY - 9, 22, 15, OutlineColor);
            DrawRect(pixels, width, height, centerX - 8, centerY - 13, 16, 21, BoneColor);
            DrawRect(pixels, width, height, centerX - 11, centerY - 8, 22, 12, BoneColor);
            DrawRect(pixels, width, height, centerX - 6, centerY + 6, 12, 5, BoneColor);
            DrawRect(pixels, width, height, centerX - 7, centerY - 4, 5, 5, OutlineColor);
            DrawRect(pixels, width, height, centerX + 2, centerY - 4, 5, 5, OutlineColor);
            DrawRect(pixels, width, height, centerX - 5, centerY - 3, 2, 2, WarningColor);
            DrawRect(pixels, width, height, centerX + 3, centerY - 3, 2, 2, WarningColor);
            DrawRect(pixels, width, height, centerX - 1, centerY + 2, 3, 4, OutlineColor);
            for (int x = centerX - 5; x <= centerX + 5; x += 5)
            {
                DrawRect(pixels, width, height, x, centerY + 7, 2, 5, OutlineColor);
            }
        }

        private static void DrawBossPlaque(Color32[] pixels, int width, int height)
        {
            DrawRect(pixels, width, height, 20, 27, 56, 18, OutlineColor);
            DrawRect(pixels, width, height, 23, 30, 50, 12, FrameColor);

            string[] glyphs =
            {
                "11110|10001|10001|11110|10001|10001|11110",
                "01110|10001|10001|10001|10001|10001|01110",
                "01111|10000|10000|01110|00001|00001|11110",
                "01111|10000|10000|01110|00001|00001|11110"
            };
            int scale = 1;
            int glyphWidth = 5;
            int spacing = 3;
            int totalWidth = glyphs.Length * glyphWidth * scale + (glyphs.Length - 1) * spacing;
            int startX = (width - totalWidth) / 2;
            for (int glyphIndex = 0; glyphIndex < glyphs.Length; glyphIndex++)
            {
                string[] rows = glyphs[glyphIndex].Split('|');
                for (int row = 0; row < rows.Length; row++)
                {
                    for (int column = 0; column < rows[row].Length; column++)
                    {
                        if (rows[row][column] != '1')
                        {
                            continue;
                        }
                        DrawRect(
                            pixels,
                            width,
                            height,
                            startX + glyphIndex * (glyphWidth * scale + spacing) + column * scale,
                            33 + (rows.Length - 1 - row) * scale,
                            scale,
                            scale,
                            BoneColor);
                    }
                }
            }
        }

        private static void DrawSkullMask(Color32[] pixels, int width, int height, int centerX, int centerY, Color color)
        {
            Color32 value = color;
            DrawRect(pixels, width, height, centerX - 9, centerY - 10, 18, 15, value);
            DrawRect(pixels, width, height, centerX - 6, centerY + 4, 12, 7, value);
            DrawRect(pixels, width, height, centerX - 7, centerY - 4, 4, 5, new Color32(0, 0, 0, 0));
            DrawRect(pixels, width, height, centerX + 3, centerY - 4, 4, 5, new Color32(0, 0, 0, 0));
            DrawRect(pixels, width, height, centerX - 1, centerY + 1, 3, 4, new Color32(0, 0, 0, 0));
        }

        private static void DrawRing(
            Color32[] pixels,
            int width,
            int height,
            int centerX,
            int centerY,
            int radius,
            int thickness,
            Color color)
        {
            float outer = radius * radius;
            float innerRadius = Mathf.Max(0f, radius - thickness);
            float inner = innerRadius * innerRadius;
            Color32 value = color;
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                for (int x = centerX - radius; x <= centerX + radius; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distance = dx * dx + dy * dy;
                    if (distance <= outer && distance >= inner)
                    {
                        SetPixel(pixels, width, height, x, y, value);
                    }
                }
            }
        }

        private static void DrawChevron(
            Color32[] pixels,
            int width,
            int height,
            int centerX,
            int baseY,
            int armLength,
            int thickness,
            Color color)
        {
            Color32 value = color;
            for (int step = 0; step <= armLength; step++)
            {
                int y = baseY + armLength - step;
                for (int offset = -thickness / 2; offset <= thickness / 2; offset++)
                {
                    SetPixel(pixels, width, height, centerX - step + offset, y, value);
                    SetPixel(pixels, width, height, centerX + step + offset, y, value);
                }
            }
        }

        private static void DrawTriangle(
            Color32[] pixels,
            int width,
            int height,
            int tipX,
            int centerY,
            int triangleWidth,
            int triangleHeight,
            bool pointsRight,
            Color color)
        {
            Color32 value = color;
            for (int row = 0; row < triangleHeight; row++)
            {
                int span = Mathf.Max(1, triangleWidth * (triangleHeight - row) / triangleHeight);
                int x = pointsRight ? tipX - row : tipX + row;
                DrawRect(pixels, width, height, x, centerY - span / 2, 1, span, value);
            }
        }

        private static void DrawRect(
            Color32[] pixels,
            int width,
            int height,
            int x,
            int y,
            int rectWidth,
            int rectHeight,
            Color32 color)
        {
            for (int py = y; py < y + rectHeight; py++)
            {
                for (int px = x; px < x + rectWidth; px++)
                {
                    SetPixel(pixels, width, height, px, py, color);
                }
            }
        }

        private static void SetPixel(Color32[] pixels, int width, int height, int x, int y, Color32 color)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return;
            }
            pixels[y * width + x] = color;
        }
    }
}
