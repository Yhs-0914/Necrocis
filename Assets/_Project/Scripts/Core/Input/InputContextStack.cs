using System;
using System.Collections.Generic;

namespace Necrocis
{
    public sealed class InputContextLease : IDisposable
    {
        private InputContextStack owner;
        private readonly long id;
        internal InputContextLease(InputContextStack owner, long id) { this.owner = owner; this.id = id; }
        public bool CanInteract => owner != null && owner.IsTop(id);
        public void Dispose() { var current = owner; owner = null; current?.Remove(id); }
    }

    // Visual priority is authoritative; later contexts win ties at the same Canvas order.
    public sealed class InputContextStack : IDisposable
    {
        private sealed class Entry
        {
            public long Id;
            public object Owner;
            public int Priority;
            public bool BlocksGameplay, AllowAuxiliary, ConsumeEscape;
            public Action Escape;
            public Action<bool> Interactivity;
            public PauseLease Pause;
            public bool? LastInteractive;
        }
        private readonly List<Entry> entries = new List<Entry>();
        private long nextId;
        private int escapeFrame = -1;
        private bool notifying, changed, disposed, resetting;
        public PauseCoordinator Pause { get; }
        public bool IsFocused { get; private set; } = true;
        public int Count => entries.Count;
        public object TopOwner => Top()?.Owner;
        public bool CanOpenAuxiliary => !disposed && !resetting && IsFocused && (Top() == null || Top().AllowAuxiliary);
        public bool BlocksGameplay
        {
            get
            {
                if (!IsFocused) return true;
                foreach (Entry entry in entries) if (entry.BlocksGameplay) return true;
                return false;
            }
        }

        public InputContextStack(PauseCoordinator pause) => Pause = pause ?? throw new ArgumentNullException(nameof(pause));

        public InputContextLease Register(object owner, int priority, bool pause, bool blocksGameplay = true,
            bool allowAuxiliary = true, Action escape = null, bool consumeEscape = true, Action<bool> interactivity = null)
        {
            if (disposed) throw new ObjectDisposedException(nameof(InputContextStack));
            if (resetting) throw new InvalidOperationException("Cannot open an input context while its session is resetting.");
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            foreach (Entry existing in entries)
                if (ReferenceEquals(existing.Owner, owner)) throw new InvalidOperationException("An owner already has an input context.");
            var entry = new Entry { Id = ++nextId, Owner = owner, Priority = priority, BlocksGameplay = blocksGameplay,
                AllowAuxiliary = allowAuxiliary, Escape = escape, ConsumeEscape = consumeEscape, Interactivity = interactivity };
            if (pause) entry.Pause = Pause.Acquire(owner);
            entries.Add(entry);
            try { Notify(); }
            catch (Exception error)
            {
                var errors = new List<Exception> { error };
                entries.Remove(entry); entry.Pause?.Dispose();
                try { entry.Interactivity?.Invoke(false); } catch (Exception cleanup) { errors.Add(cleanup); }
                try { Notify(); } catch (Exception cleanup) { errors.Add(cleanup); }
                throw new AggregateException("Input context registration failed and was rolled back.", errors);
            }
            return new InputContextLease(this, entry.Id);
        }

        public bool CanInteract(object owner) => owner != null && IsFocused && ReferenceEquals(Top()?.Owner, owner);
        internal bool IsTop(long id) => IsFocused && Top()?.Id == id;

        private Entry Top()
        {
            Entry top = null;
            foreach (Entry entry in entries)
                if (top == null || entry.Priority > top.Priority || entry.Priority == top.Priority && entry.Id > top.Id) top = entry;
            return top;
        }

        public bool DispatchEscape(int frame)
        {
            if (escapeFrame == frame) return true;
            escapeFrame = frame;
            if (!IsFocused) return true;
            Entry top = Top();
            if (top == null || !top.ConsumeEscape) return false;
            top.Escape?.Invoke();
            return true;
        }

        internal void Remove(long id)
        {
            int index = entries.FindIndex(entry => entry.Id == id);
            if (index < 0) return;
            Entry removed = entries[index]; entries.RemoveAt(index);
            removed.Pause?.Dispose();
            var errors = new List<Exception>();
            try { removed.Interactivity?.Invoke(false); } catch (Exception error) { errors.Add(error); }
            try { Notify(); } catch (Exception error) { errors.Add(error); }
            if (errors.Count > 0) throw new AggregateException("Input context was released but a view callback failed.", errors);
        }

        public void SetFocused(bool focused)
        {
            if (IsFocused == focused) return;
            IsFocused = focused; Notify();
        }

        public void ResetSession(float baseline = 1f)
        {
            if (disposed) throw new ObjectDisposedException(nameof(InputContextStack));
            if (resetting) return;
            var errors = new List<Exception>();
            resetting = true;
            try
            {
                foreach (Entry entry in entries.ToArray())
                    try { Remove(entry.Id); } catch (Exception error) { errors.Add(error); }
            }
            finally
            {
                // The focus pause is independent of modal entries and remains owned.
                Pause.SetBaseline(baseline); resetting = false;
            }
            if (errors.Count > 0) throw new AggregateException("Session contexts were cleared but a view callback failed.", errors);
        }

        private void Notify()
        {
            changed = true;
            if (notifying) return;
            var errors = new List<Exception>();
            notifying = true;
            try
            {
                while (changed)
                {
                    changed = false;
                    Entry top = Top();
                    foreach (Entry entry in entries.ToArray())
                    {
                        bool interactive = IsFocused && ReferenceEquals(entry, top);
                        if (!entries.Contains(entry) || entry.LastInteractive == interactive) continue;
                        // Record the transition before invoking a callback that may partially
                        // apply it and throw; registration rollback must notify the old top again.
                        entry.LastInteractive = interactive;
                        try { entry.Interactivity?.Invoke(interactive); }
                        catch (Exception error) { errors.Add(error); }
                    }
                }
            }
            finally { notifying = false; }
            if (errors.Count > 0) throw new AggregateException("An input view callback failed.", errors);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            var errors = new List<Exception>();
            try
            {
                foreach (Entry entry in entries.ToArray())
                    try { Remove(entry.Id); } catch (Exception error) { errors.Add(error); }
            }
            finally { Pause.Dispose(); }
            if (errors.Count > 0) throw new AggregateException("Input contexts were disposed but a view callback failed.", errors);
        }
    }
}
