using UnityEngine;

namespace Necrocis
{
    // Enemy movement uses cell terrain rules; player footprint and projectile rules are separate.
    public interface IEnemyWorldQueries
    {
        bool HasTerrain { get; }
        bool IsValidCell(Vector2Int cell);
        bool IsWalkableCell(Vector2Int cell);
        Vector2Int WorldToCell(Vector3 position);
        Vector3 CellToWorld(Vector2Int cell);
        float GetGroundHeight(Vector3 position);
        float GetGroundHeight(Vector2Int cell);
        bool CanEnemyMove(Vector3 from, Vector3 to, Vector2 halfExtents);
        bool IsPlayerInsideLockedEncounter(Vector3 position);
    }
}
