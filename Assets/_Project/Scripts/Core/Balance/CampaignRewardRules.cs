using System;
using System.Collections.Generic;
using System.Linq;

namespace Necrocis
{
    public enum CampaignRewardRole { Attack, Survival, Utility }

    public static class CampaignRewardRules
    {
        public const string StarterRewardId = "campaign.starter-mutation";
        public const int NormalKillsPerRecovery = 30;
        public const int RecoveriesPerRegion = 3;
        public const float RecoveryHealth = 2f;
        public const float NormalItemDropChance = 0f;
        public const float EliteItemDropChance = 0.15f;

        public static bool ShouldDropItem(MonsterTier tier, float roll) => tier == MonsterTier.Boss
            || (roll >= 0f && roll < (tier == MonsterTier.Elite ? EliteItemDropChance : NormalItemDropChance));

        // Draw from the entire catalog without job preferences or role quotas.
        public static List<string> ChooseStarterOffers(IEnumerable<string> catalog, IEnumerable<string> owned, int seed)
        {
            var excluded = new HashSet<string>(owned ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var available = (catalog ?? Array.Empty<string>()).Where(id => !string.IsNullOrWhiteSpace(id)
                    && (!excluded.Contains(id) || PlayerStatItemCatalog.IsRepeatable(id)))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(id => id, StringComparer.Ordinal).ToList();
            var random = new Random(seed);
            var result = new List<string>(3);
            while (result.Count < 3 && available.Count > 0)
            {
                int index = random.Next(available.Count);
                result.Add(available[index]);
                available.RemoveAt(index);
            }
            return result;
        }

        private static readonly HashSet<string> survival = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "exoskeleton", "platelet_membrane", "recovery_factor", "reflective_skin", "bio_barrier",
          "split_regeneration", "imperfect_regeneration", "blood_contract", "guardian_organ", "vital_cell", "vital_organ" };
        private static readonly HashSet<string> utility = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "homing_cell", "hypertrophy_cell", "piercing_mucus", "pulse_bullet", "vascular_reflection",
          "freezing_nerve", "overclock_nerve", "muscle_spasm", "gluttonous_organ", "tentacle_colony", "elastic_fiber",
          "elongated_fiber", "active_fiber" };

        public static CampaignRewardRole Role(string id) => survival.Contains(id) ? CampaignRewardRole.Survival
            : utility.Contains(id) ? CampaignRewardRole.Utility : CampaignRewardRole.Attack;

        public static List<string> ChooseOffers(IEnumerable<string> catalog, IEnumerable<string> owned, string preferred, int seed)
        {
            var excluded = new HashSet<string>(owned ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var available = (catalog ?? Array.Empty<string>()).Where(id => !string.IsNullOrWhiteSpace(id)
                    && (!excluded.Contains(id) || PlayerStatItemCatalog.IsRepeatable(id)))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(id => id, StringComparer.Ordinal).ToList();
            var random = new Random(seed);
            var result = new List<string>(3);
            foreach (CampaignRewardRole role in Enum.GetValues(typeof(CampaignRewardRole)))
            {
                var pool = available.Where(id => Role(id) == role).ToList();
                if (pool.Count == 0) continue;
                string choice = pool.Find(id => string.Equals(id, preferred, StringComparison.OrdinalIgnoreCase)) ?? pool[random.Next(pool.Count)];
                result.Add(choice);
                available.Remove(choice);
            }
            while (result.Count < 3 && available.Count > 0)
            {
                int index = random.Next(available.Count);
                result.Add(available[index]);
                available.RemoveAt(index);
            }
            return result;
        }

        // Keep saved choices stable; replace choices acquired from another box in the meantime.
        public static List<string> RefreshOffers(IEnumerable<string> previous, IEnumerable<string> catalog,
            IEnumerable<string> owned, string preferred, int seed)
        {
            var excluded = new HashSet<string>(owned ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var available = new HashSet<string>((catalog ?? Array.Empty<string>()).Where(id => !string.IsNullOrWhiteSpace(id)
                && (!excluded.Contains(id) || PlayerStatItemCatalog.IsRepeatable(id))), StringComparer.OrdinalIgnoreCase);
            var result = (previous ?? Array.Empty<string>()).Where(available.Contains)
                .Distinct(StringComparer.OrdinalIgnoreCase).Take(3).ToList();
            if (result.Count == 3) return result;
            foreach (string id in ChooseOffers(available, Array.Empty<string>(), preferred, seed))
                if (!result.Contains(id, StringComparer.OrdinalIgnoreCase) && result.Count < 3) result.Add(id);
            return result;
        }

        public static bool RecoveryDue(int normalKills, int alreadyCreated) => alreadyCreated < RecoveriesPerRegion
            && normalKills >= (alreadyCreated + 1) * NormalKillsPerRecovery;

        public static bool TryUseEncounter(ICollection<string> used, string key)
        {
            if (string.IsNullOrEmpty(key) || used.Contains(key)) return false;
            used.Add(key);
            return true;
        }
    }

}
