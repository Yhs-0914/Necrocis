using System.Collections;
using UnityEngine;

namespace Necrocis
{
    /// <summary>Transform into the dedicated cerebrum artwork, detach and pursue the player.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(FinalBossArena))]
    public sealed partial class FinalBossPhaseThreeController : MonoBehaviour
    {
        [SerializeField, Min(.1f)] private float detachDuration = 1.8f;
        [SerializeField, Min(.1f)] private float pursuitSpeed = 3.1f;
        [SerializeField, Min(.1f)] private float stoppingDistance = 3.2f;
        [SerializeField, Range(.4f, 1f)] private float mobileScale = .65f;

        private FinalBossArena arena;
        private Transform mobileRoot;
        private Transform bossVisual;
        private EnemyController damageTarget;
        private SpriteRenderer bossRenderer;
        private Vector3 restingVisualPosition;
        private Vector2 footprint;
        private bool detaching;
        private Rigidbody targetBody;
        private RigidbodyInterpolation previousInterpolation;
        private Sprite mobileSprite;
        private Material mobileMaterial;
        private bool appearanceApplied;
        private Vector3 mobileVisualScale;
        private static Material transparentMobileMaterial;
        private float bodyLean;
        private BoxCollider[] artworkHitboxes;
        // Normalized image-space regions (bottom-left origin). Small overlaps forgive edges,
        // while separate tentacle regions leave the large transparent corners unhittable.
        private static readonly string[] ArtworkHitboxNames =
        {
            "PhaseIII_LowerBrainHitbox", "PhaseIII_CrownHitbox", "PhaseIII_StemHitbox",
            "PhaseIII_LeftTentacleHitbox", "PhaseIII_RightTentacleHitbox",
            "PhaseIII_LeftLowerTentacleHitbox", "PhaseIII_RightLowerTentacleHitbox"
        };
        private static readonly Rect[] ArtworkHitboxRegions =
        {
            Rect.MinMaxRect(.14f, .44f, .88f, .77f),
            Rect.MinMaxRect(.23f, .73f, .84f, .96f),
            Rect.MinMaxRect(.34f, .25f, .70f, .53f),
            Rect.MinMaxRect(.00f, .32f, .37f, .58f),
            Rect.MinMaxRect(.66f, .19f, 1.00f, .50f),
            Rect.MinMaxRect(.20f, .04f, .47f, .36f),
            Rect.MinMaxRect(.53f, .02f, .81f, .36f)
        };

        public bool HasMobileAppearance => appearanceApplied;

        public bool IsActive { get; private set; }
        public bool IsDetaching => detaching;
        public Vector3 Position => mobileRoot != null ? mobileRoot.position : transform.position;

        public void Begin(Transform visual, EnemyController target, Transform runtimeParent)
        {
            if (IsActive || visual == null || target == null) return;
            mobileSprite = Resources.Load<Sprite>("FinalBoss/PhaseThree/CerebrumBoss");
            mobileMaterial = Resources.Load<Material>("FinalBoss/PhaseThree/CerebrumBoss");
            Sprite newArtwork = Resources.Load<Sprite>("FinalBoss/PhaseThree/MobileCerebrum");
            if (newArtwork != null)
            {
                mobileSprite = newArtwork;
                if (transparentMobileMaterial == null)
                    transparentMobileMaterial = new Material(Shader.Find("Sprites/Default"))
                    { name = "PhaseIII_AlphaArtwork", hideFlags = HideFlags.HideAndDontSave };
                mobileMaterial = transparentMobileMaterial;
            }
            if (mobileSprite == null || mobileMaterial == null)
            {
                Debug.LogError("[FinalBoss] Missing dedicated phase-three CerebrumBoss sprite/material.");
                return;
            }
            arena = GetComponent<FinalBossArena>();
            bossVisual = visual;
            damageTarget = target;
            bossRenderer = visual.GetComponent<SpriteRenderer>();
            footprint = new Vector2(9f * mobileScale * .5f + .1f,
                3.5f * mobileScale * .5f + .1f);
            mobileVisualScale = Vector3.one * (12.5f / Mathf.Max(.01f, mobileSprite.bounds.size.x));
            ConfigureMobileHitboxes();

            GameObject root = new GameObject("FinalBoss_PhaseThree_MobileBody");
            root.transform.SetParent(runtimeParent, true);
            root.transform.position = target.transform.position;
            mobileRoot = root.transform;
            GameObject effects = new GameObject("FinalBoss_PhaseThree_AttackEffects");
            effects.transform.SetParent(runtimeParent, false);
            effectRoot = effects.transform;
            bossVisual.SetParent(mobileRoot, true);
            damageTarget.transform.SetParent(mobileRoot, true);
            damageTarget.SetAiSuppressed(true);
            targetBody = damageTarget.GetComponent<Rigidbody>();
            if (targetBody != null)
            {
                previousInterpolation = targetBody.interpolation;
                targetBody.interpolation = RigidbodyInterpolation.None;
            }
            SpriteYSort sorting = bossVisual.GetComponent<SpriteYSort>();
            if (sorting != null) sorting.SetUpdateMode(SpriteYSort.UpdateMode.Continuous);
            IsActive = true;
            detaching = true;
            StartCoroutine(Detach());
        }

