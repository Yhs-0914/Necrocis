using UnityEngine;
namespace Necrocis
{
    public partial class PlayerItemCombatEffects
    {
        public bool IsAvailable => this != null;
        string IItemEffectLifecycle.DescribeItem(string id) => DescribeItem(id);
        void IItemEffectLifecycle.RetireItemHealthSources(string id) => RetireItemHealthSources(id);
        private ItemCompanionRuntime companionRuntime;
        private ItemCompanionRuntime Companions => companionRuntime ?? (companionRuntime=new ItemCompanionRuntime(this,this,Ownership,Queries));
        private ItemKillEffects killEffects;
        private ItemKillEffects KillEffects => killEffects ?? (killEffects=new ItemKillEffects(this,Timed.bloodRuntime,killState));
        Transform IItemCompanionOwner.OwnerTransform => transform;
        bool IItemCompanionOwner.HasBloodDroneItem => Ownership.hasBloodDroneItem;
        bool IItemCompanionOwner.HasGuardianOrganItem => Ownership.hasGuardianOrganItem;
        float IItemCompanionOwner.BloodDroneOrbitRadius => bloodDroneOrbitRadius;
        float IItemCompanionOwner.BloodDroneTargetRadius => bloodDroneTargetRadius;
        float IItemCompanionOwner.BloodDroneFireInterval => bloodDroneFireInterval;
        float IItemCompanionOwner.BloodDroneDamageMultiplier => bloodDroneDamageMultiplier;
        float IItemCompanionOwner.GuardianOrganOrbitRadius => guardianOrganOrbitRadius;
        float IItemCompanionOwner.GuardianOrganBlockRadius => guardianOrganBlockRadius;
        float IItemCompanionOwner.GuardianOrganCooldown => guardianOrganCooldown;
        EnemyController IItemCompanionOwner.FindClosestEnemy(Vector3 center,float radius) => Queries.FindClosestEnemy(center,radius);
        float IItemCompanionOwner.GetCompanionBaseDamage() => Queries.GetCompanionBaseDamage();
        Vector3 IItemCompanionOwner.GetPlayerVisualCenter() => GetPlayerVisualCenter();
        void IItemCompanionOwner.ReleaseBioSummon(PlayerBioSummon summon) => ReleaseBioSummon(summon);
        ItemKillEffectSettings IItemKillActions.Settings => new ItemKillEffectSettings(electricChainDamage,electricChainRadius,electricChainMaxHits,electricChainCooldown,infectionTransferRadius,infectionTransferMaxTargets,infectionTransferDuration,infectionTransferTickInterval,infectionTransferTickDamage,infectionTransferCooldown,macrophageDuration,macrophageAttackBonusPerStack,macrophageMaxStacks,gluttonousOrganDuration,gluttonousOrganMoveBonusPerStack,gluttonousOrganMaxStacks,parasiticBombChance,parasiticBombRadius,parasiticBombDamageMultiplier);
        bool IItemKillActions.IsActiveAndEnabled => this != null && isActiveAndEnabled;
        bool IItemKillActions.HasCachedMacrophage => Ownership.hasMacrophageItem;
        bool IItemKillActions.HasCachedGluttonous => Ownership.hasGluttonousOrganItem;
        Vector3 IItemKillActions.OwnerPosition => playerController != null ? playerController.transform.position : transform.position;
        System.Collections.Generic.List<EnemyController> IItemKillActions.SharedTargets => tempEnemyTargets;
        EnemyController IItemKillActions.FindClosestEnemy(Vector3 center,float radius,System.Collections.Generic.List<EnemyController> excluded) => Queries.FindClosestEnemy(center,radius,excluded);
        void IItemKillActions.SpawnElectricChainVisual(Vector3 from,Vector3 to) => SpawnElectricChainVisual(from,to);
        void IItemKillActions.SpawnInfectionTransferVisual(Vector3 from,Vector3 to) => SpawnInfectionTransferVisual(from,to);
        void IItemKillActions.SpawnParasiticBombVisual(Vector3 center,float radius) => SpawnParasiticBombVisual(center,radius);
        void IItemKillActions.SpawnInfectedHost(EnemyController enemy) => Companions.TrySpawnInfectedHost(enemy);
        bool IItemKillActions.CanGrowBloodContract => Ownership.HasBloodContract && playerStats != null && !playerStats.IsDead;
        int IItemKillActions.BloodContractPeriod => bloodContractKillsPerHeal;
        int IItemKillActions.BloodContractMaximum => bloodContractMaxHealthGainCount;
        float IItemKillActions.BloodContractGain => bloodContractHealthGainAmount;
        bool IItemKillActions.HasStatsOwner => playerStats != null;
        // Keep the original cached recipient readable across synchronous removal callbacks.
        CharacterStats IItemKillActions.Recipient => playerStats?.RuntimeStats;
        object IItemKillActions.ModifierSource => this;
        void IItemKillActions.HealBloodContract(float gain) => playerStats.Heal(gain,GetItemHealthEffectSource(BloodContractId,"heal"));
    }
}
