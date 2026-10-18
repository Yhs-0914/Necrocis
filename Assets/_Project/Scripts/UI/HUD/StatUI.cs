using UnityEngine;
using UnityEngine.UI;

namespace Necrocis
{
    /// <summary>
    /// O키로 열고 닫는 스탯 확인 UI.
    /// 현재 스탯과 기본값 대비 증감을 표시.
    /// </summary>
    public class StatUI : MonoBehaviour
    {
        [Header("UI 설정")]
        [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.85f);  // 패널 배경색
        [SerializeField] private Color positiveColor = new Color(0.4f, 1f, 0.4f);  // 증가 표시 색상 (초록)
        [SerializeField] private Color negativeColor = new Color(1f, 0.4f, 0.4f);  // 감소 표시 색상 (빨강)

        private GameObject uiRoot;     // UI Canvas 루트
        private Text contentText;      // 모든 스탯 정보를 표시하는 단일 Text
        private ModalInputScope inputScope;
        private bool isShowing;        // 현재 표시 중 여부
        private CharacterStats subscribedStats;
        private IStatsDisplay statsDisplay;
        private readonly System.Text.StringBuilder statTextBuilder = new System.Text.StringBuilder(384);
        private readonly float[] readCurrentValues = new float[displayStats.Length];
        private readonly float[] readBaseValues = new float[displayStats.Length];
        private readonly float[] shownCurrentValues = new float[displayStats.Length];
        private readonly float[] shownBaseValues = new float[displayStats.Length];
        private bool hasDisplaySnapshot, shownHasBasic;
        private int shownLevel;
        private JobType shownJob;
        private float shownHealth;
        private BasicAttackDisplaySnapshot shownBasic;
        private System.Globalization.CultureInfo shownCulture;
        private string shownText;
        private string positiveColorHex;
        private string negativeColorHex;

        // 표시할 스탯 순서 정의
        private static readonly CharacterStatType[] displayStats = new CharacterStatType[]
        {
            CharacterStatType.MaxHealth,
            CharacterStatType.AttackPower,
            CharacterStatType.MoveSpeed,
            CharacterStatType.AttackSpeed,
            CharacterStatType.AttackRange,
            CharacterStatType.Magic,
            CharacterStatType.SkillCooldownReduction
        };

        private void Start()
        {
            BuildUI();
            uiRoot.SetActive(false);
            TryBindStats();
        }

        private void OnEnable()
        {
            PlayerDisplayBindings.Progression.LevelChanged += HandleProgressionChanged;
            PlayerDisplayBindings.Progression.JobSelection += HandleProgressionChanged;
            PlayerDisplayBindings.Progression.JobChanged += HandleJobChanged;
            TryBindStats();
        }

        private void OnDisable()
        {
            PlayerDisplayBindings.Progression.LevelChanged -= HandleProgressionChanged;
            PlayerDisplayBindings.Progression.JobSelection -= HandleProgressionChanged;
            PlayerDisplayBindings.Progression.JobChanged -= HandleJobChanged;
            UnbindStats();
            Hide();
        }

        // O키로 창을 토글한다. 수치는 구독한 스탯/진행 이벤트에서 갱신한다.
        private void Update()
        {
            if (subscribedStats == null)
            {
                TryBindStats();
            }

            if(isShowing)RefreshStats();
            var input = InputManager.Instance;
            if (input == null) return;

            if (input.StatWindowAction.WasPressedThisFrame()
                && (isShowing ? inputScope?.CanInteract == true : input.Contexts.CanOpenAuxiliary))
            {
                AudioManager.Instance?.PlaySFX("StatWindow");
                if (isShowing)
                    Hide();
                else
                    Show();
            }
        }

        private void Show()
        {
            TryBindStats();
            RefreshStats();
            uiRoot.SetActive(true);
            isShowing = true;
            inputScope ??= new ModalInputScope(this, uiRoot, false, blocksGameplay: false, consumeEscape: false);
        }

        private void Hide()
        {
            if (uiRoot != null) uiRoot.SetActive(false);
            isShowing = false;
            inputScope?.Dispose(); inputScope = null;
        }

