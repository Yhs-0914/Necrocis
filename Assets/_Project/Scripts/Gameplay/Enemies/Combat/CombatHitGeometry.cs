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

        public static bool BoundsTouchCollider(Bounds bounds,Collider collider,float skin=0)=>PlanarHitGeometry.BoundsTouchCollider(bounds,collider,skin);
        public static bool TryCapsule(Collider collider,out Vector2 a,out Vector2 b,out float radius)=>PlanarHitGeometry.TryCapsule(collider,out a,out b,out radius);
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
        public static Vector2 Flat(Vector3 p)=>PlanarHitGeometry.Flat(p);
        public static float PointRectDistanceSquared(Vector2 p,Bounds rect)=>PlanarHitGeometry.PointRectDistanceSquared(p,rect);
        public static float SegmentRectDistanceSquared(Vector2 a,Vector2 b,Bounds rect)=>PlanarHitGeometry.SegmentRectDistanceSquared(a,b,rect);
        public static float SegmentDistanceSquared(Vector2 a,Vector2 b,Vector2 c,Vector2 d)=>PlanarHitGeometry.SegmentDistanceSquared(a,b,c,d);
        public static float PointSegmentDistanceSquared(Vector2 p,Vector2 a,Vector2 b)=>PlanarHitGeometry.PointSegmentDistanceSquared(p,a,b);
    }
}
