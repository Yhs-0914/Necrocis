# Necrocis 작업 정리

## 작업 개요

이번 통합 작업은 `main`, `gustlr`, `kjh4845` 변경분을 합치면서 플레이어/적 전투 시스템, 보스 시스템, 포털 정책, 바이옴 설정, 성능 리스크, 대형 클래스 구조, 폴더 구조를 같이 정리한 작업이다.

최종 기준:

- 보스/보스맵/보스 안개는 `kjh4845` 작업 버전을 기준으로 유지했다.
- UI 쪽은 `main`에 올라온 변경을 기준으로 통합했다.
- 포털 시스템과 8방향 공격 스프라이트는 `gustlr` 변경을 가져왔다.
- 보스 몹 구현은 기존 `kjh4845` 장 보스 작업을 유지했다.

## Boss Create 작업 요약

이번 작업에서는 장 보스 기준으로 간, 위, 폐 보스를 추가하고 보스 공통 전투 흐름을 정리했다.

추가 보스:

- 간 보스: 혈액 폭탄 투척, 공격력 감소 디버프, 2페이즈 회복 포즈 패턴을 구현했다.
- 위 보스: 1페이즈 돌진, 2페이즈 소화액 원거리 공격과 흡입/토해내기 단거리 공격을 구현했다.
- 폐 보스: 형제 보스 구조를 추가하고, 한 명이 죽으면 남은 보스가 광폭화되는 2페이즈를 구현했다.

보스방/진행 규칙:

- 장, 간, 위, 폐 보스 모두 보스방 가두리와 안개 설정을 공통 구조로 사용한다.
- 보스 처치 후 귀환 포털은 보스 사망 위치가 아니라 보스방 중앙에 생성된다.
- 허브에서 클리어된 바이옴 포털은 회색 비활성 상태로 표시된다.
- 보스가 공격 또는 시전 중 사망하면 남아 있는 투사체, 임시 이펙트, 소환물을 즉시 정리한다.

전투 판정/체력 UI:

- 플레이어 원거리 공격이 보스나 일반 몹을 단일 타격했을 때 관통하지 않고 사라지도록 보정했다.
- 플레이어 HP는 정수 단위로만 저장, 피해, 회복되도록 정리했다.
- 하트 UI는 `1 HP = 반칸` 기준으로 정수 HP와 일치하게 표시된다.
- 플레이어에게 들어가는 보스 피해 기본값과 데미지 틱 기본값에서 소수점 피해를 제거했다.

관련 코드 및 설정:

- `Assets/_Project/Scripts/Gameplay/Enemies/Bosses/IntestineBossPattern.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Bosses/LiverBossPattern.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Bosses/StomachBossPattern.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Bosses/LungBossPattern.cs`
- `Assets/_Project/Scripts/World/Biomes/BossArena/MidBossArenaController.cs`
- `Assets/_Project/Scripts/World/Hub/HubBiomePortal.cs`
- `Assets/_Project/Scripts/World/Hub/HubRoom.cs`
- `Assets/_Project/Scripts/Core/Stats/CharacterStats.cs`
- `Assets/_Project/Scripts/UI/HUD/PlayerHeartUI.cs`
- `Assets/_Project/Data/BiomeConfigs/*BossArenaConfig.asset`

## 스탯 시스템

플레이어 기본 스탯을 `PlayerStats` 중심으로 다시 정리했다.

플레이어 기본 스탯:

- 체력
- 이동속도
- 공격력
- 공격 속도
- 공격 사거리
- 마력
- 스킬 쿨타임 감소

적용 규칙:

- 공격력은 기본 공격 계열 데미지에만 적용된다.
- 공격 속도와 공격 사거리는 기본 공격에만 적용된다.
- 마력은 스킬 데미지 증가에만 적용된다.
- 스킬 쿨타임 감소는 퍼센트 기반이며 기본값은 0%다.
- 방어력은 플레이어 스탯, 레벨업, UI 계산에서 제거했다.
- 레벨업으로 바뀐 스탯도 `PlayerStats`를 기준으로 다시 계산되도록 파이프라인을 확인했다.

관련 코드:

- `Assets/_Project/Scripts/Gameplay/Player/Runtime/PlayerStats.cs`
- `Assets/_Project/Scripts/Core/Stats/CharacterStats.cs`
- `Assets/_Project/Scripts/Core/Stats/PlayerItemStatDefinitions.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Combat/PlayerCombatCalculator.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Core/EnemyCombatCalculator.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Progression/LevelUpManager.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Progression/LevelUpStatCatalog.cs`
- `Assets/_Project/Scripts/UI/HUD/StatUI.cs`
- `Assets/_Project/Scripts/UI/LevelUp/LevelUpUI.cs`

