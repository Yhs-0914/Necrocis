using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public sealed class EnemyPatternLifetime : MonoBehaviour
    {
        private EnemyController enemy;
        private uint generation;
        private string runId;
        private bool previousAiSuppressed;
        private bool running;
        private readonly List<(GameObject item, Action<GameObject> release)> owned = new List<(GameObject, Action<GameObject>)>();
        public bool IsCycleRunning => running;
        public uint Generation => generation;
        public int OwnedObjectCount => owned.Count;

        public void Bind(EnemyController source)
        {
            Cancel();
            enemy = source;
            generation = source.SpawnGeneration;
            runId = SaveService.ActiveRunId;
        }

        public bool IsCurrent(uint token) => enemy != null && enemy.gameObject.activeInHierarchy
            && !enemy.IsDead && enemy.SpawnGeneration == token && generation == token && runId == SaveService.ActiveRunId;

        public bool TryExecute(uint token, Action action)
        {
            if (!IsCurrent(token)) return false;
            action(); return true;
        }

        public void Own(GameObject item, Action<GameObject> release = null)
        {
            if (!IsCurrent(generation)) throw new InvalidOperationException("활성 바이옴 엘리트만 생성물을 소유할 수 있습니다.");
            if (item != null) owned.Add((item, release));
        }

        public bool TryStartCycle(IEnumerator cycle)
        {
            if (cycle == null || running || !IsCurrent(generation)) return false;
            previousAiSuppressed = enemy.IsAiSuppressed;
            enemy.SetAiSuppressed(true);
            running = true;
            StartCoroutine(Run(cycle, generation));
            return true;
        }

        public void ReleaseOwned(GameObject item)
        {
            for (int i = owned.Count - 1; i >= 0; i--)
            {
                var entry = owned[i];
                if (entry.item != item) continue;
                owned.RemoveAt(i);
                if (entry.item != null)
                {
                    if (entry.release != null) entry.release(entry.item);
                    else Destroy(entry.item);
                }
                return;
            }
        }

        private IEnumerator Run(IEnumerator cycle, uint token)
        {
            try
            {
                while (IsCurrent(token) && cycle.MoveNext()) yield return cycle.Current;
            }
            finally
            {
                (cycle as IDisposable)?.Dispose();
                if (enemy != null && enemy.SpawnGeneration == token) enemy.SetAiSuppressed(previousAiSuppressed);
                running = false;
            }
        }

        public void Cancel()
        {
            StopAllCoroutines();
            if (running && enemy != null) enemy.SetAiSuppressed(previousAiSuppressed);
            running = false;
            for (int i = owned.Count - 1; i >= 0; i--)
            {
                var entry = owned[i];
                if (entry.item == null) continue;
                if (entry.release != null) entry.release(entry.item);
                else Destroy(entry.item);
            }
            owned.Clear();
        }

        private void OnDisable() => Cancel();
    }
}
