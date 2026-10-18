using System;
using UnityEngine;
namespace Necrocis
{
    // In-memory, same-visit data only. No actor, component, coroutine or callback is retained.
    public sealed class EnemyRuntimeSnapshot
    {
        internal EnemySpawnRuleConfig Rule;
        internal MonsterBalanceRuntime Balance;
        internal EnemyLogicalLife Life;
        internal CharacterStats.RuntimeSnapshot Stats;
        internal EnemyStatusRuntimeSnapshot Status;
        internal SpriteFrameAnimator.PlaybackSnapshot Animation;
        internal IEnemyState State;
        internal Vector3 Position,Anchor,Destination,Scale,Velocity,AngularVelocity,VisualPosition,VisualScale,ColliderSize,ColliderCenter,ChargeDirection;
        internal Quaternion Rotation,VisualRotation;
        internal float Idle,Attack,ChargeElapsed,ChargeSpeed,ChargeCooldown,GroundHeight;
        internal bool HasDestination,MovingAnimation,AttackAnimation,Expanded,AiSuppressed,Charging,HitByPlayer,SuppressCredit,ExperienceGranted,DefeatRaised,SuppressXp,HasGround,ColliderEnabled,ContactEnabled;
        internal int Facing,SortingLayer,SortingOrder;
        internal Vector2Int GroundCell;
        internal Sprite[] LoopFrames;
        internal Sprite DisplayedSprite;
        internal Color Color;
        internal bool FlipX,FlipY;
        internal EnemyEncounterRole Role;
        internal bool IgnoreArena;
        internal AggroRuntimeSnapshot[] Aggro;
        public Vector3 SavedPosition=>Position;
        public float SavedHealth=>Stats.Health;
        public long LogicalId=>Life.Id;
        public void Retire()=>Life.Retire();
        internal sealed class AggroRuntimeSnapshot {internal object Source;internal int Legacy;internal float Speed,Chase;}
    }
    public partial class EnemyController
    {
        private EnemyLogicalLife logicalLife;
        private bool retiringLifetime,preparingResume,preservingSuspendedLife,configuringLifetime;
        private EnemyRuntimeSnapshot pendingResume;
        public EnemyLogicalLife LogicalLife=>logicalLife;
        public bool IsRetiringLifetime=>retiringLifetime||preparingResume||configuringLifetime||(logicalLife!=null&&logicalLife.IsRetired);

