using System.Collections;
using UnityEngine;

namespace Necrocis
{
    public enum HardenedResiduePhase { Inactive, Ready, Windup, Dropping, Recovery }

    [DisallowMultipleComponent]
    public sealed class HardenedResidueElitePattern : MonsterPatternController
    {
        private EnemyController enemy;
        private EnemyPatternLifetime lifetime;
        private uint generation;
        private Vector3 anchor, landingCenter, landingDirection, attackDirection;
        private Vector2 footprint;
        private float triggerDistance, placementDistance, windup, dropSeconds, recovery, rearm, nextReady, propSeconds, nextSafetyCheck;
        private float idleFrameSeconds, moveFrameSeconds, hitUntil;
        private float verticalRubbleGroundDepthFraction;
        private int hitsToBreak;
        private Sprite[] idle, move, lift;
        private Sprite release, recoveryFrame, hitFrame, rubbleSprite, verticalRubbleSprite, airborneChip, groundDisc;
        private Color crackColor, dangerFillColor;
        private GameObject marker, shadow;
        private static Material lineMaterial;
        public HardenedResiduePhase Phase { get; private set; }
        public ResidueRubble ActiveRubble { get; private set; }
        public Vector3 LandingCenter => landingCenter;
        public Vector3 LandingDirection => landingDirection;
        public Vector2 Footprint => footprint;
        public GameObject TelegraphObject => marker;
        public int LandingCount { get; private set; }
        public int RejectedPlacements { get; private set; }
        public float NextReadyTime => nextReady;
        public float WindupDuration => windup;
        public float RecoveryDuration => recovery;
        public Vector3 LaunchPosition { get; private set; }
        public bool UsesVerticalRubble { get; private set; }

        public void Initialize(EnemyController source, HardenedResiduePatternSettings settings)
        {
            EndSpawn(); enemy = source;
            lifetime = source.GetComponent<EnemyPatternLifetime>() ?? source.gameObject.AddComponent<EnemyPatternLifetime>();
            lifetime.Bind(source); generation = source.SpawnGeneration; anchor = source.transform.position;
            triggerDistance = settings.triggerDistance; placementDistance = settings.placementDistance; footprint = settings.footprint;
            windup = settings.windupSeconds; dropSeconds = settings.dropSeconds; recovery = settings.recoverySeconds;
            rearm = source.GetRearmCooldown(settings.rearmSeconds); propSeconds = settings.rubbleLifetime; hitsToBreak = settings.hitsToBreak;
            nextReady = Time.time + settings.spawnGraceSeconds; nextSafetyCheck = 0; hitUntil = 0;
            var art = settings.presentation;
            idle = (Sprite[])art.idleFrames.Clone(); move = (Sprite[])art.moveFrames.Clone(); lift = (Sprite[])art.liftFrames.Clone();
            release = art.release; recoveryFrame = art.recovery; hitFrame = art.hit; rubbleSprite = art.rubble; groundDisc = art.groundDisc;
            airborneChip = art.airborneChip;
            verticalRubbleSprite = art.verticalRubble;
            verticalRubbleGroundDepthFraction = art.verticalRubbleGroundDepthFraction;
            idleFrameSeconds = art.idleFrameSeconds; moveFrameSeconds = art.moveFrameSeconds; crackColor = art.crackColor;
            dangerFillColor = art.dangerFillColor;
            source.Config.deathSprites = (Sprite[])art.deathFrames.Clone(); source.Config.deathAnimationSpeed = art.deathFrameSeconds;
            source.BindPatternDirections(art.directionalPresentation);
            source.AlignPatternFeetToGround(); source.SetPatternFrame(idle[0]); source.SetAiSuppressed(true);
            if (PlayerController.Instance != null) source.SetPatternFacing(PlayerController.Instance.transform.position - source.transform.position);
            CreateShadow(art.groundShadowSize, art.groundShadowColor);
            ActiveRubble = null; LaunchPosition = Vector3.zero; UsesVerticalRubble = false;
            LandingCount = RejectedPlacements = 0; Phase = HardenedResiduePhase.Ready;
            source.Defeated += OnDefeated; source.DamageTaken += OnDamaged; enabled = true;
        }

        private void Update()
        {
            if (lifetime == null || !lifetime.IsCurrent(generation)) { EndSpawn(); return; }
            if (Phase != HardenedResiduePhase.Ready || IsStunned()) return;
            if (TryBeginAttack()) return;
            PlayerController player = PlayerController.Instance;
            bool chase = player != null && enemy.IsPlayerInChaseRange() && !enemy.IsOutOfLeash();
            Vector3 delta = (chase ? player.transform.position : anchor) - transform.position; delta.y = 0;
            float stop = chase ? placementDistance : .15f;
            bool moving = delta.magnitude > stop && enemy.Stats.MoveSpeed > 0;
            if (moving) enemy.MoveByExternalPattern(delta.normalized * Mathf.Min(enemy.Stats.MoveSpeed * Time.deltaTime, delta.magnitude - stop));
            if (moving || chase) enemy.SetPatternFacing(delta);
            Sprite[] frames = moving ? move : idle;
            float interval = moving ? moveFrameSeconds : idleFrameSeconds;
            enemy.SetPatternFrame(Time.time < hitUntil ? hitFrame : frames[Mathf.FloorToInt(Time.time / interval) % frames.Length]);
        }

