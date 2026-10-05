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
            foreach (MonsterPatternController pattern in GetComponents<MonsterPatternController>()) pattern.EndSpawn();
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
            SpawnGeneration++;
            experienceGranted = false;
            PatternOwnsContact = false;
            SuppressExperienceReward = false;
            Balance = config.monsterDefinition != null ? MonsterBalanceRuntime.Capture(config.monsterDefinition) : null;
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
            hasAggroBoost = false;
            defeatEventRaised = false;
            ignoreMidBossArenaRestriction = false;
            hasCachedGroundHeight = false;
            aiSuppressed = false;

            transform.position = spawnPosition;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            EnsureComponents();
            statusEffectController?.Initialize(this);
            statusEffectController?.ResetEffects();
            enemySkillBridge?.Bind(this, statusEffectController);
            gameObject.tag = "Enemy";
            ConfigureStats();
            ApplyPhysicsSetup();
            ConfigureContactBalance();
            ApplyVisualSetup();
            SetIdleAnimation();
            SyncHeight();

            if (!ActiveEnemies.Contains(this))
            {
                ActiveEnemies.Add(this);
            }
            RegisterOrUpdateSpatialCell(GetCurrentPosition());

            enabled = config != null;
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            CombatVfx.PlayEnemySpawn(this);

            // FSM 시작 → Idle
            currentState = null;
            ChangeState(EnemyIdleState.Instance);
            config.monsterDefinition?.pattern?.Attach(this);
        }

        public void ReleaseToPool()
        {
            if (gameObject == null || !gameObject.activeSelf) return;

            foreach (MonsterPatternController pattern in GetComponents<MonsterPatternController>()) pattern.EndSpawn();

            // FSM Exit
            currentState?.Exit(this);
            currentState = null;

            GetComponent<EnemyPatternLifetime>()?.Cancel();
            EnemyProjectile.ReturnProjectilesOwnedBy(this);
            StopAllCoroutines();
            PrepareForPool();
            ResetPatternDirections();
            SetPatternPositionLocked(false);
            statusEffectController?.ResetEffects();
            EnsurePoolRoot();
            gameObject.SetActive(false);
            transform.SetParent(poolRoot, false);

            owner = null;
            Defeated = null;
            DamageTaken = null;
            config = null;
            Balance = null;
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
            hasAggroBoost = false;
            ignoreMidBossArenaRestriction = false;
            hasCachedGroundHeight = false;
            aiSuppressed = false;

            GetOrCreatePool(poolArchetypeId).Push(this);
        }


        private void OnDisable()
        {
            UnregisterSpatialCell();
            ActiveEnemies.Remove(this);
            NotifyOwnerReleased();
        }


        private void OnDestroy()
        {
            UnregisterSpatialCell();
            ActiveEnemies.Remove(this);
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
