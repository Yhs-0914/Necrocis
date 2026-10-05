using System;
using System.Collections;
using System.Linq;
using Necrocis;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace NecrocisEditor
{
    public static partial class HelicoSpiralConnectionRunner
    {
        private static IEnumerator VisitChecks()
        {
            var point=field.Plan.placements.Single();string spawnId=point.spawnId;
            Require(SaveService.TrySaveActiveRun(out string error),error);string run=SaveService.ActiveRunId,visit=biome.MonsterVisit.VisitId;
            SaveService.UseStorageRootForTests(storage);Require(SaveService.TryContinue(GameDifficulty.Normal,out error),error);
            Require(SaveService.TryRestorePendingSession(out BiomeType resume,out error)&&resume==BiomeType.Stomach,error);
            yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_STOMACH);yield return null;BindBattleScene();
            Require(SaveService.ActiveRunId==run&&biome.MonsterVisit.VisitId==visit&&SaveService.IsBiomeEliteDefeated(spawnId),"disk visit/defeat preserved");
            field.Configure(biome,temporary,biome.GetBiomeConfig().GetMidBossArenaConfig(),biome.GetBiomeConfig().GetReturnPortalConfig());
            Move(biome.GridToWorldWithHeight(point.x,point.y));field.Refresh(player.transform.position);
            Require(field.Spawners.Count==0,"saved defeated point stays absent");field.enabled=false;
            Pass("SAVE: actual disk continue/scene reload preserve the run, visit and defeated S-01 placement; no respawn");
            var catalog=Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath);
            foreach(var difficulty in new[]{GameDifficulty.Normal,GameDifficulty.Hard})
            {
                if(enemy!=null&&enemy.gameObject.activeSelf)enemy.ReleaseToPool();
                SaveService.MarkFinalBossDefeated();Require(SaveService.TryBeginNewGame(difficulty,out error),error);Require(SaveService.TryRestorePendingSession(out _,out error),error);
                GameManager.Instance.EnterBiome(BiomeType.Stomach);yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_STOMACH);yield return null;BindBattleScene();
                for(int stage=0;stage<=4;stage++)
                {
                    Require(biome.MonsterVisit.Stage==stage&&biome.MonsterVisit.Difficulty==difficulty,$"visit expected {difficulty}/{stage}, actual {biome.MonsterVisit.Difficulty}/{biome.MonsterVisit.Stage}");
                    origin=FindPatch(0,7);Spawn();enemy.ApplyBalancePhase("Default",true);
                    var basis=definition.statSets.Single(s=>s.id=="Default");var multi=catalog.difficultyCatalog.Get(difficulty).elites;
                    Require(catalog.progression.TryGetStage(stage,out var progress),"progression row");
                    Equal(CharacterStats.ToHealthUnits(basis.maxHealth*multi.maxHealth*progress.maxHealth),enemy.Stats.MaxHealth,"HP once");
                    Equal(basis.moveSpeed*multi.moveSpeed,enemy.Stats.MoveSpeed,"move speed no progression");float maxHp=enemy.Stats.MaxHealth;
                    float raw=basis.attackPower*multi.outgoingDamage*progress.attackPower*definition.patternDamage.Single(p=>p.id==HelicoSpiralPatternSettings.DamageId).coefficient;
                    Equal(raw,enemy.CreatePatternDamage(HelicoSpiralPatternSettings.DamageId,0).Amount,"attack once");
                    enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed,0);Move(origin+Vector3.right*2.6f);
                    yield return Wait(()=>pattern.DamageApplications==1,3,"progress actual dash hit");Equal(CharacterStats.ToHealthUnits(raw),100-health.CurrentHealth,"progress actual HP damage");
                    Move(origin+Vector3.forward*4);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Ready,2,"progress recovery");
                    Require(Mathf.Abs(pattern.NextReadyTime-Time.time-settings.rearmSeconds*multi.attackCooldown)<.07f,"one difficulty cooldown multiplier");
                    Equal(.7f,pattern.WindupDuration,"warning not compressed");Equal(.25f,pattern.DashDuration,"dash time unchanged");Equal(.4f,pattern.BodyRadius,"width unchanged");Equal(3.5f,pattern.Travelled,"distance unchanged");
                    var home=enemy.GetComponent<Rigidbody>().position;Move(home+Vector3.forward*1.6f);health.ResetHealth();float expectedContact=CharacterStats.ToHealthUnits(enemy.CreateContactDamage(0).Amount);
                    for(int n=0;n<35&&health.CurrentHealth==100;n++){player.TryMoveByWorld(Vector3.back*.035f);yield return new WaitForFixedUpdate();}
                    Equal(expectedContact,100-health.CurrentHealth,"scaled actual contact");Move(home+Vector3.forward*4);
                    int xp=0,expected=(int)Math.Round((double)definition.reward.baseExperience*multi.experienceReward*progress.experience,MidpointRounding.ToEven);
                    enemy.SuppressExperienceReward=false;Action<int> observe=n=>xp+=n;var callback=LevelUpManager.OnLevelUp;LevelUpManager.OnLevelUp=null;LevelUpManager.OnExpGained+=observe;
                    try{enemy.TakeDamage(10000);enemy.TakeDamage(10000);enemy.GrantExp();}
                    finally{LevelUpManager.OnExpGained-=observe;LevelUpManager.OnLevelUp=callback;}
                    Equal(expected,xp,"reward once");yield return Wait(()=>!enemy.gameObject.activeSelf,2,"progress death");
                    Pass($"BALANCE {difficulty}/stage{stage}: actual HP{maxHp}, dash{CharacterStats.ToHealthUnits(raw)}, contact{expectedContact}, XP{xp} once; tell/width/distance/speed fixed, rearm multiplier once");
                    if(stage==4)continue;
                    GameManager.Instance.CollectRelic(new[]{BiomeType.Intestine,BiomeType.Liver,BiomeType.Stomach,BiomeType.Lung}[stage]);
                    Require(biome.MonsterVisit.Stage==stage,"visit progress frozen");Spawn();Require(enemy.Balance.Current.Stage==stage,"same visit spawn frozen");enemy.ReleaseToPool();
                    GameManager.Instance.ReturnToHub();yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_HUB);
                    GameManager.Instance.EnterBiome(BiomeType.Stomach);yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_STOMACH);yield return null;BindBattleScene();
                }
            }
        }
        private static void BindBattleScene()
        {
            player=PlayerController.Instance;health=player.HealthComponent;biome=Object.FindFirstObjectByType<ProceduralBiomeBridge>();field=biome.GetComponent<BiomeEliteField>();
            Require(biome.BiomeType==BiomeType.Stomach,"actual Stomach visit");field.enabled=false;enemy=null;pattern=null;
            Require(biome.GetBiomeConfig().biomeEliteSpawnConfig.monsters.Count==0,"production Stomach unchanged");
            ((IList)typeof(ProceduralBiomeBridge).GetField("normalEnemyRules",Private).GetValue(biome)).Clear();
            if(EliteSpawner.Instance!=null)EliteSpawner.Instance.enabled=false;
            typeof(InputManager).GetMethod("SetActionsEnabled",Private).Invoke(InputManager.Instance,new object[]{false});
            groundOffset=player.transform.position.y-biome.GetGroundHeight(player.transform.position);Equal(-2,groundOffset,"visit ground offset");
            PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth,100,true);
        }
    }
}
