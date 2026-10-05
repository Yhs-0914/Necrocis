using UnityEngine;

namespace Necrocis
{
    /// <summary>One straight shot owned by one spawn. No splash, homing, secondary spawn or lingering damage.</summary>
    public sealed class GasSacOrb : MonoBehaviour
    {
        private EnemyPatternLifetime lifetime;
        private uint generation;
        private EnemyDamageRequest damage;
        private Vector3 direction;
        private float speed, remaining, radius;
        private bool finished;
        private Transform body, shadow;
        public Vector3 Direction => direction;
        public float Speed => speed;
        public float HitRadius => radius;
        public float FlightHeight => radius; // Ground-skimming orb: derive height from its one authored radius.
        public Vector3 LaunchPosition { get; private set; }

        public static bool TryGetLaunchPosition(EnemyController owner, Vector3 direction, float radius, out Vector3 position)
        {
            position = owner != null ? owner.transform.position : Vector3.zero;
            direction.y = 0;
            if (owner == null || direction.sqrMagnitude < .000001f || radius <= 0) return false;
            direction.Normalize();
            Collider collider = owner.GetComponent<Collider>();
            Vector3 extents = collider != null ? collider.bounds.extents : Vector3.zero;
            float extent = Mathf.Abs(direction.x) * extents.x + Mathf.Abs(direction.z) * extents.z;
            Vector3 origin = owner.GetPatternVisualOrigin();
            origin.y = owner.transform.position.y;
            position = origin + direction * (extent + radius * .5f);
            if (BiomeManager.Active != null) position.y = BiomeManager.Active.GetGroundHeight(position);
            return HasClearFlightPath(owner.transform.position, position, radius)
                && IsVisible(GetFlightPosition(position, radius));
        }

        public static GasSacOrb Launch(EnemyController owner, EnemyPatternLifetime lifetime, uint generation,
            Vector3 direction, EnemyDamageRequest damage, float speed, float duration, float radius,
            Sprite puff, Sprite circle, Color color)
        {
            direction.y = 0;
            if (!TryGetLaunchPosition(owner, direction, radius, out Vector3 launch)) return null;
            var go = new GameObject("GasSac_B_Orb");
            go.transform.position = launch;
            var orb = go.AddComponent<GasSacOrb>();
            orb.LaunchPosition = launch;
            orb.lifetime = lifetime; orb.generation = generation; orb.direction = direction.normalized;
            orb.damage = damage; orb.speed = speed; orb.remaining = duration; orb.radius = radius;
            lifetime.Own(go, item => { item.SetActive(false); Destroy(item); });
            orb.body = new GameObject("CompressedGas").transform; orb.body.SetParent(go.transform, false);
            AddLayer(orb.body, "Outline", circle, new Color(.22f, .12f, .24f, 1), radius * 1.1f, Vector3.zero, 5000, true);
            AddLayer(orb.body, "Gas", puff, new Color(color.r, color.g, color.b, 1), radius * 2, Vector3.zero, 5001, true);
            AddLayer(orb.body, "Highlight", circle, new Color(1, .98f, .76f, .95f), radius * .23f,
                new Vector3(-radius * .25f, radius * .3f, -.01f), 5002, true);
            orb.shadow = AddLayer(go.transform, "GroundShadow", circle, new Color(.15f, .07f, .18f, .35f),
                radius, Vector3.zero, 65).transform;
            orb.UpdateVisual();
            PlayerController player = PlayerController.Instance;
            if (player != null && !player.IsDead && !MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position)
                && HasClearPath(launch, player.transform.position) && SweepTouchesPlayer(owner.transform.position, launch, radius, player))
            { player.TakeDamage(damage); orb.Despawn(); }
            return orb;
        }

        private static SpriteRenderer AddLayer(Transform parent, string name, Sprite sprite, Color color, float scale, Vector3 offset, int order, bool sortWithWorld = false)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            go.transform.localPosition = offset; go.transform.localScale = Vector3.one * scale;
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = color; renderer.sortingOrder = order;
            if (sortWithWorld)
            {
                var sorting = go.AddComponent<SpriteYSort>();
                sorting.Configure(order, true, SpriteYSort.WorldDynamicMinSortingOrder + order - SpriteYSort.WorldDynamicBaseSortingOrder);
                sorting.SetUpdateMode(SpriteYSort.UpdateMode.Continuous);
            }
            return renderer;
        }

        private void Update()
        {
            if (finished) return;
            if (lifetime == null || !lifetime.IsCurrent(generation) || remaining <= 0) { Despawn(); return; }
            float stepTime = Mathf.Min(Time.deltaTime, remaining); remaining -= stepTime;
            Vector3 start = transform.position, end = start + direction * (speed * stepTime);
            // Subdivide only the swept path, not game time, so low frame rates cannot tunnel through a wall or player.
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(start, end) / .15f));
            PlayerController player = PlayerController.Instance;
            for (int i = 1; i <= steps; i++)
            {
                Vector3 point = Vector3.Lerp(start, end, i / (float)steps);
                if (!HasClearFlightPath(transform.position, point, radius) || !IsVisible(GetFlightPosition(point, FlightHeight))) { Despawn(); return; }
                Vector3 previous = transform.position; transform.position = point;
                if (player != null && !player.IsDead && !MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position)
                    && HasClearPath(point, player.transform.position) && SweepTouchesPlayer(previous, point, radius, player))
                {
                    player.TakeDamage(damage);
                    Despawn(); return;
                }
            }
            UpdateVisual();
            if (remaining <= 0) Despawn();
        }

        internal static bool SweepTouchesPlayer(Vector3 from, Vector3 to, float radius, PlayerController player)
            => CombatHitGeometry.SweepTouchesPlayer(from, to, radius, player);

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
                    || MidBossArenaController.IsPlayerInsideLockedArena(point)) return false;
            }
            return true;
        }

        private static bool HasClearFlightPath(Vector3 from, Vector3 to, float radius)
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

        private void UpdateVisual()
        {
            Vector3 ground = transform.position;
            if (BiomeManager.Active != null) ground.y = BiomeManager.Active.GetGroundHeight(ground);
            body.position = ground + Vector3.up * FlightHeight;
            Camera camera = DontStarveCamera.GetActiveCamera();
            if (camera != null) body.rotation = camera.transform.rotation;
            shadow.position = ground + Vector3.up * .04f;
            shadow.rotation = Quaternion.Euler(90, 0, 0);
            shadow.localScale = new Vector3(radius, radius * .65f, 1);
        }

        private void Despawn()
        {
            if (finished) return;
            finished = true; gameObject.SetActive(false);
            if (lifetime != null) lifetime.ReleaseOwned(gameObject);
            else Destroy(gameObject);
        }
    }
}
