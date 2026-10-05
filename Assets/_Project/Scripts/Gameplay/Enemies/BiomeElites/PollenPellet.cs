using UnityEngine;

namespace Necrocis
{
    // All waves in one cycle share one attempt, including an attempt absorbed by dash invulnerability.
    public sealed class PollenCycleHitBudget
    {
        public int HitAttempts { get; private set; }
        public int DamageApplications { get; private set; }
        public void TryHit(PlayerController player, EnemyDamageRequest damage)
        {
            if (HitAttempts != 0 || player == null || player.IsDead) return;
            HitAttempts++;
            float before = player.HealthComponent.CurrentHealth;
            player.TakeDamage(damage);
            if (player.HealthComponent.CurrentHealth < before) DamageApplications++;
        }
    }

    public sealed class PollenVolley
    {
        private readonly EnemyDamageRequest damage;
        private readonly PollenCycleHitBudget budget;
        public int HitAttempts => budget.HitAttempts;
        public int DamageApplications => budget.DamageApplications;
        public int WaveHitAttempts { get; private set; }
        public int WaveDamageApplications { get; private set; }
        public PollenVolley(EnemyDamageRequest request, PollenCycleHitBudget sharedBudget = null)
        { damage = request; budget = sharedBudget ?? new PollenCycleHitBudget(); }
        public void TryHit(PlayerController player)
        {
            WaveHitAttempts++; int before = budget.DamageApplications;
            budget.TryHit(player, damage); WaveDamageApplications += budget.DamageApplications - before;
        }
    }

    [DefaultExecutionOrder(100)]
    public sealed class PollenPellet : MonoBehaviour
    {
        private EnemyPatternLifetime lifetime;
        private uint generation;
        private PollenVolley volley;
        private Sprite[] frames;
        private SpriteRenderer body;
        private SpriteRenderer ownerBody;
        private SpriteYSort sorting;
        private bool frontOpening, openingExited;
        private float remaining, elapsed, frameSeconds;
        private bool finished;
        public Vector3 Direction { get; private set; }
        public Vector3 LaunchPosition { get; private set; }
        public float Radius { get; private set; }
        public float Speed { get; private set; }
        public float Duration { get; private set; }
        public SpriteRenderer Body => body;
        public bool UsesOpeningSort { get; private set; }

        public static bool TryGetLaunchPosition(EnemyController owner, float radius, out Vector3 position)
        {
            position = owner.GetPatternVisualOrigin();
            var camera = DontStarveCamera.GetActiveCamera();
            if (camera == null || Mathf.Abs(camera.transform.forward.y) < .1f) return false;
            float ground = BiomeManager.Active != null ? BiomeManager.Active.GetGroundHeight(owner.transform.position) : owner.transform.position.y;
            // Project the authored opening along the viewing ray onto the low flight plane.
            // Sprite and damage start at this same point; there is no invisible bridge hit from the owner.
            position += camera.transform.forward * ((ground + radius - position.y) / camera.transform.forward.y);
            position.y = ground;
            return DustCloud.CanTravel(owner.transform.position, position, radius)
                && GasSacOrb.IsVisible(GasSacOrb.GetFlightPosition(position, radius));
        }

