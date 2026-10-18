using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerItemManager))]
    public partial class PlayerItemCombatEffects : MonoBehaviour, IBasicAttackReadModel, IBasicAttackItemEffects, IItemEffectLifecycle, IItemCompanionOwner, IItemKillActions
    {
        public const string DoubleCoreId = PlayerItemIds.DoubleCoreId;
        public const string TripleCoreId = PlayerItemIds.TripleCoreId;
        public const string HomingCellId = PlayerItemIds.HomingCellId;
        public const string HypertrophyCellId = PlayerItemIds.HypertrophyCellId;
        public const string RefluxOrganId = PlayerItemIds.RefluxOrganId;
        public const string PiercingMucusId = PlayerItemIds.PiercingMucusId;
        public const string LaryngealNerveId = PlayerItemIds.LaryngealNerveId;
        public const string BeamOrganId = PlayerItemIds.BeamOrganId;
        public const string SplitTissueId = PlayerItemIds.SplitTissueId;
        public const string ExplosiveBloodCellId = PlayerItemIds.ExplosiveBloodCellId;
        public const string AcidicRuptureId = PlayerItemIds.AcidicRuptureId;
        public const string CellProliferationId = PlayerItemIds.CellProliferationId;
        public const string PulseBulletId = PlayerItemIds.PulseBulletId;
        public const string VascularReflectionId = PlayerItemIds.VascularReflectionId;
        public const string ToxicMucosaId = PlayerItemIds.ToxicMucosaId;
        public const string FreezingNerveId = PlayerItemIds.FreezingNerveId;
        public const string HemorrhageOrganId = PlayerItemIds.HemorrhageOrganId;
        public const string OverheatedOrganId = PlayerItemIds.OverheatedOrganId;
        public const string MutantEyeId = PlayerItemIds.MutantEyeId;
        public const string OrganTentacleId = PlayerItemIds.OrganTentacleId;
        public const string RampageBloodFlowId = PlayerItemIds.RampageBloodFlowId;
        public const string MuscleSpasmId = PlayerItemIds.MuscleSpasmId;
        public const string UnstableCoreId = PlayerItemIds.UnstableCoreId;
        public const string BioResonanceId = PlayerItemIds.BioResonanceId;
        public const string BloodPressureBurstId = PlayerItemIds.BloodPressureBurstId;
        public const string VoidCellId = PlayerItemIds.VoidCellId;
        public const string ForbiddenGrowthId = PlayerItemIds.ForbiddenGrowthId;
        public const string OverclockNerveId = PlayerItemIds.OverclockNerveId;
        public const string BloodContractId = PlayerItemIds.BloodContractId;
        public const string HyperplasiaHeartId = PlayerItemIds.HyperplasiaHeartId;
        public const string DecayOrganId = PlayerItemIds.DecayOrganId;
        public const string RuptureMuscleId = PlayerItemIds.RuptureMuscleId;
        public const string ImperfectRegenerationId = PlayerItemIds.ImperfectRegenerationId;
        public const string SeveranceReflexId = PlayerItemIds.SeveranceReflexId;
        public const string BioGambleId = PlayerItemIds.BioGambleId;
        public const string ExoskeletonId = PlayerItemIds.ExoskeletonId;
        public const string PlateletMembraneId = PlayerItemIds.PlateletMembraneId;
        public const string RecoveryFactorId = PlayerItemIds.RecoveryFactorId;
        public const string ReflectiveSkinId = PlayerItemIds.ReflectiveSkinId;
        public const string BioBarrierId = PlayerItemIds.BioBarrierId;
        public const string SplitRegenerationId = PlayerItemIds.SplitRegenerationId;
        public const string InfectedHostId = PlayerItemIds.InfectedHostId;
        public const string SporeColonyId = PlayerItemIds.SporeColonyId;
        public const string BloodDroneId = PlayerItemIds.BloodDroneId;
        public const string GuardianOrganId = PlayerItemIds.GuardianOrganId;
        public const string TentacleColonyId = PlayerItemIds.TentacleColonyId;
        public const string ElectricNeuralNetworkId = PlayerItemIds.ElectricNeuralNetworkId;
        public const string InfectionTransferenceId = PlayerItemIds.InfectionTransferenceId;
        public const string MacrophageId = PlayerItemIds.MacrophageId;
        public const string GluttonousOrganId = PlayerItemIds.GluttonousOrganId;
        public const string HeartSniperId = PlayerItemIds.HeartSniperId;
        public const string BloodflowAccelerationId = PlayerItemIds.BloodflowAccelerationId;
        public const string FocusedNerveId = PlayerItemIds.FocusedNerveId;
        public const string ExecutionInstinctId = PlayerItemIds.ExecutionInstinctId;
        public const string BerserkCellId = PlayerItemIds.BerserkCellId;
        public const string UnstableCellId = PlayerItemIds.UnstableCellId;
        public const string GrotesqueGrowthId = PlayerItemIds.GrotesqueGrowthId;
        public const string MutationRampageId = PlayerItemIds.MutationRampageId;
        public const string ParasiticBombId = PlayerItemIds.ParasiticBombId;
        public const string FrenzyHormoneId = PlayerItemIds.FrenzyHormoneId;

        private const string ParasiticBombVisualPoolName = "PlayerItem.ParasiticBombVisual";
        private const string OverheatExplosionVisualPoolName = "PlayerItem.OverheatExplosionVisual";
        private const string SporeBurstVisualPoolName = "PlayerItem.SporeBurstVisual";
        private const string GuardianBlockVisualPoolName = "PlayerItem.GuardianBlockVisual";
        private const string TentacleBindVisualPoolName = "PlayerItem.TentacleBindVisual";
        private const string LineVisualPoolName = "PlayerItem.LineVisual";
        private const string BloodDronePoolName = "PlayerItem.BloodDrone";
        private const string GuardianOrganPoolName = "PlayerItem.GuardianOrgan";
        private const string BloodDroneProjectilePoolName = "PlayerItem.BloodDroneProjectile";
        private const string BioSummonPoolName = "PlayerItem.BioSummon";
        private const string OverheatedOrganEffectPath = "ItemEffects/overheated_organ_effect";
        private const string OrganTentacleEffectPath = "ItemEffects/organ_tentacle_effect";
        private const string PlateletMembraneEffectPath = "ItemEffects/platelet_membrane_effect";
        private const string InfectedHostEffectPath = "ItemEffects/infected_host_effect";
        private const string SporeColonyEffectPath = "ItemEffects/spore_colony_effect";
        private const string BloodDroneEffectPath = "ItemEffects/blood_drone_effect";
        private const string GuardianOrganEffectPath = "ItemEffects/guardian_organ_effect";
        private const string TentacleColonyEffectPath = "ItemEffects/tentacle_colony_effect";
        private const string TentacleColonyConnectionEffectPath = "ItemEffects/tentacle_colony_connection_effect";
        private const string ElectricNeuralNetworkEffectPath = "ItemEffects/electric_neural_network_effect";
        private const string InfectionTransferenceEffectPath = "ItemEffects/infection_transference_effect";
        private const string ParasiticBombEffectPath = "ItemEffects/parasitic_bomb_effect";

        [Header("Multi Shot")]
        [SerializeField] private float doubleShotSpreadAngle = 7f;
        [SerializeField] private float tripleShotSpreadAngle = 11f;
        [SerializeField] private float doubleShotDamageMultiplier = 0.75f;
        [SerializeField] private float tripleShotDamageMultiplier = 0.55f;
        [SerializeField] private float backShotDamageMultiplier = 0.9f;

        [Header("Projectile Movement")]
        [SerializeField] private float homingTurnRate = 7f;
        [SerializeField] private float homingSearchRadius = 8f;
        [SerializeField] private float boomerangReturnDistance = 8f;
        [SerializeField] private float boomerangRepeatHitDamageMultiplier = 0.75f;
        [SerializeField, Min(1)] private int boomerangMaxHitsPerPass = 3;
        [SerializeField, Min(1)] private int piercingHitCount = 3;
        [SerializeField] private float piercingSubsequentHitDamageMultiplier = 0.75f;
        [SerializeField, Min(1)] private int reflectionBounceCount = 3;

        [Header("Projectile Scale")]
        [SerializeField] private float hypertrophyScaleMultiplier = 1.45f;
        [SerializeField] private float hypertrophyRangeMultiplier = 1.25f;
        [SerializeField] private float pulseGrowEndScale = 2.6f;
        [SerializeField] private float pulseShrinkEndScale = 0.2f;

        [Header("On-Hit")]
        [SerializeField] private float splitDamageMultiplier = 0.5f;
        [SerializeField] private float splitRangeMultiplier = 0.65f;
        [SerializeField] private float splitAngle = 90f;
        [SerializeField] private float explosionRadius = 2f;
        [SerializeField] private float explosionDamageMultiplier = 1f;
        [SerializeField] private float explosiveBloodCellRangedDamageMultiplier = 0.5f;
        [SerializeField] private float acidDuration = 4f;
        [SerializeField] private float acidTickInterval = 1f;
        [SerializeField] private float acidTickDamage = 0.5f;
        [SerializeField] private float poisonDuration = 4f;
        [SerializeField] private float poisonTickInterval = 1f;
        [SerializeField] private float poisonTickDamageRatio = 0.2f;
        [SerializeField] private float freezeSlowRatio = 0.3f;
        [SerializeField] private float freezeDuration = 2f;
        [SerializeField] private float bleedDuration = 3.5f;
        [SerializeField] private float bleedTickInterval = 0.8f;
        [SerializeField] private float bleedTickDamageRatio = 0.18f;

        [Header("Trigger Effects")]
        [SerializeField, Range(0f, 1f)] private float cellProliferationChance = 0.25f;
        [SerializeField] private float cellProliferationDamageMultiplier = 0.9f;

        [Header("Beam")]
        [SerializeField] private float beamRadius = 0.8f;
        [SerializeField] private float beamDamageMultiplier = 0.8f;
        [SerializeField, Min(1)] private int beamHitBufferSize = 48;

        [Header("Special Items")]
        [SerializeField] private int overheatMaxStacks = 10;
        [SerializeField] private float overheatStackWindow = 1.4f;
        [SerializeField] private float overheatPerStackAttackSpeedBonus = 0.1f;
        [SerializeField] private float overheatDecayInterval = 5f;
        [SerializeField] private float overheatExplosionRadius = 2.5f;
        [SerializeField] private float overheatExplosionDamageMultiplier = 1.5f;
        [SerializeField] private float overheatExplosionSelfDamage = 1f;
        [SerializeField] private float mutantEyeAccuracyPenaltyAngle = 40f;
        [SerializeField] private float mutantEyeFlatDamageBonus = 2f;
        [SerializeField] private float organTentacleAutoAttackInterval = 1.2f;
        [SerializeField] private float organTentacleAutoAttackRadius = 4.5f;
        [SerializeField] private float organTentacleAutoAttackDamageMultiplier = 0.3f;
        [SerializeField] private int organTentacleMaxTargets = 2;
        [SerializeField] private float organTentacleBaseAttackPenaltyMultiplier = 0.8f;
        [SerializeField] private float rampageMoveSecondsPerBonus = 8f;
        [SerializeField] private int rampageMaxAttackBonus = 2;
        [SerializeField] private float rampageIdleSecondsPerDecay = 4f;
        [SerializeField] private float muscleSpasmMeleeRangeMultiplier = 1.7f;
        [SerializeField] private float muscleSpasmAttackSpeedMultiplier = 0.8f;
        [SerializeField] private float unstableCoreMinAttackRatio = 0.6f;
        [SerializeField] private float unstableCoreMaxAttackRatio = 1.6f;
        [SerializeField] private float unstableCoreRerollInterval = 5f;
        [SerializeField] private float bioResonanceStackDamageBonus = 0.15f;
        [SerializeField] private int bioResonanceMaxStacks = 3;
        [SerializeField] private float bioResonanceStackWindow = 3f;
        [SerializeField] private float bloodPressureBaseAttackSpeedAdd;
        [SerializeField] private float bloodPressurePerTenPercentMissingAdd = 0.06f;
        [SerializeField] private float voidCellChance = 0.2f;
        [SerializeField] private float voidCellDamageMultiplier = 0.85f;
        [SerializeField] private float voidCellSpawnRadius = 2.2f;
        [SerializeField] private float forbiddenGrowthMaxHealthPenalty = 3f;
        [SerializeField] private float forbiddenGrowthFlatAttackBonus = 2f;
        [SerializeField] private float overclockNerveMaxHealthPenalty = 2f;
        [SerializeField] private float overclockNerveFlatMoveSpeedBonus = 2f;
        [SerializeField] private float bloodContractIncomingDamageMultiplier = 1.3f;
        [SerializeField, Min(1)] private int bloodContractKillsPerHeal = 20;
        [SerializeField] private float bloodContractHealthGainAmount = 1f;
        [SerializeField, Min(1)] private int bloodContractMaxHealthGainCount = 5;
        [SerializeField] private float hyperplasiaMissingHealthAttackBonusMax = 2f;
        [SerializeField] private float decayOrganSecondsPerAttackBonus = 180f;
        [SerializeField] private int decayOrganMaxAttackBonus = 2;
        [SerializeField] private float ruptureMuscleAttackBonusPerStack = 0.5f;
        [SerializeField] private float ruptureMuscleMovePenaltyPerStack = 0.25f;
        [SerializeField] private int ruptureMuscleMaxStacks = 3;
        [SerializeField] private float ruptureMuscleDecayDelay = 3f;
        [SerializeField] private float imperfectRegenMaxHealthPenalty = 2f;
        [SerializeField] private float imperfectRegenDelay = 2f;
        [SerializeField] private float imperfectRegenHealPerTrigger = 1f;
        [SerializeField] private float imperfectRegenCooldownDuration = 10f;
        [SerializeField] private float severanceReflexDuration = 2f;
        [SerializeField] private float severanceReflexFlatAttackBonus = 2f;
        [SerializeField] private float exoskeletonDamageReductionRatio = 0.25f;
        [SerializeField] private float exoskeletonMoveSpeedPenalty = 1.5f;
        [SerializeField] private float plateletMembraneInterval = 20f;
        [SerializeField] private float plateletMembraneShieldAmount = 1f;
        [SerializeField] private float recoveryFactorInterval = 30f;
        [SerializeField] private float recoveryFactorHealAmount = 1f;
        [SerializeField] private float reflectiveSkinDamageRatio = 0.75f;
        [SerializeField] private float bioBarrierIdleSecondsPerStep = 1.5f;
        [SerializeField] private float bioBarrierReductionPerStep = 0.1f;
        [SerializeField] private float bioBarrierMaxReduction = 0.4f;
        [SerializeField] private float splitRegenerationReviveHealth = 2f;

        [Header("Bio Companions")]
        [SerializeField, Range(0f, 1f)] private float infectedHostChance = 0.15f;
        [SerializeField] private float infectedHostLifetime = 6f;
        [SerializeField] private float infectedHostDamageMultiplier = 0.35f;
        [SerializeField] private float infectedHostAttackInterval = 1f;
        [SerializeField] private float infectedHostAttackRadius = 1.25f;
        [SerializeField] private float infectedHostSearchRadius = 7f;
        [SerializeField, Min(1)] private int infectedHostMaxAllies = 2;
        [SerializeField] private float sporeSpawnInterval = 8f;
        [SerializeField] private float sporeLifetime = 6f;
        [SerializeField] private float sporeDamageMultiplier = 1f;
        [SerializeField] private float sporeSearchRadius = 8f;
        [SerializeField] private float sporeBurstRadius = 1.1f;
        [SerializeField] private float bloodDroneOrbitRadius = 1.4f;
        [SerializeField] private float bloodDroneFireInterval = 1f;
        [SerializeField] private float bloodDroneDamageMultiplier = 0.3f;
        [SerializeField] private float bloodDroneTargetRadius = 9f;
        [SerializeField] private float guardianOrganOrbitRadius = 1.2f;
        [SerializeField] private float guardianOrganBlockRadius = 0.9f;
        [SerializeField] private float guardianOrganCooldown = 3.5f;
        [SerializeField] private float tentacleBindInterval = 3f;
        [SerializeField] private float tentacleBindRadius = 5.5f;
        [SerializeField] private float tentacleBindDuration = 1.2f;
        [SerializeField] private float tentacleSlowRatio = 0.6f;
        [SerializeField] private float tentacleDamageMultiplier = 0.1f;
        [SerializeField, Min(1)] private int tentacleMaxTargets = 2;

        [Header("Kill Chain Items")]
        [SerializeField] private float electricChainDamage = 1f;
        [SerializeField] private float electricChainRadius = 6f;
        [SerializeField, Min(1)] private int electricChainMaxHits = 3;
        [SerializeField] private float electricChainCooldown = 5f;
        [SerializeField] private float infectionTransferRadius = 4.5f;
        [SerializeField, Min(1)] private int infectionTransferMaxTargets = 3;
        [SerializeField] private float infectionTransferDuration = 3f;
        [SerializeField] private float infectionTransferTickInterval = 1f;
        [SerializeField] private float infectionTransferTickDamage = 0.5f;
        [SerializeField] private float infectionTransferCooldown = 5f;
        [SerializeField] private float macrophageDuration = 5f;
        [SerializeField] private float macrophageAttackBonusPerStack = 0.5f;
        [SerializeField, Min(1)] private int macrophageMaxStacks = 3;
        [SerializeField] private float gluttonousOrganDuration = 3f;
        [SerializeField] private float gluttonousOrganMoveBonusPerStack = 0.5f;
        [SerializeField, Min(1)] private int gluttonousOrganMaxStacks = 4;

        [Header("Boss Fight Items")]
        [SerializeField] private float heartSniperHealthThreshold = 0.6f;
        [SerializeField] private float heartSniperDamageBonusRatio = 0.35f;
        [SerializeField] private float bloodflowAccelerationRadius = 9f;
        [SerializeField] private float bloodflowAccelerationAttackSpeedBonus = 0.35f;
        [SerializeField] private float focusedNerveRadius = 6f;
        [SerializeField] private float focusedNerveHighAttackBonus = 0.75f;
        [SerializeField] private float focusedNerveLowAttackBonus = 0.5f;
        [SerializeField] private float executionInstinctHealthThreshold = 0.2f;
        [SerializeField, Range(0f, 1f)] private float executionInstinctChance = 0.2f;
        [SerializeField] private float executionInstinctBossDamageMultiplier = 1.5f;
        [SerializeField] private float berserkCellDuration = 15f;
        [SerializeField] private float berserkCellAttackBonus = 0.75f;
        [SerializeField] private float berserkCellMoveBonus = 0.5f;
        [SerializeField] private float berserkCellAttackSpeedBonus = 0.3f;
        [SerializeField] private float berserkCellBossDetectionRadius = 18f;

        [Header("Mutation Chaos Items")]
        [SerializeField] private float unstableCellMinProjectileSpeedMultiplier = 0.5f;
        [SerializeField] private float unstableCellMaxProjectileSpeedMultiplier = 1.5f;
        [SerializeField] private float unstableCellSlowThreshold = 0.8f;
        [SerializeField] private float unstableCellSlowDamageMultiplier = 1.5f;
        [SerializeField] private float grotesqueGrowthInterval = 10f;
        [SerializeField] private float grotesqueGrowthSmallScale = 0.75f;
        [SerializeField] private float grotesqueGrowthLargeScale = 1.35f;
        [SerializeField] private float grotesqueGrowthSmallMoveBonus = 1f;
        [SerializeField] private float grotesqueGrowthLargeAttackBonus = 1f;
        [SerializeField] private float grotesqueGrowthLargeMovePenalty = 0.5f;
        [SerializeField] private float mutationRampageInterval = 15f;
        [SerializeField] private float mutationRampageDuration = 7f;
        [SerializeField, Range(0f, 1f)] private float mutationRampageBuffChance = 0.6f;
        [SerializeField] private float mutationRampageAttackBuff = 1f;
        [SerializeField] private float mutationRampageMoveBuff = 1f;
        [SerializeField] private float mutationRampageAttackSpeedBuff = 0.5f;
        [SerializeField] private float mutationRampageAttackDebuff = 0.5f;
        [SerializeField] private float mutationRampageMoveDebuff = 0.75f;
        [SerializeField] private float mutationRampageAttackSpeedDebuff = 0.25f;
        [SerializeField, Range(0f, 1f)] private float parasiticBombChance = 0.2f;
        [SerializeField] private float parasiticBombRadius = 4f;
        [SerializeField] private float parasiticBombDamageMultiplier = 1.5f;
        [SerializeField] private float frenzyHormoneDuration = 3f;
        [SerializeField] private float frenzyHormoneCooldown = 8f;
        [SerializeField] private float frenzyHormoneMinBonus = 0.5f;
        [SerializeField] private float frenzyHormoneMaxBonus = 1f;


        private readonly ItemKillEffectState killState = new ItemKillEffectState();
        private PlayerItemManager itemManager;
        private PlayerController playerController;
        private PlayerStats playerStats;
        private SpriteRenderer unstableCoreOverlay;
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineSizeId = Shader.PropertyToID("_OutlineSize");
        private static readonly int OutlineExpandId = Shader.PropertyToID("_OutlineExpand");
        private Material plateletMembraneOutlineMaterial;
        private SpriteRenderer plateletMembraneOutlineTarget;
        private SpriteRenderer plateletMembraneOutlineRenderer;
        private SpriteRenderer plateletMembraneEffectRenderer;
        private bool plateletMembraneShaderWarningLogged;
        private SpriteRenderer playerVisualSpriteRenderer;
        private readonly List<SpriteRenderer> playerSpriteRendererBuffer = new List<SpriteRenderer>();
        private bool subscribedHealthEvents;
        private readonly List<EnemyController> tempEnemyTargets = new List<EnemyController>();
        private bool disablingForEmptyInventory;
        private static Material runtimeLineMaterial;
        private static readonly Func<GameObject> CreateLineVisualFunc = CreateLineVisualObject;
        public bool HasHomingCell => Ownership.HasHomingCell;
        public bool HasRefluxOrgan => Ownership.HasRefluxOrgan;
        public bool HasPiercingMucus => Ownership.HasPiercingMucus;
        public bool HasLaryngealNerve => Ownership.HasLaryngealNerve;
        public bool HasBeamOrgan => Ownership.HasBeamOrgan;
        public bool HasSplitTissue => Ownership.HasSplitTissue;
        public bool HasExplosiveBloodCell => Ownership.HasExplosiveBloodCell;
        public bool HasAcidicRupture => Ownership.HasAcidicRupture;
        public bool HasPulseBullet => Ownership.HasPulseBullet;
        public bool HasVascularReflection => Ownership.HasVascularReflection;
        public bool HasToxicMucosa => Ownership.HasToxicMucosa;
        public bool HasFreezingNerve => Ownership.HasFreezingNerve;
        public bool HasHemorrhageOrgan => Ownership.HasHemorrhageOrgan;
        public bool HasOverheatedOrgan => Ownership.HasOverheatedOrgan;
        public bool HasMutantEye => Ownership.HasMutantEye;
        public bool HasOrganTentacle => Ownership.HasOrganTentacle;
        public bool HasRampageBloodFlow => Ownership.HasRampageBloodFlow;
        public bool HasMuscleSpasm => Ownership.HasMuscleSpasm;
        public bool HasUnstableCore => Ownership.HasUnstableCore;
        public bool HasBioResonance => Ownership.HasBioResonance;
        public bool HasBloodPressureBurst => Ownership.HasBloodPressureBurst;
        public bool HasVoidCell => Ownership.HasVoidCell;
        public bool HasForbiddenGrowth => Ownership.HasForbiddenGrowth;
        public bool HasOverclockNerve => Ownership.HasOverclockNerve;
        public bool HasBloodContract => Ownership.HasBloodContract;
        public bool HasHyperplasiaHeart => Ownership.HasHyperplasiaHeart;
        public bool HasDecayOrgan => Ownership.HasDecayOrgan;
        public bool HasRuptureMuscle => Ownership.HasRuptureMuscle;
        public bool HasImperfectRegeneration => Ownership.HasImperfectRegeneration;
        public bool HasSeveranceReflex => Ownership.HasSeveranceReflex;
        public bool HasBioGamble => Ownership.HasBioGamble;
        public bool HasExoskeleton => Ownership.HasExoskeleton;
        public bool HasPlateletMembrane => Ownership.HasPlateletMembrane;
        public bool HasRecoveryFactor => Ownership.HasRecoveryFactor;
        public bool HasReflectiveSkin => Ownership.HasReflectiveSkin;
        public bool HasBioBarrier => Ownership.HasBioBarrier;
        public bool HasSplitRegeneration => Ownership.HasSplitRegeneration;
        public bool HasInfectedHost => Ownership.HasInfectedHost;
        public bool HasSporeColony => Ownership.HasSporeColony;
        public bool HasBloodDrone => Ownership.HasBloodDrone;
        public bool HasGuardianOrgan => Ownership.HasGuardianOrgan;
        public bool HasTentacleColony => Ownership.HasTentacleColony;
        public bool HasElectricNeuralNetwork => Ownership.HasElectricNeuralNetwork;
        public bool HasInfectionTransference => Ownership.HasInfectionTransference;
        public bool HasMacrophage => Ownership.HasMacrophage;
        public bool HasGluttonousOrgan => Ownership.HasGluttonousOrgan;
        public bool HasHeartSniper => Ownership.HasHeartSniper;
        public bool HasBloodflowAcceleration => Ownership.HasBloodflowAcceleration;
        public bool HasFocusedNerve => Ownership.HasFocusedNerve;
        public bool HasExecutionInstinct => Ownership.HasExecutionInstinct;
        public bool HasBerserkCell => Ownership.HasBerserkCell;
        public bool HasUnstableCell => Ownership.HasUnstableCell;
        public bool HasGrotesqueGrowth => Ownership.HasGrotesqueGrowth;
        public bool HasMutationRampage => Ownership.HasMutationRampage;
        public bool HasParasiticBomb => Ownership.HasParasiticBomb;
        public bool HasFrenzyHormone => Ownership.HasFrenzyHormone;
        public int BeamHitBufferSize => Attack.BeamHitBufferSize;
        public float BeamRadius => Attack.BeamRadius;
        public float BeamDamageMultiplier => Attack.BeamDamageMultiplier;
        public float CellProliferationDamageMultiplier => Attack.CellProliferationDamageMultiplier;

        private void Awake()
        {
            itemManager = GetComponent<PlayerItemManager>();
            playerController = GetComponent<PlayerController>();
            playerStats = GetComponent<PlayerStats>();
            Timed.previousPosition = Timed.GetMovementAnchorPosition();
            Ownership.RebuildItemCache();
        }

        private void OnEnable()
        {
            if (!runtimeBound) return;
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

            itemManager.ItemAcquired += Ownership.HandleItemAcquired;
            itemManager.ItemRemoved += Ownership.HandleItemRemoved;
            Ownership.RebuildItemCache();
        }

        private void OnDisable()
        {
            unchecked { killState.electricChainLifetimeVersion++; }
            if (itemManager != null)
            {
                itemManager.ItemAcquired -= Ownership.HandleItemAcquired;
                itemManager.ItemRemoved -= Ownership.HandleItemRemoved;
            }

            TryUnsubscribeHealthEvents();
            ClearPlateletMembraneOutline();
            Ownership.ClearPersistentStatModifiers();
            Attack.resonanceStatesByEnemyId.Clear();
            if (!disablingForEmptyInventory)
            {
                ClearBioCompanions();
            }

            disablingForEmptyInventory = false;
        }

        private bool runtimeBound;

        internal void BindRuntime(PlayerController owner, PlayerStats stats, PlayerItemManager manager)
        {
            TryUnsubscribeHealthEvents();
            if (itemManager != null)
            {
                itemManager.ItemAcquired -= Ownership.HandleItemAcquired;
                itemManager.ItemRemoved -= Ownership.HandleItemRemoved;
            }
            playerController = owner;
            playerStats = stats;
            itemManager = manager;
            runtimeBound = true;
            if (isActiveAndEnabled)
            {
                TrySubscribeHealthEvents();
                if (itemManager != null)
                {
                    itemManager.ItemAcquired += Ownership.HandleItemAcquired;
                    itemManager.ItemRemoved += Ownership.HandleItemRemoved;
                }
            }
            Timed.previousPosition = Timed.GetMovementAnchorPosition();
            Ownership.RebuildItemCache();
        }

        private void Update()
        {
            if (!Ownership.hasAnyCachedItems)
            {
                return;
            }

            if (Ownership.hasPersistentStatItems)
            {
                Timed.SyncPersistentStatModifiers();
            }

            if (Ownership.hasDefensiveUpdateItems)
            {
                Timed.UpdateDefensiveStates();
            }

            if (Ownership.hasMovementTrackingItems)
            {
                Timed.UpdateMovementIntensity();
            }

            if (Ownership.hasRampageBloodFlowItem)
            {
                Timed.UpdateRampageBloodFlowState();
            }

            if (Ownership.hasDecayOrganItem)
            {
                Timed.UpdateDecayOrganState();
            }

            if (Ownership.hasRuptureMuscleItem)
            {
                Timed.UpdateRuptureMuscleState();
            }

            if (Ownership.hasImperfectRegenItem)
            {
                Timed.UpdateImperfectRegenState();
            }

            if (Ownership.hasOrganTentacleItem)
            {
                Attack.UpdateTentacleAutoAttack();
            }

            if (Ownership.hasBioCompanionUpdateItems)
            {
                Companions.UpdateBioCompanionItems();
            }

            if (Ownership.hasKillChainBuffItems)
            {
                UpdateKillChainBuffs();
            }

            if (Ownership.hasBossFightUpdateItems)
            {
                Timed.UpdateBossFightItemStates();
            }

            if (Ownership.hasMutationChaosItems)
            {
                Timed.UpdateMutationChaosItems();
            }

            if (Ownership.hasDecayStateItems)
            {
                Attack.UpdateDecayStates();
            }

            if (Ownership.hasPlateletMembraneItem)
            {
                UpdatePlateletMembraneOutline();
            }

            if (Ownership.hasUnstableCoreItem)
            {
                UpdateUnstableCoreOverlay();
            }
        }
        private void UpdateDefensiveStates()
            => Timed.UpdateDefensiveStates();
        public int GetForwardProjectileCount()
            => Attack.GetForwardProjectileCount();
        public float GetSpreadAngleForCount(int projectileCount)
            => Attack.GetSpreadAngleForCount(projectileCount);
        public float GetForwardProjectileDamageMultiplier(int projectileCount)
            => Attack.GetForwardProjectileDamageMultiplier(projectileCount);
        public float GetBackShotDamageMultiplier()
            => Attack.GetBackShotDamageMultiplier();
        public bool RollCellProliferation()
            => Attack.RollCellProliferation();
        public float GetRangeMultiplier()
            => Attack.GetRangeMultiplier();
        public float GetScaleMultiplier()
            => Attack.GetScaleMultiplier();
        public int GetPiercingHitCount()
            => Attack.GetPiercingHitCount();
        public float GetPiercingHitDamageMultiplier(int previousHitCount)
            => Attack.GetPiercingHitDamageMultiplier(previousHitCount);
        public int GetReflectionBounceCount()
            => Attack.GetReflectionBounceCount();
        public float GetBoomerangReturnDistance()
            => Attack.GetBoomerangReturnDistance();
        public float GetBoomerangReturnDistance(float minimumReturnDistance)
            => Attack.GetBoomerangReturnDistance(minimumReturnDistance);
        public float GetBoomerangRepeatHitDamageMultiplier()
            => Attack.GetBoomerangRepeatHitDamageMultiplier();
        public int GetBoomerangMaxHitsPerPass()
            => Attack.GetBoomerangMaxHitsPerPass();
        public float GetHomingTurnRate()
            => Attack.GetHomingTurnRate();
        public float GetHomingSearchRadius()
            => Attack.GetHomingSearchRadius();
        public float GetPulseAmplitude()
            => Attack.GetPulseAmplitude();
        public float GetPulseFrequency()
            => Attack.GetPulseFrequency();
        public float GetSplitAngle()
            => Attack.GetSplitAngle();
        public float GetSplitDamageMultiplier()
            => Attack.GetSplitDamageMultiplier();
        public float GetSplitRangeMultiplier()
            => Attack.GetSplitRangeMultiplier();
        public float GetExplosionRadius()
            => Attack.GetExplosionRadius();
        public float GetExplosionDamageMultiplier()
            => Attack.GetExplosionDamageMultiplier();
        public float GetRangedBasicDamageMultiplier()
            => Attack.GetRangedBasicDamageMultiplier();
        public float GetAttackCooldownMultiplier()
            => Attack.GetAttackCooldownMultiplier();
        public float GetOutgoingBasicDamageMultiplier()
            => Attack.GetOutgoingBasicDamageMultiplier();
        public float GetOutgoingBasicDamageFlatBonus()
            => Attack.GetOutgoingBasicDamageFlatBonus();
        public float GetMeleeRangeMultiplier()
            => Attack.GetMeleeRangeMultiplier();
        public float GetAccuracyPenaltyAngle()
            => Attack.GetAccuracyPenaltyAngle();
        public float RollUnstableCoreDamageMultiplier()
            => Attack.RollUnstableCoreDamageMultiplier();
        public float RollUnstableCellProjectileSpeedMultiplier(out float damageMultiplier)
            => Attack.RollUnstableCellProjectileSpeedMultiplier(out damageMultiplier);
        public void NotifyBasicAttackPerformed(float attackDamage, LayerMask mask, float range, Vector3 forwardDirection)
            => Attack.NotifyBasicAttackPerformed(attackDamage, mask, range, forwardDirection);
        public float GetIncomingDamageMultiplier()
            => Timed.GetIncomingDamageMultiplier();
        public float ProcessIncomingDamage(float damageAmount, EnemyController sourceEnemy = null)
            => Timed.ProcessIncomingDamage(damageAmount, sourceEnemy);
        public bool TryConsumeSplitRegeneration(float currentHealth, float maxHealth, out float reviveHealth)
            => Timed.TryConsumeSplitRegeneration(currentHealth, maxHealth, out reviveHealth);

        public void NotifyEnemyDefeatedByPlayer(EnemyController enemy)
            => KillEffects.NotifyDefeated(enemy);
        public float ApplyPerTargetDamageModifiers(EnemyController enemy, float damage)
            => Attack.ApplyPerTargetDamageModifiers(enemy, damage);
        public void TryApplyPostDamageExecutionInstinct(EnemyController enemy, float appliedDamage, uint? expectedGeneration = null)
            => Attack.TryApplyPostDamageExecutionInstinct(enemy, appliedDamage, expectedGeneration);
        public bool HasItem(string itemId)
            => Ownership.HasItem(itemId);
        public void ApplyCommonOnHitEffects(
            EnemyController enemy,
            float hitDamage,
            Vector3 hitPosition, uint? expectedGeneration = null)
            => Attack.ApplyCommonOnHitEffects(enemy, hitDamage, hitPosition, expectedGeneration);
        public void SpawnSplitProjectiles(Vector3 origin, Vector3 forwardDirection, float damage, LayerMask mask, float range)
            => Attack.SpawnSplitProjectiles(origin, forwardDirection, damage, mask, range);

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
        private void SpawnChildProjectile(Vector3 origin, Vector3 direction, float damage, LayerMask mask, float range, Projectile.SpawnKind spawnKind, HealthEffectSource source)
            => Attack.SpawnChildProjectile(origin, direction, damage, mask, range, spawnKind, source);
        private void HandleItemAcquired(PlayerItemManager _, PlayerItemManager.AcquiredPlayerItem acquiredItem)
            => Ownership.HandleItemAcquired(_, acquiredItem);
        private void HandleItemRemoved(PlayerItemManager _, PlayerItemManager.AcquiredPlayerItem removedItem)
            => Ownership.HandleItemRemoved(_, removedItem);
        private void RebuildItemCache()
            => Ownership.RebuildItemCache();
        public void EnsureActiveForInventoryChange()
            => Ownership.EnsureActiveForInventoryChange();
        public void CapturePersistentItemState(SavedItemStateData target)
            => Timed.CapturePersistentItemState(target);
        public void RestorePersistentItemStates(IReadOnlyList<SavedItemStateData> savedItems)
            => Timed.RestorePersistentItemStates(savedItems);
        private bool HasCachedItem(string itemId)
            => Ownership.HasCachedItem(itemId);
        private void RefreshItemRuntimeFlags()
            => Ownership.RefreshItemRuntimeFlags();
        private void RefreshDerivedUpdateFlags()
            => Ownership.RefreshDerivedUpdateFlags();
        private void ResetInactiveItemRuntimeState()
            => Ownership.ResetInactiveItemRuntimeState();
        private void SleepIfInventoryEmpty()
            => Ownership.SleepIfInventoryEmpty();
        private void UpdateBioCompanionItems()
            => Companions.UpdateBioCompanionItems();

        private void TriggerElectricNeuralNetwork(EnemyController defeatedEnemy)
            => KillEffects.TriggerElectricNeuralNetwork(defeatedEnemy);



        private void TriggerInfectionTransference(EnemyController defeatedEnemy)
            => KillEffects.TriggerInfectionTransference(defeatedEnemy);

        private void TriggerMacrophageBuff()
            => KillEffects.TriggerMacrophageBuff();

        private void TriggerGluttonousOrganBuff()
            => KillEffects.TriggerGluttonousOrganBuff();

        private void TriggerParasiticBomb()
            => KillEffects.TriggerParasiticBomb();

        private void UpdateKillChainBuffs()
            => KillEffects.UpdateKillChainBuffs();
        private void UpdateBossFightItemStates()
            => Timed.UpdateBossFightItemStates();
        private void UpdateMutationChaosItems()
            => Timed.UpdateMutationChaosItems();
        private void UpdateGrotesqueGrowthState()
            => Timed.UpdateGrotesqueGrowthState();
        private void ClearGrotesqueGrowthState()
            => Timed.ClearGrotesqueGrowthState();
        private void UpdateMutationRampageState()
            => Timed.UpdateMutationRampageState();
        private void ApplyRandomMutationRampageModifier()
            => Timed.ApplyRandomMutationRampageModifier();
        private void ClearMutationRampageState()
            => Timed.ClearMutationRampageState();
        private void UpdateFrenzyHormoneState()
            => Timed.UpdateFrenzyHormoneState();
        private void ApplyFrenzyHormoneBuff()
            => Timed.ApplyFrenzyHormoneBuff();
        private void ClearFrenzyHormoneState()
            => Timed.ClearFrenzyHormoneState();
        private void UpdateFocusedNerveScan()
            => Timed.UpdateFocusedNerveScan();
        private void UpdateBerserkCellState()
            => Timed.UpdateBerserkCellState();
        private float GetFocusedNerveAttackBonus()
            => Timed.GetFocusedNerveAttackBonus();
        private void ActivateBerserkCellBuff()
            => Timed.ActivateBerserkCellBuff();
        private void ClearBerserkCellBuff()
            => Timed.ClearBerserkCellBuff();
        private bool IsBerserkCellActive()
            => Timed.IsBerserkCellActive();

        private void ApplyKillChainStatBuff(object source, CharacterStatType statType, ref float appliedValue, float value)
            => KillEffects.ApplyKillChainStatBuff(source, statType, ref appliedValue, value);

        private void ClearKillChainStatBuff(object source, ref float appliedValue)
            => KillEffects.ClearKillChainStatBuff(source, ref appliedValue);

        private void SpawnElectricChainVisual(Vector3 start, Vector3 end)
        {
            SpawnLineVisual(
                "ElectricChainVisual",
                start + Vector3.up * 0.45f,
                end + Vector3.up * 0.45f,
                Color.white,
                Color.white,
                0.24f,
                0.18f,
                0.18f,
                ElectricNeuralNetworkEffectPath);
        }

        private void SpawnInfectionTransferVisual(Vector3 start, Vector3 end)
        {
            SpawnLineVisual(
                "InfectionTransferVisual",
                start + Vector3.up * 0.35f,
                end + Vector3.up * 0.35f,
                Color.white,
                Color.white,
                0.2f,
                0.14f,
                0.25f,
                InfectionTransferenceEffectPath);
        }

        private void SpawnParasiticBombVisual(Vector3 center, float radius)
        {
            SpawnPooledCircleVisual(
                ParasiticBombVisualPoolName,
                "ParasiticBombVisual",
                new Vector3(center.x, center.y + 1.35f, center.z),
                Mathf.Max(0.2f, radius * 2f),
                Color.white,
                1100,
                0.22f,
                TextureSpriteCache.LoadResourceSprite(ParasiticBombEffectPath));
        }

        private void SpawnLineVisual(
            string objectName,
            Vector3 start,
            Vector3 end,
            Color startColor,
            Color endColor,
            float startWidth,
            float endWidth,
            float duration,
            string textureResourcePath = null,
            int sortingOrder = 5230)
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
            line.sharedMaterial = string.IsNullOrWhiteSpace(textureResourcePath)
                ? GetRuntimeLineMaterial()
                : TextureSpriteCache.GetResourceSpriteMaterial(textureResourcePath);
            line.textureMode = LineTextureMode.Stretch;
            line.alignment = LineAlignment.View;
            line.numCapVertices = 0;
            line.startColor = startColor;
            line.endColor = endColor;
            line.sortingOrder = sortingOrder;

            RuntimePool.EnsureAutoReturn(lineObject)?.Schedule(Mathf.Max(0.02f, duration));
        }
        private void UpdateSporeColony()
            => Companions.UpdateSporeColony();

        private void UpdateBloodDrone()
            => Companions.UpdateBloodDrone();

        private void ReleaseBloodDrone()
            => Companions.ReleaseBloodDrone();

        private void UpdateGuardianOrgan()
            => Companions.UpdateGuardianOrgan();

        private void ReleaseGuardianOrgan()
            => Companions.ReleaseGuardianOrgan();
        private void UpdateTentacleColony()
            => Companions.UpdateTentacleColony();
        private void TrySpawnInfectedHost(EnemyController defeatedEnemy)
            => Companions.TrySpawnInfectedHost(defeatedEnemy);
        private void SpawnSporeSummon()
            => Companions.SpawnSporeSummon();

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
            => Companions.SpawnBioSummon(kind, position, sprite, attackSprites, visualScale, color, damage, lifetime, searchRadius, attackRadius, attackInterval, destroyOnAttack);
        private void ApplyTentacleBind()
            => Companions.ApplyTentacleBind();

        private void SpawnTentacleBindVisual(Vector3 targetPosition, float duration)
        {
            Vector3 anchorPosition = GetPlayerVisualCenter() + Vector3.up * 0.15f;

            SpawnPooledCircleVisual(
                TentacleBindVisualPoolName,
                "TentacleColonyBind",
                targetPosition + Vector3.up * 0.4f,
                1.45f,
                Color.white,
                5225,
                duration,
                TextureSpriteCache.LoadResourceSprite(TentacleColonyEffectPath));

            SpawnLineVisual(
                "TentacleColonyLine",
                anchorPosition,
                targetPosition + Vector3.up * 0.35f,
                Color.white,
                Color.white,
                0.32f,
                0.2f,
                duration,
                TentacleColonyConnectionEffectPath,
                4900);

        }

        private void CleanupBioSummonList()
            => Companions.CleanupBioSummonList();

        private int CountBioSummons(PlayerBioSummon.SummonKind kind)
            => Companions.CountBioSummons(kind);

        private void ClearBioCompanions()
            => Companions.ClearBioCompanions();
        private bool CanInfectEnemy(EnemyController enemy)
            => Queries.CanInfectEnemy(enemy);
        private float GetCompanionBaseDamage()
            => Queries.GetCompanionBaseDamage();
        private EnemyController FindClosestEnemy(Vector3 center, float radius, List<EnemyController> excluded = null)
            => Queries.FindClosestEnemy(center, radius, excluded);
        private int CountEnemiesWithinRadius(Vector3 center, float radius)
            => Queries.CountEnemiesWithinRadius(center, radius);
        private bool IsBossOrEliteWithinRadius(float radius)
            => Queries.IsBossOrEliteWithinRadius(radius);
        private EnemyController FindNearestBossLikeEnemy(float radius)
            => Queries.FindNearestBossLikeEnemy(radius);
        private bool IsEnemyAboveHealthRatio(EnemyController enemy, float ratio)
            => Queries.IsEnemyAboveHealthRatio(enemy, ratio);
        private bool IsEnemyBelowHealthRatio(EnemyController enemy, float ratio)
            => Queries.IsEnemyBelowHealthRatio(enemy, ratio);
        private float GetEnemyCurrentHealth(EnemyController enemy)
            => Queries.GetEnemyCurrentHealth(enemy);
        private bool IsEnemyTargetable(EnemyController enemy)
            => Queries.IsEnemyTargetable(enemy);
        private bool IsBossLikeEnemy(EnemyController enemy)
            => Queries.IsBossLikeEnemy(enemy);
        private Sprite FindEnemySprite(EnemyController enemy)
            => Queries.FindEnemySprite(enemy);
        private Sprite[] FindEnemyAttackSprites(EnemyController enemy)
            => Queries.FindEnemyAttackSprites(enemy);

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

        internal static void SpawnPooledCircleVisual(
            string poolName,
            string objectName,
            Vector3 position,
            float scale,
            Color color,
            int sortingOrder,
            float duration,
            Sprite effectSprite = null)
            => ItemEffectVisuals.SpawnPooledCircleVisual(poolName, objectName, position, scale, color, sortingOrder, duration, effectSprite);



        private static GameObject CreateLineVisualObject()
        {
            GameObject obj = new GameObject("PlayerItemLineVisual");
            LineRenderer line = obj.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.sharedMaterial = GetRuntimeLineMaterial();
            return obj;
        }









        private void ReleaseBioSummon(PlayerBioSummon summon)
            => Companions.ReleaseBioSummon(summon);

        private static SpriteRenderer GetOrAddSpriteRenderer(GameObject obj)
            => ItemEffectVisuals.GetOrAddSpriteRenderer(obj);

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
        private Vector3 FindEnemyVisualScale(EnemyController enemy, Vector3 fallback)
            => Queries.FindEnemyVisualScale(enemy, fallback);
        private Vector3 ResolveGroundSpawnPosition(Vector3 desiredPosition, float heightOffset)
            => Queries.ResolveGroundSpawnPosition(desiredPosition, heightOffset);
        private PlayerProjectilePool ResolvePool()
            => Queries.ResolvePool();
        private EnemyStatusEffectController EnsureStatusController(EnemyController enemy)
            => Queries.EnsureStatusController(enemy);
        private Vector3 GetMovementAnchorPosition()
            => Timed.GetMovementAnchorPosition();
        private void UpdateMovementIntensity()
            => Timed.UpdateMovementIntensity();
        private void UpdateRampageBloodFlowState()
            => Timed.UpdateRampageBloodFlowState();
        private void UpdateTentacleAutoAttack()
            => Attack.UpdateTentacleAutoAttack();
        private void UpdateDecayStates()
            => Attack.UpdateDecayStates();
        private void UpdateDecayOrganState()
            => Timed.UpdateDecayOrganState();
        private void UpdateRuptureMuscleState()
            => Timed.UpdateRuptureMuscleState();
        private void UpdateImperfectRegenState()
            => Timed.UpdateImperfectRegenState();
        private void ApplyRuptureMovePenalty(float penalty)
            => Timed.ApplyRuptureMovePenalty(penalty);
        private void SyncPersistentStatModifiers()
            => Timed.SyncPersistentStatModifiers();
        private void SetSimpleModifierState(
            bool shouldApply,
            ref bool appliedFlag,
            object source,
            CharacterStatModifier modifier)
            => Timed.SetSimpleModifierState(shouldApply, ref appliedFlag, source, modifier);
        private void SetSimpleModifierState(
            bool shouldApply,
            ref bool appliedFlag,
            object source,
            CharacterStatModifier firstModifier,
            CharacterStatModifier secondModifier)
            => Timed.SetSimpleModifierState(shouldApply, ref appliedFlag, source, firstModifier, secondModifier);
        private void ClearPersistentStatModifiers()
            => Ownership.ClearPersistentStatModifiers();

        private void TrySubscribeHealthEvents()
        {
            if (subscribedHealthEvents || playerStats == null || playerStats.RuntimeStats == null)
            {
                return;
            }

            playerStats.HealthChanged += Timed.HandlePlayerHealthChanged;
            subscribedHealthEvents = true;
        }

        private void TryUnsubscribeHealthEvents()
        {
            if (!subscribedHealthEvents || playerStats == null)
            {
                return;
            }

            playerStats.HealthChanged -= Timed.HandlePlayerHealthChanged;
            subscribedHealthEvents = false;
        }
        private void HandlePlayerHealthChanged(CharacterStats _, CharacterHealthChangedEventArgs args)
            => Timed.HandlePlayerHealthChanged(_, args);
        private void TriggerOverheatExplosionSelfDamage(float attackDamage)
            => Attack.TriggerOverheatExplosionSelfDamage(attackDamage);
        private void SpawnVoidCellProjectile(float attackDamage, LayerMask mask, float range, Vector3 fallbackDirection)
            => Attack.SpawnVoidCellProjectile(attackDamage, mask, range, fallbackDirection);
        private EnemyController FindRandomEnemyWithinRadius(float radius)
            => Attack.FindRandomEnemyWithinRadius(radius);

        private void UpdateUnstableCoreOverlay()
        {
            if (!Ownership.hasUnstableCoreItem)
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

            float multiplier = Attack.RollUnstableCoreDamageMultiplier();
            float minMultiplier = Mathf.Max(0f, unstableCoreMinAttackRatio);
            float maxMultiplier = Mathf.Max(minMultiplier, unstableCoreMaxAttackRatio);
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
            if (!Ownership.hasPlateletMembraneItem || Timed.shieldRuntime.Shield <= 0f)
            {
                ClearPlateletMembraneOutline();
                return;
            }

            EnsurePlateletMembraneOutlineMaterial();

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

            if (plateletMembraneOutlineMaterial != null)
            {
                plateletMembraneOutlineMaterial.SetColor(OutlineColorId, new Color(0.72f, 0.74f, 0.76f, 1f));
                plateletMembraneOutlineMaterial.SetFloat(OutlineSizeId, 1.25f);
                plateletMembraneOutlineMaterial.SetFloat(OutlineExpandId, 0f);
                EnsurePlateletMembraneOutlineRenderer(playerVisualSpriteRenderer);
                if (plateletMembraneOutlineRenderer != null)
                {
                    plateletMembraneOutlineRenderer.sprite = playerVisualSpriteRenderer.sprite;
                    plateletMembraneOutlineRenderer.flipX = playerVisualSpriteRenderer.flipX;
                    plateletMembraneOutlineRenderer.flipY = playerVisualSpriteRenderer.flipY;
                    plateletMembraneOutlineRenderer.color = Color.white;
                    plateletMembraneOutlineRenderer.sortingLayerID = playerVisualSpriteRenderer.sortingLayerID;
                    plateletMembraneOutlineRenderer.sortingOrder = playerVisualSpriteRenderer.sortingOrder - 1;
                    plateletMembraneOutlineRenderer.sharedMaterial = plateletMembraneOutlineMaterial;
                    plateletMembraneOutlineRenderer.enabled = true;
                }
            }

            EnsurePlateletMembraneEffectRenderer(playerVisualSpriteRenderer);
            if (plateletMembraneEffectRenderer != null)
            {
                Sprite membraneSprite = plateletMembraneEffectRenderer.sprite;
                float targetWorldSize = Mathf.Max(
                    playerVisualSpriteRenderer.bounds.size.x,
                    playerVisualSpriteRenderer.bounds.size.y) * 1.35f;
                Vector3 parentScale = playerVisualSpriteRenderer.transform.lossyScale;
                float parentUniformScale = Mathf.Max(
                    0.0001f,
                    Mathf.Max(Mathf.Abs(parentScale.x), Mathf.Abs(parentScale.y)));
                float worldScale = TextureSpriteCache.GetUniformScaleForWorldSize(membraneSprite, targetWorldSize);
                plateletMembraneEffectRenderer.transform.localScale = Vector3.one * (worldScale / parentUniformScale);
                plateletMembraneEffectRenderer.sortingLayerID = playerVisualSpriteRenderer.sortingLayerID;
                plateletMembraneEffectRenderer.sortingOrder = playerVisualSpriteRenderer.sortingOrder + 2;
                plateletMembraneEffectRenderer.color = Color.white;
                plateletMembraneEffectRenderer.enabled = true;
            }
        }

        private void EnsurePlateletMembraneOutlineMaterial()
        {
            if (plateletMembraneOutlineMaterial != null)
            {
                return;
            }

            Material template = Resources.Load<Material>("ItemEffects/PlateletMembraneOutline");
            if (template == null || template.shader == null)
            {
                if (!plateletMembraneShaderWarningLogged)
                {
                    Debug.LogWarning("[PlayerItemCombatEffects] PlateletMembraneOutline material resource not found.");
                    plateletMembraneShaderWarningLogged = true;
                }

                return;
            }

            plateletMembraneOutlineMaterial = new Material(template)
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

            if (plateletMembraneEffectRenderer != null)
            {
                plateletMembraneEffectRenderer.enabled = false;
            }

            plateletMembraneOutlineRenderer = null;
            plateletMembraneEffectRenderer = null;
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

        private void EnsurePlateletMembraneEffectRenderer(SpriteRenderer sourceRenderer)
        {
            if (sourceRenderer == null || plateletMembraneEffectRenderer != null)
            {
                return;
            }

            Transform existing = sourceRenderer.transform.Find("PlateletMembraneVisual");
            plateletMembraneEffectRenderer = existing != null ? existing.GetComponent<SpriteRenderer>() : null;
            if (plateletMembraneEffectRenderer == null)
            {
                GameObject effectObject = new GameObject("PlateletMembraneVisual");
                effectObject.transform.SetParent(sourceRenderer.transform, false);
                effectObject.transform.localPosition = Vector3.zero;
                effectObject.transform.localRotation = Quaternion.identity;
                plateletMembraneEffectRenderer = effectObject.AddComponent<SpriteRenderer>();
            }

            plateletMembraneEffectRenderer.sprite = TextureSpriteCache.LoadResourceSprite(PlateletMembraneEffectPath);
            plateletMembraneEffectRenderer.enabled = plateletMembraneEffectRenderer.sprite != null;
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
                    || childName == "PlateletMembraneVisual"
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
            if (playerVisualSpriteRenderer != null)
            {
                return playerVisualSpriteRenderer;
            }

            playerSpriteRendererBuffer.Clear();
            GetComponentsInChildren(true, playerSpriteRendererBuffer);
            for (int i = 0; i < playerSpriteRendererBuffer.Count; i++)
            {
                SpriteRenderer renderer = playerSpriteRendererBuffer[i];
                if (renderer == null)
                {
                    continue;
                }

                string objectName = renderer.gameObject.name;
                if (objectName.Contains("Overlay") || objectName.Contains("Outline"))
                {
                    continue;
                }

                playerVisualSpriteRenderer = renderer;
                return playerVisualSpriteRenderer;
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
                Mathf.Max(0.2f, overheatExplosionRadius * 2f),
                Color.white,
                5300,
                0.2f,
                TextureSpriteCache.LoadResourceSprite(OverheatedOrganEffectPath));
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








    }
}
