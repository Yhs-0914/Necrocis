using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// FSM 기반 적 AI + 오브젝트 풀링.
    /// 배회(Wander) / 추격(Chase) / 복귀(Return) / 공격(Attack) / 사망(Dead)
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        private const string PoolRootName = "__EnemyPool";
        private const float SpatialHashCellSize = 2f;
        private const float NormalizedEnemyAttackDamage = 1f;

        private static readonly List<EnemyController> ActiveEnemies = new List<EnemyController>();              // 현재 활성 적 목록 (분리 벡터 계산용)
        private static readonly Dictionary<int, Stack<EnemyController>> PooledEnemies = new Dictionary<int, Stack<EnemyController>>(); // 타입별 오브젝트 풀
        private static readonly Dictionary<Vector2Int, List<EnemyController>> ActiveEnemyCells = new Dictionary<Vector2Int, List<EnemyController>>();
        private static Transform poolRoot; // 비활성 적을 보관할 부모 Transform

        // 소유자/설정
        private EnemySpawner owner;              // 이 적을 생성한 스포너 (사망 통보용)
        private EnemySpawnRuleConfig config;      // 적 설정 데이터 (속도, 체력, 감지범위 등)
        private int poolArchetypeId;              // 풀 분류 ID (같은 타입끼리 재사용)

        // 컴포넌트
        private Transform playerTransform;        // 플레이어 Transform 캐시
        private Transform visualRoot;             // 스프라이트가 붙는 자식 오브젝트
        private SpriteRenderer spriteRenderer;    // 스프라이트 렌더러
        private AnimatedSprite animatedSprite;    // 프레임 애니메이션 재생기
        private Billboard billboard;              // 카메라를 향해 회전
        private SpriteYSort ySort;                // Y좌표 기반 정렬
        private Rigidbody body;                   // 물리 (키네마틱)
        private BoxCollider boxCollider;          // 충돌 판정
        private CharacterStats stats;             // 체력/공격력 등 스탯 컨테이너
        private EnemyStatusEffectController statusEffectController;
        private EnemySkillBridge enemySkillBridge;

        // 이동
        private Vector3 anchorPosition;  // 스폰 기준점 (leash/wander 중심)
        private Vector3 destination;     // 현재 이동 목적지
        private bool hasDestination;     // 목적지 설정 여부

        // FSM
        private IEnemyState currentState; // 현재 상태 (Idle/Wander/Chase/Attack/Return/Dead)

        // 타이머
        private float idleTimer;   // Idle 상태 대기 타이머
        private float attackTimer; // 공격 쿨타임 타이머

        // 플래그
        private bool usingMoveAnimation; // 이동 애니메이션 사용 중
        private bool notifiedOwner;      // 소유 스포너에 해제 통보 완료
        private bool attackAnimPlaying;  // 공격 애니메이션 재생 중
        private bool deathAnimPlaying;   // 사망 애니메이션 재생 중
        private bool colliderExpanded;   // 공격 콜라이더 확장 상태

        // 돌진 (항체 엘리트)
        private Vector3 chargeDirection;  // 돌진 방향 (고정)
        private float chargeElapsed;      // 돌진 경과 시간
        private float chargeCurrentSpeed; // 현재 돌진 속도
        private bool isCharging;          // 돌진 중 여부
        private float chargeCooldownTimer; // 돌진 쿨타임 타이머

        // 어그로 부스트 (잔해 효과)
        private float originalChaseRadius; // 원래 chaseRadius (부스트 해제용)
        private bool hasAggroBoost;        // 어그로 부스트 적용 여부
        private bool defeatEventRaised;
        private bool ignoreMidBossArenaRestriction;
        private bool hasCachedGroundHeight;
        private float cachedGroundHeight;
        private Vector2Int cachedGroundGrid;
        private bool isRegisteredInSpatialHash;
        private Vector2Int currentSpatialCell;

        // ─────────────────────────────────
        // 공개 프로퍼티 (FSM 상태에서 사용)
        // ─────────────────────────────────

        public bool IsDead => stats != null && stats.IsDead;
        public EnemySpawnRuleConfig Config => config;
        public CharacterStats Stats => stats;
        public EnemyStatusEffectController StatusEffects => statusEffectController;
        public static IReadOnlyList<EnemyController> ActiveEnemyControllers => ActiveEnemies;
        public bool IsAttackAnimPlaying => attackAnimPlaying;
        public bool IsDeathAnimPlaying => deathAnimPlaying;
        public bool IsStationary => config != null && config.isRanged;
        public bool IsElite => config != null && config.isElite;
        public bool IsCharger => config != null && config.chargesAtPlayer;
        public bool IsCharging => isCharging;
        public bool CanCharge => IsCharger && chargeCooldownTimer <= 0f;
        public event System.Action<EnemyController> Defeated;

        // ─────────────────────────────────
        // 풀링 API (기존 유지)
        // ─────────────────────────────────

        // 풀에서 적을 꺼내거나 새로 생성 (오브젝트 풀링)
        public static EnemyController Acquire(Transform parent, string name, int poolArchetypeId)
        {
            EnsurePoolRoot();
            Stack<EnemyController> pool = GetOrCreatePool(poolArchetypeId);

            while (pool.Count > 0)
            {
                EnemyController pooled = pool.Pop();
                if (pooled == null) continue;

                GameObject pooledObject = pooled.gameObject;
                pooledObject.name = name;
                pooled.poolArchetypeId = poolArchetypeId;
                pooled.transform.SetParent(parent, false);
                pooled.transform.localPosition = Vector3.zero;
                pooled.transform.localRotation = Quaternion.identity;
                pooled.transform.localScale = Vector3.one;
                return pooled;
            }

            GameObject enemyObject = new GameObject(name);
            enemyObject.transform.SetParent(parent, false);
            EnemyController controller = enemyObject.AddComponent<EnemyController>();
            controller.poolArchetypeId = poolArchetypeId;
            return controller;
        }
        // Configure: 관련 설정과 상태를 구성합니다.

        // 적 초기 설정: 스폰 위치, 스탯, 물리, 비주얼, FSM 시작
        public void Configure(EnemySpawner owner, EnemySpawnRuleConfig config, Vector3 anchorPosition, Vector3 spawnPosition)
        {
            this.owner = owner;
            this.config = config;
            poolArchetypeId = GetPoolArchetypeId(config);
            this.anchorPosition = anchorPosition;
            playerTransform = null;
            destination = spawnPosition;
            idleTimer = 0f;
            attackTimer = 0f;
            hasDestination = false;
            usingMoveAnimation = false;
            notifiedOwner = false;
            attackAnimPlaying = false;
            deathAnimPlaying = false;
            colliderExpanded = false;
            isCharging = false;
            chargeElapsed = 0f;
            chargeCurrentSpeed = 0f;
            chargeCooldownTimer = 0f;
            hasAggroBoost = false;
            defeatEventRaised = false;
            ignoreMidBossArenaRestriction = false;
            hasCachedGroundHeight = false;

            transform.position = spawnPosition;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            EnsureComponents();
            statusEffectController?.Initialize(this);
            statusEffectController?.ResetEffects();
            enemySkillBridge?.Bind(this, statusEffectController);
            gameObject.tag = "Enemy";
            ConfigureStats();
            ApplyPhysicsSetup();
            ApplyVisualSetup();
            SetIdleAnimation();
            SyncHeight();

            if (!ActiveEnemies.Contains(this))
            {
                ActiveEnemies.Add(this);
            }
            RegisterOrUpdateSpatialCell(GetCurrentPosition());

            enabled = config != null;
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            // FSM 시작 → Idle
            currentState = null;
            ChangeState(EnemyIdleState.Instance);
        }
        // ReleaseToPool: 상태 전환 관련 흐름을 처리합니다.

        // 오브젝트 풀로 반환: FSM 종료 → 비활성화 → 풀에 Push
        public void ReleaseToPool()
        {
            if (gameObject == null || !gameObject.activeSelf) return;

            // FSM Exit
            currentState?.Exit(this);
            currentState = null;

            PrepareForPool();
            statusEffectController?.ResetEffects();
            EnsurePoolRoot();
            gameObject.SetActive(false);
            transform.SetParent(poolRoot, false);

            owner = null;
            config = null;
            playerTransform = null;
            destination = Vector3.zero;
            idleTimer = 0f;
            attackTimer = 0f;
            hasDestination = false;
            usingMoveAnimation = false;
            attackAnimPlaying = false;
            deathAnimPlaying = false;
            colliderExpanded = false;
            isCharging = false;
            hasAggroBoost = false;
            ignoreMidBossArenaRestriction = false;
            hasCachedGroundHeight = false;

            GetOrCreatePool(poolArchetypeId).Push(this);
        }

        // ─────────────────────────────────
        // Unity 라이프사이클
        // ─────────────────────────────────

        private void Update()
        {
            if (config == null) return;

            EnsurePlayerTransform();

            // 돌진 쿨타임 감소
            if (chargeCooldownTimer > 0f)
                chargeCooldownTimer -= Time.deltaTime;

            // FSM Update
            currentState?.Update(this, Time.deltaTime);

            SyncHeight();
        }
        // 유니티 콜백: OnDisable 이벤트에 반응합니다.

        private void OnDisable()
        {
            UnregisterSpatialCell();
            ActiveEnemies.Remove(this);
            NotifyOwnerReleased();
        }
        // 유니티 콜백: OnDestroy 이벤트에 반응합니다.

        private void OnDestroy()
        {
            UnregisterSpatialCell();
            ActiveEnemies.Remove(this);
            NotifyOwnerReleased();
        }

        // ─────────────────────────────────
        // FSM 상태 전환
        // ─────────────────────────────────

        public void ChangeState(IEnemyState newState)
        {
            if (newState == currentState) return;

            currentState?.Exit(this);
            currentState = newState;
            currentState?.Enter(this);
        }

        // ─────────────────────────────────
        // 조건 검사 (FSM 상태에서 호출)
        // ─────────────────────────────────

        public bool IsPlayerInChaseRange()
        {
            if (playerTransform == null) return false;
            if (!ignoreMidBossArenaRestriction && MidBossArenaController.IsPlayerInsideLockedArena(playerTransform.position))
            {
                return false;
            }

            float distToPlayer = GetPlanarDistance(GetCurrentPosition(), playerTransform.position);
            if (distToPlayer > config.chaseRadius) return false;

            // leash 안에 있는 플레이어만 추격
            float playerToAnchor = GetPlanarDistance(playerTransform.position, anchorPosition);
            return playerToAnchor <= config.leashRadius;
        }
        // IsPlayerInAttackRange: 조건 충족 여부를 확인합니다.

        public bool IsPlayerInAttackRange()
        {
            if (playerTransform == null) return false;
            if (!ignoreMidBossArenaRestriction && MidBossArenaController.IsPlayerInsideLockedArena(playerTransform.position))
            {
                return false;
            }
            float dist = GetPlanarDistance(GetCurrentPosition(), playerTransform.position);
            return dist <= config.attackRange;
        }
        // IsOutOfLeash: 조건 충족 여부를 확인합니다.

        public bool IsOutOfLeash()
        {
            return GetPlanarDistance(GetCurrentPosition(), anchorPosition) > config.leashRadius;
        }
        // IsIdleTimerExpired: 조건 충족 여부를 확인합니다.

        public bool IsIdleTimerExpired(float deltaTime)
        {
            idleTimer -= deltaTime;
            return idleTimer <= 0f;
        }

        // ─────────────────────────────────
        // 행동 (FSM 상태에서 호출)
        // ─────────────────────────────────

        public void ResetIdleTimer()
        {
            float min = Mathf.Min(config.idleDelayRange.x, config.idleDelayRange.y);
            float max = Mathf.Max(config.idleDelayRange.x, config.idleDelayRange.y);
            idleTimer = Random.Range(min, max);
        }
        // PickWanderDestination: 이 컴포넌트의 핵심 로직을 실행합니다.

        public void PickWanderDestination()
        {
            if (TryPickWanderDestination(out Vector3 wanderDest))
            {
                SetDestination(wanderDest);
            }
        }
        // SetChaseDestination: 관련 설정과 상태를 구성합니다.

        public void SetChaseDestination()
        {
            if (playerTransform == null) return;
            Vector3 chase = playerTransform.position;
            chase.y = GetCurrentPosition().y;
            SetDestination(chase);
        }
        // SetReturnDestination: 관련 설정과 상태를 구성합니다.

        public void SetReturnDestination()
        {
            SetDestination(anchorPosition);
        }

        /// <summary>
        /// 목적지로 이동. 도착하면 false 반환.
        /// </summary>
        public bool MoveTowardDestination(float deltaTime)
        {
            if (statusEffectController != null && statusEffectController.IsStunned)
            {
                if (usingMoveAnimation)
                {
                    SetIdleAnimation();
                }
                return false;
            }

            if (!hasDestination) return false;

            Vector3 currentPosition = GetCurrentPosition();
            Vector3 separation = GetSeparationVector(currentPosition);

            Vector3 flatCurrent = new Vector3(currentPosition.x, 0f, currentPosition.z);
            Vector3 flatDestination = new Vector3(destination.x, 0f, destination.z);
            Vector3 toDestination = flatDestination - flatCurrent;
            float distance = toDestination.magnitude;

            if (distance <= Mathf.Max(0.01f, config.stoppingDistance))
            {
                hasDestination = false;
                return false; // 도착
            }

            Vector3 moveDirection = toDestination.normalized;
            if (separation.sqrMagnitude > 0.0001f)
            {
                Vector3 combined = moveDirection + separation * config.separationStrength;
                if (combined.sqrMagnitude > 0.0001f)
                {
                    moveDirection = combined.normalized;
                }
            }

            Vector3 step = moveDirection * (stats != null ? stats.MoveSpeed : 0f) * deltaTime;
            if (step.sqrMagnitude > toDestination.sqrMagnitude)
            {
                step = toDestination;
            }

            bool moved = TryMove(currentPosition, step);
            if (!moved)
            {
                hasDestination = false;
                return false;
            }

            if (spriteRenderer != null && Mathf.Abs(step.x) > 0.001f)
            {
                spriteRenderer.flipX = step.x < 0f;
            }

            // 이동 애니메이션 전환
            if (!usingMoveAnimation)
            {
                SetMoveAnimation();
            }

            return true; // 아직 이동 중
        }
        // TryPerformAttack: 작업을 시도하고 성공 여부를 반환합니다.

        /// <summary>
        /// 쿨타임 기반 공격. 쿨타임 만료 시 공격 애니메이션 시작 → 애니메이션 완료 시 데미지 적용.
        /// 반환값: true면 공격 애니메이션 시작됨 (쿨타임 대기 중이면 false).
        /// </summary>
        public bool TryPerformAttack(float deltaTime)
        {
            if (statusEffectController != null && statusEffectController.IsStunned)
            {
                return false;
            }

            // 공격 애니메이션 재생 중이면 대기
            if (attackAnimPlaying) return false;

            attackTimer -= deltaTime;
            if (attackTimer > 0f) return false;

            // 공격 애니메이션이 있으면 재생 → 완료 시 데미지
            Sprite[] attackFrames = GetAttackFrames();
            if (attackFrames != null && attackFrames.Length > 0)
            {
                attackAnimPlaying = true;
                usingMoveAnimation = false;

                // 대식세포 등: 공격 시 콜라이더 확장
                if (config.expandColliderOnAttack)
                {
                    ExpandAttackCollider();
                }

                // NK세포: 방향별 공격 스프라이트가 있으면 flipX 설정
                bool hasDirectional = config.attackSpritesUp != null && config.attackSpritesUp.Length > 0;
                if (hasDirectional && spriteRenderer != null)
                {
                    int dir = GetAttackDirection();
                    spriteRenderer.flipX = (dir == 2); // 2 = left (우 스프라이트를 좌우 반전)
                }

                animatedSprite.enabled = true;
                animatedSprite.PlayOneShot(attackFrames, config.attackAnimationSpeed, OnAttackAnimationComplete);
                return true;
            }

            // 공격 애니메이션이 없으면 즉시 데미지 (기존 동작)
            ApplyDamageToPlayer();
            attackTimer = config.attackCooldown;
            return false;
        }
        // TakeDamage: 이 컴포넌트의 핵심 로직을 실행합니다.

        private void OnAttackAnimationComplete()
        {
            attackAnimPlaying = false;
            attackTimer = config.attackCooldown;

            // 콜라이더 복원
            if (colliderExpanded)
            {
                RestoreCollider();
            }

            // 공격 범위 내 플레이어에게 데미지
            ApplyDamageToPlayer();

            // 대기 애니메이션으로 복귀
            SetIdleAnimation();
        }

        private void ApplyDamageToPlayer()
        {
            if (PlayerController.Instance == null) return;

            float damage = NormalizedEnemyAttackDamage;

            if (config.isRanged)
            {
                LaunchProjectile(damage);
                return;
            }

            Health playerHealth = PlayerController.Instance.GetComponent<Health>();
            if (playerHealth == null) return;

            playerHealth.TakeDamage(damage);
        }

        private void LaunchProjectile(float damage)
        {
            Sprite projSprite = config.projectileSprite; // null이면 EnemyProjectile에서 기본 구체 사용

            Vector3 spawnPos = GetCurrentPosition();
            Vector3 toPlayer = PlayerController.Instance.transform.position - spawnPos;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.0001f) return;

            Vector3 dir = toPlayer.normalized;
            spawnPos += dir * config.projectileSpawnOffset;
            spawnPos.y += 2f;

            // 스프라이트 방향 업데이트
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = dir.x < 0f;
            }

            EnemyProjectile proj = EnemyProjectile.Acquire(spawnPos, projSprite, config.projectileScale);
            proj.Launch(dir, damage, config.projectileSpeed, config.projectileLifeTime);
        }

        // 데미지 처리: CharacterStats에 적용 → HP 0이면 Dead 상태로 전환
        public void TakeDamage(float damage)
        {
            if (IsDead) return;
            if (stats == null)
            {
                return;
            }

            float incomingDamageMultiplier = statusEffectController != null
                ? statusEffectController.GetIncomingDamageMultiplier()
                : 1f;
            float finalDamage = Mathf.Max(0f, damage * incomingDamageMultiplier);
            if (finalDamage <= 0f)
            {
                return;
            }

            stats.ApplyDamage(finalDamage);
            if (stats.IsDead)
            {
                RaiseDefeated();
                ChangeState(EnemyDeadState.Instance);
            }
        }
        // GrantExp: 이 컴포넌트의 핵심 로직을 실행합니다.

        // 사망 시 플레이어에게 경험치 부여 + 킬 카운트 알림
        public void GrantExp()
        {
            if (config == null) return;
            LevelUpManager.AddEnemyKillExp();

            // 엘리트 스포너에 킬 알림
            if (EliteSpawner.Instance != null && !config.isElite)
            {
                EliteSpawner.Instance.NotifyEnemyKilled(config.name);
            }
        }
        // ApplyKnockback: 변경 사항을 런타임 객체에 반영합니다.

        // 넉백: 지정 방향으로 밀어냄 (공격 적중 시 호출)
        public void ApplyKnockback(Vector3 worldDirection, float distance)
        {
            if (IsDead || distance <= 0f) return;

            Vector3 planarDirection = new Vector3(worldDirection.x, 0f, worldDirection.z);
            if (planarDirection.sqrMagnitude <= 0.0001f) return;

            planarDirection.Normalize();
            Vector3 displacement = planarDirection * distance;
            Vector3 currentPosition = GetCurrentPosition();
            bool moved = TryMove(currentPosition, displacement);
            if (moved)
            {
                hasDestination = false;
            }
        }
        // DisableCollider: 이 컴포넌트의 핵심 로직을 실행합니다.

        public void UpdateFacingDirection()
        {
            if (spriteRenderer == null || playerTransform == null) return;
            float dx = playerTransform.position.x - GetCurrentPosition().x;
            if (Mathf.Abs(dx) > 0.01f)
            {
                spriteRenderer.flipX = dx < 0f;
            }
        }

        public void DisableCollider()
        {
            if (boxCollider != null) boxCollider.enabled = false;
        }

        public void SetIgnoreMidBossArenaRestriction(bool ignore)
        {
            ignoreMidBossArenaRestriction = ignore;
        }

        /// <summary>
        /// 공격 시 콜라이더 확장 (대식세포: 가시 펼침)
        /// </summary>
        public void ExpandAttackCollider()
        {
            if (boxCollider == null || config == null || colliderExpanded) return;
            boxCollider.size = config.attackColliderSize;
            boxCollider.center = config.attackColliderCenter;
            colliderExpanded = true;
        }

        /// <summary>
        /// 콜라이더를 원래 크기로 복원
        /// </summary>
        public void RestoreCollider()
        {
            if (boxCollider == null || config == null || !colliderExpanded) return;
            boxCollider.size = config.colliderSize;
            boxCollider.center = config.colliderCenter;
            colliderExpanded = false;
        }

        /// <summary>
        /// 사망 애니메이션 재생. 완료 시 onComplete 콜백.
        /// </summary>
        public void PlayDeathAnimation(System.Action onComplete)
        {
            Sprite[] deathFrames = config != null ? config.deathSprites : null;
            if (deathFrames == null || deathFrames.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            deathAnimPlaying = true;
            usingMoveAnimation = false;
            animatedSprite.enabled = true;
            animatedSprite.PlayOneShot(deathFrames, config.deathAnimationSpeed, () =>
            {
                deathAnimPlaying = false;
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// 공격 방향에 맞는 스프라이트 프레임 반환.
        /// NK세포: 상/하/좌우 방향별, 대식세포: 단일 공격 스프라이트.
        /// </summary>
        private Sprite[] GetAttackFrames()
        {
            if (config == null) return null;

            // 방향별 공격 스프라이트가 없으면 기본 attackSprites 사용
            bool hasDirectional = config.attackSpritesUp != null && config.attackSpritesUp.Length > 0;
            if (!hasDirectional)
            {
                return config.attackSprites;
            }

            // 방향별 공격 (NK세포)
            int dir = GetAttackDirection();
            switch (dir)
            {
                case 0: return config.attackSpritesUp;                        // 상
                case 1: return config.attackSprites;                          // 우
                case 2: return config.attackSprites;                          // 좌 (flipX로 처리)
                case 3: return config.attackSpritesDown;                      // 하
                default: return config.attackSprites;
            }
        }

        /// <summary>
        /// 플레이어 방향 감지: 0=상, 1=우, 2=좌, 3=하
        /// 2.5D 쿼터뷰: X=좌우, Z=상하(깊이)
        /// </summary>
        private int GetAttackDirection()
        {
            if (playerTransform == null) return 1;

            Vector3 toPlayer = playerTransform.position - GetCurrentPosition();
            toPlayer.y = 0f;

            if (Mathf.Abs(toPlayer.x) >= Mathf.Abs(toPlayer.z))
            {
                return toPlayer.x >= 0f ? 1 : 2; // 우 / 좌
            }
            return toPlayer.z >= 0f ? 0 : 3; // 상 / 하
        }

        /// <summary>
        /// 공격 애니메이션 강제 중단 (상태 전환 시)
        /// </summary>
        public void CancelAttackAnimation()
        {
            if (!attackAnimPlaying) return;
            attackAnimPlaying = false;
            if (colliderExpanded) RestoreCollider();
            SetIdleAnimation();
        }

        // ─────────────────────────────────
        // 돌진 (항체 엘리트)
        // ─────────────────────────────────

        /// <summary>
        /// 돌진 시작: 플레이어 방향 고정, 가속 시작
        /// </summary>
        public void StartCharge()
        {
            if (playerTransform == null || config == null) return;

            Vector3 toPlayer = playerTransform.position - GetCurrentPosition();
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.0001f) return;

            chargeDirection = toPlayer.normalized;
            chargeElapsed = 0f;
            chargeCurrentSpeed = 0f;
            isCharging = true;

            // 방향에 따라 스프라이트 반전
            if (spriteRenderer != null)
                spriteRenderer.flipX = chargeDirection.x < 0f;
        }

        /// <summary>
        /// 돌진 업데이트: 가속 후 일정 속도로 이동. 반환값: true면 돌진 지속 중
        /// </summary>
        public bool UpdateCharge(float deltaTime)
        {
            if (!isCharging || config == null) return false;

            chargeElapsed += deltaTime;

            // 가속 단계
            float accelTime = config.chargeAccelTime;
            if (chargeElapsed < accelTime)
            {
                chargeCurrentSpeed = Mathf.Lerp(0f, config.chargeSpeed, chargeElapsed / accelTime);
            }
            else
            {
                chargeCurrentSpeed = config.chargeSpeed;
            }

            // 이동
            Vector3 step = chargeDirection * chargeCurrentSpeed * deltaTime;
            Vector3 currentPos = GetCurrentPosition();
            bool moved = TryMove(currentPos, step);

            if (!moved)
            {
                isCharging = false;
                return false;
            }

            // 플레이어를 지나쳤는지 체크 (플레이어와의 거리가 다시 멀어지기 시작하면 종료)
            if (playerTransform != null && chargeElapsed > accelTime)
            {
                Vector3 toPlayer = playerTransform.position - GetCurrentPosition();
                toPlayer.y = 0f;
                // 돌진 방향과 플레이어 방향이 반대면 지나친 것
                if (Vector3.Dot(chargeDirection, toPlayer.normalized) < 0f)
                {
                    isCharging = false;
                    return false;
                }
            }

            // 최대 돌진 시간 (1.5초)
            if (chargeElapsed > 1.5f)
            {
                isCharging = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 돌진 종료 → 쿨타임 시작 (3초)
        /// </summary>
        public void EndCharge()
        {
            isCharging = false;
            chargeCurrentSpeed = 0f;
            chargeCooldownTimer = 3f;
        }

        // ─────────────────────────────────
        // 어그로 부스트 (항체 잔해 효과)
        // ─────────────────────────────────

        /// <summary>
        /// chaseRadius를 percentBoost만큼 증가 (0.5 = 50%)
        /// </summary>
        public void ApplyAggroBoost(float percentBoost)
        {
            if (config == null || hasAggroBoost) return;
            originalChaseRadius = config.chaseRadius;
            config.chaseRadius *= (1f + percentBoost);
            hasAggroBoost = true;
        }

        /// <summary>
        /// chaseRadius를 원래 값으로 복원
        /// </summary>
        public void RemoveAggroBoost()
        {
            if (!hasAggroBoost || config == null) return;
            config.chaseRadius = originalChaseRadius;
            hasAggroBoost = false;
        }

        /// <summary>
        /// 강제로 플레이어를 추격 + 이동속도 부스트 (어그로 잔해 효과)
        /// </summary>
        public void ForceChasePlayer()
        {
            if (IsDead) return;

            // 이동속도 3배 버프 (source로 "AggroDebris" 문자열 사용)
            if (stats != null)
            {
                stats.AddModifier(new CharacterStatModifier(
                    CharacterStatType.MoveSpeed,
                    2.0f,
                    CharacterStatModifierMode.PercentMultiply,
                    "AggroDebris"
                ));
            }

            // 공격 쿨타임 리셋 (즉시 공격 가능)
            attackTimer = 0f;

            ChangeState(EnemyChaseState.Instance);
        }

        /// <summary>
        /// 어그로 부스트 + 이동속도 버프 모두 해제
        /// </summary>
        public void RemoveAllAggroEffects()
        {
            RemoveAggroBoost();
            if (stats != null)
            {
                stats.RemoveModifiersFromSource("AggroDebris");
            }
        }

        // ─────────────────────────────────
        // 엘리트 사망 처리
        // ─────────────────────────────────

        /// <summary>
        /// 엘리트 사망 시 특수 효과 실행 (DeadState에서 호출)
        /// </summary>
        public void HandleEliteDeath()
        {
            if (config == null || !config.isElite) return;

            Debug.Log($"[EliteDeath] {config.name} 사망 처리 시작 - splitsOnDeath={config.splitsOnDeath}, leavesDebris={config.leavesDebrisOnDeath}");

            // 육아종: 분열
            if (config.splitsOnDeath)
            {
                SpawnSplitEnemies();
            }

            // 항체: 잔해 생성
            if (config.leavesDebrisOnDeath)
            {
                SpawnAggroDebris();
            }
        }

        private void SpawnSplitEnemies()
        {
            Debug.Log($"[EliteDeath] SpawnSplitEnemies 호출 - splitEnemyName='{config.splitEnemyName}'");

            if (string.IsNullOrEmpty(config.splitEnemyName))
            {
                Debug.LogWarning("[EliteDeath] splitEnemyName이 비어있음! 분열 불가.");
                return;
            }

            EnemySpawnRuleConfig splitConfig = FindEnemyConfigByName(config.splitEnemyName);
            if (splitConfig == null)
            {
                Debug.LogWarning($"[EliteDeath] '{config.splitEnemyName}' 설정을 찾을 수 없음! BiomeConfig에 해당 적이 없음.");
                return;
            }

            Debug.Log($"[EliteDeath] 분열 설정 찾음: {splitConfig.name}, splitCount={config.splitCount}");

            Vector3 deathPos = GetCurrentPosition();

            // VoidShield 이펙트 로드 → 이펙트 재생 후 분열
            Sprite[] vfxSprites = LoadVoidShieldSprites();
            if (vfxSprites != null && vfxSprites.Length > 0)
            {
                Debug.Log($"[EliteDeath] VoidShield 이펙트 재생 시작 ({vfxSprites.Length}프레임)");
                SpawnSplitVfxThenSpawn(deathPos, splitConfig, vfxSprites);
            }
            else
            {
                Debug.Log("[EliteDeath] VoidShield 스프라이트 없음, 즉시 분열");
                DoSpawnSplitEnemies(deathPos, splitConfig);
            }
        }

        /// <summary>
        /// VoidShield 이펙트를 재생한 뒤 분열 적을 소환한다.
        /// config.splitVfxSprites가 있으면 해당 스프라이트 사용, 없으면 프로시저럴 이펙트.
        /// </summary>
        private void SpawnSplitVfxThenSpawn(Vector3 deathPos, EnemySpawnRuleConfig splitConfig, Sprite[] vfxSprites)
        {
            // VFX 오브젝트 생성
            GameObject vfxObj = new GameObject($"SplitVFX_{config.name}");
            vfxObj.transform.position = deathPos + new Vector3(0f, 0.8f, 0f);

            SpriteRenderer vfxRenderer = vfxObj.AddComponent<SpriteRenderer>();
            vfxRenderer.sprite = vfxSprites[0];
            vfxRenderer.sortingOrder = config.sortingOrder + 200;
            vfxRenderer.color = new Color(0.5f, 0.2f, 1f, 0.9f);

            Billboard vfxBb = vfxObj.AddComponent<Billboard>();
            vfxBb.enabled = true;

            vfxObj.transform.localScale = Vector3.one * config.splitVfxScale;

            AnimatedSprite vfxAnim = vfxObj.AddComponent<AnimatedSprite>();
            vfxAnim.enabled = true;

            // 캡처용 로컬 변수
            int splitCount = config.splitCount;
            Transform spawnParent = transform.parent != null ? transform.parent : transform;

            // 이펙트 재생 → 완료 후 분열
            vfxAnim.PlayOneShot(vfxSprites, config.splitVfxSpeed, () =>
            {
                // 이펙트 종료 후 분열 적 소환
                DoSpawnSplitEnemies(deathPos, splitConfig, splitCount, spawnParent);

                // VFX 페이드 아웃 후 제거
                SplitVfxFadeOut fadeOut = vfxObj.AddComponent<SplitVfxFadeOut>();
                fadeOut.Init(vfxRenderer, 0.3f);
            });
        }

        /// <summary>
        /// VoidShield 스프라이트를 로드한다. config에 있으면 그것을, 없으면 Resources에서 로드.
        /// </summary>
        private static Sprite[] cachedVoidShieldSprites;

        private Sprite[] LoadVoidShieldSprites()
        {
            // 1) config에 직접 할당된 스프라이트가 있으면 사용
            if (config.splitVfxSprites != null && config.splitVfxSprites.Length > 0)
                return config.splitVfxSprites;

            // 2) 캐시된 스프라이트가 있으면 재사용
            if (cachedVoidShieldSprites != null && cachedVoidShieldSprites.Length > 0)
                return cachedVoidShieldSprites;

            // 3) Resources에서 VoidShield 스프라이트 로드 시도
            Sprite[] loaded = Resources.LoadAll<Sprite>("VoidShield_Lite");
            if (loaded != null && loaded.Length > 0)
            {
                // 이름순 정렬 (VoidShield_0, 1, 2, ...)
                System.Array.Sort(loaded, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
                cachedVoidShieldSprites = loaded;
                return cachedVoidShieldSprites;
            }

            // 4) 프로시저럴 폴백: 간단한 원형 스프라이트 6프레임 생성
            cachedVoidShieldSprites = GenerateVoidShieldSprites(6, 64);
            return cachedVoidShieldSprites;
        }

        /// <summary>
        /// 프로시저럴 VoidShield 스프라이트 생성 (보라색 원형 쉴드)
        /// </summary>
        private static Sprite[] GenerateVoidShieldSprites(int frameCount, int size)
        {
            Sprite[] sprites = new Sprite[frameCount];
            for (int f = 0; f < frameCount; f++)
            {
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Point;
                float phase = (float)f / frameCount;
                float center = size * 0.5f;
                float maxRadius = size * 0.45f;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x - center;
                        float dy = y - center;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy) / maxRadius;

                        // 쉴드 링: 중심 비우고 바깥쪽 링
                        float innerRadius = 0.5f + phase * 0.3f;
                        float outerRadius = 0.9f + phase * 0.1f;
                        float ring = 1f - Mathf.Clamp01(Mathf.Abs(dist - (innerRadius + outerRadius) * 0.5f) / ((outerRadius - innerRadius) * 0.5f));
                        float alpha = ring * (1f - phase * 0.5f);

                        // 보라색 그라데이션
                        float r = Mathf.Lerp(0.4f, 0.7f, phase);
                        float g = Mathf.Lerp(0.1f, 0.3f, phase);
                        float b = Mathf.Lerp(0.8f, 1f, phase);

                        tex.SetPixel(x, y, new Color(r, g, b, alpha));
                    }
                }

                tex.Apply();
                sprites[f] = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
                sprites[f].name = $"VoidShield_Gen_{f}";
            }
            return sprites;
        }

        private void DoSpawnSplitEnemies(Vector3 deathPos, EnemySpawnRuleConfig splitConfig)
        {
            Transform spawnParent = transform.parent != null ? transform.parent : transform;
            DoSpawnSplitEnemies(deathPos, splitConfig, config.splitCount, spawnParent);
        }

        private void DoSpawnSplitEnemies(Vector3 deathPos, EnemySpawnRuleConfig splitConfig, int count, Transform spawnParent)
        {
            Debug.Log($"[EliteDeath] DoSpawnSplitEnemies: count={count}, deathPos={deathPos}, parent={spawnParent?.name}");

            BiomeManager biome = BiomeManager.Active;
            int poolId = GetPoolArchetypeId(splitConfig);

            for (int i = 0; i < count; i++)
            {
                // 사망 위치 주변에 분산 배치
                Vector2 offset = Random.insideUnitCircle * 1.5f;
                Vector3 spawnPos = deathPos + new Vector3(offset.x, 0f, offset.y);

                if (biome != null)
                {
                    Vector2Int grid = biome.WorldToGrid(spawnPos);
                    if (!biome.IsValidPosition(grid.x, grid.y) || !biome.IsWalkable(grid.x, grid.y))
                    {
                        spawnPos = deathPos;
                    }
                    spawnPos.y = biome.GetGroundHeight(spawnPos) + splitConfig.heightOffset;
                }

                // 직접 적 생성 (스포너 없이 즉시)
                EnemyController split = Acquire(spawnParent, $"{splitConfig.name}_Split_{i}", poolId);
                split.Configure(null, splitConfig, spawnPos, spawnPos);
                Debug.Log($"[EliteDeath] 분열 적 #{i} 생성 완료: {split.gameObject.name} at {spawnPos}");
            }
        }

        private void SpawnAggroDebris()
        {
            Vector3 pos = GetCurrentPosition();
            pos.y += 0.5f;

            GameObject debrisObj = new GameObject($"AggroDebris_{config.name}");
            debrisObj.transform.position = pos;

            AggroDebris debris = debrisObj.AddComponent<AggroDebris>();

            // 잔해 스프라이트: 사망 스프라이트의 마지막 프레임
            Sprite debrisSprite = null;
            if (config.deathSprites != null && config.deathSprites.Length > 0)
                debrisSprite = config.deathSprites[config.deathSprites.Length - 1];
            else if (config.idleSprites != null && config.idleSprites.Length > 0)
                debrisSprite = config.idleSprites[0];

            debris.Configure(
                config.debrisDuration,
                config.debrisAggroRadius,
                debrisSprite,
                config.sortingOrder,
                config.debrisVfxSprites,
                config.debrisVfxScale,
                config.debrisVfxSpeed
            );
        }

        private EnemySpawnRuleConfig FindEnemyConfigByName(string enemyName)
        {
            // ConfigurableBiomeManager에서 현재 바이옴의 적 설정 검색
            BiomeManager biome = BiomeManager.Active;
            if (biome == null)
            {
                Debug.LogWarning("[EliteDeath] FindEnemyConfigByName: BiomeManager.Active == null");
                return null;
            }

            ConfigurableBiomeManager configBiome = biome as ConfigurableBiomeManager;
            if (configBiome == null)
            {
                Debug.LogWarning($"[EliteDeath] FindEnemyConfigByName: ConfigurableBiomeManager 캐스트 실패, 타입={biome.GetType().Name}");
                return null;
            }

            BiomeConfig biomeConfig = configBiome.GetBiomeConfig();
            if (biomeConfig == null)
            {
                Debug.LogWarning("[EliteDeath] FindEnemyConfigByName: BiomeConfig == null");
                return null;
            }

            foreach (EnemySpawnRuleConfig rule in biomeConfig.enemySpawnRules)
            {
                if (rule.name == enemyName)
                    return rule;
            }
            Debug.LogWarning($"[EliteDeath] FindEnemyConfigByName: '{enemyName}'을 찾을 수 없음! 등록된 적: {biomeConfig.enemySpawnRules.Count}개");
            return null;
        }

        // ─────────────────────────────────
        // 애니메이션
        // ─────────────────────────────────

        public void SetIdleAnimation()
        {
            usingMoveAnimation = false;
            ApplyAnimation(GetIdleFrames());
        }
        // SetMoveAnimation: 관련 설정과 상태를 구성합니다.

        public void SetMoveAnimation()
        {
            usingMoveAnimation = true;
            ApplyAnimation(config.moveSprites);
        }

        // ─────────────────────────────────
        // 내부 메서드 (기존 로직 유지)
        // ─────────────────────────────────

        private void SetDestination(Vector3 targetPosition)
        {
            destination = targetPosition;
            hasDestination = true;
        }
        // TryPickWanderDestination: 작업을 시도하고 성공 여부를 반환합니다.

        private bool TryPickWanderDestination(out Vector3 wanderDestination)
        {
            BiomeManager biome = BiomeManager.Active;
            if (biome == null)
            {
                wanderDestination = anchorPosition;
                return true;
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = Random.insideUnitCircle * Mathf.Max(0f, config.wanderRadius);
                Vector3 candidate = anchorPosition + new Vector3(offset.x, 0f, offset.y);
                Vector2Int grid = biome.WorldToGrid(candidate);
                if (!biome.IsValidPosition(grid.x, grid.y) || !biome.IsWalkable(grid.x, grid.y))
                    continue;

                candidate.y = biome.GetGroundHeight(candidate) + config.heightOffset;
                wanderDestination = candidate;
                return true;
            }

            wanderDestination = anchorPosition;
            wanderDestination.y = biome.GetGroundHeight(anchorPosition) + config.heightOffset;
            return true;
        }
        // TryMove: 작업을 시도하고 성공 여부를 반환합니다.

        private bool TryMove(Vector3 currentPosition, Vector3 step)
        {
            BiomeManager biome = BiomeManager.Active;
            if (biome == null)
            {
                MoveToPosition(currentPosition + step);
                return true;
            }

            Vector3 targetPosition = currentPosition + step;
            if (biome.CanMove(currentPosition, targetPosition))
            {
                MoveToPosition(targetPosition);
                return true;
            }

            Vector3 moveX = new Vector3(step.x, 0f, 0f);
            Vector3 moveZ = new Vector3(0f, 0f, step.z);

            if (Mathf.Abs(step.x) >= Mathf.Abs(step.z))
            {
                if (moveX.sqrMagnitude > 0f && biome.CanMove(currentPosition, currentPosition + moveX))
                {
                    MoveToPosition(currentPosition + moveX);
                    return true;
                }
                if (moveZ.sqrMagnitude > 0f && biome.CanMove(currentPosition, currentPosition + moveZ))
                {
                    MoveToPosition(currentPosition + moveZ);
                    return true;
                }
            }
            else
            {
                if (moveZ.sqrMagnitude > 0f && biome.CanMove(currentPosition, currentPosition + moveZ))
                {
                    MoveToPosition(currentPosition + moveZ);
                    return true;
                }
                if (moveX.sqrMagnitude > 0f && biome.CanMove(currentPosition, currentPosition + moveX))
                {
                    MoveToPosition(currentPosition + moveX);
                    return true;
                }
            }

            return false;
        }
        // GetSeparationVector: 필요한 값을 반환합니다.

        // 다른 적과의 분리 벡터 계산 (겹침 방지용 보이드 행동)
        private Vector3 GetSeparationVector(Vector3 currentPosition)
        {
            if (config == null || config.separationDistance <= 0f)
                return Vector3.zero;

            float maxDistanceSq = config.separationDistance * config.separationDistance;
            Vector3 separation = Vector3.zero;
            Vector2Int centerCell = ToSpatialCell(currentPosition);
            int searchRadius = Mathf.Max(1, Mathf.CeilToInt(config.separationDistance / SpatialHashCellSize));

            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int y = -searchRadius; y <= searchRadius; y++)
                {
                    Vector2Int cell = new Vector2Int(centerCell.x + x, centerCell.y + y);
                    if (!ActiveEnemyCells.TryGetValue(cell, out List<EnemyController> enemiesInCell))
                    {
                        continue;
                    }

                    for (int i = 0; i < enemiesInCell.Count; i++)
                    {
                        EnemyController other = enemiesInCell[i];
                        if (other == null || other == this || other.config == null) continue;

                        Vector3 delta = currentPosition - other.GetCurrentPosition();
                        delta.y = 0f;
                        float distanceSq = delta.sqrMagnitude;
                        if (distanceSq <= 0.0001f || distanceSq > maxDistanceSq) continue;

                        float distance = Mathf.Sqrt(distanceSq);
                        float weight = 1f - (distance / config.separationDistance);
                        separation += delta.normalized * weight;
                    }
                }
            }

            return separation;
        }
        // ApplyAnimation: 변경 사항을 런타임 객체에 반영합니다.

        private void ApplyAnimation(Sprite[] frames)
        {
            if (spriteRenderer == null) return;

            if (frames == null || frames.Length == 0)
            {
                animatedSprite.Stop();
                animatedSprite.enabled = false;
                return;
            }

            if (frames.Length == 1)
            {
                animatedSprite.Stop();
                animatedSprite.enabled = false;
                spriteRenderer.sprite = frames[0];
                return;
            }

            animatedSprite.enabled = true;
            animatedSprite.SetFrames(frames, config.animationSpeed);
            animatedSprite.Play();
        }
        // GetIdleFrames: 필요한 값을 반환합니다.

        private Sprite[] GetIdleFrames()
        {
            if (config.idleSprites != null && config.idleSprites.Length > 0)
                return config.idleSprites;
            return config.moveSprites;
        }
        // SyncHeight: 변경 사항을 런타임 객체에 반영합니다.

        private void SyncHeight()
        {
            if (config == null) return;
            BiomeManager biome = BiomeManager.Active;
            if (biome == null) return;

            Vector3 position = GetCurrentPosition();
            Vector2Int grid = biome.WorldToGrid(position);
            if (!hasCachedGroundHeight || grid != cachedGroundGrid)
            {
                cachedGroundGrid = grid;
                cachedGroundHeight = biome.GetGroundHeight(grid.x, grid.y) + config.heightOffset;
                hasCachedGroundHeight = true;
            }

            if (Mathf.Abs(position.y - cachedGroundHeight) > 0.0001f)
            {
                position.y = cachedGroundHeight;
                SetPosition(position);
            }
        }
        // EnsurePlayerTransform: 이 컴포넌트의 핵심 로직을 실행합니다.

        private void EnsurePlayerTransform()
        {
            if (playerTransform != null) return;
            if (PlayerController.Instance != null)
                playerTransform = PlayerController.Instance.transform;
        }
        // EnsureComponents: 이 컴포넌트의 핵심 로직을 실행합니다.

        private void EnsureComponents()
        {
            if (visualRoot == null)
            {
                Transform child = transform.Find("Visual");
                if (child == null)
                {
                    GameObject visualObject = new GameObject("Visual");
                    child = visualObject.transform;
                    child.SetParent(transform, false);
                }
                visualRoot = child;
            }

            spriteRenderer = GetOrAddComponent<SpriteRenderer>(visualRoot.gameObject);
            animatedSprite = GetOrAddComponent<AnimatedSprite>(visualRoot.gameObject);
            billboard = GetOrAddComponent<Billboard>(visualRoot.gameObject);
            ySort = GetOrAddComponent<SpriteYSort>(visualRoot.gameObject);
            body = GetOrAddComponent<Rigidbody>(gameObject);
            boxCollider = GetOrAddComponent<BoxCollider>(gameObject);
            stats = GetOrAddComponent<CharacterStats>(gameObject);
            statusEffectController = GetOrAddComponent<EnemyStatusEffectController>(gameObject);
            enemySkillBridge = GetOrAddComponent<EnemySkillBridge>(gameObject);
        }
        // ConfigureStats: 관련 설정과 상태를 구성합니다.

        // config의 값으로 CharacterStats 기본 스탯 설정
        private void ConfigureStats()
        {
            if (stats == null || config == null)
            {
                return;
            }

            List<CharacterStatValue> additionalStats = config.additionalBaseStats ?? new List<CharacterStatValue>();
            List<CharacterStatValue> baseStats = new List<CharacterStatValue>(3 + additionalStats.Count)
            {
                new CharacterStatValue(CharacterStatType.MoveSpeed, config.moveSpeed),
                new CharacterStatValue(CharacterStatType.MaxHealth, config.maxHealth),
                new CharacterStatValue(CharacterStatType.AttackPower, NormalizedEnemyAttackDamage)
            };

            for (int i = 0; i < additionalStats.Count; i++)
            {
                baseStats.Add(additionalStats[i]);
            }

            stats.ClearModifiers();
            stats.ConfigureBaseStats(baseStats, true);
        }
        // ApplyVisualSetup: 변경 사항을 런타임 객체에 반영합니다.

        private void ApplyVisualSetup()
        {
            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = config.scale;
            spriteRenderer.sortingOrder = config.sortingOrder;
            spriteRenderer.flipX = false;

            // 엘리트 틴트 색상 적용
            spriteRenderer.color = (config.isElite && config.tintColor != Color.white)
                ? config.tintColor
                : Color.white;

            billboard.enabled = config.useBillboard;
            if (config.useBillboard)
            {
                billboard.ResetBaseLocalPosition(Vector3.zero);
                billboard.SetUpdateMode(Billboard.UpdateMode.Continuous);
            }

            ySort.enabled = config.useYSort;
            if (config.useYSort)
            {
                ySort.Configure(SpriteYSort.WorldDynamicBaseSortingOrder, true, SpriteYSort.WorldDynamicMinSortingOrder);
                ySort.SetUpdateMode(SpriteYSort.UpdateMode.Continuous);
            }
        }
        // ApplyPhysicsSetup: 변경 사항을 런타임 객체에 반영합니다.

        private void ApplyPhysicsSetup()
        {
            body.useGravity = false;
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.constraints = RigidbodyConstraints.FreezeRotation;

            boxCollider.enabled = config.addCollider;
            boxCollider.isTrigger = config.isTrigger;
            boxCollider.size = config.colliderSize;
            boxCollider.center = config.colliderCenter;
        }
        // GetCurrentPosition: 필요한 값을 반환합니다.

        private Vector3 GetCurrentPosition()
        {
            return body != null ? body.position : transform.position;
        }
        // MoveToPosition: 해당 작업 흐름을 수행합니다.

        private void MoveToPosition(Vector3 position)
        {
            if (body != null)
            {
                body.position = position;
            }
            else
            {
                transform.position = position;
            }

            RegisterOrUpdateSpatialCell(position);
        }
        // SetPosition: 관련 설정과 상태를 구성합니다.

        private void SetPosition(Vector3 position)
        {
            if (body != null)
            {
                body.position = position;
            }
            else
            {
                transform.position = position;
            }

            RegisterOrUpdateSpatialCell(position);
        }

        private static Vector2Int ToSpatialCell(Vector3 position)
        {
            return new Vector2Int(
                Mathf.FloorToInt(position.x / SpatialHashCellSize),
                Mathf.FloorToInt(position.z / SpatialHashCellSize));
        }

        private void RegisterOrUpdateSpatialCell(Vector3 position)
        {
            Vector2Int newCell = ToSpatialCell(position);
            if (isRegisteredInSpatialHash && newCell == currentSpatialCell)
            {
                return;
            }

            if (isRegisteredInSpatialHash)
            {
                UnregisterSpatialCell();
            }

            if (!ActiveEnemyCells.TryGetValue(newCell, out List<EnemyController> list))
            {
                list = new List<EnemyController>();
                ActiveEnemyCells.Add(newCell, list);
            }

            list.Add(this);
            currentSpatialCell = newCell;
            isRegisteredInSpatialHash = true;
        }

        private void UnregisterSpatialCell()
        {
            if (!isRegisteredInSpatialHash)
            {
                return;
            }

            if (ActiveEnemyCells.TryGetValue(currentSpatialCell, out List<EnemyController> list))
            {
                list.Remove(this);
                if (list.Count == 0)
                {
                    ActiveEnemyCells.Remove(currentSpatialCell);
                }
            }

            isRegisteredInSpatialHash = false;
        }

        private T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
                component = target.AddComponent<T>();
            return component;
        }
        // NotifyOwnerReleased: 이 컴포넌트의 핵심 로직을 실행합니다.

        private void NotifyOwnerReleased()
        {
            if (notifiedOwner || owner == null) return;
            owner.NotifyEnemyReleased(this);
            notifiedOwner = true;
        }

        private void RaiseDefeated()
        {
            if (defeatEventRaised)
            {
                return;
            }

            defeatEventRaised = true;
            Defeated?.Invoke(this);
        }
        // PrepareForPool: 이 컴포넌트의 핵심 로직을 실행합니다.

        private void PrepareForPool()
        {
            if (animatedSprite != null)
            {
                animatedSprite.Stop();
                animatedSprite.enabled = false;
            }

            if (spriteRenderer != null)
                spriteRenderer.flipX = false;

            if (body != null)
            {
                if (!body.isKinematic)
                    body.angularVelocity = Vector3.zero;
                body.rotation = Quaternion.identity;
            }

            // 콜라이더 복원
            if (boxCollider != null && config != null)
                boxCollider.enabled = config.addCollider;
        }
        // GetPlanarDistance: 필요한 값을 반환합니다.

        private static float GetPlanarDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
        // EnsurePoolRoot: 이 컴포넌트의 핵심 로직을 실행합니다.

        private static void EnsurePoolRoot()
        {
            if (poolRoot != null) return;
            GameObject root = GameObject.Find(PoolRootName);
            if (root == null) root = new GameObject(PoolRootName);
            poolRoot = root.transform;
        }
        // GetPoolArchetypeId: 필요한 값을 반환합니다.

        public static int GetPoolArchetypeId(EnemySpawnRuleConfig config)
        {
            if (config == null) return 0;
            return unchecked((config.poissonSalt * 397) ^ Animator.StringToHash(config.name ?? "Enemy"));
        }
        // GetOrCreatePool: 필요한 값을 반환합니다.

        private static Stack<EnemyController> GetOrCreatePool(int poolArchetypeId)
        {
            if (!PooledEnemies.TryGetValue(poolArchetypeId, out Stack<EnemyController> pool))
            {
                pool = new Stack<EnemyController>();
                PooledEnemies.Add(poolArchetypeId, pool);
            }
            return pool;
        }
    }
}
