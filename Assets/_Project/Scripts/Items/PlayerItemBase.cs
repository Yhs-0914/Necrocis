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
        [SerializeField] private List<PlayerStatModifierData> statModifiers = new List<PlayerStatModifierData>();

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public IReadOnlyList<PlayerStatModifierData> StatModifiers => statModifiers;

        public virtual void ApplyTo(PlayerStats playerStats)
        {
            if (playerStats == null)
            {
                return;
            }

            playerStats.ApplyPlayerStatModifiers(statModifiers, this);
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
