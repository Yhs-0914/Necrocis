using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap
{
    public sealed class ObstacleSpawnData
    {
        public GameObject Prefab { get; }
        public string Name { get; }
        public Vector3 Position { get; }
        public bool FlipX { get; }

        public ObstacleSpawnData(GameObject prefab, string name, Vector3 position, bool flipX)
        {
            Prefab = prefab;
            Name = name;
            Position = position;
            FlipX = flipX;
        }
    }

    /// <summary>Perlin 밀도와 확률, 최소 간격을 결합해 장애물 위치를 결정한다.</summary>
    public static class PerlinObstacleGenerator
    {
        public static void Generate(
            GridData grid, IReadOnlyList<ObstacleDefinition> definitions,
            int seed, Transform parent, List<GameObject> generatedObjects)
        {
            if (grid == null || definitions == null || parent == null) return;
            List<ObstacleSpawnData> plan = BuildPlan(grid, definitions, seed);
            for (int i = 0; i < plan.Count; i++)
            {
                ObstacleSpawnData spawn = plan[i];
                GameObject instance = Object.Instantiate(spawn.Prefab, spawn.Position, Quaternion.identity, parent);
                instance.name = spawn.Name;
                if (spawn.FlipX)
                {
                    Vector3 scale = instance.transform.localScale;
                    scale.x *= -1f;
                    instance.transform.localScale = scale;
                }
                generatedObjects.Add(instance);
            }
        }

        public static List<ObstacleSpawnData> BuildPlan(
            GridData grid, IReadOnlyList<ObstacleDefinition> definitions, int seed)
        {
            var result = new List<ObstacleSpawnData>();
            if (grid == null || definitions == null) return result;

            var occupancy = new OccupancyGrid(grid.Width, grid.Height);
            for (int ruleIndex = 0; ruleIndex < definitions.Count; ruleIndex++)
            {
                ObstacleDefinition rule = definitions[ruleIndex];
                if (rule == null || !rule.prefab || rule.maximumCount <= 0) continue;

                var random = new System.Random(unchecked(seed * 486187739 + ruleIndex * 16777619));
                float offsetX = random.Next(-100000, 100001);
                float offsetY = random.Next(-100000, 100001);
                List<Vector2Int> candidates = BuildCandidates(grid.Width, grid.Height);
                Shuffle(candidates, random);

                int placed = 0;
                for (int i = 0; i < candidates.Count && placed < rule.maximumCount; i++)
                {
                    Vector2Int origin = candidates[i];
                    float noise = Mathf.PerlinNoise(
                        (origin.x + offsetX) * rule.noiseScale,
                        (origin.y + offsetY) * rule.noiseScale);
                    if (noise < rule.noiseThreshold) continue;

                    float density = Mathf.InverseLerp(rule.noiseThreshold, 1f, noise);
                    if (random.NextDouble() > density * rule.placementChance) continue;

                    List<Vector2Int> footprint = BuildFootprint(origin, rule.footprint);
                    if (!IsAllowedTerrain(grid, footprint, rule) ||
                        !occupancy.CanPlace(footprint, rule.minimumSpacing)) continue;

                    occupancy.Occupy(footprint);
                    result.Add(new ObstacleSpawnData(
                        rule.prefab,
                        $"{rule.name}_{placed:000}",
                        new Vector3(origin.x + rule.positionOffset.x, origin.y + rule.positionOffset.y, 0f),
                        rule.randomFlipX && random.Next(2) == 1));
                    placed++;
                }
            }
            return result;
        }

        private static List<Vector2Int> BuildCandidates(int width, int height)
        {
            var result = new List<Vector2Int>(width * height);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                result.Add(new Vector2Int(x, y));
            return result;
        }

        private static List<Vector2Int> BuildFootprint(Vector2Int origin, Vector2Int size)
        {
            size.x = Mathf.Max(1, size.x);
            size.y = Mathf.Max(1, size.y);
            var result = new List<Vector2Int>(size.x * size.y);
            for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                result.Add(new Vector2Int(origin.x + x, origin.y + y));
            return result;
        }

        private static bool IsAllowedTerrain(
            GridData grid, IReadOnlyList<Vector2Int> footprint, ObstacleDefinition rule)
        {
            for (int i = 0; i < footprint.Count; i++)
            {
                Vector2Int position = footprint[i];
                if (!grid.IsInside(position.x, position.y)) return false;
                MapCell cell = grid.GetCell(position.x, position.y);
                if (cell.HasCliff || cell.IsVoid || cell.HasLava) return false;
                if (IsNearCliff(grid, position, rule.cliffClearance)) return false;

                bool allowed = cell.TerrainType == TerrainType.SecondFloor
                    ? rule.allowSecondFloor
                    : cell.HasGrass
                        ? rule.allowGrass
                        : rule.allowBase;
                if (!allowed) return false;
            }
            return true;
        }

        private static bool IsNearCliff(GridData grid, Vector2Int position, int clearance)
        {
            clearance = Mathf.Max(0, clearance);
            for (int y = position.y - clearance; y <= position.y + clearance; y++)
            for (int x = position.x - clearance; x <= position.x + clearance; x++)
            {
                if (!grid.IsInside(x, y)) continue;
                if (grid.GetCell(x, y).HasCliff) return true;
            }
            return false;
        }

        private static void Shuffle<T>(IList<T> list, System.Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                T value = list[i];
                list[i] = list[swapIndex];
                list[swapIndex] = value;
            }
        }
    }
}
