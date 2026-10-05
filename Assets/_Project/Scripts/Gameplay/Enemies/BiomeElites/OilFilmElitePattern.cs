using System.Collections;
using UnityEngine;
namespace Necrocis
{
    public enum OilFilmPhase { Inactive, Ready, Windup, Release, Recovery }
    [DisallowMultipleComponent]
    public sealed class OilFilmElitePattern : MonsterPatternController
    {
        private EnemyController enemy;
        private EnemyPatternLifetime lifetime;
        private uint generation;
        private Vector3 home;
        private Sprite[] idle, move, preparation, release, recovery, hit;
        private float trigger, stop, windup, range, arc, releaseTime, impactTime, recoveryTime, rearm, coreRadius, nextReady, hitUntil;
        private float idleTime, moveTime, hitTime;
        private Color danger, border;
        private static Material markerMaterial;
        private Mesh warningMesh;
        private LineRenderer warningEdge;
        public OilFilmPhase Phase { get; private set; }
        public OilFilmSector Footprint { get; private set; }
        public GameObject TelegraphObject { get; private set; }
        public Vector3 LockedDirection { get; private set; }
        public int ImpactCount { get; private set; }
        public int HitAttempts { get; private set; }
        public int DamageApplications { get; private set; }
        public float WindupDuration => windup;
        public float ReleaseDuration => releaseTime;
        public float ImpactNormalizedTime => impactTime;
        public float CoreRadius => coreRadius;
        public float AttackRange => range;
        public float ArcDegrees => arc;
        public float NextReadyTime => nextReady;
        public bool IsUnfolding => Phase == OilFilmPhase.Release || Phase == OilFilmPhase.Recovery;
        public Vector3 Position => enemy != null ? enemy.GetComponent<Rigidbody>().position : transform.position;
        public Vector3 GroundOrigin
        {
            get { Vector3 p = Position; if (BiomeManager.Active != null) p.y = BiomeManager.Active.GetGroundHeight(p); return p; }
        }
        public void Initialize(EnemyController source, OilFilmPatternSettings settings)
        {
            EndSpawn(); enemy = source; home = Position;
            lifetime = source.GetComponent<EnemyPatternLifetime>() ?? source.gameObject.AddComponent<EnemyPatternLifetime>();
            lifetime.Bind(source); generation = source.SpawnGeneration;
            trigger = settings.triggerDistance; stop = settings.approachStopDistance; windup = settings.windupSeconds;
            range = settings.attackRange; arc = settings.arcDegrees; releaseTime = settings.releaseSeconds; impactTime = settings.impactNormalizedTime;
            recoveryTime = settings.recoverySeconds; rearm = source.GetRearmCooldown(settings.rearmSeconds); coreRadius = settings.coreRadius;
            var art = settings.presentation;
            idle = (Sprite[])art.idleFrames.Clone(); move = (Sprite[])art.moveFrames.Clone(); preparation = (Sprite[])art.preparationFrames.Clone();
            release = (Sprite[])art.releaseFrames.Clone(); recovery = (Sprite[])art.recoveryFrames.Clone(); hit = (Sprite[])art.hitFrames.Clone();
            idleTime = art.idleFrameSeconds; moveTime = art.moveFrameSeconds; hitTime = art.hitFrameSeconds;
            danger = art.dangerColor; border = art.dangerBorderColor;
            source.Config.deathSprites = (Sprite[])art.deathFrames.Clone(); source.Config.deathAnimationSpeed = art.deathFrameSeconds;
            source.BindPatternDirections(art.directionalPresentation); source.AlignPatternFeetToGround(); source.SetPatternFrame(idle[0]); source.SetPatternFacing(Vector3.right, false);
            var visual = GetComponent<OilFilmVisual>() ?? gameObject.AddComponent<OilFilmVisual>(); visual.Configure(source, art, this);
            source.SetAiSuppressed(true); source.PatternOwnsContact = true; source.SetPatternPositionLocked(false);
            Phase = OilFilmPhase.Ready; Footprint = null; LockedDirection = Vector3.zero; ImpactCount = HitAttempts = DamageApplications = 0; hitUntil = 0;
            nextReady = Time.time + settings.spawnGraceSeconds; enabled = true;
            source.DamageTaken += OnDamaged; source.Defeated += OnDefeated;
        }
        private void Update()
        {
            if (lifetime == null || !lifetime.IsCurrent(generation)) { EndSpawn(); return; }
            ApplyCoreContact();
            if (Phase != OilFilmPhase.Ready || IsStunned()) return;
            if (TryBeginAttack()) return;
            var player = PlayerController.Instance;
            bool chase = player != null && !player.IsDead && enemy.IsPlayerInChaseRange() && !enemy.IsOutOfLeash();
            Vector3 delta = (chase ? player.transform.position : home) - Position; delta.y = 0;
            float stopAt = chase ? stop : .15f;
            bool moving = delta.magnitude > stopAt && enemy.Stats.MoveSpeed > 0;
            if (moving) enemy.MoveByExternalPattern(delta.normalized * Mathf.Min(enemy.Stats.MoveSpeed * Time.deltaTime, delta.magnitude - stopAt));
            if (delta.sqrMagnitude > .0001f && (moving || chase)) enemy.SetPatternFacing(delta, false);
            var frames = Time.time < hitUntil ? hit : moving ? move : idle;
            float seconds = Time.time < hitUntil ? hitTime : moving ? moveTime : idleTime;
            enemy.SetPatternFrame(frames[Mathf.FloorToInt(Time.time / seconds) % frames.Length]);
        }
        public bool TryBeginAttack()
        {
            var player = PlayerController.Instance;
            if (enemy == null || lifetime == null || !lifetime.IsCurrent(generation) || Phase != OilFilmPhase.Ready || IsStunned() || Time.time < nextReady
                || player == null || player.IsDead || enemy.IsOutOfLeash() || !enemy.IsPlayerInChaseRange()
                || MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position)) return false;
            Vector3 delta = player.transform.position - Position; delta.y = 0;
            if (delta.sqrMagnitude < .01f || delta.magnitude > trigger || !GasSacOrb.IsVisible(Position) || !GasSacOrb.HasClearPath(Position, player.transform.position)) return false;
            enemy.CommitPatternFacing(delta, false);
            LockedDirection = FacingDirection(enemy.PatternFacing, DontStarveCamera.GetActiveCamera());
            Footprint = new OilFilmSector(GroundOrigin, LockedDirection, range, arc);
            return lifetime.TryStartCycle(AttackCycle());
        }
        public static Vector3 FacingDirection(EnemyFacing facing, Camera camera)
        {
            Vector3 right = camera != null ? Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized : Vector3.right;
            Vector3 back = Vector3.Cross(right, Vector3.up).normalized;
            if (camera != null && Vector3.Dot(back, camera.transform.up) < 0) back = -back;
            return facing == EnemyFacing.Left ? -right : facing == EnemyFacing.Front ? -back : facing == EnemyFacing.Back ? back : right;
        }
        private IEnumerator AttackCycle()
        {
            enemy.SetPatternPositionLocked(true); HitAttempts = DamageApplications = 0;
            bool impacted = false;
            Phase = OilFilmPhase.Windup; CreateTelegraph();
            try
            {
                float elapsed = 0;
                while (elapsed < windup)
                {
                    if (IsStunned() || !RefreshFootprint()) yield break;
                    enemy.SetPatternFrame(preparation[Mathf.Min(3, Mathf.FloorToInt(elapsed / windup * 4))]);
                    yield return null; elapsed += Time.deltaTime;
                }
                Phase = OilFilmPhase.Release; elapsed = 0;
                while (elapsed < releaseTime || !impacted)
                {
                    if (IsStunned() || !lifetime.IsCurrent(generation) || !RefreshFootprint()) yield break;
                    float t = Mathf.Clamp01(elapsed / releaseTime);
                    float artT = t <= impactTime ? t / impactTime * .5f : .5f + (t - impactTime) / (1 - impactTime) * .5f;
                    enemy.SetPatternFrame(release[Mathf.Min(3, Mathf.FloorToInt(artT * 4))]);
                    if (!impacted && t >= impactTime)
                    {
                        enemy.SetPatternFrame(release[2]); // The full extension is always the impact pose, including a long frame.
                        impacted = true; ImpactCount++; ApplyImpact(); ClearTelegraph();
                    }
                    yield return null; elapsed += Time.deltaTime;
                }
                Phase = OilFilmPhase.Recovery; elapsed = 0;
                while (elapsed < recoveryTime)
                {
                    if (IsStunned() || !RefreshFootprint()) yield break;
                    enemy.SetPatternFrame(recovery[Mathf.Min(3, Mathf.FloorToInt(elapsed / recoveryTime * 4))]);
                    yield return null; elapsed += Time.deltaTime;
                }
            }
            finally
            {
                ClearTelegraph();
                if (enemy != null && enemy.SpawnGeneration == generation)
                {
                    enemy.ReleasePatternFacing(); enemy.SetPatternPositionLocked(false);
                    if (lifetime.IsCurrent(generation)) { Phase = OilFilmPhase.Ready; nextReady = Time.time + rearm; enemy.SetPatternFrame(idle[0]); }
                }
            }
        }
        private void ApplyImpact()
        {
            var player = PlayerController.Instance;
            if (player == null || player.IsDead || !Footprint.Contains(player.transform.position)
                || MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position) || !GasSacOrb.HasClearPath(Position, player.transform.position)) return;
            if (BiomeManager.Active != null && Mathf.Abs(BiomeManager.Active.GetGroundHeight(player.transform.position) - Footprint.Origin.y) > .25f) return;
            HitAttempts++; float hp = player.HealthComponent.CurrentHealth;
            player.TakeDamage(enemy.CreatePatternDamage(OilFilmPatternSettings.DamageId, 0));
            if (player.HealthComponent.CurrentHealth < hp) DamageApplications++;
        }
        private void ApplyCoreContact()
        {
            var player = PlayerController.Instance;
            if (enemy.Balance == null || !enemy.Balance.ContactEnabled || player == null || player.IsDead || player.IsDashInvincible || player.HealthComponent.IsInvincible
                || MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position) || !GasSacOrb.HasClearPath(Position, player.transform.position)
                || !CombatHitGeometry.SweepTouchesPlayer(Position, Position, coreRadius, player)) return;
            float hp = player.HealthComponent.CurrentHealth; player.TakeDamage(enemy.CreateContactDamage(0));
            if (player.HealthComponent.CurrentHealth < hp)
            {
                Vector3 away = player.transform.position - Position; away.y = 0;
                if (away.sqrMagnitude < .0001f) away = -LockedDirection;
                player.TryMoveByWorld(away.normalized * enemy.Balance.ContactKnockback);
            }
        }
        private void CreateTelegraph()
        {
            TelegraphObject = new GameObject("S03_FilledAttackArea"); TelegraphObject.transform.position = Footprint.Origin + Vector3.up * .035f;
            var mesh = Footprint.MakeMesh(); warningMesh = mesh; TelegraphObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            if (markerMaterial == null) markerMaterial = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave, mainTexture = Texture2D.whiteTexture };
            var renderer = TelegraphObject.AddComponent<MeshRenderer>(); renderer.sharedMaterial = markerMaterial; renderer.sortingOrder = 65;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false; EnemyGroundTelegraph.SetColor(renderer, danger);
            var edge = new GameObject("Boundary").AddComponent<LineRenderer>(); warningEdge = edge; edge.transform.SetParent(TelegraphObject.transform, false);
            edge.useWorldSpace = false; edge.loop = true; edge.widthMultiplier = .018f; edge.sharedMaterial = markerMaterial; edge.sortingOrder = 66;
            edge.startColor = edge.endColor = border; edge.positionCount = OilFilmSector.Segments + 2; edge.SetPosition(0, Vector3.zero);
            for (int i = 0; i <= OilFilmSector.Segments; i++) edge.SetPosition(i + 1, Footprint.Point(i / (float)OilFilmSector.Segments, 1) - Footprint.Origin);
            lifetime.Own(TelegraphObject, go => { go.SetActive(false); Destroy(mesh); Destroy(go); });
        }
        private bool RefreshFootprint()
        {
            if (Footprint == null) return false;
            if (Footprint.ReduceToCurrentTerrain() && warningMesh != null && TelegraphObject != null)
            {
                Footprint.WriteVertices(warningMesh);
                for(int i=0;i<=OilFilmSector.Segments;i++)warningEdge.SetPosition(i+1,Footprint.Point(i/(float)OilFilmSector.Segments,1)-Footprint.Origin);
            }
            return Footprint.MaximumReach > .05f;
        }
        private void ClearTelegraph() { if (TelegraphObject != null) { lifetime.ReleaseOwned(TelegraphObject); TelegraphObject = null; } warningMesh = null; warningEdge = null; }
        private bool IsStunned() => enemy.StatusEffects != null && enemy.StatusEffects.IsStunned;
        private void OnDamaged(EnemyController source, float amount) { if (amount > 0 && Phase == OilFilmPhase.Ready) hitUntil = Time.time + hitTime * 2; }
        private void OnDefeated(EnemyController source) => EndSpawn();
        public override void EndSpawn()
        {
            if (enemy != null) { enemy.DamageTaken -= OnDamaged; enemy.Defeated -= OnDefeated; }
            lifetime?.Cancel(); TelegraphObject = null; warningMesh = null; warningEdge = null; Phase = OilFilmPhase.Inactive; Footprint = null;
            GetComponent<OilFilmVisual>()?.RestoreSprite();
            if (enemy != null) { enemy.ReleasePatternFacing(); enemy.SetPatternPositionLocked(false); enemy.PatternOwnsContact = false; }
            enabled = false;
        }
        private void OnDisable() { if (Phase != OilFilmPhase.Inactive) EndSpawn(); }
    }
}
