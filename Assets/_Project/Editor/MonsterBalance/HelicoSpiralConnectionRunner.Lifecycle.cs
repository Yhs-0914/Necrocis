using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Necrocis;
using UnityEngine;
namespace NecrocisEditor
{
    public static partial class HelicoSpiralConnectionRunner
    {
        private static IEnumerator EdgesAndSources()
        {
            foreach(float offset in new[]{.375f,.425f})
            {
                Spawn();Move(origin+Vector3.right*2.6f);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,2,"edge warning");
                Move(origin+Vector3.right*2+Vector3.forward*offset);health.ResetHealth();
                yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,2,"edge crossing");
                Equal(offset<.4f?3:0,100-health.CurrentHealth,"ground-center red boundary");Move(origin+Vector3.forward*4);
                Pass($"EDGE {offset:F3}m: ground center {(offset<.4f?"inside hits3":"outside stays safe")}, no added player radius to red footprint");
            }
            Spawn();float oldHp=enemy.Stats.MaxHealth,oldTell=pattern.WindupDuration;
            Set(definition,"statSets.Array.data[0].maxHealth",42);Set(definition,"statSets.Array.data[0].attackPower",4);
            var profile=DifficultyBalanceService.GetProfile(GameDifficulty.Normal);Set(profile,"elites.maxHealth",1.5f);Set(profile,"elites.outgoingDamage",1.5f);
            int coefficient=definition.patternDamage.FindIndex(p=>p.id==HelicoSpiralPatternSettings.DamageId);Set(definition,$"patternDamage.Array.data[{coefficient}].coefficient",2);
            Set(settings,"windupSeconds",.4f);Set(settings,"dashDistance",2.5f);Set(settings,"dashSeconds",.5f);Set(settings,"bodyWidth",1);Set(settings,"bodyHalfLength",.25f);Set(settings,"recoverySeconds",.5f);Set(settings,"rearmSeconds",1.2f);
            Equal(oldHp,enemy.Stats.MaxHealth,"old HP snapshot");Equal(oldTell,pattern.WindupDuration,"old tell snapshot");
            Spawn();Equal(63,enemy.Stats.MaxHealth,"new HP63");Equal(.4f,pattern.WindupDuration,"new tell");Equal(.5f,pattern.DashDuration,"new dash time");Equal(.5f,pattern.BodyRadius,"new width");
            Move(origin+Vector3.right*1.8f);yield return Wait(()=>pattern.DamageApplications==1,3,"edited actual damage");Equal(12,100-health.CurrentHealth,"edited actual damage12");Move(origin+Vector3.forward*4);
            yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Ready,2,"edited recovery");Equal(2.5f,pattern.Travelled,"edited distance");Require(pattern.NextReadyTime-Time.time>1.1f&&pattern.NextReadyTime-Time.time<=1.21f,"edited rearm once");
            Restore();Spawn();Equal(30,enemy.Stats.MaxHealth,"HP restored");Equal(.7f,pattern.WindupDuration,"tell restored");
            Pass("INSPECTOR: existing snapshot stable, new HP63/actual damage12, edited tell .4/distance2.5/time.5/width1/recovery.5/rearm1.2; originals restored");
        }
        private static IEnumerator Lifecycle()
        {
            Set(settings,"spawnGraceSeconds",100);
            foreach(float direction in new[]{0f,45f,90f,135f,180f,225f,270f,315f})
            {
                Spawn();Move(origin+Aim(direction)*2);float hp=health.CurrentHealth;
                for(int n=0;n<40&&health.CurrentHealth==hp;n++){player.TryMoveByWorld(-Aim(direction)*.04f);yield return new WaitForFixedUpdate();}
                Equal(1,hp-health.CurrentHealth,"oriented native contact");Require(enemy.PatternOwnsContact,"no parallel common box contact");
                Pass($"CONTACT {direction:0}: native walking into narrow oriented body gives1; generous receiving box is not incoming damage area");
            }
            Restore();Spawn();Move(origin+Vector3.right*2.6f);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,2,"stun warning");enemy.StatusEffects.ApplyStun(1);
            yield return new WaitForSeconds(.2f);Require(pattern.DashCount==0&&pattern.TelegraphObject==null&&!enemy.IsPatternPositionLocked&&!enemy.IsPatternFacingLocked,"stun cancellation");
            Pass("STUN: warning cancels, no dash, marker and locks cleared; body contact remains enabled");
            Spawn();Move(origin+Vector3.right*2.6f);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,2,"late wall warning");
            var cell=biome.WorldToGrid(origin+Vector3.right*2);biome.AddRuntimeBlockedCells(new[]{cell});
            try
            {
                Move(origin+Vector3.right*4);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,2,"wall stop");
                Require(pattern.WallStopped&&pattern.Travelled<2&&pattern.TelegraphObject==null,"stops before wall, no sliding");Equal(100,health.CurrentHealth,"wall protects player");
            }
            finally{biome.RemoveRuntimeBlockedCells(new[]{cell});}
            Pass("WALL: new blocked cell after warning stops straight dash, no sliding/through-wall damage and marker clears");
            foreach(float direction in new[]{0f,45f,90f,135f,180f,225f,270f,315f})
            {
                Spawn();Move(origin+Aim(direction)*2.6f);yield return Wait(()=>pattern.DashCount==1,3,"death dash");
                var facing=enemy.PatternFacing;var body=Body();var snap=settings.presentation.directionalPresentation.Capture();var wanted=enemy.Config.deathSprites.Select(f=>snap.Resolve(f,facing)).ToArray();uint gen=enemy.SpawnGeneration;
                enemy.TakeDamage(10000);Require(pattern.TelegraphObject==null&&enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount==0,"death clears attack");Move(enemy.transform.position);health.ResetHealth();var frames=new HashSet<Sprite>();float until=Time.time+2;
                while(enemy.gameObject.activeSelf)
                {
                    Require(Time.time<until,"death timeout");frames.Add(body.sprite);yield return new WaitForEndOfFrame();
                    if(enemy.gameObject.activeSelf)CheckGround();
                }
                Require(wanted.All(frames.Contains),"all six correct death frames");Equal(100,health.CurrentHealth,"no dead contact");
                Spawn();Require(enemy.SpawnGeneration!=gen&&pattern.DashCount==0&&!enemy.IsPatternFacingLocked&&!enemy.IsPatternPositionLocked,"fresh pooled generation");
                Pass($"DEATH {direction:0}: six direction-specific frames, immediate marker/attack cleanup, no dead damage, clean reuse");
            }
            enemy.ReleaseToPool();origin=fieldOrigin;field.enabled=true;Move(origin+Vector3.right*5);field.Refresh(player.transform.position);
            enemy=field.Spawners.Single().ActiveEnemy;var point=field.Plan.placements.Single();int xp=0;Action<int> observe=n=>xp+=n;var callback=LevelUpManager.OnLevelUp;
            LevelUpManager.OnLevelUp=null;LevelUpManager.OnExpGained+=observe;
            try{enemy.TakeDamage(10000);enemy.TakeDamage(10000);enemy.GrantExp();}
            finally{LevelUpManager.OnLevelUp=callback;LevelUpManager.OnExpGained-=observe;}
            Equal(50,xp,"field XP once");Require(SaveService.IsBiomeEliteDefeated(point.spawnId),"field defeat saved");
            yield return Wait(()=>!enemy.gameObject.activeSelf,2,"field death");field.Refresh(player.transform.position);
            Require(field.Spawners.Count==0&&source.monsters.Count==0,"no field respawn, no production registration");
            Pass("FIELD: actual dedicated death gives XP50 exactly once, saves defeat and does not respawn; Stomach production remains0");
        }
    }
}
