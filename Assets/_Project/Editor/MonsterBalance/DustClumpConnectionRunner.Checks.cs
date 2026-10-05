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
    public static partial class DustClumpConnectionRunner
    {
        private static IEnumerator Checks()
        {
            var art = settings.presentation;
            var frames = art.idleFrames.Concat(art.moveFrames).Concat(art.preparationFrames).Concat(new[] { art.release, art.recovery, art.hit })
                .Concat(art.coreFrames).Concat(art.deathFrames).Concat(art.coreDeathFrames).Concat(art.cloudFrames).Concat(art.dissolveFrames).ToArray();
            Require(frames.Length == 30 && frames.Distinct().Count() == 30, "30 distinct approved sprites");
            foreach (var group in frames.GroupBy(AssetDatabase.GetAssetPath))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(group.Key);
                Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "pixel import settings");
                Require(File.ReadAllBytes(group.Key).SequenceEqual(File.ReadAllBytes(DustClumpSetup.SourceFolder + Path.GetFileName(group.Key))), "PNG bytes preserved");
                foreach (var f in group) { Equal(Mathf.Round(f.pivot.x), f.pivot.x, "integer X pivot"); Equal(Mathf.Round(f.pivot.y), f.pivot.y, "integer Y pivot"); }
            }
            Pass("30 unchanged approved sprites, Point/no mip/uncompressed, integer pivots including exposed-core ground anchors");
            GameManager.Instance.EnterBiome(BiomeType.Lung); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LUNG); yield return null;
            biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>(); field = biome.GetComponent<BiomeEliteField>();
            groundOffset = player.transform.position.y - biome.GetGroundHeight(player.transform.position);
            Require(field.Plan == null && productionLung.monsters.Count == productionLungCount, "production Lung isolated and unchanged");
            temporary = ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>(); temporary.minimumCount = temporary.maximumCount = 1; temporary.clearanceCells = 6;
            temporary.monsters.Add(DustClumpSetup.PreviewRule(definition));
            field.Configure(biome, temporary, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            var point = field.Plan.placements.Single(); fieldOrigin = origin = biome.GridToWorldWithHeight(point.x, point.y);
            Move(origin + Vector3.right * 5); field.Refresh(player.transform.position);
            enemy = field.Spawners.Single().ActiveEnemy; pattern = enemy.GetComponent<DustClumpElitePattern>();
            Require(pattern != null && enemy.IsElite && enemy.IsAiSuppressed && enemy.Balance.ContactEnabled, "dedicated zero-kill pattern hookup");
            Pass("actual Lung temporary dedicated spawner attaches P-01 at zero kills; contact enabled, production registration unchanged");
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
            if (preview)
            {
                Spawn(); Move(origin + Aim(angle) * 3.6f); yield return Wait(() => pattern.Phase == DustClumpPhase.Windup, 2, "preview windup");
                if (previewPhase == "windup") { yield return new WaitForSeconds(settings.windupSeconds * .15f); yield break; }
                if (previewPhase == "gather") { yield return new WaitForSeconds(settings.windupSeconds * .7f); yield break; }
                yield return Wait(() => pattern.ActiveCloud != null, 2, "preview cloud");
                Move(origin - Aim(angle) * 2.5f + Aim(angle + 90) * 2);
                if (previewPhase == "death") { enemy.TakeDamage(10000); yield return new WaitForSeconds(.2f); yield break; }
                yield return new WaitForSeconds(.65f);
                if (previewPhase == "occlusion")
                {
                    Move(pattern.ActiveCloud.transform.position + Vector3.forward * .35f); health.GrantTemporaryInvincibility(10);
                    var other = new GameObject("P01_OtherWarningVisualProbe"); other.transform.position = pattern.ActiveCloud.transform.position + Vector3.left * .6f + Vector3.up * .06f;
                    EnemyGroundTelegraph.Circle(other.transform, .8f, new Color(.95f, .03f, .03f, .35f));
                    enemy.GetComponent<EnemyPatternLifetime>().Own(other, go => { go.SetActive(false); Object.Destroy(go); });
                    yield return new WaitForSeconds(.12f);
                }
                yield break;
            }
            if (battle && battleSection != "all") { yield return BattleChecks(battleSection); yield break; }
            var catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath); var editor = Editor.CreateEditor(catalog);
            try
            {
                typeof(MonsterBalanceCatalogEditor).GetField("monsterIndex", Private).SetValue(editor, catalog.monsters.IndexOf(definition));
                var paths = editor.CreateInspectorGUI().Query<PropertyField>().ToList().Select(p => p.bindingPath).ToArray();
                Require(new[] { "cloudRadius", "cloudSpeed", "cloudLifetimeSeconds", "coreDeathFrames", "coreColliderSize" }.All(paths.Contains), "Inspector separates pattern and presentation");
            }
            finally { Object.DestroyImmediate(editor); }
            Pass("Inspector exposes separate pattern radius/speed/lifetime and presentation/core collider/death sources");

            foreach (float direction in (battle ? BattleAngles : new[] { 0f, 90f, 180f, 270f }))
            {
                Spawn(); Move(origin + Aim(direction) * 3.6f); Require(!pattern.TryBeginAttack(), "spawn grace");
                yield return Wait(() => pattern.Phase == DustClumpPhase.Windup, 2, "four-direction preparation");
                var fixedCore = pattern.CoreCenter; Vector3 locked = pattern.LockedDirection; float began = Time.time;
                Equal(0, (locked - Aim(direction)).magnitude, "committed direction"); CheckMarker(1.1f);
                enemy.ApplyKnockback(Aim(direction + 90), 1); Equal(0, Vector3.Distance(enemy.GetComponent<Rigidbody>().position, fixedCore), "preparation core locked");
                float enemyHp = enemy.Stats.CurrentHealth; enemy.TakeDamage(1); Equal(1, enemyHp - enemy.Stats.CurrentHealth, "locked core still vulnerable");
                Move(origin - Aim(direction) * 3.6f);
                yield return Wait(() => pattern.ActiveCloud != null, 2, "one cloud release");
                var cloud = pattern.ActiveCloud;
                Require(Time.time - began >= .88f && pattern.ReleaseCount == 1 && pattern.CoreExposed, ".9s windup and one exposed core");
                Require(!Body().flipX && !enemy.HasPatternDirections && enemy.IsPatternPositionLocked, "directionless body remains locked");
                Equal(.55f, enemy.GetComponent<BoxCollider>().size.x, "small core collider"); Equal(.55f, enemy.Config.colliderSize.x, "offensive geometry matches core");
                Equal(0, (cloud.Direction - locked).magnitude, "moving target cannot retarget cloud"); CheckMarker(1.1f);
                yield return new WaitForSeconds(battle ? .7f : .15f);
                if (battle) { yield return new WaitForEndOfFrame(); Require(!ScreenBounds(Body()).Overlaps(ScreenBounds(cloud.Body)), "separated cloud cannot conceal core"); }
                Equal(biome.GetGroundHeight(cloud.transform.position), cloud.Body.transform.position.y, "cloud bottom is on local ground");
                Equal(0, Vector3.Distance(pattern.TelegraphObject.transform.position, cloud.transform.position + Vector3.up * .055f), "moving marker follows cloud");
                Equal(0, Vector3.Distance(enemy.GetComponent<Rigidbody>().position, fixedCore), "body did not chase cloud");
                Move(cloud.LaunchPosition + Aim(direction) * 2); health.ResetHealth(); float hp = health.CurrentHealth;
                yield return Wait(() => cloud.HitAttempts == 1, 2, "moving area actually hits player"); Equal(3, hp - health.CurrentHealth, "cloud damage once");
                float until = Time.time + .65f;
                while (Time.time < until) { Move(cloud.transform.position); yield return new WaitForFixedUpdate(); }
                Equal(3, hp - health.CurrentHealth, "overlapping after hurt invulnerability does not tick again"); Require(cloud.DamageApplications == 1 && cloud.HitAttempts == 1, "one attempt/application");
                Move(origin + Aim(direction + 90) * 4);
                yield return Wait(() => pattern.Phase == DustClumpPhase.Ready, 3, "cloud cleanup then recovery");
                Require(pattern.ActiveCloud == null && pattern.TelegraphObject == null && !pattern.CoreExposed && !enemy.IsPatternPositionLocked, "full cleanup and restore");
                Equal(1.4f, enemy.GetComponent<BoxCollider>().size.x, "full collider restored");
                Require(pattern.NextReadyTime - Time.time > 3.4f && pattern.NextReadyTime - Time.time <= 3.51f, "one rearm after cloud and recovery");
                enemy.ApplyKnockback(Aim(direction), .4f); Equal(.4f, Vector3.Distance(fixedCore, enemy.GetComponent<Rigidbody>().position), "ready actor accepts normal knockback");
                Pass($"FLOW {direction:0}deg: fixed vulnerable core, .9s tell, grounded one-cloud hit3 once, moving full red area, core collider .55m, cleanup/rearm and knockback restored");
            }

            foreach (float edge in new[] { -.025f, .025f })
            {
                Spawn(); Move(origin + Vector3.right * 3.6f); yield return Wait(() => pattern.ActiveCloud != null, 3, "edge cloud"); var cloud = pattern.ActiveCloud;
                Move(cloud.LaunchPosition + Vector3.right * 1.3f + Vector3.forward * (1.1f + edge)); health.ResetHealth(); float hp = health.CurrentHealth;
                yield return Wait(() => cloud.IsDissolving, 3, "edge crossing finished"); Equal(edge < 0 ? 3 : 0, hp - health.CurrentHealth, "red radius uses player ground center");
                Require(!pattern.TelegraphObject.activeSelf, "dissolving cloud has no damage marker");
                Pass($"EDGE {edge:+.000;-.000}m: moving circle actual damage={(edge < 0 ? 3 : 0)}; no inflated player-radius hit");
            }
            yield return CoreContactAndInspector();
            if (battle) yield return BattleChecks("all");
            yield return CleanupChecks();
            if (battle) yield return VisitChecks();
            Pass(battle ? "A-3 complete; production Lung unchanged, original tuning restored and isolated save cleaned" : "A-2 basic connection complete; full P-01-A-3 matrix and Lung production registration remain separate approval stages");
        }

        private static void CheckMarker(float radius)
        {
            var renderer = pattern.TelegraphObject.GetComponentInChildren<MeshRenderer>(); var filter = renderer.GetComponent<MeshFilter>();
            Equal(radius, renderer.transform.localScale.x, "full radius from preparation start"); Equal(radius, renderer.transform.localScale.z, "round footprint");
            Require(filter.sharedMesh.triangles.Length == 96 * 3, "filled circle"); var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block); var c = block.GetColor("_Color");
            Require(c.r > c.g * 3 && c.a > 0 && c.a < 1, "red translucent full area");
        }
        private static IEnumerator CoreContactAndInspector()
        {
            Spawn(); Move(origin + Vector3.right * 3.6f); yield return Wait(() => pattern.ActiveCloud != null, 3, "core contact fixture");
            Move(origin + Vector3.left * .95f); health.ResetHealth(); float hp = health.CurrentHealth;
            yield return new WaitForSeconds(.18f); Equal(hp, health.CurrentHealth, "empty former shell does not deal contact");
            for (int i = 0; i < 20 && health.CurrentHealth == hp; i++) { player.TryMoveByWorld(Vector3.right * .04f); yield return new WaitForFixedUpdate(); }
            Equal(1, hp - health.CurrentHealth, "actual native small-core collision");
            Move(origin + Vector3.left * 1.1f); typeof(PlayerController).GetField("movement", Private).SetValue(player, Vector3.zero);
            typeof(PlayerController).GetField("lastMoveDirection", Private).SetValue(player, Vector3.right); Physics.SyncTransforms(); float enemyHp = enemy.Stats.CurrentHealth;
            typeof(PlayerAttack).GetMethod("MeleeAttack", Private).Invoke(player.GetComponent<PlayerAttack>(), null);
            Require(enemy.Stats.CurrentHealth < enemyHp, "native Q can punish exposed core"); Equal(0, Vector3.Distance(origin, enemy.GetComponent<Rigidbody>().position), "native attack cannot drag core");
            Pass("CORE: no contact in empty shell at .95m, walking into small core deals1, native Q damages fixed exposed core");

            Spawn(); Move(origin + Vector3.right * 3.6f); yield return Wait(() => pattern.Phase == DustClumpPhase.Windup, 2, "snapshot fixture");
            Set(settings, "windupSeconds", .45f); Set(settings, "cloudRadius", .8f); Set(settings, "cloudSpeed", 3); Set(settings, "cloudLifetimeSeconds", 1.2f);
            Set(definition, "statSets.Array.data[0].attackPower", 4); Set(definition, "patternDamage.Array.data[0].coefficient", 2);
            Set(DifficultyBalanceService.GetProfile(GameDifficulty.Normal), "elites.outgoingDamage", 1.5f); Set(settings.presentation, "coreColliderSize.x", .45f);
            Equal(.9f, pattern.WindupDuration, "existing tell snapshot"); Equal(1.1f, pattern.CloudRadius, "existing radius snapshot");
            Spawn(); Move(origin + Vector3.right * 3.6f); yield return Wait(() => pattern.ActiveCloud != null, 2, "edited cloud"); var edited = pattern.ActiveCloud;
            Equal(.45f, pattern.WindupDuration, "edited tell"); Equal(.8f, edited.Radius, "edited area"); Equal(3, edited.Speed, "edited motion"); Equal(1.2f, pattern.CloudLifetime, "edited duration");
            Equal(.45f, enemy.GetComponent<BoxCollider>().size.x, "edited core collider"); CheckMarker(.8f);
            Move(edited.LaunchPosition + Vector3.right * 1.2f); health.ResetHealth(); float oldHp = health.CurrentHealth;
            yield return Wait(() => edited.HitAttempts == 1, 2, "edited actual hit"); Equal(12, oldHp - health.CurrentHealth, "attack4 x difficulty1.5 x coefficient2"); Restore();
            Pass("INSPECTOR: next spawn captures tell .45s/radius .8m/speed3/lifetime1.2/core width .45 and actual damage12; previous snapshot stable, original sources restored");
        }
        private static IEnumerator CleanupChecks()
        {
            Spawn(); Move(origin + Vector3.right * 3.6f); var wall = biome.WorldToGrid(origin + Vector3.right * 1.2f); biome.AddRuntimeBlockedCells(new[] { wall });
            try { yield return new WaitForSeconds(1); Require(pattern.ReleaseCount == 0 && pattern.TelegraphObject == null && pattern.Phase == DustClumpPhase.Ready, "blocked start cannot reserve"); }
            finally { biome.RemoveRuntimeBlockedCells(new[] { wall }); }
            Pass("blocked launch area cannot create a warning or hidden cloud");
            Spawn(); Move(origin + Vector3.right * 3.6f); yield return Wait(() => pattern.Phase == DustClumpPhase.Windup, 2, "stun fixture");
            enemy.StatusEffects.ApplyStun(.4f); yield return new WaitForSeconds(1);
            Require(pattern.Phase == DustClumpPhase.Ready && pattern.ReleaseCount == 0 && pattern.TelegraphObject == null && !enemy.IsPatternPositionLocked, "stun cancels tell and lock");
            Pass("preparation stun cancels marker/gather/cloud reservation and releases position lock");
            foreach (bool exposed in new[] { false, true })
            {
                Spawn(); Move(origin + Vector3.forward * 3.6f); yield return Wait(() => exposed ? pattern.ActiveCloud != null : pattern.Phase == DustClumpPhase.Windup, 3, "death fixture");
                var cloud = pattern.ActiveCloud; var marker = pattern.TelegraphObject; var body = Body(); var frames = enemy.Config.deathSprites; uint generation = enemy.SpawnGeneration;
                Require(frames.All(f => f.name.Contains(exposed ? "CoreDeath" : "Clump_Death")), "correct state-specific death selected");
                enemy.TakeDamage(10000);
                Require((cloud == null || !cloud.gameObject.activeSelf) && (marker == null || !marker.activeSelf), "owner death removes threats synchronously");
                Move(origin); health.ResetHealth(); float hp = health.CurrentHealth; var seen = new HashSet<Sprite>(); float until = Time.time + 2;
                while (enemy.gameObject.activeSelf) { Require(Time.time < until && !body.flipX, "death finishes without mirroring"); seen.Add(body.sprite); yield return null; }
                Require(frames.All(seen.Contains), "all six death frames displayed"); Equal(hp, health.CurrentHealth, "no post-death cloud/body damage");
                Spawn(); Require(enemy.SpawnGeneration != generation && !enemy.IsPatternPositionLocked && !pattern.CoreExposed && pattern.ReleaseCount == 0, "clean pool generation and lock");
                Pass(exposed ? "DEATH exposed core: six core-only frames, instant area removal, no late hit, clean pool reuse" : "DEATH full cluster: six collapse frames, instant warning removal, clean pool reuse");
            }
            Spawn(); Move(origin + Vector3.right * 3.6f); yield return Wait(() => pattern.ActiveCloud != null, 3, "pool fixture");
            var live = pattern.ActiveCloud; enemy.ReleaseToPool(); Require(!live.gameObject.activeSelf && !enemy.IsPatternPositionLocked, "pool clears cloud and position lock"); yield return null;
            Pass("pool return clears active cloud, red marker, movement lock and owned objects");
            origin = fieldOrigin; field.enabled = true; Move(origin + Vector3.right * 5); field.Refresh(player.transform.position);
            enemy = field.Spawners.Single().ActiveEnemy; var point = field.Plan.placements.Single(); int xp = 0; Action<int> observe = value => xp += value; var callback = LevelUpManager.OnLevelUp;
            LevelUpManager.OnLevelUp = null; LevelUpManager.OnExpGained += observe;
            try { enemy.TakeDamage(10000); enemy.TakeDamage(10000); enemy.GrantExp(); }
            finally { LevelUpManager.OnLevelUp = callback; LevelUpManager.OnExpGained -= observe; }
            Equal(50, xp, "dedicated XP once"); Require(SaveService.IsBiomeEliteDefeated(point.spawnId), "field records defeat");
            yield return Wait(() => !enemy.gameObject.activeSelf, 2, "field death finishes"); field.Refresh(player.transform.position);
            Require(field.Spawners.Count == 0 && productionLung.monsters.Count == productionLungCount, "dead temporary point absent, production unchanged");
            Pass("actual dedicated field death gives XP50 once and records defeat; production Lung list unchanged");
        }
    }
}
