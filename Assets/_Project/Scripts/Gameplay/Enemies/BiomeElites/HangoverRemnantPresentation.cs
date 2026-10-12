using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Hangover Remnant Presentation")]
    public sealed class HangoverRemnantPresentation : ScriptableObject
    {
        [Tooltip("사용자 확정: 모든 이동 방향에서 정면 본체·사망을 사용합니다.")]
        public Sprite[] idleFrames, moveFrames, preparationFrames, retreatFrames, deathFrames, remnantFrames, burstFrames;
        public Sprite release, recovery, hit, groundDisc;
        [Min(.02f)] public float idleFrameSeconds = .35f;
        [Min(.02f)] public float moveFrameSeconds = .22f;
        [Min(.02f)] public float releaseFrameSeconds = .1f;
        [Min(.02f)] public float retreatFrameSeconds = .2f;
        [Min(.02f)] public float deathFrameSeconds = .14f;
        [Min(.02f)] public float remnantFrameSeconds = .18f;
        [Min(.02f)] public float burstFrameSeconds = .1f;
        public Color dangerFillColor = new Color(.92f, .04f, .04f, .3f);
        public Vector2 groundShadowSize = new Vector2(1.45f, .8f);
        public Color groundShadowColor = new Color(.14f, .065f, .12f, .28f);

        public string GetValidationError()
        {
            if (!Valid(idleFrames, 2) || !Valid(moveFrames, 2) || !Valid(preparationFrames, 3) || !Valid(retreatFrames, 2)
                || !Valid(deathFrames, 6) || !Valid(remnantFrames, 2) || !Valid(burstFrames, 4)
                || release == null || recovery == null || hit == null || groundDisc == null)
                return "정면 본체 12·사망 6·잔여물 2·터짐 4 프레임과 그림자를 지정하세요.";
            foreach (float value in new[] { idleFrameSeconds, moveFrameSeconds, releaseFrameSeconds, retreatFrameSeconds,
                deathFrameSeconds, remnantFrameSeconds, burstFrameSeconds, groundShadowSize.x, groundShadowSize.y })
                if (!MonsterBalanceNumbers.Positive(value)) return "숙취 잔재의 표시 시간·그림자 크기는 양수여야 합니다.";
            return null;
        }
        private static bool Valid(Sprite[] frames, int count)
        {
            if (frames == null || frames.Length != count) return false;
            foreach (Sprite frame in frames) if (frame == null) return false;
            return true;
        }
    }
}
