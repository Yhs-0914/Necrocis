using UnityEngine;

namespace Necrocis
{
    public class ProjectileDirectionalSprite : MonoBehaviour
    {
        private const string GeneratedProjectileSpritePath = "AttackVisuals/basic_ranged_projectile";

        [SerializeField] private float spriteScale = 0.5f;
        [SerializeField] private int sortingOrder = 2700;

        private Sprite generatedSprite;
        private SpriteRenderer spriteRenderer;
        private bool loadAttempted;

        private void Awake()
        {
            EnsureSpriteRenderer();

            Renderer meshRenderer = GetComponent<Renderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }

            LoadGeneratedSprite();
        }

        private void OnEnable()
        {
            HideSprite();
        }

        public void HideSprite()
        {
            EnsureSpriteRenderer();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }

        public void SetDirection(Vector3 direction)
        {
            EnsureSpriteRenderer();
            if (spriteRenderer == null)
            {
                return;
            }

            LoadGeneratedSprite();
            if (generatedSprite == null)
            {
                return;
            }

            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            spriteRenderer.sprite = generatedSprite;
            spriteRenderer.enabled = true;
            spriteRenderer.transform.rotation = GetScreenAlignedRotation(direction.normalized);
        }

        private void LoadGeneratedSprite()
        {
            if (loadAttempted)
            {
                return;
            }

            loadAttempted = true;
            generatedSprite = TextureSpriteCache.LoadResourceSprite(GeneratedProjectileSpritePath);
            if (generatedSprite == null)
            {
                Debug.LogWarning($"[ProjectileDirectionalSprite] Resources/{GeneratedProjectileSpritePath} sprite not found.");
            }
        }

        private void EnsureSpriteRenderer()
        {
            if (spriteRenderer != null)
            {
                return;
            }

            Transform existing = transform.Find("ProjectileSprite");
            GameObject spriteObject = existing != null
                ? existing.gameObject
                : new GameObject("ProjectileSprite");
            spriteObject.transform.SetParent(transform, false);
            spriteObject.transform.localPosition = Vector3.zero;
            spriteObject.transform.localScale = Vector3.one * Mathf.Max(0.05f, spriteScale);

            spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sortingOrder = sortingOrder;
        }

        private static Quaternion GetScreenAlignedRotation(Vector3 worldDirection)
        {
            Camera activeCamera = DontStarveCamera.GetActiveCamera();
            if (activeCamera == null)
            {
                float fallbackAngle = Mathf.Atan2(worldDirection.z, worldDirection.x) * Mathf.Rad2Deg;
                return Quaternion.Euler(90f, 0f, fallbackAngle);
            }

            Vector3 projectedDirection = Vector3.ProjectOnPlane(worldDirection, activeCamera.transform.forward);
            if (projectedDirection.sqrMagnitude <= 0.0001f)
            {
                return activeCamera.transform.rotation;
            }

            projectedDirection.Normalize();
            float x = Vector3.Dot(projectedDirection, activeCamera.transform.right);
            float y = Vector3.Dot(projectedDirection, activeCamera.transform.up);
            float rollAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
            return activeCamera.transform.rotation * Quaternion.AngleAxis(rollAngle, Vector3.forward);
        }
    }
}
