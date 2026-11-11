# 원본 수치 스냅샷 — 2026-10-25

조사 시점 증거 자료. 줄 번호는 당시 원본 기준. 숫자 포함 코드 줄은 계산식과 시각 설정도 포함한다. 기본값과 직렬화 값은 서로 다른 층이며 실행 여부를 보장하지 않는다.

## Assets/_Project/Data/BiomeConfigs/IntestineBiomeConfig.asset

```yaml
4: MonoBehaviour:
15:   biomeType: 1
16:   regionCellSize: 20
17:   regionBlendWidth: 3
18:   detailNoiseScale: 0.05
19:   regions:
20:   - name: Mud
21:     baseHeight: 0
22:     primaryTile: {fileID: 11400000, guid: 11d2f3d12749246c9991e3332a75a100, type: 2}
23:     primaryType: 1
24:     variantTile: {fileID: 11400000, guid: bac4af93bbb0c493bb1bf946190c22b5, type: 2}
25:     variantType: 2
26:     variantThreshold: 0.85
27:     tileVariants: []
28:   - name: Moss
29:     baseHeight: 0
30:     primaryTile: {fileID: 11400000, guid: 2514b191dff8f43de9fcf78ec68d0a73, type: 2}
31:     primaryType: 1
32:     variantTile: {fileID: 0}
33:     variantType: 0
34:     variantThreshold: 0.2
35:     tileVariants: []
36:   - name: Flesh
37:     baseHeight: 0
38:     primaryTile: {fileID: 11400000, guid: b224b8153a5d44787a8fe7731276bfa1, type: 2}
39:     primaryType: 1
40:     variantTile: {fileID: 11400000, guid: 5472695917296437d9a3c856571b9d6a, type: 2}
41:     variantType: 2
42:     variantThreshold: 0.85
43:     tileVariants: []
44:   heightNoiseScale: 0.015
45:   heightNoiseAmplitude: 0
46:   tileMappings:
47:   - tileType: 1
48:     tile: {fileID: 11400000, guid: 11d2f3d12749246c9991e3332a75a100, type: 2}
49:   exteriorBackdrop:
50:     enabled: 1
51:     sprite: {fileID: 21300000, guid: 7d499847c78e406eb61127fc1ddb614c, type: 3}
52:     tint: {r: 0.82, g: 0.76, b: 0.72, a: 1}
53:     opacity: 0.86
54:     cameraClearColor: {r: 0.035, g: 0.015, b: 0.018, a: 1}
55:     cameraDistance: 50
56:     overscan: 1.22
57:     driftAmount: 0.012
58:     driftSpeed: 0.07
59:     sortingOrder: -32000
60:   marginLeft: 5
61:   marginRight: 5
62:   marginBottom: 10
63:   marginTop: 5
64:   objectRules:
65:   - name: MoldTree
66:     poolKind: 3
67:     density: 0.015
68:     minDistance: 4
69:     poissonSalt: 101
70:     allowedRegions: 01000000
71:     blocksMovement: 1
72:     heightOffset: 0
73:     sortingOrder: 0
74:     sprites:
76:     useDeterministicSprite: 1
77:     spriteSalt: 0
78:     animate: 0
79:     animationSpeed: 0.15
80:     useBillboard: 1
81:     useYSort: 1
82:     addCollider: 1
83:     isTrigger: 0
84:     colliderSize: {x: 1.5, y: 3.15192, z: 1.5}
85:     colliderCenter: {x: 0, y: -0.01219428, z: -0.0000001194857}
86:   - name: Rock
87:     poolKind: 3
88:     density: 0.02
89:     minDistance: 3
90:     poissonSalt: 102
91:     allowedRegions: 00000000
92:     blocksMovement: 1
93:     heightOffset: 0
94:     sortingOrder: 0
95:     sprites:
97:     useDeterministicSprite: 1
98:     spriteSalt: 0
99:     animate: 0
100:     animationSpeed: 0.15
101:     useBillboard: 1
102:     useYSort: 1
103:     addCollider: 1
104:     isTrigger: 0
105:     colliderSize: {x: 2.751141, y: 1.319045, z: 1.5}
106:     colliderCenter: {x: -0.05412865, y: -0.3340048, z: -0.0000000561392}
107:   - name: Parasite
108:     poolKind: 4
109:     density: 0.01
110:     minDistance: 3
111:     poissonSalt: 103
112:     allowedRegions: 02000000
113:     blocksMovement: 0
114:     heightOffset: 0
115:     sortingOrder: 100
116:     sprites:
123:     useDeterministicSprite: 1
124:     spriteSalt: 0
125:     animate: 1
126:     animationSpeed: 0.15
127:     useBillboard: 1
128:     useYSort: 0
129:     addCollider: 0
130:     isTrigger: 0
131:     colliderSize: {x: 1, y: 1, z: 1}
132:     colliderCenter: {x: 0, y: 0, z: 0}
133:   - name: SlimePuddle
134:     poolKind: 1
135:     density: 0.03
136:     minDistance: 2
137:     poissonSalt: 104
138:     allowedRegions: 0000000002000000
139:     blocksMovement: 0
140:     heightOffset: 0.01
141:     sortingOrder: 100
142:     sprites:
145:     useDeterministicSprite: 1
146:     spriteSalt: 2001
147:     animate: 0
148:     animationSpeed: 0.15
149:     useBillboard: 1
150:     useYSort: 0
151:     addCollider: 0
152:     isTrigger: 0
153:     colliderSize: {x: 1, y: 1, z: 1}
154:     colliderCenter: {x: 0, y: 0, z: 0}
155:   - name: MoldPlant
156:     poolKind: 2
157:     density: 0.05
158:     minDistance: 2
159:     poissonSalt: 105
160:     allowedRegions: 01000000
161:     blocksMovement: 0
162:     heightOffset: 0
163:     sortingOrder: 100
164:     sprites:
166:     useDeterministicSprite: 1
167:     spriteSalt: 0
168:     animate: 0
169:     animationSpeed: 0.15
170:     useBillboard: 1
171:     useYSort: 0
172:     addCollider: 0
173:     isTrigger: 0
174:     colliderSize: {x: 1, y: 1, z: 1}
175:     colliderCenter: {x: 0, y: 0, z: 0}
176:   - name: Item
177:     poolKind: 5
178:     density: 0.01
179:     minDistance: 2
180:     poissonSalt: 106
181:     allowedRegions: 
182:     blocksMovement: 0
183:     heightOffset: 0
184:     sortingOrder: 100
185:     sprites: []
186:     useDeterministicSprite: 1
187:     spriteSalt: 3001
188:     animate: 0
189:     animationSpeed: 0.15
190:     useBillboard: 1
191:     useYSort: 0
192:     addCollider: 1
193:     isTrigger: 1
194:     colliderSize: {x: 1, y: 1, z: 1}
195:     colliderCenter: {x: 0, y: 0, z: 0}
196:   enemySpawnConfig: {fileID: 11400000, guid: 7ff2a46806514b7c92c4fff81629a680, type: 2}
197:   biomeEliteSpawnConfig: {fileID: 11400000, guid: bcf5eb7ed200346ef9e80766a99d434b, type: 2}
198:   enemySpawnRules: []
199:   bossArenaConfig: {fileID: 11400000, guid: 93487fc5db6a4a0196211bf55e23a032, type: 2}
200:   midBossArena:
201:     enabled: 1
202:     onlyEnableOnLargeMaps: 1
203:     minimumMapWidth: 300
204:     minimumMapHeight: 300
205:     useCustomCenter: 0
206:     centerGrid: {x: 150, y: 150}
207:     arenaSize: {x: 32, y: 32}
208:     wallThicknessInCells: 1
209:     lockBoundaryInsetInCells: 1
210:     triggerInsetInCells: 2
211:     wallHeightOffset: 1.5
212:     triggerHeight: 4
213:     sortingOrder: 3500
214:     hideBossUntilEncounter: 1
215:     presentation:
216:       enabled: 1
217:       entranceSide: 0
218:       entranceWidthInCells: 4
219:       approachLengthInCells: 9
220:       pulseSpeed: 1.4
221:       approachOpacity: 0.72
222:       entranceSprite: {fileID: 0}
223:       entranceVisualScale: 1.35
224:       wallStraightSprites: []
225:       wallCornerSprite: {fileID: 0}
226:       wallCornerSpanInCells: 2
227:       wallCornerBaseYaw: 270
228:       wallUseStraightCornerConnectors: 0
229:       floorSortingOrder: 40
230:       gateSortingOrder: 3490
231:       useBiomePalette: 1
232:       primaryColor: {r: 0.43, g: 0.72, b: 0.22, a: 1}
233:       accentColor: {r: 0.82, g: 0.48, b: 0.16, a: 1}
234:       lockedColor: {r: 0.92, g: 0.12, b: 0.08, a: 1}
235:       clearedColor: {r: 0.34, g: 0.32, b: 0.3, a: 1}
236:     returnPortalSprite: {fileID: 0}
237:     returnPortalScale: {x: 1, y: 1, z: 1}
238:     boss:
239:       displayName: MidBoss
240:       useCustomBossRule: 0
241:       bossRule:
242:         monsterDefinition: {fileID: 0}
243:         name: Enemy
244:         density: 0.0025
245:         minDistance: 8
246:         poissonSalt: 400
247:         allowedRegions: 
248:         maxAlive: 1
249:         activationRadius: 20
250:         respawnCooldown: 8
251:         spawnRadius: 1.5
252:         moveSpeed: 1.5
253:         stoppingDistance: 0.1
254:         wanderRadius: 4
255:         chaseRadius: 6
256:         leashRadius: 8
257:         idleDelayRange: {x: 0.5, y: 1.5}
258:         maxHealth: 30
259:         attackDamage: 1
260:         attackRange: 1.5
261:         attackCooldown: 1
262:         expReward: 10
263:         enableContactDamage: 1
264:         contactDamage: 1
265:         contactKnockbackDistance: 0.45
266:         additionalBaseStats: []
267:         separationDistance: 1.1
268:         separationStrength: 1
269:         heightOffset: 0
270:         scale: {x: 1, y: 1, z: 1}
271:         sortingOrder: 1000
272:         useBillboard: 1
273:         useYSort: 1
274:         animationSpeed: 0.15
275:         addCollider: 1
276:         isTrigger: 0
277:         colliderSize: {x: 0.7, y: 1.1, z: 0.7}
278:         colliderCenter: {x: 0, y: 0.55, z: 0}
279:         idleSprites: []
280:         idleSpritesUp: []
281:         idleSpritesDown: []
282:         moveSprites: []
283:         moveSpritesUp: []
284:         moveSpritesDown: []
285:         attackSprites: []
286:         attackSpritesUp: []
287:         attackSpritesDown: []
288:         attackAnimationSpeed: 0.12
289:         isRanged: 0
290:         projectileSpeed: 8
291:         projectileLifeTime: 3
292:         projectileSprite: {fileID: 0}
293:         projectileScale: {x: 0.4, y: 0.4, z: 0.4}
294:         projectileSpawnOffset: 0.5
295:         expandColliderOnAttack: 0
296:         attackColliderSize: {x: 2, y: 2, z: 2}
297:         attackColliderCenter: {x: 0, y: 0.55, z: 0}
298:         deathSprites: []
299:         deathAnimationSpeed: 0.15
300:         isElite: 0
301:         tintColor: {r: 1, g: 1, b: 1, a: 1}
302:         killTriggerEnemyName: 
303:         killTriggerCount: 10
304:         splitsOnDeath: 0
305:         splitCount: 2
306:         splitEnemyName: 
307:         splitVfxSprites: []
308:         splitVfxScale: 3
309:         splitVfxSpeed: 0.08
310:         splitVfxDuration: 0.6
311:         chargesAtPlayer: 0
312:         chargeSpeed: 6
313:         chargeAccelTime: 0.3
314:         leavesDebrisOnDeath: 0
315:         debrisDuration: 5
316:         debrisAggroRadius: 8
317:         debrisVfxSprites: []
318:         debrisVfxScale: 15
319:         debrisVfxSpeed: 0.12
320:       useEnemyRuleFallback: 1
321:       fallbackEnemyRuleIndex: 0
322:       patternType: 0
323:       overrideStats: 0
324:       maxHealthMultiplier: 1
325:       attackDamageMultiplier: 1
326:       moveSpeedMultiplier: 1
327:       scaleMultiplier: {x: 1, y: 1, z: 1}
328:       minimumMaxHealth: 0
329:       intestinePattern:
330:         phase2HealthRatio: 0.5
331:         phase1FleeDistance: 7
332:         phase1RoamRadius: 8
333:         phase1MoveMultiplier: 1
334:         phase2PreferredDistance: 2.5
335:         phase2MoveMultiplier: 1.15
336:         dungPreDelay: 0.5
337:         phase1DungCooldown: 8
338:         phase2ThrowCooldown: 5
339:         dungCollisionDamage: 1
340:         dungSlowRatio: 0.2
341:         dungSlowDuration: 3
342:         dungHazardRadius: 0.75
343:         dungLifeTime: 10
344:         throwTravelTime: 0.7
345:         throwArcHeight: 2.5
346:         parasiteMinCount: 2
347:         parasiteMaxCount: 3
348:         parasiteSpawnDelay: 0.45
349:         parasiteSpawnRadius: 1.1
350:         parasiteDefinition: {fileID: 0}
351:         parasiteMaxHealth: 6
352:         parasiteMoveSpeed: 2.3
353:         parasiteAttackDamage: 1
354:         parasiteAttackRange: 1.1
355:         parasiteAttackCooldown: 1.3
356:         parasiteScale: {x: 0.55, y: 0.55, z: 0.55}
357:         stompMinDelay: 7
358:         stompPreDelay: 0.55
359:         stompRadius: 6
360:         stompDamage: 2
361:         stompSlowRatio: 0.3
362:         stompSlowDuration: 3
363:         dungColor: {r: 0.34, g: 0.18, b: 0.08, a: 0.95}
364:         parasiteColor: {r: 0.62, g: 0.88, b: 0.36, a: 1}
365:         shockwaveColor: {r: 0.65, g: 0.38, b: 0.18, a: 0.45}
366:         startInPhase2ForDebug: 0
367:         useFastPatternCooldownsForDebug: 0
368:         fastPatternCooldown: 1
369:       liverPattern:
370:         phase2HealthRatio: 0.5
371:         phase1AttackDamage: 2
372:         phase1MoveSpeed: 1
373:         phase2AttackDamage: 2
374:         phase2MoveSpeed: 1.5
375:         bloodBombRange: 7
376:         bloodBombExplosionRadius: 2.5
377:         bloodBombFuseTime: 1
378:         bloodBombCooldown: 4
379:         attackPowerDebuffRatio: 0.15
380:         attackPowerDebuffDuration: 5
381:         bloodBombArcHeight: 1.4
382:         bloodBombProjectileScale: 1.15
383:         bloodBombSprite: {fileID: 0}
384:         healingPosePreDelay: 1
385:         healingPoseCooldown: 15
386:         healingPoseDuration: 5
387:         damageHealRatio: 0.7
388:         healingPoseLowerSprites: []
389:         healingPoseLowerSpritesDown: []
390:         healingPoseRaiseSprites: []
391:         healingPoseRaiseSpritesDown: []
392:         healingPoseAnimationSpeed: 0.12
393:         phase1PreferredDistanceRatio: 0.72
394:         phase2PreferredDistance: 2.6
395:         roamRadius: 9
396:         bloodBombColor: {r: 0.7, g: 0, b: 0.04, a: 0.96}
397:         explosionColor: {r: 0.9, g: 0.05, b: 0.08, a: 0.45}
398:         startInPhase2ForDebug: 0
399:         useFastPatternCooldownsForDebug: 0
400:         fastPatternCooldown: 1
401:       stomachPattern:
402:         phase2HealthRatio: 0.65
403:         phase1AttackDamage: 3
404:         phase1MoveSpeed: 0.5
405:         phase2AttackDamage: 3
406:         phase2MoveSpeed: 1
407:         phase1MeleeRange: 1.6
408:         phase1MeleeCooldown: 1.7
409:         chargeSpeed: 4
410:         chargeCooldown: 5
411:         chargeDamage: 4
412:         chargeKnockbackDistance: 2
413:         chargeWindup: 0.35
414:         chargeDuration: 1.2
415:         chargeHitRadius: 1.25
416:         phase2PatternInterval: 3
417:         phase2PreferredDistance: 4
418:         roamRadius: 9
419:         acidRange: 9
420:         acidDamage: 2
421:         acidDebuffDuration: 5
422:         acidTickInterval: 1
423:         acidTickDamage: 1
424:         acidProjectileTravelTime: 0.55
425:         acidArcHeight: 0.8
426:         acidProjectileScale: 0.9
427:         acidHitRadius: 1.15
428:         suctionRange: 4.2
429:         suctionDuration: 0.4
430:         suctionStopDistance: 1
431:         spitDelay: 0.35
432:         spitReturnDuration: 0.18
433:         shortRangeDamage: 5
434:         shortRangeRadius: 2.2
435:         suctionAttackSprites: []
436:         suctionAttackAnimationSpeed: 0.08
437:         acidColor: {r: 0.55, g: 0.95, b: 0.15, a: 0.95}
438:         acidSplashColor: {r: 0.5, g: 0.9, b: 0.12, a: 0.42}
439:         suctionColor: {r: 0.75, g: 0.95, b: 0.45, a: 0.42}
440:         leakColor: {r: 0.38, g: 0.85, b: 0.18, a: 0.38}
441:         startInPhase2ForDebug: 0
442:         useFastPatternCooldownsForDebug: 0
443:         fastPatternCooldown: 1
444:       lungPattern:
445:         phase1BrotherHealth: 40
446:         phase1AttackDamage: 2
447:         phase1MoveSpeed: 1
448:         windGustMoveSpeed: 3
449:         windGustSpeedMultiplier: 3
450:         windGustInterval: 5
451:         windGustDuration: 1.25
452:         brotherSpacing: 3.2
453:         phase1GasRange: 7
454:         phase1GasCooldown: 3.5
455:         phase1GasKnockback: 1
456:         phase2MaxHealth: 100
457:         phase2AttackDamage: 4
458:         phase2MoveSpeed: 1.5
459:         phase2GasRange: 10
460:         phase2GasCooldown: 4
461:         phase2GasKnockback: 1.2
462:         diveCooldown: 8
463:         diveWindup: 0.55
464:         diveTravelTime: 0.65
465:         diveArcHeight: 4
466:         diveLandingRadius: 1.45
467:         diveLandingDamage: 3
468:         diveKnockback: 1.4
469:         gasProjectileSpeed: 8
470:         gasProjectileRadius: 0.7
471:         gasProjectileScale: 1
472:         gasWindup: 0.25
473:         gasProjectileHeightOffset: 2
474:         phase1ContactDamage: 1
475:         phase2ContactDamage: 2
476:         contactRadius: 1.05
477:         contactCooldown: 1.1
478:         phase1PreferredDistance: 5.2
479:         phase2PreferredDistance: 4.2
480:         roamRadius: 10
481:         gasColor: {r: 0.72, g: 0.95, b: 0.9, a: 0.72}
482:         windColor: {r: 0.78, g: 0.9, b: 1, a: 0.34}
483:         diveColor: {r: 0.5, g: 0.85, b: 1, a: 0.42}
484:         startInPhase2ForDebug: 0
485:         useFastPatternCooldownsForDebug: 0
486:         fastPatternCooldown: 1
487:   returnPortal:
488:     enabled: 1
489:     name: ReturnPortal
490:     poolKind: 100
491:     sprite: {fileID: 1428116226, guid: fbc2f259a8b301f42933cd4689e5842c, type: 3}
492:     sortingOrder: 1000
493:     useCustomPosition: 1
494:     gridPosition: {x: 150, y: 4}
495:     heightOffset: 0
496:     useBillboard: 1
497:     scale: {x: 0.5, y: 0.5, z: 0.5}
498:     addCollider: 1
499:     isTrigger: 1
500:     colliderSize: {x: 2, y: 2, z: 2}
501:     colliderCenter: {x: 0, y: 0, z: 0}
```

## Assets/_Project/Data/BiomeConfigs/IntestineBossArenaConfig.asset

```yaml
4: MonoBehaviour:
15:   midBossArena:
16:     enabled: 1
17:     onlyEnableOnLargeMaps: 1
18:     minimumMapWidth: 300
19:     minimumMapHeight: 300
20:     useCustomCenter: 0
21:     centerGrid: {x: 150, y: 150}
22:     arenaSize: {x: 32, y: 32}
23:     wallThicknessInCells: 1
24:     lockBoundaryInsetInCells: 0
25:     triggerInsetInCells: 2
26:     wallHeightOffset: 1.5
27:     triggerHeight: 4
28:     sortingOrder: 3500
29:     hideBossUntilEncounter: 1
30:     presentation:
31:       enabled: 1
32:       entranceSide: 0
33:       entranceWidthInCells: 4
34:       approachLengthInCells: 9
35:       pulseSpeed: 1.2
36:       approachOpacity: 0.72
37:       entranceSprite: {fileID: 21300000, guid: 70c47e9c213e44eb6b6e8b700d81b307, type: 3}
38:       entranceVisualScale: 1.45
39:       wallStraightSprites:
43:       wallCornerSprite: {fileID: 1401090, guid: ab6963773a8c74c118fcbf6c0fd72359, type: 3}
44:       wallCornerSpanInCells: 2
45:       wallCornerBaseYaw: 270
46:       wallUseStraightCornerConnectors: 1
47:       floorSortingOrder: 40
48:       gateSortingOrder: 3490
49:       useBiomePalette: 1
50:       primaryColor: {r: 0.43, g: 0.72, b: 0.22, a: 1}
51:       accentColor: {r: 0.82, g: 0.48, b: 0.16, a: 1}
52:       lockedColor: {r: 0.92, g: 0.12, b: 0.08, a: 1}
53:       clearedColor: {r: 0.34, g: 0.32, b: 0.3, a: 1}
54:     returnPortalSprite: {fileID: 0}
55:     returnPortalScale: {x: 1, y: 1, z: 1}
56:     boss:
57:       displayName: IntestineBoss
58:       useCustomBossRule: 1
59:       bossRule:
60:         monsterDefinition: {fileID: 11400000, guid: 5bf40da1cb8f84b5aa7f9c28458981ba, type: 2}
61:         name: IntestineBoss
62:         density: 0
63:         minDistance: 8
64:         poissonSalt: 400
65:         allowedRegions: 
66:         maxAlive: 1
67:         activationRadius: 0
68:         respawnCooldown: 0
69:         spawnRadius: 0
70:         moveSpeed: 1.5
71:         stoppingDistance: 0.1
72:         wanderRadius: 4
73:         chaseRadius: 6
74:         leashRadius: 8
75:         idleDelayRange: {x: 0.5, y: 1.5}
76:         maxHealth: 10
77:         attackDamage: 2
78:         attackRange: 1.5
79:         attackCooldown: 1
80:         expReward: 0
81:         enableContactDamage: 1
82:         contactDamage: 1
83:         contactKnockbackDistance: 0.45
84:         additionalBaseStats: []
85:         separationDistance: 0
86:         separationStrength: 0
87:         heightOffset: 0
88:         scale: {x: 1, y: 1, z: 1}
89:         sortingOrder: 1000
90:         useBillboard: 1
91:         useYSort: 1
92:         animationSpeed: 0.15
93:         addCollider: 1
94:         isTrigger: 0
95:         colliderSize: {x: 0.7, y: 1.1, z: 0.7}
96:         colliderCenter: {x: 0, y: 0.55, z: 0}
97:         idleSprites:
112:         idleSpritesUp: []
113:         idleSpritesDown: []
114:         moveSprites:
121:         moveSpritesUp: []
122:         moveSpritesDown: []
123:         attackSprites:
132:         attackSpritesUp: []
133:         attackSpritesDown: []
134:         attackAnimationSpeed: 0.08
135:         isRanged: 0
136:         projectileSpeed: 8
137:         projectileLifeTime: 3
138:         projectileSprite: {fileID: 0}
139:         projectileScale: {x: 0.4, y: 0.4, z: 0.4}
140:         projectileSpawnOffset: 0.5
141:         expandColliderOnAttack: 0
142:         attackColliderSize: {x: 2, y: 2, z: 2}
143:         attackColliderCenter: {x: 0, y: 0.55, z: 0}
144:         deathSprites:
152:         deathAnimationSpeed: 0.15
153:         isElite: 0
154:         tintColor: {r: 1, g: 1, b: 1, a: 1}
155:         killTriggerEnemyName: 
156:         killTriggerCount: 10
157:         splitsOnDeath: 0
158:         splitCount: 2
159:         splitEnemyName: 
160:         splitVfxSprites: []
161:         splitVfxScale: 3
162:         splitVfxSpeed: 0.08
163:         splitVfxDuration: 0.6
164:         chargesAtPlayer: 0
165:         chargeSpeed: 6
166:         chargeAccelTime: 0.3
167:         leavesDebrisOnDeath: 0
168:         debrisDuration: 5
169:         debrisAggroRadius: 8
170:         debrisVfxSprites: []
171:         debrisVfxScale: 15
172:         debrisVfxSpeed: 0.12
173:       useEnemyRuleFallback: 1
174:       fallbackEnemyRuleIndex: 0
175:       patternType: 2
176:       overrideStats: 0
177:       maxHealthMultiplier: 1
178:       attackDamageMultiplier: 1
179:       moveSpeedMultiplier: 1
180:       scaleMultiplier: {x: 4, y: 4, z: 4}
181:       minimumMaxHealth: 0
182:       intestinePattern:
183:         phase2HealthRatio: 0.5
184:         phase1FleeDistance: 7
185:         phase1RoamRadius: 8
186:         phase1MoveMultiplier: 1
187:         phase2PreferredDistance: 2.5
188:         phase2MoveMultiplier: 1.15
189:         dungPreDelay: 0.5
190:         phase1DungCooldown: 8
191:         phase2ThrowCooldown: 5
192:         dungCollisionDamage: 1
193:         dungSlowRatio: 0.2
194:         dungSlowDuration: 3
195:         dungHazardRadius: 0.75
196:         dungLifeTime: 10
197:         throwTravelTime: 0.7
198:         throwArcHeight: 2.5
199:         parasiteMinCount: 2
200:         parasiteMaxCount: 3
201:         parasiteSpawnDelay: 0.45
202:         parasiteSpawnRadius: 1.1
203:         parasiteDefinition: {fileID: 11400000, guid: 37557a1dd447346a5abed8830605e96c, type: 2}
204:         parasiteMaxHealth: 6
205:         parasiteMoveSpeed: 2.3
206:         parasiteAttackDamage: 1
207:         parasiteAttackRange: 1.1
208:         parasiteAttackCooldown: 1.3
209:         parasiteScale: {x: 0.55, y: 0.55, z: 0.55}
210:         stompMinDelay: 7
211:         stompPreDelay: 0.55
212:         stompRadius: 6
213:         stompDamage: 2
214:         stompSlowRatio: 0.3
215:         stompSlowDuration: 3
216:         dungColor: {r: 0.34, g: 0.18, b: 0.08, a: 0.95}
217:         parasiteColor: {r: 0.62, g: 0.88, b: 0.36, a: 1}
218:         shockwaveColor: {r: 0.65, g: 0.38, b: 0.18, a: 0.45}
219:         startInPhase2ForDebug: 0
220:         useFastPatternCooldownsForDebug: 0
221:         fastPatternCooldown: 1
222:       liverPattern:
223:         phase2HealthRatio: 0.5
224:         phase1AttackDamage: 2
225:         phase1MoveSpeed: 1
226:         phase2AttackDamage: 2
227:         phase2MoveSpeed: 1.5
228:         bloodBombRange: 7
229:         bloodBombExplosionRadius: 2.5
230:         bloodBombFuseTime: 1
231:         bloodBombCooldown: 4
232:         attackPowerDebuffRatio: 0.15
233:         attackPowerDebuffDuration: 5
234:         bloodBombArcHeight: 1.4
235:         bloodBombProjectileScale: 1.15
236:         bloodBombSprite: {fileID: 0}
237:         healingPosePreDelay: 1
238:         healingPoseCooldown: 15
239:         healingPoseDuration: 5
240:         damageHealRatio: 0.7
241:         healingPoseLowerSprites: []
242:         healingPoseLowerSpritesDown: []
243:         healingPoseRaiseSprites: []
244:         healingPoseRaiseSpritesDown: []
245:         healingPoseAnimationSpeed: 0.12
246:         phase1PreferredDistanceRatio: 0.72
247:         phase2PreferredDistance: 2.6
248:         roamRadius: 9
249:         bloodBombColor: {r: 0.7, g: 0, b: 0.04, a: 0.96}
250:         explosionColor: {r: 0.9, g: 0.05, b: 0.08, a: 0.45}
251:         startInPhase2ForDebug: 0
252:         useFastPatternCooldownsForDebug: 0
253:         fastPatternCooldown: 1
254:       stomachPattern:
255:         phase2HealthRatio: 0.65
256:         phase1AttackDamage: 3
257:         phase1MoveSpeed: 0.5
258:         phase2AttackDamage: 3
259:         phase2MoveSpeed: 1
260:         phase1MeleeRange: 1.6
261:         phase1MeleeCooldown: 1.7
262:         chargeSpeed: 4
263:         chargeCooldown: 5
264:         chargeDamage: 4
265:         chargeKnockbackDistance: 2
266:         chargeWindup: 0.35
267:         chargeDuration: 1.2
268:         chargeHitRadius: 1.25
269:         phase2PatternInterval: 3
270:         phase2PreferredDistance: 4
271:         roamRadius: 9
272:         acidRange: 9
273:         acidDamage: 2
274:         acidDebuffDuration: 5
275:         acidTickInterval: 1
276:         acidTickDamage: 1
277:         acidProjectileTravelTime: 0.55
278:         acidArcHeight: 0.8
279:         acidProjectileScale: 0.9
280:         acidHitRadius: 1.15
281:         suctionRange: 4.2
282:         suctionDuration: 0.4
283:         suctionStopDistance: 1
284:         spitDelay: 0.35
285:         spitReturnDuration: 0.18
286:         shortRangeDamage: 5
287:         shortRangeRadius: 2.2
288:         suctionAttackSprites: []
289:         suctionAttackAnimationSpeed: 0.08
290:         acidColor: {r: 0.55, g: 0.95, b: 0.15, a: 0.95}
291:         acidSplashColor: {r: 0.5, g: 0.9, b: 0.12, a: 0.42}
292:         suctionColor: {r: 0.75, g: 0.95, b: 0.45, a: 0.42}
293:         leakColor: {r: 0.38, g: 0.85, b: 0.18, a: 0.38}
294:         startInPhase2ForDebug: 0
295:         useFastPatternCooldownsForDebug: 0
296:         fastPatternCooldown: 1
297:       lungPattern:
298:         phase1BrotherHealth: 40
299:         phase1AttackDamage: 2
300:         phase1MoveSpeed: 1
301:         windGustMoveSpeed: 3
302:         windGustSpeedMultiplier: 3
303:         windGustInterval: 5
304:         windGustDuration: 1.25
305:         brotherSpacing: 3.2
306:         phase1GasRange: 7
307:         phase1GasCooldown: 3.5
308:         phase1GasKnockback: 1
309:         phase2MaxHealth: 100
310:         phase2AttackDamage: 4
311:         phase2MoveSpeed: 1.5
312:         phase2GasRange: 10
313:         phase2GasCooldown: 4
314:         phase2GasKnockback: 1.2
315:         diveCooldown: 8
316:         diveWindup: 0.55
317:         diveTravelTime: 0.65
318:         diveArcHeight: 4
319:         diveLandingRadius: 1.45
320:         diveLandingDamage: 3
321:         diveKnockback: 1.4
322:         gasProjectileSpeed: 8
323:         gasProjectileRadius: 0.7
324:         gasProjectileScale: 1
325:         gasWindup: 0.25
326:         gasProjectileHeightOffset: 2
327:         phase1ContactDamage: 1
328:         phase2ContactDamage: 2
329:         contactRadius: 1.05
330:         contactCooldown: 1.1
331:         phase1PreferredDistance: 5.2
332:         phase2PreferredDistance: 4.2
333:         roamRadius: 10
334:         gasColor: {r: 0.72, g: 0.95, b: 0.9, a: 0.72}
335:         windColor: {r: 0.78, g: 0.9, b: 1, a: 0.34}
336:         diveColor: {r: 0.5, g: 0.85, b: 1, a: 0.42}
337:         startInPhase2ForDebug: 0
338:         useFastPatternCooldownsForDebug: 0
339:         fastPatternCooldown: 1
```

## Assets/_Project/Data/BiomeConfigs/IntestineEnemySpawnConfig.asset

```yaml
4: MonoBehaviour:
15:   normalKillsPerElite: 10
16:   enemySpawnRules:
17:   - monsterDefinition: {fileID: 11400000, guid: 650bae08b30304f06acc8eb4119faeac, type: 2}
18:     name: Macrophage
19:     density: 0.05
20:     minDistance: 6
21:     poissonSalt: 501
22:     allowedRegions:
23:     maxAlive: 2
24:     activationRadius: 55
25:     respawnCooldown: 5
26:     spawnRadius: 2
27:     moveSpeed: 1
28:     stoppingDistance: 0.1
29:     wanderRadius: 6
30:     chaseRadius: 10
31:     leashRadius: 14
32:     idleDelayRange: {x: 0.8, y: 1.6}
33:     maxHealth: 6
34:     attackDamage: 1
35:     attackRange: 4
36:     attackCooldown: 1
37:     expReward: 10
38:     enableContactDamage: 1
39:     contactDamage: 1
40:     contactKnockbackDistance: 0.45
41:     additionalBaseStats: []
42:     separationDistance: 1.2
43:     separationStrength: 1.1
44:     heightOffset: 0
45:     scale: {x: 1.1, y: 1.1, z: 1.1}
46:     sortingOrder: 1000
47:     useBillboard: 1
48:     useYSort: 1
49:     animationSpeed: 0.14
50:     addCollider: 1
51:     isTrigger: 0
52:     colliderSize: {x: 0.75, y: 1.1, z: 0.75}
53:     colliderCenter: {x: 0, y: 0.55, z: 0}
54:     idleSprites:
63:     idleSpritesUp: []
64:     idleSpritesDown: []
65:     moveSprites:
75:     moveSpritesUp: []
76:     moveSpritesDown: []
77:     attackSprites:
84:     attackSpritesUp: []
85:     attackSpritesDown: []
86:     attackAnimationSpeed: 0.12
87:     isRanged: 0
88:     projectileSpeed: 8
89:     projectileLifeTime: 3
90:     projectileSprite: {fileID: 0}
91:     projectileScale: {x: 0.4, y: 0.4, z: 0.4}
92:     projectileSpawnOffset: 0.5
93:     expandColliderOnAttack: 1
94:     attackColliderSize: {x: 2.2, y: 2, z: 2.2}
95:     attackColliderCenter: {x: 0, y: 0.55, z: 0}
96:     deathSprites:
104:     deathAnimationSpeed: 0.15
105:     isElite: 0
106:     tintColor: {r: 1, g: 1, b: 1, a: 1}
107:     killTriggerEnemyName:
108:     killTriggerCount: 5
109:     splitsOnDeath: 0
110:     splitCount: 2
111:     splitEnemyName:
112:     splitVfxSprites: []
113:     splitVfxScale: 3
114:     splitVfxSpeed: 0.08
115:     splitVfxDuration: 0.6
116:     chargesAtPlayer: 0
117:     chargeSpeed: 6
118:     chargeAccelTime: 0.3
119:     leavesDebrisOnDeath: 0
120:     debrisDuration: 5
121:     debrisAggroRadius: 8
122:     debrisVfxSprites: []
123:     debrisVfxScale: 15
124:     debrisVfxSpeed: 0.12
125:   - monsterDefinition: {fileID: 11400000, guid: b025256d6048b45cba4d3e38214b1bac, type: 2}
126:     name: NKCell
127:     density: 0.07
128:     minDistance: 5
129:     poissonSalt: 502
130:     allowedRegions:
131:     maxAlive: 3
132:     activationRadius: 58
133:     respawnCooldown: 4
134:     spawnRadius: 2.5
135:     moveSpeed: 1
136:     stoppingDistance: 0.1
137:     wanderRadius: 5
138:     chaseRadius: 7
139:     leashRadius: 10
140:     idleDelayRange: {x: 0.4, y: 1.1}
141:     maxHealth: 6
142:     attackDamage: 1
143:     attackRange: 1.5
144:     attackCooldown: 1
145:     expReward: 10
146:     enableContactDamage: 1
147:     contactDamage: 1
148:     contactKnockbackDistance: 0.45
149:     additionalBaseStats: []
150:     separationDistance: 1.05
151:     separationStrength: 1.2
152:     heightOffset: 0
153:     scale: {x: 1, y: 1, z: 1}
154:     sortingOrder: 1000
155:     useBillboard: 1
156:     useYSort: 1
157:     animationSpeed: 0.12
158:     addCollider: 1
159:     isTrigger: 0
160:     colliderSize: {x: 0.65, y: 0.95, z: 0.65}
161:     colliderCenter: {x: 0, y: 0.48, z: 0}
162:     idleSprites: []
163:     idleSpritesUp: []
164:     idleSpritesDown: []
165:     moveSprites:
172:     moveSpritesUp: []
173:     moveSpritesDown: []
174:     attackSprites:
183:     attackSpritesUp:
192:     attackSpritesDown:
201:     attackAnimationSpeed: 0.1
202:     isRanged: 0
203:     projectileSpeed: 8
204:     projectileLifeTime: 3
205:     projectileSprite: {fileID: 0}
206:     projectileScale: {x: 0.4, y: 0.4, z: 0.4}
207:     projectileSpawnOffset: 0.5
208:     expandColliderOnAttack: 0
209:     attackColliderSize: {x: 0.65, y: 0.95, z: 0.65}
210:     attackColliderCenter: {x: 0, y: 0.48, z: 0}
211:     deathSprites:
219:     deathAnimationSpeed: 0.12
220:     isElite: 0
221:     tintColor: {r: 1, g: 1, b: 1, a: 1}
222:     killTriggerEnemyName:
223:     killTriggerCount: 5
224:     splitsOnDeath: 0
225:     splitCount: 2
226:     splitEnemyName:
227:     splitVfxSprites: []
228:     splitVfxScale: 3
229:     splitVfxSpeed: 0.08
230:     splitVfxDuration: 0.6
231:     chargesAtPlayer: 0
232:     chargeSpeed: 6
233:     chargeAccelTime: 0.3
234:     leavesDebrisOnDeath: 0
235:     debrisDuration: 5
236:     debrisAggroRadius: 8
237:     debrisVfxSprites: []
238:     debrisVfxScale: 15
239:     debrisVfxSpeed: 0.12
240:   - monsterDefinition: {fileID: 11400000, guid: 38da51144b79d46af83b7d0260844d36, type: 2}
241:     name: BCell
242:     density: 0.04
243:     minDistance: 7
244:     poissonSalt: 503
245:     allowedRegions:
246:     maxAlive: 2
247:     activationRadius: 55
248:     respawnCooldown: 6
249:     spawnRadius: 2
250:     moveSpeed: 0.8
251:     stoppingDistance: 0.1
252:     wanderRadius: 4
253:     chaseRadius: 10
254:     leashRadius: 14
255:     idleDelayRange: {x: 0.6, y: 1.4}
256:     maxHealth: 4
257:     attackDamage: 1
258:     attackRange: 7
259:     attackCooldown: 1.5
260:     expReward: 15
261:     enableContactDamage: 1
262:     contactDamage: 1
263:     contactKnockbackDistance: 0.45
264:     additionalBaseStats: []
265:     separationDistance: 1.3
266:     separationStrength: 1
267:     heightOffset: 0
268:     scale: {x: 1, y: 1, z: 1}
269:     sortingOrder: 1000
270:     useBillboard: 1
271:     useYSort: 1
272:     animationSpeed: 0.14
273:     addCollider: 1
274:     isTrigger: 0
275:     colliderSize: {x: 0.9, y: 1.5, z: 0.9}
276:     colliderCenter: {x: 0, y: 0.75, z: 0}
277:     idleSprites:
286:     idleSpritesUp: []
287:     idleSpritesDown: []
288:     moveSprites:
297:     moveSpritesUp: []
298:     moveSpritesDown: []
299:     attackSprites:
308:     attackSpritesUp: []
309:     attackSpritesDown: []
310:     attackAnimationSpeed: 0.12
311:     isRanged: 1
312:     projectileSpeed: 6
313:     projectileLifeTime: 3
314:     projectileSprite: {fileID: 0}
315:     projectileScale: {x: 0.3, y: 0.3, z: 0.3}
316:     projectileSpawnOffset: 0.5
317:     expandColliderOnAttack: 0
318:     attackColliderSize: {x: 2, y: 2, z: 2}
319:     attackColliderCenter: {x: 0, y: 0.45, z: 0}
320:     deathSprites:
329:     deathAnimationSpeed: 0.15
330:     isElite: 0
331:     tintColor: {r: 1, g: 1, b: 1, a: 1}
332:     killTriggerEnemyName:
333:     killTriggerCount: 5
334:     splitsOnDeath: 0
335:     splitCount: 2
336:     splitEnemyName:
337:     splitVfxSprites: []
338:     splitVfxScale: 3
339:     splitVfxSpeed: 0.08
340:     splitVfxDuration: 0.6
341:     chargesAtPlayer: 0
342:     chargeSpeed: 6
343:     chargeAccelTime: 0.3
344:     leavesDebrisOnDeath: 0
345:     debrisDuration: 5
346:     debrisAggroRadius: 8
347:     debrisVfxSprites: []
348:     debrisVfxScale: 15
349:     debrisVfxSpeed: 0.12
350:   - monsterDefinition: {fileID: 11400000, guid: 7ac5acdc9d7ad4d9f8c020f3491bb0e2, type: 2}
351:     name: Granuloma
352:     density: 0.01
353:     minDistance: 15
354:     poissonSalt: 510
355:     allowedRegions:
356:     maxAlive: 1
357:     activationRadius: 60
358:     respawnCooldown: 30
359:     spawnRadius: 3
360:     moveSpeed: 1.2
361:     stoppingDistance: 0.1
362:     wanderRadius: 6
363:     chaseRadius: 10
364:     leashRadius: 14
365:     idleDelayRange: {x: 0.8, y: 1.6}
366:     maxHealth: 30
367:     attackDamage: 1
368:     attackRange: 4
369:     attackCooldown: 1
370:     expReward: 50
371:     enableContactDamage: 1
372:     contactDamage: 1
373:     contactKnockbackDistance: 0.45
374:     additionalBaseStats:
375:     - statType: 3
376:       value: 0.15
377:     separationDistance: 1.5
378:     separationStrength: 1.1
379:     heightOffset: 0
380:     scale: {x: 3.5, y: 3.5, z: 3.5}
381:     sortingOrder: 1100
382:     useBillboard: 1
383:     useYSort: 1
384:     animationSpeed: 0.14
385:     addCollider: 1
386:     isTrigger: 0
387:     colliderSize: {x: 2, y: 3, z: 2}
388:     colliderCenter: {x: 0, y: 1.5, z: 0}
389:     idleSprites:
398:     idleSpritesUp: []
399:     idleSpritesDown: []
400:     moveSprites:
410:     moveSpritesUp: []
411:     moveSpritesDown: []
412:     attackSprites:
419:     attackSpritesUp: []
420:     attackSpritesDown: []
421:     attackAnimationSpeed: 0.12
422:     isRanged: 0
423:     projectileSpeed: 8
424:     projectileLifeTime: 3
425:     projectileSprite: {fileID: 0}
426:     projectileScale: {x: 0.4, y: 0.4, z: 0.4}
427:     projectileSpawnOffset: 0.5
428:     expandColliderOnAttack: 1
429:     attackColliderSize: {x: 6, y: 4, z: 6}
430:     attackColliderCenter: {x: 0, y: 1.5, z: 0}
431:     deathSprites:
439:     deathAnimationSpeed: 0.15
440:     isElite: 1
441:     tintColor: {r: 0.8, g: 0.4, b: 0.9, a: 1}
442:     killTriggerEnemyName: Macrophage
443:     killTriggerCount: 5
444:     splitsOnDeath: 1
445:     splitCount: 2
446:     splitEnemyName: Macrophage
447:     splitVfxSprites: []
448:     splitVfxScale: 3
449:     splitVfxSpeed: 0.08
450:     splitVfxDuration: 0.6
451:     chargesAtPlayer: 0
452:     chargeSpeed: 6
453:     chargeAccelTime: 0.3
454:     leavesDebrisOnDeath: 0
455:     debrisDuration: 5
456:     debrisAggroRadius: 8
457:     debrisVfxSprites: []
458:     debrisVfxScale: 15
459:     debrisVfxSpeed: 0.12
460:   - monsterDefinition: {fileID: 11400000, guid: 9e12b36d011fb490ba7505d3e3219228, type: 2}
461:     name: Antibody
462:     density: 0.01
463:     minDistance: 15
464:     poissonSalt: 520
465:     allowedRegions:
466:     maxAlive: 1
467:     activationRadius: 60
468:     respawnCooldown: 30
469:     spawnRadius: 3
470:     moveSpeed: 1.4
471:     stoppingDistance: 0.1
472:     wanderRadius: 5
473:     chaseRadius: 8
474:     leashRadius: 14
475:     idleDelayRange: {x: 0.5, y: 1.2}
476:     maxHealth: 35
477:     attackDamage: 1
478:     attackRange: 1.5
479:     attackCooldown: 0.8
480:     expReward: 60
481:     enableContactDamage: 1
482:     contactDamage: 1
483:     contactKnockbackDistance: 0.45
484:     additionalBaseStats:
485:     - statType: 3
486:       value: 0.15
487:     separationDistance: 1.3
488:     separationStrength: 1
489:     heightOffset: 0
490:     scale: {x: 5, y: 5, z: 5}
491:     sortingOrder: 1100
492:     useBillboard: 1
493:     useYSort: 1
494:     animationSpeed: 0.12
495:     addCollider: 1
496:     isTrigger: 0
497:     colliderSize: {x: 3, y: 4, z: 3}
498:     colliderCenter: {x: 0, y: 2, z: 0}
499:     idleSprites:
508:     idleSpritesUp: []
509:     idleSpritesDown: []
510:     moveSprites:
519:     moveSpritesUp: []
520:     moveSpritesDown: []
521:     attackSprites:
530:     attackSpritesUp: []
531:     attackSpritesDown: []
532:     attackAnimationSpeed: 0.1
533:     isRanged: 0
534:     projectileSpeed: 8
535:     projectileLifeTime: 3
536:     projectileSprite: {fileID: 0}
537:     projectileScale: {x: 0.4, y: 0.4, z: 0.4}
538:     projectileSpawnOffset: 0.5
539:     expandColliderOnAttack: 0
540:     attackColliderSize: {x: 2, y: 2, z: 2}
541:     attackColliderCenter: {x: 0, y: 0.6, z: 0}
542:     deathSprites:
548:     deathAnimationSpeed: 0.15
549:     isElite: 1
550:     tintColor: {r: 1, g: 1, b: 1, a: 1}
551:     killTriggerEnemyName: NKCell
552:     killTriggerCount: 5
553:     splitsOnDeath: 0
554:     splitCount: 2
555:     splitEnemyName:
556:     splitVfxSprites: []
557:     splitVfxScale: 3
558:     splitVfxSpeed: 0.08
559:     splitVfxDuration: 0.6
560:     chargesAtPlayer: 1
561:     chargeSpeed: 6
562:     chargeAccelTime: 0.3
563:     leavesDebrisOnDeath: 1
564:     debrisDuration: 10
565:     debrisAggroRadius: 8
566:     debrisVfxSprites:
573:     debrisVfxScale: 25
574:     debrisVfxSpeed: 0.08
575:   - monsterDefinition: {fileID: 11400000, guid: fa0e27fbe157f4123897b7576c0f6c43, type: 2}
576:     name: IntestineFoldworm
577:     density: 0.02
578:     minDistance: 6
579:     poissonSalt: 1604
580:     allowedRegions:
581:     maxAlive: 2
582:     activationRadius: 55
583:     respawnCooldown: 6
584:     spawnRadius: 2
585:     moveSpeed: 1.1
586:     stoppingDistance: 0.1
587:     wanderRadius: 5
588:     chaseRadius: 10
589:     leashRadius: 14
590:     idleDelayRange: {x: 0.4, y: 1.1}
591:     maxHealth: 6
592:     attackDamage: 1
593:     attackRange: 1.5
594:     attackCooldown: 1
595:     expReward: 10
596:     enableContactDamage: 1
597:     contactDamage: 1
598:     contactKnockbackDistance: 0.45
599:     additionalBaseStats: []
600:     separationDistance: 1.05
601:     separationStrength: 1.2
602:     heightOffset: 0
603:     scale: {x: 1, y: 1, z: 1}
604:     sortingOrder: 1000
605:     useBillboard: 1
606:     useYSort: 1
607:     animationSpeed: 0.16
608:     addCollider: 1
609:     isTrigger: 0
610:     colliderSize: {x: 0.85, y: 1.2, z: 0.85}
611:     colliderCenter: {x: 0, y: 0.6, z: 0}
612:     idleSprites:
617:     idleSpritesUp: []
618:     idleSpritesDown: []
619:     moveSprites:
624:     moveSpritesUp: []
625:     moveSpritesDown: []
626:     attackSprites:
631:     attackSpritesUp: []
632:     attackSpritesDown: []
633:     attackAnimationSpeed: 0.14
634:     isRanged: 0
635:     projectileSpeed: 6
636:     projectileLifeTime: 3
637:     projectileSprite: {fileID: 0}
638:     projectileScale: {x: 0.3, y: 0.3, z: 0.3}
639:     projectileSpawnOffset: 0.5
640:     expandColliderOnAttack: 0
641:     attackColliderSize: {x: 0.65, y: 0.95, z: 0.65}
642:     attackColliderCenter: {x: 0, y: 0.48, z: 0}
643:     deathSprites:
648:     deathAnimationSpeed: 0.16
649:     isElite: 0
650:     tintColor: {r: 1, g: 1, b: 1, a: 1}
651:     killTriggerEnemyName:
652:     killTriggerCount: 5
653:     splitsOnDeath: 0
654:     splitCount: 2
655:     splitEnemyName:
656:     splitVfxSprites: []
657:     splitVfxScale: 3
658:     splitVfxSpeed: 0.08
659:     splitVfxDuration: 0.6
660:     chargesAtPlayer: 0
661:     chargeSpeed: 6
662:     chargeAccelTime: 0.3
663:     leavesDebrisOnDeath: 0
664:     debrisDuration: 5
665:     debrisAggroRadius: 8
666:     debrisVfxSprites: []
667:     debrisVfxScale: 15
668:     debrisVfxSpeed: 0.12
669:   - monsterDefinition: {fileID: 11400000, guid: 78210f5a2c21844c1b8ec2acc9654ab2, type: 2}
670:     name: IntestineVillusHand
671:     density: 0.02
672:     minDistance: 6
673:     poissonSalt: 1605
674:     allowedRegions:
675:     maxAlive: 2
676:     activationRadius: 55
677:     respawnCooldown: 6
678:     spawnRadius: 2
679:     moveSpeed: 0.8
680:     stoppingDistance: 0.1
681:     wanderRadius: 5
682:     chaseRadius: 10
683:     leashRadius: 14
684:     idleDelayRange: {x: 0.4, y: 1.1}
685:     maxHealth: 6
686:     attackDamage: 1
687:     attackRange: 1.8
688:     attackCooldown: 1.2
689:     expReward: 10
690:     enableContactDamage: 1
691:     contactDamage: 1
692:     contactKnockbackDistance: 0.45
693:     additionalBaseStats: []
694:     separationDistance: 1.05
695:     separationStrength: 1.2
696:     heightOffset: 0
697:     scale: {x: 1, y: 1, z: 1}
698:     sortingOrder: 1000
699:     useBillboard: 1
700:     useYSort: 1
701:     animationSpeed: 0.16
702:     addCollider: 1
703:     isTrigger: 0
704:     colliderSize: {x: 0.85, y: 1.2, z: 0.85}
705:     colliderCenter: {x: 0, y: 0.6, z: 0}
706:     idleSprites:
711:     idleSpritesUp: []
712:     idleSpritesDown: []
713:     moveSprites:
718:     moveSpritesUp: []
719:     moveSpritesDown: []
720:     attackSprites:
725:     attackSpritesUp: []
726:     attackSpritesDown: []
727:     attackAnimationSpeed: 0.14
728:     isRanged: 0
729:     projectileSpeed: 6
730:     projectileLifeTime: 3
731:     projectileSprite: {fileID: 0}
732:     projectileScale: {x: 0.3, y: 0.3, z: 0.3}
733:     projectileSpawnOffset: 0.5
734:     expandColliderOnAttack: 0
735:     attackColliderSize: {x: 0.65, y: 0.95, z: 0.65}
736:     attackColliderCenter: {x: 0, y: 0.48, z: 0}
737:     deathSprites:
742:     deathAnimationSpeed: 0.16
743:     isElite: 0
744:     tintColor: {r: 1, g: 1, b: 1, a: 1}
745:     killTriggerEnemyName:
746:     killTriggerCount: 5
747:     splitsOnDeath: 0
748:     splitCount: 2
749:     splitEnemyName:
750:     splitVfxSprites: []
751:     splitVfxScale: 3
752:     splitVfxSpeed: 0.08
753:     splitVfxDuration: 0.6
754:     chargesAtPlayer: 0
755:     chargeSpeed: 6
756:     chargeAccelTime: 0.3
757:     leavesDebrisOnDeath: 0
758:     debrisDuration: 5
759:     debrisAggroRadius: 8
760:     debrisVfxSprites: []
761:     debrisVfxScale: 15
762:     debrisVfxSpeed: 0.12
763:   - monsterDefinition: {fileID: 11400000, guid: 78699e52925f548d1acecb49cb2d265f, type: 2}
764:     name: IntestineKnottedEel
765:     density: 0.02
766:     minDistance: 6
767:     poissonSalt: 1606
768:     allowedRegions:
769:     maxAlive: 2
770:     activationRadius: 55
771:     respawnCooldown: 6
772:     spawnRadius: 2
773:     moveSpeed: 0.9
774:     stoppingDistance: 0.1
775:     wanderRadius: 5
776:     chaseRadius: 10
777:     leashRadius: 14
778:     idleDelayRange: {x: 0.4, y: 1.1}
779:     maxHealth: 8
780:     attackDamage: 1
781:     attackRange: 1.7
782:     attackCooldown: 1.2
783:     expReward: 12
784:     enableContactDamage: 1
785:     contactDamage: 1
786:     contactKnockbackDistance: 0.45
787:     additionalBaseStats: []
788:     separationDistance: 1.05
789:     separationStrength: 1.2
790:     heightOffset: 0
791:     scale: {x: 1, y: 1, z: 1}
792:     sortingOrder: 1000
793:     useBillboard: 1
794:     useYSort: 1
795:     animationSpeed: 0.16
796:     addCollider: 1
797:     isTrigger: 0
798:     colliderSize: {x: 0.85, y: 1.2, z: 0.85}
799:     colliderCenter: {x: 0, y: 0.6, z: 0}
800:     idleSprites:
805:     idleSpritesUp: []
806:     idleSpritesDown: []
807:     moveSprites:
812:     moveSpritesUp: []
813:     moveSpritesDown: []
814:     attackSprites:
819:     attackSpritesUp: []
820:     attackSpritesDown: []
821:     attackAnimationSpeed: 0.14
822:     isRanged: 0
823:     projectileSpeed: 6
824:     projectileLifeTime: 3
825:     projectileSprite: {fileID: 0}
826:     projectileScale: {x: 0.3, y: 0.3, z: 0.3}
827:     projectileSpawnOffset: 0.5
828:     expandColliderOnAttack: 0
829:     attackColliderSize: {x: 0.65, y: 0.95, z: 0.65}
830:     attackColliderCenter: {x: 0, y: 0.48, z: 0}
831:     deathSprites:
836:     deathAnimationSpeed: 0.16
837:     isElite: 0
838:     tintColor: {r: 1, g: 1, b: 1, a: 1}
839:     killTriggerEnemyName:
840:     killTriggerCount: 5
841:     splitsOnDeath: 0
842:     splitCount: 2
843:     splitEnemyName:
844:     splitVfxSprites: []
845:     splitVfxScale: 3
846:     splitVfxSpeed: 0.08
847:     splitVfxDuration: 0.6
848:     chargesAtPlayer: 0
849:     chargeSpeed: 6
850:     chargeAccelTime: 0.3
851:     leavesDebrisOnDeath: 0
852:     debrisDuration: 5
853:     debrisAggroRadius: 8
854:     debrisVfxSprites: []
855:     debrisVfxScale: 15
856:     debrisVfxSpeed: 0.12
```

## Assets/_Project/Data/BiomeConfigs/LiverBiomeConfig.asset

```yaml
4: MonoBehaviour:
15:   biomeType: 2
16:   regionCellSize: 35
17:   regionBlendWidth: 4
18:   detailNoiseScale: 0.015
19:   regions:
20:   - name: Grass1
21:     baseHeight: 0
22:     primaryTile: {fileID: 11400000, guid: a1b2c3d4e5f6a7b8c9d0e1f2a3b4c501, type: 2}
23:     primaryType: 1
24:     variantTile: {fileID: 11400000, guid: a1b2c3d4e5f6a7b8c9d0e1f2a3b4c502, type: 2}
25:     variantType: 2
26:     variantThreshold: 0.9
27:     tileVariants:
36:   - name: Grass3
37:     baseHeight: -1
38:     primaryTile: {fileID: 11400000, guid: a1b2c3d4e5f6a7b8c9d0e1f2a3b4c503, type: 2}
39:     primaryType: 1
40:     variantTile: {fileID: 11400000, guid: a1b2c3d4e5f6a7b8c9d0e1f2a3b4c504, type: 2}
41:     variantType: 2
42:     variantThreshold: 0.9
43:     tileVariants:
52:   - name: Grass5
53:     baseHeight: 1
54:     primaryTile: {fileID: 11400000, guid: a1b2c3d4e5f6a7b8c9d0e1f2a3b4c505, type: 2}
55:     primaryType: 1
56:     variantTile: {fileID: 11400000, guid: a1b2c3d4e5f6a7b8c9d0e1f2a3b4c506, type: 2}
57:     variantType: 2
58:     variantThreshold: 0.9
59:     tileVariants:
68:   - name: Sand
69:     baseHeight: -2
70:     primaryTile: {fileID: 11400000, guid: a1b2c3d4e5f6a7b8c9d0e1f2a3b4c507, type: 2}
71:     primaryType: 1
72:     variantTile: {fileID: 11400000, guid: a1b2c3d4e5f6a7b8c9d0e1f2a3b4c508, type: 2}
73:     variantType: 2
74:     variantThreshold: 0.95
75:     tileVariants: []
76:   - name: SandLake
77:     baseHeight: -3
78:     primaryTile: {fileID: 11400000, guid: a1b2c3d4e5f6a7b8c9d0e1f2a3b4c509, type: 2}
79:     primaryType: 1
80:     variantTile: {fileID: 0}
81:     variantType: 0
82:     variantThreshold: 0.2
83:     tileVariants:
92:   heightNoiseScale: 0.015
93:   heightNoiseAmplitude: 0.3
94:   tileMappings:
95:   - tileType: 1
96:     tile: {fileID: 11400000, guid: a1b2c3d4e5f6a7b8c9d0e1f2a3b4c501, type: 2}
97:   exteriorBackdrop:
98:     enabled: 0
99:     sprite: {fileID: 0}
100:     tint: {r: 1, g: 1, b: 1, a: 1}
101:     opacity: 0.82
102:     cameraClearColor: {r: 0.035, g: 0.015, b: 0.018, a: 1}
103:     cameraDistance: 50
104:     overscan: 1.2
105:     driftAmount: 0.015
106:     driftSpeed: 0.08
107:     sortingOrder: -32000
108:   marginLeft: 5
109:   marginRight: 5
110:   marginBottom: 10
111:   marginTop: 5
112:   objectRules: []
113:   enemySpawnConfig: {fileID: 11400000, guid: 946670425bda4f0293f85cfd208a6778, type: 2}
114:   biomeEliteSpawnConfig: {fileID: 11400000, guid: d717064dbf9f84c7ba2ffa089d53d1d4, type: 2}
115:   enemySpawnRules: []
116:   bossArenaConfig: {fileID: 11400000, guid: 83239c28f0d24030ad9fe289abc11f93, type: 2}
117:   midBossArena:
118:     enabled: 1
119:     onlyEnableOnLargeMaps: 1
120:     minimumMapWidth: 300
121:     minimumMapHeight: 300
122:     useCustomCenter: 0
123:     centerGrid: {x: 150, y: 150}
124:     arenaSize: {x: 32, y: 32}
125:     wallThicknessInCells: 1
126:     lockBoundaryInsetInCells: 1
127:     triggerInsetInCells: 2
128:     wallHeightOffset: 1.5
129:     triggerHeight: 4
130:     sortingOrder: 3500
131:     hideBossUntilEncounter: 1
132:     presentation:
133:       enabled: 1
134:       entranceSide: 0
135:       entranceWidthInCells: 4
136:       approachLengthInCells: 9
137:       pulseSpeed: 1.4
138:       approachOpacity: 0.72
139:       entranceSprite: {fileID: 0}
140:       entranceVisualScale: 1.35
141:       wallStraightSprites: []
142:       wallCornerSprite: {fileID: 0}
143:       wallCornerSpanInCells: 2
144:       wallCornerBaseYaw: 270
145:       wallUseStraightCornerConnectors: 0
146:       floorSortingOrder: 40
147:       gateSortingOrder: 3490
148:       useBiomePalette: 1
149:       primaryColor: {r: 0.43, g: 0.72, b: 0.22, a: 1}
150:       accentColor: {r: 0.82, g: 0.48, b: 0.16, a: 1}
151:       lockedColor: {r: 0.92, g: 0.12, b: 0.08, a: 1}
152:       clearedColor: {r: 0.34, g: 0.32, b: 0.3, a: 1}
153:     returnPortalSprite: {fileID: 0}
154:     returnPortalScale: {x: 1, y: 1, z: 1}
155:     boss:
156:       displayName: MidBoss
157:       useCustomBossRule: 0
158:       bossRule:
159:         monsterDefinition: {fileID: 0}
160:         name: Enemy
161:         density: 0.0025
162:         minDistance: 8
163:         poissonSalt: 400
164:         allowedRegions: 
165:         maxAlive: 1
166:         activationRadius: 20
167:         respawnCooldown: 8
168:         spawnRadius: 1.5
169:         moveSpeed: 1.5
170:         stoppingDistance: 0.1
171:         wanderRadius: 4
172:         chaseRadius: 6
173:         leashRadius: 8
174:         idleDelayRange: {x: 0.5, y: 1.5}
175:         maxHealth: 30
176:         attackDamage: 1
177:         attackRange: 1.5
178:         attackCooldown: 1
179:         expReward: 10
180:         enableContactDamage: 1
181:         contactDamage: 1
182:         contactKnockbackDistance: 0.45
183:         additionalBaseStats: []
184:         separationDistance: 1.1
185:         separationStrength: 1
186:         heightOffset: 0
187:         scale: {x: 1, y: 1, z: 1}
188:         sortingOrder: 1000
189:         useBillboard: 1
190:         useYSort: 1
191:         animationSpeed: 0.15
192:         addCollider: 1
193:         isTrigger: 0
194:         colliderSize: {x: 0.7, y: 1.1, z: 0.7}
195:         colliderCenter: {x: 0, y: 0.55, z: 0}
196:         idleSprites: []
197:         idleSpritesUp: []
198:         idleSpritesDown: []
199:         moveSprites: []
200:         moveSpritesUp: []
201:         moveSpritesDown: []
202:         attackSprites: []
203:         attackSpritesUp: []
204:         attackSpritesDown: []
205:         attackAnimationSpeed: 0.12
206:         isRanged: 0
207:         projectileSpeed: 8
208:         projectileLifeTime: 3
209:         projectileSprite: {fileID: 0}
210:         projectileScale: {x: 0.4, y: 0.4, z: 0.4}
211:         projectileSpawnOffset: 0.5
212:         expandColliderOnAttack: 0
213:         attackColliderSize: {x: 2, y: 2, z: 2}
214:         attackColliderCenter: {x: 0, y: 0.55, z: 0}
215:         deathSprites: []
216:         deathAnimationSpeed: 0.15
217:         isElite: 0
218:         tintColor: {r: 1, g: 1, b: 1, a: 1}
219:         killTriggerEnemyName: 
220:         killTriggerCount: 10
221:         splitsOnDeath: 0
222:         splitCount: 2
223:         splitEnemyName: 
224:         splitVfxSprites: []
225:         splitVfxScale: 3
226:         splitVfxSpeed: 0.08
227:         splitVfxDuration: 0.6
228:         chargesAtPlayer: 0
229:         chargeSpeed: 6
230:         chargeAccelTime: 0.3
231:         leavesDebrisOnDeath: 0
232:         debrisDuration: 5
233:         debrisAggroRadius: 8
234:         debrisVfxSprites: []
235:         debrisVfxScale: 15
236:         debrisVfxSpeed: 0.12
237:       useEnemyRuleFallback: 1
238:       fallbackEnemyRuleIndex: 0
239:       patternType: 0
240:       overrideStats: 0
241:       maxHealthMultiplier: 1
242:       attackDamageMultiplier: 1
243:       moveSpeedMultiplier: 1
244:       scaleMultiplier: {x: 1, y: 1, z: 1}
245:       minimumMaxHealth: 0
246:       intestinePattern:
247:         phase2HealthRatio: 0.5
248:         phase1FleeDistance: 7
249:         phase1RoamRadius: 8
250:         phase1MoveMultiplier: 1
251:         phase2PreferredDistance: 2.5
252:         phase2MoveMultiplier: 1.15
253:         dungPreDelay: 0.5
254:         phase1DungCooldown: 8
255:         phase2ThrowCooldown: 5
256:         dungCollisionDamage: 1
257:         dungSlowRatio: 0.2
258:         dungSlowDuration: 3
259:         dungHazardRadius: 0.75
260:         dungLifeTime: 10
261:         throwTravelTime: 0.7
262:         throwArcHeight: 2.5
263:         parasiteMinCount: 2
264:         parasiteMaxCount: 3
265:         parasiteSpawnDelay: 0.45
266:         parasiteSpawnRadius: 1.1
267:         parasiteDefinition: {fileID: 0}
268:         parasiteMaxHealth: 6
269:         parasiteMoveSpeed: 2.3
270:         parasiteAttackDamage: 1
271:         parasiteAttackRange: 1.1
272:         parasiteAttackCooldown: 1.3
273:         parasiteScale: {x: 0.55, y: 0.55, z: 0.55}
274:         stompMinDelay: 7
275:         stompPreDelay: 0.55
276:         stompRadius: 6
277:         stompDamage: 2
278:         stompSlowRatio: 0.3
279:         stompSlowDuration: 3
280:         dungColor: {r: 0.34, g: 0.18, b: 0.08, a: 0.95}
281:         parasiteColor: {r: 0.62, g: 0.88, b: 0.36, a: 1}
282:         shockwaveColor: {r: 0.65, g: 0.38, b: 0.18, a: 0.45}
283:         startInPhase2ForDebug: 0
284:         useFastPatternCooldownsForDebug: 0
285:         fastPatternCooldown: 1
286:       liverPattern:
287:         phase2HealthRatio: 0.5
288:         phase1AttackDamage: 2
289:         phase1MoveSpeed: 1
290:         phase2AttackDamage: 2
291:         phase2MoveSpeed: 1.5
292:         bloodBombRange: 7
293:         bloodBombExplosionRadius: 2.5
294:         bloodBombFuseTime: 1
295:         bloodBombCooldown: 4
296:         attackPowerDebuffRatio: 0.15
297:         attackPowerDebuffDuration: 5
298:         bloodBombArcHeight: 1.4
299:         bloodBombProjectileScale: 1.15
300:         bloodBombSprite: {fileID: 0}
301:         healingPosePreDelay: 1
302:         healingPoseCooldown: 15
303:         healingPoseDuration: 5
304:         damageHealRatio: 0.7
305:         healingPoseLowerSprites: []
306:         healingPoseLowerSpritesDown: []
307:         healingPoseRaiseSprites: []
308:         healingPoseRaiseSpritesDown: []
309:         healingPoseAnimationSpeed: 0.12
310:         phase1PreferredDistanceRatio: 0.72
311:         phase2PreferredDistance: 2.6
312:         roamRadius: 9
313:         bloodBombColor: {r: 0.7, g: 0, b: 0.04, a: 0.96}
314:         explosionColor: {r: 0.9, g: 0.05, b: 0.08, a: 0.45}
315:         startInPhase2ForDebug: 0
316:         useFastPatternCooldownsForDebug: 0
317:         fastPatternCooldown: 1
318:       stomachPattern:
319:         phase2HealthRatio: 0.65
320:         phase1AttackDamage: 3
321:         phase1MoveSpeed: 0.5
322:         phase2AttackDamage: 3
323:         phase2MoveSpeed: 1
324:         phase1MeleeRange: 1.6
325:         phase1MeleeCooldown: 1.7
326:         chargeSpeed: 4
327:         chargeCooldown: 5
328:         chargeDamage: 4
329:         chargeKnockbackDistance: 2
330:         chargeWindup: 0.35
331:         chargeDuration: 1.2
332:         chargeHitRadius: 1.25
333:         phase2PatternInterval: 3
334:         phase2PreferredDistance: 4
335:         roamRadius: 9
336:         acidRange: 9
337:         acidDamage: 2
338:         acidDebuffDuration: 5
339:         acidTickInterval: 1
340:         acidTickDamage: 1
341:         acidProjectileTravelTime: 0.55
342:         acidArcHeight: 0.8
343:         acidProjectileScale: 0.9
344:         acidHitRadius: 1.15
345:         suctionRange: 4.2
346:         suctionDuration: 0.4
347:         suctionStopDistance: 1
348:         spitDelay: 0.35
349:         spitReturnDuration: 0.18
350:         shortRangeDamage: 5
351:         shortRangeRadius: 2.2
352:         suctionAttackSprites: []
353:         suctionAttackAnimationSpeed: 0.08
354:         acidColor: {r: 0.55, g: 0.95, b: 0.15, a: 0.95}
355:         acidSplashColor: {r: 0.5, g: 0.9, b: 0.12, a: 0.42}
356:         suctionColor: {r: 0.75, g: 0.95, b: 0.45, a: 0.42}
357:         leakColor: {r: 0.38, g: 0.85, b: 0.18, a: 0.38}
358:         startInPhase2ForDebug: 0
359:         useFastPatternCooldownsForDebug: 0
360:         fastPatternCooldown: 1
361:       lungPattern:
362:         phase1BrotherHealth: 40
363:         phase1AttackDamage: 2
364:         phase1MoveSpeed: 1
365:         windGustMoveSpeed: 3
366:         windGustSpeedMultiplier: 3
367:         windGustInterval: 5
368:         windGustDuration: 1.25
369:         brotherSpacing: 3.2
370:         phase1GasRange: 7
371:         phase1GasCooldown: 3.5
372:         phase1GasKnockback: 1
373:         phase2MaxHealth: 100
374:         phase2AttackDamage: 4
375:         phase2MoveSpeed: 1.5
376:         phase2GasRange: 10
377:         phase2GasCooldown: 4
378:         phase2GasKnockback: 1.2
379:         diveCooldown: 8
380:         diveWindup: 0.55
381:         diveTravelTime: 0.65
382:         diveArcHeight: 4
383:         diveLandingRadius: 1.45
384:         diveLandingDamage: 3
385:         diveKnockback: 1.4
386:         gasProjectileSpeed: 8
387:         gasProjectileRadius: 0.7
388:         gasProjectileScale: 1
389:         gasWindup: 0.25
390:         gasProjectileHeightOffset: 2
391:         phase1ContactDamage: 1
392:         phase2ContactDamage: 2
393:         contactRadius: 1.05
394:         contactCooldown: 1.1
395:         phase1PreferredDistance: 5.2
396:         phase2PreferredDistance: 4.2
397:         roamRadius: 10
398:         gasColor: {r: 0.72, g: 0.95, b: 0.9, a: 0.72}
399:         windColor: {r: 0.78, g: 0.9, b: 1, a: 0.34}
400:         diveColor: {r: 0.5, g: 0.85, b: 1, a: 0.42}
401:         startInPhase2ForDebug: 0
402:         useFastPatternCooldownsForDebug: 0
403:         fastPatternCooldown: 1
404:   returnPortal:
405:     enabled: 1
406:     name: ReturnPortal
407:     poolKind: 100
408:     sprite: {fileID: 249089422, guid: fbc2f259a8b301f42933cd4689e5842c, type: 3}
409:     sortingOrder: 1000
410:     useCustomPosition: 1
411:     gridPosition: {x: 75, y: 4}
412:     heightOffset: 0
413:     useBillboard: 1
414:     scale: {x: 0.5, y: 0.5, z: 0.5}
415:     addCollider: 1
416:     isTrigger: 1
417:     colliderSize: {x: 2, y: 2, z: 2}
418:     colliderCenter: {x: 0, y: 0, z: 0}
```

## Assets/_Project/Data/BiomeConfigs/LiverBossArenaConfig.asset

```yaml
4: MonoBehaviour:
15:   midBossArena:
16:     enabled: 1
17:     onlyEnableOnLargeMaps: 0
18:     minimumMapWidth: 300
19:     minimumMapHeight: 300
20:     useCustomCenter: 0
21:     centerGrid: {x: 150, y: 150}
22:     arenaSize: {x: 32, y: 32}
23:     wallThicknessInCells: 1
24:     lockBoundaryInsetInCells: 0
25:     triggerInsetInCells: 2
26:     wallHeightOffset: 1.5
27:     triggerHeight: 4
28:     sortingOrder: 3500
29:     hideBossUntilEncounter: 1
30:     presentation:
31:       enabled: 1
32:       entranceSide: 0
33:       entranceWidthInCells: 4
34:       approachLengthInCells: 10
35:       pulseSpeed: 1.7
36:       approachOpacity: 0.76
37:       entranceSprite: {fileID: 21300000, guid: b597b9b9edb14483ab33adb7be88d60d, type: 3}
38:       entranceVisualScale: 1.45
39:       wallStraightSprites:
42:       wallCornerSprite: {fileID: 21300000, guid: f7947f3ff84cc43d0ad46dab6801c7cd, type: 3}
43:       wallCornerSpanInCells: 2
44:       wallCornerBaseYaw: 270
45:       wallUseStraightCornerConnectors: 1
46:       floorSortingOrder: 40
47:       gateSortingOrder: 3490
48:       useBiomePalette: 1
49:       primaryColor: {r: 0.43, g: 0.72, b: 0.22, a: 1}
50:       accentColor: {r: 0.82, g: 0.48, b: 0.16, a: 1}
51:       lockedColor: {r: 0.92, g: 0.12, b: 0.08, a: 1}
52:       clearedColor: {r: 0.34, g: 0.32, b: 0.3, a: 1}
53:     returnPortalSprite: {fileID: 0}
54:     returnPortalScale: {x: 1, y: 1, z: 1}
55:     boss:
56:       displayName: LiverBoss
57:       useCustomBossRule: 1
58:       bossRule:
59:         monsterDefinition: {fileID: 11400000, guid: 847841a3fded34a95b1389ef9d84b3b9, type: 2}
60:         name: LiverBoss
61:         density: 0
62:         minDistance: 8
63:         poissonSalt: 7102
64:         allowedRegions: 
65:         maxAlive: 1
66:         activationRadius: 0
67:         respawnCooldown: 0
68:         spawnRadius: 0
69:         moveSpeed: 1
70:         stoppingDistance: 0.1
71:         wanderRadius: 4
72:         chaseRadius: 16
73:         leashRadius: 24
74:         idleDelayRange: {x: 0.5, y: 1.2}
75:         maxHealth: 180
76:         attackDamage: 2
77:         attackRange: 7
78:         attackCooldown: 1
79:         expReward: 0
80:         enableContactDamage: 1
81:         contactDamage: 1
82:         contactKnockbackDistance: 0.45
83:         additionalBaseStats: []
84:         separationDistance: 0
85:         separationStrength: 0
86:         heightOffset: 0
87:         scale: {x: 1.1, y: 1.1, z: 1.1}
88:         sortingOrder: 1000
89:         useBillboard: 1
90:         useYSort: 1
91:         animationSpeed: 0.14
92:         addCollider: 1
93:         isTrigger: 0
94:         colliderSize: {x: 0.75, y: 1.1, z: 0.75}
95:         colliderCenter: {x: 0, y: 0.55, z: 0}
96:         idleSprites:
101:         idleSpritesUp:
106:         idleSpritesDown:
111:         moveSprites:
118:         moveSpritesUp:
125:         moveSpritesDown:
132:         attackSprites:
138:         attackSpritesUp:
144:         attackSpritesDown:
150:         attackAnimationSpeed: 0.12
151:         isRanged: 0
152:         projectileSpeed: 8
153:         projectileLifeTime: 3
154:         projectileSprite: {fileID: 0}
155:         projectileScale: {x: 0.4, y: 0.4, z: 0.4}
156:         projectileSpawnOffset: 0.5
157:         expandColliderOnAttack: 0
158:         attackColliderSize: {x: 2, y: 2, z: 2}
159:         attackColliderCenter: {x: 0, y: 0.55, z: 0}
160:         deathSprites:
168:         deathAnimationSpeed: 0.15
169:         isElite: 0
170:         tintColor: {r: 1, g: 1, b: 1, a: 1}
171:         killTriggerEnemyName: 
172:         killTriggerCount: 0
173:         splitsOnDeath: 0
174:         splitCount: 0
175:         splitEnemyName: 
176:         splitVfxSprites: []
177:         splitVfxScale: 3
178:         splitVfxSpeed: 0.08
179:         splitVfxDuration: 0.6
180:         chargesAtPlayer: 0
181:         chargeSpeed: 6
182:         chargeAccelTime: 0.3
183:         leavesDebrisOnDeath: 0
184:         debrisDuration: 5
185:         debrisAggroRadius: 8
186:         debrisVfxSprites: []
187:         debrisVfxScale: 15
188:         debrisVfxSpeed: 0.12
189:       useEnemyRuleFallback: 1
190:       fallbackEnemyRuleIndex: 0
191:       patternType: 3
192:       overrideStats: 0
193:       maxHealthMultiplier: 1
194:       attackDamageMultiplier: 1
195:       moveSpeedMultiplier: 1
196:       scaleMultiplier: {x: 4, y: 4, z: 4}
197:       minimumMaxHealth: 0
198:       intestinePattern:
199:         phase2HealthRatio: 0.5
200:         phase1FleeDistance: 7
201:         phase1RoamRadius: 8
202:         phase1MoveMultiplier: 1
203:         phase2PreferredDistance: 2.5
204:         phase2MoveMultiplier: 1.15
205:         dungPreDelay: 0.5
206:         phase1DungCooldown: 8
207:         phase2ThrowCooldown: 5
208:         dungCollisionDamage: 1
209:         dungSlowRatio: 0.2
210:         dungSlowDuration: 3
211:         dungHazardRadius: 0.75
212:         dungLifeTime: 10
213:         throwTravelTime: 0.7
214:         throwArcHeight: 2.5
215:         parasiteMinCount: 2
216:         parasiteMaxCount: 3
217:         parasiteSpawnDelay: 0.45
218:         parasiteSpawnRadius: 1.1
219:         parasiteDefinition: {fileID: 0}
220:         parasiteMaxHealth: 6
221:         parasiteMoveSpeed: 2.3
222:         parasiteAttackDamage: 1
223:         parasiteAttackRange: 1.1
224:         parasiteAttackCooldown: 1.3
225:         parasiteScale: {x: 0.55, y: 0.55, z: 0.55}
226:         stompMinDelay: 7
227:         stompPreDelay: 0.55
228:         stompRadius: 6
229:         stompDamage: 2
230:         stompSlowRatio: 0.3
231:         stompSlowDuration: 3
232:         dungColor: {r: 0.34, g: 0.18, b: 0.08, a: 0.95}
233:         parasiteColor: {r: 0.62, g: 0.88, b: 0.36, a: 1}
234:         shockwaveColor: {r: 0.65, g: 0.38, b: 0.18, a: 0.45}
235:         startInPhase2ForDebug: 0
236:         useFastPatternCooldownsForDebug: 0
237:         fastPatternCooldown: 1
238:       liverPattern:
239:         phase2HealthRatio: 0.5
240:         phase1AttackDamage: 2
241:         phase1MoveSpeed: 1
242:         phase2AttackDamage: 2
243:         phase2MoveSpeed: 1.5
244:         bloodBombRange: 7
245:         bloodBombExplosionRadius: 2.5
246:         bloodBombFuseTime: 1
247:         bloodBombCooldown: 4
248:         attackPowerDebuffRatio: 0.15
249:         attackPowerDebuffDuration: 5
250:         bloodBombArcHeight: 1.4
251:         bloodBombProjectileScale: 4
252:         bloodBombSprite: {fileID: 473467103, guid: a71f4732a4349ab45b4b1d038597c5f8, type: 3}
253:         healingPosePreDelay: 1
254:         healingPoseCooldown: 15
255:         healingPoseDuration: 5
256:         damageHealRatio: 0.7
257:         healingPoseLowerSprites:
262:         healingPoseLowerSpritesDown:
267:         healingPoseRaiseSprites:
273:         healingPoseRaiseSpritesDown:
279:         healingPoseAnimationSpeed: 0.12
280:         phase1PreferredDistanceRatio: 0.72
281:         phase2PreferredDistance: 2.6
282:         roamRadius: 9
283:         bloodBombColor: {r: 0.7, g: 0, b: 0.04, a: 0.96}
284:         explosionColor: {r: 0.9, g: 0.05, b: 0.08, a: 0.45}
285:         startInPhase2ForDebug: 0
286:         useFastPatternCooldownsForDebug: 0
287:         fastPatternCooldown: 1
288:       stomachPattern:
289:         phase2HealthRatio: 0.65
290:         phase1AttackDamage: 3
291:         phase1MoveSpeed: 0.5
292:         phase2AttackDamage: 3
293:         phase2MoveSpeed: 1
294:         phase1MeleeRange: 1.6
295:         phase1MeleeCooldown: 1.7
296:         chargeSpeed: 4
297:         chargeCooldown: 5
298:         chargeDamage: 4
299:         chargeKnockbackDistance: 2
300:         chargeWindup: 0.35
301:         chargeDuration: 1.2
302:         chargeHitRadius: 1.25
303:         phase2PatternInterval: 3
304:         phase2PreferredDistance: 4
305:         roamRadius: 9
306:         acidRange: 9
307:         acidDamage: 2
308:         acidDebuffDuration: 5
309:         acidTickInterval: 1
310:         acidTickDamage: 1
311:         acidProjectileTravelTime: 0.55
312:         acidArcHeight: 0.8
313:         acidProjectileScale: 0.9
314:         acidHitRadius: 1.15
315:         suctionRange: 4.2
316:         suctionDuration: 0.4
317:         suctionStopDistance: 1
318:         spitDelay: 0.35
319:         spitReturnDuration: 0.18
320:         shortRangeDamage: 5
321:         shortRangeRadius: 2.2
322:         suctionAttackSprites: []
323:         suctionAttackAnimationSpeed: 0.08
324:         acidColor: {r: 0.55, g: 0.95, b: 0.15, a: 0.95}
325:         acidSplashColor: {r: 0.5, g: 0.9, b: 0.12, a: 0.42}
326:         suctionColor: {r: 0.75, g: 0.95, b: 0.45, a: 0.42}
327:         leakColor: {r: 0.38, g: 0.85, b: 0.18, a: 0.38}
328:         startInPhase2ForDebug: 0
329:         useFastPatternCooldownsForDebug: 0
330:         fastPatternCooldown: 1
331:       lungPattern:
332:         phase1BrotherHealth: 40
333:         phase1AttackDamage: 2
334:         phase1MoveSpeed: 1
335:         windGustMoveSpeed: 3
336:         windGustSpeedMultiplier: 3
337:         windGustInterval: 5
338:         windGustDuration: 1.25
339:         brotherSpacing: 3.2
340:         phase1GasRange: 7
341:         phase1GasCooldown: 3.5
342:         phase1GasKnockback: 1
343:         phase2MaxHealth: 100
344:         phase2AttackDamage: 4
345:         phase2MoveSpeed: 1.5
346:         phase2GasRange: 10
347:         phase2GasCooldown: 4
348:         phase2GasKnockback: 1.2
349:         diveCooldown: 8
350:         diveWindup: 0.55
351:         diveTravelTime: 0.65
352:         diveArcHeight: 4
353:         diveLandingRadius: 1.45
354:         diveLandingDamage: 3
355:         diveKnockback: 1.4
356:         gasProjectileSpeed: 8
357:         gasProjectileRadius: 0.7
358:         gasProjectileScale: 1
359:         gasWindup: 0.25
360:         gasProjectileHeightOffset: 2
361:         phase1ContactDamage: 1
362:         phase2ContactDamage: 2
363:         contactRadius: 1.05
364:         contactCooldown: 1.1
365:         phase1PreferredDistance: 5.2
366:         phase2PreferredDistance: 4.2
367:         roamRadius: 10
368:         gasColor: {r: 0.72, g: 0.95, b: 0.9, a: 0.72}
369:         windColor: {r: 0.78, g: 0.9, b: 1, a: 0.34}
370:         diveColor: {r: 0.5, g: 0.85, b: 1, a: 0.42}
371:         startInPhase2ForDebug: 0
372:         useFastPatternCooldownsForDebug: 0
373:         fastPatternCooldown: 1
```

## Assets/_Project/Data/BiomeConfigs/LiverEnemySpawnConfig.asset

```yaml
4: MonoBehaviour:
15:   normalKillsPerElite: 10
16:   enemySpawnRules:
17:   - monsterDefinition: {fileID: 11400000, guid: 83e58fda032b14a40b4a66d7631a4558, type: 2}
18:     name: LiverSutureCell
19:     density: 0.05
20:     minDistance: 6
21:     poissonSalt: 1607
22:     allowedRegions:
23:     maxAlive: 2
24:     activationRadius: 55
25:     respawnCooldown: 6
26:     spawnRadius: 2
27:     moveSpeed: 0.8
28:     stoppingDistance: 0.1
29:     wanderRadius: 4
30:     chaseRadius: 10
31:     leashRadius: 14
32:     idleDelayRange: {x: 0.6, y: 1.4}
33:     maxHealth: 4
34:     attackDamage: 1
35:     attackRange: 7
36:     attackCooldown: 1.5
37:     expReward: 15
38:     enableContactDamage: 1
39:     contactDamage: 1
40:     contactKnockbackDistance: 0.45
41:     additionalBaseStats: []
42:     separationDistance: 1.3
43:     separationStrength: 1
44:     heightOffset: 0
45:     scale: {x: 1, y: 1, z: 1}
46:     sortingOrder: 1000
47:     useBillboard: 1
48:     useYSort: 1
49:     animationSpeed: 0.16
50:     addCollider: 1
51:     isTrigger: 0
52:     colliderSize: {x: 0.85, y: 1.2, z: 0.85}
53:     colliderCenter: {x: 0, y: 0.6, z: 0}
54:     idleSprites:
59:     idleSpritesUp: []
60:     idleSpritesDown: []
61:     moveSprites:
66:     moveSpritesUp: []
67:     moveSpritesDown: []
68:     attackSprites:
73:     attackSpritesUp: []
74:     attackSpritesDown: []
75:     attackAnimationSpeed: 0.14
76:     isRanged: 1
77:     projectileSpeed: 6
78:     projectileLifeTime: 3
79:     projectileSprite: {fileID: 0}
80:     projectileScale: {x: 0.3, y: 0.3, z: 0.3}
81:     projectileSpawnOffset: 0.5
82:     expandColliderOnAttack: 0
83:     attackColliderSize: {x: 2, y: 2, z: 2}
84:     attackColliderCenter: {x: 0, y: 0.45, z: 0}
85:     deathSprites:
90:     deathAnimationSpeed: 0.16
91:     isElite: 0
92:     tintColor: {r: 1, g: 1, b: 1, a: 1}
93:     killTriggerEnemyName:
94:     killTriggerCount: 5
95:     splitsOnDeath: 0
96:     splitCount: 2
97:     splitEnemyName:
98:     splitVfxSprites: []
99:     splitVfxScale: 3
100:     splitVfxSpeed: 0.08
101:     splitVfxDuration: 0.6
102:     chargesAtPlayer: 0
103:     chargeSpeed: 6
104:     chargeAccelTime: 0.3
105:     leavesDebrisOnDeath: 0
106:     debrisDuration: 5
107:     debrisAggroRadius: 8
108:     debrisVfxSprites: []
109:     debrisVfxScale: 15
110:     debrisVfxSpeed: 0.12
111:   - monsterDefinition: {fileID: 11400000, guid: b9f33a6fa1cea4cdeb17d27e9cc9f8c0, type: 2}
112:     name: LiverClot
113:     density: 0.05
114:     minDistance: 6
115:     poissonSalt: 1608
116:     allowedRegions:
117:     maxAlive: 2
118:     activationRadius: 55
119:     respawnCooldown: 6
120:     spawnRadius: 2
121:     moveSpeed: 0.8
122:     stoppingDistance: 0.1
123:     wanderRadius: 5
124:     chaseRadius: 10
125:     leashRadius: 14
126:     idleDelayRange: {x: 0.4, y: 1.1}
127:     maxHealth: 8
128:     attackDamage: 1
129:     attackRange: 1.5
130:     attackCooldown: 1.2
131:     expReward: 12
132:     enableContactDamage: 1
133:     contactDamage: 1
134:     contactKnockbackDistance: 0.45
135:     additionalBaseStats: []
136:     separationDistance: 1.05
137:     separationStrength: 1.2
138:     heightOffset: 0
139:     scale: {x: 1, y: 1, z: 1}
140:     sortingOrder: 1000
141:     useBillboard: 1
142:     useYSort: 1
143:     animationSpeed: 0.16
144:     addCollider: 1
145:     isTrigger: 0
146:     colliderSize: {x: 0.85, y: 1.2, z: 0.85}
147:     colliderCenter: {x: 0, y: 0.6, z: 0}
148:     idleSprites:
153:     idleSpritesUp: []
154:     idleSpritesDown: []
155:     moveSprites:
160:     moveSpritesUp: []
161:     moveSpritesDown: []
162:     attackSprites:
167:     attackSpritesUp: []
168:     attackSpritesDown: []
169:     attackAnimationSpeed: 0.14
170:     isRanged: 0
171:     projectileSpeed: 6
172:     projectileLifeTime: 3
173:     projectileSprite: {fileID: 0}
174:     projectileScale: {x: 0.3, y: 0.3, z: 0.3}
175:     projectileSpawnOffset: 0.5
176:     expandColliderOnAttack: 0
177:     attackColliderSize: {x: 0.65, y: 0.95, z: 0.65}
178:     attackColliderCenter: {x: 0, y: 0.48, z: 0}
179:     deathSprites:
184:     deathAnimationSpeed: 0.16
185:     isElite: 0
186:     tintColor: {r: 1, g: 1, b: 1, a: 1}
187:     killTriggerEnemyName:
188:     killTriggerCount: 5
189:     splitsOnDeath: 0
190:     splitCount: 2
191:     splitEnemyName:
192:     splitVfxSprites: []
193:     splitVfxScale: 3
194:     splitVfxSpeed: 0.08
195:     splitVfxDuration: 0.6
196:     chargesAtPlayer: 0
197:     chargeSpeed: 6
198:     chargeAccelTime: 0.3
199:     leavesDebrisOnDeath: 0
200:     debrisDuration: 5
201:     debrisAggroRadius: 8
202:     debrisVfxSprites: []
203:     debrisVfxScale: 15
204:     debrisVfxSpeed: 0.12
205:   - monsterDefinition: {fileID: 11400000, guid: d29c7a3924e2c4b068c031c5159dacfb, type: 2}
206:     name: LiverCoagulationCore
207:     density: 0.05
208:     minDistance: 6
209:     poissonSalt: 1609
210:     allowedRegions:
211:     maxAlive: 2
212:     activationRadius: 55
213:     respawnCooldown: 6
214:     spawnRadius: 2
215:     moveSpeed: 0.8
216:     stoppingDistance: 0.1
217:     wanderRadius: 4
218:     chaseRadius: 10
219:     leashRadius: 14
220:     idleDelayRange: {x: 0.6, y: 1.4}
221:     maxHealth: 6
222:     attackDamage: 1
223:     attackRange: 6.5
224:     attackCooldown: 1.8
225:     expReward: 15
226:     enableContactDamage: 1
227:     contactDamage: 1
228:     contactKnockbackDistance: 0.45
229:     additionalBaseStats: []
230:     separationDistance: 1.3
231:     separationStrength: 1
232:     heightOffset: 0
233:     scale: {x: 1, y: 1, z: 1}
234:     sortingOrder: 1000
235:     useBillboard: 1
236:     useYSort: 1
237:     animationSpeed: 0.16
238:     addCollider: 1
239:     isTrigger: 0
240:     colliderSize: {x: 0.85, y: 1.2, z: 0.85}
241:     colliderCenter: {x: 0, y: 0.6, z: 0}
242:     idleSprites:
247:     idleSpritesUp: []
248:     idleSpritesDown: []
249:     moveSprites:
254:     moveSpritesUp: []
255:     moveSpritesDown: []
256:     attackSprites:
261:     attackSpritesUp: []
262:     attackSpritesDown: []
263:     attackAnimationSpeed: 0.14
264:     isRanged: 1
265:     projectileSpeed: 6
266:     projectileLifeTime: 3
267:     projectileSprite: {fileID: 0}
268:     projectileScale: {x: 0.3, y: 0.3, z: 0.3}
269:     projectileSpawnOffset: 0.5
270:     expandColliderOnAttack: 0
271:     attackColliderSize: {x: 2, y: 2, z: 2}
272:     attackColliderCenter: {x: 0, y: 0.45, z: 0}
273:     deathSprites:
278:     deathAnimationSpeed: 0.16
279:     isElite: 0
280:     tintColor: {r: 1, g: 1, b: 1, a: 1}
281:     killTriggerEnemyName:
282:     killTriggerCount: 5
283:     splitsOnDeath: 0
284:     splitCount: 2
285:     splitEnemyName:
286:     splitVfxSprites: []
287:     splitVfxScale: 3
288:     splitVfxSpeed: 0.08
289:     splitVfxDuration: 0.6
290:     chargesAtPlayer: 0
291:     chargeSpeed: 6
292:     chargeAccelTime: 0.3
293:     leavesDebrisOnDeath: 0
294:     debrisDuration: 5
295:     debrisAggroRadius: 8
296:     debrisVfxSprites: []
297:     debrisVfxScale: 15
298:     debrisVfxSpeed: 0.12
```

## Assets/_Project/Data/BiomeConfigs/LungBiomeConfig.asset

```yaml
4: MonoBehaviour:
15:   biomeType: 4
16:   regionCellSize: 35
17:   regionBlendWidth: 4
18:   detailNoiseScale: 0.015
19:   regions:
20:   - name: Soil
21:     baseHeight: 0
22:     primaryTile: {fileID: 11400000, guid: c1b2c3d4e5f6a7b8c9d0e1f2a3b4c501, type: 2}
23:     primaryType: 1
24:     variantTile: {fileID: 11400000, guid: c1b2c3d4e5f6a7b8c9d0e1f2a3b4c503, type: 2}
25:     variantType: 2
26:     variantThreshold: 0.92
27:     tileVariants:
36:   - name: Rock
37:     baseHeight: 1
38:     primaryTile: {fileID: 11400000, guid: c1b2c3d4e5f6a7b8c9d0e1f2a3b4c502, type: 2}
39:     primaryType: 1
40:     variantTile: {fileID: 11400000, guid: c1b2c3d4e5f6a7b8c9d0e1f2a3b4c504, type: 2}
41:     variantType: 2
42:     variantThreshold: 0.92
43:     tileVariants:
52:   - name: Sand
53:     baseHeight: -1
54:     primaryTile: {fileID: 11400000, guid: c1b2c3d4e5f6a7b8c9d0e1f2a3b4c503, type: 2}
55:     primaryType: 1
56:     variantTile: {fileID: 11400000, guid: c1b2c3d4e5f6a7b8c9d0e1f2a3b4c501, type: 2}
57:     variantType: 2
58:     variantThreshold: 0.8
59:     tileVariants: []
60:   heightNoiseScale: 0.015
61:   heightNoiseAmplitude: 0.25
62:   tileMappings:
63:   - tileType: 1
64:     tile: {fileID: 11400000, guid: c1b2c3d4e5f6a7b8c9d0e1f2a3b4c501, type: 2}
65:   exteriorBackdrop:
66:     enabled: 0
67:     sprite: {fileID: 0}
68:     tint: {r: 1, g: 1, b: 1, a: 1}
69:     opacity: 0.82
70:     cameraClearColor: {r: 0.035, g: 0.015, b: 0.018, a: 1}
71:     cameraDistance: 50
72:     overscan: 1.2
73:     driftAmount: 0.015
74:     driftSpeed: 0.08
75:     sortingOrder: -32000
76:   marginLeft: 5
77:   marginRight: 5
78:   marginBottom: 10
79:   marginTop: 5
80:   objectRules: []
81:   enemySpawnConfig: {fileID: 11400000, guid: c93fc930d0dc4ab6a9211f386f571da8, type: 2}
82:   biomeEliteSpawnConfig: {fileID: 11400000, guid: b552e3f7fd4bb46cba55fa399fa8e2ea, type: 2}
83:   enemySpawnRules: []
84:   bossArenaConfig: {fileID: 11400000, guid: 353d9400a39f48b3a072e0f202b091a0, type: 2}
85:   midBossArena:
86:     enabled: 1
87:     onlyEnableOnLargeMaps: 1
88:     minimumMapWidth: 300
89:     minimumMapHeight: 300
90:     useCustomCenter: 0
91:     centerGrid: {x: 150, y: 150}
92:     arenaSize: {x: 32, y: 32}
93:     wallThicknessInCells: 1
94:     lockBoundaryInsetInCells: 1
95:     triggerInsetInCells: 2
96:     wallHeightOffset: 1.5
97:     triggerHeight: 4
98:     sortingOrder: 3500
99:     hideBossUntilEncounter: 1
100:     presentation:
101:       enabled: 1
102:       entranceSide: 0
103:       entranceWidthInCells: 4
104:       approachLengthInCells: 9
105:       pulseSpeed: 1.4
106:       approachOpacity: 0.72
107:       entranceSprite: {fileID: 0}
108:       entranceVisualScale: 1.35
109:       wallStraightSprites: []
110:       wallCornerSprite: {fileID: 0}
111:       wallCornerSpanInCells: 2
112:       wallCornerBaseYaw: 270
113:       wallUseStraightCornerConnectors: 0
114:       floorSortingOrder: 40
115:       gateSortingOrder: 3490
116:       useBiomePalette: 1
117:       primaryColor: {r: 0.43, g: 0.72, b: 0.22, a: 1}
118:       accentColor: {r: 0.82, g: 0.48, b: 0.16, a: 1}
119:       lockedColor: {r: 0.92, g: 0.12, b: 0.08, a: 1}
120:       clearedColor: {r: 0.34, g: 0.32, b: 0.3, a: 1}
121:     returnPortalSprite: {fileID: 0}
122:     returnPortalScale: {x: 1, y: 1, z: 1}
123:     boss:
124:       displayName: MidBoss
125:       useCustomBossRule: 0
126:       bossRule:
127:         monsterDefinition: {fileID: 0}
128:         name: Enemy
129:         density: 0.0025
130:         minDistance: 8
131:         poissonSalt: 400
132:         allowedRegions: 
133:         maxAlive: 1
134:         activationRadius: 20
135:         respawnCooldown: 8
136:         spawnRadius: 1.5
137:         moveSpeed: 1.5
138:         stoppingDistance: 0.1
139:         wanderRadius: 4
140:         chaseRadius: 6
141:         leashRadius: 8
142:         idleDelayRange: {x: 0.5, y: 1.5}
143:         maxHealth: 30
144:         attackDamage: 1
145:         attackRange: 1.5
146:         attackCooldown: 1
147:         expReward: 10
148:         enableContactDamage: 1
149:         contactDamage: 1
150:         contactKnockbackDistance: 0.45
151:         additionalBaseStats: []
152:         separationDistance: 1.1
153:         separationStrength: 1
154:         heightOffset: 0
155:         scale: {x: 1, y: 1, z: 1}
156:         sortingOrder: 1000
157:         useBillboard: 1
158:         useYSort: 1
159:         animationSpeed: 0.15
160:         addCollider: 1
161:         isTrigger: 0
162:         colliderSize: {x: 0.7, y: 1.1, z: 0.7}
163:         colliderCenter: {x: 0, y: 0.55, z: 0}
164:         idleSprites: []
165:         idleSpritesUp: []
166:         idleSpritesDown: []
167:         moveSprites: []
168:         moveSpritesUp: []
169:         moveSpritesDown: []
170:         attackSprites: []
171:         attackSpritesUp: []
172:         attackSpritesDown: []
173:         attackAnimationSpeed: 0.12
174:         isRanged: 0
175:         projectileSpeed: 8
176:         projectileLifeTime: 3
177:         projectileSprite: {fileID: 0}
178:         projectileScale: {x: 0.4, y: 0.4, z: 0.4}
179:         projectileSpawnOffset: 0.5
180:         expandColliderOnAttack: 0
181:         attackColliderSize: {x: 2, y: 2, z: 2}
182:         attackColliderCenter: {x: 0, y: 0.55, z: 0}
183:         deathSprites: []
184:         deathAnimationSpeed: 0.15
185:         isElite: 0
186:         tintColor: {r: 1, g: 1, b: 1, a: 1}
187:         killTriggerEnemyName: 
188:         killTriggerCount: 10
189:         splitsOnDeath: 0
190:         splitCount: 2
191:         splitEnemyName: 
192:         splitVfxSprites: []
193:         splitVfxScale: 3
194:         splitVfxSpeed: 0.08
195:         splitVfxDuration: 0.6
196:         chargesAtPlayer: 0
197:         chargeSpeed: 6
198:         chargeAccelTime: 0.3
199:         leavesDebrisOnDeath: 0
200:         debrisDuration: 5
201:         debrisAggroRadius: 8
202:         debrisVfxSprites: []
203:         debrisVfxScale: 15
204:         debrisVfxSpeed: 0.12
205:       useEnemyRuleFallback: 1
206:       fallbackEnemyRuleIndex: 0
207:       patternType: 0
208:       overrideStats: 0
209:       maxHealthMultiplier: 1
210:       attackDamageMultiplier: 1
211:       moveSpeedMultiplier: 1
212:       scaleMultiplier: {x: 1, y: 1, z: 1}
213:       minimumMaxHealth: 0
214:       intestinePattern:
215:         phase2HealthRatio: 0.5
216:         phase1FleeDistance: 7
217:         phase1RoamRadius: 8
218:         phase1MoveMultiplier: 1
219:         phase2PreferredDistance: 2.5
220:         phase2MoveMultiplier: 1.15
221:         dungPreDelay: 0.5
222:         phase1DungCooldown: 8
223:         phase2ThrowCooldown: 5
224:         dungCollisionDamage: 1
225:         dungSlowRatio: 0.2
226:         dungSlowDuration: 3
227:         dungHazardRadius: 0.75
228:         dungLifeTime: 10
229:         throwTravelTime: 0.7
230:         throwArcHeight: 2.5
231:         parasiteMinCount: 2
232:         parasiteMaxCount: 3
233:         parasiteSpawnDelay: 0.45
234:         parasiteSpawnRadius: 1.1
235:         parasiteDefinition: {fileID: 0}
236:         parasiteMaxHealth: 6
237:         parasiteMoveSpeed: 2.3
238:         parasiteAttackDamage: 1
239:         parasiteAttackRange: 1.1
240:         parasiteAttackCooldown: 1.3
241:         parasiteScale: {x: 0.55, y: 0.55, z: 0.55}
242:         stompMinDelay: 7
243:         stompPreDelay: 0.55
244:         stompRadius: 6
245:         stompDamage: 2
246:         stompSlowRatio: 0.3
247:         stompSlowDuration: 3
248:         dungColor: {r: 0.34, g: 0.18, b: 0.08, a: 0.95}
249:         parasiteColor: {r: 0.62, g: 0.88, b: 0.36, a: 1}
250:         shockwaveColor: {r: 0.65, g: 0.38, b: 0.18, a: 0.45}
251:         startInPhase2ForDebug: 0
252:         useFastPatternCooldownsForDebug: 0
253:         fastPatternCooldown: 1
254:       liverPattern:
255:         phase2HealthRatio: 0.5
256:         phase1AttackDamage: 2
257:         phase1MoveSpeed: 1
258:         phase2AttackDamage: 2
259:         phase2MoveSpeed: 1.5
260:         bloodBombRange: 7
261:         bloodBombExplosionRadius: 2.5
262:         bloodBombFuseTime: 1
263:         bloodBombCooldown: 4
264:         attackPowerDebuffRatio: 0.15
265:         attackPowerDebuffDuration: 5
266:         bloodBombArcHeight: 1.4
267:         bloodBombProjectileScale: 1.15
268:         bloodBombSprite: {fileID: 0}
269:         healingPosePreDelay: 1
270:         healingPoseCooldown: 15
271:         healingPoseDuration: 5
272:         damageHealRatio: 0.7
273:         healingPoseLowerSprites: []
274:         healingPoseLowerSpritesDown: []
275:         healingPoseRaiseSprites: []
276:         healingPoseRaiseSpritesDown: []
277:         healingPoseAnimationSpeed: 0.12
278:         phase1PreferredDistanceRatio: 0.72
279:         phase2PreferredDistance: 2.6
280:         roamRadius: 9
281:         bloodBombColor: {r: 0.7, g: 0, b: 0.04, a: 0.96}
282:         explosionColor: {r: 0.9, g: 0.05, b: 0.08, a: 0.45}
283:         startInPhase2ForDebug: 0
284:         useFastPatternCooldownsForDebug: 0
285:         fastPatternCooldown: 1
286:       stomachPattern:
287:         phase2HealthRatio: 0.65
288:         phase1AttackDamage: 3
289:         phase1MoveSpeed: 0.5
290:         phase2AttackDamage: 3
291:         phase2MoveSpeed: 1
292:         phase1MeleeRange: 1.6
293:         phase1MeleeCooldown: 1.7
294:         chargeSpeed: 4
295:         chargeCooldown: 5
296:         chargeDamage: 4
297:         chargeKnockbackDistance: 2
298:         chargeWindup: 0.35
299:         chargeDuration: 1.2
300:         chargeHitRadius: 1.25
301:         phase2PatternInterval: 3
302:         phase2PreferredDistance: 4
303:         roamRadius: 9
304:         acidRange: 9
305:         acidDamage: 2
306:         acidDebuffDuration: 5
307:         acidTickInterval: 1
308:         acidTickDamage: 1
309:         acidProjectileTravelTime: 0.55
310:         acidArcHeight: 0.8
311:         acidProjectileScale: 0.9
312:         acidHitRadius: 1.15
313:         suctionRange: 4.2
314:         suctionDuration: 0.4
315:         suctionStopDistance: 1
316:         spitDelay: 0.35
317:         spitReturnDuration: 0.18
318:         shortRangeDamage: 5
319:         shortRangeRadius: 2.2
320:         suctionAttackSprites: []
321:         suctionAttackAnimationSpeed: 0.08
322:         acidColor: {r: 0.55, g: 0.95, b: 0.15, a: 0.95}
323:         acidSplashColor: {r: 0.5, g: 0.9, b: 0.12, a: 0.42}
324:         suctionColor: {r: 0.75, g: 0.95, b: 0.45, a: 0.42}
325:         leakColor: {r: 0.38, g: 0.85, b: 0.18, a: 0.38}
326:         startInPhase2ForDebug: 0
327:         useFastPatternCooldownsForDebug: 0
328:         fastPatternCooldown: 1
329:       lungPattern:
330:         phase1BrotherHealth: 40
331:         phase1AttackDamage: 2
332:         phase1MoveSpeed: 1
333:         windGustMoveSpeed: 3
334:         windGustSpeedMultiplier: 3
335:         windGustInterval: 5
336:         windGustDuration: 1.25
337:         brotherSpacing: 3.2
338:         phase1GasRange: 7
339:         phase1GasCooldown: 3.5
340:         phase1GasKnockback: 1
341:         phase2MaxHealth: 100
342:         phase2AttackDamage: 4
343:         phase2MoveSpeed: 1.5
344:         phase2GasRange: 10
345:         phase2GasCooldown: 4
346:         phase2GasKnockback: 1.2
347:         diveCooldown: 8
348:         diveWindup: 0.55
349:         diveTravelTime: 0.65
350:         diveArcHeight: 4
351:         diveLandingRadius: 1.45
352:         diveLandingDamage: 3
353:         diveKnockback: 1.4
354:         gasProjectileSpeed: 8
355:         gasProjectileRadius: 0.7
356:         gasProjectileScale: 1
357:         gasWindup: 0.25
358:         gasProjectileHeightOffset: 2
359:         phase1ContactDamage: 1
360:         phase2ContactDamage: 2
361:         contactRadius: 1.05
362:         contactCooldown: 1.1
363:         phase1PreferredDistance: 5.2
364:         phase2PreferredDistance: 4.2
365:         roamRadius: 10
366:         gasColor: {r: 0.72, g: 0.95, b: 0.9, a: 0.72}
367:         windColor: {r: 0.78, g: 0.9, b: 1, a: 0.34}
368:         diveColor: {r: 0.5, g: 0.85, b: 1, a: 0.42}
369:         startInPhase2ForDebug: 0
370:         useFastPatternCooldownsForDebug: 0
371:         fastPatternCooldown: 1
372:   returnPortal:
373:     enabled: 1
374:     name: ReturnPortal
375:     poolKind: 100
376:     sprite: {fileID: 1816856329, guid: fbc2f259a8b301f42933cd4689e5842c, type: 3}
377:     sortingOrder: 1000
378:     useCustomPosition: 1
379:     gridPosition: {x: 67, y: 4}
380:     heightOffset: 0
381:     useBillboard: 1
382:     scale: {x: 0.5, y: 0.5, z: 0.5}
383:     addCollider: 1
384:     isTrigger: 1
385:     colliderSize: {x: 2, y: 2, z: 2}
386:     colliderCenter: {x: 0, y: 0, z: 0}
```

## Assets/_Project/Data/BiomeConfigs/LungBossArenaConfig.asset

```yaml
4: MonoBehaviour:
15:   midBossArena:
16:     enabled: 1
17:     onlyEnableOnLargeMaps: 0
18:     minimumMapWidth: 300
19:     minimumMapHeight: 300
20:     useCustomCenter: 0
21:     centerGrid: {x: 150, y: 150}
22:     arenaSize: {x: 32, y: 32}
23:     wallThicknessInCells: 1
24:     lockBoundaryInsetInCells: 0
25:     triggerInsetInCells: 2
26:     wallHeightOffset: 1.5
27:     triggerHeight: 4
28:     sortingOrder: 3500
29:     hideBossUntilEncounter: 1
30:     presentation:
31:       enabled: 1
32:       entranceSide: 0
33:       entranceWidthInCells: 6
34:       approachLengthInCells: 12
35:       pulseSpeed: 0.55
36:       approachOpacity: 0.66
37:       entranceSprite: {fileID: 21300000, guid: 17fab6da3b11d47829d6263c08d54ab1, type: 3}
38:       entranceVisualScale: 1.55
39:       wallStraightSprites:
42:       wallCornerSprite: {fileID: 1404090, guid: da546a1e79b0941d5a8eb86a3639f6c8, type: 3}
43:       wallCornerSpanInCells: 2
44:       wallCornerBaseYaw: 90
45:       wallUseStraightCornerConnectors: 0
46:       floorSortingOrder: 40
47:       gateSortingOrder: 3490
48:       useBiomePalette: 1
49:       primaryColor: {r: 0.43, g: 0.72, b: 0.22, a: 1}
50:       accentColor: {r: 0.82, g: 0.48, b: 0.16, a: 1}
51:       lockedColor: {r: 0.92, g: 0.12, b: 0.08, a: 1}
52:       clearedColor: {r: 0.34, g: 0.32, b: 0.3, a: 1}
53:     returnPortalSprite: {fileID: 0}
54:     returnPortalScale: {x: 1, y: 1, z: 1}
55:     boss:
56:       displayName: LungBoss
57:       useCustomBossRule: 1
58:       bossRule:
59:         monsterDefinition: {fileID: 11400000, guid: ef3ce2ce13fe04147ae8c692cecec940, type: 2}
60:         name: LungBoss
61:         density: 0
62:         minDistance: 7
63:         poissonSalt: 7304
64:         allowedRegions: 
65:         maxAlive: 2
66:         activationRadius: 0
67:         respawnCooldown: 0
68:         spawnRadius: 0
69:         moveSpeed: 1
70:         stoppingDistance: 0.1
71:         wanderRadius: 4
72:         chaseRadius: 16
73:         leashRadius: 24
74:         idleDelayRange: {x: 0.6, y: 1.4}
75:         maxHealth: 40
76:         attackDamage: 2
77:         attackRange: 7
78:         attackCooldown: 1
79:         expReward: 0
80:         enableContactDamage: 1
81:         contactDamage: 1
82:         contactKnockbackDistance: 0.45
83:         additionalBaseStats: []
84:         separationDistance: 0
85:         separationStrength: 0
86:         heightOffset: 0
87:         scale: {x: 1, y: 1, z: 1}
88:         sortingOrder: 1000
89:         useBillboard: 1
90:         useYSort: 1
91:         animationSpeed: 0.14
92:         addCollider: 1
93:         isTrigger: 0
94:         colliderSize: {x: 0.95, y: 1.65, z: 0.95}
95:         colliderCenter: {x: 0, y: 0.82, z: 0}
96:         idleSprites:
101:         idleSpritesUp: []
102:         idleSpritesDown: []
103:         moveSprites:
108:         moveSpritesUp: []
109:         moveSpritesDown: []
110:         attackSprites:
120:         attackSpritesUp: []
121:         attackSpritesDown: []
122:         attackAnimationSpeed: 0.12
123:         isRanged: 0
124:         projectileSpeed: 8
125:         projectileLifeTime: 3
126:         projectileSprite: {fileID: 0}
127:         projectileScale: {x: 0.4, y: 0.4, z: 0.4}
128:         projectileSpawnOffset: 0.5
129:         expandColliderOnAttack: 0
130:         attackColliderSize: {x: 2, y: 2, z: 2}
131:         attackColliderCenter: {x: 0, y: 0.48, z: 0}
132:         deathSprites:
137:         deathAnimationSpeed: 0.15
138:         isElite: 0
139:         tintColor: {r: 1, g: 1, b: 1, a: 1}
140:         killTriggerEnemyName: 
141:         killTriggerCount: 0
142:         splitsOnDeath: 0
143:         splitCount: 0
144:         splitEnemyName: 
145:         splitVfxSprites: []
146:         splitVfxScale: 3
147:         splitVfxSpeed: 0.08
148:         splitVfxDuration: 0.6
149:         chargesAtPlayer: 0
150:         chargeSpeed: 6
151:         chargeAccelTime: 0.3
152:         leavesDebrisOnDeath: 0
153:         debrisDuration: 5
154:         debrisAggroRadius: 8
155:         debrisVfxSprites: []
156:         debrisVfxScale: 15
157:         debrisVfxSpeed: 0.12
158:       useEnemyRuleFallback: 1
159:       fallbackEnemyRuleIndex: 0
160:       patternType: 5
161:       overrideStats: 0
162:       maxHealthMultiplier: 1
163:       attackDamageMultiplier: 1
164:       moveSpeedMultiplier: 1
165:       scaleMultiplier: {x: 3, y: 3, z: 3}
166:       minimumMaxHealth: 0
167:       intestinePattern:
168:         phase2HealthRatio: 0.5
169:         phase1FleeDistance: 7
170:         phase1RoamRadius: 8
171:         phase1MoveMultiplier: 1
172:         phase2PreferredDistance: 2.5
173:         phase2MoveMultiplier: 1.15
174:         dungPreDelay: 0.5
175:         phase1DungCooldown: 8
176:         phase2ThrowCooldown: 5
177:         dungCollisionDamage: 1
178:         dungSlowRatio: 0.2
179:         dungSlowDuration: 3
180:         dungHazardRadius: 0.75
181:         dungLifeTime: 10
182:         throwTravelTime: 0.7
183:         throwArcHeight: 2.5
184:         parasiteMinCount: 2
185:         parasiteMaxCount: 3
186:         parasiteSpawnDelay: 0.45
187:         parasiteSpawnRadius: 1.1
188:         parasiteDefinition: {fileID: 0}
189:         parasiteMaxHealth: 6
190:         parasiteMoveSpeed: 2.3
191:         parasiteAttackDamage: 1
192:         parasiteAttackRange: 1.1
193:         parasiteAttackCooldown: 1.3
194:         parasiteScale: {x: 0.55, y: 0.55, z: 0.55}
195:         stompMinDelay: 7
196:         stompPreDelay: 0.55
197:         stompRadius: 6
198:         stompDamage: 2
199:         stompSlowRatio: 0.3
200:         stompSlowDuration: 3
201:         dungColor: {r: 0.34, g: 0.18, b: 0.08, a: 0.95}
202:         parasiteColor: {r: 0.62, g: 0.88, b: 0.36, a: 1}
203:         shockwaveColor: {r: 0.65, g: 0.38, b: 0.18, a: 0.45}
204:         startInPhase2ForDebug: 0
205:         useFastPatternCooldownsForDebug: 0
206:         fastPatternCooldown: 1
207:       liverPattern:
208:         phase2HealthRatio: 0.5
209:         phase1AttackDamage: 2
210:         phase1MoveSpeed: 1
211:         phase2AttackDamage: 2
212:         phase2MoveSpeed: 1.5
213:         bloodBombRange: 7
214:         bloodBombExplosionRadius: 2.5
215:         bloodBombFuseTime: 1
216:         bloodBombCooldown: 4
217:         attackPowerDebuffRatio: 0.15
218:         attackPowerDebuffDuration: 5
219:         bloodBombArcHeight: 1.4
220:         bloodBombProjectileScale: 1.15
221:         bloodBombSprite: {fileID: 0}
222:         healingPosePreDelay: 1
223:         healingPoseCooldown: 15
224:         healingPoseDuration: 5
225:         damageHealRatio: 0.7
226:         healingPoseLowerSprites: []
227:         healingPoseLowerSpritesDown: []
228:         healingPoseRaiseSprites: []
229:         healingPoseRaiseSpritesDown: []
230:         healingPoseAnimationSpeed: 0.12
231:         phase1PreferredDistanceRatio: 0.72
232:         phase2PreferredDistance: 2.6
233:         roamRadius: 9
234:         bloodBombColor: {r: 0.7, g: 0, b: 0.04, a: 0.96}
235:         explosionColor: {r: 0.9, g: 0.05, b: 0.08, a: 0.45}
236:         startInPhase2ForDebug: 0
237:         useFastPatternCooldownsForDebug: 0
238:         fastPatternCooldown: 1
239:       stomachPattern:
240:         phase2HealthRatio: 0.65
241:         phase1AttackDamage: 3
242:         phase1MoveSpeed: 0.5
243:         phase2AttackDamage: 3
244:         phase2MoveSpeed: 1
245:         phase1MeleeRange: 1.6
246:         phase1MeleeCooldown: 1.7
247:         chargeSpeed: 4
248:         chargeCooldown: 5
249:         chargeDamage: 4
250:         chargeKnockbackDistance: 2
251:         chargeWindup: 0.35
252:         chargeDuration: 1.2
253:         chargeHitRadius: 1.25
254:         phase2PatternInterval: 3
255:         phase2PreferredDistance: 4
256:         roamRadius: 9
257:         acidRange: 9
258:         acidDamage: 2
259:         acidDebuffDuration: 5
260:         acidTickInterval: 1
261:         acidTickDamage: 1
262:         acidProjectileTravelTime: 0.55
263:         acidArcHeight: 0.8
264:         acidProjectileScale: 0.9
265:         acidHitRadius: 1.15
266:         suctionRange: 4.2
267:         suctionDuration: 0.4
268:         suctionStopDistance: 1
269:         spitDelay: 0.35
270:         spitReturnDuration: 0.18
271:         shortRangeDamage: 5
272:         shortRangeRadius: 2.2
273:         suctionAttackSprites: []
274:         suctionAttackAnimationSpeed: 0.08
275:         acidColor: {r: 0.55, g: 0.95, b: 0.15, a: 0.95}
276:         acidSplashColor: {r: 0.5, g: 0.9, b: 0.12, a: 0.42}
277:         suctionColor: {r: 0.75, g: 0.95, b: 0.45, a: 0.42}
278:         leakColor: {r: 0.38, g: 0.85, b: 0.18, a: 0.38}
279:         startInPhase2ForDebug: 0
280:         useFastPatternCooldownsForDebug: 0
281:         fastPatternCooldown: 1
282:       lungPattern:
283:         phase1BrotherHealth: 40
284:         phase1AttackDamage: 2
285:         phase1MoveSpeed: 1
286:         windGustMoveSpeed: 3
287:         windGustSpeedMultiplier: 3
288:         windGustInterval: 5
289:         windGustDuration: 1.25
290:         brotherSpacing: 3.2
291:         phase1GasRange: 7
292:         phase1GasCooldown: 3.5
293:         phase1GasKnockback: 1
294:         phase2MaxHealth: 100
295:         phase2AttackDamage: 4
296:         phase2MoveSpeed: 1.5
297:         phase2GasRange: 10
298:         phase2GasCooldown: 4
299:         phase2GasKnockback: 1.2
300:         diveCooldown: 8
301:         diveWindup: 0.55
302:         diveTravelTime: 0.65
303:         diveArcHeight: 4
304:         diveLandingRadius: 1.45
305:         diveLandingDamage: 3
306:         diveKnockback: 1.4
307:         gasProjectileSpeed: 8
308:         gasProjectileRadius: 0.7
309:         gasProjectileScale: 1
310:         gasWindup: 0.25
311:         gasProjectileHeightOffset: 2
312:         phase1ContactDamage: 1
313:         phase2ContactDamage: 2
314:         contactRadius: 1.05
315:         contactCooldown: 1.1
316:         phase1PreferredDistance: 5.2
317:         phase2PreferredDistance: 4.2
318:         roamRadius: 10
319:         gasColor: {r: 0.72, g: 0.95, b: 0.9, a: 0.72}
320:         windColor: {r: 0.78, g: 0.9, b: 1, a: 0.34}
321:         diveColor: {r: 0.5, g: 0.85, b: 1, a: 0.42}
322:         startInPhase2ForDebug: 0
323:         useFastPatternCooldownsForDebug: 0
324:         fastPatternCooldown: 1
```

## Assets/_Project/Data/BiomeConfigs/LungEnemySpawnConfig.asset

```yaml
4: MonoBehaviour:
15:   normalKillsPerElite: 10
16:   enemySpawnRules:
17:   - monsterDefinition: {fileID: 11400000, guid: 499aed563f6cc4da79eaad32bd62751b, type: 2}
18:     name: LungBreathBubble
19:     density: 0.05
20:     minDistance: 6
21:     poissonSalt: 1610
22:     allowedRegions:
23:     maxAlive: 2
24:     activationRadius: 55
25:     respawnCooldown: 6
26:     spawnRadius: 2
27:     moveSpeed: 0.8
28:     stoppingDistance: 0.1
29:     wanderRadius: 4
30:     chaseRadius: 10
31:     leashRadius: 14
32:     idleDelayRange: {x: 0.6, y: 1.4}
33:     maxHealth: 4
34:     attackDamage: 1
35:     attackRange: 7
36:     attackCooldown: 1.5
37:     expReward: 15
38:     enableContactDamage: 1
39:     contactDamage: 1
40:     contactKnockbackDistance: 0.45
41:     additionalBaseStats: []
42:     separationDistance: 1.3
43:     separationStrength: 1
44:     heightOffset: 0
45:     scale: {x: 1, y: 1, z: 1}
46:     sortingOrder: 1000
47:     useBillboard: 1
48:     useYSort: 1
49:     animationSpeed: 0.16
50:     addCollider: 1
51:     isTrigger: 0
52:     colliderSize: {x: 0.85, y: 1.2, z: 0.85}
53:     colliderCenter: {x: 0, y: 0.6, z: 0}
54:     idleSprites:
59:     idleSpritesUp: []
60:     idleSpritesDown: []
61:     moveSprites:
66:     moveSpritesUp: []
67:     moveSpritesDown: []
68:     attackSprites:
73:     attackSpritesUp: []
74:     attackSpritesDown: []
75:     attackAnimationSpeed: 0.14
76:     isRanged: 1
77:     projectileSpeed: 6
78:     projectileLifeTime: 3
79:     projectileSprite: {fileID: 0}
80:     projectileScale: {x: 0.3, y: 0.3, z: 0.3}
81:     projectileSpawnOffset: 0.5
82:     expandColliderOnAttack: 0
83:     attackColliderSize: {x: 2, y: 2, z: 2}
84:     attackColliderCenter: {x: 0, y: 0.45, z: 0}
85:     deathSprites:
90:     deathAnimationSpeed: 0.16
91:     isElite: 0
92:     tintColor: {r: 1, g: 1, b: 1, a: 1}
93:     killTriggerEnemyName:
94:     killTriggerCount: 5
95:     splitsOnDeath: 0
96:     splitCount: 2
97:     splitEnemyName:
98:     splitVfxSprites: []
99:     splitVfxScale: 3
100:     splitVfxSpeed: 0.08
101:     splitVfxDuration: 0.6
102:     chargesAtPlayer: 0
103:     chargeSpeed: 6
104:     chargeAccelTime: 0.3
105:     leavesDebrisOnDeath: 0
106:     debrisDuration: 5
107:     debrisAggroRadius: 8
108:     debrisVfxSprites: []
109:     debrisVfxScale: 15
110:     debrisVfxSpeed: 0.12
111:   - monsterDefinition: {fileID: 11400000, guid: 09c64a797ef7441ad977d9749ce1b1ab, type: 2}
112:     name: LungCiliaBrush
113:     density: 0.05
114:     minDistance: 6
115:     poissonSalt: 1611
116:     allowedRegions:
117:     maxAlive: 2
118:     activationRadius: 55
119:     respawnCooldown: 6
120:     spawnRadius: 2
121:     moveSpeed: 1.1
122:     stoppingDistance: 0.1
123:     wanderRadius: 5
124:     chaseRadius: 10
125:     leashRadius: 14
126:     idleDelayRange: {x: 0.4, y: 1.1}
127:     maxHealth: 6
128:     attackDamage: 1
129:     attackRange: 1.7
130:     attackCooldown: 1
131:     expReward: 10
132:     enableContactDamage: 1
133:     contactDamage: 1
134:     contactKnockbackDistance: 0.45
135:     additionalBaseStats: []
136:     separationDistance: 1.05
137:     separationStrength: 1.2
138:     heightOffset: 0
139:     scale: {x: 1, y: 1, z: 1}
140:     sortingOrder: 1000
141:     useBillboard: 1
142:     useYSort: 1
143:     animationSpeed: 0.16
144:     addCollider: 1
145:     isTrigger: 0
146:     colliderSize: {x: 0.85, y: 1.2, z: 0.85}
147:     colliderCenter: {x: 0, y: 0.6, z: 0}
148:     idleSprites:
153:     idleSpritesUp: []
154:     idleSpritesDown: []
155:     moveSprites:
160:     moveSpritesUp: []
161:     moveSpritesDown: []
162:     attackSprites:
167:     attackSpritesUp: []
168:     attackSpritesDown: []
169:     attackAnimationSpeed: 0.14
170:     isRanged: 0
171:     projectileSpeed: 6
172:     projectileLifeTime: 3
173:     projectileSprite: {fileID: 0}
174:     projectileScale: {x: 0.3, y: 0.3, z: 0.3}
175:     projectileSpawnOffset: 0.5
176:     expandColliderOnAttack: 0
177:     attackColliderSize: {x: 0.65, y: 0.95, z: 0.65}
178:     attackColliderCenter: {x: 0, y: 0.48, z: 0}
179:     deathSprites:
184:     deathAnimationSpeed: 0.16
185:     isElite: 0
186:     tintColor: {r: 1, g: 1, b: 1, a: 1}
187:     killTriggerEnemyName:
188:     killTriggerCount: 5
189:     splitsOnDeath: 0
190:     splitCount: 2
191:     splitEnemyName:
192:     splitVfxSprites: []
193:     splitVfxScale: 3
194:     splitVfxSpeed: 0.08
195:     splitVfxDuration: 0.6
196:     chargesAtPlayer: 0
197:     chargeSpeed: 6
198:     chargeAccelTime: 0.3
199:     leavesDebrisOnDeath: 0
200:     debrisDuration: 5
201:     debrisAggroRadius: 8
202:     debrisVfxSprites: []
203:     debrisVfxScale: 15
204:     debrisVfxSpeed: 0.12
205:   - monsterDefinition: {fileID: 11400000, guid: 643bd1d9f29654393b97f17d249d2599, type: 2}
206:     name: LungOverinflatedAlveoli
207:     density: 0.05
208:     minDistance: 6
209:     poissonSalt: 1612
210:     allowedRegions:
211:     maxAlive: 2
212:     activationRadius: 55
213:     respawnCooldown: 6
214:     spawnRadius: 2
215:     moveSpeed: 0.8
216:     stoppingDistance: 0.1
217:     wanderRadius: 4
218:     chaseRadius: 10
219:     leashRadius: 14
220:     idleDelayRange: {x: 0.6, y: 1.4}
221:     maxHealth: 6
222:     attackDamage: 1
223:     attackRange: 6.5
224:     attackCooldown: 1.8
225:     expReward: 15
226:     enableContactDamage: 1
227:     contactDamage: 1
228:     contactKnockbackDistance: 0.45
229:     additionalBaseStats: []
230:     separationDistance: 1.3
231:     separationStrength: 1
232:     heightOffset: 0
233:     scale: {x: 1, y: 1, z: 1}
234:     sortingOrder: 1000
235:     useBillboard: 1
236:     useYSort: 1
237:     animationSpeed: 0.16
238:     addCollider: 1
239:     isTrigger: 0
240:     colliderSize: {x: 0.85, y: 1.2, z: 0.85}
241:     colliderCenter: {x: 0, y: 0.6, z: 0}
242:     idleSprites:
247:     idleSpritesUp: []
248:     idleSpritesDown: []
249:     moveSprites:
254:     moveSpritesUp: []
255:     moveSpritesDown: []
256:     attackSprites:
261:     attackSpritesUp: []
262:     attackSpritesDown: []
263:     attackAnimationSpeed: 0.14
264:     isRanged: 1
265:     projectileSpeed: 6
266:     projectileLifeTime: 3
267:     projectileSprite: {fileID: 0}
268:     projectileScale: {x: 0.3, y: 0.3, z: 0.3}
269:     projectileSpawnOffset: 0.5
270:     expandColliderOnAttack: 0
271:     attackColliderSize: {x: 2, y: 2, z: 2}
272:     attackColliderCenter: {x: 0, y: 0.45, z: 0}
273:     deathSprites:
278:     deathAnimationSpeed: 0.16
279:     isElite: 0
280:     tintColor: {r: 1, g: 1, b: 1, a: 1}
281:     killTriggerEnemyName:
282:     killTriggerCount: 5
283:     splitsOnDeath: 0
284:     splitCount: 2
285:     splitEnemyName:
286:     splitVfxSprites: []
287:     splitVfxScale: 3
288:     splitVfxSpeed: 0.08
289:     splitVfxDuration: 0.6
290:     chargesAtPlayer: 0
291:     chargeSpeed: 6
292:     chargeAccelTime: 0.3
293:     leavesDebrisOnDeath: 0
294:     debrisDuration: 5
295:     debrisAggroRadius: 8
296:     debrisVfxSprites: []
297:     debrisVfxScale: 15
298:     debrisVfxSpeed: 0.12
```

## Assets/_Project/Data/BiomeConfigs/StomachBiomeConfig.asset

```yaml
4: MonoBehaviour:
15:   biomeType: 3
16:   regionCellSize: 30
17:   regionBlendWidth: 4
18:   detailNoiseScale: 0.02
19:   regions:
20:   - name: Stomach
21:     baseHeight: 0
22:     primaryTile: {fileID: 11400000, guid: b1b2c3d4e5f6a7b8c9d0e1f2a3b4c501, type: 2}
23:     primaryType: 1
24:     variantTile: {fileID: 11400000, guid: b1b2c3d4e5f6a7b8c9d0e1f2a3b4c503, type: 2}
25:     variantType: 2
26:     variantThreshold: 0.92
27:     tileVariants:
36:   - name: StomachRock
37:     baseHeight: 1
38:     primaryTile: {fileID: 11400000, guid: b1b2c3d4e5f6a7b8c9d0e1f2a3b4c502, type: 2}
39:     primaryType: 1
40:     variantTile: {fileID: 11400000, guid: b1b2c3d4e5f6a7b8c9d0e1f2a3b4c504, type: 2}
41:     variantType: 2
42:     variantThreshold: 0.92
43:     tileVariants:
52:   - name: RockFloor
53:     baseHeight: -1
54:     primaryTile: {fileID: 11400000, guid: b1b2c3d4e5f6a7b8c9d0e1f2a3b4c503, type: 2}
55:     primaryType: 1
56:     variantTile: {fileID: 11400000, guid: b1b2c3d4e5f6a7b8c9d0e1f2a3b4c501, type: 2}
57:     variantType: 2
58:     variantThreshold: 0.8
59:     tileVariants: []
60:   heightNoiseScale: 0.02
61:   heightNoiseAmplitude: 0.4
62:   tileMappings:
63:   - tileType: 1
64:     tile: {fileID: 11400000, guid: b1b2c3d4e5f6a7b8c9d0e1f2a3b4c501, type: 2}
65:   exteriorBackdrop:
66:     enabled: 0
67:     sprite: {fileID: 0}
68:     tint: {r: 1, g: 1, b: 1, a: 1}
69:     opacity: 0.82
70:     cameraClearColor: {r: 0.035, g: 0.015, b: 0.018, a: 1}
71:     cameraDistance: 50
72:     overscan: 1.2
73:     driftAmount: 0.015
74:     driftSpeed: 0.08
75:     sortingOrder: -32000
76:   marginLeft: 5
77:   marginRight: 5
78:   marginBottom: 10
79:   marginTop: 5
80:   objectRules: []
81:   enemySpawnConfig: {fileID: 11400000, guid: c50840300fb540059cdca5d6a8e4fed9, type: 2}
82:   biomeEliteSpawnConfig: {fileID: 11400000, guid: 87e915bbf2edc486e81fdece0808036b, type: 2}
83:   enemySpawnRules: []
84:   bossArenaConfig: {fileID: 11400000, guid: 7c9112d9322b4ae1934c9e750e22d060, type: 2}
85:   midBossArena:
86:     enabled: 1
87:     onlyEnableOnLargeMaps: 1
88:     minimumMapWidth: 300
89:     minimumMapHeight: 300
90:     useCustomCenter: 0
91:     centerGrid: {x: 150, y: 150}
92:     arenaSize: {x: 32, y: 32}
93:     wallThicknessInCells: 1
94:     lockBoundaryInsetInCells: 1
95:     triggerInsetInCells: 2
96:     wallHeightOffset: 1.5
97:     triggerHeight: 4
98:     sortingOrder: 3500
99:     hideBossUntilEncounter: 1
100:     presentation:
101:       enabled: 1
102:       entranceSide: 0
103:       entranceWidthInCells: 4
104:       approachLengthInCells: 9
105:       pulseSpeed: 1.4
106:       approachOpacity: 0.72
107:       entranceSprite: {fileID: 0}
108:       entranceVisualScale: 1.35
109:       wallStraightSprites: []
110:       wallCornerSprite: {fileID: 0}
111:       wallCornerSpanInCells: 2
112:       wallCornerBaseYaw: 270
113:       wallUseStraightCornerConnectors: 0
114:       floorSortingOrder: 40
115:       gateSortingOrder: 3490
116:       useBiomePalette: 1
117:       primaryColor: {r: 0.43, g: 0.72, b: 0.22, a: 1}
118:       accentColor: {r: 0.82, g: 0.48, b: 0.16, a: 1}
119:       lockedColor: {r: 0.92, g: 0.12, b: 0.08, a: 1}
120:       clearedColor: {r: 0.34, g: 0.32, b: 0.3, a: 1}
121:     returnPortalSprite: {fileID: 0}
122:     returnPortalScale: {x: 1, y: 1, z: 1}
123:     boss:
124:       displayName: MidBoss
125:       useCustomBossRule: 0
126:       bossRule:
127:         monsterDefinition: {fileID: 0}
128:         name: Enemy
129:         density: 0.0025
130:         minDistance: 8
131:         poissonSalt: 400
132:         allowedRegions: 
133:         maxAlive: 1
134:         activationRadius: 20
135:         respawnCooldown: 8
136:         spawnRadius: 1.5
137:         moveSpeed: 1.5
138:         stoppingDistance: 0.1
139:         wanderRadius: 4
140:         chaseRadius: 6
141:         leashRadius: 8
142:         idleDelayRange: {x: 0.5, y: 1.5}
143:         maxHealth: 30
144:         attackDamage: 1
145:         attackRange: 1.5
146:         attackCooldown: 1
147:         expReward: 10
148:         enableContactDamage: 1
149:         contactDamage: 1
150:         contactKnockbackDistance: 0.45
151:         additionalBaseStats: []
152:         separationDistance: 1.1
153:         separationStrength: 1
154:         heightOffset: 0
155:         scale: {x: 1, y: 1, z: 1}
156:         sortingOrder: 1000
157:         useBillboard: 1
158:         useYSort: 1
159:         animationSpeed: 0.15
160:         addCollider: 1
161:         isTrigger: 0
162:         colliderSize: {x: 0.7, y: 1.1, z: 0.7}
163:         colliderCenter: {x: 0, y: 0.55, z: 0}
164:         idleSprites: []
165:         idleSpritesUp: []
166:         idleSpritesDown: []
167:         moveSprites: []
168:         moveSpritesUp: []
169:         moveSpritesDown: []
170:         attackSprites: []
171:         attackSpritesUp: []
172:         attackSpritesDown: []
173:         attackAnimationSpeed: 0.12
174:         isRanged: 0
175:         projectileSpeed: 8
176:         projectileLifeTime: 3
177:         projectileSprite: {fileID: 0}
178:         projectileScale: {x: 0.4, y: 0.4, z: 0.4}
179:         projectileSpawnOffset: 0.5
180:         expandColliderOnAttack: 0
181:         attackColliderSize: {x: 2, y: 2, z: 2}
182:         attackColliderCenter: {x: 0, y: 0.55, z: 0}
183:         deathSprites: []
184:         deathAnimationSpeed: 0.15
185:         isElite: 0
186:         tintColor: {r: 1, g: 1, b: 1, a: 1}
187:         killTriggerEnemyName: 
188:         killTriggerCount: 10
189:         splitsOnDeath: 0
190:         splitCount: 2
191:         splitEnemyName: 
192:         splitVfxSprites: []
193:         splitVfxScale: 3
194:         splitVfxSpeed: 0.08
195:         splitVfxDuration: 0.6
196:         chargesAtPlayer: 0
197:         chargeSpeed: 6
198:         chargeAccelTime: 0.3
199:         leavesDebrisOnDeath: 0
200:         debrisDuration: 5
201:         debrisAggroRadius: 8
202:         debrisVfxSprites: []
203:         debrisVfxScale: 15
204:         debrisVfxSpeed: 0.12
205:       useEnemyRuleFallback: 1
206:       fallbackEnemyRuleIndex: 0
207:       patternType: 0
208:       overrideStats: 0
209:       maxHealthMultiplier: 1
210:       attackDamageMultiplier: 1
211:       moveSpeedMultiplier: 1
212:       scaleMultiplier: {x: 1, y: 1, z: 1}
213:       minimumMaxHealth: 0
214:       intestinePattern:
215:         phase2HealthRatio: 0.5
216:         phase1FleeDistance: 7
217:         phase1RoamRadius: 8
218:         phase1MoveMultiplier: 1
219:         phase2PreferredDistance: 2.5
220:         phase2MoveMultiplier: 1.15
221:         dungPreDelay: 0.5
222:         phase1DungCooldown: 8
223:         phase2ThrowCooldown: 5
224:         dungCollisionDamage: 1
225:         dungSlowRatio: 0.2
226:         dungSlowDuration: 3
227:         dungHazardRadius: 0.75
228:         dungLifeTime: 10
229:         throwTravelTime: 0.7
230:         throwArcHeight: 2.5
231:         parasiteMinCount: 2
232:         parasiteMaxCount: 3
233:         parasiteSpawnDelay: 0.45
234:         parasiteSpawnRadius: 1.1
235:         parasiteDefinition: {fileID: 0}
236:         parasiteMaxHealth: 6
237:         parasiteMoveSpeed: 2.3
238:         parasiteAttackDamage: 1
239:         parasiteAttackRange: 1.1
240:         parasiteAttackCooldown: 1.3
241:         parasiteScale: {x: 0.55, y: 0.55, z: 0.55}
242:         stompMinDelay: 7
243:         stompPreDelay: 0.55
244:         stompRadius: 6
245:         stompDamage: 2
246:         stompSlowRatio: 0.3
247:         stompSlowDuration: 3
248:         dungColor: {r: 0.34, g: 0.18, b: 0.08, a: 0.95}
249:         parasiteColor: {r: 0.62, g: 0.88, b: 0.36, a: 1}
250:         shockwaveColor: {r: 0.65, g: 0.38, b: 0.18, a: 0.45}
251:         startInPhase2ForDebug: 0
252:         useFastPatternCooldownsForDebug: 0
253:         fastPatternCooldown: 1
254:       liverPattern:
255:         phase2HealthRatio: 0.5
256:         phase1AttackDamage: 2
257:         phase1MoveSpeed: 1
258:         phase2AttackDamage: 2
259:         phase2MoveSpeed: 1.5
260:         bloodBombRange: 7
261:         bloodBombExplosionRadius: 2.5
262:         bloodBombFuseTime: 1
263:         bloodBombCooldown: 4
264:         attackPowerDebuffRatio: 0.15
265:         attackPowerDebuffDuration: 5
266:         bloodBombArcHeight: 1.4
267:         bloodBombProjectileScale: 1.15
268:         bloodBombSprite: {fileID: 0}
269:         healingPosePreDelay: 1
270:         healingPoseCooldown: 15
271:         healingPoseDuration: 5
272:         damageHealRatio: 0.7
273:         healingPoseLowerSprites: []
274:         healingPoseLowerSpritesDown: []
275:         healingPoseRaiseSprites: []
276:         healingPoseRaiseSpritesDown: []
277:         healingPoseAnimationSpeed: 0.12
278:         phase1PreferredDistanceRatio: 0.72
279:         phase2PreferredDistance: 2.6
280:         roamRadius: 9
281:         bloodBombColor: {r: 0.7, g: 0, b: 0.04, a: 0.96}
282:         explosionColor: {r: 0.9, g: 0.05, b: 0.08, a: 0.45}
283:         startInPhase2ForDebug: 0
284:         useFastPatternCooldownsForDebug: 0
285:         fastPatternCooldown: 1
286:       stomachPattern:
287:         phase2HealthRatio: 0.65
288:         phase1AttackDamage: 3
289:         phase1MoveSpeed: 0.5
290:         phase2AttackDamage: 3
291:         phase2MoveSpeed: 1
292:         phase1MeleeRange: 1.6
293:         phase1MeleeCooldown: 1.7
294:         chargeSpeed: 4
295:         chargeCooldown: 5
296:         chargeDamage: 4
297:         chargeKnockbackDistance: 2
298:         chargeWindup: 0.35
299:         chargeDuration: 1.2
300:         chargeHitRadius: 1.25
301:         phase2PatternInterval: 3
302:         phase2PreferredDistance: 4
303:         roamRadius: 9
304:         acidRange: 9
305:         acidDamage: 2
306:         acidDebuffDuration: 5
307:         acidTickInterval: 1
308:         acidTickDamage: 1
309:         acidProjectileTravelTime: 0.55
310:         acidArcHeight: 0.8
311:         acidProjectileScale: 0.9
312:         acidHitRadius: 1.15
313:         suctionRange: 4.2
314:         suctionDuration: 0.4
315:         suctionStopDistance: 1
316:         spitDelay: 0.35
317:         spitReturnDuration: 0.18
318:         shortRangeDamage: 5
319:         shortRangeRadius: 2.2
320:         suctionAttackSprites: []
321:         suctionAttackAnimationSpeed: 0.08
322:         acidColor: {r: 0.55, g: 0.95, b: 0.15, a: 0.95}
323:         acidSplashColor: {r: 0.5, g: 0.9, b: 0.12, a: 0.42}
324:         suctionColor: {r: 0.75, g: 0.95, b: 0.45, a: 0.42}
325:         leakColor: {r: 0.38, g: 0.85, b: 0.18, a: 0.38}
326:         startInPhase2ForDebug: 0
327:         useFastPatternCooldownsForDebug: 0
328:         fastPatternCooldown: 1
329:       lungPattern:
330:         phase1BrotherHealth: 40
331:         phase1AttackDamage: 2
332:         phase1MoveSpeed: 1
333:         windGustMoveSpeed: 3
334:         windGustSpeedMultiplier: 3
335:         windGustInterval: 5
336:         windGustDuration: 1.25
337:         brotherSpacing: 3.2
338:         phase1GasRange: 7
339:         phase1GasCooldown: 3.5
340:         phase1GasKnockback: 1
341:         phase2MaxHealth: 100
342:         phase2AttackDamage: 4
343:         phase2MoveSpeed: 1.5
344:         phase2GasRange: 10
345:         phase2GasCooldown: 4
346:         phase2GasKnockback: 1.2
347:         diveCooldown: 8
348:         diveWindup: 0.55
349:         diveTravelTime: 0.65
350:         diveArcHeight: 4
351:         diveLandingRadius: 1.45
352:         diveLandingDamage: 3
353:         diveKnockback: 1.4
354:         gasProjectileSpeed: 8
355:         gasProjectileRadius: 0.7
356:         gasProjectileScale: 1
357:         gasWindup: 0.25
358:         gasProjectileHeightOffset: 2
359:         phase1ContactDamage: 1
360:         phase2ContactDamage: 2
361:         contactRadius: 1.05
362:         contactCooldown: 1.1
363:         phase1PreferredDistance: 5.2
364:         phase2PreferredDistance: 4.2
365:         roamRadius: 10
366:         gasColor: {r: 0.72, g: 0.95, b: 0.9, a: 0.72}
367:         windColor: {r: 0.78, g: 0.9, b: 1, a: 0.34}
368:         diveColor: {r: 0.5, g: 0.85, b: 1, a: 0.42}
369:         startInPhase2ForDebug: 0
370:         useFastPatternCooldownsForDebug: 0
371:         fastPatternCooldown: 1
372:   returnPortal:
373:     enabled: 1
374:     name: ReturnPortal
375:     poolKind: 100
376:     sprite: {fileID: -898939165, guid: fbc2f259a8b301f42933cd4689e5842c, type: 3}
377:     sortingOrder: 1000
378:     useCustomPosition: 1
379:     gridPosition: {x: 45, y: 4}
380:     heightOffset: 0
381:     useBillboard: 1
382:     scale: {x: 0.5, y: 0.5, z: 0.5}
383:     addCollider: 1
384:     isTrigger: 1
385:     colliderSize: {x: 2, y: 2, z: 2}
386:     colliderCenter: {x: 0, y: 0, z: 0}
```

## Assets/_Project/Data/BiomeConfigs/StomachBossArenaConfig.asset

```yaml
4: MonoBehaviour:
15:   midBossArena:
16:     enabled: 1
17:     onlyEnableOnLargeMaps: 0
18:     minimumMapWidth: 300
19:     minimumMapHeight: 300
20:     useCustomCenter: 0
21:     centerGrid: {x: 150, y: 150}
22:     arenaSize: {x: 32, y: 32}
23:     wallThicknessInCells: 1
24:     lockBoundaryInsetInCells: 0
25:     triggerInsetInCells: 2
26:     wallHeightOffset: 1.5
27:     triggerHeight: 4
28:     sortingOrder: 3500
29:     hideBossUntilEncounter: 1
30:     presentation:
31:       enabled: 1
32:       entranceSide: 0
33:       entranceWidthInCells: 5
34:       approachLengthInCells: 8
35:       pulseSpeed: 1.05
36:       approachOpacity: 0.78
37:       entranceSprite: {fileID: 21300000, guid: 903413b7fccbe4f3386e1b7d47881e26, type: 3}
38:       entranceVisualScale: 1.5
39:       wallStraightSprites:
43:       wallCornerSprite: {fileID: 1403090, guid: af8974ace7d914da4a72936cde31496a, type: 3}
44:       wallCornerSpanInCells: 2
45:       wallCornerBaseYaw: 270
46:       wallUseStraightCornerConnectors: 0
47:       floorSortingOrder: 40
48:       gateSortingOrder: 3490
49:       useBiomePalette: 1
50:       primaryColor: {r: 0.43, g: 0.72, b: 0.22, a: 1}
51:       accentColor: {r: 0.82, g: 0.48, b: 0.16, a: 1}
52:       lockedColor: {r: 0.92, g: 0.12, b: 0.08, a: 1}
53:       clearedColor: {r: 0.34, g: 0.32, b: 0.3, a: 1}
54:     returnPortalSprite: {fileID: 0}
55:     returnPortalScale: {x: 1, y: 1, z: 1}
56:     boss:
57:       displayName: StomachBoss
58:       useCustomBossRule: 1
59:       bossRule:
60:         monsterDefinition: {fileID: 11400000, guid: a3cc0c00447f347f2b324b82b8e9dc41, type: 2}
61:         name: StomachBoss
62:         density: 0
63:         minDistance: 8
64:         poissonSalt: 7203
65:         allowedRegions: 
66:         maxAlive: 1
67:         activationRadius: 0
68:         respawnCooldown: 0
69:         spawnRadius: 0
70:         moveSpeed: 0.5
71:         stoppingDistance: 0.1
72:         wanderRadius: 4
73:         chaseRadius: 16
74:         leashRadius: 24
75:         idleDelayRange: {x: 0.5, y: 1.2}
76:         maxHealth: 10
77:         attackDamage: 3
78:         attackRange: 1.6
79:         attackCooldown: 1
80:         expReward: 0
81:         enableContactDamage: 1
82:         contactDamage: 1
83:         contactKnockbackDistance: 0.45
84:         additionalBaseStats: []
85:         separationDistance: 0
86:         separationStrength: 0
87:         heightOffset: 0
88:         scale: {x: 1.1, y: 1.1, z: 1.1}
89:         sortingOrder: 1000
90:         useBillboard: 1
91:         useYSort: 1
92:         animationSpeed: 0.14
93:         addCollider: 1
94:         isTrigger: 0
95:         colliderSize: {x: 0.8, y: 1.15, z: 0.8}
96:         colliderCenter: {x: 0, y: 0.58, z: 0}
97:         idleSprites:
102:         idleSpritesUp:
107:         idleSpritesDown:
112:         moveSprites:
117:         moveSpritesUp:
122:         moveSpritesDown:
131:         attackSprites:
138:         attackSpritesUp: []
139:         attackSpritesDown: []
140:         attackAnimationSpeed: 0.1
141:         isRanged: 0
142:         projectileSpeed: 8
143:         projectileLifeTime: 3
144:         projectileSprite: {fileID: 0}
145:         projectileScale: {x: 0.4, y: 0.4, z: 0.4}
146:         projectileSpawnOffset: 0.5
147:         expandColliderOnAttack: 0
148:         attackColliderSize: {x: 2, y: 2, z: 2}
149:         attackColliderCenter: {x: 0, y: 0.55, z: 0}
150:         deathSprites:
155:         deathAnimationSpeed: 0.15
156:         isElite: 0
157:         tintColor: {r: 1, g: 1, b: 1, a: 1}
158:         killTriggerEnemyName: 
159:         killTriggerCount: 0
160:         splitsOnDeath: 0
161:         splitCount: 0
162:         splitEnemyName: 
163:         splitVfxSprites: []
164:         splitVfxScale: 3
165:         splitVfxSpeed: 0.08
166:         splitVfxDuration: 0.6
167:         chargesAtPlayer: 0
168:         chargeSpeed: 6
169:         chargeAccelTime: 0.3
170:         leavesDebrisOnDeath: 0
171:         debrisDuration: 5
172:         debrisAggroRadius: 8
173:         debrisVfxSprites: []
174:         debrisVfxScale: 15
175:         debrisVfxSpeed: 0.12
176:       useEnemyRuleFallback: 1
177:       fallbackEnemyRuleIndex: 0
178:       patternType: 4
179:       overrideStats: 0
180:       maxHealthMultiplier: 1
181:       attackDamageMultiplier: 1
182:       moveSpeedMultiplier: 1
183:       scaleMultiplier: {x: 4, y: 4, z: 4}
184:       minimumMaxHealth: 0
185:       intestinePattern:
186:         phase2HealthRatio: 0.5
187:         phase1FleeDistance: 7
188:         phase1RoamRadius: 8
189:         phase1MoveMultiplier: 1
190:         phase2PreferredDistance: 2.5
191:         phase2MoveMultiplier: 1.15
192:         dungPreDelay: 0.5
193:         phase1DungCooldown: 8
194:         phase2ThrowCooldown: 5
195:         dungCollisionDamage: 1
196:         dungSlowRatio: 0.2
197:         dungSlowDuration: 3
198:         dungHazardRadius: 0.75
199:         dungLifeTime: 10
200:         throwTravelTime: 0.7
201:         throwArcHeight: 2.5
202:         parasiteMinCount: 2
203:         parasiteMaxCount: 3
204:         parasiteSpawnDelay: 0.45
205:         parasiteSpawnRadius: 1.1
206:         parasiteDefinition: {fileID: 0}
207:         parasiteMaxHealth: 6
208:         parasiteMoveSpeed: 2.3
209:         parasiteAttackDamage: 1
210:         parasiteAttackRange: 1.1
211:         parasiteAttackCooldown: 1.3
212:         parasiteScale: {x: 0.55, y: 0.55, z: 0.55}
213:         stompMinDelay: 7
214:         stompPreDelay: 0.55
215:         stompRadius: 6
216:         stompDamage: 2
217:         stompSlowRatio: 0.3
218:         stompSlowDuration: 3
219:         dungColor: {r: 0.34, g: 0.18, b: 0.08, a: 0.95}
220:         parasiteColor: {r: 0.62, g: 0.88, b: 0.36, a: 1}
221:         shockwaveColor: {r: 0.65, g: 0.38, b: 0.18, a: 0.45}
222:         startInPhase2ForDebug: 0
223:         useFastPatternCooldownsForDebug: 0
224:         fastPatternCooldown: 1
225:       liverPattern:
226:         phase2HealthRatio: 0.5
227:         phase1AttackDamage: 2
228:         phase1MoveSpeed: 1
229:         phase2AttackDamage: 2
230:         phase2MoveSpeed: 1.5
231:         bloodBombRange: 7
232:         bloodBombExplosionRadius: 2.5
233:         bloodBombFuseTime: 1
234:         bloodBombCooldown: 4
235:         attackPowerDebuffRatio: 0.15
236:         attackPowerDebuffDuration: 5
237:         bloodBombArcHeight: 1.4
238:         bloodBombProjectileScale: 4
239:         bloodBombSprite: {fileID: 473467103, guid: a71f4732a4349ab45b4b1d038597c5f8, type: 3}
240:         healingPosePreDelay: 1
241:         healingPoseCooldown: 15
242:         healingPoseDuration: 5
243:         damageHealRatio: 0.7
244:         healingPoseLowerSprites: []
245:         healingPoseLowerSpritesDown: []
246:         healingPoseRaiseSprites: []
247:         healingPoseRaiseSpritesDown: []
248:         healingPoseAnimationSpeed: 0.12
249:         phase1PreferredDistanceRatio: 0.72
250:         phase2PreferredDistance: 2.6
251:         roamRadius: 9
252:         bloodBombColor: {r: 0.7, g: 0, b: 0.04, a: 0.96}
253:         explosionColor: {r: 0.9, g: 0.05, b: 0.08, a: 0.45}
254:         startInPhase2ForDebug: 0
255:         useFastPatternCooldownsForDebug: 0
256:         fastPatternCooldown: 1
257:       stomachPattern:
258:         phase2HealthRatio: 0.65
259:         phase1AttackDamage: 3
260:         phase1MoveSpeed: 0.5
261:         phase2AttackDamage: 3
262:         phase2MoveSpeed: 1
263:         phase1MeleeRange: 1.6
264:         phase1MeleeCooldown: 1.7
265:         chargeSpeed: 4
266:         chargeCooldown: 5
267:         chargeDamage: 4
268:         chargeKnockbackDistance: 2
269:         chargeWindup: 0.35
270:         chargeDuration: 1.2
271:         chargeHitRadius: 1.25
272:         phase2PatternInterval: 3
273:         phase2PreferredDistance: 4
274:         roamRadius: 9
275:         acidRange: 9
276:         acidDamage: 2
277:         acidDebuffDuration: 5
278:         acidTickInterval: 1
279:         acidTickDamage: 1
280:         acidProjectileTravelTime: 0.55
281:         acidArcHeight: 0.8
282:         acidProjectileScale: 0.9
283:         acidHitRadius: 1.15
284:         suctionRange: 4.2
285:         suctionDuration: 0.4
286:         suctionStopDistance: 1
287:         spitDelay: 0.35
288:         spitReturnDuration: 0.18
289:         shortRangeDamage: 5
290:         shortRangeRadius: 2.2
291:         suctionAttackSprites:
304:         suctionAttackAnimationSpeed: 0.08
305:         acidColor: {r: 0.55, g: 0.95, b: 0.15, a: 0.95}
306:         acidSplashColor: {r: 0.5, g: 0.9, b: 0.12, a: 0.42}
307:         suctionColor: {r: 0.75, g: 0.95, b: 0.45, a: 0.42}
308:         leakColor: {r: 0.38, g: 0.85, b: 0.18, a: 0.38}
309:         startInPhase2ForDebug: 0
310:         useFastPatternCooldownsForDebug: 0
311:         fastPatternCooldown: 1
312:       lungPattern:
313:         phase1BrotherHealth: 40
314:         phase1AttackDamage: 2
315:         phase1MoveSpeed: 1
316:         windGustMoveSpeed: 3
317:         windGustSpeedMultiplier: 3
318:         windGustInterval: 5
319:         windGustDuration: 1.25
320:         brotherSpacing: 3.2
321:         phase1GasRange: 7
322:         phase1GasCooldown: 3.5
323:         phase1GasKnockback: 1
324:         phase2MaxHealth: 100
325:         phase2AttackDamage: 4
326:         phase2MoveSpeed: 1.5
327:         phase2GasRange: 10
328:         phase2GasCooldown: 4
329:         phase2GasKnockback: 1.2
330:         diveCooldown: 8
331:         diveWindup: 0.55
332:         diveTravelTime: 0.65
333:         diveArcHeight: 4
334:         diveLandingRadius: 1.45
335:         diveLandingDamage: 3
336:         diveKnockback: 1.4
337:         gasProjectileSpeed: 8
338:         gasProjectileRadius: 0.7
339:         gasProjectileScale: 1
340:         gasWindup: 0.25
341:         gasProjectileHeightOffset: 2
342:         phase1ContactDamage: 1
343:         phase2ContactDamage: 2
344:         contactRadius: 1.05
345:         contactCooldown: 1.1
346:         phase1PreferredDistance: 5.2
347:         phase2PreferredDistance: 4.2
348:         roamRadius: 10
349:         gasColor: {r: 0.72, g: 0.95, b: 0.9, a: 0.72}
350:         windColor: {r: 0.78, g: 0.9, b: 1, a: 0.34}
351:         diveColor: {r: 0.5, g: 0.85, b: 1, a: 0.42}
352:         startInPhase2ForDebug: 0
353:         useFastPatternCooldownsForDebug: 0
354:         fastPatternCooldown: 1
```

## Assets/_Project/Data/BiomeConfigs/StomachEnemySpawnConfig.asset

```yaml
4: MonoBehaviour:
15:   normalKillsPerElite: 10
16:   enemySpawnRules:
17:   - monsterDefinition: {fileID: 11400000, guid: ed14b06444759459aa397748f15059de, type: 2}
18:     name: StomachGulp
19:     density: 0.05
20:     minDistance: 6
21:     poissonSalt: 1601
22:     allowedRegions:
23:     maxAlive: 2
24:     activationRadius: 55
25:     respawnCooldown: 6
26:     spawnRadius: 2
27:     moveSpeed: 0.8
28:     stoppingDistance: 0.1
29:     wanderRadius: 4
30:     chaseRadius: 10
31:     leashRadius: 14
32:     idleDelayRange: {x: 0.6, y: 1.4}
33:     maxHealth: 4
34:     attackDamage: 1
35:     attackRange: 7
36:     attackCooldown: 1.5
37:     expReward: 15
38:     enableContactDamage: 1
39:     contactDamage: 1
40:     contactKnockbackDistance: 0.45
41:     additionalBaseStats: []
42:     separationDistance: 1.3
43:     separationStrength: 1
44:     heightOffset: 0
45:     scale: {x: 1, y: 1, z: 1}
46:     sortingOrder: 1000
47:     useBillboard: 1
48:     useYSort: 1
49:     animationSpeed: 0.16
50:     addCollider: 1
51:     isTrigger: 0
52:     colliderSize: {x: 0.85, y: 1.2, z: 0.85}
53:     colliderCenter: {x: 0, y: 0.6, z: 0}
54:     idleSprites:
59:     idleSpritesUp: []
60:     idleSpritesDown: []
61:     moveSprites:
66:     moveSpritesUp: []
67:     moveSpritesDown: []
68:     attackSprites:
73:     attackSpritesUp: []
74:     attackSpritesDown: []
75:     attackAnimationSpeed: 0.14
76:     isRanged: 1
77:     projectileSpeed: 6
78:     projectileLifeTime: 3
79:     projectileSprite: {fileID: 0}
80:     projectileScale: {x: 0.3, y: 0.3, z: 0.3}
81:     projectileSpawnOffset: 0.5
82:     expandColliderOnAttack: 0
83:     attackColliderSize: {x: 2, y: 2, z: 2}
84:     attackColliderCenter: {x: 0, y: 0.45, z: 0}
85:     deathSprites:
90:     deathAnimationSpeed: 0.16
91:     isElite: 0
92:     tintColor: {r: 1, g: 1, b: 1, a: 1}
93:     killTriggerEnemyName:
94:     killTriggerCount: 5
95:     splitsOnDeath: 0
96:     splitCount: 2
97:     splitEnemyName:
98:     splitVfxSprites: []
99:     splitVfxScale: 3
100:     splitVfxSpeed: 0.08
101:     splitVfxDuration: 0.6
102:     chargesAtPlayer: 0
103:     chargeSpeed: 6
104:     chargeAccelTime: 0.3
105:     leavesDebrisOnDeath: 0
106:     debrisDuration: 5
107:     debrisAggroRadius: 8
108:     debrisVfxSprites: []
109:     debrisVfxScale: 15
110:     debrisVfxSpeed: 0.12
111:   - monsterDefinition: {fileID: 11400000, guid: 5776acaa48a8544d2b6221fe5ca01aab, type: 2}
112:     name: StomachMucusSnail
113:     density: 0.05
114:     minDistance: 6
115:     poissonSalt: 1602
116:     allowedRegions:
117:     maxAlive: 2
118:     activationRadius: 55
119:     respawnCooldown: 6
120:     spawnRadius: 2
121:     moveSpeed: 0.8
122:     stoppingDistance: 0.1
123:     wanderRadius: 5
124:     chaseRadius: 10
125:     leashRadius: 14
126:     idleDelayRange: {x: 0.4, y: 1.1}
127:     maxHealth: 6
128:     attackDamage: 1
129:     attackRange: 1.5
130:     attackCooldown: 1.1
131:     expReward: 10
132:     enableContactDamage: 1
133:     contactDamage: 1
134:     contactKnockbackDistance: 0.45
135:     additionalBaseStats: []
136:     separationDistance: 1.05
137:     separationStrength: 1.2
138:     heightOffset: 0
139:     scale: {x: 1, y: 1, z: 1}
140:     sortingOrder: 1000
141:     useBillboard: 1
142:     useYSort: 1
143:     animationSpeed: 0.16
144:     addCollider: 1
145:     isTrigger: 0
146:     colliderSize: {x: 0.85, y: 1.2, z: 0.85}
147:     colliderCenter: {x: 0, y: 0.6, z: 0}
148:     idleSprites:
153:     idleSpritesUp: []
154:     idleSpritesDown: []
155:     moveSprites:
160:     moveSpritesUp: []
161:     moveSpritesDown: []
162:     attackSprites:
167:     attackSpritesUp: []
168:     attackSpritesDown: []
169:     attackAnimationSpeed: 0.14
170:     isRanged: 0
171:     projectileSpeed: 6
172:     projectileLifeTime: 3
173:     projectileSprite: {fileID: 0}
174:     projectileScale: {x: 0.3, y: 0.3, z: 0.3}
175:     projectileSpawnOffset: 0.5
176:     expandColliderOnAttack: 0
177:     attackColliderSize: {x: 0.65, y: 0.95, z: 0.65}
178:     attackColliderCenter: {x: 0, y: 0.48, z: 0}
179:     deathSprites:
184:     deathAnimationSpeed: 0.16
185:     isElite: 0
186:     tintColor: {r: 1, g: 1, b: 1, a: 1}
187:     killTriggerEnemyName:
188:     killTriggerCount: 5
189:     splitsOnDeath: 0
190:     splitCount: 2
191:     splitEnemyName:
192:     splitVfxSprites: []
193:     splitVfxScale: 3
194:     splitVfxSpeed: 0.08
195:     splitVfxDuration: 0.6
196:     chargesAtPlayer: 0
197:     chargeSpeed: 6
198:     chargeAccelTime: 0.3
199:     leavesDebrisOnDeath: 0
200:     debrisDuration: 5
201:     debrisAggroRadius: 8
202:     debrisVfxSprites: []
203:     debrisVfxScale: 15
204:     debrisVfxSpeed: 0.12
205:   - monsterDefinition: {fileID: 11400000, guid: 83507b86eb762412da73736941f79ecf, type: 2}
206:     name: StomachOverfedSac
207:     density: 0.05
208:     minDistance: 6
209:     poissonSalt: 1603
210:     allowedRegions:
211:     maxAlive: 2
212:     activationRadius: 55
213:     respawnCooldown: 6
214:     spawnRadius: 2
215:     moveSpeed: 0.8
216:     stoppingDistance: 0.1
217:     wanderRadius: 4
218:     chaseRadius: 10
219:     leashRadius: 14
220:     idleDelayRange: {x: 0.6, y: 1.4}
221:     maxHealth: 6
222:     attackDamage: 1
223:     attackRange: 6.5
224:     attackCooldown: 1.8
225:     expReward: 15
226:     enableContactDamage: 1
227:     contactDamage: 1
228:     contactKnockbackDistance: 0.45
229:     additionalBaseStats: []
230:     separationDistance: 1.3
231:     separationStrength: 1
232:     heightOffset: 0
233:     scale: {x: 1, y: 1, z: 1}
234:     sortingOrder: 1000
235:     useBillboard: 1
236:     useYSort: 1
237:     animationSpeed: 0.16
238:     addCollider: 1
239:     isTrigger: 0
240:     colliderSize: {x: 0.85, y: 1.2, z: 0.85}
241:     colliderCenter: {x: 0, y: 0.6, z: 0}
242:     idleSprites:
247:     idleSpritesUp: []
248:     idleSpritesDown: []
249:     moveSprites:
254:     moveSpritesUp: []
255:     moveSpritesDown: []
256:     attackSprites:
261:     attackSpritesUp: []
262:     attackSpritesDown: []
263:     attackAnimationSpeed: 0.14
264:     isRanged: 1
265:     projectileSpeed: 6
266:     projectileLifeTime: 3
267:     projectileSprite: {fileID: 0}
268:     projectileScale: {x: 0.3, y: 0.3, z: 0.3}
269:     projectileSpawnOffset: 0.5
270:     expandColliderOnAttack: 0
271:     attackColliderSize: {x: 2, y: 2, z: 2}
272:     attackColliderCenter: {x: 0, y: 0.45, z: 0}
273:     deathSprites:
278:     deathAnimationSpeed: 0.16
279:     isElite: 0
280:     tintColor: {r: 1, g: 1, b: 1, a: 1}
281:     killTriggerEnemyName:
282:     killTriggerCount: 5
283:     splitsOnDeath: 0
284:     splitCount: 2
285:     splitEnemyName:
286:     splitVfxSprites: []
287:     splitVfxScale: 3
288:     splitVfxSpeed: 0.08
289:     splitVfxDuration: 0.6
290:     chargesAtPlayer: 0
291:     chargeSpeed: 6
292:     chargeAccelTime: 0.3
293:     leavesDebrisOnDeath: 0
294:     debrisDuration: 5
295:     debrisAggroRadius: 8
296:     debrisVfxSprites: []
297:     debrisVfxScale: 15
298:     debrisVfxSpeed: 0.12
```

## Assets/_Project/Data/BiomeElites/IntestineBiomeEliteSpawnConfig.asset

```yaml
4: MonoBehaviour:
15:   enabled: 1
16:   monsters:
17:   - monsterDefinition: {fileID: 11400000, guid: c30ad02f9edec4b8e9166b2eeea4ce85, type: 2}
18:     name: I-01 GasSac
19:     density: 0.0025
20:     minDistance: 8
21:     poissonSalt: 400
22:     allowedRegions: 
23:     maxAlive: 1
24:     activationRadius: 20
25:     respawnCooldown: 8
26:     spawnRadius: 1.5
27:     moveSpeed: 1.5
28:     stoppingDistance: 0.2
29:     wanderRadius: 0
30:     chaseRadius: 7
31:     leashRadius: 16
32:     idleDelayRange: {x: 0.5, y: 1.5}
33:     maxHealth: 30
34:     attackDamage: 1
35:     attackRange: 1.5
36:     attackCooldown: 1
37:     expReward: 10
38:     enableContactDamage: 1
39:     contactDamage: 1
40:     contactKnockbackDistance: 0.45
41:     additionalBaseStats: []
42:     separationDistance: 1.1
43:     separationStrength: 1
44:     heightOffset: 0
45:     scale: {x: 1, y: 1, z: 1}
46:     sortingOrder: 1000
47:     useBillboard: 1
48:     useYSort: 1
49:     animationSpeed: 0.15
50:     addCollider: 1
51:     isTrigger: 1
52:     colliderSize: {x: 1.1, y: 1.25, z: 1.1}
53:     colliderCenter: {x: 0, y: 0.6, z: 0}
54:     idleSprites: []
55:     idleSpritesUp: []
56:     idleSpritesDown: []
57:     moveSprites: []
58:     moveSpritesUp: []
59:     moveSpritesDown: []
60:     attackSprites: []
61:     attackSpritesUp: []
62:     attackSpritesDown: []
63:     attackAnimationSpeed: 0.12
64:     isRanged: 0
65:     projectileSpeed: 8
66:     projectileLifeTime: 3
67:     projectileSprite: {fileID: 0}
68:     projectileScale: {x: 0.4, y: 0.4, z: 0.4}
69:     projectileSpawnOffset: 0.5
70:     expandColliderOnAttack: 0
71:     attackColliderSize: {x: 2, y: 2, z: 2}
72:     attackColliderCenter: {x: 0, y: 0.55, z: 0}
73:     deathSprites: []
74:     deathAnimationSpeed: 0.15
75:     isElite: 1
76:     tintColor: {r: 1, g: 1, b: 1, a: 1}
77:     killTriggerEnemyName: 
78:     killTriggerCount: 10
79:     splitsOnDeath: 0
80:     splitCount: 2
81:     splitEnemyName: 
82:     splitVfxSprites: []
83:     splitVfxScale: 3
84:     splitVfxSpeed: 0.08
85:     splitVfxDuration: 0.6
86:     chargesAtPlayer: 0
87:     chargeSpeed: 6
88:     chargeAccelTime: 0.3
89:     leavesDebrisOnDeath: 0
90:     debrisDuration: 5
91:     debrisAggroRadius: 8
92:     debrisVfxSprites: []
93:     debrisVfxScale: 15
94:     debrisVfxSpeed: 0.12
95:   - monsterDefinition: {fileID: 11400000, guid: 528841926549649778261e9115162e30, type: 2}
96:     name: I-02 Hardened Residue
97:     density: 0.0025
98:     minDistance: 8
99:     poissonSalt: 400
100:     allowedRegions: 
101:     maxAlive: 1
102:     activationRadius: 20
103:     respawnCooldown: 8
104:     spawnRadius: 1.5
105:     moveSpeed: 1.5
106:     stoppingDistance: 0.2
107:     wanderRadius: 0
108:     chaseRadius: 7
109:     leashRadius: 16
110:     idleDelayRange: {x: 0.5, y: 1.5}
111:     maxHealth: 30
112:     attackDamage: 1
113:     attackRange: 1.5
114:     attackCooldown: 1
115:     expReward: 10
116:     enableContactDamage: 1
117:     contactDamage: 1
118:     contactKnockbackDistance: 0.45
119:     additionalBaseStats: []
120:     separationDistance: 1.1
121:     separationStrength: 1
122:     heightOffset: 0
123:     scale: {x: 1, y: 1, z: 1}
124:     sortingOrder: 1000
125:     useBillboard: 1
126:     useYSort: 1
127:     animationSpeed: 0.15
128:     addCollider: 1
129:     isTrigger: 1
130:     colliderSize: {x: 1.6, y: 1.2, z: 0.95}
131:     colliderCenter: {x: 0, y: 0.55, z: 0}
132:     idleSprites: []
133:     idleSpritesUp: []
134:     idleSpritesDown: []
135:     moveSprites: []
136:     moveSpritesUp: []
137:     moveSpritesDown: []
138:     attackSprites: []
139:     attackSpritesUp: []
140:     attackSpritesDown: []
141:     attackAnimationSpeed: 0.12
142:     isRanged: 0
143:     projectileSpeed: 8
144:     projectileLifeTime: 3
145:     projectileSprite: {fileID: 0}
146:     projectileScale: {x: 0.4, y: 0.4, z: 0.4}
147:     projectileSpawnOffset: 0.5
148:     expandColliderOnAttack: 0
149:     attackColliderSize: {x: 2, y: 2, z: 2}
150:     attackColliderCenter: {x: 0, y: 0.55, z: 0}
151:     deathSprites: []
152:     deathAnimationSpeed: 0.15
153:     isElite: 1
154:     tintColor: {r: 1, g: 1, b: 1, a: 1}
155:     killTriggerEnemyName: 
156:     killTriggerCount: 10
157:     splitsOnDeath: 0
158:     splitCount: 2
159:     splitEnemyName: 
160:     splitVfxSprites: []
161:     splitVfxScale: 3
162:     splitVfxSpeed: 0.08
163:     splitVfxDuration: 0.6
164:     chargesAtPlayer: 0
165:     chargeSpeed: 6
166:     chargeAccelTime: 0.3
167:     leavesDebrisOnDeath: 0
168:     debrisDuration: 5
169:     debrisAggroRadius: 8
170:     debrisVfxSprites: []
171:     debrisVfxScale: 15
172:     debrisVfxSpeed: 0.12
173:   minimumCount: 2
174:   maximumCount: 2
175:   minimumPerType: 1
176:   minimumSpacing: 64
177:   entranceExclusionRadius: 18
178:   portalExclusionRadius: 10
179:   bossExclusionPadding: 8
180:   clearanceCells: 1
181:   activationDistance: 48
182:   releaseDistance: 72
183:   leashDistance: 16
```

## Assets/_Project/Data/BiomeElites/LiverBiomeEliteSpawnConfig.asset

```yaml
4: MonoBehaviour:
15:   enabled: 1
16:   monsters:
17:   - monsterDefinition: {fileID: 11400000, guid: d6df2b4e6bd5a4c21838d0e2934d6b06, type: 2}
18:     name: H-02 Inflammation Ember
19:     density: 0.0025
20:     minDistance: 8
21:     poissonSalt: 400
22:     allowedRegions: 
23:     maxAlive: 1
24:     activationRadius: 20
25:     respawnCooldown: 8
26:     spawnRadius: 1.5
27:     moveSpeed: 1.5
28:     stoppingDistance: 0.2
29:     wanderRadius: 0
30:     chaseRadius: 7
31:     leashRadius: 16
32:     idleDelayRange: {x: 0.5, y: 1.5}
33:     maxHealth: 30
34:     attackDamage: 1
35:     attackRange: 1.5
36:     attackCooldown: 1
37:     expReward: 10
38:     enableContactDamage: 1
39:     contactDamage: 1
40:     contactKnockbackDistance: 0.45
41:     additionalBaseStats: []
42:     separationDistance: 1.1
43:     separationStrength: 1
44:     heightOffset: 0
45:     scale: {x: 1, y: 1, z: 1}
46:     sortingOrder: 1000
47:     useBillboard: 1
48:     useYSort: 1
49:     animationSpeed: 0.15
50:     addCollider: 1
51:     isTrigger: 1
52:     colliderSize: {x: 1.2, y: 1.2, z: 0.95}
53:     colliderCenter: {x: 0, y: 0.55, z: 0}
54:     idleSprites: []
55:     idleSpritesUp: []
56:     idleSpritesDown: []
57:     moveSprites: []
58:     moveSpritesUp: []
59:     moveSpritesDown: []
60:     attackSprites: []
61:     attackSpritesUp: []
62:     attackSpritesDown: []
63:     attackAnimationSpeed: 0.12
64:     isRanged: 0
65:     projectileSpeed: 8
66:     projectileLifeTime: 3
67:     projectileSprite: {fileID: 0}
68:     projectileScale: {x: 0.4, y: 0.4, z: 0.4}
69:     projectileSpawnOffset: 0.5
70:     expandColliderOnAttack: 0
71:     attackColliderSize: {x: 2, y: 2, z: 2}
72:     attackColliderCenter: {x: 0, y: 0.55, z: 0}
73:     deathSprites: []
74:     deathAnimationSpeed: 0.15
75:     isElite: 1
76:     tintColor: {r: 1, g: 1, b: 1, a: 1}
77:     killTriggerEnemyName: 
78:     killTriggerCount: 10
79:     splitsOnDeath: 0
80:     splitCount: 2
81:     splitEnemyName: 
82:     splitVfxSprites: []
83:     splitVfxScale: 3
84:     splitVfxSpeed: 0.08
85:     splitVfxDuration: 0.6
86:     chargesAtPlayer: 0
87:     chargeSpeed: 6
88:     chargeAccelTime: 0.3
89:     leavesDebrisOnDeath: 0
90:     debrisDuration: 5
91:     debrisAggroRadius: 8
92:     debrisVfxSprites: []
93:     debrisVfxScale: 15
94:     debrisVfxSpeed: 0.12
95:   - monsterDefinition: {fileID: 11400000, guid: 41d268ef4aa33425ea41d7e7c1585d70, type: 2}
96:     name: H-03 Hangover Remnant
97:     density: 0.0025
98:     minDistance: 8
99:     poissonSalt: 400
100:     allowedRegions: 
101:     maxAlive: 1
102:     activationRadius: 20
103:     respawnCooldown: 8
104:     spawnRadius: 1.5
105:     moveSpeed: 1.5
106:     stoppingDistance: 0.2
107:     wanderRadius: 0
108:     chaseRadius: 7
109:     leashRadius: 16
110:     idleDelayRange: {x: 0.5, y: 1.5}
111:     maxHealth: 30
112:     attackDamage: 1
113:     attackRange: 1.5
114:     attackCooldown: 1
115:     expReward: 10
116:     enableContactDamage: 1
117:     contactDamage: 1
118:     contactKnockbackDistance: 0.45
119:     additionalBaseStats: []
120:     separationDistance: 1.1
121:     separationStrength: 1
122:     heightOffset: 0
123:     scale: {x: 1, y: 1, z: 1}
124:     sortingOrder: 1000
125:     useBillboard: 1
126:     useYSort: 1
127:     animationSpeed: 0.15
128:     addCollider: 1
129:     isTrigger: 1
130:     colliderSize: {x: 1.25, y: 1.1, z: 0.95}
131:     colliderCenter: {x: 0, y: 0.5, z: 0}
132:     idleSprites: []
133:     idleSpritesUp: []
134:     idleSpritesDown: []
135:     moveSprites: []
136:     moveSpritesUp: []
137:     moveSpritesDown: []
138:     attackSprites: []
139:     attackSpritesUp: []
140:     attackSpritesDown: []
141:     attackAnimationSpeed: 0.12
142:     isRanged: 0
143:     projectileSpeed: 8
144:     projectileLifeTime: 3
145:     projectileSprite: {fileID: 0}
146:     projectileScale: {x: 0.4, y: 0.4, z: 0.4}
147:     projectileSpawnOffset: 0.5
148:     expandColliderOnAttack: 0
149:     attackColliderSize: {x: 2, y: 2, z: 2}
150:     attackColliderCenter: {x: 0, y: 0.55, z: 0}
151:     deathSprites: []
152:     deathAnimationSpeed: 0.15
153:     isElite: 1
154:     tintColor: {r: 1, g: 1, b: 1, a: 1}
155:     killTriggerEnemyName: 
156:     killTriggerCount: 10
157:     splitsOnDeath: 0
158:     splitCount: 2
159:     splitEnemyName: 
160:     splitVfxSprites: []
161:     splitVfxScale: 3
162:     splitVfxSpeed: 0.08
163:     splitVfxDuration: 0.6
164:     chargesAtPlayer: 0
165:     chargeSpeed: 6
166:     chargeAccelTime: 0.3
167:     leavesDebrisOnDeath: 0
168:     debrisDuration: 5
169:     debrisAggroRadius: 8
170:     debrisVfxSprites: []
171:     debrisVfxScale: 15
172:     debrisVfxSpeed: 0.12
173:   minimumCount: 2
174:   maximumCount: 2
175:   minimumPerType: 1
176:   minimumSpacing: 64
177:   entranceExclusionRadius: 18
178:   portalExclusionRadius: 10
179:   bossExclusionPadding: 8
180:   clearanceCells: 5
181:   activationDistance: 48
182:   releaseDistance: 72
183:   leashDistance: 16
```

## Assets/_Project/Data/BiomeElites/LungBiomeEliteSpawnConfig.asset

```yaml
4: MonoBehaviour:
15:   enabled: 1
16:   monsters:
17:   - monsterDefinition: {fileID: 11400000, guid: c4d1d7182f0e648f9b4b50d6d91f26da, type: 2}
18:     name: P-03 Pollen Invader
19:     density: 0.0025
20:     minDistance: 8
21:     poissonSalt: 400
22:     allowedRegions: 
23:     maxAlive: 1
24:     activationRadius: 20
25:     respawnCooldown: 8
26:     spawnRadius: 1.5
27:     moveSpeed: 1.5
28:     stoppingDistance: 0.2
29:     wanderRadius: 0
30:     chaseRadius: 8
31:     leashRadius: 16
32:     idleDelayRange: {x: 0.5, y: 1.5}
33:     maxHealth: 30
34:     attackDamage: 1
35:     attackRange: 1.5
36:     attackCooldown: 1
37:     expReward: 10
38:     enableContactDamage: 1
39:     contactDamage: 1
40:     contactKnockbackDistance: 0.45
41:     additionalBaseStats: []
42:     separationDistance: 1.1
43:     separationStrength: 1
44:     heightOffset: 0
45:     scale: {x: 1, y: 1, z: 1}
46:     sortingOrder: 1000
47:     useBillboard: 1
48:     useYSort: 1
49:     animationSpeed: 0.15
50:     addCollider: 1
51:     isTrigger: 1
52:     colliderSize: {x: 1.2, y: 1.5, z: 1.1}
53:     colliderCenter: {x: 0, y: 0.75, z: 0}
54:     idleSprites: []
55:     idleSpritesUp: []
56:     idleSpritesDown: []
57:     moveSprites: []
58:     moveSpritesUp: []
59:     moveSpritesDown: []
60:     attackSprites: []
61:     attackSpritesUp: []
62:     attackSpritesDown: []
63:     attackAnimationSpeed: 0.12
64:     isRanged: 0
65:     projectileSpeed: 8
66:     projectileLifeTime: 3
67:     projectileSprite: {fileID: 0}
68:     projectileScale: {x: 0.4, y: 0.4, z: 0.4}
69:     projectileSpawnOffset: 0.5
70:     expandColliderOnAttack: 0
71:     attackColliderSize: {x: 2, y: 2, z: 2}
72:     attackColliderCenter: {x: 0, y: 0.55, z: 0}
73:     deathSprites: []
74:     deathAnimationSpeed: 0.15
75:     isElite: 1
76:     tintColor: {r: 1, g: 1, b: 1, a: 1}
77:     killTriggerEnemyName: 
78:     killTriggerCount: 10
79:     splitsOnDeath: 0
80:     splitCount: 2
81:     splitEnemyName: 
82:     splitVfxSprites: []
83:     splitVfxScale: 3
84:     splitVfxSpeed: 0.08
85:     splitVfxDuration: 0.6
86:     chargesAtPlayer: 0
87:     chargeSpeed: 6
88:     chargeAccelTime: 0.3
89:     leavesDebrisOnDeath: 0
90:     debrisDuration: 5
91:     debrisAggroRadius: 8
92:     debrisVfxSprites: []
93:     debrisVfxScale: 15
94:     debrisVfxSpeed: 0.12
95:   - monsterDefinition: {fileID: 11400000, guid: 84269ffb6bc924237a2eb0a9b0b69e36, type: 2}
96:     name: P-01 Dust Clump
97:     density: 0.0025
98:     minDistance: 8
99:     poissonSalt: 400
100:     allowedRegions: 
101:     maxAlive: 1
102:     activationRadius: 20
103:     respawnCooldown: 8
104:     spawnRadius: 1.5
105:     moveSpeed: 1.5
106:     stoppingDistance: 0.2
107:     wanderRadius: 0
108:     chaseRadius: 7
109:     leashRadius: 16
110:     idleDelayRange: {x: 0.5, y: 1.5}
111:     maxHealth: 30
112:     attackDamage: 1
113:     attackRange: 1.5
114:     attackCooldown: 1
115:     expReward: 10
116:     enableContactDamage: 1
117:     contactDamage: 1
118:     contactKnockbackDistance: 0.45
119:     additionalBaseStats: []
120:     separationDistance: 1.1
121:     separationStrength: 1
122:     heightOffset: 0
123:     scale: {x: 1, y: 1, z: 1}
124:     sortingOrder: 1000
125:     useBillboard: 1
126:     useYSort: 1
127:     animationSpeed: 0.15
128:     addCollider: 1
129:     isTrigger: 1
130:     colliderSize: {x: 1.4, y: 1.6, z: 1.2}
131:     colliderCenter: {x: 0, y: 0.8, z: 0}
132:     idleSprites: []
133:     idleSpritesUp: []
134:     idleSpritesDown: []
135:     moveSprites: []
136:     moveSpritesUp: []
137:     moveSpritesDown: []
138:     attackSprites: []
139:     attackSpritesUp: []
140:     attackSpritesDown: []
141:     attackAnimationSpeed: 0.12
142:     isRanged: 0
143:     projectileSpeed: 8
144:     projectileLifeTime: 3
145:     projectileSprite: {fileID: 0}
146:     projectileScale: {x: 0.4, y: 0.4, z: 0.4}
147:     projectileSpawnOffset: 0.5
148:     expandColliderOnAttack: 0
149:     attackColliderSize: {x: 2, y: 2, z: 2}
150:     attackColliderCenter: {x: 0, y: 0.55, z: 0}
151:     deathSprites: []
152:     deathAnimationSpeed: 0.15
153:     isElite: 1
154:     tintColor: {r: 1, g: 1, b: 1, a: 1}
155:     killTriggerEnemyName: 
156:     killTriggerCount: 10
157:     splitsOnDeath: 0
158:     splitCount: 2
159:     splitEnemyName: 
160:     splitVfxSprites: []
161:     splitVfxScale: 3
162:     splitVfxSpeed: 0.08
163:     splitVfxDuration: 0.6
164:     chargesAtPlayer: 0
165:     chargeSpeed: 6
166:     chargeAccelTime: 0.3
167:     leavesDebrisOnDeath: 0
168:     debrisDuration: 5
169:     debrisAggroRadius: 8
170:     debrisVfxSprites: []
171:     debrisVfxScale: 15
172:     debrisVfxSpeed: 0.12
173:   minimumCount: 2
174:   maximumCount: 2
175:   minimumPerType: 1
176:   minimumSpacing: 64
177:   entranceExclusionRadius: 18
178:   portalExclusionRadius: 10
179:   bossExclusionPadding: 8
180:   clearanceCells: 5
181:   activationDistance: 48
182:   releaseDistance: 72
183:   leashDistance: 16
```

## Assets/_Project/Data/BiomeElites/StomachBiomeEliteSpawnConfig.asset

```yaml
4: MonoBehaviour:
15:   enabled: 1
16:   monsters:
17:   - monsterDefinition: {fileID: 11400000, guid: b37cc3e78ec8a415a85f8b1316b78c2b, type: 2}
18:     name: S-01 Helico Spiral
19:     density: 0.0025
20:     minDistance: 8
21:     poissonSalt: 400
22:     allowedRegions: 
23:     maxAlive: 1
24:     activationRadius: 20
25:     respawnCooldown: 8
26:     spawnRadius: 1.5
27:     moveSpeed: 1.5
28:     stoppingDistance: 0.2
29:     wanderRadius: 0
30:     chaseRadius: 8
31:     leashRadius: 16
32:     idleDelayRange: {x: 0.5, y: 1.5}
33:     maxHealth: 30
34:     attackDamage: 1
35:     attackRange: 1.5
36:     attackCooldown: 1
37:     expReward: 10
38:     enableContactDamage: 1
39:     contactDamage: 1
40:     contactKnockbackDistance: 0.45
41:     additionalBaseStats: []
42:     separationDistance: 1.1
43:     separationStrength: 1
44:     heightOffset: 0
45:     scale: {x: 1, y: 1, z: 1}
46:     sortingOrder: 1000
47:     useBillboard: 1
48:     useYSort: 1
49:     animationSpeed: 0.15
50:     addCollider: 1
51:     isTrigger: 1
52:     colliderSize: {x: 1.5, y: 0.8, z: 1.5}
53:     colliderCenter: {x: 0, y: 0.4, z: 0}
54:     idleSprites: []
55:     idleSpritesUp: []
56:     idleSpritesDown: []
57:     moveSprites: []
58:     moveSpritesUp: []
59:     moveSpritesDown: []
60:     attackSprites: []
61:     attackSpritesUp: []
62:     attackSpritesDown: []
63:     attackAnimationSpeed: 0.12
64:     isRanged: 0
65:     projectileSpeed: 8
66:     projectileLifeTime: 3
67:     projectileSprite: {fileID: 0}
68:     projectileScale: {x: 0.4, y: 0.4, z: 0.4}
69:     projectileSpawnOffset: 0.5
70:     expandColliderOnAttack: 0
71:     attackColliderSize: {x: 2, y: 2, z: 2}
72:     attackColliderCenter: {x: 0, y: 0.55, z: 0}
73:     deathSprites: []
74:     deathAnimationSpeed: 0.15
75:     isElite: 1
76:     tintColor: {r: 1, g: 1, b: 1, a: 1}
77:     killTriggerEnemyName: 
78:     killTriggerCount: 10
79:     splitsOnDeath: 0
80:     splitCount: 2
81:     splitEnemyName: 
82:     splitVfxSprites: []
83:     splitVfxScale: 3
84:     splitVfxSpeed: 0.08
85:     splitVfxDuration: 0.6
86:     chargesAtPlayer: 0
87:     chargeSpeed: 6
88:     chargeAccelTime: 0.3
89:     leavesDebrisOnDeath: 0
90:     debrisDuration: 5
91:     debrisAggroRadius: 8
92:     debrisVfxSprites: []
93:     debrisVfxScale: 15
94:     debrisVfxSpeed: 0.12
95:   - monsterDefinition: {fileID: 11400000, guid: 7ee838cb9898b43c89ae21d91271b46b, type: 2}
96:     name: S-03 Oil Film Glider
97:     density: 0.0025
98:     minDistance: 8
99:     poissonSalt: 400
100:     allowedRegions: 
101:     maxAlive: 1
102:     activationRadius: 20
103:     respawnCooldown: 8
104:     spawnRadius: 1.5
105:     moveSpeed: 1.5
106:     stoppingDistance: 0.2
107:     wanderRadius: 0
108:     chaseRadius: 8
109:     leashRadius: 16
110:     idleDelayRange: {x: 0.5, y: 1.5}
111:     maxHealth: 30
112:     attackDamage: 1
113:     attackRange: 1.5
114:     attackCooldown: 1
115:     expReward: 10
116:     enableContactDamage: 1
117:     contactDamage: 1
118:     contactKnockbackDistance: 0.45
119:     additionalBaseStats: []
120:     separationDistance: 1.1
121:     separationStrength: 1
122:     heightOffset: 0
123:     scale: {x: 1, y: 1, z: 1}
124:     sortingOrder: 1000
125:     useBillboard: 1
126:     useYSort: 1
127:     animationSpeed: 0.15
128:     addCollider: 1
129:     isTrigger: 1
130:     colliderSize: {x: 1, y: 0.8, z: 1}
131:     colliderCenter: {x: 0, y: 0.4, z: 0}
132:     idleSprites: []
133:     idleSpritesUp: []
134:     idleSpritesDown: []
135:     moveSprites: []
136:     moveSpritesUp: []
137:     moveSpritesDown: []
138:     attackSprites: []
139:     attackSpritesUp: []
140:     attackSpritesDown: []
141:     attackAnimationSpeed: 0.12
142:     isRanged: 0
143:     projectileSpeed: 8
144:     projectileLifeTime: 3
145:     projectileSprite: {fileID: 0}
146:     projectileScale: {x: 0.4, y: 0.4, z: 0.4}
147:     projectileSpawnOffset: 0.5
148:     expandColliderOnAttack: 0
149:     attackColliderSize: {x: 2, y: 2, z: 2}
150:     attackColliderCenter: {x: 0, y: 0.55, z: 0}
151:     deathSprites: []
152:     deathAnimationSpeed: 0.15
153:     isElite: 1
154:     tintColor: {r: 1, g: 1, b: 1, a: 1}
155:     killTriggerEnemyName: 
156:     killTriggerCount: 10
157:     splitsOnDeath: 0
158:     splitCount: 2
159:     splitEnemyName: 
160:     splitVfxSprites: []
161:     splitVfxScale: 3
162:     splitVfxSpeed: 0.08
163:     splitVfxDuration: 0.6
164:     chargesAtPlayer: 0
165:     chargeSpeed: 6
166:     chargeAccelTime: 0.3
167:     leavesDebrisOnDeath: 0
168:     debrisDuration: 5
169:     debrisAggroRadius: 8
170:     debrisVfxSprites: []
171:     debrisVfxScale: 15
172:     debrisVfxSpeed: 0.12
173:   minimumCount: 2
174:   maximumCount: 2
175:   minimumPerType: 1
176:   minimumSpacing: 64
177:   entranceExclusionRadius: 18
178:   portalExclusionRadius: 10
179:   bossExclusionPadding: 8
180:   clearanceCells: 5
181:   activationDistance: 48
182:   releaseDistance: 72
183:   leashDistance: 16
```

## Assets/_Project/Data/MonsterBalance/Definitions/boss.intestine.asset

```yaml
4: MonoBehaviour:
15:   monsterId: boss.intestine
16:   displayName: IntestineBoss
17:   tier: 2
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 10
25:     attackPower: 2
26:     moveSpeed: 1.5
27:   - id: Phase2
28:     maxHealth: 10
29:     attackPower: 2
30:     moveSpeed: 1.7249999
31:   reward:
32:     baseExperience: 0
33:   contact:
34:     enabled: 1
35:     damageCoefficient: 0.5
36:     knockbackDistance: 0.45
37:   patternDamage:
38:   - id: dung
39:     coefficient: 0.5
40:   - id: stomp
41:     coefficient: 1
```

## Assets/_Project/Data/MonsterBalance/Definitions/boss.liver.asset

```yaml
4: MonoBehaviour:
15:   monsterId: boss.liver
16:   displayName: LiverBoss
17:   tier: 2
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 180
25:     attackPower: 2
26:     moveSpeed: 1
27:   - id: Phase2
28:     maxHealth: 180
29:     attackPower: 2
30:     moveSpeed: 1.5
31:   reward:
32:     baseExperience: 0
33:   contact:
34:     enabled: 1
35:     damageCoefficient: 0.5
36:     knockbackDistance: 0.45
37:   patternDamage:
38:   - id: blood-bomb
39:     coefficient: 1
```

## Assets/_Project/Data/MonsterBalance/Definitions/boss.lung.asset

```yaml
4: MonoBehaviour:
15:   monsterId: boss.lung
16:   displayName: LungBoss
17:   tier: 2
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 40
25:     attackPower: 2
26:     moveSpeed: 1
27:   - id: Phase2
28:     maxHealth: 100
29:     attackPower: 4
30:     moveSpeed: 1.5
31:   reward:
32:     baseExperience: 0
33:   contact:
34:     enabled: 1
35:     damageCoefficient: 0.5
36:     knockbackDistance: 0.45
37:   patternDamage:
38:   - id: gas
39:     coefficient: 1
40:   - id: dive
41:     coefficient: 0.75
```

## Assets/_Project/Data/MonsterBalance/Definitions/boss.stomach.asset

```yaml
4: MonoBehaviour:
15:   monsterId: boss.stomach
16:   displayName: StomachBoss
17:   tier: 2
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 10
25:     attackPower: 3
26:     moveSpeed: 0.5
27:   - id: Phase2
28:     maxHealth: 10
29:     attackPower: 3
30:     moveSpeed: 1
31:   reward:
32:     baseExperience: 0
33:   contact:
34:     enabled: 1
35:     damageCoefficient: 0.33333334
36:     knockbackDistance: 0.45
37:   patternDamage:
38:   - id: melee
39:     coefficient: 1
40:   - id: charge
41:     coefficient: 1.3333334
42:   - id: acid
43:     coefficient: 0.6666667
44:   - id: acid-dot
45:     coefficient: 0.33333334
46:   - id: short-range
47:     coefficient: 1.6666666
```

## Assets/_Project/Data/MonsterBalance/Definitions/H-02.asset

```yaml
4: MonoBehaviour:
15:   monsterId: elite.h-02
16:   displayName: "H-02 \uC5FC\uC99D \uBD88\uC528"
17:   tier: 1
18:   designNote: "H-02-A: \uC720\uD6A8 \uD53C\uACA9 \uC2DC \uBC29\uD5A5 \uACE0\uC815
19:     \u2192 \uD575 \uC810\uB4F1 \u2192 \uAC00\uC2DC\uD0C4 1\uBC1C \u2192 \uD68C\uBCF5.
20:     \uC5F0\uD0C0\uB294 \uC608\uC57D/\uD0C0\uC774\uBA38\uB97C \uB298\uB9AC\uC9C0 \uC54A\uC74C.
21:     \uBCC4\uB3C4 \uC0AC\uB9DD 6\uD504\uB808\uC784. H-MAP \uBCF8\uB9F5 \uB4F1\uB85D\uC740
22:     \uBCC4\uB3C4 \uC2B9\uC778."
23:   statSets:
24:   - id: Default
25:     maxHealth: 30
26:     attackPower: 2
27:     moveSpeed: 1
28:   reward:
29:     baseExperience: 50
30:   contact:
31:     enabled: 1
32:     damageCoefficient: 0.5
33:     knockbackDistance: 0.45
34:   pattern: {fileID: 11400000, guid: d4e2a97a3aae14ca1a1a8a8708af454f, type: 2}
35:   patternDamage:
36:   - id: ember-counter
37:     coefficient: 1.5
```

## Assets/_Project/Data/MonsterBalance/Definitions/H-03.asset

```yaml
4: MonoBehaviour:
15:   monsterId: elite.h-03
16:   displayName: "H-03 \uC219\uCDE8 \uC794\uC7AC"
17:   tier: 1
18:   designNote: "H-03-A: \uC815\uBA74 \uACE0\uC815, \uC790\uAE30 \uC9C0\uBA74\uC5D0
19:     0.65\uCD08 \uC608\uACE0 \u2192 \uC794\uC5EC\uBB3C 1\uAC1C \u2192 \uB300\uC0C1
20:     \uBC18\uB300 \uD6C4\uD1F4 \u2192 \uC0DD\uC131 1.1\uCD08 \uD6C4 \uB2E8\uBC1C \uD3ED\uBC1C.
21:     \uC804\uC6A9 \uC815\uBA74 \uC0AC\uB9DD 6\uC7A5. H-MAP \uB4F1\uB85D \uBCC4\uB3C4
22:     \uC2B9\uC778."
23:   statSets:
24:   - id: Default
25:     maxHealth: 30
26:     attackPower: 2
27:     moveSpeed: 1
28:   reward:
29:     baseExperience: 50
30:   contact:
31:     enabled: 1
32:     damageCoefficient: 0.5
33:     knockbackDistance: 0.45
34:   pattern: {fileID: 11400000, guid: dce7f7b014dd94caf844a1e09ff47ae5, type: 2}
35:   patternDamage:
36:   - id: hangover-burst
37:     coefficient: 1.5
```

## Assets/_Project/Data/MonsterBalance/Definitions/I-01.asset

```yaml
4: MonoBehaviour:
15:   monsterId: elite.i-01
16:   displayName: "I-01 \uAC00\uC2A4\uB0AD"
17:   tier: 1
18:   designNote: "I-01 A/B: \uADFC\uAC70\uB9AC \uB2E8\uBC1C \uD3ED\uBC1C, \uC6D0\uAC70\uB9AC
19:     \uACE0\uC815 \uC870\uC900 \uAC00\uC2A4\uD0C4 1\uBC1C. \uD68C\uBCF5/\uB300\uAE30\uB294
20:     \uC120\uD0DD\uD55C \uD328\uD134\uB9CC \uC801\uC6A9. I-MAP \uB4F1\uB85D \uC804
21:     \uAC1C\uBCC4 \uAC80\uD1A0 \uB2E8\uACC4."
22:   statSets:
23:   - id: Default
24:     maxHealth: 30
25:     attackPower: 2
26:     moveSpeed: 1
27:   reward:
28:     baseExperience: 50
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 0.5
32:     knockbackDistance: 0.45
33:   pattern: {fileID: 11400000, guid: eb3bfeb7f81d74452bca371176d4ab23, type: 2}
34:   patternDamage:
35:   - id: gas-burst
36:     coefficient: 1.5
37:   - id: gas-orb
38:     coefficient: 1.25
```

## Assets/_Project/Data/MonsterBalance/Definitions/I-02.asset

```yaml
4: MonoBehaviour:
15:   monsterId: elite.i-02
16:   displayName: "I-02 \uAD73\uC740 \uC794\uC5EC\uCCB4"
17:   tier: 1
18:   designNote: "I-02-A: \uC870\uAC01 \uB4E4\uAE30 \u2192 \uACE0\uC815 \uCC29\uC9C0
19:     1\uD68C \uD53C\uD574 \u2192 1\uD0C0 \uD30C\uAD34 \uAC00\uB2A5\uD55C \uC774\uB3D9
20:     \uBC29\uD574\uBB3C 1\uAC1C \u2192 \uD68C\uBCF5. \uC804\uC6A9 \uC0AC\uB9DD 6\uD504\uB808\uC784.
21:     I-MAP \uBCF8\uB9F5 \uB4F1\uB85D\uC740 \uBCC4\uB3C4 \uAC80\uD1A0."
22:   statSets:
23:   - id: Default
24:     maxHealth: 30
25:     attackPower: 2
26:     moveSpeed: 1
27:   reward:
28:     baseExperience: 50
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 0.5
32:     knockbackDistance: 0.45
33:   pattern: {fileID: 11400000, guid: 43df375e56167444397765f7a5cb63c5, type: 2}
34:   patternDamage:
35:   - id: residue-landing
36:     coefficient: 1.5
```

## Assets/_Project/Data/MonsterBalance/Definitions/legacy-elite.antibody.asset

```yaml
4: MonoBehaviour:
15:   monsterId: legacy-elite.antibody
16:   displayName: Antibody
17:   tier: 1
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 35
25:     attackPower: 1
26:     moveSpeed: 1.4
27:   reward:
28:     baseExperience: 60
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/legacy-elite.granuloma.asset

```yaml
4: MonoBehaviour:
15:   monsterId: legacy-elite.granuloma
16:   displayName: Granuloma
17:   tier: 1
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 30
25:     attackPower: 1
26:     moveSpeed: 1.2
27:   reward:
28:     baseExperience: 50
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.bcell.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.bcell
16:   displayName: BCell
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 4
25:     attackPower: 1
26:     moveSpeed: 0.8
27:   reward:
28:     baseExperience: 15
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.intestine-parasite.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.intestine-parasite
16:   displayName: "\uC7A5 \uBCF4\uC2A4 \uC18C\uD658 \uAE30\uC0DD\uCDA9"
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 6
25:     attackPower: 1
26:     moveSpeed: 2.3
27:   reward:
28:     baseExperience: 0
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.intestinefoldworm.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.intestinefoldworm
16:   displayName: IntestineFoldworm
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 6
25:     attackPower: 1
26:     moveSpeed: 1.1
27:   reward:
28:     baseExperience: 10
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   pattern: {fileID: 0}
34:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.intestineknottedeel.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.intestineknottedeel
16:   displayName: IntestineKnottedEel
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 8
25:     attackPower: 1
26:     moveSpeed: 0.9
27:   reward:
28:     baseExperience: 12
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   pattern: {fileID: 0}
34:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.intestinevillushand.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.intestinevillushand
16:   displayName: IntestineVillusHand
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 6
25:     attackPower: 1
26:     moveSpeed: 0.8
27:   reward:
28:     baseExperience: 10
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   pattern: {fileID: 0}
34:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.liverclot.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.liverclot
16:   displayName: LiverClot
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 8
25:     attackPower: 1
26:     moveSpeed: 0.8
27:   reward:
28:     baseExperience: 12
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   pattern: {fileID: 0}
34:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.livercoagulationcore.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.livercoagulationcore
16:   displayName: LiverCoagulationCore
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 6
25:     attackPower: 1
26:     moveSpeed: 0.8
27:   reward:
28:     baseExperience: 15
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   pattern: {fileID: 0}
34:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.liversuturecell.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.liversuturecell
16:   displayName: LiverSutureCell
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 4
25:     attackPower: 1
26:     moveSpeed: 0.8
27:   reward:
28:     baseExperience: 15
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   pattern: {fileID: 0}
34:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.lungbreathbubble.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.lungbreathbubble
16:   displayName: LungBreathBubble
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 4
25:     attackPower: 1
26:     moveSpeed: 0.8
27:   reward:
28:     baseExperience: 15
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   pattern: {fileID: 0}
34:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.lungciliabrush.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.lungciliabrush
16:   displayName: LungCiliaBrush
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 6
25:     attackPower: 1
26:     moveSpeed: 1.1
27:   reward:
28:     baseExperience: 10
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   pattern: {fileID: 0}
34:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.lungoverinflatedalveoli.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.lungoverinflatedalveoli
16:   displayName: LungOverinflatedAlveoli
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 6
25:     attackPower: 1
26:     moveSpeed: 0.8
27:   reward:
28:     baseExperience: 15
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   pattern: {fileID: 0}
34:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.macrophage.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.macrophage
16:   displayName: Macrophage
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 6
25:     attackPower: 1
26:     moveSpeed: 1
27:   reward:
28:     baseExperience: 10
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.nkcell.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.nkcell
16:   displayName: NKCell
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 6
25:     attackPower: 1
26:     moveSpeed: 1
27:   reward:
28:     baseExperience: 10
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.stomachgulp.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.stomachgulp
16:   displayName: StomachGulp
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 4
25:     attackPower: 1
26:     moveSpeed: 0.8
27:   reward:
28:     baseExperience: 15
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   pattern: {fileID: 0}
34:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.stomachmucussnail.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.stomachmucussnail
16:   displayName: StomachMucusSnail
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 6
25:     attackPower: 1
26:     moveSpeed: 0.8
27:   reward:
28:     baseExperience: 10
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   pattern: {fileID: 0}
34:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/normal.stomachoverfedsac.asset

```yaml
4: MonoBehaviour:
15:   monsterId: normal.stomachoverfedsac
16:   displayName: StomachOverfedSac
17:   tier: 0
18:   designNote: "C-02 \uAE30\uC874 \uC804\uD22C \uC774\uAD00. \uC6D0\uBCF8 \uC218\uC815\uC740
19:     \uC0C8 \uAC1C\uCCB4\uBD80\uD130 \uC801\uC6A9. \uC7AC\uC0AC\uC6A9 \uB300\uAE30\xB7\uBC94\uC704\xB7\uC608\uACE0
20:     \uC2DC\uAC04\uC740 \uC2A4\uD3F0 \uADDC\uCE59/\uBCF4\uC2A4 \uD328\uD134 \uC124\uC815\uC5D0\uC11C
21:     \uC870\uC808."
22:   statSets:
23:   - id: Default
24:     maxHealth: 6
25:     attackPower: 1
26:     moveSpeed: 0.8
27:   reward:
28:     baseExperience: 15
29:   contact:
30:     enabled: 1
31:     damageCoefficient: 1
32:     knockbackDistance: 0.45
33:   pattern: {fileID: 0}
34:   patternDamage: []
```

## Assets/_Project/Data/MonsterBalance/Definitions/P-01.asset

```yaml
4: MonoBehaviour:
15:   monsterId: elite.p-01
16:   displayName: "P-01 \uBD84\uC9C4 \uBB49\uCE58"
17:   tier: 1
18:   designNote: 'P-01-A: fixed vulnerable core, one committed moving dust area, one
19:     hit per cloud, separate cluster/core deaths. P-MAP registration requires its
20:     own approval.'
21:   statSets:
22:   - id: Default
23:     maxHealth: 30
24:     attackPower: 2
25:     moveSpeed: 1
26:   reward:
27:     baseExperience: 50
28:   contact:
29:     enabled: 1
30:     damageCoefficient: 0.5
31:     knockbackDistance: 0.45
32:   pattern: {fileID: 11400000, guid: 8b235f8c64ffa495bb97c4d980246a1f, type: 2}
33:   patternDamage:
34:   - id: dust-cloud
35:     coefficient: 1.5
```

## Assets/_Project/Data/MonsterBalance/Definitions/P-03.asset

```yaml
4: MonoBehaviour:
15:   monsterId: elite.p-03
16:   displayName: "P-03 \uAF43\uAC00\uB8E8 \uCE68\uC785\uC790"
17:   tier: 1
18:   designNote: 'P-03 A/B: original three-pellet fan or shell-turn follow-up. Shared
19:     one-hit budget per cycle; selected recovery/rearm once. P-MAP registration requires
20:     separate approval.'
21:   statSets:
22:   - id: Default
23:     maxHealth: 30
24:     attackPower: 2
25:     moveSpeed: 1
26:   reward:
27:     baseExperience: 50
28:   contact:
29:     enabled: 1
30:     damageCoefficient: 0.5
31:     knockbackDistance: 0.45
32:   pattern: {fileID: 11400000, guid: 507419c7cb0474f6ea4cdf9211b0dbb3, type: 2}
33:   patternDamage:
34:   - id: pollen-volley
35:     coefficient: 1.5
36:   - id: pollen-followup
37:     coefficient: 1.5
```

## Assets/_Project/Data/MonsterBalance/Definitions/S-01.asset

```yaml
4: MonoBehaviour:
15:   monsterId: elite.s-01
16:   displayName: "S-01 \uD5EC\uB9AC\uCF54 \uB098\uC120\uCDA9"
17:   tier: 1
18:   designNote: 'S-01-A: committed head-first dash, eight views/death, narrow oriented
19:     contact, separate receiving box. S-MAP registration requires approval.'
20:   statSets:
21:   - id: Default
22:     maxHealth: 30
23:     attackPower: 2
24:     moveSpeed: 1
25:   reward:
26:     baseExperience: 50
27:   contact:
28:     enabled: 1
29:     damageCoefficient: 0.5
30:     knockbackDistance: 0.45
31:   pattern: {fileID: 11400000, guid: c42152b0e34d942bcb5a860d535a9b8c, type: 2}
32:   patternDamage:
33:   - id: helico-dash
34:     coefficient: 1.5
```

## Assets/_Project/Data/MonsterBalance/Definitions/S-03.asset

```yaml
4: MonoBehaviour:
15:   monsterId: elite.s-03
16:   displayName: "S-03 \uAE30\uB984\uB9C9 \uD65C\uC8FC\uCCB4"
17:   tier: 1
18:   designNote: 'S-03-A: fixed core, committed full frontal sector, one impact, no
19:     inner safe zone. Production S-MAP placement requires separate approval.'
20:   statSets:
21:   - id: Default
22:     maxHealth: 30
23:     attackPower: 2
24:     moveSpeed: 1
25:   reward:
26:     baseExperience: 50
27:   contact:
28:     enabled: 1
29:     damageCoefficient: 0.5
30:     knockbackDistance: 0.45
31:   pattern: {fileID: 11400000, guid: 103ecbe581c6f492c8121e777edf6aa9, type: 2}
32:   patternDamage:
33:   - id: oil-film-spread
34:     coefficient: 1.5
```

## Assets/_Project/Data/MonsterBalance/MonsterProgressionProfile.asset

```yaml
4: MonoBehaviour:
15:   stages:
16:   - stage: 0
17:     maxHealth: 1
18:     attackPower: 1
19:     experience: 1
20:   - stage: 1
21:     maxHealth: 1.15
22:     attackPower: 1.08
23:     experience: 1.1
24:   - stage: 2
25:     maxHealth: 1.35
26:     attackPower: 1.2
27:     experience: 1.25
28:   - stage: 3
29:     maxHealth: 1.6
30:     attackPower: 1.35
31:     experience: 1.4
32:   - stage: 4
33:     maxHealth: 1.85
34:     attackPower: 1.5
35:     experience: 1.6
```

## Assets/_Project/Data/MonsterBalance/Patterns/DustClump_A.asset

```yaml
4: MonoBehaviour:
15:   triggerDistance: 4.5
16:   spawnGraceSeconds: 0.75
17:   windupSeconds: 0.9
18:   launchOffset: 1.2
19:   cloudRadius: 1.1
20:   cloudSpeed: 2
21:   cloudLifetimeSeconds: 2
22:   recoverySeconds: 0.8
23:   rearmSeconds: 3.5
24:   presentation: {fileID: 11400000, guid: 3409b16232f4844699020492a8dbd3a1, type: 2}
```

## Assets/_Project/Data/MonsterBalance/Patterns/GasSac_A.asset

```yaml
4: MonoBehaviour:
15:   triggerDistance: 2.5
16:   burstRadius: 3.25
17:   windupSeconds: 1.3
18:   burstVisualSeconds: 0.18
19:   recoverySeconds: 1.5
20:   rearmSeconds: 2.5
21:   spawnGraceSeconds: 0.75
22:   orbEnabled: 1
23:   orbExitDistance: 3
24:   orbEnterDistance: 3.8
25:   boundaryPriority: 0
26:   orbMaxDistance: 6.5
27:   orbWindupSeconds: 1.1
28:   orbSpeed: 3
29:   orbLifetimeSeconds: 3
30:   orbHitRadius: 0.3
31:   orbRecoverySeconds: 1.2
32:   orbRearmSeconds: 2.5
33:   presentation: {fileID: 11400000, guid: 7fba94a29e0c745c394d4f59f6c6efde, type: 2}
```

## Assets/_Project/Data/MonsterBalance/Patterns/HangoverRemnant_A.asset

```yaml
4: MonoBehaviour:
15:   triggerDistance: 2.5
16:   spawnGraceSeconds: 0.75
17:   windupSeconds: 0.65
18:   retreatDistance: 2.4
19:   retreatSpeed: 6
20:   burstDelaySeconds: 1.1
21:   burstRadius: 1.6
22:   recoverySeconds: 1
23:   rearmSeconds: 3.5
24:   presentation: {fileID: 11400000, guid: 5b18b2aac10db43158fdca8c3f1e50ff, type: 2}
```

## Assets/_Project/Data/MonsterBalance/Patterns/HardenedResidue_A.asset

```yaml
4: MonoBehaviour:
15:   triggerDistance: 2.8
16:   placementDistance: 2.3
17:   footprint: {x: 2.2, y: 0.7}
18:   windupSeconds: 0.8
19:   dropSeconds: 0.12
20:   recoverySeconds: 1
21:   rearmSeconds: 2.5
22:   spawnGraceSeconds: 0.75
23:   rubbleLifetime: 2
24:   hitsToBreak: 1
25:   presentation: {fileID: 11400000, guid: e21f71e3be4c84938b5e875d44651ce5, type: 2}
```

## Assets/_Project/Data/MonsterBalance/Patterns/HelicoSpiral_A.asset

```yaml
4: MonoBehaviour:
15:   triggerDistance: 4.5
16:   approachStopDistance: 2.5
17:   spawnGraceSeconds: 0.75
18:   windupSeconds: 0.7
19:   dashDistance: 3.5
20:   dashSeconds: 0.25
21:   bodyWidth: 0.8
22:   bodyHalfLength: 0.35
23:   recoverySeconds: 0.9
24:   rearmSeconds: 2.5
25:   tailSweepEnabled: 0
26:   tailWindupSeconds: 0.9
27:   tailSweepSeconds: 0.18
28:   tailReach: 1.1
29:   tailArcDegrees: 120
30:   tailWidth: 0.09
31:   tailRecoverySeconds: 1.25
32:   tailRearmSeconds: 2.5
33:   presentation: {fileID: 11400000, guid: 4128ba587c29a4856a52952a7b132024, type: 2}
```

## Assets/_Project/Data/MonsterBalance/Patterns/InflammationEmber_A.asset

```yaml
4: MonoBehaviour:
15:   counterMaxDistance: 8
16:   approachStopDistance: 1.5
17:   windupSeconds: 0.7
18:   recoverySeconds: 1.2
19:   rearmSeconds: 2.5
20:   thornSpeed: 4
21:   thornLifetimeSeconds: 2
22:   thornLength: 0.7
23:   thornRadius: 0.16
24:   presentation: {fileID: 11400000, guid: fc88a6aac716344f686df6a4a7f1a22b, type: 2}
```

## Assets/_Project/Data/MonsterBalance/Patterns/OilFilm_A.asset

```yaml
4: MonoBehaviour:
15:   triggerDistance: 3.2
16:   approachStopDistance: 2
17:   spawnGraceSeconds: 0.75
18:   windupSeconds: 0.85
19:   attackRange: 2.5
20:   arcDegrees: 110
21:   releaseSeconds: 0.28
22:   impactNormalizedTime: 0.5
23:   recoverySeconds: 1.1
24:   rearmSeconds: 2.5
25:   coreRadius: 0.28
26:   presentation: {fileID: 11400000, guid: 65aeced933c0940a8a9d0bd0fc73b3bb, type: 2}
```

## Assets/_Project/Data/MonsterBalance/Patterns/PollenInvader_A.asset

```yaml
4: MonoBehaviour:
15:   triggerDistance: 6
16:   approachStopDistance: 3.2
17:   spawnGraceSeconds: 0.75
18:   windupSeconds: 0.85
19:   spreadHalfAngle: 30
20:   pelletSpeed: 3.5
21:   pelletLifetimeSeconds: 2.4
22:   pelletRadius: 0.16
23:   recoverySeconds: 1
24:   rearmSeconds: 2.5
25:   followup:
26:     enabled: 1
27:     angleOffset: 15
28:     rotationSeconds: 0.8
29:     recoverySeconds: 1.2
30:     rearmSeconds: 3.2
31:   presentation: {fileID: 11400000, guid: 6754b93d7d3a14e988882a151fcc6520, type: 2}
```

## Assets/_Project/Resources/Balance/Difficulty/DifficultyBalanceCatalog.asset

```yaml
4: MonoBehaviour:
15:   normal: {fileID: 11400000, guid: 7be8df381e984a25ad3231c1dfa59f52, type: 2}
16:   hard: {fileID: 11400000, guid: f905a4020bbf4bf39ecf1df71f62cda8, type: 2}
```

## Assets/_Project/Resources/Balance/Difficulty/HardDifficultyBalance.asset

```yaml
4: MonoBehaviour:
15:   difficulty: 1
16:   displayName: Hard
17:   description: "\uC0AC\uB9DD \uC2DC \uC804\uBD80 \uCD08\uAE30\uD654\uB418\uB294 \uB3C5\uB9BD
18:     Run \uB09C\uC774\uB3C4."
19:   progression: {fileID: 11400000, guid: 95e5feaa12c54801b912479277307f41, type: 2}
20:   player:
21:     maxHealth: 1
22:     moveSpeed: 1
23:     attackPower: 1
24:     attackSpeed: 1
25:     attackRange: 1
26:     magic: 1
27:     skillCooldownReduction: 1
28:     basicAttackDamage: 1
29:     basicAttackCooldown: 1
30:     basicAttackRange: 1
31:     skillDamage: 1
32:     skillCooldown: 1
33:   enemies:
34:     maxHealth: 1
35:     moveSpeed: 1
36:     outgoingDamage: 1
37:     attackCooldown: 1
38:     experienceReward: 1
39:   elites:
40:     maxHealth: 1
41:     moveSpeed: 1
42:     outgoingDamage: 1
43:     attackCooldown: 1
44:     experienceReward: 1
45:   bosses:
46:     maxHealth: 1
47:     moveSpeed: 1
48:     outgoingDamage: 1
49:     attackCooldown: 1
50:     experienceReward: 1
51:   items:
52:     maxSlotsOverride: 0
53:     worldSpawnCount: 1
54:   world:
55:     enemySpawnerDensity: 1
56:     sceneObjectDensity: 1
57:     enemyMaxAlive: 1
58:     enemyRespawnCooldown: 1
59:   biomeWorldOverrides: []
60:   biomeOverrides: []
```

## Assets/_Project/Resources/Balance/Difficulty/HardLevelProgressionConfig.asset

```yaml
4: MonoBehaviour:
15:   maxLevel: 30
16:   baseExp: 30
17:   expIncreasePerLevel: 20
18:   enemyKillExp: 10
19:   earlyLevelUpperBound: 9
20:   earlyExpGainMultiplier: 2
21:   jobSelectionBlockedExpMultiplier: 0
22:   jobSelectionSelectedExpMultiplier: 1
23:   midLevelUpperBound: 20
24:   midExpGainMultiplier: 1
25:   lateExpGainMultiplier: 0.8
26:   jobSelectionLevel: 10
27:   jobBasedChoiceStartLevel: 11
28:   skill1UnlockLevel: 10
29:   skill2UnlockLevel: 20
30:   playerBaseStats:
31:   - statType: 0
32:     value: 10
33:   - statType: 1
34:     value: 10
35:   - statType: 2
36:     value: 1
37:   - statType: 4
38:     value: 1
39:   - statType: 5
40:     value: 1
41:   - statType: 6
42:     value: 1
43:   - statType: 7
44:     value: 0
45:   levelUpStatValues:
46:   - statType: 2
47:     mode: 0
48:     value: 1
49:     healWhenPositive: 0
50:   - statType: 5
51:     mode: 1
52:     value: 1
53:     healWhenPositive: 0
54:   - statType: 4
55:     mode: 1
56:     value: 1
57:     healWhenPositive: 0
58:   - statType: 6
59:     mode: 0
60:     value: 1
61:     healWhenPositive: 0
62:   - statType: 1
63:     mode: 1
64:     value: 1
65:     healWhenPositive: 0
66:   - statType: 0
67:     mode: 0
68:     value: 1
69:     healWhenPositive: 1
70:   bioGambleEnabled: 1
71:   bioGambleMinDelta: -1
72:   bioGambleMaxDelta: 3
```

## Assets/_Project/Resources/Balance/Difficulty/NormalDifficultyBalance.asset

```yaml
4: MonoBehaviour:
15:   difficulty: 0
16:   displayName: Normal
17:   description: "\uAE30\uBCF8 \uCEA0\uD398\uC778. \uC131\uC7A5\uACFC \uC9C4\uD589\uC744
18:     \uC800\uC7A5\uD55C\uB2E4."
19:   progression: {fileID: 11400000, guid: 2fa02adf21e34da0be20e35bde8f1569, type: 2}
20:   player:
21:     maxHealth: 1
22:     moveSpeed: 1
23:     attackPower: 1
24:     attackSpeed: 1
25:     attackRange: 1
26:     magic: 1
27:     skillCooldownReduction: 1
28:     basicAttackDamage: 1
29:     basicAttackCooldown: 1
30:     basicAttackRange: 1
31:     skillDamage: 1
32:     skillCooldown: 1
33:   enemies:
34:     maxHealth: 1
35:     moveSpeed: 1
36:     outgoingDamage: 1
37:     attackCooldown: 1
38:     experienceReward: 1
39:   elites:
40:     maxHealth: 1
41:     moveSpeed: 1
42:     outgoingDamage: 1
43:     attackCooldown: 1
44:     experienceReward: 1
45:   bosses:
46:     maxHealth: 1
47:     moveSpeed: 1
48:     outgoingDamage: 1
49:     attackCooldown: 1
50:     experienceReward: 1
51:   items:
52:     maxSlotsOverride: 0
53:     worldSpawnCount: 1
54:   world:
55:     enemySpawnerDensity: 1
56:     sceneObjectDensity: 1
57:     enemyMaxAlive: 1
58:     enemyRespawnCooldown: 1
59:   biomeWorldOverrides:
60:   - biome: 1
61:     world:
62:       enemySpawnerDensity: 1.25
63:       sceneObjectDensity: 1
64:       enemyMaxAlive: 1.5
65:       enemyRespawnCooldown: 1
66:   biomeOverrides: []
```

## Assets/_Project/Resources/Balance/FinalBoss/FinalBossBalanceConfig.asset

```yaml
4: MonoBehaviour:
15:   phaseOne:
16:     spawnInterval: 3.5
17:     commonMinionsPerWave: 4
18:     exclusiveTypesPerBiome: 2
19:     exclusiveCopiesPerType: 2
20:     weakenedMidBossStatRatio: 0.33
21:     weakenedMidBossScaleRatio: 1
22:     pillarMaxHealth: 65
23:   phaseTwo:
24:     bossMaxHealth: 520
25:     phaseStartDelay: 1.4
26:     patternCooldown: 1.8
27:     electricWarningDuration: 1.35
28:     electricLineCount: 3
29:     electricLineWidth: 1.45
30:     electricDamage: 2
31:     reversedControlDuration: 1
32:     missileWarningDuration: 1.15
33:     missileCount: 12
34:     missileInterval: 0.11
35:     missileSpeed: 7.8
36:     missileDamage: 1
37:     missileTurnSpeed: 115
38:     bombWarningDuration: 1.45
39:     bombCount: 6
40:     bombRadius: 2.35
41:     bombDamage: 2
42:     sweepWarningDuration: 1.55
43:     sweepDamage: 2
44:     reflectionWarningDuration: 1.1
45:     reflectionActiveDuration: 3.2
46:     reflectedDamageRatio: 0.55
47:     barrageWarningDuration: 1.2
48:     barrageCellsPerWave: 24
49:     barrageWaveCount: 3
50:     barrageWaveInterval: 0.65
51:     barrageCellSpeed: 6.4
52:     barrageCellDamage: 1
53:     phaseThreeHealthRatio: 0.4
54:   phaseThree:
55:     detachDuration: 1.8
56:     pursuitSpeed: 3.1
57:     stoppingDistance: 3.2
58:     collapseDuration: 45
59:     patternRecovery: 1.8
60:     dashDamage: 2
61:     grabDamage: 2
62:     tentacleDamage: 2
63:     fallingCellDamage: 1
64:     fallingBombDamage: 2
65:     boundaryDamage: 1
66:     overtimeDamage: 2
67:     collapseHealthRatio: 0.1
68:   normal:
69:     health: 1
70:     damage: 1
71:     summonMoveSpeed: 1
72:     summonCooldown: 1
73:     summonExperience: 1
74:   hard:
75:     health: 1
76:     damage: 1
77:     summonMoveSpeed: 1
78:     summonCooldown: 1
79:     summonExperience: 1
```

## Assets/_Project/Resources/Balance/LevelProgressionConfig.asset

```yaml
4: MonoBehaviour:
15:   maxLevel: 30
16:   baseExp: 30
17:   expIncreasePerLevel: 20
18:   enemyKillExp: 10
19:   earlyLevelUpperBound: 9
20:   earlyExpGainMultiplier: 2
21:   jobSelectionBlockedExpMultiplier: 0
22:   jobSelectionSelectedExpMultiplier: 1
23:   midLevelUpperBound: 20
24:   midExpGainMultiplier: 1
25:   lateExpGainMultiplier: 0.8
26:   jobSelectionLevel: 10
27:   jobBasedChoiceStartLevel: 11
28:   skill1UnlockLevel: 10
29:   skill2UnlockLevel: 20
30:   playerBaseStats:
31:   - statType: 0
32:     value: 10
33:   - statType: 1
34:     value: 10
35:   - statType: 2
36:     value: 1
37:   - statType: 4
38:     value: 1
39:   - statType: 5
40:     value: 1
41:   - statType: 6
42:     value: 1
43:   - statType: 7
44:     value: 0
45:   levelUpStatValues:
46:   - statType: 2
47:     mode: 0
48:     value: 1
49:     healWhenPositive: 0
50:   - statType: 5
51:     mode: 1
52:     value: 1
53:     healWhenPositive: 0
54:   - statType: 4
55:     mode: 1
56:     value: 1
57:     healWhenPositive: 0
58:   - statType: 6
59:     mode: 0
60:     value: 1
61:     healWhenPositive: 0
62:   - statType: 1
63:     mode: 1
64:     value: 1
65:     healWhenPositive: 0
66:   - statType: 0
67:     mode: 0
68:     value: 1
69:     healWhenPositive: 1
70:   bioGambleEnabled: 1
71:   bioGambleMinDelta: -1
72:   bioGambleMaxDelta: 3
```

## Assets/_Project/Resources/Balance/MonsterBalanceCatalog.asset

```yaml
4: MonoBehaviour:
15:   difficultyCatalog: {fileID: 11400000, guid: 86c1087c7b1646a9ab1b7155c3b9d611, type: 2}
16:   progression: {fileID: 11400000, guid: d9493964a6e88459da41ac326520adaa, type: 2}
17:   biomeEliteSpawns:
22:   monsters:
```

## Assets/_Project/Scripts/Core/Balance/MonsterBalanceCatalog.cs

```csharp
28: if (monsters == null || monsters.Count == 0) errors.Add("몬스터 정의가 없습니다.");
```

## Assets/_Project/Scripts/Core/Balance/MonsterBalanceResolver.cs

```csharp
14: public float HealthUnits => Mathf.Max(1f, CharacterStats.ToHealthUnits(MaxHealth));
60: || !MonsterBalanceNumbers.Positive(CooldownMultiplier) || Experience < 0)
95: return "난이도 배율에 음수, 0 이하 체력/속도/대기 배율 또는 NaN/Infinity가 있습니다.";
119: error = "진행도 범위는 0~4입니다.";
129: float contact = definition.contact.enabled ? attack * definition.contact.damageCoefficient : 0f;
```

## Assets/_Project/Scripts/Core/Balance/MonsterBalanceRuntime.cs

```csharp
48: catalog.progression, inVisit ? visit.Stage : 0);
89: generation = source != null ? source.SpawnGeneration : 0;
```

## Assets/_Project/Scripts/Core/Balance/MonsterDefinition.cs

```csharp
7: public enum MonsterTier { Normal = 0, Elite = 1, Boss = 2 }
13: [Min(0.01f)] public float maxHealth = 30f;
14: [Min(0f)] public float attackPower = 2f;
15: [Min(0f)] public float moveSpeed = 1f;
21: [Min(0)] public int baseExperience = 50;
29: [Min(0f)] public float coefficient = 1f;
36: [Min(0f)] public float damageCoefficient = 0.5f;
37: [Min(0f)] public float knockbackDistance = 0.45f;
58: if (statSets == null || statSets.Count == 0) return "StatSet이 필요합니다.";
69: if (reward == null || reward.baseExperience < 0) return "기본 경험치가 잘못되었습니다.";
102: internal static bool NonNegative(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
103: internal static bool Positive(float value) => NonNegative(value) && value > 0f;
```

## Assets/_Project/Scripts/Core/Balance/MonsterProgressionProfile.cs

```csharp
10: [Range(0, 4)] public int stage;
11: [Min(0.01f)] public float maxHealth = 1f;
12: [Min(0f)] public float attackPower = 1f;
13: [Min(0f)] public float experience = 1f;
19: public const int MaximumStage = 4;
26: new MonsterProgressionStage { stage = 0 },
27: new MonsterProgressionStage { stage = 1, maxHealth = 1.15f, attackPower = 1.08f, experience = 1.10f },
28: new MonsterProgressionStage { stage = 2, maxHealth = 1.35f, attackPower = 1.20f, experience = 1.25f },
29: new MonsterProgressionStage { stage = 3, maxHealth = 1.60f, attackPower = 1.35f, experience = 1.40f },
30: new MonsterProgressionStage { stage = 4, maxHealth = 1.85f, attackPower = 1.50f, experience = 1.60f }
36: if (stages == null || stages.Count != MaximumStage + 1) return "진행도 0~4를 각각 한 번 정의해야 합니다.";
40: if (value == null || value.stage < 0 || value.stage > MaximumStage || !seen.Add(value.stage))
45: return "진행도 배율은 유한한 값이어야 하며 체력은 양수, 공격력/경험치는 0 이상이어야 합니다.";
```

## Assets/_Project/Scripts/Core/Difficulty/DifficultyBalanceProfile.cs

```csharp
14: [TextArea(2, 4)] public string description;
48: for (int index = 0; index < biomeOverrides.Count; index++)
65: for (int index = 0; index < biomeWorldOverrides.Count; index++)
83: [Min(0.01f)] public float maxHealth = 1f;
84: [Min(0.01f)] public float moveSpeed = 1f;
85: [Min(0.01f)] public float attackPower = 1f;
86: [Min(0.01f)] public float attackSpeed = 1f;
87: [Min(0.01f)] public float attackRange = 1f;
88: [Min(0f)] public float magic = 1f;
89: [Min(0f)] public float skillCooldownReduction = 1f;
92: [Min(0f)] public float basicAttackDamage = 1f;
93: [Min(0.01f)] public float basicAttackCooldown = 1f;
94: [Min(0f)] public float basicAttackRange = 1f;
95: [Min(0f)] public float skillDamage = 1f;
96: [Min(0.01f)] public float skillCooldown = 1f;
102: CharacterStatType.MaxHealth => Mathf.Max(0.01f, maxHealth),
103: CharacterStatType.MoveSpeed => Mathf.Max(0.01f, moveSpeed),
104: CharacterStatType.AttackPower => Mathf.Max(0.01f, attackPower),
105: CharacterStatType.AttackSpeed => Mathf.Max(0.01f, attackSpeed),
106: CharacterStatType.AttackRange => Mathf.Max(0.01f, attackRange),
107: CharacterStatType.Magic => Mathf.Max(0f, magic),
108: CharacterStatType.SkillCooldownReduction => Mathf.Max(0f, skillCooldownReduction),
109: _ => 1f
117: [Min(0.01f)] public float maxHealth = 1f;
118: [Min(0.01f)] public float moveSpeed = 1f;
119: [Min(0f)] public float outgoingDamage = 1f;
120: [Min(0.01f)] public float attackCooldown = 1f;
121: [Min(0f)] public float experienceReward = 1f;
127: [Tooltip("0이면 PlayerItemManager에 설정된 슬롯 수를 사용합니다.")]
128: [Min(0)] public int maxSlotsOverride;
129: [Min(0f)] public float worldSpawnCount = 1f;
135: [Min(0f)] public float enemySpawnerDensity = 1f;
136: [Min(0f)] public float sceneObjectDensity = 1f;
137: [Min(0f)] public float enemyMaxAlive = 1f;
138: [Min(0.01f)] public float enemyRespawnCooldown = 1f;
```

## Assets/_Project/Scripts/Core/Difficulty/DifficultyBalanceService.cs

```csharp
40: return player != null ? player.GetBaseStatMultiplier(statType) : 1f;
57: return Mathf.Max(0f, GetEnemyBalance(boss).outgoingDamage);
```

## Assets/_Project/Scripts/Core/Stats/CharacterStats.cs

```csharp
10: MaxHealth = 0,               // 최대 체력
11: MoveSpeed = 1,               // 이동 속도
12: AttackPower = 2,             // 기본공격 공격력
14: Defense = 3,
15: AttackSpeed = 4,             // 기본공격 공격 속도
16: AttackRange = 5,             // 기본공격 사거리
17: Magic = 6,                   // 스킬 데미지 증가율(%)
18: SkillCooldownReduction = 7,  // 스킬 쿨타임 감소율(%)
29: Flat,             // 고정값 덧셈 (예: +10)
30: PercentAdd,       // 기본값 기준 퍼센트 합산 (예: +10%, +5% → 총 +15%)
31: PercentMultiply   // 최종값에 곱셈 (예: ×1.1)
113: public float NormalizedCurrent => MaxValue <= 0f ? 0f : CurrentValue / MaxValue; // 0~1 정규화 HP
130: private const float HealthUnitEpsilon = 0.0001f;
151: public float Defense => 0f;
157: public float HealthNormalized => MaxHealth <= 0f ? 0f : currentHealth / MaxHealth; // HP 비율 (0~1)
158: public bool IsDead => currentHealth <= 0f;                                         // 사망 여부
213: return baseStatValues.TryGetValue(statType, out float value) ? value : 0f;
219: return finalStatValues.TryGetValue(statType, out float value) ? value : 0f;
231: int index = 0;
264: return 0;
270: int removedCount = 0;
271: for (int i = modifiers.Count - 1; i >= 0; i--)
281: if (removedCount <= 0)
283: return 0;
294: if (modifiers.Count == 0)
310: if (damage <= 0f || MaxHealth <= 0f)
312: return 0f;
317: currentHealth = Mathf.Max(0f, previousCurrentHealth - damageUnits);
325: if (amount <= 0f || MaxHealth <= 0f)
327: return 0f;
348: float minimumAliveHealth = MaxHealth > 0f ? Mathf.Min(1f, MaxHealth) : 0f;
362: return Mathf.Clamp(ToHealthUnits(value), 0f, MaxHealth);
367: if (value <= 0f)
369: return 0f;
372: return Mathf.Floor(value + 0.5f + HealthUnitEpsilon);
377: if (value <= 0f)
379: return 0f;
382: return Mathf.Max(1f, Mathf.Floor(value + 0.5f + HealthUnitEpsilon));
417: for (int i = 0; i < modifiers.Count; i++)
429: float previousValue = previousStatValues.TryGetValue(statType, out float cachedValue) ? cachedValue : 0f;
438: if (finalStatValues.ContainsKey(pair.Key) || Mathf.Approximately(pair.Value, 0f))
443: StatChanged?.Invoke(this, new CharacterStatChangedEventArgs(pair.Key, pair.Value, 0f));
450: float value = GetBaseValue(statType);    // 1단계: 기본값
451: float additivePercent = 0f;              // PercentAdd 합계 (여러 개가 합산됨)
452: float multiplicativePercent = 1f;        // PercentMultiply 누적곱
454: for (int i = 0; i < modifiers.Count; i++)
464: case CharacterStatModifierMode.Flat:          // 2단계: 고정값 합산
467: case CharacterStatModifierMode.PercentAdd:    // 3단계: 퍼센트 합산 (나중에 한번에 적용)
470: case CharacterStatModifierMode.PercentMultiply: // 4단계: 곱셈 누적 (나중에 한번에 적용)
471: multiplicativePercent *= 1f + modifier.Value;
476: value *= 1f + additivePercent;    // PercentAdd 적용 (예: 기본100, +10% → 110)
477: value *= multiplicativePercent;  // PercentMultiply 적용 (예: 110 × 1.2 → 132)
478: return Mathf.Max(0f, value);     // 음수 방지
```

## Assets/_Project/Scripts/Gameplay/Effects/BossEncounterVfx.cs

```csharp
8: private const int GroundSortingOrder = 5300;
9: private const int DetailSortingOrder = 5330;
10: private const int ParticleSortingOrder = 5360;
11: private const int DetailCount = 10;
12: private const int RayCount = 8;
26: private Vector3 sourceBaseScale;
27: private Vector3 centerOffset;
53: GroundSortingOrder + 1);
58: GroundSortingOrder + 2);
63: GroundSortingOrder + 3);
65: for (int i = 0; i < RayCount; i++)
69: $"BossGroundRay{i + 1}",
71: GroundSortingOrder + 4 + i);
74: for (int i = 0; i < DetailCount; i++)
78: $"BossDetail{i + 1}",
87: DetailSortingOrder + DetailCount + 2);
92: DetailSortingOrder + DetailCount + 3);
101: Vector3 groundPosition,
102: Vector3 center,
118: effectScale = Mathf.Clamp(scale * 1.22f, 1.15f, 4.2f);
119: duration = biome == BiomeType.Liver ? 2.25f : biome == BiomeType.Lung ? 2.1f : 2.05f;
120: elapsed = 0f;
121: angleSeed = Random.Range(0f, 360f);
129: : Vector3.one * effectScale;
142: float t = Mathf.Clamp01(elapsed / duration);
144: if (shouldShakeCamera && !cameraShakePlayed && t >= 0.22f)
147: DontStarveCamera.Instance?.AddCombatImpulse(0.34f, 0.42f);
171: if (t >= 1f)
180: groundMark.transform.localPosition = Vector3.up * 0.022f;
181: groundMark.transform.localScale = Vector3.one * 0.04f;
182: groundMark.color = WithAlpha(primaryColor, 0f);
185: groundAccent.transform.localPosition = Vector3.up * 0.034f;
186: groundAccent.transform.localScale = Vector3.one * 0.04f;
187: groundAccent.color = WithAlpha(accentColor, 0f);
190: shockwave.transform.localPosition = Vector3.up * 0.046f;
191: shockwave.transform.localScale = Vector3.one * 0.04f;
192: shockwave.color = WithAlpha(accentColor, 0f);
195: secondaryShockwave.transform.localPosition = Vector3.up * 0.052f;
196: secondaryShockwave.transform.localScale = Vector3.one * 0.04f;
197: secondaryShockwave.transform.rotation = GroundRotation(0f);
198: secondaryShockwave.color = WithAlpha(accentColor, 0f);
200: for (int i = 0; i < groundRays.Length; i++)
209: ray.transform.localPosition = Vector3.up * (0.056f + i * 0.001f);
210: ray.transform.localScale = Vector3.one * 0.02f;
211: ray.transform.rotation = GroundRotation(i * (360f / RayCount));
212: ray.color = WithAlpha(i % 2 == 0 ? primaryColor : accentColor, 0f);
216: bossEcho.color = WithAlpha(primaryColor, 0f);
218: bossFlash.color = WithAlpha(Color.white, 0f);
220: for (int i = 0; i < details.Length; i++)
230: detail.transform.localScale = Vector3.one * 0.02f;
232: detail.color = WithAlpha(primaryColor, 0f);
246: bossEcho.sortingOrder = sourceRenderer.sortingOrder + 18;
252: bossFlash.sortingOrder = sourceRenderer.sortingOrder + 19;
254: SyncBossOverlay(bossEcho, 1f);
255: SyncBossOverlay(bossFlash, 1f);
265: for (int i = 0; i < DetailCount; i++)
267: EnableDetail(i, i < 8
276: for (int i = 0; i < 8; i++)
280: EnableDetail(8, CombatVfxResources.GetRuneSprite());
281: EnableDetail(9, CombatVfxResources.GetStarSprite());
287: for (int i = 0; i < 4; i++)
291: for (int i = 4; i < DetailCount; i++)
300: for (int i = 0; i < 7; i++)
304: for (int i = 7; i < DetailCount; i++)
313: EnableDetail(0, CombatVfxResources.GetStarSprite());
338: float anticipation = Mathf.Clamp01(t / 0.24f);
339: float echoBurstT = Mathf.Clamp01((t - 0.18f) / 0.5f);
341: float presenceFade = Mathf.Clamp01((0.84f - t) / 0.3f);
342: float echoScale = Mathf.Lerp(0.92f, 1.08f, anticipation)
343: + echoBurst * 0.22f;
345: Color presenceColor = Color.Lerp(primaryColor, accentColor, 0.62f);
348: presenceFade * (anticipation * 0.24f + echoBurst * 0.48f));
350: float flashT = Mathf.Clamp01((t - 0.22f) / 0.24f);
352: SyncBossOverlay(bossFlash, Mathf.Lerp(0.96f, 1.1f, flashPulse));
354: Color.Lerp(Color.white, accentColor, 0.2f),
355: flashPulse * 0.72f);
360: float secondWaveT = Mathf.Clamp01((t - 0.34f) / 0.4f);
361: secondaryShockwave.transform.localScale = Vector3.one
363: * Mathf.Lerp(0.12f, 2.38f, EaseOut(secondWaveT));
364: secondaryShockwave.transform.rotation = GroundRotation(t * -35f);
366: Color.Lerp(primaryColor, accentColor, 0.58f),
367: Mathf.Sin(secondWaveT * Mathf.PI) * 0.64f);
369: for (int i = 0; i < groundRays.Length; i++)
377: float delay = 0.18f + (i % 2) * 0.035f;
378: float rayT = Mathf.Clamp01((t - delay) / 0.48f);
381: float angle = angleSeed + i * (360f / RayCount) + (i % 2) * 11f;
382: float radians = angle * Mathf.Deg2Rad;
383: Vector3 radial = new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
384: float length = effectScale * Mathf.Lerp(0.08f, 1.35f + (i % 3) * 0.18f, burst);
385: ray.transform.localPosition = radial * length * 0.48f
386: + Vector3.up * (0.056f + i * 0.001f);
387: ray.transform.localScale = new Vector3(
388: effectScale * Mathf.Lerp(0.075f, 0.025f, burst),
390: 1f);
393: i % 2 == 0 ? primaryColor : accentColor,
394: alpha * (0.68f - (i % 3) * 0.08f));
408: Vector3 liveScale = sourceTransform.lossyScale;
409: if (liveScale.sqrMagnitude <= 0.0001f)
418: float envelope = FadeEnvelope(t, 0.1f, 0.2f);
419: float spread = 1f - Mathf.Pow(1f - t, 3f);
421: groundMark.transform.localScale = Vector3.one
423: * Mathf.Lerp(0.08f, 1.38f, spread);
424: groundMark.transform.rotation = GroundRotation(12f);
425: groundMark.color = WithAlpha(primaryColor, envelope * (1f - t * 0.55f) * 0.72f);
427: float crackT = Mathf.Clamp01((t - 0.14f) / 0.62f);
428: groundAccent.transform.localScale = Vector3.one
430: * Mathf.Lerp(0.04f, 1.08f, EaseOut(crackT));
431: groundAccent.transform.rotation = GroundRotation(74f);
432: groundAccent.color = WithAlpha(accentColor, Mathf.Sin(crackT * Mathf.PI) * 0.62f);
433: UpdateExpandingShockwave(t, 0.28f, 0.46f, 1.72f, primaryColor, 0.66f);
435: for (int i = 0; i < 8; i++)
438: float delay = i * 0.025f;
439: float sporeT = Mathf.Clamp01((t - delay) / 0.82f);
440: float angle = (angleSeed + i * 137.5f) * Mathf.Deg2Rad;
441: Vector3 radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
442: float radius = effectScale * Mathf.Lerp(0.16f, 0.82f, EaseOut(sporeT));
445: + Vector3.up * effectScale * Mathf.Lerp(-0.34f, 1.18f + (i % 3) * 0.18f, sporeT);
447: spore.transform.localScale = Vector3.one
449: * Mathf.Lerp(0.07f, 0.2f + (i % 2) * 0.06f, pulse);
451: Color sporeColor = i % 3 == 0
452: ? new Color(1f, 0.94f, 0.58f, 1f)
453: : i % 3 == 1 ? accentColor : Color.Lerp(primaryColor, accentColor, 0.4f);
454: spore.color = WithAlpha(sporeColor, pulse * 0.9f);
457: for (int i = 8; i < DetailCount; i++)
460: float globT = Mathf.Clamp01((t - 0.12f - (i - 8) * 0.08f) / 0.58f);
461: float side = i == 8 ? -1f : 1f;
463: + GetCameraRight() * side * effectScale * Mathf.Lerp(0.05f, 0.72f, EaseOut(globT))
464: + Vector3.up * effectScale * Mathf.Sin(globT * Mathf.PI) * 0.48f;
465: glob.transform.localScale = Vector3.one * effectScale * Mathf.Lerp(0.08f, 0.3f, Mathf.Sin(globT * Mathf.PI));
466: glob.transform.rotation = GetCameraRotation() * Quaternion.Euler(0f, 0f, side * 30f);
467: glob.color = WithAlpha(primaryColor, Mathf.Sin(globT * Mathf.PI) * 0.78f);
473: float envelope = FadeEnvelope(t, 0.12f, 0.22f);
474: float poolSpread = EaseOut(Mathf.Clamp01(t / 0.72f));
475: groundMark.transform.localScale = Vector3.one
477: * Mathf.Lerp(0.08f, 1.48f, poolSpread);
478: groundMark.transform.rotation = GroundRotation(-18f);
479: groundMark.color = WithAlpha(primaryColor, envelope * 0.68f);
481: float runePulse = Mathf.Sin(Mathf.Clamp01(t / 0.78f) * Mathf.PI);
482: groundAccent.transform.localScale = Vector3.one
484: * Mathf.Lerp(0.22f, 1.08f, runePulse);
485: groundAccent.transform.rotation = GroundRotation(t * 110f);
486: groundAccent.color = WithAlpha(accentColor, runePulse * envelope * 0.78f);
487: UpdateExpandingShockwave(t, 0.42f, 0.42f, 1.65f, accentColor, 0.72f);
489: for (int i = 0; i < 8; i++)
492: float delay = i * 0.018f;
493: float dropT = Mathf.Clamp01((t - delay) / 0.76f);
494: float angle = (angleSeed + i * 45f) * Mathf.Deg2Rad;
495: float radius = effectScale * Mathf.Lerp(1.0f, 0.13f, EaseOut(dropT));
496: Vector3 radial = GetCameraRight() * Mathf.Cos(angle)
497: + Vector3.up * Mathf.Sin(angle);
500: + Vector3.up * effectScale * 0.16f;
502: drop.transform.localScale = new Vector3(
503: effectScale * Mathf.Lerp(0.09f, 0.18f, pulse),
504: effectScale * Mathf.Lerp(0.15f, 0.3f, pulse),
505: 1f);
507: * Quaternion.Euler(0f, 0f, -angle * Mathf.Rad2Deg - 90f);
508: drop.color = WithAlpha(Color.Lerp(primaryColor, new Color(1f, 0.08f, 0.12f, 1f), i / 7f), pulse * 0.95f);
511: SpriteRenderer bodyRune = details[8];
512: float bodyRuneT = Mathf.Clamp01((t - 0.28f) / 0.62f);
514: bodyRune.transform.localPosition = centerOffset + Vector3.up * effectScale * 0.08f;
515: bodyRune.transform.localScale = Vector3.one * effectScale * Mathf.Lerp(0.12f, 0.82f, bodyRunePulse);
516: bodyRune.transform.rotation = GetCameraRotation() * Quaternion.Euler(0f, 0f, t * -210f);
517: bodyRune.color = WithAlpha(accentColor, bodyRunePulse * 0.72f);
519: SpriteRenderer bloodCore = details[9];
520: float coreT = Mathf.Clamp01((t - 0.44f) / 0.38f);
522: bloodCore.transform.localPosition = centerOffset + Vector3.up * effectScale * 0.08f;
523: bloodCore.transform.localScale = Vector3.one * effectScale * Mathf.Lerp(0.03f, 0.72f, corePulse);
524: bloodCore.transform.rotation = GetCameraRotation() * Quaternion.Euler(0f, 0f, coreT * 140f);
525: bloodCore.color = WithAlpha(new Color(1f, 0.82f, 0.9f, 1f), corePulse);
530: float envelope = FadeEnvelope(t, 0.1f, 0.2f);
531: float acidSpread = EaseOut(Mathf.Clamp01(t / 0.72f));
532: groundMark.transform.localScale = Vector3.one
534: * Mathf.Lerp(0.08f, 1.34f, acidSpread);
535: groundMark.transform.rotation = GroundRotation(24f);
536: groundMark.color = WithAlpha(primaryColor, envelope * 0.7f);
538: groundAccent.transform.localScale = Vector3.one
540: * Mathf.Lerp(0.04f, 0.9f, acidSpread);
541: groundAccent.transform.rotation = GroundRotation(-58f);
542: groundAccent.color = WithAlpha(Color.Lerp(primaryColor, accentColor, 0.35f), envelope * 0.5f);
544: float suctionT = Mathf.Clamp01(t / 0.56f);
545: shockwave.transform.localScale = Vector3.one
547: * Mathf.Lerp(1.72f, 0.08f, EaseOut(suctionT));
548: shockwave.transform.rotation = GroundRotation(t * -100f);
549: shockwave.color = WithAlpha(primaryColor, Mathf.Sin(suctionT * Mathf.PI) * 0.76f);
551: for (int i = 0; i < 4; i++)
554: float delay = (i % 2) * 0.045f;
555: float biteT = Mathf.Clamp01((t - delay) / 0.58f);
556: float side = i % 2 == 0 ? -1f : 1f;
557: bool upper = i < 2;
558: float vertical = upper ? 1f : -1f;
560: + GetCameraRight() * side * effectScale * Mathf.Lerp(0.82f, 0.24f, EaseOut(biteT))
561: + Vector3.up * vertical * effectScale * Mathf.Lerp(0.72f, 0.26f, EaseOut(biteT));
563: fang.transform.localScale = new Vector3(
564: effectScale * 0.28f,
565: effectScale * Mathf.Lerp(0.42f, 0.72f, bitePulse),
566: 1f);
567: float rotation = upper ? side * 22f : 180f - side * 22f;
568: fang.transform.rotation = GetCameraRotation() * Quaternion.Euler(0f, 0f, rotation);
569: fang.color = WithAlpha(i % 2 == 0 ? accentColor : new Color(1f, 0.94f, 0.72f, 1f), bitePulse * 0.95f);
572: for (int i = 4; i < DetailCount; i++)
575: float delay = (i - 4) * 0.035f;
576: float bubbleT = Mathf.Clamp01((t - delay) / 0.8f);
577: float angle = (angleSeed + (i - 4) * 60f) * Mathf.Deg2Rad;
578: float radius = effectScale * Mathf.Lerp(1.05f, 0.08f, EaseOut(bubbleT));
581: + Vector3.up * (Mathf.Sin(angle) * radius * 0.55f + effectScale * 0.12f);
583: bubble.transform.localScale = Vector3.one
585: * Mathf.Lerp(0.08f, 0.22f + (i % 2) * 0.08f, pulse);
587: bubble.color = WithAlpha(Color.Lerp(primaryColor, new Color(0.9f, 1f, 0.4f, 1f), (i - 4) / 5f), pulse * 0.82f);
593: float envelope = FadeEnvelope(t, 0.08f, 0.2f);
595: groundMark.transform.localScale = Vector3.one
597: * Mathf.Lerp(0.18f, 1.22f, expand);
598: groundMark.transform.rotation = GroundRotation(t * 220f);
599: groundMark.color = WithAlpha(primaryColor, envelope * 0.62f);
601: groundAccent.transform.localScale = Vector3.one
603: * Mathf.Lerp(0.12f, 0.9f, expand);
604: groundAccent.transform.rotation = GroundRotation(-t * 310f + 180f);
605: groundAccent.color = WithAlpha(accentColor, envelope * 0.5f);
606: UpdateExpandingShockwave(t, 0.24f, 0.54f, 1.58f, primaryColor, 0.5f);
609: for (int i = 0; i < 7; i++)
612: float delay = i * 0.028f;
613: float arcT = Mathf.Clamp01((t - delay) / 0.76f);
614: float direction = i % 2 == 0 ? 1f : -1f;
615: float angle = angleSeed + i * 51.4f + direction * arcT * 210f;
616: float radius = effectScale * Mathf.Lerp(0.12f, 0.78f, EaseOut(arcT));
617: float radians = angle * Mathf.Deg2Rad;
620: + Vector3.up * (Mathf.Sin(radians) * radius * 0.52f + effectScale * 0.12f);
622: arc.transform.localScale = Vector3.one
624: * Mathf.Lerp(0.15f, 0.46f + (i % 3) * 0.08f, pulse);
625: arc.transform.rotation = cameraRotation * Quaternion.Euler(0f, 0f, angle + direction * 70f);
626: arc.color = WithAlpha(i % 2 == 0 ? primaryColor : accentColor, pulse * 0.78f);
629: for (int i = 7; i < DetailCount; i++)
632: float puffT = Mathf.Clamp01((t - (i - 7) * 0.08f) / 0.82f);
633: float side = i % 2 == 0 ? -1f : 1f;
635: + GetCameraRight() * side * effectScale * Mathf.Lerp(0.12f, 1.18f, EaseOut(puffT))
636: + Vector3.up * effectScale * Mathf.Lerp(-0.08f, 0.58f + (i - 7) * 0.14f, puffT);
638: puff.transform.localScale = new Vector3(
639: effectScale * Mathf.Lerp(0.1f, 0.42f, pulse),
640: effectScale * Mathf.Lerp(0.08f, 0.24f, pulse),
641: 1f);
643: puff.color = WithAlpha(Color.Lerp(primaryColor, Color.white, 0.55f), pulse * 0.56f);
649: float envelope = FadeEnvelope(t, 0.1f, 0.2f);
650: groundMark.transform.localScale = Vector3.one * effectScale * Mathf.Lerp(0.08f, 1.2f, EaseOut(t));
651: groundMark.transform.rotation = GroundRotation(t * 90f);
652: groundMark.color = WithAlpha(primaryColor, envelope * 0.68f);
653: groundAccent.transform.localScale = Vector3.one * effectScale * Mathf.Lerp(0.04f, 1f, EaseOut(t));
654: groundAccent.transform.rotation = GroundRotation(45f);
655: groundAccent.color = WithAlpha(accentColor, envelope * 0.6f);
656: UpdateExpandingShockwave(t, 0.2f, 0.5f, 1.6f, accentColor, 0.72f);
667: float waveT = Mathf.Clamp01((t - delay) / span);
668: shockwave.transform.localScale = Vector3.one
670: * Mathf.Lerp(0.05f, maxSize, EaseOut(waveT));
671: shockwave.transform.rotation = GroundRotation(0f);
675: private void EmitBiomeParticles(Vector3 groundPosition, BiomeType biome)
684: int count = biome == BiomeType.Lung ? 88 : biome == BiomeType.Liver ? 76 : 68;
685: for (int i = 0; i < count; i++)
687: Vector3 radial = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
688: Vector3 velocity;
689: Vector3 position;
695: velocity = radial * Random.Range(0.25f, 0.9f) * effectScale
696: + Vector3.up * Random.Range(0.8f, 2.1f) * effectScale;
697: position = groundPosition + radial * Random.Range(0.08f, 0.62f) * effectScale;
698: size = Random.Range(0.07f, 0.18f);
701: velocity = radial * Random.Range(0.45f, 1.35f) * effectScale
702: + Vector3.up * Random.Range(0.3f, 1.15f) * effectScale;
703: position = groundPosition + centerOffset + radial * Random.Range(0.05f, 0.3f) * effectScale;
704: size = Random.Range(0.055f, 0.15f);
707: velocity = radial * Random.Range(0.22f, 0.78f) * effectScale
708: + Vector3.up * Random.Range(0.55f, 1.7f) * effectScale;
709: position = groundPosition + radial * Random.Range(0.1f, 0.72f) * effectScale;
710: size = Random.Range(0.08f, 0.2f);
713: velocity = radial * Random.Range(1.4f, 3.2f) * effectScale
714: + Vector3.up * Random.Range(0.08f, 0.5f) * effectScale;
715: position = groundPosition + centerOffset * 0.45f + radial * Random.Range(0.05f, 0.35f) * effectScale;
716: size = Random.Range(0.045f, 0.12f);
719: velocity = radial * Random.Range(0.6f, 1.6f) * effectScale + Vector3.up;
721: size = 0.1f;
727: position = position + Vector3.up * Random.Range(0.04f, 0.18f),
729: startColor = Color.Lerp(primaryColor, accentColor, Random.Range(0.08f, 0.92f)),
730: startLifetime = Random.Range(0.68f, 1.28f),
732: rotation = Random.Range(0f, 360f)
734: particles.Emit(parameters, 1);
740: if (index < 0 || index >= details.Length || details[index] == null)
783: for (int i = 0; i < DetailCount; i++)
787: details[i] = transform.Find($"BossDetail{i + 1}")?.GetComponent<SpriteRenderer>();
790: for (int i = 0; i < RayCount; i++)
794: groundRays[i] = transform.Find($"BossGroundRay{i + 1}")?.GetComponent<SpriteRenderer>();
823: main.maxParticles = 128;
824: main.startSpeed = 0f;
825: main.startLifetime = 1f;
826: main.startSize = 0.15f;
827: main.gravityModifier = 0f;
834: 1f,
836: new Keyframe(0f, 0f),
837: new Keyframe(0.12f, 1f),
838: new Keyframe(0.72f, 0.76f),
839: new Keyframe(1f, 0f)));
852: return Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
858: return camera != null ? camera.transform.rotation : Quaternion.Euler(45f, 0f, 0f);
861: private static Vector3 GetCameraRight()
864: return camera != null ? camera.transform.right : Vector3.right;
869: return Mathf.Clamp01(t / Mathf.Max(0.01f, fadeIn))
870: * Mathf.Clamp01((1f - t) / Mathf.Max(0.01f, fadeOut));
875: return 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
880: color.a = Mathf.Clamp01(alpha);
```

## Assets/_Project/Scripts/Gameplay/Effects/CombatDashTrail.cs

```csharp
9: private const float SpawnInterval = 0.035f;
12: private Vector3 dashDirection;
16: public void Begin(SpriteRenderer source, Vector3 direction, float duration)
25: dashDirection.y = 0f;
26: if (dashDirection.sqrMagnitude > 0.0001f)
31: emitUntil = Time.unscaledTime + Mathf.Max(0.05f, duration);
66: ghost.Show(sourceRenderer, -dashDirection * 0.45f + Vector3.up * 0.12f, 0.19f);
74: private Vector3 drift;
88: public void Show(SpriteRenderer source, Vector3 worldDrift, float lifeTime)
111: spriteRenderer.sortingOrder = source.sortingOrder - 1;
114: startColor = Color.Lerp(source.color, new Color(0.85f, 0.12f, 0.32f, 1f), 0.58f);
115: startColor.a = 0.38f;
120: duration = Mathf.Max(0.05f, lifeTime);
121: elapsed = 0f;
128: float t = Mathf.Clamp01(elapsed / duration);
130: transform.localScale *= 1f + Time.unscaledDeltaTime * 0.5f;
133: color.a *= Mathf.Pow(1f - t, 1.4f);
135: if (t >= 1f)
147: elapsed = 0f;
```

## Assets/_Project/Scripts/Gameplay/Effects/CombatHitFlash.cs

```csharp
9: private readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>(4);
10: private readonly List<Color> originalColors = new List<Color>(4);
21: restoreAt = Mathf.Max(restoreAt, Time.unscaledTime + Mathf.Max(0.02f, duration));
25: for (int i = 0; i < renderers.Count; i++)
34: Color applied = Color.Lerp(original, flashColor, 0.82f);
58: for (int i = 0; i < renderers.Count; i++)
67: for (int i = 0; i < count; i++)
76: restoreAt = 0f;
```

## Assets/_Project/Scripts/Gameplay/Effects/CombatImpactVfx.cs

```csharp
8: private const int FragmentSortingOrder = 5200;
9: private const int MistSortingOrder = 5180;
10: private const int CoreSortingOrder = 5190;
11: private const int RingSortingOrder = 5170;
41: Vector3 position,
42: Vector3 direction,
57: duration = Mathf.Max(0.08f, lifeTime);
58: elapsed = 0f;
59: effectScale = Mathf.Max(0.08f, scale);
65: core.color = WithAlpha(primary, Mathf.Max(0.6f, primary.a));
66: core.transform.localScale = Vector3.one * effectScale * 0.22f;
69: ring.color = WithAlpha(secondary, Mathf.Max(0.5f, secondary.a));
70: ring.transform.localScale = Vector3.one * effectScale * 0.3f;
72: EmitFragments(position, direction, primary, secondary, Mathf.Max(0, fragmentCount), directionalBias);
73: EmitMist(position, primary, secondary, Mathf.Max(0, mistCount));
82: float t = Mathf.Clamp01(elapsed / duration);
83: float fastT = 1f - Mathf.Pow(1f - t, 3f);
88: float pulse = Mathf.Sin(Mathf.Clamp01(t * 1.7f) * Mathf.PI);
89: core.transform.localScale = Vector3.one * effectScale * Mathf.Lerp(0.18f, 0.72f, fastT);
90: core.color = WithAlpha(coreColor, pulse * coreColor.a * 0.8f);
95: ring.transform.localScale = Vector3.one * effectScale * Mathf.Lerp(0.28f, 1.25f, fastT);
96: ring.color = WithAlpha(ringColor, (1f - t) * ringColor.a);
99: if (t >= 1f)
106: Vector3 worldPosition,
107: Vector3 direction,
113: if (fragments == null || count <= 0)
121: Vector3 forward = direction;
122: forward.y = 0f;
123: bool hasDirection = forward.sqrMagnitude > 0.0001f;
129: float bias = Mathf.Clamp01(directionalBias);
130: for (int i = 0; i < count; i++)
132: float angle = Random.Range(-72f, 72f);
133: Vector3 radial = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
134: Vector3 biased = hasDirection
135: ? Quaternion.AngleAxis(angle, Vector3.up) * forward
137: Vector3 velocity = Vector3.Slerp(radial, biased, bias).normalized;
138: velocity *= Random.Range(1.1f, 3.8f) * effectScale;
139: velocity.y = Random.Range(0.2f, 1.25f) * Mathf.Sqrt(effectScale);
143: position = worldPosition + Random.insideUnitSphere * effectScale * 0.06f,
146: startLifetime = Random.Range(duration * 0.7f, duration * 1.25f),
147: startSize = Random.Range(0.06f, 0.18f) * effectScale,
148: rotation = Random.Range(0f, 360f)
150: fragments.Emit(parameters, 1);
154: private void EmitMist(Vector3 worldPosition, Color primary, Color secondary, int count)
156: if (mist == null || count <= 0)
164: for (int i = 0; i < count; i++)
166: Vector3 velocity = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
167: velocity *= Random.Range(0.15f, 0.8f) * effectScale;
168: velocity.y = Random.Range(0.05f, 0.4f);
170: Color color = Color.Lerp(primary, secondary, Random.Range(0.15f, 0.65f));
171: color.a = Mathf.Min(color.a, Random.Range(0.12f, 0.32f));
174: position = worldPosition + Random.insideUnitSphere * effectScale * 0.1f,
177: startLifetime = Random.Range(duration * 0.9f, duration * 1.45f),
178: startSize = Random.Range(0.24f, 0.55f) * effectScale,
179: rotation = Random.Range(0f, 360f)
181: mist.Emit(parameters, 1);
219: main.maxParticles = soft ? 24 : 64;
220: main.startSpeed = 0f;
221: main.startLifetime = 0.3f;
222: main.startSize = 0.1f;
223: main.gravityModifier = soft ? -0.015f : 0.12f;
231: 1f,
233: ? new AnimationCurve(new Keyframe(0f, 0.45f), new Keyframe(0.35f, 1f), new Keyframe(1f, 1.25f))
234: : new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.72f, 0.82f), new Keyframe(1f, 0f)));
240: new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
243: new GradientAlphaKey(0f, 0f),
244: new GradientAlphaKey(1f, soft ? 0.18f : 0.03f),
245: new GradientAlphaKey(0f, 1f)
275: return camera != null ? camera.transform.rotation : Quaternion.Euler(45f, 0f, 0f);
280: color.a = Mathf.Clamp01(alpha);
```

## Assets/_Project/Scripts/Gameplay/Effects/CombatVfx.cs

```csharp
17: private const string BossEncounterPoolName = "CombatVfx.BossEncounter.V3";
26: private static readonly Color BloodRed = new Color(0.96f, 0.035f, 0.11f, 0.92f);
27: private static readonly Color WarmCell = new Color(1f, 0.5f, 0.18f, 0.85f);
28: private static readonly Color NecroticPurple = new Color(0.38f, 0.035f, 0.28f, 0.86f);
29: private static readonly Color BoneWhite = new Color(1f, 0.93f, 0.72f, 0.95f);
33: if (enemy == null || !TryGetVisualContext(enemy.transform, out Vector3 center, out float scale))
43: Vector3 ground = enemy.transform.position + Vector3.up * 0.12f;
46: Vector3.zero,
47: scale * 0.7f,
48: new Color(0.55f, 0.08f, 0.28f, 0.48f),
49: new Color(0.94f, 0.3f, 0.34f, 0.55f),
50: 4,
51: 3,
52: 0.28f,
54: 0f);
59: if (enemy == null || !TryGetVisualContext(enemy.transform, out Vector3 center, out float targetScale))
69: flash.Flash(lethal ? BoneWhite : new Color(1f, 0.72f, 0.62f, 1f), lethal ? 0.1f : 0.065f);
76: Vector3 direction = Vector3.zero;
81: direction.y = 0f;
84: float damageScale = Mathf.Clamp(0.82f + Mathf.Log10(1f + Mathf.Max(0f, damage)) * 0.24f, 0.82f, 1.45f);
85: float scale = targetScale * damageScale * (lethal ? 1.3f : 0.78f);
90: lethal ? BloodRed : new Color(1f, 0.12f, 0.18f, 0.86f),
92: lethal ? 18 : 8,
93: lethal ? 7 : 3,
94: lethal ? 0.46f : 0.25f,
96: 0.78f);
98: AddCameraShake(lethal ? 0.16f : 0.045f, lethal ? 0.22f : 0.09f);
101: public static void PlayPlayerHit(Transform player, Vector3 sourcePosition, float damage, bool lethal)
103: if (player == null || !TryGetVisualContext(player, out Vector3 center, out float scale))
108: Vector3 direction = center - sourcePosition;
109: direction.y = 0f;
110: if (direction.sqrMagnitude <= 0.0001f)
112: direction = Vector3.back;
118: scale * (lethal ? 1.15f : 0.75f),
121: lethal ? 16 : 7,
122: lethal ? 7 : 3,
123: lethal ? 0.42f : 0.24f,
125: 0.82f);
127: float damageWeight = Mathf.Clamp01(Mathf.Log10(1f + Mathf.Max(0f, damage)) * 0.35f);
128: DamageVignetteOverlay.Pulse(lethal ? 0.42f : Mathf.Lerp(0.16f, 0.25f, damageWeight), lethal ? 0.55f : 0.28f);
129: AddCameraShake(lethal ? 0.25f : Mathf.Lerp(0.09f, 0.15f, damageWeight), lethal ? 0.32f : 0.18f);
132: public static void PlayMeleeSwing(Vector3 origin, Vector3 direction, float radius)
137: arc.Show(origin, direction, radius, 0.17f);
144: Vector3 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
146: origin + forward * Mathf.Min(radius * 0.42f, 0.9f) + Vector3.up * 0.35f,
148: Mathf.Clamp(radius * 0.22f, 0.35f, 0.8f),
151: 5,
152: 1,
153: 0.17f,
155: 0.92f);
158: public static void PlayRangedMuzzle(Vector3 origin, Vector3 direction)
163: 0.48f,
166: 7,
167: 2,
168: 0.19f,
170: 0.94f);
173: public static void PlayProjectileImpact(Vector3 position, Vector3 direction)
178: 0.38f,
180: new Color(1f, 0.18f, 0.2f, 0.82f),
181: 5,
182: 1,
183: 0.17f,
185: 0.9f);
188: public static void PlayHostileProjectileImpact(Vector3 position, Vector3 direction)
193: 0.52f,
196: 6,
197: 2,
198: 0.2f,
200: 0.72f);
203: public static void PlayDash(Transform player, SpriteRenderer sourceRenderer, Vector3 direction, float duration)
218: player.position + Vector3.up * 0.35f,
220: 0.6f,
221: new Color(1f, 0.72f, 0.62f, 0.7f),
222: new Color(0.75f, 0.04f, 0.22f, 0.72f),
223: 7,
224: 3,
225: 0.22f,
227: 0.88f);
231: Vector3 visualPosition,
241: Color accent = Color.Lerp(primary, BoneWhite, 0.72f);
242: Vector3 collectorCenter = collector.position + Vector3.up * 0.7f;
243: float collectorScale = 1f;
247: out Vector3 presentationCenter,
276: out Vector3 groundPosition,
277: out Vector3 center,
302: out Vector3 groundPosition,
303: out Vector3 center,
368: out Vector3 groundPosition,
369: out Vector3 center,
406: Vector3 position,
407: Vector3 direction,
432: Mathf.Clamp(scale, 0.08f, 4f),
442: private static bool TryGetVisualContext(Transform target, out Vector3 center, out float scale)
444: center = target != null ? target.position + Vector3.up * 0.7f : Vector3.zero;
445: scale = 1f;
455: float verticalSize = bounds.size.y * 0.55f;
456: scale = Mathf.Clamp(Mathf.Max(planarSize, verticalSize), 0.65f, 3.2f);
463: out Vector3 groundPosition,
464: out Vector3 center,
468: ? player.position + Vector3.up * 0.035f
469: : Vector3.zero;
470: center = groundPosition + Vector3.up * 0.7f;
471: scale = 1f;
488: scale = Mathf.Clamp(scale * 0.82f, 0.65f, 1.55f);
494: Vector3 localBottomLeft = new Vector3(
498: Vector3 localBottomRight = new Vector3(
502: Vector3 localTopCenter = new Vector3(
506: Vector3 localBottomCenter = new Vector3(
511: Vector3 worldBottomLeft = spriteTransform.TransformPoint(localBottomLeft);
512: Vector3 worldBottomRight = spriteTransform.TransformPoint(localBottomRight);
513: Vector3 worldBottomCenter = spriteTransform.TransformPoint(localBottomCenter);
514: Vector3 worldTopCenter = spriteTransform.TransformPoint(localTopCenter);
517: float visualWidth = Vector3.Distance(worldBottomLeft, worldBottomRight);
518: float visualHeight = Vector3.Distance(worldBottomCenter, worldTopCenter);
519: scale = Mathf.Clamp(Mathf.Max(visualWidth, visualHeight * 0.55f), 0.65f, 1.55f);
529: Vector3 leftScreen = camera.WorldToScreenPoint(worldBottomLeft);
530: Vector3 rightScreen = camera.WorldToScreenPoint(worldBottomRight);
531: Vector3 footScreen = (leftScreen + rightScreen) * 0.5f;
534: if (Mathf.Abs(footRay.direction.y) <= 0.0001f)
542: if (distance < 0f)
550: groundPosition.y = player.position.y + 0.035f;
556: out Vector3 groundPosition,
557: out Vector3 center,
561: ? boss.position + Vector3.up * 0.04f
562: : Vector3.zero;
563: center = groundPosition + Vector3.up;
564: scale = 1.35f;
581: scale = Mathf.Clamp(scale, 0.9f, 3.4f);
587: Vector3 localBottomLeft = new Vector3(
591: Vector3 localBottomRight = new Vector3(
595: Vector3 localTopCenter = new Vector3(
599: Vector3 localBottomCenter = new Vector3(
604: Vector3 worldBottomLeft = spriteTransform.TransformPoint(localBottomLeft);
605: Vector3 worldBottomRight = spriteTransform.TransformPoint(localBottomRight);
606: Vector3 worldBottomCenter = spriteTransform.TransformPoint(localBottomCenter);
607: Vector3 worldTopCenter = spriteTransform.TransformPoint(localTopCenter);
610: float visualWidth = Vector3.Distance(worldBottomLeft, worldBottomRight);
611: float visualHeight = Vector3.Distance(worldBottomCenter, worldTopCenter);
612: scale = Mathf.Clamp(Mathf.Max(visualWidth, visualHeight * 0.58f), 0.9f, 3.4f);
622: Vector3 leftScreen = camera.WorldToScreenPoint(worldBottomLeft);
623: Vector3 rightScreen = camera.WorldToScreenPoint(worldBottomRight);
624: Vector3 footScreen = (leftScreen + rightScreen) * 0.5f;
627: if (Mathf.Abs(footRay.direction.y) <= 0.0001f)
635: if (distance < 0f)
643: groundPosition.y = boss.position.y + 0.04f;
652: primary = new Color(0.92f, 0.055f, 0.12f, 0.96f);
653: accent = new Color(1f, 0.58f, 0.12f, 1f);
656: primary = new Color(0.58f, 0.1f, 0.95f, 0.96f);
657: accent = new Color(0.22f, 0.92f, 1f, 1f);
660: primary = new Color(0.18f, 0.86f, 0.32f, 0.96f);
661: accent = new Color(1f, 0.84f, 0.2f, 1f);
664: primary = new Color(0.95f, 0.16f, 0.32f, 0.96f);
665: accent = new Color(1f, 0.94f, 0.7f, 1f);
678: primary = new Color(0.34f, 0.18f, 0.07f, 0.96f);
679: accent = new Color(0.68f, 0.9f, 0.3f, 1f);
682: primary = new Color(0.62f, 0.015f, 0.09f, 0.98f);
683: accent = new Color(0.65f, 0.22f, 0.9f, 1f);
686: primary = new Color(0.46f, 0.9f, 0.08f, 0.96f);
687: accent = new Color(1f, 0.38f, 0.43f, 1f);
690: primary = new Color(0.65f, 0.9f, 1f, 0.94f);
691: accent = new Color(1f, 0.78f, 0.24f, 1f);
694: primary = new Color(0.66f, 0.04f, 0.26f, 0.96f);
700: private static bool IsNearView(Vector3 worldPosition)
708: Vector3 viewport = camera.WorldToViewportPoint(worldPosition);
709: return viewport.z > 0f
710: && viewport.x >= -0.3f
711: && viewport.x <= 1.3f
712: && viewport.y >= -0.3f
713: && viewport.y <= 1.3f;
716: private static bool IsBossEntranceVisible(Vector3 worldPosition)
724: Vector3 viewport = camera.WorldToViewportPoint(worldPosition);
725: return viewport.z > 0f
726: && viewport.x >= 0.08f
727: && viewport.x <= 0.92f
728: && viewport.y >= 0.1f
729: && viewport.y <= 0.9f;
736: PlayerItemCategory.AttackStyle => new Color(1f, 0.28f, 0.08f, 0.95f),
737: PlayerItemCategory.HighRiskHighReturn => new Color(0.76f, 0.08f, 0.62f, 0.95f),
738: PlayerItemCategory.SurvivalDefense => new Color(0.18f, 0.88f, 0.92f, 0.95f),
739: PlayerItemCategory.SummonPet => new Color(0.28f, 0.92f, 0.42f, 0.95f),
740: PlayerItemCategory.ChainClear => new Color(0.32f, 0.56f, 1f, 0.95f),
741: PlayerItemCategory.BossSpecialized => new Color(1f, 0.72f, 0.12f, 0.98f),
742: PlayerItemCategory.FunRandom => new Color(1f, 0.24f, 0.78f, 0.95f),
743: _ => new Color(0.96f, 0.08f, 0.2f, 0.95f)
```

## Assets/_Project/Scripts/Gameplay/Effects/CombatVfxResources.cs

```csharp
20: private static Texture2D particleTexture;
32: const int size = 64;
33: Texture2D texture = CreateTexture(size, "CombatVfxSoftCircle");
34: float center = (size - 1) * 0.5f;
37: for (int y = 0; y < size; y++)
39: for (int x = 0; x < size; x++)
41: float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / radius;
42: float alpha = Mathf.Clamp01(1f - distance);
43: alpha = alpha * alpha * (3f - 2f * alpha);
44: texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
60: const int size = 96;
61: Texture2D texture = CreateTexture(size, "CombatVfxRing");
62: float center = (size - 1) * 0.5f;
64: const float ringCenter = 0.72f;
65: const float halfWidth = 0.1f;
67: for (int y = 0; y < size; y++)
69: for (int x = 0; x < size; x++)
71: float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / radius;
73: float alpha = 1f - Mathf.Clamp01(ringDistance / halfWidth);
74: alpha *= Mathf.Clamp01((1f - distance) * 8f);
75: texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
91: const int size = 64;
92: Texture2D texture = CreateTexture(size, "CombatVfxStar");
94: for (int y = 0; y < size; y++)
96: for (int x = 0; x < size; x++)
98: Vector2 p = PixelToNormalized(x, y, size);
101: float verticalWidth = Mathf.Lerp(0.34f, 0.025f, Mathf.Clamp01(ay));
102: float horizontalWidth = Mathf.Lerp(0.34f, 0.025f, Mathf.Clamp01(ax));
103: float vertical = Mathf.Clamp01((verticalWidth - ax) * 24f) * Mathf.Clamp01((1f - ay) * 8f);
104: float horizontal = Mathf.Clamp01((horizontalWidth - ay) * 24f) * Mathf.Clamp01((1f - ax) * 8f);
105: float diamond = Mathf.Clamp01((0.38f - ax - ay) * 12f);
106: texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Max(diamond, Mathf.Max(vertical, horizontal))));
122: const int size = 64;
123: Texture2D texture = CreateTexture(size, "CombatVfxChevron");
125: Vector2 left = new Vector2(-0.72f, -0.28f);
126: Vector2 tip = new Vector2(0f, 0.42f);
127: Vector2 right = new Vector2(0.72f, -0.28f);
128: for (int y = 0; y < size; y++)
130: for (int x = 0; x < size; x++)
132: Vector2 p = PixelToNormalized(x, y, size);
136: float alpha = Mathf.Clamp01((0.13f - distance) * 24f);
137: texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
153: const int size = 96;
154: Texture2D texture = CreateTexture(size, "CombatVfxSlash");
156: Vector2[] curve =
158: new Vector2(-0.82f, -0.58f),
159: new Vector2(-0.46f, -0.18f),
160: new Vector2(0.02f, 0.2f),
161: new Vector2(0.48f, 0.48f),
162: new Vector2(0.82f, 0.62f)
165: for (int y = 0; y < size; y++)
167: for (int x = 0; x < size; x++)
169: Vector2 p = PixelToNormalized(x, y, size);
171: for (int i = 0; i < curve.Length - 1; i++)
173: distance = Mathf.Min(distance, DistanceToSegment(p, curve[i], curve[i + 1]));
176: float taper = Mathf.Clamp01((0.94f - Mathf.Abs(p.x)) * 5f);
177: float alpha = Mathf.Clamp01((0.105f * taper - distance) * 28f);
178: texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
194: const int size = 96;
195: Texture2D texture = CreateTexture(size, "CombatVfxRune");
197: for (int y = 0; y < size; y++)
199: for (int x = 0; x < size; x++)
201: Vector2 p = PixelToNormalized(x, y, size);
203: float ring = Mathf.Clamp01((0.055f - Mathf.Abs(radius - 0.68f)) * 26f);
204: float diamond = Mathf.Clamp01((0.49f - Mathf.Abs(p.x) - Mathf.Abs(p.y)) * 22f)
205: * Mathf.Clamp01((Mathf.Abs(p.x) + Mathf.Abs(p.y) - 0.42f) * 22f);
206: float ticks = 0f;
207: for (int i = 0; i < 8; i++)
209: float angle = i * Mathf.PI * 0.25f;
210: Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
211: Vector2 tangent = new Vector2(-direction.y, direction.x);
212: float along = Vector2.Dot(p, direction);
213: float across = Mathf.Abs(Vector2.Dot(p, tangent));
214: float tick = Mathf.Clamp01((0.045f - across) * 30f)
215: * Mathf.Clamp01((along - 0.73f) * 20f)
216: * Mathf.Clamp01((0.92f - along) * 20f);
219: texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Max(ring, Mathf.Max(diamond, ticks))));
235: const int size = 64;
236: Texture2D texture = CreateTexture(size, "CombatVfxArrow");
238: for (int y = 0; y < size; y++)
240: for (int x = 0; x < size; x++)
242: Vector2 p = PixelToNormalized(x, y, size);
243: float stem = Mathf.Clamp01((0.075f - Mathf.Abs(p.x)) * 30f)
244: * Mathf.Clamp01((p.y + 0.82f) * 10f)
245: * Mathf.Clamp01((0.35f - p.y) * 10f);
246: float headWidth = Mathf.Clamp01((0.72f - p.y) * 2.7f);
247: float head = Mathf.Clamp01((headWidth - Mathf.Abs(p.x)) * 18f)
248: * Mathf.Clamp01((p.y - 0.04f) * 9f)
249: * Mathf.Clamp01((0.82f - p.y) * 10f);
250: texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Max(stem, head)));
266: const int size = 96;
267: Texture2D texture = CreateTexture(size, "CombatVfxCrack");
269: Vector2[][] branches =
271: new[] { Vector2.zero, new Vector2(0.14f, 0.2f), new Vector2(0.08f, 0.48f), new Vector2(0.34f, 0.86f) },
272: new[] { Vector2.zero, new Vector2(-0.22f, 0.12f), new Vector2(-0.38f, 0.4f), new Vector2(-0.78f, 0.58f) },
273: new[] { Vector2.zero, new Vector2(0.18f, -0.18f), new Vector2(0.5f, -0.3f), new Vector2(0.82f, -0.62f) },
274: new[] { Vector2.zero, new Vector2(-0.1f, -0.24f), new Vector2(-0.04f, -0.54f), new Vector2(-0.28f, -0.88f) },
275: new[] { new Vector2(-0.38f, 0.4f), new Vector2(-0.3f, 0.68f), new Vector2(-0.46f, 0.86f) },
276: new[] { new Vector2(0.5f, -0.3f), new Vector2(0.64f, -0.08f), new Vector2(0.9f, 0.02f) }
279: for (int y = 0; y < size; y++)
281: for (int x = 0; x < size; x++)
283: Vector2 p = PixelToNormalized(x, y, size);
285: for (int branch = 0; branch < branches.Length; branch++)
287: Vector2[] points = branches[branch];
288: for (int i = 0; i < points.Length - 1; i++)
290: distance = Mathf.Min(distance, DistanceToSegment(p, points[i], points[i + 1]));
293: float alpha = Mathf.Clamp01((0.045f - distance) * 35f);
294: texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
310: const int size = 96;
311: Texture2D texture = CreateTexture(size, "CombatVfxSplat");
313: for (int y = 0; y < size; y++)
315: for (int x = 0; x < size; x++)
317: Vector2 p = PixelToNormalized(x, y, size);
318: float angle = Mathf.Atan2(p.y, p.x);
319: float boundary = 0.58f
320: + Mathf.Sin(angle * 5f + 0.4f) * 0.1f
321: + Mathf.Sin(angle * 9f - 0.7f) * 0.055f;
322: float body = Mathf.Clamp01((boundary - p.magnitude) * 18f);
323: float centerHole = Mathf.Clamp01((p.magnitude - 0.16f) * 18f);
324: float alpha = Mathf.Max(body * 0.62f, body * centerHole);
325: texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
341: const int size = 64;
342: Texture2D texture = CreateTexture(size, "CombatVfxBloodDrop");
344: for (int y = 0; y < size; y++)
346: for (int x = 0; x < size; x++)
348: Vector2 p = PixelToNormalized(x, y, size);
349: float bulb = Mathf.Clamp01((0.48f - Vector2.Distance(p, new Vector2(0f, -0.3f))) * 18f);
350: float taperWidth = Mathf.Clamp01((0.86f - p.y) / 1.1f) * 0.34f;
351: float taper = Mathf.Clamp01((taperWidth - Mathf.Abs(p.x)) * 22f)
352: * Mathf.Clamp01((p.y + 0.28f) * 8f)
353: * Mathf.Clamp01((0.88f - p.y) * 12f);
354: texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Max(bulb, taper)));
370: const int size = 64;
371: Texture2D texture = CreateTexture(size, "CombatVfxFang");
373: for (int y = 0; y < size; y++)
375: for (int x = 0; x < size; x++)
377: Vector2 p = PixelToNormalized(x, y, size);
378: float normalizedY = Mathf.Clamp01((p.y + 0.78f) / 1.56f);
379: float centerCurve = Mathf.Sin(normalizedY * Mathf.PI) * 0.1f;
380: float width = Mathf.Lerp(0.055f, 0.36f, normalizedY);
381: float body = Mathf.Clamp01((width - Mathf.Abs(p.x - centerCurve)) * 24f)
382: * Mathf.Clamp01((p.y + 0.82f) * 12f)
383: * Mathf.Clamp01((0.82f - p.y) * 12f);
384: texture.SetPixel(x, y, new Color(1f, 1f, 1f, body));
400: const int size = 96;
401: Texture2D texture = CreateTexture(size, "CombatVfxWindArc");
403: Vector2 tailA = new Vector2(-0.76f, -0.2f);
404: Vector2 tailB = new Vector2(-0.28f, -0.1f);
405: Vector2 tailC = new Vector2(0.18f, -0.34f);
406: for (int y = 0; y < size; y++)
408: for (int x = 0; x < size; x++)
410: Vector2 p = PixelToNormalized(x, y, size);
412: float angle = Mathf.Atan2(p.y, p.x);
413: float ring = Mathf.Clamp01((0.07f - Mathf.Abs(radius - 0.63f)) * 22f);
414: float angularMask = Mathf.Clamp01((angle + 2.55f) * 5f)
415: * Mathf.Clamp01((2.2f - angle) * 5f);
416: float tail = Mathf.Clamp01((0.065f - Mathf.Min(
418: DistanceToSegment(p, tailB, tailC))) * 24f);
419: texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Max(ring * angularMask, tail)));
435: const int size = 128;
436: Texture2D texture = CreateTexture(size, "CombatDamageVignette");
437: float center = (size - 1) * 0.5f;
438: Vector2 inverseRadius = new Vector2(1f / center, 1f / center);
440: for (int y = 0; y < size; y++)
442: for (int x = 0; x < size; x++)
444: Vector2 normalized = Vector2.Scale(
445: new Vector2(Mathf.Abs(x - center), Mathf.Abs(y - center)),
448: float radial = normalized.magnitude * 0.72f;
449: float alpha = Mathf.SmoothStep(0f, 1f, Mathf.Max(edge, radial) - 0.3f);
450: texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
455: vignetteSprite = CreateSprite(texture, "CombatDamageVignetteSprite", 100f);
479: lineMaterial = CreateUnlitMaterial("CombatVfxLineMaterial", Texture2D.whiteTexture);
507: private static Texture2D CreateDiamondTexture()
509: const int size = 32;
510: Texture2D texture = CreateTexture(size, "CombatVfxParticle");
511: float center = (size - 1) * 0.5f;
514: for (int y = 0; y < size; y++)
516: for (int x = 0; x < size; x++)
519: float alpha = Mathf.Clamp01(1f - distance * 0.62f);
521: texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
551: private static Texture2D CreateTexture(int size, string textureName)
553: Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
563: private static Sprite CreateSprite(Texture2D texture, string spriteName, float pixelsPerUnit = 64f)
567: new Rect(0f, 0f, texture.width, texture.height),
568: new Vector2(0.5f, 0.5f),
575: private static Vector2 PixelToNormalized(int x, int y, int size)
577: float center = (size - 1) * 0.5f;
578: return new Vector2((x - center) / center, (y - center) / center);
581: private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
583: Vector2 segment = end - start;
585: if (lengthSquared <= 0.000001f)
587: return Vector2.Distance(point, start);
590: float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
591: return Vector2.Distance(point, start + segment * t);
```

## Assets/_Project/Scripts/Gameplay/Effects/DamageVignetteOverlay.cs

```csharp
24: instance.peakAlpha = Mathf.Max(instance.peakAlpha, Mathf.Clamp01(alpha));
25: instance.duration = Mathf.Max(instance.duration, Mathf.Max(0.08f, lifeTime));
26: instance.elapsed = 0f;
43: canvas.sortingOrder = 32000;
53: rect.anchorMin = Vector2.zero;
54: rect.anchorMax = Vector2.one;
55: rect.offsetMin = Vector2.zero;
56: rect.offsetMax = Vector2.zero;
66: float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
67: float alpha = peakAlpha * Mathf.Pow(1f - t, 2.2f);
70: if (t >= 1f)
72: peakAlpha = 0f;
73: duration = 0f;
74: ApplyColor(0f);
91: image.color = new Color(0.72f, 0.015f, 0.035f, Mathf.Clamp01(alpha));
```

## Assets/_Project/Scripts/Gameplay/Effects/ItemPickupVfx.cs

```csharp
8: private const int TrailSortingOrder = 5240;
9: private const int SparkSortingOrder = 5250;
10: private const int FlashSortingOrder = 5260;
11: private const int SparkCount = 5;
20: private Vector3 collectorOffset;
21: private Vector3 startPosition;
44: FlashSortingOrder + 1);
46: for (int i = 0; i < SparkCount; i++)
50: $"AbsorbSpark{i + 1}",
60: Vector3 visualPosition,
62: Vector3 collectorCenter,
70: transform.position = Vector3.zero;
76: : Vector3.zero;
80: duration = 0.56f;
81: elapsed = 0f;
82: effectScale = Mathf.Clamp(scale * 2f, 1.3f, 3f);
85: trail.positionCount = 7;
88: sourceFlash.transform.localScale = Vector3.one * effectScale * 0.32f;
89: sourceFlash.color = WithAlpha(accentColor, 1f);
93: targetFlash.transform.localScale = Vector3.one * 0.02f;
94: targetFlash.color = WithAlpha(accentColor, 0f);
96: for (int i = 0; i < sparks.Length; i++)
105: sparks[i].transform.localScale = Vector3.one * effectScale * (0.15f - i * 0.01f);
106: sparks[i].color = WithAlpha(Color.Lerp(primaryColor, accentColor, i * 0.18f), 0f);
110: UpdateTrail(startPosition, 0f, 1f);
118: float t = Mathf.Clamp01(elapsed / duration);
119: Vector3 targetPosition = GetCollectorPosition();
120: float headT = Smooth01(Mathf.Clamp01(t / 0.76f));
121: float trailFade = Mathf.Clamp01((0.88f - t) / 0.2f);
129: if (t >= 1f)
142: float localT = Mathf.Clamp01(t / 0.32f);
144: sourceFlash.transform.localScale = Vector3.one
146: * Mathf.Lerp(0.32f, 0.9f, Mathf.Sin(localT * Mathf.PI));
147: sourceFlash.transform.Rotate(0f, 0f, 280f * Time.unscaledDeltaTime, Space.Self);
148: sourceFlash.color = WithAlpha(accentColor, 1f - localT);
151: private void UpdateTravelSparks(float t, Vector3 targetPosition)
153: for (int i = 0; i < sparks.Length; i++)
161: float delayedT = Mathf.Clamp01((t - i * 0.045f) / 0.76f);
162: float travelT = Smooth01(delayedT);
165: spark.transform.localScale = Vector3.one
167: * Mathf.Lerp(0.065f, 0.23f - i * 0.014f, pulse);
168: spark.transform.Rotate(0f, 0f, (180f + i * 35f) * Time.unscaledDeltaTime, Space.Self);
170: Color.Lerp(primaryColor, accentColor, i * 0.18f),
171: pulse * (0.92f - i * 0.08f));
175: private void UpdateTargetFlash(float t, Vector3 targetPosition)
182: float flashT = Mathf.Clamp01((t - 0.63f) / 0.37f);
185: targetFlash.transform.localScale = Vector3.one
187: * Mathf.Lerp(0.03f, 0.74f, pulse);
188: targetFlash.transform.Rotate(0f, 0f, -360f * Time.unscaledDeltaTime, Space.Self);
192: private void UpdateTrail(Vector3 targetPosition, float headT, float alpha)
199: float tailT = Mathf.Max(0f, headT - 0.34f);
200: int count = Mathf.Max(2, trail.positionCount);
201: for (int i = 0; i < count; i++)
203: float segmentT = Mathf.Lerp(tailT, headT, i / (count - 1f));
207: Color transparentPrimary = WithAlpha(primaryColor, 0f);
209: trail.endColor = WithAlpha(accentColor, alpha * 0.9f);
210: trail.startWidth = effectScale * 0.05f;
211: trail.endWidth = effectScale * 0.15f;
214: private Vector3 EvaluatePath(float t, Vector3 targetPosition)
216: Vector3 straight = Vector3.Lerp(startPosition, targetPosition, t);
217: float arcHeight = Mathf.Max(0.42f, Vector3.Distance(startPosition, targetPosition) * 0.18f);
218: return straight + Vector3.up * Mathf.Sin(t * Mathf.PI) * arcHeight;
221: private Vector3 GetCollectorPosition()
225: : startPosition + Vector3.up * 0.65f;
239: for (int i = 0; i < sparks.Length; i++)
266: for (int i = 0; i < SparkCount; i++)
270: sparks[i] = transform.Find($"AbsorbSpark{i + 1}")?.GetComponent<SpriteRenderer>();
282: renderer.positionCount = 7;
283: renderer.numCornerVertices = 2;
284: renderer.numCapVertices = 2;
310: return camera != null ? camera.transform.rotation : Quaternion.Euler(45f, 0f, 0f);
313: private static float Smooth01(float t)
315: return t * t * (3f - 2f * t);
320: color.a = Mathf.Clamp01(alpha);
```

## Assets/_Project/Scripts/Gameplay/Effects/JobChangeVfx.cs

```csharp
8: private const int SymbolSortingOrder = 5350;
9: private const int ParticleSortingOrder = 5380;
10: private const int SymbolCount = 8;
17: private Vector3 followOffset;
18: private Vector3 centerOffset;
32: for (int i = 0; i < SymbolCount; i++)
36: $"JobSymbol{i + 1}",
48: Vector3 groundPosition,
49: Vector3 center,
58: followOffset = target != null ? groundPosition - target.position : Vector3.zero;
63: effectScale = Mathf.Clamp(scale, 0.65f, 1.55f);
64: duration = job == JobType.Mage ? 1.35f : job == JobType.Warrior ? 0.88f : 1.02f;
65: elapsed = 0f;
80: float t = Mathf.Clamp01(elapsed / duration);
101: if (t >= 1f)
109: for (int i = 0; i < symbols.Length; i++)
119: symbol.transform.localScale = Vector3.one * 0.02f;
121: symbol.color = WithAlpha(primaryColor, 0f);
126: EnableSymbol(0, CombatVfxResources.GetSlashSprite());
127: EnableSymbol(1, CombatVfxResources.GetSlashSprite());
128: EnableSymbol(2, CombatVfxResources.GetStarSprite());
132: EnableSymbol(0, CombatVfxResources.GetRuneSprite());
133: EnableSymbol(1, CombatVfxResources.GetRuneSprite());
134: for (int i = 2; i < 6; i++)
141: for (int i = 0; i < 7; i++)
151: for (int i = 0; i < 2; i++)
159: float delay = i * 0.13f;
160: float slashT = Mathf.Clamp01((t - delay) / 0.55f);
161: float sweep = 1f - Mathf.Pow(1f - slashT, 3f);
163: float side = i == 0 ? -1f : 1f;
165: + GetCameraRight() * side * effectScale * Mathf.Lerp(0.5f, -0.1f, sweep)
166: + Vector3.up * effectScale * Mathf.Lerp(-0.28f, 0.32f, sweep);
167: slash.transform.localScale = new Vector3(
168: effectScale * Mathf.Lerp(0.4f, 1.15f, sweep),
169: effectScale * Mathf.Lerp(0.3f, 0.82f, sweep),
170: 1f);
172: * Quaternion.Euler(0f, 0f, i == 0 ? -24f : 114f);
173: slash.color = WithAlpha(i == 0 ? primaryColor : accentColor, alpha);
176: SpriteRenderer impact = symbols[2];
179: float impactT = Mathf.Clamp01((t - 0.24f) / 0.5f);
182: impact.transform.localScale = Vector3.one
184: * Mathf.Lerp(0.05f, 0.72f, pulse);
186: * Quaternion.Euler(0f, 0f, impactT * 110f);
187: impact.color = WithAlpha(accentColor, pulse * 0.9f);
194: float appear = Mathf.Clamp01(t / 0.16f);
195: float fade = Mathf.Clamp01((1f - t) / 0.24f);
198: for (int i = 0; i < 2; i++)
206: float direction = i == 0 ? 1f : -1f;
207: float size = i == 0
208: ? Mathf.Lerp(0.18f, 1.02f, Mathf.SmoothStep(0f, 1f, t))
209: : Mathf.Lerp(0.12f, 0.66f, Mathf.SmoothStep(0f, 1f, t));
211: + Vector3.up * effectScale * (i == 0 ? 0.05f : 0.18f);
212: rune.transform.localScale = Vector3.one * effectScale * size;
214: * Quaternion.Euler(0f, 0f, direction * t * (i == 0 ? 210f : 320f));
215: rune.color = WithAlpha(i == 0 ? primaryColor : accentColor, envelope * (i == 0 ? 0.82f : 0.62f));
218: for (int i = 2; i < 6; i++)
226: float angle = t * -260f + (i - 2) * 90f;
227: float radians = angle * Mathf.Deg2Rad;
228: float radius = effectScale * Mathf.Lerp(0.22f, 0.68f, Mathf.Sin(t * Mathf.PI));
231: + Vector3.up * (Mathf.Sin(radians) * radius + effectScale * 0.15f);
232: star.transform.localScale = Vector3.one
234: * (0.1f + Mathf.Sin(t * Mathf.PI) * 0.11f);
236: * Quaternion.Euler(0f, 0f, -angle * 0.6f);
237: star.color = WithAlpha(Color.Lerp(primaryColor, accentColor, (i - 2) / 3f), envelope);
244: Vector3 cameraRight = GetCameraRight();
245: for (int i = 0; i < 7; i++)
253: float delay = i * 0.025f;
254: float arrowT = Mathf.Clamp01((t - delay) / 0.76f);
255: float launch = 1f - Mathf.Pow(1f - arrowT, 3f);
257: float angle = Mathf.Lerp(-58f, 58f, i / 6f);
258: float radians = angle * Mathf.Deg2Rad;
259: Vector3 direction = Vector3.up * Mathf.Cos(radians)
262: - Vector3.up * effectScale * 0.22f
263: + direction * effectScale * Mathf.Lerp(0.05f, 1.3f, launch);
264: arrow.transform.localScale = new Vector3(
265: effectScale * 0.22f,
266: effectScale * Mathf.Lerp(0.35f, 0.62f, alpha),
267: 1f);
269: * Quaternion.Euler(0f, 0f, -angle);
270: arrow.color = WithAlpha(Color.Lerp(primaryColor, accentColor, i / 6f), alpha * 0.94f);
274: private void EmitJobParticles(Vector3 groundPosition, JobType job)
283: int count = job == JobType.Mage ? 18 : 24;
284: for (int i = 0; i < count; i++)
286: Vector3 velocity;
287: Vector3 spawnPosition;
290: Vector3 radial = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
291: velocity = radial * Random.Range(1.6f, 3.2f) * effectScale
292: + Vector3.up * Random.Range(0.35f, 1.2f) * effectScale;
293: spawnPosition = groundPosition + centerOffset + radial * Random.Range(0.02f, 0.2f);
297: Vector3 radial = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
298: velocity = radial * Random.Range(0.15f, 0.55f) * effectScale
299: + Vector3.up * Random.Range(0.6f, 1.5f) * effectScale;
300: spawnPosition = groundPosition + centerOffset + radial * Random.Range(0.25f, 0.62f) * effectScale;
304: float angle = Random.Range(-55f, 55f) * Mathf.Deg2Rad;
305: velocity = (Vector3.up * Mathf.Cos(angle) + GetCameraRight() * Mathf.Sin(angle))
306: * Random.Range(1.5f, 3.1f) * effectScale;
307: spawnPosition = groundPosition + centerOffset - Vector3.up * effectScale * 0.2f;
314: startColor = Color.Lerp(primaryColor, accentColor, Random.Range(0.12f, 0.9f)),
315: startLifetime = Random.Range(0.42f, 0.92f),
316: startSize = Random.Range(0.045f, 0.13f) * effectScale,
317: rotation = Random.Range(0f, 360f)
319: particles.Emit(parameters, 1);
325: if (index < 0 || index >= symbols.Length || symbols[index] == null)
344: for (int i = 0; i < SymbolCount; i++)
348: symbols[i] = transform.Find($"JobSymbol{i + 1}")?.GetComponent<SpriteRenderer>();
377: main.maxParticles = 40;
378: main.startSpeed = 0f;
379: main.startLifetime = 0.7f;
380: main.startSize = 0.1f;
381: main.gravityModifier = 0f;
388: 1f,
390: new Keyframe(0f, 0f),
391: new Keyframe(0.12f, 1f),
392: new Keyframe(0.72f, 0.72f),
393: new Keyframe(1f, 0f)));
398: renderer.lengthScale = 1.6f;
399: renderer.velocityScale = 0.18f;
409: return camera != null ? camera.transform.rotation : Quaternion.Euler(45f, 0f, 0f);
412: private static Vector3 GetCameraRight()
415: return camera != null ? camera.transform.right : Vector3.right;
420: color.a = Mathf.Clamp01(alpha);
```

## Assets/_Project/Scripts/Gameplay/Effects/LevelUpVfx.cs

```csharp
8: private const int ShaftSortingOrder = 5280;
9: private const int ChevronSortingOrder = 5290;
10: private const int CrownSortingOrder = 5310;
11: private const int ParticleSortingOrder = 5320;
12: private const int ShaftCount = 2;
13: private const int ChevronCount = 4;
22: private Vector3 followOffset;
23: private Vector3 centerOffset;
28: private readonly Color energyColor = new Color(0.96f, 0.08f, 0.3f, 0.95f);
29: private readonly Color ascentColor = new Color(1f, 0.48f, 0.1f, 0.98f);
30: private readonly Color highlightColor = new Color(1f, 0.96f, 0.68f, 1f);
38: for (int i = 0; i < ShaftCount; i++)
42: $"LightShaft{i + 1}",
47: for (int i = 0; i < ChevronCount; i++)
51: $"AscendMark{i + 1}",
68: Vector3 groundPosition,
69: Vector3 center,
75: followOffset = target != null ? groundPosition - target.position : Vector3.zero;
77: duration = 1.28f;
78: elapsed = 0f;
79: effectScale = Mathf.Clamp(scale, 0.65f, 1.55f);
85: for (int i = 0; i < shafts.Length; i++)
95: shaft.transform.localScale = new Vector3(
96: effectScale * (i == 0 ? 0.34f : 0.18f),
97: effectScale * (i == 0 ? 2.2f : 2.8f),
98: 1f);
99: shaft.color = WithAlpha(i == 0 ? energyColor : highlightColor, 0f);
102: for (int i = 0; i < chevrons.Length; i++)
111: chevron.transform.localPosition = centerOffset - Vector3.up * effectScale * 0.65f;
112: chevron.transform.localScale = Vector3.one * effectScale * 0.34f;
113: chevron.color = WithAlpha(Color.Lerp(ascentColor, highlightColor, i * 0.22f), 0f);
117: crown.transform.localPosition = centerOffset + Vector3.up * effectScale * 0.82f;
118: crown.transform.localScale = Vector3.one * 0.02f;
119: crown.color = WithAlpha(highlightColor, 0f);
130: float t = Mathf.Clamp01(elapsed / duration);
131: float appear = Mathf.Clamp01(t / 0.12f);
132: float fade = Mathf.Clamp01((1f - t) / 0.25f);
146: if (t >= 1f)
154: Vector3 cameraRight = GetCameraRight();
155: for (int i = 0; i < shafts.Length; i++)
163: float side = i == 0 ? -1f : 1f;
164: float rise = Mathf.SmoothStep(0f, 1f, t);
167: + cameraRight * side * effectScale * 0.22f
168: + Vector3.up * effectScale * Mathf.Lerp(0.1f, 0.62f, rise);
169: shaft.transform.localScale = new Vector3(
170: effectScale * Mathf.Lerp(i == 0 ? 0.34f : 0.19f, 0.075f, rise),
171: effectScale * Mathf.Lerp(i == 0 ? 2.15f : 2.65f, 3.2f, rise),
172: 1f);
173: Color color = i == 0 ? energyColor : highlightColor;
174: shaft.color = WithAlpha(color, envelope * (1f - t) * (i == 0 ? 0.38f : 0.24f));
180: for (int i = 0; i < chevrons.Length; i++)
188: float delay = i * 0.09f;
189: float markT = Mathf.Clamp01((t - delay) / Mathf.Max(0.01f, 0.7f - delay));
190: float rise = 1f - Mathf.Pow(1f - markT, 2f);
193: + Vector3.up * effectScale * Mathf.Lerp(-0.68f, 1.72f, rise);
194: chevron.transform.localScale = new Vector3(
195: effectScale * Mathf.Lerp(0.36f, 0.58f, alpha),
196: effectScale * Mathf.Lerp(0.2f, 0.34f, alpha),
197: 1f);
199: Color.Lerp(ascentColor, highlightColor, i * 0.22f),
200: alpha * (0.9f - i * 0.1f));
211: float crownT = Mathf.Clamp01((t - 0.28f) / 0.55f);
213: float settle = 1f - Mathf.Pow(1f - crownT, 3f);
215: + Vector3.up * effectScale * Mathf.Lerp(0.62f, 1.05f, settle);
216: crown.transform.localScale = Vector3.one
218: * Mathf.Lerp(0.04f, 0.56f, pop);
220: * Quaternion.Euler(0f, 0f, t * 160f);
227: for (int i = 0; i < shafts.Length; i++)
234: for (int i = 0; i < chevrons.Length; i++)
243: private void EmitMotes(Vector3 groundPosition)
252: for (int i = 0; i < 24; i++)
254: Vector3 offset = GetCameraRight() * Random.Range(-0.36f, 0.36f) * effectScale;
257: position = groundPosition + offset + Vector3.up * Random.Range(0.05f, 0.55f) * effectScale,
258: velocity = Vector3.up * Random.Range(1.7f, 3.9f) * effectScale,
259: startColor = Color.Lerp(energyColor, highlightColor, Random.Range(0.2f, 0.9f)),
260: startLifetime = Random.Range(0.55f, 1.05f),
261: startSize = Random.Range(0.05f, 0.13f) * effectScale,
262: rotation = Random.Range(0f, 360f)
264: motes.Emit(parameters, 1);
282: for (int i = 0; i < ShaftCount; i++)
286: shafts[i] = transform.Find($"LightShaft{i + 1}")?.GetComponent<SpriteRenderer>();
289: for (int i = 0; i < ChevronCount; i++)
293: chevrons[i] = transform.Find($"AscendMark{i + 1}")?.GetComponent<SpriteRenderer>();
322: main.maxParticles = 40;
323: main.startSpeed = 0f;
324: main.startLifetime = 0.8f;
325: main.startSize = 0.1f;
326: main.gravityModifier = -0.06f;
333: 1f,
335: new Keyframe(0f, 0f),
336: new Keyframe(0.16f, 1f),
337: new Keyframe(0.78f, 0.62f),
338: new Keyframe(1f, 0f)));
352: return camera != null ? camera.transform.rotation : Quaternion.Euler(45f, 0f, 0f);
355: private static Vector3 GetCameraRight()
358: return camera != null ? camera.transform.right : Vector3.right;
363: color.a = Mathf.Clamp01(alpha);
```

## Assets/_Project/Scripts/Gameplay/Effects/MeleeArcVfx.cs

```csharp
8: private const int PointCount = 15;
13: private readonly Vector3[] points = new Vector3[PointCount];
15: private Vector3 origin;
16: private Vector3 forward;
27: effect.outerLine = CreateLine(root.transform, "Outer", 0.24f, 5160);
28: effect.innerLine = CreateLine(root.transform, "Inner", 0.075f, 5161);
33: public void Show(Vector3 worldOrigin, Vector3 attackDirection, float attackRadius, float lifeTime)
36: origin = worldOrigin + Vector3.up * 0.35f;
38: forward.y = 0f;
39: if (forward.sqrMagnitude <= 0.0001f)
41: forward = Vector3.forward;
45: radius = Mathf.Max(0.65f, attackRadius);
46: duration = Mathf.Max(0.08f, lifeTime);
47: elapsed = 0f;
50: UpdateLines(0f);
58: float t = Mathf.Clamp01(elapsed / duration);
60: if (t >= 1f)
68: float eased = 1f - Mathf.Pow(1f - t, 3f);
69: float currentRadius = radius * Mathf.Lerp(0.48f, 1.08f, eased);
70: float sweepOffset = Mathf.Lerp(-16f, 10f, eased);
72: for (int i = 0; i < PointCount; i++)
74: float normalized = i / (PointCount - 1f);
75: float angle = Mathf.Lerp(-68f, 68f, normalized) + sweepOffset;
76: Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
77: float taperedRadius = currentRadius * Mathf.Lerp(0.86f, 1f, Mathf.Sin(normalized * Mathf.PI));
83: float fade = Mathf.Pow(1f - t, 1.6f);
84: outerLine.startColor = new Color(0.44f, 0.015f, 0.045f, fade * 0.88f);
85: outerLine.endColor = new Color(0.95f, 0.06f, 0.16f, fade * 0.25f);
86: innerLine.startColor = new Color(1f, 0.92f, 0.72f, fade);
87: innerLine.endColor = new Color(1f, 0.22f, 0.18f, fade * 0.35f);
88: outerLine.widthMultiplier = Mathf.Lerp(1f, 0.28f, t);
89: innerLine.widthMultiplier = Mathf.Lerp(1f, 0.15f, t);
118: line.numCapVertices = 3;
119: line.numCornerVertices = 2;
121: line.endWidth = width * 0.22f;
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/DustCloud.cs

```csharp
14: private float radius, speed, duration, began, dissolveAt = -1, frameSeconds, dissolveSeconds, baseOpacity, overlapOpacity;
15: public Vector3 Direction { get; private set; }
16: public Vector3 LaunchPosition { get; private set; }
21: public bool IsDissolving => dissolveAt >= 0;
25: public static DustCloud Launch(EnemyPatternLifetime lifetime, uint generation, Vector3 position, Vector3 direction,
27: Sprite[] moving, Sprite[] dissolve, float frameSeconds, float dissolveSeconds, float opacity, float overlapOpacity = .35f)
29: var go = new GameObject("P01_MovingDustCloud"); go.transform.position = position;
43: body.sprite = frames[0]; body.color = new Color(1, 1, 1, opacity);
44: float width = 0; foreach (var f in frames) width = Mathf.Max(width, f.bounds.size.x);
45: body.transform.localScale = Vector3.one * (radius * 2 / width);
51: public static bool CanTravel(Vector3 from, Vector3 to, float radius)
53: if (!GasSacOrb.HasClearPath(from, to) || !ResidueRubble.CanTraverse(from, to, new Vector2(radius, radius))) return false;
56: var min = biome.WorldToGrid(new Vector3(Mathf.Min(from.x, to.x) - radius, 0, Mathf.Min(from.z, to.z) - radius));
57: var max = biome.WorldToGrid(new Vector3(Mathf.Max(from.x, to.x) + radius, 0, Mathf.Max(from.z, to.z) + radius));
63: var bounds = new Bounds(center, new Vector3(biome.TileSize, 1, biome.TileSize));
72: if (dissolveAt >= 0)
79: float dt = Mathf.Min(Time.deltaTime, Mathf.Max(0, duration - (elapsed - Time.deltaTime)));
80: Vector3 from = transform.position, end = from + Direction * (speed * dt);
81: int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(from, end) / .1f));
82: for (int i = 1; i <= steps; i++)
84: var next = Vector3.Lerp(from, end, i / (float)steps);
86: Vector3 previous = transform.position; transform.position = next; TryHit(previous, next);
91: private void TryHit(Vector3 from, Vector3 to)
94: if (HitAttempts != 0 || player == null || player.IsDead || MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position)
103: if (dissolveAt >= 0) return;
104: dissolveAt = Time.time; if (marker != null) marker.SetActive(false); body.sprite = dissolveFrames[0]; UpdateView();
112: if (marker != null) marker.transform.position = p + Vector3.up * .055f;
127: color.a = Mathf.MoveTowards(color.a, overlap ? Mathf.Min(opacity, overlapOpacity) : opacity, Time.deltaTime * 6);
132: Bounds b = renderer.sprite.bounds; Vector2 min = Vector2.one * float.PositiveInfinity, max = Vector2.one * float.NegativeInfinity;
135: Vector3 local = new Vector3(renderer.flipX ? -x : x, renderer.flipY ? -y : y, 0);
136: Vector2 point = camera.WorldToViewportPoint(renderer.transform.TransformPoint(local)); min = Vector2.Min(min, point); max = Vector2.Max(max, point);
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/DustClumpElitePattern.cs

```csharp
19: private Vector3 anchor, coreCenter, launch, direction, fullSize, fullCenter, coreSize, coreColliderCenter;
25: private Vector2 shadowSize, coreShadowSize;
27: private Vector3 Position => physicsBody != null ? physicsBody.position : transform.position;
30: public Vector3 CoreCenter => coreCenter;
31: public Vector3 LockedDirection => direction;
32: public Vector3 LaunchPosition => launch;
64: body = source.transform.Find("Visual").GetComponent<SpriteRenderer>(); SetGeometry(false); SetFrame(idle[0]);
66: var sr = shadow.GetComponent<SpriteRenderer>(); sr.sprite = art.groundDisc; sr.color = art.groundShadowColor; sr.sortingOrder = 56;
67: shadow.transform.rotation = Quaternion.Euler(90, 0, 0); shadow.SetActive(true); LateUpdate();
68: nextReady = Time.time + grace; nextSafety = hitUntil = 0; ReleaseCount = RejectedPaths = 0; ReleasedAt = 0; direction = Vector3.zero;
78: Vector3 delta = (chase ? player.transform.position : anchor) - Position; delta.y = 0;
79: float stop = chase ? trigger * .8f : .15f; bool moving = delta.magnitude > stop && enemy.Stats.MoveSpeed > 0;
81: SetFrame(Time.time < hitUntil ? hit : (moving ? move : idle)[Mathf.FloorToInt(Time.time / (moving ? moveSeconds : idleSeconds)) % 2]);
89: Vector3 delta = player.transform.position - Position; delta.y = 0;
90: if (delta.sqrMagnitude < .0001f || delta.sqrMagnitude > trigger * trigger
91: || !GasSacOrb.IsVisible(Position + Vector3.up * .6f) || !GasSacOrb.HasClearPath(Position, player.transform.position)) return false;
92: Vector3 target = Position + delta.normalized * offset;
93: if (!DustCloud.CanTravel(Position, target, radius)) { RejectedPaths++; nextSafety = Time.time + .25f; return false; }
100: uint token = generation; var damage = enemy.CreatePatternDamage(DustClumpPatternSettings.DamageId, 0);
104: marker = new GameObject("P01_FullDangerCircle"); marker.transform.position = launch + Vector3.up * .055f;
111: if (t < 1f / 3) SetFrame(preparation[Mathf.Min(2, Mathf.FloorToInt(t * 9))]);
114: SetGeometry(true); SetFrame(core[0]);
115: if (gather == null) { gather = new GameObject("P01_GatheringDust"); Own(gather); gatherBody = DustCloud.CreateVisual(gather.transform, clouds, radius, opacity); }
116: gather.transform.position = Vector3.Lerp(coreCenter, launch, Mathf.InverseLerp(1f / 3, 1, t));
117: gatherBody.sprite = clouds[Mathf.FloorToInt(Time.time / cloudSeconds) % 4];
122: if (IsStunned() || !DustCloud.CanTravel(coreCenter, launch, radius) || !GasSacOrb.IsVisible(launch + Vector3.up * .6f)) yield break;
129: SetFrame(Time.time - ReleasedAt < .1f ? release : core[Mathf.FloorToInt(Time.time / coreSeconds) % 2]);
140: { SetGeometry(false); SetFrame(idle[0]); nextReady = Time.time + rearm; Phase = DustClumpPhase.Ready; }
153: private void OnDamaged(EnemyController source, float damage) { if (damage > 0) hitUntil = Time.time + .12f; }
161: Vector3 point = transform.position; if (BiomeManager.Active != null) point.y = BiomeManager.Active.GetGroundHeight(point);
162: shadow.transform.position = point + Vector3.up * .035f;
163: var size = coreExposed ? coreShadowSize : shadowSize; shadow.transform.localScale = new Vector3(size.x * .5f, size.y * .5f, 1);
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/DustClumpPatternSettings.cs

```csharp
9: [Min(.1f)] public float triggerDistance = 4.5f;
10: [Min(0)] public float spawnGraceSeconds = .75f;
11: [Min(.1f)] public float windupSeconds = .9f;
12: [Min(.1f)] public float launchOffset = 1.2f;
14: [Min(.1f)] public float cloudRadius = 1.1f;
15: [Min(.1f)] public float cloudSpeed = 2f;
16: [Min(.1f)] public float cloudLifetimeSeconds = 2f;
17: [Min(.1f)] public float recoverySeconds = .8f;
18: [Min(0)] public float rearmSeconds = 3.5f;
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/DustClumpPresentation.cs

```csharp
10: [Min(.02f)] public float idleFrameSeconds = .3f;
11: [Min(.02f)] public float moveFrameSeconds = .22f;
12: [Min(.02f)] public float coreFrameSeconds = .25f;
13: [Min(.02f)] public float cloudFrameSeconds = .15f;
14: [Min(.02f)] public float dissolveFrameSeconds = .12f;
15: [Min(.02f)] public float deathFrameSeconds = .14f;
16: [Range(.2f, 1)] public float cloudOpacity = .85f;
18: [Range(.1f, 1)] public float playerOverlapOpacity = .35f;
19: public Color dangerFillColor = new Color(.92f, .04f, .04f, .3f);
20: public Vector3 coreColliderSize = new Vector3(.55f, .85f, .55f);
21: public Vector3 coreColliderCenter = new Vector3(0, .6f, 0);
22: public Vector2 groundShadowSize = new Vector2(1.5f, .8f);
23: public Vector2 coreShadowSize = new Vector2(.7f, .45f);
24: public Color groundShadowColor = new Color(.12f, .1f, .15f, .25f);
27: if (!Valid(idleFrames, 2) || !Valid(moveFrames, 2) || !Valid(preparationFrames, 3) || !Valid(coreFrames, 2)
28: || !Valid(deathFrames, 6) || !Valid(coreDeathFrames, 6) || !Valid(cloudFrames, 4) || !Valid(dissolveFrames, 2)
29: || release == null || recovery == null || hit == null || groundDisc == null) return "Dust requires all 30 approved sprites and a shadow disc.";
34: if (!MonsterBalanceNumbers.Positive(playerOverlapOpacity) || playerOverlapOpacity > 1) return "Invalid player-overlap opacity.";
35: if (!MonsterBalanceNumbers.Positive(cloudOpacity) || cloudOpacity > 1) return "Invalid cloud opacity.";
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/EmberThorn.cs

```csharp
10: private Vector3 direction;
15: public Vector3 Direction => direction;
20: public Vector3 LaunchPosition { get; private set; }
23: Vector3 direction, EnemyDamageRequest damage, float speed, float duration, float length, float radius,
26: direction.y = 0; direction.Normalize();
28: Vector3 extents = collider != null ? collider.bounds.extents : Vector3.zero;
31: Vector3 origin = owner.GetPatternVisualOrigin(); origin.y = owner.transform.position.y;
32: Vector3 launch = origin + direction * (bodyExtent + radius);
34: Vector3 side = new Vector3(-direction.z, 0, direction.x) * radius;
35: Vector3 tip = launch + direction * (length * .5f);
39: var go = new GameObject("H02_CounterThorn"); go.transform.position = launch;
58: if (lifetime == null || !lifetime.IsCurrent(generation) || remaining <= 0) { Despawn(); return; }
60: Vector3 start = transform.position, end = start + direction * (speed * dt);
61: int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(start, end) / .15f));
62: float halfLine = Mathf.Max(0, length * .5f - radius);
63: Vector3 side = new Vector3(-direction.z, 0, direction.x) * radius;
65: for (int i = 1; i <= steps; i++)
67: Vector3 point = Vector3.Lerp(start, end, i / (float)steps), previous = transform.position;
68: Vector3 rear = previous - direction * halfLine, tip = point + direction * halfLine;
69: Vector3 terrainRear = previous - direction * (length * .5f), terrainTip = point + direction * (length * .5f);
79: UpdateVisual(); if (remaining <= 0) Despawn();
88: Vector3 up = Vector3.Cross(camera != null ? camera.transform.forward : Vector3.down, direction);
89: if (up.sqrMagnitude < .0001f) up = Vector3.Cross(Vector3.up, direction);
91: body.transform.rotation = Quaternion.LookRotation(Vector3.Cross(direction, up), up);
92: body.transform.localScale = new Vector3(length / body.sprite.bounds.size.x, radius * 2 / body.sprite.bounds.size.y, 1);
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/EnemyDirectionalPresentation.cs

```csharp
26: [Range(0, 20)] public float boundaryHysteresisDegrees = 8;
28: public Vector2 sideOrigin, frontOrigin, backOrigin, frontDiagonalOrigin, backDiagonalOrigin;
35: || boundaryHysteresisDegrees < 0 || boundaryHysteresisDegrees > 20) return "방향 경계 유지 각도는 0~20도입니다.";
36: foreach (Vector2 value in new[] { sideOrigin, frontOrigin, backOrigin, frontDiagonalOrigin, backDiagonalOrigin })
39: if (frames == null || frames.Length == 0) return "방향별 프레임을 지정하세요.";
48: return "8방향 모드에는 앞·뒤 사선 원본이 필요합니다.";
66: private readonly Vector2[] origins;
83: case EnemyFacing.Front: return 1;
84: case EnemyFacing.Back: return 2;
85: case EnemyFacing.FrontLeft: case EnemyFacing.FrontRight: return 3;
86: case EnemyFacing.BackLeft: case EnemyFacing.BackRight: return 4;
87: default: return 0;
108: public Vector2 Origin(EnemyFacing facing)
110: Vector2 origin = origins[Bank(facing)];
115: public EnemyFacing Select(Vector3 direction, Camera camera, EnemyFacing current, bool initialized)
117: direction.y = 0;
118: if (direction.sqrMagnitude < .000001f) return current;
119: Vector3 right = camera != null ? Vector3.ProjectOnPlane(camera.transform.right, Vector3.up) : Vector3.right;
120: if (right.sqrMagnitude < .000001f) right = Vector3.right;
122: Vector3 up = Vector3.Cross(right, Vector3.up);
123: if (camera != null && Vector3.Dot(up, camera.transform.up) < 0) up = -up;
124: float angle = Mathf.Atan2(Vector3.Dot(direction, up), Vector3.Dot(direction, right)) * Mathf.Rad2Deg;
125: float sector = Mode == EnemyDirectionMode.Eight ? 45 : 90;
126: if (initialized && Mathf.Abs(Mathf.DeltaAngle((int)current * 45, angle)) <= sector * .5f + Hysteresis) return current;
127: int index = Mathf.RoundToInt(angle / sector) * (Mode == EnemyDirectionMode.Eight ? 1 : 2);
128: return (EnemyFacing)((index % 8 + 8) % 8);
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/EnemyGroundTelegraph.cs

```csharp
16: const int segments = 96;
17: var vertices = new Vector3[segments + 1]; var uv = new Vector2[vertices.Length]; var triangles = new int[segments * 3];
18: for (int i = 0; i < segments; i++)
20: float angle = i * Mathf.PI * 2 / segments;
21: vertices[i + 1] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
22: triangles[i * 3] = 0; triangles[i * 3 + 1] = 1 + (i + 1) % segments; triangles[i * 3 + 2] = i + 1;
26: return Create(parent, circle, new Vector3(radius, 1, radius), color);
29: public static MeshRenderer Rectangle(Transform parent, Vector2 size, Color color)
32: rectangle = Build("DangerRectangle", new[] { new Vector3(-.5f, 0, -.5f), new Vector3(.5f, 0, -.5f),
33: new Vector3(.5f, 0, .5f), new Vector3(-.5f, 0, .5f) }, new Vector2[4], new[] { 0, 2, 1, 0, 3, 2 });
34: return Create(parent, rectangle, new Vector3(size.x, 1, size.y), color);
37: private static Mesh Build(string name, Vector3[] vertices, Vector2[] uv, int[] triangles)
39: var colors = new Color[vertices.Length]; for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
44: private static MeshRenderer Create(Transform parent, Mesh mesh, Vector3 scale, Color color)
47: { hideFlags = HideFlags.HideAndDontSave, mainTexture = Texture2D.whiteTexture };
50: var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material; renderer.sortingOrder = 70;
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/GasSacElitePattern.cs

```csharp
15: private Vector3 anchor;
22: private readonly SpriteRenderer[] puffs = new SpriteRenderer[8];
25: private Vector2 shadowSize;
27: private float shadowPoseScale = 1;
32: public Vector3 TelegraphCenter => marker != null ? marker.transform.position : transform.position;
63: shadowPoseScale = 1;
65: BurstCount = 0;
82: Vector3 destination = chase ? player.transform.position : anchor;
83: Vector3 direction = destination - enemy.transform.position; direction.y = 0;
84: float stopDistance = chase ? triggerDistance * .8f : .15f;
98: Vector3 delta = player.transform.position - enemy.transform.position; delta.y = 0;
104: private IEnumerator BurstCycle(Vector3 direction)
106: EnemyDamageRequest damage = enemy.CreatePatternDamage(GasSacPatternSettings.DamageId, 0);
112: float elapsed = 0;
116: float t = Mathf.Clamp01(elapsed / windup);
117: shadowPoseScale = Mathf.Lerp(1, 1.18f, t);
119: enemy.SetPatternFrame(inflation[Mathf.Min(inflation.Length - 1, Mathf.FloorToInt(t * inflation.Length))]);
128: shadowPoseScale = 1.25f;
130: boundary.widthMultiplier = .12f;
131: EnemyGroundTelegraph.SetColor(fill, new Color(burstColor.r, burstColor.g, burstColor.b, Mathf.Max(dangerFillColor.a, .5f)));
133: elapsed = 0;
136: float t = Mathf.Clamp01(elapsed / burstVisual);
137: for (int i = 0; i < puffs.Length; i++)
139: float angle = i * Mathf.PI * 2 / puffs.Length;
141: puffs[i].transform.localPosition = new Vector3(Mathf.Cos(angle), .35f, Mathf.Sin(angle)) * (radius * (.2f + t * .65f));
142: puffs[i].transform.localScale = Vector3.one * (.5f + t * .4f);
143: puffs[i].color = new Color(gasColor.r, gasColor.g, gasColor.b, gasColor.a * (1f - t));
149: elapsed = 0;
159: shadowPoseScale = 1;
170: Vector3 delta = player.transform.position - TelegraphCenter; delta.y = 0;
175: Vector2Int origin = biome.WorldToGrid(TelegraphCenter);
176: Vector2Int target = biome.WorldToGrid(player.transform.position);
190: boundary.positionCount = 96; boundary.widthMultiplier = .07f; boundary.sortingOrder = 80;
192: for (int i = 0; i < boundary.positionCount; i++)
194: float angle = i * Mathf.PI * 2 / boundary.positionCount;
195: boundary.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
198: for (int i = 0; i < puffs.Length; i++)
201: puffs[i].sortingOrder = 3000;
217: Vector3 center = enemy.transform.position;
218: center.y = (BiomeManager.Active != null ? BiomeManager.Active.GetGroundHeight(center) : center.y) + .08f;
235: for (int i = 0; i < 3; i++)
238: layer.sortingOrder = 55 + i;
239: layer.transform.localScale = Vector3.one * (1f - i * .18f);
243: for (int i = 0; i < layers.Length; i++)
246: layers[i].color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowColor.a * (.4f + i * .22f));
257: Vector3 position = enemy.transform.position;
258: position.y = (BiomeManager.Active != null ? BiomeManager.Active.GetGroundHeight(position) : position.y) + .035f;
260: groundShadow.transform.rotation = Quaternion.Euler(90, 0, 0);
261: groundShadow.transform.localScale = new Vector3(shadowSize.x * .5f, shadowSize.y * .5f, 1) * shadowPoseScale;
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/GasSacElitePattern.Orb.cs

```csharp
13: public Vector3 LockedOrbDirection { get; private set; }
29: ActiveOrb = null; OrbCount = 0; LockedOrbDirection = Vector3.zero;
39: Vector3 delta = player.transform.position - enemy.transform.position; delta.y = 0;
52: private bool CanLaunchOrb(PlayerController player, Vector3 direction)
56: && GasSacOrb.IsVisible(enemy.transform.position + Vector3.up * .6f)
65: EnemyDamageRequest damage = enemy.CreatePatternDamage(GasSacPatternSettings.OrbDamageId, 0);
70: float elapsed = 0;
74: float t = Mathf.Clamp01(elapsed / orbWindup);
76: enemy.SetPatternFrame(inflation[Mathf.Min(inflation.Length - 1, Mathf.FloorToInt(t * inflation.Length))]);
77: shadowPoseScale = Mathf.Lerp(1, 1.12f, t);
89: enemy.SetPatternFrame(deflated); shadowPoseScale = 1.15f;
90: elapsed = 0;
98: enemy.SetPatternFrame(idle); shadowPoseScale = 1;
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/GasSacOrb.cs

```csharp
11: private Vector3 direction;
15: public Vector3 Direction => direction;
19: public Vector3 LaunchPosition { get; private set; }
21: public static bool TryGetLaunchPosition(EnemyController owner, Vector3 direction, float radius, out Vector3 position)
23: position = owner != null ? owner.transform.position : Vector3.zero;
24: direction.y = 0;
25: if (owner == null || direction.sqrMagnitude < .000001f || radius <= 0) return false;
28: Vector3 extents = collider != null ? collider.bounds.extents : Vector3.zero;
30: Vector3 origin = owner.GetPatternVisualOrigin();
32: position = origin + direction * (extent + radius * .5f);
39: Vector3 direction, EnemyDamageRequest damage, float speed, float duration, float radius,
42: direction.y = 0;
43: if (!TryGetLaunchPosition(owner, direction, radius, out Vector3 launch)) return null;
52: AddLayer(orb.body, "Outline", circle, new Color(.22f, .12f, .24f, 1), radius * 1.1f, Vector3.zero, 5000, true);
53: AddLayer(orb.body, "Gas", puff, new Color(color.r, color.g, color.b, 1), radius * 2, Vector3.zero, 5001, true);
54: AddLayer(orb.body, "Highlight", circle, new Color(1, .98f, .76f, .95f), radius * .23f,
55: new Vector3(-radius * .25f, radius * .3f, -.01f), 5002, true);
56: orb.shadow = AddLayer(go.transform, "GroundShadow", circle, new Color(.15f, .07f, .18f, .35f),
57: radius, Vector3.zero, 65).transform;
66: private static SpriteRenderer AddLayer(Transform parent, string name, Sprite sprite, Color color, float scale, Vector3 offset, int order, bool sortWithWorld = false)
69: go.transform.localPosition = offset; go.transform.localScale = Vector3.one * scale;
83: if (lifetime == null || !lifetime.IsCurrent(generation) || remaining <= 0) { Despawn(); return; }
85: Vector3 start = transform.position, end = start + direction * (speed * stepTime);
87: int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(start, end) / .15f));
89: for (int i = 1; i <= steps; i++)
91: Vector3 point = Vector3.Lerp(start, end, i / (float)steps);
93: Vector3 previous = transform.position; transform.position = point;
102: if (remaining <= 0) Despawn();
105: internal static bool SweepTouchesPlayer(Vector3 from, Vector3 to, float radius, PlayerController player)
108: private static Vector2 Planar(Vector3 value) => new Vector2(value.x, value.z);
110: private static float SegmentDistanceSquared(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
112: Vector2 ab = b - a, cd = d - c, ac = c - a;
114: if (Mathf.Abs(cross) > .000001f)
118: if (t >= 0 && t <= 1 && u >= 0 && u <= 1) return 0;
125: private static float PointSegmentDistanceSquared(Vector2 point, Vector2 from, Vector2 to)
127: Vector2 delta = to - from;
128: float t = delta.sqrMagnitude > .000001f ? Mathf.Clamp01(Vector2.Dot(point - from, delta) / delta.sqrMagnitude) : 0;
132: public static Vector3 GetFlightPosition(Vector3 position, float height)
135: return position + Vector3.up * height;
140: if (Mathf.Abs(delta) < .00001f) return start >= min && start <= max;
146: public static bool IsVisible(Vector3 point)
150: Vector3 view = camera.WorldToViewportPoint(point);
151: return view.z > 0 && view.x >= 0 && view.x <= 1 && view.y >= 0 && view.y <= 1;
154: public static bool HasClearPath(Vector3 from, Vector3 to)
158: Vector2Int origin = biome.WorldToGrid(from);
160: int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(from, to) / .15f));
161: for (int i = 0; i <= steps; i++)
163: Vector3 point = Vector3.Lerp(from, to, i / (float)steps);
164: Vector2Int grid = biome.WorldToGrid(point);
172: private static bool HasClearFlightPath(Vector3 from, Vector3 to, float radius)
177: Vector2 a = Planar(from), b = Planar(to);
178: Vector2Int first = biome.WorldToGrid(from);
180: Vector2Int min = biome.WorldToGrid(new Vector3(Mathf.Min(a.x, b.x) - radius, 0, Mathf.Min(a.y, b.y) - radius));
181: Vector2Int max = biome.WorldToGrid(new Vector3(Mathf.Max(a.x, b.x) + radius, 0, Mathf.Max(a.y, b.y) + radius));
182: float half = biome.TileSize * .5f, radiusSquared = radius * radius;
186: Vector3 center = biome.GridToWorld(x, y);
188: float near = 0, far = 1;
192: Vector2 bl = new Vector2(left, bottom), br = new Vector2(right, bottom);
193: Vector2 tl = new Vector2(left, top), tr = new Vector2(right, top);
204: Vector3 ground = transform.position;
206: body.position = ground + Vector3.up * FlightHeight;
209: shadow.position = ground + Vector3.up * .04f;
210: shadow.rotation = Quaternion.Euler(90, 0, 0);
211: shadow.localScale = new Vector3(radius, radius * .65f, 1);
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/GasSacPatternSettings.cs

```csharp
13: [Min(.1f)] public float triggerDistance = 2.5f;
14: [Min(.1f)] public float burstRadius = 3.25f;
15: [Min(.1f)] public float windupSeconds = 1.3f;
16: [Min(.05f)] public float burstVisualSeconds = .18f;
17: [Min(.1f)] public float recoverySeconds = 1.5f;
19: [Min(0)] public float rearmSeconds = 2.5f;
20: [Min(0)] public float spawnGraceSeconds = .75f;
24: [Min(.1f)] public float orbExitDistance = 3f;
26: [Min(.1f)] public float orbEnterDistance = 3.8f;
29: [Min(.1f)] public float orbMaxDistance = 6.5f;
31: [Min(.1f)] public float orbWindupSeconds = 1.1f;
32: [Min(.1f)] public float orbSpeed = 3f;
33: [Min(.1f)] public float orbLifetimeSeconds = 3f;
35: [Min(.05f)] public float orbHitRadius = .3f;
36: [Min(.1f)] public float orbRecoverySeconds = 1.2f;
38: [Min(0)] public float orbRearmSeconds = 2.5f;
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/GasSacPresentation.cs

```csharp
16: [Min(.02f)] public float deathFrameSeconds = .14f;
20: public Color warningColor = new Color(1f, .2f, .16f, .95f);
22: public Color dangerFillColor = new Color(1f, .04f, .03f, .42f);
23: public Color burstColor = new Color(.92f, .3f, .2f, 1f);
24: public Color gasColor = new Color(.78f, .87f, .56f, .8f);
26: public Vector2 groundShadowSize = new Vector2(1.3f, .8f);
27: public Color groundShadowColor = new Color(.12f, .055f, .16f, .32f);
32: || inflationFrames == null || inflationFrames.Length < 2) return "가스낭 표현 스프라이트가 필요합니다.";
34: if (deathFrames == null || deathFrames.Length < 2 || !MonsterBalanceNumbers.Positive(deathFrameSeconds))
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/HangoverRemnantElitePattern.cs

```csharp
14: private Vector3 anchor, center, retreatDirection;
21: private Vector3 Position => physicsBody != null ? physicsBody.position : transform.position;
27: public Vector3 RemnantCenter => center;
28: public Vector3 RetreatDirection => retreatDirection;
57: body = source.transform.Find("Visual").GetComponent<SpriteRenderer>(); SetFrame(idle[0]);
58: CreateShadow(art); nextReady = Time.time + settings.spawnGraceSeconds; nextSafetyCheck = hitUntil = 0;
59: ReleaseCount = BurstCount = RejectedPaths = 0; ReleasedAt = BurstAt = 0;
60: center = Position; retreatDirection = Vector3.zero;
71: Vector3 delta = (chase ? player.transform.position : anchor) - Position; delta.y = 0;
72: float stop = chase ? triggerDistance * .8f : .15f;
73: bool moving = delta.magnitude > stop && enemy.Stats.MoveSpeed > 0;
75: SetFrame(Time.time < hitUntil ? hit : (moving ? move : idle)[Mathf.FloorToInt(Time.time / (moving ? moveTime : idleTime)) % 2]);
84: Vector3 delta = player.transform.position - Position; delta.y = 0;
85: if (delta.sqrMagnitude < .0001f || delta.sqrMagnitude > triggerDistance * triggerDistance
86: || !GasSacOrb.IsVisible(Position + Vector3.up * .6f) || !GasSacOrb.HasClearPath(Position, player.transform.position)) return false;
87: Vector3 direction = -delta.normalized;
89: { RejectedPaths++; nextSafetyCheck = Time.time + .25f; return false; }
98: var damage = enemy.CreatePatternDamage(HangoverRemnantPatternSettings.DamageId, 0);
102: marker = new GameObject("H03_DangerArea"); marker.transform.position = center + Vector3.up * .055f;
108: SetFrame(preparation[Mathf.Min(2, Mathf.FloorToInt((Time.time - began) / windup * 3))]); yield return null;
112: float travelled = 0, recoveryAt = -1;
117: if (recoveryAt < 0)
120: Vector3 from = Position, to = from + retreatDirection * step;
125: Vector3 moved = Position - from; moved.y = 0; travelled += moved.magnitude;
126: if (travelled >= retreatDistance - .001f) recoveryAt = Time.time;
128: SetFrame(elapsed < releaseTime ? release : retreat[Mathf.FloorToInt(elapsed / retreatTime) % 2]);
130: if (recoveryAt >= 0) { Phase = HangoverRemnantPhase.Recovery; SetFrame(recovery); }
142: else remnantBody.sprite = remnantFrames[Mathf.FloorToInt(elapsed / remnantTime) % 2];
144: if (recoveryAt >= 0 && Time.time - recoveryAt >= recoverySeconds && remnant == null) break;
152: { SetFrame(idle[0]); nextReady = Time.time + rearm; Phase = HangoverRemnantPhase.Ready; }
156: private bool ClearRetreat(Vector3 from, Vector3 to)
159: Vector3 leash = to - anchor; leash.y = 0;
160: if (enemy.Config.leashRadius < 0 || leash.sqrMagnitude > enemy.Config.leashRadius * enemy.Config.leashRadius) return false;
161: var collider = enemy.GetComponent<Collider>(); Vector3 extents = collider != null ? collider.bounds.extents : Vector3.one * .4f;
162: if (!ResidueRubble.CanTraverse(from, to, new Vector2(extents.x, extents.z))) return false;
164: Vector2Int first = biome.WorldToGrid(from); int level = biome.GetHeightLevel(first.x, first.y);
165: int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(from, to) / .1f));
166: for (int i = 0; i <= steps; i++)
168: Vector3 p = Vector3.Lerp(from, to, (float)i / steps);
169: Vector2Int min = biome.WorldToGrid(p - new Vector3(extents.x, 0, extents.z));
170: Vector2Int max = biome.WorldToGrid(p + new Vector3(extents.x, 0, extents.z));
178: private bool ClearBurstArea(Vector3 point)
181: Vector2Int first = biome.WorldToGrid(point); int level = biome.GetHeightLevel(first.x, first.y);
182: Vector2Int min = biome.WorldToGrid(point - new Vector3(radius, 0, radius)), max = biome.WorldToGrid(point + new Vector3(radius, 0, radius));
183: float half = biome.TileSize * .5f;
186: Vector3 c = biome.GridToWorld(x, z); float dx = Mathf.Max(0, Mathf.Abs(c.x - point.x) - half), dz = Mathf.Max(0, Mathf.Abs(c.z - point.z) - half);
198: Vector3 delta = player.transform.position - center; delta.y = 0;
203: remnant = new GameObject("H03_Remnant"); remnant.transform.position = center; Own(remnant);
205: remnantBody.sprite = remnantFrames[0];
211: private void OnDamaged(EnemyController source, float amount) { if (amount > 0) hitUntil = Time.time + .12f; }
218: var renderer = shadow.GetComponent<SpriteRenderer>(); renderer.sprite = art.groundDisc; renderer.color = art.groundShadowColor; renderer.sortingOrder = 56;
219: shadow.transform.rotation = Quaternion.Euler(90, 0, 0); shadow.transform.localScale = new Vector3(art.groundShadowSize.x * .5f, art.groundShadowSize.y * .5f, 1); shadow.SetActive(true); LateUpdate();
232: Vector3 p = transform.position; if (BiomeManager.Active != null) p.y = BiomeManager.Active.GetGroundHeight(p);
233: shadow.transform.position = p + Vector3.up * .035f;
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/HangoverRemnantPatternSettings.cs

```csharp
9: [Min(.1f)] public float triggerDistance = 2.5f;
10: [Min(0)] public float spawnGraceSeconds = .75f;
11: [Min(.1f)] public float windupSeconds = .65f;
12: [Min(.1f)] public float retreatDistance = 2.4f;
13: [Min(.1f)] public float retreatSpeed = 6f;
14: [Min(.1f)] public float burstDelaySeconds = 1.1f;
16: [Min(.1f)] public float burstRadius = 1.6f;
17: [Min(.1f)] public float recoverySeconds = 1f;
19: [Min(0)] public float rearmSeconds = 3.5f;
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/HangoverRemnantPresentation.cs

```csharp
11: [Min(.02f)] public float idleFrameSeconds = .35f;
12: [Min(.02f)] public float moveFrameSeconds = .22f;
13: [Min(.02f)] public float releaseFrameSeconds = .1f;
14: [Min(.02f)] public float retreatFrameSeconds = .2f;
15: [Min(.02f)] public float deathFrameSeconds = .14f;
16: [Min(.02f)] public float remnantFrameSeconds = .18f;
17: [Min(.02f)] public float burstFrameSeconds = .1f;
18: public Color dangerFillColor = new Color(.92f, .04f, .04f, .3f);
19: public Vector2 groundShadowSize = new Vector2(1.45f, .8f);
20: public Color groundShadowColor = new Color(.14f, .065f, .12f, .28f);
24: if (!Valid(idleFrames, 2) || !Valid(moveFrames, 2) || !Valid(preparationFrames, 3) || !Valid(retreatFrames, 2)
25: || !Valid(deathFrames, 6) || !Valid(remnantFrames, 2) || !Valid(burstFrames, 4)
27: return "정면 본체 12·사망 6·잔여물 2·터짐 4 프레임과 그림자를 지정하세요.";
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/HardenedResidueElitePattern.cs

```csharp
14: private Vector3 anchor, landingCenter, landingDirection, attackDirection;
15: private Vector2 footprint;
27: public Vector3 LandingCenter => landingCenter;
28: public Vector3 LandingDirection => landingDirection;
29: public Vector2 Footprint => footprint;
36: public Vector3 LaunchPosition { get; private set; }
47: nextReady = Time.time + settings.spawnGraceSeconds; nextSafetyCheck = 0; hitUntil = 0;
58: source.AlignPatternFeetToGround(); source.SetPatternFrame(idle[0]); source.SetAiSuppressed(true);
61: ActiveRubble = null; LaunchPosition = Vector3.zero; UsesVerticalRubble = false;
62: LandingCount = RejectedPlacements = 0; Phase = HardenedResiduePhase.Ready;
73: Vector3 delta = (chase ? player.transform.position : anchor) - transform.position; delta.y = 0;
74: float stop = chase ? placementDistance : .15f;
75: bool moving = delta.magnitude > stop && enemy.Stats.MoveSpeed > 0;
89: Vector3 delta = player.transform.position - transform.position; delta.y = 0;
91: Vector3 aim = delta.sqrMagnitude > .0001f ? delta.normalized : Vector3.forward;
95: Vector3 screenRight = camera != null ? Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized : Vector3.right;
96: if (screenRight.sqrMagnitude < .0001f) screenRight = Vector3.right;
98: landingDirection = UsesVerticalRubble ? -screenRight : Vector3.Cross(screenRight, Vector3.up);
102: { RejectedPlacements++; nextSafetyCheck = Time.time + .25f; return false; }
110: EnemyDamageRequest damage = enemy.CreatePatternDamage(HardenedResiduePatternSettings.DamageId, 0);
115: CreateCrack(); float elapsed = 0;
119: enemy.SetPatternFrame(lift[Mathf.Min(lift.Length - 1, Mathf.FloorToInt(elapsed / windup * lift.Length))]);
125: LaunchPosition = enemy.HasPatternDirections ? enemy.GetPatternVisualOrigin() : transform.position + Vector3.up * 1.5f;
129: UsesVerticalRubble ? verticalRubbleGroundDepthFraction : 0);
130: elapsed = 0;
134: float t = Mathf.Clamp01(elapsed / dropSeconds);
135: ActiveRubble.SetAirPosition(Vector3.Lerp(LaunchPosition, ActiveRubble.LandingVisualPosition, t));
143: elapsed = 0;
152: { enemy.SetPatternFrame(idle[0]); nextReady = Time.time + rearm; Phase = HardenedResiduePhase.Ready; }
164: Vector2Int a = biome.WorldToGrid(landingCenter), b = biome.WorldToGrid(player.transform.position);
172: marker = new GameObject("I02_LandingCrack"); marker.transform.position = landingCenter + Vector3.up * .06f;
178: crack.widthMultiplier = .065f; crack.sortingOrder = 82; crack.startColor = crack.endColor = crackColor;
179: crack.positionCount = 9;
180: for (int i = 0; i < 9; i++) crack.SetPosition(i, new Vector3(Mathf.Lerp(-footprint.x * .5f, footprint.x * .5f, i / 8f), 0,
181: i == 0 || i == 8 ? 0 : (i % 2 == 0 ? 1 : -1) * footprint.y * .28f));
183: edge.sharedMaterial = lineMaterial; edge.useWorldSpace = false; edge.loop = true; edge.positionCount = 4;
184: edge.widthMultiplier = .025f; edge.sortingOrder = 80;
185: edge.startColor = edge.endColor = new Color(crackColor.r, crackColor.g, crackColor.b, .55f);
186: edge.SetPositions(new[] { new Vector3(-footprint.x * .5f, 0, -footprint.y * .5f), new Vector3(footprint.x * .5f, 0, -footprint.y * .5f),
187: new Vector3(footprint.x * .5f, 0, footprint.y * .5f), new Vector3(-footprint.x * .5f, 0, footprint.y * .5f) });
192: private void OnDamaged(EnemyController source, float damage) { if (damage > 0) hitUntil = Time.time + .12f; }
195: private void CreateShadow(Vector2 size, Color color)
202: var renderer = shadow.GetComponent<SpriteRenderer>(); renderer.sprite = groundDisc; renderer.color = color; renderer.sortingOrder = 56;
203: shadow.transform.rotation = Quaternion.Euler(90, 0, 0); shadow.transform.localScale = new Vector3(size.x * .5f, size.y * .5f, 1);
210: Vector3 position = transform.position;
212: shadow.transform.position = position + Vector3.up * .035f;
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/HardenedResiduePatternSettings.cs

```csharp
10: [Min(.1f)] public float triggerDistance = 2.8f;
11: [Min(.1f)] public float placementDistance = 2.3f;
13: public Vector2 footprint = new Vector2(2.2f, .7f);
14: [Min(.1f)] public float windupSeconds = .8f;
15: [Min(.02f)] public float dropSeconds = .12f;
16: [Min(.1f)] public float recoverySeconds = 1f;
18: [Min(0)] public float rearmSeconds = 2.5f;
19: [Min(0)] public float spawnGraceSeconds = .75f;
21: [Min(.1f)] public float rubbleLifetime = 2f;
22: [Tooltip("서로 다른 유효 플레이어 타격 수. 기본 1회. 동일 투사체의 중복 충돌은 한 번만 셉니다.")]
23: [Min(1)] public int hitsToBreak = 1;
34: || !MonsterBalanceNumbers.NonNegative(spawnGraceSeconds) || hitsToBreak < 1)
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/HardenedResiduePresentation.cs

```csharp
17: [Range(.25f, 1f)]
19: public float verticalRubbleGroundDepthFraction = .78f;
21: [Min(.02f)] public float idleFrameSeconds = .35f;
22: [Min(.02f)] public float moveFrameSeconds = .22f;
23: [Min(.02f)] public float deathFrameSeconds = .14f;
25: public Color crackColor = new Color(1f, .2f, .16f, .95f);
27: public Color dangerFillColor = new Color(1f, .04f, .03f, .42f);
28: public Color groundShadowColor = new Color(.14f, .065f, .12f, .28f);
29: public Vector2 groundShadowSize = new Vector2(1.65f, .75f);
40: || verticalRubbleGroundDepthFraction < .25f || verticalRubbleGroundDepthFraction > 1)
41: return "세로 잔해의 바닥 길이 비율은 0.25~1 사이여야 합니다.";
43: if (!Valid(idleFrames, 1) || !Valid(moveFrames, 2) || !Valid(liftFrames, 2)
44: || !Valid(deathFrames, 2) || release == null || recovery == null || hit == null || rubble == null || groundDisc == null)
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/HelicoGroundedVisual.cs

```csharp
7: [DefaultExecutionOrder(200)]
14: public Vector3 GroundContactPosition { get; private set; }
26: if (body == null || body.sprite == null || pattern == null || camera == null || camera.transform.forward.y >= -.1f) return;
29: Vector3 ground = physicsBody != null ? physicsBody.position : transform.position;
31: Vector3 screenDepth = Vector3.ProjectOnPlane(camera.transform.up, Vector3.up).normalized;
32: float nearAxis = pattern.BodyHalfLength * Mathf.Abs(Vector3.Dot(pattern.BodyDirection, screenDepth));
33: GroundContactPosition = ground - screenDepth * nearAxis + Vector3.up * .025f;
35: Vector3 position = GroundContactPosition - camera.transform.up * (bottom * body.transform.lossyScale.y);
39: float lift = Mathf.Max(0, ground.y + .025f - lowest);
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/HelicoSpiralElitePattern.cs

```csharp
13: private Vector3 anchor, bodyDirection = Vector3.right;
21: public Vector3 LockedDirection { get; private set; }
22: public Vector3 DashStart { get; private set; }
23: public Vector3 DashEnd { get; private set; }
33: public Vector3 BodyDirection => bodyDirection;
38: private Vector3 Position => enemy.GetComponent<Rigidbody>().position;
48: distance = settings.dashDistance; dashTime = settings.dashSeconds; radius = settings.bodyWidth * .5f; halfLength = settings.bodyHalfLength;
55: source.BindPatternDirections(art.directionalPresentation); source.AlignPatternFeetToGround(); source.SetPatternFrame(idle[0]);
60: bodyDirection = Vector3.right; source.SetPatternFacing(bodyDirection, false);
61: nextReady = Time.time + settings.spawnGraceSeconds; hitUntil = 0; DashCount = HitAttempts = DamageApplications = 0;
62: PlannedDistance = Travelled = ReleasedAt = 0; WallStopped = false; Phase = HelicoSpiralPhase.Ready;
73: Vector3 delta = (chase ? player.transform.position : anchor) - Position; delta.y = 0;
74: float stopAt = chase ? stop : .15f;
75: bool moving = delta.magnitude > stopAt && enemy.Stats.MoveSpeed > 0;
77: if ((moving || chase) && delta.sqrMagnitude > .0001f) { bodyDirection = delta.normalized; enemy.SetPatternFacing(delta, false); }
86: Vector3 delta = player.transform.position - Position; delta.y = 0;
87: if (delta.sqrMagnitude < .01f || delta.magnitude > trigger || !GasSacOrb.IsVisible(Position) || !GasSacOrb.HasClearPath(Position, player.transform.position)) return false;
88: LockedDirection = bodyDirection = delta.normalized; DashStart = Position; PlannedDistance = 0;
90: for (float d = Mathf.Min(.1f, distance); d <= distance + .0001f; d = Mathf.Min(distance, d + .1f))
95: if (PlannedDistance < .2f) { nextReady = Time.time + .25f; return false; }
99: private bool CanStep(Vector3 from, Vector3 to)
102: Vector2 half = new Vector2(collider.bounds.extents.x, collider.bounds.extents.z);
109: HitAttempts = DamageApplications = 0; Travelled = 0; WallStopped = PlannedDistance < distance - .01f;
111: var damage = enemy.CreatePatternDamage(HelicoSpiralPatternSettings.DamageId, 0);
114: float elapsed = 0;
118: enemy.SetPatternFrame(preparation[Mathf.Min(2, Mathf.FloorToInt(elapsed / windup * 3))]);
122: Phase = HelicoSpiralPhase.Dash; DashCount++; ReleasedAt = Time.time; elapsed = 0;
124: while (Travelled < PlannedDistance - .001f)
128: int slices = Mathf.Max(1, Mathf.CeilToInt(step / .08f));
130: for (int i = 0; i < slices; i++)
132: Vector3 from = Position, to = from + LockedDirection * (step / slices);
136: elapsed += Time.deltaTime; enemy.SetPatternFrame(dash[Mathf.Min(1, Mathf.FloorToInt(elapsed / dashTime * 2))]);
141: Phase = HelicoSpiralPhase.Recovery; elapsed = 0;
144: enemy.SetPatternFrame(recovery[Mathf.Min(1, Mathf.FloorToInt(elapsed / recoveryTime * 2))]);
154: if (lifetime.IsCurrent(generation)) { Phase = HelicoSpiralPhase.Ready; nextReady = Time.time + rearm; enemy.SetPatternFrame(idle[0]); }
160: TelegraphObject = new GameObject("S01_ChargePath");
161: TelegraphObject.transform.position = DashStart + LockedDirection * (PlannedDistance * .5f) + Vector3.up * .055f;
162: TelegraphObject.transform.rotation = Quaternion.LookRotation(LockedDirection, Vector3.up);
163: float line = PlannedDistance + halfLength * 2;
164: EnemyGroundTelegraph.Rectangle(TelegraphObject.transform, new Vector2(radius * 2, line), danger);
165: foreach (float end in new[] { -.5f, .5f })
167: var cap = new GameObject("RoundEnd"); cap.transform.SetParent(TelegraphObject.transform, false); cap.transform.localPosition = Vector3.forward * (line * end);
172: if (borderMaterial == null) borderMaterial = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave, mainTexture = Texture2D.whiteTexture };
174: border.useWorldSpace = false; border.loop = true; border.widthMultiplier = .025f;
175: border.sharedMaterial = borderMaterial; border.startColor = border.endColor = dangerBorder; border.sortingOrder = 71;
177: var outline = new Vector3[66];
178: for (int i = 0; i <= 32; i++)
180: float front = (-90 + 180f * i / 32) * Mathf.Deg2Rad, back = (90 + 180f * i / 32) * Mathf.Deg2Rad;
181: outline[i] = new Vector3(Mathf.Sin(front) * radius, .004f, line * .5f + Mathf.Cos(front) * radius);
182: outline[33 + i] = new Vector3(Mathf.Sin(back) * radius, .004f, -line * .5f + Mathf.Cos(back) * radius);
191: private void ApplyDashDamage(Vector3 from, Vector3 to, EnemyDamageRequest damage)
194: if (HitAttempts != 0 || player == null || player.IsDead || MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position)
208: float hp = player.HealthComponent.CurrentHealth; player.TakeDamage(enemy.CreateContactDamage(0));
211: Vector3 away = player.transform.position - Position; away.y = 0;
212: if (away.sqrMagnitude < .0001f) away = -bodyDirection;
217: private void OnDamaged(EnemyController source, float amount) { if (amount > 0 && Phase == HelicoSpiralPhase.Ready) hitUntil = Time.time + .12f; }
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/HelicoSpiralElitePattern.Tail.cs

```csharp
8: public float TailProgress => 0;
10: public int TailHitAttempts => 0;
12: public float TailWindupDuration => 0;
13: public float TailSweepDuration => 0;
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/HelicoSpiralPatternSettings.cs

```csharp
9: [Min(.1f)] public float triggerDistance = 4.5f;
10: [Min(.1f)] public float approachStopDistance = 2.5f;
11: [Min(0)] public float spawnGraceSeconds = .75f;
12: [Min(.1f)] public float windupSeconds = .7f;
13: [Min(.1f)] public float dashDistance = 3.5f;
14: [Min(.05f)] public float dashSeconds = .25f;
16: [Min(.1f)] public float bodyWidth = .8f;
17: [Tooltip("몸 중앙에서 앞뒤로 이어지는 캡슐 중심선의 반길이. 반원 반경은 몸통 폭/2입니다.")]
18: [Min(0)] public float bodyHalfLength = .35f;
19: [Min(.1f)] public float recoverySeconds = .9f;
20: [Min(0)] public float rearmSeconds = 2.5f;
23: [HideInInspector, Min(.1f)] public float tailWindupSeconds = .9f;
24: [HideInInspector, Min(.05f)] public float tailSweepSeconds = .18f;
25: [HideInInspector, Min(.1f)] public float tailReach = 1.1f;
26: [HideInInspector, Range(10, 160)] public float tailArcDegrees = 120;
27: [HideInInspector, Min(.01f)] public float tailWidth = .09f;
28: [HideInInspector, Min(.1f)] public float tailRecoverySeconds = 1.25f;
29: [HideInInspector, Min(0)] public float tailRearmSeconds = 2.5f;
36: if (!MonsterBalanceNumbers.NonNegative(n)) return "나선충 대기·몸통 반길이는 0 이상이어야 합니다.";
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/HelicoSpiralPresentation.cs

```csharp
10: public Vector2[] bodyHull;
11: public Vector2 attachment;
30: [Min(.02f)] public float idleFrameSeconds = .3f;
31: [Min(.02f)] public float moveFrameSeconds = .2f;
32: [Min(.02f)] public float deathFrameSeconds = .14f;
33: public Color dangerColor = new Color(.72f, .035f, .1f, .58f);
34: public Color dangerBorderColor = new Color(1, .68f, .72f, .95f);
38: int[] counts = { 3, 3, 2 };
39: for (int i = 0; i < banks.Length; i++)
40: if (banks[i] == null || banks[i].Length != counts[i] || Array.Exists(banks[i], s => s == null)) return "꼬리 예고3/휩쓸기3/회복2가 필요합니다.";
50: if (!Array.Exists(tailBodies, g => g != null && g.sprite == snapshot.Resolve(source, facing) && g.bodyHull != null && g.bodyHull.Length >= 3))
59: if (directionalPresentation == null || directionalPresentation.mode != EnemyDirectionMode.Eight) return "나선충은 사선을 포함한 8방향 원본이 필요합니다.";
62: int[] counts = { 2, 2, 3, 2, 2, 6 };
63: for (int i = 0; i < banks.Length; i++)
64: if (banks[i] == null || banks[i].Length != counts[i] || Array.Exists(banks[i], s => s == null)) return "나선충 기본12장과 사망6장을 확인하세요.";
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/HelicoTailSweepGeometry.cs

```csharp
9: public readonly Vector3 Origin, Rear, Right;
11: public HelicoTailSweepGeometry(Vector3 origin, Vector3 rear, float reach, float arc, float radius)
13: Origin = origin; Rear = Vector3.ProjectOnPlane(rear, Vector3.up).normalized;
14: Right = Vector3.Cross(Vector3.up, Rear); Reach = reach; Arc = arc; Radius = radius;
16: public Vector3 Direction(float progress)
18: float a = (Mathf.Clamp01(progress) - .5f) * Arc * Mathf.Deg2Rad;
21: public Vector3 Tip(float progress) => Origin + Direction(progress) * Reach;
22: public const int CurveSegments = 24;
23: public Vector3 CurvePoint(float progress, float along, int flag = 0)
25: float p = Mathf.Clamp01(progress), t = Mathf.Clamp01(along);
26: if (flag != 0) p = Mathf.Clamp01(p - .1f * Mathf.Sin(p * Mathf.PI));
28: float bow = Arc * (flag == 0 ? .95f : .8f) * (1-p) * Mathf.Sin(p*Mathf.PI*.5f) * Mathf.Sin(t*Mathf.PI);
29: float angle = ((p-.5f)*Arc+bow)*Mathf.Deg2Rad;
30: return Origin + (Rear * Mathf.Cos(angle) + Right * Mathf.Sin(angle)) * (Reach * t * (flag == 0 ? 1 : .92f));
32: public Vector3 AirPoint(float progress, float along, int flag = 0)
34: float height = .65f + .28f * Mathf.Sin(along * Mathf.PI) + .1f * Mathf.Sin(progress * Mathf.PI) - (flag == 0 ? 0 : .09f);
35: return CurvePoint(progress, along, flag) + Vector3.up * height;
37: public bool Contains(Vector3 point, float from = 0, float to = 1)
39: if (!ContainsSector(point, 0, 1)) return false;
40: if (from <= 0 && to >= 1) return true;
43: int slices = Mathf.Max(1, Mathf.CeilToInt((to - from) * 160));
44: Vector2 target = CombatHitGeometry.Flat(point);
45: float r2 = (Radius + .0001f) * (Radius + .0001f);
46: for (int flag = 0; flag < 2; flag++) for (int slice = 0; slice < slices; slice++)
48: float p0 = Mathf.Lerp(from, to, slice / (float)slices), p1 = Mathf.Lerp(from, to, (slice + 1f) / slices);
49: Vector2 a = CombatHitGeometry.Flat(CurvePoint(p0, 0, flag)), d = CombatHitGeometry.Flat(CurvePoint(p1, 0, flag));
50: for (int j = 1; j <= CurveSegments; j++)
52: Vector2 b = CombatHitGeometry.Flat(CurvePoint(p0, j / (float)CurveSegments, flag));
53: Vector2 c = CombatHitGeometry.Flat(CurvePoint(p1, j / (float)CurveSegments, flag));
55: || CombatHitGeometry.PointSegmentDistanceSquared(target,a,b) <= r2
56: || CombatHitGeometry.PointSegmentDistanceSquared(target,b,c) <= r2
57: || CombatHitGeometry.PointSegmentDistanceSquared(target,c,d) <= r2
58: || CombatHitGeometry.PointSegmentDistanceSquared(target,d,a) <= r2) return true;
64: private static bool InTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
66: float Cross(Vector2 u,Vector2 v) => u.x*v.y-u.y*v.x;
67: if (Mathf.Abs(Cross(b-a,c-a)) < .00000001f) return false;
69: return (x>=0&&y>=0&&z>=0)||(x<=0&&y<=0&&z<=0);
71: private bool ContainsSector(Vector3 point, float from, float to)
73: Vector3 d = point - Origin; d.y = 0;
75: float edgeRadius = Radius + .0001f;
76: float a = Mathf.Atan2(Vector3.Dot(d, Right), Vector3.Dot(d, Rear)) * Mathf.Rad2Deg;
77: float first = (Mathf.Clamp01(from) - .5f) * Arc, last = (Mathf.Clamp01(to) - .5f) * Arc;
82: public Vector3[] Outline()
84: var points = new List<Vector2>();
85: for (int i = -1; i <= 80; i++)
87: float a = (i / 80f - .5f) * Arc * Mathf.Deg2Rad;
88: Vector2 tip = i < 0 ? Vector2.zero : new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * Reach;
89: for (int j = 0; j < 32; j++)
91: float b = j * Mathf.PI * 2 / 32;
92: points.Add(tip + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * Radius);
96: var hull = new List<Vector2>();
97: void Add(Vector2 p, int start)
99: while (hull.Count >= start + 2)
101: Vector2 a = hull[hull.Count - 1] - hull[hull.Count - 2], b = p - hull[hull.Count - 1];
102: if (a.x * b.y - a.y * b.x > 0) break;
103: hull.RemoveAt(hull.Count - 1);
107: foreach (var p in points) Add(p, 0);
108: int end = hull.Count - 1;
109: for (int i = points.Count - 2; i >= 0; i--) Add(points[i], end);
110: hull.RemoveAt(hull.Count - 1);
111: var result = new Vector3[hull.Count];
112: for (int i = 0; i < result.Length; i++) result[i] = Right * hull[i].x + Rear * hull[i].y;
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/HelicoTailSweepVisual.cs

```csharp
8: [DefaultExecutionOrder(250)]
21: public Vector3 VisibleTip { get; private set; }
28: material = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave, mainTexture = Texture2D.whiteTexture };
30: root = new GameObject("S01_AirborneTailAction"); root.transform.SetParent(body.transform, false);
33: flags = new LineRenderer[6];
34: for (int i = 0; i < flags.Length; i++)
36: var line = new GameObject("Flag" + (i / 3) + "Layer" + (i % 3)).AddComponent<LineRenderer>(); line.transform.SetParent(root.transform, false);
37: line.useWorldSpace = true; line.alignment = LineAlignment.View; line.sharedMaterial = material; line.positionCount = 9 + HelicoTailSweepGeometry.CurveSegments; line.numCapVertices = 2; line.numCornerVertices = 1;
39: Color c = i % 3 == 0 ? new Color(.14f,.025f,.12f) : i % 3 == 1 ? new Color(.94f,.43f,.59f) : new Color(1,.73f,.7f);
47: var sprite = frame.sprite; var vertices = new Vector3[frame.bodyHull.Length]; var uv = new Vector2[vertices.Length];
48: var colors = new Color[vertices.Length]; var triangles = new int[(vertices.Length - 2) * 3];
49: for (int i = 0; i < vertices.Length; i++)
52: Vector2 pixel = frame.bodyHull[i] * sprite.pixelsPerUnit + sprite.pivot + sprite.rect.position;
53: uv[i] = new Vector2(pixel.x / sprite.texture.width, pixel.y / sprite.texture.height);
54: if (i >= 2) { int t = (i-2)*3; triangles[t]=0; triangles[t+1]=i-1; triangles[t+2]=i; }
56: var mesh = new Mesh { name = "S01_TailBody_" + sprite.name, vertices = vertices, uv = uv, colors = colors, triangles = triangles };
64: root.transform.localScale = new Vector3(body.flipX ? -1 : 1, 1, 1);
68: Vector3 attachment = body.transform.TransformPoint(new Vector3(frame.attachment.x * (body.flipX ? -1 : 1), frame.attachment.y, 0));
69: VisibleTip = g.AirPoint(p, 1);
70: for (int i = 0; i < flags.Length; i++)
72: bool trailing = i >= 3; int flag = trailing ? 1 : 0; var line = flags[i];
73: line.widthMultiplier = g.Radius * 2 * (i % 3 == 0 ? 1 : i % 3 == 1 ? .64f : .19f);
75: Vector3 depth = camera != null ? Vector3.ProjectOnPlane(camera.transform.up, Vector3.up).normalized : Vector3.forward;
76: line.sortingOrder = body.sortingOrder + (Vector3.Dot(g.Rear, depth) > .1f ? -3 : 1) + i % 3; line.sortingLayerID = body.sortingLayerID;
77: var points = new Vector3[9 + HelicoTailSweepGeometry.CurveSegments];
78: Vector3 start = attachment + (trailing && camera != null ? camera.transform.right * .025f : Vector3.zero);
79: Vector3 joint = g.AirPoint(p, 0, flag);
80: Vector3 tangent = (g.AirPoint(p, .04f, flag) - joint).normalized;
81: Vector3 control1 = Vector3.Lerp(start, joint, .4f) + Vector3.up * .10f;
82: Vector3 control2 = joint - tangent * .12f;
83: for (int j=0;j<=8;j++)
85: float t=j/8f,u=1-t;
86: points[j]=u*u*u*start+3*u*u*t*control1+3*u*t*t*control2+t*t*t*joint;
88: for (int j=1;j<=HelicoTailSweepGeometry.CurveSegments;j++) points[8+j]=g.AirPoint(p,j/(float)HelicoTailSweepGeometry.CurveSegments,flag);
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/InflammationEmberElitePattern.cs

```csharp
14: private Vector3 anchor;
21: public Vector3 LockedDirection { get; private set; }
46: source.AlignPatternFeetToGround(); source.SetPatternFrame(idle[0]); source.SetAiSuppressed(true);
48: CreateShadow(art); Phase = InflammationEmberPhase.Ready; ShotCount = 0; ActiveThorn = null;
49: nextReady = Time.time; hitUntil = 0; LockedDirection = Vector3.zero;
59: Vector3 delta = (chase ? player.transform.position : anchor) - transform.position; delta.y = 0;
60: float stop = chase ? stopDistance : .15f;
61: bool moving = delta.magnitude > stop && enemy.Stats.MoveSpeed > 0;
70: if (damage <= 0 || source.IsDead || lifetime == null || !lifetime.IsCurrent(generation)) return;
72: hitUntil = Time.time + .12f;
75: Vector3 direction = player.transform.position - transform.position; direction.y = 0;
76: if (direction.sqrMagnitude < .0001f) return;
85: Vector3 delta = player.transform.position - transform.position; delta.y = 0;
87: && GasSacOrb.IsVisible(transform.position + Vector3.up * .6f)
95: var damage = enemy.CreatePatternDamage(InflammationEmberPatternSettings.DamageId, 0);
99: float elapsed = 0;
103: enemy.SetPatternFrame(preparation[Mathf.Min(preparation.Length - 1, Mathf.FloorToInt(elapsed / windup * preparation.Length))]);
113: Phase = InflammationEmberPhase.Recovery; elapsed = 0;
124: { enemy.SetPatternFrame(idle[0]); nextReady = Time.time + rearm; Phase = InflammationEmberPhase.Ready; }
134: var renderer = shadow.GetComponent<SpriteRenderer>(); renderer.sprite = art.groundDisc; renderer.color = art.groundShadowColor; renderer.sortingOrder = 56;
135: shadow.transform.rotation = Quaternion.Euler(90, 0, 0);
136: shadow.transform.localScale = new Vector3(art.groundShadowSize.x * .5f, art.groundShadowSize.y * .5f, 1);
142: Vector3 position = transform.position;
144: shadow.transform.position = position + Vector3.up * .035f;
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/InflammationEmberPatternSettings.cs

```csharp
9: [Min(.1f)] public float counterMaxDistance = 8f;
10: [Min(.1f)] public float approachStopDistance = 1.5f;
11: [Min(.1f)] public float windupSeconds = .7f;
12: [Min(.1f)] public float recoverySeconds = 1.2f;
14: [Min(0)] public float rearmSeconds = 2.5f;
15: [Min(.1f)] public float thornSpeed = 4f;
16: [Min(.1f)] public float thornLifetimeSeconds = 2f;
18: [Min(.1f)] public float thornLength = .7f;
20: [Min(.02f)] public float thornRadius = .16f;
30: || thornRadius * 2 > thornLength) return "염증 불씨의 거리·시간·탄 크기를 확인하세요. 가시 길이는 반경의 두 배 이상이어야 합니다.";
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/InflammationEmberPresentation.cs

```csharp
12: [Min(.02f)] public float idleFrameSeconds = .35f;
13: [Min(.02f)] public float moveFrameSeconds = .22f;
14: [Min(.02f)] public float releaseFrameSeconds = .12f;
15: [Min(.02f)] public float deathFrameSeconds = .14f;
16: [Min(.02f)] public float thornFrameSeconds = .12f;
17: public Vector2 groundShadowSize = new Vector2(1.4f, .7f);
18: public Color groundShadowColor = new Color(.14f, .065f, .12f, .28f);
27: if (!Valid(idleFrames, 2) || !Valid(moveFrames, 2) || !Valid(preparationFrames, 3)
28: || !Valid(deathFrames, 6) || !Valid(thornFrames, 2) || release == null || recovery == null || hit == null || groundDisc == null)
29: return "염증 불씨의 본체 10장·사망 6장·가시탄 2장과 그림자를 지정하세요.";
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/OilFilmElitePattern.cs

```csharp
12: private Vector3 home;
23: public Vector3 LockedDirection { get; private set; }
35: public Vector3 Position => enemy != null ? enemy.GetComponent<Rigidbody>().position : transform.position;
36: public Vector3 GroundOrigin
38: get { Vector3 p = Position; if (BiomeManager.Active != null) p.y = BiomeManager.Active.GetGroundHeight(p); return p; }
54: source.BindPatternDirections(art.directionalPresentation); source.AlignPatternFeetToGround(); source.SetPatternFrame(idle[0]); source.SetPatternFacing(Vector3.right, false);
57: Phase = OilFilmPhase.Ready; Footprint = null; LockedDirection = Vector3.zero; ImpactCount = HitAttempts = DamageApplications = 0; hitUntil = 0;
69: Vector3 delta = (chase ? player.transform.position : home) - Position; delta.y = 0;
70: float stopAt = chase ? stop : .15f;
71: bool moving = delta.magnitude > stopAt && enemy.Stats.MoveSpeed > 0;
73: if (delta.sqrMagnitude > .0001f && (moving || chase)) enemy.SetPatternFacing(delta, false);
84: Vector3 delta = player.transform.position - Position; delta.y = 0;
85: if (delta.sqrMagnitude < .01f || delta.magnitude > trigger || !GasSacOrb.IsVisible(Position) || !GasSacOrb.HasClearPath(Position, player.transform.position)) return false;
91: public static Vector3 FacingDirection(EnemyFacing facing, Camera camera)
93: Vector3 right = camera != null ? Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized : Vector3.right;
94: Vector3 back = Vector3.Cross(right, Vector3.up).normalized;
95: if (camera != null && Vector3.Dot(back, camera.transform.up) < 0) back = -back;
100: enemy.SetPatternPositionLocked(true); HitAttempts = DamageApplications = 0;
105: float elapsed = 0;
109: enemy.SetPatternFrame(preparation[Mathf.Min(3, Mathf.FloorToInt(elapsed / windup * 4))]);
112: Phase = OilFilmPhase.Release; elapsed = 0;
116: float t = Mathf.Clamp01(elapsed / releaseTime);
117: float artT = t <= impactTime ? t / impactTime * .5f : .5f + (t - impactTime) / (1 - impactTime) * .5f;
118: enemy.SetPatternFrame(release[Mathf.Min(3, Mathf.FloorToInt(artT * 4))]);
121: enemy.SetPatternFrame(release[2]); // The full extension is always the impact pose, including a long frame.
126: Phase = OilFilmPhase.Recovery; elapsed = 0;
130: enemy.SetPatternFrame(recovery[Mathf.Min(3, Mathf.FloorToInt(elapsed / recoveryTime * 4))]);
140: if (lifetime.IsCurrent(generation)) { Phase = OilFilmPhase.Ready; nextReady = Time.time + rearm; enemy.SetPatternFrame(idle[0]); }
149: if (BiomeManager.Active != null && Mathf.Abs(BiomeManager.Active.GetGroundHeight(player.transform.position) - Footprint.Origin.y) > .25f) return;
151: player.TakeDamage(enemy.CreatePatternDamage(OilFilmPatternSettings.DamageId, 0));
160: float hp = player.HealthComponent.CurrentHealth; player.TakeDamage(enemy.CreateContactDamage(0));
163: Vector3 away = player.transform.position - Position; away.y = 0;
164: if (away.sqrMagnitude < .0001f) away = -LockedDirection;
170: TelegraphObject = new GameObject("S03_FilledAttackArea"); TelegraphObject.transform.position = Footprint.Origin + Vector3.up * .035f;
172: if (markerMaterial == null) markerMaterial = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave, mainTexture = Texture2D.whiteTexture };
173: var renderer = TelegraphObject.AddComponent<MeshRenderer>(); renderer.sharedMaterial = markerMaterial; renderer.sortingOrder = 65;
176: edge.useWorldSpace = false; edge.loop = true; edge.widthMultiplier = .018f; edge.sharedMaterial = markerMaterial; edge.sortingOrder = 66;
177: edge.startColor = edge.endColor = border; edge.positionCount = OilFilmSector.Segments + 2; edge.SetPosition(0, Vector3.zero);
178: for (int i = 0; i <= OilFilmSector.Segments; i++) edge.SetPosition(i + 1, Footprint.Point(i / (float)OilFilmSector.Segments, 1) - Footprint.Origin);
187: for(int i=0;i<=OilFilmSector.Segments;i++)warningEdge.SetPosition(i+1,Footprint.Point(i/(float)OilFilmSector.Segments,1)-Footprint.Origin);
189: return Footprint.MaximumReach > .05f;
193: private void OnDamaged(EnemyController source, float amount) { if (amount > 0 && Phase == OilFilmPhase.Ready) hitUntil = Time.time + hitTime * 2; }
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/OilFilmPatternSettings.cs

```csharp
8: [Min(.1f)] public float triggerDistance = 3.2f;
9: [Min(.1f)] public float approachStopDistance = 2f;
10: [Min(0)] public float spawnGraceSeconds = .75f;
11: [Min(.1f)] public float windupSeconds = .85f;
12: [Min(.1f)] public float attackRange = 2.5f;
13: [Range(30, 160)] public float arcDegrees = 110f;
14: [Min(.08f)] public float releaseSeconds = .28f;
15: [Range(.15f, .85f)] public float impactNormalizedTime = .5f;
16: [Min(.1f)] public float recoverySeconds = 1.1f;
17: [Min(0)] public float rearmSeconds = 2.5f;
19: [Min(.05f)] public float coreRadius = .28f;
27: if (!MonsterBalanceNumbers.Positive(arcDegrees) || arcDegrees < 30 || arcDegrees > 160
28: || !MonsterBalanceNumbers.Positive(impactNormalizedTime) || impactNormalizedTime < .15f || impactNormalizedTime > .85f)
29: return "기름막 각도는30~160도, 타격 시점은0.15~0.85입니다.";
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/OilFilmPresentation.cs

```csharp
22: [Min(.02f)] public float idleFrameSeconds = .35f, moveFrameSeconds = .22f, hitFrameSeconds = .1f, deathFrameSeconds = .14f;
23: public Color dangerColor = new Color(.8f, .025f, .075f, .55f);
24: public Color dangerBorderColor = new Color(1, .65f, .7f, .95f);
28: if (directionalPresentation == null || directionalPresentation.mode != EnemyDirectionMode.Four) return "기름막4방향 표현을 지정하세요.";
31: int[] counts = { 2, 2, 4, 4, 4, 2, 6 };
32: for (int i = 0; i < banks.Length; i++) if (banks[i] == null || banks[i].Length != counts[i] || Array.Exists(banks[i], s => s == null)) return "기름막 기본18장과 사망6장이 필요합니다.";
34: if (profiles == null || profiles.Length != 72) return "승인된72장 표현 기준점이 필요합니다.";
38: if (p == null || p.sprite == null || !selected.Add(p.sprite) || p.view < 0 || p.view > 2 || p.index < 0 || p.index >= 24) return "표현 프로필의 Sprite/방향/중복을 확인하세요.";
39: if (p.unfold && (p.rimRadii == null || p.rimRadii.Length != 65 || !MonsterBalanceNumbers.Positive(p.coreCut)
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/OilFilmSector.cs

```csharp
7: public const int Segments = 64;
8: private const float TerrainInset = .02f;
9: public readonly Vector3 Origin, Forward, Right;
13: private readonly float[] limits = new float[Segments + 1], scratch = new float[Segments + 1];
16: public OilFilmSector(Vector3 origin, Vector3 direction, float range, float arc)
18: Origin = origin; Forward = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
19: Right = Vector3.Cross(Vector3.up, Forward); Range = range; Arc = arc;
22: for (int i = 0; i < limits.Length; i++) limits[i] = range;
25: private Vector3 Direction(float u)
27: float a = (u - .5f) * Arc * Mathf.Deg2Rad;
32: float sample = Mathf.Clamp01(u) * Segments; int lo = Mathf.Min(Segments - 1, Mathf.FloorToInt(sample));
33: return Mathf.Lerp(limits[lo], limits[lo + 1], sample - lo);
36: public Vector3 UnclippedPoint(float angle01, float radius01) => Origin + Direction(angle01) * (Range * radius01);
37: public Vector3 Point(float angle01, float radius01) => Origin + Direction(angle01) * (RadiusAtAngle(angle01) * radius01);
38: public bool Contains(Vector3 point)
40: Vector3 p = point - Origin; p.y = 0;
41: if (p.sqrMagnitude < .000001f) return MaximumReach > .001f;
42: float angle = Mathf.Atan2(Vector3.Dot(p, Right), Vector3.Dot(p, Forward)) * Mathf.Rad2Deg;
43: if (Mathf.Abs(angle) > Arc * .5f + .00001f) return false;
44: float reach = RadiusAtAngle(angle / Arc + .5f);
45: return p.sqrMagnitude <= reach * reach + .000001f;
51: for (int i = 0; i <= Segments; i++)
53: float u = i / (float)Segments, halfStep = .5f / Segments;
54: scratch[i] = Mathf.Min(RayLimit(Direction(u)), Mathf.Min(RayLimit(Direction(Mathf.Max(0, u-halfStep))), RayLimit(Direction(Mathf.Min(1, u+halfStep)))));
56: bool changed = false; MaximumReach = 0;
57: for (int i = 0; i <= Segments; i++)
60: float safe = Mathf.Min(scratch[i], Mathf.Min(scratch[Mathf.Max(0,i-1)], scratch[Mathf.Min(Segments,i+1)]));
62: if (next < limits[i] - .00001f) { limits[i] = next; changed = true; }
64: TerrainClipped |= limits[i] < Range - .0001f;
73: private float RayLimit(Vector3 direction)
76: if (!ClearCell(x,z)) return 0;
78: int sx=direction.x>=0?1:-1,sz=direction.z>=0?1:-1;
79: float dx=Mathf.Abs(direction.x)>.000001f?tile/Mathf.Abs(direction.x):float.PositiveInfinity;
80: float dz=Mathf.Abs(direction.z)>.000001f?tile/Mathf.Abs(direction.z):float.PositiveInfinity;
81: float tx=float.IsPositiveInfinity(dx)?dx:Mathf.Max(0,(center.x+sx*tile*.5f-Origin.x)/direction.x);
82: float tz=float.IsPositiveInfinity(dz)?dz:Mathf.Max(0,(center.z+sz*tile*.5f-Origin.z)/direction.z);
85: float at=Mathf.Min(tx,tz);bool crossX=tx<=tz+.000001f,crossZ=tz<=tx+.000001f;
86: if ((crossX&&!ClearCell(x+sx,z)) || (crossZ&&!ClearCell(x,z+sz)) || (crossX&&crossZ&&!ClearCell(x+sx,z+sz))) return Mathf.Max(0,at-TerrainInset);
94: for(int i=0;i<=Segments;i++)vertices[i+1]=Point(i/(float)Segments,1)-Origin;
99: var v = new Vector3[Segments + 2]; var colors = new Color[v.Length]; var triangles = new int[Segments * 3];
100: for (int i = 0; i <= Segments; i++) v[i + 1] = Point(i / (float)Segments, 1) - Origin;
101: for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
102: for (int i = 0; i < Segments; i++) { triangles[i * 3] = 0; triangles[i * 3 + 1] = i + 1; triangles[i * 3 + 2] = i + 2; }
103: var mesh = new Mesh { name = "OilFilmFilledSector", vertices = v, uv = new Vector2[v.Length], colors = colors, triangles = triangles };
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/OilFilmVisual.cs

```csharp
7: [DefaultExecutionOrder(250)]
10: private const float AuthoredForeshortening = .70710678f;
11: private const int Columns = 40, Rows = 32;
20: private readonly float[] clipLimits = new float[OilFilmSector.Segments + 1];
21: private Vector3[] sourceVertices, vertices;
27: public Vector3 CoreGroundPosition { get; private set; }
57: var camera = DontStarveCamera.GetActiveCamera(); if (camera == null || camera.transform.forward.y >= -.1f) return;
58: CoreGroundPosition = pattern.GroundOrigin + Vector3.up * .055f;
60: Vector3 position = CoreGroundPosition;
69: for (int i = 0; i < vertices.Length; i++) vertices[i] = transform.InverseTransformPoint(MapSourcePoint(sourceVertices[i], camera));
74: properties.SetVector("_ClipOrigin", new Vector4(footprint.Origin.x, footprint.Origin.z, 0, 0));
75: properties.SetVector("_ClipAxes", new Vector4(footprint.Forward.x, footprint.Forward.z, footprint.Right.x, footprint.Right.z));
76: properties.SetFloat("_ClipArc", footprint.Arc * Mathf.Deg2Rad); properties.SetFloatArray("_Reach", clipLimits);
79: filmRenderer.sortingOrder = 60; coreRenderer.enabled = filmRenderer.enabled = true; body.enabled = false;
81: private Vector2 Native(Vector3 local)
83: if (current.view == 0) return new Vector2(local.x, -local.y / AuthoredForeshortening);
84: if (current.view == 1) return new Vector2(-local.y / AuthoredForeshortening, -local.x);
85: return new Vector2(local.y / AuthoredForeshortening, local.x);
87: private Vector3 FromNative(float forward, float lateral)
89: if (current.view == 0) return new Vector3(forward, -lateral * AuthoredForeshortening, 0);
90: if (current.view == 1) return new Vector3(-lateral, -forward * AuthoredForeshortening, 0);
91: return new Vector3(lateral, forward * AuthoredForeshortening, 0);
93: public Vector3 MapSourcePoint(Vector3 local, Camera camera)
95: Vector3 original = CoreGroundPosition + camera.transform.right * (local.x * (body.flipX ? -1 : 1)) + camera.transform.up * local.y;
97: Vector2 p = Native(local);
99: float angle = Mathf.Atan2(p.y, p.x);
101: float sample = u * (current.rimRadii.Length - 1); int lo = Mathf.Min(current.rimRadii.Length - 2, Mathf.FloorToInt(sample));
102: float radius = Mathf.Lerp(current.rimRadii[lo], current.rimRadii[lo + 1], sample - lo);
103: float t = Mathf.Clamp01(p.magnitude / radius) * Mathf.Clamp01(current.reachFraction);
104: if (body.flipX && current.view == 0) u = 1 - u;
105: Vector3 target = pattern.Footprint.UnclippedPoint(u, t);
108: target.y = pattern.Footprint.Origin.y + .055f;
109: float blend = Mathf.SmoothStep(0, 1, (p.x - current.coreCut) / (current.coreCut * .5f));
110: return Vector3.Lerp(original, target, blend);
112: public Vector3 VisibleRimSample(float u)
114: float sample = Mathf.Clamp01(u) * (current.rimRadii.Length - 1); int lo = Mathf.Min(current.rimRadii.Length - 2, Mathf.FloorToInt(sample));
115: float r = Mathf.Lerp(current.rimRadii[lo], current.rimRadii[lo + 1], sample - lo);
117: Vector3 point = MapSourcePoint(FromNative(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r), DontStarveCamera.GetActiveCamera());
118: float actualU = body.flipX && current.view == 0 ? 1 - u : u;
121: Vector3 edge = pattern.Footprint.UnclippedPoint(actualU, visible / pattern.Footprint.Range);
122: edge.y = pattern.Footprint.Origin.y + .055f;
128: int n = (Columns + 1) * (Rows + 1); sourceVertices = new Vector3[n]; vertices = new Vector3[n];
129: var uv = new Vector2[n]; var colors = new Color[n]; var core = new List<int>(); var film = new List<int>();
130: Rect r = sprite.rect; Vector2 pivot = sprite.pivot; float ppu = sprite.pixelsPerUnit;
131: for (int y = 0; y <= Rows; y++) for (int x = 0; x <= Columns; x++)
133: int i = y * (Columns + 1) + x; float px = x / (float)Columns * r.width, py = y / (float)Rows * r.height;
134: sourceVertices[i] = new Vector3((px - pivot.x) / ppu, (py - pivot.y) / ppu, 0);
135: uv[i] = new Vector2((r.x + px) / sprite.texture.width, (r.y + py) / sprite.texture.height); colors[i] = Color.white;
139: var target = Native((sourceVertices[a] + sourceVertices[b] + sourceVertices[c]) / 3).x <= current.coreCut ? core : film;
142: for (int y = 0; y < Rows; y++) for (int x = 0; x < Columns; x++)
144: int i = y * (Columns + 1) + x; Triangle(i, i + 1, i + Columns + 2); Triangle(i, i + Columns + 2, i + Columns + 1);
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/PollenInvaderElitePattern.cs

```csharp
14: private Vector3 anchor;
23: public Vector3 LockedDirection { get; private set; }
24: public Vector3 LaunchPosition { get; private set; }
25: public Vector3 OpeningPosition { get; private set; }
26: public PollenPellet[] ActivePellets { get; private set; } = new PollenPellet[0];
30: public PollenPellet[] FirstWavePellets { get; private set; } = new PollenPellet[0];
31: public PollenPellet[] SecondWavePellets { get; private set; } = new PollenPellet[0];
33: public Vector3 SecondLockedDirection { get; private set; }
55: rotationSeconds = followupEnabled ? settings.followup.rotationSeconds : 0;
56: angleOffset = followupEnabled ? settings.followup.angleOffset : 0;
62: rotation = followupEnabled ? (Sprite[])art.rotationFrames.Clone() : new Sprite[0];
65: source.BindPatternDirections(art.directionalPresentation); source.AlignPatternFeetToGround(); source.SetPatternFrame(idle[0]);
67: nextReady = Time.time + settings.spawnGraceSeconds; hitUntil = 0; VolleyCount = 0; LastVolley = null; ActivePellets = new PollenPellet[0];
68: FirstWavePellets = SecondWavePellets = new PollenPellet[0]; FirstVolley = SecondVolley = null; CycleHitBudget = null; CycleVolleyCount = 0;
69: SecondLockedDirection = Vector3.zero; RotationStartedAt = SecondReleasedAt = SignedRotationOffset = 0;
80: Vector3 delta = (chase ? player.transform.position : anchor) - transform.position; delta.y = 0;
81: float distance = chase ? stop : .15f;
82: bool moving = delta.magnitude > distance && enemy.Stats.MoveSpeed > 0;
91: || IsStunned() || Time.time < nextReady || lifetime.OwnedObjectCount > 0) return false;
94: Vector3 target = player.transform.position, delta = target - transform.position; delta.y = 0;
95: if (delta.sqrMagnitude < .0001f || delta.sqrMagnitude > trigger * trigger
99: delta = target - start; delta.y = 0; if (delta.sqrMagnitude < .0001f) return false;
106: enemy.CommitPatternFacing(Vector3.zero, false); enemy.SetPatternPositionLocked(true);
107: var damage = enemy.CreatePatternDamage(PollenInvaderPatternSettings.DamageId, 0);
108: var secondDamage = followupEnabled ? enemy.CreatePatternDamage(PollenInvaderPatternSettings.FollowupDamageId, 0) : default;
109: CycleHitBudget = new PollenCycleHitBudget(); CycleVolleyCount = 0;
110: ActivePellets = FirstWavePellets = SecondWavePellets = new PollenPellet[0];
112: SecondLockedDirection = Vector3.zero; RotationStartedAt = SecondReleasedAt = 0;
116: float elapsed = 0;
120: enemy.SetPatternFrame(preparation[Mathf.Min(preparation.Length - 1, Mathf.FloorToInt(elapsed / windup * preparation.Length))]);
130: SecondLockedDirection = Quaternion.AngleAxis(SignedRotationOffset, Vector3.up) * LockedDirection;
131: elapsed = 0; bool interrupted = false;
132: float releaseHold = Mathf.Min(releaseSeconds, rotationSeconds * .25f);
136: float progress = Mathf.Clamp01((elapsed - releaseHold) / (rotationSeconds - releaseHold));
137: enemy.SetPatternFrame(elapsed < releaseHold ? release : rotation[Mathf.Min(rotation.Length - 1, Mathf.FloorToInt(progress * rotation.Length))]);
142: Phase = PollenInvaderPhase.Recovery; elapsed = 0;
145: Sprite pose = secondFired ? rotation[rotation.Length - 1] : release;
155: if (lifetime.IsCurrent(generation)) { enemy.SetPatternFrame(idle[0]); nextReady = Time.time + rearm; Phase = PollenInvaderPhase.Ready; }
159: private bool EmitWave(Vector3 centerDirection, EnemyDamageRequest damage, bool second)
163: var wave = new PollenPellet[3];
164: for (int i = 0; i < wave.Length; i++)
166: var direction = Quaternion.AngleAxis((i - 1) * spread, Vector3.up) * centerDirection;
170: System.Array.Copy(ActivePellets, combined, ActivePellets.Length); System.Array.Copy(wave, 0, combined, ActivePellets.Length, wave.Length); ActivePellets = combined;
176: private void OnDamaged(EnemyController source, float damage) { if (damage > 0 && Phase == PollenInvaderPhase.Ready) hitUntil = Time.time + .12f; }
181: var renderer = shadow.GetComponent<SpriteRenderer>(); renderer.sprite = art.groundDisc; renderer.color = art.groundShadowColor; renderer.sortingOrder = 56;
182: shadow.transform.rotation = Quaternion.Euler(90, 0, 0); shadow.transform.localScale = new Vector3(art.groundShadowSize.x * .5f, art.groundShadowSize.y * .5f, 1);
189: shadow.transform.position = p + Vector3.up * .035f;
195: ActivePellets = FirstWavePellets = SecondWavePellets = new PollenPellet[0]; Phase = PollenInvaderPhase.Inactive; enabled = false;
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/PollenInvaderPatternSettings.cs

```csharp
10: [Range(1, 59)] public float angleOffset = 15f;
12: [Min(.2f)] public float rotationSeconds = .8f;
13: [Min(.1f)] public float recoverySeconds = 1.2f;
14: [Min(0)] public float rearmSeconds = 3.2f;
22: [Min(.1f)] public float triggerDistance = 6f;
23: [Min(.1f)] public float approachStopDistance = 3.2f;
24: [Min(0)] public float spawnGraceSeconds = .75f;
25: [Min(.1f)] public float windupSeconds = .85f;
26: [Range(10, 60)] public float spreadHalfAngle = 30f;
27: [Min(.1f)] public float pelletSpeed = 3.5f;
28: [Min(.1f)] public float pelletLifetimeSeconds = 2.4f;
30: [Min(.02f)] public float pelletRadius = .16f;
31: [Min(.1f)] public float recoverySeconds = 1f;
32: [Min(0)] public float rearmSeconds = 2.5f;
42: || spreadHalfAngle < 10 || spreadHalfAngle > 60) return "꽃가루 접근 거리·대기·발사 각도(10~60도)를 확인하세요.";
49: || !MonsterBalanceNumbers.Positive(followup.rotationSeconds) || followup.rotationSeconds < .2f
51: return "B 회전각은 0보다 크고 기본 탄 간격보다 작아야 하며, 예고는 0.2초 이상이어야 합니다.";
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/PollenInvaderPresentation.cs

```csharp
11: [Min(.02f)] public float idleFrameSeconds = .35f;
12: [Min(.02f)] public float moveFrameSeconds = .22f;
13: [Min(.02f)] public float releaseFrameSeconds = .12f;
14: [Min(.02f)] public float deathFrameSeconds = .14f;
15: [Min(.02f)] public float pelletFrameSeconds = .12f;
16: public Vector2 groundShadowSize = new Vector2(1.1f, .6f);
17: public Color groundShadowColor = new Color(.16f, .1f, .04f, .25f);
22: if (!Valid(idleFrames, 2) || !Valid(moveFrames, 2) || !Valid(preparationFrames, 3) || !Valid(deathFrames, 6)
23: || !Valid(pelletFrames, 2) || release == null || recovery == null || hit == null || groundDisc == null)
24: return "꽃가루 본체 10장·사망 6장·공용 탄 2장과 그림자를 지정하세요.";
27: if (includeFollowup && !Valid(rotationFrames, 3)) return "B 회전 예고 3장의 방향 원본을 지정하세요.";
31: foreach (var bank in new[] { idleFrames, moveFrames, preparationFrames, deathFrames, new[] { release, recovery, hit }, includeFollowup ? rotationFrames : new Sprite[0] })
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/PollenPellet.cs

```csharp
12: if (HitAttempts != 0 || player == null || player.IsDead) return;
37: [DefaultExecutionOrder(100)]
50: public Vector3 Direction { get; private set; }
51: public Vector3 LaunchPosition { get; private set; }
58: public static bool TryGetLaunchPosition(EnemyController owner, float radius, out Vector3 position)
62: if (camera == null || Mathf.Abs(camera.transform.forward.y) < .1f) return false;
73: Vector3 origin, Vector3 direction, float speed, float duration, float radius, Sprite[] frames, float frameSeconds)
75: var go = new GameObject("P03_PollenPellet"); go.transform.position = origin;
78: pellet.LaunchPosition = origin; direction.y = 0; pellet.Direction = direction.normalized;
93: if (lifetime == null || !lifetime.IsCurrent(generation) || remaining <= 0) { Finish(); return; }
95: Vector3 from = transform.position, end = from + Direction * (Speed * dt);
96: int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(from, end) / .1f));
97: for (int i = 1; i <= steps; i++)
99: var next = Vector3.Lerp(from, end, i / (float)steps);
109: UpdateVisual(); if (remaining <= 0) Finish();
118: Vector3 p = ownerBody.transform.InverseTransformPoint(body.transform.position);
124: if (overlaps) body.sortingOrder = Mathf.Max(body.sortingOrder, ownerBody.sortingOrder + 1);
135: float diameter = Mathf.Max(frames[0].bounds.size.x, frames[0].bounds.size.y);
136: body.transform.localScale = Vector3.one * (Radius * 2 / diameter);
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/ResiduePlacementSafety.cs

```csharp
7: public static bool CanPlace(Vector3 center, Vector3 forward, Vector2 size)
12: Vector2 feet = motor != null ? motor.TerrainHalfExtents : new Vector2(.68f, .48f);
13: Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
14: float clearance = feet.magnitude + .15f;
15: Vector2 outer = size * .5f + Vector2.one * clearance;
17: if (outer.x > 8 || outer.y > 8) return false;
18: float exclusion = outer.magnitude + 1f;
19: Vector3 entrance = biome.GetPlayerSpawnPosition(); entrance.y = center.y;
25: Vector3 portalPosition = portal != null && portal.useCustomPosition
33: Vector2Int arenaCell = arena.useCustomCenter ? arena.centerGrid : new Vector2Int(biome.MapWidth / 2, biome.MapHeight / 2);
34: Vector2Int cell = biome.WorldToGrid(center);
35: if (Mathf.Abs(cell.x - arenaCell.x) <= arena.arenaSize.x * .5f + exclusion + arena.wallThicknessInCells
36: && Mathf.Abs(cell.y - arenaCell.y) <= arena.arenaSize.y * .5f + exclusion + arena.wallThicknessInCells) return false;
39: Vector2Int middle = biome.WorldToGrid(center);
42: bool Clear(Vector3 point)
45: for (int x = -1; x <= 1; x++) for (int z = -1; z <= 1; z++)
47: Vector3 sample = point + new Vector3(x * feet.x, 0, z * feet.y);
48: Vector2Int cell = biome.WorldToGrid(sample);
56: Vector3[] corners = { center - right * outer.x - forward * outer.y, center + right * outer.x - forward * outer.y,
58: for (int side = 0; side < 4; side++)
60: Vector3 a = corners[side], b = corners[(side + 1) % 4];
61: int steps = Mathf.CeilToInt(Vector3.Distance(a, b) / .25f);
62: Vector3 previous = a;
63: for (int i = 0; i <= steps; i++)
65: Vector3 p = Vector3.Lerp(a, b, i / (float)steps);
71: for (float x = -size.x * .5f; x <= size.x * .5f + .01f; x += .25f)
72: for (float z = -size.y * .5f; z <= size.y * .5f + .01f; z += .25f)
```

## Assets/_Project/Scripts/Gameplay/Enemies/BiomeElites/ResidueRubble.cs

```csharp
14: private Vector3 forward, right, landingVisualOffset;
15: private Vector2 halfSize;
24: public Vector2 Footprint => halfSize * 2;
26: public Vector3 Forward => forward;
27: public Vector3 LandingVisualPosition => transform.position + landingVisualOffset;
30: get { int count = 0; foreach (var prop in Active) if (prop != null && prop.IsBlocking) count++; return count; }
34: public static ResidueRubble Create(EnemyController owner, EnemyPatternLifetime life, Vector3 center,
35: Vector3 forward, Vector2 size, Sprite sprite, int hits, Sprite airborneChip = null,
36: Vector3? launchPosition = null, Vector3? launchScale = null, float groundDepthFraction = 0)
38: var go = new GameObject("I02_ResidueRubble") { layer = owner.gameObject.layer };
42: prop.forward = forward.normalized; prop.right = Vector3.Cross(Vector3.up, prop.forward);
43: prop.halfSize = size * .5f; prop.remainingHits = hits;
48: Vector3 screenRight = camera != null ? Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized : Vector3.right;
49: if (screenRight.sqrMagnitude < .0001f) screenRight = Vector3.right;
50: Vector3 screenUp = Vector3.Cross(screenRight, Vector3.up);
51: float width = Mathf.Abs(Vector3.Dot(prop.right, screenRight)) * size.x + Mathf.Abs(Vector3.Dot(prop.forward, screenRight)) * size.y;
52: float depth = Mathf.Abs(Vector3.Dot(prop.right, screenUp)) * size.x + Mathf.Abs(Vector3.Dot(prop.forward, screenUp)) * size.y;
53: prop.landingVisualOffset = -screenUp * depth * .5f;
56: float groundProjection = camera != null ? Mathf.Abs(Vector3.Dot(screenUp, camera.transform.up)) : .7071068f;
57: float scale = groundDepthFraction > 0
58: ? depth * Mathf.Max(.01f, groundProjection) / Mathf.Max(.01f, sprite.bounds.size.y * groundDepthFraction)
59: : width / Mathf.Max(.01f, sprite.bounds.size.x);
61: spriteObject.transform.localScale = airborneChip != null ? launchScale ?? Vector3.one : Vector3.one * scale;
66: prop.hitCollider.center = Vector3.up * .6f; prop.hitCollider.size = new Vector3(size.x, 2f, size.y);
71: else prop.SetDropHeight(1.2f);
77: SetAirPosition(LandingVisualPosition + Vector3.up * height);
80: public void SetAirPosition(Vector3 position)
91: visual.transform.localScale = Vector3.one * landedScale;
92: SetDropHeight(0); landed = true; expiresAt = Time.time + seconds;
106: if (remainingHits <= 0) Release();
128: private Vector2 Local(Vector3 point)
130: Vector3 delta = point - transform.position; delta.y = 0;
131: return new Vector2(Vector3.Dot(delta, right), Vector3.Dot(delta, forward));
134: public bool ContainsPoint(Vector3 point)
136: Vector2 local = Local(point);
140: public static bool CanOccupy(Vector3 point, Vector2 moverHalfExtents)
145: Vector2 p = prop.Local(point);
153: public static bool CanTraverse(Vector3 from, Vector3 to, Vector2 moverHalfExtents)
161: Vector2Int cell = biome.WorldToGrid(prop.transform.position), player = biome.WorldToGrid(from);
164: Vector2 expansion = new Vector2(Mathf.Abs(prop.right.x) * moverHalfExtents.x + Mathf.Abs(prop.right.z) * moverHalfExtents.y,
166: Vector2 size = prop.halfSize + expansion, a = prop.Local(from), b = prop.Local(to);
168: if (penetration >= 0)
171: Vector2 normalizedFrom = new Vector2(a.x / size.x, a.y / size.y), normalizedTo = new Vector2(b.x / size.x, b.y / size.y);
172: if (normalizedTo.sqrMagnitude > normalizedFrom.sqrMagnitude + .000001f) continue;
173: if ((b - a).sqrMagnitude < .000001f) continue;
181: public static void HitAlongSegment(Vector3 from, Vector3 to, float radius, float damage, LayerMask mask, int attackToken)
183: if (damage <= 0) return;
184: for (int i = Active.Count - 1; i >= 0; i--)
187: if (prop == null || !prop.IsBlocking || (mask.value & (1 << prop.gameObject.layer)) == 0) continue;
188: if (CrossesRectangle(prop.Local(from), prop.Local(to), prop.halfSize + Vector2.one * Mathf.Max(0, radius)))
193: public static void HitArea(Vector3 center, float radius, Vector3 facing, float halfAngle, float damage, LayerMask mask, int attackToken)
195: for (int i = Active.Count - 1; i >= 0; i--)
198: if (prop == null || !prop.IsBlocking || (mask.value & (1 << prop.gameObject.layer)) == 0) continue;
199: Vector2 local = prop.Local(center);
200: Vector2 closest = new Vector2(Mathf.Clamp(local.x, -prop.halfSize.x, prop.halfSize.x), Mathf.Clamp(local.y, -prop.halfSize.y, prop.halfSize.y));
202: Vector3 delta = prop.transform.position - center; delta.y = 0;
203: if (halfAngle < 180 && delta.sqrMagnitude > .0001f && Vector3.Angle(facing, delta) > halfAngle) continue;
208: private static bool CrossesRectangle(Vector2 from, Vector2 to, Vector2 half)
210: float near = 0, far = 1; Vector2 delta = to - from;
216: if (Mathf.Abs(delta) < .000001f) return Mathf.Abs(start) <= half;
```

## Assets/_Project/Scripts/Gameplay/Enemies/Bosses/BossPatternVisualPool.cs

```csharp
21: Vector3 position,
35: obj.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);
```

## Assets/_Project/Scripts/Gameplay/Enemies/Bosses/IntestineBossPattern.cs

```csharp
11: [Range(0.1f, 0.9f)] public float phase2HealthRatio = 0.5f;
14: public float phase1FleeDistance = 7f;
15: public float phase1RoamRadius = 8f;
16: [HideInInspector] public float phase1MoveMultiplier = 1f;
17: public float phase2PreferredDistance = 2.5f;
18: [HideInInspector] public float phase2MoveMultiplier = 1.15f;
21: public float dungPreDelay = 0.5f;
22: public float phase1DungCooldown = 8f;
23: public float phase2ThrowCooldown = 5f;
24: [HideInInspector] public float dungCollisionDamage = 1f;
25: [Range(0f, 1f)] public float dungSlowRatio = 0.2f;
26: public float dungSlowDuration = 3f;
27: public float dungHazardRadius = 0.75f;
28: public float dungLifeTime = 10f;
29: public float throwTravelTime = 0.7f;
30: public float throwArcHeight = 2.5f;
33: public int parasiteMinCount = 2;
34: public int parasiteMaxCount = 3;
35: public float parasiteSpawnDelay = 0.45f;
36: public float parasiteSpawnRadius = 1.1f;
38: [HideInInspector] public float parasiteMaxHealth = 6f;
39: [HideInInspector] public float parasiteMoveSpeed = 2.3f;
40: [HideInInspector] public float parasiteAttackDamage = 1f;
41: public float parasiteAttackRange = 1.1f;
42: public float parasiteAttackCooldown = 1.3f;
43: public Vector3 parasiteScale = new Vector3(0.55f, 0.55f, 0.55f);
46: public float stompMinDelay = 7f;
47: public float stompPreDelay = 0.55f;
48: public float stompRadius = 6f;
49: [HideInInspector] public float stompDamage = 2f;
50: [Range(0f, 1f)] public float stompSlowRatio = 0.3f;
51: public float stompSlowDuration = 3f;
54: public Color dungColor = new Color(0.34f, 0.18f, 0.08f, 0.95f);
55: public Color parasiteColor = new Color(0.62f, 0.88f, 0.36f, 1f);
56: public Color shockwaveColor = new Color(0.65f, 0.38f, 0.18f, 0.45f);
59: public bool startInPhase2ForDebug = false;
61: public float fastPatternCooldown = 1f;
70: Phase1,
72: Phase2
76: [SerializeField, Range(0.1f, 0.9f)] private float phase2HealthRatio = 0.5f;
79: [SerializeField] private float phase1FleeDistance = 7f;
80: [SerializeField] private float phase1RoamRadius = 8f;
81: [SerializeField, HideInInspector] private float phase1MoveMultiplier = 1f;
82: [SerializeField] private float phase2PreferredDistance = 2.5f;
83: [SerializeField, HideInInspector] private float phase2MoveMultiplier = 1.15f;
86: [SerializeField] private float dungPreDelay = 0.5f;
87: [SerializeField] private float phase1DungCooldown = 8f;
88: [SerializeField] private float phase2ThrowCooldown = 5f;
89: [SerializeField, HideInInspector] private float dungCollisionDamage = 1f;
90: [SerializeField, Range(0f, 1f)] private float dungSlowRatio = 0.2f;
91: [SerializeField] private float dungSlowDuration = 3f;
92: [SerializeField] private float dungHazardRadius = 0.75f;
93: [SerializeField] private float dungLifeTime = 10f;
94: [SerializeField] private float throwTravelTime = 0.7f;
95: [SerializeField] private float throwArcHeight = 2.5f;
98: [SerializeField] private int parasiteMinCount = 2;
99: [SerializeField] private int parasiteMaxCount = 3;
100: [SerializeField] private float parasiteSpawnDelay = 0.45f;
101: [SerializeField] private float parasiteSpawnRadius = 1.1f;
104: [SerializeField] private float stompMinDelay = 7f;
105: [SerializeField] private float stompPreDelay = 0.55f;
106: [SerializeField] private float stompRadius = 6f;
107: [SerializeField, HideInInspector] private float stompDamage = 2f;
108: [SerializeField, Range(0f, 1f)] private float stompSlowRatio = 0.3f;
109: [SerializeField] private float stompSlowDuration = 3f;
112: [SerializeField] private Color dungColor = new Color(0.34f, 0.18f, 0.08f, 0.95f);
113: [SerializeField] private Color parasiteColor = new Color(0.62f, 0.88f, 0.36f, 1f);
114: [SerializeField] private Color shockwaveColor = new Color(0.65f, 0.38f, 0.18f, 0.45f);
117: [SerializeField, HideInInspector] private float parasiteMaxHealth = 6f;
118: [SerializeField, HideInInspector] private float parasiteMoveSpeed = 2.3f;
119: [SerializeField, HideInInspector] private float parasiteAttackDamage = 1f;
120: [SerializeField] private float parasiteAttackRange = 1.1f;
121: [SerializeField] private float parasiteAttackCooldown = 1.3f;
122: [SerializeField] private Vector3 parasiteScale = new Vector3(0.55f, 0.55f, 0.55f);
125: [SerializeField] private bool startInPhase2ForDebug = false;
127: [SerializeField] private float fastPatternCooldown = 1f;
139: private Vector3 anchorPosition;
140: private Vector3 baseScale = Vector3.one;
149: public void Initialize(EnemyController controller, Vector3 anchor, Transform parent, IntestineBossPatternSettings settings = null)
157: phase = startInPhase2ForDebug ? BossPhase.Phase2 : BossPhase.Phase1;
158: boss?.ApplyBalancePhase(phase == BossPhase.Phase2 ? "Phase2" : "Default");
159: nextDungTime = Time.time + 1f;
160: nextStompTime = phase == BossPhase.Phase2
188: phase2HealthRatio = settings.phase2HealthRatio;
189: phase1FleeDistance = settings.phase1FleeDistance;
190: phase1RoamRadius = settings.phase1RoamRadius;
191: phase1MoveMultiplier = settings.phase1MoveMultiplier;
192: phase2PreferredDistance = settings.phase2PreferredDistance;
193: phase2MoveMultiplier = settings.phase2MoveMultiplier;
195: phase1DungCooldown = settings.phase1DungCooldown;
196: phase2ThrowCooldown = settings.phase2ThrowCooldown;
223: startInPhase2ForDebug = settings.startInPhase2ForDebug;
270: nextDungTime = Time.time + 1f;
271: nextStompTime = phase == BossPhase.Phase2
277: [ContextMenu("Debug/Force Phase 1")]
278: public void ForcePhase1ForDebug()
282: phase = BossPhase.Phase1;
290: [ContextMenu("Debug/Force Phase 2")]
291: public void ForcePhase2ForDebug()
295: phase = BossPhase.Phase2;
296: boss?.ApplyBalancePhase("Phase2");
303: [ContextMenu("Debug/Run Phase 1 Dung")]
304: public void RunPhase1DungForDebug()
306: ForcePhase1ForDebug();
307: StartCoroutine(Phase1DungRoutine());
310: [ContextMenu("Debug/Run Phase 2 Throw")]
311: public void RunPhase2ThrowForDebug()
313: ForcePhase2ForDebug();
317: [ContextMenu("Debug/Run Phase 2 Stomp")]
318: public void RunPhase2StompForDebug()
320: ForcePhase2ForDebug();
342: if (phase == BossPhase.Phase1 && stats != null && stats.HealthNormalized <= phase2HealthRatio)
344: StartCoroutine(EnterPhase2Routine());
348: if (phase == BossPhase.Phase1 && Time.time >= nextDungTime && !actionRunning)
350: StartCoroutine(Phase1DungRoutine());
352: else if (phase == BossPhase.Phase2 && !actionRunning)
372: Vector3 moveDirection = phase == BossPhase.Phase1
373: ? GetPhase1MoveDirection()
374: : GetPhase2MoveDirection();
376: if (moveDirection.sqrMagnitude <= 0.0001f)
381: float multiplier = boss.Balance != null ? 1f : (phase == BossPhase.Phase1 ? phase1MoveMultiplier : phase2MoveMultiplier);
382: float speed = stats != null ? stats.MoveSpeed : 1.5f;
383: Vector3 step = moveDirection.normalized * Mathf.Max(0f, speed * multiplier) * Time.deltaTime;
387: private float GetPhase1DungCooldown()
390: ? Mathf.Max(0.05f, fastPatternCooldown)
391: : Mathf.Max(0f, phase1DungCooldown));
394: private float GetPhase2ThrowCooldown()
397: ? Mathf.Max(0.05f, fastPatternCooldown)
398: : Mathf.Max(0f, phase2ThrowCooldown));
404: ? Mathf.Max(0.05f, fastPatternCooldown)
405: : Mathf.Max(0f, stompMinDelay));
408: private Vector3 GetPhase1MoveDirection()
410: Vector3 position = transform.position;
411: Vector3 playerPosition = PlayerController.Instance.transform.position;
412: Vector3 fromPlayer = position - playerPosition;
413: fromPlayer.y = 0f;
415: Vector3 fromAnchor = position - anchorPosition;
416: fromAnchor.y = 0f;
417: if (fromAnchor.magnitude > phase1RoamRadius)
419: Vector3 toAnchor = anchorPosition - position;
420: toAnchor.y = 0f;
424: if (fromPlayer.magnitude <= phase1FleeDistance)
426: return fromPlayer.sqrMagnitude > 0.0001f ? fromPlayer : RandomPlanarDirection();
429: Vector3 orbit = Vector3.Cross(Vector3.up, fromPlayer);
430: return orbit.sqrMagnitude > 0.0001f ? orbit : RandomPlanarDirection();
433: private Vector3 GetPhase2MoveDirection()
435: Vector3 toPlayer = PlayerController.Instance.transform.position - transform.position;
436: toPlayer.y = 0f;
437: if (toPlayer.magnitude <= phase2PreferredDistance)
439: return Vector3.zero;
445: private IEnumerator EnterPhase2Routine()
451: float elapsed = 0f;
452: float duration = 1.1f;
456: float pulse = 1f + Mathf.Sin(elapsed * 22f) * 0.12f;
462: phase = BossPhase.Phase2;
463: boss?.ApplyBalancePhase("Phase2");
464: nextDungTime = Time.time + 1.5f;
470: private IEnumerator Phase1DungRoutine()
473: nextDungTime = Time.time + GetPhase1DungCooldown();
474: yield return new WaitForSeconds(Mathf.Max(0f, dungPreDelay));
476: AudioManager.Instance?.PlaySFX("IntestineSkill1");
477: Vector3 dropPosition = transform.position - GetDirectionToPlayer() * 0.7f;
486: nextDungTime = Time.time + GetPhase2ThrowCooldown();
487: yield return new WaitForSeconds(Mathf.Max(0f, dungPreDelay));
489: Vector3 start = transform.position + Vector3.up * 1.2f;
490: Vector3 target = PlayerController.Instance != null
492: : transform.position + GetDirectionToPlayer() * 4f;
493: target.y = GetGroundHeight(target) + 0.05f;
495: AudioManager.Instance?.PlaySFX("IntestineSkill2");
496: GameObject projectile = CreateTempSpriteObject("IntestineBoss_DungProjectile", GetDungSprite(), dungColor, start, 0.7f, 5000);
497: float elapsed = 0f;
498: float duration = Mathf.Max(0.1f, throwTravelTime);
502: float t = Mathf.Clamp01(elapsed / duration);
503: Vector3 pos = Vector3.Lerp(start, target, t);
528: Vector3 originalScale = baseScale;
529: float elapsed = 0f;
530: float delay = Mathf.Max(0f, stompPreDelay);
534: float lift = Mathf.Sin(Mathf.Clamp01(elapsed / Mathf.Max(0.01f, delay)) * Mathf.PI);
535: transform.localScale = new Vector3(originalScale.x * (1f + lift * 0.08f), originalScale.y * (1f + lift * 0.22f), originalScale.z * (1f + lift * 0.08f));
546: Vector3 toPlayer = player.transform.position - transform.position;
547: toPlayer.y = 0f;
558: private void SpawnDungHazard(Vector3 position, bool spawnParasites)
560: position.y = GetGroundHeight(position) + 0.05f;
561: GameObject obj = CreateTempSpriteObject("IntestineBoss_Dung", GetDungSprite(), dungColor, position, dungHazardRadius * 2f, 2500);
566: public void SpawnParasites(Vector3 center)
568: int min = Mathf.Max(0, parasiteMinCount);
570: int count = Random.Range(min, max + 1);
574: for (int i = 0; i < count; i++)
576: Vector2 offset2D = Random.insideUnitCircle.normalized * Random.Range(0.35f, parasiteSpawnRadius);
577: Vector3 spawnPosition = center + new Vector3(offset2D.x, 0f, offset2D.y);
589: private void SpawnShockwave(Vector3 center, float radius)
591: center.y = GetGroundHeight(center) + 0.08f;
592: GameObject obj = CreateTempSpriteObject("IntestineBoss_StompShockwave", GetShockwaveSprite(), shockwaveColor, center, radius * 2f, 2400);
593: StartCoroutine(FadeAndDestroy(obj, 0.45f));
604: Vector3 startScale = obj.transform.localScale * 0.35f;
605: Vector3 endScale = obj.transform.localScale;
606: float elapsed = 0f;
610: float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
611: obj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
615: color.a = Mathf.Lerp(shockwaveColor.a, 0f, t);
650: renderer.sortingOrder = 1800;
654: private Vector3 GetDirectionToPlayer()
658: return transform.forward.sqrMagnitude > 0.0001f ? transform.forward : Vector3.forward;
661: Vector3 direction = PlayerController.Instance.transform.position - transform.position;
662: direction.y = 0f;
663: return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
666: private float GetGroundHeight(Vector3 position)
672: private static Vector3 RandomPlanarDirection()
674: Vector2 dir = Random.insideUnitCircle.normalized;
675: return dir.sqrMagnitude > 0.0001f ? new Vector3(dir.x, 0f, dir.y) : Vector3.forward;
694: private GameObject CreateTempSpriteObject(string name, Sprite sprite, Color color, Vector3 position, float scale, int sortingOrder)
712: for (int i = 0; i < activeTempObjects.Count; i++)
722: for (int i = 0; i < activeSummons.Count; i++)
745: poissonSalt = 9917,
746: maxAlive = 1,
747: activationRadius = 0f,
748: respawnCooldown = 0f,
749: spawnRadius = 0f,
751: stoppingDistance = 0.15f,
752: wanderRadius = 2f,
753: chaseRadius = 18f,
754: leashRadius = 24f,
755: idleDelayRange = new Vector2(0.1f, 0.35f),
760: expReward = 0,
761: separationDistance = 0.7f,
762: separationStrength = 0.8f,
763: heightOffset = 0f,
765: sortingOrder = 1800,
770: colliderSize = new Vector3(0.5f, 0.7f, 0.5f),
771: colliderCenter = new Vector3(0f, 0.35f, 0f)
781: dungSprite = CreateCircleSprite("TempDungSprite", 48, 0.82f, true);
791: parasiteSprite = CreateCapsuleSprite("TempParasiteSprite", 48, 28);
801: shockwaveSprite = CreateCircleSprite("TempShockwaveSprite", 64, 0.95f, false);
809: Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
811: float center = (size - 1) * 0.5f;
813: float innerRadius = radius * 0.72f;
815: for (int y = 0; y < size; y++)
817: for (int x = 0; x < size; x++)
822: float alpha = 0f;
826: alpha = Mathf.Clamp01(1f - distance / radius * 0.35f);
830: alpha = Mathf.Clamp01(1f - Mathf.Abs(distance - (innerRadius + radius) * 0.5f) / Mathf.Max(0.01f, radius - innerRadius));
833: texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
838: Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
845: Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
847: Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
848: float rx = width * 0.42f;
849: float ry = height * 0.36f;
851: for (int y = 0; y < height; y++)
853: for (int x = 0; x < width; x++)
858: float alpha = value <= 1f ? 1f : 0f;
859: texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
864: Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), width);
888: this.radius = Mathf.Max(0.05f, radius);
891: this.parasiteDelay = Mathf.Max(0f, parasiteDelay);
892: destroyTime = Time.time + Mathf.Max(0.2f, lifeTime);
929: Vector3 toPlayer = PlayerController.Instance.transform.position - transform.position;
930: toPlayer.y = 0f;
```

## Assets/_Project/Scripts/Gameplay/Enemies/Bosses/LiverBossPattern.cs

```csharp
11: [Range(0.1f, 0.9f)] public float phase2HealthRatio = 0.5f;
14: [HideInInspector] public float phase1AttackDamage = 2f;
15: [HideInInspector] public float phase1MoveSpeed = 1f;
16: [HideInInspector] public float phase2AttackDamage = 2f;
17: [HideInInspector] public float phase2MoveSpeed = 1.5f;
20: public float bloodBombRange = 7f;
21: public float bloodBombExplosionRadius = 2.5f;
22: public float bloodBombFuseTime = 1f;
23: public float bloodBombCooldown = 4f;
24: [Range(0f, 1f)] public float attackPowerDebuffRatio = 0.15f;
25: public float attackPowerDebuffDuration = 5f;
26: public float bloodBombArcHeight = 1.4f;
27: public float bloodBombProjectileScale = 1.15f;
31: public float healingPosePreDelay = 1f;
32: public float healingPoseCooldown = 15f;
33: public float healingPoseDuration = 5f;
34: [Range(0f, 1f)] public float damageHealRatio = 0.7f;
39: public float healingPoseAnimationSpeed = 0.12f;
42: public float phase1PreferredDistanceRatio = 0.72f;
43: public float phase2PreferredDistance = 2.6f;
44: public float roamRadius = 9f;
47: public Color bloodBombColor = new Color(0.7f, 0f, 0.04f, 0.96f);
48: public Color explosionColor = new Color(0.9f, 0.05f, 0.08f, 0.45f);
51: public bool startInPhase2ForDebug = false;
53: public float fastPatternCooldown = 1f;
62: Phase1,
64: Phase2
68: [SerializeField, Range(0.1f, 0.9f)] private float phase2HealthRatio = 0.5f;
71: [SerializeField, HideInInspector] private float phase1AttackDamage = 2f;
72: [SerializeField, HideInInspector] private float phase1MoveSpeed = 1f;
73: [SerializeField, HideInInspector] private float phase2AttackDamage = 2f;
74: [SerializeField, HideInInspector] private float phase2MoveSpeed = 1.5f;
77: [SerializeField] private float bloodBombRange = 7f;
78: [SerializeField] private float bloodBombExplosionRadius = 2.5f;
79: [SerializeField] private float bloodBombFuseTime = 1f;
80: [SerializeField] private float bloodBombCooldown = 4f;
81: [SerializeField, Range(0f, 1f)] private float attackPowerDebuffRatio = 0.15f;
82: [SerializeField] private float attackPowerDebuffDuration = 5f;
83: [SerializeField] private float bloodBombArcHeight = 1.4f;
84: [SerializeField] private float bloodBombProjectileScale = 1.15f;
88: [SerializeField] private float healingPosePreDelay = 1f;
89: [SerializeField] private float healingPoseCooldown = 15f;
90: [SerializeField] private float healingPoseDuration = 5f;
91: [SerializeField, Range(0f, 1f)] private float damageHealRatio = 0.7f;
96: [SerializeField] private float healingPoseAnimationSpeed = 0.12f;
99: [SerializeField] private float phase1PreferredDistanceRatio = 0.72f;
100: [SerializeField] private float phase2PreferredDistance = 2.6f;
101: [SerializeField] private float roamRadius = 9f;
104: [SerializeField] private Color bloodBombColor = new Color(0.7f, 0f, 0.04f, 0.96f);
105: [SerializeField] private Color explosionColor = new Color(0.9f, 0.05f, 0.08f, 0.45f);
108: [SerializeField] private bool startInPhase2ForDebug = false;
110: [SerializeField] private float fastPatternCooldown = 1f;
118: private Vector3 anchorPosition;
119: private Vector3 baseScale = Vector3.one;
128: public void Initialize(EnemyController controller, Vector3 anchor, Transform parent, LiverBossPatternSettings settings = null)
135: phase = startInPhase2ForDebug ? BossPhase.Phase2 : BossPhase.Phase1;
136: nextBloodBombTime = Time.time + 1f;
137: nextHealingPoseTime = phase == BossPhase.Phase2 ? Time.time + GetHealingPoseCooldown() : float.PositiveInfinity;
165: phase2HealthRatio = settings.phase2HealthRatio;
166: phase1AttackDamage = settings.phase1AttackDamage;
167: phase1MoveSpeed = settings.phase1MoveSpeed;
168: phase2AttackDamage = settings.phase2AttackDamage;
169: phase2MoveSpeed = settings.phase2MoveSpeed;
188: phase1PreferredDistanceRatio = settings.phase1PreferredDistanceRatio;
189: phase2PreferredDistance = settings.phase2PreferredDistance;
193: startInPhase2ForDebug = settings.startInPhase2ForDebug;
244: nextBloodBombTime = Time.time + 1f;
245: nextHealingPoseTime = phase == BossPhase.Phase2
251: [ContextMenu("Debug/Force Phase 1")]
252: public void ForcePhase1ForDebug()
257: phase = BossPhase.Phase1;
265: [ContextMenu("Debug/Force Phase 2")]
266: public void ForcePhase2ForDebug()
271: phase = BossPhase.Phase2;
282: ForcePhase1ForDebug();
289: ForcePhase2ForDebug();
312: if (phase == BossPhase.Phase1 && stats != null && stats.HealthNormalized <= phase2HealthRatio)
314: StartCoroutine(EnterPhase2Routine());
323: if (phase == BossPhase.Phase1 && Time.time >= nextBloodBombTime && IsPlayerWithinBloodBombRange(player))
327: else if (phase == BossPhase.Phase2 && Time.time >= nextHealingPoseTime)
340: Vector3 moveDirection = GetMoveDirection();
341: if (moveDirection.sqrMagnitude <= 0.0001f)
346: float speed = stats != null ? stats.MoveSpeed : (phase == BossPhase.Phase2 ? phase2MoveSpeed : phase1MoveSpeed);
347: Vector3 step = moveDirection.normalized * Mathf.Max(0f, speed) * Time.deltaTime;
351: private IEnumerator EnterPhase2Routine()
358: float elapsed = 0f;
359: const float duration = 1f;
363: float t = Mathf.Clamp01(elapsed / duration);
364: float pulse = 1f + Mathf.Sin(t * Mathf.PI * 6f) * 0.1f;
370: phase = BossPhase.Phase2;
373: nextHealingPoseTime = Time.time + 1.5f;
385: Vector3 start = transform.position + Vector3.up * 1.2f;
386: Vector3 target = PlayerController.Instance != null
389: target.y = GetGroundHeight(target) + 0.05f;
391: EnemyDamageRequest damage = boss.CreatePatternDamage("blood-bomb", phase1AttackDamage);
399: 5000);
400: float elapsed = 0f;
401: float duration = Mathf.Max(0.05f, bloodBombFuseTime);
405: float t = Mathf.Clamp01(elapsed / duration);
406: Vector3 position = Vector3.Lerp(start, target, t);
430: float preDelay = Mathf.Max(0f, healingPosePreDelay);
435: float elapsed = 0f;
439: float pulse = 1f + Mathf.Sin(elapsed * 18f) * 0.06f;
445: elapsed = 0f;
446: float duration = Mathf.Max(0.05f, healingPoseDuration);
450: float pulse = 1f + Mathf.Sin(elapsed * 9f) * 0.05f;
451: transform.localScale = new Vector3(baseScale.x * (1.12f + pulse * 0.02f), baseScale.y * 0.86f, baseScale.z * (1.12f + pulse * 0.02f));
501: Vector3 toPlayer = GetDirectionToPlayer();
512: if (visualRenderer != null && Mathf.Abs(toPlayer.x) > 0.01f)
514: visualRenderer.flipX = toPlayer.x < 0f;
522: return frames != null && frames.Length > 0;
525: private void ExplodeBloodBomb(Vector3 center, EnemyDamageRequest damage)
528: center.y = GetGroundHeight(center) + 0.08f;
534: bloodBombExplosionRadius * 2f,
535: 4900);
536: StartCoroutine(FadeAndDestroy(explosion, 0.35f, explosionColor));
544: Vector3 toPlayer = player.transform.position - center;
545: toPlayer.y = 0f;
563: Vector3 startScale = obj.transform.localScale * 0.35f;
564: Vector3 endScale = obj.transform.localScale;
565: float elapsed = 0f;
569: float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
570: obj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
574: color.a = Mathf.Lerp(startColor.a, 0f, t);
586: if (damagedBoss != boss || !healingPoseActive || stats == null || appliedDamage <= 0f)
598: boss.ApplyBalancePhase(phase == BossPhase.Phase2 ? "Phase2" : "Default");
606: if (phase == BossPhase.Phase2)
608: stats.SetBaseStat(CharacterStatType.MoveSpeed, phase2MoveSpeed);
609: stats.SetBaseStat(CharacterStatType.AttackPower, phase2AttackDamage);
613: stats.SetBaseStat(CharacterStatType.MoveSpeed, phase1MoveSpeed);
614: stats.SetBaseStat(CharacterStatType.AttackPower, phase1AttackDamage);
631: private Vector3 GetMoveDirection()
633: Vector3 position = transform.position;
634: Vector3 fromAnchor = position - anchorPosition;
635: fromAnchor.y = 0f;
638: Vector3 toAnchor = anchorPosition - position;
639: toAnchor.y = 0f;
643: Vector3 toPlayer = PlayerController.Instance.transform.position - position;
644: toPlayer.y = 0f;
646: if (distance <= 0.0001f)
648: return Vector3.zero;
651: if (phase == BossPhase.Phase1)
653: float preferredDistance = Mathf.Max(0.5f, bloodBombRange * phase1PreferredDistanceRatio);
654: if (distance > bloodBombRange * 0.96f)
659: if (distance < preferredDistance * 0.72f)
664: return Vector3.Cross(Vector3.up, toPlayer);
667: if (distance > phase2PreferredDistance)
672: if (distance < phase2PreferredDistance * 0.55f)
677: return Vector3.zero;
687: Vector3 toPlayer = player.transform.position - transform.position;
688: toPlayer.y = 0f;
689: float range = Mathf.Max(0.1f, bloodBombRange);
696: ? Mathf.Max(0.05f, fastPatternCooldown)
697: : Mathf.Max(0f, bloodBombCooldown));
703: ? Mathf.Max(0.05f, fastPatternCooldown)
704: : Mathf.Max(0f, healingPoseCooldown));
707: private Vector3 GetDirectionToPlayer()
711: return Vector3.forward;
714: Vector3 direction = PlayerController.Instance.transform.position - transform.position;
715: direction.y = 0f;
716: return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
719: private float GetGroundHeight(Vector3 position)
741: private GameObject CreateTempSpriteObject(string name, Sprite sprite, Color color, Vector3 position, float scale, int sortingOrder)
759: for (int i = 0; i < activeTempObjects.Count; i++)
779: bloodBombSprite = CreateCircleSprite("TempBloodBombSprite", 48, 0.82f, true);
789: explosionSprite = CreateCircleSprite("TempBloodBombExplosionSprite", 64, 0.94f, false);
797: Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
799: float center = (size - 1) * 0.5f;
801: float innerRadius = radius * 0.72f;
803: for (int y = 0; y < size; y++)
805: for (int x = 0; x < size; x++)
810: float alpha = 0f;
814: alpha = Mathf.Clamp01(1f - distance / radius * 0.35f);
818: alpha = Mathf.Clamp01(1f - Mathf.Abs(distance - (innerRadius + radius) * 0.5f) / Mathf.Max(0.01f, radius - innerRadius));
821: texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
826: Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
```

## Assets/_Project/Scripts/Gameplay/Enemies/Bosses/LungBossPattern.cs

```csharp
10: [Header("Phase 1")]
11: [HideInInspector] public float phase1BrotherHealth = 40f;
12: [HideInInspector] public float phase1AttackDamage = 2f;
13: [HideInInspector] public float phase1MoveSpeed = 1f;
14: [HideInInspector] public float windGustMoveSpeed = 3f;
15: [Min(0f)] public float windGustSpeedMultiplier = 3f;
16: public float windGustInterval = 5f;
17: public float windGustDuration = 1.25f;
18: public float brotherSpacing = 3.2f;
20: [Header("Phase 1 Gas")]
21: public float phase1GasRange = 7f;
22: public float phase1GasCooldown = 3.5f;
23: public float phase1GasKnockback = 1f;
25: [Header("Phase 2")]
26: [HideInInspector] public float phase2MaxHealth = 100f;
27: [HideInInspector] public float phase2AttackDamage = 4f;
28: [HideInInspector] public float phase2MoveSpeed = 1.5f;
29: public float phase2GasRange = 10f;
30: public float phase2GasCooldown = 4f;
31: public float phase2GasKnockback = 1.2f;
34: public float diveCooldown = 8f;
35: public float diveWindup = 0.55f;
36: public float diveTravelTime = 0.65f;
37: public float diveArcHeight = 4f;
38: public float diveLandingRadius = 1.45f;
39: [HideInInspector] public float diveLandingDamage = 3f;
40: public float diveKnockback = 1.4f;
43: public float gasProjectileSpeed = 8f;
44: public float gasProjectileRadius = 0.7f;
45: public float gasProjectileScale = 1f;
46: public float gasWindup = 0.25f;
47: public float gasProjectileHeightOffset = 2f;
50: [HideInInspector] public float phase1ContactDamage = 1f;
51: [HideInInspector] public float phase2ContactDamage = 2f;
52: public float contactRadius = 1.05f;
53: public float contactCooldown = 1.1f;
56: public float phase1PreferredDistance = 5.2f;
57: public float phase2PreferredDistance = 4.2f;
58: public float roamRadius = 10f;
61: public Color gasColor = new Color(0.72f, 0.95f, 0.9f, 0.72f);
62: public Color windColor = new Color(0.78f, 0.9f, 1f, 0.34f);
63: public Color diveColor = new Color(0.5f, 0.85f, 1f, 0.42f);
66: public bool startInPhase2ForDebug = false;
68: public float fastPatternCooldown = 1f;
76: Phase1,
78: Phase2,
82: [Header("Phase 1")]
83: [SerializeField, HideInInspector] private float phase1BrotherHealth = 40f;
84: [SerializeField, HideInInspector] private float phase1AttackDamage = 2f;
85: [SerializeField, HideInInspector] private float phase1MoveSpeed = 1f;
86: [SerializeField, HideInInspector] private float windGustMoveSpeed = 3f;
87: [SerializeField] private float windGustInterval = 5f;
88: [SerializeField] private float windGustDuration = 1.25f;
89: [SerializeField] private float brotherSpacing = 3.2f;
91: [Header("Phase 1 Gas")]
92: [SerializeField] private float phase1GasRange = 7f;
93: [SerializeField] private float phase1GasCooldown = 3.5f;
94: [SerializeField] private float phase1GasKnockback = 1f;
96: [Header("Phase 2")]
97: [SerializeField, HideInInspector] private float phase2MaxHealth = 100f;
98: [SerializeField, HideInInspector] private float phase2AttackDamage = 4f;
99: [SerializeField, HideInInspector] private float phase2MoveSpeed = 1.5f;
100: [SerializeField] private float phase2GasRange = 10f;
101: [SerializeField] private float phase2GasCooldown = 4f;
102: [SerializeField] private float phase2GasKnockback = 1.2f;
105: [SerializeField] private float diveCooldown = 8f;
106: [SerializeField] private float diveWindup = 0.55f;
107: [SerializeField] private float diveTravelTime = 0.65f;
108: [SerializeField] private float diveArcHeight = 4f;
109: [SerializeField] private float diveLandingRadius = 1.45f;
110: [SerializeField, HideInInspector] private float diveLandingDamage = 3f;
111: [SerializeField] private float diveKnockback = 1.4f;
114: [SerializeField] private float gasProjectileSpeed = 8f;
115: [SerializeField] private float gasProjectileRadius = 0.7f;
116: [SerializeField] private float gasProjectileScale = 1f;
117: [SerializeField] private float gasWindup = 0.25f;
118: [SerializeField] private float gasProjectileHeightOffset = 2f;
121: [SerializeField, HideInInspector] private float phase1ContactDamage = 1f;
122: [SerializeField, HideInInspector] private float phase2ContactDamage = 2f;
123: [SerializeField] private float contactRadius = 1.05f;
124: [SerializeField] private float contactCooldown = 1.1f;
127: [SerializeField] private float phase1PreferredDistance = 5.2f;
128: [SerializeField] private float phase2PreferredDistance = 4.2f;
129: [SerializeField] private float roamRadius = 10f;
132: [SerializeField] private Color gasColor = new Color(0.72f, 0.95f, 0.9f, 0.72f);
133: [SerializeField] private Color windColor = new Color(0.78f, 0.9f, 1f, 0.34f);
134: [SerializeField] private Color diveColor = new Color(0.5f, 0.85f, 1f, 0.42f);
137: [SerializeField] private bool startInPhase2ForDebug = false;
139: [SerializeField] private float fastPatternCooldown = 1f;
144: private readonly EnemyController[] brothers = new EnemyController[2];
145: private readonly CharacterStats[] brotherStats = new CharacterStats[2];
146: private readonly SpriteRenderer[] brotherRenderers = new SpriteRenderer[2];
147: private readonly bool[] gasRunning = new bool[2];
148: private readonly float[] nextGasTime = new float[2];
149: private readonly float[] nextContactTime = new float[2];
152: private Vector3 anchorPosition;
153: private Vector3 baseScale = Vector3.one;
156: private float windGustSpeedMultiplier = 3f;
159: private float nextPhase2GasTime;
161: private bool phase2ActionRunning;
164: private Vector3 lastDefeatedPosition;
168: public Vector3 LastDefeatedPosition => lastDefeatedPosition;
171: public void Initialize(EnemyController controller, Vector3 anchor, Transform parent, LungBossPatternSettings settings = null)
178: phase = startInPhase2ForDebug ? BossPhase.Phase2 : BossPhase.Phase1;
181: phase2ActionRunning = false;
183: nextWindGustTime = Time.time + 1.2f;
185: nextGasTime[0] = Time.time + 0.8f;
186: nextGasTime[1] = Time.time + 2.2f;
187: nextContactTime[0] = Time.time + 1f;
188: nextContactTime[1] = Time.time + 1f;
189: nextPhase2GasTime = Time.time + 1.2f;
190: nextDiveTime = Time.time + 3f;
192: brothers[0] = controller;
196: ConfigureBrother(0);
197: ConfigureBrother(1);
199: if (phase == BossPhase.Phase2)
201: survivor = brothers[0];
202: EnterPhase2(survivor);
215: phase1BrotherHealth = settings.phase1BrotherHealth;
216: phase1AttackDamage = settings.phase1AttackDamage;
217: phase1MoveSpeed = settings.phase1MoveSpeed;
223: phase1GasRange = settings.phase1GasRange;
224: phase1GasCooldown = settings.phase1GasCooldown;
225: phase1GasKnockback = settings.phase1GasKnockback;
226: phase2MaxHealth = settings.phase2MaxHealth;
227: phase2AttackDamage = settings.phase2AttackDamage;
228: phase2MoveSpeed = settings.phase2MoveSpeed;
229: phase2GasRange = settings.phase2GasRange;
230: phase2GasCooldown = settings.phase2GasCooldown;
231: phase2GasKnockback = settings.phase2GasKnockback;
244: phase1ContactDamage = settings.phase1ContactDamage;
245: phase2ContactDamage = settings.phase2ContactDamage;
248: phase1PreferredDistance = settings.phase1PreferredDistance;
249: phase2PreferredDistance = settings.phase2PreferredDistance;
254: startInPhase2ForDebug = settings.startInPhase2ForDebug;
261: if (brothers[0] == null || brothers[0].Config == null)
266: Vector3 spawnPosition = GetGroundedPosition(anchorPosition + Vector3.right * Mathf.Max(1f, brotherSpacing) * 0.5f);
267: int poolArchetypeId = EnemyController.GetPoolArchetypeId(brothers[0].Config);
268: brothers[1] = EnemyController.Acquire(encounterParent, $"{brothers[0].Config.name}_Brother", poolArchetypeId);
269: brothers[1].Configure(null, brothers[0].Config, spawnPosition, spawnPosition);
270: brothers[1].transform.SetParent(encounterParent, true);
271: brothers[1].SetIgnoreMidBossArenaRestriction(true);
276: float halfSpacing = Mathf.Max(1f, brotherSpacing) * 0.5f;
277: if (brothers[0] != null)
279: brothers[0].transform.position = GetGroundedPosition(anchorPosition + Vector3.left * halfSpacing);
282: if (brothers[1] != null)
284: brothers[1].transform.position = GetGroundedPosition(anchorPosition + Vector3.right * halfSpacing);
307: brotherStats[index].SetBaseStat(CharacterStatType.MaxHealth, phase1BrotherHealth, true);
308: brotherStats[index].SetBaseStat(CharacterStatType.AttackPower, phase1AttackDamage);
309: brotherStats[index].SetBaseStat(CharacterStatType.MoveSpeed, phase1MoveSpeed);
347: for (int i = 0; i < gasRunning.Length; i++)
352: phase2ActionRunning = false;
357: nextWindGustTime = Time.time + 1.2f;
359: nextGasTime[0] = Time.time + 0.8f;
360: nextGasTime[1] = Time.time + 2.2f;
361: nextContactTime[0] = Time.time + 1f;
362: nextContactTime[1] = Time.time + 1f;
363: nextPhase2GasTime = Time.time + 1.2f;
364: nextDiveTime = Time.time + 3f;
380: for (int i = 0; i < brothers.Length; i++)
391: for (int i = 0; i < brothers.Length; i++)
425: if (phase == BossPhase.Phase1)
428: for (int i = 0; i < brothers.Length; i++)
436: TryRunPhase1Gas(i, brother);
437: TryApplyContactDamage(i, brother, phase1ContactDamage);
440: else if (phase == BossPhase.Phase2 && survivor != null && !survivor.IsDead)
442: TryApplyContactDamage(GetBrotherIndex(survivor), survivor, phase2ContactDamage);
443: if (!phase2ActionRunning)
449: else if (Time.time >= nextPhase2GasTime)
464: if (phase == BossPhase.Phase1)
466: for (int i = 0; i < brothers.Length; i++)
474: MoveBrotherPhase1(i, brother);
477: else if (phase == BossPhase.Phase2 && survivor != null && !survivor.IsDead && !phase2ActionRunning)
479: MoveSurvivorPhase2(survivor);
485: bool firstAlive = IsAlive(brothers[0]);
486: bool secondAlive = IsAlive(brothers[1]);
495: if (phase == BossPhase.Phase1 && firstAlive != secondAlive)
497: EnterPhase2(firstAlive ? brothers[0] : brothers[1]);
499: else if (phase == BossPhase.Phase2 && survivor != null && survivor.IsDead)
506: private void EnterPhase2(EnemyController newSurvivor)
512: for (int i = 0; i < gasRunning.Length; i++)
518: phase = BossPhase.Phase2;
519: AudioManager.Instance?.PlaySFX("LungPhase2");
520: phase2ActionRunning = false;
521: nextPhase2GasTime = Time.time + 1f;
522: nextDiveTime = Time.time + 2.5f;
525: if (survivor != null && survivor.Balance != null) survivor.ApplyBalancePhase("Phase2", true);
526: else if (survivorIndex >= 0 && brotherStats[survivorIndex] != null)
528: brotherStats[survivorIndex].SetBaseStat(CharacterStatType.MaxHealth, phase2MaxHealth, true);
529: brotherStats[survivorIndex].SetBaseStat(CharacterStatType.AttackPower, phase2AttackDamage);
530: brotherStats[survivorIndex].SetBaseStat(CharacterStatType.MoveSpeed, phase2MoveSpeed);
541: Vector3 center = survivor.transform.position;
542: center.y = GetGroundHeight(center) + 0.08f;
543: GameObject ring = CreateTempSpriteObject("LungBoss_EnrageWind", GetRingSprite(), windColor, center, 1.2f, 4900);
544: StartCoroutine(FadeAndDestroy(ring, 0.75f, windColor, 4f));
559: phase2ActionRunning = false;
560: for (int i = 0; i < gasRunning.Length; i++)
576: windGustEndTime = Time.time + Mathf.Max(0.1f, windGustDuration);
577: nextWindGustTime = Time.time + GetEncounterRearmCooldown(Mathf.Max(0.2f, windGustInterval));
579: Vector3 center = anchorPosition;
580: center.y = GetGroundHeight(center) + 0.08f;
581: GameObject wind = CreateTempSpriteObject("LungBoss_WindGust", GetRingSprite(), windColor, center, 2.4f, 2400);
582: StartCoroutine(FadeAndDestroy(wind, Mathf.Max(0.1f, windGustDuration), windColor, 7f));
585: private void TryRunPhase1Gas(int index, EnemyController shooter)
587: if (gasRunning[index] || Time.time < nextGasTime[index] || !IsPlayerWithinRange(shooter.transform.position, phase1GasRange))
593: nextGasTime[index] = Time.time + GetPhase1GasCooldown();
597: phase1GasRange,
598: phase1AttackDamage,
599: phase1GasKnockback,
605: phase2ActionRunning = true;
609: float windup = Mathf.Max(0f, diveWindup);
610: float elapsed = 0f;
611: Vector3 actorBaseScale = actor.transform.localScale;
615: float pulse = 1f + Mathf.Sin(elapsed * 18f) * 0.08f;
620: Vector3 start = actor.transform.position;
621: Vector3 target = PlayerController.Instance != null ? PlayerController.Instance.transform.position : anchorPosition;
623: elapsed = 0f;
624: float travelTime = Mathf.Max(0.05f, diveTravelTime);
628: float t = Mathf.Clamp01(elapsed / travelTime);
629: Vector3 position = Vector3.Lerp(start, target, t);
638: Vector3 landing = actor.transform.position;
639: landing.y = GetGroundHeight(landing) + 0.08f;
640: GameObject ring = CreateTempSpriteObject("LungBoss_DiveLanding", GetRingSprite(), diveColor, landing, diveLandingRadius * 1.4f, 4900);
641: StartCoroutine(FadeAndDestroy(ring, 0.35f, diveColor, diveLandingRadius * 2.4f));
650: phase2ActionRunning = false;
655: phase2ActionRunning = true;
656: nextPhase2GasTime = Time.time + GetPhase2GasCooldown();
658: Vector3 forward = GetDirectionToPlayer(shooter.transform.position);
661: AudioManager.Instance?.PlaySFX("LungPhase2Skill");
669: phase2GasRange,
670: shooter.CreatePatternDamage("gas", phase2AttackDamage),
671: phase2GasKnockback);
673: phase2ActionRunning = false;
678: Vector3 direction,
697: Vector3 startScale = shooter.transform.localScale;
699: float elapsed = 0f;
700: float duration = Mathf.Max(0f, gasWindup);
704: float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
705: shooter.transform.localScale = new Vector3(startScale.x * (1f + t * 0.08f), startScale.y * (1f - t * 0.05f), startScale.z);
715: Vector3 origin,
716: Vector3 direction,
721: float distance = 0f;
722: float maxRange = Mathf.Max(0.1f, range);
723: float speed = Mathf.Max(0.1f, gasProjectileSpeed);
759: private void MoveBrotherPhase1(int index, EnemyController brother)
761: Vector3 direction = GetPhase1MoveDirection(index, brother.transform.position);
762: if (direction.sqrMagnitude <= 0.0001f)
768: ? brother.Stats.MoveSpeed * (IsWindGustActive() ? windGustSpeedMultiplier : 1f)
769: : (IsWindGustActive() ? windGustMoveSpeed : phase1MoveSpeed);
770: brother.MoveByExternalPattern(direction.normalized * Mathf.Max(0f, speed) * Time.deltaTime);
773: private void MoveSurvivorPhase2(EnemyController actor)
775: Vector3 direction = GetPhase2MoveDirection(actor.transform.position);
776: if (direction.sqrMagnitude <= 0.0001f)
781: actor.MoveByExternalPattern(direction.normalized * Mathf.Max(0f, actor.Balance != null ? actor.Stats.MoveSpeed : phase2MoveSpeed) * Time.deltaTime);
784: private Vector3 GetPhase1MoveDirection(int index, Vector3 position)
789: return Vector3.zero;
792: Vector3 fromAnchor = position - anchorPosition;
793: fromAnchor.y = 0f;
796: Vector3 toAnchor = anchorPosition - position;
797: toAnchor.y = 0f;
801: Vector3 toPlayer = player.transform.position - position;
802: toPlayer.y = 0f;
804: if (distance <= 0.0001f)
806: return Vector3.zero;
809: if (distance > phase1GasRange * 0.95f)
814: if (distance < phase1PreferredDistance * 0.7f)
819: Vector3 tangent = Vector3.Cross(Vector3.up, toPlayer);
820: return index == 0 ? tangent : -tangent;
823: private Vector3 GetPhase2MoveDirection(Vector3 position)
828: return Vector3.zero;
831: Vector3 toPlayer = player.transform.position - position;
832: toPlayer.y = 0f;
834: if (distance <= 0.0001f)
836: return Vector3.zero;
839: if (distance > phase2GasRange * 0.9f)
844: if (distance < phase2PreferredDistance * 0.7f)
849: return Vector3.zero;
854: if (index < 0 || index >= nextContactTime.Length || Time.time < nextContactTime[index])
865: nextContactTime[index] = Time.time + actor.GetRearmCooldown(Mathf.Max(0.1f, contactCooldown));
869: private GameObject CreateGasProjectile(Vector3 shooterPosition, Vector3 direction)
871: direction.y = 0f;
872: direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
873: Vector3 position = shooterPosition + direction * 0.65f;
874: position.y = GetGroundHeight(position) + Mathf.Max(0f, gasProjectileHeightOffset);
875: return CreateTempSpriteObject("LungBoss_GasShot", GetGasSprite(), gasColor, position, gasProjectileScale, 5000);
878: private void MoveGasProjectile(GameObject projectile, Vector3 direction, float step)
885: direction.y = 0f;
896: Vector3 toPlayer = player.transform.position - obj.transform.position;
897: toPlayer.y = 0f;
901: private bool IsPlayerWithinRange(Vector3 center, float range)
909: Vector3 toPlayer = player.transform.position - center;
910: toPlayer.y = 0f;
911: float safeRange = Mathf.Max(0.1f, range);
915: private Vector3 GetDirectionToPlayer(Vector3 from)
920: return Vector3.forward;
923: Vector3 direction = player.transform.position - from;
924: direction.y = 0f;
925: return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
935: EnemyController actor = survivor != null ? survivor : GetBrother(0);
939: private float GetPhase1GasCooldown()
942: ? Mathf.Max(0.05f, fastPatternCooldown)
943: : Mathf.Max(0.05f, phase1GasCooldown));
946: private float GetPhase2GasCooldown()
949: ? Mathf.Max(0.05f, fastPatternCooldown)
950: : Mathf.Max(0.05f, phase2GasCooldown));
956: ? Mathf.Max(0.05f, fastPatternCooldown)
957: : Mathf.Max(0.05f, diveCooldown));
962: if (index < 0 || index >= brothers.Length)
972: for (int i = 0; i < brothers.Length; i++)
980: return 0;
988: private Vector3 GetGroundedPosition(Vector3 position)
994: private float GetGroundHeight(Vector3 position)
1000: private static void ApplyPlayerKnockback(PlayerController player, Vector3 sourcePosition, float distance)
1002: if (player == null || distance <= 0f)
1007: Vector3 direction = player.transform.position - sourcePosition;
1008: direction.y = 0f;
1009: if (direction.sqrMagnitude <= 0.0001f)
1011: direction = Vector3.forward;
1014: Vector3 displacement = direction.normalized * distance;
1026: Vector3 startScale = obj.transform.localScale;
1027: Vector3 finalScale = Vector3.one * Mathf.Max(0.01f, endScale);
1028: float elapsed = 0f;
1032: float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
1033: obj.transform.localScale = Vector3.Lerp(startScale, finalScale, t);
1037: color.a = Mathf.Lerp(startColor.a, 0f, t);
1047: private GameObject CreateTempSpriteObject(string name, Sprite sprite, Color color, Vector3 position, float scale, int sortingOrder)
1065: for (int i = 0; i < activeTempObjects.Count; i++)
1080: gasSprite = CreateCircleSprite("TempLungGasSprite", 48, 0.86f, true);
1090: ringSprite = CreateCircleSprite("TempLungWindRingSprite", 64, 0.94f, false);
1098: Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
1100: float center = (size - 1) * 0.5f;
1102: float innerRadius = radius * 0.7f;
1104: for (int y = 0; y < size; y++)
1106: for (int x = 0; x < size; x++)
1111: float alpha = 0f;
1115: alpha = Mathf.Clamp01(1f - distance / radius * 0.55f);
1119: float midpoint = (innerRadius + radius) * 0.5f;
1120: alpha = Mathf.Clamp01(1f - Mathf.Abs(distance - midpoint) / Mathf.Max(0.01f, radius - innerRadius));
1123: texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
1128: Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
```

## Assets/_Project/Scripts/Gameplay/Enemies/Bosses/StomachBossPattern.cs

```csharp
11: [Range(0.1f, 0.9f)] public float phase2HealthRatio = 0.65f;
14: [HideInInspector] public float phase1AttackDamage = 3f;
15: [HideInInspector] public float phase1MoveSpeed = 0.5f;
16: [HideInInspector] public float phase2AttackDamage = 3f;
17: [HideInInspector] public float phase2MoveSpeed = 1f;
19: [Header("Phase 1 Basic")]
20: public float phase1MeleeRange = 1.6f;
21: public float phase1MeleeCooldown = 1.7f;
24: public float chargeSpeed = 4f;
25: public float chargeCooldown = 5f;
26: [HideInInspector] public float chargeDamage = 4f;
27: public float chargeKnockbackDistance = 2f;
28: public float chargeWindup = 0.35f;
29: public float chargeDuration = 1.2f;
30: public float chargeHitRadius = 1.25f;
32: [Header("Phase 2 Pattern")]
33: public float phase2PatternInterval = 3f;
34: public float phase2PreferredDistance = 4f;
35: public float roamRadius = 9f;
38: public float acidRange = 9f;
39: [HideInInspector] public float acidDamage = 2f;
40: public float acidDebuffDuration = 5f;
41: public float acidTickInterval = 1f;
42: [HideInInspector] public float acidTickDamage = 1f;
43: public float acidProjectileTravelTime = 0.55f;
44: public float acidArcHeight = 0.8f;
45: public float acidProjectileScale = 0.9f;
46: public float acidHitRadius = 1.15f;
49: public float suctionRange = 4.2f;
50: public float suctionDuration = 0.4f;
51: public float suctionStopDistance = 1f;
52: public float spitDelay = 0.35f;
53: public float spitReturnDuration = 0.18f;
54: [HideInInspector] public float shortRangeDamage = 5f;
55: public float shortRangeRadius = 2.2f;
57: public float suctionAttackAnimationSpeed = 0.08f;
60: public Color acidColor = new Color(0.55f, 0.95f, 0.15f, 0.95f);
61: public Color acidSplashColor = new Color(0.5f, 0.9f, 0.12f, 0.42f);
62: public Color suctionColor = new Color(0.75f, 0.95f, 0.45f, 0.42f);
63: public Color leakColor = new Color(0.38f, 0.85f, 0.18f, 0.38f);
66: public bool startInPhase2ForDebug = false;
68: public float fastPatternCooldown = 1f;
77: Phase1,
79: Phase2
82: private enum Phase2PatternRequest
89: private const float ArenaMovementClampExtraMargin = 0.2f;
92: [SerializeField, Range(0.1f, 0.9f)] private float phase2HealthRatio = 0.65f;
95: [SerializeField, HideInInspector] private float phase1AttackDamage = 3f;
96: [SerializeField, HideInInspector] private float phase1MoveSpeed = 0.5f;
97: [SerializeField, HideInInspector] private float phase2AttackDamage = 3f;
98: [SerializeField, HideInInspector] private float phase2MoveSpeed = 1f;
100: [Header("Phase 1 Basic")]
101: [SerializeField] private float phase1MeleeRange = 1.6f;
102: [SerializeField] private float phase1MeleeCooldown = 1.7f;
105: [SerializeField] private float chargeSpeed = 4f;
106: [SerializeField] private float chargeCooldown = 5f;
107: [SerializeField, HideInInspector] private float chargeDamage = 4f;
108: [SerializeField] private float chargeKnockbackDistance = 2f;
109: [SerializeField] private float chargeWindup = 0.35f;
110: [SerializeField] private float chargeDuration = 1.2f;
111: [SerializeField] private float chargeHitRadius = 1.25f;
113: [Header("Phase 2 Pattern")]
114: [SerializeField] private float phase2PatternInterval = 3f;
115: [SerializeField] private float phase2PreferredDistance = 4f;
116: [SerializeField] private float roamRadius = 9f;
119: [SerializeField] private float acidRange = 9f;
120: [SerializeField, HideInInspector] private float acidDamage = 2f;
121: [SerializeField] private float acidDebuffDuration = 5f;
122: [SerializeField] private float acidTickInterval = 1f;
123: [SerializeField, HideInInspector] private float acidTickDamage = 1f;
124: [SerializeField] private float acidProjectileTravelTime = 0.55f;
125: [SerializeField] private float acidArcHeight = 0.8f;
126: [SerializeField] private float acidProjectileScale = 0.9f;
127: [SerializeField] private float acidHitRadius = 1.15f;
130: [SerializeField] private float suctionRange = 4.2f;
131: [SerializeField] private float suctionDuration = 0.4f;
132: [SerializeField] private float suctionStopDistance = 1f;
133: [SerializeField] private float spitDelay = 0.35f;
134: [SerializeField] private float spitReturnDuration = 0.18f;
135: [SerializeField, HideInInspector] private float shortRangeDamage = 5f;
136: [SerializeField] private float shortRangeRadius = 2.2f;
138: [SerializeField] private float suctionAttackAnimationSpeed = 0.08f;
141: [SerializeField] private Color acidColor = new Color(0.55f, 0.95f, 0.15f, 0.95f);
142: [SerializeField] private Color acidSplashColor = new Color(0.5f, 0.9f, 0.12f, 0.42f);
143: [SerializeField] private Color suctionColor = new Color(0.75f, 0.95f, 0.45f, 0.42f);
144: [SerializeField] private Color leakColor = new Color(0.38f, 0.85f, 0.18f, 0.38f);
147: [SerializeField] private bool startInPhase2ForDebug = false;
149: [SerializeField] private float fastPatternCooldown = 1f;
157: private Vector3 anchorPosition;
158: private Vector3 baseScale = Vector3.one;
162: private float nextPhase2AttackTime;
169: public void Initialize(EnemyController controller, Vector3 anchor, Transform parent, StomachBossPatternSettings settings = null)
176: phase = startInPhase2ForDebug ? BossPhase.Phase2 : BossPhase.Phase1;
177: nextChargeTime = Time.time + 1.2f;
178: nextMeleeTime = Time.time + 1f;
179: nextPhase2AttackTime = phase == BossPhase.Phase2 ? Time.time + 1f : float.PositiveInfinity;
204: phase2HealthRatio = settings.phase2HealthRatio;
205: phase1AttackDamage = settings.phase1AttackDamage;
206: phase1MoveSpeed = settings.phase1MoveSpeed;
207: phase2AttackDamage = settings.phase2AttackDamage;
208: phase2MoveSpeed = settings.phase2MoveSpeed;
209: phase1MeleeRange = settings.phase1MeleeRange;
210: phase1MeleeCooldown = settings.phase1MeleeCooldown;
218: phase2PatternInterval = settings.phase2PatternInterval;
219: phase2PreferredDistance = settings.phase2PreferredDistance;
243: startInPhase2ForDebug = settings.startInPhase2ForDebug;
290: nextChargeTime = Time.time + 1.2f;
291: nextMeleeTime = Time.time + 1f;
292: nextPhase2AttackTime = phase == BossPhase.Phase2
293: ? Time.time + 1f
298: [ContextMenu("Debug/Force Phase 1")]
299: public void ForcePhase1ForDebug()
303: phase = BossPhase.Phase1;
306: nextPhase2AttackTime = float.PositiveInfinity;
312: [ContextMenu("Debug/Force Phase 2")]
313: public void ForcePhase2ForDebug()
317: phase = BossPhase.Phase2;
320: nextPhase2AttackTime = Time.time;
329: ForcePhase1ForDebug();
336: ForcePhase2ForDebug();
337: StartCoroutine(Phase2AttackRoutine(Phase2PatternRequest.AcidSpray));
343: ForcePhase2ForDebug();
344: StartCoroutine(Phase2AttackRoutine(Phase2PatternRequest.SuckAndSpit));
366: if (phase == BossPhase.Phase1 && stats != null && stats.HealthNormalized <= phase2HealthRatio)
368: StartCoroutine(EnterPhase2Routine());
377: if (phase == BossPhase.Phase1)
383: else if (Time.time >= nextMeleeTime && IsPlayerWithinRange(player, phase1MeleeRange))
388: else if (phase == BossPhase.Phase2 && Time.time >= nextPhase2AttackTime)
390: StartCoroutine(Phase2AttackRoutine());
401: Vector3 moveDirection = GetMoveDirection();
402: if (moveDirection.sqrMagnitude <= 0.0001f)
407: float speed = stats != null ? stats.MoveSpeed : phase == BossPhase.Phase2 ? phase2MoveSpeed : phase1MoveSpeed;
408: boss.MoveByExternalPattern(moveDirection.normalized * Mathf.Max(0f, speed) * Time.deltaTime);
411: private IEnumerator EnterPhase2Routine()
417: Vector3 leakPosition = transform.position;
418: leakPosition.y = GetGroundHeight(leakPosition) + 0.08f;
419: GameObject leak = CreateTempSpriteObject("StomachBoss_AcidLeak", GetRingSprite(), leakColor, leakPosition, 1.5f, 4900);
420: StartCoroutine(FadeAndDestroy(leak, 1f, leakColor, 3.8f));
422: float elapsed = 0f;
423: const float duration = 1f;
427: float t = Mathf.Clamp01(elapsed / duration);
428: transform.localScale = baseScale * (1f + Mathf.Sin(t * Mathf.PI * 6f) * 0.08f);
433: phase = BossPhase.Phase2;
437: nextPhase2AttackTime = Time.time + 1f;
446: nextMeleeTime = Time.time + boss.GetRearmCooldown(Mathf.Max(0.1f, phase1MeleeCooldown));
448: float elapsed = 0f;
449: const float duration = 0.28f;
453: float t = Mathf.Clamp01(elapsed / duration);
454: transform.localScale = new Vector3(baseScale.x * (1f + t * 0.08f), baseScale.y * (1f - t * 0.06f), baseScale.z);
459: if (IsPlayerWithinRange(player, phase1MeleeRange))
462: player.TakeDamage(boss.CreatePatternDamage("melee", phase1AttackDamage));
474: Vector3 direction = GetDirectionToPlayer();
475: float windup = Mathf.Max(0f, chargeWindup);
476: float elapsed = 0f;
481: float pulse = 1f + Mathf.Sin(elapsed * 22f) * 0.08f;
482: transform.localScale = new Vector3(baseScale.x * 1.12f, baseScale.y * 0.88f, baseScale.z) * pulse;
488: elapsed = 0f;
489: float duration = Mathf.Max(0.05f, chargeDuration);
490: float speed = Mathf.Max(0f, chargeSpeed);
513: private IEnumerator Phase2AttackRoutine(Phase2PatternRequest request = Phase2PatternRequest.Auto)
516: nextPhase2AttackTime = Time.time + GetPhase2PatternInterval();
522: if (request == Phase2PatternRequest.AcidSpray)
526: else if (request == Phase2PatternRequest.SuckAndSpit)
551: float windup = 0.25f;
552: float elapsed = 0f;
556: transform.localScale = new Vector3(baseScale.x * 1.08f, baseScale.y * 0.92f, baseScale.z);
560: Vector3 start = transform.position + Vector3.up * 1.05f;
561: Vector3 direction = GetDirectionToPlayer();
562: Vector3 target = PlayerController.Instance != null
565: Vector3 fromBoss = target - transform.position;
566: fromBoss.y = 0f;
567: float range = Mathf.Max(0.1f, acidRange);
573: target.y = GetGroundHeight(target) + 0.05f;
582: 5000);
584: elapsed = 0f;
585: float travelTime = Mathf.Max(0.05f, acidProjectileTravelTime);
589: float t = Mathf.Clamp01(elapsed / travelTime);
590: Vector3 position = Vector3.Lerp(start, target, t);
618: Vector3 startPosition = player.transform.position;
619: Vector3 directionToPlayer = startPosition - transform.position;
620: directionToPlayer.y = 0f;
621: Vector3 pullDirection = directionToPlayer.sqrMagnitude > 0.0001f ? directionToPlayer.normalized : Vector3.forward;
622: Vector3 pullTarget = transform.position + pullDirection * Mathf.Max(0.2f, suctionStopDistance);
630: Mathf.Max(0.1f, suctionRange * 0.55f),
631: 4900);
632: StartCoroutine(FadeAndDestroy(suction, Mathf.Max(0.1f, suctionDuration), suctionColor, Mathf.Max(0.1f, suctionRange * 0.9f)));
634: float elapsed = 0f;
635: float duration = Mathf.Max(0.05f, suctionDuration);
639: float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
640: Vector3 desired = Vector3.Lerp(startPosition, pullTarget, t);
642: transform.localScale = new Vector3(baseScale.x * (1f + t * 0.12f), baseScale.y * (1f - t * 0.08f), baseScale.z);
646: elapsed = 0f;
647: float delay = Mathf.Max(0f, spitDelay);
653: float pulse = 1f + Mathf.Sin(elapsed * 18f) * 0.04f;
654: transform.localScale = new Vector3(baseScale.x * 1.12f, baseScale.y * 0.9f, baseScale.z) * pulse;
668: if (boss == null || suctionAttackSprites == null || suctionAttackSprites.Length == 0)
676: private IEnumerator SpitPlayerBackToStart(PlayerController player, Vector3 targetPosition)
684: Vector3 returnStart = player.transform.position;
685: float elapsed = 0f;
686: float duration = Mathf.Max(0.01f, spitReturnDuration);
690: float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
691: Vector3 desired = Vector3.Lerp(returnStart, targetPosition, t);
699: private void SplashAcid(Vector3 center, EnemyDamageRequest damage, EnemyDamageRequest tickDamage)
701: center.y = GetGroundHeight(center) + 0.08f;
707: acidHitRadius * 2f,
708: 4900);
709: StartCoroutine(FadeAndDestroy(splash, 0.35f, acidSplashColor, acidHitRadius * 2.7f));
725: boss.ApplyBalancePhase(phase == BossPhase.Phase2 ? "Phase2" : "Default");
733: if (phase == BossPhase.Phase2)
735: stats.SetBaseStat(CharacterStatType.MoveSpeed, phase2MoveSpeed);
736: stats.SetBaseStat(CharacterStatType.AttackPower, phase2AttackDamage);
740: stats.SetBaseStat(CharacterStatType.MoveSpeed, phase1MoveSpeed);
741: stats.SetBaseStat(CharacterStatType.AttackPower, phase1AttackDamage);
758: private Vector3 GetMoveDirection()
760: Vector3 position = transform.position;
761: Vector3 fromAnchor = position - anchorPosition;
762: fromAnchor.y = 0f;
765: Vector3 toAnchor = anchorPosition - position;
766: toAnchor.y = 0f;
773: return Vector3.zero;
776: Vector3 toPlayer = player.transform.position - position;
777: toPlayer.y = 0f;
779: if (distance <= 0.0001f)
781: return Vector3.zero;
784: if (phase == BossPhase.Phase1)
786: return distance > phase1MeleeRange * 1.05f ? toPlayer : Vector3.zero;
789: if (distance > acidRange * 0.88f)
794: if (distance < phase2PreferredDistance * 0.65f)
799: if (distance > phase2PreferredDistance * 1.25f)
804: return Vector3.zero;
812: private static bool IsPlayerWithinRange(PlayerController player, Vector3 center, float range)
819: Vector3 toPlayer = player.transform.position - center;
820: toPlayer.y = 0f;
821: float safeRange = Mathf.Max(0.1f, range);
828: ? Mathf.Max(0.05f, fastPatternCooldown)
829: : Mathf.Max(0f, chargeCooldown));
832: private float GetPhase2PatternInterval()
835: ? Mathf.Max(0.05f, fastPatternCooldown)
836: : Mathf.Max(0f, phase2PatternInterval));
839: private Vector3 GetDirectionToPlayer()
844: return Vector3.forward;
847: Vector3 direction = player.transform.position - transform.position;
848: direction.y = 0f;
849: return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
852: private float GetGroundHeight(Vector3 position)
874: private static void ApplyPlayerKnockback(PlayerController player, Vector3 sourcePosition, float distance)
876: if (player == null || distance <= 0f)
881: Vector3 direction = player.transform.position - sourcePosition;
882: direction.y = 0f;
883: if (direction.sqrMagnitude <= 0.0001f)
885: direction = Vector3.forward;
891: private static void MovePlayerBy(PlayerController player, Vector3 displacement)
898: displacement.y = 0f;
899: if (displacement.sqrMagnitude <= 0.000001f)
904: Vector3 currentPosition = player.transform.position;
905: Vector3 desiredPosition = currentPosition + displacement;
911: out Vector3 clampedDesiredPosition))
914: displacement.y = 0f;
915: if (displacement.sqrMagnitude <= 0.000001f)
924: displacement.y = 0f;
925: if (displacement.sqrMagnitude <= 0.000001f)
942: return 0.55f;
945: return Mathf.Max(0.35f, Mathf.Max(hitCollider.bounds.extents.x, hitCollider.bounds.extents.z) + ArenaMovementClampExtraMargin);
955: Vector3 currentPosition = player.transform.position;
956: if (!MidBossArenaController.TryClampPositionInsideLockedArena(currentPosition, margin, out Vector3 correctedPosition))
961: Vector3 planarDelta = correctedPosition - currentPosition;
962: planarDelta.y = 0f;
963: if (planarDelta.sqrMagnitude <= 0.000001f)
978: Vector3 currentPosition = player.transform.position;
979: if (!TryClampPositionInsideActiveMap(currentPosition, margin, out Vector3 correctedPosition))
984: Vector3 planarDelta = correctedPosition - currentPosition;
985: planarDelta.y = 0f;
986: if (planarDelta.sqrMagnitude <= 0.000001f)
994: private static bool TryClampPositionInsideActiveMap(Vector3 position, float margin, out Vector3 clampedPosition)
998: if (biome == null || biome.MapWidth <= 0 || biome.MapHeight <= 0)
1003: float tileSize = Mathf.Max(0.01f, biome.TileSize);
1004: float safeMargin = Mathf.Max(0f, margin);
1023: Vector3 startScale = obj.transform.localScale;
1024: Vector3 finalScale = Vector3.one * Mathf.Max(0.01f, endScale);
1025: float elapsed = 0f;
1029: float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
1030: obj.transform.localScale = Vector3.Lerp(startScale, finalScale, t);
1034: color.a = Mathf.Lerp(startColor.a, 0f, t);
1044: private GameObject CreateTempSpriteObject(string name, Sprite sprite, Color color, Vector3 position, float scale, int sortingOrder)
1062: for (int i = 0; i < activeTempObjects.Count; i++)
1077: circleSprite = CreateCircleSprite("TempStomachAcidSprite", 48, 0.86f, true);
1087: ringSprite = CreateCircleSprite("TempStomachRingSprite", 64, 0.94f, false);
1095: Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
1097: float center = (size - 1) * 0.5f;
1099: float innerRadius = radius * 0.68f;
1101: for (int y = 0; y < size; y++)
1103: for (int x = 0; x < size; x++)
1108: float alpha = 0f;
1112: alpha = Mathf.Clamp01(1f - distance / radius * 0.35f);
1116: float midpoint = (innerRadius + radius) * 0.5f;
1117: alpha = Mathf.Clamp01(1f - Mathf.Abs(distance - midpoint) / Mathf.Max(0.01f, radius - innerRadius));
1120: texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
1125: Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
```

## Assets/_Project/Scripts/Gameplay/Enemies/Combat/AggroDebris.cs

```csharp
14: private const float AggroScanInterval = 0.2f;
41: this.elapsed = 0f;
54: debrisRenderer.color = new Color(0.6f, 0.2f, 0.8f, 0.7f);
62: if (vfxSprites != null && vfxSprites.Length > 0)
64: SpawnShockwaveVFX(vfxSprites, vfxSpeed, sortingOrder + 100);
80: vfxObject.transform.localPosition = new Vector3(0f, 1f, 0f);
81: vfxObject.transform.localScale = Vector3.one;
85: vfxRenderer.color = new Color(0.7f, 0.3f, 1f, 0.9f); // 보라색 충격파
86: vfxRenderer.sprite = frames[0];
95: int loopCount = 0;
100: if (loopCount >= 2)
122: float expandTime = 0.5f;
123: float t = Mathf.Clamp01(elapsed / expandTime);
124: float currentScale = Mathf.Lerp(1f, vfxScale, t);
125: vfxObject.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
130: float vfxAlpha = Mathf.Lerp(0.9f, 0.3f, t);
131: vfxRenderer.color = new Color(0.7f, 0.3f, 1f, vfxAlpha);
137: if (remaining < 3f && debrisRenderer != null)
139: float alpha = Mathf.Clamp01(remaining / 3f) * 0.7f;
166: for (int i = 0; i < enemies.Count; i++)
173: Vector3 delta = enemy.transform.position - transform.position;
174: delta.y = 0f;
180: enemy.ApplyAggroBoost(1.0f);  // chaseRadius 2배 증가
181: enemy.ForceChasePlayer();     // 이동속도 3배 + 즉시 플레이어에게 돌진
```

## Assets/_Project/Scripts/Gameplay/Enemies/Combat/CombatHitGeometry.cs

```csharp
8: public static bool SweepTouchesPlayer(Vector3 from, Vector3 to, float radius, PlayerController player)
12: if (collider == null) return PointSegmentDistanceSquared(Flat(player.transform.position), Flat(from), Flat(to)) <= (radius + .35f) * (radius + .35f);
14: if (TryCapsule(collider, out Vector2 a, out Vector2 b, out float bodyRadius))
15: return SegmentDistanceSquared(Flat(from), Flat(to), a, b) <= Mathf.Pow(Mathf.Max(0, radius) + bodyRadius, 2);
19: public static bool BoundsTouchCollider(Bounds bounds, Collider collider, float skin = 0)
22: if (TryCapsule(collider, out Vector2 a, out Vector2 b, out float radius))
23: return SegmentRectDistanceSquared(a, b, bounds) <= Mathf.Pow(radius + Mathf.Max(0, skin), 2);
24: Bounds other = collider.bounds; other.Expand(new Vector3(skin * 2, 0, skin * 2));
28: public static bool TryCapsule(Collider collider, out Vector2 a, out Vector2 b, out float radius)
30: Vector3 s = collider.transform.lossyScale; s = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
34: radius = capsule.radius * (axis == 0 ? Mathf.Max(s.y, s.z) : axis == 1 ? Mathf.Max(s.x, s.z) : Mathf.Max(s.x, s.y));
35: Vector3 direction = collider.transform.TransformDirection(axis == 0 ? Vector3.right : axis == 1 ? Vector3.up : Vector3.forward);
36: Vector3 center = collider.transform.TransformPoint(capsule.center);
37: Vector3 half = direction * Mathf.Max(0, capsule.height * s[axis] * .5f - radius);
45: a = b = default; radius = 0; return false;
51: if (config != null && config.colliderSize.x > 0 && config.colliderSize.z > 0)
53: Vector3 scale = enemy.transform.lossyScale; scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
54: return new Bounds(enemy.transform.TransformPoint(config.colliderCenter), Vector3.Scale(config.colliderSize, scale));
56: var collider = enemy.GetComponent<Collider>(); return collider != null ? collider.bounds : new Bounds(enemy.transform.position, Vector3.zero);
58: public static Vector2 Flat(Vector3 p) => new Vector2(p.x, p.z);
59: public static float PointRectDistanceSquared(Vector2 p, Bounds rect)
61: float x = Mathf.Max(0, Mathf.Max(rect.min.x - p.x, p.x - rect.max.x));
62: float z = Mathf.Max(0, Mathf.Max(rect.min.z - p.y, p.y - rect.max.z)); return x * x + z * z;
64: public static float SegmentRectDistanceSquared(Vector2 a, Vector2 b, Bounds rect)
66: Vector2 bl = new Vector2(rect.min.x, rect.min.z), br = new Vector2(rect.max.x, rect.min.z);
67: Vector2 tl = new Vector2(rect.min.x, rect.max.z), tr = new Vector2(rect.max.x, rect.max.z);
68: if (PointRectDistanceSquared(a, rect) == 0 || PointRectDistanceSquared(b, rect) == 0) return 0;
72: public static float SegmentDistanceSquared(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
74: Vector2 ab = b - a, cd = d - c, ac = c - a;
76: if (Mathf.Abs(cross) > .000001f)
79: if (t >= 0 && t <= 1 && u >= 0 && u <= 1) return 0;
84: public static float PointSegmentDistanceSquared(Vector2 p, Vector2 a, Vector2 b)
86: Vector2 v = b - a; float t = v.sqrMagnitude > .000001f ? Mathf.Clamp01(Vector2.Dot(p - a, v) / v.sqrMagnitude) : 0;
```

## Assets/_Project/Scripts/Gameplay/Enemies/Combat/EnemyContactDamage.cs

```csharp
26: damage = Mathf.Max(0f, contactDamage);
27: knockbackDistance = Mathf.Max(0f, contactKnockbackDistance);
73: || damage <= 0f
92: float skin = (collider != null ? collider.contactOffset : 0) + (player.HitCollider != null ? player.HitCollider.contactOffset : 0);
109: if (knockbackDistance <= 0f)
114: Vector3 direction = player.transform.position - transform.position;
115: direction.y = 0f;
116: if (direction.sqrMagnitude <= 0.0001f)
119: direction.y = 0f;
122: if (direction.sqrMagnitude <= 0.0001f)
124: direction = Vector3.forward;
```

## Assets/_Project/Scripts/Gameplay/Enemies/Combat/EnemyProjectile.cs

```csharp
13: private const float MinHitRadius = 0.15f;
23: private Vector3 moveDirection;
41: return 0;
44: int returnedCount = 0;
45: for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
70: public static EnemyProjectile Acquire(Vector3 position, Sprite sprite, Vector3 scale)
75: while (Pool.Count > 0)
95: proj.elapsed = 0f;
104: public void Launch(Vector3 direction, float damage, float speed, float lifeTime, EnemyController sourceEnemy = null)
111: elapsed = 0f;
149: Vector3 previousPosition = transform.position;
219: private bool IsTouchingPlayer(PlayerController player, Collider playerCollider, Vector3 previousPosition, Vector3 currentPosition)
224: Vector3 scale = transform.lossyScale;
225: float visualRadius = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z)) * 0.5f;
246: spriteRenderer.sortingOrder = 5000;
263: int size = 32;
264: Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
266: float center = (size - 1) * 0.5f;
270: Color coreColor = new Color(1f, 1f, 0.4f, 1f);    // 밝은 노란색 중심
271: Color edgeColor = new Color(1f, 0.5f, 0.1f, 1f);  // 주황색 가장자리
272: Color glowColor = new Color(1f, 0.7f, 0.2f, 0.5f); // 외곽 글로우
274: for (int y = 0; y < size; y++)
276: for (int x = 0; x < size; x++)
283: if (dist <= radius * 0.6f)
286: float t = dist / (radius * 0.6f);
292: float t = (dist - radius * 0.6f) / (radius * 0.4f);
304: defaultProjectileSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
```

## Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyBalance.cs

```csharp
18: stats.SetBaseStat(CharacterStatType.MaxHealth, Mathf.Max(1f, values.MaxHealth), restoreHealth);
34: : seconds * Mathf.Max(0.01f, DifficultyBalanceService.GetEnemyBalance(IsBossEncounter).attackCooldown);
48: ? new EnemyDamageRequest(Balance.ContactEnabled ? stats.AttackPower * Balance.ContactCoefficient : 0f, this)
```

## Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyCombat.cs

```csharp
15: if (IsDead || IsBossEncounter || stats == null || damage <= 0f) return;
19: float wholeDamage = Mathf.Floor(pendingFloodDamage + 0.00001f);
20: if (wholeDamage < 1f) return;
21: pendingFloodDamage = Mathf.Max(0f, pendingFloodDamage - wholeDamage);
25: private float DifficultyAttackCooldown => config != null ? GetRearmCooldown(config.attackCooldown) : 0f;
38: if (attackTimer > 0f) return false;
42: if (attackFrames != null && attackFrames.Length > 0)
75: if (attackFrames == null || attackFrames.Length == 0)
93: if (IsDead || animatedSprite == null || frames == null || frames.Length == 0 || attackAnimPlaying)
102: animatedSprite.PlayOneShot(frames, Mathf.Max(0.01f, frameRate), () =>
183: Vector3 delta = player.transform.position - GetCurrentPosition(); delta.y = 0;
199: Vector3 localCenter = config.expandColliderOnAttack ? config.attackColliderCenter : config.colliderCenter;
200: Vector3 localSize = config.expandColliderOnAttack ? config.attackColliderSize : config.colliderSize;
201: if (localSize.x <= 0f || localSize.y <= 0f || localSize.z <= 0f)
206: Vector3 scaledSize = Vector3.Scale(localSize, Abs(transform.lossyScale));
212: private static Vector3 Abs(Vector3 value)
214: return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
222: Vector3 spawnPos = GetCurrentPosition();
223: Vector3 toPlayer = PlayerController.Instance.transform.position - spawnPos;
224: toPlayer.y = 0f;
225: if (toPlayer.sqrMagnitude < 0.0001f) return;
227: Vector3 dir = toPlayer.normalized;
229: spawnPos.y += 2f;
234: spriteRenderer.flipX = dir.x < 0f;
261: if (finalDamage <= 0f)
267: if (appliedDamage > 0f)
280: Debug.Log($"[DamageLog] Player -> {enemyName} : {appliedDamage:0.##} (HP {stats.CurrentHealth:0.##}/{stats.MaxHealth:0.##})");
300: else if (appliedDamage > 0f && !ignoreMidBossArenaRestriction)
345: bool hasDirectional = config.attackSpritesUp != null && config.attackSpritesUp.Length > 0;
355: case 0: return config.attackSpritesUp;                        // 상
356: case 1: return config.attackSprites;                          // 우
357: case 2: return config.attackSprites;                          // 좌 (flipX로 처리)
358: case 3: return config.attackSpritesDown;                      // 하
367: Vector3 toPlayer = playerTransform.position - GetCurrentPosition();
368: toPlayer.y = 0f;
370: if (toPlayer.sqrMagnitude <= 0.000001f)
386: bool hasDirectional = config.attackSpritesUp != null && config.attackSpritesUp.Length > 0;
400: Vector3 toPlayer = player.transform.position - GetCurrentPosition();
401: if (Mathf.Abs(toPlayer.x) > 0.01f)
403: spriteRenderer.flipX = toPlayer.x < 0f;
```

## Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyDirectionalVisual.cs

```csharp
27: public void CommitPatternFacing(Vector3 direction, bool authoredFacingLeft = true)
38: public Vector3 GetPatternVisualOrigin()
41: Vector2 origin = patternDirections.Origin(patternFacing);
42: return visualRoot.TransformPoint(new Vector3(origin.x, origin.y, 0));
53: private bool ApplyPatternDirection(Vector3 direction)
57: if (Vector3.ProjectOnPlane(direction, Vector3.up).sqrMagnitude >= .000001f) patternFacingInitialized = true;
67: for (int i = 0; i < source.Length; i++) result[i] = patternDirections.Resolve(source[i], patternFacing);
```

## Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyEliteDeathHandler.cs

```csharp
56: Vector3 deathPos = GetCurrentPosition();
60: if (vfxSprites != null && vfxSprites.Length > 0)
78: private void SpawnSplitVfxThenSpawn(Vector3 deathPos, EnemySpawnRuleConfig splitConfig, Sprite[] vfxSprites)
82: vfxObj.transform.position = deathPos + new Vector3(0f, 0.8f, 0f);
85: vfxRenderer.sprite = vfxSprites[0];
86: vfxRenderer.sortingOrder = config.sortingOrder + 200;
87: vfxRenderer.color = new Color(0.5f, 0.2f, 1f, 0.9f);
92: vfxObj.transform.localScale = Vector3.one * config.splitVfxScale;
109: fadeOut.Init(vfxRenderer, 0.3f);
117: if (config.splitVfxSprites != null && config.splitVfxSprites.Length > 0)
121: if (cachedVoidShieldSprites != null && cachedVoidShieldSprites.Length > 0)
126: if (loaded != null && loaded.Length > 0)
135: cachedVoidShieldSprites = GenerateVoidShieldSprites(6, 64);
142: for (int f = 0; f < frameCount; f++)
144: Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
147: float center = size * 0.5f;
148: float maxRadius = size * 0.45f;
150: for (int y = 0; y < size; y++)
152: for (int x = 0; x < size; x++)
159: float innerRadius = 0.5f + phase * 0.3f;
160: float outerRadius = 0.9f + phase * 0.1f;
161: float ring = 1f - Mathf.Clamp01(Mathf.Abs(dist - (innerRadius + outerRadius) * 0.5f) / ((outerRadius - innerRadius) * 0.5f));
162: float alpha = ring * (1f - phase * 0.5f);
165: float r = Mathf.Lerp(0.4f, 0.7f, phase);
166: float g = Mathf.Lerp(0.1f, 0.3f, phase);
167: float b = Mathf.Lerp(0.8f, 1f, phase);
174: sprites[f] = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
181: private void DoSpawnSplitEnemies(Vector3 deathPos, EnemySpawnRuleConfig splitConfig)
188: private void DoSpawnSplitEnemies(Vector3 deathPos, EnemySpawnRuleConfig splitConfig, int count, Transform spawnParent)
198: for (int i = 0; i < count; i++)
201: Vector2 offset = Random.insideUnitCircle * 1.5f;
202: Vector3 spawnPos = deathPos + new Vector3(offset.x, 0f, offset.y);
206: Vector2Int grid = biome.WorldToGrid(spawnPos);
227: Vector3 pos = GetCurrentPosition();
228: pos.y += 0.5f;
237: if (config.deathSprites != null && config.deathSprites.Length > 0)
238: debrisSprite = config.deathSprites[config.deathSprites.Length - 1];
239: else if (config.idleSprites != null && config.idleSprites.Length > 0)
240: debrisSprite = config.idleSprites[0];
```

## Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyMovement.cs

```csharp
18: if (chaseRadius < 0f
26: return leashRadius >= 0f
40: return effectiveAttackRange >= 0f
49: return leashRadius < 0f
57: return idleTimer <= 0f;
71: if (TryPickWanderDestination(out Vector3 wanderDest))
81: Vector3 chase = playerTransform.position;
105: Vector3 currentPosition = GetCurrentPosition();
106: Vector3 separation = GetSeparationVector(currentPosition);
108: Vector3 flatCurrent = new Vector3(currentPosition.x, 0f, currentPosition.z);
109: Vector3 flatDestination = new Vector3(destination.x, 0f, destination.z);
110: Vector3 toDestination = flatDestination - flatCurrent;
113: if (distance <= Mathf.Max(0.01f, config.stoppingDistance))
119: Vector3 moveDirection = toDestination.normalized;
120: if (separation.sqrMagnitude > 0.0001f)
122: Vector3 combined = moveDirection + separation * config.separationStrength;
123: if (combined.sqrMagnitude > 0.0001f)
129: Vector3 step = moveDirection * (stats != null ? stats.MoveSpeed : 0f) * deltaTime;
156: public void ApplyKnockback(Vector3 worldDirection, float distance)
158: if (IsDead || IsPatternPositionLocked || distance <= 0f) return;
160: Vector3 planarDirection = new Vector3(worldDirection.x, 0f, worldDirection.z);
161: if (planarDirection.sqrMagnitude <= 0.0001f) return;
164: Vector3 displacement = planarDirection * distance;
165: Vector3 currentPosition = GetCurrentPosition();
203: public bool MoveByExternalPattern(Vector3 step)
205: if (IsDead || step.sqrMagnitude <= 0.000001f)
231: public void SetPositionFromExternalPattern(Vector3 position)
240: Vector3 toPlayer = playerTransform.position - GetCurrentPosition();
241: toPlayer.y = 0f;
242: if (toPlayer.sqrMagnitude < 0.0001f) return;
245: chargeElapsed = 0f;
246: chargeCurrentSpeed = 0f;
262: chargeCurrentSpeed = Mathf.Lerp(0f, config.chargeSpeed, chargeElapsed / accelTime);
270: Vector3 step = chargeDirection * chargeCurrentSpeed * deltaTime;
271: Vector3 currentPos = GetCurrentPosition();
283: Vector3 toPlayer = playerTransform.position - GetCurrentPosition();
284: toPlayer.y = 0f;
286: if (Vector3.Dot(chargeDirection, toPlayer.normalized) < 0f)
294: if (chargeElapsed > 1.5f)
306: chargeCurrentSpeed = 0f;
307: chargeCooldownTimer = GetRearmCooldown(3f);
314: config.chaseRadius *= (1f + percentBoost);
334: 2.0f,
341: attackTimer = 0f;
356: private void SetDestination(Vector3 targetPosition)
363: private bool TryPickWanderDestination(out Vector3 wanderDestination)
372: for (int i = 0; i < 8; i++)
374: Vector2 offset = Random.insideUnitCircle * Mathf.Max(0f, config.wanderRadius);
375: Vector3 candidate = anchorPosition + new Vector3(offset.x, 0f, offset.y);
376: Vector2Int grid = biome.WorldToGrid(candidate);
391: private bool TryMove(Vector3 currentPosition, Vector3 step)
394: Vector2 half = boxCollider != null ? new Vector2(boxCollider.bounds.extents.x, boxCollider.bounds.extents.z) : Vector2.one * .4f;
395: bool Clear(Vector3 target) => ResidueRubble.CanTraverse(currentPosition, target, half)
404: Vector3 targetPosition = currentPosition + step;
411: Vector3 moveX = new Vector3(step.x, 0f, 0f);
412: Vector3 moveZ = new Vector3(0f, 0f, step.z);
416: if (moveX.sqrMagnitude > 0f && Clear(currentPosition + moveX))
421: if (moveZ.sqrMagnitude > 0f && Clear(currentPosition + moveZ))
429: if (moveZ.sqrMagnitude > 0f && Clear(currentPosition + moveZ))
434: if (moveX.sqrMagnitude > 0f && Clear(currentPosition + moveX))
444: private Vector3 GetSeparationVector(Vector3 currentPosition)
446: if (config == null || config.separationDistance <= 0f)
447: return Vector3.zero;
450: Vector3 separation = Vector3.zero;
451: Vector2Int centerCell = ToSpatialCell(currentPosition);
452: int searchRadius = Mathf.Max(1, Mathf.CeilToInt(config.separationDistance / SpatialHashCellSize));
458: Vector2Int cell = new Vector2Int(centerCell.x + x, centerCell.y + y);
464: for (int i = 0; i < enemiesInCell.Count; i++)
469: Vector3 delta = currentPosition - other.GetCurrentPosition();
470: delta.y = 0f;
472: if (distanceSq <= 0.0001f || distanceSq > maxDistanceSq) continue;
475: float weight = 1f - (distance / config.separationDistance);
485: private Vector3 GetCurrentPosition()
491: private void MoveToPosition(Vector3 position)
506: private void SetPosition(Vector3 position)
521: private static Vector2Int ToSpatialCell(Vector3 position)
523: return new Vector2Int(
529: private void RegisterOrUpdateSpatialCell(Vector3 position)
531: Vector2Int newCell = ToSpatialCell(position);
564: if (list.Count == 0)
574: private static float GetPlanarDistanceSqr(Vector3 a, Vector3 b)
586: return 0f;
590: Vector3 activeAttackSize = config.expandColliderOnAttack
594: float minRange = Mathf.Max(0.25f, bodyContactRange);
597: if (config.attackRange <= 0f)
606: private float GetMeleeContactRange(Vector3 localSize, PlayerController player)
608: Vector3 scaledSize = Vector3.Scale(localSize, Abs(transform.lossyScale));
609: float enemyReach = Mathf.Max(scaledSize.x, scaledSize.z) * 0.5f;
617: float skin = (boxCollider != null ? boxCollider.contactOffset : 0) + (player != null && player.HitCollider != null ? player.HitCollider.contactOffset : 0);
618: return Mathf.Min(.02f, Mathf.Max(0, skin)); // Only PhysX skin tolerance, not the former 0.2m reach bonus.
625: return 0.45f;
632: return 0.45f;
```

## Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyPatternLifetime.cs

```csharp
56: for (int i = owned.Count - 1; i >= 0; i--)
89: for (int i = owned.Count - 1; i >= 0; i--)
```

## Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyPooling.cs

```csharp
13: while (pool.Count > 0)
22: pooled.transform.localPosition = Vector3.zero;
24: pooled.transform.localScale = Vector3.one;
35: public void Configure(IEnemySpawnOwner owner, EnemySpawnRuleConfig config, Vector3 anchorPosition, Vector3 spawnPosition)
40: public void Configure(IEnemySpawnOwner owner, EnemySpawnRuleConfig config, Vector3 anchorPosition, Vector3 spawnPosition,
45: pendingFloodDamage = 0f;
66: idleTimer = 0f;
67: attackTimer = 0f;
74: facingDirection = 3;
77: chargeElapsed = 0f;
78: chargeCurrentSpeed = 0f;
79: chargeCooldownTimer = 0f;
88: transform.localScale = Vector3.one;
150: destination = Vector3.zero;
151: idleTimer = 0f;
152: attackTimer = 0f;
158: facingDirection = 3;
230: body.angularVelocity = Vector3.zero;
251: if (config == null) return 0;
252: return unchecked((config.poissonSalt * 397) ^ Animator.StringToHash(config.name ?? "Enemy"));
```

## Assets/_Project/Scripts/Gameplay/Enemies/Core/Behaviors/EnemyVisual.cs

```csharp
11: billboard.ResetBaseLocalPosition(Vector3.down * billboard.YOffset);
12: else if (visualRoot != null) visualRoot.localPosition = Vector3.zero;
27: public void SetPatternFacing(Vector3 direction, bool authoredFacingLeft = true)
31: direction.y = 0;
33: float horizontal = camera != null ? Vector3.Dot(direction, camera.transform.right) : direction.x;
34: if (Mathf.Abs(horizontal) <= .01f) return; // Keep the last side when the target is directly above/below.
35: spriteRenderer.flipX = authoredFacingLeft ? horizontal > 0 : horizontal < 0;
53: if (deathFrames == null || deathFrames.Length == 0)
89: if (frames == null || frames.Length == 0)
104: if (frames.Length == 1)
108: spriteRenderer.sprite = frames[0];
146: case 0:
148: case 3:
158: return frames != null && frames.Length > 0;
162: private void UpdateFacingFromVector(Vector3 direction)
164: direction.y = 0f;
165: if (direction.sqrMagnitude <= 0.000001f)
189: private int GetPlanarDirection(Vector3 direction)
193: return direction.x >= 0f ? 1 : 2;
196: return direction.z >= 0f ? 0 : 3;
207: if (facingDirection == 1)
213: if (facingDirection == 2)
233: if (facingDirection == 0)
238: if (facingDirection == 3)
253: Vector3 position = GetCurrentPosition();
254: Vector2Int grid = biome.WorldToGrid(position);
262: if (Mathf.Abs(position.y - cachedGroundHeight) > 0.0001f)
307: Balance != null ? Balance.Current.MoveSpeed : config.moveSpeed * Mathf.Max(0.01f, balance.moveSpeed)));
310: Balance != null ? Mathf.Max(1f, Balance.Current.MaxHealth) : config.maxHealth * Mathf.Max(0.01f, balance.maxHealth)));
316: for (int i = 0; i < additionalStats.Count; i++)
331: visualRoot.localPosition = Vector3.zero;
345: billboard.ResetBaseLocalPosition(Vector3.zero);
```

## Assets/_Project/Scripts/Gameplay/Enemies/Core/EnemyCombatCalculator.cs

```csharp
11: : config != null ? config.attackDamage : 0f;
13: return Mathf.Max(0f, damage);
20: : 1f;
22: return Mathf.Max(0f, rawDamage * multiplier);
```

## Assets/_Project/Scripts/Gameplay/Enemies/Core/EnemyController.cs

```csharp
14: private const float SpatialHashCellSize = 2f;
18: private static readonly Dictionary<Vector2Int, List<EnemyController>> ActiveEnemyCells = new Dictionary<Vector2Int, List<EnemyController>>();
43: private Vector3 anchorPosition;  // 스폰 기준점 (leash/wander 중심)
44: private Vector3 destination;     // 현재 이동 목적지
60: private int facingDirection = 3;  // 0=상, 1=우, 2=좌, 3=하
64: private Vector3 chargeDirection;  // 돌진 방향 (고정)
77: private Vector2Int cachedGroundGrid;
79: private Vector2Int currentSpatialCell;
94: public bool CanCharge => IsCharger && chargeCooldownTimer <= 0f;
112: if (chargeCooldownTimer > 0f)
```

## Assets/_Project/Scripts/Gameplay/Enemies/FSM/EnemyAttackState.cs

```csharp
18: enemy.TryPerformAttack(0f);
```

## Assets/_Project/Scripts/Gameplay/Enemies/Spawning/BiomeEliteFieldSpawner.cs

```csharp
33: public void Refresh(Vector3 playerPosition)
36: Vector3 delta = playerPosition - transform.position;
37: delta.y = 0;
40: Vector3 enemyDelta = playerPosition - ActiveEnemy.transform.position;
41: enemyDelta.y = 0;
```

## Assets/_Project/Scripts/Gameplay/Enemies/Spawning/EliteSpawner.cs

```csharp
15: [SerializeField] private float spawnDistanceMin = 8f;
16: [SerializeField] private float spawnDistanceMax = 12f;
17: [SerializeField, Min(1)] private int eliteSpawnEveryNKills = 10;
27: private float warningDuration = 3f;
36: private float miniDuration = 1.5f;
72: eliteSpawnEveryNKills = Mathf.Max(1, normalKillsPerElite);
86: totalNormalKills = 0;
101: || activeEliteConfigs.Count == 0)
107: int killsPerElite = Mathf.Max(1, eliteSpawnEveryNKills);
108: if (totalNormalKills % killsPerElite != 0) return;
149: if (!showingWarning && warningQueue.Count > 0)
163: if (warningTimer < 0.5f)
165: float t = warningTimer / 0.5f;
167: float scale = Mathf.Lerp(2f, 1f, t);
168: warningText.transform.localScale = new Vector3(scale, scale, 1f);
170: else if (warningTimer < 2f)
172: canvasGroup.alpha = 1f;
173: float pulse = 1f + Mathf.Sin(warningTimer * 6f) * 0.05f;
174: warningText.transform.localScale = new Vector3(pulse, pulse, 1f);
178: float t = (warningTimer - 2f) / 1f;
179: canvasGroup.alpha = 1f - t;
196: canvasGroup.alpha = 0f;
197: warningTimer = 0f;
212: miniCanvasGroup.alpha = 0f;
213: miniTimer = 0f;
222: if (miniTimer < 0.2f)
224: miniCanvasGroup.alpha = miniTimer / 0.2f;
227: else if (miniTimer < 1f)
229: miniCanvasGroup.alpha = 1f;
234: float t = (miniTimer - 1f) / 0.5f;
235: miniCanvasGroup.alpha = 1f - t;
250: if (activeEliteConfigs.Count == 0) return;
253: EnemySpawnRuleConfig config = activeEliteConfigs[Random.Range(0, activeEliteConfigs.Count)];
256: Vector3 spawnPos = FindSpawnPosition(player.position, biome, config);
297: warningCanvas.sortingOrder = 9999;
301: scaler.referenceResolution = new Vector2(1920, 1080);
304: canvasGroup.alpha = 0f;
312: bgImage.color = new Color(0f, 0f, 0f, 0.6f);
314: bgRect.anchorMin = Vector2.zero;
315: bgRect.anchorMax = Vector2.one;
316: bgRect.offsetMin = Vector2.zero;
317: bgRect.offsetMax = Vector2.zero;
325: warningText.fontSize = 64;
327: warningText.color = new Color(1f, 0.3f, 0.3f, 1f);
335: outline.effectColor = new Color(0f, 0f, 0f, 1f);
336: outline.effectDistance = new Vector2(3f, -3f);
339: textRect.anchorMin = new Vector2(0.5f, 0.5f);
340: textRect.anchorMax = new Vector2(0.5f, 0.5f);
341: textRect.sizeDelta = new Vector2(1200f, 300f);
342: textRect.anchoredPosition = Vector2.zero;
357: miniCanvas.sortingOrder = 9998;
361: scaler.referenceResolution = new Vector2(1920, 1080);
364: miniCanvasGroup.alpha = 0f;
374: miniText.fontSize = 32;
376: miniText.color = new Color(1f, 0.6f, 0.6f, 0.9f); // 연한 빨간
384: outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
385: outline.effectDistance = new Vector2(2f, -2f);
388: textRect.anchorMin = new Vector2(0.5f, 1f);
389: textRect.anchorMax = new Vector2(0.5f, 1f);
390: textRect.sizeDelta = new Vector2(800f, 60f);
391: textRect.anchoredPosition = new Vector2(0f, -80f); // 상단에서 80px 아래
396: private Vector3 FindSpawnPosition(Vector3 playerPos, BiomeManager biome, EnemySpawnRuleConfig config)
398: for (int i = 0; i < 20; i++)
400: Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(spawnDistanceMin, spawnDistanceMax);
401: Vector3 candidate = playerPos + new Vector3(offset.x, 0f, offset.y);
405: Vector2Int grid = biome.WorldToGrid(candidate);
415: Vector3 fallback = playerPos + new Vector3(spawnDistanceMin, 0f, 0f);
```

## Assets/_Project/Scripts/Gameplay/Enemies/Spawning/EnemySpawner.cs

```csharp
11: private const int SpawnPositionAttempts = 10;       // 스폰 위치 탐색 최대 시도 횟수
12: private const float MinSpawnSpacing = 0.75f;       // 적 간 최소 거리 (겹침 방지)
13: private const float ViewportVisibilityMargin = 0.05f; // 카메라 시야 마진 (밖에서만 리스폰)
14: private const float EvaluationInterval = 0.1f;
15: private const int EvaluationStaggerSlots = 10;
22: private Vector3 anchorPosition;          // 스폰 중심점
27: private float activationRadiusOverride = -1f;
31: float multiplier = balance != null ? balance.enemyMaxAlive : 1f;
32: return Mathf.Max(0, Mathf.RoundToInt(config.maxAlive * Mathf.Max(0f, multiplier)));
37: float multiplier = balance != null ? balance.enemyRespawnCooldown : 1f;
38: return config.respawnCooldown * Mathf.Max(0.01f, multiplier);
52: Vector3 anchorPosition,
53: float activationRadiusOverride = -1f)
62: nextSpawnTime = 0f;
98: float activationRadius = activationRadiusOverride >= 0f
101: Vector3 playerDelta = playerTransform.position - anchorPosition;
103: if (activationRadius < 0f || playerDistanceSqr > activationRadius * activationRadius)
105: if (activeEnemies.Count > 0)
202: return activeEnemies.Count > 0;
207: if (!TryGetSpawnPosition(out Vector3 spawnPosition))
225: private bool TryGetSpawnPosition(out Vector3 spawnPosition)
230: spawnPosition = Vector3.zero;
234: for (int i = 0; i < SpawnPositionAttempts; i++)
236: Vector2 offset2D = Random.insideUnitCircle * Mathf.Max(0f, config.spawnRadius);
237: Vector3 candidate = anchorPosition + new Vector3(offset2D.x, 0f, offset2D.y);
238: Vector2Int grid = biome.WorldToGrid(candidate);
254: spawnPosition = Vector3.zero;
258: private bool IsTooCloseToSpawnedEnemy(Vector3 candidate)
261: for (int i = 0; i < activeEnemies.Count; i++)
269: Vector3 delta = enemy.transform.position - candidate;
270: delta.y = 0f;
295: for (int i = activeEnemies.Count - 1; i >= 0; i--)
306: for (int i = activeEnemies.Count - 1; i >= 0; i--)
327: Vector3 visibilityPosition = anchorPosition;
328: visibilityPosition.y += config != null ? config.heightOffset : 0f;
330: Vector3 viewportPoint = activeCamera.WorldToViewportPoint(visibilityPosition);
331: if (viewportPoint.z <= 0f)
337: && viewportPoint.x <= 1f + ViewportVisibilityMargin
339: && viewportPoint.y <= 1f + ViewportVisibilityMargin;
```

## Assets/_Project/Scripts/Gameplay/Enemies/StatusEffects/EnemySkillBridge.cs

```csharp
31: if (!IsValidTarget || damage <= 0f)
```

## Assets/_Project/Scripts/Gameplay/Enemies/StatusEffects/EnemyStatusEffectController.cs

```csharp
29: private float poisonWaitDuration = -1f;
36: private float bleedWaitDuration = -1f;
55: stunEndTime = 0f;
56: vulnerabilityEndTime = 0f;
57: damageTakenIncreaseRatio = 0f;
59: poisonEndTime = 0f;
60: poisonTickInterval = 0f;
61: poisonTickDamage = 0f;
63: bleedEndTime = 0f;
64: bleedTickInterval = 0f;
65: bleedTickDamage = 0f;
67: freezeVisualEndTime = 0f;
98: if (damageTakenIncreaseRatio > 0f && Time.time >= vulnerabilityEndTime)
100: damageTakenIncreaseRatio = 0f;
101: vulnerabilityEndTime = 0f;
104: return 1f + Mathf.Max(0f, damageTakenIncreaseRatio);
109: if (duration <= 0f)
122: Debug.Log($"[EnemyStatus] Stun applied to {EnemyName} for {duration:0.##}s");
128: if (increaseRatio <= 0f || duration <= 0f)
143: Debug.Log($"[EnemyStatus] Damage taken increased on {EnemyName} by {increaseRatio * 100f:0.#}% for {duration:0.##}s");
149: if (duration <= 0f || tickInterval <= 0f || tickDamage <= 0f)
167: Debug.Log($"[EnemyStatus] Bleed applied to {EnemyName} | {duration:0.#}s / {tickDamage:0.#}dmg per {tickInterval:0.#}s");
173: if (duration <= 0f || tickInterval <= 0f || tickDamage <= 0f)
191: Debug.Log($"Poison applied to {EnemyName} for {duration:0.##}s");
197: if (slowRatio <= 0f || duration <= 0f || enemy == null || enemy.Stats == null)
204: ratio = Mathf.Clamp01(slowRatio),
220: Debug.Log($"[EnemyStatus] Slow applied to {EnemyName} for {duration:0.##}s ({slowRatio * 100f:0.#}%)");
228: float interval = Mathf.Max(0.05f, poisonTickInterval);
245: poisonEndTime = 0f;
253: float interval = Mathf.Max(0.05f, bleedTickInterval);
270: bleedEndTime = 0f;
276: while (moveSpeedSlows.Count > 0)
279: for (int i = 0; i < moveSpeedSlows.Count; i++)
284: float waitTime = Mathf.Max(0.02f, nextEndTime - Time.time);
291: freezeVisualEndTime = 0f;
298: for (int i = moveSpeedSlows.Count - 1; i >= 0; i--)
313: float strongestSlow = 0f;
314: for (int i = 0; i < moveSpeedSlows.Count; i++)
319: if (strongestSlow > 0f)
370: private static readonly Color PoisonColor = new Color(0.22f, 0.95f, 0.3f, 0.56f);
371: private static readonly Color FreezeColor = new Color(0.62f, 0.86f, 1f, 1f);
372: private static readonly Color BleedColor = new Color(1f, 0.12f, 0.16f, 0.56f);
398: poisonOverlay = CreateOverlayRenderer("PoisonOverlay", PoisonColor, 0.01f);
403: freezeOverlay = CreateOverlayRenderer("FreezeOverlay", FreezeColor, 0.1f);
408: bleedOverlay = CreateOverlayRenderer("BleedOverlay", BleedColor, 0.03f);
455: for (int i = 0; i < renderers.Length; i++)
472: overlayObject.transform.localPosition = Vector3.zero;
474: overlayObject.transform.localScale = Vector3.one;
480: renderer.sortingOrder = sourceRenderer.sortingOrder + 1;
481: overlayObject.transform.localScale = Vector3.one * (1f + scaleOffset);
495: overlay.sortingOrder = sourceRenderer.sortingOrder + 1;
```

## Assets/_Project/Scripts/Gameplay/Enemies/VFX/SplitVfxFadeOut.cs

```csharp
18: elapsed = 0f;
30: float t = Mathf.Clamp01(elapsed / fadeDuration);
32: c.a = Mathf.Lerp(0.9f, 0f, t);
35: if (t >= 1f)
```

## Assets/_Project/Scripts/Gameplay/Items/AcidPuddle.cs

```csharp
10: private const int MaxActivePuddles = 2;
23: public static AcidPuddle Spawn(Vector3 position, float tickDamage, float duration, float radius, float tickInterval)
28: AcidPuddle oldestPuddle = ActivePuddles[0];
29: ActivePuddles.RemoveAt(0);
44: puddleObject.transform.position = new Vector3(position.x, position.y + 0.05f, position.z);
58: tickDamage = Mathf.Max(0.1f, damage);
59: radius = Mathf.Max(0.2f, areaRadius);
60: tickInterval = Mathf.Max(0.05f, interval);
61: lifeDuration = Mathf.Max(0.1f, duration);
63: remainingTicks = Mathf.Max(1, Mathf.RoundToInt(lifeDuration / tickInterval));
67: transform.localScale = Vector3.one * TextureSpriteCache.GetUniformScaleForWorldSize(
69: Mathf.Max(0.2f, radius * 2f * 0.85f));
74: if (remainingTicks <= 0)
95: if (enemies == null || enemies.Count == 0)
100: Vector3 origin = transform.position;
103: for (int i = 0; i < enemies.Count; i++)
111: Vector3 toEnemy = enemy.transform.position - origin;
112: toEnemy.y = 0f;
138: ? new Color(1f, 1f, 1f, 0.72f)
139: : new Color(0.32f, 0.95f, 0.28f, 0.42f);
140: visualRenderer.sortingOrder = 1200;
150: float elapsed = Mathf.Clamp01((Time.time - startTime) / Mathf.Max(0.01f, lifeDuration));
151: float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.05f;
154: Mathf.Max(0.2f, radius * 2f * 0.85f));
155: transform.localScale = Vector3.one * effectScale * pulse;
157: color.a = Mathf.Lerp(0.72f, 0.08f, elapsed);
171: for (int i = ActivePuddles.Count - 1; i >= 0; i--)
```

## Assets/_Project/Scripts/Gameplay/Items/PlayerItemCombatEffects.cs

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
304: private Vector3 previousPosition;
313: private float unstableCoreCurrentMultiplier = 1f;
424: private Vector3 grotesqueGrowthBaseScale = Vector3.one;
438: private readonly Collider[] parasiticBombHitBuffer = new Collider[32];
504: public int BeamHitBufferSize => Mathf.Max(1, beamHitBufferSize);
505: public float BeamRadius => Mathf.Max(0.05f, beamRadius);
506: public float BeamDamageMultiplier => Mathf.Max(0.05f, beamDamageMultiplier);
507: public float CellProliferationDamageMultiplier => Mathf.Max(0.05f, cellProliferationDamageMultiplier);
654: plateletMembraneCurrentShield = 0f;
655: plateletMembraneNextReadyTime = Time.time + Mathf.Max(0.1f, plateletMembraneInterval);
659: if (plateletMembraneCurrentShield <= 0f && Time.time >= plateletMembraneNextReadyTime)
661: plateletMembraneCurrentShield = Mathf.Max(0f, plateletMembraneShieldAmount);
667: recoveryFactorNextHealTime = Time.time + Mathf.Max(0.1f, recoveryFactorInterval);
671: playerStats.Heal(Mathf.Max(0f, recoveryFactorHealAmount));
672: recoveryFactorNextHealTime = Time.time + Mathf.Max(0.1f, recoveryFactorInterval);
675: bool isMovingNow = (playerController != null && playerController.IsMoving) || isMovingByPosition || movementIntensity > 0.02f;
678: bioBarrierIdleTime = 0f;
696: return 3;
701: return 2;
704: return 1;
709: if (projectileCount >= 3)
711: return Mathf.Max(0f, tripleShotSpreadAngle);
714: if (projectileCount == 2)
716: return Mathf.Max(0f, doubleShotSpreadAngle);
719: return 0f;
724: if (projectileCount >= 3)
726: return Mathf.Clamp(tripleShotDamageMultiplier, 0.05f, 1f);
729: if (projectileCount == 2)
731: return Mathf.Clamp(doubleShotDamageMultiplier, 0.05f, 1f);
734: return 1f;
739: return Mathf.Max(0.05f, backShotDamageMultiplier);
749: return HasItem(HypertrophyCellId) ? Mathf.Max(0.1f, hypertrophyRangeMultiplier) : 1f;
754: return HasItem(HypertrophyCellId) ? Mathf.Max(0.1f, hypertrophyScaleMultiplier) : 1f;
759: return HasPiercingMucus ? Mathf.Max(1, piercingHitCount) : 1;
764: if (!HasPiercingMucus || previousHitCount <= 0)
766: return 1f;
769: float falloff = Mathf.Clamp(piercingSubsequentHitDamageMultiplier, 0.05f, 1f);
775: return HasVascularReflection ? Mathf.Max(1, reflectionBounceCount) : 0;
780: return Mathf.Max(0.5f, boomerangReturnDistance);
790: return Mathf.Clamp(boomerangRepeatHitDamageMultiplier, 0.05f, 1f);
795: return Mathf.Max(1, boomerangMaxHitsPerPass);
800: return Mathf.Max(0f, homingTurnRate);
805: return Mathf.Max(0.5f, homingSearchRadius);
810: return Mathf.Max(1.2f, pulseGrowEndScale);
815: return Mathf.Clamp(pulseShrinkEndScale, 0.08f, 0.9f);
820: return Mathf.Max(90f, splitAngle);
825: return Mathf.Max(0.05f, splitDamageMultiplier);
830: return Mathf.Max(0.05f, splitRangeMultiplier);
835: return Mathf.Max(0.2f, explosionRadius);
840: return Mathf.Max(0.05f, explosionDamageMultiplier);
846: ? Mathf.Clamp(explosiveBloodCellRangedDamageMultiplier, 0.05f, 1f)
847: : 1f;
852: float speedMultiplier = 1f;
854: if (HasOverheatedOrgan && overheatStacks > 0)
856: float bonus = 1f + Mathf.Max(0f, overheatPerStackAttackSpeedBonus) * overheatStacks;
860: if (HasBloodPressureBurst && playerStats != null && playerStats.MaxHealth > 0f)
862: float healthRatio = Mathf.Clamp01(playerStats.CurrentHealth / playerStats.MaxHealth);
863: int missingTenPercentSteps = Mathf.Clamp(Mathf.FloorToInt((1f - healthRatio) * 10f + 0.0001f), 0, 10);
865: Mathf.Max(0f, bloodPressureBaseAttackSpeedAdd)
866: + Mathf.Max(0f, bloodPressurePerTenPercentMissingAdd) * missingTenPercentSteps;
867: float burstMultiplier = 1f + additiveAttackSpeed;
873: speedMultiplier *= Mathf.Clamp(muscleSpasmAttackSpeedMultiplier, 0.25f, 1f);
876: if (HasBloodflowAcceleration && IsBossOrEliteWithinRadius(Mathf.Max(0.1f, bloodflowAccelerationRadius)))
878: speedMultiplier *= 1f + Mathf.Max(0f, bloodflowAccelerationAttackSpeedBonus);
883: speedMultiplier *= 1f + Mathf.Max(0f, berserkCellAttackSpeedBonus);
886: return 1f / Mathf.Max(0.05f, speedMultiplier);
891: float multiplier = 1f;
895: multiplier *= Mathf.Clamp(organTentacleBaseAttackPenaltyMultiplier, 0.2f, 1f);
898: return Mathf.Max(0.05f, multiplier);
903: float bonus = 0f;
906: bonus += Mathf.Max(0f, mutantEyeFlatDamageBonus);
911: bonus += Mathf.Max(0, rampageAttackBonus);
914: if (HasHyperplasiaHeart && playerStats != null && playerStats.MaxHealth > 0f)
916: float healthRatio = Mathf.Clamp01(playerStats.CurrentHealth / playerStats.MaxHealth);
917: bonus += (1f - healthRatio) * Mathf.Max(0f, hyperplasiaMissingHealthAttackBonusMax);
922: bonus += Mathf.Max(0, decayOrganAttackBonus);
925: if (HasRuptureMuscle && ruptureMuscleStacks > 0)
927: int effectiveRuptureStacks = Mathf.FloorToInt(Mathf.Max(0f, ruptureMuscleStacks));
928: bonus += Mathf.Max(0f, ruptureMuscleAttackBonusPerStack) * effectiveRuptureStacks;
933: bonus += Mathf.Max(0f, severanceReflexFlatAttackBonus);
946: return HasMuscleSpasm ? Mathf.Max(1f, muscleSpasmMeleeRangeMultiplier) : 1f;
951: return HasMutantEye ? Mathf.Max(0f, mutantEyeAccuracyPenaltyAngle) : 0f;
958: unstableCoreCurrentMultiplier = 1f;
960: return 1f;
965: float attackPower = playerStats != null ? Mathf.Max(0.01f, playerStats.AttackPower) : 1f;
966: float minDamage = attackPower * Mathf.Max(0f, unstableCoreMinAttackRatio);
973: unstableCoreNextRerollTime = Time.time + Mathf.Max(0.1f, unstableCoreRerollInterval);
982: damageMultiplier = 1f;
985: return 1f;
988: float minSpeed = Mathf.Max(0.05f, unstableCellMinProjectileSpeedMultiplier);
991: if (speedMultiplier <= Mathf.Max(0.01f, unstableCellSlowThreshold))
993: damageMultiplier = Mathf.Max(1f, unstableCellSlowDamageMultiplier);
999: public void NotifyBasicAttackPerformed(float attackDamage, LayerMask mask, float range, Vector3 forwardDirection)
1005: if (now - overheatLastAttackTime <= Mathf.Max(0.1f, overheatStackWindow))
1007: overheatStacks = Mathf.Clamp(overheatStacks + 1, 1, Mathf.Max(1, overheatMaxStacks));
1011: overheatStacks = 1;
1015: overheatNextDecayTime = now + Mathf.Max(0.25f, overheatDecayInterval);
1017: if (overheatStacks >= Mathf.Max(1, overheatMaxStacks))
1020: overheatStacks = 0;
1026: if (HasVoidCell && UnityEngine.Random.value <= Mathf.Clamp01(voidCellChance))
1033: ruptureMuscleStacks = Mathf.Clamp(ruptureMuscleStacks + 0.5f, 0f, Mathf.Max(1f, ruptureMuscleMaxStacks));
1040: float multiplier = 1f;
1043: multiplier *= Mathf.Max(1f, bloodContractIncomingDamageMultiplier);
1048: float reduction = Mathf.Clamp01(exoskeletonDamageReductionRatio);
1049: multiplier *= 1f - reduction;
1054: int steps = Mathf.FloorToInt(bioBarrierIdleTime / Mathf.Max(0.1f, bioBarrierIdleSecondsPerStep));
1055: float reduction = Mathf.Clamp(steps * Mathf.Max(0f, bioBarrierReductionPerStep), 0f, Mathf.Clamp01(bioBarrierMaxReduction));
1056: multiplier *= 1f - reduction;
1059: return Mathf.Max(0.01f, multiplier);
1064: float damage = Mathf.Max(0f, damageAmount);
1065: if (damage <= 0f)
1067: return 0f;
1071: damage = Mathf.Max(0f, damage);
1073: if (HasPlateletMembrane && plateletMembraneCurrentShield > 0f)
1078: if (plateletMembraneCurrentShield <= 0f)
1080: plateletMembraneCurrentShield = 0f;
1081: plateletMembraneNextReadyTime = Time.time + Mathf.Max(0.1f, plateletMembraneInterval);
1085: if (HasReflectiveSkin && sourceEnemy != null && !sourceEnemy.IsDead && damage > 0f)
1087: float reflectedDamage = damage * Mathf.Max(0f, reflectiveSkinDamageRatio);
1088: if (reflectedDamage > 0f)
1094: return Mathf.Max(0f, damage);
1099: reviveHealth = 0f;
1106: reviveHealth = Mathf.Clamp(Mathf.Max(0f, splitRegenerationReviveHealth), 0f, Mathf.Max(0f, maxHealth));
1107: if (reviveHealth <= 0f)
1109: reviveHealth = Mathf.Max(1f, Mathf.Min(2f, maxHealth));
1112: return reviveHealth > 0f;
1129: int maxHealthGains = Mathf.Max(1, bloodContractMaxHealthGainCount);
1132: bloodContractKillProgress = 0;
1137: int killsPerHeal = Mathf.Max(1, bloodContractKillsPerHeal);
1143: bloodContractKillProgress = 0;
1144: float gain = Mathf.Max(0f, bloodContractHealthGainAmount);
1145: if (gain <= 0f)
1161: float adjustedDamage = Mathf.Max(0f, damage);
1167: if (HasHeartSniper && IsEnemyAboveHealthRatio(enemy, Mathf.Clamp01(heartSniperHealthThreshold)))
1169: adjustedDamage *= 1f + Mathf.Max(0f, heartSniperDamageBonusRatio);
1179: int maxStacks = Mathf.Max(1, bioResonanceMaxStacks);
1180: float window = Mathf.Max(0.1f, bioResonanceStackWindow);
1186: Stacks = 0,
1191: int effectiveStacks = 0;
1194: effectiveStacks = Mathf.Clamp(state.Stacks, 0, maxStacks);
1197: float stackBonus = Mathf.Max(0f, bioResonanceStackDamageBonus) * effectiveStacks;
1198: adjustedDamage *= 1f + stackBonus;
1200: state.Stacks = Mathf.Clamp(effectiveStacks + 1, 1, maxStacks);
1214: if (!IsEnemyBelowHealthRatio(enemy, Mathf.Clamp01(executionInstinctHealthThreshold)))
1219: if (UnityEngine.Random.value > Mathf.Clamp01(executionInstinctChance))
1226: float bonusMultiplier = Mathf.Max(1f, executionInstinctBossDamageMultiplier) - 1f;
1227: float bonusDamage = Mathf.Max(0f, appliedDamage) * bonusMultiplier;
1228: if (bonusDamage > 0f)
1236: if (remainingHealth > 0f)
1265: Vector3 hitPosition)
1275: float tickDamage = Mathf.Max(0.1f, acidTickDamage);
1288: float tickDamage = Mathf.Max(0.05f, hitDamage * poisonTickDamageRatio);
1299: float tickDamage = Mathf.Max(0.05f, hitDamage * bleedTickDamageRatio);
1304: public void SpawnSplitProjectiles(Vector3 origin, Vector3 forwardDirection, float damage, LayerMask mask, float range)
1306: Vector3 forward = forwardDirection.sqrMagnitude > 0.0001f
1308: : Vector3.forward;
1313: SpawnChildProjectile(origin, Quaternion.Euler(0f, angle, 0f) * forward, sideDamage, mask, sideRange, Projectile.SpawnKind.SplitChild);
1314: SpawnChildProjectile(origin, Quaternion.Euler(0f, -angle, 0f) * forward, sideDamage, mask, sideRange, Projectile.SpawnKind.SplitChild);
1317: public void TrySpawnMutantEyeSplitProjectiles(Vector3 origin, Vector3 forwardDirection, float damage, LayerMask mask, float range)
1322: public void NotifyMutantEyeFireVisual(Vector3 origin, Vector3 direction, float penaltyAngle)
1327: public void NotifyMeleeAttackVisual(Vector3 center, Vector3 direction, float width, float depth)
1332: public void NotifyUnstableCoreRollVisual(float multiplier, Vector3 origin)
1337: private void SpawnChildProjectile(Vector3 origin, Vector3 direction, float damage, LayerMask mask, float range, Projectile.SpawnKind spawnKind)
1401: for (int i = 0; i < acquiredItems.Count; i++)
1433: target.bloodContractKillProgress = Mathf.Max(0, bloodContractKillProgress);
1434: target.bloodContractHealthGainCount = Mathf.Max(0, bloodContractHealthGainCount);
1442: target.decayOrganElapsedSeconds = decayOrganStartTime <= float.NegativeInfinity * 0.5f
1443: ? 0f
1444: : Mathf.Max(0f, Time.time - decayOrganStartTime);
1448: target.plateletMembraneShield = Mathf.Max(0f, plateletMembraneCurrentShield);
1449: target.plateletMembraneCooldownRemaining = Mathf.Max(0f, plateletMembraneNextReadyTime - Time.time);
1453: target.recoveryFactorCooldownRemaining = Mathf.Max(0f, recoveryFactorNextHealTime - Time.time);
1459: bloodContractKillProgress = 0;
1460: bloodContractHealthGainCount = 0;
1463: plateletMembraneCurrentShield = 0f;
1464: plateletMembraneNextReadyTime = Time.time + Mathf.Max(0.1f, plateletMembraneInterval);
1465: recoveryFactorNextHealTime = Time.time + Mathf.Max(0.1f, recoveryFactorInterval);
1470: for (int i = 0; i < savedItems.Count; i++)
1483: 0,
1484: Mathf.Max(1, bloodContractKillsPerHeal) - 1);
1487: 0,
1488: Mathf.Max(1, bloodContractMaxHealthGainCount));
1498: decayOrganStartTime = Time.time - Mathf.Max(0f, state.decayOrganElapsedSeconds);
1505: 0f,
1506: Mathf.Max(0f, plateletMembraneShieldAmount));
1508: Time.time + Mathf.Max(0f, state.plateletMembraneCooldownRemaining);
1514: Time.time + Mathf.Max(0f, state.recoveryFactorCooldownRemaining);
1520: * Mathf.Max(0f, bloodContractHealthGainAmount);
1521: if (hasBloodContractItem && restoredBloodContractHealth > 0f && playerStats != null)
1541: hasAnyCachedItems = acquiredItemIds.Count > 0;
1600: || activeBioSummons.Count > 0;
1604: || macrophageStacks > 0
1605: || gluttonousOrganStacks > 0;
1614: hasDecayStateItems = hasOverheatedOrganItem || hasBioResonanceItem || resonanceStatesByEnemyId.Count > 0 || overheatStacks > 0;
1623: plateletMembraneCurrentShield = 0f;
1624: plateletMembraneNextReadyTime = now + Mathf.Max(0.1f, plateletMembraneInterval);
1630: recoveryFactorNextHealTime = now + Mathf.Max(0.1f, recoveryFactorInterval);
1635: bioBarrierIdleTime = 0f;
1645: bloodContractKillProgress = 0;
1646: bloodContractHealthGainCount = 0;
1659: movementIntensity = 0f;
1664: rampageMoveAccumulatedTime = 0f;
1665: rampageIdleAccumulatedTime = 0f;
1666: rampageAttackBonus = 0;
1672: decayOrganAttackBonus = 0;
1677: ruptureMuscleStacks = 0f;
1679: ApplyRuptureMovePenalty(0f);
1684: imperfectRegenPendingHeal = 0f;
1691: nextSporeSpawnTime = now + Mathf.Max(0.1f, sporeSpawnInterval);
1706: tentacleNextBindTime = now + Mathf.Max(0.1f, tentacleBindInterval);
1709: if (!hasMacrophageItem && macrophageStacks > 0)
1711: macrophageStacks = 0;
1716: if (!hasGluttonousOrganItem && gluttonousOrganStacks > 0)
1718: gluttonousOrganStacks = 0;
1726: focusedNerveNextScanTime = 0f;
1732: berserkCellLastTriggeredBossId = 0;
1738: grotesqueGrowthNextRollTime = 0f;
1744: mutationRampageNextRollTime = 0f;
1755: overheatStacks = 0;
1767: unstableCoreCurrentMultiplier = 1f;
1769: unstableCoreNextRerollTime = 0f;
1810: Vector3 chainOrigin = defeatedEnemy.transform.position;
1811: Vector3 previousPosition = chainOrigin;
1812: int maxHits = Mathf.Max(1, electricChainMaxHits);
1813: float radius = Mathf.Max(0.1f, electricChainRadius);
1814: float damage = Mathf.Max(0f, electricChainDamage);
1815: if (damage <= 0f)
1820: for (int i = 0; i < maxHits; i++)
1829: Vector3 targetPosition = target.transform.position;
1835: if (tempEnemyTargets.Count > 0)
1837: electricChainNextReadyTime = Time.time + Mathf.Max(0f, electricChainCooldown);
1854: int maxTargets = Mathf.Max(1, infectionTransferMaxTargets);
1855: Vector3 center = defeatedEnemy.transform.position;
1856: for (int i = 0; i < maxTargets; i++)
1858: EnemyController target = FindClosestEnemy(center, Mathf.Max(0.1f, infectionTransferRadius), tempEnemyTargets);
1867: for (int i = 0; i < tempEnemyTargets.Count; i++)
1877: Mathf.Max(0.1f, infectionTransferDuration),
1878: Mathf.Max(0.1f, infectionTransferTickInterval),
1879: Mathf.Max(0f, infectionTransferTickDamage));
1883: if (tempEnemyTargets.Count > 0)
1885: infectionTransferNextReadyTime = Time.time + Mathf.Max(0f, infectionTransferCooldown);
1896: macrophageStacks = Mathf.Clamp(macrophageStacks + 1, 1, Mathf.Max(1, macrophageMaxStacks));
1897: macrophageExpireTime = Time.time + Mathf.Max(0.1f, macrophageDuration);
1902: Mathf.Max(0f, macrophageAttackBonusPerStack) * macrophageStacks);
1912: gluttonousOrganStacks = Mathf.Clamp(gluttonousOrganStacks + 1, 1, Mathf.Max(1, gluttonousOrganMaxStacks));
1913: gluttonousOrganExpireTime = Time.time + Mathf.Max(0.1f, gluttonousOrganDuration);
1918: Mathf.Max(0f, gluttonousOrganMoveBonusPerStack) * gluttonousOrganStacks);
1928: if (UnityEngine.Random.value > Mathf.Clamp01(parasiticBombChance))
1933: Vector3 center = playerController != null ? playerController.transform.position : transform.position;
1934: float radius = Mathf.Max(0.1f, parasiticBombRadius);
1935: float damage = Mathf.Max(0.1f, playerStats.AttackPower * Mathf.Max(0f, parasiticBombDamageMultiplier));
1940: ~0,
1947: for (int i = 0; i < hitCount; i++)
1971: if ((!hasMacrophageItem || Time.time >= macrophageExpireTime) && macrophageStacks > 0)
1973: macrophageStacks = 0;
1978: if ((!hasGluttonousOrganItem || Time.time >= gluttonousOrganExpireTime) && gluttonousOrganStacks > 0)
1980: gluttonousOrganStacks = 0;
2010: grotesqueGrowthNextRollTime = 0f;
2019: grotesqueGrowthNextRollTime = Time.time + Mathf.Max(0.1f, grotesqueGrowthInterval);
2020: bool rollSmall = UnityEngine.Random.value < 0.5f;
2026: transform.localScale = grotesqueGrowthBaseScale * Mathf.Max(0.1f, grotesqueGrowthSmallScale);
2029: Mathf.Max(0f, grotesqueGrowthSmallMoveBonus),
2035: transform.localScale = grotesqueGrowthBaseScale * Mathf.Max(0.1f, grotesqueGrowthLargeScale);
2038: Mathf.Max(0f, grotesqueGrowthLargeAttackBonus),
2043: -Mathf.Max(0f, grotesqueGrowthLargeMovePenalty),
2068: mutationRampageNextRollTime = 0f;
2082: mutationRampageNextRollTime = Time.time + Mathf.Max(0.1f, mutationRampageInterval);
2083: mutationRampageExpireTime = Time.time + Mathf.Max(0.1f, mutationRampageDuration);
2095: bool isBuff = UnityEngine.Random.value <= Mathf.Clamp01(mutationRampageBuffChance);
2096: int statRoll = UnityEngine.Random.Range(0, 3);
2100: if (statRoll == 0)
2103: value = isBuff ? Mathf.Max(0f, mutationRampageAttackBuff) : -Mathf.Max(0f, mutationRampageAttackDebuff);
2105: else if (statRoll == 1)
2108: value = isBuff ? Mathf.Max(0f, mutationRampageMoveBuff) : -Mathf.Max(0f, mutationRampageMoveDebuff);
2113: value = isBuff ? Mathf.Max(0f, mutationRampageAttackSpeedBuff) : -Mathf.Max(0f, mutationRampageAttackSpeedDebuff);
2159: int statRoll = UnityEngine.Random.Range(0, 3);
2160: CharacterStatType statType = statRoll == 0
2162: : statRoll == 1 ? CharacterStatType.MoveSpeed : CharacterStatType.AttackSpeed;
2163: float minBonus = Mathf.Max(0f, frenzyHormoneMinBonus);
2168: frenzyHormoneExpireTime = Time.time + Mathf.Max(0.1f, frenzyHormoneDuration);
2169: frenzyHormoneCooldownUntil = Time.time + Mathf.Max(0f, frenzyHormoneCooldown);
2188: focusedNerveNextScanTime = 0f;
2197: focusedNerveNextScanTime = Time.time + 0.2f;
2198: focusedNerveEnemyCount = CountEnemiesWithinRadius(transform.position, Mathf.Max(0.1f, focusedNerveRadius));
2206: berserkCellLastTriggeredBossId = 0;
2210: EnemyController boss = FindNearestBossLikeEnemy(Mathf.Max(0.1f, berserkCellBossDetectionRadius));
2223: berserkCellActiveBossId = 0;
2234: if (focusedNerveEnemyCount <= 1)
2236: return Mathf.Max(0f, focusedNerveHighAttackBonus);
2239: if (focusedNerveEnemyCount <= 3)
2241: return Mathf.Max(0f, focusedNerveLowAttackBonus);
2244: return 0f;
2257: Mathf.Max(0f, berserkCellAttackBonus),
2262: Mathf.Max(0f, berserkCellMoveBonus),
2265: berserkCellExpireTime = Time.time + Mathf.Max(0.1f, berserkCellDuration);
2294: appliedValue = Mathf.Max(0f, value);
2295: if (appliedValue <= 0f)
2314: appliedValue = 0f;
2317: private void SpawnElectricChainVisual(Vector3 start, Vector3 end)
2321: start + Vector3.up * 0.45f,
2322: end + Vector3.up * 0.45f,
2325: 0.24f,
2326: 0.18f,
2327: 0.18f,
2331: private void SpawnInfectionTransferVisual(Vector3 start, Vector3 end)
2335: start + Vector3.up * 0.35f,
2336: end + Vector3.up * 0.35f,
2339: 0.2f,
2340: 0.14f,
2341: 0.25f,
2345: private void SpawnParasiticBombVisual(Vector3 center, float radius)
2350: new Vector3(center.x, center.y + 1.35f, center.z),
2351: Mathf.Max(0.2f, radius * 2f),
2353: 1100,
2354: 0.22f,
2360: Vector3 start,
2361: Vector3 end,
2368: int sortingOrder = 5230)
2385: line.positionCount = 2;
2387: line.SetPosition(0, start);
2388: line.SetPosition(1, end);
2389: line.startWidth = Mathf.Max(0.01f, startWidth);
2390: line.endWidth = Mathf.Max(0.01f, endWidth);
2396: line.numCapVertices = 0;
2401: RuntimePool.EnsureAutoReturn(lineObject)?.Schedule(Mathf.Max(0.02f, duration));
2408: nextSporeSpawnTime = Time.time + Mathf.Max(0.1f, sporeSpawnInterval);
2417: nextSporeSpawnTime = Time.time + Mathf.Max(0.1f, sporeSpawnInterval);
2505: tentacleNextBindTime = Time.time + Mathf.Max(0.1f, tentacleBindInterval);
2514: tentacleNextBindTime = Time.time + Mathf.Max(0.1f, tentacleBindInterval);
2525: if (CountBioSummons(PlayerBioSummon.SummonKind.InfectedHost) >= Mathf.Max(1, infectedHostMaxAllies))
2530: if (UnityEngine.Random.value > Mathf.Clamp01(infectedHostChance))
2537: Vector3 visualScale = FindEnemyVisualScale(defeatedEnemy, Vector3.one * 0.9f);
2538: float damage = GetCompanionBaseDamage() * Mathf.Max(0.05f, infectedHostDamageMultiplier);
2545: new Color(1f, 0.28f, 0.22f, 0.9f),
2547: Mathf.Max(0.1f, infectedHostLifetime),
2548: Mathf.Max(0.1f, infectedHostSearchRadius),
2549: Mathf.Max(0.1f, infectedHostAttackRadius),
2550: Mathf.Max(0.1f, infectedHostAttackInterval),
2556: Vector2 offset2D = UnityEngine.Random.insideUnitCircle;
2557: if (offset2D.sqrMagnitude <= 0.0001f)
2559: offset2D = Vector2.right;
2562: Vector3 desiredPosition = transform.position + new Vector3(offset2D.x, 0f, offset2D.y).normalized * 1.2f;
2563: Vector3 spawnPosition = ResolveGroundSpawnPosition(desiredPosition, 0.15f);
2564: float damage = GetCompanionBaseDamage() * Mathf.Max(0.05f, sporeDamageMultiplier);
2576: Vector3.one * TextureSpriteCache.GetUniformScaleForWorldSize(sporeSprite, 0.65f),
2579: Mathf.Max(0.1f, sporeLifetime),
2580: Mathf.Max(0.1f, sporeSearchRadius),
2581: Mathf.Max(0.1f, sporeBurstRadius),
2582: 0.1f,
2588: Vector3 position,
2591: Vector3 visualScale,
2616: int targetCount = Mathf.Max(1, tentacleMaxTargets);
2617: Vector3 center = transform.position;
2619: for (int i = 0; i < targetCount; i++)
2621: EnemyController enemy = FindClosestEnemy(center, Mathf.Max(0.1f, tentacleBindRadius), tempEnemyTargets);
2630: float damage = GetCompanionBaseDamage() * Mathf.Max(0f, tentacleDamageMultiplier);
2631: for (int i = 0; i < tempEnemyTargets.Count; i++)
2640: status?.ApplyMoveSpeedSlow(Mathf.Clamp01(tentacleSlowRatio), Mathf.Max(0.1f, tentacleBindDuration));
2641: if (damage > 0f)
2646: SpawnTentacleBindVisual(enemy.transform.position, Mathf.Max(0.1f, tentacleBindDuration));
2650: private void SpawnTentacleBindVisual(Vector3 targetPosition, float duration)
2652: Vector3 anchorPosition = GetPlayerVisualCenter() + Vector3.up * 0.15f;
2657: targetPosition + Vector3.up * 0.4f,
2658: 1.45f,
2660: 5225,
2667: targetPosition + Vector3.up * 0.35f,
2670: 0.32f,
2671: 0.2f,
2674: 4900);
2680: for (int i = activeBioSummons.Count - 1; i >= 0; i--)
2692: int count = 0;
2693: for (int i = 0; i < activeBioSummons.Count; i++)
2706: for (int i = activeBioSummons.Count - 1; i >= 0; i--)
2738: return playerStats != null ? Mathf.Max(0.1f, playerStats.AttackPower) : 1f;
2741: private EnemyController FindClosestEnemy(Vector3 center, float radius, List<EnemyController> excluded = null)
2744: if (enemies == null || enemies.Count == 0)
2749: float radiusSqr = Mathf.Max(0.1f, radius);
2754: for (int i = 0; i < enemies.Count; i++)
2762: Vector3 toEnemy = enemy.transform.position - center;
2763: toEnemy.y = 0f;
2777: private int CountEnemiesWithinRadius(Vector3 center, float radius)
2780: if (enemies == null || enemies.Count == 0)
2782: return 0;
2785: float radiusSqr = Mathf.Max(0.1f, radius);
2787: int count = 0;
2788: for (int i = 0; i < enemies.Count; i++)
2796: Vector3 toEnemy = enemy.transform.position - center;
2797: toEnemy.y = 0f;
2810: if (enemies == null || enemies.Count == 0)
2815: float radiusSqr = Mathf.Max(0.1f, radius);
2817: Vector3 center = transform.position;
2818: for (int i = 0; i < enemies.Count; i++)
2826: Vector3 toEnemy = enemy.transform.position - center;
2827: toEnemy.y = 0f;
2840: if (enemies == null || enemies.Count == 0)
2845: Vector3 center = PlayerController.Instance != null ? PlayerController.Instance.transform.position : Vector3.zero;
2846: float radiusSqr = Mathf.Max(0.1f, radius);
2850: for (int i = 0; i < enemies.Count; i++)
2858: Vector3 toEnemy = enemy.transform.position - center;
2859: toEnemy.y = 0f;
2876: if (stats == null || stats.MaxHealth <= 0f)
2887: if (stats == null || stats.MaxHealth <= 0f)
2898: return stats != null ? Mathf.Max(0f, stats.CurrentHealth) : 0f;
2929: for (int i = 0; i < renderers.Length; i++)
2951: if (config.attackSprites != null && config.attackSprites.Length > 0)
2956: if (config.attackSpritesDown != null && config.attackSpritesDown.Length > 0)
2961: if (config.attackSpritesUp != null && config.attackSpritesUp.Length > 0)
2987: Vector3 position,
3014: visualObject.transform.localScale = Vector3.one * TextureSpriteCache.GetUniformScaleForWorldSize(
3016: Mathf.Max(0.01f, scale));
3018: RuntimePool.EnsureAutoReturn(visualObject)?.Schedule(Mathf.Max(0.02f, duration));
3034: line.positionCount = 2;
3111: private static Vector3 FindEnemyVisualScale(EnemyController enemy, Vector3 fallback)
3119: for (int i = 0; i < renderers.Length; i++)
3133: private Vector3 ResolveGroundSpawnPosition(Vector3 desiredPosition, float heightOffset)
3138: Vector3 fallback = desiredPosition;
3143: Vector2Int desiredGrid = biome.WorldToGrid(desiredPosition);
3149: Vector2Int playerGrid = biome.WorldToGrid(transform.position);
3150: for (int radius = 1; radius <= 3; radius++)
3213: private Vector3 GetMovementAnchorPosition()
3221: Vector3 currentPosition = GetMovementAnchorPosition();
3222: Vector3 delta = currentPosition - previousPosition;
3225: delta.y = 0f;
3226: float planarSpeed = Time.deltaTime > 0.0001f ? delta.magnitude / Time.deltaTime : 0f;
3227: isMovingByPosition = planarSpeed > 0.05f;
3228: float target = Mathf.Clamp01(planarSpeed / 2f);
3229: movementIntensity = Mathf.MoveTowards(movementIntensity, target, Time.deltaTime * 5f);
3236: rampageMoveAccumulatedTime = 0f;
3237: rampageIdleAccumulatedTime = 0f;
3238: rampageAttackBonus = 0;
3242: bool isMovingNow = (playerController != null && playerController.IsMoving) || isMovingByPosition || movementIntensity > 0.02f;
3245: rampageIdleAccumulatedTime = 0f;
3246: if (rampageAttackBonus >= Mathf.Max(0, rampageMaxAttackBonus))
3252: float secondsPerBonus = Mathf.Max(0.1f, rampageMoveSecondsPerBonus);
3253: while (rampageMoveAccumulatedTime >= secondsPerBonus && rampageAttackBonus < Mathf.Max(0, rampageMaxAttackBonus))
3262: if (rampageAttackBonus <= 0)
3268: float secondsPerDecay = Mathf.Max(0.1f, rampageIdleSecondsPerDecay);
3269: while (rampageIdleAccumulatedTime >= secondsPerDecay && rampageAttackBonus > 0)
3288: tentacleNextAutoAttackTime = Time.time + Mathf.Max(0.15f, organTentacleAutoAttackInterval);
3291: if (enemies == null || enemies.Count == 0)
3296: Vector3 center = transform.position;
3297: float radiusSqr = Mathf.Max(0.5f, organTentacleAutoAttackRadius);
3299: float damage = Mathf.Max(0.1f, playerStats.AttackPower * Mathf.Max(0.05f, organTentacleAutoAttackDamageMultiplier));
3300: int hitLimit = Mathf.Max(1, organTentacleMaxTargets);
3301: int hitCount = 0;
3303: for (int i = 0; i < enemies.Count && hitCount < hitLimit; i++)
3311: Vector3 toEnemy = enemy.transform.position - center;
3312: toEnemy.y = 0f;
3322: Vector3 tentacleStart = GetPlayerVisualCenter();
3323: Vector3 tentacleEnd = enemy.transform.position + Vector3.up * 0.4f;
3324: Vector3 tentacleMidpoint = (tentacleStart + tentacleEnd) * 0.5f;
3325: Vector3 extendedHalfLength = (tentacleEnd - tentacleStart) * 0.625f;
3332: 0.52f,
3333: 0.24f,
3334: 0.2f,
3345: if (overheatStacks > 0 && now >= overheatNextDecayTime)
3347: overheatStacks = 0;
3354: overheatStacks = 0;
3365: if (resonanceStatesByEnemyId.Count == 0)
3370: float cleanupThreshold = Mathf.Max(0.2f, bioResonanceStackWindow * 4f);
3380: for (int i = 0; i < tempResonanceRemovalIds.Count; i++)
3391: decayOrganAttackBonus = 0;
3395: if (decayOrganStartTime <= float.NegativeInfinity * 0.5f)
3398: decayOrganAttackBonus = 0;
3402: float elapsed = Mathf.Max(0f, Time.time - decayOrganStartTime);
3403: float secondsPerBonus = Mathf.Max(0.1f, decayOrganSecondsPerAttackBonus);
3405: decayOrganAttackBonus = Mathf.Clamp(bonus, 0, Mathf.Max(0, decayOrganMaxAttackBonus));
3412: ruptureMuscleStacks = 0;
3414: ApplyRuptureMovePenalty(0f);
3418: float decayDelay = Mathf.Max(0.25f, ruptureMuscleDecayDelay);
3419: if (ruptureMuscleStacks > 0 && Time.time - ruptureMuscleLastAttackTime > decayDelay)
3421: ruptureMuscleStacks = Mathf.Max(0f, ruptureMuscleStacks - 1f);
3425: int effectiveRuptureStacks = Mathf.FloorToInt(Mathf.Max(0f, ruptureMuscleStacks));
3426: float targetPenalty = Mathf.Max(0f, ruptureMuscleMovePenaltyPerStack) * effectiveRuptureStacks;
3434: imperfectRegenPendingHeal = 0f;
3440: if (imperfectRegenPendingHeal <= 0f || Time.time < imperfectRegenHealReadyTime)
3446: imperfectRegenPendingHeal = 0f;
3449: imperfectRegenCooldownUntil = Time.time + Mathf.Max(0f, imperfectRegenCooldownDuration);
3466: ruptureAppliedMovePenalty = Mathf.Max(0f, penalty);
3467: if (ruptureAppliedMovePenalty > 0f)
3488: new CharacterStatModifier(CharacterStatType.MaxHealth, -Mathf.Max(0f, forbiddenGrowthMaxHealthPenalty), CharacterStatModifierMode.Flat, forbiddenGrowthModifierSource),
3489: new CharacterStatModifier(CharacterStatType.AttackPower, Mathf.Max(0f, forbiddenGrowthFlatAttackBonus), CharacterStatModifierMode.Flat, forbiddenGrowthModifierSource));
3495: new CharacterStatModifier(CharacterStatType.MaxHealth, -Mathf.Max(0f, overclockNerveMaxHealthPenalty), CharacterStatModifierMode.Flat, overclockNerveModifierSource),
3496: new CharacterStatModifier(CharacterStatType.MoveSpeed, Mathf.Max(0f, overclockNerveFlatMoveSpeedBonus), CharacterStatModifierMode.Flat, overclockNerveModifierSource));
3502: new CharacterStatModifier(CharacterStatType.MaxHealth, -Mathf.Max(0f, imperfectRegenMaxHealthPenalty), CharacterStatModifierMode.Flat, imperfectRegenModifierSource));
3508: new CharacterStatModifier(CharacterStatType.MoveSpeed, -Mathf.Max(0f, exoskeletonMoveSpeedPenalty), CharacterStatModifierMode.Flat, exoskeletonModifierSource));
3588: ruptureAppliedMovePenalty = 0f;
3589: macrophageStacks = 0;
3590: macrophageAppliedAttackBonus = 0f;
3592: gluttonousOrganStacks = 0;
3593: gluttonousOrganAppliedMoveBonus = 0f;
3597: berserkCellActiveBossId = 0;
3634: float damageTaken = Mathf.Max(0f, args.PreviousValue - args.CurrentValue);
3635: if (damageTaken <= 0f)
3643: canScheduleRegen &= !(imperfectRegenPendingHeal > 0f && Time.time < imperfectRegenHealReadyTime);
3646: imperfectRegenPendingHeal = Mathf.Max(0f, imperfectRegenHealPerTrigger);
3647: imperfectRegenHealReadyTime = Time.time + Mathf.Max(0.1f, imperfectRegenDelay);
3653: severanceReflexBuffUntil = Time.time + Mathf.Max(0.1f, severanceReflexDuration);
3663: Vector3 center = transform.position;
3664: float radius = Mathf.Max(0.1f, overheatExplosionRadius);
3667: 0.1f,
3668: attackDamage * Mathf.Max(0.05f, overheatExplosionDamageMultiplier));
3672: for (int i = 0; i < enemies.Count; i++)
3680: Vector3 toEnemy = enemy.transform.position - center;
3681: toEnemy.y = 0f;
3694: float selfDamage = Mathf.Max(0.1f, overheatExplosionSelfDamage);
3698: private void SpawnVoidCellProjectile(float attackDamage, LayerMask mask, float range, Vector3 fallbackDirection)
3700: EnemyController target = FindRandomEnemyWithinRadius(12f);
3706: Vector3 targetPos = target.transform.position;
3707: Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * Mathf.Max(0.3f, voidCellSpawnRadius);
3708: Vector3 spawnPos = targetPos + new Vector3(randomCircle.x, 0f, randomCircle.y);
3709: spawnPos.y = transform.position.y + 0.2f;
3711: Vector3 shootDir = targetPos - spawnPos;
3712: shootDir.y = 0f;
3713: if (shootDir.sqrMagnitude <= 0.0001f)
3715: shootDir = fallbackDirection.sqrMagnitude > 0.0001f ? fallbackDirection.normalized : Vector3.forward;
3721: Mathf.Max(0.05f, attackDamage * Mathf.Max(0.05f, voidCellDamageMultiplier)),
3723: Mathf.Max(0.25f, range * 0.6f),
3730: if (enemies == null || enemies.Count == 0)
3736: Vector3 center = PlayerController.Instance != null ? PlayerController.Instance.transform.position : Vector3.zero;
3737: float radiusSqr = Mathf.Max(0.5f, radius);
3740: for (int i = 0; i < enemies.Count; i++)
3748: Vector3 toEnemy = enemy.transform.position - center;
3749: toEnemy.y = 0f;
3758: if (randomEnemyCandidates.Count == 0)
3763: int index = UnityEngine.Random.Range(0, randomEnemyCandidates.Count);
3799: unstableCoreOverlay.sortingOrder = playerVisualSpriteRenderer.sortingOrder + 2;
3803: float minMultiplier = Mathf.Max(0f, unstableCoreMinAttackRatio);
3809: Color lowColor = new Color(0.38f, 0.78f, 1f, 0.88f);
3810: Color highColor = new Color(1f, 0.22f, 0.2f, 0.88f);
3812: unstableCoreOverlay.transform.localScale = Vector3.one * 1.12f;
3817: if (!hasPlateletMembraneItem || plateletMembraneCurrentShield <= 0f)
3845: plateletMembraneOutlineMaterial.SetColor(OutlineColorId, new Color(0.72f, 0.74f, 0.76f, 1f));
3846: plateletMembraneOutlineMaterial.SetFloat(OutlineSizeId, 1.25f);
3847: plateletMembraneOutlineMaterial.SetFloat(OutlineExpandId, 0f);
3856: plateletMembraneOutlineRenderer.sortingOrder = playerVisualSpriteRenderer.sortingOrder - 1;
3868: playerVisualSpriteRenderer.bounds.size.y) * 1.35f;
3869: Vector3 parentScale = playerVisualSpriteRenderer.transform.lossyScale;
3871: 0.0001f,
3874: plateletMembraneEffectRenderer.transform.localScale = Vector3.one * (worldScale / parentUniformScale);
3876: plateletMembraneEffectRenderer.sortingOrder = playerVisualSpriteRenderer.sortingOrder + 2;
3942: outlineObject.transform.localPosition = Vector3.zero;
3944: outlineObject.transform.localScale = Vector3.one;
3964: effectObject.transform.localPosition = Vector3.zero;
3980: for (int i = sourceTransform.childCount - 1; i >= 0; i--)
4023: overlayObject.transform.localPosition = Vector3.zero;
4025: overlayObject.transform.localScale = Vector3.one;
4031: unstableCoreOverlay.sortingOrder = sourceRenderer.sortingOrder + 2;
4044: for (int i = 0; i < playerSpriteRendererBuffer.Count; i++)
4067: Vector3 center = GetPlayerVisualCenter();
4072: Mathf.Max(0.2f, overheatExplosionRadius * 2f),
4074: 5300,
4075: 0.2f,
4079: private Vector3 GetPlayerVisualCenter()
4087: return transform.position + Vector3.up * 0.6f;
4122: Vector3 visualScale,
4133: this.damage = Mathf.Max(0f, damage);
4134: this.searchRadius = Mathf.Max(0.1f, searchRadius);
4135: this.attackRadius = Mathf.Max(0.1f, attackRadius);
4136: this.attackInterval = Mathf.Max(0.05f, attackInterval);
4140: nextAttackTime = 0f;
4141: attackVisualEndTime = 0f;
4142: attackVisualFrameTime = 0f;
4143: moveSpeed = kind == SummonKind.Spore ? 5.2f : 3.2f;
4144: expireTime = Time.time + Mathf.Max(0.1f, lifetime);
4153: spriteRenderer.sortingOrder = kind == SummonKind.Spore ? 5100 : 5050;
4188: markerObject.transform.localPosition = Vector3.zero;
4192: infectionMarkerRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
4196: 0.0001f,
4198: float markerScale = TextureSpriteCache.GetUniformScaleForWorldSize(markerSprite, 0.48f);
4199: markerObject.transform.localScale = Vector3.one * (markerScale / parentScale);
4222: Vector3 toTarget = target.transform.position - transform.position;
4223: toTarget.y = 0f;
4227: Vector3 step = toTarget.normalized * moveSpeed * Time.deltaTime;
4263: if (attackSprites == null || attackSprites.Length == 0)
4268: attackVisualEndTime = Time.time + 0.35f;
4270: spriteRenderer.sprite = attackSprites[0] != null ? attackSprites[0] : idleSprite;
4275: if (attackSprites == null || attackSprites.Length == 0 || Time.time >= attackVisualEndTime)
4285: float frameDuration = 0.35f / Mathf.Max(1, attackSprites.Length);
4292: Mathf.FloorToInt((0.35f - Mathf.Max(0f, attackVisualEndTime - Time.time)) / frameDuration),
4293: 0,
4294: attackSprites.Length - 1);
4309: Vector3 home = owner.transform.position;
4310: Vector3 toHome = home - transform.position;
4311: toHome.y = 0f;
4312: if (toHome.sqrMagnitude <= 1.8f * 1.8f)
4317: transform.position += toHome.normalized * moveSpeed * 0.5f * Time.deltaTime;
4326: 0.9f,
4327: new Color(0.52f, 1f, 0.45f, 0.45f),
4328: 5150,
4329: 0.18f,
4360: angle = 0f;
4361: nextFireTime = 0f;
4370: spriteRenderer.color = droneSprite != null ? Color.white : new Color(0.95f, 0.08f, 0.18f, 0.92f);
4371: spriteRenderer.sortingOrder = 5300;
4373: transform.localScale = Vector3.one * TextureSpriteCache.GetUniformScaleForWorldSize(spriteRenderer.sprite, 0.68f);
4390: angle += Time.deltaTime * 170f;
4391: float radians = angle * Mathf.Deg2Rad;
4392: Vector3 offset = new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)) * Mathf.Max(0.2f, owner.bloodDroneOrbitRadius);
4393: transform.position = owner.transform.position + offset + Vector3.up * 0.45f;
4400: EnemyController target = owner.FindClosestEnemy(owner.transform.position, Mathf.Max(0.1f, owner.bloodDroneTargetRadius));
4406: nextFireTime = Time.time + Mathf.Max(0.1f, owner.bloodDroneFireInterval);
4407: float damage = owner.GetCompanionBaseDamage() * Mathf.Max(0.05f, owner.bloodDroneDamageMultiplier);
4408: PlayerBioProjectile.Spawn(transform.position, target, damage, new Color(1f, 0.05f, 0.16f, 0.95f));
4432: private Vector3 cameraRight = Vector3.right;
4433: private Vector3 cameraUp = Vector3.up;
4438: angle = 0f;
4439: nextBlockTime = 0f;
4448: spriteRenderer.color = guardianSprite != null ? Color.white : new Color(0.68f, 0.82f, 1f, 0.95f);
4449: spriteRenderer.sortingOrder = 5350;
4451: transform.localScale = Vector3.one * TextureSpriteCache.GetUniformScaleForWorldSize(spriteRenderer.sprite, 0.82f);
4468: angle += Time.deltaTime * 260f;
4469: float radians = angle * Mathf.Deg2Rad;
4470: Vector3 center = owner.GetPlayerVisualCenter();
4471: Vector3 offset = (cameraRight * Mathf.Cos(radians) + cameraUp * Mathf.Sin(radians)) * Mathf.Max(0.2f, owner.guardianOrganOrbitRadius);
4475: : new Color(0.45f, 0.55f, 0.7f, 0.5f);
4487: float blockRadius = Mathf.Max(0.1f, owner.guardianOrganBlockRadius);
4490: for (int i = projectiles.Count - 1; i >= 0; i--)
4498: Vector3 toProjectile = projectile.transform.position - transform.position;
4499: toProjectile.y = 0f;
4506: nextBlockTime = Time.time + Mathf.Max(0.1f, owner.guardianOrganCooldown);
4518: 1.2f,
4519: new Color(0.68f, 0.82f, 1f, 0.35f),
4520: 5360,
4521: 0.16f);
4535: cameraRight = Vector3.right;
4536: cameraUp = Vector3.up;
4555: public static void Spawn(Vector3 position, EnemyController target, float damage, Color color)
4577: this.damage = Mathf.Max(0f, damage);
4578: speed = 11f;
4579: expireTime = Time.time + 1.6f;
4587: spriteRenderer.sortingOrder = 5320;
4589: transform.localScale = Vector3.one * 0.22f;
4605: Vector3 toTarget = target.transform.position - transform.position;
4606: toTarget.y = 0f;
4607: if (toTarget.sqrMagnitude <= 0.45f * 0.45f)
```

## Assets/_Project/Scripts/Gameplay/Items/PlayerItemInventoryUI.cs

```csharp
21: private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.85f);
22: private static readonly Color PanelFallbackColor = new Color(0.19f, 0.075f, 0.11f, 0.98f);
24: private static readonly Color EmptyCardColor = new Color(0.52f, 0.42f, 0.46f, 0.82f);
25: private static readonly Color AccentColor = new Color(0.47f, 0.11f, 0.15f, 1f);
26: private static readonly Color MainTextColor = new Color(0.93f, 0.81f, 0.76f, 1f);
27: private static readonly Color MutedTextColor = new Color(0.72f, 0.56f, 0.57f, 1f);
30: [SerializeField] private int sortingOrder = 210;
40: private float previousTimeScale = 1f;
148: Time.timeScale = 0f;
186: scaler.referenceResolution = new Vector2(1920f, 1080f);
187: scaler.matchWidthOrHeight = 0.5f;
196: panelRect.anchorMin = new Vector2(0.5f, 0.5f);
197: panelRect.anchorMax = new Vector2(0.5f, 0.5f);
198: panelRect.pivot = new Vector2(0.5f, 0.5f);
199: panelRect.sizeDelta = new Vector2(1600f, 760f);
208: panelOutline.effectColor = new Color(0.53f, 0.31f, 0.32f, 0.95f);
209: panelOutline.effectDistance = new Vector2(3f, -3f);
211: Text fallbackTitle = CreateText("Title", panel.transform, 44, FontStyle.Bold, TextAnchor.MiddleCenter);
212: SetRect(fallbackTitle.rectTransform, new Vector2(0f, 320f), new Vector2(900f, 64f));
220: new Vector2(650f, 270f),
221: new Vector2(52f, 46f),
223: new Color(0.32f, 0.085f, 0.12f, 0.96f));
228: SetRect(cardsRect, new Vector2(0f, -22f), new Vector2(1190f, 520f));
230: layout.spacing = 34f;
238: Text hint = CreateText("Hint", panel.transform, 22, FontStyle.Normal, TextAnchor.MiddleCenter);
239: SetRect(hint.rectTransform, new Vector2(0f, -326f), new Vector2(1100f, 40f));
253: for (int i = cardContainer.childCount - 1; i >= 0; i--)
261: int slotCount = Mathf.Max(1, itemManager.MaxItemSlots);
262: float cardWidth = Mathf.Clamp((1190f - 34f * (slotCount - 1)) / slotCount, 285f, 360f);
264: for (int i = 0; i < slotCount; i++)
279: GameObject card = CreateCardRoot($"Item_{slotIndex + 1}", width, CardColor);
281: Text name = CreateText("Name", card.transform, 29, FontStyle.Bold, TextAnchor.MiddleCenter);
282: SetRect(name.rectTransform, new Vector2(0f, 174f), new Vector2(width - 52f, 54f));
288: SetRect(iconFrameRect, new Vector2(0f, 66f), new Vector2(142f, 142f));
290: iconFrameImage.color = new Color(0.12f, 0.035f, 0.055f, 0.9f);
292: iconOutline.effectColor = new Color(0.48f, 0.27f, 0.27f, 0.95f);
293: iconOutline.effectDistance = new Vector2(2f, -2f);
299: iconRect.anchorMin = new Vector2(0.5f, 0.5f);
300: iconRect.anchorMax = new Vector2(0.5f, 0.5f);
301: iconRect.sizeDelta = new Vector2(124f, 124f);
308: Text noIcon = CreateText("NoIcon", iconFrame.transform, 18, FontStyle.Normal, TextAnchor.MiddleCenter);
314: Text description = CreateText("Description", card.transform, 21, FontStyle.Normal, TextAnchor.UpperCenter);
315: SetRect(description.rectTransform, new Vector2(0f, -78f), new Vector2(width - 62f, 100f));
323: new Vector2(0f, -194f),
324: new Vector2(width - 92f, 48f),
332: GameObject card = CreateCardRoot($"Empty_{slotIndex + 1}", width, EmptyCardColor);
334: Text empty = CreateText("Empty", card.transform, 27, FontStyle.Normal, TextAnchor.MiddleCenter);
335: SetRect(empty.rectTransform, Vector2.zero, new Vector2(width - 40f, 120f));
344: rect.sizeDelta = new Vector2(width, 510f);
347: layoutElement.preferredHeight = 510f;
350: image.color = cardSprite != null ? color : new Color(0.22f, 0.08f, 0.11f, color.a);
356: outline.effectColor = new Color(0.56f, 0.33f, 0.31f, 0.9f);
357: outline.effectDistance = new Vector2(2f, -2f);
370: Vector2 position,
371: Vector2 size,
384: colors.highlightedColor = new Color(1f, 0.72f, 0.72f, 1f);
385: colors.pressedColor = new Color(0.72f, 0.52f, 0.52f, 1f);
387: colors.colorMultiplier = 1f;
391: outline.effectColor = new Color(0.66f, 0.36f, 0.34f, 0.95f);
392: outline.effectDistance = new Vector2(2f, -2f);
394: Text text = CreateText("Label", buttonObject.transform, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
413: shadow.effectColor = new Color(0.08f, 0.005f, 0.012f, 0.95f);
414: shadow.effectDistance = new Vector2(2f, -2f);
425: private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
427: rect.anchorMin = new Vector2(0.5f, 0.5f);
428: rect.anchorMax = new Vector2(0.5f, 0.5f);
429: rect.pivot = new Vector2(0.5f, 0.5f);
436: rect.anchorMin = Vector2.zero;
437: rect.anchorMax = Vector2.one;
438: rect.offsetMin = Vector2.zero;
439: rect.offsetMax = Vector2.zero;
```

## Assets/_Project/Scripts/Gameplay/Items/PlayerItemManager.cs

```csharp
9: None = 0,
140: [SerializeField, Min(1)] private int maxItemSlots = 3;
178: int slotOverride = DifficultyBalanceService.ActiveProfile?.items?.maxSlotsOverride ?? 0;
179: if (slotOverride > 0)
209: if (startingItemIds == null || startingItemIds.Count == 0)
214: for (int i = 0; i < startingItemIds.Count && !IsFull; i++)
222: if (maxItemSlots < 1)
224: maxItemSlots = 1;
227: if (!Application.isPlaying && autoPopulateBasicProjectileItems && (itemEntries == null || itemEntries.Count == 0))
248: new PlayerItemEntry("double_core", "이중핵", "발사체 2개 발사 (각 75% 피해)", PlayerItemCategory.BasicProjectile),
249: new PlayerItemEntry("triple_core", "삼중핵", "발사체 3개 발사 (각 55% 피해)", PlayerItemCategory.BasicProjectile),
251: new PlayerItemEntry("hypertrophy_cell", "비대 세포", "투사체 크기·충돌 범위 45%, 사거리 25% 증가", PlayerItemCategory.BasicProjectile),
252: new PlayerItemEntry("reflux_organ", "역류 장기", "왕복 경로마다 최대 3명 타격, 복귀 중 같은 적 재적중 시 75% 피해", PlayerItemCategory.BasicProjectile),
253: new PlayerItemEntry("piercing_mucus", "관통 점액", "최대 3명 관통, 관통할 때마다 피해 25% 감소", PlayerItemCategory.BasicProjectile),
255: new PlayerItemEntry("beam_organ", "광선 기관", "80% 피해의 관통 광선 공격 (다중핵·후방 발사·세포 증식 적용)", PlayerItemCategory.BasicProjectile),
256: new PlayerItemEntry("split_tissue", "분열 조직", "적 명중 시 양옆으로 각각 50% 피해의 추가 투사체 생성", PlayerItemCategory.BasicProjectile),
257: new PlayerItemEntry("explosive_blood_cell", "폭발 혈구", "원거리 피해가 50% 감소하고 범위 폭발 공격으로 변경", PlayerItemCategory.BasicProjectile),
258: new PlayerItemEntry("acidic_rupture", "산성 파열", "적중 위치에 4초간 1초마다 0.5 피해를 주는 산성 장판 생성 (동시에 최대 2개)", PlayerItemCategory.BasicProjectile),
262: new PlayerItemEntry("toxic_mucosa", "독성 점막", "적중 시 4초간 1초마다 적중 피해의 20% 중독 피해", PlayerItemCategory.BasicProjectile),
263: new PlayerItemEntry("freezing_nerve", "빙결 신경", "공격 적중 시 2초간 적 이동속도 30% 감소", PlayerItemCategory.BasicProjectile),
264: new PlayerItemEntry("hemorrhage_organ", "출혈 기관", "적중 시 3.5초간 0.8초마다 적중 피해의 18% 출혈 피해", PlayerItemCategory.BasicProjectile),
265: new PlayerItemEntry("overheated_organ", "과열 기관", "연속 공격마다 공격속도 +10% (10중첩 시 주변 150% 피해, 자신 1 피해)", PlayerItemCategory.BasicProjectile),
266: new PlayerItemEntry("mutant_eye", "돌연변이 안구", "발사 방향이 좌우 40도 내에서 무작위 변화, 공격력 +2", PlayerItemCategory.BasicProjectile),
267: new PlayerItemEntry("organ_tentacle", "장기 촉수", "주변 적 최대 2명을 1.2초마다 30% 피해로 공격, 기본 공격 피해 20% 감소", PlayerItemCategory.BasicProjectile),
268: new PlayerItemEntry("rampage_bloodflow", "폭주 혈류", "이동 8초마다 공격력 +1 (최대 +2), 정지 시 4초마다 감소", PlayerItemCategory.BasicProjectile),
269: new PlayerItemEntry("muscle_spasm", "근육 경련", "근접 공격 범위 1.7배, 공격속도 20% 감소", PlayerItemCategory.BasicProjectile),
270: new PlayerItemEntry("unstable_core", "불안정 핵", "5초마다 기본 공격 피해가 60~160% 사이에서 변경", PlayerItemCategory.BasicProjectile),
271: new PlayerItemEntry("bio_resonance", "생체 공명", "3초 내 같은 적 연속 공격 시 피해 +15% (최대 +45%)", PlayerItemCategory.BasicProjectile),
272: new PlayerItemEntry("blood_pressure_burst", "혈압 폭발", "잃은 체력 10%당 공격속도 +6% (최대 +60%)", PlayerItemCategory.BasicProjectile),
273: new PlayerItemEntry("void_cell", "공허 세포", "공격 시 20% 확률로 적 주변에서 85% 피해의 추가 탄환 생성", PlayerItemCategory.BasicProjectile),
274: new PlayerItemEntry("forbidden_growth", "금단 성장", "체력 -3, 공격력 +2 증가", PlayerItemCategory.BasicProjectile),
275: new PlayerItemEntry("overclock_nerve", "과속 신경", "체력 -2, 이동속도 +2 증가", PlayerItemCategory.BasicProjectile),
276: new PlayerItemEntry("blood_contract", "피의 계약", "적 20명 처치 시 체력 +1 (최대 +5), 받는 피해 1.3배", PlayerItemCategory.BasicProjectile),
277: new PlayerItemEntry("hyperplasia_heart", "과증식 심장", "잃은 체력에 비례해 공격력 증가 (최대 +2)", PlayerItemCategory.BasicProjectile),
278: new PlayerItemEntry("decay_organ", "부패 장기", "3분마다 공격력 +1 (최대 +2)", PlayerItemCategory.BasicProjectile),
279: new PlayerItemEntry("rupture_muscle", "파열 근육", "공격 2회마다 공격력 +0.5·이동속도 -0.25 (최대 3중첩)", PlayerItemCategory.BasicProjectile),
280: new PlayerItemEntry("imperfect_regeneration", "불완전 재생", "체력 -2, 피격 2초 후 체력 +1 회복 (10초 쿨타임)", PlayerItemCategory.BasicProjectile),
281: new PlayerItemEntry("severance_reflex", "절단 반사", "피격 후 2초간 공격력 +2", PlayerItemCategory.BasicProjectile),
282: new PlayerItemEntry("bio_gamble", "생체 도박", "레벨업 선택 시 성장량이 0~+2 범위에서 무작위 결정", PlayerItemCategory.BasicProjectile),
283: new PlayerItemEntry("exoskeleton", "외골격", "받는 피해 25% 감소, 대신 이동속도 -1.5", PlayerItemCategory.BasicProjectile),
284: new PlayerItemEntry("platelet_membrane", "혈소판 막", "20초마다 피해 1을 막는 보호막 생성", PlayerItemCategory.BasicProjectile),
285: new PlayerItemEntry("recovery_factor", "회복 인자", "30초마다 체력 +1 회복", PlayerItemCategory.BasicProjectile),
286: new PlayerItemEntry("reflective_skin", "반사 피부", "피격 시 실제 받은 피해의 75% 반사", PlayerItemCategory.BasicProjectile),
287: new PlayerItemEntry("bio_barrier", "생체 장막", "정지 1.5초마다 받는 피해 10% 감소 (최대 40%)", PlayerItemCategory.BasicProjectile),
288: new PlayerItemEntry("split_regeneration", "분열 재생", "체력 0이 될 때 작은 체력으로 부활", PlayerItemCategory.BasicProjectile),
289: new PlayerItemEntry("infected_host", "감염 숙주", "잡몹 처치 시 15% 확률로 6초간 35% 피해의 아군 생성 (최대 2명)", PlayerItemCategory.BasicProjectile),
291: new PlayerItemEntry("blood_drone", "혈액 드론", "1초마다 가까운 적에게 공격력의 30% 피해", PlayerItemCategory.BasicProjectile),
292: new PlayerItemEntry("guardian_organ", "수호 장기", "플레이어 주변을 맴돌며 3.5초마다 적 투사체 하나 방어", PlayerItemCategory.BasicProjectile),
293: new PlayerItemEntry("tentacle_colony", "촉수 군체", "3초마다 적 2명에게 1.2초간 60% 둔화와 10% 피해", PlayerItemCategory.BasicProjectile),
294: new PlayerItemEntry("electric_neural_network", "전기 신경망", "적 처치 시 최대 3명에게 피해 1의 연쇄 전류 (5초 쿨타임)", PlayerItemCategory.BasicProjectile),
295: new PlayerItemEntry("infection_transference", "감염 전이", "적 처치 시 주변 3명에게 3초간 초당 0.5 감염 피해 (5초 쿨타임)", PlayerItemCategory.BasicProjectile),
296: new PlayerItemEntry("macrophage", "대식 세포", "적 처치 시 5초간 공격력 +0.5 (최대 3중첩)", PlayerItemCategory.BasicProjectile),
297: new PlayerItemEntry("gluttonous_organ", "폭식 장기", "적 처치 시 3초 동안 이동속도 증가 (최대 4스택)", PlayerItemCategory.BasicProjectile),
298: new PlayerItemEntry("heart_sniper", "심장 저격", "체력 60% 이상인 적에게 직접 공격 피해 35% 증가", PlayerItemCategory.BasicProjectile),
299: new PlayerItemEntry("bloodflow_acceleration", "혈류 가속", "보스·엘리트 근처에서 공격속도 35% 증가", PlayerItemCategory.BasicProjectile),
300: new PlayerItemEntry("focused_nerve", "집중 신경", "주변 적 1명 이하일 때 공격력 +0.75, 2~3명일 때 +0.5", PlayerItemCategory.BasicProjectile),
301: new PlayerItemEntry("execution_instinct", "처형 본능", "체력 20% 미만 적을 20% 확률로 처형 (보스·엘리트는 추가 피해 50%)", PlayerItemCategory.BasicProjectile),
302: new PlayerItemEntry("berserk_cell", "광폭 세포", "보스전 진입 시 15초간 공격력 +0.75, 이동속도 +0.5, 공격속도 +30%", PlayerItemCategory.BasicProjectile),
303: new PlayerItemEntry("unstable_cell", "불안정 세포", "투사체 속도 50~150%, 80% 이하의 느린 탄은 피해 150%", PlayerItemCategory.BasicProjectile),
304: new PlayerItemEntry("grotesque_growth", "기괴 성장", "10초마다 소형화·이동속도 +1 또는 대형화·공격력 +1·이동속도 -0.5", PlayerItemCategory.BasicProjectile),
305: new PlayerItemEntry("mutation_rampage", "돌연변이 폭주", "15초마다 7초간 무작위 스탯 버프 또는 디버프", PlayerItemCategory.BasicProjectile),
306: new PlayerItemEntry("parasitic_bomb", "기생 폭탄", "적 처치 시 20% 확률로 반경 4에 공격력 150% 폭발", PlayerItemCategory.BasicProjectile),
307: new PlayerItemEntry("frenzy_hormone", "광란 호르몬", "피격 시 무작위 능력 +0.5~1 (3초 지속, 8초 쿨타임)", PlayerItemCategory.BasicProjectile)
319: for (int i = 0; i < itemEntries.Count; i++)
351: for (int i = 0; i < itemEntries.Count; i++)
363: for (int i = 0; i < templates.Count; i++)
383: if (itemEntries == null || itemEntries.Count == 0)
388: for (int i = itemEntries.Count - 1; i >= 0; i--)
402: if (startingItemIds == null || startingItemIds.Count == 0)
407: for (int i = startingItemIds.Count - 1; i >= 0; i--)
476: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.35f, 0.15f), true));
479: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.16f, 0.14f), false));
482: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.32f, 0.94f, 0.28f), false));
485: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.92f, 0.96f, 0.9f), false));
488: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.52f, 1f, 0.45f), false));
491: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.95f, 0.08f, 0.18f), true));
494: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.68f, 0.82f, 1f), false));
497: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.68f, 0.24f, 0.86f), true));
500: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.45f, 0.82f, 1f), true));
503: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.42f, 0.95f, 0.32f), false));
506: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.34f, 0.18f), false));
509: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.96f, 0.52f, 0.22f), true));
512: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.12f, 0.18f), true));
515: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.95f, 0.05f, 0.08f), true));
518: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.78f, 0.9f, 1f), false));
521: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.96f, 0.86f, 0.2f), true));
524: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.24f, 0.08f), false));
527: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.75f, 0.38f, 1f), true));
530: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.42f, 1f, 0.58f), false));
533: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.42f, 0.95f), true));
536: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.88f, 0.12f, 0.68f), false));
539: entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.72f, 0.12f), true));
551: const int size = 48;
552: Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
556: Color edge = new Color(accentColor.r * 0.35f, accentColor.g * 0.35f, accentColor.b * 0.35f, 1f);
557: Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
558: float radius = size * 0.44f;
560: for (int y = 0; y < size; y++)
562: for (int x = 0; x < size; x++)
564: float distance = Vector2.Distance(new Vector2(x, y), center);
571: float radial = Mathf.InverseLerp(radius, 0f, distance);
575: float stripe = Mathf.Abs((x - center.x) / Mathf.Max(1f, radius));
576: float mask = Mathf.SmoothStep(0.85f, 0.15f, stripe);
577: baseColor = Color.Lerp(baseColor * 0.55f, new Color(1f, 0.93f, 0.75f, 1f), mask);
585: Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
600: entry.SetDescription("발사체 2개 발사 (각 75% 피해)");
604: entry.SetDescription("발사체 3개 발사 (각 55% 피해)");
608: entry.SetDescription("투사체 크기·충돌 범위 45%, 사거리 25% 증가");
612: entry.SetDescription("왕복 경로마다 최대 3명 타격, 복귀 중 같은 적 재적중 시 75% 피해");
616: entry.SetDescription("최대 3명 관통, 관통할 때마다 피해 25% 감소");
620: entry.SetDescription("80% 피해의 관통 광선 공격 (다중핵·후방 발사·세포 증식 적용)");
624: entry.SetDescription("적 명중 시 양옆으로 각각 50% 피해의 추가 투사체 생성");
628: entry.SetDescription("원거리 피해가 50% 감소하고 범위 폭발 공격으로 변경");
632: entry.SetDescription("적중 위치에 4초간 1초마다 0.5 피해를 주는 산성 장판 생성 (동시에 최대 2개)");
636: entry.SetDescription("연속 공격마다 공격속도 +10% (10중첩 시 주변 150% 피해, 자신 1 피해)");
640: entry.SetDescription("발사 방향이 좌우 40도 내에서 무작위 변화, 공격력 +2");
644: entry.SetDescription("근접 공격 범위 1.7배, 공격속도 20% 감소");
648: entry.SetDescription("적중 시 4초간 1초마다 적중 피해의 20% 중독 피해");
652: entry.SetDescription("적중 시 3.5초간 0.8초마다 적중 피해의 18% 출혈 피해");
656: entry.SetDescription("주변 적 최대 2명을 1.2초마다 30% 피해로 공격, 기본 공격 피해 20% 감소");
664: entry.SetDescription("공격 적중 시 2초간 적 이동속도 30% 감소");
668: entry.SetDescription("이동 8초마다 공격력 +1 (최대 +2), 정지 시 4초마다 감소");
672: entry.SetDescription("5초마다 기본 공격 피해가 60~160% 사이에서 변경");
676: entry.SetDescription("공격 시 20% 확률로 적 주변에서 85% 피해의 추가 탄환 생성");
680: entry.SetDescription("3초 내 같은 적 연속 공격 시 피해 +15% (최대 +45%)");
684: entry.SetDescription("잃은 체력 10%당 공격속도 +6% (최대 +60%)");
688: entry.SetDescription("체력 -3, 공격력 +2 증가");
692: entry.SetDescription("체력 -2, 이동속도 +2 증가");
696: entry.SetDescription("적 20명 처치 시 체력 +1 (최대 +5), 받는 피해 1.3배");
700: entry.SetDescription("체력 -2, 피격 2초 후 체력 +1 회복 (10초 쿨타임)");
704: entry.SetDescription("피격 후 2초간 공격력 +2");
708: entry.SetDescription("잃은 체력에 비례해 공격력 증가 (최대 +2)");
712: entry.SetDescription("3분마다 공격력 +1 (최대 +2)");
716: entry.SetDescription("공격 2회마다 공격력 +0.5·이동속도 -0.25 (최대 3중첩)");
720: entry.SetDescription("레벨업 선택 시 성장량이 0~+2 범위에서 무작위 결정");
724: entry.SetDescription("받는 피해 25% 감소, 대신 이동속도 -1.5");
728: entry.SetDescription("20초마다 피해 1을 막는 보호막 생성");
732: entry.SetDescription("30초마다 체력 +1 회복");
736: entry.SetDescription("피격 시 실제 받은 피해의 75% 반사");
740: entry.SetDescription("정지 1.5초마다 받는 피해 10% 감소 (최대 40%)");
744: entry.SetDescription("잡몹 처치 시 15% 확률로 6초간 35% 피해의 아군 생성 (최대 2명)");
752: entry.SetDescription("1초마다 가까운 적에게 공격력의 30% 피해");
756: entry.SetDescription("플레이어 주변을 맴돌며 3.5초마다 적 투사체 하나 방어");
760: entry.SetDescription("3초마다 적 2명에게 1.2초간 60% 둔화와 10% 피해");
764: entry.SetDescription("적 처치 시 최대 3명에게 피해 1의 연쇄 전류 (5초 쿨타임)");
768: entry.SetDescription("적 처치 시 주변 3명에게 3초간 초당 0.5 감염 피해 (5초 쿨타임)");
772: entry.SetDescription("적 처치 시 5초간 공격력 +0.5 (최대 3중첩)");
776: entry.SetDescription("적 처치 시 3초 동안 이동속도 증가 (최대 4스택)");
780: entry.SetDescription("체력 60% 이상인 적에게 직접 공격 피해 35% 증가");
784: entry.SetDescription("보스·엘리트 근처에서 공격속도 35% 증가");
788: entry.SetDescription("주변 적 1명 이하일 때 공격력 +0.75, 2~3명일 때 +0.5");
792: entry.SetDescription("체력 20% 미만 적을 20% 확률로 처형 (보스·엘리트는 추가 피해 50%)");
796: entry.SetDescription("보스전 진입 시 15초간 공격력 +0.75, 이동속도 +0.5, 공격속도 +30%");
800: entry.SetDescription("투사체 속도 50~150%, 80% 이하의 느린 탄은 피해 150%");
804: entry.SetDescription("10초마다 소형화·이동속도 +1 또는 대형화·공격력 +1·이동속도 -0.5");
808: entry.SetDescription("15초마다 7초간 무작위 스탯 버프 또는 디버프");
812: entry.SetDescription("적 처치 시 20% 확률로 반경 4에 공격력 150% 폭발");
816: entry.SetDescription("피격 시 무작위 능력 +0.5~1 (3초 지속, 8초 쿨타임)");
886: for (int i = 0; i < acquiredItems.Count; i++)
909: if (acquiredItems.Count == 0)
915: for (int i = acquiredItems.Count - 1; i >= 0; i--)
926: for (int i = 0; i < removedItems.Count; i++)
936: for (int i = 0; i < acquiredItems.Count; i++)
960: for (int i = 0; i < savedItems.Count; i++)
991: for (int i = 0; i < acquiredItems.Count; i++)
```

## Assets/_Project/Scripts/Gameplay/Items/PlayerItemPickupNotifier.cs

```csharp
11: [SerializeField] private float messageDuration = 2.2f;
12: [SerializeField] private int fontSize = 28;
95: canvas.sortingOrder = 190;
99: scaler.referenceResolution = new Vector2(1920f, 1080f);
107: rect.anchorMin = new Vector2(0.5f, 1f);
108: rect.anchorMax = new Vector2(0.5f, 1f);
109: rect.pivot = new Vector2(0.5f, 1f);
110: rect.anchoredPosition = new Vector2(0f, -70f);
111: rect.sizeDelta = new Vector2(1100f, 80f);
117: messageText.color = new Color(0.95f, 0.95f, 0.25f, 1f);
```

## Assets/_Project/Scripts/Gameplay/Items/PlayerItemTestPanel.cs

```csharp
18: [SerializeField] private KeyCode toggleKey = KeyCode.F8;
19: [SerializeField] private int sortingOrder = 220;
109: scaler.referenceResolution = new Vector2(1920f, 1080f);
110: scaler.matchWidthOrHeight = 0.5f;
116: panelImage.color = new Color(0.06f, 0.1f, 0.14f, 0.92f);
118: panelRect.anchorMin = Vector2.zero;
119: panelRect.anchorMax = Vector2.one;
120: panelRect.pivot = new Vector2(0.5f, 0.5f);
121: panelRect.offsetMin = new Vector2(24f, 24f);
122: panelRect.offsetMax = new Vector2(-24f, -24f);
124: titleText = CreateLabel("Title", panelObject.transform, new Vector2(0f, 470f), new Vector2(1740f, 56f), 36, TextAnchor.MiddleCenter);
126: titleText.color = new Color(0.95f, 0.96f, 0.98f, 1f);
128: summaryText = CreateLabel("Summary", panelObject.transform, new Vector2(0f, 420f), new Vector2(1740f, 42f), 26, TextAnchor.MiddleCenter);
129: summaryText.color = new Color(0.77f, 0.89f, 0.98f, 1f);
131: detailText = CreateLabel("Detail", panelObject.transform, new Vector2(0f, 365f), new Vector2(1740f, 60f), 24, TextAnchor.MiddleCenter);
132: detailText.color = new Color(0.93f, 0.93f, 0.93f, 1f);
134: CreateLabel("CatalogLabel", panelObject.transform, new Vector2(-450f, 315f), new Vector2(840f, 40f), 26, TextAnchor.MiddleLeft).text = "전체 아이템 목록";
135: CreateLabel("AcquiredLabel", panelObject.transform, new Vector2(450f, 315f), new Vector2(840f, 40f), 26, TextAnchor.MiddleLeft).text = "현재 보유 아이템";
137: catalogContent = CreateScrollList(panelObject.transform, new Vector2(-450f, -10f), new Vector2(840f, 620f));
138: acquiredContent = CreateScrollList(panelObject.transform, new Vector2(450f, -10f), new Vector2(840f, 620f));
140: CreateActionButton("RefreshButton", panelObject.transform, new Vector2(-690f, -365f), new Vector2(220f, 68f), "새로고침", RefreshLists, new Color(0.2f, 0.3f, 0.42f, 1f));
141: CreateActionButton("AddButton", panelObject.transform, new Vector2(-430f, -365f), new Vector2(260f, 68f), "선택 아이템 추가", AddSelectedCatalogItem, new Color(0.18f, 0.44f, 0.22f, 1f));
142: CreateActionButton("SpawnButton", panelObject.transform, new Vector2(-120f, -365f), new Vector2(300f, 68f), "플레이어 위치 생성", SpawnSelectedCatalogItemAtPlayer, new Color(0.44f, 0.32f, 0.12f, 1f));
143: CreateActionButton("RemoveButton", panelObject.transform, new Vector2(210f, -365f), new Vector2(260f, 68f), "선택 아이템 삭제", RemoveSelectedAcquiredItem, new Color(0.5f, 0.18f, 0.18f, 1f));
144: CreateActionButton("ClearButton", panelObject.transform, new Vector2(500f, -365f), new Vector2(220f, 68f), "전체 삭제", ClearAllItems, new Color(0.41f, 0.14f, 0.14f, 1f));
145: CreateActionButton("HealButton", panelObject.transform, new Vector2(0f, -445f), new Vector2(340f, 58f), "플레이어 체력 전체 회복", RestorePlayerHealth, new Color(0.12f, 0.48f, 0.4f, 1f));
147: CreateActionButton("CompleteBiomeBossesButton", panelObject.transform, new Vector2(520f, -445f), new Vector2(620f, 58f), "4개 맵 보스 처치 완료 (저장 반영)", CompleteBiomeBosses, new Color(0.38f, 0.24f, 0.53f, 1f));
149: statusText = CreateLabel("Status", panelObject.transform, new Vector2(0f, -500f), new Vector2(1740f, 46f), 24, TextAnchor.MiddleCenter);
150: statusText.color = new Color(0.97f, 0.83f, 0.35f, 1f);
182: case KeyCode.F1: keyControl = keyboard.f1Key; return true;
183: case KeyCode.F2: keyControl = keyboard.f2Key; return true;
184: case KeyCode.F3: keyControl = keyboard.f3Key; return true;
185: case KeyCode.F4: keyControl = keyboard.f4Key; return true;
186: case KeyCode.F5: keyControl = keyboard.f5Key; return true;
187: case KeyCode.F6: keyControl = keyboard.f6Key; return true;
188: case KeyCode.F7: keyControl = keyboard.f7Key; return true;
189: case KeyCode.F8: keyControl = keyboard.f8Key; return true;
190: case KeyCode.F9: keyControl = keyboard.f9Key; return true;
191: case KeyCode.F10: keyControl = keyboard.f10Key; return true;
192: case KeyCode.F11: keyControl = keyboard.f11Key; return true;
193: case KeyCode.F12: keyControl = keyboard.f12Key; return true;
203: private Transform CreateScrollList(Transform parent, Vector2 anchoredPosition, Vector2 size)
207: bg.color = new Color(0f, 0f, 0f, 0.26f);
215: viewportRect.anchorMin = Vector2.zero;
216: viewportRect.anchorMax = Vector2.one;
217: viewportRect.offsetMin = new Vector2(8f, 8f);
218: viewportRect.offsetMax = new Vector2(-8f, -8f);
219: viewport.AddComponent<RectMask2D>();
221: viewportImage.color = new Color(0f, 0f, 0f, 0.16f);
225: contentRect.anchorMin = new Vector2(0f, 1f);
226: contentRect.anchorMax = new Vector2(1f, 1f);
227: contentRect.pivot = new Vector2(0.5f, 1f);
228: contentRect.anchoredPosition = Vector2.zero;
229: contentRect.sizeDelta = new Vector2(0f, 0f);
232: layout.spacing = 6f;
233: layout.padding = new RectOffset(6, 6, 6, 6);
249: scrollRect.scrollSensitivity = 28f;
261: for (int i = catalogContent.childCount - 1; i >= 0; i--)
266: for (int i = acquiredContent.childCount - 1; i >= 0; i--)
272: for (int i = 0; i < entries.Count; i++)
296: for (int i = 0; i < acquired.Count; i++)
304: string label = $"{i + 1}. {item.DisplayName} ({item.ItemId})";
452: UpdateStatus("보스 처치 및 부산물 4/4 완료. F8을 닫고 허브 중앙 포탈로 들어가세요. (현재 저장에 반영)");
472: UpdateStatus($"플레이어 체력 전체 회복: {previousHealth:0.##} → {maxHealth:0.##}");
487: rowRect.sizeDelta = new Vector2(0f, 62f);
491: ? new Color(0.24f, 0.49f, 0.67f, 0.95f)
492: : new Color(0.14f, 0.17f, 0.21f, 0.9f);
497: Text rowText = CreateLabel("Text", row.transform, Vector2.zero, new Vector2(0f, 0f), 22, TextAnchor.MiddleLeft);
499: rowText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
501: textRect.anchorMin = Vector2.zero;
502: textRect.anchorMax = Vector2.one;
503: textRect.offsetMin = new Vector2(10f, 0f);
504: textRect.offsetMax = new Vector2(-10f, 0f);
510: Vector2 anchoredPosition,
511: Vector2 size,
527: Text text = CreateLabel("Label", buttonObject.transform, Vector2.zero, Vector2.zero, 24, TextAnchor.MiddleCenter);
531: textRect.anchorMin = Vector2.zero;
532: textRect.anchorMax = Vector2.one;
533: textRect.offsetMin = Vector2.zero;
534: textRect.offsetMax = Vector2.zero;
540: Vector2 anchoredPosition,
541: Vector2 size,
```

## Assets/_Project/Scripts/Gameplay/Items/WorldItemBoxPickup.cs

```csharp
15: [SerializeField, Min(0.05f)] private float revealDuration = 0.6f;
16: [SerializeField, Min(0f)] private float openedBoxHideDelay = 2f;
17: [SerializeField, Min(0f)] private float itemRestHeight = 0.8f;
18: [SerializeField, Min(0f)] private float itemPopHeight = 1.6f;
19: [SerializeField] private int itemSortingOrderOffset = 1;
49: EnsureSetup(0);
114: float duration = Mathf.Max(0.05f, revealDuration);
115: float halfDuration = duration * 0.5f;
116: Vector3 start = new Vector3(0f, itemRestHeight * 0.15f, 0f);
117: Vector3 peak = new Vector3(0f, itemPopHeight, 0f);
118: Vector3 end = new Vector3(0f, itemRestHeight, 0f);
120: for (float elapsed = 0f; elapsed < halfDuration; elapsed += Time.deltaTime)
122: SetItemLocalPosition(Vector3.Lerp(start, peak, Smooth01(elapsed / halfDuration)));
126: for (float elapsed = 0f; elapsed < halfDuration; elapsed += Time.deltaTime)
128: SetItemLocalPosition(Vector3.Lerp(peak, end, Smooth01(elapsed / halfDuration)));
139: if (boxHideScheduled || openedBoxHideDelay <= 0f)
175: Vector3 effectPosition = itemVisual != null
177: : transform.position + Vector3.up * itemRestHeight;
213: if (boxSortingOrder != 0)
241: SetItemLocalPosition(new Vector3(0f, itemRestHeight * 0.15f, 0f));
244: private void SetItemLocalPosition(Vector3 localPosition)
259: private static float Smooth01(float t)
261: t = Mathf.Clamp01(t);
262: return t * t * (3f - 2f * t);
```

## Assets/_Project/Scripts/Gameplay/Items/WorldItemPickup.cs

```csharp
76: transform.position + Vector3.up * 0.45f,
```

## Assets/_Project/Scripts/Gameplay/Items/WorldItemSpawner.cs

```csharp
15: [SerializeField, Min(1)] private int spawnCount = 3;
16: [SerializeField, Min(1)] private int maxPositionAttemptsPerItem = 220;
17: [SerializeField, Min(0)] private int edgePadding = 10;
18: [SerializeField, Min(0f)] private float minDistanceBetweenItems = 14f;
19: [SerializeField, Min(0f)] private float minDistanceFromPlayer = 10f;
21: [SerializeField, Min(8)] private int spreadSampleCount = 280;
22: [SerializeField] private float itemGroundOffset = 0.15f;
23: [SerializeField, Min(1)] private int intestineSpreadGenerationTries = 14;
24: [SerializeField, Min(16)] private int intestineSpreadSampleCount = 1400;
30: [SerializeField, Min(0.1f)] private float itemBoxTargetHeight = 1.35f;
31: [SerializeField, Min(0.1f)] private float itemBoxTriggerWidth = 1.25f;
32: [SerializeField, Min(0.1f)] private float itemBoxTriggerHeight = 1.35f;
33: [SerializeField] private int sortingOrder = 250;
39: private readonly List<Vector3> spawnedPositions = new List<Vector3>();
57: maxPositionAttemptsPerItem = Mathf.Max(maxPositionAttemptsPerItem, 220);
58: edgePadding = Mathf.Max(edgePadding, 10);
59: minDistanceBetweenItems = Mathf.Max(minDistanceBetweenItems, 14f);
60: minDistanceFromPlayer = Mathf.Max(minDistanceFromPlayer, 10f);
61: spreadSampleCount = Mathf.Max(spreadSampleCount, 280);
65: minDistanceBetweenItems = Mathf.Max(minDistanceBetweenItems, 110f);
66: minDistanceFromPlayer = Mathf.Max(minDistanceFromPlayer, 24f);
67: intestineSpreadGenerationTries = Mathf.Max(intestineSpreadGenerationTries, 14);
68: intestineSpreadSampleCount = Mathf.Max(intestineSpreadSampleCount, 1400);
85: if (itemManager.ItemEntries == null || itemManager.ItemEntries.Count == 0)
91: if (candidates.Count == 0)
110: minDistanceBetweenItems = Mathf.Max(minDistanceBetweenItems, 110f);
111: minDistanceFromPlayer = Mathf.Max(minDistanceFromPlayer, 24f);
112: edgePadding = Mathf.Max(edgePadding, 10);
113: maxPositionAttemptsPerItem = Mathf.Max(maxPositionAttemptsPerItem, 220);
114: intestineSpreadGenerationTries = Mathf.Max(intestineSpreadGenerationTries, 14);
115: intestineSpreadSampleCount = Mathf.Max(intestineSpreadSampleCount, 1400);
119: float spawnMultiplier = DifficultyBalanceService.ActiveProfile?.items?.worldSpawnCount ?? 1f;
121: 0,
122: Mathf.RoundToInt(spawnCount * Mathf.Max(0f, spawnMultiplier)));
126: List<Vector3> positions = null;
137: for (int i = 0; i < actualSpawnCount; i++)
140: Vector3 spawnPosition;
158: public bool TrySpawnSingleRandomItemNear(Vector3 centerWorldPosition, float radius = 2f)
166: if (itemManager.ItemEntries == null || itemManager.ItemEntries.Count == 0)
172: if (candidates.Count == 0)
189: float safeRadius = Mathf.Max(0.5f, radius);
191: for (int i = 0; i < maxPositionAttemptsPerItem; i++)
193: Vector2 offset = Random.insideUnitCircle * safeRadius;
194: Vector3 candidateWorld = new Vector3(centerWorldPosition.x + offset.x, centerWorldPosition.y, centerWorldPosition.z + offset.y);
195: Vector2Int grid = biome.WorldToGrid(candidateWorld);
201: Vector3 spawnPos = biome.GridToWorld(grid.x, grid.y);
209: PlayerItemManager.PlayerItemEntry selected = candidates[Random.Range(0, candidates.Count)];
218: public bool TrySpawnSingleRandomItemAt(Vector3 worldPosition)
226: if (itemManager.ItemEntries == null || itemManager.ItemEntries.Count == 0)
232: if (candidates.Count == 0)
248: Vector2Int grid = biome.WorldToGrid(worldPosition);
251: return TrySpawnSingleRandomItemNear(worldPosition, 2.5f);
254: Vector3 spawnPos = biome.GridToWorld(grid.x, grid.y);
257: PlayerItemManager.PlayerItemEntry selected = candidates[Random.Range(0, candidates.Count)];
265: Vector3 worldPosition,
280: if (itemManager.ItemEntries == null || itemManager.ItemEntries.Count == 0)
296: Vector3 spawnPos = worldPosition;
299: Vector2Int grid = biome.WorldToGrid(worldPosition);
305: else if (!TryFindNearestWalkableSpawnPosition(biome, worldPosition, 2.5f, out spawnPos))
333: for (int i = 0; i < entries.Count; i++)
347: private bool TryFindSpawnPosition(BiomeManager biome, Transform playerTransform, out Vector3 spawnPosition)
349: int minX = Mathf.Clamp(edgePadding, 0, Mathf.Max(0, biome.MapWidth - 1));
350: int minY = Mathf.Clamp(edgePadding, 0, Mathf.Max(0, biome.MapHeight - 1));
351: int maxX = Mathf.Clamp(biome.MapWidth - edgePadding, 1, biome.MapWidth);
352: int maxY = Mathf.Clamp(biome.MapHeight - edgePadding, 1, biome.MapHeight);
354: for (int i = 0; i < maxPositionAttemptsPerItem; i++)
366: if (playerTransform != null && Vector3.Distance(playerTransform.position, spawnPosition) < minDistanceFromPlayer)
383: Vector3 centerWorldPosition,
385: out Vector3 spawnPosition)
387: float safeRadius = Mathf.Max(0.5f, radius);
388: for (int i = 0; i < maxPositionAttemptsPerItem; i++)
390: Vector2 offset = Random.insideUnitCircle * safeRadius;
391: Vector3 candidateWorld = new Vector3(
396: Vector2Int grid = biome.WorldToGrid(candidateWorld);
411: private bool IsTooCloseToExisting(Vector3 position)
414: for (int i = 0; i < spawnedPositions.Count; i++)
425: private List<Vector3> GenerateSpreadPositions(BiomeManager biome, Transform playerTransform, int targetCount)
427: List<Vector3> candidates = BuildCandidatePositions(biome, playerTransform, Mathf.Max(spreadSampleCount, targetCount * 24));
431: private List<Vector3> BuildCandidatePositions(BiomeManager biome, Transform playerTransform, int sampleCount)
433: List<Vector3> result = new List<Vector3>();
434: int minX = Mathf.Clamp(edgePadding, 0, Mathf.Max(0, biome.MapWidth - 1));
435: int minY = Mathf.Clamp(edgePadding, 0, Mathf.Max(0, biome.MapHeight - 1));
436: int maxX = Mathf.Clamp(biome.MapWidth - edgePadding, 1, biome.MapWidth);
437: int maxY = Mathf.Clamp(biome.MapHeight - edgePadding, 1, biome.MapHeight);
439: for (int i = 0; i < sampleCount; i++)
448: Vector3 pos = biome.GridToWorld(x, y);
450: if (playerTransform != null && Vector3.Distance(playerTransform.position, pos) < minDistanceFromPlayer)
461: private static float GetNearestDistance(Vector3 position, List<Vector3> references)
464: for (int i = 0; i < references.Count; i++)
476: private List<Vector3> GenerateIntestineUltraSpreadPositions(BiomeManager biome, Transform playerTransform, int targetCount)
478: List<Vector3> best = new List<Vector3>();
479: if (targetCount <= 0 || biome == null)
484: float requiredDistance = Mathf.Max(minDistanceBetweenItems, Mathf.Min(biome.MapWidth, biome.MapHeight) * 0.34f);
487: for (int attempt = 0; attempt < intestineSpreadGenerationTries; attempt++)
489: List<Vector3> candidates = BuildCandidatePositions(
492: Mathf.Max(intestineSpreadSampleCount, targetCount * 120));
499: List<Vector3> current = BuildUltraSpreadSet(candidates, targetCount);
521: private List<Vector3> BuildUltraSpreadSet(List<Vector3> candidates, int targetCount)
523: List<Vector3> selected = new List<Vector3>();
524: if (candidates == null || candidates.Count == 0 || targetCount <= 0)
529: int firstIndex = Random.Range(0, candidates.Count);
533: while (selected.Count < targetCount && candidates.Count > 0)
535: int bestIndex = -1;
538: for (int i = 0; i < candidates.Count; i++)
548: if (bestIndex < 0)
560: private static float GetMinimumPairwiseDistance(List<Vector3> points)
562: if (points == null || points.Count < 2)
564: return 0f;
568: for (int i = 0; i < points.Count - 1; i++)
570: for (int j = i + 1; j < points.Count; j++)
583: private void SpawnItemObject(PlayerItemManager.PlayerItemEntry entry, Vector3 worldPosition)
592: itemObject.transform.localScale = Vector3.one * ResolveItemBoxScale(closedSprite);
608: marker.transform.localScale = Vector3.one * 0.6f;
619: markerRenderer.material.color = new Color(0.7f, 0.35f, 0.12f, 1f);
625: float itemBoxScale = Mathf.Max(0.001f, itemObject.transform.localScale.x);
626: collider.size = new Vector3(itemBoxTriggerWidth / itemBoxScale, itemBoxTriggerHeight / itemBoxScale, itemBoxTriggerWidth / itemBoxScale);
627: collider.center = new Vector3(0f, (itemBoxTriggerHeight * 0.5f) / itemBoxScale, 0f);
635: if (sprite == null || sprite.bounds.size.y <= 0.0001f)
637: return 1f;
640: return Mathf.Max(0.001f, itemBoxTargetHeight / sprite.bounds.size.y);
706: for (int i = list.Count - 1; i > 0; i--)
708: int randomIndex = Random.Range(0, i + 1);
```

## Assets/_Project/Scripts/Gameplay/Player/Combat/PlayerAttack.cs

```csharp
17: [SerializeField] private float meleeAttackDamage = 20f;
18: [SerializeField] private Vector3 meleeAttackBoxSize = new Vector3(3f, 3f, 3f);
19: [SerializeField] private float meleeAttackOffset = 2f;
20: [SerializeField] private LayerMask meleeTargetMask = ~0;
21: [SerializeField, Min(1)] private int meleeOverlapBufferSize = 24;
26: [SerializeField] private float projectileSpawnOffset = 0.65f;
27: [SerializeField] private float projectileSpawnHeight = 1f;
28: [SerializeField] private float projectileSpawnExtraHeight = 2f;
29: [SerializeField] private float projectileRange = 8f;
30: [SerializeField] private float rangedAttackDamage = 10f;
31: [SerializeField] private LayerMask rangedTargetMask = ~0;
34: [SerializeField, Min(1)] private int beamOverlapBufferSize = 48;
35: [SerializeField] private float beamVerticalHalfHeight = 2.5f;
36: [SerializeField] private float beamHeightOffset = -0.3f;
37: [SerializeField] private float beamVisualDuration = 0.1f;
41: [SerializeField] private float meleeCooldown  = 0.2f;
42: [SerializeField] private float attackCooldown = 0.3f;
43: [SerializeField, Min(0f)] private float cellProliferationDelay = 0.5f;
48: [SerializeField] private float meleeSlashLifetime = 0.18f;
49: [SerializeField] private float meleeSlashScale = 0.95f;
50: [SerializeField] private float meleeSlashHeightOffset = 2f;
51: [SerializeField] private int meleeSlashSortingOrder = 5050;
63: private Texture2D beamTexture;
86: if (meleeTargetMask.value == 0)
88: meleeTargetMask = ~0;
92: if (rangedTargetMask.value == 0)
94: rangedTargetMask = ~0;
172: private Vector3 GetAttackDirection()
177: return Vector3.forward;
180: Vector3 direction = controller.GetLogicalFacingDirection();
181: direction.y = 0f;
182: return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
190: Vector3 direction = GetAttackDirection();
191: float rangeMultiplier = itemEffects != null ? itemEffects.GetMeleeRangeMultiplier() : 1f;
193: Vector3 boxCenter = transform.position + direction * effectiveAttackOffset;
200: Vector3 tallBoxSize = new Vector3(effectiveWidth, 20f, effectiveDepth);
202: float damageMultiplier = itemEffects != null ? itemEffects.GetOutgoingBasicDamageMultiplier() : 1f;
203: float flatDamageBonus = itemEffects != null ? itemEffects.GetOutgoingBasicDamageFlatBonus() : 0f;
204: float unstableMultiplier = itemEffects != null ? itemEffects.RollUnstableCoreDamageMultiplier() : 1f;
207: itemEffects?.NotifyBasicAttackPerformed(finalDamage, meleeTargetMask, effectiveAttackOffset + effectiveDepth * 0.5f, direction);
215: tallBoxSize / 2f + new Vector3(PlayerAttackGeometry.Padding, 0, PlayerAttackGeometry.Padding),
221: for (int i = 0; i < hitCount; i++)
232: if (enemy == null || enemy.IsDead || !PlayerAttackGeometry.InBox(boxCenter, tallBoxSize / 2f, rotation, enemy) || !meleeHitEnemies.Add(enemy))
246: private void SpawnMeleeSlashVisual(Vector3 center, Vector3 direction, Sprite sprite, float targetWorldSize)
254: visualObject.transform.position = new Vector3(center.x, transform.position.y + meleeSlashHeightOffset, center.z);
255: visualObject.transform.localScale = Vector3.one * GetMeleeSlashWorldScale(sprite, targetWorldSize);
261: StartCoroutine(FadeAndDestroyMeleeSlash(renderer, Mathf.Max(0.02f, meleeSlashLifetime)));
288: return Mathf.Max(0.1f, Mathf.Max(effectiveWidth, effectiveDepth) * meleeSlashScale);
295: return Mathf.Max(0.05f, meleeSlashScale);
299: if (spriteWorldSize <= 0.0001f)
301: return Mathf.Max(0.05f, meleeSlashScale);
304: return Mathf.Max(0.05f, targetWorldSize / spriteWorldSize);
307: private static Quaternion GetScreenAlignedRotation(Vector3 worldDirection)
309: worldDirection.y = 0f;
310: if (worldDirection.sqrMagnitude <= 0.0001f)
312: worldDirection = Vector3.forward;
318: float fallbackAngle = Mathf.Atan2(worldDirection.z, worldDirection.x) * Mathf.Rad2Deg;
319: return Quaternion.Euler(90f, 0f, fallbackAngle);
322: Vector3 projectedDirection = Vector3.ProjectOnPlane(worldDirection.normalized, activeCamera.transform.forward);
323: if (projectedDirection.sqrMagnitude <= 0.0001f)
329: float x = Vector3.Dot(projectedDirection, activeCamera.transform.right);
330: float y = Vector3.Dot(projectedDirection, activeCamera.transform.up);
331: float rollAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
332: return activeCamera.transform.rotation * Quaternion.AngleAxis(rollAngle, Vector3.forward);
352: float alpha = Mathf.Lerp(baseColor.a, 0f, elapsed / lifetime);
365: int size = Mathf.Max(1, meleeOverlapBufferSize);
374: int size = Mathf.Max(1, beamOverlapBufferSize);
391: Vector3 direction = GetAttackDirection();
393: float flatDamageBonus = itemEffects != null ? itemEffects.GetOutgoingBasicDamageFlatBonus() : 0f;
395: float unstableMultiplier = 1f;
406: Vector3 muzzleOrigin = firePoint != null ? firePoint.position : transform.position;
428: private void FireProjectileVolley(Vector3 direction, float damage, float range, bool allowExtraVolley)
430: int projectileCount = itemEffects != null ? itemEffects.GetForwardProjectileCount() : 1;
431: float spread = itemEffects != null ? itemEffects.GetSpreadAngleForCount(projectileCount) : 0f;
432: float accuracyPenalty = itemEffects != null ? itemEffects.GetAccuracyPenaltyAngle() : 0f;
433: Vector3 firingDirection = ApplyAccuracyPenalty(direction, accuracyPenalty);
436: : 1f;
439: if (projectileCount <= 1)
445: float center = (projectileCount - 1) * 0.5f;
446: for (int i = 0; i < projectileCount; i++)
449: Vector3 shotDirection = Quaternion.Euler(0f, offset, 0f) * firingDirection;
469: private static Vector3 ApplyAccuracyPenalty(Vector3 direction, float penaltyAngle)
471: Vector3 safeDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
472: if (penaltyAngle <= 0f)
478: return (Quaternion.Euler(0f, randomOffset, 0f) * safeDirection).normalized;
481: private IEnumerator FireCellProliferationVolleyDelayed(Vector3 direction, float damage, float range)
483: if (cellProliferationDelay > 0f)
500: private void FireBeamVolley(Vector3 direction, float damage, float range, bool allowExtraVolley)
502: int beamCount = itemEffects != null ? itemEffects.GetForwardProjectileCount() : 1;
503: float spread = itemEffects != null ? itemEffects.GetSpreadAngleForCount(beamCount) : 0f;
504: float accuracyPenalty = itemEffects != null ? itemEffects.GetAccuracyPenaltyAngle() : 0f;
505: Vector3 firingDirection = ApplyAccuracyPenalty(direction, accuracyPenalty);
508: : 1f;
511: if (beamCount <= 1)
517: float center = (beamCount - 1) * 0.5f;
518: for (int i = 0; i < beamCount; i++)
521: Vector3 beamDirection = Quaternion.Euler(0f, offset, 0f) * firingDirection;
540: private IEnumerator FireCellProliferationBeamVolleyDelayed(Vector3 direction, float damage, float range)
542: if (cellProliferationDelay > 0f)
559: private void SpawnProjectile(Vector3 direction, float damage, float range, Projectile.SpawnKind spawnKind)
575: Vector3 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
576: Vector3 spawnOrigin = firePoint != null ? firePoint.position : transform.position;
577: Vector3 spawnPos = spawnOrigin + forward * projectileSpawnOffset;
600: private void FireBeam(Vector3 direction, float damage, float range)
605: Vector3 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
606: Vector3 origin = firePoint != null ? firePoint.position : transform.position;
610: Vector3 end = origin + forward * Mathf.Max(0.5f, range);
611: float radius = itemEffects != null ? itemEffects.BeamRadius : 0.8f;
612: float halfHeight = Mathf.Max(0.05f, beamVerticalHalfHeight);
617: origin + Vector3.up * halfHeight,
618: end - Vector3.up * halfHeight,
624: for (int i = 0; i < hitCount; i++)
649: private void SpawnBeamVisual(Vector3 start, Vector3 end, float radius)
662: Mathf.Max(0.05f, beamVisualDuration),
697: : Resources.Load<Texture2D>(beamTextureResourcePath);
773: firePoint.localPosition = Vector3.zero;
809: Vector3 direction = GetAttackDirection();
811: float rangeMultiplier = itemEffects != null ? itemEffects.GetMeleeRangeMultiplier() : 1f;
817: Gizmos.matrix = Matrix4x4.TRS(
820: Vector3.one);
821: Gizmos.DrawWireCube(Vector3.zero, new Vector3(gizmoWidth, 20f, gizmoDepth));
844: line.positionCount = 2;
846: line.numCapVertices = 6;
848: line.sortingOrder = 5005;
850: Color baseColor = new Color(1f, 0.24f, 0.15f, 0.85f);
855: new GradientColorKey(baseColor, 0f),
856: new GradientColorKey(new Color(1f, 0.7f, 0.2f, 1f), 0.55f),
857: new GradientColorKey(baseColor, 1f)
861: new GradientAlphaKey(0.95f, 0f),
862: new GradientAlphaKey(0.65f, 0.8f),
863: new GradientAlphaKey(0f, 1f)
868: public void Show(Vector3 start, Vector3 end, float radius, float duration, Material material)
886: line.SetPosition(0, start);
887: line.SetPosition(1, end);
888: line.startWidth = Mathf.Max(0.05f, radius * 1.45f);
889: line.endWidth = Mathf.Max(0.03f, radius * 1.1f);
```

## Assets/_Project/Scripts/Gameplay/Player/Combat/PlayerAttackGeometry.cs

```csharp
7: public static float Padding => PlayerController.Instance != null ? PlayerController.Instance.AttackEdgePadding : .08f;
10: if (collider == null) return 0;
11: if (CombatHitGeometry.TryCapsule(collider, out Vector2 a, out Vector2 b, out float radius))
12: return radius + Vector2.Distance(a, b) * .5f;
15: Vector3 size = Vector3.Scale(box.size, box.transform.lossyScale);
16: return new Vector2(size.x, size.z).magnitude * .5f;
24: return collider != null && collider.enabled && (mask.value & (1 << collider.gameObject.layer)) != 0;
26: public static float DistanceSquared(Vector3 center, EnemyController enemy) => CombatHitGeometry.PointRectDistanceSquared(CombatHitGeometry.Flat(center), CombatHitGeometry.EnemyBodyBounds(enemy));
27: public static bool InCircle(Vector3 center, float radius, EnemyController enemy) => DistanceSquared(center, enemy) <= Mathf.Pow(Mathf.Max(0, radius) + Padding, 2);
28: public static bool InArc(Vector3 center, Vector3 forward, float radius, float halfAngle, EnemyController enemy)
30: if (halfAngle >= 179.99f) return InCircle(center, radius, enemy);
32: Vector2 origin = CombatHitGeometry.Flat(center), aim = CombatHitGeometry.Flat(forward).normalized;
33: if (aim.sqrMagnitude < .0001f) aim = Vector2.up;
34: float r = Mathf.Max(0, radius), cos = Mathf.Cos(Mathf.Clamp(halfAngle, .5f, 180) * Mathf.Deg2Rad);
35: bool Contains(Vector2 p) { Vector2 delta = p - origin; return delta.sqrMagnitude <= (r + Padding) * (r + Padding) && (delta.sqrMagnitude < .000001f || Vector2.Dot(delta.normalized, aim) >= cos); }
36: Vector2 nearest = new Vector2(Mathf.Clamp(origin.x, body.min.x, body.max.x), Mathf.Clamp(origin.y, body.min.z, body.max.z));
38: if (Contains(new Vector2(body.min.x, body.min.z)) || Contains(new Vector2(body.max.x, body.min.z))
39: || Contains(new Vector2(body.max.x, body.max.z)) || Contains(new Vector2(body.min.x, body.max.z))) return true;
40: for (int sign = -1; sign <= 1; sign += 2)
42: float a = halfAngle * Mathf.Deg2Rad * sign;
43: Vector2 edge = new Vector2(aim.x * Mathf.Cos(a) - aim.y * Mathf.Sin(a), aim.x * Mathf.Sin(a) + aim.y * Mathf.Cos(a));
44: if (CombatHitGeometry.SegmentRectDistanceSquared(origin, origin + edge * r, body) <= Padding * Padding + .0000001f) return true;
48: public static bool InBox(Vector3 center, Vector3 halfSize, Quaternion rotation, EnemyController enemy)
50: Bounds b = CombatHitGeometry.EnemyBodyBounds(enemy); Vector2 delta = CombatHitGeometry.Flat(b.center - center);
51: Vector2 right = CombatHitGeometry.Flat(rotation * Vector3.right).normalized, forward = CombatHitGeometry.Flat(rotation * Vector3.forward).normalized;
53: bool Separated(Vector2 axis) => Mathf.Abs(Vector2.Dot(delta, axis)) > x * Mathf.Abs(Vector2.Dot(right, axis)) + z * Mathf.Abs(Vector2.Dot(forward, axis))
55: return !Separated(Vector2.right) && !Separated(Vector2.up) && !Separated(right) && !Separated(forward);
```

## Assets/_Project/Scripts/Gameplay/Player/Combat/PlayerCombatCalculator.cs

```csharp
7: private const float PercentToRatio = 0.01f;
8: private const float MaximumSkillCooldownReductionPercent = 40f;
10: public static float GetBasicAttackDamage(PlayerStats stats, float fallbackDamage = 0f)
13: float multiplier = DifficultyBalanceService.ActiveProfile?.player?.basicAttackDamage ?? 1f;
14: return Mathf.Max(0f, damage * Mathf.Max(0f, multiplier));
19: float attackSpeed = stats != null ? stats.AttackSpeed : 1f;
20: attackSpeed = Mathf.Max(0.01f, attackSpeed);
21: float multiplier = DifficultyBalanceService.ActiveProfile?.player?.basicAttackCooldown ?? 1f;
22: return Mathf.Max(0f, baseCooldown / attackSpeed * Mathf.Max(0.01f, multiplier));
27: float attackRange = stats != null ? stats.AttackRange : 1f;
28: float multiplier = DifficultyBalanceService.ActiveProfile?.player?.basicAttackRange ?? 1f;
29: return Mathf.Max(0f, baseRange * Mathf.Max(0f, attackRange) * Mathf.Max(0f, multiplier));
34: float reductionPercent = stats != null ? stats.SkillCooldownReduction : 0f;
35: reductionPercent = Mathf.Clamp(reductionPercent, 0f, MaximumSkillCooldownReductionPercent);
36: float multiplier = 1f - reductionPercent * PercentToRatio;
37: float difficultyMultiplier = DifficultyBalanceService.ActiveProfile?.player?.skillCooldown ?? 1f;
38: return Mathf.Max(0f, baseCooldown * multiplier * Mathf.Max(0.01f, difficultyMultiplier));
43: float multiplier = DifficultyBalanceService.ActiveProfile?.player?.skillDamage ?? 1f;
44: return Mathf.Max(0f, baseDamage * GetSkillDamageMultiplier(stats) * Mathf.Max(0f, multiplier));
49: float magicPercent = stats != null ? stats.Magic : 0f;
50: return Mathf.Max(0f, 1f + magicPercent * PercentToRatio);
```

## Assets/_Project/Scripts/Gameplay/Player/Combat/Projectile.cs

```csharp
10: Normal = 0,
11: SplitChild = 1
14: private const int HitBufferSize = 8;
15: private const int ExplosionBufferSize = 24;
16: private const int ObstacleHitBufferSize = 12;
19: [SerializeField] private float speed = 15f;
20: [SerializeField] private float lifeTime = 3f;
21: [SerializeField] private LayerMask targetMask = ~0;
22: [SerializeField] private float hitCheckRadius = 0.35f;
23: [SerializeField] private float hitCheckHeightOffset = 0.75f;
24: [SerializeField] private float hitCheckVerticalHalfHeight = 2.5f;
26: private Vector3 moveDirection;
35: private int maxHitCount = 1;
46: private Vector3 activeBaseScale = Vector3.one;
55: private Vector3 defaultLocalScale = Vector3.one;
58: public void Launch(Vector3 direction, float damage)
60: Launch(direction, damage, targetMask, lifeTime * Mathf.Max(0.1f, speed), null, SpawnKind.Normal);
63: public void Launch(Vector3 direction, float damage, LayerMask mask)
65: Launch(direction, damage, mask, lifeTime * Mathf.Max(0.1f, speed), null, SpawnKind.Normal);
68: public void Launch(Vector3 direction, float damage, LayerMask mask, float range)
73: public void Launch(Vector3 direction, float damage, LayerMask mask, float range, PlayerItemCombatEffects effects, SpawnKind kind = SpawnKind.Normal)
76: direction.y = 0f;
77: moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
79: currentSpeed = Mathf.Max(0.01f, speed);
86: traveledDistance = 0f;
87: currentHitCount = 0;
93: boomerangPassHitCount = 0;
94: launchRange = Mathf.Max(0.05f, range);
98: maxHitCount = 1;
104: maxHitCount = Mathf.Max(1, itemEffects.GetPiercingHitCount());
114: remainingBounces = 0;
118: float scaleMultiplier = 1f;
133: effectiveRange *= 2f;
139: pulseScaleGrowMode = Random.value < 0.5f;
143: deactivateTime = Time.time + effectiveRange / Mathf.Max(0.01f, currentSpeed);
149: if (currentSpeed <= 0f)
151: currentSpeed = Mathf.Max(0.01f, speed);
178: Vector3 step = moveDirection * stepDistance;
185: Vector3 nextPosition = transform.position + step;
198: Vector3 toOwner = ownerTransform.position - transform.position;
199: toOwner.y = 0f;
200: if (toOwner.sqrMagnitude <= 0.49f)
237: if ((targetMask.value & (1 << other.gameObject.layer)) == 0)
317: Vector3 hitCenter = transform.position;
319: float halfHeight = Mathf.Max(0.05f, hitCheckVerticalHalfHeight);
320: Vector3 capsuleTop = hitCenter + Vector3.up * halfHeight;
321: Vector3 capsuleBottom = hitCenter - Vector3.up * halfHeight;
331: for (int i = 0; i < count; i++)
355: Vector3 toTarget = nearestEnemy.transform.position - transform.position;
356: toTarget.y = 0f;
357: if (toTarget.sqrMagnitude <= 0.0001f)
362: Vector3 desiredDirection = toTarget.normalized;
364: moveDirection = Vector3.Slerp(moveDirection, desiredDirection, turnRate * deltaTime).normalized;
370: if (enemies == null || enemies.Count == 0)
379: for (int i = 0; i < enemies.Count; i++)
387: Vector3 toEnemy = enemy.transform.position - transform.position;
388: toEnemy.y = 0f;
409: float returnDistance = itemEffects.GetBoomerangReturnDistance(Mathf.Max(launchRange, 0.5f));
420: Vector3 toOwner = ownerTransform.position - transform.position;
421: toOwner.y = 0f;
422: if (toOwner.sqrMagnitude > 0.0001f)
431: boomerangPassHitCount = 0;
436: private bool TryReflectFromBiome(ref Vector3 step)
438: if (remainingBounces <= 0)
449: Vector3 current = transform.position;
450: int sampleCount = Mathf.Max(2, Mathf.CeilToInt(step.magnitude / 0.2f));
451: Vector3 segment = step / sampleCount;
452: Vector3 hitProbe = current;
453: Vector3 preHitProbe = current;
455: for (int i = 1; i <= sampleCount; i++)
457: Vector3 probe = current + segment * i;
461: preHitProbe = current + segment * (i - 1);
472: Vector3 localStep = hitProbe - preHitProbe;
473: bool xBlocked = !IsWalkablePosition(biome, preHitProbe + new Vector3(localStep.x, 0f, 0f));
474: bool zBlocked = !IsWalkablePosition(biome, preHitProbe + new Vector3(0f, 0f, localStep.z));
476: Vector3 reflectedDirection = moveDirection;
492: moveDirection = reflectedDirection.sqrMagnitude > 0.0001f ? reflectedDirection.normalized : -moveDirection;
498: private bool TryReflectFromObstacleCollider(ref Vector3 step)
500: if (remainingBounces <= 0)
506: if (stepDistance <= 0.0001f)
511: Vector3 direction = step / stepDistance;
512: float probeRadius = Mathf.Max(0.05f, GetScaledHitCheckRadius() * 0.65f);
518: stepDistance + 0.02f,
519: ~0,
522: if (hitCount <= 0)
530: for (int i = 0; i < hitCount; i++)
552: Vector3 normal = nearestHit.normal.sqrMagnitude > 0.0001f ? nearestHit.normal.normalized : -moveDirection;
553: Vector3 reflectedDirection = Vector3.Reflect(moveDirection, normal);
554: moveDirection = reflectedDirection.sqrMagnitude > 0.0001f ? reflectedDirection.normalized : -moveDirection;
557: transform.position = nearestHit.point + normal * (probeRadius + 0.02f);
588: private static bool IsWalkablePosition(BiomeManager biome, Vector3 worldPosition)
590: Vector2Int grid = biome.WorldToGrid(worldPosition);
603: pulseScaleGrowMode = Random.value < 0.5f;
607: float progress = Mathf.Clamp01(traveledDistance / Mathf.Max(0.05f, pulseRangeBudget));
609: float acceleratedProgress = 1f - Mathf.Pow(1f - progress, 2.2f);
610: float startScale = 1f;
622: 0.0001f,
629: float scaleRatio = Mathf.Max(0.05f, currentSize / defaultSize);
630: return Mathf.Max(.05f, Mathf.Max(hitCheckRadius * scaleRatio, PlayerAttackGeometry.ColliderRadius(hitCollider))) + PlayerAttackGeometry.Padding;
643: float explosionDamage = Mathf.Max(0f, sourceDamage * itemEffects.GetExplosionDamageMultiplier());
644: ResidueRubble.HitArea(transform.position, radius, Vector3.forward, 180, explosionDamage, targetMask, propAttackToken);
646: for (int i = 0; i < hitCount; i++)
668: private static void SpawnExplosionVisual(Vector3 center, float radius)
674: new Vector3(center.x, center.y + 0.08f, center.z),
675: Mathf.Max(0.2f, radius * 2f * 0.85f),
676: effectSprite != null ? Color.white : new Color(1f, 0.26f, 0.12f, 0.55f),
677: 5100,
678: 0.2f,
684: float remainingTime = Mathf.Max(0f, deactivateTime - Time.time);
685: return remainingTime * Mathf.Max(0.01f, currentSpeed);
734: pulseRangeBudget = 0f;
735: currentHitCount = 0;
736: remainingBounces = 0;
737: launchRange = 0f;
739: currentSpeed = Mathf.Max(0.01f, speed);
750: public static Sprite LoadResourceSprite(string resourcePath, float pixelsPerUnit = 100f)
765: Texture2D texture = Resources.Load<Texture2D>(resourcePath);
770: new Rect(0f, 0f, texture.width, texture.height),
771: new Vector2(0.5f, 0.5f),
772: Mathf.Max(1f, pixelsPerUnit));
785: : 1f;
786: return Mathf.Max(0.01f, targetWorldSize) / Mathf.Max(0.0001f, spriteSize);
824: const int size = 64;
825: Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
829: float center = (size - 1) * 0.5f;
830: float radius = center - 1f;
831: float innerRadius = radius * 0.45f;
833: for (int y = 0; y < size; y++)
835: for (int x = 0; x < size; x++)
853: texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
860: new Rect(0f, 0f, size, size),
861: new Vector2(0.5f, 0.5f),
```

## Assets/_Project/Scripts/Gameplay/Player/Combat/ProjectileDirectionalSprite.cs

```csharp
11: private enum Dir8
13: N = 0,
24: private static readonly int[] DirToSpriteIndex = { 0, 1, 2, 5, 7, 8, 6, 3 };
30: [Tooltip("Off: HEAD 8-direction sprites first. On: Incoming generated sprite first.")]
32: [SerializeField] private float spriteScale = 0.4f;
33: [SerializeField] private float incomingSpriteScale = 0.4f;
34: [SerializeField] private int sortingOrder = 2700;
69: public void SetDirection(Vector3 direction)
77: direction.y = 0f;
78: if (direction.sqrMagnitude < 0.0001f)
83: Vector3 normalizedDirection = direction.normalized;
91: private bool TryApplyHeadSprite(Vector3 direction)
94: if (headSprites == null || headSprites.Length == 0)
99: int directionIndex = GetDir8Index(direction);
101: if (spriteIndex < 0 || spriteIndex >= headSprites.Length)
107: spriteRenderer.transform.localScale = Vector3.one * Mathf.Max(0.05f, spriteScale);
112: : Quaternion.Euler(90f, 0f, 0f);
116: private bool TryApplyIncomingSprite(Vector3 direction)
126: Vector3.one * Mathf.Max(0.05f, incomingSpriteScale);
146: if (loaded == null || loaded.Length == 0)
194: spriteObject.transform.localPosition = Vector3.zero;
195: spriteObject.transform.localScale = Vector3.one * Mathf.Max(0.05f, spriteScale);
219: if (separator < 0 || separator >= spriteName.Length - 1)
224: int value = 0;
225: for (int i = separator + 1; i < spriteName.Length; i++)
228: if (digit < '0' || digit > '9')
233: value = value * 10 + (digit - '0');
239: private static int GetDir8Index(Vector3 direction)
241: float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
242: if (angle < 0f)
244: angle += 360f;
247: if (angle < 22.5f || angle >= 337.5f)
249: return (int)Dir8.E;
252: if (angle < 67.5f)
254: return (int)Dir8.NE;
257: if (angle < 112.5f)
259: return (int)Dir8.N;
262: if (angle < 157.5f)
264: return (int)Dir8.NW;
267: if (angle < 202.5f)
269: return (int)Dir8.W;
272: if (angle < 247.5f)
274: return (int)Dir8.SW;
277: if (angle < 292.5f)
279: return (int)Dir8.S;
282: return (int)Dir8.SE;
285: private static Quaternion GetScreenAlignedRotation(Vector3 worldDirection)
290: float fallbackAngle = Mathf.Atan2(worldDirection.z, worldDirection.x) * Mathf.Rad2Deg;
291: return Quaternion.Euler(90f, 0f, fallbackAngle);
294: Vector3 projectedDirection =
295: Vector3.ProjectOnPlane(worldDirection, activeCamera.transform.forward);
296: if (projectedDirection.sqrMagnitude <= 0.0001f)
302: float x = Vector3.Dot(projectedDirection, activeCamera.transform.right);
303: float y = Vector3.Dot(projectedDirection, activeCamera.transform.up);
304: float rollAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
305: return activeCamera.transform.rotation * Quaternion.AngleAxis(rollAngle, Vector3.forward);
```

## Assets/_Project/Scripts/Gameplay/Player/Progression/LevelProgressionConfig.cs

```csharp
13: [Min(1)] public int maxLevel = 30;
14: [Min(1)] public int baseExp = 30;
15: [Min(0)] public int expIncreasePerLevel = 20;
16: [Min(0)] public int enemyKillExp = 10;
19: [Min(1)] public int earlyLevelUpperBound = 9;
20: [Min(0f)] public float earlyExpGainMultiplier = 2f;
21: [Min(0f)] public float jobSelectionBlockedExpMultiplier = 0f;
22: [Min(0f)] public float jobSelectionSelectedExpMultiplier = 1f;
23: [Min(1)] public int midLevelUpperBound = 20;
24: [Min(0f)] public float midExpGainMultiplier = 1f;
25: [Min(0f)] public float lateExpGainMultiplier = 0.8f;
28: [Min(1)] public int jobSelectionLevel = 10;
29: [Min(1)] public int jobBasedChoiceStartLevel = 11;
30: [Min(1)] public int skill1UnlockLevel = 10;
31: [Min(1)] public int skill2UnlockLevel = 20;
41: public int bioGambleMinDelta = -1;
42: public int bioGambleMaxDelta = 3;
44: public int MaxLevel => Mathf.Max(1, maxLevel);
45: public int BaseExp => Mathf.Max(1, baseExp);
46: public int ExpIncreasePerLevel => Mathf.Max(0, expIncreasePerLevel);
47: public int EnemyKillExp => Mathf.Max(0, enemyKillExp);
48: public int JobSelectionLevel => Mathf.Max(1, jobSelectionLevel);
49: public int JobBasedChoiceStartLevel => Mathf.Max(1, jobBasedChoiceStartLevel);
50: public int Skill1UnlockLevel => Mathf.Max(1, skill1UnlockLevel);
51: public int Skill2UnlockLevel => Mathf.Max(1, skill2UnlockLevel);
55: int completedLevelUps = Mathf.Max(0, currentLevel - 1);
56: return Mathf.Max(1, BaseExp + completedLevelUps * ExpIncreasePerLevel);
61: if (currentLevel <= Mathf.Max(1, earlyLevelUpperBound))
63: return Mathf.Max(0f, earlyExpGainMultiplier);
69: ? Mathf.Max(0f, jobSelectionBlockedExpMultiplier)
70: : Mathf.Max(0f, jobSelectionSelectedExpMultiplier);
75: return Mathf.Max(0f, midExpGainMultiplier);
78: return Mathf.Max(0f, lateExpGainMultiplier);
83: return skillSlotIndex <= 1 ? Skill1UnlockLevel : Skill2UnlockLevel;
89: if (source == null || source.Count == 0)
95: for (int i = 0; i < source.Count; i++)
106: if (source == null || source.Count == 0)
111: for (int i = 0; i < source.Count; i++)
121: for (int i = 0; i < defaults.Count; i++)
130: return 0f;
147: if (source == null || source.Count == 0)
152: for (int i = 0; i < source.Count; i++)
170: return 0f;
174: ? valueConfig.value / 100f
186: for (int i = 0; i < statTypes.Length; i++)
210: new CharacterStatValue(CharacterStatType.MaxHealth, 10f),
211: new CharacterStatValue(CharacterStatType.MoveSpeed, 10f),
212: new CharacterStatValue(CharacterStatType.AttackPower, 1f),
213: new CharacterStatValue(CharacterStatType.AttackSpeed, 1f),
214: new CharacterStatValue(CharacterStatType.AttackRange, 1f),
215: new CharacterStatValue(CharacterStatType.Magic, 1f),
216: new CharacterStatValue(CharacterStatType.SkillCooldownReduction, 0f)
224: new LevelUpStatValueConfig(CharacterStatType.AttackPower, CharacterStatModifierMode.Flat, 1f),
225: new LevelUpStatValueConfig(CharacterStatType.AttackRange, CharacterStatModifierMode.PercentAdd, 1f),
226: new LevelUpStatValueConfig(CharacterStatType.AttackSpeed, CharacterStatModifierMode.PercentAdd, 1f),
227: new LevelUpStatValueConfig(CharacterStatType.Magic, CharacterStatModifierMode.Flat, 1f),
228: new LevelUpStatValueConfig(CharacterStatType.MoveSpeed, CharacterStatModifierMode.PercentAdd, 1f),
229: new LevelUpStatValueConfig(CharacterStatType.MaxHealth, CharacterStatModifierMode.Flat, 1f, true)
```

## Assets/_Project/Scripts/Gameplay/Player/Progression/LevelUpManager.cs

```csharp
23: private static int currentLevel = 1;   // 현재 레벨
24: private static int currentExp = 0;     // 현재 누적 경험치
25: private static int expRequired = 100;  // 다음 레벨까지 필요 경험치
60: AddEnemyKillExp(amount, 1f);
66: AddResolvedEnemyKillExp(Mathf.Max(0, Mathf.RoundToInt(Mathf.Max(0, amount) * Mathf.Max(0f, rewardMultiplier))));
74: if (enemyKillExp <= 0) return;
101: if (pendingLevelUps > 0)
116: return pendingLevelUps > 0;
121: if (pendingLevelUps > 0)
208: return allChoices.Take(4).ToList();
223: result.Add(remaining[0]);
227: result.Add(remaining[0]);
240: counts[choice] = 0;
244: int maxCount = 0;
261: if (mostSelectedList.Count > 0)
262: return mostSelectedList[UnityEngine.Random.Range(0, mostSelectedList.Count)];
268: return allChoices[UnityEngine.Random.Range(0, allChoices.Count)];
281: for (int i = list.Count - 1; i > 0; i--)
283: int randomIndex = UnityEngine.Random.Range(0, i + 1);
311: target.level = Mathf.Clamp(currentLevel, 1, Config.MaxLevel);
312: target.experience = Mathf.Max(0, currentExp);
316: for (int i = 0; i < resolvedModifiers.Count; i++)
331: currentLevel = Mathf.Clamp(source.level, 1, Config.MaxLevel);
332: currentExp = Mathf.Max(0, source.experience);
333: pendingLevelUps = 0;
337: currentExp = Mathf.Min(currentExp, Mathf.Max(0, expRequired - 1));
341: currentExp = 0;
353: for (int i = 0; i < source.levelUpModifiers.Count; i++)
377: OnExpGained?.Invoke(0);
```

## Assets/_Project/Scripts/Gameplay/Player/Runtime/Health.cs

```csharp
15: [SerializeField] private float invincibilityDuration = 0.5f;
16: [SerializeField] private float hitFlashInterval = 0.08f;
17: [SerializeField] private Color hitFlashColor = new Color(1f, 0.18f, 0.12f, 1f);
29: public float CurrentHealth => Stats?.CurrentHealth ?? 0f;
30: public float MaxHealth => Stats?.MaxHealth ?? 0f;
83: if (args.CurrentValue <= 0f && args.PreviousValue > 0f)
98: if (Stats != null && Stats.CurrentHealth > 0f)
109: if (!effects.TryConsumeSplitRegeneration(0f, maxHealth, out float reviveHealth))
122: TakeDamage(new EnemyDamageRequest(Mathf.Max(0f, damageAmount * DifficultyBalanceService.GetIncomingDamageMultiplier(sourceEnemy)), sourceEnemy));
129: if (isInvincible || IsDead || damageAmount <= 0f) return;
139: if (actualDamage <= 0f)
151: float targetHealth = Mathf.Clamp(reviveHealth, 0f, maxHealth);
168: if (appliedDamage > 0f)
170: Vector3 sourcePosition = sourceEnemy != null
172: : transform.position - Vector3.forward;
184: if (IsDead || amount <= 0f) return;
205: if (duration <= 0f)
229: float elapsed = 0f;
230: float nextFlashTime = 0f;
232: float interval = Mathf.Max(0.03f, hitFlashInterval);
273: if (cachedSpriteRenderers.Count == 0)
278: for (int i = 0; i < cachedSpriteRenderers.Count; i++)
288: for (int i = 0; i < cachedSpriteRenderers.Count; i++)
299: if (cachedSpriteRenderers.Count == 0 || cachedSpriteColors.Count == 0)
309: if (cachedSpriteRenderers.Count == 0 || cachedSpriteColors.Count == 0)
315: for (int i = 0; i < count; i++)
```

## Assets/_Project/Scripts/Gameplay/Player/Runtime/PlayerController.cs

```csharp
63: [Header("근접공격 스프라이트 (8방향)")]
73: [Header("원거리공격 스프라이트 (8방향)")]
84: [SerializeField] private float dashSpeed = 20f;
85: [SerializeField] private float dashDuration = 0.15f;
86: [SerializeField] private float dashCooldown = 0.8f;
90: [SerializeField] private float idleFrameRate = 4f;
91: [SerializeField] private float walkFrameRate = 8f;
92: [SerializeField] private float attackAnimDuration = 0.3f;
93: [SerializeField] private float attackFrameRate = 12f;
95: [SerializeField] private float deathFrameRate = 8f;
99: [SerializeField] private float lockedY = -2f;
100: [SerializeField] private float groundOffsetY = -2f;
106: private Vector3 lastMoveDirection = Vector3.forward;
112: private Vector3 dashVelocity;
119: private float attackAnimEndTime = 0f;                       // ?대룞 以??щ?
123: private int currentFrame = 0;        // ?꾩옱 ?꾨젅???몃뜳??
124: private float frameTimer = 0f;       // ?꾨젅???꾪솚 ??대㉧
128: private Vector3 movement;                  // ?대룞 踰≫꽣
145: public float MoveSpeed => playerStats != null ? playerStats.MoveSpeed : 0f;
146: public float CurrentHealth => playerStats != null ? playerStats.CurrentHealth : 0f;
147: public float MaxHealth => playerStats != null ? playerStats.MaxHealth : 0f;
148: public float AttackPower => playerStats != null ? playerStats.AttackPower : 0f;
149: public float AttackSpeed => playerStats != null ? playerStats.AttackSpeed : 0f;
150: public float AttackRange => playerStats != null ? playerStats.AttackRange : 0f;
151: public float Magic => playerStats != null ? playerStats.Magic : 0f;
152: public float SkillCooldownReduction => playerStats != null ? playerStats.SkillCooldownReduction : 0f;
196: for (int i = 0; i < sprites.Length; i++)
241: spriteObj.transform.localPosition = Vector3.zero;
296: Vector3 pos = transform.position;
297: pos.y = 0f;
337: if (!Application.isFocused || Time.timeSinceLevelLoad < 0.5f)
339: movement = Vector3.zero;
353: Vector2 moveInput = input.MoveAction.ReadValue<Vector2>();
358: movement = new Vector3(moveInput.x, 0, moveInput.y).normalized;
359: isMoving = movement.sqrMagnitude > 0.01f;
389: currentDirection = v > 0 ? Direction.Up : Direction.Down;
393: currentDirection = h > 0 ? Direction.Right : Direction.Left;
450: currentFrame = 0;
451: frameTimer = 0f;
454: if (currentAnimation != null && currentAnimation.Length > 0)
456: spriteRenderer.sprite = currentAnimation[0];
477: if (currentAnimation == null || currentAnimation.Length == 0) return;
480: float frameDuration = 1f / currentFrameRate;
484: int framesElapsed = isPlayingAttackAnim ? Mathf.FloorToInt(frameTimer / frameDuration) : 1;
487: ? Mathf.Min(currentFrame + framesElapsed, currentAnimation.Length - 1)
519: rb.linearVelocity = Vector3.zero;
520: rb.angularVelocity = Vector3.zero;
525: Vector3 moveVector = movement * MoveSpeed * Time.fixedDeltaTime;
530: rb.linearVelocity = Vector3.zero;
531: rb.angularVelocity = Vector3.zero;
539: private bool TryMoveWithHeight(Vector3 moveVector)
546: Vector3 proceduralCurrentPos = characterController == null && rb != null ? rb.position : transform.position;
547: Vector3 proceduralTargetPos = proceduralCurrentPos + moveVector;
554: Vector3 proceduralMoveX = new Vector3(moveVector.x, 0f, 0f);
555: Vector3 proceduralMoveZ = new Vector3(0f, 0f, moveVector.z);
558: if (proceduralMoveX.sqrMagnitude > 0f && proceduralTerrainMotor.CanMove(proceduralCurrentPos, proceduralCurrentPos + proceduralMoveX))
564: if (proceduralMoveZ.sqrMagnitude > 0f && proceduralTerrainMotor.CanMove(proceduralCurrentPos, proceduralCurrentPos + proceduralMoveZ))
572: if (proceduralMoveZ.sqrMagnitude > 0f && proceduralTerrainMotor.CanMove(proceduralCurrentPos, proceduralCurrentPos + proceduralMoveZ))
578: if (proceduralMoveX.sqrMagnitude > 0f && proceduralTerrainMotor.CanMove(proceduralCurrentPos, proceduralCurrentPos + proceduralMoveX))
595: Vector3 currentPos = characterController == null && rb != null ? rb.position : transform.position;
596: Vector3 targetPos = currentPos + moveVector;
604: Vector3 moveX = new Vector3(moveVector.x, 0f, 0f);
605: Vector3 moveZ = new Vector3(0f, 0f, moveVector.z);
609: if (moveX.sqrMagnitude > 0f && CanMoveInBiome(biome, currentPos, currentPos + moveX))
615: if (moveZ.sqrMagnitude > 0f && CanMoveInBiome(biome, currentPos, currentPos + moveZ))
624: if (moveZ.sqrMagnitude > 0f && CanMoveInBiome(biome, currentPos, currentPos + moveZ))
630: if (moveX.sqrMagnitude > 0f && CanMoveInBiome(biome, currentPos, currentPos + moveX))
639: private bool CanMoveInBiome(BiomeManager biome, Vector3 currentPosition, Vector3 targetPosition)
646: public bool TryMoveByWorld(Vector3 displacement)
649: displacement.y = 0f;
651: if (distance <= 0.000001f)
658: ? Mathf.Max(0.05f, biome.TileSize * 0.45f)
660: int stepCount = Mathf.Max(1, Mathf.CeilToInt(distance / maxStep));
661: Vector3 step = displacement / stepCount;
664: for (int i = 0; i < stepCount; i++)
679: private void ApplyMove(Vector3 moveVector)
709: public void SetBossDisplacementPosition(Vector3 position)
741: movement = Vector3.zero;
745: rb.linearVelocity = Vector3.zero;
746: rb.angularVelocity = Vector3.zero;
753: public void SpawnAt(Vector3 position)
760: rb.linearVelocity = Vector3.zero;
761: rb.angularVelocity = Vector3.zero;
781: if (duration <= 0f) return;
810: movement = Vector3.zero;
869: Vector3 pos = transform.position;
877: Vector3 pos = rb.position;
881: Vector3 vel = rb.linearVelocity;
882: vel.y = 0f;
887: Vector3 fallback = transform.position;
898: rb.angularVelocity = Vector3.zero;
912: public Vector3 GetLogicalFacingDirection()
914: if (movement.sqrMagnitude > 0.0001f)
919: if (lastMoveDirection.sqrMagnitude > 0.0001f)
927: public static Vector3 DirectionToVector(Direction direction)
931: Direction.Up => Vector3.forward,
932: Direction.Down => Vector3.back,
933: Direction.Left => Vector3.left,
934: Direction.Right => Vector3.right,
935: _ => Vector3.forward
966: float finalDamage = Mathf.Max(0f, damage);
1016: if (sprites == null || sprites.Length == 0)
1025: float duration = Mathf.Max(0.05f, attackAnimDuration);
1028: float frameRate = Mathf.Max(attackFrameRate > 0f ? attackFrameRate : 12f, sprites.Length / duration);
1034: Debug.Log($"[PlayerController] 공격 애니 시작: sprites={sprites.Length} s[0]={sprites[0]?.name ?? "NULL"} duration={duration} frameRate={frameRate}");
1044: bool diagX = absX > 0.3f;
1045: bool diagZ = absZ > 0.3f;
1049: if (x < 0 && z > 0) return meleeUpLeft;
1050: if (x > 0 && z > 0) return meleeUpRight;
1051: if (x < 0 && z < 0) return meleeDownLeft;
1055: return z > 0 ? meleeUp : meleeDown;
1056: return x > 0 ? meleeRight : meleeLeft;
1065: bool diagX = absX > 0.3f;
1066: bool diagZ = absZ > 0.3f;
1070: if (x < 0 && z > 0) return rangedUpLeft;
1071: if (x > 0 && z > 0) return rangedUpRight;
1072: if (x < 0 && z < 0) return rangedDownLeft;
1076: return z > 0 ? rangedUp : rangedDown;
1077: return x > 0 ? rangedRight : rangedLeft;
1086: Vector3 dir = lastMoveDirection.sqrMagnitude > 0.001f
1089: dir.y = 0f;
1096: dashVelocity = Vector3.zero;
1171: movement = Vector3.zero;
1174: dashVelocity = Vector3.zero;
1179: rb.linearVelocity = Vector3.zero;
1180: rb.angularVelocity = Vector3.zero;
1245: if (args.CurrentValue <= 0f && args.PreviousValue > 0f)
1264: if (!deathHandled && args.CurrentValue <= 0f)
1272: if (playerStats != null && playerStats.CurrentHealth > 0f)
1283: if (!itemEffects.TryConsumeSplitRegeneration(0f, maxHealth, out float reviveHealth))
1290: health?.GrantTemporaryInvincibility(0.5f);
1315: dashVelocity = Vector3.zero;
1333: Debug.Log("[Player] HP媛 0???섏뼱 ?щ쭩?덉뒿?덈떎.");
1340: if (deathFrames.Length > 0)
1343: float frameDuration = 1f / Mathf.Max(1f, deathFrameRate);
1344: for (int i = 0; i < deathFrames.Length; i++)
1367: if (deathSprites == null || deathSprites.Length == 0)
1379: for (int i = 0; i < deathSprites.Length; i++)
1392: if (deathSprites.Length >= 2
1393: && IsHorizontalDeathSheet(deathSprites[0])
1394: && IsHorizontalDeathSheet(deathSprites[1]))
1396: Vector3 facing = GetLogicalFacingDirection();
1397: return facing.z > 0.1f ? deathSprites[1] : deathSprites[0];
1400: return deathSprites[0];
1414: if (frameCount <= 1)
1420: for (int i = 0; i < frameCount; i++)
1431: new Vector2(0.5f, 0.5f),
1433: 0,
1450: return height > 0 && width >= height * 2 && width % height == 0;
```

## Assets/_Project/Scripts/Gameplay/Player/Runtime/PlayerController.HitGeometry.cs

```csharp
9: [SerializeField, Range(.5f, 1f)] private float hurtboxWidthRatio = .9f;
11: [SerializeField, Range(0f, .2f)] private float attackEdgePadding = .08f;
13: private float bodyRadiusReference, bodyHeightReference, appliedHurtRatio = -1;
14: public float AttackEdgePadding => Mathf.Clamp(attackEdgePadding, 0, .2f);
15: public float HurtboxWidthRatio => Mathf.Clamp(hurtboxWidthRatio, .5f, 1f);
30: float worldWidth = spriteRenderer.transform.TransformVector(Vector3.right * reference.bounds.size.x).magnitude;
31: Vector3 scale = combatCapsule.transform.lossyScale;
33: if (horizontalScale > .0001f) bodyRadiusReference = Mathf.Min(originalRadius, worldWidth * .5f / horizontalScale);
35: bodyHeightReference = combatCapsule.height * bodyRadiusReference / Mathf.Max(.0001f, originalRadius);
42: combatCapsule.radius = Mathf.Max(.0001f, bodyRadiusReference * appliedHurtRatio);
43: combatCapsule.height = Mathf.Max(combatCapsule.radius * 2, bodyHeightReference * appliedHurtRatio);
```

## Assets/_Project/Scripts/Gameplay/Player/Runtime/PlayerStats.cs

```csharp
108: for (int index = 0; index < baseStats.Length; index++)
185: stat.Value / 100f, // UI에선 3%로 표시, 내부적으론 0.03
190: stat.Value / 100f,
195: if (maxHealthIncrease > 0f)
227: public float GetDefense() => 0f;
```

## Assets/_Project/Scripts/Gameplay/Player/Runtime/ProceduralTerrainMotor.cs

```csharp
14: [SerializeField] private Vector2 terrainHalfExtents = new Vector2(0.68f, 0.48f);
15: [SerializeField, Min(0.1f)] private float holdDuration = 2f;
16: [SerializeField, Min(0.05f)] private float climbMoveDuration = 0.18f;
17: [SerializeField, Min(0.1f)] private float lavaJumpDuration = 0.5f;
18: [SerializeField, Min(0f)] private float lavaJumpHeight = 0.8f;
19: [SerializeField, Min(0f)] private float heightStep = 0.5f;
26: private Vector3 pendingDestination;
32: private Vector3 traversalStart;
33: private Vector3 traversalDestination;
37: public Vector2 TerrainHalfExtents => terrainHalfExtents;
53: public bool CanMove(Vector3 current, Vector3 target)
76: Vector3 spawn = map.GetPlayerSpawnWorldPosition();
91: Vector3 facing = player.GetLogicalFacingDirection();
94: transform.position, facing, out Vector3 destination, out _);
105: (pendingDestination - destination).sqrMagnitude > 0.001f)
110: holdTimer = 0f;
118: private void StartTraversal(Vector3 destination, bool lavaJump)
136: traversalTimer = 0f;
144: float t = Mathf.Clamp01(traversalTimer / traversalDuration);
145: float smooth = t * t * (3f - 2f * t);
146: Vector3 position = Vector3.Lerp(traversalStart, traversalDestination, smooth);
150: if (t < 1f) return;
158: holdTimer = 0f;
```

## Assets/_Project/Scripts/Gameplay/Player/Skills/Execution/ArcherSkillExecution.cs

```csharp
10: private void ExecuteArcherSkill1FanShot()
15: poisonDuration = archerSkill1.poisonDuration,
16: poisonTickInterval = archerSkill1.poisonTickInterval,
17: poisonTickDamage = PlayerCombatCalculator.GetSkillDamage(archerSkill1.poisonTickDamage, CurrentPlayerStats)
20: int projectileCount = Mathf.Max(1, archerSkill1.projectileCount);
21: float fanAngle = Mathf.Max(0f, archerSkill1.fanAngle);
22: projectileCount = Mathf.Max(5, projectileCount);
24: float projectileSpeed = Mathf.Max(0.01f, archerSkill1.projectileSpeed);
25: float targetRange = archerSkill1.projectileRange > 0f ? archerSkill1.projectileRange : 4f;
26: float projectileLifeTime = Mathf.Max(0.05f, targetRange / projectileSpeed);
28: Vector3 centerDirection = GetFacingDirection();
29: float startAngle = -fanAngle * 0.5f;
30: float stepAngle = projectileCount > 1 ? fanAngle / (projectileCount - 1) : 0f;
32: for (int i = 0; i < projectileCount; i++)
35: Vector3 shotDirection = Quaternion.AngleAxis(angle, Vector3.up) * centerDirection;
38: archerSkill1.projectilePrefab,
39: archerSkill1.projectileScale,
40: PlayerCombatCalculator.GetSkillDamage(archerSkill1.projectileDamage, CurrentPlayerStats),
48: if (archerSkill1.shootEffectPrefab != null)
51: archerSkill1.shootEffectPrefab,
53: archerSkill1.shootEffectLifetime,
54: 0.25f,
55: new Color(0.7f, 1f, 0.5f, 0.55f));
60: Debug.Log($"Archer Skill E fan-shot fired {projectileCount} piercing projectiles. FanAngle={fanAngle:0.#}, Range={targetRange:0.#}");
65: private IEnumerator ExecuteArcherSkill2()
67: archerSkill2Running = true;
68: Vector3 direction = GetArcherSkill2Direction();
70: float aimDuration = Mathf.Max(0f, archerSkill2.aimDuration);
71: if (aimDuration > 0f)
76: FireArcherSkill2Projectile(direction);
82: archerSkill2Running = false;
86: private Vector3 GetArcherSkill2Direction()
88: Vector3 fallbackDirection = GetFacingDirection();
89: if (!autoTargetForwardEnemyForArcherSkill2)
94: float maxDistance = Mathf.Max(archerSkill2.range, archerSkill2.targetForwardOffset, 1f);
95: if (!TryFindForwardEnemyPoint(maxDistance, archerSkill2AutoTargetAngle, out Vector3 targetPoint))
100: Vector3 toTarget = targetPoint - transform.position;
101: toTarget.y = 0f;
102: if (toTarget.sqrMagnitude <= 0.0001f)
111: private void FireArcherSkill2Projectile(Vector3 direction)
113: Vector3 moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : GetFacingDirection();
114: float range = Mathf.Max(0.5f, archerSkill2.range);
115: float speed = Mathf.Max(0.1f, archerSkill2.projectileTravelSpeed);
117: float lifeTime = archerSkill2.projectileLifeTime > 0f
118: ? archerSkill2.projectileLifeTime
121: Vector3 spawnPosition = GetProjectileSpawnPosition(moveDirection);
122: GameObject projectileObject = CreateArcherSkill2ProjectileObject(spawnPosition, moveDirection);
136: archerSkill2.projectileHitRadius,
142: PlayerCombatCalculator.GetSkillDamage(archerSkill2.projectileDamage, CurrentPlayerStats),
144: Mathf.Max(0.05f, lifeTime),
148: HandleArcherSkill2EnemyHit);
152: private GameObject CreateArcherSkill2ProjectileObject(Vector3 spawnPosition, Vector3 direction)
154: GameObject projectileObject = archerSkill2.projectilePrefab != null
155: ? RuntimePool.Acquire(archerSkill2.projectilePrefab)
157: ArcherSkill2FallbackProjectilePoolName,
159: archerSkill2.projectileScale,
160: new Color(0.95f, 0.72f, 0.25f, 0.9f));
168: projectileObject.transform.rotation = GetArcherSkill2ProjectileRotation(direction, archerSkill2.projectilePrefab != null);
170: if (archerSkill2.projectilePrefab != null)
172: projectileObject.transform.localScale *= Mathf.Max(0.01f, archerSkill2.projectileScale);
176: float thickness = Mathf.Max(0.1f, archerSkill2.projectileVisualThickness);
177: float length = Mathf.Max(0.5f, archerSkill2.projectileVisualLength);
178: projectileObject.transform.localScale = new Vector3(thickness, length * 0.5f, thickness);
181: ConfigureArcherSkill2AfterImage(projectileObject);
186: private void ConfigureArcherSkill2AfterImage(GameObject projectileObject)
202: archerSkill2.enableAfterImage,
203: archerSkill2.afterImageInterval,
204: archerSkill2.afterImageFadeDuration,
205: archerSkill2.afterImageStartAlpha,
206: archerSkill2.afterImageMaxVisibleCount);
210: private Quaternion GetArcherSkill2ProjectileRotation(Vector3 direction, bool usesPrefabVisual)
212: Vector3 safeDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
213: Vector3 sourceAxis = usesPrefabVisual
214: ? GetDirectionReferenceAxis(archerSkill2.prefabDirectionAxis)
215: : Vector3.up;
223: Vector3 offsetEuler = archerSkill2.prefabRotationOffsetEuler;
224: if (offsetEuler.sqrMagnitude > 0.0001f)
229: if (archerSkill2.autoRollToCamera)
231: rotation = AlignProjectileRollToCamera(rotation, safeDirection, archerSkill2.prefabSurfaceNormalAxis);
238: private static Vector3 GetDirectionReferenceAxis(ProjectileDirectionReferenceAxis axis)
243: return Vector3.up;
245: return Vector3.forward;
247: return Vector3.right;
254: Vector3 moveDirection,
263: Vector3 targetNormal = Vector3.ProjectOnPlane(-activeCamera.transform.forward, moveDirection);
264: if (targetNormal.sqrMagnitude <= 0.0001f)
266: targetNormal = Vector3.ProjectOnPlane(activeCamera.transform.up, moveDirection);
267: if (targetNormal.sqrMagnitude <= 0.0001f)
275: Vector3 currentNormal = baseRotation * GetDirectionReferenceAxis(surfaceNormalAxis);
276: currentNormal = Vector3.ProjectOnPlane(currentNormal, moveDirection);
277: if (currentNormal.sqrMagnitude <= 0.0001f)
283: float rollAngle = Vector3.SignedAngle(currentNormal, targetNormal, moveDirection);
288: private void HandleArcherSkill2EnemyHit(EnemyController enemy, Vector3 hitPosition)
301: TriggerArcherSkill2VirusExplosion(enemy, hitPosition);
305: private void TriggerArcherSkill2VirusExplosion(EnemyController target, Vector3 fallbackCenter)
307: Vector3 center = target != null ? GetTargetEffectPosition(target) : fallbackCenter;
308: float effectScaleMultiplier = target != null ? GetTargetEffectScaleMultiplier(target) : 1f;
309: float damage = PlayerCombatCalculator.GetSkillDamage(archerSkill2.virusExplosionDamage, CurrentPlayerStats);
313: archerSkill2.virusExplosionRadius,
317: archerSkill2.virusExplosionEffectPrefab,
319: archerSkill2.virusExplosionEffectLifetime,
320: Mathf.Max(0.25f, archerSkill2.virusExplosionRadius * 0.9f),
321: new Color(1f, 0.45f, 0.2f, 0.45f),
326: Debug.Log($"[Archer Skill R] Virus explosion triggered. Hit={explosionHit}, Damage={damage:0.##}");
331: private bool TryFindForwardEnemyPoint(float maxDistance, float maxAngle, out Vector3 point)
333: point = Vector3.zero;
336: Vector3 forward = GetFacingDirection();
337: Vector3 origin = transform.position;
342: Mathf.Max(0.1f, maxDistance),
349: float halfAngle = Mathf.Max(1f, maxAngle) * 0.5f;
351: for (int i = 0; i < count; i++)
365: Vector3 toEnemy = enemy.transform.position - transform.position;
366: toEnemy.y = 0f;
368: if (distance <= 0.01f || distance > maxDistance)
373: Vector3 toEnemyDir = toEnemy / distance;
374: float angle = Vector3.Angle(forward, toEnemyDir);
380: float forwardScore = Vector3.Dot(forward, toEnemyDir) * 2f;
381: float distanceScore = -Mathf.Abs(distance - archerSkill2.targetForwardOffset) * 0.15f;
```

## Assets/_Project/Scripts/Gameplay/Player/Skills/Execution/MageSkillExecution.cs

```csharp
10: private void ExecuteMageSkill1()
12: Vector3 center = GetSkillCenter(mageSkill1.forwardOffset);
13: float baseDamage = mageSkill1.baseDamage;
14: float bonusMin = Mathf.Min(mageSkill1.additionalDamageMin, mageSkill1.additionalDamageMax);
15: float bonusMax = Mathf.Max(mageSkill1.additionalDamageMin, mageSkill1.additionalDamageMax);
16: if (bonusMax <= 0f && mageSkill1.additionalDamage > 0f)
18: bonusMin = mageSkill1.additionalDamage;
19: bonusMax = mageSkill1.additionalDamage;
24: mageSkill1.radius,
27: float bonusDamage = UnityEngine.Random.Range(bonusMin, bonusMax + 0.001f);
31: status?.ApplyStun(mageSkill1.stunDuration);
35: mageSkill1.areaEffectPrefab,
37: mageSkill1.areaEffectLifetime,
38: mageSkill1.fallbackEffectScale,
39: new Color(0.35f, 0.8f, 1f, 0.45f),
40: GetMageSkill1EffectScaleMultiplier());
44: Debug.Log($"Mage Skill E hit {hitCount} enemies. BonusDamage={bonusMin:0.#}~{bonusMax:0.#}");
49: private float GetMageSkill1EffectScaleMultiplier()
51: float referenceRadius = Mathf.Max(0.01f, mageSkill1.effectReferenceRadius);
52: return Mathf.Max(0.01f, mageSkill1.radius) / referenceRadius;
56: private IEnumerator ExecuteMageSkill2(EnemyController target)
63: float markLifeTime = Mathf.Max(0.1f, Mathf.Min(mageSkill2.markEffectLifetime, mageSkill2.detonationDelay + 0.05f));
65: mageSkill2.markEffectPrefab,
68: mageSkill2.markFallbackEffectScale,
69: new Color(0.8f, 0.95f, 1f, 0.5f),
70: mageSkill2.markHeadOffset);
72: float delay = Mathf.Max(0f, mageSkill2.detonationDelay);
73: if (delay > 0f)
87: Vector3 targetEffectPosition = GetTargetEffectPosition(target);
89: float damage = PlayerCombatCalculator.GetSkillDamage(mageSkill2.baseDamage, CurrentPlayerStats);
92: status?.ApplyDamageTakenIncrease(mageSkill2.damageTakenIncreaseRatio, mageSkill2.damageTakenIncreaseDuration);
95: mageSkill2.explosionEffectPrefab,
97: mageSkill2.explosionEffectLifetime,
98: mageSkill2.fallbackEffectScale,
99: new Color(1f, 0.5f, 0.2f, 0.45f),
104: Debug.Log($"Mage Skill R detonated on {target.name}. Damage={damage:0.##}, Debuff={mageSkill2.damageTakenIncreaseRatio * 100f:0.#}% for {mageSkill2.damageTakenIncreaseDuration:0.##}s");
```

## Assets/_Project/Scripts/Gameplay/Player/Skills/Execution/WarriorSkillExecution.cs

```csharp
10: private void ExecuteWarriorSkill1Cleave()
12: Vector3 center = GetSkillCenter(0f);
13: Vector3 facingDirection = GetFacingDirection();
14: float damage = PlayerCombatCalculator.GetSkillDamage(warriorSkill1.damage, CurrentPlayerStats);
15: float bleedTickDamage = PlayerCombatCalculator.GetSkillDamage(warriorSkill1.bleedTickDamage, CurrentPlayerStats);
18: warriorSkill1.range,
19: warriorSkill1.forwardAngle,
20: warriorSkill1.maxTargets,
25: status?.ApplyBleed(warriorSkill1.bleedDuration, warriorSkill1.bleedTickInterval, bleedTickDamage);
29: warriorSkill1.hitEffectPrefab,
31: warriorSkill1.hitEffectLifetime,
32: warriorSkill1.fallbackEffectScale,
33: new Color(0.85f, 0.1f, 0.1f, 0.6f));
37: StartCoroutine(AnimateWarriorSkillEffect(effect, warriorSkill1.hitEffectLifetime, 1f, 1f));
41: Debug.Log($"Warrior Skill E cleave hit {hitCount} targets. Damage={damage}, Bleed={bleedTickDamage}/s for {warriorSkill1.bleedDuration}s");
46: private IEnumerator ExecuteWarriorSkill2Dash()
48: AudioManager.Instance?.PlaySFX("WarriorSkill2");
51: if (!TryFindForwardEnemyPoint(warriorSkill2.searchRange, warriorSkill2.searchAngle, out Vector3 targetPoint))
54: targetPoint = transform.position + GetFacingDirection() * warriorSkill2.searchRange;
57: Vector3 dashTarget = targetPoint;
60: Vector3 dashDir = (dashTarget - transform.position);
61: dashDir.y = 0f;
64: if (dashDistance > 0.01f)
67: float dashDuration = dashDistance / Mathf.Max(0.1f, warriorSkill2.dashSpeed);
68: float elapsed = 0f;
72: float step = warriorSkill2.dashSpeed * Time.deltaTime;
81: Vector3 hitCenter = GetSkillCenter(0f);
82: ResidueRubble.HitArea(hitCenter, warriorSkill1.range + 1f, Vector3.forward, 180,
83: PlayerCombatCalculator.GetSkillDamage(warriorSkill2.damage, CurrentPlayerStats), enemyMask, ResidueRubble.NextAttackToken());
84: if (TryFindNearestEnemyInRadius(hitCenter, warriorSkill1.range + 1f, out EnemyController hitTarget))
86: float damage = PlayerCombatCalculator.GetSkillDamage(warriorSkill2.damage, CurrentPlayerStats);
89: status?.ApplyStun(warriorSkill2.rootDuration);
91: Vector3 effectPos = GetTargetEffectPosition(hitTarget);
93: warriorSkill2.hitEffectPrefab,
95: warriorSkill2.hitEffectLifetime,
96: warriorSkill2.fallbackEffectScale,
97: new Color(0.9f, 0.2f, 0.05f, 0.7f));
98: StartCoroutine(AnimateWarriorSkillEffect(effect, warriorSkill2.hitEffectLifetime, 0.62f, 1.12f));
101: Debug.Log($"Warrior Skill R hit {hitTarget.name}. Damage={damage}, Root={warriorSkill2.rootDuration}s");
109: private static void OrientSkillEffectToward(GameObject effect, Vector3 origin, Vector3 direction)
111: if (effect == null || direction.sqrMagnitude <= 0.0001f)
119: effect.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg);
123: Vector3 screenOrigin = activeCamera.WorldToScreenPoint(origin);
124: Vector3 screenForward = activeCamera.WorldToScreenPoint(origin + direction.normalized);
125: Vector2 screenDirection = screenForward - screenOrigin;
126: if (screenDirection.sqrMagnitude <= 0.0001f)
131: float screenAngle = Mathf.Atan2(screenDirection.y, screenDirection.x) * Mathf.Rad2Deg;
132: effect.transform.rotation = Quaternion.Euler(0f, 0f, screenAngle);
146: float safeDuration = Mathf.Max(0.05f, duration);
147: Vector3 baseScale = effect.transform.localScale;
150: for (int i = 0; i < renderers.Length; i++)
159: restoredColor.a = 1f;
164: float elapsed = 0f;
167: float normalizedTime = Mathf.Clamp01(elapsed / safeDuration);
168: float easedTime = 1f - Mathf.Pow(1f - normalizedTime, 3f);
170: float alphaMultiplier = 1f - normalizedTime * normalizedTime;
172: for (int i = 0; i < renderers.Length; i++)
191: for (int i = 0; i < renderers.Length; i++)
```

## Assets/_Project/Scripts/Gameplay/Player/Skills/PlayerClassSkillController.cs

```csharp
23: Skill1,
24: Skill2
30: private const string ArcherSkill2FallbackProjectilePoolName = "__ArcherSkill2FallbackCylinder";
52: private class MageSkill1Config
54: public float cooldown = 7f;
55: public float radius = 3.75f;
56: public float forwardOffset = 0f;
57: public float baseDamage = 0f;
58: public float additionalDamage = 0f;
59: public float additionalDamageMin = 4f;
60: public float additionalDamageMax = 6f;
61: public float stunDuration = 1f;
63: public float areaEffectLifetime = 1f;
64: public float fallbackEffectScale = 10f;
67: public float effectReferenceRadius = 5.28f;
71: private class MageSkill2Config
73: public float cooldown = 12f;
74: public float radius = 3.5f;
75: public float forwardOffset = 0f;
76: public float baseDamage = 10f;
77: public float detonationDelay = 3f;
78: public float damageTakenIncreaseRatio = 0.1f;
79: public float damageTakenIncreaseDuration = 3f;
81: public float markEffectLifetime = 3f;
82: public float markFallbackEffectScale = 0.8f;
83: public float markHeadOffset = 0.2f;
85: public float effectReferenceTargetHeight = 1.1f;
86: public float effectSizeMultiplier = 1f;
87: public float effectMinScaleMultiplier = 0.7f;
88: public float effectMaxScaleMultiplier = 2.5f;
90: public float explosionEffectLifetime = 1f;
91: public float fallbackEffectScale = 3f;
95: private class ArcherSkill1Config
97: public float cooldown = 6f;
98: public int projectileCount = 5;
99: public float fanAngle = 55f;
100: public float projectileDamage = 1f;
101: public float projectileSpeed = 16f;
102: public float projectileRange = 6f;
103: public float projectileLifeTime = 2f;
105: public float projectileScale = 0.25f;
106: public float poisonDuration = 3f;
107: public float poisonTickInterval = 1f;
108: public float poisonTickDamage = 1f;
110: public float shootEffectLifetime = 0.4f;
114: private class ArcherSkill2Config
116: public float cooldown = 10f;
117: public float aimDuration = 0.5f;
118: public float range = 10f;
120: public float projectileHitRadius = 0.8f;
121: public float projectileVisualLength = 4f;
122: public float projectileVisualThickness = 0.8f;
123: public float targetForwardOffset = 6f;
124: public float projectileDamage = 4f;
125: public float projectileTravelSpeed = 16f;
126: public float projectileLifeTime = 0f;
128: public float projectileScale = 1f;
130: public Vector3 prefabRotationOffsetEuler = Vector3.zero;
134: public float virusExplosionDamage = 5f;
136: public float virusExplosionRadius = 1.8f;
140: public float virusExplosionEffectLifetime = 1f;
142: public float afterImageInterval = 0.03f;
143: public float afterImageFadeDuration = 0.15f;
144: public float afterImageStartAlpha = 0.4f;
145: public int afterImageMaxVisibleCount = 3;
149: private class WarriorSkill1Config
151: public float cooldown = 4f;
152: public float range = 4.125f;
153: public float forwardAngle = 140f;
154: public int maxTargets = 8;
155: public float damage = 4f;
156: public float bleedDuration = 2f;
157: public float bleedTickInterval = 1f;
158: public float bleedTickDamage = 1f;
160: public float hitEffectLifetime = 0.35f;
161: public float fallbackEffectScale = 0.8f;
165: private class WarriorSkill2Config
167: public float cooldown = 8f;
168: public float searchRange = 6f;   // 돌진 대상 탐색 범위
169: public float dashSpeed = 18f;    // 돌진 속도
170: public float damage = 8f;
171: public float rootDuration = 2f;  // 구속(이동불가) 시간
172: public float searchAngle = 90f;  // 전방 탐색 각도
174: public float hitEffectLifetime = 0.5f;
175: public float fallbackEffectScale = 1.0f;
182: [SerializeField] private LayerMask enemyMask = ~0;
183: [SerializeField] private int overlapBufferSize = 32;
185: [SerializeField] private float projectileSpawnOffset = 0.65f;
186: [SerializeField] private float projectileSpawnHeight = 1f;
187: [SerializeField] private float skillVerticalOffset = 2f;
188: [SerializeField] private float skillHitHeightOffset = 0.75f;
189: [SerializeField] private float skillHitVerticalHalfHeight = 4f;
190: [SerializeField, Min(1)] private int maxAreaSkillHitTargets = 32;
191: [SerializeField] private int skillEffectSortingOrder = 5200;
195: [SerializeField] private MageSkill1Config mageSkill1 = new MageSkill1Config();
196: [SerializeField] private MageSkill2Config mageSkill2 = new MageSkill2Config();
199: [SerializeField] private ArcherSkill1Config archerSkill1 = new ArcherSkill1Config();
200: [SerializeField] private ArcherSkill2Config archerSkill2 = new ArcherSkill2Config();
203: [SerializeField] private WarriorSkill1Config warriorSkill1 = new WarriorSkill1Config();
204: [SerializeField] private WarriorSkill2Config warriorSkill2 = new WarriorSkill2Config();
205: [SerializeField] private bool autoTargetForwardEnemyForArcherSkill2 = true;
206: [SerializeField] private float archerSkill2AutoTargetAngle = 90f;
210: [SerializeField] private float testSkill1Cooldown = 1f;
211: [SerializeField] private float testSkill2Cooldown = 3f;
213: private readonly List<AreaSkillCandidate> areaSkillCandidates = new List<AreaSkillCandidate>(64);
218: private float nextSkill1ReadyTime;
219: private float nextSkill2ReadyTime;
221: private bool archerSkill2Running;
233: if (enemyMask.value == 0)
235: enemyMask = ~0;
263: if (input.Skill1Action.WasPressedThisFrame())
265: TryUseSkill1();
268: if (input.Skill2Action.WasPressedThisFrame())
270: TryUseSkill2();
299: return GetRemainingCooldown(slot) > 0f;
305: float nextReady = slot == SkillSlot.Skill1 ? nextSkill1ReadyTime : nextSkill2ReadyTime;
306: return Mathf.Max(0f, nextReady - now);
313: PlayerClassType.Mage => slot == SkillSlot.Skill1 ? Mathf.Max(0f, mageSkill1.cooldown) : Mathf.Max(0f, mageSkill2.cooldown),
314: PlayerClassType.Archer => slot == SkillSlot.Skill1 ? Mathf.Max(0f, archerSkill1.cooldown) : Mathf.Max(0f, archerSkill2.cooldown),
315: PlayerClassType.Warrior => slot == SkillSlot.Skill1 ? Mathf.Max(0f, warriorSkill1.cooldown) : Mathf.Max(0f, warriorSkill2.cooldown),
316: _ => 0f
343: if (!Application.isFocused || Time.timeSinceLevelLoad < 0.5f)
363: float skill1Cooldown = Mathf.Max(0f, testSkill1Cooldown);
364: float skill2Cooldown = Mathf.Max(0f, testSkill2Cooldown);
366: mageSkill1.cooldown = skill1Cooldown;
367: archerSkill1.cooldown = skill1Cooldown;
368: warriorSkill1.cooldown = skill1Cooldown;
369: mageSkill2.cooldown = skill2Cooldown;
370: archerSkill2.cooldown = skill2Cooldown;
371: warriorSkill2.cooldown = skill2Cooldown;
374: private void TryUseSkill1()
376: if (!CanUseSkillSlot(SkillSlot.Skill1))
384: if (!TryStartCooldown(ref nextSkill1ReadyTime, mageSkill1.cooldown, "Mage Skill E", SkillSlot.Skill1))
389: AudioManager.Instance?.PlaySFX("MageSkill1");
390: ExecuteMageSkill1();
394: if (!TryStartCooldown(ref nextSkill1ReadyTime, archerSkill1.cooldown, "Archer Skill E", SkillSlot.Skill1))
399: AudioManager.Instance?.PlaySFX("ArcherSkill1");
400: ExecuteArcherSkill1FanShot();
404: if (!TryStartCooldown(ref nextSkill1ReadyTime, warriorSkill1.cooldown, "Warrior Skill E", SkillSlot.Skill1))
409: AudioManager.Instance?.PlaySFX("WarriorSkill1");
410: ExecuteWarriorSkill1Cleave();
415: private void TryUseSkill2()
417: if (!CanUseSkillSlot(SkillSlot.Skill2))
425: Vector3 mageSkill2Center = GetSkillCenter(mageSkill2.forwardOffset);
426: if (!TryFindNearestEnemyInRadius(mageSkill2Center, mageSkill2.radius, out EnemyController mageSkill2Target))
436: if (!TryStartCooldown(ref nextSkill2ReadyTime, mageSkill2.cooldown, "Mage Skill R", SkillSlot.Skill2))
441: AudioManager.Instance?.PlaySFX("MageSkill2");
442: StartCoroutine(ExecuteMageSkill2(mageSkill2Target));
446: if (archerSkill2Running)
451: if (!TryStartCooldown(ref nextSkill2ReadyTime, archerSkill2.cooldown, "Archer Skill R", SkillSlot.Skill2))
456: AudioManager.Instance?.PlaySFX("ArcherSkill2");
457: StartCoroutine(ExecuteArcherSkill2());
461: if (!TryStartCooldown(ref nextSkill2ReadyTime, warriorSkill2.cooldown, "Warrior Skill R", SkillSlot.Skill2))
466: StartCoroutine(ExecuteWarriorSkill2Dash());
479: float remain = Mathf.Max(0f, nextReadyTime - now);
480: Debug.Log($"[{label}] Cooldown: {remain:0.00}s");
495: int skillSlotIndex = slot == SkillSlot.Skill1 ? 1 : 2;
505: string skillKey = slot == SkillSlot.Skill1 ? "E" : "R";
514: nextSkill1ReadyTime = 0f;
515: nextSkill2ReadyTime = 0f;
516: archerSkill2Running = false;
518: CooldownReset?.Invoke(SkillSlot.Skill1);
519: CooldownReset?.Invoke(SkillSlot.Skill2);
```

## Assets/_Project/Scripts/Gameplay/Player/Skills/ProjectileAfterImageTrail.cs

```csharp
9: private const string AfterImagePoolName = "__ArcherSkill2AfterImageSprite";
10: private const float DefaultSpawnInterval = 0.03f;
11: private const float DefaultFadeDuration = 0.15f;
12: private const float DefaultStartAlpha = 0.4f;
13: private const int DefaultMaxVisibleCount = 3;
20: [SerializeField] private float movementThreshold = 0.0005f;
22: private readonly List<ProjectileAfterImageGhost> activeGhosts = new List<ProjectileAfterImageGhost>(DefaultMaxVisibleCount + 1);
26: private Vector3 lastPosition;
39: spawnInterval = Mathf.Max(0.005f, interval);
40: fadeDuration = Mathf.Max(0.01f, fade);
41: startAlpha = Mathf.Clamp01(alpha);
42: maxVisibleCount = Mathf.Max(1, maxVisible);
44: spawnTimer = 0f;
53: spawnTimer = 0f;
59: spawnTimer = 0f;
68: spawnTimer = 0f;
73: Vector3 currentPosition = transform.position;
85: spawnTimer = 0f;
158: for (int i = activeGhosts.Count - 1; i >= 0; i--)
170: if (activeGhosts.Count == 0)
175: ProjectileAfterImageGhost oldestGhost = activeGhosts[0];
176: activeGhosts.RemoveAt(0);
190: for (int i = 0; i < activeGhosts.Count; i++)
231: Vector3 worldPosition,
257: color.a = Mathf.Clamp01(startAlpha);
260: fadeDuration = Mathf.Max(0.01f, duration);
286: float t = Mathf.Clamp01(elapsed / fadeDuration);
289: color.a = Mathf.Lerp(initialAlpha, 0f, t);
292: if (t >= 1f)
```

## Assets/_Project/Scripts/Gameplay/Player/Skills/Runtime/SkillProjectilesAndEffects.cs

```csharp
16: Vector3 direction,
30: Vector3 direction,
32: Vector3? worldSpawnPosition,
35: Vector3 spawnPosition = worldSpawnPosition ?? GetProjectileSpawnPosition(direction);
53: private GameObject CreateProjectileObject(GameObject prefab, Vector3 position, float fallbackScale)
61: new Color(0.75f, 1f, 0.65f, 0.9f));
98: private Vector3 GetProjectileSpawnPosition(Vector3 direction)
100: Vector3 basePos = skillSpawnPoint != null ? skillSpawnPoint.position : transform.position;
107: private Vector3 GetSkillCenter(float forwardOffset)
109: Vector3 center = transform.position + GetFacingDirection() * forwardOffset;
115: private Vector3 GetFacingDirection()
119: return Vector3.forward;
122: Vector3 direction = playerController.GetLogicalFacingDirection();
123: direction.y = 0f;
124: return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
150: skillSpawnPoint.localPosition = Vector3.zero;
187: SpawnSkillEffect(prefab, GetSkillCenter(0f), lifeTime, fallbackScale, fallbackColor);
216: mageSkill2.scaleEffectByTargetSize,
217: mageSkill2.effectReferenceTargetHeight,
218: mageSkill2.effectSizeMultiplier,
219: mageSkill2.effectMinScaleMultiplier,
220: mageSkill2.effectMaxScaleMultiplier,
229: Vector3 position,
233: float scaleMultiplier = 1f)
235: float safeScaleMultiplier = Mathf.Max(0.01f, scaleMultiplier);
277: if (renderers.Length == 0)
283: for (int i = 0; i < renderers.Length; i++)
296: for (int i = 0; i < renderers.Length; i++)
304: int relativeOrder = Mathf.Max(0, renderer.sortingOrder - minimumOrder);
358: visual.transform.localScale = Vector3.one * Mathf.Max(0.05f, scale);
387: autoReturn?.Schedule(Mathf.Max(0.1f, lifeTime));
393: int desiredSize = Mathf.Max(1, overlapBufferSize);
405: Gizmos.matrix = Matrix4x4.identity;
410: DrawSkillRadius(mageSkill1.forwardOffset, mageSkill1.radius, new Color(0.3f, 0.9f, 1f, 0.4f));
411: DrawSkillRadius(0f, mageSkill2.radius, new Color(1f, 0.5f, 0.2f, 0.4f));
415: DrawSkillRadius(archerSkill2.targetForwardOffset, archerSkill2.projectileHitRadius, new Color(0.4f, 1f, 0.4f, 0.6f));
416: DrawArcherSkill2LineGizmo(new Color(1f, 0.45f, 0.25f, 0.65f));
420: DrawSkillArc(warriorSkill1.range, warriorSkill1.forwardAngle, new Color(0.9f, 0.1f, 0.1f, 0.65f));
421: DrawSkillRadius(0f, warriorSkill2.searchRange, new Color(1f, 0.4f, 0.0f, 0.3f));
429: Vector3 direction = Application.isPlaying ? GetFacingDirection() : Vector3.forward;
430: if (direction.sqrMagnitude < 0.0001f)
432: direction = Vector3.forward;
435: Vector3 center = transform.position + direction.normalized * forwardOffset;
439: Gizmos.DrawWireSphere(center, Mathf.Max(0f, radius));
444: Vector3 forward = Application.isPlaying ? GetFacingDirection() : Vector3.forward;
445: if (forward.sqrMagnitude < 0.0001f)
447: forward = Vector3.forward;
450: Vector3 center = transform.position;
452: float safeRadius = Mathf.Max(0f, radius);
453: float halfAngle = Mathf.Clamp(angle * 0.5f, 0.5f, 180f);
454: const int segments = 24;
457: Vector3 leftDirection = Quaternion.AngleAxis(-halfAngle, Vector3.up) * forward.normalized;
458: Vector3 previousPoint = center + leftDirection * safeRadius;
461: for (int i = 1; i <= segments; i++)
464: Vector3 currentDirection = Quaternion.AngleAxis(currentAngle, Vector3.up) * forward.normalized;
465: Vector3 currentPoint = center + currentDirection * safeRadius;
474: private void DrawArcherSkill2LineGizmo(Color color)
476: Vector3 direction = Application.isPlaying ? GetArcherSkill2Direction() : Vector3.forward;
477: if (direction.sqrMagnitude < 0.0001f)
479: direction = Vector3.forward;
482: Vector3 start = transform.position + direction.normalized * projectileSpawnOffset;
484: Vector3 end = start + direction.normalized * Mathf.Max(0.5f, archerSkill2.range);
488: Gizmos.DrawWireSphere(end, Mathf.Max(0.05f, archerSkill2.projectileHitRadius));
```

## Assets/_Project/Scripts/Gameplay/Player/Skills/Runtime/SkillTargeting.cs

```csharp
11: private int ApplyAreaSkill(Vector3 center, float radius, Action<EnemyController> apply, float propDamage = 0)
13: ResidueRubble.HitArea(center, Mathf.Max(0, radius), Vector3.forward, 180, propDamage, enemyMask, ResidueRubble.NextAttackToken());
14: CollectBodyTargets(center, radius, Vector3.forward, 180);
18: private int ApplyForwardArcSkill(Vector3 center, float radius, float forwardAngle, int maxTargets,
19: Action<EnemyController> apply, float propDamage = 0)
21: Vector3 forward = GetFacingDirection();
22: float halfAngle = Mathf.Clamp(forwardAngle * .5f, .5f, 180);
23: ResidueRubble.HitArea(center, Mathf.Max(0, radius), forward, halfAngle, propDamage, enemyMask, ResidueRubble.NextAttackToken());
28: private void CollectBodyTargets(Vector3 center, float radius, Vector3 forward, float halfAngle)
32: for (int i = 0; i < enemies.Count; i++)
44: int count = Mathf.Min(Mathf.Max(1, limit), areaSkillCandidates.Count);
45: for (int i = 0; i < count; i++) apply?.Invoke(areaSkillCandidates[i].Enemy);
49: private bool TryFindNearestEnemyInForwardArc(Vector3 center, float radius, float forwardAngle, out EnemyController nearestEnemy)
51: CollectBodyTargets(center, radius, GetFacingDirection(), Mathf.Clamp(forwardAngle * .5f, .5f, 180));
52: nearestEnemy = areaSkillCandidates.Count > 0 ? areaSkillCandidates[0].Enemy : null;
56: private bool TryFindNearestEnemyInRadius(Vector3 center, float radius, out EnemyController nearestEnemy)
58: CollectBodyTargets(center, radius, Vector3.forward, 180);
59: nearestEnemy = areaSkillCandidates.Count > 0 ? areaSkillCandidates[0].Enemy : null;
63: private Vector3 GetTargetEffectPosition(EnemyController target)
67: return GetSkillCenter(0f);
81: if (!mageSkill2.scaleEffectByTargetSize || target == null)
83: return 1f;
86: float minMultiplier = Mathf.Max(0.05f, mageSkill2.effectMinScaleMultiplier);
87: float maxMultiplier = Mathf.Max(minMultiplier, mageSkill2.effectMaxScaleMultiplier);
88: float referenceHeight = Mathf.Max(0.01f, mageSkill2.effectReferenceTargetHeight);
89: float sizeMultiplier = Mathf.Max(0.01f, mageSkill2.effectSizeMultiplier);
96: float targetHeight = Mathf.Max(0.01f, targetBounds.size.y);
```

## Assets/_Project/Scripts/Gameplay/Player/Skills/SkillProjectile.cs

```csharp
26: private const int HitBufferSize = 8;
28: [SerializeField] private float speed = 14f;
29: [SerializeField] private float lifeTime = 3f;
30: [SerializeField] private LayerMask targetMask = ~0;
33: [SerializeField] private float hitEffectLifetime = 1f;
34: [SerializeField] private float hitCheckRadius = 0.35f;
35: [SerializeField] private float hitCheckHeightOffset = 0.75f;
36: [SerializeField] private float hitCheckVerticalHalfHeight = 2.5f;
38: private Vector3 moveDirection = Vector3.forward;
47: private int visualSortingOrder = 5200;
50: private Action<EnemyController, Vector3> onEnemyHit;
54: Vector3 direction,
61: Action<EnemyController, Vector3> onEnemyHit = null)
63: moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
64: keepFlightHeight = Mathf.Abs(moveDirection.y) <= 0.0001f;
67: speed = Mathf.Max(0.01f, projectileSpeed);
68: lifeTime = Mathf.Max(0.05f, projectileLifeTime);
83: hitCheckRadius = Mathf.Max(0.05f, radius);
85: hitCheckVerticalHalfHeight = Mathf.Max(0.05f, verticalHalfHeight);
104: Vector3 nextPosition = transform.position + moveDirection * speed * Time.deltaTime;
137: float radius = Mathf.Max(.05f, Mathf.Max(hitCheckRadius, PlayerAttackGeometry.ColliderRadius(hitCollider))) + PlayerAttackGeometry.Padding;
138: Vector3 hitCenter = transform.position;
140: float halfHeight = Mathf.Max(0.05f, hitCheckVerticalHalfHeight);
141: Vector3 capsuleTop = hitCenter + Vector3.up * halfHeight;
142: Vector3 capsuleBottom = hitCenter - Vector3.up * halfHeight;
152: for (int i = 0; i < hitCount; i++)
174: if ((targetMask.value & (1 << other.gameObject.layer)) == 0)
259: private void SpawnHitEffect(Vector3 position)
275: RuntimePool.EnsureAutoReturn(effect)?.Schedule(Mathf.Max(0.1f, hitEffectLifetime));
286: if (renderers.Length == 0)
292: for (int i = 0; i < renderers.Length; i++)
305: for (int i = 0; i < renderers.Length; i++)
313: int relativeOrder = Mathf.Max(0, renderer.sortingOrder - minimumOrder);
```

## Assets/_Project/Scripts/Gameplay/Player/StatusEffects/PlayerStatusEffectController.cs

```csharp
36: if (slowRatio <= 0f || duration <= 0f)
53: ratio = Mathf.Clamp01(slowRatio),
64: if (reductionRatio <= 0f || duration <= 0f)
81: ratio = Mathf.Clamp01(reductionRatio),
92: Mathf.Max(0f, tickDamage * DifficultyBalanceService.GetIncomingDamageMultiplier(null)), null));
97: if (duration <= 0f || tickInterval <= 0f || tickDamage.Amount <= 0f)
104: Mathf.Max(0.01f, duration),
105: Mathf.Max(0.01f, tickInterval),
140: while (moveSpeedSlows.Count > 0 || attackPowerReductions.Count > 0)
143: for (int i = 0; i < moveSpeedSlows.Count; i++)
148: for (int i = 0; i < attackPowerReductions.Count; i++)
153: float waitTime = Mathf.Max(0.02f, nextEndTime - Time.time);
164: float elapsed = 0f;
189: for (int i = 0; i < damageOverTimeRoutines.Count; i++)
203: for (int i = moveSpeedSlows.Count - 1; i >= 0; i--)
211: for (int i = attackPowerReductions.Count - 1; i >= 0; i--)
226: float strongestSlow = 0f;
227: for (int i = 0; i < moveSpeedSlows.Count; i++)
232: float strongestAttackReduction = 0f;
233: for (int i = 0; i < attackPowerReductions.Count; i++)
238: if (strongestSlow > 0f)
247: if (strongestAttackReduction > 0f)
```

## Assets/_Project/Scripts/UI/HUD/PlayerHeartUI.cs

```csharp
32: [SerializeField] private int canvasSortingOrder = 60;
33: [SerializeField] private Vector2 canvasReferenceResolution = new Vector2(1920f, 1080f);
34: [SerializeField] private Vector2 anchoredPosition = new Vector2(24f, -24f);
35: [SerializeField] private Vector2 heartSize = new Vector2(32f, 32f);
36: [SerializeField] private float spacing = 4f;
39: [SerializeField] private int healthPerHeart = 2;
40: [SerializeField] private int maxVisibleHearts = 12;
41: [SerializeField] private Color emptyHeartTint = new Color(1f, 1f, 1f, 0.35f);
47: private int cachedHeartCount = -1;
48: private Vector2 cachedAppliedHeartSize = new Vector2(-1f, -1f);
49: private float cachedAppliedSpacing = -1f;
50: private Vector2 cachedAppliedAnchorPos = new Vector2(float.MinValue, float.MinValue);
228: heartContainer.anchorMin = new Vector2(0f, 1f);
229: heartContainer.anchorMax = new Vector2(0f, 1f);
230: heartContainer.pivot = new Vector2(0f, 1f);
232: heartContainer.localScale = Vector3.one;
268: if (heartContainer == null || maxHealth <= 0f)
273: int valuePerHeart = Mathf.Max(1, healthPerHeart);
274: int maxHealthUnits = Mathf.Max(0, Mathf.RoundToInt(maxHealth));
275: int currentHealthUnits = Mathf.Clamp(Mathf.RoundToInt(currentHealth), 0, maxHealthUnits);
276: int totalHearts = Mathf.Max(1, (maxHealthUnits + valuePerHeart - 1) / valuePerHeart);
277: if (maxVisibleHearts > 0)
285: int filledUnits = Mathf.Clamp(currentHealthUnits, 0, totalHearts * valuePerHeart);
287: for (int i = 0; i < totalHearts; i++)
289: int units = Mathf.Clamp(filledUnits - (i * valuePerHeart), 0, valuePerHeart);
301: for (int i = heartContainer.childCount - 1; i >= 0; i--)
307: for (int i = 0; i < count; i++)
309: GameObject heartObject = new GameObject($"Heart_{i + 1}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
314: rect.localScale = Vector3.one;
321: layout.flexibleWidth = 0f;
322: layout.flexibleHeight = 0f;
330: cachedAppliedHeartSize = new Vector2(-1f, -1f);
362: if (heartContainer == null || heartImages.Count == 0)
367: Vector2 clampedSize = new Vector2(
368: Mathf.Max(1f, heartSize.x),
369: Mathf.Max(1f, heartSize.y));
376: for (int i = 0; i < heartImages.Count; i++)
386: rect.localScale = Vector3.one;
398: layout.flexibleWidth = 0f;
399: layout.flexibleHeight = 0f;
413: if (units >= 2)
420: if (units == 1)
433: if (healthUnitsInHeart <= 0)
435: return 0;
438: return healthUnitsInHeart >= valuePerHeart ? 2 : 1;
```

## Assets/_Project/Scripts/World/Biomes/BossArena/MidBossArenaController.cs

```csharp
17: private readonly List<Vector2Int> blockedBoundaryCells = new List<Vector2Int>();
18: private readonly List<Vector2Int> approachBoundaryCells = new List<Vector2Int>();
32: private Vector2Int centerGrid;
33: private Vector2Int arenaSize;
54: arenaSize = new Vector2Int(
55: Mathf.Max(8, arenaConfig.arenaSize.x),
56: Mathf.Max(8, arenaConfig.arenaSize.y));
145: Vector2 playerHalfExtents = GetPlayerClampExtents(player);
216: List<SpriteRenderer> renderers = new List<SpriteRenderer>(2);
261: AudioManager.Instance?.PlayTimedSFX("BossSpawn", 5f);
274: float bestArea = -1f;
275: for (int i = 0; i < renderers.Length; i++)
309: Vector3 bossSpawnPosition = ResolveArenaCenterWorld(bossRule.heightOffset);
325: private void ConfigureBiomeSpecificBossPattern(EnemyController boss, Vector3 bossSpawnPosition)
504: for (int i = 0; i < activeContactDamage.Count; i++)
533: for (int i = 0; i < renderers.Length; i++)
563: Vector3 center = ResolveArenaCenterWorld(bossRule.heightOffset);
646: Vector3 returnPortalPosition = ResolveReturnPortalPosition();
647: Vector3 bossDeathPos = activeBoss != null
710: AudioManager.Instance?.PlayTimedSFX("LiverBossImpact", 0.8f);
735: private Vector3 ResolveReturnPortalPosition()
742: float heightOffset = returnPortalConfig != null ? returnPortalConfig.heightOffset : 0f;
746: private void SpawnReturnPortal(Vector3 portalPos)
761: : Vector3.one;
771: : new Color(0.6f, 0.2f, 1f, 0.85f);
774: : arenaConfig != null ? arenaConfig.sortingOrder : 3500;
795: col.size = GetSafeColliderSize(returnPortalConfig != null ? returnPortalConfig.colliderSize : new Vector3(2f, 2f, 2f));
796: col.center = returnPortalConfig != null ? returnPortalConfig.colliderCenter : Vector3.zero;
803: private void SpawnBonusItemDrop(Vector3 bossDeathPos)
831: float tileSize = Mathf.Max(0.01f, biome.TileSize);
834: 2,
835: Mathf.Max(2, arenaSize.x - 2));
838: ArenaWorldBounds arenaBounds = GetArenaWorldBounds(Vector2.zero);
842: trigger.size = new Vector3(triggerWidth, arenaConfig.triggerHeight, triggerDepth);
843: trigger.center = new Vector3(
844: 0f,
846: southInnerEdgeLocal + triggerDepth * 0.5f);
864: public bool ContainsWorldPosition(Vector3 worldPosition)
866: return GetArenaWorldBounds(Vector2.zero).ContainsOuter(worldPosition);
870: Vector3 currentPosition,
871: Vector3 desiredPosition,
872: Vector2 playerHalfExtents)
874: for (int i = 0; i < ActiveArenas.Count; i++)
888: Vector3 currentPosition,
889: Vector3 desiredPosition,
891: out Vector3 clampedPosition)
904: public static bool TryClampPositionInsideLockedArena(Vector3 position, float margin, out Vector3 clampedPosition)
917: public static bool IsPlayerInsideLockedArena(Vector3 playerPosition)
919: for (int i = 0; i < ActiveArenas.Count; i++)
936: private static MidBossArenaController FindLockedArenaForMovement(Vector3 currentPosition, Vector3 desiredPosition)
940: for (int i = 0; i < ActiveArenas.Count; i++)
959: return CountLockedArenas() == 1 ? fallback : null;
962: private static MidBossArenaController FindLockedArenaForPosition(Vector3 position)
964: for (int i = 0; i < ActiveArenas.Count; i++)
984: int count = 0;
986: for (int i = 0; i < ActiveArenas.Count; i++)
998: return count == 1 ? single : null;
1003: int count = 0;
1004: for (int i = 0; i < ActiveArenas.Count; i++)
1016: private Vector3 ClampToPlayableBounds(Vector3 worldPosition, float margin)
1018: float safeMargin = Mathf.Max(0f, margin);
1019: return ClampToPlayableBounds(worldPosition, new Vector2(safeMargin, safeMargin));
1022: private Vector3 ClampToPlayableBounds(Vector3 worldPosition, Vector2 margins)
1036: Vector3 currentPosition,
1037: Vector3 desiredPosition,
1038: Vector2 playerHalfExtents)
1045: IReadOnlyList<Vector2Int> solidCells = arenaLocked
1048: if (solidCells == null || solidCells.Count == 0)
1053: Vector2 safeHalfExtents = new Vector2(
1054: Mathf.Max(0f, playerHalfExtents.x),
1055: Mathf.Max(0f, playerHalfExtents.y));
1065: if (currentOverlap > 0f)
1067: ArenaWorldBounds rawBounds = GetArenaWorldBounds(Vector2.zero);
1076: return desiredOverlap + 0.000001f < currentOverlap;
1079: if (desiredOverlap > 0f)
1092: Vector3 position,
1093: Vector2 playerHalfExtents,
1094: IReadOnlyList<Vector2Int> solidCells)
1096: float tileSize = Mathf.Max(0.01f, biome.TileSize);
1097: float score = 0f;
1098: for (int i = 0; i < solidCells.Count; i++)
1100: Vector2Int cell = solidCells[i];
1101: Vector3 center = biome.GridToWorld(cell.x, cell.y);
1102: float minX = center.x - tileSize * 0.5f - playerHalfExtents.x;
1103: float maxX = center.x + tileSize * 0.5f + playerHalfExtents.x;
1104: float minZ = center.z - tileSize * 0.5f - playerHalfExtents.y;
1105: float maxZ = center.z + tileSize * 0.5f + playerHalfExtents.y;
1123: Vector3 start,
1124: Vector3 end,
1125: Vector2 playerHalfExtents,
1126: IReadOnlyList<Vector2Int> solidCells)
1128: float tileSize = Mathf.Max(0.01f, biome.TileSize);
1129: Vector3 delta = end - start;
1130: for (int i = 0; i < solidCells.Count; i++)
1132: Vector2Int cell = solidCells[i];
1133: Vector3 center = biome.GridToWorld(cell.x, cell.y);
1134: float enter = 0f;
1135: float exit = 1f;
1139: center.x - tileSize * 0.5f - playerHalfExtents.x,
1140: center.x + tileSize * 0.5f + playerHalfExtents.x,
1146: center.z - tileSize * 0.5f - playerHalfExtents.y,
1147: center.z + tileSize * 0.5f + playerHalfExtents.y,
1154: if (exit - enter > 0.0001f)
1171: if (Mathf.Abs(delta) <= 0.000001f)
1196: Vector2 margins = GetPlayerClampExtents(player);
1197: Vector3 currentPosition = player.transform.position;
1198: Vector3 clampedPosition = ClampToPlayableBounds(currentPosition, margins);
1199: Vector3 planarDelta = clampedPosition - currentPosition;
1200: planarDelta.y = 0f;
1201: if (planarDelta.sqrMagnitude <= 0.000001f)
1209: private static Vector2 GetPlayerClampExtents(PlayerController player)
1222: return new Vector2(0.5f, 0.5f);
1225: return new Vector2(
1226: Mathf.Max(0.1f, hitCollider.bounds.extents.x),
1227: Mathf.Max(0.1f, hitCollider.bounds.extents.z));
1238: int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
1255: blockedBoundaryCells.Add(new Vector2Int(x, y));
1259: approachBoundaryCells.Add(new Vector2Int(x, y));
1267: if (approachBoundaryActive || biome == null || approachBoundaryCells.Count == 0)
1338: int availableWidth = Mathf.Max(2, sideMax - sideMin + 1 - thickness * 2);
1339: int openingWidth = Mathf.Clamp(presentation.entranceWidthInCells, 2, availableWidth);
1340: openingStart = sideMin + (sideMax - sideMin + 1 - openingWidth) / 2;
1341: openingEnd = openingStart + openingWidth - 1;
1351: minX = centerGrid.x - arenaSize.x / 2 + lockInset;
1352: minY = centerGrid.y - arenaSize.y / 2 + lockInset;
1353: maxX = centerGrid.x - arenaSize.x / 2 + arenaSize.x - 1 - lockInset;
1354: maxY = centerGrid.y - arenaSize.y / 2 + arenaSize.y - 1 - lockInset;
1357: private ArenaWorldBounds GetArenaWorldBounds(Vector2 playerHalfExtents)
1360: float tileSize = Mathf.Max(0.01f, biome.TileSize);
1361: int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
1362: Vector3 minimumCellCenter = biome.GridToWorld(minX, minY);
1363: Vector3 maximumCellCenter = biome.GridToWorld(maxX, maxY);
1365: float outerMinX = minimumCellCenter.x - tileSize * 0.5f;
1366: float outerMaxX = maximumCellCenter.x + tileSize * 0.5f;
1367: float outerMinZ = minimumCellCenter.z - tileSize * 0.5f;
1368: float outerMaxZ = maximumCellCenter.z + tileSize * 0.5f;
1369: float playableMinX = outerMinX + thickness * tileSize + Mathf.Max(0f, playerHalfExtents.x);
1370: float playableMaxX = outerMaxX - thickness * tileSize - Mathf.Max(0f, playerHalfExtents.x);
1371: float playableMinZ = outerMinZ + thickness * tileSize + Mathf.Max(0f, playerHalfExtents.y);
1372: float playableMaxZ = outerMaxZ - thickness * tileSize - Mathf.Max(0f, playerHalfExtents.y);
1376: float midpoint = (outerMinX + outerMaxX) * 0.5f;
1383: float midpoint = (outerMinZ + outerMaxZ) * 0.5f;
1401: int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
1402: int configuredInset = Mathf.Max(0, arenaConfig.lockBoundaryInsetInCells);
1403: int maxInset = Mathf.Max(0, (Mathf.Min(arenaSize.x, arenaSize.y) - thickness * 2 - 2) / 2);
1407: private Vector3 ResolveArenaCenterWorld(float heightOffset = 0f)
1409: Vector3 center = biome.GridToWorldWithHeight(centerGrid.x, centerGrid.y, heightOffset);
1410: float halfCell = biome.TileSize * 0.5f;
1411: if (arenaSize.x % 2 == 0)
1416: if (arenaSize.y % 2 == 0)
1424: private Vector2Int ResolveCenterGrid()
1431: return new Vector2Int(biome.MapWidth / 2, biome.MapHeight / 2);
1456: && availableEnemyRules.Count > 0)
1458: int index = Mathf.Clamp(bossDefinition.fallbackEnemyRuleIndex, 0, availableEnemyRules.Count - 1);
1511: if (boss.idleSprites == null || boss.idleSprites.Length == 0)
1516: if (boss.moveSprites == null || boss.moveSprites.Length == 0)
1521: if (boss.attackSprites == null || boss.attackSprites.Length == 0)
1526: if (boss.deathSprites == null || boss.deathSprites.Length == 0)
1547: maxAlive = 1,
1548: activationRadius = 0f,
1549: respawnCooldown = 0f,
1550: spawnRadius = 0f,
1563: separationDistance = 0f,
1564: separationStrength = 0f,
1620: boss.maxHealth *= Mathf.Max(0.01f, bossDefinition.maxHealthMultiplier);
1621: boss.attackDamage *= Mathf.Max(0.01f, bossDefinition.attackDamageMultiplier);
1622: boss.moveSpeed *= Mathf.Max(0.01f, bossDefinition.moveSpeedMultiplier);
1627: Vector3 scaleMultiplier = GetSafeScaleMultiplier(bossDefinition.scaleMultiplier);
1628: boss.scale = Vector3.Scale(boss.scale, scaleMultiplier);
1638: private static void ApplyBossScaleToCollision(EnemySpawnRuleConfig boss, Vector3 scaleMultiplier)
1649: private static Vector3 ScaleVector(Vector3 value, Vector3 scale)
1651: return new Vector3(value.x * scale.x, value.y * scale.y, value.z * scale.z);
1654: private static Vector3 GetSafeScaleMultiplier(Vector3 scaleMultiplier)
1656: return new Vector3(
1657: Mathf.Approximately(scaleMultiplier.x, 0f) ? 1f : Mathf.Max(0.01f, scaleMultiplier.x),
1658: Mathf.Approximately(scaleMultiplier.y, 0f) ? 1f : Mathf.Max(0.01f, scaleMultiplier.y),
1659: Mathf.Approximately(scaleMultiplier.z, 0f) ? 1f : Mathf.Max(0.01f, scaleMultiplier.z));
1662: private static Vector3 GetSafeColliderSize(Vector3 size)
1664: return new Vector3(
1665: size.x > 0.0001f ? size.x : 2f,
1666: size.y > 0.0001f ? size.y : 2f,
1667: size.z > 0.0001f ? size.z : 2f);
1673: && ((rule.idleSprites != null && rule.idleSprites.Length > 0)
1674: || (rule.idleSpritesUp != null && rule.idleSpritesUp.Length > 0)
1675: || (rule.idleSpritesDown != null && rule.idleSpritesDown.Length > 0)
1676: || (rule.moveSprites != null && rule.moveSprites.Length > 0)
1677: || (rule.moveSpritesUp != null && rule.moveSpritesUp.Length > 0)
1678: || (rule.moveSpritesDown != null && rule.moveSpritesDown.Length > 0)
1679: || (rule.attackSprites != null && rule.attackSprites.Length > 0)
1680: || (rule.deathSprites != null && rule.deathSprites.Length > 0));
1685: if (bossDefinition != null && bossDefinition.minimumMaxHealth > 0f)
1690: return 0f;
1700: const int size = 48;
1701: Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
1703: Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
1704: float outerRadius = size * 0.44f;
1705: float innerRadius = size * 0.27f;
1707: for (int y = 0; y < size; y++)
1709: for (int x = 0; x < size; x++)
1711: float distance = Vector2.Distance(new Vector2(x, y), center);
1720: new Rect(0f, 0f, size, size),
1721: new Vector2(0.5f, 0.5f),
1734: const int width = 48;
1735: const int height = 40;
1736: Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
1738: Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
1739: float rx = width * 0.4f;
1740: float ry = height * 0.36f;
1742: for (int y = 0; y < height; y++)
1744: for (int x = 0; x < width; x++)
1749: texture.SetPixel(x, y, value <= 1f ? Color.white : Color.clear);
1754: runtimeBossSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), width);
1790: public bool ContainsOuter(Vector3 position)
1798: public bool ContainsPlayableCenter(Vector3 position)
```

## Assets/_Project/Scripts/World/Biomes/Configs/BiomeConfig.cs

```csharp
14: public float regionCellSize = 20f;
15: public float regionBlendWidth = 3f;
16: public float detailNoiseScale = 0.05f;
20: public float heightNoiseScale = 0.02f;
21: public float heightNoiseAmplitude = 0.45f;
120: [Range(0f, 1f)]
121: public float variantThreshold = 0.5f;
140: [Range(0f, 1f)] public float opacity = 0.82f;
142: public Color cameraClearColor = new Color(0.035f, 0.015f, 0.018f, 1f);
144: [Min(1f)] public float cameraDistance = 50f;
146: [Range(1f, 2f)] public float overscan = 1.2f;
148: [Range(0f, 0.1f)] public float driftAmount = 0.015f;
149: [Min(0f)] public float driftSpeed = 0.08f;
150: public int sortingOrder = -32000;
162: public float density = 0.01f;
163: public float minDistance = 2f;
164: public int poissonSalt = 100;
171: public float heightOffset = 0f;
172: public int sortingOrder = 100;
177: public int spriteSalt = 0;
179: public float animationSpeed = 0.15f;
188: public Vector3 colliderSize = new Vector3(1f, 1f, 1f);
189: public Vector3 colliderCenter = Vector3.zero;
199: public int sortingOrder = 1000;
201: public Vector2Int gridPosition = new Vector2Int(0, 4);
202: public float heightOffset = 0f;
204: public Vector3 scale = new Vector3(0.5f, 0.5f, 0.5f);
207: public Vector3 colliderSize = new Vector3(2f, 2f, 2f);
208: public Vector3 colliderCenter = Vector3.zero;
221: public float density = 0.0025f;
222: public float minDistance = 8f;
223: public int poissonSalt = 400;
229: public int maxAlive = 1;
230: public float activationRadius = 20f;
231: public float respawnCooldown = 8f;
232: public float spawnRadius = 1.5f;
235: public float moveSpeed = 1.5f;
236: public float stoppingDistance = 0.1f;
237: public float wanderRadius = 4f;
238: public float chaseRadius = 6f;
239: public float leashRadius = 8f;
240: public Vector2 idleDelayRange = new Vector2(0.5f, 1.5f);
243: public float maxHealth = 30f;
244: public float attackDamage = 1f;
245: public float attackRange = 1.5f;
246: public float attackCooldown = 1f;
247: public int expReward = 10;
251: [Min(0f)] public float contactDamage = 1f;
252: [Min(0f)] public float contactKnockbackDistance = 0.45f;
258: public float separationDistance = 1.1f;
259: public float separationStrength = 1f;
262: public float heightOffset = 0f;
263: public Vector3 scale = Vector3.one;
264: public int sortingOrder = 1000;
267: public float animationSpeed = 0.15f;
272: public Vector3 colliderSize = new Vector3(0.7f, 1.1f, 0.7f);
273: public Vector3 colliderCenter = new Vector3(0f, 0.55f, 0f);
287: public float attackAnimationSpeed = 0.12f;
291: public float projectileSpeed = 8f;
292: public float projectileLifeTime = 3f;
294: public Vector3 projectileScale = new Vector3(0.4f, 0.4f, 0.4f);
295: public float projectileSpawnOffset = 0.5f;
299: public Vector3 attackColliderSize = new Vector3(2f, 2f, 2f);
300: public Vector3 attackColliderCenter = new Vector3(0f, 0.55f, 0f);
304: public float deathAnimationSpeed = 0.15f;
313: public int killTriggerCount = 10;
317: public int splitCount = 2;
322: public float splitVfxScale = 3f;
323: public float splitVfxSpeed = 0.08f;
324: public float splitVfxDuration = 0.6f;
328: public float chargeSpeed = 6f;
329: public float chargeAccelTime = 0.3f;
333: public float debrisDuration = 5f;
334: public float debrisAggroRadius = 8f;
338: public float debrisVfxScale = 15f;
339: public float debrisVfxSpeed = 0.12f;
347: public int minimumMapWidth = 300;
348: public int minimumMapHeight = 300;
352: public Vector2Int centerGrid = new Vector2Int(150, 150);
353: public Vector2Int arenaSize = new Vector2Int(32, 32);
354: public int wallThicknessInCells = 1;
356: public int lockBoundaryInsetInCells = 1;
358: public int triggerInsetInCells = 2;
361: public float wallHeightOffset = 1.5f;
362: public float triggerHeight = 4f;
363: public int sortingOrder = 3500;
374: public Vector3 returnPortalScale = Vector3.one;
405: [Min(2)] public int entranceWidthInCells = 4;
406: [Min(3)] public int approachLengthInCells = 9;
409: [Min(0.1f)] public float pulseSpeed = 1.4f;
410: [Range(0f, 1f)] public float approachOpacity = 0.72f;
414: [Min(0.25f)] public float entranceVisualScale = 1.35f;
419: [Range(1, 4)] public int wallCornerSpanInCells = 2;
421: public float wallCornerBaseYaw = 270f;
425: public int floorSortingOrder = 40;
426: public int gateSortingOrder = 3490;
431: public Color primaryColor = new Color(0.43f, 0.72f, 0.22f, 1f);
432: public Color accentColor = new Color(0.82f, 0.48f, 0.16f, 1f);
433: public Color lockedColor = new Color(0.92f, 0.12f, 0.08f, 1f);
434: public Color clearedColor = new Color(0.34f, 0.32f, 0.3f, 1f);
444: public int fallbackEnemyRuleIndex = 0;
449: public float maxHealthMultiplier = 1f;
450: public float attackDamageMultiplier = 1f;
451: public float moveSpeedMultiplier = 1f;
452: public Vector3 scaleMultiplier = Vector3.one;
455: [Tooltip("0이면 bossRule의 maxHealth를 그대로 사용합니다. 0보다 크면 최소 체력으로 보정합니다.")]
456: public float minimumMaxHealth = 0f;
```

## Assets/_Project/Scripts/World/Biomes/Configs/BiomeEliteSpawnConfig.cs

```csharp
14: [Min(0)] public int minimumCount = 2;
16: [Min(0)] public int maximumCount = 2;
17: [Min(0)] public int minimumPerType = 1;
19: [Min(1f)] public float minimumSpacing = 64f;
20: [Min(0f)] public float entranceExclusionRadius = 18f;
21: [Min(0f)] public float portalExclusionRadius = 10f;
22: [Min(0f)] public float bossExclusionPadding = 8f;
23: [Min(0)] public int clearanceCells = 1;
24: [Min(1f)] public float activationDistance = 48f;
25: [Min(1f)] public float releaseDistance = 72f;
26: [Min(1f)] public float leashDistance = 16f;
30: if (minimumCount < 0 || maximumCount < minimumCount || minimumPerType < 0 || clearanceCells < 0)
```

## Assets/_Project/Scripts/World/Biomes/Configs/EnemySpawnConfig.cs

```csharp
10: [Min(1)] public int normalKillsPerElite = 10;
15: public int NormalKillsPerElite => Mathf.Max(1, normalKillsPerElite);
```

## Assets/_Project/Scripts/World/Biomes/Portals/ReturnPortal.cs

```csharp
12: [SerializeField] private float activationDelay = 1f;  // 씬 로드 후 활성화 딜레이
43: private void OnTriggerEnter2D(Collider2D other)
```

## Assets/_Project/Scripts/World/Biomes/Presentation/BiomeExteriorBackdrop.cs

```csharp
9: [DefaultExecutionOrder(1000)]
40: phase = Mathf.Abs(backdropConfig.sprite.name.GetHashCode() % 1000) * 0.001f;
111: Mathf.Max(activeCamera.nearClipPlane + 1f, config.cameraDistance),
112: Mathf.Max(activeCamera.nearClipPlane + 1f, activeCamera.farClipPlane - 1f));
115: float drift = Mathf.Max(0f, config.driftAmount);
116: float time = Time.unscaledTime * Mathf.Max(0f, config.driftSpeed) + phase;
117: Vector3 driftOffset = activeCamera.transform.right * (Mathf.Sin(time) * viewWidth * drift);
118: driftOffset += activeCamera.transform.up * (Mathf.Cos(time * 0.73f) * viewHeight * drift * 0.65f);
124: Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
131: float scale = requiredScale * Mathf.Max(1f, config.overscan);
132: transform.localScale = new Vector3(scale, scale, 1f);
139: height = camera.orthographicSize * 2f;
143: height = 2f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
146: width = height * Mathf.Max(0.01f, camera.aspect);
151: color.a *= Mathf.Clamp01(opacity);
```

## Assets/_Project/Scripts/World/Biomes/Presentation/BossArenaPresentation.cs

```csharp
20: private const int DetailCount = 8;
23: private readonly List<WallTileVisual> wallTileVisuals = new List<WallTileVisual>(64);
30: private Vector3 entranceLocalPosition;
36: public Vector3 EntranceWorldPosition => transform.TransformPoint(entranceLocalPosition);
41: Vector2Int arenaSize,
65: Vector2Int arenaSize,
71: phase = Mathf.Abs(((int)ownerBiome.BiomeType * 1.731f) % Mathf.PI);
73: float tileSize = Mathf.Max(0.01f, ownerBiome.TileSize);
74: int lockInset = Mathf.Max(0, arenaConfig.lockBoundaryInsetInCells);
75: int thickness = Mathf.Max(1, arenaConfig.wallThicknessInCells);
76: float playableWidth = Mathf.Max(tileSize * 4f, (arenaSize.x - lockInset * 2) * tileSize);
77: float playableDepth = Mathf.Max(tileSize * 4f, (arenaSize.y - lockInset * 2) * tileSize);
81: Vector2 outward = GetOutwardDirection(config.entranceSide);
82: float halfExtent = Mathf.Abs(outward.x) > 0f ? playableWidth * 0.5f : playableDepth * 0.5f;
83: float boundaryOffset = Mathf.Max(tileSize, halfExtent - thickness * tileSize * 0.5f);
84: entranceLocalPosition = new Vector3(outward.x * boundaryOffset, 0f, outward.y * boundaryOffset);
87: if (wallTileVisuals.Count == 0)
107: float time = Time.unscaledTime * Mathf.Max(0.1f, config.pulseSpeed) + phase;
108: float pulse = (Mathf.Sin(time * Mathf.PI * 2f) + 1f) * 0.5f;
117: gateObject.transform.localPosition = entranceLocalPosition + Vector3.up * (tileSize * 1.65f);
125: gateCore.color = WithAlpha(Color.black, 0.58f);
132: config.gateSortingOrder + 1);
136: float desiredSize = Mathf.Max(tileSize * 3.6f, config.entranceWidthInCells * tileSize * 0.92f)
137: * Mathf.Max(0.25f, config.entranceVisualScale);
138: float gateBaseScale = desiredSize / Mathf.Max(0.01f, gateRing.sprite.bounds.size.x);
139: gateRing.transform.localScale = Vector3.one * gateBaseScale;
140: gateCore.transform.localScale = Vector3.one * (gateBaseScale * 0.82f);
141: gateRoot.localPosition = entranceLocalPosition + Vector3.up * (desiredSize * 0.48f);
147: Vector3[] positions =
149: new Vector3(-width * 0.25f, 0.065f, depth * 0.47f),
150: new Vector3(width * 0.25f, 0.065f, depth * 0.47f),
151: new Vector3(width * 0.47f, 0.065f, depth * 0.25f),
152: new Vector3(width * 0.47f, 0.065f, -depth * 0.25f),
153: new Vector3(width * 0.25f, 0.065f, -depth * 0.47f),
154: new Vector3(-width * 0.25f, 0.065f, -depth * 0.47f),
155: new Vector3(-width * 0.47f, 0.065f, -depth * 0.25f),
156: new Vector3(-width * 0.47f, 0.065f, depth * 0.25f)
159: for (int i = 0; i < biomeDetails.Length; i++)
163: $"BiomeOrganMark{i + 1}",
166: i * -45f,
167: config.floorSortingOrder + 1);
168: SetRendererSize(detail, 1.35f, 1.35f);
182: gateRing.color = GetGateColor(cleared ? 0.22f : 0.92f);
183: gateCore.color = WithAlpha(cleared ? palette.cleared : Color.black, cleared ? 0.14f : 0.62f);
184: for (int i = 0; i < wallTileVisuals.Count; i++)
199: || config.wallStraightSprites.Length == 0
205: int widthInCells = Mathf.Max(8, Mathf.RoundToInt(width / tileSize));
206: int depthInCells = Mathf.Max(8, Mathf.RoundToInt(depth / tileSize));
209: 1,
210: Mathf.Max(1, Mathf.Min(widthInCells, depthInCells) / 2 - 1));
211: int entranceWidth = Mathf.Clamp(config.entranceWidthInCells, 2, widthInCells - 2);
212: int openingStart = (widthInCells - entranceWidth) / 2;
213: int openingEnd = openingStart + entranceWidth - 1;
214: float lowCornerCell = (cornerSpan - 1) * 0.5f;
215: float highCornerXCell = widthInCells - 1 - lowCornerCell;
216: float highCornerZCell = depthInCells - 1 - lowCornerCell;
238: config.wallCornerBaseYaw + 90f,
249: config.wallCornerBaseYaw + 180f,
260: config.wallCornerBaseYaw + 270f,
263: if (config.wallUseStraightCornerConnectors && cornerSpan >= 2)
275: $"Boundary_North_{x:D2}",
278: depthInCells - 1,
283: 0f,
290: $"Boundary_West_{z:D2}",
292: 0,
298: 270f,
301: $"Boundary_East_{z:D2}",
302: GetStraightSprite(cornerSpan - 1 - z),
303: widthInCells - 1,
309: 90f,
317: isDoorClosure ? $"Boundary_DoorClosure_{x:D2}" : $"Boundary_South_{x:D2}",
318: GetStraightSprite(cornerSpan - 1 - x),
320: 0,
325: 180f,
343: int westConnectorX = cornerSpan - 1;
345: int southConnectorZ = cornerSpan - 1;
352: depthInCells - 1,
357: 0f,
359: 4);
364: depthInCells - 1,
369: 0f,
371: 4);
374: GetStraightSprite(cornerSpan - 1 - westConnectorX),
376: 0,
381: 180f,
383: 4);
386: GetStraightSprite(cornerSpan - 1 - eastConnectorX),
388: 0,
393: 180f,
395: 4);
399: 0,
405: 270f,
407: 4);
411: 0,
417: 270f,
419: 4);
422: GetStraightSprite(cornerSpan - 1 - southConnectorZ),
423: widthInCells - 1,
429: 90f,
431: 4);
434: GetStraightSprite(cornerSpan - 1 - northConnectorZ),
435: widthInCells - 1,
441: 90f,
443: 4);
457: int sortingOrderOffset = 3)
459: Vector3 position = new Vector3(
460: (xCell - (widthInCells - 1) * 0.5f) * tileSize,
461: 0.12f,
462: (zCell - (depthInCells - 1) * 0.5f) * tileSize);
477: phase = index * 1.731f + Hash01(index * 17 + 3) * 2.4f,
478: speed = Mathf.Lerp(0.68f, 1.18f, Hash01(index * 23 + 11)),
492: float sourceWidth = Mathf.Max(0.01f, renderer.sprite.bounds.size.x);
494: renderer.transform.localScale = new Vector3(uniformScale, uniformScale, uniformScale);
501: BossArenaPresentationState.Locked => Color.Lerp(Color.white, palette.locked, 0.18f),
502: BossArenaPresentationState.Cleared => Color.Lerp(Color.white, palette.cleared, 0.78f),
505: baseColor.a = state == BossArenaPresentationState.Cleared ? 0.16f : 1f;
507: for (int i = 0; i < wallTileVisuals.Count; i++)
516: ? 0f
517: : Mathf.Sin(time * visual.speed * Mathf.PI * 2f + visual.phase);
520: ? 0f
521: : (breath + 1f) * 0.012f;
528: private static float Hash01(int value)
530: float raw = Mathf.Sin(value * 12.9898f + 78.233f) * 43758.5453f;
538: BossArenaPresentationState.Locked => Color.Lerp(palette.locked, palette.accent, pulse * 0.35f),
540: _ => Color.Lerp(palette.primary, palette.accent, pulse * 0.45f)
542: float alpha = state == BossArenaPresentationState.Cleared ? 0.16f : Mathf.Lerp(0.32f, 0.58f, pulse);
544: for (int i = 0; i < biomeDetails.Length; i++)
572: BossArenaPresentationState.Locked => Color.Lerp(Color.white, palette.locked, 0.28f),
573: BossArenaPresentationState.Cleared => Color.Lerp(Color.white, palette.cleared, 0.68f),
599: Vector3 localPosition,
605: renderer.transform.localRotation = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
616: Vector2 size = renderer.sprite.bounds.size;
617: renderer.transform.localScale = new Vector3(
618: width / Mathf.Max(0.01f, size.x),
619: height / Mathf.Max(0.01f, size.y),
620: 1f);
623: private static Vector2 GetOutwardDirection(BossArenaEntranceSide side)
627: BossArenaEntranceSide.North => Vector2.up,
628: BossArenaEntranceSide.West => Vector2.left,
629: BossArenaEntranceSide.East => Vector2.right,
630: _ => Vector2.down
659: new Color(0.58f, 0.04f, 0.08f, 1f),
660: new Color(0.74f, 0.55f, 0.08f, 1f),
661: new Color(0.94f, 0.04f, 0.06f, 1f),
662: new Color(0.26f, 0.12f, 0.14f, 1f)),
664: new Color(0.96f, 0.38f, 0.04f, 1f),
665: new Color(1f, 0.74f, 0.18f, 1f),
666: new Color(0.9f, 0.08f, 0.04f, 1f),
667: new Color(0.34f, 0.2f, 0.12f, 1f)),
669: new Color(0.42f, 0.84f, 0.9f, 1f),
670: new Color(0.78f, 0.68f, 0.94f, 1f),
671: new Color(0.56f, 0.2f, 0.5f, 1f),
672: new Color(0.38f, 0.42f, 0.46f, 1f)),
674: new Color(0.4f, 0.68f, 0.2f, 1f),
675: new Color(0.75f, 0.42f, 0.12f, 1f),
676: new Color(0.78f, 0.08f, 0.04f, 1f),
677: new Color(0.25f, 0.23f, 0.2f, 1f))
683: color.a *= Mathf.Clamp01(alpha);
690: public Vector3 basePosition;
```

## Assets/_Project/Scripts/World/Biomes/Runtime/BiomeDeterministic.cs

```csharp
10: public static float Hash01(int seed, int x, int y, int salt = 0)
13: return (h & 0x00FFFFFF) / 16777216f; // 0..1
18: if (range <= 1) return 0;
23: public static Vector2 HashInCell(int seed, int cellX, int cellY, int salt)
25: float ox = Hash01(seed, cellX, cellY, salt);
26: float oy = Hash01(seed, cellX, cellY, salt + 1337);
27: return new Vector2(ox, oy);
33: h ^= x * 0x9E3779B9u;
34: h ^= y * 0x85EBCA6Bu;
35: h ^= salt * 0xC2B2AE35u;
36: h ^= h >> 16;
37: h *= 0x7FEB352Du;
38: h ^= h >> 15;
39: h *= 0x846CA68Bu;
40: h ^= h >> 16;
```

## Assets/_Project/Scripts/World/Biomes/Runtime/BiomeEliteField.cs

```csharp
32: if (source == null || !source.enabled || source.monsters == null || source.monsters.Count == 0) return;
39: Vector2Int center = arena != null && arena.useCustomCenter ? arena.centerGrid : new Vector2Int(biome.MapWidth / 2, biome.MapHeight / 2);
42: + (arena.presentation != null && arena.presentation.enabled ? arena.presentation.approachLengthInCells : 0) : 0;
43: Vector2Int entrance = biome.WorldToGrid(biome.GetPlayerSpawnPosition());
44: Vector2Int portalCell = portal != null && portal.useCustomPosition ? portal.gridPosition : entrance;
45: bool Excluded(Vector2Int p) => (arenaEnabled && Mathf.Abs(p.x - center.x) <= arena.arenaSize.x / 2f + padding
46: && Mathf.Abs(p.y - center.y) <= arena.arenaSize.y / 2f + padding)
64: nextRefresh = Time.time + .2f;
68: public void Refresh(Vector3 playerPosition)
113: Gizmos.DrawWireSphere(biome.GridToWorldWithHeight(point.x, point.y), .8f);
```

## Assets/_Project/Scripts/World/Biomes/Runtime/BiomeElitePlacementService.cs

```csharp
9: private static readonly Vector2Int[] Steps = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
12: Vector2Int start, Func<Vector2Int, bool> walkable, Func<Vector2Int, Vector2Int, bool> canMove,
13: Func<Vector2Int, bool> excluded)
18: if (!config.enabled || config.monsters.Count == 0 || config.maximumCount == 0) return plan;
19: var random = new System.Random(unchecked(seed * 397 ^ (int)biome * 7919));
20: int count = random.Next(config.minimumCount, checked(config.maximumCount + 1));
23: if (count == 0) return plan;
24: bool Inside(Vector2Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;
26: var visited = new HashSet<Vector2Int> { start };
27: var queue = new Queue<Vector2Int>();
28: var candidates = new List<Vector2Int>();
30: while (queue.Count > 0)
32: Vector2Int cell = queue.Dequeue();
37: Vector2Int neighbor = cell + new Vector2Int(x, y);
41: foreach (Vector2Int step in Steps)
43: Vector2Int next = cell + step;
48: for (int i = candidates.Count - 1; i > 0; i--)
50: int other = random.Next(i + 1);
55: foreach (Vector2Int cell in candidates)
59: if ((cell - new Vector2Int(previous.x, previous.y)).sqrMagnitude < config.minimumSpacing * config.minimumSpacing)
```

## Assets/_Project/Scripts/World/Biomes/Runtime/BiomeManager.cs

```csharp
18: [SerializeField] protected float tileSize = 1f;
21: [SerializeField] protected int seed = 0;
24: private const int RandomSeedRange = 100000;
25: private const int BiomeSeedBucketSize = 100000;
49: [SerializeField] private int defaultMaxPoolSizePerType = 64;
51: [SerializeField] private int maxTotalPoolSize = 0;
55: [SerializeField] protected int minHeightLevel = -1;
56: [SerializeField] protected int maxHeightLevel = 1;
57: [SerializeField] protected int maxStepHeight = 1;
58: [SerializeField] protected float heightStep = 0.5f;
59: [SerializeField] protected float cliffOverlayOffset = 0.01f;
60: [SerializeField] protected Color cliffTint = new Color(0.6f, 0.6f, 0.6f, 1f);
61: [SerializeField] protected float playerHeightOffset = -2f;
72: protected HashSet<Vector2Int> loadedChunks = new HashSet<Vector2Int>();
73: private readonly HashSet<Vector2Int> chunksToLoadCache = new HashSet<Vector2Int>();
74: private readonly List<Vector2Int> chunksToUnloadCache = new List<Vector2Int>();
76: protected Vector2Int lastPlayerChunk = new Vector2Int(-999, -999);
77: protected float chunkUpdateTimer = 0f;
83: protected HashSet<Vector2Int> blockedCells = new HashSet<Vector2Int>();
89: private readonly List<Vector2Int> occupiedCellBuffer = new List<Vector2Int>();
93: private readonly HashSet<Vector2Int> reportedResidualChunks = new HashSet<Vector2Int>();
118: if (chunkSize <= 0)
120: Debug.LogError("[BiomeManager] chunkSize가 0 이하입니다. 인스펙터에서 설정하세요.");
121: chunkSize = 1;
135: return Random.Range(0, RandomSeedRange);
140: int baseSeed = Random.Range(0, RandomSeedRange);
162: chunkUpdateTimer = 0f;
206: for (int cx = 0; cx < chunksX; cx++)
208: for (int cy = 0; cy < chunksY; cy++)
222: grid.cellSize = new Vector3(tileSize, tileSize, tileSize);
223: grid.cellGap = Vector3.zero;
251: Vector3 spawnPos = GetPlayerSpawnPosition();
252: Vector2Int spawnGrid = WorldToGrid(spawnPos);
304: Vector2Int playerGrid = WorldToGrid(playerTransform.position);
305: Vector2Int playerChunk = GridToChunk(playerGrid.x, playerGrid.y);
323: chunksToLoadCache.Add(new Vector2Int(cx, cy));
375: loadedChunks.Add(new Vector2Int(chunkX, chunkY));
404: loadedChunks.Remove(new Vector2Int(chunkX, chunkY));
413: if (chunk.tilemaps == null || chunk.tilemaps.Length == 0) return;
422: for (int i = 0; i < tileCount; i++)
427: for (int i = 0; i < tileCount; i++)
432: for (int ly = 0; ly < chunkSize; ly++)
434: for (int lx = 0; lx < chunkSize; lx++)
455: for (int ly = 1; ly < chunkSize; ly++)
457: for (int lx = 0; lx < chunkSize; lx++)
460: int lowerIndex = (ly - 1) * chunkSize + lx;
478: BoundsInt bounds = new BoundsInt(0, 0, 0, chunkSize, chunkSize, 1);
479: for (int i = 0; i < levelCount; i++)
482: System.Array.Clear(chunk.tileBuffer, 0, tileCount);
485: System.Array.Clear(chunk.cliffBuffer, 0, tileCount);
489: for (int c = 0; c < tileCount; c++)
495: for (int index = 0; index < tileCount; index++)
579: return 0;
```

## Assets/_Project/Scripts/World/Biomes/Runtime/BiomeNavigation.cs

```csharp
10: return chunkX >= 0 && chunkX < chunksX && chunkY >= 0 && chunkY < chunksY;
13: public Vector3 GridToWorld(int gridX, int gridY)
15: float worldX = gridX * tileSize + tileSize / 2f;
16: float worldZ = gridY * tileSize + tileSize / 2f;
17: return new Vector3(worldX, 0f, worldZ);
20: public Vector2Int WorldToGrid(Vector3 worldPos)
24: return new Vector2Int(gridX, gridY);
27: public Vector2Int GridToChunk(int gridX, int gridY)
29: return new Vector2Int(gridX / chunkSize, gridY / chunkSize);
34: return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
39: if (!enableHeight) return 0;
48: if (mapWidth <= 0 || mapHeight <= 0)
50: return 0f;
53: gridX = Mathf.Clamp(gridX, 0, mapWidth - 1);
54: gridY = Mathf.Clamp(gridY, 0, mapHeight - 1);
60: public float GetGroundHeight(Vector3 worldPos)
62: Vector2Int grid = WorldToGrid(worldPos);
66: public Vector3 GridToWorldWithHeight(int gridX, int gridY, float yOffset = 0f)
68: Vector3 pos = GridToWorld(gridX, gridY);
73: public bool CanMove(Vector3 currentWorldPos, Vector3 desiredWorldPos)
75: Vector2Int currentGrid = WorldToGrid(currentWorldPos);
76: Vector2Int targetGrid = WorldToGrid(desiredWorldPos);
90: if (blockedCells.Contains(new Vector2Int(x, y))) return false;
92: Vector2Int chunkCoord = GridToChunk(x, y);
102: public void AddRuntimeBlockedCells(IEnumerable<Vector2Int> occupiedCells)
107: public void RemoveRuntimeBlockedCells(IEnumerable<Vector2Int> occupiedCells)
112: public virtual Vector3 GetPlayerSpawnPosition()
114: return GridToWorld(mapWidth / 2, 5);
135: return Mathf.Max(1, maxHeightLevel - minHeightLevel + 1);
```

## Assets/_Project/Scripts/World/Biomes/Runtime/BiomeObjectState.cs

```csharp
16: private readonly List<Vector2Int> occupiedCells = new List<Vector2Int>();
21: public IReadOnlyList<Vector2Int> OccupiedCells => occupiedCells;
37: public void SetOccupiedCells(List<Vector2Int> cells)
```

## Assets/_Project/Scripts/World/Biomes/Runtime/BiomeRuntimeTypes.cs

```csharp
43: int hash = 17;
44: hash = hash * 31 + x;
45: hash = hash * 31 + y;
46: hash = hash * 31 + (int)type;
66: public ObjectPoolKey(BiomeObjectKind kind, int archetypeId = 0)
76: int hash = 17;
77: hash = hash * 31 + (int)kind;
78: hash = hash * 31 + archetypeId;
95: SceneObject = 0,
96: EnemySpawner = 1,
97: Portal = 2
220: None = 0,
221: FloorDecoration = 1,
222: SmallDecoration = 2,
223: LargeObstacle = 3,
224: AnimatedDecoration = 4,
225: Item = 5,
226: EnemySpawner = 6,
227: Portal = 100
```

## Assets/_Project/Scripts/World/Biomes/Runtime/Chunks/ChunkCleanupService.cs

```csharp
15: for (int i = 0; i < states.Length; i++)
32: Vector2Int chunkCoord = GridToChunk(state.ObjectId.x, state.ObjectId.y);
43: chunk.liveObjects.Count > 0 ||
51: if (chunks == null || reportedResidualChunks.Count == 0) return;
53: Vector2Int[] residualChunks = new Vector2Int[reportedResidualChunks.Count];
56: for (int i = 0; i < residualChunks.Length; i++)
58: Vector2Int chunkPos = residualChunks[i];
104: reportedResidualChunks.Remove(new Vector2Int(chunkX, chunkY));
116: Vector2Int chunkPos = new Vector2Int(chunk.chunkX, chunk.chunkY);
146: int detailCount = 0;
147: for (int i = 0; i < states.Length; i++)
155: if (detailCount == 0)
160: if (detailCount < 16)
162: if (detailCount > 0)
187: if (detailCount > 16)
222: for (int i = 0; i < states.Length; i++)
251: sb.Insert(0, '/');
252: sb.Insert(0, current.name);
```

## Assets/_Project/Scripts/World/Biomes/Runtime/Chunks/ChunkTileRenderer.cs

```csharp
27: Vector3 chunkOrigin = new Vector3(chunk.chunkX * chunkSize * tileSize, 0f, chunk.chunkY * chunkSize * tileSize);
57: for (int i = 0; i < levelCount; i++)
62: tilemaps[i] = CreateTilemapLayer(parent, $"Tiles_{heightLevel}", heightOffset, -1000 + i * 2, out tilemapRenderers[i]);
65: cliffTilemaps[i] = CreateTilemapLayer(parent, $"Cliff_{heightLevel}", heightOffset + cliffOverlayOffset, -999 + i * 2, out cliffTilemapRenderers[i]);
78: tileObj.transform.localPosition = new Vector3(0f, yOffset, 0f);
79: tileObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
131: int index = 0;
132: for (int y = 0; y < chunkSize; y++)
134: for (int x = 0; x < chunkSize; x++)
138: Vector3Int cell = new Vector3Int(x, y, 0);
183: if (chunkRootPool.Count == 0) return null;
236: for (int i = root.childCount - 1; i >= 0; i--)
```

## Assets/_Project/Scripts/World/Biomes/Runtime/ConfigurableBiomeManager.cs

```csharp
27: public bool IsWorldPositionUsingRegionTile(Vector3 worldPosition, string regionName)
34: Vector2Int gridPosition = WorldToGrid(worldPosition);
41: for (int i = 0; i < config.regions.Count; i++)
75: if (config.regions == null || config.regions.Count == 0)
130: yield return new WaitForSeconds(Random.Range(25f, 50f));
150: float detailValue = 0f;
153: detailValue = (detailNoise.GetNoise(worldX, worldY) + 1f) * 0.5f;
162: if (!useVariant && region.tileVariants != null && region.tileVariants.Length > 0)
164: int totalTiles = 1 + region.tileVariants.Length; // primaryTile + variants
165: int hash = BiomeDeterministic.HashRange(seed, worldX, worldY, 777, totalTiles);
166: if (hash > 0)
168: tile = region.tileVariants[hash - 1];
182: if (config == null || config.regions == null || config.regions.Count == 0)
184: return 0;
187: int index = Mathf.Clamp(regionType, 0, config.regions.Count - 1);
195: int left = Mathf.Max(0, config.marginLeft);
196: int right = Mathf.Max(0, config.marginRight);
197: int bottom = Mathf.Max(0, config.marginBottom);
198: int top = Mathf.Max(0, config.marginTop);
206: if (density <= 0f)
208: return 0f;
213: return 0f;
218: ? world?.enemySpawnerDensity ?? 1f
219: : world?.sceneObjectDensity ?? 1f;
220: return density * Mathf.Max(0f, multiplier);
242: : 10);
244: int allMask = 0;
245: for (int i = 0; i < config.regions.Count; i++)
247: allMask |= 1 << i;
252: for (int i = 0; i < config.objectRules.Count; i++)
258: int salt = ruleConfig.poissonSalt != 0 ? ruleConfig.poissonSalt : 200 + i;
279: for (int i = 0; i < enemySpawnRules.Count; i++)
294: int salt = ruleConfig.poissonSalt != 0 ? ruleConfig.poissonSalt : 600 + i;
322: if (record.configIndex < 0 || record.configIndex >= runtimeEnemyRules.Count) return;
329: if (record.configIndex < 0 || record.configIndex >= runtimeRules.Count) return;
341: if (regions == null || regions.Count == 0) return fallbackMask;
343: int mask = 0;
346: if (regionIndex < 0 || regionIndex >= config.regions.Count) continue;
347: mask |= 1 << regionIndex;
350: return mask == 0 ? fallbackMask : mask;
355: if (config == null || config.regions == null || config.regions.Count == 0)
360: int index = Mathf.Clamp(GetRegionTypeCached(worldX, worldY), 0, config.regions.Count - 1);
413: Vector2Int center = midBossArenaConfig.useCustomCenter
415: : new Vector2Int(mapWidth / 2, mapHeight / 2);
417: int arenaWidth = Mathf.Max(8, midBossArenaConfig.arenaSize.x);
418: int arenaHeight = Mathf.Max(8, midBossArenaConfig.arenaSize.y);
419: int minX = center.x - arenaWidth / 2;
420: int minY = center.y - arenaHeight / 2;
421: int maxX = minX + arenaWidth - 1;
422: int maxY = minY + arenaHeight - 1;
432: if (rule.sprites == null || rule.sprites.Length == 0) return;
451: sr.sprite = rule.sprites[0];
491: if (rule.sprites == null || rule.sprites.Length == 0) return null;
492: if (!rule.useDeterministicSprite || rule.sprites.Length == 1)
494: return rule.sprites[0];
497: int salt = rule.spriteSalt != 0 ? rule.spriteSalt : rule.poissonSalt;
507: obj.transform.localPosition = Vector3.zero;
509: obj.transform.localScale = Vector3.one;
524: int archetypeId = record.category == ChunkSpawnCategory.Portal ? 0 : record.configIndex + 1;
```

## Assets/_Project/Scripts/World/Biomes/Runtime/IntestineBiomeManager.cs

```csharp
12: [SerializeField] private TileBase mudTile1;   // 기본 진흙
13: [SerializeField] private TileBase mudTile2;   // 진흙 변형
26: [SerializeField, HideInInspector] private Vector3 rockColliderSize = new Vector3(1.5f, 3f, 1.5f);
27: [SerializeField, HideInInspector] private Vector3 rockColliderCenter = new Vector3(0, 1.5f, 0);
28: [SerializeField, HideInInspector] private Vector3 moldTreeColliderSize = new Vector3(1.5f, 3f, 1.5f);
29: [SerializeField, HideInInspector] private Vector3 moldTreeColliderCenter = new Vector3(0, 1.5f, 0);
32: [SerializeField] private Sprite[] parasiteFrames;         // 기생충 (6프레임)
33: [SerializeField] private float parasiteAnimSpeed = 0.15f; // 애니메이션 속도
39: [SerializeField] private float slimePuddleDensity = 0.03f;
40: [SerializeField] private float moldPlantDensity = 0.05f;
41: [SerializeField] private float rockDensity = 0.02f;
42: [SerializeField] private float moldTreeDensity = 0.015f;
43: [SerializeField] private float parasiteDensity = 0.01f;
44: [SerializeField] private float itemDensity = 0.005f;
47: [SerializeField] private float slimePuddleMinDistance = 2f;
48: [SerializeField] private float moldPlantMinDistance = 2f;
49: [SerializeField] private float rockMinDistance = 3f;
50: [SerializeField] private float moldTreeMinDistance = 4f;
51: [SerializeField] private float parasiteMinDistance = 3f;
52: [SerializeField] private float itemMinDistance = 2f;
55: [SerializeField] private float detailNoiseScale = 0.05f;  // Perlin 스케일
56: [SerializeField] private float mudVariantThreshold = 0.5f;
57: [SerializeField] private float mossMixThreshold = 0.2f;
58: [SerializeField] private float fleshVariantThreshold = 0.5f;
63: private const int GroundDecorationOrder = 100;
64: private const int SlimePuddleSalt = 2001;
65: private const int ItemSalt = 3001;
69: Mud = 0,
70: Moss = 1,
71: Flesh = 2
90: float detailValue = (detailNoise.GetNoise(worldX, worldY) + 1f) * 0.5f;
97: return new TileSample(BiomeTileType.Floor, mudTile1, true);
104: return new TileSample(BiomeTileType.Floor, mudTile1, true);
106: return new TileSample(BiomeTileType.FloorVariant, mudTile2, true);
111: return new TileSample(BiomeTileType.Floor, mudTile1, true);
113: return new TileSample(BiomeTileType.FloorVariant, mudTile2, true);
122: BiomeTileType.FloorVariant => mudTile2,
123: _ => mudTile1
137: salt = 101,
138: configIndex = 0
147: salt = 102,
148: configIndex = 1
157: salt = 103,
158: configIndex = 2
167: salt = 104,
168: configIndex = 3
177: salt = 105,
178: configIndex = 4
187: salt = 106,
188: configIndex = 5
194: return x >= 5 && x < mapWidth - 5 && y >= 10 && y < mapHeight - 5;
201: RegionType.Mud => -1,
202: RegionType.Moss => 0,
203: RegionType.Flesh => 1,
204: _ => 0
228: if (record.configIndex == 0)
255: obj.transform.localPosition = Vector3.zero;
257: obj.transform.localScale = Vector3.one;
304: bool large = BiomeDeterministic.Hash01(seed, record.x, record.y, SlimePuddleSalt) > 0.5f;
313: Vector3 worldPos = GridToWorldWithHeight(record.x, record.y, 0.01f);
333: Vector3 worldPos = GridToWorldWithHeight(record.x, record.y);
349: private void PlaceLargeObstacle(ChunkSpawnRecord record, Sprite sprite, string name, Vector3 colliderSize, Vector3 colliderCenter, Chunk chunk, ObjectId id)
353: Vector3 worldPos = GridToWorldWithHeight(record.x, record.y);
375: if (parasiteFrames == null || parasiteFrames.Length == 0) return;
377: Vector3 worldPos = GridToWorldWithHeight(record.x, record.y);
384: sr.sprite = parasiteFrames[0];
401: Vector3 worldPos = GridToWorldWithHeight(record.x, record.y);
414: col.size = new Vector3(1f, 1f, 1f);
423: if (sprites == null || sprites.Length == 0) return null;
428: public override Vector3 GetPlayerSpawnPosition()
430: return GridToWorld(mapWidth / 2, 7);
435: int archetypeId = record.category == ChunkSpawnCategory.Portal ? 0 : record.configIndex + 1;
```

## Assets/_Project/Scripts/World/Biomes/Runtime/IntestineTransitHazard.cs

```csharp
16: [SerializeField, Min(0)] private float initialDelay = 8f;
17: [SerializeField, Range(3, 5)] private float warningDuration = 4f;
19: [SerializeField, Min(1)] private float restDuration = 30f;
21: [SerializeField, Min(6)] private float latestArrival = 7f;
23: [SerializeField, Min(1)] private float passageDuration = 8f;
26: [SerializeField, Range(3, 28)] private int carriageCount = 14;
28: [SerializeField, Min(1)] private float speed = 100f;
29: [SerializeField, Range(0.05f, 0.2f)] private float connectionOverlap = 0.1f;
31: [SerializeField, Min(1.3f)] private float laneWidth = 2.6f;
32: [SerializeField, Min(0)] private float damage = 1f;
33: [SerializeField, Range(0, 0.8f)] private float slowRatio = 0.35f;
34: [SerializeField, Min(0)] private float slowDuration = 2f;
35: [SerializeField, Min(0.2f)] private float hitCooldown = 1.2f;
43: private Texture2D whiteTexture;
48: private Vector3 start, end;
49: private Vector3 direction = Vector3.right;
50: private Vector3 across = Vector3.forward;
51: private static readonly int[] LaneOffsets = { 0, 2, -2, 4, -4 };
52: private float VisualScale => Mathf.Max(1.3f, laneWidth) / 1.3f;
53: private float HalfLength => 1.6f * VisualScale;
54: private float HalfWidth => Mathf.Max(1.3f, laneWidth) * 0.5f;
55: private float CarSpacing => HalfLength * 2f * (1f - Mathf.Clamp(connectionOverlap, 0.05f, 0.2f));
66: if (Time.deltaTime <= 0 || map == null || !map.IsReady) return;
77: if (timer > 0) return;
80: timer = 2f;
86: timer = Mathf.Clamp(warningDuration, 3f, 5f);
92: float pulse = 0.25f + 0.3f * (0.5f + 0.5f * Mathf.Sin(Time.time * (timer < 1f ? 18f : 8f)));
94: marker.color = new Color(1f, 0.28f, 0.08f, pulse);
95: if (timer > 0) return;
98: nextHit = 0;
99: foreach (SpriteRenderer marker in warnings) marker.color = new Color(1f, 0.38f, 0.08f, 0.18f);
106: int firstCar = Mathf.Max(0, Mathf.CeilToInt((travel - length - HalfLength) / CarSpacing));
107: for (int i = 0; i < cars.Count; i++)
113: float left = Mathf.Max(0, distance - HalfLength);
119: cars[i].position = start + direction * ((left + right) * 0.5f) + Vector3.up * (0.65f * VisualScale);
122: Vector3 viewAxis = camera != null ? camera.transform.InverseTransformDirection(direction) : direction;
123: Vector2 projected = new Vector2(viewAxis.x, viewAxis.y);
124: if (camera == null) projected = new Vector2(direction.x, direction.z);
125: float angle = Mathf.Atan2(projected.y, projected.x) * Mathf.Rad2Deg;
126: cars[i].rotation = (camera != null ? camera.transform.rotation : Quaternion.Euler(90, 0, 0)) * Quaternion.Euler(0, 0, angle);
128: float fullWidth = (right - left) / 0.93f;
129: cars[i].localScale = new Vector3(fullWidth * projected.magnitude / bolusSprite.bounds.size.x, VisualScale, 1f);
134: float sweptLeft = Mathf.Max(0, previous - (transitCarriageCount - 1) * CarSpacing - HalfLength);
137: if (travel - (transitCarriageCount - 1) * CarSpacing - HalfLength >= length) CancelCycle();
142: float travelBudget = Mathf.Max(0.5f, latestArrival - Mathf.Clamp(warningDuration, 3f, 5f));
143: transitSpeed = Mathf.Max(1f, speed, length / travelBudget);
144: float requiredSpan = transitSpeed * Mathf.Max(1f, passageDuration);
146: Mathf.CeilToInt(Mathf.Max(0f, requiredSpan - HalfLength * 2f) / CarSpacing) + 1);
149: private bool TryChooseLane(Vector3 playerPosition)
151: int first = Random.Range(0, 4);
152: for (int attempt = 0; attempt < 4; attempt++)
153: if (TryChooseDirectionalLane(playerPosition, (first + attempt) % 4)) return true;
158: private bool TryChooseDirectionalLane(Vector3 playerPosition, int heading)
160: Vector3Int cell = ground.WorldToCell(playerPosition);
161: if (cell.x < 0 || cell.x >= map.MapWidth || cell.y < 0 || cell.y >= map.MapHeight) return false;
163: bool vertical = heading >= 2;
167: float crossSize = Vector3.Distance(map.GetCellCenterWorld(0, 0),
168: map.GetCellCenterWorld(vertical ? 1 : 0, vertical ? 0 : 1));
169: int halfRows = Mathf.CeilToInt(HalfWidth / Mathf.Max(0.01f, crossSize));
173: if (!SafeCrossSection(alongCell, row, height, vertical, halfRows + 1)) continue;
175: for (int x = 0; x < mapLength && !crossesArena; x++)
179: Vector3 first = map.GetCellCenterWorld(vertical ? row : 0, vertical ? 0 : row);
180: Vector3 step = map.GetCellCenterWorld(vertical ? row : 1, vertical ? 1 : row) - first;
181: start = first - step * 0.5f;
182: end = first + step * (mapLength - 0.5f);
183: if ((heading & 1) != 0) { Vector3 swap = start; start = end; end = swap; }
184: length = Vector3.Distance(start, end);
186: across = new Vector3(-direction.z, 0, direction.x);
187: if (length >= 10f) return true;
208: Bounds bounds = player.HitCollider != null ? player.HitCollider.bounds : new Bounds(player.transform.position, Vector3.one * 0.5f);
220: Vector3 relative = bounds.center - start;
221: float along = Vector3.Dot(relative, direction);
222: float side = Vector3.Dot(relative, across);
236: whiteTexture = new Texture2D(1, 1);
237: whiteTexture.SetPixel(0, 0, Color.white);
239: whiteSprite = Sprite.Create(whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
241: AddMarker((start + end) * 0.5f, new Vector2(length, HalfWidth * 2), 0);
242: AddMarker((start + end) * 0.5f + across * HalfWidth, new Vector2(length, 0.06f), 0);
243: AddMarker((start + end) * 0.5f - across * HalfWidth, new Vector2(length, 0.06f), 0);
244: for (float x = 1; x < length - 1; x += 2.5f)
246: Vector3 point = start + direction * x;
247: AddMarker(point + across * 0.2f, new Vector2(0.65f, 0.1f), -40);
248: AddMarker(point - across * 0.2f, new Vector2(0.65f, 0.1f), 40);
251: Mathf.CeilToInt((length + HalfLength * 2f) / CarSpacing) + 2);
252: for (int i = 0; i < rendererCount; i++)
254: var car = new GameObject("Bolus " + (i + 1));
266: private void AddMarker(Vector3 position, Vector2 size, float angle)
270: marker.transform.position = position + Vector3.up * 0.08f;
271: float headingAngle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
272: marker.transform.rotation = Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0, 0, headingAngle + angle);
273: marker.transform.localScale = new Vector3(size.x, size.y, 1);
276: renderer.sortingOrder = 150;
277: renderer.color = new Color(1, 0.28f, 0.08f, 0.4f);
```

## Assets/_Project/Scripts/World/Biomes/Runtime/LiverToxicRain.cs

```csharp
16: [SerializeField, Min(0f)] private float initialDelay = 5f;
18: [SerializeField, Min(1f)] private float interval = 12f;
19: [SerializeField, Min(0.1f)] private float showerDuration = 3f;
20: [SerializeField, Range(1f, 20f)] private float dropsPerSecond = 2f;
22: [SerializeField, Min(0.2f)] private float warningDuration = 0.6f;
23: [SerializeField, Min(0.1f)] private float impactRadius = 0.45f;
24: [SerializeField, Min(0f)] private float damage = 1f;
25: [SerializeField, Min(0.1f)] private float hitCooldown = 0.8f;
26: [SerializeField, Min(0f)] private float minimumDropSpacing = 1.6f;
28: [SerializeField, Min(1f)] private float fallHeight = 9f;
29: [SerializeField, Min(1f)] private float fallSpeed = 10f;
30: [SerializeField, Min(0.1f)] private float dropHeight = 2.4f;
31: [SerializeField, Min(0.1f)] private float splashWidth = 1.3f;
32: [SerializeField, Min(0.05f)] private float splashDuration = 0.3f;
38: public Vector3 landing;
43: private const int PoolSize = 48;
48: private Texture2D warningTexture;
59: showerRemaining = emissionTimer = 0f;
60: nextHit = 0f;
66: if (dt <= 0f) return;
73: showerRemaining = 0f;
80: if (cycleTimer <= 0f)
82: showerRemaining = Mathf.Max(0.1f, showerDuration);
84: emissionTimer = 0f;
87: if (showerRemaining > 0f)
92: int budget = 3;
93: while (emissionTimer <= 0f && budget-- > 0)
96: emissionTimer += 1f / Mathf.Clamp(dropsPerSecond, 1f, 20f);
98: emissionTimer = Mathf.Max(0f, emissionTimer);
104: if (drop.warningRemaining > 0f)
107: if (drop.warningRemaining > 0f) continue;
114: drop.height = Mathf.Max(0f, drop.height - Mathf.Max(1f, fallSpeed) * dt);
116: drop.landing + Vector3.up * drop.height, camera.transform.rotation);
117: if (drop.height > 0f) continue;
119: drop.splashAge = 0f;
126: float progress = drop.splashAge / Mathf.Max(0.05f, splashDuration);
127: if (progress >= 1f)
134: float scale = splashWidth * drop.size * Mathf.Lerp(0.65f, 1.2f, progress)
135: / Mathf.Max(0.001f, splashSprite.bounds.size.x);
136: drop.renderer.transform.localScale = Vector3.one * scale;
137: drop.renderer.color = new Color(1f, 1f, 1f, 1f - progress);
144: Plane plane = new Plane(Vector3.up, ground.transform.position);
145: Vector3 landing = default;
147: for (int attempt = 0; attempt < 6; attempt++)
149: Ray ray = camera.ViewportPointToRay(new Vector3(Random.Range(0.06f, 0.94f), Random.Range(0.08f, 0.9f), 0f));
152: Vector3Int cell = ground.WorldToCell(landing);
159: Vector2 offset = new Vector2(landing.x - existing.landing.x, landing.z - existing.landing.z);
167: landing.y = map.GetCellCenterWorld(cell.x, cell.y).y + 0.08f;
173: for (int i = 0; i < drops.Length; i++)
192: drop.height = Mathf.Max(1f, fallHeight);
193: drop.warningRemaining = Mathf.Max(0.2f, warningDuration);
194: drop.size = Random.Range(0.75f, 1.15f);
196: drop.renderer.color = new Color(1f, 1f, 1f, 0.88f);
198: SpriteYSort.WorldDynamicBaseSortingOrder - Mathf.RoundToInt(landing.z * 10f) + 2);
199: drop.renderer.transform.localScale = Vector3.one * (dropHeight * drop.size
200: / Mathf.Max(0.001f, dropSprite.bounds.size.y));
201: drop.renderer.transform.SetPositionAndRotation(landing + Vector3.up * drop.height, camera.transform.rotation);
203: drop.warning.transform.SetPositionAndRotation(landing, Quaternion.Euler(90f, 0f, 0f));
204: drop.warning.transform.localScale = Vector3.one * (Mathf.Max(0.1f, impactRadius) * 2f);
205: drop.warning.sortingOrder = drop.renderer.sortingOrder - 1;
206: drop.warning.color = new Color(0.85f, 0.95f, 0.2f, 0.8f);
217: private void TryHit(PlayerController player, Vector3 landing)
219: if (damage <= 0f || Time.time < nextHit || player.IsDead || player.IsDashInvincible ||
222: : new Bounds(player.transform.position, Vector3.one * 0.5f);
226: float radius = Mathf.Max(0.1f, impactRadius);
228: nextHit = Time.time + Mathf.Max(0.1f, hitCooldown);
235: const int size = 64;
236: warningTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
240: var pixels = new Color32[size * size];
241: for (int y = 0; y < size; y++)
242: for (int x = 0; x < size; x++)
244: float radius = new Vector2(x + 0.5f - size * 0.5f, y + 0.5f - size * 0.5f).magnitude;
245: byte alpha = radius > 32f ? (byte)0 : radius >= 28f ? (byte)255 : (byte)35;
246: pixels[y * size + x] = new Color32(255, 255, 255, alpha);
248: warningTexture.SetPixels32(pixels);
250: warningSprite = Sprite.Create(warningTexture, new Rect(0, 0, size, size),
251: new Vector2(0.5f, 0.5f), size);
```

## Assets/_Project/Scripts/World/Biomes/Runtime/LungBiomeHazardController.cs

```csharp
23: [SerializeField, Min(0f)] private float initialDelay = 4f;
24: [SerializeField, Min(0.1f)] private float warningDuration = 1f;
25: [SerializeField, Min(0.1f)] private float inhaleDuration = 4f;
26: [SerializeField, Min(0.1f)] private float exhaleDuration = 4f;
27: [SerializeField, Min(0f)] private float restDuration = 3f;
28: [SerializeField, Min(0f)] private float inhaleSpeed = 3.2f;
29: [SerializeField, Min(0f)] private float exhaleSpeed = 3.8f;
30: [SerializeField, Min(0f)] private float centerDeadZone = 1.25f;
34: [SerializeField, Range(0f, 1f)] private float visionMaskAlpha = 1f;
35: [SerializeField, Min(40f)] private float clearVisionRadius = 230f;
36: [SerializeField, Min(1f)] private float visionFeather = 170f;
39: [SerializeField, Range(4, 32)] private int windStreakCount = 14;
40: [SerializeField, Min(0.1f)] private float windStreakLength = 2.2f;
41: [SerializeField, Min(1f)] private float windVisualRadius = 9f;
53: private Vector3 mapCenter;
58: private Texture2D visionMaskTexture;
66: mapCenter = biome.GridToWorld(biome.MapWidth / 2, biome.MapHeight / 2);
73: mapCenter = owner.GetCellCenterWorld(owner.MapWidth / 2, owner.MapHeight / 2);
142: Vector3 fromCenter = player.transform.position - mapCenter;
143: fromCenter.y = 0f;
150: fromCenter = Vector3.forward;
153: Vector3 direction = fromCenter.normalized;
181: for (int i = 0; i < windStreakCount; i++)
183: GameObject streakObject = new GameObject($"WindStreak_{i + 1}");
187: line.positionCount = 2;
188: line.startWidth = 0.055f;
189: line.endWidth = 0.015f;
190: line.numCapVertices = 2;
191: line.sortingOrder = 5200;
209: windVisualTime += Time.deltaTime * (warning ? 5f : 10f);
210: Vector3 radial = player.transform.position - mapCenter;
211: radial.y = 0f;
212: if (radial.sqrMagnitude < 0.01f) radial = Vector3.forward;
213: Vector3 outward = radial.normalized;
215: Vector3 direction = inward ? -outward : outward;
216: Vector3 side = new Vector3(-direction.z, 0f, direction.x);
218: ? new Color(1f, 0.72f, 0.22f, 0.9f)
219: : new Color(0.75f, 0.9f, 1f, 0.72f);
221: for (int i = 0; i < windStreaks.Count; i++)
223: float seed = i * 1.618f;
224: float travel = Mathf.Repeat(windVisualTime + seed, windVisualRadius * 2f) - windVisualRadius;
225: float lateral = Mathf.Sin(seed * 2.37f) * windVisualRadius;
226: Vector3 start = player.transform.position + direction * travel + side * lateral;
227: start.y += 0.35f + Mathf.Repeat(seed, 1.7f);
230: line.endColor = new Color(color.r, color.g, color.b, 0f);
231: line.SetPosition(0, start);
232: line.SetPosition(1, start - direction * windStreakLength);
243: visionCanvas.sortingOrder = -100;
246: scaler.referenceResolution = new Vector2(1920f, 1080f);
248: visionCanvasGroup.alpha = 0f;
257: const int textureSize = 256;
258: const float maskDisplaySize = 3840f;
263: visionMaskTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
270: Color32[] pixels = new Color32[textureSize * textureSize];
271: for (int y = 0; y < textureSize; y++)
273: for (int x = 0; x < textureSize; x++)
275: float nx = (x + 0.5f) / textureSize - 0.5f;
276: float ny = (y + 0.5f) / textureSize - 0.5f;
278: float alpha = Mathf.SmoothStep(0f, visionMaskAlpha,
283: visionMaskTexture.SetPixels32(pixels);
285: visionMaskSprite = Sprite.Create(visionMaskTexture, new Rect(0f, 0f, textureSize, textureSize),
286: new Vector2(0.5f, 0.5f), 100f);
291: visionMaskRect.anchorMin = new Vector2(0.5f, 0.5f);
292: visionMaskRect.anchorMax = new Vector2(0.5f, 0.5f);
293: visionMaskRect.pivot = new Vector2(0.5f, 0.5f);
294: visionMaskRect.sizeDelta = Vector2.one * maskDisplaySize;
307: Vector2 screenPoint = worldCamera.WorldToScreenPoint(player.transform.position);
310: && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint))
319: float target = visible ? 1f : 0f;
320: visionCanvasGroup.alpha = Mathf.MoveTowards(visionCanvasGroup.alpha, target, Time.unscaledDeltaTime * 2.5f);
325: for (int i = 0; i < windStreaks.Count; i++)
```

## Assets/_Project/Scripts/World/Biomes/Runtime/Objects/BiomeObjectPool.cs

```csharp
18: while (stack.Count > 0)
46: if (maxSize <= 0 || stack.Count >= maxSize)
52: if (maxTotalPoolSize > 0 && pooledObjectCount >= maxTotalPoolSize)
79: if (limit.maxSize >= 0)
```

## Assets/_Project/Scripts/World/Biomes/Runtime/Objects/ChunkObjectSpawner.cs

```csharp
54: if (chunk.liveObjects.Count > 0)
56: for (int i = 0; i < chunk.liveObjects.Count; i++)
69: for (int i = 0; i < childCount; i++)
75: for (int i = 0; i < chunkObjectReleaseBuffer.Count; i++)
169: internal void NotifyObjectRemoved(ObjectId id, bool blocksMovement, System.Collections.Generic.IReadOnlyList<Vector2Int> occupiedCells)
172: if (occupiedCells != null && occupiedCells.Count > 0)
178: blockedCells.Remove(new Vector2Int(id.x, id.y));
202: private void PopulateOccupiedCellsForObject(GameObject obj, ObjectId id, List<Vector2Int> results)
207: results.Add(new Vector2Int(id.x, id.y));
214: results.Add(new Vector2Int(id.x, id.y));
218: Vector3 lossyScale = obj.transform.lossyScale;
219: lossyScale = new Vector3(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
220: Vector3 halfSize = Vector3.Scale(collider.size, lossyScale) * 0.5f;
221: Vector3 center = obj.transform.TransformPoint(collider.center);
223: float epsilon = tileSize * 0.001f;
235: results.Add(new Vector2Int(x, y));
240: if (results.Count == 0)
242: results.Add(new Vector2Int(id.x, id.y));
247: private void AddBlockedCells(System.Collections.Generic.IEnumerable<Vector2Int> occupiedCells)
254: foreach (Vector2Int cell in occupiedCells)
261: private void RemoveBlockedCells(System.Collections.Generic.IEnumerable<Vector2Int> occupiedCells)
268: foreach (Vector2Int cell in occupiedCells)
```

## Assets/_Project/Scripts/World/Biomes/Runtime/ProceduralBiomeBridge.cs

```csharp
18: [SerializeField, Min(8)] private int proceduralChunkSize = 32;
19: [SerializeField, Min(1)] private int proceduralLoadDistance = 2;
20: [SerializeField, Min(1)] private int proceduralUnloadDistance = 3;
25: [Tooltip("이전 맵의 스포너 배치 단위입니다. 16이면 기존 16x16 청크 밀도를 유지합니다.")]
26: [SerializeField, Min(4)] private int enemySpawnerCellSize = 16;
28: [SerializeField, Min(1f)] private float enemySpawnerActivationRadius = 28f;
57: chunkUpdateInterval = 0.2f;
58: objectGenerationBudget = 8;
59: tileSize = 1f;
62: minHeightLevel = 0;
63: maxHeightLevel = 2;
64: maxStepHeight = 0;
65: heightStep = 0.5f;
81: Vector2Int center = arenaConfig.useCustomCenter
83: : new Vector2Int(mapGenerator.MapWidth / 2, mapGenerator.MapHeight / 2);
85: 2,
86: arenaConfig.wallThicknessInCells + arenaConfig.lockBoundaryInsetInCells + 2);
90: padding = Mathf.Max(padding, presentation.approachLengthInCells + 2);
162: : 10);
165: for (int i = 0; i < rules.Count; i++)
223: return mapGenerator != null ? mapGenerator.GetCellHeightLevel(worldX, worldY) : 0;
226: public override Vector3 GetPlayerSpawnPosition()
241: for (int ruleIndex = 0; ruleIndex < normalEnemyRules.Count; ruleIndex++)
244: float chance = Mathf.Clamp01(rule.density * 4f);
246: seed, chunk.chunkX, chunk.chunkY, rule.poissonSalt + 1701, 10000);
247: if (chanceHash >= Mathf.RoundToInt(chance * 10000f)) continue;
263: int placementCellSize = Mathf.Max(4, enemySpawnerCellSize);
270: ? Mathf.Max(0f, worldBalance.enemySpawnerDensity)
271: : 1f;
272: HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
282: int cellArea = Mathf.Max(1, (cellEndX - cellStartX) * (cellEndY - cellStartY));
284: for (int ruleIndex = 0; ruleIndex < normalEnemyRules.Count; ruleIndex++)
289: float spacingArea = Mathf.Max(1f, rule.minDistance * rule.minDistance);
290: float chance = Mathf.Clamp01(rule.density * densityMultiplier * cellArea / spacingArea);
292: seed, cellKeyX, cellKeyY, rule.poissonSalt + 1701, 10000);
293: if (chanceHash >= Mathf.RoundToInt(chance * 10000f)) continue;
311: occupiedPositions.Add(new Vector2Int(x, y));
325: HashSet<Vector2Int> occupiedPositions,
329: int inset = Mathf.Max(1, Mathf.CeilToInt(rule.minDistance * 0.5f));
330: int minX = Mathf.Min(endX - 1, startX + inset);
331: int minY = Mathf.Min(endY - 1, startY + inset);
332: int maxX = Mathf.Max(minX, endX - inset - 1);
333: int maxY = Mathf.Max(minY, endY - inset - 1);
334: int width = Mathf.Max(1, maxX - minX + 1);
335: int height = Mathf.Max(1, maxY - minY + 1);
337: for (int attempt = 0; attempt < 24; attempt++)
339: int salt = rule.poissonSalt + attempt * 2;
341: int y = minY + BiomeDeterministic.HashRange(seed, cellKeyX, cellKeyY, salt + 1, height);
342: Vector2Int position = new Vector2Int(x, y);
353: resultX = resultY = 0;
361: int left = Mathf.Max(0, config.marginLeft);
362: int right = Mathf.Max(0, config.marginRight);
363: int bottom = Mathf.Max(0, config.marginBottom);
364: int top = Mathf.Max(0, config.marginTop);
380: Vector2Int center = arenaConfig.useCustomCenter
382: : new Vector2Int(mapWidth / 2, mapHeight / 2);
383: int halfWidth = Mathf.Max(4, arenaConfig.arenaSize.x / 2);
384: int halfHeight = Mathf.Max(4, arenaConfig.arenaSize.y / 2);
397: float activationRadiusOverride = -1f)
413: for (int attempt = 0; attempt < 24; attempt++)
415: int x = startX + BiomeDeterministic.HashRange(seed, chunk.chunkX, chunk.chunkY, salt + attempt * 2, chunkSize);
416: int y = startY + BiomeDeterministic.HashRange(seed, chunk.chunkX, chunk.chunkY, salt + attempt * 2 + 1, chunkSize);
427: resultX = resultY = 0;
```

## Assets/_Project/Scripts/World/Biomes/Runtime/RegionPoissonBiomeManager.cs

```csharp
12: [SerializeField] protected int regionCount = 3;
13: [SerializeField] protected float regionCellSize = 20f;
14: [SerializeField] protected float regionBlendWidth = 3f;
17: [SerializeField] protected float heightNoiseScale = 0.02f;
18: [SerializeField] protected float heightNoiseAmplitude = 0.45f;
23: private readonly Dictionary<Vector2Int, ChunkCache> chunkCaches = new Dictionary<Vector2Int, ChunkCache>();
25: private const int RegionCellJitterSalt = 501;
26: private const int RegionTypeSalt = 777;
27: private const int RegionBlendSalt = 888;
57: SceneObject = 0,
58: EnemySpawner = 1
86: heightNoise = new BiomePerlinNoise(seed + 97);
144: Vector2Int chunkPos = new Vector2Int(chunk.chunkX, chunk.chunkY);
150: int mask = 0;
151: for (int i = 0; i < regionTypes.Length; i++)
153: mask |= 1 << regionTypes[i];
170: HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
171: int processed = 0;
172: int budget = Mathf.Max(16, objectGenerationBudget);
174: for (int lx = 0; lx < chunkSize; lx++)
176: for (int ly = 0; ly < chunkSize; ly++)
185: Vector2Int pos = new Vector2Int(gx, gy);
207: processed = 0;
219: int processed = 0;
220: int budget = Mathf.Max(16, objectGenerationBudget);
222: for (int i = 0; i < chunk.spawnManifest.Count; i++)
229: processed = 0;
242: private ChunkCache GetOrCreateChunkCache(Vector2Int chunkPos)
252: private int GetChunkIndex(int worldX, int worldY, Vector2Int chunkPos)
267: Vector2Int chunkPos = GridToChunk(worldX, worldY);
289: Vector2Int chunkPos = GridToChunk(worldX, worldY);
305: float cellSize = Mathf.Max(1f, regionCellSize);
311: int bestType = 0;
312: int secondType = 0;
314: int safeRegionCount = Mathf.Max(1, regionCount);
315: for (int dx = -1; dx <= 1; dx++)
317: for (int dy = -1; dy <= 1; dy++)
322: Vector2 offset = BiomeDeterministic.HashInCell(seed, nx, ny, RegionCellJitterSalt);
344: float blend = 0f;
345: if (regionBlendWidth > 0f)
348: blend = Mathf.Clamp01((regionBlendWidth - edge) / regionBlendWidth);
356: if (sample.blend <= 0f || sample.primary == sample.secondary)
361: float mix = BiomeDeterministic.Hash01(seed, worldX, worldY, RegionBlendSalt);
371: if (sample.blend > 0f && primaryHeight != secondaryHeight)
376: float noise = 0f;
391: int region = 1 << regionType;
392: return (mask & region) != 0;
397: return IsRegionAllowed(rule.regionMask, regionType) ? rule.density : 0f;
403: if (density <= 0f) return false;
405: int cellSize = Mathf.Max(1, Mathf.RoundToInt(rule.minDistance));
408: Vector2Int candidate = GetCandidateInCell(cellX, cellY, cellSize, rule.salt);
411: float selfValue = BiomeDeterministic.Hash01(seed, worldX, worldY, rule.salt);
414: float radius = Mathf.Max(0.5f, rule.minDistance);
422: if (dx == 0 && dy == 0) continue;
423: Vector2Int otherCandidate = GetCandidateInCell(cellX + dx, cellY + dy, cellSize, rule.salt);
432: if (otherDensity <= 0f) continue;
434: float otherValue = BiomeDeterministic.Hash01(seed, otherCandidate.x, otherCandidate.y, rule.salt);
445: private Vector2Int GetCandidateInCell(int cellX, int cellY, int cellSize, int salt)
447: Vector2 offset = BiomeDeterministic.HashInCell(seed, cellX, cellY, salt);
452: return new Vector2Int(candidateX, candidateY);
```

## Assets/_Project/Scripts/World/Biomes/Runtime/StomachAcidCycle.cs

```csharp
14: [SerializeField, Min(1f)] private float maximumGauge = 100f;
15: [SerializeField, Min(0f)] private float gaugePerSecond = 5f;
18: [SerializeField, Min(0.1f)] private float warningDuration = 5f;
19: [SerializeField, Min(0.1f)] private float riseDuration = 3f;
20: [SerializeField, Min(0.1f)] private float floodDuration = 10f;
21: [SerializeField, Min(0.1f)] private float drainDuration = 3f;
38: [SerializeField, Min(0f)] private float damagePerTick = 1f;
39: [SerializeField, Min(0.1f)] private float damageInterval = 1f;
42: [SerializeField, Range(0f, 1f)] private float normalEnemyFloodDamage = 0.3f;
43: [SerializeField, Range(0f, 1f)] private float eliteEnemyFloodDamage = 0.1f;
49: if (floodMap == null || !floodMap.IsReady || exposureSeconds <= 0f) return;
60: exposureSeconds / Mathf.Max(0.1f, floodDuration));
65: public float NormalizedGauge => currentGauge / Mathf.Max(1f, maximumGauge);
68: public float PhaseSecondsRemaining => Mathf.Max(0f, currentPhase switch
74: _ => gaugePerSecond > 0f ? (maximumGauge - currentGauge) / gaugePerSecond : 0f
108: if (Time.deltaTime <= 0f || floodMap == null || !floodMap.IsReady ||
109: damagePerTick <= 0f || Time.time < nextDamageTime) return;
117: if (!floodMap.IsPlayerInAcid(player.transform.position, acidLevel > 0f)) return;
119: nextDamageTime = Time.time + Mathf.Max(0.1f, damageInterval);
127: if (Time.deltaTime <= 0f) return;
136: ApplyEnemyFloodDamage(Mathf.Min(Time.deltaTime, Mathf.Max(0f, floodDuration - phaseElapsed)));
144: SetAcidLevel(phaseElapsed / Mathf.Max(0.1f, riseDuration));
155: SetAcidLevel(1f - phaseElapsed / Mathf.Max(0.1f, drainDuration));
169: amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount)) return;
171: currentGauge = Mathf.Min(currentGauge + amount, Mathf.Max(1f, maximumGauge));
172: bool full = NormalizedGauge >= 1f;
188: currentGauge = 0f;
189: acidLevel = 0f;
190: if (floodMap != null) floodMap.SetAcidFloodLevel(0f);
191: onGaugeChanged.Invoke(0f);
192: onAcidLevelChanged.Invoke(0f);
198: phaseElapsed = 0f;
203: acidLevel = Mathf.Clamp01(value);
227: canvas.sortingOrder = 50;
230: scaler.referenceResolution = new Vector2(1920f, 1080f);
231: scaler.matchWidthOrHeight = 0.5f;
233: Image panel = RuntimeUiFactory.CreateImage("Acid Panel", canvasObject.transform, new Color(0.08f, 0.025f, 0.045f, 0.92f));
235: RuntimeUiFactory.SetRect(panel.rectTransform, new Vector2(1f, 1f), new Vector2(380f, 112f), new Vector2(-214f, -80f));
237: Image accent = RuntimeUiFactory.CreateImage("Lava Accent", panel.transform, new Color(1f, 0.48f, 0.08f));
239: RuntimeUiFactory.SetRect(accent.rectTransform, new Vector2(0f, 0.5f), new Vector2(4f, 112f), new Vector2(2f, 0f));
240: title = RuntimeUiFactory.CreateText("Gauge Label", panel.transform, "", font, 22, Color.white);
241: RuntimeUiFactory.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(348f, 30f), new Vector2(0f, -22f));
243: Image track = RuntimeUiFactory.CreateImage("Gauge Track", panel.transform, new Color(0.22f, 0.12f, 0.13f));
245: RuntimeUiFactory.SetRect(track.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(348f, 18f), Vector2.zero);
246: fill = RuntimeUiFactory.CreateImage("Gauge Fill", track.transform, new Color(0.75f, 0.9f, 0.18f));
249: status = RuntimeUiFactory.CreateText("Cycle Status", panel.transform, "", font, 18, Color.white);
250: RuntimeUiFactory.SetRect(status.rectTransform, new Vector2(0.5f, 0f), new Vector2(348f, 30f), new Vector2(0f, 22f));
251: warning = RuntimeUiFactory.CreateText("Evacuation Warning", canvasObject.transform, "", font, 30, new Color(1f, 0.8f, 0.3f));
253: RuntimeUiFactory.SetRect(warning.rectTransform, new Vector2(0.5f, 1f), new Vector2(700f, 50f), new Vector2(0f, -170f));
263: float gauge = Mathf.Clamp01(cycle.NormalizedGauge);
264: fill.rectTransform.anchorMax = new Vector2(gauge, 1f);
265: title.text = $"위액 게이지  {Mathf.FloorToInt(gauge * 100f)}%";
276: ? new Color(1f, 0.55f, 0.08f)
277: : new Color(1f, 0.38f, 0.15f);
282: warning.text = $"위액 범람까지 {seconds}초 · 2층 이상으로 대피하세요!";
283: warning.color = new Color(1f, 0.8f, 0.3f, 0.75f + 0.25f * Mathf.Sin(Time.time * 6f));
```

## Assets/_Project/Scripts/World/FinalBoss/FinalBossArena.cs

```csharp
37: float multiplier = Instance != null ? Instance.Balance.GetDifficulty(SaveService.ActiveDifficulty).damage : 1f;
38: return new EnemyDamageRequest(Mathf.Max(0f, amount * multiplier), null);
53: var result = new MonsterBalanceRuntime(definition, summonDifficulty, summonProgression, 0);
55: float ratio = weakened ? Balance.phaseOne.weakenedMidBossStatRatio : 1f;
57: difficulty.summonMoveSpeed, weakened ? 0f : difficulty.summonExperience, difficulty.summonCooldown);
62: : new RectInt(0, 0, map.MapWidth, map.MapHeight);
63: public Vector2 WorldSize => new Vector2(RoomArea.width, RoomArea.height);
64: public Vector2 RoomOrigin => RoomArea.position;
68: public Vector3 SpawnPosition => map.GetPlayerSpawnWorldPosition();
69: public int PillarCount => pillars != null ? pillars.Length : 0;
87: new RectInt(9, 23, 6, 3), new RectInt(33, 23, 6, 3),
88: new RectInt(9, 9, 6, 3), new RectInt(33, 9, 6, 3)
90: for (int i = 0; i < areas.Length; i++) areas[i] = RoomToMapArea(areas[i]);
91: var phasePillars = new FinalBossPillar[4];
92: for (int i = 0; i < names.Length; i++)
131: Vector3 position = PlayerController.Instance.transform.position;
132: float entrance = RoomArea.y + 7f;
133: if (position.z >= entrance && IsWalkable(position, new Vector2(.55f, .42f)))
137: public Vector3 UVToWorld(Vector2 uv) => new Vector3(
138: RoomArea.x + uv.x * RoomArea.width, 0f, RoomArea.y + uv.y * RoomArea.height);
139: public Vector3 GetPillarPosition(int index) => pillars[index].position;
142: public bool CanTraverse(Vector3 from, Vector3 to, Vector2 halfExtents) => map.CanPlayerMoveWorld(from, to, halfExtents);
143: public bool IsWalkable(Vector3 position, Vector2 halfExtents)
145: for (int x = -1; x <= 1; x++)
146: for (int z = -1; z <= 1; z++)
148: Vector2Int cell = map.WorldToCell(position + new Vector3(x * halfExtents.x, 0f, z * halfExtents.y));
```

## Assets/_Project/Scripts/World/FinalBoss/FinalBossBalanceConfig.cs

```csharp
30: if (!MonsterBalanceNumbers.NonNegative(number)) return field.Name + ": 유한한 0 이상의 값이 필요합니다.";
39: if (count < 1 || (range != null && (count < range.min || count > range.max)))
50: [Min(.01f)] public float health = 1f;
51: [Min(0f)] public float damage = 1f;
52: [Min(.01f)] public float summonMoveSpeed = 1f;
53: [Min(.01f)] public float summonCooldown = 1f;
54: [Min(0f)] public float summonExperience = 1f;
60: [Min(.25f)] public float spawnInterval = 3.5f;
61: [Range(1, 12)] public int commonMinionsPerWave = 4;
62: [Range(1, 3)] public int exclusiveTypesPerBiome = 2;
63: [Range(1, 4)] public int exclusiveCopiesPerType = 2;
64: [Range(.1f, 1f)] public float weakenedMidBossStatRatio = .33f;
65: [Range(.1f, 1f)] public float weakenedMidBossScaleRatio = 1f;
66: [Min(1f)] public float pillarMaxHealth = 65f;
72: [Min(1f)] public float bossMaxHealth = 520f;
73: [Min(.1f)] public float phaseStartDelay = 1.4f;
74: [Min(.1f)] public float patternCooldown = 1.8f;
75: [Min(.1f)] public float electricWarningDuration = 1.35f;
76: [Range(1, 6)] public int electricLineCount = 3;
77: [Min(.2f)] public float electricLineWidth = 1.45f;
78: [Min(0f)] public float electricDamage = 2f;
79: [Min(.1f)] public float reversedControlDuration = 1f;
80: [Min(.1f)] public float missileWarningDuration = 1.15f;
81: [Range(1, 24)] public int missileCount = 12;
82: [Min(.01f)] public float missileInterval = .11f;
83: [Min(.1f)] public float missileSpeed = 7.8f;
84: [Min(0f)] public float missileDamage = 1f;
85: [Min(0f)] public float missileTurnSpeed = 115f;
86: [Min(.1f)] public float bombWarningDuration = 1.45f;
87: [Range(1, 12)] public int bombCount = 6;
88: [Min(.2f)] public float bombRadius = 2.35f;
89: [Min(0f)] public float bombDamage = 2f;
90: [Min(.1f)] public float sweepWarningDuration = 1.55f;
91: [Min(0f)] public float sweepDamage = 2f;
92: [Min(.1f)] public float reflectionWarningDuration = 1.1f;
93: [Min(.1f)] public float reflectionActiveDuration = 3.2f;
94: [Range(0f, 2f)] public float reflectedDamageRatio = .55f;
95: [Min(.1f)] public float barrageWarningDuration = 1.2f;
96: [Range(12, 36)] public int barrageCellsPerWave = 24;
97: [Range(1, 5)] public int barrageWaveCount = 3;
98: [Min(.2f)] public float barrageWaveInterval = .65f;
99: [Min(.1f)] public float barrageCellSpeed = 6.4f;
100: [Min(0f)] public float barrageCellDamage = 1f;
101: [Range(.01f, .99f)] public float phaseThreeHealthRatio = .4f;
107: [Min(.1f)] public float detachDuration = 1.8f;
108: [Min(.1f)] public float pursuitSpeed = 3.1f;
109: [Min(.1f)] public float stoppingDistance = 3.2f;
110: [Min(10f)] public float collapseDuration = 45f;
111: [Min(.5f)] public float patternRecovery = 1.8f;
112: [Min(0f)] public float dashDamage = 2f;
113: [Min(0f)] public float grabDamage = 2f;
114: [Min(0f)] public float tentacleDamage = 2f;
115: [Min(0f)] public float fallingCellDamage = 1f;
116: [Min(0f)] public float fallingBombDamage = 2f;
117: [Min(0f)] public float boundaryDamage = 1f;
118: [Min(0f)] public float overtimeDamage = 2f;
119: [Range(.01f, .99f)] public float collapseHealthRatio = .1f;
```

## Assets/_Project/Scripts/World/FinalBoss/FinalBossHomingProjectile.cs

```csharp
12: private Vector3 direction;
20: private Vector3 previousPlayerPosition;
22: private Vector3 baseScale;
28: Vector3 position,
29: Vector3 initialDirection,
44: renderer.color = turnRate > 0f ? new Color(1f, .65f, .82f) : new Color(1f, .82f, .5f);
45: renderer.sortingOrder = 5100;
46: float spriteWidth = sprite != null ? Mathf.Max(.01f, sprite.bounds.size.x) : 1f;
47: go.transform.localScale = Vector3.one * (1.25f / spriteWidth);
50: projectile.direction = initialDirection.sqrMagnitude > .0001f
52: : Vector3.back;
53: projectile.direction.y = 0f;
54: projectile.damage = Mathf.Max(0f, hitDamage);
55: projectile.speed = Mathf.Max(.1f, moveSpeed);
56: projectile.turnSpeed = Mathf.Max(0f, turnRate);
57: projectile.lifeTime = Mathf.Max(.1f, duration);
66: FinalBossPhaseThreeController.AddEnergyTrail(go.transform, .13f, .24f);
80: const int size = 32;
81: Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
88: float center = (size - 1) * .5f;
89: for (int y = 0; y < size; y++)
90: for (int x = 0; x < size; x++)
92: float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
93: float alpha = Mathf.Clamp01((1f - distance) * 5f);
94: Color color = Color.Lerp(new Color(1f, .15f, .5f), Color.white, Mathf.Clamp01(1f - distance * 1.6f));
99: sharedSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 22f);
116: activeCount = Mathf.Max(0, activeCount - 1);
121: if (Time.deltaTime <= 0f) return;
130: Vector3 toPlayer = player.transform.position - transform.position;
131: toPlayer.y = 0f;
133: if (turnSpeed > 0f && elapsed >= .2f && elapsed < 2.2f && toPlayer.sqrMagnitude > .0001f)
135: direction = Vector3.RotateTowards(
138: turnSpeed * Mathf.Deg2Rad * Time.deltaTime,
139: 0f).normalized;
142: Vector3 previousPosition = transform.position;
147: tint.a = Mathf.Clamp01((lifeTime - elapsed) / .45f);
149: transform.localScale = baseScale * (1f + Mathf.Sin(elapsed * 11f) * .035f);
154: float screenX = Vector3.Dot(direction, camera.transform.right);
155: float screenY = Vector3.Dot(direction, camera.transform.up);
156: float angle = Mathf.Atan2(screenY, screenX) * Mathf.Rad2Deg;
157: transform.rotation = camera.transform.rotation * Quaternion.Euler(0f, 0f, angle);
160: Vector3 playerPosition = player.transform.position;
162: if ((playerPosition - previousPlayerPosition).sqrMagnitude > 100f)
164: Vector3 relativeStart = previousPosition - previousPlayerPosition;
165: Vector3 relativeEnd = transform.position - playerPosition;
168: if (elapsed >= lifeTime - .45f
169: || RelativeSegmentDistanceSquared(relativeStart, relativeEnd) > .62f * .62f) return;
175: private static float RelativeSegmentDistanceSquared(Vector3 start, Vector3 end)
177: start.y = end.y = 0f;
178: Vector3 travel = end - start;
179: float nearest = travel.sqrMagnitude > .000001f
180: ? Mathf.Clamp01(-Vector3.Dot(start, travel) / travel.sqrMagnitude) : 0f;
```

## Assets/_Project/Scripts/World/FinalBoss/FinalBossNeuralVfx.cs

```csharp
10: private Vector3[] vertices;
11: private Vector2[] uv;
17: private float radius, lifetime, age, sweepStart, sweepAngle = 100f;
45: line.numCapVertices = 3; line.numCornerVertices = 3;
51: public static FinalBossNeuralVfx Create(Transform parent, Vector3 position, Shape form,
52: float size, Color color, float duration = 0f, Camera camera = null)
59: effect.Build(); effect.RenderAt(0f);
65: int capacity = shape == Shape.Shield ? 800 : shape == Shape.Boundary ? 400 : 256;
66: vertices = new Vector3[capacity * 4]; uv = new Vector2[capacity * 4];
67: colors = new Color[capacity * 4]; indices = new int[capacity * 6];
72: rendererComponent.sortingOrder = shape == Shape.Boundary ? 4070 : 5060;
75: for (int i = 0; i < capacity; i++)
77: int v = i * 4, k = i * 6;
78: indices[k] = v; indices[k + 1] = v + 1; indices[k + 2] = v + 2;
79: indices[k + 3] = v; indices[k + 4] = v + 2; indices[k + 5] = v + 3;
80: uv[v] = new Vector2(0f, 0f); uv[v + 1] = new Vector2(0f, 1f);
81: uv[v + 2] = new Vector2(1f, 1f); uv[v + 3] = new Vector2(1f, 0f);
93: if (Time.deltaTime <= 0f) return;
95: if (lifetime > 0f && age >= lifetime) { Destroy(gameObject); return; }
103: quadCount = 0;
104: float t = lifetime > 0f ? Mathf.Clamp01(seconds / lifetime) : 0f;
106: Vector3 right = camera != null ? camera.transform.right : Vector3.right;
107: Vector3 up = camera != null ? camera.transform.up : Vector3.up;
108: float opacity = lifetime > 0f ? Mathf.Min(1f, (1f - t) * 3f) : 1f;
112: float expansion = 1f - Mathf.Pow(1f - t, 3f);
113: Arc(Vector3.zero, Vector3.right, Vector3.forward, radius * (.28f + expansion * .72f),
114: 0f, 360f, radius * .13f * (1f - t) + .025f, tint * new Color(1f, 1f, 1f, opacity), 80);
115: Arc(Vector3.up * .13f, Vector3.right, Vector3.forward, radius * (.2f + expansion * .48f),
116: 0f, 360f, .07f, new Color(.8f, 1f, 1f, opacity * .45f), 64);
117: Arc(Vector3.up * .35f, right, up, radius * .18f * (1f - t), 0f, 360f,
118: radius * .18f * (1f - t), new Color(.4f, .9f, 1f, opacity * (1f - t) * .7f), 48);
119: for (int i = 0; i < 18; i++)
121: float angle = i * 2.399963f;
122: Vector3 dir = new Vector3(Mathf.Cos(angle), .3f + (i % 4) * .17f, Mathf.Sin(angle));
123: Vector3 p = dir * radius * expansion * (.55f + (i % 3) * .16f);
124: p.y -= t * t * radius * .65f;
125: float shard = (.07f + radius * .025f) * (1f - t);
126: Quad(p - right * shard, p + up * shard * 3f, p + right * shard,
127: p - up * shard * 2f, new Color(.4f, .85f, 1f, opacity));
131: Arc(Vector3.zero, right, up, radius * .5f, 0f, 360f, radius * .5f,
132: new Color(.12f, .55f, .85f, opacity * .075f), 96);
134: for (int i = 0; i < 3; i++)
136: float h = (i - 1) * .5f;
137: float r = radius * Mathf.Sqrt(1f - h * h);
138: Arc(Vector3.up * h * radius, Vector3.right, Vector3.forward, r,
139: seconds * (i % 2 == 0 ? 24f : -19f), 325f, .045f, tint * new Color(1f, 1f, 1f, .5f * opacity), 64);
141: for (int i = 0; i < 3; i++)
143: Vector3 axis = Quaternion.Euler(0f, i * 60f + seconds * 22f, 0f) * Vector3.right;
144: Arc(Vector3.zero, axis, Vector3.up, radius, 0f, 360f, .075f, tint * new Color(1f, 1f, 1f, opacity * .75f), 80);
146: Arc(Vector3.zero, right, up, radius * 1.02f, 0f, 360f, .18f,
147: new Color(.4f, .83f, 1f, .23f * opacity), 96);
150: Arc(Vector3.zero, right, up, radius * (1f + Mathf.Sin(seconds * 4f) * .035f),
151: seconds * 55f, 270f, .09f, tint * new Color(1f, 1f, 1f, opacity * .55f), 64);
152: Arc(Vector3.zero, right, up, radius * .72f, -seconds * 80f, 230f, .04f, tint, 48);
155: for (int i = 0; i < 3; i++)
156: Arc(Vector3.up * (.08f + i * .035f), Vector3.right, Vector3.forward, radius - i * .28f,
157: sweepStart, sweepAngle, .2f - i * .05f, tint * new Color(1f, 1f, 1f, .6f - i * .15f), 72);
160: for (int side = 0; side < 4; side++)
162: Vector3 a = new Vector3(side < 2 ? bounds.xMin : bounds.xMax, 0f,
163: side == 0 || side == 3 ? bounds.yMin : bounds.yMax);
164: Vector3 b = new Vector3(side == 0 || side == 3 ? bounds.xMin : bounds.xMax, 0f,
165: side < 2 ? bounds.yMax : bounds.yMin);
166: for (int layer = 0; layer < 3; layer++)
168: Vector3 previous = a + Vector3.up * (layer * .42f);
169: for (int j = 1; j <= 32; j++)
171: float u = j / 32f;
172: Vector3 next = Vector3.Lerp(a, b, u) + Vector3.up *
173: (layer * .42f + Mathf.Sin(u * 38f + seconds * 3f + side) * .08f);
174: Segment(previous, next, .10f - layer * .025f,
175: new Color(.18f, .8f, .83f, .85f - layer * .23f), up);
183: mesh.SetTriangles(indices, 0, quadCount * 6, 0, false);
187: private void Arc(Vector3 center, Vector3 x, Vector3 y, float r, float start, float extent, float width, Color color, int steps)
189: for (int i = 0; i < steps; i++)
191: float a = (start + extent * i / steps) * Mathf.Deg2Rad;
192: float b = (start + extent * (i + 1) / steps) * Mathf.Deg2Rad;
193: Vector3 p = x * Mathf.Cos(a) + y * Mathf.Sin(a);
194: Vector3 q = x * Mathf.Cos(b) + y * Mathf.Sin(b);
195: float fade = Mathf.Abs(extent) < 350f ? Mathf.Clamp01(Mathf.Min(i + 1, steps - i) / 7f) : 1f;
202: private void Segment(Vector3 a, Vector3 b, float width, Color color, Vector3 planeUp)
204: Vector3 cross = Vector3.Cross((b - a).normalized, planeUp).normalized * width;
205: if (cross.sqrMagnitude < .00001f) cross = Vector3.right * width;
209: private void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
211: if (quadCount >= indices.Length / 6) return;
212: int v = quadCount++ * 4;
213: vertices[v] = a; vertices[v + 1] = b; vertices[v + 2] = c; vertices[v + 3] = d;
214: colors[v] = colors[v + 1] = colors[v + 2] = colors[v + 3] = color;
```

## Assets/_Project/Scripts/World/FinalBoss/FinalBossPhaseOneController.cs

```csharp
42: private Vector3 bossRestScale;
48: public Vector3 BossGroundPosition => new Vector3(
49: bossVisual != null ? bossVisual.position.x : 24f,
50: arena != null ? arena.SpawnPosition.y + .12f : .12f,
51: bossVisual != null ? bossVisual.position.z : 30f);
53: public int CurrentPhase { get; private set; } = 1;
54: public int DestroyedPillarCount => pillars == null ? 0 : pillars.Count(pillar => pillar != null && pillar.IsDestroyed);
60: && CurrentPhase == 1 && !disabledBiomes.Contains(biome);
62: exclusiveRules.TryGetValue(biome, out List<EnemySpawnRuleConfig> rules) ? rules.Count : 0;
93: sealGauge = FinalBossScreenHealthBar.Create(transform, 4f, 4f);
94: sealGauge.SetSealProgress(DestroyedPillarCount, 4);
106: if (!initialized || phaseTransition != null || CurrentPhase != 1 || spawningPausedForTests
115: if (!initialized || CurrentPhase != 1 || bossVisual == null) return;
116: float progress = DestroyedPillarCount / 4f;
117: float breath = Mathf.Sin(Time.time * Mathf.Lerp(1.35f, 2.8f, progress));
118: bossVisual.localScale = Vector3.Scale(bossRestScale,
119: new Vector3(1f + breath * .009f, 1f + breath * .016f, 1f));
122: new Color(bossAwakeColor.r * .64f, bossAwakeColor.g * .58f,
123: bossAwakeColor.b * .68f, bossAwakeColor.a), bossAwakeColor, progress * .6f);
134: commonSpawnCursor = 0;
135: exclusiveSpawnSequence = 0;
154: if (biomeSpecific.Count > 0)
187: for (int i = 0; i < typeCount; i++)
231: while (commonMinions.Count < commonMinionsPerWave && commonRules.Count > 0)
243: if (rule == null || !TryFindSpawnPosition(biome, out Vector3 position)) return null;
250: private bool TryFindSpawnPosition(BiomeType sourceBiome, out Vector3 position)
253: for (int i = 0; i < 24; i++)
255: Vector2 uv = new Vector2(Random.Range(.22f, .78f), Random.Range(.24f, .73f));
256: Vector3 candidate = arena.UVToWorld(uv);
257: if (sourcePillar != null && i < 16)
259: float angle = Random.Range(0f, Mathf.PI * 2f);
260: float distance = Random.Range(3.5f, 6.5f);
262: + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
264: if (!arena.IsWalkable(candidate, new Vector2(.55f, .42f))) continue;
267: Vector3 delta = candidate - PlayerController.Instance.transform.position;
268: delta.y = 0f;
269: if (delta.sqrMagnitude < 20f) continue;
276: position = arena.UVToWorld(new Vector2(.5f, .5f));
280: Vector3 delta = position - PlayerController.Instance.transform.position;
281: delta.y = 0f;
282: if (delta.sqrMagnitude < 20f) return false;
284: return arena.IsWalkable(position, new Vector2(.55f, .42f));
289: if (pillar == null || CurrentPhase != 1) return;
291: sealGauge?.SetSealProgress(DestroyedPillarCount, 4);
292: DontStarveCamera.Instance?.AddCombatImpulse(.16f, .22f);
293: nextSpawnTime = Mathf.Max(nextSpawnTime, Time.time + 2f);
294: if (DestroyedPillarCount >= 4)
299: AudioManager.Instance?.PlaySFX("BossPhaseChange", .45f);
307: yield return new WaitForSeconds(.15f);
340: originalHealth *= Mathf.Max(.01f, definition.maxHealthMultiplier);
341: originalAttackDamage *= Mathf.Max(.01f, definition.attackDamageMultiplier);
342: originalMoveSpeed *= Mathf.Max(.01f, definition.moveSpeedMultiplier);
347: Vector3 originalScaleMultiplier = GetSafeScaleMultiplier(definition?.scaleMultiplier ?? Vector3.one);
348: Vector3 weakenedScaleMultiplier = originalScaleMultiplier * weakenedMidBossScaleRatio;
349: Vector3 originalScale = Vector3.Scale(source.scale, originalScaleMultiplier);
355: poissonSalt = 9300 + (int)biome,
356: maxAlive = 1,
359: wanderRadius = Mathf.Min(source.wanderRadius, 6f),
360: chaseRadius = Mathf.Max(source.chaseRadius, 14f),
361: leashRadius = 24f,
363: maxHealth = Mathf.Max(1f, originalHealth * weakenedMidBossStatRatio),
367: expReward = 0,
382: colliderSize = Vector3.Scale(source.colliderSize, weakenedScaleMultiplier),
383: colliderCenter = Vector3.Scale(source.colliderCenter, weakenedScaleMultiplier),
410: private static Vector3 GetSafeScaleMultiplier(Vector3 value)
412: return new Vector3(
413: Mathf.Approximately(value.x, 0f) ? 1f : Mathf.Max(.01f, value.x),
414: Mathf.Approximately(value.y, 0f) ? 1f : Mathf.Max(.01f, value.y),
415: Mathf.Approximately(value.z, 0f) ? 1f : Mathf.Max(.01f, value.z));
420: CurrentPhase = 2;
440: if (CurrentPhase == 2) CurrentPhase = 3;
476: ? new Color(bossAwakeColor.r * .64f, bossAwakeColor.g * .58f,
477: bossAwakeColor.b * .68f, bossAwakeColor.a)
```

## Assets/_Project/Scripts/World/FinalBoss/FinalBossPhaseThreeController.cs

```csharp
14: [SerializeField, Range(.4f, 1f)] private float mobileScale = .65f;
21: private Vector3 restingVisualPosition;
22: private Vector2 footprint;
29: private Vector3 mobileVisualScale;
33: private Vector3 lastAnimationPosition;
45: Rect.MinMaxRect(.14f, .44f, .88f, .77f),
46: Rect.MinMaxRect(.23f, .73f, .84f, .96f),
47: Rect.MinMaxRect(.34f, .25f, .70f, .53f),
48: Rect.MinMaxRect(.00f, .32f, .37f, .58f),
49: Rect.MinMaxRect(.66f, .19f, 1.00f, .50f),
50: Rect.MinMaxRect(.20f, .04f, .47f, .36f),
51: Rect.MinMaxRect(.53f, .02f, .81f, .36f)
58: public Vector3 Position => mobileRoot != null ? mobileRoot.position : transform.position;
91: footprint = new Vector2(9f * mobileScale * .5f + .1f,
92: 3.5f * mobileScale * .5f + .1f);
93: mobileVisualScale = Vector3.one * (12.5f / Mathf.Max(.01f, mobileSprite.bounds.size.x));
126: core.center = new Vector3(0f, 1f, 0f);
127: core.size = new Vector3(4f, 2f, 3.25f);
129: for (int i = 0; i < artworkHitboxes.Length; i++)
130: artworkHitboxes[i] = ConfigureHitbox(ArtworkHitboxNames[i], Vector3.zero, Vector3.one, core);
135: private BoxCollider ConfigureHitbox(string objectName, Vector3 center, Vector3 size, BoxCollider source)
145: child.localPosition = Vector3.zero;
147: child.localScale = Vector3.one;
166: float depth = Mathf.Max(bounds.size.y, bounds.size.x) * 1.15f;
167: for (int i = 0; i < artworkHitboxes.Length; i++)
174: hitbox.center = new Vector3(x,
175: Mathf.Lerp(bounds.min.y, bounds.max.y, region.center.y), depth * .5f);
176: hitbox.size = new Vector3(bounds.size.x * region.width, bounds.size.y * region.height, depth);
185: Vector3 start = mobileRoot.position;
186: Vector3 entry = FindEntryPosition();
187: Vector3 visualStart = bossVisual.localPosition;
188: Vector3 originalVisualScale = bossVisual.localScale;
189: Vector3 targetStart = damageTarget.transform.localPosition;
190: restingVisualPosition = new Vector3(0f, .35f, 0f);
191: AudioManager.Instance?.PlaySFX("BossPhaseChange", .8f);
192: DontStarveCamera.Instance?.AddCombatImpulse(.22f, .3f);
194: for (float elapsed = 0f; elapsed < detachDuration && IsActive; elapsed += Time.deltaTime)
196: float normalized = Mathf.Clamp01(elapsed / detachDuration);
197: float t = Mathf.SmoothStep(0f, 1f, normalized);
198: mobileRoot.position = Vector3.Lerp(start, entry, t);
199: mobileRoot.localScale = Vector3.one * Mathf.Lerp(1f, mobileScale, t);
200: if (normalized >= .22f && !appearanceApplied) ApplyMobileAppearance();
202: bossVisual.localScale = Vector3.Lerp(originalVisualScale, mobileVisualScale,
203: Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((normalized - .22f) / .78f)));
204: bossVisual.localPosition = Vector3.Lerp(visualStart, restingVisualPosition, t)
205: + Vector3.up * Mathf.Sin(normalized * Mathf.PI) * 1.5f;
206: damageTarget.transform.localPosition = Vector3.Lerp(targetStart, Vector3.zero, t);
212: mobileRoot.localScale = Vector3.one * mobileScale;
216: damageTarget.transform.localPosition = Vector3.zero;
220: AudioManager.Instance?.PlaySFX("BossRoar", .7f);
221: DontStarveCamera.Instance?.AddCombatImpulse(.14f, .2f);
246: private Vector3 FindEntryPosition()
249: Vector3 preferred = arena.UVToWorld(new Vector2(.5f, .7f));
250: if (TryGroundedPosition(preferred, out Vector3 result)) return result;
251: for (int z = Mathf.RoundToInt(arena.WorldSize.y * .7f); z >= 7; z--)
252: for (int x = 5; x < arena.WorldSize.x - 5; x++)
253: if (TryGroundedPosition(new Vector3(x + .5f, 0f, z + .5f), out result)) return result;
260: private bool TryGroundedPosition(Vector3 candidate, out Vector3 result)
270: if (!IsActive || detaching || mobileRoot == null || Time.deltaTime <= 0f) return;
279: Vector3 position = mobileRoot.position;
280: Vector3 step = CalculatePursuitStep(position, player.transform.position,
282: if (step.sqrMagnitude <= .000001f) return;
288: if (!TryMove(position, new Vector3(step.x, 0f, 0f)))
289: TryMove(position, new Vector3(0f, 0f, step.z));
291: else if (!TryMove(position, new Vector3(0f, 0f, step.z)))
292: TryMove(position, new Vector3(step.x, 0f, 0f));
296: private bool TryMove(Vector3 position, Vector3 step)
298: if (step.sqrMagnitude <= .000001f) return false;
299: Vector3 candidate = position + step;
305: if (bossRenderer != null && Mathf.Abs(step.x) > .001f) bossRenderer.flipX = step.x < 0f;
316: if (camera != null) bossVisual.rotation = camera.transform.rotation * Quaternion.Euler(0f, 0f, bodyLean);
323: bossVisual.localPosition = restingVisualPosition + Vector3.up * Mathf.Sin(Time.time * 2.7f) * .12f;
324: float pulse = Mathf.Sin(Time.time * (attacking ? 8f : 2.7f)) * (attacking ? .024f : .012f);
325: float charge = Time.time < chargeEnd && chargeDuration > 0f
326: ? Mathf.Clamp01(1f - (chargeEnd - Time.time) / chargeDuration) * .06f : 0f;
327: bossVisual.localScale = Vector3.Scale(mobileVisualScale, new Vector3(1f + pulse + charge, 1f - pulse - charge * .5f, 1f));
329: bossRenderer.color = reflectingAppearance ? new Color(.55f, .95f, 1f)
330: : Color.Lerp(Color.white, new Color(1f, .6f, .72f), charge * 8f);
336: bool moving = (mobileRoot.position - lastAnimationPosition).sqrMagnitude > .000001f;
355: spriteAnimation.Play(FinalBossSpriteAnimator.Pose.Death, 1.6f);
359: private float GroundHeight(Vector3 position)
367: Vector3 position = mobileRoot.position;
372: private static Vector3 CalculatePursuitStep(Vector3 position, Vector3 target,
375: Vector3 delta = target - position;
376: delta.y = 0f;
378: float travel = Mathf.Min(Mathf.Max(0f, speed) * Mathf.Max(0f, deltaTime),
379: Mathf.Max(0f, distance - Mathf.Max(0f, stopDistance)));
380: return distance > .0001f ? delta * (travel / distance) : Vector3.zero;
```

## Assets/_Project/Scripts/World/FinalBoss/FinalBossPhaseThreePatterns.cs

```csharp
22: private readonly FinalBossTelegraph[] collapsedFloor = new FinalBossTelegraph[4];
25: private Vector3 grabSafePosition;
27: private static readonly Color WarningColor = new Color(1f, .12f, .24f, .48f);
28: private static readonly Color ImpactColor = new Color(.35f, .9f, 1f, .6f);
31: public float TimeRemaining => Mathf.Max(0f, collapseDuration - collapseElapsed);
37: yield return new WaitForSeconds(1f);
42: if (bag.Count == 0)
44: for (int i = 1; i <= 6; i++) bag.Add((MobilePattern)i);
45: for (int i = bag.Count - 1; i > 0; i--)
47: int j = Random.Range(0, i + 1);
50: if (bag[0] == previous) { MobilePattern swap = bag[0]; bag[0] = bag[1]; bag[1] = swap; }
52: CurrentMobilePattern = bag[0]; bag.RemoveAt(0); previous = CurrentMobilePattern;
64: while (IsActive && FinalBossHomingProjectile.ActiveCount > 0) yield return null;
68: yield return new WaitForSeconds(collapseStarted ? 1.1f : patternRecovery);
75: Owner?.SetMobilePattern(label, TimerHint(), 2f);
79: ? $"{(TimeRemaining > 0f ? Mathf.CeilToInt(TimeRemaining) + "s" : "OVERTIME")}  /  {patternHint}"
88: private Vector3 PlayerPosition => PlayerController.Instance != null ? PlayerController.Instance.transform.position : Position;
89: private static Vector3 Flat(Vector3 value) { value.y = 0f; return value; }
90: private Vector3 DirectionToPlayer() => Flat(PlayerPosition - Position).sqrMagnitude > .001f
91: ? Flat(PlayerPosition - Position).normalized : Vector3.back;
92: private Vector3 Floor(Vector3 value) { value.y = GroundHeight(value) + .06f; return value; }
94: private void PoseTentacle(FinalBossTentacleVisual limb, Vector3 origin, Vector3 direction,
95: float reach, float width, float curl, float energy, float alpha = 1f)
98: limb.Pose(origin + Vector3.up * 1.9f, Floor(origin + direction * reach) + Vector3.up * .6f,
102: private IEnumerator PrepareTentacle(FinalBossTentacleVisual limb, Vector3 origin,
103: Vector3 direction, float width, float seconds)
106: for (float elapsed = 0f; elapsed < seconds; elapsed += Time.deltaTime)
108: float t = Mathf.Clamp01(elapsed / seconds);
109: PoseTentacle(limb, origin, direction, Mathf.Lerp(2.8f, 1.4f, t), width,
110: Mathf.Lerp(1.2f, 2f, t), t * .65f, Mathf.Min(1f, t * 5f));
114: private FinalBossTelegraph Circle(Vector3 center, float radius, float duration)
116: var warning = FinalBossTelegraph.CreateCircle(EffectRoot, Floor(center), radius, duration + .15f, WarningColor);
120: private FinalBossTelegraph Lane(Vector3 origin, Vector3 direction, float length, float width, float duration)
122: var warning = FinalBossTelegraph.CreateRectangle(EffectRoot, Floor(origin + direction * length * .5f),
123: new Vector2(width, length), duration + .15f, WarningColor);
124: warning.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
132: Vector3 origin = Position, direction = DirectionToPlayer();
133: const float distance = 12f, windup = 1.15f;
134: var lane = Lane(origin, direction, distance, 3.2f, windup);
137: AudioManager.Instance?.PlaySFX("IntestineBossImpact", .7f);
139: float nextEcho = 0f;
140: bodyLean = -Mathf.Sign(Vector3.Dot(direction, Vector3.right)) * 12f;
141: for (float t = 0f; t < .55f; t += Time.deltaTime)
143: Vector3 before = Position;
144: if (!TryMove(before, direction * (distance / .55f) * Time.deltaTime)) break;
145: if (t >= nextEcho) { StartCoroutine(DashEcho()); nextEcho = t + .075f; }
147: if (!hit && player != null && SegmentDistance(player.transform.position, before, Position) < 1.8f)
151: bodyLean = 0f;
152: DontStarveCamera.Instance?.AddCombatImpulse(.2f, .18f);
153: for (int wave = 0; wave < 3; wave++)
155: Vector3 target = PlayerPosition;
156: for (int i = 0; i < 7; i++)
158: float angle = i * Mathf.PI * 2f / 7f + wave * .35f;
159: Vector3 point = i == 0 ? target : target + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (3f + wave);
160: StartCoroutine(FallingCell(point, 1.35f, 1f, false));
162: yield return new WaitForSeconds(.65f);
164: yield return new WaitForSeconds(1.8f);
170: Vector3 origin = Position, direction = DirectionToPlayer();
171: var warning = Lane(origin, direction, 9f, 2.4f, 1.3f);
173: yield return PrepareTentacle(claw, origin, direction, 2.1f, 1.3f);
175: PlayMeleeAnimation(.65f);
177: for (float t = 0f; t < .3f; t += Time.deltaTime)
179: float length = Mathf.Lerp(1.4f, 9f, 1f - Mathf.Pow(1f - Mathf.Clamp01(t / .22f), 3f));
180: PoseTentacle(claw, origin, direction, length, 2.1f, .3f, 1f);
182: if (!attempted && player != null && InLane(player.transform.position, origin, direction, length, 1.2f))
193: PlayMeleeAnimation(.95f);
194: Vector3 start = grabSafePosition;
195: for (float t = 0f; t < .3f && grabbedPlayer != null && !grabbedPlayer.IsDead; t += Time.deltaTime)
197: Vector3 held = start + Vector3.up * (Mathf.Sin(t / .3f * Mathf.PI * .5f) * 2f);
199: claw.Pose(origin + Vector3.up * 1.9f, held + Vector3.up * .5f, 2.1f, 1f, 1f);
202: Vector3 landing = start;
203: for (float d = .4f; d <= 7f; d += .4f)
205: Vector3 next = start + direction * d;
206: if (!arena.CanTraverse(landing, next, new Vector2(.7f, .5f))
207: || !CanCrossBoundary(landing, next, new Vector2(.7f, .5f))) break;
210: var landingWarning = Circle(landing, 1.5f, .7f);
211: for (float t = 0f; t < .65f && grabbedPlayer != null && !grabbedPlayer.IsDead; t += Time.deltaTime)
213: float n = Mathf.Clamp01(t / .65f);
214: Vector3 pos = Vector3.Lerp(start, landing, n);
215: pos.y = GroundHeight(pos) + Mathf.Lerp(2f, 0f, n) + Mathf.Sin(n * Mathf.PI) * 2f;
217: PoseTentacle(claw, origin, direction, Mathf.Lerp(Flat(start - origin).magnitude, 1f, Mathf.SmoothStep(0f, 1f, n)),
218: 2.1f, Mathf.Sin(n * Mathf.PI) * -1.2f, 1f - n, 1f - n);
228: Impact(landing, 1.5f);
234: for (float t = 0f; t < .35f; t += Time.deltaTime)
236: float n = t / .35f;
237: PoseTentacle(claw, origin, direction, Mathf.Lerp(9f, .7f, Mathf.SmoothStep(0f, 1f, n)), 2.1f, n, .5f, 1f - n);
248: Vector3 position = grabSafePosition; position.y = GroundHeight(position);
257: yield return Thrust(DirectionToPlayer(), 11f, 2.4f, .95f);
258: yield return new WaitForSeconds(.35f);
260: Vector3 direction = DirectionToPlayer(), origin = Position;
262: var directions = new List<Vector3>();
264: for (int i = -2; i <= 2; i++)
266: Vector3 dir = Quaternion.AngleAxis(i * 27f, Vector3.up) * direction;
267: directions.Add(dir); warnings.Add(Lane(origin, dir, 9f, 1.6f, 1f));
270: Windup(1f);
271: for (float t = 0f; t < 1f; t += Time.deltaTime)
273: for (int i = 0; i < limbs.Count; i++)
274: PoseTentacle(limbs[i], origin, directions[i], Mathf.Lerp(2.8f, 1.4f, t), 1.6f,
275: (i - 2) * .45f, t * .65f, Mathf.Min(1f, t * 5f));
279: for (int i = 0; i < directions.Count; i++) StartCoroutine(AnimateThrust(origin, directions[i], 9f, 1.6f, limbs[i]));
280: yield return new WaitForSeconds(.75f);
283: var circle = Circle(origin, 7.5f, 1.2f);
285: yield return PrepareTentacle(sweep, origin, Vector3.right, 1.8f, 1.2f);
287: PlayMeleeAnimation(.85f);
289: 7.5f, new Color(.22f, .8f, 1f, .8f));
291: for (float t = 0f; t < .85f; t += Time.deltaTime)
293: Vector3 dir = Quaternion.AngleAxis(360f * t / .85f, Vector3.up) * Vector3.right;
294: PoseTentacle(sweep, origin, dir, 7.5f, 1.8f, -1.1f, 1f);
295: wake.SetSweep(-360f * t / .85f, 70f);
297: if (!hit && player != null && InLane(player.transform.position, origin, dir, 7.5f, .9f))
302: for (float t = 0f; t < .28f; t += Time.deltaTime)
304: float n = t / .28f;
305: PoseTentacle(sweep, origin, Vector3.right, Mathf.Lerp(7.5f, .8f, n), 1.8f, n * 1.5f, 1f - n, 1f - n);
311: private IEnumerator Thrust(Vector3 direction, float length, float width, float windup)
313: Vector3 origin = Position;
321: private IEnumerator AnimateThrust(Vector3 origin, Vector3 direction, float length, float width,
325: PlayMeleeAnimation(.62f);
327: for (float t = 0f; t < .62f; t += Time.deltaTime)
329: float extension = t < .16f ? 1f - Mathf.Pow(1f - t / .16f, 3f)
330: : t < .28f ? 1f : 1f - Mathf.SmoothStep(0f, 1f, (t - .28f) / .34f);
331: float reach = Mathf.Lerp(.8f, length, extension);
332: PoseTentacle(sprite, origin, direction, reach, width, (1f - extension) * 1.6f,
333: extension, Mathf.Min(1f, (.62f - t) * 8f));
335: if (!hit && t < .28f && player != null && InLane(player.transform.position, origin, direction, reach, width * .5f))
345: var warning = Circle(Position, 4f, 1.15f);
346: var ports = new FinalBossNeuralVfx[6];
347: for (int i = 0; i < ports.Length; i++)
349: Vector3 port = Position + Quaternion.Euler(0f, i * 60f, 0f) * Vector3.forward * 3.1f + Vector3.up * 1.6f;
350: ports[i] = FinalBossNeuralVfx.Create(EffectRoot, port, FinalBossNeuralVfx.Shape.Halo, .55f,
351: new Color(.24f, .85f, 1f, .85f));
353: yield return Windup(1.15f);
355: for (int wave = 0; wave < 3; wave++)
357: for (int i = 0; i < 12; i++)
359: Vector3 direction = Quaternion.AngleAxis(i / 2 * 60f + (i % 2 == 0 ? -10f : 10f) + wave * 6f,
360: Vector3.up) * Vector3.forward;
361: FinalBossHomingProjectile.Launch(EffectRoot, ports[i / 2].transform.position,
362: direction, 1f, 5.2f, 48f, 4.5f, "FinalBoss/PhaseThree/HomingNeuralCell");
364: AudioManager.Instance?.PlaySFX("LiverBloodThrow", .5f);
365: yield return new WaitForSeconds(.65f);
373: var warning = Circle(Position, 4f, 1.2f);
374: yield return Windup(1.2f);
377: FinalBossNeuralVfx.Shape.Shield, 4.65f, new Color(.22f, .78f, 1f, .85f));
381: yield return new WaitForSeconds(3f);
390: for (int wave = 0; wave < 4; wave++)
392: Vector3 target = PlayerPosition;
393: StartCoroutine(FallingCell(target, 2.1f, 1.25f, true));
394: for (int i = 0; i < 6; i++)
396: float angle = i * Mathf.PI / 3f + wave * .45f;
397: Vector3 point = target + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * Random.Range(4f, 8f);
398: if (arena.IsWalkable(point, Vector2.one * .3f)) StartCoroutine(FallingCell(point, 1.8f, 1.25f, true));
400: yield return new WaitForSeconds(.8f);
402: yield return new WaitForSeconds(1.8f);
405: private IEnumerator FallingCell(Vector3 point, float radius, float windup, bool bomb)
411: SpriteRenderer sprite = AttackSprite(bomb ? "CellBomb" : "HomingNeuralCell", point, Vector3.right, radius);
412: var fallTrail = sprite != null ? AddEnergyTrail(sprite.transform, .16f, .28f) : null;
416: if (camera != null) sprite.transform.rotation = camera.transform.rotation * Quaternion.Euler(0f, 0f, bomb ? 0f : -90f);
418: for (float t = 0f; t < .4f; t += Time.deltaTime)
422: float n = Mathf.Clamp01(t / .4f);
423: sprite.transform.position = point + Vector3.up * (12f * (1f - n * n));
434: private SpriteRenderer AttackSprite(string name, Vector3 position, Vector3 direction, float width)
441: go.transform.position = Floor(position) + Vector3.up * .2f;
443: renderer.sprite = sprite; renderer.sortingOrder = 5050;
446: go.transform.localScale = Vector3.one * (width / Mathf.Max(.01f, sprite.bounds.size.x));
450: private void Impact(Vector3 point, float radius)
453: radius, new Color(.24f, .76f, 1f, .9f), .52f);
456: nextImpactAccent = Time.time + .12f;
457: DontStarveCamera.Instance?.AddCombatImpulse(.08f, .1f);
458: AudioManager.Instance?.PlaySFX("StomachImpact", .4f);
467: trail.time = duration; trail.minVertexDistance = .1f;
468: trail.startWidth = width; trail.endWidth = 0f;
469: trail.startColor = new Color(.4f, .94f, 1f, .6f);
470: trail.endColor = new Color(.15f, .3f, .65f, 0f);
471: trail.sortingOrder = 5044; trail.emitting = true;
483: echo.sortingOrder = bossRenderer.sortingOrder - 1;
484: for (float t = 0f; t < .24f; t += Time.deltaTime)
485: { echo.color = new Color(.3f, .72f, 1f, .19f * (1f - t / .24f)); yield return null; }
489: private static bool InLane(Vector3 point, Vector3 origin, Vector3 direction, float length, float halfWidth)
491: Vector3 delta = Flat(point - origin);
492: float along = Vector3.Dot(delta, direction);
493: return along >= 0f && along <= length && (delta - direction * along).magnitude <= halfWidth;
496: private static float SegmentDistance(Vector3 point, Vector3 start, Vector3 end)
498: Vector3 delta = Flat(end - start);
499: float t = delta.sqrMagnitude > .0001f ? Mathf.Clamp01(Vector3.Dot(Flat(point - start), delta) / delta.sqrMagnitude) : 0f;
507: collapseStarted = true; collapseElapsed = 0f;
508: AudioManager.Instance?.PlaySFX("BossPhaseChange", .8f);
515: boundaryVisual.positionCount = 4; boundaryVisual.widthMultiplier = .45f;
516: boundaryVisual.sortingOrder = 5090;
517: boundaryVisual.startColor = boundaryVisual.endColor = new Color(.3f, 1f, .92f);
518: for (int i = 0; i < 4; i++)
521: Vector2.one, new Color(.06f, .01f, .13f, .86f));
523: neuralBoundary = FinalBossNeuralVfx.Create(boundaryVisual.transform, Vector3.zero,
524: FinalBossNeuralVfx.Shape.Boundary, 1f, ImpactColor);
530: float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(collapseElapsed / collapseDuration));
531: Vector2 origin = arena.RoomOrigin;
532: Vector2 limit = origin + arena.WorldSize;
533: Vector2 center = origin + arena.WorldSize * .5f;
534: Vector2 half = Vector2.Lerp(new Vector2(arena.WorldSize.x * .5f - 5f, arena.WorldSize.y * .5f - 7f), new Vector2(7f, 6f), t);
535: safeArea = new Rect(center - half, half * 2f);
536: SetCollapsedFloor(0, new Rect(origin.x, origin.y, safeArea.xMin - origin.x, arena.WorldSize.y));
537: SetCollapsedFloor(1, new Rect(safeArea.xMax, origin.y, limit.x - safeArea.xMax, arena.WorldSize.y));
538: SetCollapsedFloor(2, new Rect(safeArea.xMin, origin.y, safeArea.width, safeArea.yMin - origin.y));
539: SetCollapsedFloor(3, new Rect(safeArea.xMin, safeArea.yMax, safeArea.width, limit.y - safeArea.yMax));
543: boundaryVisual.SetPositions(new[] { Floor(new Vector3(safeArea.xMin, 0f, safeArea.yMin)),
544: Floor(new Vector3(safeArea.xMin, 0f, safeArea.yMax)), Floor(new Vector3(safeArea.xMax, 0f, safeArea.yMax)),
545: Floor(new Vector3(safeArea.xMax, 0f, safeArea.yMin)) });
549: nextTimerUpdate = Time.time + 1f;
550: Owner?.SetMobilePattern(patternLabel, TimerHint(), 1.2f);
554: && (TimeRemaining <= 0f || !safeArea.Contains(new Vector2(player.transform.position.x, player.transform.position.z))))
555: { nextBoundaryHit = Time.time + 1f; player.TakeDamage(FinalBossArena.CreateDamage(TimeRemaining <= 0f ? arena.Balance.phaseThree.overtimeDamage : arena.Balance.phaseThree.boundaryDamage)); }
557: Vector3 target = Position;
560: Vector3 step = Flat(target - Position);
561: if (!attacking && step.sqrMagnitude > .001f) TryMove(Position, Vector3.ClampMagnitude(step, 4f * Time.deltaTime));
567: collapsedFloor[index].transform.position = Floor(new Vector3(area.center.x, 0f, area.center.y));
568: collapsedFloor[index].transform.localScale = new Vector3(area.width, 1f, area.height);
571: public bool CanCrossBoundary(Vector3 from, Vector3 to, Vector2 halfExtents)
577: public static bool CanTraverseSafeArea(Rect area, Vector3 from, Vector3 to, Vector2 halfExtents)
582: return newOverflow <= .001f || newOverflow <= oldOverflow + .00001f;
585: private static float BoundaryOverflow(Rect safeArea, Vector3 point, Vector2 extents)
587: return Mathf.Max(0f, safeArea.xMin + extents.x - point.x) + Mathf.Max(0f, point.x + extents.x - safeArea.xMax)
588: + Mathf.Max(0f, safeArea.yMin + extents.y - point.z) + Mathf.Max(0f, point.z + extents.y - safeArea.yMax);
```

## Assets/_Project/Scripts/World/FinalBoss/FinalBossPhaseTwoController.cs

```csharp
19: None = 0,
20: ElectricLines = 1,
21: HomingVolley = 2,
22: CellBombs = 3,
23: HalfArenaSweep = 4,
24: ReflectionShield = 5,
25: CellBarrage = 6
33: [Header("Pattern 1 - Electric Lines")]
40: [Header("Pattern 2 - Homing Volley")]
48: [Header("Pattern 3 - Cell Bombs")]
54: [Header("Pattern 4 - Half Arena")]
58: [Header("Pattern 5 - Reflection")]
63: [Header("Pattern 6 - Cell Barrage")]
71: private static readonly Color WarningColor = new Color(1f, .48f, .03f, .42f);
72: private static readonly Color ElectricColor = new Color(.2f, .78f, 1f, .72f);
73: private static readonly Color BombColor = new Color(1f, .1f, .32f, .72f);
74: private static readonly Color ReflectionColor = new Color(.62f, .22f, 1f, .48f);
77: private readonly List<PhaseTwoPattern> patternBag = new List<PhaseTwoPattern>(6);
83: private Vector3 bossBaseScale = Vector3.one;
109: public int CurrentPhase => phaseThreeStarted ? 3 : 2;
118: : 0f;
183: poissonSalt = 9801,
185: attackDamage = 0f,
186: expReward = 0,
187: moveSpeed = 0f,
188: chaseRadius = 0f,
189: leashRadius = 0f,
193: colliderSize = new Vector3(12.5f, 6f, 4.2f),
194: colliderCenter = new Vector3(0f, 2.25f, 0f),
203: Vector3 targetPosition = bossVisual.position + Vector3.forward * 1.7f;
227: AudioManager.Instance?.PlaySFX("BossPhaseChange", .8f);
228: Vector3 originalScale = bossVisual != null ? bossVisual.localScale : Vector3.one;
229: float elapsed = 0f;
233: float t = Mathf.Clamp01(elapsed / phaseStartDelay);
235: bossVisual.localScale = originalScale * (1f + Mathf.Sin(t * Mathf.PI) * .045f);
237: bossRenderer.color = Color.Lerp(new Color(.44f, .2f, .55f, bossBaseColor.a), bossBaseColor, t);
243: AudioManager.Instance?.PlaySFX("BossRoar", .7f);
244: DontStarveCamera.Instance?.AddCombatImpulse(.2f, .3f);
253: if (!enraged && Health <= MaxHealth * .5f)
256: healthBar?.SetPattern("NEURAL OVERDRIVE", BombColor, "THE CEREBRUM'S PULSE QUICKENS", 1.1f);
257: AudioManager.Instance?.PlaySFX("BossRoar", .65f);
258: recoil = .05f;
259: DontStarveCamera.Instance?.AddCombatImpulse(.15f, .22f);
260: yield return new WaitForSeconds(1.1f);
273: while (active && FinalBossHomingProjectile.ActiveCount > 0) yield return null;
279: float healthRatio = MaxHealth > 0f ? Health / MaxHealth : 1f;
280: float recovery = patternCooldown * Mathf.Lerp(.7f, 1f, healthRatio);
282: recovery = Mathf.Max(2.1f, recovery);
283: healthBar?.SetPattern("CORE EXPOSED", new Color(.48f, .92f, .72f),
310: recoil = Mathf.MoveTowards(recoil, 0f, Time.deltaTime * .18f);
311: float breath = Mathf.Sin(Time.time * (enraged ? 3.6f : 2.2f)) * .012f;
312: float charge = Time.time < windupEnd && windupDuration > 0f
313: ? Mathf.Clamp01(1f - (windupEnd - Time.time) / windupDuration) * .025f : 0f;
314: bossVisual.localScale = Vector3.Scale(bossBaseScale,
315: new Vector3(1f + breath + charge + recoil, 1f - breath * .5f + charge - recoil * .6f, 1f));
318: private void PlayAttackAccent(string sound, float volume = .5f)
320: recoil = .04f;
326: if (patternBag.Count == 0)
334: for (int i = patternBag.Count - 1; i > 0; i--)
336: int swap = Random.Range(0, i + 1);
339: if (patternBag.Count > 1 && patternBag[patternBag.Count - 1] == previousPattern)
341: int swap = Random.Range(0, patternBag.Count - 1);
342: (patternBag[swap], patternBag[patternBag.Count - 1]) =
343: (patternBag[patternBag.Count - 1], patternBag[swap]);
347: int last = patternBag.Count - 1;
383: bool horizontal = Random.value < .5f;
384: int laneTotal = 7;
387: for (int i = 0; i < laneTotal; i++) laneIndices.Add(i);
388: for (int i = laneIndices.Count - 1; i > 0; i--)
390: int swap = Random.Range(0, i + 1);
397: for (int i = 0; i < count; i++)
399: float lane = (laneIndices[i] + .5f) / laneTotal;
400: Vector3 center = ArenaCenter;
401: Vector2 size;
405: size = new Vector2(width, electricLineWidth);
410: size = new Vector2(electricLineWidth, height);
419: PlayAttackAccent("LiverBossImpact", .5f);
429: area.Center + Vector3.up * .04f,
432: 4100);
433: if (electricLine != null) Destroy(electricLine, .3f);
435: .3f, ElectricColor, true);
445: yield return new WaitForSeconds(.3f);
453: missileWarningDuration, new Color(1f, .15f, .5f, .55f));
458: for (int i = 0; i < missileCount && active; i++)
462: Vector3 spawn = bossVisual != null ? bossVisual.position : ArenaCenter;
463: spawn.y = player.transform.position.y + 1.15f;
464: Vector3 direction = player.transform.position - spawn;
465: direction.y = 0f;
466: float spread = Mathf.Lerp(-42f, 42f, missileCount <= 1 ? .5f : i / (float)(missileCount - 1));
467: direction = Quaternion.Euler(0f, spread, 0f) * direction.normalized;
469: missileDamage, missileSpeed, missileTurnSpeed, 6.5f);
470: if (i % 4 == 0) PlayAttackAccent("LiverBloodThrow", .32f);
478: new Color(1f, .25f, .65f, .65f));
482: int count = Mathf.Clamp(barrageCellsPerWave, 12, 36);
483: int waves = Mathf.Clamp(barrageWaveCount, 1, 5);
484: const float fanAngle = 150f;
485: float angleStep = fanAngle / (count - 1);
486: for (int wave = 0; wave < waves && active; wave++)
490: Vector3 origin = bossVisual != null ? bossVisual.position : ArenaCenter;
491: origin.y = player.transform.position.y + 1.15f;
492: Vector3 forward = ArenaCenter - origin;
493: forward.y = 0f;
494: forward = forward.sqrMagnitude > .0001f ? forward.normalized : Vector3.back;
497: float stagger = wave % 2 == 0 ? -angleStep * .25f : angleStep * .25f;
499: $"WAVE {wave + 1}/{waves}  //  WEAVE THROUGH THE GAPS");
500: PlayAttackAccent("LiverBloodBurst", .4f);
501: for (int i = 0; i < count; i++)
503: float angle = -fanAngle * .5f + angleStep * i + stagger;
504: Vector3 direction = Quaternion.Euler(0f, angle, 0f) * forward;
506: runtimeRoot, origin + direction * .8f, direction,
507: barrageCellDamage, barrageCellSpeed, 0f, 5f);
511: 2.4f, .25f, new Color(1f, .25f, .65f, .6f), true);
512: DontStarveCamera.Instance?.AddCombatImpulse(.09f, .15f);
513: if (wave + 1 < waves) yield return new WaitForSeconds(barrageWaveInterval);
515: yield return new WaitForSeconds(2f);
520: var positions = new List<Vector3>(bombCount);
525: int attempts = 0;
526: while (positions.Count < bombCount && attempts++ < 120)
528: Vector3 candidate = arena.UVToWorld(new Vector2(Random.Range(.17f, .83f), Random.Range(.24f, .77f)));
530: if (!arena.IsWalkable(candidate, new Vector2(.3f, .3f))) continue;
532: PlanarDistanceSqr(existing, candidate) < Mathf.Pow(bombRadius * 2f + .6f, 2f));
538: foreach (Vector3 position in positions)
545: float elapsed = 0f;
549: float t = Mathf.Clamp01(elapsed / bombWarningDuration);
551: for (int i = 0; i < fallingBombs.Count; i++)
555: Vector3 position = positions[i];
556: position.y += Mathf.Lerp(9f, .65f, t * t);
569: PlayAttackAccent("StomachImpact", .55f);
570: DontStarveCamera.Instance?.AddCombatImpulse(.13f, .2f);
571: foreach (Vector3 position in positions)
572: FinalBossTelegraph.CreateCircle(runtimeRoot, position, bombRadius, .3f, BombColor, true);
574: var impactScales = new List<Vector3>(fallingBombs.Count);
576: impactScales.Add(bomb != null ? bomb.transform.localScale : Vector3.one);
577: const float burstDuration = .3f;
578: elapsed = 0f;
582: float t = Mathf.Clamp01(elapsed / burstDuration);
583: float burst = 1f - Mathf.Pow(1f - t, 3f);
585: for (int i = 0; i < fallingBombs.Count; i++)
589: bomb.transform.position = positions[i] + Vector3.up * Mathf.Lerp(.65f, 1.05f, t);
590: bomb.transform.localScale = impactScales[i] * Mathf.Lerp(1f, 2.15f, burst);
596: color.a = 1f - t;
608: int side = Random.Range(0, 2);
609: float direction = side == 0 ? -1f : 1f;
610: Vector3 center = ArenaCenter;
611: Vector2 size = new Vector2(CombatWidth * .5f, CombatHeight);
612: center.x += direction * CombatWidth * .25f;
616: sweepWarningDuration, new Color(1f, .05f, .08f, .55f));
622: float windupAngle = direction * 54f;
623: float elapsed = 0f;
627: float t = Mathf.Clamp01(elapsed / sweepWarningDuration);
628: float anticipation = Mathf.SmoothStep(0f, 1f, t);
629: float angle = Mathf.Lerp(direction * -22f, windupAngle, anticipation);
630: float warningLength = Mathf.Lerp(8.5f, 11.5f, anticipation)
631: + Mathf.Sin(elapsed * 13f) * .22f;
632: SetTentaclePose(tentacle, direction, angle, warningLength, 5.6f);
635: float pulse = .56f + Mathf.Sin(elapsed * 16f) * .12f + t * .25f;
636: tentacleRenderer.color = new Color(1f, .26f, .46f, Mathf.Clamp01(pulse));
641: const float swingDuration = .66f;
653: afterimageRenderer.sortingOrder = tentacleRenderer.sortingOrder - 1;
656: elapsed = 0f;
660: float t = Mathf.Clamp01(elapsed / swingDuration);
665: new Color(1f, .92f, .98f, 1f),
666: new Color(.62f, .17f, .72f, .72f),
667: Mathf.Clamp01((t - .55f) / .45f));
669: float trailT = t - .075f;
670: if (trailT > 0f && afterimage != null)
675: SetTentaclePose(afterimage, direction, trailAngle, trailLength, trailThickness * 1.05f);
678: float trailAlpha = Mathf.Sin(Mathf.Clamp01(trailT) * Mathf.PI) * .34f;
679: afterimageRenderer.color = new Color(.28f, .9f, 1f, trailAlpha);
683: if (!dealtDamage && t >= .34f)
686: PlayAttackAccent("IntestineBossImpact", .55f);
687: DontStarveCamera.Instance?.AddCombatImpulse(.22f, .24f);
701: Vector3 center = GroundPosition(bossVisual != null ? bossVisual.position + Vector3.forward * 1.6f : ArenaCenter);
702: FinalBossTelegraph warning = FinalBossTelegraph.CreateCircle(runtimeRoot, center, 6.2f,
711: 15.5f,
712: bossRenderer != null ? bossRenderer.sortingOrder + 6 : 5200,
714: Vector3 shieldScale = shield != null ? shield.transform.localScale : Vector3.one;
715: float elapsed = 0f;
719: float t = Mathf.Clamp01(elapsed / reflectionWarningDuration);
721: if (shield != null) shield.transform.localScale = shieldScale * Mathf.Lerp(.68f, 1f, t);
723: shieldRenderer.color = new Color(.78f, .42f, 1f, Mathf.Lerp(.15f, .78f, t));
731: PlayAttackAccent("BossPhaseChange", .35f);
732: nextReflectionHitTime = 0f;
735: if (bossRenderer != null) bossRenderer.color = new Color(.72f, .38f, 1f, bossBaseColor.a);
736: elapsed = 0f;
742: shield.transform.localScale = shieldScale * (1f + Mathf.Sin(elapsed * 5.5f) * .025f);
744: shieldRenderer.color = new Color(.86f, .64f, 1f, .76f + Mathf.Sin(elapsed * 8f) * .14f);
766: nextReflectionHitTime = Time.time + .45f;
767: player.TakeDamage(FinalBossArena.CreateDamage(Mathf.Clamp(damage * reflectedDamageRatio, 1f, 2f)));
779: bossHitFlash?.Flash(new Color(1f, .76f, .84f), .1f);
780: recoil = Mathf.Max(recoil, .018f);
783: AudioManager.Instance?.PlaySFX("EnemyHit", .4f);
784: nextHitSoundTime = Time.time + .15f;
808: map.SetAuthoredAreaBlocked(arena.RoomToMapArea(new RectInt(17, 30, 14, 5)), false);
812: map.SetAuthoredAreaBlocked(arena.RoomToMapArea(new RectInt(17, 30, 14, 5)), true);
834: healthBar?.SetPattern("PHASE III  //  UNBOUND", new Color(1f, .35f, .55f));
845: internal void SetMobilePattern(string label, string hint, float duration = 2f)
847: healthBar?.SetPattern(label, new Color(1f, .38f, .65f), hint, duration);
869: healthBar.SetValue(0f, 0f, MaxHealth);
870: healthBar.SetPattern("CEREBRUM SILENCED", new Color(.55f, .86f, 1f), "FINAL BOSS DEFEATED", 3f);
881: DontStarveCamera.Instance?.AddCombatImpulse(.32f, .4f);
882: AudioManager.Instance?.PlaySFX("BossDeath", .85f);
892: const float duration = 1.6f;
896: Color corpseColor = animatedCorpse ? new Color(.7f, .65f, .72f, bossBaseColor.a)
897: : new Color(.24f, .18f, .28f, bossBaseColor.a);
898: for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
900: float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
905: bossVisual.localScale = Vector3.Scale(bossBaseScale, new Vector3(1f - t * .04f, 1f - t * .12f, 1f));
909: yield return new WaitForSeconds(1.4f);
923: default: return 0f;
943: Vector3 center = bossVisual != null ? bossVisual.position + Vector3.forward * 1.5f : ArenaCenter;
945: runtimeRoot, GroundPosition(center), 3.6f, duration, color);
948: private GameObject CreateFallingBomb(Vector3 target)
952: bomb.transform.position = target + Vector3.up * 9f;
956: renderer.color = cellBombSprite != null ? Color.white : new Color(.85f, .06f, .28f, 1f);
957: renderer.sortingOrder = 5200;
958: float spriteWidth = sprite != null ? Mathf.Max(.01f, sprite.bounds.size.x) : 1f;
959: bomb.transform.localScale = Vector3.one * (2.1f / spriteWidth);
974: Vector3 center,
975: Vector2 size,
983: Vector2 bounds = sprite.bounds.size;
985: visual.transform.rotation = Quaternion.Euler(90f, 0f, 0f)
986: * Quaternion.Euler(0f, 0f, vertical ? 90f : 0f);
988: ? new Vector3(
989: size.y / Mathf.Max(.01f, bounds.x),
990: size.x / Mathf.Max(.01f, bounds.y),
991: 1f)
992: : new Vector3(
993: size.x / Mathf.Max(.01f, bounds.x),
994: size.y / Mathf.Max(.01f, bounds.y),
995: 1f);
1007: GameObject tentacle = new GameObject(direction < 0f
1013: renderer.color = new Color(1f, .28f, .48f, .5f);
1014: renderer.sortingOrder = 6300;
1015: SetTentaclePose(tentacle, direction, direction * -22f, 8.5f, 5.6f);
1032: tentacle.transform.rotation = facing * Quaternion.Euler(0f, 0f, angle);
1033: Vector2 spriteSize = tentacleSprite != null ? tentacleSprite.bounds.size : Vector2.one;
1034: float lengthScale = Mathf.Max(.1f, desiredLength) / Mathf.Max(.01f, spriteSize.x);
1035: float thicknessScale = Mathf.Max(.1f, desiredThickness) / Mathf.Max(.01f, spriteSize.y);
1036: tentacle.transform.localScale = new Vector3(
1049: float t = Mathf.Clamp01(normalized);
1050: const float extensionEnd = .34f;
1053: float extension = Mathf.SmoothStep(0f, 1f, t / extensionEnd);
1054: angle = Mathf.Lerp(direction * 54f, direction * -62f, extension);
1055: length = Mathf.Lerp(11.5f, 34f, extension);
1056: thickness = Mathf.Lerp(5.6f, 7.4f, Mathf.Sin(extension * Mathf.PI * .5f));
1060: float sweep = Mathf.SmoothStep(0f, 1f, (t - extensionEnd) / (1f - extensionEnd));
1061: angle = Mathf.Lerp(direction * -62f, direction * -14f, sweep);
1062: length = Mathf.Lerp(34f, 31f, sweep);
1063: thickness = Mathf.Lerp(7.4f, 6.4f, sweep);
1066: private Vector3 GetTentacleAnchor(float direction)
1068: if (bossVisual == null) return ArenaCenter + Vector3.up * 3f;
1070: + bossVisual.right * (direction * 4.75f)
1071: + bossVisual.up * 2.7f;
1077: Vector3 position,
1090: float scale = desiredWidth / Mathf.Max(.01f, sprite.bounds.size.x);
1091: visual.transform.localScale = Vector3.one * scale;
1096: private Vector3 GetBossSpriteCenter()
1098: if (bossVisual == null) return ArenaCenter + Vector3.up * 3f;
1099: return bossVisual.position + bossVisual.up * 3.25f;
1109: private Vector3 GroundPosition(Vector3 position)
1111: position.y = GetGroundHeight(position) + .08f;
1115: private float GetGroundHeight(Vector3 position)
1121: private Vector3 ArenaCenter => new Vector3(
1122: ArenaMinX + CombatWidth * .5f,
1123: 0f,
1124: ArenaMinZ + CombatHeight * .5f);
1125: private float ArenaMinX => arena.RoomOrigin.x + 5f;
1126: private float ArenaMinZ => arena.RoomOrigin.y + 7f;
1127: private float CombatWidth => Mathf.Max(8f, arena.WorldSize.x - 10f);
1128: private float CombatHeight => Mathf.Max(8f, arena.WorldSize.y - 14f);
1130: private static float PlanarDistanceSqr(Vector3 a, Vector3 b)
1155: case PhaseTwoPattern.ElectricLines: return new Color(.28f, .86f, 1f);
1156: case PhaseTwoPattern.HomingVolley: return new Color(1f, .3f, .66f);
1157: case PhaseTwoPattern.CellBombs: return new Color(1f, .2f, .3f);
1158: case PhaseTwoPattern.HalfArenaSweep: return new Color(1f, .12f, .24f);
1159: case PhaseTwoPattern.ReflectionShield: return new Color(.72f, .38f, 1f);
1160: case PhaseTwoPattern.CellBarrage: return new Color(1f, .25f, .65f);
1161: default: return new Color(.72f, .52f, .72f);
1167: public AttackArea(Vector3 center, Vector2 size)
1173: public Vector3 Center { get; }
1174: public Vector2 Size { get; }
1176: public bool Contains(Vector3 point)
1178: return Mathf.Abs(point.x - Center.x) <= Size.x * .5f
1179: && Mathf.Abs(point.z - Center.z) <= Size.y * .5f;
```

## Assets/_Project/Scripts/World/FinalBoss/FinalBossPillar.cs

```csharp
13: [SerializeField, Min(1f)] private float maxHealth = 65f;
23: private Vector3 intactScale;
24: private Vector3 intactPosition;
30: private float linkVisibility = 1f;
36: public int DestructionFrameIndex { get; private set; } = -1;
37: public int DestructionFrameCount => destructionFrames != null ? destructionFrames.Length : 0;
38: public float Health => damageTarget != null && damageTarget.Stats != null ? damageTarget.Stats.CurrentHealth : 0f;
51: linkVisibility = 0f;
76: poissonSalt = 9100 + (int)biome,
78: attackDamage = 0f,
79: expReward = 0,
80: moveSpeed = 0f,
81: chaseRadius = 0f,
82: leashRadius = 0f,
86: colliderSize = new Vector3(Mathf.Max(2.5f, blockedArea.width - .7f), 6f, Mathf.Max(1.8f, blockedArea.height - .5f)),
87: colliderCenter = new Vector3(0f, 2.5f, 0f),
109: healthBar.SetValue(1f);
125: neuralLink.positionCount = 20;
127: neuralLink.widthMultiplier = .065f;
128: neuralLink.sortingOrder = 4050;
137: destroyed || owner.CurrentPhase != 1 ? 0f : 1f, Time.deltaTime * 2.5f);
138: neuralLink.enabled = linkVisibility > .001f;
140: Vector3 from = transform.position;
141: Vector3 to = owner.BossGroundPosition;
143: Vector3 bend = Vector3.Cross((to - from).normalized, Vector3.up) * 1.2f;
144: for (int i = 0; i < neuralLink.positionCount; i++)
146: float t = i / (float)(neuralLink.positionCount - 1);
147: neuralLink.SetPosition(i, Vector3.Lerp(from, to, t) + bend * Mathf.Sin(t * Mathf.PI));
150: color.a = (.19f + .09f * Mathf.Sin(Time.time * 2.4f + (int)biome)) * linkVisibility;
152: color.a *= .4f;
175: if (pillarRenderer == null || pillarRenderer.sprite == null) return 5f;
176: return Mathf.Clamp(pillarRenderer.bounds.size.y + .55f, 3.8f, 7f);
183: BiomeType.Intestine => new Color(.35f, .82f, .58f),
184: BiomeType.Liver => new Color(.76f, .37f, .75f),
185: BiomeType.Stomach => new Color(.92f, .55f, .18f),
186: BiomeType.Lung => new Color(.94f, .78f, .51f),
187: _ => new Color(.9f, .2f, .3f)
197: hitFlash?.Flash(new Color(1f, .9f, .55f), .11f);
204: Vector3 basePosition = intactPosition;
205: Vector3 baseScale = intactScale;
206: const float duration = .14f;
207: for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
209: float strength = 1f - elapsed / duration;
210: transform.localPosition = basePosition + new Vector3(Mathf.Sin(elapsed * 95f) * .11f * strength, 0f, 0f);
211: transform.localScale = baseScale * (1f + .045f * strength);
229: if (destructionFrames != null && destructionFrames.Length == 5)
241: for (int i = 0; i < destructionFrames.Length; i++)
245: if (i < destructionFrames.Length - 1)
257: BiomeType.Stomach => index switch { 0 => .12f, 1 => .14f, 2 => .20f, _ => .26f },
258: BiomeType.Intestine => index switch { 0 => .16f, 1 => .22f, 2 => .30f, _ => .36f },
259: BiomeType.Liver => index switch { 0 => .14f, 1 => .22f, 2 => .24f, _ => .32f },
260: _ => index switch { 0 => .24f, 1 => .30f, 2 => .24f, _ => .32f }
283: if (!destroyed && damageTarget != null) damageTarget.TakeDamage(MaxHealth * 2f);
```

## Assets/_Project/Scripts/World/FinalBoss/FinalBossScreenHealthBar.cs

```csharp
11: private const int SegmentCount = 4;
13: private const float GaugeVerticalCenter = 222f;
14: private static readonly Color NormalAccent = new Color(.96f, .07f, .3f, 1f);
15: private static readonly Color ReflectionAccent = new Color(.68f, .3f, 1f, 1f);
16: private static readonly Color TrailColor = new Color(1f, .55f, .18f, .9f);
21: private readonly Image[] nodeImages = new Image[SegmentCount + 1];
23: private readonly float[] sealValues = { 1f, 1f, 1f, 1f };
34: private float targetValue = 1f;
35: private float displayedValue = 1f;
36: private float trailingValue = 1f;
52: canvas.sortingOrder = 7600;
56: scaler.referenceResolution = new Vector2(1920f, 1080f);
58: scaler.matchWidthOrHeight = .5f;
62: gauge.canvasGroup.alpha = 0f;
66: gauge.SetValue(maxHealth > 0f ? currentHealth / maxHealth : 0f, currentHealth, maxHealth);
67: gauge.revealTimer = 2.2f;
77: new Color(.025f, .005f, .022f, .72f));
81: new Vector2(.5f, 0f),
82: new Vector2(650f, 70f),
83: new Vector2(0f, 205f));
86: shadow.effectColor = new Color(0f, 0f, 0f, .8f);
87: shadow.effectDistance = new Vector2(0f, -5f);
94: 16,
95: new Color(1f, .78f, .88f, .95f));
100: new Vector2(.5f, .5f),
101: new Vector2(420f, 24f),
102: new Vector2(0f, 20f));
107: new Color(NormalAccent.r, NormalAccent.g, NormalAccent.b, .48f));
110: new Vector2(.5f, .5f),
111: new Vector2(570f, 3f),
112: new Vector2(0f, -13f));
114: const float segmentWidth = 130f;
115: const float gap = 12f;
116: float startX = -((segmentWidth + gap) * (SegmentCount - 1)) * .5f;
117: for (int i = 0; i < SegmentCount; i++)
120: BuildSegment(panel.transform, i, new Vector2(x, -13f), segmentWidth);
122: 11, new Color(.85f, .86f, .92f));
124: RuntimeUiFactory.SetRect(sealLabels[i].rectTransform, new Vector2(.5f, .5f),
125: new Vector2(segmentWidth, 16f), new Vector2(x, 3f));
129: 13, new Color(.78f, .8f, .88f));
131: RuntimeUiFactory.SetRect(hintText.rectTransform, new Vector2(.5f, .5f),
132: new Vector2(590f, 20f), new Vector2(0f, -34f));
135: for (int i = 0; i <= SegmentCount; i++)
137: float x = startX - segmentWidth * .5f - gap * .5f + i * (segmentWidth + gap);
138: nodeImages[i] = CreateNode(panel.transform, new Vector2(x, -13f));
144: new Color(1f, .75f, .86f, 0f));
147: UpdateSegmentVisuals(1f, 1f);
151: private void BuildSegment(Transform parent, int index, Vector2 position, float width)
154: $"Stage_{index + 1}_Frame",
156: new Color(.2f, .035f, .13f, .96f));
157: RuntimeUiFactory.SetRect(frame.rectTransform, new Vector2(.5f, .5f),
158: new Vector2(width, 21f), position);
163: new Color(.075f, .012f, .06f, 1f));
164: RuntimeUiFactory.Stretch(background.rectTransform, new Vector2(3f, 3f), new Vector2(-3f, -3f));
178: new Color(1f, .72f, .86f, .22f));
179: gloss.rectTransform.anchorMin = new Vector2(0f, .58f);
180: gloss.rectTransform.anchorMax = Vector2.one;
181: gloss.rectTransform.offsetMin = Vector2.zero;
182: gloss.rectTransform.offsetMax = Vector2.zero;
185: private static Image CreateNode(Transform parent, Vector2 position)
188: RuntimeUiFactory.SetRect(node.rectTransform, new Vector2(.5f, .5f),
189: new Vector2(13f, 13f), position);
190: node.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
196: float next = Mathf.Clamp01(normalized);
197: if (next < targetValue - .0001f)
199: flashStrength = 1f;
200: revealTimer = 1.8f;
205: public void SetPattern(string label, Color accent, string hint = "", float duration = 0f)
209: RuntimeUiFactory.SetRect(panelRect, new Vector2(.5f, 0f),
210: new Vector2(650f, 104f), new Vector2(0f, GaugeVerticalCenter));
211: stateText.rectTransform.anchoredPosition = new Vector2(0f, 31f);
215: stateDuration = Mathf.Max(0f, duration);
216: stateElapsed = 0f;
225: ? new Color(.86f, .68f, 1f, 1f)
226: : new Color(1f, .78f, .88f, .95f);
228: revealTimer = 2f;
235: RuntimeUiFactory.SetRect(panelRect, new Vector2(.5f, 0f),
236: new Vector2(650f, 104f), new Vector2(0f, GaugeVerticalCenter));
237: stateText.rectTransform.anchoredPosition = new Vector2(0f, 31f);
243: for (int i = 0; i < SegmentCount; i++)
253: SetValue(total > 0 ? (total - destroyed) / (float)total : 0f, 0f, 0f);
254: ApplyAccent(new Color(.28f, .86f, 1f));
268: revealTimer = Mathf.Max(0f, revealTimer - delta);
269: float desiredAlpha = objectiveVisible || reflecting || stateElapsed < stateDuration || revealTimer > 0f ? 1f : .62f;
271: canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, desiredAlpha, delta * 3.8f);
273: displayedValue = Mathf.MoveTowards(displayedValue, targetValue, delta * 2.4f);
274: float trailSpeed = trailingValue > displayedValue ? .22f : 2.4f;
280: flashStrength = Mathf.MoveTowards(flashStrength, 0f, delta * 3.5f);
282: hitFlash.color = new Color(1f, .72f, .86f, flashStrength * .36f);
287: for (int i = 0; i < SegmentCount; i++)
291: float value = intact && pillar.MaxHealth > 0f ? pillar.Health / pillar.MaxHealth : 0f;
292: sealValues[i] = Mathf.MoveTowards(sealValues[i], value, delta * 1.8f);
294: ApplyFill(segmentTrails[i], 0f);
297: nodeImages[i].color = intact ? accent : new Color(.18f, .13f, .2f);
298: sealLabels[i].color = intact ? Color.Lerp(accent, Color.white, .55f) : new Color(.4f, .35f, .43f);
304: for (int i = 0; i < SegmentCount; i++)
306: ApplyFill(segmentFills[i], Mathf.Clamp01(fillValue * SegmentCount - i));
307: ApplyFill(segmentTrails[i], Mathf.Clamp01(trailValue * SegmentCount - i));
313: for (int i = 0; i < fillImages.Length; i++)
315: for (int i = 0; i < nodeImages.Length; i++)
318: neuralLine.color = new Color(accent.r, accent.g, accent.b, .48f);
324: rect.anchorMin = Vector2.zero;
325: rect.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1f);
326: rect.offsetMin = Vector2.zero;
327: rect.offsetMax = Vector2.zero;
```

## Assets/_Project/Scripts/World/FinalBoss/FinalBossSpriteAnimator.cs

```csharp
12: private readonly Sprite[][] frames = new Sprite[6][];
22: || (oneShotDuration > 0f && elapsed < oneShotDuration);
23: public Sprite IdleSprite => initialized ? frames[(int)Pose.Idle][0] : null;
33: if (sequence.Length != 4)
44: public void Play(Pose pose, float duration = 0f, bool restart = false)
50: elapsed = 0f;
52: spriteRenderer.sprite = frames[(int)pose][0];
60: elapsed += Mathf.Max(0f, deltaTime);
68: if (count <= 1) return 0;
69: time = Mathf.Max(0f, time);
70: float fps = pose == Pose.Move ? 9f : pose == Pose.Idle ? 5f : 7f;
71: if (duration > 0f || pose == Pose.Death)
73: float length = duration > 0f ? duration : 1.6f;
74: return Mathf.Min(count - 1, Mathf.FloorToInt(time / length * count));
```

## Assets/_Project/Scripts/World/FinalBoss/FinalBossTelegraph.cs

```csharp
29: public static FinalBossTelegraph CreateFloorOverlay(Transform parent, Vector3 center,
30: Vector2 size, Color tint)
33: new Vector3(Mathf.Max(.05f, size.x), 1f, Mathf.Max(.05f, size.y)),
34: GetRectangleMesh(), 3600f, tint, true, false);
42: countdownDuration = Mathf.Max(.01f, windup);
43: color.a = .16f;
47: boundary.widthMultiplier = .055f;
48: boundary.startColor = boundary.endColor = new Color(1f, .15f, .2f, .38f);
54: countdown.positionCount = 0;
55: countdown.widthMultiplier = .10f; countdown.sortingOrder = 4083;
56: countdown.numCapVertices = 2; countdown.shadowCastingMode = ShadowCastingMode.Off;
61: Vector3 center,
71: new Vector3(Mathf.Max(.05f, radius), 1f, Mathf.Max(.05f, radius)),
80: Vector3 center,
81: Vector2 size,
90: new Vector3(Mathf.Max(.05f, size.x), 1f, Mathf.Max(.05f, size.y)),
100: Vector3 center,
101: Vector3 scale,
117: renderer.sortingOrder = isImpact ? 4090 : 4080;
124: warning.duration = Mathf.Max(.02f, lifeTime);
135: float normalized = Mathf.Clamp01(elapsed / duration);
138: float charge = Mathf.Clamp01(elapsed / countdownDuration);
139: Color fill = color; fill.a *= .6f + charge * .4f;
141: boundary.startColor = boundary.endColor = new Color(1f, .15f, .2f, .38f);
142: int count = Mathf.Max(2, Mathf.CeilToInt(charge * 64f) + 1);
144: for (int i = 0; i < count; i++)
145: countdown.SetPosition(i, WarningPerimeter(charge * i / (count - 1)));
146: countdown.startColor = countdown.endColor = Color.Lerp(new Color(1f, .5f, .25f, .85f),
147: new Color(1f, .18f, .24f, 1f), charge);
152: ? 1f - normalized
153: : Mathf.Lerp(.42f, 1f, normalized) * (.82f + Mathf.Sin(elapsed * 17f) * .18f);
155: current.a *= Mathf.Clamp01(pulse);
158: current = Color.Lerp(current, new Color(1f, .06f, .03f, current.a), normalized * .72f);
164: edge.a = impact ? 1f - normalized : Mathf.Lerp(.65f, 1f, normalized);
198: boundary.widthMultiplier = .09f;
199: boundary.numCornerVertices = 3;
200: boundary.sortingOrder = impact ? 4092 : 4082;
203: boundary.positionCount = circle ? 64 : 4;
206: for (int i = 0; i < boundary.positionCount; i++)
208: float angle = i * Mathf.PI * 2f / boundary.positionCount;
209: boundary.SetPosition(i, new Vector3(Mathf.Cos(angle), .015f, Mathf.Sin(angle)));
215: new Vector3(-.5f, .015f, -.5f), new Vector3(-.5f, .015f, .5f),
216: new Vector3(.5f, .015f, .5f), new Vector3(.5f, .015f, -.5f) });
221: private Vector3 WarningPerimeter(float t)
225: float angle = (t * 360f + 90f) * Mathf.Deg2Rad;
226: return new Vector3(Mathf.Cos(angle), .025f, Mathf.Sin(angle));
228: float p = Mathf.Clamp01(t) * 4f;
229: if (p < 1f) return new Vector3(-.5f, .025f, p - .5f);
230: if (p < 2f) return new Vector3(p - 1.5f, .025f, .5f);
231: if (p < 3f) return new Vector3(.5f, .025f, 2.5f - p);
232: return new Vector3(3.5f - p, .025f, -.5f);
244: renderQueue = 3100
246: sharedMaterial.mainTexture = Texture2D.whiteTexture;
259: new Vector3(-.5f, 0f, -.5f), new Vector3(-.5f, 0f, .5f),
260: new Vector3(.5f, 0f, .5f), new Vector3(.5f, 0f, -.5f)
262: uv = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right },
263: triangles = new[] { 0, 1, 2, 0, 2, 3 }
273: const int segments = 48;
274: var vertices = new Vector3[segments + 1];
275: var uv = new Vector2[segments + 1];
276: var triangles = new int[segments * 3];
277: vertices[0] = Vector3.zero;
278: uv[0] = new Vector2(.5f, .5f);
279: for (int i = 0; i < segments; i++)
281: float angle = Mathf.PI * 2f * i / segments;
284: vertices[i + 1] = new Vector3(x, 0f, z);
285: uv[i + 1] = new Vector2(x * .5f + .5f, z * .5f + .5f);
286: int next = (i + 1) % segments;
287: int index = i * 3;
288: triangles[index] = 0;
289: triangles[index + 1] = i + 1;
290: triangles[index + 2] = next + 1;
```

## Assets/_Project/Scripts/World/FinalBoss/FinalBossTentacleVisual.cs

```csharp
9: private const int Segments = 40;
10: private readonly Vector3[] vertices = new Vector3[(Segments + 1) * 2];
11: private readonly Vector2[] uv = new Vector2[(Segments + 1) * 2];
12: private readonly Color[] colors = new Color[(Segments + 1) * 2];
17: private readonly Vector3[] nervePoints = new Vector3[25];
41: var triangles = new int[Segments * 6];
42: for (int i = 0; i < Segments; i++)
44: int v = i * 2, t = i * 6;
45: triangles[t] = v; triangles[t + 1] = v + 1; triangles[t + 2] = v + 2;
46: triangles[t + 3] = v + 2; triangles[t + 4] = v + 1; triangles[t + 5] = v + 3;
52: body.sortingOrder = 5046;
55: nerve = FinalBossNeuralVfx.CreateLine(transform, "LivingAxon", .055f, 5047);
59: public void Pose(Vector3 root, Vector3 tip, float width, float curl, float energy, float alpha = 1f)
63: Vector3 forward = camera != null ? camera.transform.forward : new Vector3(0f, -.7f, .7f);
64: Vector3 delta = tip - root;
65: float length = Mathf.Max(.1f, delta.magnitude);
66: Vector3 side = Vector3.Cross(Vector3.up, delta.normalized);
67: Vector3 control = (root + tip) * .5f + Vector3.up * Mathf.Min(1.8f, length * .18f) + side * curl;
68: for (int i = 0; i <= Segments; i++)
71: Vector3 p = Curve(root, control, tip, t);
72: Vector3 tangent = ((control - root) * (1f - t) + (tip - control) * t).normalized;
73: Vector3 across = Vector3.Cross(forward, tangent).normalized;
74: if (across.sqrMagnitude < .01f) across = Vector3.up;
76: float cap = Mathf.Min(.32f, 1.5f / length);
77: float u = t < cap ? .23f + t / cap * .12f : t > 1f - cap
78: ? .76f + (t - (1f - cap)) / cap * .24f
79: : .35f + (t - cap) / (1f - cap * 2f) * .41f;
80: float halfWidth = width * .68f * Mathf.Lerp(1.1f, .85f, t);
81: vertices[i * 2] = transform.InverseTransformPoint(p - across * halfWidth);
82: vertices[i * 2 + 1] = transform.InverseTransformPoint(p + across * halfWidth);
83: uv[i * 2] = new Vector2(u, 0f); uv[i * 2 + 1] = new Vector2(u, 1f);
84: Color tint = Color.Lerp(new Color(.82f, .63f, .72f, alpha), new Color(1f, .96f, 1f, alpha), energy);
85: tint.a *= Mathf.Clamp01(t * 14f);
86: colors[i * 2] = colors[i * 2 + 1] = tint;
89: for (int i = 0; i < nervePoints.Length; i++)
91: float t = i / (float)(nervePoints.Length - 1) * .88f;
92: nervePoints[i] = Curve(root, control, tip, t) - forward * .04f
93: + side * Mathf.Sin(t * 36f - Time.time * 18f) * .055f * Mathf.Sin(t * Mathf.PI);
96: nerve.startColor = new Color(.18f, .62f, .75f, energy * alpha * .65f);
97: nerve.endColor = new Color(.55f, 1f, 1f, energy * alpha);
98: nerve.widthMultiplier = Mathf.Lerp(.025f, .075f, energy);
101: private static Vector3 Curve(Vector3 a, Vector3 b, Vector3 c, float t)
102: => (1f - t) * (1f - t) * a + 2f * (1f - t) * t * b + t * t * c;
```

## Assets/_Project/Scripts/World/FinalBoss/FinalBossWorldHealthBar.cs

```csharp
20: root.transform.localPosition = Vector3.up * height;
28: width = 3.4f;
29: CreatePart("Frame", new Color(.07f, .035f, .06f, .96f), new Vector3(width + .18f, .38f, 1f), 5998, out _);
30: CreatePart("Background", new Color(.18f, .08f, .12f, .96f), new Vector3(width, .22f, 1f), 5999, out _);
31: CreatePart("Fill", fillColor, new Vector3(width, .22f, 1f), 6000, out fill);
34: private void CreatePart(string objectName, Color color, Vector3 scale, int order, out Transform part)
49: float value = Mathf.Clamp01(normalized);
50: fill.localScale = new Vector3(width * value, .22f, 1f);
51: fill.localPosition = new Vector3((value - 1f) * width * .5f, 0f, -.01f);
61: transform.position = artwork.transform.TransformPoint(new Vector3(0f, artwork.sprite.bounds.max.y, 0f))
62: + camera.transform.up * .5f;
68: Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
74: texture.SetPixel(0, 0, Color.white);
76: whiteSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1f);
```

## Assets/_Project/Scripts/World/Hub/Altar.cs

```csharp
12: [SerializeField] private float interactionRadius = 3f;
16: [SerializeField] private Transform[] relicSlots = new Transform[4];
17: [SerializeField] private GameObject[] relicVisuals = new GameObject[4];
80: Debug.Log($"[Altar] 부산물이 부족합니다. ({collected}/4)");
145: for (int i = 0; i < relicVisuals.Length; i++)
184: for (int i = 0; i < biomes.Length && i < relicVisuals.Length; i++)
201: private void OnTriggerEnter2D(Collider2D other)
222: Gizmos.DrawWireCube(slot.position, Vector3.one * 0.5f);
```

## Assets/_Project/Scripts/World/Hub/HubBiomePortal.cs

```csharp
15: [SerializeField] private float interactionRadius = 2f;
59: float wallWidth = 0.3f;
60: float wallHeight = 3f;
61: float wallDepth = 2f;
62: float portalWidth = 1.5f;
67: leftWall.transform.localPosition = new Vector3(-portalWidth, 0, 0);
70: leftCol.size = new Vector3(wallWidth, wallHeight, wallDepth);
76: rightWall.transform.localPosition = new Vector3(portalWidth, 0, 0);
79: rightCol.size = new Vector3(wallWidth, wallHeight, wallDepth);
212: if (Time.timeSinceLevelLoad < 1f) return;
221: private void OnTriggerEnter2D(Collider2D other)
224: if (Time.timeSinceLevelLoad < 1f) return;
```

## Assets/_Project/Scripts/World/Hub/HubMapRenderer.cs

```csharp
11: [SerializeField] private int mapWidth = 30;
12: [SerializeField] private int mapHeight = 30;
13: [SerializeField] private float tileSize = 1f;
18: [SerializeField] private Texture2D mapTexture;  // Texture로도 사용 가능
21: [SerializeField] private int wallThickness = 2;  // 가장자리 벽 두께
47: public void Initialize(Texture2D backgroundTexture)
62: float centerX = mapWorldWidth / 2f;
63: float centerZ = mapWorldHeight / 2f;
80: mapBackground.transform.position = new Vector3(centerX, -0.01f, centerZ);
83: mapBackground.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
86: mapBackground.transform.localScale = new Vector3(mapWorldWidth, mapWorldHeight, 1f);
89: Texture2D tex = null;
114: mat.color = new Color(0.3f, 0.1f, 0.1f);
132: for (int x = 0; x < mapWidth; x++)
134: for (int y = 0; y < mapHeight; y++)
180: return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
195: public bool IsWalkableWorld(Vector3 worldPos)
197: Vector2Int grid = WorldToGrid(worldPos);
204: public Vector3 GridToWorld(int gridX, int gridY)
206: float worldX = gridX * tileSize + tileSize / 2f;
207: float worldZ = gridY * tileSize + tileSize / 2f;
208: return new Vector3(worldX, 0f, worldZ);
214: public Vector2Int WorldToGrid(Vector3 worldPos)
218: return new Vector2Int(gridX, gridY);
224: public Vector3 GetPlayerSpawnPosition()
226: return GridToWorld(mapWidth / 2, mapHeight / 2 - 3);
232: public Vector3 GetAltarPosition()
234: return GridToWorld(mapWidth / 2, mapHeight / 2);
240: public Vector3[] GetPortalPositions()
242: int topY = mapHeight - 5;  // 위쪽에서 5칸 아래
243: int spacing = 6;           // 포털 간격
244: int startX = mapWidth / 2 - (spacing * 3 / 2);  // 중앙 기준 시작점
246: return new Vector3[]
250: GridToWorld(startX + spacing * 2, topY),      // 위
251: GridToWorld(startX + spacing * 3, topY)       // 폐
260: Vector3 center = new Vector3(mapWidth * tileSize / 2f, 0, mapHeight * tileSize / 2f);
261: Vector3 size = new Vector3(mapWidth * tileSize, 0.2f, mapHeight * tileSize);
265: Gizmos.color = new Color(0, 1, 0, 0.2f);
266: Vector3 walkableCenter = new Vector3(mapWidth * tileSize / 2f, 0, mapHeight * tileSize / 2f);
267: Vector3 walkableSize = new Vector3(
268: (mapWidth - wallThickness * 2) * tileSize,
269: 0.1f,
270: (mapHeight - wallThickness * 2) * tileSize
278: Gizmos.DrawWireSphere(pos, 1.5f);
283: Gizmos.DrawWireSphere(GetAltarPosition(), 2f);
287: Gizmos.DrawWireSphere(GetPlayerSpawnPosition(), 1f);
```

## Assets/_Project/Scripts/World/Hub/HubRoom.cs

```csharp
28: [SerializeField] private float portalScale = 0.5f;    // 포털 크기
29: [SerializeField] private float altarScale = 0.5f;     // 재단 크기
32: [SerializeField] private HubBiomePortal[] portals = new HubBiomePortal[4];
118: Vector3 pos = player.transform.position;
119: pos.y = -2f;
134: float wallThickness = 0.5f;  // 벽 두께 줄임
136: float wallHeight = 3f;  // 벽 높이
140: new Vector3(mapWidth * tileSize / 2f, wallHeight / 2f, mapHeight * tileSize - wallThickness * tileSize / 2f),
141: new Vector3(mapWidth * tileSize, wallHeight, wallThickness * tileSize));
145: new Vector3(mapWidth * tileSize / 2f, wallHeight / 2f, wallThickness * tileSize / 2f),
146: new Vector3(mapWidth * tileSize, wallHeight, wallThickness * tileSize));
150: new Vector3(mapWidth * tileSize - wallThickness * tileSize / 2f, wallHeight / 2f, mapHeight * tileSize / 2f),
151: new Vector3(wallThickness * tileSize, wallHeight, mapHeight * tileSize));
155: new Vector3(wallThickness * tileSize / 2f, wallHeight / 2f, mapHeight * tileSize / 2f),
156: new Vector3(wallThickness * tileSize, wallHeight, mapHeight * tileSize));
164: private void CreateWall(Transform parent, string name, Vector3 position, Vector3 size)
182: for (int i = 0; i < 4; i++)
195: Vector3[] positions = mapRenderer.GetPortalPositions();
200: for (int i = 0; i < 4; i++)
213: portalObj.transform.localScale = Vector3.one * portalScale;
225: col.size = new Vector3(2f, 2f, 1f);
239: Vector3 altarPos = mapRenderer.GetAltarPosition();
251: altarObj.transform.localScale = Vector3.one * altarScale;
259: sr.sortingOrder = 2;
267: col.size = new Vector3(3f, 3f, 1f);
276: public Vector3 GetPlayerSpawnPosition()
280: return Vector3.zero;
286: public bool IsWalkable(Vector3 worldPos)
299: BiomeType.Intestine => portals[0],
300: BiomeType.Liver => portals[1],
301: BiomeType.Stomach => portals[2],
302: BiomeType.Lung => portals[3],
320: portals = new HubBiomePortal[4];
340: while (objectsParent.childCount > 0)
342: DestroyImmediate(objectsParent.GetChild(0).gameObject);
345: portals = new HubBiomePortal[4];
```

## Assets/_Project/Scripts/World/Hub/HubTileGenerator.cs

```csharp
11: [SerializeField] private int mapWidth = 30;
12: [SerializeField] private int mapHeight = 30;
13: [SerializeField] private float tileSize = 1f;
48: for (int x = 0; x < mapWidth; x++)
50: for (int y = 0; y < mapHeight; y++)
53: if (x < 2 || x >= mapWidth - 2 || y < 2 || y >= mapHeight - 2)
73: int centerX = mapWidth / 2;
74: int centerY = mapHeight / 2;
75: int portalOffset = 10;
78: Vector2Int[] portalPositions = new Vector2Int[]
80: new Vector2Int(centerX, centerY + portalOffset),      // 북
81: new Vector2Int(centerX + portalOffset, centerY),      // 동
82: new Vector2Int(centerX, centerY - portalOffset),      // 남
83: new Vector2Int(centerX - portalOffset, centerY)       // 서
88: for (int dx = -1; dx <= 1; dx++)
90: for (int dy = -1; dy <= 1; dy++)
120: for (int x = 0; x < mapWidth; x++)
122: for (int y = 0; y < mapHeight; y++)
143: Vector3 worldPos = GridToWorld(x, y);
168: tile.transform.localScale = new Vector3(tileSize, 0.1f, tileSize);
177: TileType.Floor => new Color(0.4f, 0.3f, 0.2f),  // 갈색
178: TileType.Wall => new Color(0.3f, 0.3f, 0.3f),   // 회색
195: while (tilesParent.childCount > 0)
197: Transform child = tilesParent.GetChild(0);
210: public Vector3 GridToWorld(int gridX, int gridY)
212: float worldX = gridX * tileSize + tileSize / 2f;
213: float worldZ = gridY * tileSize + tileSize / 2f;
214: return new Vector3(worldX, 0, worldZ);
220: public Vector2Int WorldToGrid(Vector3 worldPos)
224: return new Vector2Int(gridX, gridY);
232: return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
257: public Vector3 GetPlayerSpawnPosition()
259: int centerX = mapWidth / 2;
260: int centerY = mapHeight / 2;
261: return GridToWorld(centerX, centerY - 3);
267: public Vector3 GetAltarPosition()
269: return GridToWorld(mapWidth / 2, mapHeight / 2);
275: public Vector3[] GetPortalPositions()
277: int centerX = mapWidth / 2;
278: int centerY = mapHeight / 2;
279: int offset = 10;
281: return new Vector3[]
295: Vector3 center = new Vector3(mapWidth * tileSize / 2f, 0, mapHeight * tileSize / 2f);
296: Vector3 size = new Vector3(mapWidth * tileSize, 0.2f, mapHeight * tileSize);
303: Gizmos.DrawWireSphere(pos, 1.5f);
308: Gizmos.DrawWireSphere(GetAltarPosition(), 2f);
312: Gizmos.DrawWireSphere(GetPlayerSpawnPosition(), 1f);
```

## Assets/_Project/Scenes/FinalBoss.unity

```yaml
280: MonoBehaviour:
291:   biome: 2
292:   blockedArea:
293:     x: 9
294:     y: 33
295:     width: 6
296:     height: 3
297:   maxHealth: 65
298:   pillarRenderer: {fileID: 46271479}
299:   intactSprite: {fileID: 21300000, guid: 05eeb628201609541a49c9c3111b8f55, type: 3}
300:   brokenSprite: {fileID: 21300000, guid: 0de60c662b41fd04e9ddecb10cc0cb06, type: 3}
703: MonoBehaviour:
714:   pillars:
909: MonoBehaviour:
920:   bossVisual: {fileID: 1176931567}
921:   pillars:
926:   sourceBiomes:
1500: MonoBehaviour:
1511:   biome: 3
1512:   blockedArea:
1513:     x: 9
1514:     y: 47
1515:     width: 6
1516:     height: 3
1517:   maxHealth: 65
1518:   pillarRenderer: {fileID: 1866689291}
1519:   intactSprite: {fileID: 21300000, guid: cfbe7710a4a2023428e0476b3f46fb6d, type: 3}
1520:   brokenSprite: {fileID: 21300000, guid: fc3ec1f262352ba49b8a7acc69de12a9, type: 3}
1675: MonoBehaviour:
1686:   biome: 1
1687:   blockedArea:
1688:     x: 33
1689:     y: 47
1690:     width: 6
1691:     height: 3
1692:   maxHealth: 65
1693:   pillarRenderer: {fileID: 1886500541}
1694:   intactSprite: {fileID: 21300000, guid: 204a31a807c949d4186aebf6ff4a6ec4, type: 3}
1695:   brokenSprite: {fileID: 21300000, guid: f9bb22e1bd800b947ac06b9840100f42, type: 3}
1900: MonoBehaviour:
1911:   biome: 4
1912:   blockedArea:
1913:     x: 33
1914:     y: 33
1915:     width: 6
1916:     height: 3
1917:   maxHealth: 65
1918:   pillarRenderer: {fileID: 2040703910}
1919:   intactSprite: {fileID: 21300000, guid: b69e7a55ca97c7b489fc5c8e6c65c261, type: 3}
1920:   brokenSprite: {fileID: 21300000, guid: a907a4d41bada5d49a51c9fce3bd9bab, type: 3}
```

## Assets/_Project/Scenes/Hub.unity

```yaml
10516: MonoBehaviour:
10527:   spriteRenderer: {fileID: 571540666}
10528:   idleSprites:
10534:   walkDownSprites:
10541:   walkUpSprites:
10548:   walkLeftSprites:
10555:   walkRightSprites:
10562:   warriorIdleSprites:
10568:   warriorWalkDownSprites:
10575:   warriorWalkUpSprites:
10582:   warriorWalkLeftSprites:
10589:   warriorWalkRightSprites:
10596:   mageIdleSprites:
10605:   mageWalkDownSprites:
10612:   mageWalkUpSprites:
10619:   mageWalkLeftSprites:
10626:   mageWalkRightSprites:
10633:   archerIdleSprites:
10638:   archerWalkDownSprites:
10644:   archerWalkUpSprites:
10650:   archerWalkLeftSprites:
10656:   archerWalkRightSprites:
10662:   meleeDown:
10671:   meleeUp:
10680:   meleeLeft:
10689:   meleeRight:
10698:   meleeDownLeft:
10707:   meleeDownRight:
10716:   meleeUpLeft:
10725:   meleeUpRight:
10734:   rangedDown:
10742:   rangedUp:
10751:   rangedLeft:
10759:   rangedRight:
10768:   rangedDownLeft:
10776:   rangedDownRight:
10784:   rangedUpLeft:
10793:   rangedUpRight:
10801:   idleFrameRate: 4
10802:   walkFrameRate: 8
10803:   attackAnimDuration: 0.2
10804:   attackFrameRate: 12
10805:   deathSprites:
10808:   deathFrameRate: 8
10809:   lockYPosition: 0
10810:   lockedY: -2
10811:   groundOffsetY: -2
10812:   useDynamicGroundHeight: 1
10892: MonoBehaviour:
10903:   invincibilityDuration: 0.2
10904:   disableIncomingDamageForTest: 0
10906: MonoBehaviour:
10917:   meleeAttackDamage: 20
10918:   meleeAttackBoxSize: {x: 1, y: 1, z: 2}
10919:   meleeAttackOffset: 1
10920:   meleeTargetMask:
10921:     serializedVersion: 2
10923:   meleeOverlapBufferSize: 24
10924:   firePoint: {fileID: 1905923572}
10925:   projectileSpawnPoint: {fileID: 1905923572}
10926:   projectileSpawnOffset: 0.65
10927:   projectileSpawnHeight: 1
10928:   projectileSpawnExtraHeight: 2
10929:   projectileRange: 8
10930:   rangedAttackDamage: 10
10931:   rangedTargetMask:
10932:     serializedVersion: 2
10934:   beamOverlapBufferSize: 48
10935:   beamVerticalHalfHeight: 2.5
10936:   beamHeightOffset: -0.3
10937:   beamVisualDuration: 0.1
10938:   attackCooldown: 1
10939:   cellProliferationDelay: 0.5
10940:   enableDebugLogs: 0
10942: MonoBehaviour:
10953:   currentClass: 0
10954:   enemyMask:
10955:     serializedVersion: 2
10957:   overlapBufferSize: 32
10958:   skillSpawnPoint: {fileID: 0}
10959:   projectileSpawnOffset: 0.65
10960:   projectileSpawnHeight: 1
10961:   skillVerticalOffset: 2
10962:   skillHitHeightOffset: 0.75
10963:   skillHitVerticalHalfHeight: 4
10964:   maxAreaSkillHitTargets: 32
10965:   skillEffectSortingOrder: 5200
10966:   enableDebugLogs: 0
10967:   mageSkill1:
10968:     cooldown: 7
10969:     radius: 3.75
10970:     forwardOffset: 0
10971:     baseDamage: 0
10972:     additionalDamage: 0
10973:     additionalDamageMin: 4
10974:     additionalDamageMax: 6
10975:     stunDuration: 1
10976:     areaEffectPrefab: {fileID: 3983581763031629201, guid: f73f946bcf806594fb157aac0fee0209, type: 3}
10977:     areaEffectLifetime: 1
10978:     fallbackEffectScale: 10
10979:     effectReferenceRadius: 5.28
10980:   mageSkill2:
10981:     cooldown: 12
10982:     radius: 3.5
10983:     forwardOffset: 2.5
10984:     baseDamage: 10
10985:     detonationDelay: 0.7
10986:     damageTakenIncreaseRatio: 0.1
10987:     damageTakenIncreaseDuration: 3
10988:     markEffectPrefab: {fileID: 6458501098902630320, guid: cf3d327d8753b394bb815654ed1f9694, type: 3}
10989:     markEffectLifetime: 0.8
10990:     markFallbackEffectScale: 0.8
10991:     markHeadOffset: 0.2
10992:     scaleEffectByTargetSize: 1
10993:     effectReferenceTargetHeight: 1.1
10994:     effectSizeMultiplier: 1
10995:     effectMinScaleMultiplier: 0.7
10996:     effectMaxScaleMultiplier: 2.5
10997:     explosionEffectPrefab: {fileID: 827007673572255353, guid: 6b33e3138bc5f0d4680d9129b6702575, type: 3}
10998:     explosionEffectLifetime: 1
10999:     fallbackEffectScale: 3
11000:   archerSkill1:
11001:     cooldown: 6
11002:     projectileCount: 5
11003:     fanAngle: 55
11004:     projectileDamage: 1
11005:     projectileSpeed: 16
11006:     projectileRange: 6
11007:     projectileLifeTime: 2
11008:     projectilePrefab: {fileID: 4631496915048584632, guid: c1cbf5f8078a0444d8f033977407ff70, type: 3}
11009:     projectileScale: 0.5
11010:     poisonDuration: 3
11011:     poisonTickInterval: 1
11012:     poisonTickDamage: 1
11013:     shootEffectPrefab: {fileID: 0}
11014:     shootEffectLifetime: 0.4
11015:   archerSkill2:
11016:     cooldown: 10
11017:     aimDuration: 0.5
11018:     range: 10
11019:     projectileHitRadius: 0.55
11020:     projectileVisualLength: 4
11021:     projectileVisualThickness: 0.8
11022:     targetForwardOffset: 6
11023:     projectileDamage: 4
11024:     projectileTravelSpeed: 24
11025:     projectileLifeTime: 0.65
11026:     projectilePrefab: {fileID: 5377739703324984544, guid: 21c6d35c963bab846bfde230ca8d29fa, type: 3}
11027:     projectileScale: 0.45
11028:     prefabDirectionAxis: 1
11029:     prefabRotationOffsetEuler: {x: 0, y: 0, z: 0}
11030:     autoRollToCamera: 1
11031:     prefabSurfaceNormalAxis: 2
11032:     virusExplosionDamage: 5
11033:     virusExplosionRadius: 1.8
11034:     virusExplosionEffectPrefab: {fileID: 545757833818925850, guid: e85d0693183cd4c4ca091330d68d1599, type: 3}
11035:     virusExplosionEffectLifetime: 1
11036:     enableAfterImage: 1
11037:     afterImageInterval: 0.03
11038:     afterImageFadeDuration: 0.15
11039:     afterImageStartAlpha: 0.4
11040:     afterImageMaxVisibleCount: 3
11041:   warriorSkill1:
11042:     cooldown: 4
11043:     range: 4.125
11044:     forwardAngle: 140
11045:     maxTargets: 8
11046:     damage: 4
11047:     bleedDuration: 2
11048:     bleedTickInterval: 1
11049:     bleedTickDamage: 1
11050:     hitEffectPrefab: {fileID: 840110001, guid: 67f284e219c3405b98cdb7df65e8bf81, type: 3}
11051:     hitEffectLifetime: 0.35
11052:     fallbackEffectScale: 0.8
11053:   warriorSkill2:
11054:     cooldown: 8
11055:     searchRange: 6
11056:     dashSpeed: 18
11057:     damage: 8
11058:     rootDuration: 2
11059:     searchAngle: 90
11060:     hitEffectPrefab: {fileID: 840120001, guid: e1b7d39d0ac54e2098105cb7a35a0da4, type: 3}
11061:     hitEffectLifetime: 0.5
11062:     fallbackEffectScale: 1
11063:   autoTargetForwardEnemyForArcherSkill2: 1
11064:   archerSkill2AutoTargetAngle: 90
11065:   useTestCooldownOverride: 0
11066:   testSkill1Cooldown: 1
11067:   testSkill2Cooldown: 3
11172: MonoBehaviour:
11183:   maxItemSlots: 3
11184:   allowDuplicateItems: 0
11185:   itemEntries:
11186:   - itemId: double_core
11187:     displayName: "\uC774\uC911\uD575"
11188:     description: "\uBC1C\uC0AC\uCCB4 2\uAC1C \uBC1C\uC0AC"
11189:     category: 0
11190:     icon: {fileID: 21300000, guid: 0fdf7b5b219f4ad1982fc647c05a9b8a, type: 3}
11191:     implementation: {fileID: 0}
11192:   - itemId: triple_core
11193:     displayName: "\uC0BC\uC911\uD575"
11194:     description: "\uBC1C\uC0AC\uCCB4 3\uAC1C \uBC1C\uC0AC"
11195:     category: 0
11196:     icon: {fileID: 21300000, guid: 5dd1a4e6e42049c2a47562b7c4f40f2a, type: 3}
11197:     implementation: {fileID: 0}
11198:   - itemId: homing_cell
11199:     displayName: "\uC720\uB3C4 \uC138\uD3EC"
11200:     description: "\uC801\uC744 \uCD94\uC801\uD558\uB294 \uC720\uB3C4 \uD22C\uC0AC\uCCB4"
11201:     category: 0
11202:     icon: {fileID: 21300000, guid: 81d1d7f20f304bb0be0a9cf202b92d1a, type: 3}
11203:     implementation: {fileID: 0}
11204:   - itemId: hypertrophy_cell
11205:     displayName: "\uBE44\uB300 \uC138\uD3EC"
11206:     description: "\uD22C\uC0AC\uCCB4 \uD06C\uAE30 \uC99D\uAC00 + \uACF5\uACA9 \uBC94\uC704
11207:       \uC99D\uAC00"
11208:     category: 0
11209:     icon: {fileID: 21300000, guid: c4f57f7fd6eb454f8d5d6f18f5bc5b2d, type: 3}
11210:     implementation: {fileID: 0}
11211:   - itemId: reflux_organ
11212:     displayName: "\uC5ED\uB958 \uC7A5\uAE30"
11213:     description: "\uD22C\uC0AC\uCCB4\uAC00 \uB418\uB3CC\uC544\uC624\uB294 \uBD80\uBA54\uB791
11214:       \uD615\uD0DC"
11215:     category: 0
11216:     icon: {fileID: 21300000, guid: 9a7e0131c7664cdb94ca5f2cf389a101, type: 3}
11217:     implementation: {fileID: 0}
11218:   - itemId: piercing_mucus
11219:     displayName: "\uAD00\uD1B5 \uC810\uC561"
11220:     description: "\uC801 \uC5EC\uB7EC \uBA85\uC744 \uAD00\uD1B5\uD558\uB294 \uACF5\uACA9"
11221:     category: 0
11222:     icon: {fileID: 21300000, guid: b6629a0e59624c9a8df06efecc705d51, type: 3}
11223:     implementation: {fileID: 0}
11224:   - itemId: laryngeal_nerve
11225:     displayName: "\uD6C4\uB450 \uC2E0\uACBD"
11226:     description: "\uB4A4\uCABD\uC73C\uB85C \uCD94\uAC00 \uD22C\uC0AC\uCCB4 \uBC1C\uC0AC"
11227:     category: 0
11228:     icon: {fileID: 21300000, guid: 7c24bb9d53fa45fda66af0e0bde10276, type: 3}
11229:     implementation: {fileID: 0}
11230:   - itemId: beam_organ
11231:     displayName: "\uAD11\uC120 \uAE30\uAD00"
11232:     description: "\uC77C\uBC18 \uD22C\uC0AC\uCCB4 \uB300\uC2E0 \uB808\uC774\uC800
11233:       \uACF5\uACA9"
11234:     category: 0
11235:     icon: {fileID: 21300000, guid: 0a725d4bc1ca43be955108ef3f6b412c, type: 3}
11236:     implementation: {fileID: 0}
11237:   - itemId: split_tissue
11238:     displayName: "\uBD84\uC5F4 \uC870\uC9C1"
11239:     description: "\uC801 \uBA85\uC911 \uC2DC \uC591\uC606\uC73C\uB85C \uCD94\uAC00
11240:       \uD22C\uC0AC\uCCB4 \uC0DD\uC131"
11241:     category: 0
11242:     icon: {fileID: 21300000, guid: f22457e2296c4e5bb8a989d08d3fe3a4, type: 3}
11243:     implementation: {fileID: 0}
11244:   - itemId: explosive_blood_cell
11245:     displayName: "\uD3ED\uBC1C \uD608\uAD6C"
11246:     description: "\uD22C\uC0AC\uCCB4\uAC00 \uBC94\uC704 \uD3ED\uBC1C \uACF5\uACA9\uC73C\uB85C
11247:       \uBCC0\uACBD"
11248:     category: 0
11249:     icon: {fileID: 21300000, guid: 2c49df8a280c4f168256c5b1f1b28011, type: 3}
11250:     implementation: {fileID: 0}
11251:   - itemId: acidic_rupture
11252:     displayName: "\uC0B0\uC131 \uD30C\uC5F4"
11253:     description: "\uACF5\uACA9 \uC801\uC911 \uC2DC \uBC14\uB2E5\uC5D0 \uC9C0\uC18D
11254:       \uD53C\uD574 \uC7A5\uD310 \uC0DD\uC131"
11255:     category: 0
11256:     icon: {fileID: 21300000, guid: 4bb260e9caea4c50a97fc2e6f9af41de, type: 3}
11257:     implementation: {fileID: 0}
11258:   - itemId: cell_proliferation
11259:     displayName: "\uC138\uD3EC \uC99D\uC2DD"
11260:     description: "\uC77C\uC815 \uD655\uB960\uB85C \uACF5\uACA9\uC774 \uD55C \uBC88
11261:       \uB354 \uBC1C\uB3D9"
11262:     category: 0
11263:     icon: {fileID: 21300000, guid: 93a5bb1467774edb80e4cc9f0f9586f7, type: 3}
11264:     implementation: {fileID: 0}
11265:   - itemId: pulse_bullet
11266:     displayName: "\uB9E5\uB3D9 \uD0C4\uD658"
11267:     description: "\uD22C\uC0AC\uCCB4\uAC00 \uBA40\uB9AC\uAC08\uC218\uB85D \uD06C\uAE30\uAC00
11268:       \uC99D\uAC00\uD558\uAC70\uB098 \uC791\uC544\uC9D0"
11269:     category: 0
11270:     icon: {fileID: 21300000, guid: 65c749a76f734243b63c76d5d1d43c62, type: 3}
11271:     implementation: {fileID: 0}
11272:   - itemId: vascular_reflection
11273:     displayName: "\uD608\uAD00 \uBC18\uC0AC"
11274:     description: "\uBCBD\uC5D0 \uD295\uAE30\uB294 \uBC18\uC0AC \uD22C\uC0AC\uCCB4"
11275:     category: 0
11276:     icon: {fileID: 21300000, guid: e8a3d94111ab4dd2b70352572c158a16, type: 3}
11277:     implementation: {fileID: 0}
11278:   - itemId: toxic_mucosa
11279:     displayName: "\uB3C5\uC131 \uC810\uB9C9"
11280:     description: "\uACF5\uACA9 \uC801\uC911 \uC2DC \uC911\uB3C5 \uD53C\uD574 \uBD80\uC5EC"
11281:     category: 0
11282:     icon: {fileID: 21300000, guid: 9986fc82441c47119bb568655973acd9, type: 3}
11283:     implementation: {fileID: 0}
11284:   - itemId: freezing_nerve
11285:     displayName: "\uBE59\uACB0 \uC2E0\uACBD"
11286:     description: "\uACF5\uACA9 \uC801\uC911 \uC2DC \uC801 \uC774\uB3D9\uC18D\uB3C4
11287:       \uAC10\uC18C"
11288:     category: 0
11289:     icon: {fileID: 21300000, guid: 2dae7275370f4782bbefcadb7758e704, type: 3}
11290:     implementation: {fileID: 0}
11291:   - itemId: hemorrhage_organ
11292:     displayName: "\uCD9C\uD608 \uAE30\uAD00"
11293:     description: "\uC801\uC911 \uC2DC \uC9C0\uC18D \uCD9C\uD608 \uD53C\uD574"
11294:     category: 0
11295:     icon: {fileID: 21300000, guid: 48f6cc999e9a45039fc8eceffc54f26a, type: 3}
11296:     implementation: {fileID: 0}
11297:   - itemId: overheated_organ
11298:     displayName: "\uACFC\uC5F4 \uAE30\uAD00"
11299:     description: "\uACC4\uC18D \uACF5\uACA9 \uC2DC \uACF5\uACA9\uC18D\uB3C4 \uC99D\uAC00,
11300:       \uACFC\uC5F4 \uC2DC \uD3ED\uBC1C \uD53C\uD574"
11301:     category: 0
11302:     icon: {fileID: 21300000, guid: a2eef1bb211a4210ab5619137abc7952, type: 3}
11303:     implementation: {fileID: 0}
11304:   - itemId: mutant_eye
11305:     displayName: "\uB3CC\uC5F0\uBCC0\uC774 \uC548\uAD6C"
11306:     description: "\uC804\uBC29 180\uB3C4 \uBC94\uC704\uB85C \uBB34\uC791\uC704 \uBC1C\uC0AC,
11307:       \uACF5\uACA9\uB825 +5"
11308:     category: 0
11309:     icon: {fileID: 21300000, guid: 02864ace75954328bb1c8ad1c7e6d6c7, type: 3}
11310:     implementation: {fileID: 0}
11311:   - itemId: organ_tentacle
11312:     displayName: "\uC7A5\uAE30 \uCD09\uC218"
11313:     description: "\uC8FC\uBCC0 \uC801 \uC790\uB3D9 \uACF5\uACA9, \uB300\uC2E0 \uAE30\uBCF8
11314:       \uACF5\uACA9 \uC57D\uD654"
11315:     category: 0
11316:     icon: {fileID: 21300000, guid: 30ea581a77ed4f50b423105a66d8f601, type: 3}
11317:     implementation: {fileID: 0}
11318:   - itemId: rampage_bloodflow
11319:     displayName: "\uD3ED\uC8FC \uD608\uB958"
11320:     description: "\uC774\uB3D9 \uC911 \uACF5\uACA9\uB825 \uC99D\uAC00, \uBA48\uCD94\uBA74
11321:       \uAC10\uC18C"
11322:     category: 0
11323:     icon: {fileID: 21300000, guid: ac6f186a235348a0bfbfdfde74c06175, type: 3}
11324:     implementation: {fileID: 0}
11325:   - itemId: muscle_spasm
11326:     displayName: "\uADFC\uC721 \uACBD\uB828"
11327:     description: "\uADFC\uC811 \uACF5\uACA9 \uBC94\uC704 \uC99D\uAC00, \uACF5\uACA9\uC18D\uB3C4
11328:       \uAC10\uC18C"
11329:     category: 0
11330:     icon: {fileID: 21300000, guid: da1db62dc85d4ff8b3718e999489bbae, type: 3}
11331:     implementation: {fileID: 0}
11332:   - itemId: unstable_core
11333:     displayName: "\uBD88\uC548\uC815 \uD575"
11334:     description: "\uACF5\uACA9\uB825\uC774 \uB79C\uB364\uD558\uAC8C \uD06C\uAC8C
11335:       \uBCC0\uB3D9\uB428"
11336:     category: 0
11337:     icon: {fileID: 21300000, guid: 0238764490da4eb7b6b4775811a9ca1f, type: 3}
11338:     implementation: {fileID: 0}
11339:   - itemId: bio_resonance
11340:     displayName: "\uC0DD\uCCB4 \uACF5\uBA85"
11341:     description: "\uAC19\uC740 \uC801 \uC5F0\uC18D \uACF5\uACA9 \uC2DC \uD574\uB2F9
11342:       \uC801\uC5D0\uAC8C \uD53C\uD574\uB7C9 \uC99D\uAC00"
11343:     category: 0
11344:     icon: {fileID: 21300000, guid: 08005822b1914de0bee328b21739e6af, type: 3}
11345:     implementation: {fileID: 0}
11346:   - itemId: blood_pressure_burst
11347:     displayName: "\uD608\uC555 \uD3ED\uBC1C"
11348:     description: "\uCCB4\uB825\uC774 \uB0AE\uC744\uC218\uB85D \uACF5\uACA9\uC18D\uB3C4
11349:       \uC99D\uAC00"
11350:     category: 0
11351:     icon: {fileID: 21300000, guid: bf1c77e0f1a348268cc2ef88efd45fe6, type: 3}
11352:     implementation: {fileID: 0}
11353:   - itemId: void_cell
11354:     displayName: "\uACF5\uD5C8 \uC138\uD3EC"
11355:     description: "\uACF5\uACA9 \uC2DC \uC77C\uC815 \uD655\uB960\uB85C \uC21C\uAC04\uC774\uB3D9
11356:       \uD0C4\uD658 \uC0DD\uC131"
11357:     category: 0
11358:     icon: {fileID: 21300000, guid: 44b4c21f87dc4d1980ec7aa03b0977b4, type: 3}
11359:     implementation: {fileID: 0}
11360:   - itemId: forbidden_growth
11361:     displayName: "\uAE08\uB2E8 \uC131\uC7A5"
11362:     description: "\uCCB4\uB825 -4, \uACF5\uACA9\uB825 +6 \uC99D\uAC00"
11363:     category: 0
11364:     icon: {fileID: 21300000, guid: a0bf5a6d28c64823b020d59972591ee2, type: 3}
11365:     implementation: {fileID: 0}
11366:   - itemId: overclock_nerve
11367:     displayName: "\uACFC\uC18D \uC2E0\uACBD"
11368:     description: "\uCCB4\uB825 -2, \uC774\uB3D9\uC18D\uB3C4 +4 \uC99D\uAC00"
11369:     category: 0
11370:     icon: {fileID: 21300000, guid: d4b0d7a6e8b94e12a5b9c77fa6d42430, type: 3}
11371:     implementation: {fileID: 0}
11372:   - itemId: blood_contract
11373:     displayName: "\uD53C\uC758 \uACC4\uC57D"
11374:     description: "\uC801 10\uBA85 \uCC98\uCE58 \uC2DC \uCCB4\uB825 +1 \uC99D\uAC00,
11375:       \uB300\uC2E0 \uBC1B\uB294 \uD53C\uD574 1.5\uBC30"
11376:     category: 0
11377:     icon: {fileID: 21300000, guid: 7e0fe0ed2b824d7d9fdc4f25b49b77bd, type: 3}
11378:     implementation: {fileID: 0}
11379:   - itemId: hyperplasia_heart
11380:     displayName: "\uACFC\uC99D\uC2DD \uC2EC\uC7A5"
11381:     description: "\uCCB4\uB825\uC774 \uB0AE\uC744\uC218\uB85D \uACF5\uACA9\uB825
11382:       \uC99D\uAC00"
11383:     category: 0
11384:     icon: {fileID: 21300000, guid: e9365419c64b46c9aa094fe60ef9d0ce, type: 3}
11385:     implementation: {fileID: 0}
11386:   - itemId: decay_organ
11387:     displayName: "\uBD80\uD328 \uC7A5\uAE30"
11388:     description: "\uC2DC\uAC04\uC774 \uC9C0\uB0A0\uC218\uB85D \uACF5\uACA9\uB825
11389:       \uC99D\uAC00"
11390:     category: 0
11391:     icon: {fileID: 21300000, guid: 9b3e06a4a4c74590918318573745d224, type: 3}
11392:     implementation: {fileID: 0}
11393:   - itemId: rupture_muscle
11394:     displayName: "\uD30C\uC5F4 \uADFC\uC721"
11395:     description: "\uACF5\uACA9\uD560\uC218\uB85D \uAC15\uD574\uC9C0\uC9C0\uB9CC \uC774\uB3D9\uC18D\uB3C4
11396:       \uAC10\uC18C"
11397:     category: 0
11398:     icon: {fileID: 21300000, guid: 64519f00218d4dc5945148c55ee65576, type: 3}
11399:     implementation: {fileID: 0}
11400:   - itemId: imperfect_regeneration
11401:     displayName: "\uBD88\uC644\uC804 \uC7AC\uC0DD"
11402:     description: "\uCCB4\uB825 -4, \uD53C\uD574\uB97C \uBC1B\uC73C\uBA74 3\uCD08
11403:       \uD6C4 \uCCB4\uB825 +1 \uD68C\uBCF5 (15\uCD08 \uCFE8\uD0C0\uC784)"
11404:     category: 0
11405:     icon: {fileID: 21300000, guid: b8585326984a471c81e363552ede00cc, type: 3}
11406:     implementation: {fileID: 0}
11407:   - itemId: severance_reflex
11408:     displayName: "\uC808\uB2E8 \uBC18\uC0AC"
11409:     description: "\uD53C\uACA9 \uC9C1\uD6C4 \uACF5\uACA9\uB825 +6"
11410:     category: 0
11411:     icon: {fileID: 21300000, guid: edba15146bde460caf3d26ebb02fdcb4, type: 3}
11412:     implementation: {fileID: 0}
11413:   - itemId: bio_gamble
11414:     displayName: "\uC0DD\uCCB4 \uB3C4\uBC15"
11415:     description: "\uB808\uBCA8\uC5C5 \uC120\uD0DD \uC2DC \uC2A4\uD0EF\uC774 -2~+3
11416:       \uBC94\uC704\uC5D0\uC11C \uB79C\uB364 \uC99D\uAC10"
11417:     category: 0
11418:     icon: {fileID: 21300000, guid: 3975e26e5a454500bb016c167ad152e7, type: 3}
11419:     implementation: {fileID: 0}
11420:   - itemId: exoskeleton
11421:     displayName: "\uC678\uACE8\uACA9"
11422:     description: "\uBC1B\uB294 \uD53C\uD574 30% \uAC10\uC18C, \uB300\uC2E0 \uC774\uB3D9\uC18D\uB3C4
11423:       -2"
11424:     category: 0
11425:     icon: {fileID: 21300000, guid: 1c06e224706e4e41ae9ffc85e79707bc, type: 3}
11426:     implementation: {fileID: 0}
11427:   - itemId: platelet_membrane
11428:     displayName: "\uD608\uC18C\uD310 \uB9C9"
11429:     description: "30\uCD08\uB9C8\uB2E4 \uBCF4\uD638\uB9C9 1 \uC0DD\uC131"
11430:     category: 0
11431:     icon: {fileID: 21300000, guid: ac5d40ecd9bc4f81aeb5db68f9ff801b, type: 3}
11432:     implementation: {fileID: 0}
11433:   - itemId: recovery_factor
11434:     displayName: "\uD68C\uBCF5 \uC778\uC790"
11435:     description: "45\uCD08\uB2F9 \uCCB4\uB825 +1 \uD68C\uBCF5"
11436:     category: 0
11437:     icon: {fileID: 21300000, guid: ea4f76c79d6e4826af2526d2eb2ab3fa, type: 3}
11438:     implementation: {fileID: 0}
11439:   - itemId: reflective_skin
11440:     displayName: "\uBC18\uC0AC \uD53C\uBD80"
11441:     description: "\uD53C\uACA9 \uC2DC \uC801\uC5D0\uAC8C \uD53C\uD574 \uBC18\uC0AC"
11442:     category: 0
11443:     icon: {fileID: 21300000, guid: 515cafcd131d46d5b031bd4d409ee9b0, type: 3}
11444:     implementation: {fileID: 0}
11445:   - itemId: bio_barrier
11446:     displayName: "\uC0DD\uCCB4 \uC7A5\uB9C9"
11447:     description: "\uC774\uB3D9\uD558\uC9C0 \uC54A\uC73C\uBA74 \uD53C\uD574 \uAC10\uC18C
11448:       \uC99D\uAC00 (\uCD5C\uB300 50%)"
11449:     category: 0
11450:     icon: {fileID: 21300000, guid: bd715116c6d946a18bd57a4363532296, type: 3}
11451:     implementation: {fileID: 0}
11452:   - itemId: split_regeneration
11453:     displayName: "\uBD84\uC5F4 \uC7AC\uC0DD"
11454:     description: "\uCCB4\uB825 0\uC774 \uB420 \uB54C \uC791\uC740 \uCCB4\uB825\uC73C\uB85C
11455:       \uBD80\uD65C"
11456:     category: 0
11457:     icon: {fileID: 21300000, guid: b7e6394cf9d64f5aacfe52a31e639e7c, type: 3}
11458:     implementation: {fileID: 0}
11459:   - itemId: infected_host
11460:     displayName: "\uAC10\uC5FC \uC219\uC8FC"
11461:     description: "\uC7A1\uBAB9 \uCC98\uCE58 \uC2DC \uC77C\uC815 \uD655\uB960\uB85C
11462:       \uC9E7\uC740 \uC2DC\uAC04 \uC544\uAD70 \uC0DD\uCCB4 \uC0DD\uC131"
11463:     category: 0
11464:     icon: {fileID: 21300000, guid: 2b9e25cfeeef435eac4f4788c2beae32, type: 3}
11465:     implementation: {fileID: 0}
11466:   - itemId: spore_colony
11467:     displayName: "\uD3EC\uC790 \uAD70\uC9D1"
11468:     description: "\uC77C\uC815 \uC2DC\uAC04\uB9C8\uB2E4 \uC801\uC5D0\uAC8C \uB2EC\uB824\uB4DC\uB294
11469:       \uC791\uC740 \uD3EC\uC790 \uC0DD\uBA85\uCCB4 \uC18C\uD658"
11470:     category: 0
11471:     icon: {fileID: 21300000, guid: 1fd0bb7116fe498ba24b0558dc0b04c4, type: 3}
11472:     implementation: {fileID: 0}
11473:   - itemId: blood_drone
11474:     displayName: "\uD608\uC561 \uB4DC\uB860"
11475:     description: "\uD50C\uB808\uC774\uC5B4 \uC8FC\uBCC0\uC744 \uB5A0\uB2E4\uB2C8\uBA70
11476:       \uAC00\uAE4C\uC6B4 \uC801\uC744 \uC790\uB3D9 \uCD94\uC801 \uACF5\uACA9"
11477:     category: 0
11478:     icon: {fileID: 21300000, guid: 1f4bbe17d34a41efbfeacca751dc798a, type: 3}
11479:     implementation: {fileID: 0}
11480:   - itemId: guardian_organ
11481:     displayName: "\uC218\uD638 \uC7A5\uAE30"
11482:     description: "\uD50C\uB808\uC774\uC5B4 \uC8FC\uBCC0\uC744 \uB9F4\uB3CC\uBA70
11483:       \uC801 \uD22C\uC0AC\uCCB4\uB97C \uBC29\uC5B4"
11484:     category: 0
11485:     icon: {fileID: 21300000, guid: 1b6965b707da40f08ab8f6391cf9312f, type: 3}
11486:     implementation: {fileID: 0}
11487:   - itemId: tentacle_colony
11488:     displayName: "\uCD09\uC218 \uAD70\uCCB4"
11489:     description: "\uC8FC\uBCC0 \uC801\uC744 \uC8FC\uAE30\uC801\uC73C\uB85C \uC18D\uBC15\uD574
11490:       \uC774\uB3D9\uC744 \uD06C\uAC8C \uBC29\uD574"
11491:     category: 0
11492:     icon: {fileID: 21300000, guid: 40925b61970b44f4bb0771f2324091c4, type: 3}
11493:     implementation: {fileID: 0}
11494:   - itemId: electric_neural_network
11495:     displayName: "\uC804\uAE30 \uC2E0\uACBD\uB9DD"
11496:     description: "\uC801 \uCC98\uCE58 \uC2DC \uC0AC\uB9DD \uC9C0\uC810\uC5D0\uC11C
11497:       \uC804\uB958\uAC00 \uD280\uC5B4 \uC8FC\uBCC0 \uC801\uC5D0\uAC8C \uC5F0\uC1C4
11498:       \uD53C\uD574"
11499:     category: 0
11500:     icon: {fileID: 21300000, guid: 79ccd4e44605424b904bacee6d233d5d, type: 3}
11501:     implementation: {fileID: 0}
11502:   - itemId: infection_transference
11503:     displayName: "\uAC10\uC5FC \uC804\uC774"
11504:     description: "\uC801 \uC0AC\uB9DD \uC2DC \uC8FC\uBCC0 \uC801 3\uBA85\uC5D0\uAC8C
11505:       3\uCD08 \uAC10\uC5FC \uD53C\uD574 \uBD80\uC5EC"
11506:     category: 0
11507:     icon: {fileID: 21300000, guid: 44c180ca917c49a9a6025e11944cb3a6, type: 3}
11508:     implementation: {fileID: 0}
11509:   - itemId: macrophage
11510:     displayName: "\uB300\uC2DD \uC138\uD3EC"
11511:     description: "\uC801 \uCC98\uCE58 \uC2DC 5\uCD08 \uB3D9\uC548 \uACF5\uACA9\uB825
11512:       \uC99D\uAC00 (\uCD5C\uB300 3\uC2A4\uD0DD)"
11513:     category: 0
11514:     icon: {fileID: 21300000, guid: 9b0c2139036f4cd4a9bc0b31cfc7c1dc, type: 3}
11515:     implementation: {fileID: 0}
11516:   - itemId: gluttonous_organ
11517:     displayName: "\uD3ED\uC2DD \uC7A5\uAE30"
11518:     description: "\uC801 \uCC98\uCE58 \uC2DC 3\uCD08 \uB3D9\uC548 \uC774\uB3D9\uC18D\uB3C4
11519:       \uC99D\uAC00 (\uCD5C\uB300 4\uC2A4\uD0DD)"
11520:     category: 0
11521:     icon: {fileID: 21300000, guid: 235474cec8ea4c9b9b40cd93bfffbbc8, type: 3}
11522:     implementation: {fileID: 0}
11523:   - itemId: heart_sniper
11524:     displayName: "\uC2EC\uC7A5 \uC800\uACA9"
11525:     description: "\uCCB4\uB825 \uB192\uC740 \uC801\uC5D0\uAC8C \uC9C1\uC811 \uACF5\uACA9
11526:       \uD53C\uD574 \uC99D\uAC00"
11527:     category: 0
11528:     icon: {fileID: 21300000, guid: ca1bb1b767904f3db5d0b69dcbf1fc7b, type: 3}
11529:     implementation: {fileID: 0}
11530:   - itemId: bloodflow_acceleration
11531:     displayName: "\uD608\uB958 \uAC00\uC18D"
11532:     description: "\uBCF4\uC2A4/\uC5D8\uB9AC\uD2B8 \uADFC\uCC98\uC5D0\uC11C \uACF5\uACA9\uC18D\uB3C4
11533:       \uC99D\uAC00"
11534:     category: 0
11535:     icon: {fileID: 21300000, guid: 304747b797864b1fa1386ba975331c2d, type: 3}
11536:     implementation: {fileID: 0}
11537:   - itemId: focused_nerve
11538:     displayName: "\uC9D1\uC911 \uC2E0\uACBD"
11539:     description: "\uC8FC\uBCC0 \uC801\uC774 \uC801\uC744\uC218\uB85D \uACF5\uACA9\uB825
11540:       \uC99D\uAC00"
11541:     category: 0
11542:     icon: {fileID: 21300000, guid: 298d285e577e42a895bc20ba5738ceae, type: 3}
11543:     implementation: {fileID: 0}
11544:   - itemId: execution_instinct
11545:     displayName: "\uCC98\uD615 \uBCF8\uB2A5"
11546:     description: "\uD53C\uD574 \uD6C4 \uCCB4\uB825 \uB0AE\uC740 \uC801\uC744 \uD655\uB960
11547:       \uCC98\uD615"
11548:     category: 0
11549:     icon: {fileID: 21300000, guid: 486ccc7133284a47a64f0ad5e95cfa36, type: 3}
11550:     implementation: {fileID: 0}
11551:   - itemId: berserk_cell
11552:     displayName: "\uAD11\uD3ED \uC138\uD3EC"
11553:     description: "\uBCF4\uC2A4\uC804 \uC9C4\uC785 \uC2DC \uC7A0\uC2DC \uC2A4\uD0EF
11554:       \uC0C1\uC2B9"
11555:     category: 0
11556:     icon: {fileID: 21300000, guid: bb50b60bc68046478ebcc0893f470801, type: 3}
11557:     implementation: {fileID: 0}
11558:   - itemId: unstable_cell
11559:     displayName: "\uBD88\uC548\uC815 \uC138\uD3EC"
11560:     description: "\uD22C\uC0AC\uCCB4 \uC18D\uB3C4 \uB79C\uB364, \uB290\uB9B0 \uD0C4
11561:       \uD53C\uD574 2\uBC30"
11562:     category: 0
11563:     icon: {fileID: 21300000, guid: a1a40e325ce64e12a81f09b18f96cd98, type: 3}
11564:     implementation: {fileID: 0}
11565:   - itemId: grotesque_growth
11566:     displayName: "\uAE30\uAD34 \uC131\uC7A5"
11567:     description: "10\uCD08\uB9C8\uB2E4 \uD06C\uAE30\uC640 \uC2A4\uD0EF\uC774 \uB79C\uB364
11568:       \uBCC0\uD654"
11569:     category: 0
11570:     icon: {fileID: 21300000, guid: 8758da4361e84648830e30f74074f09b, type: 3}
11571:     implementation: {fileID: 0}
11572:   - itemId: mutation_rampage
11573:     displayName: "\uB3CC\uC5F0\uBCC0\uC774 \uD3ED\uC8FC"
11574:     description: "15\uCD08\uB9C8\uB2E4 7\uCD08 \uB79C\uB364 \uBC84\uD504/\uB514\uBC84\uD504"
11575:     category: 0
11576:     icon: {fileID: 21300000, guid: 9d4c40e93e804cc585eeee9f04224548, type: 3}
11577:     implementation: {fileID: 0}
11578:   - itemId: parasitic_bomb
11579:     displayName: "\uAE30\uC0DD \uD3ED\uD0C4"
11580:     description: "\uC801 \uCC98\uCE58 \uC2DC \uD655\uB960\uB85C \uD50C\uB808\uC774\uC5B4
11581:       \uC8FC\uBCC0 \uD3ED\uBC1C"
11582:     category: 0
11583:     icon: {fileID: 21300000, guid: aeee8a2e20324ab69bb150c9b2e87f30, type: 3}
11584:     implementation: {fileID: 0}
11585:   - itemId: frenzy_hormone
11586:     displayName: "\uAD11\uB780 \uD638\uB974\uBAAC"
11587:     description: "\uD53C\uACA9 \uC2DC \uC9E7\uAC8C \uB79C\uB364 \uB2A5\uB825 \uAC15\uD654"
11588:     category: 0
11589:     icon: {fileID: 21300000, guid: ee0b8c6b690b4141934f6d2bbea51407, type: 3}
11590:     implementation: {fileID: 0}
11591:   startingItemIds: []
11592:   autoPopulateBasicProjectileItems: 1
11593:   enablePickupNotification: 1
11595: MonoBehaviour:
```

## Assets/_Project/Scenes/Intestine.unity

```yaml
523: MonoBehaviour:
534:   ground: {fileID: 143192846}
535:   bolusSprite: {fileID: 21300000, guid: 74ce80df133846afb908e96d2a403d35, type: 3}
536:   initialDelay: 8
537:   warningDuration: 4
538:   restDuration: 30
539:   latestArrival: 7
540:   passageDuration: 8
541:   carriageCount: 14
542:   speed: 100
543:   connectionOverlap: 0.1
544:   laneWidth: 2.6
545:   damage: 1
546:   slowRatio: 0.35
547:   slowDuration: 2
548:   hitCooldown: 1.2
```

## Assets/_Project/Scenes/Liver.unity

```yaml
396: MonoBehaviour:
407:   ground: {fileID: 143192846}
408:   dropSprite: {fileID: 21300000, guid: d58b4b96730447a69f9d018f613b1a61, type: 3}
409:   splashSprite: {fileID: 21300000, guid: f0cdb53f6bd34ee8b9210f9a8fe85942, type: 3}
410:   initialDelay: 5
411:   interval: 12
412:   showerDuration: 3
413:   dropsPerSecond: 2
414:   warningDuration: 0.6
415:   impactRadius: 0.45
416:   damage: 1
417:   hitCooldown: 0.8
418:   minimumDropSpacing: 1.6
419:   fallHeight: 9
420:   fallSpeed: 10
421:   dropHeight: 2.4
422:   splashWidth: 1.3
423:   splashDuration: 0.3
```

## Assets/_Project/Scenes/Stomach.unity

```yaml
716: MonoBehaviour:
727:   maximumGauge: 100
728:   gaugePerSecond: 5
729:   warningDuration: 5
730:   riseDuration: 3
731:   floodDuration: 10
732:   drainDuration: 3
733:   onGaugeChanged:
736:   onAcidLevelChanged:
739:   onWarningStarted:
742:   onFloodStarted:
745:   onCycleCompleted:
748:   currentPhase: 0
749:   currentGauge: 0
750:   acidLevel: 0
```

## Assets/_Project/Art/Generated/FinalBoss/FinalBossPlayerFallback.prefab

```yaml
51: MonoBehaviour:
62:   spriteRenderer: {fileID: 7260781526556950840}
63:   idleSprites:
69:   walkDownSprites:
76:   walkUpSprites:
83:   walkLeftSprites:
90:   walkRightSprites:
97:   warriorIdleSprites:
103:   warriorWalkDownSprites:
110:   warriorWalkUpSprites:
117:   warriorWalkLeftSprites:
124:   warriorWalkRightSprites:
131:   mageIdleSprites:
140:   mageWalkDownSprites:
147:   mageWalkUpSprites:
154:   mageWalkLeftSprites:
161:   mageWalkRightSprites:
168:   archerIdleSprites:
173:   archerWalkDownSprites:
179:   archerWalkUpSprites:
185:   archerWalkLeftSprites:
191:   archerWalkRightSprites:
197:   meleeDown:
206:   meleeUp:
215:   meleeLeft:
224:   meleeRight:
233:   meleeDownLeft:
242:   meleeDownRight:
251:   meleeUpLeft:
260:   meleeUpRight:
269:   rangedDown:
277:   rangedUp:
286:   rangedLeft:
294:   rangedRight:
303:   rangedDownLeft:
311:   rangedDownRight:
319:   rangedUpLeft:
328:   rangedUpRight:
336:   dashSpeed: 20
337:   dashDuration: 0.15
338:   dashCooldown: 0.8
339:   invincibleDuringDash: 1
340:   idleFrameRate: 4
341:   walkFrameRate: 8
342:   attackAnimDuration: 0.2
343:   attackFrameRate: 12
344:   deathSprites:
347:   deathFrameRate: 8
348:   lockYPosition: 0
349:   lockedY: -2
350:   groundOffsetY: -2
351:   useDynamicGroundHeight: 1
352:   footstepInterval: 0.35
353:   enableDebugLogs: 0
405: MonoBehaviour:
416:   meleeAttackDamage: 20
417:   meleeAttackBoxSize: {x: 1, y: 1, z: 2}
418:   meleeAttackOffset: 1
419:   meleeTargetMask:
420:     serializedVersion: 2
422:   meleeOverlapBufferSize: 24
423:   firePoint: {fileID: 173807494929277640}
424:   projectileSpawnPoint: {fileID: 173807494929277640}
425:   projectileSpawnOffset: 0.65
426:   projectileSpawnHeight: 1
427:   projectileSpawnExtraHeight: 2
428:   projectileRange: 8
429:   rangedAttackDamage: 10
430:   rangedTargetMask:
431:     serializedVersion: 2
433:   beamOverlapBufferSize: 48
434:   beamVerticalHalfHeight: 2.5
435:   beamHeightOffset: -0.3
436:   beamVisualDuration: 0.1
437:   beamTextureResourcePath: ItemEffects/beam_organ_beam
438:   meleeCooldown: 0.2
439:   attackCooldown: 1
440:   cellProliferationDelay: 0.5
441:   enableDebugLogs: 0
442:   meleeSlashSpriteResourcePath: AttackVisuals/basic_melee_slash
443:   meleeSlashLifetime: 0.18
444:   meleeSlashScale: 0.95
445:   meleeSlashHeightOffset: 2
446:   meleeSlashSortingOrder: 5050
448: MonoBehaviour:
459:   invincibilityDuration: 0.2
460:   hitFlashInterval: 0.08
461:   hitFlashColor: {r: 1, g: 0.18, b: 0.12, a: 1}
537: MonoBehaviour:
548:   currentClass: 0
549:   enemyMask:
550:     serializedVersion: 2
552:   overlapBufferSize: 32
553:   skillSpawnPoint: {fileID: 0}
554:   projectileSpawnOffset: 0.65
555:   projectileSpawnHeight: 1
556:   skillVerticalOffset: 2
557:   skillHitHeightOffset: 0.75
558:   skillHitVerticalHalfHeight: 4
559:   maxAreaSkillHitTargets: 32
560:   skillEffectSortingOrder: 5200
561:   enableDebugLogs: 0
562:   mageSkill1:
563:     cooldown: 7
564:     radius: 3.75
565:     forwardOffset: 0
566:     baseDamage: 0
567:     additionalDamage: 0
568:     additionalDamageMin: 4
569:     additionalDamageMax: 6
570:     stunDuration: 1
571:     areaEffectPrefab: {fileID: 3983581763031629201, guid: f73f946bcf806594fb157aac0fee0209, type: 3}
572:     areaEffectLifetime: 1
573:     fallbackEffectScale: 10
574:     effectReferenceRadius: 5.28
575:   mageSkill2:
576:     cooldown: 12
577:     radius: 3.5
578:     forwardOffset: 2.5
579:     baseDamage: 10
580:     detonationDelay: 0.7
581:     damageTakenIncreaseRatio: 0.1
582:     damageTakenIncreaseDuration: 3
583:     markEffectPrefab: {fileID: 6458501098902630320, guid: cf3d327d8753b394bb815654ed1f9694, type: 3}
584:     markEffectLifetime: 0.8
585:     markFallbackEffectScale: 0.8
586:     markHeadOffset: 0.2
587:     scaleEffectByTargetSize: 1
588:     effectReferenceTargetHeight: 1.1
589:     effectSizeMultiplier: 1
590:     effectMinScaleMultiplier: 0.7
591:     effectMaxScaleMultiplier: 2.5
592:     explosionEffectPrefab: {fileID: 827007673572255353, guid: 6b33e3138bc5f0d4680d9129b6702575, type: 3}
593:     explosionEffectLifetime: 1
594:     fallbackEffectScale: 3
595:   archerSkill1:
596:     cooldown: 6
597:     projectileCount: 5
598:     fanAngle: 55
599:     projectileDamage: 1
600:     projectileSpeed: 16
601:     projectileRange: 6
602:     projectileLifeTime: 2
603:     projectilePrefab: {fileID: 4631496915048584632, guid: c1cbf5f8078a0444d8f033977407ff70, type: 3}
604:     projectileScale: 0.5
605:     poisonDuration: 3
606:     poisonTickInterval: 1
607:     poisonTickDamage: 1
608:     shootEffectPrefab: {fileID: 0}
609:     shootEffectLifetime: 0.4
610:   archerSkill2:
611:     cooldown: 10
612:     aimDuration: 0.5
613:     range: 10
614:     projectileHitRadius: 0.55
615:     projectileVisualLength: 4
616:     projectileVisualThickness: 0.8
617:     targetForwardOffset: 6
618:     projectileDamage: 4
619:     projectileTravelSpeed: 24
620:     projectileLifeTime: 0.65
621:     projectilePrefab: {fileID: 5377739703324984544, guid: 21c6d35c963bab846bfde230ca8d29fa, type: 3}
622:     projectileScale: 0.45
623:     prefabDirectionAxis: 1
624:     prefabRotationOffsetEuler: {x: 0, y: 0, z: 0}
625:     autoRollToCamera: 1
626:     prefabSurfaceNormalAxis: 2
627:     virusExplosionDamage: 5
628:     virusExplosionRadius: 1.8
629:     virusExplosionEffectPrefab: {fileID: 545757833818925850, guid: e85d0693183cd4c4ca091330d68d1599, type: 3}
630:     virusExplosionEffectLifetime: 1
631:     enableAfterImage: 1
632:     afterImageInterval: 0.03
633:     afterImageFadeDuration: 0.15
634:     afterImageStartAlpha: 0.4
635:     afterImageMaxVisibleCount: 3
636:   warriorSkill1:
637:     cooldown: 4
638:     range: 4.125
639:     forwardAngle: 140
640:     maxTargets: 8
641:     damage: 4
642:     bleedDuration: 2
643:     bleedTickInterval: 1
644:     bleedTickDamage: 1
645:     hitEffectPrefab: {fileID: 840110001, guid: 67f284e219c3405b98cdb7df65e8bf81, type: 3}
646:     hitEffectLifetime: 0.35
647:     fallbackEffectScale: 0.8
648:   warriorSkill2:
649:     cooldown: 8
650:     searchRange: 6
651:     dashSpeed: 18
652:     damage: 8
653:     rootDuration: 2
654:     searchAngle: 90
655:     hitEffectPrefab: {fileID: 840120001, guid: e1b7d39d0ac54e2098105cb7a35a0da4, type: 3}
656:     hitEffectLifetime: 0.5
657:     fallbackEffectScale: 1
658:   autoTargetForwardEnemyForArcherSkill2: 1
659:   archerSkill2AutoTargetAngle: 90
660:   useTestCooldownOverride: 0
661:   testSkill1Cooldown: 1
662:   testSkill2Cooldown: 3
755: MonoBehaviour:
766:   maxItemSlots: 3
767:   allowDuplicateItems: 0
768:   itemEntries:
769:   - itemId: double_core
770:     displayName: "\uC774\uC911\uD575"
771:     description: "\uBC1C\uC0AC\uCCB4 2\uAC1C \uBC1C\uC0AC (\uAC01 75% \uD53C\uD574)"
772:     category: 0
773:     icon: {fileID: 21300000, guid: 0fdf7b5b219f4ad1982fc647c05a9b8a, type: 3}
774:     implementation: {fileID: 0}
775:   - itemId: triple_core
776:     displayName: "\uC0BC\uC911\uD575"
777:     description: "\uBC1C\uC0AC\uCCB4 3\uAC1C \uBC1C\uC0AC (\uAC01 55% \uD53C\uD574)"
778:     category: 0
779:     icon: {fileID: 21300000, guid: 5dd1a4e6e42049c2a47562b7c4f40f2a, type: 3}
780:     implementation: {fileID: 0}
781:   - itemId: homing_cell
782:     displayName: "\uC720\uB3C4 \uC138\uD3EC"
783:     description: "\uC801\uC744 \uCD94\uC801\uD558\uB294 \uC720\uB3C4 \uD22C\uC0AC\uCCB4"
784:     category: 0
785:     icon: {fileID: 21300000, guid: 81d1d7f20f304bb0be0a9cf202b92d1a, type: 3}
786:     implementation: {fileID: 0}
787:   - itemId: hypertrophy_cell
788:     displayName: "\uBE44\uB300 \uC138\uD3EC"
789:     description: "\uD22C\uC0AC\uCCB4 \uD06C\uAE30\xB7\uCDA9\uB3CC \uBC94\uC704 45%,
790:       \uC0AC\uAC70\uB9AC 25% \uC99D\uAC00"
791:     category: 0
792:     icon: {fileID: 21300000, guid: c4f57f7fd6eb454f8d5d6f18f5bc5b2d, type: 3}
793:     implementation: {fileID: 0}
794:   - itemId: reflux_organ
795:     displayName: "\uC5ED\uB958 \uC7A5\uAE30"
796:     description: "\uC655\uBCF5 \uACBD\uB85C\uB9C8\uB2E4 \uCD5C\uB300 3\uBA85 \uD0C0\uACA9,
797:       \uBCF5\uADC0 \uC911 \uAC19\uC740 \uC801 \uC7AC\uC801\uC911 \uC2DC 75% \uD53C\uD574"
798:     category: 0
799:     icon: {fileID: 21300000, guid: 9a7e0131c7664cdb94ca5f2cf389a101, type: 3}
800:     implementation: {fileID: 0}
801:   - itemId: piercing_mucus
802:     displayName: "\uAD00\uD1B5 \uC810\uC561"
803:     description: "\uCD5C\uB300 3\uBA85 \uAD00\uD1B5, \uAD00\uD1B5\uD560 \uB54C\uB9C8\uB2E4
804:       \uD53C\uD574 25% \uAC10\uC18C"
805:     category: 0
806:     icon: {fileID: 21300000, guid: b6629a0e59624c9a8df06efecc705d51, type: 3}
807:     implementation: {fileID: 0}
808:   - itemId: laryngeal_nerve
809:     displayName: "\uD6C4\uB450 \uC2E0\uACBD"
810:     description: "\uB4A4\uCABD\uC73C\uB85C \uCD94\uAC00 \uD22C\uC0AC\uCCB4 \uBC1C\uC0AC"
811:     category: 0
812:     icon: {fileID: 21300000, guid: 7c24bb9d53fa45fda66af0e0bde10276, type: 3}
813:     implementation: {fileID: 0}
814:   - itemId: beam_organ
815:     displayName: "\uAD11\uC120 \uAE30\uAD00"
816:     description: "80% \uD53C\uD574\uC758 \uAD00\uD1B5 \uAD11\uC120 \uACF5\uACA9 (\uB2E4\uC911\uD575\xB7\uD6C4\uBC29
817:       \uBC1C\uC0AC\xB7\uC138\uD3EC \uC99D\uC2DD \uC801\uC6A9)"
818:     category: 0
819:     icon: {fileID: 21300000, guid: 0a725d4bc1ca43be955108ef3f6b412c, type: 3}
820:     implementation: {fileID: 0}
821:   - itemId: split_tissue
822:     displayName: "\uBD84\uC5F4 \uC870\uC9C1"
823:     description: "\uC801 \uBA85\uC911 \uC2DC \uC591\uC606\uC73C\uB85C \uAC01\uAC01
824:       50% \uD53C\uD574\uC758 \uCD94\uAC00 \uD22C\uC0AC\uCCB4 \uC0DD\uC131"
825:     category: 0
826:     icon: {fileID: 21300000, guid: f22457e2296c4e5bb8a989d08d3fe3a4, type: 3}
827:     implementation: {fileID: 0}
828:   - itemId: explosive_blood_cell
829:     displayName: "\uD3ED\uBC1C \uD608\uAD6C"
830:     description: "\uC6D0\uAC70\uB9AC \uD53C\uD574\uAC00 50% \uAC10\uC18C\uD558\uACE0
831:       \uBC94\uC704 \uD3ED\uBC1C \uACF5\uACA9\uC73C\uB85C \uBCC0\uACBD"
832:     category: 0
833:     icon: {fileID: 21300000, guid: 2c49df8a280c4f168256c5b1f1b28011, type: 3}
834:     implementation: {fileID: 0}
835:   - itemId: acidic_rupture
836:     displayName: "\uC0B0\uC131 \uD30C\uC5F4"
837:     description: "\uC801\uC911 \uC704\uCE58\uC5D0 4\uCD08\uAC04 1\uCD08\uB9C8\uB2E4
838:       0.5 \uD53C\uD574\uB97C \uC8FC\uB294 \uC0B0\uC131 \uC7A5\uD310 \uC0DD\uC131
839:       (\uB3D9\uC2DC\uC5D0 \uCD5C\uB300 2\uAC1C)"
840:     category: 0
841:     icon: {fileID: 21300000, guid: 4bb260e9caea4c50a97fc2e6f9af41de, type: 3}
842:     implementation: {fileID: 0}
843:   - itemId: cell_proliferation
844:     displayName: "\uC138\uD3EC \uC99D\uC2DD"
845:     description: "\uC77C\uC815 \uD655\uB960\uB85C \uACF5\uACA9\uC774 \uD55C \uBC88
846:       \uB354 \uBC1C\uB3D9"
847:     category: 0
848:     icon: {fileID: 21300000, guid: 93a5bb1467774edb80e4cc9f0f9586f7, type: 3}
849:     implementation: {fileID: 0}
850:   - itemId: pulse_bullet
851:     displayName: "\uB9E5\uB3D9 \uD0C4\uD658"
852:     description: "\uD22C\uC0AC\uCCB4\uAC00 \uC774\uB3D9\uD558\uBA70 \uD06C\uAE30\uC640
853:       \uC2E4\uC81C \uCDA9\uB3CC \uBC94\uC704\uAC00 \uC99D\uAC00\uD558\uAC70\uB098
854:       \uAC10\uC18C"
855:     category: 0
856:     icon: {fileID: 21300000, guid: 65c749a76f734243b63c76d5d1d43c62, type: 3}
857:     implementation: {fileID: 0}
858:   - itemId: vascular_reflection
859:     displayName: "\uD608\uAD00 \uBC18\uC0AC"
860:     description: "\uBCBD\uC5D0 \uD295\uAE30\uB294 \uBC18\uC0AC \uD22C\uC0AC\uCCB4"
861:     category: 0
862:     icon: {fileID: 21300000, guid: e8a3d94111ab4dd2b70352572c158a16, type: 3}
863:     implementation: {fileID: 0}
864:   - itemId: toxic_mucosa
865:     displayName: "\uB3C5\uC131 \uC810\uB9C9"
866:     description: "\uC801\uC911 \uC2DC 4\uCD08\uAC04 1\uCD08\uB9C8\uB2E4 \uC801\uC911
867:       \uD53C\uD574\uC758 20% \uC911\uB3C5 \uD53C\uD574"
868:     category: 0
869:     icon: {fileID: 21300000, guid: 9986fc82441c47119bb568655973acd9, type: 3}
870:     implementation: {fileID: 0}
871:   - itemId: freezing_nerve
872:     displayName: "\uBE59\uACB0 \uC2E0\uACBD"
873:     description: "\uACF5\uACA9 \uC801\uC911 \uC2DC 2\uCD08\uAC04 \uC801 \uC774\uB3D9\uC18D\uB3C4
874:       30% \uAC10\uC18C"
875:     category: 0
876:     icon: {fileID: 21300000, guid: 2dae7275370f4782bbefcadb7758e704, type: 3}
877:     implementation: {fileID: 0}
878:   - itemId: hemorrhage_organ
879:     displayName: "\uCD9C\uD608 \uAE30\uAD00"
880:     description: "\uC801\uC911 \uC2DC 3.5\uCD08\uAC04 0.8\uCD08\uB9C8\uB2E4 \uC801\uC911
881:       \uD53C\uD574\uC758 18% \uCD9C\uD608 \uD53C\uD574"
882:     category: 0
883:     icon: {fileID: 21300000, guid: 48f6cc999e9a45039fc8eceffc54f26a, type: 3}
884:     implementation: {fileID: 0}
885:   - itemId: overheated_organ
886:     displayName: "\uACFC\uC5F4 \uAE30\uAD00"
887:     description: "\uC5F0\uC18D \uACF5\uACA9\uB9C8\uB2E4 \uACF5\uACA9\uC18D\uB3C4
888:       +10% (10\uC911\uCCA9 \uC2DC \uC8FC\uBCC0 150% \uD53C\uD574, \uC790\uC2E0 1
889:       \uD53C\uD574)"
890:     category: 0
891:     icon: {fileID: 21300000, guid: a2eef1bb211a4210ab5619137abc7952, type: 3}
892:     implementation: {fileID: 0}
893:   - itemId: mutant_eye
894:     displayName: "\uB3CC\uC5F0\uBCC0\uC774 \uC548\uAD6C"
895:     description: "\uBC1C\uC0AC \uBC29\uD5A5\uC774 \uC88C\uC6B0 40\uB3C4 \uB0B4\uC5D0\uC11C
896:       \uBB34\uC791\uC704 \uBCC0\uD654, \uACF5\uACA9\uB825 +2"
897:     category: 0
898:     icon: {fileID: 21300000, guid: 02864ace75954328bb1c8ad1c7e6d6c7, type: 3}
899:     implementation: {fileID: 0}
900:   - itemId: organ_tentacle
901:     displayName: "\uC7A5\uAE30 \uCD09\uC218"
902:     description: "\uC8FC\uBCC0 \uC801 \uCD5C\uB300 2\uBA85\uC744 1.2\uCD08\uB9C8\uB2E4
903:       30% \uD53C\uD574\uB85C \uACF5\uACA9, \uAE30\uBCF8 \uACF5\uACA9 \uD53C\uD574
904:       20% \uAC10\uC18C"
905:     category: 0
906:     icon: {fileID: 21300000, guid: 30ea581a77ed4f50b423105a66d8f601, type: 3}
907:     implementation: {fileID: 0}
908:   - itemId: rampage_bloodflow
909:     displayName: "\uD3ED\uC8FC \uD608\uB958"
910:     description: "\uC774\uB3D9 8\uCD08\uB9C8\uB2E4 \uACF5\uACA9\uB825 +1 (\uCD5C\uB300
911:       +2), \uC815\uC9C0 \uC2DC 4\uCD08\uB9C8\uB2E4 \uAC10\uC18C"
912:     category: 0
913:     icon: {fileID: 21300000, guid: ac6f186a235348a0bfbfdfde74c06175, type: 3}
914:     implementation: {fileID: 0}
915:   - itemId: muscle_spasm
916:     displayName: "\uADFC\uC721 \uACBD\uB828"
917:     description: "\uADFC\uC811 \uACF5\uACA9 \uBC94\uC704 1.7\uBC30, \uACF5\uACA9\uC18D\uB3C4
918:       20% \uAC10\uC18C"
919:     category: 0
920:     icon: {fileID: 21300000, guid: da1db62dc85d4ff8b3718e999489bbae, type: 3}
921:     implementation: {fileID: 0}
922:   - itemId: unstable_core
923:     displayName: "\uBD88\uC548\uC815 \uD575"
924:     description: "5\uCD08\uB9C8\uB2E4 \uAE30\uBCF8 \uACF5\uACA9 \uD53C\uD574\uAC00
925:       60~160% \uC0AC\uC774\uC5D0\uC11C \uBCC0\uACBD"
926:     category: 0
927:     icon: {fileID: 21300000, guid: 0238764490da4eb7b6b4775811a9ca1f, type: 3}
928:     implementation: {fileID: 0}
929:   - itemId: bio_resonance
930:     displayName: "\uC0DD\uCCB4 \uACF5\uBA85"
931:     description: "3\uCD08 \uB0B4 \uAC19\uC740 \uC801 \uC5F0\uC18D \uACF5\uACA9 \uC2DC
932:       \uD53C\uD574 +15% (\uCD5C\uB300 +45%)"
933:     category: 0
934:     icon: {fileID: 21300000, guid: 08005822b1914de0bee328b21739e6af, type: 3}
935:     implementation: {fileID: 0}
936:   - itemId: blood_pressure_burst
937:     displayName: "\uD608\uC555 \uD3ED\uBC1C"
938:     description: "\uC783\uC740 \uCCB4\uB825 10%\uB2F9 \uACF5\uACA9\uC18D\uB3C4 +6%
939:       (\uCD5C\uB300 +60%)"
940:     category: 0
941:     icon: {fileID: 21300000, guid: bf1c77e0f1a348268cc2ef88efd45fe6, type: 3}
942:     implementation: {fileID: 0}
943:   - itemId: void_cell
944:     displayName: "\uACF5\uD5C8 \uC138\uD3EC"
945:     description: "\uACF5\uACA9 \uC2DC 20% \uD655\uB960\uB85C \uC801 \uC8FC\uBCC0\uC5D0\uC11C
946:       85% \uD53C\uD574\uC758 \uCD94\uAC00 \uD0C4\uD658 \uC0DD\uC131"
947:     category: 0
948:     icon: {fileID: 21300000, guid: 44b4c21f87dc4d1980ec7aa03b0977b4, type: 3}
949:     implementation: {fileID: 0}
950:   - itemId: forbidden_growth
951:     displayName: "\uAE08\uB2E8 \uC131\uC7A5"
952:     description: "\uCCB4\uB825 -3, \uACF5\uACA9\uB825 +2 \uC99D\uAC00"
953:     category: 0
954:     icon: {fileID: 21300000, guid: a0bf5a6d28c64823b020d59972591ee2, type: 3}
955:     implementation: {fileID: 0}
956:   - itemId: overclock_nerve
957:     displayName: "\uACFC\uC18D \uC2E0\uACBD"
958:     description: "\uCCB4\uB825 -2, \uC774\uB3D9\uC18D\uB3C4 +2 \uC99D\uAC00"
959:     category: 0
960:     icon: {fileID: 21300000, guid: d4b0d7a6e8b94e12a5b9c77fa6d42430, type: 3}
961:     implementation: {fileID: 0}
962:   - itemId: blood_contract
963:     displayName: "\uD53C\uC758 \uACC4\uC57D"
964:     description: "\uC801 20\uBA85 \uCC98\uCE58 \uC2DC \uCCB4\uB825 +1 (\uCD5C\uB300
965:       +5), \uBC1B\uB294 \uD53C\uD574 1.3\uBC30"
966:     category: 0
967:     icon: {fileID: 21300000, guid: 7e0fe0ed2b824d7d9fdc4f25b49b77bd, type: 3}
968:     implementation: {fileID: 0}
969:   - itemId: hyperplasia_heart
970:     displayName: "\uACFC\uC99D\uC2DD \uC2EC\uC7A5"
971:     description: "\uC783\uC740 \uCCB4\uB825\uC5D0 \uBE44\uB840\uD574 \uACF5\uACA9\uB825
972:       \uC99D\uAC00 (\uCD5C\uB300 +2)"
973:     category: 0
974:     icon: {fileID: 21300000, guid: e9365419c64b46c9aa094fe60ef9d0ce, type: 3}
975:     implementation: {fileID: 0}
976:   - itemId: decay_organ
977:     displayName: "\uBD80\uD328 \uC7A5\uAE30"
978:     description: "3\uBD84\uB9C8\uB2E4 \uACF5\uACA9\uB825 +1 (\uCD5C\uB300 +2)"
979:     category: 0
980:     icon: {fileID: 21300000, guid: 9b3e06a4a4c74590918318573745d224, type: 3}
981:     implementation: {fileID: 0}
982:   - itemId: rupture_muscle
983:     displayName: "\uD30C\uC5F4 \uADFC\uC721"
984:     description: "\uACF5\uACA9 2\uD68C\uB9C8\uB2E4 \uACF5\uACA9\uB825 +0.5\xB7\uC774\uB3D9\uC18D\uB3C4
985:       -0.25 (\uCD5C\uB300 3\uC911\uCCA9)"
986:     category: 0
987:     icon: {fileID: 21300000, guid: 64519f00218d4dc5945148c55ee65576, type: 3}
988:     implementation: {fileID: 0}
989:   - itemId: imperfect_regeneration
990:     displayName: "\uBD88\uC644\uC804 \uC7AC\uC0DD"
991:     description: "\uCCB4\uB825 -2, \uD53C\uACA9 2\uCD08 \uD6C4 \uCCB4\uB825 +1 \uD68C\uBCF5
992:       (10\uCD08 \uCFE8\uD0C0\uC784)"
993:     category: 0
994:     icon: {fileID: 21300000, guid: b8585326984a471c81e363552ede00cc, type: 3}
995:     implementation: {fileID: 0}
996:   - itemId: severance_reflex
997:     displayName: "\uC808\uB2E8 \uBC18\uC0AC"
998:     description: "\uD53C\uACA9 \uD6C4 2\uCD08\uAC04 \uACF5\uACA9\uB825 +2"
999:     category: 0
1000:     icon: {fileID: 21300000, guid: edba15146bde460caf3d26ebb02fdcb4, type: 3}
1001:     implementation: {fileID: 0}
1002:   - itemId: bio_gamble
1003:     displayName: "\uC0DD\uCCB4 \uB3C4\uBC15"
1004:     description: "\uB808\uBCA8\uC5C5 \uC120\uD0DD \uC2DC \uC131\uC7A5\uB7C9\uC774
1005:       0~+2 \uBC94\uC704\uC5D0\uC11C \uBB34\uC791\uC704 \uACB0\uC815"
1006:     category: 0
1007:     icon: {fileID: 21300000, guid: 3975e26e5a454500bb016c167ad152e7, type: 3}
1008:     implementation: {fileID: 0}
1009:   - itemId: exoskeleton
1010:     displayName: "\uC678\uACE8\uACA9"
1011:     description: "\uBC1B\uB294 \uD53C\uD574 25% \uAC10\uC18C, \uB300\uC2E0 \uC774\uB3D9\uC18D\uB3C4
1012:       -1.5"
1013:     category: 0
1014:     icon: {fileID: 21300000, guid: 1c06e224706e4e41ae9ffc85e79707bc, type: 3}
1015:     implementation: {fileID: 0}
1016:   - itemId: platelet_membrane
1017:     displayName: "\uD608\uC18C\uD310 \uB9C9"
1018:     description: "20\uCD08\uB9C8\uB2E4 \uD53C\uD574 1\uC744 \uB9C9\uB294 \uBCF4\uD638\uB9C9
1019:       \uC0DD\uC131"
1020:     category: 0
1021:     icon: {fileID: 21300000, guid: ac5d40ecd9bc4f81aeb5db68f9ff801b, type: 3}
1022:     implementation: {fileID: 0}
1023:   - itemId: recovery_factor
1024:     displayName: "\uD68C\uBCF5 \uC778\uC790"
1025:     description: "30\uCD08\uB9C8\uB2E4 \uCCB4\uB825 +1 \uD68C\uBCF5"
1026:     category: 0
1027:     icon: {fileID: 21300000, guid: ea4f76c79d6e4826af2526d2eb2ab3fa, type: 3}
1028:     implementation: {fileID: 0}
1029:   - itemId: reflective_skin
1030:     displayName: "\uBC18\uC0AC \uD53C\uBD80"
1031:     description: "\uD53C\uACA9 \uC2DC \uC2E4\uC81C \uBC1B\uC740 \uD53C\uD574\uC758
1032:       75% \uBC18\uC0AC"
1033:     category: 0
1034:     icon: {fileID: 21300000, guid: 515cafcd131d46d5b031bd4d409ee9b0, type: 3}
1035:     implementation: {fileID: 0}
1036:   - itemId: bio_barrier
1037:     displayName: "\uC0DD\uCCB4 \uC7A5\uB9C9"
1038:     description: "\uC815\uC9C0 1.5\uCD08\uB9C8\uB2E4 \uBC1B\uB294 \uD53C\uD574 10%
1039:       \uAC10\uC18C (\uCD5C\uB300 40%)"
1040:     category: 0
1041:     icon: {fileID: 21300000, guid: bd715116c6d946a18bd57a4363532296, type: 3}
1042:     implementation: {fileID: 0}
1043:   - itemId: split_regeneration
1044:     displayName: "\uBD84\uC5F4 \uC7AC\uC0DD"
1045:     description: "\uCCB4\uB825 0\uC774 \uB420 \uB54C \uC791\uC740 \uCCB4\uB825\uC73C\uB85C
1046:       \uBD80\uD65C"
1047:     category: 0
1048:     icon: {fileID: 21300000, guid: b7e6394cf9d64f5aacfe52a31e639e7c, type: 3}
1049:     implementation: {fileID: 0}
1050:   - itemId: infected_host
1051:     displayName: "\uAC10\uC5FC \uC219\uC8FC"
1052:     description: "\uC7A1\uBAB9 \uCC98\uCE58 \uC2DC 15% \uD655\uB960\uB85C 6\uCD08\uAC04
1053:       35% \uD53C\uD574\uC758 \uC544\uAD70 \uC0DD\uC131 (\uCD5C\uB300 2\uBA85)"
1054:     category: 0
1055:     icon: {fileID: 21300000, guid: 2b9e25cfeeef435eac4f4788c2beae32, type: 3}
1056:     implementation: {fileID: 0}
1057:   - itemId: spore_colony
1058:     displayName: "\uD3EC\uC790 \uAD70\uC9D1"
1059:     description: "\uC77C\uC815 \uC2DC\uAC04\uB9C8\uB2E4 \uC801\uC5D0\uAC8C \uB2EC\uB824\uB4DC\uB294
1060:       \uC791\uC740 \uD3EC\uC790 \uC0DD\uBA85\uCCB4 \uC18C\uD658"
1061:     category: 0
1062:     icon: {fileID: 21300000, guid: 1fd0bb7116fe498ba24b0558dc0b04c4, type: 3}
1063:     implementation: {fileID: 0}
1064:   - itemId: blood_drone
1065:     displayName: "\uD608\uC561 \uB4DC\uB860"
1066:     description: "1\uCD08\uB9C8\uB2E4 \uAC00\uAE4C\uC6B4 \uC801\uC5D0\uAC8C \uACF5\uACA9\uB825\uC758
1067:       30% \uD53C\uD574"
1068:     category: 0
1069:     icon: {fileID: 21300000, guid: 1f4bbe17d34a41efbfeacca751dc798a, type: 3}
1070:     implementation: {fileID: 0}
1071:   - itemId: guardian_organ
1072:     displayName: "\uC218\uD638 \uC7A5\uAE30"
1073:     description: "\uD50C\uB808\uC774\uC5B4 \uC8FC\uBCC0\uC744 \uB9F4\uB3CC\uBA70
1074:       3.5\uCD08\uB9C8\uB2E4 \uC801 \uD22C\uC0AC\uCCB4 \uD558\uB098 \uBC29\uC5B4"
1075:     category: 0
1076:     icon: {fileID: 21300000, guid: 1b6965b707da40f08ab8f6391cf9312f, type: 3}
1077:     implementation: {fileID: 0}
1078:   - itemId: tentacle_colony
1079:     displayName: "\uCD09\uC218 \uAD70\uCCB4"
1080:     description: "3\uCD08\uB9C8\uB2E4 \uC801 2\uBA85\uC5D0\uAC8C 1.2\uCD08\uAC04
1081:       60% \uB454\uD654\uC640 10% \uD53C\uD574"
1082:     category: 0
1083:     icon: {fileID: 21300000, guid: 40925b61970b44f4bb0771f2324091c4, type: 3}
1084:     implementation: {fileID: 0}
1085:   - itemId: electric_neural_network
1086:     displayName: "\uC804\uAE30 \uC2E0\uACBD\uB9DD"
1087:     description: "\uC801 \uCC98\uCE58 \uC2DC \uCD5C\uB300 3\uBA85\uC5D0\uAC8C \uD53C\uD574
1088:       1\uC758 \uC5F0\uC1C4 \uC804\uB958 (5\uCD08 \uCFE8\uD0C0\uC784)"
1089:     category: 0
1090:     icon: {fileID: 21300000, guid: 79ccd4e44605424b904bacee6d233d5d, type: 3}
1091:     implementation: {fileID: 0}
1092:   - itemId: infection_transference
1093:     displayName: "\uAC10\uC5FC \uC804\uC774"
1094:     description: "\uC801 \uCC98\uCE58 \uC2DC \uC8FC\uBCC0 3\uBA85\uC5D0\uAC8C 3\uCD08\uAC04
1095:       \uCD08\uB2F9 0.5 \uAC10\uC5FC \uD53C\uD574 (5\uCD08 \uCFE8\uD0C0\uC784)"
1096:     category: 0
1097:     icon: {fileID: 21300000, guid: 44c180ca917c49a9a6025e11944cb3a6, type: 3}
1098:     implementation: {fileID: 0}
1099:   - itemId: macrophage
1100:     displayName: "\uB300\uC2DD \uC138\uD3EC"
1101:     description: "\uC801 \uCC98\uCE58 \uC2DC 5\uCD08\uAC04 \uACF5\uACA9\uB825 +0.5
1102:       (\uCD5C\uB300 3\uC911\uCCA9)"
1103:     category: 0
1104:     icon: {fileID: 21300000, guid: 9b0c2139036f4cd4a9bc0b31cfc7c1dc, type: 3}
1105:     implementation: {fileID: 0}
1106:   - itemId: gluttonous_organ
1107:     displayName: "\uD3ED\uC2DD \uC7A5\uAE30"
1108:     description: "\uC801 \uCC98\uCE58 \uC2DC 3\uCD08 \uB3D9\uC548 \uC774\uB3D9\uC18D\uB3C4
1109:       \uC99D\uAC00 (\uCD5C\uB300 4\uC2A4\uD0DD)"
1110:     category: 0
1111:     icon: {fileID: 21300000, guid: 235474cec8ea4c9b9b40cd93bfffbbc8, type: 3}
1112:     implementation: {fileID: 0}
1113:   - itemId: heart_sniper
1114:     displayName: "\uC2EC\uC7A5 \uC800\uACA9"
1115:     description: "\uCCB4\uB825 60% \uC774\uC0C1\uC778 \uC801\uC5D0\uAC8C \uC9C1\uC811
1116:       \uACF5\uACA9 \uD53C\uD574 35% \uC99D\uAC00"
1117:     category: 0
1118:     icon: {fileID: 21300000, guid: ca1bb1b767904f3db5d0b69dcbf1fc7b, type: 3}
1119:     implementation: {fileID: 0}
1120:   - itemId: bloodflow_acceleration
1121:     displayName: "\uD608\uB958 \uAC00\uC18D"
1122:     description: "\uBCF4\uC2A4\xB7\uC5D8\uB9AC\uD2B8 \uADFC\uCC98\uC5D0\uC11C \uACF5\uACA9\uC18D\uB3C4
1123:       35% \uC99D\uAC00"
1124:     category: 0
1125:     icon: {fileID: 21300000, guid: 304747b797864b1fa1386ba975331c2d, type: 3}
1126:     implementation: {fileID: 0}
1127:   - itemId: focused_nerve
1128:     displayName: "\uC9D1\uC911 \uC2E0\uACBD"
1129:     description: "\uC8FC\uBCC0 \uC801 1\uBA85 \uC774\uD558\uC77C \uB54C \uACF5\uACA9\uB825
1130:       +0.75, 2~3\uBA85\uC77C \uB54C +0.5"
1131:     category: 0
1132:     icon: {fileID: 21300000, guid: 298d285e577e42a895bc20ba5738ceae, type: 3}
1133:     implementation: {fileID: 0}
1134:   - itemId: execution_instinct
1135:     displayName: "\uCC98\uD615 \uBCF8\uB2A5"
1136:     description: "\uCCB4\uB825 20% \uBBF8\uB9CC \uC801\uC744 20% \uD655\uB960\uB85C
1137:       \uCC98\uD615 (\uBCF4\uC2A4\xB7\uC5D8\uB9AC\uD2B8\uB294 \uCD94\uAC00 \uD53C\uD574
1138:       50%)"
1139:     category: 0
1140:     icon: {fileID: 21300000, guid: 486ccc7133284a47a64f0ad5e95cfa36, type: 3}
1141:     implementation: {fileID: 0}
1142:   - itemId: berserk_cell
1143:     displayName: "\uAD11\uD3ED \uC138\uD3EC"
1144:     description: "\uBCF4\uC2A4\uC804 \uC9C4\uC785 \uC2DC 15\uCD08\uAC04 \uACF5\uACA9\uB825
1145:       +0.75, \uC774\uB3D9\uC18D\uB3C4 +0.5, \uACF5\uACA9\uC18D\uB3C4 +30%"
1146:     category: 0
1147:     icon: {fileID: 21300000, guid: bb50b60bc68046478ebcc0893f470801, type: 3}
1148:     implementation: {fileID: 0}
1149:   - itemId: unstable_cell
1150:     displayName: "\uBD88\uC548\uC815 \uC138\uD3EC"
1151:     description: "\uD22C\uC0AC\uCCB4 \uC18D\uB3C4 50~150%, 80% \uC774\uD558\uC758
1152:       \uB290\uB9B0 \uD0C4\uC740 \uD53C\uD574 150%"
1153:     category: 0
1154:     icon: {fileID: 21300000, guid: a1a40e325ce64e12a81f09b18f96cd98, type: 3}
1155:     implementation: {fileID: 0}
1156:   - itemId: grotesque_growth
1157:     displayName: "\uAE30\uAD34 \uC131\uC7A5"
1158:     description: "10\uCD08\uB9C8\uB2E4 \uC18C\uD615\uD654\xB7\uC774\uB3D9\uC18D\uB3C4
1159:       +1 \uB610\uB294 \uB300\uD615\uD654\xB7\uACF5\uACA9\uB825 +1\xB7\uC774\uB3D9\uC18D\uB3C4
1160:       -0.5"
1161:     category: 0
1162:     icon: {fileID: 21300000, guid: 8758da4361e84648830e30f74074f09b, type: 3}
1163:     implementation: {fileID: 0}
1164:   - itemId: mutation_rampage
1165:     displayName: "\uB3CC\uC5F0\uBCC0\uC774 \uD3ED\uC8FC"
1166:     description: "15\uCD08\uB9C8\uB2E4 7\uCD08\uAC04 \uBB34\uC791\uC704 \uC2A4\uD0EF
1167:       \uBC84\uD504 \uB610\uB294 \uB514\uBC84\uD504"
1168:     category: 0
1169:     icon: {fileID: 21300000, guid: 9d4c40e93e804cc585eeee9f04224548, type: 3}
1170:     implementation: {fileID: 0}
1171:   - itemId: parasitic_bomb
1172:     displayName: "\uAE30\uC0DD \uD3ED\uD0C4"
1173:     description: "\uC801 \uCC98\uCE58 \uC2DC 20% \uD655\uB960\uB85C \uBC18\uACBD
1174:       4\uC5D0 \uACF5\uACA9\uB825 150% \uD3ED\uBC1C"
1175:     category: 0
1176:     icon: {fileID: 21300000, guid: aeee8a2e20324ab69bb150c9b2e87f30, type: 3}
1177:     implementation: {fileID: 0}
1178:   - itemId: frenzy_hormone
1179:     displayName: "\uAD11\uB780 \uD638\uB974\uBAAC"
1180:     description: "\uD53C\uACA9 \uC2DC \uBB34\uC791\uC704 \uB2A5\uB825 +0.5~1 (3\uCD08
1181:       \uC9C0\uC18D, 8\uCD08 \uCFE8\uD0C0\uC784)"
1182:     category: 0
1183:     icon: {fileID: 21300000, guid: ee0b8c6b690b4141934f6d2bbea51407, type: 3}
1184:     implementation: {fileID: 0}
1185:   startingItemIds: []
1186:   autoPopulateBasicProjectileItems: 1
1187:   enablePickupNotification: 1
1189: MonoBehaviour:
1200:   doubleShotSpreadAngle: 7
1201:   tripleShotSpreadAngle: 11
1202:   doubleShotDamageMultiplier: 0.75
1203:   tripleShotDamageMultiplier: 0.55
1204:   backShotDamageMultiplier: 0.9
1205:   homingTurnRate: 7
1206:   homingSearchRadius: 8
1207:   boomerangReturnDistance: 8
1208:   boomerangRepeatHitDamageMultiplier: 0.75
1209:   boomerangMaxHitsPerPass: 3
1210:   piercingHitCount: 3
1211:   piercingSubsequentHitDamageMultiplier: 0.75
1212:   reflectionBounceCount: 3
1213:   hypertrophyScaleMultiplier: 1.45
1214:   hypertrophyRangeMultiplier: 1.25
1215:   pulseGrowEndScale: 2.6
1216:   pulseShrinkEndScale: 0.2
1217:   splitDamageMultiplier: 0.5
1218:   splitRangeMultiplier: 0.65
1219:   splitAngle: 90
1220:   explosionRadius: 2
1221:   explosionDamageMultiplier: 1
1222:   explosiveBloodCellRangedDamageMultiplier: 0.5
1223:   acidDuration: 4
1224:   acidTickInterval: 1
1225:   acidTickDamage: 0.5
1226:   poisonDuration: 4
1227:   poisonTickInterval: 1
1228:   poisonTickDamageRatio: 0.2
1229:   freezeSlowRatio: 0.3
1230:   freezeDuration: 2
1231:   bleedDuration: 3.5
1232:   bleedTickInterval: 0.8
1233:   bleedTickDamageRatio: 0.18
1234:   cellProliferationChance: 0.25
1235:   cellProliferationDamageMultiplier: 0.9
1236:   beamRadius: 0.8
1237:   beamDamageMultiplier: 0.8
1238:   beamHitBufferSize: 48
1239:   overheatMaxStacks: 10
1240:   overheatStackWindow: 1.4
1241:   overheatPerStackAttackSpeedBonus: 0.1
1242:   overheatDecayInterval: 5
1243:   overheatExplosionRadius: 2.5
1244:   overheatExplosionDamageMultiplier: 1.5
1245:   overheatExplosionSelfDamage: 1
1246:   mutantEyeAccuracyPenaltyAngle: 40
1247:   mutantEyeFlatDamageBonus: 2
1248:   organTentacleAutoAttackInterval: 1.2
1249:   organTentacleAutoAttackRadius: 4.5
1250:   organTentacleAutoAttackDamageMultiplier: 0.3
1251:   organTentacleMaxTargets: 2
1252:   organTentacleBaseAttackPenaltyMultiplier: 0.8
1253:   rampageMoveSecondsPerBonus: 8
1254:   rampageMaxAttackBonus: 2
1255:   rampageIdleSecondsPerDecay: 4
1256:   muscleSpasmMeleeRangeMultiplier: 1.7
1257:   muscleSpasmAttackSpeedMultiplier: 0.8
1258:   unstableCoreMinAttackRatio: 0.6
1259:   unstableCoreMaxAttackRatio: 1.6
1260:   unstableCoreRerollInterval: 5
1261:   bioResonanceStackDamageBonus: 0.15
1262:   bioResonanceMaxStacks: 3
1263:   bioResonanceStackWindow: 3
1264:   bloodPressureBaseAttackSpeedAdd: 0
1265:   bloodPressurePerTenPercentMissingAdd: 0.06
1266:   voidCellChance: 0.2
1267:   voidCellDamageMultiplier: 0.85
1268:   voidCellSpawnRadius: 2.2
1269:   forbiddenGrowthMaxHealthPenalty: 3
1270:   forbiddenGrowthFlatAttackBonus: 2
1271:   overclockNerveMaxHealthPenalty: 2
1272:   overclockNerveFlatMoveSpeedBonus: 2
1273:   bloodContractIncomingDamageMultiplier: 1.3
1274:   bloodContractKillsPerHeal: 20
1275:   bloodContractHealthGainAmount: 1
1276:   bloodContractMaxHealthGainCount: 5
1277:   hyperplasiaMissingHealthAttackBonusMax: 2
1278:   decayOrganSecondsPerAttackBonus: 180
1279:   decayOrganMaxAttackBonus: 2
1280:   ruptureMuscleAttackBonusPerStack: 0.5
1281:   ruptureMuscleMovePenaltyPerStack: 0.25
1282:   ruptureMuscleMaxStacks: 3
1283:   ruptureMuscleDecayDelay: 3
1284:   imperfectRegenMaxHealthPenalty: 2
1285:   imperfectRegenDelay: 2
1286:   imperfectRegenHealPerTrigger: 1
1287:   imperfectRegenCooldownDuration: 10
1288:   severanceReflexDuration: 2
1289:   severanceReflexFlatAttackBonus: 2
1290:   exoskeletonDamageReductionRatio: 0.25
1291:   exoskeletonMoveSpeedPenalty: 1.5
1292:   plateletMembraneInterval: 20
1293:   plateletMembraneShieldAmount: 1
1294:   recoveryFactorInterval: 30
1295:   recoveryFactorHealAmount: 1
1296:   reflectiveSkinDamageRatio: 0.75
1297:   bioBarrierIdleSecondsPerStep: 1.5
1298:   bioBarrierReductionPerStep: 0.1
1299:   bioBarrierMaxReduction: 0.4
1300:   splitRegenerationReviveHealth: 2
1301:   infectedHostChance: 0.15
1302:   infectedHostLifetime: 6
1303:   infectedHostDamageMultiplier: 0.35
1304:   infectedHostAttackInterval: 1
1305:   infectedHostAttackRadius: 1.25
1306:   infectedHostSearchRadius: 7
1307:   infectedHostMaxAllies: 2
1308:   sporeSpawnInterval: 8
1309:   sporeLifetime: 6
1310:   sporeDamageMultiplier: 1
1311:   sporeSearchRadius: 8
1312:   sporeBurstRadius: 1.1
1313:   bloodDroneOrbitRadius: 1.4
1314:   bloodDroneFireInterval: 1
1315:   bloodDroneDamageMultiplier: 0.3
1316:   bloodDroneTargetRadius: 9
1317:   guardianOrganOrbitRadius: 1.2
1318:   guardianOrganBlockRadius: 0.9
1319:   guardianOrganCooldown: 3.5
1320:   tentacleBindInterval: 3
1321:   tentacleBindRadius: 5.5
1322:   tentacleBindDuration: 1.2
1323:   tentacleSlowRatio: 0.6
1324:   tentacleDamageMultiplier: 0.1
1325:   tentacleMaxTargets: 2
1326:   electricChainDamage: 1
1327:   electricChainRadius: 6
1328:   electricChainMaxHits: 3
1329:   electricChainCooldown: 5
1330:   infectionTransferRadius: 4.5
1331:   infectionTransferMaxTargets: 3
1332:   infectionTransferDuration: 3
1333:   infectionTransferTickInterval: 1
1334:   infectionTransferTickDamage: 0.5
1335:   infectionTransferCooldown: 5
1336:   macrophageDuration: 5
1337:   macrophageAttackBonusPerStack: 0.5
1338:   macrophageMaxStacks: 3
1339:   gluttonousOrganDuration: 3
1340:   gluttonousOrganMoveBonusPerStack: 0.5
1341:   gluttonousOrganMaxStacks: 4
1342:   heartSniperHealthThreshold: 0.6
1343:   heartSniperDamageBonusRatio: 0.35
1344:   bloodflowAccelerationRadius: 9
1345:   bloodflowAccelerationAttackSpeedBonus: 0.35
1346:   focusedNerveRadius: 6
1347:   focusedNerveHighAttackBonus: 0.75
1348:   focusedNerveLowAttackBonus: 0.5
1349:   executionInstinctHealthThreshold: 0.2
1350:   executionInstinctChance: 0.2
1351:   executionInstinctBossDamageMultiplier: 1.5
1352:   berserkCellDuration: 15
1353:   berserkCellAttackBonus: 0.75
1354:   berserkCellMoveBonus: 0.5
1355:   berserkCellAttackSpeedBonus: 0.3
1356:   berserkCellBossDetectionRadius: 18
1357:   unstableCellMinProjectileSpeedMultiplier: 0.5
1358:   unstableCellMaxProjectileSpeedMultiplier: 1.5
1359:   unstableCellSlowThreshold: 0.8
1360:   unstableCellSlowDamageMultiplier: 1.5
1361:   grotesqueGrowthInterval: 10
1362:   grotesqueGrowthSmallScale: 0.75
1363:   grotesqueGrowthLargeScale: 1.35
1364:   grotesqueGrowthSmallMoveBonus: 1
1365:   grotesqueGrowthLargeAttackBonus: 1
1366:   grotesqueGrowthLargeMovePenalty: 0.5
1367:   mutationRampageInterval: 15
1368:   mutationRampageDuration: 7
1369:   mutationRampageBuffChance: 0.6
1370:   mutationRampageAttackBuff: 1
1371:   mutationRampageMoveBuff: 1
1372:   mutationRampageAttackSpeedBuff: 0.5
1373:   mutationRampageAttackDebuff: 0.5
1374:   mutationRampageMoveDebuff: 0.75
1375:   mutationRampageAttackSpeedDebuff: 0.25
1376:   parasiticBombChance: 0.2
1377:   parasiticBombRadius: 4
1378:   parasiticBombDamageMultiplier: 1.5
1379:   frenzyHormoneDuration: 3
1380:   frenzyHormoneCooldown: 8
1381:   frenzyHormoneMinBonus: 0.5
1382:   frenzyHormoneMaxBonus: 1
```