        public bool TryBeginAttack()
        {
            if (enemy == null || lifetime == null || !lifetime.IsCurrent(generation) || Phase != HardenedResiduePhase.Ready
                || IsStunned() || Time.time < nextReady || Time.time < nextSafetyCheck || ActiveRubble != null) return false;
            PlayerController player = PlayerController.Instance;
            if (player == null || player.IsDead || !enemy.IsPlayerInChaseRange() || enemy.IsOutOfLeash()) return false;
            Vector3 delta = player.transform.position - transform.position; delta.y = 0;
            if (delta.sqrMagnitude > triggerDistance * triggerDistance) return false;
            Vector3 aim = delta.sqrMagnitude > .0001f ? delta.normalized : Vector3.forward;
            enemy.SetPatternFacing(aim);
            UsesVerticalRubble = enemy.PatternFacing == EnemyFacing.Front || enemy.PatternFacing == EnemyFacing.Back;
            Camera camera = DontStarveCamera.GetActiveCamera();
            Vector3 screenRight = camera != null ? Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized : Vector3.right;
            if (screenRight.sqrMagnitude < .0001f) screenRight = Vector3.right;
            // Local X remains the authored long axis; only its ground orientation changes.
            landingDirection = UsesVerticalRubble ? -screenRight : Vector3.Cross(screenRight, Vector3.up);
            landingCenter = transform.position + aim * placementDistance;
            if (BiomeManager.Active != null) landingCenter.y = BiomeManager.Active.GetGroundHeight(landingCenter);
            if (!ResiduePlacementSafety.CanPlace(landingCenter, landingDirection, footprint))
            { RejectedPlacements++; nextSafetyCheck = Time.time + .25f; return false; }
            attackDirection = aim;
            return lifetime.TryStartCycle(AttackCycle());
        }

        private IEnumerator AttackCycle()
        {
            enemy.CommitPatternFacing(attackDirection);
            EnemyDamageRequest damage = enemy.CreatePatternDamage(HardenedResiduePatternSettings.DamageId, 0);
            Phase = HardenedResiduePhase.Windup;
            bool landed = false;
            try
            {
                CreateCrack(); float elapsed = 0;
                while (elapsed < windup)
                {
                    if (IsStunned()) yield break;
                    enemy.SetPatternFrame(lift[Mathf.Min(lift.Length - 1, Mathf.FloorToInt(elapsed / windup * lift.Length))]);
                    yield return null; elapsed += Time.deltaTime;
                }
                if (!lifetime.IsCurrent(generation) || IsStunned()
                    || !ResiduePlacementSafety.CanPlace(landingCenter, landingDirection, footprint)) yield break;
                Phase = HardenedResiduePhase.Dropping; enemy.SetPatternFrame(release);
                LaunchPosition = enemy.HasPatternDirections ? enemy.GetPatternVisualOrigin() : transform.position + Vector3.up * 1.5f;
                Sprite landedSprite = UsesVerticalRubble ? verticalRubbleSprite : rubbleSprite;
                ActiveRubble = ResidueRubble.Create(enemy, lifetime, landingCenter, landingDirection, footprint, landedSprite, hitsToBreak,
                    airborneChip, LaunchPosition, enemy.transform.Find("Visual").lossyScale,
                    UsesVerticalRubble ? verticalRubbleGroundDepthFraction : 0);
                elapsed = 0;
                while (elapsed < dropSeconds)
                {
                    if (IsStunned()) yield break;
                    float t = Mathf.Clamp01(elapsed / dropSeconds);
                    ActiveRubble.SetAirPosition(Vector3.Lerp(LaunchPosition, ActiveRubble.LandingVisualPosition, t));
                    yield return null; elapsed += Time.deltaTime;
                }
                if (!lifetime.IsCurrent(generation) || IsStunned()
                    || !ResiduePlacementSafety.CanPlace(landingCenter, landingDirection, footprint)) yield break;
                ActiveRubble.Land(propSeconds); landed = true; LandingCount++;
                ApplyLanding(damage); ClearCrack();
                Phase = HardenedResiduePhase.Recovery; enemy.SetPatternFrame(recoveryFrame);
                elapsed = 0;
                while (elapsed < recovery) { yield return null; elapsed += Time.deltaTime; }
            }
            finally
            {
                enemy.ReleasePatternFacing();
                ClearCrack();
                if (!landed && ActiveRubble != null) ActiveRubble.Release();
                if (enemy != null && lifetime.IsCurrent(generation))
                { enemy.SetPatternFrame(idle[0]); nextReady = Time.time + rearm; Phase = HardenedResiduePhase.Ready; }
            }
        }

