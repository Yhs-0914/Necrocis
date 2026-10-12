using UnityEngine;

namespace Necrocis
{
    // One owned moving area. It keeps travelling after one hit but never deals repeated ticks.
    public sealed class DustCloud : MonoBehaviour
    {
        private EnemyPatternLifetime lifetime;
        private uint generation;
        private EnemyDamageRequest damage;
        private SpriteRenderer body;
        private Sprite[] movingFrames, dissolveFrames;
        private GameObject marker;
        private float radius, speed, duration, began, dissolveAt = -1, frameSeconds, dissolveSeconds, baseOpacity, overlapOpacity;
        public Vector3 Direction { get; private set; }
        public Vector3 LaunchPosition { get; private set; }
        public float Radius => radius;
        public float Speed => speed;
        public int HitAttempts { get; private set; }
        public int DamageApplications { get; private set; }
        public bool IsDissolving => dissolveAt >= 0;
        public SpriteRenderer Body => body;
        public bool IsPlayerOccluded { get; private set; }

        public static DustCloud Launch(EnemyPatternLifetime lifetime, uint generation, Vector3 position, Vector3 direction,
            EnemyDamageRequest damage, GameObject marker, float radius, float speed, float duration,
            Sprite[] moving, Sprite[] dissolve, float frameSeconds, float dissolveSeconds, float opacity, float overlapOpacity = .35f)
        {
            var go = new GameObject("P01_MovingDustCloud"); go.transform.position = position;
            var cloud = go.AddComponent<DustCloud>(); cloud.lifetime = lifetime; cloud.generation = generation;
            cloud.LaunchPosition = position; cloud.Direction = direction; cloud.damage = damage; cloud.marker = marker;
            cloud.radius = radius; cloud.speed = speed; cloud.duration = duration; cloud.began = Time.time;
            cloud.movingFrames = (Sprite[])moving.Clone(); cloud.dissolveFrames = (Sprite[])dissolve.Clone();
            cloud.frameSeconds = frameSeconds; cloud.dissolveSeconds = dissolveSeconds; cloud.baseOpacity = opacity; cloud.overlapOpacity = overlapOpacity;
            cloud.body = CreateVisual(go.transform, moving, radius, opacity); cloud.UpdateView();
            lifetime.Own(go, item => { item.SetActive(false); Destroy(item); });
            cloud.TryHit(position, position); return cloud;
        }

        public static SpriteRenderer CreateVisual(Transform parent, Sprite[] frames, float radius, float opacity)
        {
            var body = new GameObject("DustSprite").AddComponent<SpriteRenderer>(); body.transform.SetParent(parent, false);
            body.sprite = frames[0]; body.color = new Color(1, 1, 1, opacity);
            float width = 0; foreach (var f in frames) width = Mathf.Max(width, f.bounds.size.x);
            body.transform.localScale = Vector3.one * (radius * 2 / width);
            var sorting = parent.gameObject.AddComponent<SpriteYSort>();
            sorting.Configure(SpriteYSort.WorldDynamicBaseSortingOrder, true, SpriteYSort.WorldDynamicMinSortingOrder);
            sorting.SetUpdateMode(SpriteYSort.UpdateMode.Continuous); return body;
        }

        public static bool CanTravel(Vector3 from, Vector3 to, float radius)
        {
            if (!GasSacOrb.HasClearPath(from, to) || !ResidueRubble.CanTraverse(from, to, new Vector2(radius, radius))) return false;
            var biome = BiomeManager.Active; if (biome == null) return true;
            var first = biome.WorldToGrid(from); int level = biome.GetHeightLevel(first.x, first.y);
            var min = biome.WorldToGrid(new Vector3(Mathf.Min(from.x, to.x) - radius, 0, Mathf.Min(from.z, to.z) - radius));
            var max = biome.WorldToGrid(new Vector3(Mathf.Max(from.x, to.x) + radius, 0, Mathf.Max(from.z, to.z) + radius));
            for (int z = min.y; z <= max.y; z++) for (int x = min.x; x <= max.x; x++)
            {
                var center = biome.GridToWorld(x, z);
                if (biome.IsValidPosition(x, z) && biome.IsWalkable(x, z) && biome.GetHeightLevel(x, z) == level
                    && !MidBossArenaController.IsPlayerInsideLockedArena(center)) continue;
                var bounds = new Bounds(center, new Vector3(biome.TileSize, 1, biome.TileSize));
                if (CombatHitGeometry.SegmentRectDistanceSquared(CombatHitGeometry.Flat(from), CombatHitGeometry.Flat(to), bounds) <= radius * radius) return false;
            }
            return true;
        }

