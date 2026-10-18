using System;
using System.Collections.Generic;
using UnityEngine;
namespace Necrocis
{
    public static class PlayerDisplayAdapters
    {
        public static void Register()
        {
            PlayerDisplayBindings.Health=owner=>{
                var health=owner!=null?owner.GetComponent<Health>():null;
                if(health==null)health=UnityEngine.Object.FindFirstObjectByType<Health>();
                return health!=null?new HealthDisplay(health):null;};
            PlayerDisplayBindings.Stats=owner=>PlayerStats.Instance!=null?new StatsDisplay(PlayerStats.Instance):null;
            PlayerDisplayBindings.Skills=owner=>{
                var skills=owner!=null?owner.GetComponent<PlayerClassSkillController>():null;
                if(skills==null)skills=UnityEngine.Object.FindFirstObjectByType<PlayerClassSkillController>();
                return skills!=null?new SkillDisplay(skills):null;};
            PlayerDisplayBindings.Inventory=owner=>{
                var items=owner.GetComponent<PlayerItemManager>();
                // Compatibility for an explicitly added inventory/notifier on an empty owner.
                if(items==null)items=owner.AddComponent<PlayerItemManager>();
                return new InventoryDisplay(items);};
            PlayerDisplayBindings.Progression=new ProgressionDisplay();
        }
        private sealed class HealthDisplay : IHealthDisplay
        {
            private readonly Health health;public HealthDisplay(Health health){this.health=health;}
            public UnityEngine.Object Owner=>health;
            public float CurrentHealth=>health.CurrentHealth;
            public float MaxHealth=>health.MaxHealth;
            public event Action<float,float> OnHealthChanged {add{health.OnHealthChanged+=value;}remove{health.OnHealthChanged-=value;}}
        }
        private sealed class StatsDisplay : IStatsDisplay
        {
            private readonly PlayerStats stats;public StatsDisplay(PlayerStats stats){this.stats=stats;}
            public UnityEngine.Object Owner=>stats;
            public CharacterStats RuntimeStats=>stats!=null?stats.RuntimeStats:null;
        }
        private sealed class ProgressionDisplay : IProgressionDisplay
        {
            public int Level=>LevelUpManager.GetCurrentLevel();public JobType Job=>LevelUpManager.GetCurrentJob();
            public bool IsSkillUnlocked(int slot)=>LevelUpManager.IsSkillUnlocked(slot);
            public event Action LevelChanged {add{LevelUpManager.OnLevelUp+=value;}remove{LevelUpManager.OnLevelUp-=value;}}
            public event Action JobSelection {add{LevelUpManager.OnJobSelect+=value;}remove{LevelUpManager.OnJobSelect-=value;}}
            public event Action<JobType> JobChanged {add{LevelUpManager.OnJobChanged+=value;}remove{LevelUpManager.OnJobChanged-=value;}}
        }
        private sealed class SkillDisplay : ISkillCooldownDisplay
        {
            private readonly PlayerClassSkillController skills;
            private readonly Dictionary<Action<int,float>,List<Action<PlayerClassSkillController.SkillSlot,float>>> starts=new Dictionary<Action<int,float>,List<Action<PlayerClassSkillController.SkillSlot,float>>>();
            private readonly Dictionary<Action<int>,List<Action<PlayerClassSkillController.SkillSlot>>> resets=new Dictionary<Action<int>,List<Action<PlayerClassSkillController.SkillSlot>>>();
            public SkillDisplay(PlayerClassSkillController skills){this.skills=skills;}
            public UnityEngine.Object Owner=>skills;
            private static PlayerClassSkillController.SkillSlot Slot(int value)=>value==1?PlayerClassSkillController.SkillSlot.Skill1:PlayerClassSkillController.SkillSlot.Skill2;
            public float GetRemainingCooldown(int slot)=>skills.GetRemainingCooldown(Slot(slot));
            public CooldownSnapshot GetCooldownSnapshot(int slot)=>skills.GetCooldownSnapshot(Slot(slot));
            public event Action<int,float> CooldownStarted
            {
                add{Action<PlayerClassSkillController.SkillSlot,float> bridge=(slot,duration)=>value((int)slot+1,duration);if(!starts.TryGetValue(value,out var list))starts[value]=list=new List<Action<PlayerClassSkillController.SkillSlot,float>>();list.Add(bridge);skills.CooldownStarted+=bridge;}
                remove{if(starts.TryGetValue(value,out var list)&&list.Count>0){var bridge=list[list.Count-1];list.RemoveAt(list.Count-1);skills.CooldownStarted-=bridge;if(list.Count==0)starts.Remove(value);}}
            }
            public event Action<int> CooldownReset
            {
                add{Action<PlayerClassSkillController.SkillSlot> bridge=slot=>value((int)slot+1);if(!resets.TryGetValue(value,out var list))resets[value]=list=new List<Action<PlayerClassSkillController.SkillSlot>>();list.Add(bridge);skills.CooldownReset+=bridge;}
                remove{if(resets.TryGetValue(value,out var list)&&list.Count>0){var bridge=list[list.Count-1];list.RemoveAt(list.Count-1);skills.CooldownReset-=bridge;if(list.Count==0)resets.Remove(value);}}
            }
        }
        private sealed class InventoryDisplay : IInventoryDisplay
        {
            private readonly PlayerItemManager items;
            private readonly Dictionary<Action<IInventoryItemView>,List<Action<PlayerItemManager,PlayerItemManager.AcquiredPlayerItem>>> added=new Dictionary<Action<IInventoryItemView>,List<Action<PlayerItemManager,PlayerItemManager.AcquiredPlayerItem>>>();
            private readonly Dictionary<Action<IInventoryItemView>,List<Action<PlayerItemManager,PlayerItemManager.AcquiredPlayerItem>>> removed=new Dictionary<Action<IInventoryItemView>,List<Action<PlayerItemManager,PlayerItemManager.AcquiredPlayerItem>>>();
            public InventoryDisplay(PlayerItemManager items){this.items=items;}
            public UnityEngine.Object Owner=>items;
            public int MaxItemSlots=>items.MaxItemSlots;
            public bool IsRestoringSavedItems=>items.IsRestoringSavedItems;
            public IReadOnlyList<IInventoryItemView> AcquiredItems=>items.AcquiredItems;
            public bool RemoveItem(string id)=>items.RemoveItem(id);
            private void Add(Action<IInventoryItemView> callback,bool acquire)
            {
                var map=acquire?added:removed;Action<PlayerItemManager,PlayerItemManager.AcquiredPlayerItem> bridge=(_,item)=>callback(item);
                if(!map.TryGetValue(callback,out var list))map[callback]=list=new List<Action<PlayerItemManager,PlayerItemManager.AcquiredPlayerItem>>();list.Add(bridge);
                if(acquire)items.ItemAcquired+=bridge;else items.ItemRemoved+=bridge;
            }
            private void Remove(Action<IInventoryItemView> callback,bool acquire)
            {
                var map=acquire?added:removed;if(!map.TryGetValue(callback,out var list)||list.Count==0)return;
                var bridge=list[list.Count-1];list.RemoveAt(list.Count-1);if(list.Count==0)map.Remove(callback);
                if(acquire)items.ItemAcquired-=bridge;else items.ItemRemoved-=bridge;
            }
            public event Action<IInventoryItemView> ItemAcquired {add{Add(value,true);}remove{Remove(value,true);}}
            public event Action<IInventoryItemView> ItemRemoved {add{Add(value,false);}remove{Remove(value,false);}}
        }
    }
}
