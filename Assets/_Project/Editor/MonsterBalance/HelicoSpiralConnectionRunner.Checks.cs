using System;
using System.Collections;
using System.Collections.Generic;
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
    public static partial class HelicoSpiralConnectionRunner
    {
        private static IEnumerator Checks()
        {
            GameManager.Instance.EnterBiome(BiomeType.Stomach); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_STOMACH); yield return null;
            player = PlayerController.Instance; health = player.HealthComponent;
            biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>(); Require(biome.BiomeType == BiomeType.Stomach, "real Stomach scene");
            groundOffset = player.transform.position.y - biome.GetGroundHeight(player.transform.position); Equal(-2, groundOffset, "player ground offset");
            field = biome.GetComponent<BiomeEliteField>(); Require(field.Plan == null, "unregistered production map");
            temporary = ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>(); temporary.minimumCount = temporary.maximumCount = 1; temporary.minimumPerType = 1; temporary.clearanceCells = 5;
            temporary.monsters.Add(HelicoSpiralSetup.PreviewRule(definition));
            field.Configure(biome, temporary, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Require(field.Plan.placements.Count == 1, "temporary dedicated placement at zero kills");
            var point = field.Plan.placements.Single(); fieldOrigin = biome.GridToWorldWithHeight(point.x, point.y);
            Move(fieldOrigin + Vector3.right * 5); field.Refresh(player.transform.position);
            enemy = field.Spawners.Single().ActiveEnemy; Require(enemy.GetComponent<HelicoSpiralElitePattern>() != null && enemy.Balance.ContactEnabled, "attached pattern and contact");
            Equal(30, enemy.Stats.MaxHealth, "base HP30"); Equal(50, enemy.Balance.Current.Experience, "base XP50");
            foreach (var s in field.Spawners) s.ReleaseEnemy(); field.enabled = false; enemy = null;
            if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
            ((IList)typeof(ProceduralBiomeBridge).GetField("normalEnemyRules", Private).GetValue(biome)).Clear();
            typeof(InputManager).GetMethod("SetActionsEnabled", Private).Invoke(InputManager.Instance, new object[] { false });
            bool found = false;
            for (int z = 10; z < biome.MapHeight - 10 && !found; z++) for (int x = 10; x < biome.MapWidth - 10 && !found; x++)
            {
                int level = biome.GetHeightLevel(x, z); bool clear = true;
                for (int dx = -8; dx <= 8 && clear; dx++) for (int dz = -8; dz <= 8 && clear; dz++) clear = biome.IsWalkable(x + dx, z + dz) && biome.GetHeightLevel(x + dx, z + dz) == level;
                if (clear) { origin = biome.GridToWorldWithHeight(x, z); found = true; }
            }
            Require(found, "flat Stomach patch"); PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth, 100, true);
            if (battleSection == "tail") { yield return TailChecks(); yield break; }
            if (preview)
            {
                if (battle) { yield return BattlePreview(); yield break; }
                Spawn(); Move(origin + Aim(angle) * 2.6f); yield return Wait(() => pattern.Phase == HelicoSpiralPhase.Windup, 2, "preview windup");
                if (phase == "windup") { yield return new WaitForSeconds(.38f); yield break; }
                if (phase != "hit") Move(origin - Aim(angle) * 2 + Aim(angle + 90) * 2.5f);
                yield return Wait(() => pattern.DashCount == 1, 2, "preview dash");
                if (phase == "death") { enemy.TakeDamage(10000); yield return new WaitForSeconds(.26f); yield break; }
                if (phase == "recovery") yield return Wait(() => pattern.Phase == HelicoSpiralPhase.Recovery, 1, "preview recovery");
                else if (phase == "hit") yield return Wait(() => pattern.DamageApplications == 1, 1, "preview damage");
                else yield return new WaitForSeconds(.075f);
                yield break;
            }
            if (battle && battleSection != "all") { yield return BattleChecks(battleSection); yield break; }
            Pass("SPAWN: actual Stomach temporary dedicated point, zero kills, HP30/XP50/contact on; production registration remains0");
            var catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath); var editor = Editor.CreateEditor(catalog);
            try
            {
                typeof(MonsterBalanceCatalogEditor).GetField("monsterIndex", Private).SetValue(editor, catalog.monsters.IndexOf(definition));
                var paths = editor.CreateInspectorGUI().Query<PropertyField>().ToList().Select(p => p.bindingPath).ToArray();
                Require(new[] { "windupSeconds", "dashDistance", "dashSeconds", "bodyWidth", "bodyHalfLength", "directionalPresentation", "dashFrames", "deathFrames" }.All(paths.Contains), "Inspector source separation");
                Require(!paths.Any(p => p.StartsWith("tail")), "retired tail fields absent");
            }
            finally { Object.DestroyImmediate(editor); }
            Require(!settings.tailSweepEnabled && !definition.patternDamage.Any(p => p.id == HelicoSpiralPatternSettings.TailDamageId), "retired tail source absent");
            Equal(18, settings.presentation.directionalPresentation.frames.Length, "active direction keys");
            Equal(90, settings.presentation.groundings.Length, "active groundings");
            var textures = settings.presentation.groundings.Select(g => AssetDatabase.GetAssetPath(g.sprite)).Distinct(); int spriteCount = 0;
            foreach (var path in textures)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                Require(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, "point/uncompressed imports");
                foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>())
                { Equal(Mathf.Round(sprite.pivot.x), sprite.pivot.x, "integer X pivot"); Equal(Mathf.Round(sprite.pivot.y), sprite.pivot.y, "integer Y pivot"); spriteCount++; }
            }
            Equal(90, spriteCount, "selected sprite count"); Pass("ART/INSPECTOR: 90 selected sprites,18 eight-direction keys, Point/no mip/no compression/integer pivots; base/pattern/art source fields and no B");
            foreach (float direction in new[] { 0f,45f,90f,135f,180f,225f,270f,315f })
            {
                Spawn(); enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth, 200, true); Move(origin + Aim(direction) * 2.6f); Require(!pattern.TryBeginAttack(), "spawn grace");
                yield return Wait(() => pattern.Phase == HelicoSpiralPhase.Windup, 2, "locked warning");
                var facing = enemy.PatternFacing; Require((int)facing == Mathf.RoundToInt(direction / 45), "correct eight-way view");
                CheckWarning(); yield return new WaitForEndOfFrame(); CheckGround(); var locked = pattern.LockedDirection; float began = Time.time;
                enemy.ApplyKnockback(Aim(direction), .5f); Equal(0, Vector3.Distance(origin, enemy.GetComponent<Rigidbody>().position), "windup fixed body");
                Move(origin - Aim(direction) * 2 + Aim(direction + 90) * 2.5f); health.ResetHealth(); float hp = health.CurrentHealth;
                while (pattern.DashCount == 0)
                {
                    Require(Time.time - began < 2, "release timeout");
                    yield return new WaitForEndOfFrame(); CheckGround();
                }
                Require(Time.time - began >= .66f, "full tell");
                while (pattern.Phase == HelicoSpiralPhase.Dash)
                {
                    Require(enemy.PatternFacing == facing && enemy.IsPatternFacingLocked, "dash facing stays committed");
                    yield return new WaitForEndOfFrame(); CheckGround();
                }
                Equal(3.5f, pattern.Travelled, "straight travel"); Equal(0, Vector3.Distance(pattern.DashEnd, enemy.GetComponent<Rigidbody>().position), "fixed endpoint");
                Equal(0, (locked-pattern.LockedDirection).magnitude, "no homing"); Equal(hp, health.CurrentHealth, "side dodge safe");
                Require(pattern.Phase == HelicoSpiralPhase.Recovery && pattern.TelegraphObject == null, "dash goes directly to recovery");
                Require(!pattern.TailEnabled && pattern.TailGeometry == null && enemy.GetComponent<HelicoTailSweepVisual>() == null, "no retired tail action, damage geometry or renderer allocated");
                yield return new WaitForEndOfFrame(); CheckGround();
                Move(pattern.DashEnd - Aim(direction) * 1.6f); typeof(PlayerController).GetField("movement", Private).SetValue(player, Vector3.zero);
                typeof(PlayerController).GetField("lastMoveDirection", Private).SetValue(player, Aim(direction)); float before = enemy.Stats.CurrentHealth;
                typeof(PlayerAttack).GetMethod("MeleeAttack", Private).Invoke(player.GetComponent<PlayerAttack>(), null);
                Require(enemy.Stats.CurrentHealth < before, "native Q can punish recovery"); Move(pattern.DashEnd + Aim(direction + 90) * 3);
                float recoveryUntil = Time.time + 2;
                while (pattern.Phase != HelicoSpiralPhase.Ready)
                {
                    Require(Time.time < recoveryUntil, "recovery timeout");
                    yield return new WaitForEndOfFrame();
                    if (pattern.Phase != HelicoSpiralPhase.Ready) CheckGround();
                }
                Require(!enemy.IsPatternFacingLocked && !enemy.IsPatternPositionLocked && pattern.NextReadyTime-Time.time>2.3f, "locks release and rearm once");
                Pass($"FLOW {direction:0}: correct view/head-first fixed 3.5m dash, full red capsule, grounded rendering, side dodge0, Q punish, recovery/rearm");
                Spawn(); Move(origin + Aim(direction) * 2.6f); yield return Wait(() => pattern.DamageApplications == 1, 3, "head-first actual hit");
                Equal(3, 100-health.CurrentHealth, "dash damage3"); Require(pattern.HitAttempts == 1, "one attempt per dash"); Move(origin + Aim(direction + 90)*4);
                yield return Wait(() => pattern.Phase == HelicoSpiralPhase.Recovery, 1, "hit recovery");
                Equal(3, 100-health.CurrentHealth, "no contact double charge"); Pass($"HIT {direction:0}: actual dash damage3 once; contact path does not double charge");
            }
            yield return EdgesAndSources();
            if (battle) yield return BattleChecks("all");
            yield return Lifecycle();
            if (battle) yield return VisitChecks();
            Pass(battle ? "A-3 COMPLETE: Stomach combat/terrain/lifecycle/visits verified; sources restored, production registration remains0" : "A-2 COMPLETE: sources restored, Stomach production remains0; A-3 full terrain/difficulty matrix remains separate");
        }
        private static void CheckWarning()
        {
            Require(pattern.TelegraphObject != null && pattern.TelegraphObject.GetComponentsInChildren<MeshRenderer>().Length == 3, "filled rectangle plus two circular ends");
            Equal(.8f, pattern.BodyRadius*2, "same body width"); Equal(3.5f, pattern.PlannedDistance, "open planned distance");
            Equal(biome.GetGroundHeight(origin)+.055f, pattern.TelegraphObject.transform.position.y, "red area grounded");
            Require(pattern.TelegraphObject.GetComponentsInChildren<Collider>().Length == 0, "warning has no collider");
            var border = pattern.TelegraphObject.GetComponentInChildren<LineRenderer>();
            Require(border != null && border.loop && border.positionCount == 66 && border.startColor.a > .8f, "visible closed boundary on red floor");
            float line = pattern.PlannedDistance + pattern.BodyHalfLength * 2;
            foreach (int i in new[] { 0, 16, 32, 33, 49, 65 })
            {
                var p = border.GetPosition(i);
                Equal(pattern.BodyRadius, Mathf.Sqrt(CombatHitGeometry.PointSegmentDistanceSquared(new Vector2(p.x, p.z), new Vector2(0, -line * .5f), new Vector2(0, line * .5f))), "outline follows exact footprint");
            }
        }
        private static void CheckGround()
        {
            var body=Body(); var camera=DontStarveCamera.GetActiveCamera(); var ground=enemy.GetComponent<Rigidbody>().position; ground.y=biome.GetGroundHeight(ground);
            var grounding = enemy.GetComponent<HelicoGroundedVisual>();
            Vector3 depth = Vector3.ProjectOnPlane(camera.transform.up, Vector3.up).normalized;
            Vector3 contact = ground - depth * (pattern.BodyHalfLength * Mathf.Abs(Vector3.Dot(pattern.BodyDirection, depth))) + Vector3.up * .025f;
            Vector3 renderedFoot = body.transform.TransformPoint(new Vector3(0, grounding.CurrentBodyBottomY, 0));
            Equal(0, Vector2.Distance(camera.WorldToViewportPoint(contact), camera.WorldToViewportPoint(renderedFoot)), "thick-body underside projects to attack-body ground axis");
            Require(camera.WorldToViewportPoint(body.transform.position).y > camera.WorldToViewportPoint(renderedFoot).y, "warning sits below the rendered body center");
            float lowest=float.PositiveInfinity;var b=body.sprite.bounds;
            foreach(float x in new[]{b.min.x,b.max.x})foreach(float y in new[]{b.min.y,b.max.y})lowest=Mathf.Min(lowest,body.transform.TransformPoint(new Vector3(x,y,0)).y);
            Require(lowest>=ground.y+.02f && lowest<=ground.y+.04f, "lowest sprite plane stays above floor: "+lowest);
            var head=enemy.GetPatternVisualOrigin(); var screenHead=(Vector2)camera.WorldToViewportPoint(head)-(Vector2)camera.WorldToViewportPoint(body.transform.position);
            var screenAim=(Vector2)camera.WorldToViewportPoint(ground+pattern.BodyDirection)-(Vector2)camera.WorldToViewportPoint(ground);
            Require(Vector2.Dot(screenHead,screenAim)>0, "visible head leads travel");
        }
    }
}
