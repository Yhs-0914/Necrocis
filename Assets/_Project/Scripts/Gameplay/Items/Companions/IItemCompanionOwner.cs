using UnityEngine;

namespace Necrocis
{
    public interface IItemCompanionOwner
    {
        bool IsAvailable { get; }
        Transform OwnerTransform { get; }
        bool HasBloodDroneItem { get; }
        bool HasGuardianOrganItem { get; }
        float BloodDroneOrbitRadius { get; }
        float BloodDroneTargetRadius { get; }
        float BloodDroneFireInterval { get; }
        float BloodDroneDamageMultiplier { get; }
        float GuardianOrganOrbitRadius { get; }
        float GuardianOrganBlockRadius { get; }
        float GuardianOrganCooldown { get; }
        EnemyController FindClosestEnemy(Vector3 center, float radius);
        float GetCompanionBaseDamage();
        Vector3 GetPlayerVisualCenter();
        void ReleaseBioSummon(PlayerBioSummon summon);
    }
}
