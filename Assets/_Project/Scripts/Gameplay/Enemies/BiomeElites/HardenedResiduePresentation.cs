using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Hardened Residue Presentation")]
    public sealed class HardenedResiduePresentation : ScriptableObject
    {
        public EnemyDirectionalPresentation directionalPresentation;
        [Tooltip("승인된 들기 자세에서 분리한 조각. 비행 중만 사용하며 착지하면 기존 부스러기로 전환합니다.")]
        public Sprite airborneChip;
        public Sprite[] idleFrames;
        public Sprite[] moveFrames;
        public Sprite[] liftFrames;
        public Sprite release, recovery, hit, rubble;
        [Tooltip("상하 공격에 사용하는 선택된 세로 잔해. 착지 시 기존 가로 잔해 대신 사용합니다.")]
        public Sprite verticalRubble;
        [Range(.25f, 1f)]
        [Tooltip("세로 잔해 이미지 높이 중 바닥 길이에 해당하는 비율입니다. 나머지는 돌의 높이로 표현합니다. 그림을 균일 배율로 표시하며 피해/차단 범위는 바꾸지 않습니다. 변경은 다음 생성부터 반영됩니다.")]
        public float verticalRubbleGroundDepthFraction = .78f;
        public Sprite[] deathFrames;
        [Min(.02f)] public float idleFrameSeconds = .35f;
        [Min(.02f)] public float moveFrameSeconds = .22f;
        [Min(.02f)] public float deathFrameSeconds = .14f;
        public Sprite groundDisc;
        public Color crackColor = new Color(1f, .2f, .16f, .95f);
        [Tooltip("착지 피해 범위 전체에 표시하는 배경색. Alpha로 투명도를 조절합니다.")]
        public Color dangerFillColor = new Color(1f, .04f, .03f, .42f);
        public Color groundShadowColor = new Color(.14f, .065f, .12f, .28f);
        public Vector2 groundShadowSize = new Vector2(1.65f, .75f);

        public string GetValidationError()
        {
            if (directionalPresentation != null)
            {
                string error = directionalPresentation.GetValidationError();
                if (error != null) return error;
                if (airborneChip == null) return "방향별 조각 출발점과 함께 비행용 조각을 지정하세요.";
                if (verticalRubble == null) return "상하 공격용 세로 잔해를 지정하세요.";
                if (!MonsterBalanceNumbers.Positive(verticalRubbleGroundDepthFraction)
                    || verticalRubbleGroundDepthFraction < .25f || verticalRubbleGroundDepthFraction > 1)
                    return "세로 잔해의 바닥 길이 비율은 0.25~1 사이여야 합니다.";
            }
            if (!Valid(idleFrames, 1) || !Valid(moveFrames, 2) || !Valid(liftFrames, 2)
                || !Valid(deathFrames, 2) || release == null || recovery == null || hit == null || rubble == null || groundDisc == null)
                return "굳은 잔여체의 대기/이동/들기/내려놓기/회복/피격/사망/부스러기 표현을 지정하세요.";
            if (!MonsterBalanceNumbers.Positive(idleFrameSeconds) || !MonsterBalanceNumbers.Positive(moveFrameSeconds)
                || !MonsterBalanceNumbers.Positive(deathFrameSeconds)) return "표현 프레임 시간은 양수여야 합니다.";
            foreach (Sprite frame in deathFrames)
                if (frame == recovery || frame == release) return "전용 사망 프레임을 사용하세요.";
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
