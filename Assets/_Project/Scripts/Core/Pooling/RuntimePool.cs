using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// Runtime-only GameObject pool used for temporary combat objects such as skill projectiles and effects.
    /// </summary>
    public static class RuntimePool
    {
        private const string PoolRootName = "__RuntimePool";

        private static readonly Dictionary<int, Stack<GameObject>> Pools = new Dictionary<int, Stack<GameObject>>();
        private static Transform poolRoot;

        public static GameObject Acquire(GameObject prefab, Transform parent = null)
        {
            if (prefab == null)
            {
                return null;
            }

            return AcquireInternal(prefab.GetInstanceID(), () => UnityEngine.Object.Instantiate(prefab), parent);
        }

        public static GameObject Acquire(string poolName, Func<GameObject> createFunc, Transform parent = null)
        {
            if (string.IsNullOrEmpty(poolName) || createFunc == null)
            {
                return null;
            }

            return AcquireInternal(Animator.StringToHash(poolName), createFunc, parent);
        }

        public static void Release(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (!obj.TryGetComponent(out PooledRuntimeObject member))
            {
                DestroyObject(obj);
                return;
            }

            EnsurePoolRoot();

            if (!obj.activeSelf && obj.transform.parent == poolRoot)
            {
                return;
            }

            member.PrepareForPool(poolRoot);
            obj.SetActive(false);
            obj.transform.SetParent(poolRoot, false);
            GetOrCreatePool(member.PoolKey).Push(obj);
        }

        public static RuntimePoolAutoReturn EnsureAutoReturn(GameObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            RuntimePoolAutoReturn autoReturn = obj.GetComponent<RuntimePoolAutoReturn>();
            if (autoReturn == null)
            {
                autoReturn = obj.AddComponent<RuntimePoolAutoReturn>();
            }

            return autoReturn;
        }

        private static GameObject AcquireInternal(int poolKey, Func<GameObject> createFunc, Transform parent)
        {
            EnsurePoolRoot();

            Stack<GameObject> pool = GetOrCreatePool(poolKey);
            GameObject obj = null;

            while (pool.Count > 0)
            {
                obj = pool.Pop();
                if (obj != null)
                {
                    break;
                }
            }

            if (obj == null)
            {
                obj = createFunc.Invoke();
                if (obj == null)
                {
                    return null;
                }
            }

            PooledRuntimeObject member = EnsurePoolMember(obj, poolKey);
            member.PrepareForReuse(parent);
            obj.SetActive(true);
            member.RestartCachedComponents();
            return obj;
        }

        private static PooledRuntimeObject EnsurePoolMember(GameObject obj, int poolKey)
        {
            PooledRuntimeObject member = obj.GetComponent<PooledRuntimeObject>();
            if (member == null)
            {
                member = obj.AddComponent<PooledRuntimeObject>();
            }

            member.Initialize(poolKey);
            return member;
        }

        private static Stack<GameObject> GetOrCreatePool(int poolKey)
        {
            if (!Pools.TryGetValue(poolKey, out Stack<GameObject> pool))
            {
                pool = new Stack<GameObject>();
                Pools.Add(poolKey, pool);
            }

            return pool;
        }

        private static void EnsurePoolRoot()
        {
            if (poolRoot != null)
            {
                return;
            }

            GameObject root = GameObject.Find(PoolRootName);
            if (root == null)
            {
                root = new GameObject(PoolRootName);
            }

            UnityEngine.Object.DontDestroyOnLoad(root);
            poolRoot = root.transform;
        }

        private static void DestroyObject(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(obj);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }

    }

    [DisallowMultipleComponent]
    public sealed class PooledRuntimeObject : MonoBehaviour
    {
        [SerializeField] private int poolKey;
        [SerializeField] private Vector3 initialLocalScale = Vector3.one;

        private bool initialized;
        private bool restartComponentCacheValid;
        private ParticleSystem[] cachedParticleSystems = Array.Empty<ParticleSystem>();
        private Animator[] cachedAnimators = Array.Empty<Animator>();
        private Animation[] cachedAnimations = Array.Empty<Animation>();

        public int PoolKey => poolKey;

        public void Initialize(int key)
        {
            poolKey = key;
            if (initialized)
            {
                return;
            }

            initialLocalScale = transform.localScale;
            initialized = true;
            CacheRestartComponents();
        }

        public void PrepareForReuse(Transform parent)
        {
            if (!initialized)
            {
                initialLocalScale = transform.localScale;
                initialized = true;
            }

            transform.localScale = initialLocalScale;
            transform.localRotation = Quaternion.identity;
            transform.SetParent(parent, false);

            RuntimePoolAutoReturn autoReturn = GetComponent<RuntimePoolAutoReturn>();
            autoReturn?.ResetTimer();
        }

        public void RestartCachedComponents()
        {
            EnsureRestartComponentCache();

            for (int i = 0; i < cachedParticleSystems.Length; i++)
            {
                ParticleSystem particleSystem = cachedParticleSystems[i];
                if (particleSystem == null)
                {
                    continue;
                }

                particleSystem.Clear(true);
                particleSystem.Play(true);
            }

            for (int i = 0; i < cachedAnimators.Length; i++)
            {
                Animator animator = cachedAnimators[i];
                if (animator == null)
                {
                    continue;
                }

                animator.Rebind();
                animator.Update(0f);
            }

            for (int i = 0; i < cachedAnimations.Length; i++)
            {
                Animation animationComponent = cachedAnimations[i];
                if (animationComponent == null || animationComponent.clip == null)
                {
                    continue;
                }

                animationComponent.Stop();
                animationComponent.Play();
            }
        }

        public void PrepareForPool(Transform parent)
        {
            RuntimePoolAutoReturn autoReturn = GetComponent<RuntimePoolAutoReturn>();
            autoReturn?.ResetTimer();
            transform.SetParent(parent, false);
        }

        private void EnsureRestartComponentCache()
        {
            if (restartComponentCacheValid)
            {
                return;
            }

            CacheRestartComponents();
        }

        private void CacheRestartComponents()
        {
            cachedParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
            cachedAnimators = GetComponentsInChildren<Animator>(true);
            cachedAnimations = GetComponentsInChildren<Animation>(true);
            restartComponentCacheValid = true;
        }

        private void OnTransformChildrenChanged()
        {
            restartComponentCacheValid = false;
        }
    }

    [DisallowMultipleComponent]
    public sealed class RuntimePoolAutoReturn : MonoBehaviour
    {
        private float releaseAt = -1f;

        public void Schedule(float lifeTime)
        {
            if (lifeTime <= 0f)
            {
                RuntimePool.Release(gameObject);
                return;
            }

            releaseAt = Time.time + lifeTime;
            enabled = true;
        }

        public void ResetTimer()
        {
            releaseAt = -1f;
            enabled = false;
        }

        private void Update()
        {
            if (releaseAt >= 0f && Time.time >= releaseAt)
            {
                RuntimePool.Release(gameObject);
            }
        }

        private void OnDisable()
        {
            ResetTimer();
        }
    }
}
