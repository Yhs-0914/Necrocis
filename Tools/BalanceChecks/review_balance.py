"""Generate arithmetic evidence for the 2026-10-26 balance review; never edits game assets."""
from pathlib import Path
import hashlib
import json
import math
import re
import struct
import yaml

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Docs/Balance'
sources = set()


def read(path):
    sources.add(path)
    return (ROOT / path).read_text(encoding='utf-8-sig')


def documents(path):
    text = re.sub(r'^%.*\n', '', read(path), flags=re.M)
    text = re.sub(r'^--- !u!\d+ &[^\n]+', '---', text, flags=re.M)
    return list(yaml.safe_load_all(text))


def asset(path):
    return documents(path)[0]['MonoBehaviour']


def table(headers, rows):
    return '| ' + ' | '.join(headers) + ' |\n| ' + ' | '.join(['---'] * len(headers)) + ' |\n' + ''.join(
        '| ' + ' | '.join(str(v) for v in row) + ' |\n' for row in rows) + '\n'


growth = asset('Assets/_Project/Resources/Balance/LevelProgressionConfig.asset')
stages = asset('Assets/_Project/Data/MonsterBalance/MonsterProgressionProfile.asset')['stages']
normal = asset('Assets/_Project/Resources/Balance/Difficulty/NormalDifficultyBalance.asset')
hard = asset('Assets/_Project/Resources/Balance/Difficulty/HardDifficultyBalance.asset')
hard_growth = asset('Assets/_Project/Resources/Balance/Difficulty/HardLevelProgressionConfig.asset')
definitions = [asset(p.relative_to(ROOT).as_posix()) for p in sorted(
    (ROOT / 'Assets/_Project/Data/MonsterBalance/Definitions').glob('*.asset'))]
final = asset('Assets/_Project/Resources/Balance/FinalBoss/FinalBossBalanceConfig.asset')
hub = documents('Assets/_Project/Scenes/Hub.unity')
components = {v['MonoBehaviour'].get('m_EditorClassIdentifier', '').split('::')[-1]: v['MonoBehaviour']
              for v in hub if v and 'MonoBehaviour' in v}


def required(level):
    return growth['baseExp'] + growth['expIncreasePerLevel'] * (level - 1)


def alternative(level):
    # Conditional proposal only: preserve levels 1..10, soften the slope afterwards.
    return required(level) if level <= 10 else required(10) + 16 * (level - 10)


def float32(value):
    # Unity serializes these multipliers into C# float before Resolver promotes to double.
    return struct.unpack('f', struct.pack('f', value))[0]


lines = ['# 밸런스 재점검 계산표 — 2026-10-26\n\n',
         '생성: `python Tools/BalanceChecks/review_balance.py`. 저장 자산과 명시한 가정으로 계산한 값이며 플레이 실측이 아니다.\n\n',
         '## 1. 경험치 전체 곡선\n\n',
         '잡몹 처치 수는 진행도0, 한 마리당 정확히 EXP10 또는 EXP15만 얻을 때의 누적 환산값이다. 엘리트·보스 보상은 제외했다.\n\n']
xp_rows = []
for level in range(1, growth['maxLevel'] + 1):
    cumulative = sum(required(i) for i in range(1, level))
    band = (level - 1) // growth['normalEnemySpawnLevelInterval']
    xp_rows.append([level, required(level) if level < growth['maxLevel'] else 'MAX', cumulative,
                    f'{math.ceil(cumulative / 15)}~{math.ceil(cumulative / 10)}',
                    f"{growth['normalEnemySpawnMinCount'] + band}~{growth['normalEnemySpawnMaxCount'] + band}"])
