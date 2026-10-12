using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public static class BiomeElitePlacementService
    {
        private static readonly Vector2Int[] Steps = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        public static BiomeElitePlan Build(BiomeEliteSpawnConfig config, BiomeType biome, int seed, int width, int height,
            Vector2Int start, Func<Vector2Int, bool> walkable, Func<Vector2Int, Vector2Int, bool> canMove,
            Func<Vector2Int, bool> excluded)
        {
            string error = config.GetValidationError();
            if (error != null) throw new InvalidOperationException(error);
            var plan = new BiomeElitePlan { biome = biome, mapSeed = seed };
            if (!config.enabled || config.monsters.Count == 0 || config.maximumCount == 0) return plan;
            var random = new System.Random(unchecked(seed * 397 ^ (int)biome * 7919));
            int count = random.Next(config.minimumCount, checked(config.maximumCount + 1));
            count = Math.Max(count, config.minimumPerType * config.monsters.Count);
            plan.requestedCount = count;
            if (count == 0) return plan;
            bool Inside(Vector2Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;
            if (!Inside(start) || !walkable(start)) return plan;
            var visited = new HashSet<Vector2Int> { start };
            var queue = new Queue<Vector2Int>();
            var candidates = new List<Vector2Int>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue();
                bool safe = !excluded(cell) && (cell - start).sqrMagnitude >= config.entranceExclusionRadius * config.entranceExclusionRadius;
                for (int y = -config.clearanceCells; safe && y <= config.clearanceCells; y++)
                    for (int x = -config.clearanceCells; safe && x <= config.clearanceCells; x++)
                    {
                        Vector2Int neighbor = cell + new Vector2Int(x, y);
                        safe = Inside(neighbor) && walkable(neighbor) && canMove(cell, neighbor) && !excluded(neighbor);
                    }
                if (safe) candidates.Add(cell);
                foreach (Vector2Int step in Steps)
                {
                    Vector2Int next = cell + step;
                    if (!Inside(next) || visited.Contains(next) || !walkable(next) || !canMove(cell, next)) continue;
                    visited.Add(next); queue.Enqueue(next);
                }
            }
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int other = random.Next(i + 1);
                (candidates[i], candidates[other]) = (candidates[other], candidates[i]);
            }
            var rules = new List<EnemySpawnRuleConfig>(config.monsters);
            rules.Sort((a, b) => string.CompareOrdinal(a.monsterDefinition.monsterId, b.monsterDefinition.monsterId));
            foreach (Vector2Int cell in candidates)
            {
                bool separated = true;
                foreach (BiomeElitePlacement previous in plan.placements)
                    if ((cell - new Vector2Int(previous.x, previous.y)).sqrMagnitude < config.minimumSpacing * config.minimumSpacing)
                    { separated = false; break; }
                if (!separated) continue;
                int index = plan.placements.Count < config.minimumPerType * rules.Count ? plan.placements.Count % rules.Count : random.Next(rules.Count);
                string id = rules[index].monsterDefinition.monsterId;
                plan.placements.Add(new BiomeElitePlacement
                {
                    spawnId = $"biome-elite:{biome}:{seed}:{cell.x}:{cell.y}:{id}", monsterId = id, x = cell.x, y = cell.y
                });
                if (plan.placements.Count == count) break;
            }
            return plan;
        }
    }
}
