using System.Collections.Generic;
using UnityEngine;
namespace Necrocis
{
    public static class TextureSpriteCache
    {
        private static Sprite circleSprite;
        private static Material spriteMaterial;
        private static readonly Dictionary<string, Sprite> ResourceSprites = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Material> ResourceSpriteMaterials = new Dictionary<string, Material>();

        public static Sprite LoadResourceSprite(string resourcePath, float pixelsPerUnit = 100f)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return null;
            }

            if (ResourceSprites.TryGetValue(resourcePath, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null)
            {
                Texture2D texture = Resources.Load<Texture2D>(resourcePath);
                if (texture != null)
                {
                    sprite = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        Mathf.Max(1f, pixelsPerUnit));
                    sprite.name = texture.name;
                }
            }

            ResourceSprites[resourcePath] = sprite;
            return sprite;
        }

        public static float GetUniformScaleForWorldSize(Sprite sprite, float targetWorldSize)
        {
            float spriteSize = sprite != null
                ? Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y)
                : 1f;
            return Mathf.Max(0.01f, targetWorldSize) / Mathf.Max(0.0001f, spriteSize);
        }

        public static Material GetResourceSpriteMaterial(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return GetSpriteMaterial();
            }

            if (ResourceSpriteMaterials.TryGetValue(resourcePath, out Material cachedMaterial))
            {
                return cachedMaterial;
            }

            Sprite sprite = LoadResourceSprite(resourcePath);
            Material baseMaterial = GetSpriteMaterial();
            if (sprite == null || baseMaterial == null)
            {
                return baseMaterial;
            }

            Material material = new Material(baseMaterial)
            {
                name = $"Runtime_{sprite.name}_Material",
                mainTexture = sprite.texture
            };
            ResourceSpriteMaterials[resourcePath] = material;
            return material;
        }

        public static Sprite GetCircleSprite()
        {
            if (circleSprite != null)
            {
                return circleSprite;
            }

            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            float center = (size - 1) * 0.5f;
            float radius = center - 1f;
            float innerRadius = radius * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    if (distance > radius)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    if (distance <= innerRadius)
                    {
                        texture.SetPixel(x, y, Color.white);
                        continue;
                    }

                    float alpha = Mathf.InverseLerp(radius, innerRadius, distance);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            circleSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size);
            circleSprite.name = "RuntimeCircleSprite";
            return circleSprite;
        }

        public static Material GetSpriteMaterial()
        {
            if (spriteMaterial != null)
            {
                return spriteMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }

            if (shader == null)
            {
                return null;
            }

            spriteMaterial = new Material(shader)
            {
                name = "RuntimeSpriteMaterial"
            };
            return spriteMaterial;
        }
    }
}
