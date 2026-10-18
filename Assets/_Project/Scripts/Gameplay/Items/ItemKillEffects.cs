using UnityEngine;
namespace Necrocis
{
    internal interface IItemKillActions
    {
        ItemKillEffectSettings Settings { get; }
        bool IsActiveAndEnabled { get; }
        bool HasItem(string id);
        bool HasCachedMacrophage { get; }
        bool HasCachedGluttonous { get; }
        Vector3 OwnerPosition { get; }
        System.Collections.Generic.List<EnemyController> SharedTargets { get; }
        EnemyController FindClosestEnemy(Vector3 center,float radius,System.Collections.Generic.List<EnemyController> excluded);
        HealthEffectSource GetItemHealthEffectSource(string id,string effect);
        void SpawnElectricChainVisual(Vector3 from,Vector3 to);
        void SpawnInfectionTransferVisual(Vector3 from,Vector3 to);
        void SpawnParasiticBombVisual(Vector3 center,float radius);
        void SpawnInfectedHost(EnemyController enemy);
        bool CanGrowBloodContract { get; }
        int BloodContractPeriod { get; }
        int BloodContractMaximum { get; }
        float BloodContractGain { get; }
        bool HasStatsOwner { get; }
        CharacterStats Recipient { get; }
        object ModifierSource { get; }
        void HealBloodContract(float gain);
    }
    internal sealed class ItemKillEffects
    {
        private readonly IItemKillActions actions;
        private readonly BloodContractRuntime blood;
        private readonly ItemKillEffectState state;
        internal ItemKillEffects(IItemKillActions actions, BloodContractRuntime blood, ItemKillEffectState state) { this.actions=actions; this.blood=blood; this.state=state; }
        internal void NotifyDefeated(EnemyController enemy)
        {
            actions.SpawnInfectedHost(enemy);
            TriggerElectricNeuralNetwork(enemy);
            TriggerInfectionTransference(enemy);
            TriggerMacrophageBuff();
            TriggerGluttonousOrganBuff();
            TriggerParasiticBomb();
            if (!actions.CanGrowBloodContract) return;
            if (!blood.RecordKill(actions.BloodContractPeriod, actions.BloodContractMaximum)) return;
            float gain=Mathf.Max(0f,actions.BloodContractGain);
            if (gain<=0f) return;
            actions.Recipient.AddModifier(CharacterStatType.MaxHealth,gain,CharacterStatModifierMode.Flat,actions.ModifierSource);
            blood.CommitHealthGain();
            actions.HealBloodContract(gain);
        }
        internal void TriggerElectricNeuralNetwork(EnemyController defeatedEnemy)
        {
            if (!actions.IsActiveAndEnabled || state.applyingElectricChain || !actions.HasItem(PlayerItemIds.ElectricNeuralNetworkId) || defeatedEnemy == null)
            {
                return;
            }

            if (Time.time < state.electricChainNextReadyTime)
            {
                return;
            }

            Vector3 previousPosition = defeatedEnemy.transform.position;
            int maxHits = Mathf.Max(1, actions.Settings.electricChainMaxHits);
            float radius = Mathf.Max(0.1f, actions.Settings.electricChainRadius);
            float damage = Mathf.Max(0f, actions.Settings.electricChainDamage);
            if (damage <= 0f)
            {
                return;
            }

            HealthEffectSource source = actions.GetItemHealthEffectSource(PlayerItemIds.ElectricNeuralNetworkId, "chain");
            state.applyingElectricChain = true;
            uint lifetimeVersion = state.electricChainLifetimeVersion;
            state.electricChainCandidates.Clear();
            try
            {
                // Keep this activation independent from other kill effects and new spawns.
                var enemies = EnemyController.ActiveEnemyControllers;
                for (int i = 0; i < enemies.Count; i++)
                {
                    EnemyController enemy = enemies[i];
                    if (ItemEnemyQueries.IsTargetable(enemy))
                    {
                        state.electricChainCandidates.Add((enemy, enemy.SpawnGeneration));
                    }
                }

                int hitCount = 0;
                while (hitCount < maxHits && lifetimeVersion == state.electricChainLifetimeVersion
                    && actions.IsActiveAndEnabled && actions.HasItem(PlayerItemIds.ElectricNeuralNetworkId))
                {
                    int closestIndex = -1;
                    float bestDistanceSqr = float.PositiveInfinity;
                    for (int i = 0; i < state.electricChainCandidates.Count; i++)
                    {
                        var candidate = state.electricChainCandidates[i];
                        if (!IsElectricChainCandidateCurrent(candidate)) continue;
                        Vector3 delta = candidate.enemy.transform.position - previousPosition;
                        delta.y = 0f;
                        float distanceSqr = delta.sqrMagnitude;
                        if (distanceSqr > radius * radius || distanceSqr >= bestDistanceSqr) continue;
                        bestDistanceSqr = distanceSqr;
                        closestIndex = i;
                    }
                    if (closestIndex < 0) break;

                    var targetSnapshot = state.electricChainCandidates[closestIndex];
                    state.electricChainCandidates.RemoveAt(closestIndex);
                    EnemyController target = targetSnapshot.enemy;
                    Vector3 targetPosition = target.transform.position;
                    if (hitCount == 0)
                    {
                        // Reserve before damage can synchronously enter another kill callback.
                        state.electricChainNextReadyTime = Time.time + Mathf.Max(0f, actions.Settings.electricChainCooldown);
                    }
                    hitCount++;
                    actions.SpawnElectricChainVisual(previousPosition, targetPosition);
                    if (lifetimeVersion != state.electricChainLifetimeVersion || !actions.IsActiveAndEnabled || !actions.HasItem(PlayerItemIds.ElectricNeuralNetworkId)) break;
                    if (!IsElectricChainCandidateCurrent(targetSnapshot)) continue;
                    target.TakeDamage(damage, source);
                    previousPosition = targetPosition;
                }
            }
            finally
            {
                state.electricChainCandidates.Clear();
                state.applyingElectricChain = false;
            }
        }

