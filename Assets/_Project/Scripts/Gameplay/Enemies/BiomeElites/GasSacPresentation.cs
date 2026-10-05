using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Gas Sac Presentation")]
    public sealed class GasSacPresentation : ScriptableObject
    {
        public Sprite idle;
        public Sprite[] inflationFrames;
        public Sprite deflated;
        [Tooltip("A/B 본체와 사망의 방향별 원본입니다. 새 개체 생성 시 프레임·기준점을 고정합니다.")]
        public EnemyDirectionalPresentation directionalPresentation;
        [Header("Death · Gas Drain and Collapse")]
        public Sprite[] deathFrames;
        [Tooltip("사망 프레임 한 장당 재생 시간(초). 난이도/진행도 배율을 적용하지 않습니다.")]
        [Min(.02f)] public float deathFrameSeconds = .14f;
        [Header("Combat VFX")]
        public Sprite gasPuff;
        public Sprite filledCircle;
        public Color warningColor = new Color(1f, .2f, .16f, .95f);
        [Tooltip("예고 시작부터 실제 공격 범위 전체에 표시하는 배경색. Alpha로 투명도를 조절합니다.")]
        public Color dangerFillColor = new Color(1f, .04f, .03f, .42f);
        public Color burstColor = new Color(.92f, .3f, .2f, 1f);
        public Color gasColor = new Color(.78f, .87f, .56f, .8f);
        [Header("Ground Contact")]
        public Vector2 groundShadowSize = new Vector2(1.3f, .8f);
        public Color groundShadowColor = new Color(.12f, .055f, .16f, .32f);

        public string GetValidationError()
        {
            if (idle == null || deflated == null || gasPuff == null || filledCircle == null
                || inflationFrames == null || inflationFrames.Length < 2) return "가스낭 표현 스프라이트가 필요합니다.";
            foreach (Sprite frame in inflationFrames) if (frame == null) return "가스낭 팽창 프레임이 비어 있습니다.";
            if (deathFrames == null || deathFrames.Length < 2 || !MonsterBalanceNumbers.Positive(deathFrameSeconds))
                return "가스낭의 사망 전용 프레임과 양수 프레임 시간이 필요합니다.";
            foreach (Sprite frame in deathFrames)
                if (frame == null || frame == deflated || frame == idle) return "사망 전용 프레임을 지정하세요. 회복/대기 포즈를 재사용하지 않습니다.";
            if (directionalPresentation != null)
            {
                string error = directionalPresentation.GetValidationError();
                if (error != null) return error;
                bool HasKey(Sprite sprite) => System.Array.Exists(directionalPresentation.frames, row => row.source == sprite);
                if (!HasKey(idle) || !HasKey(deflated)) return "대기·회복의 방향 원본 키가 없습니다.";
                foreach (Sprite frame in inflationFrames) if (!HasKey(frame)) return "팽창의 방향 원본 키가 없습니다: " + frame.name;
                foreach (Sprite frame in deathFrames) if (!HasKey(frame)) return "사망의 방향 원본 키가 없습니다: " + frame.name;
            }
            return null;
        }
    }
}
