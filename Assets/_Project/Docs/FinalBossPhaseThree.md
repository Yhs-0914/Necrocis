# 최종보스 3페이즈 (2026-10-04)

## 피격 판정

이동형 보스는 실제 `MobileCerebrum` 이미지의 좌표를 기준으로 피격 영역을 잡는다. 기존 수직 박스 3개는 카메라를 향해 기울어진 이미지와 어긋나고 좌우 촉수를 제외해 판정이 좁았다. 3페이즈 진입 시 바닥 중심 판정 1개와 이미지 실루엣 판정 7개를 사용한다.

- 기존 자동 조준이 향하는 발밑 중심 박스
- 뇌의 넓은 하부와 정수리를 덮는 머리 박스 2개
- 중앙 줄기 박스
- 좌우로 뻗은 촉수와 하단 촉수 박스 4개

영역은 조금씩 겹쳐 경계에서 공격이 빠지지 않는다. 머리, 몸통, 주요 촉수는 맞으며 이미지 상단 모서리의 큰 투명 공간과 극히 가는 장식 끝은 제외한다. 이미지 판정은 실제 스프라이트의 피벗·종횡비를 사용하고, 카메라 회전·기울기·좌우 반전·부유·맥동을 따라간다. 카메라 반대 방향으로 판정을 늘려 바닥 높이의 투사체도 머리에 닿게 하되, 화면에서 보이는 판정 폭과 높이는 늘리지 않는다. 이동/지형 통과 판정은 기존 발밑 크기를 유지한다.

추가 박스는 보스 전투 대상의 자식이고 같은 레이어를 사용하므로 근접 공격, 투사체, 범위 스킬 모두 기존 `EnemyController` 하나에 피해를 준다. 접촉 피해는 계속 비활성 상태다. `Validate Phase Three Hitboxes`는 생성·재사용·재생성, 주요 이미지 지점의 포함, 투명 모서리 제외 및 반전/부유/스케일 추종을 검사한다.

## 연출 개선: 변형 촉수와 실시간 신경 이펙트

후속 품질 개선에서는 기존 아트를 활용하는 렌더링 방식과 동작을 수정했다.

- `FinalBossTentacleVisual`: 바닥의 평면 스프라이트 대신 카메라를 향하는 40구간 곡선 메쉬를 사용한다. 몸체 높이에서 출발해 휘어지고, 끝의 갈고리와 뿌리 비율을 유지하며 힘줄 부분만 늘어난다. 뿌리의 잔가지 중첩을 제거하고 연결부를 부드럽게 페이드한다. 준비 자세 → 빠른 찌르기 → 짧은 유지 → 휘어지며 회수하는 모션을 추가했다.
- 잡기는 실제 플레이어를 잡은 위치에서 촉수가 멈추고, 들어 올리는 동안 끝이 플레이어를 따라간다. 투척 이후 촉수를 접어 회수한다. 빗나간 경우도 회수 동작을 보여준다.
- 부채꼴은 5개 촉수를 함께 준비하고 펴며, 원형 휩쓸기에는 진행 방향을 보여주는 청록색 잔광이 붙는다. 회수 중의 직선 촉수는 피해를 주지 않는다.
- 반사는 본체 렌더러 중심에 맞춘 3차원 구형 보호막으로 변경했다. 회전하는 곡선과 얇은 막을 실시간 메쉬로 그리며, 기존 평면 링 이미지를 사용하지 않는다.
- 폭탄과 탄막은 원본 종횡비를 유지한다. 낙하는 가속하고, 탄막은 아래를 향한다. 폭발은 두 겹의 확장 충격파와 발광 파편으로 표현한다.
- 유도탄은 6개 충전 지점에서 두 발씩 나오며 짧은 비행 잔광을 남긴다. 돌진에는 본체 기울기와 투명 잔상을 추가했다.
- 3페이즈 위험 표시는 얇은 외곽선과 차오르는 예고선, 낮은 불투명도의 바닥 표시로 바꿨다. 공격 시작 때 채움과 두 외곽선을 모두 숨긴다.
- 축소 경계는 큰 촉수 이미지를 늘여 붙이는 방식에서 3단 신경 에너지 벽으로 바뀌었다.

`NeuralEnergy.shader`는 Resources에 포함하여 빌드 시에도 로드되며 별도 블룸 설정 없이 부드러운 가산광을 그린다. 메쉬 버퍼는 생성 시 한 번 할당하고, 형태별로 크기를 제한한다. 메쉬는 효과 오브젝트와 함께 해제한다. 공격 피해 1~2와 페이즈 전환 조건은 유지한다.

검증: 런타임/에디터 컴파일, 기존 판정 계산 검사, 독립 Unity 미리보기 씬에서 게임용 촉수 메쉬와 셰이더의 실제 1920×1080 렌더를 확인했다. 렌더 결과를 보고 촉수 연결부의 중첩과 보호막 위치를 한 차례 수정했다. 전체 전투를 플레이하는 스모크 검증과는 별개다.