        public static PollenPellet Launch(EnemyController owner, EnemyPatternLifetime lifetime, uint generation, PollenVolley volley,
            Vector3 origin, Vector3 direction, float speed, float duration, float radius, Sprite[] frames, float frameSeconds)
        {
            var go = new GameObject("P03_PollenPellet"); go.transform.position = origin;
            var pellet = go.AddComponent<PollenPellet>();
            pellet.lifetime = lifetime; pellet.generation = generation; pellet.volley = volley;
            pellet.LaunchPosition = origin; direction.y = 0; pellet.Direction = direction.normalized;
            pellet.Speed = speed; pellet.Duration = pellet.remaining = duration; pellet.Radius = radius;
            pellet.frames = frames; pellet.frameSeconds = frameSeconds;
            pellet.ownerBody = owner.transform.Find("Visual").GetComponent<SpriteRenderer>();
            pellet.frontOpening = owner.PatternFacing != EnemyFacing.Back;
            pellet.body = new GameObject("PelletSprite").AddComponent<SpriteRenderer>(); pellet.body.transform.SetParent(go.transform, false);
            pellet.sorting = go.AddComponent<SpriteYSort>();
            pellet.sorting.Configure(SpriteYSort.WorldDynamicBaseSortingOrder, true, SpriteYSort.WorldDynamicMinSortingOrder);
            pellet.sorting.SetUpdateMode(SpriteYSort.UpdateMode.Continuous);
            lifetime.Own(go, item => { item.SetActive(false); Destroy(item); });
            pellet.UpdateVisual(); return pellet;
        }
        private void Update()
        {
            if (finished) return;
            if (lifetime == null || !lifetime.IsCurrent(generation) || remaining <= 0) { Finish(); return; }
            float dt = Mathf.Min(Time.deltaTime, remaining); remaining -= dt; elapsed += dt;
            Vector3 from = transform.position, end = from + Direction * (Speed * dt);
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(from, end) / .1f));
            for (int i = 1; i <= steps; i++)
            {
                var next = Vector3.Lerp(from, end, i / (float)steps);
                if (!DustCloud.CanTravel(transform.position, next, Radius)
                    || !GasSacOrb.IsVisible(GasSacOrb.GetFlightPosition(next, Radius))) { Finish(); return; }
                var previous = transform.position; transform.position = next;
                var player = PlayerController.Instance;
                if (player != null && !player.IsDead && !MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position)
                    && GasSacOrb.HasClearPath(next, player.transform.position)
                    && GasSacOrb.SweepTouchesPlayer(previous, next, Radius, player))
                { volley.TryHit(player); Finish(); return; }
            }
            UpdateVisual(); if (remaining <= 0) Finish();
        }
        private void LateUpdate()
        {
            if (finished) return;
            UpdateVisual();
            bool overlaps = false;
            if (frontOpening && !openingExited && ownerBody != null && ownerBody.sprite != null)
            {
                Vector3 p = ownerBody.transform.InverseTransformPoint(body.transform.position);
                Bounds bounds = ownerBody.sprite.bounds;
                if (ownerBody.flipX) p.x = -p.x;
                overlaps = p.x >= bounds.min.x && p.x <= bounds.max.x && p.y >= bounds.min.y && p.y <= bounds.max.y;
            }
            // Only a front/side muzzle can draw across its own near shell. Back shots retain depth occlusion.
            if (overlaps) body.sortingOrder = Mathf.Max(body.sortingOrder, ownerBody.sortingOrder + 1);
            else if (UsesOpeningSort) sorting.SetUpdateMode(SpriteYSort.UpdateMode.Continuous);
            // A slow shot may overlap longer; once it has left, later owner movement cannot re-enable the override.
            if (!overlaps) openingExited = true;
            UsesOpeningSort = overlaps;
        }
        private void UpdateVisual()
        {
            body.sprite = frames[Mathf.FloorToInt(elapsed / frameSeconds) % frames.Length];
            body.transform.position = GasSacOrb.GetFlightPosition(transform.position, Radius);
            var camera = DontStarveCamera.GetActiveCamera(); if (camera != null) body.transform.rotation = camera.transform.rotation;
            float diameter = Mathf.Max(frames[0].bounds.size.x, frames[0].bounds.size.y);
            body.transform.localScale = Vector3.one * (Radius * 2 / diameter);
        }
        private void Finish()
        {
            if (finished) return;
            finished = true; gameObject.SetActive(false);
            if (lifetime != null) lifetime.ReleaseOwned(gameObject); else Destroy(gameObject);
        }
    }
}
