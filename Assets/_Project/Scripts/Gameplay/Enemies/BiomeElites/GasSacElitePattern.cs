using System.Collections;
using UnityEngine;

namespace Necrocis
{
    public enum GasSacPhase { Inactive, Ready, Windup, Burst, Recovery, OrbWindup, OrbRecovery }

    [DisallowMultipleComponent]
    public sealed partial class GasSacElitePattern : MonsterPatternController
    {
        private EnemyController enemy;
        private EnemyPatternLifetime lifetime;
        private uint generation;
        private float triggerDistance, radius, windup, burstVisual, recovery, rearm, nextReady;
        private Vector3 anchor;
        private Sprite idle, deflated, puffSprite, filledCircle;
        private Sprite[] inflation;
        private Color warningColor, dangerFillColor, burstColor, gasColor;
        private GameObject marker;
        private LineRenderer boundary;
        private MeshRenderer fill;
        private readonly SpriteRenderer[] puffs = new SpriteRenderer[8];
        private static Material ringMaterial;
        private GameObject groundShadow;
        private Vector2 shadowSize;
        private Color shadowColor;
        private float shadowPoseScale = 1;

        public GasSacPhase Phase { get; private set; }
        public int BurstCount { get; private set; }
        public float BurstRadius => radius;
        public Vector3 TelegraphCenter => marker != null ? marker.transform.position : transform.position;
        public GameObject TelegraphObject => marker;
        public float NextReadyTime => nextReady;
        public float WindupDuration => windup;
        public float RecoveryDuration => recovery;
        public GameObject GroundShadowObject => groundShadow;

        public void Initialize(EnemyController source, GasSacPatternSettings settings)
        {
            EndSpawn();
            enemy = source;
            lifetime = source.GetComponent<EnemyPatternLifetime>() ?? source.gameObject.AddComponent<EnemyPatternLifetime>();
            lifetime.Bind(source);
            generation = source.SpawnGeneration;
            anchor = source.transform.position;
            triggerDistance = settings.triggerDistance; radius = settings.burstRadius;
            windup = settings.windupSeconds; burstVisual = settings.burstVisualSeconds;
            recovery = settings.recoverySeconds; rearm = source.GetRearmCooldown(settings.rearmSeconds);
            nextReady = Time.time + settings.spawnGraceSeconds;
            CaptureOrbSettings(settings);
            GasSacPresentation art = settings.presentation;
            // Configure already cloned this rule for the spawned actor. Presentation remains the only authoring source.
            source.Config.deathSprites = (Sprite[])art.deathFrames.Clone();
            source.Config.deathAnimationSpeed = art.deathFrameSeconds;
            source.BindPatternDirections(art.directionalPresentation);
            idle = art.idle; deflated = art.deflated; puffSprite = art.gasPuff; filledCircle = art.filledCircle;
            inflation = (Sprite[])art.inflationFrames.Clone();
            warningColor = art.warningColor; burstColor = art.burstColor; gasColor = art.gasColor;
            dangerFillColor = art.dangerFillColor;
            shadowSize = art.groundShadowSize * Mathf.Abs(source.Config.scale.x);
            shadowColor = art.groundShadowColor;
            shadowPoseScale = 1;
            CreateGroundShadow();
            BurstCount = 0;
            Phase = GasSacPhase.Ready;
            source.SetAiSuppressed(true); // This pattern owns movement and attacks; the default melee FSM is inactive.
            source.AlignPatternFeetToGround();
            source.SetPatternFrame(idle);
            if (PlayerController.Instance != null) source.SetPatternFacing(PlayerController.Instance.transform.position - source.transform.position);
            source.Defeated += HandleDefeated;
            enabled = true;
        }

        private void Update()
        {
            if (lifetime == null || !lifetime.IsCurrent(generation)) { EndSpawn(); return; }
            if (Phase != GasSacPhase.Ready || (enemy.StatusEffects != null && enemy.StatusEffects.IsStunned)) return;
            if (TryBeginAttack()) return;
            PlayerController player = PlayerController.Instance;
            bool chase = player != null && enemy.IsPlayerInChaseRange() && !enemy.IsOutOfLeash();
            Vector3 destination = chase ? player.transform.position : anchor;
            Vector3 direction = destination - enemy.transform.position; direction.y = 0;
            float stopDistance = chase ? triggerDistance * .8f : .15f;
            if (direction.magnitude > stopDistance)
                enemy.MoveByExternalPattern(direction.normalized * Mathf.Min(enemy.Stats.MoveSpeed * Time.deltaTime, direction.magnitude - stopDistance));
            enemy.SetPatternFacing(direction);
            enemy.SetPatternFrame(idle);
        }

