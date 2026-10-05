# Necrocis Gameplay

인체의 장기를 배경으로 개발 중인 Unity 2.5D 액션 게임입니다. 허브에서 장·간·위·폐 바이옴에 진입하고, 적을 처치하며 성장한 뒤 각 지역의 보스를 상대합니다.

팀 프로젝트의 게임플레이 코드를 정리한 저장소입니다. 저는 보스 전투, 공통 스탯, 저장과 난이도 처리, 성능 개선, 브랜치 통합을 담당했습니다. 플레이어 스킬, 아이템, 맵, 사운드에는 팀원들이 개발한 내용도 함께 포함되어 있습니다.

## 담당 작업

- 장·간·위·폐 보스의 공격 패턴과 페이즈 전환을 구현하고, 보스방 진입·봉쇄·처치 보상·귀환 포털을 공통 흐름으로 연결했습니다.
- 플레이어와 적의 전투 계산을 분리하고, 레벨업·아이템·상태이상의 스탯 보정 방식을 정리했습니다. 근접·투사체 피격 판정, 무적 시간, 넉백과 사망 처리도 보완했습니다.
- Normal·Hard별 저장 슬롯과 영구 프로필을 분리했습니다. 새 게임, 이어하기, 사망 후 처리, 저장 파일 백업 복구를 구현했습니다.
- 청크 로딩과 오브젝트 풀을 정리하고, Non-Alloc 물리 쿼리·컴포넌트 캐싱·공간 해시를 적용해 반복 할당과 전체 적 탐색을 줄였습니다.
- 메인 메뉴의 보스 처치 진행도 표시와 전투 피드백을 구현하고, 팀 브랜치를 통합하면서 코드 충돌과 Unity 에셋 참조를 점검했습니다.

## 주요 구현

### 보스 전투

각 보스는 별도의 패턴 클래스를 사용합니다. 장 보스는 기생충 소환과 충격파, 간 보스는 혈액 폭탄과 회복 자세, 위 보스는 돌진과 산성 공격·흡입, 폐 보스는 형제 보스와 생존 개체의 광폭화를 처리합니다.

보스방의 경계, 입장 트리거, 전투 중 이동 제한, 처치 후 포털 생성은 공통 컨트롤러에서 관리합니다.

- [보스 패턴](Assets/_Project/Scripts/Gameplay/Enemies/Bosses/)
- [보스방 컨트롤러](Assets/_Project/Scripts/World/Biomes/BossArena/MidBossArenaController.cs)
- [보스·적·바이옴 설정](Assets/_Project/Data/BiomeConfigs/)

### 스탯과 밸런스

공통 스탯 모델에 고정값·비율 보정치를 적용하고, 보정치의 출처를 구분해 아이템이나 버프가 제거될 때 해당 효과만 해제합니다. 플레이어와 적의 피해 계산은 별도 클래스로 나눴습니다.

성장 곡선과 기본 스탯, 난이도 배율, 적 스폰, 보스 패턴 수치는 ScriptableObject 에셋에서 관리합니다.

- [공통 스탯 모델](Assets/_Project/Scripts/Core/Stats/CharacterStats.cs)
- [플레이어 전투 계산](Assets/_Project/Scripts/Gameplay/Player/Combat/PlayerCombatCalculator.cs)
- [난이도 설정](Assets/_Project/Scripts/Core/Difficulty/)
- [성장·밸런스 데이터](Assets/_Project/Resources/Balance/)

### 저장과 절차적 맵

JSON 저장 파일은 영구 프로필, Normal 진행, Hard 런으로 구분합니다. Normal 사망 시에는 성장 정보를 유지하고 허브로 복귀하며, Hard 사망 시에는 해당 런을 초기화합니다. 저장 파일이 손상되면 백업 파일을 읽도록 처리했습니다.

새 게임을 시작할 때 네 바이옴의 지형 시드를 각각 생성해 저장합니다. 같은 런에서 재입장하거나 이어하기를 하면 저장된 시드로 지형을 다시 생성합니다. 이어하기는 마지막 바이옴의 입구에서 재개하며, 보스 전투 도중의 상태나 플레이어의 정확한 좌표를 복원하는 방식은 아닙니다.

- [저장 서비스](Assets/_Project/Scripts/Core/Save/SaveService.cs)
- [저장 파일 입출력](Assets/_Project/Scripts/Core/Save/SaveFileStore.cs)
- [절차적 맵 생성](Assets/_Project/Scripts/ProceduralMap/MapGenerator.cs)
- [맵과 게임플레이 연결](Assets/_Project/Scripts/World/Biomes/Runtime/ProceduralBiomeBridge.cs)

### 성능 개선과 검증

투사체와 전투 이펙트는 풀에서 재사용합니다. 근접 공격은 재사용 버퍼를 사용하는 물리 쿼리로 처리하고, 적 간 분리 이동은 공간 해시의 주변 셀을 조회합니다. 청크 생성은 코루틴으로 나누어 처리합니다.

저장, 사망, 메뉴 전환, 접촉 피해, 보스방 경계는 Unity 배치 모드에서 실행하는 스모크 테스트로 확인합니다. 이 테스트가 전체 플레이나 모든 아이템 조합을 검증하는 것은 아닙니다.

- [런타임 오브젝트 풀](Assets/_Project/Scripts/Core/Pooling/RuntimePool.cs)
- [적 이동과 공간 해시](Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyMovement.cs)
- [검증 스크립트](Assets/_Project/Editor/)

## 개발 환경

- Unity `6000.3.9f1` / C#
- Universal Render Pipeline `17.3.0`
- Input System `1.18.0`
- Unity UI, TextMesh Pro, Tilemap
- Git / GitHub

## 현재 개발 상태

현재 버전은 출시 빌드가 아닌 개발 중인 프로젝트입니다.

- 장·간·위·폐의 지형과 보스 패턴은 포함되어 있습니다. 간·위·폐의 일반 적 스폰 설정과 바이옴별 전투 배치는 작업 중입니다.
- 부산물 수집 이후의 최종 보스 씬 로드와 엔딩 연결은 아직 구현되지 않았습니다.

외부 코드, 폰트, VFX의 저작권과 이용 조건은 각 제작자의 라이선스를 따릅니다. 이 저장소 전체에 하나의 오픈소스 라이선스를 적용한 것은 아닙니다.