        private void ConfigureMobileHitboxes()
        {
            BoxCollider core = damageTarget.GetComponent<BoxCollider>();
            if (core == null) return;

            // Preserve a small, ground-level target for attacks aimed at the enemy anchor.
            // The image-facing volumes below cover the actual brain and all four major limbs.
            core.center = new Vector3(0f, 1f, 0f);
            core.size = new Vector3(4f, 2f, 3.25f);
            artworkHitboxes = new BoxCollider[ArtworkHitboxNames.Length];
            for (int i = 0; i < artworkHitboxes.Length; i++)
                artworkHitboxes[i] = ConfigureHitbox(ArtworkHitboxNames[i], Vector3.zero, Vector3.one, core);
            UpdateArtworkHitboxes();
            Physics.SyncTransforms();
        }

        private BoxCollider ConfigureHitbox(string objectName, Vector3 center, Vector3 size, BoxCollider source)
        {
            Transform child = damageTarget.transform.Find(objectName);
            if (child == null)
            {
                GameObject go = new GameObject(objectName);
                go.layer = damageTarget.gameObject.layer;
                child = go.transform;
                child.SetParent(damageTarget.transform, false);
            }
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
            // Unity can return a native-null component wrapper. ?? does not use Unity's
            // overloaded null check and can skip AddComponent, aborting the phase transition.
            BoxCollider hitbox = child.GetComponent<BoxCollider>();
            if (hitbox == null) hitbox = child.gameObject.AddComponent<BoxCollider>();
            hitbox.center = center;
            hitbox.size = size;
            hitbox.isTrigger = source.isTrigger;
            hitbox.sharedMaterial = source.sharedMaterial;
            hitbox.enabled = true;
            return hitbox;
        }

        private void UpdateArtworkHitboxes()
        {
            if (artworkHitboxes == null || mobileSprite == null || bossVisual == null) return;
            Bounds bounds = mobileSprite.bounds;
            // Extrude away from the camera down toward the attack plane. This lets ground-level
            // projectiles hit the displayed head without inflating its screen-space silhouette.
            float depth = Mathf.Max(bounds.size.y, bounds.size.x) * 1.15f;
            for (int i = 0; i < artworkHitboxes.Length; i++)
            {
                BoxCollider hitbox = artworkHitboxes[i];
                if (hitbox == null) continue;
                Rect region = ArtworkHitboxRegions[i];
                float x = Mathf.Lerp(bounds.min.x, bounds.max.x, region.center.x);
                if (bossRenderer != null && bossRenderer.flipX) x = -x;
                hitbox.center = new Vector3(x,
                    Mathf.Lerp(bounds.min.y, bounds.max.y, region.center.y), depth * .5f);
                hitbox.size = new Vector3(bounds.size.x * region.width, bounds.size.y * region.height, depth);
                // Both objects share the mobile parent, but the artwork bobs, leans and pulses.
                hitbox.transform.SetPositionAndRotation(bossVisual.position, bossVisual.rotation);
                hitbox.transform.localScale = bossVisual.localScale;
            }
        }

