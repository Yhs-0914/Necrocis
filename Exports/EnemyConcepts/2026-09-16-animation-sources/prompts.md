# Biome enemy animation generation

Create a production sprite animation sheet for this EXACT referenced Necrocis monster. Preserve recognizable anatomy, pink flesh, burgundy pixel outline, large eyes, pixel-art style. This is now a SMALL NORMAL ENEMY, never a boss. Output a square image with EXACTLY 4 columns and EXACTLY 4 rows, 16 equally sized square cells on an invisible regular grid, no margins around the overall sheet. Each cell contains one full-body animation frame, with plenty of green space separating frames. Entire sheet background MUST be perfectly flat solid chroma green #00FF00, no shadows, no gradients, no text, no labels, no borders, no guide lines. Sprite itself must NOT contain green: use amber for acid. Each character stays same scale and same fixed center and ground baseline in its cell. Same front three-quarter top-down game camera facing slightly right/down in EVERY frame, no rotation to another direction. Crisp low-resolution pixel art approx 64x64 logical sprite enlarged into each cell. ROW 1: 4-frame idle breathing loop, subtly inflate, rise, exhale, settle. ROW 2: 4-frame locomotion loop with visible body squash/stretch and alternating tiny feet, continuous crawl/slither/hop appropriate to anatomy. ROW 3: 4-frame attack, anticipation, preparation, strike, recoil. ROW 4: 4-frame death, eyes close in pain and body recoils, collapse downward, melt/burst into small pink pieces, final low small puddle with NO intact standing body. Death row must progressively collapse while staying on same ground baseline. Never repeat identical frames; distinct readable poses; avoid weapon additions. Attack design: 

## 01_stomach_gulp
RANGED: inflate cheeks, open mouth to spit acid, recoil, settle. No detached projectile.

## 02_stomach_mucus_snail
MELEE: pull eye stalks back, stretch head forward for bite, strongest bite pose, retract.

## 03_stomach_overfed_sac
RANGED: swell belly, purse mouth, forcefully spit acid with recoil, settle. No detached projectile.

## 04_intestine_foldworm
MELEE: compress accordion segments, extend head for bite, maximum lunge, retract.

## 05_intestine_villus_hand
MELEE: bend tall stalk backward, whip head forward, maximum bent strike, return.

## 06_intestine_knotted_eel
MELEE: pull both heads apart, snap both heads inward, jaws biting, return.

## 07_liver_suture_cell
RANGED: wind thread tip back, thrust thread tip forward casting energy, recoil, return. No external shield bubble.

## 08_liver_clot
MELEE: brace scab shield, lean shield forward, shield bash, retract.

## 09_liver_coagulation_core
RANGED: pull three orbit plates inward, open plates revealing nucleus, pulse nucleus and recoil, settle. Keep three plates attached visually.

## 10_lung_breath_bubble
RANGED: inhale round body, open mouth, exhale with body compressed, settle. No detached bubbles.

## 11_lung_cilia_brush
MELEE: bend cilia back, sweep cilia forward, maximum sweep, return.

## 12_lung_overinflated_alveoli
RANGED: inflate top air sac, swell side sacs, central mouth exhales and sacs compress, settle.
