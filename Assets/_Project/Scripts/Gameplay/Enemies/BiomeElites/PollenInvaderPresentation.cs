using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Pollen Invader Presentation")]
    public sealed class PollenInvaderPresentation : ScriptableObject
    {
        public EnemyDirectionalPresentation directionalPresentation;
        public Sprite[] idleFrames, moveFrames, preparationFrames, deathFrames, pelletFrames, rotationFrames;
        public Sprite release, recovery, hit, groundDisc;
        [Min(.02f)] public float idleFrameSeconds = .35f;
        [Min(.02f)] public float moveFrameSeconds = .22f;
        [Min(.02f)] public float releaseFrameSeconds = .12f;
        [Min(.02f)] public float deathFrameSeconds = .14f;
        [Min(.02f)] public float pelletFrameSeconds = .12f;
        public Vector2 groundShadowSize = new Vector2(1.1f, .6f);
        public Color groundShadowColor = new Color(.16f, .1f, .04f, .25f);
        public string GetValidationError(bool includeFollowup = false)
        {
            if (directionalPresentation == null) return "꽃가루는 측면·정면·후면 본체와 사망이 필요합니다.";
            string error = directionalPresentation.GetValidationError(); if (error != null) return error;
            if (!Valid(idleFrames, 2) || !Valid(moveFrames, 2) || !Valid(preparationFrames, 3) || !Valid(deathFrames, 6)
                || !Valid(pelletFrames, 2) || release == null || recovery == null || hit == null || groundDisc == null)
                return "꽃가루 본체 10장·사망 6장·공용 탄 2장과 그림자를 지정하세요.";
            foreach (float n in new[] { idleFrameSeconds, moveFrameSeconds, releaseFrameSeconds, deathFrameSeconds, pelletFrameSeconds })
                if (!MonsterBalanceNumbers.Positive(n)) return "꽃가루 표현 시간은 양수여야 합니다.";
            if (includeFollowup && !Valid(rotationFrames, 3)) return "B 회전 예고 3장의 방향 원본을 지정하세요.";
            try
            {
                var snapshot = directionalPresentation.Capture();
                foreach (var bank in new[] { idleFrames, moveFrames, preparationFrames, deathFrames, new[] { release, recovery, hit }, includeFollowup ? rotationFrames : new Sprite[0] })
                    foreach (var frame in bank)
                        foreach (var facing in new[] { EnemyFacing.Right, EnemyFacing.Front, EnemyFacing.Back })
                            snapshot.Resolve(frame, facing);
            }
            catch (System.InvalidOperationException exception) { return exception.Message; }
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