lines.append(table(['레벨', '다음 레벨 EXP', '누적 EXP', '누적 잡몹 환산', '스포너당 수'], xp_rows))
lines.append('## 2. 해금·보상 시간 민감도\n\n평균 EXP12/킬, 지역0, 일반몹만 처치, UI/이동 시간까지 포함한 분당 처치 수를 가정했다. 엘리트 보상이 들어오면 해금은 앞당겨진다. 첫 반복 엘리트는 현재 장에서만 해당한다.\n\n')
lines.append(table(['분당 처치', 'Lv5 분', 'Lv10 분', '회복30킬 분', '반복엘리트40킬 분'],
                   [[rate, round(240 / (12 * rate), 2), round(990 / (12 * rate), 2),
                     round(30 / rate, 2), round(40 / rate, 2)] for rate in [4.4, 8, 12, 18]]))
lines.append('## 3. 후반 경험치 완화 대안 — 미적용\n\n기본 권고는 현행 유지다. Lv10 이후 성장이 실제로 느릴 때만 `Q(L)=210+16×(L−10)`을 비교한다.\n\n')
lines.append(table(['도달 레벨', '현재 누적', '대안 누적', '감소'],
                   [[level, sum(required(i) for i in range(1, level)), sum(alternative(i) for i in range(1, level)),
                     sum(required(i) - alternative(i) for i in range(1, level))] for level in [5, 10, 15, 20, 25, 30]]))
lines.append('## 4. 보상과 적 성장\n\n일반 EXP는 원본 보상에 진행 배율을 곱한 후 최근접 짝수 반올림이다. 전용 엘리트와 지역 보스는 별도 보상표를 사용한다.\n\n')
lines.append(table(['진행도', 'HP배율', '공격배율', 'EXP10 최종', 'EXP12 최종', 'EXP15 최종', '전용엘리트', '보스'],
                   [[s['stage'], s['maxHealth'], s['attackPower'], *[round(x * float32(s['experience'])) for x in [10, 12, 15]],
                     s['fieldEliteExperience'], s['bossExperience']] for s in stages]))
lines.append('## 5. 지역별 스폰 규칙\n\n규칙 수는 실제 만나는 개체 수가 아니다. 배치기·지형·활성 거리도 영향을 준다.\n\n')
region_rows = []
for region in ['Intestine', 'Liver', 'Lung', 'Stomach']:
    config = asset(f'Assets/_Project/Data/BiomeConfigs/{region}EnemySpawnConfig.asset')
    rules = config['enemySpawnRules']
    ordinary = [r for r in rules if not r.get('isElite')]
    elite = [r['name'] for r in rules if r.get('isElite')]
    region_rows.append([region, len(ordinary), ', '.join(elite) or '등록 없음', config['normalKillsPerElite'],
                        ', '.join(str(x) for x in sorted({r['respawnCooldown'] for r in ordinary}))])
lines.append(table(['지역', '일반 규칙 수', '반복 엘리트', '설정 킬 간격', '일반 재생성 초'], region_rows))
lines.append('## 6. 공격·공속·마력 1회 선택의 상대 효율\n\n아이템 없는 예시이며 전체 DPS가 아니라 각 능력치가 담당하는 공격의 증가율이다.\n\n')
lines.append(table(['항목', '현재 상태', '현재 선택 증가율', '제안 선택 증가율'], [
    ['공격력', '공격1', '25.00%', '25.00% (+0.25 유지)'],
    ['공격력', '공격4', '6.25%', '6.25%'],
    ['공속', '공속1', '5.00%', '8.00%'],
    ['공속', '공속1.4', f'{.05/1.4*100:.2f}%', f'{.08/1.4*100:.2f}%'],
    ['마력', '마력1 → 배율1.01', f'{.08/1.01*100:.2f}%', f'{.12/1.01*100:.2f}%'],
    ['마력', '마력81 → 배율1.81', f'{.08/1.81*100:.2f}%', f'{.12/1.81*100:.2f}%']]))
