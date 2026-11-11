using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    // Scene/visit-owned, intentionally absent from Save DTOs. Chunk unload suspends a ticket.
    [DisallowMultipleComponent]
    public sealed class DeathSpawnCoordinator : MonoBehaviour, IEnemyDeathSpawnSink
    {
        private BiomeManager biome;
        private string runId, visitId;
        private readonly Dictionary<string, DeathSpawnTicket> pending = new Dictionary<string, DeathSpawnTicket>(StringComparer.Ordinal);
        private readonly HashSet<string> completed = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<DeathSpawnTicket> updateBuffer = new List<DeathSpawnTicket>();
        private readonly List<GameObject> visuals = new List<GameObject>();
        public int PendingCount => pending.Count;
        public int ExecutedCount => completed.Count;

        public static DeathSpawnCoordinator ForBiome(BiomeManager biome)
        {
            if (biome == null) return null;
            var coordinator = biome.GetComponent<DeathSpawnCoordinator>() ?? biome.gameObject.AddComponent<DeathSpawnCoordinator>();
            coordinator.Bind(biome);
            return coordinator;
        }

        private void Bind(BiomeManager source)
        {
            MonsterVisitContext visit = source.MonsterVisit;
            if (biome == source && runId == visit.RunId && visitId == visit.VisitId) return;
            CancelAll();
            biome = source; runId = visit.RunId; visitId = visit.VisitId;
        }

        public bool TryQueue(EnemyDeathSpawnRequest request)
        {
            if (request == null || request.SourceContext.WorldQueries == null
                || !isActiveAndEnabled || !IsCurrent(request.SourceContext.Scope)) return false;
            string key = request.SourceInstanceId + ":" + request.SourceGeneration;
            if (pending.ContainsKey(key) || completed.Contains(key)) return true;
            Vector2Int cell = request.SourceContext.WorldQueries.WorldToCell(request.Position);
            Vector2Int chunk = biome.GridToChunk(cell.x, cell.y);
            EnemySpawnScope scope = request.SourceContext.Scope.WithChunk(chunk);
            int count = Mathf.Max(0, request.Count);
            var children = new EnemySpawnContext[count];
            for (int i = 0; i < count; i++)
            {
                // A fresh mutable balance for every child, frozen before the original actor is reused.
                children[i] = EnemySpawnContextFactory.CaptureForBiome(biome, request.ChildRule, request.Position);
            }
            var ticket = new DeathSpawnTicket(key, request, scope, children, Time.time);
            pending.Add(key, ticket);
            Advance(ticket);
            return true;
        }

        private bool IsCurrent(EnemySpawnScope scope)
        {
            return biome != null && biome.gameObject.scene.isLoaded && BiomeManager.Active == biome
                && scope.IsSpecified && scope.WorldInstanceId == biome.GetInstanceID()
                && scope.SceneHandle == biome.gameObject.scene.handle && scope.RunId == runId && scope.VisitId == visitId
                && biome.MonsterVisit.RunId == runId && biome.MonsterVisit.VisitId == visitId
                && WorldProgressBindings.Session.ActiveRunId == runId;
        }

        private void Update()
        {
            if (biome == null || biome.MonsterVisit.RunId != runId || biome.MonsterVisit.VisitId != visitId
                || WorldProgressBindings.Session.ActiveRunId != runId || BiomeManager.Active != biome)
            {
                CancelAll();
                return;
            }
            updateBuffer.Clear();
            foreach (DeathSpawnTicket ticket in pending.Values) updateBuffer.Add(ticket);
            for (int i = 0; i < updateBuffer.Count; i++) Advance(updateBuffer[i]);
            updateBuffer.Clear();
            visuals.RemoveAll(value => value == null);
        }

        private void Advance(DeathSpawnTicket ticket)
        {
            if (ticket.Executed || !pending.ContainsKey(ticket.Key)) return;
            if (!IsCurrent(ticket.Scope)) { CancelAll(); return; }
            if (!biome.TryGetEnemySpawnParent(ticket.Scope.Chunk, out Transform parent))
            {
                ticket.Deferred = true;
                DestroyVisual(ticket);
                return;
            }
            if (ticket.Deferred && Time.time >= ticket.DueAt) ticket.VisualCompleted = true;
            if (ticket.VisualCompleted)
            {
                Execute(ticket, parent);
                return;
            }
            if (ticket.Visual == null) CreateVisual(ticket);
        }

        private void CreateVisual(DeathSpawnTicket ticket)
        {
            EnemyDeathSpawnRequest request = ticket.Request;
            Sprite[] frames = request.Frames;
            float interval = request.FrameInterval;
            if (ticket.Deferred)
            {
                float remaining = Mathf.Max(0f, ticket.DueAt - Time.time);
                int first = interval > 0f ? Mathf.Clamp(Mathf.FloorToInt((Time.time - ticket.StartedAt) / interval), 0, frames.Length - 1) : 0;
                var rest = new Sprite[frames.Length - first];
                Array.Copy(frames, first, rest, 0, rest.Length);
                frames = rest;
                interval = rest.Length > 0 ? remaining / rest.Length : interval;
            }
            var visual = new GameObject("SplitVFX_" + request.SourceName);
            visual.transform.position = request.Position + new Vector3(0f, .8f, 0f);
            visual.transform.localScale = Vector3.one * request.Scale;
            visual.transform.SetParent(transform, true);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = frames[0];
            renderer.sortingOrder = request.SortingOrder;
            renderer.color = new Color(.5f, .2f, 1f, .9f);
            visual.AddComponent<Billboard>().enabled = true;
            var animator = visual.AddComponent<SpriteFrameAnimator>();
            ticket.Visual = visual;
            visuals.Add(visual);
            animator.PlayOneShot(frames, interval, () =>
            {
                // While loaded the original animation callback remains authoritative.
                if (!pending.ContainsKey(ticket.Key) || ticket.Executed || !IsCurrent(ticket.Scope)) return;
                ticket.VisualCompleted = true;
                Advance(ticket);
            });
        }

        private void Execute(DeathSpawnTicket ticket, Transform parent)
        {
            if (ticket.Executed || !IsCurrent(ticket.Scope)) return;
            ticket.Executed = true;
            completed.Add(ticket.Key); // Claim before Configure can invoke another gameplay callback.
            EnemySpawnRuleConfig rule = ticket.Request.ChildRule;
            int poolId = EnemyController.GetPoolArchetypeId(rule);
            try
            {
                while (ticket.NextChild < ticket.Children.Length)
                {
                    if (!IsCurrent(ticket.Scope)) break;
                    int index = ticket.NextChild++;
                    Vector3 position = ticket.Request.Position;
                    // Keep the old random draw interleaved with each child's Configure/VFX.
                    Vector2 offset = UnityEngine.Random.insideUnitCircle * 1.5f;
                    position += new Vector3(offset.x, 0f, offset.y);
                    IEnemyWorldQueries world = ticket.Children[index].WorldQueries;
                    Vector2Int cell = world.WorldToCell(position);
                    if (!world.IsValidCell(cell) || !world.IsWalkableCell(cell)) position = ticket.Request.Position;
                    position.y = world.GetGroundHeight(position) + rule.heightOffset;
                    EnemyController child = EnemyController.Acquire(parent, rule.name + "_Split_" + index, poolId);
                    try { child.ConfigureWithContext(null, rule, position, position, ticket.Children[index]);
                        EnemyVisitStateStore.ForBiome(biome).RegisterLooseActor(child,ticket.Scope.Chunk); }
                    catch { child.ReleaseToPool(); throw; }
                }
            }
            finally
            {
                pending.Remove(ticket.Key);
                if (ticket.Visual != null)
                {
                    var fade = ticket.Visual.AddComponent<SplitVfxFadeOut>();
                    fade.Init(ticket.Visual.GetComponent<SpriteRenderer>(), .3f);
                    ticket.Visual = null;
                }
            }
        }

        private void DestroyVisual(DeathSpawnTicket ticket)
        {
            if (ticket.Visual == null) return;
            SpriteFrameAnimator animator = ticket.Visual.GetComponent<SpriteFrameAnimator>();
            if (animator != null) animator.Stop();
            ticket.Visual.SetActive(false);
            Destroy(ticket.Visual);
            ticket.Visual = null;
        }

        public void CancelAll()
        {
            foreach (GameObject visual in visuals)
                if (visual != null) { visual.SetActive(false); Destroy(visual); }
            visuals.Clear(); pending.Clear(); completed.Clear(); updateBuffer.Clear();
        }

        private void OnDisable() => CancelAll();
        private void OnDestroy() => CancelAll();
    }
}