미리보기: `Exports/FinalBossConcepts/PhaseThreePolish/`의 촉수 찌르기·부채꼴·반사·폭발·휩쓸기·경계 PNG 및 `motion-00.png`부터 `motion-07.png`. Unity 메뉴 `Tools > Necrocis > Final Boss > Render Phase Three Combat Previews`로 다시 렌더링할 수 있다. 이 메뉴는 독립 미리보기 씬을 사용하며 게임 씬이나 저장 데이터를 변경하지 않는다.

## 진입과 전투

전체 체력 40% 이하에서 맵과 분리하여 플레이어를 추적한다. 일반적인 전환에서는 체력을 회복하지 않는다. 2페이즈에서 체력을 한 번에 0으로 만드는 초대형 공격만 40%로 보호하여 페이즈를 건너뛰지 않게 한다. 1.8초 분리 연출 중에는 피해를 복구한다. 2페이즈 장판과 발사체는 전환 시 정리된다.

6종 패턴은 섞인 가방에서 한 번씩 뽑고, 가방 경계에서도 동일 패턴이 바로 반복되지 않게 한다. 예고 중 목표 방향을 고정하고 공격 시 빨간 위험 표시를 제거한다. 유도탄이 소멸한 뒤 다음 패턴을 시작하여 잡기/촉수와의 강제 중첩을 방지한다. 회복 구간에서는 다시 플레이어를 추적하며 공격 기회를 준다.

| 패턴 | 구현 | 피해 |
| --- | --- | --- |
| 돌진 + 탄막 | 1.15초 직선 예고, 12단위 돌진, 3차례 낙하 탄막 | 돌진 2 / 탄막 1 |
| 잡기 + 던지기 | 1.3초 좁은 직선 예고, 촉수 뻗기, 피격한 플레이어 띄우기와 포물선 투척 | 착지 2 |
| 촉수 3연타 | 직선 찌르기 → 27도 간격의 5갈래 부채꼴 → 7.5단위 반경 360도 휩쓸기 | 각 2 |
| 유도탄 | 12발씩 3회, 총 36발, 추적 제한 후 직진하여 회피 가능 | 1 |
| 반사 | 1.2초 예고 후 3초 방어, 청록색 링과 본체 색상 변화, 발사 중단 안내 | 반사 1~2 / 0.45초 간격 제한 |
| 세포 폭탄 | 4개 낙하 웨이브, 웨이브마다 최대 7개, 개별 착지 지점 예고 | 2 |

잡기는 대시 무적/피격 무적/지형 횡단 중인 플레이어를 붙잡지 않는다. 붙잡힌 동안 이동·스킬 입력을 막고, 착지 지점은 공통 지형 판정과 축소 경계 안에서 찾는다. 보스 사망, 플레이어 사망, 컴포넌트 비활성화 시 강제 이동과 조작 잠금을 해제하고 모든 3페이즈 이펙트·발사체를 즉시 비활성화한다.

## 체력 10%: 타임어택

- 기본 45초. `FinalBossPhaseThreeController`의 `collapseDuration`으로 조정한다.
- 신경 촉수 경계가 부드럽게 안으로 좁아지며, 바깥 영역을 어두운 생체 오염으로 덮는다. 최종 안전 영역은 14×12 월드 단위다.
- 경계 밖으로 나가는 이동과 대시는 막는다. 경계가 플레이어를 지나쳤다면 안쪽으로 돌아오는 이동은 허용한다.
- 바깥에서는 초당 1 피해. 시간이 끝나면 즉사가 아니라 초당 2 피해를 주는 연장전이 시작된다.
- 남은 시간과 행동 안내는 보스 HUD에 표시한다. HUD 중앙 높이 222 reference pixels를 유지하여 기존 경험치 UI와 구분한다.
- 전투 피해는 기존 요청대로 1~2로 제한한다. 실제 플레이어 방어/아이템/무적 판정은 기존 `TakeDamage` 경로를 그대로 사용한다.

## 새 이미지와 연결

생성 방식: 내장 image generation 도구. imagegen 스킬에 따라 실제 알파 배경 PNG를 생성하고 프로젝트 안에 복사했다. 원본 생성물은 삭제하지 않았다. 기존 보스/2페이즈 이미지는 덮어쓰지 않았다.

리소스 폴더: `Assets/_Project/Resources/FinalBoss/PhaseThree/`

- `MobileCerebrum.png`: 분리 후 본체. 기존 뇌 이미지를 스타일 참고로 사용. 알파 대응 기본 Sprite 머티리얼로 렌더링하며 기존 배경 제거 셰이더는 폴백에만 사용한다.
- `NeuralLance.png`: 직선/부채꼴/원형 촉수, 잡기, 축소 벽. 왼쪽 뿌리 피벗에서 실제 사거리까지 뻗고 거둔다.
- `NeuralRing.png`: 반사 장벽과 폭발 충격파. 회전·확대·페이드 애니메이션을 코드에서 처리한다.
- `CellBomb.png`: 강화된 낙하 세포 폭탄.
- `HomingNeuralCell.png`: 강화 유도탄과 하늘에서 떨어지는 탄막.

