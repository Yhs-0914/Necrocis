using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Necrocis
{
    public static class WorldItemRewardChoice
    {
        public static WorldRewardSaveData Prepare(WorldRewardSaveData reward, PlayerItemManager inventory,
            Component pickup, string preferred)
        {
            bool isNew = reward == null;
            if (isNew)
            {
                Vector3 p = pickup.transform.position;
                string id = "pickup." + pickup.gameObject.scene.name + "." + pickup.name + "."
                    + p.x.ToString("R", CultureInfo.InvariantCulture) + "." + p.y.ToString("R", CultureInfo.InvariantCulture)
                    + "." + p.z.ToString("R", CultureInfo.InvariantCulture);
                reward = SaveService.FindWorldReward(id) ?? new WorldRewardSaveData
                {
                    id = id, biome = BiomeManager.Active != null ? BiomeManager.Active.BiomeType : BiomeType.None,
                    x = p.x, y = p.y, z = p.z
                };
            }
            if (reward.collected) return reward;
            var offers = CampaignRewardRules.RefreshOffers(reward.itemChoices,
                inventory.ItemEntries.Where(item => item != null).Select(item => item.ItemId),
                inventory.AcquiredItems.Select(item => item.ItemId), preferred, UnityEngine.Random.Range(1, int.MaxValue));
            bool changed = reward.itemChoices == null || !reward.itemChoices.SequenceEqual(offers, StringComparer.OrdinalIgnoreCase);
            reward.itemChoices = offers;
            if (isNew) reward = SaveService.RememberWorldReward(reward);
            else if (changed && SaveService.HasActiveSession) SaveService.TrySaveActiveRun(out _);
            return reward;
        }

        public static void PlayPickup(PlayerController player, string selected, Vector3 position)
        {
            var category = PlayerItemCategory.BasicProjectile;
            var inventory = player.GetComponent<PlayerItemManager>();
            if (inventory != null && inventory.TryGetItemEntry(selected, out var entry)) category = entry.Category;
            AudioManager.Instance?.PlayItemCategorySFX(category);
            CombatVfx.PlayItemPickup(position, player.transform, category);
        }
    }
}
