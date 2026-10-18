using UnityEngine;

namespace Necrocis
{
    // Separate point, circular flight and moving-area policies retain their original geometry.
    public static class EnemyProjectileWorldQueries
    {
        private static Vector2 Planar(Vector3 value) => new Vector2(value.x, value.z);

        private static float SegmentDistanceSquared(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            Vector2 ab = b - a, cd = d - c, ac = c - a;
            float cross = ab.x * cd.y - ab.y * cd.x;
            if (Mathf.Abs(cross) > .000001f)
            {
                float t = (ac.x * cd.y - ac.y * cd.x) / cross;
                float u = (ac.x * ab.y - ac.y * ab.x) / cross;
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1) return 0;
            }
            // Also covers parallel segments and a vertical capsule whose projected center line is a point.
            return Mathf.Min(Mathf.Min(PointSegmentDistanceSquared(a, c, d), PointSegmentDistanceSquared(b, c, d)),
                Mathf.Min(PointSegmentDistanceSquared(c, a, b), PointSegmentDistanceSquared(d, a, b)));
        }

        private static float PointSegmentDistanceSquared(Vector2 point, Vector2 from, Vector2 to)
        {
            Vector2 delta = to - from;
            float t = delta.sqrMagnitude > .000001f ? Mathf.Clamp01(Vector2.Dot(point - from, delta) / delta.sqrMagnitude) : 0;
            return (point - (from + delta * t)).sqrMagnitude;
        }

        public static Vector3 GetFlightPosition(Vector3 position, float height)
        {
            if (BiomeManager.Active != null) position.y = BiomeManager.Active.GetGroundHeight(position);
            return position + Vector3.up * height;
        }

        private static bool Clip(float start, float delta, float min, float max, ref float near, ref float far)
        {
            if (Mathf.Abs(delta) < .00001f) return start >= min && start <= max;
            float a = (min - start) / delta, b = (max - start) / delta;
            near = Mathf.Max(near, Mathf.Min(a, b)); far = Mathf.Min(far, Mathf.Max(a, b));
            return near <= far;
        }

        public static bool IsVisible(Vector3 point)
        {
            Camera camera = DontStarveCamera.GetActiveCamera();
            if (camera == null) return false;
            Vector3 view = camera.WorldToViewportPoint(point);
            return view.z > 0 && view.x >= 0 && view.x <= 1 && view.y >= 0 && view.y <= 1;
        }

        public static bool HasClearPath(Vector3 from, Vector3 to)
        {
            BiomeManager biome = BiomeManager.Active;
            if (biome == null) return true;
            Vector2Int origin = biome.WorldToGrid(from);
            int height = biome.GetHeightLevel(origin.x, origin.y);
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(from, to) / .15f));
            for (int i = 0; i <= steps; i++)
            {
                Vector3 point = Vector3.Lerp(from, to, i / (float)steps);
                Vector2Int grid = biome.WorldToGrid(point);
                if (!biome.IsValidPosition(grid.x, grid.y) || !biome.IsWalkable(grid.x, grid.y)
                    || biome.GetHeightLevel(grid.x, grid.y) != height
                    || EncounterSpatialQueries.IsInsideLockedArena(point)) return false;
            }
            return true;
        }

        public static bool HasClearFlightPath(Vector3 from, Vector3 to, float radius)
        {
            if (!HasClearPath(from, to)) return false;
            BiomeManager biome = BiomeManager.Active;
            if (biome == null) return true;
            Vector2 a = Planar(from), b = Planar(to);
            Vector2Int first = biome.WorldToGrid(from);
            int height = biome.GetHeightLevel(first.x, first.y);
            Vector2Int min = biome.WorldToGrid(new Vector3(Mathf.Min(a.x, b.x) - radius, 0, Mathf.Min(a.y, b.y) - radius));
            Vector2Int max = biome.WorldToGrid(new Vector3(Mathf.Max(a.x, b.x) + radius, 0, Mathf.Max(a.y, b.y) + radius));
            float half = biome.TileSize * .5f, radiusSquared = radius * radius;
            for (int y = min.y; y <= max.y; y++) for (int x = min.x; x <= max.x; x++)
            {
                if (biome.IsValidPosition(x, y) && biome.IsWalkable(x, y) && biome.GetHeightLevel(x, y) == height) continue;
                Vector3 center = biome.GridToWorld(x, y);
                float left = center.x - half, right = center.x + half, bottom = center.z - half, top = center.z + half;
                float near = 0, far = 1;
                if (Clip(a.x, b.x - a.x, left, right, ref near, ref far)
                    && Clip(a.y, b.y - a.y, bottom, top, ref near, ref far)) return false;
                // Sweep a circle against the cell's actual edges. An expanded rectangle would falsely block round corners.
                Vector2 bl = new Vector2(left, bottom), br = new Vector2(right, bottom);
                Vector2 tl = new Vector2(left, top), tr = new Vector2(right, top);
                if (SegmentDistanceSquared(a, b, bl, br) <= radiusSquared
                    || SegmentDistanceSquared(a, b, br, tr) <= radiusSquared
                    || SegmentDistanceSquared(a, b, tr, tl) <= radiusSquared
                    || SegmentDistanceSquared(a, b, tl, bl) <= radiusSquared) return false;
            }
            return true;
        }

        public static bool CanTravelDisk(Vector3 from, Vector3 to, float radius)
        {
            if (!HasClearPath(from, to) || !ResidueRubble.CanTraverse(from, to, new Vector2(radius, radius))) return false;
            var biome = BiomeManager.Active; if (biome == null) return true;
            var first = biome.WorldToGrid(from); int level = biome.GetHeightLevel(first.x, first.y);
            var min = biome.WorldToGrid(new Vector3(Mathf.Min(from.x, to.x) - radius, 0, Mathf.Min(from.z, to.z) - radius));
            var max = biome.WorldToGrid(new Vector3(Mathf.Max(from.x, to.x) + radius, 0, Mathf.Max(from.z, to.z) + radius));
            for (int z = min.y; z <= max.y; z++) for (int x = min.x; x <= max.x; x++)
            {
                var center = biome.GridToWorld(x, z);
                if (biome.IsValidPosition(x, z) && biome.IsWalkable(x, z) && biome.GetHeightLevel(x, z) == level
                    && !EncounterSpatialQueries.IsInsideLockedArena(center)) continue;
                var bounds = new Bounds(center, new Vector3(biome.TileSize, 1, biome.TileSize));
                if (CombatHitGeometry.SegmentRectDistanceSquared(CombatHitGeometry.Flat(from), CombatHitGeometry.Flat(to), bounds) <= radius * radius) return false;
            }
            return true;
        }

    }
}
