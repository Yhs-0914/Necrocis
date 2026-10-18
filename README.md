# Necrocis 작업 정리

인체의 장기를 배경으로 한 Unity 2.5D 액션 게임이다. 허브에서 장·간·위·폐 지역을 탐험하고, 적을 처치하며 성장해 보스를 상대한다.

## 작업 개요

기존 통합 작업에서는 `main`, `gustlr`, `kjh4845` 변경분을 합치면서 플레이어/적 전투 시스템, 보스 시스템, 포털 정책, 바이옴 설정, 성능 리스크, 대형 클래스 구조, 폴더 구조를 같이 정리했다. 아래 작업 기록에 현재 코드와 달라진 처리 규칙과 경로를 반영했다.

기존 통합 기준:

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

- 관통 효과가 없는 플레이어 원거리 공격은 보스나 일반 몹을 한 번 타격한 뒤 사라진다.
- 실제 HP는 정수 단위로 처리한다. 소수 피해·회복은 피격자와 효과 출처별로 따로 누적해 합계 1 이상일 때 적용하며, 피해와 회복 잔여량은 서로 상쇄하지 않는다.
- 하트 UI는 `1 HP = 반칸` 기준이며, 최대 체력이 24 HP를 넘으면 여러 줄로 확장한다.
- 보스 피해와 지속 피해는 각 설정값을 사용한다. 소수량도 버리지 않고 누적한다.

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
- 레벨업으로 바뀐 스탯도 `PlayerStats`를 기준으로 다시 계산한다.
- 레벨업 선택지는 현재 규칙인 6종 중 무작위 5개 제시를 사용한다.

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

- 물리 범위 검색은 재사용 버퍼로 처리하고, 적의 실제 피격 범위와 공통 공격 판정 코드를 함께 사용한다.
- 전용 `LayerMask`로 기본 공격 판정 대상을 제한한다.
- 동일 HP를 공유하는 여러 콜라이더가 한 공격에 중복으로 맞지 않도록 처리한다.
- 신규 잔몹은 발을 지면에 맞추고 몸통과 실체가 있는 팔·꼬리까지 피격 범위에 포함한다. 그림자·빛 효과는 제외한다.
- Q 베기 그림은 매번 생성·파괴하지 않고 다시 사용한다.
- 전투 중 반복 로그는 디버그 옵션으로 제어한다.

관련 코드:

- `Assets/_Project/Scripts/Gameplay/Player/Combat/PlayerAttack.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Combat/PlayerAttackGeometry.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Combat/EnemyHitSnapshot.cs`

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

- 체력과 공격력은 연결된 몬스터 정의와 난이도 설정을 사용한다.
- 보스별 설정 에셋에서 스케일, 패턴과 필요한 스탯 덮어쓰기를 조정한다.
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
- 적·보스 설정 참조

현재 절차적 맵은 실제 사용하는 배치 규칙을 기준으로 생성한다. 옛 맵용 설정이나 대체 설정이 현재 맵에도 모두 적용되는 것은 아니다.

`EnemySpawnConfig` 담당:

- 적 스폰 규칙
- 몬스터 정의 참조와 정의가 없을 때의 대체 스탯·스프라이트
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

유지보수를 위해 큰 클래스를 partial 구조와 역할별 파일로 나눴다. 피해 처리, 특수 이동, 아이템 효과, 화면 입력과 맵 진행 연결은 별도 담당 코드로 추가 분리했다. partial 파일 분리는 같은 클래스 안의 정리이며, 이 작업으로 모든 모듈 간 의존성이 해소된 것은 아니다.

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

- `Core`: 부트스트랩, 입력·일시정지, 풀링, 공통 계약·판정·스탯
- `Gameplay/Player`: 플레이어 런타임, 전투, 성장, 스킬, 상태이상
- `Gameplay/Enemies`: 적 코어, 행동, 전투, FSM, 스폰, 상태이상, 보스, VFX
- `Gameplay/Items`: 아이템 설정, 획득·제거, 전투 효과, 시간에 따른 상태, 소환물 관리
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
- `Assets/_Project/Scripts/Core/Contracts/BiomeType.cs`

### 피격 피드백과 무적 시간

- 실제로 1 HP 이상 감소하면 스프라이트가 짧게 붉은색으로 깜빡이고 피격 무적 0.5초를 시작한다. 소수량만 누적된 타격은 피격 무적을 시작하지 않는다.
- 분열 재생으로 부활한 직후 보호 시간도 0.5초다.
- 피격·대시 무적은 전투 공격의 HP 피해를 막는다. 둔화·공격력 감소·새 지속 피해 부여·넉백·잡기·끌어당기기는 허용한다.
- 사망, 비활성화, 체력 리셋 시 스프라이트 색상과 무적 상태를 원래대로 복구한다.
- 레벨업 등 외부에서 부여하는 임시 무적은 플래시 없이 무적만 적용한다.

