using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class WorldItemPickup : MonoBehaviour
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        private bool rewardRequested;
        private bool collected;
        private WorldRewardSaveData reward;

        public void Initialize(string itemId, string displayName)
        {
            this.itemId = itemId;
            this.displayName = displayName;
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

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() != null)
            {
                rewardRequested = false;
            }
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

            PlayerItemManager itemManager = player.GetComponent<PlayerItemManager>();
            if (itemManager == null)
            {
                return;
            }

            if (rewardRequested) return;
            reward = WorldItemRewardChoice.Prepare(reward, itemManager, this, itemId);
            if (reward.collected) { collected = true; Destroy(gameObject); return; }
            var ui = player.GetComponent<PlayerItemInventoryUI>();
            if (ui == null) ui = player.gameObject.AddComponent<PlayerItemInventoryUI>();
            rewardRequested = ui.RequestReward(reward.itemChoices, selected =>
            {
                if (this == null || reward.collected) return;
                WorldItemRewardChoice.PlayPickup(player, selected, transform.position + Vector3.up * 0.45f);
                SaveService.CompleteWorldReward(reward);
                collected = true;
                Destroy(gameObject);
            });
        }
    }
}
