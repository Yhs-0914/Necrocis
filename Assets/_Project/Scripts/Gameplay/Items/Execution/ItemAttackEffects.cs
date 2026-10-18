using System;
using System.Collections.Generic;
using UnityEngine;
using static Necrocis.PlayerItemIds;
namespace Necrocis
{
    internal sealed class ItemAttackEffects
    {
        private readonly IItemCombatHost host;
        private IItemCombatSettings settings => host.Settings;
        private readonly IItemOwnershipView ownership;
        private readonly ItemCombatQueries queries;
        private readonly ItemTimedModifiers timed;
        internal ItemAttackEffects(IItemCombatHost host,IItemOwnershipView ownership,ItemCombatQueries queries,ItemTimedModifiers timed){this.host=host;this.ownership=ownership;this.queries=queries;this.timed=timed;}
        internal float tentacleNextAutoAttackTime;
        internal float overheatLastAttackTime = float.NegativeInfinity;
        internal float overheatNextDecayTime = float.PositiveInfinity;
        internal int overheatStacks;
        internal readonly Dictionary<EnemyLogicalLife, ResonanceState> resonanceStatesByEnemyId = new Dictionary<EnemyLogicalLife, ResonanceState>();
        internal readonly List<EnemyLogicalLife> tempResonanceRemovalIds = new List<EnemyLogicalLife>();
        internal float unstableCoreNextRerollTime;
        internal float unstableCoreCurrentMultiplier = 1f;
        internal bool unstableCoreInitialized;
        internal readonly List<EnemyController> randomEnemyCandidates = new List<EnemyController>();

        internal struct ResonanceState
        {
            public int Stacks;
            public float LastHitTime;
        }

        public int BeamHitBufferSize => Mathf.Max(1, settings.beamHitBufferSize);
        public float BeamRadius => Mathf.Max(0.05f, settings.beamRadius);
        public float BeamDamageMultiplier => Mathf.Max(0.05f, settings.beamDamageMultiplier);
        public float CellProliferationDamageMultiplier => Mathf.Max(0.05f, settings.cellProliferationDamageMultiplier);

        public int GetForwardProjectileCount()
        {
            if (ownership.HasItem(TripleCoreId))
            {
                return 3;
            }

            if (ownership.HasItem(DoubleCoreId))
            {
                return 2;
            }

            return 1;
        }

        public float GetSpreadAngleForCount(int projectileCount)
        {
            if (projectileCount >= 3)
            {
                return Mathf.Max(0f, settings.tripleShotSpreadAngle);
            }

            if (projectileCount == 2)
            {
                return Mathf.Max(0f, settings.doubleShotSpreadAngle);
            }

            return 0f;
        }

        public float GetForwardProjectileDamageMultiplier(int projectileCount)
        {
            if (projectileCount >= 3)
            {
                return Mathf.Clamp(settings.tripleShotDamageMultiplier, 0.05f, 1f);
            }

            if (projectileCount == 2)
            {
                return Mathf.Clamp(settings.doubleShotDamageMultiplier, 0.05f, 1f);
            }

            return 1f;
        }

        public float GetBackShotDamageMultiplier()
        {
            return Mathf.Max(0.05f, settings.backShotDamageMultiplier);
        }

        public bool RollCellProliferation()
        {
            return ownership.HasItem(CellProliferationId) && UnityEngine.Random.value <= settings.cellProliferationChance;
        }

        public float GetRangeMultiplier()
        {
            return ownership.HasItem(HypertrophyCellId) ? Mathf.Max(0.1f, settings.hypertrophyRangeMultiplier) : 1f;
        }

        public float GetScaleMultiplier()
        {
            return ownership.HasItem(HypertrophyCellId) ? Mathf.Max(0.1f, settings.hypertrophyScaleMultiplier) : 1f;
        }

        public int GetPiercingHitCount()
        {
            return ownership.HasPiercingMucus ? Mathf.Max(1, settings.piercingHitCount) : 1;
        }

        public float GetPiercingHitDamageMultiplier(int previousHitCount)
        {
            if (!ownership.HasPiercingMucus || previousHitCount <= 0)
            {
                return 1f;
            }

            float falloff = Mathf.Clamp(settings.piercingSubsequentHitDamageMultiplier, 0.05f, 1f);
            return Mathf.Pow(falloff, previousHitCount);
        }

