# 잡몹 시스템 (Enemy System)

## 전체 흐름

```

청크 로드 (BiomeManager.OnChunkLoaded)
  → Budget System으로 몹 조합 결정
    → BiomeManager.IsWalkable()로 스폰 위치 검증
      → 몹 스폰
        → EnemyFSM 초기화 (Idle 상태)
          → 플레이어 감지 (detectRange)
            → Chase (BiomeManager.CanMove 기반 이동)
              → 사정거리 진입 (attackRange)
                → Attack (쿨다운 관리)
                  → 체력 0
                    → Dead → 제거
```

---

## 파일 구조

```
Assets/02.Scripts/Enemy/
├── EnemySpawnData.cs            ← ScriptableObject (몹 정의)
├── EnemyBudgetSpawner.cs        ← Budget System + 스폰 총괄
├── EnemyBase.cs                 ← 기존 적 베이스 (레거시)
├── EnemySpawner.cs              ← 기존 스포너 (레거시)
├── NKCell.cs                    ← 기존 NK세포 (레거시)
└── FSM/
    ├── IEnemyState.cs           ← 상태 인터페이스 (Enter/Update/Exit)
    ├── EnemyFSM.cs              ← 새 몹 베이스 (BiomeManager 기반 이동)
    ├── EnemyIdleState.cs        ← 대기 → 감지시 Chase
    ├── EnemyChaseState.cs       ← 추격 → 사정거리 진입시 Attack
    ├── EnemyAttackState.cs      ← 공격 → 범위 이탈시 Chase
    └── EnemyDeadState.cs        ← 사망 → 콜라이더 비활성
```

---

## FSM (Finite State Machine)

몹은 항상 **하나의 상태**에만 존재. 각 상태는 Enter/Update/Exit 패턴.

### 상태 전환 흐름

```
Idle ──(감지범위 진입)──→ Chase
Chase ──(공격범위 진입)──→ Attack
Chase ──(감지범위 이탈)──→ Idle
Attack ──(범위 이탈)────→ Chase
Attack ──(체력 0)───────→ Dead
Chase ──(체력 0)────────→ Dead
```

### IEnemyState 인터페이스

```csharp
public interface IEnemyState
{
    void Enter(EnemyFSM enemy);   // 상태 진입 시 1회
    void Update(EnemyFSM enemy);  // 매 프레임
    void FixedUpdate(EnemyFSM enemy); // 물리 프레임
    void Exit(EnemyFSM enemy);    // 상태 나갈 때 1회
}
```

### 상태 인스턴스 (GC 방지)

```csharp
// static readonly → 모든 몹이 공유, 매번 new 안 함
public static readonly EnemyIdleState IdleState = new EnemyIdleState();
public static readonly EnemyChaseState ChaseState = new EnemyChaseState();
public static readonly EnemyAttackState AttackState = new EnemyAttackState();
public static readonly EnemyDeadState DeadState = new EnemyDeadState();
```

---

## Budget System

### 예산 계산

```
기본 예산 = baseBudget + (층 × budgetPerFloor)
거리 보정 = AnimationCurve로 0~1 사이 값

인접 청크 (거리 0~1) → 예산 100%
1칸 거리 (거리 1.4)  → 예산 ~80%
2칸 거리             → 예산 ~40%
3칸 이상             → 스폰 안 함
```

### 등급별 예산 비율 강제 (방법 2)

예산을 구간으로 나눠서 각 구간 안에서만 Weighted Random:

```
전체 예산 150pt 기준:
  약한몹 구간 → 150 × 0.3 = 45pt → 10pt짜리 4마리
  중간몹 구간 → 150 × 0.4 = 60pt → 25pt짜리 2마리
  강한몹 구간 → 150 × 0.3 = 45pt → 50pt짜리 0마리 (코스트 부족)
```

### 등급별 최대 개수 제한 (방법 1)

```
강한몹 → 청크당 최대 1마리
중간몹 → 청크당 최대 3마리
약한몹 → 사실상 무제한 (99)
```

### 이중 안전장치

강한몹 과다 출현 방지:
1. 구간 예산이 코스트(50pt)보다 적으면 아예 못 삼
2. 설령 예산이 충분해도 청크당 최대 1마리 제한

---

## EnemySpawnData (ScriptableObject)

Unity에서 `Create → Necrocis → Enemy Spawn Data`로 생성.

