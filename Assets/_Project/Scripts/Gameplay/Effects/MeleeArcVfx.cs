using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    internal sealed class MeleeArcVfx : MonoBehaviour
    {
        private const int PointCount = 15;

        private LineRenderer outerLine;
        private LineRenderer innerLine;
        private RuntimePoolAutoReturn autoReturn;
        private readonly Vector3[] points = new Vector3[PointCount];

        private Vector3 origin;
        private Vector3 forward;
        private float radius;
        private float duration;
        private float elapsed;

        public static GameObject CreateObject()
        {
            GameObject root = new GameObject("MeleeArcFx");
            root.SetActive(false);

            MeleeArcVfx effect = root.AddComponent<MeleeArcVfx>();
            effect.outerLine = CreateLine(root.transform, "Outer", 0.24f, 5160);
            effect.innerLine = CreateLine(root.transform, "Inner", 0.075f, 5161);
            effect.autoReturn = RuntimePool.EnsureAutoReturn(root);
            return root;
        }

        public void Show(Vector3 worldOrigin, Vector3 attackDirection, float attackRadius, float lifeTime)
        {
            EnsureComponents();
            origin = worldOrigin + Vector3.up * 0.35f;
            forward = attackDirection;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }
            forward.Normalize();

            radius = Mathf.Max(0.65f, attackRadius);
            duration = Mathf.Max(0.08f, lifeTime);
            elapsed = 0f;
            outerLine.enabled = true;
            innerLine.enabled = true;
            UpdateLines(0f);
            autoReturn.Schedule(duration);
            enabled = true;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            UpdateLines(t);
            if (t >= 1f)
            {
                RuntimePool.Release(gameObject);
            }
        }

        private void UpdateLines(float t)
        {
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            float currentRadius = radius * Mathf.Lerp(0.48f, 1.08f, eased);
            float sweepOffset = Mathf.Lerp(-16f, 10f, eased);

            for (int i = 0; i < PointCount; i++)
            {
                float normalized = i / (PointCount - 1f);
                float angle = Mathf.Lerp(-68f, 68f, normalized) + sweepOffset;
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                float taperedRadius = currentRadius * Mathf.Lerp(0.86f, 1f, Mathf.Sin(normalized * Mathf.PI));
                points[i] = origin + direction * taperedRadius;
            }

            outerLine.SetPositions(points);
            innerLine.SetPositions(points);
            float fade = Mathf.Pow(1f - t, 1.6f);
            outerLine.startColor = new Color(0.44f, 0.015f, 0.045f, fade * 0.88f);
            outerLine.endColor = new Color(0.95f, 0.06f, 0.16f, fade * 0.25f);
            innerLine.startColor = new Color(1f, 0.92f, 0.72f, fade);
            innerLine.endColor = new Color(1f, 0.22f, 0.18f, fade * 0.35f);
            outerLine.widthMultiplier = Mathf.Lerp(1f, 0.28f, t);
            innerLine.widthMultiplier = Mathf.Lerp(1f, 0.15f, t);
        }

        private void EnsureComponents()
        {
            if (outerLine == null)
            {
                outerLine = transform.Find("Outer")?.GetComponent<LineRenderer>();
            }
            if (innerLine == null)
            {
                innerLine = transform.Find("Inner")?.GetComponent<LineRenderer>();
            }
            if (autoReturn == null)
            {
                autoReturn = RuntimePool.EnsureAutoReturn(gameObject);
            }
        }

        private static LineRenderer CreateLine(Transform parent, string objectName, float width, int sortingOrder)
        {
            GameObject lineObject = new GameObject(objectName);
            lineObject.transform.SetParent(parent, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.positionCount = PointCount;
            line.useWorldSpace = true;
            line.loop = false;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Stretch;
            line.numCapVertices = 3;
            line.numCornerVertices = 2;
            line.startWidth = width;
            line.endWidth = width * 0.22f;
            line.sortingOrder = sortingOrder;
            line.sharedMaterial = CombatVfxResources.GetLineMaterial();
            line.enabled = false;
            return line;
        }
    }
}
