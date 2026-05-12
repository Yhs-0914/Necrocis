using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public class TargetAttachedEffect : MonoBehaviour
    {
        private Transform target;
        private Collider[] cachedColliders = System.Array.Empty<Collider>();
        private Renderer[] cachedRenderers = System.Array.Empty<Renderer>();

        private Vector3 baseScale = Vector3.one;
        private float headOffset;
        private bool scaleByTargetSize;
        private float referenceTargetHeight = 1.1f;
        private float sizeMultiplier = 1f;
        private float minScaleMultiplier = 0.7f;
        private float maxScaleMultiplier = 2.5f;
        private bool destroyIfTargetMissing = true;

        public void Bind(
            Transform target,
            float headOffset,
            bool scaleByTargetSize,
            float referenceTargetHeight,
            float sizeMultiplier,
            float minScaleMultiplier,
            float maxScaleMultiplier,
            bool destroyIfTargetMissing = true)
        {
            this.target = target;
            this.headOffset = headOffset;
            this.scaleByTargetSize = scaleByTargetSize;
            this.referenceTargetHeight = Mathf.Max(0.01f, referenceTargetHeight);
            this.sizeMultiplier = Mathf.Max(0.01f, sizeMultiplier);
            this.minScaleMultiplier = Mathf.Max(0.05f, minScaleMultiplier);
            this.maxScaleMultiplier = Mathf.Max(this.minScaleMultiplier, maxScaleMultiplier);
            this.destroyIfTargetMissing = destroyIfTargetMissing;

            baseScale = transform.localScale;
            CacheTargetSources();
            UpdateTransform(false);
        }

        private void LateUpdate()
        {
            UpdateTransform(false);
        }

        private void CacheTargetSources()
        {
            if (target == null)
            {
                cachedColliders = System.Array.Empty<Collider>();
                cachedRenderers = System.Array.Empty<Renderer>();
                return;
            }

            cachedColliders = target.GetComponentsInChildren<Collider>(false);
            cachedRenderers = target.GetComponentsInChildren<Renderer>(false);
        }

        private void UpdateTransform(bool forceRefreshSources)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                if (destroyIfTargetMissing)
                {
                    Destroy(gameObject);
                }
                return;
            }

            if (forceRefreshSources
                || ((cachedColliders == null || cachedColliders.Length == 0)
                    && (cachedRenderers == null || cachedRenderers.Length == 0)))
            {
                CacheTargetSources();
            }

            if (!TryGetBoundsFromCachedSources(out Bounds bounds))
            {
                Vector3 fallbackPosition = target.position;
                fallbackPosition.y += headOffset;
                transform.position = fallbackPosition;
                return;
            }

            transform.position = new Vector3(bounds.center.x, bounds.max.y + headOffset, bounds.center.z);

            if (!scaleByTargetSize)
            {
                return;
            }

            float heightScale = bounds.size.y / referenceTargetHeight;
            float scaleMultiplier = Mathf.Clamp(heightScale * sizeMultiplier, minScaleMultiplier, maxScaleMultiplier);
            transform.localScale = baseScale * scaleMultiplier;
        }

        private bool TryGetBoundsFromCachedSources(out Bounds bounds)
        {
            bounds = default(Bounds);
            bool foundBounds = false;

            if (cachedColliders != null)
            {
                for (int i = 0; i < cachedColliders.Length; i++)
                {
                    Collider col = cachedColliders[i];
                    if (col == null || !col.enabled || !col.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    if (!foundBounds)
                    {
                        bounds = col.bounds;
                        foundBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(col.bounds);
                    }
                }
            }

            if (foundBounds)
            {
                return true;
            }

            if (cachedRenderers != null)
            {
                for (int i = 0; i < cachedRenderers.Length; i++)
                {
                    Renderer renderer = cachedRenderers[i];
                    if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    if (!foundBounds)
                    {
                        bounds = renderer.bounds;
                        foundBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }

            return foundBounds;
        }

        public static bool TryGetTargetBounds(Transform target, out Bounds bounds)
        {
            bounds = default(Bounds);
            if (target == null)
            {
                return false;
            }

            bool foundBounds = false;
            Collider[] colliders = target.GetComponentsInChildren<Collider>(false);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider col = colliders[i];
                if (col == null || !col.enabled || !col.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!foundBounds)
                {
                    bounds = col.bounds;
                    foundBounds = true;
                }
                else
                {
                    bounds.Encapsulate(col.bounds);
                }
            }

            if (foundBounds)
            {
                return true;
            }

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(false);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!foundBounds)
                {
                    bounds = renderer.bounds;
                    foundBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return foundBounds;
        }
    }
}