attack = components['Necrocis.PlayerAttack']
lines.append('## 7. 평타 이상적 수치\n\n매번 쿨타임 직후 입력·명중한다고 가정한다. 실제 입력은 누를 때만 발동하며 Q/W의 타이머가 독립이다. 이동·회피·빗나감은 제외했다.\n\n')
lines.append(table(['공격력', '공속', '근접 DPS', '원거리 DPS'],
                   [[a, speed, round(a * speed / attack['meleeCooldown'], 3), round(a * speed / attack['attackCooldown'], 3)]
                    for a, speed in [(1, 1), (1.25, 1), (1, 1.05), (2, 1.2), (4, 1.4)]]))
lines.append('## 8. 몬스터 정의 원본\n\nHP는 정의의 기본값이며 진행 배율 적용 전이다. 보스 페이즈별 HP를 단순 합산하면 안 된다.\n\n')
lines.append(table(['ID', '등급', 'HP(페이즈)', '공격(페이즈)', 'EXP원본'],
                   [[d['monsterId'], d['tier'], ', '.join(f"{s['id']}:{s['maxHealth']}" for s in d['statSets']),
                     ', '.join(f"{s['id']}:{s['attackPower']}" for s in d['statSets']), d['reward']['baseExperience']] for d in definitions]))
lines.append('## 9. 최종 보스 이론 하한\n\n기둥4개와 본체를 순차 공격하고 회복·무적·소환물·이동을 제외한 환산이다. 본체 40% 전환과 10% 붕괴는 공유 HP 일부이며 새 HP가 아니다.\n\n')
hp = 4 * final['phaseOne']['pillarMaxHealth'] + final['phaseTwo']['bossMaxHealth']
lines.append(table(['실효 DPS 가정', '기둥+본체 피해시간 분'], [[dps, round(hp / dps / 60, 2)] for dps in [3, 5, 7, 10]]))
lines.append(f"붕괴 시작 잔여 HP 최대 {final['phaseTwo']['bossMaxHealth'] * final['phaseThree']['collapseHealthRatio']:g}, "
             f"{final['phaseThree']['collapseDuration']:g}초 안에 제거할 DPS는 "
             f"{final['phaseTwo']['bossMaxHealth'] * final['phaseThree']['collapseHealthRatio'] / final['phaseThree']['collapseDuration']:.3f}. "
             '60초는 즉사 판정이 아니라 영역 축소 및 초과시간 피해와 연결된다.\n')

OUT.mkdir(parents=True, exist_ok=True)
(OUT / 'REVIEW_TABLES_2026-10-26.md').write_text(''.join(lines), encoding='utf-8')
for folder in ['Assets/_Project/Scripts/Gameplay/Player', 'Assets/_Project/Scripts/Gameplay/Items',
               'Assets/_Project/Scripts/UI/LevelUp', 'Assets/_Project/Scripts/Core/Save',
               'Assets/_Project/Scripts/Core/Balance', 'Assets/_Project/Scripts/Gameplay/Enemies/Spawning',
               'Assets/_Project/Scripts/Gameplay/Enemies/StatusEffects', 'Assets/_Project/Scripts/World/FinalBoss']:
    sources.update(p.relative_to(ROOT).as_posix() for p in (ROOT / folder).rglob('*.cs'))
sources.update(['Assets/_Project/Scripts/Core/Stats/CharacterStats.cs',
                'Assets/_Project/Scripts/World/Biomes/Runtime/ProceduralBiomeBridge.cs',
                'Assets/_Project/Scripts/World/Biomes/Runtime/ConfigurableBiomeManager.cs',
                'Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyCombat.cs',
                'Tools/BalanceReview/Program.cs',
                'Tools/BalanceReview/BalanceReview.csproj',
                'Tools/BalanceChecks/EngineShim.cs',
                'Tools/BalanceChecks/review_balance.py'])
(OUT / 'REVIEW_SOURCE_HASHES_2026-10-26.json').write_text(json.dumps(
    {p: hashlib.sha256((ROOT / p).read_bytes()).hexdigest() for p in sorted(sources)}, indent=2), encoding='utf-8')
print(f'Generated review tables: {len(xp_rows)} levels, {len(definitions)} monster definitions, {len(sources)} source hashes.')
