using UnityEngine;
using UnityEngine.Rendering;

namespace Necrocis
{
    /// <summary>Runtime floor warning shared by every final-boss phase-two attack.</summary>
    [DisallowMultipleComponent]
    public sealed class FinalBossTelegraph : MonoBehaviour
    {
        private static Mesh circleMesh;
        private static Mesh rectangleMesh;
        private static Material sharedMaterial;

        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock propertyBlock;
        private Color color;
        private float duration;
        private float elapsed;
        private bool impact;
        private LineRenderer boundary;
        private LineRenderer countdown;
        private bool neuralStyle;
        private bool circleShape;
        private float countdownDuration;

        public void UseNeuralStyle(float windup)
        {
            neuralStyle = true;
            countdownDuration = Mathf.Max(.01f, windup);
            color.a = .16f;
            ApplyColor(color);
            if (boundary != null)
            {
                boundary.widthMultiplier = .055f;
                boundary.startColor = boundary.endColor = new Color(1f, .15f, .2f, .38f);
            }
            var go = new GameObject("ClosingWarningOutline");
            go.transform.SetParent(transform, false);
            countdown = go.AddComponent<LineRenderer>();
            countdown.sharedMaterial = GetMaterial(); countdown.useWorldSpace = false;
            countdown.positionCount = 0;
            countdown.widthMultiplier = .10f; countdown.sortingOrder = 4083;
            countdown.numCapVertices = 2; countdown.shadowCastingMode = ShadowCastingMode.Off;
        }

        public static FinalBossTelegraph CreateCircle(
            Transform parent,
            Vector3 center,
            float radius,
            float lifeTime,
            Color warningColor,
            bool isImpact = false)
        {
            return Create(
                parent,
                isImpact ? "FinalBoss_ImpactCircle" : "FinalBoss_WarningCircle",
                center,
                new Vector3(Mathf.Max(.05f, radius), 1f, Mathf.Max(.05f, radius)),
                GetCircleMesh(),
                lifeTime,
                warningColor,
                isImpact);
        }

        public static FinalBossTelegraph CreateRectangle(
            Transform parent,
            Vector3 center,
            Vector2 size,
            float lifeTime,
            Color warningColor,
            bool isImpact = false)
        {
            return Create(
                parent,
                isImpact ? "FinalBoss_ImpactArea" : "FinalBoss_WarningArea",
                center,
                new Vector3(Mathf.Max(.05f, size.x), 1f, Mathf.Max(.05f, size.y)),
                GetRectangleMesh(),
                lifeTime,
                warningColor,
                isImpact);
        }

        private static FinalBossTelegraph Create(
            Transform parent,
            string objectName,
            Vector3 center,
            Vector3 scale,
            Mesh mesh,
            float lifeTime,
            Color warningColor,
            bool isImpact)
        {
            GameObject go = new GameObject(objectName);
            go.transform.SetParent(parent, true);
            go.transform.position = center;
            go.transform.localScale = scale;

            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = GetMaterial();
            renderer.sortingOrder = isImpact ? 4090 : 4080;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            FinalBossTelegraph warning = go.AddComponent<FinalBossTelegraph>();
            warning.meshRenderer = renderer;
            warning.color = warningColor;
            warning.duration = Mathf.Max(.02f, lifeTime);
            warning.impact = isImpact;
            warning.CreateBoundary(mesh == GetCircleMesh());
            warning.ApplyColor(warningColor);
            return warning;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            if (neuralStyle)
            {
                float charge = Mathf.Clamp01(elapsed / countdownDuration);
                Color fill = color; fill.a *= .6f + charge * .4f;
                ApplyColor(fill);
                boundary.startColor = boundary.endColor = new Color(1f, .15f, .2f, .38f);
                int count = Mathf.Max(2, Mathf.CeilToInt(charge * 64f) + 1);
                countdown.positionCount = count;
                for (int i = 0; i < count; i++)
                    countdown.SetPosition(i, WarningPerimeter(charge * i / (count - 1)));
                countdown.startColor = countdown.endColor = Color.Lerp(new Color(1f, .5f, .25f, .85f),
                    new Color(1f, .18f, .24f, 1f), charge);
                if (elapsed >= duration) Destroy(gameObject);
                return;
            }
            float pulse = impact
                ? 1f - normalized
                : Mathf.Lerp(.42f, 1f, normalized) * (.82f + Mathf.Sin(elapsed * 17f) * .18f);
            Color current = color;
            current.a *= Mathf.Clamp01(pulse);
            if (!impact)
            {
                current = Color.Lerp(current, new Color(1f, .06f, .03f, current.a), normalized * .72f);
            }
            ApplyColor(current);
            if (boundary != null)
            {
                Color edge = current;
                edge.a = impact ? 1f - normalized : Mathf.Lerp(.65f, 1f, normalized);
                boundary.startColor = boundary.endColor = edge;
            }

            if (elapsed >= duration) Destroy(gameObject);
        }

