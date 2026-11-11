using System;
using System.Collections.Generic;

namespace Necrocis
{
    [Serializable]
    public sealed class MonsterVisitSaveData
    {
        public string visitId;
        public BiomeType biome;
        public int stageAtEntry;
    }

    [Serializable]
    public sealed class BiomeElitePlacement
    {
        public string spawnId;
        public string monsterId;
        public int x;
        public int y;
    }

    [Serializable]
    public sealed class BiomeElitePlan
    {
        public BiomeType biome;
        public int mapSeed;
        public int requestedCount;
        public List<BiomeElitePlacement> placements = new List<BiomeElitePlacement>();
    }
}
