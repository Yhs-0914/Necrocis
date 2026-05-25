using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public enum PlayerItemAcquireFailureReason
    {
        None = 0,
        InvalidItemId,
        NotFoundInCatalog,
        DuplicateNotAllowed,
        SlotsFull
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerItemManager : MonoBehaviour
    {
        private const string RemovedParasiticSporeItemId = "parasitic_spore";

        [Serializable]
        public class PlayerItemEntry
        {
            [SerializeField] private string itemId;
            [SerializeField] private string displayName;
            [TextArea]
            [SerializeField] private string description;
            [SerializeField] private PlayerItemCategory category;
            [SerializeField] private Sprite icon;
            [SerializeField] private PlayerItemBase implementation;

            public string ItemId => itemId;
            public string DisplayName => displayName;
            public string Description => description;
            public PlayerItemCategory Category => category;
            public Sprite Icon => icon;
            public PlayerItemBase Implementation => implementation;

            public PlayerItemEntry()
            {
            }

            public PlayerItemEntry(
                string itemId,
                string displayName,
                string description,
                PlayerItemCategory category,
                Sprite icon = null,
                PlayerItemBase implementation = null)
            {
                this.itemId = itemId;
                this.displayName = displayName;
                this.description = description;
                this.category = category;
                this.icon = icon;
                this.implementation = implementation;
            }
        }

        [Serializable]
        public class AcquiredPlayerItem
        {
            [SerializeField] private string itemId;
            [SerializeField] private string displayName;
            [SerializeField] private string description;
            [SerializeField] private PlayerItemCategory category;
            [SerializeField] private Sprite icon;
            [SerializeField] private PlayerItemBase implementation;

            public string ItemId => itemId;
            public string DisplayName => displayName;
            public string Description => description;
            public PlayerItemCategory Category => category;
            public Sprite Icon => icon;
            public PlayerItemBase Implementation => implementation;

            public AcquiredPlayerItem(PlayerItemEntry entry)
            {
                itemId = entry.ItemId;
                displayName = entry.DisplayName;
                description = entry.Description;
                category = entry.Category;
                icon = entry.Icon;
                implementation = entry.Implementation;
            }
        }

        private static PlayerItemManager instance;

        public static PlayerItemManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<PlayerItemManager>();
                }

                return instance;
            }
        }

        [Header("Slots")]
        [SerializeField, Min(1)] private int maxItemSlots = 3;
        [SerializeField] private bool allowDuplicateItems;

        [Header("Catalog")]
        [SerializeField] private List<PlayerItemEntry> itemEntries = new List<PlayerItemEntry>();
        [SerializeField] private List<string> startingItemIds = new List<string>();

        [Header("Template")]
        [SerializeField] private bool autoPopulateBasicProjectileItems = true;
        [SerializeField] private bool enablePickupNotification = true;

        private readonly List<AcquiredPlayerItem> acquiredItems = new List<AcquiredPlayerItem>();
        private readonly Dictionary<string, PlayerItemEntry> entryMap = new Dictionary<string, PlayerItemEntry>(StringComparer.OrdinalIgnoreCase);
        private PlayerStats playerStats;

        public event Action<PlayerItemManager, AcquiredPlayerItem> ItemAcquired;
        public event Action<PlayerItemManager, AcquiredPlayerItem> ItemRemoved;

        public int MaxItemSlots => maxItemSlots;
        public int ItemCount => acquiredItems.Count;
        public bool IsFull => acquiredItems.Count >= maxItemSlots;
        public IReadOnlyList<PlayerItemEntry> ItemEntries => itemEntries;
        public IReadOnlyList<AcquiredPlayerItem> AcquiredItems => acquiredItems;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this);
                return;
            }

            playerStats = GetComponent<PlayerStats>();
            PurgeRemovedItemsFromCatalog();
            RebuildCatalog();

            if (enablePickupNotification && GetComponent<PlayerItemPickupNotifier>() == null)
            {
                gameObject.AddComponent<PlayerItemPickupNotifier>();
            }

            if (GetComponent<PlayerItemCombatEffects>() == null)
            {
                gameObject.AddComponent<PlayerItemCombatEffects>();
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Start()
        {
            if (startingItemIds == null || startingItemIds.Count == 0)
            {
                return;
            }

            for (int i = 0; i < startingItemIds.Count && !IsFull; i++)
            {
                TryAcquireItem(startingItemIds[i], out _);
            }
        }

        private void OnValidate()
        {
            if (maxItemSlots < 1)
            {
                maxItemSlots = 1;
            }

            if (!Application.isPlaying && autoPopulateBasicProjectileItems && (itemEntries == null || itemEntries.Count == 0))
            {
                PopulateBasicProjectileTemplateItems();
            }

            PurgeRemovedItemsFromCatalog();
            RebuildCatalog();
        }

        [ContextMenu("Populate Basic Projectile Items")]
        public void PopulateBasicProjectileTemplateItems()
        {
            itemEntries = BuildBasicProjectileTemplateItems();
            RebuildCatalog();
        }

        private static List<PlayerItemEntry> BuildBasicProjectileTemplateItems()
        {
            return new List<PlayerItemEntry>
            {
                new PlayerItemEntry("double_core", "이중핵", "발사체 2개 발사", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("triple_core", "삼중핵", "발사체 3개 발사", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("homing_cell", "유도 세포", "적을 추적하는 유도 투사체", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("hypertrophy_cell", "비대 세포", "투사체 크기 증가 + 공격 범위 증가", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("reflux_organ", "역류 장기", "투사체가 되돌아오는 부메랑 형태", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("piercing_mucus", "관통 점액", "적 여러 명을 관통하는 공격", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("laryngeal_nerve", "후두 신경", "뒤쪽으로 추가 투사체 발사", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("beam_organ", "광선 기관", "일반 투사체 대신 레이저 공격", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("split_tissue", "분열 조직", "적 명중 시 양옆으로 추가 투사체 생성", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("explosive_blood_cell", "폭발 혈구", "투사체가 범위 폭발 공격으로 변경", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("acidic_rupture", "산성 파열", "공격 적중 시 바닥에 지속 피해 장판 생성", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("cell_proliferation", "세포 증식", "일정 확률로 공격이 한 번 더 발동", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("pulse_bullet", "맥동 탄환", "투사체가 일정 거리마다 커졌다 작아짐", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("vascular_reflection", "혈관 반사", "벽에 튕기는 반사 투사체", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("toxic_mucosa", "독성 점막", "공격 적중 시 중독 피해 부여", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("freezing_nerve", "빙결 신경", "공격 적중 시 적 이동속도 감소", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("hemorrhage_organ", "출혈 기관", "적중 시 지속 출혈 피해", PlayerItemCategory.BasicProjectile)
            };
        }

        public void RebuildCatalog()
        {
            entryMap.Clear();
            if (itemEntries == null)
            {
                return;
            }

            for (int i = 0; i < itemEntries.Count; i++)
            {
                PlayerItemEntry entry = itemEntries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    continue;
                }

                if (!entryMap.ContainsKey(entry.ItemId))
                {
                    entryMap.Add(entry.ItemId, entry);
                }
            }
        }

        private void PurgeRemovedItemsFromCatalog()
        {
            if (itemEntries == null || itemEntries.Count == 0)
            {
                return;
            }

            for (int i = itemEntries.Count - 1; i >= 0; i--)
            {
                PlayerItemEntry entry = itemEntries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    continue;
                }

                if (string.Equals(entry.ItemId, RemovedParasiticSporeItemId, StringComparison.OrdinalIgnoreCase))
                {
                    itemEntries.RemoveAt(i);
                }
            }
        }

        public bool TryAcquireItem(string itemId, out PlayerItemAcquireFailureReason failureReason)
        {
            failureReason = PlayerItemAcquireFailureReason.None;

            if (string.IsNullOrWhiteSpace(itemId))
            {
                failureReason = PlayerItemAcquireFailureReason.InvalidItemId;
                return false;
            }

            if (IsFull)
            {
                failureReason = PlayerItemAcquireFailureReason.SlotsFull;
                return false;
            }

            if (!entryMap.TryGetValue(itemId, out PlayerItemEntry entry))
            {
                failureReason = PlayerItemAcquireFailureReason.NotFoundInCatalog;
                return false;
            }

            if (!allowDuplicateItems && ContainsItem(itemId))
            {
                failureReason = PlayerItemAcquireFailureReason.DuplicateNotAllowed;
                return false;
            }

            AcquiredPlayerItem acquiredItem = new AcquiredPlayerItem(entry);
            acquiredItems.Add(acquiredItem);

            if (acquiredItem.Implementation != null)
            {
                acquiredItem.Implementation.ApplyTo(playerStats);
            }

            ItemAcquired?.Invoke(this, acquiredItem);
            Debug.Log($"[PlayerItemManager] 아이템 획득: {acquiredItem.DisplayName} ({acquiredItem.ItemId}) [{acquiredItems.Count}/{maxItemSlots}]");
            return true;
        }

        public bool RemoveItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            for (int i = 0; i < acquiredItems.Count; i++)
            {
                AcquiredPlayerItem acquiredItem = acquiredItems[i];
                if (!string.Equals(acquiredItem.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (acquiredItem.Implementation != null)
                {
                    acquiredItem.Implementation.RemoveFrom(playerStats);
                }

                acquiredItems.RemoveAt(i);
                ItemRemoved?.Invoke(this, acquiredItem);
                return true;
            }

            return false;
        }

        public void ClearAllItems()
        {
            for (int i = acquiredItems.Count - 1; i >= 0; i--)
            {
                AcquiredPlayerItem acquiredItem = acquiredItems[i];
                if (acquiredItem.Implementation != null)
                {
                    acquiredItem.Implementation.RemoveFrom(playerStats);
                }
            }

            acquiredItems.Clear();
        }

        public bool ContainsItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            for (int i = 0; i < acquiredItems.Count; i++)
            {
                if (string.Equals(acquiredItems[i].ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetItemEntry(string itemId, out PlayerItemEntry entry)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                entry = null;
                return false;
            }

            return entryMap.TryGetValue(itemId, out entry);
        }
    }
}
