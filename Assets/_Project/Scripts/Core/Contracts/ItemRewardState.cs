using System;
using System.Collections.Generic;

namespace Necrocis
{
    [Serializable]
    public sealed class ItemRewardState
    {
        public int version;
        public int claimedMask;
        public int pendingReward;
        public List<string> pendingItemIds = new List<string>();

        public static ItemRewardState ForNewRun() => new ItemRewardState { version = 1 };

        public void MigrateLegacy(int level, int midgameLevel)
        {
            // Existing runs were balanced without these grants; do not replay either reward.
            if (version < 1)
            {
                claimedMask = 1 | (midgameLevel > 0 && level >= midgameLevel ? 2 : 0);
                pendingReward = 0; pendingItemIds.Clear(); version = 1;
            }
        }

        public bool BeginOffer(int reward, IEnumerable<string> ids)
        {
            if (pendingReward != 0 || (reward != 1 && reward != 2) || (claimedMask & reward) != 0 || ids == null)
                return false;
            var choices = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string id in ids)
            {
                if (string.IsNullOrWhiteSpace(id) || !seen.Add(id)) return false;
                choices.Add(id);
            }
            if (choices.Count == 0 || choices.Count > 3) return false;
            pendingReward = reward;
            pendingItemIds = choices;
            return true;
        }

        public bool Claim(string id)
        {
            if ((pendingReward != 1 && pendingReward != 2) || (claimedMask & pendingReward) != 0
                || !pendingItemIds.Contains(id)) return false;
            claimedMask |= pendingReward;
            pendingReward = 0;
            pendingItemIds.Clear();
            return true;
        }

        public ItemRewardState Copy() => new ItemRewardState
        {
            version = version, claimedMask = claimedMask, pendingReward = pendingReward,
            pendingItemIds = pendingItemIds != null ? new List<string>(pendingItemIds) : new List<string>()
        };
    }
}
