using UnityEngine;
using UnityEngine.Rendering;

namespace Necrocis
{
    /// <summary>Small reusable mesh effects; no full-screen flashes or per-frame particle allocations.</summary>
    public sealed class FinalBossNeuralVfx : MonoBehaviour
    {
        public enum Shape { Burst, Shield, Halo, Sweep, Boundary }
        private Vector3[] vertices;
        private Vector2[] uv;
        private Color[] colors;
        private int[] indices;
        private Mesh mesh;
        private MeshRenderer rendererComponent;
        private Shape shape;
        private float radius, lifetime, age, sweepStart, sweepAngle = 100f;
        private Color tint;
        private int quadCount;
        private Camera view;
        private Rect bounds;
        private static Material energyMaterial;

        public static Material EnergyMaterial
        {
            get
            {
                if (energyMaterial == null)
                {
                    Shader shader = Resources.Load<Shader>("FinalBoss/PhaseThree/NeuralEnergy");
                    energyMaterial = new Material(shader != null ? shader : Shader.Find("Sprites/Default"))
                    { name = "PhaseIII_NeuralEnergy", hideFlags = HideFlags.HideAndDontSave };
                }
                return energyMaterial;
            }
        }

        public static LineRenderer CreateLine(Transform parent, string name, float width, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = EnergyMaterial;
            line.useWorldSpace = true; line.widthMultiplier = width;
            line.numCapVertices = 3; line.numCornerVertices = 3;
            line.sortingOrder = order; line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            return line;
        }

        public static FinalBossNeuralVfx Create(Transform parent, Vector3 position, Shape form,
            float size, Color color, float duration = 0f, Camera camera = null)
        {
            var go = new GameObject("PhaseIII_" + form);
            go.transform.SetParent(parent, false); go.transform.position = position;
            var effect = go.AddComponent<FinalBossNeuralVfx>();
            effect.shape = form; effect.radius = size; effect.tint = color;
            effect.lifetime = duration; effect.view = camera;
            effect.Build(); effect.RenderAt(0f);
            return effect;
        }

        private void Build()
        {
            int capacity = shape == Shape.Shield ? 800 : shape == Shape.Boundary ? 400 : 256;
            vertices = new Vector3[capacity * 4]; uv = new Vector2[capacity * 4];
            colors = new Color[capacity * 4]; indices = new int[capacity * 6];
            mesh = new Mesh { name = "PhaseIII_EnergyMesh" }; mesh.MarkDynamic();
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            rendererComponent = gameObject.AddComponent<MeshRenderer>();
            rendererComponent.sharedMaterial = EnergyMaterial;
            rendererComponent.sortingOrder = shape == Shape.Boundary ? 4070 : 5060;
            rendererComponent.shadowCastingMode = ShadowCastingMode.Off;
            rendererComponent.receiveShadows = false;
            for (int i = 0; i < capacity; i++)
            {
                int v = i * 4, k = i * 6;
                indices[k] = v; indices[k + 1] = v + 1; indices[k + 2] = v + 2;
                indices[k + 3] = v; indices[k + 4] = v + 2; indices[k + 5] = v + 3;
                uv[v] = new Vector2(0f, 0f); uv[v + 1] = new Vector2(0f, 1f);
                uv[v + 2] = new Vector2(1f, 1f); uv[v + 3] = new Vector2(1f, 0f);
            }
            mesh.vertices = vertices; mesh.uv = uv;
        }

        public void SetSweep(float startDegrees, float arcDegrees)
        { sweepStart = startDegrees; sweepAngle = arcDegrees; }

        public void SetBoundary(Rect area) => bounds = area;

        private void Update()
        {
            if (Time.deltaTime <= 0f) return;
            age += Time.deltaTime;
            if (lifetime > 0f && age >= lifetime) { Destroy(gameObject); return; }
            RenderAt(age);
        }

