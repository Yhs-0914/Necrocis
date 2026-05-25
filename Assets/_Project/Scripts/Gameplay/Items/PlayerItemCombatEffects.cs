using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerItemManager))]
    public class PlayerItemCombatEffects : MonoBehaviour
    {
        public const string DoubleCoreId = "double_core";
        public const string TripleCoreId = "triple_core";
        public const string HomingCellId = "homing_cell";
        public const string HypertrophyCellId = "hypertrophy_cell";
        public const string RefluxOrganId = "reflux_organ";
        public const string PiercingMucusId = "piercing_mucus";
        public const string LaryngealNerveId = "laryngeal_nerve";
        public const string BeamOrganId = "beam_organ";
        public const string SplitTissueId = "split_tissue";
        public const string ExplosiveBloodCellId = "explosive_blood_cell";
        public const string AcidicRuptureId = "acidic_rupture";
        public const string CellProliferationId = "cell_proliferation";
        public const string PulseBulletId = "pulse_bullet";
        public const string VascularReflectionId = "vascular_reflection";
        public const string ToxicMucosaId = "toxic_mucosa";
        public const string FreezingNerveId = "freezing_nerve";
        public const string HemorrhageOrganId = "hemorrhage_organ";

        [Header("Multi Shot")]
        [SerializeField] private float doubleShotSpreadAngle = 7f;
        [SerializeField] private float tripleShotSpreadAngle = 11f;
        [SerializeField] private float backShotDamageMultiplier = 0.9f;

        [Header("Projectile Movement")]
        [SerializeField] private float homingTurnRate = 7f;
        [SerializeField] private float homingSearchRadius = 8f;
        [SerializeField] private float boomerangReturnDistance = 5.5f;
        [SerializeField, Min(1)] private int piercingHitCount = 4;
        [SerializeField, Min(1)] private int reflectionBounceCount = 3;

        [Header("Projectile Scale")]
        [SerializeField] private float hypertrophyScaleMultiplier = 1.45f;
        [SerializeField] private float hypertrophyRangeMultiplier = 1.25f;
        [SerializeField] private float pulseAmplitude = 0.22f;
        [SerializeField] private float pulseFrequency = 0.3f;

        [Header("On-Hit")]
        [SerializeField] private float splitDamageMultiplier = 0.7f;
        [SerializeField] private float splitRangeMultiplier = 0.65f;
        [SerializeField] private float splitAngle = 28f;
        [SerializeField] private float explosionRadius = 2f;
        [SerializeField] private float explosionDamageMultiplier = 0.75f;
        [SerializeField] private float acidDuration = 4f;
        [SerializeField] private float acidTickInterval = 0.5f;
        [SerializeField] private float acidTickDamageRatio = 0.18f;
        [SerializeField] private float poisonDuration = 4f;
        [SerializeField] private float poisonTickInterval = 1f;
        [SerializeField] private float poisonTickDamageRatio = 0.2f;
        [SerializeField] private float freezeSlowRatio = 0.35f;
        [SerializeField] private float freezeDuration = 2.5f;
        [SerializeField] private float bleedDuration = 3.5f;
        [SerializeField] private float bleedTickInterval = 0.8f;
        [SerializeField] private float bleedTickDamageRatio = 0.18f;

        [Header("Trigger Effects")]
        [SerializeField, Range(0f, 1f)] private float cellProliferationChance = 0.25f;
        [SerializeField] private float cellProliferationDamageMultiplier = 0.9f;

        [Header("Beam")]
        [SerializeField] private float beamRadius = 0.8f;
        [SerializeField] private float beamDamageMultiplier = 0.95f;
        [SerializeField, Min(1)] private int beamHitBufferSize = 48;


        private PlayerItemManager itemManager;

        public bool HasHomingCell => HasItem(HomingCellId);
        public bool HasRefluxOrgan => HasItem(RefluxOrganId);
        public bool HasPiercingMucus => HasItem(PiercingMucusId);
        public bool HasLaryngealNerve => HasItem(LaryngealNerveId);
        public bool HasBeamOrgan => HasItem(BeamOrganId);
        public bool HasSplitTissue => HasItem(SplitTissueId);
        public bool HasExplosiveBloodCell => HasItem(ExplosiveBloodCellId);
        public bool HasAcidicRupture => HasItem(AcidicRuptureId);
        public bool HasPulseBullet => HasItem(PulseBulletId);
        public bool HasVascularReflection => HasItem(VascularReflectionId);
        public bool HasToxicMucosa => HasItem(ToxicMucosaId);
        public bool HasFreezingNerve => HasItem(FreezingNerveId);
        public bool HasHemorrhageOrgan => HasItem(HemorrhageOrganId);

        public int BeamHitBufferSize => Mathf.Max(1, beamHitBufferSize);
        public float BeamRadius => Mathf.Max(0.05f, beamRadius);
        public float BeamDamageMultiplier => Mathf.Max(0.05f, beamDamageMultiplier);
        public float CellProliferationDamageMultiplier => Mathf.Max(0.05f, cellProliferationDamageMultiplier);

        private void Awake()
        {
            itemManager = GetComponent<PlayerItemManager>();
        }

        public int GetForwardProjectileCount()
        {
            if (HasItem(TripleCoreId))
            {
                return 3;
            }

            if (HasItem(DoubleCoreId))
            {
                return 2;
            }

            return 1;
        }

        public float GetSpreadAngleForCount(int projectileCount)
        {
            if (projectileCount >= 3)
            {
                return Mathf.Max(0f, tripleShotSpreadAngle);
            }

            if (projectileCount == 2)
            {
                return Mathf.Max(0f, doubleShotSpreadAngle);
            }

            return 0f;
        }

        public float GetBackShotDamageMultiplier()
        {
            return Mathf.Max(0.05f, backShotDamageMultiplier);
        }

        public bool RollCellProliferation()
        {
            return HasItem(CellProliferationId) && Random.value <= cellProliferationChance;
        }

        public float GetRangeMultiplier()
        {
            return HasItem(HypertrophyCellId) ? Mathf.Max(0.1f, hypertrophyRangeMultiplier) : 1f;
        }

        public float GetScaleMultiplier()
        {
            return HasItem(HypertrophyCellId) ? Mathf.Max(0.1f, hypertrophyScaleMultiplier) : 1f;
        }

        public int GetPiercingHitCount()
        {
            return HasPiercingMucus ? Mathf.Max(1, piercingHitCount) : 1;
        }

        public int GetReflectionBounceCount()
        {
            return HasVascularReflection ? Mathf.Max(1, reflectionBounceCount) : 0;
        }

        public float GetBoomerangReturnDistance()
        {
            return Mathf.Max(0.5f, boomerangReturnDistance);
        }

        public float GetHomingTurnRate()
        {
            return Mathf.Max(0f, homingTurnRate);
        }

        public float GetHomingSearchRadius()
        {
            return Mathf.Max(0.5f, homingSearchRadius);
        }

        public float GetPulseAmplitude()
        {
            return Mathf.Max(0f, pulseAmplitude);
        }

        public float GetPulseFrequency()
        {
            return Mathf.Max(0.01f, pulseFrequency);
        }

        public float GetSplitAngle()
        {
            return Mathf.Max(0f, splitAngle);
        }

        public float GetSplitDamageMultiplier()
        {
            return Mathf.Max(0.05f, splitDamageMultiplier);
        }

        public float GetSplitRangeMultiplier()
        {
            return Mathf.Max(0.05f, splitRangeMultiplier);
        }

        public float GetExplosionRadius()
        {
            return Mathf.Max(0.2f, explosionRadius);
        }

        public float GetExplosionDamageMultiplier()
        {
            return Mathf.Max(0.05f, explosionDamageMultiplier);
        }

        public bool HasItem(string itemId)
        {
            if (itemManager == null)
            {
                itemManager = GetComponent<PlayerItemManager>();
            }

            return itemManager != null && itemManager.ContainsItem(itemId);
        }

        public void ApplyCommonOnHitEffects(
            EnemyController enemy,
            float hitDamage,
            Vector3 hitPosition)
        {
            if (enemy == null || enemy.IsDead)
            {
                return;
            }

            EnemyStatusEffectController status = EnsureStatusController(enemy);

            if (HasToxicMucosa)
            {
                float tickDamage = Mathf.Max(1f, hitDamage * poisonTickDamageRatio);
                status?.ApplyPoison(poisonDuration, poisonTickInterval, tickDamage);
            }

            if (HasFreezingNerve)
            {
                status?.ApplyMoveSpeedSlow(freezeSlowRatio, freezeDuration);
            }

            if (HasHemorrhageOrgan)
            {
                float tickDamage = Mathf.Max(1f, hitDamage * bleedTickDamageRatio);
                status?.ApplyBleed(bleedDuration, bleedTickInterval, tickDamage);
            }

            if (HasAcidicRupture)
            {
                float tickDamage = Mathf.Max(1f, hitDamage * acidTickDamageRatio);
                AcidPuddle.Spawn(hitPosition, tickDamage, acidDuration, GetExplosionRadius(), acidTickInterval);
            }
        }

        public void SpawnSplitProjectiles(Vector3 origin, Vector3 forwardDirection, float damage, LayerMask mask, float range)
        {
            Vector3 forward = forwardDirection.sqrMagnitude > 0.0001f ? forwardDirection.normalized : Vector3.forward;
            float angle = GetSplitAngle();
            float sideDamage = damage * GetSplitDamageMultiplier();
            float sideRange = range * GetSplitRangeMultiplier();

            SpawnChildProjectile(origin, Quaternion.Euler(0f, angle, 0f) * forward, sideDamage, mask, sideRange, Projectile.SpawnKind.SplitChild);
            SpawnChildProjectile(origin, Quaternion.Euler(0f, -angle, 0f) * forward, sideDamage, mask, sideRange, Projectile.SpawnKind.SplitChild);
        }

        private void SpawnChildProjectile(Vector3 origin, Vector3 direction, float damage, LayerMask mask, float range, Projectile.SpawnKind spawnKind)
        {
            PlayerProjectilePool pool = ResolvePool();
            if (pool == null)
            {
                return;
            }

            GameObject projectileObject = pool.GetPooledObject();
            if (projectileObject == null)
            {
                return;
            }

            projectileObject.transform.position = origin;
            projectileObject.SetActive(true);

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            if (projectile == null)
            {
                return;
            }

            projectile.Launch(direction, damage, mask, range, this, spawnKind);
        }

        private static PlayerProjectilePool ResolvePool()
        {
            PlayerProjectilePool pool = PlayerProjectilePool.Instance;
            if (pool != null)
            {
                return pool;
            }

            pool = FindFirstObjectByType<PlayerProjectilePool>();
            if (pool != null)
            {
                PlayerProjectilePool.Instance = pool;
            }

            return pool;
        }

        private static EnemyStatusEffectController EnsureStatusController(EnemyController enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            EnemyStatusEffectController status = enemy.StatusEffects;
            if (status == null)
            {
                status = enemy.GetComponent<EnemyStatusEffectController>();
            }

            if (status == null)
            {
                status = enemy.gameObject.AddComponent<EnemyStatusEffectController>();
            }

            status.Initialize(enemy);
            return status;
        }
    }
}
