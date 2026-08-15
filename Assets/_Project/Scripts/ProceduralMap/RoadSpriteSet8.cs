using System;
using UnityEngine;

namespace ProceduralMap
{
    public enum RoadTileKind
    {
        None,
        HorizontalA,
        HorizontalB,
        VerticalA,
        VerticalB,
        CornerUpRight,
        CornerUpLeft,
        CornerDownRight,
        CornerDownLeft
    }

    [Serializable]
    public sealed class RoadSpriteSet8
    {
        public Sprite horizontalA;
        public Sprite horizontalB;
        public Sprite verticalA;
        public Sprite verticalB;
        public Sprite cornerUpRight;
        public Sprite cornerUpLeft;
        public Sprite cornerDownRight;
        public Sprite cornerDownLeft;

        public bool IsComplete => horizontalA && horizontalB && verticalA && verticalB &&
                                  cornerUpRight && cornerUpLeft && cornerDownRight && cornerDownLeft;

        public Sprite Get(RoadTileKind kind)
        {
            switch (kind)
            {
                case RoadTileKind.HorizontalA: return horizontalA;
                case RoadTileKind.HorizontalB: return horizontalB;
                case RoadTileKind.VerticalA: return verticalA;
                case RoadTileKind.VerticalB: return verticalB;
                case RoadTileKind.CornerUpRight: return cornerUpRight;
                case RoadTileKind.CornerUpLeft: return cornerUpLeft;
                case RoadTileKind.CornerDownRight: return cornerDownRight;
                case RoadTileKind.CornerDownLeft: return cornerDownLeft;
                default: return null;
            }
        }
    }
}