        public int GetReflectionBounceCount()
        {
            return ownership.HasVascularReflection ? Mathf.Max(1, settings.reflectionBounceCount) : 0;
        }

        public float GetBoomerangReturnDistance()
        {
            return Mathf.Max(0.5f, settings.boomerangReturnDistance);
        }

        public float GetBoomerangReturnDistance(float minimumReturnDistance)
        {
            return Mathf.Max(minimumReturnDistance, GetBoomerangReturnDistance());
        }

        public float GetBoomerangRepeatHitDamageMultiplier()
        {
            return Mathf.Clamp(settings.boomerangRepeatHitDamageMultiplier, 0.05f, 1f);
        }

        public int GetBoomerangMaxHitsPerPass()
        {
            return Mathf.Max(1, settings.boomerangMaxHitsPerPass);
        }

        public float GetHomingTurnRate()
        {
            return Mathf.Max(0f, settings.homingTurnRate);
        }

        public float GetHomingSearchRadius()
        {
            return Mathf.Max(0.5f, settings.homingSearchRadius);
        }

        public float GetPulseAmplitude()
        {
            return Mathf.Max(1.2f, settings.pulseGrowEndScale);
        }

        public float GetPulseFrequency()
        {
            return Mathf.Clamp(settings.pulseShrinkEndScale, 0.08f, 0.9f);
        }

        public float GetSplitAngle()
        {
            return Mathf.Max(90f, settings.splitAngle);
        }

        public float GetSplitDamageMultiplier()
        {
            return Mathf.Max(0.05f, settings.splitDamageMultiplier);
        }

        public float GetSplitRangeMultiplier()
        {
            return Mathf.Max(0.05f, settings.splitRangeMultiplier);
        }

        public float GetExplosionRadius()
        {
            return Mathf.Max(0.2f, settings.explosionRadius);
        }

        public float GetExplosionDamageMultiplier()
        {
            return Mathf.Max(0.05f, settings.explosionDamageMultiplier);
        }

        public float GetRangedBasicDamageMultiplier()
        {
            return ownership.HasExplosiveBloodCell
                ? Mathf.Clamp(settings.explosiveBloodCellRangedDamageMultiplier, 0.05f, 1f)
                : 1f;
        }

        public float GetAttackCooldownMultiplier()
        {
            float speedMultiplier = 1f;

            if (ownership.HasOverheatedOrgan && overheatStacks > 0)
            {
                float bonus = 1f + Mathf.Max(0f, settings.overheatPerStackAttackSpeedBonus) * overheatStacks;
                speedMultiplier *= bonus;
            }

            if (ownership.HasBloodPressureBurst && host.HasStatsOwner && host.MaxHealth > 0f)
            {
                float healthRatio = Mathf.Clamp01(host.CurrentHealth / host.MaxHealth);
                int missingTenPercentSteps = Mathf.Clamp(Mathf.FloorToInt((1f - healthRatio) * 10f + 0.0001f), 0, 10);
                float additiveAttackSpeed =
                    Mathf.Max(0f, settings.bloodPressureBaseAttackSpeedAdd)
                    + Mathf.Max(0f, settings.bloodPressurePerTenPercentMissingAdd) * missingTenPercentSteps;
                float burstMultiplier = 1f + additiveAttackSpeed;
                speedMultiplier *= burstMultiplier;
            }

            if (ownership.HasMuscleSpasm)
            {
                speedMultiplier *= Mathf.Clamp(settings.muscleSpasmAttackSpeedMultiplier, 0.25f, 1f);
            }

            if (ownership.HasBloodflowAcceleration && queries.IsBossOrEliteWithinRadius(Mathf.Max(0.1f, settings.bloodflowAccelerationRadius)))
            {
                speedMultiplier *= 1f + Mathf.Max(0f, settings.bloodflowAccelerationAttackSpeedBonus);
            }

            if (timed.IsBerserkCellActive())
            {
                speedMultiplier *= 1f + Mathf.Max(0f, settings.berserkCellAttackSpeedBonus);
            }

            return 1f / Mathf.Max(0.05f, speedMultiplier);
        }

