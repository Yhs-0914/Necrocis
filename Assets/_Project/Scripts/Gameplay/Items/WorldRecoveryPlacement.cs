using System.Collections.Generic;
using ProceduralMap;
using UnityEngine;

namespace Necrocis
{
    public static class WorldRecoveryPlacement
    {
        public static void SpawnForBiome(ProceduralBiomeBridge biome, MapGenerator terrain, WorldMerchantSpawner merchantSpawner)
        {
            var biomeType = biome.BiomeType;
            if (merchantSpawner == null || !terrain.IsReady) return;

            string prefix = $"region.{biomeType}.recovery-organ.{biome.Seed}.";
            var approach = SaveService.FindWorldReward(prefix + "approach");
            var field = SaveService.FindWorldReward(prefix + "field");
            if (approach != null && field != null)
            {
                WorldRecoveryPickup.Spawn(approach);
                WorldRecoveryPickup.Spawn(field);
                return;
            }

            var bossArea = merchantSpawner.BossArea;
            var entrance = biome.WorldToGrid(biome.GetPlayerSpawnPosition());
            var portal = biome.GetBiomeConfig().GetReturnPortalConfig();
            var portalCell = portal != null && portal.useCustomPosition ? portal.gridPosition : entrance;
            Vector2 route = bossArea.HasValue ? (bossArea.Value.center - (Vector2)entrance).normalized : Vector2.up;
            Vector2Int? merchantCell = merchantSpawner.Merchant != null ? merchantSpawner.Merchant.Cell : (Vector2Int?)null;
            var boxes = new List<Vector2Int>();
            foreach (var reward in SaveService.WorldRewards)
                if (reward.biome == biomeType && !reward.recovery && !reward.collected)
                    boxes.Add(biome.WorldToGrid(new Vector3(reward.x, reward.y, reward.z)));

            bool Walkable(Vector2Int cell) => biome.IsWalkable(cell.x, cell.y) && !terrain.Data.GetCell(cell.x, cell.y).Occupied;
            bool Excluded(Vector2Int cell)
            {
                if (terrain.IsCellReservedForBossArena(cell.x, cell.y)
                    || terrain.Data.GetCell(cell.x, cell.y).HasLava
                    || (cell - portalCell).sqrMagnitude < 10 * 10
                    || (merchantCell.HasValue && (cell - merchantCell.Value).sqrMagnitude < 18 * 18)) return true;
                foreach (var box in boxes)
                    if ((cell - box).sqrMagnitude < 12 * 12) return true;
                return false;
            }

            var candidates = MerchantPlacement.FindCandidates(terrain.MapWidth, terrain.MapHeight, entrance, bossArea,
                Walkable, biome.CanStepBetweenHeights, Excluded);

            WorldRewardSaveData Place(string slot, WorldRewardSaveData other, System.Func<Vector2Int, float> score, int salt)
            {
                Vector2Int? otherCell = other != null ? biome.WorldToGrid(new Vector3(other.x, other.y, other.z)) : (Vector2Int?)null;
                float ScoreWithClearance(Vector2Int cell)
                {
                    if (otherCell.HasValue)
                    {
                        // Check the closest cell in the candidate's 3x3 footprint.
                        int dx = Mathf.Max(0, Mathf.Abs(cell.x - otherCell.Value.x) - 1);
                        int dy = Mathf.Max(0, Mathf.Abs(cell.y - otherCell.Value.y) - 1);
                        if (dx * dx + dy * dy < 24 * 24) return float.NegativeInfinity;
                    }
                    return score(cell);
                }

                int seed = unchecked(biome.Seed * 397 ^ (int)biomeType * 7919 ^ salt);
                if (!MerchantPlacement.TrySelect(candidates, seed, out var cell, out _, scoreCandidate: ScoreWithClearance))
                {
                    Debug.LogWarning($"[RecoveryOrgan] {biomeType}의 {slot} 회복 기관을 배치할 수 없습니다.");
                    return null;
                }

                var position = biome.GridToWorldWithHeight(cell.x, cell.y, .02f);
                return SaveService.RememberWorldReward(new WorldRewardSaveData
                {
                    id = prefix + slot, biome = biomeType, recovery = true, recoveryOrgan = true,
                    x = position.x, y = position.y, z = position.z
                });
            }

            float ApproachScore(Vector2Int cell)
            {
                if (!bossArea.HasValue) return 1f / (1f + Mathf.Abs(Vector2Int.Distance(cell, entrance) - 110f));
                float distance = MerchantPlacement.DistanceToArea(cell, bossArea.Value);
                Vector2 offset = (Vector2)cell - bossArea.Value.center;
                float facing = Vector2.Dot(offset.normalized, -route);
                if (distance >= 10f && distance <= 25f && facing >= .2f) return 2f;
                // Prefer the approach from the entrance if ideal terrain is unavailable.
                return 1f / (1f + Mathf.Abs(distance - 17f) * .08f + Mathf.Max(0f, .2f - facing));
            }

            float FieldScore(Vector2Int cell)
            {
                Vector2 offset = cell - entrance;
                float forward = Vector2.Dot(offset, route);
                float sideways = Mathf.Abs(route.x * offset.y - route.y * offset.x);
                float targetDistance = biomeType == BiomeType.Intestine ? 55f : 75f;
                float bossDistance = bossArea.HasValue ? MerchantPlacement.DistanceToArea(cell, bossArea.Value) : 50f;
                if (Mathf.Abs(forward - targetDistance) > 22f || sideways < 14f || sideways > 44f || bossDistance < 32f)
                    return 1f / (1f + Mathf.Abs(forward - targetDistance) * .05f + Mathf.Abs(sideways - 28f) * .05f
                        + Mathf.Max(0f, 32f - bossDistance) * .06f);
                if (biomeType == BiomeType.Lung && terrain.Data.GetCell(cell.x, cell.y).HasRoad) return 1f;
                return NearTerrainFeature(terrain, biomeType, cell) ? 2f : 1.6f;
            }

            approach ??= Place("approach", field, ApproachScore, 0x4845414C);
            field ??= Place("field", approach, FieldScore, 0x4649454C);
            WorldRecoveryPickup.Spawn(approach);
            WorldRecoveryPickup.Spawn(field);
        }

        private static bool NearTerrainFeature(MapGenerator terrain, BiomeType biome, Vector2Int cell)
        {
            if (biome != BiomeType.Lung && biome != BiomeType.Stomach) return true;
            for (int dz = -6; dz <= 6; dz += 2)
            for (int dx = -6; dx <= 6; dx += 2)
            {
                int x = cell.x + dx, y = cell.y + dz;
                if (x < 0 || x >= terrain.MapWidth || y < 0 || y >= terrain.MapHeight) continue;
                var sample = terrain.Data.GetCell(x, y);
                if (biome == BiomeType.Lung ? sample.HasRoad : sample.HasLava) return true;
            }
            return false;
        }
    }
}
