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
        private bool isRestoringSavedItems;

        public event Action<PlayerItemManager, AcquiredPlayerItem> ItemAcquired;
        public event Action<PlayerItemManager, AcquiredPlayerItem> ItemRemoved;

        public int MaxItemSlots => maxItemSlots;
        public int ItemCount => acquiredItems.Count;
        public bool IsFull => acquiredItems.Count >= maxItemSlots;
        public IReadOnlyList<PlayerItemEntry> ItemEntries => itemEntries;
        public IReadOnlyList<AcquiredPlayerItem> AcquiredItems => acquiredItems;
        public bool IsRestoringSavedItems => isRestoringSavedItems;

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

            int slotOverride = DifficultyBalanceService.ActiveProfile?.items?.maxSlotsOverride ?? 0;
            if (slotOverride > 0)
            {
                maxItemSlots = slotOverride;
            }

            playerStats = GetComponent<PlayerStats>();
            PurgeRemovedItemsFromCatalog();
            EnsureTemplateCatalogEntries();
            RebuildCatalog();

            if (enablePickupNotification)
            {
                EnsureComponent<PlayerItemPickupNotifier>();
            }

            EnsureComponent<PlayerItemCombatEffects>();
            EnsureComponent<PlayerItemInventoryUI>();
            EnsureComponent<PlayerItemTestPanel>();
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
                new PlayerItemEntry("double_core", "이중핵", "발사체 2개 발사 (각 75% 피해)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("triple_core", "삼중핵", "발사체 3개 발사 (각 55% 피해)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("homing_cell", "유도 세포", "적을 추적하는 유도 투사체", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("hypertrophy_cell", "비대 세포", "투사체 크기·충돌 범위 45%, 사거리 25% 증가", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("reflux_organ", "역류 장기", "왕복 경로마다 최대 3명 타격, 복귀 중 같은 적 재적중 시 75% 피해", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("piercing_mucus", "관통 점액", "최대 3명 관통, 관통할 때마다 피해 25% 감소", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("laryngeal_nerve", "후두 신경", "뒤쪽으로 추가 투사체 발사", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("beam_organ", "광선 기관", "80% 피해의 관통 광선 공격 (다중핵·후방 발사·세포 증식 적용)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("split_tissue", "분열 조직", "적 명중 시 양옆으로 각각 50% 피해의 추가 투사체 생성", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("explosive_blood_cell", "폭발 혈구", "원거리 피해가 50% 감소하고 범위 폭발 공격으로 변경", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("acidic_rupture", "산성 파열", "적중 위치에 4초간 1초마다 0.5 피해를 주는 산성 장판 생성", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("cell_proliferation", "세포 증식", "일정 확률로 공격이 한 번 더 발동", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("pulse_bullet", "맥동 탄환", "투사체가 이동하며 크기와 실제 충돌 범위가 증가하거나 감소", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("vascular_reflection", "혈관 반사", "벽에 튕기는 반사 투사체", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("toxic_mucosa", "독성 점막", "적중 시 4초간 1초마다 적중 피해의 20% 중독 피해", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("freezing_nerve", "빙결 신경", "공격 적중 시 2초간 적 이동속도 30% 감소", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("hemorrhage_organ", "출혈 기관", "적중 시 3.5초간 0.8초마다 적중 피해의 18% 출혈 피해", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("overheated_organ", "과열 기관", "연속 공격마다 공격속도 +10% (10중첩 시 주변 150% 피해, 자신 1 피해)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("mutant_eye", "돌연변이 안구", "발사 방향이 좌우 40도 내에서 무작위 변화, 공격력 +2", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("organ_tentacle", "장기 촉수", "주변 적 최대 2명을 1.2초마다 30% 피해로 공격, 기본 공격 피해 20% 감소", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("rampage_bloodflow", "폭주 혈류", "이동 8초마다 공격력 +1 (최대 +2), 정지 시 4초마다 감소", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("muscle_spasm", "근육 경련", "근접 공격 범위 1.7배, 공격속도 20% 감소", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("unstable_core", "불안정 핵", "5초마다 기본 공격 피해가 60~160% 사이에서 변경", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("bio_resonance", "생체 공명", "3초 내 같은 적 연속 공격 시 피해 +15% (최대 +45%)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("blood_pressure_burst", "혈압 폭발", "잃은 체력 10%당 공격속도 +6% (최대 +60%)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("void_cell", "공허 세포", "공격 시 20% 확률로 적 주변에서 85% 피해의 추가 탄환 생성", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("forbidden_growth", "금단 성장", "체력 -3, 공격력 +2 증가", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("overclock_nerve", "과속 신경", "체력 -2, 이동속도 +2 증가", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("blood_contract", "피의 계약", "적 20명 처치 시 체력 +1 (최대 +5), 받는 피해 1.3배", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("hyperplasia_heart", "과증식 심장", "잃은 체력에 비례해 공격력 증가 (최대 +2)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("decay_organ", "부패 장기", "3분마다 공격력 +1 (최대 +4)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("rupture_muscle", "파열 근육", "공격 2회마다 공격력 +0.5·이동속도 -0.25 (최대 3중첩)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("imperfect_regeneration", "불완전 재생", "체력 -2, 피격 2초 후 체력 +1 회복 (10초 쿨타임)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("severance_reflex", "절단 반사", "피격 후 2초간 공격력 +2", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("bio_gamble", "생체 도박", "레벨업 선택 시 성장량이 0~+2 범위에서 무작위 결정", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("exoskeleton", "외골격", "받는 피해 25% 감소, 대신 이동속도 -1.5", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("platelet_membrane", "혈소판 막", "20초마다 피해 1을 막는 보호막 생성", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("recovery_factor", "회복 인자", "30초마다 체력 +1 회복", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("reflective_skin", "반사 피부", "피격 시 실제 받은 피해의 75% 반사", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("bio_barrier", "생체 장막", "정지 1.5초마다 받는 피해 10% 감소 (최대 40%)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("split_regeneration", "분열 재생", "체력 0이 될 때 작은 체력으로 부활", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("infected_host", "감염 숙주", "잡몹 처치 시 15% 확률로 6초간 35% 피해의 아군 생성 (최대 2명)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("spore_colony", "포자 군집", "일정 시간마다 적에게 달려드는 작은 포자 생명체 소환", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("blood_drone", "혈액 드론", "1초마다 가까운 적에게 공격력의 30% 피해", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("guardian_organ", "수호 장기", "플레이어 주변을 맴돌며 3.5초마다 적 투사체 하나 방어", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("tentacle_colony", "촉수 군체", "3초마다 적 2명에게 1.2초간 60% 둔화와 10% 피해", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("electric_neural_network", "전기 신경망", "적 처치 시 최대 3명에게 피해 1의 연쇄 전류 (5초 쿨타임)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("infection_transference", "감염 전이", "적 처치 시 주변 3명에게 3초간 초당 0.5 감염 피해 (5초 쿨타임)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("macrophage", "대식 세포", "적 처치 시 5초간 공격력 +0.5 (최대 3중첩)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("gluttonous_organ", "폭식 장기", "적 처치 시 3초 동안 이동속도 증가 (최대 4스택)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("heart_sniper", "심장 저격", "체력 60% 이상인 적에게 직접 공격 피해 35% 증가", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("bloodflow_acceleration", "혈류 가속", "보스·엘리트 근처에서 공격속도 35% 증가", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("focused_nerve", "집중 신경", "주변 적 1명 이하일 때 공격력 +1.5, 2~3명일 때 +0.5", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("execution_instinct", "처형 본능", "체력 20% 미만 적을 20% 확률로 처형 (보스·엘리트는 추가 피해 50%)", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("berserk_cell", "광폭 세포", "보스전 진입 시 15초간 공격력 +1.5, 이동속도 +0.5, 공격속도 +50%", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("unstable_cell", "불안정 세포", "투사체 속도 50~150%, 80% 이하의 느린 탄은 피해 150%", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("grotesque_growth", "기괴 성장", "10초마다 소형화·이동속도 +1 또는 대형화·공격력 +1·이동속도 -0.5", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("mutation_rampage", "돌연변이 폭주", "15초마다 7초간 무작위 스탯 버프 또는 디버프", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("parasitic_bomb", "기생 폭탄", "적 처치 시 20% 확률로 반경 4에 공격력 150% 폭발", PlayerItemCategory.BasicProjectile),
                new PlayerItemEntry("frenzy_hormone", "광란 호르몬", "피격 시 무작위 능력 +0.5~1 (3초 지속, 8초 쿨타임)", PlayerItemCategory.BasicProjectile)
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
                case PlayerItemCombatEffects.OverheatedOrganId:
                case PlayerItemCombatEffects.MutantEyeId:
                case PlayerItemCombatEffects.OrganTentacleId:
                case PlayerItemCombatEffects.RampageBloodFlowId:
                case PlayerItemCombatEffects.MuscleSpasmId:
                case PlayerItemCombatEffects.UnstableCoreId:
                case PlayerItemCombatEffects.BioResonanceId:
                case PlayerItemCombatEffects.BloodPressureBurstId:
                case PlayerItemCombatEffects.VoidCellId:
                case PlayerItemCombatEffects.InfectedHostId:
                case PlayerItemCombatEffects.SporeColonyId:
                case PlayerItemCombatEffects.BloodDroneId:
                case PlayerItemCombatEffects.GuardianOrganId:
                case PlayerItemCombatEffects.TentacleColonyId:
                case PlayerItemCombatEffects.ElectricNeuralNetworkId:
                case PlayerItemCombatEffects.InfectionTransferenceId:
                case PlayerItemCombatEffects.MacrophageId:
                case PlayerItemCombatEffects.GluttonousOrganId:
                case PlayerItemCombatEffects.HeartSniperId:
                case PlayerItemCombatEffects.BloodflowAccelerationId:
                case PlayerItemCombatEffects.FocusedNerveId:
                case PlayerItemCombatEffects.ExecutionInstinctId:
                case PlayerItemCombatEffects.BerserkCellId:
                case PlayerItemCombatEffects.UnstableCellId:
                case PlayerItemCombatEffects.GrotesqueGrowthId:
                case PlayerItemCombatEffects.MutationRampageId:
                case PlayerItemCombatEffects.ParasiticBombId:
                case PlayerItemCombatEffects.FrenzyHormoneId:
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
                case PlayerItemCombatEffects.InfectedHostId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.92f, 0.96f, 0.9f), false));
                    break;
                case PlayerItemCombatEffects.SporeColonyId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.52f, 1f, 0.45f), false));
                    break;
                case PlayerItemCombatEffects.BloodDroneId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.95f, 0.08f, 0.18f), true));
                    break;
                case PlayerItemCombatEffects.GuardianOrganId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.68f, 0.82f, 1f), false));
                    break;
                case PlayerItemCombatEffects.TentacleColonyId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.68f, 0.24f, 0.86f), true));
                    break;
                case PlayerItemCombatEffects.ElectricNeuralNetworkId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.45f, 0.82f, 1f), true));
                    break;
                case PlayerItemCombatEffects.InfectionTransferenceId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.42f, 0.95f, 0.32f), false));
                    break;
                case PlayerItemCombatEffects.MacrophageId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.34f, 0.18f), false));
                    break;
                case PlayerItemCombatEffects.GluttonousOrganId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.96f, 0.52f, 0.22f), true));
                    break;
                case PlayerItemCombatEffects.HeartSniperId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.12f, 0.18f), true));
                    break;
                case PlayerItemCombatEffects.BloodflowAccelerationId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.95f, 0.05f, 0.08f), true));
                    break;
                case PlayerItemCombatEffects.FocusedNerveId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.78f, 0.9f, 1f), false));
                    break;
                case PlayerItemCombatEffects.ExecutionInstinctId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.96f, 0.86f, 0.2f), true));
                    break;
                case PlayerItemCombatEffects.BerserkCellId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.24f, 0.08f), false));
                    break;
                case PlayerItemCombatEffects.UnstableCellId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.75f, 0.38f, 1f), true));
                    break;
                case PlayerItemCombatEffects.GrotesqueGrowthId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.42f, 1f, 0.58f), false));
                    break;
                case PlayerItemCombatEffects.MutationRampageId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(1f, 0.42f, 0.95f), true));
                    break;
                case PlayerItemCombatEffects.ParasiticBombId:
                    entry.AssignIconIfMissing(GetFallbackIcon(entry.ItemId, new Color(0.88f, 0.12f, 0.68f), false));
                    break;
                case PlayerItemCombatEffects.FrenzyHormoneId:
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

        private static void NormalizeDescription(PlayerItemEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            if (string.Equals(entry.ItemId, PlayerItemCombatEffects.DoubleCoreId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("발사체 2개 발사 (각 75% 피해)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.TripleCoreId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("발사체 3개 발사 (각 55% 피해)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.HypertrophyCellId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("투사체 크기·충돌 범위 45%, 사거리 25% 증가");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.RefluxOrganId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("왕복 경로마다 최대 3명 타격, 복귀 중 같은 적 재적중 시 75% 피해");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.PiercingMucusId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("최대 3명 관통, 관통할 때마다 피해 25% 감소");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.BeamOrganId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("80% 피해의 관통 광선 공격 (다중핵·후방 발사·세포 증식 적용)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.SplitTissueId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("적 명중 시 양옆으로 각각 50% 피해의 추가 투사체 생성");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.ExplosiveBloodCellId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("원거리 피해가 50% 감소하고 범위 폭발 공격으로 변경");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.AcidicRuptureId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("적중 위치에 4초간 1초마다 0.5 피해를 주는 산성 장판 생성");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.OverheatedOrganId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("연속 공격마다 공격속도 +10% (10중첩 시 주변 150% 피해, 자신 1 피해)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.MutantEyeId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("발사 방향이 좌우 40도 내에서 무작위 변화, 공격력 +2");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.MuscleSpasmId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("근접 공격 범위 1.7배, 공격속도 20% 감소");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.ToxicMucosaId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("적중 시 4초간 1초마다 적중 피해의 20% 중독 피해");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.HemorrhageOrganId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("적중 시 3.5초간 0.8초마다 적중 피해의 18% 출혈 피해");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.OrganTentacleId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("주변 적 최대 2명을 1.2초마다 30% 피해로 공격, 기본 공격 피해 20% 감소");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.PulseBulletId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("투사체가 이동하며 크기와 실제 충돌 범위가 증가하거나 감소");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.FreezingNerveId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("공격 적중 시 2초간 적 이동속도 30% 감소");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.RampageBloodFlowId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("이동 8초마다 공격력 +1 (최대 +2), 정지 시 4초마다 감소");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.UnstableCoreId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("5초마다 기본 공격 피해가 60~160% 사이에서 변경");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.VoidCellId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("공격 시 20% 확률로 적 주변에서 85% 피해의 추가 탄환 생성");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.BioResonanceId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("3초 내 같은 적 연속 공격 시 피해 +15% (최대 +45%)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.BloodPressureBurstId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("잃은 체력 10%당 공격속도 +6% (최대 +60%)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.ForbiddenGrowthId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("체력 -3, 공격력 +2 증가");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.OverclockNerveId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("체력 -2, 이동속도 +2 증가");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.BloodContractId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("적 20명 처치 시 체력 +1 (최대 +5), 받는 피해 1.3배");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.ImperfectRegenerationId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("체력 -2, 피격 2초 후 체력 +1 회복 (10초 쿨타임)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.SeveranceReflexId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("피격 후 2초간 공격력 +2");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.HyperplasiaHeartId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("잃은 체력에 비례해 공격력 증가 (최대 +2)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.DecayOrganId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("3분마다 공격력 +1 (최대 +4)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.RuptureMuscleId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("공격 2회마다 공격력 +0.5·이동속도 -0.25 (최대 3중첩)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.BioGambleId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("레벨업 선택 시 성장량이 0~+2 범위에서 무작위 결정");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.ExoskeletonId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("받는 피해 25% 감소, 대신 이동속도 -1.5");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.PlateletMembraneId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("20초마다 피해 1을 막는 보호막 생성");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.RecoveryFactorId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("30초마다 체력 +1 회복");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.ReflectiveSkinId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("피격 시 실제 받은 피해의 75% 반사");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.BioBarrierId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("정지 1.5초마다 받는 피해 10% 감소 (최대 40%)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.InfectedHostId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("잡몹 처치 시 15% 확률로 6초간 35% 피해의 아군 생성 (최대 2명)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.SporeColonyId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("일정 시간마다 적에게 달려드는 작은 포자 생명체 소환");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.BloodDroneId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("1초마다 가까운 적에게 공격력의 30% 피해");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.GuardianOrganId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("플레이어 주변을 맴돌며 3.5초마다 적 투사체 하나 방어");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.TentacleColonyId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("3초마다 적 2명에게 1.2초간 60% 둔화와 10% 피해");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.ElectricNeuralNetworkId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("적 처치 시 최대 3명에게 피해 1의 연쇄 전류 (5초 쿨타임)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.InfectionTransferenceId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("적 처치 시 주변 3명에게 3초간 초당 0.5 감염 피해 (5초 쿨타임)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.MacrophageId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("적 처치 시 5초간 공격력 +0.5 (최대 3중첩)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.GluttonousOrganId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("적 처치 시 3초 동안 이동속도 증가 (최대 4스택)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.HeartSniperId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("체력 60% 이상인 적에게 직접 공격 피해 35% 증가");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.BloodflowAccelerationId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("보스·엘리트 근처에서 공격속도 35% 증가");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.FocusedNerveId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("주변 적 1명 이하일 때 공격력 +1.5, 2~3명일 때 +0.5");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.ExecutionInstinctId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("체력 20% 미만 적을 20% 확률로 처형 (보스·엘리트는 추가 피해 50%)");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.BerserkCellId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("보스전 진입 시 15초간 공격력 +1.5, 이동속도 +0.5, 공격속도 +50%");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.UnstableCellId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("투사체 속도 50~150%, 80% 이하의 느린 탄은 피해 150%");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.GrotesqueGrowthId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("10초마다 소형화·이동속도 +1 또는 대형화·공격력 +1·이동속도 -0.5");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.MutationRampageId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("15초마다 7초간 무작위 스탯 버프 또는 디버프");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.ParasiticBombId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("적 처치 시 20% 확률로 반경 4에 공격력 150% 폭발");
            }
            else if (string.Equals(entry.ItemId, PlayerItemCombatEffects.FrenzyHormoneId, StringComparison.OrdinalIgnoreCase))
            {
                entry.SetDescription("피격 시 무작위 능력 +0.5~1 (3초 지속, 8초 쿨타임)");
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

        private void EnsureCombatEffectsForInventoryChange()
        {
            PlayerItemCombatEffects combatEffects = EnsureComponent<PlayerItemCombatEffects>();
            combatEffects.EnsureActiveForInventoryChange();
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
                    acquiredItem.Implementation.RemoveFrom(playerStats);
                }
            }

            acquiredItems.Clear();

            for (int i = 0; i < removedItems.Count; i++)
            {
                ItemRemoved?.Invoke(this, removedItems[i]);
            }
        }

        public List<SavedItemStateData> CaptureSavedItems()
        {
            List<SavedItemStateData> result = new List<SavedItemStateData>(acquiredItems.Count);
            PlayerItemCombatEffects combatEffects = GetComponent<PlayerItemCombatEffects>();
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

                GetComponent<PlayerItemCombatEffects>()?.RestorePersistentItemStates(savedItems);
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
