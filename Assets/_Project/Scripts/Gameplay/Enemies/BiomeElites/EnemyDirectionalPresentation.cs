using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public enum EnemyFacing { Right, BackRight, Back, BackLeft, Left, FrontLeft, Front, FrontRight }
    public enum EnemyDirectionMode { Four, Eight }
    public enum EnemyPoseGroup { Idle, Preparation, Recovery, Hit, Death, Move, Release }

    [Serializable]
    public sealed class DirectionalSpriteFrame
    {
        public EnemyPoseGroup motion;
        public string label;
        [Tooltip("기존 측면 원본을 찾는 키입니다. 패턴의 재생 순서/시간은 복제하지 않습니다.")]
        public Sprite source;
        public Sprite front, back, frontDiagonal, backDiagonal;
    }

    [CreateAssetMenu(menuName = "Necrocis/Biome Elites/Directional Presentation")]
    public sealed class EnemyDirectionalPresentation : ScriptableObject
    {
        public EnemyDirectionMode mode;
        public bool authoredFacingLeft = true;
        [Range(0, 20)] public float boundaryHysteresisDegrees = 8;
        [Tooltip("Sprite의 정수 발 피벗이 몸체 기준입니다. 아래 값은 Visual 로컬 평면의 시각적 생성 기준점입니다.")]
        public Vector2 sideOrigin, frontOrigin, backOrigin, frontDiagonalOrigin, backDiagonalOrigin;
        public DirectionalSpriteFrame[] frames = Array.Empty<DirectionalSpriteFrame>();

        public string GetValidationError()
        {
            if (!Enum.IsDefined(typeof(EnemyDirectionMode), mode)) return "방향 모드를 확인하세요.";
            if (float.IsNaN(boundaryHysteresisDegrees) || float.IsInfinity(boundaryHysteresisDegrees)
                || boundaryHysteresisDegrees < 0 || boundaryHysteresisDegrees > 20) return "방향 경계 유지 각도는 0~20도입니다.";
            foreach (Vector2 value in new[] { sideOrigin, frontOrigin, backOrigin, frontDiagonalOrigin, backDiagonalOrigin })
                if (float.IsNaN(value.x) || float.IsInfinity(value.x) || float.IsNaN(value.y) || float.IsInfinity(value.y))
                    return "시각 기준점은 유한한 값이어야 합니다.";
            if (frames == null || frames.Length == 0) return "방향별 프레임을 지정하세요.";
            var keys = new HashSet<Sprite>();
            bool hasDeath = false;
            foreach (DirectionalSpriteFrame frame in frames)
            {
                if (frame == null || frame.source == null || frame.front == null || frame.back == null)
                    return "측면 원본 키와 정면·후면 프레임을 모두 지정하세요.";
                if (!keys.Add(frame.source)) return "동일 측면 원본 키가 중복되었습니다: " + frame.source.name;
                if (mode == EnemyDirectionMode.Eight && (frame.frontDiagonal == null || frame.backDiagonal == null))
                    return "8방향 모드에는 앞·뒤 사선 원본이 필요합니다.";
                if (!Enum.IsDefined(typeof(EnemyPoseGroup), frame.motion)) return "동작 분류를 확인하세요.";
                hasDeath |= frame.motion == EnemyPoseGroup.Death;
            }
            return hasDeath ? null : "방향별 사망 프레임을 포함하세요.";
        }

        public EnemyDirectionalSnapshot Capture()
        {
            string error = GetValidationError();
            if (error != null) throw new InvalidOperationException(name + ": " + error);
            return new EnemyDirectionalSnapshot(this);
        }
    }

    public sealed class EnemyDirectionalSnapshot
    {
        private readonly Dictionary<Sprite, Sprite[]> frames = new Dictionary<Sprite, Sprite[]>();
        private readonly Vector2[] origins;
        public readonly EnemyDirectionMode Mode;
        public readonly bool AuthoredFacingLeft;
        public readonly float Hysteresis;

        internal EnemyDirectionalSnapshot(EnemyDirectionalPresentation source)
        {
            Mode = source.mode; AuthoredFacingLeft = source.authoredFacingLeft; Hysteresis = source.boundaryHysteresisDegrees;
            origins = new[] { source.sideOrigin, source.frontOrigin, source.backOrigin, source.frontDiagonalOrigin, source.backDiagonalOrigin };
            foreach (DirectionalSpriteFrame frame in source.frames)
                frames.Add(frame.source, new[] { frame.source, frame.front, frame.back, frame.frontDiagonal, frame.backDiagonal });
        }

        private static int Bank(EnemyFacing facing)
        {
            switch (facing)
            {
                case EnemyFacing.Front: return 1;
                case EnemyFacing.Back: return 2;
                case EnemyFacing.FrontLeft: case EnemyFacing.FrontRight: return 3;
                case EnemyFacing.BackLeft: case EnemyFacing.BackRight: return 4;
                default: return 0;
            }
        }

        public Sprite Resolve(Sprite source, EnemyFacing facing)
        {
            if (source == null) return null;
            if (!frames.TryGetValue(source, out Sprite[] row))
                throw new InvalidOperationException("방향 원본 키가 없습니다: " + source.name);
            Sprite sprite = row[Bank(facing)];
            if (sprite == null) throw new InvalidOperationException("방향 프레임이 없습니다: " + facing + "/" + source.name);
            return sprite;
        }

        public bool Flip(EnemyFacing facing)
        {
            if (facing == EnemyFacing.Front || facing == EnemyFacing.Back) return false;
            bool left = facing == EnemyFacing.Left || facing == EnemyFacing.FrontLeft || facing == EnemyFacing.BackLeft;
            return AuthoredFacingLeft ? !left : left;
        }

        public Vector2 Origin(EnemyFacing facing)
        {
            Vector2 origin = origins[Bank(facing)];
            if (Flip(facing)) origin.x = -origin.x;
            return origin;
        }

        public EnemyFacing Select(Vector3 direction, Camera camera, EnemyFacing current, bool initialized)
        {
            direction.y = 0;
            if (direction.sqrMagnitude < .000001f) return current;
            Vector3 right = camera != null ? Vector3.ProjectOnPlane(camera.transform.right, Vector3.up) : Vector3.right;
            if (right.sqrMagnitude < .000001f) right = Vector3.right;
            right.Normalize();
            Vector3 up = Vector3.Cross(right, Vector3.up);
            if (camera != null && Vector3.Dot(up, camera.transform.up) < 0) up = -up;
            float angle = Mathf.Atan2(Vector3.Dot(direction, up), Vector3.Dot(direction, right)) * Mathf.Rad2Deg;
            float sector = Mode == EnemyDirectionMode.Eight ? 45 : 90;
            if (initialized && Mathf.Abs(Mathf.DeltaAngle((int)current * 45, angle)) <= sector * .5f + Hysteresis) return current;
            int index = Mathf.RoundToInt(angle / sector) * (Mode == EnemyDirectionMode.Eight ? 1 : 2);
            return (EnemyFacing)((index % 8 + 8) % 8);
        }
    }
}
