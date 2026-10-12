# Modern Life v2 — 실제 이미지 생성 프롬프트

생성 방식: 내장 `image_gen`. 기존 일반몹 2종과 플레이어만 스타일 참조로 사용했다. 보스 스프라이트 및 거절된 v1 시안은 생성 입력에 포함하지 않았다. 이후 시트에는 이번 장 시트를 레이아웃 참조로 추가했다.

## 장

출력 파일: `Intestine_Normal_v2.png`

입력 참조:
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/대식세포IiDLE 1.png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/NK세포 이동 (2).png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/Player/Warrior/대기/전사형 IDEL.png`

```text
Use case: stylized-concept.
Asset type: NEW Necrocis 2D PIXEL ART ordinary-enemy visual reference and attack storyboard, modern-life body interior theme.
Reference images 1 and 2 are the existing game's tiny common-enemy sprite strips; reference 3 is the red player sprite strip. Use them ONLY for small game-sprite simplicity, dark single-pixel contour, basic face marks, palette economy and player style. Design ALL enemies anew from the material motifs described below. None of these are bosses or elites.
STYLE IS CRITICAL: authentic low-resolution hand-placed sprite pixel art, the bodies plausibly designed on 32x32 to 48x48 logical pixel canvases, enlarged with crisp square pixel clusters. ONE dark outline, 3 stepped shading values per material, approximately 8–12 colors per enemy. Flat matte clusters, very few highlight pixels. Simple enough to animate. Do not generate high-resolution painting with pixel noise or shiny volumetric render. No smooth gradients, antialiasing, soft bloom, textured brushwork, intricate pores or hundreds of tiny details. Use tiny dot/slit eyes only where appropriate; no huge grinning teeth or mascot-like humanoid faces. Diverse material-based silhouettes, no repeated round blob anatomy. Slight top-down/front three-quarter upright sprite view suitable for 2.5D billboard gameplay, with a small clear ground shadow and stable bottom-center contact point. Non-graphic, slightly uncanny biological cartoon style.
LAYOUT: one wide landscape sheet, 2048x1536 if available. Flat deep charcoal #202027 background, fine muted gray row dividers, warm cream readable Korean labels. Exactly THREE horizontal rows, one named ordinary enemy per row, generous clear margins. A title and brief subtitle at the top. Each row has FOUR well-separated columns with exact headers "기본", "예고", "공격", "빈틈". The first column is the largest idle design, followed by wind-up, actual attack, then recovery. Show the SAME identifiable creature through the four poses, not four different enemies. Keep its scale consistent across poses except deliberate squash/stretch. Name and ID at upper-left of each row. Small arrows connect action stages. One short Korean caption below each row, exactly as specified. In attack/telegraph panels, show a tiny red player for spatial clarity where useful, small enough not to dominate. Warm amber dotted outline means preview area only; solid white/colored action accents mean actual movement/attack. Arrows must match attack direction and not overlap labels.
Prohibited: no organ-shaped boss copies, no green egg-carrying toad, no purple caped liver creature, no pink grinning stomach man, no paired yellow Y-shaped lung heads, no medieval weapons, crowns, armor, horns, clothing, limbs borrowed from a humanoid, no giant boss health bars. No decorated gothic frames or noisy floor scenery. No existing v1 concept sheet designs. This is a clean practical pixel-sprite reference sheet, not a clinical diagram, not an actual sliced animation atlas. Do not add extra named monsters, UI, watermarks or long paragraphs.

