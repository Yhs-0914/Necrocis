using System;
using System.Threading;
namespace Necrocis
{
    // A logical actor survives suspension; native component generations still guard callbacks.
    // Contains no Unity object, coroutine or delegate. Never serialized into a save file.
    public sealed class EnemyLogicalLife
    {
        private static long nextId;
        public readonly long Id=Interlocked.Increment(ref nextId);
        private float stoppedAt,pausedTime;
        public bool IsSuspended {get;private set;}
        public bool IsRetired {get;private set;}
        private HealthDeltaAccumulator ledger;
        public float Clock(float now)=>(IsSuspended?stoppedAt:now)-pausedTime;
        public void Suspend(float now){if(IsRetired)throw new InvalidOperationException("Retired enemy life.");if(!IsSuspended){stoppedAt=now;IsSuspended=true;}}
        public void Resume(float now){if(IsRetired)throw new InvalidOperationException("Retired enemy life.");if(IsSuspended){pausedTime+=Math.Max(0,now-stoppedAt);IsSuspended=false;}}
        public void BindLedger(HealthDeltaAccumulator value)=>ledger=value;
        public void RemoveHealthSource(HealthEffectSource source)=>ledger?.RemoveSource(source);
        public void Retire(){IsRetired=true;ledger=null;}
        public HealthEffectSource Source(string effect)=>HealthEffectSource.ForOwner(this,effect);
    }
}
