using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>A spawn-owned prop: movement blocker and breakable target, never an enemy or XP source.</summary>
    public sealed class ResidueRubble : MonoBehaviour
    {
        private static readonly List<ResidueRubble> Active = new List<ResidueRubble>();
        private static int attackSequence;
        private readonly HashSet<int> receivedAttacks = new HashSet<int>();
        private EnemyPatternLifetime lifetime;
        private uint generation;
        private Vector3 forward, right, landingVisualOffset;
        private Vector2 halfSize;
        private SpriteRenderer visual;
        private Sprite landedSprite;
        private float landedScale;
        private BoxCollider hitCollider;
        private float expiresAt;
        private int remainingHits;
        private bool landed, released;
        public bool IsBlocking => landed && !released && gameObject.activeInHierarchy && lifetime != null && lifetime.IsCurrent(generation);
        public Vector2 Footprint => halfSize * 2;
        public int RemainingHits => remainingHits;
        public Vector3 Forward => forward;
        public Vector3 LandingVisualPosition => transform.position + landingVisualOffset;
        public static int ActiveBlockCount
        {
            get { int count = 0; foreach (var prop in Active) if (prop != null && prop.IsBlocking) count++; return count; }
        }
        public static int NextAttackToken() => unchecked(++attackSequence);

        public static ResidueRubble Create(EnemyController owner, EnemyPatternLifetime life, Vector3 center,
            Vector3 forward, Vector2 size, Sprite sprite, int hits, Sprite airborneChip = null,
            Vector3? launchPosition = null, Vector3? launchScale = null, float groundDepthFraction = 0)
        {
            var go = new GameObject("I02_ResidueRubble") { layer = owner.gameObject.layer };
            go.transform.position = center;
            var prop = go.AddComponent<ResidueRubble>();
            prop.lifetime = life; prop.generation = owner.SpawnGeneration;
            prop.forward = forward.normalized; prop.right = Vector3.Cross(Vector3.up, prop.forward);
            prop.halfSize = size * .5f; prop.remainingHits = hits;
            var spriteObject = new GameObject("CrustSlab"); spriteObject.transform.SetParent(go.transform, false);
            prop.visual = spriteObject.AddComponent<SpriteRenderer>(); prop.visual.sprite = airborneChip != null ? airborneChip : sprite;
            prop.landedSprite = sprite;
            Camera camera = DontStarveCamera.GetActiveCamera();
            Vector3 screenRight = camera != null ? Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized : Vector3.right;
            if (screenRight.sqrMagnitude < .0001f) screenRight = Vector3.right;
            Vector3 screenUp = Vector3.Cross(screenRight, Vector3.up);
            float width = Mathf.Abs(Vector3.Dot(prop.right, screenRight)) * size.x + Mathf.Abs(Vector3.Dot(prop.forward, screenRight)) * size.y;
            float depth = Mathf.Abs(Vector3.Dot(prop.right, screenUp)) * size.x + Mathf.Abs(Vector3.Dot(prop.forward, screenUp)) * size.y;
            prop.landingVisualOffset = -screenUp * depth * .5f;
            // The vertical artwork includes raised top/side faces above its ground span.
            // Fit that span, not the whole silhouette, and keep square pixels with uniform scale.
            float groundProjection = camera != null ? Mathf.Abs(Vector3.Dot(screenUp, camera.transform.up)) : .7071068f;
            float scale = groundDepthFraction > 0
                ? depth * Mathf.Max(.01f, groundProjection) / Mathf.Max(.01f, sprite.bounds.size.y * groundDepthFraction)
                : width / Mathf.Max(.01f, sprite.bounds.size.x);
            prop.landedScale = scale;
            spriteObject.transform.localScale = airborneChip != null ? launchScale ?? Vector3.one : Vector3.one * scale;
            var sort = spriteObject.AddComponent<SpriteYSort>();
            sort.Configure(SpriteYSort.WorldDynamicBaseSortingOrder, true, SpriteYSort.WorldDynamicMinSortingOrder);
            sort.SetUpdateMode(SpriteYSort.UpdateMode.Continuous);
            prop.hitCollider = go.AddComponent<BoxCollider>(); prop.hitCollider.isTrigger = true;
            prop.hitCollider.center = Vector3.up * .6f; prop.hitCollider.size = new Vector3(size.x, 2f, size.y);
            go.transform.rotation = Quaternion.LookRotation(prop.forward);
            prop.hitCollider.enabled = false;
            life.Own(go, item => { var rubble = item.GetComponent<ResidueRubble>(); rubble?.Deactivate(); Destroy(item); });
            if (launchPosition.HasValue) prop.SetAirPosition(launchPosition.Value);
            else prop.SetDropHeight(1.2f);
            return prop;
        }

        public void SetDropHeight(float height)
        {
            SetAirPosition(LandingVisualPosition + Vector3.up * height);
        }

        public void SetAirPosition(Vector3 position)
        {
            visual.transform.position = position;
            Camera camera = DontStarveCamera.GetActiveCamera();
            if (camera != null) visual.transform.rotation = camera.transform.rotation;
        }

        public void Land(float seconds)
        {
            if (released || lifetime == null || !lifetime.IsCurrent(generation)) return;
            visual.sprite = landedSprite;
            visual.transform.localScale = Vector3.one * landedScale;
            SetDropHeight(0); landed = true; expiresAt = Time.time + seconds;
            hitCollider.enabled = true; Active.Add(this);
        }

        private void Update()
        {
            if (released) return;
            if (lifetime == null || !lifetime.IsCurrent(generation) || (landed && Time.time >= expiresAt)) Release();
        }

        public bool ApplyPlayerHit(float damage, int attackToken)
        {
            if (!IsBlocking || !MonsterBalanceNumbers.Positive(damage) || !receivedAttacks.Add(attackToken)) return false;
            remainingHits--;
            if (remainingHits <= 0) Release();
            return true;
        }

        public void Release()
        {
            if (released) return;
            Deactivate();
            if (lifetime != null) lifetime.ReleaseOwned(gameObject);
            else Destroy(gameObject);
        }

        private void Deactivate()
        {
            released = true; landed = false; Active.Remove(this);
            if (hitCollider != null) hitCollider.enabled = false;
            gameObject.SetActive(false);
        }

        private void OnDisable() => Active.Remove(this);
        private void OnDestroy() => Active.Remove(this);

        private Vector2 Local(Vector3 point)
        {
            Vector3 delta = point - transform.position; delta.y = 0;
            return new Vector2(Vector3.Dot(delta, right), Vector3.Dot(delta, forward));
        }

        public bool ContainsPoint(Vector3 point)
        {
            Vector2 local = Local(point);
            return Mathf.Abs(local.x) <= halfSize.x && Mathf.Abs(local.y) <= halfSize.y;
        }

        public static bool CanOccupy(Vector3 point, Vector2 moverHalfExtents)
        {
            foreach (ResidueRubble prop in Active)
            {
                if (prop == null || !prop.IsBlocking) continue;
                Vector2 p = prop.Local(point);
                float x = prop.halfSize.x + Mathf.Abs(prop.right.x) * moverHalfExtents.x + Mathf.Abs(prop.right.z) * moverHalfExtents.y;
                float z = prop.halfSize.y + Mathf.Abs(prop.forward.x) * moverHalfExtents.x + Mathf.Abs(prop.forward.z) * moverHalfExtents.y;
                if (Mathf.Abs(p.x) <= x && Mathf.Abs(p.y) <= z) return false;
            }
            return true;
        }

        public static bool CanTraverse(Vector3 from, Vector3 to, Vector2 moverHalfExtents)
        {
            foreach (ResidueRubble prop in Active)
            {
                if (prop == null || !prop.IsBlocking) continue;
                BiomeManager biome = BiomeManager.Active;
                if (biome != null)
                {
                    Vector2Int cell = biome.WorldToGrid(prop.transform.position), player = biome.WorldToGrid(from);
                    if (biome.GetHeightLevel(cell.x, cell.y) != biome.GetHeightLevel(player.x, player.y)) continue;
                }
                Vector2 expansion = new Vector2(Mathf.Abs(prop.right.x) * moverHalfExtents.x + Mathf.Abs(prop.right.z) * moverHalfExtents.y,
                    Mathf.Abs(prop.forward.x) * moverHalfExtents.x + Mathf.Abs(prop.forward.z) * moverHalfExtents.y);
                Vector2 size = prop.halfSize + expansion, a = prop.Local(from), b = prop.Local(to);
                float penetration = Mathf.Min(size.x - Mathf.Abs(a.x), size.y - Mathf.Abs(a.y));
                if (penetration >= 0)
                {
                    // A slab may land on the player. Permit escape, without allowing deeper movement through it.
                    Vector2 normalizedFrom = new Vector2(a.x / size.x, a.y / size.y), normalizedTo = new Vector2(b.x / size.x, b.y / size.y);
                    if (normalizedTo.sqrMagnitude > normalizedFrom.sqrMagnitude + .000001f) continue;
                    if ((b - a).sqrMagnitude < .000001f) continue;
                    return false;
                }
                if (CrossesRectangle(a, b, size)) return false;
            }
            return true;
        }

        public static void HitAlongSegment(Vector3 from, Vector3 to, float radius, float damage, LayerMask mask, int attackToken)
        {
            if (damage <= 0) return;
            for (int i = Active.Count - 1; i >= 0; i--)
            {
                ResidueRubble prop = Active[i];
                if (prop == null || !prop.IsBlocking || (mask.value & (1 << prop.gameObject.layer)) == 0) continue;
                if (CrossesRectangle(prop.Local(from), prop.Local(to), prop.halfSize + Vector2.one * Mathf.Max(0, radius)))
                    prop.ApplyPlayerHit(damage, attackToken);
            }
        }

        public static void HitArea(Vector3 center, float radius, Vector3 facing, float halfAngle, float damage, LayerMask mask, int attackToken)
        {
            for (int i = Active.Count - 1; i >= 0; i--)
            {
                ResidueRubble prop = Active[i];
                if (prop == null || !prop.IsBlocking || (mask.value & (1 << prop.gameObject.layer)) == 0) continue;
                Vector2 local = prop.Local(center);
                Vector2 closest = new Vector2(Mathf.Clamp(local.x, -prop.halfSize.x, prop.halfSize.x), Mathf.Clamp(local.y, -prop.halfSize.y, prop.halfSize.y));
                if ((local - closest).sqrMagnitude > radius * radius) continue;
                Vector3 delta = prop.transform.position - center; delta.y = 0;
                if (halfAngle < 180 && delta.sqrMagnitude > .0001f && Vector3.Angle(facing, delta) > halfAngle) continue;
                prop.ApplyPlayerHit(damage, attackToken);
            }
        }

        private static bool CrossesRectangle(Vector2 from, Vector2 to, Vector2 half)
        {
            float near = 0, far = 1; Vector2 delta = to - from;
            return Clip(from.x, delta.x, half.x, ref near, ref far) && Clip(from.y, delta.y, half.y, ref near, ref far);
        }

        private static bool Clip(float start, float delta, float half, ref float near, ref float far)
        {
            if (Mathf.Abs(delta) < .000001f) return Mathf.Abs(start) <= half;
            float a = (-half - start) / delta, b = (half - start) / delta;
            near = Mathf.Max(near, Mathf.Min(a, b)); far = Mathf.Min(far, Mathf.Max(a, b));
            return near <= far;
        }
    }
}
