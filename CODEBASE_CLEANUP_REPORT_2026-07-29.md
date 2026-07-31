# Necrocis 코드베이스 정리 보고서

- 조사/정리 일자: 2026-07-29
- 조사 범위: `Assets/_Project/Scripts`, `Assets/_Project/Editor`
- 원칙: 기능, 밸런스 수치, 저장 포맷, 직렬화 참조, 런타임 성능을 바꾸지 않는 정리만 즉시 반영
- 별도 보존: 기존 저장/난이도 작업과 사용자 수정 에셋은 정리 범위에서 제외

## 1. 조사 결과 요약

| 항목 | 결과 |
|---|---:|
| 정리 전 C# 줄 수 | 44,336 |
| 정리 후 C# 줄 수 | 43,842 |
| 순감소 | 494줄 (약 1.11%) |
| 현재 C# 파일 수 | 134 |
| 코드 줄 추정치 | 36,468 |
| 주석 전용 줄 | 1,076 |
| 빈 줄 | 6,298 |
| Unity 컴파일 경고 | 0 |
| `TODO` 표시 | 3 |

줄 수 분류는 단순 정적 집계다. 여러 줄 주석 내부나 한 줄에 코드와 주석이 같이 있는 경우까지 구문 분석한 값은 아니므로, 코드 품질 추세를 확인하는 지표로만 사용한다.

현재 가장 큰 파일은 다음과 같다.

| 파일 | 줄 수 | 판단 |
|---|---:|---|
| `PlayerItemCombatEffects.cs` | 4,545 | 아이템별 상태와 효과가 한 클래스에 집중되어 탐색/병합 비용이 큼 |
| `MidBossArenaController.cs` | 1,363 | 보스 생성, 경계, 안개, 보상, 포털 책임이 한 클래스에 집중됨 |
| `PlayerController.cs` | 1,293 | 입력, 이동, 상태 연동이 함께 있어 변경 파급 범위가 큼 |
| `StomachBossPattern.cs` | 1,123 | 개별 보스 패턴 파일 중 가장 큼 |
| `LungBossPattern.cs` | 1,121 | 개별 보스 패턴 파일 중 두 번째로 큼 |
| `IntestineBossPattern.cs` | 992 | 보스 전용 패턴과 연출이 집중됨 |
| `PlayerItemManager.cs` | 912 | 카탈로그, 획득 상태, 저장 복원이 한 클래스에 있음 |
| `Projectile.cs` | 839 | 투사체 변형별 동작이 집중됨 |
| `LevelUpStackChoiceUI.cs` | 828 | 런타임 UI 생성과 선택 흐름이 함께 있음 |
| `LiverBossPattern.cs` | 826 | 보스 전용 패턴과 연출이 집중됨 |
| `PlayerAttack.cs` | 819 | 현재 사용 중인 기본 공격 구현 |
| `MainMenuController.cs` | 812 | 런타임 UI와 메뉴 상태 흐름이 함께 있음 |

## 2. 이번에 실제 반영한 정리

### 2.1 미사용 중복 구현 제거

삭제한 파일:

- `Assets/_Project/Scripts/Gameplay/Player/Combat/PlayerAttackModule.cs`
- `Assets/_Project/Scripts/Player/Skills/PlayerClassSkillController.cs`

`PlayerAttackModule`은 현재 사용 중인 `PlayerAttack`과 별도로 남아 있던 구형 공격 구현이었다. 코드 심볼 참조와 `.meta` GUID의 씬/프리팹/에셋 참조를 모두 검색했으며 참조가 없었다. `PlayerClassSkillController`는 전체가 `#if false`인 5줄짜리 백업 안내 파일이었고, 실제 구현은 `Gameplay/Player/Skills` 아래에 있다.

두 파일 모두 대응 `.meta`를 함께 삭제했다. 따라서 Unity가 같은 이름의 새 스크립트로 잘못 인식하거나, 사용하지 않는 스크립트가 검색 결과에 섞이는 문제도 없앴다.

### 2.2 런타임 UI 생성 공통화

추가한 파일:

- `Assets/_Project/Scripts/UI/RuntimeUiFactory.cs`

다음 중복 코드를 `MainMenuController`와 `InGameAudioSettings`에서 공통화했다.

- EventSystem 생성
- `RectTransform` 기반 UI 오브젝트 생성
- 이미지와 텍스트 생성
- 볼륨 슬라이더의 배경, Fill, Handle 구성
- 전체 영역 Stretch
- Anchor, 크기, 위치 지정
- 명시적 상하 버튼 탐색 연결

화면별 버튼 크기, 정렬, 색상, 패널 배치와 메뉴 흐름은 각 컨트롤러에 그대로 남겼다. 공통 함수도 기존과 같은 컴포넌트 추가 순서, 색상, 크기, 슬라이더 범위를 사용한다. 이 코드는 메뉴를 만들 때만 실행되므로 프레임 단위 비용을 추가하지 않는다.

