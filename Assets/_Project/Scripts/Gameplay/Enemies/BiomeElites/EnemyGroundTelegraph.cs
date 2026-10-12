using UnityEngine;

namespace Necrocis
{
    /// <summary>Flat translucent damage area, with geometry taken directly from the attack footprint. No collider.</summary>
    public static class EnemyGroundTelegraph
    {
        private static Mesh circle, rectangle;
        private static Material material;
        private static MaterialPropertyBlock colorBlock;

        public static MeshRenderer Circle(Transform parent, float radius, Color color)
        {
            if (circle == null)
            {
                const int segments = 96;
                var vertices = new Vector3[segments + 1]; var uv = new Vector2[vertices.Length]; var triangles = new int[segments * 3];
                for (int i = 0; i < segments; i++)
                {
                    float angle = i * Mathf.PI * 2 / segments;
                    vertices[i + 1] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                    triangles[i * 3] = 0; triangles[i * 3 + 1] = 1 + (i + 1) % segments; triangles[i * 3 + 2] = i + 1;
                }
                circle = Build("DangerCircle", vertices, uv, triangles);
            }
            return Create(parent, circle, new Vector3(radius, 1, radius), color);
        }

        public static MeshRenderer Rectangle(Transform parent, Vector2 size, Color color)
        {
            if (rectangle == null)
                rectangle = Build("DangerRectangle", new[] { new Vector3(-.5f, 0, -.5f), new Vector3(.5f, 0, -.5f),
                    new Vector3(.5f, 0, .5f), new Vector3(-.5f, 0, .5f) }, new Vector2[4], new[] { 0, 2, 1, 0, 3, 2 });
            return Create(parent, rectangle, new Vector3(size.x, 1, size.y), color);
        }

        private static Mesh Build(string name, Vector3[] vertices, Vector2[] uv, int[] triangles)
        {
            var colors = new Color[vertices.Length]; for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
            var mesh = new Mesh { name = name, hideFlags = HideFlags.HideAndDontSave, vertices = vertices, uv = uv, colors = colors, triangles = triangles };
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        private static MeshRenderer Create(Transform parent, Mesh mesh, Vector3 scale, Color color)
        {
            if (material == null) material = new Material(Shader.Find("Sprites/Default"))
            { hideFlags = HideFlags.HideAndDontSave, mainTexture = Texture2D.whiteTexture };
            var go = new GameObject("DamageArea"); go.transform.SetParent(parent, false); go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material; renderer.sortingOrder = 70;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false;
            SetColor(renderer, color); return renderer;
        }

        public static void SetColor(Renderer renderer, Color color)
        {
            colorBlock ??= new MaterialPropertyBlock(); colorBlock.Clear(); colorBlock.SetColor("_Color", color);
            colorBlock.SetColor("_RendererColor", Color.white); renderer.SetPropertyBlock(colorBlock);
        }
    }
}