        public float GetOutgoingBasicDamageMultiplier()
        {
            float multiplier = 1f;

            if (ownership.HasOrganTentacle)
            {
                multiplier *= Mathf.Clamp(settings.organTentacleBaseAttackPenaltyMultiplier, 0.2f, 1f);
            }

            return Mathf.Max(0.05f, multiplier);
        }

        public float GetOutgoingBasicDamageFlatBonus()
        {
            float bonus = 0f;
            if (ownership.HasMutantEye)
            {
                bonus += Mathf.Max(0f, settings.mutantEyeFlatDamageBonus);
            }

            if (ownership.HasRampageBloodFlow)
            {
                bonus += Mathf.Max(0, timed.rampageAttackBonus);
            }

            if (ownership.HasHyperplasiaHeart && host.HasStatsOwner && host.MaxHealth > 0f)
            {
                float healthRatio = Mathf.Clamp01(host.CurrentHealth / host.MaxHealth);
                bonus += (1f - healthRatio) * Mathf.Max(0f, settings.hyperplasiaMissingHealthAttackBonusMax);
            }

            if (ownership.HasDecayOrgan)
            {
                bonus += Mathf.Max(0, timed.decayRuntime.AttackBonus);
            }

            if (ownership.HasRuptureMuscle && timed.ruptureMuscleStacks > 0)
            {
                int effectiveRuptureStacks = Mathf.FloorToInt(Mathf.Max(0f, timed.ruptureMuscleStacks));
                bonus += Mathf.Max(0f, settings.ruptureMuscleAttackBonusPerStack) * effectiveRuptureStacks;
            }

            if (ownership.HasSeveranceReflex && Time.time <= timed.severanceReflexBuffUntil)
            {
                bonus += Mathf.Max(0f, settings.severanceReflexFlatAttackBonus);
            }

            if (ownership.HasFocusedNerve)
            {
                bonus += timed.GetFocusedNerveAttackBonus();
            }

            return bonus;
        }

        public float GetMeleeRangeMultiplier()
        {
            return ownership.HasMuscleSpasm ? Mathf.Max(1f, settings.muscleSpasmMeleeRangeMultiplier) : 1f;
        }

        public float GetAccuracyPenaltyAngle()
        {
            return ownership.HasMutantEye ? Mathf.Max(0f, settings.mutantEyeAccuracyPenaltyAngle) : 0f;
        }

        public float RollUnstableCoreDamageMultiplier()
        {
            if (!ownership.HasUnstableCore)
            {
                unstableCoreCurrentMultiplier = 1f;
                unstableCoreInitialized = false;
                return 1f;
            }

            if (!unstableCoreInitialized || Time.time >= unstableCoreNextRerollTime)
            {
                float attackPower = host.HasStatsOwner ? Mathf.Max(0.01f, host.AttackPower) : 1f;
                float minDamage = attackPower * Mathf.Max(0f, settings.unstableCoreMinAttackRatio);
                float maxDamage = attackPower * Mathf.Max(
                    settings.unstableCoreMinAttackRatio,
                    settings.unstableCoreMaxAttackRatio);
                maxDamage = Mathf.Max(minDamage, maxDamage);
                float rolledDamage = UnityEngine.Random.Range(minDamage, maxDamage);
                unstableCoreCurrentMultiplier = rolledDamage / attackPower;
                unstableCoreNextRerollTime = Time.time + Mathf.Max(0.1f, settings.unstableCoreRerollInterval);
                unstableCoreInitialized = true;
            }

            return unstableCoreCurrentMultiplier;
        }

        public float RollUnstableCellProjectileSpeedMultiplier(out float damageMultiplier)
        {
            damageMultiplier = 1f;
            if (!ownership.HasUnstableCell)
            {
                return 1f;
            }

            float minSpeed = Mathf.Max(0.05f, settings.unstableCellMinProjectileSpeedMultiplier);
            float maxSpeed = Mathf.Max(minSpeed, settings.unstableCellMaxProjectileSpeedMultiplier);
            float speedMultiplier = UnityEngine.Random.Range(minSpeed, maxSpeed);
            if (speedMultiplier <= Mathf.Max(0.01f, settings.unstableCellSlowThreshold))
            {
                damageMultiplier = Mathf.Max(1f, settings.unstableCellSlowDamageMultiplier);
            }

            return speedMultiplier;
        }