Title exactly: "NECROCIS / 장 일반몹"
Subtitle exactly: "가스 · 건조 잔여물 · 경련"
ROW 1 name: "I-01 가스낭"
Material/design: a thin pale sage translucent membrane shaped like an OFF-CENTER PEAR / asymmetric gas bubble, one large inner gas void with just two pale highlight blocks, a tiny pinched mouth on the lower-left and two little anchor nubs. NOT a frog, snail, larva or egg carrier. Face is only two small strained dots/slits. Idle is gently tilted and underinflated; wind-up visibly swells to a taut pale sphere over a small dotted circular danger radius; attack expels one short radial GAS BURST as a clean broken ring from rupturing membrane seam, with the red player just outside the circle; recovery is the same deflated crumpled little membrane and intact nucleus at the same position, no hazard lingering. This is a reusable short gas burst, not a death explosion.
Caption exactly: "팽창 → 짧은 가스 폭발 → 쭈그러짐"
ROW 2 name: "I-02 굳은 잔여체"
Material/design: a LOW ANGULAR irregular compacted dry waste/food-residue chunk, ochre and dusty brown, three large pale cracks and tiny blunt underside pegs. Almost block-like uneven trapezoid silhouette, no shell, tusks, round armored ball or realistic feces. Tiny face notch in one crack. Idle squat and heavy; wind-up tilts its entire flat front back, small short rectangular ground cue; attack pushes its low mass forward ONLY A SHORT DISTANCE, one stout white rightward arrow, little red player moving aside; recovery is leaning too far forward, soft inner seam visibly open between the chunks. Does not spin or roll.
Caption: "몸 기울임 → 짧게 밀치기 → 갈라진 틈"
ROW 3 name: "I-03 경련 마디"
Material/design: a SHORT ACCORDION of three pink-red muscle bands, lean horizontal zigzag silhouette with pointed underside pads, one small dark nerve dot at one end, no worm face or teeth. Idle tiny contraction twitch; wind-up compresses all three bands into a visibly short spring and shows a straight dashed lane; attack extends sharply into ONE straight springing lunge at player's previous position; recovery is slack drooped accordion tissue, no second strike. This looks like living muscle material, not an entire intestine.
Caption: "마디 수축 → 한 번 튀기 → 늘어져 멈춤"
```

## 간

출력 파일: `Liver_Normal_v2.png`

입력 참조:
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/대식세포IiDLE 1.png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/NK세포 이동 (2).png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/Player/Warrior/대기/전사형 IDEL.png`
- `/Users/kjh/.codex/generated_images/01a093cd-a6b4-7760-8b3a-9d8ea4aaf755/exec-d20b5861-68f4-498b-b2a0-06cf36b0e4e8.png`

