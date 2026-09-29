using UnityEngine;

namespace Necrocis
{
    /// <summary>Small camera-facing health bar used above phase-one pillars.</summary>
    [DisallowMultipleComponent]
    public sealed class FinalBossWorldHealthBar : MonoBehaviour
    {
        private static Sprite whiteSprite;
        private Transform fill;
        private float width;

        public static FinalBossWorldHealthBar Create(Transform parent, float height, Color fillColor)
        {
            GameObject root = new GameObject("PillarHealthBar");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.up * height;
            FinalBossWorldHealthBar bar = root.AddComponent<FinalBossWorldHealthBar>();
            bar.Build(fillColor);
            return bar;
        }

        private void Build(Color fillColor)
        {
            width = 3.4f;
            CreatePart("Frame", new Color(.07f, .035f, .06f, .96f), new Vector3(width + .18f, .38f, 1f), 5998, out _);
            CreatePart("Background", new Color(.18f, .08f, .12f, .96f), new Vector3(width, .22f, 1f), 5999, out _);
            CreatePart("Fill", fillColor, new Vector3(width, .22f, 1f), 6000, out fill);
        }

        private void CreatePart(string objectName, Color color, Vector3 scale, int order, out Transform part)
        {
            GameObject go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            go.transform.localScale = scale;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = GetWhiteSprite();
            renderer.color = color;
            renderer.sortingOrder = order;
            part = go.transform;
        }

        public void SetValue(float normalized)
        {
            if (fill == null) return;
            float value = Mathf.Clamp01(normalized);
            fill.localScale = new Vector3(width * value, .22f, 1f);
            fill.localPosition = new Vector3((value - 1f) * width * .5f, 0f, -.01f);
        }

        public void Hide() => gameObject.SetActive(false);

        private void LateUpdate()
        {
            Camera camera = DontStarveCamera.GetActiveCamera();
            if (camera != null) transform.rotation = camera.transform.rotation;
        }

        private static Sprite GetWhiteSprite()
        {
            if (whiteSprite != null) return whiteSprite;
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "FinalBossHealthBarPixel",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            whiteSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1f);
            whiteSprite.name = "FinalBossHealthBarSprite";
            whiteSprite.hideFlags = HideFlags.HideAndDontSave;
            return whiteSprite;
        }
    }
}