### 2.3 의미 없는 주석 제거

다음 형태의 자동 생성형 주석을 전부 제거했다.

- “이 컴포넌트의 핵심 로직을 실행합니다.”
- “관련 설정과 상태를 구성합니다.”
- “상태 또는 컬렉션을 갱신합니다.”
- “필요한 값을 반환합니다.”
- “변경 사항을 런타임 객체에 반영합니다.”

특히 `EnemyController` 본체에는 partial 파일에 구현된 메서드 이름만 나열하는 빈 문서 블록이 대량으로 남아 있었다. 실제 필드, 프로퍼티, `Update`, 상태 전환, 공통 컴포넌트 헬퍼는 유지하고 빈 설명 블록만 제거했다. `cachedVoidShieldSprites` 필드는 동작 변경 없이 다른 정적 캐시와 같은 위치로 옮겼다.

`PlayerStats`, `StatUI`, `GameInitializer`에서도 메서드 이름을 그대로 반복할 뿐인 주석을 제거했다. 반대로 입력 키, 수치 단위, 색상 의미, 기본값 대비 증감처럼 코드만 보고 바로 알기 어려운 설명은 유지했다.

### 2.4 단순 매핑 정리

`StatUI`의 스탯명/직업명 변환을 `switch` 문에서 정적 `switch` 식으로 바꿨다. 매핑 결과는 동일하며 인스턴스 상태를 사용하지 않는다는 점이 선언에 드러난다.

## 3. 기능 및 성능 보존 확인

### 정적 확인

- 삭제 스크립트의 타입 이름과 Unity GUID를 코드, 씬, 프리팹, 에셋에서 검색
- 자동 생성형 무의미 주석 잔존 0건 확인
- `git diff --check` 통과
- Unity C# 컴파일 오류/경고 0건
- 사용자 수정 파일 `Anton SDF.asset`의 기존 차이를 변경하지 않음

### Unity 회귀 테스트

| 테스트 | 결과 | 검증 범위 |
|---|---|---|
| `SaveSystemSmokeRunner.Run` | PASS | 프로필, Normal 슬롯, Hard 해금/사망 초기화, 백업 복구 |
| `MainMenuSmokeRunner.Run` | PASS | 설정 열기/닫기, 난이도 선택, Hub 시작 |
| `PauseMenuSmokeRunner.Run` | PASS | 일시정지, 설정, 저장 후 메인 화면 |
| `HardDeathSmokeRunner.Run` | PASS | Hard 사망 데이터 삭제와 메인 화면 복귀 |
| `MainMenuSmokeRunner.RunPartial` | PASS | 일부 보스 그림자만 해제된 메인 화면 |
| `MainMenuSmokeRunner.RunRevealed` | PASS | 모든 보스 그림자가 해제된 메인 화면 |

테스트 과정에서 Unity가 자동 재직렬화한 `ProjectSettings/TimeManager.asset`은 기존 `Fixed Timestep: 0.02` 형식으로 복원했고 최종 diff가 없음을 확인했다.

### 성능 관련 정적 조사

이번 작업에서는 Unity Profiler 기반 프레임 측정을 수행하지 않았으므로 FPS나 GC 수치가 개선됐다고 주장하지 않는다. 정적으로 확인한 현재 구조는 다음과 같다.

- `Physics.*NonAlloc` 호출 10곳을 사용하고, 할당형 `OverlapSphere/OverlapBox/OverlapCapsule/RaycastAll/SphereCastAll` 호출은 발견되지 않았다.
- `RuntimePool.Acquire/Release` 사용 지점이 48곳 있어 전투 임시 오브젝트의 재사용 기반이 이미 있다.
- `PlayerItemCombatEffects.Update`는 보유 아이템 캐시 플래그로 각 효과 갱신을 건너뛰며, 아이템이 없으면 즉시 반환한다.
- 프로젝트 스크립트에는 `Update`, `LateUpdate`, `FixedUpdate`가 총 62개 있다. 개수만으로 문제라고 볼 수 없으며 실제 비용 판단에는 Profiler 계측이 필요하다.

공통 UI 함수는 초기 UI 구성 시에만 호출된다. 기존 두 구현을 하나로 옮겼을 뿐 생성하는 GameObject와 컴포넌트 수는 동일하다.

## 4. 조사했지만 이번에는 바꾸지 않은 개선안

아래 항목은 정리 효과가 있지만 변경 범위가 커서, 기능/성능 보존을 우선한 이번 작업에서는 적용하지 않았다.

