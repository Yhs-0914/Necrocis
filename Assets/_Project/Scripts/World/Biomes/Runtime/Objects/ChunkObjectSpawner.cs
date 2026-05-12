using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Necrocis
{
    public abstract partial class BiomeManager
    {

        private void LoadChunkObjects(Chunk chunk)
        {
            if (chunk.isObjectsLoaded || chunk.objectGenerationRoutine != null) return;

            chunk.objectGenerationRoutine = StartCoroutine(GenerateObjectsRoutine(chunk));
            Log($"[BiomeManager] 오브젝트 로드 예약: ({chunk.chunkX}, {chunk.chunkY})");
        }


        private System.Collections.IEnumerator GenerateObjectsRoutine(Chunk chunk)
        {
            System.Collections.IEnumerator inner = GenerateObjectsForChunkAsync(chunk);
            while (inner.MoveNext())
            {
                yield return inner.Current;
            }

            chunk.isObjectsLoaded = true;
            chunk.objectGenerationRoutine = null;
        }


        private void UnloadChunkObjects(Chunk chunk)
        {
            if (!HasResidualChunkObjects(chunk)) return;

            if (chunk.objectGenerationRoutine != null)
            {
                StopCoroutine(chunk.objectGenerationRoutine);
                chunk.objectGenerationRoutine = null;
            }

            DestroyChunkObjects(chunk);
            ReleaseChunkObjectsRoot(chunk);
            chunk.isObjectsLoaded = false;
            Log($"[BiomeManager] 오브젝트 언로드: ({chunk.chunkX}, {chunk.chunkY})");
            StartCoroutine(VerifyChunkObjectsClearedNextFrame(chunk.chunkX, chunk.chunkY));
        }

        protected virtual void DestroyChunkObjects(Chunk chunk)
        {
            HashSet<GameObject> releasedObjects = new HashSet<GameObject>();

            if (chunk.liveObjects.Count > 0)
            {
                GameObject[] liveSnapshot = chunk.liveObjects.ToArray();
                for (int i = 0; i < liveSnapshot.Length; i++)
                {
                    ReleaseChunkObject(liveSnapshot[i], releasedObjects);
                }
            }

            if (chunk.objectsRoot != null)
            {
                int childCount = chunk.objectsRoot.childCount;
                GameObject[] rootChildren = new GameObject[childCount];
                for (int i = 0; i < childCount; i++)
                {
                    rootChildren[i] = chunk.objectsRoot.GetChild(i).gameObject;
                }

                for (int i = 0; i < rootChildren.Length; i++)
                {
                    ReleaseChunkObject(rootChildren[i], releasedObjects);
                }
            }

            chunk.liveObjects.Clear();
        }


        private void ReleaseChunkObject(GameObject obj, HashSet<GameObject> releasedObjects)
        {
            if (obj == null || releasedObjects == null || !releasedObjects.Add(obj))
            {
                return;
            }

            if (pooledObjectsParent != null && obj.transform.parent == pooledObjectsParent && !obj.activeSelf)
            {
                return;
            }

            EnemySpawner spawner = obj.GetComponent<EnemySpawner>();
            if (spawner != null)
            {
                spawner.ReleaseSpawnedEnemies();
                spawner.enabled = false;
            }

            EnemyController enemy = obj.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.ReleaseToPool();
                return;
            }

            BiomeObjectState state = obj.GetComponent<BiomeObjectState>();
            if (state == null)
            {
                DestroyPooledObject(obj);
                return;
            }

            state.SuppressDestroy();
            if (state.BlocksMovement)
            {
                RemoveBlockedCells(state.OccupiedCells);
            }

            ReleasePooledObject(state.PoolKey, obj);
        }


        protected void RegisterObject(Chunk chunk, GameObject obj, ObjectId id, ObjectPoolKey poolKey, bool blocksMovement)
        {
            EnsureChunkObjectsRoot(chunk);
            if (chunk.objectsRoot != null && obj.transform.parent != chunk.objectsRoot)
            {
                obj.transform.SetParent(chunk.objectsRoot, true);
            }

            if (!chunk.liveObjects.Contains(obj))
            {
                chunk.liveObjects.Add(obj);
            }

            BiomeObjectState state = obj.GetComponent<BiomeObjectState>();
            if (state == null)
            {
                state = obj.AddComponent<BiomeObjectState>();
            }

            if (state.BlocksMovement)
            {
                RemoveBlockedCells(state.OccupiedCells);
            }

            state.Initialize(this, id, poolKey, blocksMovement);

            if (blocksMovement)
            {
                List<Vector2Int> occupiedCells = GetOccupiedCellsForObject(obj, id);
                state.SetOccupiedCells(occupiedCells);
                AddBlockedCells(occupiedCells);
            }
            else
            {
                state.SetOccupiedCells(null);
            }
        }


        internal void NotifyObjectRemoved(ObjectId id, bool blocksMovement, System.Collections.Generic.IReadOnlyList<Vector2Int> occupiedCells)
        {
            if (!blocksMovement) return;
            if (occupiedCells != null && occupiedCells.Count > 0)
            {
                RemoveBlockedCells(occupiedCells);
                return;
            }

            blockedCells.Remove(new Vector2Int(id.x, id.y));
        }


        private void EnsureChunkObjectsRoot(Chunk chunk)
        {
            if (chunk.objectsRoot != null || objectsParent == null) return;

            GameObject root = new GameObject($"ChunkRuntime_{chunk.chunkX}_{chunk.chunkY}");
            root.transform.SetParent(objectsParent, false);
            chunk.objectsRoot = root.transform;
        }


        private void ReleaseChunkObjectsRoot(Chunk chunk)
        {
            if (chunk.objectsRoot == null) return;

            chunk.objectsRoot.gameObject.SetActive(false);
            DestroyChunkRootObject(chunk.objectsRoot.gameObject);
            chunk.objectsRoot = null;
        }


        private List<Vector2Int> GetOccupiedCellsForObject(GameObject obj, ObjectId id)
        {
            List<Vector2Int> occupiedCells = new List<Vector2Int>();
            if (obj == null)
            {
                occupiedCells.Add(new Vector2Int(id.x, id.y));
                return occupiedCells;
            }

            BoxCollider collider = obj.GetComponent<BoxCollider>();
            if (collider == null || !collider.enabled || collider.isTrigger)
            {
                occupiedCells.Add(new Vector2Int(id.x, id.y));
                return occupiedCells;
            }

            Vector3 lossyScale = obj.transform.lossyScale;
            lossyScale = new Vector3(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
            Vector3 halfSize = Vector3.Scale(collider.size, lossyScale) * 0.5f;
            Vector3 center = obj.transform.TransformPoint(collider.center);

            float epsilon = tileSize * 0.001f;
            int minX = Mathf.FloorToInt((center.x - halfSize.x + epsilon) / tileSize);
            int maxX = Mathf.FloorToInt((center.x + halfSize.x - epsilon) / tileSize);
            int minY = Mathf.FloorToInt((center.z - halfSize.z + epsilon) / tileSize);
            int maxY = Mathf.FloorToInt((center.z + halfSize.z - epsilon) / tileSize);

            HashSet<Vector2Int> uniqueCells = new HashSet<Vector2Int>();
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (IsValidPosition(x, y))
                    {
                        uniqueCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (uniqueCells.Count == 0)
            {
                uniqueCells.Add(new Vector2Int(id.x, id.y));
            }

            occupiedCells.AddRange(uniqueCells);
            return occupiedCells;
        }


        private void AddBlockedCells(System.Collections.Generic.IEnumerable<Vector2Int> occupiedCells)
        {
            if (occupiedCells == null)
            {
                return;
            }

            foreach (Vector2Int cell in occupiedCells)
            {
                blockedCells.Add(cell);
            }
        }


        private void RemoveBlockedCells(System.Collections.Generic.IEnumerable<Vector2Int> occupiedCells)
        {
            if (occupiedCells == null)
            {
                return;
            }

            foreach (Vector2Int cell in occupiedCells)
            {
                blockedCells.Remove(cell);
            }
        }

    }
}
