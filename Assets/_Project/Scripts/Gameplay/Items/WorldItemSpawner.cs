using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public class WorldItemSpawner : MonoBehaviour
    {
        private const string ItemBoxSpriteConfigResourcePath = "WorldItemBoxSpriteConfig";

        [Header("Biome Scope")]
        [SerializeField] private bool spawnOnlyInIntestine = false;

        [Header("Spawn")]
        [SerializeField, Min(1)] private int spawnCount = 3;
        [SerializeField, Min(1)] private int maxPositionAttemptsPerItem = 220;
        [SerializeField, Min(0)] private int edgePadding = 10;
        [SerializeField, Min(0f)] private float minDistanceBetweenItems = 14f;
        [SerializeField, Min(0f)] private float minDistanceFromPlayer = 10f;
        [SerializeField] private bool enforceWideDistribution = true;
        [SerializeField, Min(8)] private int spreadSampleCount = 280;
        [SerializeField] private float itemGroundOffset = 0.15f;
        [SerializeField, Min(1)] private int intestineSpreadGenerationTries = 14;
        [SerializeField, Min(16)] private int intestineSpreadSampleCount = 1400;

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
        private readonly List<Vector3> spawnedPositions = new List<Vector3>();
        private WorldItemBoxSpriteConfig itemBoxSpriteConfig;

        private void Start()
        {
            if (autoSpawnOnStart)
            {
                SpawnItemsNow();
            }
        }

        private void OnValidate()
        {
            if (!enforceWideDistribution)
            {
                return;
            }

            maxPositionAttemptsPerItem = Mathf.Max(maxPositionAttemptsPerItem, 220);
            edgePadding = Mathf.Max(edgePadding, 10);
            minDistanceBetweenItems = Mathf.Max(minDistanceBetweenItems, 14f);
            minDistanceFromPlayer = Mathf.Max(minDistanceFromPlayer, 10f);
            spreadSampleCount = Mathf.Max(spreadSampleCount, 280);

            if (spawnOnlyInIntestine)
            {
                minDistanceBetweenItems = Mathf.Max(minDistanceBetweenItems, 110f);
                minDistanceFromPlayer = Mathf.Max(minDistanceFromPlayer, 24f);
                intestineSpreadGenerationTries = Mathf.Max(intestineSpreadGenerationTries, 14);
                intestineSpreadSampleCount = Mathf.Max(intestineSpreadSampleCount, 1400);
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

            if (spawnOnlyInIntestine && biome.BiomeType == BiomeType.Intestine)
            {
                minDistanceBetweenItems = Mathf.Max(minDistanceBetweenItems, 110f);
                minDistanceFromPlayer = Mathf.Max(minDistanceFromPlayer, 24f);
                edgePadding = Mathf.Max(edgePadding, 10);
                maxPositionAttemptsPerItem = Mathf.Max(maxPositionAttemptsPerItem, 220);
                intestineSpreadGenerationTries = Mathf.Max(intestineSpreadGenerationTries, 14);
                intestineSpreadSampleCount = Mathf.Max(intestineSpreadSampleCount, 1400);
            }

            Shuffle(candidates);
            int targetCount = Mathf.Min(spawnCount, candidates.Count);
            Transform playerTransform = PlayerController.Instance != null ? PlayerController.Instance.transform : null;

            List<Vector3> positions = null;
            if (biome.BiomeType == BiomeType.Intestine)
            {
                positions = GenerateIntestineUltraSpreadPositions(biome, playerTransform, targetCount);
            }
            else if (enforceWideDistribution)
            {
                positions = GenerateSpreadPositions(biome, playerTransform, targetCount);
            }

            int actualSpawnCount = positions != null ? Mathf.Min(positions.Count, targetCount) : targetCount;
            for (int i = 0; i < actualSpawnCount; i++)
            {
                PlayerItemManager.PlayerItemEntry entry = candidates[i];
                Vector3 spawnPosition;

                if (positions != null)
                {
                    spawnPosition = positions[i];
                }
                else if (!TryFindSpawnPosition(biome, playerTransform, out spawnPosition))
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

            if (spawnOnlyInIntestine && biome.BiomeType != BiomeType.Intestine)
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

        public bool TrySpawnSingleRandomItemAt(Vector3 worldPosition)
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

            if (spawnOnlyInIntestine && biome.BiomeType != BiomeType.Intestine)
            {
                return false;
            }

            Vector2Int grid = biome.WorldToGrid(worldPosition);
            if (!biome.IsValidPosition(grid.x, grid.y) || !biome.IsWalkable(grid.x, grid.y))
            {
                return TrySpawnSingleRandomItemNear(worldPosition, 2.5f);
            }

            Vector3 spawnPos = biome.GridToWorld(grid.x, grid.y);
            spawnPos.y = biome.GetGroundHeight(grid.x, grid.y) + itemGroundOffset;

            PlayerItemManager.PlayerItemEntry selected = candidates[Random.Range(0, candidates.Count)];
            SpawnItemObject(selected, spawnPos);
            spawnedPositions.Add(spawnPos);
            return true;
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

                spawnPosition = biome.GridToWorld(x, y);
                spawnPosition.y = biome.GetGroundHeight(x, y) + itemGroundOffset;

                if (playerTransform != null && Vector3.Distance(playerTransform.position, spawnPosition) < minDistanceFromPlayer)
                {
                    continue;
                }

                if (!IsTooCloseToExisting(spawnPosition))
                {
                    return true;
                }
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

        private List<Vector3> GenerateSpreadPositions(BiomeManager biome, Transform playerTransform, int targetCount)
        {
            List<Vector3> candidates = BuildCandidatePositions(biome, playerTransform, Mathf.Max(spreadSampleCount, targetCount * 24));
            return BuildUltraSpreadSet(candidates, targetCount);
        }

        private List<Vector3> BuildCandidatePositions(BiomeManager biome, Transform playerTransform, int sampleCount)
        {
            List<Vector3> result = new List<Vector3>();
            int minX = Mathf.Clamp(edgePadding, 0, Mathf.Max(0, biome.MapWidth - 1));
            int minY = Mathf.Clamp(edgePadding, 0, Mathf.Max(0, biome.MapHeight - 1));
            int maxX = Mathf.Clamp(biome.MapWidth - edgePadding, 1, biome.MapWidth);
            int maxY = Mathf.Clamp(biome.MapHeight - edgePadding, 1, biome.MapHeight);

            for (int i = 0; i < sampleCount; i++)
            {
                int x = Random.Range(minX, maxX);
                int y = Random.Range(minY, maxY);
                if (!biome.IsValidPosition(x, y) || !biome.IsWalkable(x, y))
                {
                    continue;
                }

                Vector3 pos = biome.GridToWorld(x, y);
                pos.y = biome.GetGroundHeight(x, y) + itemGroundOffset;
                if (playerTransform != null && Vector3.Distance(playerTransform.position, pos) < minDistanceFromPlayer)
                {
                    continue;
                }

                result.Add(pos);
            }

            return result;
        }

        private static float GetNearestDistance(Vector3 position, List<Vector3> references)
        {
            float nearestDistanceSqr = float.MaxValue;
            for (int i = 0; i < references.Count; i++)
            {
                float distSqr = (references[i] - position).sqrMagnitude;
                if (distSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distSqr;
                }
            }

            return Mathf.Sqrt(nearestDistanceSqr);
        }

        private List<Vector3> GenerateIntestineUltraSpreadPositions(BiomeManager biome, Transform playerTransform, int targetCount)
        {
            List<Vector3> best = new List<Vector3>();
            if (targetCount <= 0 || biome == null)
            {
                return best;
            }

            float requiredDistance = Mathf.Max(minDistanceBetweenItems, Mathf.Min(biome.MapWidth, biome.MapHeight) * 0.34f);
            float bestMinDistance = float.NegativeInfinity;

            for (int attempt = 0; attempt < intestineSpreadGenerationTries; attempt++)
            {
                List<Vector3> candidates = BuildCandidatePositions(
                    biome,
                    playerTransform,
                    Mathf.Max(intestineSpreadSampleCount, targetCount * 120));

                if (candidates.Count < targetCount)
                {
                    continue;
                }

                List<Vector3> current = BuildUltraSpreadSet(candidates, targetCount);
                if (current.Count < targetCount)
                {
                    continue;
                }

                float minDistance = GetMinimumPairwiseDistance(current);
                if (minDistance > bestMinDistance)
                {
                    bestMinDistance = minDistance;
                    best = current;
                }

                if (minDistance >= requiredDistance)
                {
                    return current;
                }
            }

            return best;
        }

        private List<Vector3> BuildUltraSpreadSet(List<Vector3> candidates, int targetCount)
        {
            List<Vector3> selected = new List<Vector3>();
            if (candidates == null || candidates.Count == 0 || targetCount <= 0)
            {
                return selected;
            }

            int firstIndex = Random.Range(0, candidates.Count);
            selected.Add(candidates[firstIndex]);
            candidates.RemoveAt(firstIndex);

            while (selected.Count < targetCount && candidates.Count > 0)
            {
                int bestIndex = -1;
                float bestNearest = float.NegativeInfinity;

                for (int i = 0; i < candidates.Count; i++)
                {
                    float nearest = GetNearestDistance(candidates[i], selected);
                    if (nearest > bestNearest)
                    {
                        bestNearest = nearest;
                        bestIndex = i;
                    }
                }

                if (bestIndex < 0)
                {
                    break;
                }

                selected.Add(candidates[bestIndex]);
                candidates.RemoveAt(bestIndex);
            }

            return selected;
        }

        private static float GetMinimumPairwiseDistance(List<Vector3> points)
        {
            if (points == null || points.Count < 2)
            {
                return 0f;
            }

            float minDistanceSqr = float.MaxValue;
            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    float distSqr = (points[i] - points[j]).sqrMagnitude;
                    if (distSqr < minDistanceSqr)
                    {
                        minDistanceSqr = distSqr;
                    }
                }
            }

            return Mathf.Sqrt(minDistanceSqr);
        }

        private void SpawnItemObject(PlayerItemManager.PlayerItemEntry entry, Vector3 worldPosition)
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
