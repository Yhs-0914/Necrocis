using System;
using System.Collections;
using UnityEngine;
namespace Necrocis
{
    public sealed class EnemyStatusRuntimeSnapshot
    {
        public struct Slow {public float Ratio,EndRemaining;}
        public float Stun,Vulnerability,VulnerabilityRatio,Freeze,AppliedSlow,NextSlow;
        public bool SlowRunning;
        public Slow[] Slows;
        public struct Dot
        {
            public HealthEffectSource Source;
            public bool HasSource,Running,Pending;
            public float EndRemaining,NextRemaining,Interval,Damage;
        }
        public Dot Poison,Bleed;
    }
    public partial class EnemyStatusEffectController
    {
        public EnemyStatusRuntimeSnapshot CaptureSuspension()
        {
            float now=Time.time;var slows=new EnemyStatusRuntimeSnapshot.Slow[moveSpeedSlows.Count];
            for(int i=0;i<slows.Length;i++)slows[i]=new EnemyStatusRuntimeSnapshot.Slow{Ratio=moveSpeedSlows[i].ratio,EndRemaining=moveSpeedSlows[i].endTime-now};
            return new EnemyStatusRuntimeSnapshot{
                Stun=stunEndTime-now,Vulnerability=vulnerabilityEndTime-now,VulnerabilityRatio=damageTakenIncreaseRatio,
                Freeze=freezeVisualEndTime-now,AppliedSlow=appliedSlowRatio,NextSlow=Mathf.Max(0,slowNextRefreshTime-now),SlowRunning=slowRoutine!=null,Slows=slows,
                Poison=new EnemyStatusRuntimeSnapshot.Dot{Source=poisonSource,HasSource=hasPoisonSource,Running=poisonCoroutine!=null,Pending=poisonWaitPending,EndRemaining=poisonEndTime-now,NextRemaining=Mathf.Max(0,poisonNextTickTime-now),Interval=poisonTickInterval,Damage=poisonTickDamage},
                Bleed=new EnemyStatusRuntimeSnapshot.Dot{Source=bleedSource,HasSource=hasBleedSource,Running=bleedCoroutine!=null,Pending=bleedWaitPending,EndRemaining=bleedEndTime-now,NextRemaining=Mathf.Max(0,bleedNextTickTime-now),Interval=bleedTickInterval,Damage=bleedTickDamage}};
        }
        public void RestoreSuspension(EnemyStatusRuntimeSnapshot state)
        {
            if(state==null)throw new ArgumentNullException(nameof(state));
            uint restoringGeneration=enemy.SpawnGeneration;
            ResetEffects();
            if(enemy==null||enemy.SpawnGeneration!=restoringGeneration)return;
            float now=Time.time;
            stunEndTime=now+state.Stun;vulnerabilityEndTime=now+state.Vulnerability;damageTakenIncreaseRatio=state.VulnerabilityRatio;freezeVisualEndTime=now+state.Freeze;
            foreach(var slow in state.Slows)moveSpeedSlows.Add(new MoveSpeedSlow{ratio=slow.Ratio,endTime=now+slow.EndRemaining});
            appliedSlowRatio=state.AppliedSlow;slowNextRefreshTime=now+state.NextSlow;
            if(appliedSlowRatio>0)enemy.Stats.AddModifier(CharacterStatType.MoveSpeed,-appliedSlowRatio,CharacterStatModifierMode.PercentAdd,this);
            if(enemy==null||enemy.SpawnGeneration!=restoringGeneration)return;
            poisonSource=state.Poison.Source;hasPoisonSource=state.Poison.HasSource;poisonEndTime=now+state.Poison.EndRemaining;poisonTickInterval=state.Poison.Interval;poisonTickDamage=state.Poison.Damage;poisonNextTickTime=now+state.Poison.NextRemaining;poisonWaitPending=state.Poison.Pending;
            bleedSource=state.Bleed.Source;hasBleedSource=state.Bleed.HasSource;bleedEndTime=now+state.Bleed.EndRemaining;bleedTickInterval=state.Bleed.Interval;bleedTickDamage=state.Bleed.Damage;bleedNextTickTime=now+state.Bleed.NextRemaining;bleedWaitPending=state.Bleed.Pending;
            resumeWaitersReserved=true;
            RefreshStatusVisuals();
        }
        public void ResumeSuspensionRoutines(EnemyStatusRuntimeSnapshot state)
        {
            if (!resumeWaitersReserved) return; // Reset/reconfigure during activation owns its new routines.
            resumeWaitersReserved=false;
            if (poisonCoroutine==null && hasPoisonSource)
                poisonCoroutine=StartCoroutine(state.Poison.Running?ResumeDot(state.Poison,true):PoisonRoutine());
            if (bleedCoroutine==null && hasBleedSource)
                bleedCoroutine=StartCoroutine(state.Bleed.Running?ResumeDot(state.Bleed,false):BleedRoutine());
            if (slowRoutine==null && moveSpeedSlows.Count>0)
                slowRoutine=StartCoroutine(slowReappliedBeforeResume?SlowRoutine():ResumeSlow(state.NextSlow));
            slowReappliedBeforeResume=false;
        }
        private IEnumerator ResumeDot(EnemyStatusRuntimeSnapshot.Dot saved,bool poison)
        {
            uint generation=enemy.SpawnGeneration;
            if(saved.Pending)
            {
                if(saved.NextRemaining>0)yield return new WaitForSeconds(saved.NextRemaining);else yield return null;
                if(enemy==null||enemy.SpawnGeneration!=generation||enemy.IsDead)yield break;
                if(poison)poisonWaitPending=false;else bleedWaitPending=false;
                enemy.TakeDamage(poison ? poisonTickDamage : bleedTickDamage, poison ? poisonSource : bleedSource);
                if(enemy==null||enemy.SpawnGeneration!=generation)yield break;
            }
            IEnumerator remaining=poison?PoisonRoutine():BleedRoutine();
            while(remaining.MoveNext())yield return remaining.Current;
            (remaining as IDisposable)?.Dispose();
        }
        private IEnumerator ResumeSlow(float remaining)
        {
            if(remaining>0)yield return new WaitForSeconds(remaining);else yield return null;
            RefreshMoveSpeedSlow();RefreshStatusVisuals();
            IEnumerator routine=SlowRoutine();while(routine.MoveNext())yield return routine.Current;
            (routine as IDisposable)?.Dispose();
        }
    }
}
