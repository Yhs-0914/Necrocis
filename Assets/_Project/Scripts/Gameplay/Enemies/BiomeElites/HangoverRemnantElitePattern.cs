using System.Collections;
using UnityEngine;

namespace Necrocis
{
    public enum HangoverRemnantPhase { Inactive, Ready, Windup, Retreat, Recovery }

    [DisallowMultipleComponent]
    public sealed class HangoverRemnantElitePattern : MonsterPatternController
    {
        private EnemyController enemy;
        private EnemyPatternLifetime lifetime;
        private uint generation;
        private Vector3 anchor, center, retreatDirection;
        private float triggerDistance, windup, retreatDistance, retreatSpeed, delay, radius, recoverySeconds, rearm, nextReady, nextSafetyCheck, hitUntil;
        private float idleTime, moveTime, releaseTime, retreatTime, remnantTime, burstTime;
        private Sprite[] idle, move, preparation, retreat, remnantFrames, burstFrames;
        private Sprite release, recovery, hit;
        private SpriteRenderer body, remnantBody;
        private Rigidbody physicsBody;
        private Vector3 Position => physicsBody != null ? physicsBody.position : transform.position;
        private GameObject marker, remnant, shadow;
        private Color dangerColor;
        public HangoverRemnantPhase Phase { get; private set; }
        public GameObject TelegraphObject => marker;
        public GameObject ActiveRemnant => remnant;
        public Vector3 RemnantCenter => center;
        public Vector3 RetreatDirection => retreatDirection;
        public float BurstRadius => radius;
        public float WindupDuration => windup;
        public float RetreatDistance => retreatDistance;
        public float RetreatSpeed => retreatSpeed;
        public float BurstDelay => delay;
        public float NextReadyTime => nextReady;
        public int ReleaseCount { get; private set; }
        public int BurstCount { get; private set; }
        public int RejectedPaths { get; private set; }
        public float ReleasedAt { get; private set; }
        public float BurstAt { get; private set; }

        public void Initialize(EnemyController source, HangoverRemnantPatternSettings settings)
        {
            EndSpawn(); enemy = source; physicsBody = source.GetComponent<Rigidbody>();
            lifetime = source.GetComponent<EnemyPatternLifetime>() ?? source.gameObject.AddComponent<EnemyPatternLifetime>();
            lifetime.Bind(source); generation = source.SpawnGeneration; anchor = Position;
            triggerDistance = settings.triggerDistance; windup = settings.windupSeconds; retreatDistance = settings.retreatDistance;
            retreatSpeed = settings.retreatSpeed; delay = settings.burstDelaySeconds; radius = settings.burstRadius;
            recoverySeconds = settings.recoverySeconds; rearm = source.GetRearmCooldown(settings.rearmSeconds);
            var art = settings.presentation;
            idle = (Sprite[])art.idleFrames.Clone(); move = (Sprite[])art.moveFrames.Clone(); preparation = (Sprite[])art.preparationFrames.Clone();
            retreat = (Sprite[])art.retreatFrames.Clone(); remnantFrames = (Sprite[])art.remnantFrames.Clone(); burstFrames = (Sprite[])art.burstFrames.Clone();
            release = art.release; recovery = art.recovery; hit = art.hit;
            idleTime = art.idleFrameSeconds; moveTime = art.moveFrameSeconds; releaseTime = art.releaseFrameSeconds;
            retreatTime = art.retreatFrameSeconds; remnantTime = art.remnantFrameSeconds; burstTime = art.burstFrameSeconds; dangerColor = art.dangerFillColor;
            source.Config.deathSprites = (Sprite[])art.deathFrames.Clone(); source.Config.deathAnimationSpeed = art.deathFrameSeconds;
            source.BindPatternDirections(null); source.AlignPatternFeetToGround(); source.SetAiSuppressed(true);
            body = source.transform.Find("Visual").GetComponent<SpriteRenderer>(); SetFrame(idle[0]);
            CreateShadow(art); nextReady = Time.time + settings.spawnGraceSeconds; nextSafetyCheck = hitUntil = 0;
            ReleaseCount = BurstCount = RejectedPaths = 0; ReleasedAt = BurstAt = 0;
            center = Position; retreatDirection = Vector3.zero;
            Phase = HangoverRemnantPhase.Ready; source.Defeated += OnDefeated; source.DamageTaken += OnDamaged; enabled = true;
        }

