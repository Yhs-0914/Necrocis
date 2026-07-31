using System;
using System.Collections.Generic;

namespace Necrocis
{
    [Serializable]
    public sealed class ProfileSaveData
    {
        public int schemaVersion = SaveService.CurrentSchemaVersion;
        public bool normalCampaignCompleted;
        public bool hardUnlocked;
        public BossProgressSaveData bossDiscoveries = new BossProgressSaveData();
        public int normalClearCount;
        public int hardClearCount;
        public GameDifficulty lastSelectedDifficulty = GameDifficulty.Normal;
        public GameDifficulty lastPlayedDifficulty = GameDifficulty.Normal;
        public long lastSavedUtcTicks;
    }

    [Serializable]
    public sealed class RunSaveData
    {
        public int schemaVersion = SaveService.CurrentSchemaVersion;
        public string saveId;
        public GameDifficulty difficulty;
        public bool isActive;
        public bool campaignStarted;
        public bool campaignCompleted;
        public PlayerProgressSaveData player = new PlayerProgressSaveData();
        public BossProgressSaveData bosses = new BossProgressSaveData();
        public ResumeCheckpointSaveData checkpoint = new ResumeCheckpointSaveData();
        public WorldRunSaveData world = new WorldRunSaveData();
        public long playTimeSeconds;
        public long lastSavedUtcTicks;
    }

    [Serializable]
    public sealed class PlayerProgressSaveData
    {
        public int level = 1;
        public int experience;
        public JobType job = JobType.None;
        public float currentHealth = -1f;
        public List<LevelUpStatChoice> selectionHistory = new List<LevelUpStatChoice>();
        public List<SavedStatModifierData> levelUpModifiers = new List<SavedStatModifierData>();
        public List<SavedItemStateData> items = new List<SavedItemStateData>();
    }

    [Serializable]
    public sealed class SavedStatModifierData
    {
        public CharacterStatType statType;
        public CharacterStatModifierMode mode;
        public float value;
    }

    [Serializable]
    public sealed class SavedItemStateData
    {
        public string itemId;
        public int bloodContractKillProgress;
        public int bloodContractHealthGainCount;
        public bool splitRegenerationUsed;
        public float decayOrganElapsedSeconds;
        public float plateletMembraneShield;
        public float plateletMembraneCooldownRemaining;
        public float recoveryFactorCooldownRemaining;
    }

    [Serializable]
    public sealed class BossProgressSaveData
    {
        public bool intestineDefeated;
        public bool liverDefeated;
        public bool stomachDefeated;
        public bool lungDefeated;
        public bool finalBossDefeated;

        public bool IsDefeated(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Intestine => intestineDefeated,
                BiomeType.Liver => liverDefeated,
                BiomeType.Stomach => stomachDefeated,
                BiomeType.Lung => lungDefeated,
                _ => false
            };
        }

        public void SetDefeated(BiomeType biome, bool defeated)
        {
            switch (biome)
            {
                case BiomeType.Intestine:
                    intestineDefeated = defeated;
                    break;
                case BiomeType.Liver:
                    liverDefeated = defeated;
                    break;
                case BiomeType.Stomach:
                    stomachDefeated = defeated;
                    break;
                case BiomeType.Lung:
                    lungDefeated = defeated;
                    break;
            }
        }

        public int DefeatedCount =>
            (intestineDefeated ? 1 : 0)
            + (liverDefeated ? 1 : 0)
            + (stomachDefeated ? 1 : 0)
            + (lungDefeated ? 1 : 0);
    }

    [Serializable]
    public sealed class ResumeCheckpointSaveData
    {
        public string sceneName = SceneLoader.SCENE_HUB;
        public BiomeType biome = BiomeType.None;
        public bool wasInBossRoom;
    }

    [Serializable]
    public sealed class WorldRunSaveData
    {
        public int intestineSeed;
        public int liverSeed;
        public int stomachSeed;
        public int lungSeed;
        public int intestineEntryCount;
        public int liverEntryCount;
        public int stomachEntryCount;
        public int lungEntryCount;

        public int GetSeed(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Intestine => intestineSeed,
                BiomeType.Liver => liverSeed,
                BiomeType.Stomach => stomachSeed,
                BiomeType.Lung => lungSeed,
                _ => 0
            };
        }

        public void SetSeed(BiomeType biome, int value)
        {
            switch (biome)
            {
                case BiomeType.Intestine:
                    intestineSeed = value;
                    break;
                case BiomeType.Liver:
                    liverSeed = value;
                    break;
                case BiomeType.Stomach:
                    stomachSeed = value;
                    break;
                case BiomeType.Lung:
                    lungSeed = value;
                    break;
            }
        }

        public int GetEntryCount(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Intestine => intestineEntryCount,
                BiomeType.Liver => liverEntryCount,
                BiomeType.Stomach => stomachEntryCount,
                BiomeType.Lung => lungEntryCount,
                _ => 0
            };
        }

        public void SetEntryCount(BiomeType biome, int value)
        {
            value = Math.Max(0, value);
            switch (biome)
            {
                case BiomeType.Intestine:
                    intestineEntryCount = value;
                    break;
                case BiomeType.Liver:
                    liverEntryCount = value;
                    break;
                case BiomeType.Stomach:
                    stomachEntryCount = value;
                    break;
                case BiomeType.Lung:
                    lungEntryCount = value;
                    break;
            }
        }
    }

    public readonly struct ContinueSaveSummary
    {
        public ContinueSaveSummary(
            GameDifficulty difficulty,
            int level,
            JobType job,
            int defeatedBossCount,
            string sceneName,
            long lastSavedUtcTicks)
        {
            Difficulty = difficulty;
            Level = level;
            Job = job;
            DefeatedBossCount = defeatedBossCount;
            SceneName = sceneName;
            LastSavedUtcTicks = lastSavedUtcTicks;
        }

        public GameDifficulty Difficulty { get; }
        public int Level { get; }
        public JobType Job { get; }
        public int DefeatedBossCount { get; }
        public string SceneName { get; }
        public long LastSavedUtcTicks { get; }
    }
}
