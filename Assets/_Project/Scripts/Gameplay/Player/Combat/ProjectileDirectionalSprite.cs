using System;
using UnityEngine;

namespace Necrocis
{
    public class ProjectileDirectionalSprite : MonoBehaviour
    {
        private const string HeadSpritesheetName = "침 투사체";
        private const string IncomingSpritePath = "AttackVisuals/basic_ranged_projectile";

        private enum Dir8
        {
            N = 0,
            NE,
            E,
            SE,
            S,
            SW,
            W,
            NW
        }

        // Dir8 index -> HEAD spritesheet index.
        private static readonly int[] DirToSpriteIndex = { 0, 1, 2, 5, 7, 8, 6, 3 };
        private static Sprite[] cachedHeadSprites;
        private static Sprite cachedIncomingSprite;
        private static bool headLoadAttempted;
        private static bool incomingLoadAttempted;

        [Tooltip("Off: HEAD 8-direction sprites first. On: Incoming generated sprite first.")]
        [SerializeField] private bool useIncomingSprite;
        [SerializeField] private float spriteScale = 0.4f;
        [SerializeField] private float incomingSpriteScale = 0.4f;
        [SerializeField] private int sortingOrder = 2700;

        private Sprite[] headSprites;
        private Sprite incomingSprite;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            EnsureSpriteRenderer();
            HideMeshRenderer();

            if (useIncomingSprite)
            {
                LoadIncomingSprite();
            }
            else
            {
                LoadHeadSprites();
            }
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

            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Vector3 normalizedDirection = direction.normalized;
            bool applied = useIncomingSprite
                ? TryApplyIncomingSprite(normalizedDirection) || TryApplyHeadSprite(normalizedDirection)
                : TryApplyHeadSprite(normalizedDirection) || TryApplyIncomingSprite(normalizedDirection);

            spriteRenderer.enabled = applied;
        }

        private bool TryApplyHeadSprite(Vector3 direction)
        {
            LoadHeadSprites();
            if (headSprites == null || headSprites.Length == 0)
            {
                return false;
            }

            int directionIndex = GetDir8Index(direction);
            int spriteIndex = DirToSpriteIndex[directionIndex];
            if (spriteIndex < 0 || spriteIndex >= headSprites.Length)
            {
                return false;
            }

            spriteRenderer.sprite = headSprites[spriteIndex];
            spriteRenderer.transform.localScale = Vector3.one * Mathf.Max(0.05f, spriteScale);

            Camera activeCamera = DontStarveCamera.GetActiveCamera();
            spriteRenderer.transform.rotation = activeCamera != null
                ? activeCamera.transform.rotation
                : Quaternion.Euler(90f, 0f, 0f);
            return true;
        }

        private bool TryApplyIncomingSprite(Vector3 direction)
        {
            LoadIncomingSprite();
            if (incomingSprite == null)
            {
                return false;
            }

            spriteRenderer.sprite = incomingSprite;
            spriteRenderer.transform.localScale =
                Vector3.one * Mathf.Max(0.05f, incomingSpriteScale);
            spriteRenderer.transform.rotation = GetScreenAlignedRotation(direction);
            return true;
        }

        private void LoadHeadSprites()
        {
            if (cachedHeadSprites != null)
            {
                headSprites = cachedHeadSprites;
                return;
            }

            if (headLoadAttempted)
            {
                return;
            }

            headLoadAttempted = true;
            Sprite[] loaded = Resources.LoadAll<Sprite>(HeadSpritesheetName);
            if (loaded == null || loaded.Length == 0)
            {
                Debug.LogWarning(
                    $"[ProjectileDirectionalSprite] Resources/{HeadSpritesheetName} HEAD sprites not found. " +
                    $"Falling back to Resources/{IncomingSpritePath}.");
                return;
            }

            Array.Sort(loaded, (left, right) => GetSpriteIndex(left).CompareTo(GetSpriteIndex(right)));
            cachedHeadSprites = loaded;
            headSprites = cachedHeadSprites;
        }

        private void LoadIncomingSprite()
        {
            if (cachedIncomingSprite != null)
            {
                incomingSprite = cachedIncomingSprite;
                return;
            }

            if (incomingLoadAttempted)
            {
                return;
            }

            incomingLoadAttempted = true;
            cachedIncomingSprite = TextureSpriteCache.LoadResourceSprite(IncomingSpritePath);
            incomingSprite = cachedIncomingSprite;
            if (incomingSprite == null)
            {
                Debug.LogWarning(
                    $"[ProjectileDirectionalSprite] Resources/{IncomingSpritePath} Incoming sprite not found.");
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

        private void HideMeshRenderer()
        {
            Renderer meshRenderer = GetComponent<Renderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }
        }

        private static int GetSpriteIndex(Sprite sprite)
        {
            string spriteName = sprite != null ? sprite.name : string.Empty;
            int separator = spriteName.LastIndexOf('_');
            if (separator < 0 || separator >= spriteName.Length - 1)
            {
                return int.MaxValue;
            }

            int value = 0;
            for (int i = separator + 1; i < spriteName.Length; i++)
            {
                char digit = spriteName[i];
                if (digit < '0' || digit > '9')
                {
                    return int.MaxValue;
                }

                value = value * 10 + (digit - '0');
            }

            return value;
        }

        private static int GetDir8Index(Vector3 direction)
        {
            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            if (angle < 0f)
            {
                angle += 360f;
            }

            if (angle < 22.5f || angle >= 337.5f)
            {
                return (int)Dir8.E;
            }

            if (angle < 67.5f)
            {
                return (int)Dir8.NE;
            }

            if (angle < 112.5f)
            {
                return (int)Dir8.N;
            }

            if (angle < 157.5f)
            {
                return (int)Dir8.NW;
            }

            if (angle < 202.5f)
            {
                return (int)Dir8.W;
            }

            if (angle < 247.5f)
            {
                return (int)Dir8.SW;
            }

            if (angle < 292.5f)
            {
                return (int)Dir8.S;
            }

            return (int)Dir8.SE;
        }

        private static Quaternion GetScreenAlignedRotation(Vector3 worldDirection)
        {
            Camera activeCamera = DontStarveCamera.GetActiveCamera();
            if (activeCamera == null)
            {
                float fallbackAngle = Mathf.Atan2(worldDirection.z, worldDirection.x) * Mathf.Rad2Deg;
                return Quaternion.Euler(90f, 0f, fallbackAngle);
            }

            Vector3 projectedDirection =
                Vector3.ProjectOnPlane(worldDirection, activeCamera.transform.forward);
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
