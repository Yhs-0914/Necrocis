using System.Collections;
using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// Exploration-only cerebral chamber. UV-space collision follows the approved painted map.
    /// No enemy, health, damage, pillar destruction or phase logic is installed here.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public sealed class FinalBossArena : MonoBehaviour
    {
        public static FinalBossArena Instance { get; private set; }

        [SerializeField] private Vector2 worldSize = new Vector2(48f, 38f);
        [SerializeField] private Material backgroundMaterial;
        [SerializeField] private Vector2 spawnUV = new Vector2(0.5f, 0.23f);

        // Counter-clockwise around the actual visible floor, including the southern corridor
        // and the recess occupied by the dormant brain. UV origin is bottom left.
        private static readonly Vector2[] Boundary =
        {
            new Vector2(.45f, .045f), new Vector2(.55f, .045f),
            new Vector2(.55f, .16f), new Vector2(.80f, .18f),
            new Vector2(.88f, .27f), new Vector2(.92f, .43f),
            new Vector2(.90f, .66f), new Vector2(.82f, .77f),
            new Vector2(.63f, .80f), new Vector2(.61f, .76f),
            new Vector2(.39f, .76f), new Vector2(.37f, .80f),
            new Vector2(.18f, .77f), new Vector2(.10f, .66f),
            new Vector2(.08f, .43f), new Vector2(.12f, .27f),
            new Vector2(.20f, .18f), new Vector2(.45f, .16f)
        };

        // Whole painted silhouettes are reserved so a player cannot walk over the baked organs.
        private static readonly Vector2[] PillarCenters =
        {
            new Vector2(.247f, .737f), new Vector2(.750f, .729f),
            new Vector2(.262f, .346f), new Vector2(.747f, .350f)
        };
        private static readonly Vector2[] PillarRadii =
        {
            new Vector2(.057f, .128f), new Vector2(.057f, .122f),
            new Vector2(.066f, .116f), new Vector2(.058f, .124f)
        };
        private static readonly string[] PillarNames = { "Stomach", "Intestine", "Liver", "Lung" };

        public Vector2 WorldSize => worldSize;
        public Vector3 SpawnPosition => UVToWorld(spawnUV);
        public Vector3 ReturnPosition => UVToWorld(new Vector2(.5f, .10f));
        public int PillarCount => PillarCenters.Length;

        private void Awake()
        {
            Instance = this;
        }

        private IEnumerator Start()
        {
            // Let the player, camera and optional direct-scene bootstrap finish initialization.
            yield return null;
            PlayerController player = PlayerController.Instance;
            if (player == null)
            {
                Debug.LogError("[FinalBossArena] No player. Enter from Hub or rebuild the arena scene.");
                yield break;
            }

            player.LockY(0f);
            player.SpawnAt(SpawnPosition);
            Physics.SyncTransforms();
            if (GameManager.Instance != null)
                GameManager.Instance.SetGameState(GameState.InFinalBoss);
            DontStarveCamera camera = DontStarveCamera.Instance;
            if (camera != null)
            {
                camera.SetTarget(player.transform);
                camera.SnapToTarget();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void LateUpdate()
        {
            Camera camera = DontStarveCamera.GetActiveCamera();
            if (camera == null || !camera.orthographic || camera.transform.forward.y >= -.01f) return;
            // Keep the camera's ground footprint on the painted room while allowing normal zoom.
            Vector3 position = camera.transform.position;
            Vector3 groundCenter = position + camera.transform.forward
                * ((transform.position.y - position.y) / camera.transform.forward.y);
            float halfWidth = Mathf.Min(worldSize.x * .5f, camera.orthographicSize * camera.aspect);
            float halfDepth = Mathf.Min(worldSize.y * .5f, camera.orthographicSize / -camera.transform.forward.y);
            Vector3 clamped = groundCenter;
            clamped.x = Mathf.Clamp(groundCenter.x, transform.position.x + halfWidth, transform.position.x + worldSize.x - halfWidth);
            clamped.z = Mathf.Clamp(groundCenter.z, transform.position.z + halfDepth, transform.position.z + worldSize.y - halfDepth);
            camera.transform.position += clamped - groundCenter;
        }

        public Vector3 UVToWorld(Vector2 uv)
        {
            return transform.position + new Vector3(uv.x * worldSize.x, 0f, uv.y * worldSize.y);
        }

        public Vector3 GetPillarPosition(int index) => UVToWorld(PillarCenters[index]);

        public bool CanTraverse(Vector3 from, Vector3 to, Vector2 halfExtents)
        {
            // Swept sampling also covers the full dash/knockback path, not only its endpoint.
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(from, to) / .15f));
            for (int i = 0; i <= steps; i++)
                if (!IsWalkable(Vector3.Lerp(from, to, (float)i / steps), halfExtents))
                    return false;
            return true;
        }

        public bool IsWalkable(Vector3 position, Vector2 halfExtents)
        {
            // Nine samples enclose the same footprint used by the existing biome terrain motor.
            for (int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z++)
                    if (!IsFloorPoint(position + new Vector3(x * halfExtents.x, 0f, z * halfExtents.y)))
                        return false;
            return true;
        }

        private bool IsFloorPoint(Vector3 world)
        {
            Vector3 local = world - transform.position;
            Vector2 p = new Vector2(local.x / worldSize.x, local.z / worldSize.y);
            bool inside = false;
            for (int i = 0, j = Boundary.Length - 1; i < Boundary.Length; j = i++)
            {
                Vector2 a = Boundary[i], b = Boundary[j];
                if ((a.y > p.y) != (b.y > p.y)
                    && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x)
                    inside = !inside;
            }
            if (!inside) return false;
            for (int i = 0; i < PillarCenters.Length; i++)
            {
                Vector2 d = p - PillarCenters[i];
                d = new Vector2(d.x / PillarRadii[i].x, d.y / PillarRadii[i].y);
                if (d.sqrMagnitude <= 1f) return false;
            }
            return true;
        }

#if UNITY_EDITOR
        // Explicit editor builder; no destructive ExecuteAlways regeneration of authored scenes.
        public void BuildLayout(Material material)
        {
            backgroundMaterial = material;
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
            visual.name = "DormantCerebrum_MapArtwork";
            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = new Vector3(worldSize.x * .5f, -.05f, worldSize.y * .5f);
            visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            visual.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1f);
            DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<MeshRenderer>().sharedMaterial = material;

            Transform walls = new GameObject("OrganicBoundary_Colliders").transform;
            walls.SetParent(transform, false);
            for (int i = 0; i < Boundary.Length; i++)
                AddEdge(walls, $"Boundary_{i:00}", Boundary[i], Boundary[(i + 1) % Boundary.Length]);

            Transform pillars = new GameObject("FourBiomePillars_Inert").transform;
            pillars.SetParent(transform, false);
            for (int i = 0; i < PillarCenters.Length; i++)
            {
                Transform pillar = new GameObject($"Pillar_{PillarNames[i]}").transform;
                pillar.SetParent(pillars, false);
                for (int edge = 0; edge < 24; edge++)
                {
                    float a = edge * Mathf.PI * 2f / 24;
                    float b = (edge + 1) * Mathf.PI * 2f / 24;
                    Vector2 p = PillarCenters[i] + Vector2.Scale(PillarRadii[i], new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
                    Vector2 q = PillarCenters[i] + Vector2.Scale(PillarRadii[i], new Vector2(Mathf.Cos(b), Mathf.Sin(b)));
                    AddEdge(pillar, $"Collider_{edge:00}", p, q);
                }
            }

            GameObject exit = new GameObject("ReturnToHub_Entrance");
            exit.transform.SetParent(transform, false);
            exit.transform.position = ReturnPosition;
            BoxCollider trigger = exit.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = Vector3.up;
            trigger.size = new Vector3(3.2f, 4f, 1.5f);
            exit.AddComponent<ReturnPortal>().SetActive(true);
        }

        private void AddEdge(Transform parent, string objectName, Vector2 a, Vector2 b)
        {
            Vector3 start = UVToWorld(a), end = UVToWorld(b);
            GameObject wall = new GameObject(objectName);
            wall.transform.SetParent(parent, false);
            wall.transform.position = (start + end) * .5f + Vector3.up * 2f;
            wall.transform.rotation = Quaternion.LookRotation(end - start);
            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.size = new Vector3(.16f, 6f, Vector3.Distance(start, end) + .10f);
        }
#endif
    }
}
