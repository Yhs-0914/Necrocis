using UnityEngine;

namespace Necrocis
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class WorldItemPickup : MonoBehaviour
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;

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
            if (string.IsNullOrWhiteSpace(itemId))
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
                Destroy(gameObject);
                return;
            }

            if (failure == PlayerItemAcquireFailureReason.SlotsFull)
            {
                Debug.Log($"[WorldItemPickup] 슬롯이 가득 차서 획득 실패: {displayName} ({itemId})");
            }
        }
    }
}
