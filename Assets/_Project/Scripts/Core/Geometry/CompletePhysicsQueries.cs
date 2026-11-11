using System;
using UnityEngine;

namespace Necrocis
{
    // Initial buffer capacity is not a gameplay target limit. Retry before invoking any hit logic.
    public static class CompletePhysicsQueries
    {
        private static void Ensure<T>(ref T[] buffer){if(buffer==null || buffer.Length==0)buffer=new T[1];}
        private static void Grow<T>(ref T[] buffer) => Array.Resize(ref buffer,checked(buffer.Length*2));
        private static T[] Snapshot<T>(T[] buffer,int count)
        {if(count==0)return Array.Empty<T>();var result=new T[count];Array.Copy(buffer,result,count);return result;}
        public static Collider[] Box(Vector3 center,Vector3 halfExtents,Quaternion rotation,int mask,QueryTriggerInteraction trigger,ref Collider[] buffer)
        {int count=FillBox(center,halfExtents,rotation,mask,trigger,ref buffer);return Snapshot(buffer,count);}

        public static int FillBox(Vector3 center,Vector3 halfExtents,Quaternion rotation,int mask,QueryTriggerInteraction trigger,ref Collider[] buffer)
        {
            Ensure(ref buffer);
            while(true){int count=Physics.OverlapBoxNonAlloc(center,halfExtents,buffer,rotation,mask,trigger);
                if(count<buffer.Length)return count;Grow(ref buffer);}
        }
        public static Collider[] Capsule(Vector3 first,Vector3 second,float radius,int mask,QueryTriggerInteraction trigger,ref Collider[] buffer)
        {int count=FillCapsule(first,second,radius,mask,trigger,ref buffer);return Snapshot(buffer,count);}

        public static int FillCapsule(Vector3 first,Vector3 second,float radius,int mask,QueryTriggerInteraction trigger,ref Collider[] buffer)
        {
            Ensure(ref buffer);
            while(true){int count=Physics.OverlapCapsuleNonAlloc(first,second,radius,buffer,mask,trigger);
                if(count<buffer.Length)return count;Grow(ref buffer);}
        }
        public static Collider[] Sphere(Vector3 center,float radius,int mask,QueryTriggerInteraction trigger,ref Collider[] buffer)
        {int count=FillSphere(center,radius,mask,trigger,ref buffer);return Snapshot(buffer,count);}

        public static int FillSphere(Vector3 center,float radius,int mask,QueryTriggerInteraction trigger,ref Collider[] buffer)
        {
            Ensure(ref buffer);
            while(true){int count=Physics.OverlapSphereNonAlloc(center,radius,buffer,mask,trigger);
                if(count<buffer.Length)return count;Grow(ref buffer);}
        }
        public static RaycastHit[] SphereCast(Vector3 origin,float radius,Vector3 direction,float distance,int mask,QueryTriggerInteraction trigger,ref RaycastHit[] buffer)
        {int count=FillSphereCast(origin,radius,direction,distance,mask,trigger,ref buffer);return Snapshot(buffer,count);}

        public static int FillSphereCast(Vector3 origin,float radius,Vector3 direction,float distance,int mask,QueryTriggerInteraction trigger,ref RaycastHit[] buffer)
        {
            Ensure(ref buffer);
            while(true){int count=Physics.SphereCastNonAlloc(origin,radius,direction,buffer,distance,mask,trigger);
                if(count<buffer.Length)return count;Grow(ref buffer);}
        }
    }
}
