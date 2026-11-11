# 수정 전 아이템 카탈로그 — 2026-10-25

총 60종. 설명은 런타임 설명 교체를 반영한 표기이며 실제 계산·정수 반올림과 구분한다. 효과 필드 기본값과 Hub 직렬화 값은 SOURCE_SNAPSHOT.md에 보존했다. 삭제 ID: parasitic_spore / neural_overload / coagulation_cell.

| ID | 이름 | 분류 | 현재 설명 |
| --- | --- | --- | --- |
| double_core | 이중핵 | BasicProjectile | 발사체 2개 발사 (각 75% 피해) |
| triple_core | 삼중핵 | BasicProjectile | 발사체 3개 발사 (각 55% 피해) |
| homing_cell | 유도 세포 | BasicProjectile | 적을 추적하는 유도 투사체 |
| hypertrophy_cell | 비대 세포 | BasicProjectile | 투사체 크기·충돌 범위 45%, 사거리 25% 증가 |
| reflux_organ | 역류 장기 | BasicProjectile | 왕복 경로마다 최대 3명 타격, 복귀 중 같은 적 재적중 시 75% 피해 |
| piercing_mucus | 관통 점액 | BasicProjectile | 최대 3명 관통, 관통할 때마다 피해 25% 감소 |
| laryngeal_nerve | 후두 신경 | BasicProjectile | 뒤쪽으로 추가 투사체 발사 |
| beam_organ | 광선 기관 | BasicProjectile | 80% 피해의 관통 광선 공격 (다중핵·후방 발사·세포 증식 적용) |
| split_tissue | 분열 조직 | BasicProjectile | 적 명중 시 양옆으로 각각 50% 피해의 추가 투사체 생성 |
| explosive_blood_cell | 폭발 혈구 | BasicProjectile | 원거리 피해가 50% 감소하고 범위 폭발 공격으로 변경 |
| acidic_rupture | 산성 파열 | BasicProjectile | 적중 위치에 4초간 1초마다 0.5 피해를 주는 산성 장판 생성 (동시에 최대 2개) |
| cell_proliferation | 세포 증식 | BasicProjectile | 일정 확률로 공격이 한 번 더 발동 |
| pulse_bullet | 맥동 탄환 | BasicProjectile | 투사체가 이동하며 크기와 실제 충돌 범위가 증가하거나 감소 |
| vascular_reflection | 혈관 반사 | BasicProjectile | 벽에 튕기는 반사 투사체 |
| toxic_mucosa | 독성 점막 | BasicProjectile | 적중 시 4초간 1초마다 적중 피해의 20% 중독 피해 |
| freezing_nerve | 빙결 신경 | BasicProjectile | 공격 적중 시 2초간 적 이동속도 30% 감소 |
| hemorrhage_organ | 출혈 기관 | BasicProjectile | 적중 시 3.5초간 0.8초마다 적중 피해의 18% 출혈 피해 |
| overheated_organ | 과열 기관 | BasicProjectile | 연속 공격마다 공격속도 +10% (10중첩 시 주변 150% 피해, 자신 1 피해) |
| mutant_eye | 돌연변이 안구 | BasicProjectile | 발사 방향이 좌우 40도 내에서 무작위 변화, 공격력 +2 |
| organ_tentacle | 장기 촉수 | BasicProjectile | 주변 적 최대 2명을 1.2초마다 30% 피해로 공격, 기본 공격 피해 20% 감소 |
| rampage_bloodflow | 폭주 혈류 | BasicProjectile | 이동 8초마다 공격력 +1 (최대 +2), 정지 시 4초마다 감소 |
| muscle_spasm | 근육 경련 | BasicProjectile | 근접 공격 범위 1.7배, 공격속도 20% 감소 |
| unstable_core | 불안정 핵 | BasicProjectile | 5초마다 기본 공격 피해가 60~160% 사이에서 변경 |
| bio_resonance | 생체 공명 | BasicProjectile | 3초 내 같은 적 연속 공격 시 피해 +15% (최대 +45%) |
| blood_pressure_burst | 혈압 폭발 | BasicProjectile | 잃은 체력 10%당 공격속도 +6% (최대 +60%) |
| void_cell | 공허 세포 | BasicProjectile | 공격 시 20% 확률로 적 주변에서 85% 피해의 추가 탄환 생성 |
| forbidden_growth | 금단 성장 | BasicProjectile | 체력 -3, 공격력 +2 증가 |
| overclock_nerve | 과속 신경 | BasicProjectile | 체력 -2, 이동속도 +2 증가 |
| blood_contract | 피의 계약 | BasicProjectile | 적 20명 처치 시 체력 +1 (최대 +5), 받는 피해 1.3배 |
| hyperplasia_heart | 과증식 심장 | BasicProjectile | 잃은 체력에 비례해 공격력 증가 (최대 +2) |
| decay_organ | 부패 장기 | BasicProjectile | 3분마다 공격력 +1 (최대 +2) |
| rupture_muscle | 파열 근육 | BasicProjectile | 공격 2회마다 공격력 +0.5·이동속도 -0.25 (최대 3중첩) |
| imperfect_regeneration | 불완전 재생 | BasicProjectile | 체력 -2, 피격 2초 후 체력 +1 회복 (10초 쿨타임) |
| severance_reflex | 절단 반사 | BasicProjectile | 피격 후 2초간 공격력 +2 |
| bio_gamble | 생체 도박 | BasicProjectile | 레벨업 선택 시 성장량이 0~+2 범위에서 무작위 결정 |
| exoskeleton | 외골격 | BasicProjectile | 받는 피해 25% 감소, 대신 이동속도 -1.5 |
| platelet_membrane | 혈소판 막 | BasicProjectile | 20초마다 피해 1을 막는 보호막 생성 |
| recovery_factor | 회복 인자 | BasicProjectile | 30초마다 체력 +1 회복 |
| reflective_skin | 반사 피부 | BasicProjectile | 피격 시 실제 받은 피해의 75% 반사 |
| bio_barrier | 생체 장막 | BasicProjectile | 정지 1.5초마다 받는 피해 10% 감소 (최대 40%) |
| split_regeneration | 분열 재생 | BasicProjectile | 체력 0이 될 때 작은 체력으로 부활 |
| infected_host | 감염 숙주 | BasicProjectile | 잡몹 처치 시 15% 확률로 6초간 35% 피해의 아군 생성 (최대 2명) |
| spore_colony | 포자 군집 | BasicProjectile | 일정 시간마다 적에게 달려드는 작은 포자 생명체 소환 |
| blood_drone | 혈액 드론 | BasicProjectile | 1초마다 가까운 적에게 공격력의 30% 피해 |
| guardian_organ | 수호 장기 | BasicProjectile | 플레이어 주변을 맴돌며 3.5초마다 적 투사체 하나 방어 |
| tentacle_colony | 촉수 군체 | BasicProjectile | 3초마다 적 2명에게 1.2초간 60% 둔화와 10% 피해 |
| electric_neural_network | 전기 신경망 | BasicProjectile | 적 처치 시 최대 3명에게 피해 1의 연쇄 전류 (5초 쿨타임) |
| infection_transference | 감염 전이 | BasicProjectile | 적 처치 시 주변 3명에게 3초간 초당 0.5 감염 피해 (5초 쿨타임) |
| macrophage | 대식 세포 | BasicProjectile | 적 처치 시 5초간 공격력 +0.5 (최대 3중첩) |
| gluttonous_organ | 폭식 장기 | BasicProjectile | 적 처치 시 3초 동안 이동속도 증가 (최대 4스택) |
| heart_sniper | 심장 저격 | BasicProjectile | 체력 60% 이상인 적에게 직접 공격 피해 35% 증가 |
| bloodflow_acceleration | 혈류 가속 | BasicProjectile | 보스·엘리트 근처에서 공격속도 35% 증가 |
| focused_nerve | 집중 신경 | BasicProjectile | 주변 적 1명 이하일 때 공격력 +0.75, 2~3명일 때 +0.5 |
| execution_instinct | 처형 본능 | BasicProjectile | 체력 20% 미만 적을 20% 확률로 처형 (보스·엘리트는 추가 피해 50%) |
| berserk_cell | 광폭 세포 | BasicProjectile | 보스전 진입 시 15초간 공격력 +0.75, 이동속도 +0.5, 공격속도 +30% |
| unstable_cell | 불안정 세포 | BasicProjectile | 투사체 속도 50~150%, 80% 이하의 느린 탄은 피해 150% |
| grotesque_growth | 기괴 성장 | BasicProjectile | 10초마다 소형화·이동속도 +1 또는 대형화·공격력 +1·이동속도 -0.5 |
| mutation_rampage | 돌연변이 폭주 | BasicProjectile | 15초마다 7초간 무작위 스탯 버프 또는 디버프 |
| parasitic_bomb | 기생 폭탄 | BasicProjectile | 적 처치 시 20% 확률로 반경 4에 공격력 150% 폭발 |
| frenzy_hormone | 광란 호르몬 | BasicProjectile | 피격 시 무작위 능력 +0.5~1 (3초 지속, 8초 쿨타임) |

