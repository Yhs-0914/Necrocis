using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap
{
    /// <summary>논리 Shape만 만들며 Tile/Sprite 선택에는 관여하지 않는다.</summary>
    public static class RectangularShapeGenerator
    {
        public static List<Vector2Int> CreateRandom(
            System.Random random, int mapWidth, int mapHeight,
            Vector2Int minSize, Vector2Int maxSize, int edgeMargin, int extraParts)
        {
            int maxAllowedWidth = mapWidth - edgeMargin * 2;
            int maxAllowedHeight = mapHeight - edgeMargin * 2;
            if (maxAllowedWidth < minSize.x || maxAllowedHeight < minSize.y)
                return null;

            int width = random.Next(minSize.x, Mathf.Min(maxSize.x, maxAllowedWidth) + 1);
            int height = random.Next(minSize.y, Mathf.Min(maxSize.y, maxAllowedHeight) + 1);
            int originX = random.Next(edgeMargin, mapWidth - edgeMargin - width + 1);
            int originY = random.Next(edgeMargin, mapHeight - edgeMargin - height + 1);

            var cells = new HashSet<Vector2Int>();
            AddRectangle(cells, originX, originY, width, height);

            // 본체의 네 방향에 작은 직사각형 돌출부를 붙여 T/L/계단형 Shape를 만든다.
            for (int part = 0; part < extraParts; part++)
            {
                int direction = random.Next(4);
                int partWidth = random.Next(2, Mathf.Max(3, width + 1));
                int partHeight = random.Next(2, Mathf.Max(3, height + 1));
                int x;
                int y;

                if (direction <= 1)
                {
                    partWidth = Mathf.Min(partWidth, width);
                    partHeight = Mathf.Min(partHeight, Mathf.Max(2, height / 2));
                    x = originX + random.Next(0, width - partWidth + 1);
                    y = direction == 0 ? originY + height : originY - partHeight;
                }
                else
                {
                    partHeight = Mathf.Min(partHeight, height);
                    partWidth = Mathf.Min(partWidth, Mathf.Max(2, width / 2));
                    x = direction == 2 ? originX - partWidth : originX + width;
                    y = originY + random.Next(0, height - partHeight + 1);
                }

                if (x < edgeMargin || y < edgeMargin ||
                    x + partWidth > mapWidth - edgeMargin || y + partHeight > mapHeight - edgeMargin)
                    continue;

                AddRectangle(cells, x, y, partWidth, partHeight);
            }

            return new List<Vector2Int>(cells);
        }

        private static void AddRectangle(HashSet<Vector2Int> cells, int originX, int originY, int width, int height)
        {
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                cells.Add(new Vector2Int(originX + x, originY + y));
        }
    }
}
