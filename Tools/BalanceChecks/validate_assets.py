"""Run from repo root. Requires PyYAML; exports actual assets for the C# math harness."""
from pathlib import Path
from itertools import permutations
import json
import re
import yaml

ROOT = Path(__file__).resolve().parents[2]
assertions = 0


def require(condition, message):
    global assertions
    assert condition, message
    assertions += 1


def documents(path):
    text = (ROOT / path).read_text(encoding='utf-8-sig')
    text = re.sub(r'^%.*\n', '', text, flags=re.M)
    text = re.sub(r'^--- !u!\d+ &[^\n]+', '---', text, flags=re.M)
    return list(yaml.safe_load_all(text))


def asset(path):
    return documents(path)[0]['MonoBehaviour']


defs = []
for path in sorted((ROOT / 'Assets/_Project/Data/MonsterBalance/Definitions').glob('*.asset')):
    value = asset(path)
    value['contact']['enabled'] = bool(value['contact']['enabled'])
    value['pattern'] = None  # Controllers require Unity. Actual damage coefficients are retained.
    defs.append(value)
require(len(defs) == 30, 'all 30 monster definitions')
growth = asset('Assets/_Project/Resources/Balance/LevelProgressionConfig.asset')
hard_growth = asset('Assets/_Project/Resources/Balance/Difficulty/HardLevelProgressionConfig.asset')
for mode, progression in [('Normal', growth), ('Hard', hard_growth)]:
    require(progression['jobSelectionLevel'] == 5 and progression['skill1UnlockLevel'] == 5, mode + ' job and E at level 5')
    require(progression['jobBasedChoiceStartLevel'] == 6 and progression['skill2UnlockLevel'] == 10, mode + ' job growth at level 6 and R at level 10')
    require(progression['normalEnemySpawnLevelInterval'] == 5, mode + ' normal enemy population increases every 5 levels')
    require(progression['normalEnemySpawnMinCount'] == 1, mode + ' initial minimum population')
    require(progression['normalEnemySpawnMaxCount'] == 2, mode + ' initial maximum population')
    require(progression['enemyHealthLevelInterval'] == 5, mode + ' five-level health interval')
    require(progression['normalEnemyHealthBonusPerBand'] == 2, mode + ' normal health grows by a flat 2 per band')
    require('normalEnemyHealthIncreasePerBand' not in progression, mode + ' old normal health percentage removed')
    require(progression['eliteEnemyHealthIncreasePerBand'] == .12, mode + ' elite health growth')
    expected_growth = {2: (0, 1), 5: (1, 10), 4: (1, 20), 6: (0, 25), 1: (1, 5), 0: (0, 2)}
    for stat, (modifier_mode, amount) in expected_growth.items():
        entries = [entry for entry in progression['levelUpStatValues'] if entry['statType'] == stat]
        require(len(entries) == 1 and (entries[0]['mode'], entries[0]['value']) == (modifier_mode, amount),
                mode + ' level-up stat ' + str(stat))
    require(next(entry for entry in progression['levelUpStatValues'] if entry['statType'] == 0)['healWhenPositive'] == 1,
            mode + ' health choice also heals the added heart')
growth['bioGambleEnabled'] = bool(growth['bioGambleEnabled'])
for value in growth['levelUpStatValues']:
    value['healWhenPositive'] = bool(value['healWhenPositive'])
stages = asset('Assets/_Project/Data/MonsterBalance/MonsterProgressionProfile.asset')
effect_source = (ROOT / 'Assets/_Project/Scripts/Gameplay/Items/PlayerItemCombatEffects.cs').read_text(encoding='utf-8-sig')
effect_defaults = {k: float(v) for k, v in re.findall(r'\b(?:float|int) (\w+)\s*=\s*([.\d]+)f?;', effect_source)}

skills = {'mageSkill1': {'additionalDamageMin': 5, 'additionalDamageMax': 7, 'stunDuration': .75},
          'mageSkill2': {'baseDamage': 16, 'detonationDelay': .7, 'damageTakenIncreaseRatio': .15},
          'archerSkill1': {'projectileDamage': 1.5, 'poisonTickDamage': .75, 'poisonDuration': 4},
          'archerSkill2': {'projectileDamage': 8, 'virusExplosionDamage': 6},
          'warriorSkill1': {'damage': 5, 'cooldown': 5, 'bleedDuration': 3},
          'warriorSkill2': {'damage': 12, 'cooldown': 9, 'rootDuration': 1.5}}
