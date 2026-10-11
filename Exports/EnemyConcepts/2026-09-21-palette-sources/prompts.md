# Biome enemy palette edit — 2026-09-21

Mode: built-in image generation, one reference edit per enemy (12 requests).
Target: corresponding sheet from ../2026-09-16-animation-sources/.
Color references: original macrophage, B-cell and NK-cell sprites under Assets/_Project/Art/Images/enemy.

## Shared prompt

Use case: precise-object-edit. This is a STRICT PALETTE RECOLOR EDIT of an existing Unity pixel art animation sheet, NOT a redesign.
Input image 1: EDIT TARGET, a 4x4 sheet of one enemy's 16 animation frames.
Input image 2: existing macrophage STYLE/COLOR REFERENCE ONLY.
Input image 3: existing B-cell STYLE/COLOR REFERENCE ONLY.
Input image 4: existing NK-cell STYLE/COLOR REFERENCE ONLY.
Primary request: Make the flesh/body color of EVERY one of the 16 target frames match the subdued dusty grey-rose bodies of the three existing reference enemies. The current target's warm bright salmon/coral/orange flesh looks like the player and must change to the existing enemy palette.
EXACT flesh palette sampled from reference sprites: dominant base #AC7A7D (muted dusky rose), lighter base #BF989B (grey rose), shadows #946B82 and #AA8393 (muted mauve), limited highlights #DFBCBC and #EDD6D6 (pale dusty pink), deepest outlines #4A1A48 and #801A34. Body midtones must be low saturation, not orange, peach, warm salmon or vivid pink. Avoid yellow cream skin highlights: recolor them to pale dusty pink. Preserve the dark eyes and small pale eye highlights.
Keep distinctly amber acid liquid and attack effects, burgundy clot armor, and pale blue air sacs recognizable, but do not mistake skin for an effect. Recolor pink flesh fragments and dead puddles in death row using the SAME dusty palette.
Absolute invariants: preserve all 16 individual poses, character design, face, anatomy, appendages, accessories, silhouette, outlines, frame order, exact 4 column x4 row layout, cell spacing, camera angle and pixel-art texture. Top row idle, second row locomotion, third row attack, bottom row progressive collapse/death. Do NOT copy reference characters into output. Do NOT add/remove frames, add labels, add borders, invent new poses, crop body parts, smooth into a painting, or change the solid bright green chroma-key background. Retain the original square sprite sheet composition exactly. Only change body colors and saturation consistently across all 16 frames.
