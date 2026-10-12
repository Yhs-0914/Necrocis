using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Dust Clump Presentation")]
    public sealed class DustClumpPresentation : ScriptableObject
    {
        public Sprite[] idleFrames, moveFrames, preparationFrames, coreFrames, deathFrames, coreDeathFrames, cloudFrames, dissolveFrames;
        public Sprite release, recovery, hit, groundDisc;
        [Min(.02f)] public float idleFrameSeconds = .3f;
        [Min(.02f)] public float moveFrameSeconds = .22f;
        [Min(.02f)] public float coreFrameSeconds = .25f;
        [Min(.02f)] public float cloudFrameSeconds = .15f;
        [Min(.02f)] public float dissolveFrameSeconds = .12f;
        [Min(.02f)] public float deathFrameSeconds = .14f;
        [Range(.2f, 1)] public float cloudOpacity = .85f;
        [Tooltip("Visual-only alpha cap while dust overlaps the player on screen.")]
        [Range(.1f, 1)] public float playerOverlapOpacity = .35f;
        public Color dangerFillColor = new Color(.92f, .04f, .04f, .3f);
        public Vector3 coreColliderSize = new Vector3(.55f, .85f, .55f);
        public Vector3 coreColliderCenter = new Vector3(0, .6f, 0);
        public Vector2 groundShadowSize = new Vector2(1.5f, .8f);
        public Vector2 coreShadowSize = new Vector2(.7f, .45f);
        public Color groundShadowColor = new Color(.12f, .1f, .15f, .25f);
        public string GetValidationError()
        {
            if (!Valid(idleFrames, 2) || !Valid(moveFrames, 2) || !Valid(preparationFrames, 3) || !Valid(coreFrames, 2)
                || !Valid(deathFrames, 6) || !Valid(coreDeathFrames, 6) || !Valid(cloudFrames, 4) || !Valid(dissolveFrames, 2)
                || release == null || recovery == null || hit == null || groundDisc == null) return "Dust requires all 30 approved sprites and a shadow disc.";
            foreach (float value in new[] { idleFrameSeconds, moveFrameSeconds, coreFrameSeconds, cloudFrameSeconds,
                dissolveFrameSeconds, deathFrameSeconds, coreColliderSize.x, coreColliderSize.y, coreColliderSize.z,
                groundShadowSize.x, groundShadowSize.y, coreShadowSize.x, coreShadowSize.y })
                if (!MonsterBalanceNumbers.Positive(value)) return "Invalid dust presentation size/time.";
            if (!MonsterBalanceNumbers.Positive(playerOverlapOpacity) || playerOverlapOpacity > 1) return "Invalid player-overlap opacity.";
            if (!MonsterBalanceNumbers.Positive(cloudOpacity) || cloudOpacity > 1) return "Invalid cloud opacity.";
            return null;
        }
        private static bool Valid(Sprite[] frames, int count)
        {
            if (frames == null || frames.Length != count) return false;
            foreach (var f in frames) if (f == null) return false;
            return true;
        }
    }
}