| 필드 | 설명 |
|------|------|
| enemyName | 몹 이름 |
| prefab | 프리팹 |
| tier | Weak / Medium / Strong |
| budgetCost | 예산 소모량 |
| weight | 같은 등급 내 뽑힐 확률 (0~1) |

### 현재 설정

| 에셋 | Tier | Cost | Weight |
|------|------|------|--------|
| EnemyData_Weak | Weak | 10 | 0.5 |
| EnemyData_Medium | Medium | 25 | 0.35 |
| EnemyData_Strong | Strong | 50 | 0.15 |

---

## EnemyFSM (몹 베이스 클래스)

### 주요 스탯

| 필드 | 설명 | Weak | Medium | Strong |
|------|------|------|--------|--------|
| maxHealth | 최대 체력 | 30 | 60 | 120 |
| attackDamage | 공격력 | 5 | 15 | 30 |
| moveSpeed | 이동속도 | 3 | 2.5 | 2 |
| detectRange | 감지 범위 | 7 | 10 | 12 |
| attackRange | 공격 범위 | 1.5 | 2 | 2.5 |

### 이동 방식

NavMeshAgent 대신 **BiomeManager.CanMove()** 사용 (Tilemap 기반 프로젝트):

```csharp
public void MoveToward(Vector3 targetPos)
{
    // 방향 계산
    Vector3 movement = direction * moveSpeed * Time.fixedDeltaTime;

    // BiomeManager로 이동 가능 여부 확인
    if (!biome.CanMove(currentPos, nextPos))
    {
        // X, Z 분리 시도 (벽 슬라이딩)
    }

    rb.MovePosition(rb.position + movement);
}
```

---

## BiomeManager 연동

`BiomeManager.OnChunkLoaded()`에서 자동 호출:

```csharp
protected virtual void OnChunkLoaded(Chunk chunk)
{
    var spawner = GetEnemySpawner(); // 캐싱됨
    if (spawner != null)
    {
        spawner.OnChunkLoaded(coord, center, dist);
    }
}
```

청크 언로드 시 해당 청크의 적 자동 제거:

```csharp
protected virtual void OnChunkUnloaded(Chunk chunk)
{
    spawner.OnChunkUnloaded(coord);
}
```

---

## 씬 배치

```
Hub.unity        → 몹 없음 (안전지대)
Intestine.unity  → EnemySystem 오브젝트 (EnemyBudgetSpawner)
Liver.unity      → EnemySystem 오브젝트 (바이옴별 다른 몹 풀 가능)
Stomach.unity    → EnemySystem 오브젝트
Lung.unity       → EnemySystem 오브젝트
```

각 바이옴 씬의 EnemySystem에 해당 바이옴 전용 몹 데이터를 연결.

---

## 프리팹 구성

각 몹 프리팹에 필요한 컴포넌트:

- **EnemyFSM** (스탯 설정)
- **Capsule Collider** (3D, 2D 아님!)
- **SpriteRenderer** (자식 오브젝트 또는 직접)

> NavMeshAgent는 사용하지 않음 (Tilemap 기반이라 BiomeManager로 이동)

---

## Inspector 권장값 (EnemyBudgetSpawner)

| 필드 | 권장값 | 설명 |
|------|--------|------|
| Base Budget | 150 | 기본 예산 |
| Budget Per Floor | 10 | 층당 추가 예산 |
| Weak Budget Ratio | 0.3 | 약한몹 예산 비율 |
| Medium Budget Ratio | 0.4 | 중간몹 예산 비율 |
| Strong Budget Ratio | 0.3 | 강한몹 예산 비율 |
| Max Weak Per Chunk | 99 | 약한몹 최대 |
| Max Medium Per Chunk | 3 | 중간몹 최대 |
| Max Strong Per Chunk | 1 | 강한몹 최대 |
| Full Budget Distance | 1 | 100% 예산 거리 |
| Min Budget Distance | 3 | 스폰 안 하는 거리 |
| Spawn Radius | 10 | 스폰 범위 |
| Min Dist From Player | 5 | 플레이어 최소 거리 |

---

## TODO

- [ ] 플레이어 데미지 시스템 연동 (PlayerController.TakeDamage)
- [ ] 스프라이트 애니메이션 연동 (방향별 이동/공격)
- [ ] 아이템 드롭 시스템
- [ ] 사망 애니메이션
- [ ] 보스몹은 BT(Behavior Tree)로 별도 구현
- [ ] 바이옴별 전용 몹 종류 추가