        // 모든 스탯 정보를 StringBuilder로 조합하여 Text에 표시
        // 기본값 대비 증감분을 색상으로 표시 (초록: 증가, 빨강: 감소)
        private void RefreshStats()
        {
            CharacterStats stats = subscribedStats != null
                ? subscribedStats
                : statsDisplay?.RuntimeStats;
            if (stats == null) return;
            // Read all live values at the original update rate, including item bonuses
            // that can change with time or nearby enemies without a stat event.
            int level = PlayerDisplayBindings.Progression.Level;
            JobType job = PlayerDisplayBindings.Progression.Job;
            float hp = stats.CurrentHealth;
            float maxHp = stats.MaxHealth;
            float hpBase = stats.GetBaseValue(CharacterStatType.MaxHealth);
            readCurrentValues[0] = maxHp;
            readBaseValues[0] = hpBase;
            for (int i = 1; i < displayStats.Length; i++)
            {
                readCurrentValues[i] = stats.GetValue(displayStats[i]);
                readBaseValues[i] = stats.GetBaseValue(displayStats[i]);
            }
            var attacks = GetComponent<IBasicAttackDisplayReadModel>();
            bool hasBasic = attacks != null;
            var view = hasBasic ? attacks.ReadBasicAttackDisplay() : default;
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            // Mutable cultures can change formatting in place without changing their identity.
            // Preserve that behavior; normal read-only cultures can reuse the formatted text.
            bool changed = !culture.IsReadOnly || !culture.NumberFormat.IsReadOnly
                || !hasDisplaySnapshot || shownLevel != level || shownJob != job
                || !shownHealth.Equals(hp) || shownHasBasic != hasBasic
                || !object.ReferenceEquals(shownCulture, culture)
                || (hasBasic && !SameBasicDisplay(shownBasic, view));
            for (int i = 0; i < displayStats.Length; i++)
                changed |= !shownCurrentValues[i].Equals(readCurrentValues[i])
                    || !shownBaseValues[i].Equals(readBaseValues[i]);
            if (!changed && string.Equals(contentText.text, shownText, System.StringComparison.Ordinal)) return;


            positiveColorHex ??= ColorUtility.ToHtmlStringRGB(positiveColor);
            negativeColorHex ??= ColorUtility.ToHtmlStringRGB(negativeColor);
            System.Text.StringBuilder sb = statTextBuilder;
            sb.Clear();

            // 레벨
            sb.AppendLine($"<color=#FFD933><b>스탯 정보</b></color>");
            sb.AppendLine($"Lv.{level}");

            // 직업
            string jobName = job == JobType.None ? "없음" : GetJobName(job);
            sb.AppendLine($"<color=#99CCFF>직업: {jobName}</color>");
            sb.AppendLine();

            // HP (현재/최대 형태)
            float hpDiff = maxHp - hpBase;
            sb.Append($"체력: {hp:F0}/{maxHp:F0}");
            if (Mathf.Abs(hpDiff) > 0.01f)
                sb.Append(hpDiff > 0 ? $" <color=#{positiveColorHex}>(+{hpDiff:F1})</color>" : $" <color=#{negativeColorHex}>({hpDiff:F1})</color>");
            sb.AppendLine();

            // 나머지 스탯
            for (int i = 1; i < displayStats.Length; i++)
            {
                CharacterStatType type = displayStats[i];
                float current = readCurrentValues[i];
                float baseVal = readBaseValues[i];
                float diff = current - baseVal;

                sb.Append($"{GetStatName(type)}: {current:F1}");
                if (Mathf.Abs(diff) > 0.01f)
                {
                    if (diff > 0)
                        sb.Append($" <color=#{positiveColorHex}>(+{diff:F1})</color>");
                    else
                        sb.Append($" <color=#{negativeColorHex}>({diff:F1})</color>");
                }
                sb.AppendLine();
            }

            if(hasBasic)
            {
                sb.AppendLine();sb.AppendLine("<b>기본공격 전용</b>");
                sb.AppendLine($"Q 피해: {view.MeleeDamage:0.##} / 간격: {view.MeleeInterval:0.##}초");
                sb.AppendLine($"W 기준 피해: {view.RangedDamage:0.##} / 간격: {view.RangedInterval:0.##}초");
                sb.AppendLine("탄별·관통·폭발 보정 별도");
                if(view.Items.HasRandomMultiplier)sb.AppendLine(view.Items.RandomMultiplierInitialized?"현재 확정된 무작위 배율 기준":"무작위 배율 미확정 · 기본 배율 기준");
                if(view.Items.HasTargetConditionalBonus)sb.AppendLine("대상별 추가 보정은 적중 시 적용");
            }
            string rendered = sb.ToString();
            // Commit before assigning Text, whose dirty callbacks can synchronously reenter.
            shownLevel = level; shownJob = job; shownHealth = hp; shownHasBasic = hasBasic;
            shownBasic = view; shownCulture = culture; shownText = rendered; hasDisplaySnapshot = true;
            for (int i = 0; i < displayStats.Length; i++)
            {
                shownCurrentValues[i] = readCurrentValues[i];
                shownBaseValues[i] = readBaseValues[i];
            }
            contentText.text = rendered;
        }