        public void NotifyBasicAttackPerformed(float attackDamage, LayerMask mask, float range, Vector3 forwardDirection)
        {
            float now = Time.time;

            if (ownership.HasOverheatedOrgan)
            {
                if (now - overheatLastAttackTime <= Mathf.Max(0.1f, settings.overheatStackWindow))
                {
                    overheatStacks = Mathf.Clamp(overheatStacks + 1, 1, Mathf.Max(1, settings.overheatMaxStacks));
                }
                else
                {
                    overheatStacks = 1;
                }

                overheatLastAttackTime = now;
                overheatNextDecayTime = now + Mathf.Max(0.25f, settings.overheatDecayInterval);

                if (overheatStacks >= Mathf.Max(1, settings.overheatMaxStacks))
                {
                    TriggerOverheatExplosionSelfDamage(attackDamage);
                    overheatStacks = 0;
                    overheatLastAttackTime = float.NegativeInfinity;
                    overheatNextDecayTime = float.PositiveInfinity;
                }
            }

            if (ownership.HasVoidCell && UnityEngine.Random.value <= Mathf.Clamp01(settings.voidCellChance))
            {
                SpawnVoidCellProjectile(attackDamage, mask, range, forwardDirection);
            }

            if (ownership.HasRuptureMuscle)
            {
                timed.ruptureMuscleStacks = Mathf.Clamp(timed.ruptureMuscleStacks + 0.5f, 0f, Mathf.Max(1f, settings.ruptureMuscleMaxStacks));
                timed.ruptureMuscleLastAttackTime = now;
            }
        }

        public float ApplyPerTargetDamageModifiers(EnemyController enemy, float damage)
        {
            float adjustedDamage = Mathf.Max(0f, damage);
            if (enemy == null)
            {
                return adjustedDamage;
            }

            if (ownership.HasHeartSniper && queries.IsEnemyAboveHealthRatio(enemy, Mathf.Clamp01(settings.heartSniperHealthThreshold)))
            {
                adjustedDamage *= 1f + Mathf.Max(0f, settings.heartSniperDamageBonusRatio);
            }

            if (!ownership.HasBioResonance || enemy == null)
            {
                return adjustedDamage;
            }

            var enemyId=enemy.LogicalLife;
            if(enemyId==null||enemyId.IsRetired)return adjustedDamage;
            float now = enemyId.Clock(Time.time);
            int maxStacks = Mathf.Max(1, settings.bioResonanceMaxStacks);
            float window = Mathf.Max(0.1f, settings.bioResonanceStackWindow);

            if (!resonanceStatesByEnemyId.TryGetValue(enemyId, out ResonanceState state))
            {
                state = new ResonanceState
                {
                    Stacks = 0,
                    LastHitTime = float.NegativeInfinity
                };
            }

            int effectiveStacks = 0;
            if (now - state.LastHitTime <= window)
            {
                effectiveStacks = Mathf.Clamp(state.Stacks, 0, maxStacks);
            }

            float stackBonus = Mathf.Max(0f, settings.bioResonanceStackDamageBonus) * effectiveStacks;
            adjustedDamage *= 1f + stackBonus;

            state.Stacks = Mathf.Clamp(effectiveStacks + 1, 1, maxStacks);
            state.LastHitTime = now;
            resonanceStatesByEnemyId[enemyId] = state;

            return adjustedDamage;
        }

        public void TryApplyPostDamageExecutionInstinct(EnemyController enemy, float appliedDamage, uint? expectedGeneration = null)
        {
            if (!ownership.HasExecutionInstinct || enemy == null || enemy.IsDead || (expectedGeneration.HasValue && enemy.SpawnGeneration != expectedGeneration.Value))
            {
                return;
            }

            if (!queries.IsEnemyBelowHealthRatio(enemy, Mathf.Clamp01(settings.executionInstinctHealthThreshold)))
            {
                return;
            }

            if (UnityEngine.Random.value > Mathf.Clamp01(settings.executionInstinctChance))
            {
                return;
            }

            if (queries.IsBossLikeEnemy(enemy) || enemy.IsElite)
            {
                float bonusMultiplier = Mathf.Max(1f, settings.executionInstinctBossDamageMultiplier) - 1f;
                float bonusDamage = Mathf.Max(0f, appliedDamage) * bonusMultiplier;
                if (bonusDamage > 0f)
                {
                    enemy.TakeDamage(bonusDamage, host.GetItemHealthEffectSource(ExecutionInstinctId, "bonus"));
                }
                return;
            }

            float remainingHealth = queries.GetEnemyCurrentHealth(enemy);
            if (remainingHealth > 0f)
            {
                enemy.TakeDamage(remainingHealth, host.GetItemHealthEffectSource(ExecutionInstinctId, "execution"));
            }
        }

