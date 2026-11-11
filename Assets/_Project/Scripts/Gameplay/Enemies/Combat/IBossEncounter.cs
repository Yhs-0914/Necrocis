using System;

namespace Necrocis
{
    public interface IBossEncounter : IDisposable
    {
        bool HasPattern { get; }
        bool IsGroup { get; }
        bool IsDefeated { get; }
        void SetActive(bool active);
        void ForEachActor(Action<EnemyController> action);
        void Recenter();
    }
}
