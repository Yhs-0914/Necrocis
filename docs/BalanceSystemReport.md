# Necrocis 밸런스 시스템 변경 보고

작업 브랜치: `gustlr02` (시작 커밋 `d1bf8ece28c86c9f9fd273efd698f951e42262c6`).
최초 작업 폴더는 비어 있었으며, 해당 브랜치만 clone하고 fetch 후 HEAD와 원격 SHA 일치를 확인했다. 기존 미커밋 변경은 없었다. main checkout·수정·push·merge·PR은 수행하지 않는다.

## 구현 및 기존 동작

| 영역 | 결과 |
|---|---|
| 레벨 | 최대 30 유지. 기본 설정 및 Resources 에셋의 직업 선택/1번 스킬은 5, 2번 스킬은 10으로 변경 |
| 10레벨 성장 | 직업을 유지하며 기존 2번 스킬 해금. `GetGrowthStage()`로 전직 전/1차/2차 단계 조회. 새 직업·스킬·추가 능력치 없음 |
| 경험치 | 필요 경험치 공식, 처치 경험치, 구간 배율 수치 유지. 직업 선택 대기는 초기 구간 배율보다 우선. 대량 경험치는 5에서 멈추고 이미 얻은 초과분은 직업 선택과 기존 UI 대기열 처리 후 소비 |
| 직업 선택 | 기존 ClassChoiceUI/LevelUpUI/PlayerClassSkillController 호출 유지. 5 이전 선택, 중복 전직, 잘못된 JobType 방지. 직업별 고정 스탯 선택 시작 레벨은 기존 11 유지 |
| 아이템 티어 | 카탈로그와 획득 인스턴스에 0~4 티어 저장/조회. 기존 데이터는 0. 기존 ID·장착 슬롯·효과 적용·제거 경로 유지. 티어 자체는 효과를 증폭하지 않음 |
| 바이옴 수량 | WorldItemSpawner의 `biomeSpawnCounts`에서 바이옴별 최초 바닥 생성 수량 설정. 미지정은 기존 spawnCount, 0은 생성 없음. 기존처럼 카탈로그 후보 수/배치 가능한 위치 수가 상한 |
| 엘리트 타이밍 | EnemySpawnRuleConfig의 `eliteAttackCooldown`, `eliteAttackFrameInterval`로 일반/엘리트 타이밍 분리. 각각 음수면 기존 attackCooldown/attackAnimationSpeed 상속. 돌진 cooldown은 기존 3초를 Inspector 필드로 노출 |
| 난이도 | 기존 GameDifficulty에 Hard 추가. 기존 GameManager에서 Normal/Hard 각각 일반/엘리트 체력·공격력·일반 웨이브 수량 프로필 제공. 모든 초기 배율은 1. `TrySetDifficulty`는 허브에서만 허용 |
| 런타임 설정 | 일반 스포너, 엘리트 등록, 분열 적에 원본의 런타임 복사본 적용. 원본 에셋 수치 및 추가 스탯 리스트를 변경하지 않음 |
| 보스 | 보스 전용 설정/패턴/씬/프리팹 미수정. 난이도 배율 적용 경로에서 제외. PlayAttackAnimationOnly/PlayPatternAnimation의 기존 애니메이션 설정 유지 |

## 엘리트 공격속도 분석

`SpriteFrameAnimator`는 attackAnimationSpeed를 **프레임당 초**로 사용한다. 값을 작게 하면 빨라진다. 기존 장 바이옴 설정에서 Granuloma는 cooldown 1초/프레임 간격 0.12초, Antibody는 0.8초/0.1초다. 일반 적 cooldown은 1~1.5초다.

공격 애니메이션이 있으면 완료 후 cooldown이 시작하므로 공격 간격은 대략 `프레임 수 × 프레임 간격 + cooldown`이다. 프레임 지연과 공격 범위 이탈에 따라 실제 간격은 달라진다. 애니메이션이 없으면 피해 직후 cooldown을 적용한다. 생성 직후 attackTimer=0이므로 첫 공격은 즉시 가능하다. EliteSpawner는 별도 공격 가속을 적용하지 않는다. 여러 엘리트의 동시 공격과 항체 돌진 접근도 체감 빈도에 영향을 줄 수 있으나, 실제 전투 재현으로 확인하지는 않았다.

