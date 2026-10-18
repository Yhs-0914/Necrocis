using UnityEngine;

namespace Necrocis
{
    // Bound to the spawning biome. A pooled actor receives a fresh binding on Configure.
    public sealed class BiomeWorldQueries : IEnemyWorldQueries
    {
        private readonly BiomeManager biome;
        private BiomeWorldQueries(BiomeManager biome) { this.biome = biome; }
        public static IEnemyWorldQueries ForBiome(BiomeManager biome) => new BiomeWorldQueries(biome);
        public bool HasTerrain => biome != null;
        public bool IsValidCell(Vector2Int cell) => biome != null && biome.IsValidPosition(cell.x, cell.y);
        public bool IsWalkableCell(Vector2Int cell) => biome != null && biome.IsWalkable(cell.x, cell.y);
        public Vector2Int WorldToCell(Vector3 position) => biome != null ? biome.WorldToGrid(position)
            : new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.z));
        public Vector3 CellToWorld(Vector2Int cell) => biome != null ? biome.GridToWorld(cell.x, cell.y)
            : new Vector3(cell.x, 0f, cell.y);
        public float GetGroundHeight(Vector3 position) => biome != null ? biome.GetGroundHeight(position) : 0f;
        public float GetGroundHeight(Vector2Int cell) => biome != null ? biome.GetGroundHeight(cell.x, cell.y) : 0f;
        public bool CanEnemyMove(Vector3 from, Vector3 to, Vector2 halfExtents)
            => ResidueRubble.CanTraverse(from, to, halfExtents) && (biome == null || biome.CanMove(from, to));
        public bool IsPlayerInsideLockedEncounter(Vector3 position)
            => EncounterSpatialQueries.IsInsideLockedArena(position);
    }
}
