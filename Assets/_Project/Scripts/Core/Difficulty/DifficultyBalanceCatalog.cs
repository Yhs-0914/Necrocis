using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(
        menuName = "Necrocis/Balance/Difficulty Balance Catalog",
        fileName = "DifficultyBalanceCatalog")]
    public sealed class DifficultyBalanceCatalog : ScriptableObject
    {
        public DifficultyBalanceProfile normal;
        public DifficultyBalanceProfile hard;

        public DifficultyBalanceProfile Get(GameDifficulty difficulty)
        {
            return difficulty == GameDifficulty.Hard ? hard : normal;
        }
    }
}