```text
Use case: stylized-concept.
Asset type: NEW Necrocis 2D PIXEL ART ordinary-enemy visual reference and attack storyboard, modern-life body interior theme.
Reference images 1 and 2 are the existing game's tiny common-enemy sprite strips; reference 3 is the red player sprite strip. Use them ONLY for small game-sprite simplicity, dark single-pixel contour, basic face marks, palette economy and player style. Design ALL enemies anew from the material motifs described below. None of these are bosses or elites.
STYLE IS CRITICAL: authentic low-resolution hand-placed sprite pixel art, the bodies plausibly designed on 32x32 to 48x48 logical pixel canvases, enlarged with crisp square pixel clusters. ONE dark outline, 3 stepped shading values per material, approximately 8–12 colors per enemy. Flat matte clusters, very few highlight pixels. Simple enough to animate. Do not generate high-resolution painting with pixel noise or shiny volumetric render. No smooth gradients, antialiasing, soft bloom, textured brushwork, intricate pores or hundreds of tiny details. Use tiny dot/slit eyes only where appropriate; no huge grinning teeth or mascot-like humanoid faces. Diverse material-based silhouettes, no repeated round blob anatomy. Slight top-down/front three-quarter upright sprite view suitable for 2.5D billboard gameplay, with a small clear ground shadow and stable bottom-center contact point. Non-graphic, slightly uncanny biological cartoon style.
LAYOUT: one wide landscape sheet, 2048x1536 if available. Flat deep charcoal #202027 background, fine muted gray row dividers, warm cream readable Korean labels. Exactly THREE horizontal rows, one named ordinary enemy per row, generous clear margins. A title and brief subtitle at the top. Each row has FOUR well-separated columns with exact headers "기본", "예고", "공격", "빈틈". The first column is the largest idle design, followed by wind-up, actual attack, then recovery. Show the SAME identifiable creature through the four poses, not four different enemies. Keep its scale consistent across poses except deliberate squash/stretch. Name and ID at upper-left of each row. Small arrows connect action stages. One short Korean caption below each row, exactly as specified. In attack/telegraph panels, show a tiny red player for spatial clarity where useful, small enough not to dominate. Warm amber dotted outline means preview area only; solid white/colored action accents mean actual movement/attack. Arrows must match attack direction and not overlap labels.
Prohibited: no organ-shaped boss copies, no green egg-carrying toad, no purple caped liver creature, no pink grinning stomach man, no paired yellow Y-shaped lung heads, no medieval weapons, crowns, armor, horns, clothing, limbs borrowed from a humanoid, no giant boss health bars. No decorated gothic frames or noisy floor scenery. No existing v1 concept sheet designs. This is a clean practical pixel-sprite reference sheet, not a clinical diagram, not an actual sliced animation atlas. Do not add extra named monsters, UI, watermarks or long paragraphs.

Title exactly: "NECROCIS / 간 일반몹"
Subtitle exactly: "지방 · 염증 · 숙취의 감각"
ROW 1 name: "H-01 지방 소낭"
Material/design: a translucent cream cell membrane holding ONE large lopsided butter-yellow fat droplet which displaces a small mauve nucleus into a side corner. Asymmetric hanging bean/teardrop silhouette and a tiny flat crawling underside, no liver-shaped cape or armor. One sleepy slit near nucleus. Idle leans under the weight of fat; wind-up leans back visibly, fat shifts to rear; attack swings the weighted body forward in a SHORT BODY BUMP with a small front wedge, never a rolling ball; recovery droops onto its side with the displaced nucleus more visible, no armor mechanics.
Caption exactly: "무게 싣기 → 몸통 박치기 → 옆으로 처짐"
ROW 2 name: "H-02 염증 불씨"
Material/design: a small jagged crimson cell with only FIVE short asymmetrical pointed protrusions and a central hot coral nucleus. Lean forward-pointed COMET-like silhouette, tiny dot-eye, not literal fire elemental or humanoid. Strict stepped red/salmon palette, only a few pale alert pixels. Idle restless; wind-up nucleus brightens and its rear spines fold backward, short locked direction line; attack a single QUICK NEEDLE-LIKE JAB forward with its pointed cell edge; recovery all tips droop pale and still. No fire pool, projectile or burst.
Caption: "핵 점등 → 빠른 찌르기 → 돌기 처짐"
ROW 3 name: "H-03 숙취 잔재"
Material/design: the unpleasant sensation of a hangover represented as a squat dusky mauve-gray liquid blob with an offset swollen trailing lobe, a slanted weary face of two thin marks and one tiny opening. Two lopsided lobes drag out of phase, horizontal rather than tall spherical silhouette. No alcohol bottle, glass or humanoid drunk. Idle off-balance liquid sway; wind-up stops, trailing liquid catches up and bunches toward its single mouth opening; attack SPITS EXACTLY ONE dark red-violet droplet in a simple straight path toward the tiny red player; recovery the emptied trailing lobe slumps and flattens. No second shot or lingering hazard.
Caption: "출렁이다 정지 → 액체 한 발 → 납작해짐"
Keep H-03 an exploratory simple ordinary enemy, as readable as the first two, not a mini boss.

Reference image 4 is the newly generated intestine board from THIS modern-life set. Match its clean three horizontal rows, four action columns, label sizes, thin dividers, charcoal backdrop and low-resolution pixel scale. It is a layout reference ONLY. Replace all creature designs, names and captions with those required for this board. Do not copy its gas sac, brown chunk or muscle design.
```

## 위

출력 파일: `Stomach_Normal_v2.png`

