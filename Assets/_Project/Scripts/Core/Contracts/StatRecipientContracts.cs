using System.Collections.Generic;
namespace Necrocis
{
    public interface IPlayerGrowthRecipient
    {
        bool IsAvailable { get; }
        void ApplyModifier(CharacterStatModifier modifier);
        void Heal(float amount);
    }
    public interface IItemStatRecipient
    {
        bool IsAvailable { get; }
        void ApplyPlayerItemStatModifiers(IEnumerable<PlayerItemStatModifierData> modifiers, object source);
        int RemoveModifiersFromSource(object source);
    }
}