## 아이템 베이스

ScriptableObject 기반 플레이어 아이템 베이스를 추가했다.

- 아이템에서 플레이어 기본 스탯 7종을 공통 타입으로 사용할 수 있다.
- 아이템 스탯 타입 이름을 `PlayerItemStatType`으로 정리해 플레이어 런타임 스탯과 구분했다.
- 아이템 스탯 수정 데이터 이름을 `PlayerItemStatModifierData`로 정리했다.
- 아이템 적용 함수 이름도 `ApplyPlayerItemStatModifiers`로 명확하게 바꿨다.

관련 코드:

- `Assets/_Project/Scripts/Gameplay/Items/PlayerItemBase.cs`
- `Assets/_Project/Scripts/Core/Stats/PlayerItemStatDefinitions.cs`

## 플레이어/적 계산식 분리

플레이어와 적의 전투 계산식을 분리했다.

- 플레이어는 기본 공격과 스킬 데미지 계산을 분리한다.
- 적은 필요한 전투 계산만 `EnemyCombatCalculator`에서 유지한다.
- 플레이어 스탯 확장과 적 스탯 확장이 서로 영향을 덜 받도록 분리했다.

관련 코드:

- `Assets/_Project/Scripts/Gameplay/Player/Combat/PlayerCombatCalculator.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Core/EnemyCombatCalculator.cs`

## 스킬 판정

스킬 범위 판정에서 타격 대상 수를 명시적으로 제한하도록 정리했다.

- 일정 수 이상은 맞지 않는 것이 의도라서 최대 타격 수를 설정값으로 노출했다.
- 범위 판정은 불명확한 누락이 아니라 설정된 최대 타격 수 기준으로 동작하게 정리했다.
- 스킬 데미지는 마력 기반 증가가 적용된다.

관련 코드:

- `Assets/_Project/Scripts/Gameplay/Player/Skills/PlayerClassSkillController.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Skills/Runtime/SkillTargeting.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Skills/Runtime/SkillProjectilesAndEffects.cs`

## 기본 공격 최적화

기본 공격 근접 판정의 매 공격 배열 할당을 줄였다.

- `Physics.OverlapBox` 대신 `Physics.OverlapBoxNonAlloc`을 사용한다.
- 전용 `LayerMask`를 통해 기본 공격 판정 대상을 제한할 수 있다.
- 중복 적 타격 방지를 위한 `HashSet<EnemyController>`를 사용한다.
- 전투 중 반복 로그는 디버그 옵션으로 제어하도록 정리했다.

관련 코드:

- `Assets/_Project/Scripts/Gameplay/Player/Combat/PlayerAttack.cs`

## 런타임 풀 최적화

런타임 풀 재사용 시 하위 컴포넌트를 매번 다시 찾던 구조를 줄였다.

- `PooledRuntimeObject`가 파티클, 애니메이터, 애니메이션 컴포넌트를 최초 1회 캐시한다.
- 재사용 시 캐시된 컴포넌트 배열을 기준으로 재시작한다.
- 투사체와 이펙트 풀에서 반복 `GetComponentsInChildren` 비용을 줄였다.

관련 코드:

- `Assets/_Project/Scripts/Core/Pooling/RuntimePool.cs`

## 포털 정책

바이옴 입장 후에는 입장 포털이 사라지고, 보스를 잡아야 허브로 돌아갈 수 있도록 정책을 정리했다.

- 청크에 일반 귀환 포털이 남지 않도록 정리했다.
- 보스 처치 후 보스 귀환 포털만 허브 복귀 수단으로 유지했다.
- `gustlr` 브랜치의 포털 시스템은 유지하되 보스 몹은 `kjh4845` 버전을 기준으로 통합했다.

관련 코드:

- `Assets/_Project/Scripts/World/Biomes/Portals/ReturnPortal.cs`
- `Assets/_Project/Scripts/World/Hub/HubBiomePortal.cs`
- `Assets/_Project/Scripts/World/Biomes/Runtime/BiomeManager.cs`
- `Assets/_Project/Scripts/World/Biomes/BossArena/MidBossArenaController.cs`

## 장 보스

장 보스 임시 패턴을 구현했다.

1페이즈:

- 플레이어를 피해 이동한다.
- 0.5초 선딜 후 배설물을 배출한다.
- 배설물은 충돌 시 1HP 데미지를 준다.
- 배설물은 3초 동안 이동속도 20% 감소 디버프를 준다.
- 배설물에서 기생충 2~3마리가 생성된다.
- 스킬 쿨타임은 8초다.

2페이즈:

- 기본 기준은 체력 50% 이하 진입이다.
- 분노 전환 연출 후 2페이즈로 들어간다.
- 플레이어를 따라다니며 배설물을 투척한다.
- 제자리 점프 후 충격파를 발생시킨다.
- 충격파 반지름은 6 유닛이다.
- 충격파는 2HP 데미지를 준다.
- 충격파는 3초 동안 이동속도 30% 감소 디버프를 준다.
- 충격파 최소 딜레이는 7초다.

보스 설정:

- 장 보스는 최소 체력 500, 스케일 배율 4배 기준으로 설정했다.
- 보스별 설정 에셋에서 보스 체력, 스케일, 패턴 설정을 바꿀 수 있다.
- 보스 패턴 검증용 ContextMenu를 추가했다.
- 1페이즈, 2페이즈를 빠르게 검증할 수 있는 디버그 옵션을 추가했다.

관련 코드 및 설정:

- `Assets/_Project/Scripts/Gameplay/Enemies/Bosses/IntestineBossPattern.cs`
- `Assets/_Project/Scripts/World/Biomes/BossArena/MidBossArenaController.cs`
- `Assets/_Project/Scripts/Gameplay/Player/StatusEffects/PlayerStatusEffectController.cs`
- `Assets/_Project/Data/BiomeConfigs/IntestineBossArenaConfig.asset`

## 보스 안개

보스방 안개 연출을 정리했다.

- 보스방만 안개 이미지로 덮도록 범위를 조정했다.
- 안개가 보스방 내부 오브젝트를 가려서 입장 전에는 안쪽이 보이지 않게 했다.
- 진입 후 안개 이미지가 옅어지면서 테두리 역할을 하도록 구성했다.
- 보스방 가두리는 안개 안쪽 기준으로 맞췄다.
- 특정 시점 이후 안개가 사라져 보이는 문제를 수정했다.
- Scene 기준으로 보스방 범위와 수평하게 맞도록 크기와 배치를 조정했다.

관련 설정:

- `Assets/_Project/Data/BiomeConfigs/*BossArenaConfig.asset`
- `Assets/_Project/Scripts/World/Biomes/BossArena/MidBossArenaController.cs`

## 바이옴 설정 분리

`BiomeConfig`에 몰려 있던 설정을 역할별로 분리했다.

`BiomeConfig` 담당:

- 바이옴 맵 기본 설정
- 타일/지역 설정
- 오브젝트 스폰 규칙
- 귀환 포털 설정

`EnemySpawnConfig` 담당:

- 적 스폰 규칙
- 적 개별 스탯
- 적 스프라이트
- 엘리트 특수 설정

`BossArenaConfig` 담당:

- 보스 아레나 설정
- 보스 선택/fallback 규칙
- 보스 체력/스케일 오버라이드
- 보스 패턴 설정
- 보스 안개/가두리 설정

관련 코드:

- `Assets/_Project/Scripts/World/Biomes/Configs/BiomeConfig.cs`
- `Assets/_Project/Scripts/World/Biomes/Configs/EnemySpawnConfig.cs`
- `Assets/_Project/Scripts/World/Biomes/Configs/BossArenaConfig.cs`
- `Assets/_Project/Scripts/World/Biomes/Runtime/ConfigurableBiomeManager.cs`

관련 설정 에셋:

- `Assets/_Project/Data/BiomeConfigs/*BiomeConfig.asset`
- `Assets/_Project/Data/BiomeConfigs/*EnemySpawnConfig.asset`
- `Assets/_Project/Data/BiomeConfigs/*BossArenaConfig.asset`

## 대형 클래스 분리

유지보수를 위해 큰 클래스를 partial 구조와 역할별 파일로 나눴다.

`PlayerClassSkillController` 분리:

- `Assets/_Project/Scripts/Gameplay/Player/Skills/PlayerClassSkillController.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Skills/Execution/MageSkillExecution.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Skills/Execution/ArcherSkillExecution.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Skills/Execution/WarriorSkillExecution.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Skills/Runtime/SkillProjectilesAndEffects.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Skills/Runtime/SkillTargeting.cs`

`EnemyController` 분리:

- `Assets/_Project/Scripts/Gameplay/Enemies/Core/EnemyController.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyMovement.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyCombat.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyVisual.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyEliteDeathHandler.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyPooling.cs`

