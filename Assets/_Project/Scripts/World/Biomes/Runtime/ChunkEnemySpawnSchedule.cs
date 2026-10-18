using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    // Only registered, loaded chunk placements are held here. Each selected chunk has a due
    // heap; sleeping chunks have no per-spawner frame callbacks or repeated distance checks.
    [DisallowMultipleComponent]
    public sealed class ChunkEnemySpawnSchedule : MonoBehaviour, IEnemySpawnerSchedule
    {
        private sealed class Entry
        {
            internal EnemySpawner Actor;
            internal Bucket Bucket;
            internal float Due, Radius;
            internal int HeapIndex = -1;
            internal long Serial;
        }

        private sealed class Bucket
        {
            internal readonly List<Entry> Members = new List<Entry>();
            internal readonly List<Entry> Heap = new List<Entry>();
            internal Vector3 Min, Max;
            internal float Radius;
            internal bool Selected;
        }

        private BiomeManager biome;
        private readonly Dictionary<EnemySpawner, Entry> entries = new Dictionary<EnemySpawner, Entry>();
        private readonly Dictionary<Vector2Int, Bucket> chunks = new Dictionary<Vector2Int, Bucket>();
        private readonly List<Bucket> buckets = new List<Bucket>();
        private long serial;
        private bool ticking;
        private readonly List<Entry> deferred = new List<Entry>();
        public int RegisteredCount => entries.Count;
        public int LastEvaluated { get; private set; }
        public int LastSelectedChunks { get; private set; }

        public static ChunkEnemySpawnSchedule ForBiome(BiomeManager source)
        {
            var clock = source.GetComponent<ChunkEnemySpawnSchedule>() ?? source.gameObject.AddComponent<ChunkEnemySpawnSchedule>();
            clock.biome = source;
            return clock;
        }

        public void Register(EnemySpawner spawner)
        {
            if (spawner == null || !spawner.IsConfigured || entries.ContainsKey(spawner) || biome == null) return;
            Vector2Int cell = biome.WorldToGrid(spawner.AnchorPosition);
            Vector2Int coordinate = biome.GridToChunk(cell.x, cell.y);
            if (!chunks.TryGetValue(coordinate, out var bucket))
            {
                var min = biome.GridToWorld(coordinate.x * biome.ChunkSize, coordinate.y * biome.ChunkSize);
                bucket = new Bucket { Min = min, Max = min + new Vector3(biome.ChunkSize * biome.TileSize, 0, biome.ChunkSize * biome.TileSize) };
                // Include cell-center and edge conventions without missing a placement on the boundary.
                bucket.Min -= new Vector3(biome.TileSize, 0, biome.TileSize);
                chunks.Add(coordinate, bucket);
                buckets.Add(bucket);
            }
            var entry = new Entry { Actor = spawner, Bucket = bucket, Due = spawner.NextEvaluationTime,
                Radius = Mathf.Max(0, spawner.ActivationRadius), Serial = ++serial };
            entries.Add(spawner, entry);
            bucket.Members.Add(entry);
            bucket.Radius = Mathf.Max(bucket.Radius, entry.Radius);
            // A new or departing placement must get one due evaluation even if the bucket is asleep.
            Push(bucket, entry);
        }

        public void Unregister(EnemySpawner spawner)
        {
            if (!entries.TryGetValue(spawner, out var entry)) return;
            entries.Remove(spawner);
            Bucket bucket = entry.Bucket;
            Remove(bucket, entry);
            bucket.Members.Remove(entry);
            if (entry.Radius >= bucket.Radius)
            {
                bucket.Radius = 0;
                foreach (var remaining in bucket.Members) bucket.Radius = Mathf.Max(bucket.Radius, remaining.Radius);
            }
            entry.Actor = null;
        }

        private void Update() => Tick(Time.time, PlayerWorldBindings.ResolvePlayerTransform());

        public void Tick(float now, Transform player)
        {
            if (ticking) return;
            LastEvaluated = LastSelectedChunks = 0;
            if (player == null || biome == null) return;
            ticking = true;
            try
            {
                List<Exception> failures = null;
                Vector3 position = player.position;
                int count = buckets.Count;
                for (int i = 0; i < count; i++)
                {
                    Bucket bucket = buckets[i];
                    if (bucket.Members.Count == 0) continue;
                    float dx = Mathf.Max(0, Mathf.Max(bucket.Min.x - position.x, position.x - bucket.Max.x));
                    float dz = Mathf.Max(0, Mathf.Max(bucket.Min.z - position.z, position.z - bucket.Max.z));
                    bool selected = dx * dx + dz * dz <= bucket.Radius * bucket.Radius;
                    if (selected)
                    {
                        LastSelectedChunks++;
                        if (!bucket.Selected)
                            foreach (var entry in bucket.Members)
                                if (entry.HeapIndex < 0) { entry.Due = entry.Actor.NextEvaluationTime; Push(bucket, entry); }
                    }
                    bucket.Selected = selected;
                    while (bucket.Heap.Count > 0 && bucket.Heap[0].Due <= now)
                    {
                        Entry entry = bucket.Heap[0];
                        Remove(bucket, entry);
                        EnemySpawner actor = entry.Actor;
                        if (!IsCurrent(entry) || !actor.isActiveAndEnabled) continue;
                        LastEvaluated++;
                        try { actor.EvaluateSpawn(now, player); }
                        catch (Exception error)
                        {
                            if (failures == null) failures = new List<Exception>();
                            failures.Add(error);
                        }
                        finally
                        {
                            if (bucket.Selected && IsCurrent(entry) && actor.isActiveAndEnabled)
                            {
                                entry.Due = actor.NextEvaluationTime;
                                // A restored zero remainder is due on the next frame, as with
                                // the former MonoBehaviour Update, never twice in this Tick.
                                if (entry.Due <= now) deferred.Add(entry);
                                else Push(bucket, entry);
                            }
                        }
                    }
                }
                if (failures != null) throw new AggregateException("Spawner callbacks failed; due registrations retained.", failures);
            }
            finally
            {
                foreach (var entry in deferred)
                    if (entry.Bucket.Selected && IsCurrent(entry) && entry.Actor.isActiveAndEnabled) Push(entry.Bucket, entry);
                deferred.Clear();
                ticking = false;
            }
        }

        private bool IsCurrent(Entry entry) => entry.Actor != null && entries.TryGetValue(entry.Actor, out var current) && ReferenceEquals(entry, current);
        private static bool Before(Entry a, Entry b)
        {
            int due = a.Due.CompareTo(b.Due);
            if (due != 0) return due < 0;
            int key = string.CompareOrdinal(a.Actor.Identity.Key, b.Actor.Identity.Key);
            return key != 0 ? key < 0 : a.Serial < b.Serial;
        }
        private static void Push(Bucket bucket, Entry entry)
        {
            if (entry.HeapIndex >= 0) return;
            entry.HeapIndex = bucket.Heap.Count;
            bucket.Heap.Add(entry);
            Up(bucket, entry.HeapIndex);
        }
        private static void Swap(Bucket bucket, int a, int b)
        {
            Entry item = bucket.Heap[a]; bucket.Heap[a] = bucket.Heap[b]; bucket.Heap[b] = item;
            bucket.Heap[a].HeapIndex = a; bucket.Heap[b].HeapIndex = b;
        }
        private static void Up(Bucket bucket, int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (!Before(bucket.Heap[index], bucket.Heap[parent])) break;
                Swap(bucket, index, parent); index = parent;
            }
        }
        private static void Remove(Bucket bucket, Entry entry)
        {
            int index = entry.HeapIndex;
            if (index < 0) return;
            int last = bucket.Heap.Count - 1;
            if (index != last) Swap(bucket, index, last);
            bucket.Heap.RemoveAt(last); entry.HeapIndex = -1;
            if (index >= last) return;
            if (index > 0 && Before(bucket.Heap[index], bucket.Heap[(index - 1) / 2])) { Up(bucket, index); return; }
            while (true)
            {
                int child = index * 2 + 1;
                if (child >= bucket.Heap.Count) return;
                if (child + 1 < bucket.Heap.Count && Before(bucket.Heap[child + 1], bucket.Heap[child])) child++;
                if (!Before(bucket.Heap[child], bucket.Heap[index])) return;
                Swap(bucket, index, child); index = child;
            }
        }
    }
}
