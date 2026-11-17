using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public static class MerchantPlacement
    {
        public const float EntranceDistance = 30f;
        public const float BossDistance = 20f;
        public const float TestEntranceRadius = 12f;
        private static readonly Vector2Int[] Steps = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        public static float DistanceToArea(Vector2Int cell, RectInt area)
        {
            float dx = Mathf.Max(0, Mathf.Max(area.xMin - cell.x, cell.x - (area.xMax - 1)));
            float dy = Mathf.Max(0, Mathf.Max(area.yMin - cell.y, cell.y - (area.yMax - 1)));
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        // Search the entrance's connected floor first; random selection uses only a local seed.
        public static bool TryChoose(int width, int height, int seed, Vector2Int entrance, RectInt? bossArea,
            Func<Vector2Int, bool> walkable, Func<Vector2Int, Vector2Int, bool> canMove,
            Func<Vector2Int, bool> excluded, out Vector2Int placement, out bool preferredDistances, bool nearEntrance = false)
        {
            placement = default; preferredDistances = false;
            bool Inside(Vector2Int p) => p.x >= 0 && p.y >= 0 && p.x < width && p.y < height;
            if (!Inside(entrance) || !walkable(entrance)) return false;
            var seen = new bool[width * height];
            var queue = new Queue<Vector2Int>();
            var candidates = new List<(Vector2Int cell, float score)>();
            seen[entrance.y * width + entrance.x] = true; queue.Enqueue(entrance);
            float best = 0;
            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue();
                bool clear = excluded == null || !excluded(cell);
                // A 3x3 level, unobstructed footprint leaves space to approach the merchant.
                for (int y = -1; clear && y <= 1; y++)
                    for (int x = -1; clear && x <= 1; x++)
                    {
                        Vector2Int neighbor = cell + new Vector2Int(x, y);
                        clear = Inside(neighbor) && walkable(neighbor) && canMove(cell, neighbor)
                            && (excluded == null || !excluded(neighbor));
                    }
                float fromEntrance = Vector2Int.Distance(cell, entrance);
                float fromBoss = bossArea.HasValue ? DistanceToArea(cell, bossArea.Value) : float.PositiveInfinity;
                if (clear && fromEntrance >= (nearEntrance ? 3f : 5f) && fromBoss >= 3f
                    && (!nearEntrance || fromEntrance <= TestEntranceRadius))
                {
                    float score = nearEntrance ? 1f / (1f + Mathf.Abs(fromEntrance - 5f))
                        : Mathf.Min(fromEntrance / EntranceDistance, fromBoss / BossDistance);
                    candidates.Add((cell, score)); best = Mathf.Max(best, score);
                }
                foreach (Vector2Int step in Steps)
                {
                    Vector2Int next = cell + step;
                    if (!Inside(next) || (nearEntrance && Vector2Int.Distance(next, entrance) > TestEntranceRadius)) continue;
                    int index = next.y * width + next.x;
                    if (seen[index] || !walkable(next) || !canMove(cell, next)) continue;
                    seen[index] = true; queue.Enqueue(next);
                }
            }
            if (candidates.Count == 0) return false;
            preferredDistances = nearEntrance || best >= 1;
            float threshold = nearEntrance ? best * .9f : preferredDistances ? Mathf.Max(1, best * .75f) : best * .95f;
            var pool = candidates.FindAll(value => value.score >= threshold);
            placement = pool[new System.Random(seed).Next(pool.Count)].cell;
            return true;
        }
    }
}
