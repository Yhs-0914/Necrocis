using System;

namespace Necrocis
{
    public enum PlayerStatType
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
    public struct PlayerStatModifierData
    {
        public PlayerStatType statType;
        public float value;
        public CharacterStatModifierMode mode;

        public PlayerStatModifierData(PlayerStatType statType, float value, CharacterStatModifierMode mode)
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

    public static class PlayerStatTypeExtensions
    {
        public static CharacterStatType ToCharacterStatType(this PlayerStatType statType)
        {
            switch (statType)
            {
                case PlayerStatType.MaxHealth:
                    return CharacterStatType.MaxHealth;
                case PlayerStatType.MoveSpeed:
                    return CharacterStatType.MoveSpeed;
                case PlayerStatType.AttackPower:
                    return CharacterStatType.AttackPower;
                case PlayerStatType.AttackSpeed:
                    return CharacterStatType.AttackSpeed;
                case PlayerStatType.AttackRange:
                    return CharacterStatType.AttackRange;
                case PlayerStatType.Magic:
                    return CharacterStatType.Magic;
                case PlayerStatType.SkillCooldownReduction:
                    return CharacterStatType.SkillCooldownReduction;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }
    }
}
