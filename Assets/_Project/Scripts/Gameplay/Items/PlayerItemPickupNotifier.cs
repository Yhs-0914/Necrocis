using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Necrocis
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerItemManager))]
    public class PlayerItemPickupNotifier : MonoBehaviour
    {
        [SerializeField] private float messageDuration = 2.2f;
        [SerializeField] private int fontSize = 28;

        private PlayerItemManager itemManager;
        private GameObject canvasObject;
        private Text messageText;
        private Coroutine hideRoutine;

        private void Awake()
        {
            itemManager = GetComponent<PlayerItemManager>();
            EnsureUi();
        }

        private void OnEnable()
        {
            if (itemManager != null)
            {
                itemManager.ItemAcquired += HandleItemAcquired;
            }
        }

        private void OnDisable()
        {
            if (itemManager != null)
            {
                itemManager.ItemAcquired -= HandleItemAcquired;
            }
        }

        private void HandleItemAcquired(PlayerItemManager _, PlayerItemManager.AcquiredPlayerItem item)
        {
            if (item == null || (itemManager != null && itemManager.IsRestoringSavedItems))
            {
                return;
            }

            string itemName = string.IsNullOrWhiteSpace(item.DisplayName) ? item.ItemId : item.DisplayName;
            string message = $"아이템 획득: {itemName}";
            Debug.Log($"[PlayerItem] {message}");
            ShowMessage(message);
        }

        private void ShowMessage(string message)
        {
            if (messageText == null)
            {
                return;
            }

            messageText.text = message;
            messageText.enabled = true;

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }

            hideRoutine = StartCoroutine(HideMessageAfterDelay());
        }

        private IEnumerator HideMessageAfterDelay()
        {
            yield return new WaitForSeconds(messageDuration);
            if (messageText != null)
            {
                messageText.enabled = false;
            }

            hideRoutine = null;
        }

        private void EnsureUi()
        {
            if (canvasObject != null && messageText != null)
            {
                return;
            }

            canvasObject = new GameObject("ItemPickupNotifierCanvas");
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 190;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject textObject = new GameObject("Message");
            textObject.transform.SetParent(canvasObject.transform, false);

            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -70f);
            rect.sizeDelta = new Vector2(1100f, 80f);

            messageText = textObject.AddComponent<Text>();
            messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            messageText.fontSize = fontSize;
            messageText.alignment = TextAnchor.MiddleCenter;
            messageText.color = new Color(0.95f, 0.95f, 0.25f, 1f);
            messageText.enabled = false;
        }
    }
}
