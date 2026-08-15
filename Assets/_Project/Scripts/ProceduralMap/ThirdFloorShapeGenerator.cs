using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap
{
    /// <summary>이미 생성된 2층 셀 안에서만 3층 Shape를 성장시킨다.</summary>
    public static class ThirdFloorShapeGenerator
    {
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        public static List<Vector2Int> Create(
            System.Random random, GridData grid, int targetCells, int smoothingIterations)
        {
            var seeds = new List<Vector2Int>();
            for (int y = 1; y < grid.Height - 1; y++)
            for (int x = 1; x < grid.Width - 1; x++)
                if (IsAllowed(grid, new Vector2Int(x, y))) seeds.Add(new Vector2Int(x, y));
            if (seeds.Count == 0) return null;

            var shape = new HashSet<Vector2Int>();
            var frontier = new List<Vector2Int>();
            var frontierSet = new HashSet<Vector2Int>();
            Vector2Int seed = seeds[random.Next(seeds.Count)];
            shape.Add(seed);
            AddNeighbours(seed, grid, shape, frontier, frontierSet);

            while (shape.Count < targetCells && frontier.Count > 0)
            {
                int index = random.Next(frontier.Count);
                Vector2Int cell = frontier[index];
                frontier[index] = frontier[frontier.Count - 1];
                frontier.RemoveAt(frontier.Count - 1);
                frontierSet.Remove(cell);
                if (!IsAllowed(grid, cell) || !shape.Add(cell)) continue;
                AddNeighbours(cell, grid, shape, frontier, frontierSet);
            }

            for (int iteration = 0; iteration < smoothingIterations; iteration++)
                Smooth(grid, shape);
            return new List<Vector2Int>(shape);
        }

        private static bool IsAllowed(GridData grid, Vector2Int p)
        {
            if (!grid.IsInside(p.x, p.y)) return false;
            MapCell cell = grid.GetCell(p.x, p.y);
            return cell.HeightLevel == 1 && !cell.HasCliff && !cell.IsVoid;
        }

        private static void AddNeighbours(
            Vector2Int center, GridData grid, HashSet<Vector2Int> shape,
            List<Vector2Int> frontier, HashSet<Vector2Int> frontierSet)
        {
            for (int i = 0; i < Directions.Length; i++)
            {
                Vector2Int next = center + Directions[i];
                if (!IsAllowed(grid, next) || shape.Contains(next) || !frontierSet.Add(next)) continue;
                frontier.Add(next);
            }
        }

        private static void Smooth(GridData grid, HashSet<Vector2Int> shape)
        {
            var candidates = new HashSet<Vector2Int>(shape);
            foreach (Vector2Int cell in shape)
                for (int i = 0; i < Directions.Length; i++) candidates.Add(cell + Directions[i]);

            var next = new HashSet<Vector2Int>();
            foreach (Vector2Int cell in candidates)
            {
                if (!IsAllowed(grid, cell)) continue;
                int neighbours = 0;
                for (int i = 0; i < Directions.Length; i++)
                    if (shape.Contains(cell + Directions[i])) neighbours++;
                if ((shape.Contains(cell) && neighbours >= 2) || neighbours >= 3) next.Add(cell);
            }
            if (next.Count >= 4)
            {
                shape.Clear();
                shape.UnionWith(next);
            }
        }
    }
}
