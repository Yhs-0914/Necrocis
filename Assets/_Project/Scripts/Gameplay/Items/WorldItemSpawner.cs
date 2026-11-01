using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public partial class WorldItemSpawner : MonoBehaviour
    {
        private const string ItemBoxSpriteConfigResourcePath = "WorldItemBoxSpriteConfig";

        [Header("Biome Scope")]
        [SerializeField] private bool spawnOnlyInIntestine = false;

        [SerializeField] private float itemGroundOffset = 0.15f;

        [Header("Visual")]
        [SerializeField] private Sprite fallbackWorldItemSprite;
        [SerializeField] private Sprite itemBoxClosedSprite;
        [SerializeField] private Sprite itemBoxOpenSprite;
        [SerializeField, Min(0.1f)] private float itemBoxTargetHeight = 1.35f;
        [SerializeField, Min(0.1f)] private float itemBoxTriggerWidth = 1.25f;
        [SerializeField, Min(0.1f)] private float itemBoxTriggerHeight = 1.35f;
        [SerializeField] private int sortingOrder = 250;

        [Header("Runtime")]
        [SerializeField] private bool autoSpawnOnStart = true;

        private bool spawned;
        private WorldItemBoxSpriteConfig itemBoxSpriteConfig;

        private void Start()
        {
            if (autoSpawnOnStart)
            {
                SpawnItemsNow();
            }
        }

        public void SpawnItemsNow()
        {
            if (spawned)
            {
                return;
            }

            PlayerItemManager itemManager = PlayerItemManager.Instance;
            if (itemManager == null)
            {
                return;
            }

            if (itemManager.ItemEntries == null || itemManager.ItemEntries.Count == 0)
            {
                itemManager.PopulateBasicProjectileTemplateItems();
            }

            List<PlayerItemManager.PlayerItemEntry> candidates = BuildCandidates(itemManager.ItemEntries);
            if (candidates.Count == 0)
            {
                return;
            }

            BiomeManager biome = BiomeManager.Active;
            if (biome == null)
            {
                return;
            }

            if (spawnOnlyInIntestine && biome.BiomeType != BiomeType.Intestine)
            {
                spawned = true;
                return;
            }

            // Random map caches have been replaced by enemy drops. Restore only
            // rewards already saved, including unopened caches from older runs.
            spawned = true;
            RestorePendingRewards(biome.BiomeType);
        }

        public bool TrySpawnSingleRandomItemNear(Vector3 centerWorldPosition, float radius = 2f)
            => TrySpawnChoiceRewardAt("drop." + System.Guid.NewGuid().ToString("N"), centerWorldPosition, searchRadius: radius);

        public bool TrySpawnSingleRandomItemAt(Vector3 worldPosition)
            => TrySpawnChoiceRewardAt("drop." + System.Guid.NewGuid().ToString("N"), worldPosition);

        public bool TrySpawnItemAt(string itemId, Vector3 worldPosition,
            bool snapToWalkableGround = true, bool ignoreBiomeScope = false)
        {
            var inventory = PlayerItemManager.Instance;
            if (inventory == null || !inventory.TryGetItemEntry(itemId, out var preferred)) return false;
            var biome = BiomeManager.Active;
            if (!ignoreBiomeScope && spawnOnlyInIntestine && (biome == null || biome.BiomeType != BiomeType.Intestine)) return false;
            return TrySpawnChoiceRewardAt("item." + System.Guid.NewGuid().ToString("N"), worldPosition, preferred, snapToWalkableGround);
        }

        public void SetAutoSpawnOnStart(bool enabled)
        {
            autoSpawnOnStart = enabled;
        }

        private List<PlayerItemManager.PlayerItemEntry> BuildCandidates(IReadOnlyList<PlayerItemManager.PlayerItemEntry> entries)
        {
            List<PlayerItemManager.PlayerItemEntry> result = new List<PlayerItemManager.PlayerItemEntry>();
            if (entries == null)
            {
                return result;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                PlayerItemManager.PlayerItemEntry entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    continue;
                }

                result.Add(entry);
            }

            return result;
        }

        private bool TryFindNearestWalkableSpawnPosition(
            BiomeManager biome, Vector3 centerWorldPosition, float radius, out Vector3 spawnPosition)
        {
            Vector2Int origin = biome.WorldToGrid(centerWorldPosition);
            int searchRadius = Mathf.Max(1, Mathf.CeilToInt(radius));
            for (int ring = 0; ring <= searchRadius; ring++)
            {
                for (int y = -ring; y <= ring; y++)
                for (int x = -ring; x <= ring; x++)
                {
                    if (ring > 0 && Mathf.Abs(x) != ring && Mathf.Abs(y) != ring) continue;
                    int gx = origin.x + x, gy = origin.y + y;
                    if (!biome.IsValidPosition(gx, gy) || !biome.IsWalkable(gx, gy)) continue;
                    spawnPosition = biome.GridToWorld(gx, gy);
                    spawnPosition.y = biome.GetGroundHeight(gx, gy) + itemGroundOffset;
                    return true;
                }
            }
            spawnPosition = default;
            return false;
        }

        private WorldItemBoxPickup SpawnItemObject(PlayerItemManager.PlayerItemEntry entry, Vector3 worldPosition)
        {
            Sprite itemSprite = entry.Icon != null ? entry.Icon : fallbackWorldItemSprite;
            Sprite closedSprite = ResolveItemBoxClosedSprite();
            Sprite openSprite = ResolveItemBoxOpenSprite();

            GameObject itemObject = new GameObject($"ItemBox_{entry.ItemId}");
            itemObject.transform.SetParent(transform, true);
            itemObject.transform.position = worldPosition;
            itemObject.transform.localScale = Vector3.one * ResolveItemBoxScale(closedSprite);

            if (closedSprite != null)
            {
                SpriteRenderer renderer = itemObject.AddComponent<SpriteRenderer>();
                renderer.sprite = closedSprite;
                renderer.sortingOrder = sortingOrder;

                Billboard billboard = itemObject.AddComponent<Billboard>();
                billboard.SetUpdateMode(Billboard.UpdateMode.Once);
            }
            else
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "Marker";
                marker.transform.SetParent(itemObject.transform, false);
                marker.transform.localScale = Vector3.one * 0.6f;

                Collider markerCollider = marker.GetComponent<Collider>();
                if (markerCollider != null)
                {
                    Destroy(markerCollider);
                }

                Renderer markerRenderer = marker.GetComponent<Renderer>();
                if (markerRenderer != null && markerRenderer.material != null)
                {
                    markerRenderer.material.color = new Color(0.7f, 0.35f, 0.12f, 1f);
                }
            }

            BoxCollider collider = itemObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            float itemBoxScale = Mathf.Max(0.001f, itemObject.transform.localScale.x);
            collider.size = new Vector3(itemBoxTriggerWidth / itemBoxScale, itemBoxTriggerHeight / itemBoxScale, itemBoxTriggerWidth / itemBoxScale);
            collider.center = new Vector3(0f, (itemBoxTriggerHeight * 0.5f) / itemBoxScale, 0f);

            WorldItemBoxPickup pickup = itemObject.AddComponent<WorldItemBoxPickup>();
            pickup.Initialize(entry.ItemId, entry.DisplayName, closedSprite, openSprite, itemSprite, sortingOrder);
            return pickup;
        }

        private float ResolveItemBoxScale(Sprite sprite)
        {
            if (sprite == null || sprite.bounds.size.y <= 0.0001f)
            {
                return 1f;
            }

            return Mathf.Max(0.001f, itemBoxTargetHeight / sprite.bounds.size.y);
        }

        private Sprite ResolveItemBoxClosedSprite()
        {
            if (itemBoxClosedSprite != null)
            {
                return itemBoxClosedSprite;
            }

            itemBoxClosedSprite = Resources.Load<Sprite>("Item/Box/ItemBox_close");
            if (itemBoxClosedSprite == null)
            {
                WorldItemBoxSpriteConfig config = ResolveItemBoxSpriteConfig();
                if (config != null)
                {
                    itemBoxClosedSprite = config.ClosedSprite;
                }
            }
#if UNITY_EDITOR
            if (itemBoxClosedSprite == null)
            {
                itemBoxClosedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Images/Item/Box/ItemBox_close.png");
            }
#endif
            return itemBoxClosedSprite;
        }

        private Sprite ResolveItemBoxOpenSprite()
        {
            if (itemBoxOpenSprite != null)
            {
                return itemBoxOpenSprite;
            }

            itemBoxOpenSprite = Resources.Load<Sprite>("Item/Box/ItemBox_open");
            if (itemBoxOpenSprite == null)
            {
                WorldItemBoxSpriteConfig config = ResolveItemBoxSpriteConfig();
                if (config != null)
                {
                    itemBoxOpenSprite = config.OpenSprite;
                }
            }
#if UNITY_EDITOR
            if (itemBoxOpenSprite == null)
            {
                itemBoxOpenSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Images/Item/Box/ItemBox_open.png");
            }
#endif
            return itemBoxOpenSprite;
        }

        private WorldItemBoxSpriteConfig ResolveItemBoxSpriteConfig()
        {
            if (itemBoxSpriteConfig != null)
            {
                return itemBoxSpriteConfig;
            }

            itemBoxSpriteConfig = Resources.Load<WorldItemBoxSpriteConfig>(ItemBoxSpriteConfigResourcePath);
            return itemBoxSpriteConfig;
        }

    }
}