        private void Update()
        {
            if (lifetime == null || !lifetime.IsCurrent(generation)) { EndSpawn(); return; }
            if (Phase != HangoverRemnantPhase.Ready || IsStunned()) return;
            if (TryBeginAttack()) return;
            var player = PlayerController.Instance;
            bool chase = player != null && !player.IsDead && enemy.IsPlayerInChaseRange() && !enemy.IsOutOfLeash();
            Vector3 delta = (chase ? player.transform.position : anchor) - Position; delta.y = 0;
            float stop = chase ? triggerDistance * .8f : .15f;
            bool moving = delta.magnitude > stop && enemy.Stats.MoveSpeed > 0;
            if (moving) enemy.MoveByExternalPattern(delta.normalized * Mathf.Min(enemy.Stats.MoveSpeed * Time.deltaTime, delta.magnitude - stop));
            SetFrame(Time.time < hitUntil ? hit : (moving ? move : idle)[Mathf.FloorToInt(Time.time / (moving ? moveTime : idleTime)) % 2]);
        }

        public bool TryBeginAttack()
        {
            if (enemy == null || lifetime == null || !lifetime.IsCurrent(generation) || Phase != HangoverRemnantPhase.Ready
                || IsStunned() || Time.time < nextReady || Time.time < nextSafetyCheck || remnant != null) return false;
            var player = PlayerController.Instance;
            if (player == null || player.IsDead || !enemy.IsPlayerInChaseRange() || enemy.IsOutOfLeash()) return false;
            Vector3 delta = player.transform.position - Position; delta.y = 0;
            if (delta.sqrMagnitude < .0001f || delta.sqrMagnitude > triggerDistance * triggerDistance
                || !GasSacOrb.IsVisible(Position + Vector3.up * .6f) || !GasSacOrb.HasClearPath(Position, player.transform.position)) return false;
            Vector3 direction = -delta.normalized;
            if (!ClearRetreat(Position, Position + direction * retreatDistance) || !ClearBurstArea(Position))
            { RejectedPaths++; nextSafetyCheck = Time.time + .25f; return false; }
            center = Position; if (BiomeManager.Active != null) center.y = BiomeManager.Active.GetGroundHeight(center);
            retreatDirection = direction;
            return lifetime.TryStartCycle(AttackCycle());
        }

