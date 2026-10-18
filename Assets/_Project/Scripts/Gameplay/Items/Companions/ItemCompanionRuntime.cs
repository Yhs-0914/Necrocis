using System;
using System.Collections.Generic;
using UnityEngine;
using static Necrocis.ItemCompanionResources;
using static Necrocis.PlayerItemIds;

namespace Necrocis
{
    internal sealed class ItemCompanionRuntime
    {
        private readonly IItemCompanionOwner owner;
        private readonly IItemCombatHost host;
        private readonly IItemOwnershipView ownership;
        private readonly ItemCombatQueries queries;
        private IItemCombatSettings settings=>host.Settings;
        private readonly List<PlayerBioSummon> activeBioSummons = new List<PlayerBioSummon>();
        internal int ActiveCount => activeBioSummons.Count;
        private PlayerBloodDrone bloodDrone;
        private PlayerGuardianOrgan guardianOrgan;
        internal ItemCompanionRuntime(IItemCompanionOwner owner,IItemCombatHost host,IItemOwnershipView ownership,ItemCombatQueries queries)
        {this.owner=owner;this.host=host;this.ownership=ownership;this.queries=queries;}
        private static readonly Func<GameObject> CreateBloodDroneFunc = CreateBloodDroneObject;
        private static readonly Func<GameObject> CreateGuardianOrganFunc = CreateGuardianOrganObject;
        internal static readonly Func<GameObject> CreateBloodDroneProjectileFunc = CreateBloodDroneProjectileObject;
        private static readonly Func<GameObject> CreateBioSummonFunc = CreateBioSummonObject;
        internal void UpdateBloodDrone()
        {
            if (!owner.HasBloodDroneItem)
            {
                if (bloodDrone != null)
                {
                    ReleaseBloodDrone();
                }

                return;
            }

            if (bloodDrone != null)
            {
                return;
            }

            GameObject droneObject = RuntimePool.Acquire(BloodDronePoolName, CreateBloodDroneFunc);
            if (droneObject == null)
            {
                return;
            }

            droneObject.name = "BloodDrone";
            bloodDrone = droneObject.GetComponent<PlayerBloodDrone>();
            bloodDrone.Initialize(owner);
        }

        internal void ReleaseBloodDrone()
        {
            if (bloodDrone == null)
            {
                return;
            }

            bloodDrone.ClearOwner();
            RuntimePool.Release(bloodDrone.gameObject);
            bloodDrone = null;
        }

        internal void UpdateGuardianOrgan()
        {
            if (!owner.HasGuardianOrganItem)
            {
                if (guardianOrgan != null)
                {
                    ReleaseGuardianOrgan();
                }

                return;
            }

            if (guardianOrgan != null)
            {
                return;
            }

            GameObject guardianObject = RuntimePool.Acquire(GuardianOrganPoolName, CreateGuardianOrganFunc);
            if (guardianObject == null)
            {
                return;
            }

            guardianObject.name = "GuardianOrgan";
            guardianOrgan = guardianObject.GetComponent<PlayerGuardianOrgan>();
            guardianOrgan.Initialize(owner);
        }

        internal void ReleaseGuardianOrgan()
        {
            if (guardianOrgan == null)
            {
                return;
            }

            guardianOrgan.ClearOwner();
            RuntimePool.Release(guardianOrgan.gameObject);
            guardianOrgan = null;
        }

        internal void SpawnBioSummon(
            PlayerBioSummon.SummonKind kind,
            Vector3 position,
            Sprite sprite,
            Sprite[] attackSprites,
            Vector3 visualScale,
            Color color,
            float damage,
            float lifetime,
            float searchRadius,
            float attackRadius,
            float attackInterval,
            bool destroyOnAttack)
        {
            GameObject summonObject = RuntimePool.Acquire(BioSummonPoolName, CreateBioSummonFunc);
            if (summonObject == null || !summonObject.TryGetComponent(out PlayerBioSummon summon))
            {
                RuntimePool.Release(summonObject);
                return;
            }

            summonObject.name = kind == PlayerBioSummon.SummonKind.Spore ? "SporeSummon" : "InfectedHostAlly";
            summonObject.transform.position = position;
            summon.Initialize(owner, kind, sprite, attackSprites, visualScale, color, damage, lifetime, searchRadius, attackRadius, attackInterval, destroyOnAttack);
            activeBioSummons.Add(summon);
        }