        public void ApplyCommonOnHitEffects(
            EnemyController enemy,
            float hitDamage,
            Vector3 hitPosition, uint? expectedGeneration = null)
        {
            if (enemy == null)
            {
                return;
            }

            // Spawn acid puddle immediately at hit position even when the enemy dies on impact.
            if (ownership.HasAcidicRupture)
            {
                float tickDamage = Mathf.Max(0.1f, settings.acidTickDamage);
                AcidPuddle.Spawn(hitPosition, tickDamage, settings.acidDuration, GetExplosionRadius(), settings.acidTickInterval);
            }

            uint generation = expectedGeneration ?? enemy.SpawnGeneration;
            if (enemy.IsDead || enemy.SpawnGeneration != generation)
            {
                return;
            }

            EnemyStatusEffectController status = queries.EnsureStatusController(enemy);

            if (ownership.HasToxicMucosa)
            {
                float tickDamage = Mathf.Max(0.05f, hitDamage * settings.poisonTickDamageRatio);
                status?.ApplyPoison(settings.poisonDuration, settings.poisonTickInterval, tickDamage, host.GetItemHealthEffectSource(ToxicMucosaId, "poison"));
            }

            if (enemy == null || enemy.SpawnGeneration != generation) return;
            if (ownership.HasFreezingNerve)
            {
                status?.ApplyMoveSpeedSlow(settings.freezeSlowRatio, settings.freezeDuration);
            }

            if (enemy == null || enemy.SpawnGeneration != generation) return;
            if (ownership.HasHemorrhageOrgan)
            {
                float tickDamage = Mathf.Max(0.05f, hitDamage * settings.bleedTickDamageRatio);
                status?.ApplyBleed(settings.bleedDuration, settings.bleedTickInterval, tickDamage, host.GetItemHealthEffectSource(HemorrhageOrganId, "bleed"));
            }
        }

        public void SpawnSplitProjectiles(Vector3 origin, Vector3 forwardDirection, float damage, LayerMask mask, float range)
        {
            Vector3 forward = forwardDirection.sqrMagnitude > 0.0001f
                ? forwardDirection.normalized
                : Vector3.forward;
            float angle = GetSplitAngle();
            float sideDamage = damage * GetSplitDamageMultiplier();
            float sideRange = range * GetSplitRangeMultiplier();

            SpawnChildProjectile(origin, Quaternion.Euler(0f, angle, 0f) * forward, sideDamage, mask, sideRange, Projectile.SpawnKind.SplitChild, host.GetItemHealthEffectSource(SplitTissueId, "projectile"));
            SpawnChildProjectile(origin, Quaternion.Euler(0f, -angle, 0f) * forward, sideDamage, mask, sideRange, Projectile.SpawnKind.SplitChild, host.GetItemHealthEffectSource(SplitTissueId, "projectile"));
        }

        public void SpawnChildProjectile(Vector3 origin, Vector3 direction, float damage, LayerMask mask, float range, Projectile.SpawnKind spawnKind, HealthEffectSource source)
        {
            PlayerProjectilePool pool = queries.ResolvePool();
            if (pool == null)
            {
                return;
            }

            GameObject projectileObject = pool.GetPooledObject();
            if (projectileObject == null)
            {
                return;
            }

            projectileObject.transform.position = origin;
            projectileObject.SetActive(true);

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            if (projectile == null)
            {
                return;
            }

            projectile.LaunchWithItemContext(direction, damage, mask, range, host.ProjectileEffects, spawnKind, source);
            ProjectileDirectionalSprite directionalSprite = projectileObject.GetComponent<ProjectileDirectionalSprite>();
            if (directionalSprite == null)
            {
                directionalSprite = projectileObject.AddComponent<ProjectileDirectionalSprite>();
            }

            directionalSprite.SetDirection(direction);
        }

