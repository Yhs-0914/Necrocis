using System;
using System.Collections;
using System.Linq;
using Necrocis;
using UnityEngine;
using Object=UnityEngine.Object;
namespace NecrocisEditor
{
    public static partial class OilFilmConnectionRunner
    {
        private static readonly float[] BattleAngles={0,45,90,135,180,225,270,315};
        private static IEnumerator BattleChecks(string section)
        {
            if(section=="terrain")yield return TerrainChecks();
            if(section=="edges")yield return TerrainEdgeChecks();
            if(section=="states")yield return StateChecks();
            if(section=="lifecycle")yield return BattleLifecycleChecks();
            if(section=="frames")yield return FrameChecks();
            if(section=="visits")yield return VisitChecks();
        }
        private static IEnumerator WalkTo(Vector3 target,float seconds)
        {
            float until=Time.time+seconds;
            while(true)
            {
                Vector3 delta=target-player.transform.position;delta.y=0;if(delta.magnitude<.06f)yield break;
                Require(Time.time<until,"native walk timeout");player.TryMoveByWorld(delta.normalized*Mathf.Min(delta.magnitude,player.MoveSpeed*Time.fixedDeltaTime));yield return new WaitForFixedUpdate();
            }
        }
        private static void Face(Vector3 direction)
        {
            typeof(PlayerController).GetField("movement",Private).SetValue(player,Vector3.zero);
            typeof(PlayerController).GetField("lastMoveDirection",Private).SetValue(player,direction);
        }
        private static void BeginPlayerDash(Vector3 direction)
        {
            Face(direction);player.StartCoroutine((IEnumerator)typeof(PlayerController).GetMethod("DashCoroutine",Private).Invoke(player,null));Require(player.IsDashInvincible,"native dash active");
        }
        private static IEnumerator StateChecks()
        {
            foreach(float direction in BattleAngles)
            {
                Spawn();enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed,1);Move(origin+Aim(direction)*6);
                yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,5,"native pursuit");Require(Vector3.Distance(origin,pattern.Position)>.8f,"actual movement enters range");
                var point=pattern.Position;var aim=pattern.LockedDirection;var facing=enemy.PatternFacing;
                Move(origin-Aim(direction)*3);yield return Wait(()=>pattern.ImpactCount==1,3,"pursuit impact");
                Equal(0,Vector3.Distance(point,pattern.Position),"core stays put");Require(enemy.PatternFacing==facing&&Vector3.Angle(aim,pattern.LockedDirection)<.01f,"no retarget");Equal(100,health.CurrentHealth,"relocated target unharmed");
                Pass($"PURSUIT {direction}: native approach, fixed core/view, moved target cannot retarget attack");
            }
            foreach(float direction in BattleAngles)
            {
                foreach(int sign in direction%90==0?new[]{-1,1}:new[]{0})
                {
                    Spawn();enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth,200,true);Move(origin+Aim(direction)*2.1f);
                    yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"walking tell");
                    var forward=pattern.LockedDirection;var right=pattern.Footprint.Right;int chosen=sign==0?(Vector3.Dot(player.transform.position-origin,right)>=0?1:-1):sign;var side=right*chosen;
                    yield return WalkTo(origin+forward*.8f+side*1.8f,1.2f);yield return Wait(()=>pattern.Phase==OilFilmPhase.Recovery,2,"walk dodge");
                    Equal(100,health.CurrentHealth,"walk avoids area");yield return WalkTo(origin+side*1.05f,1);
                    Require(pattern.Phase==OilFilmPhase.Recovery,"counter within recovery opening");Face(-side);Physics.SyncTransforms();float before=enemy.Stats.CurrentHealth;
                    typeof(PlayerAttack).GetMethod("MeleeAttack",Private).Invoke(player.GetComponent<PlayerAttack>(),null);Require(enemy.Stats.CurrentHealth<before,"native Q counter");
                    if(direction==270&&chosen==1){yield return new WaitForEndOfFrame();Capture("walk-counter");}
                    yield return WalkTo(origin+side*3,1);Equal(100,health.CurrentHealth,"counter retreat safe");
                    Pass($"WALK/Q {direction}/side{chosen}: native sidestep, recovery approach/Q/retreat, damage0");
                }
                Spawn();Move(origin+Aim(direction)*2.1f);yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"backstep tell");
                yield return WalkTo(origin+Aim(direction)*3.4f,1);yield return Wait(()=>pattern.ImpactCount==1,2,"backstep impact");Equal(100,health.CurrentHealth,"outside range avoids");
                Pass($"BACKSTEP {direction}: actual walk beyond maximum reach avoids whole-area strike");
            }
            foreach(float direction in BattleAngles)
            {
                Spawn();Move(origin+Aim(direction)*2.2f);yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"dash align warning");
                yield return WalkTo(origin+pattern.LockedDirection*2.2f,.9f);yield return Wait(()=>pattern.Phase==OilFilmPhase.Release,3,"dash release");
                yield return new WaitForSeconds(.075f);BeginPlayerDash(-pattern.LockedDirection);
                yield return Wait(()=>pattern.ImpactCount==1,1,"dash impact");Require(pattern.HitAttempts==1,"dash actually crosses impact area");Equal(100,health.CurrentHealth,"dash invulnerability");
                if(direction==0){yield return new WaitForEndOfFrame();Capture("dash-cross");}
                yield return Wait(()=>!player.IsDashInvincible,1,"dash ends");Move(origin+Vector3.forward*4);Equal(100,health.CurrentHealth,"no late dash hit");
                Pass($"DASH {direction}: native dash crosses at impact, one protected attempt and damage0");
            }
            Set(settings,"spawnGraceSeconds",100);Spawn();
            foreach(var sample in new[]{(40f,EnemyFacing.Right),(48f,EnemyFacing.Right),(54f,EnemyFacing.Back),(46f,EnemyFacing.Back),(36f,EnemyFacing.Right),(320f,EnemyFacing.Right),(312f,EnemyFacing.Right),(306f,EnemyFacing.Front)})
            {
                Move(origin+Aim(sample.Item1)*2);yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();
                Require(enemy.PatternFacing==sample.Item2,"four-way hysteresis");Require(Body().flipX==settings.presentation.directionalPresentation.Capture().Flip(sample.Item2),"matching mirror");
            }
            Restore();Pass("FACING: four-direction hysteresis and side mirror stable around front/back boundaries");
            yield return OutgoingChecks();
        }
        private static IEnumerator OutgoingChecks()
        {
            Set(settings,"spawnGraceSeconds",100);
            foreach(float direction in BattleAngles)
            {
                Spawn();enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth,200,true);Move(origin-Aim(direction)*4);Face(Aim(direction));Physics.SyncTransforms();
                typeof(PlayerAttack).GetMethod("RangedAttack",Private).Invoke(player.GetComponent<PlayerAttack>(),null);
                yield return Wait(()=>enemy.Stats.CurrentHealth<200,1.5f,"native W reaches core");
                foreach(var p in Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None))p.gameObject.SetActive(false);
                yield return new WaitForFixedUpdate();Pass($"PLAYER W {direction}: native projectile hits vulnerable core at its actual receiving height");
            }
            Restore();
        }
    }
}