입력 참조:
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/대식세포IiDLE 1.png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/NK세포 이동 (2).png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/Player/Warrior/대기/전사형 IDEL.png`
- `/Users/kjh/.codex/generated_images/01a093cd-a6b4-7760-8b3a-9d8ea4aaf755/exec-d20b5861-68f4-498b-b2a0-06cf36b0e4e8.png`

```text
Use case: stylized-concept.
Asset type: NEW Necrocis 2D PIXEL ART ordinary-enemy visual reference and attack storyboard, modern-life body interior theme.
Reference images 1 and 2 are the existing game's tiny common-enemy sprite strips; reference 3 is the red player sprite strip. Use them ONLY for small game-sprite simplicity, dark single-pixel contour, basic face marks, palette economy and player style. Design ALL enemies anew from the material motifs described below. None of these are bosses or elites.
STYLE IS CRITICAL: authentic low-resolution hand-placed sprite pixel art, the bodies plausibly designed on 32x32 to 48x48 logical pixel canvases, enlarged with crisp square pixel clusters. ONE dark outline, 3 stepped shading values per material, approximately 8–12 colors per enemy. Flat matte clusters, very few highlight pixels. Simple enough to animate. Do not generate high-resolution painting with pixel noise or shiny volumetric render. No smooth gradients, antialiasing, soft bloom, textured brushwork, intricate pores or hundreds of tiny details. Use tiny dot/slit eyes only where appropriate; no huge grinning teeth or mascot-like humanoid faces. Diverse material-based silhouettes, no repeated round blob anatomy. Slight top-down/front three-quarter upright sprite view suitable for 2.5D billboard gameplay, with a small clear ground shadow and stable bottom-center contact point. Non-graphic, slightly uncanny biological cartoon style.
LAYOUT: one wide landscape sheet, 2048x1536 if available. Flat deep charcoal #202027 background, fine muted gray row dividers, warm cream readable Korean labels. Exactly THREE horizontal rows, one named ordinary enemy per row, generous clear margins. A title and brief subtitle at the top. Each row has FOUR well-separated columns with exact headers "기본", "예고", "공격", "빈틈". The first column is the largest idle design, followed by wind-up, actual attack, then recovery. Show the SAME identifiable creature through the four poses, not four different enemies. Keep its scale consistent across poses except deliberate squash/stretch. Name and ID at upper-left of each row. Small arrows connect action stages. One short Korean caption below each row, exactly as specified. In attack/telegraph panels, show a tiny red player for spatial clarity where useful, small enough not to dominate. Warm amber dotted outline means preview area only; solid white/colored action accents mean actual movement/attack. Arrows must match attack direction and not overlap labels.
Prohibited: no organ-shaped boss copies, no green egg-carrying toad, no purple caped liver creature, no pink grinning stomach man, no paired yellow Y-shaped lung heads, no medieval weapons, crowns, armor, horns, clothing, limbs borrowed from a humanoid, no giant boss health bars. No decorated gothic frames or noisy floor scenery. No existing v1 concept sheet designs. This is a clean practical pixel-sprite reference sheet, not a clinical diagram, not an actual sliced animation atlas. Do not add extra named monsters, UI, watermarks or long paragraphs.

Title exactly: "NECROCIS / 위 일반몹"
Subtitle exactly: "나선균 · 역류 · 기름막"
ROW 1 name: "S-01 헬리코 나선충"
Material/design: a SHORT slender corkscrew-shaped muted rose/lavender bacterium with two or three helical turns and two thin tail filaments. Narrow diagonal screw silhouette. At most one tiny eye dot, no large mouth, teeth, legs or organ body. Idle tail wiggles with one small clear mucus patch underneath; wind-up visibly twists/compresses like a screw on a dotted straight path; attack drills along ONE locked short straight line, no homing and no circular area; recovery helix is stretched and momentarily stopped, tail slack.
Caption exactly: "나선 비틀기 → 직선 파고들기 → 꼬리 처짐"
ROW 2 name: "S-02 역류 방울"
Material/design: a pale lime-amber acid drop in a thin dusty-pink membrane, bottom-heavy body with a single long narrow upward liquid neck like a bent droplet, not a stomach man or toothed mouth. Tiny face notches low on heavy base. Idle liquid rests at bottom; wind-up liquid visibly climbs into the thin top neck and a small target circle is marked; attack lobs EXACTLY ONE acid glob in a CURVED ARC into that ground circle, with small immediate splash; recovery neck collapses down onto base. No returning projectile, no persistent pool.
Caption: "액체 끌어올림 → 곡사 한 발 → 목 부분 처짐"
ROW 3 name: "S-03 기름막 활주체"
Material/design: a very FLAT uneven golden-ochre oily membrane sheet, broad thin triangular/ribbon silhouette, one curled edge and a tiny dark nucleus on the fold. Two sparse blue-gray highlight pixels can suggest an oily sheen, no smooth rainbow gradient. NO legs, animal head or face mask. Idle folded edge creeps; wind-up flattens and pulls into a pointed forward sheet on a dashed straight lane; attack slides along ONE longer locked ground line with two thin clean trailing streaks, never airborne; recovery crumples into a little accordion fold at lane end. No sustained oil puddle or player slipping effect.
Caption: "납작해짐 → 바닥 활주 → 가장자리 접힘"

