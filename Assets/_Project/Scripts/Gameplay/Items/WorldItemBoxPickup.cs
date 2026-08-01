using System.Collections;
using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class WorldItemBoxPickup : MonoBehaviour
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Sprite itemSprite;
        [SerializeField, Min(0.05f)] private float revealDuration = 0.6f;
        [SerializeField, Min(0f)] private float openedBoxHideDelay = 2f;
        [SerializeField, Min(0f)] private float itemRestHeight = 0.8f;
        [SerializeField, Min(0f)] private float itemPopHeight = 1.6f;
        [SerializeField] private int itemSortingOrderOffset = 1;

        private SpriteRenderer boxRenderer;
        private SpriteRenderer itemRenderer;
        private Transform itemVisual;
        private bool opening;
        private bool readyToPickup;
        private bool collected;
        private bool slotsFullLogged;
        private bool boxHideScheduled;

        public void Initialize(
            string itemId,
            string displayName,
            Sprite closedSprite,
            Sprite openSprite,
            Sprite itemSprite,
            int boxSortingOrder)
        {
            this.itemId = itemId;
            this.displayName = displayName;
            this.closedSprite = closedSprite;
            this.openSprite = openSprite;
            this.itemSprite = itemSprite;

            EnsureSetup(boxSortingOrder);
        }

        private void Awake()
        {
            EnsureSetup(0);
        }

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandlePlayerContact(other);
        }

        private void OnTriggerStay(Collider other)
        {
            HandlePlayerContact(other);
        }

        private void HandlePlayerContact(Collider other)
        {
            if (collected || string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player == null)
            {
                return;
            }

            if (!readyToPickup)
            {
                if (!opening)
                {
                    StartCoroutine(OpenRoutine());
                }

                return;
            }

            TryPickup(player);
        }

        private IEnumerator OpenRoutine()
        {
            opening = true;

            EnsureItemVisual();
            if (boxRenderer != null && openSprite != null)
            {
                boxRenderer.sprite = openSprite;
            }

            ScheduleBoxHide();

            if (itemRenderer != null)
            {
                itemRenderer.enabled = true;
            }

            float duration = Mathf.Max(0.05f, revealDuration);
            float halfDuration = duration * 0.5f;
            Vector3 start = new Vector3(0f, itemRestHeight * 0.15f, 0f);
            Vector3 peak = new Vector3(0f, itemPopHeight, 0f);
            Vector3 end = new Vector3(0f, itemRestHeight, 0f);

            for (float elapsed = 0f; elapsed < halfDuration; elapsed += Time.deltaTime)
            {
                SetItemLocalPosition(Vector3.Lerp(start, peak, Smooth01(elapsed / halfDuration)));
                yield return null;
            }

            for (float elapsed = 0f; elapsed < halfDuration; elapsed += Time.deltaTime)
            {
                SetItemLocalPosition(Vector3.Lerp(peak, end, Smooth01(elapsed / halfDuration)));
                yield return null;
            }

            SetItemLocalPosition(end);
            readyToPickup = true;
            opening = false;
        }

        private void ScheduleBoxHide()
        {
            if (boxHideScheduled || openedBoxHideDelay <= 0f)
            {
                return;
            }

            boxHideScheduled = true;
            StartCoroutine(HideBoxRendererAfterDelay());
        }

        private IEnumerator HideBoxRendererAfterDelay()
        {
            yield return new WaitForSeconds(openedBoxHideDelay);

            if (boxRenderer != null)
            {
                boxRenderer.enabled = false;
            }
        }

        private void TryPickup(PlayerController player)
        {
            PlayerItemManager itemManager = player.GetComponent<PlayerItemManager>();
            if (itemManager == null)
            {
                return;
            }

            if (itemManager.TryAcquireItem(itemId, out PlayerItemAcquireFailureReason failure))
            {
                PlayerItemCategory category = PlayerItemCategory.BasicProjectile;
                AudioManager.Instance?.PlaySFX("ItemPickup");
                if (itemManager.TryGetItemEntry(itemId, out PlayerItemManager.PlayerItemEntry entry))
                {
                    category = entry.Category;
                    AudioManager.Instance?.PlayItemCategorySFX(entry.Category);
                }
                Vector3 effectPosition = itemVisual != null
                    ? itemVisual.position
                    : transform.position + Vector3.up * itemRestHeight;
                CombatVfx.PlayItemPickup(
                    effectPosition,
                    player.transform,
                    category);
                collected = true;
                Destroy(gameObject);
                return;
            }

            if (failure == PlayerItemAcquireFailureReason.SlotsFull && !slotsFullLogged)
            {
                slotsFullLogged = true;
                Debug.Log($"[WorldItemBoxPickup] Pickup failed because item slots are full: {displayName} ({itemId})");
            }
        }

        private void EnsureSetup(int boxSortingOrder)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            boxRenderer = GetComponent<SpriteRenderer>();
            if (boxRenderer == null)
            {
                boxRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (closedSprite != null)
            {
                boxRenderer.sprite = closedSprite;
            }

            if (boxSortingOrder != 0)
            {
                boxRenderer.sortingOrder = boxSortingOrder;
            }
        }

        private void EnsureItemVisual()
        {
            if (itemVisual != null)
            {
                return;
            }

            GameObject itemObject = new GameObject("RevealedItem");
            itemObject.transform.SetParent(transform, false);
            itemVisual = itemObject.transform;

            itemRenderer = itemObject.AddComponent<SpriteRenderer>();
            itemRenderer.sprite = itemSprite;
            itemRenderer.enabled = false;
            if (boxRenderer != null)
            {
                itemRenderer.sortingOrder = boxRenderer.sortingOrder + itemSortingOrderOffset;
            }

            Billboard billboard = itemObject.AddComponent<Billboard>();
            billboard.SetUpdateMode(Billboard.UpdateMode.Once);

            SetItemLocalPosition(new Vector3(0f, itemRestHeight * 0.15f, 0f));
        }

        private void SetItemLocalPosition(Vector3 localPosition)
        {
            if (itemVisual == null)
            {
                return;
            }

            itemVisual.localPosition = localPosition;
            Billboard billboard = itemVisual.GetComponent<Billboard>();
            if (billboard != null)
            {
                billboard.ResetBaseLocalPosition(localPosition);
            }
        }

        private static float Smooth01(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }
    }
}
