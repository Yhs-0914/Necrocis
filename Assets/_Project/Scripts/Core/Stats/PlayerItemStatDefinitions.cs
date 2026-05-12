using System;

namespace Necrocis
{
    public enum PlayerItemStatType
    {
        MaxHealth,
        MoveSpeed,
        AttackPower,
        AttackSpeed,
        AttackRange,
        Magic,
        SkillCooldownReduction
    }

    [Serializable]
    public struct PlayerItemStatModifierData
    {
        public PlayerItemStatType statType;
        public float value;
        public CharacterStatModifierMode mode;

        public PlayerItemStatModifierData(PlayerItemStatType statType, float value, CharacterStatModifierMode mode)
        {
            this.statType = statType;
            this.value = value;
            this.mode = mode;
        }

        public CharacterStatModifier ToModifier(object source)
        {
            return new CharacterStatModifier(statType.ToCharacterStatType(), value, mode, source);
        }
    }

    public static class PlayerItemStatTypeExtensions
    {
        public static CharacterStatType ToCharacterStatType(this PlayerItemStatType statType)
        {
            switch (statType)
            {
                case PlayerItemStatType.MaxHealth:
                    return CharacterStatType.MaxHealth;
                case PlayerItemStatType.MoveSpeed:
                    return CharacterStatType.MoveSpeed;
                case PlayerItemStatType.AttackPower:
                    return CharacterStatType.AttackPower;
                case PlayerItemStatType.AttackSpeed:
                    return CharacterStatType.AttackSpeed;
                case PlayerItemStatType.AttackRange:
                    return CharacterStatType.AttackRange;
                case PlayerItemStatType.Magic:
                    return CharacterStatType.Magic;
                case PlayerItemStatType.SkillCooldownReduction:
                    return CharacterStatType.SkillCooldownReduction;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }
    }
}