        public void Dismiss()
        {
            if (this == null) return;
            if (meshRenderer != null) meshRenderer.enabled = false;
            if (boundary != null) boundary.enabled = false;
            if (countdown != null) countdown.enabled = false;
            if (gameObject != null) Destroy(gameObject);
        }

        private void ApplyColor(Color value)
        {
            if (meshRenderer == null) return;
            propertyBlock ??= new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", value);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }

        private void CreateBoundary(bool circle)
        {
            circleShape = circle;
            GameObject edge = new GameObject("DangerBoundary");
            edge.transform.SetParent(transform, false);
            boundary = edge.AddComponent<LineRenderer>();
            boundary.sharedMaterial = GetMaterial();
            boundary.useWorldSpace = false;
            boundary.loop = true;
            boundary.widthMultiplier = .09f;
            boundary.numCornerVertices = 3;
            boundary.sortingOrder = impact ? 4092 : 4082;
            boundary.shadowCastingMode = ShadowCastingMode.Off;
            boundary.receiveShadows = false;
            boundary.positionCount = circle ? 64 : 4;
            if (circle)
            {
                for (int i = 0; i < boundary.positionCount; i++)
                {
                    float angle = i * Mathf.PI * 2f / boundary.positionCount;
                    boundary.SetPosition(i, new Vector3(Mathf.Cos(angle), .015f, Mathf.Sin(angle)));
                }
            }
            else
            {
                boundary.SetPositions(new[] {
                    new Vector3(-.5f, .015f, -.5f), new Vector3(-.5f, .015f, .5f),
                    new Vector3(.5f, .015f, .5f), new Vector3(.5f, .015f, -.5f) });
            }
            boundary.startColor = boundary.endColor = color;
        }

        private Vector3 WarningPerimeter(float t)
        {
            if (circleShape)
            {
                float angle = (t * 360f + 90f) * Mathf.Deg2Rad;
                return new Vector3(Mathf.Cos(angle), .025f, Mathf.Sin(angle));
            }
            float p = Mathf.Clamp01(t) * 4f;
            if (p < 1f) return new Vector3(-.5f, .025f, p - .5f);
            if (p < 2f) return new Vector3(p - 1.5f, .025f, .5f);
            if (p < 3f) return new Vector3(.5f, .025f, 2.5f - p);
            return new Vector3(3.5f - p, .025f, -.5f);
        }

        private static Material GetMaterial()
        {
            if (sharedMaterial != null) return sharedMaterial;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");
            sharedMaterial = new Material(shader)
            {
                name = "FinalBossTelegraphMaterial",
                hideFlags = HideFlags.HideAndDontSave,
                renderQueue = 3100
            };
            sharedMaterial.mainTexture = Texture2D.whiteTexture;
            return sharedMaterial;
        }

        private static Mesh GetRectangleMesh()
        {
            if (rectangleMesh != null) return rectangleMesh;
            rectangleMesh = new Mesh
            {
                name = "FinalBossTelegraphRectangle",
                hideFlags = HideFlags.HideAndDontSave,
                vertices = new[]
                {
                    new Vector3(-.5f, 0f, -.5f), new Vector3(-.5f, 0f, .5f),
                    new Vector3(.5f, 0f, .5f), new Vector3(.5f, 0f, -.5f)
                },
                uv = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right },
                triangles = new[] { 0, 1, 2, 0, 2, 3 }
            };
            rectangleMesh.RecalculateNormals();
            rectangleMesh.RecalculateBounds();
            return rectangleMesh;
        }

        private static Mesh GetCircleMesh()
        {
            if (circleMesh != null) return circleMesh;
            const int segments = 48;
            var vertices = new Vector3[segments + 1];
            var uv = new Vector2[segments + 1];
            var triangles = new int[segments * 3];
            vertices[0] = Vector3.zero;
            uv[0] = new Vector2(.5f, .5f);
            for (int i = 0; i < segments; i++)
            {
                float angle = Mathf.PI * 2f * i / segments;
                float x = Mathf.Cos(angle);
                float z = Mathf.Sin(angle);
                vertices[i + 1] = new Vector3(x, 0f, z);
                uv[i + 1] = new Vector2(x * .5f + .5f, z * .5f + .5f);
                int next = (i + 1) % segments;
                int index = i * 3;
                triangles[index] = 0;
                triangles[index + 1] = i + 1;
                triangles[index + 2] = next + 1;
            }
            circleMesh = new Mesh
            {
                name = "FinalBossTelegraphCircle",
                hideFlags = HideFlags.HideAndDontSave,
                vertices = vertices,
                uv = uv,
                triangles = triangles
            };
            circleMesh.RecalculateNormals();
            circleMesh.RecalculateBounds();
            return circleMesh;
        }
    }
}
