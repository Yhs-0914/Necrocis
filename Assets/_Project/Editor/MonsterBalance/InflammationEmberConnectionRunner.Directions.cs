using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NecrocisEditor
{
    public static partial class InflammationEmberConnectionRunner
    {
        public static void RunDirections() => Start(false, false, false, 180, false, true);
        public static void PreviewDirections(float angle = 270, bool flight = false) => Start(true, false, flight, angle, false, true);

        private static Vector3 FindDirectionalPatch()
        {
            for (int z = 6; z < biome.MapHeight - 6; z++) for (int x = 6; x < biome.MapWidth - 6; x++)
            {
                int level = biome.GetHeightLevel(x, z); bool clear = biome.IsWalkable(x, z);
                for (int dx = -5; dx <= 5 && clear; dx++) for (int dz = -5; dz <= 5 && clear; dz++)
                    clear = biome.IsWalkable(x + dx, z + dz) && biome.GetHeightLevel(x + dx, z + dz) == level;
                if (clear) return biome.GridToWorldWithHeight(x, z);
            }
            throw new InvalidOperationException("No clear directional projectile patch in the real Liver map");
        }

        private static IEnumerator DirectionChecks()
        {
            var art = settings.presentation; var directions = art.directionalPresentation;
            Require(enemy.HasPatternDirections && directions != null, "temporary field spawn binds H-02 direction data");
            Pass("dedicated zero-kill field spawn binds approved directions; production Liver list unchanged");
            field.enabled = false; enemy = null;
            ((IList)typeof(ProceduralBiomeBridge).GetField("normalEnemyRules", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(biome)).Clear();
            if (!preview) typeof(InputManager).GetMethod("SetActionsEnabled", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(InputManager.Instance, new object[] { false });
            origin = FindDirectionalPatch();
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 60, true);
            if (preview)
            {
                SpawnBattle(); MoveTo(origin + previewDirection * 4.5f); yield return new WaitForSeconds(.2f); enemy.TakeDamage(1);
                yield return Wait(() => pattern.Phase == InflammationEmberPhase.Windup, 1, "directional preview tell");
                if (flightPreview)
                {
                    yield return Wait(() => pattern.ActiveThorn != null, 2, "directional preview launch");
                    yield return Wait(() => pattern.ActiveThorn == null || Vector3.Distance(pattern.ActiveThorn.transform.position, pattern.ActiveThorn.LaunchPosition) > .2f, 1, "initial separation");
                    Require(pattern.ActiveThorn != null, "initial thorn remains visible");
                }
                else yield return new WaitForSeconds(settings.windupSeconds * .7f);
                yield break;
            }

            Require(directions.frames.Length == 16 && !directions.authoredFacingLeft
                && directions.frames.SelectMany(f => new[] { f.front, f.back }).Distinct().Count() == 32, "16 source keys, 32 new sprites, original right-facing side");
            foreach (var group in directions.frames.SelectMany(f => new[] { f.front, f.back }).GroupBy(s => AssetDatabase.GetAssetPath(s)))
            {
                Require(File.ReadAllBytes(group.Key).SequenceEqual(File.ReadAllBytes(InflammationEmberDirectionalImport.SourceFolder + Path.GetFileName(group.Key))), "approved PNG bytes preserved");
                var importer = (TextureImporter)AssetImporter.GetAtPath(group.Key);
                Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "directional pixel import");
                foreach (var sprite in group)
                { Equal(Mathf.Round(sprite.pivot.x), sprite.pivot.x, "integer directional x pivot"); Equal(Mathf.Round(sprite.pivot.y), sprite.pivot.y, "integer directional y pivot"); }
            }
            Pass("32 approved PNG sprites unchanged, Point/no mip/uncompressed and integer ground pivots; 16 directional source keys");
            var editor = UnityEditor.Editor.CreateEditor(directions); var root = editor.CreateInspectorGUI();
            Require(root.Query<DropdownField>().ToList().Any(f => f.choices.Contains("이동") && f.choices.Contains("발사 · 내려놓기")), "all seven motion groups in directional Inspector");
            UnityEngine.Object.DestroyImmediate(editor);
            var catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath);
            var catalogEditor = UnityEditor.Editor.CreateEditor(catalog);
            try
            {
                typeof(MonsterBalanceCatalogEditor).GetField("monsterIndex", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(catalogEditor, catalog.monsters.IndexOf(definition));
                var ui = catalogEditor.CreateInspectorGUI();
                Require(ui.Query<PropertyField>().ToList().Any(p => p.bindingPath == "directionalPresentation"), "catalog exposes the H-02 directional source");
            }
            finally { UnityEngine.Object.DestroyImmediate(catalogEditor); }
            Pass("directional and catalog Inspectors expose H-02 direction/motion/origin source independently of balance");

            var snapshot = directions.Capture();
            SpawnBattle(); pattern.enabled = false;
            foreach (float angle in new[] { 0f, 90f, 180f, 270f })
            {
                enemy.SetPatternFacing(Direction(angle), false); EnemyFacing facing = enemy.PatternFacing;
                foreach (var row in directions.frames.Where(f => f.motion != EnemyPoseGroup.Death))
                {
                    enemy.SetPatternFrame(row.source);
                    Require(Body().sprite == snapshot.Resolve(row.source, facing) && Body().flipX == snapshot.Flip(facing), "actual body renderer resolves " + facing + "/" + row.label);
                }
            }
            Pass("40 live body-pose checks: ten motions/frames in four directions, original side mirroring retained");

            foreach (float angle in new[] { 0f, 90f, 180f, 270f })
            {
                Vector3 aim = Direction(angle); SpawnBattle(); MoveTo(origin + aim * 3); health.ResetHealth(); yield return null;
                Require(pattern.ShotCount == 0 && pattern.Phase == InflammationEmberPhase.Ready, "no autonomous firing");
                float began = Time.time, hp = health.CurrentHealth; enemy.TakeDamage(1);
                EnemyFacing facing = enemy.PatternFacing;
                Require(pattern.Phase == InflammationEmberPhase.Windup && enemy.IsPatternFacingLocked, "first hit locks preparation direction");
                MoveTo(origin - aim * 3); enemy.SetPatternFacing(-aim, false); enemy.TakeDamage(1);
                Require(enemy.PatternFacing == facing && pattern.LockedDirection == aim, "extra hit/target move cannot retarget committed attack");
                yield return Wait(() => pattern.ActiveThorn != null, 2, "directional release " + angle);
                var shot = pattern.ActiveThorn; var sprite = shot.GetComponentInChildren<SpriteRenderer>();
                Require(Time.time - began >= settings.windupSeconds - .01f && pattern.ShotCount == 1, "one shot after original preparation duration");
                Equal(0, (shot.Direction - aim).magnitude, "flight follows committed vector");
                Vector3 start = enemy.GetPatternVisualOrigin(); start.y = enemy.transform.position.y;
                Vector3 extents = enemy.GetComponent<Collider>().bounds.extents;
                float extent = Mathf.Abs(aim.x) * extents.x + Mathf.Abs(aim.z) * extents.z;
                start += aim * (extent + settings.thornRadius); start.y = biome.GetGroundHeight(start);
                Equal(0, (shot.LaunchPosition - start).magnitude, "directional origin reaches actual ground launch");
                float forward = Vector3.Dot(shot.LaunchPosition - enemy.transform.position, aim);
                Require(forward - shot.Length * .5f < extent && forward + shot.Length * .5f > extent, "projectile tail overlaps body front without a detached gap");
                if (facing == EnemyFacing.Front)
                {
                    Vector3 tail = GasSacOrb.GetFlightPosition(shot.LaunchPosition, shot.FlightHeight) - aim * (shot.Length * .5f);
                    Require(Vector3.Dot(tail - enemy.transform.position, DontStarveCamera.GetActiveCamera().transform.up) >= -.02f,
                        "front launch tail meets the visible grounded spine without an initial gap");
                }
                float minimum = float.PositiveInfinity, maximum = float.NegativeInfinity;
                var seen = new HashSet<Sprite>(); float end = Time.time + .18f;
                while (Time.time < end)
                {
                    yield return new WaitForEndOfFrame(); Require(shot != null, "visual probe stays alive"); seen.Add(sprite.sprite);
                    float ground = biome.GetGroundHeight(shot.transform.position);
                    Equal(.16f, sprite.transform.position.y - ground, "flight height remains ground plus radius");
                    Require(Vector3.Dot(sprite.transform.right, aim) > .999f, "thorn long axis follows XZ flight");
                    Bounds b = sprite.sprite.bounds;
                    foreach (float x in new[] { b.min.x, b.max.x }) foreach (float y in new[] { b.min.y, b.max.y })
                    {
                        float h = sprite.transform.TransformPoint(new Vector3(x, y, 0)).y - ground;
                        minimum = Mathf.Min(minimum, h); maximum = Mathf.Max(maximum, h);
                        Require(h >= -.003f && h <= .323f, "whole thorn quad stays near ground without penetration");
                    }
                    if (Mathf.Abs(aim.z) > .9f) Require(aim.z > 0 ? sprite.sortingOrder < Body().sortingOrder : sprite.sortingOrder > Body().sortingOrder, "front/back depth ordering");
                    Require(shot.transform.childCount == 1 && shot.GetComponentsInChildren<LineRenderer>().Length == 0, "simple thorn has no floor UI");
                }
                Require(art.thornFrames.All(seen.Contains), "both thorn frames keep the same ground alignment");
                MoveTo(origin + aim * 3);
                yield return Wait(() => health.CurrentHealth < hp, 2, "actual directional hit");
                Equal(3, hp - health.CurrentHealth, "original 3 damage once");
                MoveTo(origin - aim * 3);
                yield return Wait(() => pattern.Phase == InflammationEmberPhase.Ready, 2, "direction unlock after recovery"); yield return null;
                Require(!enemy.IsPatternFacingLocked && enemy.PatternFacing != facing, "ready follows new target after unlock");
                Pass($"{facing}: fixed tell/recovery, native origin, quad {minimum:F3}..{maximum:F3}m, ground .16m, two frames, sorting/no UI, damage 3 once and unlock");
            }

            SpawnBattle(); MoveTo(origin + Vector3.back * 3); yield return null;
            Vector3 frozen = enemy.GetPatternVisualOrigin(); Vector2 original = directions.frontOrigin;
            Set(directions, "frontOrigin.x", original.x + .08f); Set(directions, "frontOrigin.y", original.y + .12f);
            Equal(0, (enemy.GetPatternVisualOrigin() - frozen).magnitude, "existing actor retains captured Inspector origin");
            SpawnBattle(); MoveTo(origin + Vector3.back * 3); yield return null; enemy.TakeDamage(1);
            Vector3 expectedOrigin = enemy.GetPatternVisualOrigin(); expectedOrigin.y = enemy.transform.position.y;
            expectedOrigin += Vector3.back * (enemy.GetComponent<Collider>().bounds.extents.z + settings.thornRadius);
            expectedOrigin.y = biome.GetGroundHeight(expectedOrigin);
            yield return Wait(() => pattern.ActiveThorn != null, 2, "edited origin release");
            Equal(0, (pattern.ActiveThorn.LaunchPosition - expectedOrigin).magnitude, "Inspector front origin changes next actual launch");
            Equal(.16f, pattern.ActiveThorn.FlightHeight, "origin cannot raise flight"); Equal(.7f, pattern.ActiveThorn.Length, "origin cannot resize hit capsule");
            RestoreAssets();
            Pass("Serialized Inspector origin edit affects next spawned shot, leaves current snapshot/height/length unchanged; source restored");

            foreach (float angle in new[] { 0f, 90f, 180f, 270f })
            {
                SpawnBattle(); MoveTo(origin + Direction(angle) * 3); yield return null; enemy.TakeDamage(1);
                var facing = enemy.PatternFacing; enemy.TakeDamage(10000);
                var seen = new HashSet<Sprite>(); var body = Body(); float until = Time.time + 2;
                Require(pattern.Phase == InflammationEmberPhase.Inactive && pattern.ActiveThorn == null, "death cancels queued counter");
                while (enemy.gameObject.activeSelf && Time.time < until) { seen.Add(body.sprite); yield return null; }
                Require(!enemy.gameObject.activeSelf && art.deathFrames.All(f => seen.Contains(snapshot.Resolve(f, facing))), "all six selected-direction death frames before pooling");
                Pass(facing + ": preparation death cancels counter and plays all six matching death frames");
            }
            SpawnBattle(); MoveTo(origin + Vector3.forward * 3); yield return null; enemy.TakeDamage(1);
            enemy.StatusEffects.ApplyStun(.3f);
            yield return Wait(() => pattern.Phase == InflammationEmberPhase.Ready, 1, "stun cancellation");
            Require(!enemy.IsPatternFacingLocked && pattern.ShotCount == 0 && pattern.ActiveThorn == null, "stun unlocks and cancels shot");
            SpawnBattle(); MoveTo(origin + Vector3.forward * 3); yield return null; enemy.TakeDamage(1);
            yield return Wait(() => pattern.ActiveThorn != null, 2, "pool cleanup live thorn");
            var owned = pattern.ActiveThorn; enemy.ReleaseToPool();
            Require(!owned.gameObject.activeSelf, "pool release hides thorn immediately");
            SpawnBattle(); Require(enemy.HasPatternDirections && !enemy.IsPatternFacingLocked && pattern.ActiveThorn == null && pattern.ShotCount == 0, "reuse starts with clean direction and shot state");
            Pass("stun and live-shot pool release cleanly cancel/unlock; new generation rebinds directions without old effects");
            Pass("H-02-D-2 connection complete; D-3 terrain/combat matrix and Liver production registration remain separate stages");
        }
    }
}
