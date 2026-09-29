using System.Collections;
using System.Linq;
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
            EnsurePhaseOneController();
        }

        private void EnsurePhaseOneController()
        {
            if (GetComponent<FinalBossPhaseOneController>() != null) return;
            Transform props = transform.Find("Arena Props");
            if (props == null) return;
            string[] names = { "Pillar_Stomach", "Pillar_Intestine", "Pillar_Liver", "Pillar_Lung" };
            BiomeType[] biomes = { BiomeType.Stomach, BiomeType.Intestine, BiomeType.Liver, BiomeType.Lung };
            RectInt[] areas =
            {
                new RectInt(9, 23, 6, 3), new RectInt(33, 23, 6, 3),
                new RectInt(9, 9, 6, 3), new RectInt(33, 9, 6, 3)
            };
            var phasePillars = new FinalBossPillar[4];
            for (int i = 0; i < names.Length; i++)
            {
                Transform target = props.Find(names[i]);
                if (target == null) return;
                phasePillars[i] = target.GetComponent<FinalBossPillar>() ?? target.gameObject.AddComponent<FinalBossPillar>();
                phasePillars[i].Configure(biomes[i], areas[i], target.GetComponent<SpriteRenderer>());
            }
            Transform boss = props.Find("DormantCerebrum");
            BiomeConfig config = GetComponent<ProceduralBiomeBridge>()?.GetBiomeConfig();
            BiomeConfig[] sources = config != null && config.finalBossSourceBiomes != null
                ? config.finalBossSourceBiomes.Where(value => value != null).ToArray()
                : System.Array.Empty<BiomeConfig>();
            GetComponent<FinalBossPhaseOneController>()?.Configure(boss, phasePillars, sources);
            if (GetComponent<FinalBossPhaseOneController>() == null)
                gameObject.AddComponent<FinalBossPhaseOneController>().Configure(boss, phasePillars, sources);
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