Reference image 4 is the newly generated intestine board from THIS modern-life set. Match its clean three horizontal rows, four action columns, label sizes, thin dividers, charcoal backdrop and low-resolution pixel scale. It is a layout reference ONLY. Replace all creature designs, names and captions with those required for this board. Do not copy its gas sac, brown chunk or muscle design.
```

## 폐

출력 파일: `Lung_Normal_v2.png`

입력 참조:
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/대식세포IiDLE 1.png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/enemy/NK세포 이동 (2).png`
- `/Users/kjh/local_dev/Necrocis/Assets/_Project/Art/Images/Player/Warrior/대기/전사형 IDEL.png`
- `/Users/kjh/.codex/generated_images/01a093cd-a6b4-7760-8b3a-9d8ea4aaf755/exec-d20b5861-68f4-498b-b2a0-06cf36b0e4e8.png`

```text
Use case: stylized-concept.
Asset type: NEW Necrocis 2D PIXEL ART ordinary-enemy visual reference and attack storyboard, modern-life body interior theme.
Reference images 1 and 2 are the existing game's tiny common-enemy sprite strips; reference 3 is the red player sprite strip. Use them ONLY for small game-sprite simplicity, dark single-pixel contour, basic face marks, palette economy and player style. Design ALL enemies anew from the material motifs described below. None of these are bosses or elites.
STYLE IS CRITICAL: authentic low-resolution hand-placed sprite pixel art, the bodies plausibly designed on 32x32 to 48x48 logical pixel canvases, enlarged with crisp square pixel clusters. ONE dark outline, 3 stepped shading values per material, approximately 8–12 colors per enemy. Flat matte clusters, very few highlight pixels. Simple enough to animate. Do not generate high-resolution painting with pixel noise or shiny volumetric render. No smooth gradients, antialiasing, soft bloom, textured brushwork, intricate pores or hundreds of tiny details. Use tiny dot/slit eyes only where appropriate; no huge grinning teeth or mascot-like humanoid faces. Diverse material-based silhouettes, no repeated round blob anatomy. Slight top-down/front three-quarter upright sprite view suitable for 2.5D billboard gameplay, with a small clear ground shadow and stable bottom-center contact point. Non-graphic, slightly uncanny biological cartoon style.
LAYOUT: one wide landscape sheet, 2048x1536 if available. Flat deep charcoal #202027 background, fine muted gray row dividers, warm cream readable Korean labels. Exactly THREE horizontal rows, one named ordinary enemy per row, generous clear margins. A title and brief subtitle at the top. Each row has FOUR well-separated columns with exact headers "기본", "예고", "공격", "빈틈". The first column is the largest idle design, followed by wind-up, actual attack, then recovery. Show the SAME identifiable creature through the four poses, not four different enemies. Keep its scale consistent across poses except deliberate squash/stretch. Name and ID at upper-left of each row. Small arrows connect action stages. One short Korean caption below each row, exactly as specified. In attack/telegraph panels, show a tiny red player for spatial clarity where useful, small enough not to dominate. Warm amber dotted outline means preview area only; solid white/colored action accents mean actual movement/attack. Arrows must match attack direction and not overlap labels.
Prohibited: no organ-shaped boss copies, no green egg-carrying toad, no purple caped liver creature, no pink grinning stomach man, no paired yellow Y-shaped lung heads, no medieval weapons, crowns, armor, horns, clothing, limbs borrowed from a humanoid, no giant boss health bars. No decorated gothic frames or noisy floor scenery. No existing v1 concept sheet designs. This is a clean practical pixel-sprite reference sheet, not a clinical diagram, not an actual sliced animation atlas. Do not add extra named monsters, UI, watermarks or long paragraphs.

Title exactly: "NECROCIS / 폐 일반몹"
Subtitle exactly: "분진 · 가래 · 꽃가루"
ROW 1 name: "P-01 분진 뭉치"
Material/design: a small dark slate/graphite irregular swarm of just 7–9 chunky square soot clumps around ONE tiny pale teal core. Asymmetrical ragged cloud silhouette with gaps, low hovering over a clearly attached ground shadow. No gas bag, frog face, yellow lung organs or paired heads. Idle particles drift loosely; wind-up clumps PACK TIGHT into one dense forward cluster, dashed short line; attack that packed cluster makes ONE short dart toward the player's old position; recovery dust disperses loosely around a clearly EXPOSED core that remains still and visible at ground-shadow position.
Caption exactly: "입자 응집 → 짧은 돌진 → 핵 노출"
ROW 2 name: "P-02 가래 점착체"
Material/design: a low STRETCHED viscous muted sage-yellow mucus ribbon with 2–3 gray dust specks embedded and two trailing torn cilia strands. Wide flat asymmetrical shape with one thicker front lump and two tiny half-closed eye marks. No snail shell or limbs. Idle stretches along ground; wind-up gathers mucus into its thicker front, a small ground target circle appears; attack spits ONE small mucus glob that becomes a SINGLE SMALL STICKY PUDDLE, player clearly moves around its edge; recovery body is thinned and stretched, same puddle remains only in attack scene. No grabbing or binding string and no damage cloud.
Caption: "점액 모으기 → 둔화 웅덩이 → 몸 얇아짐"
ROW 3 name: "P-03 꽃가루 침입자"
Material/design: a small OCHRE pollen grain shell, slightly triangular/hexagonal spiky silhouette with exactly six short blunt spikes and one simple equatorial seam. A dark pinpoint core visible through seam, no expressive lung face. Idle grain hovers just over ground shadow; wind-up shell splits OPEN on its center seam and displays a short forward fan cue; attack spits EXACTLY THREE small separated pollen pellets in a clear forward fan with generous dodge gaps, not radial fire; recovery opened shell closes and spikes tilt downward slightly.
Caption: "껍질 벌림 → 입자 3갈래 → 껍질 닫힘"
Make soot graphite, mucus sage-yellow, pollen ochre: visually distinct from each other and the existing boss colors.

Reference image 4 is the newly generated intestine board from THIS modern-life set. Match its clean three horizontal rows, four action columns, label sizes, thin dividers, charcoal backdrop and low-resolution pixel scale. It is a layout reference ONLY. Replace all creature designs, names and captions with those required for this board. Do not copy its gas sac, brown chunk or muscle design.
```


