# 실제 적용 수치 — 2026-10-26

적용 자산과 런타임 설명을 추출했다. 수정 전 표는 CURRENT_VALUES.md·ITEM_CATALOG.md에 보존했다. 검증 한계는 IMPLEMENTATION.md 참고.

후속 적용값: 직업/E Lv5, R Lv10. 잡몹 스포너는 Lv1~5에1~2마리부터 시작해5레벨마다 최소/최대가1씩 증가한다. 직업 선택 후 캠페인당 시작 변이 보상1회가 추가됐다. [전체 재점검](REVIEW_2026-10-26.md)의 공속8·마력12·사거리6은 **미적용 제안**이며 아래 표가 현재값이다. 실제 성장 화면은 공속·사거리를 각각 선택한다.

## 플레이어 성장

| 능력치 | 선택당 수치 | 모드 |
| --- | --- | --- |
| 공격 | 0.25 | 고정 |
| 사거리 | 3 | % 합산 |
| 공속 | 5 | % 합산 |
| 마력 | 8 | 고정 |
| 이동 | 3 | % 합산 |
| HP | 1 | 고정 |

## 진행 배율·최종 보상

| 이전 클리어 수 | 공통 HP | 공격 | 일반 EXP | 보스 HP | 필드 엘리트 EXP | 지역 보스 EXP |
| --- | --- | --- | --- | --- | --- | --- |
| 0 | 1 | 1 | 1 | 1.0 | 100 | 240 |
| 1 | 1.15 | 1.08 | 1.1 | 1.19444444 | 120 | 300 |
| 2 | 1.35 | 1.2 | 1.25 | 1.38888889 | 125 | 300 |
| 3 | 1.6 | 1.35 | 1.4 | 1.61111111 | 140 | 350 |
| 4 | 1.85 | 1.5 | 1.6 | 1.83333333 | 160 | 400 |

## 몬스터 기본값

노말 진행도0의 값. 이후 진행 배율·등급 보상을 적용한다. 폐 Phase2는 생존체의 새 체력이며 두 몸 각각에 전투 전체 HP를 부여하지 않는다.

