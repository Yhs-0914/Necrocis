using System;

namespace Necrocis
{
    [Serializable]
    public sealed class SaveOperation
    {
        public int formatVersion = 1;
        public string id;
        public string kind;
        public string sourceRunId;
        public string runFile;
        public GameDifficulty difficulty;
        public bool terminal;
        public ProfileSaveData profile;
        public RunSaveData run;
    }
}
