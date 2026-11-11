using UnityEngine;
namespace Necrocis
{
    public readonly struct CooldownSnapshot
    {
        public readonly float ReadyAt,EffectiveDuration;
        public CooldownSnapshot(float readyAt,float duration){ReadyAt=readyAt;EffectiveDuration=duration;}
        public float Remaining(float now)=>Mathf.Max(0f,ReadyAt-now);
    }
}
