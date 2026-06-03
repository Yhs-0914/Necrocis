using System.Collections.Generic;
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
        public const string OverheatedOrganId = "overheated_organ";
        public const string MutantEyeId = "mutant_eye";
        public const string OrganTentacleId = "organ_tentacle";
        public const string RampageBloodFlowId = "rampage_bloodflow";
        public const string MuscleSpasmId = "muscle_spasm";
        public const string UnstableCoreId = "unstable_core";
        public const string BioResonanceId = "bio_resonance";
        public const string BloodPressureBurstId = "blood_pressure_burst";
        public const string VoidCellId = "void_cell";
        public const string ForbiddenGrowthId = "forbidden_growth";
        public const string OverclockNerveId = "overclock_nerve";
        public const string BloodContractId = "blood_contract";
        public const string HyperplasiaHeartId = "hyperplasia_heart";
        public const string DecayOrganId = "decay_organ";
        public const string RuptureMuscleId = "rupture_muscle";
        public const string ImperfectRegenerationId = "imperfect_regeneration";
        public const string SeveranceReflexId = "severance_reflex";
        public const string BioGambleId = "bio_gamble";
        public const string ExoskeletonId = "exoskeleton";
        public const string PlateletMembraneId = "platelet_membrane";
        public const string RecoveryFactorId = "recovery_factor";
        public const string ReflectiveSkinId = "reflective_skin";
        public const string BioBarrierId = "bio_barrier";
        public const string SplitRegenerationId = "split_regeneration";

        [Header("Multi Shot")]
        [SerializeField] private float doubleShotSpreadAngle = 7f;
        [SerializeField] private float tripleShotSpreadAngle = 11f;
        [SerializeField] private float backShotDamageMultiplier = 0.9f;

        [Header("Projectile Movement")]
        [SerializeField] private float homingTurnRate = 7f;
        [SerializeField] private float homingSearchRadius = 8f;
        [SerializeField] private float boomerangReturnDistance = 8f;
        [SerializeField] private float boomerangRepeatHitDamageMultiplier = 0.75f;
        [SerializeField, Min(1)] private int piercingHitCount = 4;
        [SerializeField, Min(1)] private int reflectionBounceCount = 3;

        [Header("Projectile Scale")]
        [SerializeField] private float hypertrophyScaleMultiplier = 1.45f;
        [SerializeField] private float hypertrophyRangeMultiplier = 1.25f;
        [SerializeField] private float pulseGrowEndScale = 2.6f;
        [SerializeField] private float pulseShrinkEndScale = 0.2f;

        [Header("On-Hit")]
        [SerializeField] private float splitDamageMultiplier = 0.7f;
        [SerializeField] private float splitRangeMultiplier = 0.65f;
        [SerializeField] private float splitAngle = 90f;
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

        [Header("Special Items")]
        [SerializeField] private int overheatMaxStacks = 10;
        [SerializeField] private float overheatStackWindow = 1.4f;
        [SerializeField] private float overheatPerStackAttackSpeedBonus = 2f;
        [SerializeField] private float overheatDecayInterval = 5f;
        [SerializeField] private float overheatExplosionSelfDamage = 2f;
        [SerializeField] private float mutantEyeAccuracyPenaltyAngle = 90f;
        [SerializeField] private float mutantEyeFlatDamageBonus = 5f;
        [SerializeField] private float organTentacleAutoAttackInterval = 0.75f;
        [SerializeField] private float organTentacleAutoAttackRadius = 4.5f;
        [SerializeField] private float organTentacleAutoAttackDamageMultiplier = 0.55f;
        [SerializeField] private int organTentacleMaxTargets = 3;
        [SerializeField] private float organTentacleBaseAttackPenaltyMultiplier = 0.72f;
        [SerializeField] private float rampageMoveSecondsPerBonus = 5f;
        [SerializeField] private int rampageMaxAttackBonus = 3;
        [SerializeField] private float rampageIdleSecondsPerDecay = 5f;
        [SerializeField] private float muscleSpasmMeleeRangeMultiplier = 8f;
        [SerializeField] private float muscleSpasmAttackSpeedMultiplier = 0.72f;
        [SerializeField] private float unstableCoreMinAttackRatio = 0.5f;
        [SerializeField] private float unstableCoreMaxAttackFlatBonus = 3f;
        [SerializeField] private float unstableCoreRerollInterval = 5f;
        [SerializeField] private float bioResonanceStackDamageBonus = 0.3f;
        [SerializeField] private int bioResonanceMaxStacks = 3;
        [SerializeField] private float bioResonanceStackWindow = 3f;
        [SerializeField] private float bloodPressureBaseAttackSpeedAdd = 0.2f;
        [SerializeField] private float bloodPressurePerTenPercentMissingAdd = 0.2f;
        [SerializeField] private float voidCellChance = 0.2f;
        [SerializeField] private float voidCellDamageMultiplier = 0.85f;
        [SerializeField] private float voidCellSpawnRadius = 2.2f;
        [SerializeField] private float forbiddenGrowthMaxHealthPenalty = 4f;
        [SerializeField] private float forbiddenGrowthFlatAttackBonus = 6f;
        [SerializeField] private float overclockNerveMaxHealthPenalty = 2f;
        [SerializeField] private float overclockNerveFlatMoveSpeedBonus = 4f;
        [SerializeField] private float bloodContractIncomingDamageMultiplier = 1.5f;
        [SerializeField, Min(1)] private int bloodContractKillsPerHeal = 10;
        [SerializeField] private float bloodContractHealthGainAmount = 1f;
        [SerializeField] private float hyperplasiaMissingHealthAttackBonusMax = 6f;
        [SerializeField] private float decayOrganSecondsPerAttackBonus = 180f;
        [SerializeField] private int decayOrganMaxAttackBonus = 8;
        [SerializeField] private float ruptureMuscleAttackBonusPerStack = 2f;
        [SerializeField] private float ruptureMuscleMovePenaltyPerStack = 1f;
        [SerializeField] private int ruptureMuscleMaxStacks = 5;
        [SerializeField] private float ruptureMuscleDecayDelay = 3f;
        [SerializeField] private float imperfectRegenMaxHealthPenalty = 4f;
        [SerializeField] private float imperfectRegenDelay = 3f;
        [SerializeField] private float imperfectRegenHealPerTrigger = 1f;
        [SerializeField] private float imperfectRegenCooldownDuration = 15f;
        [SerializeField] private float severanceReflexDuration = 2f;
        [SerializeField] private float severanceReflexFlatAttackBonus = 6f;
        [SerializeField] private float exoskeletonDamageReductionRatio = 0.3f;
        [SerializeField] private float exoskeletonMoveSpeedPenalty = 2f;
        [SerializeField] private float plateletMembraneInterval = 30f;
        [SerializeField] private float plateletMembraneShieldAmount = 1f;
        [SerializeField] private float recoveryFactorInterval = 45f;
        [SerializeField] private float recoveryFactorHealAmount = 1f;
        [SerializeField] private float reflectiveSkinDamageRatio = 0.5f;
        [SerializeField] private float bioBarrierIdleSecondsPerStep = 1f;
        [SerializeField] private float bioBarrierReductionPerStep = 0.1f;
        [SerializeField] private float bioBarrierMaxReduction = 0.5f;
        [SerializeField] private float splitRegenerationReviveHealth = 2f;


        private PlayerItemManager itemManager;
        private PlayerController playerController;
        private PlayerStats playerStats;
        private Vector3 previousPosition;
        private float movementIntensity;
        private float tentacleNextAutoAttackTime;
        private float overheatLastAttackTime = float.NegativeInfinity;
        private float overheatNextDecayTime = float.PositiveInfinity;
        private int overheatStacks;
        private readonly Dictionary<int, ResonanceState> resonanceStatesByEnemyId = new Dictionary<int, ResonanceState>();
        private readonly List<int> tempResonanceRemovalIds = new List<int>();
        private float unstableCoreNextRerollTime;
        private float unstableCoreCurrentMultiplier = 1f;
        private bool unstableCoreInitialized;
        private SpriteRenderer unstableCoreOverlay;
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineSizeId = Shader.PropertyToID("_OutlineSize");
        private static readonly int OutlineExpandId = Shader.PropertyToID("_OutlineExpand");
        private Material plateletMembraneOutlineMaterial;
        private SpriteRenderer plateletMembraneOutlineTarget;
        private SpriteRenderer plateletMembraneOutlineRenderer;
        private bool plateletMembraneShaderWarningLogged;
        private SpriteRenderer playerVisualSpriteRenderer;
        private float rampageMoveAccumulatedTime;
        private float rampageIdleAccumulatedTime;
        private int rampageAttackBonus;
        private bool isMovingByPosition;
        private bool subscribedHealthEvents;
        private bool forbiddenGrowthModifierApplied;
        private bool overclockNerveModifierApplied;
        private bool imperfectRegenModifierApplied;
        private int bloodContractKillProgress;
        private float decayOrganStartTime = float.NegativeInfinity;
        private int decayOrganAttackBonus;
        private float ruptureMuscleStacks;
        private float ruptureMuscleLastAttackTime = float.NegativeInfinity;
        private float severanceReflexBuffUntil = float.NegativeInfinity;
        private float imperfectRegenPendingHeal;
        private float imperfectRegenHealReadyTime = float.PositiveInfinity;
        private float imperfectRegenCooldownUntil = float.NegativeInfinity;
        private float ruptureAppliedMovePenalty;
        private bool exoskeletonModifierApplied;
        private float plateletMembraneCurrentShield;
        private float plateletMembraneNextReadyTime;
        private float recoveryFactorNextHealTime;
        private float bioBarrierIdleTime;
        private bool splitRegenerationUsed;
        private readonly object forbiddenGrowthModifierSource = new object();
        private readonly object overclockNerveModifierSource = new object();
        private readonly object imperfectRegenModifierSource = new object();
        private readonly object ruptureMovePenaltyModifierSource = new object();
        private readonly object exoskeletonModifierSource = new object();

        private struct ResonanceState
        {
            public int Stacks;
            public float LastHitTime;
        }

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
        public bool HasOverheatedOrgan => HasItem(OverheatedOrganId);
        public bool HasMutantEye => HasItem(MutantEyeId);
        public bool HasOrganTentacle => HasItem(OrganTentacleId);
        public bool HasRampageBloodFlow => HasItem(RampageBloodFlowId);
        public bool HasMuscleSpasm => HasItem(MuscleSpasmId);
        public bool HasUnstableCore => HasItem(UnstableCoreId);
        public bool HasBioResonance => HasItem(BioResonanceId);
        public bool HasBloodPressureBurst => HasItem(BloodPressureBurstId);
        public bool HasVoidCell => HasItem(VoidCellId);
        public bool HasForbiddenGrowth => HasItem(ForbiddenGrowthId);
        public bool HasOverclockNerve => HasItem(OverclockNerveId);
        public bool HasBloodContract => HasItem(BloodContractId);
        public bool HasHyperplasiaHeart => HasItem(HyperplasiaHeartId);
        public bool HasDecayOrgan => HasItem(DecayOrganId);
        public bool HasRuptureMuscle => HasItem(RuptureMuscleId);
        public bool HasImperfectRegeneration => HasItem(ImperfectRegenerationId);
        public bool HasSeveranceReflex => HasItem(SeveranceReflexId);
        public bool HasBioGamble => HasItem(BioGambleId);
        public bool HasExoskeleton => HasItem(ExoskeletonId);
        public bool HasPlateletMembrane => HasItem(PlateletMembraneId);
        public bool HasRecoveryFactor => HasItem(RecoveryFactorId);
        public bool HasReflectiveSkin => HasItem(ReflectiveSkinId);
        public bool HasBioBarrier => HasItem(BioBarrierId);
        public bool HasSplitRegeneration => HasItem(SplitRegenerationId);

        public int BeamHitBufferSize => Mathf.Max(1, beamHitBufferSize);
        public float BeamRadius => Mathf.Max(0.05f, beamRadius);
        public float BeamDamageMultiplier => Mathf.Max(0.05f, beamDamageMultiplier);
        public float CellProliferationDamageMultiplier => Mathf.Max(0.05f, cellProliferationDamageMultiplier);

        private void Awake()
        {
            itemManager = GetComponent<PlayerItemManager>();
            playerController = GetComponent<PlayerController>();
            playerStats = GetComponent<PlayerStats>();
            previousPosition = GetMovementAnchorPosition();
        }

        private void OnEnable()
        {
            TrySubscribeHealthEvents();
        }

        private void OnDisable()
        {
            TryUnsubscribeHealthEvents();
            ClearPlateletMembraneOutline();
            ClearPersistentStatModifiers();
            resonanceStatesByEnemyId.Clear();
        }

        private void Update()
        {
            SyncPersistentStatModifiers();
            UpdateDefensiveStates();
            if (!HasBloodContract)
            {
                bloodContractKillProgress = 0;
            }

            if (!HasSeveranceReflex)
            {
                severanceReflexBuffUntil = float.NegativeInfinity;
            }

            UpdateMovementIntensity();
            UpdateRampageBloodFlowState();
            UpdateDecayOrganState();
            UpdateRuptureMuscleState();
            UpdateImperfectRegenState();
            UpdateTentacleAutoAttack();
            UpdateDecayStates();
            UpdatePlateletMembraneOutline();
            UpdateUnstableCoreOverlay();
        }

        private void UpdateDefensiveStates()
        {
            if (!HasPlateletMembrane)
            {
                plateletMembraneCurrentShield = 0f;
                plateletMembraneNextReadyTime = Time.time + Mathf.Max(0.1f, plateletMembraneInterval);
            }
            else
            {
                if (plateletMembraneCurrentShield <= 0f && Time.time >= plateletMembraneNextReadyTime)
                {
                    plateletMembraneCurrentShield = Mathf.Max(0f, plateletMembraneShieldAmount);
                }
            }

            if (!HasRecoveryFactor || playerStats == null || playerStats.IsDead)
            {
                recoveryFactorNextHealTime = Time.time + Mathf.Max(0.1f, recoveryFactorInterval);
            }
            else if (Time.time >= recoveryFactorNextHealTime)
            {
                playerStats.Heal(Mathf.Max(0f, recoveryFactorHealAmount));
                recoveryFactorNextHealTime = Time.time + Mathf.Max(0.1f, recoveryFactorInterval);
            }

            bool isMovingNow = (playerController != null && playerController.IsMoving) || isMovingByPosition || movementIntensity > 0.02f;
            if (!HasBioBarrier || isMovingNow)
            {
                bioBarrierIdleTime = 0f;
            }
            else
            {
                bioBarrierIdleTime += Time.deltaTime;
            }

            if (!HasSplitRegeneration)
            {
                splitRegenerationUsed = false;
            }

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

        public float GetBoomerangReturnDistance(float minimumReturnDistance)
        {
            return Mathf.Max(minimumReturnDistance, GetBoomerangReturnDistance());
        }

        public float GetBoomerangRepeatHitDamageMultiplier()
        {
            return Mathf.Clamp(boomerangRepeatHitDamageMultiplier, 0.05f, 1f);
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
            return Mathf.Max(1.2f, pulseGrowEndScale);
        }

        public float GetPulseFrequency()
        {
            return Mathf.Clamp(pulseShrinkEndScale, 0.08f, 0.9f);
        }

        public float GetSplitAngle()
        {
            return Mathf.Max(90f, splitAngle);
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

        public float GetAttackCooldownMultiplier()
        {
            float speedMultiplier = 1f;

            if (HasOverheatedOrgan && overheatStacks > 0)
            {
                float bonus = 1f + Mathf.Max(0f, overheatPerStackAttackSpeedBonus) * overheatStacks;
                speedMultiplier *= bonus;
            }

            if (HasBloodPressureBurst && playerStats != null && playerStats.MaxHealth > 0f)
            {
                float healthRatio = Mathf.Clamp01(playerStats.CurrentHealth / playerStats.MaxHealth);
                int missingTenPercentSteps = Mathf.Clamp(Mathf.FloorToInt((1f - healthRatio) * 10f + 0.0001f), 0, 10);
                float additiveAttackSpeed =
                    Mathf.Max(0f, bloodPressureBaseAttackSpeedAdd)
                    + Mathf.Max(0f, bloodPressurePerTenPercentMissingAdd) * missingTenPercentSteps;
                float burstMultiplier = 1f + additiveAttackSpeed;
                speedMultiplier *= burstMultiplier;
            }

            if (HasMuscleSpasm)
            {
                speedMultiplier *= Mathf.Clamp(muscleSpasmAttackSpeedMultiplier, 0.25f, 1f);
            }

            return 1f / Mathf.Max(0.05f, speedMultiplier);
        }

        public float GetOutgoingBasicDamageMultiplier()
        {
            float multiplier = 1f;

            if (HasOrganTentacle)
            {
                multiplier *= Mathf.Clamp(organTentacleBaseAttackPenaltyMultiplier, 0.2f, 1f);
            }

            return Mathf.Max(0.05f, multiplier);
        }

        public float GetOutgoingBasicDamageFlatBonus()
        {
            float bonus = 0f;
            if (HasMutantEye)
            {
                bonus += Mathf.Max(0f, mutantEyeFlatDamageBonus);
            }

            if (HasRampageBloodFlow)
            {
                bonus += Mathf.Max(0, rampageAttackBonus);
            }

            if (HasHyperplasiaHeart && playerStats != null && playerStats.MaxHealth > 0f)
            {
                float healthRatio = Mathf.Clamp01(playerStats.CurrentHealth / playerStats.MaxHealth);
                bonus += (1f - healthRatio) * Mathf.Max(0f, hyperplasiaMissingHealthAttackBonusMax);
            }

            if (HasDecayOrgan)
            {
                bonus += Mathf.Max(0, decayOrganAttackBonus);
            }

            if (HasRuptureMuscle && ruptureMuscleStacks > 0)
            {
                int effectiveRuptureStacks = Mathf.FloorToInt(Mathf.Max(0f, ruptureMuscleStacks));
                bonus += Mathf.Max(0f, ruptureMuscleAttackBonusPerStack) * effectiveRuptureStacks;
            }

            if (HasSeveranceReflex && Time.time <= severanceReflexBuffUntil)
            {
                bonus += Mathf.Max(0f, severanceReflexFlatAttackBonus);
            }

            return bonus;
        }

        public float GetMeleeRangeMultiplier()
        {
            return HasMuscleSpasm ? Mathf.Max(1f, muscleSpasmMeleeRangeMultiplier) : 1f;
        }

        public float GetAccuracyPenaltyAngle()
        {
            return HasMutantEye ? Mathf.Max(0f, mutantEyeAccuracyPenaltyAngle) : 0f;
        }

        public float RollUnstableCoreDamageMultiplier()
        {
            if (!HasUnstableCore)
            {
                unstableCoreCurrentMultiplier = 1f;
                unstableCoreInitialized = false;
                return 1f;
            }

            if (!unstableCoreInitialized || Time.time >= unstableCoreNextRerollTime)
            {
                float attackPower = playerStats != null ? Mathf.Max(0.01f, playerStats.AttackPower) : 1f;
                float minDamage = attackPower * Mathf.Max(0f, unstableCoreMinAttackRatio);
                float maxDamage = attackPower + Mathf.Max(0f, unstableCoreMaxAttackFlatBonus);
                maxDamage = Mathf.Max(minDamage, maxDamage);
                float rolledDamage = Random.Range(minDamage, maxDamage);
                unstableCoreCurrentMultiplier = rolledDamage / attackPower;
                unstableCoreNextRerollTime = Time.time + Mathf.Max(0.1f, unstableCoreRerollInterval);
                unstableCoreInitialized = true;
            }

            return unstableCoreCurrentMultiplier;
        }

        public void NotifyBasicAttackPerformed(float attackDamage, LayerMask mask, float range, Vector3 forwardDirection)
        {
            float now = Time.time;

            if (HasOverheatedOrgan)
            {
                if (now - overheatLastAttackTime <= Mathf.Max(0.1f, overheatStackWindow))
                {
                    overheatStacks = Mathf.Clamp(overheatStacks + 1, 1, Mathf.Max(1, overheatMaxStacks));
                }
                else
                {
                    overheatStacks = 1;
                }

                overheatLastAttackTime = now;
                overheatNextDecayTime = now + Mathf.Max(0.25f, overheatDecayInterval);

                if (overheatStacks >= Mathf.Max(1, overheatMaxStacks))
                {
                    TriggerOverheatExplosionSelfDamage(attackDamage);
                    overheatStacks = 0;
                    overheatLastAttackTime = float.NegativeInfinity;
                    overheatNextDecayTime = float.PositiveInfinity;
                }
            }

            if (HasVoidCell && Random.value <= Mathf.Clamp01(voidCellChance))
            {
                SpawnVoidCellProjectile(attackDamage, mask, range, forwardDirection);
            }

            if (HasRuptureMuscle)
            {
                ruptureMuscleStacks = Mathf.Clamp(ruptureMuscleStacks + 0.5f, 0f, Mathf.Max(1f, ruptureMuscleMaxStacks));
                ruptureMuscleLastAttackTime = now;
            }
        }

        public float GetIncomingDamageMultiplier()
        {
            float multiplier = 1f;
            if (HasBloodContract)
            {
                multiplier *= Mathf.Max(1f, bloodContractIncomingDamageMultiplier);
            }

            if (HasExoskeleton)
            {
                float reduction = Mathf.Clamp01(exoskeletonDamageReductionRatio);
                multiplier *= 1f - reduction;
            }

            if (HasBioBarrier)
            {
                int steps = Mathf.FloorToInt(bioBarrierIdleTime / Mathf.Max(0.1f, bioBarrierIdleSecondsPerStep));
                float reduction = Mathf.Clamp(steps * Mathf.Max(0f, bioBarrierReductionPerStep), 0f, Mathf.Clamp01(bioBarrierMaxReduction));
                multiplier *= 1f - reduction;
            }

            return Mathf.Max(0.01f, multiplier);
        }

        public float ProcessIncomingDamage(float damageAmount, EnemyController sourceEnemy = null)
        {
            float damage = Mathf.Max(0f, damageAmount);
            if (damage <= 0f)
            {
                return 0f;
            }

            damage *= GetIncomingDamageMultiplier();
            damage = Mathf.Max(0f, damage);

            if (HasPlateletMembrane && plateletMembraneCurrentShield > 0f)
            {
                float absorbed = Mathf.Min(plateletMembraneCurrentShield, damage);
                plateletMembraneCurrentShield -= absorbed;
                damage -= absorbed;
                if (plateletMembraneCurrentShield <= 0f)
                {
                    plateletMembraneCurrentShield = 0f;
                    plateletMembraneNextReadyTime = Time.time + Mathf.Max(0.1f, plateletMembraneInterval);
                }
            }

            if (HasReflectiveSkin && sourceEnemy != null && !sourceEnemy.IsDead && damage > 0f)
            {
                float reflectedDamage = damage * Mathf.Max(0f, reflectiveSkinDamageRatio);
                if (reflectedDamage > 0f)
                {
                    sourceEnemy.TakeDamage(reflectedDamage);
                }
            }

            return Mathf.Max(0f, damage);
        }

        public bool TryConsumeSplitRegeneration(float currentHealth, float maxHealth, out float reviveHealth)
        {
            reviveHealth = 0f;
            if (!HasSplitRegeneration || splitRegenerationUsed)
            {
                return false;
            }

            splitRegenerationUsed = true;
            reviveHealth = Mathf.Clamp(Mathf.Max(0f, splitRegenerationReviveHealth), 0f, Mathf.Max(0f, maxHealth));
            if (reviveHealth <= 0f)
            {
                reviveHealth = Mathf.Max(1f, Mathf.Min(2f, maxHealth));
            }

            return reviveHealth > 0f;
        }

        public void NotifyEnemyDefeatedByPlayer(EnemyController enemy)
        {
            if (!HasBloodContract || playerStats == null || playerStats.IsDead)
            {
                return;
            }

            bloodContractKillProgress++;
            int killsPerHeal = Mathf.Max(1, bloodContractKillsPerHeal);
            if (bloodContractKillProgress < killsPerHeal)
            {
                return;
            }

            bloodContractKillProgress = 0;
            float gain = Mathf.Max(0f, bloodContractHealthGainAmount);
            if (gain <= 0f)
            {
                return;
            }

            playerStats.RuntimeStats.AddModifier(
                CharacterStatType.MaxHealth,
                gain,
                CharacterStatModifierMode.Flat,
                this);
            playerStats.Heal(gain);
        }

        public float ApplyPerTargetDamageModifiers(EnemyController enemy, float damage)
        {
            float adjustedDamage = Mathf.Max(0f, damage);
            if (!HasBioResonance || enemy == null)
            {
                return adjustedDamage;
            }

            int enemyId = enemy.GetInstanceID();
            float now = Time.time;
            int maxStacks = Mathf.Max(1, bioResonanceMaxStacks);
            float window = Mathf.Max(0.1f, bioResonanceStackWindow);

            if (!resonanceStatesByEnemyId.TryGetValue(enemyId, out ResonanceState state))
            {
                state = new ResonanceState
                {
                    Stacks = 0,
                    LastHitTime = float.NegativeInfinity
                };
            }

            int effectiveStacks = 0;
            if (now - state.LastHitTime <= window)
            {
                effectiveStacks = Mathf.Clamp(state.Stacks, 0, maxStacks);
            }

            float stackBonus = Mathf.Max(0f, bioResonanceStackDamageBonus) * effectiveStacks;
            adjustedDamage *= 1f + stackBonus;

            state.Stacks = Mathf.Clamp(effectiveStacks + 1, 1, maxStacks);
            state.LastHitTime = now;
            resonanceStatesByEnemyId[enemyId] = state;

            return adjustedDamage;
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
            if (enemy == null)
            {
                return;
            }

            // Spawn acid puddle immediately at hit position even when the enemy dies on impact.
            if (HasAcidicRupture)
            {
                float tickDamage = Mathf.Max(1f, hitDamage * acidTickDamageRatio);
                AcidPuddle.Spawn(hitPosition, tickDamage, acidDuration, GetExplosionRadius(), acidTickInterval);
            }

            if (enemy.IsDead)
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

        public void TrySpawnMutantEyeSplitProjectiles(Vector3 origin, Vector3 forwardDirection, float damage, LayerMask mask, float range)
        {
            // Mutant Eye behavior changed: no split projectile spawning.
        }

        public void NotifyMutantEyeFireVisual(Vector3 origin, Vector3 direction, float penaltyAngle)
        {
            // Visual intentionally disabled.
        }

        public void NotifyMeleeAttackVisual(Vector3 center, Vector3 direction, float width, float depth)
        {
            // Visual intentionally disabled.
        }

        public void NotifyUnstableCoreRollVisual(float multiplier, Vector3 origin)
        {
            // Visual intentionally disabled. Unstable Core now uses player overlay tint.
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

        private Vector3 GetMovementAnchorPosition()
        {
            Transform anchor = playerController != null ? playerController.transform : transform;
            return anchor.position;
        }

        private void UpdateMovementIntensity()
        {
            Vector3 currentPosition = GetMovementAnchorPosition();
            Vector3 delta = currentPosition - previousPosition;
            previousPosition = currentPosition;

            delta.y = 0f;
            float planarSpeed = Time.deltaTime > 0.0001f ? delta.magnitude / Time.deltaTime : 0f;
            isMovingByPosition = planarSpeed > 0.05f;
            float target = Mathf.Clamp01(planarSpeed / 2f);
            movementIntensity = Mathf.MoveTowards(movementIntensity, target, Time.deltaTime * 5f);
        }

        private void UpdateRampageBloodFlowState()
        {
            if (!HasRampageBloodFlow)
            {
                rampageMoveAccumulatedTime = 0f;
                rampageIdleAccumulatedTime = 0f;
                rampageAttackBonus = 0;
                return;
            }

            bool isMovingNow = (playerController != null && playerController.IsMoving) || isMovingByPosition || movementIntensity > 0.02f;
            if (isMovingNow)
            {
                rampageIdleAccumulatedTime = 0f;
                if (rampageAttackBonus >= Mathf.Max(0, rampageMaxAttackBonus))
                {
                    return;
                }

                rampageMoveAccumulatedTime += Time.deltaTime;
                float secondsPerBonus = Mathf.Max(0.1f, rampageMoveSecondsPerBonus);
                while (rampageMoveAccumulatedTime >= secondsPerBonus && rampageAttackBonus < Mathf.Max(0, rampageMaxAttackBonus))
                {
                    rampageMoveAccumulatedTime -= secondsPerBonus;
                    rampageAttackBonus++;
                }

                return;
            }

            if (rampageAttackBonus <= 0)
            {
                return;
            }

            rampageIdleAccumulatedTime += Time.deltaTime;
            float secondsPerDecay = Mathf.Max(0.1f, rampageIdleSecondsPerDecay);
            while (rampageIdleAccumulatedTime >= secondsPerDecay && rampageAttackBonus > 0)
            {
                rampageIdleAccumulatedTime -= secondsPerDecay;
                rampageAttackBonus--;
            }
        }

        private void UpdateTentacleAutoAttack()
        {
            if (!HasOrganTentacle || playerStats == null || playerStats.IsDead)
            {
                return;
            }

            if (Time.time < tentacleNextAutoAttackTime)
            {
                return;
            }

            tentacleNextAutoAttackTime = Time.time + Mathf.Max(0.15f, organTentacleAutoAttackInterval);

            var enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return;
            }

            Vector3 center = transform.position;
            float radiusSqr = Mathf.Max(0.5f, organTentacleAutoAttackRadius);
            radiusSqr *= radiusSqr;
            float damage = Mathf.Max(0.1f, playerStats.AttackPower * Mathf.Max(0.05f, organTentacleAutoAttackDamageMultiplier));
            int hitLimit = Mathf.Max(1, organTentacleMaxTargets);
            int hitCount = 0;

            for (int i = 0; i < enemies.Count && hitCount < hitLimit; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - center;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude > radiusSqr)
                {
                    continue;
                }

                float adjustedDamage = ApplyPerTargetDamageModifiers(enemy, damage);
                enemy.TakeDamage(adjustedDamage);
                ApplyCommonOnHitEffects(enemy, adjustedDamage, enemy.transform.position);
                hitCount++;
            }
        }

        private void UpdateDecayStates()
        {
            float now = Time.time;
            if (HasOverheatedOrgan)
            {
                if (overheatStacks > 0 && now >= overheatNextDecayTime)
                {
                    overheatStacks = 0;
                    overheatLastAttackTime = float.NegativeInfinity;
                    overheatNextDecayTime = float.PositiveInfinity;
                }
            }
            else
            {
                overheatStacks = 0;
                overheatLastAttackTime = float.NegativeInfinity;
                overheatNextDecayTime = float.PositiveInfinity;
            }

            if (!HasBioResonance)
            {
                resonanceStatesByEnemyId.Clear();
                return;
            }

            if (resonanceStatesByEnemyId.Count == 0)
            {
                return;
            }

            float cleanupThreshold = Mathf.Max(0.2f, bioResonanceStackWindow * 4f);
            tempResonanceRemovalIds.Clear();
            foreach (KeyValuePair<int, ResonanceState> pair in resonanceStatesByEnemyId)
            {
                if (now - pair.Value.LastHitTime > cleanupThreshold)
                {
                    tempResonanceRemovalIds.Add(pair.Key);
                }
            }

            for (int i = 0; i < tempResonanceRemovalIds.Count; i++)
            {
                resonanceStatesByEnemyId.Remove(tempResonanceRemovalIds[i]);
            }
        }

        private void UpdateDecayOrganState()
        {
            if (!HasDecayOrgan)
            {
                decayOrganStartTime = float.NegativeInfinity;
                decayOrganAttackBonus = 0;
                return;
            }

            if (decayOrganStartTime <= float.NegativeInfinity * 0.5f)
            {
                decayOrganStartTime = Time.time;
                decayOrganAttackBonus = 0;
                return;
            }

            float elapsed = Mathf.Max(0f, Time.time - decayOrganStartTime);
            float secondsPerBonus = Mathf.Max(0.1f, decayOrganSecondsPerAttackBonus);
            int bonus = Mathf.FloorToInt(elapsed / secondsPerBonus);
            decayOrganAttackBonus = Mathf.Clamp(bonus, 0, Mathf.Max(0, decayOrganMaxAttackBonus));
        }

        private void UpdateRuptureMuscleState()
        {
            if (!HasRuptureMuscle)
            {
                ruptureMuscleStacks = 0;
                ruptureMuscleLastAttackTime = float.NegativeInfinity;
                ApplyRuptureMovePenalty(0f);
                return;
            }

            float decayDelay = Mathf.Max(0.25f, ruptureMuscleDecayDelay);
            if (ruptureMuscleStacks > 0 && Time.time - ruptureMuscleLastAttackTime > decayDelay)
            {
                ruptureMuscleStacks = Mathf.Max(0f, ruptureMuscleStacks - 1f);
                ruptureMuscleLastAttackTime = Time.time;
            }

            int effectiveRuptureStacks = Mathf.FloorToInt(Mathf.Max(0f, ruptureMuscleStacks));
            float targetPenalty = Mathf.Max(0f, ruptureMuscleMovePenaltyPerStack) * effectiveRuptureStacks;
            ApplyRuptureMovePenalty(targetPenalty);
        }

        private void UpdateImperfectRegenState()
        {
            if (!HasImperfectRegeneration || playerStats == null || playerStats.IsDead)
            {
                imperfectRegenPendingHeal = 0f;
                imperfectRegenHealReadyTime = float.PositiveInfinity;
                imperfectRegenCooldownUntil = float.NegativeInfinity;
                return;
            }

            if (imperfectRegenPendingHeal <= 0f || Time.time < imperfectRegenHealReadyTime)
            {
                return;
            }

            float healAmount = imperfectRegenPendingHeal;
            imperfectRegenPendingHeal = 0f;
            imperfectRegenHealReadyTime = float.PositiveInfinity;
            playerStats.Heal(healAmount);
            imperfectRegenCooldownUntil = Time.time + Mathf.Max(0f, imperfectRegenCooldownDuration);
        }

        private void ApplyRuptureMovePenalty(float penalty)
        {
            if (playerStats == null || playerStats.RuntimeStats == null)
            {
                ruptureAppliedMovePenalty = penalty;
                return;
            }

            if (Mathf.Approximately(ruptureAppliedMovePenalty, penalty))
            {
                return;
            }

            playerStats.RuntimeStats.RemoveModifiersFromSource(ruptureMovePenaltyModifierSource);
            ruptureAppliedMovePenalty = Mathf.Max(0f, penalty);
            if (ruptureAppliedMovePenalty > 0f)
            {
                playerStats.RuntimeStats.AddModifier(
                    CharacterStatType.MoveSpeed,
                    -ruptureAppliedMovePenalty,
                    CharacterStatModifierMode.Flat,
                    ruptureMovePenaltyModifierSource);
            }
        }

        private void SyncPersistentStatModifiers()
        {
            if (playerStats == null)
            {
                return;
            }

            SetSimpleModifierState(
                HasForbiddenGrowth,
                ref forbiddenGrowthModifierApplied,
                forbiddenGrowthModifierSource,
                new CharacterStatModifier(CharacterStatType.MaxHealth, -Mathf.Max(0f, forbiddenGrowthMaxHealthPenalty), CharacterStatModifierMode.Flat, forbiddenGrowthModifierSource),
                new CharacterStatModifier(CharacterStatType.AttackPower, Mathf.Max(0f, forbiddenGrowthFlatAttackBonus), CharacterStatModifierMode.Flat, forbiddenGrowthModifierSource));

            SetSimpleModifierState(
                HasOverclockNerve,
                ref overclockNerveModifierApplied,
                overclockNerveModifierSource,
                new CharacterStatModifier(CharacterStatType.MaxHealth, -Mathf.Max(0f, overclockNerveMaxHealthPenalty), CharacterStatModifierMode.Flat, overclockNerveModifierSource),
                new CharacterStatModifier(CharacterStatType.MoveSpeed, Mathf.Max(0f, overclockNerveFlatMoveSpeedBonus), CharacterStatModifierMode.Flat, overclockNerveModifierSource));

            SetSimpleModifierState(
                HasImperfectRegeneration,
                ref imperfectRegenModifierApplied,
                imperfectRegenModifierSource,
                new CharacterStatModifier(CharacterStatType.MaxHealth, -Mathf.Max(0f, imperfectRegenMaxHealthPenalty), CharacterStatModifierMode.Flat, imperfectRegenModifierSource));

            SetSimpleModifierState(
                HasExoskeleton,
                ref exoskeletonModifierApplied,
                exoskeletonModifierSource,
                new CharacterStatModifier(CharacterStatType.MoveSpeed, -Mathf.Max(0f, exoskeletonMoveSpeedPenalty), CharacterStatModifierMode.Flat, exoskeletonModifierSource));
        }

        private void SetSimpleModifierState(
            bool shouldApply,
            ref bool appliedFlag,
            object source,
            params CharacterStatModifier[] modifiers)
        {
            if (playerStats == null || playerStats.RuntimeStats == null)
            {
                return;
            }

            if (shouldApply && !appliedFlag)
            {
                for (int i = 0; i < modifiers.Length; i++)
                {
                    playerStats.RuntimeStats.AddModifier(modifiers[i]);
                }

                appliedFlag = true;
                return;
            }

            if (!shouldApply && appliedFlag)
            {
                playerStats.RuntimeStats.RemoveModifiersFromSource(source);
                appliedFlag = false;
            }
        }

        private void ClearPersistentStatModifiers()
        {
            if (playerStats == null || playerStats.RuntimeStats == null)
            {
                return;
            }

            playerStats.RuntimeStats.RemoveModifiersFromSource(forbiddenGrowthModifierSource);
            playerStats.RuntimeStats.RemoveModifiersFromSource(overclockNerveModifierSource);
            playerStats.RuntimeStats.RemoveModifiersFromSource(imperfectRegenModifierSource);
            playerStats.RuntimeStats.RemoveModifiersFromSource(ruptureMovePenaltyModifierSource);
            playerStats.RuntimeStats.RemoveModifiersFromSource(exoskeletonModifierSource);
            forbiddenGrowthModifierApplied = false;
            overclockNerveModifierApplied = false;
            imperfectRegenModifierApplied = false;
            exoskeletonModifierApplied = false;
            ruptureAppliedMovePenalty = 0f;
        }

        private void TrySubscribeHealthEvents()
        {
            if (subscribedHealthEvents || playerStats == null || playerStats.RuntimeStats == null)
            {
                return;
            }

            playerStats.HealthChanged += HandlePlayerHealthChanged;
            subscribedHealthEvents = true;
        }

        private void TryUnsubscribeHealthEvents()
        {
            if (!subscribedHealthEvents || playerStats == null)
            {
                return;
            }

            playerStats.HealthChanged -= HandlePlayerHealthChanged;
            subscribedHealthEvents = false;
        }

        private void HandlePlayerHealthChanged(CharacterStats _, CharacterHealthChangedEventArgs args)
        {
            if (args.CurrentValue >= args.PreviousValue)
            {
                return;
            }

            if (!Mathf.Approximately(args.MaxValue, args.PreviousMaxValue))
            {
                return;
            }

            float damageTaken = Mathf.Max(0f, args.PreviousValue - args.CurrentValue);
            if (damageTaken <= 0f)
            {
                return;
            }

            if (HasImperfectRegeneration)
            {
                bool canScheduleRegen = Time.time >= imperfectRegenCooldownUntil;
                canScheduleRegen &= !(imperfectRegenPendingHeal > 0f && Time.time < imperfectRegenHealReadyTime);
                if (canScheduleRegen)
                {
                    imperfectRegenPendingHeal = Mathf.Max(0f, imperfectRegenHealPerTrigger);
                    imperfectRegenHealReadyTime = Time.time + Mathf.Max(0.1f, imperfectRegenDelay);
                }
            }

            if (HasSeveranceReflex)
            {
                severanceReflexBuffUntil = Time.time + Mathf.Max(0.1f, severanceReflexDuration);
            }
        }

        private void TriggerOverheatExplosionSelfDamage(float attackDamage)
        {
            float damage = Mathf.Max(0.1f, overheatExplosionSelfDamage);
            SpawnOverheatExplosionVisual();
            playerStats?.RuntimeStats?.ApplyDamage(damage);
        }

        private void SpawnVoidCellProjectile(float attackDamage, LayerMask mask, float range, Vector3 fallbackDirection)
        {
            EnemyController target = FindRandomEnemyWithinRadius(12f);
            if (target == null)
            {
                return;
            }

            Vector3 targetPos = target.transform.position;
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Mathf.Max(0.3f, voidCellSpawnRadius);
            Vector3 spawnPos = targetPos + new Vector3(randomCircle.x, 0f, randomCircle.y);
            spawnPos.y = transform.position.y + 0.2f;
            TeleportPlayerTo(spawnPos);

            Vector3 shootDir = targetPos - spawnPos;
            shootDir.y = 0f;
            if (shootDir.sqrMagnitude <= 0.0001f)
            {
                shootDir = fallbackDirection.sqrMagnitude > 0.0001f ? fallbackDirection.normalized : Vector3.forward;
            }

            SpawnChildProjectile(
                spawnPos,
                shootDir.normalized,
                Mathf.Max(0.05f, attackDamage * Mathf.Max(0.05f, voidCellDamageMultiplier)),
                mask,
                Mathf.Max(0.25f, range * 0.6f),
                Projectile.SpawnKind.SplitChild);
        }

        private static EnemyController FindRandomEnemyWithinRadius(float radius)
        {
            var enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return null;
            }

            List<EnemyController> candidates = new List<EnemyController>();
            Vector3 center = PlayerController.Instance != null ? PlayerController.Instance.transform.position : Vector3.zero;
            float radiusSqr = Mathf.Max(0.5f, radius);
            radiusSqr *= radiusSqr;

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - center;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude > radiusSqr)
                {
                    continue;
                }

                candidates.Add(enemy);
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            int index = Random.Range(0, candidates.Count);
            return candidates[index];
        }

        private void UpdateUnstableCoreOverlay()
        {
            if (!HasUnstableCore)
            {
                if (unstableCoreOverlay != null)
                {
                    unstableCoreOverlay.enabled = false;
                }

                return;
            }

            EnsureUnstableCoreOverlay();
            if (unstableCoreOverlay == null)
            {
                return;
            }

            unstableCoreOverlay.enabled = true;
            if (playerVisualSpriteRenderer == null)
            {
                playerVisualSpriteRenderer = FindPlayerSpriteRenderer();
            }

            if (playerVisualSpriteRenderer != null)
            {
                unstableCoreOverlay.sprite = playerVisualSpriteRenderer.sprite;
                unstableCoreOverlay.flipX = playerVisualSpriteRenderer.flipX;
                unstableCoreOverlay.flipY = playerVisualSpriteRenderer.flipY;
                unstableCoreOverlay.sortingLayerID = playerVisualSpriteRenderer.sortingLayerID;
                unstableCoreOverlay.sortingOrder = playerVisualSpriteRenderer.sortingOrder + 2;
            }

            float multiplier = RollUnstableCoreDamageMultiplier();
            float attackPower = playerStats != null ? Mathf.Max(0.01f, playerStats.AttackPower) : 1f;
            float minMultiplier = Mathf.Max(0f, unstableCoreMinAttackRatio);
            float maxMultiplier = Mathf.Max(minMultiplier, (attackPower + Mathf.Max(0f, unstableCoreMaxAttackFlatBonus)) / attackPower);
            float lowToHigh = Mathf.InverseLerp(
                minMultiplier,
                maxMultiplier,
                multiplier);
            Color lowColor = new Color(0.38f, 0.78f, 1f, 0.88f);
            Color highColor = new Color(1f, 0.22f, 0.2f, 0.88f);
            unstableCoreOverlay.color = Color.Lerp(lowColor, highColor, lowToHigh);
            unstableCoreOverlay.transform.localScale = Vector3.one * 1.12f;
        }

        private void UpdatePlateletMembraneOutline()
        {
            if (!HasPlateletMembrane || plateletMembraneCurrentShield <= 0f)
            {
                ClearPlateletMembraneOutline();
                return;
            }

            EnsurePlateletMembraneOutlineMaterial();
            if (plateletMembraneOutlineMaterial == null)
            {
                return;
            }

            if (playerVisualSpriteRenderer == null)
            {
                playerVisualSpriteRenderer = FindPlayerSpriteRenderer();
            }

            if (playerVisualSpriteRenderer == null)
            {
                ClearPlateletMembraneOutline();
                return;
            }

            if (plateletMembraneOutlineTarget != playerVisualSpriteRenderer)
            {
                ClearPlateletMembraneOutline();
                CleanupPlateletMembraneLegacyVisuals(playerVisualSpriteRenderer.transform);
                plateletMembraneOutlineTarget = playerVisualSpriteRenderer;
            }

            plateletMembraneOutlineMaterial.SetColor(OutlineColorId, new Color(0.72f, 0.74f, 0.76f, 1f));
            plateletMembraneOutlineMaterial.SetFloat(OutlineSizeId, 1.25f);
            plateletMembraneOutlineMaterial.SetFloat(OutlineExpandId, 0f);
            EnsurePlateletMembraneOutlineRenderer(playerVisualSpriteRenderer);
            if (plateletMembraneOutlineRenderer == null)
            {
                return;
            }

            plateletMembraneOutlineRenderer.sprite = playerVisualSpriteRenderer.sprite;
            plateletMembraneOutlineRenderer.flipX = playerVisualSpriteRenderer.flipX;
            plateletMembraneOutlineRenderer.flipY = playerVisualSpriteRenderer.flipY;
            plateletMembraneOutlineRenderer.color = Color.white;
            plateletMembraneOutlineRenderer.sortingLayerID = playerVisualSpriteRenderer.sortingLayerID;
            plateletMembraneOutlineRenderer.sortingOrder = playerVisualSpriteRenderer.sortingOrder - 1;
            plateletMembraneOutlineRenderer.sharedMaterial = plateletMembraneOutlineMaterial;
            plateletMembraneOutlineRenderer.enabled = true;
        }

        private void EnsurePlateletMembraneOutlineMaterial()
        {
            if (plateletMembraneOutlineMaterial != null)
            {
                return;
            }

            Shader outlineShader = Shader.Find("Necrocis/SpriteOutline");
            if (outlineShader == null)
            {
                if (!plateletMembraneShaderWarningLogged)
                {
                    Debug.LogWarning("[PlayerItemCombatEffects] Necrocis/SpriteOutline shader not found.");
                    plateletMembraneShaderWarningLogged = true;
                }

                return;
            }

            plateletMembraneOutlineMaterial = new Material(outlineShader)
            {
                name = "Runtime_PlateletMembraneOutline"
            };
        }

        private void ClearPlateletMembraneOutline()
        {
            if (plateletMembraneOutlineRenderer != null)
            {
                plateletMembraneOutlineRenderer.enabled = false;
            }

            plateletMembraneOutlineRenderer = null;
            plateletMembraneOutlineTarget = null;
        }

        private void EnsurePlateletMembraneOutlineRenderer(SpriteRenderer sourceRenderer)
        {
            if (sourceRenderer == null)
            {
                return;
            }

            if (plateletMembraneOutlineRenderer != null)
            {
                return;
            }

            Transform existing = sourceRenderer.transform.Find("PlateletMembraneOutline");
            plateletMembraneOutlineRenderer = existing != null ? existing.GetComponent<SpriteRenderer>() : null;
            if (plateletMembraneOutlineRenderer == null)
            {
                GameObject outlineObject = new GameObject("PlateletMembraneOutline");
                outlineObject.transform.SetParent(sourceRenderer.transform, false);
                outlineObject.transform.localPosition = Vector3.zero;
                outlineObject.transform.localRotation = Quaternion.identity;
                outlineObject.transform.localScale = Vector3.one;
                plateletMembraneOutlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
            }

            plateletMembraneOutlineRenderer.enabled = false;
        }

        private static void CleanupPlateletMembraneLegacyVisuals(Transform sourceTransform)
        {
            if (sourceTransform == null)
            {
                return;
            }

            for (int i = sourceTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = sourceTransform.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                string childName = child.name;
                if (childName == "PlateletMembraneOverlay"
                    || childName == "PlateletMembraneOutline"
                    || childName.StartsWith("PlateletMembraneOutline_"))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void EnsureUnstableCoreOverlay()
        {
            if (unstableCoreOverlay != null)
            {
                return;
            }

            SpriteRenderer sourceRenderer = FindPlayerSpriteRenderer();
            if (sourceRenderer == null)
            {
                return;
            }
            playerVisualSpriteRenderer = sourceRenderer;

            Transform existing = sourceRenderer.transform.Find("UnstableCoreOverlay");
            if (existing != null)
            {
                unstableCoreOverlay = existing.GetComponent<SpriteRenderer>();
            }

            if (unstableCoreOverlay == null)
            {
                GameObject overlayObject = new GameObject("UnstableCoreOverlay");
                overlayObject.transform.SetParent(sourceRenderer.transform, false);
                overlayObject.transform.localPosition = Vector3.zero;
                overlayObject.transform.localRotation = Quaternion.identity;
                overlayObject.transform.localScale = Vector3.one;
                unstableCoreOverlay = overlayObject.AddComponent<SpriteRenderer>();
            }

            unstableCoreOverlay.sprite = sourceRenderer.sprite;
            unstableCoreOverlay.sortingLayerID = sourceRenderer.sortingLayerID;
            unstableCoreOverlay.sortingOrder = sourceRenderer.sortingOrder + 2;
            unstableCoreOverlay.enabled = false;
        }

        private SpriteRenderer FindPlayerSpriteRenderer()
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                string objectName = renderer.gameObject.name;
                if (objectName.Contains("Overlay") || objectName.Contains("Outline"))
                {
                    continue;
                }

                return renderer;
            }

            return null;
        }

        private void SpawnOverheatExplosionVisual()
        {
            Vector3 center = GetPlayerVisualCenter();
            GameObject fx = new GameObject("OverheatExplosionFx");
            fx.transform.position = center;

            SpriteRenderer renderer = fx.AddComponent<SpriteRenderer>();
            renderer.sprite = TextureSpriteCache.GetCircleSprite();
            renderer.color = new Color(1f, 0.22f, 0.1f, 0.86f);
            renderer.sortingOrder = 5300;
            fx.transform.localScale = Vector3.one * 1.6f;
            Destroy(fx, 0.2f);
        }

        private Vector3 GetPlayerVisualCenter()
        {
            SpriteRenderer renderer = FindPlayerSpriteRenderer();
            if (renderer != null && renderer.sprite != null)
            {
                return renderer.bounds.center;
            }

            return transform.position + Vector3.up * 0.6f;
        }

        private void TeleportPlayerTo(Vector3 position)
        {
            if (playerController != null)
            {
                playerController.SpawnAt(position);
                return;
            }

            transform.position = position;
        }
    }
}
