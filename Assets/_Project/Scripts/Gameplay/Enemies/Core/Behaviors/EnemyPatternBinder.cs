using System;

namespace Necrocis
{
    public interface IEnemyPatternAttachment
    {
        void Attach(EnemyController enemy);
    }

    public static class EnemyPatternBinder
    {
        // The extension preserves existing typed-as-MonsterPatternSettings editor callers.
        public static void Attach(this MonsterPatternSettings settings, EnemyController enemy)
        {
            if (settings == null) return;
            if (!(settings is IEnemyPatternAttachment attachment))
                throw new InvalidOperationException(settings.name + ": Enemy pattern attachment is not registered.");
            attachment.Attach(enemy);
        }
    }
}