        public void UpdateTentacleAutoAttack()
        {
            if (!ownership.hasOrganTentacleItem || !host.HasStatsOwner || host.IsDead)
            {
                return;
            }

            if (Time.time < tentacleNextAutoAttackTime)
            {
                return;
            }

            tentacleNextAutoAttackTime = Time.time + Mathf.Max(0.15f, settings.organTentacleAutoAttackInterval);

            using var enemies = EnemyHitLife.CaptureOwned(EnemyController.ActiveEnemyControllers);
            var enemyData=enemies.Buffer;
            if (enemies.Length == 0)
            {
                return;
            }

            Vector3 center = host.Transform.position;
            float radiusSqr = Mathf.Max(0.5f, settings.organTentacleAutoAttackRadius);
            radiusSqr *= radiusSqr;
            float damage = Mathf.Max(0.1f, host.AttackPower * Mathf.Max(0.05f, settings.organTentacleAutoAttackDamageMultiplier));
            int hitLimit = Mathf.Max(1, settings.organTentacleMaxTargets);
            int hitCount = 0;

            HealthEffectSource source = host.GetItemHealthEffectSource(OrganTentacleId, "strike");
            for (int i = 0; i < enemies.Length && hitCount < hitLimit; i++)
            {
                EnemyController enemy = enemyData[i].Enemy;
                if (!enemyData[i].IsActiveCandidate || enemy.IsDead)
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - center;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude > radiusSqr)
                {
                    continue;
                }

                float adjustedDamage = ApplyPerTargetDamageModifiers(enemy, damage);
                var hitLife = new EnemyHitLife(enemy);
                Vector3 hitPosition = enemy.transform.position;
                enemy.TakeDamage(adjustedDamage, source);
                TryApplyPostDamageExecutionInstinct(enemy, adjustedDamage, hitLife.Generation);
                ApplyCommonOnHitEffects(enemy, adjustedDamage, hitPosition, hitLife.Generation);
                Vector3 tentacleStart = host.GetPlayerVisualCenter();
                Vector3 tentacleEnd = enemy.transform.position + Vector3.up * 0.4f;
                Vector3 tentacleMidpoint = (tentacleStart + tentacleEnd) * 0.5f;
                Vector3 extendedHalfLength = (tentacleEnd - tentacleStart) * 0.625f;
                host.SpawnLineVisual(
                    "OrganTentacleStrike",
                    tentacleMidpoint - extendedHalfLength,
                    tentacleMidpoint + extendedHalfLength,
                    Color.white,
                    Color.white,
                    0.52f,
                    0.24f,
                    0.2f,
                    OrganTentacleEffectPath);
                hitCount++;
            }
        }

        public void UpdateDecayStates()
        {
            float now = Time.time;
            if (ownership.hasOverheatedOrganItem)
            {
                if (overheatStacks > 0 && now >= overheatNextDecayTime)
                {
                    overheatStacks = 0;
                    overheatLastAttackTime = float.NegativeInfinity;
                    overheatNextDecayTime = float.PositiveInfinity;
                }
            }
            else
            {
                overheatStacks = 0;
                overheatLastAttackTime = float.NegativeInfinity;
                overheatNextDecayTime = float.PositiveInfinity;
            }

            if (!ownership.hasBioResonanceItem)
            {
                resonanceStatesByEnemyId.Clear();
                return;
            }

            if (resonanceStatesByEnemyId.Count == 0)
            {
                return;
            }

            float cleanupThreshold = Mathf.Max(0.2f, settings.bioResonanceStackWindow * 4f);
            tempResonanceRemovalIds.Clear();
            foreach (KeyValuePair<EnemyLogicalLife, ResonanceState> pair in resonanceStatesByEnemyId)
            {
                if (pair.Key.IsRetired || pair.Key.Clock(now) - pair.Value.LastHitTime > cleanupThreshold)
                {
                    tempResonanceRemovalIds.Add(pair.Key);
                }
            }

            for (int i = 0; i < tempResonanceRemovalIds.Count; i++)
            {
                resonanceStatesByEnemyId.Remove(tempResonanceRemovalIds[i]);
            }
        }

