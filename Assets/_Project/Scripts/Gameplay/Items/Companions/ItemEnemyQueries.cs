using UnityEngine;
namespace Necrocis
{
    internal static class ItemEnemyQueries
    {
        internal static bool IsTargetable(EnemyController enemy)
        {
            return enemy != null && enemy.gameObject.activeInHierarchy && !enemy.IsDead;
        }
        internal static EnemyStatusEffectController EnsureStatusController(EnemyController enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            EnemyStatusEffectController status = enemy.StatusEffects;
            if (status == null)
            {
                status = enemy.GetComponent<EnemyStatusEffectController>();
            }

            if (status == null)
            {
                status = enemy.gameObject.AddComponent<EnemyStatusEffectController>();
            }

            status.Initialize(enemy);
            return status;
        }
    }
}
