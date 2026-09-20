using System.Collections.Generic;
using ProceduralMap;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Necrocis
{
    /// <summary>A connected, telegraphed convoy crossing the map in four directions.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MapGenerator))]
    public sealed class IntestineTransitHazard : MonoBehaviour
    {
        [SerializeField] private Tilemap ground;
        [SerializeField] private Sprite bolusSprite;
        [Header("Timing (scaled game seconds)")]
        [SerializeField, Min(0)] private float initialDelay = 8f;
        [SerializeField, Range(3, 5)] private float warningDuration = 4f;
        [SerializeField, Min(1)] private float restDuration = 10f;
        [Tooltip("Latest front arrival anywhere on the lane, measured from warning start.")]
        [SerializeField, Min(6)] private float latestArrival = 7f;
        [Tooltip("Minimum continuous passage duration at any fixed point on the lane.")]
        [SerializeField, Min(10)] private float passageDuration = 12f;
        [Header("Four-direction convoy")]
        [Tooltip("Minimum segment count. More are streamed automatically for the passage duration.")]
        [SerializeField, Range(3, 28)] private int carriageCount = 14;
        [Tooltip("Minimum speed; increased automatically for longer maps to meet arrival timing.")]
        [SerializeField, Min(1)] private float speed = 100f;
        [SerializeField, Range(0.05f, 0.2f)] private float connectionOverlap = 0.1f;
        [Tooltip("Full warning and collision width in world units. Bolus visuals scale with it.")]
        [SerializeField, Min(1.3f)] private float laneWidth = 2.6f;
        [SerializeField, Min(0)] private float damage = 1f;
        [SerializeField, Range(0, 0.8f)] private float slowRatio = 0.35f;
        [SerializeField, Min(0)] private float slowDuration = 2f;
        [SerializeField, Min(0.2f)] private float hitCooldown = 1.2f;

        private enum Phase { Rest, Warning, Transit }
        private Phase phase;
        private MapGenerator map;
        private readonly List<SpriteRenderer> warnings = new List<SpriteRenderer>();
        private readonly List<Transform> cars = new List<Transform>();
        private GameObject visuals;
        private Texture2D whiteTexture;
        private Sprite whiteSprite;
        private float timer, travel, length, nextHit;
        private float transitSpeed;
        private int transitCarriageCount;
        private Vector3 start, end;
        private Vector3 direction = Vector3.right;
        private Vector3 across = Vector3.forward;
        private static readonly int[] LaneOffsets = { 0, 2, -2, 4, -4 };
        private float VisualScale => Mathf.Max(1.3f, laneWidth) / 1.3f;
        private float HalfLength => 1.6f * VisualScale;
        private float HalfWidth => Mathf.Max(1.3f, laneWidth) * 0.5f;
        private float CarSpacing => HalfLength * 2f * (1f - Mathf.Clamp(connectionOverlap, 0.05f, 0.2f));

        private void OnEnable()
        {
            map = GetComponent<MapGenerator>();
            timer = initialDelay;
            phase = Phase.Rest;
        }

        private void Update()
        {
            if (Time.deltaTime <= 0 || map == null || !map.IsReady) return;
            PlayerController player = PlayerController.Instance;
            if (player == null || !player.isActiveAndEnabled || player.IsDead ||
                MidBossArenaController.IsPlayerInsideLockedArena(player.transform.position))
            {
                CancelCycle();
                return;
            }
            if (phase == Phase.Rest)
            {
                timer -= Time.deltaTime;
                if (timer > 0) return;
                if (ground == null || bolusSprite == null || !TryChooseLane(player.transform.position))
                {
                    timer = 2f;
                    return;
                }
                ConfigureTransit();
                BuildVisuals();
                phase = Phase.Warning;
                timer = Mathf.Clamp(warningDuration, 3f, 5f);
                return;
            }
            if (phase == Phase.Warning)
            {
                timer -= Time.deltaTime;
                float pulse = 0.25f + 0.3f * (0.5f + 0.5f * Mathf.Sin(Time.time * (timer < 1f ? 18f : 8f)));
                foreach (SpriteRenderer marker in warnings)
                    marker.color = new Color(1f, 0.28f, 0.08f, pulse);
                if (timer > 0) return;
                phase = Phase.Transit;
                travel = -HalfLength;
                nextHit = 0;
                foreach (SpriteRenderer marker in warnings) marker.color = new Color(1f, 0.38f, 0.08f, 0.18f);
            }

            float previous = travel;
            travel += transitSpeed * Time.deltaTime;
            Camera camera = DontStarveCamera.GetActiveCamera();
            // Reuse just enough renderers for the map, even if the full train is much longer.
            int firstCar = Mathf.Max(0, Mathf.CeilToInt((travel - length - HalfLength) / CarSpacing));
            for (int i = 0; i < cars.Count; i++)
            {
                int carIndex = firstCar + i;
                float offset = carIndex * CarSpacing;
                float distance = travel - offset;
                // Grow/shrink at the marked entry/exit so nothing hits outside the warning strip.
                float left = Mathf.Max(0, distance - HalfLength);
                float right = Mathf.Min(length, distance + HalfLength);
                bool visible = carIndex < transitCarriageCount && right > left;
                cars[i].gameObject.SetActive(visible);
                if (visible)
                {
                    cars[i].position = start + direction * ((left + right) * 0.5f) + Vector3.up * (0.65f * VisualScale);
                    // Project the world travel axis onto the billboard plane. This keeps the
                    // connected tips aligned even for north/south travel with a tilted camera.
                    Vector3 viewAxis = camera != null ? camera.transform.InverseTransformDirection(direction) : direction;
                    Vector2 projected = new Vector2(viewAxis.x, viewAxis.y);
                    if (camera == null) projected = new Vector2(direction.x, direction.z);
                    float angle = Mathf.Atan2(projected.y, projected.x) * Mathf.Rad2Deg;
                    cars[i].rotation = (camera != null ? camera.transform.rotation : Quaternion.Euler(90, 0, 0)) * Quaternion.Euler(0, 0, angle);
                    // The edited image has approximately 3.5% transparent padding at each end.
                    float fullWidth = (right - left) / 0.93f;
                    cars[i].localScale = new Vector3(fullWidth * projected.magnitude / bolusSprite.bounds.size.x, VisualScale, 1f);
                }
            }
            // Overlapping segments form one continuous train. Sweep its whole occupied interval
            // so a fast front cannot tunnel past a player between frames or renderer reuse.
            float sweptLeft = Mathf.Max(0, previous - (transitCarriageCount - 1) * CarSpacing - HalfLength);
            float sweptRight = Mathf.Min(length, travel + HalfLength);
            if (sweptRight > sweptLeft) TryHit(player, sweptLeft, sweptRight);
            if (travel - (transitCarriageCount - 1) * CarSpacing - HalfLength >= length) CancelCycle();
        }

        private void ConfigureTransit()
        {
            float travelBudget = Mathf.Max(0.5f, latestArrival - Mathf.Clamp(warningDuration, 3f, 5f));
            transitSpeed = Mathf.Max(1f, speed, length / travelBudget);
            float requiredSpan = transitSpeed * Mathf.Max(10f, passageDuration);
            transitCarriageCount = Mathf.Max(carriageCount,
                Mathf.CeilToInt(Mathf.Max(0f, requiredSpan - HalfLength * 2f) / CarSpacing) + 1);
        }

        private bool TryChooseLane(Vector3 playerPosition)
        {
            int first = Random.Range(0, 4);
            for (int attempt = 0; attempt < 4; attempt++)
                if (TryChooseDirectionalLane(playerPosition, (first + attempt) % 4)) return true;
            return false;
        }

        // 0: left->right, 1: right->left, 2: bottom->top, 3: top->bottom.
        private bool TryChooseDirectionalLane(Vector3 playerPosition, int heading)
        {
            Vector3Int cell = ground.WorldToCell(playerPosition);
            if (cell.x < 0 || cell.x >= map.MapWidth || cell.y < 0 || cell.y >= map.MapHeight) return false;
            int height = map.GetCellHeightLevel(cell.x, cell.y);
            bool vertical = heading >= 2;
            int alongCell = vertical ? cell.y : cell.x;
            int crossCell = vertical ? cell.x : cell.y;
            int mapLength = vertical ? map.MapHeight : map.MapWidth;
            float crossSize = Vector3.Distance(map.GetCellCenterWorld(0, 0),
                map.GetCellCenterWorld(vertical ? 1 : 0, vertical ? 0 : 1));
            int halfRows = Mathf.CeilToInt(HalfWidth / Mathf.Max(0.01f, crossSize));
            foreach (int offset in LaneOffsets)
            {
                int row = crossCell + offset;
                if (!SafeCrossSection(alongCell, row, height, vertical, halfRows + 1)) continue;
                bool crossesArena = false;
                for (int x = 0; x < mapLength && !crossesArena; x++)
                    for (int z = row - halfRows; z <= row + halfRows; z++)
                        if (map.IsCellReservedForBossArena(vertical ? z : x, vertical ? x : z)) { crossesArena = true; break; }
                if (crossesArena) continue;
                Vector3 first = map.GetCellCenterWorld(vertical ? row : 0, vertical ? 0 : row);
                Vector3 step = map.GetCellCenterWorld(vertical ? row : 1, vertical ? 1 : row) - first;
                start = first - step * 0.5f;
                end = first + step * (mapLength - 0.5f);
                if ((heading & 1) != 0) { Vector3 swap = start; start = end; end = swap; }
                length = Vector3.Distance(start, end);
                direction = (end - start).normalized;
                across = new Vector3(-direction.z, 0, direction.x);
                if (length >= 10f) return true;
            }
            return false;
        }

        private bool SafeCrossSection(int along, int row, int height, bool vertical, int escapeRows)
        {
            for (int z = row - escapeRows; z <= row + escapeRows; z++)
            {
                int xCell = vertical ? z : along;
                int zCell = vertical ? along : z;
                if (!map.IsCellWalkable(xCell, zCell) || map.IsCellReservedForBossArena(xCell, zCell) ||
                    map.GetCellHeightLevel(xCell, zCell) != height) return false;
            }
            return true;
        }

        private void TryHit(PlayerController player, float left, float right)
        {
            if (Time.time < nextHit || player.IsDashInvincible || player.IsDead ||
                (player.HealthComponent != null && player.HealthComponent.IsInvincible)) return;
            Bounds bounds = player.HitCollider != null ? player.HitCollider.bounds : new Bounds(player.transform.position, Vector3.one * 0.5f);
            if (!OverlapsLane(bounds, left, right)) return;
            nextHit = Time.time + hitCooldown;
            player.TakeDamage(damage);
            if (player.IsDead) return;
            PlayerStatusEffectController status = player.GetComponent<PlayerStatusEffectController>();
            if (status == null) status = player.gameObject.AddComponent<PlayerStatusEffectController>();
            status.ApplyMoveSpeedSlow(slowRatio, slowDuration);
        }

        private bool OverlapsLane(Bounds bounds, float left, float right)
        {
            Vector3 relative = bounds.center - start;
            float along = Vector3.Dot(relative, direction);
            float side = Vector3.Dot(relative, across);
            float alongExtent = Mathf.Abs(direction.x) * bounds.extents.x + Mathf.Abs(direction.z) * bounds.extents.z;
            float sideExtent = Mathf.Abs(across.x) * bounds.extents.x + Mathf.Abs(across.z) * bounds.extents.z;
            return along + alongExtent >= left && along - alongExtent <= right &&
                Mathf.Abs(side) <= HalfWidth + sideExtent;
        }

        private void BuildVisuals()
        {
            ClearVisuals();
            visuals = new GameObject("Intestine transit warning and convoy");
            visuals.transform.SetParent(transform, false);
            if (whiteSprite == null)
            {
                whiteTexture = new Texture2D(1, 1);
                whiteTexture.SetPixel(0, 0, Color.white);
                whiteTexture.Apply();
                whiteSprite = Sprite.Create(whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
            }
            AddMarker((start + end) * 0.5f, new Vector2(length, HalfWidth * 2), 0);
            AddMarker((start + end) * 0.5f + across * HalfWidth, new Vector2(length, 0.06f), 0);
            AddMarker((start + end) * 0.5f - across * HalfWidth, new Vector2(length, 0.06f), 0);
            for (float x = 1; x < length - 1; x += 2.5f)
            {
                Vector3 point = start + direction * x;
                AddMarker(point + across * 0.2f, new Vector2(0.65f, 0.1f), -40);
                AddMarker(point - across * 0.2f, new Vector2(0.65f, 0.1f), 40);
            }
            int rendererCount = Mathf.Min(transitCarriageCount,
                Mathf.CeilToInt((length + HalfLength * 2f) / CarSpacing) + 2);
            for (int i = 0; i < rendererCount; i++)
            {
                var car = new GameObject("Bolus " + (i + 1));
                car.transform.SetParent(visuals.transform, false);
                SpriteRenderer renderer = car.AddComponent<SpriteRenderer>();
                renderer.sprite = bolusSprite;
                SpriteYSort sorter = car.AddComponent<SpriteYSort>();
                sorter.Configure(SpriteYSort.WorldDynamicBaseSortingOrder, true, SpriteYSort.WorldDynamicMinSortingOrder);
                sorter.SetUpdateMode(SpriteYSort.UpdateMode.Continuous);
                cars.Add(car.transform);
                car.SetActive(false);
            }
        }

        private void AddMarker(Vector3 position, Vector2 size, float angle)
        {
            var marker = new GameObject("Warning");
            marker.transform.SetParent(visuals.transform, false);
            marker.transform.position = position + Vector3.up * 0.08f;
            float headingAngle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            marker.transform.rotation = Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0, 0, headingAngle + angle);
            marker.transform.localScale = new Vector3(size.x, size.y, 1);
            SpriteRenderer renderer = marker.AddComponent<SpriteRenderer>();
            renderer.sprite = whiteSprite;
            renderer.sortingOrder = 150;
            renderer.color = new Color(1, 0.28f, 0.08f, 0.4f);
            warnings.Add(renderer);
        }

        private void ClearVisuals()
        {
            if (visuals != null) { visuals.SetActive(false); Destroy(visuals); }
            warnings.Clear();
            cars.Clear();
        }

        private void CancelCycle()
        {
            ClearVisuals();
            phase = Phase.Rest;
            timer = restDuration;
        }

        private void OnDisable() => CancelCycle();
        private void OnDestroy()
        {
            if (whiteSprite != null) Destroy(whiteSprite);
            if (whiteTexture != null) Destroy(whiteTexture);
        }
    }
}
