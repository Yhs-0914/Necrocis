namespace Necrocis
{
    public interface ISessionRuntimeSavePort
    {
        void Capture(RunSaveData run);
        bool TryRestore(RunSaveData run,out string error);
    }
    public static class BossLegacyKeys
    {
        public static string Get(BiomeType biome)=>biome switch{
            BiomeType.Intestine=>"necrocis.boss-defeated.intestine",
            BiomeType.Liver=>"necrocis.boss-defeated.liver",
            BiomeType.Stomach=>"necrocis.boss-defeated.stomach",
            BiomeType.Lung=>"necrocis.boss-defeated.lung",
            _=>string.Empty};
    }
}
