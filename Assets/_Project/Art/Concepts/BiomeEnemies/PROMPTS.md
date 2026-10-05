# 바이옴 전용 잔몹 — 이미지 생성 프롬프트

생성일: 2026-09-12

방식: 내장 `image_gen` 도구. 외부 이미지 검색이나 API/CLI 생성은 사용하지 않았다. 기존 프로젝트 스프라이트를 스타일 참조로 제공했다. 아래는 실제 전달한 프롬프트이며, 수치와 전투 규칙의 기준은 `README.md`다.

## 장 — intestine

참조 이미지:
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/대식세포IiDLE 1.png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/NK세포 이동 (2).png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/Player/Warrior/대기/전사형 IDEL.png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/middle boss/장/대기/정면 idle 1.png`

### 최종 생성 프롬프트

```text
Use case: stylized-concept.
Asset type: Necrocis biome-exclusive ordinary-enemy art and combat reference board.
Input images: The first two supplied images are EXISTING small common-enemy sprite strips, third is the tiny red player sprite strip, and fourth is this biome's EXISTING BOSS sprite strip. They are style, scale-language and palette references ONLY. Make NEW common enemies, not copies of bosses. Match the actual simple, charming-but-uncanny biological cartoon pixel art: chunky deliberate pixels, dark plum 1-pixel outlines at source scale, flat 3-tone shading, expressive simple eyes, soft membranous silhouettes. Ordinary enemies with implied 24–48-pixel base sprites enlarged with crisp nearest-neighbor-like pixel edges; no realistic anatomical gore, no painterly rendering, no 3D, no detailed fantasy armor, no generic skeletons/zombies, no humans. It is a Unity 2.5D game with upright 2D billboard sprites over a top-down ground plane; show characters from frontal/slight three-quarter view with their lower contact point clear, never a side-scrolling scene.
Create one finished landscape art-director reference sheet, 2048x1536 if possible, consistent clean 2 by 2 card layout, exactly FOUR distinct enemy concepts. Warm off-white short Korean labels, restrained biome accent lines on flat deep plum charcoal background. Large board title top left, subtitle directly below. Every card: bold ID and Korean name, one large complete isolated IDLE sprite at left, then two smaller complete poses at right showing wind-up and attack outcome, separated by one clear arrow. Short labels above poses exactly "기본", "예고", "공격". Add the one short Korean attack caption given below at the bottom of each card. Leave generous padding and crisp readable text. Each attack picture must include a very small simplified red player target for spatial clarity; amber dotted floor outlines are wind-up warnings, solid colored effects are actual attacks. Avoid excessive particles and overlapping visual clutter. Keep enemy silhouettes different and color readable against backdrop. Make attack organs change visibly during wind-up. This is a concept board, not a packed animation atlas, not an in-game screenshot. No watermarks, fake UI, paragraphs, pseudo code, tiny annotation text or extra enemies.

Title (verbatim): "NECROCIS / 장 전용 잔몹"
Subtitle: "기생 · 점액 · 연동운동"
Palette: pale cream parasite tissue, muted olive and yellow-green matching the intestine boss, occasional mauve mucosa; yellow eggs, dark plum outlines. Four cards in reading order:
I-01 "갈고리 유충" — low crescent segmented pale larva, olive back with two prominent hooked mouth tusks; no humanoid body. Wind-up: coils, burrow ridge and a dotted short straight ground lane clearly visible. Attack: bursts forward along that fixed lane with open hooks toward the player's old position; no homing. Caption: "땅속 예고 → 직선 습격".
I-02 "점액 달팽이" — asymmetrical crawling little snail, tall translucent pale-green goblet-shaped mucus sac instead of shell, drooping skeptical eyes, pink underside. Wind-up: cheek and goblet swell with bright mucus. Attack: lobs ONE sticky droplet to the marked ground circle near the red player, leaving a small olive puddle; visibly arcing projectile. Caption: "곡사 점액 → 둔화 웅덩이".
I-03 "융모 채찍벌레" — short rooted mauve intestinal nub with three long cream villus-like arms fanning from its top, tiny face at base, wide rather than round silhouette. Wind-up: all arms lean to one side, amber crescent ground cue. Attack: sweeps just a short 120-degree fan ahead, illustrated as a clean single crescent, with safe space behind. Caption: "몸 기울임 → 부채꼴 휩쓸기".
I-04 "충란 운반충" — squat four-legged olive beetle-like organism with TWO oversized pale-yellow translucent eggs on its back, anxious eyes, no armor. Wind-up: stops, eggs stretch upward and wobble. Attack: drops exactly TWO very small cream larvae close beside it, leaving the visibly empty sagging back sac. Caption: "알주머니 팽창 → 유충 2마리".
Keep each an ordinary small fodder enemy, with low-complexity anatomy an animator can reproduce. All four very different silhouettes.
```

## 간 — liver

참조 이미지:
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/대식세포IiDLE 1.png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/NK세포 이동 (2).png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/Player/Warrior/대기/전사형 IDEL.png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/middle boss/간/대기/정면 IDLE.png`
- `/Users/kjh/.codex/generated_images/01a093cd-a6b4-7760-8b3a-9d8ea4aaf755/exec-efd8c406-0d9a-49f1-9fe3-ac27e0270010.png`

