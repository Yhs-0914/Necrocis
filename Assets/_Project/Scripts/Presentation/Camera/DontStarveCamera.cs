using UnityEngine;
using UnityEngine.InputSystem;

namespace Necrocis
{
    /// <summary>
    /// 돈스타브 스타일 2.5D 카메라
    /// - 탑다운 + 약간 기울어진 시점
    /// - 플레이어 추적
    /// </summary>
    public class DontStarveCamera : MonoBehaviour
    {
        private static DontStarveCamera instance;

        public static DontStarveCamera Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<DontStarveCamera>();
                }

                return instance;
            }
            private set => instance = value;
        }

        [Header("타겟")]
        [SerializeField] private Transform target;

        [Header("카메라 설정")]
        [SerializeField] private bool useOrthographic = true;  // Orthographic 사용 (돈스타브 스타일)
        [SerializeField] private float height = 10f;           // 카메라 높이
        [SerializeField] private float distance = 5f;          // 뒤로 떨어진 거리
        [SerializeField] private float angle = 45f;            // 내려다보는 각도
        [SerializeField] private float smoothSpeed = 5f;       // 부드러운 이동
        [SerializeField] private bool centerTargetInView = true;
        [SerializeField] private bool useTargetRendererCenter = true;
        [SerializeField] private float targetForwardScreenOffset = 0f;

        [Header("줌 (Orthographic = Size, Perspective = Height)")]
        [SerializeField] private float orthoSize = 5f;         // Orthographic 크기
        [SerializeField] private float zoomSpeed = 1f;
        [SerializeField] private float minZoom = 3f;
        [SerializeField] private float maxZoom = 10f;

        private Camera cam;
        private Vector3 offset;
        private Transform cachedRendererTarget;
        private Renderer[] cachedTargetRenderers;

        public static Camera GetActiveCamera()
        {
            if (Instance != null && Instance.cam != null)
            {
                return Instance.cam;
            }

            return Camera.main;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);  // 씬 전환해도 유지

            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = gameObject.AddComponent<Camera>();
            }
        }

        private void Start()
        {
            TryAssignDefaultTarget();

            CalculateOffset();
            SetupCamera();

            // 카메라 배경색 설정 (어두운 붉은색 - 내장 테마)
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.15f, 0.05f, 0.05f);  // 어두운 붉은색
            }
        }

        private void LateUpdate()
        {
            if (!TryAssignDefaultTarget()) return;

            // 줌 처리
            HandleZoom();

            // 부드러운 추적
            Vector3 desiredPosition = GetTargetViewCenter() + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }

        /// <summary>
        /// 오프셋 계산
        /// </summary>
        private void CalculateOffset()
        {
            if (centerTargetInView)
            {
                float angleRadians = Mathf.Max(1f, Mathf.Abs(angle)) * Mathf.Deg2Rad;
                distance = Mathf.Max(0f, height / Mathf.Tan(angleRadians) - targetForwardScreenOffset);
            }

            offset = new Vector3(0, height, -distance);
        }

        /// <summary>
        /// 카메라 초기 설정
        /// </summary>
        private void SetupCamera()
        {
            // Orthographic / Perspective 설정
            if (cam != null)
            {
                cam.orthographic = useOrthographic;
                if (useOrthographic)
                {
                    cam.orthographicSize = orthoSize;
                }
            }

            // 카메라 각도 설정
            transform.rotation = Quaternion.Euler(angle, 0, 0);

            // 초기 위치
            if (target != null)
            {
                transform.position = GetTargetViewCenter() + offset;
            }
        }

        /// <summary>
        /// 줌 처리
        /// </summary>
        private void HandleZoom()
        {
            // 새 Input System 사용
            float scroll = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
            scroll *= 0.01f; // 스크롤 값 정규화

            if (Mathf.Abs(scroll) > 0.001f)
            {
                if (useOrthographic && cam != null)
                {
                    // Orthographic: Size 조절
                    orthoSize -= scroll * zoomSpeed;
                    orthoSize = Mathf.Clamp(orthoSize, minZoom, maxZoom);
                    cam.orthographicSize = orthoSize;
                }
                else
                {
                    // Perspective: 높이 조절
                    height -= scroll * zoomSpeed;
                    height = Mathf.Clamp(height, minZoom, maxZoom);
                    distance = height * 0.5f;
                    CalculateOffset();
                }
            }
        }

        /// <summary>
        /// 타겟 설정
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            InvalidateRendererCache();
        }

        private bool TryAssignDefaultTarget()
        {
            if (target != null)
            {
                return true;
            }

            PlayerController player = PlayerController.Instance;
            if (player != null)
            {
                target = player.transform;
                InvalidateRendererCache();
                return true;
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                target = playerObject.transform;
                InvalidateRendererCache();
            }

            return target != null;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// 즉시 타겟 위치로 이동
        /// </summary>
        public void SnapToTarget()
        {
            if (target != null)
            {
                transform.position = GetTargetViewCenter() + offset;
            }
        }

        private Vector3 GetTargetViewCenter()
        {
            if (target == null || !useTargetRendererCenter)
            {
                return target != null ? target.position : Vector3.zero;
            }

            Renderer[] renderers = GetCachedTargetRenderers();
            if (renderers == null || renderers.Length == 0)
            {
                return target.position;
            }

            bool hasBounds = false;
            Bounds bounds = default;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds ? bounds.center : target.position;
        }

        private Renderer[] GetCachedTargetRenderers()
        {
            if (target == null)
            {
                return null;
            }

            if (cachedRendererTarget != target || cachedTargetRenderers == null)
            {
                cachedRendererTarget = target;
                cachedTargetRenderers = target.GetComponentsInChildren<Renderer>();
            }

            return cachedTargetRenderers;
        }

        private void InvalidateRendererCache()
        {
            cachedRendererTarget = null;
            cachedTargetRenderers = null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (target != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, target.position);
                Gizmos.DrawWireSphere(target.position, 0.5f);
            }
        }
#endif
    }
}
