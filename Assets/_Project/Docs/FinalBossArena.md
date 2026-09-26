# 최종보스 맵 — 공통 타일맵 구현

## 사용법

- 플레이 중 F8 → **4개 맵 보스 처치 완료 (저장 반영)** 버튼을 누릅니다.
- 위·장·간·폐 보스 처치 기록과 부산물 4종이 정상 획득 경로인 `GameManager.CollectRelic`을 통해 반영됩니다. 반복 클릭해도 이미 완료된 기록은 중복 처리하지 않습니다.
- 버튼은 현재 게임/저장 상태를 변경합니다. F8을 닫고 허브 중앙 포탈에 들어가면 최종보스 맵으로 이동합니다. 다른 맵에서 눌렀다면 먼저 허브로 귀환합니다.
- `FinalBoss.unity`를 단독으로 열어 Play해도 허브와 같은 플레이어·카메라를 생성합니다. 단독 미리보기 자체는 보스 처치 기록을 해금하지 않습니다.
- 아래쪽 입구의 귀환 트리거로 허브에 돌아갈 수 있습니다.

## 기존 4개 맵 분석과 통합

위·장·간·폐 씬은 모두 `ProceduralMap.MapGenerator`와 `ProceduralBiomeBridge`를 사용합니다.
XZ 평면으로 회전한 Grid 아래 Tilemap 레이어에 지형을 렌더링하고,
`GridData/MapCell`에서 이동 가능 여부를 관리합니다. 플레이어는
`ProceduralTerrainMotor`를 통해 같은 데이터를 조회하며, 타일과 오브젝트는 청크 단위로 로드합니다.

최종보스 맵도 같은 구조를 사용합니다.

- `Grid → Base / Grass / Second Floor / Cliff Tilemap`
- `MapGenerator → AuthoredMapLayout → GridData/MapCell → 공통 청크 렌더링`
- `ProceduralBiomeBridge`: 공통 바이옴·카메라·청크 연결. 별도 최종보스용 BiomeConfig에는 일반 적과 중간보스 생성 규칙이 없습니다.
- `ProceduralTerrainMotor`: 기존 4개 맵과 동일한 스폰·이동 경로.
- `FinalBossArena`: 방 상태와 기둥·귀환 지점 참조만 담당합니다. 독립적인 UV 이동 판정과 PlayerController의 최종보스 전용 이동 분기를 제거했습니다.

최종보스 방은 48×38 고정 배치를 유지합니다. 기존 탐험 맵의 300×300 무작위 지형을 그대로 복제하지 않고,
공통 생성기에 선택적인 `AuthoredMapLayout` 자산을 추가해 보스방의 배치를 지정합니다.
기존 4개 씬은 이 자산을 사용하지 않아 원래의 절차적 생성 경로를 그대로 실행합니다.
고정 맵 이동에는 경로 중간도 검사하여 대시·넉백으로 기둥이나 벽을 넘어가지 못하게 합니다.

## 아트와 배치

방 전체 이미지를 붙인 Quad는 사용하지 않습니다. 바닥은 반복 Tilemap,
외곽은 기존 장기 맵 벽 타일, 네 장기 기둥과 잠든 대뇌는 개별 SpriteRenderer 오브젝트입니다.
기존 대뇌 맵 원본 텍스처를 참조하는 별도 Sprite 자산과 윤곽 메시를 사용하며,
원본 이미지를 수정하거나 새 이미지를 생성하지 않습니다.
기둥은 Billboard와 SpriteYSort를 사용하고, 바닥 점유 영역은 레이아웃 자산에서 지정합니다.
대뇌와 기둥의 전투 AI·체력·파괴·페이즈는 아직 구현하지 않습니다.

## 편집과 검증

- `FinalBossLayout.asset`: 방 크기, 스폰 셀, 외곽 다각형, 기둥/대뇌 점유 영역.
- `FinalBossBiomeConfig.asset`: 공통 바이옴 연결 설정.
- `FinalBossSceneBuilder.cs`: 씬·개별 Sprite 자산·허브 기반 대체 플레이어/카메라 생성.
- **Tools > Necrocis > Final Boss > Build Exploration Scene**: 최종보스 씬을 닫은 상태에서 재생성합니다. 생성 씬과 관련 자산의 수동 편집을 덮어쓰므로 레이아웃 변경을 유지하려면 빌더도 함께 수정해야 합니다.
- **Run Exploration Smoke Test**: 임시 저장으로 15종 미완료 조합의 잠금, 실제 F8 버튼 콜백, 4/4 기록, 허브 입장, 공통 지형 연결, 바닥 연결성, 기둥/벽 경로 차단, 이동 및 귀환 후 기록을 검증합니다.
- **Run Direct Scene Smoke Test**: 단독 씬 시작과 스폰·공통 지형 연결·이동·귀환·처치 기록 비변경을 검증합니다.
- 테스트 결과와 카메라 캡처: `Exports/FinalBossConcepts/2026-09-26/TilemapImplementation/`.
