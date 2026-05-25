using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public class WorldItemSpawner : MonoBehaviour
    {
        [Header("Spawn")]
        [SerializeField, Min(1)] private int spawnCount = 3;
        [SerializeField, Min(1)] private int maxPositionAttemptsPerItem = 40;
        [SerializeField, Min(0)] private int edgePadding = 6;
        [SerializeField, Min(0f)] private float minDistanceBetweenItems = 4f;
        [SerializeField, Min(0f)] private float minDistanceFromPlayer = 5f;
        [SerializeField] private float itemGroundOffset = 0.15f;

        [Header("Visual")]
        [SerializeField] private Sprite fallbackWorldItemSprite;
        [SerializeField] private int sortingOrder = 250;

        [Header("Runtime")]
        [SerializeField] private bool autoSpawnOnStart = true;

        private bool spawned;
        private readonly List<Vector3> spawnedPositions = new List<Vector3>();
        private readonly List<GameObject> spawnedObjects = new List<GameObject>();

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

            Shuffle(candidates);
            int targetCount = Mathf.Min(spawnCount, candidates.Count);
            Transform playerTransform = PlayerController.Instance != null ? PlayerController.Instance.transform : null;

            for (int i = 0; i < targetCount; i++)
            {
                PlayerItemManager.PlayerItemEntry entry = candidates[i];
                if (!TryFindSpawnPosition(biome, playerTransform, out Vector3 spawnPosition))
                {
                    continue;
                }

                SpawnItemObject(entry, spawnPosition);
                spawnedPositions.Add(spawnPosition);
            }

            spawned = true;
        }

        public bool TrySpawnSingleRandomItemNear(Vector3 centerWorldPosition, float radius = 2f)
        {
            PlayerItemManager itemManager = PlayerItemManager.Instance;
            if (itemManager == null)
            {
                return false;
            }

            if (itemManager.ItemEntries == null || itemManager.ItemEntries.Count == 0)
            {
                itemManager.PopulateBasicProjectileTemplateItems();
            }

            List<PlayerItemManager.PlayerItemEntry> candidates = BuildCandidates(itemManager.ItemEntries);
            if (candidates.Count == 0)
            {
                return false;
            }

            BiomeManager biome = BiomeManager.Active;
            if (biome == null)
            {
                return false;
            }

            Shuffle(candidates);
            float safeRadius = Mathf.Max(0.5f, radius);

            for (int i = 0; i < maxPositionAttemptsPerItem; i++)
            {
                Vector2 offset = Random.insideUnitCircle * safeRadius;
                Vector3 candidateWorld = new Vector3(centerWorldPosition.x + offset.x, centerWorldPosition.y, centerWorldPosition.z + offset.y);
                Vector2Int grid = biome.WorldToGrid(candidateWorld);
                if (!biome.IsValidPosition(grid.x, grid.y) || !biome.IsWalkable(grid.x, grid.y))
                {
                    continue;
                }

                Vector3 spawnPos = biome.GridToWorld(grid.x, grid.y);
                spawnPos.y = biome.GetGroundHeight(grid.x, grid.y) + itemGroundOffset;

                if (IsTooCloseToExisting(spawnPos))
                {
                    continue;
                }

                PlayerItemManager.PlayerItemEntry selected = candidates[Random.Range(0, candidates.Count)];
                SpawnItemObject(selected, spawnPos);
                spawnedPositions.Add(spawnPos);
                return true;
            }

            return false;
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

        private bool TryFindSpawnPosition(BiomeManager biome, Transform playerTransform, out Vector3 spawnPosition)
        {
            int minX = Mathf.Clamp(edgePadding, 0, Mathf.Max(0, biome.MapWidth - 1));
            int minY = Mathf.Clamp(edgePadding, 0, Mathf.Max(0, biome.MapHeight - 1));
            int maxX = Mathf.Clamp(biome.MapWidth - edgePadding, 1, biome.MapWidth);
            int maxY = Mathf.Clamp(biome.MapHeight - edgePadding, 1, biome.MapHeight);

            for (int i = 0; i < maxPositionAttemptsPerItem; i++)
            {
                int x = Random.Range(minX, maxX);
                int y = Random.Range(minY, maxY);
                if (!biome.IsValidPosition(x, y) || !biome.IsWalkable(x, y))
                {
                    continue;
                }

                float groundHeight = biome.GetGroundHeight(x, y);
                spawnPosition = biome.GridToWorld(x, y);
                spawnPosition.y = groundHeight + itemGroundOffset;

                if (playerTransform != null && Vector3.Distance(playerTransform.position, spawnPosition) < minDistanceFromPlayer)
                {
                    continue;
                }

                if (IsTooCloseToExisting(spawnPosition))
                {
                    continue;
                }

                return true;
            }

            spawnPosition = default;
            return false;
        }

        private bool IsTooCloseToExisting(Vector3 position)
        {
            float minDistanceSqr = minDistanceBetweenItems * minDistanceBetweenItems;
            for (int i = 0; i < spawnedPositions.Count; i++)
            {
                if ((spawnedPositions[i] - position).sqrMagnitude < minDistanceSqr)
                {
                    return true;
                }
            }

            return false;
        }

        private void SpawnItemObject(PlayerItemManager.PlayerItemEntry entry, Vector3 worldPosition)
        {
            GameObject itemObject = new GameObject($"WorldItem_{entry.ItemId}");
            itemObject.transform.SetParent(transform, true);
            itemObject.transform.position = worldPosition;

            Sprite sprite = entry.Icon != null ? entry.Icon : fallbackWorldItemSprite;
            if (sprite != null)
            {
                SpriteRenderer renderer = itemObject.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = sortingOrder;

                Billboard billboard = itemObject.AddComponent<Billboard>();
                billboard.SetUpdateMode(Billboard.UpdateMode.Continuous);
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
                    markerRenderer.material.color = new Color(0.8f, 1f, 0.25f, 1f);
                }
            }

            BoxCollider collider = itemObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(1f, 1.2f, 1f);
            collider.center = new Vector3(0f, 0.6f, 0f);

            WorldItemPickup pickup = itemObject.AddComponent<WorldItemPickup>();
            pickup.Initialize(entry.ItemId, entry.DisplayName);

            spawnedObjects.Add(itemObject);
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}
