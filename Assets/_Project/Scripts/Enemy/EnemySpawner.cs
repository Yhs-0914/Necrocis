using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 플레이어 근처에서만 적을 활성화하고 리스폰을 관리하는 스포너.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        private const int SpawnPositionAttempts = 10;       // 스폰 위치 탐색 최대 시도 횟수
        private const float MinSpawnSpacing = 0.75f;       // 적 간 최소 거리 (겹침 방지)
        private const float ViewportVisibilityMargin = 0.05f; // 카메라 시야 마진 (밖에서만 리스폰)

        private readonly List<EnemyController> activeEnemies = new List<EnemyController>(); // 현재 활성 적 목록

        private EnemySpawnRuleConfig config;     // 스폰 규칙 설정
        private Transform playerTransform;       // 플레이어 Transform 캐시
        private Transform spawnParent;           // 생성된 적의 부모 Transform
        private Vector3 anchorPosition;          // 스폰 중심점
        private float nextSpawnTime;             // 다음 리스폰 가능 시간
        private bool initializedWave;            // 초기 웨이브 생성 완료 여부
        private int enemyPoolArchetypeId;        // 풀 분류 ID

        // 스포너 초기 설정: 이전 적 정리 → 새 설정 적용
        public void Configure(EnemySpawnRuleConfig config, Vector3 anchorPosition)
        {
            ClearSpawnedEnemies();

            this.config = config;
            this.anchorPosition = anchorPosition;
            playerTransform = null;
            spawnParent = transform.parent;
            nextSpawnTime = 0f;
            initializedWave = false;
            enemyPoolArchetypeId = EnemyController.GetPoolArchetypeId(config);
            enabled = config != null;
        }

        // 매 프레임: 플레이어 거리 확인 → 활성화 범위 내면 스폰, 밖이면 정리
        // 리스폰은 카메라 시야 밖에서만 발생 (플레이어 눈앞에서 갑자기 나타나는 것 방지)
        private void Update()
        {
            if (config == null)
            {
                return;
            }

            CleanupReleasedEnemies();
            EnsurePlayerTransform();
            if (playerTransform == null)
            {
                return;
            }

            // 활성화 범위 밖이면 모든 적 해제
            float playerDistance = GetPlanarDistance(playerTransform.position, anchorPosition);
            if (playerDistance > config.activationRadius)
            {
                if (activeEnemies.Count > 0)
                {
                    ClearSpawnedEnemies();
                }
                initializedWave = false;
                return;
            }

            if (!initializedWave)
            {
                if (TrySpawnWave())
                {
                    initializedWave = true;
                    nextSpawnTime = Time.time + config.respawnCooldown;
                }
                return;
            }

            if (activeEnemies.Count >= config.maxAlive || Time.time < nextSpawnTime)
            {
                return;
            }

            if (IsSpawnerVisibleToCamera())
            {
                return;
            }

            if (TrySpawnWave())
            {
                nextSpawnTime = Time.time + config.respawnCooldown;
            }
        }

        private void OnDisable()
        {
            if (ShouldAutoReleaseOnLifecycleEvent())
            {
                ReleaseSpawnedEnemies();
            }
        }

        private void OnDestroy()
        {
            if (ShouldAutoReleaseOnLifecycleEvent())
            {
                ReleaseSpawnedEnemies();
            }
        }

        public void ReleaseSpawnedEnemies()
        {
            ClearSpawnedEnemies();
        }

        private bool ShouldAutoReleaseOnLifecycleEvent()
        {
            if (!Application.isPlaying)
            {
                return false;
            }

            if (!gameObject.activeSelf)
            {
                return false;
            }

            Transform parent = transform.parent;
            if (parent != null && !parent.gameObject.activeInHierarchy)
            {
                return false;
            }

            return true;
        }

        public void NotifyEnemyReleased(EnemyController enemy)
        {
            activeEnemies.Remove(enemy);
            if (enemy != null && enemy.IsDead && initializedWave && activeEnemies.Count < config.maxAlive)
            {
                nextSpawnTime = Time.time + config.respawnCooldown;
            }
        }

        private bool TrySpawnWave()
        {
            while (activeEnemies.Count < config.maxAlive)
            {
                if (!SpawnEnemy())
                {
                    break;
                }
            }

            return activeEnemies.Count > 0;
        }

        private bool SpawnEnemy()
        {
            if (!TryGetSpawnPosition(out Vector3 spawnPosition))
            {
                return false;
            }

            Transform currentSpawnParent = transform.parent != null ? transform.parent : spawnParent;
            if (currentSpawnParent == null)
            {
                return false;
            }

            spawnParent = currentSpawnParent;
            EnemyController controller = EnemyController.Acquire(currentSpawnParent, $"{config.name}_{activeEnemies.Count}", enemyPoolArchetypeId);
            controller.Configure(this, config, anchorPosition, spawnPosition);
            activeEnemies.Add(controller);
            return true;
        }

        private bool TryGetSpawnPosition(out Vector3 spawnPosition)
        {
            BiomeManager biome = BiomeManager.Active;
            if (biome == null)
            {
                spawnPosition = Vector3.zero;
                return false;
            }

            for (int i = 0; i < SpawnPositionAttempts; i++)
            {
                Vector2 offset2D = Random.insideUnitCircle * Mathf.Max(0f, config.spawnRadius);
                Vector3 candidate = anchorPosition + new Vector3(offset2D.x, 0f, offset2D.y);
                Vector2Int grid = biome.WorldToGrid(candidate);
                if (!biome.IsValidPosition(grid.x, grid.y) || !biome.IsWalkable(grid.x, grid.y))
                {
                    continue;
                }

                candidate.y = biome.GetGroundHeight(candidate) + config.heightOffset;
                if (IsTooCloseToSpawnedEnemy(candidate))
                {
                    continue;
                }

                spawnPosition = candidate;
                return true;
            }

            spawnPosition = Vector3.zero;
            return false;
        }

        private bool IsTooCloseToSpawnedEnemy(Vector3 candidate)
        {
            float minDistanceSq = MinSpawnSpacing * MinSpawnSpacing;
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                EnemyController enemy = activeEnemies[i];
                if (enemy == null)
                {
                    continue;
                }

                Vector3 delta = enemy.transform.position - candidate;
                delta.y = 0f;
                if (delta.sqrMagnitude < minDistanceSq)
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsurePlayerTransform()
        {
            if (playerTransform != null)
            {
                return;
            }

            if (PlayerController.Instance != null)
            {
                playerTransform = PlayerController.Instance.transform;
            }
        }

        private void CleanupReleasedEnemies()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] == null)
                {
                    activeEnemies.RemoveAt(i);
                }
            }
        }

        private void ClearSpawnedEnemies()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                EnemyController enemy = activeEnemies[i];
                if (enemy != null)
                {
                    enemy.ReleaseToPool();
                }
            }

            activeEnemies.Clear();
        }

        private bool IsSpawnerVisibleToCamera()
        {
            Camera activeCamera = DontStarveCamera.GetActiveCamera();

            if (activeCamera == null)
            {
                return false;
            }

            Vector3 visibilityPosition = anchorPosition;
            visibilityPosition.y += config != null ? config.heightOffset : 0f;

            Vector3 viewportPoint = activeCamera.WorldToViewportPoint(visibilityPosition);
            if (viewportPoint.z <= 0f)
            {
                return false;
            }

            return viewportPoint.x >= -ViewportVisibilityMargin
                && viewportPoint.x <= 1f + ViewportVisibilityMargin
                && viewportPoint.y >= -ViewportVisibilityMargin
                && viewportPoint.y <= 1f + ViewportVisibilityMargin;
        }

        private static float GetPlanarDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
