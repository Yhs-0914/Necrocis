# Necrocis 작업 정리

## 이번 작업 개요

이번 작업은 플레이어/적 전투 스탯 분리, 아이템 기반 코드 추가, 장 보스 임시 패턴 구현, 그리고 바이옴 설정 구조 분리를 중심으로 진행했다.

## 스탯 분리

플레이어 기본 스탯을 다음 7개 기준으로 재정의했다.

- 체력
- 이동속도
- 공격력
- 공격 속도
- 공격 사거리
- 마력
- 스킬 쿨타임 감소

공격력과 마력의 역할을 분리했다.

- 공격력은 기본 공격 계열 데미지에만 적용된다.
- 공격 속도와 공격 사거리는 기본 공격에만 적용된다.
- 마력은 스킬 데미지 증가에만 적용된다.
- 스킬 쿨타임 감소는 퍼센트 기반이며 기본값은 0%다.
- 방어력은 플레이어 스탯/레벨업/UI 계산에서 제거했다.

관련 주요 코드:

- `Assets/_Project/Scripts/Core/Stats/CharacterStats.cs`
- `Assets/_Project/Scripts/Core/Stats/PlayerStatDefinitions.cs`
- `Assets/_Project/Scripts/Player/PlayerCombatCalculator.cs`
- `Assets/_Project/Scripts/Enemy/EnemyCombatCalculator.cs`
- `Assets/_Project/Scripts/Player/PlayerStats.cs`
- `Assets/_Project/Scripts/Player/StatManager.cs`
- `Assets/_Project/Scripts/Player/StatUI.cs`
- `Assets/_Project/Scripts/Player/LevelUpManager.cs`
- `Assets/_Project/Scripts/Player/LevelUpUI.cs`
- `Assets/_Project/Scripts/Player/PlayerAttack.cs`
- `Assets/_Project/Scripts/Player/PlayerAttackModule.cs`
- `Assets/_Project/Scripts/Player/Projectile.cs`
- `Assets/_Project/Scripts/Player/Skills/PlayerClassSkillController.cs`

## 아이템 베이스

ScriptableObject 기반 아이템 베이스를 추가했다.

- 플레이어 기본 스탯 7종을 아이템에서 재사용할 수 있다.
- 아이템은 스탯 타입과 수치 리스트를 가진다.
- 쿨타임 감소 아이템도 같은 구조로 확장 가능하다.

관련 코드:

- `Assets/_Project/Scripts/Items/PlayerItemBase.cs`

## 플레이어/적 계산식 분리

플레이어와 적의 계산식을 분리했다.

- 플레이어는 기본 공격/스킬 분리 계산을 사용한다.
- 적은 필요한 전투 계산만 별도 계산기로 유지한다.
- 플레이어 스탯 확장과 적 스탯 확장이 서로 영향을 덜 받도록 분리했다.

관련 코드:

- `Assets/_Project/Scripts/Player/PlayerCombatCalculator.cs`
- `Assets/_Project/Scripts/Enemy/EnemyCombatCalculator.cs`

## 장 보스 임시 패턴

장 보스 패턴을 임시 에셋 기반으로 구현했다.

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

장 보스는 보스답게 보이도록 최소 체력 500, 스케일 배율 4배로 설정했다. 보스 패턴은 보스별 설정 에셋에서 조정할 수 있다.

관련 코드 및 설정:

- `Assets/_Project/Scripts/Enemy/IntestineBossPattern.cs`
- `Assets/_Project/Scripts/Player/PlayerStatusEffectController.cs`
- `Assets/_Project/Scripts/Biome/MidBossArenaController.cs`
- `Assets/_Project/Data/BiomeConfigs/IntestineBossArenaConfig.asset`

## 보스 검증 기능

장 보스 패턴 검증용 ContextMenu를 추가했다.

- `Debug/Force Phase 1`
- `Debug/Force Phase 2`
- `Debug/Run Phase 1 Dung`
- `Debug/Run Phase 2 Throw`
- `Debug/Run Phase 2 Stomp`

빠른 검증을 위해 보스 설정에 디버그 옵션도 추가했다.

- `startInPhase2ForDebug`
- `useFastPatternCooldownsForDebug`
- `fastPatternCooldown`

## 바이옴 설정 분리

`BiomeConfig`에 너무 많은 설정이 들어가 있던 구조를 분리했다.

`BiomeConfig`는 다음을 담당한다.

- 바이옴 맵 기본 설정
- 타일/지역 설정
- 오브젝트 스폰 규칙
- 귀환 포털 설정

`EnemySpawnConfig`는 다음을 담당한다.

- 적 스폰 규칙
- 적 개별 스탯
- 적 스프라이트
- 엘리트 특수 설정

`BossArenaConfig`는 다음을 담당한다.

- 보스 아레나 설정
- 보스 선택/fallback 규칙
- 보스 체력/스케일 오버라이드
- 보스 패턴 설정

기존 바이옴 에셋이 바로 깨지지 않도록 기존 `enemySpawnRules`, `midBossArena` 데이터는 숨긴 fallback으로 유지했다.

관련 코드:

- `Assets/_Project/Scripts/Biome/BiomeConfig.cs`
- `Assets/_Project/Scripts/Biome/EnemySpawnConfig.cs`
- `Assets/_Project/Scripts/Biome/BossArenaConfig.cs`
- `Assets/_Project/Scripts/Biome/ConfigurableBiomeManager.cs`

생성/연결된 설정 에셋:

- `Assets/_Project/Data/BiomeConfigs/IntestineEnemySpawnConfig.asset`
- `Assets/_Project/Data/BiomeConfigs/LiverEnemySpawnConfig.asset`
- `Assets/_Project/Data/BiomeConfigs/LungEnemySpawnConfig.asset`
- `Assets/_Project/Data/BiomeConfigs/StomachEnemySpawnConfig.asset`
- `Assets/_Project/Data/BiomeConfigs/IntestineBossArenaConfig.asset`
- `Assets/_Project/Data/BiomeConfigs/LiverBossArenaConfig.asset`
- `Assets/_Project/Data/BiomeConfigs/LungBossArenaConfig.asset`
- `Assets/_Project/Data/BiomeConfigs/StomachBossArenaConfig.asset`

## 설정 위치

플레이어 기본 스탯:

- `Assets/_Project/Scripts/Player/PlayerStats.cs`

플레이어 스탯 타입 정의:

- `Assets/_Project/Scripts/Core/Stats/PlayerStatDefinitions.cs`
- `Assets/_Project/Scripts/Core/Stats/CharacterStats.cs`

적 기본 수치:

- `Assets/_Project/Data/BiomeConfigs/*EnemySpawnConfig.asset`

장 보스 패턴/체력/스케일:

- `Assets/_Project/Data/BiomeConfigs/IntestineBossArenaConfig.asset`

바이옴 맵/오브젝트/귀환 포털:

- `Assets/_Project/Data/BiomeConfigs/*BiomeConfig.asset`

## 검증

다음 명령으로 컴파일 검증을 진행했다.

```bash
dotnet build Assembly-CSharp.csproj --no-restore
```

신규 컴파일 오류는 없었다. 기존 `PlayerClassSkillController`의 스킬 prefab 미할당 경고 9개는 남아 있으며, 이번 작업 범위의 신규 오류는 아니다.
