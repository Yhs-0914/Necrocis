using System;
using System.Collections.Generic;
using UnityEngine;
namespace Necrocis
{
    [Serializable]
    public sealed class OilFilmFrameProfile
    {
        public Sprite sprite;
        public int view, index;
        public float coreCut, angleMin, angleMax, reachFraction;
        public float[] rimRadii;
        public bool unfold;
    }
    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Oil Film Presentation")]
    public sealed class OilFilmPresentation : ScriptableObject
    {
        public EnemyDirectionalPresentation directionalPresentation;
        [HideInInspector] public Shader membraneShader;
        public Sprite[] idleFrames, moveFrames, preparationFrames, releaseFrames, recoveryFrames, hitFrames, deathFrames;
        public OilFilmFrameProfile[] profiles;
        [Min(.02f)] public float idleFrameSeconds = .35f, moveFrameSeconds = .22f, hitFrameSeconds = .1f, deathFrameSeconds = .14f;
        public Color dangerColor = new Color(.8f, .025f, .075f, .55f);
        public Color dangerBorderColor = new Color(1, .65f, .7f, .95f);
        public string GetValidationError()
        {
            if (membraneShader == null) return "기름막 지형 클리핑 셰이더 참조가 없습니다.";
            if (directionalPresentation == null || directionalPresentation.mode != EnemyDirectionMode.Four) return "기름막4방향 표현을 지정하세요.";
            string error = directionalPresentation.GetValidationError(); if (error != null) return error;
            var banks = new[] { idleFrames, moveFrames, preparationFrames, releaseFrames, recoveryFrames, hitFrames, deathFrames };
            int[] counts = { 2, 2, 4, 4, 4, 2, 6 };
            for (int i = 0; i < banks.Length; i++) if (banks[i] == null || banks[i].Length != counts[i] || Array.Exists(banks[i], s => s == null)) return "기름막 기본18장과 사망6장이 필요합니다.";
            foreach (float n in new[] { idleFrameSeconds, moveFrameSeconds, hitFrameSeconds, deathFrameSeconds }) if (!MonsterBalanceNumbers.Positive(n)) return "표현 시간은 양수여야 합니다.";
            if (profiles == null || profiles.Length != 72) return "승인된72장 표현 기준점이 필요합니다.";
            var selected = new HashSet<Sprite>();
            foreach (var p in profiles)
            {
                if (p == null || p.sprite == null || !selected.Add(p.sprite) || p.view < 0 || p.view > 2 || p.index < 0 || p.index >= 24) return "표현 프로필의 Sprite/방향/중복을 확인하세요.";
                if (p.unfold && (p.rimRadii == null || p.rimRadii.Length != 65 || !MonsterBalanceNumbers.Positive(p.coreCut)
                    || !MonsterBalanceNumbers.Positive(p.reachFraction) || !(p.angleMax > p.angleMin))) return "펼침 원본의 막 경계 정보가 없습니다.";
                if (p.unfold) foreach (float r in p.rimRadii) if (!MonsterBalanceNumbers.Positive(r)) return "막 경계 반경은 양수여야 합니다.";
            }
            try
            {
                var snapshot = directionalPresentation.Capture();
                foreach (var bank in banks) foreach (var frame in bank) foreach (var facing in new[] { EnemyFacing.Right, EnemyFacing.Left, EnemyFacing.Front, EnemyFacing.Back })
                    if (!selected.Contains(snapshot.Resolve(frame, facing))) return "방향별 표현 기준이 누락됐습니다.";
            }
            catch (InvalidOperationException e) { return e.Message; }
            return null;
        }
    }
}