        private IEnumerator AttackCycle()
        {
            uint token = generation;
            var damage = enemy.CreatePatternDamage(HangoverRemnantPatternSettings.DamageId, 0);
            Phase = HangoverRemnantPhase.Windup;
            try
            {
                marker = new GameObject("H03_DangerArea"); marker.transform.position = center + Vector3.up * .055f;
                Own(marker); EnemyGroundTelegraph.Circle(marker.transform, radius, dangerColor);
                float began = Time.time;
                while (Time.time - began < windup)
                {
                    if (IsStunned()) yield break;
                    SetFrame(preparation[Mathf.Min(2, Mathf.FloorToInt((Time.time - began) / windup * 3))]); yield return null;
                }
                if (IsStunned() || !ClearRetreat(Position, Position + retreatDirection * retreatDistance) || !ClearBurstArea(center)) yield break;
                CreateRemnant(); ReleaseCount++; ReleasedAt = Time.time; Phase = HangoverRemnantPhase.Retreat;
                float travelled = 0, recoveryAt = -1;
                bool burst = false;
                while (true)
                {
                    float elapsed = Time.time - ReleasedAt;
                    if (recoveryAt < 0)
                    {
                        float step = Mathf.Min(retreatSpeed * Time.deltaTime, retreatDistance - travelled);
                        Vector3 from = Position, to = from + retreatDirection * step;
                        if (IsStunned() || !ClearRetreat(from, to) || !enemy.MoveByExternalPattern(retreatDirection * step))
                            recoveryAt = Time.time;
                        else
                        {
                            Vector3 moved = Position - from; moved.y = 0; travelled += moved.magnitude;
                            if (travelled >= retreatDistance - .001f) recoveryAt = Time.time;
                        }
                        SetFrame(elapsed < releaseTime ? release : retreat[Mathf.FloorToInt(elapsed / retreatTime) % 2]);
                    }
                    if (recoveryAt >= 0) { Phase = HangoverRemnantPhase.Recovery; SetFrame(recovery); }
                    if (!burst && elapsed >= delay)
                    {
                        burst = true; BurstAt = Time.time; BurstCount++; Clear(ref marker); ApplyBurst(damage);
                    }
                    if (remnant != null)
                    {
                        if (burst)
                        {
                            int frame = Mathf.FloorToInt((Time.time - BurstAt) / burstTime);
                            if (frame >= burstFrames.Length) Clear(ref remnant); else remnantBody.sprite = burstFrames[frame];
                        }
                        else remnantBody.sprite = remnantFrames[Mathf.FloorToInt(elapsed / remnantTime) % 2];
                    }
                    if (recoveryAt >= 0 && Time.time - recoveryAt >= recoverySeconds && remnant == null) break;
                    yield return null;
                }
            }
            finally
            {
                Clear(ref marker); Clear(ref remnant);
                if (enemy != null && enemy.SpawnGeneration == token && lifetime.IsCurrent(token))
                { SetFrame(idle[0]); nextReady = Time.time + rearm; Phase = HangoverRemnantPhase.Ready; }
            }
        }

