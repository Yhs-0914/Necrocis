using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Necrocis
{
    public abstract partial class BiomeManager
    {

        private void EnsureChunkRoot(Chunk chunk)
        {
            if (chunk.root != null) return;

            int levelCount = GetHeightLevelCount();
            bool hasCliffTilemaps = useCliffOverlayTilemaps;
            GameObject chunkRoot = AcquireChunkRoot();
            if (chunkRoot == null)
            {
                chunkRoot = new GameObject($"Chunk_{chunk.chunkX}_{chunk.chunkY}");
                chunkRoot.transform.SetParent(tilesParent, false);
            }
            else
            {
                chunkRoot.name = $"Chunk_{chunk.chunkX}_{chunk.chunkY}";
            }

            Vector3 chunkOrigin = new Vector3(chunk.chunkX * chunkSize * tileSize, 0f, chunk.chunkY * chunkSize * tileSize);
            chunkRoot.transform.localPosition = chunkOrigin;

            ChunkRoot rootData = chunkRoot.GetComponent<ChunkRoot>();
            if (rootData == null)
            {
                rootData = chunkRoot.AddComponent<ChunkRoot>();
            }

            if (!rootData.Matches(levelCount, hasCliffTilemaps))
            {
                ClearChunkRootChildren(chunkRoot.transform);
                CreateChunkTilemaps(chunkRoot.transform, levelCount, hasCliffTilemaps, rootData);
            }

            chunk.root = chunkRoot;
            chunk.tilemaps = rootData.tilemaps;
            chunk.tilemapRenderers = rootData.tilemapRenderers;
            chunk.cliffTilemaps = rootData.cliffTilemaps;
            chunk.cliffTilemapRenderers = rootData.cliffTilemapRenderers;
        }


        private void CreateChunkTilemaps(Transform parent, int levelCount, bool hasCliffTilemaps, ChunkRoot rootData)
        {
            Tilemap[] tilemaps = new Tilemap[levelCount];
            TilemapRenderer[] tilemapRenderers = new TilemapRenderer[levelCount];
            Tilemap[] cliffTilemaps = hasCliffTilemaps ? new Tilemap[levelCount] : null;
            TilemapRenderer[] cliffTilemapRenderers = hasCliffTilemaps ? new TilemapRenderer[levelCount] : null;

            for (int i = 0; i < levelCount; i++)
            {
                int heightLevel = minHeightLevel + i;
                float heightOffset = heightLevel * heightStep;

                tilemaps[i] = CreateTilemapLayer(parent, $"Tiles_{heightLevel}", heightOffset, -1000 + i * 2, out tilemapRenderers[i]);
                if (hasCliffTilemaps)
                {
                    cliffTilemaps[i] = CreateTilemapLayer(parent, $"Cliff_{heightLevel}", heightOffset + cliffOverlayOffset, -999 + i * 2, out cliffTilemapRenderers[i]);
                    cliffTilemaps[i].color = cliffTint;
                }
            }

            rootData.Configure(levelCount, hasCliffTilemaps, tilemaps, tilemapRenderers, cliffTilemaps, cliffTilemapRenderers);
        }


        private Tilemap CreateTilemapLayer(Transform parent, string name, float yOffset, int sortingOrder, out TilemapRenderer renderer)
        {
            GameObject tileObj = new GameObject(name);
            tileObj.transform.SetParent(parent, false);
            tileObj.transform.localPosition = new Vector3(0f, yOffset, 0f);
            tileObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Tilemap tilemap = tileObj.AddComponent<Tilemap>();
            renderer = tileObj.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;
            return tilemap;
        }


        private void EnsureChunkBuffers(Chunk chunk, int tileCount)
        {
            if (chunk.baseTiles == null || chunk.baseTiles.Length != tileCount)
            {
                chunk.baseTiles = new TileBase[tileCount];
            }

            if (chunk.heightLevels == null || chunk.heightLevels.Length != tileCount)
            {
                chunk.heightLevels = new int[tileCount];
            }

            if (chunk.cliffLevels == null || chunk.cliffLevels.Length != tileCount)
            {
                chunk.cliffLevels = new int[tileCount];
            }

            if (chunk.tileBuffer == null || chunk.tileBuffer.Length != tileCount)
            {
                chunk.tileBuffer = new TileBase[tileCount];
            }

            if (useCliffOverlayTilemaps)
            {
                if (chunk.cliffBuffer == null || chunk.cliffBuffer.Length != tileCount)
                {
                    chunk.cliffBuffer = new TileBase[tileCount];
                }
                chunk.colorBuffer = null;
            }
            else
            {
                chunk.cliffBuffer = null;
                if (chunk.colorBuffer == null || chunk.colorBuffer.Length != tileCount)
                {
                    chunk.colorBuffer = new Color[tileCount];
                }
            }
        }


        private void ApplyTileColors(Tilemap tilemap, TileBase[] tiles, Color[] colors)
        {
            int index = 0;
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    if (tiles[index] != null)
                    {
                        Vector3Int cell = new Vector3Int(x, y, 0);
                        tilemap.SetTileFlags(cell, TileFlags.None);
                        tilemap.SetColor(cell, colors[index]);
                    }
                    index++;
                }
            }
        }


        protected TileSample CreateTileSample(BiomeTileType tileType)
        {
            return new TileSample(tileType, GetTileAsset(tileType), IsTileWalkable(tileType));
        }


        private void ClearChunkTilemaps(Chunk chunk)
        {
            if (chunk.tilemaps != null)
            {
                foreach (Tilemap tilemap in chunk.tilemaps)
                {
                    if (tilemap != null)
                    {
                        tilemap.ClearAllTiles();
                    }
                }
            }

            if (chunk.cliffTilemaps != null)
            {
                foreach (Tilemap tilemap in chunk.cliffTilemaps)
                {
                    if (tilemap != null)
                    {
                        tilemap.ClearAllTiles();
                    }
                }
            }
        }


        private GameObject AcquireChunkRoot()
        {
            if (!useChunkRootPooling) return null;
            if (chunkRootPool.Count == 0) return null;

            GameObject root = chunkRootPool.Pop();
            root.SetActive(true);
            root.transform.SetParent(tilesParent, false);
            return root;
        }


        private void ReleaseChunkRoot(Chunk chunk)
        {
            if (chunk.root == null) return;

            if (useChunkRootPooling && chunkRootPool.Count < maxChunkRootPoolSize)
            {
                chunk.root.SetActive(false);
                if (pooledChunkRootsParent != null)
                {
                    chunk.root.transform.SetParent(pooledChunkRootsParent, false);
                }
                chunkRootPool.Push(chunk.root);
            }
            else
            {
                DestroyChunkRootObject(chunk.root);
            }

            chunk.root = null;
            chunk.tilemaps = null;
            chunk.tilemapRenderers = null;
            chunk.cliffTilemaps = null;
            chunk.cliffTilemapRenderers = null;
        }


        private void DestroyChunkRootObject(GameObject root)
        {
            if (root == null) return;

            if (Application.isPlaying)
            {
                Destroy(root);
            }
            else
            {
                DestroyImmediate(root);
            }
        }


        private void ClearChunkRootChildren(Transform root)
        {
            if (root == null) return;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                DestroyChunkRootObject(child.gameObject);
            }
        }

    }
}
