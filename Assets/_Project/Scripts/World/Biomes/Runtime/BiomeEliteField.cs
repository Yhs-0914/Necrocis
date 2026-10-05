using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public sealed class BiomeEliteField : MonoBehaviour
    {
        private BiomeManager biome;
        private BiomeEliteSpawnConfig settings;
        private string runId;
        private float nextRefresh;
        private readonly HashSet<string> localDefeated = new HashSet<string>();
        private readonly Dictionary<string, EnemySpawnRuleConfig> rules = new Dictionary<string, EnemySpawnRuleConfig>();
        private readonly Dictionary<string, BiomeEliteFieldSpawner> spawners = new Dictionary<string, BiomeEliteFieldSpawner>();
        public BiomeElitePlan Plan { get; private set; }
        public IReadOnlyCollection<BiomeEliteFieldSpawner> Spawners => spawners.Values;

        public static BiomeEliteField Create(BiomeManager biome, BiomeConfig config)
        {
            var field = biome.GetComponent<BiomeEliteField>() ?? biome.gameObject.AddComponent<BiomeEliteField>();
            field.Configure(biome, config.biomeEliteSpawnConfig, config.GetMidBossArenaConfig(), config.GetReturnPortalConfig());
            return field;
        }

        public void Configure(BiomeManager owner, BiomeEliteSpawnConfig source, MidBossArenaConfig arena, BiomeReturnPortalConfig portal = null)
        {
            Clear();
            biome = owner;
            settings = source;
            runId = SaveService.ActiveRunId;
            if (source == null || !source.enabled || source.monsters == null || source.monsters.Count == 0) return;
            string error = source.GetValidationError();
            if (error != null) { Debug.LogError("[BiomeElite] " + error); return; }
            foreach (EnemySpawnRuleConfig rule in source.monsters) rules.Add(rule.monsterDefinition.monsterId, rule);
            Plan = SaveService.GetBiomeElitePlan(biome.BiomeType);
            if (Plan == null)
            {
                Vector2Int center = arena != null && arena.useCustomCenter ? arena.centerGrid : new Vector2Int(biome.MapWidth / 2, biome.MapHeight / 2);
                bool arenaEnabled = arena != null && arena.enabled && (!arena.onlyEnableOnLargeMaps || (biome.MapWidth >= arena.minimumMapWidth && biome.MapHeight >= arena.minimumMapHeight));
                float padding = arenaEnabled ? settings.bossExclusionPadding + arena.wallThicknessInCells
                    + (arena.presentation != null && arena.presentation.enabled ? arena.presentation.approachLengthInCells : 0) : 0;
                Vector2Int entrance = biome.WorldToGrid(biome.GetPlayerSpawnPosition());
                Vector2Int portalCell = portal != null && portal.useCustomPosition ? portal.gridPosition : entrance;
                bool Excluded(Vector2Int p) => (arenaEnabled && Mathf.Abs(p.x - center.x) <= arena.arenaSize.x / 2f + padding
                    && Mathf.Abs(p.y - center.y) <= arena.arenaSize.y / 2f + padding)
                    || (portal != null && portal.enabled && (p - portalCell).sqrMagnitude <= settings.portalExclusionRadius * settings.portalExclusionRadius);
                Plan = BiomeElitePlacementService.Build(settings, biome.BiomeType, biome.Seed, biome.MapWidth, biome.MapHeight,
                    biome.WorldToGrid(biome.GetPlayerSpawnPosition()), p => biome.IsWalkable(p.x, p.y),
                    (a, b) => biome.CanMove(biome.GridToWorld(a.x, a.y), biome.GridToWorld(b.x, b.y)), Excluded);
                if (SaveService.HasActiveSession && !SaveService.TryStoreBiomeElitePlan(Plan, out error))
                { Debug.LogError("[BiomeElite] 배치 저장 실패: " + error); Plan = null; return; }
            }
            if (Plan.mapSeed != biome.Seed) { Debug.LogError("[BiomeElite] 저장된 맵 시드와 배치가 다릅니다."); Plan = null; return; }
            if (Plan.placements.Count < Plan.requestedCount)
                Debug.LogWarning($"[BiomeElite] 유효 위치 부족: {Plan.requestedCount} 중 {Plan.placements.Count} 배치. 간격/안전 조건은 유지합니다.");
            foreach (BiomeElitePlacement point in Plan.placements)
                if (!rules.ContainsKey(point.monsterId)) Debug.LogWarning("[BiomeElite] 저장된 정의가 현재 목록에 없습니다: " + point.monsterId);
        }

        private void Update()
        {
            if (Time.time < nextRefresh || PlayerController.Instance == null) return;
            nextRefresh = Time.time + .2f;
            Refresh(PlayerController.Instance.transform.position);
        }

        public void Refresh(Vector3 playerPosition)
        {
            if (Plan == null || (runId != SaveService.ActiveRunId)) { ClearSpawners(); return; }
            foreach (BiomeElitePlacement point in Plan.placements)
            {
                bool loaded = biome.IsChunkLoadedAt(point.x, point.y);
                bool defeated = localDefeated.Contains(point.spawnId) || SaveService.IsBiomeEliteDefeated(point.spawnId);
                if (loaded && defeated && spawners.TryGetValue(point.spawnId, out BiomeEliteFieldSpawner dying)
                    && dying.ActiveEnemy != null && dying.ActiveEnemy.IsDeathAnimPlaying)
                {
                    dying.Refresh(playerPosition); // Keeps only the corpse animation; distance/run changes still release it.
                    continue;
                }
                if (!loaded || defeated)
                {
                    if (spawners.Remove(point.spawnId, out BiomeEliteFieldSpawner old)) { old.ReleaseEnemy(); old.gameObject.SetActive(false); Destroy(old.gameObject); }
                    continue;
                }
                if (!spawners.TryGetValue(point.spawnId, out BiomeEliteFieldSpawner spawner))
                {
                    if (!rules.TryGetValue(point.monsterId, out EnemySpawnRuleConfig rule) || !biome.IsWalkable(point.x, point.y)) continue;
                    GameObject node = new GameObject(point.spawnId);
                    node.transform.SetParent(transform, false);
                    node.transform.position = biome.GridToWorldWithHeight(point.x, point.y, rule.heightOffset);
                    spawner = node.AddComponent<BiomeEliteFieldSpawner>();
                    spawner.Configure(point, rule, settings, runId, id => localDefeated.Add(id));
                    spawners.Add(point.spawnId, spawner);
                }
                spawner.Refresh(playerPosition);
            }
        }

        private void ClearSpawners()
        {
            foreach (BiomeEliteFieldSpawner spawner in spawners.Values)
                if (spawner != null) { spawner.ReleaseEnemy(); spawner.gameObject.SetActive(false); Destroy(spawner.gameObject); }
            spawners.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            if (Plan == null || biome == null) return;
            foreach (BiomeElitePlacement point in Plan.placements)
            {
                Gizmos.color = localDefeated.Contains(point.spawnId) || SaveService.IsBiomeEliteDefeated(point.spawnId) ? Color.gray : Color.cyan;
                Gizmos.DrawWireSphere(biome.GridToWorldWithHeight(point.x, point.y), .8f);
            }
        }

        private void Clear() { ClearSpawners(); rules.Clear(); localDefeated.Clear(); Plan = null; }
        private void OnDisable() => ClearSpawners();
    }
}
