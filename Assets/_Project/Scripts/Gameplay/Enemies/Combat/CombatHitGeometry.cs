using UnityEngine;

namespace Necrocis
{
    // Gameplay-plane geometry. Rounded shapes keep rounded corners instead of expanding an AABB.
    public static class CombatHitGeometry
    {
        public static bool SweepTouchesPlayer(Vector3 from, Vector3 to, float radius, PlayerController player)
        {
            if (player == null) return false;
            Collider collider = player.HitCollider;
            if (collider == null) return PointSegmentDistanceSquared(Flat(player.transform.position), Flat(from), Flat(to)) <= (radius + .35f) * (radius + .35f);
            if (!collider.enabled || !collider.gameObject.activeInHierarchy) return false;
            if (TryCapsule(collider, out Vector2 a, out Vector2 b, out float bodyRadius))
                return SegmentDistanceSquared(Flat(from), Flat(to), a, b) <= Mathf.Pow(Mathf.Max(0, radius) + bodyRadius, 2);
            return SegmentRectDistanceSquared(Flat(from), Flat(to), collider.bounds) <= radius * radius;
        }

        public static bool BoundsTouchCollider(Bounds bounds, Collider collider, float skin = 0)
        {
            if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy) return false;
            if (TryCapsule(collider, out Vector2 a, out Vector2 b, out float radius))
                return SegmentRectDistanceSquared(a, b, bounds) <= Mathf.Pow(radius + Mathf.Max(0, skin), 2);
            Bounds other = collider.bounds; other.Expand(new Vector3(skin * 2, 0, skin * 2));
            return bounds.min.x <= other.max.x && bounds.max.x >= other.min.x && bounds.min.z <= other.max.z && bounds.max.z >= other.min.z;
        }

        public static bool TryCapsule(Collider collider, out Vector2 a, out Vector2 b, out float radius)
        {
            Vector3 s = collider.transform.lossyScale; s = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
            if (collider is CapsuleCollider capsule)
            {
                int axis = capsule.direction;
                radius = capsule.radius * (axis == 0 ? Mathf.Max(s.y, s.z) : axis == 1 ? Mathf.Max(s.x, s.z) : Mathf.Max(s.x, s.y));
                Vector3 direction = collider.transform.TransformDirection(axis == 0 ? Vector3.right : axis == 1 ? Vector3.up : Vector3.forward);
                Vector3 center = collider.transform.TransformPoint(capsule.center);
                Vector3 half = direction * Mathf.Max(0, capsule.height * s[axis] * .5f - radius);
                a = Flat(center - half); b = Flat(center + half); return true;
            }
            if (collider is SphereCollider sphere)
            {
                radius = sphere.radius * Mathf.Max(s.x, Mathf.Max(s.y, s.z));
                a = b = Flat(collider.transform.TransformPoint(sphere.center)); return true;
            }
            a = b = default; radius = 0; return false;
        }

        public static Bounds EnemyBodyBounds(EnemyController enemy)
        {
            var config = enemy.Config;
            if (config != null && config.colliderSize.x > 0 && config.colliderSize.z > 0)
            {
                Vector3 scale = enemy.transform.lossyScale; scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                return new Bounds(enemy.transform.TransformPoint(config.colliderCenter), Vector3.Scale(config.colliderSize, scale));
            }
            var collider = enemy.GetComponent<Collider>(); return collider != null ? collider.bounds : new Bounds(enemy.transform.position, Vector3.zero);
        }
        public static Vector2 Flat(Vector3 p) => new Vector2(p.x, p.z);
        public static float PointRectDistanceSquared(Vector2 p, Bounds rect)
        {
            float x = Mathf.Max(0, Mathf.Max(rect.min.x - p.x, p.x - rect.max.x));
            float z = Mathf.Max(0, Mathf.Max(rect.min.z - p.y, p.y - rect.max.z)); return x * x + z * z;
        }
        public static float SegmentRectDistanceSquared(Vector2 a, Vector2 b, Bounds rect)
        {
            Vector2 bl = new Vector2(rect.min.x, rect.min.z), br = new Vector2(rect.max.x, rect.min.z);
            Vector2 tl = new Vector2(rect.min.x, rect.max.z), tr = new Vector2(rect.max.x, rect.max.z);
            if (PointRectDistanceSquared(a, rect) == 0 || PointRectDistanceSquared(b, rect) == 0) return 0;
            return Mathf.Min(Mathf.Min(SegmentDistanceSquared(a, b, bl, br), SegmentDistanceSquared(a, b, br, tr)),
                Mathf.Min(SegmentDistanceSquared(a, b, tr, tl), SegmentDistanceSquared(a, b, tl, bl)));
        }
        public static float SegmentDistanceSquared(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            Vector2 ab = b - a, cd = d - c, ac = c - a;
            float cross = ab.x * cd.y - ab.y * cd.x;
            if (Mathf.Abs(cross) > .000001f)
            {
                float t = (ac.x * cd.y - ac.y * cd.x) / cross, u = (ac.x * ab.y - ac.y * ab.x) / cross;
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1) return 0;
            }
            return Mathf.Min(Mathf.Min(PointSegmentDistanceSquared(a, c, d), PointSegmentDistanceSquared(b, c, d)),
                Mathf.Min(PointSegmentDistanceSquared(c, a, b), PointSegmentDistanceSquared(d, a, b)));
        }
        public static float PointSegmentDistanceSquared(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 v = b - a; float t = v.sqrMagnitude > .000001f ? Mathf.Clamp01(Vector2.Dot(p - a, v) / v.sqrMagnitude) : 0;
            return (p - a - v * t).sqrMagnitude;
        }
    }
}
