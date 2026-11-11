using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Necrocis
{
    [DisallowMultipleComponent]
    public class PlayerDeathScreen : MonoBehaviour
    {
        private const int SortingOrder = 500;
        private const string DeathArtworkResourcePath = "UI/Death/death_screen";

        [Header("Artwork")]
        [SerializeField] private Sprite deathArtwork;
        [SerializeField] private Color fallbackOverlayColor = new Color(0.02f, 0.015f, 0.018f, 1f);

        [Header("Button")]
        [SerializeField] private Rect buttonImageNormalizedRect = new Rect(0.34f, 0.25f, 0.32f, 0.12f);

        private GameObject uiRoot;
        private bool isShowing;
        private bool isLoading;
        private ModalInputScope inputScope;

        public bool IsShowing => isShowing;

        private void Start()
        {
            EnsureEventSystem();
            BuildUI();
            if (!isShowing) HideImmediate();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            HideImmediate();
            isLoading = false;
        }

        private void OnDestroy() { inputScope?.Dispose(); inputScope = null; }

        public void ShowDeath()
        {
            if (isShowing || isLoading)
            {
                return;
            }

            if (uiRoot == null)
            {
                BuildUI();
            }

            uiRoot.SetActive(true);
            isShowing = true;
            inputScope = new ModalInputScope(this, uiRoot, true, allowAuxiliary: false);
        }

        private void ReturnToHub()
        {
            if (inputScope?.CanInteract != true) return;
            if (isLoading)
            {
                AudioManager.Instance?.PlaySFX("UIInvalid");
                return;
            }

            AudioManager.Instance?.PlaySFX("ButtonClick");
            StartCoroutine(ReturnToHubRoutine());
        }

        private IEnumerator ReturnToHubRoutine()
        {
            isLoading=true;
            if(!SessionActionBindings.Death.TryPrepare(out var plan,out var error))
            {
                Debug.LogError(error);isLoading=false;AudioManager.Instance?.PlaySFX("UIInvalid");yield break;
            }
            HideImmediate();
            var routine=SessionActionBindings.Death.Execute(plan);
            try {while(routine.MoveNext())yield return routine.Current;}
            finally {(routine as System.IDisposable)?.Dispose();}
            isLoading=false;
        }

        private void HideImmediate()
        {
            if (uiRoot != null)
            {
                uiRoot.SetActive(false);
            }

            isShowing = false;
            inputScope?.Dispose(); inputScope = null;
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private void BuildUI()
        {
            if (uiRoot != null)
            {
                return;
            }

            GameObject canvasObj = new GameObject("PlayerDeathCanvas");
            canvasObj.transform.SetParent(transform, false);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();
            uiRoot = canvasObj;

            CreateFullscreenArtwork(canvasObj.transform);
        }

        private void CreateFullscreenArtwork(Transform parent)
        {
            GameObject imageObj = CreateUIElement("DeathArtwork", parent);
            RectTransform imageRect = imageObj.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;

            Image image = imageObj.AddComponent<Image>();
            image.color = fallbackOverlayColor;

            Sprite artwork = GetDeathArtwork();
            if (artwork == null)
            {
                return;
            }

            image.sprite = artwork;
            image.color = Color.white;
            image.preserveAspect = true;

            AspectRatioFitter fitter = imageObj.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = artwork.rect.width / artwork.rect.height;

            CreateEmbeddedReturnButton(imageObj.transform);
        }

        private void CreateEmbeddedReturnButton(Transform parent)
        {
            GameObject buttonObj = CreateUIElement("ReturnToHubHotspot", parent);
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;

            Button hubButton = buttonObj.AddComponent<Button>();
            ColorBlock colors = hubButton.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.06f);
            colors.pressedColor = new Color(0f, 0f, 0f, 0.08f);
            colors.selectedColor = Color.clear;
            colors.disabledColor = Color.clear;
            hubButton.colors = colors;
            hubButton.onClick.AddListener(ReturnToHub);

            RectTransform rect = hubButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(buttonImageNormalizedRect.xMin, buttonImageNormalizedRect.yMin);
            rect.anchorMax = new Vector2(buttonImageNormalizedRect.xMax, buttonImageNormalizedRect.yMax);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private Sprite GetDeathArtwork()
        {
            if (deathArtwork == null)
            {
                deathArtwork = Resources.Load<Sprite>(DeathArtworkResourcePath);
            }

            return deathArtwork;
        }

        private GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }
    }
}
