using System;
using UnityEngine;

namespace ProceduralMap
{
    [Serializable]
    public sealed class TerrainSpriteSet9
    {
        [Tooltip("1: 좌상단")]
        public Sprite tile1;
        [Tooltip("2: 위쪽 변")]
        public Sprite tile2;
        [Tooltip("3: 우상단")]
        public Sprite tile3;
        [Tooltip("4: 왼쪽 변")]
        public Sprite tile4;
        [Tooltip("5: 내부")]
        public Sprite tile5;
        [Tooltip("6: 오른쪽 변")]
        public Sprite tile6;
        [Tooltip("7: 좌하단")]
        public Sprite tile7;
        [Tooltip("8: 아래쪽 변")]
        public Sprite tile8;
        [Tooltip("9: 우하단")]
        public Sprite tile9;

        public bool IsComplete => tile1 && tile2 && tile3 && tile4 && tile5 && tile6 && tile7 && tile8 && tile9;

        public Sprite Get(int number)
        {
            switch (number)
            {
                case 1: return tile1;
                case 2: return tile2;
                case 3: return tile3;
                case 4: return tile4;
                case 5: return tile5;
                case 6: return tile6;
                case 7: return tile7;
                case 8: return tile8;
                case 9: return tile9;
                default: throw new ArgumentOutOfRangeException(nameof(number));
            }
        }
    }

    /// <summary>완성된 Shape의 상하좌우 이웃을 보고 1~9 타일을 결정한다.</summary>
    public static class TileResolver9
    {
        public static int Resolve(GridData grid, int x, int y, TerrainType terrain)
        {
            bool up = grid.IsTerrain(x, y + 1, terrain);
            bool down = grid.IsTerrain(x, y - 1, terrain);
            bool left = grid.IsTerrain(x - 1, y, terrain);
            bool right = grid.IsTerrain(x + 1, y, terrain);

            if (!up && !left) return 1;
            if (!up && !right) return 3;
            if (!down && !left) return 7;
            if (!down && !right) return 9;
            if (!up) return 2;
            if (!left) return 4;
            if (!right) return 6;
            if (!down) return 8;
            return 5;
        }

        public static int ResolveGrass(GridData grid, int x, int y)
        {
            bool up = grid.HasGrassAt(x, y + 1);
            bool down = grid.HasGrassAt(x, y - 1);
            bool left = grid.HasGrassAt(x - 1, y);
            bool right = grid.HasGrassAt(x + 1, y);

            if (!up && !left) return 1;
            if (!up && !right) return 3;
            if (!down && !left) return 7;
            if (!down && !right) return 9;
            if (!up) return 2;
            if (!left) return 4;
            if (!right) return 6;
            if (!down) return 8;
            return 5;
        }

        public static int ResolveGrass(GridData grid, int x, int y, int variant)
        {
            bool up = grid.HasGrassVariantAt(x, y + 1, variant);
            bool down = grid.HasGrassVariantAt(x, y - 1, variant);
            bool left = grid.HasGrassVariantAt(x - 1, y, variant);
            bool right = grid.HasGrassVariantAt(x + 1, y, variant);

            if (!up && !left) return 1;
            if (!up && !right) return 3;
            if (!down && !left) return 7;
            if (!down && !right) return 9;
            if (!up) return 2;
            if (!left) return 4;
            if (!right) return 6;
            if (!down) return 8;
            return 5;
        }

        public static int ResolveLava(GridData grid, int x, int y)
        {
            bool up = grid.IsInside(x, y + 1) && grid.GetCell(x, y + 1).HasLava;
            bool down = grid.IsInside(x, y - 1) && grid.GetCell(x, y - 1).HasLava;
            bool left = grid.IsInside(x - 1, y) && grid.GetCell(x - 1, y).HasLava;
            bool right = grid.IsInside(x + 1, y) && grid.GetCell(x + 1, y).HasLava;

            if (!up && !left) return 1;
            if (!up && !right) return 3;
            if (!down && !left) return 7;
            if (!down && !right) return 9;
            if (!up) return 2;
            if (!left) return 4;
            if (!right) return 6;
            if (!down) return 8;
            return 5;
        }

        public static int ResolveStomachRock(GridData grid, int x, int y)
        {
            bool up = grid.HasStomachRockAt(x, y + 1);
            bool down = grid.HasStomachRockAt(x, y - 1);
            bool left = grid.HasStomachRockAt(x - 1, y);
            bool right = grid.HasStomachRockAt(x + 1, y);
            if (!up && !left) return 1;
            if (!up && !right) return 3;
            if (!down && !left) return 7;
            if (!down && !right) return 9;
            if (!up) return 2;
            if (!left) return 4;
            if (!right) return 6;
            if (!down) return 8;
            return 5;
        }
    }
}
