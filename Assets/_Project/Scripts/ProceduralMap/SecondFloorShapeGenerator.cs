using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap
{
    /// <summary>연결된 셀을 무작위로 성장시킨 뒤 간단히 다듬어 불규칙한 2층 Shape를 만든다.</summary>
    public static class SecondFloorShapeGenerator
    {
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        public static List<Vector2Int> Create(
            System.Random random, int mapWidth, int mapHeight,
            int targetCellCount, int edgeMargin, int smoothingIterations)
        {
            int minX = edgeMargin;
            int minY = edgeMargin;
            int maxX = mapWidth - edgeMargin - 1;
            int maxY = mapHeight - edgeMargin - 1;
            if (minX > maxX || minY > maxY || targetCellCount <= 0) return null;

            var shape = new HashSet<Vector2Int>();
            var cells = new List<Vector2Int>();
            Vector2Int seed = new Vector2Int(random.Next(minX, maxX + 1), random.Next(minY, maxY + 1));
            shape.Add(seed);
            cells.Add(seed);

            // Shape마다 가로/세로 성장 성향을 다르게 뽑아 폭과 실루엣의 다양성을 만든다.
            double horizontalChance = 0.2 + random.NextDouble() * 0.6;

            int failedGrowth = 0;
            int failureLimit = Mathf.Max(100, targetCellCount * 20);
            while (shape.Count < targetCellCount && failedGrowth < failureLimit)
            {
                // 최근에 생긴 셀에서 자라나는 가지와 기존 덩어리에서 뻗는 성장을 혼합한다.
                int recentRange = Mathf.Min(cells.Count, Mathf.Max(12, targetCellCount / 20));
                int sourceIndex = random.NextDouble() < 0.4
                    ? random.Next(cells.Count - recentRange, cells.Count)
                    : random.Next(cells.Count);
                Vector2Int source = cells[sourceIndex];
                bool horizontal = random.NextDouble() < horizontalChance;
                Vector2Int direction = horizontal
                    ? (random.Next(2) == 0 ? Vector2Int.left : Vector2Int.right)
                    : (random.Next(2) == 0 ? Vector2Int.down : Vector2Int.up);
                Vector2Int next = source + direction;
                if (next.x < minX || next.x > maxX || next.y < minY || next.y > maxY || !shape.Add(next))
                {
                    failedGrowth++;
                    continue;
                }
                cells.Add(next);
            }

            for (int i = 0; i < smoothingIterations; i++)
                shape = Smooth(shape, minX, minY, maxX, maxY);

            return new List<Vector2Int>(shape);
        }

        private static HashSet<Vector2Int> Smooth(
            HashSet<Vector2Int> source, int minX, int minY, int maxX, int maxY)
        {
            var candidates = new HashSet<Vector2Int>(source);
            foreach (Vector2Int cell in source)
            foreach (Vector2Int direction in Directions)
            {
                Vector2Int neighbor = cell + direction;
                if (neighbor.x >= minX && neighbor.x <= maxX && neighbor.y >= minY && neighbor.y <= maxY)
                    candidates.Add(neighbor);
            }

            var result = new HashSet<Vector2Int>();
            foreach (Vector2Int cell in candidates)
            {
                int neighbors = 0;
                for (int i = 0; i < Directions.Length; i++)
                    if (source.Contains(cell + Directions[i])) neighbors++;

                // 기존 셀은 이웃 1개 이상이면 유지하고, 빈 셀은 3면이 둘러싸였을 때 메운다.
                if ((source.Contains(cell) && neighbors >= 1) || (!source.Contains(cell) && neighbors >= 3))
                    result.Add(cell);
            }
            return result;
        }
    }
}
