using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap.Pooling
{
    /// <summary>Prefab마다 별도의 Queue를 유지하는 범용 오브젝트 풀.</summary>
    public sealed class ObjectPoolManager : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maximumInactivePerPrefab = 512;

        private readonly Dictionary<GameObject, Queue<GameObject>> pools =
            new Dictionary<GameObject, Queue<GameObject>>();

        public static ObjectPoolManager GetOrCreate()
        {
            ObjectPoolManager existing = FindFirstObjectByType<ObjectPoolManager>();
            if (existing) return existing;

            GameObject root = new GameObject("Object Pool Manager");
            return root.AddComponent<ObjectPoolManager>();
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!prefab) return null;
            Queue<GameObject> pool = GetPool(prefab);
            GameObject instance = null;
            while (pool.Count > 0 && !instance) instance = pool.Dequeue();

            if (!instance)
            {
                instance = Instantiate(prefab);
                PooledObjectIdentity newIdentity = instance.GetComponent<PooledObjectIdentity>();
                if (!newIdentity) newIdentity = instance.AddComponent<PooledObjectIdentity>();
                newIdentity.Initialize(prefab);
            }

            PooledObjectIdentity identity = instance.GetComponent<PooledObjectIdentity>();
            identity.IsInPool = false;
            instance.transform.SetParent(parent, false);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.transform.localScale = identity.OriginalScale;
            instance.SetActive(true);
            NotifyTaken(instance);
            return instance;
        }

        public void Release(GameObject instance)
        {
            if (!instance) return;
            PooledObjectIdentity identity = instance.GetComponent<PooledObjectIdentity>();
            if (!identity || !identity.SourcePrefab)
            {
                Destroy(instance);
                return;
            }
            if (identity.IsInPool) return;

            NotifyReturned(instance);
            identity.IsInPool = true;
            instance.SetActive(false);
            instance.transform.SetParent(transform, false);

            Queue<GameObject> pool = GetPool(identity.SourcePrefab);
            if (pool.Count >= maximumInactivePerPrefab)
            {
                Destroy(instance);
                return;
            }
            pool.Enqueue(instance);
        }

        public void Prewarm(GameObject prefab, int count)
        {
            if (!prefab || count <= 0) return;
            var created = new List<GameObject>(count);
            for (int i = 0; i < count; i++) created.Add(Get(prefab, Vector3.zero, Quaternion.identity));
            for (int i = 0; i < created.Count; i++) Release(created[i]);
        }

        private Queue<GameObject> GetPool(GameObject prefab)
        {
            if (!pools.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                pools.Add(prefab, pool);
            }
            return pool;
        }

        private static void NotifyTaken(GameObject instance)
        {
            MonoBehaviour[] behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
                if (behaviours[i] is IPoolable poolable) poolable.OnTakenFromPool();
        }

        private static void NotifyReturned(GameObject instance)
        {
            MonoBehaviour[] behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
                if (behaviours[i] is IPoolable poolable) poolable.OnReturnedToPool();
        }
    }
}
