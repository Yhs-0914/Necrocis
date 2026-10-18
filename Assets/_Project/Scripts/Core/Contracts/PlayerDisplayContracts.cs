using System;
using System.Collections.Generic;
using UnityEngine;
namespace Necrocis
{
    public interface IHealthDisplay
    {
        UnityEngine.Object Owner {get;}
        float CurrentHealth {get;}
        float MaxHealth {get;}
        event Action<float,float> OnHealthChanged;
    }
    public interface IProgressionDisplay
    {
        int Level {get;}
        JobType Job {get;}
        bool IsSkillUnlocked(int slot);
        event Action LevelChanged;
        event Action JobSelection;
        event Action<JobType> JobChanged;
    }
    public interface IStatsDisplay {UnityEngine.Object Owner {get;} CharacterStats RuntimeStats {get;}}
    public interface ISkillCooldownDisplay
    {
        UnityEngine.Object Owner {get;}
        float GetRemainingCooldown(int slot);
        CooldownSnapshot GetCooldownSnapshot(int slot);
        event Action<int,float> CooldownStarted;
        event Action<int> CooldownReset;
    }
    public interface IInventoryItemView
    {
        string ItemId {get;}
        string DisplayName {get;}
        string Description {get;}
        Sprite Icon {get;}
    }
    public interface IInventoryDisplay
    {
        UnityEngine.Object Owner {get;}
        int MaxItemSlots {get;}
        bool IsRestoringSavedItems {get;}
        IReadOnlyList<IInventoryItemView> AcquiredItems {get;}
        bool RemoveItem(string itemId);
        event Action<IInventoryItemView> ItemAcquired;
        event Action<IInventoryItemView> ItemRemoved;
    }
    // Ports are installed by upper composition, without constructing UI or retaining an actor.
    public static class PlayerDisplayBindings
    {
        public static Func<GameObject,IHealthDisplay> Health;
        public static Func<GameObject,IStatsDisplay> Stats;
        public static Func<GameObject,ISkillCooldownDisplay> Skills;
        public static Func<GameObject,IInventoryDisplay> Inventory;
        public static IProgressionDisplay Progression;
    }
}
