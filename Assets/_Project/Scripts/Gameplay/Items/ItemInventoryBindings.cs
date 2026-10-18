using System;
using System.Collections.Generic;
using UnityEngine;
namespace Necrocis
{
    public interface IItemEffectLifecycle
    {
        bool IsAvailable { get; }
        string DescribeItem(string id);
        void EnsureActiveForInventoryChange();
        void RetireItemHealthSources(string id);
        void CapturePersistentItemState(SavedItemStateData state);
        void RestorePersistentItemStates(IReadOnlyList<SavedItemStateData> states);
    }
    public readonly struct ItemDescriptionPolicy
    {
        public readonly bool GambleEnabled;
        public readonly int GambleMinimum,GambleMaximum;
        public ItemDescriptionPolicy(bool enabled,int minimum,int maximum){GambleEnabled=enabled;GambleMinimum=minimum;GambleMaximum=maximum;}
    }
    public static class ItemInventoryBindings
    {
        public static Func<GameObject,IItemStatRecipient> StatRecipient;
        public static Func<int> SlotOverride;
        public static Func<ItemDescriptionPolicy> DescriptionPolicy;
        public static Action<PlayerItemBase,IItemStatRecipient> ApplyImplementation;
        public static Action<PlayerItemBase,IItemStatRecipient> RemoveImplementation;
    }
}
