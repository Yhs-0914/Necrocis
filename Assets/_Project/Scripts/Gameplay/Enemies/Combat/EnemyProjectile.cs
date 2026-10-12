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
        private const float MinHitRadius = 0.15f;

        private static readonly Stack<EnemyProjectile> Pool = new Stack<EnemyProjectile>();
        private static readonly List<EnemyProjectile> ActiveProjectiles = new List<EnemyProjectile>();
        private static Transform poolRoot;

        private static Sprite defaultProjectileSprite;

        private SpriteRenderer spriteRenderer;

        private Vector3 moveDirection;
        private float speed;
        private EnemyDamageRequest damage;
        private float lifeTime;
        private float elapsed;
        private bool launched;
        private EnemyController ownerEnemy;
        private PlayerController cachedPlayer;
        private Collider cachedPlayerCollider;
        private Health cachedPlayerHealth;

        public static IReadOnlyList<EnemyProjectile> ActiveEnemyProjectiles => ActiveProjectiles;
        public bool IsLaunched => launched && gameObject.activeSelf;

        public static int ReturnProjectilesOwnedBy(EnemyController owner)
        {
            if (owner == null)
            {
                return 0;
            }

            int returnedCount = 0;
            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
            {
                EnemyProjectile projectile = ActiveProjectiles[i];
                if (projectile == null)
                {
                    ActiveProjectiles.RemoveAt(i);
                    continue;
                }

                if (projectile.ownerEnemy != owner)
                {
                    continue;
                }

                projectile.ReturnToPool();
                returnedCount++;
            }

            return returnedCount;
        }

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
            if (!ActiveProjectiles.Contains(proj))
            {
                ActiveProjectiles.Add(proj);
            }
            return proj;
        }

        public void Launch(Vector3 direction, float damage, float speed, float lifeTime, EnemyController sourceEnemy = null)
        {
            moveDirection = direction.normalized;
            this.damage = sourceEnemy != null ? sourceEnemy.CreateAttackDamage(damage)
                : new EnemyDamageRequest(damage * DifficultyBalanceService.GetIncomingDamageMultiplier(null), null);
            this.speed = speed;
            this.lifeTime = lifeTime;
            elapsed = 0f;
            launched = true;
            ownerEnemy = sourceEnemy;
        }

        private void ReturnToPool()
        {
            if (!launched && !gameObject.activeSelf) return;
            ActiveProjectiles.Remove(this);
            launched = false;
            ownerEnemy = null;
            gameObject.SetActive(false);
            EnsurePoolRoot();
            transform.SetParent(poolRoot, false);
            Pool.Push(this);
        }

        public void Deflect()
        {
            ReturnToPool();
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

            Vector3 previousPosition = transform.position;

            // 이동
            transform.position += moveDirection * speed * Time.deltaTime;

            // 카메라를 향해 회전 (빌보드)
            Camera activeCamera = DontStarveCamera.GetActiveCamera();
            if (activeCamera != null)
            {
                Quaternion cameraRotation = activeCamera.transform.rotation;
                if (transform.rotation != cameraRotation)
                {
                    transform.rotation = cameraRotation;
                }
            }

            // 거리 기반 플레이어 충돌 판정
            if (!TryGetPlayerHitContext(out PlayerController player, out Collider playerCollider, out Health playerHealth))
            {
                return;
            }

            if (IsTouchingPlayer(player, playerCollider, previousPosition, transform.position))
            {
                if (playerHealth != null && !playerHealth.IsDead)
                {
                    player.TakeDamage(damage);
                }
                CombatVfx.PlayHostileProjectileImpact(transform.position, moveDirection);
                ReturnToPool();
            }
        }

        private bool TryGetPlayerHitContext(out PlayerController player, out Collider playerCollider, out Health playerHealth)
        {
            player = PlayerController.Instance;
            if (player == null)
            {
                playerCollider = null;
                playerHealth = null;
                cachedPlayer = null;
                cachedPlayerCollider = null;
                cachedPlayerHealth = null;
                return false;
            }

            if (cachedPlayer != player)
            {
                cachedPlayer = player;
                cachedPlayerCollider = player.HitCollider;
                cachedPlayerHealth = player.HealthComponent;
            }
            else
            {
                if (cachedPlayerCollider == null)
                {
                    cachedPlayerCollider = player.HitCollider;
                }

                if (cachedPlayerHealth == null)
                {
                    cachedPlayerHealth = player.HealthComponent;
                }
            }

            playerCollider = cachedPlayerCollider;
            playerHealth = cachedPlayerHealth;
            return true;
        }

        private bool IsTouchingPlayer(PlayerController player, Collider playerCollider, Vector3 previousPosition, Vector3 currentPosition)
            => CombatHitGeometry.SweepTouchesPlayer(previousPosition, currentPosition, GetCurrentHitRadius(), player);

        private float GetCurrentHitRadius()
        {
            Vector3 scale = transform.lossyScale;
            float visualRadius = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z)) * 0.5f;
            return Mathf.Max(MinHitRadius, visualRadius);
        }

        private void OnDisable()
        {
            ActiveProjectiles.Remove(this);
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
