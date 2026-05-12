using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public class ProjectileAfterImageTrail : MonoBehaviour
    {
        private const string AfterImagePoolName = "__ArcherSkill2AfterImageSprite";
        private const float DefaultSpawnInterval = 0.03f;
        private const float DefaultFadeDuration = 0.15f;
        private const float DefaultStartAlpha = 0.4f;
        private const int DefaultMaxVisibleCount = 3;

        [SerializeField] private SpriteRenderer sourceRenderer;
        [SerializeField] private float spawnInterval = DefaultSpawnInterval;
        [SerializeField] private float fadeDuration = DefaultFadeDuration;
        [SerializeField] private float startAlpha = DefaultStartAlpha;
        [SerializeField] private int maxVisibleCount = DefaultMaxVisibleCount;
        [SerializeField] private float movementThreshold = 0.0005f;

        private readonly List<ProjectileAfterImageGhost> activeGhosts = new List<ProjectileAfterImageGhost>(DefaultMaxVisibleCount + 1);

        private bool trailEnabled = true;
        private bool hasLastPosition;
        private Vector3 lastPosition;
        private float spawnTimer;

        public void Configure(
            SpriteRenderer renderer,
            bool isEnabled,
            float interval,
            float fade,
            float alpha,
            int maxVisible)
        {
            sourceRenderer = renderer;
            trailEnabled = isEnabled;
            spawnInterval = Mathf.Max(0.005f, interval);
            fadeDuration = Mathf.Max(0.01f, fade);
            startAlpha = Mathf.Clamp01(alpha);
            maxVisibleCount = Mathf.Max(1, maxVisible);

            spawnTimer = 0f;
            hasLastPosition = false;
            CleanupInactiveGhosts();

            this.enabled = trailEnabled;
        }

        private void OnEnable()
        {
            spawnTimer = 0f;
            hasLastPosition = false;
        }

        private void OnDisable()
        {
            spawnTimer = 0f;
            hasLastPosition = false;
            ReleaseAllActiveGhosts();
        }

        private void Update()
        {
            if (!CanSpawnAfterImage())
            {
                spawnTimer = 0f;
                hasLastPosition = false;
                return;
            }

            Vector3 currentPosition = transform.position;
            if (!hasLastPosition)
            {
                hasLastPosition = true;
                lastPosition = currentPosition;
                return;
            }

            float moveSqrDistance = (currentPosition - lastPosition).sqrMagnitude;
            lastPosition = currentPosition;
            if (moveSqrDistance < movementThreshold * movementThreshold)
            {
                spawnTimer = 0f;
                return;
            }

            spawnTimer += Time.deltaTime;
            while (spawnTimer >= spawnInterval)
            {
                spawnTimer -= spawnInterval;
                SpawnAfterImage();
            }
        }

        private bool CanSpawnAfterImage()
        {
            if (!trailEnabled)
            {
                return false;
            }

            if (sourceRenderer == null)
            {
                sourceRenderer = GetComponentInChildren<SpriteRenderer>(true);
            }

            return sourceRenderer != null
                && sourceRenderer.enabled
                && sourceRenderer.sprite != null
                && sourceRenderer.gameObject.activeInHierarchy;
        }

        private void SpawnAfterImage()
        {
            CleanupInactiveGhosts();

            if (activeGhosts.Count >= maxVisibleCount)
            {
                ReleaseOldestGhost();
            }

            GameObject ghostObject = RuntimePool.Acquire(AfterImagePoolName, CreateGhostObject);
            if (ghostObject == null)
            {
                return;
            }

            ProjectileAfterImageGhost ghost = ghostObject.GetComponent<ProjectileAfterImageGhost>();
            if (ghost == null)
            {
                RuntimePool.Release(ghostObject);
                return;
            }

            ghost.Show(
                sourceRenderer,
                transform.position,
                fadeDuration,
                startAlpha,
                HandleGhostFadeCompleted);

            activeGhosts.Add(ghost);
        }

        private void HandleGhostFadeCompleted(ProjectileAfterImageGhost ghost)
        {
            if (ghost != null)
            {
                activeGhosts.Remove(ghost);
                RuntimePool.Release(ghost.gameObject);
            }
        }

        private void CleanupInactiveGhosts()
        {
            for (int i = activeGhosts.Count - 1; i >= 0; i--)
            {
                ProjectileAfterImageGhost ghost = activeGhosts[i];
                if (ghost == null || !ghost.IsActive)
                {
                    activeGhosts.RemoveAt(i);
                }
            }
        }

        private void ReleaseOldestGhost()
        {
            if (activeGhosts.Count == 0)
            {
                return;
            }

            ProjectileAfterImageGhost oldestGhost = activeGhosts[0];
            activeGhosts.RemoveAt(0);
            if (oldestGhost == null)
            {
                return;
            }

            if (oldestGhost.gameObject.activeInHierarchy)
            {
                RuntimePool.Release(oldestGhost.gameObject);
            }
        }

        private void ReleaseAllActiveGhosts()
        {
            for (int i = 0; i < activeGhosts.Count; i++)
            {
                ProjectileAfterImageGhost ghost = activeGhosts[i];
                if (ghost == null)
                {
                    continue;
                }

                if (ghost.gameObject.activeInHierarchy)
                {
                    RuntimePool.Release(ghost.gameObject);
                }
            }

            activeGhosts.Clear();
        }

        private static GameObject CreateGhostObject()
        {
            GameObject ghostObject = new GameObject("ProjectileAfterImage");
            ghostObject.AddComponent<SpriteRenderer>();
            ghostObject.AddComponent<ProjectileAfterImageGhost>();
            return ghostObject;
        }
    }

    [DisallowMultipleComponent]
    public sealed class ProjectileAfterImageGhost : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private System.Action<ProjectileAfterImageGhost> onFadeCompleted;

        private float fadeDuration;
        private float fadeStartTime;
        private float initialAlpha;
        private bool isActive;

        public bool IsActive => isActive;

        public void Show(
            SpriteRenderer sourceRenderer,
            Vector3 worldPosition,
            float duration,
            float startAlpha,
            System.Action<ProjectileAfterImageGhost> onCompleted)
        {
            if (sourceRenderer == null)
            {
                return;
            }

            EnsureSpriteRenderer();

            transform.SetParent(null, false);
            transform.position = worldPosition;
            transform.rotation = sourceRenderer.transform.rotation;
            transform.localScale = sourceRenderer.transform.lossyScale;
            gameObject.layer = sourceRenderer.gameObject.layer;

            spriteRenderer.sprite = sourceRenderer.sprite;
            spriteRenderer.flipX = sourceRenderer.flipX;
            spriteRenderer.flipY = sourceRenderer.flipY;
            spriteRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
            spriteRenderer.sortingOrder = sourceRenderer.sortingOrder;
            spriteRenderer.sharedMaterial = sourceRenderer.sharedMaterial;

            Color color = sourceRenderer.color;
            color.a = Mathf.Clamp01(startAlpha);
            spriteRenderer.color = color;

            fadeDuration = Mathf.Max(0.01f, duration);
            fadeStartTime = Time.time;
            initialAlpha = color.a;
            onFadeCompleted = onCompleted;
            isActive = true;
            enabled = true;
        }

        public void ReleaseImmediately()
        {
            if (!isActive)
            {
                return;
            }

            CompleteFade();
        }

        private void Update()
        {
            if (!isActive || spriteRenderer == null)
            {
                return;
            }

            float elapsed = Time.time - fadeStartTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(initialAlpha, 0f, t);
            spriteRenderer.color = color;

            if (t >= 1f)
            {
                CompleteFade();
            }
        }

        private void OnDisable()
        {
            isActive = false;
            onFadeCompleted = null;
            enabled = false;
        }

        private void EnsureSpriteRenderer()
        {
            if (spriteRenderer != null)
            {
                return;
            }

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        private void CompleteFade()
        {
            if (!isActive)
            {
                return;
            }

            isActive = false;
            enabled = false;

            System.Action<ProjectileAfterImageGhost> callback = onFadeCompleted;
            onFadeCompleted = null;
            callback?.Invoke(this);
        }
    }
}
