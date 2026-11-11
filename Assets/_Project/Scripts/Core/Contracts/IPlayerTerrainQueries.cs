using System;
using UnityEngine;

namespace Necrocis
{
    public interface IPlayerTerrainQueries
    {
        bool IsReady { get; }
        bool TryBindPlayer(UnityEngine.Object player, Action<Vector3> bindSpawn);
        bool CanPlayerMove(Vector3 from, Vector3 to, Vector2 halfExtents);
        bool CanStartTraversal(Vector3 from, Vector3 to, Vector2 halfExtents);
        bool TryGetClimbDestination(Vector3 from, Vector3 facing, out Vector3 destination);
        bool TryGetLavaJumpDestination(Vector3 from, Vector3 facing, out Vector3 destination);
        bool TryGetClearTraversalLanding(Vector3 destination, Vector3 facing, Vector2 halfExtents, out Vector3 landing);
        int GetHeightLevel(Vector3 position);
    }
}
