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
            if (EnemyWorldBindings.Surface != null) position.y = EnemyWorldBindings.Surface.GetGroundHeight(position);
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
            if (player != null && !player.IsDead && !EnemyWorldBindings.Encounters.IsInsideLockedArena(player.transform.position)
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
                if (player != null && !player.IsDead && !EnemyWorldBindings.Encounters.IsInsideLockedArena(player.transform.position)
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

        public static Vector3 GetFlightPosition(Vector3 position, float height)
            => EnemyWorldBindings.Attacks.GetFlightPosition(position, height);
        public static bool IsVisible(Vector3 point) => EnemyWorldBindings.Attacks.IsVisible(point);
        public static bool HasClearPath(Vector3 from, Vector3 to) => EnemyWorldBindings.Attacks.HasClearPath(from, to);
        private static bool HasClearFlightPath(Vector3 from, Vector3 to, float radius)
            => EnemyWorldBindings.Attacks.HasClearFlightPath(from, to, radius);

        private void UpdateVisual()
        {
            Vector3 ground = transform.position;
            if (EnemyWorldBindings.Surface != null) ground.y = EnemyWorldBindings.Surface.GetGroundHeight(ground);
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
