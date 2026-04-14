# Necrocis 최적화 검토

## 문서 목적

이 문서는 현재 `main` 기준 코드에 대해

- 지금 당장 손댈 가치가 큰 최적화 포인트
- 아직은 우선순위가 낮은 포인트
- 왜 그런 판단을 했는지

를 정리한 문서다.

보스 구역 작업이 반영된 현재 구조를 기준으로 작성했다.

---

## 결론 요약

지금 상태에서 가장 먼저 최적화할 가치가 큰 곳은 아래 4개다.

1. 적 separation 계산
2. 스포너 Update 빈도
3. 거리 판정의 `Vector3.Distance` / `magnitude` 사용
4. 플레이어 공격 판정의 allocation

반대로,

- 중간보스 안개 구역 자체
- `CharacterStats` 재계산

은 현재 기준으로 병목 우선순위가 높지 않다.

---

## 1. 가장 먼저 손댈 곳

### 1) 적 separation 계산

관련 파일:

- `Assets/02.Scripts/Enemy/EnemyController.cs`

관련 메서드:

- `GetSeparationVector(...)`

현재 구조:

- 이동 중인 적 하나가 separation 벡터를 구할 때
- `ActiveEnemies` 전체를 순회한다

즉 적 수가 많아지면 사실상 `O(N²)` 구조가 된다.

예:

- 적 10마리: 체감 적음
- 적 30마리 이상: 누적 비용이 커지기 시작
- 적 50~60마리 이상: 가장 먼저 병목이 될 가능성 큼

왜 위험한가:

- 모든 적이 매 프레임 이동
- 이동할 때마다 separation 계산
- separation 계산이 모든 적을 다시 훑음

추천 방향:

1. 청크 기반 근처 적만 검사
2. 일정 거리 안 적만 spatial bucket으로 조회
3. separation 갱신을 매 프레임이 아니라 2~3프레임에 한 번만 수행

추천 우선순위:

- 매우 높음

---

### 2) 스포너 Update 빈도

관련 파일:

- `Assets/02.Scripts/Enemy/EnemySpawner.cs`

현재 구조:

- 모든 스포너가 `Update()`에서 매 프레임 실행
- 플레이어 거리 확인
- 활성화 범위 확인
- 카메라 화면 안/밖 확인
- 리스폰 여부 판단

문제:

- 스포너 수가 많아질수록 매 프레임 반복 비용이 누적된다
- 특히 카메라 viewport 계산은 몬스터가 많아질 때 무의미한 반복이 될 수 있다

추천 방향:

1. 스포너 갱신을 `0.1 ~ 0.25초` 간격으로만 수행
2. 스포너마다 타이머 offset을 줘서 프레임 분산
3. 거리 비교는 제곱거리(`sqrMagnitude`)로 교체
4. 카메라 참조는 캐싱

추천 우선순위:

- 매우 높음

---

### 3) 거리 판정 최적화

관련 파일:

- `Assets/02.Scripts/Enemy/EnemyController.cs`
- `Assets/02.Scripts/Enemy/EnemySpawner.cs`

현재 구조:

- `Vector3.Distance`
- `magnitude`

를 여러 곳에서 사용한다.

문제:

- 루트 연산이 매번 들어간다
- 한 번은 작아 보여도 적 수가 많고 판정이 반복되면 누적된다

추천 방향:

- 가능한 곳은 모두 `sqrMagnitude` 비교로 교체

예:

- 추적 반경
- 공격 반경
- leash 반경
- 플레이어-스포너 거리

추천 우선순위:

- 높음

난이도 대비 효과가 좋다.

---

### 4) 플레이어 공격 판정 allocation

관련 파일:

- `Assets/02.Scripts/Player/PlayerAttack.cs`

현재 구조:

- 근접 공격 시 `Physics.OverlapBox(...)`
- 호출할 때마다 새 배열 반환

문제:

- 공격이 반복될수록 GC가 생길 수 있다
- 전투가 길어지면 프레임 드랍이 아니라 미세한 끊김으로 먼저 느껴질 가능성이 있다

추천 방향:

- `Physics.OverlapBoxNonAlloc(...)`
- 재사용 버퍼 사용

추가 권장:

- 디버그 로그도 옵션화

현재는 공격할 때 로그를 자주 찍기 때문에
에디터 플레이에서는 로그 비용도 무시하기 어렵다.

추천 우선순위:

- 높음

---

## 2. 그 다음 손댈 곳

### 5) 중간보스 구역 상태 조회 방식

