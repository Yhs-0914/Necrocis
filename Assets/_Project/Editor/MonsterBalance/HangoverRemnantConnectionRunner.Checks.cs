using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class HangoverRemnantConnectionRunner
    {
        private static IEnumerator Checks()
        {
            var art = settings.presentation;
            var frames = art.idleFrames.Concat(art.moveFrames).Concat(art.preparationFrames).Concat(new[] { art.release })
                .Concat(art.retreatFrames).Concat(new[] { art.recovery, art.hit }).Concat(art.deathFrames).Concat(art.remnantFrames).Concat(art.burstFrames).ToArray();
            Require(frames.Length == 24 && frames.Distinct().Count() == 24, "24 distinct approved sprites");
            foreach (var group in frames.GroupBy(AssetDatabase.GetAssetPath))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(group.Key);
                Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "pixel import");
                string source = HangoverRemnantSetup.SourceFolder + Path.GetFileName(group.Key);
                if (!File.Exists(source)) source = "output/art/HangoverRemnant/" + Path.GetFileName(group.Key);
                Require(File.ReadAllBytes(group.Key).SequenceEqual(File.ReadAllBytes(source)), "approved PNG bytes identical");
                foreach (var f in group) { Equal(Mathf.Round(f.pivot.x), f.pivot.x, "integer pivot x"); Equal(Mathf.Round(f.pivot.y), f.pivot.y, "integer pivot y"); }
            }
            Require(definition.contact.enabled && art.deathFrames.All(f => f.name.Contains("Front_Death")), "contact enabled and dedicated front death");
            Pass("24 approved sprites: front body 12/death 6 + remnant 2/burst 4; unchanged PNGs, Point, no mip/compression, integer ground pivots");
            GameManager.Instance.EnterBiome(BiomeType.Liver); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LIVER); yield return null;
            biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>(); field = biome.GetComponent<BiomeEliteField>();
            groundOffset = player.transform.position.y - biome.GetGroundHeight(player.transform.position);
            Require(field.Plan == null && biome.GetBiomeConfig().biomeEliteSpawnConfig.monsters.Count == productionLiverCount, "production field isolated; original list unchanged");
            temporary = ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>(); temporary.minimumCount = temporary.maximumCount = 1; temporary.clearanceCells = 5;
            temporary.monsters.Add(HangoverRemnantSetup.PreviewRule(definition));
            field.Configure(biome, temporary, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            var point = field.Plan.placements.Single(); fieldOrigin = biome.GridToWorldWithHeight(point.x, point.y); origin = fieldOrigin;
            if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
            Move(origin + Vector3.right * 5); field.Refresh(player.transform.position);
            enemy = field.Spawners.Single().ActiveEnemy; pattern = enemy.GetComponent<HangoverRemnantElitePattern>();
            Require(pattern != null && enemy.IsAiSuppressed && enemy.IsElite && enemy.Config.deathSprites.Length == 6, "dedicated zero-kill field attaches pattern/death");
            Pass("temporary real Liver field attaches H-03 at zero kills; production map list unchanged");
            field.enabled = false; enemy = null;
            ((IList)typeof(ProceduralBiomeBridge).GetField("normalEnemyRules", Private).GetValue(biome)).Clear();
            typeof(InputManager).GetMethod("SetActionsEnabled", Private).Invoke(InputManager.Instance, new object[] { false });
            bool found = false;
            for (int z = 6; z < biome.MapHeight - 6 && !found; z++) for (int x = 6; x < biome.MapWidth - 6 && !found; x++)
            {
                int level = biome.GetHeightLevel(x, z); bool clear = true;
                for (int dx = -5; dx <= 5 && clear; dx++) for (int dz = -5; dz <= 5 && clear; dz++) clear = biome.IsWalkable(x + dx, z + dz) && biome.GetHeightLevel(x + dx, z + dz) == level;
                if (clear) { origin = biome.GridToWorldWithHeight(x, z); found = true; }
            }
            Require(found, "real clear flat patch"); PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 60, true);
            if (preview)
            {
                Spawn(); Move(origin + Aim(angle) * 2.2f);
                yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Windup, 2, "preview preparation");
                if (previewPhase == "windup") { yield return new WaitForSeconds(settings.windupSeconds * .6f); yield break; }
                yield return Wait(() => pattern.ActiveRemnant != null, 2, "preview release");
                Move(origin + Aim(angle + 90) * 3.5f);
                if (previewPhase == "burst")
                {
                    yield return Wait(() => pattern.BurstCount == 1, 2, "preview burst");
                    yield return new WaitForSeconds(art.burstFrameSeconds * 1.1f);
                }
                else
                {
                    yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Recovery, 2, "preview retreat endpoint");
                    yield return new WaitForSeconds(.05f); // Allow the interpolated visual to reach the physical endpoint.
                }
                yield break;
            }
            var catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath); var editor = Editor.CreateEditor(catalog);
            try
            {
                typeof(MonsterBalanceCatalogEditor).GetField("monsterIndex", Private).SetValue(editor, catalog.monsters.IndexOf(definition));
                var ui = editor.CreateInspectorGUI(); var paths = ui.Query<PropertyField>().ToList().Select(x => x.bindingPath).ToArray();
                Require(new[] { "retreatDistance", "retreatSpeed", "burstRadius", "remnantFrames", "deathFrames", "dangerFillColor" }.All(paths.Contains), "catalog exposes pattern and presentation sources");
            }
            finally { Object.DestroyImmediate(editor); }
            Pass("catalog Inspector separates base stats, pattern distances/times, damage coefficient and fixed-front presentation");

            foreach (float facing in new[] { 0f, 90f, 180f, 270f })
            {
                Spawn(); enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed, 1); Move(origin + Aim(facing) * 4);
                Vector3 start = enemy.GetComponent<Rigidbody>().position; yield return new WaitForSeconds(.3f);
                Require(Vector3.Dot(enemy.GetComponent<Rigidbody>().position - start, Aim(facing)) > .2f
                    && !Body().flipX && art.moveFrames.Contains(Body().sprite), "actual approach keeps front movement frames");
                enemy.TakeDamage(1); yield return null;
                Require(Body().sprite == art.hit && !Body().flipX, "moving hit remains front");
            }
            Pass("actual four-direction approach at base speed 1 and damage flinch keep front frames without mirroring");

            foreach (float direction in (battle ? BattleAngles : new[] { 0f, 90f, 180f, 270f }))
            {
                Spawn(); Move(origin + Aim(direction) * 2.2f); float created = Time.time;
                Require(!pattern.TryBeginAttack(), "spawn grace prevents immediate reservation");
                yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Windup, 2, "basic windup");
                Require(Time.time - created >= .73f && pattern.ReleaseCount == 0, ".75s spawn grace");
                Vector3 fixedCenter = pattern.RemnantCenter, committed = pattern.RetreatDirection; float began = Time.time;
                Equal(0, (committed + Aim(direction)).magnitude, "retreat away from initial target"); CheckMarker(1.6f);
                Move(origin - Aim(direction) * 2.2f); Require(!pattern.TryBeginAttack(), "one reservation during preparation");
                yield return Wait(() => pattern.ReleaseCount == 1, 2, "one remnant release");
                Require(Time.time - began >= .63f, ".65s windup"); Equal(0, (pattern.ActiveRemnant.transform.position - fixedCenter).magnitude, "remnant stays at original center");
                var seen = new HashSet<Sprite>(); float until = Time.time + 2;
                while (pattern.Phase == HangoverRemnantPhase.Retreat)
                { Require(Time.time < until && !Body().flipX, "retreat never mirrors front"); seen.Add(Body().sprite); yield return null; }
                Equal(0, (enemy.GetComponent<Rigidbody>().position - (origin + committed * 2.4f)).magnitude, "actual 2.4m retreat endpoint");
                Require(Time.time - pattern.ReleasedAt < .48f && art.retreatFrames.All(seen.Contains), "6m/s retreat and retreat pose");
                Equal(0, (pattern.RetreatDirection - committed).magnitude, "target motion cannot reverse retreat");
                var prop = pattern.ActiveRemnant.GetComponentInChildren<SpriteRenderer>(); yield return new WaitForEndOfFrame();
                Equal(biome.GetGroundHeight(fixedCenter), prop.transform.position.y, "residue remains grounded");
                Require(!Body().flipX && Body().sprite == art.recovery, "front recovery frame");
                Camera camera = DontStarveCamera.GetActiveCamera();
                var actorBounds = ScreenBounds(Body(), camera); var propBounds = ScreenBounds(prop, camera);
                Require(!actorBounds.Overlaps(propBounds), "retreated body does not conceal remnant");
                Move(fixedCenter); health.ResetHealth(); float hp = health.CurrentHealth;
                yield return Wait(() => pattern.BurstCount == 1, 2, "delayed single burst");
                Require(pattern.BurstAt - pattern.ReleasedAt >= 1.09f && pattern.BurstAt - pattern.ReleasedAt < 1.18f, "1.1 seconds from creation, not from retreat end");
                Equal(3, hp - health.CurrentHealth, "original burst damage once"); Require(pattern.TelegraphObject == null, "warning removed with hit");
                yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Ready, 2, "recovery and burst finish");
                Equal(3, hp - health.CurrentHealth, "no lingering damage");
                Require(pattern.ActiveRemnant == null && pattern.NextReadyTime - Time.time > 3.4f && pattern.NextReadyTime - Time.time <= 3.51f, "one 3.5s rearm after cleanup");
                Pass($"FLOW target={direction:0}deg: fixed front, .65s tell, original center, 2.4m/6mps retreat, visible grounded remnant, 1.1s single damage 3, cleanup and rearm");
            }

            foreach (float offset in new[] { -.025f, .025f })
            {
                Spawn(); Move(origin + Vector3.right * 2.2f); yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Windup, 2, "edge tell"); CheckMarker(1.6f);
                Move(origin + Vector3.forward * (settings.burstRadius + offset)); float hp = health.CurrentHealth;
                yield return Wait(() => pattern.BurstCount == 1, 3, "edge burst"); Equal(offset < 0 ? 3 : 0, hp - health.CurrentHealth, "circle edge matches player ground center");
                Pass($"EDGE radius=1.6m offset={offset:F3}m: actual damage={hp-health.CurrentHealth}, red fill and hit share one radius");
            }
            Spawn(); Move(origin + Vector3.right * 2.2f); yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Windup, 2, "old snapshot");
            Set(settings, "windupSeconds", .35f); Set(settings, "retreatDistance", 1.8f); Set(settings, "retreatSpeed", 3);
            Set(settings, "burstDelaySeconds", .8f); Set(settings, "burstRadius", 2);
            Set(definition, "statSets.Array.data[0].attackPower", 4);
            int damageIndex = definition.patternDamage.FindIndex(x => x.id == HangoverRemnantPatternSettings.DamageId);
            Set(definition, $"patternDamage.Array.data[{damageIndex}].coefficient", 2);
            Set(DifficultyBalanceService.GetProfile(GameDifficulty.Normal), "elites.outgoingDamage", 1.5f);
            Equal(.65f, pattern.WindupDuration, "old tell captured"); Equal(1.6f, pattern.BurstRadius, "old radius captured"); Equal(6, pattern.RetreatSpeed, "old retreat captured");
            Spawn(); Move(origin + Vector3.right * 2.2f); yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Windup, 2, "edited tell");
            Equal(.35f, pattern.WindupDuration, "new tell"); Equal(1.8f, pattern.RetreatDistance, "new distance"); Equal(3, pattern.RetreatSpeed, "new speed"); Equal(.8f, pattern.BurstDelay, "new delay"); CheckMarker(2);
            Move(origin + Vector3.right * 1.5f); float oldHp = health.CurrentHealth;
            yield return Wait(() => pattern.BurstCount == 1, 3, "edited hit"); Equal(12, oldHp - health.CurrentHealth, "edited damage 4 x 1.5 x 2");
            Equal(1.8f, Vector3.Distance(origin, enemy.GetComponent<Rigidbody>().position), "edited physical retreat distance"); Restore();
            Pass("Serialized Inspector edits reach next actor/tell/retreat/marker/delay/damage=12, old snapshot stays unchanged, originals restored");

            if (battle) yield return BattleChecks();
            yield return CleanupChecks();
            if (battle) yield return VisitChecks();
            Pass(battle ? "A-3 complete; production Liver list unchanged, isolated save and source assets restored" : "A-2 connection complete; full A-3 combat/terrain matrix uses its separate entry point");
        }
        private static void CheckMarker(float radius)
        {
            var renderer = pattern.TelegraphObject.GetComponentInChildren<MeshRenderer>(); var filter = renderer.GetComponent<MeshFilter>();
            Equal(radius, renderer.transform.localScale.x, "full warning radius from first frame"); Equal(radius, renderer.transform.localScale.z, "round warning radius");
            Require(filter.sharedMesh.triangles.Length == 96 * 3, "filled circle, not ring-only");
            var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block); var c = block.GetColor("_Color");
            Require(c.r > c.g * 3 && c.a > 0 && c.a < 1, "red translucent fill");
        }
        private static Rect ScreenBounds(SpriteRenderer renderer, Camera camera)
        {
            Bounds b = renderer.sprite.bounds; Vector2 min = Vector2.one * float.PositiveInfinity, max = Vector2.one * float.NegativeInfinity;
            foreach (float x in new[] { b.min.x, b.max.x }) foreach (float y in new[] { b.min.y, b.max.y })
            { Vector2 p = camera.WorldToScreenPoint(renderer.transform.TransformPoint(new Vector3(x, y, 0))); min = Vector2.Min(min, p); max = Vector2.Max(max, p); }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
        private static IEnumerator CleanupChecks()
        {
            Spawn(); Move(origin + Vector3.right * 2.2f); var wall = biome.WorldToGrid(origin - Vector3.right * 2);
            biome.AddRuntimeBlockedCells(new[] { wall });
            try { yield return new WaitForSeconds(1); Require(pattern.Phase == HangoverRemnantPhase.Ready && pattern.RejectedPaths > 0 && pattern.TelegraphObject == null && pattern.ReleaseCount == 0, "unsafe path cannot begin"); }
            finally { biome.RemoveRuntimeBlockedCells(new[] { wall }); }
            Pass("blocked retreat path rejects preparation with no warning, residue or damage");
            Spawn(); Move(origin + Vector3.right * 2.2f); yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Windup, 2, "stun tell");
            enemy.StatusEffects.ApplyStun(.4f); yield return new WaitForSeconds(.7f);
            Require(pattern.Phase == HangoverRemnantPhase.Ready && pattern.TelegraphObject == null && pattern.ReleaseCount == 0, "preparation stun cancels");
            Pass("preparation stun cancels warning and pending residue without a stuck cycle");
            Spawn(); Move(origin + Vector3.right * 2.2f); yield return Wait(() => pattern.ActiveRemnant != null, 2, "late wall release");
            var blocked = biome.WorldToGrid(origin - Vector3.right * 1.5f); biome.AddRuntimeBlockedCells(new[] { blocked });
            try
            {
                yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Recovery, 1, "late wall stop");
                Require(Vector3.Distance(origin, enemy.GetComponent<Rigidbody>().position) < 2.4f, "late obstruction stops retreat early");
                yield return Wait(() => pattern.BurstCount == 1, 2, "original residue timer after stop");
                Require(pattern.BurstAt-pattern.ReleasedAt >= 1.09f && pattern.BurstAt-pattern.ReleasedAt < 1.18f, "blocked retreat does not reset detonation clock");
            }
            finally { biome.RemoveRuntimeBlockedCells(new[] { blocked }); }
            Pass("new obstacle stops retreat safely; already-created remnant retains original detonation clock");
            foreach (bool afterRelease in new[] { false, true })
            {
                Spawn(); Move(origin + Vector3.forward * 2.2f); yield return Wait(() => pattern.Phase == HangoverRemnantPhase.Windup, 2, "death tell");
                if (afterRelease) yield return Wait(() => pattern.ActiveRemnant != null, 2, "death residue");
                var marker = pattern.TelegraphObject; var remnant = pattern.ActiveRemnant; var body = Body(); uint generation = enemy.SpawnGeneration;
                enemy.TakeDamage(10000); Require((marker == null || !marker.activeSelf) && (remnant == null || !remnant.activeSelf), "death effects hidden synchronously");
                var seen = new HashSet<Sprite>(); float until = Time.time + 2;
                while (enemy.gameObject.activeSelf) { Require(Time.time < until && !body.flipX, "fixed front death completes"); seen.Add(body.sprite); yield return null; }
                Require(settings.presentation.deathFrames.All(seen.Contains), "all six front death frames");
                Spawn(); Require(enemy.SpawnGeneration != generation && pattern.ReleaseCount == 0 && pattern.ActiveRemnant == null && !Body().flipX, "pooled clean front state");
                Pass(afterRelease ? "death after release immediately clears remnant/tell, six front death frames, clean pool reuse" : "death during preparation immediately clears tell, six front death frames, clean pool reuse");
            }
            Spawn(); Move(origin + Vector3.forward * 2.2f); yield return Wait(() => pattern.ActiveRemnant != null, 2, "pool residue");
            var prop = pattern.ActiveRemnant; enemy.ReleaseToPool(); Require(!prop.activeSelf, "pool return disables residue immediately"); yield return null;
            Pass("pool release removes owned residue and warning immediately");
            origin = fieldOrigin; field.enabled = true; Move(origin + Vector3.right * 5); field.Refresh(player.transform.position);
            enemy = field.Spawners.Single().ActiveEnemy; pattern = enemy.GetComponent<HangoverRemnantElitePattern>();
            var point = field.Plan.placements.Single(); int xp = 0; Action<int> onXp = amount => xp += amount; var level = LevelUpManager.OnLevelUp;
            LevelUpManager.OnExpGained += onXp; LevelUpManager.OnLevelUp = null;
            try { enemy.TakeDamage(10000); enemy.TakeDamage(10000); enemy.GrantExp(); }
            finally { LevelUpManager.OnExpGained -= onXp; LevelUpManager.OnLevelUp = level; }
            Equal(50, xp, "field reward once"); Require(SaveService.IsBiomeEliteDefeated(point.spawnId), "field defeat recorded");
            yield return Wait(() => !enemy.gameObject.activeSelf, 2, "field death release"); field.Refresh(player.transform.position);
            Require(field.Spawners.Count == 0 && biome.GetBiomeConfig().biomeEliteSpawnConfig.monsters.Count == productionLiverCount, "defeated temp point removed and production list unchanged");
            Pass("actual dedicated field death grants XP 50 once and records defeat; production Liver list unchanged");
        }
    }
}
