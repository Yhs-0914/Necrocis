using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class WorldItemBoxPickup : MonoBehaviour
    {
        [SerializeField] private string itemId;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite openSprite;

        private SpriteRenderer boxRenderer;
        private bool collected;
        private bool rewardRequested;
        private WorldRewardSaveData reward;

        public void BindReward(WorldRewardSaveData state) { reward = state; }

        public void Initialize(string itemId, Sprite closedSprite, Sprite openSprite, int boxSortingOrder)
        {
            this.itemId = itemId;
            this.closedSprite = closedSprite;
            this.openSprite = openSprite;
            EnsureSetup(boxSortingOrder);
        }

        private void Awake() => EnsureSetup(0);

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other) => HandlePlayerContact(other);
        private void OnTriggerStay(Collider other) => HandlePlayerContact(other);

        private void HandlePlayerContact(Collider other)
        {
            if (!isActiveAndEnabled || collected || rewardRequested || other == null
                || (reward == null && string.IsNullOrWhiteSpace(itemId))) return;

            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player == null || player.IsDead) return;
            PlayerItemManager itemManager = player.GetComponent<PlayerItemManager>();
            if (itemManager == null) return;

            var ui = player.GetComponent<PlayerItemInventoryUI>();
            if (ui == null) ui = player.gameObject.AddComponent<PlayerItemInventoryUI>();
            // Another modal can temporarily block the request. Stay retries after it closes.
            if (!ui.CanRequestReward) return;

            reward = WorldItemRewardChoice.Prepare(reward, itemManager, this, itemId);
            if (reward.collected) { collected = true; Destroy(gameObject); return; }

            rewardRequested = true;
            if (!ui.RequestReward(reward.itemChoices, selected =>
            {
                if (this == null || collected || reward.collected) return;
                collected = true;
                WorldItemRewardChoice.PlayPickup(player, selected, player.transform.position);
                SaveService.CompleteWorldReward(reward);
                Destroy(gameObject);
            }))
            {
                rewardRequested = false;
                return;
            }

            // Open the chest and selection together, without a world-item reveal or delay.
            if (boxRenderer != null && openSprite != null) boxRenderer.sprite = openSprite;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() != null) rewardRequested = false;
        }

        private void EnsureSetup(int boxSortingOrder)
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            boxRenderer = GetComponentInChildren<SpriteRenderer>(true);
            if (boxRenderer == null) boxRenderer = gameObject.AddComponent<SpriteRenderer>();
            if (closedSprite != null) boxRenderer.sprite = closedSprite;
            if (boxSortingOrder != 0) boxRenderer.sortingOrder = boxSortingOrder;
        }
    }
}
