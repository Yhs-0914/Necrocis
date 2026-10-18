using System;
namespace Necrocis
{
    public static class EnemyDefeatBindings
    {
        public static Action<EnemyController> PlayerKillReaction;
        public static Action<int> ResolvedExperience;
        public static Action<int, float> LegacyExperience;
        public static void NotifyPlayerKill(EnemyController enemy) => PlayerKillReaction?.Invoke(enemy);
    }
}
