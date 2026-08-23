using System;
using UnityEngine;

namespace Necrocis
{
    public class AcidPuddle : MonoBehaviour
    {
        private const string PoolName = "PlayerItem.AcidPuddle";
        private static readonly Func<GameObject> CreateFunc = CreatePuddleObject;

        private float tickDamage;
        private float radius;
        private float tickInterval;
        private float nextTickTime;
        private float startTime;
        private float lifeDuration;
        private int remainingTicks;
        private SpriteRenderer visualRenderer;

        public static AcidPuddle Spawn(Vector3 position, float tickDamage, float duration, float radius, float tickInterval)
        {
            GameObject puddleObject = RuntimePool.Acquire(PoolName, CreateFunc);
            if (puddleObject == null || !puddleObject.TryGetComponent(out AcidPuddle puddle))
            {
                RuntimePool.Release(puddleObject);
                return null;
            }

            puddleObject.name = "AcidPuddle";
            puddleObject.transform.position = new Vector3(position.x, position.y + 0.05f, position.z);
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
            remainingTicks = Mathf.Max(1, Mathf.RoundToInt(lifeDuration / tickInterval));
            nextTickTime = startTime + tickInterval;
            EnsureVisual();
            visualRenderer.enabled = true;
            transform.localScale = Vector3.one * TextureSpriteCache.GetUniformScaleForWorldSize(
                visualRenderer != null ? visualRenderer.sprite : null,
                Mathf.Max(0.2f, radius * 2f * 0.85f));
        }

        private void Update()
        {
            if (remainingTicks <= 0)
            {
                RuntimePool.Release(gameObject);
                return;
            }

            if (Time.time < nextTickTime)
            {
                UpdateVisual();
                return;
            }

            nextTickTime += tickInterval;
            ApplyTickDamage();
            remainingTicks--;
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

            Sprite effectSprite = TextureSpriteCache.LoadResourceSprite("ItemEffects/acidic_rupture_effect");
            visualRenderer.sprite = effectSprite != null ? effectSprite : TextureSpriteCache.GetCircleSprite();
            visualRenderer.color = effectSprite != null
                ? new Color(1f, 1f, 1f, 0.72f)
                : new Color(0.32f, 0.95f, 0.28f, 0.42f);
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
            float effectScale = TextureSpriteCache.GetUniformScaleForWorldSize(
                visualRenderer.sprite,
                Mathf.Max(0.2f, radius * 2f * 0.85f));
            transform.localScale = Vector3.one * effectScale * pulse;
            Color color = visualRenderer.color;
            color.a = Mathf.Lerp(0.72f, 0.08f, elapsed);
            visualRenderer.color = color;
        }

        private static GameObject CreatePuddleObject()
        {
            GameObject obj = new GameObject("AcidPuddle");
            obj.AddComponent<SpriteRenderer>();
            obj.AddComponent<AcidPuddle>();
            return obj;
        }
    }
}
