using UnityEngine;

namespace Necrocis
{
    public static class ResiduePlacementSafety
    {
        public static bool CanPlace(Vector3 center, Vector3 forward, Vector2 size)
        {
            BiomeManager biome = BiomeManager.Active;
            if (biome == null || PlayerController.Instance == null) return false;
            var motor = PlayerController.Instance.GetComponent<ProceduralTerrainMotor>();
            Vector2 feet = motor != null ? motor.TerrainHalfExtents : new Vector2(.68f, .48f);
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            float clearance = feet.magnitude + .15f;
            Vector2 outer = size * .5f + Vector2.one * clearance;
            // Avoid unbounded work if a development Inspector value is accidentally enormous.
            if (outer.x > 8 || outer.y > 8) return false;
            float exclusion = outer.magnitude + 1f;
            Vector3 entrance = biome.GetPlayerSpawnPosition(); entrance.y = center.y;
            if ((center - entrance).sqrMagnitude <= exclusion * exclusion) return false;
            if (biome is ProceduralBiomeBridge bridge)
            {
                BiomeConfig config = bridge.GetBiomeConfig();
                var portal = config.GetReturnPortalConfig();
                Vector3 portalPosition = portal != null && portal.useCustomPosition
                    ? biome.GridToWorld(portal.gridPosition.x, portal.gridPosition.y) : entrance;
                portalPosition.y = center.y;
                if (portal != null && portal.enabled && (center - portalPosition).sqrMagnitude <= exclusion * exclusion) return false;
                var arena = config.GetMidBossArenaConfig();
                if (arena != null && arena.enabled && (!arena.onlyEnableOnLargeMaps
                    || (biome.MapWidth >= arena.minimumMapWidth && biome.MapHeight >= arena.minimumMapHeight)))
                {
                    Vector2Int arenaCell = arena.useCustomCenter ? arena.centerGrid : new Vector2Int(biome.MapWidth / 2, biome.MapHeight / 2);
                    Vector2Int cell = biome.WorldToGrid(center);
                    if (Mathf.Abs(cell.x - arenaCell.x) <= arena.arenaSize.x * .5f + exclusion + arena.wallThicknessInCells
                        && Mathf.Abs(cell.y - arenaCell.y) <= arena.arenaSize.y * .5f + exclusion + arena.wallThicknessInCells) return false;
                }
            }
            Vector2Int middle = biome.WorldToGrid(center);
            if (!biome.IsValidPosition(middle.x, middle.y)) return false;
            int level = biome.GetHeightLevel(middle.x, middle.y);
            bool Clear(Vector3 point)
            {
                if (!ResidueRubble.CanOccupy(point, feet)) return false;
                for (int x = -1; x <= 1; x++) for (int z = -1; z <= 1; z++)
                {
                    Vector3 sample = point + new Vector3(x * feet.x, 0, z * feet.y);
                    Vector2Int cell = biome.WorldToGrid(sample);
                    if (!biome.IsValidPosition(cell.x, cell.y) || !biome.IsWalkable(cell.x, cell.y)
                        || biome.GetHeightLevel(cell.x, cell.y) != level) return false;
                }
                return motor == null || motor.CanMove(point, point);
            }
            if (!Clear(center)) return false;
            // A continuous, player-width walkable perimeter guarantees a way around either side of this low slab.
            Vector3[] corners = { center - right * outer.x - forward * outer.y, center + right * outer.x - forward * outer.y,
                center + right * outer.x + forward * outer.y, center - right * outer.x + forward * outer.y };
            for (int side = 0; side < 4; side++)
            {
                Vector3 a = corners[side], b = corners[(side + 1) % 4];
                int steps = Mathf.CeilToInt(Vector3.Distance(a, b) / .25f);
                Vector3 previous = a;
                for (int i = 0; i <= steps; i++)
                {
                    Vector3 p = Vector3.Lerp(a, b, i / (float)steps);
                    if (!Clear(p) || (motor != null && !motor.CanMove(previous, p))) return false;
                    previous = p;
                }
            }
            // The landing footprint itself must also be walkable, without holes inside the clear perimeter.
            for (float x = -size.x * .5f; x <= size.x * .5f + .01f; x += .25f)
                for (float z = -size.y * .5f; z <= size.y * .5f + .01f; z += .25f)
                    if (!Clear(center + right * x + forward * z)) return false;
            return true;
        }
    }
}
