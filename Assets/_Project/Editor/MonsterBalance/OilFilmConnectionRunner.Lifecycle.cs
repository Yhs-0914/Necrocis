using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Necrocis;
using UnityEngine;
namespace NecrocisEditor
{
    public static partial class OilFilmConnectionRunner
    {
        private static IEnumerator SourceAndLifecycle()
        {
            Spawn();float oldHp=enemy.Stats.MaxHealth,oldRange=pattern.AttackRange;
            Set(definition,"statSets.Array.data[0].maxHealth",42);Set(definition,"statSets.Array.data[0].attackPower",4);
            var profile=DifficultyBalanceService.GetProfile(GameDifficulty.Normal);Set(profile,"elites.maxHealth",1.5f);Set(profile,"elites.outgoingDamage",1.5f);
            int coefficient=definition.patternDamage.FindIndex(p=>p.id==OilFilmPatternSettings.DamageId);Set(definition,$"patternDamage.Array.data[{coefficient}].coefficient",2);
            Set(settings,"attackRange",3f);Set(settings,"arcDegrees",90);Set(settings,"windupSeconds",.4f);Set(settings,"releaseSeconds",.2f);
            Set(settings,"impactNormalizedTime",.35f);Set(settings,"recoverySeconds",.5f);Set(settings,"rearmSeconds",1.2f);
            Equal(oldHp,enemy.Stats.MaxHealth,"existing HP snapshot");Equal(oldRange,pattern.AttackRange,"existing range snapshot");
            Spawn();Equal(63,enemy.Stats.MaxHealth,"new HP63");Equal(3,pattern.AttackRange,"new range3");Equal(90,pattern.ArcDegrees,"new arc90");Equal(.4f,pattern.WindupDuration,"new tell");
            Move(origin+Vector3.right*2.7f);yield return Wait(()=>pattern.ImpactCount==1,3,"edited impact");yield return new WaitForEndOfFrame();CheckSurface();Equal(12,100-health.CurrentHealth,"edited real damage12");Move(origin+Vector3.forward*4);
            yield return Wait(()=>pattern.Phase==OilFilmPhase.Ready,2,"edited recovery");Require(pattern.NextReadyTime-Time.time>1.1f&&pattern.NextReadyTime-Time.time<=1.21f,"edited rearm once");
            Restore();Pass("INSPECTOR: new HP63/damage12/range3/arc90/tell.4/release.2/impact.35/recovery.5/rearm1.2 applied; prior snapshot stable; sources restored");
            Set(settings,"spawnGraceSeconds",100);
            foreach(float direction in new[]{0f,45f,90f,135f,180f,225f,270f,315f})
            {
                Spawn();Move(origin+Aim(direction)*1.5f);float hp=health.CurrentHealth;
                for(int n=0;n<40&&health.CurrentHealth==hp;n++){player.TryMoveByWorld(-Aim(direction)*.04f);yield return new WaitForFixedUpdate();}
                Equal(1,hp-health.CurrentHealth,"native body contact1");Require(enemy.PatternOwnsContact,"no receiving-box contact duplication");
                Pass($"CONTACT {direction}: native walk into core damage1, broad membrane/receiving box not persistent contact");
            }
            Spawn();enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth,200,true);Move(origin-Vector3.right*1.2f);
            typeof(PlayerController).GetField("movement",Private).SetValue(player,Vector3.zero);typeof(PlayerController).GetField("lastMoveDirection",Private).SetValue(player,Vector3.right);
            float before=enemy.Stats.CurrentHealth;typeof(PlayerAttack).GetMethod("MeleeAttack",Private).Invoke(player.GetComponent<PlayerAttack>(),null);
            Require(enemy.Stats.CurrentHealth<before,"native Q hits receiving body");Pass("OUTGOING: native Q damages vulnerable core with existing forgiving receiving collider");Restore();
            Spawn();Move(origin+Vector3.right*1.6f);yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"stun warning");enemy.StatusEffects.ApplyStun(1);yield return new WaitForSeconds(.2f);
            Require(pattern.ImpactCount==0&&pattern.TelegraphObject==null&&!enemy.IsPatternFacingLocked&&!enemy.IsPatternPositionLocked,"warning stun cancels");Equal(100,health.CurrentHealth,"cancelled attack no damage");Pass("STUN: warning cancelled with no impact or retained locks/area");
            Spawn();Move(origin+Vector3.right*2);yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"wall tell");var cell=biome.WorldToGrid(origin+Vector3.right);
            biome.AddRuntimeBlockedCells(new[]{cell});
            try{yield return Wait(()=>pattern.ImpactCount==1,3,"wall impact");Equal(100,health.CurrentHealth,"wall blocks damage");}
            finally{biome.RemoveRuntimeBlockedCells(new[]{cell});}
            Pass("WALL: obstruction inserted after warning prevents damage through wall");
            foreach(float direction in new[]{0f,90f,180f,270f})
            {
                Spawn();Move(origin+Aim(direction)*1.6f);yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"death warning");
                var facing=enemy.PatternFacing;var body=Body();var snapshot=settings.presentation.directionalPresentation.Capture();var wanted=enemy.Config.deathSprites.Select(f=>snapshot.Resolve(f,facing)).ToArray();uint generation=enemy.SpawnGeneration;
                enemy.TakeDamage(10000);Require(pattern.TelegraphObject==null&&enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount==0,"death clears warning");Move(origin);health.ResetHealth();var seen=new HashSet<Sprite>();float until=Time.time+2;
                while(enemy.gameObject.activeSelf){Require(Time.time<until,"death timeout");seen.Add(body.sprite);yield return new WaitForEndOfFrame();Require(!enemy.GetComponent<OilFilmVisual>().IsUnfolded,"death restores approved death sprites");}
                Require(wanted.All(seen.Contains),"six direction-specific death frames");Equal(100,health.CurrentHealth,"no dead contact or late impact");Spawn();Require(enemy.SpawnGeneration!=generation&&pattern.ImpactCount==0&&!enemy.IsPatternFacingLocked,"fresh pooled spawn");
                Pass($"DEATH/POOL {direction}: six correct frames, no late/contact damage, mesh/warning cleared and clean reuse");
            }
            enemy.ReleaseToPool();origin=fieldOrigin;field.enabled=true;Move(origin+Vector3.right*5);field.Refresh(player.transform.position);enemy=field.Spawners.Single().ActiveEnemy;var point=field.Plan.placements.Single();
            int xp=0;Action<int> observe=n=>xp+=n;var callback=LevelUpManager.OnLevelUp;LevelUpManager.OnLevelUp=null;LevelUpManager.OnExpGained+=observe;
            try{enemy.TakeDamage(10000);enemy.TakeDamage(10000);enemy.GrantExp();}
            finally{LevelUpManager.OnLevelUp=callback;LevelUpManager.OnExpGained-=observe;}
            Equal(50,xp,"XP once");Require(SaveService.IsBiomeEliteDefeated(point.spawnId),"dedicated defeat saved");yield return Wait(()=>!enemy.gameObject.activeSelf,2,"field death");field.Refresh(player.transform.position);
            Require(field.Spawners.Count==0&&source.monsters.Count==0,"no field respawn or production registration");Pass("FIELD: dedicated defeat saved, XP50 exactly once, no respawn, production Stomach remains0");
        }
    }
}
