using UnityEngine;

namespace Necrocis
{
    public readonly struct ItemEffectRuntimeSettings
    {
        public readonly float RecoveryInterval, ShieldInterval, ShieldAmount, DecayInterval;
        public readonly int DecayMax, ContractPeriod, ContractMax;
        public ItemEffectRuntimeSettings(float recovery, float shieldInterval, float shieldAmount,
            float decayInterval, int decayMax, int contractPeriod, int contractMax)
        {
            RecoveryInterval=recovery;ShieldInterval=shieldInterval;ShieldAmount=shieldAmount;
            DecayInterval=decayInterval;DecayMax=decayMax;ContractPeriod=contractPeriod;ContractMax=contractMax;
        }
    }

    public interface IItemEffectRuntime
    {
        string ItemId { get; }
        void OnAcquire(float now, ItemEffectRuntimeSettings settings);
        void OnRemove(float now, ItemEffectRuntimeSettings settings);
        void Capture(SavedItemStateData target, float now);
        void Restore(SavedItemStateData source, float now, ItemEffectRuntimeSettings settings);
    }

    public abstract class ItemEffectRuntime : IItemEffectRuntime
    {
        public abstract string ItemId { get; }
        // Legacy acquisition preserves the removal-time schedule, including after an empty-inventory sleep.
        public virtual void OnAcquire(float now, ItemEffectRuntimeSettings settings) { }
        public abstract void OnRemove(float now, ItemEffectRuntimeSettings settings);
        public abstract void Capture(SavedItemStateData target, float now);
        public abstract void Restore(SavedItemStateData source, float now, ItemEffectRuntimeSettings settings);
    }

    public sealed class RecoveryFactorRuntime : ItemEffectRuntime
    {
        public override string ItemId => PlayerItemIds.RecoveryFactorId;
        public float ReadyAt { get; private set; }
        public void Schedule(float now,float interval) => ReadyAt=now+Mathf.Max(.1f,interval);
        public bool ShouldHeal(bool active,bool alive,float now,float interval)
        {
            if(!active || !alive){Schedule(now,interval);return false;}
            return now>=ReadyAt;
        }
        public override void OnRemove(float now,ItemEffectRuntimeSettings settings) => Schedule(now,settings.RecoveryInterval);
        public override void Capture(SavedItemStateData target,float now) => target.recoveryFactorCooldownRemaining=Mathf.Max(0f,ReadyAt-now);
        public override void Restore(SavedItemStateData source,float now,ItemEffectRuntimeSettings settings)
            => ReadyAt=now+Mathf.Max(0f,source.recoveryFactorCooldownRemaining);
    }

    public sealed class PlateletMembraneRuntime : ItemEffectRuntime
    {
        public override string ItemId => PlayerItemIds.PlateletMembraneId;
        public float Shield { get; private set; }
        public float ReadyAt { get; private set; }
        public void Tick(bool active,float now,float interval,float amount)
        {
            if(!active){Shield=0f;ReadyAt=now+Mathf.Max(.1f,interval);}
            else if(Shield<=0f && now>=ReadyAt)Shield=Mathf.Max(0f,amount);
        }
        public float Absorb(float damage,float now,float interval)
        {
            if(Shield<=0f)return damage;
            float absorbed=Mathf.Min(Shield,damage);Shield-=absorbed;damage-=absorbed;
            if(Shield<=0f){Shield=0f;ReadyAt=now+Mathf.Max(.1f,interval);}
            return damage;
        }
        public override void OnRemove(float now,ItemEffectRuntimeSettings settings)
        {Shield=0f;ReadyAt=now+Mathf.Max(.1f,settings.ShieldInterval);}
        public override void Capture(SavedItemStateData target,float now)
        {target.plateletMembraneShield=Mathf.Max(0f,Shield);target.plateletMembraneCooldownRemaining=Mathf.Max(0f,ReadyAt-now);}
        public override void Restore(SavedItemStateData source,float now,ItemEffectRuntimeSettings settings)
        {Shield=Mathf.Clamp(source.plateletMembraneShield,0f,Mathf.Max(0f,settings.ShieldAmount));ReadyAt=now+Mathf.Max(0f,source.plateletMembraneCooldownRemaining);}
    }

    public sealed class DecayOrganRuntime : ItemEffectRuntime
    {
        public override string ItemId => PlayerItemIds.DecayOrganId;
        public float StartedAt { get; private set; }=float.NegativeInfinity;
        public int AttackBonus { get; private set; }
        public void BeginRestore(bool active,float now) => StartedAt=active?now:float.NegativeInfinity;
        public void Tick(bool active,float now,float interval,int maximum)
        {
            if(!active){StartedAt=float.NegativeInfinity;AttackBonus=0;return;}
            if(StartedAt<=float.NegativeInfinity*.5f){StartedAt=now;AttackBonus=0;return;}
            float elapsed=Mathf.Max(0f,now-StartedAt);
            AttackBonus=Mathf.Clamp(Mathf.FloorToInt(elapsed/Mathf.Max(.1f,interval)),0,Mathf.Max(0,maximum));
        }
        public override void OnRemove(float now,ItemEffectRuntimeSettings settings)
        {StartedAt=float.NegativeInfinity;AttackBonus=0;}
        public override void Capture(SavedItemStateData target,float now)
            => target.decayOrganElapsedSeconds=StartedAt<=float.NegativeInfinity*.5f?0f:Mathf.Max(0f,now-StartedAt);
        public override void Restore(SavedItemStateData source,float now,ItemEffectRuntimeSettings settings)
            => StartedAt=now-Mathf.Max(0f,source.decayOrganElapsedSeconds);
    }

    public sealed class BloodContractRuntime : ItemEffectRuntime
    {
        public override string ItemId => PlayerItemIds.BloodContractId;
        public int KillProgress { get; private set; }
        public int HealthGainCount { get; private set; }
        public bool RecordKill(int period,int maximum)
        {
            if(HealthGainCount>=Mathf.Max(1,maximum)){KillProgress=0;return false;}
            KillProgress++;
            if(KillProgress<Mathf.Max(1,period))return false;
            KillProgress=0;return true;
        }
        // The facade commits after the synchronous MaxHP modifier callback, preserving event order.
        public void CommitHealthGain() => HealthGainCount++;
        public override void OnRemove(float now,ItemEffectRuntimeSettings settings){KillProgress=0;HealthGainCount=0;}
        public override void Capture(SavedItemStateData target,float now)
        {target.bloodContractKillProgress=Mathf.Max(0,KillProgress);target.bloodContractHealthGainCount=Mathf.Max(0,HealthGainCount);}
        public override void Restore(SavedItemStateData source,float now,ItemEffectRuntimeSettings settings)
        {
            KillProgress=Mathf.Clamp(source.bloodContractKillProgress,0,Mathf.Max(1,settings.ContractPeriod)-1);
            HealthGainCount=Mathf.Clamp(source.bloodContractHealthGainCount,0,Mathf.Max(1,settings.ContractMax));
        }
    }

    public sealed class SplitRegenerationRuntime : ItemEffectRuntime
    {
        public override string ItemId => PlayerItemIds.SplitRegenerationId;
        public bool Used { get; private set; }
        public bool TryConsume(){if(Used)return false;Used=true;return true;}
        public override void OnRemove(float now,ItemEffectRuntimeSettings settings) => Used=false;
        public override void Capture(SavedItemStateData target,float now) => target.splitRegenerationUsed=Used;
        public override void Restore(SavedItemStateData source,float now,ItemEffectRuntimeSettings settings) => Used=source.splitRegenerationUsed;
    }
}
