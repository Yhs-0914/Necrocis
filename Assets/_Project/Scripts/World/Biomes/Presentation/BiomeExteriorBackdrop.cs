using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// 플레이 가능한 타일 뒤에서만 보이는 바이옴별 원경 배경입니다.
    /// 카메라를 느리게 따라가며 맵 밖의 단색 공백을 가립니다.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public sealed class BiomeExteriorBackdrop : MonoBehaviour
    {
        private const string ObjectName = "ExteriorBackdrop";

        private BiomeExteriorBackdropConfig config;
        private SpriteRenderer spriteRenderer;
        private Camera activeCamera;
        private CameraClearFlags previousClearFlags;
        private Color previousClearColor;
        private bool cameraStateCaptured;
        private float phase;

        public static BiomeExteriorBackdrop Create(Transform owner, BiomeExteriorBackdropConfig backdropConfig)
        {
            if (owner == null || backdropConfig == null || !backdropConfig.IsUsable)
            {
                return null;
            }

            GameObject backdropObject = new GameObject(ObjectName);
            backdropObject.transform.SetParent(owner, false);

            BiomeExteriorBackdrop backdrop = backdropObject.AddComponent<BiomeExteriorBackdrop>();
            backdrop.Initialize(backdropConfig);
            return backdrop;
        }

        private void Initialize(BiomeExteriorBackdropConfig backdropConfig)
        {
            config = backdropConfig;
            phase = Mathf.Abs(backdropConfig.sprite.name.GetHashCode() % 1000) * 0.001f;

            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = backdropConfig.sprite;
            spriteRenderer.color = WithAlpha(backdropConfig.tint, backdropConfig.opacity);
            spriteRenderer.sortingOrder = backdropConfig.sortingOrder;
            spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            spriteRenderer.receiveShadows = false;

            ResolveCamera();
            UpdateTransform();
        }

        private void LateUpdate()
        {
            if (config == null || spriteRenderer == null)
            {
                return;
            }

            ResolveCamera();
            UpdateTransform();
        }

        private void OnDestroy()
        {
            RestoreCameraState();
        }

        private void ResolveCamera()
        {
            Camera candidate = DontStarveCamera.GetActiveCamera();
            if (candidate == activeCamera)
            {
                return;
            }

            RestoreCameraState();
            activeCamera = candidate;
            if (activeCamera == null)
            {
                return;
            }

            previousClearFlags = activeCamera.clearFlags;
            previousClearColor = activeCamera.backgroundColor;
            cameraStateCaptured = true;
            activeCamera.clearFlags = CameraClearFlags.SolidColor;
            activeCamera.backgroundColor = config.cameraClearColor;
        }

        private void RestoreCameraState()
        {
            if (!cameraStateCaptured || activeCamera == null)
            {
                return;
            }

            activeCamera.clearFlags = previousClearFlags;
            activeCamera.backgroundColor = previousClearColor;
            cameraStateCaptured = false;
        }

        private void UpdateTransform()
        {
            if (activeCamera == null || spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return;
            }

            float distance = Mathf.Min(
                Mathf.Max(activeCamera.nearClipPlane + 1f, config.cameraDistance),
                Mathf.Max(activeCamera.nearClipPlane + 1f, activeCamera.farClipPlane - 1f));
            GetViewSize(activeCamera, distance, out float viewWidth, out float viewHeight);

            float drift = Mathf.Max(0f, config.driftAmount);
            float time = Time.unscaledTime * Mathf.Max(0f, config.driftSpeed) + phase;
            Vector3 driftOffset = activeCamera.transform.right * (Mathf.Sin(time) * viewWidth * drift);
            driftOffset += activeCamera.transform.up * (Mathf.Cos(time * 0.73f) * viewHeight * drift * 0.65f);

            transform.SetPositionAndRotation(
                activeCamera.transform.position + activeCamera.transform.forward * distance + driftOffset,
                activeCamera.transform.rotation);

            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            if (spriteSize.x <= Mathf.Epsilon || spriteSize.y <= Mathf.Epsilon)
            {
                return;
            }

            float requiredScale = Mathf.Max(viewWidth / spriteSize.x, viewHeight / spriteSize.y);
            float scale = requiredScale * Mathf.Max(1f, config.overscan);
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        private static void GetViewSize(Camera camera, float distance, out float width, out float height)
        {
            if (camera.orthographic)
            {
                height = camera.orthographicSize * 2f;
            }
            else
            {
                height = 2f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            }

            width = height * Mathf.Max(0.01f, camera.aspect);
        }

        private static Color WithAlpha(Color color, float opacity)
        {
            color.a *= Mathf.Clamp01(opacity);
            return color;
        }
    }
}