for path in ['Assets/_Project/Scenes/Hub.unity', 'Assets/_Project/Art/Generated/FinalBoss/FinalBossPlayerFallback.prefab']:
    components = {v['MonoBehaviour'].get('m_EditorClassIdentifier', '').split('::')[-1]: v['MonoBehaviour']
                  for v in documents(path) if v and 'MonoBehaviour' in v}
    attack = components['Necrocis.PlayerAttack']
    require(attack['meleeCooldown'] == .55 and attack['attackCooldown'] == .75, path + ' attack cooldowns')
    require(components['Necrocis.Health']['invincibilityDuration'] == .6, path + ' hit immunity')
    require('maxItemSlots' not in components['Necrocis.PlayerItemManager'], path + ' old slot cap removed')
    require(not components['Necrocis.PlayerItemManager']['allowDuplicateItems'], path + ' unique items')
    for group, values in skills.items():
        for key, expected in values.items():
            require(components['Necrocis.PlayerClassSkillController'][group][key] == expected, path + '/' + group + '/' + key)
    if 'Necrocis.PlayerItemCombatEffects' in components:
        effects = components['Necrocis.PlayerItemCombatEffects']
        for key, value in {'doubleShotDamageMultiplier': .7, 'tripleShotDamageMultiplier': .5,
                           'recoveryFactorInterval': 45, 'splitRegenerationReviveHealth': 4,
                           'rampageMaxAttackBonus': 3, 'decayOrganMaxAttackBonus': 6,
                           'forbiddenGrowthMaxHealthPenalty': 2, 'forbiddenGrowthFlatAttackBonus': 3,
                           'decayOrganSecondsPerAttackBonus': 90, 'decayOrganAttackBonusPerStack': .5}.items():
            require(effects.get(key, effect_defaults.get(key)) == value, path + '/' + key)

for region in ['Intestine', 'Liver', 'Stomach', 'Lung']:
    elite = asset(f'Assets/_Project/Data/BiomeElites/{region}BiomeEliteSpawnConfig.asset')
    require(elite['minimumCount'] == 2 and elite['maximumCount'] == 2, region + ' two field elites')
    arena = asset(f'Assets/_Project/Data/BiomeConfigs/{region}BossArenaConfig.asset')
    require('guid' in arena['midBossArena']['boss']['bossRule']['monsterDefinition'], region + ' connected boss definition')
for order in permutations(['intestine', 'liver', 'stomach', 'lung']):
    for index, region in enumerate(order):
        definition = next(d for d in defs if d['monsterId'] == 'boss.' + region)
        require(stages['stages'][index]['bossExperience'] >= 240 and definition['reward']['baseExperience'] > 0,
                str(order) + '/' + region)
final = asset('Assets/_Project/Resources/Balance/FinalBoss/FinalBossBalanceConfig.asset')
require(final['phaseOne']['pillarMaxHealth'] == 180 and final['phaseTwo']['bossMaxHealth'] == 1600, 'final boss HP')
require(final['normal']['summonExperience'] == 0 and final['hard']['summonExperience'] == 0, 'summon XP disabled')
require(final['phaseThree']['collapseDuration'] == 60, 'collapse timeout')
liver_hazards = [v['MonoBehaviour'] for v in documents('Assets/_Project/Scenes/Liver.unity')
                if v and 'MonoBehaviour' in v and 'LiverToxicRain' in v['MonoBehaviour'].get('m_EditorClassIdentifier', '')]
require(len(liver_hazards) == 1 and liver_hazards[0]['hitCooldown'] == 1, 'liver environmental hit cooldown')
inputs = {'growth': growth, 'stages': stages,
          'normal': asset('Assets/_Project/Resources/Balance/Difficulty/NormalDifficultyBalance.asset'),
          'hard': asset('Assets/_Project/Resources/Balance/Difficulty/HardDifficultyBalance.asset'), 'definitions': defs}
manager_source = (ROOT / 'Assets/_Project/Scripts/Gameplay/Items/PlayerItemManager.cs').read_text(encoding='utf-8-sig')
inputs['itemIds'] = re.findall(r'new PlayerItemEntry\(\s*"([^"]+)"', manager_source)
stat_source = (ROOT / 'Assets/_Project/Scripts/Gameplay/Items/PlayerStatItemCatalog.cs').read_text(encoding='utf-8-sig')
inputs['itemIds'] += re.findall(r'new Definition\(\s*"([^"]+)"', stat_source)
require(len(inputs['itemIds']) == 68 and len(set(inputs['itemIds'])) == 68, 'all 68 offer candidates')
require(inputs['normal']['elites']['fixedHitDamage'] == 3, 'normal elite damage is 1.5 hearts')
require(inputs['hard']['elites']['fixedHitDamage'] == 4, 'hard elite damage is 2 hearts')
output = ROOT / 'output/balance'
output.mkdir(parents=True, exist_ok=True)
(output / 'validation_inputs.json').write_text(json.dumps(inputs, ensure_ascii=True), encoding='utf-8')
report = f'PASS: {assertions} serialized asset assertions, including all 24 region orders.\n'
(output / 'asset_checks.txt').write_text(report, encoding='utf-8')
print(report.strip())
