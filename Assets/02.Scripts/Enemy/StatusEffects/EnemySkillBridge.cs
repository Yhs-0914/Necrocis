using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public class EnemySkillBridge : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyController;
        [SerializeField] private EnemyStatusEffectController statusController;

        public string EnemyName => enemyController != null ? enemyController.gameObject.name : gameObject.name;

        public bool IsValidTarget
        {
            get
            {
                EnsureReferences();
                return enemyController != null && !enemyController.IsDead;
            }
        }

        public void Bind(EnemyController enemy, EnemyStatusEffectController status)
        {
            enemyController = enemy;
            statusController = status;
        }

        public void ApplyDamage(float damage)
        {
            EnsureReferences();
            if (!IsValidTarget || damage <= 0f)
            {
                return;
            }

            enemyController.TakeDamage(damage);
        }

        public void ApplyStun(float duration)
        {
            EnsureReferences();
            if (!IsValidTarget)
            {
                return;
            }

            statusController?.ApplyStun(duration);
        }

        public void ApplyPoison(float duration, float tickInterval, float tickDamage)
        {
            EnsureReferences();
            if (!IsValidTarget)
            {
                return;
            }

            statusController?.ApplyPoison(duration, tickInterval, tickDamage);
        }

        public void ApplyDamageTakenIncrease(float increaseRatio, float duration)
        {
            EnsureReferences();
            if (!IsValidTarget)
            {
                return;
            }

            statusController?.ApplyDamageTakenIncrease(increaseRatio, duration);
        }

        public static bool TryGetFromCollider(Collider other, out EnemySkillBridge bridge)
        {
            bridge = null;
            if (other == null)
            {
                return false;
            }

            bridge = other.GetComponent<EnemySkillBridge>();
            if (bridge == null)
            {
                bridge = other.GetComponentInParent<EnemySkillBridge>();
            }

            if (bridge != null)
            {
                bridge.EnsureReferences();
                return bridge.enemyController != null;
            }

            EnemyController enemy = other.GetComponentInParent<EnemyController>();
            if (enemy == null)
            {
                return false;
            }

            bridge = enemy.GetComponent<EnemySkillBridge>();
            if (bridge == null)
            {
                bridge = enemy.gameObject.AddComponent<EnemySkillBridge>();
            }

            EnemyStatusEffectController status = enemy.GetComponent<EnemyStatusEffectController>();
            if (status == null)
            {
                status = enemy.gameObject.AddComponent<EnemyStatusEffectController>();
            }

            status.Initialize(enemy);
            bridge.Bind(enemy, status);
            return true;
        }

        private void EnsureReferences()
        {
            if (enemyController == null)
            {
                enemyController = GetComponent<EnemyController>();
                if (enemyController == null)
                {
                    enemyController = GetComponentInParent<EnemyController>();
                }
            }

            if (statusController == null)
            {
                statusController = GetComponent<EnemyStatusEffectController>();
                if (statusController == null && enemyController != null)
                {
                    statusController = enemyController.GetComponent<EnemyStatusEffectController>();
                }
            }

            if (statusController == null && enemyController != null)
            {
                statusController = enemyController.gameObject.AddComponent<EnemyStatusEffectController>();
            }

            if (statusController != null && enemyController != null)
            {
                statusController.Initialize(enemyController);
            }
        }
    }
}
