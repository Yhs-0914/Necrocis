using System.Collections.Generic;
using UnityEngine;
namespace Necrocis
{
    internal sealed class ItemKillEffectState
    {
        internal readonly List<(EnemyController enemy, uint generation)> electricChainCandidates = new List<(EnemyController, uint)>();
        internal bool applyingElectricChain;
        internal uint electricChainLifetimeVersion;
        internal readonly HashSet<EnemyController> tempParasiticBombEnemies = new HashSet<EnemyController>();
        internal int macrophageStacks;
        internal float macrophageExpireTime = float.NegativeInfinity;
        internal float macrophageAppliedAttackBonus;
        internal int gluttonousOrganStacks;
        internal float gluttonousOrganExpireTime = float.NegativeInfinity;
        internal float gluttonousOrganAppliedMoveBonus;
        internal float electricChainNextReadyTime = float.NegativeInfinity;
        internal float infectionTransferNextReadyTime = float.NegativeInfinity;
        internal bool applyingParasiticBombDamage;
        internal readonly object macrophageModifierSource = new object();
        internal readonly object gluttonousOrganModifierSource = new object();
        internal Collider[] parasiticBombHitBuffer = new Collider[32];
    }
    internal readonly struct ItemKillEffectSettings
    {
        internal readonly float electricChainDamage;
        internal readonly float electricChainRadius;
        internal readonly int electricChainMaxHits;
        internal readonly float electricChainCooldown;
        internal readonly float infectionTransferRadius;
        internal readonly int infectionTransferMaxTargets;
        internal readonly float infectionTransferDuration;
        internal readonly float infectionTransferTickInterval;
        internal readonly float infectionTransferTickDamage;
        internal readonly float infectionTransferCooldown;
        internal readonly float macrophageDuration;
        internal readonly float macrophageAttackBonusPerStack;
        internal readonly int macrophageMaxStacks;
        internal readonly float gluttonousOrganDuration;
        internal readonly float gluttonousOrganMoveBonusPerStack;
        internal readonly int gluttonousOrganMaxStacks;
        internal readonly float parasiticBombChance;
        internal readonly float parasiticBombRadius;
        internal readonly float parasiticBombDamageMultiplier;
        internal ItemKillEffectSettings(float electricChainDamage,float electricChainRadius,int electricChainMaxHits,float electricChainCooldown,float infectionTransferRadius,int infectionTransferMaxTargets,float infectionTransferDuration,float infectionTransferTickInterval,float infectionTransferTickDamage,float infectionTransferCooldown,float macrophageDuration,float macrophageAttackBonusPerStack,int macrophageMaxStacks,float gluttonousOrganDuration,float gluttonousOrganMoveBonusPerStack,int gluttonousOrganMaxStacks,float parasiticBombChance,float parasiticBombRadius,float parasiticBombDamageMultiplier)
        {
            this.electricChainDamage=electricChainDamage;
            this.electricChainRadius=electricChainRadius;
            this.electricChainMaxHits=electricChainMaxHits;
            this.electricChainCooldown=electricChainCooldown;
            this.infectionTransferRadius=infectionTransferRadius;
            this.infectionTransferMaxTargets=infectionTransferMaxTargets;
            this.infectionTransferDuration=infectionTransferDuration;
            this.infectionTransferTickInterval=infectionTransferTickInterval;
            this.infectionTransferTickDamage=infectionTransferTickDamage;
            this.infectionTransferCooldown=infectionTransferCooldown;
            this.macrophageDuration=macrophageDuration;
            this.macrophageAttackBonusPerStack=macrophageAttackBonusPerStack;
            this.macrophageMaxStacks=macrophageMaxStacks;
            this.gluttonousOrganDuration=gluttonousOrganDuration;
            this.gluttonousOrganMoveBonusPerStack=gluttonousOrganMoveBonusPerStack;
            this.gluttonousOrganMaxStacks=gluttonousOrganMaxStacks;
            this.parasiticBombChance=parasiticBombChance;
            this.parasiticBombRadius=parasiticBombRadius;
            this.parasiticBombDamageMultiplier=parasiticBombDamageMultiplier;
        }
    }
}
