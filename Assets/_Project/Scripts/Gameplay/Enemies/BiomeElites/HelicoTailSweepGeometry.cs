using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    // Curved airborne flags; warning and damage use their vertical ground projection.
    public sealed class HelicoTailSweepGeometry
    {
        public readonly Vector3 Origin, Rear, Right;
        public readonly float Reach, Arc, Radius;
        public HelicoTailSweepGeometry(Vector3 origin, Vector3 rear, float reach, float arc, float radius)
        {
            Origin = origin; Rear = Vector3.ProjectOnPlane(rear, Vector3.up).normalized;
            Right = Vector3.Cross(Vector3.up, Rear); Reach = reach; Arc = arc; Radius = radius;
        }
        public Vector3 Direction(float progress)
        {
            float a = (Mathf.Clamp01(progress) - .5f) * Arc * Mathf.Deg2Rad;
            return Rear * Mathf.Cos(a) + Right * Mathf.Sin(a);
        }
        public Vector3 Tip(float progress) => Origin + Direction(progress) * Reach;
        public const int CurveSegments = 24;
        public Vector3 CurvePoint(float progress, float along, int flag = 0)
        {
            float p = Mathf.Clamp01(progress), t = Mathf.Clamp01(along);
            if (flag != 0) p = Mathf.Clamp01(p - .1f * Mathf.Sin(p * Mathf.PI));
            // Smooth lead along the whole tail, bounded without a hard angle clamp.
            float bow = Arc * (flag == 0 ? .95f : .8f) * (1-p) * Mathf.Sin(p*Mathf.PI*.5f) * Mathf.Sin(t*Mathf.PI);
            float angle = ((p-.5f)*Arc+bow)*Mathf.Deg2Rad;
            return Origin + (Rear * Mathf.Cos(angle) + Right * Mathf.Sin(angle)) * (Reach * t * (flag == 0 ? 1 : .92f));
        }
        public Vector3 AirPoint(float progress, float along, int flag = 0)
        {
            float height = .65f + .28f * Mathf.Sin(along * Mathf.PI) + .1f * Mathf.Sin(progress * Mathf.PI) - (flag == 0 ? 0 : .09f);
            return CurvePoint(progress, along, flag) + Vector3.up * height;
        }
        public bool Contains(Vector3 point, float from = 0, float to = 1)
        {
            if (!ContainsSector(point, 0, 1)) return false;
            if (from <= 0 && to >= 1) return true;
            // Sweep the curved polyline, including the region between frames. This
            // remains continuous during the 0.18-second attack even at 15 FPS.
            int slices = Mathf.Max(1, Mathf.CeilToInt((to - from) * 160));
            Vector2 target = CombatHitGeometry.Flat(point);
            float r2 = (Radius + .0001f) * (Radius + .0001f);
            for (int flag = 0; flag < 2; flag++) for (int slice = 0; slice < slices; slice++)
            {
                float p0 = Mathf.Lerp(from, to, slice / (float)slices), p1 = Mathf.Lerp(from, to, (slice + 1f) / slices);
                Vector2 a = CombatHitGeometry.Flat(CurvePoint(p0, 0, flag)), d = CombatHitGeometry.Flat(CurvePoint(p1, 0, flag));
                for (int j = 1; j <= CurveSegments; j++)
                {
                    Vector2 b = CombatHitGeometry.Flat(CurvePoint(p0, j / (float)CurveSegments, flag));
                    Vector2 c = CombatHitGeometry.Flat(CurvePoint(p1, j / (float)CurveSegments, flag));
                    if (InTriangle(target,a,b,c) || InTriangle(target,a,c,d)
                        || CombatHitGeometry.PointSegmentDistanceSquared(target,a,b) <= r2
                        || CombatHitGeometry.PointSegmentDistanceSquared(target,b,c) <= r2
                        || CombatHitGeometry.PointSegmentDistanceSquared(target,c,d) <= r2
                        || CombatHitGeometry.PointSegmentDistanceSquared(target,d,a) <= r2) return true;
                    a = b; d = c;
                }
            }
            return false;
        }
        private static bool InTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float Cross(Vector2 u,Vector2 v) => u.x*v.y-u.y*v.x;
            if (Mathf.Abs(Cross(b-a,c-a)) < .00000001f) return false;
            float x=Cross(b-a,p-a), y=Cross(c-b,p-b), z=Cross(a-c,p-c);
            return (x>=0&&y>=0&&z>=0)||(x<=0&&y<=0&&z<=0);
        }
        private bool ContainsSector(Vector3 point, float from, float to)
        {
            Vector3 d = point - Origin; d.y = 0;
            // World-space float roundoff at the drawn boundary (0.1 mm).
            float edgeRadius = Radius + .0001f;
            float a = Mathf.Atan2(Vector3.Dot(d, Right), Vector3.Dot(d, Rear)) * Mathf.Rad2Deg;
            float first = (Mathf.Clamp01(from) - .5f) * Arc, last = (Mathf.Clamp01(to) - .5f) * Arc;
            if (a >= first && a <= last && d.magnitude <= Reach + edgeRadius) return true;
            return CombatHitGeometry.PointSegmentDistanceSquared(CombatHitGeometry.Flat(point), CombatHitGeometry.Flat(Origin), CombatHitGeometry.Flat(Tip(from))) <= edgeRadius * edgeRadius
                || CombatHitGeometry.PointSegmentDistanceSquared(CombatHitGeometry.Flat(point), CombatHitGeometry.Flat(Origin), CombatHitGeometry.Flat(Tip(to))) <= edgeRadius * edgeRadius;
        }
        public Vector3[] Outline()
        {
            var points = new List<Vector2>();
            for (int i = -1; i <= 80; i++)
            {
                float a = (i / 80f - .5f) * Arc * Mathf.Deg2Rad;
                Vector2 tip = i < 0 ? Vector2.zero : new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * Reach;
                for (int j = 0; j < 32; j++)
                {
                    float b = j * Mathf.PI * 2 / 32;
                    points.Add(tip + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * Radius);
                }
            }
            points.Sort((a,b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            var hull = new List<Vector2>();
            void Add(Vector2 p, int start)
            {
                while (hull.Count >= start + 2)
                {
                    Vector2 a = hull[hull.Count - 1] - hull[hull.Count - 2], b = p - hull[hull.Count - 1];
                    if (a.x * b.y - a.y * b.x > 0) break;
                    hull.RemoveAt(hull.Count - 1);
                }
                hull.Add(p);
            }
            foreach (var p in points) Add(p, 0);
            int end = hull.Count - 1;
            for (int i = points.Count - 2; i >= 0; i--) Add(points[i], end);
            hull.RemoveAt(hull.Count - 1);
            var result = new Vector3[hull.Count];
            for (int i = 0; i < result.Length; i++) result[i] = Right * hull[i].x + Rear * hull[i].y;
            return result;
        }
    }
}
