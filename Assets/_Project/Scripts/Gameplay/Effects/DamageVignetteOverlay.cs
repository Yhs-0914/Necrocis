using UnityEngine;
using UnityEngine.UI;

namespace Necrocis
{
    [DisallowMultipleComponent]
    internal sealed class DamageVignetteOverlay : MonoBehaviour
    {
        private static DamageVignetteOverlay instance;

        private Image image;
        private float peakAlpha;
        private float duration;
        private float elapsed;

        public static void Pulse(float alpha, float lifeTime)
        {
            EnsureInstance();
            if (instance == null)
            {
                return;
            }

            instance.peakAlpha = Mathf.Max(instance.peakAlpha, Mathf.Clamp01(alpha));
            instance.duration = Mathf.Max(instance.duration, Mathf.Max(0.08f, lifeTime));
            instance.elapsed = 0f;
            instance.enabled = true;
            instance.ApplyColor(instance.peakAlpha);
        }

        private static void EnsureInstance()
        {
            if (instance != null)
            {
                return;
            }

            GameObject root = new GameObject("__CombatDamageVignette", typeof(RectTransform));
            Object.DontDestroyOnLoad(root);

            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            Image overlayImage = root.AddComponent<Image>();
            overlayImage.sprite = CombatVfxResources.GetVignetteSprite();
            overlayImage.type = Image.Type.Simple;
            overlayImage.preserveAspect = false;
            overlayImage.raycastTarget = false;
            overlayImage.color = Color.clear;

            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            instance = root.AddComponent<DamageVignetteOverlay>();
            instance.image = overlayImage;
            instance.enabled = false;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
            float alpha = peakAlpha * Mathf.Pow(1f - t, 2.2f);
            ApplyColor(alpha);

            if (t >= 1f)
            {
                peakAlpha = 0f;
                duration = 0f;
                ApplyColor(0f);
                enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void ApplyColor(float alpha)
        {
            if (image != null)
            {
                image.color = new Color(0.72f, 0.015f, 0.035f, Mathf.Clamp01(alpha));
            }
        }
    }
}
