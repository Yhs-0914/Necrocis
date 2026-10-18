using System;
using System.Collections.Generic;
using UnityEngine;
using static Necrocis.PlayerItemIds;
namespace Necrocis
{
    internal sealed class ItemTimedModifiers
    {
        private readonly IItemCombatHost host;
        private IItemCombatSettings settings => host.Settings;
        private readonly IItemOwnershipView ownership;
        private readonly ItemCombatQueries queries;
        internal ItemTimedModifiers(IItemCombatHost host,IItemOwnershipView ownership,ItemCombatQueries queries){this.host=host;this.ownership=ownership;this.queries=queries;}
        internal readonly RecoveryFactorRuntime recoveryRuntime=new RecoveryFactorRuntime();
        internal readonly PlateletMembraneRuntime shieldRuntime=new PlateletMembraneRuntime();
        internal readonly DecayOrganRuntime decayRuntime=new DecayOrganRuntime();
        internal readonly BloodContractRuntime bloodRuntime=new BloodContractRuntime();
        internal readonly SplitRegenerationRuntime splitRuntime=new SplitRegenerationRuntime();
        internal Dictionary<string,IItemEffectRuntime> persistentRuntimes;
        public ItemEffectRuntimeSettings PersistentSettings => new ItemEffectRuntimeSettings(
            settings.recoveryFactorInterval,settings.plateletMembraneInterval,settings.plateletMembraneShieldAmount,
            settings.decayOrganSecondsPerAttackBonus,settings.decayOrganMaxAttackBonus,settings.bloodContractKillsPerHeal,settings.bloodContractMaxHealthGainCount);
        public Dictionary<string,IItemEffectRuntime> PersistentRuntimes
        {
            get
            {
                if(persistentRuntimes==null)
                {
                    persistentRuntimes=new Dictionary<string,IItemEffectRuntime>(StringComparer.OrdinalIgnoreCase);
                    foreach(var runtime in new IItemEffectRuntime[]{recoveryRuntime,shieldRuntime,decayRuntime,bloodRuntime,splitRuntime})
                        persistentRuntimes.Add(runtime.ItemId,runtime);
                }
                return persistentRuntimes;
            }
        }
        internal Vector3 previousPosition;
        internal float movementIntensity;
        internal float rampageMoveAccumulatedTime;
        internal float rampageIdleAccumulatedTime;
        internal int rampageAttackBonus;
        internal bool isMovingByPosition;
        internal bool forbiddenGrowthModifierApplied;
        internal bool overclockNerveModifierApplied;
        internal bool imperfectRegenModifierApplied;
        internal float ruptureMuscleStacks;
        internal float ruptureMuscleLastAttackTime = float.NegativeInfinity;
        internal float severanceReflexBuffUntil = float.NegativeInfinity;
        internal float imperfectRegenPendingHeal;
        internal float imperfectRegenHealReadyTime = float.PositiveInfinity;
        internal float imperfectRegenCooldownUntil = float.NegativeInfinity;
        internal float ruptureAppliedMovePenalty;
        internal bool exoskeletonModifierApplied;
        internal float bioBarrierIdleTime;
        internal readonly object forbiddenGrowthModifierSource = new object();
        internal readonly object overclockNerveModifierSource = new object();
        internal readonly object imperfectRegenModifierSource = new object();
        internal readonly object ruptureMovePenaltyModifierSource = new object();
        internal readonly object exoskeletonModifierSource = new object();
        internal int focusedNerveEnemyCount = int.MaxValue;
        internal float focusedNerveNextScanTime;
        internal float berserkCellExpireTime = float.NegativeInfinity;
        internal int berserkCellActiveBossId;
        internal int berserkCellLastTriggeredBossId;
        internal bool berserkCellModifierApplied;
        internal float grotesqueGrowthNextRollTime;
        internal bool grotesqueGrowthModifierApplied;
        internal bool grotesqueGrowthScaleCached;
        internal Vector3 grotesqueGrowthBaseScale = Vector3.one;
        internal float mutationRampageNextRollTime;
        internal float mutationRampageExpireTime = float.NegativeInfinity;
        internal bool mutationRampageModifierApplied;
        internal float frenzyHormoneCooldownUntil = float.NegativeInfinity;
        internal float frenzyHormoneExpireTime = float.NegativeInfinity;
        internal bool frenzyHormoneModifierApplied;
        internal readonly object berserkCellModifierSource = new object();
        internal readonly object grotesqueGrowthModifierSource = new object();
        internal readonly object mutationRampageModifierSource = new object();
        internal readonly object frenzyHormoneModifierSource = new object();

