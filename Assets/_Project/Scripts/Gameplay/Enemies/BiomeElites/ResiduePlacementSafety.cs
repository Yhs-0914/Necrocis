using UnityEngine;
namespace Necrocis
{
    public static class ResiduePlacementSafety
    {
        public static bool CanPlace(Vector3 center,Vector3 forward,Vector2 size)=>EnemyWorldBindings.Placement.CanPlaceResidue(center,forward,size);
    }
}