## 최종 검수 수정 프롬프트

최종 파일은 아래 수정이 반영된 이미지다. 간 시트는 최초 생성본을 사용한다.

### intestine

입력 이미지: `/Users/kjh/.codex/generated_images/01a093cd-a6b4-7760-8b3a-9d8ea4aaf755/exec-d20b5861-68f4-498b-b2a0-06cf36b0e4e8.png`

```text
Use case: precise-object-edit.
Edit only the TOP ROW I-01 "가스낭" attack-and-recovery depiction of the supplied intestine pixel reference sheet so it accurately communicates a SELF-CENTERED short gas burst. Preserve the entire lower two rows, all typography/captions/IDs, dividers, canvas dimensions and pixel treatment.
In the TOP ROW "공격" column, replace the current one-sided cloud with a clear small 360-degree gas-burst diagram CENTERED ON the sac: the small deflating body in the middle at its existing approximate position, one broken elliptical ring of 6–8 sparse pale sage gas puffs surrounding it on all sides, a few very short outward ticks. Gas ring must fit entirely within the attack column and never touch the column-to-column transition arrows. This is a close-range all-around burst, NOT a projectile, stream or rightward cone. Place the tiny red player to the right OUTSIDE the ring, preserving style.
In TOP ROW "빈틈" column, replace the current mostly full oval sac with the SAME creature clearly DEFLATED into a LOW WRINKLED membrane heap less than half its idle height, with its tiny face and intact nucleus still present and bottom contact aligned to the original baseline. This is alive and exhausted, not a corpse or a second full bubble.
Keep the first-row "기본" and "예고" sprites unchanged. Keep every Korean text character exactly unchanged. Same crisp low-resolution pixel clusters, limited palette, no added labels or new creatures.
```

