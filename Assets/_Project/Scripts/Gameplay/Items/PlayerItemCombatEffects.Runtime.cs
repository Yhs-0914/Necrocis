using System.Collections.Generic;
using UnityEngine;
namespace Necrocis
{
    public partial class PlayerItemCombatEffects : IItemCombatHost
    {
        private ItemOwnershipTransitions ownership;
        private ItemTimedModifiers timed;
        private ItemAttackEffects attack;
        private ItemCombatQueries queries;
        private void EnsureExecutors()
        {
            if(ownership!=null)return;
            ownership=new ItemOwnershipTransitions(this);
            queries=new ItemCombatQueries(this);
            timed=new ItemTimedModifiers(this,ownership,queries);
            attack=new ItemAttackEffects(this,ownership,queries,timed);
            ownership.timed=timed;
            ownership.attack=attack;
        }
        private ItemOwnershipTransitions Ownership {get{EnsureExecutors();return ownership;}}
        private ItemTimedModifiers Timed {get{EnsureExecutors();return timed;}}
        private ItemAttackEffects Attack {get{EnsureExecutors();return attack;}}
        private ItemCombatQueries Queries {get{EnsureExecutors();return queries;}}
        IItemCombatSettings IItemCombatHost.Settings=>this;
        Transform IItemCombatHost.Transform=>transform;
        bool IItemCombatHost.HasStatsOwner=>playerStats!=null;
        bool IItemCombatHost.HasManagedStatsOwner=>!ReferenceEquals(playerStats,null);
        // Preserve C# null-conditional read-through of a destroyed cached Unity component.
        CharacterStats IItemCombatHost.Recipient=>playerStats?.RuntimeStats;
        float IItemCombatHost.AttackPower=>playerStats.AttackPower;
        float IItemCombatHost.CurrentHealth=>playerStats.CurrentHealth;
        float IItemCombatHost.MaxHealth=>playerStats.MaxHealth;
        bool IItemCombatHost.IsDead=>playerStats.IsDead;
        bool IItemCombatHost.HasPlayerController=>playerController!=null;
        Transform IItemCombatHost.PlayerTransform=>playerController.transform;
        bool IItemCombatHost.PlayerIsMoving=>playerController.IsMoving;
        Vector3 IItemCombatHost.GlobalPlayerPosition=>PlayerController.Instance!=null?PlayerController.Instance.transform.position:Vector3.zero;
        object IItemCombatHost.ModifierSource=>this;
        IProjectileItemEffects IItemCombatHost.ProjectileEffects=>this;
        List<EnemyController> IItemCombatHost.SharedTargets=>tempEnemyTargets;
        ItemKillEffectState IItemCombatHost.KillState=>killState;
        ItemCompanionRuntime IItemCombatHost.Companions=>Companions;
        int IItemCombatHost.ExistingCompanionCount=>companionRuntime!=null?companionRuntime.ActiveCount:0;
        SpriteRenderer IItemCombatHost.UnstableCoreOverlay=>unstableCoreOverlay;
        bool IItemCombatHost.Enabled{get=>enabled;set=>enabled=value;}
        bool IItemCombatHost.InventoryContains(string id)
        {
            if(itemManager==null)itemManager=GetComponent<PlayerItemManager>();
            return itemManager!=null&&itemManager.ContainsItem(id);
        }
        void IItemCombatHost.CopyItemIdsTo(HashSet<string> target)
        {
            if(itemManager==null||itemManager.AcquiredItems==null)return;
            IReadOnlyList<PlayerItemManager.AcquiredPlayerItem> items=itemManager.AcquiredItems;
            for(int i=0;i<items.Count;i++)
            {
                string id=items[i].ItemId;
                if(!string.IsNullOrWhiteSpace(id))target.Add(id);
            }
        }
        void IItemCombatHost.SleepEmptyInventory(){disablingForEmptyInventory=true;enabled=false;}
        void IItemCombatHost.Heal(float amount,HealthEffectSource source)=>playerStats.Heal(amount,source);
        void IItemCombatHost.TakeDamage(float amount,HealthEffectSource source)=>playerStats.TakeDamage(amount,source);
        void IItemCombatHost.ApplyBypassDamage(float amount,HealthEffectSource source)
        {
            var coordinator=GetComponent<PlayerDamageCoordinator>();
            if(coordinator!=null)coordinator.ApplyBypassDamage(amount,source);
            else playerStats.TakeDamage(amount,source);
        }
        Vector3 IItemCombatHost.GetPlayerVisualCenter()=>GetPlayerVisualCenter();
        void IItemCombatHost.SpawnLineVisual(string name,Vector3 start,Vector3 end,Color first,Color last,float firstWidth,float lastWidth,float duration,string texture,int sorting)
            =>SpawnLineVisual(name,start,end,first,last,firstWidth,lastWidth,duration,texture,sorting);
        void IItemCombatHost.SpawnOverheatExplosionVisual()=>SpawnOverheatExplosionVisual();
        void IItemCombatHost.SpawnTentacleBindVisual(Vector3 position,float duration)=>SpawnTentacleBindVisual(position,duration);
        void IItemCombatHost.ClearPlateletMembraneOutline()=>ClearPlateletMembraneOutline();
        void IItemCombatHost.UpdatePlateletMembraneOutline()=>UpdatePlateletMembraneOutline();
        void IItemCombatHost.ClearKillChainStatBuff(object source,ref float value)=>ClearKillChainStatBuff(source,ref value);
    }
}
