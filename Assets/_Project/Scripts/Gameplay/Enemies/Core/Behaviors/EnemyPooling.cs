using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public partial class EnemyController
    {
        public static EnemyController Acquire(Transform parent, string name, int poolArchetypeId)
        {
            EnsurePoolRoot();
            Stack<EnemyController> pool = GetOrCreatePool(poolArchetypeId);

            while (pool.Count > 0)
            {
                EnemyController pooled = pool.Pop();
                if (pooled == null) continue;

                GameObject pooledObject = pooled.gameObject;
                pooledObject.name = name;
                pooled.poolArchetypeId = poolArchetypeId;
                pooled.transform.SetParent(parent, false);
                pooled.transform.localPosition = Vector3.zero;
                pooled.transform.localRotation = Quaternion.identity;
                pooled.transform.localScale = Vector3.one;
                return pooled;
            }

            GameObject enemyObject = new GameObject(name);
            enemyObject.transform.SetParent(parent, false);
            EnemyController controller = enemyObject.AddComponent<EnemyController>();
            controller.poolArchetypeId = poolArchetypeId;
            return controller;
        }

        public void Configure(IEnemySpawnOwner owner, EnemySpawnRuleConfig config, Vector3 anchorPosition, Vector3 spawnPosition)
        {
            Configure(owner, config, anchorPosition, spawnPosition, null);
        }

        public void Configure(IEnemySpawnOwner owner, EnemySpawnRuleConfig config, Vector3 anchorPosition, Vector3 spawnPosition,
            MonsterBalanceRuntime encounterBalance)
        {
            ConfigureWithContext(owner, config, anchorPosition, spawnPosition,
                EnemySpawnBindings.CaptureCurrent(config, spawnPosition, encounterBalance));
        }

        public void ConfigureWithContext(IEnemySpawnOwner owner, EnemySpawnRuleConfig config,
            Vector3 anchorPosition, Vector3 spawnPosition, EnemySpawnContext context)
            =>ConfigureWithContextCore(owner,config,anchorPosition,spawnPosition,context,null);

        private void ConfigureWithContextCore(IEnemySpawnOwner owner, EnemySpawnRuleConfig config,
            Vector3 anchorPosition, Vector3 spawnPosition, EnemySpawnContext context,EnemyRuntimeSnapshot resume)
        {
            if (config.monsterDefinition != null)
            {
                string validation = config.monsterDefinition.GetValidationError();
                if (validation != null) throw new System.InvalidOperationException(validation);
                if (context.Balance == null)
                    throw new System.InvalidOperationException(config.name + ": 명시적인 몬스터 밸런스가 필요합니다.");
            }
            uint configuringGeneration=++SpawnGeneration;
            configuringLifetime=true;
            if(resume!=null && gameObject.activeSelf)gameObject.SetActive(false);
            if(this==null||SpawnGeneration!=configuringGeneration)return;
            if(logicalLife!=null&&!logicalLife.IsSuspended&&!ReferenceEquals(logicalLife,resume?.Life))logicalLife.Retire();
            logicalLife=resume?.Life??new EnemyLogicalLife();pendingResume=resume;preparingResume=resume!=null;retiringLifetime=false;preservingSuspendedLife=false;
            ResetAggroEffects();
            if(this==null||SpawnGeneration!=configuringGeneration)return;
            BindDefeatApproval(null);
            ResetDamageGate();
            hitByPlayer = false;
            suppressKillCredit = false;
            foreach (MonsterPatternController pattern in GetComponents<MonsterPatternController>())
            {pattern.EndSpawn();if(this==null||SpawnGeneration!=configuringGeneration)return;}
            ResetPatternDirections();
            SetPatternPositionLocked(false);
            if (config.monsterDefinition != null && config.additionalBaseStats != null)
                foreach (CharacterStatValue value in config.additionalBaseStats)
                    if (value.statType == CharacterStatType.MaxHealth || value.statType == CharacterStatType.AttackPower
                        || value.statType == CharacterStatType.MoveSpeed)
                        throw new System.InvalidOperationException(config.name + ": 핵심 스탯은 MonsterDefinition에서만 조절하세요: " + value.statType);
            config = config.CreateRuntimeCopy();
            this.owner = owner;
            this.config = config;
            experienceGranted = false;
            PatternOwnsContact = false;
            SuppressExperienceReward = false;
            SpawnContext = context;
            EncounterRole = context.Role;
            Balance = context.Balance;
            poolArchetypeId = GetPoolArchetypeId(config);
            this.anchorPosition = anchorPosition;
            playerTransform = null;
            destination = spawnPosition;
            idleTimer = 0f;
            attackTimer = 0f;
            hasDestination = false;
            usingMoveAnimation = false;
            notifiedOwner = false;
            attackAnimPlaying = false;
            deathAnimPlaying = false;
            colliderExpanded = false;
            facingDirection = 3;
            currentLoopFrames = null;
            isCharging = false;
            chargeElapsed = 0f;
            chargeCurrentSpeed = 0f;
            chargeCooldownTimer = 0f;
            defeatEventRaised = false;
            ignoreMidBossArenaRestriction = context.IgnoreEncounterMovementRestrictions;
            hasCachedGroundHeight = false;
            aiSuppressed = false;

            transform.position = spawnPosition;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            EnsureComponents();
            stats.BeginHealthLifetime();
            statusEffectController?.Initialize(this);
            statusEffectController?.ResetEffects();
            if(this==null||SpawnGeneration!=configuringGeneration)return;
            enemySkillBridge?.Bind(this, statusEffectController);
            gameObject.tag = "Enemy";
            if(resume==null)ConfigureStats();
            if(this==null||SpawnGeneration!=configuringGeneration)return;
            ApplyPhysicsSetup();
            ConfigureContactBalance();
            ApplyVisualSetup();
            SetIdleAnimation();
            SyncHeight();
            EnemyReceivingShape.Bind(this, config.receivingProfile, spriteRenderer);

            enabled = config != null;
            if(resume!=null){ApplySuspendedState(resume);return;}
            logicalLife.BindLedger(stats.RuntimeHealthLedger);
            PublishConfiguredLife();
            CombatVfx.PlayEnemySpawn(this);

            // FSM 시작 → Idle
            currentState = null;
            ChangeState(EnemyIdleState.Instance);
            EnemyPatternBinder.Attach(config.monsterDefinition?.pattern, this);
        }

        private void PublishConfiguredLife()
        {
            configuringLifetime=false;
            if(ActiveMembership.Add(this))ActiveEnemies.Add(this);
            RegisterOrUpdateSpatialCell(GetCurrentPosition());
            if(!gameObject.activeSelf)gameObject.SetActive(true);
        }

        public void ReleaseToPool()
        {
            if (gameObject == null || !gameObject.activeSelf) return;
            uint releasingGeneration=SpawnGeneration;
            retiringLifetime=true;ActiveMembership.Remove(this);ActiveEnemies.Remove(this);UnregisterSpatialCell();
            if(!preservingSuspendedLife)logicalLife?.Retire();
            ResetAggroEffects();
            if(SpawnGeneration!=releasingGeneration)return;

            foreach (MonsterPatternController pattern in GetComponents<MonsterPatternController>())
            {pattern.EndSpawn();if(this==null||SpawnGeneration!=releasingGeneration)return;}

            // FSM Exit
            currentState?.Exit(this);
            if(this==null||SpawnGeneration!=releasingGeneration)return;
            currentState = null;

            BindDefeatApproval(null);
            GetComponent<EnemyPatternLifetime>()?.Cancel();
            if(this==null||SpawnGeneration!=releasingGeneration)return;
            EnemyProjectile.ReturnProjectilesOwnedBy(this);
            StopAllCoroutines();
            PrepareForPool();
            ResetPatternDirections();
            SetPatternPositionLocked(false);
            statusEffectController?.ResetEffects();
            if(this==null||SpawnGeneration!=releasingGeneration)return;
            EnsurePoolRoot();
            gameObject.SetActive(false);
            if(this==null||SpawnGeneration!=releasingGeneration)return;
            transform.SetParent(poolRoot, false);

            owner = null;
            Defeated = null;
            DamageTaken = null;
            DamageAccepted = null;
            config = null;
            Balance = null;
            SpawnContext = default;
            EncounterRole = EnemyEncounterRole.Field;
            SpawnGeneration++;
            playerTransform = null;
            destination = Vector3.zero;
            idleTimer = 0f;
            attackTimer = 0f;
            hasDestination = false;
            usingMoveAnimation = false;
            attackAnimPlaying = false;
            deathAnimPlaying = false;
            colliderExpanded = false;
            facingDirection = 3;
            currentLoopFrames = null;
            isCharging = false;
            ignoreMidBossArenaRestriction = false;
            hasCachedGroundHeight = false;
            aiSuppressed = false;

            logicalLife=null;pendingResume=null;preparingResume=false;preservingSuspendedLife=false;retiringLifetime=false;configuringLifetime=false;
            GetOrCreatePool(poolArchetypeId).Push(this);
        }


        private void OnDisable()
        {
            if(!preservingSuspendedLife&&!preparingResume)logicalLife?.Retire();
            ResetDamageGate();
            stats?.BeginHealthLifetime();
            ResetAggroEffects();
            UnregisterSpatialCell();
            ActiveEnemies.Remove(this);
            ActiveMembership.Remove(this);
            NotifyOwnerReleased();
        }


        private void OnDestroy()
        {
            if(!preservingSuspendedLife)logicalLife?.Retire();
            ResetDamageGate();
            BindDefeatApproval(null);
            UnregisterSpatialCell();
            ActiveEnemies.Remove(this);
            ActiveMembership.Remove(this);
            NotifyOwnerReleased();
        }


        private void EnsurePlayerTransform()
        {
            if (playerTransform != null) return;
            if (PlayerController.Instance != null)
                playerTransform = PlayerController.Instance.transform;
        }


        private void NotifyOwnerReleased()
        {
            if (notifiedOwner || owner == null) return;
            notifiedOwner = true;
            if (owner is UnityEngine.Object value && value == null) return;
            owner.NotifyEnemyReleased(this);
        }


        private void RaiseDefeated()
        {
            if (defeatEventRaised)
            {
                return;
            }

            defeatEventRaised = true;
            Defeated?.Invoke(this);
        }


        private void PrepareForPool()
        {
            if (animatedSprite != null)
            {
                animatedSprite.Stop();
                animatedSprite.enabled = false;
            }
            currentLoopFrames = null;

            if (spriteRenderer != null)
                spriteRenderer.flipX = false;

            if (body != null)
            {
                if (!body.isKinematic)
                    body.angularVelocity = Vector3.zero;
                body.rotation = Quaternion.identity;
            }

            // 콜라이더 복원
            if (boxCollider != null && config != null)
                boxCollider.enabled = config.addCollider;
        }


        private static void EnsurePoolRoot()
        {
            if (poolRoot != null) return;
            GameObject root = GameObject.Find(PoolRootName);
            if (root == null) root = new GameObject(PoolRootName);
            poolRoot = root.transform;
        }


        public static int GetPoolArchetypeId(EnemySpawnRuleConfig config)
        {
            if (config == null) return 0;
            return unchecked((config.poissonSalt * 397) ^ Animator.StringToHash(config.name ?? "Enemy"));
        }


        private static Stack<EnemyController> GetOrCreatePool(int poolArchetypeId)
        {
            if (!PooledEnemies.TryGetValue(poolArchetypeId, out Stack<EnemyController> pool))
            {
                pool = new Stack<EnemyController>();
                PooledEnemies.Add(poolArchetypeId, pool);
            }
            return pool;
        }

    }
}
