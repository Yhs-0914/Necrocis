"""Export the saved, applied values. Run validate_assets.py first."""
from pathlib import Path
import json
import re

root = Path(__file__).resolve().parents[2]
data = json.loads((root / 'output/balance/validation_inputs.json').read_text())


def table(header, rows):
    return '| ' + ' | '.join(header) + ' |\n| ' + ' | '.join(['---'] * len(header)) + ' |\n' + ''.join(
        '| ' + ' | '.join(str(x).replace('|', '/') for x in row) + ' |\n' for row in rows) + '\n'


s = '# 실제 적용 수치 — 2026-10-26\n\n적용 자산과 런타임 설명을 추출했다. 수정 전 표는 CURRENT_VALUES.md·ITEM_CATALOG.md에 보존했다. 검증 한계는 IMPLEMENTATION.md 참고.\n\n'
s += '후속 적용값: 직업/E Lv5, R Lv10. 잡몹 스포너는 Lv1~5에1~2마리부터 시작해5레벨마다 최소/최대가1씩 증가한다. 직업 선택 후 캠페인당 시작 변이 보상1회가 추가됐다. [전체 재점검](REVIEW_2026-10-26.md)의 공속8·마력12·사거리6은 **미적용 제안**이며 아래 표가 현재값이다. 실제 성장 화면은 공속·사거리를 각각 선택한다.\n\n'
s += '## 플레이어 성장\n\n' + table(['능력치', '선택당 수치', '모드'], [
    [name, value['value'], '고정' if value['mode'] == 0 else '% 합산']
    for value in data['growth']['levelUpStatValues']
    for name in [{0: 'HP', 1: '이동', 2: '공격', 4: '공속', 5: '사거리', 6: '마력'}[value['statType']]]])
s += '## 진행 배율·최종 보상\n\n' + table(['이전 클리어 수', '공통 HP', '공격', '일반 EXP', '보스 HP', '필드 엘리트 EXP', '지역 보스 EXP'], [
    [v[k] for k in ['stage', 'maxHealth', 'attackPower', 'experience', 'bossHealth', 'fieldEliteExperience', 'bossExperience']]
    for v in data['stages']['stages']])
s += '## 몬스터 기본값\n\n노말 진행도0의 값. 이후 진행 배율·등급 보상을 적용한다. 폐 Phase2는 생존체의 새 체력이며 두 몸 각각에 전투 전체 HP를 부여하지 않는다.\n\n'
s += table(['ID', '페이즈', '기본 HP', '공격', '이동', '기본 EXP', '접촉 계수', '패턴 계수'], [
    [d['monsterId'], phase['id'], phase['maxHealth'], phase['attackPower'], phase['moveSpeed'], d['reward']['baseExperience'], d['contact']['damageCoefficient'],
     ', '.join(f"{v['id']}={v['coefficient']:g}" for v in d['patternDamage']) or '-']
    for d in data['definitions'] for phase in d['statSets']])
s += '## 스킬\n\n' + table(['직업·슬롯', '피해', '쿨타임', '추가 효과'], [
    ['마법사 E', '5~7', '7초', '기절0.75초'], ['마법사 R', '16', '12초', '0.7초 지연, 받는 피해+15% 4초'],
    ['궁수 E', '5발 각1.5', '6초', '중독0.75/초 4초, 동일 중독 갱신'], ['궁수 R', '직격8 / 조건 폭발6', '10초', '관통·감염 조건 유지'],
    ['전사 E', '직접5 + 출혈1/초 3초', '5초', '전방140도, 최대8명'], ['전사 R', '12', '9초', '구속1.5초, 보스는25% 지속']])
manager = (root / 'Assets/_Project/Scripts/Gameplay/Items/PlayerItemManager.cs').read_text(encoding='utf-8-sig')
effects = (root / 'Assets/_Project/Scripts/Gameplay/Items/PlayerItemCombatEffects.cs').read_text(encoding='utf-8-sig')
ids = dict(re.findall(r'const string (\w+)\s*=\s*"([^"]+)"', effects))
overrides = {ids.get(k, k): v for k, v in re.findall(r'entry.ItemId, PlayerItemCombatEffects\.(\w+).*?entry.SetDescription\("([^"]+)"\)', manager, re.S)}
items = re.findall(r'new PlayerItemEntry\(\s*"([^"]+)",\s*"([^"]+)",\s*"([^"]+)",\s*PlayerItemCategory\.(\w+)', manager)
assert len(items) == 60
s += '## 아이템 60종의 적용 설명\n\n소수 피해는 최소1로 올림하지 않는다. 기능 유지 항목도 포함한다.\n\n'
s += table(['ID', '이름', '적용 설명'], [[i, n, overrides.get(i, d)] for i, n, d, c in items])
s += '## 최종전 및 전투 공통\n\n기둥180, 본체 공유HP1,600, 붕괴60초, 반사30%, 최종전 소환EXP0. 근접0.55초·원거리0.75초·피격 무적0.6초. 필드 엘리트 맵당2, 반복 엘리트40킬 간격.\n'
s += '\n## 월드 보상·추가 규칙\n\n필드 상자3개/지역과 지역 보스 보상은 공격·생존·기능 후보 중 하나를 선택한다. 일반몹30/60/90킬에 각각HP2 회복을 드랍하며 지역당최대6이다. 선택지와수령 여부·회복 예산은 저장한다. 간 독비 재피격1초, 동일 투사체의 대상 재적중최소0.2초, 광폭 세포는 전투당한번이다. 자세한 구현 상태는 STATUS.md를 참고한다.\n'
(root / 'Docs/Balance/APPLIED_VALUES.md').write_text(s, encoding='utf-8')
print('Exported Docs/Balance/APPLIED_VALUES.md')
