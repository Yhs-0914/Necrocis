using System;
using System.Collections.Generic;
using UnityEngine;
namespace Necrocis
{
    public partial class EnemyVisitStateStore
    {
        private sealed class LooseEntry : IEnemySuspensionOwner
        {
            internal EnemyVisitStateStore Store;
            internal Vector2Int Chunk;
            internal EnemyController Active;
            internal EnemyRuntimeSnapshot Saved;
            internal bool Suspending;
            public bool TrySuspend(EnemyController actor)
            {
                if(Store==null||!Store.IsAvailable||actor!=Active||actor.IsDead||Saved!=null)return false;
                if (actor.HasPersistentPlayerAggro) { Store.KeepPursuerActive(actor); return true; }
                var saved=actor.CaptureSuspension();Saved=saved;Suspending=true;
                try{actor.ReleaseForSuspension(saved);Active=null;return true;}
                finally{Suspending=false;}
            }
            public void NotifyEnemyReleased(EnemyController actor)
            {
                if(actor!=Active)return;Active=null;
                if(!Suspending){Saved?.Retire();Saved=null;Store?.loose.Remove(this);}
            }
        }
        private readonly List<LooseEntry> loose=new List<LooseEntry>();
        private Transform pursuersRoot;
        private void KeepPursuerActive(EnemyController actor)
        {
            if (pursuersRoot == null)
            {
                pursuersRoot = new GameObject("PursuingEnemies").transform;
                pursuersRoot.SetParent(biome.transform, false);
            }
            actor.transform.SetParent(pursuersRoot, true);
        }

        public bool TryRetainPursuer(EnemyController actor, IEnemySpawnOwner previousOwner)
        {
            if (!IsAvailable || actor == null || !actor.HasPersistentPlayerAggro || actor.LogicalLife == null) return false;
            var cell = biome.WorldToGrid(actor.transform.position);
            var entry = new LooseEntry { Store = this, Chunk = biome.GridToChunk(cell.x, cell.y), Active = actor };
            if (!actor.TransferSpawnOwner(previousOwner, entry)) return false;
            loose.Add(entry);
            KeepPursuerActive(actor);
            return true;
        }
        public int StoredLooseActors {get{int n=0;foreach(var entry in loose)if(entry.Saved!=null)n++;return n;}}
        public void RegisterLooseActor(EnemyController actor,Vector2Int chunk)
        {
            if(!IsAvailable||actor==null||actor.IsDead)return;
            var entry=new LooseEntry{Store=this,Chunk=chunk,Active=actor};actor.BindLooseSuspensionOwner(entry);loose.Add(entry);
        }
        public bool HasStoredLooseChunk(Vector2Int chunk)
        {
            foreach (var entry in loose)
                if (entry.Chunk == chunk && entry.Saved != null) return true;
            return false;
        }
        public void ResumeLooseChunk(Vector2Int chunk,Transform parent)
        {
            if(!IsAvailable||parent==null)return;
            foreach(var entry in loose.ToArray())
            {
                if(entry.Chunk!=chunk||entry.Saved==null||entry.Active!=null)continue;
                var saved=entry.Saved;var actor=EnemyController.Acquire(parent,saved.Rule.name+"_ResumeSplit_"+saved.LogicalId,EnemyController.GetPoolArchetypeId(saved.Rule));
                entry.Suspending=true;
                try
                {
                    actor.PrepareSuspendedRestore(entry,saved,EnemySpawnBindings.CaptureForSurface(EnemyWorldBindings.Surface,saved.Rule,saved.SavedPosition));
                    entry.Active=actor;
                    entry.Suspending=false;
                    entry.Saved=null; // Ownership transfers before activation callbacks can retire this actor.
                    actor.PublishSuspendedRestore();
                }
                catch
                {
                    if(actor.IsPreparedSnapshot(saved)) {actor.DiscardPreparedRestore();entry.Saved=saved;entry.Active=null;}
                    throw;
                }
                finally{entry.Suspending=false;}
            }
        }
        private void ClearLooseActors()
        {
            foreach (var entry in loose.ToArray())
            {
                entry.Saved?.Retire();
                if (entry.Active != null) entry.Active.ReleaseToPool();
            }
            loose.Clear();
        }
    }
}
