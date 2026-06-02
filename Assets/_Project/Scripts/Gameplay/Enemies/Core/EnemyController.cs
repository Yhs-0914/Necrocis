using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// FSM 기반 적 AI + 오브젝트 풀링.
    /// 배회(Wander) / 추격(Chase) / 복귀(Return) / 공격(Attack) / 사망(Dead)
    /// </summary>
    public partial class EnemyController : MonoBehaviour
    {
        private const string PoolRootName = "__EnemyPool";
        private const float SpatialHashCellSize = 2f;

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
        private SpriteFrameAnimator animatedSprite;    // 프레임 애니메이션 재생기
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
        private bool aiSuppressed;

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
        public event System.Action<EnemyController, float> DamageTaken;

        // ─────────────────────────────────
        // 풀링 API (기존 유지)
        // ─────────────────────────────────

        // 풀에서 적을 꺼내거나 새로 생성 (오브젝트 풀링)
        // Configure: 관련 설정과 상태를 구성합니다.

        // 적 초기 설정: 스폰 위치, 스탯, 물리, 비주얼, FSM 시작
        // ReleaseToPool: 상태 전환 관련 흐름을 처리합니다.

        // 오브젝트 풀로 반환: FSM 종료 → 비활성화 → 풀에 Push

        // ─────────────────────────────────
        // Unity 라이프사이클
        // ─────────────────────────────────

        private void Update()
        {
            if (config == null) return;

            EnsurePlayerTransform();

            if (aiSuppressed && !IsDead)
            {
                SyncHeight();
                return;
            }

            // 돌진 쿨타임 감소
            if (chargeCooldownTimer > 0f)
                chargeCooldownTimer -= Time.deltaTime;

            // FSM Update
            currentState?.Update(this, Time.deltaTime);

            SyncHeight();
        }
        // 유니티 콜백: OnDisable 이벤트에 반응합니다.
        // 유니티 콜백: OnDestroy 이벤트에 반응합니다.

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
        // IsPlayerInAttackRange: 조건 충족 여부를 확인합니다.
        // IsOutOfLeash: 조건 충족 여부를 확인합니다.
        // IsIdleTimerExpired: 조건 충족 여부를 확인합니다.

        // ─────────────────────────────────
        // 행동 (FSM 상태에서 호출)
        // ─────────────────────────────────
        // PickWanderDestination: 이 컴포넌트의 핵심 로직을 실행합니다.
        // SetChaseDestination: 관련 설정과 상태를 구성합니다.
        // SetReturnDestination: 관련 설정과 상태를 구성합니다.

        /// <summary>
        /// 목적지로 이동. 도착하면 false 반환.
        /// </summary>
        // TryPerformAttack: 작업을 시도하고 성공 여부를 반환합니다.

        /// <summary>
        /// 쿨타임 기반 공격. 쿨타임 만료 시 공격 애니메이션 시작 → 애니메이션 완료 시 데미지 적용.
        /// 반환값: true면 공격 애니메이션 시작됨 (쿨타임 대기 중이면 false).
        /// </summary>
        // TakeDamage: 이 컴포넌트의 핵심 로직을 실행합니다.

        // 데미지 처리: CharacterStats에 적용 → HP 0이면 Dead 상태로 전환
        // GrantExp: 이 컴포넌트의 핵심 로직을 실행합니다.

        // 사망 시 플레이어에게 경험치 부여 + 킬 카운트 알림
        // ApplyKnockback: 변경 사항을 런타임 객체에 반영합니다.

        // 넉백: 지정 방향으로 밀어냄 (공격 적중 시 호출)
        // DisableCollider: 이 컴포넌트의 핵심 로직을 실행합니다.

        /// <summary>
        /// 공격 시 콜라이더 확장 (대식세포: 가시 펼침)
        /// </summary>

        /// <summary>
        /// 콜라이더를 원래 크기로 복원
        /// </summary>

        /// <summary>
        /// 사망 애니메이션 재생. 완료 시 onComplete 콜백.
        /// </summary>

        /// <summary>
        /// 공격 방향에 맞는 스프라이트 프레임 반환.
        /// NK세포: 상/하/좌우 방향별, 대식세포: 단일 공격 스프라이트.
        /// </summary>

        /// <summary>
        /// 플레이어 방향 감지: 0=상, 1=우, 2=좌, 3=하
        /// 2.5D 쿼터뷰: X=좌우, Z=상하(깊이)
        /// </summary>

        /// <summary>
        /// 공격 애니메이션 강제 중단 (상태 전환 시)
        /// </summary>

        // ─────────────────────────────────
        // 돌진 (항체 엘리트)
        // ─────────────────────────────────

        /// <summary>
        /// 돌진 시작: 플레이어 방향 고정, 가속 시작
        /// </summary>

        /// <summary>
        /// 돌진 업데이트: 가속 후 일정 속도로 이동. 반환값: true면 돌진 지속 중
        /// </summary>

        /// <summary>
        /// 돌진 종료 → 쿨타임 시작 (3초)
        /// </summary>

        // ─────────────────────────────────
        // 어그로 부스트 (항체 잔해 효과)
        // ─────────────────────────────────

        /// <summary>
        /// chaseRadius를 percentBoost만큼 증가 (0.5 = 50%)
        /// </summary>

        /// <summary>
        /// chaseRadius를 원래 값으로 복원
        /// </summary>

        /// <summary>
        /// 강제로 플레이어를 추격 + 이동속도 부스트 (어그로 잔해 효과)
        /// </summary>

        /// <summary>
        /// 어그로 부스트 + 이동속도 버프 모두 해제
        /// </summary>

        // ─────────────────────────────────
        // 엘리트 사망 처리
        // ─────────────────────────────────

        /// <summary>
        /// 엘리트 사망 시 특수 효과 실행 (DeadState에서 호출)
        /// </summary>

        /// <summary>
        /// VoidShield 이펙트를 재생한 뒤 분열 적을 소환한다.
        /// config.splitVfxSprites가 있으면 해당 스프라이트 사용, 없으면 프로시저럴 이펙트.
        /// </summary>

        /// <summary>
        /// VoidShield 스프라이트를 로드한다. config에 있으면 그것을, 없으면 Resources에서 로드.
        /// </summary>
        private static Sprite[] cachedVoidShieldSprites;

        /// <summary>
        /// 프로시저럴 VoidShield 스프라이트 생성 (보라색 원형 쉴드)
        /// </summary>

        // ─────────────────────────────────
        // 애니메이션
        // ─────────────────────────────────
        // SetMoveAnimation: 관련 설정과 상태를 구성합니다.

        // ─────────────────────────────────
        // 내부 메서드 (기존 로직 유지)
        // ─────────────────────────────────
        // TryPickWanderDestination: 작업을 시도하고 성공 여부를 반환합니다.
        // TryMove: 작업을 시도하고 성공 여부를 반환합니다.
        // GetSeparationVector: 필요한 값을 반환합니다.

        // 다른 적과의 분리 벡터 계산 (겹침 방지용 보이드 행동)
        // ApplyAnimation: 변경 사항을 런타임 객체에 반영합니다.
        // GetIdleFrames: 필요한 값을 반환합니다.
        // SyncHeight: 변경 사항을 런타임 객체에 반영합니다.
        // EnsurePlayerTransform: 이 컴포넌트의 핵심 로직을 실행합니다.
        // EnsureComponents: 이 컴포넌트의 핵심 로직을 실행합니다.
        // ConfigureStats: 관련 설정과 상태를 구성합니다.

        // config의 값으로 CharacterStats 기본 스탯 설정
        // ApplyVisualSetup: 변경 사항을 런타임 객체에 반영합니다.
        // ApplyPhysicsSetup: 변경 사항을 런타임 객체에 반영합니다.
        // GetCurrentPosition: 필요한 값을 반환합니다.
        // MoveToPosition: 해당 작업 흐름을 수행합니다.
        // SetPosition: 관련 설정과 상태를 구성합니다.

        private T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
                component = target.AddComponent<T>();
            return component;
        }
        // NotifyOwnerReleased: 이 컴포넌트의 핵심 로직을 실행합니다.
        // PrepareForPool: 이 컴포넌트의 핵심 로직을 실행합니다.
        // GetPlanarDistance: 필요한 값을 반환합니다.
        // EnsurePoolRoot: 이 컴포넌트의 핵심 로직을 실행합니다.
        // GetPoolArchetypeId: 필요한 값을 반환합니다.
        // GetOrCreatePool: 필요한 값을 반환합니다.
    }
}
