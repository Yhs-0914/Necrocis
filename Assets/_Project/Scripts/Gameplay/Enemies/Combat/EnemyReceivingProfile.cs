using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    // Authored physical sprite regions; no texture readback or image analysis during play.
    public sealed class EnemyReceivingProfile : ScriptableObject
    {
        [Serializable] public sealed class Frame
        {
            public Sprite sprite;
            public Rect[] regions=Array.Empty<Rect>();
            public Rect bounds;
        }
        public Frame[] frames=Array.Empty<Frame>();
        private Dictionary<Sprite,Frame> lookup;
        public Frame Find(Sprite sprite)
        {
            if(sprite==null)return null;
            if(lookup==null){lookup=new Dictionary<Sprite,Frame>();foreach(var f in frames)if(f.sprite!=null)lookup[f.sprite]=f;}
            return lookup.TryGetValue(sprite,out var result)?result:null;
        }
        private void OnEnable()=>lookup=null;
    }
}
