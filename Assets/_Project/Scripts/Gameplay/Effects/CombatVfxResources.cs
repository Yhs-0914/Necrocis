using UnityEngine;

namespace Necrocis
{
    internal static class CombatVfxResources
    {
        private static Sprite softCircleSprite;
        private static Sprite ringSprite;
        private static Sprite starSprite;
        private static Sprite chevronSprite;
        private static Sprite slashSprite;
        private static Sprite runeSprite;
        private static Sprite arrowSprite;
        private static Sprite crackSprite;
        private static Sprite splatSprite;
        private static Sprite bloodDropSprite;
        private static Sprite fangSprite;
        private static Sprite windArcSprite;
        private static Sprite vignetteSprite;
        private static Texture2D particleTexture;
        private static Material particleMaterial;
        private static Material mistMaterial;
        private static Material lineMaterial;

        public static Sprite GetSoftCircleSprite()
        {
            if (softCircleSprite != null)
            {
                return softCircleSprite;
            }

            const int size = 64;
            Texture2D texture = CreateTexture(size, "CombatVfxSoftCircle");
            float center = (size - 1) * 0.5f;
            float radius = center;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / radius;
                    float alpha = Mathf.Clamp01(1f - distance);
                    alpha = alpha * alpha * (3f - 2f * alpha);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            softCircleSprite = CreateSprite(texture, "CombatVfxSoftCircleSprite");
            return softCircleSprite;
        }

        public static Sprite GetRingSprite()
        {
            if (ringSprite != null)
            {
                return ringSprite;
            }

            const int size = 96;
            Texture2D texture = CreateTexture(size, "CombatVfxRing");
            float center = (size - 1) * 0.5f;
            float radius = center;
            const float ringCenter = 0.72f;
            const float halfWidth = 0.1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / radius;
                    float ringDistance = Mathf.Abs(distance - ringCenter);
                    float alpha = 1f - Mathf.Clamp01(ringDistance / halfWidth);
                    alpha *= Mathf.Clamp01((1f - distance) * 8f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            ringSprite = CreateSprite(texture, "CombatVfxRingSprite");
            return ringSprite;
        }

        public static Sprite GetStarSprite()
        {
            if (starSprite != null)
            {
                return starSprite;
            }

            const int size = 64;
            Texture2D texture = CreateTexture(size, "CombatVfxStar");
            texture.filterMode = FilterMode.Point;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = PixelToNormalized(x, y, size);
                    float ax = Mathf.Abs(p.x);
                    float ay = Mathf.Abs(p.y);
                    float verticalWidth = Mathf.Lerp(0.34f, 0.025f, Mathf.Clamp01(ay));
                    float horizontalWidth = Mathf.Lerp(0.34f, 0.025f, Mathf.Clamp01(ax));
                    float vertical = Mathf.Clamp01((verticalWidth - ax) * 24f) * Mathf.Clamp01((1f - ay) * 8f);
                    float horizontal = Mathf.Clamp01((horizontalWidth - ay) * 24f) * Mathf.Clamp01((1f - ax) * 8f);
                    float diamond = Mathf.Clamp01((0.38f - ax - ay) * 12f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Max(diamond, Mathf.Max(vertical, horizontal))));
                }
            }

