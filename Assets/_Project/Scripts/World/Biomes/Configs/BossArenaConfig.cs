using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Biome/Boss Arena Config", fileName = "BossArenaConfig")]
    public class BossArenaConfig : ScriptableObject
    {
        [Header("Mid Boss Arena")]
        public MidBossArenaConfig midBossArena = new MidBossArenaConfig();

        public MidBossArenaConfig GetMidBossArenaConfig()
        {
            return midBossArena;
        }
    }
}
