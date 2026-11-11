using System;
using System.Collections.Generic;
namespace Necrocis
{
    [Serializable]
    public sealed class WorldRewardSaveData
    {
        public string id;
        public BiomeType biome;
        public float x, y, z;
        public bool recovery;
        public bool collected;
        public List<string> itemChoices = new List<string>();
    }

    [Serializable]
    public sealed class RegionRecoverySaveData
    {
        public BiomeType biome;
        public int normalKills;
        public int dropsCreated;
    }
}