        public void TriggerOverheatExplosionSelfDamage(float attackDamage)
        {
            host.SpawnOverheatExplosionVisual();

            Vector3 center = host.Transform.position;
            HealthEffectSource explosionSource = host.GetItemHealthEffectSource(OverheatedOrganId, "explosion");
            HealthEffectSource selfSource = host.GetItemHealthEffectSource(OverheatedOrganId, "self");
            float radius = Mathf.Max(0.1f, settings.overheatExplosionRadius);
            float radiusSqr = radius * radius;
            float explosionDamage = Mathf.Max(
                0.1f,
                attackDamage * Mathf.Max(0.05f, settings.overheatExplosionDamageMultiplier));
            using var enemies = EnemyHitLife.CaptureOwned(EnemyController.ActiveEnemyControllers);
            var enemyData=enemies.Buffer;
            if (enemies.Length > 0)
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    EnemyController enemy = enemyData[i].Enemy;
                    if (!enemyData[i].IsActiveCandidate || enemy.IsDead)
                    {
                        continue;
                    }

                    Vector3 toEnemy = enemy.transform.position - center;
                    toEnemy.y = 0f;
                    if (toEnemy.sqrMagnitude > radiusSqr)
                    {
                        continue;
                    }

                    float appliedDamage = ApplyPerTargetDamageModifiers(enemy, explosionDamage);
                    var hitLife = new EnemyHitLife(enemy);
                    Vector3 hitPosition = enemy.transform.position;
                    enemy.TakeDamage(appliedDamage, explosionSource);
                    TryApplyPostDamageExecutionInstinct(enemy, appliedDamage, hitLife.Generation);
                    ApplyCommonOnHitEffects(enemy, appliedDamage, hitPosition, hitLife.Generation);
                }
            }

            float selfDamage = Mathf.Max(0.1f, settings.overheatExplosionSelfDamage);
            if (host.HasStatsOwner)
            {
                host.ApplyBypassDamage(selfDamage,selfSource);
            }
        }

        public void SpawnVoidCellProjectile(float attackDamage, LayerMask mask, float range, Vector3 fallbackDirection)
        {
            EnemyController target = FindRandomEnemyWithinRadius(12f);
            if (target == null)
            {
                return;
            }

            Vector3 targetPos = target.transform.position;
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * Mathf.Max(0.3f, settings.voidCellSpawnRadius);
            Vector3 spawnPos = targetPos + new Vector3(randomCircle.x, 0f, randomCircle.y);
            spawnPos.y = host.Transform.position.y + 0.2f;

            Vector3 shootDir = targetPos - spawnPos;
            shootDir.y = 0f;
            if (shootDir.sqrMagnitude <= 0.0001f)
            {
                shootDir = fallbackDirection.sqrMagnitude > 0.0001f ? fallbackDirection.normalized : Vector3.forward;
            }

            SpawnChildProjectile(
                spawnPos,
                shootDir.normalized,
                Mathf.Max(0.05f, attackDamage * Mathf.Max(0.05f, settings.voidCellDamageMultiplier)),
                mask,
                Mathf.Max(0.25f, range * 0.6f),
                Projectile.SpawnKind.SplitChild, host.GetItemHealthEffectSource(VoidCellId, "projectile"));
        }

        public EnemyController FindRandomEnemyWithinRadius(float radius)
        {
            var enemies = EnemyController.ActiveEnemyControllers;
            if (enemies == null || enemies.Count == 0)
            {
                return null;
            }

            randomEnemyCandidates.Clear();
            Vector3 center = host.GlobalPlayerPosition;
            float radiusSqr = Mathf.Max(0.5f, radius);
            radiusSqr *= radiusSqr;

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 toEnemy = enemy.transform.position - center;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude > radiusSqr)
                {
                    continue;
                }

                randomEnemyCandidates.Add(enemy);
            }

            if (randomEnemyCandidates.Count == 0)
            {
                return null;
            }

            int index = UnityEngine.Random.Range(0, randomEnemyCandidates.Count);
            EnemyController selected = randomEnemyCandidates[index];
            randomEnemyCandidates.Clear();
            return selected;
        }
        private const string OrganTentacleEffectPath = "ItemEffects/organ_tentacle_effect";
    }
}
