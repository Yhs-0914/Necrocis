using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class WorldItemPickup : MonoBehaviour
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        private bool slotsFullLogged;
        private bool collected;

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
                slotsFullLogged = false;
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

            if (itemManager.TryAcquireItem(itemId, out PlayerItemAcquireFailureReason failure))
            {
                PlayerItemCategory category = PlayerItemCategory.BasicProjectile;
                AudioManager.Instance?.PlaySFX("ItemPickup");
                if (itemManager.TryGetItemEntry(itemId, out PlayerItemManager.PlayerItemEntry entry))
                {
                    category = entry.Category;
                    AudioManager.Instance?.PlayItemCategorySFX(entry.Category);
                }
                CombatVfx.PlayItemPickup(
                    transform.position + Vector3.up * 0.45f,
                    player.transform,
                    category);
                collected = true;
                Destroy(gameObject);
                return;
            }

            if (failure == PlayerItemAcquireFailureReason.SlotsFull && !slotsFullLogged)
            {
                slotsFullLogged = true;
                Debug.Log($"[WorldItemPickup] 아이템 슬롯이 가득 참: {displayName} ({itemId}). I 키로 인벤토리를 열어 아이템을 버릴 수 있습니다.");
            }
        }
    }
}