### 최종 생성 프롬프트

```text
Use case: stylized-concept.
Asset type: Necrocis biome-exclusive ordinary-enemy art and combat reference board.
Input images: The first two supplied images are EXISTING small common-enemy sprite strips, third is the tiny red player sprite strip, and fourth is this biome's EXISTING BOSS sprite strip. They are style, scale-language and palette references ONLY. Make NEW common enemies, not copies of bosses. Match the actual simple, charming-but-uncanny biological cartoon pixel art: chunky deliberate pixels, dark plum 1-pixel outlines at source scale, flat 3-tone shading, expressive simple eyes, soft membranous silhouettes. Ordinary enemies with implied 24–48-pixel base sprites enlarged with crisp nearest-neighbor-like pixel edges; no realistic anatomical gore, no painterly rendering, no 3D, no detailed fantasy armor, no generic skeletons/zombies, no humans. It is a Unity 2.5D game with upright 2D billboard sprites over a top-down ground plane; show characters from frontal/slight three-quarter view with their lower contact point clear, never a side-scrolling scene.
Create one finished landscape art-director reference sheet, 2048x1536 if possible, consistent clean 2 by 2 card layout, exactly FOUR distinct enemy concepts. Warm off-white short Korean labels, restrained biome accent lines on flat deep plum charcoal background. Large board title top left, subtitle directly below. Every card: bold ID and Korean name, one large complete isolated IDLE sprite at left, then two smaller complete poses at right showing wind-up and attack outcome, separated by one clear arrow. Short labels above poses exactly "기본", "예고", "공격". Add the one short Korean attack caption given below at the bottom of each card. Leave generous padding and crisp readable text. Each attack picture must include a very small simplified red player target for spatial clarity; amber dotted floor outlines are wind-up warnings, solid colored effects are actual attacks. Avoid excessive particles and overlapping visual clutter. Keep enemy silhouettes different and color readable against backdrop. Make attack organs change visibly during wind-up. This is a concept board, not a packed animation atlas, not an in-game screenshot. No watermarks, fake UI, paragraphs, pseudo code, tiny annotation text or extra enemies.

Title: "NECROCIS / 간 전용 잔몹"
Subtitle: "응고 · 담즙 · 혈액 · 재생"
Palette: violet, indigo and pale lavender membranes matching the liver boss; crimson blood accents and small yellow bile highlights. Four cards:
H-01 "응고 방패충" — broad low crab-like clot creature, a crescent violet fibrin plate covering only its FRONT, two short legs and a small pale soft exposed rear belly. Wind-up: raises the frontal plate and leans forward, dotted short lane. Attack: a single short forward shield bash, a small impact wedge, the exposed back still readable. Caption: "정면 방어 → 짧은 밀치기".
H-02 "담즙 물집충" — pear-shaped purple hopping creature with one large translucent mustard-yellow bile blister and a small protruding nozzle mouth, uneven eyes. Wind-up: yellow bladder inflates and mouth tilts up; one marked target circle. Attack: lobs ONE yellow droplet on an arc, creates a single small yellow hazard pool, limited splash. Caption: "담즙 팽창 → 착탄 웅덩이".
H-03 "혈전 도약체" — compact dark crimson knot of three round blood cells, held by lavender threads, two tiny frog feet and a single eye; compact triangular silhouette. Wind-up: squashes low, ONE dotted landing circle at player's previous position. Attack: leaps to that circle, lands with a short ring of exactly SIX red droplets and becomes flattened; show an arc indicating airborne travel. Caption: "착지점 예고 → 도약 혈액탄".
H-04 "재생 성상충" — flat five-armed lilac star-shaped cell with pale core and thin vascular feelers, expressive central face. Wind-up: core brightens and ONE thin crimson organic tether extends to ONE small generic purple ally. Attack: stationary regeneration pulse traveling through that tether toward ally, red player approaching the exposed healer from the side; communicate one tether, interruptible. Caption: "혈관 연결 → 아군 1체 회복".
The healer must be distinct from a wizard: no staff or robe, biological star-cell only.

Image 5 is the previously generated intestine reference board from this same set. Match its 2x2 card layout, background, typography sizes, sprite scale, pixel treatment and diagram conventions closely, but replace ALL its enemy designs, palette accents, IDs and labels with the requested biome designs above. The four actual sprite strips remain the biological style source.
```