| ID | 페이즈 | 기본 HP | 공격 | 이동 | 기본 EXP | 접촉 계수 | 패턴 계수 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| boss.intestine | Default | 360 | 2 | 1.5 | 240 | 0.5 | dung=0.5, stomp=1 |
| boss.intestine | Phase2 | 360 | 2 | 1.7249999 | 240 | 0.5 | dung=0.5, stomp=1 |
| boss.liver | Default | 360 | 2 | 1 | 240 | 0.5 | blood-bomb=1 |
| boss.liver | Phase2 | 360 | 2 | 1.5 | 240 | 0.5 | blood-bomb=1 |
| boss.lung | Default | 80 | 2 | 1 | 240 | 0.5 | gas=1, dive=0.75 |
| boss.lung | Phase2 | 200 | 2 | 1.5 | 240 | 0.5 | gas=1, dive=0.75 |
| boss.stomach | Default | 360 | 3 | 0.5 | 240 | 0.33333334 | melee=0.666667, charge=1, acid=0.666667, acid-dot=0.333333, short-range=1 |
| boss.stomach | Phase2 | 360 | 3 | 1 | 240 | 0.33333334 | melee=0.666667, charge=1, acid=0.666667, acid-dot=0.333333, short-range=1 |
| elite.h-02 | Default | 40 | 2 | 1 | 100 | 0.5 | ember-counter=1.5 |
| elite.h-03 | Default | 40 | 2 | 1 | 100 | 0.5 | hangover-burst=1.5 |
| elite.i-01 | Default | 40 | 2 | 1 | 100 | 0.5 | gas-burst=1.5, gas-orb=1.25 |
| elite.i-02 | Default | 40 | 2 | 1 | 100 | 0.5 | residue-landing=1.5 |
| legacy-elite.antibody | Default | 35 | 1 | 1.4 | 60 | 1 | - |
| legacy-elite.granuloma | Default | 30 | 1 | 1.2 | 50 | 1 | - |
| normal.bcell | Default | 4 | 1 | 0.8 | 15 | 1 | - |
| normal.intestine-parasite | Default | 6 | 1 | 2.3 | 0 | 1 | - |
| normal.intestinefoldworm | Default | 6 | 1 | 1.1 | 10 | 1 | - |
| normal.intestineknottedeel | Default | 8 | 1 | 0.9 | 12 | 1 | - |
| normal.intestinevillushand | Default | 6 | 1 | 0.8 | 10 | 1 | - |
| normal.liverclot | Default | 8 | 1 | 0.8 | 12 | 1 | - |
| normal.livercoagulationcore | Default | 6 | 1 | 0.8 | 15 | 1 | - |
| normal.liversuturecell | Default | 4 | 1 | 0.8 | 15 | 1 | - |
| normal.lungbreathbubble | Default | 4 | 1 | 0.8 | 15 | 1 | - |
| normal.lungciliabrush | Default | 6 | 1 | 1.1 | 10 | 1 | - |
| normal.lungoverinflatedalveoli | Default | 6 | 1 | 0.8 | 15 | 1 | - |
| normal.macrophage | Default | 6 | 1 | 1 | 10 | 1 | - |
| normal.nkcell | Default | 6 | 1 | 1 | 10 | 1 | - |
| normal.stomachgulp | Default | 4 | 1 | 0.8 | 15 | 1 | - |
| normal.stomachmucussnail | Default | 6 | 1 | 0.8 | 10 | 1 | - |
| normal.stomachoverfedsac | Default | 6 | 1 | 0.8 | 15 | 1 | - |
| elite.p-01 | Default | 40 | 2 | 1 | 100 | 0.5 | dust-cloud=1.5 |
| elite.p-03 | Default | 40 | 2 | 1 | 100 | 0.5 | pollen-volley=1.5, pollen-followup=1.5 |
| elite.s-01 | Default | 40 | 2 | 1 | 100 | 0.5 | helico-dash=1.5 |
| elite.s-03 | Default | 40 | 2 | 1 | 100 | 0.5 | oil-film-spread=1.5 |

## 스킬

| 직업·슬롯 | 피해 | 쿨타임 | 추가 효과 |
| --- | --- | --- | --- |
| 마법사 E | 5~7 | 7초 | 기절0.75초 |
| 마법사 R | 16 | 12초 | 0.7초 지연, 받는 피해+15% 4초 |
| 궁수 E | 5발 각1.5 | 6초 | 중독0.75/초 4초, 동일 중독 갱신 |
| 궁수 R | 직격8 / 조건 폭발6 | 10초 | 관통·감염 조건 유지 |
| 전사 E | 직접5 + 출혈1/초 3초 | 5초 | 전방140도, 최대8명 |
| 전사 R | 12 | 9초 | 구속1.5초, 보스는25% 지속 |

## 아이템 60종의 적용 설명

소수 피해는 최소1로 올림하지 않는다. 기능 유지 항목도 포함한다.