        // Also used by the editor capture, so previews render the actual runtime geometry.
        public void RenderAt(float seconds)
        {
            if (mesh == null) return;
            quadCount = 0;
            float t = lifetime > 0f ? Mathf.Clamp01(seconds / lifetime) : 0f;
            Camera camera = view != null ? view : DontStarveCamera.GetActiveCamera();
            Vector3 right = camera != null ? camera.transform.right : Vector3.right;
            Vector3 up = camera != null ? camera.transform.up : Vector3.up;
            float opacity = lifetime > 0f ? Mathf.Min(1f, (1f - t) * 3f) : 1f;
            switch (shape)
            {
                case Shape.Burst:
                    float expansion = 1f - Mathf.Pow(1f - t, 3f);
                    Arc(Vector3.zero, Vector3.right, Vector3.forward, radius * (.28f + expansion * .72f),
                        0f, 360f, radius * .13f * (1f - t) + .025f, tint * new Color(1f, 1f, 1f, opacity), 80);
                    Arc(Vector3.up * .13f, Vector3.right, Vector3.forward, radius * (.2f + expansion * .48f),
                        0f, 360f, .07f, new Color(.8f, 1f, 1f, opacity * .45f), 64);
                    Arc(Vector3.up * .35f, right, up, radius * .18f * (1f - t), 0f, 360f,
                        radius * .18f * (1f - t), new Color(.4f, .9f, 1f, opacity * (1f - t) * .7f), 48);
                    for (int i = 0; i < 18; i++)
                    {
                        float angle = i * 2.399963f;
                        Vector3 dir = new Vector3(Mathf.Cos(angle), .3f + (i % 4) * .17f, Mathf.Sin(angle));
                        Vector3 p = dir * radius * expansion * (.55f + (i % 3) * .16f);
                        p.y -= t * t * radius * .65f;
                        float shard = (.07f + radius * .025f) * (1f - t);
                        Quad(p - right * shard, p + up * shard * 3f, p + right * shard,
                            p - up * shard * 2f, new Color(.4f, .85f, 1f, opacity));
                    }
                    break;
                case Shape.Shield:
                    Arc(Vector3.zero, right, up, radius * .5f, 0f, 360f, radius * .5f,
                        new Color(.12f, .55f, .85f, opacity * .075f), 96);
                    // Curving meridians, latitude arcs and a subtle membrane surround the whole body.
                    for (int i = 0; i < 3; i++)
                    {
                        float h = (i - 1) * .5f;
                        float r = radius * Mathf.Sqrt(1f - h * h);
                        Arc(Vector3.up * h * radius, Vector3.right, Vector3.forward, r,
                            seconds * (i % 2 == 0 ? 24f : -19f), 325f, .045f, tint * new Color(1f, 1f, 1f, .5f * opacity), 64);
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        Vector3 axis = Quaternion.Euler(0f, i * 60f + seconds * 22f, 0f) * Vector3.right;
                        Arc(Vector3.zero, axis, Vector3.up, radius, 0f, 360f, .075f, tint * new Color(1f, 1f, 1f, opacity * .75f), 80);
                    }
                    Arc(Vector3.zero, right, up, radius * 1.02f, 0f, 360f, .18f,
                        new Color(.4f, .83f, 1f, .23f * opacity), 96);
                    break;
                case Shape.Halo:
                    Arc(Vector3.zero, right, up, radius * (1f + Mathf.Sin(seconds * 4f) * .035f),
                        seconds * 55f, 270f, .09f, tint * new Color(1f, 1f, 1f, opacity * .55f), 64);
                    Arc(Vector3.zero, right, up, radius * .72f, -seconds * 80f, 230f, .04f, tint, 48);
                    break;
                case Shape.Sweep:
                    for (int i = 0; i < 3; i++)
                        Arc(Vector3.up * (.08f + i * .035f), Vector3.right, Vector3.forward, radius - i * .28f,
                            sweepStart, sweepAngle, .2f - i * .05f, tint * new Color(1f, 1f, 1f, .6f - i * .15f), 72);
                    break;
                case Shape.Boundary:
                    for (int side = 0; side < 4; side++)
                    {
                        Vector3 a = new Vector3(side < 2 ? bounds.xMin : bounds.xMax, 0f,
                            side == 0 || side == 3 ? bounds.yMin : bounds.yMax);
                        Vector3 b = new Vector3(side == 0 || side == 3 ? bounds.xMin : bounds.xMax, 0f,
                            side < 2 ? bounds.yMax : bounds.yMin);
                        for (int layer = 0; layer < 3; layer++)
                        {
                            Vector3 previous = a + Vector3.up * (layer * .42f);
                            for (int j = 1; j <= 32; j++)
                            {
                                float u = j / 32f;
                                Vector3 next = Vector3.Lerp(a, b, u) + Vector3.up *
                                    (layer * .42f + Mathf.Sin(u * 38f + seconds * 3f + side) * .08f);
                                Segment(previous, next, .10f - layer * .025f,
                                    new Color(.18f, .8f, .83f, .85f - layer * .23f), up);
                                previous = next;
                            }
                        }
                    }
                    break;
            }
            mesh.vertices = vertices; mesh.colors = colors;
            mesh.SetTriangles(indices, 0, quadCount * 6, 0, false);
            mesh.RecalculateBounds();
        }

        private void Arc(Vector3 center, Vector3 x, Vector3 y, float r, float start, float extent, float width, Color color, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                float a = (start + extent * i / steps) * Mathf.Deg2Rad;
                float b = (start + extent * (i + 1) / steps) * Mathf.Deg2Rad;
                Vector3 p = x * Mathf.Cos(a) + y * Mathf.Sin(a);
                Vector3 q = x * Mathf.Cos(b) + y * Mathf.Sin(b);
                float fade = Mathf.Abs(extent) < 350f ? Mathf.Clamp01(Mathf.Min(i + 1, steps - i) / 7f) : 1f;
                Color c = color; c.a *= fade;
                Quad(center + p * (r - width), center + p * (r + width),
                    center + q * (r + width), center + q * (r - width), c);
            }
        }

        private void Segment(Vector3 a, Vector3 b, float width, Color color, Vector3 planeUp)
        {
            Vector3 cross = Vector3.Cross((b - a).normalized, planeUp).normalized * width;
            if (cross.sqrMagnitude < .00001f) cross = Vector3.right * width;
            Quad(a - cross, a + cross, b + cross, b - cross, color);
        }

        private void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
        {
            if (quadCount >= indices.Length / 6) return;
            int v = quadCount++ * 4;
            vertices[v] = a; vertices[v + 1] = b; vertices[v + 2] = c; vertices[v + 3] = d;
            colors[v] = colors[v + 1] = colors[v + 2] = colors[v + 3] = color;
        }

        private void OnDestroy()
        { if (mesh != null) { if (Application.isPlaying) Destroy(mesh); else DestroyImmediate(mesh); } }
    }
}