## 위 — stomach

참조 이미지:
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/대식세포IiDLE 1.png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/NK세포 이동 (2).png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/Player/Warrior/대기/전사형 IDEL.png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/middle boss/위/대기/정면 idle.png`
- `/Users/kjh/.codex/generated_images/01a093cd-a6b4-7760-8b3a-9d8ea4aaf755/exec-efd8c406-0d9a-49f1-9fe3-ac27e0270010.png`

### 최종 생성 프롬프트

```text
Use case: stylized-concept.
Asset type: Necrocis biome-exclusive ordinary-enemy art and combat reference board.
Input images: The first two supplied images are EXISTING small common-enemy sprite strips, third is the tiny red player sprite strip, and fourth is this biome's EXISTING BOSS sprite strip. They are style, scale-language and palette references ONLY. Make NEW common enemies, not copies of bosses. Match the actual simple, charming-but-uncanny biological cartoon pixel art: chunky deliberate pixels, dark plum 1-pixel outlines at source scale, flat 3-tone shading, expressive simple eyes, soft membranous silhouettes. Ordinary enemies with implied 24–48-pixel base sprites enlarged with crisp nearest-neighbor-like pixel edges; no realistic anatomical gore, no painterly rendering, no 3D, no detailed fantasy armor, no generic skeletons/zombies, no humans. It is a Unity 2.5D game with upright 2D billboard sprites over a top-down ground plane; show characters from frontal/slight three-quarter view with their lower contact point clear, never a side-scrolling scene.
Create one finished landscape art-director reference sheet, 2048x1536 if possible, consistent clean 2 by 2 card layout, exactly FOUR distinct enemy concepts. Warm off-white short Korean labels, restrained biome accent lines on flat deep plum charcoal background. Large board title top left, subtitle directly below. Every card: bold ID and Korean name, one large complete isolated IDLE sprite at left, then two smaller complete poses at right showing wind-up and attack outcome, separated by one clear arrow. Short labels above poses exactly "기본", "예고", "공격". Add the one short Korean attack caption given below at the bottom of each card. Leave generous padding and crisp readable text. Each attack picture must include a very small simplified red player target for spatial clarity; amber dotted floor outlines are wind-up warnings, solid colored effects are actual attacks. Avoid excessive particles and overlapping visual clutter. Keep enemy silhouettes different and color readable against backdrop. Make attack organs change visibly during wind-up. This is a concept board, not a packed animation atlas, not an in-game screenshot. No watermarks, fake UI, paragraphs, pseudo code, tiny annotation text or extra enemies.

