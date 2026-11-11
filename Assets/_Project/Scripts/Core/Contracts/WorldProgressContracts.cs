using System;
using UnityEngine;

namespace Necrocis
{
    // Read the current session at the original call point; never cache a run in a scene adapter.
    public interface ISessionReadView
    {
        GameDifficulty ActiveDifficulty { get; }
        bool IsRestorePending { get; }
        bool HasActiveSession { get; }
        string ActiveRunId { get; }
    }

    public interface IBiomeVisitProgress
    {
        MonsterVisitContext EnterLoadedBiome(BiomeType biome, int sceneHandle);
        int GetOrCreateSeed(BiomeType biome);
        BiomeElitePlan GetElitePlan(BiomeType biome);
        bool TryStoreElitePlan(BiomeElitePlan plan, out string error);
    }

    public interface IFieldEliteProgress
    {
        string CurrentRunId { get; }
        bool IsDefeated(string spawnId);
        bool TryRecordDefeat(string runId, string spawnId, out bool fresh, out string error);
    }

    public interface IEncounterProgression
    {
        void EnterMidBoss();
        void CompleteMidBoss(BiomeType? biome);
        void EnterFinalBoss();
        bool TryCompleteFinalBoss(out string error);
    }

    public interface IRelicProgressView
    {
        bool IsAvailable { get; }
        bool HasAll { get; }
        int Count { get; }
        bool HasRelic(BiomeType biome);
        IDisposable SubscribeAllCollected(Action callback);
    }

    public interface IWorldTravelCommands
    {
        bool HasLoader { get; }
        void NotifyBiomeEntered(BiomeType biome);
        void NotifyHubReturned();
        void LoadBiome(BiomeType biome);
        void ReturnToHub();
        bool LoadFinalBoss();
    }

    public interface IBossRewardCommands
    {
        bool TrySpawnBonus(GameObject owner, Vector3 position);
    }

    public interface IFinalVictoryPresentation
    {
        void Schedule(GameObject owner, bool clearSaved, string saveError);
    }

    // The upper composition root installs stateless adapters before scenes start.
    // Contracts contain no concrete player, map, persistence service, or screen types.
    public static class WorldProgressBindings
    {
        public static ISessionReadView Session { get; set; }
        public static IBiomeVisitProgress Visits { get; set; }
        public static IFieldEliteProgress Elites { get; set; }
        public static IEncounterProgression Encounters { get; set; }
        public static IRelicProgressView Relics { get; set; }
        public static IWorldTravelCommands Travel { get; set; }
        public static IBossRewardCommands Rewards { get; set; }
        public static IFinalVictoryPresentation Victory { get; set; }
    }
}