        private static bool IsElectricChainCandidateCurrent((EnemyController enemy, uint generation) candidate)
        {
            return ItemEnemyQueries.IsTargetable(candidate.enemy) && candidate.enemy.SpawnGeneration == candidate.generation;
        }

        internal void TriggerInfectionTransference(EnemyController defeatedEnemy)
        {
            if (!actions.HasItem(PlayerItemIds.InfectionTransferenceId) || defeatedEnemy == null)
            {
                return;
            }

            if (Time.time < state.infectionTransferNextReadyTime)
            {
                return;
            }

            actions.SharedTargets.Clear();
            int maxTargets = Mathf.Max(1, actions.Settings.infectionTransferMaxTargets);
            Vector3 center = defeatedEnemy.transform.position;
            for (int i = 0; i < maxTargets; i++)
            {
                EnemyController target = actions.FindClosestEnemy(center, Mathf.Max(0.1f, actions.Settings.infectionTransferRadius), actions.SharedTargets);
                if (target == null)
                {
                    break;
                }

                actions.SharedTargets.Add(target);
            }

            HealthEffectSource source = actions.GetItemHealthEffectSource(PlayerItemIds.InfectionTransferenceId, "poison");
            for (int i = 0; i < actions.SharedTargets.Count; i++)
            {
                EnemyController enemy = actions.SharedTargets[i];
                if (!ItemEnemyQueries.IsTargetable(enemy))
                {
                    continue;
                }

                EnemyStatusEffectController status = ItemEnemyQueries.EnsureStatusController(enemy);
                status?.ApplyPoison(
                    Mathf.Max(0.1f, actions.Settings.infectionTransferDuration),
                    Mathf.Max(0.1f, actions.Settings.infectionTransferTickInterval),
                    Mathf.Max(0f, actions.Settings.infectionTransferTickDamage), source);
                actions.SpawnInfectionTransferVisual(center, enemy.transform.position);
            }

            if (actions.SharedTargets.Count > 0)
            {
                state.infectionTransferNextReadyTime = Time.time + Mathf.Max(0f, actions.Settings.infectionTransferCooldown);
            }
        }

        internal void TriggerMacrophageBuff()
        {
            if (!actions.HasItem(PlayerItemIds.MacrophageId) || !actions.HasStatsOwner || actions.Recipient == null)
            {
                return;
            }

            state.macrophageStacks = Mathf.Clamp(state.macrophageStacks + 1, 1, Mathf.Max(1, actions.Settings.macrophageMaxStacks));
            state.macrophageExpireTime = Time.time + Mathf.Max(0.1f, actions.Settings.macrophageDuration);
            ApplyKillChainStatBuff(
                state.macrophageModifierSource,
                CharacterStatType.AttackPower,
                ref state.macrophageAppliedAttackBonus,
                Mathf.Max(0f, actions.Settings.macrophageAttackBonusPerStack) * state.macrophageStacks);
        }

