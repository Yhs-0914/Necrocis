using UnityEngine;
using UnityEngine.Rendering;

namespace Necrocis
{
    /// <summary>A skinned ribbon: the root and claw retain their proportions while the tendon bends.</summary>
    public sealed class FinalBossTentacleVisual : MonoBehaviour
    {
        private const int Segments = 40;
        private readonly Vector3[] vertices = new Vector3[(Segments + 1) * 2];
        private readonly Vector2[] uv = new Vector2[(Segments + 1) * 2];
        private readonly Color[] colors = new Color[(Segments + 1) * 2];
        private Mesh mesh;
        private MeshRenderer body;
        private static Material tissueMaterial;
        private LineRenderer nerve;
        private readonly Vector3[] nervePoints = new Vector3[25];
        private Camera view;

        public static FinalBossTentacleVisual Create(Transform parent, Camera camera = null)
        {
            var go = new GameObject("PhaseIII_ArticulatedTentacle");
            go.transform.SetParent(parent, false);
            var visual = go.AddComponent<FinalBossTentacleVisual>();
            visual.view = camera;
            visual.Build();
            return visual;
        }

        private void Build()
        {
            Sprite sprite = Resources.Load<Sprite>("FinalBoss/PhaseThree/NeuralLance");
            if (tissueMaterial == null)
            {
                tissueMaterial = new Material(Shader.Find("Sprites/Default"))
                { name = "PhaseIII_TentacleTissue", hideFlags = HideFlags.HideAndDontSave };
                if (sprite != null) tissueMaterial.mainTexture = sprite.texture;
            }
            mesh = new Mesh { name = "PhaseIII_FlexibleTendon" };
            mesh.MarkDynamic();
            var triangles = new int[Segments * 6];
            for (int i = 0; i < Segments; i++)
            {
                int v = i * 2, t = i * 6;
                triangles[t] = v; triangles[t + 1] = v + 1; triangles[t + 2] = v + 2;
                triangles[t + 3] = v + 2; triangles[t + 4] = v + 1; triangles[t + 5] = v + 3;
            }
            mesh.vertices = vertices; mesh.triangles = triangles;
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            body = gameObject.AddComponent<MeshRenderer>();
            body.sharedMaterial = tissueMaterial;
            body.sortingOrder = 5046;
            body.shadowCastingMode = ShadowCastingMode.Off;
            body.receiveShadows = false;
            nerve = FinalBossNeuralVfx.CreateLine(transform, "LivingAxon", .055f, 5047);
            nerve.positionCount = nervePoints.Length;
        }

        public void Pose(Vector3 root, Vector3 tip, float width, float curl, float energy, float alpha = 1f)
        {
            if (mesh == null) return;
            Camera camera = view != null ? view : DontStarveCamera.GetActiveCamera();
            Vector3 forward = camera != null ? camera.transform.forward : new Vector3(0f, -.7f, .7f);
            Vector3 delta = tip - root;
            float length = Mathf.Max(.1f, delta.magnitude);
            Vector3 side = Vector3.Cross(Vector3.up, delta.normalized);
            Vector3 control = (root + tip) * .5f + Vector3.up * Mathf.Min(1.8f, length * .18f) + side * curl;
            for (int i = 0; i <= Segments; i++)
            {
                float t = i / (float)Segments;
                Vector3 p = Curve(root, control, tip, t);
                Vector3 tangent = ((control - root) * (1f - t) + (tip - control) * t).normalized;
                Vector3 across = Vector3.Cross(forward, tangent).normalized;
                if (across.sqrMagnitude < .01f) across = Vector3.up;
                // Cap UVs stop the detailed claw from stretching with the entire attack range.
                float cap = Mathf.Min(.32f, 1.5f / length);
                float u = t < cap ? .23f + t / cap * .12f : t > 1f - cap
                    ? .76f + (t - (1f - cap)) / cap * .24f
                    : .35f + (t - cap) / (1f - cap * 2f) * .41f;
                float halfWidth = width * .68f * Mathf.Lerp(1.1f, .85f, t);
                vertices[i * 2] = transform.InverseTransformPoint(p - across * halfWidth);
                vertices[i * 2 + 1] = transform.InverseTransformPoint(p + across * halfWidth);
                uv[i * 2] = new Vector2(u, 0f); uv[i * 2 + 1] = new Vector2(u, 1f);
                Color tint = Color.Lerp(new Color(.82f, .63f, .72f, alpha), new Color(1f, .96f, 1f, alpha), energy);
                tint.a *= Mathf.Clamp01(t * 14f);
                colors[i * 2] = colors[i * 2 + 1] = tint;
            }
            mesh.vertices = vertices; mesh.uv = uv; mesh.colors = colors; mesh.RecalculateBounds();
            for (int i = 0; i < nervePoints.Length; i++)
            {
                float t = i / (float)(nervePoints.Length - 1) * .88f;
                nervePoints[i] = Curve(root, control, tip, t) - forward * .04f
                    + side * Mathf.Sin(t * 36f - Time.time * 18f) * .055f * Mathf.Sin(t * Mathf.PI);
            }
            nerve.SetPositions(nervePoints);
            nerve.startColor = new Color(.18f, .62f, .75f, energy * alpha * .65f);
            nerve.endColor = new Color(.55f, 1f, 1f, energy * alpha);
            nerve.widthMultiplier = Mathf.Lerp(.025f, .075f, energy);
        }

        private static Vector3 Curve(Vector3 a, Vector3 b, Vector3 c, float t)
            => (1f - t) * (1f - t) * a + 2f * (1f - t) * t * b + t * t * c;

        private void OnDestroy()
        {
            if (mesh != null) { if (Application.isPlaying) Destroy(mesh); else DestroyImmediate(mesh); }
        }
    }
}