`BiomeManager` 분리:

- `Assets/_Project/Scripts/World/Biomes/Runtime/BiomeManager.cs`
- `Assets/_Project/Scripts/World/Biomes/Runtime/BiomeNavigation.cs`
- `Assets/_Project/Scripts/World/Biomes/Runtime/BiomeRuntimeTypes.cs`
- `Assets/_Project/Scripts/World/Biomes/Runtime/Chunks/ChunkTileRenderer.cs`
- `Assets/_Project/Scripts/World/Biomes/Runtime/Chunks/ChunkCleanupService.cs`
- `Assets/_Project/Scripts/World/Biomes/Runtime/Objects/BiomeObjectPool.cs`
- `Assets/_Project/Scripts/World/Biomes/Runtime/Objects/ChunkObjectSpawner.cs`

## 폴더 구조 정리

스크립트 폴더를 역할 기준으로 다시 정리했다.

- `Core`: 부트스트랩, 입력, 풀링, 렌더링 보조, 타입, 공통 스탯
- `Gameplay/Player`: 플레이어 런타임, 전투, 성장, 스킬, 상태이상
- `Gameplay/Enemies`: 적 코어, 행동, 전투, FSM, 스폰, 상태이상, 보스, VFX
- `Gameplay/Items`: 아이템 ScriptableObject 기반 코드
- `World/Biomes`: 바이옴 설정, 런타임, 청크, 오브젝트, 보스 아레나, 포털
- `World/Hub`: 허브 맵, 방, 제단, 바이옴 포털
- `UI/HUD`: 체력, 경험치, 스탯, 스킬 쿨타임 UI
- `UI/LevelUp`: 레벨업 선택 UI
- `Presentation/Camera`: 카메라와 화면 표현 보조

이름이 겹치거나 역할이 애매한 파일도 같이 정리했다.

- `ObjectPooler` -> `PlayerProjectilePool`
- `Portal` -> `HubBiomePortal`
- `StatManager` -> `LevelUpStatCatalog`
- `ClassChoice` -> `ClassChoiceUI`
- `StackChoice` -> `LevelUpStackChoiceUI`
- `PerlinNoise` -> `BiomePerlinNoise`
- `AnimatedSprite` -> `SpriteFrameAnimator`
- `PlayerStatDefinitions` -> `PlayerItemStatDefinitions`

## 2026-05-19 추가 작업

플레이어 사망, 피격 피드백, 피격 판정, 체력 UI 표시 기준을 다시 정리했다.

### 플레이어 사망 처리

- 노말 난이도 기준을 `GameDifficulty.Normal`로 추가했다.
- 노말 난이도에서 플레이어가 사망하면 레벨과 스탯을 초기화하지 않고 허브로 복귀하도록 구성했다.
- 사망 시 `PlayerDeathScreen`을 통해 사망 화면을 표시한다.
- 사망 화면은 `Resources/UI/Death/death_screen.png` 이미지를 사용한다.
- 이미지 안의 `Return to Hub` 영역을 투명 버튼으로 살려서 허브 복귀 입력으로 사용한다.
- HP가 0이 되면 이동 입력, 물리 속도, 기본 공격, 스킬 입력을 즉시 차단한다.
- 사망 애니메이션 스프라이트가 할당되어 있으면 애니메이션 재생 후 사망 화면을 표시하고, 없으면 바로 사망 화면으로 넘어간다.
- 허브 복귀 시 바이옴에서 사용하던 Y 위치 잠금을 해제한 뒤 허브 스폰 위치로 이동한다.

관련 코드 및 리소스:

- `Assets/_Project/Scripts/Gameplay/Player/Runtime/PlayerController.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Runtime/Health.cs`
- `Assets/_Project/Scripts/UI/Death/PlayerDeathScreen.cs`
- `Assets/_Project/Resources/UI/Death/death_screen.png`
- `Assets/_Project/Scripts/Core/Bootstrap/GameInitializer.cs`
- `Assets/_Project/Scripts/Core/Bootstrap/GameManager.cs`
- `Assets/_Project/Scripts/Core/Bootstrap/SceneLoader.cs`
- `Assets/_Project/Scripts/Core/Types/BiomeType.cs`

### 피격 피드백과 무적 시간

