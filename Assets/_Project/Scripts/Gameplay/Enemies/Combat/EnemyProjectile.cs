using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 적 원거리 투사체. 플레이어 방향으로 날아가서 거리 기반으로 충돌 판정.
    /// EnemyController에서 Acquire → Launch로 발사.
    /// </summary>
    public class EnemyProjectile : MonoBehaviour
    {
        private const string PoolRootName = "__EnemyProjectilePool";
        private const float HitRadius = 0.8f;

        private static readonly Stack<EnemyProjectile> Pool = new Stack<EnemyProjectile>();
        private static Transform poolRoot;

        private static Sprite defaultProjectileSprite;

        private SpriteRenderer spriteRenderer;

        private Vector3 moveDirection;
        private float speed;
        private float damage;
        private float lifeTime;
        private float elapsed;
        private bool launched;
        private EnemyController ownerEnemy;

        // ─────────────────────────────────
        // 풀링 API
        // ─────────────────────────────────

        public static EnemyProjectile Acquire(Vector3 position, Sprite sprite, Vector3 scale)
        {
            EnsurePoolRoot();

            EnemyProjectile proj = null;
            while (Pool.Count > 0)
            {
                proj = Pool.Pop();
                if (proj != null && proj.gameObject != null) break;
                proj = null;
            }

            if (proj == null)
            {
                GameObject go = new GameObject("EnemyProjectile");
                proj = go.AddComponent<EnemyProjectile>();
            }

            proj.EnsureComponents();
            proj.transform.SetParent(null, false);
            proj.transform.position = position;
            proj.transform.localScale = scale;
            proj.spriteRenderer.sprite = sprite != null ? sprite : GetDefaultSprite();
            proj.spriteRenderer.enabled = true;
            proj.launched = false;
            proj.elapsed = 0f;
            proj.gameObject.SetActive(true);
            return proj;
        }

        public void Launch(Vector3 direction, float damage, float speed, float lifeTime, EnemyController sourceEnemy = null)
        {
            moveDirection = direction.normalized;
            this.damage = damage;
            this.speed = speed;
            this.lifeTime = lifeTime;
            elapsed = 0f;
            launched = true;
            ownerEnemy = sourceEnemy;
        }

        private void ReturnToPool()
        {
            if (!launched && !gameObject.activeSelf) return;
            launched = false;
            ownerEnemy = null;
            gameObject.SetActive(false);
            EnsurePoolRoot();
            transform.SetParent(poolRoot, false);
            Pool.Push(this);
        }

        // ─────────────────────────────────
        // Unity 라이프사이클
        // ─────────────────────────────────

        private void Update()
        {
            if (!launched) return;

            // 수명 체크
            elapsed += Time.deltaTime;
            if (elapsed >= lifeTime)
            {
                ReturnToPool();
                return;
            }

            // 이동
            transform.position += moveDirection * speed * Time.deltaTime;

            // 카메라를 향해 회전 (빌보드)
            Camera activeCamera = DontStarveCamera.GetActiveCamera();
            if (activeCamera != null)
            {
                transform.rotation = activeCamera.transform.rotation;
            }

            // 거리 기반 플레이어 충돌 판정
            if (PlayerController.Instance == null) return;

            Vector3 toPlayer = PlayerController.Instance.transform.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude <= HitRadius * HitRadius)
            {
                Health playerHealth = PlayerController.Instance.GetComponent<Health>();
                if (playerHealth != null && !playerHealth.IsDead)
                {
                    playerHealth.TakeDamage(damage, ownerEnemy);
                }
                ReturnToPool();
            }
        }

        // ─────────────────────────────────
        // 초기화
        // ─────────────────────────────────

        private void EnsureComponents()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                    spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            spriteRenderer.sortingOrder = 5000;

            // 기존 Billboard 제거 (직접 카메라 회전 사용)
            Billboard oldBillboard = gameObject.GetComponent<Billboard>();
            if (oldBillboard != null)
            {
                Object.Destroy(oldBillboard);
            }
        }

        /// <summary>
        /// 기본 구체 스프라이트 생성 (투사체 스프라이트가 없을 때 사용)
        /// </summary>
        private static Sprite GetDefaultSprite()
        {
            if (defaultProjectileSprite != null) return defaultProjectileSprite;

            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = (size - 1) * 0.5f;
            float radius = center;
            float radiusSq = radius * radius;

            Color coreColor = new Color(1f, 1f, 0.4f, 1f);    // 밝은 노란색 중심
            Color edgeColor = new Color(1f, 0.5f, 0.1f, 1f);  // 주황색 가장자리
            Color glowColor = new Color(1f, 0.7f, 0.2f, 0.5f); // 외곽 글로우

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float distSq = dx * dx + dy * dy;
                    float dist = Mathf.Sqrt(distSq);

                    if (dist <= radius * 0.6f)
                    {
                        // 밝은 중심
                        float t = dist / (radius * 0.6f);
                        tex.SetPixel(x, y, Color.Lerp(coreColor, edgeColor, t));
                    }
                    else if (dist <= radius)
                    {
                        // 가장자리 → 글로우
                        float t = (dist - radius * 0.6f) / (radius * 0.4f);
                        tex.SetPixel(x, y, Color.Lerp(edgeColor, glowColor, t));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();

            // PPU를 낮춰서 월드에서 크게 보이도록 (32px / 16PPU = 2 월드 유닛)
            defaultProjectileSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            defaultProjectileSprite.name = "DefaultProjectile";
            return defaultProjectileSprite;
        }

        private static void EnsurePoolRoot()
        {
            if (poolRoot != null) return;
            GameObject root = GameObject.Find(PoolRootName);
            if (root == null) root = new GameObject(PoolRootName);
            poolRoot = root.transform;
        }
    }
}