        private static bool SameBasicDisplay(BasicAttackDisplaySnapshot a, BasicAttackDisplaySnapshot b)
        {
            return a.MeleeDamage.Equals(b.MeleeDamage) && a.RangedDamage.Equals(b.RangedDamage)
                && a.MeleeInterval.Equals(b.MeleeInterval) && a.RangedInterval.Equals(b.RangedInterval)
                && a.Items.FlatDamageBonus.Equals(b.Items.FlatDamageBonus)
                && a.Items.DamageMultiplier.Equals(b.Items.DamageMultiplier)
                && a.Items.RangedDamageMultiplier.Equals(b.Items.RangedDamageMultiplier)
                && a.Items.CooldownMultiplier.Equals(b.Items.CooldownMultiplier)
                && a.Items.HasRandomMultiplier == b.Items.HasRandomMultiplier
                && a.Items.RandomMultiplierInitialized == b.Items.RandomMultiplierInitialized
                && a.Items.CurrentRandomMultiplier.Equals(b.Items.CurrentRandomMultiplier)
                && a.Items.HasTargetConditionalBonus == b.Items.HasTargetConditionalBonus;
        }

        private void TryBindStats()
        {
            if(statsDisplay?.Owner==null)statsDisplay=PlayerDisplayBindings.Stats(gameObject);
            CharacterStats currentStats = statsDisplay?.RuntimeStats;
            if (currentStats == subscribedStats)
            {
                return;
            }

            UnbindStats();
            subscribedStats = currentStats;
            if (subscribedStats == null)
            {
                return;
            }

            subscribedStats.StatChanged += HandleStatChanged;
            subscribedStats.HealthChanged += HandleHealthChanged;
        }

        private void UnbindStats()
        {
            if (subscribedStats == null)
            {
                return;
            }

            subscribedStats.StatChanged -= HandleStatChanged;
            subscribedStats.HealthChanged -= HandleHealthChanged;
            subscribedStats = null;
        }

        private void HandleStatChanged(CharacterStats _, CharacterStatChangedEventArgs __)
        {
            RefreshIfVisible();
        }

        private void HandleHealthChanged(CharacterStats _, CharacterHealthChangedEventArgs __)
        {
            RefreshIfVisible();
        }

        private void HandleProgressionChanged()
        {
            RefreshIfVisible();
        }

        private void HandleJobChanged(JobType _)
        {
            RefreshIfVisible();
        }

        private void RefreshIfVisible()
        {
            if (isShowing)
            {
                RefreshStats();
            }
        }

        // 스탯 타입 → 한글 이름 변환
        private static string GetStatName(CharacterStatType type)
        {
            return type switch
            {
                CharacterStatType.MaxHealth => "체력",
                CharacterStatType.AttackPower => "공격력",
                CharacterStatType.MoveSpeed => "이동속도",
                CharacterStatType.AttackSpeed => "공격속도",
                CharacterStatType.AttackRange => "공격 사거리",
                CharacterStatType.Magic => "마력",
                CharacterStatType.SkillCooldownReduction => "스킬 쿨타임 감소",
                _ => type.ToString()
            };
        }

        // 직업 타입 → 한글 이름 변환
        private static string GetJobName(JobType job)
        {
            return job switch
            {
                JobType.Warrior => "전사",
                JobType.Mage => "마법사",
                JobType.Archer => "궁수",
                _ => job.ToString()
            };
        }

        // ─────────────────────────────────
        // UI 자동 생성
        // ─────────────────────────────────

        private void BuildUI()
        {
            hasDisplaySnapshot = false;
            // Canvas
            GameObject canvasObj = new GameObject("StatUICanvas");
            canvasObj.transform.SetParent(transform);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            uiRoot = canvasObj;

            // 패널 (왼쪽 상단 고정, 내용에 맞게 크기 조절)
            GameObject panel = new GameObject("StatPanel", typeof(RectTransform));
            panel.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(10, -10);

            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = panelColor;

            ContentSizeFitter fitter = panel.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 15, 15);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            // 텍스트 1개로 모든 정보 표시
            GameObject textObj = new GameObject("Content", typeof(RectTransform));
            textObj.transform.SetParent(panel.transform, false);

            contentText = textObj.AddComponent<Text>();
            contentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            contentText.fontSize = 22;
            contentText.color = Color.white;
            contentText.alignment = TextAnchor.UpperLeft;
            contentText.supportRichText = true;
            contentText.horizontalOverflow = HorizontalWrapMode.Overflow;
            contentText.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }
}
