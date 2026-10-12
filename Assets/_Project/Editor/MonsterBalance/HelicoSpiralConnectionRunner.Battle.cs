using System;
using System.Collections;
using System.Linq;
using Necrocis;
using UnityEngine;
namespace NecrocisEditor
{
    public static partial class HelicoSpiralConnectionRunner
    {
        private static readonly float[] BattleAngles = { 0,45,90,135,180,225,270,315 };
        private static IEnumerator BattleChecks(string section)
        {
            if(section=="all"||section=="states")yield return StateChecks();
            if(section=="all"||section=="terrain")yield return TerrainChecks();
            if(section=="all"||section=="death")yield return MoreDeathChecks();
            if(section=="all"||section=="field")yield return FieldUnloadChecks();
            if(section=="visits"){yield return Lifecycle();yield return VisitChecks();}
            if(section=="repeat")yield return StateInterruptions();
            if(section=="sorting")yield return SortingChecks();
            if(section=="outgoing")yield return OutgoingChecks();
        }
        private static IEnumerator WalkTo(Vector3 target,float seconds)
        {
            float until=Time.time+seconds;
            while(true)
            {
                Vector3 delta=target-player.transform.position;delta.y=0;if(delta.magnitude<.06f)yield break;
                Require(Time.time<until,"native walk timeout "+player.transform.position+" -> "+target);
                player.TryMoveByWorld(delta.normalized*Mathf.Min(delta.magnitude,player.MoveSpeed*Time.fixedDeltaTime));yield return new WaitForFixedUpdate();
            }
        }
        private static void Face(Vector3 direction)
        {
            typeof(PlayerController).GetField("movement",Private).SetValue(player,Vector3.zero);
            typeof(PlayerController).GetField("lastMoveDirection",Private).SetValue(player,direction);
        }
        private static void BeginPlayerDash(Vector3 direction)
        {
            Face(direction);player.StartCoroutine((IEnumerator)typeof(PlayerController).GetMethod("DashCoroutine",Private).Invoke(player,null));
            Require(player.IsDashInvincible,"native dash invulnerability active");
        }
        private static IEnumerator StateChecks()
        {
            foreach(float direction in BattleAngles)
            {
                Spawn();enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed,1);Move(origin+Aim(direction)*6);
                yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,5,"native pursuit");
                Require(Vector3.Distance(origin,enemy.GetComponent<Rigidbody>().position)>.8f,"real movement enters trigger");
                var aim=pattern.LockedDirection;var facing=enemy.PatternFacing;
                Move(origin-Aim(direction)*3);yield return Wait(()=>pattern.DashCount==1,2,"committed pursuit dash");
                Require(enemy.PatternFacing==facing&&Vector3.Angle(aim,pattern.LockedDirection)<.01f,"pursuit does not home after tell");
                yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,1,"pursuit recovery");
                Pass($"PURSUIT {direction:0}: base-speed movement enters range, view locks, relocated target cannot redirect dash");
            }
            foreach(float direction in BattleAngles)foreach(int sign in new[]{-1,1})
                yield return WalkingCounter(direction,sign,false);
            foreach(float direction in BattleAngles)
            {
                Spawn();Move(origin+Aim(direction)*2.6f);yield return Wait(()=>pattern.DashCount==1,3,"native dash encounter");
                BeginPlayerDash(-Aim(direction));yield return Wait(()=>pattern.HitAttempts==1,1,"actual invulnerable crossing");
                Equal(100,health.CurrentHealth,"native dash absorbs actual hit");
                yield return Wait(()=>!player.IsDashInvincible,1,"native dash ends");Move(origin+Aim(direction+90)*4);
                yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Ready,2,"dash encounter settles");Equal(100,health.CurrentHealth,"no later hit from cycle");
                Pass($"DASH {direction:0}: actual head-on native dash crosses attack, consumes one attempt, damage0");
            }
            Set(settings,"spawnGraceSeconds",100);Spawn();
            foreach(float center in BattleAngles)
            {
                foreach(var s in new[]{(0f,0),(20f,0),(28f,0),(31f,1),(24f,1),(14f,0)})
                {
                    Move(origin+Aim(center+s.Item1)*3);yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();
                    var expected=(EnemyFacing)((Mathf.RoundToInt(center/45)+s.Item2)%8);
                    Require(enemy.PatternFacing==expected,$"view boundary {center}+{s.Item1}: {enemy.PatternFacing} != {expected}");CheckGround();
                    var bank=settings.presentation.directionalPresentation.Capture();Require(Body().flipX==bank.Flip(expected),"matching mirror");
                }
            }
            Restore();Pass("FACING: all eight boundaries preserve 8-degree hysteresis, correct mirror/bank and body-bottom ground alignment");
            foreach(float direction in new[]{45f,135f,225f,315f})foreach(float offset in new[]{.375f,.425f})
                yield return EdgeAt(direction,offset,60,false);
            yield return StateInterruptions();
            foreach(int fps in new[]{60,15})foreach(float direction in new[]{0f,45f,270f})foreach(float offset in new[]{.375f,.425f})
                yield return EdgeAt(direction,offset,fps,true);
            yield return SortingChecks();
            yield return OutgoingChecks();
        }
        private static IEnumerator WalkingCounter(float direction,int sign,bool stopForPreview)
        {
            Spawn();enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth,200,true);var aim=Aim(direction);var side=Vector3.Cross(aim,Vector3.up)*sign;
            Move(origin+aim*2.6f);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,2,"walking tell");
            yield return WalkTo(origin+aim*2.6f+side*1.6f,1);
            yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,2,"walking dodge finishes");
            yield return WalkTo(pattern.DashEnd-aim*1.6f,1);
            Face(aim);Physics.SyncTransforms();float before=enemy.Stats.CurrentHealth;
            typeof(PlayerAttack).GetMethod("MeleeAttack",Private).Invoke(player.GetComponent<PlayerAttack>(),null);
            Require(enemy.Stats.CurrentHealth<before,"native walking reaches Q counterattack");Equal(100,health.CurrentHealth,"native walk/counter has no damage");
            if(stopForPreview)yield break;
            yield return WalkTo(pattern.DashEnd-aim*1.6f+side*1.6f,1);
            yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Ready,2,"walking recovery settles");Equal(100,health.CurrentHealth,"safe retreat after Q");
            Pass($"WALK/Q {direction:0}/side{sign}: actual perpendicular walk, approach, Q and retreat; no teleport after starting point, damage0");
        }
        private static IEnumerator EdgeAt(float direction,float offset,int fps,bool lowFrameTest)
        {
            int oldRate=Application.targetFrameRate,oldSync=QualitySettings.vSyncCount;
            try
            {
                Application.targetFrameRate=fps;QualitySettings.vSyncCount=0;Spawn();var aim=Aim(direction);
                Move(origin+aim*2.6f);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,3,"edge tell");
                Move(origin+aim*2+Vector3.Cross(aim,Vector3.up)*offset);health.ResetHealth();
                yield return Wait(()=>pattern.DashCount==1,2,"edge dash");float maxDt=Time.deltaTime;
                while(pattern.Phase==HelicoSpiralPhase.Dash)
                {yield return new WaitForEndOfFrame();maxDt=Mathf.Max(maxDt,Time.deltaTime);CheckGround();}
                Equal(offset<.4f?3:0,100-health.CurrentHealth,"swept capsule edge");Equal(3.5f,pattern.Travelled,"full distance at frame rate");
                if(fps==15)Require(maxDt>.045f,"actual slow frame observed");
                Move(origin+Vector3.forward*4);
                Pass($"{(lowFrameTest?"FRAME":"DIAGONAL EDGE")} {fps}fps/{direction:0}/{offset:F3}m: actual damage{(offset<.4f?3:0)}, dt={maxDt:F3}s, exact distance and ground alignment");
            }
            finally{Application.targetFrameRate=oldRate;QualitySettings.vSyncCount=oldSync;}
        }
        private static IEnumerator StateInterruptions()
        {
            Set(settings,"dashSeconds",.8f);Spawn();Move(origin+Vector3.right*2.6f);yield return Wait(()=>pattern.DashCount==1,3,"slow dash budget");
            BeginPlayerDash(Vector3.left);yield return Wait(()=>pattern.HitAttempts==1,1,"protected attempt");Equal(100,health.CurrentHealth,"protected first contact");
            yield return Wait(()=>!player.IsDashInvincible,1,"protection ends");
            Move(enemy.GetComponent<Rigidbody>().position);yield return new WaitForSeconds(.1f);
            Require(pattern.Phase==HelicoSpiralPhase.Dash&&pattern.HitAttempts==1,"same slow dash still active");Equal(100,health.CurrentHealth,"spent attempt cannot retry after invulnerability");
            Move(origin+Vector3.forward*4);Restore();Pass("BUDGET: native dash absorbs first attempt; actual reentry after invulnerability during same dash remains harmless");
            Spawn();enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth,200,true);Move(origin+Vector3.right*2.6f);
            yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,2,"rapid attack tell");float began=Time.time;var locked=pattern.LockedDirection;
            enemy.StatusEffects.ApplyPoison(.8f,.1f,1);
            while(pattern.DashCount==0)
            {
                enemy.TakeDamage(1);enemy.ApplyKnockback(Vector3.left,.3f);Require(Time.time-began<1.05f,"hits do not extend preparation");
                Equal(0,Vector3.Distance(origin,enemy.GetComponent<Rigidbody>().position),"windup resists displacement");yield return new WaitForSeconds(.04f);
            }
            Require(enemy.Stats.CurrentHealth<200&&Vector3.Angle(locked,pattern.LockedDirection)<.01f,"damage applies without retarget");
            Move(origin+Vector3.forward*4);enemy.StatusEffects.ApplyStun(.5f);
            yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,1,"mid-dash stun");Require(pattern.Travelled>0&&pattern.Travelled<3.5f&&pattern.TelegraphObject==null,"stun stops moving attack");
            var stopped=enemy.GetComponent<Rigidbody>().position;yield return new WaitForSeconds(.2f);Equal(0,Vector3.Distance(stopped,enemy.GetComponent<Rigidbody>().position),"stunned dash stays stopped");
            yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Ready,2,"stun recovery");Require(pattern.DashCount==1&&!enemy.IsPatternFacingLocked&&!enemy.IsPatternPositionLocked,"stun does not restart attack");
            Pass("RAPID/POISON/STUN: damage applies, tell/direction fixed, mid-dash stun stops immediately and recovers without another charge");
            Set(settings,"rearmSeconds",0);Set(settings,"recoverySeconds",.1f);Spawn();
            int seen=0;float until=Time.time+8;Move(origin+Vector3.right*2.6f);
            while(seen<3)
            {
                Require(Time.time<until,$"three zero-rearm cycles: count={pattern.DashCount}, phase={pattern.Phase}, enemy={enemy.GetComponent<Rigidbody>().position}, hero={player.transform.position}, distance={pattern.PlannedDistance}, stunned={enemy.StatusEffects.IsStunned}, running={enemy.GetComponent<EnemyPatternLifetime>().IsCycleRunning}");
                if(pattern.Phase==HelicoSpiralPhase.Windup)Move(enemy.GetComponent<Rigidbody>().position+Vector3.forward*3);
                if(pattern.DashCount>seen)seen=pattern.DashCount;
                Require(enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount<=1,"only one owned warning");
                if(pattern.Phase==HelicoSpiralPhase.Recovery||pattern.Phase==HelicoSpiralPhase.Ready)Move(enemy.GetComponent<Rigidbody>().position-(pattern.LockedDirection.sqrMagnitude>.5f?pattern.LockedDirection:Vector3.right)*2.6f);
                yield return null;
            }
            Restore();enemy.ReleaseToPool();Pass("REPEAT: three zero-rearm cycles, one owned warning maximum, no leaked markers or locked state");
        }
        private static IEnumerator SortingChecks()
        {
            foreach(float direction in BattleAngles)
            {
                Spawn();Move(origin+Aim(direction)*2.6f);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,2,"overlap tell");
                health.GrantTemporaryInvincibility(2);
                foreach(float z in new[]{-.15f,.15f})
                {
                    Move(origin+new Vector3(.15f,0,z));yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();
                    var hero=player.GetComponentInChildren<SpriteRenderer>();
                    Require(Body().sortingLayerID==hero.sortingLayerID,"shared world layer");
                    Require(z<0?hero.sortingOrder>Body().sortingOrder:hero.sortingOrder<Body().sortingOrder,
                        $"ground depth sorting {direction}/{z}: hero={hero.sortingOrder}, enemy={Body().sortingOrder}, bodyZ={Body().transform.position.z}, physicsZ={enemy.GetComponent<Rigidbody>().position.z}");
                    CheckGround();
                }
                Pass($"VISIBILITY {direction:0}: player at front/back ground depth sorts correctly despite sprite lift; underside stays on ground axis");
            }
        }
        private static IEnumerator BattlePreview()
        {
            if(phase=="walk"){yield return WalkingCounter(angle,-1,true);yield break;}
            if(phase=="height")origin=FindPatch(1,6);
            Spawn();Move(origin+Aim(angle)*2.6f);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,2,"battle preview tell");
            if(phase=="wall")
            {
                var cell=biome.WorldToGrid(origin+Aim(angle)*2.8f);biome.AddRuntimeBlockedCells(new[]{cell});
                var obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);obstacle.name="S01_TestObstacle";
                obstacle.GetComponent<Collider>().enabled=false;
                obstacle.transform.position=biome.GridToWorldWithHeight(cell.x,cell.y)+Vector3.up*.5f;obstacle.transform.localScale=new Vector3(.98f,1,.98f);
                obstacle.GetComponent<Renderer>().material.color=new Color(.25f,.23f,.3f);
                Move(origin+Aim(angle)*4);
                yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,2,"wall preview stop");yield break;
            }
            if(phase=="dash")
            {
                yield return Wait(()=>pattern.DashCount==1,2,"dash preview release");BeginPlayerDash(-Aim(angle));
                yield return Wait(()=>pattern.HitAttempts==1,1,"dash preview collision");yield break;
            }
            if(phase=="overlap-front"||phase=="overlap-back")
            {health.GrantTemporaryInvincibility(2);Move(origin+new Vector3(.15f,0,phase=="overlap-front"?-.15f:.15f));yield return new WaitForEndOfFrame();yield break;}
            Move(origin+Aim(angle+90)*3);yield return Wait(()=>pattern.DashCount==1,2,"height preview release");yield return new WaitForSeconds(.075f);
        }
    }
}
