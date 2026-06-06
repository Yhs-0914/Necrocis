using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public class WorldMapUI : MonoBehaviour
    {
        private const int CanvasOrder = 5000;
        private static readonly Color EmptyColor = new Color(0.04f, 0.025f, 0.04f, 1f);
        private static readonly Color BossFogColor = new Color(0.22f, 0.08f, 0.3f, 0.72f);

        private ConfigurableBiomeManager biome;
        private Transform player;
        private Texture2D mapTexture;
        private RawImage miniMapImage;
        private RawImage fullMapImage;
        private RectTransform miniPlayerMarker;
        private RectTransform fullPlayerMarker;
        private RectTransform miniBossFog;
        private RectTransform fullBossFog;
        private GameObject fullMapRoot;
        private float previousTimeScale = 1f;
        private bool isOpen;

        private void Start()
        {
            biome = GetComponent<ConfigurableBiomeManager>();
            if (biome == null)
            {
                enabled = false;
                return;
            }

            PlayerController controller = FindFirstObjectByType<PlayerController>();
            player = controller != null ? controller.transform : null;
            BuildMapTexture();
            BuildUI();
            UpdateMarkers();
        }

        private void Update()
        {
            if (InputManager.Instance.MapAction.WasPressedThisFrame())
            {
                SetFullMapOpen(!isOpen);
            }

            UpdateMarkers();
        }

        private void OnDestroy()
        {
            if (isOpen)
            {
                Time.timeScale = previousTimeScale;
            }

            if (mapTexture != null)
            {
                Destroy(mapTexture);
            }
        }

        private void BuildMapTexture()
        {
            int width = biome.MapWidth;
            int height = biome.MapHeight;
            mapTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = $"{biome.BiomeType}_WorldMap",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            Color[] pixels = new Color[width * height];
            Dictionary<Sprite, Color> colorCache = new Dictionary<Sprite, Color>();
            for (int y = 0; y < height; y++)
            {
                int gridY = biome.MinGridYPublic + y;
                for (int x = 0; x < width; x++)
                {
                    int gridX = biome.MinGridXPublic + x;
                    Sprite sprite = biome.GetAuthoredMapSprite(gridX, gridY);
                    pixels[y * width + x] = GetSpriteColor(sprite, colorCache);
                }
            }

            mapTexture.SetPixels(pixels);
            mapTexture.Apply(false, false);
        }

        private static Color GetSpriteColor(Sprite sprite, Dictionary<Sprite, Color> cache)
        {
            if (sprite == null)
            {
                return EmptyColor;
            }

            if (cache.TryGetValue(sprite, out Color cached))
            {
                return cached;
            }

            Texture2D source = sprite.texture;
            Rect rect = sprite.textureRect;
            RenderTexture temporary = RenderTexture.GetTemporary(1, 1, 0, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            Graphics.Blit(source, temporary);
            RenderTexture.active = temporary;

            Texture2D sample = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            sample.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
            sample.Apply();
            Color color = sample.GetPixel(0, 0);

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);
            Destroy(sample);

            color.a = 1f;
            cache[sprite] = color;
            return color;
        }

        private void BuildUI()
        {
            GameObject canvasObject = new GameObject("WorldMapCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CanvasOrder;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            miniMapImage = CreateMapImage(canvas.transform, "MiniMap", new Vector2(270f, 270f));
            RectTransform miniRect = miniMapImage.rectTransform;
            miniRect.anchorMin = miniRect.anchorMax = new Vector2(1f, 1f);
            miniRect.pivot = new Vector2(1f, 1f);
            miniRect.anchoredPosition = new Vector2(-28f, -28f);
            CreateBorder(miniRect, new Color(0.16f, 0.05f, 0.12f, 0.96f), 12f);
            miniBossFog = CreateOverlay(miniRect, "BossFog", BossFogColor);
            miniPlayerMarker = CreateMarker(miniRect, "PlayerMarker", 13f);

            fullMapRoot = new GameObject("FullMap", typeof(RectTransform), typeof(Image));
            fullMapRoot.transform.SetParent(canvas.transform, false);
            RectTransform fullRootRect = fullMapRoot.GetComponent<RectTransform>();
            Stretch(fullRootRect);
            fullMapRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);

            fullMapImage = CreateMapImage(fullMapRoot.transform, "Map", new Vector2(820f, 820f));
            fullMapImage.rectTransform.anchorMin = fullMapImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            fullMapImage.rectTransform.anchoredPosition = Vector2.zero;
            CreateBorder(fullMapImage.rectTransform, new Color(0.16f, 0.05f, 0.12f, 1f), 18f);
            fullBossFog = CreateOverlay(fullMapImage.rectTransform, "BossFog", BossFogColor);
            fullPlayerMarker = CreateMarker(fullMapImage.rectTransform, "PlayerMarker", 18f);
            fullMapRoot.SetActive(false);
        }

        private RawImage CreateMapImage(Transform parent, string name, Vector2 size)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            RawImage image = obj.GetComponent<RawImage>();
            image.texture = mapTexture;
            image.color = Color.white;
            return image;
        }

        private static void CreateBorder(RectTransform target, Color color, float padding)
        {
            CreateBorderEdge(target, "BorderTop", color, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, padding));
            CreateBorderEdge(target, "BorderBottom", color, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, padding));
            CreateBorderEdge(target, "BorderLeft", color, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(padding, 0f));
            CreateBorderEdge(target, "BorderRight", color, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(padding, 0f));
        }

        private static void CreateBorderEdge(
            RectTransform target,
            string name,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size)
        {
            GameObject edge = new GameObject(name, typeof(RectTransform), typeof(Image));
            edge.transform.SetParent(target, false);
            RectTransform rect = edge.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = (anchorMin + anchorMax) * 0.5f;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            edge.GetComponent<Image>().color = color;
        }

        private static RectTransform CreateMarker(RectTransform parent, string name, float size)
        {
            GameObject marker = new GameObject(name, typeof(RectTransform), typeof(Image));
            marker.transform.SetParent(parent, false);
            RectTransform rect = marker.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(size, size);
            marker.GetComponent<Image>().color = new Color(1f, 0.18f, 0.25f, 1f);
            return rect;
        }

        private static RectTransform CreateOverlay(RectTransform parent, string name, Color color)
        {
            GameObject overlay = new GameObject(name, typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(parent, false);
            overlay.GetComponent<Image>().color = color;
            return overlay.GetComponent<RectTransform>();
        }

        private void UpdateMarkers()
        {
            if (player != null)
            {
                Vector2 normalized = WorldToNormalized(player.position);
                PositionInMap(miniPlayerMarker, normalized);
                PositionInMap(fullPlayerMarker, normalized);
            }

            MidBossArenaController arena = FindFirstObjectByType<MidBossArenaController>();
            bool showFog = arena != null && !arena.IsDefeated;
            UpdateBossFog(miniBossFog, arena, showFog);
            UpdateBossFog(fullBossFog, arena, showFog);
        }

        private void UpdateBossFog(RectTransform fog, MidBossArenaController arena, bool visible)
        {
            if (fog == null)
            {
                return;
            }

            fog.gameObject.SetActive(visible);
            if (!visible || arena == null)
            {
                return;
            }

            Vector2 center = GridToNormalized(arena.CenterGrid);
            fog.anchorMin = fog.anchorMax = center;
            fog.pivot = new Vector2(0.5f, 0.5f);
            RectTransform parent = (RectTransform)fog.parent;
            fog.sizeDelta = new Vector2(
                parent.rect.width * arena.ArenaSize.x / biome.MapWidth,
                parent.rect.height * arena.ArenaSize.y / biome.MapHeight);
            fog.anchoredPosition = Vector2.zero;
        }

        private Vector2 WorldToNormalized(Vector3 position)
        {
            Vector2Int grid = biome.WorldToGrid(position);
            return GridToNormalized(grid);
        }

        private Vector2 GridToNormalized(Vector2Int grid)
        {
            return new Vector2(
                Mathf.Clamp01((grid.x - biome.MinGridXPublic + 0.5f) / biome.MapWidth),
                Mathf.Clamp01((grid.y - biome.MinGridYPublic + 0.5f) / biome.MapHeight));
        }

        private static void PositionInMap(RectTransform marker, Vector2 normalized)
        {
            if (marker == null)
            {
                return;
            }

            marker.anchorMin = marker.anchorMax = normalized;
            marker.anchoredPosition = Vector2.zero;
        }

        private void SetFullMapOpen(bool open)
        {
            isOpen = open;
            fullMapRoot.SetActive(open);
            miniMapImage.gameObject.SetActive(!open);

            if (open)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = previousTimeScale;
            }
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
