using System;

namespace Necrocis
{
    // A special motor owns execution; ordinary input only observes the gate.
    public sealed class MovementExecutionGate
    {
        private MovementExecutionLease current;
        public bool BlocksOrdinaryInput => current != null;
        public MovementExecutionLease TryAcquire(object owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (current != null) return null;
            return current = new MovementExecutionLease(this);
        }
        internal bool Owns(MovementExecutionLease lease) => ReferenceEquals(current, lease);
        internal void Release(MovementExecutionLease lease) { if (Owns(lease)) current = null; }
        public void CancelAll() => current = null;
    }

    public sealed class MovementExecutionLease : IDisposable
    {
        private readonly MovementExecutionGate gate;
        internal MovementExecutionLease(MovementExecutionGate gate) => this.gate = gate;
        public bool IsActive => gate.Owns(this);
        public void Dispose() => gate.Release(this);
    }
}