        private IEnumerator Detach()
        {
            Vector3 start = mobileRoot.position;
            Vector3 entry = FindEntryPosition();
            Vector3 visualStart = bossVisual.localPosition;
            Vector3 originalVisualScale = bossVisual.localScale;
            Vector3 targetStart = damageTarget.transform.localPosition;
            restingVisualPosition = new Vector3(0f, .35f, 0f);
            AudioManager.Instance?.PlaySFX("BossPhaseChange", .8f);
            DontStarveCamera.Instance?.AddCombatImpulse(.22f, .3f);

            for (float elapsed = 0f; elapsed < detachDuration && IsActive; elapsed += Time.deltaTime)
            {
                float normalized = Mathf.Clamp01(elapsed / detachDuration);
                float t = Mathf.SmoothStep(0f, 1f, normalized);
                mobileRoot.position = Vector3.Lerp(start, entry, t);
                mobileRoot.localScale = Vector3.one * Mathf.Lerp(1f, mobileScale, t);
                if (normalized >= .22f && !appearanceApplied) ApplyMobileAppearance();
                if (appearanceApplied)
                    bossVisual.localScale = Vector3.Lerp(originalVisualScale, mobileVisualScale,
                        Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((normalized - .22f) / .78f)));
                bossVisual.localPosition = Vector3.Lerp(visualStart, restingVisualPosition, t)
                    + Vector3.up * Mathf.Sin(normalized * Mathf.PI) * 1.5f;
                damageTarget.transform.localPosition = Vector3.Lerp(targetStart, Vector3.zero, t);
                SyncDamageTarget();
                yield return null;
            }
            if (!IsActive) yield break;
            mobileRoot.position = entry;
            mobileRoot.localScale = Vector3.one * mobileScale;
            if (!appearanceApplied) ApplyMobileAppearance();
            bossVisual.localScale = mobileVisualScale;
            bossVisual.localPosition = restingVisualPosition;
            damageTarget.transform.localPosition = Vector3.zero;
            SyncDamageTarget();
            UpdateArtworkHitboxes();
            detaching = false;
            AudioManager.Instance?.PlaySFX("BossRoar", .7f);
            DontStarveCamera.Instance?.AddCombatImpulse(.14f, .2f);
            StartCoroutine(PatternLoop());
        }

        private void ApplyMobileAppearance()
        {
            // The old outline owns a mesh cropped from the arena. Release it before swapping
            // to the separate brain-and-tentacles image so it cannot rebuild over the new sprite.
            SpriteOutline outline = bossVisual.GetComponent<SpriteOutline>();
            if (outline != null) outline.enabled = false;
            Billboard billboard = bossVisual.GetComponent<Billboard>();
            if (billboard != null) billboard.enabled = false;
            bossRenderer.sprite = mobileSprite;
            bossRenderer.sharedMaterial = mobileMaterial;
            bossRenderer.color = Color.white;
            bossRenderer.flipX = false;
            appearanceApplied = true;
            Camera camera = DontStarveCamera.GetActiveCamera();
            if (camera != null) bossVisual.rotation = camera.transform.rotation;
            UpdateArtworkHitboxes();
        }

        private Vector3 FindEntryPosition()
        {
            // The old boss anchor is outside the navigable floor. Enter through a valid interior cell.
            Vector3 preferred = arena.UVToWorld(new Vector2(.5f, .7f));
            if (TryGroundedPosition(preferred, out Vector3 result)) return result;
            for (int z = Mathf.RoundToInt(arena.WorldSize.y * .7f); z >= 7; z--)
            for (int x = 5; x < arena.WorldSize.x - 5; x++)
                if (TryGroundedPosition(new Vector3(x + .5f, 0f, z + .5f), out result)) return result;
            // An invalid arena should not teleport the boss into blocked terrain.
            StopMovement();
            Debug.LogError("[FinalBoss] No walkable phase-three entry position.");
            return mobileRoot.position;
        }

        private bool TryGroundedPosition(Vector3 candidate, out Vector3 result)
        {
            result = candidate;
            if (!arena.IsWalkable(candidate, footprint)) return false;
            result.y = GroundHeight(candidate);
            return true;
        }

