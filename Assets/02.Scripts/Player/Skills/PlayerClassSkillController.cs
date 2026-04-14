using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public enum PlayerClassType
    {
        None,
        Warrior,
        Mage,
        Archer
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController))]
    public class PlayerClassSkillController : MonoBehaviour
    {
        [System.Serializable]
        private class WarriorSkill1Config
        {
            public float cooldown = 4f;
            public float damage = 6f;
            public float range = 3f;
            public float angle = 120f;
            public float bleedDuration = 3f;
            public float bleedTickInterval = 1f;
            public float bleedTickDamage = 1.5f;
            public GameObject hitEffectPrefab;
            public float hitEffectLifetime = 0.4f;
            public float fallbackEffectScale = 0.8f;
        }

        [System.Serializable]
        private class WarriorSkill2Config
        {
            public float cooldown = 8f;
            public float radius = 2.8f;
            public float attackPowerScale = 1.5f;
            public float knockbackDistance = 1.5f;
            public GameObject areaEffectPrefab;
            public float areaEffectLifetime = 0.5f;
            public float fallbackEffectScale = 3f;
        }

        [System.Serializable]
        private class MageSkill1Config
        {
            public float cooldown = 4f;
            public float radius = 3f;
            public float forwardOffset = 0f;
            public float baseDamage = 0f;
            public float attackPowerScale = 1f;
            public float additionalDamage = 0f;
            public float stunDuration = 1f;
            public GameObject areaEffectPrefab;
            public float areaEffectLifetime = 1f;
            public float fallbackEffectScale = 2.5f;
        }

        [System.Serializable]
        private class MageSkill2Config
        {
            public float cooldown = 10f;
            public float radius = 3.5f;
            public float forwardOffset = 2.5f;
            public float baseDamage = 15f;
            public float attackPowerScale = 0f;
            public float damageTakenIncreaseRatio = 0.1f;
            public float damageTakenIncreaseDuration = 3f;
            public GameObject explosionEffectPrefab;
            public float explosionEffectLifetime = 1f;
            public float fallbackEffectScale = 3f;
        }

        [System.Serializable]
        private class ArcherSkill1Config
        {
            public float cooldown = 5f;
            public int projectileCount = 3;
            public float burstInterval = 0.12f;
            public float projectileDamage = 1.7f;
            public float projectileSpeed = 16f;
            public float projectileLifeTime = 2f;
            public GameObject projectilePrefab;
            public float projectileScale = 0.25f;
            public float poisonDuration = 4f;
            public float poisonTickInterval = 1f;
            public float poisonTickDamage = 0.5f;
            public GameObject shootEffectPrefab;
            public float shootEffectLifetime = 0.4f;
        }

        [System.Serializable]
        private class ArcherSkill2Config
        {
            public float cooldown = 8f;
            public float targetForwardOffset = 6f;

            public float bigCellDamage = 7f;
            public float bigCellImpactRadius = 1.5f;
            public float bigCellTravelSpeed = 10f;
            public GameObject bigCellVisualPrefab;
            public float bigCellVisualScale = 0.6f;
            public GameObject bigCellImpactEffectPrefab;
            public float bigCellImpactEffectLifetime = 1f;

            public int rainProjectileCount = 10;
            public float rainRadius = 4f;
            public float rainDamage = 0.7f;
            public float rainSpawnHeight = 6f;
            public float rainProjectileSpeed = 14f;
            public float rainProjectileLifeTime = 2f;
            public float rainSpawnInterval = 0.04f;
            public GameObject rainProjectilePrefab;
            public float rainProjectileScale = 0.2f;
            public GameObject rainAreaMarkerEffectPrefab;
            public float rainAreaMarkerLifetime = 1f;
        }

        [Header("Class")]
        [SerializeField] private PlayerClassType currentClass = PlayerClassType.Mage;

        [Header("Shared")]
        [SerializeField] private LayerMask enemyMask = ~0;
        [SerializeField] private int overlapBufferSize = 32;
        [SerializeField] private Transform skillSpawnPoint;
        [SerializeField] private float projectileSpawnOffset = 0.65f;
        [SerializeField] private float projectileSpawnHeight = 1f;
        [SerializeField] private float skillVerticalOffset = 2f;
        [SerializeField] private float skillHitHeightOffset = 0.75f;
        [SerializeField] private float skillHitVerticalHalfHeight = 4f;
        [SerializeField] private bool enableDebugLogs = true;

        [Header("Warrior")]
        [SerializeField] private WarriorSkill1Config warriorSkill1 = new WarriorSkill1Config();
        [SerializeField] private WarriorSkill2Config warriorSkill2 = new WarriorSkill2Config();

        [Header("Mage")]
        [SerializeField] private MageSkill1Config mageSkill1 = new MageSkill1Config();
        [SerializeField] private MageSkill2Config mageSkill2 = new MageSkill2Config();

        [Header("Archer")]
        [SerializeField] private ArcherSkill1Config archerSkill1 = new ArcherSkill1Config();
        [SerializeField] private ArcherSkill2Config archerSkill2 = new ArcherSkill2Config();
        [SerializeField] private bool autoTargetForwardEnemyForArcherSkill2 = true;
        [SerializeField] private float archerSkill2AutoTargetAngle = 90f;

        private readonly HashSet<EnemyController> uniqueEnemies = new HashSet<EnemyController>();

        private PlayerController playerController;
        private Collider[] overlapBuffer;

        private float nextSkill1ReadyTime;
        private float nextSkill2ReadyTime;

        private bool archerSkill1BurstRunning;
        private bool archerSkill2Running;

        public bool ConsumesSkillInput => enabled && currentClass != PlayerClassType.None;

        private void OnEnable()
        {
            LevelUpManager.OnJobChanged += OnJobChanged;
        }

        private void OnDisable()
        {
            LevelUpManager.OnJobChanged -= OnJobChanged;
            archerSkill1BurstRunning = false;
            archerSkill2Running = false;
            StopAllCoroutines();
        }

        private void OnJobChanged(JobType job)
        {
            currentClass = job switch
            {
                JobType.Warrior => PlayerClassType.Warrior,
                JobType.Mage    => PlayerClassType.Mage,
                JobType.Archer  => PlayerClassType.Archer,
                _               => PlayerClassType.None
            };
        }

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            if (enemyMask.value == 0)
            {
                enemyMask = ~0;
                Debug.LogWarning("[PlayerClassSkillController] enemyMask was Nothing. Fallback to Everything.");
            }

            EnsureOverlapBuffer();
            ResolveSkillSpawnPoint();
        }

        private void Update()
        {
            if (!ShouldAcceptInput())
            {
                return;
            }

            InputManager input = InputManager.Instance;
            if (input == null)
            {
                return;
            }

            if (input.Skill1Action.WasPressedThisFrame())
            {
                TryUseSkill1();
            }

            if (input.Skill2Action.WasPressedThisFrame())
            {
                TryUseSkill2();
            }
        }

        private bool ShouldAcceptInput()
        {
            if (!Application.isFocused || Time.timeSinceLevelLoad < 0.5f)
            {
                return false;
            }

            if (playerController == null || playerController.IsDead)
            {
                return false;
            }

            return currentClass != PlayerClassType.None;
        }

        private void TryUseSkill1()
        {
            switch (currentClass)
            {
                case PlayerClassType.Warrior:
                    if (!TryStartCooldown(ref nextSkill1ReadyTime, warriorSkill1.cooldown, "Warrior Skill E"))
                        return;
                    ExecuteWarriorSkill1();
                    break;

                case PlayerClassType.Mage:
                    if (!TryStartCooldown(ref nextSkill1ReadyTime, mageSkill1.cooldown, "Mage Skill E"))
                    {
                        return;
                    }

                    ExecuteMageSkill1();
                    break;

                case PlayerClassType.Archer:
                    if (archerSkill1BurstRunning)
                    {
                        return;
                    }

                    if (!TryStartCooldown(ref nextSkill1ReadyTime, archerSkill1.cooldown, "Archer Skill E"))
                    {
                        return;
                    }

                    StartCoroutine(ExecuteArcherSkill1Burst());
                    break;
            }
        }

        private void TryUseSkill2()
        {
            switch (currentClass)
            {
                case PlayerClassType.Warrior:
                    if (!TryStartCooldown(ref nextSkill2ReadyTime, warriorSkill2.cooldown, "Warrior Skill R"))
                        return;
                    ExecuteWarriorSkill2();
                    break;

                case PlayerClassType.Mage:
                    if (!TryStartCooldown(ref nextSkill2ReadyTime, mageSkill2.cooldown, "Mage Skill R"))
                    {
                        return;
                    }

                    ExecuteMageSkill2();
                    break;

                case PlayerClassType.Archer:
                    if (archerSkill2Running)
                    {
                        return;
                    }

                    if (!TryStartCooldown(ref nextSkill2ReadyTime, archerSkill2.cooldown, "Archer Skill R"))
                    {
                        return;
                    }

                    StartCoroutine(ExecuteArcherSkill2());
                    break;
            }
        }

        private bool TryStartCooldown(ref float nextReadyTime, float cooldown, string label)
        {
            float now = Time.time;
            if (now < nextReadyTime)
            {
                if (enableDebugLogs)
                {
                    float remain = Mathf.Max(0f, nextReadyTime - now);
                    Debug.Log($"[{label}] Cooldown: {remain:0.00}s");
                }

                return false;
            }

            nextReadyTime = now + Mathf.Max(0f, cooldown);
            return true;
        }

        // ── 전사 스킬 ─────────────────────────────────────────────

        /// <summary>
        /// 강타(E) - 전방 적 한 명을 물어뜯음. 6 데미지 + 출혈(3초, 초당 1.5)
        /// </summary>
        private void ExecuteWarriorSkill1()
        {
            if (!TryFindSingleTargetInFront(warriorSkill1.range, warriorSkill1.angle, out EnemyController target))
            {
                if (enableDebugLogs)
                    Debug.Log("Warrior Skill E: 전방에 적 없음");
                return;
            }

            target.TakeDamage(warriorSkill1.damage);

            EnemyStatusEffectController status = EnsureStatusController(target);
            if (status != null)
                status.ApplyBleed(warriorSkill1.bleedDuration, warriorSkill1.bleedTickInterval, warriorSkill1.bleedTickDamage);

            Vector3 hitPos = target.transform.position;
            hitPos.y += skillVerticalOffset;
            SpawnSkillEffect(
                warriorSkill1.hitEffectPrefab,
                hitPos,
                warriorSkill1.hitEffectLifetime,
                warriorSkill1.fallbackEffectScale,
                new Color(0.9f, 0.1f, 0.1f, 0.7f));

            if (enableDebugLogs)
                Debug.Log($"Warrior Skill E (강타) → {target.gameObject.name} | {warriorSkill1.damage} 데미지 + 출혈");
        }

        /// <summary>
        /// 회오리베기(R) - 주변 360도 범위 공격 + 넉백
        /// </summary>
        private void ExecuteWarriorSkill2()
        {
            Vector3 center = transform.position;
            center.y += skillVerticalOffset;
            float damage = playerController.AttackPower * warriorSkill2.attackPowerScale;

            int hitCount = ApplyAreaSkill(center, warriorSkill2.radius, enemy =>
            {
                enemy.TakeDamage(damage);
                Vector3 knockDir = enemy.transform.position - transform.position;
                knockDir.y = 0f;
                if (knockDir.sqrMagnitude < 0.001f)
                    knockDir = GetFacingDirection();
                enemy.ApplyKnockback(knockDir.normalized, warriorSkill2.knockbackDistance);
            });

            SpawnSkillEffect(
                warriorSkill2.areaEffectPrefab,
                center,
                warriorSkill2.areaEffectLifetime,
                warriorSkill2.fallbackEffectScale,
                new Color(1f, 0.6f, 0.1f, 0.45f));

            if (enableDebugLogs)
                Debug.Log($"Warrior Skill R (회오리베기) hit {hitCount} enemies | damage {damage:0.#}");
        }

        /// <summary>
        /// 전방 콘 안에서 가장 가까운 적 한 명을 반환
        /// </summary>
        private bool TryFindSingleTargetInFront(float range, float angle, out EnemyController result)
        {
            result = null;
            EnsureOverlapBuffer();

            Vector3 origin = transform.position;
            origin.y += skillHitHeightOffset;
            Vector3 forward = GetFacingDirection();
            float halfAngle = angle * 0.5f;

            int count = Physics.OverlapSphereNonAlloc(
                origin, Mathf.Max(0.1f, range),
                overlapBuffer, enemyMask,
                QueryTriggerInteraction.Collide);

            float bestDist = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if (overlapBuffer[i] == null) continue;
                if (!TryGetEnemyFromCollider(overlapBuffer[i], out EnemyController enemy)) continue;

                Vector3 toEnemy = enemy.transform.position - transform.position;
                toEnemy.y = 0f;
                float dist = toEnemy.magnitude;
                if (dist > range) continue;
                if (dist > 0.01f && Vector3.Angle(forward, toEnemy / dist) > halfAngle) continue;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    result = enemy;
                }
            }

            return result != null;
        }

        // ──────────────────────────────────────────────────────────

        private void ExecuteMageSkill1()
        {
            Vector3 center = GetSkillCenter(mageSkill1.forwardOffset);
            float damage = mageSkill1.baseDamage + playerController.AttackPower * mageSkill1.attackPowerScale + mageSkill1.additionalDamage;
            int hitCount = ApplyAreaSkill(
                center,
                mageSkill1.radius,
                enemy =>
                {
                    enemy.TakeDamage(damage);
                    EnemyStatusEffectController status = EnsureStatusController(enemy);
                    status?.ApplyStun(mageSkill1.stunDuration);
                });

            SpawnSkillEffect(
                mageSkill1.areaEffectPrefab,
                center,
                mageSkill1.areaEffectLifetime,
                mageSkill1.fallbackEffectScale,
                new Color(0.35f, 0.8f, 1f, 0.45f));

            Debug.Log($"Mage Skill E hit {hitCount} enemies");
        }

        private void ExecuteMageSkill2()
        {
            Vector3 center = GetSkillCenter(mageSkill2.forwardOffset);
            float damage = mageSkill2.baseDamage + playerController.AttackPower * mageSkill2.attackPowerScale;
            int hitCount = ApplyAreaSkill(
                center,
                mageSkill2.radius,
                enemy =>
                {
                    enemy.TakeDamage(damage);
                    EnemyStatusEffectController status = EnsureStatusController(enemy);
                    status?.ApplyDamageTakenIncrease(mageSkill2.damageTakenIncreaseRatio, mageSkill2.damageTakenIncreaseDuration);
                });

            SpawnSkillEffect(
                mageSkill2.explosionEffectPrefab,
                center,
                mageSkill2.explosionEffectLifetime,
                mageSkill2.fallbackEffectScale,
                new Color(1f, 0.5f, 0.2f, 0.45f));

            if (enableDebugLogs)
            {
                Debug.Log($"Mage Skill R hit {hitCount} enemies");
            }
        }

        private IEnumerator ExecuteArcherSkill1Burst()
        {
            archerSkill1BurstRunning = true;

            SkillProjectileDebuff debuff = new SkillProjectileDebuff
            {
                applyPoison = true,
                poisonDuration = archerSkill1.poisonDuration,
                poisonTickInterval = archerSkill1.poisonTickInterval,
                poisonTickDamage = archerSkill1.poisonTickDamage
            };

            int projectileCount = Mathf.Max(1, archerSkill1.projectileCount);
            float interval = Mathf.Max(0.01f, archerSkill1.burstInterval);
            Vector3 direction = GetFacingDirection();

            for (int i = 0; i < projectileCount; i++)
            {
                SpawnSkillProjectile(
                    archerSkill1.projectilePrefab,
                    archerSkill1.projectileScale,
                    archerSkill1.projectileDamage,
                    archerSkill1.projectileSpeed,
                    archerSkill1.projectileLifeTime,
                    direction,
                    debuff);

                SpawnSkillEffect(
                    archerSkill1.shootEffectPrefab,
                    GetProjectileSpawnPosition(direction),
                    archerSkill1.shootEffectLifetime,
                    0.25f,
                    new Color(0.7f, 1f, 0.5f, 0.55f));

                if (i < projectileCount - 1)
                {
                    yield return new WaitForSeconds(interval);
                }
            }

            if (enableDebugLogs)
            {
                Debug.Log($"Archer Skill E fired {projectileCount} projectiles");
            }

            archerSkill1BurstRunning = false;
        }

        private IEnumerator ExecuteArcherSkill2()
        {
            archerSkill2Running = true;

            Vector3 center = ResolveArcherSkill2Center();
            SpawnSkillEffect(
                archerSkill2.rainAreaMarkerEffectPrefab,
                center,
                archerSkill2.rainAreaMarkerLifetime,
                archerSkill2.rainRadius * 2f,
                new Color(0.4f, 0.9f, 0.4f, 0.2f));

            Vector3 start = GetProjectileSpawnPosition(GetFacingDirection()) + Vector3.up * 2f;
            yield return TravelBigCellVisual(start, center);

            int bigCellHitCount = ApplyAreaSkill(
                center,
                archerSkill2.bigCellImpactRadius,
                enemy => enemy.TakeDamage(archerSkill2.bigCellDamage));

            SpawnSkillEffect(
                archerSkill2.bigCellImpactEffectPrefab,
                center,
                archerSkill2.bigCellImpactEffectLifetime,
                1.2f,
                new Color(1f, 0.35f, 0.35f, 0.45f));

            if (enableDebugLogs)
            {
                Debug.Log($"Archer Skill R big cell hit {bigCellHitCount} enemies");
            }

            int rainCount = Mathf.Max(1, archerSkill2.rainProjectileCount);
            float spawnInterval = Mathf.Max(0.01f, archerSkill2.rainSpawnInterval);
            for (int i = 0; i < rainCount; i++)
            {
                Vector2 random = Random.insideUnitCircle * Mathf.Max(0f, archerSkill2.rainRadius);
                Vector3 target = center + new Vector3(random.x, 0f, random.y);
                Vector3 spawn = target + Vector3.up * Mathf.Max(0.5f, archerSkill2.rainSpawnHeight);
                Vector3 direction = (target - spawn).normalized;

                SpawnSkillProjectile(
                    archerSkill2.rainProjectilePrefab,
                    archerSkill2.rainProjectileScale,
                    archerSkill2.rainDamage,
                    archerSkill2.rainProjectileSpeed,
                    archerSkill2.rainProjectileLifeTime,
                    direction,
                    default,
                    spawn);

                if (i < rainCount - 1)
                {
                    yield return new WaitForSeconds(spawnInterval);
                }
            }

            Debug.Log($"Archer Skill R spawned {rainCount} rain projectiles");
            archerSkill2Running = false;
        }

        private Vector3 ResolveArcherSkill2Center()
        {
            Vector3 fallbackCenter = GetSkillCenter(archerSkill2.targetForwardOffset);
            if (!autoTargetForwardEnemyForArcherSkill2)
            {
                return fallbackCenter;
            }

            float maxDistance = Mathf.Max(archerSkill2.targetForwardOffset + archerSkill2.rainRadius, 1f);
            if (!TryFindForwardEnemyPoint(maxDistance, archerSkill2AutoTargetAngle, out Vector3 targetPoint))
            {
                return fallbackCenter;
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[Archer Skill R] Auto-target center: {targetPoint}");
            }

            return targetPoint;
        }

        private bool TryFindForwardEnemyPoint(float maxDistance, float maxAngle, out Vector3 point)
        {
            point = Vector3.zero;
            EnsureOverlapBuffer();

            Vector3 forward = GetFacingDirection();
            Vector3 origin = transform.position;
            origin.y += skillHitHeightOffset;

            int count = Physics.OverlapSphereNonAlloc(
                origin,
                Mathf.Max(0.1f, maxDistance),
                overlapBuffer,
                enemyMask,
                QueryTriggerInteraction.Collide);

            EnemyController bestEnemy = null;
            float bestScore = float.NegativeInfinity;
            float halfAngle = Mathf.Max(1f, maxAngle) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                EnemyController enemy = collider.GetComponentInParent<EnemyController>();
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - transform.position;
                toEnemy.y = 0f;
                float distance = toEnemy.magnitude;
                if (distance <= 0.01f || distance > maxDistance)
                {
                    continue;
                }

                Vector3 toEnemyDir = toEnemy / distance;
                float angle = Vector3.Angle(forward, toEnemyDir);
                if (angle > halfAngle)
                {
                    continue;
                }

                float forwardScore = Vector3.Dot(forward, toEnemyDir) * 2f;
                float distanceScore = -Mathf.Abs(distance - archerSkill2.targetForwardOffset) * 0.15f;
                float score = forwardScore + distanceScore;
                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestEnemy = enemy;
            }

            if (bestEnemy == null)
            {
                return false;
            }

            point = bestEnemy.transform.position;
            point.y = transform.position.y + skillVerticalOffset;
            return true;
        }

        private IEnumerator TravelBigCellVisual(Vector3 start, Vector3 end)
        {
            GameObject visual = CreateVisualObject(
                archerSkill2.bigCellVisualPrefab,
                archerSkill2.bigCellVisualScale,
                new Color(1f, 0.7f, 0.25f, 0.7f));
            if (visual == null)
            {
                yield break;
            }

            visual.transform.position = start;

            float distance = Vector3.Distance(start, end);
            float speed = Mathf.Max(0.1f, archerSkill2.bigCellTravelSpeed);
            float duration = Mathf.Max(0.05f, distance / speed);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                visual.transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            Destroy(visual);
        }

        private int ApplyAreaSkill(Vector3 center, float radius, System.Action<EnemyController> apply)
        {
            EnsureOverlapBuffer();
            uniqueEnemies.Clear();
            float safeRadius = Mathf.Max(0f, radius);

            Vector3 hitCenter = center;
            hitCenter.y += skillHitHeightOffset;
            float halfHeight = Mathf.Max(0.05f, skillHitVerticalHalfHeight);
            Vector3 capsuleTop = hitCenter + Vector3.up * halfHeight;
            Vector3 capsuleBottom = hitCenter - Vector3.up * halfHeight;

            int count = Physics.OverlapCapsuleNonAlloc(
                capsuleTop,
                capsuleBottom,
                safeRadius,
                overlapBuffer,
                enemyMask,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                if (!TryGetEnemyFromCollider(collider, out EnemyController enemy))
                {
                    continue;
                }

                if (!uniqueEnemies.Add(enemy))
                {
                    continue;
                }

                apply?.Invoke(enemy);
            }

            // Fallback: if colliders are missing or overlap filtering misses enemies,
            // run a lightweight distance check against active EnemyController instances.
            if (uniqueEnemies.Count == 0)
            {
                int fallbackHits = ApplyAreaSkillDistanceFallback(center, safeRadius, apply);
                if (enableDebugLogs && fallbackHits > 0)
                {
                    Debug.Log($"[SkillHit] Overlap fallback applied {fallbackHits} enemies.");
                }
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[SkillHit] Center={center}, HitCenter={hitCenter}, Radius={safeRadius:0.##}, OverlapCount={count}, UniqueHit={uniqueEnemies.Count}");
            }

            return uniqueEnemies.Count;
        }

        private int ApplyAreaSkillDistanceFallback(Vector3 center, float radius, System.Action<EnemyController> apply)
        {
            EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            if (enemies == null || enemies.Length == 0)
            {
                return 0;
            }

            int hitCount = 0;
            float radiusSqr = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);

            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 enemyPos = enemy.transform.position;
                enemyPos.y = center.y;
                if ((enemyPos - center).sqrMagnitude > radiusSqr)
                {
                    continue;
                }

                if (!uniqueEnemies.Add(enemy))
                {
                    continue;
                }

                apply?.Invoke(enemy);
                hitCount++;
            }

            return hitCount;
        }

        private static bool TryGetEnemyFromCollider(Collider collider, out EnemyController enemy)
        {
            enemy = null;
            if (collider == null)
            {
                return false;
            }

            enemy = collider.GetComponent<EnemyController>();
            if (enemy == null)
            {
                enemy = collider.GetComponentInParent<EnemyController>();
            }

            return enemy != null && !enemy.IsDead;
        }

        private static EnemyStatusEffectController EnsureStatusController(EnemyController enemy)
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

        private void SpawnSkillProjectile(
            GameObject prefab,
            float fallbackScale,
            float damage,
            float speed,
            float lifeTime,
            Vector3 direction,
            SkillProjectileDebuff debuff)
        {
            SpawnSkillProjectile(prefab, fallbackScale, damage, speed, lifeTime, direction, debuff, null);
        }

        private void SpawnSkillProjectile(
            GameObject prefab,
            float fallbackScale,
            float damage,
            float speed,
            float lifeTime,
            Vector3 direction,
            SkillProjectileDebuff debuff,
            Vector3? worldSpawnPosition)
        {
            Vector3 spawnPosition = worldSpawnPosition ?? GetProjectileSpawnPosition(direction);
            GameObject projectileObject = CreateProjectileObject(prefab, spawnPosition, fallbackScale);
            if (projectileObject == null)
            {
                return;
            }

            SkillProjectile projectile = projectileObject.GetComponent<SkillProjectile>();
            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<SkillProjectile>();
            }

            projectile.Launch(direction, damage, speed, lifeTime, enemyMask, true, debuff);
        }

        private GameObject CreateProjectileObject(GameObject prefab, Vector3 position, float fallbackScale)
        {
            GameObject projectileObject = prefab != null
                ? Instantiate(prefab, position, Quaternion.identity)
                : CreateVisualObject(null, fallbackScale, new Color(0.75f, 1f, 0.65f, 0.9f));

            if (projectileObject == null)
            {
                return null;
            }

            EnsureProjectilePhysics(projectileObject);
            projectileObject.transform.position = position;
            return projectileObject;
        }

        private void EnsureProjectilePhysics(GameObject projectileObject)
        {
            Collider collider = projectileObject.GetComponent<Collider>();
            if (collider == null)
            {
                collider = projectileObject.AddComponent<SphereCollider>();
            }

            collider.isTrigger = true;

            Rigidbody body = projectileObject.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = projectileObject.AddComponent<Rigidbody>();
            }

            body.useGravity = false;
            body.isKinematic = true;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        private Vector3 GetProjectileSpawnPosition(Vector3 direction)
        {
            Vector3 basePos = skillSpawnPoint != null ? skillSpawnPoint.position : transform.position;
            basePos += direction.normalized * projectileSpawnOffset;
            basePos.y += projectileSpawnHeight + skillVerticalOffset;
            return basePos;
        }

        private Vector3 GetSkillCenter(float forwardOffset)
        {
            Vector3 center = transform.position + GetFacingDirection() * forwardOffset;
            center.y = transform.position.y + skillVerticalOffset;
            return center;
        }

        private Vector3 GetFacingDirection()
        {
            if (playerController == null)
            {
                return Vector3.forward;
            }

            Vector3 direction = playerController.GetLogicalFacingDirection();
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        private void ResolveSkillSpawnPoint()
        {
            if (IsValidRootSpawnPoint(skillSpawnPoint))
            {
                return;
            }

            Transform found = transform.Find("SkillSpawnPoint");
            if (!IsValidRootSpawnPoint(found))
            {
                found = transform.Find("FirePoint");
            }

            if (IsValidRootSpawnPoint(found))
            {
                skillSpawnPoint = found;
                return;
            }

            GameObject pointObject = new GameObject("SkillSpawnPoint");
            skillSpawnPoint = pointObject.transform;
            skillSpawnPoint.SetParent(transform, false);
            skillSpawnPoint.localPosition = Vector3.zero;
            skillSpawnPoint.localRotation = Quaternion.identity;
        }

        private bool IsValidRootSpawnPoint(Transform point)
        {
            if (point == null || !point.IsChildOf(transform))
            {
                return false;
            }

            Transform cursor = point;
            while (cursor != null && cursor != transform)
            {
                if (cursor.GetComponent<SpriteRenderer>() != null)
                {
                    return false;
                }

                cursor = cursor.parent;
            }

            return true;
        }

        private void SpawnSkillEffect(GameObject prefab, Vector3 position, float lifeTime, float fallbackScale, Color fallbackColor)
        {
            if (prefab != null)
            {
                GameObject effect = Instantiate(prefab, position, Quaternion.identity);
                Destroy(effect, Mathf.Max(0.1f, lifeTime));
                return;
            }

            GameObject fallback = CreateVisualObject(null, fallbackScale, fallbackColor);
            if (fallback == null)
            {
                return;
            }

            fallback.transform.position = position;
            Destroy(fallback, Mathf.Max(0.1f, lifeTime));
        }

        private static GameObject CreateVisualObject(GameObject prefab, float fallbackScale, Color fallbackColor)
        {
            if (prefab != null)
            {
                return Instantiate(prefab);
            }

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Collider col = sphere.GetComponent<Collider>();
            if (col != null)
            {
                Destroy(col);
            }

            sphere.transform.localScale = Vector3.one * Mathf.Max(0.05f, fallbackScale);
            Renderer renderer = sphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                if (shader != null)
                {
                    Material material = new Material(shader)
                    {
                        color = fallbackColor
                    };
                    renderer.material = material;
                }
            }

            return sphere;
        }

        private void EnsureOverlapBuffer()
        {
            int desiredSize = Mathf.Max(1, overlapBufferSize);
            if (overlapBuffer != null && overlapBuffer.Length == desiredSize)
            {
                return;
            }

            overlapBuffer = new Collider[desiredSize];
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.identity;

            switch (currentClass)
            {
                case PlayerClassType.Mage:
                    DrawSkillRadius(mageSkill1.forwardOffset, mageSkill1.radius, new Color(0.3f, 0.9f, 1f, 0.4f));
                    DrawSkillRadius(mageSkill2.forwardOffset, mageSkill2.radius, new Color(1f, 0.5f, 0.2f, 0.4f));
                    break;

                case PlayerClassType.Archer:
                    DrawSkillRadius(archerSkill2.targetForwardOffset, archerSkill2.rainRadius, new Color(0.4f, 1f, 0.4f, 0.4f));
                    break;
            }
        }

        private void DrawSkillRadius(float forwardOffset, float radius, Color color)
        {
            Vector3 direction = Application.isPlaying ? GetFacingDirection() : Vector3.forward;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.forward;
            }

            Vector3 center = transform.position + direction.normalized * forwardOffset;
            center.y = transform.position.y + skillVerticalOffset;

            Gizmos.color = color;
            Gizmos.DrawWireSphere(center, Mathf.Max(0f, radius));
        }
    }
}
