using System;
using UnityEngine;

namespace ProceduralMap
{
    [Serializable]
    public sealed class LungGrassSpriteSet6
    {
        [Tooltip("왼쪽이 비어 있는 경계 타일")]
        public Sprite leftEdge;
        [Tooltip("주변이 이어진 중앙 타일")]
        public Sprite center;
        [Tooltip("오른쪽이 비어 있는 경계 타일")]
        public Sprite rightEdge;
        [Tooltip("아래와 오른쪽이 비어 있는 코너 타일")]
        public Sprite bottomRight;
        [Tooltip("아래가 비어 있는 경계 타일")]
        public Sprite bottomEdge;
        [Tooltip("아래와 왼쪽이 비어 있는 코너 타일")]
        public Sprite bottomLeft;

        public bool IsComplete => leftEdge && center && rightEdge &&
                                  bottomRight && bottomEdge && bottomLeft;

        public Sprite Get(int index)
        {
            switch (index)
            {
                case 0: return leftEdge;
                case 1: return center;
                case 2: return rightEdge;
                case 3: return bottomRight;
                case 4: return bottomEdge;
                case 5: return bottomLeft;
                default: return null;
            }
        }
    }

    public static class LungGrassResolver6
    {
        public static int Resolve(GridData grid, int x, int y)
        {
            bool left = grid.HasGrassAt(x - 1, y);
            bool right = grid.HasGrassAt(x + 1, y);
            bool down = grid.HasGrassAt(x, y - 1);

            if (!down && !right) return 3;
            if (!down && !left) return 5;
            if (!down) return 4;
            if (!left) return 0;
            if (!right) return 2;
            return 1;
        }
    }
}
