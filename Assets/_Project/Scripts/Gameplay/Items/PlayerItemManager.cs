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
        private static readonly HashSet<string> RemovedItemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "parasitic_spore",
            "neural_overload",
            "coagulation_cell"
        };

        [Serializable]
        public class PlayerItemEntry
        {
            [SerializeField] private string itemId;
            [SerializeField] private string displayName;
            [TextArea]
            [SerializeField] private string description;
            [SerializeField] private ItemDescriptionMode descriptionMode;
            [NonSerialized] private Func<PlayerItemEntry, string> readDescription;
            [SerializeField] private PlayerItemCategory category;
            [SerializeField] private Sprite icon;
            [SerializeField] private PlayerItemBase implementation;

            public string ItemId => itemId;
            public string DisplayName => displayName;
            public string Description => readDescription?.Invoke(this) ?? description;
            internal string RawDescription => description;
            public ItemDescriptionMode DescriptionMode { get => descriptionMode; set => descriptionMode = value; }
            internal void BindDescription(Func<PlayerItemEntry, string> resolver) => readDescription = resolver;
            public PlayerItemCategory Category => category;
            public Sprite Icon => icon;
            public PlayerItemBase Implementation => implementation;
            public bool HasIcon => icon != null;

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

            public void AssignIconIfMissing(Sprite fallbackIcon)
            {
                if (icon == null)
                {
                    icon = fallbackIcon;
                }
            }

            public void ClearRuntimeIconIfMatchesPrefix(string prefix)
            {
                if (icon == null || string.IsNullOrEmpty(prefix))
                {
                    return;
                }

                if (icon.name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    icon = null;
                }
            }

            public void SetDescription(string text)
            {
                description = text ?? string.Empty;
                descriptionMode = ItemDescriptionMode.Custom;
            }
        }

        [Serializable]
        public class AcquiredPlayerItem : IInventoryItemView
        {
            [SerializeField] private string itemId;
            [SerializeField] private string displayName;
            [SerializeField] private string description;
            [NonSerialized] private PlayerItemEntry definition;
            [SerializeField] private PlayerItemCategory category;
            [SerializeField] private Sprite icon;
            [SerializeField] private PlayerItemBase implementation;

            public string ItemId => itemId;
            public string DisplayName => displayName;
            public string Description => definition != null ? definition.Description : description;
            public PlayerItemCategory Category => category;
            public Sprite Icon => icon;
            public PlayerItemBase Implementation => implementation;

            public AcquiredPlayerItem(PlayerItemEntry entry)
            {
                definition = entry;
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
        private IItemStatRecipient playerStats;
        private IItemEffectLifecycle combatEffects;
        private bool isRestoringSavedItems;

        public event Action<PlayerItemManager, AcquiredPlayerItem> ItemAcquired;
        public event Action<PlayerItemManager, AcquiredPlayerItem> ItemRemoved;

        public int MaxItemSlots => maxItemSlots;
        public int ItemCount => acquiredItems.Count;
        public bool IsFull => acquiredItems.Count >= maxItemSlots;
        public IReadOnlyList<PlayerItemEntry> ItemEntries => itemEntries;
        public IReadOnlyList<AcquiredPlayerItem> AcquiredItems => acquiredItems;
        public bool IsRestoringSavedItems => isRestoringSavedItems;
        public bool PickupNotificationsEnabled => enablePickupNotification;

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

            int slotOverride = ItemInventoryBindings.SlotOverride?.Invoke() ?? 0;
            if (slotOverride > 0)
            {
                maxItemSlots = slotOverride;
            }

            playerStats = ItemInventoryBindings.StatRecipient?.Invoke(gameObject);
            PurgeRemovedItemsFromCatalog();
            EnsureTemplateCatalogEntries();
            RebuildCatalog();


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
            EnsureTemplateCatalogEntries();
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
            var entries = new List<PlayerItemEntry>(ItemDefinitions.All.Count);
            foreach (var definition in ItemDefinitions.All)
                entries.Add(new PlayerItemEntry(definition.ItemId, definition.DisplayName,
                    definition.LegacyDescription, definition.Category));
            return entries;
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

                ClearRuntimeFallbackIconForSpecialItems(entry);
                AssignFallbackIcon(entry);
                entry.BindDescription(ResolveDescription);

                if (!entryMap.ContainsKey(entry.ItemId))
                {
                    entryMap.Add(entry.ItemId, entry);
                }
            }
        }

        private void EnsureTemplateCatalogEntries()
        {
            if (!autoPopulateBasicProjectileItems)
            {
                return;
            }

            if (itemEntries == null)
            {
                itemEntries = new List<PlayerItemEntry>();
            }

            HashSet<string> existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < itemEntries.Count; i++)
            {
                PlayerItemEntry entry = itemEntries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    continue;
                }

                existingIds.Add(entry.ItemId);
            }

            List<PlayerItemEntry> templates = BuildBasicProjectileTemplateItems();
            for (int i = 0; i < templates.Count; i++)
            {
                PlayerItemEntry template = templates[i];
                if (template == null || string.IsNullOrWhiteSpace(template.ItemId))
                {
                    continue;
                }

                if (existingIds.Contains(template.ItemId))
                {
                    continue;
                }

                itemEntries.Add(template);
                existingIds.Add(template.ItemId);
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

                if (RemovedItemIds.Contains(entry.ItemId))
                {
                    itemEntries.RemoveAt(i);
                }
            }

            if (startingItemIds == null || startingItemIds.Count == 0)
            {
                return;
            }

            for (int i = startingItemIds.Count - 1; i >= 0; i--)
            {
                string itemId = startingItemIds[i];
                if (string.IsNullOrWhiteSpace(itemId))
                {
                    continue;
                }

                if (RemovedItemIds.Contains(itemId))
                {
                    startingItemIds.RemoveAt(i);
                }
            }
        }

        private static readonly Dictionary<string, Sprite> FallbackIconCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

        private static void ClearRuntimeFallbackIconForSpecialItems(PlayerItemEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId))
            {
                return;
            }

            switch (entry.ItemId)
            {
                case PlayerItemIds.OverheatedOrganId:
                case PlayerItemIds.MutantEyeId:
                case PlayerItemIds.OrganTentacleId:
                case PlayerItemIds.RampageBloodFlowId:
                case PlayerItemIds.MuscleSpasmId:
                case PlayerItemIds.UnstableCoreId:
                case PlayerItemIds.BioResonanceId:
                case PlayerItemIds.BloodPressureBurstId:
                case PlayerItemIds.VoidCellId:
                case PlayerItemIds.InfectedHostId:
                case PlayerItemIds.SporeColonyId:
                case PlayerItemIds.BloodDroneId:
                case PlayerItemIds.GuardianOrganId:
                case PlayerItemIds.TentacleColonyId:
                case PlayerItemIds.ElectricNeuralNetworkId:
                case PlayerItemIds.InfectionTransferenceId:
                case PlayerItemIds.MacrophageId:
                case PlayerItemIds.GluttonousOrganId:
                case PlayerItemIds.HeartSniperId:
                case PlayerItemIds.BloodflowAccelerationId:
                case PlayerItemIds.FocusedNerveId:
                case PlayerItemIds.ExecutionInstinctId:
                case PlayerItemIds.BerserkCellId:
                case PlayerItemIds.UnstableCellId:
                case PlayerItemIds.GrotesqueGrowthId:
                case PlayerItemIds.MutationRampageId:
                case PlayerItemIds.ParasiticBombId:
                case PlayerItemIds.FrenzyHormoneId:
                    entry.ClearRuntimeIconIfMatchesPrefix("RuntimeItemIcon_");
                    break;
            }
        }

        private static void AssignFallbackIcon(PlayerItemEntry entry)
        {
            if (entry == null || entry.HasIcon || string.IsNullOrWhiteSpace(entry.ItemId))
            {
                return;
            }

            switch (entry.ItemId)
            {
                case PlayerItemIds.BeamOrganId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.35f, 0.15f), true));
                    break;
                case PlayerItemIds.ExplosiveBloodCellId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.16f, 0.14f), false));
                    break;
                case PlayerItemIds.AcidicRuptureId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.32f, 0.94f, 0.28f), false));
                    break;
                case PlayerItemIds.InfectedHostId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.92f, 0.96f, 0.9f), false));
                    break;
                case PlayerItemIds.SporeColonyId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.52f, 1f, 0.45f), false));
                    break;
                case PlayerItemIds.BloodDroneId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.95f, 0.08f, 0.18f), true));
                    break;
                case PlayerItemIds.GuardianOrganId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.68f, 0.82f, 1f), false));
                    break;
                case PlayerItemIds.TentacleColonyId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.68f, 0.24f, 0.86f), true));
                    break;
                case PlayerItemIds.ElectricNeuralNetworkId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.45f, 0.82f, 1f), true));
                    break;
                case PlayerItemIds.InfectionTransferenceId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.42f, 0.95f, 0.32f), false));
                    break;
                case PlayerItemIds.MacrophageId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.34f, 0.18f), false));
                    break;
                case PlayerItemIds.GluttonousOrganId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.96f, 0.52f, 0.22f), true));
                    break;
                case PlayerItemIds.HeartSniperId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.12f, 0.18f), true));
                    break;
                case PlayerItemIds.BloodflowAccelerationId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.95f, 0.05f, 0.08f), true));
                    break;
                case PlayerItemIds.FocusedNerveId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.78f, 0.9f, 1f), false));
                    break;
                case PlayerItemIds.ExecutionInstinctId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.96f, 0.86f, 0.2f), true));
                    break;
                case PlayerItemIds.BerserkCellId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.24f, 0.08f), false));
                    break;
                case PlayerItemIds.UnstableCellId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.75f, 0.38f, 1f), true));
                    break;
                case PlayerItemIds.GrotesqueGrowthId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.42f, 1f, 0.58f), false));
                    break;
                case PlayerItemIds.MutationRampageId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.42f, 0.95f), true));
                    break;
                case PlayerItemIds.ParasiticBombId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.88f, 0.12f, 0.68f), false));
                    break;
                case PlayerItemIds.FrenzyHormoneId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.72f, 0.12f), true));
                    break;
            }
        }

        private static Sprite GetFallbackIcon(string key, Color accentColor, bool beamPattern)
        {
            if (FallbackIconCache.TryGetValue(key, out Sprite cached) && cached != null)
            {
                return cached;
            }

            const int size = 48;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            Color edge = new Color(accentColor.r * 0.35f, accentColor.g * 0.35f, accentColor.b * 0.35f, 1f);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.44f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance > radius)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float radial = Mathf.InverseLerp(radius, 0f, distance);
                    Color baseColor = Color.Lerp(edge, accentColor, radial);
                    if (beamPattern)
                    {
                        float stripe = Mathf.Abs((x - center.x) / Mathf.Max(1f, radius));
                        float mask = Mathf.SmoothStep(0.85f, 0.15f, stripe);
                        baseColor = Color.Lerp(baseColor * 0.55f, new Color(1f, 0.93f, 0.75f, 1f), mask);
                    }

                    texture.SetPixel(x, y, baseColor);
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = $"RuntimeItemIcon_{key}";
            FallbackIconCache[key] = sprite;
            return sprite;
        }

        private string ResolveDescription(PlayerItemEntry entry)
        {
            var policy = ItemInventoryBindings.DescriptionPolicy();
            return ItemDescriptionReadModel.Resolve(entry.ItemId, entry.RawDescription, entry.DescriptionMode,
                combatEffects != null && combatEffects.IsAvailable ? combatEffects.DescribeItem : null,
                policy.GambleEnabled,
                policy.GambleMinimum,
                policy.GambleMaximum);
        }

        internal void BindRuntime(IItemStatRecipient stats, IItemEffectLifecycle effects)
        {
            playerStats = stats;
            combatEffects = effects;
        }

        private void EnsureCombatEffectsForInventoryChange()
        {
            combatEffects?.EnsureActiveForInventoryChange();
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
                ItemInventoryBindings.ApplyImplementation(acquiredItem.Implementation,playerStats);
            }

            EnsureCombatEffectsForInventoryChange();
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
                    ItemInventoryBindings.RemoveImplementation(acquiredItem.Implementation,playerStats);
                }

                acquiredItems.RemoveAt(i);
                combatEffects?.RetireItemHealthSources(acquiredItem.ItemId);
                ItemRemoved?.Invoke(this, acquiredItem);
                return true;
            }

            return false;
        }

        public void ClearAllItems()
        {
            if (acquiredItems.Count == 0)
            {
                return;
            }

            List<AcquiredPlayerItem> removedItems = new List<AcquiredPlayerItem>(acquiredItems);
            for (int i = acquiredItems.Count - 1; i >= 0; i--)
            {
                AcquiredPlayerItem acquiredItem = acquiredItems[i];
                if (acquiredItem.Implementation != null)
                {
                    ItemInventoryBindings.RemoveImplementation(acquiredItem.Implementation,playerStats);
                }
            }

            acquiredItems.Clear();

            for (int i = 0; i < removedItems.Count; i++)
            {
                combatEffects?.RetireItemHealthSources(removedItems[i].ItemId);
                ItemRemoved?.Invoke(this, removedItems[i]);
            }
        }

        public List<SavedItemStateData> CaptureSavedItems()
        {
            List<SavedItemStateData> result = new List<SavedItemStateData>(acquiredItems.Count);
            IItemEffectLifecycle combatEffects = GetComponent<IItemEffectLifecycle>();
            for (int i = 0; i < acquiredItems.Count; i++)
            {
                AcquiredPlayerItem item = acquiredItems[i];
                if (item == null || string.IsNullOrWhiteSpace(item.ItemId))
                {
                    continue;
                }

                SavedItemStateData state = new SavedItemStateData { itemId = item.ItemId };
                combatEffects?.CapturePersistentItemState(state);
                result.Add(state);
            }

            return result;
        }

        public void RestoreSavedItems(IReadOnlyList<SavedItemStateData> savedItems)
        {
            isRestoringSavedItems = true;
            try
            {
                ClearAllItems();
                if (savedItems != null)
                {
                    for (int i = 0; i < savedItems.Count; i++)
                    {
                        SavedItemStateData state = savedItems[i];
                        if (state == null || string.IsNullOrWhiteSpace(state.itemId))
                        {
                            continue;
                        }

                        if (!TryAcquireItem(state.itemId, out PlayerItemAcquireFailureReason failureReason))
                        {
                            Debug.LogWarning(
                                $"[PlayerItemManager] 저장 아이템 복원 실패: {state.itemId} ({failureReason})");
                        }
                    }
                }

                GetComponent<IItemEffectLifecycle>()?.RestorePersistentItemStates(savedItems);
            }
            finally
            {
                isRestoringSavedItems = false;
            }
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
