using System;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
#endif

namespace Necrocis
{
    // Native callbacks and the Editor integration adapter use this same state transition.
    public sealed class GameplayFocusOwner : IDisposable
    {
        private readonly InputContextStack contexts;
        private readonly Action clearHeldIntent;
        private PauseLease pause;
        private bool disposed, hasNativeValue;
        private bool? applied;
        public bool NativeFocused { get; private set; }
        public int NativeTransitions { get; private set; }

#if UNITY_EDITOR
        private static readonly HashSet<GameplayFocusOwner> instances = new HashSet<GameplayFocusOwner>();
        private static bool? testFocusOverride;
        public static bool? TestFocusOverride
        {
            get => testFocusOverride;
            set
            {
                testFocusOverride = value;
                foreach (var instance in instances.ToArray()) instance.Apply();
            }
        }
#endif

        public GameplayFocusOwner(InputContextStack contexts, Action clearHeldIntent)
        {
            this.contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));
            this.clearHeldIntent = clearHeldIntent;
#if UNITY_EDITOR
            instances.Add(this);
#endif
        }

        public void SetNativeFocused(bool focused)
        {
            if (disposed) return;
            if (!hasNativeValue || NativeFocused != focused) NativeTransitions++;
            hasNativeValue = true; NativeFocused = focused;
            Apply();
        }

        private void Apply()
        {
            if (disposed || !hasNativeValue) return;
            bool focused = NativeFocused;
#if UNITY_EDITOR
            focused = testFocusOverride ?? focused;
#endif
            if (applied == focused) return;
            applied = focused;
            if (!focused)
            {
                pause ??= contexts.Pause.Acquire(this);
                try { contexts.SetFocused(false); }
                finally { clearHeldIntent?.Invoke(); }
            }
            else
            {
                try { contexts.SetFocused(true); }
                finally { pause?.Dispose(); pause = null; }
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
#if UNITY_EDITOR
            instances.Remove(this);
#endif
            pause?.Dispose(); pause = null;
        }
    }
}
