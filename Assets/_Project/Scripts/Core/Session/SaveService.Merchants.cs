using System.Collections.Generic;

namespace Necrocis
{
    public static partial class SaveService
    {
        public static WorldMerchantSaveData FindWorldMerchant(BiomeType biome, int mapSeed)
            => HasActiveSession ? activeRun.world.merchants?.Find(value => value != null
                && value.biome == biome && value.mapSeed == mapSeed) : null;

        public static bool TryStoreWorldMerchant(WorldMerchantSaveData placement, out string error)
        {
            error = null;
            if (!HasActiveSession) return true;
            if (placement == null || placement.biome == BiomeType.None || placement.x < 0 || placement.y < 0)
            { error = "상인 배치 정보가 올바르지 않습니다."; return false; }
            var placements = activeRun.world.merchants ??= new List<WorldMerchantSaveData>();
            var previous = placements.Find(value => value != null && value.biome == placement.biome);
            if (previous != null) placements.Remove(previous);
            placements.Add(placement);
            if (TryWriteRun(activeRun, out error)) return true;
            placements.Remove(placement);
            if (previous != null) placements.Add(previous);
            return false;
        }
    }
}
