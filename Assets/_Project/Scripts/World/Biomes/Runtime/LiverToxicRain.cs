using ProceduralMap;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Necrocis
{
    /// <summary>Sparse, telegraphed bile showers that damage the player on impact.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MapGenerator))]
    public sealed class LiverToxicRain : MonoBehaviour
    {
        [SerializeField] private Tilemap ground;
        [SerializeField] private Sprite dropSprite;
        [SerializeField] private Sprite splashSprite;
        [Header("Timing (scaled game seconds)")]
        [SerializeField, Min(0f)] private float initialDelay = 5f;
        [Tooltip("Time between the start of successive showers.")]
        [SerializeField, Min(1f)] private float interval = 12f;
        [SerializeField, Min(0.1f)] private float showerDuration = 3f;
        [SerializeField, Range(1f, 20f)] private float dropsPerSecond = 2f;
        [Header("Avoidance and damage")]
        [SerializeField, Min(0.2f)] private float warningDuration = 0.6f;
        [SerializeField, Min(0.1f)] private float impactRadius = 0.45f;
        [SerializeField, Min(0f)] private float damage = 1f;
        [SerializeField, Min(0.1f)] private float hitCooldown = 0.8f;
        [SerializeField, Min(0f)] private float minimumDropSpacing = 1.6f;
        [Header("Appearance")]
        [SerializeField, Min(1f)] private float fallHeight = 9f;
        [SerializeField, Min(1f)] private float fallSpeed = 10f;
        [SerializeField, Min(0.1f)] private float dropHeight = 2.4f;
        [SerializeField, Min(0.1f)] private float splashWidth = 1.3f;
        [SerializeField, Min(0.05f)] private float splashDuration = 0.3f;

        private sealed class Drop
        {
            public SpriteRenderer renderer;
            public SpriteRenderer warning;
            public Vector3 landing;
            public float height, splashAge, size, warningRemaining;
            public bool active, splashing;
        }

        private const int PoolSize = 48;
        private readonly Drop[] drops = new Drop[PoolSize];
        private MapGenerator map;
        private float cycleTimer, showerRemaining, emissionTimer;
        private float nextHit;
        private Texture2D warningTexture;
        private Sprite warningSprite;

        private void Awake()
        {
            map = GetComponent<MapGenerator>();
        }

        private void OnEnable()
        {
            cycleTimer = initialDelay;
            showerRemaining = emissionTimer = 0f;
            nextHit = 0f;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            PlayerController player = PlayerController.Instance;
            if (map == null || !map.IsReady || player == null || !player.isActiveAndEnabled || player.IsDead ||
                MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position))
            {
                ClearDrops();
                cycleTimer = initialDelay;
                showerRemaining = 0f;
                return;
            }

            Camera camera = DontStarveCamera.GetActiveCamera();
            if (camera == null || ground == null || dropSprite == null || splashSprite == null) return;
            cycleTimer -= dt;
            if (cycleTimer <= 0f)
            {
                showerRemaining = Mathf.Max(0.1f, showerDuration);
                cycleTimer = Mathf.Max(showerRemaining, interval);
                emissionTimer = 0f;
            }

            if (showerRemaining > 0f)
            {
                emissionTimer -= Mathf.Min(dt, showerRemaining);
                showerRemaining -= dt;
                // Bound catch-up work during a slow frame; never create a sudden downpour.
                int budget = 3;
                while (emissionTimer <= 0f && budget-- > 0)
                {
                    SpawnDrop(camera);
                    emissionTimer += 1f / Mathf.Clamp(dropsPerSecond, 1f, 20f);
                }
                emissionTimer = Mathf.Max(0f, emissionTimer);
            }

            foreach (Drop drop in drops)
            {
                if (drop == null || !drop.active) continue;
                if (drop.warningRemaining > 0f)
                {
                    drop.warningRemaining -= dt;
                    if (drop.warningRemaining > 0f) continue;
                    drop.renderer.enabled = true;
                    // Start falling next frame so even slow frames preserve the warning time.
                    continue;
                }
                if (!drop.splashing)
                {
                    drop.height = Mathf.Max(0f, drop.height - Mathf.Max(1f, fallSpeed) * dt);
                    drop.renderer.transform.SetPositionAndRotation(
                        drop.landing + Vector3.up * drop.height, camera.transform.rotation);
                    if (drop.height > 0f) continue;
                    drop.splashing = true;
                    drop.splashAge = 0f;
                    drop.renderer.sprite = splashSprite;
                    drop.warning.enabled = false;
                    TryHit(player, drop.landing);
                }

                drop.splashAge += dt;
                float progress = drop.splashAge / Mathf.Max(0.05f, splashDuration);
                if (progress >= 1f)
                {
                    drop.active = false;
                    drop.renderer.enabled = false;
                    continue;
                }
                drop.renderer.transform.SetPositionAndRotation(drop.landing, camera.transform.rotation);
                float scale = splashWidth * drop.size * Mathf.Lerp(0.65f, 1.2f, progress)
                    / Mathf.Max(0.001f, splashSprite.bounds.size.x);
                drop.renderer.transform.localScale = Vector3.one * scale;
                drop.renderer.color = new Color(1f, 1f, 1f, 1f - progress);
            }
        }

        private void SpawnDrop(Camera camera)
        {
            // Sample the visible ground, not the entire map; a light shower stays visible while moving.
            Plane plane = new Plane(Vector3.up, ground.transform.position);
            Vector3 landing = default;
            bool found = false;
            for (int attempt = 0; attempt < 6; attempt++)
            {
                Ray ray = camera.ViewportPointToRay(new Vector3(Random.Range(0.06f, 0.94f), Random.Range(0.08f, 0.9f), 0f));
                if (!plane.Raycast(ray, out float distance)) continue;
                landing = ray.GetPoint(distance);
                Vector3Int cell = ground.WorldToCell(landing);
                if (!map.IsCellWalkable(cell.x, cell.y)) continue;
                if (map.IsCellReservedForBossArena(cell.x, cell.y)) continue;
                bool tooClose = false;
                foreach (Drop existing in drops)
                {
                    if (existing == null || !existing.active) continue;
                    Vector2 offset = new Vector2(landing.x - existing.landing.x, landing.z - existing.landing.z);
                    if (offset.sqrMagnitude < minimumDropSpacing * minimumDropSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;
                landing.y = map.GetCellCenterWorld(cell.x, cell.y).y + 0.08f;
                found = true;
                break;
            }
            if (!found) return;

            for (int i = 0; i < drops.Length; i++)
            {
                Drop drop = drops[i];
                if (drop != null && drop.active) continue;
                if (drop == null)
                {
                    var visual = new GameObject("Toxic rain drop");
                    visual.transform.SetParent(transform, false);
                    drop = new Drop { renderer = visual.AddComponent<SpriteRenderer>() };
                    EnsureWarningSprite();
                    var marker = new GameObject("Toxic rain impact warning");
                    marker.transform.SetParent(transform, false);
                    drop.warning = marker.AddComponent<SpriteRenderer>();
                    drop.warning.sprite = warningSprite;
                    drops[i] = drop;
                }
                drop.active = true;
                drop.splashing = false;
                drop.landing = landing;
                drop.height = Mathf.Max(1f, fallHeight);
                drop.warningRemaining = Mathf.Max(0.2f, warningDuration);
                drop.size = Random.Range(0.75f, 1.15f);
                drop.renderer.sprite = dropSprite;
                drop.renderer.color = new Color(1f, 1f, 1f, 0.88f);
                drop.renderer.sortingOrder = Mathf.Max(SpriteYSort.WorldDynamicMinSortingOrder,
                    SpriteYSort.WorldDynamicBaseSortingOrder - Mathf.RoundToInt(landing.z * 10f) + 2);
                drop.renderer.transform.localScale = Vector3.one * (dropHeight * drop.size
                    / Mathf.Max(0.001f, dropSprite.bounds.size.y));
                drop.renderer.transform.SetPositionAndRotation(landing + Vector3.up * drop.height, camera.transform.rotation);
                drop.renderer.enabled = false;
                drop.warning.transform.SetPositionAndRotation(landing, Quaternion.Euler(90f, 0f, 0f));
                drop.warning.transform.localScale = Vector3.one * (Mathf.Max(0.1f, impactRadius) * 2f);
                drop.warning.sortingOrder = drop.renderer.sortingOrder - 1;
                drop.warning.color = new Color(0.85f, 0.95f, 0.2f, 0.8f);
                drop.warning.enabled = true;
                return;
            }
        }

        private void OnDisable()
        {
            ClearDrops();
        }

        private void TryHit(PlayerController player, Vector3 landing)
        {
            if (damage <= 0f || Time.time < nextHit || player.IsDead || player.IsDashInvincible ||
                (player.HealthComponent != null && player.HealthComponent.IsInvincible)) return;
            Bounds bounds = player.HitCollider != null ? player.HitCollider.bounds
                : new Bounds(player.transform.position, Vector3.one * 0.5f);
            // Match the circular ground warning to the player's horizontal collision footprint.
            float dx = landing.x - Mathf.Clamp(landing.x, bounds.min.x, bounds.max.x);
            float dz = landing.z - Mathf.Clamp(landing.z, bounds.min.z, bounds.max.z);
            float radius = Mathf.Max(0.1f, impactRadius);
            if (dx * dx + dz * dz > radius * radius) return;
            nextHit = Time.time + Mathf.Max(0.1f, hitCooldown);
            player.TakeDamage(damage);
        }

        private void EnsureWarningSprite()
        {
            if (warningSprite != null) return;
            const int size = 64;
            warningTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            warningTexture.name = "Toxic rain ground ring";
            warningTexture.filterMode = FilterMode.Point;
            warningTexture.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float radius = new Vector2(x + 0.5f - size * 0.5f, y + 0.5f - size * 0.5f).magnitude;
                byte alpha = radius > 32f ? (byte)0 : radius >= 28f ? (byte)255 : (byte)35;
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
            warningTexture.SetPixels32(pixels);
            warningTexture.Apply(false, true);
            warningSprite = Sprite.Create(warningTexture, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), size);
        }

        private void OnDestroy()
        {
            foreach (Drop drop in drops)
            {
                if (drop == null) continue;
                if (drop.renderer != null) Destroy(drop.renderer.gameObject);
                if (drop.warning != null) Destroy(drop.warning.gameObject);
            }
            if (warningSprite != null) Destroy(warningSprite);
            if (warningTexture != null) Destroy(warningTexture);
        }

        private void ClearDrops()
        {
            foreach (Drop drop in drops)
            {
                if (drop == null) continue;
                drop.active = false;
                if (drop.renderer != null) drop.renderer.enabled = false;
                if (drop.warning != null) drop.warning.enabled = false;
            }
        }
    }
}
