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

        internal readonly struct Candidate
        {
            public readonly Vector2Int Cell;
            public readonly float FromEntrance;
            public readonly float FromBoss;

            public Candidate(Vector2Int cell, float fromEntrance, float fromBoss)
            {
                Cell = cell;
                FromEntrance = fromEntrance;
                FromBoss = fromBoss;
            }
        }

        private enum CellCheck : byte { Unknown, Blocked, Clear }

        public static float DistanceToArea(Vector2Int cell, RectInt area)
        {
            float dx = Mathf.Max(0, Mathf.Max(area.xMin - cell.x, cell.x - (area.xMax - 1)));
            float dy = Mathf.Max(0, Mathf.Max(area.yMin - cell.y, cell.y - (area.yMax - 1)));
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        // Search the entrance's connected floor first; random selection uses only a local seed.
        public static bool TryChoose(int width, int height, int seed, Vector2Int entrance, RectInt? bossArea,
            Func<Vector2Int, bool> walkable, Func<Vector2Int, Vector2Int, bool> canMove,
            Func<Vector2Int, bool> excluded, out Vector2Int placement, out bool preferredDistances, bool nearEntrance = false,
            Func<Vector2Int, float> scoreCandidate = null)
        {
            var candidates = FindCandidates(width, height, entrance, bossArea, walkable, canMove, excluded, nearEntrance);
            return TrySelect(candidates, seed, out placement, out preferredDistances, nearEntrance, scoreCandidate);
        }

        internal static List<Candidate> FindCandidates(int width, int height, Vector2Int entrance, RectInt? bossArea,
            Func<Vector2Int, bool> walkable, Func<Vector2Int, Vector2Int, bool> canMove, Func<Vector2Int, bool> excluded,
            bool nearEntrance = false)
        {
            var candidates = new List<Candidate>();
            bool Inside(Vector2Int cell) => cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height;
            if (!Inside(entrance) || !walkable(entrance)) return candidates;
            var walkableCells = new CellCheck[width * height];
            var clearCells = new CellCheck[width * height];
            var seen = new bool[width * height];
            var queue = new Queue<Vector2Int>();
            int CellIndex(Vector2Int cell) => cell.y * width + cell.x;
            bool IsWalkable(Vector2Int cell, int index)
            {
                if (walkableCells[index] == CellCheck.Unknown)
                    walkableCells[index] = walkable(cell) ? CellCheck.Clear : CellCheck.Blocked;
                return walkableCells[index] == CellCheck.Clear;
            }
            bool IsClear(Vector2Int cell)
            {
                int index = CellIndex(cell);
                if (clearCells[index] == CellCheck.Unknown)
                    clearCells[index] = IsWalkable(cell, index) && (excluded == null || !excluded(cell))
                        ? CellCheck.Clear : CellCheck.Blocked;
                return clearCells[index] == CellCheck.Clear;
            }

            int entranceIndex = CellIndex(entrance);
            walkableCells[entranceIndex] = CellCheck.Clear;
            seen[entranceIndex] = true;
            queue.Enqueue(entrance);
            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue();
                bool clear = IsClear(cell);
                // A clear 3x3 footprint leaves room to approach the pickup or merchant.
                for (int y = -1; clear && y <= 1; y++)
                    for (int x = -1; clear && x <= 1; x++)
                    {
                        Vector2Int neighbor = cell + new Vector2Int(x, y);
                        clear = Inside(neighbor) && IsClear(neighbor) && canMove(cell, neighbor);
                    }
                if (clear)
                {
                    float fromEntrance = Vector2Int.Distance(cell, entrance);
                    float fromBoss = bossArea.HasValue ? DistanceToArea(cell, bossArea.Value) : float.PositiveInfinity;
                    if (fromEntrance >= (nearEntrance ? 3f : 5f) && fromBoss >= 3f)
                        candidates.Add(new Candidate(cell, fromEntrance, fromBoss));
                }
                foreach (Vector2Int step in Steps)
                {
                    Vector2Int next = cell + step;
                    if (!Inside(next) || (nearEntrance && (next - entrance).sqrMagnitude > TestEntranceRadius * TestEntranceRadius)) continue;
                    int index = CellIndex(next);
                    if (seen[index] || !IsWalkable(next, index) || !canMove(cell, next)) continue;
                    seen[index] = true;
                    queue.Enqueue(next);
                }
            }
            return candidates;
        }

        internal static bool TrySelect(List<Candidate> candidates, int seed, out Vector2Int placement,
            out bool preferredDistances, bool nearEntrance = false, Func<Vector2Int, float> scoreCandidate = null)
        {
            placement = default;
            preferredDistances = false;
            if (candidates.Count == 0) return false;

            float best = 0;
            var scores = new float[candidates.Count];
            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                float score = scoreCandidate?.Invoke(candidate.Cell) ?? (nearEntrance
                    ? 1f / (1f + Mathf.Abs(candidate.FromEntrance - 5f))
                    : Mathf.Min(candidate.FromEntrance / EntranceDistance, candidate.FromBoss / BossDistance));
                scores[i] = score;
                best = Mathf.Max(best, score);
            }
            preferredDistances = nearEntrance || best >= 1;
            float threshold = nearEntrance ? best * .9f : preferredDistances ? Mathf.Max(1, best * .75f) : best * .95f;
            int choices = 0;
            for (int i = 0; i < scores.Length; i++)
                if (scores[i] >= threshold) choices++;
            if (choices == 0) return false;

            int selected = new System.Random(seed).Next(choices);
            for (int i = 0; i < scores.Length; i++)
            {
                if (!(scores[i] >= threshold) || selected-- != 0) continue;
                placement = candidates[i].Cell;
                return true;
            }
            return false;
        }
    }
}
