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
        [SerializeField] private Transform[] pillars;
        private MapGenerator map;
        private FinalBossBalanceConfig runtimeBalance;
        private DifficultyBalanceProfile summonDifficulty;
        private MonsterProgressionProfile summonProgression;
        public FinalBossBalanceConfig Balance
        {
            get
            {
                if (runtimeBalance == null)
                {
                    FinalBossBalanceConfig source = Resources.Load<FinalBossBalanceConfig>(FinalBossBalanceConfig.ResourcePath);
                    if (source == null) throw new System.InvalidOperationException("최종 보스 전용 밸런스 설정이 없습니다.");
                    string error = source.GetValidationError();
                    if (error != null) throw new System.InvalidOperationException(error);
                    runtimeBalance = Instantiate(source);
                    runtimeBalance.hideFlags = HideFlags.HideAndDontSave;
                }
                return runtimeBalance;
            }
        }

        public static EnemyDamageRequest CreateDamage(float amount)
        {
            float multiplier = Instance != null ? Instance.Balance.GetDifficulty(SaveService.ActiveDifficulty).damage : 1f;
            return new EnemyDamageRequest(Mathf.Max(0f, amount * multiplier), null);
        }

        public MonsterBalanceRuntime CaptureSummonBalance(MonsterDefinition definition, bool weakened)
        {
            if (definition == null) throw new System.InvalidOperationException("최종 보스 소환 몬스터의 정의가 없습니다.");
            if (summonDifficulty == null)
            {
                summonDifficulty = ScriptableObject.CreateInstance<DifficultyBalanceProfile>();
                summonDifficulty.difficulty = SaveService.ActiveDifficulty;
                summonDifficulty.hideFlags = HideFlags.HideAndDontSave;
                summonProgression = ScriptableObject.CreateInstance<MonsterProgressionProfile>();
                summonProgression.hideFlags = HideFlags.HideAndDontSave;
            }
            // This encounter has its own multipliers and no biome visit progression.
            var result = new MonsterBalanceRuntime(definition, summonDifficulty, summonProgression, 0);
            FinalBossDifficultyBalance difficulty = Balance.GetDifficulty(summonDifficulty.difficulty);
            float ratio = weakened ? Balance.phaseOne.weakenedMidBossStatRatio : 1f;
            result.ApplyEncounterMultipliers(difficulty.health * ratio, difficulty.damage * ratio,
                difficulty.summonMoveSpeed, weakened ? 0f : difficulty.summonExperience, difficulty.summonCooldown);
            return result;
        }

        public RectInt RoomArea => map.AuthoredLayout != null ? map.AuthoredLayout.AtlasArea
            : new RectInt(0, 0, map.MapWidth, map.MapHeight);
        public Vector2 WorldSize => new Vector2(RoomArea.width, RoomArea.height);
        public Vector2 RoomOrigin => RoomArea.position;
        public bool HasEnteredEncounter { get; private set; }
        public RectInt RoomToMapArea(RectInt area) => new RectInt(
            area.x + RoomArea.x, area.y + RoomArea.y, area.width, area.height);
        public Vector3 SpawnPosition => map.GetPlayerSpawnWorldPosition();
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
            for (int i = 0; i < areas.Length; i++) areas[i] = RoomToMapArea(areas[i]);
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
            if (runtimeBalance != null) Destroy(runtimeBalance);
            if (summonDifficulty != null) Destroy(summonDifficulty);
            if (summonProgression != null) Destroy(summonProgression);
        }

        private void Update()
        {
            if (HasEnteredEncounter || map == null || !map.IsReady || SaveService.IsRestorePending
                || PlayerController.Instance == null || PlayerController.Instance.IsDead) return;
            Vector3 position = PlayerController.Instance.transform.position;
            float entrance = RoomArea.y + 7f;
            if (position.z >= entrance && IsWalkable(position, new Vector2(.55f, .42f)))
                HasEnteredEncounter = true;
        }

        public Vector3 UVToWorld(Vector2 uv) => new Vector3(
            RoomArea.x + uv.x * RoomArea.width, 0f, RoomArea.y + uv.y * RoomArea.height);
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
