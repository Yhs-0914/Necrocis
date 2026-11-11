using System;
using System.Collections.Generic;

namespace Necrocis
{
    public sealed class PauseLease : IDisposable
    {
        private PauseCoordinator owner;
        private readonly long id;
        internal PauseLease(PauseCoordinator owner, long id) { this.owner = owner; this.id = id; }
        public void Dispose() { var current = owner; owner = null; current?.Release(id); }
    }

    // The first owner captures the clock. Only the last owner may restore it.
    public sealed class PauseCoordinator : IDisposable
    {
        private readonly Func<float> read;
        private readonly Action<float> write;
        private readonly Dictionary<long, object> owners = new Dictionary<long, object>();
        private long nextId;
        private float baseline;
        private bool disposed;
        public int Count => owners.Count;

        public PauseCoordinator(Func<float> read, Action<float> write)
        {
            this.read = read ?? throw new ArgumentNullException(nameof(read));
            this.write = write ?? throw new ArgumentNullException(nameof(write));
        }

        public PauseLease Acquire(object owner)
        {
            if (disposed) throw new ObjectDisposedException(nameof(PauseCoordinator));
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (owners.Count == 0) baseline = read();
            long id = ++nextId; owners.Add(id, owner); write(0f);
            return new PauseLease(this, id);
        }

        internal void Release(long id)
        {
            if (!owners.Remove(id)) return;
            write(owners.Count == 0 ? baseline : 0f);
        }

        public void SetBaseline(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f) throw new ArgumentOutOfRangeException(nameof(value));
            baseline = value;
            write(owners.Count == 0 ? value : 0f);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (owners.Count > 0) { owners.Clear(); write(baseline); }
        }
    }
}
