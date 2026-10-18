using UnityEngine;
namespace Necrocis
{
    internal interface IItemCombatSettings
    {
        float doubleShotSpreadAngle { get; }
        float tripleShotSpreadAngle { get; }
        float doubleShotDamageMultiplier { get; }
        float tripleShotDamageMultiplier { get; }
        float backShotDamageMultiplier { get; }
        float homingTurnRate { get; }
        float homingSearchRadius { get; }
        float boomerangReturnDistance { get; }
        float boomerangRepeatHitDamageMultiplier { get; }
        int boomerangMaxHitsPerPass { get; }
        int piercingHitCount { get; }
        float piercingSubsequentHitDamageMultiplier { get; }
        int reflectionBounceCount { get; }
        float hypertrophyScaleMultiplier { get; }
        float hypertrophyRangeMultiplier { get; }
        float pulseGrowEndScale { get; }
        float pulseShrinkEndScale { get; }
        float splitDamageMultiplier { get; }
        float splitRangeMultiplier { get; }
        float splitAngle { get; }
        float explosionRadius { get; }
        float explosionDamageMultiplier { get; }
        float explosiveBloodCellRangedDamageMultiplier { get; }
        float acidDuration { get; }
        float acidTickInterval { get; }
        float acidTickDamage { get; }
        float poisonDuration { get; }
        float poisonTickInterval { get; }
        float poisonTickDamageRatio { get; }
        float freezeSlowRatio { get; }
        float freezeDuration { get; }
        float bleedDuration { get; }
        float bleedTickInterval { get; }
        float bleedTickDamageRatio { get; }
        float cellProliferationChance { get; }
        float cellProliferationDamageMultiplier { get; }
        float beamRadius { get; }
        float beamDamageMultiplier { get; }
        int beamHitBufferSize { get; }
        int overheatMaxStacks { get; }
        float overheatStackWindow { get; }
        float overheatPerStackAttackSpeedBonus { get; }
        float overheatDecayInterval { get; }
        float overheatExplosionRadius { get; }
        float overheatExplosionDamageMultiplier { get; }
        float overheatExplosionSelfDamage { get; }
        float mutantEyeAccuracyPenaltyAngle { get; }
        float mutantEyeFlatDamageBonus { get; }
        float organTentacleAutoAttackInterval { get; }
        float organTentacleAutoAttackRadius { get; }
        float organTentacleAutoAttackDamageMultiplier { get; }
        int organTentacleMaxTargets { get; }
        float organTentacleBaseAttackPenaltyMultiplier { get; }
        float rampageMoveSecondsPerBonus { get; }
        int rampageMaxAttackBonus { get; }
        float rampageIdleSecondsPerDecay { get; }
        float muscleSpasmMeleeRangeMultiplier { get; }
        float muscleSpasmAttackSpeedMultiplier { get; }
        float unstableCoreMinAttackRatio { get; }
        float unstableCoreMaxAttackRatio { get; }
        float unstableCoreRerollInterval { get; }
        float bioResonanceStackDamageBonus { get; }
        int bioResonanceMaxStacks { get; }
        float bioResonanceStackWindow { get; }
        float bloodPressureBaseAttackSpeedAdd { get; }
        float bloodPressurePerTenPercentMissingAdd { get; }
        float voidCellChance { get; }
        float voidCellDamageMultiplier { get; }
        float voidCellSpawnRadius { get; }
        float forbiddenGrowthMaxHealthPenalty { get; }
        float forbiddenGrowthFlatAttackBonus { get; }
        float overclockNerveMaxHealthPenalty { get; }
        float overclockNerveFlatMoveSpeedBonus { get; }
        float bloodContractIncomingDamageMultiplier { get; }
        int bloodContractKillsPerHeal { get; }
        float bloodContractHealthGainAmount { get; }
        int bloodContractMaxHealthGainCount { get; }
        float hyperplasiaMissingHealthAttackBonusMax { get; }
        float decayOrganSecondsPerAttackBonus { get; }
        int decayOrganMaxAttackBonus { get; }
        float ruptureMuscleAttackBonusPerStack { get; }
        float ruptureMuscleMovePenaltyPerStack { get; }
        int ruptureMuscleMaxStacks { get; }
        float ruptureMuscleDecayDelay { get; }
        float imperfectRegenMaxHealthPenalty { get; }
        float imperfectRegenDelay { get; }
        float imperfectRegenHealPerTrigger { get; }
        float imperfectRegenCooldownDuration { get; }
        float severanceReflexDuration { get; }
        float severanceReflexFlatAttackBonus { get; }
        float exoskeletonDamageReductionRatio { get; }
        float exoskeletonMoveSpeedPenalty { get; }
        float plateletMembraneInterval { get; }
        float plateletMembraneShieldAmount { get; }
        float recoveryFactorInterval { get; }
        float recoveryFactorHealAmount { get; }
        float reflectiveSkinDamageRatio { get; }
        float bioBarrierIdleSecondsPerStep { get; }
        float bioBarrierReductionPerStep { get; }
        float bioBarrierMaxReduction { get; }
        float splitRegenerationReviveHealth { get; }
        float infectedHostChance { get; }
        float infectedHostLifetime { get; }
        float infectedHostDamageMultiplier { get; }
        float infectedHostAttackInterval { get; }
        float infectedHostAttackRadius { get; }
        float infectedHostSearchRadius { get; }
        int infectedHostMaxAllies { get; }
        float sporeSpawnInterval { get; }
        float sporeLifetime { get; }
        float sporeDamageMultiplier { get; }
        float sporeSearchRadius { get; }
        float sporeBurstRadius { get; }
        float bloodDroneOrbitRadius { get; }
        float bloodDroneFireInterval { get; }
        float bloodDroneDamageMultiplier { get; }
        float bloodDroneTargetRadius { get; }
        float guardianOrganOrbitRadius { get; }
        float guardianOrganBlockRadius { get; }
        float guardianOrganCooldown { get; }
        float tentacleBindInterval { get; }
        float tentacleBindRadius { get; }
        float tentacleBindDuration { get; }
        float tentacleSlowRatio { get; }
        float tentacleDamageMultiplier { get; }
        int tentacleMaxTargets { get; }
        float electricChainDamage { get; }
        float electricChainRadius { get; }
        int electricChainMaxHits { get; }
        float electricChainCooldown { get; }
        float infectionTransferRadius { get; }
        int infectionTransferMaxTargets { get; }
        float infectionTransferDuration { get; }
        float infectionTransferTickInterval { get; }
        float infectionTransferTickDamage { get; }
        float infectionTransferCooldown { get; }
        float macrophageDuration { get; }
        float macrophageAttackBonusPerStack { get; }
        int macrophageMaxStacks { get; }
        float gluttonousOrganDuration { get; }
        float gluttonousOrganMoveBonusPerStack { get; }
        int gluttonousOrganMaxStacks { get; }
        float heartSniperHealthThreshold { get; }
        float heartSniperDamageBonusRatio { get; }
        float bloodflowAccelerationRadius { get; }
        float bloodflowAccelerationAttackSpeedBonus { get; }
        float focusedNerveRadius { get; }
        float focusedNerveHighAttackBonus { get; }
        float focusedNerveLowAttackBonus { get; }
        float executionInstinctHealthThreshold { get; }
        float executionInstinctChance { get; }
        float executionInstinctBossDamageMultiplier { get; }
        float berserkCellDuration { get; }
        float berserkCellAttackBonus { get; }
        float berserkCellMoveBonus { get; }
        float berserkCellAttackSpeedBonus { get; }
        float berserkCellBossDetectionRadius { get; }
        float unstableCellMinProjectileSpeedMultiplier { get; }
        float unstableCellMaxProjectileSpeedMultiplier { get; }
        float unstableCellSlowThreshold { get; }
        float unstableCellSlowDamageMultiplier { get; }
        float grotesqueGrowthInterval { get; }
        float grotesqueGrowthSmallScale { get; }
        float grotesqueGrowthLargeScale { get; }
        float grotesqueGrowthSmallMoveBonus { get; }
        float grotesqueGrowthLargeAttackBonus { get; }
        float grotesqueGrowthLargeMovePenalty { get; }
        float mutationRampageInterval { get; }
        float mutationRampageDuration { get; }
        float mutationRampageBuffChance { get; }
        float mutationRampageAttackBuff { get; }
        float mutationRampageMoveBuff { get; }
        float mutationRampageAttackSpeedBuff { get; }
        float mutationRampageAttackDebuff { get; }
        float mutationRampageMoveDebuff { get; }
        float mutationRampageAttackSpeedDebuff { get; }
        float parasiticBombChance { get; }
        float parasiticBombRadius { get; }
        float parasiticBombDamageMultiplier { get; }
        float frenzyHormoneDuration { get; }
        float frenzyHormoneCooldown { get; }
        float frenzyHormoneMinBonus { get; }
        float frenzyHormoneMaxBonus { get; }
    }
    public partial class PlayerItemCombatEffects : IItemCombatSettings
    {
        float IItemCombatSettings.doubleShotSpreadAngle => doubleShotSpreadAngle;
        float IItemCombatSettings.tripleShotSpreadAngle => tripleShotSpreadAngle;
        float IItemCombatSettings.doubleShotDamageMultiplier => doubleShotDamageMultiplier;
        float IItemCombatSettings.tripleShotDamageMultiplier => tripleShotDamageMultiplier;
        float IItemCombatSettings.backShotDamageMultiplier => backShotDamageMultiplier;
        float IItemCombatSettings.homingTurnRate => homingTurnRate;
        float IItemCombatSettings.homingSearchRadius => homingSearchRadius;
        float IItemCombatSettings.boomerangReturnDistance => boomerangReturnDistance;
        float IItemCombatSettings.boomerangRepeatHitDamageMultiplier => boomerangRepeatHitDamageMultiplier;
        int IItemCombatSettings.boomerangMaxHitsPerPass => boomerangMaxHitsPerPass;
        int IItemCombatSettings.piercingHitCount => piercingHitCount;
        float IItemCombatSettings.piercingSubsequentHitDamageMultiplier => piercingSubsequentHitDamageMultiplier;
        int IItemCombatSettings.reflectionBounceCount => reflectionBounceCount;
        float IItemCombatSettings.hypertrophyScaleMultiplier => hypertrophyScaleMultiplier;
        float IItemCombatSettings.hypertrophyRangeMultiplier => hypertrophyRangeMultiplier;
        float IItemCombatSettings.pulseGrowEndScale => pulseGrowEndScale;
        float IItemCombatSettings.pulseShrinkEndScale => pulseShrinkEndScale;
        float IItemCombatSettings.splitDamageMultiplier => splitDamageMultiplier;
        float IItemCombatSettings.splitRangeMultiplier => splitRangeMultiplier;
        float IItemCombatSettings.splitAngle => splitAngle;
        float IItemCombatSettings.explosionRadius => explosionRadius;
        float IItemCombatSettings.explosionDamageMultiplier => explosionDamageMultiplier;
        float IItemCombatSettings.explosiveBloodCellRangedDamageMultiplier => explosiveBloodCellRangedDamageMultiplier;
        float IItemCombatSettings.acidDuration => acidDuration;
        float IItemCombatSettings.acidTickInterval => acidTickInterval;
        float IItemCombatSettings.acidTickDamage => acidTickDamage;
        float IItemCombatSettings.poisonDuration => poisonDuration;
        float IItemCombatSettings.poisonTickInterval => poisonTickInterval;
        float IItemCombatSettings.poisonTickDamageRatio => poisonTickDamageRatio;
        float IItemCombatSettings.freezeSlowRatio => freezeSlowRatio;
        float IItemCombatSettings.freezeDuration => freezeDuration;
        float IItemCombatSettings.bleedDuration => bleedDuration;
        float IItemCombatSettings.bleedTickInterval => bleedTickInterval;
        float IItemCombatSettings.bleedTickDamageRatio => bleedTickDamageRatio;
        float IItemCombatSettings.cellProliferationChance => cellProliferationChance;
        float IItemCombatSettings.cellProliferationDamageMultiplier => cellProliferationDamageMultiplier;
        float IItemCombatSettings.beamRadius => beamRadius;
        float IItemCombatSettings.beamDamageMultiplier => beamDamageMultiplier;
        int IItemCombatSettings.beamHitBufferSize => beamHitBufferSize;
        int IItemCombatSettings.overheatMaxStacks => overheatMaxStacks;
        float IItemCombatSettings.overheatStackWindow => overheatStackWindow;
        float IItemCombatSettings.overheatPerStackAttackSpeedBonus => overheatPerStackAttackSpeedBonus;
        float IItemCombatSettings.overheatDecayInterval => overheatDecayInterval;
        float IItemCombatSettings.overheatExplosionRadius => overheatExplosionRadius;
        float IItemCombatSettings.overheatExplosionDamageMultiplier => overheatExplosionDamageMultiplier;
        float IItemCombatSettings.overheatExplosionSelfDamage => overheatExplosionSelfDamage;
        float IItemCombatSettings.mutantEyeAccuracyPenaltyAngle => mutantEyeAccuracyPenaltyAngle;
        float IItemCombatSettings.mutantEyeFlatDamageBonus => mutantEyeFlatDamageBonus;
        float IItemCombatSettings.organTentacleAutoAttackInterval => organTentacleAutoAttackInterval;
        float IItemCombatSettings.organTentacleAutoAttackRadius => organTentacleAutoAttackRadius;
        float IItemCombatSettings.organTentacleAutoAttackDamageMultiplier => organTentacleAutoAttackDamageMultiplier;
        int IItemCombatSettings.organTentacleMaxTargets => organTentacleMaxTargets;
        float IItemCombatSettings.organTentacleBaseAttackPenaltyMultiplier => organTentacleBaseAttackPenaltyMultiplier;
        float IItemCombatSettings.rampageMoveSecondsPerBonus => rampageMoveSecondsPerBonus;
        int IItemCombatSettings.rampageMaxAttackBonus => rampageMaxAttackBonus;
        float IItemCombatSettings.rampageIdleSecondsPerDecay => rampageIdleSecondsPerDecay;
        float IItemCombatSettings.muscleSpasmMeleeRangeMultiplier => muscleSpasmMeleeRangeMultiplier;
        float IItemCombatSettings.muscleSpasmAttackSpeedMultiplier => muscleSpasmAttackSpeedMultiplier;
        float IItemCombatSettings.unstableCoreMinAttackRatio => unstableCoreMinAttackRatio;
        float IItemCombatSettings.unstableCoreMaxAttackRatio => unstableCoreMaxAttackRatio;
        float IItemCombatSettings.unstableCoreRerollInterval => unstableCoreRerollInterval;
        float IItemCombatSettings.bioResonanceStackDamageBonus => bioResonanceStackDamageBonus;
        int IItemCombatSettings.bioResonanceMaxStacks => bioResonanceMaxStacks;
        float IItemCombatSettings.bioResonanceStackWindow => bioResonanceStackWindow;
        float IItemCombatSettings.bloodPressureBaseAttackSpeedAdd => bloodPressureBaseAttackSpeedAdd;
        float IItemCombatSettings.bloodPressurePerTenPercentMissingAdd => bloodPressurePerTenPercentMissingAdd;
        float IItemCombatSettings.voidCellChance => voidCellChance;
        float IItemCombatSettings.voidCellDamageMultiplier => voidCellDamageMultiplier;
        float IItemCombatSettings.voidCellSpawnRadius => voidCellSpawnRadius;
        float IItemCombatSettings.forbiddenGrowthMaxHealthPenalty => forbiddenGrowthMaxHealthPenalty;
        float IItemCombatSettings.forbiddenGrowthFlatAttackBonus => forbiddenGrowthFlatAttackBonus;
        float IItemCombatSettings.overclockNerveMaxHealthPenalty => overclockNerveMaxHealthPenalty;
        float IItemCombatSettings.overclockNerveFlatMoveSpeedBonus => overclockNerveFlatMoveSpeedBonus;
        float IItemCombatSettings.bloodContractIncomingDamageMultiplier => bloodContractIncomingDamageMultiplier;
        int IItemCombatSettings.bloodContractKillsPerHeal => bloodContractKillsPerHeal;
        float IItemCombatSettings.bloodContractHealthGainAmount => bloodContractHealthGainAmount;
        int IItemCombatSettings.bloodContractMaxHealthGainCount => bloodContractMaxHealthGainCount;
        float IItemCombatSettings.hyperplasiaMissingHealthAttackBonusMax => hyperplasiaMissingHealthAttackBonusMax;
        float IItemCombatSettings.decayOrganSecondsPerAttackBonus => decayOrganSecondsPerAttackBonus;
        int IItemCombatSettings.decayOrganMaxAttackBonus => decayOrganMaxAttackBonus;
        float IItemCombatSettings.ruptureMuscleAttackBonusPerStack => ruptureMuscleAttackBonusPerStack;
        float IItemCombatSettings.ruptureMuscleMovePenaltyPerStack => ruptureMuscleMovePenaltyPerStack;
        int IItemCombatSettings.ruptureMuscleMaxStacks => ruptureMuscleMaxStacks;
        float IItemCombatSettings.ruptureMuscleDecayDelay => ruptureMuscleDecayDelay;
        float IItemCombatSettings.imperfectRegenMaxHealthPenalty => imperfectRegenMaxHealthPenalty;
        float IItemCombatSettings.imperfectRegenDelay => imperfectRegenDelay;
        float IItemCombatSettings.imperfectRegenHealPerTrigger => imperfectRegenHealPerTrigger;
        float IItemCombatSettings.imperfectRegenCooldownDuration => imperfectRegenCooldownDuration;
        float IItemCombatSettings.severanceReflexDuration => severanceReflexDuration;
        float IItemCombatSettings.severanceReflexFlatAttackBonus => severanceReflexFlatAttackBonus;
        float IItemCombatSettings.exoskeletonDamageReductionRatio => exoskeletonDamageReductionRatio;
        float IItemCombatSettings.exoskeletonMoveSpeedPenalty => exoskeletonMoveSpeedPenalty;
        float IItemCombatSettings.plateletMembraneInterval => plateletMembraneInterval;
        float IItemCombatSettings.plateletMembraneShieldAmount => plateletMembraneShieldAmount;
        float IItemCombatSettings.recoveryFactorInterval => recoveryFactorInterval;
        float IItemCombatSettings.recoveryFactorHealAmount => recoveryFactorHealAmount;
        float IItemCombatSettings.reflectiveSkinDamageRatio => reflectiveSkinDamageRatio;
        float IItemCombatSettings.bioBarrierIdleSecondsPerStep => bioBarrierIdleSecondsPerStep;
        float IItemCombatSettings.bioBarrierReductionPerStep => bioBarrierReductionPerStep;
        float IItemCombatSettings.bioBarrierMaxReduction => bioBarrierMaxReduction;
        float IItemCombatSettings.splitRegenerationReviveHealth => splitRegenerationReviveHealth;
        float IItemCombatSettings.infectedHostChance => infectedHostChance;
        float IItemCombatSettings.infectedHostLifetime => infectedHostLifetime;
        float IItemCombatSettings.infectedHostDamageMultiplier => infectedHostDamageMultiplier;
        float IItemCombatSettings.infectedHostAttackInterval => infectedHostAttackInterval;
        float IItemCombatSettings.infectedHostAttackRadius => infectedHostAttackRadius;
        float IItemCombatSettings.infectedHostSearchRadius => infectedHostSearchRadius;
        int IItemCombatSettings.infectedHostMaxAllies => infectedHostMaxAllies;
        float IItemCombatSettings.sporeSpawnInterval => sporeSpawnInterval;
        float IItemCombatSettings.sporeLifetime => sporeLifetime;
        float IItemCombatSettings.sporeDamageMultiplier => sporeDamageMultiplier;
        float IItemCombatSettings.sporeSearchRadius => sporeSearchRadius;
        float IItemCombatSettings.sporeBurstRadius => sporeBurstRadius;
        float IItemCombatSettings.bloodDroneOrbitRadius => bloodDroneOrbitRadius;
        float IItemCombatSettings.bloodDroneFireInterval => bloodDroneFireInterval;
        float IItemCombatSettings.bloodDroneDamageMultiplier => bloodDroneDamageMultiplier;
        float IItemCombatSettings.bloodDroneTargetRadius => bloodDroneTargetRadius;
        float IItemCombatSettings.guardianOrganOrbitRadius => guardianOrganOrbitRadius;
        float IItemCombatSettings.guardianOrganBlockRadius => guardianOrganBlockRadius;
        float IItemCombatSettings.guardianOrganCooldown => guardianOrganCooldown;
        float IItemCombatSettings.tentacleBindInterval => tentacleBindInterval;
        float IItemCombatSettings.tentacleBindRadius => tentacleBindRadius;
        float IItemCombatSettings.tentacleBindDuration => tentacleBindDuration;
        float IItemCombatSettings.tentacleSlowRatio => tentacleSlowRatio;
        float IItemCombatSettings.tentacleDamageMultiplier => tentacleDamageMultiplier;
        int IItemCombatSettings.tentacleMaxTargets => tentacleMaxTargets;
        float IItemCombatSettings.electricChainDamage => electricChainDamage;
        float IItemCombatSettings.electricChainRadius => electricChainRadius;
        int IItemCombatSettings.electricChainMaxHits => electricChainMaxHits;
        float IItemCombatSettings.electricChainCooldown => electricChainCooldown;
        float IItemCombatSettings.infectionTransferRadius => infectionTransferRadius;
        int IItemCombatSettings.infectionTransferMaxTargets => infectionTransferMaxTargets;
        float IItemCombatSettings.infectionTransferDuration => infectionTransferDuration;
        float IItemCombatSettings.infectionTransferTickInterval => infectionTransferTickInterval;
        float IItemCombatSettings.infectionTransferTickDamage => infectionTransferTickDamage;
        float IItemCombatSettings.infectionTransferCooldown => infectionTransferCooldown;
        float IItemCombatSettings.macrophageDuration => macrophageDuration;
        float IItemCombatSettings.macrophageAttackBonusPerStack => macrophageAttackBonusPerStack;
        int IItemCombatSettings.macrophageMaxStacks => macrophageMaxStacks;
        float IItemCombatSettings.gluttonousOrganDuration => gluttonousOrganDuration;
        float IItemCombatSettings.gluttonousOrganMoveBonusPerStack => gluttonousOrganMoveBonusPerStack;
        int IItemCombatSettings.gluttonousOrganMaxStacks => gluttonousOrganMaxStacks;
        float IItemCombatSettings.heartSniperHealthThreshold => heartSniperHealthThreshold;
        float IItemCombatSettings.heartSniperDamageBonusRatio => heartSniperDamageBonusRatio;
        float IItemCombatSettings.bloodflowAccelerationRadius => bloodflowAccelerationRadius;
        float IItemCombatSettings.bloodflowAccelerationAttackSpeedBonus => bloodflowAccelerationAttackSpeedBonus;
        float IItemCombatSettings.focusedNerveRadius => focusedNerveRadius;
        float IItemCombatSettings.focusedNerveHighAttackBonus => focusedNerveHighAttackBonus;
        float IItemCombatSettings.focusedNerveLowAttackBonus => focusedNerveLowAttackBonus;
        float IItemCombatSettings.executionInstinctHealthThreshold => executionInstinctHealthThreshold;
        float IItemCombatSettings.executionInstinctChance => executionInstinctChance;
        float IItemCombatSettings.executionInstinctBossDamageMultiplier => executionInstinctBossDamageMultiplier;
        float IItemCombatSettings.berserkCellDuration => berserkCellDuration;
        float IItemCombatSettings.berserkCellAttackBonus => berserkCellAttackBonus;
        float IItemCombatSettings.berserkCellMoveBonus => berserkCellMoveBonus;
        float IItemCombatSettings.berserkCellAttackSpeedBonus => berserkCellAttackSpeedBonus;
        float IItemCombatSettings.berserkCellBossDetectionRadius => berserkCellBossDetectionRadius;
        float IItemCombatSettings.unstableCellMinProjectileSpeedMultiplier => unstableCellMinProjectileSpeedMultiplier;
        float IItemCombatSettings.unstableCellMaxProjectileSpeedMultiplier => unstableCellMaxProjectileSpeedMultiplier;
        float IItemCombatSettings.unstableCellSlowThreshold => unstableCellSlowThreshold;
        float IItemCombatSettings.unstableCellSlowDamageMultiplier => unstableCellSlowDamageMultiplier;
        float IItemCombatSettings.grotesqueGrowthInterval => grotesqueGrowthInterval;
        float IItemCombatSettings.grotesqueGrowthSmallScale => grotesqueGrowthSmallScale;
        float IItemCombatSettings.grotesqueGrowthLargeScale => grotesqueGrowthLargeScale;
        float IItemCombatSettings.grotesqueGrowthSmallMoveBonus => grotesqueGrowthSmallMoveBonus;
        float IItemCombatSettings.grotesqueGrowthLargeAttackBonus => grotesqueGrowthLargeAttackBonus;
        float IItemCombatSettings.grotesqueGrowthLargeMovePenalty => grotesqueGrowthLargeMovePenalty;
        float IItemCombatSettings.mutationRampageInterval => mutationRampageInterval;
        float IItemCombatSettings.mutationRampageDuration => mutationRampageDuration;
        float IItemCombatSettings.mutationRampageBuffChance => mutationRampageBuffChance;
        float IItemCombatSettings.mutationRampageAttackBuff => mutationRampageAttackBuff;
        float IItemCombatSettings.mutationRampageMoveBuff => mutationRampageMoveBuff;
        float IItemCombatSettings.mutationRampageAttackSpeedBuff => mutationRampageAttackSpeedBuff;
        float IItemCombatSettings.mutationRampageAttackDebuff => mutationRampageAttackDebuff;
        float IItemCombatSettings.mutationRampageMoveDebuff => mutationRampageMoveDebuff;
        float IItemCombatSettings.mutationRampageAttackSpeedDebuff => mutationRampageAttackSpeedDebuff;
        float IItemCombatSettings.parasiticBombChance => parasiticBombChance;
        float IItemCombatSettings.parasiticBombRadius => parasiticBombRadius;
        float IItemCombatSettings.parasiticBombDamageMultiplier => parasiticBombDamageMultiplier;
        float IItemCombatSettings.frenzyHormoneDuration => frenzyHormoneDuration;
        float IItemCombatSettings.frenzyHormoneCooldown => frenzyHormoneCooldown;
        float IItemCombatSettings.frenzyHormoneMinBonus => frenzyHormoneMinBonus;
        float IItemCombatSettings.frenzyHormoneMaxBonus => frenzyHormoneMaxBonus;
    }
}
