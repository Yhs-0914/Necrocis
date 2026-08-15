using System;
using UnityEngine;

namespace ProceduralMap
{
    public enum TerrainType
    {
        Base,
        Grass,
        SecondFloor,
        ThirdFloor,
        Pond
    }

    [Serializable]
    public sealed class MapCell
    {
        public TerrainType TerrainType { get; set; }
        public bool HasGrass { get; set; }
        public int GrassVariant { get; set; }
        public bool HasCliff { get; set; }
        public int CliffLevel { get; set; }
        public bool IsVoid { get; set; }
        public bool HasLava { get; set; }
        public bool HasStomachRock { get; set; }
        public bool HasRoad { get; set; }
        public RoadTileKind RoadKind { get; set; }
        public bool Occupied { get; set; }
        public int HeightLevel { get; set; }

        public MapCell()
        {
            Reset();
        }

        public void Reset()
        {
            TerrainType = TerrainType.Base;
            HasGrass = false;
            GrassVariant = -1;
            HasCliff = false;
            CliffLevel = 0;
            IsVoid = false;
            HasLava = false;
            HasStomachRock = false;
            HasRoad = false;
            RoadKind = RoadTileKind.None;
            Occupied = false;
            HeightLevel = 0;
        }
    }

    /// <summary>맵의 논리 셀 데이터. 렌더링용 Tilemap과 분리해 관리한다.</summary>
    public sealed class GridData
    {
        private readonly MapCell[,] cells;

        public int Width { get; }
        public int Height { get; }

        public GridData(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
            cells = new MapCell[width, height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                cells[x, y] = new MapCell();
        }

        public bool IsInside(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public MapCell GetCell(int x, int y)
        {
            if (!IsInside(x, y)) throw new ArgumentOutOfRangeException($"셀 ({x}, {y})은 맵 밖입니다.");
            return cells[x, y];
        }

        public bool IsTerrain(int x, int y, TerrainType terrainType) =>
            IsInside(x, y) && cells[x, y].TerrainType == terrainType;

        public bool HasGrassAt(int x, int y) => IsInside(x, y) && cells[x, y].HasGrass;
        public bool HasGrassVariantAt(int x, int y, int variant) =>
            IsInside(x, y) && cells[x, y].HasGrass && cells[x, y].GrassVariant == variant;
        public bool HasStomachRockAt(int x, int y) => IsInside(x, y) && cells[x, y].HasStomachRock;

        public void Clear()
        {
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                cells[x, y].Reset();
        }
    }
}
