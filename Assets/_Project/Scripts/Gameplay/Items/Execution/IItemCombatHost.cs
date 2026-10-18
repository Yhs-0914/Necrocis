using System.Collections.Generic;
using UnityEngine;
namespace Necrocis
{
    // Physical owner/inventory/presentation access. Gameplay calculations live in the executors.
    internal interface IItemCombatHost
    {
        IItemCombatSettings Settings { get; }
        Transform Transform { get; }
        bool HasStatsOwner { get; }
        bool HasManagedStatsOwner { get; }
        CharacterStats Recipient { get; }
        float AttackPower { get; }
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsDead { get; }
        bool HasPlayerController { get; }
        Transform PlayerTransform { get; }
        bool PlayerIsMoving { get; }
        Vector3 GlobalPlayerPosition { get; }
        object ModifierSource { get; }
        IProjectileItemEffects ProjectileEffects { get; }
        List<EnemyController> SharedTargets { get; }
        ItemKillEffectState KillState { get; }
        ItemCompanionRuntime Companions { get; }
        int ExistingCompanionCount { get; }
        SpriteRenderer UnstableCoreOverlay { get; }
        bool Enabled { get; set; }
        bool InventoryContains(string id);
        void CopyItemIdsTo(HashSet<string> target);
        void SleepEmptyInventory();
        void Heal(float amount,HealthEffectSource source);
        void TakeDamage(float amount,HealthEffectSource source);
        void ApplyBypassDamage(float amount,HealthEffectSource source);
        HealthEffectSource GetItemHealthEffectSource(string id,string effect);
        Vector3 GetPlayerVisualCenter();
        void SpawnLineVisual(string objectName,Vector3 start,Vector3 end,Color startColor,Color endColor,float startWidth,float endWidth,float duration,string textureResourcePath=null,int sortingOrder=5230);
        void SpawnOverheatExplosionVisual();
        void SpawnTentacleBindVisual(Vector3 position,float duration);
        void ClearPlateletMembraneOutline();
        void UpdatePlateletMembraneOutline();
        void ClearKillChainStatBuff(object source,ref float value);
    }
}
