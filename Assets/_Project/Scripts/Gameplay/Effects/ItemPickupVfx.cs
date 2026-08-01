using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    internal sealed class ItemPickupVfx : MonoBehaviour
    {
        private const int TrailSortingOrder = 5240;
        private const int SparkSortingOrder = 5250;
        private const int FlashSortingOrder = 5260;
        private const int SparkCount = 5;

        private readonly SpriteRenderer[] sparks = new SpriteRenderer[SparkCount];

        private LineRenderer trail;
        private SpriteRenderer sourceFlash;
        private SpriteRenderer targetFlash;
        private RuntimePoolAutoReturn autoReturn;
        private Transform collector;
        private Vector3 collectorOffset;
        private Vector3 startPosition;
        private Color primaryColor;
        private Color accentColor;
        private float duration;
        private float elapsed;
        private float effectScale;

        public static GameObject CreateObject()
        {
            GameObject root = new GameObject("ItemPickupFx");
            root.SetActive(false);

            ItemPickupVfx effect = root.AddComponent<ItemPickupVfx>();
            effect.trail = CreateTrail(root.transform);
            effect.sourceFlash = CreateSpriteRenderer(
                root.transform,
                "PickupSpark",
                CombatVfxResources.GetStarSprite(),
                FlashSortingOrder);
            effect.targetFlash = CreateSpriteRenderer(
                root.transform,
                "CollectorSpark",
                CombatVfxResources.GetStarSprite(),
                FlashSortingOrder + 1);

            for (int i = 0; i < SparkCount; i++)
            {
                effect.sparks[i] = CreateSpriteRenderer(
                    root.transform,
                    $"AbsorbSpark{i + 1}",
                    CombatVfxResources.GetStarSprite(),
                    SparkSortingOrder + i);
            }

            effect.autoReturn = RuntimePool.EnsureAutoReturn(root);
            return root;
        }

        public void Show(
            Vector3 visualPosition,
            Transform collectorTarget,
            Vector3 collectorCenter,
            Color primary,
            Color accent,
            float scale)
        {
            EnsureComponents();

            transform.SetParent(null, false);
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            collector = collectorTarget;
            collectorOffset = collectorTarget != null
                ? collectorCenter - collectorTarget.position
                : Vector3.zero;
            startPosition = visualPosition;
            primaryColor = primary;
            accentColor = accent;
            duration = 0.56f;
            elapsed = 0f;
            effectScale = Mathf.Clamp(scale * 2f, 1.3f, 3f);

            trail.enabled = true;
            trail.positionCount = 7;
            sourceFlash.enabled = true;
            sourceFlash.transform.position = startPosition;
            sourceFlash.transform.localScale = Vector3.one * effectScale * 0.32f;
            sourceFlash.color = WithAlpha(accentColor, 1f);

            targetFlash.enabled = true;
            targetFlash.transform.position = GetCollectorPosition();
            targetFlash.transform.localScale = Vector3.one * 0.02f;
            targetFlash.color = WithAlpha(accentColor, 0f);

            for (int i = 0; i < sparks.Length; i++)
            {
                if (sparks[i] == null)
                {
                    continue;
                }

                sparks[i].enabled = true;
                sparks[i].transform.position = startPosition;
                sparks[i].transform.localScale = Vector3.one * effectScale * (0.15f - i * 0.01f);
                sparks[i].color = WithAlpha(Color.Lerp(primaryColor, accentColor, i * 0.18f), 0f);
            }

            ApplyBillboardOrientation();
            UpdateTrail(startPosition, 0f, 1f);
            autoReturn.Schedule(duration);
            enabled = true;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 targetPosition = GetCollectorPosition();
            float headT = Smooth01(Mathf.Clamp01(t / 0.76f));
            float trailFade = Mathf.Clamp01((0.88f - t) / 0.2f);

            ApplyBillboardOrientation();
            UpdateTrail(targetPosition, headT, trailFade);
            UpdateSourceFlash(t);
            UpdateTravelSparks(t, targetPosition);
            UpdateTargetFlash(t, targetPosition);

            if (t >= 1f)
            {
                RuntimePool.Release(gameObject);
            }
        }

        private void UpdateSourceFlash(float t)
        {
            if (sourceFlash == null)
            {
                return;
            }

            float localT = Mathf.Clamp01(t / 0.32f);
            sourceFlash.transform.position = startPosition;
            sourceFlash.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.32f, 0.9f, Mathf.Sin(localT * Mathf.PI));
            sourceFlash.transform.Rotate(0f, 0f, 280f * Time.unscaledDeltaTime, Space.Self);
            sourceFlash.color = WithAlpha(accentColor, 1f - localT);
        }

        private void UpdateTravelSparks(float t, Vector3 targetPosition)
        {
            for (int i = 0; i < sparks.Length; i++)
            {
                SpriteRenderer spark = sparks[i];
                if (spark == null)
                {
                    continue;
                }

                float delayedT = Mathf.Clamp01((t - i * 0.045f) / 0.76f);
                float travelT = Smooth01(delayedT);
                spark.transform.position = EvaluatePath(travelT, targetPosition);
                float pulse = Mathf.Sin(delayedT * Mathf.PI);
                spark.transform.localScale = Vector3.one
                    * effectScale
                    * Mathf.Lerp(0.065f, 0.23f - i * 0.014f, pulse);
                spark.transform.Rotate(0f, 0f, (180f + i * 35f) * Time.unscaledDeltaTime, Space.Self);
                spark.color = WithAlpha(
                    Color.Lerp(primaryColor, accentColor, i * 0.18f),
                    pulse * (0.92f - i * 0.08f));
            }
        }

        private void UpdateTargetFlash(float t, Vector3 targetPosition)
        {
            if (targetFlash == null)
            {
                return;
            }

            float flashT = Mathf.Clamp01((t - 0.63f) / 0.37f);
            float pulse = Mathf.Sin(flashT * Mathf.PI);
            targetFlash.transform.position = targetPosition;
            targetFlash.transform.localScale = Vector3.one
                * effectScale
                * Mathf.Lerp(0.03f, 0.74f, pulse);
            targetFlash.transform.Rotate(0f, 0f, -360f * Time.unscaledDeltaTime, Space.Self);
            targetFlash.color = WithAlpha(accentColor, pulse);
        }

        private void UpdateTrail(Vector3 targetPosition, float headT, float alpha)
        {
            if (trail == null)
            {
                return;
            }

            float tailT = Mathf.Max(0f, headT - 0.34f);
            int count = Mathf.Max(2, trail.positionCount);
            for (int i = 0; i < count; i++)
            {
                float segmentT = Mathf.Lerp(tailT, headT, i / (count - 1f));
                trail.SetPosition(i, EvaluatePath(segmentT, targetPosition));
            }

            Color transparentPrimary = WithAlpha(primaryColor, 0f);
            trail.startColor = transparentPrimary;
            trail.endColor = WithAlpha(accentColor, alpha * 0.9f);
            trail.startWidth = effectScale * 0.05f;
            trail.endWidth = effectScale * 0.15f;
        }

        private Vector3 EvaluatePath(float t, Vector3 targetPosition)
        {
            Vector3 straight = Vector3.Lerp(startPosition, targetPosition, t);
            float arcHeight = Mathf.Max(0.42f, Vector3.Distance(startPosition, targetPosition) * 0.18f);
            return straight + Vector3.up * Mathf.Sin(t * Mathf.PI) * arcHeight;
        }

        private Vector3 GetCollectorPosition()
        {
            return collector != null
                ? collector.position + collectorOffset
                : startPosition + Vector3.up * 0.65f;
        }

        private void ApplyBillboardOrientation()
        {
            Quaternion rotation = GetCameraRotation();
            if (sourceFlash != null)
            {
                sourceFlash.transform.rotation = rotation;
            }
            if (targetFlash != null)
            {
                targetFlash.transform.rotation = rotation;
            }
            for (int i = 0; i < sparks.Length; i++)
            {
                if (sparks[i] != null)
                {
                    sparks[i].transform.rotation = rotation;
                }
            }
        }

        private void EnsureComponents()
        {
            if (trail == null)
            {
                trail = transform.Find("AbsorbTrail")?.GetComponent<LineRenderer>();
            }
            if (sourceFlash == null)
            {
                sourceFlash = transform.Find("PickupSpark")?.GetComponent<SpriteRenderer>();
            }
            if (targetFlash == null)
            {
                targetFlash = transform.Find("CollectorSpark")?.GetComponent<SpriteRenderer>();
            }
            if (autoReturn == null)
            {
                autoReturn = RuntimePool.EnsureAutoReturn(gameObject);
            }
            for (int i = 0; i < SparkCount; i++)
            {
                if (sparks[i] == null)
                {
                    sparks[i] = transform.Find($"AbsorbSpark{i + 1}")?.GetComponent<SpriteRenderer>();
                }
            }
        }

        private static LineRenderer CreateTrail(Transform parent)
        {
            GameObject trailObject = new GameObject("AbsorbTrail");
            trailObject.transform.SetParent(parent, false);
            LineRenderer renderer = trailObject.AddComponent<LineRenderer>();
            renderer.useWorldSpace = true;
            renderer.loop = false;
            renderer.positionCount = 7;
            renderer.numCornerVertices = 2;
            renderer.numCapVertices = 2;
            renderer.textureMode = LineTextureMode.Stretch;
            renderer.sortingOrder = TrailSortingOrder;
            renderer.sharedMaterial = CombatVfxResources.GetLineMaterial();
            renderer.enabled = false;
            return renderer;
        }

        private static SpriteRenderer CreateSpriteRenderer(
            Transform parent,
            string objectName,
            Sprite sprite,
            int sortingOrder)
        {
            GameObject spriteObject = new GameObject(objectName);
            spriteObject.transform.SetParent(parent, false);
            SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.enabled = false;
            return renderer;
        }

        private static Quaternion GetCameraRotation()
        {
            Camera camera = DontStarveCamera.GetActiveCamera();
            return camera != null ? camera.transform.rotation : Quaternion.Euler(45f, 0f, 0f);
        }

        private static float Smooth01(float t)
        {
            return t * t * (3f - 2f * t);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }
    }
}
