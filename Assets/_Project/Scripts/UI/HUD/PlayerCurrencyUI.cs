using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Necrocis
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerHeartUI))]
    public sealed class PlayerCurrencyUI : MonoBehaviour
    {
        public const string CoinResourcePath = "UI/Currency/CellCoin";
        private const float BadgeHeight = 52f;
        private const float GapBelowHearts = 12f;
        [SerializeField] private long amount;
        private PlayerHeartUI hearts;
        private RectTransform layoutHeart;
        private RectTransform badge;
        private Text amountText;
        private Image coin;
        private long displayedAmount = -1;
        public long Amount => amount;
        public RectTransform Badge => badge;
        public Text AmountText => amountText;
        public Image CoinImage => coin;

        private void OnEnable()
        {
            hearts = GetComponent<PlayerHeartUI>();
            SaveService.CurrencyChanged += SetAmount;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            EnsureDisplay();
            SetAmount(SaveService.CurrencyBalance);
            UpdateLayout();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            SaveService.CurrencyChanged -= SetAmount;
            if (badge != null) badge.gameObject.SetActive(false);
        }

        private void OnDestroy() { if (badge != null) Destroy(badge.gameObject); }

        public void SetAmount(long value)
        {
            amount = System.Math.Max(0L, value);
            if (RefreshAmount()) UpdateLayout();
        }

        private void OnValidate()
        {
            amount = System.Math.Max(0L, amount);
            if (Application.isPlaying) { RefreshAmount(); UpdateLayout(); }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureDisplay();
            SetAmount(SaveService.CurrencyBalance);
            UpdateLayout();
        }

        private void OnActiveSceneChanged(Scene previous, Scene next) => UpdateVisibility();

        private void LateUpdate()
        {
            var heartRect = hearts != null ? hearts.HeartContainer : null;
            if (heartRect == null) return;
            if (badge == null || layoutHeart != heartRect || !heartRect.IsChildOf(badge.parent))
            {
                EnsureDisplay();
                RefreshAmount();
            }
            UpdateLayout();
        }

        private void EnsureDisplay()
        {
            if (hearts == null) hearts = GetComponent<PlayerHeartUI>();
            var heartRect = hearts != null ? hearts.HeartContainer : null;
            var canvas = heartRect != null ? heartRect.GetComponentInParent<Canvas>() : null;
            if (canvas == null) return;
            layoutHeart = heartRect;
            if (badge != null)
            {
                if (badge.parent != canvas.transform) badge.SetParent(canvas.transform, false);
                UpdateVisibility();
                return;
            }
            Image background = RuntimeUiFactory.CreateImage("CurrencyBadge", canvas.transform,
                new Color(.10f, .045f, .07f, .80f));
            badge = background.rectTransform;
            badge.anchorMin = badge.anchorMax = badge.pivot = new Vector2(0, 1);
            background.raycastTarget = false;
            var group = badge.gameObject.AddComponent<CanvasGroup>();
            group.interactable = false; group.blocksRaycasts = false;

            Image accent = RuntimeUiFactory.CreateImage("GoldAccent", badge, new Color(.77f, .48f, .17f, .85f));
            accent.raycastTarget = false;
            RuntimeUiFactory.SetRect(accent.rectTransform, new Vector2(0, .5f), new Vector2(2, 30), new Vector2(1, 0));
            coin = RuntimeUiFactory.CreateImage("Coin", badge, Resources.Load<Sprite>(CoinResourcePath), Color.white);
            coin.preserveAspect = true; coin.raycastTarget = false;
            RuntimeUiFactory.SetRect(coin.rectTransform, new Vector2(0, .5f), new Vector2(44, 44), new Vector2(30, 0));
            amountText = RuntimeUiFactory.CreateText("Amount", badge, "0", GameUiTheme.LoadFont(), 30,
                new Color(1f, .91f, .67f));
            amountText.fontStyle = FontStyle.Bold;
            amountText.alignment = TextAnchor.MiddleLeft;
            amountText.horizontalOverflow = HorizontalWrapMode.Wrap;
            amountText.verticalOverflow = VerticalWrapMode.Truncate;
            amountText.resizeTextForBestFit = true;
            amountText.resizeTextMinSize = 14; amountText.resizeTextMaxSize = 30;
            amountText.GetComponent<Shadow>().effectDistance = new Vector2(1.5f, -1.5f);
            amountText.rectTransform.anchorMin = Vector2.zero;
            amountText.rectTransform.anchorMax = Vector2.one;
            amountText.rectTransform.offsetMin = new Vector2(62, 0);
            amountText.rectTransform.offsetMax = new Vector2(-14, 0);
            displayedAmount = -1;
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (badge != null)
                badge.gameObject.SetActive(SceneManager.GetActiveScene().name != SceneNames.MainMenu);
        }

        private bool RefreshAmount()
        {
            if (amountText == null || displayedAmount == amount) return false;
            displayedAmount = amount;
            amountText.text = amount.ToString("N0", CultureInfo.InvariantCulture);
            badge.sizeDelta = new Vector2(Mathf.Clamp(amountText.preferredWidth + 80, 156, 320), BadgeHeight);
            return true;
        }

        private void UpdateLayout()
        {
            if (badge == null || layoutHeart == null || !badge.gameObject.activeSelf) return;
            var canvasRect = badge.parent as RectTransform;
            if (canvasRect == null) return;
            Vector3 bottomLeft = canvasRect.InverseTransformPoint(layoutHeart.TransformPoint(layoutHeart.rect.min));
            Rect canvasBounds = canvasRect.rect;
            float x = Mathf.Clamp(bottomLeft.x - canvasBounds.xMin, 24,
                Mathf.Max(24, canvasBounds.width - badge.sizeDelta.x - 24));
            float y = Mathf.Min(-24, bottomLeft.y - canvasBounds.yMax - GapBelowHearts);
            var position = new Vector2(x, y);
            if (badge.anchoredPosition != position) badge.anchoredPosition = position;
        }
    }
}