관련 파일:

- `Assets/02.Scripts/Biome/MidBossArenaController.cs`
- `Assets/02.Scripts/Enemy/EnemyController.cs`

현재 구조:

- 일반 적이 추적/공격 판단할 때
- `MidBossArenaController.IsPlayerInsideLockedArena(...)`를 호출
- 활성 아레나 리스트를 검사

현재는 문제 크지 않다:

- 중간보스 구역이 사실상 1개
- static 리스트 길이도 매우 짧음

하지만 나중에:

- 구역 수가 늘거나
- 특수 구역이 여러 개 생기면

플레이어 상태를 직접 캐싱하는 쪽이 더 낫다.

추천 방향:

- `PlayerController` 또는 별도 zone state에
  - `isInsideLockedMidBossArena`
  - 같은 값을 캐싱

추천 우선순위:

- 중간

---

### 6) 맵 생성 시 아레나 내부 스포너 배치 차단 방식

관련 파일:

- `Assets/02.Scripts/Biome/ConfigurableBiomeManager.cs`
- `Assets/02.Scripts/Biome/RegionPoissonBiomeManager.cs`

현재 구조:

- 적 스포너용 density 계산 시
- 중간보스 구역 내부면 density를 0으로 만들어 배치 제외

현재는 충분히 괜찮다.

다만 더 최적화하려면:

- 아레나에 걸치는 청크/영역을 미리 계산
- 그 영역은 스포너 Poisson 후보 검사 자체를 줄이는 방향

도 가능하다.

추천 우선순위:

- 중간 이하

현재는 기능적으로 충분하다.

---

## 3. 우선순위 낮은 곳

### 7) 중간보스 안개 애니메이션

관련 파일:

- `Assets/02.Scripts/Biome/MidBossArenaController.cs`

현재 구조:

- 안개 띠 몇 개의 알파값만 `sin`으로 흔들림

이건 지금 기준 비용이 거의 없다.

왜냐하면:

- 안개 오브젝트 수가 매우 적음
- 계산도 단순함
- 보스 구역도 많지 않음

추천 우선순위:

- 낮음

---

### 8) `CharacterStats` 자체

관련 파일:

- `Assets/02.Scripts/Core/Stats/CharacterStats.cs`

현재 구조:

- 스탯 변경 시에만 재계산
- 평소엔 캐시된 최종값을 읽기만 함

즉 지금은 병목 후보가 아니다.

주의할 점은 하나 있다.

- modifier를 여러 개 한 번에 넣을 때 재계산이 여러 번 일어날 수 있음

하지만 현재 게임 규모에선
그보다 AI/스포너/물리 판정 쪽이 훨씬 먼저 병목이 된다.

추천 우선순위:

- 낮음

---

## 4. 실제 적용 우선순위 제안

### 1차 최적화 세트

지금 당장 코드에 넣을 가치가 큰 것:

1. EnemySpawner 갱신 주기 줄이기
2. EnemyController / EnemySpawner 거리 판정 `sqrMagnitude`화
3. PlayerAttack 근접 판정 `NonAlloc`화

이 세 개는

- 효과가 바로 나오고
- 위험이 비교적 낮고
- 로직을 크게 바꾸지 않는다

는 장점이 있다.

---

### 2차 최적화 세트

그 다음에 손댈 것:

1. Enemy separation spatial partition
2. 중간보스 아레나 상태 캐싱

이건 효과는 크지만 구조 변경량도 커진다.

---

## 5. 지금 기준 판단

현재 플레이에 체감 문제가 아직 없다고 했기 때문에,
지금은 “미리 과하게 최적화”보다는

- 적 수가 늘어날 때 확실히 커지는 비용
- 구조를 덜 깨고 줄일 수 있는 비용

부터 손대는 게 맞다.

즉 최적화 방향을 한 줄로 정리하면:

**AI 전체 구조를 뜯기 전에, 스포너/거리판정/공격 allocation부터 줄이고, 적 수가 더 늘어나면 separation을 구조적으로 바꾸는 순서가 가장 안전하다.**

---

## 6. 바로 다음 추천 작업

다음 작업으로 추천하는 순서:

1. `EnemySpawner.Update()`를 interval 기반으로 변경
2. 거리 비교를 `sqrMagnitude` 기반으로 치환
3. `PlayerAttack` 근접 공격을 `OverlapBoxNonAlloc`로 변경

이 세 개는 지금 상태에서 넣어도 리스크가 낮고,
보스맵/전투 구조와도 충돌이 적다.
