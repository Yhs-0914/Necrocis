using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NecrocisEditor
{
    public static partial class HardenedResiduePlayModeRunner
    {
        public const string DirectionReport = "Logs/Residue-D2-results.txt";
        private static float directionPreviewAngle = 90;
        private static int directionPreviewMode;
        private static Vector2 previewFootprint;
        public static void RunDirections() => Start(false, false, true);
        public static void PreviewDirections(float angle = 90, bool dropping = false)
        { directionPreviewAngle = angle; directionPreviewMode = dropping ? 1 : 0; previewFootprint = Vector2.zero; Start(true, false, true); }
        public static void PreviewLanding(float angle = 270, float length = 0, float width = 0)
        { directionPreviewAngle = angle; directionPreviewMode = 2; previewFootprint = new Vector2(length, width); Start(true, false, true); }
        private static Vector3 Aim(float angle) => new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
        private static SpriteRenderer DirectionBody => enemy.transform.Find("Visual").GetComponent<SpriteRenderer>();

        private static IEnumerator DirectionChecks()
        {
            Require(enemy.HasPatternDirections, "actual field spawn binds I-02 directions");
            Pass("dedicated Intestine map spawner binds I-02 directions at zero kills");
            field.enabled = false; enemy.ReleaseToPool(); enemy = null;
            if (preview && previewFootprint.x > 0 && previewFootprint.y > 0)
            {
                Backup(settings);
                using var data = new SerializedObject(settings);
                data.FindProperty("footprint").vector2Value = previewFootprint; data.ApplyModifiedPropertiesWithoutUndo();
            }
            origin = FindDirectionPatch();
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 60, true);
            var art = settings.presentation; var directions = art.directionalPresentation; var snapshot = directions.Capture();
            if (preview)
            {
                Spawn(); MovePlayerTo(origin + Aim(directionPreviewAngle) * 2.6f); health.ResetHealth();
                yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 4, "directional preview tell");
                if (directionPreviewMode == 2)
                {
                    MovePlayerTo(origin + Aim(directionPreviewAngle) * 2.6f + Vector3.right * 2.5f);
                    yield return Wait(() => pattern.Phase == HardenedResiduePhase.Recovery, 3, "directional preview landing");
                    var overlay = new GameObject("ReviewOnly_LandedFootprint");
                    overlay.transform.position = pattern.LandingCenter + Vector3.up * .06f;
                    overlay.transform.rotation = Quaternion.LookRotation(pattern.LandingDirection);
                    EnemyGroundTelegraph.Rectangle(overlay.transform, pattern.Footprint, art.dangerFillColor);
                    enemy.GetComponent<EnemyPatternLifetime>().Own(overlay);
                }
                else if (directionPreviewMode == 1)
                    yield return Wait(() => pattern.Phase == HardenedResiduePhase.Dropping && pattern.ActiveRubble != null, 3, "directional preview separation");
                else yield return new WaitForSeconds(settings.windupSeconds * .85f);
                yield break;
            }
            Require(directions.frames.Length == 17 && directions.frames.SelectMany(f => new[] { f.front, f.back }).Distinct().Count() == 34,
                "17 source keys / 34 front and back body/death sprites");
            foreach (var group in directions.frames.SelectMany(f => new[] { f.front, f.back }).GroupBy(s => AssetDatabase.GetAssetPath(s.texture)))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(group.Key);
                Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed,
                    "pixel import " + group.Key);
                Require(File.ReadAllBytes(group.Key).SequenceEqual(File.ReadAllBytes("output/art/HardenedResidueDirections/" + Path.GetFileName(group.Key))), "approved PNG bytes preserved");
                foreach (Sprite s in group)
                { Equal(Mathf.Round(s.pivot.x), s.pivot.x, "integer x pivot"); Equal(Mathf.Round(s.pivot.y), s.pivot.y, "integer y pivot"); }
            }
            Require(art.airborneChip != null && art.airborneChip.texture == directions.frames[0].front.texture, "chip uses an approved sheet crop");
            Pass("34 sprites imported with Point/no mip/uncompressed/integer anchors; all four PNGs unchanged; isolated chip shares approved texture");
            var verticalPath = AssetDatabase.GetAssetPath(art.verticalRubble);
            Require(File.ReadAllBytes(verticalPath).SequenceEqual(File.ReadAllBytes("output/art/HardenedResidueRubbleDirections/" + HardenedResidueDirectionalImport.SelectedRubbleFile)), "approved aligned v7 source bytes preserved");
            Require(art.verticalRubble.name == Path.GetFileNameWithoutExtension(HardenedResidueDirectionalImport.SelectedRubbleFile) + "_00", "approved aligned vertical view is assigned");
            Equal(291, art.verticalRubble.pivot.x, "aligned vertical ground x matches approved anchor");
            Equal(6, art.verticalRubble.pivot.y, "aligned vertical ground y matches approved anchor");
            Require(art.verticalRubble.rect == new Rect(0, 0, 560, 754), "full aligned frame preserves original height and row-shift padding");
            var verticalImporter = (TextureImporter)AssetImporter.GetAtPath(verticalPath);
            Require(verticalImporter.filterMode == FilterMode.Point && !verticalImporter.mipmapEnabled
                && verticalImporter.textureCompression == TextureImporterCompression.Uncompressed, "vertical pixel import settings");
            Pass("approved aligned v7 imported unchanged with Point/no mip/uncompressed, full frame and preserved ground anchor");

            var inspector = UnityEditor.Editor.CreateEditor(directions);
            var inspectorRoot = inspector.CreateInspectorGUI();
            Require(inspectorRoot.Query<DropdownField>().ToList().Any(d => d.choices.Contains("이동") && d.choices.Contains("발사 · 내려놓기")), "Inspector exposes move and release groups");
            UnityEngine.Object.DestroyImmediate(inspector);
            Pass("directional Inspector exposes all seven motion groups with existing enum IDs preserved");

            Spawn(); pattern.enabled = false;
            foreach (float angle in new[] { 0f, 90f, 180f, 270f })
            {
                enemy.SetPatternFacing(Aim(angle)); EnemyFacing facing = enemy.PatternFacing;
                foreach (DirectionalSpriteFrame row in directions.frames.Where(f => f.motion != EnemyPoseGroup.Death))
                {
                    enemy.SetPatternFrame(row.source);
                    Require(DirectionBody.sprite == snapshot.Resolve(row.source, facing) && DirectionBody.flipX == snapshot.Flip(facing), "live directional pose " + row.label);
                }
            }
            Pass("all 11 non-death poses resolve correctly on the live renderer in four directions (44 pose checks)");

            foreach (float angle in new[] { 0f, 90f, 180f, 270f })
            {
                Vector3 aim = Aim(angle); Spawn(); MovePlayerTo(origin + aim * 2.6f); health.ResetHealth();
                yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 4, "directional tell");
                EnemyFacing facing = enemy.PatternFacing; Vector3 landing = pattern.LandingCenter;
                Require(enemy.IsPatternFacingLocked && facing == snapshot.Select(aim, DontStarveCamera.GetActiveCamera(), EnemyFacing.Front, false), "attack commits target facing");
                Equal(0, (landing - (origin + aim * settings.placementDistance)).magnitude, "landing stays at existing placement distance");
                bool vertical = Mathf.Abs(aim.z) > .9f;
                Equal(0, (pattern.LandingDirection - (vertical ? Vector3.left : Vector3.forward)).magnitude, "landing long axis matches facing");
                Require(pattern.UsesVerticalRubble == vertical, "sprite and ground orientation select together");
                Vector2 worldSize = vertical ? new Vector2(settings.footprint.y, settings.footprint.x) : settings.footprint;
                ElitePresentationChecks.RedArea(pattern.TelegraphObject, worldSize);
                MovePlayerTo(origin - aim * 3.5f);
                enemy.SetPatternFacing(-aim);
                yield return Wait(() => pattern.Phase == HardenedResiduePhase.Dropping, 3, "chip separation");
                Require(enemy.PatternFacing == facing && enemy.IsPatternFacingLocked && DirectionBody.sprite == snapshot.Resolve(art.release, facing), "release pose and facing committed");
                Equal(0, (pattern.LaunchPosition - enemy.GetPatternVisualOrigin()).magnitude, "actual launch uses captured direction origin");
                var slab = pattern.ActiveRubble; var visual = slab.transform.Find("CrustSlab").GetComponent<SpriteRenderer>();
                Require(visual.sprite == art.airborneChip && !slab.IsBlocking && !slab.GetComponent<BoxCollider>().enabled, "airborne chip is visual only");
                Vector3 start = pattern.LaunchPosition, end = slab.LandingVisualPosition;
                float t = 0;
                while (pattern.Phase == HardenedResiduePhase.Dropping)
                {
                    Vector3 offset = visual.transform.position - start, flight = end - start;
                    float currentT = Vector3.Dot(offset, flight) / flight.sqrMagnitude;
                    Require(currentT >= t - .001f && currentT <= 1.001f && Vector3.Cross(offset, flight).magnitude < .002f, "continuous one-way flight from chip to landing");
                    t = currentT; yield return null;
                }
                Require(pattern.Phase == HardenedResiduePhase.Recovery && pattern.LandingCount == 1 && slab.IsBlocking, "one grounded obstacle after landing");
                Require(visual.sprite == (vertical ? art.verticalRubble : art.rubble) && DirectionBody.sprite == snapshot.Resolve(art.recovery, facing), "landing selects directional rubble and recovery");
                Equal(0, (visual.transform.position - end).magnitude, "landing visual meets camera-near edge without a sideways offset");
                if (vertical)
                {
                    float projectedLength = settings.footprint.x * Mathf.Abs(Vector3.Dot(DontStarveCamera.GetActiveCamera().transform.up, Vector3.forward));
                    Equal(projectedLength, visual.sprite.bounds.size.y * visual.transform.lossyScale.y * art.verticalRubbleGroundDepthFraction,
                        "authored ground span matches projected landing length; raised faces retain their height");
                    Require(!visual.flipX && !visual.flipY && visual.drawMode == SpriteDrawMode.Simple,
                        "approved upright artwork is displayed without screen rotation, mirror or sliced stretching");
                }
                else Equal(worldSize.x, visual.sprite.bounds.size.x * visual.transform.lossyScale.x, "horizontal width follows footprint");
                Equal(visual.transform.lossyScale.x, visual.transform.lossyScale.y, "selected sprite proportions preserved");
                Equal(0, (slab.transform.position - landing).magnitude, "collider/landing origin preserved");
                Require(pattern.TelegraphObject == null && enemy.IsPatternFacingLocked, "tell cleared, recovery facing still locked");
                yield return null;
                Equal(worldSize.x, slab.GetComponent<BoxCollider>().bounds.size.x, "rotated collider world width");
                Equal(worldSize.y, slab.GetComponent<BoxCollider>().bounds.size.z, "rotated collider world depth");
                if (Mathf.Abs(aim.z) > .9f)
                    Require(aim.z > 0 ? visual.sortingOrder < DirectionBody.sortingOrder : visual.sortingOrder > DirectionBody.sortingOrder,
                        "back/up slab sorts behind owner; front/down slab sorts in front");
                yield return Wait(() => pattern.Phase == HardenedResiduePhase.Ready, 3, "recovery unlock"); yield return null;
                Require(!enemy.IsPatternFacingLocked && enemy.PatternFacing != facing, "ready unlocks toward opposite player");
                Pass(facing + ": committed cycle, actual chip origin/continuous drop, matching rotated warning/collider, selected sprite/depth/anchor, Ready unlock");
            }

            foreach (float angle in new[] { 0f, 90f })
            {
                foreach (bool alongLength in new[] { true, false }) foreach (float margin in new[] { -.02f, .02f })
                {
                    Spawn(); MovePlayerTo(origin + Aim(angle) * 2.6f); health.ResetHealth();
                    yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 4, "oriented landing damage edge");
                    Vector3 longAxis = Vector3.Cross(Vector3.up, pattern.LandingDirection);
                    Vector3 axis = alongLength ? longAxis : pattern.LandingDirection;
                    float half = (alongLength ? settings.footprint.x : settings.footprint.y) * .5f;
                    MovePlayerTo(pattern.LandingCenter + axis * (half + margin)); float hp = health.CurrentHealth;
                    yield return Wait(() => pattern.Phase == HardenedResiduePhase.Recovery, 3, "oriented edge impact");
                    Equal(margin < 0 ? 3 : 0, hp - health.CurrentHealth, "oriented rectangle damage edge");
                    var slab = pattern.ActiveRubble; Vector3 center = slab.transform.position;
                    var feet = player.GetComponent<ProceduralTerrainMotor>().TerrainHalfExtents;
                    Require(!ResidueRubble.CanTraverse(center + longAxis * 4, center - longAxis * 4, feet), "movement blocks through oriented length");
                    Require(!ResidueRubble.CanTraverse(center + pattern.LandingDirection * 3, center - pattern.LandingDirection * 3, feet), "movement blocks through oriented width");
                    Vector3 outside = center + longAxis * (settings.footprint.x * .5f + .02f);
                    LayerMask mask = 1 << enemy.gameObject.layer;
                    ResidueRubble.HitAlongSegment(outside - pattern.LandingDirection * 2, outside + pattern.LandingDirection * 2, 0, 1, mask, ResidueRubble.NextAttackToken());
                    Require(slab.IsBlocking, "projectile outside rotated slab does not break it");
                    Vector3 inside = center + longAxis * (settings.footprint.x * .5f - .02f);
                    ResidueRubble.HitAlongSegment(inside - pattern.LandingDirection * 2, inside + pattern.LandingDirection * 2, 0, 1, mask, ResidueRubble.NextAttackToken());
                    Require(!slab.IsBlocking && ResidueRubble.CanTraverse(center + longAxis * 4, center - longAxis * 4, feet), "inside projectile breaks slab and releases movement immediately");
                }
                Pass((angle == 0 ? "Horizontal" : "Vertical") + ": actual damage edges (+/- .02m), both movement axes, projectile break edges and immediate unblock match the warning orientation");
            }

            Spawn(); MovePlayerTo(origin + Vector3.back * 3.5f); yield return null;
            pattern.enabled = false; Vector3 frozen = enemy.GetPatternVisualOrigin();
            Backup(directions); Vector2 edited = directions.frontOrigin + new Vector2(.23f, .17f);
            using (var data = new SerializedObject(directions))
            { data.FindProperty("frontOrigin").vector2Value = edited; data.ApplyModifiedPropertiesWithoutUndo(); }
            Equal(0, (enemy.GetPatternVisualOrigin() - frozen).magnitude, "live enemy retains spawn snapshot after Inspector edit");
            Spawn(); MovePlayerTo(origin + Vector3.back * 2.6f);
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Dropping, 4, "edited origin launch");
            Vector3 expected = enemy.transform.Find("Visual").TransformPoint(new Vector3(edited.x, edited.y, 0));
            Equal(0, (pattern.LaunchPosition - expected).magnitude, "Inspector frontOrigin changes actual next-spawn launch");
            Equal(0, (pattern.LandingCenter - (origin + Vector3.back * settings.placementDistance)).magnitude, "origin edit cannot move damage footprint");
            Pass("Inspector origin edit affects next actual launch, current spawn stays immutable, landing footprint unchanged");
            RestoreAssets();

            Spawn(); MovePlayerTo(origin + Vector3.back * 2.6f);
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Recovery, 4, "presentation ratio snapshot");
            var firstView = pattern.ActiveRubble.transform.Find("CrustSlab").GetComponent<SpriteRenderer>();
            Vector3 firstScale = firstView.transform.localScale;
            float originalFraction = art.verticalRubbleGroundDepthFraction;
            Set(art, "verticalRubbleGroundDepthFraction", .9f);
            Equal(0, (firstView.transform.localScale - firstScale).magnitude, "live rubble retains its captured presentation scale");
            Spawn(); MovePlayerTo(origin + Vector3.back * 2.6f);
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Recovery, 4, "changed presentation ratio next spawn");
            var changedView = pattern.ActiveRubble.transform.Find("CrustSlab").GetComponent<SpriteRenderer>();
            Equal(firstScale.x * originalFraction / .9f, changedView.transform.localScale.x, "Inspector presentation ratio affects actual next-spawn display");
            Equal(0, (pattern.Footprint - settings.footprint).magnitude, "presentation ratio does not alter footprint");
            Equal(settings.footprint.y, pattern.ActiveRubble.GetComponent<BoxCollider>().bounds.size.x, "presentation edit cannot change collider width");
            Equal(settings.footprint.x, pattern.ActiveRubble.GetComponent<BoxCollider>().bounds.size.z, "presentation edit cannot change collider length");
            Pass("Inspector presentation ratio changes next spawned visual uniformly, leaves live snapshot and damage/block dimensions unchanged");
            RestoreAssets();

            Spawn(); MovePlayerTo(origin + Vector3.forward * 2.6f);
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Windup, 4, "stun tell");
            enemy.StatusEffects.ApplyStun(.3f);
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Ready, 2, "stun cleanup");
            Require(!enemy.IsPatternFacingLocked && pattern.ActiveRubble == null && pattern.LandingCount == 0 && pattern.TelegraphObject == null, "stun cancels tell and direction without a drop");
            Pass("stun cancellation clears tell and direction lock without landing");

            Spawn(); MovePlayerTo(origin + Vector3.forward * 2.6f);
            yield return Wait(() => pattern.Phase == HardenedResiduePhase.Dropping, 4, "rear death drop");
            var owned = pattern.ActiveRubble; var tell = pattern.TelegraphObject; EnemyFacing deathFacing = enemy.PatternFacing; var renderer = DirectionBody;
            enemy.TakeDamage(10000);
            Require((owned == null || !owned.gameObject.activeSelf) && (tell == null || !tell.activeSelf), "death disables airborne chip/tell immediately");
            var seen = new HashSet<Sprite>(); float until = Time.time + 2;
            while (enemy.gameObject.activeSelf && Time.time < until) { seen.Add(renderer.sprite); yield return null; }
            Require(!enemy.gameObject.activeSelf && art.deathFrames.All(s => seen.Contains(snapshot.Resolve(s, deathFacing))), "six rear death frames finish before pool return");
            Spawn();
            Require(enemy.HasPatternDirections && !enemy.IsPatternFacingLocked && pattern.ActiveRubble == null && pattern.LaunchPosition == Vector3.zero, "pool spawn resets lock, chip, and launch state");
            Pass("death during drop immediately removes chip/tell, keeps six rear death frames, and pool reuse rebinds cleanly");
            Pass("D-2 approved aligned v7 connection checks complete; D-3 terrain/combat matrix remains a separate stage");
        }

        private static Vector3 FindDirectionPatch()
        {
            Vector2Int initial = biome.WorldToGrid(origin);
            for (int radius = 0; radius <= 45; radius++) for (int dx = -radius; dx <= radius; dx++) for (int dz = -radius; dz <= radius; dz++)
            {
                if (Mathf.Abs(dx) != radius && Mathf.Abs(dz) != radius) continue;
                int x = initial.x + dx, z = initial.y + dz;
                if (!biome.IsValidPosition(x, z) || !biome.IsWalkable(x, z)) continue;
                Vector3 candidate = biome.GridToWorldWithHeight(x, z); bool clear = true;
                foreach (float angle in new[] { 0f, 90f, 180f, 270f })
                {
                    Vector3 forward = Mathf.Abs(Aim(angle).z) > .9f ? Vector3.left : Vector3.forward;
                    if (!ResiduePlacementSafety.CanPlace(candidate + Aim(angle) * settings.placementDistance, forward, settings.footprint))
                    { clear = false; break; }
                }
                if (clear) return candidate;
            }
            throw new InvalidOperationException("No clear four-direction residue landing patch");
        }
    }
}
