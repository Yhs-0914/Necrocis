using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Necrocis
{
    /// <summary>
    /// 폐 바이옴 전체에서 들숨/날숨 바람과 모래바람 시야 제한을 관리한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LungBiomeHazardController : MonoBehaviour
    {
        private enum WindState
        {
            Calm,
            WarningInhale,
            Inhale,
            WarningExhale,
            Exhale
        }

        [Header("Wind Cycle")]
        [SerializeField, Min(0f)] private float initialDelay = 4f;
        [SerializeField, Min(0.1f)] private float warningDuration = 1f;
        [SerializeField, Min(0.1f)] private float inhaleDuration = 4f;
        [SerializeField, Min(0.1f)] private float exhaleDuration = 4f;
        [SerializeField, Min(0f)] private float restDuration = 3f;
        [SerializeField, Min(0f)] private float inhaleSpeed = 3.2f;
        [SerializeField, Min(0f)] private float exhaleSpeed = 3.8f;
        [SerializeField, Min(0f)] private float centerDeadZone = 1.25f;

        [Header("Sandstorm Region")]
        [SerializeField] private string sandstormRegionName = "Sand";
        [SerializeField, Range(0f, 1f)] private float visionMaskAlpha = 1f;
        [SerializeField, Min(40f)] private float clearVisionRadius = 230f;
        [SerializeField, Min(1f)] private float visionFeather = 170f;

        [Header("Wind Visual")]
        [SerializeField, Range(4, 32)] private int windStreakCount = 14;
        [SerializeField, Min(0.1f)] private float windStreakLength = 2.2f;
        [SerializeField, Min(1f)] private float windVisualRadius = 9f;

        // AudioManager에 음원이 등록되면 이 키를 그대로 사용한다.
        public const string WarningSoundKey = "LungWindWarning";
        public const string InhaleSoundKey = "LungWindInhale";
        public const string ExhaleSoundKey = "LungWindExhale";

        private readonly System.Collections.Generic.List<LineRenderer> windStreaks = new System.Collections.Generic.List<LineRenderer>();
        private ConfigurableBiomeManager biome;
        private ProceduralMap.MapGenerator proceduralMap;
        private PlayerController player;
        private WindState windState;
        private Vector3 mapCenter;
        private float windVisualTime;
        private Canvas visionCanvas;
        private CanvasGroup visionCanvasGroup;
        private RectTransform visionMaskRect;
        private Texture2D visionMaskTexture;
        private Sprite visionMaskSprite;
        private Coroutine windRoutine;
        private Material windMaterial;

        public void Initialize(ConfigurableBiomeManager owner)
        {
            biome = owner;
            mapCenter = biome.GridToWorld(biome.MapWidth / 2, biome.MapHeight / 2);
            StartHazards();
        }

        public void Initialize(ProceduralMap.MapGenerator owner)
        {
            proceduralMap = owner;
            mapCenter = owner.GetCellCenterWorld(owner.MapWidth / 2, owner.MapHeight / 2);
            StartHazards();
        }

        private void StartHazards()
        {
            BuildWindVisuals();
            BuildVisionMask();

            if (windRoutine != null)
            {
                StopCoroutine(windRoutine);
            }
            windRoutine = StartCoroutine(WindCycle());
        }

        private void Update()
        {
            ResolvePlayer();
            if (player == null)
            {
                SetVisionMask(false);
                return;
            }

            ApplyWindForce();
            UpdateWindVisuals();
            UpdateVisionMaskPosition();
            SetVisionMask(IsPlayerInSandstorm());
        }

        private IEnumerator WindCycle()
        {
            windState = WindState.Calm;
            yield return new WaitForSeconds(initialDelay);

            while (enabled)
            {
                windState = WindState.WarningInhale;
                TryPlayRegisteredSfx(WarningSoundKey);
                yield return new WaitForSeconds(warningDuration);

                windState = WindState.Inhale;
                TryPlayRegisteredSfx(InhaleSoundKey);
                yield return new WaitForSeconds(inhaleDuration);

                windState = WindState.Calm;
                yield return new WaitForSeconds(restDuration);

                windState = WindState.WarningExhale;
                TryPlayRegisteredSfx(WarningSoundKey);
                yield return new WaitForSeconds(warningDuration);

                windState = WindState.Exhale;
                TryPlayRegisteredSfx(ExhaleSoundKey);
                yield return new WaitForSeconds(exhaleDuration);

                windState = WindState.Calm;
                yield return new WaitForSeconds(restDuration);
            }
        }

        private void ApplyWindForce()
        {
            if (windState != WindState.Inhale && windState != WindState.Exhale)
            {
                return;
            }

            Vector3 fromCenter = player.transform.position - mapCenter;
            fromCenter.y = 0f;
            if (fromCenter.sqrMagnitude <= centerDeadZone * centerDeadZone)
            {
                if (windState == WindState.Inhale)
                {
                    return;
                }
                fromCenter = Vector3.forward;
            }

            Vector3 direction = fromCenter.normalized;
            float speed = exhaleSpeed;
            if (windState == WindState.Inhale)
            {
                direction = -direction;
                speed = inhaleSpeed;
            }

            player.TryMoveByWorld(direction * speed * Time.deltaTime);
        }

        private bool IsPlayerInSandstorm()
        {
            if (proceduralMap != null && player != null)
                return proceduralMap.IsExposedLungSand(player.transform.position);
            return biome != null
                && player != null
                && biome.IsWorldPositionUsingRegionTile(player.transform.position, sandstormRegionName);
        }

        private void BuildWindVisuals()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                windMaterial = new Material(shader) { name = "RuntimeLungWindMaterial" };
            }

            for (int i = 0; i < windStreakCount; i++)
            {
                GameObject streakObject = new GameObject($"WindStreak_{i + 1}");
                streakObject.transform.SetParent(transform, false);
                LineRenderer line = streakObject.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.positionCount = 2;
                line.startWidth = 0.055f;
                line.endWidth = 0.015f;
                line.numCapVertices = 2;
                line.sortingOrder = 5200;
                line.material = windMaterial;
                line.enabled = false;
                windStreaks.Add(line);
            }
        }

        private void UpdateWindVisuals()
        {
            bool warning = windState == WindState.WarningInhale || windState == WindState.WarningExhale;
            bool active = warning || windState == WindState.Inhale || windState == WindState.Exhale;
            if (!active || player == null)
            {
                SetWindStreaksEnabled(false);
                return;
            }

            SetWindStreaksEnabled(true);
            windVisualTime += Time.deltaTime * (warning ? 5f : 10f);
            Vector3 radial = player.transform.position - mapCenter;
            radial.y = 0f;
            if (radial.sqrMagnitude < 0.01f) radial = Vector3.forward;
            Vector3 outward = radial.normalized;
            bool inward = windState == WindState.Inhale || windState == WindState.WarningInhale;
            Vector3 direction = inward ? -outward : outward;
            Vector3 side = new Vector3(-direction.z, 0f, direction.x);
            Color color = warning
                ? new Color(1f, 0.72f, 0.22f, 0.9f)
                : new Color(0.75f, 0.9f, 1f, 0.72f);

            for (int i = 0; i < windStreaks.Count; i++)
            {
                float seed = i * 1.618f;
                float travel = Mathf.Repeat(windVisualTime + seed, windVisualRadius * 2f) - windVisualRadius;
                float lateral = Mathf.Sin(seed * 2.37f) * windVisualRadius;
                Vector3 start = player.transform.position + direction * travel + side * lateral;
                start.y += 0.35f + Mathf.Repeat(seed, 1.7f);
                LineRenderer line = windStreaks[i];
                line.startColor = color;
                line.endColor = new Color(color.r, color.g, color.b, 0f);
                line.SetPosition(0, start);
                line.SetPosition(1, start - direction * windStreakLength);
            }
        }

        private void BuildVisionMask()
        {
            GameObject canvasObject = new GameObject("LungSandstormVisionMask", typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            canvasObject.transform.SetParent(transform, false);
            visionCanvas = canvasObject.GetComponent<Canvas>();
            visionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // 월드 위에는 표시하되 HUD보다 먼저 그려 UI까지 흐려지지 않게 한다.
            visionCanvas.sortingOrder = -100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            visionCanvasGroup = canvasObject.GetComponent<CanvasGroup>();
            visionCanvasGroup.alpha = 0f;
            visionCanvasGroup.blocksRaycasts = false;
            visionCanvasGroup.interactable = false;

            CreateRadialVisionMask(canvasObject.transform);
        }

        private void CreateRadialVisionMask(Transform parent)
        {
            const int textureSize = 256;
            const float maskDisplaySize = 3840f;
            float clearNormalized = clearVisionRadius / maskDisplaySize;
            float featherNormalized = visionFeather / maskDisplaySize;
            Color sandColor = Color.black;

            visionMaskTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                name = "RuntimeLungRadialVisionMask",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color32[] pixels = new Color32[textureSize * textureSize];
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float nx = (x + 0.5f) / textureSize - 0.5f;
                    float ny = (y + 0.5f) / textureSize - 0.5f;
                    float distance = Mathf.Sqrt(nx * nx + ny * ny);
                    float alpha = Mathf.SmoothStep(0f, visionMaskAlpha,
                        Mathf.InverseLerp(clearNormalized, clearNormalized + featherNormalized, distance));
                    pixels[y * textureSize + x] = new Color(sandColor.r, sandColor.g, sandColor.b, alpha);
                }
            }
            visionMaskTexture.SetPixels32(pixels);
            visionMaskTexture.Apply(false, true);
            visionMaskSprite = Sprite.Create(visionMaskTexture, new Rect(0f, 0f, textureSize, textureSize),
                new Vector2(0.5f, 0.5f), 100f);

            GameObject mask = new GameObject("PlayerCenteredRadialMask", typeof(RectTransform), typeof(Image));
            mask.transform.SetParent(parent, false);
            visionMaskRect = mask.GetComponent<RectTransform>();
            visionMaskRect.anchorMin = new Vector2(0.5f, 0.5f);
            visionMaskRect.anchorMax = new Vector2(0.5f, 0.5f);
            visionMaskRect.pivot = new Vector2(0.5f, 0.5f);
            visionMaskRect.sizeDelta = Vector2.one * maskDisplaySize;
            Image image = mask.GetComponent<Image>();
            image.sprite = visionMaskSprite;
            image.color = Color.white;
            image.raycastTarget = false;
        }

        private void UpdateVisionMaskPosition()
        {
            if (visionMaskRect == null || visionCanvas == null || player == null) return;
            Camera worldCamera = Camera.main;
            if (worldCamera == null) return;

            Vector2 screenPoint = worldCamera.WorldToScreenPoint(player.transform.position);
            RectTransform canvasRect = visionCanvas.transform as RectTransform;
            if (canvasRect != null
                && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint))
            {
                visionMaskRect.anchoredPosition = localPoint;
            }
        }

        private void SetVisionMask(bool visible)
        {
            if (visionCanvasGroup == null) return;
            float target = visible ? 1f : 0f;
            visionCanvasGroup.alpha = Mathf.MoveTowards(visionCanvasGroup.alpha, target, Time.unscaledDeltaTime * 2.5f);
        }

        private void SetWindStreaksEnabled(bool value)
        {
            for (int i = 0; i < windStreaks.Count; i++)
            {
                windStreaks[i].enabled = value;
            }
        }

        private void ResolvePlayer()
        {
            if (player == null)
            {
                player = PlayerController.Instance ?? FindFirstObjectByType<PlayerController>();
            }
        }

        private static void TryPlayRegisteredSfx(string key)
        {
            // 음원이 전달되기 전에는 경고 로그를 만들지 않는다.
            // AudioManager 등록 후 아래 호출을 활성화한다.
        }

        private void OnDestroy()
        {
            if (windMaterial != null)
            {
                Destroy(windMaterial);
            }
            if (visionMaskSprite != null)
            {
                Destroy(visionMaskSprite);
            }
            if (visionMaskTexture != null)
            {
                Destroy(visionMaskTexture);
            }
        }

    }
}
