namespace Necrocis
{
    public readonly struct MonsterVisitContext
    {
        public string RunId { get; }
        public string VisitId { get; }
        public BiomeType Biome { get; }
        public GameDifficulty Difficulty { get; }
        public int Stage { get; }

        public MonsterVisitContext(string runId, string visitId, BiomeType biome, GameDifficulty difficulty, int stage)
        {
            RunId = runId; VisitId = visitId; Biome = biome; Difficulty = difficulty; Stage = stage;
        }
    }
}
