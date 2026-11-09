using System;

namespace Necrocis
{
    // Deterministic rules shared by gameplay, save restoration and offline tests.
    public static class TempoBalanceRules
    {
        public static float LevelMultiplier(int level, float growthPerLevel, int maxLevel)
        {
            int steps = Math.Max(0, Math.Min(level, Math.Max(1, maxLevel)) - 1);
            float growth = float.IsNaN(growthPerLevel) || float.IsInfinity(growthPerLevel)
                ? 0f : Math.Max(0f, growthPerLevel);
            return 1f + steps * growth;
        }

        public static float FieldHitDamage(bool elite, bool hard) => elite ? (hard ? 4f : 3f) : 2f;

        public static bool RollDrop(float roll, float chance)
            => !float.IsNaN(roll) && !float.IsNaN(chance) && roll >= 0f && roll < 1f
               && roll < Math.Min(1f, Math.Max(0f, chance));

        // Bits are persisted, so dropping an item or changing scenes cannot replay a reward.
        public static int PendingItemReward(int level, int claimedMask, bool starterEnabled, int midgameLevel)
        {
            if (starterEnabled && (claimedMask & 1) == 0) return 1;
            if (midgameLevel > 0 && level >= midgameLevel && (claimedMask & 2) == 0) return 2;
            return 0;
        }
    }
}
