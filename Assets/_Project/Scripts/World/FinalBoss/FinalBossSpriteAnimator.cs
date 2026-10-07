using System;
using UnityEngine;

namespace Necrocis
{
    /// <summary>Frame animation on the artwork itself, so the corpse survives combat-root cleanup.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class FinalBossSpriteAnimator : MonoBehaviour
    {
        public enum Pose { Idle, Move, Melee, Cast, Reflection, Death }
        private readonly Sprite[][] frames = new Sprite[6][];
        private SpriteRenderer spriteRenderer;
        private float elapsed;
        private float oneShotDuration;
        private bool initialized;
        private bool playing;

        public Pose CurrentPose { get; private set; }
        public bool HasFrames => initialized;
        public bool IsOneShotPlaying => CurrentPose == Pose.Death
            || (oneShotDuration > 0f && elapsed < oneShotDuration);
        public Sprite IdleSprite => initialized ? frames[(int)Pose.Idle][0] : null;

        public bool Initialize()
        {
            if (initialized) return true;
            spriteRenderer = GetComponent<SpriteRenderer>();
            foreach (Pose pose in Enum.GetValues(typeof(Pose)))
            {
                Sprite[] sequence = Resources.LoadAll<Sprite>("FinalBoss/PhaseThree/Animations/" + pose);
                Array.Sort(sequence, (a, b) => string.CompareOrdinal(a.name, b.name));
                if (sequence.Length != 4)
                {
                    Debug.LogWarning("[FinalBoss] Missing four-frame animation: " + pose);
                    return false;
                }
                frames[(int)pose] = sequence;
            }
            initialized = true;
            return true;
        }

        public void Play(Pose pose, float duration = 0f, bool restart = false)
        {
            if (!initialized || (CurrentPose == Pose.Death && !restart)) return;
            if (playing && CurrentPose == pose && !restart) return;
            CurrentPose = pose;
            playing = true;
            elapsed = 0f;
            oneShotDuration = duration;
            spriteRenderer.sprite = frames[(int)pose][0];
        }

        private void Update() => Advance(Time.deltaTime);

        public void Advance(float deltaTime)
        {
            if (!initialized || !playing) return;
            elapsed += Mathf.Max(0f, deltaTime);
            Sprite[] sequence = frames[(int)CurrentPose];
            spriteRenderer.sprite = sequence[FrameIndex(CurrentPose, elapsed, oneShotDuration, sequence.Length)];
        }

        // One-shots hold their final frame. Death never wraps back to a living frame.
        public static int FrameIndex(Pose pose, float time, float duration, int count)
        {
            if (count <= 1) return 0;
            time = Mathf.Max(0f, time);
            float fps = pose == Pose.Move ? 9f : pose == Pose.Idle ? 5f : 7f;
            if (duration > 0f || pose == Pose.Death)
            {
                float length = duration > 0f ? duration : 1.6f;
                return Mathf.Min(count - 1, Mathf.FloorToInt(time / length * count));
            }
            return Mathf.FloorToInt(time * fps) % count;
        }
    }
}
