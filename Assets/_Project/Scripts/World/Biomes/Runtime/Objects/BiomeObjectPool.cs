using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Necrocis
{
    public abstract partial class BiomeManager
    {

        protected GameObject GetPooledObject(ObjectPoolKey poolKey, System.Func<GameObject> createFunc)
        {
            if (!objectPool.TryGetValue(poolKey, out Stack<GameObject> stack))
            {
                stack = new Stack<GameObject>();
                objectPool[poolKey] = stack;
            }

            while (stack.Count > 0)
            {
                GameObject obj = stack.Pop();
                pooledObjectCount--;
                if (obj != null)
                {
                    Log($"[BiomePool] 재사용: type={poolKey.kind} archetype={poolKey.archetypeId} name={obj.name}");
                    return obj;
                }
            }

            GameObject created = createFunc();
            Log($"[BiomePool] 생성: type={poolKey.kind} archetype={poolKey.archetypeId} name={created.name}");
            return created;
        }


        protected void ReleasePooledObject(ObjectPoolKey poolKey, GameObject obj)
        {
            if (obj == null) return;

            if (!objectPool.TryGetValue(poolKey, out Stack<GameObject> stack))
            {
                stack = new Stack<GameObject>();
                objectPool[poolKey] = stack;
            }

            int maxSize = GetPoolLimit(poolKey.kind);
            if (maxSize <= 0 || stack.Count >= maxSize)
            {
                DestroyPooledObject(obj);
                return;
            }

            if (maxTotalPoolSize > 0 && pooledObjectCount >= maxTotalPoolSize)
            {
                DestroyPooledObject(obj);
                return;
            }

            obj.SetActive(false);
            obj.transform.SetParent(pooledObjectsParent, false);
            stack.Push(obj);
            pooledObjectCount++;
            Log($"[BiomePool] 반환: type={poolKey.kind} archetype={poolKey.archetypeId} name={obj.name}");
        }


        private void BuildPoolLimitLookup()
        {
            if (poolLimitLookup == null)
            {
                poolLimitLookup = new Dictionary<BiomeObjectKind, int>();
            }
            else
            {
                poolLimitLookup.Clear();
            }

            foreach (var limit in poolLimits)
            {
                if (limit.maxSize >= 0)
                {
                    poolLimitLookup[limit.type] = limit.maxSize;
                }
            }
        }


        private int GetPoolLimit(BiomeObjectKind poolKey)
        {
            if (poolLimitLookup != null && poolLimitLookup.TryGetValue(poolKey, out int maxSize))
            {
                return maxSize;
            }
            return defaultMaxPoolSizePerType;
        }


        private void DestroyPooledObject(GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);
            if (pooledObjectsParent != null)
            {
                obj.transform.SetParent(pooledObjectsParent, false);
            }

            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else
            {
                DestroyImmediate(obj);
            }
        }

    }
}
