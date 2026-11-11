using System;
using System.Collections.Generic;

namespace Necrocis
{
    [Serializable]
    public sealed class RegionBalanceMetrics
    {
        public BiomeType biome;
        public float fieldSeconds, bossSeconds;
        public int normalKills, eliteKills, experience, bossClears;
        public float normalDamage, eliteDamage, bossDamage;
    }

    [Serializable]
    public sealed class CampaignBalanceEvent
    {
        public float seconds;
        public string kind, value;
    }

    [Serializable]
    public sealed class CampaignBalanceMetrics
    {
        public float activeSeconds, hubSeconds, finalBossSeconds;
        public float sessionSeconds, clearSessionSeconds = -1f;
        public float enemyDamageDealt, playerDamageTaken, healing;
        public float finalBossDamage;
        public string job;
        public float level10Seconds = -1f, level20Seconds = -1f, firstItemSeconds = -1f, clearSeconds = -1f;
        public int deaths, skill1Uses, skill2Uses;
        public List<RegionBalanceMetrics> regions = new List<RegionBalanceMetrics>();
        public List<CampaignBalanceEvent> events = new List<CampaignBalanceEvent>();

        public RegionBalanceMetrics Region(BiomeType biome)
        {
            regions ??= new List<RegionBalanceMetrics>();
            var row = regions.Find(value => value.biome == biome);
            if (row == null) { row = new RegionBalanceMetrics { biome = biome }; regions.Add(row); }
            return row;
        }

        public void Tick(float seconds, BiomeType biome, GameState state)
        {
            if (seconds <= 0f || float.IsNaN(seconds) || float.IsInfinity(seconds)) return;
            activeSeconds += seconds;
            if (state == GameState.InHub) hubSeconds += seconds;
            else if (state == GameState.InFinalBoss) finalBossSeconds += seconds;
            else if (biome != BiomeType.None)
            {
                if (state == GameState.InBossRoom) Region(biome).bossSeconds += seconds;
                else Region(biome).fieldSeconds += seconds;
            }
        }

        public void TickSession(float seconds)
        {
            if (seconds > 0f && !float.IsNaN(seconds) && !float.IsInfinity(seconds)) sessionSeconds += seconds;
        }

        public void Record(string kind, string value)
        {
            events ??= new List<CampaignBalanceEvent>();
            if (events.Count < 1024) events.Add(new CampaignBalanceEvent { seconds = activeSeconds, kind = kind, value = value });
        }
    }
}
