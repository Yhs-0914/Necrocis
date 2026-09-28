# Cerebrum environment v2

Generated with the built-in image_gen tool, 2026-09-28.
Edit target: `Assets/_Project/Art/Generated/FinalBoss/CerebrumArena_Dormant.png`.
Saved asset: `Assets/_Project/Art/Generated/FinalBoss/CerebrumEnvironment_v2.png`.

## Prompt

Use case: precise-object-edit. Asset type: Unity game arena environment clean plate, wide 16:9. Edit the input image. Remove ONLY the four standing organ pillars (stomach upper left, intestine upper right, liver lower left, lung lower right) and the large brain at top center, including their root bases and shadows. Fill removed regions with the natural dark mauve floor or surrounding wall as appropriate. Preserve exactly the original framing, thick curved fleshy perimeter walls, bottom center entrance, fine blue-grey neuron network on floor, muted purple/pink pixel-painted style, lighting and all wall detail. Keep the pale tendons from walls toward former organ positions, ending on empty floor. No new objects, no text, no UI. This is the empty environment behind separately rendered interactive organ sprites, not a new design. Keep original room proportions and as much original detail unchanged as possible.

## Integration

The common MapGenerator samples this texture into position-specific cell sprites and tiles, rendered and unloaded by the existing chunk pipeline. The atlas does not define collision. AuthoredMapLayout floor and blocked cells remain the traversal authority. Five independent organ/brain sprites are rendered above the environment. The source reference remains unchanged.
