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
<<<<<<< Updated upstream
        private const string RemovedParasiticSporeItemId = "parasitic_spore";
        private const string RemovedNeuralOverloadItemId = "neural_overload";
        private const string RemovedCoagulationCellItemId = "coagulation_cell";
=======
        private static readonly HashSet<string> RemovedItemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "parasitic_spore"
        };
>>>>>>> Stashed changes

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
                if (!string.IsNullOrWhiteSpace(text))
                {
                    description = text;
                }
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
            EnsureTemplateCatalogEntries();
            RebuildCatalog();

            if (enablePickupNotification)
            {
                EnsureComponent<PlayerItemPickupNotifier>();
            }

<<<<<<< Updated upstream
            if (GetComponent<PlayerItemCombatEffects>() == null)
            {
                gameObject.AddComponent<PlayerItemCombatEffects>();
            }

            if (GetComponent<PlayerItemTestPanel>() == null)
            {
                gameObject.AddComponent<PlayerItemTestPanel>();
            }
=======
            EnsureComponent<PlayerItemCombatEffects>();
>>>>>>> Stashed changes
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
                new PlayerItemEntry("pulse_bullet", "맥동 탄환", "투사체가 멀리갈수록 크기가 증가하거나 작아짐", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("vascular_reflection", "혈관 반사", "벽에 튕기는 반사 투사체", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("toxic_mucosa", "독성 점막", "공격 적중 시 중독 피해 부여", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("freezing_nerve", "빙결 신경", "공격 적중 시 적 이동속도 감소", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("hemorrhage_organ", "출혈 기관", "적중 시 지속 출혈 피해", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("overheated_organ", "과열 기관", "계속 공격 시 공격속도 증가, 과열 시 폭발 피해(자해형)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("mutant_eye", "돌연변이 안구", "전방 180도 범위로 무작위 발사, 공격력 +5", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("organ_tentacle", "장기 촉수", "주변 적 자동 공격, 대신 기본 공격 약화", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("rampage_bloodflow", "폭주 혈류", "이동 중 공격력 증가, 멈추면 감소", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("muscle_spasm", "근육 경련", "근접 공격 범위 증가, 공격속도 감소", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("unstable_core", "불안정 핵", "공격력이 랜덤하게 크게 변동됨", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("bio_resonance", "생체 공명", "같은 적 연속 공격 시 해당 적에게 피해량 증가", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("blood_pressure_burst", "혈압 폭발", "체력이 낮을수록 공격속도 증가", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("void_cell", "공허 세포", "공격 시 일정 확률로 순간이동 탄환 생성", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("forbidden_growth", "금단 성장", "체력 -4, 공격력 +6 증가", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("overclock_nerve", "과속 신경", "체력 -2, 이동속도 +4 증가", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("blood_contract", "피의 계약", "적 10명 처치 시 체력 +1 증가, 대신 받는 피해 1.5배", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("hyperplasia_heart", "과증식 심장", "체력이 낮을수록 공격력 증가", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("decay_organ", "부패 장기", "시간이 지날수록 공격력 증가", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("rupture_muscle", "파열 근육", "공격할수록 강해지지만 이동속도 감소", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("imperfect_regeneration", "불완전 재생", "체력 -4, 피해를 받으면 3초 후 체력 +1 회복 (15초 쿨타임)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("severance_reflex", "절단 반사", "피격 직후 공격력 +6", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("bio_gamble", "생체 도박", "레벨업 선택 시 스탯이 -2~+3 범위에서 랜덤 증감", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("exoskeleton", "외골격", "받는 피해 30% 감소, 대신 이동속도 -2", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("platelet_membrane", "혈소판 막", "30초마다 보호막 1 생성", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("recovery_factor", "회복 인자", "45초당 체력 +1 회복", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("reflective_skin", "반사 피부", "피격 시 적에게 피해 반사", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("bio_barrier", "생체 장막", "이동하지 않으면 피해 감소 증가 (최대 50%)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("split_regeneration", "분열 재생", "체력 0이 될 때 작은 체력으로 부활", PlayerItemCategory.BasicProjectile)
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

                ClearRuntimeFallbackIconForSpecialItems(entry);
                AssignFallbackIcon(entry);
                NormalizeDescription(entry);

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

<<<<<<< Updated upstream
                if (string.Equals(entry.ItemId, RemovedParasiticSporeItemId, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(entry.ItemId, RemovedNeuralOverloadItemId, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(entry.ItemId, RemovedCoagulationCellItemId, StringComparison.OrdinalIgnoreCase))
=======
                if (RemovedItemIds.Contains(entry.ItemId))
>>>>>>> Stashed changes
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

                if (string.Equals(itemId, RemovedParasiticSporeItemId, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(itemId, RemovedNeuralOverloadItemId, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(itemId, RemovedCoagulationCellItemId, StringComparison.OrdinalIgnoreCase))
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
                case PlayerItemCombatEffects.OverheatedOrganId:
                case PlayerItemCombatEffects.MutantEyeId:
                case PlayerItemCombatEffects.OrganTentacleId:
                case PlayerItemCombatEffects.RampageBloodFlowId:
                case PlayerItemCombatEffects.MuscleSpasmId:
                case PlayerItemCombatEffects.UnstableCoreId:
                case PlayerItemCombatEffects.BioResonanceId:
                case PlayerItemCombatEffects.BloodPressureBurstId:
                case PlayerItemCombatEffects.VoidCellId:
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
                case PlayerItemCombatEffects.BeamOrganId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.35f, 0.15f), true));
                    break;
                case PlayerItemCombatEffects.ExplosiveBloodCellId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.16f, 0.14f), false));
                    break;
                case PlayerItemCombatEffects.AcidicRuptureId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.32f, 0.94f, 0.28f), false));
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

        private static void NormalizeDescription(PlayerItemEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            if (string.Equals(entry.ItemId, PlayerItemCombatEffects.PulseBulletId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("투사체가 멀리갈수록 크기가 증가하거나 작아짐");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.BioResonanceId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("같은 적 연속 공격 시 해당 적에게 피해량 증가");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.ForbiddenGrowthId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("체력 -4, 공격력 +6 증가");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.OverclockNerveId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("체력 -2, 이동속도 +4 증가");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.BloodContractId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("적 10명 처치 시 체력 +1 증가, 대신 받는 피해 1.5배");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.ImperfectRegenerationId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("체력 -4, 피해를 받으면 3초 후 체력 +1 회복 (15초 쿨타임)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.SeveranceReflexId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("피격 직후 공격력 +6");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.ExoskeletonId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("받는 피해 30% 감소, 대신 이동속도 -2");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.PlateletMembraneId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("30초마다 보호막 1 생성");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.RecoveryFactorId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("45초당 체력 +1 회복");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.BioBarrierId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("이동하지 않으면 피해 감소 증가 (최대 50%)");
            }
        }

        private T EnsureComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
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
