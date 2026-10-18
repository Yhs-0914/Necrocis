using System;
using UnityEngine;
using UnityEngine.Events;

namespace Necrocis
{
    public static class WorldProgressAdapters
    {
        private sealed class SessionView : ISessionReadView
        {
            public GameDifficulty ActiveDifficulty => SaveService.ActiveDifficulty;
            public bool IsRestorePending => SaveService.IsRestorePending;
            public bool HasActiveSession => SaveService.HasActiveSession;
            public string ActiveRunId => SaveService.ActiveRunId;
        }

        private sealed class Visits : IBiomeVisitProgress
        {
            public MonsterVisitContext EnterLoadedBiome(BiomeType biome, int sceneHandle)
                => SaveService.EnterLoadedBiome(biome, sceneHandle);
            public int GetOrCreateSeed(BiomeType biome) => SaveService.GetOrCreateBiomeSeed(biome);
            public BiomeElitePlan GetElitePlan(BiomeType biome) => SaveService.GetBiomeElitePlan(biome);
            public bool TryStoreElitePlan(BiomeElitePlan plan, out string error)
                => SaveService.TryStoreBiomeElitePlan(plan, out error);
        }

        private sealed class Elites : IFieldEliteProgress
        {
            public string CurrentRunId => SaveService.ActiveRunId;
            public bool IsDefeated(string spawnId) => SaveService.IsBiomeEliteDefeated(spawnId);
            public bool TryRecordDefeat(string runId, string spawnId, out bool fresh, out string error)
                => SaveService.TryRecordBiomeEliteDefeat(runId, spawnId, out fresh, out error);
        }

        private sealed class Encounters : IEncounterProgression
        {
            public void EnterMidBoss() => GameManager.Instance?.SetGameState(GameState.InBossRoom);
            public void CompleteMidBoss(BiomeType? biome)
            {
                if (GameManager.Instance == null) return;
                if (biome.HasValue) GameManager.Instance.CollectRelic(biome.Value);
                GameManager.Instance.SetGameState(GameState.InBiome);
            }
            public void EnterFinalBoss() => GameManager.Instance?.SetGameState(GameState.InFinalBoss);
            public bool TryCompleteFinalBoss(out string error) => SaveService.TryMarkFinalBossDefeated(out error);
        }

        private sealed class Relics : IRelicProgressView
        {
            public bool IsAvailable => GameManager.Instance != null;
            public bool HasAll => GameManager.Instance.HasAllRelics;
            public int Count => GameManager.Instance.CollectedRelicCount;
            public bool HasRelic(BiomeType biome) => GameManager.Instance.HasRelic(biome);
            public IDisposable SubscribeAllCollected(Action callback)
            {
                var manager = GameManager.Instance;
                if (manager == null) return null;
                return new RelicSubscription(manager.OnAllRelicsCollected, callback);
            }
        }

        private sealed class RelicSubscription : IDisposable
        {
            private UnityEvent source;
            private readonly UnityAction callback;
            public RelicSubscription(UnityEvent source, Action callback)
            {
                this.source = source;
                this.callback = callback.Invoke;
                source.AddListener(this.callback);
            }
            public void Dispose()
            {
                if (source == null) return;
                source.RemoveListener(callback);
                source = null;
            }
        }

        private sealed class Travel : IWorldTravelCommands
        {
            public bool HasLoader => SceneLoader.Instance != null;
            public void NotifyBiomeEntered(BiomeType biome) => GameManager.Instance?.EnterBiome(biome);
            public void NotifyHubReturned() => GameManager.Instance?.ReturnToHub();
            public void LoadBiome(BiomeType biome) => SceneLoader.Instance.LoadBiome(biome);
            public void ReturnToHub() => SceneLoader.Instance.ReturnToHub();
            public bool LoadFinalBoss() => SceneLoader.Instance != null && SceneLoader.Instance.LoadFinalBoss();
        }

        private sealed class Rewards : IBossRewardCommands
        {
            public bool TrySpawnBonus(GameObject owner, Vector3 position)
            {
                var spawner = owner.GetComponent<WorldItemSpawner>();
                if (spawner == null) spawner = owner.AddComponent<WorldItemSpawner>();
                return spawner.TrySpawnSingleRandomItemAt(position);
            }
        }

        private sealed class Victory : IFinalVictoryPresentation
        {
            public void Schedule(GameObject owner, bool clearSaved, string saveError)
            {
                var prompt = owner.GetComponent<FinalBossVictoryPrompt>();
                if (prompt == null) prompt = owner.AddComponent<FinalBossVictoryPrompt>();
                prompt.Schedule(clearSaved, saveError);
            }
        }

        public static void Register()
        {
            WorldProgressBindings.Session = new SessionView();
            WorldProgressBindings.Visits = new Visits();
            WorldProgressBindings.Elites = new Elites();
            WorldProgressBindings.Encounters = new Encounters();
            WorldProgressBindings.Relics = new Relics();
            WorldProgressBindings.Travel = new Travel();
            WorldProgressBindings.Rewards = new Rewards();
            WorldProgressBindings.Victory = new Victory();
            EnemyDefeatBindings.ResolvedExperience = LevelUpManager.AddResolvedEnemyKillExp;
            EnemyDefeatBindings.LegacyExperience = LevelUpManager.AddEnemyKillExp;
        }
    }
}
