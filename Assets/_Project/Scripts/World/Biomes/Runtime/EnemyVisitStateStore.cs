using System;
using System.Collections.Generic;
using UnityEngine;
namespace Necrocis
{
    // Current world instance/visit only. Nothing is written into RunSaveData.
    [DisallowMultipleComponent]
    public sealed partial class EnemyVisitStateStore : MonoBehaviour,IEnemySpawnerStateStore
    {
        private BiomeManager biome;
        private MonsterVisitContext visit;
        private string runId;
        private bool cancelled;
        private readonly Dictionary<EnemySpawnerIdentity,EnemySpawnerRuntimeSnapshot> groups=new Dictionary<EnemySpawnerIdentity,EnemySpawnerRuntimeSnapshot>();
        public int StoredGroups=>groups.Count;
        public bool IsAvailable=>this!=null&&isActiveAndEnabled&&!cancelled&&biome!=null
            &&biome.MonsterVisit.RunId==visit.RunId&&biome.MonsterVisit.VisitId==visit.VisitId
            &&biome.MonsterVisit.Biome==visit.Biome&&biome.MonsterVisit.Difficulty==visit.Difficulty&&biome.MonsterVisit.Stage==visit.Stage&&string.Equals(runId,WorldProgressBindings.Session.ActiveRunId,StringComparison.Ordinal);
        public static EnemyVisitStateStore ForBiome(BiomeManager biome)
        {
            if(biome==null)return null;var store=biome.GetComponent<EnemyVisitStateStore>();
            if(store==null){store=biome.gameObject.AddComponent<EnemyVisitStateStore>();store.biome=biome;store.visit=biome.MonsterVisit;store.runId=WorldProgressBindings.Session.ActiveRunId;}
            return store;
        }
        public bool TryGet(EnemySpawnerIdentity id,out EnemySpawnerRuntimeSnapshot state)
        {state=null;return IsAvailable&&groups.TryGetValue(id,out state);}
        public void Save(EnemySpawnerIdentity id,EnemySpawnerRuntimeSnapshot state)
        {
            if(!IsAvailable)throw new InvalidOperationException("Enemy world visit is no longer available.");
            if(groups.ContainsKey(id))throw new InvalidOperationException("An enemy placement already has a stored group.");groups.Add(id,state);
        }
        public bool CommitRestore(EnemySpawnerIdentity id,EnemySpawnerRuntimeSnapshot expected)
            =>IsAvailable&&groups.TryGetValue(id,out var current)&&ReferenceEquals(current,expected)&&groups.Remove(id);
        private void Update(){if(!cancelled&&!IsAvailable)CancelStoredState();}
        private void CancelStoredState(){cancelled=true;foreach(var state in groups.Values)state.Retire();groups.Clear();ClearLooseActors();}
        private void OnDisable()=>CancelStoredState();
        private void OnDestroy()=>CancelStoredState();
    }
}