        private void ApplyLanding(EnemyDamageRequest damage)
        {
            PlayerController player = PlayerController.Instance;
            if (player == null || player.IsDead || !ActiveRubble.ContainsPoint(player.transform.position)
                || MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position)) return;
            var biome = BiomeManager.Active;
            if (biome != null)
            {
                Vector2Int a = biome.WorldToGrid(landingCenter), b = biome.WorldToGrid(player.transform.position);
                if (biome.GetHeightLevel(a.x, a.y) != biome.GetHeightLevel(b.x, b.y)) return;
            }
            player.TakeDamage(damage); // One landing hit; the remaining obstacle never inflicts contact damage.
        }

        private void CreateCrack()
        {
            marker = new GameObject("I02_LandingCrack"); marker.transform.position = landingCenter + Vector3.up * .06f;
            marker.transform.rotation = Quaternion.LookRotation(landingDirection);
            lifetime.Own(marker, item => { item.SetActive(false); Destroy(item); });
            EnemyGroundTelegraph.Rectangle(marker.transform, footprint, dangerFillColor);
            if (lineMaterial == null) lineMaterial = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave };
            var crack = marker.AddComponent<LineRenderer>(); crack.sharedMaterial = lineMaterial; crack.useWorldSpace = false;
            crack.widthMultiplier = .065f; crack.sortingOrder = 82; crack.startColor = crack.endColor = crackColor;
            crack.positionCount = 9;
            for (int i = 0; i < 9; i++) crack.SetPosition(i, new Vector3(Mathf.Lerp(-footprint.x * .5f, footprint.x * .5f, i / 8f), 0,
                i == 0 || i == 8 ? 0 : (i % 2 == 0 ? 1 : -1) * footprint.y * .28f));
            var edge = new GameObject("LandingBoundary").AddComponent<LineRenderer>(); edge.transform.SetParent(marker.transform, false);
            edge.sharedMaterial = lineMaterial; edge.useWorldSpace = false; edge.loop = true; edge.positionCount = 4;
            edge.widthMultiplier = .025f; edge.sortingOrder = 80;
            edge.startColor = edge.endColor = new Color(crackColor.r, crackColor.g, crackColor.b, .55f);
            edge.SetPositions(new[] { new Vector3(-footprint.x * .5f, 0, -footprint.y * .5f), new Vector3(footprint.x * .5f, 0, -footprint.y * .5f),
                new Vector3(footprint.x * .5f, 0, footprint.y * .5f), new Vector3(-footprint.x * .5f, 0, footprint.y * .5f) });
        }

        private void ClearCrack() { if (marker != null && lifetime != null) lifetime.ReleaseOwned(marker); marker = null; }
        private bool IsStunned() => enemy.StatusEffects != null && enemy.StatusEffects.IsStunned;
        private void OnDamaged(EnemyController source, float damage) { if (damage > 0) hitUntil = Time.time + .12f; }
        private void OnDefeated(EnemyController source) => EndSpawn();

        private void CreateShadow(Vector2 size, Color color)
        {
            if (shadow == null)
            {
                shadow = new GameObject("Residue_GroundShadow"); shadow.transform.SetParent(transform, false);
                shadow.AddComponent<SpriteRenderer>();
            }
            var renderer = shadow.GetComponent<SpriteRenderer>(); renderer.sprite = groundDisc; renderer.color = color; renderer.sortingOrder = 56;
            shadow.transform.rotation = Quaternion.Euler(90, 0, 0); shadow.transform.localScale = new Vector3(size.x * .5f, size.y * .5f, 1);
            shadow.SetActive(true); LateUpdate();
        }

        private void LateUpdate()
        {
            if (shadow == null || !shadow.activeSelf) return;
            Vector3 position = transform.position;
            if (BiomeManager.Active != null) position.y = BiomeManager.Active.GetGroundHeight(position);
            shadow.transform.position = position + Vector3.up * .035f;
        }

        public override void EndSpawn()
        {
            if (enemy != null) { enemy.Defeated -= OnDefeated; enemy.DamageTaken -= OnDamaged; }
            lifetime?.Cancel(); ClearCrack();
            if (enemy != null) enemy.ReleasePatternFacing();
            if (shadow != null) shadow.SetActive(false);
            Phase = HardenedResiduePhase.Inactive; enabled = false;
        }

        private void OnDisable() { if (Phase != HardenedResiduePhase.Inactive) EndSpawn(); }
    }
}
