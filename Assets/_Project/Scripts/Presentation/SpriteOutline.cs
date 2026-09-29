using UnityEngine;

namespace Necrocis
{
    /// <summary>Retains a star-shaped sprite outline across asset imports and scene reloads.</summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteOutline : MonoBehaviour
    {
        [SerializeField] private Sprite sourceSprite;
        [SerializeField] private Vector2[] normalizedOutline;
        private Sprite runtimeSprite;
        private bool rebuildPending;

        public void Configure(Sprite source, Vector2[] outline)
        {
            ReleaseSprite();
            sourceSprite = source;
            normalizedOutline = outline;
            Rebuild();
        }

        // OnEnable may run while scene resources are still being deserialized.
        private void OnEnable() => rebuildPending = true;

        private void Update()
        {
            if (rebuildPending) Rebuild();
        }

        private void OnDisable()
        {
            rebuildPending = false;
            ReleaseSprite();
        }

        [ContextMenu("Rebuild Sprite Outline")]
        public void Rebuild()
        {
            rebuildPending = false;
            ReleaseSprite();
            if (sourceSprite == null || normalizedOutline == null || normalizedOutline.Length < 3) return;
            Rect rect = sourceSprite.rect;
            float ppu = sourceSprite.pixelsPerUnit;
            Vector2 pivot = sourceSprite.pivot;
            runtimeSprite = Sprite.Create(sourceSprite.texture, rect,
                new Vector2(pivot.x / rect.width, pivot.y / rect.height), ppu, 0, SpriteMeshType.FullRect);
            runtimeSprite.name = sourceSprite.name + " (Outline)";
            runtimeSprite.hideFlags = HideFlags.HideAndDontSave;
            var vertices = new Vector2[normalizedOutline.Length + 1];
            // OverrideGeometry takes rect-local pixels; Unity applies pivot and PPU itself.
            vertices[0] = rect.size * .5f;
            var triangles = new ushort[normalizedOutline.Length * 3];
            for (int i = 0; i < normalizedOutline.Length; i++)
            {
                Vector2 pixel = Vector2.Scale(normalizedOutline[i], rect.size);
                pixel.x = Mathf.Clamp(pixel.x, .01f, rect.width - .01f);
                pixel.y = Mathf.Clamp(pixel.y, .01f, rect.height - .01f);
                vertices[i + 1] = pixel;
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = (ushort)(i + 1);
                triangles[i * 3 + 2] = (ushort)((i + 1) % normalizedOutline.Length + 1);
            }
            runtimeSprite.OverrideGeometry(vertices, triangles);
            GetComponent<SpriteRenderer>().sprite = runtimeSprite;
        }

        private void ReleaseSprite()
        {
            if (runtimeSprite == null) return;
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.sprite == runtimeSprite) renderer.sprite = sourceSprite;
            if (Application.isPlaying) Destroy(runtimeSprite);
            else DestroyImmediate(runtimeSprite);
            runtimeSprite = null;
        }
    }
}
