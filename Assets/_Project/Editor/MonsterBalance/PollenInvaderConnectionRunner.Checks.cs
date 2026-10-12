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
    public static partial class PollenInvaderConnectionRunner
    {
        private static IEnumerator Checks()
        {
            var art = settings.presentation; var directions = art.directionalPresentation;
            var frames = directions.frames.SelectMany(f => new[] { f.source, f.front, f.back }).Concat(art.pelletFrames).ToArray();
            int expectedFrames = art.rotationFrames != null && art.rotationFrames.Length == 3 ? 59 : 50;
            Require(frames.Length == expectedFrames && frames.Distinct().Count() == expectedFrames, "distinct body/death/rotation/pellet sprites");
            foreach (var group in frames.GroupBy(AssetDatabase.GetAssetPath))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(group.Key);
                Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "pixel import settings");
                Require(File.ReadAllBytes(group.Key).SequenceEqual(File.ReadAllBytes(PollenInvaderSetup.SourceFolder + Path.GetFileName(group.Key))), "source PNG bytes");
                foreach (var f in group) { Equal(Mathf.Round(f.pivot.x), f.pivot.x, "integer X pivot"); Equal(Mathf.Round(f.pivot.y), f.pivot.y, "integer Y pivot"); }
            }
            Pass($"{expectedFrames} unchanged source sprites, Point/no mip/uncompressed, integer body/death/rotation/pellet pivots");
            GameManager.Instance.EnterBiome(BiomeType.Lung); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LUNG); yield return null;
            biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>(); field = biome.GetComponent<BiomeEliteField>();
            groundOffset = player.transform.position.y - biome.GetGroundHeight(player.transform.position);
            Require(field.Plan == null && productionLung.monsters.Count == productionLungCount, "production Lung isolated and unchanged");
            temporary = ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>(); temporary.minimumCount = temporary.maximumCount = 1; temporary.clearanceCells = 6;
            temporary.monsters.Add(PollenInvaderSetup.PreviewRule(definition));
            field.Configure(biome, temporary, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            var point = field.Plan.placements.Single(); fieldOrigin = origin = biome.GridToWorldWithHeight(point.x, point.y);
            Move(origin + Vector3.right * 5); field.Refresh(player.transform.position);
            enemy = field.Spawners.Single().ActiveEnemy; pattern = enemy.GetComponent<PollenInvaderElitePattern>();
            Require(pattern != null && enemy.IsElite && enemy.IsAiSuppressed && enemy.Balance.ContactEnabled, "dedicated zero-kill pattern hookup");
            Pass("actual Lung temporary dedicated spawner attaches P-03 at zero kills; contact enabled, production registration unchanged");
            if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
            field.enabled = false; enemy = null;
            ((IList)typeof(ProceduralBiomeBridge).GetField("normalEnemyRules", Private).GetValue(biome)).Clear();
            typeof(InputManager).GetMethod("SetActionsEnabled", Private).Invoke(InputManager.Instance, new object[] { false });
            bool found = false;
            for (int z = 8; z < biome.MapHeight - 8 && !found; z++) for (int x = 8; x < biome.MapWidth - 8 && !found; x++)
            {
                int level = biome.GetHeightLevel(x, z); bool clear = true;
                for (int dx = -7; dx <= 7 && clear; dx++) for (int dz = -7; dz <= 7 && clear; dz++)
                    clear = biome.IsWalkable(x + dx, z + dz) && biome.GetHeightLevel(x + dx, z + dz) == level;
                if (clear) { origin = biome.GridToWorldWithHeight(x, z); found = true; }
            }
            Require(found, "real flat Lung patch"); PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 60, true);
            if (extension) { if (preview) yield return FollowupPreview(); else yield return FollowupChecks(); yield break; }
            if (preview)
            {
                if (previewPhase == "slow") Set(settings, "pelletSpeed", .8f);
                if (previewPhase == "plateau") origin = FindPatch(1, 5);
                Spawn(); Move(origin + Aim(angle) * 4); yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 2, "preview tell");
                if (previewPhase == "windup") { yield return new WaitForSeconds(.6f); yield break; }
                Move(origin - Aim(angle) * 3 + Aim(angle + 90) * 2);
                yield return Wait(() => pattern.VolleyCount == 1, 2, "preview release");
                if (previewPhase == "death") { enemy.TakeDamage(10000); yield return new WaitForSeconds(.25f); yield break; }
                yield return new WaitForSeconds(previewPhase == "launch" ? .01f : previewPhase == "slow" ? .55f : .4f); yield break;
            }
            if (battle && battleSection != "all") { yield return BattleChecks(battleSection); yield break; }
            var catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath); var editor = Editor.CreateEditor(catalog);
            try
            {
                typeof(MonsterBalanceCatalogEditor).GetField("monsterIndex", Private).SetValue(editor, catalog.monsters.IndexOf(definition));
                var paths = editor.CreateInspectorGUI().Query<PropertyField>().ToList().Select(p => p.bindingPath).ToArray();
                Require(new[] { "pelletRadius", "pelletSpeed", "pelletLifetimeSeconds", "spreadHalfAngle", "directionalPresentation", "deathFrames" }.All(paths.Contains), "Inspector source fields");
                Require(paths.Contains("followup.enabled"), "explicit A/B content switch");
            }
            finally { Object.DestroyImmediate(editor); }
            Pass("Inspector separates common/A/B/art and base/difficulty/progression sources; this regression runs with B disabled");

            foreach (float direction in new[] { 0f, 90f, 180f, 270f })
            {
                Spawn(); Move(origin + Aim(direction) * 4); Require(!pattern.TryBeginAttack(), "spawn grace");
                yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 2, "directional tell");
                float began = Time.time; var locked = pattern.LockedDirection; var facing = enemy.PatternFacing;
                var intended = direction == 0 ? EnemyFacing.Right : direction == 90 ? EnemyFacing.Back : direction == 180 ? EnemyFacing.Left : EnemyFacing.Front;
                Require(facing == intended, "correct body facing " + facing);
                enemy.ApplyKnockback(Aim(direction), .4f); Equal(0, Vector3.Distance(enemy.GetComponent<Rigidbody>().position, origin), "stable firing body");
                Move(origin - Aim(direction) * 3 + Aim(direction + 90) * 2);
                yield return Wait(() => pattern.VolleyCount == 1, 2, "three-pellet release");
                Require(Time.time - began >= .82f && enemy.PatternFacing == facing && enemy.IsPatternFacingLocked, "tell and direction committed");
                Equal(0, (pattern.LockedDirection - locked).magnitude, "moving player does not retarget");
                Require(pattern.ActivePellets.Length == 3 && pattern.ActivePellets.All(p => p != null), "exactly three pellets");
                yield return new WaitForEndOfFrame();
                Require(pattern.ActivePellets.All(p => facing == EnemyFacing.Back ? !p.UsesOpeningSort : p.UsesOpeningSort && p.Body.sortingOrder > Body().sortingOrder), "opening visibility follows shell side");
                for (int i = 0; i < 3; i++)
                {
                    var pellet = pattern.ActivePellets[i]; var expected = Quaternion.AngleAxis((i - 1) * 30, Vector3.up) * locked;
                    Equal(0, Vector3.Distance(expected, pellet.Direction), "fan angle"); CheckPellet(pellet, .16f, 3.5f);
                    Equal(0, Vector3.Distance(pattern.LaunchPosition, pellet.LaunchPosition), "one opening shared by fan");
                }
                var camera = DontStarveCamera.GetActiveCamera();
                var aperture = camera.WorldToViewportPoint(pattern.OpeningPosition); var launch = camera.WorldToViewportPoint(pattern.LaunchPosition + Vector3.up * .16f);
                Require(Vector2.Distance(aperture, launch) < .001f, "low projectile joins visible aperture");
                var middle = pattern.ActivePellets[1]; yield return new WaitForSeconds(.18f); CheckPellet(middle, .16f, 3.5f);
                Require(Vector3.Distance(middle.LaunchPosition, middle.transform.position) > .5f, "actual projectile travel");
                Move(middle.transform.position + middle.Direction * 2.6f); health.ResetHealth(); float hp = health.CurrentHealth;
                yield return Wait(() => pattern.LastVolley.HitAttempts == 1, 1, "center pellet hit"); Equal(3, hp - health.CurrentHealth, "default damage3");
                yield return new WaitForSeconds(.55f);
                Require(pattern.ActivePellets.Where(p => p != null).All(p => !p.UsesOpeningSort), "muzzle-only sort returns to world order");
                var other = pattern.ActivePellets.FirstOrDefault(p => p != null && p.gameObject.activeSelf);
                Require(other != null, "another live pellet remains"); Move(other.transform.position + other.Direction * .6f);
                yield return Wait(() => other == null || !other.gameObject.activeSelf, 1, "second actual collision");
                Equal(3, hp - health.CurrentHealth, "shared volley cannot damage again after invulnerability");
                Require(pattern.LastVolley.HitAttempts == 1 && pattern.LastVolley.DamageApplications == 1, "one shared application");
                Move(origin + Aim(direction + 90) * 5); yield return Wait(() => pattern.Phase == PollenInvaderPhase.Ready, 2, "recovery");
                Require(!enemy.IsPatternFacingLocked && !enemy.IsPatternPositionLocked, "recovery releases locks");
                float remaining = pattern.NextReadyTime - Time.time;
                Require(remaining > 1.4f && remaining <= 2.51f, "single rearm after recovery");
                yield return Wait(() => enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, 3, "pellets expire");
                Pass($"FLOW {direction:0}deg: correct view/muzzle visibility, .85s locked tell, 3 x +/-30deg from aperture, .16m low flight, damage3 once including later pellet, world sorting restored, cleanup/rearm");
            }
            yield return GapAndEdges();
            yield return SourceEdits();
            if (battle) yield return BattleChecks("all");
            yield return CleanupAndContact();
            if (battle) yield return VisitChecks();
            Pass(battle ? "P-03-A-3 complete; production Lung unchanged, sources restored and isolated save cleaned" : regression ? "P-03-B A regression complete; original single-volley behavior retained, production Lung unchanged" : "P-03-A-2 complete; production Lung registration remains separate");
        }
        private static void CheckPellet(PollenPellet pellet, float radius, float speed)
        {
            Equal(radius, pellet.Radius, "pellet radius"); Equal(speed, pellet.Speed, "pellet speed");
            Equal(biome.GetGroundHeight(pellet.transform.position) + radius, pellet.Body.transform.position.y, "low flight height");
            Require(pellet.GetComponentsInChildren<SpriteRenderer>().Length == 1 && pellet.GetComponentsInChildren<MeshRenderer>().Length == 0, "simple pellet has no floor UI or secondary visual");
            var sprite = pellet.Body.sprite; var size = Vector3.Scale(sprite.bounds.size, pellet.Body.transform.lossyScale);
            Require(Mathf.Max(size.x, size.y) <= radius * 2.02f, "visual diameter follows radius");
        }
        private static IEnumerator GapAndEdges()
        {
            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 2, "gap tell");
            Move(origin - Vector3.right * 3); yield return Wait(() => pattern.VolleyCount == 1, 2, "gap release");
            Vector3 gap = Quaternion.AngleAxis(15, Vector3.up) * pattern.LockedDirection;
            Move(pattern.LaunchPosition + gap * 3.4f); health.ResetHealth(); float hp = health.CurrentHealth;
            yield return Wait(() => enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, 3, "gap pass"); Equal(hp, health.CurrentHealth, "actual player fits fan gap");
            Pass("GAP: actual player hurt capsule passes between two pellets at 3.4m without damage");
            foreach (float edge in new[] { -.025f, .025f })
            {
                Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 2, "edge tell");
                Move(origin - Vector3.right * 3); yield return Wait(() => pattern.VolleyCount == 1, 2, "edge release");
                var pellet = pattern.ActivePellets[0]; Vector3 side = Quaternion.AngleAxis(-90, Vector3.up) * pellet.Direction;
                Move(pellet.LaunchPosition + pellet.Direction * 2.3f);
                Require(CombatHitGeometry.TryCapsule(player.HitCollider, out var a, out var b, out float r), "player capsule");
                Vector2 axis = CombatHitGeometry.Flat(side); float reach = Mathf.Max(Vector2.Dot(a - CombatHitGeometry.Flat(player.transform.position), -axis), Vector2.Dot(b - CombatHitGeometry.Flat(player.transform.position), -axis)) + r;
                Move(player.transform.position + side * (reach + pellet.Radius + edge)); health.ResetHealth(); hp = health.CurrentHealth;
                yield return Wait(() => enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount == 0, 3, "edge flight");
                Equal(edge < 0 ? 3 : 0, hp - health.CurrentHealth, "rounded projectile boundary");
                Pass($"EDGE {edge:+.000;-.000}m: actual capsule and round pellet damage={(edge < 0 ? 3 : 0)}");
            }
            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.VolleyCount == 1, 3, "invulnerability release");
            var middle = pattern.ActivePellets[1]; Move(middle.transform.position + middle.Direction * 1.1f); health.ResetHealth(); hp = health.CurrentHealth; health.GrantTemporaryInvincibility(.5f);
            yield return Wait(() => pattern.LastVolley.HitAttempts == 1, 1, "protected hit"); Equal(hp, health.CurrentHealth, "invulnerability consumes volley harmlessly");
            yield return new WaitForSeconds(.55f); var other = pattern.ActivePellets.First(p => p != null && p.gameObject.activeSelf); Move(other.transform.position + other.Direction * .6f);
            yield return Wait(() => other == null || !other.gameObject.activeSelf, 1, "protected volley later hit"); Equal(hp, health.CurrentHealth, "no delayed damage after protection ends");
            Pass("INVULNERABILITY: first protected impact consumes volley; later pellet cannot hit after protection ends");
        }
        private static IEnumerator SourceEdits()
        {
            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 2, "snapshot tell");
            Set(settings, "windupSeconds", .4f); Set(settings, "spreadHalfAngle", 22); Set(settings, "pelletSpeed", 5); Set(settings, "pelletLifetimeSeconds", 1.5f); Set(settings, "pelletRadius", .22f);
            Set(definition, "statSets.Array.data[0].attackPower", 4); Set(definition, "patternDamage.Array.data[0].coefficient", 2);
            Set(DifficultyBalanceService.GetProfile(GameDifficulty.Normal), "elites.outgoingDamage", 1.5f);
            Equal(.85f, pattern.WindupDuration, "existing tell immutable"); Equal(.16f, pattern.PelletRadius, "existing radius immutable"); Equal(30, pattern.SpreadHalfAngle, "existing fan immutable");
            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.VolleyCount == 1, 2, "edited release");
            var pellet = pattern.ActivePellets[1]; Equal(.4f, pattern.WindupDuration, "edited windup"); Equal(22, pattern.SpreadHalfAngle, "edited spread"); Equal(1.5f, pellet.Duration, "edited lifetime"); CheckPellet(pellet, .22f, 5);
            Move(pellet.transform.position + pellet.Direction * 1.2f); health.ResetHealth(); float hp = health.CurrentHealth;
            yield return Wait(() => pattern.LastVolley.HitAttempts == 1, 1, "edited actual hit"); Equal(12, hp - health.CurrentHealth, "attack4 x difficulty1.5 x coefficient2");
            Restore();
            Pass("INSPECTOR: actual serialized source edits affect next spawn (.4s,22deg,speed5,1.5s,radius.22,damage12); prior spawn stable and originals restored");
        }
        private static IEnumerator CleanupAndContact()
        {
            Spawn(); pattern.EndSpawn(); Move(origin + Vector3.right * 1.6f); health.ResetHealth(); float hp = health.CurrentHealth;
            for (int i = 0; i < 35 && health.CurrentHealth == hp; i++) { player.TryMoveByWorld(Vector3.left * .035f); yield return new WaitForFixedUpdate(); }
            Equal(1, hp - health.CurrentHealth, "native body contact"); Pass("CONTACT: actual walk into body deals1; original base attack/contact coefficient retained");
            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 2, "stun tell"); enemy.StatusEffects.ApplyStun(.5f);
            yield return new WaitForSeconds(1); Require(pattern.VolleyCount == 0 && pattern.Phase == PollenInvaderPhase.Ready && !enemy.IsPatternPositionLocked, "stun cancellation");
            Pass("STUN: preparation cancels without projectiles and releases body/facing lock");
            Spawn(); Move(origin + Vector3.right * 4); yield return Wait(() => pattern.Phase == PollenInvaderPhase.Windup, 2, "wall tell");
            var wall = biome.WorldToGrid(pattern.LaunchPosition); biome.AddRuntimeBlockedCells(new[] { wall });
            try { yield return new WaitForSeconds(1); Require(pattern.VolleyCount == 0, "new wall blocks release"); }
            finally { biome.RemoveRuntimeBlockedCells(new[] { wall }); }
            Pass("WALL: blocked opening during tell creates no hidden projectile");
            foreach (float direction in new[] { 0f, 90f, 180f, 270f })
            {
                Spawn(); Move(origin + Aim(direction) * 4); yield return Wait(() => pattern.VolleyCount == 1, 3, "death release");
                var flying = pattern.ActivePellets.ToArray(); var body = Body(); var bank = settings.presentation.directionalPresentation.Capture(); var facing = enemy.PatternFacing;
                var expected = enemy.Config.deathSprites.Select(f => bank.Resolve(f, facing)).ToArray(); uint generation = enemy.SpawnGeneration;
                enemy.TakeDamage(10000); Require(flying.All(p => p == null || !p.gameObject.activeSelf), "death clears all pellets immediately");
                Move(origin); health.ResetHealth(); hp = health.CurrentHealth; var seen = new HashSet<Sprite>(); float until = Time.time + 2;
                while (enemy.gameObject.activeSelf) { Require(Time.time < until, "death timeout"); seen.Add(body.sprite); yield return null; }
                Require(expected.All(seen.Contains), "all six directional death frames"); Equal(hp, health.CurrentHealth, "no post-death damage");
                Spawn(); Require(enemy.SpawnGeneration != generation && pattern.VolleyCount == 0 && pattern.LastVolley == null && !enemy.IsPatternPositionLocked, "clean pool reuse");
                Pass($"DEATH {direction:0}deg: six matching frames, instant pellet cleanup, no late damage, clean pool generation");
            }
            enemy.ReleaseToPool(); origin = fieldOrigin; field.enabled = true; Move(origin + Vector3.right * 5); field.Refresh(player.transform.position);
            enemy = field.Spawners.Single().ActiveEnemy; var point = field.Plan.placements.Single(); int xp = 0; Action<int> observe = value => xp += value; var callback = LevelUpManager.OnLevelUp;
            LevelUpManager.OnLevelUp = null; LevelUpManager.OnExpGained += observe;
            try { enemy.TakeDamage(10000); enemy.TakeDamage(10000); enemy.GrantExp(); }
            finally { LevelUpManager.OnLevelUp = callback; LevelUpManager.OnExpGained -= observe; }
            Equal(50, xp, "dedicated XP once"); Require(SaveService.IsBiomeEliteDefeated(point.spawnId), "field records defeat");
            yield return Wait(() => !enemy.gameObject.activeSelf, 2, "field death finishes"); field.Refresh(player.transform.position);
            Require(field.Spawners.Count == 0 && productionLung.monsters.Count == productionLungCount, "dead temporary point absent, production unchanged");
            Pass("FIELD: actual dedicated zero-kill spawn gives XP50 once and records defeat; production Lung unchanged");
        }
    }
}
