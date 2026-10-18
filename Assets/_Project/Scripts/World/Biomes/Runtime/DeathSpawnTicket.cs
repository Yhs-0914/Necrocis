using UnityEngine;

namespace Necrocis
{
    internal sealed class DeathSpawnTicket
    {
        internal readonly string Key;
        internal readonly EnemyDeathSpawnRequest Request;
        internal readonly EnemySpawnScope Scope;
        internal readonly EnemySpawnContext[] Children;
        internal readonly float StartedAt;
        internal readonly float DueAt;
        internal GameObject Visual;
        internal bool VisualCompleted;
        internal bool Deferred;
        internal bool Executed;
        internal int NextChild;

        internal DeathSpawnTicket(string key, EnemyDeathSpawnRequest request, EnemySpawnScope scope,
            EnemySpawnContext[] children, float now)
        {
            Key = key; Request = request; Scope = scope; Children = children; StartedAt = now;
            DueAt = now + request.Frames.Length * Mathf.Max(0f, request.FrameInterval);
            VisualCompleted = request.Frames.Length == 0;
        }
    }
}
