using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap
{
    public static class LavaPopGenerator
    {
        public static List<ObstacleSpawnData> BuildPlan(
            GridData grid, IReadOnlyList<LavaPopDefinition> definitions, int seed)
        {
            var result = new List<ObstacleSpawnData>();
            if (grid == null || definitions == null) return result;

            for (int ruleIndex = 0; ruleIndex < definitions.Count; ruleIndex++)
            {
                LavaPopDefinition rule = definitions[ruleIndex];
                if (rule == null || !rule.prefab || rule.maximumCount <= 0) continue;
                var candidates = new List<Vector2Int>();
                for (int y = 0; y < grid.Height; y++)
                for (int x = 0; x < grid.Width; x++)
                {
                    MapCell cell = grid.GetCell(x, y);
                    if (cell.HasLava && !cell.HasStomachRock && !cell.HasCliff && !cell.IsVoid)
                        candidates.Add(new Vector2Int(x, y));
                }

                var random = new System.Random(unchecked(seed + ruleIndex * 16777619));
                for (int i = candidates.Count - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
                }

                var occupied = new List<Vector2Int>();
                int placed = 0;
                for (int i = 0; i < candidates.Count && placed < rule.maximumCount; i++)
                {
                    Vector2Int p = candidates[i];
                    bool tooClose = false;
                    for (int n = 0; n < occupied.Count; n++)
                        if (Mathf.Max(Mathf.Abs(p.x - occupied[n].x), Mathf.Abs(p.y - occupied[n].y)) < rule.minimumSpacing)
                        { tooClose = true; break; }
                    if (tooClose) continue;

                    occupied.Add(p);
                    result.Add(new ObstacleSpawnData(rule.prefab, $"{rule.name}_{placed:000}",
                        new Vector3(p.x + rule.positionOffset.x, p.y + rule.positionOffset.y, 0f), false));
                    placed++;
                }
            }
            return result;
        }
    }
}