### stomach

입력 이미지: `/Users/kjh/.codex/generated_images/01a093cd-a6b4-7760-8b3a-9d8ea4aaf755/exec-632a9d02-b2b3-4be0-a732-1bf73bbddf71.png`

```text
Use case: precise-object-edit.
Edit the supplied Necrocis stomach three-row pixel-art reference board. Make ONLY ONE correction: the facing direction of the bacterium in the TOP ROW S-01 "헬리코 나선충". In EACH of its four columns, horizontally mirror ONLY that bacterium's body and filaments about its own existing center, keeping the silhouette scale and position intact. The blunt head with tiny dark eye/nucleus MUST be at the RIGHT end, and both long trailing flagella MUST extend from the LEFT rear end. In the attack panel the bacterium must therefore drill HEAD FIRST toward the red player on its RIGHT, consistent with the already-rightward white arrow. Do not mirror the row, labels, arrows, player, dashes, backdrop or any other object. Preserve the row's idle/twist/lunge/slack-tail pose distinctions. Preserve all text, Korean captions, resolution, palette, chunky low-resolution pixel art, layout and the second/third rows exactly. No other design changes.
```

### lung

입력 이미지: `/Users/kjh/.codex/generated_images/01a093cd-a6b4-7760-8b3a-9d8ea4aaf755/exec-baa8f4bf-48a0-4aac-b5cb-07fcee6aeb59.png`

```text
Use case: precise-object-edit.
Correct the ATTACK STORYBOARD SPATIAL CLARITY on the supplied Necrocis lung three-row pixel reference sheet, with minimal localized edits.
1. In middle row P-02 "가래 점착체", the "예고" column currently places an amber dotted ellipse UNDER the mucus creature. Remove that under-body dotted ellipse and instead show ONE small amber dotted target ellipse on EMPTY GROUND to the RIGHT of that creature, within the same column, mirroring the relative landing location of the actual mucus puddle in the next "공격" column. Keep it small and unobstructed, not under the caster. All mucus creature sprites remain unchanged.
2. In bottom row P-03 "꽃가루 침입자", the "공격" column currently shows only three pellets and the red player. ADD the MISSING pollen creature at the LEFT side of that attack column (just right of its transition arrow), matching the already-designed ochre six-spike pollen shell, horizontally open seam and dark pinpoint core. It should be the same recognizable creature shown in the neighboring wind-up and recovery columns, at a slightly reduced size only if necessary to fit. Show EXACTLY THREE separated ochre pollen pellets traveling from its open seam toward the red player to the RIGHT, with short clearly rightward trails. Reposition those existing three pellets slightly to the right to make room if needed; do not add extra pellets or players. Keep the tiny player at the right edge.
Preserve everything else: entire first row, all titles/names/Korean captions/headers, pixel font, canvas size, background and dividers, all other sprites/poses, and crisp chunky low-resolution pixel style. No new text, no new types of monster, no new decoration.
```

## 최종 이미지 원본 기록

- `Intestine_Normal_v2.png` ← `/Users/kjh/.codex/generated_images/01a093cd-a6b4-7760-8b3a-9d8ea4aaf755/exec-cbd8d0a9-fb8a-44f8-ad46-7d97d6643f0e.png`
- `Liver_Normal_v2.png` ← `/Users/kjh/.codex/generated_images/01a093cd-a6b4-7760-8b3a-9d8ea4aaf755/exec-5728af84-795f-4829-9efb-2ec9a82eb587.png`
- `Stomach_Normal_v2.png` ← `/Users/kjh/.codex/generated_images/01a093cd-a6b4-7760-8b3a-9d8ea4aaf755/exec-de9ba24c-bdd8-432f-b5f8-2d18a4929394.png`
- `Lung_Normal_v2.png` ← `/Users/kjh/.codex/generated_images/01a093cd-a6b4-7760-8b3a-9d8ea4aaf755/exec-b629245f-7c4c-4415-9b95-df4dff78ae88.png`
