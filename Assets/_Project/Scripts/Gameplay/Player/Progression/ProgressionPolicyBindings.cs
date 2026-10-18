using System;
namespace Necrocis
{
    // Upper composition supplies the current session policy at each original read point.
    public static class ProgressionPolicyBindings
    {
        public static Func<LevelProgressionConfig> ResolveActiveConfig;
        public static Func<CharacterStatValue[]> BuildPlayerBaseStats;
    }
    public static class GrowthChoiceBindings
    {
        public static Func<IPlayerGrowthRecipient> ResolveRecipient;
        public static Func<IPlayerGrowthRecipient,bool> OwnsBioGamble;
        public static Func<bool> AnyPlayerOwnsBioGamble;
    }
}
