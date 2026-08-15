using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap
{
    /// <summary>용암 위에만 걸을 수 있는 StomachRock 발판을 배치한다.</summary>
    public static class StomachRockGenerator
    {
        public static List<ObstacleSpawnData> BuildPlan(
            GridData grid, IReadOnlyList<StomachRockDefinition> definitions, int seed)
        {
            var result = new List<ObstacleSpawnData>();
            if (grid == null || definitions == null) return result;
            var occupancy = new OccupancyGrid(grid.Width, grid.Height);

            for (int ruleIndex = 0; ruleIndex < definitions.Count; ruleIndex++)
            {
                StomachRockDefinition rule = definitions[ruleIndex];
                if (rule == null || !rule.prefab || rule.maximumCount <= 0) continue;
                var random = new System.Random(unchecked(seed * 73856093 + ruleIndex * 19349663));
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
                    if (!IsLavaFootprint(grid, footprint) ||
                        !occupancy.CanPlace(footprint, rule.minimumSpacing)) continue;

                    occupancy.Occupy(footprint);
                    for (int cellIndex = 0; cellIndex < footprint.Count; cellIndex++)
                        grid.GetCell(footprint[cellIndex].x, footprint[cellIndex].y).HasStomachRock = true;

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

        private static bool IsLavaFootprint(GridData grid, IReadOnlyList<Vector2Int> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int position = cells[i];
                if (!grid.IsInside(position.x, position.y)) return false;
                MapCell cell = grid.GetCell(position.x, position.y);
                if (!cell.HasLava || cell.HasCliff || cell.IsVoid || cell.HeightLevel != 0) return false;
            }
            return true;
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

        private static List<Vector2Int> BuildCandidates(int width, int height)
        {
            var result = new List<Vector2Int>(width * height);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++) result.Add(new Vector2Int(x, y));
            return result;
        }

        private static void Shuffle<T>(IList<T> list, System.Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int other = random.Next(i + 1);
                T value = list[i];
                list[i] = list[other];
                list[other] = value;
            }
        }
    }
}