| ID | 이름 | 적용 설명 |
| --- | --- | --- |
| double_core | 이중핵 | 발사체 2개 발사 (각 70% 피해) |
| triple_core | 삼중핵 | 발사체 3개 발사 (각 50% 피해) |
| homing_cell | 유도 세포 | 적을 추적하는 유도 투사체 |
| hypertrophy_cell | 비대 세포 | 투사체 크기·충돌 범위 45%, 사거리 25% 증가 |
| reflux_organ | 역류 장기 | 왕복 경로마다 최대 3명 타격, 복귀 중 같은 적 재적중 시 60% 피해 |
| piercing_mucus | 관통 점액 | 최대 3명 관통, 관통할 때마다 피해 25% 감소 |
| laryngeal_nerve | 후두 신경 | 뒤쪽으로 75% 피해의 추가 투사체 발사 |
| beam_organ | 광선 기관 | 80% 피해의 관통 광선 공격 (다중핵·후방 발사·세포 증식 적용) |
| split_tissue | 분열 조직 | 적 명중 시 양옆으로 각각 35% 피해의 추가 투사체 생성 (추가 분열 없음) |
| explosive_blood_cell | 폭발 혈구 | 원거리 직접 피해 70%, 주변 적에게 같은 피해의 폭발 공격 |
| acidic_rupture | 산성 파열 | 적중 위치에 4초간 1초마다 0.5 피해를 주는 산성 장판 생성 (동시에 최대 2개) |
| cell_proliferation | 세포 증식 | 공격마다 25% 확률로 0.5초 후 70% 피해의 추가 공격 |
| pulse_bullet | 맥동 탄환 | 투사체가 이동하며 크기와 실제 충돌 범위가 증가하거나 감소 |
| vascular_reflection | 혈관 반사 | 벽에 튕기는 반사 투사체 |
| toxic_mucosa | 독성 점막 | 적중 시 4초간 1초마다 적중 피해의 15% 중독 피해 (지속 갱신) |
| freezing_nerve | 빙결 신경 | 공격 적중 시 2초간 적 이동속도 30% 감소 |
| hemorrhage_organ | 출혈 기관 | 적중 시 3.2초간 0.8초마다 적중 피해의 15% 출혈 피해 (지속 갱신) |
| overheated_organ | 과열 기관 | 연속 공격마다 공격속도 +6% (10중첩 시 주변 150% 피해, 자신 1 피해) |
| mutant_eye | 돌연변이 안구 | 발사 방향이 좌우 40도 내에서 무작위 변화, 기본 공격 피해 +0.75 |
| organ_tentacle | 장기 촉수 | 주변 적 최대 2명을 1.2초마다 30% 피해로 공격, 기본 공격 피해 20% 감소 |
| rampage_bloodflow | 폭주 혈류 | 이동 8초마다 기본 공격 피해 +0.25 (최대 +0.75), 정지 시 4초마다 감소 |
| muscle_spasm | 근육 경련 | 근접 공격 범위 1.7배, 공격속도 20% 감소 |
| unstable_core | 불안정 핵 | 5초마다 기본 공격 피해가 70~150% 사이에서 변경 |
| bio_resonance | 생체 공명 | 3초 내 같은 적 연속 공격 시 피해 +10% (최대 +30%) |
| blood_pressure_burst | 혈압 폭발 | 잃은 체력 10%당 공격속도 +6% (최대 +54%) |
| void_cell | 공허 세포 | 공격 시 20% 확률로 적 주변에서 70% 피해의 추가 탄환 생성 |
| forbidden_growth | 금단 성장 | 최대 체력 -3, 공격력 +0.75 |
| overclock_nerve | 과속 신경 | 체력 -2, 이동속도 +2 증가 |
| blood_contract | 피의 계약 | 적 20명 처치 시 최대 체력 +1 (최대 +4), 받는 피해 1.25배 |
| hyperplasia_heart | 과증식 심장 | 잃은 체력에 비례해 기본 공격 피해 증가 (최대 +0.75) |
| decay_organ | 부패 장기 | 전투 시간 3분마다 기본 공격 피해 +0.25 (최대 +0.75) |
| rupture_muscle | 파열 근육 | 공격 2회마다 기본 공격 피해 +0.2·이동속도 -0.25 (최대 3중첩) |
| imperfect_regeneration | 불완전 재생 | 최대 체력 -2, 피격 2초 후 체력 +1 회복 (15초 쿨타임) |
| severance_reflex | 절단 반사 | 피격 후 3초간 기본 공격 피해 +0.75 |
| bio_gamble | 생체 도박 | 성장량이 0배·1배·2배로 결정 (확률 20%·60%·20%) |
| exoskeleton | 외골격 | 받는 피해 25% 감소, 대신 이동속도 -1.5 |
| platelet_membrane | 혈소판 막 | 20초마다 피해 1을 막는 보호막 생성 |
| recovery_factor | 회복 인자 | 전투 시간 45초마다 체력 +1 회복 |
| reflective_skin | 반사 피부 | 피격 시 실제 받은 피해의 75% 반사 |
| bio_barrier | 생체 장막 | 정지 1.5초마다 받는 피해 10% 감소 (최대 30%) |
| split_regeneration | 분열 재생 | 한 런에 한 번, 체력이 0이 되기 직전 체력 4로 부활하고 1.5초 무적 |
| infected_host | 감염 숙주 | 잡몹 처치 시 15% 확률로 6초간 30% 피해의 아군 생성 (최대 2명) |
| spore_colony | 포자 군집 | 일정 시간마다 적에게 달려드는 작은 포자 생명체 소환 |
| blood_drone | 혈액 드론 | 1초마다 가까운 적에게 공격력의 25% 피해 |
| guardian_organ | 수호 장기 | 플레이어 주변을 맴돌며 4초마다 적 투사체 하나 방어 |
| tentacle_colony | 촉수 군체 | 3초마다 적 2명에게 1.2초간 50% 둔화와 10% 피해 |
| electric_neural_network | 전기 신경망 | 적 처치 시 최대 3명에게 피해 1의 연쇄 전류 (5초 쿨타임) |
| infection_transference | 감염 전이 | 적 처치 시 주변 3명에게 3초간 초당 0.5 감염 피해 (5초 쿨타임) |
| macrophage | 대식 세포 | 적 처치 시 5초간 공격력 +0.2 (최대 3중첩) |
| gluttonous_organ | 폭식 장기 | 적 처치 시 3초 동안 이동속도 증가 (최대 4스택) |
| heart_sniper | 심장 저격 | 체력 60% 이상인 적에게 직접 공격 피해 25% 증가 |
| bloodflow_acceleration | 혈류 가속 | 보스·엘리트 근처에서 공격속도 25% 증가 |
| focused_nerve | 집중 신경 | 주변 적 1명 이하일 때 기본 공격 피해 +0.5, 2~3명일 때 +0.25 |
| execution_instinct | 처형 본능 | 체력 20% 미만 적을 20% 확률로 처형 (보스·엘리트는 추가 피해 50%) |
| berserk_cell | 광폭 세포 | 보스전 진입 시 15초간 공격력 +0.5, 이동속도 +0.5, 공격속도 +25% |
| unstable_cell | 불안정 세포 | 투사체 속도 50~150%, 80% 이하의 느린 탄은 피해 130% |
| grotesque_growth | 기괴 성장 | 10초마다 소형화·이동속도 +1 또는 대형화·공격력 +0.5·이동속도 -0.5 |
| mutation_rampage | 돌연변이 폭주 | 15초마다 7초간 무작위 스탯 버프 또는 디버프 |
| parasitic_bomb | 기생 폭탄 | 적 처치 시 20% 확률로 반경 4에 공격력 100% 폭발 |
| frenzy_hormone | 광란 호르몬 | 피격 시 공격력 +0.5·이동속도 +1·공격속도 +20% 중 하나 (3초 지속, 8초 쿨타임) |

## 최종전 및 전투 공통

기둥180, 본체 공유HP1,600, 붕괴60초, 반사30%, 최종전 소환EXP0. 근접0.55초·원거리0.75초·피격 무적0.6초. 필드 엘리트 맵당2, 반복 엘리트40킬 간격.

## 월드 보상·추가 규칙

필드 상자3개/지역과 지역 보스 보상은 공격·생존·기능 후보 중 하나를 선택한다. 일반몹30/60/90킬에 각각HP2 회복을 드랍하며 지역당최대6이다. 선택지와수령 여부·회복 예산은 저장한다. 간 독비 재피격1초, 동일 투사체의 대상 재적중최소0.2초, 광폭 세포는 전투당한번이다. 자세한 구현 상태는 STATUS.md를 참고한다.
