using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public abstract class PlayerItemBase : ScriptableObject
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [TextArea]
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private List<PlayerItemStatModifierData> statModifiers = new List<PlayerItemStatModifierData>();

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public IReadOnlyList<PlayerItemStatModifierData> StatModifiers => statModifiers;

        public virtual void ApplyTo(PlayerStats playerStats)
        {
            if (playerStats == null)
            {
                return;
            }

            playerStats.ApplyPlayerItemStatModifiers(statModifiers, this);
        }

        public virtual void RemoveFrom(PlayerStats playerStats)
        {
            if (playerStats == null)
            {
                return;
            }

            playerStats.RemoveModifiersFromSource(this);
        }
    }
}
