using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public abstract partial class BiomeManager
    {
        public bool IsValidChunk(int chunkX, int chunkY)
        {
            return chunkX >= 0 && chunkX < chunksX && chunkY >= 0 && chunkY < chunksY;
        }

        public Vector3 GridToWorld(int gridX, int gridY)
        {
            float worldX = gridX * tileSize + tileSize / 2f;
            float worldZ = gridY * tileSize + tileSize / 2f;
            return new Vector3(worldX, 0f, worldZ);
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int gridX = Mathf.FloorToInt(worldPos.x / tileSize);
            int gridY = Mathf.FloorToInt(worldPos.z / tileSize);
            return new Vector2Int(gridX, gridY);
        }

        public Vector2Int GridToChunk(int gridX, int gridY)
        {
            int localX = gridX - MinGridX;
            int localY = gridY - MinGridY;
            return new Vector2Int(
                Mathf.FloorToInt(localX / (float)chunkSize),
                Mathf.FloorToInt(localY / (float)chunkSize));
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= MinGridX && x < MaxGridXExclusive
                && y >= MinGridY && y < MaxGridYExclusive;
        }

        public int GetHeightLevel(int worldX, int worldY)
        {
            if (!enableHeight) return 0;
            int level = GetBaseHeightLevel(worldX, worldY);
            return Mathf.Clamp(level, minHeightLevel, maxHeightLevel);
        }

        public float GetGroundHeight(int gridX, int gridY)
        {
            if (!IsValidPosition(gridX, gridY))
            {
                if (mapWidth <= 0 || mapHeight <= 0)
                {
                    return 0f;
                }

                gridX = Mathf.Clamp(gridX, MinGridX, MaxGridXExclusive - 1);
                gridY = Mathf.Clamp(gridY, MinGridY, MaxGridYExclusive - 1);
            }

            return GetHeightLevel(gridX, gridY) * heightStep;
        }

        public float GetGroundHeight(Vector3 worldPos)
        {
            Vector2Int grid = WorldToGrid(worldPos);
            return GetGroundHeight(grid.x, grid.y);
        }

        public Vector3 GridToWorldWithHeight(int gridX, int gridY, float yOffset = 0f)
        {
            Vector3 pos = GridToWorld(gridX, gridY);
            pos.y = GetGroundHeight(gridX, gridY) + yOffset;
            return pos;
        }

        public bool CanMove(Vector3 currentWorldPos, Vector3 desiredWorldPos)
        {
            Vector2Int currentGrid = WorldToGrid(currentWorldPos);
            Vector2Int targetCenterGrid = WorldToGrid(desiredWorldPos);
            Vector3 moveDelta = desiredWorldPos - currentWorldPos;
            int currentLevel = GetHeightLevel(currentGrid.x, currentGrid.y);
            int targetCenterLevel = GetHeightLevel(targetCenterGrid.x, targetCenterGrid.y);
            int centerDiff = targetCenterLevel - currentLevel;

            if (centerDiff > 0)
            {
                return false;
            }

            if (centerDiff < 0
                && (Mathf.Abs(centerDiff) > maxDropHeight
                    || !IsValidDropDestination(targetCenterGrid.x, targetCenterGrid.y, currentLevel)))
            {
                return false;
            }

            int referenceLevel = Mathf.Max(currentLevel, targetCenterLevel);
            float radius = Mathf.Max(0f, movementCollisionRadius);
            int minX = Mathf.FloorToInt((desiredWorldPos.x - radius) / tileSize);
            int maxX = Mathf.FloorToInt((desiredWorldPos.x + radius) / tileSize);
            int minY = Mathf.FloorToInt((desiredWorldPos.z - radius) / tileSize);
            int maxY = Mathf.FloorToInt((desiredWorldPos.z + radius) / tileSize);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (!IsCellEnterableFromLevel(x, y, referenceLevel, currentGrid, currentWorldPos, moveDelta))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsDropTarget(int x, int y, int currentLevel)
        {
            return IsWalkable(x, y)
                || IsDropTransitionCell(x, y, currentLevel)
                || HasWalkableNeighborAtLowerLevel(x, y, currentLevel);
        }

        protected virtual bool IsValidDropDestination(int x, int y, int currentLevel)
        {
            return IsDropTarget(x, y, currentLevel);
        }

        private bool IsCellEnterableFromLevel(int x, int y, int currentLevel, Vector2Int currentGrid, Vector3 currentWorldPos, Vector3 moveDelta)
        {
            if (!IsValidPosition(x, y)) return false;
            if (!IsWalkable(x, y))
            {
                if (x == currentGrid.x && y == currentGrid.y)
                {
                    return true;
                }

                if (!IsDropTransitionCell(x, y, currentLevel))
                {
                    return false;
                }
            }

            int targetLevel = GetHeightLevel(x, y);
            int diff = targetLevel - currentLevel;
            if (diff > 0)
            {
                return !IsMovingTowardCell(x, y, currentWorldPos, moveDelta);
            }

            return Mathf.Abs(diff) <= maxDropHeight;
        }

        private bool HasWalkableNeighborAtLowerLevel(int x, int y, int currentLevel)
        {
            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            foreach (Vector2Int direction in directions)
            {
                int nx = x + direction.x;
                int ny = y + direction.y;
                if (!IsValidPosition(nx, ny) || !IsWalkable(nx, ny))
                {
                    continue;
                }

                int neighborLevel = GetHeightLevel(nx, ny);
                if (neighborLevel < currentLevel && Mathf.Abs(neighborLevel - currentLevel) <= maxDropHeight)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsMovingTowardCell(int x, int y, Vector3 currentWorldPos, Vector3 moveDelta)
        {
            Vector3 cellCenter = GridToWorld(x, y);
            Vector2 toCell = new Vector2(cellCenter.x - currentWorldPos.x, cellCenter.z - currentWorldPos.z);
            Vector2 move = new Vector2(moveDelta.x, moveDelta.z);
            if (move.sqrMagnitude <= 0.000001f)
            {
                return false;
            }

            return Vector2.Dot(toCell, move.normalized) > 0f;
        }

        protected virtual bool IsDropTransitionCell(int x, int y, int referenceLevel)
        {
            return false;
        }

        protected virtual bool IsClimbTransitionCell(int x, int y)
        {
            return false;
        }

        public bool CanClimb(Vector3 currentWorldPos, Vector3 desiredWorldPos)
        {
            Vector2Int currentGrid = WorldToGrid(currentWorldPos);
            Vector2Int targetGrid = WorldToGrid(desiredWorldPos);
            return CanClimb(currentGrid, targetGrid);
        }

        public bool CanClimb(Vector2Int currentGrid, Vector2Int targetGrid)
        {
            if (!IsValidPosition(currentGrid.x, currentGrid.y)) return false;
            if (!IsValidPosition(targetGrid.x, targetGrid.y)) return false;
            if (!IsWalkable(targetGrid.x, targetGrid.y)) return false;

            int currentLevel = GetHeightLevel(currentGrid.x, currentGrid.y);
            int targetLevel = GetHeightLevel(targetGrid.x, targetGrid.y);
            int diff = targetLevel - currentLevel;
            return diff > 0 && diff <= maxClimbHeight;
        }

        public bool TryGetClimbDestination(Vector2Int currentGrid, Vector2Int facingDirection, out Vector2Int destination)
        {
            destination = currentGrid + facingDirection;
            if (CanClimb(currentGrid, destination))
            {
                return true;
            }

            Vector2Int transitionGrid = destination;
            if (!IsClimbTransitionCell(transitionGrid.x, transitionGrid.y))
            {
                return TryFindNearbyClimbDestination(currentGrid, transitionGrid, out destination);
            }

            Vector2Int beyondGrid = transitionGrid + facingDirection;
            if (!CanClimb(currentGrid, beyondGrid))
            {
                return TryFindNearbyClimbDestination(currentGrid, transitionGrid, out destination);
            }

            destination = beyondGrid;
            return true;
        }

        private bool TryFindNearbyClimbDestination(Vector2Int currentGrid, Vector2Int searchCenter, out Vector2Int destination)
        {
            for (int y = searchCenter.y - 2; y <= searchCenter.y + 2; y++)
            {
                for (int x = searchCenter.x - 2; x <= searchCenter.x + 2; x++)
                {
                    Vector2Int candidate = new Vector2Int(x, y);
                    if (candidate == currentGrid)
                    {
                        continue;
                    }

                    if (CanClimb(currentGrid, candidate))
                    {
                        destination = candidate;
                        return true;
                    }
                }
            }

            destination = default;
            return false;
        }

        public bool IsWalkable(int x, int y)
        {
            if (!IsValidPosition(x, y)) return false;
            if (blockedCells.Contains(new Vector2Int(x, y))) return false;

            Vector2Int chunkCoord = GridToChunk(x, y);
            if (IsValidChunk(chunkCoord.x, chunkCoord.y) && chunks != null)
            {
                TileSample sample = SampleTile(x, y, chunks[chunkCoord.x, chunkCoord.y]);
                return sample.walkable;
            }

            return SampleBaseTile(x, y).walkable;
        }

        public virtual bool CanSpawnEnemyAt(int x, int y)
        {
            return IsValidPosition(x, y) && IsWalkable(x, y);
        }

        public void AddRuntimeBlockedCells(IEnumerable<Vector2Int> occupiedCells)
        {
            AddBlockedCells(occupiedCells);
        }

        public void RemoveRuntimeBlockedCells(IEnumerable<Vector2Int> occupiedCells)
        {
            RemoveBlockedCells(occupiedCells);
        }

        public virtual Vector3 GetPlayerSpawnPosition()
        {
            return GridToWorld(mapWidth / 2, 5);
        }

        protected virtual void OnChunkLoaded(Chunk chunk)
        {
        }

        protected virtual void OnChunkUnloaded(Chunk chunk)
        {
        }

        protected virtual void OnDestroy()
        {
            if (Active == this)
            {
                Active = null;
            }
        }

        private int GetHeightLevelCount()
        {
            return Mathf.Max(1, maxHeightLevel - minHeightLevel + 1);
        }

        private int GetHeightLevelIndex(int heightLevel)
        {
            int clamped = Mathf.Clamp(heightLevel, minHeightLevel, maxHeightLevel);
            return clamped - minHeightLevel;
        }

        private void Log(string message)
        {
            if (!enableDebugLogs) return;
            Debug.Log(message);
        }
    }
}