## 런타임 효과 필드 기본값

```csharp
96: [SerializeField] private float doubleShotSpreadAngle = 7f;
97: [SerializeField] private float tripleShotSpreadAngle = 11f;
98: [SerializeField] private float doubleShotDamageMultiplier = 0.75f;
99: [SerializeField] private float tripleShotDamageMultiplier = 0.55f;
100: [SerializeField] private float backShotDamageMultiplier = 0.9f;
103: [SerializeField] private float homingTurnRate = 7f;
104: [SerializeField] private float homingSearchRadius = 8f;
105: [SerializeField] private float boomerangReturnDistance = 8f;
106: [SerializeField] private float boomerangRepeatHitDamageMultiplier = 0.75f;
107: [SerializeField, Min(1)] private int boomerangMaxHitsPerPass = 3;
108: [SerializeField, Min(1)] private int piercingHitCount = 3;
109: [SerializeField] private float piercingSubsequentHitDamageMultiplier = 0.75f;
110: [SerializeField, Min(1)] private int reflectionBounceCount = 3;
113: [SerializeField] private float hypertrophyScaleMultiplier = 1.45f;
114: [SerializeField] private float hypertrophyRangeMultiplier = 1.25f;
115: [SerializeField] private float pulseGrowEndScale = 2.6f;
116: [SerializeField] private float pulseShrinkEndScale = 0.2f;
119: [SerializeField] private float splitDamageMultiplier = 0.5f;
120: [SerializeField] private float splitRangeMultiplier = 0.65f;
121: [SerializeField] private float splitAngle = 90f;
122: [SerializeField] private float explosionRadius = 2f;
123: [SerializeField] private float explosionDamageMultiplier = 1f;
124: [SerializeField] private float explosiveBloodCellRangedDamageMultiplier = 0.5f;
125: [SerializeField] private float acidDuration = 4f;
126: [SerializeField] private float acidTickInterval = 1f;
127: [SerializeField] private float acidTickDamage = 0.5f;
128: [SerializeField] private float poisonDuration = 4f;
129: [SerializeField] private float poisonTickInterval = 1f;
130: [SerializeField] private float poisonTickDamageRatio = 0.2f;
131: [SerializeField] private float freezeSlowRatio = 0.3f;
132: [SerializeField] private float freezeDuration = 2f;
133: [SerializeField] private float bleedDuration = 3.5f;
134: [SerializeField] private float bleedTickInterval = 0.8f;
135: [SerializeField] private float bleedTickDamageRatio = 0.18f;
138: [SerializeField, Range(0f, 1f)] private float cellProliferationChance = 0.25f;
139: [SerializeField] private float cellProliferationDamageMultiplier = 0.9f;
142: [SerializeField] private float beamRadius = 0.8f;
143: [SerializeField] private float beamDamageMultiplier = 0.8f;
144: [SerializeField, Min(1)] private int beamHitBufferSize = 48;
147: [SerializeField] private int overheatMaxStacks = 10;
148: [SerializeField] private float overheatStackWindow = 1.4f;
149: [SerializeField] private float overheatPerStackAttackSpeedBonus = 0.1f;
150: [SerializeField] private float overheatDecayInterval = 5f;
151: [SerializeField] private float overheatExplosionRadius = 2.5f;
152: [SerializeField] private float overheatExplosionDamageMultiplier = 1.5f;
153: [SerializeField] private float overheatExplosionSelfDamage = 1f;
154: [SerializeField] private float mutantEyeAccuracyPenaltyAngle = 40f;
155: [SerializeField] private float mutantEyeFlatDamageBonus = 2f;
156: [SerializeField] private float organTentacleAutoAttackInterval = 1.2f;
157: [SerializeField] private float organTentacleAutoAttackRadius = 4.5f;
158: [SerializeField] private float organTentacleAutoAttackDamageMultiplier = 0.3f;
159: [SerializeField] private int organTentacleMaxTargets = 2;
160: [SerializeField] private float organTentacleBaseAttackPenaltyMultiplier = 0.8f;
161: [SerializeField] private float rampageMoveSecondsPerBonus = 8f;
162: [SerializeField] private int rampageMaxAttackBonus = 2;
163: [SerializeField] private float rampageIdleSecondsPerDecay = 4f;
164: [SerializeField] private float muscleSpasmMeleeRangeMultiplier = 1.7f;
165: [SerializeField] private float muscleSpasmAttackSpeedMultiplier = 0.8f;
166: [SerializeField] private float unstableCoreMinAttackRatio = 0.6f;
167: [SerializeField] private float unstableCoreMaxAttackRatio = 1.6f;
168: [SerializeField] private float unstableCoreRerollInterval = 5f;
169: [SerializeField] private float bioResonanceStackDamageBonus = 0.15f;
170: [SerializeField] private int bioResonanceMaxStacks = 3;
171: [SerializeField] private float bioResonanceStackWindow = 3f;
172: [SerializeField] private float bloodPressureBaseAttackSpeedAdd;
173: [SerializeField] private float bloodPressurePerTenPercentMissingAdd = 0.06f;
174: [SerializeField] private float voidCellChance = 0.2f;
175: [SerializeField] private float voidCellDamageMultiplier = 0.85f;
176: [SerializeField] private float voidCellSpawnRadius = 2.2f;
177: [SerializeField] private float forbiddenGrowthMaxHealthPenalty = 3f;
178: [SerializeField] private float forbiddenGrowthFlatAttackBonus = 2f;
179: [SerializeField] private float overclockNerveMaxHealthPenalty = 2f;
180: [SerializeField] private float overclockNerveFlatMoveSpeedBonus = 2f;
181: [SerializeField] private float bloodContractIncomingDamageMultiplier = 1.3f;
182: [SerializeField, Min(1)] private int bloodContractKillsPerHeal = 20;
183: [SerializeField] private float bloodContractHealthGainAmount = 1f;
184: [SerializeField, Min(1)] private int bloodContractMaxHealthGainCount = 5;
185: [SerializeField] private float hyperplasiaMissingHealthAttackBonusMax = 2f;
186: [SerializeField] private float decayOrganSecondsPerAttackBonus = 180f;
187: [SerializeField] private int decayOrganMaxAttackBonus = 2;
188: [SerializeField] private float ruptureMuscleAttackBonusPerStack = 0.5f;
189: [SerializeField] private float ruptureMuscleMovePenaltyPerStack = 0.25f;
190: [SerializeField] private int ruptureMuscleMaxStacks = 3;
191: [SerializeField] private float ruptureMuscleDecayDelay = 3f;
192: [SerializeField] private float imperfectRegenMaxHealthPenalty = 2f;
193: [SerializeField] private float imperfectRegenDelay = 2f;
194: [SerializeField] private float imperfectRegenHealPerTrigger = 1f;
195: [SerializeField] private float imperfectRegenCooldownDuration = 10f;
196: [SerializeField] private float severanceReflexDuration = 2f;
197: [SerializeField] private float severanceReflexFlatAttackBonus = 2f;
198: [SerializeField] private float exoskeletonDamageReductionRatio = 0.25f;
199: [SerializeField] private float exoskeletonMoveSpeedPenalty = 1.5f;
200: [SerializeField] private float plateletMembraneInterval = 20f;
201: [SerializeField] private float plateletMembraneShieldAmount = 1f;
202: [SerializeField] private float recoveryFactorInterval = 30f;
203: [SerializeField] private float recoveryFactorHealAmount = 1f;
204: [SerializeField] private float reflectiveSkinDamageRatio = 0.75f;
205: [SerializeField] private float bioBarrierIdleSecondsPerStep = 1.5f;
206: [SerializeField] private float bioBarrierReductionPerStep = 0.1f;
207: [SerializeField] private float bioBarrierMaxReduction = 0.4f;
208: [SerializeField] private float splitRegenerationReviveHealth = 2f;
211: [SerializeField, Range(0f, 1f)] private float infectedHostChance = 0.15f;
212: [SerializeField] private float infectedHostLifetime = 6f;
213: [SerializeField] private float infectedHostDamageMultiplier = 0.35f;
214: [SerializeField] private float infectedHostAttackInterval = 1f;
215: [SerializeField] private float infectedHostAttackRadius = 1.25f;
216: [SerializeField] private float infectedHostSearchRadius = 7f;
217: [SerializeField, Min(1)] private int infectedHostMaxAllies = 2;
218: [SerializeField] private float sporeSpawnInterval = 8f;
219: [SerializeField] private float sporeLifetime = 6f;
220: [SerializeField] private float sporeDamageMultiplier = 1f;
221: [SerializeField] private float sporeSearchRadius = 8f;
222: [SerializeField] private float sporeBurstRadius = 1.1f;
223: [SerializeField] private float bloodDroneOrbitRadius = 1.4f;
224: [SerializeField] private float bloodDroneFireInterval = 1f;
225: [SerializeField] private float bloodDroneDamageMultiplier = 0.3f;
226: [SerializeField] private float bloodDroneTargetRadius = 9f;
227: [SerializeField] private float guardianOrganOrbitRadius = 1.2f;
228: [SerializeField] private float guardianOrganBlockRadius = 0.9f;
229: [SerializeField] private float guardianOrganCooldown = 3.5f;
230: [SerializeField] private float tentacleBindInterval = 3f;
231: [SerializeField] private float tentacleBindRadius = 5.5f;
232: [SerializeField] private float tentacleBindDuration = 1.2f;
233: [SerializeField] private float tentacleSlowRatio = 0.6f;
234: [SerializeField] private float tentacleDamageMultiplier = 0.1f;
235: [SerializeField, Min(1)] private int tentacleMaxTargets = 2;
238: [SerializeField] private float electricChainDamage = 1f;
239: [SerializeField] private float electricChainRadius = 6f;
240: [SerializeField, Min(1)] private int electricChainMaxHits = 3;
241: [SerializeField] private float electricChainCooldown = 5f;
242: [SerializeField] private float infectionTransferRadius = 4.5f;
243: [SerializeField, Min(1)] private int infectionTransferMaxTargets = 3;
244: [SerializeField] private float infectionTransferDuration = 3f;
245: [SerializeField] private float infectionTransferTickInterval = 1f;
246: [SerializeField] private float infectionTransferTickDamage = 0.5f;
247: [SerializeField] private float infectionTransferCooldown = 5f;
248: [SerializeField] private float macrophageDuration = 5f;
249: [SerializeField] private float macrophageAttackBonusPerStack = 0.5f;
250: [SerializeField, Min(1)] private int macrophageMaxStacks = 3;
251: [SerializeField] private float gluttonousOrganDuration = 3f;
252: [SerializeField] private float gluttonousOrganMoveBonusPerStack = 0.5f;
253: [SerializeField, Min(1)] private int gluttonousOrganMaxStacks = 4;
256: [SerializeField] private float heartSniperHealthThreshold = 0.6f;
257: [SerializeField] private float heartSniperDamageBonusRatio = 0.35f;
258: [SerializeField] private float bloodflowAccelerationRadius = 9f;
259: [SerializeField] private float bloodflowAccelerationAttackSpeedBonus = 0.35f;
260: [SerializeField] private float focusedNerveRadius = 6f;
261: [SerializeField] private float focusedNerveHighAttackBonus = 0.75f;
262: [SerializeField] private float focusedNerveLowAttackBonus = 0.5f;
263: [SerializeField] private float executionInstinctHealthThreshold = 0.2f;
264: [SerializeField, Range(0f, 1f)] private float executionInstinctChance = 0.2f;
265: [SerializeField] private float executionInstinctBossDamageMultiplier = 1.5f;
266: [SerializeField] private float berserkCellDuration = 15f;
267: [SerializeField] private float berserkCellAttackBonus = 0.75f;
268: [SerializeField] private float berserkCellMoveBonus = 0.5f;
269: [SerializeField] private float berserkCellAttackSpeedBonus = 0.3f;
270: [SerializeField] private float berserkCellBossDetectionRadius = 18f;
273: [SerializeField] private float unstableCellMinProjectileSpeedMultiplier = 0.5f;
274: [SerializeField] private float unstableCellMaxProjectileSpeedMultiplier = 1.5f;
275: [SerializeField] private float unstableCellSlowThreshold = 0.8f;
276: [SerializeField] private float unstableCellSlowDamageMultiplier = 1.5f;
277: [SerializeField] private float grotesqueGrowthInterval = 10f;
278: [SerializeField] private float grotesqueGrowthSmallScale = 0.75f;
279: [SerializeField] private float grotesqueGrowthLargeScale = 1.35f;
280: [SerializeField] private float grotesqueGrowthSmallMoveBonus = 1f;
281: [SerializeField] private float grotesqueGrowthLargeAttackBonus = 1f;
282: [SerializeField] private float grotesqueGrowthLargeMovePenalty = 0.5f;
283: [SerializeField] private float mutationRampageInterval = 15f;
284: [SerializeField] private float mutationRampageDuration = 7f;
285: [SerializeField, Range(0f, 1f)] private float mutationRampageBuffChance = 0.6f;
286: [SerializeField] private float mutationRampageAttackBuff = 1f;
287: [SerializeField] private float mutationRampageMoveBuff = 1f;
288: [SerializeField] private float mutationRampageAttackSpeedBuff = 0.5f;
289: [SerializeField] private float mutationRampageAttackDebuff = 0.5f;
290: [SerializeField] private float mutationRampageMoveDebuff = 0.75f;
291: [SerializeField] private float mutationRampageAttackSpeedDebuff = 0.25f;
292: [SerializeField, Range(0f, 1f)] private float parasiticBombChance = 0.2f;
293: [SerializeField] private float parasiticBombRadius = 4f;
294: [SerializeField] private float parasiticBombDamageMultiplier = 1.5f;
295: [SerializeField] private float frenzyHormoneDuration = 3f;
296: [SerializeField] private float frenzyHormoneCooldown = 8f;
297: [SerializeField] private float frenzyHormoneMinBonus = 0.5f;
298: [SerializeField] private float frenzyHormoneMaxBonus = 1f;
```
