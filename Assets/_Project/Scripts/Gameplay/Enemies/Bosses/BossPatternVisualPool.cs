using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    internal interface IBossPatternTempSpriteOwner
    {
        void ReleaseTempSprite(GameObject obj);
    }

    internal static class BossPatternVisualPool
    {
        private const string PoolName = "BossPattern.TempSprite";
        private static readonly Func<GameObject> CreateFunc = CreateObject;

        public static GameObject Acquire(
            string objectName,
            Sprite sprite,
            Color color,
            Vector3 position,
            float scale,
            int sortingOrder,
            IBossPatternTempSpriteOwner owner,
            List<GameObject> activeObjects)
        {
            GameObject obj = RuntimePool.Acquire(PoolName, CreateFunc);
            if (obj == null)
            {
                return null;
            }

            obj.name = objectName;
            obj.transform.position = position;
            obj.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);

            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = obj.AddComponent<SpriteRenderer>();
            }

            renderer.enabled = true;
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            BossPatternTempSprite tempSprite = obj.GetComponent<BossPatternTempSprite>();
            if (tempSprite == null)
            {
                tempSprite = obj.AddComponent<BossPatternTempSprite>();
            }

            tempSprite.Initialize(owner);
            activeObjects?.Add(obj);

            Billboard billboard = obj.GetComponent<Billboard>();
            if (billboard == null)
            {
                billboard = obj.AddComponent<Billboard>();
            }

            billboard.ResetBaseLocalPosition(obj.transform.localPosition);
            billboard.SetUpdateMode(Billboard.UpdateMode.Once);
            return obj;
        }

        public static void Release(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            RuntimePool.Release(obj);
        }

        private static GameObject CreateObject()
        {
            GameObject obj = new GameObject("BossPatternTempSprite");
            obj.AddComponent<SpriteRenderer>();
            obj.AddComponent<Billboard>();
            obj.AddComponent<BossPatternTempSprite>();
            return obj;
        }
    }

    internal sealed class BossPatternTempSprite : MonoBehaviour
    {
        private IBossPatternTempSpriteOwner owner;

        public void Initialize(IBossPatternTempSpriteOwner owner)
        {
            this.owner = owner;
        }

        public void Release()
        {
            if (owner != null)
            {
                owner.ReleaseTempSprite(gameObject);
                return;
            }

            BossPatternVisualPool.Release(gameObject);
        }

        private void OnDisable()
        {
            owner = null;
        }
    }
}
