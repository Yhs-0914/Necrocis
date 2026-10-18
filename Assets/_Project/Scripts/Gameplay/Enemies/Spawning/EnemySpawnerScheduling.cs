using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public interface IEnemySpawnerSchedule
    {
        void Register(EnemySpawner spawner);
        void Unregister(EnemySpawner spawner);
    }

    public partial class EnemySpawner
    {
        private IEnemySpawnerSchedule schedule;
        private bool standaloneSchedule=true;
        public float NextEvaluationTime => nextEvaluationTime;
        public Vector3 AnchorPosition => anchorPosition;
        public float ActivationRadius => activationRadiusOverride >= 0 ? activationRadiusOverride : config != null ? config.activationRadius : -1f;
        public bool IsConfigured => config != null;

        public void BindSchedule(IEnemySpawnerSchedule value)
        {
            if (ReferenceEquals(schedule, value)) return;
            schedule?.Unregister(this);
            schedule = value;
            if (isActiveAndEnabled && config != null) schedule?.Register(this);
        }

        public bool ParkForReuse()
        {
            uint version=++configurationVersion;
            BindSchedule(null);
            ClearSpawnedEnemies();
            if(version!=configurationVersion)return false;
            config=null;
            stateStore=null;
            playerTransform=null;
            spawnParent=null;
            initializedWave=false;
            nextSpawnTime=nextEvaluationTime=0;
            return true;
        }

        private void OnEnable()
        {
            if (config == null) return;
            if (schedule == null) { if(standaloneSchedule)BindSchedule(EnemySpawnerClock.Default); }
            else schedule.Register(this);
        }
    }

    // Compatibility for authored/legacy spawners outside a chunk owner. Current maps bind their
    // own chunk schedule, so this clock does not traverse their placements.
    internal sealed class EnemySpawnerClock : MonoBehaviour, IEnemySpawnerSchedule
    {
        private static EnemySpawnerClock instance;
        private readonly HashSet<EnemySpawner> members = new HashSet<EnemySpawner>();
        private readonly List<EnemySpawner> ordered = new List<EnemySpawner>();
        private readonly List<EnemySpawner> current = new List<EnemySpawner>();
        internal static EnemySpawnerClock Default
        {
            get
            {
                if (instance == null) instance = new GameObject("EnemySpawnerClock").AddComponent<EnemySpawnerClock>();
                return instance;
            }
        }

        public void Register(EnemySpawner spawner)
        {
            if (members.Add(spawner))
            {
                ordered.Add(spawner);
                ordered.Sort((a,b)=>string.CompareOrdinal(a.Identity.Key,b.Identity.Key));
            }
            enabled = true;
        }
        public void Unregister(EnemySpawner spawner) { members.Remove(spawner); ordered.Remove(spawner); if (members.Count == 0) enabled = false; }
        private void Update()
        {
            current.Clear();
            current.AddRange(ordered);
            Transform player = PlayerWorldBindings.ResolvePlayerTransform();
            float now = Time.time;
            foreach (var spawner in current)
                if (spawner != null && spawner.isActiveAndEnabled && members.Contains(spawner))
                    spawner.EvaluateSpawn(now, player);
            current.Clear();
        }
    }
}
