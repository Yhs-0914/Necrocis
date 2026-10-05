using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace NecrocisEditor
{
    public static class EliteExistingSaveRunner
    {
        private static string storage,runId,resumeScene;
        private static bool started,passed,oldOptionsEnabled,oldBackground;
        private static EnterPlayModeOptions oldOptions;
        private static SceneSetup[] scenes;
        private static int enteredFrame;
        private static double deadline;
        private static float oldScale;
        private static readonly List<string> results=new List<string>();
        public static void Run()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Edit Mode required");
            for(int i=0;i<SceneManager.sceneCount;i++)Require(!SceneManager.GetSceneAt(i).isDirty,"saved scenes required");
            var original=Path.Combine(Application.persistentDataPath,"Saves");
            var saved=JsonUtility.FromJson<RunSaveData>(File.ReadAllText(Path.Combine(original,"normal.json")));
            runId=saved.saveId;resumeScene=saved.checkpoint.sceneName;
            storage=Path.Combine(Path.GetTempPath(),"necrocis-existing-elite-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Path.Combine(storage,"Saves"));
            foreach(var name in new[]{"profile.json","profile.json.bak","normal.json","normal.json.bak"})
                if(File.Exists(Path.Combine(original,name)))File.Copy(Path.Combine(original,name),Path.Combine(storage,"Saves",name));
            SaveService.UseStorageRootForTests(storage);Require(SaveService.TryContinue(GameDifficulty.Normal,out string error),error);
            scenes=EditorSceneManager.GetSceneManagerSetup();oldOptionsEnabled=EditorSettings.enterPlayModeOptionsEnabled;oldOptions=EditorSettings.enterPlayModeOptions;
            oldScale=Time.timeScale;oldBackground=Application.runInBackground;Application.runInBackground=true;
            EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions|=EnterPlayModeOptions.DisableDomainReload;
            results.Clear();started=passed=false;deadline=EditorApplication.timeSinceStartup+180;
            EditorApplication.playModeStateChanged+=OnPlay;EditorApplication.update+=Tick;
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Hub.unity");EditorApplication.isPlaying=true;
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredPlayMode)enteredFrame=Time.frameCount;
            if(state!=PlayModeStateChange.EnteredEditMode)return;
            EditorApplication.update-=Tick;EditorApplication.playModeStateChanged-=OnPlay;
            EditorSettings.enterPlayModeOptionsEnabled=oldOptionsEnabled;EditorSettings.enterPlayModeOptions=oldOptions;
            Application.runInBackground=oldBackground;Time.timeScale=oldScale;SaveService.ResetStaticStateForTests();DifficultyBalanceService.ResetForTests();
            if(Directory.Exists(storage))Directory.Delete(storage,true);
            Directory.CreateDirectory("Logs/Elite-ALL01");File.WriteAllLines("Logs/Elite-ALL01/existing-save-results.txt",results);
            if(scenes!=null)EditorSceneManager.RestoreSceneManagerSetup(scenes);Debug.Log("[EliteExisting] "+(passed?"ALL PASS":"FAIL"));
        }
        private static void Tick()
        {
            if(EditorApplication.timeSinceStartup>deadline){deadline=double.PositiveInfinity;Fail(new TimeoutException("Existing save verification timeout"));return;}
            if(started||!EditorApplication.isPlaying||Time.frameCount-enteredFrame<15||PlayerController.Instance==null)return;
            started=true;Time.timeScale=1;Application.runInBackground=true;PlayerController.Instance.StartCoroutine(Guard(Checks()));
        }
        private static IEnumerator Guard(IEnumerator root)
        {
            var stack=new Stack<IEnumerator>();stack.Push(root);
            while(stack.Count>0)
            {
                object next;
                try{var c=stack.Peek();if(!c.MoveNext()){(c as IDisposable)?.Dispose();stack.Pop();continue;}next=c.Current;if(next is IEnumerator nested){stack.Push(nested);continue;}}
                catch(Exception e){Fail(e);yield break;}yield return next;
            }
            passed=true;EditorApplication.isPlaying=false;
        }
        private static IEnumerator Checks()
        {
            yield return Wait(()=>SceneManager.GetActiveScene().name==resumeScene&&Object.FindFirstObjectByType<ProceduralBiomeBridge>()?.GetComponent<BiomeEliteField>()?.Plan!=null,"normal continue checkpoint");
            Require(SaveService.ActiveRunId==runId,"same copied run");Pass("Existing Normal slot resumes its original checkpoint and run ID through GameplaySaveCoordinator; original files are not mounted");
            foreach(var kind in new[]{BiomeType.Intestine,BiomeType.Liver,BiomeType.Lung,BiomeType.Stomach})
            {
                if(SceneManager.GetActiveScene().name!=SceneLoader.GetSceneName(kind))
                {
                    GameManager.Instance.ReturnToHub();yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_HUB);
                    GameManager.Instance.EnterBiome(kind);yield return SceneManager.LoadSceneAsync(SceneLoader.GetSceneName(kind));yield return null;
                }
                var biome=Object.FindFirstObjectByType<ProceduralBiomeBridge>();var field=biome.GetComponent<BiomeEliteField>();var config=biome.GetBiomeConfig().biomeEliteSpawnConfig;
                var player=PlayerController.Instance;float offset=player.transform.position.y-biome.GetGroundHeight(player.transform.position);
                PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth,1000,true);
                typeof(InputManager).GetMethod("SetActionsEnabled",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(InputManager.Instance,new object[]{false});
                Require(field.Plan!=null&&field.Plan.placements.Count>=config.minimumCount&&field.Plan.placements.Count<=config.maximumCount,"actual sparse field "+kind);
                Pass(kind+": current saved run creates "+field.Plan.placements.Count+" dedicated placements without kills");
                foreach(var rule in config.monsters)
                {
                    var point=field.Plan.placements.First(p=>p.monsterId==rule.monsterDefinition.monsterId);var pos=biome.GridToWorldWithHeight(point.x,point.y)+Vector3.right*4;pos.y=biome.GetGroundHeight(pos)+offset;
                    player.SpawnAt(pos);DontStarveCamera.Instance?.SnapToTarget();typeof(BiomeManager).GetMethod("UpdateChunks",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(biome,null);
                    foreach(var spawner in Object.FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None)){spawner.ReleaseSpawnedEnemies();spawner.enabled=false;}
                    field.Refresh(pos);yield return null;
                    var actor=field.Spawners.Single(p=>p.Placement.spawnId==point.spawnId).ActiveEnemy;
                    Require(actor!=null&&!actor.IsDead&&actor.Balance.Current.MonsterId==point.monsterId&&actor.Balance.ContactEnabled&&actor.GetComponents<MonsterPatternController>().Any(p=>p.enabled),"actual approved actor active");
                    Pass(kind+"/"+point.monsterId+": actual runtime actor and approved pattern/contact active in copied existing run");
                }
            }
            Require(SaveService.TrySaveActiveRun(out string error),error);
            var disk=JsonUtility.FromJson<RunSaveData>(File.ReadAllText(Path.Combine(storage,"Saves","normal.json")));
            Require(disk.saveId==runId&&disk.world.biomeElitePlans.Count==4,"copy saves four generated plans");Pass("Copied continue saves all four biome plans under the same run; original save remains untouched");
        }
        private static IEnumerator Wait(Func<bool> condition,string message){float until=Time.time+30;while(!condition()){Require(Time.time<until,message);yield return null;}}
        private static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
        private static void Pass(string value){results.Add("PASS "+value);Debug.Log("[EliteExisting] PASS "+value);}
        private static void Fail(Exception error){passed=false;results.Add("FAIL "+error);Debug.LogException(error);EditorApplication.isPaused=false;EditorApplication.isPlaying=false;}
    }
}