        public void UpdateDefensiveStates()
        {
            shieldRuntime.Tick(ownership.hasPlateletMembraneItem,Time.time,settings.plateletMembraneInterval,settings.plateletMembraneShieldAmount);
            if(recoveryRuntime.ShouldHeal(ownership.hasRecoveryFactorItem,host.HasStatsOwner && !host.IsDead,Time.time,settings.recoveryFactorInterval))
            {
                host.Heal(Mathf.Max(0f,settings.recoveryFactorHealAmount),host.GetItemHealthEffectSource(RecoveryFactorId,"heal"));
                recoveryRuntime.Schedule(Time.time,settings.recoveryFactorInterval);
            }

            bool isMovingNow = (host.HasPlayerController && host.PlayerIsMoving) || isMovingByPosition || movementIntensity > 0.02f;
            if (!ownership.hasBioBarrierItem || isMovingNow)
            {
                bioBarrierIdleTime = 0f;
            }
            else
            {
                bioBarrierIdleTime += Time.deltaTime;
            }

            if (!ownership.hasSplitRegenerationItem)
            {
                splitRuntime.OnRemove(Time.time,PersistentSettings);
            }

        }

        public float GetIncomingDamageMultiplier()
        {
            float multiplier = 1f;
            if (ownership.HasBloodContract)
            {
                multiplier *= Mathf.Max(1f, settings.bloodContractIncomingDamageMultiplier);
            }

            if (ownership.HasExoskeleton)
            {
                float reduction = Mathf.Clamp01(settings.exoskeletonDamageReductionRatio);
                multiplier *= 1f - reduction;
            }

            if (ownership.HasBioBarrier)
            {
                int steps = Mathf.FloorToInt(bioBarrierIdleTime / Mathf.Max(0.1f, settings.bioBarrierIdleSecondsPerStep));
                float reduction = Mathf.Clamp(steps * Mathf.Max(0f, settings.bioBarrierReductionPerStep), 0f, Mathf.Clamp01(settings.bioBarrierMaxReduction));
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

            if (ownership.HasPlateletMembrane && shieldRuntime.Shield > 0f)
                damage=shieldRuntime.Absorb(damage,Time.time,settings.plateletMembraneInterval);

            if (ownership.HasReflectiveSkin && sourceEnemy != null && !sourceEnemy.IsDead && damage > 0f)
            {
                float reflectedDamage = damage * Mathf.Max(0f, settings.reflectiveSkinDamageRatio);
                if (reflectedDamage > 0f)
                {
                    sourceEnemy.TakeDamage(reflectedDamage, host.GetItemHealthEffectSource(ReflectiveSkinId, "reflection"));
                }
            }

            return Mathf.Max(0f, damage);
        }

        public bool TryConsumeSplitRegeneration(float currentHealth, float maxHealth, out float reviveHealth)
        {
            reviveHealth = 0f;
            if (!ownership.HasSplitRegeneration || !splitRuntime.TryConsume())
            {
                return false;
            }

            reviveHealth = Mathf.Clamp(Mathf.Max(0f, settings.splitRegenerationReviveHealth), 0f, Mathf.Max(0f, maxHealth));
            if (reviveHealth <= 0f)
            {
                reviveHealth = Mathf.Max(1f, Mathf.Min(2f, maxHealth));
            }

            return reviveHealth > 0f;
        }

        public void CapturePersistentItemState(SavedItemStateData target)
        {
            if(target==null || string.IsNullOrWhiteSpace(target.itemId))return;
            if(PersistentRuntimes.TryGetValue(target.itemId,out var runtime))runtime.Capture(target,Time.time);
        }

        public void RestorePersistentItemStates(IReadOnlyList<SavedItemStateData> savedItems)
        {
            bloodRuntime.OnRemove(Time.time,PersistentSettings);
            splitRuntime.OnRemove(Time.time,PersistentSettings);
            decayRuntime.BeginRestore(ownership.hasDecayOrganItem,Time.time);
            shieldRuntime.OnRemove(Time.time,PersistentSettings);
            recoveryRuntime.OnRemove(Time.time,PersistentSettings);
            if (host.HasManagedStatsOwner)
                host.Recipient.RemoveModifiersFromSource(host.ModifierSource);

            if (savedItems != null)
            {
                for (int i = 0; i < savedItems.Count; i++)
                {
                    SavedItemStateData state = savedItems[i];
                    if (state == null || string.IsNullOrWhiteSpace(state.itemId))
                    {
                        continue;
                    }

                    if(ownership.IsPersistentRuntimeOwned(state.itemId) && PersistentRuntimes.TryGetValue(state.itemId,out var runtime))
                        runtime.Restore(state,Time.time,PersistentSettings);
                }
            }

            float restoredBloodContractHealth = bloodRuntime.HealthGainCount
                                                * Mathf.Max(0f, settings.bloodContractHealthGainAmount);
            if (ownership.hasBloodContractItem && restoredBloodContractHealth > 0f && host.HasStatsOwner)
            {
                host.Recipient.AddModifier(
                    CharacterStatType.MaxHealth,
                    restoredBloodContractHealth,
                    CharacterStatModifierMode.Flat,
                    host.ModifierSource);
            }

            UpdateDecayOrganState();
            host.UpdatePlateletMembraneOutline();
        }

        public void UpdateBossFightItemStates()
        {
            UpdateFocusedNerveScan();
            UpdateBerserkCellState();
        }

        public void UpdateMutationChaosItems()
        {
            UpdateGrotesqueGrowthState();
            UpdateMutationRampageState();
            UpdateFrenzyHormoneState();
        }

        public void UpdateGrotesqueGrowthState()
        {
            if (!grotesqueGrowthScaleCached)
            {
                grotesqueGrowthBaseScale = host.Transform.localScale;
                grotesqueGrowthScaleCached = true;
            }

            if (!ownership.hasGrotesqueGrowthItem)
            {
                ClearGrotesqueGrowthState();
                grotesqueGrowthNextRollTime = 0f;
                return;
            }

            if (Time.time < grotesqueGrowthNextRollTime)
            {
                return;
            }

            grotesqueGrowthNextRollTime = Time.time + Mathf.Max(0.1f, settings.grotesqueGrowthInterval);
            bool rollSmall = UnityEngine.Random.value < 0.5f;
            host.Recipient?.RemoveModifiersFromSource(grotesqueGrowthModifierSource);
            grotesqueGrowthModifierApplied = true;

            if (rollSmall)
            {
                host.Transform.localScale = grotesqueGrowthBaseScale * Mathf.Max(0.1f, settings.grotesqueGrowthSmallScale);
                host.Recipient?.AddModifier(
                    CharacterStatType.MoveSpeed,
                    Mathf.Max(0f, settings.grotesqueGrowthSmallMoveBonus),
                    CharacterStatModifierMode.Flat,
                    grotesqueGrowthModifierSource);
                return;
            }

            host.Transform.localScale = grotesqueGrowthBaseScale * Mathf.Max(0.1f, settings.grotesqueGrowthLargeScale);
            host.Recipient?.AddModifier(
                CharacterStatType.AttackPower,
                Mathf.Max(0f, settings.grotesqueGrowthLargeAttackBonus),
                CharacterStatModifierMode.Flat,
                grotesqueGrowthModifierSource);
            host.Recipient?.AddModifier(
                CharacterStatType.MoveSpeed,
                -Mathf.Max(0f, settings.grotesqueGrowthLargeMovePenalty),
                CharacterStatModifierMode.Flat,
                grotesqueGrowthModifierSource);
        }

        public void ClearGrotesqueGrowthState()
        {
            if (grotesqueGrowthModifierApplied && host.HasStatsOwner && host.Recipient != null)
            {
                host.Recipient.RemoveModifiersFromSource(grotesqueGrowthModifierSource);
            }

            if (grotesqueGrowthScaleCached)
            {
                host.Transform.localScale = grotesqueGrowthBaseScale;
            }

            grotesqueGrowthModifierApplied = false;
        }

        public void UpdateMutationRampageState()
        {
            if (!ownership.hasMutationRampageItem)
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

            mutationRampageNextRollTime = Time.time + Mathf.Max(0.1f, settings.mutationRampageInterval);
            mutationRampageExpireTime = Time.time + Mathf.Max(0.1f, settings.mutationRampageDuration);
            ApplyRandomMutationRampageModifier();
        }

        public void ApplyRandomMutationRampageModifier()
        {
            if (!host.HasStatsOwner || host.Recipient == null)
            {
                return;
            }

            host.Recipient.RemoveModifiersFromSource(mutationRampageModifierSource);
            bool isBuff = UnityEngine.Random.value <= Mathf.Clamp01(settings.mutationRampageBuffChance);
            int statRoll = UnityEngine.Random.Range(0, 3);
            CharacterStatType statType;
            float value;

            if (statRoll == 0)
            {
                statType = CharacterStatType.AttackPower;
                value = isBuff ? Mathf.Max(0f, settings.mutationRampageAttackBuff) : -Mathf.Max(0f, settings.mutationRampageAttackDebuff);
            }
            else if (statRoll == 1)
            {
                statType = CharacterStatType.MoveSpeed;
                value = isBuff ? Mathf.Max(0f, settings.mutationRampageMoveBuff) : -Mathf.Max(0f, settings.mutationRampageMoveDebuff);
            }
            else
            {
                statType = CharacterStatType.AttackSpeed;
                value = isBuff ? Mathf.Max(0f, settings.mutationRampageAttackSpeedBuff) : -Mathf.Max(0f, settings.mutationRampageAttackSpeedDebuff);
            }

            host.Recipient.AddModifier(statType, value, CharacterStatModifierMode.Flat, mutationRampageModifierSource);
            mutationRampageModifierApplied = true;
        }

        public void ClearMutationRampageState()
        {
            if (mutationRampageModifierApplied && host.HasStatsOwner && host.Recipient != null)
            {
                host.Recipient.RemoveModifiersFromSource(mutationRampageModifierSource);
            }

            mutationRampageModifierApplied = false;
            mutationRampageExpireTime = float.NegativeInfinity;
        }

        public void UpdateFrenzyHormoneState()
        {
            if (!ownership.hasFrenzyHormoneItem)
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

        public void ApplyFrenzyHormoneBuff()
        {
            if (!ownership.HasFrenzyHormone || !host.HasStatsOwner || host.Recipient == null)
            {
                return;
            }

            if (Time.time < frenzyHormoneCooldownUntil)
            {
                return;
            }

            host.Recipient.RemoveModifiersFromSource(frenzyHormoneModifierSource);
            int statRoll = UnityEngine.Random.Range(0, 3);
            CharacterStatType statType = statRoll == 0
                ? CharacterStatType.AttackPower
                : statRoll == 1 ? CharacterStatType.MoveSpeed : CharacterStatType.AttackSpeed;
            float minBonus = Mathf.Max(0f, settings.frenzyHormoneMinBonus);
            float maxBonus = Mathf.Max(minBonus, settings.frenzyHormoneMaxBonus);
            float bonus = UnityEngine.Random.Range(minBonus, maxBonus);
            host.Recipient.AddModifier(statType, bonus, CharacterStatModifierMode.Flat, frenzyHormoneModifierSource);
            frenzyHormoneModifierApplied = true;
            frenzyHormoneExpireTime = Time.time + Mathf.Max(0.1f, settings.frenzyHormoneDuration);
            frenzyHormoneCooldownUntil = Time.time + Mathf.Max(0f, settings.frenzyHormoneCooldown);
        }

        public void ClearFrenzyHormoneState()
        {
            if (frenzyHormoneModifierApplied && host.HasStatsOwner && host.Recipient != null)
            {
                host.Recipient.RemoveModifiersFromSource(frenzyHormoneModifierSource);
            }

            frenzyHormoneModifierApplied = false;
            frenzyHormoneExpireTime = float.NegativeInfinity;
        }

        public void UpdateFocusedNerveScan()
        {
            if (!ownership.hasFocusedNerveItem)
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
            focusedNerveEnemyCount = queries.CountEnemiesWithinRadius(host.Transform.position, Mathf.Max(0.1f, settings.focusedNerveRadius));
        }

        public void UpdateBerserkCellState()
        {
            if (!ownership.hasBerserkCellItem)
            {
                ClearBerserkCellBuff();
                berserkCellLastTriggeredBossId = 0;
                return;
            }

            EnemyController boss = queries.FindNearestBossLikeEnemy(Mathf.Max(0.1f, settings.berserkCellBossDetectionRadius));
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

        public float GetFocusedNerveAttackBonus()
        {
            if (focusedNerveEnemyCount <= 1)
            {
                return Mathf.Max(0f, settings.focusedNerveHighAttackBonus);
            }

            if (focusedNerveEnemyCount <= 3)
            {
                return Mathf.Max(0f, settings.focusedNerveLowAttackBonus);
            }

            return 0f;
        }

        public void ActivateBerserkCellBuff()
        {
            if (!host.HasStatsOwner || host.Recipient == null)
            {
                return;
            }

            host.Recipient.RemoveModifiersFromSource(berserkCellModifierSource);
            host.Recipient.AddModifier(
                CharacterStatType.AttackPower,
                Mathf.Max(0f, settings.berserkCellAttackBonus),
                CharacterStatModifierMode.Flat,
                berserkCellModifierSource);
            host.Recipient.AddModifier(
                CharacterStatType.MoveSpeed,
                Mathf.Max(0f, settings.berserkCellMoveBonus),
                CharacterStatModifierMode.Flat,
                berserkCellModifierSource);
            berserkCellExpireTime = Time.time + Mathf.Max(0.1f, settings.berserkCellDuration);
            berserkCellModifierApplied = true;
        }

        public void ClearBerserkCellBuff()
        {
            if (host.HasStatsOwner && host.Recipient != null)
            {
                host.Recipient.RemoveModifiersFromSource(berserkCellModifierSource);
            }

            berserkCellExpireTime = float.NegativeInfinity;
            berserkCellModifierApplied = false;
        }

        public bool IsBerserkCellActive()
        {
            return berserkCellModifierApplied && Time.time < berserkCellExpireTime;
        }

        public Vector3 GetMovementAnchorPosition()
        {
            Transform anchor = host.HasPlayerController ? host.PlayerTransform : host.Transform;
            return anchor.position;
        }

        public void UpdateMovementIntensity()
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

        public void UpdateRampageBloodFlowState()
        {
            if (!ownership.hasRampageBloodFlowItem)
            {
                rampageMoveAccumulatedTime = 0f;
                rampageIdleAccumulatedTime = 0f;
                rampageAttackBonus = 0;
                return;
            }

            bool isMovingNow = (host.HasPlayerController && host.PlayerIsMoving) || isMovingByPosition || movementIntensity > 0.02f;
            if (isMovingNow)
            {
                rampageIdleAccumulatedTime = 0f;
                if (rampageAttackBonus >= Mathf.Max(0, settings.rampageMaxAttackBonus))
                {
                    return;
                }

                rampageMoveAccumulatedTime += Time.deltaTime;
                float secondsPerBonus = Mathf.Max(0.1f, settings.rampageMoveSecondsPerBonus);
                while (rampageMoveAccumulatedTime >= secondsPerBonus && rampageAttackBonus < Mathf.Max(0, settings.rampageMaxAttackBonus))
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
            float secondsPerDecay = Mathf.Max(0.1f, settings.rampageIdleSecondsPerDecay);
            while (rampageIdleAccumulatedTime >= secondsPerDecay && rampageAttackBonus > 0)
            {
                rampageIdleAccumulatedTime -= secondsPerDecay;
                rampageAttackBonus--;
            }
        }

        public void UpdateDecayOrganState()
            => decayRuntime.Tick(ownership.hasDecayOrganItem,Time.time,settings.decayOrganSecondsPerAttackBonus,settings.decayOrganMaxAttackBonus);

        public void UpdateRuptureMuscleState()
        {
            if (!ownership.hasRuptureMuscleItem)
            {
                ruptureMuscleStacks = 0;
                ruptureMuscleLastAttackTime = float.NegativeInfinity;
                ApplyRuptureMovePenalty(0f);
                return;
            }

            float decayDelay = Mathf.Max(0.25f, settings.ruptureMuscleDecayDelay);
            if (ruptureMuscleStacks > 0 && Time.time - ruptureMuscleLastAttackTime > decayDelay)
            {
                ruptureMuscleStacks = Mathf.Max(0f, ruptureMuscleStacks - 1f);
                ruptureMuscleLastAttackTime = Time.time;
            }

            int effectiveRuptureStacks = Mathf.FloorToInt(Mathf.Max(0f, ruptureMuscleStacks));
            float targetPenalty = Mathf.Max(0f, settings.ruptureMuscleMovePenaltyPerStack) * effectiveRuptureStacks;
            ApplyRuptureMovePenalty(targetPenalty);
        }

        public void UpdateImperfectRegenState()
        {
            if (!ownership.hasImperfectRegenItem || !host.HasStatsOwner || host.IsDead)
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
            host.Heal(healAmount, host.GetItemHealthEffectSource(ImperfectRegenerationId, "heal"));
            imperfectRegenCooldownUntil = Time.time + Mathf.Max(0f, settings.imperfectRegenCooldownDuration);
        }

        public void ApplyRuptureMovePenalty(float penalty)
        {
            if (!host.HasStatsOwner || host.Recipient == null)
            {
                ruptureAppliedMovePenalty = penalty;
                return;
            }

            if (Mathf.Approximately(ruptureAppliedMovePenalty, penalty))
            {
                return;
            }

            host.Recipient.RemoveModifiersFromSource(ruptureMovePenaltyModifierSource);
            ruptureAppliedMovePenalty = Mathf.Max(0f, penalty);
            if (ruptureAppliedMovePenalty > 0f)
            {
                host.Recipient.AddModifier(
                    CharacterStatType.MoveSpeed,
                    -ruptureAppliedMovePenalty,
                    CharacterStatModifierMode.Flat,
                    ruptureMovePenaltyModifierSource);
            }
        }

        public void SyncPersistentStatModifiers()
        {
            if (!host.HasStatsOwner)
            {
                return;
            }

            SetSimpleModifierState(
                ownership.HasForbiddenGrowth,
                ref forbiddenGrowthModifierApplied,
                forbiddenGrowthModifierSource,
                new CharacterStatModifier(CharacterStatType.MaxHealth, -Mathf.Max(0f, settings.forbiddenGrowthMaxHealthPenalty), CharacterStatModifierMode.Flat, forbiddenGrowthModifierSource),
                new CharacterStatModifier(CharacterStatType.AttackPower, Mathf.Max(0f, settings.forbiddenGrowthFlatAttackBonus), CharacterStatModifierMode.Flat, forbiddenGrowthModifierSource));

            SetSimpleModifierState(
                ownership.HasOverclockNerve,
                ref overclockNerveModifierApplied,
                overclockNerveModifierSource,
                new CharacterStatModifier(CharacterStatType.MaxHealth, -Mathf.Max(0f, settings.overclockNerveMaxHealthPenalty), CharacterStatModifierMode.Flat, overclockNerveModifierSource),
                new CharacterStatModifier(CharacterStatType.MoveSpeed, Mathf.Max(0f, settings.overclockNerveFlatMoveSpeedBonus), CharacterStatModifierMode.Flat, overclockNerveModifierSource));

            SetSimpleModifierState(
                ownership.HasImperfectRegeneration,
                ref imperfectRegenModifierApplied,
                imperfectRegenModifierSource,
                new CharacterStatModifier(CharacterStatType.MaxHealth, -Mathf.Max(0f, settings.imperfectRegenMaxHealthPenalty), CharacterStatModifierMode.Flat, imperfectRegenModifierSource));

            SetSimpleModifierState(
                ownership.HasExoskeleton,
                ref exoskeletonModifierApplied,
                exoskeletonModifierSource,
                new CharacterStatModifier(CharacterStatType.MoveSpeed, -Mathf.Max(0f, settings.exoskeletonMoveSpeedPenalty), CharacterStatModifierMode.Flat, exoskeletonModifierSource));
        }

        public void SetSimpleModifierState(
            bool shouldApply,
            ref bool appliedFlag,
            object source,
            CharacterStatModifier modifier)
        {
            if (!host.HasStatsOwner || host.Recipient == null)
            {
                return;
            }

            if (shouldApply && !appliedFlag)
            {
                host.Recipient.AddModifier(modifier);
                appliedFlag = true;
                return;
            }

            if (!shouldApply && appliedFlag)
            {
                host.Recipient.RemoveModifiersFromSource(source);
                appliedFlag = false;
            }
        }

        public void SetSimpleModifierState(
            bool shouldApply,
            ref bool appliedFlag,
            object source,
            CharacterStatModifier firstModifier,
            CharacterStatModifier secondModifier)
        {
            if (!host.HasStatsOwner || host.Recipient == null)
            {
                return;
            }

            if (shouldApply && !appliedFlag)
            {
                host.Recipient.AddModifier(firstModifier);
                host.Recipient.AddModifier(secondModifier);
                appliedFlag = true;
                return;
            }

            if (!shouldApply && appliedFlag)
            {
                host.Recipient.RemoveModifiersFromSource(source);
                appliedFlag = false;
            }
        }

        public void HandlePlayerHealthChanged(CharacterStats _, CharacterHealthChangedEventArgs args)
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

            if (ownership.HasImperfectRegeneration)
            {
                bool canScheduleRegen = Time.time >= imperfectRegenCooldownUntil;
                canScheduleRegen &= !(imperfectRegenPendingHeal > 0f && Time.time < imperfectRegenHealReadyTime);
                if (canScheduleRegen)
                {
                    imperfectRegenPendingHeal = Mathf.Max(0f, settings.imperfectRegenHealPerTrigger);
                    imperfectRegenHealReadyTime = Time.time + Mathf.Max(0.1f, settings.imperfectRegenDelay);
                }
            }

            if (ownership.HasSeveranceReflex)
            {
                severanceReflexBuffUntil = Time.time + Mathf.Max(0.1f, settings.severanceReflexDuration);
            }

            ApplyFrenzyHormoneBuff();
        }
    }
}
