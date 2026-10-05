using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Inflammation Ember Presentation")]
    public sealed class InflammationEmberPresentation : ScriptableObject
    {
        [Tooltip("방향별 본체·사망과 발사 기준점입니다. 가시탄은 기준점의 XZ만 사용하며 높이는 패턴의 반두께로 유지합니다.")]
        public EnemyDirectionalPresentation directionalPresentation;
        public Sprite[] idleFrames, moveFrames, preparationFrames, deathFrames, thornFrames;
        public Sprite release, recovery, hit, groundDisc;
        [Min(.02f)] public float idleFrameSeconds = .35f;
        [Min(.02f)] public float moveFrameSeconds = .22f;
        [Min(.02f)] public float releaseFrameSeconds = .12f;
        [Min(.02f)] public float deathFrameSeconds = .14f;
        [Min(.02f)] public float thornFrameSeconds = .12f;
        public Vector2 groundShadowSize = new Vector2(1.4f, .7f);
        public Color groundShadowColor = new Color(.14f, .065f, .12f, .28f);

        public string GetValidationError()
        {
            if (directionalPresentation != null)
            {
                string error = directionalPresentation.GetValidationError();
                if (error != null) return error;
            }
            if (!Valid(idleFrames, 2) || !Valid(moveFrames, 2) || !Valid(preparationFrames, 3)
                || !Valid(deathFrames, 6) || !Valid(thornFrames, 2) || release == null || recovery == null || hit == null || groundDisc == null)
                return "염증 불씨의 본체 10장·사망 6장·가시탄 2장과 그림자를 지정하세요.";
            if (!MonsterBalanceNumbers.Positive(idleFrameSeconds) || !MonsterBalanceNumbers.Positive(moveFrameSeconds)
                || !MonsterBalanceNumbers.Positive(releaseFrameSeconds) || !MonsterBalanceNumbers.Positive(deathFrameSeconds)
                || !MonsterBalanceNumbers.Positive(thornFrameSeconds)) return "염증 불씨 표현 시간은 양수여야 합니다.";
            foreach (Sprite frame in deathFrames)
                if (frame == recovery || frame == release) return "염증 불씨 전용 사망 프레임을 사용하세요.";
            return null;
        }

        private static bool Valid(Sprite[] frames, int minimum)
        {
            if (frames == null || frames.Length < minimum) return false;
            foreach (Sprite frame in frames) if (frame == null) return false;
            return true;
        }
    }
}
