using UnityEngine;

namespace Necrocis
{
    /// <summary>Lightweight homing cell used by the final boss missile volley.</summary>
    [DisallowMultipleComponent]
    public sealed class FinalBossHomingProjectile : MonoBehaviour
    {
        private static Sprite sharedSprite;
        private static int activeCount;

        private Vector3 direction;
        private float damage;
        private float speed;
        private float turnSpeed;
        private float lifeTime;
        private float elapsed;
        private bool counted;
        private SpriteRenderer visual;
        private Vector3 previousPlayerPosition;
        private Color flightColor;
        private Vector3 baseScale;

        public static int ActiveCount => activeCount;

        public static FinalBossHomingProjectile Launch(
            Transform parent,
            Vector3 position,
            Vector3 initialDirection,
            float hitDamage,
            float moveSpeed,
            float turnRate,
            float duration,
            string spriteResource = null)
        {
            GameObject go = new GameObject("FinalBoss_HomingCell");
            go.transform.SetParent(parent, true);
            go.transform.position = position;

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            Sprite sprite = !string.IsNullOrEmpty(spriteResource) ? Resources.Load<Sprite>(spriteResource) : null;
            if (sprite == null) sprite = GetSharedSprite();
            renderer.sprite = sprite;
            renderer.color = turnRate > 0f ? new Color(1f, .65f, .82f) : new Color(1f, .82f, .5f);
            renderer.sortingOrder = 5100;
            float spriteWidth = sprite != null ? Mathf.Max(.01f, sprite.bounds.size.x) : 1f;
            go.transform.localScale = Vector3.one * (1.25f / spriteWidth);

            FinalBossHomingProjectile projectile = go.AddComponent<FinalBossHomingProjectile>();
            projectile.direction = initialDirection.sqrMagnitude > .0001f
                ? initialDirection.normalized
                : Vector3.back;
            projectile.direction.y = 0f;
            projectile.damage = Mathf.Max(0f, hitDamage);
            projectile.speed = Mathf.Max(.1f, moveSpeed);
            projectile.turnSpeed = Mathf.Max(0f, turnRate);
            projectile.lifeTime = Mathf.Max(.1f, duration);
            projectile.visual = renderer;
            projectile.flightColor = renderer.color;
            projectile.baseScale = go.transform.localScale;
            projectile.previousPlayerPosition = PlayerController.Instance != null
                ? PlayerController.Instance.transform.position : position;
            if (!string.IsNullOrEmpty(spriteResource))
            {
                projectile.flightColor = Color.white;
                FinalBossPhaseThreeController.AddEnergyTrail(go.transform, .13f, .24f);
            }
            return projectile;
        }

        public static Sprite GetSharedSprite()
        {
            if (sharedSprite != null) return sharedSprite;
            sharedSprite = Resources.Load<Sprite>("FinalBoss/PhaseTwo/HomingNeuralCell");
            if (sharedSprite != null)
            {
                return sharedSprite;
            }

            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "FinalBossHomingCellTexture",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            float center = (size - 1) * .5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                float alpha = Mathf.Clamp01((1f - distance) * 5f);
                Color color = Color.Lerp(new Color(1f, .15f, .5f), Color.white, Mathf.Clamp01(1f - distance * 1.6f));
                color.a = alpha;
                texture.SetPixel(x, y, color);
            }
            texture.Apply();
            sharedSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 22f);
            sharedSprite.name = "FinalBossHomingCellSprite";
            sharedSprite.hideFlags = HideFlags.HideAndDontSave;
            return sharedSprite;
        }

        private void OnEnable()
        {
            if (counted) return;
            counted = true;
            activeCount++;
        }

        private void OnDisable()
        {
            if (!counted) return;
            counted = false;
            activeCount = Mathf.Max(0, activeCount - 1);
        }

        private void Update()
        {
            if (Time.deltaTime <= 0f) return;
            elapsed += Time.deltaTime;
            PlayerController player = PlayerController.Instance;
            if (player == null || player.IsDead || elapsed >= lifeTime)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;
            // Commit to a heading after the tracking window so a successful dodge stays successful.
            if (turnSpeed > 0f && elapsed >= .2f && elapsed < 2.2f && toPlayer.sqrMagnitude > .0001f)
            {
                direction = Vector3.RotateTowards(
                    direction,
                    toPlayer.normalized,
                    turnSpeed * Mathf.Deg2Rad * Time.deltaTime,
                    0f).normalized;
            }

            Vector3 previousPosition = transform.position;
            transform.position += direction * speed * Time.deltaTime;
            if (visual != null)
            {
                Color tint = flightColor;
                tint.a = Mathf.Clamp01((lifeTime - elapsed) / .45f);
                visual.color = tint;
                transform.localScale = baseScale * (1f + Mathf.Sin(elapsed * 11f) * .035f);
            }
            Camera camera = DontStarveCamera.GetActiveCamera();
            if (camera != null)
            {
                float screenX = Vector3.Dot(direction, camera.transform.right);
                float screenY = Vector3.Dot(direction, camera.transform.up);
                float angle = Mathf.Atan2(screenY, screenX) * Mathf.Rad2Deg;
                transform.rotation = camera.transform.rotation * Quaternion.Euler(0f, 0f, angle);
            }

            Vector3 playerPosition = player.transform.position;
            // Ignore scene-spawn teleports; ordinary motion uses a swept relative segment.
            if ((playerPosition - previousPlayerPosition).sqrMagnitude > 100f)
                previousPlayerPosition = playerPosition;
            Vector3 relativeStart = previousPosition - previousPlayerPosition;
            Vector3 relativeEnd = transform.position - playerPosition;
            previousPlayerPosition = playerPosition;
            // The dissipating tail is visual only, so a nearly invisible cell cannot hurt the player.
            if (elapsed >= lifeTime - .45f
                || RelativeSegmentDistanceSquared(relativeStart, relativeEnd) > .62f * .62f) return;

            player.TakeDamage(damage);
            Destroy(gameObject);
        }

        private static float RelativeSegmentDistanceSquared(Vector3 start, Vector3 end)
        {
            start.y = end.y = 0f;
            Vector3 travel = end - start;
            float nearest = travel.sqrMagnitude > .000001f
                ? Mathf.Clamp01(-Vector3.Dot(start, travel) / travel.sqrMagnitude) : 0f;
            return (start + travel * nearest).sqrMagnitude;
        }
    }
}