        private void Update()
        {
            if (!IsActive || detaching || mobileRoot == null || Time.deltaTime <= 0f) return;
            PlayerController player = PlayerController.Instance;
            if (player == null || player.IsDead || damageTarget == null || damageTarget.IsDead)
            {
                StopMovement();
                return;
            }
            UpdateCollapse();
            if (attacking) return;
            Vector3 position = mobileRoot.position;
            Vector3 step = CalculatePursuitStep(position, player.transform.position,
                pursuitSpeed, stoppingDistance, Time.deltaTime);
            if (step.sqrMagnitude <= .000001f) return;
            if (!TryMove(position, step))
            {
                // Slide along arena boundaries rather than passing through the authored terrain.
                if (Mathf.Abs(step.x) >= Mathf.Abs(step.z))
                {
                    if (!TryMove(position, new Vector3(step.x, 0f, 0f)))
                        TryMove(position, new Vector3(0f, 0f, step.z));
                }
                else if (!TryMove(position, new Vector3(0f, 0f, step.z)))
                    TryMove(position, new Vector3(step.x, 0f, 0f));
            }
        }

        private bool TryMove(Vector3 position, Vector3 step)
        {
            if (step.sqrMagnitude <= .000001f) return false;
            Vector3 candidate = position + step;
            if (!arena.CanTraverse(position, candidate, footprint)
                || !CanCrossBoundary(position, candidate, footprint)) return false;
            candidate.y = GroundHeight(candidate);
            mobileRoot.position = candidate;
            SyncDamageTarget();
            if (bossRenderer != null && Mathf.Abs(step.x) > .001f) bossRenderer.flipX = step.x < 0f;
            return true;
        }

        private void LateUpdate()
        {
            if (!IsActive || bossVisual == null) return;
            if (appearanceApplied)
            {
                Camera camera = DontStarveCamera.GetActiveCamera();
                if (camera != null) bossVisual.rotation = camera.transform.rotation * Quaternion.Euler(0f, 0f, bodyLean);
            }
            if (detaching)
            {
                if (appearanceApplied) UpdateArtworkHitboxes();
                return;
            }
            bossVisual.localPosition = restingVisualPosition + Vector3.up * Mathf.Sin(Time.time * 2.7f) * .12f;
            float pulse = Mathf.Sin(Time.time * (attacking ? 8f : 2.7f)) * (attacking ? .024f : .012f);
            float charge = Time.time < chargeEnd && chargeDuration > 0f
                ? Mathf.Clamp01(1f - (chargeEnd - Time.time) / chargeDuration) * .06f : 0f;
            bossVisual.localScale = Vector3.Scale(mobileVisualScale, new Vector3(1f + pulse + charge, 1f - pulse - charge * .5f, 1f));
            if (bossRenderer != null)
                bossRenderer.color = reflectingAppearance ? new Color(.55f, .95f, 1f)
                    : Color.Lerp(Color.white, new Color(1f, .6f, .72f), charge * 8f);
            UpdateArtworkHitboxes();
        }

        private float GroundHeight(Vector3 position)
        {
            return BiomeManager.Active != null
                ? BiomeManager.Active.GetGroundHeight(position) : arena.SpawnPosition.y;
        }

        private void SyncDamageTarget()
        {
            Vector3 position = mobileRoot.position;
            position.y = GroundHeight(position);
            damageTarget.SetPositionFromExternalPattern(position);
        }

        private static Vector3 CalculatePursuitStep(Vector3 position, Vector3 target,
            float speed, float stopDistance, float deltaTime)
        {
            Vector3 delta = target - position;
            delta.y = 0f;
            float distance = delta.magnitude;
            float travel = Mathf.Min(Mathf.Max(0f, speed) * Mathf.Max(0f, deltaTime),
                Mathf.Max(0f, distance - Mathf.Max(0f, stopDistance)));
            return distance > .0001f ? delta * (travel / distance) : Vector3.zero;
        }

        public void StopMovement()
        {
            ReleaseGrab();
            GetComponent<FinalBossPhaseTwoController>()?.SetMobileReflection(false);
            if (effectRoot != null)
            {
                effectRoot.gameObject.SetActive(false);
                Destroy(effectRoot.gameObject);
            }
            if (boundaryVisual != null) Destroy(boundaryVisual.gameObject);
            if (boundaryMaterial != null) Destroy(boundaryMaterial);
            collapseStarted = false;
            attacking = false;
            reflectingAppearance = false;
            CurrentMobilePattern = MobilePattern.None;
            IsActive = false;
            detaching = false;
            StopAllCoroutines();
            if (targetBody != null) targetBody.interpolation = previousInterpolation;
        }

        private void OnDisable() => StopMovement();
    }
}