Title: "NECROCIS / 위 전용 잔몹"
Subtitle: "산성 · 소화 · 압착 · 흡입"
Palette: salmon pink, dusty rose and maroon matching the stomach boss, butter-yellow stomach acid, ivory teeth; bright acid only in attack organs. Four cards:
S-01 "산낭 두꺼비" — low broad salmon-pink frog organ with two large inflated cheek sacs, tiny back legs and a wide valve mouth, its cheeks translucent yellow inside. Wind-up: cheeks expand asymmetrically, body stays still. Attack: fires exactly THREE separated yellow acid pellets in a short forward fan toward the little red player; visible gaps between shots, no endless stream. Caption: "볼주머니 팽창 → 산성 3갈래탄".
S-02 "위석 굴림충" — squat rounded bezoar made of muted tan food fibers bound in rose tissue, one eye slit and little folding pink feet, no rocky fantasy golem armor. Wind-up: tucks feet in and leans back, a dotted straight lane. Attack: rolls quickly down that single locked line with two crisp motion echoes and rebounds into a dizzy wobble. Caption: "몸 말기 → 직선 굴러가기".
S-03 "유문 집게" — upright tiny rose muscle ring creature with two opposing ivory ridges like clamp jaws, stubby paired feet and one suspicious eye; horseshoe silhouette. Wind-up: opens opposing jaws WIDE and marks a narrow wedge immediately ahead. Attack: jaws clap shut on a short forward bite; small clear impact, player safely to side. Caption: "집게 벌림 → 전방 압착".
S-04 "흡입 깔때기충" — stationary flared pink funnel stomach tube on three fleshy anchoring roots, one eye in side membrane, very broad open rim and narrow base, yellow inner cavity. Wind-up: rim expands and an amber short cone appears. Attack: draws sparse air streaks and the red player's small movement arrow gently TOWARD its mouth, visibly INWARD arrows, then a closed contracted tired pose indicated by sagging rim. Caption: "입구 확장 → 짧은 흡입".
Keep the creatures humorous and unsettling like the grinning pink stomach boss, not grotesque realistic organ dissection.

Image 5 is the previously generated intestine reference board from this same set. Match its 2x2 card layout, background, typography sizes, sprite scale, pixel treatment and diagram conventions closely, but replace ALL its enemy designs, palette accents, IDs and labels with the requested biome designs above. The four actual sprite strips remain the biological style source.
```

## 폐 — lung

참조 이미지:
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/대식세포IiDLE 1.png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/NK세포 이동 (2).png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/Player/Warrior/대기/전사형 IDEL.png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/middle boss/폐 보스 정면 idle.png`
- `/Users/kjh/.codex/generated_images/01a093cd-a6b4-7760-8b3a-9d8ea4aaf755/exec-efd8c406-0d9a-49f1-9fe3-ac27e0270010.png`

### 최종 생성 프롬프트

