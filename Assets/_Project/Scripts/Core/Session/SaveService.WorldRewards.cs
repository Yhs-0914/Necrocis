using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Necrocis
{
    public static partial class SaveService
    {
        public static IReadOnlyList<WorldRewardSaveData> WorldRewards => HasActiveSession
            ? (activeRun.world.rewards ??= new List<WorldRewardSaveData>()) : Array.Empty<WorldRewardSaveData>();

        public static WorldRewardSaveData FindWorldReward(string id) => string.IsNullOrEmpty(id) ? null
            : WorldRewards.FirstOrDefault(value => value.id == id);

        public static WorldRewardSaveData RememberWorldReward(WorldRewardSaveData reward)
        {
            var existing = FindWorldReward(reward.id);
            if (existing != null) return existing;
            if (HasActiveSession)
            {
                activeRun.world.rewards.Add(reward);
                SaveRewardProgress();
            }
            return reward;
        }

        // The caller grants the item/heal first, then saves inventory and consumption together.
        public static void CompleteWorldReward(WorldRewardSaveData reward)
        {
            reward.collected = true;
            BalanceMetrics?.Record(reward.recovery ? "recovery_pickup" : "reward_claim", reward.id);
            SaveRewardProgress();
        }

        private static void SaveRewardProgress()
        {
            if (HasActiveSession && !TrySaveActiveRun(out string error))
                Debug.LogWarning("[WorldReward] " + error);
        }

        public static bool TryUseBerserkEncounter(string key)
        {
            if (!HasActiveSession) return true;
            activeRun.world.berserkEncounters ??= new List<string>();
            if (string.IsNullOrEmpty(key) || activeRun.world.berserkEncounters.Contains(key)) return false;
            activeRun.world.berserkEncounters.Add(key);
            SaveRewardProgress();
            return true;
        }

        public static WorldRewardSaveData RegisterNormalKillForRecovery(BiomeType biome, Vector3 position)
        {
            if (!HasActiveSession || biome == BiomeType.None) return null;
            activeRun.world.recoveryProgress ??= new List<RegionRecoverySaveData>();
            var progress = activeRun.world.recoveryProgress.FirstOrDefault(value => value.biome == biome);
            if (progress == null)
            {
                progress = new RegionRecoverySaveData { biome = biome };
                activeRun.world.recoveryProgress.Add(progress);
            }
            progress.normalKills++;
            if (!(progress.dropsCreated < 3 && progress.normalKills >= (progress.dropsCreated + 1) * 30)) return null;
            string id = "region." + biome + ".recovery." + progress.dropsCreated++;
            return RememberWorldReward(new WorldRewardSaveData
            { id = id, biome = biome, recovery = true, x = position.x, y = position.y, z = position.z });
        }
    }
}
