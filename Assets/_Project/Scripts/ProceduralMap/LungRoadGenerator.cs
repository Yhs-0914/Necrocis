using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap
{
    /// <summary>한쪽 맵 경계에서 반대 경계까지 끊기지 않는 폐 길을 만든다.</summary>
    public static class LungRoadGenerator
    {
        public static List<Vector2Int> Create(
            System.Random random, int width, int height, int margin, float turnChance,
            bool horizontal, int preferredStart = -1)
        {
            var path = new List<Vector2Int>();
            var used = new HashSet<Vector2Int>();
            margin = Mathf.Clamp(margin, 1, Mathf.Max(1, Mathf.Min(width, height) / 3));

            if (horizontal)
            {
                int y = preferredStart >= 0
                    ? Mathf.Clamp(preferredStart, margin, height - margin - 1)
                    : random.Next(margin, height - margin);
                Add(path, used, new Vector2Int(0, y));
                for (int x = 0; x < width - 1; x++)
                {
                    if (random.NextDouble() < turnChance)
                    {
                        int delta = random.Next(2) == 0 ? -1 : 1;
                        int nextY = Mathf.Clamp(y + delta, margin, height - margin - 1);
                        if (nextY != y) { y = nextY; Add(path, used, new Vector2Int(x, y)); }
                    }
                    Add(path, used, new Vector2Int(x + 1, y));
                }
            }
            else
            {
                int x = preferredStart >= 0
                    ? Mathf.Clamp(preferredStart, margin, width - margin - 1)
                    : random.Next(margin, width - margin);
                Add(path, used, new Vector2Int(x, 0));
                for (int y = 0; y < height - 1; y++)
                {
                    if (random.NextDouble() < turnChance)
                    {
                        int delta = random.Next(2) == 0 ? -1 : 1;
                        int nextX = Mathf.Clamp(x + delta, margin, width - margin - 1);
                        if (nextX != x) { x = nextX; Add(path, used, new Vector2Int(x, y)); }
                    }
                    Add(path, used, new Vector2Int(x, y + 1));
                }
            }
            return path;
        }

        public static RoadTileKind Resolve(List<Vector2Int> path, int index, int seed)
        {
            Vector2Int current = path[index];
            Vector2Int before = index > 0 ? path[index - 1] - current : Vector2Int.zero;
            Vector2Int after = index < path.Count - 1 ? path[index + 1] - current : Vector2Int.zero;
            bool left = before == Vector2Int.left || after == Vector2Int.left;
            bool right = before == Vector2Int.right || after == Vector2Int.right;
            bool up = before == Vector2Int.up || after == Vector2Int.up;
            bool down = before == Vector2Int.down || after == Vector2Int.down;

            if (up && right) return RoadTileKind.CornerUpRight;
            if (up && left) return RoadTileKind.CornerUpLeft;
            if (down && right) return RoadTileKind.CornerDownRight;
            if (down && left) return RoadTileKind.CornerDownLeft;
            int hash = unchecked(seed * 397 ^ current.x * 73856093 ^ current.y * 19349663);
            if (up || down) return (hash & 1) == 0 ? RoadTileKind.VerticalA : RoadTileKind.VerticalB;
            return (hash & 1) == 0 ? RoadTileKind.HorizontalA : RoadTileKind.HorizontalB;
        }

        /// <summary>한 칸 중심선을 2칸 이상 두께의 이어진 길 Shape로 확장한다.</summary>
        public static HashSet<Vector2Int> BuildRibbon(
            List<Vector2Int> path, int mapWidth, int mapHeight)
        {
            var area = new HashSet<Vector2Int>();
            for (int i = 0; i < path.Count; i++)
            {
                Vector2Int p = path[i];
                AddInside(area, p, mapWidth, mapHeight);
                AddInside(area, p + Vector2Int.right, mapWidth, mapHeight);
                AddInside(area, p + Vector2Int.down, mapWidth, mapHeight);
                AddInside(area, p + Vector2Int.right + Vector2Int.down, mapWidth, mapHeight);
            }
            return area;
        }

        /// <summary>3×3 테두리 번호 1/2/3/4/6/7/8/9에 대응하는 Sprite를 결정한다.</summary>
        public static RoadTileKind Resolve(HashSet<Vector2Int> area, Vector2Int cell)
        {
            bool up = area.Contains(cell + Vector2Int.up);
            bool down = area.Contains(cell + Vector2Int.down);
            bool left = area.Contains(cell + Vector2Int.left);
            bool right = area.Contains(cell + Vector2Int.right);

            if (!up && !left) return RoadTileKind.CornerUpLeft;
            if (!up && !right) return RoadTileKind.CornerUpRight;
            if (!down && !left) return RoadTileKind.CornerDownLeft;
            if (!down && !right) return RoadTileKind.CornerDownRight;
            if (!up) return RoadTileKind.HorizontalA;   // 2번: 위 경계
            if (!left) return RoadTileKind.VerticalA;   // 4번: 왼쪽 경계
            if (!right) return RoadTileKind.VerticalB;  // 6번: 오른쪽 경계
            if (!down) return RoadTileKind.HorizontalB; // 8번: 아래 경계

            // 중앙 타일이 없는 세트이므로 굽은 곳의 완전 내부 셀은 빈 셀로 둔다.
            return RoadTileKind.None;
        }

        private static void AddInside(
            HashSet<Vector2Int> area, Vector2Int cell, int width, int height)
        {
            if (cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height)
                area.Add(cell);
        }

        private static void Add(List<Vector2Int> path, HashSet<Vector2Int> used, Vector2Int cell)
        {
            if (used.Add(cell)) path.Add(cell);
        }
    }
}