        internal void TriggerGluttonousOrganBuff()
        {
            if (!actions.HasItem(PlayerItemIds.GluttonousOrganId) || !actions.HasStatsOwner || actions.Recipient == null)
            {
                return;
            }

            state.gluttonousOrganStacks = Mathf.Clamp(state.gluttonousOrganStacks + 1, 1, Mathf.Max(1, actions.Settings.gluttonousOrganMaxStacks));
            state.gluttonousOrganExpireTime = Time.time + Mathf.Max(0.1f, actions.Settings.gluttonousOrganDuration);
            ApplyKillChainStatBuff(
                state.gluttonousOrganModifierSource,
                CharacterStatType.MoveSpeed,
                ref state.gluttonousOrganAppliedMoveBonus,
                Mathf.Max(0f, actions.Settings.gluttonousOrganMoveBonusPerStack) * state.gluttonousOrganStacks);
        }

        internal void TriggerParasiticBomb()
        {
            if (state.applyingParasiticBombDamage || !actions.HasItem(PlayerItemIds.ParasiticBombId) || !actions.HasStatsOwner || actions.Recipient.IsDead)
            {
                return;
            }

            if (UnityEngine.Random.value > Mathf.Clamp01(actions.Settings.parasiticBombChance))
            {
                return;
            }

            Vector3 center = actions.OwnerPosition;
            float radius = Mathf.Max(0.1f, actions.Settings.parasiticBombRadius);
            float damage = Mathf.Max(0.1f, actions.Recipient.AttackPower * Mathf.Max(0f, actions.Settings.parasiticBombDamageMultiplier));
            var hits=EnemyHitSnapshot.Sphere(center,radius,~0,
                QueryTriggerInteraction.Collide,ref state.parasiticBombHitBuffer);
            var hitData=hits;
            int hitCount=hits.Length;

            HealthEffectSource source = actions.GetItemHealthEffectSource(PlayerItemIds.ParasiticBombId, "explosion");
            state.tempParasiticBombEnemies.Clear();
            state.applyingParasiticBombDamage = true;
            try
            {
                for (int i = 0; i < hitCount; i++)
                {
                    if(!hitData[i].IsCurrent)continue;
                    EnemyController enemy = hitData[i].Enemy;
                    if (enemy == null || enemy.IsDead || !EnemyReceivingShape.IntersectsSphere(enemy,center,radius) || !state.tempParasiticBombEnemies.Add(enemy))
                    {
                        continue;
                    }

                    enemy.TakeDamage(damage, source);
                }
            }
            finally
            {
                state.applyingParasiticBombDamage = false;
                state.tempParasiticBombEnemies.Clear();
            }

            actions.SpawnParasiticBombVisual(center, radius);
        }

        internal void UpdateKillChainBuffs()
        {
            if ((!actions.HasCachedMacrophage || Time.time >= state.macrophageExpireTime) && state.macrophageStacks > 0)
            {
                state.macrophageStacks = 0;
                state.macrophageExpireTime = float.NegativeInfinity;
                ClearKillChainStatBuff(state.macrophageModifierSource, ref state.macrophageAppliedAttackBonus);
            }

            if ((!actions.HasCachedGluttonous || Time.time >= state.gluttonousOrganExpireTime) && state.gluttonousOrganStacks > 0)
            {
                state.gluttonousOrganStacks = 0;
                state.gluttonousOrganExpireTime = float.NegativeInfinity;
                ClearKillChainStatBuff(state.gluttonousOrganModifierSource, ref state.gluttonousOrganAppliedMoveBonus);
            }
        }

        internal void ApplyKillChainStatBuff(object source, CharacterStatType statType, ref float appliedValue, float value)
        {
            if (!actions.HasStatsOwner || actions.Recipient == null)
            {
                appliedValue = value;
                return;
            }

            actions.Recipient.RemoveModifiersFromSource(source);
            appliedValue = Mathf.Max(0f, value);
            if (appliedValue <= 0f)
            {
                return;
            }

            actions.Recipient.AddModifier(
                statType,
                appliedValue,
                CharacterStatModifierMode.Flat,
                source);
        }

        internal void ClearKillChainStatBuff(object source, ref float appliedValue)
        {
            if (actions.HasStatsOwner && actions.Recipient != null)
            {
                actions.Recipient.RemoveModifiersFromSource(source);
            }

            appliedValue = 0f;
        }
    }
}
