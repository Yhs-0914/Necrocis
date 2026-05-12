using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Necrocis
{
    public abstract partial class BiomeManager
    {

        private bool HasChunkObjectsInHierarchy(Chunk chunk)
        {
            if (objectsParent == null) return false;

            BiomeObjectState[] states = objectsParent.GetComponentsInChildren<BiomeObjectState>(true);
            for (int i = 0; i < states.Length; i++)
            {
                if (IsHierarchyStateForChunk(chunk, states[i]))
                {
                    return true;
                }
            }

            return false;
        }


        private bool IsHierarchyStateForChunk(Chunk chunk, BiomeObjectState state)
        {
            if (chunk == null || state == null) return false;
            if (pooledObjectsParent != null && state.transform.IsChildOf(pooledObjectsParent)) return false;

            Vector2Int chunkCoord = GridToChunk(state.ObjectId.x, state.ObjectId.y);
            return chunkCoord.x == chunk.chunkX && chunkCoord.y == chunk.chunkY;
        }


        private bool HasResidualChunkObjects(Chunk chunk)
        {
            if (chunk == null) return false;

            return chunk.isObjectsLoaded ||
                   chunk.objectGenerationRoutine != null ||
                   chunk.liveObjects.Count > 0 ||
                   chunk.objectsRoot != null ||
                   HasChunkObjectsInHierarchy(chunk);
        }


        private void SweepOrphanedChunkObjects()
        {
            if (chunks == null || reportedResidualChunks.Count == 0) return;

            Vector2Int[] residualChunks = new Vector2Int[reportedResidualChunks.Count];
            reportedResidualChunks.CopyTo(residualChunks);

            for (int i = 0; i < residualChunks.Length; i++)
            {
                Vector2Int chunkPos = residualChunks[i];
                if (!IsValidChunk(chunkPos.x, chunkPos.y))
                {
                    reportedResidualChunks.Remove(chunkPos);
                    continue;
                }

                Chunk chunk = chunks[chunkPos.x, chunkPos.y];
                if (chunk == null || chunk.isLoaded)
                {
                    continue;
                }

                if (HasResidualChunkObjects(chunk))
                {
                    ReportResidualChunkObjects("Orphaned chunk cleanup retry", chunk);
                    UnloadChunkObjects(chunk);
                }
                else
                {
                    reportedResidualChunks.Remove(chunkPos);
                }
            }
        }


        private System.Collections.IEnumerator VerifyChunkObjectsClearedNextFrame(int chunkX, int chunkY)
        {
            yield return null;

            if (!IsValidChunk(chunkX, chunkY) || chunks == null)
            {
                yield break;
            }

            Chunk chunk = chunks[chunkX, chunkY];
            if (HasResidualChunkObjects(chunk))
            {
                ReportResidualChunkObjects("Residual chunk objects after unload", chunk);
                ForceDestroyResidualChunkObjects(chunk);
            }
            else
            {
                reportedResidualChunks.Remove(new Vector2Int(chunkX, chunkY));
            }
        }


        private void ReportResidualChunkObjects(string context, Chunk chunk)
        {
            if (chunk == null)
            {
                return;
            }

            Vector2Int chunkPos = new Vector2Int(chunk.chunkX, chunk.chunkY);
            if (!reportedResidualChunks.Add(chunkPos) && !enableDebugLogs)
            {
                return;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[BiomeManager] ");
            sb.Append(context);
            sb.Append(" chunk=(");
            sb.Append(chunk.chunkX);
            sb.Append(", ");
            sb.Append(chunk.chunkY);
            sb.Append(")");
            sb.Append(" isLoaded=");
            sb.Append(chunk.isLoaded);
            sb.Append(" isObjectsLoaded=");
            sb.Append(chunk.isObjectsLoaded);
            sb.Append(" routine=");
            sb.Append(chunk.objectGenerationRoutine != null);
            sb.Append(" manifestCount=");
            sb.Append(chunk.spawnManifest.Count);
            sb.Append(" liveObjects=");
            sb.Append(chunk.liveObjects.Count);
            sb.Append(" objectsRoot=");
            sb.Append(chunk.objectsRoot != null ? chunk.objectsRoot.name : "null");

            if (objectsParent != null)
            {
                BiomeObjectState[] states = objectsParent.GetComponentsInChildren<BiomeObjectState>(true);
                int detailCount = 0;
                for (int i = 0; i < states.Length; i++)
                {
                    BiomeObjectState state = states[i];
                    if (!IsHierarchyStateForChunk(chunk, state))
                    {
                        continue;
                    }

                    if (detailCount == 0)
                    {
                        sb.Append(" residuals=");
                    }

                    if (detailCount < 16)
                    {
                        if (detailCount > 0)
                        {
                            sb.Append(" | ");
                        }

                        Transform tr = state.transform;
                        sb.Append(tr.name);
                        sb.Append(" activeSelf=");
                        sb.Append(state.gameObject.activeSelf);
                        sb.Append(" activeInHierarchy=");
                        sb.Append(state.gameObject.activeInHierarchy);
                        sb.Append(" parent=");
                        sb.Append(GetTransformPath(tr.parent));
                        sb.Append(" objectId=(");
                        sb.Append(state.ObjectId.x);
                        sb.Append(", ");
                        sb.Append(state.ObjectId.y);
                        sb.Append(", ");
                        sb.Append((int)state.ObjectId.type);
                        sb.Append(")");
                    }

                    detailCount++;
                }

                if (detailCount > 16)
                {
                    sb.Append(" | ... total=");
                    sb.Append(detailCount);
                }
            }

            Debug.LogWarning(sb.ToString(), this);
        }


        private void ForceDestroyResidualChunkObjects(Chunk chunk)
        {
            if (chunk == null)
            {
                return;
            }

            chunk.liveObjects.Clear();
            chunk.isObjectsLoaded = false;
            chunk.objectGenerationRoutine = null;

            if (chunk.objectsRoot != null)
            {
                GameObject runtimeRoot = chunk.objectsRoot.gameObject;
                chunk.objectsRoot = null;
                DestroyChunkRootObject(runtimeRoot);
            }

            if (objectsParent == null)
            {
                return;
            }

            BiomeObjectState[] states = objectsParent.GetComponentsInChildren<BiomeObjectState>(true);
            for (int i = 0; i < states.Length; i++)
            {
                BiomeObjectState state = states[i];
                if (!IsHierarchyStateForChunk(chunk, state))
                {
                    continue;
                }

                if (state.BlocksMovement)
                {
                    RemoveBlockedCells(state.OccupiedCells);
                }

                DestroyPooledObject(state.gameObject);
            }
        }


        private static string GetTransformPath(Transform target)
        {
            if (target == null)
            {
                return "<null>";
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder(target.name);
            Transform current = target.parent;
            while (current != null)
            {
                sb.Insert(0, '/');
                sb.Insert(0, current.name);
                current = current.parent;
            }
            return sb.ToString();
        }

    }
}
