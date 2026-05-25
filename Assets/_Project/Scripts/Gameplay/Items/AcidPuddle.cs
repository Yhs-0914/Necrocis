using UnityEngine;

namespace Necrocis
{
    public class AcidPuddle : MonoBehaviour
    {
        private float tickDamage;
        private float radius;
        private float tickInterval;
        private float endTime;
        private float nextTickTime;

        public static AcidPuddle Spawn(Vector3 position, float tickDamage, float duration, float radius, float tickInterval)
        {
            GameObject puddleObject = new GameObject("AcidPuddle");
            puddleObject.transform.position = new Vector3(position.x, position.y + 0.05f, position.z);

            AcidPuddle puddle = puddleObject.AddComponent<AcidPuddle>();
            puddle.Initialize(tickDamage, duration, radius, tickInterval);
            return puddle;
        }

        public void Initialize(float damage, float duration, float areaRadius, float interval)
        {
            tickDamage = Mathf.Max(0.1f, damage);
            radius = Mathf.Max(0.2f, areaRadius);
            tickInterval = Mathf.Max(0.05f, interval);
            endTime = Time.time + Mathf.Max(0.1f, duration);
            nextTickTime = Time.time;
        }

        private void Update()
        {
            if (Time.time >= endTime)
            {
                Destroy(gameObject);
                return;
            }

            if (Time.time < nextTickTime)
            {
                return;
            }

            nextTickTime = Time.time + tickInterval;
            ApplyTickDamage();
        }

        private void ApplyTickDamage()
        {
            var enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return;
            }

            Vector3 origin = transform.position;
            float radiusSqr = radius * radius;

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - origin;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude > radiusSqr)
                {
                    continue;
                }

                enemy.TakeDamage(tickDamage);
            }
        }
    }
}
