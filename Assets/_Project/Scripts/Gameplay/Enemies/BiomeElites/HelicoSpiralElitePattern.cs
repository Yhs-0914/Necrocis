using System.Collections;
using UnityEngine;

namespace Necrocis
{
    public enum HelicoSpiralPhase { Inactive, Ready, Windup, Dash, Recovery, TailWindup, TailSweep }
    [DisallowMultipleComponent]
    public sealed partial class HelicoSpiralElitePattern : MonsterPatternController
    {
        private EnemyController enemy;
        private EnemyPatternLifetime lifetime;
        private uint generation;
        private Vector3 anchor, bodyDirection = Vector3.right;
        private float trigger, stop, windup, distance, dashTime, halfLength, radius, recoveryTime, rearm, nextReady, hitUntil;
        private float idleSeconds, moveSeconds;
        private Sprite[] idle, move, preparation, dash, recovery;
        private Sprite hit;
        private Color danger, dangerBorder;
        private static Material borderMaterial;
        public HelicoSpiralPhase Phase { get; private set; }
        public Vector3 LockedDirection { get; private set; }
        public Vector3 DashStart { get; private set; }
        public Vector3 DashEnd { get; private set; }
        public GameObject TelegraphObject { get; private set; }
        public float PlannedDistance { get; private set; }
        public float Travelled { get; private set; }
        public float ReleasedAt { get; private set; }
        public float NextReadyTime => nextReady;
        public float WindupDuration => windup;
        public float DashDuration => dashTime;
        public float BodyRadius => radius;
        public float BodyHalfLength => halfLength;
        public Vector3 BodyDirection => bodyDirection;
        public int DashCount { get; private set; }
        public int HitAttempts { get; private set; }
        public int DamageApplications { get; private set; }
        public bool WallStopped { get; private set; }
        private Vector3 Position => enemy.GetComponent<Rigidbody>().position;
        public void Initialize(EnemyController source, HelicoSpiralPatternSettings settings)
        {
            EndSpawn(); enemy = source;
            // This pattern advances in render-frame slices; interpolation would draw
            // the fast body behind its current damage position by a physics step.
            source.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
            lifetime = source.GetComponent<EnemyPatternLifetime>() ?? source.gameObject.AddComponent<EnemyPatternLifetime>();
            lifetime.Bind(source); generation = source.SpawnGeneration; anchor = Position;
            trigger = settings.triggerDistance; stop = settings.approachStopDistance; windup = settings.windupSeconds;
            distance = settings.dashDistance; dashTime = settings.dashSeconds; radius = settings.bodyWidth * .5f; halfLength = settings.bodyHalfLength;
            recoveryTime = settings.recoverySeconds; rearm = source.GetRearmCooldown(settings.rearmSeconds);
            var art = settings.presentation;
            idle = (Sprite[])art.idleFrames.Clone(); move = (Sprite[])art.moveFrames.Clone(); preparation = (Sprite[])art.preparationFrames.Clone();
            dash = (Sprite[])art.dashFrames.Clone(); recovery = (Sprite[])art.recoveryFrames.Clone(); hit = art.hit;
            idleSeconds = art.idleFrameSeconds; moveSeconds = art.moveFrameSeconds; danger = art.dangerColor; dangerBorder = art.dangerBorderColor;
            source.Config.deathSprites = (Sprite[])art.deathFrames.Clone(); source.Config.deathAnimationSpeed = art.deathFrameSeconds;
            source.BindPatternDirections(art.directionalPresentation); source.AlignPatternFeetToGround(); source.SetPatternFrame(idle[0]);
            var visual = source.GetComponent<HelicoGroundedVisual>() ?? source.gameObject.AddComponent<HelicoGroundedVisual>();
            visual.Configure(source.transform.Find("Visual").GetComponent<SpriteRenderer>(), art, this);
            DisableLegacyTailRendering();
            source.SetAiSuppressed(true); source.SetPatternPositionLocked(false); source.PatternOwnsContact = true;
            bodyDirection = Vector3.right; source.SetPatternFacing(bodyDirection, false);
            nextReady = Time.time + settings.spawnGraceSeconds; hitUntil = 0; DashCount = HitAttempts = DamageApplications = 0;
            PlannedDistance = Travelled = ReleasedAt = 0; WallStopped = false; Phase = HelicoSpiralPhase.Ready;
            source.DamageTaken += OnDamaged; source.Defeated += OnDefeated; enabled = true;
        }
        private void Update()
        {
            if (lifetime == null || !lifetime.IsCurrent(generation)) { EndSpawn(); return; }
            if (Phase != HelicoSpiralPhase.Dash) ApplyBodyContact();
            if (Phase != HelicoSpiralPhase.Ready || IsStunned()) return;
            if (TryBeginAttack()) return;
            var player = PlayerController.Instance;
            bool chase = player != null && !player.IsDead && enemy.IsPlayerInChaseRange() && !enemy.IsOutOfLeash();
            Vector3 delta = (chase ? player.transform.position : anchor) - Position; delta.y = 0;
            float stopAt = chase ? stop : .15f;
            bool moving = delta.magnitude > stopAt && enemy.Stats.MoveSpeed > 0;
            if (moving) enemy.MoveByExternalPattern(delta.normalized * Mathf.Min(enemy.Stats.MoveSpeed * Time.deltaTime, delta.magnitude - stopAt));
            if ((moving || chase) && delta.sqrMagnitude > .0001f) { bodyDirection = delta.normalized; enemy.SetPatternFacing(delta, false); }
            var bank = moving ? move : idle;
            enemy.SetPatternFrame(Time.time < hitUntil ? hit : bank[Mathf.FloorToInt(Time.time / (moving ? moveSeconds : idleSeconds)) % bank.Length]);
        }
        public bool TryBeginAttack()
        {
            if (enemy == null || lifetime == null || !lifetime.IsCurrent(generation) || Phase != HelicoSpiralPhase.Ready || IsStunned() || Time.time < nextReady) return false;
            var player = PlayerController.Instance;
            if (player == null || player.IsDead || enemy.IsOutOfLeash() || !enemy.IsPlayerInChaseRange() || MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position)) return false;
            Vector3 delta = player.transform.position - Position; delta.y = 0;
            if (delta.sqrMagnitude < .01f || delta.magnitude > trigger || !GasSacOrb.IsVisible(Position) || !GasSacOrb.HasClearPath(Position, player.transform.position)) return false;
            LockedDirection = bodyDirection = delta.normalized; DashStart = Position; PlannedDistance = 0;
            // Clip the forecast to the same swept body used by the moving attack. Never slide along a wall.
            for (float d = Mathf.Min(.1f, distance); d <= distance + .0001f; d = Mathf.Min(distance, d + .1f))
            {
                if (!CanStep(DashStart, DashStart + LockedDirection * d)) break;
                PlannedDistance = d; if (d >= distance) break;
            }
            if (PlannedDistance < .2f) { nextReady = Time.time + .25f; return false; }
            DashEnd = DashStart + LockedDirection * PlannedDistance;
            return lifetime.TryStartCycle(DashCycle());
        }
        private bool CanStep(Vector3 from, Vector3 to)
        {
            var collider = enemy.GetComponent<Collider>();
            Vector2 half = new Vector2(collider.bounds.extents.x, collider.bounds.extents.z);
            return DustCloud.CanTravel(from - LockedDirection * halfLength, to + LockedDirection * halfLength, radius)
                && ResidueRubble.CanTraverse(from, to, half) && (BiomeManager.Active == null || BiomeManager.Active.CanMove(from, to));
        }
        private IEnumerator DashCycle()
        {
            enemy.CommitPatternFacing(LockedDirection, false); enemy.SetPatternPositionLocked(true);
            HitAttempts = DamageApplications = 0; Travelled = 0; WallStopped = PlannedDistance < distance - .01f;
            Phase = HelicoSpiralPhase.Windup; CreateTelegraph();
            var damage = enemy.CreatePatternDamage(HelicoSpiralPatternSettings.DamageId, 0);
            try
            {
                float elapsed = 0;
                while (elapsed < windup)
                {
                    if (IsStunned()) yield break;
                    enemy.SetPatternFrame(preparation[Mathf.Min(2, Mathf.FloorToInt(elapsed / windup * 3))]);
                    yield return null; elapsed += Time.deltaTime;
                }
                if (!lifetime.IsCurrent(generation) || IsStunned()) yield break;
                Phase = HelicoSpiralPhase.Dash; DashCount++; ReleasedAt = Time.time; elapsed = 0;
                // The movement API rejects sub-millimetre steps; a tiny remainder is not a wall.
                while (Travelled < PlannedDistance - .001f)
                {
                    if (IsStunned()) break;
                    float step = Mathf.Min(distance / dashTime * Time.deltaTime, PlannedDistance - Travelled);
                    int slices = Mathf.Max(1, Mathf.CeilToInt(step / .08f));
                    bool blocked = false;
                    for (int i = 0; i < slices; i++)
                    {
                        Vector3 from = Position, to = from + LockedDirection * (step / slices);
                        if (!CanStep(from, to) || !enemy.MoveByExternalPattern(to - from)) { WallStopped = blocked = true; break; }
                        Travelled += step / slices; ApplyDashDamage(from, Position, damage);
                    }
                    elapsed += Time.deltaTime; enemy.SetPatternFrame(dash[Mathf.Min(1, Mathf.FloorToInt(elapsed / dashTime * 2))]);
                    if (blocked) break;
                    yield return null;
                }
                ClearTelegraph();
                Phase = HelicoSpiralPhase.Recovery; elapsed = 0;
                while (elapsed < recoveryTime)
                {
                    enemy.SetPatternFrame(recovery[Mathf.Min(1, Mathf.FloorToInt(elapsed / recoveryTime * 2))]);
                    yield return null; elapsed += Time.deltaTime;
                }
            }
            finally
            {
                ClearTelegraph();
                if (enemy != null && enemy.SpawnGeneration == generation)
                {
                    enemy.ReleasePatternFacing(); enemy.SetPatternPositionLocked(false);
                    if (lifetime.IsCurrent(generation)) { Phase = HelicoSpiralPhase.Ready; nextReady = Time.time + rearm; enemy.SetPatternFrame(idle[0]); }
                }
            }
        }
        private void CreateTelegraph()
        {
            TelegraphObject = new GameObject("S01_ChargePath");
            TelegraphObject.transform.position = DashStart + LockedDirection * (PlannedDistance * .5f) + Vector3.up * .055f;
            TelegraphObject.transform.rotation = Quaternion.LookRotation(LockedDirection, Vector3.up);
            float line = PlannedDistance + halfLength * 2;
            EnemyGroundTelegraph.Rectangle(TelegraphObject.transform, new Vector2(radius * 2, line), danger);
            foreach (float end in new[] { -.5f, .5f })
            {
                var cap = new GameObject("RoundEnd"); cap.transform.SetParent(TelegraphObject.transform, false); cap.transform.localPosition = Vector3.forward * (line * end);
                EnemyGroundTelegraph.Circle(cap.transform, radius, danger);
            }
            // A pale boundary separates the red footprint from Stomach's red floor.
            // Its center follows the exact damage capsule, without widening damage.
            if (borderMaterial == null) borderMaterial = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave, mainTexture = Texture2D.whiteTexture };
            var border = new GameObject("Boundary").AddComponent<LineRenderer>(); border.transform.SetParent(TelegraphObject.transform, false);
            border.useWorldSpace = false; border.loop = true; border.widthMultiplier = .025f;
            border.sharedMaterial = borderMaterial; border.startColor = border.endColor = dangerBorder; border.sortingOrder = 71;
            border.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; border.receiveShadows = false;
            var outline = new Vector3[66];
            for (int i = 0; i <= 32; i++)
            {
                float front = (-90 + 180f * i / 32) * Mathf.Deg2Rad, back = (90 + 180f * i / 32) * Mathf.Deg2Rad;
                outline[i] = new Vector3(Mathf.Sin(front) * radius, .004f, line * .5f + Mathf.Cos(front) * radius);
                outline[33 + i] = new Vector3(Mathf.Sin(back) * radius, .004f, -line * .5f + Mathf.Cos(back) * radius);
            }
            border.positionCount = outline.Length; border.SetPositions(outline);
            lifetime.Own(TelegraphObject, go => { go.SetActive(false); Destroy(go); });
        }
        private void ClearTelegraph()
        {
            if (TelegraphObject != null) { lifetime.ReleaseOwned(TelegraphObject); TelegraphObject = null; }
        }
        private void ApplyDashDamage(Vector3 from, Vector3 to, EnemyDamageRequest damage)
        {
            var player = PlayerController.Instance;
            if (HitAttempts != 0 || player == null || player.IsDead || MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position)
                || !GasSacOrb.HasClearPath(to, player.transform.position)) return;
            if (CombatHitGeometry.PointSegmentDistanceSquared(CombatHitGeometry.Flat(player.transform.position), CombatHitGeometry.Flat(from - LockedDirection * halfLength),
                CombatHitGeometry.Flat(to + LockedDirection * halfLength)) > radius * radius) return;
            HitAttempts++; float hp = player.HealthComponent.CurrentHealth; player.TakeDamage(damage);
            if (player.HealthComponent.CurrentHealth < hp) DamageApplications++;
        }
        private void ApplyBodyContact()
        {
            var player = PlayerController.Instance;
            if (enemy.Balance == null || !enemy.Balance.ContactEnabled || player == null || player.IsDead || player.IsDashInvincible || player.HealthComponent.IsInvincible
                || MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position) || !GasSacOrb.HasClearPath(Position, player.transform.position)) return;
            // The generous receiving box stays separate from this narrow oriented body.
            if (!CombatHitGeometry.SweepTouchesPlayer(Position - bodyDirection * halfLength, Position + bodyDirection * halfLength, radius, player)) return;
            float hp = player.HealthComponent.CurrentHealth; player.TakeDamage(enemy.CreateContactDamage(0));
            if (player.HealthComponent.CurrentHealth < hp)
            {
                Vector3 away = player.transform.position - Position; away.y = 0;
                if (away.sqrMagnitude < .0001f) away = -bodyDirection;
                player.TryMoveByWorld(away.normalized * enemy.Balance.ContactKnockback);
            }
        }
        private bool IsStunned() => enemy.StatusEffects != null && enemy.StatusEffects.IsStunned;
        private void OnDamaged(EnemyController source, float amount) { if (amount > 0 && Phase == HelicoSpiralPhase.Ready) hitUntil = Time.time + .12f; }
        private void OnDefeated(EnemyController source) => EndSpawn();
        public override void EndSpawn()
        {
            if (enemy != null) { enemy.DamageTaken -= OnDamaged; enemy.Defeated -= OnDefeated; }
            lifetime?.Cancel(); TelegraphObject = null; DisableLegacyTailRendering();
            if (enemy != null) { enemy.ReleasePatternFacing(); enemy.SetPatternPositionLocked(false); enemy.PatternOwnsContact = false; }
            Phase = HelicoSpiralPhase.Inactive; enabled = false;
        }
        private void OnDisable() { if (Phase != HelicoSpiralPhase.Inactive) EndSpawn(); }
    }
}
