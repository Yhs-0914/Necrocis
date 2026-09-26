using System.Collections;
using ProceduralMap;
using UnityEngine;

namespace Necrocis
{
    /// <summary>Room state and landmarks only. MapGenerator owns terrain and movement.</summary>
    [RequireComponent(typeof(MapGenerator), typeof(ProceduralBiomeBridge))]
    public sealed class FinalBossArena : MonoBehaviour
    {
        public static FinalBossArena Instance { get; private set; }
        [SerializeField] private Transform returnEntrance;
        [SerializeField] private Transform[] pillars;
        private MapGenerator map;

        public Vector2 WorldSize => new Vector2(map.MapWidth, map.MapHeight);
        public Vector3 SpawnPosition => map.GetPlayerSpawnWorldPosition();
        public Vector3 ReturnPosition => returnEntrance.position;
        public int PillarCount => pillars != null ? pillars.Length : 0;

        private void Awake()
        {
            Instance = this;
            map = GetComponent<MapGenerator>();
        }

        private IEnumerator Start()
        {
            // A direct-scene preview may restore the new run's Hub checkpoint on the first frame.
            // Apply the scene state only after that restore has finished.
            yield return null;
            while (!map.IsReady || PlayerController.Instance == null || SaveService.IsRestorePending) yield return null;
            // Shared ProceduralTerrainMotor handles spawn and floor height, as in all four biomes.
            GameManager.Instance?.SetGameState(GameState.InFinalBoss);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public Vector3 UVToWorld(Vector2 uv) => new Vector3(uv.x * map.MapWidth, 0f, uv.y * map.MapHeight);
        public Vector3 GetPillarPosition(int index) => pillars[index].position;

        // Editor checks query the shared grid, never a second collision model.
        public bool CanTraverse(Vector3 from, Vector3 to, Vector2 halfExtents) => map.CanPlayerMoveWorld(from, to, halfExtents);
        public bool IsWalkable(Vector3 position, Vector2 halfExtents)
        {
            for (int x = -1; x <= 1; x++)
            for (int z = -1; z <= 1; z++)
            {
                Vector2Int cell = map.WorldToCell(position + new Vector3(x * halfExtents.x, 0f, z * halfExtents.y));
                if (!map.IsCellWalkable(cell.x, cell.y)) return false;
            }
            return true;
        }
    }
}