        private bool ClearRetreat(Vector3 from, Vector3 to)
        {
            var biome = BiomeManager.Active;
            Vector3 leash = to - anchor; leash.y = 0;
            if (enemy.Config.leashRadius < 0 || leash.sqrMagnitude > enemy.Config.leashRadius * enemy.Config.leashRadius) return false;
            var collider = enemy.GetComponent<Collider>(); Vector3 extents = collider != null ? collider.bounds.extents : Vector3.one * .4f;
            if (!ResidueRubble.CanTraverse(from, to, new Vector2(extents.x, extents.z))) return false;
            if (biome == null) return true;
            Vector2Int first = biome.WorldToGrid(from); int level = biome.GetHeightLevel(first.x, first.y);
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(from, to) / .1f));
            for (int i = 0; i <= steps; i++)
            {
                Vector3 p = Vector3.Lerp(from, to, (float)i / steps);
                Vector2Int min = biome.WorldToGrid(p - new Vector3(extents.x, 0, extents.z));
                Vector2Int max = biome.WorldToGrid(p + new Vector3(extents.x, 0, extents.z));
                for (int z = min.y; z <= max.y; z++) for (int x = min.x; x <= max.x; x++)
                    if (!biome.IsValidPosition(x, z) || !biome.IsWalkable(x, z) || biome.GetHeightLevel(x, z) != level
                        || MidBossArenaController.IsPlayerInsideLockedArena(biome.GridToWorld(x, z))) return false;
            }
            return true;
        }

        private bool ClearBurstArea(Vector3 point)
        {
            var biome = BiomeManager.Active; if (biome == null) return true;
            Vector2Int first = biome.WorldToGrid(point); int level = biome.GetHeightLevel(first.x, first.y);
            Vector2Int min = biome.WorldToGrid(point - new Vector3(radius, 0, radius)), max = biome.WorldToGrid(point + new Vector3(radius, 0, radius));
            float half = biome.TileSize * .5f;
            for (int z = min.y; z <= max.y; z++) for (int x = min.x; x <= max.x; x++)
            {
                Vector3 c = biome.GridToWorld(x, z); float dx = Mathf.Max(0, Mathf.Abs(c.x - point.x) - half), dz = Mathf.Max(0, Mathf.Abs(c.z - point.z) - half);
                if (dx * dx + dz * dz > radius * radius) continue;
                if (!biome.IsValidPosition(x, z) || !biome.IsWalkable(x, z) || biome.GetHeightLevel(x, z) != level
                    || MidBossArenaController.IsPlayerInsideLockedArena(c)) return false;
            }
            return true;
        }

        private void ApplyBurst(EnemyDamageRequest damage)
        {
            var player = PlayerController.Instance;
            if (player == null || player.IsDead || MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position)) return;
            Vector3 delta = player.transform.position - center; delta.y = 0;
            if (delta.sqrMagnitude <= radius * radius && GasSacOrb.HasClearPath(center, player.transform.position)) player.TakeDamage(damage);
        }
        private void CreateRemnant()
        {
            remnant = new GameObject("H03_Remnant"); remnant.transform.position = center; Own(remnant);
            remnantBody = new GameObject("RemnantSprite").AddComponent<SpriteRenderer>(); remnantBody.transform.SetParent(remnant.transform, false);
            remnantBody.sprite = remnantFrames[0];
            var sorting = remnant.AddComponent<SpriteYSort>(); sorting.Configure(SpriteYSort.WorldDynamicBaseSortingOrder, true, SpriteYSort.WorldDynamicMinSortingOrder);
            sorting.SetUpdateMode(SpriteYSort.UpdateMode.Continuous); UpdateRemnantView();
        }
        private void SetFrame(Sprite frame) { enemy.SetPatternFrame(frame); if (body != null) body.flipX = false; }
        private bool IsStunned() => enemy.StatusEffects != null && enemy.StatusEffects.IsStunned;
        private void OnDamaged(EnemyController source, float amount) { if (amount > 0) hitUntil = Time.time + .12f; }
        private void OnDefeated(EnemyController source) => EndSpawn();
        private void Own(GameObject go) => lifetime.Own(go, item => { item.SetActive(false); Destroy(item); });
        private void Clear(ref GameObject go) { if (go != null && lifetime != null) lifetime.ReleaseOwned(go); go = null; }
        private void CreateShadow(HangoverRemnantPresentation art)
        {
            if (shadow == null) { shadow = new GameObject("Hangover_GroundShadow"); shadow.transform.SetParent(transform, false); shadow.AddComponent<SpriteRenderer>(); }
            var renderer = shadow.GetComponent<SpriteRenderer>(); renderer.sprite = art.groundDisc; renderer.color = art.groundShadowColor; renderer.sortingOrder = 56;
            shadow.transform.rotation = Quaternion.Euler(90, 0, 0); shadow.transform.localScale = new Vector3(art.groundShadowSize.x * .5f, art.groundShadowSize.y * .5f, 1); shadow.SetActive(true); LateUpdate();
        }
        private void UpdateRemnantView()
        {
            if (remnant == null || remnantBody == null) return;
            Camera camera = DontStarveCamera.GetActiveCamera(); if (camera != null) remnantBody.transform.rotation = camera.transform.rotation;
            remnantBody.transform.position = center;
        }
        private void LateUpdate()
        {
            UpdateRemnantView();
            if (shadow != null && shadow.activeSelf)
            {
                Vector3 p = transform.position; if (BiomeManager.Active != null) p.y = BiomeManager.Active.GetGroundHeight(p);
                shadow.transform.position = p + Vector3.up * .035f;
            }
        }
        public override void EndSpawn()
        {
            if (enemy != null) { enemy.Defeated -= OnDefeated; enemy.DamageTaken -= OnDamaged; }
            lifetime?.Cancel(); Clear(ref marker); Clear(ref remnant); remnantBody = null;
            if (shadow != null) shadow.SetActive(false);
            Phase = HangoverRemnantPhase.Inactive; enabled = false;
        }
        private void OnDisable() { if (Phase != HangoverRemnantPhase.Inactive) EndSpawn(); }
    }
}
