using UnityEngine;

namespace Necrocis
{
    public sealed class EmberThorn : MonoBehaviour
    {
        private EnemyPatternLifetime lifetime;
        private uint generation;
        private EnemyDamageRequest damage;
        private Vector3 direction;
        private float speed, remaining, length, radius, frameSeconds, elapsed;
        private Sprite[] frames;
        private SpriteRenderer body;
        private bool finished;
        public Vector3 Direction => direction;
        public float Speed => speed;
        public float Length => length;
        public float HitRadius => radius;
        public float FlightHeight => radius;
        public Vector3 LaunchPosition { get; private set; }

        public static EmberThorn Launch(EnemyController owner, EnemyPatternLifetime lifetime, uint generation,
            Vector3 direction, EnemyDamageRequest damage, float speed, float duration, float length, float radius,
            Sprite[] frames, float frameSeconds)
        {
            direction.y = 0; direction.Normalize();
            var collider = owner.GetComponent<Collider>();
            Vector3 extents = collider != null ? collider.bounds.extents : Vector3.zero;
            float bodyExtent = Mathf.Abs(direction.x) * extents.x + Mathf.Abs(direction.z) * extents.z;
            // Direction data adjusts the ground origin, never the thorn's low flight height.
            Vector3 origin = owner.GetPatternVisualOrigin(); origin.y = owner.transform.position.y;
            Vector3 launch = origin + direction * (bodyExtent + radius);
            if (BiomeManager.Active != null) launch.y = BiomeManager.Active.GetGroundHeight(launch);
            Vector3 side = new Vector3(-direction.z, 0, direction.x) * radius;
            Vector3 tip = launch + direction * (length * .5f);
            if (!GasSacOrb.HasClearPath(owner.transform.position, tip)
                || !GasSacOrb.HasClearPath(owner.transform.position + side, tip + side)
                || !GasSacOrb.HasClearPath(owner.transform.position - side, tip - side)) return null;
            var go = new GameObject("H02_CounterThorn"); go.transform.position = launch;
            var thorn = go.AddComponent<EmberThorn>();
            thorn.lifetime = lifetime; thorn.generation = generation; thorn.damage = damage;
            thorn.direction = direction.normalized; thorn.speed = speed; thorn.remaining = duration;
            thorn.length = length; thorn.radius = radius; thorn.frames = (Sprite[])frames.Clone(); thorn.frameSeconds = frameSeconds;
            thorn.LaunchPosition = launch;
            thorn.body = new GameObject("ThornSprite").AddComponent<SpriteRenderer>(); thorn.body.transform.SetParent(go.transform, false);
            var ownerBody = owner.transform.Find("Visual")?.GetComponent<SpriteRenderer>();
            thorn.body.sortingOrder = ownerBody != null ? ownerBody.sortingOrder : SpriteYSort.WorldDynamicBaseSortingOrder;
            var sorting = go.AddComponent<SpriteYSort>();
            sorting.Configure(SpriteYSort.WorldDynamicBaseSortingOrder, true, SpriteYSort.WorldDynamicMinSortingOrder);
            sorting.SetUpdateMode(SpriteYSort.UpdateMode.Continuous);
            lifetime.Own(go, item => { item.SetActive(false); Destroy(item); });
            thorn.UpdateVisual(); return thorn;
        }

        private void Update()
        {
            if (finished) return;
            if (lifetime == null || !lifetime.IsCurrent(generation) || remaining <= 0) { Despawn(); return; }
            float dt = Mathf.Min(Time.deltaTime, remaining); remaining -= dt; elapsed += dt;
            Vector3 start = transform.position, end = start + direction * (speed * dt);
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(start, end) / .15f));
            float halfLine = Mathf.Max(0, length * .5f - radius);
            Vector3 side = new Vector3(-direction.z, 0, direction.x) * radius;
            var player = PlayerController.Instance;
            for (int i = 1; i <= steps; i++)
            {
                Vector3 point = Vector3.Lerp(start, end, i / (float)steps), previous = transform.position;
                Vector3 rear = previous - direction * halfLine, tip = point + direction * halfLine;
                Vector3 terrainRear = previous - direction * (length * .5f), terrainTip = point + direction * (length * .5f);
                if (!GasSacOrb.HasClearPath(terrainRear, terrainTip) || !GasSacOrb.HasClearPath(terrainRear + side, terrainTip + side)
                    || !GasSacOrb.HasClearPath(terrainRear - side, terrainTip - side) || !GasSacOrb.IsVisible(GasSacOrb.GetFlightPosition(point, FlightHeight)))
                { Despawn(); return; }
                transform.position = point;
                if (player != null && !player.IsDead && !MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position)
                    && GasSacOrb.HasClearPath(point, player.transform.position)
                    && GasSacOrb.SweepTouchesPlayer(rear, tip, radius, player))
                { player.TakeDamage(damage); Despawn(); return; }
            }
            UpdateVisual(); if (remaining <= 0) Despawn();
        }

        private void UpdateVisual()
        {
            body.sprite = frames[Mathf.FloorToInt(elapsed / frameSeconds) % frames.Length];
            body.transform.position = GasSacOrb.GetFlightPosition(transform.position, FlightHeight);
            Camera camera = DontStarveCamera.GetActiveCamera();
            // Keep the long axis on the actual flight plane. Rotating a full billboard lets long tips pierce the floor.
            Vector3 up = Vector3.Cross(camera != null ? camera.transform.forward : Vector3.down, direction);
            if (up.sqrMagnitude < .0001f) up = Vector3.Cross(Vector3.up, direction);
            up.Normalize();
            body.transform.rotation = Quaternion.LookRotation(Vector3.Cross(direction, up), up);
            body.transform.localScale = new Vector3(length / body.sprite.bounds.size.x, radius * 2 / body.sprite.bounds.size.y, 1);
        }
        private void Despawn()
        {
            if (finished) return; finished = true; gameObject.SetActive(false);
            if (lifetime != null) lifetime.ReleaseOwned(gameObject); else Destroy(gameObject);
        }
    }
}
