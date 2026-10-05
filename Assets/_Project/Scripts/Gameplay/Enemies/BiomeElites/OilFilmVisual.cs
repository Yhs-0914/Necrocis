using System.Collections.Generic;
using UnityEngine;
namespace Necrocis
{
    // Approved texture pixels stay intact. Only the flexible membrane is laid over
    // the shared ground sector; the rolled core retains its original billboard size.
    [DefaultExecutionOrder(250)]
    public sealed class OilFilmVisual : MonoBehaviour
    {
        private const float AuthoredForeshortening = .70710678f;
        private const int Columns = 40, Rows = 32;
        private SpriteRenderer body;
        private EnemyController enemy;
        private OilFilmElitePattern pattern;
        private readonly Dictionary<Sprite, OilFilmFrameProfile> profiles = new Dictionary<Sprite, OilFilmFrameProfile>();
        private Mesh coreMesh, filmMesh;
        private MeshRenderer coreRenderer, filmRenderer;
        private MaterialPropertyBlock properties;
        private static Material material, filmMaterial;
        private readonly float[] clipLimits = new float[OilFilmSector.Segments + 1];
        private Vector3[] sourceVertices, vertices;
        private Sprite builtSprite;
        private OilFilmFrameProfile current;
        public bool IsSurfaceVisible => filmRenderer != null && filmRenderer.enabled;
        public bool IsUnfolded => IsSurfaceVisible && current != null && current.unfold;
        public OilFilmFrameProfile CurrentProfile => current;
        public Vector3 CoreGroundPosition { get; private set; }
        public void Configure(EnemyController source, OilFilmPresentation art, OilFilmElitePattern owner)
        {
            enemy = source; pattern = owner; body = source.transform.Find("Visual").GetComponent<SpriteRenderer>();
            body.GetComponent<SpriteYSort>()?.SetSortingAnchor(source.GetComponent<Rigidbody>());
            profiles.Clear(); foreach (var p in art.profiles) profiles.Add(p.sprite, p);
            if (coreRenderer == null)
            {
                if (material == null) material = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave };
                coreRenderer = MakePart("OilFilmOriginalCore", out coreMesh); filmRenderer = MakePart("OilFilmOriginalMembrane", out filmMesh);
                var shader = art.membraneShader;
                if (shader == null) throw new System.InvalidOperationException("OilFilm terrain shader missing");
                if (filmMaterial == null || filmMaterial.shader != shader) filmMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                filmRenderer.sharedMaterial = filmMaterial;
                properties = new MaterialPropertyBlock();
            }
            builtSprite = null; enabled = true; RestoreSprite(); LateUpdate();
        }
        private MeshRenderer MakePart(string name, out Mesh mesh)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false);
            mesh = new Mesh { name = name, hideFlags = HideFlags.HideAndDontSave }; mesh.MarkDynamic();
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false; renderer.enabled = false;
            return renderer;
        }
        private void LateUpdate()
        {
            if (body == null || body.sprite == null || pattern == null || enemy == null || !profiles.TryGetValue(body.sprite, out current)) return;
            var camera = DontStarveCamera.GetActiveCamera(); if (camera == null || camera.transform.forward.y >= -.1f) return;
            CoreGroundPosition = pattern.GroundOrigin + Vector3.up * .055f;
            body.transform.rotation = camera.transform.rotation;
            Vector3 position = CoreGroundPosition;
            float lowest = position.y + body.sprite.bounds.min.y * camera.transform.up.y * body.transform.lossyScale.y;
            if (lowest < CoreGroundPosition.y) position += camera.transform.forward * ((CoreGroundPosition.y - lowest) / camera.transform.forward.y);
            body.transform.position = position;
            bool attackActive = pattern.Phase == OilFilmPhase.Windup || pattern.IsUnfolding;
            bool useSurface = pattern.Footprint != null && !enemy.IsDead && attackActive
                && (current.unfold || pattern.Footprint.TerrainClipped);
            if (!useSurface) { RestoreSprite(); return; }
            if (builtSprite != body.sprite) Build(body.sprite);
            for (int i = 0; i < vertices.Length; i++) vertices[i] = transform.InverseTransformPoint(MapSourcePoint(sourceVertices[i], camera));
            coreMesh.vertices = vertices; filmMesh.vertices = vertices; coreMesh.RecalculateBounds(); filmMesh.RecalculateBounds();
            properties.Clear(); properties.SetTexture("_MainTex", body.sprite.texture); properties.SetColor("_Color", body.color); properties.SetColor("_RendererColor", Color.white);
            coreRenderer.SetPropertyBlock(properties);
            var footprint = pattern.Footprint; footprint.CopyLimits(clipLimits);
            properties.SetVector("_ClipOrigin", new Vector4(footprint.Origin.x, footprint.Origin.z, 0, 0));
            properties.SetVector("_ClipAxes", new Vector4(footprint.Forward.x, footprint.Forward.z, footprint.Right.x, footprint.Right.z));
            properties.SetFloat("_ClipArc", footprint.Arc * Mathf.Deg2Rad); properties.SetFloatArray("_Reach", clipLimits);
            filmRenderer.SetPropertyBlock(properties);
            coreRenderer.sortingLayerID = body.sortingLayerID; coreRenderer.sortingOrder = body.sortingOrder;
            filmRenderer.sortingOrder = 60; coreRenderer.enabled = filmRenderer.enabled = true; body.enabled = false;
        }
        private Vector2 Native(Vector3 local)
        {
            if (current.view == 0) return new Vector2(local.x, -local.y / AuthoredForeshortening);
            if (current.view == 1) return new Vector2(-local.y / AuthoredForeshortening, -local.x);
            return new Vector2(local.y / AuthoredForeshortening, local.x);
        }
        private Vector3 FromNative(float forward, float lateral)
        {
            if (current.view == 0) return new Vector3(forward, -lateral * AuthoredForeshortening, 0);
            if (current.view == 1) return new Vector3(-lateral, -forward * AuthoredForeshortening, 0);
            return new Vector3(lateral, forward * AuthoredForeshortening, 0);
        }
        public Vector3 MapSourcePoint(Vector3 local, Camera camera)
        {
            Vector3 original = CoreGroundPosition + camera.transform.right * (local.x * (body.flipX ? -1 : 1)) + camera.transform.up * local.y;
            if (original.y < CoreGroundPosition.y) original += camera.transform.forward * ((CoreGroundPosition.y - original.y) / camera.transform.forward.y);
            Vector2 p = Native(local);
            if (!current.unfold || p.x <= current.coreCut || pattern.Footprint == null) return original;
            float angle = Mathf.Atan2(p.y, p.x);
            float u = Mathf.InverseLerp(current.angleMin, current.angleMax, angle);
            float sample = u * (current.rimRadii.Length - 1); int lo = Mathf.Min(current.rimRadii.Length - 2, Mathf.FloorToInt(sample));
            float radius = Mathf.Lerp(current.rimRadii[lo], current.rimRadii[lo + 1], sample - lo);
            float t = Mathf.Clamp01(p.magnitude / radius) * Mathf.Clamp01(current.reachFraction);
            if (body.flipX && current.view == 0) u = 1 - u;
            Vector3 target = pattern.Footprint.UnclippedPoint(u, t);
            // Keep approved pixels on the source plane, then discard occluded fragments.
            // Shrinking the UV surface would squeeze a broad sheet into thin spokes at a wall.
            target.y = pattern.Footprint.Origin.y + .055f;
            float blend = Mathf.SmoothStep(0, 1, (p.x - current.coreCut) / (current.coreCut * .5f));
            return Vector3.Lerp(original, target, blend);
        }
        public Vector3 VisibleRimSample(float u)
        {
            float sample = Mathf.Clamp01(u) * (current.rimRadii.Length - 1); int lo = Mathf.Min(current.rimRadii.Length - 2, Mathf.FloorToInt(sample));
            float r = Mathf.Lerp(current.rimRadii[lo], current.rimRadii[lo + 1], sample - lo);
            float angle = Mathf.Lerp(current.angleMin, current.angleMax, u);
            Vector3 point = MapSourcePoint(FromNative(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r), DontStarveCamera.GetActiveCamera());
            float actualU = body.flipX && current.view == 0 ? 1 - u : u;
            float reach = pattern.Footprint.RadiusAtAngle(actualU);
            float visible = Mathf.Min(pattern.Footprint.Range * current.reachFraction, reach);
            Vector3 edge = pattern.Footprint.UnclippedPoint(actualU, visible / pattern.Footprint.Range);
            edge.y = pattern.Footprint.Origin.y + .055f;
            return current.unfold ? edge : point;
        }
        private void Build(Sprite sprite)
        {
            builtSprite = sprite; coreMesh.Clear(); filmMesh.Clear();
            int n = (Columns + 1) * (Rows + 1); sourceVertices = new Vector3[n]; vertices = new Vector3[n];
            var uv = new Vector2[n]; var colors = new Color[n]; var core = new List<int>(); var film = new List<int>();
            Rect r = sprite.rect; Vector2 pivot = sprite.pivot; float ppu = sprite.pixelsPerUnit;
            for (int y = 0; y <= Rows; y++) for (int x = 0; x <= Columns; x++)
            {
                int i = y * (Columns + 1) + x; float px = x / (float)Columns * r.width, py = y / (float)Rows * r.height;
                sourceVertices[i] = new Vector3((px - pivot.x) / ppu, (py - pivot.y) / ppu, 0);
                uv[i] = new Vector2((r.x + px) / sprite.texture.width, (r.y + py) / sprite.texture.height); colors[i] = Color.white;
            }
            void Triangle(int a, int b, int c)
            {
                var target = Native((sourceVertices[a] + sourceVertices[b] + sourceVertices[c]) / 3).x <= current.coreCut ? core : film;
                target.Add(a); target.Add(b); target.Add(c);
            }
            for (int y = 0; y < Rows; y++) for (int x = 0; x < Columns; x++)
            {
                int i = y * (Columns + 1) + x; Triangle(i, i + 1, i + Columns + 2); Triangle(i, i + Columns + 2, i + Columns + 1);
            }
            foreach (var mesh in new[] { coreMesh, filmMesh }) { mesh.vertices = vertices; mesh.uv = uv; mesh.colors = colors; }
            coreMesh.triangles = core.ToArray(); filmMesh.triangles = film.ToArray();
        }
        public void RestoreSprite()
        {
            if (body != null) body.enabled = true;
            if (coreRenderer != null) coreRenderer.enabled = false;
            if (filmRenderer != null) filmRenderer.enabled = false;
        }
        private void OnDisable() => RestoreSprite();
        private void OnDestroy() { if (coreMesh != null) Destroy(coreMesh); if (filmMesh != null) Destroy(filmMesh); }
    }
}
