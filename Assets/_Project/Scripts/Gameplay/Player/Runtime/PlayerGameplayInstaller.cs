using System;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace Necrocis
{
    // Feature-local installation works without any HUD or camera implementation.
    public static class PlayerGameplayInstaller
    {
        public static event Action<GameObject> CompositionRequested;
        private static readonly ConditionalWeakTable<GameObject,Installation> installations=new ConditionalWeakTable<GameObject,Installation>();
        public static void Ensure(GameObject owner)
        {
            var state=installations.GetValue(owner,key=>new Installation(key));
            if(state.installing || state.installed)return;
            if(!state.requesting){state.requesting=true;try{CompositionRequested?.Invoke(owner);}finally{state.requesting=false;}}
            state.Install();
        }
        private sealed class Installation
        {
            private readonly GameObject gameObject;
            public bool installing,installed,requesting;
            public Installation(GameObject owner){gameObject=owner;}
            private T GetComponent<T>() where T:Component=>gameObject.GetComponent<T>();
            private T GetComponentInChildren<T>() where T:Component=>gameObject.GetComponentInChildren<T>();
            private T Ensure<T>() where T:Component=>GetComponent<T>()??gameObject.AddComponent<T>();
            private Transform transform=>gameObject.transform;
            public void Install()
            {
                if(installing||installed)return;installing=true;
                try{
                EnsureDefaultPhysics();
                var stats = Ensure<PlayerStats>();
                stats.EnsureInitialized();
                var damage = Ensure<PlayerDamageCoordinator>();
                var health = Ensure<Health>();
                var items = Ensure<PlayerItemManager>();
                var effects = Ensure<PlayerItemCombatEffects>();
                var terrain = Ensure<ProceduralTerrainMotor>();
                var player = Ensure<PlayerController>();
                Ensure<PlayerClassSkillController>();
                var attack = Ensure<PlayerAttack>();
                SpriteRenderer renderer = player.AuthoredSpriteRenderer;
                if (renderer == null) renderer = GetComponentInChildren<SpriteRenderer>();
                if (renderer == null)
                {
                    var sprite = new GameObject("Sprite");
                    sprite.transform.SetParent(transform, false);
                    renderer = sprite.AddComponent<SpriteRenderer>();
                }
                renderer.color = Color.white;
                var billboard = renderer.GetComponent<Billboard>() ?? renderer.gameObject.AddComponent<Billboard>();
                billboard.SetUpdateMode(Billboard.UpdateMode.Once);
                var sort = renderer.GetComponent<SpriteYSort>() ?? renderer.gameObject.AddComponent<SpriteYSort>();
                sort.Configure(SpriteYSort.WorldDynamicBaseSortingOrder, true, SpriteYSort.WorldDynamicMinSortingOrder);
                sort.SetUpdateMode(SpriteYSort.UpdateMode.Continuous);

                // Consumers subscribe only when bound; bind the sole fatal-outcome owner first.
                damage.Bind(player, stats, health, effects);
                health.BindRuntime(player, stats, damage);
                player.BindRuntime(stats, terrain, renderer);
                items.BindRuntime(stats, effects);
                effects.BindRuntime(player, stats, items);
                attack.BindRuntime(player, effects);
                    installed=true;
                }finally{installing=false;}
            }
        private void EnsureDefaultPhysics()
        {
            if (GetComponent<Collider>() == null)
            {
                var capsule = gameObject.AddComponent<CapsuleCollider>();
                capsule.height = 2f; capsule.radius = .5f; capsule.center = new Vector3(0f, 1f, 0f);
            }
            if (GetComponent<Rigidbody>() != null || GetComponent<CharacterController>() != null) return;
            var body = gameObject.AddComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            body.useGravity = false; body.linearDamping = body.angularDamping = 10f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        }
    }
}
