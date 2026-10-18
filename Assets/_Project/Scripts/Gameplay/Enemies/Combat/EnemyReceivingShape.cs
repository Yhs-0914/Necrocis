using System;
using UnityEngine;

namespace Necrocis
{
    // Receiving geometry is separate from movement/contact geometry. One actor still owns one HP.
    [DefaultExecutionOrder(10000)]
    public sealed class EnemyReceivingShape : MonoBehaviour
    {
        private EnemyController owner;
        private SpriteRenderer visual;
        private Billboard billboard;
        private EnemyReceivingProfile profile;
        private EnemyReceivingProfile.Frame frame;
        private BoxCollider broad;
        private Collider[] explicitParts;
        private Sprite previousSprite;
        private Quaternion previousRotation;
        private bool previousFlip;
        public bool HasCustom => profile!=null || explicitParts!=null;
        public int ShapeCount {get{PrepareFrame();return explicitParts!=null?explicitParts.Length:frame?.regions.Length??0;}}
        public static EnemyReceivingShape Bind(EnemyController actor,EnemyReceivingProfile value,SpriteRenderer renderer)
        {
            var result=actor.GetComponent<EnemyReceivingShape>();
            if(result==null && value==null)return null;
            if(result==null)result=actor.gameObject.AddComponent<EnemyReceivingShape>();
            if(result.explicitParts!=null)foreach(var part in result.explicitParts)if(part!=null && part.gameObject!=actor.gameObject)part.enabled=false;
            result.owner=actor;result.profile=value;result.visual=renderer;result.billboard=renderer!=null?renderer.GetComponent<Billboard>():null;
            result.explicitParts=null;result.frame=null;result.previousSprite=null;result.enabled=value!=null;
            if(result.broad!=null)result.broad.enabled=false;
            if(value!=null && renderer!=null)
            {
                if(result.broad==null)
                {
                    var go=new GameObject("ReceivingBody");go.transform.SetParent(actor.transform,false);go.layer=actor.gameObject.layer;
                    result.broad=go.AddComponent<BoxCollider>();result.broad.isTrigger=true;
                    // Own physics body prevents receiving-trigger callbacks from being
                    // delivered to the movement body's EnemyContactDamage a second time.
                    var queryBody=go.AddComponent<Rigidbody>();queryBody.isKinematic=true;queryBody.useGravity=false;
                }
                result.ConfigureBroadBounds();
                result.PrepareFrame();
            }
            return result;
        }
        public static void BindParts(EnemyController actor,Collider[] parts)
        {
            var result=actor.GetComponent<EnemyReceivingShape>();if(result==null)result=actor.gameObject.AddComponent<EnemyReceivingShape>();
            result.owner=actor;result.profile=null;result.frame=null;result.explicitParts=parts;result.enabled=true;
            if(result.broad!=null)result.broad.enabled=false;
        }
        private void LateUpdate()=>PrepareFrame();
        public void PrepareFrame()
        {
            if(profile==null || visual==null)return;
            bool changed=visual.sprite!=previousSprite || visual.transform.rotation!=previousRotation || visual.flipX!=previousFlip;
            if(changed)
            {
                frame=profile.Find(visual.sprite);previousSprite=visual.sprite;previousRotation=visual.transform.rotation;previousFlip=visual.flipX;
                if(frame!=null)
                {
                    float bottom=(visual.transform.localRotation * Vector3.Scale(new Vector3(0,frame.bounds.yMin,0),visual.transform.localScale)).y;
                    var position=visual.transform.localPosition;position.y=-bottom;
                    if(billboard!=null)billboard.ResetBaseLocalPosition(position-Vector3.up*billboard.YOffset);else visual.transform.localPosition=position;

                }
            }
            broad.enabled=frame!=null && owner!=null && !owner.IsDead && owner.gameObject.activeInHierarchy;
        }
        private void ConfigureBroadBounds()
        {
            // Stable conservative candidate bounds cover every pose and both flips. Only the
            // current frame's physical regions authorize a hit; the union never applies damage.
            var camera=DontStarveCamera.GetActiveCamera();
            Quaternion rotation=Quaternion.Inverse(owner.transform.rotation)*(camera!=null?camera.transform.rotation:visual.transform.rotation);
            Bounds union=default;bool first=true;
            foreach(var orientation in new[]{rotation,visual.transform.localRotation})
            foreach(var f in profile.frames)for(int flip=-1;flip<=1;flip+=2)
            {
                float bottom=(orientation*Vector3.Scale(new Vector3(0,f.bounds.yMin,0),visual.transform.localScale)).y;
                foreach(var p in new[]{new Vector2(f.bounds.xMin,f.bounds.yMin),new Vector2(f.bounds.xMax,f.bounds.yMin),new Vector2(f.bounds.xMin,f.bounds.yMax),new Vector2(f.bounds.xMax,f.bounds.yMax)})
                {
                    Vector3 point=orientation*Vector3.Scale(new Vector3(p.x*flip,p.y,0),visual.transform.localScale)-Vector3.up*bottom;
                    if(first){union=new Bounds(point,Vector3.zero);first=false;}else union.Encapsulate(point);
                }
            }
            union.Expand(.08f);broad.center=union.center;broad.size=union.size;
        }
        public bool Accepts(Collider collider)
        {
            PrepareFrame();
            if(explicitParts!=null){for(int i=0;i<explicitParts.Length;i++)if(explicitParts[i]==collider)return true;return false;}
            return collider==broad && broad!=null && broad.enabled;
        }
        public bool IsEnabled(int index,int mask)
        {
            Collider c=explicitParts!=null?explicitParts[index]:broad;
            return c!=null && c.enabled && c.gameObject.activeInHierarchy && (mask&(1<<c.gameObject.layer))!=0;
        }
        public Bounds GetBounds(int index)
        {
            if(explicitParts!=null)return explicitParts[index]!=null?explicitParts[index].bounds:default;
            Rect r=frame.regions[index];if(visual.flipX)r.x=-r.xMax;
            var t=visual.transform;Vector3 a=t.TransformPoint(new Vector3(r.xMin,r.yMin,0));
            Bounds bounds=new Bounds(a,Vector3.zero);
            bounds.Encapsulate(t.TransformPoint(new Vector3(r.xMax,r.yMin,0)));bounds.Encapsulate(t.TransformPoint(new Vector3(r.xMin,r.yMax,0)));bounds.Encapsulate(t.TransformPoint(new Vector3(r.xMax,r.yMax,0)));
            Vector3 depth=t.TransformVector(Vector3.forward*.02f);bounds.Expand(new Vector3(Mathf.Abs(depth.x),Mathf.Abs(depth.y),Mathf.Abs(depth.z))*2);return bounds;
        }
        public static bool Allows(EnemyController enemy,Collider collider)
        {
            var shape=enemy!=null?enemy.GetComponent<EnemyReceivingShape>():null;
            return shape==null || !shape.HasCustom || shape.Accepts(collider);
        }
        public static bool IntersectsCapsule(EnemyController enemy,Vector3 a,Vector3 b,float radius)
        {
            var shape=enemy.GetComponent<EnemyReceivingShape>();if(shape==null || !shape.HasCustom)return true;
            int count=shape.ShapeCount;
            for(int i=0;i<count;i++)if(shape.IsEnabled(i,~0))
            {
                var bounds=shape.GetBounds(i);float dy=Mathf.Max(0,Mathf.Max(bounds.min.y-Mathf.Max(a.y,b.y),Mathf.Min(a.y,b.y)-bounds.max.y));
                if(PlanarHitGeometry.SegmentRectDistanceSquared(PlanarHitGeometry.Flat(a),PlanarHitGeometry.Flat(b),bounds)+dy*dy<=radius*radius)return true;
            }
            return false;
        }
        public static bool IntersectsSphere(EnemyController enemy,Vector3 center,float radius)=>IntersectsCapsule(enemy,center,center,radius);
        public static bool IntersectsBeam(EnemyController enemy,Collider candidate,Vector3 from,Vector3 to,float radius,float halfHeight)
        {
            var shape=enemy.GetComponent<EnemyReceivingShape>();int count=shape!=null && shape.HasCustom?shape.ShapeCount:1;
            for(int i=0;i<count;i++)
            {
                if(shape!=null && shape.HasCustom && !shape.IsEnabled(i,~0))continue;
                Bounds b=shape!=null && shape.HasCustom?shape.GetBounds(i):candidate.bounds;
                if(b.max.y<Mathf.Min(from.y,to.y)-halfHeight-radius || b.min.y>Mathf.Max(from.y,to.y)+halfHeight+radius)continue;
                if(PlanarHitGeometry.SegmentRectDistanceSquared(PlanarHitGeometry.Flat(from),PlanarHitGeometry.Flat(to),b)<=radius*radius)return true;
            }
            return false;
        }
    }
}
