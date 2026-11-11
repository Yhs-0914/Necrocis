using UnityEngine;

namespace Necrocis
{
    // A hit owns the target life observed before synchronous damage callbacks.
    internal readonly struct EnemyHitLife
    {
        private readonly EnemyController enemy;
        private readonly uint generation;
        public EnemyHitLife(EnemyController enemy) { this.enemy=enemy; generation=enemy!=null?enemy.SpawnGeneration:0; }
        public EnemyController Enemy => enemy;
        public uint Generation => generation;
        public static EnemyHitLife[] Capture(System.Collections.Generic.IReadOnlyList<EnemyController> enemies)
        {
            if(enemies==null)return System.Array.Empty<EnemyHitLife>();
            var result=new EnemyHitLife[enemies.Count];
            for(int i=0;i<result.Length;i++)result[i]=new EnemyHitLife(enemies[i]);
            return result;
        }
        public static ScratchLease<EnemyHitLife> CaptureOwned(System.Collections.Generic.IReadOnlyList<EnemyController> enemies)
        {
            int count=enemies?.Count??0;var result=ScratchLease<EnemyHitLife>.Rent(count);
            var data=result.Buffer;
            for(int i=0;i<count;i++)data[i]=new EnemyHitLife(enemies[i]);
            return result;
        }
        public bool IsCurrent => enemy!=null && enemy.SpawnGeneration==generation;
        // Active-list attacks keep the original registration policy even when a
        // callback disables and immediately re-enables a pending target.
        public bool IsActiveCandidate => IsCurrent && enemy.isActiveAndEnabled
            && EnemyController.IsRegisteredActive(enemy);
        // A normal death may return this life to the pool before the hit callback.
        // Configure always advances the generation again, so a new life is excluded.
        public bool HasNotRespawned => IsCurrent || (enemy!=null && !enemy.gameObject.activeSelf && enemy.SpawnGeneration==unchecked(generation+1));
    }

    internal readonly struct EnemyHitSnapshot
    {
        public readonly Collider Collider;
        public readonly EnemyController Enemy;
        private readonly uint generation;
        private EnemyHitSnapshot(Collider collider)
        {
            Collider=collider;Enemy=collider!=null?collider.GetComponentInParent<EnemyController>():null;
            generation=Enemy!=null?Enemy.SpawnGeneration:0;
        }
        public bool IsCurrent => Collider!=null && Collider.GetComponentInParent<EnemyController>()==Enemy
            && (Enemy==null || (Enemy.SpawnGeneration==generation && EnemyReceivingShape.Allows(Enemy,Collider)));
        private static EnemyHitSnapshot[] CaptureCompleted(Collider[] colliders,int count)
        {
            if(count==0)return System.Array.Empty<EnemyHitSnapshot>();
            var result=new EnemyHitSnapshot[count];
            for(int i=0;i<count;i++)result[i]=new EnemyHitSnapshot(colliders[i]);
            return result;
        }
        public static EnemyHitSnapshot[] Box(Vector3 center,Vector3 halfExtents,Quaternion rotation,int mask,QueryTriggerInteraction trigger,ref Collider[] buffer)
        {
            int count=CompletePhysicsQueries.FillBox(center,halfExtents,rotation,mask,trigger,ref buffer);
            return CaptureCompleted(buffer,count);
        }
        public static EnemyHitSnapshot[] Capsule(Vector3 first,Vector3 second,float radius,int mask,QueryTriggerInteraction trigger,ref Collider[] buffer)
        {
            int count=CompletePhysicsQueries.FillCapsule(first,second,radius,mask,trigger,ref buffer);
            return CaptureCompleted(buffer,count);
        }
        public static EnemyHitSnapshot[] Sphere(Vector3 center,float radius,int mask,QueryTriggerInteraction trigger,ref Collider[] buffer)
        {
            int count=CompletePhysicsQueries.FillSphere(center,radius,mask,trigger,ref buffer);
            return CaptureCompleted(buffer,count);
        }
        public static EnemyHitSnapshot[] Capture(Collider[] colliders)
        {
            var result=new EnemyHitSnapshot[colliders.Length];
            for(int i=0;i<colliders.Length;i++)result[i]=new EnemyHitSnapshot(colliders[i]);
            return result;
        }
    }
}
