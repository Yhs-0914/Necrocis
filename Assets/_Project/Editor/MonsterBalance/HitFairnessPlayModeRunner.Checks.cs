using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static partial class HitFairnessPlayModeRunner
    {
        private static IEnumerator Checks()
        {
            GameManager.Instance.EnterBiome(BiomeType.Liver); yield return SceneManager.LoadSceneAsync(SceneLoader.SCENE_LIVER); yield return null;
            biome = Object.FindFirstObjectByType<ProceduralBiomeBridge>(); field = biome.GetComponent<BiomeEliteField>(); field.enabled = false;
            groundOffset = player.transform.position.y - biome.GetGroundHeight(player.transform.position);
            if (EliteSpawner.Instance != null) EliteSpawner.Instance.enabled = false;
            ((IList)typeof(ProceduralBiomeBridge).GetField("normalEnemyRules", Private).GetValue(biome)).Clear();
            typeof(InputManager).GetMethod("SetActionsEnabled", Private).Invoke(InputManager.Instance, new object[] { false });
            bool found = false;
            for (int z = 7; z < biome.MapHeight-7 && !found; z++) for (int x = 7; x < biome.MapWidth-7 && !found; x++)
            {
                int level = biome.GetHeightLevel(x,z); bool clear = true;
                for (int dx=-6; dx<=6 && clear; dx++) for (int dz=-6; dz<=6 && clear; dz++) clear=biome.IsWalkable(x+dx,z+dz)&&biome.GetHeightLevel(x+dx,z+dz)==level;
                if (clear) { origin=biome.GridToWorldWithHeight(x,z); found=true; }
            }
            Require(found,"clear real-map test patch"); PlayerStats.Instance.RuntimeStats.SetBaseStat(CharacterStatType.MaxHealth,60,true); Move(origin);
            yield return new WaitForFixedUpdate();
            var capsule=(CapsuleCollider)player.HitCollider;var sprite=player.GetComponentsInChildren<SpriteRenderer>().First(r=>r.sprite!=null&&r.name=="Sprite");
            float width=((Sprite[])typeof(PlayerController).GetField("idleSprites",Private).GetValue(player)).Where(f=>f!=null).Min(f=>sprite.transform.TransformVector(Vector3.right*f.bounds.size.x).magnitude), radius=Radius();
            Equal(.792f,capsule.bounds.size.x,"base player hurt width"); Equal(width*.9f,capsule.bounds.size.x,"inside base idle body");
            Vector2 terrain=player.GetComponent<ProceduralTerrainMotor>().TerrainHalfExtents;
            Require(terrain==new Vector2(.68f,.48f),"terrain clearance remains independent");
            Pass($"BODY visual width={width:F3}m, hurt width={capsule.bounds.size.x:F3}m (90%), radius={radius:F3}; terrain clearance unchanged");
            foreach(var job in new[]{JobType.Warrior,JobType.Mage,JobType.Archer})
            {
                typeof(PlayerController).GetMethod("ApplyJobVisual",Private).Invoke(player,new object[]{job});yield return null;Equal(radius,Radius(),"job does not inflate hitbox");
            }
            Vector3 scale=player.transform.localScale;player.transform.localScale=scale*1.5f;yield return new WaitForFixedUpdate();Equal(radius*1.5f,Radius(),"item size follows collider proportion");player.transform.localScale=scale;yield return new WaitForFixedUpdate();
            Pass("hurt size stable across three job visuals; body scaling scales it once and returns without drift");
            Set(player,"hurtboxWidthRatio",.8f);yield return new WaitForFixedUpdate();Equal(width*.8f,capsule.bounds.size.x,"Inspector hurt ratio applies");
            Set(player,"hurtboxWidthRatio",.9f);yield return new WaitForFixedUpdate();Equal(radius,Radius(),"Inspector ratio restores without repeated shrink");
            Pass("Inspector hurt ratio 90% -> 80% -> 90% updates physical collider without cumulative shrink");

            foreach(float angle in new[]{0f,45f,90f,135f,180f,225f,270f,315f}) foreach(float margin in new[]{-.02f,.02f})
            {
                Move(origin);health.ResetHealth();yield return new WaitForFixedUpdate();float hp=health.CurrentHealth;
                Vector3 point=player.HitCollider.bounds.center+Aim(angle)*(Radius()+.15f+margin);
                var shot=EnemyProjectile.Acquire(point,null,Vector3.one*.2f);shot.Launch(Vector3.right,1,0,1);
                yield return null;yield return null;Equal(margin<0?1:0,hp-health.CurrentHealth,"rounded incoming boundary at "+angle);shot.Deflect();
                Pass($"INCOMING {angle:0}deg margin={margin:F2}m: actual damage={hp-health.CurrentHealth:0}");
            }
            foreach(float speed in new[]{30f,120f}) foreach(float margin in new[]{-.02f,.02f})
            {
                Move(origin);health.ResetHealth();Vector3 aim=Aim(45),side=new Vector3(-aim.z,0,aim.x),p=player.HitCollider.bounds.center;
                var shot=EnemyProjectile.Acquire(p-aim*3+side*(Radius()+.15f+margin),null,Vector3.one*.2f);shot.Launch(aim,1,speed,6/speed);float hp=health.CurrentHealth;
                yield return Wait(()=>!shot.IsLaunched,1,"fast incoming sweep");Equal(margin<0?1:0,hp-health.CurrentHealth,"fast exact round boundary");
                Pass($"SWEEP speed={speed:0} margin={margin:F2}: actual damage={hp-health.CurrentHealth:0}, no enlarged diagonal corner");
            }
            Move(origin);health.ResetHealth();Vector3 corner=player.HitCollider.bounds.center+new Vector3(.74f,0,.74f);float before=health.CurrentHealth;
            var oldCorner=EnemyProjectile.Acquire(corner,null,Vector3.one*.2f);oldCorner.Launch(Vector3.right,1,0,1);yield return null;yield return null;
            Equal(before,health.CurrentHealth,"previous false corner hit is now a miss");oldCorner.Deflect();Pass("old false-positive projectile at diagonal offset .74/.74 now deals zero damage");

            SpawnTarget();Move(origin);SetFacing(Vector3.right);var attack=player.GetComponent<PlayerAttack>();
            var data=new SerializedObject(attack);float multiplier=player.GetComponent<PlayerItemCombatEffects>().GetMeleeRangeMultiplier();
            float offset=PlayerCombatCalculator.GetBasicAttackRange(data.FindProperty("meleeAttackOffset").floatValue*multiplier,PlayerStats.Instance);
            Vector3 box=data.FindProperty("meleeAttackBoxSize").vector3Value;
            float w=PlayerCombatCalculator.GetBasicAttackRange(box.x*multiplier,PlayerStats.Instance),d=PlayerCombatCalculator.GetBasicAttackRange(box.z*multiplier,PlayerStats.Instance);
            float size=(float)typeof(PlayerAttack).GetMethod("GetMeleeSlashTargetWorldSize",Private).Invoke(attack,new object[]{w,d});float edge=offset+size*.5f;
            foreach(float gap in new[]{.07f,.10f})
            {
                PlaceTarget(origin+Vector3.right*(edge+HalfX()+gap));float hp=enemy.Stats.CurrentHealth;
                typeof(PlayerAttack).GetMethod("MeleeAttack",Private).Invoke(attack,null);bool hit=enemy.Stats.CurrentHealth<hp;Require(hit==(gap<.08f),"Q edge tolerance");
                Pass($"Q body-edge gap={gap:F2}m: hit={hit}; .08m padding has an outer miss boundary");
            }
            foreach(float extra in new[]{.07f,.10f})
            {
                PlaceTarget(origin+Vector3.right*(1+HalfX()+extra));float hp=enemy.Stats.CurrentHealth;
                int hits=Area(origin,1,false);Require((hits>0)==(extra<.08f),"area edge tolerance");Equal(extra<.08f?1:0,hp-enemy.Stats.CurrentHealth,"area actual damage once");
                Pass($"AREA body-edge gap={extra:F2}m: targets={hits}, damage={hp-enemy.Stats.CurrentHealth:0}");
            }
            PlaceTarget(origin+Vector3.right*1.1f);Require(Area(origin,1,false)==1,"body overlap counts when center lies outside circle");
            Pass("previous area miss fixed: enemy center 1.1m outside 1m radius, body overlaps, actual damage applies once");
            PlaceTarget(origin+Aim(55)*.9f);Require(Area(origin,1,true)==1,"body intersects arc edge although center angle is outside");
            PlaceTarget(origin+Vector3.left*2);Require(Area(origin,1,true)==0,"arc cannot hit a body entirely behind player");
            PlaceTarget(origin+Vector3.right*3);Require(Area(origin,1,false)==0,"area cannot reach distant body");
            Pass("ARC recognizes body grazing 90-degree sector, excludes fully rear/distant targets");
            Set(player,"attackEdgePadding",0);PlaceTarget(origin+Vector3.right*(1+HalfX()+.05f));Require(Area(origin,1,false)==0,"zero padding misses positive gap");
            Set(player,"attackEdgePadding",.08f);Require(Area(origin,1,false)==1,"Inspector padding enables same near-edge hit");
            Pass("Inspector attack padding 0 -> .08m affects target edge, without changing enemy attack or damage values");

            foreach(bool skill in new[]{false,true}) foreach(float margin in new[]{-.02f,.02f})
            {
                PlaceTarget(origin+Vector3.right*4);float r=skill ? .43f : .58f;float hp=enemy.Stats.CurrentHealth;
                var go=skill?new GameObject("FairnessSkillProjectile"):Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/bullet.prefab"));
                go.transform.position=enemy.transform.position+Vector3.right*(HalfX()+r+margin);
                if(skill) go.AddComponent<SkillProjectile>().Launch(Vector3.forward,1,.01f,1,~0,true,default);
                else
                {
                    var projectile=go.GetComponent<Projectile>();typeof(Projectile).GetField("speed",Private).SetValue(projectile,.01f);
                    projectile.Launch(Vector3.forward,1,~0,1);
                    Equal(r,(float)typeof(Projectile).GetMethod("GetScaledHitCheckRadius",Private).Invoke(projectile,null),"preserved physical radius plus padding");
                }
                yield return null;yield return null;Equal(margin<0?1:0,hp-enemy.Stats.CurrentHealth,"player projectile tolerance");
                Object.Destroy(go);Pass($"{(skill?"SKILL SHOT":"W SHOT")} margin={margin:F2}m: damage={hp-enemy.Stats.CurrentHealth:0}");
            }
            enemy.ReleaseToPool();
            var normal=AssetDatabase.LoadAssetAtPath<EnemySpawnConfig>("Assets/_Project/Data/BiomeConfigs/IntestineEnemySpawnConfig.asset").GetEnemySpawnRules().First(r=>!r.isElite&&!r.isRanged);
            enemy=EnemyController.Acquire(null,"FairnessNormalMelee",EnemyController.GetPoolArchetypeId(normal));enemy.Configure(null,normal,origin,origin);enemy.SetAiSuppressed(true);enemy.SuppressExperienceReward=true;enemy.GetComponent<EnemyContactDamage>().SetDamageActive(false);
            typeof(EnemyController).GetMethod("EnsurePlayerTransform",Private).Invoke(enemy,null);
            object[] args={new Bounds()};typeof(EnemyController).GetMethod("TryGetMeleeAttackBounds",Private).Invoke(enemy,args);Bounds melee=(Bounds)args[0];
            Move(new Vector3(melee.max.x+Radius()+.05f,origin.y,melee.center.z));Physics.SyncTransforms();health.ResetHealth();before=health.CurrentHealth;
            typeof(EnemyController).GetMethod("ApplyDamageToPlayer",Private).Invoke(enemy,null);Equal(before,health.CurrentHealth,"enemy melee no .2m overreach");
            Move(melee.center);Physics.SyncTransforms();health.ResetHealth();before=health.CurrentHealth;
            typeof(EnemyController).GetMethod("ApplyDamageToPlayer",Private).Invoke(enemy,null);Require(health.CurrentHealth<before,"enemy melee still hits real overlap");
            Pass("normal enemy melee misses 5cm beyond true rounded overlap and still damages inside its authored volume");
            health.ResetHealth();SetFacing(Vector3.forward);player.StartCoroutine((IEnumerator)typeof(PlayerController).GetMethod("DashCoroutine",Private).Invoke(player,null));
            before=health.CurrentHealth;Require(player.IsDashInvincible,"real dash coroutine active");
            typeof(EnemyController).GetMethod("ApplyDamageToPlayer",Private).Invoke(enemy,null);
            var dashShot=EnemyProjectile.Acquire(player.HitCollider.bounds.center,null,Vector3.one*.2f);dashShot.Launch(Vector3.right,1,0,1);
            typeof(EnemyProjectile).GetMethod("Update",Private).Invoke(dashShot,null);Equal(before,health.CurrentHealth,"melee and projectile both respect dash invincibility");dashShot.Deflect();
            yield return new WaitForSeconds(.2f);Pass("real dash state blocks ordinary melee and hostile projectile damage through the shared player damage gate");
            enemy.ReleaseToPool();
            foreach (var rule in AssetDatabase.LoadAssetAtPath<EnemySpawnConfig>("Assets/_Project/Data/BiomeConfigs/IntestineEnemySpawnConfig.asset").GetEnemySpawnRules().Where(r=>!r.isRanged))
            {
                Move(origin);health.ResetHealth();float hp=health.CurrentHealth;
                Vector3 at=origin+Vector3.left*5;
                enemy=EnemyController.Acquire(null,"FairnessAI_"+rule.name,EnemyController.GetPoolArchetypeId(rule));
                enemy.Configure(null,rule,at,at);enemy.SuppressExperienceReward=true;
                enemy.GetComponent<EnemyContactDamage>().SetDamageActive(false);
                yield return Wait(()=>health.CurrentHealth<hp,8,"natural AI melee "+rule.name);
                Require(hp-health.CurrentHealth>=1,"AI attack still damages real overlap");
                Pass("AI "+rule.name+": native approach/attack still damages player with body-contact channel isolated");
                enemy.ReleaseToPool();
            }
            Restore();
            Pass("fairness checks complete; native body-contact matrix is verified separately, no monster contact source disabled");
        }
        private static float Radius(){var c=(CapsuleCollider)player.HitCollider;return c.radius*Mathf.Max(Mathf.Abs(c.transform.lossyScale.x),Mathf.Abs(c.transform.lossyScale.z));}
        private static float HalfX()=>CombatHitGeometry.EnemyBodyBounds(enemy).extents.x;
        private static void SpawnTarget(){Spawn();pattern.EndSpawn();enemy.GetComponent<EnemyContactDamage>().SetDamageActive(false);enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth,500,true);}
        private static void PlaceTarget(Vector3 p){enemy.GetComponent<Rigidbody>().position=p;enemy.transform.position=p;Physics.SyncTransforms();}
        private static void SetFacing(Vector3 d){typeof(PlayerController).GetField("movement",Private).SetValue(player,Vector3.zero);typeof(PlayerController).GetField("lastMoveDirection",Private).SetValue(player,d);}
        private static int Area(Vector3 center,float radius,bool arc)
        {
            var controller=player.GetComponent<PlayerClassSkillController>();Action<EnemyController> apply=e=>e.TakeDamage(1);
            return (int)typeof(PlayerClassSkillController).GetMethod(arc?"ApplyForwardArcSkill":"ApplyAreaSkill",Private).Invoke(controller,
                arc?new object[]{center,radius,90f,8,apply,0f}:new object[]{center,radius,apply,0f});
        }
    }
}