```text
Use case: stylized-concept.
Asset type: Necrocis biome-exclusive ordinary-enemy art and combat reference board.
Input images: The first two supplied images are EXISTING small common-enemy sprite strips, third is the tiny red player sprite strip, and fourth is this biome's EXISTING BOSS sprite strip. They are style, scale-language and palette references ONLY. Make NEW common enemies, not copies of bosses. Match the actual simple, charming-but-uncanny biological cartoon pixel art: chunky deliberate pixels, dark plum 1-pixel outlines at source scale, flat 3-tone shading, expressive simple eyes, soft membranous silhouettes. Ordinary enemies with implied 24–48-pixel base sprites enlarged with crisp nearest-neighbor-like pixel edges; no realistic anatomical gore, no painterly rendering, no 3D, no detailed fantasy armor, no generic skeletons/zombies, no humans. It is a Unity 2.5D game with upright 2D billboard sprites over a top-down ground plane; show characters from frontal/slight three-quarter view with their lower contact point clear, never a side-scrolling scene.
Create one finished landscape art-director reference sheet, 2048x1536 if possible, consistent clean 2 by 2 card layout, exactly FOUR distinct enemy concepts. Warm off-white short Korean labels, restrained biome accent lines on flat deep plum charcoal background. Large board title top left, subtitle directly below. Every card: bold ID and Korean name, one large complete isolated IDLE sprite at left, then two smaller complete poses at right showing wind-up and attack outcome, separated by one clear arrow. Short labels above poses exactly "기본", "예고", "공격". Add the one short Korean attack caption given below at the bottom of each card. Leave generous padding and crisp readable text. Each attack picture must include a very small simplified red player target for spatial clarity; amber dotted floor outlines are wind-up warnings, solid colored effects are actual attacks. Avoid excessive particles and overlapping visual clutter. Keep enemy silhouettes different and color readable against backdrop. Make attack organs change visibly during wind-up. This is a concept board, not a packed animation atlas, not an in-game screenshot. No watermarks, fake UI, paragraphs, pseudo code, tiny annotation text or extra enemies.

Title: "NECROCIS / 폐 전용 잔몹"
Subtitle: "호흡 · 기류 · 활공 · 잔류 가스"
Palette: warm honey yellow, ochre and pale cream matching the paired yellow lung boss; muted teal gray only in gas/VFX, pale blue wind highlights, dark plum outlines. Do NOT turn the entire biome roster blue.
Four cards:
P-01 "폐포 풀무충" — low yellow organism with two inflated bellows-like alveolar lobes and one very short tracheal nozzle, two small feet, single comically strained face. Wind-up: BOTH sacs puff up and a short dotted cone on the floor. Attack: one clear SHORT cone of pale blue air blowing OUTWARD at the red target, target movement arrow away from creature. Caption: "폐포 팽창 → 부채꼴 밀치기".
P-02 "기관지 쌍분사충" — slender upright Y-shaped honey-yellow bronchial creature, two unequal nozzle heads, narrow segmented stem and two root feet; eyes on outside of each nozzle. Wind-up: both heads tilt in the same locked aim direction and puff out. Attack: two offset volleys of three small pale-teal gas pellets shown as separate staggered fans with visible dodge gaps, no beam. Caption: "분사구 조준 → 시차 가스탄".
P-03 "흉막 활공충" — small golden flattened kite-shaped membrane with two wing flaps, pink seam, dangling thin curled tail and two close-set eyes; nothing like a real bird. Wind-up: lifts just above ground, clearly visible shadow and ONE dotted landing circle. Attack: short diagonal dive to player's previous location, a curved travel arrow, a small landing puff then folded exhausted membrane. Caption: "그림자 예고 → 비스듬히 급강하".
P-04 "매연 포낭충" — squat mustard-yellow wobbling sac with charcoal-gray deposits visible inside, three tiny cilia feet, tight worried mouth; largest organ is one soot-filled bubble. Wind-up: sac darkens and swells, small dotted ground circle underneath. Attack: releases ONE localized low teal-gray gas puddle while the empty wrinkled sack hops backward; open floor outside patch clearly readable, gas never opaque enough to hide player. Caption: "포낭 팽창 → 잔류 가스".
Keep existing yellow lung boss's cute organ logic, no realistic respiratory illustration, no mechanical gas masks.

Image 5 is the previously generated intestine reference board from this same set. Match its 2x2 card layout, background, typography sizes, sprite scale, pixel treatment and diagram conventions closely, but replace ALL its enemy designs, palette accents, IDs and labels with the requested biome designs above. The four actual sprite strips remain the biological style source.
```


## 위 시트 최종 수정

최초 생성본에서 S-04 흡입 방향 화살표 하나가 바깥쪽을 향해, 아래 프롬프트로 수정했다. 프로젝트에 보관하는 Stomach_Enemies_v1.png는 이 수정본이다.

수정 입력: `/Users/kjh/.codex/generated_images/01a093cd-a6b4-7760-8b3a-9d8ea4aaf755/exec-b21fac30-d37e-4c67-a4d7-3bd2f97a1158.png`

```text
Use case: precise-object-edit.
Edit the supplied NECROCIS stomach four-enemy reference board. Change ONLY the airflow arrows inside the LOWER-RIGHT S-04 "흡입 깔때기충" card's rightmost "공격" vignette. This enemy INHALES: every white airflow arrow must travel from the open space on the LEFT toward the creature's open funnel mouth on the RIGHT. In particular REMOVE the erroneous white arrow pointing down-left away from the mouth. Show two clear white arrows pointing RIGHT into the dark yellow mouth: one above the small red player and one level with/just above the player, both with arrowheads immediately to the LEFT of the mouth, pointing INTO it. Keep the small red player and the funnel creature exactly as they are. Use no outward-pointing airflow arrow. Preserve absolutely everything else: canvas size, all text, title, Korean captions, all four monsters, all colors, all frames, all dots, the three other cards, sprite scale and pixel style. Do not redesign, add or remove any other element.
```
