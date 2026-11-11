using System;
using UnityEngine;

namespace Necrocis
{
    internal static class ItemEffectVisuals
    {
        private static readonly Func<GameObject> CreateCircleVisualFunc = CreateCircleVisualObject;
        internal static void SpawnPooledCircleVisual(
            string poolName,
            string objectName,
            Vector3 position,
            float scale,
            Color color,
            int sortingOrder,
            float duration,
            Sprite effectSprite = null)
        {
            GameObject visualObject = RuntimePool.Acquire(poolName, CreateCircleVisualFunc);
            if (visualObject == null)
            {
                return;
            }

            visualObject.name = objectName;
            visualObject.transform.position = position;

            SpriteRenderer renderer = GetOrAddSpriteRenderer(visualObject);
            if (renderer == null)
            {
                RuntimePool.Release(visualObject);
                return;
            }

            renderer.enabled = true;
            renderer.sprite = effectSprite != null ? effectSprite : TextureSpriteCache.GetCircleSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            visualObject.transform.localScale = Vector3.one * TextureSpriteCache.GetUniformScaleForWorldSize(
                renderer.sprite,
                Mathf.Max(0.01f, scale));

            RuntimePool.EnsureAutoReturn(visualObject)?.Schedule(Mathf.Max(0.02f, duration));
        }

        private static GameObject CreateCircleVisualObject()
        {
            GameObject obj = new GameObject("PlayerItemCircleVisual");
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = TextureSpriteCache.GetCircleSprite();
            return obj;
        }

        internal static SpriteRenderer GetOrAddSpriteRenderer(GameObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = obj.AddComponent<SpriteRenderer>();
            }

            return renderer;
        }
    }
}
