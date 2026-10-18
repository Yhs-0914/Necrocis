using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// Basic player attack controller.
    /// Q = melee, W = ranged.
    /// </summary>
    public partial class PlayerAttack : MonoBehaviour, IBasicAttackDisplayReadModel
    {
        private const string MeleeSlashPoolName = "PlayerAttack.MeleeSlash";
        private static readonly System.Func<GameObject> CreateMeleeSlashFunc = BasicMeleeSlashVisual.CreateObject;
        private const string BeamVisualPoolName = "PlayerAttack.BeamVisual";
        private static readonly System.Func<GameObject> CreateBeamVisualFunc = CreateBeamVisualObject;

        [Header("Melee Attack (Q)")]
        [SerializeField] private float meleeAttackDamage = 20f;
        [SerializeField] private Vector3 meleeAttackBoxSize = new Vector3(3f, 3f, 3f);
        [SerializeField] private float meleeAttackOffset = 2f;
        [SerializeField] private LayerMask meleeTargetMask = ~0;
        [SerializeField, Min(1)] private int meleeOverlapBufferSize = 24;

        [Header("Ranged Attack")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private float projectileSpawnOffset = 0.65f;
        [SerializeField] private float projectileSpawnHeight = 1f;
        [SerializeField] private float projectileSpawnExtraHeight = 2f;
        [SerializeField] private float projectileRange = 8f;
        [SerializeField] private float rangedAttackDamage = 10f;
        [SerializeField] private LayerMask rangedTargetMask = ~0;

        [Header("Beam")]
        [SerializeField, Min(1)] private int beamOverlapBufferSize = 48;
        [SerializeField] private float beamVerticalHalfHeight = 2.5f;
        [SerializeField] private float beamHeightOffset = -0.3f;
        [SerializeField] private float beamVisualDuration = 0.1f;
        [SerializeField] private string beamTextureResourcePath = "ItemEffects/beam_organ_beam";

        [Header("Shared")]
        [SerializeField] private float meleeCooldown  = 0.2f;
        [SerializeField] private float attackCooldown = 0.3f;
        [SerializeField, Min(0f)] private float cellProliferationDelay = 0.5f;
        [SerializeField] private bool enableDebugLogs;

        [Header("Attack Visuals")]
        [SerializeField] private string meleeSlashSpriteResourcePath = "AttackVisuals/basic_melee_slash";
        [SerializeField] private float meleeSlashLifetime = 0.18f;
        [SerializeField] private float meleeSlashScale = 0.95f;
        [SerializeField] private float meleeSlashHeightOffset = 2f;
        [SerializeField] private int meleeSlashSortingOrder = 5050;

        private PlayerController playerController;
        private IBasicAttackItemEffects itemEffects;
        private bool HasItemEffects => itemEffects != null && itemEffects.IsAvailable;
        private float lastAttackTime = float.NegativeInfinity;
        private float lastMeleeTime = float.NegativeInfinity;
        private Collider[] meleeOverlapResults;
        private Collider[] beamOverlapResults;
        private Sprite meleeSlashSprite;
        private bool meleeSlashSpriteLoadAttempted;
        private Texture2D beamTexture;
        private Material beamMaterial;
        private bool beamTextureLoadAttempted;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = GetComponentInParent<PlayerController>();
            }

            if (playerController == null)
            {
                Debug.LogError("[PlayerAttack] PlayerController not found.");
            }

            itemEffects = GetComponent<IBasicAttackItemEffects>();

            if (meleeTargetMask.value == 0)
            {
                meleeTargetMask = ~0;
                Debug.LogWarning("[PlayerAttack] meleeTargetMask was Nothing. Fallback to Everything.");
            }

            if (rangedTargetMask.value == 0)
            {
                rangedTargetMask = ~0;
                Debug.LogWarning("[PlayerAttack] rangedTargetMask was Nothing. Fallback to Everything.");
            }

            EnsureMeleeOverlapBuffer();
            EnsureBeamOverlapBuffer();
            ResolveRootFirePoint();
        }

        internal void BindRuntime(PlayerController owner, IBasicAttackItemEffects effects)
        {
            playerController = owner;
            itemEffects = effects;
        }

        private void Update()
        {
            HandleAttackInput();
        }

        private void HandleAttackInput()
        {
            if (Time.timeScale <= Mathf.Epsilon)
            {
                return;
            }

            InputManager input = InputManager.Instance;
            if (input == null || !input.Gameplay.CanReadGameplay)
            {
                return;
            }

            if (playerController == null || playerController.IsDead || playerController.IsBossDisplaced || playerController.MovementExecutions.BlocksOrdinaryInput)
            {
                return;
            }

            PlayerStats stats = playerController.Stats;
            if (stats == null || stats.IsDead)
            {
                return;
            }

            float effectiveAttackCooldown = PlayerCombatCalculator.GetBasicAttackCooldown(attackCooldown, stats);
            if (HasItemEffects)
            {
                effectiveAttackCooldown *= itemEffects.GetAttackCooldownMultiplier();
            }
            bool canAttack = Time.time >= lastAttackTime + effectiveAttackCooldown;

            if (input.DebugLevelUpAction.WasPressedThisFrame())
            {
                LevelUpManager.DebugLevelUp();
                return;
            }

            if (input.MeleeAttackAction.WasPressedThisFrame())
            {
                float effectiveMeleeCooldown = PlayerCombatCalculator.GetBasicAttackCooldown(meleeCooldown, stats);
                if (HasItemEffects)
                {
                    effectiveMeleeCooldown *= itemEffects.GetAttackCooldownMultiplier();
                }

                if (Time.time < lastMeleeTime + effectiveMeleeCooldown) return;

                lastMeleeTime = Time.time;
                MeleeAttack();
                return;
            }

            if (input.RangedAttackAction.WasPressedThisFrame())
            {
                if (!canAttack)
                {
                    return;
                }

                lastAttackTime = Time.time;
                RangedAttack();
            }
        }

        private Vector3 GetAttackDirection()
        {
            PlayerController controller = playerController != null ? playerController : PlayerController.Instance;
            if (controller == null)
            {
                return Vector3.forward;
            }

            Vector3 direction = controller.GetLogicalFacingDirection();
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        private void MeleeAttack()
        {
            HealthEffectSource source = playerController.GetHealthEffectSource("basic/melee");
            PlayerStats stats = playerController.Stats;
            playerController?.PlayAttackAnimation(true);
            AudioManager.Instance?.PlayPlayerSfx(PlayerSoundId.MeleeAttack);
            Vector3 direction = GetAttackDirection();
            float rangeMultiplier = HasItemEffects ? itemEffects.GetMeleeRangeMultiplier() : 1f;
            float effectiveAttackOffset = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackOffset * rangeMultiplier, stats);
            Vector3 boxCenter = transform.position + direction * effectiveAttackOffset;
            float configuredWidth = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackBoxSize.x * rangeMultiplier, stats);
            float configuredDepth = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackBoxSize.z * rangeMultiplier, stats);
            Sprite slashSprite = GetMeleeSlashSprite();
            float slashWorldSize = GetMeleeSlashTargetWorldSize(configuredWidth, configuredDepth);
            float effectiveWidth = slashSprite != null ? slashWorldSize : configuredWidth;
            float effectiveDepth = slashSprite != null ? slashWorldSize : configuredDepth;
            Vector3 tallBoxSize = new Vector3(effectiveWidth, 20f, effectiveDepth);
            Quaternion rotation = Quaternion.LookRotation(direction);
            float damageMultiplier = HasItemEffects ? itemEffects.GetOutgoingBasicDamageMultiplier() : 1f;
            float flatDamageBonus = HasItemEffects ? itemEffects.GetOutgoingBasicDamageFlatBonus() : 0f;
            float unstableMultiplier = HasItemEffects ? itemEffects.RollUnstableCoreDamageMultiplier() : 1f;
            float baseDamage = PlayerCombatCalculator.GetBasicAttackDamage(stats, meleeAttackDamage) + flatDamageBonus;
            float finalDamage = baseDamage * damageMultiplier * unstableMultiplier;
            itemEffects?.NotifyBasicAttackPerformed(finalDamage, meleeTargetMask, effectiveAttackOffset + effectiveDepth * 0.5f, direction);
            SpawnMeleeSlashVisual(boxCenter, direction, slashSprite, slashWorldSize);

            EnsureMeleeOverlapBuffer();
            var meleeHitEnemies=new HashSet<EnemyController>();
            int propAttackToken = ResidueRubble.NextAttackToken();
            var hits=EnemyHitSnapshot.Box(boxCenter,
                tallBoxSize/2f+new Vector3(PlayerAttackGeometry.Padding,0,PlayerAttackGeometry.Padding),
                rotation,meleeTargetMask,QueryTriggerInteraction.Collide,ref meleeOverlapResults);
            var hitData=hits;
            int hitCount=hits.Length;

            for (int i = 0; i < hitCount; i++)
            {
                if(!hitData[i].IsCurrent)continue;
                Collider hitCollider = hitData[i].Collider;
                if (hitCollider == null)
                {
                    continue;
                }

                if (hitCollider.TryGetComponent(out ResidueRubble rubble))
                { rubble.ApplyPlayerHit(finalDamage, propAttackToken); continue; }
                EnemyController enemy = hitData[i].Enemy;
                if (enemy == null || enemy.IsDead || !PlayerAttackGeometry.InBox(boxCenter, tallBoxSize / 2f, rotation, enemy) || !meleeHitEnemies.Add(enemy))
                {
                    continue;
                }

                float appliedDamage = HasItemEffects
                    ? itemEffects.ApplyPerTargetDamageModifiers(enemy, finalDamage)
                    : finalDamage;
                var hitLife = new EnemyHitLife(enemy);
                Vector3 hitPosition = enemy.transform.position;
                enemy.TakeDamage(appliedDamage, source);
                itemEffects?.TryApplyPostDamageExecutionInstinct(enemy, appliedDamage, hitLife.Generation);
                itemEffects?.ApplyCommonOnHitEffects(enemy, appliedDamage, hitPosition, hitLife.Generation);
            }
        }

        private void SpawnMeleeSlashVisual(Vector3 center, Vector3 direction, Sprite sprite, float targetWorldSize)
        {
            if (sprite == null) return;
            GameObject pooledVisual = RuntimePool.Acquire(MeleeSlashPoolName, CreateMeleeSlashFunc);
            if (pooledVisual == null) return;
            pooledVisual.GetComponent<BasicMeleeSlashVisual>().Show(this, sprite,
                new Vector3(center.x, transform.position.y + meleeSlashHeightOffset, center.z),
                GetScreenAlignedRotation(direction), GetMeleeSlashWorldScale(sprite, targetWorldSize),
                meleeSlashSortingOrder, Mathf.Max(0.02f, meleeSlashLifetime));
        }

        private Sprite GetMeleeSlashSprite()
        {
            if (meleeSlashSpriteLoadAttempted)
            {
                return meleeSlashSprite;
            }

            meleeSlashSpriteLoadAttempted = true;
            if (string.IsNullOrWhiteSpace(meleeSlashSpriteResourcePath))
            {
                return null;
            }

            meleeSlashSprite = TextureSpriteCache.LoadResourceSprite(meleeSlashSpriteResourcePath);
            if (meleeSlashSprite == null)
            {
                Debug.LogWarning($"[PlayerAttack] Resources/{meleeSlashSpriteResourcePath} sprite not found.");
            }

            return meleeSlashSprite;
        }

        private float GetMeleeSlashTargetWorldSize(float effectiveWidth, float effectiveDepth)
        {
            return Mathf.Max(0.1f, Mathf.Max(effectiveWidth, effectiveDepth) * meleeSlashScale);
        }

        private float GetMeleeSlashWorldScale(Sprite sprite, float targetWorldSize)
        {
            if (sprite == null)
            {
                return Mathf.Max(0.05f, meleeSlashScale);
            }

            float spriteWorldSize = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            if (spriteWorldSize <= 0.0001f)
            {
                return Mathf.Max(0.05f, meleeSlashScale);
            }

            return Mathf.Max(0.05f, targetWorldSize / spriteWorldSize);
        }

        private static Quaternion GetScreenAlignedRotation(Vector3 worldDirection)
        {
            worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude <= 0.0001f)
            {
                worldDirection = Vector3.forward;
            }

            Camera activeCamera = DontStarveCamera.GetActiveCamera();
            if (activeCamera == null)
            {
                float fallbackAngle = Mathf.Atan2(worldDirection.z, worldDirection.x) * Mathf.Rad2Deg;
                return Quaternion.Euler(90f, 0f, fallbackAngle);
            }

            Vector3 projectedDirection = Vector3.ProjectOnPlane(worldDirection.normalized, activeCamera.transform.forward);
            if (projectedDirection.sqrMagnitude <= 0.0001f)
            {
                return activeCamera.transform.rotation;
            }

            projectedDirection.Normalize();
            float x = Vector3.Dot(projectedDirection, activeCamera.transform.right);
            float y = Vector3.Dot(projectedDirection, activeCamera.transform.up);
            float rollAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
            return activeCamera.transform.rotation * Quaternion.AngleAxis(rollAngle, Vector3.forward);
        }


        private void EnsureMeleeOverlapBuffer()
        {
            int size = Mathf.Max(1, meleeOverlapBufferSize);
            if (meleeOverlapResults == null || meleeOverlapResults.Length < size)
            {
                meleeOverlapResults = new Collider[size];
            }
        }

        private void EnsureBeamOverlapBuffer()
        {
            int size = Mathf.Max(1, beamOverlapBufferSize);
            if (HasItemEffects)
            {
                size = Mathf.Max(size, itemEffects.BeamHitBufferSize);
            }

            if (beamOverlapResults == null || beamOverlapResults.Length < size)
            {
                beamOverlapResults = new Collider[size];
            }
        }

        private void RangedAttack()
        {
            playerController?.PlayAttackAnimation(false);
            AudioManager.Instance?.PlayPlayerSfx(PlayerSoundId.RangedAttack);

            Vector3 direction = GetAttackDirection();
            PlayerStats stats = playerController.Stats;
            float flatDamageBonus = HasItemEffects ? itemEffects.GetOutgoingBasicDamageFlatBonus() : 0f;
            float damage = PlayerCombatCalculator.GetBasicAttackDamage(stats, rangedAttackDamage) + flatDamageBonus;
            float unstableMultiplier = 1f;
            if (HasItemEffects)
            {
                damage *= itemEffects.GetOutgoingBasicDamageMultiplier();
                damage *= itemEffects.GetRangedBasicDamageMultiplier();
                unstableMultiplier = itemEffects.RollUnstableCoreDamageMultiplier();
                damage *= unstableMultiplier;
            }
            float effectiveProjectileRange = PlayerCombatCalculator.GetBasicAttackRange(projectileRange, stats);
            itemEffects?.NotifyBasicAttackPerformed(damage, rangedTargetMask, effectiveProjectileRange, direction);

            Vector3 muzzleOrigin = firePoint != null ? firePoint.position : transform.position;
            muzzleOrigin += direction * projectileSpawnOffset;
            muzzleOrigin.y += projectileSpawnHeight + projectileSpawnExtraHeight;
            CombatVfx.PlayRangedMuzzle(muzzleOrigin, direction);

            if (HasItemEffects && itemEffects.HasBeamOrgan)
            {
                FireBeamVolley(
                    direction,
                    damage * itemEffects.BeamDamageMultiplier,
                    effectiveProjectileRange,
                    true);
                return;
            }

            FireProjectileVolley(direction, damage, effectiveProjectileRange, true);
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayerAttack] Fired ranged attack toward {direction}");
            }
        }

        private void FireProjectileVolley(Vector3 direction, float damage, float range, bool allowExtraVolley)
            => FireProjectileVolleyFromSource(direction, damage, range, allowExtraVolley, playerController.GetHealthEffectSource("basic/ranged"));

        private void FireProjectileVolleyFromSource(Vector3 direction, float damage, float range, bool allowExtraVolley, HealthEffectSource source)
        {
            int projectileCount = HasItemEffects ? itemEffects.GetForwardProjectileCount() : 1;
            float spread = HasItemEffects ? itemEffects.GetSpreadAngleForCount(projectileCount) : 0f;
            float accuracyPenalty = HasItemEffects ? itemEffects.GetAccuracyPenaltyAngle() : 0f;
            Vector3 firingDirection = ApplyAccuracyPenalty(direction, accuracyPenalty);
            float forwardDamageMultiplier = HasItemEffects
                ? itemEffects.GetForwardProjectileDamageMultiplier(projectileCount)
                : 1f;
            float forwardDamage = damage * forwardDamageMultiplier;

            if (projectileCount <= 1)
            {
                SpawnProjectileFromSource(firingDirection, forwardDamage, range, Projectile.SpawnKind.Normal, source);
            }
            else
            {
                float center = (projectileCount - 1) * 0.5f;
                for (int i = 0; i < projectileCount; i++)
                {
                    float offset = (i - center) * spread;
                    Vector3 shotDirection = Quaternion.Euler(0f, offset, 0f) * firingDirection;
                    SpawnProjectileFromSource(shotDirection, forwardDamage, range, Projectile.SpawnKind.Normal, source);
                }
            }

            if (HasItemEffects && itemEffects.HasLaryngealNerve)
            {
                SpawnProjectileFromSource(
                    -firingDirection,
                    damage * itemEffects.GetBackShotDamageMultiplier(),
                    range,
                    Projectile.SpawnKind.Normal, itemEffects.GetItemHealthEffectSource(PlayerItemIds.LaryngealNerveId, "projectile").WithEffect(source.EffectId));
            }

            if (allowExtraVolley && HasItemEffects && itemEffects.RollCellProliferation())
            {
                StartCoroutine(FireCellProliferationVolleyDelayed(direction, damage, range));
            }
        }

        private static Vector3 ApplyAccuracyPenalty(Vector3 direction, float penaltyAngle)
        {
            Vector3 safeDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            if (penaltyAngle <= 0f)
            {
                return safeDirection;
            }

            float randomOffset = Random.Range(-penaltyAngle, penaltyAngle);
            return (Quaternion.Euler(0f, randomOffset, 0f) * safeDirection).normalized;
        }

        private IEnumerator FireCellProliferationVolleyDelayed(Vector3 direction, float damage, float range)
            => FireCellProliferationVolleyDelayedFromSource(direction, damage, range,
                itemEffects.GetItemHealthEffectSource(PlayerItemIds.CellProliferationId, "projectile"));

        private IEnumerator FireCellProliferationVolleyDelayedFromSource(Vector3 direction, float damage, float range, HealthEffectSource source)
        {
            if (cellProliferationDelay > 0f)
            {
                yield return new WaitForSeconds(cellProliferationDelay);
            }

            if (!HasItemEffects || !isActiveAndEnabled)
            {
                yield break;
            }

            FireProjectileVolleyFromSource(
                direction,
                damage * itemEffects.CellProliferationDamageMultiplier,
                range,
                false, source);
        }

        private void FireBeamVolley(Vector3 direction, float damage, float range, bool allowExtraVolley)
            => FireBeamVolleyFromSource(direction, damage, range, allowExtraVolley, playerController.GetHealthEffectSource("basic/beam"));

        private void FireBeamVolleyFromSource(Vector3 direction, float damage, float range, bool allowExtraVolley, HealthEffectSource source)
        {
            int beamCount = HasItemEffects ? itemEffects.GetForwardProjectileCount() : 1;
            float spread = HasItemEffects ? itemEffects.GetSpreadAngleForCount(beamCount) : 0f;
            float accuracyPenalty = HasItemEffects ? itemEffects.GetAccuracyPenaltyAngle() : 0f;
            Vector3 firingDirection = ApplyAccuracyPenalty(direction, accuracyPenalty);
            float forwardDamageMultiplier = HasItemEffects
                ? itemEffects.GetForwardProjectileDamageMultiplier(beamCount)
                : 1f;
            float forwardDamage = damage * forwardDamageMultiplier;

            if (beamCount <= 1)
            {
                FireBeamFromSource(firingDirection, forwardDamage, range, source);
            }
            else
            {
                float center = (beamCount - 1) * 0.5f;
                for (int i = 0; i < beamCount; i++)
                {
                    float offset = (i - center) * spread;
                    Vector3 beamDirection = Quaternion.Euler(0f, offset, 0f) * firingDirection;
                    FireBeamFromSource(beamDirection, forwardDamage, range, source);
                }
            }

            if (HasItemEffects && itemEffects.HasLaryngealNerve)
            {
                FireBeamFromSource(
                    -firingDirection,
                    damage * itemEffects.GetBackShotDamageMultiplier(),
                    range, itemEffects.GetItemHealthEffectSource(PlayerItemIds.LaryngealNerveId, "beam").WithEffect(source.EffectId));
            }

            if (allowExtraVolley && HasItemEffects && itemEffects.RollCellProliferation())
            {
                StartCoroutine(FireCellProliferationBeamVolleyDelayed(direction, damage, range));
            }
        }

        private IEnumerator FireCellProliferationBeamVolleyDelayed(Vector3 direction, float damage, float range)
            => FireCellProliferationBeamVolleyDelayedFromSource(direction, damage, range,
                itemEffects.GetItemHealthEffectSource(PlayerItemIds.CellProliferationId, "beam"));

        private IEnumerator FireCellProliferationBeamVolleyDelayedFromSource(Vector3 direction, float damage, float range, HealthEffectSource source)
        {
            if (cellProliferationDelay > 0f)
            {
                yield return new WaitForSeconds(cellProliferationDelay);
            }

            if (!HasItemEffects || !isActiveAndEnabled)
            {
                yield break;
            }

            FireBeamVolleyFromSource(
                direction,
                damage * itemEffects.CellProliferationDamageMultiplier,
                range,
                false, source);
        }

        private void SpawnProjectile(Vector3 direction, float damage, float range, Projectile.SpawnKind spawnKind)
            => SpawnProjectileFromSource(direction, damage, range, spawnKind, playerController.GetHealthEffectSource("basic/ranged"));

        private void SpawnProjectileFromSource(Vector3 direction, float damage, float range, Projectile.SpawnKind spawnKind, HealthEffectSource source)
        {
            PlayerProjectilePool pooler = ResolveObjectPooler();
            if (pooler == null)
            {
                Debug.LogWarning("[PlayerAttack] PlayerProjectilePool.Instance is null");
                return;
            }

            GameObject projectile = pooler.GetPooledObject();
            if (projectile == null)
            {
                LogPoolUnavailableReason(pooler);
                return;
            }

            Vector3 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            Vector3 spawnOrigin = firePoint != null ? firePoint.position : transform.position;
            Vector3 spawnPos = spawnOrigin + forward * projectileSpawnOffset;
            spawnPos.y += projectileSpawnHeight + projectileSpawnExtraHeight;

            projectile.transform.position = spawnPos;
            projectile.SetActive(true);

            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj == null)
            {
                Debug.LogWarning("[PlayerAttack] Pooled projectile has no Projectile component.");
                return;
            }

            proj.LaunchWithItemContext(forward, damage, rangedTargetMask, range, itemEffects, spawnKind, source);
            ProjectileDirectionalSprite directionalSprite = projectile.GetComponent<ProjectileDirectionalSprite>();
            if (directionalSprite == null)
            {
                directionalSprite = projectile.AddComponent<ProjectileDirectionalSprite>();
            }

            directionalSprite.SetDirection(forward);
        }

        private void FireBeam(Vector3 direction, float damage, float range)
            => FireBeamFromSource(direction, damage, range, playerController.GetHealthEffectSource("basic/beam"));

        private void FireBeamFromSource(Vector3 direction, float damage, float range, HealthEffectSource source)
        {
            EnsureBeamOverlapBuffer();
            var beamHitEnemies=new HashSet<EnemyController>();

            Vector3 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            Vector3 origin = firePoint != null ? firePoint.position : transform.position;
            origin += forward * projectileSpawnOffset;
            origin.y += projectileSpawnHeight + projectileSpawnExtraHeight + beamHeightOffset;

            Vector3 end = origin + forward * Mathf.Max(0.5f, range);
            float radius = HasItemEffects ? itemEffects.BeamRadius : 0.8f;
            float halfHeight = Mathf.Max(0.05f, beamVerticalHalfHeight);
            SpawnBeamVisual(origin, end, radius);
            ResidueRubble.HitAlongSegment(origin, end, radius, damage, rangedTargetMask, ResidueRubble.NextAttackToken());

            var hits=EnemyHitSnapshot.Box((origin+end)*.5f,
                new Vector3(radius,halfHeight+radius,Vector3.Distance(origin,end)*.5f+radius),Quaternion.LookRotation(forward),
                rangedTargetMask,QueryTriggerInteraction.Collide,ref beamOverlapResults);
            var hitData=hits;
            int hitCount=hits.Length;

            for (int i = 0; i < hitCount; i++)
            {
                if(!hitData[i].IsCurrent)continue;
                Collider collider = hitData[i].Collider;
                if (collider == null)
                {
                    continue;
                }

                EnemyController enemy = hitData[i].Enemy;

                if (enemy == null || enemy.IsDead || !EnemyReceivingShape.IntersectsBeam(enemy,collider,origin,end,radius,halfHeight) || !beamHitEnemies.Add(enemy))
                {
                    continue;
                }

                float appliedDamage = HasItemEffects
                    ? itemEffects.ApplyPerTargetDamageModifiers(enemy, damage)
                    : damage;
                var hitLife = new EnemyHitLife(enemy);
                Vector3 hitPosition = enemy.transform.position;
                enemy.TakeDamage(appliedDamage, source);
                itemEffects?.TryApplyPostDamageExecutionInstinct(enemy, appliedDamage, hitLife.Generation);
                itemEffects?.ApplyCommonOnHitEffects(enemy, appliedDamage, hitPosition, hitLife.Generation);
            }
        }

        private void SpawnBeamVisual(Vector3 start, Vector3 end, float radius)
        {
            GameObject fx = RuntimePool.Acquire(BeamVisualPoolName, CreateBeamVisualFunc);
            if (fx == null || !fx.TryGetComponent(out PlayerBeamVisual visual))
            {
                RuntimePool.Release(fx);
                return;
            }

            visual.Show(
                start,
                end,
                radius,
                Mathf.Max(0.05f, beamVisualDuration),
                GetBeamMaterial());
        }

        private static GameObject CreateBeamVisualObject()
        {
            GameObject fx = new GameObject("BeamOrganFx");
            LineRenderer line = fx.AddComponent<LineRenderer>();
            PlayerBeamVisual visual = fx.AddComponent<PlayerBeamVisual>();
            visual.Initialize(line);
            RuntimePool.EnsureAutoReturn(fx);
            return fx;
        }

        private Material GetBeamMaterial()
        {
            if (beamMaterial != null)
            {
                return beamMaterial;
            }

            Material baseMaterial = TextureSpriteCache.GetSpriteMaterial();
            if (baseMaterial == null)
            {
                return null;
            }

            if (!beamTextureLoadAttempted)
            {
                beamTextureLoadAttempted = true;
                if (!string.IsNullOrWhiteSpace(beamTextureResourcePath))
                {
                    Sprite beamSprite = TextureSpriteCache.LoadResourceSprite(beamTextureResourcePath);
                    beamTexture = beamSprite != null
                        ? beamSprite.texture
                        : Resources.Load<Texture2D>(beamTextureResourcePath);
                }

                if (beamTexture == null)
                {
                    Debug.LogWarning($"[PlayerAttack] Resources/{beamTextureResourcePath} beam texture not found. Using the fallback line material.");
                }
            }

            if (beamTexture == null)
            {
                return baseMaterial;
            }

            beamTexture.wrapMode = TextureWrapMode.Clamp;
            beamTexture.filterMode = FilterMode.Point;
            beamMaterial = new Material(baseMaterial)
            {
                name = "BeamOrganMaterial",
                mainTexture = beamTexture
            };
            return beamMaterial;
        }

        private static PlayerProjectilePool ResolveObjectPooler()
        {
            PlayerProjectilePool pooler = PlayerProjectilePool.Instance;
            if (pooler != null)
            {
                return pooler;
            }

            pooler = FindFirstObjectByType<PlayerProjectilePool>();
            if (pooler != null)
            {
                PlayerProjectilePool.Instance = pooler;
            }

            return pooler;
        }

        private static void LogPoolUnavailableReason(PlayerProjectilePool pooler)
        {
            if (pooler == null)
            {
                Debug.LogWarning("[PlayerAttack] PlayerProjectilePool.Instance is null");
                return;
            }

            string status = pooler.GetDebugStatus();
            Debug.LogWarning($"[PlayerAttack] {status}");
        }

        private void ResolveRootFirePoint()
        {
            if (IsValidRootSpawnPoint(firePoint))
            {
                return;
            }

            if (IsValidRootSpawnPoint(projectileSpawnPoint))
            {
                firePoint = projectileSpawnPoint;
                return;
            }

            Transform found = transform.Find("FirePoint");
            if (IsValidRootSpawnPoint(found))
            {
                firePoint = found;
                return;
            }

            GameObject pointObject = new GameObject("FirePoint");
            firePoint = pointObject.transform;
            firePoint.SetParent(transform, false);
            firePoint.localPosition = Vector3.zero;
            firePoint.localRotation = Quaternion.identity;
        }

        private bool IsValidRootSpawnPoint(Transform point)
        {
            if (point == null || !point.IsChildOf(transform))
            {
                return false;
            }

            Transform cursor = point;
            while (cursor != null && cursor != transform)
            {
                if (cursor.GetComponent<SpriteRenderer>() != null)
                {
                    return false;
                }

                cursor = cursor.parent;
            }

            return true;
        }

        private void OnDestroy()
        {
            if (beamMaterial != null)
            {
                Destroy(beamMaterial);
                beamMaterial = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 direction = GetAttackDirection();
            PlayerStats stats = PlayerStats.Instance;
            float rangeMultiplier = HasItemEffects ? itemEffects.GetMeleeRangeMultiplier() : 1f;
            float gizmoOffset = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackOffset * rangeMultiplier, stats);
            float gizmoWidth = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackBoxSize.x * rangeMultiplier, stats);
            float gizmoDepth = PlayerCombatCalculator.GetBasicAttackRange(meleeAttackBoxSize.z * rangeMultiplier, stats);

            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(
                transform.position + direction * gizmoOffset,
                Quaternion.LookRotation(direction),
                Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(gizmoWidth, 20f, gizmoDepth));
        }
    }

    [DisallowMultipleComponent]
    internal sealed class PlayerBeamVisual : MonoBehaviour
    {
        private LineRenderer line;
        private RuntimePoolAutoReturn autoReturn;

        public void Initialize(LineRenderer targetLine)
        {
            line = targetLine;
            if (line == null)
            {
                line = GetComponent<LineRenderer>();
            }

            if (line == null)
            {
                return;
            }

            line.positionCount = 2;
            line.useWorldSpace = true;
            line.numCapVertices = 6;
            line.sharedMaterial = TextureSpriteCache.GetSpriteMaterial();
            line.sortingOrder = 5005;

            Color baseColor = new Color(1f, 0.24f, 0.15f, 0.85f);
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(baseColor, 0f),
                    new GradientColorKey(new Color(1f, 0.7f, 0.2f, 1f), 0.55f),
                    new GradientColorKey(baseColor, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.95f, 0f),
                    new GradientAlphaKey(0.65f, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                });
            line.colorGradient = gradient;
        }

        public void Show(Vector3 start, Vector3 end, float radius, float duration, Material material)
        {
            if (line == null)
            {
                Initialize(GetComponent<LineRenderer>());
            }

            if (line == null)
            {
                RuntimePool.Release(gameObject);
                return;
            }

            if (material != null)
            {
                line.sharedMaterial = material;
            }

            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startWidth = Mathf.Max(0.05f, radius * 1.45f);
            line.endWidth = Mathf.Max(0.03f, radius * 1.1f);
            line.enabled = true;

            if (autoReturn == null)
            {
                autoReturn = RuntimePool.EnsureAutoReturn(gameObject);
            }
            autoReturn.Schedule(duration);
        }
    }
}