        public EnemyRuntimeSnapshot CaptureSuspension()
        {
            if(config==null||IsDead||IsRetiringLifetime||!gameObject.activeInHierarchy)throw new InvalidOperationException("Only a live configured enemy can be suspended.");
            if(config.monsterDefinition?.pattern!=null || patternDirections!=null)throw new InvalidOperationException("Pattern encounters require their own suspension policy.");
            var rule=config.CreateRuntimeCopy();if(aggroRuntimeConfig==config)rule.chaseRadius=aggroBaseChaseRadius;
            var aggro=new EnemyRuntimeSnapshot.AggroRuntimeSnapshot[aggroSources.Count];int index=0;
            foreach(var pair in aggroSources)aggro[index++]=new EnemyRuntimeSnapshot.AggroRuntimeSnapshot{Source=pair.Key,Legacy=ReferenceEquals(pair.Key,legacyChaseSource)?1:ReferenceEquals(pair.Key,legacySpeedSource)?2:0,Speed=pair.Value.SpeedMultiplier,Chase=pair.Value.ChaseMultiplier};
            var snapshot=new EnemyRuntimeSnapshot{
                Rule=rule,Balance=Balance,Life=logicalLife,Stats=stats.CaptureRuntime(source=>{
                    if(ReferenceEquals(source,statusEffectController)||ReferenceEquals(source,aggroModifierSource))return false;
                    if(source is UnityEngine.Object)throw new InvalidOperationException("Native modifier source requires an explicit suspension adapter.");return true;}),
                Status=statusEffectController.CaptureSuspension(),Animation=animatedSprite.CapturePlayback(),State=currentState,
                Position=GetCurrentPosition(),Rotation=transform.rotation,Scale=transform.localScale,Anchor=anchorPosition,Destination=destination,HasDestination=hasDestination,
                Idle=idleTimer,Attack=attackTimer,Facing=facingDirection,MovingAnimation=usingMoveAnimation,AttackAnimation=attackAnimPlaying,Expanded=colliderExpanded,AiSuppressed=aiSuppressed,
                Charging=isCharging,ChargeDirection=chargeDirection,ChargeElapsed=chargeElapsed,ChargeSpeed=chargeCurrentSpeed,ChargeCooldown=chargeCooldownTimer,
                HitByPlayer=hitByPlayer,SuppressCredit=suppressKillCredit,ExperienceGranted=experienceGranted,DefeatRaised=defeatEventRaised,SuppressXp=SuppressExperienceReward,
                HasGround=hasCachedGroundHeight,GroundHeight=cachedGroundHeight,GroundCell=cachedGroundGrid,LoopFrames=currentLoopFrames,Role=EncounterRole,IgnoreArena=ignoreMidBossArenaRestriction,Aggro=aggro,
                ColliderSize=boxCollider.size,ColliderCenter=boxCollider.center,ColliderEnabled=boxCollider.enabled,ContactEnabled=contactDamage.enabled,
                DisplayedSprite=spriteRenderer.sprite,Color=spriteRenderer.color,FlipX=spriteRenderer.flipX,FlipY=spriteRenderer.flipY,SortingLayer=spriteRenderer.sortingLayerID,SortingOrder=spriteRenderer.sortingOrder,
                VisualPosition=visualRoot.localPosition,VisualScale=visualRoot.localScale,VisualRotation=visualRoot.localRotation};
            if(body!=null&&!body.isKinematic){snapshot.Velocity=body.linearVelocity;snapshot.AngularVelocity=body.angularVelocity;}
            return snapshot;
        }
        public void ReleaseForSuspension(EnemyRuntimeSnapshot snapshot)
        {
            if(snapshot==null||!ReferenceEquals(snapshot.Life,logicalLife))throw new ArgumentException("Wrong logical enemy snapshot.");
            PrepareSuspension(snapshot);
            ReleaseToPool();
        }
        internal void PrepareSuspension(EnemyRuntimeSnapshot snapshot)
        {
            if(snapshot==null||!ReferenceEquals(snapshot.Life,logicalLife))throw new ArgumentException("Wrong logical enemy snapshot.");
            logicalLife.BindLedger(snapshot.Stats.Ledger);logicalLife.Suspend(Time.time);preservingSuspendedLife=true;
            // Exclude the old physical lifetime before cleanup can invoke synchronous subscribers.
            retiringLifetime=true;ActiveMembership.Remove(this);ActiveEnemies.Remove(this);UnregisterSpatialCell();SpawnGeneration++;
        }
        public void PrepareSuspendedRestore(IEnemySpawnOwner owner,EnemyRuntimeSnapshot snapshot,EnemySpawnContext context)
        {
            if(snapshot==null||snapshot.Life.IsRetired)throw new ArgumentException("Unavailable suspended enemy.");
            var restoredContext=new EnemySpawnContext(snapshot.Balance,context.WorldQueries,snapshot.Role,snapshot.IgnoreArena,context.Rules,context.DeathSpawns,context.Scope);
            ConfigureWithContextCore(owner,snapshot.Rule,snapshot.Anchor,snapshot.Position,restoredContext,snapshot);
        }
        private void ApplySuspendedState(EnemyRuntimeSnapshot state)
        {
            pendingResume=state;uint restoringGeneration=SpawnGeneration;
            bool Current()=>this!=null&&SpawnGeneration==restoringGeneration&&ReferenceEquals(pendingResume,state);
            stats.RestoreRuntime(state.Stats,Current);
            if(!Current())return;
            statusEffectController.RestoreSuspension(state.Status);
            if(!Current())return;
            anchorPosition=state.Anchor;destination=state.Destination;hasDestination=state.HasDestination;idleTimer=state.Idle;attackTimer=state.Attack;facingDirection=state.Facing;usingMoveAnimation=state.MovingAnimation;attackAnimPlaying=state.AttackAnimation;colliderExpanded=state.Expanded;aiSuppressed=state.AiSuppressed;
            isCharging=state.Charging;chargeDirection=state.ChargeDirection;chargeElapsed=state.ChargeElapsed;chargeCurrentSpeed=state.ChargeSpeed;chargeCooldownTimer=state.ChargeCooldown;
            hitByPlayer=state.HitByPlayer;suppressKillCredit=state.SuppressCredit;experienceGranted=state.ExperienceGranted;defeatEventRaised=state.DefeatRaised;SuppressExperienceReward=state.SuppressXp;
            hasCachedGroundHeight=state.HasGround;cachedGroundHeight=state.GroundHeight;cachedGroundGrid=state.GroundCell;currentLoopFrames=state.LoopFrames;currentState=state.State;
            transform.SetPositionAndRotation(state.Position,state.Rotation);transform.localScale=state.Scale;
            if(body!=null){body.position=state.Position;body.rotation=state.Rotation;if(!body.isKinematic){body.linearVelocity=state.Velocity;body.angularVelocity=state.AngularVelocity;}}
            boxCollider.size=state.ColliderSize;boxCollider.center=state.ColliderCenter;boxCollider.enabled=state.ColliderEnabled;contactDamage.enabled=state.ContactEnabled;
            visualRoot.localPosition=state.VisualPosition;visualRoot.localScale=state.VisualScale;visualRoot.localRotation=state.VisualRotation;
            uint generation=SpawnGeneration;Action complete=state.AttackAnimation?(Action)(()=>{if(SpawnGeneration==generation&&!IsRetiringLifetime&&gameObject.activeInHierarchy)OnAttackAnimationComplete();}):null;
            animatedSprite.RestorePlayback(state.Animation,complete);spriteRenderer.sprite=state.DisplayedSprite;spriteRenderer.color=state.Color;spriteRenderer.flipX=state.FlipX;spriteRenderer.flipY=state.FlipY;spriteRenderer.sortingLayerID=state.SortingLayer;spriteRenderer.sortingOrder=state.SortingOrder;
            EnemyReceivingShape.Bind(this,config.receivingProfile,spriteRenderer);
        }
        public bool PublishSuspendedRestore()
        {
            var state=pendingResume;if(!preparingResume||state==null)throw new InvalidOperationException("No prepared enemy.");
            stats.RuntimeHealthLedger.RestoreRuntime(state.Stats.Ledger);logicalLife.BindLedger(stats.RuntimeHealthLedger);logicalLife.Resume(Time.time);
            uint generation = SpawnGeneration;
            preparingResume=false;retiringLifetime=false;PublishConfiguredLife();
            if (!IsPublishedSnapshot(state, generation)) return false;
            statusEffectController.ResumeSuspensionRoutines(state.Status);
            if (!IsPublishedSnapshot(state, generation)) return false;
            foreach(var effect in state.Aggro)
            {
                object source=effect.Legacy==1?legacyChaseSource:effect.Legacy==2?legacySpeedSource:effect.Source;
                if(effect.Legacy==0&&!AggroDebris.IsSourceAlive(source))continue;
                if(TryRegisterAggroEffect(source,SpawnGeneration,effect.Speed,effect.Chase,false,out var handle))
                {
                    if(effect.Legacy==1)legacyChaseHandle=handle;
                    else if(effect.Legacy==2)legacySpeedHandle=handle;
                    else AggroDebris.ReconnectSource(source, this, handle);
                }
                if (!IsPublishedSnapshot(state, generation)) return false;
            }
            pendingResume=null;
            return true;
        }
        private bool IsPublishedSnapshot(EnemyRuntimeSnapshot state, uint generation)
            => this != null && SpawnGeneration == generation && ReferenceEquals(logicalLife, state.Life)
                && !IsRetiringLifetime && gameObject.activeInHierarchy;

        internal bool IsPreparedSnapshot(EnemyRuntimeSnapshot state)
            => this != null && preparingResume && ReferenceEquals(pendingResume, state)
                && ReferenceEquals(logicalLife, state.Life);

        public void DiscardPreparedRestore()
        {
            if(!preparingResume)return;
            StopAllCoroutines();statusEffectController?.ResetEffects();ResetAggroEffects();PrepareForPool();
            currentState=null;owner=null;config=null;Balance=null;SpawnContext=default;logicalLife=null;pendingResume=null;preparingResume=false;retiringLifetime=false;configuringLifetime=false;
            SpawnGeneration++;EnsurePoolRoot();gameObject.SetActive(false);transform.SetParent(poolRoot,false);GetOrCreatePool(poolArchetypeId).Push(this);
        }
    }
    public partial class EnemyController
    {
        internal void BindLooseSuspensionOwner(IEnemySuspensionOwner value)
        {if(owner!=null)throw new InvalidOperationException("An enemy already has a spawn owner.");owner=value;notifiedOwner=false;}
        public bool TrySuspendByOwner()=>owner is IEnemySuspensionOwner suspension&&suspension.TrySuspend(this);
    }
}
