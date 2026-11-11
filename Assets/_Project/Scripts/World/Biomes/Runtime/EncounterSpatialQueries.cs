using UnityEngine;

namespace Necrocis
{
    // Encounter boundaries remain dynamic. Do not cache the final boss's shrinking floor.
    public static class EncounterSpatialQueries
    {
        public static bool IsInsideLockedArena(Vector3 position)
            => MidBossArenaController.IsPlayerInsideLockedArena(position);

        public static bool CanPlayerTraverse(Vector3 from, Vector3 to, Vector2 halfExtents)
        {
            if (!ResidueRubble.CanTraverse(from, to, halfExtents)) return false;
            FinalBossPhaseThreeController boss = FinalBossArena.Instance != null
                ? FinalBossArena.Instance.GetComponent<FinalBossPhaseThreeController>() : null;
            return (boss == null || boss.CanCrossBoundary(from, to, halfExtents))
                && MidBossArenaController.CanPlayerTraverseArenaBoundary(from, to, halfExtents);
        }

        public static bool TryClampPlayerMovement(Vector3 from, Vector3 to, float margin, out Vector3 clamped)
            => MidBossArenaController.TryClampPlayerMovementInsideLockedArena(from, to, margin, out clamped);
        public static bool TryClampPosition(Vector3 position, float margin, out Vector3 clamped)
            => MidBossArenaController.TryClampPositionInsideLockedArena(position, margin, out clamped);

        // The existing traversal start only checks rubble and the mid-boss boundary.
        public static bool CanStartTraversal(Vector3 from, Vector3 to, Vector2 halfExtents)
            => ResidueRubble.CanTraverse(from, to, halfExtents)
                && MidBossArenaController.CanPlayerTraverseArenaBoundary(from, to, halfExtents);
    }
}
