using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public sealed class EnemySpawnerRuntimeSnapshot
    {
        public readonly EnemyRuntimeSnapshot[] Living;
        public readonly bool Initialized;
        public readonly float SpawnRemaining, EvaluationRemaining;
        public readonly uint RandomState;

        public EnemySpawnerRuntimeSnapshot(EnemyRuntimeSnapshot[] living, bool initialized,
            float spawn, float evaluation, uint random)
        {
            Living = living;
            Initialized = initialized;
            SpawnRemaining = spawn;
            EvaluationRemaining = evaluation;
            RandomState = random;
        }

        public void Retire() { foreach (var actor in Living) actor.Retire(); }
    }

    public interface IEnemySuspensionOwner : IEnemySpawnOwner
    {
        bool TrySuspend(EnemyController actor);
    }

    public interface IEnemySpawnerStateStore
    {
        bool IsAvailable { get; }
        bool TryGet(EnemySpawnerIdentity id, out EnemySpawnerRuntimeSnapshot state);
        void Save(EnemySpawnerIdentity id, EnemySpawnerRuntimeSnapshot state);
        bool CommitRestore(EnemySpawnerIdentity id, EnemySpawnerRuntimeSnapshot expected);
    }

    public partial class EnemySpawner
    {
        private IEnemySpawnerStateStore stateStore;
        private bool suspendingGroup, restoringGroup;

        public void BindStateStore(IEnemySpawnerStateStore store) => stateStore = store;

        public bool SuspendSpawnedEnemies()
        {
            var store = stateStore;
            if (suspendingGroup || restoringGroup || store == null || !store.IsAvailable) return false;
            var identity = Identity;
            if (store.TryGet(identity, out _)) return true;
            // An untouched placement has no lifetime or timer to freeze.
            if (!initializedWave && activeEnemies.Count == 0 && spawnRandom.State == identity.StableHash) return true;

            uint version = configurationVersion;
            var actors = activeEnemies.ToArray();
            var snapshots = new List<EnemyRuntimeSnapshot>();
            var records = new Dictionary<EnemyController, EnemyRuntimeSnapshot>();
            var generations = new Dictionary<EnemyController, uint>();
            bool hasCorpse = false;
            foreach (var actor in actors)
            {
                if (actor == null) continue;
                hasCorpse |= actor.IsDead;
                if (!actor.IsDead && actor.gameObject.activeInHierarchy)
                {
                    var state = actor.CaptureSuspension();
                    snapshots.Add(state);
                    records.Add(actor, state);
                }
            }

            // A corpse returned during unload starts the original respawn cooldown.
            float spawnRemaining = hasCorpse && initializedWave
                ? GetDifficultyRespawnCooldown(GetActiveWorldBalance())
                : Mathf.Max(0, nextSpawnTime - Time.time);
            var saved = new EnemySpawnerRuntimeSnapshot(snapshots.ToArray(), initializedWave,
                spawnRemaining, Mathf.Max(0, nextEvaluationTime - Time.time), spawnRandom.State);
            suspendingGroup = true;
            try
            {
                // The store owns the snapshots before cleanup invokes any subscriber.
                store.Save(identity, saved);
                foreach (var pair in records) pair.Key.PrepareSuspension(pair.Value);
                foreach (var actor in actors) if (actor != null) generations[actor] = actor.SpawnGeneration;
                List<Exception> failures = null;
                foreach (var actor in actors)
                {
                    if (actor == null || actor.SpawnGeneration != generations[actor]) continue;
                    try { actor.ReleaseToPool(); }
                    catch (Exception error)
                    {
                        if (failures == null) failures = new List<Exception>();
                        failures.Add(error);
                        // Preserve the saved lifetime even if a custom cleanup subscriber fails.
                        if (actor != null && actor.SpawnGeneration == generations[actor]) actor.gameObject.SetActive(false);
                    }
                }
                if (version == configurationVersion) activeEnemies.Clear();
                if (failures != null) throw new AggregateException("Enemy suspension cleanup failed; snapshots retained.", failures);
                return true;
            }
            finally { suspendingGroup = false; }
        }

        private bool TryResumeStoredGroup()
        {
            var store = stateStore;
            var identity = Identity;
            if (store == null || !store.IsAvailable || !store.TryGet(identity, out var saved)) return false;
            var prepared = new List<EnemyController>();
            uint version = configurationVersion;
            bool committed = false;
            restoringGroup = true;
            try
            {
                Transform parent = transform.parent != null ? transform.parent : spawnParent;
                if (parent == null) return true;
                foreach (var snapshot in saved.Living)
                {
                    var context=EnemySpawnBindings.CaptureForSurface(EnemyWorldBindings.Surface, config, snapshot.SavedPosition);
                    var actor = EnemyController.Acquire(parent, config.name + "_Resume_" + snapshot.LogicalId, enemyPoolArchetypeId);
                    prepared.Add(actor);
                    actor.PrepareSuspendedRestore(this, snapshot, context);
                    if (version != configurationVersion || !actor.IsPreparedSnapshot(snapshot))
                        throw new InvalidOperationException("Spawner changed while preparing enemies.");
                }
                if (!store.CommitRestore(identity, saved)) throw new InvalidOperationException("Enemy visit changed during restore.");
                committed = true;
                // Transfer ownership before activation/StatChanged callbacks. Those callbacks may
                // legitimately kill or return an actor; they must see the original owner's list.
                activeEnemies.AddRange(prepared);
                initializedWave = saved.Initialized;
                nextSpawnTime = Time.time + saved.SpawnRemaining;
                nextEvaluationTime = Time.time + saved.EvaluationRemaining;
                spawnRandom.State = saved.RandomState;
                List<Exception> publishFailures = null;
                for (int i = 0; i < prepared.Count; i++)
                {
                    var actor = prepared[i];
                    if (version != configurationVersion)
                    {
                        if (actor.IsPreparedSnapshot(saved.Living[i]))
                        {
                            actor.DiscardPreparedRestore();
                            saved.Living[i].Retire();
                        }
                        continue;
                    }
                    try { if (!actor.PublishSuspendedRestore()) activeEnemies.Remove(actor); }
                    catch(Exception error)
                    {
                        if(publishFailures==null)publishFailures=new List<Exception>();
                        publishFailures.Add(error);
                    }
                }
                if(publishFailures!=null)throw new AggregateException("Enemy publication callbacks failed; other living actors were still restored.",publishFailures);
                return true;
            }
            catch
            {
                // Before publication the original record remains authoritative. After publication,
                // deaths and rewards are real events and must never be rolled back or replayed.
                for (int i = 0; i < prepared.Count; i++)
                {
                    var actor = prepared[i];
                    if (actor == null || !actor.IsPreparedSnapshot(saved.Living[i])) continue;
                    actor.DiscardPreparedRestore();
                    if (committed) { activeEnemies.Remove(actor); saved.Living[i].Retire(); }
                }
                throw;
            }
            finally { restoringGroup = false; }
        }
    }
}