        private void Update()
        {
            if (lifetime == null || !lifetime.IsCurrent(generation)) { Finish(); return; }
            if (dissolveAt >= 0)
            {
                int frame = Mathf.FloorToInt((Time.time - dissolveAt) / dissolveSeconds);
                if (frame >= dissolveFrames.Length) { Finish(); return; }
                body.sprite = dissolveFrames[frame]; UpdateView(); return;
            }
            float elapsed = Time.time - began;
            float dt = Mathf.Min(Time.deltaTime, Mathf.Max(0, duration - (elapsed - Time.deltaTime)));
            Vector3 from = transform.position, end = from + Direction * (speed * dt);
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(from, end) / .1f));
            for (int i = 1; i <= steps; i++)
            {
                var next = Vector3.Lerp(from, end, i / (float)steps);
                if (!CanTravel(transform.position, next, radius)) { BeginDissolve(); return; }
                Vector3 previous = transform.position; transform.position = next; TryHit(previous, next);
            }
            body.sprite = movingFrames[Mathf.FloorToInt(elapsed / frameSeconds) % movingFrames.Length]; UpdateView();
            if (elapsed >= duration) BeginDissolve();
        }
        private void TryHit(Vector3 from, Vector3 to)
        {
            var player = PlayerController.Instance;
            if (HitAttempts != 0 || player == null || player.IsDead || MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position)
                || !GasSacOrb.HasClearPath(to, player.transform.position)) return;
            if (CombatHitGeometry.PointSegmentDistanceSquared(CombatHitGeometry.Flat(player.transform.position), CombatHitGeometry.Flat(from), CombatHitGeometry.Flat(to)) > radius * radius) return;
            HitAttempts++; float hp = player.HealthComponent.CurrentHealth;
            player.TakeDamage(damage);
            if (player.HealthComponent.CurrentHealth < hp) DamageApplications++;
        }
        private void BeginDissolve()
        {
            if (dissolveAt >= 0) return;
            dissolveAt = Time.time; if (marker != null) marker.SetActive(false); body.sprite = dissolveFrames[0]; UpdateView();
        }
        private void UpdateView()
        {
            var p = transform.position; if (BiomeManager.Active != null) p.y = BiomeManager.Active.GetGroundHeight(p);
            transform.position = p;
            var camera = DontStarveCamera.GetActiveCamera(); if (camera != null) body.transform.rotation = camera.transform.rotation;
            body.transform.position = p;
            if (marker != null) marker.transform.position = p + Vector3.up * .055f;
        }
        private void LateUpdate()
        {
            if (body == null) return;
            UpdateView(); IsPlayerOccluded = ApplyPlayerOverlapOpacity(body, baseOpacity, overlapOpacity);
        }
        public static bool ApplyPlayerOverlapOpacity(SpriteRenderer dust, float opacity, float overlapOpacity)
        {
            var player = PlayerController.Instance;
            var target = player != null ? player.GetComponentInChildren<SpriteRenderer>() : null;
            var camera = DontStarveCamera.GetActiveCamera();
            bool overlap = target != null && target.enabled && target.sprite != null && camera != null
                && ScreenRect(dust, camera).Overlaps(ScreenRect(target, camera));
            Color color = dust.color;
            color.a = Mathf.MoveTowards(color.a, overlap ? Mathf.Min(opacity, overlapOpacity) : opacity, Time.deltaTime * 6);
            dust.color = color; return overlap;
        }
        private static Rect ScreenRect(SpriteRenderer renderer, Camera camera)
        {
            Bounds b = renderer.sprite.bounds; Vector2 min = Vector2.one * float.PositiveInfinity, max = Vector2.one * float.NegativeInfinity;
            foreach (float x in new[] { b.min.x, b.max.x }) foreach (float y in new[] { b.min.y, b.max.y })
            {
                Vector3 local = new Vector3(renderer.flipX ? -x : x, renderer.flipY ? -y : y, 0);
                Vector2 point = camera.WorldToViewportPoint(renderer.transform.TransformPoint(local)); min = Vector2.Min(min, point); max = Vector2.Max(max, point);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
        private void Finish()
        {
            if (marker != null) marker.SetActive(false);
            gameObject.SetActive(false); if (lifetime != null) lifetime.ReleaseOwned(gameObject); else Destroy(gameObject);
        }
    }
}
