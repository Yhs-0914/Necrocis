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
    public static partial class OilFilmConnectionRunner
    {
        private static IEnumerator Checks()
        {
            GameManager.Instance.EnterBiome(BiomeType.Stomach); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_STOMACH); yield return null;
            player = PlayerController.Instance; health = player.HealthComponent; biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>();
            Require(biome.BiomeType == BiomeType.Stomach, "actual Stomach scene"); groundOffset = player.transform.position.y - biome.GetGroundHeight(player.transform.position);
            field = biome.GetComponent<BiomeEliteField>(); Require(field.Plan == null, "production map unregistered");
            temporary = ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>(); temporary.minimumCount = temporary.maximumCount = 1; temporary.minimumPerType = 1; temporary.clearanceCells = 5;
            temporary.monsters.Add(OilFilmSetup.PreviewRule(definition)); field.Configure(biome, temporary, biome.GetBiomeConfig().GetMidBossArenaConfig(), biome.GetBiomeConfig().GetReturnPortalConfig());
            Require(field.Plan.placements.Count == 1, "dedicated point at zero kills"); var point = field.Plan.placements.Single(); fieldOrigin = biome.GridToWorldWithHeight(point.x, point.y);
            Move(fieldOrigin + Vector3.right * 5); field.Refresh(player.transform.position); enemy = field.Spawners.Single().ActiveEnemy;
            Require(enemy.GetComponent<OilFilmElitePattern>() != null && enemy.Balance.ContactEnabled, "pattern attached and body contact enabled");
            Equal(30, enemy.Stats.MaxHealth, "base HP30"); Equal(50, enemy.Balance.Current.Experience, "base XP50");
            foreach (var spawner in field.Spawners) spawner.ReleaseEnemy(); field.enabled = false; enemy = null;
            if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
            ((IList)typeof(ProceduralBiomeBridge).GetField("normalEnemyRules", Private).GetValue(biome)).Clear();
            typeof(InputManager).GetMethod("SetActionsEnabled", Private).Invoke(InputManager.Instance, new object[] { false });
            bool found = false;
            for (int z = 10; z < biome.MapHeight - 10 && !found; z++) for (int x = 10; x < biome.MapWidth - 10 && !found; x++)
            {
                int level = biome.GetHeightLevel(x,z); bool clear = true;
                for (int dx = -8; dx <= 8 && clear; dx++) for (int dz = -8; dz <= 8 && clear; dz++) clear = biome.IsWalkable(x+dx,z+dz) && biome.GetHeightLevel(x+dx,z+dz) == level;
                if (clear) { origin = biome.GridToWorldWithHeight(x,z); found = true; }
            }
            Require(found, "flat patch"); PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth,100,true);
            if (phase.StartsWith("a3:")) { yield return BattleChecks(phase.Substring(3)); yield break; }
            if (phase == "motion") { yield return RecordMotion(); yield break; }
            if (preview)
            {
                Spawn(); Move(origin + Aim(angle) * 1.6f); yield return Wait(() => pattern.Phase == OilFilmPhase.Windup, 3, "preview warning");
                Move(origin + Aim(angle + 100) * 2);
                if (phase == "windup") { yield return new WaitForSeconds(.35f); yield return new WaitForEndOfFrame(); Capture("preview-windup"); yield break; }
                yield return Wait(() => pattern.ImpactCount == 1,3,"preview impact"); yield return new WaitForEndOfFrame();
                if (phase == "death") { enemy.TakeDamage(10000); yield return new WaitForSeconds(.3f); }
                Capture("preview-"+phase); yield break;
            }
            Pass("SPAWN: actual Stomach dedicated point, zero kills, HP30/XP50 and contact enabled; production remains0");
            var catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath); var editor = Editor.CreateEditor(catalog);
            try
            {
                typeof(MonsterBalanceCatalogEditor).GetField("monsterIndex", Private).SetValue(editor,catalog.monsters.IndexOf(definition));
                var paths = editor.CreateInspectorGUI().Query<PropertyField>().ToList().Select(p=>p.bindingPath).ToArray();
                Require(new[]{"attackRange","arcDegrees","impactNormalizedTime","coreRadius","preparationFrames","deathFrames"}.All(paths.Contains),"Inspector source separation");
                Require(!paths.Any(p=>p.Contains("safeDistance")||p.Contains("innerRadius")||p.Contains("rimWidth")),"no retired inner-safe or rim-width fields");
            }
            finally { Object.DestroyImmediate(editor); }
            int spriteCount = 0;
            foreach (string path in settings.presentation.profiles.Select(p=>AssetDatabase.GetAssetPath(p.sprite)).Distinct())
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);Require(importer.filterMode==FilterMode.Point&&!importer.mipmapEnabled&&importer.textureCompression==TextureImporterCompression.Uncompressed,"point sprite imports");
                foreach(var sprite in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()){Equal(Mathf.Round(sprite.pivot.x),sprite.pivot.x,"integer X pivot");Equal(Mathf.Round(sprite.pivot.y),sprite.pivot.y,"integer Y pivot");spriteCount++;}
            }
            Equal(72,spriteCount,"72 sprites");Equal(24,settings.presentation.directionalPresentation.frames.Length,"24 direction keys");
            Pass("ART/INSPECTOR: 72 unchanged sprites,24 four-direction keys, Point/no mip/uncompressed/integer pivots; full-area controls separated from stats/art/contact");
            foreach(float direction in new[]{0f,45f,90f,135f,180f,225f,270f,315f})
            {
                Spawn(); Move(origin+Aim(direction)*1.6f); Require(!pattern.TryBeginAttack(),"spawn grace");
                yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"warning");var facing=enemy.PatternFacing;var locked=pattern.LockedDirection;
                Require(pattern.Footprint.Contains(player.transform.position),"diagonal target belongs to committed sector");CheckWarning();
                enemy.ApplyKnockback(Aim(direction),.5f);Equal(0,Vector3.Distance(origin,pattern.Position),"core fixed during preparation");
                if(direction%90==0){yield return new WaitForEndOfFrame();Capture("warning-"+direction.ToString("000"));}
                yield return Wait(()=>pattern.ImpactCount==1,3,"impact");yield return new WaitForEndOfFrame();
                Require(enemy.PatternFacing==facing&&enemy.IsPatternFacingLocked&&enemy.IsPatternPositionLocked,"facing and core locked");
                Equal(0,Vector3.Distance(origin,pattern.Position),"fixed nucleus");Equal(0,Vector3.Distance(locked,pattern.LockedDirection),"no tracking");
                Equal(3,100-health.CurrentHealth,"actual full-area damage3");Equal(1,pattern.HitAttempts,"one impact attempt");Require(pattern.TelegraphObject==null,"warning ends at impact");
                CheckSurface();if(direction%90==0)Capture("impact-"+direction.ToString("000"));
                Move(origin+Aim(direction+100)*3);health.ResetHealth();
                yield return Wait(()=>pattern.Phase==OilFilmPhase.Recovery,1,"recovery");
                Move(origin+locked*1.4f);yield return new WaitForSeconds(.4f);Equal(100,health.CurrentHealth,"no recovery damage");Move(origin+Aim(direction+100)*4);
                yield return Wait(()=>pattern.Phase==OilFilmPhase.Ready,2,"ready");Require(!enemy.IsPatternFacingLocked&&!enemy.IsPatternPositionLocked&&pattern.NextReadyTime-Time.time>2.3f,"unlock and rearm once");
                Pass($"FLOW {direction}: fixed core/direction, entire sector damage3 once, full extension matches ground boundary, harmless recovery and rearm");
            }
            foreach(var sample in new[]{new Vector3(.85f,0,3),new Vector3(1.5f,0,3),new Vector3(2.475f,0,3),new Vector3(2.525f,0,0),new Vector3(1.5f,54.5f,3),new Vector3(1.5f,55.5f,0),new Vector3(1.5f,-54.5f,3),new Vector3(1.5f,-55.5f,0),new Vector3(1.5f,180,0)})
            {
                Spawn();Move(origin+Vector3.right*1.6f);yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"edge warning");
                Move(origin+Aim(sample.y)*sample.x);health.ResetHealth();yield return Wait(()=>pattern.ImpactCount==1,3,"edge impact");
                Equal(sample.z,100-health.CurrentHealth,"sector boundary and inner area");Move(origin+Vector3.forward*4);
                Pass($"AREA r{sample.x:F3}/angle{sample.y:F1}: damage{sample.z}; no inner hole or player-radius expansion");
            }
            yield return SourceAndLifecycle();
            Pass("A-2 COMPLETE: basic connection, actual damage, surface/telegraph alignment, contact, inspector, cancel/death/pool and XP verified; S-MAP remains0; A-3 separate");
        }
        private static void CheckWarning()
        {
            Require(pattern.TelegraphObject!=null,"filled warning exists");var mesh=pattern.TelegraphObject.GetComponent<MeshFilter>().sharedMesh;
            Require(mesh.vertexCount==OilFilmSector.Segments+2&&mesh.triangles.Length==OilFilmSector.Segments*3,"filled fan with center vertex, not a ring");Equal(0,mesh.vertices[0].magnitude,"no inner hole");
            foreach(int i in new[]{1,17,33,49,65})Equal(0,Vector2.Distance(CombatHitGeometry.Flat(pattern.TelegraphObject.transform.TransformPoint(mesh.vertices[i])),CombatHitGeometry.Flat(pattern.Footprint.Point((i-1)/(float)OilFilmSector.Segments,1))),"warning boundary");
            Require(pattern.TelegraphObject.GetComponentsInChildren<Collider>().Length==0,"warning has no contact collider");
        }
        private static void CheckSurface()
        {
            var visual=enemy.GetComponent<OilFilmVisual>();Require(visual.IsUnfolded&&visual.CurrentProfile.index==10,"full extension rendered at impact");
            for(int i=0;i<=16;i++)
            {
                float t=i/16f;var actual=visual.VisibleRimSample(t);var expected=pattern.Footprint.Point(Body().flipX?1-t:t,1);
                Equal(0,Vector2.Distance(CombatHitGeometry.Flat(actual),CombatHitGeometry.Flat(expected)),"visible rim/filled area edge");Equal(biome.GetGroundHeight(actual)+.055f,actual.y,"membrane on ground");
            }
        }
        private static void Capture(string name) => CaptureFile("Logs/"+Label+"-"+name+".png");
        private static void CaptureFile(string path)
        {
            var camera=DontStarveCamera.GetActiveCamera();var position=camera.transform.position;float size=camera.orthographicSize,aspect=camera.aspect;
            var target=RenderTexture.GetTemporary(960,540,24,RenderTextureFormat.ARGB32);var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;var texture=new Texture2D(960,540,TextureFormat.RGBA32,false);
            try
            {
                camera.orthographicSize=2.65f;camera.aspect=960f/540;camera.transform.position=pattern.GroundOrigin+(phase=="motion"?Aim(angle):pattern.LockedDirection)*.8f+Vector3.up*.2f-camera.transform.forward*12;
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,960,540),0,0);texture.Apply();Directory.CreateDirectory("Logs");File.WriteAllBytes(path,texture.EncodeToPNG());
            }
            finally{camera.targetTexture=previousTarget;RenderTexture.active=previousActive;camera.transform.position=position;camera.orthographicSize=size;camera.aspect=aspect;RenderTexture.ReleaseTemporary(target);Object.Destroy(texture);}
        }
    }
}