관련 코드:

- `Assets/_Project/Scripts/Gameplay/Player/Runtime/Health.cs`
- `Assets/_Project/Scripts/Gameplay/Player/Runtime/PlayerDamageCoordinator.cs`
- `Assets/_Project/Scripts/Core/Stats/HealthDeltaAccumulator.cs`

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
- 잔몹별 피해는 해당 몬스터 정의와 난이도 설정을 사용한다.
- 실제 1 HP 피해는 하트 반 칸 감소로 표시된다.
- `PlayerHeartUI`는 `Health.OnHealthChanged` 이벤트의 실제 현재 HP와 최대 HP 값을 받아 표시한다.
- Hub 씬에 직렬화된 `PlayerHeartUI` 설정도 하트 1개 = 2 HP 기준으로 맞췄다.
- 실제 적 설정과 옛 대체 설정을 구분한다. 옛 대체 설정을 수정해도 연결된 몬스터 정의의 값이 함께 바뀌는 것은 아니다.
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

플레이어 스탯 적용:

- `Assets/_Project/Scripts/Gameplay/Player/Runtime/PlayerStats.cs`

레벨업·성장·난이도 설정:

- `Assets/_Project/Resources/Balance/LevelProgressionConfig.asset`
- `Assets/_Project/Resources/Balance/Difficulty/`

아이템에서 쓰는 플레이어 스탯 타입:

- `Assets/_Project/Scripts/Core/Stats/PlayerItemStatDefinitions.cs`

공통 캐릭터 스탯 데이터:

- `Assets/_Project/Scripts/Core/Stats/CharacterStats.cs`

적 기본 수치와 스폰 규칙:

- `Assets/_Project/Data/MonsterBalance/Definitions/`: 몬스터별 정의
- `Assets/_Project/Resources/Balance/MonsterBalanceCatalog.asset`: 몬스터 목록
- `Assets/_Project/Data/BiomeConfigs/*EnemySpawnConfig.asset`: 바이옴별 등장 규칙

보스 패턴, 체력, 스케일, 안개, 아레나:

- `Assets/_Project/Data/BiomeConfigs/*BossArenaConfig.asset`

바이옴 맵, 오브젝트, 포털 기본 설정:

- `Assets/_Project/Data/BiomeConfigs/*BiomeConfig.asset`

## 실행과 검증

1. Unity Hub에서 이 폴더를 프로젝트로 추가하고 Unity `6000.3.9f1`로 연다.
2. 패키지 설치와 에셋 가져오기가 끝나면 `Assets/_Project/Scenes/MainMenu.unity`를 연다.
3. Play Mode로 들어가 새 게임 또는 이어하기를 선택한다.

프로젝트에는 `Assets`, `Packages`, `ProjectSettings`와 필요한 `.meta` 파일을 포함한다. `Library`, 빌드 결과, 개발 보고서와 임시 검사 코드는 업로드 대상에서 제외한다. 외부 에셋의 라이선스 파일은 유지한다.

업로드 준비본에서 실제 Play Mode 21개 항목과 기존 최종 보스의 클리어·메뉴 복귀를 확인했다. 7개 게임 씬의 일반 Player 빌드는 오류 0개·경고 0개였고, `Library`가 없는 상태의 새 에셋 가져오기도 확인했다. 모든 플랫폼과 아이템 조합을 전수 검증한 결과는 아니다.

## 업데이트 내역

### 2026-10-19

- 플레이어·적·아이템·맵·화면 코드를 역할별로 분리하고 공통 연결을 정리했다.
- 피격 무적, 소수 피해·회복, 저장·복구, 입력 잠금·창 중첩·일시정지 문제를 수정했다.
- 신규 잔몹의 피격 범위를 보완하고, 같은 방문에서 생존 적의 수·위치·HP·상태를 보관·복원하도록 개선했다. 보관 중 효과와 대기시간은 멈추며 실행 종료 시 기록은 사라진다.
- 스폰 검사와 청크 재진입의 불필요한 처리를 줄이고, 스포너·반복 연출 객체를 다시 사용하도록 정리했다.
- 사용하지 않는 온라인 기능과 실행에 필요 없는 개발 자료·검사 파일을 업로드 대상에서 제외했다.

기존 기능과 전투 수치를 유지했다. 스폰의 세부 등장 순서·좌표는 승인한 새 규칙을 사용한다. 기존 main의 최종 보스는 유지하고, 새로 작업한 최종 보스 구현은 이번 반영에서 제외했다.
