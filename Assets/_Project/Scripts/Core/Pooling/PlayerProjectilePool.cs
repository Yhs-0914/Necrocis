using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// Shared projectile object pool.
    /// </summary>
    public class PlayerProjectilePool : MonoBehaviour
    {
        public static PlayerProjectilePool Instance; // 싱글톤 (씬에 하나만 존재)

        [SerializeField] private GameObject objectToPool;  // 풀링할 프리팹
        [SerializeField] private int amountToPool = 10;    // 초기 풀 크기

        private List<GameObject> pooledObjects; // 풀에 생성된 오브젝트 목록
        private bool initialized;
        private string lastInitializationError;

        public GameObject PooledPrefab => objectToPool;
        public bool IsPrefabAssigned => objectToPool != null;
        public int PooledCount => pooledObjects != null ? pooledObjects.Count : 0;

        public int InactiveCount
        {
            get
            {
                if (pooledObjects == null)
                {
                    return 0;
                }

                int count = 0;
                for (int i = 0; i < pooledObjects.Count; i++)
                {
                    GameObject pooled = pooledObjects[i];
                    if (pooled != null && !pooled.activeInHierarchy)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }

        // 시작 시 프리팹을 미리 생성하여 비활성화 상태로 풀에 보관
        private void Start()
        {
            if (!initialized)
            {
                InitializePool();
            }
        }

        private void OnEnable()
        {
            if (Instance == this && !initialized)
            {
                InitializePool();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private bool InitializePool()
        {
            if (initialized)
            {
                return true;
            }

            if (objectToPool == null)
            {
                lastInitializationError = "objectToPool prefab is missing";
                Debug.LogError("[PlayerProjectilePool] objectToPool is not assigned.");
                return false;
            }

            if (amountToPool <= 0)
            {
                lastInitializationError = "amountToPool must be greater than 0";
                Debug.LogError($"[PlayerProjectilePool] amountToPool must be greater than 0. Current: {amountToPool}");
                return false;
            }

            if (pooledObjects == null)
            {
                pooledObjects = new List<GameObject>(amountToPool);
            }
            else
            {
                pooledObjects.Clear();
            }

            for (int i = 0; i < amountToPool; i++)
            {
                GameObject obj = Instantiate(objectToPool, transform);
                obj.name = $"{objectToPool.name}_Pooled_{i}";
                obj.SetActive(false);
                pooledObjects.Add(obj);
            }

            initialized = true;
            lastInitializationError = null;
            Debug.Log($"[PlayerProjectilePool] Initialized pool with {pooledObjects.Count} bullets.");
            return true;
        }

        // 비활성화된 오브젝트를 찾아 반환 (없으면 null)
        // 사용 후 SetActive(false)로 반환하면 재사용 가능
        public GameObject GetPooledObject()
        {
            if (!initialized && !InitializePool())
            {
                Debug.LogWarning($"[PlayerProjectilePool] GetPooledObject failed: {GetDebugStatus()}");
                return null;
            }

            if (pooledObjects == null)
            {
                Debug.LogWarning("[PlayerProjectilePool] pooledObjects is null.");
                return null;
            }

            if (pooledObjects.Count == 0)
            {
                Debug.LogWarning("[PlayerProjectilePool] pooledObjects is empty.");
                return null;
            }

            for (int i = 0; i < pooledObjects.Count; i++)
            {
                GameObject pooled = pooledObjects[i];

                // Destroyed references can remain in the list if something removed a pooled object.
                // Keep this guard so activeInHierarchy never throws MissingReferenceException.
                if (pooled == null)
                {
                    continue;
                }

                if (!pooled.activeInHierarchy)
                {
                    return pooled;
                }
            }

            Debug.LogWarning("[PlayerProjectilePool] no inactive pooled projectile available.");
            return null; // 모든 오브젝트가 사용 중
        }

        public string GetDebugStatus()
        {
            if (objectToPool == null)
            {
                return "objectToPool prefab is missing";
            }

            if (!initialized)
            {
                return string.IsNullOrEmpty(lastInitializationError)
                    ? "pool is not initialized"
                    : lastInitializationError;
            }

            if (pooledObjects == null)
            {
                return "pooledObjects is null";
            }

            if (pooledObjects.Count == 0)
            {
                return "pooledObjects is empty";
            }

            if (InactiveCount <= 0)
            {
                return "no inactive pooled projectile available";
            }

            return "pool is ready";
        }
    }
}
