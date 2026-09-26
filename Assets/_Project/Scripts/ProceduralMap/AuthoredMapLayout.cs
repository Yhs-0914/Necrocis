using UnityEngine;

namespace ProceduralMap
{
    /// <summary>A fixed room plan rendered and traversed by the same pipeline as random organ maps.</summary>
    [CreateAssetMenu(menuName = "Procedural Map/Authored Layout")]
    public sealed class AuthoredMapLayout : ScriptableObject
    {
        public Vector2Int size = new Vector2Int(48, 38);
        public Vector2Int spawnCell = new Vector2Int(24, 9);
        public Vector2[] floorPolygon;
        public RectInt[] blockedAreas;

        public bool ContainsFloor(int x, int y)
        {
            if (x < 0 || y < 0 || x >= size.x || y >= size.y || floorPolygon == null || floorPolygon.Length < 3)
                return false;
            Vector2 point = new Vector2(x + .5f, y + .5f);
            bool inside = false;
            for (int i = 0, j = floorPolygon.Length - 1; i < floorPolygon.Length; j = i++)
            {
                Vector2 a = floorPolygon[i], b = floorPolygon[j];
                if ((a.y > point.y) != (b.y > point.y)
                    && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                    inside = !inside;
            }
            return inside;
        }

        public bool IsBlocked(int x, int y)
        {
            if (blockedAreas != null)
                foreach (RectInt area in blockedAreas)
                    if (area.Contains(new Vector2Int(x, y))) return true;
            return false;
        }

        public void Apply(GridData data)
        {
            for (int y = 0; y < data.Height; y++)
            for (int x = 0; x < data.Width; x++)
            {
                MapCell cell = data.GetCell(x, y);
                cell.Reset();
                bool floor = ContainsFloor(x, y);
                bool blocked = IsBlocked(x, y);
                cell.IsVoid = !floor || blocked;
                cell.Occupied = blocked;
                // Only the outer edge is drawn as a wall; props supply their own artwork.
                cell.HasCliff = !floor && (ContainsFloor(x - 1, y) || ContainsFloor(x + 1, y)
                    || ContainsFloor(x, y - 1) || ContainsFloor(x, y + 1));
                cell.CliffLevel = cell.HasCliff ? 1 : 0;
            }
        }
    }
}
