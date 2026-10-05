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
        private static IEnumerator MoreDeathChecks()
        {
            foreach(float direction in BattleAngles)foreach(string state in new[]{"ready","windup","recovery"})
            {
                Spawn();Move(origin+Aim(direction)*2.6f);
                if(state=="ready"){yield return null;yield return null;}
                else
                {
                    yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,2,"death phase warning");
                    if(state=="recovery")
                    {Move(origin+Aim(direction+90)*3);yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Recovery,2,"death recovery");}
                }
                var facing=enemy.PatternFacing;var view=settings.presentation.directionalPresentation.Capture();
                var wanted=enemy.Config.deathSprites.Select(s=>view.Resolve(s,facing)).ToArray();var body=Body();uint gen=enemy.SpawnGeneration;
                var life=enemy.GetComponent<EnemyPatternLifetime>();enemy.TakeDamage(10000);
                Require(life.OwnedObjectCount==0&&pattern.TelegraphObject==null&&!enemy.IsPatternPositionLocked,"death cleanup immediate");
                Move(enemy.GetComponent<Rigidbody>().position);health.ResetHealth();float until=Time.time+2;var seen=new HashSet<Sprite>();
                while(enemy.gameObject.activeSelf)
                {
                    Require(Time.time<until&&body.flipX==view.Flip(facing),"death facing/replay completion");seen.Add(body.sprite);yield return new WaitForEndOfFrame();
                    if(enemy.gameObject.activeSelf)CheckGround();
                }
                Require(wanted.All(seen.Contains),"six matching death frames");Equal(100,health.CurrentHealth,"no corpse damage");
                Spawn();Require(enemy.SpawnGeneration!=gen&&pattern.DashCount==0&&pattern.HitAttempts==0&&pattern.PlannedDistance==0&&!enemy.IsPatternFacingLocked,"fresh pooled state");
                Pass($"DEATH {direction:0}/{state}: all six {facing} frames, ground underside, immediate cleanup/no late hit, clean next generation");
            }
            Spawn();Move(origin+Vector3.right*2.6f);yield return Wait(()=>pattern.DashCount==1,3,"manual release moving dash");
            var marker=pattern.TelegraphObject;var owner=enemy.GetComponent<EnemyPatternLifetime>();uint generation=enemy.SpawnGeneration;
            enemy.ReleaseToPool();Require(owner.OwnedObjectCount==0&&(marker==null||!marker.activeSelf),"manual pool clears live marker");
            Spawn();Require(enemy.SpawnGeneration!=generation&&pattern.DashCount==0&&!enemy.IsPatternFacingLocked&&!enemy.IsPatternPositionLocked,"manual pool returns clean");
            Pass("POOL: direct release during movement synchronously clears warning and attack; reused actor has no stale lock or hit budget");
        }
        private static IEnumerator FieldUnloadChecks()
        {
            var saved=origin;
            foreach(var state in new[]{HelicoSpiralPhase.Windup,HelicoSpiralPhase.Dash,HelicoSpiralPhase.Recovery})
            {
                if(enemy!=null)enemy.ReleaseToPool();origin=fieldOrigin;field.enabled=true;
                var approach=BattleAngles.Select(Aim).FirstOrDefault(a=>DustCloud.CanTravel(origin-a*settings.bodyHalfLength,origin+a*(settings.dashDistance+settings.bodyHalfLength),settings.bodyWidth*.5f));
                Require(approach.sqrMagnitude>.5f,"one safe full attack lane around actual field point");
                Move(origin+approach*2.6f);field.Refresh(player.transform.position);
                enemy=field.Spawners.Single().ActiveEnemy;enemy.Stats.SetBaseStat(CharacterStatType.MoveSpeed,0);pattern=enemy.GetComponent<HelicoSpiralElitePattern>();
                yield return Wait(()=>pattern.Phase==HelicoSpiralPhase.Windup,3,"field tell");Move(origin+Vector3.Cross(approach,Vector3.up)*3);
                if(state!=HelicoSpiralPhase.Windup)yield return Wait(()=>pattern.Phase==state,2,"field selected phase");
                var point=field.Plan.placements.Single();var life=enemy.GetComponent<EnemyPatternLifetime>();uint gen=enemy.SpawnGeneration;
                var marker=pattern.TelegraphObject;var oldPosition=enemy.GetComponent<Rigidbody>().position;
                Move(biome.GridToWorldWithHeight(point.x<150?280:15,point.y<150?280:15));field.Refresh(player.transform.position);yield return null;
                Require(!life.IsCurrent(gen)&&life.OwnedObjectCount==0&&(marker==null||!marker.activeSelf)&&!SaveService.IsBiomeEliteDefeated(point.spawnId),"unload clears without defeat");
                Move(origin+Vector3.right*5);field.Refresh(player.transform.position);
                enemy=field.Spawners.Single().ActiveEnemy;pattern=enemy.GetComponent<HelicoSpiralElitePattern>();
                Require(pattern.DashCount==0&&pattern.PlannedDistance==0&&pattern.HitAttempts==0&&!enemy.IsPatternPositionLocked&&!enemy.IsPatternFacingLocked,"same point fresh actor");
                Equal(0,Vector3.Distance(origin,enemy.GetComponent<Rigidbody>().position),"return to source point, not unfinished dash end");
                field.enabled=false;enemy=null;Pass($"CHUNK {state}: actual unload cancels owned state without defeat and returns at same placement with fresh attack state");
            }
            origin=saved;
        }
    }
}