| 개선안 | 기존 방식 | 제안 방식 | 장점 | 단점/위험 |
|---|---|---|---|---|
| 아이템 전투 효과 분할 | 4,545줄 한 클래스 | `partial` 파일을 투사체/방어/소환/보스전/변이 계열로 분리 | 런타임 비용 변화 없이 탐색과 Git 충돌 감소 | 필드와 중첩 타입 이동량이 커서 누락 검토가 필요하고 현재 작업과 충돌 가능 |
| 보스 아레나 분할 | 생성, 경계, 안개, 보상, 포털을 한 파일에서 처리 | `Lifecycle`, `BossRuleFactory`, `Boundary`, `FogAndReward` partial로 분리 | 책임 위치가 명확하고 보스 추가 시 수정 범위 축소 | 직렬화 필드와 실행 순서를 잘못 옮기면 보스전 회귀 가능 |
| 보스 패턴 공통 기반 확장 | 보스별 파일에 유사한 임시 VFX/타이머/타깃 처리 존재 | 공통 유틸리티 또는 조합 가능한 패턴 모듈 | 중복 감소, 패턴 테스트 단위 축소 | 보스별 예외가 추상화에 끌려가면 조정이 어려워지고 호출 계층이 늘어남 |
| Assembly Definition 도입 | 런타임 대부분이 `Assembly-CSharp`에 함께 컴파일 | `Core`, `Gameplay`, `UI`, `Editor.Tests`로 단계적 분리 | 수정 시 재컴파일 범위 축소, 의존 방향 명시 | 현재 순환 참조를 먼저 풀어야 하며 설정을 한 번에 바꾸면 컴파일 장애 범위가 큼 |
| 런타임 UI의 Prefab 전환 | 코드로 계층 생성 | 공통 Prefab과 화면별 Presenter | 에디터 미리보기, 디자이너 수정, 로컬라이징 연결이 쉬움 | Prefab/코드 양쪽 상태 동기화가 필요하고 현재 자동 재생성 장점을 잃음 |
| 모든 EventSystem 보장 코드 통합 | 화면별로 “없을 때 생성” 또는 “기존 모듈 활성화” 정책이 다름 | 정책을 매개변수화한 단일 서비스 | 중복 감소, 입력 모듈 누락 방지 | 기존 화면별 정책 차이를 먼저 테스트해야 하며 잘못 합치면 UI 입력이 중복될 수 있음 |
| 자동 플레이 모드 테스트 확대 | 저장/메뉴/사망 흐름 중심 Smoke Test | 전투, 아이템, 보스 패턴별 Edit/PlayMode Test | 대형 파일 분할과 밸런스 수정의 안전성 상승 | 테스트용 씬/시간 제어/랜덤 고정 기반을 추가해야 함 |

### 권장 적용 순서

1. `PlayerItemCombatEffects`를 동작 수정 없이 partial 파일로만 분리한다.
2. 아이템 획득/제거, 투사체 변형, 지속 효과의 PlayMode 회귀 테스트를 추가한다.
3. `MidBossArenaController`를 책임별 partial 파일로 분리하고 4종 보스 입장/클리어 테스트를 추가한다.
4. 순환 의존성을 확인한 뒤 Assembly Definition을 `Core`부터 단계적으로 도입한다.
5. 실제 기기 Unity Profiler 캡처를 기준으로만 프레임 단위 최적화를 진행한다.

## 5. 남아 있는 명시적 TODO

| 파일 | 내용 | 성격 |
|---|---|---|
| `SceneLoader.cs` | 페이드 아웃 효과 추가 가능 | 선택적 연출 개선 |
| `SceneLoader.cs` | 페이드 인 효과 추가 가능 | 선택적 연출 개선 |
| `Altar.cs` | 실제 대뇌 맵 로드 | 미완성 콘텐츠 연결 |

이 세 항목은 불필요한 주석이 아니라 아직 구현되지 않은 동작을 표시하므로 삭제하지 않았다.

## 6. 다음 정리 작업 체크리스트

- [ ] 대상 타입의 코드 참조와 `.meta` GUID 직렬화 참조를 모두 검색한다.
- [ ] 직렬화 필드 이름과 타입을 유지한다.
- [ ] `Awake`/`OnEnable`/`Start` 호출 순서를 바꾸지 않는다.
- [ ] `Update` 경로에는 LINQ, 새 컬렉션, 새 문자열 조합을 추가하지 않는다.
- [ ] 오브젝트 풀 Acquire/Release 쌍과 이벤트 구독/해제 쌍을 확인한다.
- [ ] Unity 컴파일 경고를 0으로 유지한다.
- [ ] 저장, 메인 메뉴, 일시정지, 난이도, 보스 그림자 회귀 테스트를 다시 실행한다.
- [ ] 성능 개선을 주장할 때는 동일 장면/동일 조건의 Profiler 전후 캡처를 남긴다.
