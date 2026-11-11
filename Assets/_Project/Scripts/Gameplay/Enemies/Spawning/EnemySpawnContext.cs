using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Necrocis
{
    public enum EnemyEncounterRole { Field = 0, Boss = 1 }

    public interface IEnemyRuleCatalog
    {
        bool TryFindByName(string name, out EnemySpawnRuleConfig rule);
    }

    public interface IEnemyRuleCatalogProvider
    {
        IEnemyRuleCatalog GetEnemyRuleCatalog();
    }

    public interface IEnemyDeathSpawnSink
    {
        bool TryQueue(EnemyDeathSpawnRequest request);
    }

    public readonly struct EnemySpawnScope
    {
        public string RunId { get; }
        public string VisitId { get; }
        public int SceneHandle { get; }
        public int WorldInstanceId { get; }
        public Vector2Int Chunk { get; }
        public bool IsSpecified => WorldInstanceId != 0 && !string.IsNullOrEmpty(VisitId);

        public EnemySpawnScope(string runId, string visitId, int sceneHandle, int worldInstanceId, Vector2Int chunk)
        {
            RunId = runId; VisitId = visitId; SceneHandle = sceneHandle;
            WorldInstanceId = worldInstanceId; Chunk = chunk;
        }

        public EnemySpawnScope WithChunk(Vector2Int chunk)
            => new EnemySpawnScope(RunId, VisitId, SceneHandle, WorldInstanceId, chunk);
    }

    // Balance is unique to this actor. Reuse the scope/services, never an actor's mutable Balance.
    public readonly struct EnemySpawnContext
    {
        public MonsterBalanceRuntime Balance { get; }
        public IEnemyWorldQueries WorldQueries { get; }
        public EnemyEncounterRole Role { get; }
        public bool IgnoreEncounterMovementRestrictions { get; }
        public IEnemyRuleCatalog Rules { get; }
        public IEnemyDeathSpawnSink DeathSpawns { get; }
        public EnemySpawnScope Scope { get; }

        public EnemySpawnContext(MonsterBalanceRuntime balance, IEnemyWorldQueries worldQueries,
            EnemyEncounterRole role = EnemyEncounterRole.Field, bool ignoreEncounterMovementRestrictions = false,
            IEnemyRuleCatalog rules = null, IEnemyDeathSpawnSink deathSpawns = null, EnemySpawnScope scope = default)
        {
            Balance = balance; WorldQueries = worldQueries; Role = role;
            IgnoreEncounterMovementRestrictions = ignoreEncounterMovementRestrictions;
            Rules = rules; DeathSpawns = deathSpawns; Scope = scope;
        }
    }

    // A copied death result, not ownership of the original actor's future lifetime.
    public sealed class EnemyDeathSpawnRequest
    {
        public int SourceInstanceId { get; }
        public uint SourceGeneration { get; }
        public EnemySpawnContext SourceContext { get; }
        public EnemySpawnRuleConfig ChildRule { get; }
        public Vector3 Position { get; }
        public int Count { get; }
        public Sprite[] Frames { get; }
        public float FrameInterval { get; }
        public float Scale { get; }
        public int SortingOrder { get; }
        public string SourceName { get; }

        public EnemyDeathSpawnRequest(int sourceInstanceId, uint sourceGeneration, EnemySpawnContext sourceContext,
            EnemySpawnRuleConfig childRule, Vector3 position, int count, Sprite[] frames,
            float frameInterval, float scale, int sortingOrder, string sourceName = null)
        {
            SourceInstanceId = sourceInstanceId; SourceGeneration = sourceGeneration; SourceContext = sourceContext;
            ChildRule = CopyRule(childRule); Position = position; Count = count;
            Frames = frames != null ? (Sprite[])frames.Clone() : Array.Empty<Sprite>();
            FrameInterval = frameInterval; Scale = scale; SortingOrder = sortingOrder;
            SourceName = sourceName ?? childRule.name;
        }

        private static EnemySpawnRuleConfig CopyRule(EnemySpawnRuleConfig source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            EnemySpawnRuleConfig copy = source.CreateRuntimeCopy();
            copy.allowedRegions = source.allowedRegions != null ? new List<int>(source.allowedRegions) : null;
            copy.additionalBaseStats = source.additionalBaseStats != null ? new List<CharacterStatValue>(source.additionalBaseStats) : null;
            foreach (FieldInfo field in typeof(EnemySpawnRuleConfig).GetFields(BindingFlags.Public | BindingFlags.Instance))
                if (field.GetValue(source) is Array array) field.SetValue(copy, array.Clone());
            return copy;
        }
    }
}