        public bool TryBeginBurst()
        {
            if (enemy == null || lifetime == null || !lifetime.IsCurrent(generation) || Phase != GasSacPhase.Ready
                || Time.time < nextReady || (enemy.StatusEffects != null && enemy.StatusEffects.IsStunned)) return false;
            PlayerController player = PlayerController.Instance;
            if (player == null || player.IsDead || !enemy.IsPlayerInChaseRange()) return false;
            if (ActiveOrb != null || enemy.IsOutOfLeash()) return false;
            Vector3 delta = player.transform.position - enemy.transform.position; delta.y = 0;
            if (delta.sqrMagnitude > triggerDistance * triggerDistance) return false;
            SelectedAttack = GasSacAttackKind.Burst;
            return lifetime.TryStartCycle(BurstCycle(delta));
        }

        private IEnumerator BurstCycle(Vector3 direction)
        {
            EnemyDamageRequest damage = enemy.CreatePatternDamage(GasSacPatternSettings.DamageId, 0);
            Phase = GasSacPhase.Windup;
            enemy.CommitPatternFacing(direction);
            try
            {
                CreateMarker();
                float elapsed = 0;
                while (elapsed < windup)
                {
                    if (enemy.StatusEffects != null && enemy.StatusEffects.IsStunned) yield break;
                    float t = Mathf.Clamp01(elapsed / windup);
                    shadowPoseScale = Mathf.Lerp(1, 1.18f, t);
                    UpdateMarkerPosition();
                    enemy.SetPatternFrame(inflation[Mathf.Min(inflation.Length - 1, Mathf.FloorToInt(t * inflation.Length))]);
                    yield return null;
                    elapsed += Time.deltaTime;
                }
                if (!lifetime.IsCurrent(generation) || (enemy.StatusEffects != null && enemy.StatusEffects.IsStunned)) yield break;
                UpdateMarkerPosition();
                Phase = GasSacPhase.Burst;
                BurstCount++;
                enemy.SetPatternFrame(deflated);
                shadowPoseScale = 1.25f;
                boundary.startColor = boundary.endColor = burstColor;
                boundary.widthMultiplier = .12f;
                EnemyGroundTelegraph.SetColor(fill, new Color(burstColor.r, burstColor.g, burstColor.b, Mathf.Max(dangerFillColor.a, .5f)));
                ApplyBurst(damage); // Single application, never a per-frame damage zone.
                elapsed = 0;
                while (elapsed < burstVisual)
                {
                    float t = Mathf.Clamp01(elapsed / burstVisual);
                    for (int i = 0; i < puffs.Length; i++)
                    {
                        float angle = i * Mathf.PI * 2 / puffs.Length;
                        puffs[i].gameObject.SetActive(true);
                        puffs[i].transform.localPosition = new Vector3(Mathf.Cos(angle), .35f, Mathf.Sin(angle)) * (radius * (.2f + t * .65f));
                        puffs[i].transform.localScale = Vector3.one * (.5f + t * .4f);
                        puffs[i].color = new Color(gasColor.r, gasColor.g, gasColor.b, gasColor.a * (1f - t));
                    }
                    yield return null; elapsed += Time.deltaTime;
                }
                ClearMarker();
                Phase = GasSacPhase.Recovery;
                elapsed = 0;
                while (elapsed < recovery) { yield return null; elapsed += Time.deltaTime; }
            }
            finally
            {
                ClearMarker();
                if (enemy != null && enemy.SpawnGeneration == generation) enemy.ReleasePatternFacing();
                if (enemy != null && lifetime.IsCurrent(generation))
                {
                    enemy.SetPatternFrame(idle);
                    shadowPoseScale = 1;
                    nextReady = Time.time + rearm;
                    Phase = GasSacPhase.Ready;
                }
            }
        }

