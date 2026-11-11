using UnityEngine;

namespace Necrocis
{
    public static class PlayerAttackGeometry
    {
        public static float Padding => PlayerController.Instance != null ? PlayerController.Instance.AttackEdgePadding : .08f;
        public static float ColliderRadius(Collider collider)
        {
            if (collider == null) return 0;
            if (CombatHitGeometry.TryCapsule(collider, out Vector2 a, out Vector2 b, out float radius))
                return radius + Vector2.Distance(a, b) * .5f;
            if (collider is BoxCollider box)
            {
                Vector3 size = Vector3.Scale(box.size, box.transform.lossyScale);
                return new Vector2(size.x, size.z).magnitude * .5f;
            }
            return Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.z);
        }
        public static bool Eligible(EnemyController enemy, LayerMask mask)
        {
            if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy) return false;
            var receiving=enemy.GetComponent<EnemyReceivingShape>();
            if(receiving!=null && receiving.HasCustom){int n=receiving.ShapeCount;for(int i=0;i<n;i++)if(receiving.IsEnabled(i,mask))return true;return false;}
            var collider = enemy.GetComponent<Collider>();
            return collider != null && collider.enabled && (mask.value & (1 << collider.gameObject.layer)) != 0;
        }
        public static float DistanceSquared(Vector3 center, EnemyController enemy)
        {
            var receiving=enemy.GetComponent<EnemyReceivingShape>();
            if(receiving==null || !receiving.HasCustom)return CombatHitGeometry.PointRectDistanceSquared(CombatHitGeometry.Flat(center),CombatHitGeometry.EnemyBodyBounds(enemy));
            float distance=float.PositiveInfinity;int n=receiving.ShapeCount;
            for(int i=0;i<n;i++)if(receiving.IsEnabled(i,~0))distance=Mathf.Min(distance,CombatHitGeometry.PointRectDistanceSquared(CombatHitGeometry.Flat(center),receiving.GetBounds(i)));
            return distance;
        }
        public static bool InCircle(Vector3 center, float radius, EnemyController enemy) => DistanceSquared(center, enemy) <= Mathf.Pow(Mathf.Max(0, radius) + Padding, 2);
        public static bool InArc(Vector3 center, Vector3 forward, float radius, float halfAngle, EnemyController enemy)
        {
            if (halfAngle >= 179.99f) return InCircle(center, radius, enemy);
            var receiving=enemy.GetComponent<EnemyReceivingShape>();
            if(receiving!=null && receiving.HasCustom){int n=receiving.ShapeCount;for(int i=0;i<n;i++)if(receiving.IsEnabled(i,~0) && InArcBounds(center,forward,radius,halfAngle,receiving.GetBounds(i)))return true;return false;}
            return InArcBounds(center,forward,radius,halfAngle,CombatHitGeometry.EnemyBodyBounds(enemy));
        }
        private static bool InArcBounds(Vector3 center,Vector3 forward,float radius,float halfAngle,Bounds body)
        {
            Vector2 origin = CombatHitGeometry.Flat(center), aim = CombatHitGeometry.Flat(forward).normalized;
            if (aim.sqrMagnitude < .0001f) aim = Vector2.up;
            float r = Mathf.Max(0, radius), cos = Mathf.Cos(Mathf.Clamp(halfAngle, .5f, 180) * Mathf.Deg2Rad);
            bool Contains(Vector2 p) { Vector2 delta = p - origin; return delta.sqrMagnitude <= (r + Padding) * (r + Padding) && (delta.sqrMagnitude < .000001f || Vector2.Dot(delta.normalized, aim) >= cos); }
            Vector2 nearest = new Vector2(Mathf.Clamp(origin.x, body.min.x, body.max.x), Mathf.Clamp(origin.y, body.min.z, body.max.z));
            if (Contains(nearest)) return true;
            if (Contains(new Vector2(body.min.x, body.min.z)) || Contains(new Vector2(body.max.x, body.min.z))
                || Contains(new Vector2(body.max.x, body.max.z)) || Contains(new Vector2(body.min.x, body.max.z))) return true;
            for (int sign = -1; sign <= 1; sign += 2)
            {
                float a = halfAngle * Mathf.Deg2Rad * sign;
                Vector2 edge = new Vector2(aim.x * Mathf.Cos(a) - aim.y * Mathf.Sin(a), aim.x * Mathf.Sin(a) + aim.y * Mathf.Cos(a));
                if (CombatHitGeometry.SegmentRectDistanceSquared(origin, origin + edge * r, body) <= Padding * Padding + .0000001f) return true;
            }
            return false;
        }
        public static bool InBox(Vector3 center, Vector3 halfSize, Quaternion rotation, EnemyController enemy)
        {
            var receiving=enemy.GetComponent<EnemyReceivingShape>();
            if(receiving!=null && receiving.HasCustom){int n=receiving.ShapeCount;for(int i=0;i<n;i++)if(receiving.IsEnabled(i,~0) && InBoxBounds(center,halfSize,rotation,receiving.GetBounds(i)))return true;return false;}
            return InBoxBounds(center,halfSize,rotation,CombatHitGeometry.EnemyBodyBounds(enemy));
        }
        private static bool InBoxBounds(Vector3 center,Vector3 halfSize,Quaternion rotation,Bounds b)
        {
            Vector2 delta = CombatHitGeometry.Flat(b.center - center);
            Vector2 right = CombatHitGeometry.Flat(rotation * Vector3.right).normalized, forward = CombatHitGeometry.Flat(rotation * Vector3.forward).normalized;
            float x = halfSize.x + Padding, z = halfSize.z + Padding;
            bool Separated(Vector2 axis) => Mathf.Abs(Vector2.Dot(delta, axis)) > x * Mathf.Abs(Vector2.Dot(right, axis)) + z * Mathf.Abs(Vector2.Dot(forward, axis))
                + b.extents.x * Mathf.Abs(axis.x) + b.extents.z * Mathf.Abs(axis.y);
            return !Separated(Vector2.right) && !Separated(Vector2.up) && !Separated(right) && !Separated(forward);
        }
    }
}
