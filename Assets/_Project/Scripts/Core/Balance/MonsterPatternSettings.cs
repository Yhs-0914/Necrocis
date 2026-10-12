using UnityEngine;

namespace Necrocis
{
    public abstract class MonsterPatternSettings : ScriptableObject
    {
        public abstract string GetValidationError(MonsterDefinition definition);
        public abstract void Attach(EnemyController enemy);
    }

    public abstract class MonsterPatternController : MonoBehaviour
    {
        public abstract void EndSpawn();
    }
}
