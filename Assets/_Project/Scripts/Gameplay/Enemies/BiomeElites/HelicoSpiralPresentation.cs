using System;
using UnityEngine;

namespace Necrocis
{
    [Serializable]
    public sealed class HelicoTailBodyFrame
    {
        public Sprite sprite;
        public Vector2[] bodyHull;
        public Vector2 attachment;
    }
    [Serializable]
    public sealed class HelicoSpriteGrounding
    {
        public Sprite sprite;
        [Tooltip("편모를 제외한 몸통 밑면의 Sprite 로컬 Y. 미술에서 측정한 표현 기준이며 공격 범위가 아닙니다.")]
        public float bodyBottomY;
    }

    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Helico Spiral Presentation")]
    public sealed class HelicoSpiralPresentation : ScriptableObject
    {
        public EnemyDirectionalPresentation directionalPresentation;
        public Sprite[] idleFrames, moveFrames, preparationFrames, dashFrames, recoveryFrames, deathFrames;
        public Sprite hit;
        [HideInInspector] public Sprite[] tailPreparationFrames, tailSweepFrames, tailRecoveryFrames;
        [HideInInspector] public HelicoTailBodyFrame[] tailBodies = Array.Empty<HelicoTailBodyFrame>();
        public HelicoSpriteGrounding[] groundings = Array.Empty<HelicoSpriteGrounding>();
        [Min(.02f)] public float idleFrameSeconds = .3f;
        [Min(.02f)] public float moveFrameSeconds = .2f;
        [Min(.02f)] public float deathFrameSeconds = .14f;
        public Color dangerColor = new Color(.72f, .035f, .1f, .58f);
        public Color dangerBorderColor = new Color(1, .68f, .72f, .95f);
        public string GetTailValidationError()
        {
            var banks = new[] { tailPreparationFrames, tailSweepFrames, tailRecoveryFrames };
            int[] counts = { 3, 3, 2 };
            for (int i = 0; i < banks.Length; i++)
                if (banks[i] == null || banks[i].Length != counts[i] || Array.Exists(banks[i], s => s == null)) return "꼬리 예고3/휩쓸기3/회복2가 필요합니다.";
            if (directionalPresentation == null || tailBodies == null) return "꼬리 방향/몸통 분리 정보가 필요합니다.";
            try
            {
                var snapshot = directionalPresentation.Capture();
                foreach (EnemyFacing facing in Enum.GetValues(typeof(EnemyFacing)))
                {
                    foreach (var bank in banks) foreach (var source in bank)
                        if (!Array.Exists(groundings, g => g != null && g.sprite == snapshot.Resolve(source, facing))) return "꼬리 동작의 접지가 누락됐습니다.";
                    foreach (var source in tailSweepFrames)
                        if (!Array.Exists(tailBodies, g => g != null && g.sprite == snapshot.Resolve(source, facing) && g.bodyHull != null && g.bodyHull.Length >= 3))
                            return "휩쓸기 몸통/꼬리 분리 정보가 누락됐습니다.";
                }
            }
            catch (InvalidOperationException ex) { return ex.Message; }
            return null;
        }
        public string GetValidationError()
        {
            if (directionalPresentation == null || directionalPresentation.mode != EnemyDirectionMode.Eight) return "나선충은 사선을 포함한 8방향 원본이 필요합니다.";
            string error = directionalPresentation.GetValidationError(); if (error != null) return error;
            var banks = new[] { idleFrames, moveFrames, preparationFrames, dashFrames, recoveryFrames, deathFrames };
            int[] counts = { 2, 2, 3, 2, 2, 6 };
            for (int i = 0; i < banks.Length; i++)
                if (banks[i] == null || banks[i].Length != counts[i] || Array.Exists(banks[i], s => s == null)) return "나선충 기본12장과 사망6장을 확인하세요.";
            if (hit == null) return "나선충 피격 프레임이 없습니다.";
            foreach (float n in new[] { idleFrameSeconds, moveFrameSeconds, deathFrameSeconds })
                if (!MonsterBalanceNumbers.Positive(n)) return "프레임 시간은 양수여야 합니다.";
            try
            {
                var snapshot = directionalPresentation.Capture();
                var feet = new System.Collections.Generic.Dictionary<Sprite, float>();
                if (groundings == null) return "방향별 몸통 접지 기준이 없습니다.";
                foreach (var entry in groundings)
                {
                    if (entry == null || entry.sprite == null || float.IsNaN(entry.bodyBottomY) || float.IsInfinity(entry.bodyBottomY)
                        || entry.bodyBottomY < entry.sprite.bounds.min.y || entry.bodyBottomY > entry.sprite.bounds.max.y || feet.ContainsKey(entry.sprite))
                        return "몸통 접지 기준의 Sprite·밑면·중복을 확인하세요.";
                    feet.Add(entry.sprite, entry.bodyBottomY);
                }
                foreach (EnemyFacing facing in Enum.GetValues(typeof(EnemyFacing)))
                {
                    foreach (var bank in banks) foreach (var sprite in bank)
                        if (!feet.ContainsKey(snapshot.Resolve(sprite, facing))) return "방향별 동작의 몸통 접지 기준이 누락됐습니다.";
                    if (!feet.ContainsKey(snapshot.Resolve(hit, facing))) return "피격 동작의 몸통 접지 기준이 누락됐습니다.";
                }
            }
            catch (InvalidOperationException ex) { return ex.Message; }
            return null;
        }
    }
}