        private void ApplyBurst(EnemyDamageRequest damage)
        {
            PlayerController player = PlayerController.Instance;
            if (player == null || player.IsDead || MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position)) return;
            Vector3 delta = player.transform.position - TelegraphCenter; delta.y = 0;
            if (delta.sqrMagnitude > radius * radius) return;
            BiomeManager biome = BiomeManager.Active;
            if (biome != null)
            {
                Vector2Int origin = biome.WorldToGrid(TelegraphCenter);
                Vector2Int target = biome.WorldToGrid(player.transform.position);
                if (biome.GetHeightLevel(origin.x, origin.y) != biome.GetHeightLevel(target.x, target.y)) return;
            }
            player.TakeDamage(damage);
        }

        private void CreateMarker()
        {
            marker = new GameObject("GasSac_A_Telegraph");
            lifetime.Own(marker, item => { item.SetActive(false); Destroy(item); });
            UpdateMarkerPosition();
            if (ringMaterial == null) ringMaterial = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave };
            boundary = marker.AddComponent<LineRenderer>();
            boundary.sharedMaterial = ringMaterial; boundary.loop = true; boundary.useWorldSpace = false;
            boundary.positionCount = 96; boundary.widthMultiplier = .07f; boundary.sortingOrder = 80;
            boundary.startColor = boundary.endColor = warningColor;
            for (int i = 0; i < boundary.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2 / boundary.positionCount;
                boundary.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
            }
            fill = EnemyGroundTelegraph.Circle(marker.transform, radius, dangerFillColor);
            for (int i = 0; i < puffs.Length; i++)
            {
                puffs[i] = CreateSprite("Gas", puffSprite, marker.transform);
                puffs[i].sortingOrder = 3000;
                puffs[i].gameObject.AddComponent<Billboard>();
                puffs[i].gameObject.SetActive(false);
            }
        }

        private static SpriteRenderer CreateSprite(string name, Sprite sprite, Transform parent)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite;
            return renderer;
        }

        private void UpdateMarkerPosition()
        {
            if (marker == null) return;
            Vector3 center = enemy.transform.position;
            center.y = (BiomeManager.Active != null ? BiomeManager.Active.GetGroundHeight(center) : center.y) + .08f;
            marker.transform.position = center;
        }

        private void ClearMarker()
        {
            if (marker != null && lifetime != null) lifetime.ReleaseOwned(marker);
            marker = null; boundary = null; fill = null;
        }

        private void CreateGroundShadow()
        {
            // Persistent visual child, like the body renderer; never part of a damage cycle.
            if (groundShadow == null)
            {
                groundShadow = new GameObject("GasSac_GroundShadow");
                groundShadow.transform.SetParent(transform, false);
                for (int i = 0; i < 3; i++)
                {
                    SpriteRenderer layer = CreateSprite("ContactShadow_" + i, filledCircle, groundShadow.transform);
                    layer.sortingOrder = 55 + i;
                    layer.transform.localScale = Vector3.one * (1f - i * .18f);
                }
            }
            SpriteRenderer[] layers = groundShadow.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].sprite = filledCircle;
                layers[i].color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowColor.a * (.4f + i * .22f));
            }
            groundShadow.SetActive(true);
            UpdateGroundShadow();
        }

        private void LateUpdate() => UpdateGroundShadow();

        private void UpdateGroundShadow()
        {
            if (groundShadow == null || !groundShadow.activeSelf || enemy == null) return;
            Vector3 position = enemy.transform.position;
            position.y = (BiomeManager.Active != null ? BiomeManager.Active.GetGroundHeight(position) : position.y) + .035f;
            groundShadow.transform.position = position;
            groundShadow.transform.rotation = Quaternion.Euler(90, 0, 0);
            groundShadow.transform.localScale = new Vector3(shadowSize.x * .5f, shadowSize.y * .5f, 1) * shadowPoseScale;
        }

        private void HandleDefeated(EnemyController source) => EndSpawn();

        public override void EndSpawn()
        {
            if (enemy != null) enemy.Defeated -= HandleDefeated;
            lifetime?.Cancel();
            if (enemy != null) enemy.ReleasePatternFacing();
            ClearMarker();
            if (groundShadow != null) groundShadow.SetActive(false);
            if (enemy != null && idle != null) enemy.SetPatternFrame(idle);
            Phase = GasSacPhase.Inactive;
            enabled = false;
        }

        private void OnDisable() { if (Phase != GasSacPhase.Inactive) EndSpawn(); }
    }
}
