using System;
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
        public const string InfectedHostId = "infected_host";
        public const string SporeColonyId = "spore_colony";
        public const string BloodDroneId = "blood_drone";
        public const string GuardianOrganId = "guardian_organ";
        public const string TentacleColonyId = "tentacle_colony";
        public const string ElectricNeuralNetworkId = "electric_neural_network";
        public const string InfectionTransferenceId = "infection_transference";
        public const string MacrophageId = "macrophage";
        public const string GluttonousOrganId = "gluttonous_organ";
        public const string HeartSniperId = "heart_sniper";
        public const string BloodflowAccelerationId = "bloodflow_acceleration";
        public const string FocusedNerveId = "focused_nerve";
        public const string ExecutionInstinctId = "execution_instinct";
        public const string BerserkCellId = "berserk_cell";
        public const string UnstableCellId = "unstable_cell";
        public const string GrotesqueGrowthId = "grotesque_growth";
        public const string MutationRampageId = "mutation_rampage";
        public const string ParasiticBombId = "parasitic_bomb";
        public const string FrenzyHormoneId = "frenzy_hormone";

        private const string ParasiticBombVisualPoolName = "PlayerItem.ParasiticBombVisual";
        private const string OverheatExplosionVisualPoolName = "PlayerItem.OverheatExplosionVisual";
        private const string SporeBurstVisualPoolName = "PlayerItem.SporeBurstVisual";
        private const string GuardianBlockVisualPoolName = "PlayerItem.GuardianBlockVisual";
        private const string TentacleAnchorVisualPoolName = "PlayerItem.TentacleAnchorVisual";
        private const string LineVisualPoolName = "PlayerItem.LineVisual";
        private const string BloodDronePoolName = "PlayerItem.BloodDrone";
        private const string GuardianOrganPoolName = "PlayerItem.GuardianOrgan";
        private const string BloodDroneProjectilePoolName = "PlayerItem.BloodDroneProjectile";

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

        [Header("Bio Companions")]
        [SerializeField, Range(0f, 1f)] private float infectedHostChance = 0.22f;
        [SerializeField] private float infectedHostLifetime = 9f;
        [SerializeField] private float infectedHostDamageMultiplier = 1f;
        [SerializeField] private float infectedHostAttackInterval = 0.8f;
        [SerializeField] private float infectedHostAttackRadius = 1.25f;
        [SerializeField] private float infectedHostSearchRadius = 7f;
        [SerializeField, Min(1)] private int infectedHostMaxAllies = 3;
        [SerializeField] private float sporeSpawnInterval = 8f;
        [SerializeField] private float sporeLifetime = 6f;
        [SerializeField] private float sporeDamageMultiplier = 1f;
        [SerializeField] private float sporeSearchRadius = 8f;
        [SerializeField] private float sporeBurstRadius = 1.1f;
        [SerializeField] private float bloodDroneOrbitRadius = 1.4f;
        [SerializeField] private float bloodDroneFireInterval = 0.9f;
        [SerializeField] private float bloodDroneDamageMultiplier = 0.4f;
        [SerializeField] private float bloodDroneTargetRadius = 9f;
        [SerializeField] private float guardianOrganOrbitRadius = 1.2f;
        [SerializeField] private float guardianOrganBlockRadius = 0.9f;
        [SerializeField] private float guardianOrganCooldown = 2.5f;
        [SerializeField] private float tentacleBindInterval = 2.2f;
        [SerializeField] private float tentacleBindRadius = 5.5f;
        [SerializeField] private float tentacleBindDuration = 1.5f;
        [SerializeField] private float tentacleSlowRatio = 0.95f;
        [SerializeField] private float tentacleDamageMultiplier = 0.15f;
        [SerializeField, Min(1)] private int tentacleMaxTargets = 2;

        [Header("Kill Chain Items")]
        [SerializeField] private float electricChainDamage = 2f;
        [SerializeField] private float electricChainRadius = 8f;
        [SerializeField, Min(1)] private int electricChainMaxHits = 4;
        [SerializeField] private float electricChainCooldown = 5f;
        [SerializeField] private float infectionTransferRadius = 4.5f;
        [SerializeField, Min(1)] private int infectionTransferMaxTargets = 3;
        [SerializeField] private float infectionTransferDuration = 3f;
        [SerializeField] private float infectionTransferTickInterval = 1f;
        [SerializeField] private float infectionTransferTickDamage = 1f;
        [SerializeField] private float infectionTransferCooldown = 5f;
        [SerializeField] private float macrophageDuration = 5f;
        [SerializeField] private float macrophageAttackBonusPerStack = 1f;
        [SerializeField, Min(1)] private int macrophageMaxStacks = 3;
        [SerializeField] private float gluttonousOrganDuration = 3f;
        [SerializeField] private float gluttonousOrganMoveBonusPerStack = 0.5f;
        [SerializeField, Min(1)] private int gluttonousOrganMaxStacks = 4;

        [Header("Boss Fight Items")]
        [SerializeField] private float heartSniperHealthThreshold = 0.6f;
        [SerializeField] private float heartSniperDamageBonusRatio = 0.5f;
        [SerializeField] private float bloodflowAccelerationRadius = 9f;
        [SerializeField] private float bloodflowAccelerationAttackSpeedBonus = 0.5f;
        [SerializeField] private float focusedNerveRadius = 6f;
        [SerializeField] private float focusedNerveHighAttackBonus = 3f;
        [SerializeField] private float focusedNerveLowAttackBonus = 1f;
        [SerializeField] private float executionInstinctHealthThreshold = 0.3f;
        [SerializeField, Range(0f, 1f)] private float executionInstinctChance = 0.3f;
        [SerializeField] private float executionInstinctBossDamageMultiplier = 2f;
        [SerializeField] private float berserkCellDuration = 30f;
        [SerializeField] private float berserkCellAttackBonus = 3f;
        [SerializeField] private float berserkCellMoveBonus = 1f;
        [SerializeField] private float berserkCellAttackSpeedBonus = 2f;
        [SerializeField] private float berserkCellBossDetectionRadius = 18f;

        [Header("Mutation Chaos Items")]
        [SerializeField] private float unstableCellMinProjectileSpeedMultiplier = 0.5f;
        [SerializeField] private float unstableCellMaxProjectileSpeedMultiplier = 1.5f;
        [SerializeField] private float unstableCellSlowThreshold = 0.9f;
        [SerializeField] private float unstableCellSlowDamageMultiplier = 2f;
        [SerializeField] private float grotesqueGrowthInterval = 10f;
        [SerializeField] private float grotesqueGrowthSmallScale = 0.75f;
        [SerializeField] private float grotesqueGrowthLargeScale = 1.35f;
        [SerializeField] private float grotesqueGrowthSmallMoveBonus = 2f;
        [SerializeField] private float grotesqueGrowthLargeAttackBonus = 2f;
        [SerializeField] private float grotesqueGrowthLargeMovePenalty = 0.5f;
        [SerializeField] private float mutationRampageInterval = 15f;
        [SerializeField] private float mutationRampageDuration = 7f;
        [SerializeField, Range(0f, 1f)] private float mutationRampageBuffChance = 0.7f;
        [SerializeField] private float mutationRampageAttackBuff = 2f;
        [SerializeField] private float mutationRampageMoveBuff = 1.5f;
        [SerializeField] private float mutationRampageAttackSpeedBuff = 1f;
        [SerializeField] private float mutationRampageAttackDebuff = 1f;
        [SerializeField] private float mutationRampageMoveDebuff = 1f;
        [SerializeField] private float mutationRampageAttackSpeedDebuff = 0.5f;
        [SerializeField, Range(0f, 1f)] private float parasiticBombChance = 0.3f;
        [SerializeField] private float parasiticBombRadius = 5f;
        [SerializeField] private float parasiticBombDamageMultiplier = 2f;
        [SerializeField] private float frenzyHormoneDuration = 4f;
        [SerializeField] private float frenzyHormoneCooldown = 5f;
        [SerializeField] private float frenzyHormoneMinBonus = 1f;
        [SerializeField] private float frenzyHormoneMaxBonus = 2f;


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
        private readonly List<PlayerBioSummon> activeBioSummons = new List<PlayerBioSummon>();
        private readonly List<EnemyController> tempEnemyTargets = new List<EnemyController>();
        private readonly HashSet<EnemyController> tempParasiticBombEnemies = new HashSet<EnemyController>();
        private readonly List<EnemyController> randomEnemyCandidates = new List<EnemyController>();
        private PlayerBloodDrone bloodDrone;
        private PlayerGuardianOrgan guardianOrgan;
        private float nextSporeSpawnTime;
        private float tentacleNextBindTime;
        private bool itemCacheInitialized;
        private bool disablingForEmptyInventory;
        private bool hasAnyCachedItems;
        private bool hasPersistentStatItems;
        private bool hasDefensiveUpdateItems;
        private bool hasPlateletMembraneItem;
        private bool hasRecoveryFactorItem;
        private bool hasBioBarrierItem;
        private bool hasSplitRegenerationItem;
        private bool hasBloodContractItem;
        private bool hasSeveranceReflexItem;
        private bool hasMovementTrackingItems;
        private bool hasRampageBloodFlowItem;
        private bool hasDecayOrganItem;
        private bool hasRuptureMuscleItem;
        private bool hasImperfectRegenItem;
        private bool hasOrganTentacleItem;
        private bool hasBioCompanionUpdateItems;
        private bool hasSporeColonyItem;
        private bool hasBloodDroneItem;
        private bool hasGuardianOrganItem;
        private bool hasTentacleColonyItem;
        private bool hasKillChainBuffItems;
        private bool hasMacrophageItem;
        private bool hasGluttonousOrganItem;
        private bool hasBossFightUpdateItems;
        private bool hasFocusedNerveItem;
        private bool hasBerserkCellItem;
        private bool hasMutationChaosItems;
        private bool hasGrotesqueGrowthItem;
        private bool hasMutationRampageItem;
        private bool hasFrenzyHormoneItem;
        private bool hasDecayStateItems;
        private bool hasOverheatedOrganItem;
        private bool hasBioResonanceItem;
        private bool hasUnstableCoreItem;
        private static Material runtimeLineMaterial;
        private static readonly Func<GameObject> CreateCircleVisualFunc = CreateCircleVisualObject;
        private static readonly Func<GameObject> CreateLineVisualFunc = CreateLineVisualObject;
        private static readonly Func<GameObject> CreateBloodDroneFunc = CreateBloodDroneObject;
        private static readonly Func<GameObject> CreateGuardianOrganFunc = CreateGuardianOrganObject;
        private static readonly Func<GameObject> CreateBloodDroneProjectileFunc = CreateBloodDroneProjectileObject;
        private int macrophageStacks;
        private float macrophageExpireTime = float.NegativeInfinity;
        private float macrophageAppliedAttackBonus;
        private int gluttonousOrganStacks;
        private float gluttonousOrganExpireTime = float.NegativeInfinity;
        private float gluttonousOrganAppliedMoveBonus;
        private float electricChainNextReadyTime = float.NegativeInfinity;
        private float infectionTransferNextReadyTime = float.NegativeInfinity;
        private int focusedNerveEnemyCount = int.MaxValue;
        private float focusedNerveNextScanTime;
        private float berserkCellExpireTime = float.NegativeInfinity;
        private int berserkCellActiveBossId;
        private int berserkCellLastTriggeredBossId;
        private bool berserkCellModifierApplied;
        private float grotesqueGrowthNextRollTime;
        private bool grotesqueGrowthModifierApplied;
        private bool grotesqueGrowthScaleCached;
        private Vector3 grotesqueGrowthBaseScale = Vector3.one;
        private float mutationRampageNextRollTime;
        private float mutationRampageExpireTime = float.NegativeInfinity;
        private bool mutationRampageModifierApplied;
        private float frenzyHormoneCooldownUntil = float.NegativeInfinity;
        private float frenzyHormoneExpireTime = float.NegativeInfinity;
        private bool frenzyHormoneModifierApplied;
        private bool applyingParasiticBombDamage;
        private readonly object macrophageModifierSource = new object();
        private readonly object gluttonousOrganModifierSource = new object();
        private readonly object berserkCellModifierSource = new object();
        private readonly object grotesqueGrowthModifierSource = new object();
        private readonly object mutationRampageModifierSource = new object();
        private readonly object frenzyHormoneModifierSource = new object();
        private readonly Collider[] parasiticBombHitBuffer = new Collider[32];

        private struct ResonanceState
        {
            public int Stacks;
            public float LastHitTime;
        }
        private readonly HashSet<string> acquiredItemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
        public bool HasInfectedHost => HasItem(InfectedHostId);
        public bool HasSporeColony => HasItem(SporeColonyId);
        public bool HasBloodDrone => HasItem(BloodDroneId);
        public bool HasGuardianOrgan => HasItem(GuardianOrganId);
        public bool HasTentacleColony => HasItem(TentacleColonyId);
        public bool HasElectricNeuralNetwork => HasItem(ElectricNeuralNetworkId);
        public bool HasInfectionTransference => HasItem(InfectionTransferenceId);
        public bool HasMacrophage => HasItem(MacrophageId);
        public bool HasGluttonousOrgan => HasItem(GluttonousOrganId);
        public bool HasHeartSniper => HasItem(HeartSniperId);
        public bool HasBloodflowAcceleration => HasItem(BloodflowAccelerationId);
        public bool HasFocusedNerve => HasItem(FocusedNerveId);
        public bool HasExecutionInstinct => HasItem(ExecutionInstinctId);
        public bool HasBerserkCell => HasItem(BerserkCellId);
        public bool HasUnstableCell => HasItem(UnstableCellId);
        public bool HasGrotesqueGrowth => HasItem(GrotesqueGrowthId);
        public bool HasMutationRampage => HasItem(MutationRampageId);
        public bool HasParasiticBomb => HasItem(ParasiticBombId);
        public bool HasFrenzyHormone => HasItem(FrenzyHormoneId);

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
            RebuildItemCache();
        }

        private void OnEnable()
        {
            if (itemManager == null)
            {
                itemManager = GetComponent<PlayerItemManager>();
            }

            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStats>();
            }

            TrySubscribeHealthEvents();

            if (itemManager == null)
            {
                return;
            }

            itemManager.ItemAcquired += HandleItemAcquired;
            itemManager.ItemRemoved += HandleItemRemoved;
            RebuildItemCache();
        }

        private void OnDisable()
        {
            if (itemManager != null)
            {
                itemManager.ItemAcquired -= HandleItemAcquired;
                itemManager.ItemRemoved -= HandleItemRemoved;
            }

            TryUnsubscribeHealthEvents();
            ClearPlateletMembraneOutline();
            ClearPersistentStatModifiers();
            resonanceStatesByEnemyId.Clear();
            if (!disablingForEmptyInventory)
            {
                ClearBioCompanions();
            }

            disablingForEmptyInventory = false;
        }

        private void Update()
        {
            if (!hasAnyCachedItems)
            {
                return;
            }

            if (hasPersistentStatItems)
            {
                SyncPersistentStatModifiers();
            }

            if (hasDefensiveUpdateItems)
            {
                UpdateDefensiveStates();
            }

            if (hasMovementTrackingItems)
            {
                UpdateMovementIntensity();
            }

            if (hasRampageBloodFlowItem)
            {
                UpdateRampageBloodFlowState();
            }

            if (hasDecayOrganItem)
            {
                UpdateDecayOrganState();
            }

            if (hasRuptureMuscleItem)
            {
                UpdateRuptureMuscleState();
            }

            if (hasImperfectRegenItem)
            {
                UpdateImperfectRegenState();
            }

            if (hasOrganTentacleItem)
            {
                UpdateTentacleAutoAttack();
            }

            if (hasBioCompanionUpdateItems)
            {
                UpdateBioCompanionItems();
            }

            if (hasKillChainBuffItems)
            {
                UpdateKillChainBuffs();
            }

            if (hasBossFightUpdateItems)
            {
                UpdateBossFightItemStates();
            }

            if (hasMutationChaosItems)
            {
                UpdateMutationChaosItems();
            }

            if (hasDecayStateItems)
            {
                UpdateDecayStates();
            }

            if (hasPlateletMembraneItem)
            {
                UpdatePlateletMembraneOutline();
            }

            if (hasUnstableCoreItem)
            {
                UpdateUnstableCoreOverlay();
            }
        }

        private void UpdateDefensiveStates()
        {
            if (!hasPlateletMembraneItem)
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

            if (!hasRecoveryFactorItem || playerStats == null || playerStats.IsDead)
            {
                recoveryFactorNextHealTime = Time.time + Mathf.Max(0.1f, recoveryFactorInterval);
            }
            else if (Time.time >= recoveryFactorNextHealTime)
            {
                playerStats.Heal(Mathf.Max(0f, recoveryFactorHealAmount));
                recoveryFactorNextHealTime = Time.time + Mathf.Max(0.1f, recoveryFactorInterval);
            }

            bool isMovingNow = (playerController != null && playerController.IsMoving) || isMovingByPosition || movementIntensity > 0.02f;
            if (!hasBioBarrierItem || isMovingNow)
            {
                bioBarrierIdleTime = 0f;
            }
            else
            {
                bioBarrierIdleTime += Time.deltaTime;
            }

            if (!hasSplitRegenerationItem)
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
            return HasItem(CellProliferationId) && UnityEngine.Random.value <= cellProliferationChance;
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

            if (HasBloodflowAcceleration && IsBossOrEliteWithinRadius(Mathf.Max(0.1f, bloodflowAccelerationRadius)))
            {
                speedMultiplier *= 1f + Mathf.Max(0f, bloodflowAccelerationAttackSpeedBonus);
            }

            if (IsBerserkCellActive())
            {
                speedMultiplier *= 1f + Mathf.Max(0f, berserkCellAttackSpeedBonus);
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

            if (HasFocusedNerve)
            {
                bonus += GetFocusedNerveAttackBonus();
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
                float rolledDamage = UnityEngine.Random.Range(minDamage, maxDamage);
                unstableCoreCurrentMultiplier = rolledDamage / attackPower;
                unstableCoreNextRerollTime = Time.time + Mathf.Max(0.1f, unstableCoreRerollInterval);
                unstableCoreInitialized = true;
            }

            return unstableCoreCurrentMultiplier;
        }

        public float RollUnstableCellProjectileSpeedMultiplier(out float damageMultiplier)
        {
            damageMultiplier = 1f;
            if (!HasUnstableCell)
            {
                return 1f;
            }

            float minSpeed = Mathf.Max(0.05f, unstableCellMinProjectileSpeedMultiplier);
            float maxSpeed = Mathf.Max(minSpeed, unstableCellMaxProjectileSpeedMultiplier);
            float speedMultiplier = UnityEngine.Random.Range(minSpeed, maxSpeed);
            if (speedMultiplier <= Mathf.Max(0.01f, unstableCellSlowThreshold))
            {
                damageMultiplier = Mathf.Max(1f, unstableCellSlowDamageMultiplier);
            }

            return speedMultiplier;
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

            if (HasVoidCell && UnityEngine.Random.value <= Mathf.Clamp01(voidCellChance))
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
            TrySpawnInfectedHost(enemy);
            TriggerElectricNeuralNetwork(enemy);
            TriggerInfectionTransference(enemy);
            TriggerMacrophageBuff();
            TriggerGluttonousOrganBuff();
            TriggerParasiticBomb();

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
            if (enemy == null)
            {
                return adjustedDamage;
            }

            if (HasHeartSniper && IsEnemyAboveHealthRatio(enemy, Mathf.Clamp01(heartSniperHealthThreshold)))
            {
                adjustedDamage *= 1f + Mathf.Max(0f, heartSniperDamageBonusRatio);
            }

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

        public void TryApplyPostDamageExecutionInstinct(EnemyController enemy, float appliedDamage)
        {
            if (!HasExecutionInstinct || enemy == null || enemy.IsDead)
            {
                return;
            }

            if (!IsEnemyBelowHealthRatio(enemy, Mathf.Clamp01(executionInstinctHealthThreshold)))
            {
                return;
            }

            if (UnityEngine.Random.value > Mathf.Clamp01(executionInstinctChance))
            {
                return;
            }

            if (IsBossLikeEnemy(enemy) || enemy.IsElite)
            {
                float bonusMultiplier = Mathf.Max(1f, executionInstinctBossDamageMultiplier) - 1f;
                float bonusDamage = Mathf.Max(0f, appliedDamage) * bonusMultiplier;
                if (bonusDamage > 0f)
                {
                    enemy.TakeDamage(bonusDamage);
                }
                return;
            }

            float remainingHealth = GetEnemyCurrentHealth(enemy);
            if (remainingHealth > 0f)
            {
                enemy.TakeDamage(remainingHealth);
            }
        }

        public bool HasItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            if (itemCacheInitialized)
            {
                return acquiredItemIds.Contains(itemId);
            }

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
            Vector3 forward = forwardDirection.sqrMagnitude > 0.0001f
                ? forwardDirection.normalized
                : Vector3.forward;
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

        private void HandleItemAcquired(PlayerItemManager _, PlayerItemManager.AcquiredPlayerItem acquiredItem)
        {
            if (acquiredItem == null || string.IsNullOrWhiteSpace(acquiredItem.ItemId))
            {
                return;
            }

            RebuildItemCache();
        }

        private void HandleItemRemoved(PlayerItemManager _, PlayerItemManager.AcquiredPlayerItem removedItem)
        {
            if (removedItem == null || string.IsNullOrWhiteSpace(removedItem.ItemId))
            {
                return;
            }

            RebuildItemCache();
        }

        private void RebuildItemCache()
        {
            acquiredItemIds.Clear();
            itemCacheInitialized = true;
            if (itemManager == null || itemManager.AcquiredItems == null)
            {
                RefreshItemRuntimeFlags();
                return;
            }

            IReadOnlyList<PlayerItemManager.AcquiredPlayerItem> acquiredItems = itemManager.AcquiredItems;
            for (int i = 0; i < acquiredItems.Count; i++)
            {
                string itemId = acquiredItems[i].ItemId;
                if (!string.IsNullOrWhiteSpace(itemId))
                {
                    acquiredItemIds.Add(itemId);
                }
            }

            RefreshItemRuntimeFlags();
        }

        public void EnsureActiveForInventoryChange()
        {
            if (!enabled)
            {
                enabled = true;
                return;
            }

            RebuildItemCache();
        }

        private bool HasCachedItem(string itemId)
        {
            return acquiredItemIds.Contains(itemId);
        }

        private void RefreshItemRuntimeFlags()
        {
            hasAnyCachedItems = acquiredItemIds.Count > 0;
            hasPersistentStatItems =
                HasCachedItem(ForbiddenGrowthId)
                || HasCachedItem(OverclockNerveId)
                || HasCachedItem(ImperfectRegenerationId)
                || HasCachedItem(ExoskeletonId);

            hasPlateletMembraneItem = HasCachedItem(PlateletMembraneId);
            hasRecoveryFactorItem = HasCachedItem(RecoveryFactorId);
            hasBioBarrierItem = HasCachedItem(BioBarrierId);
            hasSplitRegenerationItem = HasCachedItem(SplitRegenerationId);

            hasBloodContractItem = HasCachedItem(BloodContractId);
            hasSeveranceReflexItem = HasCachedItem(SeveranceReflexId);
            hasRampageBloodFlowItem = HasCachedItem(RampageBloodFlowId);
            hasDecayOrganItem = HasCachedItem(DecayOrganId);
            hasRuptureMuscleItem = HasCachedItem(RuptureMuscleId);
            hasImperfectRegenItem = HasCachedItem(ImperfectRegenerationId);
            hasOrganTentacleItem = HasCachedItem(OrganTentacleId);

            hasSporeColonyItem = HasCachedItem(SporeColonyId);
            hasBloodDroneItem = HasCachedItem(BloodDroneId);
            hasGuardianOrganItem = HasCachedItem(GuardianOrganId);
            hasTentacleColonyItem = HasCachedItem(TentacleColonyId);

            hasMacrophageItem = HasCachedItem(MacrophageId);
            hasGluttonousOrganItem = HasCachedItem(GluttonousOrganId);

            hasFocusedNerveItem = HasCachedItem(FocusedNerveId);
            hasBerserkCellItem = HasCachedItem(BerserkCellId);

            hasGrotesqueGrowthItem = HasCachedItem(GrotesqueGrowthId);
            hasMutationRampageItem = HasCachedItem(MutationRampageId);
            hasFrenzyHormoneItem = HasCachedItem(FrenzyHormoneId);

            hasOverheatedOrganItem = HasCachedItem(OverheatedOrganId);
            hasBioResonanceItem = HasCachedItem(BioResonanceId);
            hasUnstableCoreItem = HasCachedItem(UnstableCoreId);

            RefreshDerivedUpdateFlags();
            SyncPersistentStatModifiers();
            ResetInactiveItemRuntimeState();
            RefreshDerivedUpdateFlags();
            SleepIfInventoryEmpty();
        }

        private void RefreshDerivedUpdateFlags()
        {
            hasDefensiveUpdateItems =
                hasPlateletMembraneItem
                || hasRecoveryFactorItem
                || hasBioBarrierItem
                || hasSplitRegenerationItem;
            hasMovementTrackingItems = hasRampageBloodFlowItem || hasBioBarrierItem;
            hasBioCompanionUpdateItems =
                hasSporeColonyItem
                || hasBloodDroneItem
                || hasGuardianOrganItem
                || hasTentacleColonyItem
                || activeBioSummons.Count > 0;
            hasKillChainBuffItems =
                hasMacrophageItem
                || hasGluttonousOrganItem
                || macrophageStacks > 0
                || gluttonousOrganStacks > 0;
            hasBossFightUpdateItems = hasFocusedNerveItem || hasBerserkCellItem || berserkCellModifierApplied;
            hasMutationChaosItems =
                hasGrotesqueGrowthItem
                || hasMutationRampageItem
                || hasFrenzyHormoneItem
                || grotesqueGrowthModifierApplied
                || mutationRampageModifierApplied
                || frenzyHormoneModifierApplied;
            hasDecayStateItems = hasOverheatedOrganItem || hasBioResonanceItem || resonanceStatesByEnemyId.Count > 0 || overheatStacks > 0;
        }

        private void ResetInactiveItemRuntimeState()
        {
            float now = Time.time;

            if (!hasPlateletMembraneItem)
            {
                plateletMembraneCurrentShield = 0f;
                plateletMembraneNextReadyTime = now + Mathf.Max(0.1f, plateletMembraneInterval);
                ClearPlateletMembraneOutline();
            }

            if (!hasRecoveryFactorItem)
            {
                recoveryFactorNextHealTime = now + Mathf.Max(0.1f, recoveryFactorInterval);
            }

            if (!hasBioBarrierItem)
            {
                bioBarrierIdleTime = 0f;
            }

            if (!hasSplitRegenerationItem)
            {
                splitRegenerationUsed = false;
            }

            if (!hasBloodContractItem)
            {
                bloodContractKillProgress = 0;
            }

            if (!hasSeveranceReflexItem)
            {
                severanceReflexBuffUntil = float.NegativeInfinity;
            }

            if (!hasMovementTrackingItems)
            {
                previousPosition = GetMovementAnchorPosition();
                isMovingByPosition = false;
                movementIntensity = 0f;
            }

            if (!hasRampageBloodFlowItem)
            {
                rampageMoveAccumulatedTime = 0f;
                rampageIdleAccumulatedTime = 0f;
                rampageAttackBonus = 0;
            }

            if (!hasDecayOrganItem)
            {
                decayOrganStartTime = float.NegativeInfinity;
                decayOrganAttackBonus = 0;
            }

            if (!hasRuptureMuscleItem)
            {
                ruptureMuscleStacks = 0f;
                ruptureMuscleLastAttackTime = float.NegativeInfinity;
                ApplyRuptureMovePenalty(0f);
            }

            if (!hasImperfectRegenItem)
            {
                imperfectRegenPendingHeal = 0f;
                imperfectRegenHealReadyTime = float.PositiveInfinity;
                imperfectRegenCooldownUntil = float.NegativeInfinity;
            }

            if (!hasSporeColonyItem)
            {
                nextSporeSpawnTime = now + Mathf.Max(0.1f, sporeSpawnInterval);
            }

            if (!hasBloodDroneItem)
            {
                ReleaseBloodDrone();
            }

            if (!hasGuardianOrganItem)
            {
                ReleaseGuardianOrgan();
            }

            if (!hasTentacleColonyItem)
            {
                tentacleNextBindTime = now + Mathf.Max(0.1f, tentacleBindInterval);
            }

            if (!hasMacrophageItem && macrophageStacks > 0)
            {
                macrophageStacks = 0;
                macrophageExpireTime = float.NegativeInfinity;
                ClearKillChainStatBuff(macrophageModifierSource, ref macrophageAppliedAttackBonus);
            }

            if (!hasGluttonousOrganItem && gluttonousOrganStacks > 0)
            {
                gluttonousOrganStacks = 0;
                gluttonousOrganExpireTime = float.NegativeInfinity;
                ClearKillChainStatBuff(gluttonousOrganModifierSource, ref gluttonousOrganAppliedMoveBonus);
            }

            if (!hasFocusedNerveItem)
            {
                focusedNerveEnemyCount = int.MaxValue;
                focusedNerveNextScanTime = 0f;
            }

            if (!hasBerserkCellItem)
            {
                ClearBerserkCellBuff();
                berserkCellLastTriggeredBossId = 0;
            }

            if (!hasGrotesqueGrowthItem)
            {
                ClearGrotesqueGrowthState();
                grotesqueGrowthNextRollTime = 0f;
            }

            if (!hasMutationRampageItem)
            {
                ClearMutationRampageState();
                mutationRampageNextRollTime = 0f;
            }

            if (!hasFrenzyHormoneItem)
            {
                ClearFrenzyHormoneState();
                frenzyHormoneCooldownUntil = float.NegativeInfinity;
            }

            if (!hasOverheatedOrganItem)
            {
                overheatStacks = 0;
                overheatLastAttackTime = float.NegativeInfinity;
                overheatNextDecayTime = float.PositiveInfinity;
            }

            if (!hasBioResonanceItem)
            {
                resonanceStatesByEnemyId.Clear();
            }

            if (!hasUnstableCoreItem)
            {
                unstableCoreCurrentMultiplier = 1f;
                unstableCoreInitialized = false;
                unstableCoreNextRerollTime = 0f;
                if (unstableCoreOverlay != null)
                {
                    unstableCoreOverlay.enabled = false;
                }
            }
        }

        private void SleepIfInventoryEmpty()
        {
            if (hasAnyCachedItems || !enabled)
            {
                return;
            }

            disablingForEmptyInventory = true;
            enabled = false;
        }

        private void UpdateBioCompanionItems()
        {
            CleanupBioSummonList();
            UpdateSporeColony();
            UpdateBloodDrone();
            UpdateGuardianOrgan();
            UpdateTentacleColony();
        }

        private void TriggerElectricNeuralNetwork(EnemyController defeatedEnemy)
        {
            if (!HasElectricNeuralNetwork || defeatedEnemy == null)
            {
                return;
            }

            if (Time.time < electricChainNextReadyTime)
            {
                return;
            }

            tempEnemyTargets.Clear();
            Vector3 chainOrigin = defeatedEnemy.transform.position;
            Vector3 previousPosition = chainOrigin;
            int maxHits = Mathf.Max(1, electricChainMaxHits);
            float radius = Mathf.Max(0.1f, electricChainRadius);
            float damage = Mathf.Max(0f, electricChainDamage);
            if (damage <= 0f)
            {
                return;
            }

            for (int i = 0; i < maxHits; i++)
            {
                EnemyController target = FindClosestEnemy(previousPosition, radius, tempEnemyTargets);
                if (target == null)
                {
                    break;
                }

                tempEnemyTargets.Add(target);
                Vector3 targetPosition = target.transform.position;
                SpawnElectricChainVisual(previousPosition, targetPosition);
                target.TakeDamage(damage);
                previousPosition = targetPosition;
            }

            if (tempEnemyTargets.Count > 0)
            {
                electricChainNextReadyTime = Time.time + Mathf.Max(0f, electricChainCooldown);
            }
        }

        private void TriggerInfectionTransference(EnemyController defeatedEnemy)
        {
            if (!HasInfectionTransference || defeatedEnemy == null)
            {
                return;
            }

            if (Time.time < infectionTransferNextReadyTime)
            {
                return;
            }

            tempEnemyTargets.Clear();
            int maxTargets = Mathf.Max(1, infectionTransferMaxTargets);
            Vector3 center = defeatedEnemy.transform.position;
            for (int i = 0; i < maxTargets; i++)
            {
                EnemyController target = FindClosestEnemy(center, Mathf.Max(0.1f, infectionTransferRadius), tempEnemyTargets);
                if (target == null)
                {
                    break;
                }

                tempEnemyTargets.Add(target);
            }

            for (int i = 0; i < tempEnemyTargets.Count; i++)
            {
                EnemyController enemy = tempEnemyTargets[i];
                if (!IsEnemyTargetable(enemy))
                {
                    continue;
                }

                EnemyStatusEffectController status = EnsureStatusController(enemy);
                status?.ApplyPoison(
                    Mathf.Max(0.1f, infectionTransferDuration),
                    Mathf.Max(0.1f, infectionTransferTickInterval),
                    Mathf.Max(0f, infectionTransferTickDamage));
                SpawnInfectionTransferVisual(center, enemy.transform.position);
            }

            if (tempEnemyTargets.Count > 0)
            {
                infectionTransferNextReadyTime = Time.time + Mathf.Max(0f, infectionTransferCooldown);
            }
        }

        private void TriggerMacrophageBuff()
        {
            if (!HasMacrophage || playerStats == null || playerStats.RuntimeStats == null)
            {
                return;
            }

            macrophageStacks = Mathf.Clamp(macrophageStacks + 1, 1, Mathf.Max(1, macrophageMaxStacks));
            macrophageExpireTime = Time.time + Mathf.Max(0.1f, macrophageDuration);
            ApplyKillChainStatBuff(
                macrophageModifierSource,
                CharacterStatType.AttackPower,
                ref macrophageAppliedAttackBonus,
                Mathf.Max(0f, macrophageAttackBonusPerStack) * macrophageStacks);
        }

        private void TriggerGluttonousOrganBuff()
        {
            if (!HasGluttonousOrgan || playerStats == null || playerStats.RuntimeStats == null)
            {
                return;
            }

            gluttonousOrganStacks = Mathf.Clamp(gluttonousOrganStacks + 1, 1, Mathf.Max(1, gluttonousOrganMaxStacks));
            gluttonousOrganExpireTime = Time.time + Mathf.Max(0.1f, gluttonousOrganDuration);
            ApplyKillChainStatBuff(
                gluttonousOrganModifierSource,
                CharacterStatType.MoveSpeed,
                ref gluttonousOrganAppliedMoveBonus,
                Mathf.Max(0f, gluttonousOrganMoveBonusPerStack) * gluttonousOrganStacks);
        }

        private void TriggerParasiticBomb()
        {
            if (applyingParasiticBombDamage || !HasParasiticBomb || playerStats == null || playerStats.IsDead)
            {
                return;
            }

            if (UnityEngine.Random.value > Mathf.Clamp01(parasiticBombChance))
            {
                return;
            }

            Vector3 center = playerController != null ? playerController.transform.position : transform.position;
            float radius = Mathf.Max(0.1f, parasiticBombRadius);
            float damage = Mathf.Max(0.1f, playerStats.AttackPower * Mathf.Max(0f, parasiticBombDamageMultiplier));
            int hitCount = Physics.OverlapSphereNonAlloc(
                center,
                radius,
                parasiticBombHitBuffer,
                ~0,
                QueryTriggerInteraction.Collide);

            tempParasiticBombEnemies.Clear();
            applyingParasiticBombDamage = true;
            try
            {
                for (int i = 0; i < hitCount; i++)
                {
                    Collider hitCollider = parasiticBombHitBuffer[i];
                    parasiticBombHitBuffer[i] = null;
                    EnemyController enemy = hitCollider != null ? hitCollider.GetComponentInParent<EnemyController>() : null;
                    if (enemy == null || enemy.IsDead || !tempParasiticBombEnemies.Add(enemy))
                    {
                        continue;
                    }

                    enemy.TakeDamage(damage);
                }
            }
            finally
            {
                applyingParasiticBombDamage = false;
                tempParasiticBombEnemies.Clear();
            }

            SpawnParasiticBombVisual(center, radius);
        }

        private void UpdateKillChainBuffs()
        {
            if ((!hasMacrophageItem || Time.time >= macrophageExpireTime) && macrophageStacks > 0)
            {
                macrophageStacks = 0;
                macrophageExpireTime = float.NegativeInfinity;
                ClearKillChainStatBuff(macrophageModifierSource, ref macrophageAppliedAttackBonus);
            }

            if ((!hasGluttonousOrganItem || Time.time >= gluttonousOrganExpireTime) && gluttonousOrganStacks > 0)
            {
                gluttonousOrganStacks = 0;
                gluttonousOrganExpireTime = float.NegativeInfinity;
                ClearKillChainStatBuff(gluttonousOrganModifierSource, ref gluttonousOrganAppliedMoveBonus);
            }
        }

        private void UpdateBossFightItemStates()
        {
            UpdateFocusedNerveScan();
            UpdateBerserkCellState();
        }

        private void UpdateMutationChaosItems()
        {
            UpdateGrotesqueGrowthState();
            UpdateMutationRampageState();
            UpdateFrenzyHormoneState();
        }

        private void UpdateGrotesqueGrowthState()
        {
            if (!grotesqueGrowthScaleCached)
            {
                grotesqueGrowthBaseScale = transform.localScale;
                grotesqueGrowthScaleCached = true;
            }

            if (!hasGrotesqueGrowthItem)
            {
                ClearGrotesqueGrowthState();
                grotesqueGrowthNextRollTime = 0f;
                return;
            }

            if (Time.time < grotesqueGrowthNextRollTime)
            {
                return;
            }

            grotesqueGrowthNextRollTime = Time.time + Mathf.Max(0.1f, grotesqueGrowthInterval);
            bool rollSmall = UnityEngine.Random.value < 0.5f;
            playerStats?.RuntimeStats?.RemoveModifiersFromSource(grotesqueGrowthModifierSource);
            grotesqueGrowthModifierApplied = true;

            if (rollSmall)
            {
                transform.localScale = grotesqueGrowthBaseScale * Mathf.Max(0.1f, grotesqueGrowthSmallScale);
                playerStats?.RuntimeStats?.AddModifier(
                    CharacterStatType.MoveSpeed,
                    Mathf.Max(0f, grotesqueGrowthSmallMoveBonus),
                    CharacterStatModifierMode.Flat,
                    grotesqueGrowthModifierSource);
                return;
            }

            transform.localScale = grotesqueGrowthBaseScale * Mathf.Max(0.1f, grotesqueGrowthLargeScale);
            playerStats?.RuntimeStats?.AddModifier(
                CharacterStatType.AttackPower,
                Mathf.Max(0f, grotesqueGrowthLargeAttackBonus),
                CharacterStatModifierMode.Flat,
                grotesqueGrowthModifierSource);
            playerStats?.RuntimeStats?.AddModifier(
                CharacterStatType.MoveSpeed,
                -Mathf.Max(0f, grotesqueGrowthLargeMovePenalty),
                CharacterStatModifierMode.Flat,
                grotesqueGrowthModifierSource);
        }

        private void ClearGrotesqueGrowthState()
        {
            if (grotesqueGrowthModifierApplied && playerStats != null && playerStats.RuntimeStats != null)
            {
                playerStats.RuntimeStats.RemoveModifiersFromSource(grotesqueGrowthModifierSource);
            }

            if (grotesqueGrowthScaleCached)
            {
                transform.localScale = grotesqueGrowthBaseScale;
            }

            grotesqueGrowthModifierApplied = false;
        }

        private void UpdateMutationRampageState()
        {
            if (!hasMutationRampageItem)
            {
                ClearMutationRampageState();
                mutationRampageNextRollTime = 0f;
                return;
            }

            if (mutationRampageModifierApplied && Time.time >= mutationRampageExpireTime)
            {
                ClearMutationRampageState();
            }

            if (Time.time < mutationRampageNextRollTime)
            {
                return;
            }

            mutationRampageNextRollTime = Time.time + Mathf.Max(0.1f, mutationRampageInterval);
            mutationRampageExpireTime = Time.time + Mathf.Max(0.1f, mutationRampageDuration);
            ApplyRandomMutationRampageModifier();
        }

        private void ApplyRandomMutationRampageModifier()
        {
            if (playerStats == null || playerStats.RuntimeStats == null)
            {
                return;
            }

            playerStats.RuntimeStats.RemoveModifiersFromSource(mutationRampageModifierSource);
            bool isBuff = UnityEngine.Random.value <= Mathf.Clamp01(mutationRampageBuffChance);
            int statRoll = UnityEngine.Random.Range(0, 3);
            CharacterStatType statType;
            float value;

            if (statRoll == 0)
            {
                statType = CharacterStatType.AttackPower;
                value = isBuff ? Mathf.Max(0f, mutationRampageAttackBuff) : -Mathf.Max(0f, mutationRampageAttackDebuff);
            }
            else if (statRoll == 1)
            {
                statType = CharacterStatType.MoveSpeed;
                value = isBuff ? Mathf.Max(0f, mutationRampageMoveBuff) : -Mathf.Max(0f, mutationRampageMoveDebuff);
            }
            else
            {
                statType = CharacterStatType.AttackSpeed;
                value = isBuff ? Mathf.Max(0f, mutationRampageAttackSpeedBuff) : -Mathf.Max(0f, mutationRampageAttackSpeedDebuff);
            }

            playerStats.RuntimeStats.AddModifier(statType, value, CharacterStatModifierMode.Flat, mutationRampageModifierSource);
            mutationRampageModifierApplied = true;
        }

        private void ClearMutationRampageState()
        {
            if (mutationRampageModifierApplied && playerStats != null && playerStats.RuntimeStats != null)
            {
                playerStats.RuntimeStats.RemoveModifiersFromSource(mutationRampageModifierSource);
            }

            mutationRampageModifierApplied = false;
            mutationRampageExpireTime = float.NegativeInfinity;
        }

        private void UpdateFrenzyHormoneState()
        {
            if (!hasFrenzyHormoneItem)
            {
                ClearFrenzyHormoneState();
                frenzyHormoneCooldownUntil = float.NegativeInfinity;
                return;
            }

            if (frenzyHormoneModifierApplied && Time.time >= frenzyHormoneExpireTime)
            {
                ClearFrenzyHormoneState();
            }
        }

        private void ApplyFrenzyHormoneBuff()
        {
            if (!HasFrenzyHormone || playerStats == null || playerStats.RuntimeStats == null)
            {
                return;
            }

            if (Time.time < frenzyHormoneCooldownUntil)
            {
                return;
            }

            playerStats.RuntimeStats.RemoveModifiersFromSource(frenzyHormoneModifierSource);
            int statRoll = UnityEngine.Random.Range(0, 3);
            CharacterStatType statType = statRoll == 0
                ? CharacterStatType.AttackPower
                : statRoll == 1 ? CharacterStatType.MoveSpeed : CharacterStatType.AttackSpeed;
            float minBonus = Mathf.Max(0f, frenzyHormoneMinBonus);
            float maxBonus = Mathf.Max(minBonus, frenzyHormoneMaxBonus);
            float bonus = UnityEngine.Random.Range(minBonus, maxBonus);
            playerStats.RuntimeStats.AddModifier(statType, bonus, CharacterStatModifierMode.Flat, frenzyHormoneModifierSource);
            frenzyHormoneModifierApplied = true;
            frenzyHormoneExpireTime = Time.time + Mathf.Max(0.1f, frenzyHormoneDuration);
            frenzyHormoneCooldownUntil = Time.time + Mathf.Max(0f, frenzyHormoneCooldown);
        }

        private void ClearFrenzyHormoneState()
        {
            if (frenzyHormoneModifierApplied && playerStats != null && playerStats.RuntimeStats != null)
            {
                playerStats.RuntimeStats.RemoveModifiersFromSource(frenzyHormoneModifierSource);
            }

            frenzyHormoneModifierApplied = false;
            frenzyHormoneExpireTime = float.NegativeInfinity;
        }

        private void UpdateFocusedNerveScan()
        {
            if (!hasFocusedNerveItem)
            {
                focusedNerveEnemyCount = int.MaxValue;
                focusedNerveNextScanTime = 0f;
                return;
            }

            if (Time.time < focusedNerveNextScanTime)
            {
                return;
            }

            focusedNerveNextScanTime = Time.time + 0.2f;
            focusedNerveEnemyCount = CountEnemiesWithinRadius(transform.position, Mathf.Max(0.1f, focusedNerveRadius));
        }

        private void UpdateBerserkCellState()
        {
            if (!hasBerserkCellItem)
            {
                ClearBerserkCellBuff();
                berserkCellLastTriggeredBossId = 0;
                return;
            }

            EnemyController boss = FindNearestBossLikeEnemy(Mathf.Max(0.1f, berserkCellBossDetectionRadius));
            if (boss != null)
            {
                int bossId = boss.GetInstanceID();
                berserkCellActiveBossId = bossId;
                if (berserkCellLastTriggeredBossId != bossId)
                {
                    berserkCellLastTriggeredBossId = bossId;
                    ActivateBerserkCellBuff();
                }
            }
            else
            {
                berserkCellActiveBossId = 0;
            }

            if (berserkCellModifierApplied && Time.time >= berserkCellExpireTime)
            {
                ClearBerserkCellBuff();
            }
        }

        private float GetFocusedNerveAttackBonus()
        {
            if (focusedNerveEnemyCount <= 1)
            {
                return Mathf.Max(0f, focusedNerveHighAttackBonus);
            }

            if (focusedNerveEnemyCount <= 3)
            {
                return Mathf.Max(0f, focusedNerveLowAttackBonus);
            }

            return 0f;
        }

        private void ActivateBerserkCellBuff()
        {
            if (playerStats == null || playerStats.RuntimeStats == null)
            {
                return;
            }

            playerStats.RuntimeStats.RemoveModifiersFromSource(berserkCellModifierSource);
            playerStats.RuntimeStats.AddModifier(
                CharacterStatType.AttackPower,
                Mathf.Max(0f, berserkCellAttackBonus),
                CharacterStatModifierMode.Flat,
                berserkCellModifierSource);
            playerStats.RuntimeStats.AddModifier(
                CharacterStatType.MoveSpeed,
                Mathf.Max(0f, berserkCellMoveBonus),
                CharacterStatModifierMode.Flat,
                berserkCellModifierSource);
            berserkCellExpireTime = Time.time + Mathf.Max(0.1f, berserkCellDuration);
            berserkCellModifierApplied = true;
        }

        private void ClearBerserkCellBuff()
        {
            if (playerStats != null && playerStats.RuntimeStats != null)
            {
                playerStats.RuntimeStats.RemoveModifiersFromSource(berserkCellModifierSource);
            }

            berserkCellExpireTime = float.NegativeInfinity;
            berserkCellModifierApplied = false;
        }

        private bool IsBerserkCellActive()
        {
            return berserkCellModifierApplied && Time.time < berserkCellExpireTime;
        }

        private void ApplyKillChainStatBuff(object source, CharacterStatType statType, ref float appliedValue, float value)
        {
            if (playerStats == null || playerStats.RuntimeStats == null)
            {
                appliedValue = value;
                return;
            }

            playerStats.RuntimeStats.RemoveModifiersFromSource(source);
            appliedValue = Mathf.Max(0f, value);
            if (appliedValue <= 0f)
            {
                return;
            }

            playerStats.RuntimeStats.AddModifier(
                statType,
                appliedValue,
                CharacterStatModifierMode.Flat,
                source);
        }

        private void ClearKillChainStatBuff(object source, ref float appliedValue)
        {
            if (playerStats != null && playerStats.RuntimeStats != null)
            {
                playerStats.RuntimeStats.RemoveModifiersFromSource(source);
            }

            appliedValue = 0f;
        }

        private void SpawnElectricChainVisual(Vector3 start, Vector3 end)
        {
            SpawnLineVisual(
                "ElectricChainVisual",
                start + Vector3.up * 0.45f,
                end + Vector3.up * 0.45f,
                new Color(0.38f, 0.86f, 1f, 0.95f),
                new Color(0.9f, 1f, 1f, 0.45f),
                0.11f,
                0.04f,
                0.18f);
        }

        private void SpawnInfectionTransferVisual(Vector3 start, Vector3 end)
        {
            SpawnLineVisual(
                "InfectionTransferVisual",
                start + Vector3.up * 0.35f,
                end + Vector3.up * 0.35f,
                new Color(0.34f, 1f, 0.28f, 0.72f),
                new Color(0.34f, 1f, 0.28f, 0.18f),
                0.07f,
                0.025f,
                0.25f);
        }

        private void SpawnParasiticBombVisual(Vector3 center, float radius)
        {
            SpawnPooledCircleVisual(
                ParasiticBombVisualPoolName,
                "ParasiticBombVisual",
                new Vector3(center.x, center.y + 1.35f, center.z),
                Mathf.Max(0.2f, radius * 2f),
                new Color(0.88f, 0.12f, 0.68f, 0.55f),
                5200,
                0.22f);
        }

        private void SpawnLineVisual(
            string objectName,
            Vector3 start,
            Vector3 end,
            Color startColor,
            Color endColor,
            float startWidth,
            float endWidth,
            float duration)
        {
            GameObject lineObject = RuntimePool.Acquire(LineVisualPoolName, CreateLineVisualFunc);
            if (lineObject == null)
            {
                return;
            }

            lineObject.name = objectName;
            LineRenderer line = GetOrAddLineRenderer(lineObject);
            if (line == null)
            {
                RuntimePool.Release(lineObject);
                return;
            }

            line.useWorldSpace = true;
            line.positionCount = 2;
            line.enabled = true;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startWidth = Mathf.Max(0.01f, startWidth);
            line.endWidth = Mathf.Max(0.01f, endWidth);
            line.sharedMaterial = GetRuntimeLineMaterial();
            line.startColor = startColor;
            line.endColor = endColor;
            line.sortingOrder = 5230;

            RuntimePool.EnsureAutoReturn(lineObject)?.Schedule(Mathf.Max(0.02f, duration));
        }

        private void UpdateSporeColony()
        {
            if (!hasSporeColonyItem)
            {
                nextSporeSpawnTime = Time.time + Mathf.Max(0.1f, sporeSpawnInterval);
                return;
            }

            if (Time.time < nextSporeSpawnTime)
            {
                return;
            }

            nextSporeSpawnTime = Time.time + Mathf.Max(0.1f, sporeSpawnInterval);
            SpawnSporeSummon();
        }

        private void UpdateBloodDrone()
        {
            if (!hasBloodDroneItem)
            {
                if (bloodDrone != null)
                {
                    ReleaseBloodDrone();
                }

                return;
            }

            if (bloodDrone != null)
            {
                return;
            }

            GameObject droneObject = RuntimePool.Acquire(BloodDronePoolName, CreateBloodDroneFunc);
            if (droneObject == null)
            {
                return;
            }

            droneObject.name = "BloodDrone";
            bloodDrone = droneObject.GetComponent<PlayerBloodDrone>();
            bloodDrone.Initialize(this);
        }

        private void ReleaseBloodDrone()
        {
            if (bloodDrone == null)
            {
                return;
            }

            bloodDrone.ClearOwner();
            RuntimePool.Release(bloodDrone.gameObject);
            bloodDrone = null;
        }

        private void UpdateGuardianOrgan()
        {
            if (!hasGuardianOrganItem)
            {
                if (guardianOrgan != null)
                {
                    ReleaseGuardianOrgan();
                }

                return;
            }

            if (guardianOrgan != null)
            {
                return;
            }

            GameObject guardianObject = RuntimePool.Acquire(GuardianOrganPoolName, CreateGuardianOrganFunc);
            if (guardianObject == null)
            {
                return;
            }

            guardianObject.name = "GuardianOrgan";
            guardianOrgan = guardianObject.GetComponent<PlayerGuardianOrgan>();
            guardianOrgan.Initialize(this);
        }

        private void ReleaseGuardianOrgan()
        {
            if (guardianOrgan == null)
            {
                return;
            }

            guardianOrgan.ClearOwner();
            RuntimePool.Release(guardianOrgan.gameObject);
            guardianOrgan = null;
        }

        private void UpdateTentacleColony()
        {
            if (!hasTentacleColonyItem)
            {
                tentacleNextBindTime = Time.time + Mathf.Max(0.1f, tentacleBindInterval);
                return;
            }

            if (Time.time < tentacleNextBindTime)
            {
                return;
            }

            tentacleNextBindTime = Time.time + Mathf.Max(0.1f, tentacleBindInterval);
            ApplyTentacleBind();
        }

        private void TrySpawnInfectedHost(EnemyController defeatedEnemy)
        {
            if (!HasInfectedHost || defeatedEnemy == null || !CanInfectEnemy(defeatedEnemy))
            {
                return;
            }

            if (CountBioSummons(PlayerBioSummon.SummonKind.InfectedHost) >= Mathf.Max(1, infectedHostMaxAllies))
            {
                return;
            }

            if (UnityEngine.Random.value > Mathf.Clamp01(infectedHostChance))
            {
                return;
            }

            Sprite enemySprite = FindEnemySprite(defeatedEnemy);
            Sprite[] attackSprites = FindEnemyAttackSprites(defeatedEnemy);
            Vector3 visualScale = FindEnemyVisualScale(defeatedEnemy, Vector3.one * 0.9f);
            float damage = GetCompanionBaseDamage() * Mathf.Max(0.05f, infectedHostDamageMultiplier);
            SpawnBioSummon(
                PlayerBioSummon.SummonKind.InfectedHost,
                defeatedEnemy.transform.position,
                enemySprite,
                attackSprites,
                visualScale,
                new Color(1f, 0.28f, 0.22f, 0.9f),
                damage,
                Mathf.Max(0.1f, infectedHostLifetime),
                Mathf.Max(0.1f, infectedHostSearchRadius),
                Mathf.Max(0.1f, infectedHostAttackRadius),
                Mathf.Max(0.1f, infectedHostAttackInterval),
                false);
        }

        private void SpawnSporeSummon()
        {
            Vector2 offset2D = UnityEngine.Random.insideUnitCircle;
            if (offset2D.sqrMagnitude <= 0.0001f)
            {
                offset2D = Vector2.right;
            }

            Vector3 desiredPosition = transform.position + new Vector3(offset2D.x, 0f, offset2D.y).normalized * 1.2f;
            Vector3 spawnPosition = ResolveGroundSpawnPosition(desiredPosition, 0.15f);
            float damage = GetCompanionBaseDamage() * Mathf.Max(0.05f, sporeDamageMultiplier);
            SpawnBioSummon(
                PlayerBioSummon.SummonKind.Spore,
                spawnPosition,
                TextureSpriteCache.GetCircleSprite(),
                null,
                Vector3.one * 0.65f,
                new Color(0.52f, 1f, 0.45f, 0.9f),
                damage,
                Mathf.Max(0.1f, sporeLifetime),
                Mathf.Max(0.1f, sporeSearchRadius),
                Mathf.Max(0.1f, sporeBurstRadius),
                0.1f,
                true);
        }

        private void SpawnBioSummon(
            PlayerBioSummon.SummonKind kind,
            Vector3 position,
            Sprite sprite,
            Sprite[] attackSprites,
            Vector3 visualScale,
            Color color,
            float damage,
            float lifetime,
            float searchRadius,
            float attackRadius,
            float attackInterval,
            bool destroyOnAttack)
        {
            GameObject summonObject = new GameObject(kind == PlayerBioSummon.SummonKind.Spore ? "SporeSummon" : "InfectedHostAlly");
            summonObject.transform.position = position;
            PlayerBioSummon summon = summonObject.AddComponent<PlayerBioSummon>();
            summon.Initialize(this, kind, sprite, attackSprites, visualScale, color, damage, lifetime, searchRadius, attackRadius, attackInterval, destroyOnAttack);
            activeBioSummons.Add(summon);
        }

        private void ApplyTentacleBind()
        {
            tempEnemyTargets.Clear();
            int targetCount = Mathf.Max(1, tentacleMaxTargets);
            Vector3 center = transform.position;

            for (int i = 0; i < targetCount; i++)
            {
                EnemyController enemy = FindClosestEnemy(center, Mathf.Max(0.1f, tentacleBindRadius), tempEnemyTargets);
                if (enemy == null)
                {
                    break;
                }

                tempEnemyTargets.Add(enemy);
            }

            float damage = GetCompanionBaseDamage() * Mathf.Max(0f, tentacleDamageMultiplier);
            for (int i = 0; i < tempEnemyTargets.Count; i++)
            {
                EnemyController enemy = tempEnemyTargets[i];
                if (!IsEnemyTargetable(enemy))
                {
                    continue;
                }

                EnemyStatusEffectController status = EnsureStatusController(enemy);
                status?.ApplyMoveSpeedSlow(Mathf.Clamp01(tentacleSlowRatio), Mathf.Max(0.1f, tentacleBindDuration));
                if (damage > 0f)
                {
                    enemy.TakeDamage(damage);
                }

                SpawnTentacleBindVisual(enemy.transform.position, Mathf.Max(0.1f, tentacleBindDuration));
            }
        }

        private void SpawnTentacleBindVisual(Vector3 targetPosition, float duration)
        {
            Vector3 anchorPosition = GetPlayerVisualCenter() + Vector3.up * 0.15f;

            SpawnPooledCircleVisual(
                TentacleAnchorVisualPoolName,
                "TentacleColonyAnchor",
                anchorPosition,
                0.34f,
                new Color(0.22f, 0.58f, 1f, 0.95f),
                5220,
                duration);

            SpawnLineVisual(
                "TentacleColonyLine",
                anchorPosition,
                targetPosition + Vector3.up * 0.35f,
                new Color(0.22f, 0.58f, 1f, 0.9f),
                new Color(0.22f, 0.58f, 1f, 0.2f),
                0.08f,
                0.035f,
                duration);

        }

        private void CleanupBioSummonList()
        {
            for (int i = activeBioSummons.Count - 1; i >= 0; i--)
            {
                if (activeBioSummons[i] == null)
                {
                    activeBioSummons.RemoveAt(i);
                }
            }
        }

        private int CountBioSummons(PlayerBioSummon.SummonKind kind)
        {
            CleanupBioSummonList();
            int count = 0;
            for (int i = 0; i < activeBioSummons.Count; i++)
            {
                if (activeBioSummons[i] != null && activeBioSummons[i].Kind == kind)
                {
                    count++;
                }
            }

            return count;
        }

        private void ClearBioCompanions()
        {
            for (int i = activeBioSummons.Count - 1; i >= 0; i--)
            {
                if (activeBioSummons[i] != null)
                {
                    Destroy(activeBioSummons[i].gameObject);
                }
            }

            activeBioSummons.Clear();
            if (bloodDrone != null)
            {
                ReleaseBloodDrone();
            }

            if (guardianOrgan != null)
            {
                ReleaseGuardianOrgan();
            }
        }

        private bool CanInfectEnemy(EnemyController enemy)
        {
            if (enemy == null || enemy.IsElite)
            {
                return false;
            }

            return !IsBossLikeEnemy(enemy);
        }

        private float GetCompanionBaseDamage()
        {
            return playerStats != null ? Mathf.Max(0.1f, playerStats.AttackPower) : 1f;
        }

        private EnemyController FindClosestEnemy(Vector3 center, float radius, List<EnemyController> excluded = null)
        {
            var enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return null;
            }

            float radiusSqr = Mathf.Max(0.1f, radius);
            radiusSqr *= radiusSqr;
            float bestDistanceSqr = float.PositiveInfinity;
            EnemyController bestEnemy = null;

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (!IsEnemyTargetable(enemy) || (excluded != null && excluded.Contains(enemy)))
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - center;
                toEnemy.y = 0f;
                float distanceSqr = toEnemy.sqrMagnitude;
                if (distanceSqr > radiusSqr || distanceSqr >= bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                bestEnemy = enemy;
            }

            return bestEnemy;
        }

        private int CountEnemiesWithinRadius(Vector3 center, float radius)
        {
            var enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return 0;
            }

            float radiusSqr = Mathf.Max(0.1f, radius);
            radiusSqr *= radiusSqr;
            int count = 0;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (!IsEnemyTargetable(enemy))
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - center;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude <= radiusSqr)
                {
                    count++;
                }
            }

            return count;
        }

        private bool IsBossOrEliteWithinRadius(float radius)
        {
            var enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return false;
            }

            float radiusSqr = Mathf.Max(0.1f, radius);
            radiusSqr *= radiusSqr;
            Vector3 center = transform.position;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (!IsEnemyTargetable(enemy) || (!enemy.IsElite && !IsBossLikeEnemy(enemy)))
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - center;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude <= radiusSqr)
                {
                    return true;
                }
            }

            return false;
        }

        private static EnemyController FindNearestBossLikeEnemy(float radius)
        {
            var enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return null;
            }

            Vector3 center = PlayerController.Instance != null ? PlayerController.Instance.transform.position : Vector3.zero;
            float radiusSqr = Mathf.Max(0.1f, radius);
            radiusSqr *= radiusSqr;
            float bestDistanceSqr = float.PositiveInfinity;
            EnemyController bestEnemy = null;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (!IsEnemyTargetable(enemy) || !IsBossLikeEnemy(enemy))
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - center;
                toEnemy.y = 0f;
                float distanceSqr = toEnemy.sqrMagnitude;
                if (distanceSqr > radiusSqr || distanceSqr >= bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                bestEnemy = enemy;
            }

            return bestEnemy;
        }

        private static bool IsEnemyAboveHealthRatio(EnemyController enemy, float ratio)
        {
            CharacterStats stats = enemy != null ? enemy.Stats : null;
            if (stats == null || stats.MaxHealth <= 0f)
            {
                return false;
            }

            return stats.CurrentHealth / stats.MaxHealth >= ratio;
        }

        private static bool IsEnemyBelowHealthRatio(EnemyController enemy, float ratio)
        {
            CharacterStats stats = enemy != null ? enemy.Stats : null;
            if (stats == null || stats.MaxHealth <= 0f)
            {
                return false;
            }

            return stats.CurrentHealth / stats.MaxHealth <= ratio;
        }

        private static float GetEnemyCurrentHealth(EnemyController enemy)
        {
            CharacterStats stats = enemy != null ? enemy.Stats : null;
            return stats != null ? Mathf.Max(0f, stats.CurrentHealth) : 0f;
        }

        private static bool IsEnemyTargetable(EnemyController enemy)
        {
            return enemy != null && enemy.gameObject.activeInHierarchy && !enemy.IsDead;
        }

        private static bool IsBossLikeEnemy(EnemyController enemy)
        {
            if (enemy == null)
            {
                return false;
            }

            if (enemy.GetComponent<IntestineBossPattern>() != null)
            {
                return true;
            }

            return enemy.gameObject.name.EndsWith("_MidBoss", StringComparison.OrdinalIgnoreCase);
        }

        private static Sprite FindEnemySprite(EnemyController enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            SpriteRenderer[] renderers = enemy.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null || renderer.sprite == null || renderer.gameObject.name.Contains("Overlay"))
                {
                    continue;
                }

                return renderer.sprite;
            }

            return TextureSpriteCache.GetCircleSprite();
        }

        private static Sprite[] FindEnemyAttackSprites(EnemyController enemy)
        {
            EnemySpawnRuleConfig config = enemy != null ? enemy.Config : null;
            if (config == null)
            {
                return null;
            }

            if (config.attackSprites != null && config.attackSprites.Length > 0)
            {
                return config.attackSprites;
            }

            if (config.attackSpritesDown != null && config.attackSpritesDown.Length > 0)
            {
                return config.attackSpritesDown;
            }

            if (config.attackSpritesUp != null && config.attackSpritesUp.Length > 0)
            {
                return config.attackSpritesUp;
            }

            return null;
        }

        private static Material GetRuntimeLineMaterial()
        {
            if (runtimeLineMaterial != null)
            {
                return runtimeLineMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default");
            runtimeLineMaterial = new Material(shader != null ? shader : Shader.Find("Universal Render Pipeline/Unlit"))
            {
                name = "Runtime_TentacleColonyLine"
            };
            return runtimeLineMaterial;
        }

        private static void SpawnPooledCircleVisual(
            string poolName,
            string objectName,
            Vector3 position,
            float scale,
            Color color,
            int sortingOrder,
            float duration)
        {
            GameObject visualObject = RuntimePool.Acquire(poolName, CreateCircleVisualFunc);
            if (visualObject == null)
            {
                return;
            }

            visualObject.name = objectName;
            visualObject.transform.position = position;
            visualObject.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);

            SpriteRenderer renderer = GetOrAddSpriteRenderer(visualObject);
            if (renderer == null)
            {
                RuntimePool.Release(visualObject);
                return;
            }

            renderer.enabled = true;
            renderer.sprite = TextureSpriteCache.GetCircleSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            RuntimePool.EnsureAutoReturn(visualObject)?.Schedule(Mathf.Max(0.02f, duration));
        }

        private static GameObject CreateCircleVisualObject()
        {
            GameObject obj = new GameObject("PlayerItemCircleVisual");
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = TextureSpriteCache.GetCircleSprite();
            return obj;
        }

        private static GameObject CreateLineVisualObject()
        {
            GameObject obj = new GameObject("PlayerItemLineVisual");
            LineRenderer line = obj.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.sharedMaterial = GetRuntimeLineMaterial();
            return obj;
        }

        private static GameObject CreateBloodDroneObject()
        {
            GameObject obj = new GameObject("BloodDrone");
            obj.AddComponent<PlayerBloodDrone>();
            return obj;
        }

        private static GameObject CreateGuardianOrganObject()
        {
            GameObject obj = new GameObject("GuardianOrgan");
            obj.AddComponent<PlayerGuardianOrgan>();
            return obj;
        }

        private static GameObject CreateBloodDroneProjectileObject()
        {
            GameObject obj = new GameObject("BloodDroneProjectile");
            obj.AddComponent<PlayerBioProjectile>();
            return obj;
        }

        private static SpriteRenderer GetOrAddSpriteRenderer(GameObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = obj.AddComponent<SpriteRenderer>();
            }

            return renderer;
        }

        private static LineRenderer GetOrAddLineRenderer(GameObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            LineRenderer line = obj.GetComponent<LineRenderer>();
            if (line == null)
            {
                line = obj.AddComponent<LineRenderer>();
            }

            return line;
        }

        private static Vector3 FindEnemyVisualScale(EnemyController enemy, Vector3 fallback)
        {
            if (enemy == null)
            {
                return fallback;
            }

            SpriteRenderer[] renderers = enemy.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null || renderer.gameObject.name.Contains("Overlay"))
                {
                    continue;
                }

                return renderer.transform.lossyScale;
            }

            return fallback;
        }

        private Vector3 ResolveGroundSpawnPosition(Vector3 desiredPosition, float heightOffset)
        {
            BiomeManager biome = BiomeManager.Active;
            if (biome == null)
            {
                Vector3 fallback = desiredPosition;
                fallback.y = transform.position.y + heightOffset;
                return fallback;
            }

            Vector2Int desiredGrid = biome.WorldToGrid(desiredPosition);
            if (biome.IsWalkable(desiredGrid.x, desiredGrid.y))
            {
                return biome.GridToWorldWithHeight(desiredGrid.x, desiredGrid.y, heightOffset);
            }

            Vector2Int playerGrid = biome.WorldToGrid(transform.position);
            for (int radius = 1; radius <= 3; radius++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius)
                        {
                            continue;
                        }

                        int gridX = playerGrid.x + x;
                        int gridY = playerGrid.y + y;
                        if (biome.IsWalkable(gridX, gridY))
                        {
                            return biome.GridToWorldWithHeight(gridX, gridY, heightOffset);
                        }
                    }
                }
            }

            return biome.GridToWorldWithHeight(playerGrid.x, playerGrid.y, heightOffset);
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
            if (!hasRampageBloodFlowItem)
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
            if (!hasOrganTentacleItem || playerStats == null || playerStats.IsDead)
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
                TryApplyPostDamageExecutionInstinct(enemy, adjustedDamage);
                ApplyCommonOnHitEffects(enemy, adjustedDamage, enemy.transform.position);
                hitCount++;
            }
        }

        private void UpdateDecayStates()
        {
            float now = Time.time;
            if (hasOverheatedOrganItem)
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

            if (!hasBioResonanceItem)
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
            if (!hasDecayOrganItem)
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
            if (!hasRuptureMuscleItem)
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
            if (!hasImperfectRegenItem || playerStats == null || playerStats.IsDead)
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
            CharacterStatModifier modifier)
        {
            if (playerStats == null || playerStats.RuntimeStats == null)
            {
                return;
            }

            if (shouldApply && !appliedFlag)
            {
                playerStats.RuntimeStats.AddModifier(modifier);
                appliedFlag = true;
                return;
            }

            if (!shouldApply && appliedFlag)
            {
                playerStats.RuntimeStats.RemoveModifiersFromSource(source);
                appliedFlag = false;
            }
        }

        private void SetSimpleModifierState(
            bool shouldApply,
            ref bool appliedFlag,
            object source,
            CharacterStatModifier firstModifier,
            CharacterStatModifier secondModifier)
        {
            if (playerStats == null || playerStats.RuntimeStats == null)
            {
                return;
            }

            if (shouldApply && !appliedFlag)
            {
                playerStats.RuntimeStats.AddModifier(firstModifier);
                playerStats.RuntimeStats.AddModifier(secondModifier);
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
            playerStats.RuntimeStats.RemoveModifiersFromSource(macrophageModifierSource);
            playerStats.RuntimeStats.RemoveModifiersFromSource(gluttonousOrganModifierSource);
            playerStats.RuntimeStats.RemoveModifiersFromSource(berserkCellModifierSource);
            playerStats.RuntimeStats.RemoveModifiersFromSource(grotesqueGrowthModifierSource);
            playerStats.RuntimeStats.RemoveModifiersFromSource(mutationRampageModifierSource);
            playerStats.RuntimeStats.RemoveModifiersFromSource(frenzyHormoneModifierSource);
            forbiddenGrowthModifierApplied = false;
            overclockNerveModifierApplied = false;
            imperfectRegenModifierApplied = false;
            exoskeletonModifierApplied = false;
            ClearGrotesqueGrowthState();
            ClearMutationRampageState();
            ClearFrenzyHormoneState();
            ruptureAppliedMovePenalty = 0f;
            macrophageStacks = 0;
            macrophageAppliedAttackBonus = 0f;
            macrophageExpireTime = float.NegativeInfinity;
            gluttonousOrganStacks = 0;
            gluttonousOrganAppliedMoveBonus = 0f;
            gluttonousOrganExpireTime = float.NegativeInfinity;
            berserkCellModifierApplied = false;
            berserkCellExpireTime = float.NegativeInfinity;
            berserkCellActiveBossId = 0;
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

            ApplyFrenzyHormoneBuff();
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
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * Mathf.Max(0.3f, voidCellSpawnRadius);
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

        private EnemyController FindRandomEnemyWithinRadius(float radius)
        {
            var enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return null;
            }

            randomEnemyCandidates.Clear();
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

                randomEnemyCandidates.Add(enemy);
            }

            if (randomEnemyCandidates.Count == 0)
            {
                return null;
            }

            int index = UnityEngine.Random.Range(0, randomEnemyCandidates.Count);
            EnemyController selected = randomEnemyCandidates[index];
            randomEnemyCandidates.Clear();
            return selected;
        }

        private void UpdateUnstableCoreOverlay()
        {
            if (!hasUnstableCoreItem)
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
            if (!hasPlateletMembraneItem || plateletMembraneCurrentShield <= 0f)
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
            SpawnPooledCircleVisual(
                OverheatExplosionVisualPoolName,
                "OverheatExplosionFx",
                center,
                1.6f,
                new Color(1f, 0.22f, 0.1f, 0.86f),
                5300,
                0.2f);
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

        private class PlayerBioSummon : MonoBehaviour
        {
            public enum SummonKind
            {
                InfectedHost,
                Spore
            }

            private PlayerItemCombatEffects owner;
            private SpriteRenderer spriteRenderer;
            private EnemyController target;
            private float damage;
            private float expireTime;
            private float searchRadius;
            private float attackRadius;
            private float attackInterval;
            private float nextAttackTime;
            private float moveSpeed;
            private bool destroyOnAttack;
            private Sprite idleSprite;
            private Sprite[] attackSprites;
            private float attackVisualEndTime;
            private float attackVisualFrameTime;

            public SummonKind Kind { get; private set; }

            public void Initialize(
                PlayerItemCombatEffects owner,
                SummonKind kind,
                Sprite sprite,
                Sprite[] attackSprites,
                Vector3 visualScale,
                Color color,
                float damage,
                float lifetime,
                float searchRadius,
                float attackRadius,
                float attackInterval,
                bool destroyOnAttack)
            {
                this.owner = owner;
                Kind = kind;
                this.damage = Mathf.Max(0f, damage);
                this.searchRadius = Mathf.Max(0.1f, searchRadius);
                this.attackRadius = Mathf.Max(0.1f, attackRadius);
                this.attackInterval = Mathf.Max(0.05f, attackInterval);
                this.destroyOnAttack = destroyOnAttack;
                this.attackSprites = attackSprites;
                moveSpeed = kind == SummonKind.Spore ? 5.2f : 3.2f;
                expireTime = Time.time + Mathf.Max(0.1f, lifetime);

                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                idleSprite = sprite != null ? sprite : TextureSpriteCache.GetCircleSprite();
                spriteRenderer.sprite = idleSprite;
                spriteRenderer.color = color;
                spriteRenderer.sortingOrder = kind == SummonKind.Spore ? 5100 : 5050;
                transform.localScale = visualScale;
            }

            private void Update()
            {
                if (owner == null || Time.time >= expireTime)
                {
                    Destroy(gameObject);
                    return;
                }

                SyncBillboard();
                UpdateAttackVisual();
                if (!IsEnemyTargetable(target))
                {
                    target = owner.FindClosestEnemy(transform.position, searchRadius);
                }

                if (!IsEnemyTargetable(target))
                {
                    DriftAroundOwner();
                    return;
                }

                Vector3 toTarget = target.transform.position - transform.position;
                toTarget.y = 0f;
                float attackDistance = attackRadius;
                if (toTarget.sqrMagnitude > attackDistance * attackDistance)
                {
                    Vector3 step = toTarget.normalized * moveSpeed * Time.deltaTime;
                    transform.position += step;
                    return;
                }

                if (Time.time < nextAttackTime)
                {
                    return;
                }

                nextAttackTime = Time.time + attackInterval;
                StartAttackVisual();
                target.TakeDamage(damage);
                if (destroyOnAttack)
                {
                    SpawnBurstVisual();
                    Destroy(gameObject);
                }
            }

            private void StartAttackVisual()
            {
                if (attackSprites == null || attackSprites.Length == 0)
                {
                    return;
                }

                attackVisualEndTime = Time.time + 0.35f;
                attackVisualFrameTime = Time.time;
                spriteRenderer.sprite = attackSprites[0] != null ? attackSprites[0] : idleSprite;
            }

            private void UpdateAttackVisual()
            {
                if (attackSprites == null || attackSprites.Length == 0 || Time.time >= attackVisualEndTime)
                {
                    if (spriteRenderer != null && spriteRenderer.sprite != idleSprite)
                    {
                        spriteRenderer.sprite = idleSprite;
                    }

                    return;
                }

                float frameDuration = 0.35f / Mathf.Max(1, attackSprites.Length);
                if (Time.time < attackVisualFrameTime + frameDuration)
                {
                    return;
                }

                int frameIndex = Mathf.Clamp(
                    Mathf.FloorToInt((0.35f - Mathf.Max(0f, attackVisualEndTime - Time.time)) / frameDuration),
                    0,
                    attackSprites.Length - 1);
                attackVisualFrameTime = Time.time;
                if (attackSprites[frameIndex] != null)
                {
                    spriteRenderer.sprite = attackSprites[frameIndex];
                }
            }

            private void DriftAroundOwner()
            {
                if (owner == null)
                {
                    return;
                }

                Vector3 home = owner.transform.position;
                Vector3 toHome = home - transform.position;
                toHome.y = 0f;
                if (toHome.sqrMagnitude <= 1.8f * 1.8f)
                {
                    return;
                }

                transform.position += toHome.normalized * moveSpeed * 0.5f * Time.deltaTime;
            }

            private void SpawnBurstVisual()
            {
                PlayerItemCombatEffects.SpawnPooledCircleVisual(
                    SporeBurstVisualPoolName,
                    "SporeBurstVisual",
                    transform.position,
                    0.9f,
                    new Color(0.52f, 1f, 0.45f, 0.45f),
                    5150,
                    0.18f);
            }

            private void SyncBillboard()
            {
                Camera activeCamera = DontStarveCamera.GetActiveCamera();
                if (activeCamera != null)
                {
                    transform.rotation = activeCamera.transform.rotation;
                }
            }
        }

        private class PlayerBloodDrone : MonoBehaviour
        {
            private PlayerItemCombatEffects owner;
            private SpriteRenderer spriteRenderer;
            private float angle;
            private float nextFireTime;

            public void Initialize(PlayerItemCombatEffects owner)
            {
                this.owner = owner;
                angle = 0f;
                nextFireTime = 0f;

                if (spriteRenderer == null)
                {
                    spriteRenderer = PlayerItemCombatEffects.GetOrAddSpriteRenderer(gameObject);
                }

                spriteRenderer.sprite = TextureSpriteCache.GetCircleSprite();
                spriteRenderer.color = new Color(0.95f, 0.08f, 0.18f, 0.92f);
                spriteRenderer.sortingOrder = 5300;
                spriteRenderer.enabled = true;
                transform.localScale = Vector3.one * 0.68f;
            }

            public void ClearOwner()
            {
                owner = null;
            }

            private void Update()
            {
                if (owner == null)
                {
                    RuntimePool.Release(gameObject);
                    return;
                }

                angle += Time.deltaTime * 170f;
                float radians = angle * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)) * Mathf.Max(0.2f, owner.bloodDroneOrbitRadius);
                transform.position = owner.transform.position + offset + Vector3.up * 0.45f;
                SyncBillboard();

                if (Time.time < nextFireTime)
                {
                    return;
                }

                EnemyController target = owner.FindClosestEnemy(owner.transform.position, Mathf.Max(0.1f, owner.bloodDroneTargetRadius));
                if (target == null)
                {
                    return;
                }

                nextFireTime = Time.time + Mathf.Max(0.1f, owner.bloodDroneFireInterval);
                float damage = owner.GetCompanionBaseDamage() * Mathf.Max(0.05f, owner.bloodDroneDamageMultiplier);
                PlayerBioProjectile.Spawn(transform.position, target, damage, new Color(1f, 0.05f, 0.16f, 0.95f));
            }

            private void SyncBillboard()
            {
                Camera activeCamera = DontStarveCamera.GetActiveCamera();
                if (activeCamera != null)
                {
                    transform.rotation = activeCamera.transform.rotation;
                }
            }

            private void OnDisable()
            {
                owner = null;
            }
        }

        private class PlayerGuardianOrgan : MonoBehaviour
        {
            private PlayerItemCombatEffects owner;
            private SpriteRenderer spriteRenderer;
            private float angle;
            private float nextBlockTime;

            public void Initialize(PlayerItemCombatEffects owner)
            {
                this.owner = owner;
                angle = 0f;
                nextBlockTime = 0f;

                if (spriteRenderer == null)
                {
                    spriteRenderer = PlayerItemCombatEffects.GetOrAddSpriteRenderer(gameObject);
                }

                spriteRenderer.sprite = TextureSpriteCache.GetCircleSprite();
                spriteRenderer.color = new Color(0.68f, 0.82f, 1f, 0.95f);
                spriteRenderer.sortingOrder = 5350;
                spriteRenderer.enabled = true;
                transform.localScale = Vector3.one * 0.82f;
            }

            public void ClearOwner()
            {
                owner = null;
            }

            private void Update()
            {
                if (owner == null)
                {
                    RuntimePool.Release(gameObject);
                    return;
                }

                angle += Time.deltaTime * 260f;
                float radians = angle * Mathf.Deg2Rad;
                Camera activeCamera = DontStarveCamera.GetActiveCamera();
                Vector3 right = activeCamera != null ? activeCamera.transform.right : Vector3.right;
                Vector3 up = activeCamera != null ? activeCamera.transform.up : Vector3.up;
                Vector3 center = owner.GetPlayerVisualCenter();
                Vector3 offset = (right * Mathf.Cos(radians) + up * Mathf.Sin(radians)) * Mathf.Max(0.2f, owner.guardianOrganOrbitRadius);
                transform.position = center + offset;
                spriteRenderer.color = Time.time >= nextBlockTime
                    ? new Color(0.68f, 0.82f, 1f, 0.95f)
                    : new Color(0.35f, 0.48f, 0.7f, 0.45f);
                SyncBillboard();

                if (Time.time < nextBlockTime)
                {
                    return;
                }

                TryBlockProjectile();
            }

            private void TryBlockProjectile()
            {
                IReadOnlyList<EnemyProjectile> projectiles = EnemyProjectile.ActiveEnemyProjectiles;
                float blockRadius = Mathf.Max(0.1f, owner.guardianOrganBlockRadius);
                float blockRadiusSqr = blockRadius * blockRadius;

                for (int i = projectiles.Count - 1; i >= 0; i--)
                {
                    EnemyProjectile projectile = projectiles[i];
                    if (projectile == null || !projectile.IsLaunched)
                    {
                        continue;
                    }

                    Vector3 toProjectile = projectile.transform.position - transform.position;
                    toProjectile.y = 0f;
                    if (toProjectile.sqrMagnitude > blockRadiusSqr)
                    {
                        continue;
                    }

                    projectile.Deflect();
                    nextBlockTime = Time.time + Mathf.Max(0.1f, owner.guardianOrganCooldown);
                    SpawnBlockVisual();
                    return;
                }
            }

            private void SpawnBlockVisual()
            {
                PlayerItemCombatEffects.SpawnPooledCircleVisual(
                    GuardianBlockVisualPoolName,
                    "GuardianBlockVisual",
                    transform.position,
                    1.2f,
                    new Color(0.68f, 0.82f, 1f, 0.35f),
                    5360,
                    0.16f);
            }

            private void SyncBillboard()
            {
                Camera activeCamera = DontStarveCamera.GetActiveCamera();
                if (activeCamera != null)
                {
                    transform.rotation = activeCamera.transform.rotation;
                }
            }

            private void OnDisable()
            {
                owner = null;
            }
        }

        private class PlayerBioProjectile : MonoBehaviour
        {
            private EnemyController target;
            private SpriteRenderer spriteRenderer;
            private float damage;
            private float expireTime;
            private float speed;

            public static void Spawn(Vector3 position, EnemyController target, float damage, Color color)
            {
                if (!IsEnemyTargetable(target))
                {
                    return;
                }

                GameObject projectileObject = RuntimePool.Acquire(BloodDroneProjectilePoolName, CreateBloodDroneProjectileFunc);
                if (projectileObject == null)
                {
                    return;
                }

                projectileObject.name = "BloodDroneProjectile";
                projectileObject.transform.position = position;
                PlayerBioProjectile projectile = projectileObject.GetComponent<PlayerBioProjectile>();
                projectile.Initialize(target, damage, color);
            }

            private void Initialize(EnemyController target, float damage, Color color)
            {
                this.target = target;
                this.damage = Mathf.Max(0f, damage);
                speed = 11f;
                expireTime = Time.time + 1.6f;
                if (spriteRenderer == null)
                {
                    spriteRenderer = PlayerItemCombatEffects.GetOrAddSpriteRenderer(gameObject);
                }

                spriteRenderer.sprite = TextureSpriteCache.GetCircleSprite();
                spriteRenderer.color = color;
                spriteRenderer.sortingOrder = 5320;
                spriteRenderer.enabled = true;
                transform.localScale = Vector3.one * 0.22f;
            }

            private void Update()
            {
                if (Time.time >= expireTime || !IsEnemyTargetable(target))
                {
                    ReleaseSelf();
                    return;
                }

                Vector3 toTarget = target.transform.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude <= 0.45f * 0.45f)
                {
                    target.TakeDamage(damage);
                    ReleaseSelf();
                    return;
                }

                transform.position += toTarget.normalized * speed * Time.deltaTime;
                Camera activeCamera = DontStarveCamera.GetActiveCamera();
                if (activeCamera != null)
                {
                    transform.rotation = activeCamera.transform.rotation;
                }
            }

            private void ReleaseSelf()
            {
                target = null;
                RuntimePool.Release(gameObject);
            }

            private void OnDisable()
            {
                target = null;
            }
        }
    }
}
