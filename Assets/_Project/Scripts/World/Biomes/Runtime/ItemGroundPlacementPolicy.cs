using UnityEngine;
namespace Necrocis
{
    public static class ItemGroundPlacementPolicy
    {
        public static Vector3 Resolve(Vector3 desiredPosition, Vector3 ownerPosition, float heightOffset)
        {
            BiomeManager biome = BiomeManager.Active;
            if (biome == null)
            {
                Vector3 fallback = desiredPosition;
                fallback.y = ownerPosition.y + heightOffset;
                return fallback;
            }

            Vector2Int desiredGrid = biome.WorldToGrid(desiredPosition);
            if (biome.IsWalkable(desiredGrid.x, desiredGrid.y))
            {
                return biome.GridToWorldWithHeight(desiredGrid.x, desiredGrid.y, heightOffset);
            }

            Vector2Int playerGrid = biome.WorldToGrid(ownerPosition);
            for (int radius = 1; radius <= 3; radius++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius)
                        {
                            continue;
                        }

                        int gridX = playerGrid.x + x;
                        int gridY = playerGrid.y + y;
                        if (biome.IsWalkable(gridX, gridY))
                        {
                            return biome.GridToWorldWithHeight(gridX, gridY, heightOffset);
                        }
                    }
                }
            }

            return biome.GridToWorldWithHeight(playerGrid.x, playerGrid.y, heightOffset);
        }
    }
}