모두 Sprite single mode, Point filter, PPU 128, mipmap 없음, 비압축, alpha transparency 설정이다. 본체 호흡/공격 전 충전 변형, 반사 색 변화, 카메라 충격, 기존 보스 효과음도 연결했다. 다수 폭발의 사운드/카메라 충격은 0.12초 간격으로 제한한다.

## 검증과 남은 확인

- 런타임 및 에디터 C# 빌드: 오류 0, 기존 미할당 필드 경고 11개.
- 컴파일된 게임 어셈블리로 경계 내부 이동, 캐릭터 폭을 포함한 벽 판정, 바깥에서 안쪽 탈출/바깥쪽 차단, 직선 공격 안/밖/뒤, 돌진 연속 충돌, 추적 정지 거리와 근접 정지 등 10개 계산 검사 통과.
- 새 PNG 5개에서 투명 픽셀/알파를 샘플 확인했다.
- Unity 메뉴 `Tools > Necrocis > Final Boss > Validate Phase Three Assets and Geometry`: 임포트 설정과 기하 판정 검사. 실제 플레이 모드에서는 아직 실행하지 않았다.
- 기존 탐험 스모크 검사를 40% 전환/체력 유지 및 새 모바일 이미지에 맞춰 갱신했다. 이번 작업에서 플레이 스모크는 실행하지 않았다.

플레이 확인: 40% 전환 → 6종 공격의 전조와 이미지 정렬 → 잡기 회피/착지/취소 → 반사 피해 제한 → 10% 경계 축소/타이머 → 보스 또는 플레이어 사망 시 잔여 공격과 잠금 정리. 특히 좁은 최종 영역에서 탄막 밀도, HUD 가독성, 카메라 효과 강도는 실제 플레이로 조정해야 한다. 코드 빌드만으로 상용 게임 수준의 완성도를 보장하지 않는다.

## 이미지 생성 프롬프트 세트

공통: 투명 Unity 2D Sprite, hand-painted pixel art, dusty pink/violet biological tissue, cyan neural fissures, no text/UI/watermark. 아래는 각 생성에 사용한 프롬프트다.

### NeuralLance

Use case: stylized-concept. Asset type: transparent Unity 2D boss attack sprite. Reference image is style reference only: match its hand-painted pixel art brain tissue, dusty pink and purple folds, cyan synapse highlights. Generate a single fully extended spear-like neural tentacle, thick root at left and a sharp hooked claw tip at right, long horizontal straight thrust silhouette, strongly readable at small size, richly detailed tissue wrapped with glowing cyan axons. Root centered at left 5% of canvas, entire object within padded frame. No environment, no text, no UI, no watermark. Actual transparent background.

### NeuralRing

Use case: stylized-concept. Transparent Unity 2D game effect sprite. Single circular neural energy shockwave ring seen from directly above, empty transparent center, organic pink brain tissue segmented ring and bright cyan electrical veins with violet sparks, hand-painted pixel art matching a dark fantasy biological final boss. Clean strongly readable circular silhouette, generous transparent padding, no background, text, UI, watermark. Actual alpha transparency.

### CellBomb

Use case: stylized-concept. Single transparent Unity 2D boss projectile sprite, grotesque airborne cell bomb with a large pink brain nucleus visible through translucent purple membrane, six short curled nerve tendrils, glowing cyan fissures, orange-hot unstable core, hand-painted pixel art dark fantasy biological action game. Isolated round silhouette with subtle falling comet energy trail above it. No floor, no backdrop, no text, no UI. Actual transparent background. Crisp readable small sprite, richly textured tissues.

### MobileCerebrum

Use case: stylized-concept. Unity transparent 2D action game final boss sprite. Style reference attached. Draw NEW isolated mobile cerebrum final boss, three-quarter frontal view: massive wrinkled dusty pink brain, violently glowing cyan synaptic cracks, violet underside, six thick articulated hanging clawed neural tentacles splayed apart, large menacing round silhouette, balanced visible core, floating organic horror with no humanoid face or eyes. Hand-painted pixel art, subtle chunky pixel edges, rich detailed shading matching reference. Entire boss inside canvas, 10% transparent padding, root tentacles lowest point near bottom center. No environment or dark backdrop, no text, UI or watermark. MUST have actual transparent background, not a drawn checkerboard.

### HomingNeuralCell

Use case: stylized-concept. Single horizontal transparent Unity 2D game homing missile sprite, a small flying neural cell with a compact violet and dusty pink brain nucleus at RIGHT, one bright cyan pointed claw nose facing RIGHT, thin tapering cyan electrical nerve trail flowing LEFT. Dark biological fantasy painted pixel art, clear readable arrowlike silhouette, rich tissue detail, matching pink brain boss cyan cracks. Object isolated and entirely inside canvas with padding, no background, text or UI, actual alpha transparency.
