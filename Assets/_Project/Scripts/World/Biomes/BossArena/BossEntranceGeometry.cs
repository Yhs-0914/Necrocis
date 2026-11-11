using UnityEngine;

namespace Necrocis
{
    public readonly struct BossEntranceGeometry
    {
        public Vector2Int Size { get; }
        public float TileSize { get; }
        public int LockInset { get; }
        public int Thickness { get; }
        public BossArenaEntranceSide Side { get; }
        public int MinX => LockInset;
        public int MinY => LockInset;
        public int MaxX => Size.x - 1 - LockInset;
        public int MaxY => Size.y - 1 - LockInset;
        public bool Horizontal => Side == BossArenaEntranceSide.South || Side == BossArenaEntranceSide.North;
        public int OpeningStart { get; }
        public int OpeningEnd => OpeningStart + OpeningWidth - 1;
        public int OpeningWidth { get; }

        private BossEntranceGeometry(Vector2Int size, float tileSize, int inset, int thickness,
            BossArenaEntranceSide side, int width)
        {
            Size = new Vector2Int(Mathf.Max(8, size.x), Mathf.Max(8, size.y));
            TileSize = Mathf.Max(.01f, tileSize);
            Thickness = Mathf.Max(1, thickness);
            int maximumInset = Mathf.Max(0, (Mathf.Min(Size.x, Size.y) - Thickness * 2 - 2) / 2);
            LockInset = Mathf.Min(Mathf.Max(0, inset), maximumInset);
            Side = side == BossArenaEntranceSide.North || side == BossArenaEntranceSide.West || side == BossArenaEntranceSide.East
                ? side : BossArenaEntranceSide.South;
            int span = (Side == BossArenaEntranceSide.South || Side == BossArenaEntranceSide.North ? Size.x : Size.y) - LockInset * 2;
            OpeningWidth = Mathf.Clamp(width, 2, Mathf.Max(2, span - Thickness * 2));
            OpeningStart = LockInset + (span - OpeningWidth) / 2;
        }

        public static BossEntranceGeometry Create(Vector2Int size, float tileSize, int inset, int thickness,
            BossArenaEntranceSide side, int entranceWidth)
            => new BossEntranceGeometry(size, tileSize, inset, thickness, side, entranceWidth);

        public bool IsBoundaryCell(int localX, int localY)
        {
            return localX >= MinX && localX <= MaxX && localY >= MinY && localY <= MaxY
                && (localX < MinX + Thickness || localX > MaxX - Thickness
                    || localY < MinY + Thickness || localY > MaxY - Thickness);
        }

        public bool IsOpeningCell(int localX, int localY)
        {
            if (!IsBoundaryCell(localX, localY)) return false;
            bool onSide = Side switch {
                BossArenaEntranceSide.North => localY > MaxY - Thickness,
                BossArenaEntranceSide.West => localX < MinX + Thickness,
                BossArenaEntranceSide.East => localX > MaxX - Thickness,
                _ => localY < MinY + Thickness
            };
            int coordinate = Horizontal ? localX : localY;
            return onSide && coordinate >= OpeningStart && coordinate <= OpeningEnd;
        }

        private Vector3 Outward => Side switch {
            BossArenaEntranceSide.North => Vector3.forward,
            BossArenaEntranceSide.West => Vector3.left,
            BossArenaEntranceSide.East => Vector3.right,
            _ => Vector3.back
        };

        public Vector3 GateLocalGroundPosition
        {
            get
            {
                float span = (Horizontal ? Size.y : Size.x) - LockInset * 2;
                float offset = Mathf.Max(TileSize, span * TileSize * .5f - Thickness * TileSize * .5f);
                // Keep the legacy center axis. Odd-width cell ranges can be half a cell off this axis.
                return Outward * offset;
            }
        }

        public void GetTriggerBox(int triggerInset, float height, float heightOffset, out Vector3 center, out Vector3 size)
        {
            float depth = Mathf.Max(TileSize, triggerInset * TileSize);
            float perpendicularSpan = (Horizontal ? Size.y : Size.x) - LockInset * 2;
            float playableHalfExtent = Mathf.Max(0f, perpendicularSpan * TileSize * .5f - Thickness * TileSize);
            center = Outward * (playableHalfExtent - depth * .5f);
            center.y = heightOffset;
            size = Horizontal
                ? new Vector3(OpeningWidth * TileSize, height, depth)
                : new Vector3(depth, height, OpeningWidth * TileSize);
        }
    }
}
