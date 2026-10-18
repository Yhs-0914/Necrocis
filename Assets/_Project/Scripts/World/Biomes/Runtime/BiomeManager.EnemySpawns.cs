using UnityEngine;

namespace Necrocis
{
    public abstract partial class BiomeManager
    {
        // OnChunkLoaded precedes these flags. Acquire the current load's parent only once ready.
        public bool TryGetEnemySpawnParent(Vector2Int coordinate, out Transform parent)
        {
            parent = null;
            if (chunks == null || !IsValidChunk(coordinate.x, coordinate.y)) return false;
            Chunk chunk = chunks[coordinate.x, coordinate.y];
            if (chunk == null || !chunk.isLoaded || !chunk.isObjectsLoaded) return false;
            EnsureChunkObjectsRoot(chunk);
            parent = chunk.objectsRoot;
            return parent != null && parent.gameObject.activeInHierarchy;
        }
    }
}
