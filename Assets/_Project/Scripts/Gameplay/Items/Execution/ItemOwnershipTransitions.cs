using System;
using System.Collections.Generic;
using UnityEngine;
using static Necrocis.PlayerItemIds;
namespace Necrocis
{
    internal sealed class ItemOwnershipTransitions : IItemOwnershipView
    {
        private readonly IItemCombatHost host;
        private IItemCombatSettings settings => host.Settings;
        internal ItemTimedModifiers timed;
        internal ItemAttackEffects attack;
        internal ItemOwnershipTransitions(IItemCombatHost host){this.host=host;}
        public bool IsPersistentRuntimeOwned(string id)
        {
            if(string.Equals(id,RecoveryFactorId,StringComparison.OrdinalIgnoreCase))return hasRecoveryFactorItem;
            if(string.Equals(id,PlateletMembraneId,StringComparison.OrdinalIgnoreCase))return hasPlateletMembraneItem;
            if(string.Equals(id,DecayOrganId,StringComparison.OrdinalIgnoreCase))return hasDecayOrganItem;
            if(string.Equals(id,BloodContractId,StringComparison.OrdinalIgnoreCase))return hasBloodContractItem;
            if(string.Equals(id,SplitRegenerationId,StringComparison.OrdinalIgnoreCase))return hasSplitRegenerationItem;
            return false;
        }
        internal bool itemCacheInitialized;
        public bool hasAnyCachedItems { get; private set; }
        public bool hasPersistentStatItems { get; private set; }
        public bool hasDefensiveUpdateItems { get; private set; }
        public bool hasPlateletMembraneItem { get; private set; }
        public bool hasRecoveryFactorItem { get; private set; }
        public bool hasBioBarrierItem { get; private set; }
        public bool hasSplitRegenerationItem { get; private set; }
        public bool hasBloodContractItem { get; private set; }
        public bool hasSeveranceReflexItem { get; private set; }
        public bool hasMovementTrackingItems { get; private set; }
        public bool hasRampageBloodFlowItem { get; private set; }
        public bool hasDecayOrganItem { get; private set; }
        public bool hasRuptureMuscleItem { get; private set; }
        public bool hasImperfectRegenItem { get; private set; }
        public bool hasOrganTentacleItem { get; private set; }
        public bool hasBioCompanionUpdateItems { get; private set; }
        public bool hasSporeColonyItem { get; private set; }
        public bool hasBloodDroneItem { get; private set; }
        public bool hasGuardianOrganItem { get; private set; }
        public bool hasTentacleColonyItem { get; private set; }
        public bool hasKillChainBuffItems { get; private set; }
        public bool hasMacrophageItem { get; private set; }
        public bool hasGluttonousOrganItem { get; private set; }
        public bool hasBossFightUpdateItems { get; private set; }
        public bool hasFocusedNerveItem { get; private set; }
        public bool hasBerserkCellItem { get; private set; }
        public bool hasMutationChaosItems { get; private set; }
        public bool hasGrotesqueGrowthItem { get; private set; }
        public bool hasMutationRampageItem { get; private set; }
        public bool hasFrenzyHormoneItem { get; private set; }
        public bool hasDecayStateItems { get; private set; }
        public bool hasOverheatedOrganItem { get; private set; }
        public bool hasBioResonanceItem { get; private set; }
        public bool hasUnstableCoreItem { get; private set; }
        internal readonly HashSet<string> acquiredItemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
        public bool HasItem(string itemId)
        {
            if(string.IsNullOrWhiteSpace(itemId))return false;
            if(itemCacheInitialized)return acquiredItemIds.Contains(itemId);
            return host.InventoryContains(itemId);
        }

        public void HandleItemAcquired(PlayerItemManager _, PlayerItemManager.AcquiredPlayerItem acquiredItem)
        {
            if (acquiredItem == null || string.IsNullOrWhiteSpace(acquiredItem.ItemId))
            {
                return;
            }

            RebuildItemCache();
        }

        public void HandleItemRemoved(PlayerItemManager _, PlayerItemManager.AcquiredPlayerItem removedItem)
        {
            if (removedItem == null || string.IsNullOrWhiteSpace(removedItem.ItemId))
            {
                return;
            }

            if (string.Equals(removedItem.ItemId, ElectricNeuralNetworkId, StringComparison.OrdinalIgnoreCase))
            {
                unchecked { host.KillState.electricChainLifetimeVersion++; }
            }
            RebuildItemCache();
        }
        public void RebuildItemCache()
        {
            acquiredItemIds.Clear();itemCacheInitialized=true;
            host.CopyItemIdsTo(acquiredItemIds);
            RefreshItemRuntimeFlags();
        }

