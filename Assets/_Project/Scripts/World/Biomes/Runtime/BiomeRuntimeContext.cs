using System;
using System.Collections;
using ProceduralMap;
using UnityEngine;

namespace Necrocis
{
    public enum BiomeReadiness { WaitingForTerrain, TerrainReady, PlayerBound, EncounterReady, Cancelled }

    // Scene lifetime and the ordered terrain/player/encounter handoff have one owner.
    // MapGenerator remains a terrain producer and knows neither saves nor players.
    [DisallowMultipleComponent]
    public sealed class BiomeRuntimeContext : MonoBehaviour, IPlayerTerrainQueries
    {
        private MapGenerator map;
        private ProceduralBiomeBridge bridge;
        private UnityEngine.Object boundPlayer;
        private bool encounterPrepared, cancelled;
        public bool IsCancelled => cancelled || map == null;
        public bool IsReady => !cancelled && map != null && map.IsReady;
        public BiomeReadiness Readiness => IsCancelled ? BiomeReadiness.Cancelled
            : !IsReady ? BiomeReadiness.WaitingForTerrain
            : boundPlayer == null || WorldProgressBindings.Session.IsRestorePending ? BiomeReadiness.TerrainReady
            : encounterPrepared ? BiomeReadiness.EncounterReady : BiomeReadiness.PlayerBound;

        public static BiomeRuntimeContext Ensure(MapGenerator map, ProceduralBiomeBridge bridge)
        {
            if (map == null) return null;
            var context = map.GetComponent<BiomeRuntimeContext>() ?? map.gameObject.AddComponent<BiomeRuntimeContext>();
            context.map = map;
            context.bridge = bridge;
            context.ConnectPlayerMotor();
            return context;
        }

        private void Update() => ConnectPlayerMotor();

        private void ConnectPlayerMotor()
        {
            if (!Application.isPlaying || !isActiveAndEnabled || cancelled || map == null) return;
            PlayerWorldBindings.ConnectTerrain?.Invoke(this);
        }

        public IEnumerator WaitForTerrainReady()
        {
            while (!IsCancelled && !map.IsReady) yield return null;
        }

        public IEnumerator WaitForEncounterReady()
        {
            while (!IsCancelled && Readiness != BiomeReadiness.EncounterReady) yield return null;
        }

        public void MarkEncounterPrepared()
        {
            if (!cancelled && map != null) encounterPrepared = true;
        }

        public bool TryBindPlayer(UnityEngine.Object player, Action<Vector3> bindSpawn)
        {
            if (!IsReady || WorldProgressBindings.Session.IsRestorePending || player == null || boundPlayer == player) return false;
            bindSpawn(map.GetPlayerSpawnWorldPosition());
            boundPlayer = player;
            return true;
        }

        public bool CanPlayerMove(Vector3 from, Vector3 to, Vector2 halfExtents)
            => EncounterSpatialQueries.CanPlayerTraverse(from, to, halfExtents)
                && (!IsReady || map.CanPlayerMoveWorld(from, to, halfExtents));
        public bool CanStartTraversal(Vector3 from, Vector3 to, Vector2 halfExtents)
            => EncounterSpatialQueries.CanStartTraversal(from, to, halfExtents);
        public bool TryGetClimbDestination(Vector3 from, Vector3 facing, out Vector3 destination)
            => map.TryGetClimbDestinationWorld(from, facing, out destination, out _);
        public bool TryGetLavaJumpDestination(Vector3 from, Vector3 facing, out Vector3 destination)
            => map.TryGetLavaJumpDestinationWorld(from, facing, out destination);
        public bool TryGetClearTraversalLanding(Vector3 destination, Vector3 facing, Vector2 halfExtents, out Vector3 landing)
            => map.TryGetClearTraversalLanding(destination, facing, halfExtents, out landing);
        public int GetHeightLevel(Vector3 position) => map.GetHeightLevelAtWorld(position);

        private void OnDisable()
        {
            cancelled = true;
            PlayerWorldBindings.DisconnectTerrain?.Invoke(this);
            boundPlayer = null;
        }
    }
}