        internal void CleanupBioSummonList()
        {
            for (int i = activeBioSummons.Count - 1; i >= 0; i--)
            {
                if (activeBioSummons[i] == null || !activeBioSummons[i].gameObject.activeInHierarchy)
                {
                    activeBioSummons.RemoveAt(i);
                }
            }
        }

        internal int CountBioSummons(PlayerBioSummon.SummonKind kind)
        {
            CleanupBioSummonList();
            int count = 0;
            for (int i = 0; i < activeBioSummons.Count; i++)
            {
                if (activeBioSummons[i] != null && activeBioSummons[i].Kind == kind)
                {
                    count++;
                }
            }

            return count;
        }

        internal void ClearBioCompanions()
        {
            for (int i = activeBioSummons.Count - 1; i >= 0; i--)
            {
                if (activeBioSummons[i] != null)
                {
                    activeBioSummons[i].ReleaseToPool();
                }
            }

            activeBioSummons.Clear();
            if (bloodDrone != null)
            {
                ReleaseBloodDrone();
            }

            if (guardianOrgan != null)
            {
                ReleaseGuardianOrgan();
            }
        }

        internal void ReleaseBioSummon(PlayerBioSummon summon)
        {
            if (summon == null)
            {
                return;
            }

            activeBioSummons.Remove(summon);
            RuntimePool.Release(summon.gameObject);
        }

        private static GameObject CreateBloodDroneObject()
        {
            GameObject obj = new GameObject("BloodDrone");
            obj.AddComponent<PlayerBloodDrone>();
            return obj;
        }

        private static GameObject CreateGuardianOrganObject()
        {
            GameObject obj = new GameObject("GuardianOrgan");
            obj.AddComponent<PlayerGuardianOrgan>();
            return obj;
        }

        private static GameObject CreateBloodDroneProjectileObject()
        {
            GameObject obj = new GameObject("BloodDroneProjectile");
            obj.AddComponent<PlayerBioProjectile>();
            return obj;
        }

        private static GameObject CreateBioSummonObject()
        {
            GameObject obj = new GameObject("PlayerBioSummon");
            obj.AddComponent<SpriteRenderer>();
            obj.AddComponent<PlayerBioSummon>();
            return obj;
        }
        internal float nextSporeSpawnTime;
        internal float tentacleNextBindTime;

        public void UpdateBioCompanionItems()
        {
            CleanupBioSummonList();
            UpdateSporeColony();
            UpdateBloodDrone();
            UpdateGuardianOrgan();
            UpdateTentacleColony();
        }

        public void UpdateSporeColony()
        {
            if (!ownership.hasSporeColonyItem)
            {
                nextSporeSpawnTime = Time.time + Mathf.Max(0.1f, settings.sporeSpawnInterval);
                return;
            }

            if (Time.time < nextSporeSpawnTime)
            {
                return;
            }

            nextSporeSpawnTime = Time.time + Mathf.Max(0.1f, settings.sporeSpawnInterval);
            SpawnSporeSummon();
        }

        public void UpdateTentacleColony()
        {
            if (!ownership.hasTentacleColonyItem)
            {
                tentacleNextBindTime = Time.time + Mathf.Max(0.1f, settings.tentacleBindInterval);
                return;
            }

            if (Time.time < tentacleNextBindTime)
            {
                return;
            }

            tentacleNextBindTime = Time.time + Mathf.Max(0.1f, settings.tentacleBindInterval);
            ApplyTentacleBind();
        }

