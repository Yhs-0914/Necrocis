# 수정 전 수치 인벤토리 — 2026-10-25

정적 파일 조사 결과이며 제안값이 아니다. 경로와 세부 필드는 SOURCE_SNAPSHOT.md 참고. 연결된 MonsterDefinition이 스폰 설정의 구형 HP/EXP보다 우선한다.

## 몬스터 전체 정의

| ID | 표시명 | 페이즈 | HP 원값 | 공격력 | 이속 | EXP | 접촉 계수 | 패턴 계수 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| boss.intestine | IntestineBoss | Default | 10 | 2 | 1.5 | 0 | 0.5 | dung=0.5, stomp=1 |
| boss.intestine | IntestineBoss | Phase2 | 10 | 2 | 1.7249999 | 0 | 0.5 | dung=0.5, stomp=1 |
| boss.liver | LiverBoss | Default | 180 | 2 | 1 | 0 | 0.5 | blood-bomb=1 |
| boss.liver | LiverBoss | Phase2 | 180 | 2 | 1.5 | 0 | 0.5 | blood-bomb=1 |
| boss.lung | LungBoss | Default | 40 | 2 | 1 | 0 | 0.5 | gas=1, dive=0.75 |
| boss.lung | LungBoss | Phase2 | 100 | 4 | 1.5 | 0 | 0.5 | gas=1, dive=0.75 |
| boss.stomach | StomachBoss | Default | 10 | 3 | 0.5 | 0 | 0.33333334 | melee=1, charge=1.3333334, acid=0.6666667, acid-dot=0.33333334, short-range=1.6666666 |
| boss.stomach | StomachBoss | Phase2 | 10 | 3 | 1 | 0 | 0.33333334 | melee=1, charge=1.3333334, acid=0.6666667, acid-dot=0.33333334, short-range=1.6666666 |
| elite.h-02 | H-02 염증 불씨 | Default | 30 | 2 | 1 | 50 | 0.5 | ember-counter=1.5 |
| elite.h-03 | H-03 숙취 잔재 | Default | 30 | 2 | 1 | 50 | 0.5 | hangover-burst=1.5 |
| elite.i-01 | I-01 가스낭 | Default | 30 | 2 | 1 | 50 | 0.5 | gas-burst=1.5, gas-orb=1.25 |
| elite.i-02 | I-02 굳은 잔여체 | Default | 30 | 2 | 1 | 50 | 0.5 | residue-landing=1.5 |
| legacy-elite.antibody | Antibody | Default | 35 | 1 | 1.4 | 60 | 1 | - |
| legacy-elite.granuloma | Granuloma | Default | 30 | 1 | 1.2 | 50 | 1 | - |
| normal.bcell | BCell | Default | 4 | 1 | 0.8 | 15 | 1 | - |
| normal.intestine-parasite | 장 보스 소환 기생충 | Default | 6 | 1 | 2.3 | 0 | 1 | - |
| normal.intestinefoldworm | IntestineFoldworm | Default | 6 | 1 | 1.1 | 10 | 1 | - |
| normal.intestineknottedeel | IntestineKnottedEel | Default | 8 | 1 | 0.9 | 12 | 1 | - |
| normal.intestinevillushand | IntestineVillusHand | Default | 6 | 1 | 0.8 | 10 | 1 | - |
| normal.liverclot | LiverClot | Default | 8 | 1 | 0.8 | 12 | 1 | - |
| normal.livercoagulationcore | LiverCoagulationCore | Default | 6 | 1 | 0.8 | 15 | 1 | - |
| normal.liversuturecell | LiverSutureCell | Default | 4 | 1 | 0.8 | 15 | 1 | - |
| normal.lungbreathbubble | LungBreathBubble | Default | 4 | 1 | 0.8 | 15 | 1 | - |
| normal.lungciliabrush | LungCiliaBrush | Default | 6 | 1 | 1.1 | 10 | 1 | - |
| normal.lungoverinflatedalveoli | LungOverinflatedAlveoli | Default | 6 | 1 | 0.8 | 15 | 1 | - |
| normal.macrophage | Macrophage | Default | 6 | 1 | 1 | 10 | 1 | - |
| normal.nkcell | NKCell | Default | 6 | 1 | 1 | 10 | 1 | - |
| normal.stomachgulp | StomachGulp | Default | 4 | 1 | 0.8 | 15 | 1 | - |
| normal.stomachmucussnail | StomachMucusSnail | Default | 6 | 1 | 0.8 | 10 | 1 | - |
| normal.stomachoverfedsac | StomachOverfedSac | Default | 6 | 1 | 0.8 | 15 | 1 | - |
| elite.p-01 | P-01 분진 뭉치 | Default | 30 | 2 | 1 | 50 | 0.5 | dust-cloud=1.5 |
| elite.p-03 | P-03 꽃가루 침입자 | Default | 30 | 2 | 1 | 50 | 0.5 | pollen-volley=1.5, pollen-followup=1.5 |
| elite.s-01 | S-01 헬리코 나선충 | Default | 30 | 2 | 1 | 50 | 0.5 | helico-dash=1.5 |
| elite.s-03 | S-03 기름막 활주체 | Default | 30 | 2 | 1 | 50 | 0.5 | oil-film-spread=1.5 |

## 현재 레벨별 요구 경험치

일반 처치 경험치는 레벨별 2배/0.8배를 거치지 않는 AddResolvedEnemyKillExp 경로. 누적은 Lv1부터 해당 레벨 도달까지.

