using UnityEngine;

namespace Necrocis
{
    internal static class CombatVfxResources
    {
        private static Sprite softCircleSprite;
        private static Sprite ringSprite;
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
    }
}
