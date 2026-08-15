using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap
{
    /// <summary>지형의 겹침과 최소 간격을 검사하는 전용 그리드.</summary>
    public sealed class OccupancyGrid
    {
        private readonly bool[,] occupied;

        public int Width { get; }
        public int Height { get; }

        public OccupancyGrid(int width, int height)
        {
            Width = width;
            Height = height;
            occupied = new bool[width, height];
        }

        public bool IsInside(Vector2Int position) =>
            position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;

        public bool IsOccupied(Vector2Int position) => IsInside(position) && occupied[position.x, position.y];

        public bool CanPlace(IReadOnlyList<Vector2Int> cells, int spacing)
        {
            spacing = Mathf.Max(0, spacing);
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int cell = cells[i];
                if (!IsInside(cell)) return false;

                for (int y = cell.y - spacing; y <= cell.y + spacing; y++)
                for (int x = cell.x - spacing; x <= cell.x + spacing; x++)
                {
                    if (x >= 0 && x < Width && y >= 0 && y < Height && occupied[x, y])
                        return false;
                }
            }
            return true;
        }

        public void Occupy(IReadOnlyList<Vector2Int> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int cell = cells[i];
                if (IsInside(cell)) occupied[cell.x, cell.y] = true;
            }
        }

        public void Clear() => System.Array.Clear(occupied, 0, occupied.Length);
    }
}
