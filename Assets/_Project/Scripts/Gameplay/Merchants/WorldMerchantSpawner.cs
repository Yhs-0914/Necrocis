using ProceduralMap;
using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public sealed class WorldMerchantSpawner : MonoBehaviour
    {
        [Tooltip("테스트 중에는 저장된 상인 위치 대신 플레이어 입구 근처에 배치합니다.")]
        [SerializeField] private bool spawnNearEntranceForTesting = true;
        private BiomeManager biome;
        private MapGenerator terrain;
        private bool attempted;
        private float nextRefresh;
        public WorldMerchantNpc Merchant { get; private set; }
        public RectInt? BossArea { get; private set; }
        public bool UsedPreferredDistances { get; private set; }
        public bool SpawnNearEntranceForTesting => spawnNearEntranceForTesting;

        public static WorldMerchantSpawner Ensure(BiomeManager owner)
        {
            if (owner == null || owner.BiomeType < BiomeType.Intestine || owner.BiomeType > BiomeType.Lung) return null;
            var spawner = owner.GetComponent<WorldMerchantSpawner>() ?? owner.gameObject.AddComponent<WorldMerchantSpawner>();
            spawner.biome = owner; spawner.terrain = owner.GetComponent<MapGenerator>();
            return spawner;
        }

        private void Start() => SpawnNow();

        public void SpawnNow()
        {
            if (attempted || biome == null || (terrain != null && !terrain.IsReady)) return;
            attempted = true;
            BiomeConfig config = biome is ProceduralBiomeBridge bridge ? bridge.GetBiomeConfig()
                : biome is ConfigurableBiomeManager configured ? configured.GetBiomeConfig() : null;
            BossArea = GetBossArea(biome, config?.GetMidBossArenaConfig());
            var portal = config?.GetReturnPortalConfig();
            Vector2Int entrance = biome.WorldToGrid(biome.GetPlayerSpawnPosition());
            Vector2Int portalCell = portal != null && portal.useCustomPosition ? portal.gridPosition : entrance;
            float portalClearance = spawnNearEntranceForTesting ? 2.5f : 12f;
            bool Excluded(Vector2Int cell) => (BossArea.HasValue && BossArea.Value.Contains(cell))
                || (portal != null && portal.enabled && (cell - portalCell).sqrMagnitude < portalClearance * portalClearance)
                || (terrain != null && terrain.IsCellReservedForBossArena(cell.x, cell.y));
            bool Walkable(Vector2Int cell) => biome.IsValidPosition(cell.x, cell.y)
                && biome.IsWalkable(cell.x, cell.y) && (terrain == null || !terrain.Data.GetCell(cell.x, cell.y).Occupied);
            bool CanMove(Vector2Int a, Vector2Int b) => biome.CanMove(biome.GridToWorld(a.x, a.y), biome.GridToWorld(b.x, b.y));
            var saved = SaveService.FindWorldMerchant(biome.BiomeType, biome.Seed);
            Vector2Int selected;
            int randomSeed = unchecked(biome.Seed * 397 ^ (int)biome.BiomeType * 7919 ^ 0x4D4552);
            if (spawnNearEntranceForTesting)
            {
                // Preview near the player without overwriting the normal saved placement.
                if (!MerchantPlacement.TryChoose(biome.MapWidth, biome.MapHeight, randomSeed, entrance, BossArea,
                    Walkable, CanMove, Excluded, out selected, out bool preferred, nearEntrance: true))
                { Debug.LogWarning("[Merchant] 입구 근처에 안전한 테스트 상인 위치가 없습니다."); return; }
                UsedPreferredDistances = preferred;
            }
            else if (saved != null)
            {
                selected = new Vector2Int(saved.x, saved.y);
                if (!Walkable(selected) || Excluded(selected))
                { Debug.LogWarning("[Merchant] 저장된 상인 위치가 현재 지형에서 유효하지 않습니다."); return; }
                UsedPreferredDistances = Vector2Int.Distance(selected, entrance) >= MerchantPlacement.EntranceDistance
                    && (!BossArea.HasValue || MerchantPlacement.DistanceToArea(selected, BossArea.Value) >= MerchantPlacement.BossDistance);
            }
            else
            {
                if (!MerchantPlacement.TryChoose(biome.MapWidth, biome.MapHeight, randomSeed, entrance, BossArea,
                    Walkable, CanMove, Excluded, out selected, out bool preferred))
                { Debug.LogWarning("[Merchant] 이동 가능한 상인 배치 위치가 없습니다."); return; }
                UsedPreferredDistances = preferred;
                if (!SaveService.TryStoreWorldMerchant(new WorldMerchantSaveData
                    { biome = biome.BiomeType, mapSeed = biome.Seed, x = selected.x, y = selected.y }, out string error))
                { Debug.LogError("[Merchant] 상인 배치 저장 실패: " + error); return; }
            }
            Merchant = WorldMerchantNpc.Create(transform, selected, biome.GridToWorldWithHeight(selected.x, selected.y, .03f));
            if (!UsedPreferredDistances) Debug.LogWarning("[Merchant] 맵 공간이 부족해 이동 가능한 먼 위치를 사용합니다.");
            RefreshVisibility();
        }

        public static RectInt? GetBossArea(BiomeManager owner, MidBossArenaConfig arena)
        {
            if (arena == null || !arena.enabled || (arena.onlyEnableOnLargeMaps
                && (owner.MapWidth < arena.minimumMapWidth || owner.MapHeight < arena.minimumMapHeight))) return null;
            Vector2Int center = arena.useCustomCenter ? arena.centerGrid : new Vector2Int(owner.MapWidth / 2, owner.MapHeight / 2);
            int padding = Mathf.Max(1, arena.wallThicknessInCells);
            var presentation = arena.GetPresentationConfig();
            if (presentation.enabled) padding += presentation.approachLengthInCells;
            Vector2Int size = new Vector2Int(Mathf.Max(8, arena.arenaSize.x), Mathf.Max(8, arena.arenaSize.y));
            return new RectInt(center.x - size.x / 2 - padding, center.y - size.y / 2 - padding,
                size.x + padding * 2, size.y + padding * 2);
        }

        private void Update()
        {
            if (!attempted) SpawnNow();
            if (Time.time < nextRefresh) return;
            nextRefresh = Time.time + .2f; RefreshVisibility();
        }

        public void RefreshVisibility()
        {
            if (Merchant != null) Merchant.SetVisible(biome != null && biome.isActiveAndEnabled
                && biome.IsChunkLoadedAt(Merchant.Cell.x, Merchant.Cell.y));
        }

        private void OnDisable() { if (Merchant != null) Merchant.SetVisible(false); }
    }
}