따라서 임의 감속 수치를 적용하지 않고 설정 분리와 단위 설명을 추가했다. 새 override를 지정하기 전까지 기존 공격속도는 동일하다.

## 검토 완료·적용 보류

- **15·20·25·30 보상:** 기존의 레벨업 스탯 선택은 유지한다. 별도 마일스톤 보상은 내용이 미확정이므로 추가하지 않았다. 후속 설계는 이 레벨들의 보상을 데이터로 정의하고, 대량 경험치 획득 시 각 보상이 정확히 한 번 제공되는 구조가 필요하다.
- **동일 아이템 강화:** 현재 기본값은 중복 불허다. 기존 allowDuplicateItems를 켜는 것은 강화가 아니라 슬롯을 차지하는 복수 획득이다. PlayerItemBase 효과가 에셋을 modifier source로 사용하므로 한 개 제거가 같은 source의 모든 효과를 제거할 수 있고, ID 기반 전투 효과도 스택 수를 반영하지 않는다. 강화 정책·최대 스택·수치 확정 전 활성화하지 않았다. 구현 시 획득 인스턴스별 source와 스택 기반 전투 효과가 필요하다.
- **드롭과 바닥 생성:** 최초 바닥 상자와 기존 보스 처치 아이템 드롭을 유지한다. 바닥 생성은 바이옴별 획득량을 제한하기 쉽고, 처치 드롭은 처치 속도에 따라 획득량이 변한다. 일반 적 드롭 확률·보장 횟수가 없으므로 새로운 드롭은 추가하지 않았다.
- **3개 중 1개:** 기존 아이템 카탈로그에서 중복/슬롯 조건을 통과한 서로 다른 후보 3개를 제시하는 방식을 검토했다. 선택 때 획득을 다시 검증하고 성공한 한 개만 지급해야 한다. 아이템 후보 부족, 슬롯 교체, 선택 취소 및 레벨업 UI와의 일시정지 소유권이 필요하다. 새 선택 UI와 지급 방식은 추가하지 않았다.
- **티어와 난이도:** 티어 확률·강화 배율·Hard의 실제 수치는 미적용이다. 티어 표시 UI, 난이도 선택 메뉴와 저장도 이번 범위에 포함하지 않았다. 난이도 수량 배율은 일반 스포너 maxAlive에 적용하며 엘리트 킬 소환 간격·분열 수·보스 수·바닥 아이템 수에는 적용하지 않는다.

## Inspector 설정 위치

- LevelProgressionConfig Resources 에셋: 5/10 해금 설정. 기존 경험치·레벨업 스탯 값 유지.
- PlayerItemManager → Item Entries → Tier: 아이템별 0~4.
- WorldItemSpawner → Biome Spawn Counts: 바이옴/수량 override 목록. 기존 범위 제한과 위치 분산 규칙도 적용된다.
- EnemySpawnConfig → 각 Enemy Spawn Rule: 기존 일반 공격 타이밍, 엘리트 override, Charge Cooldown.
- GameManager → Difficulty Balance: Normal Balance/Hard Balance의 일반·엘리트 프로필. 새 수치를 지정하지 않으면 기존과 동일하다. 시작 전 Current Difficulty 또는 허브에서 TrySetDifficulty로 선택한다.

## 검증 결과