        public void TrySpawnInfectedHost(EnemyController defeatedEnemy)
        {
            if (!ownership.HasInfectedHost || defeatedEnemy == null || !queries.CanInfectEnemy(defeatedEnemy))
            {
                return;
            }

            if (CountBioSummons(PlayerBioSummon.SummonKind.InfectedHost) >= Mathf.Max(1, settings.infectedHostMaxAllies))
            {
                return;
            }

            if (UnityEngine.Random.value > Mathf.Clamp01(settings.infectedHostChance))
            {
                return;
            }

            Sprite enemySprite = queries.FindEnemySprite(defeatedEnemy);
            Sprite[] attackSprites = queries.FindEnemyAttackSprites(defeatedEnemy);
            Vector3 visualScale = queries.FindEnemyVisualScale(defeatedEnemy, Vector3.one * 0.9f);
            float damage = queries.GetCompanionBaseDamage() * Mathf.Max(0.05f, settings.infectedHostDamageMultiplier);
            SpawnBioSummon(
                PlayerBioSummon.SummonKind.InfectedHost,
                defeatedEnemy.transform.position,
                enemySprite,
                attackSprites,
                visualScale,
                new Color(1f, 0.28f, 0.22f, 0.9f),
                damage,
                Mathf.Max(0.1f, settings.infectedHostLifetime),
                Mathf.Max(0.1f, settings.infectedHostSearchRadius),
                Mathf.Max(0.1f, settings.infectedHostAttackRadius),
                Mathf.Max(0.1f, settings.infectedHostAttackInterval),
                false);
        }

        public void SpawnSporeSummon()
        {
            Vector2 offset2D = UnityEngine.Random.insideUnitCircle;
            if (offset2D.sqrMagnitude <= 0.0001f)
            {
                offset2D = Vector2.right;
            }

            Vector3 desiredPosition = host.Transform.position + new Vector3(offset2D.x, 0f, offset2D.y).normalized * 1.2f;
            Vector3 spawnPosition = queries.ResolveGroundSpawnPosition(desiredPosition, 0.15f);
            float damage = queries.GetCompanionBaseDamage() * Mathf.Max(0.05f, settings.sporeDamageMultiplier);
            Sprite sporeSprite = TextureSpriteCache.LoadResourceSprite(SporeColonyEffectPath);
            if (sporeSprite == null)
            {
                sporeSprite = TextureSpriteCache.GetCircleSprite();
            }

            SpawnBioSummon(
                PlayerBioSummon.SummonKind.Spore,
                spawnPosition,
                sporeSprite,
                null,
                Vector3.one * TextureSpriteCache.GetUniformScaleForWorldSize(sporeSprite, 0.65f),
                Color.white,
                damage,
                Mathf.Max(0.1f, settings.sporeLifetime),
                Mathf.Max(0.1f, settings.sporeSearchRadius),
                Mathf.Max(0.1f, settings.sporeBurstRadius),
                0.1f,
                true);
        }

        public void ApplyTentacleBind()
        {
            host.SharedTargets.Clear();
            int targetCount = Mathf.Max(1, settings.tentacleMaxTargets);
            Vector3 center = host.Transform.position;

            for (int i = 0; i < targetCount; i++)
            {
                EnemyController enemy = queries.FindClosestEnemy(center, Mathf.Max(0.1f, settings.tentacleBindRadius), host.SharedTargets);
                if (enemy == null)
                {
                    break;
                }

                host.SharedTargets.Add(enemy);
            }

            float damage = queries.GetCompanionBaseDamage() * Mathf.Max(0f, settings.tentacleDamageMultiplier);
            HealthEffectSource source = host.GetItemHealthEffectSource(TentacleColonyId, "bind");
            for (int i = 0; i < host.SharedTargets.Count; i++)
            {
                EnemyController enemy = host.SharedTargets[i];
                if (!queries.IsEnemyTargetable(enemy))
                {
                    continue;
                }

                EnemyStatusEffectController status = queries.EnsureStatusController(enemy);
                status?.ApplyMoveSpeedSlow(Mathf.Clamp01(settings.tentacleSlowRatio), Mathf.Max(0.1f, settings.tentacleBindDuration));
                if (damage > 0f)
                {
                    enemy.TakeDamage(damage, source);
                }

                host.SpawnTentacleBindVisual(enemy.transform.position, Mathf.Max(0.1f, settings.tentacleBindDuration));
            }
        }
    }
}
