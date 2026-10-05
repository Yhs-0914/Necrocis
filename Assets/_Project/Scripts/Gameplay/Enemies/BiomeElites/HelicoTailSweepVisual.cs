using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    // The original PNG stays untouched. Only active-sweep frames split the body
    // mesh from the flags. The flags swing in the air; only their projection is on the ground.
    [DefaultExecutionOrder(250)]
    public sealed class HelicoTailSweepVisual : MonoBehaviour
    {
        private SpriteRenderer body;
        private HelicoSpiralElitePattern pattern;
        private MeshRenderer clippedBody;
        private MeshFilter bodyMesh;
        private GameObject root;
        private readonly Dictionary<Sprite, HelicoTailBodyFrame> frames = new Dictionary<Sprite, HelicoTailBodyFrame>();
        private readonly Dictionary<Sprite, Mesh> meshes = new Dictionary<Sprite, Mesh>();
        private LineRenderer[] flags;
        private Material material;
        private MaterialPropertyBlock block;
        public Vector3 VisibleTip { get; private set; }
        public bool IsShowing => root != null && root.activeSelf;
        public void Configure(SpriteRenderer renderer, HelicoSpiralPresentation art, HelicoSpiralElitePattern owner)
        {
            Clear(); body = renderer; pattern = owner;
            if (art.tailBodies == null) return;
            foreach (var frame in art.tailBodies) frames[frame.sprite] = frame;
            material = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave, mainTexture = Texture2D.whiteTexture };
            block = new MaterialPropertyBlock();
            root = new GameObject("S01_AirborneTailAction"); root.transform.SetParent(body.transform, false);
            bodyMesh = root.AddComponent<MeshFilter>(); clippedBody = root.AddComponent<MeshRenderer>(); clippedBody.sharedMaterial = material;
            clippedBody.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; clippedBody.receiveShadows = false;
            flags = new LineRenderer[6];
            for (int i = 0; i < flags.Length; i++)
            {
                var line = new GameObject("Flag" + (i / 3) + "Layer" + (i % 3)).AddComponent<LineRenderer>(); line.transform.SetParent(root.transform, false);
                line.useWorldSpace = true; line.alignment = LineAlignment.View; line.sharedMaterial = material; line.positionCount = 9 + HelicoTailSweepGeometry.CurveSegments; line.numCapVertices = 2; line.numCornerVertices = 1;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; line.receiveShadows = false;
                Color c = i % 3 == 0 ? new Color(.14f,.025f,.12f) : i % 3 == 1 ? new Color(.94f,.43f,.59f) : new Color(1,.73f,.7f);
                line.startColor = line.endColor = c; flags[i] = line;
            }
            root.SetActive(false); enabled = true;
        }
        private Mesh GetMesh(HelicoTailBodyFrame frame)
        {
            if (meshes.TryGetValue(frame.sprite, out Mesh existing)) return existing;
            var sprite = frame.sprite; var vertices = new Vector3[frame.bodyHull.Length]; var uv = new Vector2[vertices.Length];
            var colors = new Color[vertices.Length]; var triangles = new int[(vertices.Length - 2) * 3];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = frame.bodyHull[i]; colors[i] = Color.white;
                Vector2 pixel = frame.bodyHull[i] * sprite.pixelsPerUnit + sprite.pivot + sprite.rect.position;
                uv[i] = new Vector2(pixel.x / sprite.texture.width, pixel.y / sprite.texture.height);
                if (i >= 2) { int t = (i-2)*3; triangles[t]=0; triangles[t+1]=i-1; triangles[t+2]=i; }
            }
            var mesh = new Mesh { name = "S01_TailBody_" + sprite.name, vertices = vertices, uv = uv, colors = colors, triangles = triangles };
            mesh.RecalculateBounds(); meshes.Add(sprite, mesh); return mesh;
        }
        private void LateUpdate()
        {
            if (root == null || body == null) return;
            if (pattern == null || !pattern.TailActionVisible || pattern.TailGeometry == null || !frames.TryGetValue(body.sprite, out var frame)) { Hide(); return; }
            root.SetActive(true); body.enabled = false;
            root.transform.localScale = new Vector3(body.flipX ? -1 : 1, 1, 1);
            bodyMesh.sharedMesh = GetMesh(frame); clippedBody.sortingLayerID = body.sortingLayerID; clippedBody.sortingOrder = body.sortingOrder;
            block.Clear(); block.SetTexture("_MainTex", body.sprite.texture); block.SetColor("_Color", body.color); clippedBody.SetPropertyBlock(block);
            var g = pattern.TailGeometry; float p = pattern.TailProgress;
            Vector3 attachment = body.transform.TransformPoint(new Vector3(frame.attachment.x * (body.flipX ? -1 : 1), frame.attachment.y, 0));
            VisibleTip = g.AirPoint(p, 1);
            for (int i = 0; i < flags.Length; i++)
            {
                bool trailing = i >= 3; int flag = trailing ? 1 : 0; var line = flags[i];
                line.widthMultiplier = g.Radius * 2 * (i % 3 == 0 ? 1 : i % 3 == 1 ? .64f : .19f);
                var camera = DontStarveCamera.GetActiveCamera();
                Vector3 depth = camera != null ? Vector3.ProjectOnPlane(camera.transform.up, Vector3.up).normalized : Vector3.forward;
                line.sortingOrder = body.sortingOrder + (Vector3.Dot(g.Rear, depth) > .1f ? -3 : 1) + i % 3; line.sortingLayerID = body.sortingLayerID;
                var points = new Vector3[9 + HelicoTailSweepGeometry.CurveSegments];
                Vector3 start = attachment + (trailing && camera != null ? camera.transform.right * .025f : Vector3.zero);
                Vector3 joint = g.AirPoint(p, 0, flag);
                Vector3 tangent = (g.AirPoint(p, .04f, flag) - joint).normalized;
                Vector3 control1 = Vector3.Lerp(start, joint, .4f) + Vector3.up * .10f;
                Vector3 control2 = joint - tangent * .12f;
                for (int j=0;j<=8;j++)
                {
                    float t=j/8f,u=1-t;
                    points[j]=u*u*u*start+3*u*u*t*control1+3*u*t*t*control2+t*t*t*joint;
                }
                for (int j=1;j<=HelicoTailSweepGeometry.CurveSegments;j++) points[8+j]=g.AirPoint(p,j/(float)HelicoTailSweepGeometry.CurveSegments,flag);
                line.SetPositions(points);
            }
        }
        public void Hide() { if (root != null) root.SetActive(false); if (body != null) body.enabled = true; }
        private void Clear()
        {
            Hide(); if (root != null) Destroy(root); if (material != null) Destroy(material);
            foreach (var mesh in meshes.Values) Destroy(mesh); meshes.Clear(); frames.Clear();
        }
        private void OnDisable() => Hide();
        private void OnDestroy() => Clear();
    }
}
