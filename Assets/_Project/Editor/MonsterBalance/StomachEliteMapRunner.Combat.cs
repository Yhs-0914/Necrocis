using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class StomachEliteMapRunner
    {
        private static EnemyController helico, oil;
        private static HelicoSpiralElitePattern helicoPattern;
        private static OilFilmElitePattern oilPattern;
        private static Vector3 encounterOrigin;
        private static readonly float[] Angles = { 0, 90, 180, 270 };
        private static Vector3 Aim(float angle) => new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
        private static SpriteRenderer Body(EnemyController actor) => actor.transform.Find("Visual").GetComponent<SpriteRenderer>();
        private static Vector2Int FindPairPatch()
        {
            for (int z = 10; z < biome.MapHeight - 10; z++) for (int x = 10; x < biome.MapWidth - 10; x++)
            {
                int level = biome.GetHeightLevel(x,z); bool clear = biome.IsWalkable(x,z);
                for (int dx=-6; dx<=6 && clear; dx++) for (int dz=-6; dz<=6 && clear; dz++)
                    clear = biome.IsWalkable(x+dx,z+dz) && biome.GetHeightLevel(x+dx,z+dz)==level;
                if (clear) return new Vector2Int(x,z);
            }
            throw new InvalidOperationException("No paired combat patch in actual Stomach terrain");
        }
        private static void StagePair(Vector2Int cell, Vector3 aim)
        {
            var o = field.Plan.placements.Single(p=>p.monsterId==oilDefinition.monsterId);
            var h = field.Plan.placements.Single(p=>p.monsterId==helicoDefinition.monsterId);
            Vector3 offset = -aim*2 + Vector3.Cross(aim,Vector3.up);
            o.x=cell.x; o.y=cell.y; h.x=cell.x+Mathf.RoundToInt(offset.x); h.y=cell.y+Mathf.RoundToInt(offset.z);
            encounterOrigin=biome.GridToWorldWithHeight(cell.x,cell.y);
            field.Configure(biome,temporaryConfig,biome.GetBiomeConfig().GetMidBossArenaConfig(),biome.GetBiomeConfig().GetReturnPortalConfig());
            ResetPair();
        }
        private static void ResetPair(bool mobile=false)
        {
            foreach(var s in field.Spawners.ToArray())s.ReleaseEnemy(); Move(encounterOrigin+Vector3.right*6);
            helico=field.Spawners.Single(s=>s.Placement.monsterId==helicoDefinition.monsterId).ActiveEnemy;
            oil=field.Spawners.Single(s=>s.Placement.monsterId==oilDefinition.monsterId).ActiveEnemy;
            Require(helico!=null && oil!=null && helico.Balance.ContactEnabled && oil.Balance.ContactEnabled,"real dedicated pair with contact enabled");
            helicoPattern=helico.GetComponent<HelicoSpiralElitePattern>(); oilPattern=oil.GetComponent<OilFilmElitePattern>();
            Require(helico.HasPatternDirections && oil.HasPatternDirections && !helicoPattern.TailEnabled,"approved directional pair, tail retired");
            if(!mobile){helico.Stats.SetBaseStat(CharacterStatType.MoveSpeed,0);oil.Stats.SetBaseStat(CharacterStatType.MoveSpeed,0);}
            helico.Stats.SetBaseStat(CharacterStatType.MaxHealth,200,true);oil.Stats.SetBaseStat(CharacterStatType.MaxHealth,200,true);health.ResetHealth();
        }
        private static IEnumerator CombinedCombat()
        {
            Require(field.Plan==null,"combo isolated from production plan");temporaryConfig=ScriptableObject.CreateInstance<BiomeEliteSpawnConfig>();
            temporaryConfig.minimumCount=temporaryConfig.maximumCount=2;temporaryConfig.clearanceCells=5;
            temporaryConfig.monsters.Add(HelicoSpiralSetup.PreviewRule(helicoDefinition));temporaryConfig.monsters.Add(OilFilmSetup.PreviewRule(oilDefinition));
            field.Configure(biome,temporaryConfig,biome.GetBiomeConfig().GetMidBossArenaConfig(),biome.GetBiomeConfig().GetReturnPortalConfig());
            Require(field.Plan.placements.Count==2,"zero-kill pair");if(EliteSpawner.Instance!=null)EliteSpawner.Instance.enabled=false;
            var cell=FindPairPatch();Pass(difficulty+" COMBO: real Stomach close-pair fixture, original ranges/timings, production spacing untouched");
            foreach(float angle in Angles)
            {
                var aim=Aim(angle);var normal=Vector3.Cross(aim,Vector3.up);StagePair(cell,aim);
                foreach(int side in new[]{-1,1})
                {
                    ResetPair();Move(encounterOrigin+aim*2.1f);health.ResetHealth();float hp=health.CurrentHealth;
                    yield return Wait(()=>helicoPattern.Phase==HelicoSpiralPhase.Windup&&oilPattern.Phase==OilFilmPhase.Windup,3,"both red warnings");
                    var hDirection=helicoPattern.LockedDirection;var oDirection=oilPattern.LockedDirection;var fixedCore=oilPattern.Position;
                    Require(helicoPattern.TelegraphObject!=null&&oilPattern.TelegraphObject!=null,"both warnings visible");
                    Require(oilPattern.Footprint.Contains(encounterOrigin+oDirection*.8f),"no inner safe zone");
                    if(angle==0&&side==1){yield return new WaitForEndOfFrame();Capture("combo-tell",encounterOrigin);}
                    yield return WalkTo(encounterOrigin+aim*.8f+normal*(side*1.8f),1.2f);
                    yield return Wait(()=>oilPattern.ImpactCount==1,2,"mixed impact");
                    Equal(hp,health.CurrentHealth,"both attacks avoided by actual walk");
                    Equal(0,Distance(fixedCore,oilPattern.Position),"fixed oil core");
                    Require(Vector3.Angle(hDirection,helicoPattern.LockedDirection)<.01f&&Vector3.Angle(oDirection,oilPattern.LockedDirection)<.01f,"both attacks keep committed direction");
                    if(angle==0&&side==1){yield return new WaitForEndOfFrame();Capture("combo-impact",encounterOrigin);}
                    yield return Wait(()=>oilPattern.Phase==OilFilmPhase.Recovery,1,"oil recovery opening");
                    yield return WalkTo(encounterOrigin+normal*(side*1.05f),1);
                    Require(oilPattern.Phase==OilFilmPhase.Recovery,"counter reaches harmless recovery");Face(-normal*side);Physics.SyncTransforms();float enemyHp=oil.Stats.CurrentHealth;
                    typeof(PlayerAttack).GetMethod("MeleeAttack",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(player.GetComponent<PlayerAttack>(),null);
                    Require(oil.Stats.CurrentHealth<enemyHp,"real Q hits oil core in mixed encounter");
                    if(angle==0&&side==1){yield return new WaitForEndOfFrame();Capture("combo-counter",encounterOrigin);}
                    yield return WalkTo(encounterOrigin+normal*(side*3.4f),1);
                    yield return Wait(()=>oilPattern.Phase==OilFilmPhase.Ready&&helicoPattern.Phase==HelicoSpiralPhase.Ready,3,"both recover");
                    Equal(hp,health.CurrentHealth,"counter and retreat remain safe");
                    Require(helicoPattern.DashCount==1&&oilPattern.ImpactCount==1&&!helicoPattern.TailEnabled,"one dash, one spread, no tail followup");
                    Pass($"{difficulty} WALK/Q {angle}/side{side}: native sidestep avoids dash and full sector; recovery Q/retreat damage0, both locks retained");
                }
            }
            StagePair(cell,Vector3.right);ResetPair(true);Move(encounterOrigin+Vector3.right*5);
            var hStart=helico.GetComponent<Rigidbody>().position;var oStart=oil.GetComponent<Rigidbody>().position;yield return new WaitForSeconds(.3f);
            Require(Distance(hStart,helico.GetComponent<Rigidbody>().position)>.1f&&Distance(oStart,oil.GetComponent<Rigidbody>().position)>.1f,"both move at native speed");
            Require(helicoPattern.DashCount==0&&oilPattern.ImpactCount==0,"spawn grace");Pass(difficulty+" MOBILE: both approach at original speed during spawn grace");
            Set(helicoDefinition.pattern,"spawnGraceSeconds",100);Set(oilDefinition.pattern,"spawnGraceSeconds",100);
            foreach(bool useHelico in new[]{true,false})
            {
                ResetPair();var actor=useHelico?helico:oil;var home=actor.GetComponent<Rigidbody>().position;
                Move(home+Vector3.back*2.2f);health.ResetHealth();float hp=health.CurrentHealth;
                for(int n=0;n<80&&health.CurrentHealth==hp;n++){player.TryMoveByWorld(Vector3.forward*.04f);yield return new WaitForFixedUpdate();}
                Equal(CharacterStats.ToHealthUnits(actor.CreateContactDamage(0).Amount),hp-health.CurrentHealth,"native body contact");
                Pass(difficulty+" CONTACT "+actor.Balance.Current.MonsterId+": actual walking collision damage, attacks isolated by temporary grace");
            }
            RestoreAssets();StagePair(cell,Vector3.right);ResetPair();Move(encounterOrigin+Vector3.right*2.1f);health.ResetHealth();float startHp=health.CurrentHealth;
            yield return Wait(()=>helicoPattern.Phase==HelicoSpiralPhase.Windup&&oilPattern.Phase==OilFilmPhase.Windup,3,"priority warnings");
            var first=difficulty==GameDifficulty.Normal?helico:oil;var survivor=first==helico?oil:helico;
            string firstId=first.Balance.Current.MonsterId;first.SuppressExperienceReward=true;first.TakeDamage(10000);
            Require(first.GetComponent<EnemyPatternLifetime>().OwnedObjectCount==0&&first.IsDeathAnimPlaying,"killed owner only cleared");
            Require(survivor.GetComponent<EnemyPatternLifetime>().OwnedObjectCount>0&&!survivor.IsDead,"survivor attack intact");
            yield return WalkTo(encounterOrigin+new Vector3(1,0,-2.4f),1.2f);
            yield return Wait(()=>survivor.GetComponent<EnemyPatternLifetime>().OwnedObjectCount==0,4,"survivor expires");
            Equal(startHp,health.CurrentHealth,"priority retreat safe");survivor.SuppressExperienceReward=true;survivor.TakeDamage(10000);
            yield return Wait(()=>!first.gameObject.activeSelf&&!survivor.gameObject.activeSelf,2,"both death animations");field.Refresh(player.transform.position);
            Require(field.Spawners.Count==0&&field.Plan.placements.All(p=>SaveService.IsBiomeEliteDefeated(p.spawnId)),"both defeated points saved");
            Pass(difficulty+" PRIORITY "+firstId+": independent cleanup and remaining attack, safe retreat, both deaths saved");
        }
        private static void Capture(string name,Vector3 center)
        {
            var camera=DontStarveCamera.GetActiveCamera();var position=camera.transform.position;float size=camera.orthographicSize,aspect=camera.aspect;
            var target=RenderTexture.GetTemporary(960,540,24,RenderTextureFormat.ARGB32);var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;var texture=new Texture2D(960,540,TextureFormat.RGBA32,false);
            try
            {
                center.y=biome.GetGroundHeight(center);camera.orthographicSize=3.3f;camera.aspect=960f/540;camera.transform.position=center+Vector3.up*.25f-camera.transform.forward*12;
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,960,540),0,0);texture.Apply();Directory.CreateDirectory("Logs");
                File.WriteAllBytes("Logs/Stomach-S-MAP-"+difficulty+"-"+name+".png",texture.EncodeToPNG());
            }
            finally{camera.targetTexture=previousTarget;RenderTexture.active=previousActive;camera.transform.position=position;camera.orthographicSize=size;camera.aspect=aspect;RenderTexture.ReleaseTemporary(target);Object.Destroy(texture);}
        }
    }
}