            texture.Apply(false, true);
            starSprite = CreateSprite(texture, "CombatVfxStarSprite");
            return starSprite;
        }

        public static Sprite GetChevronSprite()
        {
            if (chevronSprite != null)
            {
                return chevronSprite;
            }

            const int size = 64;
            Texture2D texture = CreateTexture(size, "CombatVfxChevron");
            texture.filterMode = FilterMode.Point;
            Vector2 left = new Vector2(-0.72f, -0.28f);
            Vector2 tip = new Vector2(0f, 0.42f);
            Vector2 right = new Vector2(0.72f, -0.28f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = PixelToNormalized(x, y, size);
                    float distance = Mathf.Min(
                        DistanceToSegment(p, left, tip),
                        DistanceToSegment(p, tip, right));
                    float alpha = Mathf.Clamp01((0.13f - distance) * 24f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            chevronSprite = CreateSprite(texture, "CombatVfxChevronSprite");
            return chevronSprite;
        }

        public static Sprite GetSlashSprite()
        {
            if (slashSprite != null)
            {
                return slashSprite;
            }

            const int size = 96;
            Texture2D texture = CreateTexture(size, "CombatVfxSlash");
            texture.filterMode = FilterMode.Point;
            Vector2[] curve =
            {
                new Vector2(-0.82f, -0.58f),
                new Vector2(-0.46f, -0.18f),
                new Vector2(0.02f, 0.2f),
                new Vector2(0.48f, 0.48f),
                new Vector2(0.82f, 0.62f)
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = PixelToNormalized(x, y, size);
                    float distance = float.MaxValue;
                    for (int i = 0; i < curve.Length - 1; i++)
                    {
                        distance = Mathf.Min(distance, DistanceToSegment(p, curve[i], curve[i + 1]));
                    }

                    float taper = Mathf.Clamp01((0.94f - Mathf.Abs(p.x)) * 5f);
                    float alpha = Mathf.Clamp01((0.105f * taper - distance) * 28f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            slashSprite = CreateSprite(texture, "CombatVfxSlashSprite");
            return slashSprite;
        }

        public static Sprite GetRuneSprite()
        {
            if (runeSprite != null)
            {
                return runeSprite;
            }

            const int size = 96;
            Texture2D texture = CreateTexture(size, "CombatVfxRune");
            texture.filterMode = FilterMode.Point;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = PixelToNormalized(x, y, size);
                    float radius = p.magnitude;
                    float ring = Mathf.Clamp01((0.055f - Mathf.Abs(radius - 0.68f)) * 26f);
                    float diamond = Mathf.Clamp01((0.49f - Mathf.Abs(p.x) - Mathf.Abs(p.y)) * 22f)
                        * Mathf.Clamp01((Mathf.Abs(p.x) + Mathf.Abs(p.y) - 0.42f) * 22f);
                    float ticks = 0f;
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = i * Mathf.PI * 0.25f;
                        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                        Vector2 tangent = new Vector2(-direction.y, direction.x);
                        float along = Vector2.Dot(p, direction);
                        float across = Mathf.Abs(Vector2.Dot(p, tangent));
                        float tick = Mathf.Clamp01((0.045f - across) * 30f)
                            * Mathf.Clamp01((along - 0.73f) * 20f)
                            * Mathf.Clamp01((0.92f - along) * 20f);
                        ticks = Mathf.Max(ticks, tick);
                    }
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Max(ring, Mathf.Max(diamond, ticks))));
                }
            }

            texture.Apply(false, true);
            runeSprite = CreateSprite(texture, "CombatVfxRuneSprite");
            return runeSprite;
        }

        public static Sprite GetArrowSprite()
        {
            if (arrowSprite != null)
            {
                return arrowSprite;
            }

            const int size = 64;
            Texture2D texture = CreateTexture(size, "CombatVfxArrow");
            texture.filterMode = FilterMode.Point;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = PixelToNormalized(x, y, size);
                    float stem = Mathf.Clamp01((0.075f - Mathf.Abs(p.x)) * 30f)
                        * Mathf.Clamp01((p.y + 0.82f) * 10f)
                        * Mathf.Clamp01((0.35f - p.y) * 10f);
                    float headWidth = Mathf.Clamp01((0.72f - p.y) * 2.7f);
                    float head = Mathf.Clamp01((headWidth - Mathf.Abs(p.x)) * 18f)
                        * Mathf.Clamp01((p.y - 0.04f) * 9f)
                        * Mathf.Clamp01((0.82f - p.y) * 10f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Max(stem, head)));
                }
            }

            texture.Apply(false, true);
            arrowSprite = CreateSprite(texture, "CombatVfxArrowSprite");
            return arrowSprite;
        }

        public static Sprite GetCrackSprite()
        {
            if (crackSprite != null)
            {
                return crackSprite;
            }

            const int size = 96;
            Texture2D texture = CreateTexture(size, "CombatVfxCrack");
            texture.filterMode = FilterMode.Point;
            Vector2[][] branches =
            {
                new[] { Vector2.zero, new Vector2(0.14f, 0.2f), new Vector2(0.08f, 0.48f), new Vector2(0.34f, 0.86f) },
                new[] { Vector2.zero, new Vector2(-0.22f, 0.12f), new Vector2(-0.38f, 0.4f), new Vector2(-0.78f, 0.58f) },
                new[] { Vector2.zero, new Vector2(0.18f, -0.18f), new Vector2(0.5f, -0.3f), new Vector2(0.82f, -0.62f) },
                new[] { Vector2.zero, new Vector2(-0.1f, -0.24f), new Vector2(-0.04f, -0.54f), new Vector2(-0.28f, -0.88f) },
                new[] { new Vector2(-0.38f, 0.4f), new Vector2(-0.3f, 0.68f), new Vector2(-0.46f, 0.86f) },
                new[] { new Vector2(0.5f, -0.3f), new Vector2(0.64f, -0.08f), new Vector2(0.9f, 0.02f) }
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = PixelToNormalized(x, y, size);
                    float distance = float.MaxValue;
                    for (int branch = 0; branch < branches.Length; branch++)
                    {
                        Vector2[] points = branches[branch];
                        for (int i = 0; i < points.Length - 1; i++)
                        {
                            distance = Mathf.Min(distance, DistanceToSegment(p, points[i], points[i + 1]));
                        }
                    }
                    float alpha = Mathf.Clamp01((0.045f - distance) * 35f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            crackSprite = CreateSprite(texture, "CombatVfxCrackSprite");
            return crackSprite;
        }

        public static Sprite GetSplatSprite()
        {
            if (splatSprite != null)
            {
                return splatSprite;
            }

            const int size = 96;
            Texture2D texture = CreateTexture(size, "CombatVfxSplat");
            texture.filterMode = FilterMode.Point;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = PixelToNormalized(x, y, size);
                    float angle = Mathf.Atan2(p.y, p.x);
                    float boundary = 0.58f
                        + Mathf.Sin(angle * 5f + 0.4f) * 0.1f
                        + Mathf.Sin(angle * 9f - 0.7f) * 0.055f;
                    float body = Mathf.Clamp01((boundary - p.magnitude) * 18f);
                    float centerHole = Mathf.Clamp01((p.magnitude - 0.16f) * 18f);
                    float alpha = Mathf.Max(body * 0.62f, body * centerHole);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            splatSprite = CreateSprite(texture, "CombatVfxSplatSprite");
            return splatSprite;
        }

        public static Sprite GetBloodDropSprite()
        {
            if (bloodDropSprite != null)
            {
                return bloodDropSprite;
            }

            const int size = 64;
            Texture2D texture = CreateTexture(size, "CombatVfxBloodDrop");
            texture.filterMode = FilterMode.Point;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = PixelToNormalized(x, y, size);
                    float bulb = Mathf.Clamp01((0.48f - Vector2.Distance(p, new Vector2(0f, -0.3f))) * 18f);
                    float taperWidth = Mathf.Clamp01((0.86f - p.y) / 1.1f) * 0.34f;
                    float taper = Mathf.Clamp01((taperWidth - Mathf.Abs(p.x)) * 22f)
                        * Mathf.Clamp01((p.y + 0.28f) * 8f)
                        * Mathf.Clamp01((0.88f - p.y) * 12f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Max(bulb, taper)));
                }
            }

            texture.Apply(false, true);
            bloodDropSprite = CreateSprite(texture, "CombatVfxBloodDropSprite");
            return bloodDropSprite;
        }

        public static Sprite GetFangSprite()
        {
            if (fangSprite != null)
            {
                return fangSprite;
            }

            const int size = 64;
            Texture2D texture = CreateTexture(size, "CombatVfxFang");
            texture.filterMode = FilterMode.Point;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = PixelToNormalized(x, y, size);
                    float normalizedY = Mathf.Clamp01((p.y + 0.78f) / 1.56f);
                    float centerCurve = Mathf.Sin(normalizedY * Mathf.PI) * 0.1f;
                    float width = Mathf.Lerp(0.055f, 0.36f, normalizedY);
                    float body = Mathf.Clamp01((width - Mathf.Abs(p.x - centerCurve)) * 24f)
                        * Mathf.Clamp01((p.y + 0.82f) * 12f)
                        * Mathf.Clamp01((0.82f - p.y) * 12f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, body));
                }
            }

            texture.Apply(false, true);
            fangSprite = CreateSprite(texture, "CombatVfxFangSprite");
            return fangSprite;
        }

        public static Sprite GetWindArcSprite()
        {
            if (windArcSprite != null)
            {
                return windArcSprite;
            }

            const int size = 96;
            Texture2D texture = CreateTexture(size, "CombatVfxWindArc");
            texture.filterMode = FilterMode.Point;
            Vector2 tailA = new Vector2(-0.76f, -0.2f);
            Vector2 tailB = new Vector2(-0.28f, -0.1f);
            Vector2 tailC = new Vector2(0.18f, -0.34f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = PixelToNormalized(x, y, size);
                    float radius = p.magnitude;
                    float angle = Mathf.Atan2(p.y, p.x);
                    float ring = Mathf.Clamp01((0.07f - Mathf.Abs(radius - 0.63f)) * 22f);
                    float angularMask = Mathf.Clamp01((angle + 2.55f) * 5f)
                        * Mathf.Clamp01((2.2f - angle) * 5f);
                    float tail = Mathf.Clamp01((0.065f - Mathf.Min(
                        DistanceToSegment(p, tailA, tailB),
                        DistanceToSegment(p, tailB, tailC))) * 24f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Max(ring * angularMask, tail)));
                }
            }

            texture.Apply(false, true);
            windArcSprite = CreateSprite(texture, "CombatVfxWindArcSprite");
            return windArcSprite;
        }

        public static Sprite GetVignetteSprite()
        {
            if (vignetteSprite != null)
            {
                return vignetteSprite;
            }

            const int size = 128;
            Texture2D texture = CreateTexture(size, "CombatDamageVignette");
            float center = (size - 1) * 0.5f;
            Vector2 inverseRadius = new Vector2(1f / center, 1f / center);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 normalized = Vector2.Scale(
                        new Vector2(Mathf.Abs(x - center), Mathf.Abs(y - center)),
                        inverseRadius);
                    float edge = Mathf.Max(normalized.x, normalized.y);
                    float radial = normalized.magnitude * 0.72f;
                    float alpha = Mathf.SmoothStep(0f, 1f, Mathf.Max(edge, radial) - 0.3f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            vignetteSprite = CreateSprite(texture, "CombatDamageVignetteSprite", 100f);
            return vignetteSprite;
        }

        public static Material GetParticleMaterial()
        {
            if (particleMaterial != null)
            {
                return particleMaterial;
            }

            if (particleTexture == null)
            {
                particleTexture = CreateDiamondTexture();
            }

            particleMaterial = CreateUnlitMaterial("CombatVfxParticleMaterial", particleTexture);
            return particleMaterial;
        }

        public static Material GetLineMaterial()
        {
            if (lineMaterial == null)
            {
                lineMaterial = CreateUnlitMaterial("CombatVfxLineMaterial", Texture2D.whiteTexture);
            }

            return lineMaterial;
        }

        public static Material GetMistMaterial()
        {
            if (mistMaterial != null)
            {
                return mistMaterial;
            }

            Material source = GetLineMaterial();
            if (source == null)
            {
                return null;
            }

            mistMaterial = new Material(source)
            {
                name = "CombatVfxMistMaterial",
                mainTexture = GetSoftCircleSprite().texture,
                hideFlags = HideFlags.HideAndDontSave
            };
            return mistMaterial;
        }

        private static Texture2D CreateDiamondTexture()
        {
            const int size = 32;
            Texture2D texture = CreateTexture(size, "CombatVfxParticle");
            float center = (size - 1) * 0.5f;
            float radius = center;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = (Mathf.Abs(x - center) + Mathf.Abs(y - center)) / radius;
                    float alpha = Mathf.Clamp01(1f - distance * 0.62f);
                    alpha *= alpha;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            return texture;
        }

        private static Material CreateUnlitMaterial(string materialName, Texture texture)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            }

            if (shader == null)
            {
                return null;
            }

            Material material = new Material(shader)
            {
                name = materialName,
                mainTexture = texture,
                hideFlags = HideFlags.HideAndDontSave
            };
            return material;
        }

        private static Texture2D CreateTexture(int size, string textureName)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = textureName,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            return texture;
        }

        private static Sprite CreateSprite(Texture2D texture, string spriteName, float pixelsPerUnit = 64f)
        {
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit);
            sprite.name = spriteName;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        private static Vector2 PixelToNormalized(int x, int y, int size)
        {
            float center = (size - 1) * 0.5f;
            return new Vector2((x - center) / center, (y - center) / center);
        }

        private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= 0.000001f)
            {
                return Vector2.Distance(point, start);
            }

            float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
            return Vector2.Distance(point, start + segment * t);
        }
    }
}