- 플레이어가 데미지를 받으면 스프라이트가 짧게 붉은색으로 깜빡인다.
- 피격 후 기본 무적 시간은 0.5초다.
- 무적 중에는 추가 데미지를 받지 않는다.
- 사망, 비활성화, 체력 리셋 시 스프라이트 색상과 무적 상태를 원래대로 복구한다.
- 레벨업 등 외부에서 부여하는 임시 무적은 플래시 없이 무적만 적용한다.

관련 코드:

- `Assets/_Project/Scripts/Gameplay/Player/Runtime/Health.cs`

### 플레이어 피격 판정 정리

- 잔몹 근접 공격은 실제 공격 가능 범위와 콜라이더 기반 접촉 범위를 함께 사용하도록 보정했다.
- 근접 공격은 공격 애니메이션 종료 시점에 플레이어가 여전히 유효 범위 안에 있을 때만 데미지를 적용한다.
- 잔몹 원거리 투사체는 이전 프레임 위치와 현재 위치 사이의 경로를 검사해 빠른 투사체가 플레이어를 통과하는 누락을 줄였다.
- 투사체 판정 반경은 고정 과대값 대신 투사체 스케일 기반으로 계산하고 최소값만 둔다.
- 기본 공격 입력도 플레이어 HP가 0이면 더 이상 처리하지 않도록 차단했다.

관련 코드:

- `Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyMovement.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyCombat.cs`
- `Assets/_Project/Scripts/Gameplay/Enemies/Combat/EnemyProjectile.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Combat/PlayerAttack.cs`

### 체력 UI와 잔몹 데미지 기준

- 최종 기준은 하트 1개 = 2 HP다.
- 잔몹 기본 데미지는 1로 맞췄다.
- 이 기준에서 잔몹 피격 1회는 하트 반 칸 감소로 표시된다.
- `PlayerHeartUI`는 `Health.OnHealthChanged` 이벤트의 실제 현재 HP와 최대 HP 값을 받아 표시한다.
- Hub 씬에 직렬화된 `PlayerHeartUI` 설정도 하트 1개 = 2 HP 기준으로 맞췄다.
- 장 바이옴 일반 적 설정과 이전 BiomeConfig 내부 enemy rule fallback 값도 같은 데미지 기준으로 맞췄다.
- 보스 데미지는 별도 보스 설정값을 유지했다.

관련 코드 및 설정:

- `Assets/_Project/Scripts/UI/HUD/PlayerHeartUI.cs`
- `Assets/_Project/Scenes/Hub.unity`
- `Assets/_Project/Data/BiomeConfigs/IntestineEnemySpawnConfig.asset`
- `Assets/_Project/Data/BiomeConfigs/IntestineBiomeConfig.asset`
- `Assets/_Project/Scripts/World/Biomes/Configs/BiomeConfig.cs`

### 카메라와 HUD 위치 조정

- 메인 카메라 기준에서 플레이어가 화면 중앙에 오도록 카메라 추적 오프셋 계산을 조정했다.
- 플레이어 체력 UI 크기를 줄이고 좌상단 위치를 정리했다.

관련 코드:

- `Assets/_Project/Scripts/Presentation/Camera/DontStarveCamera.cs`
- `Assets/_Project/Scripts/UI/HUD/PlayerHeartUI.cs`

## 설정 위치

플레이어 기본 스탯:

- `Assets/_Project/Scripts/Gameplay/Player/Runtime/PlayerStats.cs`

아이템에서 쓰는 플레이어 스탯 타입:

- `Assets/_Project/Scripts/Core/Stats/PlayerItemStatDefinitions.cs`

공통 캐릭터 스탯 데이터:

- `Assets/_Project/Scripts/Core/Stats/CharacterStats.cs`

적 기본 수치와 스폰 규칙:

- `Assets/_Project/Data/BiomeConfigs/*EnemySpawnConfig.asset`

보스 패턴, 체력, 스케일, 안개, 아레나:

- `Assets/_Project/Data/BiomeConfigs/*BossArenaConfig.asset`

바이옴 맵, 오브젝트, 포털 기본 설정:

- `Assets/_Project/Data/BiomeConfigs/*BiomeConfig.asset`

## 검증

컴파일 검증:

```bash
dotnet build Assembly-CSharp.csproj -v:minimal /p:FrameworkPathOverride=/Applications/Unity/Hub/Editor/6000.3.9f1/Unity.app/Contents/Resources/Scripting/MonoBleedingEdge/lib/mono/4.7.1-api
```

결과:

- 빌드 성공
- 신규 컴파일 오류 없음
- 남은 경고는 `PlayerClassSkillController`의 스킬 prefab 미할당 경고 9개다.

문서 커밋 전 검증:

```bash
git diff --check
```
