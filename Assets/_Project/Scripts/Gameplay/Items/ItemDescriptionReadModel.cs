using System;
using System.Globalization;
using UnityEngine;

namespace Necrocis
{
    // Resolves display text only. It cannot apply an item, roll RNG or mutate its definition.
    public static class ItemDescriptionReadModel
    {
        public static string Resolve(string id, string raw, ItemDescriptionMode mode,
            Func<string, string> effectDescription, bool gambleEnabled, int gambleMin, int gambleMax)
        {
            if (!ItemDefinitions.TryGet(id, out var definition)) return raw ?? string.Empty;
            if (mode == ItemDescriptionMode.Custom || mode == ItemDescriptionMode.Legacy
                && !string.Equals(raw, definition.LegacyDescription, StringComparison.Ordinal)) return raw ?? string.Empty;
            if (string.Equals(id, PlayerItemIds.BioGambleId, StringComparison.OrdinalIgnoreCase))
                return gambleEnabled ? "레벨업 스탯 증가량이 " + Signed(gambleMin) + "~" + Signed(Math.Max(gambleMin, gambleMax)) + " 사이에서 무작위 변화"
                    : "현재 생체 도박 효과 비활성";
            return effectDescription?.Invoke(id) ?? definition.LegacyDescription;
        }
        internal static string DescribeEffect(string id, ItemDescriptionValues values)
        {
            switch(id?.ToLowerInvariant())
            {
                case PlayerItemIds.DoubleCoreId: return $"발사체 2개 발사 (각 {P(Mathf.Clamp(values.doubleShotDamageMultiplier,.05f,1f))}% 피해)";
                case PlayerItemIds.TripleCoreId: return $"발사체 3개 발사 (각 {P(Mathf.Clamp(values.tripleShotDamageMultiplier,.05f,1f))}% 피해)";
                case PlayerItemIds.HomingCellId: return $"반경 {F(Mathf.Max(.5f,values.homingSearchRadius))} 내 적을 추적하는 유도 투사체";
                case PlayerItemIds.HypertrophyCellId: return $"투사체 크기 {F(Mathf.Max(.1f,values.hypertrophyScaleMultiplier))}배 (충돌 판정도 크기에 따라 변화), 사거리 {F(Mathf.Max(.1f,values.hypertrophyRangeMultiplier))}배";
                case PlayerItemIds.RefluxOrganId: return $"왕복 경로마다 최대 {F(Mathf.Max(1,values.boomerangMaxHitsPerPass))}명 타격, 복귀 중 모든 적에게 {P(Mathf.Clamp(values.boomerangRepeatHitDamageMultiplier,.05f,1f))}% 피해";
                case PlayerItemIds.PiercingMucusId: return $"최대 {F(Mathf.Max(1,values.piercingHitCount))}명 관통, 관통할 때마다 피해 {P(1f-Mathf.Clamp(values.piercingSubsequentHitDamageMultiplier,.05f,1f))}% 감소";
                case PlayerItemIds.LaryngealNerveId: return $"뒤쪽으로 {P(Mathf.Max(.05f,values.backShotDamageMultiplier))}% 피해의 추가 투사체 발사";
                case PlayerItemIds.BeamOrganId: return $"{P(Mathf.Max(.05f,values.beamDamageMultiplier))}% 피해의 관통 광선 공격 (다중핵·후방 발사·세포 증식 적용)";
                case PlayerItemIds.SplitTissueId: return $"적 명중 시 양옆으로 각각 {P(Mathf.Max(.05f,values.splitDamageMultiplier))}% 피해의 추가 투사체 생성";
                case PlayerItemIds.ExplosiveBloodCellId: return $"원거리 피해가 {P(Mathf.Clamp(values.explosiveBloodCellRangedDamageMultiplier,.05f,1f))}%가 되고 반경 {F(Mathf.Max(.2f,values.explosionRadius))}의 범위 폭발 공격으로 변경";
                case PlayerItemIds.AcidicRuptureId: return $"적중 위치에 {F(Mathf.Max(.1f,values.acidDuration))}초간 {F(Mathf.Max(.05f,values.acidTickInterval))}초마다 {F(Mathf.Max(.1f,values.acidTickDamage))} 피해를 주는 산성 장판 생성 (동시에 최대 2개)";
                case PlayerItemIds.CellProliferationId: return $"{P(Mathf.Clamp01(values.cellProliferationChance))}% 확률로 {P(Mathf.Max(.05f,values.cellProliferationDamageMultiplier))}% 피해의 공격이 한 번 더 발동";
                case PlayerItemIds.PulseBulletId: return $"투사체가 이동하며 크기가 {F(Mathf.Max(1.2f,values.pulseGrowEndScale))}배로 증가하거나 {F(Mathf.Clamp(values.pulseShrinkEndScale,.08f,.9f))}배로 감소 (충돌 판정도 크기에 따라 변화)";
                case PlayerItemIds.VascularReflectionId: return $"벽에 최대 {F(Mathf.Max(1,values.reflectionBounceCount))}번 튕기는 반사 투사체";
                case PlayerItemIds.ToxicMucosaId: if(values.poisonDuration<=0f || values.poisonTickInterval<=0f) return "중독 효과 비활성"; return $"적중 시 {F(Mathf.Max(0f,values.poisonDuration))}초간 {F(Mathf.Max(.05f,values.poisonTickInterval))}초마다 적중 피해의 {P(Mathf.Max(0f,values.poisonTickDamageRatio))}% 중독 피해 (최소 0.05)";
                case PlayerItemIds.FreezingNerveId: return $"공격 적중 시 {F(Mathf.Max(0f,values.freezeDuration))}초간 적 이동속도 {P(Mathf.Clamp01(values.freezeSlowRatio))}% 감소";
                case PlayerItemIds.HemorrhageOrganId: if(values.bleedDuration<=0f || values.bleedTickInterval<=0f) return "출혈 효과 비활성"; return $"적중 시 {F(Mathf.Max(0f,values.bleedDuration))}초간 {F(Mathf.Max(.05f,values.bleedTickInterval))}초마다 적중 피해의 {P(Mathf.Max(0f,values.bleedTickDamageRatio))}% 출혈 피해 (최소 0.05)";
                case PlayerItemIds.OverheatedOrganId: return $"연속 공격마다 기본 공격속도 +{P(Mathf.Max(0f,values.overheatPerStackAttackSpeedBonus))}% ({F(Mathf.Max(1,values.overheatMaxStacks))}중첩 시 주변 {P(Mathf.Max(.05f,values.overheatExplosionDamageMultiplier))}% 피해, 자신 {F(Mathf.Max(.1f,values.overheatExplosionSelfDamage))} 피해)";
                case PlayerItemIds.MutantEyeId: return $"발사 방향이 좌우 {F(Mathf.Max(0f,values.mutantEyeAccuracyPenaltyAngle))}도 내에서 무작위 변화, 기본 공격 피해 +{F(Mathf.Max(0f,values.mutantEyeFlatDamageBonus))}";
                case PlayerItemIds.OrganTentacleId: return $"주변 적 최대 {F(Mathf.Max(1,values.organTentacleMaxTargets))}명을 {F(Mathf.Max(.15f,values.organTentacleAutoAttackInterval))}초마다 공격력의 {P(Mathf.Max(.05f,values.organTentacleAutoAttackDamageMultiplier))}% 피해로 공격 (최소 0.1), 기본 공격 피해 {P(1f-Mathf.Clamp(values.organTentacleBaseAttackPenaltyMultiplier,.2f,1f))}% 감소";
                case PlayerItemIds.RampageBloodFlowId: return $"이동 {F(Mathf.Max(.1f,values.rampageMoveSecondsPerBonus))}초마다 기본 공격 피해 +1 (최대 +{F(Mathf.Max(0,values.rampageMaxAttackBonus))}), 정지 시 {F(Mathf.Max(.1f,values.rampageIdleSecondsPerDecay))}초마다 감소";
                case PlayerItemIds.MuscleSpasmId: return $"근접 공격 범위 {F(Mathf.Max(1f,values.muscleSpasmMeleeRangeMultiplier))}배, 기본 공격속도 {P(1f-Mathf.Clamp(values.muscleSpasmAttackSpeedMultiplier,.25f,1f))}% 감소";
                case PlayerItemIds.UnstableCoreId: return $"기본 공격 피해 {P(Mathf.Max(0f,values.unstableCoreMinAttackRatio))}~{P(Mathf.Max(Mathf.Max(0f,values.unstableCoreMinAttackRatio),values.unstableCoreMaxAttackRatio))}% (재추첨 간격 {F(Mathf.Max(.1f,values.unstableCoreRerollInterval))}초)";
                case PlayerItemIds.BioResonanceId: return $"{F(Mathf.Max(.1f,values.bioResonanceStackWindow))}초 내 같은 적 연속 공격 시 피해 +{P(Mathf.Max(0f,values.bioResonanceStackDamageBonus))}% (최대 +{P(Mathf.Max(0f,values.bioResonanceStackDamageBonus)*Mathf.Max(1,values.bioResonanceMaxStacks))}%)";
                case PlayerItemIds.BloodPressureBurstId: return $"기본 공격속도 +{P(Mathf.Max(0f,values.bloodPressureBaseAttackSpeedAdd))}%, 잃은 체력 10%마다 +{P(Mathf.Max(0f,values.bloodPressurePerTenPercentMissingAdd))}% (최대 +{P(Mathf.Max(0f,values.bloodPressureBaseAttackSpeedAdd)+Mathf.Max(0f,values.bloodPressurePerTenPercentMissingAdd)*10f)}%)";
                case PlayerItemIds.VoidCellId: return $"공격 시 {P(Mathf.Clamp01(values.voidCellChance))}% 확률로 적 주변에서 {P(Mathf.Max(.05f,values.voidCellDamageMultiplier))}% 피해의 추가 탄환 생성";
                case PlayerItemIds.ForbiddenGrowthId: return $"최대 체력 -{F(Mathf.Max(0f,values.forbiddenGrowthMaxHealthPenalty))}, 공격력 +{F(Mathf.Max(0f,values.forbiddenGrowthFlatAttackBonus))}";
                case PlayerItemIds.OverclockNerveId: return $"최대 체력 -{F(Mathf.Max(0f,values.overclockNerveMaxHealthPenalty))}, 이동속도 +{F(Mathf.Max(0f,values.overclockNerveFlatMoveSpeedBonus))}";
                case PlayerItemIds.BloodContractId: return $"적 {F(Mathf.Max(1,values.bloodContractKillsPerHeal))}명 처치 시 최대 체력 +{F(Mathf.Max(0f,values.bloodContractHealthGainAmount))} 및 같은 양 회복 (최대 {F(Mathf.Max(1,values.bloodContractMaxHealthGainCount))}회), 받는 피해 {F(Mathf.Max(1f,values.bloodContractIncomingDamageMultiplier))}배";
                case PlayerItemIds.HyperplasiaHeartId: return $"잃은 체력에 비례해 기본 공격 피해 증가 (최대 +{F(Mathf.Max(0f,values.hyperplasiaMissingHealthAttackBonusMax))})";
                case PlayerItemIds.DecayOrganId: return $"{F(Mathf.Max(.1f,values.decayOrganSecondsPerAttackBonus))}초마다 기본 공격 피해 +1 (최대 +{F(Mathf.Max(0,values.decayOrganMaxAttackBonus))})";
                case PlayerItemIds.RuptureMuscleId: return $"공격 2회마다 기본 공격 피해 +{F(Mathf.Max(0f,values.ruptureMuscleAttackBonusPerStack))}·이동속도 -{F(Mathf.Max(0f,values.ruptureMuscleMovePenaltyPerStack))} (최대 {F(Mathf.Max(1f,values.ruptureMuscleMaxStacks))}중첩)";
                case PlayerItemIds.ImperfectRegenerationId: return $"최대 체력 -{F(Mathf.Max(0f,values.imperfectRegenMaxHealthPenalty))}, 피격 {F(Mathf.Max(.1f,values.imperfectRegenDelay))}초 후 체력 +{F(Mathf.Max(0f,values.imperfectRegenHealPerTrigger))} 회복 ({F(Mathf.Max(0f,values.imperfectRegenCooldownDuration))}초 쿨타임)";
                case PlayerItemIds.SeveranceReflexId: return $"피격 후 {F(Mathf.Max(.1f,values.severanceReflexDuration))}초간 기본 공격 피해 +{F(Mathf.Max(0f,values.severanceReflexFlatAttackBonus))}";
                case PlayerItemIds.ExoskeletonId: return $"받는 피해 {P(Mathf.Clamp01(values.exoskeletonDamageReductionRatio))}% 감소, 이동속도 -{F(Mathf.Max(0f,values.exoskeletonMoveSpeedPenalty))}";
                case PlayerItemIds.PlateletMembraneId: return $"보호막 소모 후 {F(Mathf.Max(.1f,values.plateletMembraneInterval))}초마다 피해 {F(Mathf.Max(0f,values.plateletMembraneShieldAmount))}을 막는 보호막 생성";
                case PlayerItemIds.RecoveryFactorId: return $"{F(Mathf.Max(.1f,values.recoveryFactorInterval))}초마다 체력 +{F(Mathf.Max(0f,values.recoveryFactorHealAmount))} 회복";
                case PlayerItemIds.ReflectiveSkinId: return $"보호막 흡수 후 남은 피격 피해의 {P(Mathf.Max(0f,values.reflectiveSkinDamageRatio))}%를 공격자에게 반사";
                case PlayerItemIds.BioBarrierId: return $"정지 {F(Mathf.Max(.1f,values.bioBarrierIdleSecondsPerStep))}초마다 받는 피해 {P(Mathf.Max(0f,values.bioBarrierReductionPerStep))}% 감소 (최대 {P(Mathf.Clamp01(values.bioBarrierMaxReduction))}%)";
                case PlayerItemIds.SplitRegenerationId: return $"체력 0이 될 때 체력 {F(values.splitRegenerationReviveHealth>0f?values.splitRegenerationReviveHealth:2f)}으로 한 번 부활 (최대 체력 이내)";
                case PlayerItemIds.InfectedHostId: return $"일반 적 처치 시 {P(Mathf.Clamp01(values.infectedHostChance))}% 확률로 {F(Mathf.Max(.1f,values.infectedHostLifetime))}초간 공격력의 {P(Mathf.Max(.05f,values.infectedHostDamageMultiplier))}% 피해를 주는 아군 생성 (최대 {F(Mathf.Max(1,values.infectedHostMaxAllies))}명)";
                case PlayerItemIds.SporeColonyId: return $"{F(Mathf.Max(.1f,values.sporeSpawnInterval))}초마다 적에게 달려드는 포자 소환 ({F(Mathf.Max(.1f,values.sporeLifetime))}초 유지, 공격력의 {P(Mathf.Max(.05f,values.sporeDamageMultiplier))}% 피해)";
                case PlayerItemIds.BloodDroneId: return $"{F(Mathf.Max(.1f,values.bloodDroneFireInterval))}초마다 가까운 적에게 공격력의 {P(Mathf.Max(.05f,values.bloodDroneDamageMultiplier))}% 피해";
                case PlayerItemIds.GuardianOrganId: return $"플레이어 주변을 맴돌며 {F(Mathf.Max(.1f,values.guardianOrganCooldown))}초마다 적 투사체 하나 방어";
                case PlayerItemIds.TentacleColonyId: return $"{F(Mathf.Max(.1f,values.tentacleBindInterval))}초마다 적 {F(Mathf.Max(1,values.tentacleMaxTargets))}명에게 {F(Mathf.Max(.1f,values.tentacleBindDuration))}초간 {P(Mathf.Clamp01(values.tentacleSlowRatio))}% 둔화와 공격력의 {P(Mathf.Max(0f,values.tentacleDamageMultiplier))}% 피해";
                case PlayerItemIds.ElectricNeuralNetworkId: return $"적 처치 시 최대 {F(Mathf.Max(1,values.electricChainMaxHits))}명에게 피해 {F(Mathf.Max(0f,values.electricChainDamage))}의 연쇄 전류 ({F(Mathf.Max(0f,values.electricChainCooldown))}초 쿨타임)";
                case PlayerItemIds.InfectionTransferenceId: return $"적 처치 시 주변 {F(Mathf.Max(1,values.infectionTransferMaxTargets))}명에게 {F(Mathf.Max(.1f,values.infectionTransferDuration))}초간 {F(Mathf.Max(.1f,values.infectionTransferTickInterval))}초마다 {F(Mathf.Max(0f,values.infectionTransferTickDamage))} 감염 피해 ({F(Mathf.Max(0f,values.infectionTransferCooldown))}초 쿨타임)";
                case PlayerItemIds.MacrophageId: return $"적 처치 시 {F(Mathf.Max(.1f,values.macrophageDuration))}초간 공격력 +{F(Mathf.Max(0f,values.macrophageAttackBonusPerStack))} (최대 {F(Mathf.Max(1,values.macrophageMaxStacks))}중첩)";
                case PlayerItemIds.GluttonousOrganId: return $"적 처치 시 {F(Mathf.Max(.1f,values.gluttonousOrganDuration))}초간 이동속도 +{F(Mathf.Max(0f,values.gluttonousOrganMoveBonusPerStack))} (최대 {F(Mathf.Max(1,values.gluttonousOrganMaxStacks))}중첩)";
                case PlayerItemIds.HeartSniperId: return $"체력 {P(Mathf.Clamp01(values.heartSniperHealthThreshold))}% 이상인 적에게 직접 공격 피해 {P(Mathf.Max(0f,values.heartSniperDamageBonusRatio))}% 증가";
                case PlayerItemIds.BloodflowAccelerationId: return $"반경 {F(Mathf.Max(.1f,values.bloodflowAccelerationRadius))} 내 보스·엘리트 근처에서 기본 공격속도 {P(Mathf.Max(0f,values.bloodflowAccelerationAttackSpeedBonus))}% 증가";
                case PlayerItemIds.FocusedNerveId: return $"반경 {F(Mathf.Max(.1f,values.focusedNerveRadius))} 내 적 1명 이하일 때 기본 공격 피해 +{F(Mathf.Max(0f,values.focusedNerveHighAttackBonus))}, 2~3명일 때 +{F(Mathf.Max(0f,values.focusedNerveLowAttackBonus))}";
                case PlayerItemIds.ExecutionInstinctId: return $"체력 {P(Mathf.Clamp01(values.executionInstinctHealthThreshold))}% 이하인 적을 {P(Mathf.Clamp01(values.executionInstinctChance))}% 확률로 처형 (처형 제한 대상은 피해 {F(Mathf.Max(1f,values.executionInstinctBossDamageMultiplier))}배)";
                case PlayerItemIds.BerserkCellId: return $"보스 접근 시 {F(Mathf.Max(.1f,values.berserkCellDuration))}초간 공격력 +{F(Mathf.Max(0f,values.berserkCellAttackBonus))}, 이동속도 +{F(Mathf.Max(0f,values.berserkCellMoveBonus))}, 기본 공격속도 +{P(Mathf.Max(0f,values.berserkCellAttackSpeedBonus))}%";
                case PlayerItemIds.UnstableCellId: return $"투사체 속도 {P(Mathf.Max(.05f,values.unstableCellMinProjectileSpeedMultiplier))}~{P(Mathf.Max(Mathf.Max(.05f,values.unstableCellMinProjectileSpeedMultiplier),values.unstableCellMaxProjectileSpeedMultiplier))}%, {P(Mathf.Max(.01f,values.unstableCellSlowThreshold))}% 이하의 느린 탄은 피해 {P(Mathf.Max(1f,values.unstableCellSlowDamageMultiplier))}%";
                case PlayerItemIds.GrotesqueGrowthId: return $"{F(Mathf.Max(.1f,values.grotesqueGrowthInterval))}초마다 소형화·이동속도 +{F(Mathf.Max(0f,values.grotesqueGrowthSmallMoveBonus))} 또는 대형화·공격력 +{F(Mathf.Max(0f,values.grotesqueGrowthLargeAttackBonus))}·이동속도 -{F(Mathf.Max(0f,values.grotesqueGrowthLargeMovePenalty))}";
                case PlayerItemIds.MutationRampageId: return $"{F(Mathf.Max(.1f,values.mutationRampageInterval))}초마다 {F(Mathf.Max(.1f,values.mutationRampageDuration))}초간 무작위 스탯 버프 또는 디버프 (버프 확률 {P(Mathf.Clamp01(values.mutationRampageBuffChance))}%)";
                case PlayerItemIds.ParasiticBombId: return $"적 처치 시 {P(Mathf.Clamp01(values.parasiticBombChance))}% 확률로 반경 {F(Mathf.Max(.1f,values.parasiticBombRadius))}에 공격력 {P(Mathf.Max(0f,values.parasiticBombDamageMultiplier))}% 폭발 (최소 0.1)";
                case PlayerItemIds.FrenzyHormoneId: return $"피격 시 무작위 능력 +{F(Mathf.Max(0f,values.frenzyHormoneMinBonus))}~{F(Mathf.Max(Mathf.Max(0f,values.frenzyHormoneMinBonus),values.frenzyHormoneMaxBonus))} ({F(Mathf.Max(.1f,values.frenzyHormoneDuration))}초 지속, {F(Mathf.Max(0f,values.frenzyHormoneCooldown))}초 쿨타임)";
                default: return null;
            }
        }
        private static string F(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
        private static string P(float ratio) => F(ratio * 100f);
        private static string Signed(int value) => value > 0 ? "+" + value.ToString(CultureInfo.InvariantCulture) : value.ToString(CultureInfo.InvariantCulture);
    }
}
