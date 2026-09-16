# 맵 전용 일반몹 12종 적용

기존 콘셉트의 일반몹 8종과 엘리트 4종을 모두 일반몹으로 등록했습니다.
각 바이옴의 EnemySpawnConfig에 전용 3종을 추가했습니다. 장의 기존 Macrophage/NKCell/BCell 및 Granuloma/Antibody 규칙은 유지합니다.

| 맵 | 몬스터 | 설정 이름 | 공격 | HP | 공격력 | 공격 쿨타임 | 경험치 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 위 | 꿀렁이 | StomachGulp | 원거리 | 4 | 1 | 1.5초 | 15 |
| 위 | 점액달팽이 | StomachMucusSnail | 근접 | 6 | 1 | 1.1초 | 10 |
| 위 | 과식낭 | StomachOverfedSac | 원거리 | 6 | 1 | 1.8초 | 15 |
| 장 | 주름벌레 | IntestineFoldworm | 근접 | 6 | 1 | 1초 | 10 |
| 장 | 융털손 | IntestineVillusHand | 근접 | 6 | 1 | 1.2초 | 10 |
| 장 | 매듭장어 | IntestineKnottedEel | 근접 | 8 | 1 | 1.2초 | 12 |
| 간 | 봉합세포 | LiverSutureCell | 원거리 | 4 | 1 | 1.5초 | 15 |
| 간 | 혈전덩이 | LiverClot | 근접 | 8 | 1 | 1.2초 | 12 |
| 간 | 응고핵 | LiverCoagulationCore | 원거리 | 6 | 1 | 1.8초 | 15 |
| 폐 | 숨방울 | LungBreathBubble | 원거리 | 4 | 1 | 1.5초 | 15 |
| 폐 | 섬모솔 | LungCiliaBrush | 근접 | 6 | 1 | 1초 | 10 |
| 폐 | 과팽창 폐포 | LungOverinflatedAlveoli | 원거리 | 6 | 1 | 1.8초 | 15 |

위 값은 기본값이며 기존 난이도 배율이 적용됩니다. 공격 쿨타임은 기존 AI처럼 공격 애니메이션이 끝난 후 계산합니다.
원거리형은 BCell과 같은 제자리 사격, 근접형은 NKCell과 같은 배회/추격/복귀 AI입니다.
초기 적용 범위는 기본 근접/원거리 공격입니다. 보호막, 독 웅덩이, 특수 돌진, 바람 밀치기 등의 개별 능력은 포함하지 않습니다.

## 파일

- 게임용 이미지: `Assets/_Project/Art/Images/Enemies/BiomeExclusive/<번호_이름>/`
- 각 몬스터: `Idle_0..3`, `Move_0..3`, `Attack_0..3`, `Death_0..3` (총 192 PNG)
- 128×128, 투명 배경, Point 필터, 압축/밉맵 없음, PPU 80, 발밑 피벗
- 이 폴더의 PNG는 재작업용 원본 시트입니다. 행 순서: 대기 → 이동 → 공격 → 사망.
- `preview.html`: 12종 동작을 함께 확인하는 로컬 미리보기
- `prompts.md`: 이미지 생성 프롬프트
- `validation.txt`: Unity 배치 플레이 테스트 결과

## 기존 시스템 연결

EnemyController / SpriteFrameAnimator / EnemyProjectile / EnemyContactDamage를 그대로 사용합니다.
플레이어의 공격·스킬·상태이상, 접촉 피해, 경험치, 엘리트 소환용 처치 집계(기존 엘리트가 있는 맵), 사망 애니메이션, 풀 재사용이 동일하게 적용됩니다.
바이옴별 고유 이름과 1601~1612번 스폰 salt를 사용하여 서로 다른 이미지가 풀에서 섞이지 않도록 구성했습니다.
기존 스폰 밀도를 참고하되, 장은 기존 3종에 추가되는 만큼 신규 종의 밀도를 0.02로, 다른 맵은 0.05로 설정했습니다.

## 재생성 및 검증

프로젝트 루트에서 실행:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/BuildBiomeEnemySprites.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/ConfigureBiomeEnemies.ps1
```

시트 슬라이서는 녹색 배경 제거, 프레임 사이 빈 공간 검출, 동일 비율 리사이즈와 발밑 정렬만 수행합니다. 포즈 자체는 생성된 원본 이미지입니다. 기존 PNG GUID는 유지됩니다.
설정 스크립트는 기존 몬스터를 남겨두고 새 12종만 추가/갱신합니다.

`NecrocisEditor.BiomeEnemySmokeRunner.Run`을 분리된 Unity 6000.3.9f1 배치 프로젝트에서 실행했습니다.
4개 씬의 바이옴 참조, 192개 스프라이트 임포트, 12종의 대기/이동/공격 재생, 근접·원거리 실제 피해, 접촉 피해, 피격, 기절, 공격 중 사망, 경험치, 중복 사망 방지 및 풀 재사용을 통과했습니다.
테스트 시작 시 Unity Editor 검색 인덱서에서 ArgumentOutOfRangeException이 한 번 발생했으나 몬스터 코드가 아닌 UnityEditor.Search 내부였으며 위 테스트는 모두 완료됐습니다.
그래픽 없는 배치 검증이므로 실제 맵에서의 화면 크기와 전투 밀도는 플레이하며 조정할 수 있습니다.
