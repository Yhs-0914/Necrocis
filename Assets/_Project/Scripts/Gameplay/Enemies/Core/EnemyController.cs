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
        private static Sprite[] cachedVoidShieldSprites;

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
        private EnemyContactDamage contactDamage;
        private readonly List<CharacterStatValue> statConfigurationBuffer = new List<CharacterStatValue>();

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
        private int facingDirection = 3;  // 0=상, 1=우, 2=좌, 3=하
        private Sprite[] currentLoopFrames;

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
        public bool IsBossEncounter => ignoreMidBossArenaRestriction;
        public event System.Action<EnemyController> Defeated;
        public event System.Action<EnemyController, float> DamageTaken;

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

        public void ChangeState(IEnemyState newState)
        {
            if (newState == currentState) return;

            currentState?.Exit(this);
            currentState = newState;
            currentState?.Enter(this);
        }

        private T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
                component = target.AddComponent<T>();
            return component;
        }
    }
}