        public void EnsureActiveForInventoryChange()
        {
            if (!host.Enabled)
            {
                host.Enabled = true;
                return;
            }

            RebuildItemCache();
        }

        public bool HasCachedItem(string itemId)
        {
            return acquiredItemIds.Contains(itemId);
        }

        public void RefreshItemRuntimeFlags()
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
            timed.SyncPersistentStatModifiers();
            ResetInactiveItemRuntimeState();
            RefreshDerivedUpdateFlags();
            SleepIfInventoryEmpty();
        }

        public void RefreshDerivedUpdateFlags()
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
                || (host.ExistingCompanionCount > 0);
            hasKillChainBuffItems =
                hasMacrophageItem
                || hasGluttonousOrganItem
                || host.KillState.macrophageStacks > 0
                || host.KillState.gluttonousOrganStacks > 0;
            hasBossFightUpdateItems = hasFocusedNerveItem || hasBerserkCellItem || timed.berserkCellModifierApplied;
            hasMutationChaosItems =
                hasGrotesqueGrowthItem
                || hasMutationRampageItem
                || hasFrenzyHormoneItem
                || timed.grotesqueGrowthModifierApplied
                || timed.mutationRampageModifierApplied
                || timed.frenzyHormoneModifierApplied;
            hasDecayStateItems = hasOverheatedOrganItem || hasBioResonanceItem || attack.resonanceStatesByEnemyId.Count > 0 || attack.overheatStacks > 0;
        }

        public void ResetInactiveItemRuntimeState()
        {
            float now = Time.time;
            foreach(var runtime in timed.PersistentRuntimes.Values)
                if(IsPersistentRuntimeOwned(runtime.ItemId))runtime.OnAcquire(now,timed.PersistentSettings);

            if (!hasPlateletMembraneItem)
            {
                timed.shieldRuntime.OnRemove(now,timed.PersistentSettings);
                host.ClearPlateletMembraneOutline();
            }

            if (!hasRecoveryFactorItem)
            {
                timed.recoveryRuntime.OnRemove(now,timed.PersistentSettings);
            }

            if (!hasBioBarrierItem)
            {
                timed.bioBarrierIdleTime = 0f;
            }

            if (!hasSplitRegenerationItem)
            {
                timed.splitRuntime.OnRemove(Time.time,timed.PersistentSettings);
            }

            if (!hasBloodContractItem)
            {
                timed.bloodRuntime.OnRemove(now,timed.PersistentSettings);
                if (host.HasManagedStatsOwner)
                    host.Recipient.RemoveModifiersFromSource(host.ModifierSource);
            }

            if (!hasSeveranceReflexItem)
            {
                timed.severanceReflexBuffUntil = float.NegativeInfinity;
            }

            if (!hasMovementTrackingItems)
            {
                timed.previousPosition = timed.GetMovementAnchorPosition();
                timed.isMovingByPosition = false;
                timed.movementIntensity = 0f;
            }

            if (!hasRampageBloodFlowItem)
            {
                timed.rampageMoveAccumulatedTime = 0f;
                timed.rampageIdleAccumulatedTime = 0f;
                timed.rampageAttackBonus = 0;
            }

            if (!hasDecayOrganItem)
            {
                timed.decayRuntime.OnRemove(now,timed.PersistentSettings);
            }

            if (!hasRuptureMuscleItem)
            {
                timed.ruptureMuscleStacks = 0f;
                timed.ruptureMuscleLastAttackTime = float.NegativeInfinity;
                timed.ApplyRuptureMovePenalty(0f);
            }

            if (!hasImperfectRegenItem)
            {
                timed.imperfectRegenPendingHeal = 0f;
                timed.imperfectRegenHealReadyTime = float.PositiveInfinity;
                timed.imperfectRegenCooldownUntil = float.NegativeInfinity;
            }

            if (!hasSporeColonyItem)
            {
                host.Companions.nextSporeSpawnTime = now + Mathf.Max(0.1f, settings.sporeSpawnInterval);
            }

            if (!hasBloodDroneItem)
            {
                host.Companions.ReleaseBloodDrone();
            }

            if (!hasGuardianOrganItem)
            {
                host.Companions.ReleaseGuardianOrgan();
            }

            if (!hasTentacleColonyItem)
            {
                host.Companions.tentacleNextBindTime = now + Mathf.Max(0.1f, settings.tentacleBindInterval);
            }

            if (!hasMacrophageItem && host.KillState.macrophageStacks > 0)
            {
                host.KillState.macrophageStacks = 0;
                host.KillState.macrophageExpireTime = float.NegativeInfinity;
                host.ClearKillChainStatBuff(host.KillState.macrophageModifierSource, ref host.KillState.macrophageAppliedAttackBonus);
            }

            if (!hasGluttonousOrganItem && host.KillState.gluttonousOrganStacks > 0)
            {
                host.KillState.gluttonousOrganStacks = 0;
                host.KillState.gluttonousOrganExpireTime = float.NegativeInfinity;
                host.ClearKillChainStatBuff(host.KillState.gluttonousOrganModifierSource, ref host.KillState.gluttonousOrganAppliedMoveBonus);
            }

            if (!hasFocusedNerveItem)
            {
                timed.focusedNerveEnemyCount = int.MaxValue;
                timed.focusedNerveNextScanTime = 0f;
            }

            if (!hasBerserkCellItem)
            {
                timed.ClearBerserkCellBuff();
                timed.berserkCellLastTriggeredBossId = 0;
            }

            if (!hasGrotesqueGrowthItem)
            {
                timed.ClearGrotesqueGrowthState();
                timed.grotesqueGrowthNextRollTime = 0f;
            }

            if (!hasMutationRampageItem)
            {
                timed.ClearMutationRampageState();
                timed.mutationRampageNextRollTime = 0f;
            }

            if (!hasFrenzyHormoneItem)
            {
                timed.ClearFrenzyHormoneState();
                timed.frenzyHormoneCooldownUntil = float.NegativeInfinity;
            }

            if (!hasOverheatedOrganItem)
            {
                attack.overheatStacks = 0;
                attack.overheatLastAttackTime = float.NegativeInfinity;
                attack.overheatNextDecayTime = float.PositiveInfinity;
            }

            if (!hasBioResonanceItem)
            {
                attack.resonanceStatesByEnemyId.Clear();
            }

            if (!hasUnstableCoreItem)
            {
                attack.unstableCoreCurrentMultiplier = 1f;
                attack.unstableCoreInitialized = false;
                attack.unstableCoreNextRerollTime = 0f;
                if (host.UnstableCoreOverlay != null)
                {
                    host.UnstableCoreOverlay.enabled = false;
                }
            }
        }

        public void SleepIfInventoryEmpty()
        {
            if (hasAnyCachedItems || !host.Enabled)
            {
                return;
            }

            host.SleepEmptyInventory();
        }

        public void ClearPersistentStatModifiers()
        {
            if (!host.HasStatsOwner || host.Recipient == null)
            {
                return;
            }

            host.Recipient.RemoveModifiersFromSource(timed.forbiddenGrowthModifierSource);
            host.Recipient.RemoveModifiersFromSource(timed.overclockNerveModifierSource);
            host.Recipient.RemoveModifiersFromSource(timed.imperfectRegenModifierSource);
            host.Recipient.RemoveModifiersFromSource(timed.ruptureMovePenaltyModifierSource);
            host.Recipient.RemoveModifiersFromSource(timed.exoskeletonModifierSource);
            host.Recipient.RemoveModifiersFromSource(host.KillState.macrophageModifierSource);
            host.Recipient.RemoveModifiersFromSource(host.KillState.gluttonousOrganModifierSource);
            host.Recipient.RemoveModifiersFromSource(timed.berserkCellModifierSource);
            host.Recipient.RemoveModifiersFromSource(timed.grotesqueGrowthModifierSource);
            host.Recipient.RemoveModifiersFromSource(timed.mutationRampageModifierSource);
            host.Recipient.RemoveModifiersFromSource(timed.frenzyHormoneModifierSource);
            timed.forbiddenGrowthModifierApplied = false;
            timed.overclockNerveModifierApplied = false;
            timed.imperfectRegenModifierApplied = false;
            timed.exoskeletonModifierApplied = false;
            timed.ClearGrotesqueGrowthState();
            timed.ClearMutationRampageState();
            timed.ClearFrenzyHormoneState();
            timed.ruptureAppliedMovePenalty = 0f;
            host.KillState.macrophageStacks = 0;
            host.KillState.macrophageAppliedAttackBonus = 0f;
            host.KillState.macrophageExpireTime = float.NegativeInfinity;
            host.KillState.gluttonousOrganStacks = 0;
            host.KillState.gluttonousOrganAppliedMoveBonus = 0f;
            host.KillState.gluttonousOrganExpireTime = float.NegativeInfinity;
            timed.berserkCellModifierApplied = false;
            timed.berserkCellExpireTime = float.NegativeInfinity;
            timed.berserkCellActiveBossId = 0;
        }
    }
}
