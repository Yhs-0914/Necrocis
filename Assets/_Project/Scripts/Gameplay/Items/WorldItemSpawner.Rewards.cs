using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Necrocis
{
    public partial class WorldItemSpawner
    {
        private readonly HashSet<string> activeRewardIds = new HashSet<string>();
        private bool starterRewardHandled;
        private bool fieldBoxHandled;

        private void Update()
        {
            if (!spawned || SaveService.IsRestorePending) return;
            if (!fieldBoxHandled) PlaceFieldBox();
            if (starterRewardHandled) return;
            const string rewardId = CampaignRewardRules.StarterRewardId;
            if (SaveService.FindWorldReward(rewardId) != null)
            {
                starterRewardHandled = true;
                return;
            }
            var player = PlayerController.Instance;
            var inventory = PlayerItemManager.Instance;
            var biome = BiomeManager.Active;
            var job = LevelUpManager.GetCurrentJob();
            if (player == null || inventory == null || biome == null || biome.gameObject != gameObject
                || biome.BiomeType == BiomeType.None || job == JobType.None) return;
            // Use the player's reachable entry tile, never a random point across a wall.
            var grid = biome.WorldToGrid(player.transform.position);
            if (!biome.IsValidPosition(grid.x, grid.y) || !biome.IsWalkable(grid.x, grid.y)) return;
            var offers = CampaignRewardRules.ChooseStarterOffers(
                inventory.ItemEntries.Where(item => item != null).Select(item => item.ItemId),
                inventory.AcquiredItems.Select(item => item.ItemId), Random.Range(1, int.MaxValue));
            if (offers.Count == 0) { starterRewardHandled = true; return; }
            var position = biome.GridToWorld(grid.x, grid.y);
            position.y = biome.GetGroundHeight(grid.x, grid.y) + itemGroundOffset;
            var reward = SaveService.RememberWorldReward(new WorldRewardSaveData
            {
                id = rewardId, biome = biome.BiomeType,
                x = position.x, y = position.y, z = position.z, itemChoices = offers
            });
            starterRewardHandled = SpawnStoredReward(reward);
        }

        private void PlaceFieldBox()
        {
            var biome = BiomeManager.Active;
            if (biome == null || biome.gameObject != gameObject || biome.BiomeType == BiomeType.None
                || PlayerItemManager.Instance == null) return;
            string id = $"region.{biome.BiomeType}.field-box";
            var existing = SaveService.FindWorldReward(id);
            if (existing != null && existing.collected)
            {
                fieldBoxHandled = true;
                return;
            }
            var terrain = GetComponent<ProceduralMap.MapGenerator>();
            if (terrain != null && !terrain.IsReady) return;
            var merchant = GetComponent<WorldMerchantSpawner>();
            var entrance = biome.WorldToGrid(biome.GetPlayerSpawnPosition());
            var recoveryCells = SaveService.WorldRewards.Where(r => r.biome == biome.BiomeType && r.recoveryOrgan)
                .Select(r => biome.WorldToGrid(new Vector3(r.x, r.y, r.z))).ToArray();
            bool Walkable(Vector2Int cell) => biome.IsWalkable(cell.x, cell.y)
                && (terrain == null || !terrain.Data.GetCell(cell.x, cell.y).Occupied);
            bool Excluded(Vector2Int cell) => (terrain != null && (terrain.IsCellReservedForBossArena(cell.x, cell.y)
                || terrain.Data.GetCell(cell.x, cell.y).HasLava))
                || (merchant != null && merchant.Merchant != null && (merchant.Merchant.Cell - cell).sqrMagnitude < 36)
                || System.Array.Exists(recoveryCells, other => (other - cell).sqrMagnitude < 36);
            var target = entrance + Vector2Int.right * 10;
            var candidates = MerchantPlacement.FindCandidates(biome.MapWidth, biome.MapHeight, entrance,
                merchant?.BossArea, Walkable, biome.CanStepBetweenHeights, Excluded);
            if (candidates.Count == 0)
            {
                Debug.LogWarning($"No reachable field box location in {biome.BiomeType}.");
                fieldBoxHandled = true;
                return;
            }
            var spot = candidates.OrderBy(candidate => (candidate.Cell - target).sqrMagnitude).First().Cell;
            var position = biome.GridToWorldWithHeight(spot.x, spot.y, itemGroundOffset);
            var reward = existing ?? PrepareReward(id, null, position);
            if (existing != null)
            {
                existing.x = position.x;
                existing.y = position.y;
                existing.z = position.z;
            }
            fieldBoxHandled = SpawnStoredReward(reward);
        }

        private WorldRewardSaveData PrepareReward(string id, PlayerItemManager.PlayerItemEntry preferred, Vector3 position)
        {
            var saved = SaveService.FindWorldReward(id);
            if (saved != null) return saved;
            var inventory = PlayerItemManager.Instance;
            var offers = CampaignRewardRules.ChooseOffers(inventory.ItemEntries.Where(item => item != null).Select(item => item.ItemId),
                inventory.AcquiredItems.Select(item => item.ItemId), preferred?.ItemId, Random.Range(1, int.MaxValue));
            return SaveService.RememberWorldReward(new WorldRewardSaveData
            {
                id = id, biome = BiomeManager.Active != null ? BiomeManager.Active.BiomeType : BiomeType.None,
                x = position.x, y = position.y, z = position.z, itemChoices = offers
            });
        }

        public bool TrySpawnEnemyReward(EnemyController enemy)
        {
            if (enemy == null || enemy.IsBossEncounter || enemy.SuppressExperienceReward) return false;
            var tier = enemy.IsElite ? MonsterTier.Elite : MonsterTier.Normal;
            if (!CampaignRewardRules.ShouldDropItem(tier, Random.value, dropScenario)) return false;
            return TrySpawnChoiceRewardAt("drop." + System.Guid.NewGuid().ToString("N"), enemy.transform.position);
        }

        private bool TrySpawnChoiceRewardAt(string id, Vector3 position, PlayerItemManager.PlayerItemEntry preferred = null,
            bool snapToGround = true, float searchRadius = 4f)
        {
            var biome = BiomeManager.Active;
            var inventory = PlayerItemManager.Instance;
            if (inventory == null) return false;
            var existing = SaveService.FindWorldReward(id);
            if (existing != null) return SpawnStoredReward(existing);
            Vector3 spawnPosition;
            if (biome == null || !snapToGround) spawnPosition = position + Vector3.up * itemGroundOffset;
            else if (!TryFindNearestWalkableSpawnPosition(biome, position, searchRadius, out spawnPosition))
            {
                // Boss arenas may use a separate floor. Keep the guaranteed reward reachable.
                spawnPosition = PlayerController.Instance != null ? PlayerController.Instance.transform.position : position;
                spawnPosition.y += itemGroundOffset;
            }
            return SpawnStoredReward(PrepareReward(id, preferred, spawnPosition));
        }

        private bool SpawnStoredReward(WorldRewardSaveData reward)
        {
            if (reward == null || reward.collected || activeRewardIds.Contains(reward.id)) return true;
            var inventory = PlayerItemManager.Instance;
            if (inventory == null) return false;
            PlayerItemManager.PlayerItemEntry entry = null;
            foreach (string id in reward.itemChoices ?? new List<string>())
                if (inventory.TryGetItemEntry(id, out entry)) break;
            if (entry == null)
            {
                reward.itemChoices = CampaignRewardRules.ChooseOffers(inventory.ItemEntries.Where(item => item != null).Select(item => item.ItemId),
                    inventory.AcquiredItems.Select(item => item.ItemId), null, Random.Range(1, int.MaxValue));
                if (reward.itemChoices.Count == 0 || !inventory.TryGetItemEntry(reward.itemChoices[0], out entry)) return false;
                if (SaveService.HasActiveSession) SaveService.TrySaveActiveRun(out _);
            }
            var pickup = SpawnItemObject(entry, new Vector3(reward.x, reward.y, reward.z));
            pickup.BindReward(reward);
            activeRewardIds.Add(reward.id);
            return true;
        }

        private void RestorePendingRewards(BiomeType biome)
        {
            foreach (var reward in SaveService.WorldRewards)
            {
                if (reward.biome != biome || reward.collected || reward.recoveryOrgan) continue;
                if (reward.recovery) WorldRecoveryPickup.Spawn(reward);
                else if (reward.id != $"region.{biome}.field-box") SpawnStoredReward(reward);
            }
            if (SaveService.HasActiveSession && SaveService.IsBossDefeated(biome)
                && SaveService.FindWorldReward("region." + biome + ".boss") == null && PlayerController.Instance != null)
                TrySpawnBossRewardAt(PlayerController.Instance.transform.position);
        }

        public bool TrySpawnBossRewardAt(Vector3 position)
        {
            var biome = BiomeManager.Active;
            if (biome == null) return false;
            string id = "region." + biome.BiomeType + ".boss";
            return TrySpawnChoiceRewardAt(id, position);
        }
    }
}