| 현재 레벨 | 다음 레벨 요구 EXP | 현재 레벨 도달 누적 |
| --- | --- | --- |
| 1 | 30 | 0 |
| 2 | 50 | 30 |
| 3 | 70 | 80 |
| 4 | 90 | 150 |
| 5 | 110 | 240 |
| 6 | 130 | 350 |
| 7 | 150 | 480 |
| 8 | 170 | 630 |
| 9 | 190 | 800 |
| 10 | 210 | 990 |
| 11 | 230 | 1200 |
| 12 | 250 | 1430 |
| 13 | 270 | 1680 |
| 14 | 290 | 1950 |
| 15 | 310 | 2240 |
| 16 | 330 | 2550 |
| 17 | 350 | 2880 |
| 18 | 370 | 3230 |
| 19 | 390 | 3600 |
| 20 | 410 | 3990 |
| 21 | 430 | 4400 |
| 22 | 450 | 4830 |
| 23 | 470 | 5280 |
| 24 | 490 | 5750 |
| 25 | 510 | 6240 |
| 26 | 530 | 6750 |
| 27 | 550 | 7280 |
| 28 | 570 | 7830 |
| 29 | 590 | 8400 |
| 30 | 최대 레벨 | 8990 |

## 일반 스폰 설정

maxAlive는 규칙/스포너 값이며 맵 전체 동시 개체 수가 아니다. density를 분당 처치 수로 직접 환산할 수 없다.

| 설정 | 종류 | 밀도 | 최대 생존 | 재생성 초 | 공격 대기 초 | 사거리 | 엘리트 | 정의 GUID |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| IntestineEnemySpawnConfig | Macrophage | 0.05 | 2 | 5 | 1 | 4 | 0 | 650bae08b30304f06acc8eb4119faeac |
| IntestineEnemySpawnConfig | NKCell | 0.07 | 3 | 4 | 1 | 1.5 | 0 | b025256d6048b45cba4d3e38214b1bac |
| IntestineEnemySpawnConfig | BCell | 0.04 | 2 | 6 | 1.5 | 7 | 0 | 38da51144b79d46af83b7d0260844d36 |
| IntestineEnemySpawnConfig | Granuloma | 0.01 | 1 | 30 | 1 | 4 | 1 | 7ac5acdc9d7ad4d9f8c020f3491bb0e2 |
| IntestineEnemySpawnConfig | Antibody | 0.01 | 1 | 30 | 0.8 | 1.5 | 1 | 9e12b36d011fb490ba7505d3e3219228 |
| IntestineEnemySpawnConfig | IntestineFoldworm | 0.02 | 2 | 6 | 1 | 1.5 | 0 | fa0e27fbe157f4123897b7576c0f6c43 |
| IntestineEnemySpawnConfig | IntestineVillusHand | 0.02 | 2 | 6 | 1.2 | 1.8 | 0 | 78210f5a2c21844c1b8ec2acc9654ab2 |
| IntestineEnemySpawnConfig | IntestineKnottedEel | 0.02 | 2 | 6 | 1.2 | 1.7 | 0 | 78699e52925f548d1acecb49cb2d265f |
| LiverEnemySpawnConfig | LiverSutureCell | 0.05 | 2 | 6 | 1.5 | 7 | 0 | 83e58fda032b14a40b4a66d7631a4558 |
| LiverEnemySpawnConfig | LiverClot | 0.05 | 2 | 6 | 1.2 | 1.5 | 0 | b9f33a6fa1cea4cdeb17d27e9cc9f8c0 |
| LiverEnemySpawnConfig | LiverCoagulationCore | 0.05 | 2 | 6 | 1.8 | 6.5 | 0 | d29c7a3924e2c4b068c031c5159dacfb |
| LungEnemySpawnConfig | LungBreathBubble | 0.05 | 2 | 6 | 1.5 | 7 | 0 | 499aed563f6cc4da79eaad32bd62751b |
| LungEnemySpawnConfig | LungCiliaBrush | 0.05 | 2 | 6 | 1 | 1.7 | 0 | 09c64a797ef7441ad977d9749ce1b1ab |
| LungEnemySpawnConfig | LungOverinflatedAlveoli | 0.05 | 2 | 6 | 1.8 | 6.5 | 0 | 643bd1d9f29654393b97f17d249d2599 |
| StomachEnemySpawnConfig | StomachGulp | 0.05 | 2 | 6 | 1.5 | 7 | 0 | ed14b06444759459aa397748f15059de |
| StomachEnemySpawnConfig | StomachMucusSnail | 0.05 | 2 | 6 | 1.1 | 1.5 | 0 | 5776acaa48a8544d2b6221fe5ca01aab |
| StomachEnemySpawnConfig | StomachOverfedSac | 0.05 | 2 | 6 | 1.8 | 6.5 | 0 | 83507b86eb762412da73736941f79ecf |

## 필드 엘리트 배치

| 맵 | 최소 | 최대 | 종별 최소 | 간격 | 활성 거리 | 해제 거리 |
| --- | --- | --- | --- | --- | --- | --- |
| IntestineBiomeEliteSpawnConfig | 2 | 2 | 1 | 64 | 48 | 72 |
| LiverBiomeEliteSpawnConfig | 2 | 2 | 1 | 64 | 48 | 72 |
| LungBiomeEliteSpawnConfig | 2 | 2 | 1 | 64 | 48 | 72 |
| StomachBiomeEliteSpawnConfig | 2 | 2 | 1 | 64 | 48 | 72 |

