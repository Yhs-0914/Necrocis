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
            return new Vector2Int(gridX / chunkSize, gridY / chunkSize);
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
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

                gridX = Mathf.Clamp(gridX, 0, mapWidth - 1);
                gridY = Mathf.Clamp(gridY, 0, mapHeight - 1);
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
            Vector2Int targetGrid = WorldToGrid(desiredWorldPos);

            if (!IsValidPosition(targetGrid.x, targetGrid.y)) return false;
            if (!IsWalkable(targetGrid.x, targetGrid.y)) return false;

            int currentLevel = GetHeightLevel(currentGrid.x, currentGrid.y);
            int targetLevel = GetHeightLevel(targetGrid.x, targetGrid.y);
            int diff = Mathf.Abs(targetLevel - currentLevel);
            return diff <= maxStepHeight;
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