- Unity 6000.3.9f1 배치 컴파일: 수정 전 통과, 수정 후 C# 컴파일 오류 없음.
- 사용자 정의 Editor runner(UTF NUnit suite가 아님): EditMode 109개 검사 통과. 빈 임시 씬의 실제 PlayMode에서도 같은 109개 검사 통과(`playing=True`).
- 검사 내용: Resources 설정, 스킬 경계 4/5/9/10/30과 세 직업, 대량 경험치/직업 대기/잔여 경험치, 레벨 상한, 중복 전직 차단, 티어 보존/범위, 바이옴 0/기본/개별 수량, 엘리트 override, 중립 난이도/설정 배율/원본 불변성/추가 스탯, 허브 난이도 변경 제한.
- 수정 전 회귀 검사에서 `Default job selection must be level 5` 실패를 확인했고, 다음 단계에서 티어 API 누락 실패도 확인했다.
- PlayMode 시작 중 UnityEditor.Search.SearchDatabase.EnumerateAll에서 ArgumentOutOfRangeException이 발생했다. 스택은 Unity Editor Search 내부이며 게임 코드 프레임이 없다. 환경/에디터 측 문제로 추정하되 원인 확정이나 수정은 하지 않았다. 첫 PlayMode 시도는 이 예외로 delayCall 기반 runner 시작이 누락되어 종료하고, update 기반 runner로 재실행하여 검사 통과를 확인했다. 예외 자체는 재실행에도 남았다.
- 기존 AudioManager의 스킬 사용 SFX 리소스 누락 경고, 라이선싱 토큰 갱신 오류 로그가 관찰됐으며 에디터 실행/검사는 진행됐다.
- **미검증:** 전체 바이옴에서의 UI 클릭·E/R 발동·아이템 획득/교체·엘리트 체감 공격 간격·보스 패턴 시각 회귀, 플레이어 빌드. 빈 씬의 API/설정 검사는 이를 대신하지 않는다.
- Unity import가 자동 변경한 Anton SDF 에셋, slnx, BuildSettings, ShaderGraphSettings는 초기 HEAD 내용으로 복구한다. 기존 씬·프리팹·GUID는 유지한다.
- 별도 코드 리뷰: 확인된 치명/중요 결함 없음. 실제 UI 패널 클릭과 최종 timeScale 복원은 전체 씬 검증 항목으로 남는다.

재현 명령(프로젝트 루트에서):

```powershell
& 'C:/Program Files/Unity/Hub/Editor/6000.3.9f1/Editor/Unity.exe' -batchmode -nographics -projectPath . -executeMethod NecrocisEditor.BalanceValidationRunner.Run -logFile ../necrocis-edit-validation.log
& 'C:/Program Files/Unity/Hub/Editor/6000.3.9f1/Editor/Unity.exe' -batchmode -nographics -projectPath . -executeMethod NecrocisEditor.BalanceValidationRunner.RunPlayMode -logFile ../necrocis-play-validation.log
```

다음 실행 행동: 머지 전 30분간 장 바이옴에서 5→10레벨 플레이 검증. 완료 기준은 직업 UI 1회, E/R 순차 해금, 선택 전 초과 경험치 보존, 보스 패턴 회귀 오류 0건이다.

## 변경 파일 목록

수정:
- `Assets/_Project/Resources/Balance/LevelProgressionConfig.asset`
- `Assets/_Project/Scripts/Gameplay/Player/Progression/LevelProgressionConfig.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Progression/LevelUpManager.cs`
- `Assets/_Project/Scripts/Gameplay/Items/PlayerItemManager.cs`
- `Assets/_Project/Scripts/Gameplay/Items/WorldItemSpawner.cs`
- `Assets/_Project/Scripts/World/Biomes/Configs/BiomeConfig.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyCombat.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyMovement.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyEliteDeathHandler.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Spawning/EnemySpawner.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Spawning/EliteSpawner.cs`
- `Assets/_Project/Scripts/Core/Bootstrap/GameManager.cs`
- `Assets/_Project/Scripts/Core/Types/BiomeType.cs`

신규:
- `Assets/_Project/Scripts/Gameplay/Enemies/Core/EnemyBalanceMultipliers.cs` 및 `.meta`
- `Assets/_Project/Editor/BalanceValidationRunner.cs` 및 `.meta`
- `docs/UnityProjectContext.md`
- `docs/superpowers/plans/2026-11-04-balance.md`
- `docs/BalanceSystemReport.md`

Commit SHA와 Push 결과는 실행 후 최종 응답에서 보고한다.
