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
        private float startTime;
        private float lifeDuration;
        private SpriteRenderer visualRenderer;

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
            lifeDuration = Mathf.Max(0.1f, duration);
            startTime = Time.time;
            endTime = startTime + lifeDuration;
            nextTickTime = Time.time;
            EnsureVisual();
            transform.localScale = Vector3.one * Mathf.Max(0.2f, radius * 2f);
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
                UpdateVisual();
                return;
            }

            nextTickTime = Time.time + tickInterval;
            ApplyTickDamage();
            UpdateVisual();
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

        private void EnsureVisual()
        {
            if (visualRenderer != null)
            {
                return;
            }

            visualRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (visualRenderer == null)
            {
                visualRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            visualRenderer.sprite = TextureSpriteCache.GetCircleSprite();
            visualRenderer.color = new Color(0.32f, 0.95f, 0.28f, 0.42f);
            visualRenderer.sortingOrder = 1200;
        }

        private void UpdateVisual()
        {
            if (visualRenderer == null)
            {
                return;
            }

            float elapsed = Mathf.Clamp01((Time.time - startTime) / Mathf.Max(0.01f, lifeDuration));
            float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.05f;
            transform.localScale = Vector3.one * Mathf.Max(0.2f, radius * 2f) * pulse;
            Color color = visualRenderer.color;
            color.a = Mathf.Lerp(0.42f, 0.08f, elapsed);
            visualRenderer.color = color;
        }
    }
}
