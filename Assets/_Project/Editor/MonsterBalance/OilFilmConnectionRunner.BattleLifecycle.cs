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
        private static IEnumerator BattleLifecycleChecks()
        {
            foreach(float direction in new[]{0f,90f,180f,270f})foreach(string state in new[]{"ready","windup","release","recovery"})
            {
                Spawn();Move(origin+Aim(direction)*1.6f);
                if(state=="ready"){yield return null;yield return null;}
                else
                {
                    yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"death windup");Move(origin+Aim(direction+100)*3);
                    if(state=="release")yield return Wait(()=>pattern.Phase==OilFilmPhase.Release,2,"death release");
                    if(state=="recovery")yield return Wait(()=>pattern.Phase==OilFilmPhase.Recovery,2,"death recovery");
                }
                var facing=enemy.PatternFacing;var snap=settings.presentation.directionalPresentation.Capture();var wanted=enemy.Config.deathSprites.Select(s=>snap.Resolve(s,facing)).ToArray();var body=Body();uint generation=enemy.SpawnGeneration;
                enemy.TakeDamage(10000);Require(pattern.TelegraphObject==null&&enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount==0&&!enemy.IsPatternPositionLocked,"death cleanup");
                Move(origin);health.ResetHealth();var seen=new HashSet<Sprite>();float until=Time.time+2;
                while(enemy.gameObject.activeSelf)
                {
                    Require(Time.time<until&&body.flipX==snap.Flip(facing),"death direction/timing");seen.Add(body.sprite);yield return new WaitForEndOfFrame();
                    Require(!enemy.GetComponent<OilFilmVisual>().IsSurfaceVisible,"dead membrane hidden");
                }
                Require(wanted.All(seen.Contains),"six visible death frames");Equal(100,health.CurrentHealth,"corpse/late damage0");Spawn();Require(enemy.SpawnGeneration!=generation&&pattern.ImpactCount==0&&pattern.HitAttempts==0&&pattern.LockedDirection==Vector3.zero,"fresh pooled actor");
                Pass($"DEATH {direction}/{state}: six matching frames/mirror, immediate cleanup and harmless clean reuse");
            }
            foreach(var state in new[]{OilFilmPhase.Windup,OilFilmPhase.Release,OilFilmPhase.Recovery})
            {
                Spawn();Move(origin+Vector3.right*1.6f);yield return Wait(()=>pattern.Phase==state,3,"stun phase");Move(origin+Vector3.forward*3);int impacts=pattern.ImpactCount;float hpBefore=health.CurrentHealth;
                enemy.StatusEffects.ApplyStun(.6f);yield return new WaitForSeconds(.2f);
                Require(pattern.ImpactCount==impacts&&pattern.TelegraphObject==null&&!enemy.IsPatternPositionLocked&&!enemy.IsPatternFacingLocked,"stun aborts selected state");Equal(hpBefore,health.CurrentHealth,"no stun late hit");
                Pass("STUN "+state+": cancels selected phase without late impact/marker/locks");
                Spawn();Move(origin+Vector3.right*1.6f);yield return Wait(()=>pattern.Phase==state,3,"pool phase");var life=enemy.GetComponent<EnemyPatternLifetime>();var marker=pattern.TelegraphObject;uint generation=enemy.SpawnGeneration;
                enemy.ReleaseToPool();Require(life.OwnedObjectCount==0&&(marker==null||!marker.activeSelf),"manual pool releases warning");Spawn();Require(enemy.SpawnGeneration!=generation&&pattern.ImpactCount==0&&!enemy.IsPatternFacingLocked,"manual pool resets");
                Pass("POOL "+state+": immediate owned cleanup and fresh next generation");
            }
            Spawn();enemy.Stats.SetBaseStat(CharacterStatType.MaxHealth,200,true);Move(origin+Vector3.right*1.6f);yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"damage pressure");float began=Time.time;
            var fixedAt=pattern.Position;var locked=pattern.LockedDirection;enemy.StatusEffects.ApplyPoison(.8f,.1f,1);
            while(pattern.Phase==OilFilmPhase.Windup){enemy.TakeDamage(1);enemy.ApplyKnockback(Vector3.left,.3f);Equal(0,Vector3.Distance(fixedAt,pattern.Position),"hit cannot displace fixed core");Require(Time.time-began<1.05f,"damage cannot extend tell");yield return new WaitForSeconds(.04f);}
            Require(enemy.Stats.CurrentHealth<200&&pattern.LockedDirection==locked,"damage accepted without retarget");Move(origin+Vector3.forward*4);Pass("PRESSURE: damage/poison applies during warning without knockback or endless preparation reset");
            Spawn();Move(origin+Vector3.right*1.6f);yield return Wait(()=>pattern.Phase==OilFilmPhase.Release,3,"invulnerable impact");health.GrantTemporaryInvincibility(1);
            yield return Wait(()=>pattern.ImpactCount==1,1,"protected attempt");Equal(100,health.CurrentHealth,"protected impact damage0");Require(pattern.HitAttempts==1,"protected attempt consumed");health.ResetHealth();yield return new WaitForSeconds(.2f);Equal(100,health.CurrentHealth,"no retry after immunity reset");Pass("BUDGET: one attempt remains spent after invulnerability ends, no lingering area damage");
            Spawn();Move(origin+Vector3.right*1.6f);health.GrantTemporaryInvincibility(40);
            for(int i=1;i<=4;i++){yield return Wait(()=>pattern.ImpactCount==i,7,"repeated impact");Move(origin+Vector3.forward*3);yield return Wait(()=>pattern.Phase==OilFilmPhase.Ready,2,"repeat clear");Require(pattern.TelegraphObject==null&&enemy.GetComponent<EnemyPatternLifetime>().OwnedObjectCount==0&&!enemy.IsPatternPositionLocked,"repeat no residue");Move(origin+Vector3.right*1.6f);}
            Pass("REPEAT: four complete cycles release markers/locks and permit exactly one impact each");
            yield return SortingChecks();yield return FieldUnloadChecks();
        }
        private static IEnumerator SortingChecks()
        {
            foreach(float direction in new[]{0f,90f,180f,270f})
            {
                Spawn();Move(origin+Aim(direction)*1.6f);yield return Wait(()=>pattern.Phase==OilFilmPhase.Release,3,"sorting release");health.GrantTemporaryInvincibility(3);
                yield return Wait(()=>pattern.ImpactCount==1,1,"sorting full extension");yield return new WaitForEndOfFrame();
                var hero=player.GetComponentInChildren<SpriteRenderer>();var film=enemy.transform.Find("OilFilmOriginalMembrane").GetComponent<MeshRenderer>();
                Require(film.enabled&&film.sortingOrder<hero.sortingOrder,"ground membrane behind player");
                Move(origin+new Vector3(.15f,0,-.15f));yield return new WaitForEndOfFrame();Require(hero.sortingOrder>Body().sortingOrder,"player in front sorts in front of core");
                Move(origin+new Vector3(.15f,0,.15f));yield return new WaitForEndOfFrame();Require(hero.sortingOrder<Body().sortingOrder,"player behind core sorts behind");
                Pass($"SORTING {direction}: membrane stays on ground under player, core respects actual body ground depth");
            }
        }
        private static IEnumerator FieldUnloadChecks()
        {
            var saved=origin;
            foreach(var state in new[]{OilFilmPhase.Windup,OilFilmPhase.Release,OilFilmPhase.Recovery})
            {
                if(enemy!=null)enemy.ReleaseToPool();origin=fieldOrigin;field.enabled=true;
                var aim=BattleAngles.Select(Aim).FirstOrDefault(a=>GasSacOrb.HasClearPath(origin,origin+a*1.6f));Require(aim.sqrMagnitude>.5f,"field attack approach");
                Move(origin+aim*1.6f);field.Refresh(player.transform.position);enemy=field.Spawners.Single().ActiveEnemy;enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed,0);pattern=enemy.GetComponent<OilFilmElitePattern>();
                yield return Wait(()=>pattern.Phase==OilFilmPhase.Windup,3,"field warning");Move(origin+Vector3.Cross(aim,Vector3.up)*3);if(state!=OilFilmPhase.Windup)yield return Wait(()=>pattern.Phase==state,2,"field phase");
                var point=field.Plan.placements.Single();var life=enemy.GetComponent<EnemyPatternLifetime>();uint gen=enemy.SpawnGeneration;var marker=pattern.TelegraphObject;
                Move(biome.GridToWorldWithHeight(point.x<150?280:15,point.y<150?280:15));field.Refresh(player.transform.position);yield return null;
                Require(!life.IsCurrent(gen)&&life.OwnedObjectCount==0&&(marker==null||!marker.activeSelf)&&!SaveService.IsBiomeEliteDefeated(point.spawnId),"chunk unload cancels without death");
                Move(origin+aim*5);field.Refresh(player.transform.position);enemy=field.Spawners.Single().ActiveEnemy;pattern=enemy.GetComponent<OilFilmElitePattern>();
                Require(pattern.ImpactCount==0&&pattern.Footprint==null&&!enemy.IsPatternPositionLocked&&!enemy.IsPatternFacingLocked,"return starts fresh");Equal(0,Vector3.Distance(origin,pattern.Position),"return to dedicated origin");
                field.enabled=false;enemy=null;Pass("CHUNK "+state+": unload/reload clears attack without defeat and restores fresh actor");
            }
            origin=saved;
        }
    }
}
